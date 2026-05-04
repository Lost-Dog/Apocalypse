using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;

/// <summary>
/// Bridges the GC2 player character to game systems (GameManager, HUDManager, loot, XP).
/// Health reads from the GC2 Traits component; combat rewards are granted via public API.
/// </summary>
public class PlayerSystemBridge : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Division Systems")]
    [SerializeField] private int playerLevel = 1;

    [Header("Combat Rewards")]
    [SerializeField] private int xpPerKill = 50;
    [SerializeField] private float lootDropChance = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private GameManager gameManager;
    private HUDManager hudManager;
    private Traits playerTraits;
    private Character playerCharacter;
    private double cachedHealth;

    private void Start()
    {
        InitializeSystems();
        SetupDeathCallback();
    }

    private void OnDestroy()
    {
        if (playerCharacter != null)
            playerCharacter.EventDie -= OnPlayerDeath;
    }

    private void InitializeSystems()
    {
        playerCharacter = ShortcutPlayer.Instance?.GetComponent<Character>();
        if (playerCharacter != null)
        {
            playerTraits = playerCharacter.GetComponent<Traits>();
        }
        else
        {
            // Fallback: component may be on this same GameObject (player prefab)
            playerCharacter = GetComponent<Character>();
            playerTraits    = GetComponent<Traits>();
        }

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

        UpdateHealthUI();
    }

    private void SetupDeathCallback()
    {
        if (playerCharacter != null)
            playerCharacter.EventDie += OnPlayerDeath;
    }

    private void Update()
    {
        if (playerTraits == null) return;

        try
        {
            double currentHealth = playerTraits.RuntimeAttributes.Get(HealthAttributeId).Value;
            if (System.Math.Abs(currentHealth - cachedHealth) > 0.001)
            {
                cachedHealth = currentHealth;
                UpdateHealthUI();
            }
        }
        catch (System.Exception) { }

        if (gameManager?.progressionManager != null)
            playerLevel = gameManager.progressionManager.currentLevel;
    }

    private void OnPlayerDeath()
    {
        if (showDebugLogs) Debug.Log("[PlayerSystemBridge] Player died!");
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (hudManager == null || playerTraits == null) return;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            hudManager.UpdateHealthDisplay((float)health.Value, (float)health.MaxValue);
        }
        catch (System.Exception) { }
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

    /// <summary>Restores the given amount of health by adding it to the Traits attribute value.</summary>
    public void Heal(float amount)
    {
        if (playerTraits == null) return;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            health.Value = System.Math.Min(health.Value + amount, health.MaxValue);
            UpdateHealthUI();

            if (showDebugLogs) Debug.Log($"[PlayerSystemBridge] Healed {amount} HP. Current: {health.Value:F0}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerSystemBridge] Heal failed — {e.Message}");
        }
    }

    /// <summary>Restores the player to full health.</summary>
    public void HealToFull()
    {
        if (playerTraits == null) return;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            health.Value = health.MaxValue;
            UpdateHealthUI();

            if (showDebugLogs) Debug.Log("[PlayerSystemBridge] Fully healed!");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerSystemBridge] HealToFull failed — {e.Message}");
        }
    }

    /// <summary>Returns current health as a 0–1 normalised value.</summary>
    public float GetHealthPercentage()
    {
        if (playerTraits == null) return 0f;

        try
        {
            RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
            return (float)(health.Value / health.MaxValue);
        }
        catch (System.Exception) { return 0f; }
    }

    /// <summary>Returns true if the player character is not dead.</summary>
    public bool IsAlive() => playerCharacter != null && !playerCharacter.IsDead;

    /// <summary>Returns the current player level.</summary>
    public int GetPlayerLevel() => playerLevel;

    /// <summary>Awards the given XP to the player.</summary>
    public void GainExperience(int amount) =>
        gameManager?.progressionManager?.AddExperience(amount);
}
