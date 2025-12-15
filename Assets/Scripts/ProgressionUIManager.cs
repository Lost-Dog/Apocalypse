using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionUIManager : MonoBehaviour
{
    [Header("XP Bar References")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Level Up Notification")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private Animator levelUpAnimator;
    [SerializeField] private float levelUpDisplayDuration = 3f;
    
    [Header("Skill Points")]
    [SerializeField] private TextMeshProUGUI skillPointsText;
    
    [Header("XP Gain Notification")]
    [SerializeField] private GameObject xpGainPanel;
    [SerializeField] private TextMeshProUGUI xpGainText;
    [SerializeField] private float xpGainDisplayDuration = 2f;
    
    private ProgressionManager progressionManager;
    private float levelUpTimer = 0f;
    private float xpGainTimer = 0f;
    
    public void Initialize(ProgressionManager manager)
    {
        progressionManager = manager;
        
        if (progressionManager != null)
        {
            progressionManager.onLevelUp.AddListener(OnLevelUp);
            progressionManager.onXPGained.AddListener(OnXPGained);
            progressionManager.onSkillPointGained.AddListener(OnSkillPointGained);
        }
        
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        
        if (xpGainPanel != null)
        {
            xpGainPanel.SetActive(false);
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (progressionManager != null)
        {
            progressionManager.onLevelUp.RemoveListener(OnLevelUp);
            progressionManager.onXPGained.RemoveListener(OnXPGained);
            progressionManager.onSkillPointGained.RemoveListener(OnSkillPointGained);
        }
    }
    
    private void Update()
    {
        if (levelUpTimer > 0)
        {
            levelUpTimer -= Time.deltaTime;
            if (levelUpTimer <= 0)
            {
                HideLevelUpNotification();
            }
        }
        
        if (xpGainTimer > 0)
        {
            xpGainTimer -= Time.deltaTime;
            if (xpGainTimer <= 0)
            {
                HideXPGainNotification();
            }
        }
    }
    
    private void OnLevelUp(int newLevel)
    {
        UpdateUI();
        ShowLevelUpNotification(newLevel);
    }
    
    private void OnXPGained(int amount)
    {
        UpdateUI();
        ShowXPGainNotification(amount);
    }
    
    private void OnSkillPointGained(int points)
    {
        UpdateSkillPointsDisplay();
    }
    
    public void UpdateUI()
    {
        UpdateXPBar();
        UpdateLevelDisplay();
        UpdateSkillPointsDisplay();
    }
    
    private void UpdateXPBar()
    {
        if (progressionManager == null) return;
        
        float xpProgress = progressionManager.GetXPProgress();
        
        if (xpSlider != null)
        {
            xpSlider.value = xpProgress;
        }
        
        if (xpText != null)
        {
            int currentXP = progressionManager.currentXP;
            int requiredXP = progressionManager.GetRequiredXPForLevel(progressionManager.currentLevel);
            int previousRequiredXP = progressionManager.GetRequiredXPForLevel(progressionManager.currentLevel - 1);
            
            int xpIntoLevel = currentXP - previousRequiredXP;
            int xpNeededForLevel = requiredXP - previousRequiredXP;
            
            xpText.text = $"{xpIntoLevel} / {xpNeededForLevel}";
        }
    }
    
    private void UpdateLevelDisplay()
    {
        if (progressionManager == null) return;
        
        if (levelText != null)
        {
            levelText.text = progressionManager.currentLevel.ToString();
        }
    }
    
    private void UpdateSkillPointsDisplay()
    {
        if (progressionManager == null) return;
        
        if (skillPointsText != null)
        {
            skillPointsText.text = progressionManager.skillPoints.ToString();
        }
    }
    
    private void ShowLevelUpNotification(int newLevel)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }
        
        if (levelUpText != null)
        {
            levelUpText.text = newLevel.ToString();
        }
        
        if (levelUpAnimator != null)
        {
            levelUpAnimator.SetTrigger("Show");
        }
        
        levelUpTimer = levelUpDisplayDuration;
    }
    
    private void HideLevelUpNotification()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }
    
    private void ShowXPGainNotification(int amount)
    {
        if (xpGainPanel != null)
        {
            xpGainPanel.SetActive(true);
        }
        
        if (xpGainText != null)
        {
            xpGainText.text = $"+{amount} XP";
        }
        
        xpGainTimer = xpGainDisplayDuration;
    }
    
    private void HideXPGainNotification()
    {
        if (xpGainPanel != null)
        {
            xpGainPanel.SetActive(false);
        }
    }
}
