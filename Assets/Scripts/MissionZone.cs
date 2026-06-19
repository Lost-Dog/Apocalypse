using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionZone : MonoBehaviour
{
    [Header("Mission Zone Info")]
    public string zoneName = "Mission Zone";
    public ChallengeData.ChallengeType missionType;
    public float zoneRadius = 30f;

    [Header("Mission Visibility")]
    public MissionData linkedMissionData;
    public bool autoMatchMissionByName = true;
    [Tooltip("Deactivate legacy hotspot objects/components under this zone at runtime")]
    public bool disableLegacyHotspots = true;
    [Tooltip("Logs why this zone becomes visible during play mode")]
    public bool debugVisibilityTransitions = false;
    
    [Header("Spawn Points")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    
    [Header("Visual Settings")]
    public bool showGizmos = true;
    public Color zoneColor = Color.cyan;
    public GameObject zoneMarkerPrefab;
    
    [Header("Auto-Generated Data")]
    public ChallengeData linkedChallengeData;
    public bool autoGenerateSpawnItems = true;
    
    private GameObject markerInstance;
    private MissionManager missionManager;
    private ChallengeManager challengeManager;
    private bool previousVisibilityState;
    private bool hasVisibilityState;
    
    [System.Serializable]
    public class SpawnPoint
    {
        public string pointName = "Spawn Point";
        public Transform transform;
        public ChallengeData.SpawnableCategory category = ChallengeData.SpawnableCategory.Enemy;
        public GameObject prefabOverride;
        public bool useCustomSettings = false;
        
        [Header("Custom Settings (if enabled)")]
        public bool requireNavMesh = true;
        public bool randomRotation = true;
        public Vector3 fixedRotation = Vector3.zero;
        public int priority = 0;
    }
    
    private void Start()
    {
        BindMissionManager();
        BindChallengeManager();
        DisableLegacyHotspotObjects();
        RefreshZoneVisibilityState();

        if (zoneMarkerPrefab != null && markerInstance == null)
        {
            markerInstance = Instantiate(zoneMarkerPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void OnEnable()
    {
        BindMissionManager();
        BindChallengeManager();
        DisableLegacyHotspotObjects();

        if (missionManager != null)
        {
            missionManager.onMissionStart.AddListener(HandleMissionChanged);
            missionManager.onMissionComplete.AddListener(HandleMissionChanged);
            missionManager.onMissionFail.AddListener(HandleMissionChanged);
        }

        if (challengeManager != null)
        {
            challengeManager.onChallengeStarted.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeCompleted.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeFailed.AddListener(HandleChallengeChanged);
            challengeManager.onChallengeExpired.AddListener(HandleChallengeChanged);
        }

        RefreshZoneVisibilityState();
    }

    private void OnDisable()
    {
        if (missionManager != null)
        {
            missionManager.onMissionStart.RemoveListener(HandleMissionChanged);
            missionManager.onMissionComplete.RemoveListener(HandleMissionChanged);
            missionManager.onMissionFail.RemoveListener(HandleMissionChanged);
        }

        if (challengeManager != null)
        {
            challengeManager.onChallengeStarted.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeCompleted.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeFailed.RemoveListener(HandleChallengeChanged);
            challengeManager.onChallengeExpired.RemoveListener(HandleChallengeChanged);
        }
    }

    private void HandleMissionChanged(MissionData mission)
    {
        RefreshZoneVisibilityState();
    }

    private void HandleChallengeChanged(ActiveChallenge challenge)
    {
        RefreshZoneVisibilityState();
    }

    private void BindMissionManager()
    {
        if (missionManager != null) return;

        missionManager = FindObjectOfType<MissionManager>();
    }

    private void BindChallengeManager()
    {
        if (challengeManager != null) return;

        challengeManager = ChallengeManager.Instance;
        if (challengeManager == null)
        {
            challengeManager = FindObjectOfType<ChallengeManager>();
        }
    }

    private void RefreshZoneVisibilityState()
    {
        bool visible = EvaluateZoneVisibility(out string visibilityReason);

        if (debugVisibilityTransitions)
        {
            if (!hasVisibilityState)
            {
                previousVisibilityState = visible;
                hasVisibilityState = true;
            }
            else if (!previousVisibilityState && visible)
            {
                string reasonText = string.IsNullOrEmpty(visibilityReason) ? "unknown reason" : visibilityReason;
                Debug.Log($"[MissionZone] '{zoneName}' became visible: {reasonText}", this);
                previousVisibilityState = visible;
            }
            else if (previousVisibilityState != visible)
            {
                previousVisibilityState = visible;
            }
        }

        if (markerInstance != null && markerInstance.activeSelf != visible)
        {
            markerInstance.SetActive(visible);
        }
    }

    private void DisableLegacyHotspotObjects()
    {
        if (!disableLegacyHotspots)
            return;

        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child == null || child == transform)
                continue;

            string childName = child.name;
            if (string.IsNullOrEmpty(childName))
                continue;

            if (childName.IndexOf("hotspot", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            GameObject hotspotObject = child.gameObject;
            if (hotspotObject.activeSelf)
            {
                hotspotObject.SetActive(false);
            }
        }
    }

    private bool IsActiveMissionZone()
    {
        return EvaluateZoneVisibility(out _);
    }

    private bool EvaluateZoneVisibility(out string reason)
    {
        if (IsLinkedMissionActive())
        {
            reason = linkedMissionData != null
                ? "mission match (linked MissionData)"
                : "mission match (name auto-match)";
            return true;
        }

        if (IsLinkedChallengeActive(out reason))
        {
            return true;
        }

        reason = string.Empty;
        return false;
    }

    private bool IsLinkedMissionActive()
    {
        if (missionManager == null) return false;

        MissionData activeMission = missionManager.activeMission;
        if (activeMission == null) return false;

        if (linkedMissionData != null)
        {
            return linkedMissionData == activeMission;
        }

        if (!autoMatchMissionByName) return false;

        string activeName = NormalizeName(activeMission.missionName);
        if (string.IsNullOrEmpty(activeName)) return false;

        string zoneKey = NormalizeName(zoneName);
        if (string.IsNullOrEmpty(zoneKey))
        {
            zoneKey = NormalizeName(gameObject.name);
        }

        if (activeName == zoneKey)
        {
            linkedMissionData = activeMission;
            return true;
        }

        if (linkedChallengeData != null)
        {
            string challengeKey = NormalizeName(linkedChallengeData.challengeName);
            if (!string.IsNullOrEmpty(challengeKey) && activeName == challengeKey)
            {
                linkedMissionData = activeMission;
                return true;
            }
        }

        return false;
    }

    private bool IsLinkedChallengeActive(out string reason)
    {
        reason = string.Empty;

        if (challengeManager == null)
        {
            BindChallengeManager();
        }

        if (challengeManager == null || challengeManager.activeChallenges == null)
        {
            return false;
        }

        for (int i = 0; i < challengeManager.activeChallenges.Count; i++)
        {
            ActiveChallenge activeChallenge = challengeManager.activeChallenges[i];
            if (activeChallenge == null || activeChallenge.challengeData == null) continue;
            if (activeChallenge.state != ActiveChallenge.ChallengeState.Active) continue;

            if (linkedChallengeData != null)
            {
                if (activeChallenge.challengeData == linkedChallengeData)
                {
                    reason = "challenge match (linked ChallengeData)";
                    return true;
                }

                string linkedKey = NormalizeName(linkedChallengeData.challengeName);
                string activeKey = NormalizeName(activeChallenge.challengeData.challengeName);
                if (!string.IsNullOrEmpty(linkedKey) && linkedKey == activeKey)
                {
                    reason = "challenge match (linked challenge name)";
                    return true;
                }

                continue;
            }
        }

        return false;
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string normalized = value.Trim().ToLowerInvariant();
        char[] separators = { ' ', '_', '-', '.', '/' };

        foreach (char separator in separators)
        {
            normalized = normalized.Replace(separator.ToString(), string.Empty);
        }

        return normalized;
    }
    
    public void GenerateSpawnItemsForChallenge()
    {
        if (linkedChallengeData == null || !autoGenerateSpawnItems)
            return;
        
        linkedChallengeData.spawnItems.Clear();
        
        Dictionary<string, List<SpawnPoint>> groupedPoints = new Dictionary<string, List<SpawnPoint>>();
        
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.transform == null)
                continue;
            
            string key = GetSpawnGroupKey(point);
            
            if (!groupedPoints.ContainsKey(key))
            {
                groupedPoints[key] = new List<SpawnPoint>();
            }
            
            groupedPoints[key].Add(point);
        }
        
        foreach (var group in groupedPoints)
        {
            SpawnPoint firstPoint = group.Value[0];
            
            ChallengeData.SpawnableItem item = new ChallengeData.SpawnableItem();
            item.itemName = group.Key;
            item.category = firstPoint.category;
            item.prefab = firstPoint.prefabOverride;
            item.minCount = group.Value.Count;
            item.maxCount = group.Value.Count;
            item.spawnLocation = ChallengeData.SpawnLocationType.AtCenter;
            item.requireNavMesh = firstPoint.useCustomSettings ? firstPoint.requireNavMesh : true;
            item.randomRotation = firstPoint.useCustomSettings ? firstPoint.randomRotation : true;
            item.priority = firstPoint.useCustomSettings ? firstPoint.priority : 0;
            
            // Populate customSpawnPoints array with transforms from this group
            Transform[] spawnTransforms = new Transform[group.Value.Count];
            for (int i = 0; i < group.Value.Count; i++)
            {
                spawnTransforms[i] = group.Value[i].transform;
            }
            item.customSpawnPoints = spawnTransforms;
            
            linkedChallengeData.spawnItems.Add(item);
        }
        
        Debug.Log($"Generated {linkedChallengeData.spawnItems.Count} spawn items for {linkedChallengeData.challengeName}");
    }
    
    private string GetSpawnGroupKey(SpawnPoint point)
    {
        if (point.prefabOverride != null)
        {
            return $"{point.category}_{point.prefabOverride.name}";
        }
        return $"{point.category}_Default";
    }
    
    public List<Transform> GetSpawnTransformsByCategory(ChallengeData.SpawnableCategory category)
    {
        List<Transform> transforms = new List<Transform>();
        
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.category == category && point.transform != null)
            {
                transforms.Add(point.transform);
            }
        }
        
        return transforms;
    }
    
    public List<Vector3> GetSpawnPositionsByCategory(ChallengeData.SpawnableCategory category)
    {
        List<Vector3> positions = new List<Vector3>();
        
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.category == category && point.transform != null)
            {
                positions.Add(point.transform.position);
            }
        }
        
        return positions;
    }
    
    public int GetSpawnPointCount(ChallengeData.SpawnableCategory category)
    {
        int count = 0;
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.category == category)
                count++;
        }
        return count;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        
        Gizmos.color = zoneColor;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.1f);
        
        if (spawnPoints != null)
        {
            foreach (SpawnPoint point in spawnPoints)
            {
                if (point.transform == null)
                    continue;
                
                Color pointColor = GetCategoryColor(point.category);
                Gizmos.color = pointColor;
                Gizmos.DrawWireCube(point.transform.position, Vector3.one * 2f);
                Gizmos.DrawLine(transform.position, point.transform.position);
                
                Gizmos.color = new Color(pointColor.r, pointColor.g, pointColor.b, 0.3f);
                Gizmos.DrawCube(point.transform.position, Vector3.one * 2f);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
            return;
        
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.transform == null)
                continue;
            
            Color pointColor = GetCategoryColor(point.category);
            Gizmos.color = pointColor;
            
            Gizmos.DrawSphere(point.transform.position, 0.5f);
            Gizmos.DrawLine(point.transform.position, point.transform.position + point.transform.forward * 3f);
        }
    }
    
    private Color GetCategoryColor(ChallengeData.SpawnableCategory category)
    {
        switch (category)
        {
            case ChallengeData.SpawnableCategory.Enemy: return Color.red;
            case ChallengeData.SpawnableCategory.Civilian: return Color.green;
            case ChallengeData.SpawnableCategory.Boss: return new Color(1f, 0f, 0.5f);
            case ChallengeData.SpawnableCategory.LootBox: return Color.yellow;
            case ChallengeData.SpawnableCategory.Objective: return Color.cyan;
            case ChallengeData.SpawnableCategory.Cover: return Color.gray;
            case ChallengeData.SpawnableCategory.Vehicle: return Color.blue;
            default: return Color.white;
        }
    }
}
