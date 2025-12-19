using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a radial dial (Image with Fill type = Radial) to display player infection level.
/// Designed for use with Stat_Dial_00 or similar dial UI elements.
/// </summary>
public class InfectionDial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SurvivalManager that tracks infection")]
    public SurvivalManager survivalManager;
    
    [Tooltip("The Image component with Image Type = Filled (Radial)")]
    public Image dialFillImage;
    
    [Tooltip("Optional needle/pointer transform to rotate (like a speedometer)")]
    public Transform needleTransform;
    
    [Tooltip("Optional text to display infection value")]
    public TMP_Text infectionText;
    
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
    
    [Header("Needle Rotation Settings")]
    [Tooltip("Enable needle rotation (alternative to radial fill)")]
    public bool enableNeedleRotation = false;
    
    [Tooltip("Lock fill amount at max (for speedometer-style dials where only needle rotates)")]
    public bool lockFillAtMax = false;
    
    [Tooltip("Minimum rotation angle (degrees) - typically -120 for left")]
    public float minRotationAngle = -120f;
    
    [Tooltip("Maximum rotation angle (degrees) - typically 120 for right")]
    public float maxRotationAngle = 120f;
    
    [Tooltip("Rotation axis (usually Z for 2D UI)")]
    public Vector3 rotationAxis = Vector3.forward;
    
    [Header("Color Settings")]
    [Tooltip("Color when infection is low/none (healthy)")]
    public Color healthyColor = new Color(0f, 1f, 0f, 1f); // Green
    
    [Tooltip("Color when infection is moderate (warning)")]
    public Color warningColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    
    [Tooltip("Color when infection is high (danger)")]
    public Color dangerColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("Color when infection is critical (deadly)")]
    public Color criticalColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Tooltip("Enable color gradient based on infection level")]
    public bool enableColorGradient = true;
    
    [Header("Text Display")]
    [Tooltip("Show infection value on text")]
    public bool showInfectionValue = true;
    
    [Tooltip("Show infection as percentage")]
    public bool showAsPercentage = true;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
    
    [Tooltip("Text format (use {0} for value)")]
    public string textFormat = "{0}%";
    
    [Header("Infection Thresholds")]
    [Tooltip("Infection level at which dial shows warning color")]
    public float warningThreshold = 25f;
    
    [Tooltip("Infection level at which dial shows danger color")]
    public float dangerThreshold = 50f;
    
    [Tooltip("Infection level at which dial shows critical color")]
    public float criticalThreshold = 75f;
    
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
        
        ValidateComponents();
        SetupDialImage();
        
        if (survivalManager != null)
        {
            // Set initial fill to current infection
            float initialFill = CalculateFillAmount(survivalManager.currentInfection);
            currentFillAmount = initialFill;
            targetFillAmount = initialFill;
            
            if (dialFillImage != null)
            {
                dialFillImage.fillAmount = initialFill;
            }
            
            // Update text display immediately to prevent showing wrong value
            UpdateTextDisplay();
            UpdateDialColor();
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[InfectionDial] Initialized on '{gameObject.name}'");
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
        
        // Find infection text
        if (infectionText == null)
        {
            infectionText = GetComponentInChildren<TMP_Text>();
        }
        
        // Find needle/pointer transform
        if (needleTransform == null)
        {
            Transform needleSearch = transform.Find("Needle");
            if (needleSearch == null) needleSearch = transform.Find("Pointer");
            if (needleSearch == null) needleSearch = transform.Find("IMG_Dial_Indicator");
            if (needleSearch == null) needleSearch = transform.Find("Indicator");
            if (needleSearch == null) needleSearch = transform.Find("Arrow");
            
            if (needleSearch != null)
            {
                needleTransform = needleSearch;
            }
        }
    }
    
    private void ValidateComponents()
    {
        if (survivalManager == null)
        {
            Debug.LogError($"[InfectionDial] SurvivalManager not found! Please assign it in the inspector or ensure one exists in the scene.");
        }
        
        if (dialFillImage == null)
        {
            Debug.LogError($"[InfectionDial] Dial Fill Image not found on '{gameObject.name}'! Please assign an Image component with Fill type.");
        }
        else if (dialFillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[InfectionDial] Image '{dialFillImage.name}' is not set to Fill type. Changing to Filled (Radial 360).");
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
        // For speedometer-style dials with needles, lock fill at max
        if (enableNeedleRotation && lockFillAtMax)
        {
            targetFillAmount = maxFillAmount;
            currentFillAmount = maxFillAmount;
        }
        else
        {
            // Calculate target fill based on current infection
            targetFillAmount = CalculateFillAmount(survivalManager.currentInfection);
            
            // Smooth interpolation or instant update
            if (smoothFill)
            {
                currentFillAmount = Mathf.MoveTowards(currentFillAmount, targetFillAmount, fillTransitionSpeed * Time.deltaTime);
            }
            else
            {
                currentFillAmount = targetFillAmount;
            }
        }
        
        // Apply fill amount to dial image if it exists
        if (dialFillImage != null)
        {
            dialFillImage.fillAmount = currentFillAmount;
        }
        
        // Rotate needle if enabled and exists
        if (enableNeedleRotation && needleTransform != null)
        {
            float normalizedValue = survivalManager.currentInfection / survivalManager.maxInfection;
            float targetAngle = Mathf.Lerp(minRotationAngle, maxRotationAngle, normalizedValue);
            Vector3 currentRotation = needleTransform.localEulerAngles;
            
            if (smoothFill)
            {
                float currentAngle = needleTransform.localEulerAngles.z;
                if (currentAngle > 180) currentAngle -= 360;
                float smoothAngle = Mathf.MoveTowards(currentAngle, targetAngle, fillTransitionSpeed * 60f * Time.deltaTime);
                needleTransform.localEulerAngles = rotationAxis * smoothAngle;
            }
            else
            {
                needleTransform.localEulerAngles = rotationAxis * targetAngle;
            }
        }
    }
    
    private float CalculateFillAmount(float infection)
    {
        // Normalize infection to 0-1 range
        float normalizedInfection = infection / survivalManager.maxInfection;
        
        // Map to fill amount range
        float fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, normalizedInfection);
        
        // Clamp to valid range
        return Mathf.Clamp01(fillAmount);
    }
    
    private void UpdateDialColor()
    {
        if (!enableColorGradient || dialFillImage == null)
            return;
        
        float infection = survivalManager.currentInfection;
        Color targetColor;
        
        // Determine color based on infection thresholds
        if (infection <= warningThreshold)
        {
            // Low infection - blend between healthy and warning
            float t = Mathf.InverseLerp(0f, warningThreshold, infection);
            targetColor = Color.Lerp(healthyColor, warningColor, t);
        }
        else if (infection <= dangerThreshold)
        {
            // Moderate infection - blend between warning and danger
            float t = Mathf.InverseLerp(warningThreshold, dangerThreshold, infection);
            targetColor = Color.Lerp(warningColor, dangerColor, t);
        }
        else if (infection <= criticalThreshold)
        {
            // High infection - blend between danger and critical
            float t = Mathf.InverseLerp(dangerThreshold, criticalThreshold, infection);
            targetColor = Color.Lerp(dangerColor, criticalColor, t);
        }
        else
        {
            // Critical infection - full red
            targetColor = criticalColor;
        }
        
        dialFillImage.color = targetColor;
    }
    
    private void UpdateTextDisplay()
    {
        if (!showInfectionValue || infectionText == null || survivalManager == null)
            return;
        
        float displayValue;
        
        if (showAsPercentage)
        {
            displayValue = (survivalManager.currentInfection / survivalManager.maxInfection) * 100f;
        }
        else
        {
            displayValue = survivalManager.currentInfection;
        }
        
        // Format with specified decimal places
        string valueStr = displayValue.ToString($"F{decimalPlaces}");
        
        // Apply text format
        infectionText.text = string.Format(textFormat, valueStr);
    }
    
    // Public API
    
    /// <summary>
    /// Set custom fill amount manually (overrides automatic infection tracking)
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
            float fillAmount = CalculateFillAmount(survivalManager.currentInfection);
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
    /// Get infection status as string
    /// </summary>
    public string GetInfectionStatus()
    {
        if (survivalManager == null) return "Unknown";
        
        float infection = survivalManager.currentInfection;
        
        if (infection >= criticalThreshold) return "Critical";
        if (infection >= dangerThreshold) return "Severe";
        if (infection >= warningThreshold) return "Moderate";
        if (infection > 0f) return "Mild";
        return "None";
    }
    
    /// <summary>
    /// Check if infection is in critical range
    /// </summary>
    public bool IsCritical()
    {
        return survivalManager != null && survivalManager.currentInfection >= criticalThreshold;
    }
    
    /// <summary>
    /// Check if infection is in danger range
    /// </summary>
    public bool IsDanger()
    {
        return survivalManager != null && survivalManager.currentInfection >= dangerThreshold;
    }
    
    /// <summary>
    /// Check if infection is in warning range
    /// </summary>
    public bool IsWarning()
    {
        return survivalManager != null && survivalManager.currentInfection >= warningThreshold;
    }
    
    private void OnValidate()
    {
        // Ensure min is less than max
        if (minFillAmount > maxFillAmount)
        {
            minFillAmount = maxFillAmount;
        }
        
        // Validate thresholds are in ascending order
        if (warningThreshold > dangerThreshold)
        {
            warningThreshold = dangerThreshold;
        }
        
        if (dangerThreshold > criticalThreshold)
        {
            dangerThreshold = criticalThreshold;
        }
    }
}
