using UnityEngine;

/// <summary>
/// Example script showing how to use RuntimeLootSpawner
/// Add this to any GameObject to test loot spawning
/// </summary>
public class RuntimeLootSpawnerTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int initialSpawnCount = 5;
    
    [Header("Input Controls")]
    [SerializeField] private KeyCode spawnSingleKey = KeyCode.L;
    [SerializeField] private KeyCode spawnMultipleKey = KeyCode.K;
    [SerializeField] private KeyCode despawnAllKey = KeyCode.O;
    
    [Header("Spawn Parameters")]
    [SerializeField] private int multipleSpawnCount = 3;
    [SerializeField] private float customMinRadius = 15f;
    [SerializeField] private float customMaxRadius = 30f;

    private void Start()
    {
        if (spawnOnStart && RuntimeLootSpawner.Instance != null)
        {
            RuntimeLootSpawner.Instance.SpawnMultipleLoot(initialSpawnCount);
            Debug.Log($"Spawned {initialSpawnCount} loot items at start");
        }
    }

    private void Update()
    {
        if (RuntimeLootSpawner.Instance == null) return;

        // Spawn single loot
        if (Input.GetKeyDown(spawnSingleKey))
        {
            GameObject loot = RuntimeLootSpawner.Instance.SpawnLoot();
            if (loot != null)
            {
                Debug.Log($"Spawned loot: {loot.name}");
            }
        }

        // Spawn multiple loot with custom radius
        if (Input.GetKeyDown(spawnMultipleKey))
        {
            var lootList = RuntimeLootSpawner.Instance.SpawnMultipleLoot(
                multipleSpawnCount, 
                customMinRadius, 
                customMaxRadius
            );
            Debug.Log($"Spawned {lootList.Count} loot items");
        }

        // Despawn all loot
        if (Input.GetKeyDown(despawnAllKey))
        {
            RuntimeLootSpawner.Instance.DespawnAllLoot();
            Debug.Log("Despawned all loot");
        }
    }

    private void OnGUI()
    {
        if (RuntimeLootSpawner.Instance == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Box("Runtime Loot Spawner Test");
        
        GUILayout.Label($"Active Loot: {RuntimeLootSpawner.Instance.GetActiveLootCount()} / {RuntimeLootSpawner.Instance.GetMaxActiveLoot()}");
        GUILayout.Label($"Press {spawnSingleKey} to spawn single loot");
        GUILayout.Label($"Press {spawnMultipleKey} to spawn {multipleSpawnCount} loot items");
        GUILayout.Label($"Press {despawnAllKey} to despawn all");
        
        GUILayout.EndArea();
    }
}
