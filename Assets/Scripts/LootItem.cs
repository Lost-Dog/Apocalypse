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

    private const string PlayerTag       = "Player";
    private const float  RotationSpeed   = 90f; // degrees per second

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _collected    = false;
        _spawnTime    = Time.time;
        _basePosition = transform.position;

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
