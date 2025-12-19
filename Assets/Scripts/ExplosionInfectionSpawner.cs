using UnityEngine;

/// <summary>
/// Add this component to your explosion prefab to automatically spawn an infection zone
/// </summary>
public class ExplosionInfectionSpawner : MonoBehaviour
{
    [Header("Infection Zone Settings")]
    [Tooltip("Prefab with ExplosionInfectionZone component (leave empty to create at runtime)")]
    public GameObject infectionZonePrefab;
    
    [Tooltip("Spawn infection zone when explosion is instantiated")]
    public bool spawnOnStart = true;
    
    [Header("Zone Configuration")]
    public float maxRadius = 50f;
    public float minRadius = 7f;
    public float expandDuration = 5f;
    public float shrinkDuration = 5f;
    public float infectionDamagePerSecond = 5f;
    
    [Header("Audio")]
    public AudioClip warningSound;
    public AudioClip ambientLoopSound;
    
    [Header("Explosion Lifetime")]
    [Tooltip("How long the explosion parent object stays active (seconds)")]
    public float explosionLifetime = 120f;
    
    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnInfectionZone();
        }
        
        // Destroy explosion parent after lifetime
        if (explosionLifetime > 0f)
        {
            Destroy(gameObject, explosionLifetime);
        }
    }
    
    public void SpawnInfectionZone()
    {
        GameObject zoneObject;
        
        if (infectionZonePrefab != null)
        {
            // Use prefab if provided
            zoneObject = Instantiate(infectionZonePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Create at runtime
            zoneObject = new GameObject("ExplosionInfectionZone");
            zoneObject.transform.position = transform.position;
            
            ExplosionInfectionZone zone = zoneObject.AddComponent<ExplosionInfectionZone>();
            
            // Configure zone
            zone.maxRadius = maxRadius;
            zone.minRadius = minRadius;
            zone.expandDuration = expandDuration;
            zone.shrinkDuration = shrinkDuration;
            zone.infectionDamagePerSecond = infectionDamagePerSecond;
            zone.warningSound = warningSound;
            zone.ambientLoopSound = ambientLoopSound;
        }
        
        Debug.Log($"<color=cyan>Spawned explosion infection zone at {transform.position}</color>");
    }
}
