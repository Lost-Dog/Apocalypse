using UnityEngine;
using JUTPS;

public class EnemyKillRewardHandler : MonoBehaviour
{
    [Header("Reward Configuration")]
    [SerializeField] private int baseXPReward = 50;
    [SerializeField] private int xpVariance = 10;
    [SerializeField] private float lootDropChance = 0.5f;
    
    [Header("Enemy Info")]
    [SerializeField] private int enemyLevel = 1;
    [SerializeField] private bool isElite = false;
    [SerializeField] private bool isBoss = false;
    
    [Header("Multipliers")]
    [SerializeField] private float eliteXPMultiplier = 2f;
    [SerializeField] private float bossXPMultiplier = 5f;
    [SerializeField] private float eliteLootChance = 0.75f;
    [SerializeField] private float bossLootChance = 1f;
    
    [Header("Health & Stamina on Kill")]
    [SerializeField] private bool restoreHealthOnKill = true;
    [SerializeField] private float healthRestoreAmount = 20f;
    [SerializeField] private float healthRestorePercentage = 0f;
    [SerializeField] private bool restoreStaminaOnKill = true;
    [SerializeField] private float staminaRestoreAmount = 30f;
    [SerializeField] private float staminaRestorePercentage = 0f;
    
    private JUHealth health;
    private bool hasRewardedPlayer = false;
    
    private void Start()
    {
        health = GetComponent<JUHealth>();
        
        if (health != null)
        {
            health.OnDeath.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: JUHealth component not found!");
        }
    }
    
    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnEnemyDeath);
        }
    }
    
    private void OnEnemyDeath()
    {
        if (hasRewardedPlayer) return;
        
        hasRewardedPlayer = true;
        
        RewardPlayer();
    }
    
    private void RewardPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found! Cannot reward XP/loot.");
            return;
        }
        
        PlayerSystemBridge playerBridge = player.GetComponent<PlayerSystemBridge>();
        if (playerBridge == null)
        {
            Debug.LogWarning("PlayerSystemBridge not found on player! Cannot reward XP/loot.");
            return;
        }
        
        GiveXPReward(playerBridge);
        TryDropLoot(playerBridge);
        RestoreHealthOnKill(player);
        RestoreStaminaOnKill(player);
    }
    
    private void GiveXPReward(PlayerSystemBridge playerBridge)
    {
        int xpReward = CalculateXPReward();
        
        playerBridge.GainExperience(xpReward);
        
        Debug.Log($"{gameObject.name} killed! Player gained {xpReward} XP");
    }
    
    private int CalculateXPReward()
    {
        int xp = baseXPReward + Random.Range(-xpVariance, xpVariance + 1);
        
        if (isBoss)
        {
            xp = Mathf.RoundToInt(xp * bossXPMultiplier);
        }
        else if (isElite)
        {
            xp = Mathf.RoundToInt(xp * eliteXPMultiplier);
        }
        
        return Mathf.Max(xp, 1);
    }
    
    private void TryDropLoot(PlayerSystemBridge playerBridge)
    {
        float dropChance = lootDropChance;
        
        if (isBoss)
        {
            dropChance = bossLootChance;
        }
        else if (isElite)
        {
            dropChance = eliteLootChance;
        }
        
        if (Random.value <= dropChance)
        {
            DropLoot(playerBridge.GetPlayerLevel());
        }
    }
    
    private void DropLoot(int playerLevel)
    {
        if (GameManager.Instance == null || GameManager.Instance.lootManager == null)
        {
            Debug.LogWarning("GameManager or LootManager not found! Cannot drop loot.");
            return;
        }
        
        Vector3 dropPosition = transform.position + Vector3.up;
        
        if (isBoss)
        {
            GameManager.Instance.lootManager.DropLootWithRarity(
                dropPosition, 
                playerLevel, 
                LootManager.Rarity.Epic
            );
        }
        else if (isElite)
        {
            GameManager.Instance.lootManager.DropLootWithRarity(
                dropPosition, 
                playerLevel, 
                LootManager.Rarity.Rare
            );
        }
        else
        {
            GameManager.Instance.lootManager.DropLoot(dropPosition, playerLevel);
        }
        
        Debug.Log($"Loot dropped at {dropPosition}");
    }
    
    public void SetEnemyLevel(int level)
    {
        enemyLevel = level;
    }
    
    public void SetAsElite(bool elite)
    {
        isElite = elite;
        if (elite)
        {
            isBoss = false;
        }
    }
    
    public void SetAsBoss(bool boss)
    {
        isBoss = boss;
        if (boss)
        {
            isElite = false;
        }
    }
    
    private void RestoreHealthOnKill(GameObject player)
    {
        if (!restoreHealthOnKill) return;
        
        JUHealth playerHealth = player.GetComponent<JUHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("JUHealth component not found on player! Cannot restore health.");
            return;
        }
        
        float healthToRestore = healthRestoreAmount;
        
        if (healthRestorePercentage > 0f)
        {
            healthToRestore += playerHealth.MaxHealth * healthRestorePercentage;
        }
        
        float oldHealth = playerHealth.Health;
        playerHealth.Health = Mathf.Min(playerHealth.MaxHealth, playerHealth.Health + healthToRestore);
        
        float actualRestore = playerHealth.Health - oldHealth;
        
        if (actualRestore > 0f)
        {
            Debug.Log($"Restored {actualRestore:F1} health on kill! (Health: {playerHealth.Health:F1}/{playerHealth.MaxHealth})");
        }
    }
    
    private void RestoreStaminaOnKill(GameObject player)
    {
        if (!restoreStaminaOnKill) return;
        
        PlayerStaminaDisplay staminaSystem = FindFirstObjectByType<PlayerStaminaDisplay>();
        if (staminaSystem == null)
        {
            Debug.LogWarning("PlayerStaminaDisplay not found! Cannot restore stamina.");
            return;
        }
        
        float staminaToRestore = staminaRestoreAmount;
        
        if (staminaRestorePercentage > 0f)
        {
            staminaToRestore += staminaSystem.maxStamina * staminaRestorePercentage;
        }
        
        float oldStamina = staminaSystem.currentStamina;
        staminaSystem.RestoreStamina(staminaToRestore);
        
        float actualRestore = staminaSystem.currentStamina - oldStamina;
        
        if (actualRestore > 0f)
        {
            Debug.Log($"Restored {actualRestore:F1} stamina on kill! (Stamina: {staminaSystem.currentStamina:F1}/{staminaSystem.maxStamina})");
        }
    }
}
