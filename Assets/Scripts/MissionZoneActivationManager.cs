using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if COMPASS_NAVIGATOR_PRO
using CompassNavigatorPro;
#endif

/// <summary>
/// Keeps MissionZone GameObjects disabled until their linked mission/challenge becomes active.
/// Works with zones that are already disabled in the scene hierarchy.
/// </summary>
[DefaultExecutionOrder(-200)]
public class MissionZoneActivationManager : MonoBehaviour
{
    private static MissionZoneActivationManager instance;

    private sealed class ZoneTransformState
    {
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;
        public bool captured;
    }

    [Header("Behavior")]
    [Tooltip("When enabled, this manager toggles the MissionZone GameObject active state.")]
    [SerializeField] private bool manageZoneGameObjectActiveState = true;

    [Tooltip("Fallback refresh interval (seconds) in case events are missed.")]
    [SerializeField] private float refreshInterval = 0.5f;

    [Tooltip("Logs when a zone is auto-enabled or auto-disabled.")]
    [SerializeField] private bool logZoneActivationChanges = false;

#if COMPASS_NAVIGATOR_PRO
    [Header("Kronnect Active Zone Marker")]
    [Tooltip("Creates a dynamic Kronnect POI at the currently active mission/challenge zone.")]
    [SerializeField] private bool useKronnectActiveZoneMarker = true;

    [Tooltip("Optional icon override for the active zone POI.")]
    [SerializeField] private Sprite kronnectMarkerIcon;

    [Tooltip("Tint color for the active zone POI icon/indicator.")]
    [SerializeField] private Color kronnectMarkerTint = Color.red;

    [Tooltip("Enable heartbeat effect on the active zone POI.")]
    [SerializeField] private bool kronnectHeartbeatEnabled = true;

    [Tooltip("Distance where heartbeat starts for the active zone POI.")]
    [SerializeField] private float kronnectHeartbeatDistance = 80f;

    [Tooltip("Show on-screen indicator for active zone POI.")]
    [SerializeField] private bool kronnectShowOnScreenIndicator = true;

    [Tooltip("Show off-screen indicator for active zone POI.")]
    [SerializeField] private bool kronnectShowOffScreenIndicator = true;
#endif

    private readonly List<MissionZone> zones = new List<MissionZone>();
    private readonly Dictionary<MissionZone, ZoneTransformState> zoneStates = new Dictionary<MissionZone, ZoneTransformState>();

    private MissionManager missionManager;
    private ChallengeManager challengeManager;

    private bool missionEventsBound;
    private bool challengeEventsBound;
    private float refreshTimer;

#if COMPASS_NAVIGATOR_PRO
    private GameObject kronnectMarkerObject;
    private CompassProPOI kronnectMarkerPOI;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        MissionZoneActivationManager existing = FindObjectOfType<MissionZoneActivationManager>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject managerObject = new GameObject("MissionZoneActivationManager");
        instance = managerObject.AddComponent<MissionZoneActivationManager>();
        DontDestroyOnLoad(managerObject);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        RebuildZoneCache();
        BindManagers();
        ReconcileZoneStates();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindManagerEvents();
#if COMPASS_NAVIGATOR_PRO
        DestroyKronnectMarker();
#endif
    }

    private void Update()
    {
        BindManagers();

        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            ReconcileZoneStates();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebuildZoneCache();
        BindManagers();
        ReconcileZoneStates();
    }

    private void BindManagers()
    {
        if (missionManager == null)
        {
            missionManager = FindObjectOfType<MissionManager>();
        }

        if (challengeManager == null)
        {
            challengeManager = ChallengeManager.Instance;
            if (challengeManager == null)
            {
                challengeManager = FindObjectOfType<ChallengeManager>();
            }
        }

        if (!missionEventsBound && missionManager != null)
        {
            missionManager.onMissionStart.AddListener(HandleMissionChanged);
            missionManager.onMissionComplete.AddListener(HandleMissionChanged);
            missionManager.onMissionFail.AddListener(HandleMissionChanged);
            missionEventsBound = true;
        }

        if (!challengeEventsBound && challengeManager != null)
        {
            challengeManager.onChallengeStarted.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeCompleted.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeFailed.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeExpired.AddListener(HandleChallengeChanged);
            challengeEventsBound = true;
        }
    }

    private void UnbindManagerEvents()
    {
        if (missionEventsBound && missionManager != null)
        {
            missionManager.onMissionStart.RemoveListener(HandleMissionChanged);
            missionManager.onMissionComplete.RemoveListener(HandleMissionChanged);
            missionManager.onMissionFail.RemoveListener(HandleMissionChanged);
            missionEventsBound = false;
        }

        if (challengeEventsBound && challengeManager != null)
        {
            challengeManager.onChallengeStarted.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeCompleted.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeFailed.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeExpired.RemoveListener(HandleChallengeChanged);
            challengeEventsBound = false;
        }
    }

    private void HandleMissionChanged(MissionData mission)
    {
        ReconcileZoneStates();
    }

    private void HandleChallengeChanged(ActiveChallenge challenge)
    {
        ReconcileZoneStates();
    }

    private void RebuildZoneCache()
    {
        zones.Clear();
        zoneStates.Clear();

        MissionZone[] foundZones = FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < foundZones.Length; i++)
        {
            MissionZone zone = foundZones[i];
            if (zone == null) continue;
            zones.Add(zone);
            CaptureZoneState(zone);
        }
    }

    private void ReconcileZoneStates()
    {
        if (!manageZoneGameObjectActiveState) return;

        if (zones.Count == 0)
        {
            RebuildZoneCache();
        }

        MissionZone firstActiveZone = null;

        for (int i = zones.Count - 1; i >= 0; i--)
        {
            MissionZone zone = zones[i];
            if (zone == null)
            {
                zones.RemoveAt(i);
                continue;
            }

            bool shouldBeActive = ShouldZoneBeActive(zone);
            GameObject zoneObject = zone.gameObject;

            if (shouldBeActive && firstActiveZone == null)
            {
                firstActiveZone = zone;
            }

            if (shouldBeActive)
            {
                if (TryGetMatchingActiveChallenge(zone, out ActiveChallenge activeChallenge))
                {
                    ApplyActiveChallengeTransform(zone, activeChallenge);
                }
                else
                {
                    RestoreZoneTransform(zone);
                }
            }
            else
            {
                RestoreZoneTransform(zone);
            }

            if (zoneObject.activeSelf == shouldBeActive) continue;

            zoneObject.SetActive(shouldBeActive);

            if (logZoneActivationChanges)
            {
                string zoneLabel = string.IsNullOrWhiteSpace(zone.zoneName) ? zoneObject.name : zone.zoneName;
                Debug.Log($"[MissionZoneActivationManager] {(shouldBeActive ? "Enabled" : "Disabled")} zone '{zoneLabel}'", zoneObject);
            }
        }

#if COMPASS_NAVIGATOR_PRO
        UpdateKronnectMarker(firstActiveZone);
#endif
    }

