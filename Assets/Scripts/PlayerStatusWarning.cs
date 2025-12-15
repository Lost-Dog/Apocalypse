using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerStatusWarning : MonoBehaviour
{
    [Header("Warning UI")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;
    public Image warningIcon;
    
    [Header("Auto-Find References")]
    public bool autoFindReferences = true;
    public JUHealth playerHealth;
    public SurvivalManager survivalManager;
    public PlayerInfectionDisplay infectionDisplay;
    
    [Header("Threshold Settings")]
    [Range(0f, 1f)] public float healthLowThreshold = 0.3f;
    [Range(0f, 1f)] public float healthCriticalThreshold = 0.15f;
    [Range(0f, 1f)] public float temperatureLowThreshold = 0.4f;
    [Range(0f, 1f)] public float temperatureCriticalThreshold = 0.2f;
    public float infectionLowThreshold = 50f;
    public float infectionCriticalThreshold = 75f;
    
    [Header("Warning Messages")]
    public string healthLowMessage = "LOW HEALTH";
    public string healthCriticalMessage = "CRITICAL HEALTH";
    public string temperatureLowMessage = "GETTING COLD";
    public string temperatureCriticalMessage = "FREEZING";
    public string infectionLowMessage = "INFECTED";
    public string infectionCriticalMessage = "SEVERE INFECTION";
    
    [Header("Display Settings")]
    public float warningDisplayDuration = 3f;
    public float warningCooldown = 5f;
    public Color lowWarningColor = new Color(1f, 0.8f, 0f, 1f);
    public Color criticalWarningColor = new Color(1f, 0f, 0f, 1f);
    
    [Header("Flashing Effect")]
    public bool enableFlashing = true;
    public float flashSpeed = 2f;
    
    private float warningTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isShowingWarning = false;
    private string currentWarning = "";
    private Color currentWarningColor = Color.white;
    
    private bool wasHealthLow = false;
    private bool wasHealthCritical = false;
    private bool wasTemperatureLow = false;
    private bool wasTemperatureCritical = false;
    private bool wasInfectionLow = false;
    private bool wasInfectionCritical = false;
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
    
    private void FindReferences()
    {
        if (warningPanel == null)
        {
            warningPanel = gameObject;
        }
        
        if (warningText == null)
        {
            warningText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (warningIcon == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            if (images.Length > 0)
            {
                warningIcon = images[0];
            }
        }
        
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<JUHealth>();
            }
        }
        
        if (survivalManager == null)
        {
            survivalManager = FindFirstObjectByType<SurvivalManager>();
        }
        
        if (infectionDisplay == null)
        {
            infectionDisplay = FindFirstObjectByType<PlayerInfectionDisplay>();
        }
    }
    
    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        if (isShowingWarning)
        {
            UpdateWarningDisplay();
        }
        else
        {
            CheckForWarnings();
        }
    }
    
    private void CheckForWarnings()
    {
        if (cooldownTimer > 0f) return;
        
        bool healthLow = false;
        bool healthCritical = false;
        bool temperatureLow = false;
        bool temperatureCritical = false;
        bool infectionLow = false;
        bool infectionCritical = false;
        
        if (playerHealth != null)
        {
            float healthPercentage = playerHealth.Health / playerHealth.MaxHealth;
            healthCritical = healthPercentage <= healthCriticalThreshold;
            healthLow = !healthCritical && healthPercentage <= healthLowThreshold;
        }
        
        if (survivalManager != null && survivalManager.enableTemperatureSystem)
        {
            float tempPercentage = survivalManager.TemperaturePercentage;
            temperatureCritical = tempPercentage <= temperatureCriticalThreshold;
            temperatureLow = !temperatureCritical && tempPercentage <= temperatureLowThreshold;
        }
        
        if (infectionDisplay != null)
        {
            float infection = infectionDisplay.currentInfection;
            infectionCritical = infection >= infectionCriticalThreshold;
            infectionLow = !infectionCritical && infection >= infectionLowThreshold;
        }
        
        if (healthCritical && !wasHealthCritical)
        {
            ShowWarning(healthCriticalMessage, criticalWarningColor);
        }
        else if (healthLow && !wasHealthLow)
        {
            ShowWarning(healthLowMessage, lowWarningColor);
        }
        else if (temperatureCritical && !wasTemperatureCritical)
        {
            ShowWarning(temperatureCriticalMessage, criticalWarningColor);
        }
        else if (temperatureLow && !wasTemperatureLow)
        {
            ShowWarning(temperatureLowMessage, lowWarningColor);
        }
        else if (infectionCritical && !wasInfectionCritical)
        {
            ShowWarning(infectionCriticalMessage, criticalWarningColor);
        }
        else if (infectionLow && !wasInfectionLow)
        {
            ShowWarning(infectionLowMessage, lowWarningColor);
        }
        
        wasHealthLow = healthLow;
        wasHealthCritical = healthCritical;
        wasTemperatureLow = temperatureLow;
        wasTemperatureCritical = temperatureCritical;
        wasInfectionLow = infectionLow;
        wasInfectionCritical = infectionCritical;
    }
    
    private void ShowWarning(string message, Color color)
    {
        currentWarning = message;
        currentWarningColor = color;
        isShowingWarning = true;
        warningTimer = warningDisplayDuration;
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
        
        if (warningText != null)
        {
            warningText.text = message;
            warningText.color = color;
        }
        
        if (warningIcon != null)
        {
            warningIcon.color = color;
        }
    }
    
    private void UpdateWarningDisplay()
    {
        warningTimer -= Time.deltaTime;
        
        if (warningTimer <= 0f)
        {
            HideWarning();
            return;
        }
        
        if (enableFlashing && warningText != null)
        {
            float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * flashSpeed * Mathf.PI);
            Color flashColor = currentWarningColor;
            flashColor.a = alpha;
            warningText.color = flashColor;
            
            if (warningIcon != null)
            {
                warningIcon.color = flashColor;
            }
        }
    }
    
    private void HideWarning()
    {
        isShowingWarning = false;
        cooldownTimer = warningCooldown;
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
    
    public void ForceShowWarning(string message, Color color, float duration = 3f)
    {
        warningDisplayDuration = duration;
        ShowWarning(message, color);
    }
    
    public void ClearWarning()
    {
        HideWarning();
    }
}
