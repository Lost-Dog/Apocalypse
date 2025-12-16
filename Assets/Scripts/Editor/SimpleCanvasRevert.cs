using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SimpleCanvasRevert
{
    [MenuItem("Division Game/UI/REVERT Canvas to ScreenSpace Overlay")]
    public static void RevertCanvasNow()
    {
        GameObject canvasObj = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        
        if (canvasObj == null)
        {
            Debug.LogError("WorldSpace_Challenges not found!");
            EditorUtility.DisplayDialog("Error", "WorldSpace_Challenges canvas not found in scene!", "OK");
            return;
        }
        
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas component not found!");
            return;
        }
        
        Undo.RecordObject(canvas, "Revert Canvas");
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        
        RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Undo.RecordObject(rectTransform, "Reset Transform");
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
        }
        
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(canvasObj);
        
        Debug.Log("<color=green>✓ Canvas reverted to ScreenSpaceOverlay - Your HUD should be visible!</color>");
        
        EditorUtility.DisplayDialog(
            "Canvas Reverted!",
            "✓ Canvas is now ScreenSpaceOverlay\n" +
            "✓ Transform reset\n\n" +
            "Your HUD UI should be visible now!\n\n" +
            "Note: Challenge markers need prefab update:\n" +
            "Open ChallengeWorldMarker.prefab\n" +
            "Set worldSpaceMode = false",
            "OK");
        
        Selection.activeGameObject = canvasObj;
    }
}
