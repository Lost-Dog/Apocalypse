using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

public class SafeZoneManager : MonoBehaviour
{
    public static SafeZoneManager Instance { get; private set; }
    
    [Header("Safe Zone Tracking")]
    [Tooltip("Any component with UnityEvents named onPlayerEnter and onPlayerExit is treated as a safe zone.")]
    public List<MonoBehaviour> allSafeZones = new List<MonoBehaviour>();
    public bool autoFindSafeZones = true;
    
    [Header("Statistics")]
    public int totalSafeZonesEntered = 0;
    public float totalTimeInSafeZones = 0f;
    public float totalHealthRestored = 0f;
    
    [Header("Current Status")]
    public MonoBehaviour currentSafeZone = null;
    public bool playerInSafeZone = false;
    
    [Header("Debug")]
    public bool showDebugInfo = true;

    private sealed class ZoneBinding
    {
        public UnityEvent enterEvent;
        public UnityEvent exitEvent;
        public UnityAction onEnter;
        public UnityAction onExit;
    }
    
    private float sessionStartTime;
    private readonly Dictionary<MonoBehaviour, ZoneBinding> zoneBindings = new Dictionary<MonoBehaviour, ZoneBinding>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (autoFindSafeZones)
        {
            FindAllSafeZones();
        }
        
