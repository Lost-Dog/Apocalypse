using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Progress Bar")]
    [Tooltip("Image set to Image Type: Filled. Fill Amount drives the bar.")]
    [SerializeField] private Image progressBarFill;
    [Tooltip("How fast the bar smoothly catches up to the real progress value.")]
    [SerializeField, Min(0.01f)] private float progressLerpSpeed = 3f;

    [Header("Labels")]
    [SerializeField] private TMP_Text phaseLabel;
    [SerializeField] private TMP_Text percentageLabel;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeInDuration  = 0.3f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;

    [Header("Options")]
    [SerializeField] private bool hideOnStart = true;

    // ── Internal state ─────────────────────────────────────────────────────
    private float _targetProgress;
    private float _displayedProgress;
    private Coroutine _fadeCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        TryAutoFixProgressFillRect();
    }

    private void Start()
    {
        if (hideOnStart)
        {
            SetVisible(false, instant: true);
        }

        SubscribeToSceneFlow();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSceneFlow();
    }

    private void Update()
    {
        if (progressBarFill == null) return;

        _displayedProgress = Mathf.MoveTowards(
            _displayedProgress,
            _targetProgress,
            progressLerpSpeed * Time.unscaledDeltaTime);

        progressBarFill.fillAmount = _displayedProgress;

        if (percentageLabel != null)
        {
            percentageLabel.text = $"{Mathf.RoundToInt(_displayedProgress * 100f)}%";
        }
    }

    // ── Subscriptions ─────────────────────────────────────────────────────
    private void SubscribeToSceneFlow()
    {
        if (SceneFlowManager.Instance == null)
        {
            // SceneFlowManager may not exist in this scene yet — listen for it next frame.
            StartCoroutine(WaitForSceneFlowManager());
            return;
        }

        Hook(SceneFlowManager.Instance);
    }

    private IEnumerator WaitForSceneFlowManager()
    {
        while (SceneFlowManager.Instance == null)
        {
            yield return null;
        }

        Hook(SceneFlowManager.Instance);
    }

    private void Hook(SceneFlowManager manager)
    {
        manager.StateChanged            -= OnStateChanged;
        manager.LoadingProgressChanged  -= OnProgressChanged;
        manager.StateChanged            += OnStateChanged;
        manager.LoadingProgressChanged  += OnProgressChanged;

        // Sync current state/progress immediately in case this UI scene loaded late.
        OnStateChanged(manager.CurrentState);
        OnProgressChanged(manager.CurrentLoadingPhase, manager.CurrentLoadingProgress);
    }

    private void UnsubscribeFromSceneFlow()
    {
        if (SceneFlowManager.Instance == null) return;

        SceneFlowManager.Instance.StateChanged           -= OnStateChanged;
        SceneFlowManager.Instance.LoadingProgressChanged -= OnProgressChanged;
    }

    // ── Event handlers ────────────────────────────────────────────────────
    private void OnStateChanged(SceneFlowManager.FlowState state)
    {
        switch (state)
        {
            case SceneFlowManager.FlowState.Loading:
            case SceneFlowManager.FlowState.Transitioning:
                if (state == SceneFlowManager.FlowState.Transitioning)
                {
                    ResetProgress();
                }

                SetVisible(true, instant: false);
                break;

            case SceneFlowManager.FlowState.Playing:
            case SceneFlowManager.FlowState.MainMenu:
                SetVisible(false, instant: false);
                break;
        }
    }

    private void OnProgressChanged(string phase, float progress)
    {
        _targetProgress = progress;

        if (phaseLabel != null)
        {
            phaseLabel.text = phase;
        }
    }

    // ── Visibility ────────────────────────────────────────────────────────
    private void SetVisible(bool visible, bool instant)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        if (instant || canvasGroup == null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = visible ? 1f : 0f;
                canvasGroup.interactable   = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            return;
        }

        float duration = visible ? fadeInDuration : fadeOutDuration;
        float target   = visible ? 1f : 0f;
        _fadeCoroutine = StartCoroutine(FadeRoutine(target, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed    = 0f;

        // Immediately block/unblock raycasts at the start of the fade direction.
        bool becomingVisible = targetAlpha > 0.5f;
        if (becomingVisible)
        {
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (!becomingVisible)
        {
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }

        _fadeCoroutine = null;
    }

    private void ResetProgress()
    {
        _targetProgress    = 0f;
        _displayedProgress = 0f;

        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (percentageLabel != null) percentageLabel.text       = "0%";
        if (phaseLabel      != null) phaseLabel.text            = "";
    }

    private void TryAutoFixProgressFillRect()
    {
        if (progressBarFill == null)
        {
            return;
        }

        RectTransform fillRect = progressBarFill.rectTransform;
        if (fillRect == null)
        {
            return;
        }

        // Older setup versions anchored X max to 0, producing a zero-width fill image.
        if (Mathf.Approximately(fillRect.anchorMin.x, 0f) &&
            Mathf.Approximately(fillRect.anchorMax.x, 0f))
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Preview: Show Loading Screen")]
    private void PreviewShow() => SetVisible(true,  instant: true);

    [ContextMenu("Preview: Hide Loading Screen")]
    private void PreviewHide() => SetVisible(false, instant: true);
#endif
}
