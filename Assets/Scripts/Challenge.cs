using UnityEngine;
using System;

[System.Serializable]
public class Challenge
{
    public ChallengeData data;
    public int currentProgress;
    public bool isActive;
    public bool isCompleted;
    public DateTime startTime;
    public DateTime expirationTime;

    public float Progress
    {
        get
        {
            if (data == null) return 0f;
            int totalCount = data.GetEnemyCount();
            if (totalCount == 0) return 0f;
            return Mathf.Clamp01((float)currentProgress / totalCount);
        }
    }

    public bool IsExpired
    {
        get
        {
            if (data.frequency == ChallengeData.ChallengeFrequency.WorldEvent)
                return false;

            return DateTime.Now > expirationTime;
        }
    }

    public Challenge(ChallengeData challengeData)
    {
        data = challengeData;
        currentProgress = 0;
        isActive = true;
        isCompleted = false;
        startTime = DateTime.Now;
        
        if (challengeData.frequency == ChallengeData.ChallengeFrequency.Daily)
            expirationTime = startTime.AddDays(1);
        else if (challengeData.frequency == ChallengeData.ChallengeFrequency.Weekly)
            expirationTime = startTime.AddDays(7);
        else
            expirationTime = DateTime.MaxValue;
    }

    public void AddProgress(int amount = 1)
    {
        if (isCompleted || !isActive) return;

        currentProgress += amount;

        if (currentProgress >= data.GetEnemyCount())
        {
            Complete();
        }
    }

    public void Complete()
    {
        if (isCompleted) return;

        isCompleted = true;
        isActive = false;
        currentProgress = data.GetEnemyCount();
    }

    public void Reset()
    {
        currentProgress = 0;
        isCompleted = false;
        isActive = true;
        startTime = DateTime.Now;

        if (data.frequency == ChallengeData.ChallengeFrequency.Daily)
            expirationTime = startTime.AddDays(1);
        else if (data.frequency == ChallengeData.ChallengeFrequency.Weekly)
            expirationTime = startTime.AddDays(7);
    }
}
