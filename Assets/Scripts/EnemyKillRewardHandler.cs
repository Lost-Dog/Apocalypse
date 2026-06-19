using EmeraldAI;
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
    [Tooltip("Flat stamina amount to restore")]
    [SerializeField] private float staminaRestoreAmount = 0f;
    [Tooltip("Percentage of max stamina to restore (0.1 = 10%)")]
    [SerializeField] private float staminaRestorePercentage = 0.1f;

    private EmeraldHealth enemyHealth;
    private bool hasRewardedPlayer = false;

    private void Start()
    {
        enemyHealth = GetComponent<EmeraldHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: EmeraldHealth component not found!");
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnEnemyDeath;
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
        GameObject player = FindPlayerObject();
        if (player == null)
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: Player not found.");
            return;
        }

        GC2PlayerProvider gc2Provider = FindGC2Provider(player);
        if (gc2Provider == null)
        {
            Debug.LogWarning($"EnemyKillRewardHandler on {gameObject.name}: GC2PlayerProvider not found on player.");
            return;
        }

        GiveXPReward();
        TryDropLoot();
        RestoreHealthOnKill(gc2Provider);
        RestoreStaminaOnKill();
    }

    // XP -----------------------------------------------------------------------------------------

    private void GiveXPReward()
    {
        int xpReward = CalculateXPReward();
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            GameManager.Instance.progressionManager.AddExperience(xpReward);
        }

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

    private void TryDropLoot()
    {
        float dropChance = isBoss ? bossLootChance : isElite ? eliteLootChance : lootDropChance;

        if (Random.value <= dropChance)
        {
            int playerLevel = 1;
            if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
                playerLevel = Mathf.Max(1, GameManager.Instance.progressionManager.currentLevel);

            DropLoot(playerLevel);
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
    /// Restores health to the GC2 player through trait-backed provider data.
    /// </summary>
    private void RestoreHealthOnKill(GC2PlayerProvider provider)
    {
        if (!restoreHealthOnKill) return;
        if (provider == null || !provider.IsAlive) return;

        float restoreAmount = healthRestoreAmount + provider.MaxHealth * healthRestorePercentage;
        if (restoreAmount > 0f)
        {
            provider.ApplyDamage(-restoreAmount);
            Debug.Log($"Restored {restoreAmount:F1} health on kill. ({provider.Health:F0}/{provider.MaxHealth:F0})");
        }
    }

    // SKILL DELEGATES ----------------------------------------------------------------------------

    private void RestoreStaminaOnKill()
    {
        if (!restoreStaminaOnKill) return;

        SurvivalManager survival = SurvivalManager.Instance;
        if (survival == null || !survival.enableStaminaSystem) return;

        float restoreAmount = staminaRestoreAmount + (survival.maxStamina * staminaRestorePercentage);
        if (restoreAmount > 0f)
        {
            survival.AddStamina(restoreAmount);
        }
    }

    // SETTERS ------------------------------------------------------------------------------------

    /// <summary>Sets the enemy's difficulty level used for XP calculation.</summary>
    public void SetEnemyLevel(int level) => enemyLevel = level;

    /// <summary>Marks this enemy as an elite. Mutually exclusive with boss.</summary>
    public void SetAsElite(bool elite) { isElite = elite; if (elite) isBoss = false; }

    /// <summary>Marks this enemy as a boss. Mutually exclusive with elite.</summary>
    public void SetAsBoss(bool boss)   { isBoss = boss;   if (boss)  isElite = false; }

    // HELPERS ------------------------------------------------------------------------------------

    private static GameObject FindPlayerObject()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;

        GC2PlayerProvider provider = FindFirstObjectByType<GC2PlayerProvider>();
        return provider != null ? provider.gameObject : null;
    }

    private static GC2PlayerProvider FindGC2Provider(GameObject player)
    {
        if (player == null) return null;

        GC2PlayerProvider provider = player.GetComponent<GC2PlayerProvider>();
        if (provider != null) return provider;

        provider = player.GetComponentInChildren<GC2PlayerProvider>(true);
        if (provider != null) return provider;

        provider = player.GetComponentInParent<GC2PlayerProvider>();
        if (provider != null) return provider;

        return FindFirstObjectByType<GC2PlayerProvider>();
    }

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
