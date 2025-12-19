using UnityEngine;
using JUTPS;

public class DifficultyScaler : MonoBehaviour
{
    [Header("Base Stats")]
    [Tooltip("Base health at level 1")]
    public float baseHealth = 100f;
    
    [Tooltip("Base damage at level 1")]
    public float baseDamage = 10f;
    
    [Header("Scaling Multipliers")]
    [Tooltip("Health increase per level (0.2 = 20% increase)")]
    public float healthMultiplierPerLevel = 0.2f;
    
    [Tooltip("Damage increase per level (0.15 = 15% increase)")]
    public float damageMultiplierPerLevel = 0.15f;
    
    [Header("Current Stats")]
    [Tooltip("Current difficulty level")]
    public int currentLevel = 1;
    
    [Tooltip("Calculated health after scaling")]
    public float scaledHealth;
    
    [Tooltip("Calculated damage after scaling")]
    public float scaledDamage;
    
    [Header("Auto-Scaling")]
    [Tooltip("Automatically scale to player level on spawn")]
    public bool autoScaleToPlayerLevel = false;
    
    [Tooltip("Use zone level if higher than player level")]
    public bool respectZoneLevel = true;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    
    private JUHealth health;
    private bool hasAppliedScaling = false;
    
    private void OnValidate()
    {
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
        
        if (health != null && baseHealth == 100f)
        {
            baseHealth = health.MaxHealth;
        }
    }
    
    private void Start()
    {
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
        
        if (baseHealth <= 0f && health != null)
        {
            baseHealth = health.MaxHealth;
        }
        
        if (autoScaleToPlayerLevel)
        {
            ApplyScaling(GetEffectiveLevel());
        }
    }
    
    private void OnEnable()
    {
        hasAppliedScaling = false;
        
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
        
        if (autoScaleToPlayerLevel)
        {
            ApplyScaling(GetEffectiveLevel());
        }
    }
    
    public void ApplyScaling(int level)
    {
        if (level < 1)
        {
            Debug.LogWarning($"{gameObject.name}: Invalid level {level}, using level 1");
            level = 1;
        }
        
        currentLevel = level;
        
        scaledHealth = CalculateScaledHealth(level);
        scaledDamage = CalculateScaledDamage(level);
        
        ApplyStatsToCharacter();
        
        hasAppliedScaling = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name} scaled to level {level}: HP={scaledHealth:F0} (base: {baseHealth}), Damage={scaledDamage:F1} (base: {baseDamage})");
        }
    }
    
    private float CalculateScaledHealth(int level)
    {
        float multiplier = 1f + ((level - 1) * healthMultiplierPerLevel);
        return baseHealth * multiplier;
    }
    
    private float CalculateScaledDamage(int level)
    {
        float multiplier = 1f + ((level - 1) * damageMultiplierPerLevel);
        return baseDamage * multiplier;
    }
    
    private void ApplyStatsToCharacter()
    {
        if (health == null)
        {
            health = GetComponent<JUHealth>();
        }
        
        if (health != null)
        {
            health.MaxHealth = scaledHealth;
            health.Health = scaledHealth;
            
            PoolableCharacter poolable = GetComponent<PoolableCharacter>();
            if (poolable != null)
            {
                poolable.health = health;
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"{gameObject.name}: DifficultyScaler could not find JUHealth component!");
            }
        }
    }
    
    private int GetEffectiveLevel()
    {
        if (GameManager.Instance != null)
        {
            int playerLevel = GameManager.Instance.currentPlayerLevel;
            
            if (respectZoneLevel)
            {
                return Mathf.Max(currentLevel, playerLevel);
            }
            
            return playerLevel;
        }
        
        return currentLevel;
    }
    
    public float GetHealthMultiplier()
    {
        return baseHealth > 0 ? scaledHealth / baseHealth : 1f;
    }
    
    public float GetDamageMultiplier()
    {
        return baseDamage > 0 ? scaledDamage / baseDamage : 1f;
    }
    
    public void SetBaseStats(float health, float damage)
    {
        baseHealth = health;
        baseDamage = damage;
        hasAppliedScaling = false;
    }
    
    public void ReapplyScaling()
    {
        hasAppliedScaling = false;
        ApplyScaling(currentLevel);
    }
}
