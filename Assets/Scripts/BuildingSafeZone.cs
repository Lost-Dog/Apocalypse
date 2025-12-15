using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SafeZone))]
public class BuildingSafeZone : MonoBehaviour
{
    [Header("Building Safe Zone Setup")]
    [Tooltip("The trigger box collider for the safe zone")]
    public BoxCollider safeZoneTrigger;
    
    [Tooltip("The mesh collider for the building structure")]
    public MeshCollider buildingCollider;
    
    [Tooltip("Auto-configure on start")]
    public bool autoSetup = true;
    
    [Tooltip("Make the safe zone trigger larger than building")]
    public bool expandSafeZone = false;
    
    [Tooltip("How much to expand safe zone on each axis")]
    public Vector3 safeZoneExpansion = new Vector3(2f, 2f, 2f);
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Color triggerColor = new Color(0f, 1f, 0.5f, 0.3f);
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupBuildingSafeZone();
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        FindColliders();
    }
    
    [ContextMenu("Setup Building Safe Zone")]
    public void SetupBuildingSafeZone()
    {
        FindColliders();
        ConfigureColliders();
        
        if (expandSafeZone && safeZoneTrigger != null)
        {
            ExpandSafeZone();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Building Safe Zone configured: {gameObject.name}</color>");
            if (safeZoneTrigger != null)
            {
                Debug.Log($"  Safe Zone Trigger: Center={safeZoneTrigger.center}, Size={safeZoneTrigger.size}");
            }
            if (buildingCollider != null)
            {
                Debug.Log($"  Building Collider: Type={buildingCollider.GetType().Name}, IsTrigger={buildingCollider.isTrigger}");
            }
        }
    }
    
    private void FindColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        
        foreach (Collider col in colliders)
        {
            if (col is BoxCollider box)
            {
                safeZoneTrigger = box;
            }
            else if (col is MeshCollider mesh)
            {
                buildingCollider = mesh;
            }
        }
        
        if (safeZoneTrigger == null && showDebugInfo)
        {
            Debug.LogWarning($"No BoxCollider found on {gameObject.name}. Adding one...");
            safeZoneTrigger = gameObject.AddComponent<BoxCollider>();
        }
    }
    
    private void ConfigureColliders()
    {
        if (safeZoneTrigger != null)
        {
            safeZoneTrigger.isTrigger = true;
            
            if (showDebugInfo && !safeZoneTrigger.isTrigger)
            {
                Debug.Log($"<color=yellow>Set BoxCollider to trigger on {gameObject.name}</color>");
            }
        }
        
        if (buildingCollider != null)
        {
            buildingCollider.isTrigger = false;
            buildingCollider.convex = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>Building MeshCollider kept as non-trigger (correct for walls)</color>");
            }
        }
    }
    
    private void ExpandSafeZone()
    {
        if (safeZoneTrigger == null) return;
        
        safeZoneTrigger.size += safeZoneExpansion;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>Expanded safe zone by {safeZoneExpansion}</color>");
        }
    }
    
    public void MakeSafeZoneInterior()
    {
        if (safeZoneTrigger == null || buildingCollider == null) return;
        
        Bounds meshBounds = buildingCollider.bounds;
        
        float padding = 1f;
        safeZoneTrigger.center = buildingCollider.bounds.center - transform.position;
        safeZoneTrigger.size = new Vector3(
            meshBounds.size.x - padding * 2f,
            meshBounds.size.y - padding * 2f,
            meshBounds.size.z - padding * 2f
        );
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>Safe zone set to building interior (1m padding)</color>");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (safeZoneTrigger == null) return;
        
        Gizmos.color = triggerColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(safeZoneTrigger.center, safeZoneTrigger.size);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (safeZoneTrigger == null) return;
        
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(safeZoneTrigger.center, safeZoneTrigger.size);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(safeZoneTrigger.center, safeZoneTrigger.size);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(BuildingSafeZone))]
public class BuildingSafeZoneEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        BuildingSafeZone buildingSafeZone = (BuildingSafeZone)target;
        
        UnityEditor.EditorGUILayout.Space(10);
        UnityEditor.EditorGUILayout.LabelField("Setup Tools", UnityEditor.EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup Building Safe Zone", GUILayout.Height(30)))
        {
            buildingSafeZone.SetupBuildingSafeZone();
        }
        
        if (GUILayout.Button("Fit Safe Zone to Building Interior", GUILayout.Height(30)))
        {
            buildingSafeZone.MakeSafeZoneInterior();
        }
        
        UnityEditor.EditorGUILayout.Space(5);
        UnityEditor.EditorGUILayout.HelpBox(
            "This script manages buildings with both:\n" +
            "• MeshCollider (solid walls)\n" +
            "• BoxCollider (safe zone trigger)\n\n" +
            "The safe zone trigger lets players walk through while the mesh collider blocks them from walls.",
            UnityEditor.MessageType.Info);
    }
}
#endif
