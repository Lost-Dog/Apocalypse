using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            Debug.Log($"<color=green>✓ Found {allZones.Count} DynamicChallengeZones under '{zoneParentName}'</color>");
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
        List<DynamicChallengeZone> availableZones = allZones
            .Where(z => z.CanHostChallenge(challengeType))
            .ToList();
        
        if (availableZones.Count == 0)
        {
            Debug.LogWarning($"No available zones for challenge type: {challengeType}");
            return null;
        }
        
        int randomIndex = Random.Range(0, availableZones.Count);
        DynamicChallengeZone selectedZone = availableZones[randomIndex];
        
        Debug.Log($"<color=cyan>Selected zone '{selectedZone.zoneName}' for {challengeType} challenge (from {availableZones.Count} available)</color>");
        
        return selectedZone;
    }
    
    public DynamicChallengeZone GetClosestAvailableZone(Vector3 position, ChallengeData.ChallengeType challengeType)
    {
        List<DynamicChallengeZone> availableZones = allZones
            .Where(z => z.CanHostChallenge(challengeType))
            .ToList();
        
        if (availableZones.Count == 0)
        {
            Debug.LogWarning($"No available zones for challenge type: {challengeType}");
            return null;
        }
        
        DynamicChallengeZone closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (DynamicChallengeZone zone in availableZones)
        {
            float distance = Vector3.Distance(position, zone.GetCenterPosition());
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = zone;
            }
        }
        
        Debug.Log($"<color=cyan>Selected closest zone '{closest.zoneName}' for {challengeType} challenge ({closestDistance:F1}m away)</color>");
        
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
        if (challengeType.HasValue)
        {
            return allZones.Count(z => z.CanHostChallenge(challengeType.Value));
        }
        return allZones.Count(z => z.IsAvailable());
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
