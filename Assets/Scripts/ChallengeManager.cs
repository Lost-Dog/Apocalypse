using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance { get; private set; }

    [Header("Challenge Pools")]
    public List<ChallengeData> worldEventChallenges = new List<ChallengeData>();
    public List<ChallengeData> dailyChallengePool = new List<ChallengeData>();
    public List<ChallengeData> weeklyChallengePool = new List<ChallengeData>();
    
    [Header("Spawn Settings")]
    public List<Transform> spawnZones = new List<Transform>();
    public bool autoSpawnChallenges = true;
    public float worldEventSpawnInterval = 300f;
    public int maxActiveWorldEvents = 2;
    public int maxDailyChallenges = 3;
    public int maxWeeklyChallenges = 2;
    
    [Header("Visual Markers")]
    public GameObject worldMarkerPrefab;
    public GameObject compassMarkerPrefab;
    public Transform compassMarkerContainer;
    public bool spawnWorldMarkers = true;
    public bool spawnCompassMarkers = true;
    
    [Header("Active Challenges")]
    public List<ActiveChallenge> activeChallenges = new List<ActiveChallenge>();
    public List<ActiveChallenge> dailyChallenges = new List<ActiveChallenge>();
    public List<ActiveChallenge> weeklyChallenges = new List<ActiveChallenge>();
    
    [Header("Challenge Events")]
    public UnityEvent<ActiveChallenge> onChallengeSpawned;
    public UnityEvent<ActiveChallenge> onChallengeProgress;
    public UnityEvent<ActiveChallenge> onChallengeCompleted;
    public UnityEvent<ActiveChallenge> onChallengeExpired;
    public UnityEvent<ActiveChallenge> onChallengeFailed;
    
    private float spawnTimer;
    private AudioSource audioSource;
    private const string CHALLENGE_RESOURCE_PATH = "Challenges";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple ChallengeManager instances detected. Using the one on GameSystems.");
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        AutoPopulateSpawnZones();
        LoadChallenges();
        GenerateDailyChallenges();
        GenerateWeeklyChallenges();
        spawnTimer = worldEventSpawnInterval;
    }
    
    private void AutoPopulateSpawnZones()
    {
        if (spawnZones.Count == 0)
        {
            Transform challengeZonesParent = transform.Find("Zones/ChallengeZones");
            if (challengeZonesParent != null)
            {
                foreach (Transform child in challengeZonesParent)
                {
                    if (child.name.Contains("ChallengePoint"))
                    {
                        spawnZones.Add(child);
                    }
                }
                
                Debug.Log($"Auto-populated {spawnZones.Count} challenge spawn zones");
            }
            else
            {
                Debug.LogWarning("Could not find Zones/ChallengeZones to auto-populate spawn zones");
            }
        }
    }
    
    private void Update()
    {
        if (autoSpawnChallenges)
        {
            spawnTimer -= Time.deltaTime;
            
            int activeWorldEvents = activeChallenges.Count(c => c.challengeData.frequency == ChallengeData.ChallengeFrequency.WorldEvent);
            if (spawnTimer <= 0f && activeWorldEvents < maxActiveWorldEvents)
            {
                SpawnRandomWorldEvent();
                spawnTimer = worldEventSpawnInterval;
            }
        }
        
        UpdateActiveChallenges();
    }
    
    private void LoadChallenges()
    {
        if (worldEventChallenges.Count == 0)
        {
            ChallengeData[] loadedChallenges = Resources.LoadAll<ChallengeData>(CHALLENGE_RESOURCE_PATH);
            worldEventChallenges = new List<ChallengeData>(loadedChallenges.Where(c => c.frequency == ChallengeData.ChallengeFrequency.WorldEvent));
            dailyChallengePool = new List<ChallengeData>(loadedChallenges.Where(c => c.frequency == ChallengeData.ChallengeFrequency.Daily));
            weeklyChallengePool = new List<ChallengeData>(loadedChallenges.Where(c => c.frequency == ChallengeData.ChallengeFrequency.Weekly));
            
            Debug.Log($"Loaded {worldEventChallenges.Count} world events, {dailyChallengePool.Count} daily challenges, {weeklyChallengePool.Count} weekly challenges");
        }
    }

    public void GenerateDailyChallenges()
    {
        dailyChallenges.Clear();

        int toGenerate = Mathf.Min(maxDailyChallenges, dailyChallengePool.Count);
        List<ChallengeData> shuffled = dailyChallengePool.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < toGenerate; i++)
        {
            ActiveChallenge dailyChallenge = new ActiveChallenge(shuffled[i], Vector3.zero, 86400f);
            dailyChallenge.isDaily = true;
            dailyChallenges.Add(dailyChallenge);
        }

        Debug.Log($"Generated {dailyChallenges.Count} daily challenges");
    }

    public void GenerateWeeklyChallenges()
    {
        weeklyChallenges.Clear();

        int toGenerate = Mathf.Min(maxWeeklyChallenges, weeklyChallengePool.Count);
        List<ChallengeData> shuffled = weeklyChallengePool.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < toGenerate; i++)
        {
            ActiveChallenge weeklyChallenge = new ActiveChallenge(shuffled[i], Vector3.zero, 604800f);
            weeklyChallenge.isWeekly = true;
            weeklyChallenges.Add(weeklyChallenge);
        }

        Debug.Log($"Generated {weeklyChallenges.Count} weekly challenges");
    }
    
    public void SpawnRandomWorldEvent()
    {
        if (worldEventChallenges.Count == 0 || spawnZones.Count == 0)
        {
            Debug.LogWarning("Cannot spawn world event: no challenges or spawn zones available");
            return;
        }
        
        Transform spawnZone = spawnZones[Random.Range(0, spawnZones.Count)];
        ChallengeData challenge = worldEventChallenges[Random.Range(0, worldEventChallenges.Count)];
        
        SpawnChallenge(challenge, spawnZone.position);
    }
    
    public void SpawnChallenge(ChallengeData challenge, Vector3 position)
    {
        ActiveChallenge activeChallenge = new ActiveChallenge(challenge, position, challenge.timeLimit);
        activeChallenges.Add(activeChallenge);

        if (challenge.startSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challenge.startSound);
        }

        if (ChallengeSpawner.Instance != null)
        {
            ChallengeSpawner.Instance.SpawnChallengeContent(
                activeChallenge,
                challenge,
                worldMarkerPrefab,
                compassMarkerPrefab,
                compassMarkerContainer
            );
        }
        else
        {
            Debug.LogWarning("ChallengeSpawner.Instance is null! Create a ChallengeSpawner in the scene.");
        }
        
        onChallengeSpawned?.Invoke(activeChallenge);
        
        Debug.Log($"Challenge spawned: {challenge.challengeName} at {position}");
    }

    private void UpdateActiveChallenges()
    {
        for (int i = activeChallenges.Count - 1; i >= 0; i--)
        {
            ActiveChallenge challenge = activeChallenges[i];
            challenge.UpdateTimer(Time.deltaTime);
            
            if (challenge.IsExpired())
            {
                onChallengeExpired?.Invoke(challenge);
                CleanupChallenge(challenge);
                activeChallenges.RemoveAt(i);
                Debug.Log($"Challenge expired: {challenge.challengeData.challengeName}");
            }
            else if (challenge.IsCompleted())
            {
                CompleteChallenge(challenge);
                CleanupChallenge(challenge);
                activeChallenges.RemoveAt(i);
            }
        }
    }
    
    public void CompleteChallenge(ActiveChallenge challenge)
    {
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            GameManager.Instance.progressionManager.AddExperience(challenge.challengeData.xpReward);
        }

        if (challenge.challengeData.guaranteedLootCount > 0)
        {
            LootManager lootManager = FindFirstObjectByType<LootManager>();
            if (lootManager != null)
            {
                for (int i = 0; i < challenge.challengeData.guaranteedLootCount; i++)
                {
                    lootManager.DropLootWithRarity(challenge.position, 1, challenge.challengeData.guaranteedLootRarity);
                }
            }
        }

        if (challenge.challengeData.completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challenge.challengeData.completeSound);
        }
        
        onChallengeCompleted?.Invoke(challenge);
        Debug.Log($"Challenge completed: {challenge.challengeData.challengeName} - Rewards: {challenge.challengeData.xpReward} XP");
    }

    public void FailChallenge(ActiveChallenge challenge)
    {
        if (challenge.challengeData.failSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challenge.challengeData.failSound);
        }

        onChallengeFailed?.Invoke(challenge);
        CleanupChallenge(challenge);
        Debug.Log($"Challenge failed: {challenge.challengeData.challengeName}");
    }

    private void CleanupChallenge(ActiveChallenge challenge)
    {
        if (ChallengeSpawner.Instance != null)
        {
            ChallengeSpawner.Instance.CleanupChallenge(challenge);
        }
    }

    public void OnEnemyKilled(ActiveChallenge challenge)
    {
        challenge.enemiesKilled++;
        challenge.currentProgress = challenge.enemiesKilled;
        onChallengeProgress?.Invoke(challenge);

        if (challenge.enemiesKilled >= challenge.challengeData.GetEnemyCount())
        {
            challenge.MarkCompleted();
        }
    }

    public void OnCivilianRescued(ActiveChallenge challenge)
    {
        challenge.civiliansRescued++;
        challenge.currentProgress = challenge.civiliansRescued;
        onChallengeProgress?.Invoke(challenge);

        if (challenge.civiliansRescued >= challenge.challengeData.GetCivilianCount())
        {
            challenge.MarkCompleted();
        }
    }

    public void OnCivilianDied(ActiveChallenge challenge)
    {
        if (challenge.challengeData.requireNoDeaths)
        {
            FailChallenge(challenge);
            activeChallenges.Remove(challenge);
        }
    }

    public void UpdateChallengeProgress(ActiveChallenge challenge, int amount = 1)
    {
        challenge.currentProgress += amount;
        onChallengeProgress?.Invoke(challenge);

        if (challenge.currentProgress >= challenge.challengeData.GetEnemyCount() && !challenge.isCompleted)
        {
            challenge.MarkCompleted();
        }
    }
    
    public ActiveChallenge GetNearestChallenge(Vector3 position)
    {
        ActiveChallenge nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var challenge in activeChallenges)
        {
            float distance = Vector3.Distance(position, challenge.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = challenge;
            }
        }
        
        return nearest;
    }

    public void ClearExpiredChallenges()
    {
        activeChallenges.RemoveAll(c => c.IsExpired());
    }

    public void ResetDailyChallenges()
    {
        GenerateDailyChallenges();
    }

    public void ResetWeeklyChallenges()
    {
        GenerateWeeklyChallenges();
    }
}

