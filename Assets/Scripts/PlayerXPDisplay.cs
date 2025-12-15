using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerXPDisplay : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;
    public TextMeshProUGUI xpText;
    public Slider xpSlider;
    
    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction = false;
    
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
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager == null)
            {
                Debug.LogWarning("PlayerXPDisplay: Could not find ProgressionManager in scene!");
            }
        }
        
        if (xpText == null)
        {
            xpText = GetComponent<TextMeshProUGUI>();
        }
        
        if (xpSlider == null)
        {
            xpSlider = GetComponent<Slider>();
        }
    }
    
    private void InitializeSlider()
    {
        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = 0f;
        }
    }
    
    private void Update()
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (progressionManager == null) return;
        
        if (xpText != null)
        {
            UpdateTextDisplay();
        }
        
        if (xpSlider != null)
        {
            xpSlider.value = progressionManager.GetXPProgress();
        }
    }
    
    private void UpdateTextDisplay()
    {
        if (showAsPercentage)
        {
            float percentage = progressionManager.GetXPProgress() * 100f;
            xpText.text = $"{Mathf.RoundToInt(percentage)}%";
        }
        else if (showFraction)
        {
            int currentXP = progressionManager.currentXP;
            int requiredXP = progressionManager.GetRequiredXPForLevel(progressionManager.currentLevel);
            xpText.text = $"{currentXP}/{requiredXP}";
        }
        else
        {
            xpText.text = progressionManager.currentXP.ToString();
        }
    }
}
