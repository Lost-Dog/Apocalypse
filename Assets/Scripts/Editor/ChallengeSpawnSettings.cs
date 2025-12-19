using UnityEngine;
using UnityEditor;

public class ChallengeSpawnSettings : EditorWindow
{
    [MenuItem("Division Game/Challenge System/Adjust Spawn Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChallengeSpawnSettings>("Spawn Settings");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Challenge Spawn Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "If challenges only spawn 4/10 or 5/15 enemies, it's because spawn positions can't be found.\n\n" +
            "Fix this by adjusting these settings:",
            MessageType.Info);

        GUILayout.Space(10);

        ChallengeSpawner spawner = FindFirstObjectByType<ChallengeSpawner>();
        
        if (spawner == null)
        {
            EditorGUILayout.HelpBox("ChallengeSpawner not found in scene! Make sure GameSystems/ChallengeSpawner exists.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Current Spawner Settings:", EditorStyles.boldLabel);
        
        var maxAttemptsField = typeof(ChallengeSpawner).GetField("maxNavMeshAttempts", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sampleDistanceField = typeof(ChallengeSpawner).GetField("navMeshSampleDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var minDistanceField = typeof(ChallengeSpawner).GetField("minimumSpawnDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (maxAttemptsField != null)
        {
            int currentAttempts = (int)maxAttemptsField.GetValue(spawner);
            EditorGUILayout.LabelField($"Max NavMesh Attempts: {currentAttempts}");
        }

        if (sampleDistanceField != null)
        {
            float currentSample = (float)sampleDistanceField.GetValue(spawner);
            EditorGUILayout.LabelField($"NavMesh Sample Distance: {currentSample}m");
        }

        if (minDistanceField != null)
        {
            float currentMinDist = (float)minDistanceField.GetValue(spawner);
            EditorGUILayout.LabelField($"Minimum Spawn Distance: {currentMinDist}m");
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Recommended Settings for Better Spawning:\n\n" +
            "• Max Attempts: 50 (was 20)\n" +
            "• Sample Distance: 10m (was 5m)\n" +
            "• Minimum Distance: 2m (was 3m)",
            MessageType.None);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Recommended Settings", GUILayout.Height(40)))
        {
            if (maxAttemptsField != null)
                maxAttemptsField.SetValue(spawner, 50);
            if (sampleDistanceField != null)
                sampleDistanceField.SetValue(spawner, 10f);
            if (minDistanceField != null)
                minDistanceField.SetValue(spawner, 2f);

            EditorUtility.SetDirty(spawner);
            
            Debug.Log("✅ Applied recommended spawn settings!");
            Debug.Log("   - Max Attempts: 50");
            Debug.Log("   - Sample Distance: 10m");
            Debug.Log("   - Minimum Distance: 2m");
            
            EditorUtility.DisplayDialog("Success", 
                "Spawn settings updated!\n\n" +
                "Max Attempts: 50\n" +
                "Sample Distance: 10m\n" +
                "Minimum Distance: 2m\n\n" +
                "This should fix the 4/10 spawn issue.\n\n" +
                "Save the scene to keep these changes.", "OK");
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Also check the Challenge Data:\n\n" +
            "1. Select your challenge asset in Project\n" +
            "2. Find 'Spawn Items' section\n" +
            "3. For 10 enemies, use Spawn Radius: 20-30m\n" +
            "4. For 15 enemies, use Spawn Radius: 30-40m",
            MessageType.Warning);
    }
}
