using UnityEngine;
using UnityEditor;
using JUTPS.WeaponSystem;
using JUTPS.ItemSystem;
using JUTPS.InventorySystem;

/// <summary>
/// Diagnostic tool to fix weapon positioning issues in JUTPS
/// </summary>
public class JUTPSWeaponPositionFixer : EditorWindow
{
    private GameObject playerCharacter;
    private Weapon selectedWeapon;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/JUTPS/Weapon Position Fixer")]
    public static void ShowWindow()
    {
        GetWindow<JUTPSWeaponPositionFixer>("Weapon Position Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("JUTPS Weapon Position Diagnostic", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool helps diagnose and fix weapon positioning issues.\n\n" +
            "Common causes:\n" +
            "• Missing or incorrectly positioned Weapon Reference Positions\n" +
            "• Wrong ItemWieldPositionID\n" +
            "• Missing WeaponAimRotationCenter component\n" +
            "• Incorrect OppositeHandPosition (left hand IK)",
            MessageType.Info);

        EditorGUILayout.Space();

        // Select player character
        playerCharacter = (GameObject)EditorGUILayout.ObjectField(
            "Player Character", 
            playerCharacter, 
            typeof(GameObject), 
            true);

        EditorGUILayout.Space();

        if (playerCharacter == null)
        {
            EditorGUILayout.HelpBox("Select your player character to diagnose weapon positioning.", MessageType.Warning);
            return;
        }

        // Find WeaponAimRotationCenter
        var weaponCenter = playerCharacter.GetComponentInChildren<WeaponAimRotationCenter>();
        
        if (weaponCenter == null)
        {
            EditorGUILayout.HelpBox("ERROR: WeaponAimRotationCenter component not found!\n\n" +
                "Your player needs this component for weapons to position correctly.", 
                MessageType.Error);
            
            if (GUILayout.Button("Search in Scene for WeaponAimRotationCenter"))
            {
                var centers = FindObjectsOfType<WeaponAimRotationCenter>();
                if (centers.Length > 0)
                {
                    Selection.activeGameObject = centers[0].gameObject;
                    EditorGUIUtility.PingObject(centers[0].gameObject);
                    Debug.Log($"Found WeaponAimRotationCenter at: {centers[0].gameObject.name}");
                }
            }
            return;
        }

        // Display weapon center info
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Weapon Rotation Center Found", EditorStyles.boldLabel);
        EditorGUILayout.ObjectField("Component", weaponCenter, typeof(WeaponAimRotationCenter), true);
        
        if (weaponCenter.WeaponPositionTransform != null)
        {
            EditorGUILayout.LabelField($"Weapon Positions: {weaponCenter.WeaponPositionTransform.Count}");
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            for (int i = 0; i < weaponCenter.WeaponPositionTransform.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Position {i}:", GUILayout.Width(80));
                EditorGUILayout.ObjectField(weaponCenter.WeaponPositionTransform[i], typeof(Transform), true);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Find weapons in inventory
        var inventory = playerCharacter.GetComponentInChildren<JUInventory>();
        if (inventory != null && inventory.AllHoldableItems != null && inventory.AllHoldableItems.Length > 0)
        {
            EditorGUILayout.LabelField("Weapons in Inventory:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            foreach (var item in inventory.AllHoldableItems)
            {
                if (item == null) continue;
                
                var weapon = item.GetComponent<Weapon>();
                if (weapon != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        selectedWeapon = weapon;
                        Selection.activeGameObject = weapon.gameObject;
                    }
                    
                    EditorGUILayout.ObjectField(weapon.gameObject, typeof(GameObject), true);
                    
                    // Display key settings
                    EditorGUILayout.LabelField($"WieldID: {weapon.ItemWieldPositionID}", GUILayout.Width(80));
                    
                    bool hasLeftHandIK = weapon.OppositeHandPosition != null;
                    GUI.color = hasLeftHandIK ? Color.green : Color.red;
                    EditorGUILayout.LabelField(hasLeftHandIK ? "✓ Has IK" : "✗ No IK", GUILayout.Width(70));
                    GUI.color = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        // Selected weapon details
        if (selectedWeapon != null)
        {
            EditorGUILayout.LabelField("Selected Weapon Details:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.ObjectField("Weapon", selectedWeapon.gameObject, typeof(GameObject), true);
            EditorGUILayout.LabelField($"Name: {selectedWeapon.ItemName}");
            EditorGUILayout.LabelField($"Wield Position ID: {selectedWeapon.ItemWieldPositionID}");
            
            if (selectedWeapon.OppositeHandPosition != null)
            {
                EditorGUILayout.ObjectField("Left Hand IK Position", selectedWeapon.OppositeHandPosition, typeof(Transform), true);
            }
            else
            {
                EditorGUILayout.HelpBox("Missing Left Hand IK Position! Weapon won't position correctly.", MessageType.Warning);
            }
            
            if (weaponCenter != null && selectedWeapon.ItemWieldPositionID < weaponCenter.WeaponPositionTransform.Count)
            {
                Transform targetPos = weaponCenter.WeaponPositionTransform[selectedWeapon.ItemWieldPositionID];
                EditorGUILayout.ObjectField("Target Wield Position", targetPos, typeof(Transform), true);
            }
            else
            {
                EditorGUILayout.HelpBox($"ERROR: Wield Position ID {selectedWeapon.ItemWieldPositionID} is out of range!", MessageType.Error);
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Fix buttons
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Reset to Wield Position 0 (Default)", GUILayout.Height(30)))
            {
                Undo.RecordObject(selectedWeapon, "Reset Wield Position");
                selectedWeapon.ItemWieldPositionID = 0;
                EditorUtility.SetDirty(selectedWeapon);
                Debug.Log($"Reset {selectedWeapon.ItemName} to Wield Position 0");
            }
            
            if (selectedWeapon.OppositeHandPosition == null)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Create Left Hand IK Position", GUILayout.Height(30)))
                {
                    CreateLeftHandIKPosition(selectedWeapon);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space();

        // Quick fix button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Auto-Fix All Weapons", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Auto-Fix Weapons",
                "This will:\n" +
                "• Reset all weapons to Wield Position 0\n" +
                "• Create missing Left Hand IK positions\n\n" +
                "Continue?",
                "Yes",
                "Cancel"))
            {
                AutoFixAllWeapons(inventory);
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void CreateLeftHandIKPosition(Weapon weapon)
    {
        GameObject ikObj = new GameObject("Left Hand IK Position");
        ikObj.transform.SetParent(weapon.transform);
        ikObj.transform.localPosition = new Vector3(-0.07f, -0.12f, -0.18f);
        ikObj.transform.localRotation = Quaternion.Euler(47.5f, 3f, 107f);
        
        Undo.RegisterCreatedObjectUndo(ikObj, "Create Left Hand IK");
        Undo.RecordObject(weapon, "Assign Left Hand IK");
        weapon.OppositeHandPosition = ikObj.transform;
        
        EditorUtility.SetDirty(weapon);
        Debug.Log($"Created Left Hand IK Position for {weapon.ItemName}");
    }

    private void AutoFixAllWeapons(JUInventory inventory)
    {
        if (inventory == null || inventory.AllHoldableItems == null) return;
        
        int fixedCount = 0;
        
        foreach (var item in inventory.AllHoldableItems)
        {
            if (item == null) continue;
            
            var weapon = item.GetComponent<Weapon>();
            if (weapon == null) continue;
            
            // Reset wield position
            if (weapon.ItemWieldPositionID != 0)
            {
                Undo.RecordObject(weapon, "Auto-Fix Weapon");
                weapon.ItemWieldPositionID = 0;
                EditorUtility.SetDirty(weapon);
                fixedCount++;
            }
            
            // Create missing IK position
            if (weapon.OppositeHandPosition == null)
            {
                CreateLeftHandIKPosition(weapon);
                fixedCount++;
            }
        }
        
        EditorUtility.DisplayDialog("Auto-Fix Complete", 
            $"Fixed {fixedCount} issue(s)!\n\nTest your weapons in Play mode.", 
            "OK");
    }
}
