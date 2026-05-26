using System.Collections;
using UnityEngine;

/// <summary>
/// Dropped health station (Division-style). Reinstates all player stats over a fixed duration.
/// Acts as a placeholder cube until a proper model is swapped in.
/// Restores stats while the player stands within <see cref="activationRadius"/>.
/// Destroys itself after <see cref="lifetimeDuration"/> seconds.
/// </summary>
public class HealthStation : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Range")]
    [Tooltip("Radius within which the station restores the player.")]
    [SerializeField] private float activationRadius = 3f;

    [Header("Restoration Rates (per second)")]
    [Tooltip("Health points restored per second.")]
    [SerializeField] private float healthPerSecond = 20f;
    [Tooltip("Armour points restored per second.")]
    [SerializeField] private float armourPerSecond = 15f;
    [Tooltip("Hunger points restored per second.")]
    [SerializeField] private float hungerPerSecond = 10f;
    [Tooltip("Thirst points restored per second.")]
    [SerializeField] private float thirstPerSecond = 10f;
    [Tooltip("Temperature points restored per second.")]
    [SerializeField] private float temperaturePerSecond = 5f;

    [Header("Duration")]
    [Tooltip("Total seconds the station remains active before it disappears.")]
    [SerializeField] private float lifetimeDuration = 15f;

    [Header("Visual Feedback")]
    [Tooltip("Material applied to the cube while the station is active.")]
    [SerializeField] private Material activeMaterial;
    [Tooltip("Material applied once the station is depleted.")]
    [SerializeField] private Material depletedMaterial;

    // ── Private state ─────────────────────────────────────────────────────────

    private bool _isHealing;
    private float _remainingLifetime;
    private Transform _playerTransform;
    private IPlayerProvider _playerProvider;
    private Renderer _renderer;

    private const float DistanceCheckInterval = 0.2f;
    private float _distanceCheckTimer;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        _remainingLifetime = lifetimeDuration;
        _renderer = GetComponent<Renderer>();

        if (activeMaterial != null && _renderer != null)
            _renderer.material = activeMaterial;

        CachePlayerReferences();
        StartCoroutine(LifetimeRoutine());
    }

    private void Update()
    {
        if (_remainingLifetime <= 0f) return;

        _distanceCheckTimer += Time.deltaTime;
        if (_distanceCheckTimer >= DistanceCheckInterval)
        {
            _distanceCheckTimer = 0f;
            CheckProximity();
        }

        if (_isHealing)
            ApplyRestoration(Time.deltaTime);
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void CachePlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("[HealthStation] No GameObject tagged 'Player' found.");
            return;
        }

        _playerTransform = playerObj.transform;

        // Resolve IPlayerProvider — check player root first, then scene-wide.
        foreach (var mb in playerObj.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb is IPlayerProvider provider)
            {
                _playerProvider = provider;
                break;
            }
        }

        if (_playerProvider == null)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is IPlayerProvider provider)
                {
                    _playerProvider = provider;
                    break;
                }
            }
        }
    }

    // ── Core logic ────────────────────────────────────────────────────────────

    private void CheckProximity()
    {
        if (_playerTransform == null) return;
        _isHealing = Vector3.Distance(transform.position, _playerTransform.position) <= activationRadius;
    }

    /// <summary>Applies one restoration tick across all available stat systems.</summary>
    private void ApplyRestoration(float deltaTime)
    {
        // Health via IPlayerProvider
        if (_playerProvider != null && _playerProvider.IsAlive)
        {
            float newHealth = Mathf.Min(_playerProvider.Health + healthPerSecond * deltaTime, _playerProvider.MaxHealth);
            _playerProvider.SetHealth(newHealth);
        }

        // Armour, hunger, thirst, temperature via SurvivalManager
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null)
        {
            survival.ModifyArmour(armourPerSecond * deltaTime);
            survival.AddHunger(hungerPerSecond * deltaTime);
            survival.AddThirst(thirstPerSecond * deltaTime);
            survival.SetTemperature(
                Mathf.Min(survival.currentTemperature + temperaturePerSecond * deltaTime, survival.maxTemperature));
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        while (_remainingLifetime > 0f)
        {
            _remainingLifetime -= Time.deltaTime;
            yield return null;
        }

        Deplete();
    }

    private void Deplete()
    {
        _isHealing = false;

        if (depletedMaterial != null && _renderer != null)
            _renderer.material = depletedMaterial;

        Destroy(gameObject, 1.5f);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
