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
    public int maxActiveWorldEvents = 1;
    public int maxDailyChallenges = 3;
    public int maxWeeklyChallenges = 2;
    
    [Header("Visual Markers")]
    public GameObject worldMarkerPrefab;
    public GameObject compassMarkerPrefab;
    public Transform compassMarkerContainer;
    public Transform worldspaceUIContainer;
    public bool spawnWorldMarkers = true;
    public bool spawnCompassMarkers = true;
    public bool spawnMinimapPointers = true;
    
    [Header("Active Challenges")]
    public List<ActiveChallenge> activeChallenges = new List<ActiveChallenge>();
    public List<ActiveChallenge> dailyChallenges = new List<ActiveChallenge>();
    public List<ActiveChallenge> weeklyChallenges = new List<ActiveChallenge>();
    
    [Header("Challenge Events")]
    public UnityEvent<ActiveChallenge> onChallengeSpawned;
    public UnityEvent<ActiveChallenge> onChallengeStarted;
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
        
        // Check if already at max active challenges
        int activeWorldEvents = activeChallenges.Count(c => c.challengeData.frequency == ChallengeData.ChallengeFrequency.WorldEvent);
        if (activeWorldEvents >= maxActiveWorldEvents)
        {
            Debug.Log($"Max active challenges reached ({maxActiveWorldEvents}). Waiting for current challenge to complete.");
            return;
        }
        
        Transform spawnZone = spawnZones[Random.Range(0, spawnZones.Count)];
        ChallengeData challenge = worldEventChallenges[Random.Range(0, worldEventChallenges.Count)];
        
        SpawnChallenge(challenge, spawnZone.position);
    }
    
    public void SpawnChallenge(ChallengeData challenge, Vector3 position)
    {
        // Double-check max challenges before spawning
        int activeWorldEvents = activeChallenges.Count(c => c.challengeData.frequency == ChallengeData.ChallengeFrequency.WorldEvent);
        if (challenge.frequency == ChallengeData.ChallengeFrequency.WorldEvent && activeWorldEvents >= maxActiveWorldEvents)
        {
            Debug.LogWarning($"Cannot spawn challenge '{challenge.challengeName}': Max active challenges ({maxActiveWorldEvents}) already reached.");
            return;
        }
        ActiveChallenge activeChallenge = new ActiveChallenge(challenge, position, challenge.timeLimit);
        activeChallenges.Add(activeChallenge);
        
        // Check if challenge requires manual start
        if (challenge.requireManualStart)
        {
            // Keep in Discovered state, show discovery notification
            if (challenge.showDiscoveryNotification)
            {
                NotifyChallengeDiscovered(activeChallenge);
            }
            
            // Don't spawn content yet - wait for player to manually start
            Debug.Log($"Challenge discovered: {challenge.challengeName} (Manual start required)");
        }
        else
        {
            // Auto-start challenge
            activeChallenge.StartChallenge();
            
            if (challenge.startSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(challenge.startSound);
            }
            
            // Spawn VFX at challenge location
            if (challenge.spawnVFX != null)
            {
                GameObject fx = Instantiate(challenge.spawnVFX, position, Quaternion.identity);
                fx.transform.localScale = Vector3.one * challenge.spawnVFXScale;
                
                if (challenge.spawnVFXDuration > 0)
                {
                    Destroy(fx, challenge.spawnVFXDuration);
                }
            }
            
            // Spawn challenge content immediately
            SpawnChallengeContentInternal(activeChallenge);
            
            onChallengeStarted?.Invoke(activeChallenge);
        }
    }
    
    private void SpawnChallengeContentInternal(ActiveChallenge activeChallenge)
    {
        ChallengeData challenge = activeChallenge.challengeData;

        if (ChallengeSpawner.Instance != null)
        {
            Transform uiContainer = spawnWorldMarkers ? worldspaceUIContainer : null;
            GameObject worldPrefab = spawnWorldMarkers ? worldMarkerPrefab : null;
            GameObject compassPrefab = spawnCompassMarkers ? compassMarkerPrefab : null;
            Transform compassContainer = spawnCompassMarkers ? compassMarkerContainer : null;
            
            ChallengeSpawner.Instance.SpawnChallengeContent(
                activeChallenge,
                challenge,
                worldPrefab,
                compassPrefab,
                compassContainer,
                uiContainer,
                spawnMinimapPointers
            );
        }
        else
        {
            Debug.LogWarning("ChallengeSpawner.Instance is null! Create a ChallengeSpawner in the scene.");
        }
        
        onChallengeSpawned?.Invoke(activeChallenge);
        
        Debug.Log($"Challenge spawned: {challenge.challengeName} at {activeChallenge.position}");
    }

    private void UpdateActiveChallenges()
    {
        for (int i = activeChallenges.Count - 1; i >= 0; i--)
        {
            ActiveChallenge challenge = activeChallenges[i];
            
            // Only update timer for active challenges
            if (challenge.state == ActiveChallenge.ChallengeState.Active)
            {
                challenge.UpdateTimer(Time.deltaTime);
            }
            
            if (challenge.IsExpired() && challenge.state == ActiveChallenge.ChallengeState.Active)
            {
                // Handle challenge failure
                if (challenge.challengeData.allowRetry)
                {
                    challenge.FailChallenge();
                    Debug.Log($"Challenge failed (time expired): {challenge.challengeData.challengeName} - Retry available in {challenge.retryCooldown}s");
                    onChallengeExpired?.Invoke(challenge);
                }
                else
                {
                    // Permanent failure
                    onChallengeExpired?.Invoke(challenge);
                    CleanupChallenge(challenge);
                    activeChallenges.RemoveAt(i);
                    Debug.Log($"Challenge expired: {challenge.challengeData.challengeName}");
                }
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
        if (challenge.challengeData.completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challenge.challengeData.completeSound);
        }
        
        // Grant scaled rewards
        GrantChallengeRewards(challenge);

        onChallengeCompleted?.Invoke(challenge);
        CleanupChallenge(challenge);
        
        Debug.Log($"Challenge completed: {challenge.challengeData.challengeName} " +
                  $"(Difficulty: {challenge.actualDifficulty}, Level: {challenge.playerLevelAtSpawn}) " +
                  $"- Rewards: {challenge.GetXPReward()} XP, {challenge.GetCurrencyReward()} Credits");
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
    
    private void GrantChallengeRewards(ActiveChallenge challenge)
    {
        // Mark as completed to record completion time
        challenge.MarkCompleted();
        
        // Grant XP with all bonuses
        int xpReward = challenge.GetTotalXPReward();
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.AddExperience(xpReward);
            Debug.Log($"Granted {xpReward} XP for completing challenge (with bonuses)");
        }
        
        // Grant currency with modifiers
        int currencyReward = challenge.GetTotalCurrencyReward();
        // TODO: Integrate with your currency system when available
        Debug.Log($"Currency reward: {currencyReward}");
        
        // Spawn loot with scaled rarity and count
        if (LootManager.Instance != null)
        {
            LootManager.Rarity lootRarity = challenge.GetTotalLootRarity();
            int lootCount = challenge.GetTotalLootCount();
            
            for (int i = 0; i < lootCount; i++)
            {
                LootManager.Instance.DropLootWithRarity(challenge.position, challenge.playerLevelAtSpawn, lootRarity);
            }
            Debug.Log($"Spawned {lootCount}x {lootRarity} loot");
        }
        
        // Log bonus achievements
        string bonusSummary = challenge.GetBonusSummary();
        if (!string.IsNullOrEmpty(bonusSummary))
        {
            Debug.Log($"Bonus Achievements:\n{bonusSummary}");
        }
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

        int totalEnemies = challenge.challengeData.GetEnemyCount();
        Debug.Log($"Enemy killed! Progress: {challenge.enemiesKilled}/{totalEnemies}");

        if (challenge.enemiesKilled >= totalEnemies)
        {
            Debug.Log($"All enemies killed! Completing challenge...");
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

    // ========== Player Agency Methods ==========
    
    /// <summary>
    /// Get preview data for a challenge (for UI display before starting)
    /// </summary>
    public ChallengePreviewData GetChallengePreview(ActiveChallenge challenge)
    {
        if (challenge == null)
            return null;
        
        return challenge.GetPreviewData();
    }
    
    /// <summary>
    /// Manually start a discovered challenge
    /// </summary>
    public bool StartDiscoveredChallenge(ActiveChallenge challenge)
    {
        if (challenge == null)
        {
            Debug.LogWarning("Cannot start null challenge");
            return false;
        }
        
        if (challenge.state != ActiveChallenge.ChallengeState.Discovered && 
            challenge.state != ActiveChallenge.ChallengeState.Available)
        {
            Debug.LogWarning($"Challenge is in wrong state to start: {challenge.state}");
            return false;
        }
        
        // Start the challenge
        challenge.StartChallenge();
        
        // Spawn challenge content
        if (ChallengeSpawner.Instance != null)
        {
            GameObject worldPrefab = spawnWorldMarkers ? worldMarkerPrefab : null;
            GameObject compassPrefab = spawnCompassMarkers ? compassMarkerPrefab : null;
            Transform compassContainer = spawnCompassMarkers ? compassMarkerContainer : null;
            Transform uiContainer = spawnWorldMarkers ? worldspaceUIContainer : null;
            
            ChallengeSpawner.Instance.SpawnChallengeContent(
                challenge,
                challenge.challengeData,
                worldPrefab,
                compassPrefab,
                compassContainer,
                uiContainer,
                spawnMinimapPointers
            );
        }
        
        // Play start sound
        if (challenge.challengeData.startSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challenge.challengeData.startSound);
        }
        
        onChallengeStarted?.Invoke(challenge);
        Debug.Log($"Challenge started: {challenge.challengeData.challengeName} (Attempt {challenge.attemptCount})");
        
        return true;
    }
    
    /// <summary>
    /// Retry a failed challenge
    /// </summary>
    public bool RetryChallenge(ActiveChallenge challenge)
    {
        if (challenge == null)
        {
            Debug.LogWarning("Cannot retry null challenge");
            return false;
        }
        
        if (!challenge.CanRetry())
        {
            float cooldownRemaining = challenge.GetRetryCooldownRemaining();
            if (cooldownRemaining > 0f)
            {
                Debug.LogWarning($"Challenge cannot be retried yet. Cooldown: {cooldownRemaining:F0}s remaining");
            }
            else if (challenge.challengeData.maxAttempts > 0 && challenge.attemptCount >= challenge.challengeData.maxAttempts)
            {
                Debug.LogWarning($"Challenge has reached max attempts: {challenge.attemptCount}/{challenge.challengeData.maxAttempts}");
            }
            return false;
        }
        
        // Retry the challenge
        challenge.RetryChallenge();
        
        Debug.Log($"Challenge ready for retry: {challenge.challengeData.challengeName}");
        return true;
    }
    
    /// <summary>
    /// Get all discovered but not started challenges
    /// </summary>
    public List<ActiveChallenge> GetDiscoveredChallenges()
    {
        return activeChallenges.Where(c => c.state == ActiveChallenge.ChallengeState.Discovered).ToList();
    }
    
    /// <summary>
    /// Get all active (in progress) challenges
    /// </summary>
    public List<ActiveChallenge> GetActiveChallengesInProgress()
    {
        return activeChallenges.Where(c => c.state == ActiveChallenge.ChallengeState.Active).ToList();
    }
    
    /// <summary>
    /// Get all failed challenges
    /// </summary>
    public List<ActiveChallenge> GetFailedChallenges()
    {
        return activeChallenges.Where(c => c.state == ActiveChallenge.ChallengeState.Failed).ToList();
    }
    
    /// <summary>
    /// Get challenges available for retry
    /// </summary>
    public List<ActiveChallenge> GetRetryableChallenges()
    {
        return activeChallenges.Where(c => c.state == ActiveChallenge.ChallengeState.Failed && c.CanRetry()).ToList();
    }
    
    /// <summary>
    /// Show discovery notification for a challenge
    /// </summary>
    public void NotifyChallengeDiscovered(ActiveChallenge challenge)
    {
        if (challenge == null || !challenge.challengeData.showDiscoveryNotification)
            return;
        
        Debug.Log($"[DISCOVERY] {challenge.challengeData.challengeName}");
        Debug.Log($"Difficulty: {challenge.actualDifficulty} | Recommended Level: {challenge.challengeData.recommendedLevel}");
        Debug.Log($"Rewards: {challenge.GetXPReward()} XP (up to {challenge.GetTotalXPReward()} with bonuses)");
        
        // You can hook this up to your UI system
        // For example: UIManager.ShowChallengeDiscovery(challenge.GetPreviewData());
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
    public enum ChallengeState
    {
        Discovered,    // Player found it but hasn't started
        Active,        // Currently in progress
        Completed,     // Successfully finished
        Failed,        // Failed (time up, death, etc.)
        Available      // Can be retried
    }
    
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
    
    // Player agency
    public ChallengeState state;
    public bool hasBeenPreviewed;
    public float discoveryTime;
    public int attemptCount;
    public float lastFailTime;
    public float retryCooldown;
    
    // Difficulty scaling
    public int playerLevelAtSpawn;
    public ChallengeData.ChallengeDifficulty actualDifficulty;
    public float enemyHealthMultiplier;
    public float enemyDamageMultiplier;
    
    // Bonus tracking
    public float startTime;
    public float completionTime;
    public bool tookDamage;
    public bool wasDetected;
    public int killCount;
    
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
        
        // Initialize player agency tracking
        state = ChallengeState.Discovered;
        hasBeenPreviewed = false;
        discoveryTime = Time.time;
        attemptCount = 0;
        lastFailTime = 0f;
        retryCooldown = 0f;
        
        // Initialize bonus tracking
        startTime = Time.time;
        completionTime = 0f;
        tookDamage = false;
        wasDetected = false;
        killCount = 0;
        
        // Calculate difficulty scaling with modifiers
        playerLevelAtSpawn = GetCurrentPlayerLevel();
        actualDifficulty = data.CalculateScaledDifficulty(playerLevelAtSpawn);
        enemyHealthMultiplier = data.GetTotalHealthMultiplier(playerLevelAtSpawn, actualDifficulty);
        enemyDamageMultiplier = data.GetTotalDamageMultiplier(playerLevelAtSpawn, actualDifficulty);
        
        // Apply modifier to time limit
        timeRemaining = data.GetModifiedTimeLimit();
    }
    
    private int GetCurrentPlayerLevel()
    {
        // Try to get from ProgressionManager
        if (ProgressionManager.Instance != null)
            return ProgressionManager.Instance.currentLevel;
        
        // Fallback to GameManager
        if (GameManager.Instance != null)
            return GameManager.Instance.currentPlayerLevel;
        
        return 1; // Default level
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
    
    /// <summary>
    /// Get the scaled XP reward for completing this challenge
    /// </summary>
    public int GetXPReward()
    {
        return challengeData.GetScaledXPReward(playerLevelAtSpawn, actualDifficulty);
    }
    
    /// <summary>
    /// Get the total XP reward including all bonuses
    /// </summary>
    public int GetTotalXPReward()
    {
        bool perfectCompletion = !playerDied && !tookDamage;
        bool speedCompletion = CheckSpeedCompletion();
        bool stealthCompletion = !wasDetected && challengeData.requireStealth;
        
        return challengeData.GetTotalXPReward(playerLevelAtSpawn, actualDifficulty, perfectCompletion, speedCompletion, stealthCompletion);
    }
    
    /// <summary>
    /// Check if challenge was completed within speed threshold
    /// </summary>
    public bool CheckSpeedCompletion()
    {
        if (completionTime == 0f) return false;
        
        float timeUsed = completionTime - startTime;
        float timeLimit = challengeData.GetModifiedTimeLimit();
        float speedThreshold = timeLimit * challengeData.speedThresholdPercentage;
        
        return timeUsed <= speedThreshold;
    }
    
    /// <summary>
    /// Get the scaled currency reward for completing this challenge
    /// </summary>
    public int GetCurrencyReward()
    {
        return challengeData.GetScaledCurrencyReward(playerLevelAtSpawn, actualDifficulty);
    }
    
    /// <summary>
    /// Get the total currency reward including modifiers
    /// </summary>
    public int GetTotalCurrencyReward()
    {
        return challengeData.GetTotalCurrencyReward(playerLevelAtSpawn, actualDifficulty);
    }
    
    /// <summary>
    /// Get the scaled loot rarity for this challenge
    /// </summary>
    public LootManager.Rarity GetLootRarity()
    {
        return challengeData.GetScaledLootRarity(actualDifficulty);
    }
    
    /// <summary>
    /// Get the total loot rarity including modifiers
    /// </summary>
    public LootManager.Rarity GetTotalLootRarity()
    {
        return challengeData.GetTotalLootRarity(actualDifficulty);
    }
    
    /// <summary>
    /// Get the total loot count including modifiers
    /// </summary>
    public int GetTotalLootCount()
    {
        return challengeData.GetTotalLootCount();
    }
    
    /// <summary>
    /// Mark challenge as completed and record completion time
    /// </summary>
    public void MarkCompleted()
    {
        isCompleted = true;
        completionTime = Time.time;
    }
    
    /// <summary>
    /// Record that player took damage
    /// </summary>
    public void OnPlayerDamaged()
    {
        tookDamage = true;
    }
    
    /// <summary>
    /// Record that player was detected
    /// </summary>
    public void OnPlayerDetected()
    {
        wasDetected = true;
    }
    
    /// <summary>
    /// Record enemy kill
    /// </summary>
    public void OnEnemyKilled()
    {
        killCount++;
        enemiesKilled++;
    }
    
    /// <summary>
    /// Get a summary of bonus achievements
    /// </summary>
    public string GetBonusSummary()
    {
        string summary = "";
        
        if (!playerDied && !tookDamage)
        {
            summary += "FLAWLESS VICTORY!\n";
        }
        
        if (CheckSpeedCompletion())
        {
            summary += "SPEED RUN BONUS!\n";
        }
        
        if (!wasDetected && challengeData.requireStealth)
        {
            summary += "GHOST BONUS!\n";
        }
        
        return summary;
    }
    
    /// <summary>
    /// Get difficulty display text with level indicator
    /// </summary>
    public string GetDifficultyDisplayText()
    {
        string diffText = challengeData.GetDifficultyText();
        
        // Show if difficulty was scaled
        if (actualDifficulty != challengeData.difficulty)
        {
            diffText += $" (Scaled from {challengeData.GetDifficultyText()})";
        }
        
        return diffText;
    }
    
    // ========== Player Agency Methods ==========
    
    /// <summary>
    /// Start the challenge (from discovered state)
    /// </summary>
    public void StartChallenge()
    {
        if (state != ChallengeState.Discovered && state != ChallengeState.Available)
        {
            Debug.LogWarning("Cannot start challenge in current state: " + state);
            return;
        }
        
        state = ChallengeState.Active;
        attemptCount++;
        startTime = Time.time;
        timeRemaining = challengeData.GetModifiedTimeLimit();
        
        // Reset tracking
        tookDamage = false;
        wasDetected = false;
        killCount = 0;
        enemiesKilled = 0;
        civiliansRescued = 0;
        playerDied = false;
    }
    
    /// <summary>
    /// Mark challenge as failed
    /// </summary>
    public void FailChallenge()
    {
        state = ChallengeState.Failed;
        lastFailTime = Time.time;
        retryCooldown = challengeData.retryCooldownSeconds;
    }
    
    /// <summary>
    /// Check if challenge can be retried
    /// </summary>
    public bool CanRetry()
    {
        if (state != ChallengeState.Failed)
            return false;
        
        // Check cooldown
        if (retryCooldown > 0f && Time.time - lastFailTime < retryCooldown)
            return false;
        
        // Check max attempts (if limited)
        if (challengeData.maxAttempts > 0 && attemptCount >= challengeData.maxAttempts)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Retry a failed challenge
    /// </summary>
    public void RetryChallenge()
    {
        if (!CanRetry())
        {
            Debug.LogWarning("Cannot retry challenge");
            return;
        }
        
        state = ChallengeState.Available;
        // Challenge can now be started again via StartChallenge()
    }
    
    /// <summary>
    /// Get time remaining on retry cooldown
    /// </summary>
    public float GetRetryCooldownRemaining()
    {
        if (state != ChallengeState.Failed)
            return 0f;
        
        float elapsed = Time.time - lastFailTime;
        return Mathf.Max(0f, retryCooldown - elapsed);
    }
    
    /// <summary>
    /// Get preview data for UI display
    /// </summary>
    public ChallengePreviewData GetPreviewData()
    {
        hasBeenPreviewed = true;
        
        return new ChallengePreviewData
        {
            challengeName = challengeData.challengeName,
            description = challengeData.description,
            challengeType = challengeData.challengeType,
            difficulty = actualDifficulty,
            recommendedLevel = challengeData.recommendedLevel,
            playerLevel = playerLevelAtSpawn,
            
            // Time info
            timeLimit = timeRemaining,
            timeLimitFormatted = FormatTime(timeRemaining),
            
            // Objectives
            enemyCount = challengeData.GetEnemyCount(),
            civilianCount = challengeData.GetCivilianCount(),
            objectiveDescription = challengeData.objectiveDescription,
            requireNoDeaths = challengeData.requireNoDeaths,
            requireStealth = challengeData.requireStealth,
            
            // Rewards
            baseXP = GetXPReward(),
            maxXP = GetTotalXPReward(),
            baseCurrency = GetCurrencyReward(),
            maxCurrency = GetTotalCurrencyReward(),
            lootRarity = GetTotalLootRarity(),
            lootCount = GetTotalLootCount(),
            
            // Scaling
            enemyHealthMultiplier = enemyHealthMultiplier,
            enemyDamageMultiplier = enemyDamageMultiplier,
            
            // Modifiers
            modifiersDescription = challengeData.GetModifiersDescription(),
            hasModifiers = challengeData.modifiers.Count > 0,
            
            // Bonuses
            canGetPerfectBonus = challengeData.perfectCompletionBonus,
            canGetSpeedBonus = challengeData.speedCompletionBonus,
            canGetStealthBonus = challengeData.stealthCompletionBonus && challengeData.requireStealth,
            
            // State
            attemptCount = attemptCount,
            state = state
        };
    }
    
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
}

/// <summary>
/// Data structure for challenge preview UI
/// </summary>
[System.Serializable]
public class ChallengePreviewData
{
    // Basic info
    public string challengeName;
    public string description;
    public ChallengeData.ChallengeType challengeType;
    public ChallengeData.ChallengeDifficulty difficulty;
    public int recommendedLevel;
    public int playerLevel;
    
    // Time info
    public float timeLimit;
    public string timeLimitFormatted;
    
    // Objectives
    public int enemyCount;
    public int civilianCount;
    public string objectiveDescription;
    public bool requireNoDeaths;
    public bool requireStealth;
    
    // Rewards
    public int baseXP;
    public int maxXP;
    public int baseCurrency;
    public int maxCurrency;
    public LootManager.Rarity lootRarity;
    public int lootCount;
    
    // Scaling
    public float enemyHealthMultiplier;
    public float enemyDamageMultiplier;
    
    // Modifiers
    public string modifiersDescription;
    public bool hasModifiers;
    
    // Bonuses
    public bool canGetPerfectBonus;
    public bool canGetSpeedBonus;
    public bool canGetStealthBonus;
    
    // State
    public int attemptCount;
    public ActiveChallenge.ChallengeState state;
}
