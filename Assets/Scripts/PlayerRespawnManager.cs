using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player respawn at the designated PlayerStartPoint after death.
/// Attach to any persistent GameObject (e.g. GameSystems).
/// Requires an IPlayerProvider in the scene to receive death events.
/// </summary>
public class PlayerRespawnManager : MonoBehaviour
{
    private const string LogPrefix = "[PlayerRespawnManager]";

    [Header("Respawn")]
    [Tooltip("The transform the player is teleported to on respawn. " +
             "If left empty, the component will search for a GameObject named 'PlayerStartPoint'.")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Seconds to wait after death before respawning.")]
    [SerializeField] private float respawnDelay = 3f;

    [Tooltip("Health percentage restored on respawn (0–1).")]
    [SerializeField, Range(0.01f, 1f)] private float respawnHealthFraction = 1f;

    [Tooltip("Optional provider assignment. If left empty, the manager auto-discovers one.")]
    [SerializeField] private MonoBehaviour playerProviderObject;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private IPlayerProvider _provider;
    private GameObject _playerObject;
    private bool _isRespawning;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        ResolveRespawnPoint();
        ResolveProvider();
    }

    private void OnDestroy()
    {
        if (_provider != null)
            _provider.OnDeath -= OnPlayerDeath;
    }

    // ── Initialization ────────────────────────────────────────────────────────

    private void ResolveRespawnPoint()
    {
        if (respawnPoint != null) return;

        var found = GameObject.Find("PlayerStartPoint");
        if (found != null)
        {
            respawnPoint = found.transform;
            if (showDebugLogs)
                Debug.Log($"{LogPrefix} Auto-found respawn point: '{found.name}'.");
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} No respawnPoint assigned and 'PlayerStartPoint' not found in scene.");
        }
    }

    private void ResolveProvider()
    {
        _provider = playerProviderObject as IPlayerProvider;
        if (_provider == null)
            _provider = FindAnyPlayerProvider();

        if (_provider == null)
        {
            Debug.LogWarning($"{LogPrefix} IPlayerProvider not found. Respawn disabled.");
            return;
        }

        _provider.OnDeath += OnPlayerDeath;
        _playerObject = _provider.PlayerObject;

        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Bound to IPlayerProvider.");
    }

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < allMonoBehaviours.Length; i++)
        {
            if (allMonoBehaviours[i] is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    // ── Death / Respawn ───────────────────────────────────────────────────────

    private void OnPlayerDeath()
    {
        if (_isRespawning) return;
        if (respawnPoint == null)
        {
            Debug.LogWarning($"{LogPrefix} Cannot respawn — no respawn point set.");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Player died. Respawning in {respawnDelay}s at '{respawnPoint.name}'.");

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _isRespawning = true;

        yield return new WaitForSeconds(respawnDelay);

        if (_provider == null)
        {
            _isRespawning = false;
            yield break;
        }

        if (_playerObject == null)
            _playerObject = _provider.PlayerObject;

        if (_playerObject == null)
        {
            Debug.LogWarning($"{LogPrefix} Cannot respawn — provider has no player object.");
            _isRespawning = false;
            yield break;
        }

        PerformRespawn();
        _isRespawning = false;
    }

    private void PerformRespawn()
    {
        // Reset physics velocity before warp to avoid post-respawn carry-over momentum.
        var rb = _playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Avoid CharacterController snapping against colliders while teleporting.
        CharacterController characterController = _playerObject.GetComponent<CharacterController>();
        bool hadCharacterController = characterController != null;
        if (hadCharacterController)
            characterController.enabled = false;

        _playerObject.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

        if (hadCharacterController)
            characterController.enabled = true;

        float targetHealth = _provider.MaxHealth * respawnHealthFraction;
        _provider.SetHealth(targetHealth);

        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Player respawned at '{respawnPoint.name}' with {targetHealth:F0} HP.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Override the respawn location at runtime (e.g. for checkpoints).
    /// </summary>
    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Respawn point changed to '{point?.name}'.");
    }

    /// <summary>
    /// Trigger an immediate respawn regardless of death state.
    /// Useful for debug or checkpoint teleport scenarios.
    /// </summary>
    public void ForceRespawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning($"{LogPrefix} Cannot force respawn — no respawn point set.");
            return;
        }

        StopAllCoroutines();
        _isRespawning = false;

        if (_provider == null)
            ResolveProvider();

        if (_provider == null)
        {
            Debug.LogWarning($"{LogPrefix} Cannot force respawn — no player provider found.");
            return;
        }

        if (_playerObject == null)
            _playerObject = _provider.PlayerObject;

        if (_playerObject != null)
            PerformRespawn();
    }
}
