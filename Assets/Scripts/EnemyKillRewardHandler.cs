using System.Collections;
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
    [Tooltip("Flat health amount to restore")]
    [SerializeField] private float healthRestoreAmount = 0f;
    [Tooltip("Percentage of max health to restore (0.1 = 10%)")]
    [SerializeField] private float healthRestorePercentage = 0.1f;
    [SerializeField] private bool restoreStaminaOnKill = true;
    [Tooltip("Flat stamina amount to restore")]
    [SerializeField] private float staminaRestoreAmount = 0f;
    [Tooltip("Percentage of max stamina to restore (0.1 = 10%)")]
    [SerializeField] private float staminaRestorePercentage = 0.1f;

    [Header("Health On Kill VFX")]
    [Tooltip("VFX prefab that flies from dead enemy to player when health is restored")]
    [SerializeField] private GameObject healthOnKillVFXPrefab;
    [SerializeField] private Vector3 healthVFXStartOffset = Vector3.up;
    [SerializeField] private Vector3 healthVFXTargetOffset = Vector3.up;
    [SerializeField, Min(0.05f)] private float healthVFXTravelDuration = 0.35f;
    [SerializeField, Min(0.1f)] private float healthVFXLifetime = 2f;
    
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
        bool restoredHealth = RestoreHealthOnKill(player);
        if (restoredHealth)
        {
            SpawnHealthOnKillVFX(player.transform);
        }

        RestoreStaminaOnKill(player);
        RestoreAmmoOnKill(player);
        RestoreTemperatureOnKill(player);
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
    
    private bool RestoreHealthOnKill(GameObject player)
    {
        if (!restoreHealthOnKill) return false;
        
        JUHealth playerHealth = player.GetComponent<JUHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("JUHealth component not found on player! Cannot restore health.");
            return false;
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
            return true;
        }

        return false;
    }

    private void SpawnHealthOnKillVFX(Transform playerTransform)
    {
        if (healthOnKillVFXPrefab == null || playerTransform == null) return;

        Vector3 startPosition = transform.position + healthVFXStartOffset;
        GameObject vfx = Instantiate(healthOnKillVFXPrefab, startPosition, Quaternion.identity);
        Destroy(vfx, healthVFXTravelDuration + healthVFXLifetime);

        StartCoroutine(MoveVFXToPlayer(vfx.transform, playerTransform, startPosition));
    }

    private IEnumerator MoveVFXToPlayer(Transform vfxTransform, Transform playerTransform, Vector3 startPosition)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, healthVFXTravelDuration);

        while (elapsed < duration && vfxTransform != null && playerTransform != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 targetPosition = playerTransform.position + healthVFXTargetOffset;
            vfxTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (vfxTransform != null && playerTransform != null)
        {
            vfxTransform.position = playerTransform.position + healthVFXTargetOffset;
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
    
    private void RestoreAmmoOnKill(GameObject player)
    {
        // Check if player has AmmoOnKillSkill
        AmmoOnKillSkill ammoSkill = player.GetComponent<AmmoOnKillSkill>();
        if (ammoSkill != null && ammoSkill.skillActive)
        {
            ammoSkill.OnEnemyKilled(gameObject);
        }
    }
    
    private void RestoreTemperatureOnKill(GameObject player)
    {
        // Check if player has TemperatureRestoreOnKill
        TemperatureRestoreOnKill tempSkill = player.GetComponent<TemperatureRestoreOnKill>();
        if (tempSkill != null && tempSkill.skillActive)
        {
            tempSkill.OnEnemyKilled(gameObject);
        }
    }
}
