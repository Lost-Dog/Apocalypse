using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Lovatto.MiniMap;

public class ChallengeSpawner : MonoBehaviour
{
    public static ChallengeSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private int maxNavMeshAttempts = 20;
    [SerializeField] private float navMeshSampleDistance = 5f;
    [SerializeField] private float minimumSpawnDistance = 3f;
    [SerializeField] private LayerMask obstructionMask;
    
    [Header("Spawn Validation")]
    [Tooltip("Force use NavMesh for all spawns (overrides item settings)")]
    public bool forceNavMesh = false;
    [Tooltip("Skip NavMesh checks even if item requires it")]
    public bool ignoreNavMeshRequirement = true;
    [Tooltip("Skip obstruction checks (spawn even if position is blocked)")]
    public bool ignoreObstructionChecks = true;

    [Header("Enemy Setup")]
    [Tooltip("Icon data for enemies on minimap")]
    public bl_MiniMapIconData enemyMinimapIcon;
    [Tooltip("Healthbar prefab to instantiate for enemies")]
    public GameObject healthBarPrefab;
    [Tooltip("Maximum number of enemies that can be spawned per challenge")]
    public int maxEnemiesPerChallenge = 10;

    private Dictionary<ActiveChallenge, ChallengeInstance> activeChallengeInstances = new Dictionary<ActiveChallenge, ChallengeInstance>();

    private class ChallengeInstance
    {
        public GameObject worldMarker;
        public GameObject compassMarker;
        public GameObject minimapPointer;
        public List<GameObject> spawnedEnemies = new List<GameObject>();
        public List<GameObject> spawnedCivilians = new List<GameObject>();
        public List<GameObject> spawnedObjects = new List<GameObject>();
        public GameObject bossEnemy;
        public MissionZone activatedZone;
        public ControlZone activatedControlZone;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnChallengeContent(ActiveChallenge challenge, ChallengeData data, GameObject worldMarkerPrefab, GameObject compassMarkerPrefab, Transform compassContainer, Transform worldspaceUIContainer = null, bool spawnMinimapPointer = true)
    {
        if (activeChallengeInstances.ContainsKey(challenge))
        {
            Debug.LogWarning($"Challenge {data.challengeName} already has spawned content!");
            return;
        }

        ChallengeInstance instance = new ChallengeInstance();
        
        if (DynamicZoneManager.Instance != null)
        {
            DynamicChallengeZone dynamicZone = DynamicZoneManager.Instance.GetRandomAvailableZone(data.challengeType);
            if (dynamicZone != null)
            {
                DynamicZoneManager.Instance.AssignZoneToChallenge(challenge, dynamicZone);
                challenge.position = dynamicZone.GetCenterPosition();
                
                SpawnMarkers(challenge, data, worldMarkerPrefab, compassMarkerPrefab, compassContainer, worldspaceUIContainer, spawnMinimapPointer, instance);
                SpawnFromDynamicZone(challenge, data, dynamicZone, instance);
                activeChallengeInstances[challenge] = instance;
                return;
            }
        }

        SpawnMarkers(challenge, data, worldMarkerPrefab, compassMarkerPrefab, compassContainer, worldspaceUIContainer, spawnMinimapPointer, instance);
        
        ControlZone controlZone = FindControlZoneForChallenge(challenge.position, data.challengeType);
        if (controlZone != null)
        {
            SpawnFromControlZone(challenge, data, controlZone, instance);
        }
        else
        {
            MissionZone linkedZone = FindMissionZoneForChallenge(challenge.position, data.challengeType);
            if (linkedZone != null)
            {
                SpawnFromMissionZone(challenge, data, linkedZone, instance);
            }
            else
            {
                SpawnFlexibleItems(challenge, data, instance);
            }
        }

        activeChallengeInstances[challenge] = instance;
    }
    
    private ControlZone FindControlZoneForChallenge(Vector3 position, ChallengeData.ChallengeType type)
    {
        if (type != ChallengeData.ChallengeType.ControlPoint)
            return null;
        
        ControlZone[] allZones = FindObjectsByType<ControlZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        ControlZone nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (ControlZone zone in allZones)
        {
            if (zone.gameObject.activeInHierarchy)
                continue;
            
            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance < zone.captureRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = zone;
            }
        }
        
        return nearest;
    }
    
    private void SpawnFromControlZone(ActiveChallenge challenge, ChallengeData data, ControlZone zone, ChallengeInstance instance)
    {
        Debug.Log($"<color=yellow>[ChallengeSpawner] Spawning challenge '{data.challengeName}' using Control Zone '{zone.zoneName}'</color>");
        
        instance.activatedControlZone = zone;
        zone.LinkToChallenge(challenge);
        zone.ActivateAndSpawn();
    }
    
    private MissionZone FindMissionZoneForChallenge(Vector3 position, ChallengeData.ChallengeType type)
    {
        MissionZone[] allZones = FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        MissionZone nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (MissionZone zone in allZones)
        {
            if (zone.gameObject.activeInHierarchy)
                continue;
            
            if (zone.missionType == type)
            {
                float distance = Vector3.Distance(position, zone.transform.position);
                if (distance < zone.zoneRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = zone;
                }
            }
        }
        
        return nearest;
    }
    
