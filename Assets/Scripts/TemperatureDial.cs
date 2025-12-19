using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a radial dial (Image with Fill type = Radial) to display player temperature.
/// Designed for use with Stat_Dial_04 or similar dial UI elements.
/// </summary>
public class TemperatureDial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SurvivalManager that tracks temperature")]
    public SurvivalManager survivalManager;
    
    [Tooltip("The Image component with Image Type = Filled (Radial)")]
    public Image dialFillImage;
    
    [Tooltip("Optional text to display temperature value")]
    public TMP_Text temperatureText;
    
    [Header("Dial Settings")]
    [Tooltip("Maximum fill amount (1.0 = full circle)")]
    [Range(0f, 1f)]
    public float maxFillAmount = 1f;
    
    [Tooltip("Minimum fill amount (empty dial)")]
    [Range(0f, 1f)]
    public float minFillAmount = 0f;
    
    [Tooltip("Smooth transition speed for fill changes")]
    public float fillTransitionSpeed = 2f;
    
    [Tooltip("Enable smooth interpolation")]
    public bool smoothFill = true;
    
    [Header("Color Settings")]
    [Tooltip("Color when temperature is normal/high (warm)")]
    public Color warmColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("Color when temperature is low (cold)")]
    public Color coldColor = new Color(0f, 0.5f, 1f, 1f); // Blue
    
    [Tooltip("Color when temperature is critical (freezing)")]
    public Color criticalColor = new Color(0.3f, 0.3f, 1f, 1f); // Dark blue
    
    [Tooltip("Enable color gradient based on temperature")]
    public bool enableColorGradient = true;
    
    [Header("Text Display")]
    [Tooltip("Show temperature value on text")]
    public bool showTemperatureValue = true;
    
    [Tooltip("Show temperature as percentage (0-100%)")]
    public bool showAsPercentage = false;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 1;
    
    [Tooltip("Text format (use {0} for value)")]
    public string textFormat = "{0}°C";
    
    [Header("Temperature Thresholds")]
    [Tooltip("Temperature at which dial shows critical color")]
    public float criticalThreshold = 10f;
    
    [Tooltip("Temperature at which dial shows cold color")]
    public float coldThreshold = 20f;
    
    [Tooltip("Normal/optimal temperature (100%)")]
    public float normalTemperature = 100f;
    
    [Header("Auto-Find")]
    [Tooltip("Automatically find required components")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private float targetFillAmount;
    private float currentFillAmount;
    private bool isInitialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        if (autoFindComponents)
        {
            FindComponents();
        }
        
        // Clear text immediately to prevent showing default value
        if (temperatureText != null)
        {
            temperatureText.text = "--";
            if (showDebugInfo)
            {
                Debug.Log($"[TemperatureDial] Cleared text to '--' during initialization");
            }
        }
        
        ValidateComponents();
        SetupDialImage();
        
        if (survivalManager != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[TemperatureDial] Initialize - currentTemp: {survivalManager.currentTemperature}, normalTemp: {normalTemperature}, showAsPercentage: {showAsPercentage}");
            }
            
            // Set initial fill to current temperature
            float initialFill = CalculateFillAmount(survivalManager.currentTemperature);
            currentFillAmount = initialFill;
            targetFillAmount = initialFill;
            
            if (dialFillImage != null)
            {
                dialFillImage.fillAmount = initialFill;
            }
            
            // Update text display immediately to prevent showing wrong value
            UpdateTextDisplay();
            UpdateDialColor();
            
            if (showDebugInfo && temperatureText != null)
            {
                Debug.Log($"[TemperatureDial] After UpdateTextDisplay - text: '{temperatureText.text}'");
            }
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[TemperatureDial] Initialized on '{gameObject.name}'");
        }
    }
    
    private void FindComponents()
    {
        // Find SurvivalManager
        if (survivalManager == null)
        {
            survivalManager = SurvivalManager.Instance;
            if (survivalManager == null)
            {
                survivalManager = FindFirstObjectByType<SurvivalManager>();
            }
        }
        
        // Find dial fill image
        if (dialFillImage == null)
        {
            // Try to find Image component on this GameObject
            dialFillImage = GetComponent<Image>();
            
            // If not found, try to find in children with specific names
            if (dialFillImage == null)
            {
                Transform fillTransform = transform.Find("Fill");
                if (fillTransform == null) fillTransform = transform.Find("Dial_Fill");
                if (fillTransform == null) fillTransform = transform.Find("IMG_Fill");
                
                if (fillTransform != null)
                {
                    dialFillImage = fillTransform.GetComponent<Image>();
                }
            }
            
            // Last resort: find any Image with Fill type in children
            if (dialFillImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.type == Image.Type.Filled)
                    {
                        dialFillImage = img;
                        break;
                    }
                }
            }
        }
        
        // Find temperature text
        if (temperatureText == null)
        {
            temperatureText = GetComponentInChildren<TMP_Text>();
        }
    }
    
    private void ValidateComponents()
    {
        if (survivalManager == null)
        {
            Debug.LogError($"[TemperatureDial] SurvivalManager not found! Please assign it in the inspector or ensure one exists in the scene.");
        }
        
        if (dialFillImage == null)
        {
            Debug.LogError($"[TemperatureDial] Dial Fill Image not found on '{gameObject.name}'! Please assign an Image component with Fill type.");
        }
        else if (dialFillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[TemperatureDial] Image '{dialFillImage.name}' is not set to Fill type. Changing to Filled (Radial 360).");
            dialFillImage.type = Image.Type.Filled;
            dialFillImage.fillMethod = Image.FillMethod.Radial360;
        }
    }
    
    private void SetupDialImage()
    {
        if (dialFillImage == null) return;
        
        // Ensure the image is set up correctly for radial fill
        dialFillImage.type = Image.Type.Filled;
        dialFillImage.fillMethod = Image.FillMethod.Radial360;
        dialFillImage.fillOrigin = (int)Image.Origin360.Top; // Start from top
        dialFillImage.fillClockwise = true;
        
        // Reset fill to 0 to prevent flash of full dial on start
        dialFillImage.fillAmount = 0f;
    }
    
    private void Update()
    {
        if (!isInitialized || survivalManager == null || dialFillImage == null)
            return;
        
        UpdateDialFill();
        UpdateDialColor();
        UpdateTextDisplay();
    }
    
    private void UpdateDialFill()
    {
        // Calculate target fill based on current temperature
        targetFillAmount = CalculateFillAmount(survivalManager.currentTemperature);
        
        // Smooth interpolation or instant update
        if (smoothFill)
        {
            currentFillAmount = Mathf.MoveTowards(currentFillAmount, targetFillAmount, fillTransitionSpeed * Time.deltaTime);
        }
        else
        {
            currentFillAmount = targetFillAmount;
        }
        
        // Apply fill amount to dial
        dialFillImage.fillAmount = currentFillAmount;
    }
    
    private float CalculateFillAmount(float temperature)
    {
        // Normalize temperature to 0-1 range based on normal temperature (37°C)
        // 0°C = 0% fill, 37°C = 100% fill
        float normalizedTemp = temperature / normalTemperature;
        
        // Map to fill amount range
        float fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, normalizedTemp);
        
        // Clamp to valid range (can go above 100% if temperature exceeds normal)
        return Mathf.Clamp01(fillAmount);
    }
    
    private void UpdateDialColor()
    {
        if (!enableColorGradient || dialFillImage == null)
            return;
        
        float temp = survivalManager.currentTemperature;
        Color targetColor;
        
        // Determine color based on temperature thresholds
        if (temp <= criticalThreshold)
        {
            // Critical cold - use critical color
            targetColor = criticalColor;
        }
        else if (temp <= coldThreshold)
        {
            // Cold - blend between critical and cold color
            float t = Mathf.InverseLerp(criticalThreshold, coldThreshold, temp);
            targetColor = Color.Lerp(criticalColor, coldColor, t);
        }
        else
        {
            // Normal to warm - blend between cold and warm color
            float t = Mathf.InverseLerp(coldThreshold, normalTemperature, temp);
            targetColor = Color.Lerp(coldColor, warmColor, t);
        }
        
        dialFillImage.color = targetColor;
    }
    
    private void UpdateTextDisplay()
    {
        if (!showTemperatureValue || temperatureText == null || survivalManager == null)
            return;
        
        float displayValue;
        
        if (showAsPercentage)
        {
            displayValue = (survivalManager.currentTemperature / normalTemperature) * 100f;
        }
        else
        {
            displayValue = survivalManager.currentTemperature;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[TemperatureDial] UpdateTextDisplay - currentTemp: {survivalManager.currentTemperature}, normalTemp: {normalTemperature}, showAsPercentage: {showAsPercentage}, displayValue: {displayValue}");
        }
        
        // Format with specified decimal places
        string valueStr = displayValue.ToString($"F{decimalPlaces}");
        
        // Apply text format
        temperatureText.text = string.Format(textFormat, valueStr);
    }
    
    // Public API
    
    /// <summary>
    /// Set custom fill amount manually (overrides automatic temperature tracking)
    /// </summary>
    public void SetFillAmount(float fillAmount)
    {
        targetFillAmount = Mathf.Clamp01(fillAmount);
        
        if (!smoothFill && dialFillImage != null)
        {
            dialFillImage.fillAmount = targetFillAmount;
            currentFillAmount = targetFillAmount;
        }
    }
    
    /// <summary>
    /// Set custom color manually
    /// </summary>
    public void SetDialColor(Color color)
    {
        if (dialFillImage != null)
        {
            dialFillImage.color = color;
        }
    }
    
    /// <summary>
    /// Force immediate update without smooth transition
    /// </summary>
    public void ForceUpdate()
    {
        if (survivalManager != null && dialFillImage != null)
        {
            float fillAmount = CalculateFillAmount(survivalManager.currentTemperature);
            dialFillImage.fillAmount = fillAmount;
            currentFillAmount = fillAmount;
            targetFillAmount = fillAmount;
            
            UpdateDialColor();
            UpdateTextDisplay();
        }
    }
    
    /// <summary>
    /// Get current fill percentage (0-100)
    /// </summary>
    public float GetFillPercentage()
    {
        return currentFillAmount * 100f;
    }
    
    /// <summary>
    /// Check if temperature is in critical range
    /// </summary>
    public bool IsCritical()
    {
        return survivalManager != null && survivalManager.currentTemperature <= criticalThreshold;
    }
    
    private void OnValidate()
    {
        // Ensure min is less than max
        if (minFillAmount > maxFillAmount)
        {
            minFillAmount = maxFillAmount;
        }
        
        // Validate thresholds
        if (criticalThreshold > coldThreshold)
        {
            criticalThreshold = coldThreshold;
        }
    }
}
