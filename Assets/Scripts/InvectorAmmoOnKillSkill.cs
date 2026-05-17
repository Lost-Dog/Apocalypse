using Invector.vShooter;
using Invector.vItemManager;
using UnityEngine;

/// <summary>
/// Skill that replenishes Invector vShooterWeapon ammo when the player kills an enemy.
/// Triggered externally (e.g. by EnemyKillRewardHandler) via OnEnemyKilled().
/// Mirrors the API surface of AmmoOnKillSkill so existing callers can swap with zero changes.
/// </summary>
public class InvectorAmmoOnKillSkill : MonoBehaviour
{
    private const string LogPrefix = "[InvectorAmmoOnKillSkill]";

    [Header("Skill Settings")]
    [Tooltip("Enable/disable the skill.")]
    public bool skillActive = true;

    [Tooltip("Activate skill automatically on Start.")]
    public bool activateOnStart = true;

    [Header("Ammo Replenishment")]
    [Tooltip("Percentage of clip capacity to restore per kill (0–1).")]
    [Range(0f, 1f)]
    public float ammoRestorePercentage = 1.0f;

    [Tooltip("Restore ammo to the active (right-hand) weapon only (true) or every equipped weapon (false).")]
    public bool currentWeaponOnly = false;

    [Tooltip("Minimum rounds to restore per kill.")]
    public int minBulletsToRestore = 5;

    [Tooltip("Maximum rounds to restore per kill (0 = no cap).")]
    public int maxBulletsToRestore = 0;

    [Header("Visual / Audio Feedback")]
    [Tooltip("Show a notification when ammo is restored.")]
    public bool showNotification = true;

    [Tooltip("Audio clip to play when ammo is restored.")]
    public AudioClip ammoRestoreSound;

    [Header("References")]
    [Tooltip("Leave empty to auto-find on Start.")]
    [SerializeField] private vShooterManager shooterManager;

    [Header("Debug")]
    public bool debugMode = false;

