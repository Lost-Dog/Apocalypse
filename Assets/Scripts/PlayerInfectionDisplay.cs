using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class PlayerInfectionDisplay : MonoBehaviour
{
    [Header("References")]
    public JUHealth playerHealth;
    public TextMeshProUGUI infectionText;
    public Slider infectionSlider;
    
    [Header("Infection Settings")]
    [Range(0f, 100f)] public float currentInfection = 0f;
    public float maxInfection = 100f;
    public float infectionGrowthRate = 0.5f;
    public float infectionDecayRate = 1f;
    
    [Header("Health Damage Settings")]
    public bool enableHealthDamage = true;
    public float healthDamagePerSecond = 2f;
    public float damageTickInterval = 1f;
    
    [Header("Display Settings")]
    public bool showStatus = true;
    public bool showPrefix = false;
    public string prefix = "Infection: ";
    public string suffix = "%";
    
    [Header("Auto-Find")]
    public bool autoFindReferences = true;
    
    private float damageTimer;
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        InitializeSlider();
        UpdateDisplay();
        damageTimer = damageTickInterval;
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
                Debug.LogWarning("PlayerInfectionDisplay: Could not find player health!");
            }
        }
        
        if (infectionText == null)
        {
            infectionText = GetComponent<TextMeshProUGUI>();
        }
        
        if (infectionSlider == null)
        {
            infectionSlider = GetComponent<Slider>();
        }
    }
    
    private void InitializeSlider()
    {
        if (infectionSlider != null)
        {
            infectionSlider.minValue = 0f;
            infectionSlider.maxValue = maxInfection;
            infectionSlider.value = currentInfection;
        }
    }
    
    private void Update()
    {
        UpdateInfection();
        ApplyInfectionDamage();
        UpdateDisplay();
    }
    
    private void UpdateInfection()
    {
        if (currentInfection > 0f)
        {
            currentInfection -= infectionDecayRate * Time.deltaTime;
            currentInfection = Mathf.Max(0f, currentInfection);
        }
    }
    
    private void ApplyInfectionDamage()
    {
        if (!enableHealthDamage || currentInfection < maxInfection || playerHealth == null)
            return;
        
        damageTimer -= Time.deltaTime;
        
        if (damageTimer <= 0f)
        {
            float damage = healthDamagePerSecond * damageTickInterval;
            playerHealth.DoDamage(damage);
            damageTimer = damageTickInterval;
            
            if (Application.isEditor)
            {
                Debug.Log($"Infection damage: {damage} HP (Infection: {currentInfection}/{maxInfection})");
            }
        }
    }
    
    private void UpdateDisplay()
    {
        if (infectionText != null)
        {
            UpdateTextDisplay();
        }
        
        if (infectionSlider != null)
        {
            infectionSlider.value = currentInfection;
        }
    }
    
    private void UpdateTextDisplay()
    {
        string displayText = $"{Mathf.RoundToInt(currentInfection)}{suffix}";
        
        if (showStatus)
        {
            string status = GetInfectionStatus();
            displayText += $" ({status})";
        }
        
        if (showPrefix)
        {
            infectionText.text = $"{prefix}{displayText}";
        }
        else
        {
            infectionText.text = displayText;
        }
    }
    
    private string GetInfectionStatus()
    {
        if (currentInfection == 0f) return "None";
        if (currentInfection < 25f) return "Mild";
        if (currentInfection < 50f) return "Moderate";
        if (currentInfection < 75f) return "Severe";
        return "Critical";
    }
    
    public void AddInfection(float amount)
    {
        currentInfection = Mathf.Clamp(currentInfection + amount, 0f, maxInfection);
    }
    
    public void RemoveInfection(float amount)
    {
        currentInfection = Mathf.Max(0f, currentInfection - amount);
    }
    
    public void CureInfection()
    {
        currentInfection = 0f;
    }
    
    public bool IsInfected()
    {
        return currentInfection > 0f;
    }
    
    public float GetInfectionPercentage()
    {
        return currentInfection / maxInfection;
    }
}
