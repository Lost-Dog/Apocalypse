using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Position where loot will spawn")]
    public Transform spawnPoint;
    
    [Tooltip("Player level used for gear score calculation")]
    [Range(1, 30)]
    public int playerLevel = 1;
    
    [Header("Spawn Triggers")]
    [Tooltip("Spawn loot when this GameObject is destroyed")]
    public bool spawnOnDestroy = true;
    
    [Tooltip("Force a specific rarity (leave unchecked for random)")]
    public bool forceRarity = false;
    public LootManager.Rarity forcedRarity = LootManager.Rarity.Common;
    
    [Header("Multiple Drops")]
    [Tooltip("Number of loot items to drop")]
    [Range(1, 10)]
    public int dropCount = 1;
    
    [Tooltip("Spread radius for multiple drops")]
    public float spreadRadius = 2f;
    
    private void OnDestroy()
    {
        if (!spawnOnDestroy || !Application.isPlaying)
            return;
        
        SpawnLoot();
    }
    
    public void SpawnLoot()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootManager not found in scene!");
            return;
        }
        
        Vector3 basePosition = spawnPoint != null ? spawnPoint.position : transform.position;
        
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPosition = basePosition;
            
            if (dropCount > 1)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
                spawnPosition += new Vector3(randomOffset.x, 0, randomOffset.y);
            }
            
            if (forceRarity)
            {
                LootManager.Instance.DropLootWithRarity(spawnPosition, playerLevel, forcedRarity);
            }
            else
            {
                LootManager.Instance.DropLoot(spawnPosition, playerLevel);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 0.5f);
        
        if (dropCount > 1)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(position, spreadRadius);
        }
    }
}
