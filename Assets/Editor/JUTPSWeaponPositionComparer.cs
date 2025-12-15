using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using JUTPS.WeaponSystem;
using JUTPS.ItemSystem;

/// <summary>
/// Compares player weapon transforms with JUTPS defaults and reverts differences
/// </summary>
public class JUTPSWeaponPositionComparer : EditorWindow
{
    private Dictionary<string, string> weaponPrefabPaths = new Dictionary<string, string>()
    {
        { "UMP", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/UMP.prefab" },
        { "P226", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/P226.prefab" },
        { "P226 Dual", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/P226 Dual.prefab" },
        { "SNIPER M82", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/SNIPER M82.prefab" },
        { "Benelli M4", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Weapons/Guns/Benelli M4.prefab" },
        { "Katana", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Melee Weapons/Katana.prefab" },
        { "BaseballBat", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Melee Weapons/BaseballBat.prefab" },
        { "Granade", "Assets/Julhiecio TPS Controller/Demos/Demo Prefabs/Items/Throwable/Granade.prefab" },
    };

    private GameObject playerCharacter;
    private List<WeaponComparison> comparisons = new List<WeaponComparison>();
    private Vector2 scrollPosition;
    private bool comparisonPerformed = false;
    private int revertedCount = 0;
    private float positionThreshold = 0.001f;
    private float rotationThreshold = 0.1f;
    private float scaleThreshold = 0.001f;

    private class WeaponComparison
    {
        public GameObject playerWeapon;
        public GameObject defaultPrefab;
        public string weaponName;
        public bool hasDifferences;
        public bool positionDifferent;
        public bool rotationDifferent;
        public bool scaleDifferent;
        public Vector3 playerPosition;
        public Vector3 defaultPosition;
        public Quaternion playerRotation;
        public Quaternion defaultRotation;
        public Vector3 playerScale;
        public Vector3 defaultScale;
        public bool selected;
    }

    [MenuItem("Tools/JUTPS/Compare & Revert Weapon Positions")]
    public static void ShowWindow()
    {
        GetWindow<JUTPSWeaponPositionComparer>("Weapon Position Comparer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Compare & Revert Weapon Positions", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Compares your player's weapon transforms with JUTPS default prefabs.\n" +
            "Detects any differences in position, rotation, or scale and allows you to revert to defaults.",
            MessageType.Info);

        EditorGUILayout.Space();

        // Player selection
        playerCharacter = (GameObject)EditorGUILayout.ObjectField(
            "Player Character", 
            playerCharacter, 
            typeof(GameObject), 
            true);

        if (playerCharacter == null)
        {
            EditorGUILayout.HelpBox("Select your player character to compare weapons.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // Threshold settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Comparison Thresholds:", EditorStyles.boldLabel);
        positionThreshold = EditorGUILayout.Slider("Position Threshold", positionThreshold, 0.0001f, 0.1f);
        rotationThreshold = EditorGUILayout.Slider("Rotation Threshold", rotationThreshold, 0.01f, 5f);
        scaleThreshold = EditorGUILayout.Slider("Scale Threshold", scaleThreshold, 0.0001f, 0.1f);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Compare button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Compare Weapons with Defaults", GUILayout.Height(35)))
        {
            CompareWeapons();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();

        // Display results
        if (comparisonPerformed)
        {
            if (comparisons.Count == 0)
            {
                EditorGUILayout.HelpBox("No weapons found to compare.", MessageType.Warning);
            }
            else
            {
                int differencesFound = comparisons.FindAll(c => c.hasDifferences).Count;
                
                if (differencesFound == 0)
                {
                    EditorGUILayout.HelpBox("✓ All weapons match defaults! No differences found.", MessageType.Info);
                }
                else
                {
                    GUI.backgroundColor = Color.yellow;
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"⚠ Found {differencesFound} weapon(s) with differences!", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();

                // Comparison results
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                
                foreach (var comparison in comparisons)
                {
                    if (comparison.playerWeapon == null || comparison.defaultPrefab == null) continue;

                    Color boxColor = comparison.hasDifferences ? new Color(1f, 0.8f, 0.5f) : new Color(0.8f, 1f, 0.8f);
                    GUI.backgroundColor = boxColor;
                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.BeginHorizontal();
                    
                    if (comparison.hasDifferences)
                    {
                        comparison.selected = EditorGUILayout.Toggle(comparison.selected, GUILayout.Width(20));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                    }
                    
                    EditorGUILayout.LabelField(comparison.weaponName, EditorStyles.boldLabel, GUILayout.Width(150));
                    
                    if (comparison.hasDifferences)
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("HAS DIFFERENCES", GUILayout.Width(130));
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("Matches Default", GUILayout.Width(130));
                        GUI.color = Color.white;
                    }
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = comparison.playerWeapon;
                        EditorGUIUtility.PingObject(comparison.playerWeapon);
                    }
                    
                    EditorGUILayout.EndHorizontal();

                    if (comparison.hasDifferences)
                    {
                        EditorGUI.indentLevel++;
                        
                        if (comparison.positionDifferent)
                        {
                            EditorGUILayout.LabelField($"Position: Player={comparison.playerPosition.ToString("F4")} | Default={comparison.defaultPosition.ToString("F4")}");
                        }
                        
                        if (comparison.rotationDifferent)
                        {
                            EditorGUILayout.LabelField($"Rotation: Player={comparison.playerRotation.eulerAngles.ToString("F2")} | Default={comparison.defaultRotation.eulerAngles.ToString("F2")}");
                        }
                        
                        if (comparison.scaleDifferent)
                        {
                            EditorGUILayout.LabelField($"Scale: Player={comparison.playerScale.ToString("F4")} | Default={comparison.defaultScale.ToString("F4")}");
                        }
                        
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.EndScrollView();

                if (differencesFound > 0)
                {
                    EditorGUILayout.Space();

                    // Selection buttons
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select All Differences"))
                    {
                        foreach (var comp in comparisons)
                        {
                            if (comp.hasDifferences) comp.selected = true;
                        }
                    }
                    if (GUILayout.Button("Select None"))
                    {
                        foreach (var comp in comparisons)
                        {
                            comp.selected = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // Revert button
                    int selectedCount = comparisons.FindAll(c => c.selected).Count;
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button($"Revert Selected ({selectedCount}) to Defaults", GUILayout.Height(40)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Confirm Revert",
                            $"Revert {selectedCount} weapon(s) to default positions?\n\n" +
                            "This will reset their local transform to match JUTPS defaults.\n" +
                            "This action can be undone with Ctrl+Z.",
                            "Revert",
                            "Cancel"))
                        {
                            RevertSelectedWeapons();
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    if (revertedCount > 0)
                    {
                        EditorGUILayout.HelpBox($"Successfully reverted {revertedCount} weapon(s) to defaults!", MessageType.Info);
                    }
                }
            }
        }
    }

    private void CompareWeapons()
    {
        comparisons.Clear();
        revertedCount = 0;
        comparisonPerformed = false;

        if (playerCharacter == null) return;

        // Find all weapons on the player
        Weapon[] weapons = playerCharacter.GetComponentsInChildren<Weapon>(true);
        
        foreach (var weapon in weapons)
        {
            string weaponName = weapon.gameObject.name.Replace("(Clone)", "").Trim();
            
            if (!weaponPrefabPaths.ContainsKey(weaponName))
            {
                Debug.LogWarning($"No default prefab registered for: {weaponName}");
                continue;
            }

            string prefabPath = weaponPrefabPaths[weaponName];
            GameObject defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (defaultPrefab == null)
            {
                Debug.LogError($"Failed to load default prefab at: {prefabPath}");
                continue;
            }

            WeaponComparison comparison = new WeaponComparison
            {
                playerWeapon = weapon.gameObject,
                defaultPrefab = defaultPrefab,
                weaponName = weaponName,
                playerPosition = weapon.transform.localPosition,
                playerRotation = weapon.transform.localRotation,
                playerScale = weapon.transform.localScale,
                defaultPosition = defaultPrefab.transform.localPosition,
                defaultRotation = defaultPrefab.transform.localRotation,
                defaultScale = defaultPrefab.transform.localScale,
                selected = false
            };

            // Check for differences
            comparison.positionDifferent = Vector3.Distance(comparison.playerPosition, comparison.defaultPosition) > positionThreshold;
            comparison.rotationDifferent = Quaternion.Angle(comparison.playerRotation, comparison.defaultRotation) > rotationThreshold;
            comparison.scaleDifferent = Vector3.Distance(comparison.playerScale, comparison.defaultScale) > scaleThreshold;
            
            comparison.hasDifferences = comparison.positionDifferent || comparison.rotationDifferent || comparison.scaleDifferent;

            if (comparison.hasDifferences)
            {
                comparison.selected = true; // Auto-select items with differences
            }

            comparisons.Add(comparison);
        }

        comparisonPerformed = true;
        
        int diffCount = comparisons.FindAll(c => c.hasDifferences).Count;
        Debug.Log($"Comparison complete: {comparisons.Count} weapons checked, {diffCount} have differences");
    }

    private void RevertSelectedWeapons()
    {
        revertedCount = 0;

        foreach (var comparison in comparisons)
        {
            if (!comparison.selected || !comparison.hasDifferences) continue;
            if (comparison.playerWeapon == null || comparison.defaultPrefab == null) continue;

            Undo.RecordObject(comparison.playerWeapon.transform, "Revert Weapon Position");

            if (comparison.positionDifferent)
            {
                comparison.playerWeapon.transform.localPosition = comparison.defaultPosition;
            }

            if (comparison.rotationDifferent)
            {
                comparison.playerWeapon.transform.localRotation = comparison.defaultRotation;
            }

            if (comparison.scaleDifferent)
            {
                comparison.playerWeapon.transform.localScale = comparison.defaultScale;
            }

            EditorUtility.SetDirty(comparison.playerWeapon);
            revertedCount++;
            
            Debug.Log($"Reverted {comparison.weaponName} to default transform");
        }

        if (revertedCount > 0)
        {
            // Re-compare after reverting
            CompareWeapons();
            EditorUtility.DisplayDialog("Success", 
                $"Reverted {revertedCount} weapon(s) to default positions!", 
                "OK");
        }
    }
}
