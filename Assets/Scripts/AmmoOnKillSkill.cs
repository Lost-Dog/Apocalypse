using UnityEngine;
using JUTPS;
using JUTPS.WeaponSystem;
using JUTPS.InventorySystem;

/// <summary>
/// Skill that replenishes ammo when killing enemies
/// </summary>
public class AmmoOnKillSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("Enable/disable the skill")]
    public bool skillActive = true;
    
    [Tooltip("Activate skill on start")]
    public bool activateOnStart = true;
    
    [Header("Ammo Replenishment")]
    [Tooltip("Percentage of max ammo to restore (0-1)")]
    [Range(0f, 1f)]
    public float ammoRestorePercentage = 1.0f;
    
    [Tooltip("Restore ammo to current weapon only (true) or all weapons (false)")]
    public bool currentWeaponOnly = false;
    
    [Tooltip("Minimum ammo bullets to restore")]
    public int minBulletsToRestore = 5;
    
    [Tooltip("Maximum ammo bullets to restore (0 = no limit)")]
    public int maxBulletsToRestore = 0;
    
    [Header("Visual/Audio Feedback")]
    [Tooltip("Show notification when ammo is restored")]
    public bool showNotification = true;
    
    [Tooltip("Audio clip to play when ammo is restored")]
    public AudioClip ammoRestoreSound;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private JUCharacterController characterController;
    private JUInventory inventory;
    private JUHealth health;
    private AudioSource audioSource;
    private int killCount = 0;
    
    void Start()
    {
        if (activateOnStart)
        {
            ActivateSkill();
        }
    }
    
    public void ActivateSkill()
    {
        // Get required components
        characterController = GetComponent<JUCharacterController>();
        inventory = GetComponent<JUInventory>();
        health = GetComponent<JUHealth>();
        
        if (characterController == null)
        {
            Debug.LogError("[AmmoOnKillSkill] No JUCharacterController found!");
            return;
        }
        
        if (inventory == null)
        {
            Debug.LogWarning("[AmmoOnKillSkill] No JUInventory found - skill won't work!");
            return;
        }
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && ammoRestoreSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Subscribe to kill events
        SubscribeToKillEvents();
        
        skillActive = true;
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[AmmoOnKillSkill] Skill activated! Ammo restore: {ammoRestorePercentage * 100}%</color>");
        }
    }
    
    public void DeactivateSkill()
    {
        UnsubscribeFromKillEvents();
        skillActive = false;
        
        if (debugMode)
        {
            Debug.Log("<color=yellow>[AmmoOnKillSkill] Skill deactivated!</color>");
        }
    }
    
    private void SubscribeToKillEvents()
    {
        // Events are handled by EnemyKillRewardHandler calling OnEnemyKilled directly
        // No need to subscribe to anything here
    }
    
    private void UnsubscribeFromKillEvents()
    {
        // No events to unsubscribe from
    }
    
    /// <summary>
    /// Call this method when player kills an enemy
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (!skillActive || inventory == null) return;
        
        killCount++;
        
        if (debugMode)
        {
            Debug.Log($"<color=green>[AmmoOnKillSkill] Enemy killed! Total kills: {killCount}</color>");
        }
        
        RestoreAmmo();
    }
    
    private void RestoreAmmo()
    {
        if (currentWeaponOnly)
        {
            RestoreCurrentWeaponAmmo();
        }
        else
        {
            RestoreAllWeaponsAmmo();
        }
    }
    
    private void RestoreCurrentWeaponAmmo()
    {
        // Get current weapon in right hand
        Weapon currentWeapon = inventory.WeaponInUseInRightHand;
        
        if (currentWeapon == null)
        {
            // Try left hand
            currentWeapon = inventory.WeaponInUseInLeftHand;
        }
        
        if (currentWeapon != null)
        {
            int ammoToRestore = CalculateAmmoToRestore(currentWeapon);
            
            if (ammoToRestore > 0)
            {
                currentWeapon.TotalBullets += ammoToRestore;
                
                if (debugMode)
                {
                    Debug.Log($"<color=green>[AmmoOnKillSkill] Restored {ammoToRestore} ammo to {currentWeapon.ItemName}. Total: {currentWeapon.TotalBullets}</color>");
                }
                
                PlayFeedback(currentWeapon.ItemName, ammoToRestore);
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning("[AmmoOnKillSkill] No weapon equipped - cannot restore ammo");
        }
    }
    
    private void RestoreAllWeaponsAmmo()
    {
        int totalRestored = 0;
        
        // Restore ammo to all weapons in right hand
        if (inventory.WeaponsRightHand != null)
        {
            foreach (Weapon weapon in inventory.WeaponsRightHand)
            {
                if (weapon != null)
                {
                    int ammoToRestore = CalculateAmmoToRestore(weapon);
                    if (ammoToRestore > 0)
                    {
                        weapon.TotalBullets += ammoToRestore;
                        totalRestored += ammoToRestore;
                    }
                }
            }
        }
        
        // Restore ammo to all weapons in left hand
        if (inventory.WeaponsLeftHand != null)
        {
            foreach (Weapon weapon in inventory.WeaponsLeftHand)
            {
                if (weapon != null)
                {
                    int ammoToRestore = CalculateAmmoToRestore(weapon);
                    if (ammoToRestore > 0)
                    {
                        weapon.TotalBullets += ammoToRestore;
                        totalRestored += ammoToRestore;
                    }
                }
            }
        }
        
        if (totalRestored > 0)
        {
            if (debugMode)
            {
                Debug.Log($"<color=green>[AmmoOnKillSkill] Restored {totalRestored} total ammo to all weapons</color>");
            }
            
            PlayFeedback("all weapons", totalRestored);
        }
    }
    
    private int CalculateAmmoToRestore(Weapon weapon)
    {
        if (weapon == null || weapon.InfiniteAmmo) return 0;
        
        // Calculate base ammo to restore (percentage of max magazine size)
        int baseRestore = Mathf.RoundToInt(weapon.BulletsPerMagazine * ammoRestorePercentage);
        
        // Apply min/max constraints
        int ammoToRestore = Mathf.Max(baseRestore, minBulletsToRestore);
        
        if (maxBulletsToRestore > 0)
        {
            ammoToRestore = Mathf.Min(ammoToRestore, maxBulletsToRestore);
        }
        
        return ammoToRestore;
    }
    
    private void PlayFeedback(string weaponName, int amountRestored)
    {
        // Play sound
        if (audioSource != null && ammoRestoreSound != null)
        {
            audioSource.PlayOneShot(ammoRestoreSound);
        }
        
        // Show notification
        if (showNotification && NotificationManager.Instance != null)
        {
            string message = currentWeaponOnly 
                ? $"+{amountRestored} Ammo"
                : $"+{amountRestored} Ammo (All Weapons)";
            
            NotificationManager.Instance.ShowNotification(message);
        }
    }
    
    /// <summary>
    /// Update ammo restore percentage at runtime
    /// </summary>
    public void SetAmmoRestorePercentage(float percentage)
    {
        ammoRestorePercentage = Mathf.Clamp01(percentage);
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[AmmoOnKillSkill] Ammo restore percentage set to {ammoRestorePercentage * 100}%</color>");
        }
    }
    
    /// <summary>
    /// Set minimum bullets to restore
    /// </summary>
    public void SetMinBulletsToRestore(int min)
    {
        minBulletsToRestore = Mathf.Max(0, min);
    }
    
    /// <summary>
    /// Set maximum bullets to restore
    /// </summary>
    public void SetMaxBulletsToRestore(int max)
    {
        maxBulletsToRestore = Mathf.Max(0, max);
    }
    
    /// <summary>
    /// Toggle between current weapon only and all weapons
    /// </summary>
    public void SetCurrentWeaponOnly(bool currentOnly)
    {
        currentWeaponOnly = currentOnly;
    }
    
    void OnDestroy()
    {
        DeactivateSkill();
    }
}
