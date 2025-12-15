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
    }
    
    private void InitializeSlider()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
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
}
