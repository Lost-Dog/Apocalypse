using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTemperatureDisplay : MonoBehaviour
{
    [Header("References")]
    public SurvivalManager survivalManager;
    public TextMeshProUGUI temperatureText;
    public Slider temperatureSlider;
    
    [Header("Display Settings")]
    [Tooltip("Always show as percentage (temperature is now 0-100%)")]
    public bool showAsPercentage = true;
    public bool showStatus = true;
    public bool showPrefix = false;
    public string prefix = "Temp: ";
    public string suffix = "%";
    
    [Header("Auto-Find")]
    public bool autoFindReferences = true;
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        InitializeSlider();
        UpdateDisplay();
    }
    
    private void FindReferences()
    {
        if (survivalManager == null)
        {
            survivalManager = FindFirstObjectByType<SurvivalManager>();
            if (survivalManager == null)
            {
                Debug.LogWarning("PlayerTemperatureDisplay: Could not find SurvivalManager in scene!");
            }
        }
        
        if (temperatureText == null)
        {
            temperatureText = GetComponent<TextMeshProUGUI>();
        }
        
        if (temperatureSlider == null)
        {
            temperatureSlider = GetComponent<Slider>();
        }
    }
    
    private void InitializeSlider()
    {
        if (temperatureSlider != null && survivalManager != null)
        {
            temperatureSlider.minValue = 0f;
            temperatureSlider.maxValue = survivalManager.maxTemperature;
            temperatureSlider.value = survivalManager.currentTemperature;
        }
    }
    
    private void Update()
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (survivalManager == null) return;
        
        if (temperatureText != null)
        {
            UpdateTextDisplay();
        }
        
        if (temperatureSlider != null)
        {
            temperatureSlider.maxValue = survivalManager.maxTemperature;
            temperatureSlider.value = survivalManager.currentTemperature;
        }
    }
    
    private void UpdateTextDisplay()
    {
        string displayText = "";
        
        if (showAsPercentage)
        {
            displayText = $"{Mathf.RoundToInt(survivalManager.currentTemperature)}{suffix}";
        }
        else
        {
            displayText = $"{survivalManager.currentTemperature:F1}";
        }
        
        if (showStatus)
        {
            string status = GetTemperatureStatus();
            displayText += $" ({status})";
        }
        
        if (showPrefix)
        {
            temperatureText.text = $"{prefix}{displayText}";
        }
        else
        {
            temperatureText.text = displayText;
        }
    }
    
    private string GetTemperatureStatus()
    {
        float temp = survivalManager.currentTemperature;
        
        if (temp >= 95f) return "Warm";
        if (temp >= 80f) return "Normal";
        if (temp >= 60f) return "Cool";
        if (temp >= 40f) return "Cold";
        if (temp >= 20f) return "Very Cold";
        if (temp >= 10f) return "Freezing";
        return "Hypothermia";
    }
}
