using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool to replace JUTPS weapon objects in the scene with their default prefab versions
/// </summary>
public class JUTPSWeaponReplacer : EditorWindow
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

    private Vector2 scrollPosition;
    private List<GameObject> foundWeapons = new List<GameObject>();
    private List<bool> selectedWeapons = new List<bool>();
    private bool searchPerformed = false;
    private int replacedCount = 0;

    [MenuItem("Tools/JUTPS/Weapon Replacer")]
    public static void ShowWindow()
    {
        GetWindow<JUTPSWeaponReplacer>("JUTPS Weapon Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("JUTPS Weapon Replacer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool finds JUTPS weapon objects in the scene and replaces them with default prefab versions. " +
            "Useful for resetting weapons that have been modified and are causing issues.",
            MessageType.Info);

        EditorGUILayout.Space();

        // Search button
        if (GUILayout.Button("Search for JUTPS Weapons in Scene", GUILayout.Height(30)))
        {
            SearchForWeapons();
        }

        EditorGUILayout.Space();

        // Display results
        if (searchPerformed)
        {
            if (foundWeapons.Count == 0)
            {
                EditorGUILayout.HelpBox("No JUTPS weapons found in the scene.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"Found {foundWeapons.Count} weapon(s):", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("box");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                for (int i = 0; i < foundWeapons.Count; i++)
                {
                    if (foundWeapons[i] == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    selectedWeapons[i] = EditorGUILayout.Toggle(selectedWeapons[i], GUILayout.Width(20));
                    
                    EditorGUILayout.ObjectField(foundWeapons[i], typeof(GameObject), true);
                    
                    string weaponName = foundWeapons[i].name.Replace("(Clone)", "").Trim();
                    if (weaponPrefabPaths.ContainsKey(weaponName))
                    {
                        EditorGUILayout.LabelField("✓ Has Default", GUILayout.Width(100));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("⚠ No Default", GUILayout.Width(100));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                // Selection buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    for (int i = 0; i < selectedWeapons.Count; i++)
                        selectedWeapons[i] = true;
                }
                if (GUILayout.Button("Select None"))
                {
                    for (int i = 0; i < selectedWeapons.Count; i++)
                        selectedWeapons[i] = false;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // Replace button
                int selectedCount = selectedWeapons.Count(x => x);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button($"Replace Selected ({selectedCount}) with Defaults", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm Replacement",
                        $"Are you sure you want to replace {selectedCount} weapon(s) with their default prefabs?\n\n" +
                        "This action can be undone with Ctrl+Z.",
                        "Replace",
                        "Cancel"))
                    {
                        ReplaceSelectedWeapons();
                    }
                }
                GUI.backgroundColor = Color.white;

                if (replacedCount > 0)
                {
                    EditorGUILayout.HelpBox($"Successfully replaced {replacedCount} weapon(s)!", MessageType.Info);
                }
            }
        }
    }

    private void SearchForWeapons()
    {
        foundWeapons.Clear();
        selectedWeapons.Clear();
        replacedCount = 0;

        // Find all Weapon components
        var weaponComponents = FindObjectsOfType<JUTPS.WeaponSystem.Weapon>();
        foreach (var weapon in weaponComponents)
        {
            foundWeapons.Add(weapon.gameObject);
            selectedWeapons.Add(false);
        }

        // Find all MeleeWeapon components
        var meleeComponents = FindObjectsOfType<JUTPS.WeaponSystem.MeleeWeapon>();
        foreach (var melee in meleeComponents)
        {
            if (!foundWeapons.Contains(melee.gameObject))
            {
                foundWeapons.Add(melee.gameObject);
                selectedWeapons.Add(false);
            }
        }

        // Also search by common weapon names in case components are missing
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            string cleanName = obj.name.Replace("(Clone)", "").Trim();
            if (weaponPrefabPaths.ContainsKey(cleanName) && !foundWeapons.Contains(obj))
            {
                foundWeapons.Add(obj);
                selectedWeapons.Add(false);
            }
        }

        searchPerformed = true;
        Debug.Log($"JUTPSWeaponReplacer: Found {foundWeapons.Count} weapons in scene");
    }

    private void ReplaceSelectedWeapons()
    {
        replacedCount = 0;
        List<GameObject> toRemove = new List<GameObject>();

        for (int i = 0; i < foundWeapons.Count; i++)
        {
            if (!selectedWeapons[i] || foundWeapons[i] == null) continue;

            GameObject weaponObj = foundWeapons[i];
            string weaponName = weaponObj.name.Replace("(Clone)", "").Trim();

            if (!weaponPrefabPaths.ContainsKey(weaponName))
            {
                Debug.LogWarning($"No default prefab found for: {weaponName}");
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
            Undo.RegisterFullObjectHierarchyUndo(weaponObj, "Replace Weapon with Default");

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
            Undo.RegisterCreatedObjectUndo(newWeapon, "Replace Weapon with Default");

            // Mark old object for deletion
            toRemove.Add(weaponObj);
            
            replacedCount++;
            Debug.Log($"Replaced {weaponName} at {parent?.name ?? "root"} with default prefab");
        }

        // Destroy old weapons
        foreach (var obj in toRemove)
        {
            Undo.DestroyObjectImmediate(obj);
        }

        // Refresh search
        if (replacedCount > 0)
        {
            SearchForWeapons();
            EditorUtility.DisplayDialog("Success", $"Successfully replaced {replacedCount} weapon(s)!", "OK");
        }
    }
}
