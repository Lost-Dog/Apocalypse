using Invector.vItemManager;
using UnityEngine;

/// <summary>
/// Handles dropping a <see cref="HealthStation"/> when the player presses LB+RB.
/// Consumes one health consumable from the Invector inventory on each deploy.
/// Attach this to the same GameObject as the Invector character / <see cref="InvectorInputBridge"/>.
/// </summary>
public class HealthStationDeployer : MonoBehaviour
{
    public static HealthStationDeployer Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Station Prefab")]
    [Tooltip("The health station prefab to spawn. If none is assigned a primitive cube is created at runtime.")]
    [SerializeField] private GameObject healthStationPrefab;

    [Header("Spawn Position")]
    [Tooltip("Offset relative to the player where the station drops (world Y is always grounded).")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, 1.5f);

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between consecutive deploys.")]
    [SerializeField] private float deployCooldown = 3f;

    [Header("Debug")]
    [SerializeField] private bool logEvents = false;

    // ── Private state ─────────────────────────────────────────────────────────

    private vItemManager _itemManager;
    private float _cooldownTimer;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Instance = this;
    }

    private void Start()
    {
        _itemManager = GetComponentInChildren<vItemManager>(includeInactive: true)
                    ?? GetComponentInParent<vItemManager>();

        if (_itemManager == null)
            Debug.LogWarning("[HealthStationDeployer] vItemManager not found. Inventory check for consumables will be skipped.");

        EnsurePrefab();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    // ── Public API called by InvectorInputBridge ──────────────────────────────

    /// <summary>
    /// Attempts to deploy a health station. Called by <see cref="InvectorInputBridge"/> when
    /// LB+RB are pressed simultaneously.
    /// Returns true if a station was successfully deployed.
    /// </summary>
    public bool TryDeploy()
    {
        if (_cooldownTimer > 0f)
        {
            if (logEvents) Debug.Log($"[HealthStationDeployer] Deploy blocked — cooldown {_cooldownTimer:F1}s remaining.");
            return false;
        }

        vItem consumable = FindHealthConsumable();
        if (consumable == null)
        {
            if (logEvents) Debug.Log("[HealthStationDeployer] No health consumables in inventory.");
            return false;
        }

        ConsumeOne(consumable);
        SpawnStation();

        _cooldownTimer = deployCooldown;

        if (logEvents) Debug.Log($"[HealthStationDeployer] Health station deployed. Consumed '{consumable.name}' x1.");
        return true;
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Finds the first health consumable available in the inventory.
    /// A health consumable is identified by being of type <c>vItemType.Consumable</c>
    /// and having a remaining amount greater than zero.
    /// </summary>
    private vItem FindHealthConsumable()
    {
        if (_itemManager == null || _itemManager.inventory == null) return null;

        foreach (vItem item in _itemManager.inventory.items)
        {
            if (item == null) continue;
            if (item.type == vItemType.Consumable && item.amount > 0)
                return item;
        }

        return null;
    }

    /// <summary>Destroys one unit of the given consumable from the inventory.</summary>
    private void ConsumeOne(vItem item)
    {
        _itemManager.DestroyItem(item, 1);
    }

    /// <summary>Instantiates the health station prefab in front of the player.</summary>
    private void SpawnStation()
    {
        Vector3 forward  = transform.forward;
        Vector3 position = transform.position + forward * spawnOffset.z
                         + transform.right   * spawnOffset.x
                         + Vector3.up        * spawnOffset.y;

        Quaternion rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Instantiate(healthStationPrefab, position, rotation);
    }

    /// <summary>
    /// If no prefab is assigned, builds a primitive cube with a <see cref="HealthStation"/>
    /// component baked in and saves it as the runtime prefab reference.
    /// </summary>
    private void EnsurePrefab()
    {
        if (healthStationPrefab != null) return;

        // Build a temporary GameObject, add the component, then convert to a prefab substitute
        // by keeping a reference — it won't be a real asset prefab, but works for runtime drops.
        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholder.name = "HealthStation_Placeholder";
        placeholder.transform.localScale = Vector3.one * 0.5f;

        // Remove default box collider to avoid physics interference; re-add as trigger.
        DestroyImmediate(placeholder.GetComponent<Collider>());
        var trigger = placeholder.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        placeholder.AddComponent<HealthStation>();
        placeholder.SetActive(false);

        healthStationPrefab = placeholder;

        // Move out of scene root to prevent it being visible.
        placeholder.transform.SetParent(transform);
        placeholder.transform.localPosition = Vector3.down * 1000f;

        if (logEvents)
            Debug.Log("[HealthStationDeployer] No prefab assigned — built placeholder cube at runtime.");
    }
}
