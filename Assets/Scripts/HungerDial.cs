using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a radial dial (Image with Fill type = Radial) to display player hunger level.
/// Designed for use with Stat_Dial_02 or similar dial UI elements.
/// </summary>
public class HungerDial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SurvivalManager that tracks hunger")]
    public SurvivalManager survivalManager;
    
    [Tooltip("The Image component with Image Type = Filled (Radial)")]
    public Image dialFillImage;
    
    [Tooltip("Optional needle/pointer transform to rotate (like a speedometer)")]
    public Transform needleTransform;
    
    [Tooltip("Optional text to display hunger value")]
    public TMP_Text hungerText;
    
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
    [Tooltip("Color when hunger is full (well fed)")]
    public Color wellFedColor = new Color(0f, 1f, 0f, 1f); // Green
    
    [Tooltip("Color when hunger is satisfied")]
    public Color satisfiedColor = new Color(0.5f, 1f, 0.5f, 1f); // Light Green
    
    [Tooltip("Color when hunger is moderate (hungry)")]
    public Color hungryColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    
    [Tooltip("Color when hunger is critical (starving)")]
    public Color starvingColor = new Color(1f, 0.5f, 0f, 1f); // Orange/Red
    
    [Tooltip("Enable color gradient based on hunger level")]
    public bool enableColorGradient = true;
    
    [Header("Text Display")]
    [Tooltip("Show hunger value on text")]
    public bool showHungerValue = true;
    
    [Tooltip("Show hunger as percentage")]
    public bool showAsPercentage = true;
    
    [Tooltip("Number of decimal places to show")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
    
    [Tooltip("Text format (use {0} for value)")]
    public string textFormat = "{0}%";
    
    [Header("Hunger Thresholds")]
    [Tooltip("Hunger level below which dial shows satisfied color")]
    public float satisfiedThreshold = 75f;
    
    [Tooltip("Hunger level below which dial shows hungry color")]
    public float hungryThreshold = 50f;
    
    [Tooltip("Hunger level below which dial shows starving color")]
    public float starvingThreshold = 30f;
    
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
            // Set initial fill to current hunger
            float initialFill = CalculateFillAmount(survivalManager.currentHunger);
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
            Debug.Log($"[HungerDial] Initialized on '{gameObject.name}'");
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
        
        // Find hunger text
        if (hungerText == null)
        {
            hungerText = GetComponentInChildren<TMP_Text>();
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
            Debug.LogError($"[HungerDial] SurvivalManager not found! Please assign it in the inspector or ensure one exists in the scene.");
        }
        
        if (dialFillImage == null)
        {
            Debug.LogError($"[HungerDial] Dial Fill Image not found on '{gameObject.name}'! Please assign an Image component with Fill type.");
        }
        else if (dialFillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[HungerDial] Image '{dialFillImage.name}' is not set to Fill type. Changing to Filled (Radial 360).");
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
            // Calculate target fill based on current hunger
            targetFillAmount = CalculateFillAmount(survivalManager.currentHunger);
            
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
            float normalizedValue = survivalManager.currentHunger / survivalManager.maxHunger;
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
    
    private float CalculateFillAmount(float hunger)
    {
        // Normalize hunger to 0-1 range
        float normalizedHunger = hunger / survivalManager.maxHunger;
        
        // Map to fill amount range
        float fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, normalizedHunger);
        
        // Clamp to valid range
        return Mathf.Clamp01(fillAmount);
    }
    
    private void UpdateDialColor()
    {
        if (!enableColorGradient || dialFillImage == null)
            return;
        
        float hunger = survivalManager.currentHunger;
        Color targetColor;
        
        // Determine color based on hunger thresholds (inverted - lower is worse)
        if (hunger >= satisfiedThreshold)
        {
            // Well fed - blend between satisfied and well fed
            float t = Mathf.InverseLerp(satisfiedThreshold, survivalManager.maxHunger, hunger);
            targetColor = Color.Lerp(satisfiedColor, wellFedColor, t);
        }
        else if (hunger >= hungryThreshold)
        {
            // Satisfied - blend between hungry and satisfied
            float t = Mathf.InverseLerp(hungryThreshold, satisfiedThreshold, hunger);
            targetColor = Color.Lerp(hungryColor, satisfiedColor, t);
        }
        else if (hunger >= starvingThreshold)
        {
            // Hungry - blend between starving and hungry
            float t = Mathf.InverseLerp(starvingThreshold, hungryThreshold, hunger);
            targetColor = Color.Lerp(starvingColor, hungryColor, t);
        }
        else
        {
            // Starving - critical color
            targetColor = starvingColor;
        }
        
        dialFillImage.color = targetColor;
    }
    
    private void UpdateTextDisplay()
    {
        if (!showHungerValue || hungerText == null || survivalManager == null)
            return;
        
        float displayValue;
        
        if (showAsPercentage)
        {
            displayValue = (survivalManager.currentHunger / survivalManager.maxHunger) * 100f;
        }
        else
        {
            displayValue = survivalManager.currentHunger;
        }
        
        // Format with specified decimal places
        string valueStr = displayValue.ToString($"F{decimalPlaces}");
        
        // Apply text format
        hungerText.text = string.Format(textFormat, valueStr);
    }
    
    // Public API
    
    /// <summary>
    /// Set custom fill amount manually (overrides automatic hunger tracking)
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
            float fillAmount = CalculateFillAmount(survivalManager.currentHunger);
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
    /// Get hunger status as string
    /// </summary>
    public string GetHungerStatus()
    {
        if (survivalManager == null) return "Unknown";
        
        float hunger = survivalManager.currentHunger;
        
        if (hunger >= satisfiedThreshold) return "Well Fed";
        if (hunger >= hungryThreshold) return "Satisfied";
        if (hunger >= starvingThreshold) return "Hungry";
        if (hunger > 0f) return "Very Hungry";
        return "Starving";
    }
    
    /// <summary>
    /// Check if player is starving (critical)
    /// </summary>
    public bool IsStarving()
    {
        return survivalManager != null && survivalManager.currentHunger < starvingThreshold;
    }
    
    /// <summary>
    /// Check if player is hungry
    /// </summary>
    public bool IsHungry()
    {
        return survivalManager != null && survivalManager.currentHunger < hungryThreshold;
    }
    
    /// <summary>
    /// Check if player needs food soon
    /// </summary>
    public bool NeedsFood()
    {
        return survivalManager != null && survivalManager.currentHunger < satisfiedThreshold;
    }
    
    private void OnValidate()
    {
        // Ensure min is less than max
        if (minFillAmount > maxFillAmount)
        {
            minFillAmount = maxFillAmount;
        }
        
        // Validate thresholds are in descending order (higher hunger = better)
        if (satisfiedThreshold < hungryThreshold)
        {
            satisfiedThreshold = hungryThreshold;
        }
        
        if (hungryThreshold < starvingThreshold)
        {
            hungryThreshold = starvingThreshold;
        }
    }
}
