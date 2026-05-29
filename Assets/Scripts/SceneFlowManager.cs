using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    public event Action<FlowState> StateChanged;
    public event Action<string, float> LoadingProgressChanged;

    public enum FlowState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Transitioning
    }

    [Serializable]
    public class SceneReference
    {
        [SerializeField] private string sceneName;

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif

        public string SceneName => sceneName;
        public bool IsAssigned => !string.IsNullOrWhiteSpace(sceneName);

#if UNITY_EDITOR
        public void SyncFromAsset()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
        }
#endif
    }

    public static SceneFlowManager Instance { get; private set; }

    [Header("Core")]
    [SerializeField] private MultiSceneBootstrapper gameplayBootstrapper;
    [SerializeField] private SceneReference mainMenuScene;

    [Header("Optional UI Roots")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject gameplayMenuRoot;

    [Header("Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private bool autoFindBootstrapper = true;

    private bool isTransitioning;
    private string currentLoadingPhase = string.Empty;
    private float currentLoadingProgress;

    public FlowState CurrentState { get; private set; } = FlowState.MainMenu;
    public string CurrentLoadingPhase => currentLoadingPhase;
    public float CurrentLoadingProgress => currentLoadingProgress;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        EnsureBootstrapperReference();
        HookBootstrapperEvents();
        ValidateConfiguredScenes();
        ApplyMenuVisibility();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookBootstrapperEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartNewGame()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[SceneFlowManager] Transition already in progress.", this);
            return;
        }

        EnsureBootstrapperReference();
        if (gameplayBootstrapper == null)
        {
            Debug.LogError("[SceneFlowManager] Missing gameplayBootstrapper reference.", this);
            return;
        }

        if (!ValidateConfiguredScenes())
        {
            return;
        }

        Time.timeScale = 1f;
        isTransitioning = true;
        SetState(FlowState.Transitioning);
        ApplyMenuVisibility();
        gameplayBootstrapper.BeginLoadSequence();
    }

    public void ReturnToMainMenu()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[SceneFlowManager] Transition already in progress.", this);
            return;
        }

        if (!mainMenuScene.IsAssigned)
        {
            Debug.LogError("[SceneFlowManager] Assign mainMenuScene in the inspector.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuScene.SceneName))
        {
            Debug.LogError($"[SceneFlowManager] Main menu scene '{mainMenuScene.SceneName}' is not in Build Settings or cannot be loaded.", this);
            return;
        }

        isTransitioning = true;
        SetState(FlowState.Transitioning);
        ApplyMenuVisibility();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene.SceneName, LoadSceneMode.Single);
    }

    public void OnPlayPressed()
    {
        StartNewGame();
    }

    public void OnResumePressed()
    {
        SetPaused(false);
    }

    public void OnPausePressed()
    {
        SetPaused(true);
    }

    public void OnQuitToMenuPressed()
    {
        ReturnToMainMenu();
    }

    public void OnQuitGamePressed()
    {
        QuitGame();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TogglePause()
    {
        if (CurrentState == FlowState.Playing)
        {
            SetPaused(true);
            return;
        }

        if (CurrentState == FlowState.Paused)
        {
            SetPaused(false);
        }
    }

    public void SetPaused(bool isPaused)
    {
        if (CurrentState != FlowState.Playing && CurrentState != FlowState.Paused)
        {
            return;
        }

        SetState(isPaused ? FlowState.Paused : FlowState.Playing);
        Time.timeScale = isPaused ? 0f : 1f;
        ApplyMenuVisibility();
    }

    private void OnBootstrapSequenceStarted()
    {
        currentLoadingPhase = "Starting";
        currentLoadingProgress = 0f;
        SetState(FlowState.Loading);
        ApplyMenuVisibility();
    }

    private void OnBootstrapSequenceCompleted()
    {
        isTransitioning = false;
        currentLoadingPhase = "Complete";
        currentLoadingProgress = 1f;
        SetState(FlowState.Playing);
        Time.timeScale = 1f;
        ApplyMenuVisibility();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isTransitioning)
        {
            return;
        }

        if (mainMenuScene.IsAssigned && string.Equals(scene.name, mainMenuScene.SceneName, StringComparison.Ordinal))
        {
            isTransitioning = false;
            SetState(FlowState.MainMenu);
            ApplyMenuVisibility();
        }

        if (autoFindBootstrapper && gameplayBootstrapper == null)
        {
            EnsureBootstrapperReference();
            HookBootstrapperEvents();
        }
    }

    private void EnsureBootstrapperReference()
    {
        if (gameplayBootstrapper != null || !autoFindBootstrapper)
        {
            return;
        }

        gameplayBootstrapper = FindFirstObjectByType<MultiSceneBootstrapper>();
    }

    private void HookBootstrapperEvents()
    {
        if (gameplayBootstrapper == null)
        {
            return;
        }

        gameplayBootstrapper.SequenceStarted -= OnBootstrapSequenceStarted;
        gameplayBootstrapper.SequenceCompleted -= OnBootstrapSequenceCompleted;
        gameplayBootstrapper.LoadingProgressChanged -= OnBootstrapLoadingProgressChanged;
        gameplayBootstrapper.SequenceStarted += OnBootstrapSequenceStarted;
        gameplayBootstrapper.SequenceCompleted += OnBootstrapSequenceCompleted;
        gameplayBootstrapper.LoadingProgressChanged += OnBootstrapLoadingProgressChanged;
    }

    private void UnhookBootstrapperEvents()
    {
        if (gameplayBootstrapper == null)
        {
            return;
        }

        gameplayBootstrapper.SequenceStarted -= OnBootstrapSequenceStarted;
        gameplayBootstrapper.SequenceCompleted -= OnBootstrapSequenceCompleted;
        gameplayBootstrapper.LoadingProgressChanged -= OnBootstrapLoadingProgressChanged;
    }

    private void OnBootstrapLoadingProgressChanged(string phase, float progress)
    {
        currentLoadingPhase = phase;
        currentLoadingProgress = Mathf.Clamp01(progress);
        LoadingProgressChanged?.Invoke(phase, progress);
    }

    private bool ValidateConfiguredScenes()
    {
        bool valid = true;

        if (!mainMenuScene.IsAssigned)
        {
            Debug.LogError("[SceneFlowManager] Assign mainMenuScene in the inspector.", this);
            valid = false;
        }
        else if (!Application.CanStreamedLevelBeLoaded(mainMenuScene.SceneName))
        {
            Debug.LogError($"[SceneFlowManager] Main menu scene '{mainMenuScene.SceneName}' is not in Build Settings or cannot be loaded.", this);
            valid = false;
        }

        return valid;
    }

    private void SetState(FlowState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }

    private void ApplyMenuVisibility()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(CurrentState == FlowState.MainMenu);
        }

        if (gameplayMenuRoot != null)
        {
            gameplayMenuRoot.SetActive(CurrentState == FlowState.Paused);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        mainMenuScene?.SyncFromAsset();
    }
#endif
}