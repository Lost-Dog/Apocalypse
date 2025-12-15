using UnityEngine;
using UnityEngine.Events;

public class MissionOfferManager : MonoBehaviour
{
    public static MissionOfferManager Instance { get; private set; }
    
    [Header("Mission Offer Settings")]
    [Tooltip("Base location where missions must be accepted")]
    public Transform baseLocation;
    
    [Tooltip("Minimum distance from base to show 'Return to Base' message")]
    public float baseDetectionRadius = 50f;
    
    [Header("Current Offer")]
    [Tooltip("The currently offered mission (waiting for player to return to base)")]
    public MissionData offeredMission;
    
    [Tooltip("Is there a mission waiting to be accepted at base?")]
    public bool hasPendingOffer = false;
    
    [Header("Mission Tracking")]
    [Tooltip("Next mission index to offer (starts at 1 for Mission01)")]
    public int nextMissionIndex = 1;
    
    [Header("Events")]
    public UnityEvent<MissionData> onMissionOffered;
    public UnityEvent<MissionData> onMissionAccepted;
    public UnityEvent onPlayerAtBase;
    
    private GameObject playerObject;
    private bool notificationShown = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.onChallengeCompleted.AddListener(OnChallengeCompleted);
        }
        
        FindPlayer();
    }
    
    private void Update()
    {
        if (hasPendingOffer && playerObject != null && baseLocation != null)
        {
            float distanceToBase = Vector3.Distance(playerObject.transform.position, baseLocation.position);
            
            if (distanceToBase <= baseDetectionRadius)
            {
                onPlayerAtBase?.Invoke();
            }
        }
    }
    
    private void FindPlayer()
    {
        if (playerObject == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {
                playerObject = players[0];
            }
        }
    }
    
    public void OnChallengeCompleted(ActiveChallenge challenge)
    {
        if (hasPendingOffer)
        {
            return;
        }
        
        OfferNextMission();
    }
    
    public void OfferNextMission()
    {
        if (GameManager.Instance == null || GameManager.Instance.missionManager == null)
        {
            Debug.LogWarning("MissionOfferManager: GameManager or MissionManager not found!");
            return;
        }
        
        MissionData nextMission = FindNextMissionToOffer();
        
        if (nextMission != null)
        {
            OfferMission(nextMission);
        }
        else
        {
            Debug.Log("MissionOfferManager: No available missions to offer");
        }
    }
    
    private MissionData FindNextMissionToOffer()
    {
        if (GameManager.Instance == null || GameManager.Instance.missionManager == null)
        {
            return null;
        }
        
        MissionManager missionManager = GameManager.Instance.missionManager;
        int playerLevel = GameManager.Instance.currentPlayerLevel;
        
        string targetMissionName = $"Mission{nextMissionIndex:D2}";
        
        foreach (MissionData mission in missionManager.allMissions)
        {
            if (mission.missionName.Contains(targetMissionName) || 
                mission.missionName == targetMissionName)
            {
                if (mission.levelRequirement <= playerLevel && 
                    !missionManager.completedMissions.Contains(mission))
                {
                    return mission;
                }
                else if (mission.levelRequirement > playerLevel)
                {
                    Debug.Log($"Mission {targetMissionName} requires level {mission.levelRequirement}, player is level {playerLevel}");
                    return null;
                }
            }
        }
        
        Debug.LogWarning($"MissionOfferManager: Could not find mission with name containing '{targetMissionName}'");
        return null;
    }
    
    public void OfferMission(MissionData mission)
    {
        if (mission == null)
        {
            Debug.LogError("MissionOfferManager: Cannot offer null mission!");
            return;
        }
        
        offeredMission = mission;
        hasPendingOffer = true;
        notificationShown = false;
        
        onMissionOffered?.Invoke(mission);
        
        ShowMissionOfferNotification(mission);
        
        Debug.Log($"Mission offered: {mission.missionName} - Return to base to accept");
    }
    
    private void ShowMissionOfferNotification(MissionData mission)
    {
        if (notificationShown) return;
        
        string message = $"<b>New Mission Available</b>\n{mission.missionName}\n\nReturn to Base to Accept";
        
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMissionNotification(message);
        }
        else
        {
            Debug.Log(message);
        }
        
        notificationShown = true;
    }
    
    public bool TryAcceptMission()
    {
        if (!hasPendingOffer || offeredMission == null)
        {
            Debug.Log("MissionOfferManager: No mission to accept");
            return false;
        }
        
        if (playerObject == null)
        {
            FindPlayer();
        }
        
        if (playerObject != null && baseLocation != null)
        {
            float distanceToBase = Vector3.Distance(playerObject.transform.position, baseLocation.position);
            
            if (distanceToBase > baseDetectionRadius)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowWarningNotification(
                        "You must be at the base to accept missions!"
                    );
                }
                return false;
            }
        }
        
        AcceptOfferedMission();
        return true;
    }
    
    private void AcceptOfferedMission()
    {
        if (GameManager.Instance == null || GameManager.Instance.missionManager == null)
        {
            Debug.LogError("MissionOfferManager: Cannot accept mission - MissionManager not found!");
            return;
        }
        
        MissionData acceptedMission = offeredMission;
        
        GameManager.Instance.missionManager.StartMission(offeredMission);
        
        onMissionAccepted?.Invoke(acceptedMission);
        
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowMissionNotification(
                $"<b>Mission Accepted</b>\n{acceptedMission.missionName}"
            );
        }
        
        nextMissionIndex++;
        hasPendingOffer = false;
        offeredMission = null;
        notificationShown = false;
        
        Debug.Log($"Mission accepted: {acceptedMission.missionName}. Next mission index: {nextMissionIndex}");
    }
    
    public void SetNextMissionIndex(int index)
    {
        nextMissionIndex = Mathf.Max(1, index);
        Debug.Log($"Next mission index set to: {nextMissionIndex}");
    }
    
    public bool IsPlayerAtBase()
    {
        if (playerObject == null || baseLocation == null)
        {
            return false;
        }
        
        return Vector3.Distance(playerObject.transform.position, baseLocation.position) <= baseDetectionRadius;
    }
    
    public string GetOfferedMissionName()
    {
        return offeredMission != null ? offeredMission.missionName : "None";
    }
    
    public float GetDistanceToBase()
    {
        if (playerObject == null || baseLocation == null)
        {
            return float.MaxValue;
        }
        
        return Vector3.Distance(playerObject.transform.position, baseLocation.position);
    }
}
