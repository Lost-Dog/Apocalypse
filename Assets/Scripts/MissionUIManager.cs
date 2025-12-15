using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionUIManager : MonoBehaviour
{
    [Header("Mission Tracker References")]
    [SerializeField] private GameObject missionTrackerPanel;
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;
    [SerializeField] private TextMeshProUGUI objectiveProgressText;
    
    [Header("Mission Complete References")]
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TextMeshProUGUI completeMissionNameText;
    [SerializeField] private TextMeshProUGUI xpRewardText;
    [SerializeField] private TextMeshProUGUI currencyRewardText;
    [SerializeField] private Animator missionCompleteAnimator;
    
    [Header("Settings")]
    [SerializeField] private float missionCompleteDisplayDuration = 5f;
    
    private MissionManager missionManager;
    private float missionCompleteTimer = 0f;
    
    public void Initialize(MissionManager manager)
    {
        missionManager = manager;
        
        if (missionManager != null)
        {
            missionManager.onMissionStart.AddListener(OnMissionStarted);
            missionManager.onMissionComplete.AddListener(OnMissionCompleted);
            missionManager.onObjectiveUpdate.AddListener(OnObjectiveUpdated);
        }
        
        HideMissionTracker();
        HideMissionComplete();
    }
    
    private void OnDestroy()
    {
        if (missionManager != null)
        {
            missionManager.onMissionStart.RemoveListener(OnMissionStarted);
            missionManager.onMissionComplete.RemoveListener(OnMissionCompleted);
            missionManager.onObjectiveUpdate.RemoveListener(OnObjectiveUpdated);
        }
    }
    
    private void Update()
    {
        if (missionCompleteTimer > 0)
        {
            missionCompleteTimer -= Time.deltaTime;
            if (missionCompleteTimer <= 0)
            {
                HideMissionComplete();
            }
        }
    }
    
    private void OnMissionStarted(MissionData mission)
    {
        ShowMissionTracker();
        UpdateMissionDisplay(mission);
    }
    
    private void OnMissionCompleted(MissionData mission)
    {
        HideMissionTracker();
        ShowMissionComplete(mission);
    }
    
    private void OnObjectiveUpdated(MissionData mission)
    {
        UpdateMissionDisplay(mission);
    }
    
    private void UpdateMissionDisplay(MissionData mission)
    {
        if (mission == null) return;
        
        if (missionNameText != null)
        {
            missionNameText.text = mission.missionName;
        }
        
        MissionObjective currentObjective = mission.GetCurrentObjective();
        if (currentObjective != null)
        {
            if (objectiveDescriptionText != null)
            {
                objectiveDescriptionText.text = currentObjective.description;
            }
            
            if (objectiveProgressText != null)
            {
                objectiveProgressText.text = $"{currentObjective.GetCurrentCount()}/{currentObjective.GetTargetCount()}";
            }
        }
        else
        {
            if (objectiveDescriptionText != null)
            {
                objectiveDescriptionText.text = "All objectives complete";
            }
            
            if (objectiveProgressText != null)
            {
                objectiveProgressText.text = "";
            }
        }
    }
    
    private void ShowMissionTracker()
    {
        if (missionTrackerPanel != null)
        {
            missionTrackerPanel.SetActive(true);
        }
    }
    
    private void HideMissionTracker()
    {
        if (missionTrackerPanel != null)
        {
            missionTrackerPanel.SetActive(false);
        }
    }
    
    private void ShowMissionComplete(MissionData mission)
    {
        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
        }
        
        if (completeMissionNameText != null)
        {
            completeMissionNameText.text = mission.missionName;
        }
        
        if (xpRewardText != null)
        {
            xpRewardText.text = $"+{mission.xpReward} XP";
        }
        
        if (currencyRewardText != null)
        {
            currencyRewardText.text = $"+{mission.currencyReward}";
        }
        
        if (missionCompleteAnimator != null)
        {
            missionCompleteAnimator.SetTrigger("Show");
        }
        
        missionCompleteTimer = missionCompleteDisplayDuration;
    }
    
    private void HideMissionComplete()
    {
        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(false);
        }
    }
    
    public void ManualUpdateMissionUI()
    {
        if (missionManager != null && missionManager.activeMission != null)
        {
            UpdateMissionDisplay(missionManager.activeMission);
        }
    }
}