        RegisterEventListeners();
    }
    
    private void FindAllSafeZones()
    {
        allSafeZones.Clear();

        MonoBehaviour[] candidates = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour candidate in candidates)
        {
            if (candidate == null) continue;
            if (!TryCreateZoneBinding(candidate, out _)) continue;

            allSafeZones.Add(candidate);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>SafeZoneManager: Found {allSafeZones.Count} safe zones</color>");
        }
    }
    
    private void RegisterEventListeners()
    {
        zoneBindings.Clear();

        foreach (MonoBehaviour zone in allSafeZones)
        {
            if (zone == null) continue;
            RegisterSafeZone(zone);
        }
    }
    
    private void OnPlayerEnterAnyZone(MonoBehaviour zone)
    {
        currentSafeZone = zone;
        playerInSafeZone = true;
        totalSafeZonesEntered++;
        sessionStartTime = Time.time;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Player entered safe zone: {GetZoneName(zone)}</color>");
        }
    }
    
    private void OnPlayerExitAnyZone(MonoBehaviour zone)
    {
        if (currentSafeZone == zone)
        {
            float sessionDuration = Time.time - sessionStartTime;
            totalTimeInSafeZones += sessionDuration;
            
            currentSafeZone = null;
            playerInSafeZone = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=yellow>Player left safe zone: {GetZoneName(zone)} (Duration: {sessionDuration:F1}s)</color>");
            }
        }
    }
    
    public MonoBehaviour GetNearestSafeZone(Vector3 position)
    {
        if (allSafeZones.Count == 0) return null;
        
        MonoBehaviour nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (MonoBehaviour zone in allSafeZones)
        {
            if (zone == null) continue;
            
            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = zone;
            }
        }
        
        return nearest;
    }
    
    public List<MonoBehaviour> GetSafeZonesInRadius(Vector3 position, float radius)
    {
        List<MonoBehaviour> zonesInRange = new List<MonoBehaviour>();
        
        foreach (MonoBehaviour zone in allSafeZones)
        {
            if (zone == null) continue;
            
            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance <= radius)
            {
                zonesInRange.Add(zone);
            }
        }
        
        return zonesInRange;
    }
    
    public void RegisterSafeZone(MonoBehaviour zone)
    {
        if (zone == null) return;

        if (!TryCreateZoneBinding(zone, out ZoneBinding binding))
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"SafeZoneManager: {zone.name} is missing onPlayerEnter/onPlayerExit UnityEvents and cannot be registered.");
            }
            return;
        }

        if (!allSafeZones.Contains(zone))
        {
            allSafeZones.Add(zone);
        }

        if (zoneBindings.ContainsKey(zone)) return;

        binding.onEnter = () => OnPlayerEnterAnyZone(zone);
        binding.onExit = () => OnPlayerExitAnyZone(zone);

        binding.enterEvent.AddListener(binding.onEnter);
        binding.exitEvent.AddListener(binding.onExit);
        zoneBindings[zone] = binding;

        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>Registered safe zone: {GetZoneName(zone)}</color>");
        }
    }
    
    public void UnregisterSafeZone(MonoBehaviour zone)
    {
        if (zone == null) return;

        if (zoneBindings.TryGetValue(zone, out ZoneBinding binding))
        {
            if (binding.enterEvent != null && binding.onEnter != null)
                binding.enterEvent.RemoveListener(binding.onEnter);

            if (binding.exitEvent != null && binding.onExit != null)
                binding.exitEvent.RemoveListener(binding.onExit);

            zoneBindings.Remove(zone);
        }

        if (allSafeZones.Contains(zone))
        {
            allSafeZones.Remove(zone);
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=orange>Unregistered safe zone: {GetZoneName(zone)}</color>");
            }
        }
    }
    
    public void ShowSafeZoneStats()
    {
        Debug.Log($"<color=cyan>=== Safe Zone Statistics ===</color>");
        Debug.Log($"Total Safe Zones: {allSafeZones.Count}");
        Debug.Log($"Total Entries: {totalSafeZonesEntered}");
        Debug.Log($"Total Time: {totalTimeInSafeZones:F1} seconds");
        Debug.Log($"Currently In Zone: {playerInSafeZone}");
        if (currentSafeZone != null)
        {
            Debug.Log($"Current Zone: {GetZoneName(currentSafeZone)}");
        }
    }
    
    private void OnGUI()
    {
        if (showDebugInfo && playerInSafeZone && currentSafeZone != null)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 300, 20), $"Safe Zone: {GetZoneName(currentSafeZone)}");
            GUI.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        List<MonoBehaviour> zones = new List<MonoBehaviour>(zoneBindings.Keys);
        foreach (MonoBehaviour zone in zones)
        {
            UnregisterSafeZone(zone);
        }
    }

    private static bool TryCreateZoneBinding(MonoBehaviour zone, out ZoneBinding binding)
    {
        binding = null;
        if (zone == null) return false;

        if (!TryGetUnityEvent(zone, "onPlayerEnter", out UnityEvent enterEvent)) return false;
        if (!TryGetUnityEvent(zone, "onPlayerExit", out UnityEvent exitEvent)) return false;

        binding = new ZoneBinding
        {
            enterEvent = enterEvent,
            exitEvent = exitEvent
        };
        return true;
    }

    private static bool TryGetUnityEvent(MonoBehaviour zone, string memberName, out UnityEvent unityEvent)
    {
        unityEvent = null;

        FieldInfo field = zone.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && typeof(UnityEvent).IsAssignableFrom(field.FieldType))
        {
            unityEvent = field.GetValue(zone) as UnityEvent;
            return unityEvent != null;
        }

        PropertyInfo property = zone.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && typeof(UnityEvent).IsAssignableFrom(property.PropertyType))
        {
            unityEvent = property.GetValue(zone) as UnityEvent;
            return unityEvent != null;
        }

        return false;
    }

    private static string GetZoneName(MonoBehaviour zone)
    {
        if (zone == null) return "Unknown";

        FieldInfo field = zone.GetType().GetField("safeZoneName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(string))
        {
            string value = field.GetValue(zone) as string;
            if (!string.IsNullOrEmpty(value)) return value;
        }

        PropertyInfo property = zone.GetType().GetProperty("safeZoneName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.PropertyType == typeof(string))
        {
            string value = property.GetValue(zone) as string;
            if (!string.IsNullOrEmpty(value)) return value;
        }

        return zone.gameObject != null ? zone.gameObject.name : zone.name;
    }
}
