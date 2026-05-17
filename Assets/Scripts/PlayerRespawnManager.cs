using System.Collections;
using Invector;
using Invector.vCharacterController;
using UnityEngine;

/// <summary>
/// Handles player respawn at the designated PlayerStartPoint after death.
/// Attach to any persistent GameObject (e.g. GameSystems).
/// Requires an InvectorPlayerProvider in the scene to receive death events.
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

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private InvectorPlayerProvider _provider;
    private vThirdPersonController _controller;
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
        _provider = FindFirstObjectByType<InvectorPlayerProvider>();
        if (_provider == null)
        {
            Debug.LogWarning($"{LogPrefix} InvectorPlayerProvider not found. Respawn disabled.");
            return;
        }

        _provider.OnDeath += OnPlayerDeath;

        // Cache the controller reference immediately.
        if (_provider.PlayerObject != null)
            _controller = _provider.PlayerObject.GetComponent<vThirdPersonController>();

        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Bound to InvectorPlayerProvider.");
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

        // Refresh controller reference in case it was set after Start.
        if (_controller == null && _provider?.PlayerObject != null)
            _controller = _provider.PlayerObject.GetComponent<vThirdPersonController>();

        if (_controller == null)
        {
            Debug.LogWarning($"{LogPrefix} Cannot respawn — vThirdPersonController not found.");
            _isRespawning = false;
            yield break;
        }

        PerformRespawn();
        _isRespawning = false;
    }

    private void PerformRespawn()
    {
        // ── 1. Exit ragdoll state ─────────────────────────────────────────────
        if (_controller.ragdolled)
            _controller.ResetRagdoll();

        // ── 2. Teleport — disable physics so the warp is instant ─────────────
        var rb = _controller.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity    = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _controller.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

        // ── 3. Restore health — this clears isDead via the property setter ────
        int targetHealth = Mathf.RoundToInt(_controller.maxHealth * respawnHealthFraction);
        _controller.ChangeHealth(targetHealth);

        // ── 4. Re-enable animator dead state ─────────────────────────────────
        if (_controller.animator != null)
            _controller.animator.SetBool(vAnimatorParameters.IsDead, false);

        if (showDebugLogs)
            Debug.Log($"{LogPrefix} Player respawned at '{respawnPoint.name}' with {targetHealth} HP.");
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

        if (_controller == null && _provider?.PlayerObject != null)
            _controller = _provider.PlayerObject.GetComponent<vThirdPersonController>();

        if (_controller != null)
            PerformRespawn();
    }
}
