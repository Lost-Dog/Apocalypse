using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bridges ShiftUI buttons to the async scene loader without touching any ShiftUI script.
/// Optionally queues a mission to auto-start once the gameplay scene initializes.
/// Lives on the "Modal Windows" scene GameObject.
/// </summary>
public class LoadingPanelBridge : MonoBehaviour
{
    [SerializeField] private bool enableDebugTrace = true;

    [Tooltip("Drives the async load and progress bar. Auto-found if left empty.")]
    [SerializeField] private MainMenuSceneLoadProgress loader;

    [Header("Direct Load Fallback")]
    [Tooltip("When false, fallback force-load is skipped while a loader is assigned. Enable only as a recovery path.")]
    [SerializeField] private bool enableDirectLoadFallback = false;
    [Tooltip("Scene name to force-load if the UI loader does not transition in time.")]
    [SerializeField] private string fallbackSceneName = "Apocalypse_GC2";
    [Tooltip("Scene path used to resolve build index fallback if name-based loading fails.")]
    [SerializeField] private string fallbackScenePath = "Assets/Scenes/Apocalypse_GC2.unity";
    [Tooltip("Seconds to wait before forcing a direct scene load.")]
    [SerializeField, Min(0.5f)] private float fallbackDelaySeconds = 5f;

    [Header("Mission Request (Optional)")]
    [Tooltip("Name of the MissionData asset (MissionData.missionName) to auto-start when the gameplay scene loads. Leave empty for free-roam / no mission.")]
    [SerializeField] private string missionToQueue;

    private Coroutine fallbackRoutine;

    private void Awake()
    {
        Trace($"Awake on '{name}'. loader preassigned={(loader != null)}");

        if (loader == null)
            loader = GetComponentInChildren<MainMenuSceneLoadProgress>(includeInactive: true)
                  ?? FindFirstObjectByType<MainMenuSceneLoadProgress>(FindObjectsInactive.Include);

        if (loader == null)
            Debug.LogError("[LoadingPanelBridge] No MainMenuSceneLoadProgress found. Assign it in the Inspector.", this);
        else
            Trace($"Loader resolved: '{loader.name}', activeSelf={loader.gameObject.activeSelf}, activeInHierarchy={loader.gameObject.activeInHierarchy}");
    }

    /// <summary>
    /// Called by a UI button's onClick. Loads the gameplay scene without queuing any mission
    /// (free-roam / whatever the inspector field specifies as default).
    /// </summary>
    public void StartLoading()
    {
        Trace("StartLoading invoked from UI.");
        QueueMissionAndLoad(missionToQueue);
    }

    /// <summary>
    /// Called by a UI button's onClick to load the gameplay scene and auto-start a specific mission.
    /// Pass the exact value of <see cref="MissionData.missionName"/> for the desired mission.
    /// </summary>
    public void StartLoadingWithMission(string missionName)
    {
        Trace($"StartLoadingWithMission invoked from UI. missionName='{missionName}'");
        QueueMissionAndLoad(missionName);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void QueueMissionAndLoad(string missionName)
    {
        Trace($"QueueMissionAndLoad start. incomingMission='{missionName}', fallbackDelay={fallbackDelaySeconds:0.00}s");

        if (!string.IsNullOrWhiteSpace(missionName))
        {
            MissionRequest.PendingMissionName = missionName;
            Debug.Log($"[LoadingPanelBridge] Mission queued: '{missionName}'", this);
        }
        else
        {
            MissionRequest.Clear();
        }

        if (loader != null)
        {
            // Ensure the Loading GameObject is active so coroutines can run on it.
            Trace($"Activating loader object '{loader.gameObject.name}' and calling BeginLoadApocalypse().");
            loader.gameObject.SetActive(true);
            loader.BeginLoadApocalypse();
        }
        else
        {
            Trace("Loader is null at QueueMissionAndLoad; waiting for fallback path.");
        }

        bool shouldRunFallback = loader == null || enableDirectLoadFallback;
        if (!shouldRunFallback)
        {
            Trace("Fallback watcher skipped (loader is present and enableDirectLoadFallback is false).");
            return;
        }

        if (fallbackRoutine != null)
        {
            StopCoroutine(fallbackRoutine);
        }

        fallbackRoutine = StartCoroutine(FallbackLoadRoutine());
    }

    private System.Collections.IEnumerator FallbackLoadRoutine()
    {
        Trace($"FallbackLoadRoutine started. sceneName='{fallbackSceneName}', scenePath='{fallbackScenePath}'");

        float startTime = Time.unscaledTime;
        float nextTickAt = Time.unscaledTime;
        while (Time.unscaledTime - startTime < fallbackDelaySeconds)
        {
            if (IsTargetSceneLoaded())
            {
                Trace("Fallback watcher: target scene detected as loaded. Exiting fallback watcher.");
                fallbackRoutine = null;
                yield break;
            }

            if (enableDebugTrace && Time.unscaledTime >= nextTickAt)
            {
                float elapsed = Time.unscaledTime - startTime;
                Trace($"Fallback watcher tick. elapsed={elapsed:0.00}s / {fallbackDelaySeconds:0.00}s, activeScene='{SceneManager.GetActiveScene().name}'");
                nextTickAt = Time.unscaledTime + 1f;
            }

            yield return null;
        }

        if (!IsTargetSceneLoaded())
        {
            ForceFallbackLoad();
        }

        fallbackRoutine = null;
        Trace("FallbackLoadRoutine finished.");
    }

    private bool IsTargetSceneLoaded()
    {
        string sceneName = (fallbackSceneName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            string scenePath = (fallbackScenePath ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(scenePath))
            {
                sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        return SceneManager.GetActiveScene().name == sceneName || SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    private void ForceFallbackLoad()
    {
        Trace("ForceFallbackLoad invoked.");

        string scenePath = (fallbackScenePath ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
            if (buildIndex >= 0)
            {
                Debug.LogWarning($"[LoadingPanelBridge] Fallback triggered. Forcing load by build index {buildIndex} (path '{scenePath}').", this);
                SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
                return;
            }
        }

        string sceneName = (fallbackSceneName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"[LoadingPanelBridge] Fallback failed. Scene name/path are both empty or unresolved. name='{fallbackSceneName}', path='{fallbackScenePath}'", this);
            return;
        }

        Debug.LogWarning($"[LoadingPanelBridge] Fallback triggered. Forcing load by name '{sceneName}'.", this);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void Trace(string message)
    {
        if (!enableDebugTrace)
        {
            return;
        }

        Debug.Log($"[LoadingPanelBridge][Trace] {message}", this);
    }
}
