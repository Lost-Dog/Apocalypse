using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor tool to distribute selected GameObjects randomly around their parent root transform.
/// </summary>
public class DistributeAroundParent : EditorWindow
{
    private float radius = 30f;
    private bool keepYAxis = true;
    private float yPosition = 0f;

    [MenuItem("Tools/Distribute Objects/Distribute Selected Around Parent")]
    public static void ShowWindow()
    {
        GetWindow<DistributeAroundParent>("Distribute Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("Distribute Selected Objects", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        radius = EditorGUILayout.FloatField("Radius (meters)", radius);
        keepYAxis = EditorGUILayout.Toggle("Keep Y Axis Fixed", keepYAxis);

        if (keepYAxis)
        {
            yPosition = EditorGUILayout.FloatField("Y Position", yPosition);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Distribute Selected Objects", GUILayout.Height(30)))
        {
            DistributeSelectedObjects();
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Select multiple GameObjects with the same parent, then click 'Distribute Selected Objects' to randomly distribute them around the parent's position.", 
            MessageType.Info);
    }

    private void DistributeSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects to distribute.", "OK");
            return;
        }

        // Get the parent of the first selected object
        Transform parentTransform = selectedObjects[0].transform.parent;

        if (parentTransform == null)
        {
            EditorUtility.DisplayDialog("No Parent", "Selected objects must have a parent transform to distribute around.", "OK");
            return;
        }

        // Verify all selected objects have the same parent
        foreach (GameObject obj in selectedObjects)
        {
            if (obj.transform.parent != parentTransform)
            {
                EditorUtility.DisplayDialog("Different Parents", 
                    "All selected objects must have the same parent.\n\n" +
                    $"Found different parent: {obj.name}", "OK");
                return;
            }
        }

        // Record undo
        Undo.RecordObjects(selectedObjects.Select(obj => obj.transform).ToArray(), "Distribute Objects Around Parent");

        Vector3 parentPosition = parentTransform.position;
        int distributedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            // Generate random point within circle
            Vector2 randomCircle = Random.insideUnitCircle * radius;

            // Create new position
            Vector3 newPosition = new Vector3(
                parentPosition.x + randomCircle.x,
                keepYAxis ? yPosition : obj.transform.position.y,
                parentPosition.z + randomCircle.y
            );

            obj.transform.position = newPosition;
            distributedCount++;

            EditorUtility.SetDirty(obj);
        }

        Debug.Log($"<color=green>✓ Distributed {distributedCount} objects around parent '{parentTransform.name}'</color>");
        Debug.Log($"  ├─ Parent Position: {parentPosition}");
        Debug.Log($"  ├─ Radius: {radius}m");
        Debug.Log($"  └─ Y Position: {(keepYAxis ? yPosition.ToString("F2") : "Preserved")}");

        EditorUtility.DisplayDialog("Distribution Complete", 
            $"Successfully distributed {distributedCount} object(s) around parent '{parentTransform.name}' within {radius}m radius.", "OK");
    }
}