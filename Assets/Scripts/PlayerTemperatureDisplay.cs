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
    [Tooltip("Show temperature with decimal point")]
    public bool showDecimal = true;
    public bool showStatus = true;
    public bool showPrefix = false;
    public string prefix = "Temp: ";
    public string suffix = "°C";
    
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
            temperatureSlider.maxValue = 100f;
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
            temperatureSlider.maxValue = 100f;
            temperatureSlider.value = survivalManager.currentTemperature;
        }
    }
    
    private void UpdateTextDisplay()
    {
        string displayText = "";
        
        if (showDecimal)
        {
            displayText = $"{survivalManager.currentTemperature:F1}{suffix}";
        }
        else
        {
            displayText = $"{Mathf.RoundToInt(survivalManager.currentTemperature)}{suffix}";
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
        
        if (temp >= 35f) return "Normal";
        if (temp >= 30f) return "Cool";
        if (temp >= 20f) return "Cold";
        if (temp >= 15f) return "Very Cold";
        if (temp >= 5f) return "Freezing";
        return "Hypothermia";
    }
}
