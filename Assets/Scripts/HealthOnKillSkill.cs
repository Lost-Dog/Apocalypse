using System;
using System.Collections;
using Invector.vCharacterController;
using UnityEngine;

/// <summary>
/// When the player kills an enemy, schedules a full health reset over
/// <see cref="regenDuration"/> seconds using Invector's AddHealth API.
/// A new kill during an active regen window restarts the timer.
///
/// Fires <see cref="OnRegenStarted"/> / <see cref="OnRegenStopped"/> so UI
/// components (e.g. HealthRegenPulse) can react without polling.
/// </summary>
public class HealthOnKillSkill : MonoBehaviour
{
    private const string LogPrefix = "[HealthOnKillSkill]";

    [Header("Skill Settings")]
    [Tooltip("Enable / disable the skill.")]
    public bool skillActive = true;

    [Tooltip("Activate the skill automatically on Start.")]
    public bool activateOnStart = true;

    [Header("Regen Settings")]
    [Tooltip("Seconds over which health is fully restored after a kill.")]
    public float regenDuration = 4f;

    [Tooltip("How often (seconds) a heal tick is applied during regen.")]
    public float tickInterval = 0.1f;

    [Header("Feedback")]
    [Tooltip("Show a HUD notification when regen begins.")]
    public bool showNotification = true;

    [Tooltip("Notification text shown when regen starts.")]
    public string notificationText = "Health Regenerating";

    [Header("Debug")]
    public bool debugMode = false;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when a regen window begins (or is restarted by a new kill).</summary>
    public event Action OnRegenStarted;

    /// <summary>Raised when the regen window completes or is cancelled.</summary>
    public event Action OnRegenStopped;

    // ── Internals ─────────────────────────────────────────────────────────────

    private vThirdPersonController _controller;
    private Coroutine              _regenCoroutine;
    private bool                   _isRegenerating;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (activateOnStart)
            ActivateSkill();
    }

    private void OnDestroy()
    {
        DeactivateSkill();
    }

    // ── Skill lifecycle ───────────────────────────────────────────────────────

    /// <summary>Caches the Invector controller reference and enables the skill.</summary>
    public void ActivateSkill()
    {
        if (_controller == null)
            _controller = GetComponent<vThirdPersonController>();

        if (_controller == null)
            _controller = FindFirstObjectByType<vThirdPersonController>();

        if (_controller == null)
        {
            Debug.LogError($"{LogPrefix} Could not find a vThirdPersonController.");
            return;
        }

        skillActive = true;

        if (debugMode)
            Debug.Log($"<color=cyan>{LogPrefix} Activated on '{_controller.name}'.</color>");
    }

    /// <summary>Disables the skill and stops any active regen.</summary>
    public void DeactivateSkill()
    {
        skillActive = false;
        StopRegen();

        if (debugMode)
            Debug.Log($"<color=yellow>{LogPrefix} Deactivated.</color>");
    }

    // ── Kill entry point ──────────────────────────────────────────────────────

    /// <summary>
    /// Call this when the player kills an enemy.
    /// Restarts the regen window if one is already running.
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (!skillActive || _controller == null) return;
        if (_controller.isDead) return;

        BeginRegen();
    }

    // ── Regen logic ───────────────────────────────────────────────────────────

    private void BeginRegen()
    {
        // Restart the coroutine to reset the 4-second window.
        if (_regenCoroutine != null)
            StopCoroutine(_regenCoroutine);

        _regenCoroutine = StartCoroutine(RegenCoroutine());
    }

    private IEnumerator RegenCoroutine()
    {
        _isRegenerating = true;
        OnRegenStarted?.Invoke();

        if (showNotification && NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification(notificationText);

        float healthMissing = _controller.maxHealth - _controller.currentHealth;
        float elapsed       = 0f;
        float healed        = 0f;

        if (debugMode)
            Debug.Log($"<color=green>{LogPrefix} Regen started. Missing HP: {healthMissing:F0}</color>");

        while (elapsed < regenDuration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            // Re-sample missing HP each tick so external damage is accounted for.
            float remaining     = _controller.maxHealth - _controller.currentHealth;
            float totalDuration = regenDuration;
            float ticksLeft     = Mathf.Max(1f, (totalDuration - elapsed) / tickInterval);
            float tickHeal      = remaining / ticksLeft;
            tickHeal            = Mathf.Max(tickHeal, 0f);

            if (tickHeal > 0f && !_controller.isDead)
            {
                _controller.AddHealth(Mathf.RoundToInt(tickHeal));
                healed += tickHeal;
            }
        }

        // Final correction to guarantee we reach max health exactly.
        if (!_controller.isDead)
        {
            int remainder = Mathf.RoundToInt(_controller.maxHealth - _controller.currentHealth);
            if (remainder > 0)
                _controller.AddHealth(remainder);
        }

        if (debugMode)
            Debug.Log($"<color=green>{LogPrefix} Regen complete. Total healed: {healed:F0}</color>");

        StopRegen();
    }

    private void StopRegen()
    {
        if (!_isRegenerating) return;

        _isRegenerating = false;

        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
            _regenCoroutine = null;
        }

        OnRegenStopped?.Invoke();
    }

    // ── Public state ──────────────────────────────────────────────────────────

    /// <summary>True while a kill-triggered regen window is active.</summary>
    public bool IsRegenerating => _isRegenerating;
}
