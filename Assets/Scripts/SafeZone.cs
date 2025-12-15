using UnityEngine;
using UnityEngine.Events;
using JUTPS;

public class SafeZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [Tooltip("Name of this safe zone")]
    public string safeZoneName = "Safe Zone";
    [Tooltip("Enable health restoration")]
    public bool restoreHealth = true;
    [Tooltip("Enable stamina restoration")]
    public bool restoreStamina = true;
    [Tooltip("Enable infection cure")]
    public bool cureInfection = true;
    [Tooltip("Enable temperature normalization")]
    public bool normalizeTemperature = true;
    
    [Header("Restoration Rates")]
    [Tooltip("Health restored per second")]
    public float healthRestoreRate = 10f;
    [Tooltip("Stamina restored per second")]
    public float staminaRestoreRate = 20f;
    [Tooltip("Infection reduced per second")]
    public float infectionCureRate = 5f;
    [Tooltip("Temperature adjustment speed")]
    public float temperatureNormalizeSpeed = 2f;
    
    [Header("Restoration Settings")]
    [Tooltip("Delay before restoration starts (seconds)")]
    public float restoreDelay = 1f;
    [Tooltip("Only restore when player is idle (not moving)")]
    public bool requireIdle = false;
    [Tooltip("Maximum distance player can move while idle")]
    public float idleMovementThreshold = 0.1f;
    
    [Header("Visual Feedback")]
    [Tooltip("Particle effect to spawn when player enters")]
    public GameObject enterEffect;
    [Tooltip("Particle effect to play while restoring")]
    public GameObject healingEffect;
    [Tooltip("Material to apply to zone mesh when active")]
    public Material activeZoneMaterial;
    [Tooltip("Color tint for healing effect")]
    public Color healingColor = new Color(0f, 1f, 0.5f, 0.3f);
    
    [Header("Audio")]
    [Tooltip("Sound to play when entering safe zone")]
    public AudioClip enterSound;
    [Tooltip("Sound to loop while healing")]
    public AudioClip healingSound;
    [Tooltip("Volume for sounds")]
    [Range(0f, 1f)] public float soundVolume = 0.5f;
    
    [Header("UI Feedback")]
    [Tooltip("Show safe zone message on screen")]
    public bool showUIMessage = true;
    [Tooltip("Message to display when entering")]
    public string enterMessage = "Entered Safe Zone - Restoring Stats";
    [Tooltip("Message duration")]
    public float messageDuration = 3f;
    
    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onRestoreComplete;
    
    private JUHealth playerHealth;
    private SurvivalManager survivalManager;
    private JUCharacterController playerController;
    private bool playerInZone = false;
    private float timeInZone = 0f;
    private Vector3 lastPlayerPosition;
    private GameObject activeHealingEffect;
    private AudioSource audioSource;
    private Renderer zoneRenderer;
    private Material originalMaterial;
    
    private void Start()
    {
        SetupPhysics();
        SetupAudio();
        SetupVisuals();
    }
    
    private void SetupPhysics()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            MeshCollider meshCol = col as MeshCollider;
            if (meshCol != null && !meshCol.convex)
            {
                Debug.LogWarning($"SafeZone '{safeZoneName}' has a concave MeshCollider. Converting to convex or adding BoxCollider trigger.");
                
                if (meshCol.sharedMesh != null && meshCol.sharedMesh.vertexCount < 256)
                {
                    meshCol.convex = true;
                    meshCol.isTrigger = true;
                }
                else
                {
                    Destroy(meshCol);
                    BoxCollider box = gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(10f, 5f, 10f);
                }
            }
            else
            {
                col.isTrigger = true;
            }
        }
        else
        {
            Debug.LogWarning($"SafeZone '{safeZoneName}' has no collider! Adding BoxCollider as trigger.");
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(10f, 5f, 10f);
        }
        
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider childCol in childColliders)
        {
            if (childCol.gameObject != gameObject)
            {
                childCol.isTrigger = true;
                Debug.Log($"<color=yellow>Set child collider '{childCol.gameObject.name}' to trigger</color>");
            }
        }
    }
    
    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (enterSound != null || healingSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = soundVolume;
        }
    }
    
    private void SetupVisuals()
    {
        zoneRenderer = GetComponent<Renderer>();
        if (zoneRenderer != null && activeZoneMaterial != null)
        {
            originalMaterial = zoneRenderer.material;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<JUHealth>();
            survivalManager = SurvivalManager.Instance;
            if (survivalManager == null)
            {
                survivalManager = FindFirstObjectByType<SurvivalManager>();
            }
            playerController = other.GetComponent<JUCharacterController>();
            
            if (playerHealth != null)
            {
                playerInZone = true;
                timeInZone = 0f;
                lastPlayerPosition = other.transform.position;
                
                OnPlayerEnterZone();
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInZone)
        {
            timeInZone += Time.deltaTime;
            
            if (timeInZone >= restoreDelay)
            {
                if (requireIdle)
                {
                    float movementDistance = Vector3.Distance(other.transform.position, lastPlayerPosition);
                    lastPlayerPosition = other.transform.position;
                    
                    if (movementDistance > idleMovementThreshold)
                    {
                        StopHealing();
                        return;
                    }
                }
                
                RestorePlayerStats();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInZone)
        {
            OnPlayerExitZone();
        }
    }
    
    private void OnPlayerEnterZone()
    {
        Debug.Log($"<color=green>Player entered {safeZoneName}</color>");
        
        if (survivalManager != null)
        {
            survivalManager.SetInSafeZone(true);
        }
        
        if (enterEffect != null)
        {
            GameObject effect = Instantiate(enterEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        if (enterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enterSound, soundVolume);
        }
        
        if (zoneRenderer != null && activeZoneMaterial != null)
        {
            zoneRenderer.material = activeZoneMaterial;
        }
        
        if (showUIMessage)
        {
            ShowSafeZoneMessage(enterMessage);
        }
        
        onPlayerEnter.Invoke();
    }
    
    private void OnPlayerExitZone()
    {
        Debug.Log($"<color=yellow>Player left {safeZoneName}</color>");
        
        if (survivalManager != null)
        {
            survivalManager.SetInSafeZone(false);
        }
        
        playerInZone = false;
        timeInZone = 0f;
        
        StopHealing();
        
        if (zoneRenderer != null && originalMaterial != null)
        {
            zoneRenderer.material = originalMaterial;
        }
        
        if (showUIMessage)
        {
            ShowSafeZoneMessage("Left Safe Zone");
        }
        
        onPlayerExit.Invoke();
        
        playerHealth = null;
        survivalManager = null;
        playerController = null;
    }
    
    private void RestorePlayerStats()
    {
        if (playerHealth == null) return;
        
        bool isRestoring = false;
        
        if (restoreHealth && playerHealth.Health < playerHealth.MaxHealth)
        {
            playerHealth.Health += healthRestoreRate * Time.deltaTime;
            playerHealth.Health = Mathf.Min(playerHealth.Health, playerHealth.MaxHealth);
            isRestoring = true;
        }
        
        if (survivalManager != null)
        {
            if (restoreStamina && survivalManager.currentStamina < survivalManager.maxStamina)
            {
                survivalManager.ModifyStamina(staminaRestoreRate * Time.deltaTime);
                isRestoring = true;
            }
            
            if (cureInfection && survivalManager.currentInfection > 0f)
            {
                survivalManager.CureInfection(infectionCureRate * Time.deltaTime);
                isRestoring = true;
            }
            
            if (normalizeTemperature && Mathf.Abs(survivalManager.currentTemperature - survivalManager.normalTemperature) > 0.1f)
            {
                survivalManager.currentTemperature = Mathf.Lerp(
                    survivalManager.currentTemperature,
                    survivalManager.normalTemperature,
                    temperatureNormalizeSpeed * Time.deltaTime
                );
                isRestoring = true;
            }
        }
        
        if (isRestoring)
        {
            if (activeHealingEffect == null && healingEffect != null)
            {
                StartHealing();
            }
        }
        else
        {
            if (activeHealingEffect != null)
            {
                StopHealing();
                onRestoreComplete.Invoke();
                Debug.Log($"<color=cyan>Player fully restored in {safeZoneName}</color>");
            }
        }
    }
    
    private void StartHealing()
    {
        if (healingEffect != null && activeHealingEffect == null)
        {
            activeHealingEffect = Instantiate(healingEffect, transform.position, Quaternion.identity, transform);
        }
        
        if (healingSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = healingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    private void StopHealing()
    {
        if (activeHealingEffect != null)
        {
            Destroy(activeHealingEffect);
            activeHealingEffect = null;
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    private void ShowSafeZoneMessage(string message)
    {
        GameObject messageDisplay = GameObject.Find("MessageDisplay");
        if (messageDisplay != null)
        {
            MessageDisplay display = messageDisplay.GetComponent<MessageDisplay>();
            if (display != null)
            {
                display.ShowMessage(message, messageDuration);
            }
        }
        else
        {
            Debug.Log($"<color=cyan>[Safe Zone] {message}</color>");
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = healingColor;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius * transform.localScale.x);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position, sphere.radius * transform.localScale.x);
            }
        }
    }
}
