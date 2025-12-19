using UnityEngine;
using System.Collections;

public class ExplosionInfectionZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Maximum radius the zone expands to")]
    public float maxRadius = 50f;
    
    [Tooltip("Final radius after shrinking")]
    public float minRadius = 7f;
    
    [Tooltip("Time to expand to max radius")]
    public float expandDuration = 5f;
    
    [Tooltip("Time to shrink to min radius")]
    public float shrinkDuration = 5f;
    
    [Header("Infection Damage")]
    [Tooltip("Infection damage per second while in zone")]
    public float infectionDamagePerSecond = 5f;
    
    [Tooltip("How often to apply infection damage")]
    public float damageTickRate = 0.5f;
    
    [Header("Audio")]
    [Tooltip("Warning sound played when explosion starts")]
    public AudioClip warningSound;
    
    [Tooltip("Ambient loop sound while zone is active")]
    public AudioClip ambientLoopSound;
    
    [Header("Visual Feedback")]
    [Tooltip("Show debug sphere gizmo")]
    public bool showDebugGizmo = true;
    
    [Tooltip("Color of the zone in scene view")]
    public Color gizmoColor = new Color(0, 1, 0, 0.3f);
    
    private SphereCollider sphereCollider;
    private AudioSource audioSource;
    private float currentRadius = 0f;
    private bool isActive = false;
    private GameObject playerInZone = null;
    private float damageTimer = 0f;
    
    private void Awake()
    {
        // Create sphere collider
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 0f;
        
        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.maxDistance = 100f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }
    
    private void Start()
    {
        StartCoroutine(ZoneLifecycle());
    }
    
    private IEnumerator ZoneLifecycle()
    {
        isActive = true;
        
        // Play warning sound
        if (warningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Start ambient loop after warning
        if (ambientLoopSound != null && audioSource != null)
        {
            yield return new WaitForSeconds(warningSound != null ? warningSound.length : 0.5f);
            audioSource.clip = ambientLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Expand phase
        yield return StartCoroutine(ExpandZone());
        
        // Shrink phase
        yield return StartCoroutine(ShrinkZone());
        
        // Cleanup
        isActive = false;
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // Disable collider but don't destroy (parent explosion will handle lifetime)
        if (sphereCollider != null)
        {
            sphereCollider.enabled = false;
        }
    }
    
    private IEnumerator ExpandZone()
    {
        float elapsed = 0f;
        
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            currentRadius = Mathf.Lerp(0f, maxRadius, t);
            sphereCollider.radius = currentRadius;
            
            yield return null;
        }
        
        currentRadius = maxRadius;
        sphereCollider.radius = currentRadius;
    }
    
    private IEnumerator ShrinkZone()
    {
        float elapsed = 0f;
        
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            currentRadius = Mathf.Lerp(maxRadius, minRadius, t);
            sphereCollider.radius = currentRadius;
            
            yield return null;
        }
        
        currentRadius = minRadius;
        sphereCollider.radius = currentRadius;
    }
    
    private void Update()
    {
        if (!isActive || playerInZone == null) return;
        
        // Apply infection damage over time
        damageTimer += Time.deltaTime;
        
        if (damageTimer >= damageTickRate)
        {
            damageTimer = 0f;
            ApplyInfectionDamage();
        }
    }
    
    private void ApplyInfectionDamage()
    {
        if (playerInZone == null) return;
        
        SurvivalManager survivalManager = SurvivalManager.Instance;
        
        if (survivalManager != null)
        {
            float damageAmount = infectionDamagePerSecond * damageTickRate;
            survivalManager.AddInfection(damageAmount);
            
            Debug.Log($"<color=yellow>Explosion zone applying {damageAmount} infection damage</color>");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = other.gameObject;
            Debug.Log("<color=orange>Player entered explosion infection zone!</color>");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = null;
            Debug.Log("<color=green>Player left explosion infection zone</color>");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmo) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
        
        // Draw max radius for reference
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
