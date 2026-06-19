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

    [Tooltip("Only optimize lights in this mask. Set to Everything to include all layers.")]
    public LayerMask optimizedLightLayers = ~0;
    
    [Tooltip("Disable shadows beyond this distance")]
    public bool disableShadowsBeyondMaxDistance = true;

    [Tooltip("Minimum seconds between enable/disable shadow state changes per light.")]
    public float minShadowStateChangeInterval = 0.75f;

    [Tooltip("Allow changing Light.shadowResolution at runtime. Disable to avoid shadow resource reallocations.")]
    public bool allowRuntimeShadowResolutionChanges = false;

    [Tooltip("Treat lights behind the camera as low priority and disable their shadows.")]
    public bool disableShadowsBehindCamera = true;

    [Range(-1f, 1f)]
    [Tooltip("Minimum forward dot product required for a light to be considered in front of the camera.")]
    public float inFrontDotThreshold = -0.1f;

    [Tooltip("Limit how many punctual lights can cast shadows at once (nearest in-view lights are prioritized).")]
    public bool limitActiveShadowCasters = true;

    [Min(1)]
    [Tooltip("Maximum number of punctual lights that can cast shadows simultaneously.")]
    public int maxActiveShadowCasters = 8;
    
    [Tooltip("Include directional lights in optimization")]
    public bool optimizeDirectionalLights = false;

    [Tooltip("Force all non-directional lights to never cast shadows (only the sun/directional light will cast shadows)")]
    public bool disableAllPunctualShadows = true;
    
    [Tooltip("Automatically find lights in scene on start")]
    public bool autoFindLights = true;
    
    private Camera mainCamera;
    private List<Light> managedLights = new List<Light>();
    private Dictionary<Light, LightShadows> originalShadowSettings = new Dictionary<Light, LightShadows>();
    private Dictionary<Light, UnityEngine.Rendering.LightShadowResolution> originalResolutions = new Dictionary<Light, UnityEngine.Rendering.LightShadowResolution>();
    private readonly Dictionary<Light, float> nextShadowStateChangeTime = new Dictionary<Light, float>();
    private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private readonly List<LightDistancePair> candidateShadowLights = new List<LightDistancePair>();
    private Coroutine optimizeRoutine;

    private struct LightDistancePair
    {
        public Light light;
        public float distanceSqr;
    }
    
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

        optimizeRoutine = StartCoroutine(OptimizeRoutine());
    }

    private System.Collections.IEnumerator OptimizeRoutine()
    {
        while (true)
        {
            if (mainCamera != null)
            {
                OptimizeLightShadows();
            }

            if (updateInterval <= 0f)
            {
                yield return waitForEndOfFrame;
            }
            else
            {
                yield return new WaitForSeconds(updateInterval);
            }
        }
    }
    
    public void FindAllLights()
    {
        managedLights.Clear();
        originalShadowSettings.Clear();
        originalResolutions.Clear();
        nextShadowStateChangeTime.Clear();
        
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        
        foreach (Light light in allLights)
        {
            if (ShouldForceNoShadows(light))
            {
                light.shadows = LightShadows.None;
                continue;
            }

            if (light.shadows != LightShadows.None)
            {
                if (!optimizeDirectionalLights && light.type == LightType.Directional)
                    continue;

                if ((optimizedLightLayers.value & (1 << light.gameObject.layer)) == 0)
                    continue;
                
                managedLights.Add(light);
                originalShadowSettings[light] = light.shadows;
                originalResolutions[light] = light.shadowResolution;
                nextShadowStateChangeTime[light] = 0f;
            }
        }
        
        Debug.Log($"LightShadowOptimizer: Managing {managedLights.Count} lights");
    }
    
    public void AddLight(Light light)
    {
        if (light == null || managedLights.Contains(light))
            return;

        if (ShouldForceNoShadows(light))
        {
            light.shadows = LightShadows.None;
            return;
        }
        
        managedLights.Add(light);
        originalShadowSettings[light] = light.shadows;
        originalResolutions[light] = light.shadowResolution;
        nextShadowStateChangeTime[light] = 0f;
    }

    private bool ShouldForceNoShadows(Light light)
    {
        if (light == null) return false;
        if (!disableAllPunctualShadows) return false;
        return light.type != LightType.Directional;
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
        nextShadowStateChangeTime.Remove(light);
    }
    
    private void OptimizeLightShadows()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;

        float highQualityDistanceSqr = highQualityDistance * highQualityDistance;
        float mediumQualityDistanceSqr = mediumQualityDistance * mediumQualityDistance;
        float lowQualityDistanceSqr = lowQualityDistance * lowQualityDistance;

        candidateShadowLights.Clear();
        
        for (int i = managedLights.Count - 1; i >= 0; i--)
        {
            Light light = managedLights[i];

            if (light == null)
            {
                managedLights.RemoveAt(i);
                continue;
            }
            
            float distanceSqr = (cameraPosition - light.transform.position).sqrMagnitude;

            if (disableShadowsBehindCamera)
            {
                Vector3 toLight = light.transform.position - cameraPosition;
                float toLightSqr = toLight.sqrMagnitude;
                if (toLightSqr > 0.0001f)
                {
                    float dot = Vector3.Dot(cameraForward, toLight / Mathf.Sqrt(toLightSqr));
                    if (dot < inFrontDotThreshold)
                    {
                        SetLightShadowQuality(light, lowQualityResolution, false);
                        continue;
                    }
                }
            }

            if (limitActiveShadowCasters)
            {
                if (distanceSqr < lowQualityDistanceSqr)
                {
                    candidateShadowLights.Add(new LightDistancePair
                    {
                        light = light,
                        distanceSqr = distanceSqr
                    });
                }
                else if (disableShadowsBeyondMaxDistance)
                {
                    SetLightShadowQuality(light, lowQualityResolution, false);
                }

                continue;
            }
            
            if (distanceSqr < highQualityDistanceSqr)
            {
                SetLightShadowQuality(light, highQualityResolution, true);
            }
            else if (distanceSqr < mediumQualityDistanceSqr)
            {
                SetLightShadowQuality(light, mediumQualityResolution, true);
            }
            else if (distanceSqr < lowQualityDistanceSqr)
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

        if (!limitActiveShadowCasters)
        {
            return;
        }

        candidateShadowLights.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

        int shadowBudget = Mathf.Max(1, maxActiveShadowCasters);
        for (int i = 0; i < candidateShadowLights.Count; i++)
        {
            Light light = candidateShadowLights[i].light;
            float distanceSqr = candidateShadowLights[i].distanceSqr;

            if (i >= shadowBudget)
            {
                SetLightShadowQuality(light, lowQualityResolution, false);
                continue;
            }

            if (distanceSqr < highQualityDistanceSqr)
            {
                SetLightShadowQuality(light, highQualityResolution, true);
            }
            else if (distanceSqr < mediumQualityDistanceSqr)
            {
                SetLightShadowQuality(light, mediumQualityResolution, true);
            }
            else
            {
                SetLightShadowQuality(light, lowQualityResolution, true);
            }
        }
    }
    
    private void SetLightShadowQuality(Light light, UnityEngine.Rendering.LightShadowResolution resolution, bool enableShadows)
    {
        if (!originalShadowSettings.ContainsKey(light))
            return;

        if (!nextShadowStateChangeTime.TryGetValue(light, out float nextAllowedTime))
        {
            nextAllowedTime = 0f;
            nextShadowStateChangeTime[light] = 0f;
        }

        float now = Time.unscaledTime;
        
        if (enableShadows)
        {
            LightShadows targetShadows = originalShadowSettings[light];
            if (light.shadows != targetShadows)
            {
                if (now < nextAllowedTime)
                {
                    return;
                }

                light.shadows = targetShadows;
                nextShadowStateChangeTime[light] = now + minShadowStateChangeInterval;
            }

            if (allowRuntimeShadowResolutionChanges && light.shadowResolution != resolution)
            {
                light.shadowResolution = resolution;
            }
        }
        else
        {
            if (light.shadows != LightShadows.None)
            {
                if (now < nextAllowedTime)
                {
                    return;
                }

                light.shadows = LightShadows.None;
                nextShadowStateChangeTime[light] = now + minShadowStateChangeInterval;
            }
        }
    }

    private void OnValidate()
    {
        if (highQualityDistance < 0f) highQualityDistance = 0f;
        if (mediumQualityDistance < highQualityDistance) mediumQualityDistance = highQualityDistance;
        if (lowQualityDistance < mediumQualityDistance) lowQualityDistance = mediumQualityDistance;
        if (updateInterval < 0f) updateInterval = 0f;
        if (minShadowStateChangeInterval < 0f) minShadowStateChangeInterval = 0f;
        if (maxActiveShadowCasters < 1) maxActiveShadowCasters = 1;
        inFrontDotThreshold = Mathf.Clamp(inFrontDotThreshold, -1f, 1f);
    }
    
    private void OnDisable()
    {
        if (optimizeRoutine != null)
        {
            StopCoroutine(optimizeRoutine);
            optimizeRoutine = null;
        }

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
