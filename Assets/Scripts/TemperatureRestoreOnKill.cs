using UnityEngine;
using JUTPS;

/// <summary>
/// Skill that restores player temperature when killing enemies
/// </summary>
public class TemperatureRestoreOnKill : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("Enable/disable the skill")]
    public bool skillActive = true;
    
    [Tooltip("Activate skill on start")]
    public bool activateOnStart = true;
    
    [Header("Temperature Replenishment")]
    [Tooltip("Percentage of max temperature to restore (0-1)")]
    [Range(0f, 1f)]
    public float temperatureRestorePercentage = 1.0f;
    
    [Tooltip("Restore temperature instantly (true) or gradually over time (false)")]
    public bool instantRestore = true;
    
    [Tooltip("If gradual restore, duration in seconds")]
    public float gradualRestoreDuration = 2f;
    
    [Header("Visual/Audio Feedback")]
    [Tooltip("Show notification when temperature is restored")]
    public bool showNotification = true;
    
    [Tooltip("Audio clip to play when temperature is restored")]
    public AudioClip temperatureRestoreSound;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private JUCharacterController characterController;
    private JUHealth health;
    private SurvivalManager survivalManager;
    private AudioSource audioSource;
    private int killCount = 0;
    
    private bool isGraduallyRestoring = false;
    private float gradualRestoreTimer = 0f;
    private float targetTemperature = 0f;
    private float startTemperature = 0f;
    
    void Start()
    {
        if (activateOnStart)
        {
            ActivateSkill();
        }
    }
    
    void Update()
    {
        if (isGraduallyRestoring)
        {
            UpdateGradualRestore();
        }
    }
    
    public void ActivateSkill()
    {
        // Get required components
        characterController = GetComponent<JUCharacterController>();
        health = GetComponent<JUHealth>();
        survivalManager = SurvivalManager.Instance;
        
        if (characterController == null)
        {
            Debug.LogError("[TemperatureRestoreOnKill] No JUCharacterController found!");
            return;
        }
        
        if (survivalManager == null)
        {
            Debug.LogWarning("[TemperatureRestoreOnKill] No SurvivalManager instance found - skill won't work!");
            return;
        }
        
        if (!survivalManager.enableTemperatureSystem)
        {
            Debug.LogWarning("[TemperatureRestoreOnKill] Temperature system is disabled in SurvivalManager!");
        }
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && temperatureRestoreSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        skillActive = true;
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[TemperatureRestoreOnKill] Skill activated! Temperature restore: {temperatureRestorePercentage * 100}%</color>");
        }
    }
    
    public void DeactivateSkill()
    {
        skillActive = false;
        isGraduallyRestoring = false;
        
        if (debugMode)
        {
            Debug.Log("<color=yellow>[TemperatureRestoreOnKill] Skill deactivated!</color>");
        }
    }
    
    /// <summary>
    /// Call this method when player kills an enemy
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (!skillActive || survivalManager == null) return;
        
        if (!survivalManager.enableTemperatureSystem) return;
        
        killCount++;
        
        if (debugMode)
        {
            Debug.Log($"<color=green>[TemperatureRestoreOnKill] Enemy killed! Total kills: {killCount}</color>");
        }
        
        RestoreTemperature();
    }
    
    private void RestoreTemperature()
    {
        if (survivalManager == null) return;
        
        float temperatureToRestore = survivalManager.maxTemperature * temperatureRestorePercentage;
        
        if (instantRestore)
        {
            RestoreTemperatureInstant(temperatureToRestore);
        }
        else
        {
            RestoreTemperatureGradual(temperatureToRestore);
        }
    }
    
    private void RestoreTemperatureInstant(float amount)
    {
        float oldTemperature = survivalManager.currentTemperature;
        
        // Calculate target temperature (don't exceed max)
        float newTemperature = Mathf.Min(survivalManager.currentTemperature + amount, survivalManager.maxTemperature);
        
        survivalManager.SetTemperature(newTemperature);
        
        float actualRestore = newTemperature - oldTemperature;
        
        if (actualRestore > 0f)
        {
            if (debugMode)
            {
                Debug.Log($"<color=green>[TemperatureRestoreOnKill] Restored {actualRestore:F1}°C temperature on kill! (Temperature: {survivalManager.currentTemperature:F1}/{survivalManager.maxTemperature:F1}°C)</color>");
            }
            
            PlayFeedback();
        }
    }
    
    private void RestoreTemperatureGradual(float amount)
    {
        startTemperature = survivalManager.currentTemperature;
        targetTemperature = Mathf.Min(survivalManager.currentTemperature + amount, survivalManager.maxTemperature);
        
        isGraduallyRestoring = true;
        gradualRestoreTimer = 0f;
        
        if (debugMode)
        {
            Debug.Log($"<color=green>[TemperatureRestoreOnKill] Starting gradual temperature restore from {startTemperature:F1}°C to {targetTemperature:F1}°C over {gradualRestoreDuration}s</color>");
        }
        
        PlayFeedback();
    }
    
    private void UpdateGradualRestore()
    {
        if (!isGraduallyRestoring) return;
        
        gradualRestoreTimer += Time.deltaTime;
        float t = Mathf.Clamp01(gradualRestoreTimer / gradualRestoreDuration);
        
        float currentTemp = Mathf.Lerp(startTemperature, targetTemperature, t);
        survivalManager.SetTemperature(currentTemp);
        
        if (t >= 1f)
        {
            isGraduallyRestoring = false;
            
            if (debugMode)
            {
                Debug.Log($"<color=green>[TemperatureRestoreOnKill] Gradual temperature restore complete! Final temperature: {survivalManager.currentTemperature:F1}°C</color>");
            }
        }
    }
    
    private void PlayFeedback()
    {
        // Play sound effect
        if (audioSource != null && temperatureRestoreSound != null)
        {
            audioSource.PlayOneShot(temperatureRestoreSound);
        }
        
        // Show notification (implement later if needed)
        if (showNotification)
        {
            // TODO: Implement notification system integration
        }
    }
    
    public void SetRestorePercentage(float percentage)
    {
        temperatureRestorePercentage = Mathf.Clamp01(percentage);
        
        if (debugMode)
        {
            Debug.Log($"[TemperatureRestoreOnKill] Restore percentage set to: {temperatureRestorePercentage * 100}%");
        }
    }
    
    public void SetInstantRestore(bool instant)
    {
        instantRestore = instant;
        
        if (debugMode)
        {
            Debug.Log($"[TemperatureRestoreOnKill] Instant restore set to: {instant}");
        }
    }
}