    private void SpawnFromMissionZone(ActiveChallenge challenge, ChallengeData data, MissionZone zone, ChallengeInstance instance)
    {
        Debug.Log($"Spawning challenge '{data.challengeName}' using Mission Zone '{zone.zoneName}'");
        
        zone.gameObject.SetActive(true);
        instance.activatedZone = zone;
        
        foreach (MissionZone.SpawnPoint spawnPoint in zone.spawnPoints)
        {
            if (spawnPoint.transform == null || spawnPoint.prefabOverride == null)
                continue;
            
            GameObject spawnedObject = Instantiate(
                spawnPoint.prefabOverride, 
                spawnPoint.transform.position, 
                spawnPoint.transform.rotation
            );
            
            spawnedObject.name = $"{spawnPoint.pointName}";
            spawnedObject.SetActive(true);
            
            CategorizeAndStoreSpawnedObject(spawnedObject, spawnPoint.category, instance, challenge);
        }
        
        if (zone.linkedChallengeData != null)
        {
            LinkZoneToChallenge(zone, challenge);
        }
    }
    
    private void LinkZoneToChallenge(MissionZone zone, ActiveChallenge challenge)
    {
        ControlZone controlZone = zone.GetComponent<ControlZone>();
        if (controlZone != null)
        {
            controlZone.LinkToChallenge(challenge);
        }
    }
    
    private void SpawnFromDynamicZone(ActiveChallenge challenge, ChallengeData data, DynamicChallengeZone zone, ChallengeInstance instance)
    {
        Debug.Log($"<color=green>✓ Spawning challenge '{data.challengeName}' using Dynamic Zone '{zone.zoneName}'</color>");
        
        if (data.spawnItems == null || data.spawnItems.Count == 0)
        {
            Debug.LogWarning($"Challenge '{data.challengeName}' has no spawn items configured!");
            return;
        }
        
        List<Transform> availableSpawnPoints = zone.GetSpawnPoints();
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError($"Dynamic Zone '{zone.zoneName}' has no spawn points!");
            return;
        }
        
        Debug.Log($"Dynamic Zone has {availableSpawnPoints.Count} spawn points available");
        
        int spawnPointIndex = 0;
        
