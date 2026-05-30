using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Serializable]
    private class PhaseContentEntry
    {
        public string phaseName;
        [TextArea] public string tipText;
        public Sprite artwork;
        public AudioClip cue;
    }

    private static Sprite _fallbackSprite;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Progress Bar")]
    [Tooltip("Image set to Image Type: Filled. Fill Amount drives the bar.")]
    [SerializeField] private Image progressBarFill;
    [Tooltip("How fast the bar smoothly catches up to the real progress value.")]
    [SerializeField, Min(0.01f)] private float progressLerpSpeed = 3f;

    [Header("Labels")]
    [SerializeField] private TMP_Text phaseLabel;
    [SerializeField] private TMP_Text percentageLabel;

    [Header("Phase Content")]
    [SerializeField] private TMP_Text tipLabel;
    [SerializeField] private Image phaseArtwork;
    [SerializeField] private string defaultTipText = "Preparing world...";
    [SerializeField] private bool hideArtworkWhenMissing = true;
    [SerializeField] private List<PhaseContentEntry> phaseContent = new List<PhaseContentEntry>();

    [Header("Audio Cues")]
    [SerializeField] private AudioSource cueAudioSource;
    [SerializeField] private bool playPhaseCues = true;
    [SerializeField, Range(0f, 1f)] private float cueVolume = 0.9f;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeInDuration  = 0.3f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;

    [Header("Options")]
    [SerializeField] private bool hideOnStart = true;

    // ── Internal state ─────────────────────────────────────────────────────
    private float _targetProgress;
    private float _displayedProgress;
    private Coroutine _fadeCoroutine;
    private bool _pendingHide;
    private string _lastPhase = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        EnsureProgressBarImageConfigured();
        EnsureCueAudioSourceConfigured();
        TryAutoFixProgressFillRect();

        if (tipLabel != null)
        {
            tipLabel.text = defaultTipText;
        }
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

        if (_pendingHide && _targetProgress >= 0.999f && _displayedProgress >= 0.999f)
        {
            _pendingHide = false;
            SetVisible(false, instant: false);
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
                if (_targetProgress >= 0.999f && _displayedProgress >= 0.999f)
                {
                    SetVisible(false, instant: false);
                }
                else
                {
                    _pendingHide = true;
                    SetVisible(true, instant: false);
                }
                break;

            case SceneFlowManager.FlowState.MainMenu:
                _pendingHide = false;
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

        HandlePhasePresentation(phase);
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

            if (canvas != null)
            {
                canvas.enabled = visible;
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

        if (canvas != null)
        {
            canvas.enabled = becomingVisible;
        }

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
        _pendingHide       = false;
        _lastPhase = string.Empty;

        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (percentageLabel != null) percentageLabel.text       = "0%";
        if (phaseLabel      != null) phaseLabel.text            = "";
        if (tipLabel        != null) tipLabel.text              = defaultTipText;

        if (phaseArtwork != null)
        {
            phaseArtwork.sprite = null;
            phaseArtwork.enabled = !hideArtworkWhenMissing;
        }
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

    private void EnsureProgressBarImageConfigured()
    {
        if (progressBarFill == null)
        {
            return;
        }

        if (progressBarFill.sprite == null)
        {
            progressBarFill.sprite = GetOrCreateFallbackSprite();
        }

        if (progressBarFill.type != Image.Type.Filled)
        {
            progressBarFill.type = Image.Type.Filled;
        }

        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressBarFill.fillClockwise = false;
        progressBarFill.preserveAspect = false;
    }

    private static Sprite GetOrCreateFallbackSprite()
    {
        if (_fallbackSprite != null)
        {
            return _fallbackSprite;
        }

        _fallbackSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f));

        _fallbackSprite.name = "LoadingBarFallbackSprite";
        return _fallbackSprite;
    }

    private void HandlePhasePresentation(string phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
        {
            return;
        }

        if (string.Equals(_lastPhase, phase, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _lastPhase = phase;
        PhaseContentEntry entry = FindPhaseEntry(phase);

        if (tipLabel != null)
        {
            if (entry != null && !string.IsNullOrWhiteSpace(entry.tipText))
            {
                tipLabel.text = entry.tipText;
            }
            else
            {
                tipLabel.text = phase;
            }
        }

        if (phaseArtwork != null)
        {
            if (entry != null && entry.artwork != null)
            {
                phaseArtwork.sprite = entry.artwork;
                phaseArtwork.enabled = true;
            }
            else
            {
                phaseArtwork.sprite = null;
                phaseArtwork.enabled = !hideArtworkWhenMissing;
            }
        }

        if (playPhaseCues && cueAudioSource != null && entry != null && entry.cue != null)
        {
            cueAudioSource.PlayOneShot(entry.cue, cueVolume);
        }
    }

    private PhaseContentEntry FindPhaseEntry(string phase)
    {
        if (phaseContent == null || phaseContent.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < phaseContent.Count; i++)
        {
            PhaseContentEntry entry = phaseContent[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.phaseName))
            {
                continue;
            }

            if (string.Equals(entry.phaseName, phase, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        for (int i = 0; i < phaseContent.Count; i++)
        {
            PhaseContentEntry entry = phaseContent[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.phaseName))
            {
                continue;
            }

            if (phase.IndexOf(entry.phaseName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                entry.phaseName.IndexOf(phase, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return entry;
            }
        }

        return null;
    }

    private void EnsureCueAudioSourceConfigured()
    {
        if (!playPhaseCues)
        {
            return;
        }

        if (cueAudioSource == null)
        {
            cueAudioSource = GetComponent<AudioSource>();
        }

        if (cueAudioSource == null)
        {
            cueAudioSource = gameObject.AddComponent<AudioSource>();
        }

        cueAudioSource.playOnAwake = false;
        cueAudioSource.loop = false;
        cueAudioSource.spatialBlend = 0f;
    }

#if UNITY_EDITOR
    [ContextMenu("Preview: Show Loading Screen")]
    private void PreviewShow() => SetVisible(true,  instant: true);

    [ContextMenu("Preview: Hide Loading Screen")]
    private void PreviewHide() => SetVisible(false, instant: true);
#endif
}
