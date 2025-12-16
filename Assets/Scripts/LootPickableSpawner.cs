using UnityEngine;
using UnityEngine.AI;

public class LootPickableSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Maximum number of loot spawn attempts before pausing")]
    [SerializeField] private int maxTotalSpawns = 50;
    
    [Tooltip("How often to try spawning new loot (seconds)")]
    [SerializeField] private float spawnInterval = 10f;
    
    [Tooltip("Start spawning loot automatically when game starts")]
    [SerializeField] private bool enableAutoSpawn = true;
    
    [Header("Distance Settings")]
    [Tooltip("Minimum distance from player to spawn loot")]
    [SerializeField] private float minSpawnDistance = 20f;
    
    [Tooltip("Maximum distance from player to spawn loot")]
    [SerializeField] private float maxSpawnDistance = 60f;
    
    [Header("NavMesh Settings")]
    [Tooltip("Number of attempts to find valid spawn position")]
    [SerializeField] private int maxSpawnAttempts = 10;
    
    [Tooltip("NavMesh sample distance")]
    [SerializeField] private float navMeshSampleDistance = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnEvents = false;
    
    private int totalSpawnedCount = 0;
    private Transform playerTransform;
    private float spawnTimer;
    
    private void Start()
    {
        InitializePlayer();
        
        if (enableAutoSpawn)
        {
            SpawnInitialLoot();
        }
        
        // if (logSpawnEvents)
        // {
        //     Debug.Log($"<color=cyan>LootPickableSpawner: Ready! Max spawns: {maxTotalSpawns}, Spawn interval: {spawnInterval}s</color>");
        //     Debug.Log($"<color=cyan>Using LootManager rarity chances - check LootManager for rarity settings</color>");
        // }
    }
    
    private void Update()
    {
        if (playerTransform == null)
        {
            InitializePlayer();
            return;
        }
        
        if (enableAutoSpawn && totalSpawnedCount < maxTotalSpawns)
        {
            UpdateAutoSpawn();
        }
    }
    
    private void InitializePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    private void SpawnInitialLoot()
    {
        int initialCount = Mathf.Min(5, maxTotalSpawns);
        
        for (int i = 0; i < initialCount; i++)
        {
            SpawnRandomLoot();
        }
        
        // if (logSpawnEvents)
        // {
        //     Debug.Log($"<color=yellow>LootPickableSpawner: Spawned {initialCount} initial loot items</color>");
        // }
    }
    
    private void UpdateAutoSpawn()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnRandomLoot();
        }
    }
    
    public void SpawnRandomLoot()
    {
        if (totalSpawnedCount >= maxTotalSpawns)
        {
            // if (logSpawnEvents)
            // {
            //     Debug.Log("LootPickableSpawner: Reached max spawn limit");
            // }
            return;
        }
        
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootPickableSpawner: LootManager not found!");
            return;
        }
        
        if (playerTransform == null)
        {
            InitializePlayer();
            return;
        }
        
        Vector3 spawnPosition;
        if (FindValidSpawnPosition(out spawnPosition))
        {
            int playerLevel = 1;
            
            if (GameManager.Instance != null)
            {
                playerLevel = GameManager.Instance.currentPlayerLevel;
            }
            
            LootManager.Instance.DropLoot(spawnPosition, playerLevel);
            totalSpawnedCount++;
            
            // if (logSpawnEvents)
            // {
            //     Debug.Log($"<color=green>LootPickableSpawner: Spawned loot at {spawnPosition} (Total: {totalSpawnedCount}/{maxTotalSpawns})</color>");
            // }
        }
    }
    
    private bool FindValidSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;
        
        if (playerTransform == null)
            return false;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            Vector3 randomDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 targetPosition = playerTransform.position + randomDirection * distance;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                position = hit.position + Vector3.up * 0.5f;
                return true;
            }
        }
        
        // if (logSpawnEvents)
        // {
        //     Debug.LogWarning("LootPickableSpawner: Failed to find valid spawn position");
        // }
        
        return false;
    }
    
    public void ResetSpawnCount()
    {
        totalSpawnedCount = 0;
        
        // if (logSpawnEvents)
        // {
        //     Debug.Log("LootPickableSpawner: Spawn count reset");
        // }
    }
    
    public int GetTotalSpawnedCount()
    {
        return totalSpawnedCount;
    }
    
    public void SetAutoSpawnEnabled(bool enabled)
    {
        enableAutoSpawn = enabled;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        Transform center = playerTransform != null ? playerTransform : transform;
        
        Gizmos.color = Color.yellow;
        DrawCircle(center.position, minSpawnDistance);
        
        Gizmos.color = Color.green;
        DrawCircle(center.position, maxSpawnDistance);
    }
    
    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
