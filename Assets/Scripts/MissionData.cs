using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Division Game/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName;
    [TextArea(3, 6)] public string description;
    public int levelRequirement = 1;
    public bool isMainStory;
    public bool isBossMission;
    
    [Header("Objectives")]
    public List<MissionObjective> objectives = new List<MissionObjective>();
    
    [Header("Rewards")]
    public int xpReward = 100;
    public int currencyReward = 50;
    
    private int currentObjectiveIndex = 0;
    private bool missionStarted = false;
    
    public void StartMission()
    {
        currentObjectiveIndex = 0;
        missionStarted = true;
        
        foreach (var objective in objectives)
        {
            objective.ResetObjective();
        }
        
        if (objectives.Count > 0)
        {
            objectives[0].ActivateObjective();
        }
    }
    
    public void UpdateProgress(float progress)
    {
        if (!missionStarted || currentObjectiveIndex >= objectives.Count) return;
        
        objectives[currentObjectiveIndex].UpdateProgress(progress);
        
        if (objectives[currentObjectiveIndex].IsComplete())
        {
            currentObjectiveIndex++;
            
            if (currentObjectiveIndex < objectives.Count)
            {
                objectives[currentObjectiveIndex].ActivateObjective();
            }
        }
    }
    
    public bool IsComplete()
    {
        return objectives.All(obj => obj.IsComplete());
    }
    
    public MissionObjective GetCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Count) return null;
        return objectives[currentObjectiveIndex];
    }
    
    public float GetOverallProgress()
    {
        if (objectives.Count == 0) return 0f;
        
        float totalProgress = objectives.Sum(obj => obj.GetProgress());
        return totalProgress / objectives.Count;
    }
}

[System.Serializable]
public class MissionObjective
{
    public enum ObjectiveType
    {
        KillEnemies,
        CollectItems,
        ReachLocation,
        DefendArea,
        SurviveTime,
        BossKill
    }
    
    public ObjectiveType type;
    public string description;
    public int targetCount = 1;
    
    private int currentCount = 0;
    private bool isActive = false;
    private bool isCompleted = false;
    
    public void ActivateObjective()
    {
        isActive = true;
        Debug.Log($"Objective activated: {description}");
    }
    
    public void UpdateProgress(float progress)
    {
        if (!isActive || isCompleted) return;
        
        currentCount = Mathf.Clamp((int)progress, 0, targetCount);
        
        if (currentCount >= targetCount)
        {
            CompleteObjective();
        }
    }
    
    public void IncrementProgress(int amount = 1)
    {
        UpdateProgress(currentCount + amount);
    }
    
    private void CompleteObjective()
    {
        isCompleted = true;
        isActive = false;
        Debug.Log($"Objective completed: {description}");
    }
    
    public void ResetObjective()
    {
        currentCount = 0;
        isActive = false;
        isCompleted = false;
    }
    
    public bool IsComplete()
    {
        return isCompleted;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    public float GetProgress()
    {
        if (targetCount == 0) return 0f;
        return (float)currentCount / targetCount;
    }
    
    public int GetCurrentCount()
    {
        return currentCount;
    }
    
    public int GetTargetCount()
    {
        return targetCount;
    }
}
