using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixStatsWarningsReferences
{
    [MenuItem("Tools/Fix Stats Warnings References")]
    public static void Fix()
    {
        GameObject statsWarnings = GameObject.Find("Stats_Warnings");
        
        if (statsWarnings == null)
        {
            Debug.LogError("Stats_Warnings GameObject not found in the scene! Make sure the scene is loaded.");
            return;
        }
        
        PlayerStatusIndicators indicators = statsWarnings.GetComponent<PlayerStatusIndicators>();
        
        if (indicators == null)
        {
            Debug.LogError("PlayerStatusIndicators component not found on Stats_Warnings!");
            return;
        }
        
        bool madeChanges = false;
        SerializedObject serializedObject = new SerializedObject(indicators);
        
        if (indicators.infectionDisplay == null)
        {
            PlayerInfectionDisplay infectionDisplay = Object.FindFirstObjectByType<PlayerInfectionDisplay>();
            
            if (infectionDisplay != null)
            {
                SerializedProperty infectionProp = serializedObject.FindProperty("infectionDisplay");
                infectionProp.objectReferenceValue = infectionDisplay;
                Debug.Log($"Fixed infectionDisplay reference: {infectionDisplay.gameObject.name}");
                madeChanges = true;
            }
            else
            {
                Debug.LogWarning("PlayerInfectionDisplay component not found in scene!");
            }
        }
        
        SerializedProperty startDisabledProp = serializedObject.FindProperty("startDisabled");
        SerializedProperty autoHideProp = serializedObject.FindProperty("autoHideWhenNoWarnings");
        SerializedProperty tempWarningProp = serializedObject.FindProperty("temperatureWarningThreshold");
        SerializedProperty tempCriticalProp = serializedObject.FindProperty("temperatureCriticalThreshold");
        
        if (startDisabledProp.boolValue != false)
        {
            startDisabledProp.boolValue = false;
            Debug.Log("Set startDisabled to false - panel will stay visible");
            madeChanges = true;
        }
        
        if (autoHideProp.boolValue != false)
        {
            autoHideProp.boolValue = false;
            Debug.Log("Set autoHideWhenNoWarnings to false - panel will stay visible");
            madeChanges = true;
        }
        
        if (Mathf.Approximately(tempWarningProp.floatValue, 0.4f) || tempWarningProp.floatValue < 1f)
        {
            tempWarningProp.floatValue = 15f;
            Debug.Log("Updated temperature warning threshold to 15°C");
            madeChanges = true;
        }
        
        if (Mathf.Approximately(tempCriticalProp.floatValue, 0.2f) || tempCriticalProp.floatValue < 1f)
        {
            tempCriticalProp.floatValue = 5f;
            Debug.Log("Updated temperature critical threshold to 5°C");
            madeChanges = true;
        }
        
        if (madeChanges)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(statsWarnings);
            EditorSceneManager.MarkSceneDirty(statsWarnings.scene);
            Debug.Log("<color=green>✓ Stats_Warnings panel fixed! Temperature now uses Celsius scale (36.9°C normal, 15°C warning, 5°C critical)</color>");
        }
        else
        {
            Debug.Log("All references and settings are already correct.");
        }
    }
}
