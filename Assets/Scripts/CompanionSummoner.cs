using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;

/// <summary>
/// Summons a friendly companion AI near the player whenever the player takes damage.
/// Uses per-prefab object pooling. Companions are returned to the pool when they
/// die or move too far from the player.
/// Attach to any persistent scene GameObject (e.g. GameManager or a dedicated Summoner object).
/// </summary>
public class CompanionSummoner : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const int MaxNavMeshSampleAttempts = 15;
    private const float NavMeshSampleRadius = 5f;

    [Header("Companion Prefabs")]
    [Tooltip("Companion prefabs to summon. One is picked at random per summon.")]
    public List<GameObject> companionPrefabs = new List<GameObject>();

    [Header("Pool Settings")]
    [Tooltip("Pre-warmed instances created per prefab on Start.")]
    public int initialPoolSizePerPrefab = 2;

    [Header("Summon Settings")]
    [Tooltip("Maximum number of companions that may be active at the same time.")]
    public int maxActiveCompanions = 3;
    [Tooltip("Companions spawn within this radius around the player.")]
    public float summonRadius = 5f;
    [Tooltip("Minimum seconds between two consecutive summons (cooldown).")]
    public float summonCooldown = 5f;
    [Tooltip("How long a companion stays active before being automatically recalled. 0 = never.")]
    public float companionLifetime = 60f;

    [Header("Despawn Settings")]
    [Tooltip("Companions further than this distance from the player are returned to the pool.")]
    public float despawnDistance = 80f;
    [Tooltip("How often (seconds) the distance check runs.")]
    public float despawnCheckInterval = 2f;

    [Header("Player Reference")]
    [Tooltip("Assign the player Transform. If empty, searched at Start by tag + vThirdPersonController.")]
    public Transform playerOverride;

    [Header("Debug")]
    public bool logEvents = false;
    public bool showGizmos = true;

    // ── Pooling ───────────────────────────────────────────────────────────────

    private class PrefabPool
    {
        public GameObject prefab;
        public Queue<GameObject> available = new Queue<GameObject>();
    }

    private Dictionary<GameObject, PrefabPool> pools = new Dictionary<GameObject, PrefabPool>();

    // active instance → source prefab
    private Dictionary<GameObject, GameObject> activeInstances = new Dictionary<GameObject, GameObject>();

    // ── State ─────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private vHealthController playerHealth;
    private vAICompanionControl companionControl;
    private float lastSummonTime = float.NegativeInfinity;
    private float despawnTimer;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        FindPlayer();
        InitializePools();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
    }

    private void Update()
    {
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnCheckInterval)
        {
            despawnTimer = 0f;
            CheckDespawnDistance();
        }
    }

    // ── Player Discovery ──────────────────────────────────────────────────────

    private void FindPlayer()
    {
        if (playerOverride != null)
        {
            playerTransform = playerOverride;
        }
        else
        {
            foreach (GameObject candidate in GameObject.FindGameObjectsWithTag(PlayerTag))
            {
                if (candidate.GetComponent<vThirdPersonController>() != null)
                {
                    playerTransform = candidate.transform;
                    break;
                }
            }
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[CompanionSummoner] Player not found. Summons will not trigger.", this);
            return;
        }

        playerHealth = playerTransform.GetComponent<vHealthController>();
        if (playerHealth == null)
        {
            Debug.LogWarning("[CompanionSummoner] vHealthController not found on player.", this);
            return;
        }

        playerHealth.onReceiveDamage.AddListener(OnPlayerDamaged);

        // Register with vAICompanionControl if present so companions auto-target attackers.
        companionControl = playerTransform.GetComponent<vAICompanionControl>();

        if (logEvents)
            Debug.Log("[CompanionSummoner] Subscribed to player damage events.", this);
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerHealth != null)
            playerHealth.onReceiveDamage.RemoveListener(OnPlayerDamaged);
    }

    // ── Damage Callback ───────────────────────────────────────────────────────

    private void OnPlayerDamaged(vDamage damage)
    {
        if (playerTransform == null || playerHealth.isDead) return;
        if (activeInstances.Count >= maxActiveCompanions) return;
        if (Time.time - lastSummonTime < summonCooldown) return;

        TrySummon(damage.sender);
    }

    // ── Summon ────────────────────────────────────────────────────────────────

    private void TrySummon(Transform attacker)
    {
        if (companionPrefabs.Count == 0) return;
        if (!TryGetSummonPosition(out Vector3 spawnPosition)) return;

        GameObject prefab = companionPrefabs[Random.Range(0, companionPrefabs.Count)];
        GameObject instance = GetFromPool(prefab);
        if (instance == null) return;

        // Place and orient toward the attacker
        Quaternion rotation = attacker != null
            ? Quaternion.LookRotation((attacker.position - spawnPosition).normalized, Vector3.up)
            : Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        instance.transform.SetPositionAndRotation(spawnPosition, rotation);

        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.Warp(spawnPosition);

        instance.SetActive(true);
        activeInstances[instance] = prefab;
        lastSummonTime = Time.time;

        // Register with companion control so it gets targets relayed from player damage
        vAICompanion aiCompanion = instance.GetComponent<vAICompanion>();
        if (aiCompanion != null && companionControl != null
            && !companionControl.aICompanions.Contains(aiCompanion))
        {
            companionControl.aICompanions.Add(aiCompanion);
        }

        // Immediately point companion at the attacker
        if (attacker != null)
        {
            vControlAI controlAI = instance.GetComponent<vControlAI>();
            if (controlAI != null)
                controlAI.SetCurrentTarget(attacker, true);
        }

        if (companionLifetime > 0f)
            StartCoroutine(LifetimeRecall(instance, companionLifetime));

        if (logEvents)
            Debug.Log($"[CompanionSummoner] Summoned '{prefab.name}' at {spawnPosition}. Active: {activeInstances.Count}/{maxActiveCompanions}.", this);
    }

    // ── NavMesh Sampling ──────────────────────────────────────────────────────

    private bool TryGetSummonPosition(out Vector3 result)
    {
        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < MaxNavMeshSampleAttempts; i++)
        {
            Vector2 disc = Random.insideUnitCircle.normalized;
            float radius = Random.Range(summonRadius * 0.5f, summonRadius);
            Vector3 candidate = playerPos + new Vector3(disc.x, 0f, disc.y) * radius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        if (logEvents)
            Debug.LogWarning("[CompanionSummoner] No valid NavMesh position found for summon.", this);

        result = Vector3.zero;
        return false;
    }

    // ── Lifetime Recall ───────────────────────────────────────────────────────

    private IEnumerator LifetimeRecall(GameObject instance, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (instance != null && activeInstances.ContainsKey(instance))
            ReturnToPool(instance);
    }

    // ── Despawn Distance ──────────────────────────────────────────────────────

    private void CheckDespawnDistance()
    {
        if (playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        var toReturn = new List<GameObject>();

        foreach (KeyValuePair<GameObject, GameObject> pair in activeInstances)
        {
            if (pair.Key == null) continue;
            if (Vector3.Distance(playerPos, pair.Key.transform.position) > despawnDistance)
                toReturn.Add(pair.Key);
        }

        foreach (GameObject instance in toReturn)
            ReturnToPool(instance);
    }

    // ── Pool Management ───────────────────────────────────────────────────────

    private void InitializePools()
    {
        foreach (GameObject prefab in companionPrefabs)
        {
            if (prefab == null || pools.ContainsKey(prefab)) continue;

            PrefabPool pool = new PrefabPool { prefab = prefab };
            for (int i = 0; i < initialPoolSizePerPrefab; i++)
                pool.available.Enqueue(CreateInstance(prefab));

            pools[prefab] = pool;

            if (logEvents)
                Debug.Log($"[CompanionSummoner] Pool created for '{prefab.name}' ({initialPoolSizePerPrefab} instances).", this);
        }
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform);
        instance.SetActive(false);
        return instance;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out PrefabPool pool))
        {
            Debug.LogWarning($"[CompanionSummoner] No pool found for '{prefab.name}'.", this);
            return null;
        }

        if (pool.available.Count > 0)
            return pool.available.Dequeue();

        if (logEvents)
            Debug.Log($"[CompanionSummoner] Pool for '{prefab.name}' exhausted — expanding.", this);

        return CreateInstance(prefab);
    }

    /// <summary>
    /// Returns an active companion instance to the pool.
    /// Call this externally when a companion dies.
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        if (!activeInstances.TryGetValue(instance, out GameObject sourcePrefab))
        {
            if (logEvents)
                Debug.LogWarning($"[CompanionSummoner] ReturnToPool called for untracked instance '{instance.name}'.", this);
            return;
        }

        // Unregister from companion control
        vAICompanion aiCompanion = instance.GetComponent<vAICompanion>();
        if (aiCompanion != null && companionControl != null)
            companionControl.aICompanions.Remove(aiCompanion);

        instance.SetActive(false);
        activeInstances.Remove(instance);

        if (pools.TryGetValue(sourcePrefab, out PrefabPool pool))
            pool.available.Enqueue(instance);

        if (logEvents)
            Debug.Log($"[CompanionSummoner] '{instance.name}' returned to pool. Active: {activeInstances.Count}/{maxActiveCompanions}.", this);
    }

    // ── Public Queries ────────────────────────────────────────────────────────

    /// <summary>Number of currently active summoned companions.</summary>
    public int ActiveCount => activeInstances.Count;

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        Gizmos.color = Color.cyan;
        DrawGizmoCircle(center, summonRadius);

        Gizmos.color = Color.red;
        DrawGizmoCircle(center, despawnDistance);
    }

    private static void DrawGizmoCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = 360f / segments * Mathf.Deg2Rad;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
