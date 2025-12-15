using UnityEngine;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }
    
    [Header("Notification Panels")]
    [Tooltip("The default notification panel to use")]
    public NotificationPanel defaultNotificationPanel;
    
    [Tooltip("Additional notification panels for different types")]
    public List<NotificationPanel> notificationPanels = new List<NotificationPanel>();
    
    [Header("Audio Library")]
    [Tooltip("Different notification sounds for different events")]
    public NotificationSoundLibrary soundLibrary;
    
    [Header("Queue Settings")]
    [Tooltip("Should notifications queue if one is already showing?")]
    public bool queueNotifications = true;
    
    [Tooltip("Maximum notifications in queue")]
    public int maxQueueSize = 5;
    
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
    private bool isProcessingQueue = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        if (defaultNotificationPanel == null)
        {
            defaultNotificationPanel = FindFirstObjectByType<NotificationPanel>();
            if (defaultNotificationPanel == null)
            {
                Debug.LogWarning("NotificationManager: No default NotificationPanel found!");
            }
        }
    }
    
    public void ShowNotification(string message)
    {
        ShowNotification(message, null, 0f, defaultNotificationPanel);
    }
    
    public void ShowNotification(string message, AudioClip sound)
    {
        ShowNotification(message, sound, 0f, defaultNotificationPanel);
    }
    
    public void ShowNotification(string message, float duration)
    {
        ShowNotification(message, null, duration, defaultNotificationPanel);
    }
    
    public void ShowNotification(string message, AudioClip sound, float duration)
    {
        ShowNotification(message, sound, duration, defaultNotificationPanel);
    }
    
    public void ShowNotification(string message, AudioClip sound, float duration, NotificationPanel panel)
    {
        if (panel == null)
        {
            panel = defaultNotificationPanel;
        }
        
        if (panel == null)
        {
            Debug.LogError("NotificationManager: No notification panel available!");
            return;
        }
        
        NotificationData data = new NotificationData
        {
            message = message,
            sound = sound,
            duration = duration > 0 ? duration : panel.displayDuration,
            panel = panel
        };
        
        if (queueNotifications && panel.IsShowing())
        {
            if (notificationQueue.Count < maxQueueSize)
            {
                notificationQueue.Enqueue(data);
                if (!isProcessingQueue)
                {
                    StartCoroutine(ProcessQueue());
                }
            }
            else
            {
                Debug.LogWarning("NotificationManager: Queue is full, notification dropped!");
            }
        }
        else
        {
            DisplayNotification(data);
        }
    }
    
    private void DisplayNotification(NotificationData data)
    {
        if (data.sound != null)
        {
            data.panel.ShowNotification(data.message, data.sound, data.duration);
        }
        else
        {
            data.panel.ShowNotification(data.message, data.duration);
        }
    }
    
    private System.Collections.IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;
        
        while (notificationQueue.Count > 0)
        {
            yield return new WaitUntil(() => !defaultNotificationPanel.IsShowing());
            
            NotificationData data = notificationQueue.Dequeue();
            DisplayNotification(data);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        isProcessingQueue = false;
    }
    
    public void ShowMissionNotification(string message)
    {
        AudioClip sound = soundLibrary != null ? soundLibrary.missionSound : null;
        ShowNotification(message, sound);
    }
    
    public void ShowLevelUpNotification(string message)
    {
        AudioClip sound = soundLibrary != null ? soundLibrary.levelUpSound : null;
        ShowNotification(message, sound);
    }
    
    public void ShowItemNotification(string message)
    {
        AudioClip sound = soundLibrary != null ? soundLibrary.itemSound : null;
        ShowNotification(message, sound);
    }
    
    public void ShowWarningNotification(string message)
    {
        AudioClip sound = soundLibrary != null ? soundLibrary.warningSound : null;
        ShowNotification(message, sound);
    }
    
    public void ClearQueue()
    {
        notificationQueue.Clear();
    }
    
    private struct NotificationData
    {
        public string message;
        public AudioClip sound;
        public float duration;
        public NotificationPanel panel;
    }
}

[System.Serializable]
public class NotificationSoundLibrary
{
    [Header("Event Sounds")]
    public AudioClip missionSound;
    public AudioClip levelUpSound;
    public AudioClip itemSound;
    public AudioClip warningSound;
    public AudioClip achievementSound;
    public AudioClip combatSound;
}
