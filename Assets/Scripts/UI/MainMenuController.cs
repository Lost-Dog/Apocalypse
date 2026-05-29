using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text versionLabel;

    [Header("Settings Panel")]
    [Tooltip("Optional panel toggled by the Settings button.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeInDuration  = 0.4f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.3f;

    [Header("Options")]
    [SerializeField] private bool showVersionOnStart = true;

    private Coroutine _fadeCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        RegisterButtons();
        SetupVersionLabel();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        SubscribeToSceneFlow();
    }

    private void OnDestroy()
    {
        UnregisterButtons();
        UnsubscribeFromSceneFlow();
    }

    // ── Button wiring ─────────────────────────────────────────────────────
    private void RegisterButtons()
    {
        if (playButton     != null) playButton.onClick.AddListener(OnPlayClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void UnregisterButtons()
    {
        if (playButton     != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
        if (quitButton     != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // ── Button handlers ────────────────────────────────────────────────────
    private void OnPlayClicked()
    {
        SetButtonsInteractable(false);

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.OnPlayPressed();
        }
        else
        {
            Debug.LogError("[MainMenuController] SceneFlowManager not found.", this);
            SetButtonsInteractable(true);
        }
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void OnQuitClicked()
    {
        SetButtonsInteractable(false);

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.OnQuitGamePressed();
        }
        else
        {
            Application.Quit();
        }
    }

    // ── SceneFlowManager integration ───────────────────────────────────────
    private void SubscribeToSceneFlow()
    {
        if (SceneFlowManager.Instance == null)
        {
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
        manager.StateChanged -= OnStateChanged;
        manager.StateChanged += OnStateChanged;

        // Reflect initial state in case we're already past MainMenu.
        OnStateChanged(manager.CurrentState);
    }

    private void UnsubscribeFromSceneFlow()
    {
        if (SceneFlowManager.Instance == null) return;
        SceneFlowManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(SceneFlowManager.FlowState state)
    {
        bool shouldShow = state == SceneFlowManager.FlowState.MainMenu;
        SetVisible(shouldShow);

        if (shouldShow)
        {
            SetButtonsInteractable(true);
        }
    }

    // ── Visibility & fade ──────────────────────────────────────────────────
    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        float duration = visible ? fadeInDuration : fadeOutDuration;
        _fadeCoroutine = StartCoroutine(FadeRoutine(visible ? 1f : 0f, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed    = 0f;

        bool becomingVisible = targetAlpha > 0.5f;
        if (becomingVisible)
        {
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (elapsed < duration)
        {
            elapsed          += Time.unscaledDeltaTime;
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

    // ── Helpers ────────────────────────────────────────────────────────────
    private void SetButtonsInteractable(bool interactable)
    {
        if (playButton     != null) playButton.interactable     = interactable;
        if (settingsButton != null) settingsButton.interactable = interactable;
        if (quitButton     != null) quitButton.interactable     = interactable;
    }

    private void SetupVersionLabel()
    {
        if (versionLabel == null) return;
        versionLabel.text    = showVersionOnStart ? $"v{Application.version}" : string.Empty;
        versionLabel.gameObject.SetActive(showVersionOnStart);
    }

#if UNITY_EDITOR
    [ContextMenu("Preview: Show Menu")]
    private void PreviewShow() { if (canvasGroup) { canvasGroup.alpha = 1f; canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true; } }

    [ContextMenu("Preview: Hide Menu")]
    private void PreviewHide() { if (canvasGroup) { canvasGroup.alpha = 0f; canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; } }
#endif
}
