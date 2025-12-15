using UnityEngine;
using System.Collections.Generic;

public class LightShadowOptimizer : MonoBehaviour
{
    [Header("Distance Thresholds")]
    [Tooltip("Distance for high quality shadows")]
    public float highQualityDistance = 20f;
    
    [Tooltip("Distance for medium quality shadows")]
    public float mediumQualityDistance = 50f;
    
    [Tooltip("Distance for low quality shadows")]
    public float lowQualityDistance = 100f;
    
    [Header("Shadow Resolutions")]
    public UnityEngine.Rendering.LightShadowResolution highQualityResolution = UnityEngine.Rendering.LightShadowResolution.High;
    public UnityEngine.Rendering.LightShadowResolution mediumQualityResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
    public UnityEngine.Rendering.LightShadowResolution lowQualityResolution = UnityEngine.Rendering.LightShadowResolution.Low;
    
    [Header("Settings")]
    [Tooltip("How often to update light shadows (in seconds)")]
    public float updateInterval = 0.5f;
    
    [Tooltip("Disable shadows beyond this distance")]
    public bool disableShadowsBeyondMaxDistance = true;
    
    [Tooltip("Include directional lights in optimization")]
    public bool optimizeDirectionalLights = false;
    
    [Tooltip("Automatically find lights in scene on start")]
    public bool autoFindLights = true;
    
    private Camera mainCamera;
    private List<Light> managedLights = new List<Light>();
    private Dictionary<Light, LightShadows> originalShadowSettings = new Dictionary<Light, LightShadows>();
    private Dictionary<Light, UnityEngine.Rendering.LightShadowResolution> originalResolutions = new Dictionary<Light, UnityEngine.Rendering.LightShadowResolution>();
    private float timeSinceLastUpdate = 0f;
    
    private const float DISTANCE_EPSILON = 0.1f;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("LightShadowOptimizer: No main camera found in scene!");
            enabled = false;
            return;
        }
        
        if (autoFindLights)
        {
            FindAllLights();
        }
    }
    
    private void Update()
    {
        if (mainCamera == null) return;
        
        timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            OptimizeLightShadows();
            timeSinceLastUpdate = 0f;
        }
    }
    
    public void FindAllLights()
    {
        managedLights.Clear();
        originalShadowSettings.Clear();
        originalResolutions.Clear();
        
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        
        foreach (Light light in allLights)
        {
            if (light.shadows != LightShadows.None)
            {
                if (!optimizeDirectionalLights && light.type == LightType.Directional)
                    continue;
                
                managedLights.Add(light);
                originalShadowSettings[light] = light.shadows;
                originalResolutions[light] = light.shadowResolution;
            }
        }
        
        Debug.Log($"LightShadowOptimizer: Managing {managedLights.Count} lights");
    }
    
    public void AddLight(Light light)
    {
        if (light == null || managedLights.Contains(light))
            return;
        
        managedLights.Add(light);
        originalShadowSettings[light] = light.shadows;
        originalResolutions[light] = light.shadowResolution;
    }
    
    public void RemoveLight(Light light)
    {
        if (light == null)
            return;
        
        if (originalShadowSettings.ContainsKey(light))
        {
            light.shadows = originalShadowSettings[light];
            light.shadowResolution = originalResolutions[light];
        }
        
        managedLights.Remove(light);
        originalShadowSettings.Remove(light);
        originalResolutions.Remove(light);
    }
    
    private void OptimizeLightShadows()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        
        foreach (Light light in managedLights)
        {
            if (light == null)
                continue;
            
            float distance = Vector3.Distance(cameraPosition, light.transform.position);
            
            if (distance < highQualityDistance)
            {
                SetLightShadowQuality(light, highQualityResolution, true);
            }
            else if (distance < mediumQualityDistance)
            {
                SetLightShadowQuality(light, mediumQualityResolution, true);
            }
            else if (distance < lowQualityDistance)
            {
                SetLightShadowQuality(light, lowQualityResolution, true);
            }
            else
            {
                if (disableShadowsBeyondMaxDistance)
                {
                    SetLightShadowQuality(light, lowQualityResolution, false);
                }
                else
                {
                    SetLightShadowQuality(light, lowQualityResolution, true);
                }
            }
        }
    }
    
    private void SetLightShadowQuality(Light light, UnityEngine.Rendering.LightShadowResolution resolution, bool enableShadows)
    {
        if (!originalShadowSettings.ContainsKey(light))
            return;
        
        if (enableShadows)
        {
            light.shadows = originalShadowSettings[light];
            light.shadowResolution = resolution;
        }
        else
        {
            light.shadows = LightShadows.None;
        }
    }
    
    private void OnDisable()
    {
        RestoreAllLights();
    }
    
    private void OnDestroy()
    {
        RestoreAllLights();
    }
    
    private void RestoreAllLights()
    {
        foreach (var kvp in originalShadowSettings)
        {
            if (kvp.Key != null)
            {
                kvp.Key.shadows = kvp.Value;
            }
        }
        
        foreach (var kvp in originalResolutions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.shadowResolution = kvp.Value;
            }
        }
    }
}
