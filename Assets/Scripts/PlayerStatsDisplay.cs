using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("Manager References")]
    public SurvivalManager survivalManager;
    public ProgressionManager progressionManager;
    
    [Header("UI Text Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI infectionText;
    
    [Header("UI Slider Elements (Optional)")]
    public Slider healthSlider;
    public Slider xpSlider;
    public Slider temperatureSlider;
    public Slider staminaSlider;
    public Slider infectionSlider;
    
    [Header("Display Settings")]
    public bool showTemperaturePrefix = false;
    public bool showStaminaPrefix = false;
    public bool showInfectionPrefix = false;
    
    [Header("Auto-Find References")]
    public bool autoFindReferences = true;
    
    private JUHealth playerHealth;
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        InitializeSliders();
    }
    
    private void FindReferences()
    {
        if (survivalManager == null)
        {
            survivalManager = SurvivalManager.Instance;
            if (survivalManager == null)
            {
                survivalManager = FindFirstObjectByType<SurvivalManager>();
            }
        }
        
        if (survivalManager != null && playerHealth == null)
        {
            playerHealth = survivalManager.playerHealth;
        }
        
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<JUHealth>();
            }
        }
        
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<ProgressionManager>();
        }
        
        if (survivalManager == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: Could not find SurvivalManager!");
        }
        
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: Could not find player health component!");
        }
        
        if (progressionManager == null)
        {
            Debug.LogWarning("PlayerStatsDisplay: Could not find ProgressionManager!");
        }
    }
    
    private void InitializeSliders()
    {
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.Health;
        }
        
        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
        }
        
        if (survivalManager != null)
        {
            if (temperatureSlider != null)
            {
                temperatureSlider.minValue = 0f;
                temperatureSlider.maxValue = survivalManager.maxTemperature;
                temperatureSlider.value = survivalManager.currentTemperature;
            }
            
            if (staminaSlider != null)
            {
                staminaSlider.minValue = 0f;
                staminaSlider.maxValue = survivalManager.maxStamina;
                staminaSlider.value = survivalManager.currentStamina;
            }
            
            if (infectionSlider != null)
            {
                infectionSlider.minValue = 0f;
                infectionSlider.maxValue = survivalManager.maxInfection;
                infectionSlider.value = survivalManager.currentInfection;
            }
        }
    }
    
    private void Update()
    {
        UpdateHealthDisplay();
        UpdateXPDisplay();
        UpdateTemperatureDisplay();
        UpdateStaminaDisplay();
        UpdateInfectionDisplay();
    }
    
    private void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(playerHealth.Health)}/{Mathf.RoundToInt(playerHealth.MaxHealth)}";
        }
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.Health;
        }
    }
    
    private void UpdateXPDisplay()
    {
        if (progressionManager == null) return;
        
        if (levelText != null)
        {
            levelText.text = $"{progressionManager.currentLevel}";
        }
        
        if (xpText != null)
        {
            xpText.text = $"{progressionManager.currentXP}";
        }
        
        if (xpSlider != null)
        {
            xpSlider.value = progressionManager.GetXPProgress();
        }
    }
    
    private void UpdateTemperatureDisplay()
    {
        if (survivalManager == null) return;
        
        if (temperatureText != null)
        {
            string status = survivalManager.GetTemperatureStatus();
            string display = showTemperaturePrefix ? $"Temp: {survivalManager.currentTemperature:F1}°C ({status})" : $"{survivalManager.currentTemperature:F1}°C ({status})";
            temperatureText.text = display;
        }
        
        if (temperatureSlider != null)
        {
            temperatureSlider.value = survivalManager.currentTemperature;
        }
    }
    
    private void UpdateStaminaDisplay()
    {
        if (survivalManager == null) return;
        
        if (staminaText != null)
        {
            string display = showStaminaPrefix ? $"Stamina: {Mathf.RoundToInt(survivalManager.currentStamina)}/{Mathf.RoundToInt(survivalManager.maxStamina)}" : $"{Mathf.RoundToInt(survivalManager.currentStamina)}/{Mathf.RoundToInt(survivalManager.maxStamina)}";
            staminaText.text = display;
        }
        
        if (staminaSlider != null)
        {
            staminaSlider.value = survivalManager.currentStamina;
        }
    }
    
    private void UpdateInfectionDisplay()
    {
        if (survivalManager == null) return;
        
        if (infectionText != null)
        {
            string status = survivalManager.GetInfectionStatus();
            string display = showInfectionPrefix ? $"Infection: {Mathf.RoundToInt(survivalManager.currentInfection)}% ({status})" : $"{Mathf.RoundToInt(survivalManager.currentInfection)}% ({status})";
            infectionText.text = display;
        }
        
        if (infectionSlider != null)
        {
            infectionSlider.value = survivalManager.currentInfection;
        }
    }
}
