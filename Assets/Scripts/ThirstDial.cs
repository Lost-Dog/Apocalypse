using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a radial dial (Image with Fill type = Radial) to display player thirst level.
/// Designed for use with Stat_Dial_01 or similar dial UI elements.
/// </summary>
public class ThirstDial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SurvivalManager that tracks thirst")]
    public SurvivalManager survivalManager;
    
    [Tooltip("The Image component with Image Type = Filled (Radial)")]
    public Image dialFillImage;
    
    [Tooltip("Optional needle/pointer transform to rotate (like a speedometer)")]
    public Transform needleTransform;
    
    [Tooltip("Optional text to display thirst value")]
    public TMP_Text thirstText;
    
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
    [Tooltip("Color when thirst is full (hydrated)")]
    public Color hydratedColor = new Color(0f, 0.5f, 1f, 1f); // Cyan/Blue
    
    [Tooltip("Color when thirst is moderate")]
    public Color moderateColor = new Color(0.5f, 0.8f, 1f, 1f); // Light Blue
    
    [Tooltip("Color when thirst is low (thirsty)")]
    public Color thirstyColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    
    [Tooltip("Color when thirst is critical (dehydrated)")]
    public Color dehydratedColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("Enable color gradient based on thirst level")]
    public bool enableColorGradient = true;
    
    [Header("Text Display")]
    [Tooltip("Show thirst value on text")]
    public bool showThirstValue = true;
    
    [Tooltip("Show thirst as percentage")]
    public bool showAsPercentage = true;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
    
    [Tooltip("Text format (use {0} for value)")]
    public string textFormat = "{0}%";
    
    [Header("Thirst Thresholds")]
    [Tooltip("Thirst level below which dial shows moderate color")]
    public float moderateThreshold = 70f;
    
    [Tooltip("Thirst level below which dial shows thirsty color")]
    public float thirstyThreshold = 40f;
    
    [Tooltip("Thirst level below which dial shows dehydrated color")]
    public float dehydratedThreshold = 20f;
    
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
            // Set initial fill to current thirst
            float initialFill = CalculateFillAmount(survivalManager.currentThirst);
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
            Debug.Log($"[ThirstDial] Initialized on '{gameObject.name}'");
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
        
        // Find thirst text
        if (thirstText == null)
        {
            thirstText = GetComponentInChildren<TMP_Text>();
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
            Debug.LogError($"[ThirstDial] SurvivalManager not found! Please assign it in the inspector or ensure one exists in the scene.");
        }
        
        if (dialFillImage == null)
        {
            Debug.LogError($"[ThirstDial] Dial Fill Image not found on '{gameObject.name}'! Please assign an Image component with Fill type.");
        }
        else if (dialFillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[ThirstDial] Image '{dialFillImage.name}' is not set to Fill type. Changing to Filled (Radial 360).");
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
            // Calculate target fill based on current thirst
            targetFillAmount = CalculateFillAmount(survivalManager.currentThirst);
            
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
            float normalizedValue = survivalManager.currentThirst / survivalManager.maxThirst;
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
    
    private float CalculateFillAmount(float thirst)
    {
        // Normalize thirst to 0-1 range
        float normalizedThirst = thirst / survivalManager.maxThirst;
        
        // Map to fill amount range
        float fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, normalizedThirst);
        
        // Clamp to valid range
        return Mathf.Clamp01(fillAmount);
    }
    
    private void UpdateDialColor()
    {
        if (!enableColorGradient || dialFillImage == null)
            return;
        
        float thirst = survivalManager.currentThirst;
        Color targetColor;
        
        // Determine color based on thirst thresholds (inverted - lower is worse)
        if (thirst >= moderateThreshold)
        {
            // Well hydrated - full color
            targetColor = hydratedColor;
        }
        else if (thirst >= thirstyThreshold)
        {
            // Moderate thirst - blend between hydrated and moderate
            float t = Mathf.InverseLerp(thirstyThreshold, moderateThreshold, thirst);
            targetColor = Color.Lerp(moderateColor, hydratedColor, t);
        }
        else if (thirst >= dehydratedThreshold)
        {
            // Thirsty - blend between moderate and thirsty
            float t = Mathf.InverseLerp(dehydratedThreshold, thirstyThreshold, thirst);
            targetColor = Color.Lerp(thirstyColor, moderateColor, t);
        }
        else
        {
            // Dehydrated - blend between thirsty and critical
            float t = Mathf.InverseLerp(0f, dehydratedThreshold, thirst);
            targetColor = Color.Lerp(dehydratedColor, thirstyColor, t);
        }
        
        dialFillImage.color = targetColor;
    }
    
    private void UpdateTextDisplay()
    {
        if (!showThirstValue || thirstText == null || survivalManager == null)
            return;
        
        float displayValue;
        
        if (showAsPercentage)
        {
            displayValue = (survivalManager.currentThirst / survivalManager.maxThirst) * 100f;
        }
        else
        {
            displayValue = survivalManager.currentThirst;
        }
        
        // Format with specified decimal places
        string valueStr = displayValue.ToString($"F{decimalPlaces}");
        
        // Apply text format
        thirstText.text = string.Format(textFormat, valueStr);
    }
    
    // Public API
    
    /// <summary>
    /// Set custom fill amount manually (overrides automatic thirst tracking)
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
            float fillAmount = CalculateFillAmount(survivalManager.currentThirst);
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
    /// Get thirst status as string
    /// </summary>
    public string GetThirstStatus()
    {
        if (survivalManager == null) return "Unknown";
        
        float thirst = survivalManager.currentThirst;
        
        if (thirst >= moderateThreshold) return "Hydrated";
        if (thirst >= thirstyThreshold) return "Moderate";
        if (thirst >= dehydratedThreshold) return "Thirsty";
        if (thirst > 0f) return "Dehydrated";
        return "Critical";
    }
    
    /// <summary>
    /// Check if player is dehydrated (critical)
    /// </summary>
    public bool IsDehydrated()
    {
        return survivalManager != null && survivalManager.currentThirst < dehydratedThreshold;
    }
    
    /// <summary>
    /// Check if player is thirsty
    /// </summary>
    public bool IsThirsty()
    {
        return survivalManager != null && survivalManager.currentThirst < thirstyThreshold;
    }
    
    /// <summary>
    /// Check if player needs water soon
    /// </summary>
    public bool NeedsWater()
    {
        return survivalManager != null && survivalManager.currentThirst < moderateThreshold;
    }
    
    private void OnValidate()
    {
        // Ensure min is less than max
        if (minFillAmount > maxFillAmount)
        {
            minFillAmount = maxFillAmount;
        }
        
        // Validate thresholds are in descending order (higher thirst = better)
        if (moderateThreshold < thirstyThreshold)
        {
            moderateThreshold = thirstyThreshold;
        }
        
        if (thirstyThreshold < dehydratedThreshold)
        {
            thirstyThreshold = dehydratedThreshold;
        }
    }
}
