using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example UI for displaying challenge preview and allowing player to start/retry
/// Attach this to a UI panel and assign references in inspector
/// </summary>
public class ChallengePreviewUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI timeLimitText;
    [SerializeField] private TextMeshProUGUI objectivesText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private TextMeshProUGUI modifiersText;
    [SerializeField] private TextMeshProUGUI warningText;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button closeButton;
    
    private ActiveChallenge currentChallenge;
    private ChallengeManager challengeManager;
    
    private void Start()
    {
        challengeManager = ChallengeManager.Instance;
        
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
        
        HidePreview();
    }
    
    /// <summary>
    /// Show preview for a challenge
    /// </summary>
    public void ShowPreview(ActiveChallenge challenge)
    {
        if (challenge == null || challengeManager == null)
            return;
        
        currentChallenge = challenge;
        ChallengePreviewData preview = challengeManager.GetChallengePreview(challenge);
        
        if (preview == null)
            return;
        
        // Update UI
        if (titleText != null)
            titleText.text = preview.challengeName;
        
        if (descriptionText != null)
            descriptionText.text = preview.description;
        
        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {preview.difficulty}";
            difficultyText.color = GetDifficultyColor(preview.difficulty);
        }
        
        if (timeLimitText != null)
            timeLimitText.text = $"Time Limit: {preview.timeLimitFormatted}";
        
        if (objectivesText != null)
        {
            string objectives = preview.objectiveDescription + "\n";
            if (preview.enemyCount > 0)
                objectives += $"• Eliminate {preview.enemyCount} enemies\n";
            if (preview.civilianCount > 0)
                objectives += $"• Rescue {preview.civilianCount} civilians\n";
            if (preview.requireNoDeaths)
                objectives += "• <color=red>No Deaths Allowed</color>\n";
            if (preview.requireStealth)
                objectives += "• <color=yellow>Stealth Required</color>\n";
            
            objectivesText.text = objectives;
        }
        
        if (rewardsText != null)
        {
            string rewards = "<b>Rewards:</b>\n";
            rewards += $"XP: {preview.baseXP}";
            
            if (preview.maxXP > preview.baseXP)
                rewards += $" (up to <color=green>{preview.maxXP}</color> with bonuses)";
            
            rewards += $"\nCurrency: {preview.baseCurrency}";
            
            if (preview.maxCurrency > preview.baseCurrency)
                rewards += $" (up to <color=green>{preview.maxCurrency}</color>)";
            
            rewards += $"\nLoot: {preview.lootCount}× {preview.lootRarity}";
            
            if (preview.canGetPerfectBonus)
                rewards += "\n<color=cyan>• Perfect Completion Bonus Available</color>";
            if (preview.canGetSpeedBonus)
                rewards += "\n<color=cyan>• Speed Completion Bonus Available</color>";
            if (preview.canGetStealthBonus)
                rewards += "\n<color=cyan>• Stealth Completion Bonus Available</color>";
            
            rewardsText.text = rewards;
        }
        
        if (modifiersText != null)
        {
            if (preview.hasModifiers)
            {
                modifiersText.gameObject.SetActive(true);
                modifiersText.text = preview.modifiersDescription;
            }
            else
            {
                modifiersText.gameObject.SetActive(false);
            }
        }
        
        if (warningText != null)
        {
            string warnings = "";
            
            if (preview.playerLevel < preview.recommendedLevel - 2)
                warnings += $"<color=red>⚠ WARNING: You are {preview.recommendedLevel - preview.playerLevel} levels below recommended!</color>\n";
            
            if (preview.enemyHealthMultiplier > 1.5f)
                warnings += $"<color=orange>Enemies have {preview.enemyHealthMultiplier:P0} health</color>\n";
            
            if (preview.enemyDamageMultiplier > 1.5f)
                warnings += $"<color=orange>Enemies deal {preview.enemyDamageMultiplier:P0} damage</color>\n";
            
            if (preview.attemptCount > 0)
                warnings += $"<color=yellow>Attempt #{preview.attemptCount + 1}</color>\n";
            
            warningText.text = warnings;
            warningText.gameObject.SetActive(!string.IsNullOrEmpty(warnings));
        }
        
        // Update buttons
        UpdateButtons(preview.state);
        
        // Show panel
        if (previewPanel != null)
            previewPanel.SetActive(true);
    }
    
    private void UpdateButtons(ActiveChallenge.ChallengeState state)
    {
        bool canStart = (state == ActiveChallenge.ChallengeState.Discovered || state == ActiveChallenge.ChallengeState.Available);
        bool canRetry = (state == ActiveChallenge.ChallengeState.Failed && currentChallenge != null && currentChallenge.CanRetry());
        
        if (startButton != null)
        {
            startButton.gameObject.SetActive(canStart && state == ActiveChallenge.ChallengeState.Discovered);
            startButton.interactable = canStart;
        }
        
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(state == ActiveChallenge.ChallengeState.Failed);
            retryButton.interactable = canRetry;
            
            if (!canRetry && currentChallenge != null)
            {
                float cooldown = currentChallenge.GetRetryCooldownRemaining();
                if (cooldown > 0f)
                {
                    var buttonText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = $"Retry ({cooldown:F0}s)";
                }
            }
        }
    }
    
    private void OnStartClicked()
    {
        if (currentChallenge != null && challengeManager != null)
        {
            if (challengeManager.StartDiscoveredChallenge(currentChallenge))
            {
                HidePreview();
            }
        }
    }
    
    private void OnRetryClicked()
    {
        if (currentChallenge != null && challengeManager != null)
        {
            if (challengeManager.RetryChallenge(currentChallenge))
            {
                // Challenge is now available to start
                UpdateButtons(ActiveChallenge.ChallengeState.Available);
            }
        }
    }
    
    private void OnCloseClicked()
    {
        HidePreview();
    }
    
    public void HidePreview()
    {
        if (previewPanel != null)
            previewPanel.SetActive(false);
        
        currentChallenge = null;
    }
    
    private Color GetDifficultyColor(ChallengeData.ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeData.ChallengeDifficulty.Easy:
                return Color.green;
            case ChallengeData.ChallengeDifficulty.Medium:
                return Color.yellow;
            case ChallengeData.ChallengeDifficulty.Hard:
                return new Color(1f, 0.5f, 0f); // Orange
            case ChallengeData.ChallengeDifficulty.Extreme:
                return Color.red;
            default:
                return Color.white;
        }
    }
    
    private void Update()
    {
        // Update retry button cooldown if visible
        if (currentChallenge != null && 
            currentChallenge.state == ActiveChallenge.ChallengeState.Failed &&
            retryButton != null && retryButton.gameObject.activeSelf)
        {
            UpdateButtons(currentChallenge.state);
        }
    }
}
