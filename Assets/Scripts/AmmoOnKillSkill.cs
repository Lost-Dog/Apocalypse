using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using UnityEngine;

/// <summary>
/// Skill that replenishes ShooterWeapon ammo when the player kills an enemy.
/// Triggered externally by EnemyKillRewardHandler via OnEnemyKilled().
/// </summary>
public class AmmoOnKillSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("Enable/disable the skill.")]
    public bool skillActive = true;

    [Tooltip("Activate skill on start.")]
    public bool activateOnStart = true;

    [Header("Ammo Replenishment")]
    [Tooltip("Percentage of magazine capacity to restore per kill (0–1).")]
    [Range(0f, 1f)]
    public float ammoRestorePercentage = 1.0f;

    [Tooltip("Restore ammo to the active weapon only (true) or every equipped weapon (false).")]
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

    [Header("Debug")]
    public bool debugMode = false;

    private Character playerCharacter;
    private AudioSource audioSource;
    private Args args;
    private int killCount;

    private void Start()
    {
        if (activateOnStart) ActivateSkill();
    }

    private void OnDestroy()
    {
        DeactivateSkill();
    }

    // LIFECYCLE: ---------------------------------------------------------------------------------

    /// <summary>
    /// Initialises the skill and caches the GC2 Character component.
    /// </summary>
    public void ActivateSkill()
    {
        playerCharacter = GetComponent<Character>();

        if (playerCharacter == null)
        {
            Debug.LogError("[AmmoOnKillSkill] No GC2 Character component found on this GameObject.");
            return;
        }

        args = new Args(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && ammoRestoreSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        skillActive = true;

        if (debugMode)
            Debug.Log($"<color=cyan>[AmmoOnKillSkill] Activated. Restore: {ammoRestorePercentage * 100}%</color>");
    }

    /// <summary>
    /// Disables the skill.
    /// </summary>
    public void DeactivateSkill()
    {
        skillActive = false;

        if (debugMode)
            Debug.Log("<color=yellow>[AmmoOnKillSkill] Deactivated.</color>");
    }

    // KILL ENTRY POINT: --------------------------------------------------------------------------

    /// <summary>
    /// Called by EnemyKillRewardHandler when the player kills an enemy.
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (!skillActive || playerCharacter == null) return;

        killCount++;

        if (debugMode)
            Debug.Log($"<color=green>[AmmoOnKillSkill] Kill #{killCount} registered.</color>");

        if (currentWeaponOnly)
            RestoreActiveWeaponAmmo();
        else
            RestoreAllWeaponsAmmo();
    }

    // AMMO RESTORATION: --------------------------------------------------------------------------

    private void RestoreActiveWeaponAmmo()
    {
        ShooterWeapon weapon = playerCharacter.Combat.GetActiveWeapon<ShooterWeapon>();

        if (weapon == null)
        {
            if (debugMode) Debug.LogWarning("[AmmoOnKillSkill] No active ShooterWeapon — skipping.");
            return;
        }

        int restored = AddAmmo(weapon);

        if (restored > 0)
        {
            if (debugMode)
                Debug.Log($"<color=green>[AmmoOnKillSkill] +{restored} ammo to active weapon.</color>");

            PlayFeedback(restored, allWeapons: false);
        }
    }

    private void RestoreAllWeaponsAmmo()
    {
        int totalRestored = 0;

        foreach (Weapon slot in playerCharacter.Combat.Weapons)
        {
            if (slot.Asset is ShooterWeapon shooterWeapon)
                totalRestored += AddAmmo(shooterWeapon);
        }

        if (totalRestored > 0)
        {
            if (debugMode)
                Debug.Log($"<color=green>[AmmoOnKillSkill] +{totalRestored} ammo across all weapons.</color>");

            PlayFeedback(totalRestored, allWeapons: true);
        }
    }

    /// <summary>
    /// Adds calculated ammo to the weapon's ShooterMunition. Returns actual rounds added.
    /// </summary>
    private int AddAmmo(ShooterWeapon weapon)
    {
        if (weapon == null) return 0;

        // Infinite-ammo weapons need no top-up
        if (weapon.Magazine.GetTotalAmmo(args) >= int.MaxValue) return 0;

        int magazineSize = weapon.Magazine.GetHasMagazine(args)
            ? weapon.Magazine.GetMagazineSize(args)
            : 0;

        int toRestore = Mathf.RoundToInt(magazineSize * ammoRestorePercentage);
        toRestore = Mathf.Max(toRestore, minBulletsToRestore);
        if (maxBulletsToRestore > 0)
            toRestore = Mathf.Min(toRestore, maxBulletsToRestore);

        if (toRestore <= 0) return 0;

        ShooterMunition munition = playerCharacter.Combat.RequestMunition(weapon) as ShooterMunition;
        if (munition == null) return 0;

        // Add to reserve total and top up magazine if it has headroom
        int oldTotal = munition.Total;
        munition.Total += toRestore;
        int actualRestored = munition.Total - oldTotal;

        // Also fill magazine headroom from the newly added reserve
        if (weapon.Magazine.GetHasMagazine(args))
        {
            int headroom = Mathf.Max(0, magazineSize - munition.InMagazine);
            int fill = Mathf.Min(headroom, munition.Total);
            munition.InMagazine += fill;
            munition.Total -= fill;
        }

        return actualRestored;
    }

    // FEEDBACK: ----------------------------------------------------------------------------------

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

    // TUNING SETTERS: ----------------------------------------------------------------------------

    /// <summary>
    /// Updates the ammo restore percentage at runtime.
    /// </summary>
    public void SetAmmoRestorePercentage(float percentage)
    {
        ammoRestorePercentage = Mathf.Clamp01(percentage);

        if (debugMode)
            Debug.Log($"<color=cyan>[AmmoOnKillSkill] Restore %: {ammoRestorePercentage * 100}%</color>");
    }

    /// <summary>
    /// Sets the minimum rounds to restore per kill.
    /// </summary>
    public void SetMinBulletsToRestore(int min) => minBulletsToRestore = Mathf.Max(0, min);

    /// <summary>
    /// Sets the maximum rounds to restore per kill (0 = no cap).
    /// </summary>
    public void SetMaxBulletsToRestore(int max) => maxBulletsToRestore = Mathf.Max(0, max);

    /// <summary>
    /// Toggles between active weapon only and all equipped weapons.
    /// </summary>
    public void SetCurrentWeaponOnly(bool currentOnly) => currentWeaponOnly = currentOnly;
}
