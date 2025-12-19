using UnityEngine;
using System.Collections.Generic;

public class MissionZone : MonoBehaviour
{
    [Header("Mission Zone Info")]
    public string zoneName = "Mission Zone";
    public ChallengeData.ChallengeType missionType;
    public float zoneRadius = 30f;
    
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
        if (zoneMarkerPrefab != null && markerInstance == null)
        {
            markerInstance = Instantiate(zoneMarkerPrefab, transform.position, Quaternion.identity, transform);
        }
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
