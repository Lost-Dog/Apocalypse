using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SafeZonePhysicsFixer : MonoBehaviour
{
    [Header("Physics Fix Settings")]
    [Tooltip("Automatically fix all colliders on Start")]
    public bool autoFixOnStart = true;
    
    [Tooltip("Fix child colliders too")]
    public bool fixChildColliders = true;
    
    [Tooltip("Remove MeshCollider from visual meshes")]
    public bool removeMeshColliders = true;
    
    [Tooltip("Set layer to Trigger layer")]
    public bool setToTriggerLayer = false;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            FixPhysics();
        }
    }
    
    public void FixPhysics()
    {
        int fixedCount = 0;
        
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
        {
            if (!mainCollider.isTrigger)
            {
                if (!TrySetTrigger(mainCollider, "main collider"))
                {
                    fixedCount++;
                }
                else
                {
                    fixedCount++;
                }
            }
        }
        
        if (fixChildColliders)
        {
            Collider[] childColliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in childColliders)
            {
                if (col.gameObject == gameObject) continue;
                
                if (removeMeshColliders && col is MeshCollider)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"<color=yellow>Removing MeshCollider from '{col.gameObject.name}'</color>");
                    }
                    
                    if (Application.isPlaying)
                    {
                        Destroy(col);
                    }
                    else
                    {
                        DestroyImmediate(col);
                    }
                    fixedCount++;
                }
                else if (!col.isTrigger)
                {
                    if (TrySetTrigger(col, col.gameObject.name))
                    {
                        fixedCount++;
                    }
                }
            }
        }
        
        if (setToTriggerLayer && LayerMask.NameToLayer("Trigger") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Trigger");
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>Set '{gameObject.name}' to Trigger layer</color>");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>SafeZone physics fix complete: {fixedCount} colliders fixed</color>");
        }
    }
    
    private bool TrySetTrigger(Collider col, string objectName)
    {
        MeshCollider meshCol = col as MeshCollider;
        
        if (meshCol != null && !meshCol.convex)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"Cannot set concave MeshCollider on '{objectName}' as trigger. Skipping.");
            }
            return false;
        }
        
        col.isTrigger = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Fixed collider on '{objectName}' - set to trigger</color>");
        }
        
        return true;
    }
    
    public void RemoveVisualColliders()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == transform) continue;
            
            if (child.name.ToLower().Contains("visual") || 
                child.name.ToLower().Contains("mesh") ||
                child.name.ToLower().Contains("model"))
            {
                Collider col = child.GetComponent<Collider>();
                if (col != null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"<color=yellow>Removing collider from visual object '{child.name}'</color>");
                    }
                    
                    if (Application.isPlaying)
                    {
                        Destroy(col);
                    }
                    else
                    {
                        DestroyImmediate(col);
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SafeZonePhysicsFixer))]
public class SafeZonePhysicsFixerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SafeZonePhysicsFixer fixer = (SafeZonePhysicsFixer)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Fix Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Fix Physics Now", GUILayout.Height(30)))
        {
            fixer.FixPhysics();
        }
        
        if (GUILayout.Button("Remove Visual Colliders", GUILayout.Height(30)))
        {
            fixer.RemoveVisualColliders();
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Use 'Fix Physics Now' to ensure all colliders are set to trigger and player can walk through.", MessageType.Info);
    }
}
#endif
