using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FixChallengeMarkerCanvas : EditorWindow
{
    [MenuItem("Division Game/UI/Fix Challenge Marker Canvas")]
    public static void FixCanvas()
    {
        GameObject canvasObj = GameObject.Find("UI/HUD/WorldSpace_Challenges");
        if (canvasObj == null)
        {
            EditorUtility.DisplayDialog("Error", "WorldSpace_Challenges canvas not found!", "OK");
            return;
        }
        
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas component not found!", "OK");
            return;
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Main Camera not found!", "OK");
            return;
        }
        
        Undo.RecordObject(canvas, "Fix Challenge Marker Canvas");
        
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        
        RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Undo.RecordObject(rectTransform, "Fix Canvas Transform");
            rectTransform.localScale = Vector3.one * 0.01f;
            rectTransform.sizeDelta = new Vector2(100, 120);
        }
        
        canvasObj.layer = LayerMask.NameToLayer("UI");
        
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(canvasObj);
        
        Debug.Log("<color=green>✓ Fixed WorldSpace_Challenges canvas to WorldSpace mode with Main Camera</color>");
        
        EditorUtility.DisplayDialog(
            "Canvas Fixed!",
            "✓ Canvas set to WorldSpace mode\n" +
            "✓ Main Camera assigned\n" +
            "✓ Scale adjusted to 0.01\n" +
            "✓ Layer set to UI\n\n" +
            "Challenge markers should now appear correctly in Play Mode!",
            "OK");
        
        Selection.activeGameObject = canvasObj;
        EditorGUIUtility.PingObject(canvasObj);
    }
}
