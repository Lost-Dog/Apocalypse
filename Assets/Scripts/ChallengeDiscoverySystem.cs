using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Handles challenge discovery through proximity detection
/// Attach to player or create as separate proximity detector
/// </summary>
public class ChallengeDiscoverySystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float discoveryCheckInterval = 1f;
    [SerializeField] private float discoveryRange = 100f;
    
    [Header("UI")]
    [SerializeField] private ChallengePreviewUI previewUI;
    [SerializeField] private GameObject interactPrompt;
    
    private ChallengeManager challengeManager;
    private float checkTimer;
    private ActiveChallenge nearestChallenge;
    private ActiveChallenge _lastPromptChallenge;
    private HashSet<ActiveChallenge> discoveredChallenges = new HashSet<ActiveChallenge>();
    private TextMeshProUGUI _promptText;
    
    private void Start()
    {
        challengeManager = ChallengeManager.Instance;
        checkTimer = discoveryCheckInterval;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
            _promptText = interactPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }
    }
    
    private void Update()
    {
        if (challengeManager == null)
            return;
        
        // Periodically check for nearby challenges
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            CheckForNearbyChallenges();
            checkTimer = discoveryCheckInterval;
        }
        
        // Handle interaction input
        if (nearestChallenge != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChallengePreview(nearestChallenge);
        }
    }
    
    private void CheckForNearbyChallenges()
    {
        nearestChallenge = null;
        float nearestDistance = float.MaxValue;
        
        // Check all discovered challenges
        var challenges = challengeManager.GetDiscoveredChallenges();
        
        foreach (var challenge in challenges)
        {
            float distance = Vector3.Distance(transform.position, challenge.position);
            
            if (distance <= discoveryRange && distance < nearestDistance)
            {
                nearestChallenge = challenge;
                nearestDistance = distance;
                
                // First time discovering this challenge?
                if (!discoveredChallenges.Contains(challenge))
                {
                    discoveredChallenges.Add(challenge);
                    OnChallengeDiscovered(challenge);
                }
            }
        }
        
        // Also check failed challenges that can be retried
        var failedChallenges = challengeManager.GetRetryableChallenges();
        
        foreach (var challenge in failedChallenges)
        {
            float distance = Vector3.Distance(transform.position, challenge.position);
            
            if (distance <= discoveryRange && distance < nearestDistance)
            {
                nearestChallenge = challenge;
                nearestDistance = distance;
            }
        }
        
        // Update interact prompt
        UpdateInteractPrompt();
    }
    
    private void OnChallengeDiscovered(ActiveChallenge challenge)
    {
        Debug.Log($"[DISCOVERY] Found challenge: {challenge.challengeData.challengeName}");
        
        // Show discovery notification
        if (challengeManager != null)
        {
            challengeManager.NotifyChallengeDiscovered(challenge);
        }
        
        // Optional: Auto-open preview on first discovery
        // OpenChallengePreview(challenge);
    }
    
    private void OpenChallengePreview(ActiveChallenge challenge)
    {
        if (previewUI != null)
        {
            previewUI.ShowPreview(challenge);
        }
        else
        {
            Debug.LogWarning("ChallengePreviewUI not assigned");
        }
    }
    
    private void UpdateInteractPrompt()
    {
        if (interactPrompt == null)
            return;

        bool shouldShow = nearestChallenge != null;

        if (interactPrompt.activeSelf != shouldShow)
            interactPrompt.SetActive(shouldShow);

        // Only rebuild the string when the nearest challenge actually changes.
        if (shouldShow && nearestChallenge != _lastPromptChallenge)
        {
            _lastPromptChallenge = nearestChallenge;
            if (_promptText != null)
            {
                string action = nearestChallenge.state == ActiveChallenge.ChallengeState.Failed ? "Retry" : "View";
                _promptText.text = $"[E] {action} Challenge: {nearestChallenge.challengeData.challengeName}";
            }
        }
    }
    
    /// <summary>
    /// Get nearest discovered challenge (for external systems)
    /// </summary>
    public ActiveChallenge GetNearestChallenge()
    {
        return nearestChallenge;
    }
    
    /// <summary>
    /// Manually trigger discovery check
    /// </summary>
    public void ForceDiscoveryCheck()
    {
        CheckForNearbyChallenges();
    }
}
