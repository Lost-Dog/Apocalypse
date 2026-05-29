using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitGameButton;

    [Header("Optional")]
    [SerializeField] private TMP_Text headerLabel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration  = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;

    private Coroutine _fadeCoroutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetupButtons();
    }

    private void Start()
    {
        SetCanvasVisible(false, instant: true);
        StartCoroutine(WaitForSceneFlowManager());
    }

    private void OnDestroy()
    {
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.StateChanged -= OnStateChanged;
    }

    // ── Button wiring ─────────────────────────────────────────────────────────

    private void SetupButtons()
    {
        if (resumeButton     != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (settingsButton   != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);
        if (quitGameButton   != null) quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }

    private void OnResumeClicked()
    {
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.OnResumePressed();
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void OnQuitToMenuClicked()
    {
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.OnQuitToMenuPressed();
    }

    private void OnQuitGameClicked()
    {
        if (SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.OnQuitGamePressed();
    }

    // ── SceneFlowManager subscription ─────────────────────────────────────────

    private IEnumerator WaitForSceneFlowManager()
    {
        while (SceneFlowManager.Instance == null)
            yield return null;

        SceneFlowManager.Instance.StateChanged += OnStateChanged;
        // Sync to current state immediately
        OnStateChanged(SceneFlowManager.Instance.CurrentState);
    }

    private void OnStateChanged(SceneFlowManager.FlowState state)
    {
        if (state == SceneFlowManager.FlowState.Paused)
        {
            // Close settings sub-panel when pausing anew
            if (settingsPanel != null) settingsPanel.SetActive(false);
            SetCanvasVisible(true);
        }
        else
        {
            SetCanvasVisible(false);
        }
    }

    // ── Fade helpers ──────────────────────────────────────────────────────────

    private void SetCanvasVisible(bool visible, bool instant = false)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (instant)
        {
            canvasGroup.alpha          = visible ? 1f : 0f;
            canvasGroup.interactable   = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeRoutine(visible ? 1f : 0f,
                                                         visible ? fadeInDuration : fadeOutDuration,
                                                         visible));
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool interactableWhenDone)
    {
        float start   = canvasGroup.alpha;
        float elapsed = 0f;

        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = interactableWhenDone; // block during fade-in too

        while (elapsed < duration)
        {
            elapsed              += Time.unscaledDeltaTime;
            canvasGroup.alpha    =  Mathf.Lerp(start, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha        = targetAlpha;
        canvasGroup.interactable = interactableWhenDone;
        _fadeCoroutine           = null;
    }

    // ── Editor preview ────────────────────────────────────────────────────────

    [ContextMenu("Preview: Show Pause Menu")]
    private void PreviewShow()
    {
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true; }
    }

    [ContextMenu("Preview: Hide Pause Menu")]
    private void PreviewHide()
    {
        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; }
    }
}
