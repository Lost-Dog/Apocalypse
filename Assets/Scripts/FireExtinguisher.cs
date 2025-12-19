using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using JUTPS.InteractionSystem;
using JUTPS.InteractionSystem.Interactables;

/// <summary>
/// Allows player to extinguish fires with a confirmation popup.
/// Works with Fire_Big FX prefabs.
/// </summary>
public class FireExtinguisher : JUInteractable
{
    [Header("Fire References")]
    [Tooltip("The fire particle system GameObject (Fire_Big FX)")]
    public GameObject fireObject;
    
    [Tooltip("Auto-find fire in children if not assigned")]
    public bool autoFindFire = true;
    
    [Header("UI Settings")]
    [Tooltip("Canvas with confirmation popup")]
    public GameObject confirmationUI;
    
    [Tooltip("Text for the prompt message")]
    public TextMeshProUGUI promptText;
    
    [Tooltip("Confirm button")]
    public Button confirmButton;
    
    [Tooltip("Cancel button")]
    public Button cancelButton;
    
    [Tooltip("Custom prompt message")]
    public string promptMessage = "Extinguish this fire?";
    
    [Header("Interaction Settings")]
    [Tooltip("Interaction prompt shown to player")]
    public string interactionPrompt = "Press E to Extinguish Fire";
    
    [Tooltip("Detection range for player")]
    public float interactionRange = 5f;
    
    [Header("Extinguish Settings")]
    [Tooltip("Time it takes for fire to fade out")]
    public float fadeOutDuration = 2f;
    
    [Tooltip("Disable object after extinguishing")]
    public bool disableAfterExtinguish = true;
    
    [Header("Effects")]
    [Tooltip("Sound to play when extinguishing")]
    public AudioClip extinguishSound;
    
    [Tooltip("Particle effect when extinguished")]
    public GameObject extinguishEffect;
    
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    
    [Header("Rewards (Optional)")]
    [Tooltip("XP reward for extinguishing")]
    public int xpReward = 25;
    
    [Tooltip("Award XP on extinguish")]
    public bool awardXP = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private bool isExtinguished = false;
    private bool isShowingUI = false;
    private ParticleSystem[] fireParticles;
    private Light[] fireLights;
    private AudioSource audioSource;
    private Transform playerTransform;
    
    protected override void Start()
    {
        base.Start();
        Initialize();
    }
    
    private void Initialize()
    {
        // Auto-find fire object
        if (autoFindFire && fireObject == null)
        {
            FindFireObject();
        }
        
        // Get particle systems and lights
        if (fireObject != null)
        {
            fireParticles = fireObject.GetComponentsInChildren<ParticleSystem>();
            fireLights = fireObject.GetComponentsInChildren<Light>();
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && extinguishSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
        
        // Setup UI buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmExtinguish);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelExtinguish);
        }
        
        // Hide UI initially
        if (confirmationUI != null)
        {
            confirmationUI.SetActive(false);
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[FireExtinguisher] Initialized on {gameObject.name}");
        }
    }
    
    private void FindFireObject()
    {
        // Look for Fire_Big in children
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Fire_Big") || child.name.Contains("FX_Fire"))
            {
                fireObject = child.gameObject;
                if (showDebugInfo)
                {
                    Debug.Log($"[FireExtinguisher] Auto-found fire: {fireObject.name}");
                }
                return;
            }
        }
        
        // If not found, look for particle systems
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            fireObject = ps.gameObject;
            if (showDebugInfo)
            {
                Debug.Log($"[FireExtinguisher] Using particle system as fire: {fireObject.name}");
            }
        }
    }
    
    public override bool CanInteract(JUInteractionSystem interactionSystem)
    {
        if (isExtinguished) return false;
        if (fireObject == null || !fireObject.activeSelf) return false;
        
        return true;
    }
    
    public override void Interact()
    {
        if (isExtinguished) return;
        
        ShowConfirmationUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"[FireExtinguisher] Player interacted with {gameObject.name}");
        }
        
        base.Interact();
    }
    
    private void ShowConfirmationUI()
    {
        if (confirmationUI == null)
        {
            // No UI - extinguish immediately
            ExtinguishFire();
            return;
        }
        
        isShowingUI = true;
        confirmationUI.SetActive(true);
        
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
        
        // Pause game or lock player controls here if needed
        Time.timeScale = 0f; // Pause game
        
        if (showDebugInfo)
        {
            Debug.Log("[FireExtinguisher] Showing confirmation UI");
        }
    }
    
    private void OnConfirmExtinguish()
    {
        HideConfirmationUI();
        ExtinguishFire();
    }
    
    private void OnCancelExtinguish()
    {
        HideConfirmationUI();
        
        if (showDebugInfo)
        {
            Debug.Log("[FireExtinguisher] Player cancelled");
        }
    }
    
    private void HideConfirmationUI()
    {
        if (confirmationUI != null)
        {
            confirmationUI.SetActive(false);
        }
        
        isShowingUI = false;
        Time.timeScale = 1f; // Unpause game
    }
    
    private void ExtinguishFire()
    {
        if (isExtinguished) return;
        
        isExtinguished = true;
        
        StartCoroutine(FadeOutFire());
        
        // Play sound
        if (extinguishSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(extinguishSound, soundVolume);
        }
        
        // Spawn effect
        if (extinguishEffect != null)
        {
            GameObject effect = Instantiate(extinguishEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }
        
        // Award XP
        if (awardXP && xpReward > 0)
        {
            ProgressionManager progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager != null)
            {
                progressionManager.AddExperience(xpReward);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[FireExtinguisher] Awarded {xpReward} XP");
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[FireExtinguisher] Fire extinguished on {gameObject.name}");
        }
    }
    
    private IEnumerator FadeOutFire()
    {
        float elapsed = 0f;
        
        // Store initial values
        float[] initialEmissionRates = new float[fireParticles.Length];
        float[] initialLightIntensities = new float[fireLights.Length];
        
        for (int i = 0; i < fireParticles.Length; i++)
        {
            var emission = fireParticles[i].emission;
            initialEmissionRates[i] = emission.rateOverTime.constant;
        }
        
        for (int i = 0; i < fireLights.Length; i++)
        {
            initialLightIntensities[i] = fireLights[i].intensity;
        }
        
        // Fade out
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float fadeValue = 1f - t;
            
            // Reduce particle emission
            for (int i = 0; i < fireParticles.Length; i++)
            {
                if (fireParticles[i] == null) continue;
                
                var emission = fireParticles[i].emission;
                var rate = emission.rateOverTime;
                rate.constant = initialEmissionRates[i] * fadeValue;
                emission.rateOverTime = rate;
            }
            
            // Reduce light intensity
            for (int i = 0; i < fireLights.Length; i++)
            {
                if (fireLights[i] == null) continue;
                fireLights[i].intensity = initialLightIntensities[i] * fadeValue;
            }
            
            yield return null;
        }
        
        // Stop all particles
        foreach (ParticleSystem ps in fireParticles)
        {
            if (ps != null) ps.Stop();
        }
        
        // Turn off lights
        foreach (Light light in fireLights)
        {
            if (light != null) light.enabled = false;
        }
        
        // Wait a bit for particles to clear
        yield return new WaitForSeconds(1f);
        
        // Disable fire object
        if (fireObject != null && disableAfterExtinguish)
        {
            fireObject.SetActive(false);
        }
        
        // Optionally disable this GameObject
        if (disableAfterExtinguish)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
