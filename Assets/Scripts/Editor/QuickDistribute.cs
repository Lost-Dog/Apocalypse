using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Quick distribute commands for selected objects.
/// </summary>
public static class QuickDistribute
{
    [MenuItem("Tools/Distribute Objects/Quick Distribute (30m, Y=0) %&D")]
    public static void QuickDistribute30m()
    {
        DistributeSelectedObjectsAroundParent(30f, true, 0f);
    }

    [MenuItem("Tools/Distribute Objects/Distribute 50m Radius")]
    public static void QuickDistribute50m()
    {
        DistributeSelectedObjectsAroundParent(50f, true, 0f);
    }

    [MenuItem("Tools/Distribute Objects/Distribute 100m Radius")]
    public static void QuickDistribute100m()
    {
        DistributeSelectedObjectsAroundParent(100f, true, 0f);
    }

    private static void DistributeSelectedObjectsAroundParent(float radius, bool keepYAxis, float yPosition)
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
                    "All selected objects must have the same parent.", "OK");
                return;
            }
        }

        // Record undo
        Undo.RecordObjects(selectedObjects.Select(obj => obj.transform).ToArray(), "Quick Distribute Objects");

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

        Debug.Log($"<color=green>✓ Quick distributed {distributedCount} objects around '{parentTransform.name}'</color>");
        Debug.Log($"  ├─ Radius: {radius}m");
        Debug.Log($"  └─ Y Position: {yPosition}");
    }
}