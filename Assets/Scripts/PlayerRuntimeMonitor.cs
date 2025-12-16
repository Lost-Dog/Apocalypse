using UnityEngine;

/// <summary>
/// Attach this to your player to monitor what happens when entering play mode
/// </summary>
public class PlayerRuntimeMonitor : MonoBehaviour
{
    private Vector3 startPosition;
    private bool wasActive = true;
    private float checkInterval = 0.5f;
    private float nextCheck = 0f;
    
    void Awake()
    {
        Debug.Log($"[PlayerMonitor] AWAKE - Player '{gameObject.name}' is starting up");
        Debug.Log($"[PlayerMonitor] Position: {transform.position}");
        Debug.Log($"[PlayerMonitor] Active: {gameObject.activeInHierarchy}");
        Debug.Log($"[PlayerMonitor] Tag: {tag}");
        startPosition = transform.position;
        
        // Check for JUAutoDestroy component
        var autoDestroy = GetComponent<JUTPS.Utilities.JUAutoDestroy>();
        if (autoDestroy != null)
        {
            Debug.LogError("[PlayerMonitor] ❌ FOUND JUAutoDestroy COMPONENT!");
            Debug.LogError("[PlayerMonitor] This will destroy the player!");
            Debug.LogError("[PlayerMonitor] REMOVING IT NOW...");
            Destroy(autoDestroy);
        }
        
        // Log all components
        Debug.Log("[PlayerMonitor] Components on player:");
        foreach (var component in GetComponents<Component>())
        {
            Debug.Log($"  - {component.GetType().Name}");
        }
    }
    
    void Start()
    {
        Debug.Log($"[PlayerMonitor] START - Player '{gameObject.name}' has started");
        Debug.Log($"[PlayerMonitor] Position: {transform.position}");
        
        // Check for common issues
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogWarning("[PlayerMonitor] ⚠️ No Rigidbody found!");
        }
        else
        {
            Debug.Log($"[PlayerMonitor] Rigidbody: useGravity={rigidbody.useGravity}, isKinematic={rigidbody.isKinematic}");
        }
        
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning("[PlayerMonitor] ⚠️ No Collider found!");
        }
        else
        {
            Debug.Log($"[PlayerMonitor] Collider: enabled={collider.enabled}, isTrigger={collider.isTrigger}");
        }
        
        CheckGroundBelow();
    }
    
    void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;
        
        // Check if position changed drastically
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        if (distanceFromStart > 100f)
        {
            Debug.LogWarning($"[PlayerMonitor] ⚠️ Player moved {distanceFromStart:F1}m from start position!");
            Debug.LogWarning($"[PlayerMonitor] Start: {startPosition}, Current: {transform.position}");
        }
        
        // Check if falling
        if (transform.position.y < -10f)
        {
            Debug.LogError($"[PlayerMonitor] ❌ PLAYER FALLING! Y = {transform.position.y}");
            Debug.LogError("[PlayerMonitor] Player is falling through the world!");
            
            // Reset position
            transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
            Debug.Log("[PlayerMonitor] Reset player to Y=2");
            
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        // Check if disabled
        if (!gameObject.activeInHierarchy && wasActive)
        {
            Debug.LogError("[PlayerMonitor] ❌ PLAYER WAS DISABLED!");
            wasActive = false;
        }
        else if (gameObject.activeInHierarchy && !wasActive)
        {
            Debug.Log("[PlayerMonitor] ✓ Player was re-enabled");
            wasActive = true;
        }
    }
    
    void OnDestroy()
    {
        Debug.LogError($"[PlayerMonitor] ❌ PLAYER IS BEING DESTROYED! '{gameObject.name}'");
        Debug.LogError("[PlayerMonitor] Something called Destroy() on the player!");
        Debug.LogError($"[PlayerMonitor] Stack trace:");
        Debug.LogError(System.Environment.StackTrace);
    }
    
    void OnDisable()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning($"[PlayerMonitor] ⚠️ Player disabled: '{gameObject.name}'");
        }
    }
    
    void OnEnable()
    {
        if (Application.isPlaying)
        {
            Debug.Log($"[PlayerMonitor] ✓ Player enabled: '{gameObject.name}'");
        }
    }
    
    private void CheckGroundBelow()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            Debug.Log($"[PlayerMonitor] ✓ Ground found {hit.distance:F2}m below player: {hit.collider.gameObject.name}");
        }
        else
        {
            Debug.LogError("[PlayerMonitor] ❌ NO GROUND DETECTED below player!");
            Debug.LogError("[PlayerMonitor] Player will fall through world!");
            Debug.LogError("[PlayerMonitor] Make sure terrain/ground has a Collider component!");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[PlayerMonitor] Collided with: {collision.gameObject.name}");
    }
}
