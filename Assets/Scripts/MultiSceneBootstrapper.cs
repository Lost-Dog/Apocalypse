using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneBootstrapper : MonoBehaviour
{
    public event Action SequenceStarted;
    public event Action SequenceCompleted;
    public event Action<string, float> LoadingProgressChanged;

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

    [Header("Startup")]
    [SerializeField] private SceneReference startupScene;

    [Header("Options")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool destroyBootstrapperWhenFinished = true;

    private bool isRunning;
    private bool hasCompletedInitialLoad;
    private string currentPhase = "Idle";

    public bool IsRunning => isRunning;
    public bool HasCompletedInitialLoad => hasCompletedInitialLoad;
    public string GameplayObjectsSceneName => startupScene != null && startupScene.IsAssigned
        ? startupScene.SceneName
        : string.Empty;
    public string CurrentPhase => currentPhase;

    private void Start()
    {
        if (runOnStart)
        {
            BeginLoadSequence();
        }
    }

    public void BeginLoadSequence()
    {
        if (isRunning)
        {
            Debug.LogWarning("[MultiSceneBootstrapper] Load sequence is already running.", this);
            return;
        }

        if (!ValidateConfiguration())
        {
            return;
        }

        isRunning = true;
        hasCompletedInitialLoad = false;
        SequenceStarted?.Invoke();
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadSequenceRoutine());
    }

    private IEnumerator LoadSequenceRoutine()
    {
        yield return LoadSceneWithProgress(
            startupScene.SceneName,
            LoadSceneMode.Single,
            "Startup Scene",
            stepIndex: 0,
            totalSteps: 1,
            skipIfAlreadyLoaded: false);

        isRunning = false;
        hasCompletedInitialLoad = true;
        ReportLoadingProgress("Complete", 1f);
        SequenceCompleted?.Invoke();

        LogLoadedScenesAtCompletion();

        bool destroyBootstrapper = destroyBootstrapperWhenFinished;

        if (destroyBootstrapper)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator LoadSceneWithProgress(
        string sceneName,
        LoadSceneMode loadMode,
        string phaseLabel,
        int stepIndex,
        int totalSteps,
        bool skipIfAlreadyLoaded)
    {
        float stepStartTime = Time.realtimeSinceStartup;

        if (skipIfAlreadyLoaded && SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            ReportLoadingProgress(phaseLabel, (stepIndex + 1f) / totalSteps);
            yield break;
        }

        float baseProgress = Mathf.Clamp01((float)stepIndex / totalSteps);
        ReportLoadingProgress(phaseLabel, baseProgress);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, loadMode);
        if (op == null)
        {
            Debug.LogError($"[MultiSceneBootstrapper] Failed to start {loadMode} load for '{sceneName}'.");
            yield break;
        }

        while (!op.isDone)
        {
            float opProgress = op.progress >= 0.9f ? 1f : Mathf.Clamp01(op.progress / 0.9f);
            float combinedProgress = (stepIndex + opProgress) / totalSteps;
            ReportLoadingProgress(phaseLabel, combinedProgress);
            yield return null;
        }

        ReportLoadingProgress(phaseLabel, (stepIndex + 1f) / totalSteps);
    }

    private void LogLoadedScenesAtCompletion()
    {
        int sceneCount = SceneManager.sceneCount;
        Scene activeScene = SceneManager.GetActiveScene();
        string activeSceneName = activeScene.IsValid() ? activeScene.name : "<invalid>";

        string[] loadedScenes = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            loadedScenes[i] = $"{i}:{scene.name}(loaded={scene.isLoaded})";
        }

        Debug.Log($"[MultiSceneBootstrapper] Completion scene snapshot | active='{activeSceneName}' | loaded=[{string.Join(", ", loadedScenes)}]", this);
    }

    private bool ValidateConfiguration()
    {
        bool valid = true;

        valid &= ValidateSceneReference(startupScene, nameof(startupScene));

        if (valid)
        {
            valid &= ValidateSceneAvailableToLoad(startupScene.SceneName, nameof(startupScene));
        }

        return valid;
    }

    private bool ValidateSceneAvailableToLoad(string sceneName, string fieldName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return true;
        }

        Debug.LogError($"[MultiSceneBootstrapper] Scene '{sceneName}' from '{fieldName}' is not in Build Settings or cannot be loaded.", this);
        return false;
    }

    private void ReportLoadingProgress(string phase, float progress)
    {
        currentPhase = phase;
        LoadingProgressChanged?.Invoke(phase, Mathf.Clamp01(progress));
    }

    private bool ValidateSceneReference(SceneReference reference, string fieldName)
    {
        if (reference != null && reference.IsAssigned)
        {
            return true;
        }

        Debug.LogError($"[MultiSceneBootstrapper] Missing scene assignment for '{fieldName}'.", this);
        return false;
    }

    private void OnDestroy()
    {
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        startupScene?.SyncFromAsset();
    }
#endif
}