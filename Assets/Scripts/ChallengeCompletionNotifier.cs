using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChallengeCompletionNotifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Header("Colors")]
    [SerializeField] private Color completedColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Color expiredColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color failedColor = new Color(1f, 0f, 0f, 1f);
    
    private CanvasGroup canvasGroup;
    private Coroutine currentNotification;
    
    private void Awake()
    {
        // Auto-find the HUD element if not assigned
        if (notificationPanel == null)
        {
            GameObject found = GameObject.Find("HUD_Apocalypse_Event_Mission_Complete_01");
            if (found != null)
            {
                notificationPanel = found;
            }
            else
            {
                Debug.LogWarning("HUD_Apocalypse_Event_Mission_Complete_01 not found! Please assign notification panel manually.");
            }
        }
        
        // Get or add CanvasGroup
        if (notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            
            // Auto-find UI components if not assigned
            if (titleText == null)
            {
                titleText = notificationPanel.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (titleText == null)
                {
                    titleText = notificationPanel.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
            
            if (descriptionText == null)
            {
                Transform descTransform = notificationPanel.transform.Find("Description");
                if (descTransform == null)
                {
                    descTransform = notificationPanel.transform.Find("Text");
                }
                if (descTransform != null)
                {
                    descriptionText = descTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            
            if (backgroundImage == null)
            {
                backgroundImage = notificationPanel.GetComponent<Image>();
                if (backgroundImage == null)
                {
                    Transform bgTransform = notificationPanel.transform.Find("Background");
                    if (bgTransform != null)
                    {
                        backgroundImage = bgTransform.GetComponent<Image>();
                    }
                }
            }
            
            // Start hidden
            canvasGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Subscribe to challenge events
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.onChallengeCompleted.AddListener(OnChallengeCompleted);
            ChallengeManager.Instance.onChallengeExpired.AddListener(OnChallengeExpired);
            ChallengeManager.Instance.onChallengeFailed.AddListener(OnChallengeFailed);
        }
        else
        {
            Debug.LogWarning("ChallengeManager not found! Notifications will not work.");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.onChallengeCompleted.RemoveListener(OnChallengeCompleted);
            ChallengeManager.Instance.onChallengeExpired.RemoveListener(OnChallengeExpired);
            ChallengeManager.Instance.onChallengeFailed.RemoveListener(OnChallengeFailed);
        }
    }
    
    private void OnChallengeCompleted(ActiveChallenge challenge)
    {
        string title = "MISSION COMPLETE";
        string description = challenge.challengeData.challengeName;
        
        // Add reward info
        int xpReward = challenge.GetTotalXPReward();
        int currencyReward = challenge.GetTotalCurrencyReward();
        
        if (xpReward > 0 || currencyReward > 0)
        {
            description += $"\n+{xpReward} XP";
            if (currencyReward > 0)
            {
                description += $"  •  +{currencyReward} Credits";
            }
        }
        
        ShowNotification(title, description, completedColor);
    }
    
    private void OnChallengeExpired(ActiveChallenge challenge)
    {
        string title = "CHALLENGE FAILED";
        string description = challenge.challengeData.challengeName + "\nTime limit exceeded";
        
        ShowNotification(title, description, failedColor);
    }
    
    private void OnChallengeFailed(ActiveChallenge challenge)
    {
        string title = "MISSION FAILED";
        string description = challenge.challengeData.challengeName;
        
        // Add failure reason if available
        if (challenge.playerDied)
        {
            description += "\nPlayer eliminated";
        }
        else if (challenge.challengeData.requireNoDeaths)
        {
            description += "\nCivilian casualties";
        }
        
        ShowNotification(title, description, failedColor);
    }
    
    public void ShowNotification(string title, string description, Color color)
    {
        if (notificationPanel == null || canvasGroup == null)
        {
            Debug.LogWarning("Notification panel not configured!");
            return;
        }
        
        // Stop any existing notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(DisplayNotification(title, description, color));
    }
    
    private IEnumerator DisplayNotification(string title, string description, Color color)
    {
        // Set text
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = color;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = color;
            bgColor.a = 0.8f;
            backgroundImage.color = bgColor;
        }
        
        // Show panel
        notificationPanel.SetActive(true);
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Wait
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        // Hide panel
        notificationPanel.SetActive(false);
        
        currentNotification = null;
    }
}
