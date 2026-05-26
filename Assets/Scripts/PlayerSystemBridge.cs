using UnityEngine;

/// <summary>
/// Bridges the player character to game systems (GameManager, HUDManager, loot, XP).
/// All player data access goes through IPlayerProvider.
/// </summary>
public class PlayerSystemBridge : MonoBehaviour
{
    [Header("Player Provider")]
    [Tooltip("Assign any IPlayerProvider implementation here.")]
    [SerializeField] private MonoBehaviour playerProviderObject;

    [Header("Division Systems")]
    [SerializeField] private int playerLevel = 1;

    [Header("Combat Rewards")]
    [SerializeField] private int xpPerKill = 50;
    [SerializeField] private float lootDropChance = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private IPlayerProvider playerProvider;
    private GameManager     gameManager;
    private HUDManager      hudManager;
    private float           lastKnownHealth = -1f;

    private void Start()
    {
        ResolveProvider();
        InitializeSystems();
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
        {
            playerProvider.OnDeath         -= OnPlayerDeath;
            playerProvider.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void ResolveProvider()
    {
        playerProvider = playerProviderObject as IPlayerProvider;

        if (playerProvider == null)
        {
            playerProvider = GetComponent<IPlayerProvider>() ?? FindAnyPlayerProvider();
        }

        if (playerProvider == null)
        {
            Debug.LogWarning("[PlayerSystemBridge] No IPlayerProvider found. Some features will be disabled.");
            return;
        }

        playerProvider.OnDeath         += OnPlayerDeath;
        playerProvider.OnHealthChanged += OnHealthChanged;
    }

    private void InitializeSystems()
    {
        gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            hudManager  = gameManager.hudManager;
            playerLevel = gameManager.progressionManager?.currentLevel ?? playerLevel;
        }
        else
        {
            Debug.LogWarning("[PlayerSystemBridge] GameManager not found. Some features will be disabled.");
        }

        if (playerProvider != null)
        {
            lastKnownHealth = playerProvider.Health;
        }

        UpdateHealthUI();
    }

    private void Update()
    {
        if (gameManager?.progressionManager != null)
            playerLevel = gameManager.progressionManager.currentLevel;
    }

    private void OnPlayerDeath()
    {
        if (showDebugLogs) Debug.Log("[PlayerSystemBridge] Player died!");

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnPlayerDied(GetPlayerPosition());
        }

        UpdateHealthUI();
    }

    private void OnHealthChanged(float current, float max)
    {
        if (lastKnownHealth >= 0f && current < lastKnownHealth && ChallengeManager.Instance != null)
        {
            float damageAmount = lastKnownHealth - current;
            ChallengeManager.Instance.OnPlayerDamaged(GetPlayerPosition(), damageAmount);
        }

        lastKnownHealth = current;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (hudManager == null || playerProvider == null) return;
        hudManager.UpdateHealthDisplay(playerProvider.Health, playerProvider.MaxHealth);
    }

    // PUBLIC API ----------------------------------------------------------------------------------

    /// <summary>Called by external systems when an enemy is killed by the player.</summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (gameManager == null) return;

        if (gameManager.progressionManager != null)
        {
            gameManager.progressionManager.AddExperience(xpPerKill);
            if (showDebugLogs) Debug.Log($"[PlayerSystemBridge] Enemy killed! Gained {xpPerKill} XP");
        }

        if (gameManager.lootManager != null && Random.value <= lootDropChance)
        {
            gameManager.lootManager.DropLoot(enemy.transform.position, playerLevel);
            if (showDebugLogs) Debug.Log($"[PlayerSystemBridge] Loot dropped at {enemy.transform.position}");
        }
    }

    /// <summary>Restores the given amount of health.</summary>
    public void Heal(float amount)
    {
        if (playerProvider == null) return;
        playerProvider.ApplyDamage(-amount);
        if (showDebugLogs) Debug.Log($"[PlayerSystemBridge] Healed {amount} HP. Current: {playerProvider.Health:F0}");
    }

    /// <summary>Restores the player to full health.</summary>
    public void HealToFull()
    {
        if (playerProvider == null) return;
        playerProvider.SetHealth(playerProvider.MaxHealth);
        if (showDebugLogs) Debug.Log("[PlayerSystemBridge] Fully healed!");
    }

    /// <summary>Returns current health as a 0–1 normalised value.</summary>
    public float GetHealthPercentage() =>
        playerProvider != null && playerProvider.MaxHealth > 0f
            ? playerProvider.Health / playerProvider.MaxHealth
            : 0f;

    /// <summary>Returns true if the player character is alive.</summary>
    public bool IsAlive() => playerProvider != null && playerProvider.IsAlive;

    /// <summary>Returns the current player level.</summary>
    public int GetPlayerLevel() => playerLevel;

    /// <summary>Awards the given XP to the player.</summary>
    public void GainExperience(int amount) =>
        gameManager?.progressionManager?.AddExperience(amount);

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }

    private Vector3 GetPlayerPosition()
    {
        if (playerProvider?.PlayerObject != null)
        {
            return playerProvider.PlayerObject.transform.position;
        }

        return transform.position;
    }
}
