using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Database")]
    public List<MissionData> allMissions = new List<MissionData>();
    
    [Header("Active Missions")]
    public MissionData activeMission;
    public List<MissionData> completedMissions = new List<MissionData>();
    public List<MissionData> availableMissions = new List<MissionData>();
    
    [Header("Mission Events")]
    public UnityEvent<MissionData> onMissionStart;
    public UnityEvent<MissionData> onMissionComplete;
    public UnityEvent<MissionData> onMissionFail;
    public UnityEvent<MissionData> onObjectiveUpdate;
    
    private const string MISSION_RESOURCE_PATH = "Missions";
    
    public void Initialize()
    {
        LoadMissions();
        RefreshAvailableMissions();
        ConsumePendingMissionRequest();
    }

    /// <summary>
    /// Checks <see cref="MissionRequest"/> for a mission queued from the main menu
    /// and starts it automatically. Clears the request whether or not a match is found.
    /// </summary>
    private void ConsumePendingMissionRequest()
    {
        string pending = MissionRequest.PendingMissionName;
        MissionRequest.Clear();

        if (string.IsNullOrWhiteSpace(pending))
            return;

        MissionData target = allMissions.Find(m => m.missionName == pending);

        if (target == null)
        {
            Debug.LogWarning($"[MissionManager] Pending mission '{pending}' not found in loaded missions. Has the MissionData asset been created in Resources/Missions?", this);
            return;
        }

        Debug.Log($"[MissionManager] Auto-starting queued mission: '{pending}'", this);
        StartMission(target);
    }
    
    private void LoadMissions()
    {
        MissionData[] loadedMissions = Resources.LoadAll<MissionData>(MISSION_RESOURCE_PATH);
        allMissions = new List<MissionData>(loadedMissions);
        
        Debug.Log($"Loaded {allMissions.Count} missions from Resources/{MISSION_RESOURCE_PATH}");
    }
    
    public void RefreshAvailableMissions()
    {
        if (GameManager.Instance == null) return;
        
        int playerLevel = GameManager.Instance.currentPlayerLevel;
        availableMissions = GetAvailableMissions(playerLevel);
    }
    
    public List<MissionData> GetAvailableMissions(int playerLevel)
    {
        return allMissions.Where(m => 
            m.levelRequirement <= playerLevel && 
            !completedMissions.Contains(m) &&
            m != activeMission
        ).ToList();
    }
    
    public void StartMission(MissionData mission)
    {
        if (activeMission != null)
        {
            Debug.LogWarning($"Already have active mission: {activeMission.missionName}");
            return;
        }
        
        if (mission == null)
        {
            Debug.LogError("Cannot start null mission!");
            return;
        }
        
        activeMission = mission;
        activeMission.StartMission();
        onMissionStart?.Invoke(mission);
        
        Debug.Log($"Started mission: {mission.missionName}");
    }
    
    public void UpdateMissionProgress(float progress)
    {
        if (activeMission == null) return;
        
        activeMission.UpdateProgress(progress);
        onObjectiveUpdate?.Invoke(activeMission);
        
        if (activeMission.IsComplete())
        {
            CompleteMission();
        }
    }
    
    public void CompleteMission()
    {
        if (activeMission == null) return;
        
        completedMissions.Add(activeMission);
        GrantRewards(activeMission);
        
        onMissionComplete?.Invoke(activeMission);
        Debug.Log($"Completed mission: {activeMission.missionName}");
        
        activeMission = null;
        RefreshAvailableMissions();
    }
    
    public void FailMission()
    {
        if (activeMission == null) return;
        
        onMissionFail?.Invoke(activeMission);
        Debug.Log($"Failed mission: {activeMission.missionName}");
        
        activeMission = null;
    }
    
    private void GrantRewards(MissionData mission)
    {
        if (GameManager.Instance != null && GameManager.Instance.progressionManager != null)
        {
            GameManager.Instance.progressionManager.AddExperience(mission.xpReward);
        }
    }
    
    public bool IsMissionComplete(string missionName)
    {
        return completedMissions.Any(m => m.missionName == missionName);
    }
    
    public int GetCompletedMissionCount()
    {
        return completedMissions.Count;
    }
}
