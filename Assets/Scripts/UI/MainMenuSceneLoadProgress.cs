using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if MICHSKY_UI_SHIFT
using Michsky.UI.Shift;
#endif
using TMPro;

public class MainMenuSceneLoadProgress : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName = "Apocalypse_GC2";
    [SerializeField] private string targetScenePath = "Assets/Scenes/Apocalypse_GC2.unity";

    [Header("UI")]
    [Tooltip("Optional Shift Slider root. If assigned, loading progress is written to Slider.value (recommended).")]
    [SerializeField] private Slider progressSlider;
    [Tooltip("Optional fill image fallback. Used only when slider is not assigned.")]
    [SerializeField] private Image progressFill;
    [Tooltip("CanvasGroup whose alpha is driven by the modal window animation.")]
    [SerializeField] private CanvasGroup modalCanvasGroup;
#if MICHSKY_UI_SHIFT
    [Tooltip("Modal window manager used to close/blur out after loading completes.")]
    [SerializeField] private ModalWindowManager modalWindowManager;
#endif
    [Tooltip("Optional glow overlay image inside the loading slider track. If null, it is auto-found by name 'Glow Overlay'.")]
    [SerializeField] private Image glowOverlayImage;

    [Header("Glow Pulse")]
    [SerializeField] private bool pulseGlowOverlay = true;
    [SerializeField, Min(0.1f)] private float glowPulseFrequency = 0.9f;
    [SerializeField, Range(0f, 1f)] private float glowPulseAmplitude = 0.08f;

    [Header("Behavior")]
    [SerializeField] private bool enableDebugTrace = true;
    [SerializeField] private bool loadSingleMode = true;
    [SerializeField] private bool preventDuplicateLoads = true;
    [SerializeField] private bool autoStartWhenEnabled = true;
    [SerializeField] private bool waitForModalAlphaOne = true;
    [SerializeField] private bool keepModalAlphaAtOneDuringLoad = true;
    [SerializeField, Min(0f)] private float modalOpenTimeoutSeconds = 3f;
    [SerializeField, Min(0f)] private float minimumVisualProgressSeconds = 6f;
    [SerializeField, Min(0.1f)] private float progressFillSpeed = 0.18f;
    [SerializeField, Min(1f)] private float forceActivationAfterSeconds = 20f;

    [Header("Simple Async Mode")]
    [Tooltip("If enabled, ignore progress UI mechanics: keep panel visible for a fixed duration while loading async, then transition when scene is ready.")]
    [SerializeField] private bool useSimpleAsyncDelayMode = true;
    [SerializeField, Min(0f)] private float simplePanelDisplaySeconds = 5f;

    [Header("Visual-Only Loading")]
    [Tooltip("If true, progress is driven by a smooth timer and not by AsyncOperation.progress.")]
    [SerializeField] private bool useVisualOnlyProgress = true;
    [Tooltip("Seconds for the progress bar to smoothly fill from 0 to 1.")]
    [SerializeField, Min(0.5f)] private float visualLoadDurationSeconds = 3.5f;
    [Tooltip("Small hold at full bar before scene activation for a cleaner visual handoff.")]
    [SerializeField, Min(0f)] private float postFillHoldSeconds = 0.15f;

    [Header("Fade Out")]
    [Tooltip("Seconds to fade the loading panel alpha from 1 to 0 after the scene is ready, before activation.")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;

    [Header("Modal Text")]
    [SerializeField] private bool overrideModalTitleOnLoad = true;
    [SerializeField] private string loadingTitleText = "Loading...";
    [SerializeField] private TMP_Text loadingTitleLabel;

    private Coroutine _loadRoutine;
    private Coroutine _autoStartRoutine;
    private int _autoStartGeneration;
    private bool _isFadingOutForActivation;
    private float _glowBaseAlpha;
    private bool _glowBaseAlphaCaptured;

    private void Awake()
    {
        Trace($"Awake on '{name}'. activeSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}");

        if (modalCanvasGroup == null)
        {
            modalCanvasGroup = GetComponent<CanvasGroup>()
                ?? GetComponentInChildren<CanvasGroup>(includeInactive: true);
        }

#if MICHSKY_UI_SHIFT
        if (modalWindowManager == null)
        {
            modalWindowManager = GetComponent<ModalWindowManager>();
        }
#endif

        if (progressSlider == null)
        {
            if (progressFill != null)
            {
                progressSlider = progressFill.GetComponentInParent<Slider>();
            }

            if (progressSlider == null && modalCanvasGroup != null)
            {
                progressSlider = modalCanvasGroup.GetComponentInChildren<Slider>(includeInactive: true);
            }

            if (progressSlider == null)
            {
                progressSlider = GetComponentInChildren<Slider>(includeInactive: true);
            }
        }

        if (progressFill == null && progressSlider != null && progressSlider.fillRect != null)
        {
            progressFill = progressSlider.fillRect.GetComponent<Image>();
        }

        TryResolveGlowOverlayImage();
        CacheGlowBaseAlpha();

#if MICHSKY_UI_SHIFT
        bool _hasModal = modalWindowManager != null;
#else
        bool _hasModal = false;
#endif
        Trace($"Resolved refs: slider={(progressSlider != null)}, fill={(progressFill != null)}, modalCanvasGroup={(modalCanvasGroup != null)}, modalWindowManager={_hasModal}, glowOverlay={(glowOverlayImage != null)}");

        NormalizeSliderRuntimeSettings();

        SetProgress(0f);
    }

    private void OnEnable()
    {
        _autoStartGeneration++;
        Trace($"OnEnable. autoStartWhenEnabled={autoStartWhenEnabled}, loadRoutineActive={_loadRoutine != null}, autoStartRoutineActive={_autoStartRoutine != null}");

        // Prevent one-frame stale 100% visuals when reopening the loading panel.
        SetProgress(0f);

        if (!autoStartWhenEnabled)
        {
            return;
        }

        if (_loadRoutine != null || _autoStartRoutine != null)
        {
            return;
        }

        _autoStartRoutine = StartCoroutine(AutoStartWhenModalVisibleRoutine(_autoStartGeneration));
    }

    private void OnDisable()
    {
        _autoStartGeneration++;
        Trace("OnDisable. Stopping auto-start routine and restoring glow alpha.");

        if (_autoStartRoutine != null)
        {
            StopCoroutine(_autoStartRoutine);
            _autoStartRoutine = null;
        }

        RestoreGlowAlpha();
    }

    public void BeginLoadApocalypse()
    {
        if (!isActiveAndEnabled)
        {
            Trace("BeginLoadApocalypse ignored because component is not active and enabled.");
            return;
        }

        // Force a deterministic visual start state before any waits begin.
        SetProgress(0f);

        if (_autoStartRoutine != null)
        {
            StopCoroutine(_autoStartRoutine);
            _autoStartRoutine = null;
        }

#if MICHSKY_UI_SHIFT
        bool _hasModalManager = modalWindowManager != null;
#else
        bool _hasModalManager = false;
#endif
        Debug.Log($"[MainMenuSceneLoadProgress] BeginLoadApocalypse called. target='{targetSceneName}', hasSlider={(progressSlider != null)}, hasFill={(progressFill != null)}, hasCanvasGroup={(modalCanvasGroup != null)}, hasModalManager={_hasModalManager}", this);

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("[MainMenuSceneLoadProgress] Target scene name is empty.", this);
            return;
        }

        if (!TryResolveTargetScene(out string resolvedSceneName, out int resolvedBuildIndex))
        {
            Debug.LogError($"[MainMenuSceneLoadProgress] Unable to resolve target scene. name='{targetSceneName}', path='{targetScenePath}'.", this);
            return;
        }

        Trace($"Resolved target scene. resolvedName='{resolvedSceneName}', resolvedBuildIndex={resolvedBuildIndex}");

        if (preventDuplicateLoads && _loadRoutine != null)
        {
            Debug.LogWarning("[MainMenuSceneLoadProgress] Load already in progress. Duplicate trigger ignored.", this);
            return;
        }

        if (overrideModalTitleOnLoad &&
#if MICHSKY_UI_SHIFT
            modalWindowManager != null &&
#endif
            !string.IsNullOrWhiteSpace(loadingTitleText))
        {
            ApplyLoadingTitle();
        }

        _loadRoutine = StartCoroutine(LoadSceneRoutine(resolvedSceneName, resolvedBuildIndex));
    }

    private void ApplyLoadingTitle()
    {
        if (string.IsNullOrWhiteSpace(loadingTitleText))
        {
            return;
        }

#if MICHSKY_UI_SHIFT
        if (modalWindowManager != null)
        {
            modalWindowManager.titleText = loadingTitleText;
            if (modalWindowManager.windowTitle != null)
            {
                modalWindowManager.windowTitle.text = loadingTitleText;
            }
        }
#endif

        if (loadingTitleLabel != null)
        {
            loadingTitleLabel.text = loadingTitleText;
        }
    }

    private IEnumerator AutoStartWhenModalVisibleRoutine(int generation)
    {
        if (modalCanvasGroup == null)
        {
            Debug.LogWarning("[MainMenuSceneLoadProgress] Auto start skipped because modalCanvasGroup is not assigned.", this);
            _autoStartRoutine = null;
            yield break;
        }

        Debug.Log($"[MainMenuSceneLoadProgress] Auto start armed. Waiting for modal alpha=1 on '{gameObject.name}'.", this);

        // If the modal is already visible when this component enables, start on the next frame.
        if (modalCanvasGroup.alpha >= 0.999f)
        {
            yield return null;
            if (generation == _autoStartGeneration && enabled && gameObject.activeInHierarchy)
            {
                Debug.Log("[MainMenuSceneLoadProgress] Modal already open. Triggering auto start immediately.", this);
                BeginLoadApocalypse();
            }

            _autoStartRoutine = null;
            yield break;
        }

        while (generation == _autoStartGeneration && enabled && gameObject.activeInHierarchy)
        {
            if (modalCanvasGroup.alpha >= 0.999f)
            {
                Debug.Log("[MainMenuSceneLoadProgress] Auto start condition met (modal alpha is 1).", this);
                BeginLoadApocalypse();
                _autoStartRoutine = null;
                yield break;
            }

            yield return null;
        }

        _autoStartRoutine = null;
    }

    private IEnumerator LoadSceneRoutine(string resolvedSceneName, int resolvedBuildIndex)
    {
        Debug.Log("[MainMenuSceneLoadProgress] LoadSceneRoutine started.", this);
        _isFadingOutForActivation = false;
        SetProgress(0f);

        Trace($"Load routine state: panelActiveSelf={gameObject.activeSelf}, panelActiveInHierarchy={gameObject.activeInHierarchy}, modalAlpha={(modalCanvasGroup != null ? modalCanvasGroup.alpha.ToString("0.000") : "n/a")}");

        if (waitForModalAlphaOne && modalCanvasGroup != null)
        {
            Debug.Log($"[MainMenuSceneLoadProgress] Waiting for modal alpha to reach 1. Current alpha={modalCanvasGroup.alpha:0.000}", this);
            float timeoutAt = Time.unscaledTime + modalOpenTimeoutSeconds;
            float nextAlphaLogAt = Time.unscaledTime;
            while (modalCanvasGroup.alpha < 0.999f)
            {
                if (Time.unscaledTime >= nextAlphaLogAt)
                {
                    Debug.Log($"[MainMenuSceneLoadProgress] Modal alpha check: {modalCanvasGroup.alpha:0.000}", this);
                    nextAlphaLogAt = Time.unscaledTime + 0.25f;
                }

                if (modalOpenTimeoutSeconds > 0f && Time.unscaledTime >= timeoutAt)
                {
                    Debug.LogWarning($"[MainMenuSceneLoadProgress] Timed out waiting for modal alpha=1. Continuing with alpha={modalCanvasGroup.alpha:0.000}", this);
                    break;
                }

                yield return null;
            }

            if (modalCanvasGroup.alpha >= 0.999f)
            {
                Debug.Log("[MainMenuSceneLoadProgress] Modal alpha reached 1. Starting async scene load.", this);
            }
        }

        LoadSceneMode mode = loadSingleMode ? LoadSceneMode.Single : LoadSceneMode.Additive;
        string sceneLabel = resolvedBuildIndex >= 0 ? $"buildIndex={resolvedBuildIndex}" : $"name='{resolvedSceneName}'";
        Debug.Log($"[MainMenuSceneLoadProgress] Calling LoadSceneAsync({sceneLabel}, mode={mode}).", this);
        AsyncOperation loadOp = resolvedBuildIndex >= 0
            ? SceneManager.LoadSceneAsync(resolvedBuildIndex, mode)
            : SceneManager.LoadSceneAsync(resolvedSceneName, mode);

        if (loadOp == null)
        {
            Debug.LogError($"[MainMenuSceneLoadProgress] Failed to start loading scene ({sceneLabel}).", this);
            _loadRoutine = null;
            yield break;
        }

        // In visual-only mode we gate activation until the fake bar timeline is complete.
        loadOp.allowSceneActivation = !useVisualOnlyProgress;
        Debug.Log($"[MainMenuSceneLoadProgress] Async load started. allowSceneActivation={loadOp.allowSceneActivation}", this);

        if (useSimpleAsyncDelayMode)
        {
            yield return RunSimpleAsyncDelayRoutine(loadOp);

            SetProgress(1f);
            RestoreGlowAlpha();

            Debug.Log($"[MainMenuSceneLoadProgress] Scene load completed. Active scene='{SceneManager.GetActiveScene().name}'.", this);
            _loadRoutine = null;
            yield break;
        }

        float displayedProgress = 0f;
        float loadStartTime = Time.unscaledTime;
        float nextProgressLogAt = Time.unscaledTime;
        bool activationArmed = false;
        float activationAt = 0f;

        while (!loadOp.isDone)
        {
            float elapsed = Time.unscaledTime - loadStartTime;
            bool sceneReadyForActivation = loadOp.progress >= 0.9f;

            if (useVisualOnlyProgress)
            {
                float duration = Mathf.Max(0.01f, Mathf.Max(visualLoadDurationSeconds, minimumVisualProgressSeconds));
                float t = Mathf.Clamp01(elapsed / duration);
                displayedProgress = Mathf.SmoothStep(0f, 1f, t);
                SetProgress(displayedProgress);

                if (!activationArmed)
                {
                    bool timelineDone = t >= 1f;
                    bool forcedByTimeout = forceActivationAfterSeconds > 0f && elapsed >= forceActivationAfterSeconds;

                    if (timelineDone || forcedByTimeout)
                    {
                        activationArmed = true;
                        activationAt = Time.unscaledTime + postFillHoldSeconds;
                        Trace($"Activation armed. timelineDone={timelineDone}, sceneReady={sceneReadyForActivation}, forcedByTimeout={forcedByTimeout}, activateAt={activationAt:0.000}");
                        SetProgress(1f);
                    }
                }

                if (activationArmed && sceneReadyForActivation && !loadOp.allowSceneActivation && Time.unscaledTime >= activationAt)
                {
                    if (fadeOutDuration > 0f && modalCanvasGroup != null)
                    {
                        _isFadingOutForActivation = true;
                        yield return FadeCanvasGroupRoutine(modalCanvasGroup, 0f, fadeOutDuration);
                    }

                    loadOp.allowSceneActivation = true;
                    Trace("allowSceneActivation set to true after visual timeline.");
                }
            }
            else
            {
                // Legacy blended mode: advance by the slower of fake and real progress.
                float fakeTarget = minimumVisualProgressSeconds > 0f
                    ? Mathf.Clamp01(elapsed / minimumVisualProgressSeconds)
                    : 1f;
                float realTarget = Mathf.Clamp01(loadOp.progress);
                float targetProgress = Mathf.Min(fakeTarget, realTarget);
                displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, progressFillSpeed * Time.unscaledDeltaTime);
                SetProgress(displayedProgress);
            }

            if (Time.unscaledTime >= nextProgressLogAt)
            {
                Debug.Log($"[MainMenuSceneLoadProgress] Loading progress raw={loadOp.progress:0.000}, visual={displayedProgress:0.000}, readyForActivation={sceneReadyForActivation}, allowSceneActivation={loadOp.allowSceneActivation}", this);
                nextProgressLogAt = Time.unscaledTime + 0.25f;
            }

            if (keepModalAlphaAtOneDuringLoad && !_isFadingOutForActivation && modalCanvasGroup != null)
            {
                modalCanvasGroup.alpha = 1f;
            }

            UpdateGlowPulse(Time.unscaledTime - loadStartTime);

            yield return null;
        }

        SetProgress(1f);
        RestoreGlowAlpha();

        Debug.Log($"[MainMenuSceneLoadProgress] Scene load completed. Active scene='{SceneManager.GetActiveScene().name}'.", this);
        _loadRoutine = null;
    }

    private IEnumerator RunSimpleAsyncDelayRoutine(AsyncOperation loadOp)
    {
        _isFadingOutForActivation = false;
        loadOp.allowSceneActivation = false;

        float holdUntil = Time.unscaledTime + Mathf.Max(0f, simplePanelDisplaySeconds);
        while (Time.unscaledTime < holdUntil)
        {
            if (keepModalAlphaAtOneDuringLoad && modalCanvasGroup != null)
            {
                modalCanvasGroup.alpha = 1f;
            }

            UpdateGlowPulse(Time.unscaledTime);
            yield return null;
        }

        while (loadOp.progress < 0.9f)
        {
            if (keepModalAlphaAtOneDuringLoad && modalCanvasGroup != null)
            {
                modalCanvasGroup.alpha = 1f;
            }

            UpdateGlowPulse(Time.unscaledTime);
            yield return null;
        }

        if (fadeOutDuration > 0f && modalCanvasGroup != null)
        {
            _isFadingOutForActivation = true;
            yield return FadeCanvasGroupRoutine(modalCanvasGroup, 0f, fadeOutDuration);
        }

        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeCanvasGroupRoutine(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }

    private void SetProgress(float value)
    {
        float clamped = Mathf.Clamp01(value);

        if (progressSlider != null)
        {
            float min = progressSlider.minValue;
            float max = progressSlider.maxValue;
            float sliderValue = Mathf.Lerp(min, max, clamped);
            progressSlider.SetValueWithoutNotify(sliderValue);
        }

        if (progressFill != null && progressFill.type == Image.Type.Filled)
        {
            progressFill.fillAmount = clamped;
        }
    }

    private void NormalizeSliderRuntimeSettings()
    {
        if (progressSlider == null)
        {
            return;
        }

        float range = Mathf.Abs(progressSlider.maxValue - progressSlider.minValue);
        if (progressSlider.wholeNumbers && range <= 1.01f)
        {
            progressSlider.wholeNumbers = false;
            Trace("Disabled wholeNumbers on progress slider because 0-1 range causes visible 0->1 jump.");
        }
    }

    private void TryResolveGlowOverlayImage()
    {
        if (glowOverlayImage != null)
        {
            return;
        }

        if (progressSlider != null && progressSlider.fillRect != null)
        {
            Transform fillArea = progressSlider.fillRect.parent;
            if (fillArea != null)
            {
                Transform glow = fillArea.Find("Glow Overlay");
                if (glow != null)
                {
                    glowOverlayImage = glow.GetComponent<Image>();
                }
            }
        }

        if (glowOverlayImage == null)
        {
            Transform glowByName = transform.Find("Glow Overlay");
            if (glowByName != null)
            {
                glowOverlayImage = glowByName.GetComponent<Image>();
            }
        }
    }

    private void CacheGlowBaseAlpha()
    {
        if (glowOverlayImage == null)
        {
            return;
        }

        _glowBaseAlpha = glowOverlayImage.color.a;
        _glowBaseAlphaCaptured = true;
    }

    private void UpdateGlowPulse(float elapsed)
    {
        if (!pulseGlowOverlay || glowOverlayImage == null)
        {
            return;
        }

        if (!_glowBaseAlphaCaptured)
        {
            CacheGlowBaseAlpha();
        }

        float wave = (Mathf.Sin(elapsed * glowPulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
        float alpha = Mathf.Clamp01(_glowBaseAlpha + ((wave - 0.5f) * 2f * glowPulseAmplitude));

        Color c = glowOverlayImage.color;
        c.a = alpha;
        glowOverlayImage.color = c;
    }

    private void RestoreGlowAlpha()
    {
        if (glowOverlayImage == null || !_glowBaseAlphaCaptured)
        {
            return;
        }

        Color c = glowOverlayImage.color;
        c.a = _glowBaseAlpha;
        glowOverlayImage.color = c;
    }

    private bool TryResolveTargetScene(out string resolvedSceneName, out int resolvedBuildIndex)
    {
        resolvedSceneName = (targetSceneName ?? string.Empty).Trim();
        resolvedBuildIndex = -1;

        if (!string.IsNullOrWhiteSpace(resolvedSceneName) && Application.CanStreamedLevelBeLoaded(resolvedSceneName))
        {
            return true;
        }

        string scenePath = (targetScenePath ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            resolvedBuildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
            if (resolvedBuildIndex >= 0)
            {
                return true;
            }
        }

        return !string.IsNullOrWhiteSpace(resolvedSceneName) && Application.CanStreamedLevelBeLoaded(resolvedSceneName);
    }

    private void Trace(string message)
    {
        if (!enableDebugTrace)
        {
            return;
        }

        Debug.Log($"[MainMenuSceneLoadProgress][Trace] {message}", this);
    }
}
