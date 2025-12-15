using UnityEngine;
using JUTPS;

public class PlayerSystemBridge : MonoBehaviour
{
    [Header("JUTPS References")]
    [SerializeField] private JUHealth jutpsHealth;
    [SerializeField] private JUCharacterController jutpsController;
    
    [Header("Division Systems")]
    [SerializeField] private int playerLevel = 1;
    
    [Header("Combat Rewards")]
    [SerializeField] private int xpPerKill = 50;
    [SerializeField] private float lootDropChance = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private GameManager gameManager;
    private HUDManager hudManager;
    private float currentHealth;
    private float maxHealth;
    
    private void Start()
    {
        InitializeSystems();
        SetupHealthCallbacks();
    }
    
    private void InitializeSystems()
    {
        if (jutpsHealth == null)
        {
            jutpsHealth = GetComponent<JUHealth>();
        }
        
        if (jutpsController == null)
        {
            jutpsController = GetComponent<JUCharacterController>();
        }
        
        gameManager = GameManager.Instance;
        
        if (gameManager != null)
        {
            hudManager = gameManager.hudManager;
            
            if (gameManager.progressionManager != null)
            {
                playerLevel = gameManager.progressionManager.currentLevel;
            }
        }
        else
        {
            Debug.LogWarning("PlayerSystemBridge: GameManager not found. Some features will be disabled.");
        }
        
        if (jutpsHealth != null)
        {
            currentHealth = jutpsHealth.Health;
            maxHealth = jutpsHealth.MaxHealth;
            UpdateHealthUI();
        }
    }
    
    private void SetupHealthCallbacks()
    {
        if (jutpsHealth != null)
        {
            jutpsHealth.OnDamaged.AddListener(OnPlayerDamaged);
            jutpsHealth.OnDeath.AddListener(OnPlayerDeath);
        }
    }
    
    private void Update()
    {
        if (jutpsHealth != null && jutpsHealth.Health != currentHealth)
        {
            currentHealth = jutpsHealth.Health;
            UpdateHealthUI();
        }
        
        if (gameManager != null && gameManager.progressionManager != null)
        {
            playerLevel = gameManager.progressionManager.currentLevel;
        }
    }
    
    private void OnPlayerDamaged(JUHealth.DamageInfo damageInfo)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Player took {damageInfo.Damage} damage. Health: {jutpsHealth.Health}/{jutpsHealth.MaxHealth}");
        }
        
        UpdateHealthUI();
    }
    
    private void OnPlayerDeath()
    {
        if (showDebugLogs)
        {
            Debug.Log("Player died!");
        }
        
        UpdateHealthUI();
    }
    
    private void UpdateHealthUI()
    {
        if (hudManager != null && jutpsHealth != null)
        {
            hudManager.UpdateHealthDisplay(jutpsHealth.Health, jutpsHealth.MaxHealth);
        }
    }
    
    public void OnEnemyKilled(GameObject enemy)
    {
        if (gameManager == null) return;
        
        if (gameManager.progressionManager != null)
        {
            gameManager.progressionManager.AddExperience(xpPerKill);
            
            if (showDebugLogs)
            {
                Debug.Log($"Enemy killed! Gained {xpPerKill} XP");
            }
        }
        
        if (gameManager.lootManager != null && Random.value <= lootDropChance)
        {
            Vector3 lootPosition = enemy.transform.position;
            gameManager.lootManager.DropLoot(lootPosition, playerLevel);
            
            if (showDebugLogs)
            {
                Debug.Log($"Loot dropped at {lootPosition}");
            }
        }
    }
    
    public void Heal(float amount)
    {
        if (jutpsHealth != null)
        {
            jutpsHealth.Health = Mathf.Min(jutpsHealth.Health + amount, jutpsHealth.MaxHealth);
            UpdateHealthUI();
            
            if (showDebugLogs)
            {
                Debug.Log($"Healed {amount} HP. Current health: {jutpsHealth.Health}");
            }
        }
    }
    
    public void HealToFull()
    {
        if (jutpsHealth != null)
        {
            jutpsHealth.Health = jutpsHealth.MaxHealth;
            UpdateHealthUI();
            
            if (showDebugLogs)
            {
                Debug.Log("Fully healed!");
            }
        }
    }
    
    public float GetHealthPercentage()
    {
        if (jutpsHealth != null)
        {
            return jutpsHealth.Health / jutpsHealth.MaxHealth;
        }
        return 0f;
    }
    
    public bool IsAlive()
    {
        if (jutpsHealth != null)
        {
            return !jutpsHealth.IsDead;
        }
        return true;
    }
    
    public int GetPlayerLevel()
    {
        return playerLevel;
    }
    
    public void GainExperience(int amount)
    {
        if (gameManager != null && gameManager.progressionManager != null)
        {
            gameManager.progressionManager.AddExperience(amount);
        }
    }
}