[System.Serializable]
public class ActiveChallenge
{
    public ChallengeData challengeData;
    public Vector3 position;
    public float timeRemaining;
    public bool isCompleted;
    public bool isDaily;
    public bool isWeekly;
    public int currentProgress;
    public int enemiesKilled;
    public int civiliansRescued;
    public bool playerDied;
    
    public ActiveChallenge(ChallengeData data, Vector3 pos, float duration)
    {
        challengeData = data;
        position = pos;
        timeRemaining = duration;
        isCompleted = false;
        isDaily = false;
        isWeekly = false;
        currentProgress = 0;
        enemiesKilled = 0;
        civiliansRescued = 0;
        playerDied = false;
    }
    
    public void UpdateTimer(float deltaTime)
    {
        if (!isCompleted)
        {
            timeRemaining -= deltaTime;
        }
    }
    
    public bool IsExpired()
    {
        return !isCompleted && timeRemaining <= 0f;
    }
    
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    public void MarkCompleted()
    {
        isCompleted = true;
    }
    
    public float GetProgress()
    {
        int enemyCount = challengeData.GetEnemyCount();
        if (enemyCount > 0)
        {
            return Mathf.Clamp01((float)enemiesKilled / enemyCount);
        }
        
        int civilianCount = challengeData.GetCivilianCount();
        if (civilianCount > 0)
        {
            return Mathf.Clamp01((float)civiliansRescued / civilianCount);
        }

        return 0f;
    }
    
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timeRemaining);
    }

    public float GetTimeProgress()
    {
        return 1f - Mathf.Clamp01(timeRemaining / challengeData.timeLimit);
    }

    public string GetTimeRemainingText()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public bool IsPlayerInRange(Vector3 playerPosition)
    {
        return Vector3.Distance(position, playerPosition) <= challengeData.detectionRadius;
    }
}
