using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ControlZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Control Point";
    public float captureRadius = 15f;
    public float captureTime = 10f;
    public bool requireClearEnemies = true;
    
    [Header("Visualization")]
    public Color neutralColor = Color.gray;
    public Color capturingColor = Color.yellow;
    public Color capturedColor = Color.green;
    public Color enemyColor = Color.red;
    public GameObject visualIndicator;
    public MeshRenderer zoneRenderer;
    
    [Header("Spawn Settings")]
    public int enemyCount = 5;
    public Transform[] enemySpawnPoints;
    public GameObject enemyPrefab;
    
    [Header("Management")]
    public bool spawnOnStart = false;
    
    [Header("Status")]
    public bool isCaptured = false;
    public bool isCapturable = false;
    public float captureProgress = 0f;
    
    [Header("Events")]
    public UnityEvent onCaptureStarted;
    public UnityEvent onCaptureProgress;
    public UnityEvent onCaptureCancelled;
    public UnityEvent onCaptureCompleted;
    public UnityEvent onEnemiesCleared;
    
    private Transform player;
    private bool playerInZone = false;
    private int enemiesRemaining;
    private ActiveChallenge linkedChallenge;
    private Material zoneMaterial;
    private bool hasSpawned = false;

    private void Start()
    {
        if (zoneRenderer != null)
        {
            zoneMaterial = zoneRenderer.material;
            SetZoneColor(neutralColor);
        }
        
        enemiesRemaining = enemyCount;
        
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }
    
    private void OnEnable()
    {
        if (!hasSpawned && !spawnOnStart && Application.isPlaying)
        {
            SpawnEnemies();
        }
    }

    private void Update()
    {
        if (isCaptured)
            return;
        
        FindPlayer();
        
        if (playerInZone && isCapturable)
        {
            CaptureZone();
        }
        else if (captureProgress > 0f && !playerInZone)
        {
            DecayCaptureProgress();
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void CaptureZone()
    {
        if (captureProgress == 0f)
        {
            onCaptureStarted?.Invoke();
            SetZoneColor(capturingColor);
            Debug.Log($"Started capturing {zoneName}");
        }
        
        captureProgress += Time.deltaTime / captureTime;
        onCaptureProgress?.Invoke();
        
        if (captureProgress >= 1f)
        {
            CompleteCaptureZone();
        }
    }

    private void DecayCaptureProgress()
    {
        captureProgress -= Time.deltaTime / (captureTime * 0.5f);
        captureProgress = Mathf.Max(0f, captureProgress);
        
        if (captureProgress <= 0f)
        {
            onCaptureCancelled?.Invoke();
            SetZoneColor(enemiesRemaining > 0 ? enemyColor : neutralColor);
            Debug.Log($"Capture cancelled for {zoneName}");
        }
    }

    private void CompleteCaptureZone()
    {
        isCaptured = true;
        captureProgress = 1f;
        SetZoneColor(capturedColor);
        onCaptureCompleted?.Invoke();
        
        if (linkedChallenge != null)
        {
            linkedChallenge.currentProgress++;
            
            if (ChallengeManager.Instance != null)
            {
                ChallengeManager.Instance.UpdateChallengeProgress(linkedChallenge, 0);
            }
        }
        
        Debug.Log($"{zoneName} captured!");
    }

    private void SpawnEnemies()
    {
        if (hasSpawned)
        {
            return;
        }
        
        if (enemyPrefab == null || enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            isCapturable = true;
            SetZoneColor(neutralColor);
            hasSpawned = true;
            return;
        }
        
        int spawnCount = Mathf.Min(enemyCount, enemySpawnPoints.Length);
        
        for (int i = 0; i < spawnCount; i++)
        {
            if (enemySpawnPoints[i] != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, enemySpawnPoints[i].rotation);
                
                if (enemy != null)
                {
                    JUTPS.JUHealth health = enemy.GetComponent<JUTPS.JUHealth>();
                    if (health != null)
                    {
                        health.OnDeath.AddListener(() => OnEnemyKilled());
                    }
                }
            }
        }
        
        SetZoneColor(enemyColor);
        hasSpawned = true;
    }
    
    public void ResetZone()
    {
        isCaptured = false;
        isCapturable = false;
        captureProgress = 0f;
        enemiesRemaining = enemyCount;
        hasSpawned = false;
        SetZoneColor(neutralColor);
    }

    public void OnEnemyKilled()
    {
        enemiesRemaining--;
        
        if (enemiesRemaining <= 0)
        {
            isCapturable = true;
            onEnemiesCleared?.Invoke();
            SetZoneColor(neutralColor);
            Debug.Log($"{zoneName} enemies cleared - zone is now capturable");
        }
    }

    private void SetZoneColor(Color color)
    {
        if (zoneMaterial != null)
        {
            zoneMaterial.color = color;
        }
    }

    public void LinkToChallenge(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isCaptured ? capturedColor : (isCapturable ? capturingColor : enemyColor);
        Gizmos.DrawWireSphere(transform.position, captureRadius);
        
        Gizmos.color = Color.yellow;
        if (enemySpawnPoints != null)
        {
            foreach (Transform spawn in enemySpawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireCube(spawn.position, Vector3.one * 2f);
                }
            }
        }
    }
}
