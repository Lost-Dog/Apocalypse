using Invector;
using Invector.vCharacterController;
using UnityEngine;

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

    private vHealthController enemyHealth;
    private bool hasRewardedPlayer = false;

    private void Start()
    {
        enemyHealth = GetComponent<vHealthController>();

        if (enemyHealth != null)
        {
            enemyHealth.onDead.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: vHealthController component not found!");
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.onDead.RemoveListener(OnEnemyDeath);
        }
    }

    private void OnEnemyDeath(GameObject deadObject)
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
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: Player not found.");
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
        RestoreHealthOnKill(player);
        RestoreAmmoOnKill(player);
        RestoreTemperatureOnKill(player);
        TriggerHealthOnKillSkill(player);
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
        int xp = baseXPReward + Random.Range(-xpVariance, xpVariance + 1);

        if (isBoss)        xp = Mathf.RoundToInt(xp * bossXPMultiplier);
        else if (isElite)  xp = Mathf.RoundToInt(xp * eliteXPMultiplier);

        return Mathf.Max(xp, 1);
    }

    // LOOT ---------------------------------------------------------------------------------------

    private void TryDropLoot(PlayerSystemBridge playerBridge)
    {
        float dropChance = isBoss ? bossLootChance : isElite ? eliteLootChance : lootDropChance;

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
            GameManager.Instance.lootManager.DropLootWithRarity(dropPosition, playerLevel, LootRarity.Epic);
        else if (isElite)
            GameManager.Instance.lootManager.DropLootWithRarity(dropPosition, playerLevel, LootRarity.Rare);
        else
            GameManager.Instance.lootManager.DropLoot(dropPosition, playerLevel);

        Debug.Log($"Loot dropped at {dropPosition}");
    }

    // HEALTH RESTORE -----------------------------------------------------------------------------

    /// <summary>
    /// Restores health to the player via IPlayerProvider on kill.
    /// </summary>
    private void RestoreHealthOnKill(GameObject player)
    {
        if (!restoreHealthOnKill) return;

        IPlayerProvider provider = FindPlayerProvider(player);
        if (provider == null || !provider.IsAlive) return;

        float restoreAmount = healthRestoreAmount + provider.MaxHealth * healthRestorePercentage;
        if (restoreAmount > 0f)
        {
            provider.ApplyDamage(-restoreAmount);
            Debug.Log($"Restored {restoreAmount:F1} health on kill. ({provider.Health:F0}/{provider.MaxHealth:F0})");
        }
    }

    // SKILL DELEGATES ----------------------------------------------------------------------------

    private void RestoreAmmoOnKill(GameObject player)
    {
        InvectorAmmoOnKillSkill ammoSkill = player.GetComponent<InvectorAmmoOnKillSkill>();
        if (ammoSkill != null && ammoSkill.skillActive)
            ammoSkill.OnEnemyKilled(gameObject);
    }

    private void RestoreTemperatureOnKill(GameObject player)
    {
        TemperatureRestoreOnKill tempSkill = player.GetComponent<TemperatureRestoreOnKill>();
        if (tempSkill != null && tempSkill.skillActive)
            tempSkill.OnEnemyKilled(gameObject);
    }

    private void TriggerHealthOnKillSkill(GameObject player)
    {
        HealthOnKillSkill healthSkill = player.GetComponent<HealthOnKillSkill>();
        if (healthSkill != null && healthSkill.skillActive)
            healthSkill.OnEnemyKilled(gameObject);
    }

    // SETTERS ------------------------------------------------------------------------------------

    /// <summary>Sets the enemy's difficulty level used for XP calculation.</summary>
    public void SetEnemyLevel(int level) => enemyLevel = level;

    /// <summary>Marks this enemy as an elite. Mutually exclusive with boss.</summary>
    public void SetAsElite(bool elite) { isElite = elite; if (elite) isBoss = false; }

    /// <summary>Marks this enemy as a boss. Mutually exclusive with elite.</summary>
    public void SetAsBoss(bool boss)   { isBoss = boss;   if (boss)  isElite = false; }

    // HELPERS ------------------------------------------------------------------------------------

    private static IPlayerProvider FindPlayerProvider(GameObject player)
    {
        IPlayerProvider provider = player.GetComponent<IPlayerProvider>();
        if (provider != null) return provider;

        foreach (var mb in player.GetComponents<MonoBehaviour>())
        {
            if (mb is IPlayerProvider p) return p;
        }
        return null;
    }
}
