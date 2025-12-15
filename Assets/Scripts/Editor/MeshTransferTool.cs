using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshTransferTool : EditorWindow
{
    private GameObject sourceCharacter;
    private GameObject targetCharacter;
    private Transform targetArmature;
    private bool preserveMaterials = true;
    private bool deleteSourceAfterTransfer = false;
    private bool autoFindArmature = true;
    private Vector2 scrollPosition;
    
    private List<SkinnedMeshRenderer> foundSourceMeshes = new List<SkinnedMeshRenderer>();
    private List<bool> selectedMeshes = new List<bool>();
    
    [MenuItem("Tools/Character/Mesh Transfer Tool")]
    public static void ShowWindow()
    {
        MeshTransferTool window = GetWindow<MeshTransferTool>("Mesh Transfer Tool");
        window.minSize = new Vector2(400, 500);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Mesh Transfer Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Transfer multiple mesh parts from one character to another while preserving the target's armature.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginChangeCheck();
        sourceCharacter = (GameObject)EditorGUILayout.ObjectField("Source Character", sourceCharacter, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck() && sourceCharacter != null)
        {
            ScanSourceMeshes();
        }
        
        targetCharacter = (GameObject)EditorGUILayout.ObjectField("Target Character", targetCharacter, typeof(GameObject), true);
        
        EditorGUILayout.Space(5);
        
        autoFindArmature = EditorGUILayout.Toggle("Auto-Find Target Armature", autoFindArmature);
        
        if (!autoFindArmature)
        {
            targetArmature = (Transform)EditorGUILayout.ObjectField("Target Armature Root", targetArmature, typeof(Transform), true);
        }
        
        EditorGUILayout.Space(5);
        
        preserveMaterials = EditorGUILayout.Toggle("Preserve Materials", preserveMaterials);
        deleteSourceAfterTransfer = EditorGUILayout.Toggle("Delete Source After Transfer", deleteSourceAfterTransfer);
        
        EditorGUILayout.Space(10);
        
        if (sourceCharacter != null && foundSourceMeshes.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {foundSourceMeshes.Count} Mesh Parts", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                for (int i = 0; i < selectedMeshes.Count; i++)
                    selectedMeshes[i] = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                for (int i = 0; i < selectedMeshes.Count; i++)
                    selectedMeshes[i] = false;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            for (int i = 0; i < foundSourceMeshes.Count; i++)
            {
                if (foundSourceMeshes[i] == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                selectedMeshes[i] = EditorGUILayout.Toggle(selectedMeshes[i], GUILayout.Width(20));
                EditorGUILayout.LabelField(foundSourceMeshes[i].name, GUILayout.Width(200));
                
                int boneCount = foundSourceMeshes[i].bones != null ? foundSourceMeshes[i].bones.Length : 0;
                EditorGUILayout.LabelField($"{boneCount} bones", GUILayout.Width(80));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space(10);
        
        GUI.enabled = sourceCharacter != null && targetCharacter != null && selectedMeshes.Any(x => x);
        
        if (GUILayout.Button("Transfer Selected Meshes", GUILayout.Height(40)))
        {
            TransferMeshes();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "How to use:\n" +
            "1. Drag source character (with many mesh parts) to 'Source Character'\n" +
            "2. Drag target character (with armature) to 'Target Character'\n" +
            "3. Select which mesh parts to transfer\n" +
            "4. Click 'Transfer Selected Meshes'\n\n" +
            "The tool will automatically retarget bones to the target's armature.",
            MessageType.None);
    }
    
    private void ScanSourceMeshes()
    {
        foundSourceMeshes.Clear();
        selectedMeshes.Clear();
        
        if (sourceCharacter == null) return;
        
        SkinnedMeshRenderer[] meshes = sourceCharacter.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foundSourceMeshes.AddRange(meshes);
        
        for (int i = 0; i < foundSourceMeshes.Count; i++)
        {
            selectedMeshes.Add(true);
        }
    }
    
    private void TransferMeshes()
    {
        if (sourceCharacter == null || targetCharacter == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign both source and target characters.", "OK");
            return;
        }
        
        Transform armatureRoot = targetArmature;
        
        if (autoFindArmature || armatureRoot == null)
        {
            armatureRoot = FindArmatureRoot(targetCharacter.transform);
            
            if (armatureRoot == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find armature in target character. Please manually assign the armature root.", "OK");
                return;
            }
        }
        
        Undo.RegisterFullObjectHierarchyUndo(targetCharacter, "Transfer Meshes");
        
        int transferredCount = 0;
        List<string> failedMeshes = new List<string>();
        
        for (int i = 0; i < foundSourceMeshes.Count; i++)
        {
            if (!selectedMeshes[i] || foundSourceMeshes[i] == null) continue;
            
            SkinnedMeshRenderer sourceMesh = foundSourceMeshes[i];
            
            GameObject newMeshObj = new GameObject(sourceMesh.name);
            newMeshObj.transform.SetParent(targetCharacter.transform);
            newMeshObj.transform.localPosition = Vector3.zero;
            newMeshObj.transform.localRotation = Quaternion.identity;
            newMeshObj.transform.localScale = Vector3.one;
            
            SkinnedMeshRenderer newMesh = newMeshObj.AddComponent<SkinnedMeshRenderer>();
            
            newMesh.sharedMesh = sourceMesh.sharedMesh;
            
            if (preserveMaterials)
            {
                newMesh.sharedMaterials = sourceMesh.sharedMaterials;
            }
            
            newMesh.quality = sourceMesh.quality;
            newMesh.updateWhenOffscreen = sourceMesh.updateWhenOffscreen;
            
            if (RetargetBones(sourceMesh, newMesh, armatureRoot))
            {
                transferredCount++;
            }
            else
            {
                failedMeshes.Add(sourceMesh.name);
            }
        }
        
        if (deleteSourceAfterTransfer && transferredCount > 0)
        {
            Undo.DestroyObjectImmediate(sourceCharacter);
        }
        
        string message = $"Successfully transferred {transferredCount} mesh parts to {targetCharacter.name}.";
        
        if (failedMeshes.Count > 0)
        {
            message += $"\n\nFailed to retarget bones for:\n{string.Join("\n", failedMeshes)}";
            EditorUtility.DisplayDialog("Transfer Complete (with warnings)", message, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Success", message, "OK");
        }
        
        EditorUtility.SetDirty(targetCharacter);
    }
    
    private Transform FindArmatureRoot(Transform character)
    {
        string[] commonArmatureNames = { "Armature", "root", "RootBone", "Root", "Skeleton", "Rig" };
        
        foreach (string name in commonArmatureNames)
        {
            Transform found = character.Find(name);
            if (found != null) return found;
        }
        
        foreach (Transform child in character)
        {
            if (child.childCount > 0 && child.GetComponent<SkinnedMeshRenderer>() == null)
            {
                Transform pelvisOrHips = FindBoneRecursive(child, new[] { "pelvis", "Hips", "hips", "spine_01", "Spine" });
                if (pelvisOrHips != null)
                {
                    return child;
                }
            }
        }
        
        return null;
    }
    
    private Transform FindBoneRecursive(Transform parent, string[] names)
    {
        foreach (string name in names)
        {
            if (parent.name.ToLower().Contains(name.ToLower()))
                return parent;
        }
        
        foreach (Transform child in parent)
        {
            Transform found = FindBoneRecursive(child, names);
            if (found != null) return found;
        }
        
        return null;
    }
    
    private bool RetargetBones(SkinnedMeshRenderer source, SkinnedMeshRenderer target, Transform targetArmatureRoot)
    {
        if (source.bones == null || source.bones.Length == 0)
        {
            Debug.LogWarning($"Source mesh {source.name} has no bones.");
            return false;
        }
        
        Transform[] newBones = new Transform[source.bones.Length];
        bool allBonesFound = true;
        
        for (int i = 0; i < source.bones.Length; i++)
        {
            if (source.bones[i] == null)
            {
                newBones[i] = null;
                continue;
            }
            
            string boneName = source.bones[i].name;
            Transform foundBone = FindBoneByName(targetArmatureRoot, boneName);
            
            if (foundBone != null)
            {
                newBones[i] = foundBone;
            }
            else
            {
                Debug.LogWarning($"Could not find bone '{boneName}' in target armature for mesh {source.name}");
                allBonesFound = false;
                newBones[i] = targetArmatureRoot;
            }
        }
        
        target.bones = newBones;
        
        target.rootBone = source.rootBone != null ? 
            FindBoneByName(targetArmatureRoot, source.rootBone.name) ?? targetArmatureRoot : 
            targetArmatureRoot;
        
        return allBonesFound;
    }
    
    private Transform FindBoneByName(Transform root, string boneName)
    {
        if (root.name == boneName)
            return root;
        
        foreach (Transform child in root)
        {
            Transform found = FindBoneByName(child, boneName);
            if (found != null)
                return found;
        }
        
        return null;
    }
}
