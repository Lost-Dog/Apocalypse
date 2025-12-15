using UnityEngine;
using System.Collections.Generic;

public class ChallengeZoneEnemyScaler : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Difficulty level of this zone (1-10+)")]
    public int zoneLevel = 1;
    
    [Tooltip("Scale enemies to player level if higher than zone level")]
    public bool scaleToPlayerLevel = true;
    
    [Header("Scaling Settings")]
    [Tooltip("Apply scaling when enemies enter this zone")]
    public bool autoScaleOnEnter = true;
    
    [Tooltip("Radius to detect and scale enemies")]
    public float detectionRadius = 50f;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool showDebugLogs = false;
    
    private List<GameObject> scaledEnemies = new List<GameObject>();
    
    private void Start()
    {
        if (autoScaleOnEnter)
        {
            InvokeRepeating(nameof(ScanAndScaleEnemies), 1f, 2f);
        }
    }
    
    private void ScanAndScaleEnemies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy") && !scaledEnemies.Contains(col.gameObject))
            {
                ScaleEnemy(col.gameObject);
                scaledEnemies.Add(col.gameObject);
            }
        }
        
        scaledEnemies.RemoveAll(e => e == null);
    }
    
    public void ScaleEnemy(GameObject enemy)
    {
        DifficultyScaler scaler = enemy.GetComponent<DifficultyScaler>();
        
        if (scaler == null)
        {
            scaler = enemy.AddComponent<DifficultyScaler>();
        }
        
        int effectiveLevel = GetEffectiveLevel();
        scaler.ApplyScaling(effectiveLevel);
        
        if (showDebugLogs)
        {
            Debug.Log($"Zone {gameObject.name}: Scaled {enemy.name} to level {effectiveLevel}");
        }
    }
    
    public void ScaleEnemiesInZone(List<GameObject> enemies)
    {
        int effectiveLevel = GetEffectiveLevel();
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                ScaleEnemy(enemy);
            }
        }
    }
    
    private int GetEffectiveLevel()
    {
        if (scaleToPlayerLevel && GameManager.Instance != null)
        {
            return Mathf.Max(zoneLevel, GameManager.Instance.currentPlayerLevel);
        }
        
        return zoneLevel;
    }
    
    public int GetZoneLevel()
    {
        return GetEffectiveLevel();
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"Zone Level: {(Application.isPlaying ? GetEffectiveLevel() : zoneLevel)}"
        );
    }
}