        foreach (ChallengeData.SpawnableItem item in data.spawnItems)
        {
            if (item.prefab == null)
            {
                GameObject defaultPrefab = GetDefaultPrefabForCategory(item.category);
                if (defaultPrefab == null)
                {
                    Debug.LogWarning($"No prefab set for spawn item '{item.itemName}' and no default found for category {item.category}");
                    continue;
                }
                item.prefab = defaultPrefab;
            }
            
            int spawnCount = Random.Range(item.minCount, item.maxCount + 1);
            
            // Limit enemy spawns based on maxEnemiesPerChallenge
            if (item.category == ChallengeData.SpawnableCategory.Enemy)
            {
                int remainingSlots = maxEnemiesPerChallenge - instance.spawnedEnemies.Count;
                if (remainingSlots <= 0)
                {
                    Debug.LogWarning($"⚠️ Skipping {item.itemName} - already at max enemy limit ({maxEnemiesPerChallenge})");
                    continue;
                }
                spawnCount = Mathf.Min(spawnCount, remainingSlots);
            }
            
            for (int i = 0; i < spawnCount; i++)
            {
                if (spawnPointIndex >= availableSpawnPoints.Count)
                {
                    Debug.LogWarning($"Ran out of spawn points! Needed {spawnCount} for '{item.itemName}', only had {availableSpawnPoints.Count} total");
                    break;
                }
                
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                spawnPointIndex++;
                
                Quaternion spawnRotation = item.randomRotation 
                    ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) 
                    : spawnPoint.rotation;
                
                GameObject spawnedObject = Instantiate(
                    item.prefab,
                    spawnPoint.position,
                    spawnRotation
                );
                
                spawnedObject.name = $"{item.itemName}_{i}";
                spawnedObject.SetActive(true);
                
                CategorizeAndStoreSpawnedObject(spawnedObject, item.category, instance, challenge);
                
                Debug.Log($"  ✓ Spawned {item.itemName} at {spawnPoint.name}");
            }
        }
        
        Debug.Log($"<color=green>Spawned {instance.spawnedEnemies.Count} enemies, {instance.spawnedCivilians.Count} civilians, {instance.spawnedObjects.Count} objects</color>");
    }
    
    private GameObject GetDefaultPrefabForCategory(ChallengeData.SpawnableCategory category)
    {
        if (DynamicZoneManager.Instance == null)
            return null;
        
        switch (category)
        {
            case ChallengeData.SpawnableCategory.Enemy:
                return DynamicZoneManager.Instance.defaultEnemyPrefab;
            case ChallengeData.SpawnableCategory.Civilian:
                return DynamicZoneManager.Instance.defaultCivilianPrefab;
            default:
                return null;
        }
    }
    
    private void SpawnFlexibleItems(ActiveChallenge challenge, ChallengeData data, ChallengeInstance instance)
    {
        if (data.spawnItems == null || data.spawnItems.Count == 0)
        {
            Debug.LogError($"❌ Challenge '{data.challengeName}' has NO SPAWN ITEMS configured!");
            Debug.LogError($"Add spawn items in the Challenge Data asset to spawn enemies/objects.");
            Debug.LogError($"Path: Select challenge data → Inspector → Flexible Spawning System → Spawn Items");
            return;
        }
        
        bool useSharedSpawnPoints = HasValidSharedSpawnPoints(data);
        
        if (useSharedSpawnPoints)
        {
            Debug.Log($"📦 Using shared spawn point pool for challenge: {data.challengeName}");
            SpawnUsingSharedPool(challenge, data, instance);
            return;
        }
        
        Debug.Log($"📦 Spawning {data.spawnItems.Count} item types for challenge: {data.challengeName}");
        
        List<Vector3> usedPositions = new List<Vector3>();
        List<ChallengeData.SpawnableItem> sortedItems = new List<ChallengeData.SpawnableItem>(data.spawnItems);
        sortedItems.Sort((a, b) => b.priority.CompareTo(a.priority));
        
        int totalSpawned = 0;
        int totalFailed = 0;
        
        foreach (ChallengeData.SpawnableItem item in sortedItems)
        {
            if (item.prefab == null)
            {
                string itemDesc = string.IsNullOrEmpty(item.itemName) ? $"[Unnamed {item.category}]" : item.itemName;
                Debug.LogError($"❌ Spawn item '{itemDesc}' has NO PREFAB assigned! Check '{data.challengeName}' spawn items in Inspector.");
                totalFailed++;
                continue;
            }
            
            int countToSpawn = item.usePoolMode ? 
                Mathf.Min(item.maxCount, item.minCount) : 
                Random.Range(item.minCount, item.maxCount + 1);
            
            // Limit enemy spawns based on maxEnemiesPerChallenge
            if (item.category == ChallengeData.SpawnableCategory.Enemy)
            {
                int remainingSlots = maxEnemiesPerChallenge - instance.spawnedEnemies.Count;
                if (remainingSlots <= 0)
                {
                    Debug.LogWarning($"⚠️ Skipping {item.itemName} - already at max enemy limit ({maxEnemiesPerChallenge})");
                    continue;
                }
                countToSpawn = Mathf.Min(countToSpawn, remainingSlots);
            }
            
            int spawnedCount = 0;
            int failedCount = 0;
            
            bool hasValidCustomSpawnPoints = item.customSpawnPoints != null && 
                                             item.customSpawnPoints.Length > 0 &&
                                             System.Array.Exists(item.customSpawnPoints, sp => sp != null);
            
            if (hasValidCustomSpawnPoints)
            {
                Debug.Log($"✓ Using {item.customSpawnPoints.Length} custom spawn points for {item.itemName}");
                
                for (int i = 0; i < Mathf.Min(countToSpawn, item.customSpawnPoints.Length); i++)
                {
                    // Check enemy limit before spawning
                    if (item.category == ChallengeData.SpawnableCategory.Enemy && instance.spawnedEnemies.Count >= maxEnemiesPerChallenge)
                    {
                        Debug.LogWarning($"⚠️ Reached max enemy limit ({maxEnemiesPerChallenge}) while spawning {item.itemName}");
                        break;
                    }
                    
                    Transform spawnPoint = item.customSpawnPoints[i];
                    if (spawnPoint == null)
                    {
                        Debug.LogWarning($"Custom spawn point {i} is null for {item.itemName}, skipping");
                        failedCount++;
                        continue;
                    }
                    
                    GameObject spawnedObject = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
                    spawnedObject.name = string.IsNullOrEmpty(item.itemName) ? 
                        $"{item.prefab.name}_{i}" : 
                        $"{item.itemName}_{i}";
                    spawnedObject.SetActive(true);
                    
                    CategorizeAndStoreSpawnedObject(spawnedObject, item.category, instance, challenge);
                    usedPositions.Add(spawnPoint.position);
                    spawnedCount++;
                    totalSpawned++;
                }
                
                if (countToSpawn > item.customSpawnPoints.Length)
                {
                    Debug.LogWarning($"Wanted to spawn {countToSpawn} but only {item.customSpawnPoints.Length} spawn points provided for {item.itemName}");
                }
            }
            else
            {
                Debug.Log($"✓ Using random/procedural spawning for {item.itemName} (spawnLocation: {item.spawnLocation}, radius: {item.spawnRadius}m)");
                
                for (int i = 0; i < countToSpawn; i++)
                {
                    // Check enemy limit before spawning
                    if (item.category == ChallengeData.SpawnableCategory.Enemy && instance.spawnedEnemies.Count >= maxEnemiesPerChallenge)
                    {
                        Debug.LogWarning($"⚠️ Reached max enemy limit ({maxEnemiesPerChallenge}) while spawning {item.itemName}");
                        break;
                    }
                    
                    Vector3 spawnPosition = Vector3.zero;
                    Quaternion spawnRotation = item.randomRotation ? 
                        Quaternion.Euler(0, Random.Range(0f, 360f), 0) : 
                        Quaternion.Euler(item.fixedRotation.x, item.fixedRotation.y, item.fixedRotation.z);
                    
                    switch (item.spawnLocation)
                    {
                        case ChallengeData.SpawnLocationType.AtCenter:
                            spawnPosition = challenge.position + item.offset;
                            break;
                        case ChallengeData.SpawnLocationType.RandomInRadius:
                            Vector2 randomCircle = Random.insideUnitCircle * item.spawnRadius;
                            spawnPosition = challenge.position + new Vector3(randomCircle.x, 0, randomCircle.y) + item.offset;
                            break;
                        case ChallengeData.SpawnLocationType.RandomOnEdge:
                            Vector2 randomEdge = Random.insideUnitCircle.normalized * item.spawnRadius;
                            spawnPosition = challenge.position + new Vector3(randomEdge.x, 0, randomEdge.y) + item.offset;
                            break;
                        case ChallengeData.SpawnLocationType.AroundPerimeter:
                            float angle = (360f / countToSpawn) * i;
                            float radians = angle * Mathf.Deg2Rad;
                            Vector3 perimeterOffset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * item.spawnRadius;
                            spawnPosition = challenge.position + perimeterOffset + item.offset;
                            break;
                        case ChallengeData.SpawnLocationType.Grid:
                            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(countToSpawn));
                            int row = i / gridSize;
                            int col = i % gridSize;
                            float spacing = item.spawnRadius * 2f / gridSize;
                            Vector3 gridOffset = new Vector3((col - gridSize / 2f) * spacing, 0, (row - gridSize / 2f) * spacing);
                            spawnPosition = challenge.position + gridOffset + item.offset;
                            break;
                    }
                    
                    // NavMesh validation (optional)
                    bool shouldCheckNavMesh = forceNavMesh || (item.requireNavMesh && !ignoreNavMeshRequirement);
                    if (shouldCheckNavMesh)
                    {
                        UnityEngine.AI.NavMeshHit hit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            spawnPosition = hit.position;
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to find NavMesh near position {spawnPosition} for {item.itemName}");
                            failedCount++;
                            continue;
                        }
                    }
                    
                    // Obstruction check (optional)
                    if (!ignoreObstructionChecks && Physics.CheckSphere(spawnPosition, 1f))
                    {
                        Debug.LogWarning($"Spawn position {spawnPosition} is obstructed for {item.itemName}");
                        failedCount++;
                        continue;
                    }
                    
                    GameObject spawnedObject = Instantiate(item.prefab, spawnPosition, spawnRotation);
                    spawnedObject.name = string.IsNullOrEmpty(item.itemName) ? 
                        $"{item.prefab.name}_{i}" : 
                        $"{item.itemName}_{i}";
                    spawnedObject.SetActive(true);
                    
                    CategorizeAndStoreSpawnedObject(spawnedObject, item.category, instance, challenge);
                    usedPositions.Add(spawnPosition);
                    spawnedCount++;
                    totalSpawned++;
                }
            }
            
            if (spawnedCount > 0)
            {
                Debug.Log($"✓ Spawned {spawnedCount}x {item.itemName} ({item.category})");
            }
            
            if (failedCount > 0)
            {
                Debug.LogWarning($"⚠️ Failed {failedCount}x {item.itemName} spawns (NavMesh/obstruction issues)");
            }
        }
        
        Debug.Log($"📊 Challenge Spawn Summary: {totalSpawned} spawned, {totalFailed} failed");
        
        if (totalSpawned == 0 && totalFailed > 0)
        {
            Debug.LogError($"❌ CRITICAL: NO objects spawned for challenge '{data.challengeName}'!");
            Debug.LogError($"Common fixes:");
            Debug.LogError($"  1. Add spawn points to 'Shared Spawn Points' in Challenge Data");
            Debug.LogError($"  2. Or assign 'Custom Spawn Points' to each spawn item");
        }
    }
    
    private bool HasValidSharedSpawnPoints(ChallengeData data)
    {
        if (data.sharedSpawnPoints == null || data.sharedSpawnPoints.Length == 0)
            return false;
        
        foreach (Transform point in data.sharedSpawnPoints)
        {
            if (point != null)
                return true;
        }
        
        return false;
    }
    
    private void SpawnUsingSharedPool(ActiveChallenge challenge, ChallengeData data, ChallengeInstance instance)
    {
        List<Transform> availableSpawnPoints = new List<Transform>(data.sharedSpawnPoints);
        availableSpawnPoints.RemoveAll(point => point == null);
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError($"❌ No valid spawn points in shared pool for '{data.challengeName}'!");
            return;
        }
        
        List<ChallengeData.SpawnableItem> sortedItems = new List<ChallengeData.SpawnableItem>(data.spawnItems);
        sortedItems.Sort((a, b) => b.priority.CompareTo(a.priority));
        
        int totalSpawned = 0;
        int totalFailed = 0;
        
        foreach (ChallengeData.SpawnableItem item in sortedItems)
        {
            if (item.prefab == null)
            {
                string itemDesc = string.IsNullOrEmpty(item.itemName) ? $"[Unnamed {item.category}]" : item.itemName;
                Debug.LogError($"❌ Spawn item '{itemDesc}' has NO PREFAB assigned!");
                totalFailed++;
                continue;
            }
            
            int countToSpawn = item.usePoolMode ? 
                Mathf.Min(item.maxCount, item.minCount) : 
                Random.Range(item.minCount, item.maxCount + 1);
            
            // Limit enemy spawns based on maxEnemiesPerChallenge
            if (item.category == ChallengeData.SpawnableCategory.Enemy)
            {
                int remainingSlots = maxEnemiesPerChallenge - instance.spawnedEnemies.Count;
                if (remainingSlots <= 0)
                {
                    Debug.LogWarning($"⚠️ Skipping {item.itemName} - already at max enemy limit ({maxEnemiesPerChallenge})");
                    continue;
                }
                countToSpawn = Mathf.Min(countToSpawn, remainingSlots);
            }
            
            int spawnedCount = 0;
            
            for (int i = 0; i < countToSpawn; i++)
            {
                if (availableSpawnPoints.Count == 0)
                {
                    Debug.LogWarning($"⚠️ Ran out of spawn points! Spawned {spawnedCount}/{countToSpawn} {item.itemName}");
                    break;
                }
                
                // Double-check enemy limit before spawning
                if (item.category == ChallengeData.SpawnableCategory.Enemy && instance.spawnedEnemies.Count >= maxEnemiesPerChallenge)
                {
                    Debug.LogWarning($"⚠️ Reached max enemy limit ({maxEnemiesPerChallenge}) while spawning {item.itemName}");
                    break;
                }
                
                int randomIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[randomIndex];
                availableSpawnPoints.RemoveAt(randomIndex);
                
                GameObject spawnedObject = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
                spawnedObject.name = string.IsNullOrEmpty(item.itemName) ? 
                    $"{item.prefab.name}_{i}" : 
                    $"{item.itemName}_{i}";
                spawnedObject.SetActive(true);
                
                CategorizeAndStoreSpawnedObject(spawnedObject, item.category, instance, challenge);
                spawnedCount++;
                totalSpawned++;
            }
            
            if (spawnedCount > 0)
            {
                Debug.Log($"✓ Spawned {spawnedCount}x {item.itemName} from shared pool ({availableSpawnPoints.Count} points remaining)");
            }
        }
        
        Debug.Log($"<color=green>📊 Shared Pool Summary: {totalSpawned} spawned, {availableSpawnPoints.Count} spawn points unused</color>");
        
        if (totalSpawned == 0)
        {
            Debug.LogError($"❌ No objects spawned from shared pool!");
        }
    }
    
    private bool GetSpawnTransform(Vector3 center, ChallengeData.SpawnableItem item, List<Vector3> usedPositions, out Vector3 position, out Quaternion rotation)
    {
        position = center;
        rotation = Quaternion.identity;
        
        switch (item.spawnLocation)
        {
            case ChallengeData.SpawnLocationType.AtCenter:
                position = center + item.offset;
                break;
                
            case ChallengeData.SpawnLocationType.RandomInRadius:
                if (item.spawnRadius <= 0.1f)
                {
                    Debug.LogError($"❌ RandomInRadius requires spawnRadius > 0! Current: {item.spawnRadius}m. Using AtCenter instead.");
                    position = center + item.offset;
                }
                else if (!FindValidSpawnPosition(center, item.spawnRadius, usedPositions, out position, item.requireNavMesh))
                    return false;
                break;
                
            case ChallengeData.SpawnLocationType.RandomOnEdge:
                if (item.spawnRadius <= 0.1f)
                {
                    Debug.LogError($"❌ RandomOnEdge requires spawnRadius > 0! Current: {item.spawnRadius}m. Spawning at center.");
                    position = center + item.offset;
                }
                else
                {
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    Vector3 edgePoint = center + new Vector3(randomDirection.x, 0, randomDirection.y) * item.spawnRadius;
                    if (!FindValidSpawnPosition(edgePoint, 2f, usedPositions, out position, item.requireNavMesh))
                        return false;
                }
                break;
                
            case ChallengeData.SpawnLocationType.AroundPerimeter:
                if (item.spawnRadius <= 0.1f)
                {
                    Debug.LogError($"❌ AroundPerimeter requires spawnRadius > 0! Current: {item.spawnRadius}m. Spawning at center.");
                    position = center + item.offset;
                }
                else
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 perimeterPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * item.spawnRadius;
                    if (!FindValidSpawnPosition(perimeterPoint, 2f, usedPositions, out position, item.requireNavMesh))
                        return false;
                }
                break;
                
            case ChallengeData.SpawnLocationType.Grid:
                if (item.spawnRadius <= 0.1f)
                {
                    Debug.LogError($"❌ Grid requires spawnRadius > 0! Current: {item.spawnRadius}m. Using AtCenter instead.");
                    position = center + item.offset;
                }
                else if (!FindValidSpawnPosition(center, item.spawnRadius, usedPositions, out position, item.requireNavMesh))
                    return false;
                break;
        }
        
        if (item.randomRotation)
        {
            rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
        else
        {
            rotation = Quaternion.Euler(item.fixedRotation);
        }
        
        return true;
    }
    
    private void SetupChallengeEnemyUI(GameObject enemyObject)
    {
        // Setup minimap entity
        bl_MiniMapEntity minimapEntity = enemyObject.GetComponent<bl_MiniMapEntity>();
        if (minimapEntity == null)
        {
            minimapEntity = enemyObject.AddComponent<bl_MiniMapEntity>();
            
            if (enemyMinimapIcon != null)
            {
                minimapEntity.iconData = enemyMinimapIcon;
            }
            
            minimapEntity.Target = enemyObject.transform;
            minimapEntity.DestroyWithObject = true;
            minimapEntity.OffScreen = true;
            
            Debug.Log($"  📍 Added minimap entity to {enemyObject.name}");
        }
        
        // Setup healthbar
        WorldSpaceHealthBar healthBar = enemyObject.GetComponentInChildren<WorldSpaceHealthBar>(true);
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.ShowTemporarily(999f);
            Debug.Log($"✓ Enabled existing health bar UI for challenge enemy: {enemyObject.name}");
        }
        else if (healthBarPrefab != null)
        {
            // Instantiate healthbar prefab
            GameObject healthBarObj = Instantiate(healthBarPrefab, enemyObject.transform);
            healthBarObj.name = "HealthBar";
            
            healthBar = healthBarObj.GetComponent<WorldSpaceHealthBar>();
            if (healthBar != null)
            {
                Debug.Log($"  ❤️ Added healthbar to {enemyObject.name}");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ Healthbar prefab doesn't have WorldSpaceHealthBar component!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No WorldSpaceHealthBar found and no healthBarPrefab assigned for: {enemyObject.name}");
        }
    }
    
    private void CategorizeAndStoreSpawnedObject(GameObject obj, ChallengeData.SpawnableCategory category, ChallengeInstance instance, ActiveChallenge challenge)
    {
        switch (category)
        {
            case ChallengeData.SpawnableCategory.Enemy:
                // Check if we've reached the maximum enemy limit
                if (instance.spawnedEnemies.Count >= maxEnemiesPerChallenge)
                {
                    Debug.LogWarning($"⚠️ Maximum enemy limit reached ({maxEnemiesPerChallenge}). Destroying excess enemy: {obj.name}");
                    Destroy(obj);
                    return;
                }
                
                instance.spawnedEnemies.Add(obj);
                ChallengeEnemy challengeEnemy = obj.GetComponent<ChallengeEnemy>();
                if (challengeEnemy == null)
                {
                    challengeEnemy = obj.AddComponent<ChallengeEnemy>();
                }
                challengeEnemy.Initialize(challenge);
                
                SetupChallengeEnemyUI(obj);
                
                ApplyDifficultyScalingToEnemy(obj, challenge);
                break;
                
            case ChallengeData.SpawnableCategory.Boss:
                instance.bossEnemy = obj;
                ChallengeEnemy bossEnemy = obj.GetComponent<ChallengeEnemy>();
                if (bossEnemy == null)
                {
                    bossEnemy = obj.AddComponent<ChallengeEnemy>();
                }
                bossEnemy.Initialize(challenge, true);
                
                SetupChallengeEnemyUI(obj);
                break;
                
            case ChallengeData.SpawnableCategory.Civilian:
                instance.spawnedCivilians.Add(obj);
                ChallengeCivilian challengeCivilian = obj.GetComponent<ChallengeCivilian>();
                if (challengeCivilian == null)
                {
                    challengeCivilian = obj.AddComponent<ChallengeCivilian>();
                }
                challengeCivilian.Initialize(challenge);
                break;
                
            default:
                instance.spawnedObjects.Add(obj);
                break;
        }
    }

    private void SpawnMarkers(ActiveChallenge challenge, ChallengeData data, GameObject worldMarkerPrefab, GameObject compassMarkerPrefab, Transform compassContainer, Transform worldspaceUIContainer, bool spawnMinimapPointer, ChallengeInstance instance)
    {
        Debug.Log($"[SpawnMarkers] Starting marker spawn for {data.challengeName}");
        
        if (worldMarkerPrefab != null && worldspaceUIContainer != null)
        {
            instance.worldMarker = Instantiate(worldMarkerPrefab, worldspaceUIContainer);
            ChallengeWorldMarker marker = instance.worldMarker.GetComponent<ChallengeWorldMarker>();
            if (marker != null)
            {
                marker.SetChallenge(challenge);
            }
            Debug.Log($"✓ World marker spawned for {data.challengeName}");
        }

        if (compassMarkerPrefab != null && compassContainer != null)
        {
            instance.compassMarker = Instantiate(compassMarkerPrefab, compassContainer);
            ChallengeCompassMarker compassMarker = instance.compassMarker.GetComponent<ChallengeCompassMarker>();
            if (compassMarker != null)
            {
                compassMarker.SetChallenge(challenge);
                Debug.Log($"✓ Compass marker spawned for {data.challengeName} - Challenge at {challenge.position}");
            }
            else
            {
                Debug.LogError($"❌ Compass marker prefab has no ChallengeCompassMarker component!");
            }
        }
        else
        {
            if (compassMarkerPrefab == null)
                Debug.LogWarning($"⚠️ No compass marker prefab assigned!");
            if (compassContainer == null)
                Debug.LogWarning($"⚠️ No compass container assigned!");
        }
        
        if (spawnMinimapPointer && data.iconData != null)
        {
            GameObject pointerObject = new GameObject($"MinimapPointer_{data.challengeName}");
            pointerObject.transform.position = challenge.position;
            
            ChallengeMinimapPointer pointer = pointerObject.AddComponent<ChallengeMinimapPointer>();
            pointer.SetChallenge(challenge);
            
            instance.minimapPointer = pointerObject;
        }
    }

    public void CleanupChallenge(ActiveChallenge challenge)
    {
        if (!activeChallengeInstances.ContainsKey(challenge))
            return;

        ChallengeInstance instance = activeChallengeInstances[challenge];

        if (instance.worldMarker != null)
            Destroy(instance.worldMarker);

        if (instance.compassMarker != null)
            Destroy(instance.compassMarker);
        
        if (instance.minimapPointer != null)
            Destroy(instance.minimapPointer);

        foreach (GameObject enemy in instance.spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        foreach (GameObject civilian in instance.spawnedCivilians)
        {
            if (civilian != null)
                Destroy(civilian);
        }

        foreach (GameObject obj in instance.spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        if (instance.bossEnemy != null)
            Destroy(instance.bossEnemy);

        if (instance.activatedZone != null)
        {
            instance.activatedZone.gameObject.SetActive(false);
        }
        
        if (instance.activatedControlZone != null)
        {
            instance.activatedControlZone.gameObject.SetActive(false);
        }
        
        if (DynamicZoneManager.Instance != null)
        {
            DynamicZoneManager.Instance.ReleaseChallengeZone(challenge);
        }

        activeChallengeInstances.Remove(challenge);
    }

    private bool FindValidSpawnPosition(Vector3 center, float radius, List<Vector3> usedPositions, out Vector3 position, bool requireNavMesh = true)
    {
        position = center;
        
        int navMeshMisses = 0;
        int obstructionBlocks = 0;
        int spacingRejects = 0;

        for (int i = 0; i < maxNavMeshAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = center + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            if (requireNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    if (!Physics.CheckSphere(hit.position, 1f, obstructionMask))
                    {
                        if (!IsTooCloseToOtherSpawns(hit.position, usedPositions))
                        {
                            position = hit.position;
                            return true;
                        }
                        else
                        {
                            spacingRejects++;
                        }
                    }
                    else
                    {
                        obstructionBlocks++;
                    }
                }
                else
                {
                    navMeshMisses++;
                }
            }
            else
            {
                if (!Physics.CheckSphere(randomPoint, 1f, obstructionMask))
                {
                    if (!IsTooCloseToOtherSpawns(randomPoint, usedPositions))
                    {
                        position = randomPoint;
                        return true;
                    }
                    else
                    {
                        spacingRejects++;
                    }
                }
                else
                {
                    obstructionBlocks++;
                }
            }
        }

        // Log detailed failure reason
        if (requireNavMesh)
        {
            NavMeshHit testHit;
            bool navMeshExists = NavMesh.SamplePosition(center, out testHit, navMeshSampleDistance * 2, NavMesh.AllAreas);
            
            if (!navMeshExists)
            {
                Debug.LogWarning($"❌ No NavMesh found near {center}! Bake NavMesh or set requireNavMesh=false");
            }
            else
            {
                Debug.LogWarning($"⚠️ Spawn failed after {maxNavMeshAttempts} attempts | Radius: {radius}m | NavMesh misses: {navMeshMisses} | Obstructions: {obstructionBlocks} | Too close: {spacingRejects}");
                
                if (spacingRejects > obstructionBlocks && spacingRejects > navMeshMisses)
                {
                    Debug.LogWarning($"  → Increase spawn radius or reduce minimumSpawnDistance (current: {minimumSpawnDistance}m)");
                }
                else if (obstructionBlocks > navMeshMisses)
                {
                    Debug.LogWarning($"  → Too many obstructions. Check obstructionMask or increase spawn radius");
                }
                else
                {
                    Debug.LogWarning($"  → NavMesh coverage incomplete. Expand NavMesh bake area or increase radius");
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Spawn failed (no NavMesh) | Obstructions: {obstructionBlocks} | Too close: {spacingRejects} | Increase radius or reduce minimumSpawnDistance");
        }

        return false;
    }

    private bool IsTooCloseToOtherSpawns(Vector3 position, List<Vector3> usedPositions)
    {
        foreach (Vector3 usedPos in usedPositions)
        {
            if (Vector3.Distance(position, usedPos) < minimumSpawnDistance)
            {
                return true;
            }
        }
        return false;
    }

    public int GetRemainingEnemies(ActiveChallenge challenge)
    {
        if (!activeChallengeInstances.ContainsKey(challenge))
            return 0;

        ChallengeInstance instance = activeChallengeInstances[challenge];
        int count = 0;

        foreach (GameObject enemy in instance.spawnedEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
                count++;
        }

        if (instance.bossEnemy != null && instance.bossEnemy.activeInHierarchy)
            count++;

        return count;
    }

    public int GetRemainingCivilians(ActiveChallenge challenge)
    {
        if (!activeChallengeInstances.ContainsKey(challenge))
            return 0;

        ChallengeInstance instance = activeChallengeInstances[challenge];
        int count = 0;

        foreach (GameObject civilian in instance.spawnedCivilians)
        {
            if (civilian != null && civilian.activeInHierarchy)
                count++;
        }

        return count;
    }
    
    /// <summary>
    /// Apply difficulty scaling to an enemy based on challenge settings
    /// </summary>
    private void ApplyDifficultyScalingToEnemy(GameObject enemy, ActiveChallenge challenge)
    {
        if (enemy == null || challenge == null)
            return;
        
        // Try to apply to JUTPS health system
        JUTPS.JUHealth juHealth = enemy.GetComponent<JUTPS.JUHealth>();
        if (juHealth != null)
        {
            float originalHealth = juHealth.MaxHealth;
            float scaledHealth = originalHealth * challenge.enemyHealthMultiplier;
            juHealth.MaxHealth = scaledHealth;
            juHealth.Health = scaledHealth;
            
            Debug.Log($"Enemy scaled: Health {originalHealth:F0} → {scaledHealth:F0} (x{challenge.enemyHealthMultiplier:F2})");
        }
        
        // Apply damage scaling
        ApplyDamageScaling(enemy, challenge.enemyDamageMultiplier);
        
        // Apply modifiers
        ApplyEnemyModifiers(enemy, challenge);
    }
    
    /// <summary>
    /// Apply challenge modifiers to enemy
    /// </summary>
    private void ApplyEnemyModifiers(GameObject enemy, ActiveChallenge challenge)
    {
        if (enemy == null || challenge == null || challenge.challengeData == null)
            return;
        
        // Increased enemy speed
        if (challenge.challengeData.HasModifier(ChallengeData.ChallengeModifier.ModifierType.IncreasedEnemySpeed))
        {
            float speedMultiplier = challenge.challengeData.GetModifierValue(ChallengeData.ChallengeModifier.ModifierType.IncreasedEnemySpeed);
            
            JUTPS.JUCharacterController charController = enemy.GetComponent<JUTPS.JUCharacterController>();
            if (charController != null)
            {
                charController.WalkSpeed *= speedMultiplier;
                charController.RunSpeed *= speedMultiplier;
                Debug.Log($"Enemy speed increased by {speedMultiplier}x");
            }
        }
        
        // Elite enemies only
        if (challenge.challengeData.HasModifier(ChallengeData.ChallengeModifier.ModifierType.EliteEnemiesOnly))
        {
            // Mark enemy as elite (you may need to adjust based on your enemy system)
            EnemyKillRewardHandler rewardHandler = enemy.GetComponent<EnemyKillRewardHandler>();
            if (rewardHandler != null)
            {
                // Use reflection to set the private field, or make it public
                System.Reflection.FieldInfo isEliteField = typeof(EnemyKillRewardHandler).GetField("isElite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isEliteField != null)
                {
                    isEliteField.SetValue(rewardHandler, true);
                    Debug.Log("Enemy marked as Elite");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply damage multiplier to enemy's attacks
    /// </summary>
    private void ApplyDamageScaling(GameObject enemy, float damageMultiplier)
    {
        // NOTE: JUTPS weapon damage is configured on the bullet prefab, not the weapon itself.
        // We add a DifficultyDamageMultiplier component that your damage system can check.
        // If you have custom damage calculations, check for this component and apply the multiplier.
        
        DifficultyDamageMultiplier damageComponent = enemy.GetComponent<DifficultyDamageMultiplier>();
        if (damageComponent == null)
        {
            damageComponent = enemy.AddComponent<DifficultyDamageMultiplier>();
        }
        damageComponent.multiplier = damageMultiplier;
        
        Debug.Log($"Enemy damage scaling applied: x{damageMultiplier:F2}");
    }
}
