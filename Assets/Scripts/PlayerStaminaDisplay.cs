using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerStaminaDisplay : MonoBehaviour
{
    [Header("References")]
    public JUCharacterController playerController;
    public TextMeshProUGUI staminaText;
    public Slider staminaSlider;
    public Image staminaDial;
    
    [Header("Stamina Settings")]
    [Range(0f, 100f)] public float currentStamina = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 15f;
    public float staminaDrainRate = 8f;
    
    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction = true;
    public bool showPrefix = false;
    public string prefix = "Stamina: ";
    
    [Header("Dial Settings")]
    [Tooltip("Enable dial fill and color updates")]
    public bool enableDial = false;
    
    [Tooltip("Smooth transition speed for dial")]
    public float dialTransitionSpeed = 4f;
    
    [Header("Dial Colors")]
    public Color fullStaminaColor = new Color(0f, 1f, 0.2f, 1f); // Bright green
    public Color highStaminaColor = new Color(0.5f, 1f, 0f, 1f); // Yellow-green
    public Color moderateStaminaColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    public Color lowStaminaColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    public Color criticalStaminaColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Header("Auto-Find")]
    public bool autoFindReferences = true;
    
    // Private variables for dial
    private float currentDialFill = 1f;
    private float targetDialFill = 1f;
    
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
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<JUCharacterController>();
            }
            
            if (playerController == null)
            {
                Debug.LogWarning("PlayerStaminaDisplay: Could not find player controller!");
            }
        }
        
        if (staminaText == null)
        {
            staminaText = GetComponent<TextMeshProUGUI>();
        }
        
        if (staminaSlider == null)
        {
            staminaSlider = GetComponent<Slider>();
        }
        
        if (staminaDial == null && enableDial)
        {
            staminaDial = GetComponent<Image>();
            
            if (staminaDial == null)
            {
                staminaDial = GetComponentInChildren<Image>();
            }
            
            if (staminaDial != null && staminaDial.type != Image.Type.Filled)
            {
                Debug.LogWarning("PlayerStaminaDisplay: Found Image but it's not set to Filled type. Setting to Radial360.");
                staminaDial.type = Image.Type.Filled;
                staminaDial.fillMethod = Image.FillMethod.Radial360;
            }
        }
    }
    
    private void InitializeSlider()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        
        if (staminaDial != null && enableDial)
        {
            currentDialFill = currentStamina / maxStamina;
            targetDialFill = currentDialFill;
            staminaDial.fillAmount = currentDialFill;
            UpdateDialColor();
        }
    }
    
    private void Update()
    {
        UpdateStamina();
        UpdateDisplay();
    }
    
    private void UpdateStamina()
    {
        if (playerController != null && playerController.IsRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }
    
    private void UpdateDisplay()
    {
        if (staminaText != null)
        {
            UpdateTextDisplay();
        }
        
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
        
        if (staminaDial != null && enableDial)
        {
            UpdateDialDisplay();
        }
    }
    
    private void UpdateTextDisplay()
    {
        string displayText = "";
        
        if (showAsPercentage)
        {
            float percentage = (currentStamina / maxStamina) * 100f;
            displayText = $"{Mathf.RoundToInt(percentage)}%";
        }
        else if (showFraction)
        {
            displayText = $"{Mathf.RoundToInt(currentStamina)}/{Mathf.RoundToInt(maxStamina)}";
        }
        else
        {
            displayText = Mathf.RoundToInt(currentStamina).ToString();
        }
        
        if (showPrefix)
        {
            staminaText.text = $"{prefix}{displayText}";
        }
        else
        {
            staminaText.text = displayText;
        }
    }
    
    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
    }
    
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }
    
    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }
    
    private void UpdateDialDisplay()
    {
        // Calculate target fill based on stamina percentage
        targetDialFill = currentStamina / maxStamina;
        
        // Smooth interpolation
        currentDialFill = Mathf.MoveTowards(currentDialFill, targetDialFill, dialTransitionSpeed * Time.deltaTime);
        
        // Apply fill amount
        staminaDial.fillAmount = currentDialFill;
        
        // Update color
        UpdateDialColor();
    }
    
    private void UpdateDialColor()
    {
        if (staminaDial == null) return;
        
        float staminaPercent = currentDialFill * 100f;
        Color targetColor;
        
        // 5-tier color system based on stamina levels
        if (staminaPercent >= 75f)
        {
            // Full stamina (75-100%)
            float t = Mathf.InverseLerp(75f, 100f, staminaPercent);
            targetColor = Color.Lerp(highStaminaColor, fullStaminaColor, t);
        }
        else if (staminaPercent >= 50f)
        {
            // High stamina (50-75%)
            float t = Mathf.InverseLerp(50f, 75f, staminaPercent);
            targetColor = Color.Lerp(moderateStaminaColor, highStaminaColor, t);
        }
        else if (staminaPercent >= 25f)
        {
            // Moderate stamina (25-50%)
            float t = Mathf.InverseLerp(25f, 50f, staminaPercent);
            targetColor = Color.Lerp(lowStaminaColor, moderateStaminaColor, t);
        }
        else if (staminaPercent > 0f)
        {
            // Low/Critical stamina (0-25%)
            float t = Mathf.InverseLerp(0f, 25f, staminaPercent);
            targetColor = Color.Lerp(criticalStaminaColor, lowStaminaColor, t);
        }
        else
        {
            // Depleted
            targetColor = criticalStaminaColor;
        }
        
        staminaDial.color = targetColor;
    }
}