    private vAmmoManager ammoManager;
    private AudioSource  audioSource;
    private int          killCount;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (activateOnStart) ActivateSkill();
    }

    private void OnDestroy()
    {
        DeactivateSkill();
    }

    // ── Skill lifecycle ───────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the skill and caches Invector manager references.
    /// </summary>
    public void ActivateSkill()
    {
        if (shooterManager == null)
            shooterManager = FindFirstObjectByType<vShooterManager>();

        if (shooterManager == null)
        {
            Debug.LogError($"{LogPrefix} Could not find a vShooterManager.");
            return;
        }

        ammoManager = shooterManager.GetComponent<vAmmoManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && ammoRestoreSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 0f;
        }

        skillActive = true;

        if (debugMode)
            Debug.Log($"<color=cyan>{LogPrefix} Activated. Restore: {ammoRestorePercentage * 100}%</color>");
    }

    /// <summary>
    /// Disables the skill.
    /// </summary>
    public void DeactivateSkill()
    {
        skillActive = false;

        if (debugMode)
            Debug.Log($"<color=yellow>{LogPrefix} Deactivated.</color>");
    }

    // ── Kill entry point ──────────────────────────────────────────────────────

    /// <summary>
    /// Call this when the player kills an enemy (same signature as AmmoOnKillSkill).
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (!skillActive || shooterManager == null) return;

        killCount++;

        if (debugMode)
            Debug.Log($"<color=green>{LogPrefix} Kill #{killCount} registered.</color>");

        if (currentWeaponOnly)
            RestoreActiveWeaponAmmo();
        else
            RestoreAllWeaponsAmmo();
    }

    // ── Ammo restoration ─────────────────────────────────────────────────────

    private void RestoreActiveWeaponAmmo()
    {
        vShooterWeapon weapon = shooterManager.rWeapon != null
            ? shooterManager.rWeapon
            : shooterManager.lWeapon;

        if (weapon == null)
        {
            if (debugMode) Debug.LogWarning($"{LogPrefix} No active vShooterWeapon — skipping.");
            return;
        }

        int restored = AddAmmo(weapon);

        if (restored > 0)
        {
            if (debugMode)
                Debug.Log($"<color=green>{LogPrefix} +{restored} ammo to active weapon.</color>");

            PlayFeedback(restored, allWeapons: false);
        }
    }

    private void RestoreAllWeaponsAmmo()
    {
        int totalRestored = 0;

        if (shooterManager.rWeapon != null)
            totalRestored += AddAmmo(shooterManager.rWeapon);

        if (shooterManager.lWeapon != null)
            totalRestored += AddAmmo(shooterManager.lWeapon);

        if (totalRestored > 0)
        {
            if (debugMode)
                Debug.Log($"<color=green>{LogPrefix} +{totalRestored} ammo across all weapons.</color>");

            PlayFeedback(totalRestored, allWeapons: true);
        }
    }

    /// <summary>
    /// Adds calculated rounds to the weapon's reserve pool via vAmmoManager.
    /// Also tops up the clip from the freshly added reserve.
    /// Returns actual rounds added.
    /// </summary>
    private int AddAmmo(vShooterWeapon weapon)
    {
        if (weapon == null || weapon.isInfinityAmmo) return 0;

        int clipSize  = weapon.clipSize;
        int toRestore = Mathf.RoundToInt(clipSize * ammoRestorePercentage);
        toRestore = Mathf.Max(toRestore, minBulletsToRestore);
        if (maxBulletsToRestore > 0)
            toRestore = Mathf.Min(toRestore, maxBulletsToRestore);

        if (toRestore <= 0) return 0;

        // Add to reserve via vAmmoManager when available.
        if (ammoManager != null)
        {
            ammoManager.AddAmmo(weapon.ammoID, toRestore);
        }
        else
        {
            // Fallback: add directly into the clip when there is no ammo manager.
            weapon.ammo = Mathf.Min(weapon.ammo + toRestore, clipSize);
        }

        // Top up the current clip from the reserve.
        if (ammoManager != null && clipSize > 0)
        {
            int headroom = Mathf.Max(0, clipSize - weapon.ammo);
            if (headroom > 0)
            {
                vAmmo entry = ammoManager.GetAmmo(weapon.ammoID);
                if (entry != null && entry.count > 0)
                {
                    int fill = Mathf.Min(headroom, entry.count);
                    weapon.ammo += fill;
                    entry.Use(fill);
                }
            }
        }

        return toRestore;
    }

    // ── Feedback ──────────────────────────────────────────────────────────────

    private void PlayFeedback(int amountRestored, bool allWeapons)
    {
        if (audioSource != null && ammoRestoreSound != null)
            audioSource.PlayOneShot(ammoRestoreSound);

        if (showNotification && NotificationManager.Instance != null)
        {
            string message = allWeapons
                ? $"+{amountRestored} Ammo (All Weapons)"
                : $"+{amountRestored} Ammo";

            NotificationManager.Instance.ShowNotification(message);
        }
    }

    // ── Runtime tuning ────────────────────────────────────────────────────────

    /// <summary>Updates the ammo restore percentage at runtime.</summary>
    public void SetAmmoRestorePercentage(float percentage)
    {
        ammoRestorePercentage = Mathf.Clamp01(percentage);

        if (debugMode)
            Debug.Log($"<color=cyan>{LogPrefix} Restore %: {ammoRestorePercentage * 100}%</color>");
    }

    /// <summary>Sets the minimum rounds to restore per kill.</summary>
    public void SetMinBulletsToRestore(int min) => minBulletsToRestore = Mathf.Max(0, min);

    /// <summary>Sets the maximum rounds to restore per kill (0 = no cap).</summary>
    public void SetMaxBulletsToRestore(int max) => maxBulletsToRestore = Mathf.Max(0, max);

    /// <summary>Toggles between active weapon only and all equipped weapons.</summary>
    public void SetCurrentWeaponOnly(bool currentOnly) => currentWeaponOnly = currentOnly;
}
