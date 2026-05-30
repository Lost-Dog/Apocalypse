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

    [Header("Load Order")]
    [SerializeField] private SceneReference loadingScene;
    [SerializeField] private SceneReference environmentScene;
    [SerializeField] private SceneReference environmentEffectsScene;
    [SerializeField] private SceneReference gameplayObjectsScene;

    [Header("Options")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool setGameplaySceneActive = true;
    [SerializeField] private bool setEnvironmentEffectsSceneActiveForLighting = true;
    [SerializeField] private bool destroyBootstrapperWhenFinished = true;
    [SerializeField, Min(0f)] private float minimumSecondsPerScene = 3f;
    [SerializeField, Min(0f)] private float minimumLoadingScreenSeconds = 0f;
    [SerializeField, Min(0f)] private float maxWaitForEnvironmentReadySeconds = 30f;
    [SerializeField] private bool useTemporaryLoadingCamera = true;

    private bool isRunning;
    private string currentPhase = "Idle";
    private GameObject temporaryCameraObject;
    private bool environmentEffectsReady;

    public bool IsRunning => isRunning;
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
        environmentEffectsReady = false;
        EnsureTemporaryLoadingCamera();
        SequenceStarted?.Invoke();
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadSequenceRoutine());
    }

    private IEnumerator LoadSequenceRoutine()
    {
        float sequenceStartTime = Time.realtimeSinceStartup;
        const int totalSteps = 4;

        yield return LoadSceneWithProgress(
            loadingScene.SceneName,
            LoadSceneMode.Single,
            "Loading Scene",
            stepIndex: 0,
            totalSteps,
            skipIfAlreadyLoaded: false);

        yield return LoadSceneWithProgress(
            environmentScene.SceneName,
            LoadSceneMode.Additive,
            "Environment Scene",
            stepIndex: 1,
            totalSteps,
            skipIfAlreadyLoaded: true);

        yield return LoadSceneWithProgress(
            environmentEffectsScene.SceneName,
            LoadSceneMode.Additive,
            "Environmental Effects Scene",
            stepIndex: 2,
            totalSteps,
            skipIfAlreadyLoaded: true);

        yield return LoadSceneWithProgress(
            gameplayObjectsScene.SceneName,
            LoadSceneMode.Additive,
            "Gameplay Objects Scene",
            stepIndex: 3,
            totalSteps,
            skipIfAlreadyLoaded: true);

        if (setEnvironmentEffectsSceneActiveForLighting)
        {
            Scene effectsScene = SceneManager.GetSceneByName(environmentEffectsScene.SceneName);
            if (effectsScene.IsValid() && effectsScene.isLoaded)
            {
                SceneManager.SetActiveScene(effectsScene);
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                Debug.LogWarning($"[MultiSceneBootstrapper] Could not set active scene for lighting override: '{environmentEffectsScene.SceneName}'.", this);
            }
        }
        else if (setGameplaySceneActive)
        {
            Scene gameplayScene = SceneManager.GetSceneByName(gameplayObjectsScene.SceneName);
            if (gameplayScene.IsValid() && gameplayScene.isLoaded)
            {
                SceneManager.SetActiveScene(gameplayScene);
            }
            else
            {
                Debug.LogWarning($"[MultiSceneBootstrapper] Could not set active scene: '{gameplayObjectsScene.SceneName}'.", this);
            }
        }

        // Wait for the scene-local ready signal from the environment effects scene.
        ReportLoadingProgress("Environment Effects Ready", 1f);
        yield return WaitForEnvironmentEffectsReady();

        float elapsed = Time.realtimeSinceStartup - sequenceStartTime;
        if (elapsed < minimumLoadingScreenSeconds)
        {
            yield return new WaitForSecondsRealtime(minimumLoadingScreenSeconds - elapsed);
        }

        isRunning = false;
        ReportLoadingProgress("Complete", 1f);
        SequenceCompleted?.Invoke();
        yield return UnloadSceneIfLoaded(loadingScene.SceneName);
        LogLoadedScenesAtCompletion();

        DestroyTemporaryLoadingCamera();

        if (destroyBootstrapperWhenFinished)
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
            yield return HoldStepForMinimumDuration(phaseLabel, stepIndex, totalSteps, stepStartTime);
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
            float elapsed = Time.realtimeSinceStartup - stepStartTime;
            float opProgress = op.progress >= 0.9f ? 1f : Mathf.Clamp01(op.progress / 0.9f);
            float timedProgress = GetTimedStepProgress(elapsed);
            float stepProgress = minimumSecondsPerScene > 0f ? Mathf.Min(opProgress, timedProgress) : opProgress;
            float combinedProgress = (stepIndex + stepProgress) / totalSteps;
            ReportLoadingProgress(phaseLabel, combinedProgress);
            yield return null;
        }

        yield return HoldStepForMinimumDuration(phaseLabel, stepIndex, totalSteps, stepStartTime);

        ReportLoadingProgress(phaseLabel, (stepIndex + 1f) / totalSteps);
    }

    private IEnumerator HoldStepForMinimumDuration(string phaseLabel, int stepIndex, int totalSteps, float stepStartTime)
    {
        if (minimumSecondsPerScene <= 0f)
        {
            yield break;
        }

        while (Time.realtimeSinceStartup - stepStartTime < minimumSecondsPerScene)
        {
            float elapsed = Time.realtimeSinceStartup - stepStartTime;
            float timedProgress = GetTimedStepProgress(elapsed);
            float combinedProgress = (stepIndex + timedProgress) / totalSteps;
            ReportLoadingProgress(phaseLabel, combinedProgress);
            yield return null;
        }
    }

    private IEnumerator UnloadSceneIfLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            yield break;
        }

        EnsureActiveSceneIsNot(sceneName);

        // Give Unity one frame to apply active-scene changes before unloading.
        yield return null;

        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(scene);
        if (unloadOperation == null)
        {
            unloadOperation = SceneManager.UnloadSceneAsync(sceneName);
        }

        if (unloadOperation == null)
        {
            Debug.LogWarning($"[MultiSceneBootstrapper] Failed to unload loading scene '{sceneName}'.", this);
            yield break;
        }

        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        Scene postUnloadScene = SceneManager.GetSceneByName(sceneName);
        if (postUnloadScene.IsValid() && postUnloadScene.isLoaded)
        {
            Debug.LogWarning($"[MultiSceneBootstrapper] Loading scene '{sceneName}' is still loaded after unload attempt.", this);
        }
    }

    private void EnsureActiveSceneIsNot(string sceneName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !string.Equals(activeScene.name, sceneName, StringComparison.Ordinal))
        {
            return;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene candidate = SceneManager.GetSceneAt(i);
            if (!candidate.IsValid() || !candidate.isLoaded)
            {
                continue;
            }

            if (string.Equals(candidate.name, sceneName, StringComparison.Ordinal))
            {
                continue;
            }

            SceneManager.SetActiveScene(candidate);
            return;
        }

        Debug.LogWarning($"[MultiSceneBootstrapper] Could not switch active scene away from '{sceneName}' before unload.", this);
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

    private float GetTimedStepProgress(float elapsed)
    {
        if (minimumSecondsPerScene <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(elapsed / minimumSecondsPerScene);
    }

    private bool ValidateConfiguration()
    {
        bool valid = true;

        valid &= ValidateSceneReference(loadingScene, nameof(loadingScene));
        valid &= ValidateSceneReference(environmentScene, nameof(environmentScene));
        valid &= ValidateSceneReference(environmentEffectsScene, nameof(environmentEffectsScene));
        valid &= ValidateSceneReference(gameplayObjectsScene, nameof(gameplayObjectsScene));

        if (valid)
        {
            valid &= ValidateSceneAvailableToLoad(loadingScene.SceneName, nameof(loadingScene));
            valid &= ValidateSceneAvailableToLoad(environmentScene.SceneName, nameof(environmentScene));
            valid &= ValidateSceneAvailableToLoad(environmentEffectsScene.SceneName, nameof(environmentEffectsScene));
            valid &= ValidateSceneAvailableToLoad(gameplayObjectsScene.SceneName, nameof(gameplayObjectsScene));
        }

        return valid;
    }

    public void NotifyEnvironmentEffectsReady(string sceneName)
    {
        if (!string.Equals(sceneName, environmentEffectsScene.SceneName, StringComparison.Ordinal))
        {
            return;
        }

        environmentEffectsReady = true;
        Debug.Log($"[MultiSceneBootstrapper] Received environment effects ready signal from '{sceneName}'.", this);
    }

    private IEnumerator WaitForEnvironmentEffectsReady()
    {
        if (environmentEffectsReady)
        {
            yield break;
        }

        float startTime = Time.realtimeSinceStartup;
        while (!environmentEffectsReady)
        {
            if (maxWaitForEnvironmentReadySeconds > 0f &&
                Time.realtimeSinceStartup - startTime >= maxWaitForEnvironmentReadySeconds)
            {
                Debug.LogWarning($"[MultiSceneBootstrapper] Timed out waiting {maxWaitForEnvironmentReadySeconds:0.##}s for environment effects ready signal from '{environmentEffectsScene.SceneName}'. Continuing load sequence.", this);
                yield break;
            }

            yield return null;
        }
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

    private void EnsureTemporaryLoadingCamera()
    {
        if (!useTemporaryLoadingCamera || temporaryCameraObject != null)
        {
            return;
        }

        temporaryCameraObject = new GameObject("TemporaryLoadingCamera");
        DontDestroyOnLoad(temporaryCameraObject);

        Camera tempCamera = temporaryCameraObject.AddComponent<Camera>();
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = Color.black;
        tempCamera.cullingMask = 0;
        tempCamera.depth = -100f;
    }

    private void DestroyTemporaryLoadingCamera()
    {
        if (temporaryCameraObject == null)
        {
            return;
        }

        Destroy(temporaryCameraObject);
        temporaryCameraObject = null;
    }

    private void OnDestroy()
    {
        DestroyTemporaryLoadingCamera();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        loadingScene?.SyncFromAsset();
        environmentScene?.SyncFromAsset();
        environmentEffectsScene?.SyncFromAsset();
        gameplayObjectsScene?.SyncFromAsset();
    }
#endif
}