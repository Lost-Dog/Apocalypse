using UnityEngine;
using System.Collections.Generic;

public class DynamicChallengeZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Display name for this zone")]
    public string zoneName = "Challenge Zone";
    
    [Tooltip("Radius for challenge detection/spawning")]
    public float zoneRadius = 50f;
    
    [Tooltip("Which challenge types can use this zone (leave empty for all)")]
    public List<ChallengeData.ChallengeType> allowedChallengeTypes = new List<ChallengeData.ChallengeType>();
    
    [Header("Spawn Point Detection")]
    [Tooltip("Auto-detect spawn points from children named 'SpawnPoint' or 'SpawnPoints_*'")]
    public bool autoDetectSpawnPoints = true;
    
    [Tooltip("Manually detected spawn points (updated automatically)")]
    public List<Transform> detectedSpawnPoints = new List<Transform>();
    
    [Header("Zone Status")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private ActiveChallenge currentChallenge;
    
    [Header("Visual Settings")]
    public bool showGizmos = true;
    public Color availableColor = Color.green;
    public Color occupiedColor = Color.red;
    
    private void Awake()
    {
        if (autoDetectSpawnPoints)
        {
            DetectSpawnPoints();
        }
    }
    
    public void DetectSpawnPoints()
    {
        detectedSpawnPoints.Clear();
        
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child == transform)
                continue;
            
            if (child.name.Contains("SpawnPoint") || child.name.StartsWith("Spawn_"))
            {
                detectedSpawnPoints.Add(child);
            }
        }
        
        Debug.Log($"<color=cyan>DynamicChallengeZone '{zoneName}': Detected {detectedSpawnPoints.Count} spawn points</color>");
    }
    
    public bool IsAvailable()
    {
        return !isOccupied && gameObject.activeInHierarchy;
    }
    
    public bool CanHostChallenge(ChallengeData.ChallengeType challengeType)
    {
        if (!IsAvailable())
            return false;
        
        if (allowedChallengeTypes.Count == 0)
            return true;
        
        return allowedChallengeTypes.Contains(challengeType);
    }
    
    public void OccupyZone(ActiveChallenge challenge)
    {
        isOccupied = true;
        currentChallenge = challenge;
        gameObject.SetActive(true);
        
        Debug.Log($"<color=yellow>Zone '{zoneName}' occupied by challenge '{challenge.challengeData.challengeName}'</color>");
    }
    
    public void ReleaseZone()
    {
        isOccupied = false;
        currentChallenge = null;
        
        Debug.Log($"<color=cyan>Zone '{zoneName}' released and available again</color>");
    }
    
    public List<Transform> GetSpawnPoints(int maxCount = -1)
    {
        if (detectedSpawnPoints.Count == 0)
        {
            DetectSpawnPoints();
        }
        
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform point in detectedSpawnPoints)
        {
            if (point != null)
            {
                validPoints.Add(point);
            }
        }
        
        if (maxCount > 0 && validPoints.Count > maxCount)
        {
            List<Transform> shuffled = new List<Transform>(validPoints);
            for (int i = 0; i < shuffled.Count; i++)
            {
                Transform temp = shuffled[i];
                int randomIndex = Random.Range(i, shuffled.Count);
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }
            return shuffled.GetRange(0, maxCount);
        }
        
        return validPoints;
    }
    
    public Vector3 GetCenterPosition()
    {
        return transform.position;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        
        Gizmos.color = isOccupied ? occupiedColor : availableColor;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        
        if (detectedSpawnPoints != null && detectedSpawnPoints.Count > 0)
        {
            Color pointColor = isOccupied ? occupiedColor : availableColor;
            pointColor.a = 0.5f;
            
            foreach (Transform point in detectedSpawnPoints)
            {
                if (point == null)
                    continue;
                
                Gizmos.color = pointColor;
                Gizmos.DrawWireCube(point.position, Vector3.one * 1.5f);
                Gizmos.DrawLine(transform.position, point.position);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (detectedSpawnPoints == null)
            return;
        
        foreach (Transform point in detectedSpawnPoints)
        {
            if (point == null)
                continue;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(point.position, 0.5f);
            Gizmos.DrawLine(point.position, point.position + point.forward * 3f);
        }
    }
    
    [ContextMenu("Re-detect Spawn Points")]
    public void RefreshSpawnPoints()
    {
        DetectSpawnPoints();
    }
}
