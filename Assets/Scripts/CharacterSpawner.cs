using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner Instance { get; private set; }

    [Header("Character Prefabs")]
    [SerializeField] private List<GameObject> civilianPrefabs = new List<GameObject>();
    
    [Header("Spawn Settings")]
    [SerializeField] private int maxActiveCharacters = 20;
    [SerializeField] private int initialPoolSize = 30;
    [SerializeField] private float spawnInterval = 2f;
    
    [Header("Distance Settings")]
    [SerializeField] private float minSpawnDistance = 30f;
    [SerializeField] private float maxSpawnDistance = 100f;
    [SerializeField] private float deactivateDistance = 120f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float navMeshSampleDistance = 5f;
    
    [Header("Performance Settings")]
    [SerializeField] private float distanceCheckInterval = 1f;
    [SerializeField] private bool enableAutoSpawn = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnEvents = false;
    
    private Dictionary<GameObject, Pool> characterPools = new Dictionary<GameObject, Pool>();
    private List<PooledCharacter> activeCharacters = new List<PooledCharacter>();
    private List<GameObject> availablePrefabsForSpawn = new List<GameObject>();
    private Transform playerTransform;
    private float spawnTimer;
    private float distanceCheckTimer;
    
    private class Pool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
        public List<GameObject> inUse = new List<GameObject>();
    }
    
    private class PooledCharacter
    {
        public GameObject gameObject;
        public Transform transform;
        public GameObject originalPrefab;
    }

    private void Awake()
    {
        Debug.Log($"<color=yellow>[CharacterSpawner] Awake() called! GameObject active: {gameObject.activeSelf}, enabled: {enabled}</color>");
        
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple CharacterSpawner instances detected.");
            Instance = this;
        }
    }

    private void Start()
    {
        Debug.Log($"<color=yellow>[CharacterSpawner] Start() called! Civilian prefabs count: {civilianPrefabs.Count}, enableAutoSpawn: {enableAutoSpawn}</color>");
        
        InitializePlayer();
        InitializePools();
        
        if (enableAutoSpawn)
        {
            Debug.LogWarning($"<color=red>[CharacterSpawner] AUTO-SPAWNING {maxActiveCharacters / 2} INITIAL CHARACTERS!</color>");
            SpawnInitialCharacters();
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            InitializePlayer();
            return;
        }

        if (enableAutoSpawn)
        {
            UpdateAutoSpawn();
        }

        UpdateDistanceChecks();
    }

    private void InitializePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("CharacterSpawner: Player not found! Make sure player has 'Player' tag.");
        }
    }

    private void InitializePools()
    {
        if (civilianPrefabs.Count == 0)
        {
            Debug.LogWarning("CharacterSpawner: No civilian prefabs assigned!");
            return;
        }

        Debug.Log($"<color=orange>[CharacterSpawner] InitializePools() - Creating pools for {civilianPrefabs.Count} prefabs with initialPoolSize: {initialPoolSize}</color>");

        foreach (GameObject prefab in civilianPrefabs)
        {
            if (prefab == null) continue;

            Pool pool = new Pool { prefab = prefab };
            
            int poolSizePerPrefab = Mathf.CeilToInt((float)initialPoolSize / civilianPrefabs.Count);
            
            Debug.Log($"<color=cyan>[CharacterSpawner] Creating {poolSizePerPrefab} instances of '{prefab.name}'</color>");
            
            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.name = $"{prefab.name}_{i}";
                obj.SetActive(false);
                pool.available.Enqueue(obj);
            }
            
            characterPools[prefab] = pool;
            
            if (logSpawnEvents)
            {
                Debug.Log($"CharacterSpawner: Initialized pool for {prefab.name} with {poolSizePerPrefab} instances");
            }
        }
        
        Debug.Log($"<color=green>[CharacterSpawner] Pool initialization complete! Total pools: {characterPools.Count}</color>");
        RefillAvailablePrefabs();
    }

    private void SpawnInitialCharacters()
    {
        int toSpawn = Mathf.Min(maxActiveCharacters / 2, initialPoolSize);
        
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnRandomCharacter();
        }
        
        if (logSpawnEvents)
        {
            Debug.Log($"CharacterSpawner: Spawned {toSpawn} initial characters");
        }
    }

    private void UpdateAutoSpawn()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            
            if (activeCharacters.Count < maxActiveCharacters)
            {
                SpawnRandomCharacter();
            }
        }
    }

    private void UpdateDistanceChecks()
    {
        distanceCheckTimer += Time.deltaTime;
        
        if (distanceCheckTimer >= distanceCheckInterval)
        {
            distanceCheckTimer = 0f;
            CheckCharacterDistances();
        }
    }

    private void CheckCharacterDistances()
    {
        if (playerTransform == null) return;

        for (int i = activeCharacters.Count - 1; i >= 0; i--)
        {
            PooledCharacter character = activeCharacters[i];
            
            if (character.gameObject == null || character.transform == null)
            {
                activeCharacters.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(playerTransform.position, character.transform.position);
            
            if (distance > deactivateDistance)
            {
                ReturnToPool(character);
                
                if (logSpawnEvents)
                {
                    Debug.Log($"CharacterSpawner: Deactivated {character.gameObject.name} (distance: {distance:F1}m)");
                }
            }
        }
    }

    public GameObject SpawnRandomCharacter()
    {
        if (civilianPrefabs.Count == 0 || playerTransform == null)
            return null;

        if (availablePrefabsForSpawn.Count == 0)
        {
            RefillAvailablePrefabs();
        }

        int randomIndex = Random.Range(0, availablePrefabsForSpawn.Count);
        GameObject selectedPrefab = availablePrefabsForSpawn[randomIndex];
        availablePrefabsForSpawn.RemoveAt(randomIndex);
        
        Vector3 spawnPosition;
        if (FindValidSpawnPosition(out spawnPosition))
        {
            return SpawnCharacter(selectedPrefab, spawnPosition);
        }
        
        return null;
    }

    private void RefillAvailablePrefabs()
    {
        availablePrefabsForSpawn.Clear();
        availablePrefabsForSpawn.AddRange(civilianPrefabs);
        
        if (logSpawnEvents)
        {
            Debug.Log($"CharacterSpawner: Refilled prefab pool with {availablePrefabsForSpawn.Count} unique prefabs");
        }
    }

    public GameObject SpawnCharacter(GameObject prefab, Vector3 position)
    {
        if (prefab == null || !characterPools.ContainsKey(prefab))
        {
            Debug.LogWarning($"CharacterSpawner: Prefab {prefab?.name} not in pool!");
            return null;
        }

        Pool pool = characterPools[prefab];
        GameObject obj;
        
        if (pool.available.Count > 0)
        {
            obj = pool.available.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
            obj.name = $"{prefab.name}_Extra_{pool.inUse.Count}";
            
            if (logSpawnEvents)
            {
                Debug.Log($"CharacterSpawner: Pool exhausted, created new instance of {prefab.name}");
            }
        }

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        obj.SetActive(true);

        pool.inUse.Add(obj);

        PooledCharacter pooledChar = new PooledCharacter
        {
            gameObject = obj,
            transform = obj.transform,
            originalPrefab = prefab
        };
        activeCharacters.Add(pooledChar);

        if (logSpawnEvents)
        {
            Debug.Log($"CharacterSpawner: Spawned {obj.name} at {position}");
        }

        return obj;
    }

    public void DespawnCharacter(GameObject character)
    {
        if (character == null) return;

        PooledCharacter pooledChar = activeCharacters.Find(c => c.gameObject == character);
        if (pooledChar != null)
        {
            ReturnToPool(pooledChar);
        }
    }

    private void ReturnToPool(PooledCharacter character)
    {
        if (character == null || character.gameObject == null) return;

        character.gameObject.SetActive(false);
        activeCharacters.Remove(character);

        if (characterPools.ContainsKey(character.originalPrefab))
        {
            Pool pool = characterPools[character.originalPrefab];
            pool.inUse.Remove(character.gameObject);
            pool.available.Enqueue(character.gameObject);
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
                position = hit.position;
                return true;
            }
        }

        if (logSpawnEvents)
        {
            Debug.LogWarning("CharacterSpawner: Failed to find valid spawn position after max attempts");
        }
        
        return false;
    }

    public void SetMaxActiveCharacters(int count)
    {
        maxActiveCharacters = Mathf.Max(0, count);
    }

    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    public void SetMinSpawnDistance(float distance)
    {
        minSpawnDistance = Mathf.Max(0f, distance);
        if (minSpawnDistance > maxSpawnDistance)
        {
            maxSpawnDistance = minSpawnDistance + 10f;
        }
    }

    public void SetMaxSpawnDistance(float distance)
    {
        maxSpawnDistance = Mathf.Max(minSpawnDistance, distance);
    }

    public void SetDeactivateDistance(float distance)
    {
        deactivateDistance = Mathf.Max(maxSpawnDistance, distance);
    }

    public void SetAutoSpawnEnabled(bool enabled)
    {
        enableAutoSpawn = enabled;
    }

    public void DespawnAllCharacters()
    {
        for (int i = activeCharacters.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeCharacters[i]);
        }
        
        activeCharacters.Clear();
    }

    public int GetActiveCharacterCount()
    {
        return activeCharacters.Count;
    }

    public int GetPooledCharacterCount()
    {
        int total = 0;
        foreach (var pool in characterPools.Values)
        {
            total += pool.available.Count;
        }
        return total;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || playerTransform == null)
            return;

        Gizmos.color = Color.yellow;
        DrawCircle(playerTransform.position, minSpawnDistance);
        
        Gizmos.color = Color.green;
        DrawCircle(playerTransform.position, maxSpawnDistance);
        
        Gizmos.color = Color.red;
        DrawCircle(playerTransform.position, deactivateDistance);
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
