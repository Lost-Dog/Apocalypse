using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshSwapTool : EditorWindow
{
    private GameObject targetCharacter;
    private GameObject sourceCharacter;
    private Transform targetRootBone;
    private Transform sourceRootBone;
    private bool autoDetectRootBones = true;
    private bool preserveMaterials = false;
    private bool deleteOldMeshes = true;
    private Vector2 scrollPosition;
    
    private List<SkinnedMeshRenderer> sourceMeshes = new List<SkinnedMeshRenderer>();
    private Dictionary<string, string> boneMapping = new Dictionary<string, string>();
    
    [MenuItem("Tools/Character/Mesh Swap Tool")]
    public static void ShowWindow()
    {
        MeshSwapTool window = GetWindow<MeshSwapTool>("Mesh Swap Tool");
        window.minSize = new Vector2(450, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Character Mesh Swap Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Swap meshes from one character to another while preserving the target armature.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("1. Select Characters", EditorStyles.boldLabel);
        
        GameObject newTarget = (GameObject)EditorGUILayout.ObjectField("Target Character (Keep Armature)", targetCharacter, typeof(GameObject), true);
        if (newTarget != targetCharacter)
        {
            targetCharacter = newTarget;
            if (autoDetectRootBones) DetectRootBones();
        }
        
        GameObject newSource = (GameObject)EditorGUILayout.ObjectField("Source Character (Meshes From)", sourceCharacter, typeof(GameObject), true);
        if (newSource != sourceCharacter)
        {
            sourceCharacter = newSource;
            if (autoDetectRootBones) DetectRootBones();
            ScanSourceMeshes();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("2. Configure Root Bones", EditorStyles.boldLabel);
        
        autoDetectRootBones = EditorGUILayout.Toggle("Auto-Detect Root Bones", autoDetectRootBones);
        
        GUI.enabled = !autoDetectRootBones;
        targetRootBone = (Transform)EditorGUILayout.ObjectField("Target Root Bone", targetRootBone, typeof(Transform), true);
        sourceRootBone = (Transform)EditorGUILayout.ObjectField("Source Root Bone", sourceRootBone, typeof(Transform), true);
        GUI.enabled = true;
        
        if (GUILayout.Button("Detect Root Bones"))
        {
            DetectRootBones();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("3. Options", EditorStyles.boldLabel);
        
        preserveMaterials = EditorGUILayout.Toggle("Preserve Target Materials", preserveMaterials);
        deleteOldMeshes = EditorGUILayout.Toggle("Delete Old Target Meshes", deleteOldMeshes);
        
        EditorGUILayout.Space(10);
        
        if (sourceMeshes.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {sourceMeshes.Count} Mesh Parts:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            foreach (var mesh in sourceMeshes)
            {
                EditorGUILayout.LabelField($"  • {mesh.name}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space(10);
        
        GUI.enabled = targetCharacter != null && sourceCharacter != null && targetRootBone != null && sourceRootBone != null;
        
        if (GUILayout.Button("SWAP MESHES", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirm Mesh Swap",
                $"Swap {sourceMeshes.Count} meshes from '{sourceCharacter.name}' to '{targetCharacter.name}'?\n\n" +
                $"Target armature will be preserved.\n" +
                (deleteOldMeshes ? "Old meshes will be deleted." : "Old meshes will be kept."),
                "Swap", "Cancel"))
            {
                SwapMeshes();
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Build Bone Mapping (Advanced)"))
        {
            BuildBoneMapping();
            EditorUtility.DisplayDialog("Bone Mapping", 
                $"Built mapping for {boneMapping.Count} bones.\n\nCheck console for details.", "OK");
        }
    }
    
    private void DetectRootBones()
    {
        if (targetCharacter != null)
        {
            targetRootBone = FindRootBone(targetCharacter.transform);
            if (targetRootBone != null)
            {
                Debug.Log($"Detected target root bone: {GetFullPath(targetRootBone)}");
            }
        }
        
        if (sourceCharacter != null)
        {
            sourceRootBone = FindRootBone(sourceCharacter.transform);
            if (sourceRootBone != null)
            {
                Debug.Log($"Detected source root bone: {GetFullPath(sourceRootBone)}");
            }
        }
    }
    
    private Transform FindRootBone(Transform parent)
    {
        SkinnedMeshRenderer[] renderers = parent.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length == 0) return null;
        
        Transform commonRoot = renderers[0].rootBone;
        
        if (commonRoot == null && renderers[0].bones.Length > 0)
        {
            commonRoot = renderers[0].bones[0];
            while (commonRoot.parent != null && commonRoot.parent != parent)
            {
                commonRoot = commonRoot.parent;
            }
        }
        
        return commonRoot;
    }
    
    private void ScanSourceMeshes()
    {
        sourceMeshes.Clear();
        
        if (sourceCharacter == null) return;
        
        sourceMeshes = sourceCharacter.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        Debug.Log($"Found {sourceMeshes.Count} skinned meshes in source character");
    }
    
    private void BuildBoneMapping()
    {
        boneMapping.Clear();
        
        if (sourceRootBone == null || targetRootBone == null)
        {
            Debug.LogWarning("Cannot build bone mapping: Root bones not set");
            return;
        }
        
        Dictionary<string, Transform> sourceBones = new Dictionary<string, Transform>();
        Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();
        
        CollectBones(sourceRootBone, sourceBones);
        CollectBones(targetRootBone, targetBones);
        
        Debug.Log($"<color=cyan>=== BONE MAPPING ===</color>");
        Debug.Log($"Source bones: {sourceBones.Count}");
        Debug.Log($"Target bones: {targetBones.Count}");
        
        foreach (var kvp in sourceBones)
        {
            string sourceName = kvp.Key;
            string mappedName = MapBoneName(sourceName);
            
            if (targetBones.ContainsKey(mappedName))
            {
                boneMapping[sourceName] = mappedName;
                Debug.Log($"  ✓ {sourceName} → {mappedName}");
            }
            else
            {
                Debug.LogWarning($"  ✗ No match for {sourceName} (tried {mappedName})");
            }
        }
    }
    
    private void CollectBones(Transform root, Dictionary<string, Transform> boneDict)
    {
        boneDict[root.name] = root;
        foreach (Transform child in root)
        {
            CollectBones(child, boneDict);
        }
    }
    
    private string MapBoneName(string sourceName)
    {
        Dictionary<string, string> commonMappings = new Dictionary<string, string>
        {
            { "pelvis", "Hips" },
            { "spine_01", "Spine" },
            { "spine_02", "Spine1" },
            { "spine_03", "Spine2" },
            { "neck_01", "Neck" },
            { "head", "Head" },
            { "clavicle_l", "LeftShoulder" },
            { "upperarm_l", "LeftArm" },
            { "lowerarm_l", "LeftForeArm" },
            { "hand_l", "LeftHand" },
            { "clavicle_r", "RightShoulder" },
            { "upperarm_r", "RightArm" },
            { "lowerarm_r", "RightForeArm" },
            { "hand_r", "RightHand" },
            { "thigh_l", "LeftUpLeg" },
            { "calf_l", "LeftLeg" },
            { "foot_l", "LeftFoot" },
            { "ball_l", "LeftToeBase" },
            { "thigh_r", "RightUpLeg" },
            { "calf_r", "RightLeg" },
            { "foot_r", "RightFoot" },
            { "ball_r", "RightToeBase" },
            { "index_01_l", "LeftHandIndex1" },
            { "index_02_l", "LeftHandIndex2" },
            { "index_03_l", "LeftHandIndex3" },
            { "middle_01_l", "LeftHandMiddle1" },
            { "middle_02_l", "LeftHandMiddle2" },
            { "middle_03_l", "LeftHandMiddle3" },
            { "ring_01_l", "LeftHandRing1" },
            { "ring_02_l", "LeftHandRing2" },
            { "ring_03_l", "LeftHandRing3" },
            { "pinky_01_l", "LeftHandPinky1" },
            { "pinky_02_l", "LeftHandPinky2" },
            { "pinky_03_l", "LeftHandPinky3" },
            { "thumb_01_l", "LeftHandThumb1" },
            { "thumb_02_l", "LeftHandThumb2" },
            { "thumb_03_l", "LeftHandThumb3" },
            { "index_01_r", "RightHandIndex1" },
            { "index_02_r", "RightHandIndex2" },
            { "index_03_r", "RightHandIndex3" },
            { "middle_01_r", "RightHandMiddle1" },
            { "middle_02_r", "RightHandMiddle2" },
            { "middle_03_r", "RightHandMiddle3" },
            { "ring_01_r", "RightHandRing1" },
            { "ring_02_r", "RightHandRing2" },
            { "ring_03_r", "RightHandRing3" },
            { "pinky_01_r", "RightHandPinky1" },
            { "pinky_02_r", "RightHandPinky2" },
            { "pinky_03_r", "RightHandPinky3" },
            { "thumb_01_r", "RightHandThumb1" },
            { "thumb_02_r", "RightHandThumb2" },
            { "thumb_03_r", "RightHandThumb3" }
        };
        
        if (commonMappings.ContainsKey(sourceName))
        {
            return commonMappings[sourceName];
        }
        
        return sourceName;
    }
    
    private void SwapMeshes()
    {
        if (sourceMeshes.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No source meshes found!", "OK");
            return;
        }
        
        Undo.RegisterFullObjectHierarchyUndo(targetCharacter, "Mesh Swap");
        
        BuildBoneMapping();
        
        Dictionary<string, Transform> targetBonesLookup = new Dictionary<string, Transform>();
        CollectBones(targetRootBone, targetBonesLookup);
        
        List<SkinnedMeshRenderer> oldMeshes = new List<SkinnedMeshRenderer>();
        if (deleteOldMeshes)
        {
            oldMeshes = targetCharacter.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        }
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (SkinnedMeshRenderer sourceMesh in sourceMeshes)
        {
            GameObject newMeshObj = new GameObject(sourceMesh.name);
            newMeshObj.transform.SetParent(targetCharacter.transform, false);
            
            SkinnedMeshRenderer newRenderer = newMeshObj.AddComponent<SkinnedMeshRenderer>();
            
            newRenderer.sharedMesh = sourceMesh.sharedMesh;
            
            if (preserveMaterials && oldMeshes.Count > 0)
            {
                newRenderer.sharedMaterials = oldMeshes[0].sharedMaterials;
            }
            else
            {
                newRenderer.sharedMaterials = sourceMesh.sharedMaterials;
            }
            
            newRenderer.quality = sourceMesh.quality;
            newRenderer.updateWhenOffscreen = sourceMesh.updateWhenOffscreen;
            
            Transform[] newBones = new Transform[sourceMesh.bones.Length];
            bool allBonesFound = true;
            
            for (int i = 0; i < sourceMesh.bones.Length; i++)
            {
                if (sourceMesh.bones[i] == null) continue;
                
                string sourceBoneName = sourceMesh.bones[i].name;
                string targetBoneName = boneMapping.ContainsKey(sourceBoneName) ? boneMapping[sourceBoneName] : sourceBoneName;
                
                if (targetBonesLookup.ContainsKey(targetBoneName))
                {
                    newBones[i] = targetBonesLookup[targetBoneName];
                }
                else
                {
                    Debug.LogWarning($"Bone not found: {sourceBoneName} → {targetBoneName}");
                    allBonesFound = false;
                }
            }
            
            newRenderer.bones = newBones;
            newRenderer.rootBone = targetRootBone;
            
            if (allBonesFound)
            {
                successCount++;
            }
            else
            {
                failCount++;
            }
        }
        
        if (deleteOldMeshes)
        {
            foreach (var oldMesh in oldMeshes)
            {
                if (oldMesh != null && oldMesh.gameObject != null)
                {
                    Undo.DestroyObjectImmediate(oldMesh.gameObject);
                }
            }
        }
        
        EditorUtility.DisplayDialog("Mesh Swap Complete",
            $"Swapped {sourceMeshes.Count} meshes\n\n" +
            $"Success: {successCount}\n" +
            $"Warnings: {failCount}\n\n" +
            (deleteOldMeshes ? $"Deleted {oldMeshes.Count} old meshes" : "Old meshes preserved"),
            "OK");
        
        Debug.Log($"<color=green>Mesh swap complete! {successCount} meshes transferred successfully.</color>");
    }
    
    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