#if COMPASS_NAVIGATOR_PRO
    private void UpdateKronnectMarker(MissionZone activeZone)
    {
        if (!useKronnectActiveZoneMarker)
        {
            DestroyKronnectMarker();
            return;
        }

        if (activeZone == null)
        {
            DestroyKronnectMarker();
            return;
        }

        EnsureKronnectMarker();
        if (kronnectMarkerObject == null || kronnectMarkerPOI == null)
        {
            return;
        }

        Transform zoneTransform = activeZone.transform;
        kronnectMarkerObject.transform.position = zoneTransform.position;

        kronnectMarkerPOI.title = string.IsNullOrWhiteSpace(activeZone.zoneName) ? activeZone.gameObject.name : activeZone.zoneName;
        kronnectMarkerPOI.visibility = POIVisibility.AlwaysVisible;
        kronnectMarkerPOI.miniMapVisibility = POIVisibility.AlwaysVisible;
        kronnectMarkerPOI.iconShowDistance = true;
        kronnectMarkerPOI.onScreenIndicatorShowDistance = true;
        kronnectMarkerPOI.showOnScreenIndicator = kronnectShowOnScreenIndicator;
        kronnectMarkerPOI.showOffScreenIndicator = kronnectShowOffScreenIndicator;
        kronnectMarkerPOI.heartbeatEnabled = kronnectHeartbeatEnabled;
        kronnectMarkerPOI.heartbeatDistance = Mathf.Max(1f, kronnectHeartbeatDistance);
        kronnectMarkerPOI.tintColor = kronnectMarkerTint;
        kronnectMarkerPOI.canBeVisited = false;
        kronnectMarkerPOI.hideWhenVisited = false;

        if (kronnectMarkerIcon != null)
        {
            kronnectMarkerPOI.iconNonVisited = kronnectMarkerIcon;
            kronnectMarkerPOI.iconVisited = kronnectMarkerIcon;
        }

        if (CompassPro.instance != null)
        {
            kronnectMarkerPOI.RegisterPOI();
        }
    }

    private void EnsureKronnectMarker()
    {
        if (kronnectMarkerObject != null && kronnectMarkerPOI != null)
        {
            return;
        }

        if (kronnectMarkerObject == null)
        {
            kronnectMarkerObject = new GameObject("GC2_ActiveZone_KronnectPOI");
            kronnectMarkerPOI = kronnectMarkerObject.AddComponent<CompassProPOI>();
        }
        else if (kronnectMarkerPOI == null)
        {
            kronnectMarkerPOI = kronnectMarkerObject.GetComponent<CompassProPOI>();
            if (kronnectMarkerPOI == null)
            {
                kronnectMarkerPOI = kronnectMarkerObject.AddComponent<CompassProPOI>();
            }
        }

        kronnectMarkerPOI.priority = 999;
        kronnectMarkerPOI.ignoreAreaOfInterest = true;
        kronnectMarkerPOI.iconScale = 1f;
        kronnectMarkerPOI.miniMapIconScale = 1f;
    }

    private void DestroyKronnectMarker()
    {
        if (kronnectMarkerObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(kronnectMarkerObject);
        }
        else
        {
            DestroyImmediate(kronnectMarkerObject);
        }

        kronnectMarkerObject = null;
        kronnectMarkerPOI = null;
    }
