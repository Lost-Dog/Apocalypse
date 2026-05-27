using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Placed on every world loot drop. Automatically collects the item when the player
/// walks into the trigger collider. Supports an optional pickup delay to prevent
/// immediately vacuuming up a freshly thrown drop.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LootItem : MonoBehaviour
{
    [Header("Item Data")]
    public LootItemData itemData;
    public int          gearScore;
    public LootRarity   rarity;

    [Header("Auto-Pickup")]
    [Tooltip("When true the item is collected automatically on trigger enter with the player.")]
    public bool autoPickupOnCollision = true;

    [Tooltip("Seconds after spawn before auto-pickup is active. Prevents collecting a drop the " +
             "moment it leaves the enemy.")]
    public float pickupDelay = 0.5f;

    [Header("Cleanup")]
    [Tooltip("Seconds before an uncollected drop is removed. 0 = never expire.")]
    public float expireAfterSeconds = 300f;   // 5 minutes

    [Tooltip("Distance from the player beyond which an uncollected drop is removed. 0 = disabled.")]
    public float despawnBeyondRange = 40f;

    [Header("Visuals")]
    public VisualEffect visualEffect;
    public Light        rarityLight;

    [Header("Bob & Rotate")]
    public float bobHeight    = 0f;
    public float bobSpeed     = 0f;
    public bool  enableRotation = true;

    // ── Private state ─────────────────────────────────────────────────────────

    private float     _spawnTime;
    private bool      _collected;
    private Vector3   _basePosition;
    private Transform _playerTransform;

    private const string PlayerTag       = "Player";
    private const float  RotationSpeed   = 90f; // degrees per second
    private const float  CleanupCheckInterval = 2f; // seconds between range/expire checks
    private float        _nextCleanupCheck;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _collected    = false;
        _spawnTime    = Time.time;
        _basePosition = transform.position;
        _nextCleanupCheck = Time.time + CleanupCheckInterval;

        CachePlayer();
        ApplyRarityLight();
    }

    private void Update()
    {
        if (_collected) return;

        if (enableRotation)
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.World);

        if (bobHeight > 0f && bobSpeed > 0f)
        {
            float newY = _basePosition.y + Mathf.Sin((Time.time - _spawnTime) * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Throttle range/expiry checks — no need to run every frame.
        if (Time.time < _nextCleanupCheck) return;
        _nextCleanupCheck = Time.time + CleanupCheckInterval;

        if (ShouldExpire())
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickupOnCollision || _collected) return;
        if (Time.time - _spawnTime < pickupDelay) return;
        if (!other.CompareTag(PlayerTag)) return;

        Collect(other.gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Collects the item into the player's inventory immediately, bypassing the trigger.
    /// Useful for forced pickup from external systems.
    /// </summary>
    public void Collect(GameObject player)
    {
        if (_collected) return;
        _collected = true;

        if (LootManager.Instance != null && itemData != null)
            LootManager.Instance.AddItemToPlayerInventory(itemData, gearScore, rarity, player);

        ReturnToPool();
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private void CachePlayer()
    {
        GameObject playerGo = GameObject.FindWithTag(PlayerTag);
        _playerTransform = playerGo != null ? playerGo.transform : null;
    }

    private bool ShouldExpire()
    {
        if (expireAfterSeconds > 0f && Time.time - _spawnTime >= expireAfterSeconds)
            return true;

        if (despawnBeyondRange > 0f && _playerTransform != null)
        {
            float sqrRange = despawnBeyondRange * despawnBeyondRange;
            if ((_playerTransform.position - transform.position).sqrMagnitude > sqrRange)
                return true;
        }

        return false;
    }

    private void ApplyRarityLight()
    {
        if (rarityLight == null || LootManager.Instance == null) return;
        rarityLight.color = LootManager.Instance.GetRarityColor(rarity);
    }

    /// <summary>
    /// Returns the object to LootManager's pool if pooling is active, otherwise destroys it.
    /// </summary>
    private void ReturnToPool()
    {
        if (LootManager.Instance != null && LootManager.Instance.TryReturnToPool(gameObject))
            return;

        Destroy(gameObject);
    }
}
