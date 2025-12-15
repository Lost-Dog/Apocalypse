using UnityEngine;
using TMPro;
using System.Collections;

public class MessageDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    
    [Header("Auto-Setup")]
    public bool autoSetup = true;
    
    private Coroutine currentMessageCoroutine;
    
    private void Start()
    {
        if (autoSetup)
        {
            if (messageText == null)
            {
                messageText = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        
        currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }
    
    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        yield return StartCoroutine(FadeIn());
        
        yield return new WaitForSeconds(duration);
        
        yield return StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
}
