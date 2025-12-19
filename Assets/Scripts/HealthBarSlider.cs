using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

/// <summary>
/// Controls a health bar slider to display player health.
/// Designed for use with standard UI Slider components.
/// </summary>
public class HealthBarSlider : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The JUHealth component that tracks player health")]
    public JUHealth playerHealth;
    
    [Tooltip("The Slider component for the health bar")]
    public Slider healthSlider;
    
    [Tooltip("The fill Image of the slider (for color changes)")]
    public Image fillImage;
    
    [Tooltip("Optional text to display health value")]
    public TMP_Text healthText;
    
    [Header("Slider Settings")]
    [Tooltip("Smooth transition speed for health changes")]
    public float fillTransitionSpeed = 6f;
    
    [Tooltip("Enable smooth interpolation")]
    public bool smoothFill = true;
    
    [Header("Color Settings")]
    [Tooltip("Color when health is full/high")]
    public Color fullHealthColor = new Color(0f, 1f, 0f, 1f); // Green
    
    [Tooltip("Color when health is moderate")]
    public Color moderateHealthColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    
    [Tooltip("Color when health is low")]
    public Color lowHealthColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("Color when health is critical")]
    public Color criticalHealthColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Tooltip("Flash color when taking damage")]
    public Color damageFlashColor = new Color(1f, 1f, 0f, 1f); // Yellow
    
    [Tooltip("Flash color when healing")]
    public Color healFlashColor = new Color(0f, 1f, 1f, 1f); // Cyan
    
    [Tooltip("Enable color gradient based on health level")]
    public bool enableColorGradient = true;
    
    [Tooltip("Enable flash effect on damage/healing")]
    public bool enableFlashEffect = true;
    
    [Tooltip("Duration of flash effect in seconds")]
    public float flashDuration = 0.3f;
    
    [Header("Text Display")]
    [Tooltip("Show health value on text")]
    public bool showHealthValue = true;
    
    [Tooltip("Text format (use {0} for current, {1} for max)")]
    public string textFormat = "{0}/{1}";
    
    [Tooltip("Show health as percentage")]
    public bool showAsPercentage = false;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
    
    [Header("Health Thresholds")]
    [Tooltip("Health percentage below which shows moderate color")]
    [Range(0f, 100f)]
    public float moderateThreshold = 66f;
    
    [Tooltip("Health percentage below which shows low color")]
    [Range(0f, 100f)]
    public float lowThreshold = 33f;
    
    [Tooltip("Health percentage below which shows critical color")]
    [Range(0f, 100f)]
    public float criticalThreshold = 15f;
    
    [Header("Visual Effects")]
    [Tooltip("Pulse effect when health is critical")]
    public bool pulseOnCritical = true;
    
    [Tooltip("Pulse speed multiplier")]
    public float pulseSpeed = 3f;
    
    [Tooltip("Pulse intensity (0-1)")]
    [Range(0f, 1f)]
    public float pulseIntensity = 0.4f;
    
    [Header("Auto-Find")]
    [Tooltip("Automatically find required components")]
    public bool autoFindComponents = true;
    
    [Tooltip("Auto-find player health component")]
    public bool autoFindPlayerHealth = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private float currentFillValue;
    private float targetFillValue;
    private bool isInitialized = false;
    private float flashTimer = 0f;
    private bool isFlashing = false;
    private Color flashColor;
    private float previousHealth;
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
        SetupSlider();
        
        if (playerHealth != null)
        {
            // Set initial value to current health
            float initialValue = playerHealth.Health / playerHealth.MaxHealth;
            currentFillValue = initialValue;
            targetFillValue = initialValue;
            previousHealth = playerHealth.Health;
            
            if (healthSlider != null)
            {
                healthSlider.value = initialValue;
            }
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[HealthBarSlider] Initialized on '{gameObject.name}'");
        }
    }
    
    private void FindComponents()
    {
        // Find player health
        if (playerHealth == null && autoFindPlayerHealth)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<JUHealth>();
            }
        }
        
        // Find slider component
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            
            // If not found on this object, try children
            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>();
            }
        }
        
        // Find fill image
        if (fillImage == null && healthSlider != null)
        {
            // Try to find fill area image
            Transform fillArea = healthSlider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                Transform fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    fillImage = fill.GetComponent<Image>();
                }
            }
            
            // Alternative: search for any Image in slider children
            if (fillImage == null)
            {
                Image[] images = healthSlider.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.name.Contains("Fill") || img.type == Image.Type.Filled)
                    {
                        fillImage = img;
                        break;
                    }
                }
            }
        }
        
        // Find health text
        if (healthText == null)
        {
            healthText = GetComponentInChildren<TMP_Text>();
        }
    }
    
    private void ValidateComponents()
    {
        if (playerHealth == null)
        {
            Debug.LogError($"[HealthBarSlider] Player Health (JUHealth) not found! Please assign it in the inspector or ensure player has JUHealth component.");
        }
        
        if (healthSlider == null)
        {
            Debug.LogError($"[HealthBarSlider] Health Slider not found on '{gameObject.name}'! Please assign a Slider component.");
        }
        
        if (fillImage == null && showDebugInfo)
        {
            Debug.LogWarning($"[HealthBarSlider] Fill Image not found. Color changes will not work.");
        }
    }
    
    private void SetupSlider()
    {
        if (healthSlider == null) return;
        
        // Configure slider
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.wholeNumbers = false;
        healthSlider.interactable = false;
    }
    
    private void Update()
    {
        if (!isInitialized || playerHealth == null || healthSlider == null)
            return;
        
        UpdateSliderValue();
        UpdateSliderColor();
        UpdateTextDisplay();
        UpdateFlashEffect();
    }
    
    private void UpdateSliderValue()
    {
        // Calculate target value based on current health
        targetFillValue = playerHealth.Health / playerHealth.MaxHealth;
        
        // Detect health change for flash effect
        if (enableFlashEffect && Mathf.Abs(playerHealth.Health - previousHealth) > 0.01f)
        {
            if (playerHealth.Health < previousHealth)
            {
                // Taking damage
                StartFlash(damageFlashColor);
            }
            else if (playerHealth.Health > previousHealth)
            {
                // Healing
                StartFlash(healFlashColor);
            }
            
            previousHealth = playerHealth.Health;
        }
        
        // Smooth interpolation or instant update
        if (smoothFill)
        {
            currentFillValue = Mathf.MoveTowards(currentFillValue, targetFillValue, fillTransitionSpeed * Time.deltaTime);
        }
        else
        {
            currentFillValue = targetFillValue;
        }
        
        // Apply value to slider
        healthSlider.value = currentFillValue;
    }
    
    private void UpdateSliderColor()
    {
        if (!enableColorGradient || fillImage == null)
            return;
        
        // Skip color update if flashing
        if (isFlashing)
            return;
        
        float healthPercent = currentFillValue * 100f;
        Color targetColor;
        
        // Determine color based on health percentage thresholds
        if (healthPercent >= moderateThreshold)
        {
            // Full to moderate health
            float t = Mathf.InverseLerp(moderateThreshold, 100f, healthPercent);
            targetColor = Color.Lerp(moderateHealthColor, fullHealthColor, t);
        }
        else if (healthPercent >= lowThreshold)
        {
            // Moderate to low health
            float t = Mathf.InverseLerp(lowThreshold, moderateThreshold, healthPercent);
            targetColor = Color.Lerp(lowHealthColor, moderateHealthColor, t);
        }
        else if (healthPercent >= criticalThreshold)
        {
            // Low to critical health
            float t = Mathf.InverseLerp(criticalThreshold, lowThreshold, healthPercent);
            targetColor = Color.Lerp(criticalHealthColor, lowHealthColor, t);
        }
        else
        {
            // Critical health
            targetColor = criticalHealthColor;
        }
        
        // Store base color for pulse effect
        baseColor = targetColor;
        
        // Apply pulse effect if critical
        if (pulseOnCritical && healthPercent <= criticalThreshold)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            Color pulseColor = Color.Lerp(targetColor, Color.white, pulse * pulseIntensity);
            fillImage.color = pulseColor;
        }
        else
        {
            fillImage.color = targetColor;
        }
    }
    
    private void UpdateFlashEffect()
    {
        if (!isFlashing || fillImage == null)
            return;
        
        flashTimer -= Time.deltaTime;
        
        if (flashTimer <= 0f)
        {
            // Flash complete
            isFlashing = false;
            // Color will be restored in next UpdateSliderColor call
        }
        else
        {
            // Interpolate from flash color back to base color
            float t = 1f - (flashTimer / flashDuration);
            fillImage.color = Color.Lerp(flashColor, baseColor, t);
        }
    }
    
    private void StartFlash(Color color)
    {
        if (fillImage == null) return;
        
        isFlashing = true;
        flashColor = color;
        flashTimer = flashDuration;
        fillImage.color = flashColor;
    }
    
    private void UpdateTextDisplay()
    {
        if (!showHealthValue || healthText == null || playerHealth == null)
            return;
        
        if (showAsPercentage)
        {
            float percentage = (playerHealth.Health / playerHealth.MaxHealth) * 100f;
            string valueStr = percentage.ToString($"F{decimalPlaces}");
            healthText.text = string.Format(textFormat, valueStr);
        }
        else
        {
            string currentStr = playerHealth.Health.ToString($"F{decimalPlaces}");
            string maxStr = playerHealth.MaxHealth.ToString($"F{decimalPlaces}");
            healthText.text = string.Format(textFormat, currentStr, maxStr);
        }
    }
    
    // Public API
    
    /// <summary>
    /// Set custom slider value manually (overrides automatic health tracking)
    /// </summary>
    public void SetSliderValue(float value)
    {
        targetFillValue = Mathf.Clamp01(value);
        
        if (!smoothFill && healthSlider != null)
        {
            healthSlider.value = targetFillValue;
            currentFillValue = targetFillValue;
        }
    }
    
    /// <summary>
    /// Set custom color manually
    /// </summary>
    public void SetSliderColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
    
    /// <summary>
    /// Force immediate update without smooth transition
    /// </summary>
    public void ForceUpdate()
    {
        if (playerHealth != null && healthSlider != null)
        {
            float value = playerHealth.Health / playerHealth.MaxHealth;
            healthSlider.value = value;
            currentFillValue = value;
            targetFillValue = value;
            previousHealth = playerHealth.Health;
            
            UpdateSliderColor();
            UpdateTextDisplay();
        }
    }
    
    /// <summary>
    /// Get current health percentage (0-100)
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentFillValue * 100f;
    }
    
    /// <summary>
    /// Get health status as string
    /// </summary>
    public string GetHealthStatus()
    {
        if (playerHealth == null) return "Unknown";
        
        float healthPercent = (playerHealth.Health / playerHealth.MaxHealth) * 100f;
        
        if (healthPercent >= moderateThreshold) return "Healthy";
        if (healthPercent >= lowThreshold) return "Hurt";
        if (healthPercent >= criticalThreshold) return "Critical";
        if (healthPercent > 0f) return "Near Death";
        return "Dead";
    }
    
    /// <summary>
    /// Check if health is critical
    /// </summary>
    public bool IsCritical()
    {
        if (playerHealth == null) return false;
        float healthPercent = (playerHealth.Health / playerHealth.MaxHealth) * 100f;
        return healthPercent <= criticalThreshold;
    }
    
    /// <summary>
    /// Check if health is low
    /// </summary>
    public bool IsLow()
    {
        if (playerHealth == null) return false;
        float healthPercent = (playerHealth.Health / playerHealth.MaxHealth) * 100f;
        return healthPercent <= lowThreshold;
    }
    
    /// <summary>
    /// Check if health is full
    /// </summary>
    public bool IsFull()
    {
        if (playerHealth == null) return false;
        return playerHealth.Health >= playerHealth.MaxHealth;
    }
    
    /// <summary>
    /// Trigger flash effect manually
    /// </summary>
    public void TriggerFlash(Color color)
    {
        StartFlash(color);
    }
    
    private void OnValidate()
    {
        // Validate thresholds are in descending order
        if (lowThreshold > moderateThreshold)
        {
            lowThreshold = moderateThreshold;
        }
        
        if (criticalThreshold > lowThreshold)
        {
            criticalThreshold = lowThreshold;
        }
        
        // Clamp thresholds to 0-100 range
        moderateThreshold = Mathf.Clamp(moderateThreshold, 0f, 100f);
        lowThreshold = Mathf.Clamp(lowThreshold, 0f, moderateThreshold);
        criticalThreshold = Mathf.Clamp(criticalThreshold, 0f, lowThreshold);
    }
}
