using UnityEngine;

public class NotificationIntegrationHelper : MonoBehaviour
{
    [Header("Notification System")]
    public NotificationPanel notificationPanel;
    
    [Header("Audio Clips")]
    public AudioClip levelUpSound;
    public AudioClip xpGainSound;
    public AudioClip missionCompleteSound;
    public AudioClip itemPickupSound;
    public AudioClip achievementSound;
    
    [Header("Settings")]
    public bool showXPGainNotifications = true;
    public bool showLevelUpNotifications = true;
    public bool showMissionNotifications = true;
    
    [Header("References (Auto-Find)")]
    public ProgressionManager progressionManager;
    
    private void Start()
    {
        if (notificationPanel == null)
        {
            notificationPanel = FindFirstObjectByType<NotificationPanel>();
        }
        
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<ProgressionManager>();
        }
        
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        if (progressionManager != null)
        {
            progressionManager.onLevelUp.AddListener(OnLevelUp);
            progressionManager.onXPGained.AddListener(OnXPGained);
        }
        else
        {
            Debug.LogWarning("NotificationIntegrationHelper: ProgressionManager not found!");
        }
    }
    
    private void OnLevelUp(int newLevel)
    {
        if (!showLevelUpNotifications || notificationPanel == null) return;
        
        string message = $"LEVEL {newLevel} REACHED!";
        notificationPanel.ShowNotification(message, levelUpSound, 4f);
    }
    
    private void OnXPGained(int amount)
    {
        if (!showXPGainNotifications || notificationPanel == null) return;
        
        if (amount >= 50)
        {
            string message = $"+{amount} XP";
            notificationPanel.ShowNotification(message, xpGainSound, 2f);
        }
    }
    
    public void ShowMissionComplete(string missionName)
    {
        if (!showMissionNotifications || notificationPanel == null) return;
        
        string message = $"{missionName}\nCOMPLETE";
        notificationPanel.ShowNotification(message, missionCompleteSound, 4f);
    }
    
    public void ShowItemPickup(string itemName, int quantity = 1)
    {
        if (notificationPanel == null) return;
        
        string message = quantity > 1 
            ? $"{itemName} x{quantity}" 
            : itemName;
        notificationPanel.ShowNotification(message, itemPickupSound, 2.5f);
    }
    
    public void ShowAchievement(string achievementName)
    {
        if (notificationPanel == null) return;
        
        string message = $"ACHIEVEMENT UNLOCKED\n{achievementName}";
        notificationPanel.ShowNotification(message, achievementSound, 5f);
    }
    
    public void ShowCustomNotification(string message, float duration = 3f)
    {
        if (notificationPanel == null) return;
        
        notificationPanel.ShowNotification(message, duration);
    }
}
