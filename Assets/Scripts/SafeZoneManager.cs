using UnityEngine;
using System.Collections.Generic;

public class SafeZoneManager : MonoBehaviour
{
    public static SafeZoneManager Instance { get; private set; }
    
    [Header("Safe Zone Tracking")]
    public List<SafeZone> allSafeZones = new List<SafeZone>();
    public bool autoFindSafeZones = true;
    
    [Header("Statistics")]
    public int totalSafeZonesEntered = 0;
    public float totalTimeInSafeZones = 0f;
    public float totalHealthRestored = 0f;
    
    [Header("Current Status")]
    public SafeZone currentSafeZone = null;
    public bool playerInSafeZone = false;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private float sessionStartTime;
    
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
        SafeZone[] foundZones = FindObjectsByType<SafeZone>(FindObjectsSortMode.None);
        allSafeZones.Clear();
        allSafeZones.AddRange(foundZones);
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>SafeZoneManager: Found {allSafeZones.Count} safe zones</color>");
        }
    }
    
    private void RegisterEventListeners()
    {
        foreach (SafeZone zone in allSafeZones)
        {
            zone.onPlayerEnter.AddListener(() => OnPlayerEnterAnyZone(zone));
            zone.onPlayerExit.AddListener(() => OnPlayerExitAnyZone(zone));
        }
    }
    
    private void OnPlayerEnterAnyZone(SafeZone zone)
    {
        currentSafeZone = zone;
        playerInSafeZone = true;
        totalSafeZonesEntered++;
        sessionStartTime = Time.time;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Player entered safe zone: {zone.safeZoneName}</color>");
        }
    }
    
    private void OnPlayerExitAnyZone(SafeZone zone)
    {
        if (currentSafeZone == zone)
        {
            float sessionDuration = Time.time - sessionStartTime;
            totalTimeInSafeZones += sessionDuration;
            
            currentSafeZone = null;
            playerInSafeZone = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=yellow>Player left safe zone: {zone.safeZoneName} (Duration: {sessionDuration:F1}s)</color>");
            }
        }
    }
    
    public SafeZone GetNearestSafeZone(Vector3 position)
    {
        if (allSafeZones.Count == 0) return null;
        
        SafeZone nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (SafeZone zone in allSafeZones)
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
    
    public List<SafeZone> GetSafeZonesInRadius(Vector3 position, float radius)
    {
        List<SafeZone> zonesInRange = new List<SafeZone>();
        
        foreach (SafeZone zone in allSafeZones)
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
    
    public void RegisterSafeZone(SafeZone zone)
    {
        if (!allSafeZones.Contains(zone))
        {
            allSafeZones.Add(zone);
            
            zone.onPlayerEnter.AddListener(() => OnPlayerEnterAnyZone(zone));
            zone.onPlayerExit.AddListener(() => OnPlayerExitAnyZone(zone));
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>Registered safe zone: {zone.safeZoneName}</color>");
            }
        }
    }
    
    public void UnregisterSafeZone(SafeZone zone)
    {
        if (allSafeZones.Contains(zone))
        {
            allSafeZones.Remove(zone);
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=orange>Unregistered safe zone: {zone.safeZoneName}</color>");
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
            Debug.Log($"Current Zone: {currentSafeZone.safeZoneName}");
        }
    }
    
    private void OnGUI()
    {
        if (showDebugInfo && playerInSafeZone && currentSafeZone != null)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 300, 20), $"Safe Zone: {currentSafeZone.safeZoneName}");
            GUI.color = Color.white;
        }
    }
}
