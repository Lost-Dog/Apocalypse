using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChallengeNotificationUI : MonoBehaviour
{
    [Header("Notification Panel")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private CanvasGroup notificationCanvasGroup;
    
    [Header("Notification Content")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Image difficultyImage;
    [SerializeField] private Image challengeIcon;
    [SerializeField] private Slider progressSlider;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip notificationSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip failSound;
    
    [Header("Display Settings")]
    [SerializeField] private bool showMultipleChallenges = true;
    [SerializeField] private int maxDisplayedChallenges = 3;
    [SerializeField] private bool showDistance = true;
    [SerializeField] private float distanceUpdateInterval = 0.5f;
    
    private AudioSource audioSource;
    private List<ActiveChallenge> trackedChallenges = new List<ActiveChallenge>();
    private ActiveChallenge currentDisplayedChallenge;
    private bool isPanelVisible = false;
    private ChallengeManager challengeManager;
    private bool isInitialized = false;
    private Transform playerTransform;
    private float distanceUpdateTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void Initialize(ChallengeManager manager)
    {
        if (isInitialized)
        {
            Debug.LogWarning("ChallengeNotificationUI already initialized!");
            return;
        }

        if (manager == null)
        {
            Debug.LogError("ChallengeNotificationUI: ChallengeManager is null!");
            return;
        }

        challengeManager = manager;

        challengeManager.onChallengeSpawned.AddListener(OnChallengeSpawned);
        challengeManager.onChallengeProgress.AddListener(OnChallengeProgress);
        challengeManager.onChallengeCompleted.AddListener(OnChallengeCompleted);
        challengeManager.onChallengeExpired.AddListener(OnChallengeExpired);
        challengeManager.onChallengeFailed.AddListener(OnChallengeFailed);

        FindPlayerTransform();
        isInitialized = true;
    }
    
    private void FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            JUTPS.JUCharacterController playerController = Object.FindFirstObjectByType<JUTPS.JUCharacterController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }
        
        if (playerTransform == null && showDistance)
        {
            Debug.LogWarning("ChallengeNotificationUI: Could not find player transform for distance calculation.");
        }
    }

    private void Update()
    {
        if (isPanelVisible && currentDisplayedChallenge != null)
        {
            UpdateChallengeDisplay();
            
            if (showDistance && distanceText != null)
            {
                distanceUpdateTimer -= Time.deltaTime;
                if (distanceUpdateTimer <= 0f)
                {
                    UpdateDistanceDisplay();
                    distanceUpdateTimer = distanceUpdateInterval;
                }
            }
        }
    }

    private void OnChallengeSpawned(ActiveChallenge challenge)
    {
        if (challenge == null || challenge.challengeData == null)
            return;

        trackedChallenges.Add(challenge);

        if (!isPanelVisible)
        {
            ShowChallenge(challenge);
        }
    }

    private void OnChallengeProgress(ActiveChallenge challenge)
    {
        if (currentDisplayedChallenge == challenge)
        {
            UpdateChallengeDisplay();
        }
    }

    private void OnChallengeCompleted(ActiveChallenge challenge)
    {
        RemoveChallengeFromTracking(challenge);

        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        if (currentDisplayedChallenge == challenge)
        {
            ShowNextChallengeOrHide();
        }
    }

    private void OnChallengeExpired(ActiveChallenge challenge)
    {
        RemoveChallengeFromTracking(challenge);

        if (currentDisplayedChallenge == challenge)
        {
            ShowNextChallengeOrHide();
        }
    }

    private void OnChallengeFailed(ActiveChallenge challenge)
    {
        RemoveChallengeFromTracking(challenge);

        if (failSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(failSound);
        }

        if (currentDisplayedChallenge == challenge)
        {
            ShowNextChallengeOrHide();
        }
    }

    private void RemoveChallengeFromTracking(ActiveChallenge challenge)
    {
        trackedChallenges.Remove(challenge);
    }

    private void ShowChallenge(ActiveChallenge challenge)
    {
        currentDisplayedChallenge = challenge;
        UpdateChallengeContent(challenge);

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }

        if (notificationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(notificationSound);
        }

        StartCoroutine(FadeIn());
        isPanelVisible = true;
    }

    private void ShowNextChallengeOrHide()
    {
        if (trackedChallenges.Count > 0)
        {
            ShowChallenge(trackedChallenges[0]);
        }
        else
        {
            StartCoroutine(HidePanel());
        }
    }

    private void UpdateChallengeContent(ActiveChallenge challenge)
    {
        if (titleText != null)
        {
            titleText.text = challenge.challengeData.challengeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = challenge.challengeData.description;
        }

        if (difficultyText != null)
        {
            difficultyText.text = challenge.challengeData.GetDifficultyText();
            difficultyText.color = challenge.challengeData.GetDifficultyColor();
        }

        if (rewardText != null)
        {
            rewardText.text = $"+{challenge.challengeData.xpReward} XP";
        }

        if (difficultyImage != null)
        {
            difficultyImage.color = challenge.challengeData.GetDifficultyColor();
        }

        if (challengeIcon != null && challenge.challengeData.challengeIcon != null)
        {
            challengeIcon.sprite = challenge.challengeData.challengeIcon;
            challengeIcon.enabled = true;
        }
        else if (challengeIcon != null)
        {
            challengeIcon.enabled = false;
        }
    }

    private void UpdateChallengeDisplay()
    {
        if (currentDisplayedChallenge == null || currentDisplayedChallenge.challengeData == null)
            return;

        if (progressText != null)
        {
            int totalCount = currentDisplayedChallenge.challengeData.GetEnemyCount();
            if (totalCount == 0)
                totalCount = currentDisplayedChallenge.challengeData.GetCivilianCount();
            progressText.text = $"{currentDisplayedChallenge.currentProgress} / {totalCount}";
        }

        if (progressSlider != null)
        {
            int totalCount = currentDisplayedChallenge.challengeData.GetEnemyCount();
            if (totalCount == 0)
                totalCount = currentDisplayedChallenge.challengeData.GetCivilianCount();
            
            float progress = totalCount > 0 
                ? (float)currentDisplayedChallenge.currentProgress / totalCount 
                : 0f;
            progressSlider.value = progress;
        }

        if (timeRemainingText != null)
        {
            float timeRemaining = currentDisplayedChallenge.GetRemainingTime();
            
            if (timeRemaining > 0)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timeRemainingText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            else if (currentDisplayedChallenge.challengeData.frequency == ChallengeData.ChallengeFrequency.WorldEvent)
            {
                timeRemainingText.text = "Active";
            }
            else
            {
                timeRemainingText.text = "Expired";
            }
        }
    }
    
    private void UpdateDistanceDisplay()
    {
        if (currentDisplayedChallenge == null || distanceText == null)
            return;
            
        if (playerTransform == null)
        {
            FindPlayerTransform();
            if (playerTransform == null)
                return;
        }
        
        float distance = Vector3.Distance(playerTransform.position, currentDisplayedChallenge.position);
        
        if (distance >= 1000f)
        {
            distanceText.text = $"{(distance / 1000f):F1} km";
        }
        else
        {
            distanceText.text = $"{Mathf.RoundToInt(distance)} m";
        }
    }

    private IEnumerator HidePanel()
    {
        yield return StartCoroutine(FadeOut());

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        isPanelVisible = false;
        currentDisplayedChallenge = null;
    }

    private IEnumerator FadeIn()
    {
        if (notificationCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            notificationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        notificationCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (notificationCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            notificationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        notificationCanvasGroup.alpha = 0f;
    }

    public void CycleToNextChallenge()
    {
        if (trackedChallenges.Count <= 1)
            return;

        int currentIndex = trackedChallenges.IndexOf(currentDisplayedChallenge);
        int nextIndex = (currentIndex + 1) % trackedChallenges.Count;
        
        currentDisplayedChallenge = trackedChallenges[nextIndex];
        UpdateChallengeContent(currentDisplayedChallenge);
    }

    public void CycleToPreviousChallenge()
    {
        if (trackedChallenges.Count <= 1)
            return;

        int currentIndex = trackedChallenges.IndexOf(currentDisplayedChallenge);
        int prevIndex = (currentIndex - 1 + trackedChallenges.Count) % trackedChallenges.Count;
        
        currentDisplayedChallenge = trackedChallenges[prevIndex];
        UpdateChallengeContent(currentDisplayedChallenge);
    }

    private void OnDestroy()
    {
        if (challengeManager != null)
        {
            challengeManager.onChallengeSpawned.RemoveListener(OnChallengeSpawned);
            challengeManager.onChallengeProgress.RemoveListener(OnChallengeProgress);
            challengeManager.onChallengeCompleted.RemoveListener(OnChallengeCompleted);
            challengeManager.onChallengeExpired.RemoveListener(OnChallengeExpired);
            challengeManager.onChallengeFailed.RemoveListener(OnChallengeFailed);
        }
    }
}
