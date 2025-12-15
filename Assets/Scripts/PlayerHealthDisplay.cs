using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerHealthDisplay : MonoBehaviour
{
    [Header("References")]
    public JUHealth playerHealth;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    
    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction = true;
    public bool showPrefix = false;
    public string prefix = "HP: ";
    
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
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<JUHealth>();
            }
            
            if (playerHealth == null)
            {
                Debug.LogWarning("PlayerHealthDisplay: Could not find player health component!");
            }
        }
        
        if (healthText == null)
        {
            healthText = GetComponent<TextMeshProUGUI>();
        }
        
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }
    
    private void InitializeSlider()
    {
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.Health;
        }
    }
    
    private void Update()
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (playerHealth == null) return;
        
        if (healthText != null)
        {
            UpdateTextDisplay();
        }
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.Health;
        }
    }
    
    private void UpdateTextDisplay()
    {
        string displayText = "";
        
        if (showAsPercentage)
        {
            float percentage = (playerHealth.Health / playerHealth.MaxHealth) * 100f;
            displayText = $"{Mathf.RoundToInt(percentage)}%";
        }
        else if (showFraction)
        {
            displayText = $"{Mathf.RoundToInt(playerHealth.Health)}/{Mathf.RoundToInt(playerHealth.MaxHealth)}";
        }
        else
        {
            displayText = Mathf.RoundToInt(playerHealth.Health).ToString();
        }
        
        if (showPrefix)
        {
            healthText.text = $"{prefix}{displayText}";
        }
        else
        {
            healthText.text = displayText;
        }
    }
}