#endif

    private bool ShouldZoneBeActive(MissionZone zone)
    {
        if (zone == null) return false;

        MissionData activeMission = missionManager != null ? missionManager.activeMission : null;
        if (activeMission != null)
        {
            if (zone.linkedMissionData != null)
            {
                if (zone.linkedMissionData == activeMission)
                {
                    return true;
                }
            }
            else if (zone.autoMatchMissionByName)
            {
                string activeMissionKey = NormalizeName(activeMission.missionName);
                string zoneKey = NormalizeName(zone.zoneName);

                if (string.IsNullOrEmpty(zoneKey))
                {
                    zoneKey = NormalizeName(zone.gameObject.name);
                }

                if (!string.IsNullOrEmpty(activeMissionKey) && !string.IsNullOrEmpty(zoneKey) && activeMissionKey == zoneKey)
                {
                    return true;
                }

                if (zone.linkedChallengeData != null)
                {
                    string challengeKey = NormalizeName(zone.linkedChallengeData.challengeName);
                    if (!string.IsNullOrEmpty(challengeKey) && activeMissionKey == challengeKey)
                    {
                        return true;
                    }
                }
            }
        }

        if (zone.linkedChallengeData == null || challengeManager == null || challengeManager.activeChallenges == null)
        {
            return false;
        }

        string linkedChallengeKey = NormalizeName(zone.linkedChallengeData.challengeName);

        for (int i = 0; i < challengeManager.activeChallenges.Count; i++)
        {
            ActiveChallenge activeChallenge = challengeManager.activeChallenges[i];
            if (activeChallenge == null || activeChallenge.challengeData == null) continue;
            if (activeChallenge.state != ActiveChallenge.ChallengeState.Active) continue;

            if (activeChallenge.challengeData == zone.linkedChallengeData)
            {
                return true;
            }

            string activeChallengeKey = NormalizeName(activeChallenge.challengeData.challengeName);
            if (!string.IsNullOrEmpty(linkedChallengeKey) && linkedChallengeKey == activeChallengeKey)
            {
                return true;
            }
        }

        return false;
    }

    private void CaptureZoneState(MissionZone zone)
    {
        if (zone == null || zoneStates.ContainsKey(zone)) return;

        zoneStates[zone] = new ZoneTransformState
        {
            originalPosition = zone.transform.position,
            originalRotation = zone.transform.rotation,
            originalScale = zone.transform.localScale,
            captured = true
        };
    }

    private bool TryGetMatchingActiveChallenge(MissionZone zone, out ActiveChallenge matchingChallenge)
    {
        matchingChallenge = null;

        if (zone == null || zone.linkedChallengeData == null || challengeManager == null || challengeManager.activeChallenges == null)
        {
            return false;
        }

        string linkedChallengeKey = NormalizeName(zone.linkedChallengeData.challengeName);

        for (int i = 0; i < challengeManager.activeChallenges.Count; i++)
        {
            ActiveChallenge activeChallenge = challengeManager.activeChallenges[i];
            if (activeChallenge == null || activeChallenge.challengeData == null) continue;
            if (activeChallenge.state != ActiveChallenge.ChallengeState.Active) continue;

            if (activeChallenge.challengeData == zone.linkedChallengeData)
            {
                matchingChallenge = activeChallenge;
                return true;
            }

            string activeChallengeKey = NormalizeName(activeChallenge.challengeData.challengeName);
            if (!string.IsNullOrEmpty(linkedChallengeKey) && linkedChallengeKey == activeChallengeKey)
            {
                matchingChallenge = activeChallenge;
                return true;
            }
        }

        return false;
    }

    private void ApplyActiveChallengeTransform(MissionZone zone, ActiveChallenge activeChallenge)
    {
        if (zone == null || activeChallenge == null) return;
        if (!zoneStates.TryGetValue(zone, out ZoneTransformState state) || !state.captured)
        {
            CaptureZoneState(zone);
            zoneStates.TryGetValue(zone, out state);
        }

        if (state == null) return;

        Transform zoneTransform = zone.transform;
        if (zoneTransform == null) return;

        zoneTransform.position = activeChallenge.position;
        zoneTransform.rotation = state.originalRotation;
        zoneTransform.localScale = state.originalScale;
    }

    private void RestoreZoneTransform(MissionZone zone)
    {
        if (zone == null) return;
        if (!zoneStates.TryGetValue(zone, out ZoneTransformState state) || !state.captured) return;

        Transform zoneTransform = zone.transform;
        if (zoneTransform == null) return;

        zoneTransform.position = state.originalPosition;
        zoneTransform.rotation = state.originalRotation;
        zoneTransform.localScale = state.originalScale;
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string normalized = value.Trim().ToLowerInvariant();
        char[] separators = { ' ', '_', '-', '.', '/' };

        for (int i = 0; i < separators.Length; i++)
        {
            normalized = normalized.Replace(separators[i].ToString(), string.Empty);
        }

        return normalized;
    }
}
