using UnityEngine;
using System.Collections.Generic;

public class DynamicZoneManager : MonoBehaviour
{
    public static DynamicZoneManager Instance { get; private set; }
    
    [Header("Zone Pool")]
    [SerializeField] private List<DynamicChallengeZone> allZones = new List<DynamicChallengeZone>();
    
    [Header("Auto-Detection")]
    [SerializeField] private bool autoFindZonesOnStart = true;
    [SerializeField] private string zoneParentName = "ChallengeZones";
    
    [Header("Default Prefabs")]
    [Tooltip("Default enemy prefab when spawn item has none")]
    public GameObject defaultEnemyPrefab;
    
    [Tooltip("Default civilian prefab when spawn item has none")]
    public GameObject defaultCivilianPrefab;
    
    private Dictionary<ActiveChallenge, DynamicChallengeZone> challengeToZoneMap = new Dictionary<ActiveChallenge, DynamicChallengeZone>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (autoFindZonesOnStart)
        {
            FindAllZones();
        }
    }
    
    [ContextMenu("Find All Zones")]
    public void FindAllZones()
    {
        allZones.Clear();
        
        GameObject zonesParent = GameObject.Find(zoneParentName);
        if (zonesParent != null)
        {
            DynamicChallengeZone[] zones = zonesParent.GetComponentsInChildren<DynamicChallengeZone>(true);
            allZones.AddRange(zones);
            // Intentionally quiet: zone discovery is not noisy enough to log every startup.
        }
        else
        {
            DynamicChallengeZone[] zones = FindObjectsByType<DynamicChallengeZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            allZones.AddRange(zones);
            Debug.Log($"<color=yellow>⚠ '{zoneParentName}' not found, searched entire scene: found {allZones.Count} zones</color>");
        }
        
        foreach (DynamicChallengeZone zone in allZones)
        {
            zone.RefreshSpawnPoints();
        }
    }
    
    public DynamicChallengeZone GetRandomAvailableZone(ChallengeData.ChallengeType challengeType)
    {
        DynamicChallengeZone selectedZone = null;
        int availableCount = 0;

        // Reservoir sampling picks a uniformly random available zone in a single pass.
        for (int i = 0; i < allZones.Count; i++)
        {
            DynamicChallengeZone zone = allZones[i];
            if (!zone.CanHostChallenge(challengeType)) continue;

            availableCount++;
            if (Random.Range(0, availableCount) == 0)
            {
                selectedZone = zone;
            }
        }

        if (selectedZone == null)
        {
            Debug.LogWarning($"No available zones for challenge type: {challengeType}");
            return null;
        }
        
        // Debug.Log($"<color=cyan>Selected zone '{selectedZone.zoneName}' for {challengeType} challenge (from {availableZones.Count} available)</color>");
        
        return selectedZone;
    }
    
    public DynamicChallengeZone GetClosestAvailableZone(Vector3 position, ChallengeData.ChallengeType challengeType)
    {
        DynamicChallengeZone closest = null;
        float closestDistanceSqr = float.MaxValue;
        
        for (int i = 0; i < allZones.Count; i++)
        {
            DynamicChallengeZone zone = allZones[i];
            if (!zone.CanHostChallenge(challengeType)) continue;

            float distanceSqr = (position - zone.GetCenterPosition()).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = zone;
            }
        }

        if (closest == null)
        {
            Debug.LogWarning($"No available zones for challenge type: {challengeType}");
            return null;
        }
        
        Debug.Log($"<color=cyan>Selected closest zone '{closest.zoneName}' for {challengeType} challenge ({Mathf.Sqrt(closestDistanceSqr):F1}m away)</color>");
        
        return closest;
    }
    
    public void AssignZoneToChallenge(ActiveChallenge challenge, DynamicChallengeZone zone)
    {
        if (challengeToZoneMap.ContainsKey(challenge))
        {
            Debug.LogWarning($"Challenge already has a zone assigned!");
            return;
        }
        
        zone.OccupyZone(challenge);
        challengeToZoneMap[challenge] = zone;
    }
    
    public DynamicChallengeZone GetZoneForChallenge(ActiveChallenge challenge)
    {
        if (challengeToZoneMap.TryGetValue(challenge, out DynamicChallengeZone zone))
        {
            return zone;
        }
        return null;
    }
    
    public void ReleaseChallengeZone(ActiveChallenge challenge)
    {
        if (challengeToZoneMap.TryGetValue(challenge, out DynamicChallengeZone zone))
        {
            zone.ReleaseZone();
            challengeToZoneMap.Remove(challenge);
        }
    }
    
    public List<Transform> GetSpawnPointsForChallenge(ActiveChallenge challenge, int requestedCount = -1)
    {
        DynamicChallengeZone zone = GetZoneForChallenge(challenge);
        
        if (zone == null)
        {
            Debug.LogError($"No zone assigned to challenge!");
            return new List<Transform>();
        }
        
        return zone.GetSpawnPoints(requestedCount);
    }
    
    public int GetAvailableZoneCount(ChallengeData.ChallengeType? challengeType = null)
    {
        int count = 0;

        if (challengeType.HasValue)
        {
            for (int i = 0; i < allZones.Count; i++)
            {
                if (allZones[i].CanHostChallenge(challengeType.Value))
                {
                    count++;
                }
            }

            return count;
        }

        for (int i = 0; i < allZones.Count; i++)
        {
            if (allZones[i].IsAvailable())
            {
                count++;
            }
        }

        return count;
    }
    
    public Vector3 GetChallengePosition(ActiveChallenge challenge)
    {
        DynamicChallengeZone zone = GetZoneForChallenge(challenge);
        
        if (zone != null)
        {
            return zone.GetCenterPosition();
        }
        
        return challenge.position;
    }
}
