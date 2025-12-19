using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a radial dial (Image with Fill type = Radial) to display player stamina level.
/// Designed for use with Stat_Dial_01 or similar dial UI elements.
/// </summary>
public class StaminaDial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SurvivalManager that tracks stamina")]
    public SurvivalManager survivalManager;
    
    [Tooltip("The Image component with Image Type = Filled (Radial)")]
    public Image dialFillImage;
    
    [Tooltip("Optional text to display stamina value")]
    public TMP_Text staminaText;
    
    [Header("Dial Settings")]
    [Tooltip("Maximum fill amount (1.0 = full circle)")]
    [Range(0f, 1f)]
    public float maxFillAmount = 1f;
    
    [Tooltip("Minimum fill amount (empty dial)")]
    [Range(0f, 1f)]
    public float minFillAmount = 0f;
    
    [Tooltip("Smooth transition speed for fill changes")]
    public float fillTransitionSpeed = 3f;
    
    [Tooltip("Enable smooth interpolation")]
    public bool smoothFill = true;
    
    [Header("Color Settings")]
    [Tooltip("Color when stamina is full/high (healthy)")]
    public Color fullColor = new Color(0f, 1f, 0.2f, 1f); // Bright Green
    
    [Tooltip("Color when stamina is moderate (normal)")]
    public Color normalColor = new Color(0.5f, 1f, 0f, 1f); // Yellow-Green
    
    [Tooltip("Color when stamina is low (warning)")]
    public Color lowColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    
    [Tooltip("Color when stamina is very low (critical)")]
    public Color criticalColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("Color when stamina is depleted (exhausted)")]
    public Color depletedColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Tooltip("Enable color gradient based on stamina level")]
    public bool enableColorGradient = true;
    
    [Header("Text Display")]
    [Tooltip("Show stamina value on text")]
    public bool showStaminaValue = true;
    
    [Tooltip("Show stamina as percentage")]
    public bool showAsPercentage = true;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
    
    [Tooltip("Text format (use {0} for value)")]
    public string textFormat = "{0}%";
    
    [Header("Stamina Thresholds")]
    [Tooltip("Stamina level below which shows low color")]
    public float lowThreshold = 50f;
    
    [Tooltip("Stamina level below which shows critical color")]
    public float criticalThreshold = 25f;
    
    [Tooltip("Stamina level below which shows depleted color")]
    public float depletedThreshold = 10f;
    
    [Header("Visual Effects")]
    [Tooltip("Pulse effect when stamina is critical")]
    public bool pulseOnCritical = true;
    
    [Tooltip("Pulse speed multiplier")]
    public float pulseSpeed = 2f;
    
    [Tooltip("Pulse intensity (0-1)")]
    [Range(0f, 1f)]
    public float pulseIntensity = 0.3f;
    
    [Header("Auto-Find")]
    [Tooltip("Automatically find required components")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private float targetFillAmount;
    private float currentFillAmount;
    private bool isInitialized = false;
    private Color baseColor;
    
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
        
        ValidateComponents();
        SetupDialImage();
        
        if (survivalManager != null)
        {
            // Set initial fill to current stamina
            float initialFill = CalculateFillAmount(survivalManager.currentStamina);
            currentFillAmount = initialFill;
            targetFillAmount = initialFill;
            
            if (dialFillImage != null)
            {
                dialFillImage.fillAmount = initialFill;
            }
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[StaminaDial] Initialized on '{gameObject.name}'");
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
        
        // Find stamina text
        if (staminaText == null)
        {
            staminaText = GetComponentInChildren<TMP_Text>();
        }
    }
    
    private void ValidateComponents()
    {
        if (survivalManager == null)
        {
            Debug.LogError($"[StaminaDial] SurvivalManager not found! Please assign it in the inspector or ensure one exists in the scene.");
        }
        
        if (dialFillImage == null)
        {
            Debug.LogError($"[StaminaDial] Dial Fill Image not found on '{gameObject.name}'! Please assign an Image component with Fill type.");
        }
        else if (dialFillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[StaminaDial] Image '{dialFillImage.name}' is not set to Fill type. Changing to Filled (Radial 360).");
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
        // Calculate target fill based on current stamina
        targetFillAmount = CalculateFillAmount(survivalManager.currentStamina);
        
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
    
    private float CalculateFillAmount(float stamina)
    {
        // Normalize stamina to 0-1 range
        float normalizedStamina = stamina / survivalManager.maxStamina;
        
        // Map to fill amount range
        float fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, normalizedStamina);
        
        // Clamp to valid range
        return Mathf.Clamp01(fillAmount);
    }
    
    private void UpdateDialColor()
    {
        if (!enableColorGradient || dialFillImage == null)
            return;
        
        float stamina = survivalManager.currentStamina;
        Color targetColor;
        
        // Determine color based on stamina thresholds
        if (stamina >= lowThreshold)
        {
            // High stamina - blend between normal and full
            float t = Mathf.InverseLerp(lowThreshold, survivalManager.maxStamina, stamina);
            targetColor = Color.Lerp(normalColor, fullColor, t);
        }
        else if (stamina >= criticalThreshold)
        {
            // Moderate stamina - blend between low and normal
            float t = Mathf.InverseLerp(criticalThreshold, lowThreshold, stamina);
            targetColor = Color.Lerp(lowColor, normalColor, t);
        }
        else if (stamina >= depletedThreshold)
        {
            // Low stamina - blend between critical and low
            float t = Mathf.InverseLerp(depletedThreshold, criticalThreshold, stamina);
            targetColor = Color.Lerp(criticalColor, lowColor, t);
        }
        else
        {
            // Very low/depleted stamina - blend between depleted and critical
            float t = Mathf.InverseLerp(0f, depletedThreshold, stamina);
            targetColor = Color.Lerp(depletedColor, criticalColor, t);
        }
        
        // Store base color for pulse effect
        baseColor = targetColor;
        
        // Apply pulse effect if critical
        if (pulseOnCritical && stamina <= criticalThreshold)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            Color pulseColor = Color.Lerp(targetColor, Color.white, pulse * pulseIntensity);
            dialFillImage.color = pulseColor;
        }
        else
        {
            dialFillImage.color = targetColor;
        }
    }
    
    private void UpdateTextDisplay()
    {
        if (!showStaminaValue || staminaText == null || survivalManager == null)
            return;
        
        float displayValue;
        
        if (showAsPercentage)
        {
            displayValue = (survivalManager.currentStamina / survivalManager.maxStamina) * 100f;
        }
        else
        {
            displayValue = survivalManager.currentStamina;
        }
        
        // Format with specified decimal places
        string valueStr = displayValue.ToString($"F{decimalPlaces}");
        
        // Apply text format
        staminaText.text = string.Format(textFormat, valueStr);
    }
    
    // Public API
    
    /// <summary>
    /// Set custom fill amount manually (overrides automatic stamina tracking)
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
            float fillAmount = CalculateFillAmount(survivalManager.currentStamina);
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
    /// Get stamina status as string
    /// </summary>
    public string GetStaminaStatus()
    {
        if (survivalManager == null) return "Unknown";
        
        float stamina = survivalManager.currentStamina;
        
        if (stamina >= lowThreshold) return "Full";
        if (stamina >= criticalThreshold) return "Normal";
        if (stamina >= depletedThreshold) return "Low";
        if (stamina > 0f) return "Critical";
        return "Exhausted";
    }
    
    /// <summary>
    /// Check if stamina is depleted
    /// </summary>
    public bool IsDepleted()
    {
        return survivalManager != null && survivalManager.currentStamina <= depletedThreshold;
    }
    
    /// <summary>
    /// Check if stamina is in critical range
    /// </summary>
    public bool IsCritical()
    {
        return survivalManager != null && survivalManager.currentStamina <= criticalThreshold;
    }
    
    /// <summary>
    /// Check if stamina is in low range
    /// </summary>
    public bool IsLow()
    {
        return survivalManager != null && survivalManager.currentStamina <= lowThreshold;
    }
    
    /// <summary>
    /// Check if stamina is full
    /// </summary>
    public bool IsFull()
    {
        return survivalManager != null && survivalManager.currentStamina >= survivalManager.maxStamina;
    }
    
    private void OnValidate()
    {
        // Ensure min is less than max
        if (minFillAmount > maxFillAmount)
        {
            minFillAmount = maxFillAmount;
        }
        
        // Validate thresholds are in descending order (high to low)
        if (criticalThreshold > lowThreshold)
        {
            criticalThreshold = lowThreshold;
        }
        
        if (depletedThreshold > criticalThreshold)
        {
            depletedThreshold = criticalThreshold;
        }
        
        // Clamp thresholds to valid stamina range
        if (survivalManager != null)
        {
            lowThreshold = Mathf.Clamp(lowThreshold, 0f, survivalManager.maxStamina);
            criticalThreshold = Mathf.Clamp(criticalThreshold, 0f, lowThreshold);
            depletedThreshold = Mathf.Clamp(depletedThreshold, 0f, criticalThreshold);
        }
    }
}
