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
    
    private readonly HashSet<GameObject> scaledEnemies = new HashSet<GameObject>();
    private Collider[] overlapResults = new Collider[64];
    
    private void Start()
    {
        if (autoScaleOnEnter)
        {
            InvokeRepeating(nameof(ScanAndScaleEnemies), 1f, 2f);
        }
    }
    
    private void ScanAndScaleEnemies()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, overlapResults);

        // Grow once if the buffer is saturated so dense areas do not miss enemies.
        if (hitCount == overlapResults.Length)
        {
            overlapResults = new Collider[overlapResults.Length * 2];
            hitCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, overlapResults);
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = overlapResults[i];
            if (col == null || !col.CompareTag("Enemy")) continue;

            GameObject enemy = col.gameObject;
            if (scaledEnemies.Add(enemy))
            {
                ScaleEnemy(enemy);
            }
        }

        scaledEnemies.RemoveWhere(enemy => enemy == null);
    }
    
    public void ScaleEnemy(GameObject enemy)
    {
        int effectiveLevel = GetEffectiveLevel();

        // Convert zone level into a lightweight multiplier so this works
        // across any enemy framework without hard dependencies.
        float levelDelta = Mathf.Max(0f, effectiveLevel - 1f);
        float healthMultiplier = 1f + (levelDelta * 0.12f);
        float damageMultiplier = 1f + (levelDelta * 0.08f);

        DifficultyHealthMultiplier health = enemy.GetComponent<DifficultyHealthMultiplier>();
        if (health == null)
        {
            health = enemy.AddComponent<DifficultyHealthMultiplier>();
        }

        health.multiplier = Mathf.Max(0.01f, healthMultiplier);
        health.TryApplyToCommonHealthFields(enemy);

        DifficultyDamageMultiplier damage = enemy.GetComponent<DifficultyDamageMultiplier>();
        if (damage == null)
        {
            damage = enemy.AddComponent<DifficultyDamageMultiplier>();
        }

        damage.multiplier = Mathf.Max(0.01f, damageMultiplier);
        
        if (showDebugLogs)
        {
            Debug.Log($"Zone {gameObject.name}: Scaled {enemy.name} to level {effectiveLevel} (HP x{health.multiplier:F2}, DMG x{damage.multiplier:F2})");
        }
    }
    
    public void ScaleEnemiesInZone(List<GameObject> enemies)
    {
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
