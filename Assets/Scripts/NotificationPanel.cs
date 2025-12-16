using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class NotificationPanel : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("How long the notification stays visible")]
    public float displayDuration = 3f;
    
    [Tooltip("Should the panel start disabled?")]
    public bool startDisabled = false;
    
    [Tooltip("Keep the GameObject always active (only control visibility via CanvasGroup alpha)")]
    public bool keepAlwaysActive = true;
    
    [Header("References")]
    [Tooltip("The text component to display the notification message")]
    public TextMeshProUGUI messageText;
    
    [Header("Audio")]
    [Tooltip("Audio source for playing notification sounds")]
    public AudioSource audioSource;
    
    [Tooltip("Default notification sound")]
    public AudioClip defaultNotificationSound;
    
    [Header("Animation (Optional)")]
    [Tooltip("Animator for panel animations (fade in/out, slide, etc.)")]
    public Animator panelAnimator;
    
    [Tooltip("Name of the show animation trigger")]
    public string showTrigger = "Show";
    
    [Tooltip("Name of the hide animation trigger")]
    public string hideTrigger = "Hide";
    
    [Header("Events")]
    public UnityEvent<string> onNotificationShown;
    public UnityEvent onNotificationHidden;
    
    private Coroutine hideCoroutine;
    private bool isShowing = false;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && keepAlwaysActive)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (startDisabled)
        {
            HideNotification();
        }
        
        if (keepAlwaysActive)
        {
            gameObject.SetActive(true);
        }
    }
    
    public void ShowNotification(string message)
    {
        ShowNotification(message, defaultNotificationSound);
    }
    
    public void ShowNotification(string message, AudioClip notificationSound)
    {
        ShowNotification(message, notificationSound, displayDuration);
    }
    
    public void ShowNotification(string message, float customDuration)
    {
        ShowNotification(message, defaultNotificationSound, customDuration);
    }
    
    public void ShowNotification(string message, AudioClip notificationSound, float customDuration)
    {
        if (keepAlwaysActive)
        {
            gameObject.SetActive(true);
        }
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false; // Don't block raycasts - only UI elements should
        }
        
        isShowing = true;
        
        if (panelAnimator != null && !string.IsNullOrEmpty(showTrigger))
        {
            panelAnimator.SetTrigger(showTrigger);
        }
        
        PlayNotificationSound(notificationSound);
        
        onNotificationShown?.Invoke(message);
        
        hideCoroutine = StartCoroutine(HideAfterDelay(customDuration));
    }
    
    private void PlayNotificationSound(AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
    
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotification();
    }
    
    public void HideNotification()
    {
        if (!isShowing) return;
        
        if (panelAnimator != null && !string.IsNullOrEmpty(hideTrigger))
        {
            panelAnimator.SetTrigger(hideTrigger);
        }
        
        if (messageText != null)
        {
            messageText.text = "";
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        isShowing = false;
        onNotificationHidden?.Invoke();
    }
    
    public void ShowSimpleMessage(string message)
    {
        ShowNotification(message);
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
}
