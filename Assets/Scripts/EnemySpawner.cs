using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public int minEnemies = 2;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    
    [Header("Respawn Settings")]
    public bool shouldRespawn = true;
    public float respawnTime = 300f;
    
    [Header("Difficulty")]
    public int zoneLevel = 1;
    public bool scaleWithPlayerLevel = false;
    
    [Header("Spawn Behavior")]
    public bool spawnOnStart = true;
    public bool useNavMesh = true;
    
    [Header("Debug")]
    public bool showSpawnRadius = true;
    public Color gizmoColor = Color.red;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float respawnTimer;
    private bool hasSpawned = false;
    
    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }
    
    private void Update()
    {
        if (!shouldRespawn || !hasSpawned) return;
        
        if (AllEnemiesDefeated())
        {
            respawnTimer -= Time.deltaTime;
            
            if (respawnTimer <= 0f)
            {
                SpawnEnemies();
            }
        }
    }
    
    public void SpawnEnemies()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"No enemy prefabs assigned to spawner: {gameObject.name}");
            return;
        }
        
        ClearDeadEnemies();
        
        int count = Random.Range(minEnemies, maxEnemies + 1);
        int effectiveLevel = GetEffectiveZoneLevel();
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = SpawnEnemy(spawnPos, effectiveLevel);
            
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
            }
        }
        
        hasSpawned = true;
        Debug.Log($"Spawned {count} enemies at level {effectiveLevel}");
    }
    
    private GameObject SpawnEnemy(Vector3 position, int level)
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        
        ApplyDifficultyScaling(enemy, level);
        
        FactionMember factionMember = enemy.GetComponent<FactionMember>();
        if (factionMember == null)
        {
            factionMember = enemy.AddComponent<FactionMember>();
            factionMember.faction = FactionManager.Faction.Rogue;
        }
        
        return enemy;
    }
    
    private void ApplyDifficultyScaling(GameObject enemy, int level)
    {
        DifficultyScaler scaler = enemy.GetComponent<DifficultyScaler>();
        if (scaler == null)
        {
            scaler = enemy.AddComponent<DifficultyScaler>();
        }
        
        scaler.ApplyScaling(level);
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        bool validPositionFound = false;
        int maxAttempts = 10;
        int attempts = 0;
        
        while (!validPositionFound && attempts < maxAttempts)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            randomPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            if (useNavMesh)
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    randomPosition = hit.position;
                    validPositionFound = true;
                }
            }
            else
            {
                validPositionFound = true;
            }
            
            attempts++;
        }
        
        return randomPosition;
    }
    
    private int GetEffectiveZoneLevel()
    {
        if (scaleWithPlayerLevel && GameManager.Instance != null)
        {
            return Mathf.Max(zoneLevel, GameManager.Instance.currentPlayerLevel);
        }
        
        return zoneLevel;
    }
    
    private bool AllEnemiesDefeated()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null) return false;
        }
        
        return spawnedEnemies.Count > 0;
    }
    
    private void ClearDeadEnemies()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        respawnTimer = respawnTime;
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        spawnedEnemies.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpawnRadius) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
