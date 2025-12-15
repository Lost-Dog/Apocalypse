using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Replaces all weapon prefabs in player's right hand bone with JUTPS defaults
/// </summary>
public class JUTPSRightHandWeaponReplacer : EditorWindow
{
    private Dictionary<string, string> weaponPrefabPaths = new Dictionary<string, string>()
    {
        // Guns
        { "UMP", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/UMP.prefab" },
        { "P226", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/P226.prefab" },
        { "P226 Dual", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/P226 Dual.prefab" },
        { "SNIPER M82", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/SNIPER M82.prefab" },
        { "Benelli M4", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/Benelli M4.prefab" },
        
        // Melee Weapons
        { "Katana", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Melee Weapons/Katana.prefab" },
        { "BaseballBat", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Melee Weapons/BaseballBat.prefab" },
        
        // Throwables
        { "Granade", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Throwable/Granade.prefab" },
    };

    private GameObject playerCharacter;
    private Transform rightHandBone;
    private List<GameObject> foundWeapons = new List<GameObject>();
    private int replacedCount = 0;

    [MenuItem("Tools/JUTPS/Replace Right Hand Weapons with Defaults")]
    public static void ShowWindow()
    {
        GetWindow<JUTPSRightHandWeaponReplacer>("Replace Right Hand Weapons");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Right Hand Weapons with Defaults", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool finds all weapons in the player's right hand bone and replaces them with default JUTPS prefabs.\n\n" +
            "This fixes issues caused by modified weapon prefabs.",
            MessageType.Info);

        EditorGUILayout.Space();

        // Select player
        playerCharacter = (GameObject)EditorGUILayout.ObjectField(
            "Player Character", 
            playerCharacter, 
            typeof(GameObject), 
            true);

        if (playerCharacter == null)
        {
            EditorGUILayout.HelpBox("Select your player character to find weapons in right hand.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // Find right hand bone
        if (GUILayout.Button("Find Right Hand Bone", GUILayout.Height(30)))
        {
            FindRightHandBone();
        }

        if (rightHandBone != null)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Right Hand Bone Found:", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(rightHandBone, typeof(Transform), true);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Search for weapons
            if (GUILayout.Button("Search for Weapons in Right Hand", GUILayout.Height(30)))
            {
                SearchWeaponsInRightHand();
            }

            if (foundWeapons.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Found {foundWeapons.Count} weapon(s):", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("box");
                foreach (var weapon in foundWeapons)
                {
                    if (weapon == null) continue;
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(weapon, typeof(GameObject), true);
                    
                    string weaponName = weapon.name.Replace("(Clone)", "").Trim();
                    if (weaponPrefabPaths.ContainsKey(weaponName))
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("✓ Has Default", GUILayout.Width(100));
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("⚠ Unknown", GUILayout.Width(100));
                        GUI.color = Color.white;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                // Replace button
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button($"Replace All {foundWeapons.Count} Weapons with Defaults", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm Replacement",
                        $"Replace {foundWeapons.Count} weapon(s) in right hand with default JUTPS prefabs?\n\n" +
                        "This action can be undone with Ctrl+Z.",
                        "Replace",
                        "Cancel"))
                    {
                        ReplaceAllWeapons();
                    }
                }
                GUI.backgroundColor = Color.white;

                if (replacedCount > 0)
                {
                    EditorGUILayout.HelpBox($"Successfully replaced {replacedCount} weapon(s)!", MessageType.Info);
                }
            }
            else if (foundWeapons.Count == 0 && rightHandBone != null)
            {
                EditorGUILayout.HelpBox("No weapons found in right hand. Try searching for weapons.", MessageType.Info);
            }
        }
    }

    private void FindRightHandBone()
    {
        if (playerCharacter == null) return;

        Animator animator = playerCharacter.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
            
            if (rightHandBone != null)
            {
                Debug.Log($"Found right hand bone: {rightHandBone.name}");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Right hand bone not found!\n\nMake sure your character has a Humanoid rig.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Animator component not found on player character!", "OK");
        }
    }

    private void SearchWeaponsInRightHand()
    {
        foundWeapons.Clear();
        replacedCount = 0;

        if (rightHandBone == null) return;

        // Search all children of right hand bone
        foreach (Transform child in rightHandBone.GetComponentsInChildren<Transform>())
        {
            if (child == rightHandBone) continue;

            string cleanName = child.name.Replace("(Clone)", "").Trim();
            
            // Check if it's a known weapon
            if (weaponPrefabPaths.ContainsKey(cleanName))
            {
                foundWeapons.Add(child.gameObject);
            }
            // Also check for Weapon component
            else if (child.GetComponent<JUTPS.WeaponSystem.Weapon>() != null)
            {
                foundWeapons.Add(child.gameObject);
            }
            // Check for MeleeWeapon component
            else if (child.GetComponent<JUTPS.WeaponSystem.MeleeWeapon>() != null)
            {
                foundWeapons.Add(child.gameObject);
            }
        }

        Debug.Log($"Found {foundWeapons.Count} weapons in right hand bone");
    }

    private void ReplaceAllWeapons()
    {
        replacedCount = 0;
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject weaponObj in foundWeapons)
        {
            if (weaponObj == null) continue;

            string weaponName = weaponObj.name.Replace("(Clone)", "").Trim();

            if (!weaponPrefabPaths.ContainsKey(weaponName))
            {
                Debug.LogWarning($"No default prefab found for: {weaponName}. Skipping...");
                continue;
            }

            string prefabPath = weaponPrefabPaths[weaponName];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab at: {prefabPath}");
                continue;
            }

            // Record for undo
            Undo.RegisterFullObjectHierarchyUndo(weaponObj, "Replace Right Hand Weapon");

            // Store transform data
            Transform parent = weaponObj.transform.parent;
            Vector3 localPos = weaponObj.transform.localPosition;
            Quaternion localRot = weaponObj.transform.localRotation;
            Vector3 localScale = weaponObj.transform.localScale;
            int siblingIndex = weaponObj.transform.GetSiblingIndex();

            // Instantiate new weapon from prefab
            GameObject newWeapon = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            
            // Restore transform
            newWeapon.transform.SetParent(parent);
            newWeapon.transform.localPosition = localPos;
            newWeapon.transform.localRotation = localRot;
            newWeapon.transform.localScale = localScale;
            newWeapon.transform.SetSiblingIndex(siblingIndex);

            // Register new object for undo
            Undo.RegisterCreatedObjectUndo(newWeapon, "Replace Right Hand Weapon");

            // Mark old object for deletion
            toRemove.Add(weaponObj);
            
            replacedCount++;
            Debug.Log($"Replaced {weaponName} in right hand with default prefab");
        }

        // Destroy old weapons
        foreach (var obj in toRemove)
        {
            Undo.DestroyObjectImmediate(obj);
        }

        // Refresh search
        if (replacedCount > 0)
        {
            SearchWeaponsInRightHand();
            EditorUtility.DisplayDialog("Success", 
                $"Successfully replaced {replacedCount} weapon(s)!\n\nTest in Play mode to verify.", 
                "OK");
        }
    }
}
