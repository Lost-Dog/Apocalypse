using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fadeImage;

    [Header("Fade")]
    [SerializeField] private float fadeToBlackDuration = 0.2f;
    [SerializeField] private float fadeFromBlackDuration = 0.25f;
    [SerializeField] private bool startTransparent = true;

    [Header("Behavior")]
    [SerializeField] private bool blockRaycastsDuringFade = true;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (fadeImage != null)
        {
            var color = fadeImage.color;
            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            fadeImage.color = color;
        }

        if (canvasGroup != null && startTransparent)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForSceneFlowManager());
    }

    private void OnDestroy()
    {
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.StateChanged -= OnStateChanged;
    }

    private IEnumerator WaitForSceneFlowManager()
    {
        while (SceneFlowManager.Instance == null)
            yield return null;

        SceneFlowManager.Instance.StateChanged += OnStateChanged;
        OnStateChanged(SceneFlowManager.Instance.CurrentState);
    }

    private void OnStateChanged(SceneFlowManager.FlowState state)
    {
        switch (state)
        {
            case SceneFlowManager.FlowState.Loading:
            case SceneFlowManager.FlowState.Transitioning:
                FadeTo(1f, fadeToBlackDuration, interactableWhenDone: blockRaycastsDuringFade);
                break;

            case SceneFlowManager.FlowState.Playing:
            case SceneFlowManager.FlowState.MainMenu:
            case SceneFlowManager.FlowState.Paused:
                FadeTo(0f, fadeFromBlackDuration, interactableWhenDone: false);
                break;
        }
    }

    private void FadeTo(float targetAlpha, float duration, bool interactableWhenDone)
    {
        if (canvasGroup == null) return;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, interactableWhenDone));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool interactableWhenDone)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = blockRaycastsDuringFade;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = interactableWhenDone;
            canvasGroup.blocksRaycasts = interactableWhenDone;
            _fadeRoutine = null;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = interactableWhenDone;
        canvasGroup.blocksRaycasts = interactableWhenDone;
        _fadeRoutine = null;
    }

    [ContextMenu("Preview: Fade To Black")]
    private void PreviewFadeToBlack()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    [ContextMenu("Preview: Fade To Clear")]
    private void PreviewFadeToClear()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
