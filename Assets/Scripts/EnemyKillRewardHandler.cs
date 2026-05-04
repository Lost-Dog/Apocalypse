using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using System;
using UnityEngine;

public class EnemyKillRewardHandler : MonoBehaviour
{
    private const string HealthAttributeId  = "health";
    private const string StaminaAttributeId = "stamina";

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

    private Character enemyCharacter;
    private bool hasRewardedPlayer = false;

    private void Start()
    {
        enemyCharacter = GetComponent<Character>();

        if (enemyCharacter != null)
        {
            enemyCharacter.EventDie += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: GC2 Character component not found!");
        }
    }

    private void OnDestroy()
    {
        if (enemyCharacter != null)
        {
            enemyCharacter.EventDie -= OnEnemyDeath;
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
        GameObject player = ShortcutPlayer.Instance;
        if (player == null)
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: Player not found via ShortcutPlayer.");
            return;
        }

        PlayerSystemBridge playerBridge = player.GetComponent<PlayerSystemBridge>();
        if (playerBridge == null)
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: PlayerSystemBridge not found on player.");
            return;
        }

        GiveXPReward(playerBridge);
        TryDropLoot(playerBridge);
        RestoreAttributeOnKill(player, HealthAttributeId,  restoreHealthOnKill,  healthRestoreAmount,  healthRestorePercentage);
        RestoreAttributeOnKill(player, StaminaAttributeId, restoreStaminaOnKill, staminaRestoreAmount, staminaRestorePercentage);
        RestoreAmmoOnKill(player);
        RestoreTemperatureOnKill(player);
    }

    // XP -----------------------------------------------------------------------------------------

    private void GiveXPReward(PlayerSystemBridge playerBridge)
    {
        int xpReward = CalculateXPReward();
        playerBridge.GainExperience(xpReward);
        Debug.Log($"{gameObject.name} killed! Player gained {xpReward} XP");
    }

    private int CalculateXPReward()
    {
        int xp = baseXPReward + UnityEngine.Random.Range(-xpVariance, xpVariance + 1);

        if (isBoss)        xp = Mathf.RoundToInt(xp * bossXPMultiplier);
        else if (isElite)  xp = Mathf.RoundToInt(xp * eliteXPMultiplier);

        return Mathf.Max(xp, 1);
    }

    // LOOT ---------------------------------------------------------------------------------------

    private void TryDropLoot(PlayerSystemBridge playerBridge)
    {
        float dropChance = isBoss ? bossLootChance : isElite ? eliteLootChance : lootDropChance;

        if (UnityEngine.Random.value <= dropChance)
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
            GameManager.Instance.lootManager.DropLootWithRarity(dropPosition, playerLevel, LootManager.Rarity.Epic);
        else if (isElite)
            GameManager.Instance.lootManager.DropLootWithRarity(dropPosition, playerLevel, LootManager.Rarity.Rare);
        else
            GameManager.Instance.lootManager.DropLoot(dropPosition, playerLevel);

        Debug.Log($"Loot dropped at {dropPosition}");
    }

    // ATTRIBUTE RESTORE --------------------------------------------------------------------------

    /// <summary>
    /// Restores a flat amount plus a percentage of the attribute's max value on the player's Traits.
    /// </summary>
    private void RestoreAttributeOnKill(GameObject player, string attributeId, bool enabled,
                                        float flatAmount, float percentage)
    {
        if (!enabled) return;

        Traits traits = player.GetComponent<Traits>();
        if (traits == null) return;

        RuntimeAttributeData attribute;
        try { attribute = traits.RuntimeAttributes.Get(attributeId); }
        catch (Exception) { return; }

        double toRestore = flatAmount + attribute.MaxValue * percentage;
        double oldValue  = attribute.Value;
        attribute.Value  = Math.Min(attribute.MaxValue, attribute.Value + toRestore);
        double restored  = attribute.Value - oldValue;

        if (restored > 0)
            Debug.Log($"Restored {restored:F1} {attributeId} on kill. ({attribute.Value:F1}/{attribute.MaxValue:F1})");
    }

    // SKILL DELEGATES ----------------------------------------------------------------------------

    private void RestoreAmmoOnKill(GameObject player)
    {
        AmmoOnKillSkill ammoSkill = player.GetComponent<AmmoOnKillSkill>();
        if (ammoSkill != null && ammoSkill.skillActive)
            ammoSkill.OnEnemyKilled(gameObject);
    }

    private void RestoreTemperatureOnKill(GameObject player)
    {
        TemperatureRestoreOnKill tempSkill = player.GetComponent<TemperatureRestoreOnKill>();
        if (tempSkill != null && tempSkill.skillActive)
            tempSkill.OnEnemyKilled(gameObject);
    }

    // SETTERS ------------------------------------------------------------------------------------

    /// <summary>Sets the enemy's difficulty level used for XP calculation.</summary>
    public void SetEnemyLevel(int level) => enemyLevel = level;

    /// <summary>Marks this enemy as an elite. Mutually exclusive with boss.</summary>
    public void SetAsElite(bool elite) { isElite = elite; if (elite) isBoss = false; }

    /// <summary>Marks this enemy as a boss. Mutually exclusive with elite.</summary>
    public void SetAsBoss(bool boss)   { isBoss = boss;   if (boss)  isElite = false; }
}
