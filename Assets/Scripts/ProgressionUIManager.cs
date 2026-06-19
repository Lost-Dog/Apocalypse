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

    [Header("Inspector Wiring")]
    [SerializeField] private bool autoWireMissingReferences = true;
    [SerializeField] private PlayerLevelDisplay levelDisplaySource;
    [SerializeField] private PlayerXPDisplay xpDisplaySource;
    
    private ProgressionManager progressionManager;
    private float levelUpTimer = 0f;
    private float xpGainTimer = 0f;
    private Coroutine delayedInitRoutine;

    private void Start()
    {
        TryAutoWireMissingReferences();

        // Self-initialize as a fallback if HUDManager didn't call Initialize() in time.
        if (progressionManager == null)
        {
            delayedInitRoutine = StartCoroutine(TryInitializeWithDelay());
        }
    }

    private void OnValidate()
    {
        TextMeshProUGUI prevXpText = xpText;
        TextMeshProUGUI prevLevelText = levelText;
        Slider prevSlider = xpSlider;

        TryAutoWireMissingReferences();

#if UNITY_EDITOR
        if (prevXpText != xpText || prevLevelText != levelText || prevSlider != xpSlider)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    private System.Collections.IEnumerator TryInitializeWithDelay()
    {
        const float maxWaitSeconds = 5f;
        float timeoutAt = Time.unscaledTime + maxWaitSeconds;

        while (progressionManager == null && Time.unscaledTime < timeoutAt)
        {
            ProgressionManager found = ProgressionManager.Instance
                ?? FindFirstObjectByType<ProgressionManager>();

            if (found != null)
            {
                Debug.LogWarning("[ProgressionUIManager] HUDManager did not initialize this component. Self-initializing via ProgressionManager.");
                Initialize(found);
                delayedInitRoutine = null;
                yield break;
            }

            yield return null;
        }

        if (progressionManager == null)
        {
            Debug.LogError("[ProgressionUIManager] Could not find a ProgressionManager after waiting. XP UI will not work.");
        }

        delayedInitRoutine = null;
    }

    public void Initialize(ProgressionManager manager)
    {
        TryAutoWireMissingReferences();

        // Avoid double-subscribing if called more than once.
        if (progressionManager != null)
        {
            progressionManager.onLevelUp.RemoveListener(OnLevelUp);
            progressionManager.onXPGained.RemoveListener(OnXPGained);
            progressionManager.onSkillPointGained.RemoveListener(OnSkillPointGained);
        }

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

    private void TryAutoWireMissingReferences()
    {
        if (!autoWireMissingReferences)
            return;

        if (levelDisplaySource == null)
            levelDisplaySource = FindFirstObjectByType<PlayerLevelDisplay>();

        if (xpDisplaySource == null)
            xpDisplaySource = FindFirstObjectByType<PlayerXPDisplay>();

        if (levelText == null)
            levelText = FindTextInChildren(levelDisplaySource != null ? levelDisplaySource.transform : null);

        if (xpText == null)
            xpText = FindTextInChildren(xpDisplaySource != null ? xpDisplaySource.transform : null);

        if (xpSlider == null && xpDisplaySource != null)
            xpSlider = xpDisplaySource.GetComponentInChildren<Slider>(true);

        if (xpSlider == null && xpText != null)
            xpSlider = xpText.GetComponentInParent<Slider>(true);

        if (levelText == null)
            Debug.LogWarning("[ProgressionUIManager] Could not auto-wire level text from PlayerLevelDisplay source.");

        if (xpText == null)
            Debug.LogWarning("[ProgressionUIManager] Could not auto-wire XP text from PlayerXPDisplay source.");
    }

    private static TextMeshProUGUI FindTextInChildren(Transform root)
    {
        if (root == null)
            return null;

        TextMeshProUGUI[] all = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < all.Length; i++)
        {
            TextMeshProUGUI component = all[i];
            if (component == null) continue;
            return component;
        }

        return null;
    }
    
    private void OnDestroy()
    {
        if (delayedInitRoutine != null)
        {
            StopCoroutine(delayedInitRoutine);
            delayedInitRoutine = null;
        }

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
