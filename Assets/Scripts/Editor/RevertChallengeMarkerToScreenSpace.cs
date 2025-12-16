using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class RevertChallengeMarkerToScreenSpace : EditorWindow
{
    [MenuItem("Division Game/UI/Revert to ScreenSpace Mode (Fix UI)")]
    public static void RevertToScreenSpace()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Revert to ScreenSpace",
            "This will:\n\n" +
            "1. Change WorldSpace_Challenges Canvas → ScreenSpaceOverlay\n" +
            "2. Update ChallengeWorldMarker prefab → ScreenSpace mode\n\n" +
            "Your normal HUD UI will be visible again!\n\nContinue?",
            "Yes, Revert",
            "Cancel");
        
        if (!confirm)
            return;
        
        bool success = true;
        
        GameObject canvasObj = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        if (canvasObj != null)
        {
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            if (canvas != null)
            {
                Undo.RecordObject(canvas, "Revert Canvas to ScreenSpace");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                
                RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Undo.RecordObject(rectTransform, "Reset Canvas Transform");
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                }
                
                EditorUtility.SetDirty(canvas);
                EditorUtility.SetDirty(canvasObj);
                
                Debug.Log("<color=green>✓ Reverted WorldSpace_Challenges to ScreenSpaceOverlay</color>");
            }
        }
        else
        {
            Debug.LogWarning("WorldSpace_Challenges canvas not found in scene");
        }
        
        string prefabPath = "Assets/Prefabs/ChallengeWorldMarker.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            ChallengeWorldMarker marker = prefab.GetComponent<ChallengeWorldMarker>();
            if (marker != null)
            {
                SerializedObject so = new SerializedObject(marker);
                SerializedProperty worldSpaceModeProp = so.FindProperty("worldSpaceMode");
                
                if (worldSpaceModeProp != null)
                {
                    worldSpaceModeProp.boolValue = false;
                    so.ApplyModifiedProperties();
                    
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                    
                    Debug.Log("<color=green>✓ Updated ChallengeWorldMarker prefab to ScreenSpace mode</color>");
                }
                else
                {
                    Debug.LogError("worldSpaceMode property not found in prefab");
                    success = false;
                }
            }
            else
            {
                Debug.LogError("ChallengeWorldMarker component not found in prefab");
                success = false;
            }
        }
        else
        {
            Debug.LogWarning("ChallengeWorldMarker prefab not found at: " + prefabPath);
            success = false;
        }
        
        if (success)
        {
            EditorUtility.DisplayDialog(
                "Reverted Successfully!",
                "✓ Canvas → ScreenSpaceOverlay\n" +
                "✓ Marker prefab → ScreenSpace mode\n\n" +
                "Your HUD should be visible again!\n" +
                "Challenge markers will now appear as 2D screen overlay.\n\n" +
                "Test in Play Mode to verify.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Partial Revert",
                "Canvas was reverted but prefab update failed.\n\n" +
                "Manually set ChallengeWorldMarker prefab:\n" +
                "worldSpaceMode = false",
                "OK");
        }
        
        if (canvasObj != null)
        {
            Selection.activeGameObject = canvasObj;
            EditorGUIUtility.PingObject(canvasObj);
        }
    }
}
