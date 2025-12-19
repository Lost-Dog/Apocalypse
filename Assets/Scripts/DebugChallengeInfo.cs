using UnityEngine;

public class DebugChallengeInfo : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            PrintChallengeManagerState();
        }
    }

    private void PrintChallengeManagerState()
    {
        if (ChallengeManager.Instance == null)
        {
            Debug.LogError("ChallengeManager.Instance is NULL!");
            return;
        }

        var manager = ChallengeManager.Instance;
        
        Debug.Log("=== CHALLENGE MANAGER DEBUG ===");
        Debug.Log($"autoSpawnChallenges: {manager.autoSpawnChallenges}");
        Debug.Log($"worldEventSpawnInterval: {manager.worldEventSpawnInterval}");
        Debug.Log($"maxActiveWorldEvents: {manager.maxActiveWorldEvents}");
        
        Debug.Log($"\n<color=cyan>World Event Challenges Pool: {manager.worldEventChallenges.Count}</color>");
        for (int i = 0; i < manager.worldEventChallenges.Count && i < 5; i++)
        {
            Debug.Log($"  [{i}] {manager.worldEventChallenges[i].challengeName}");
        }
        
        Debug.Log($"\n<color=yellow>Active Challenges: {manager.activeChallenges.Count}</color>");
        for (int i = 0; i < manager.activeChallenges.Count; i++)
        {
            var challenge = manager.activeChallenges[i];
            Debug.Log($"  [{i}] {challenge.challengeData.challengeName}");
            Debug.Log($"      State: {challenge.state}");
            Debug.Log($"      Position: {challenge.position}");
        }
        
        Debug.Log($"\n<color=green>Spawn Zones: {manager.spawnZones.Count}</color>");
        for (int i = 0; i < manager.spawnZones.Count && i < 5; i++)
        {
            if (manager.spawnZones[i] != null)
            {
                Debug.Log($"  [{i}] {manager.spawnZones[i].name}");
            }
        }
        
        if (ChallengeSpawner.Instance != null)
        {
            Debug.Log($"\n<color=magenta>ChallengeSpawner: ACTIVE</color>");
        }
        else
        {
            Debug.LogError("ChallengeSpawner.Instance is NULL!");
        }
        
        Debug.Log("=== END DEBUG ===\n");
    }
}
