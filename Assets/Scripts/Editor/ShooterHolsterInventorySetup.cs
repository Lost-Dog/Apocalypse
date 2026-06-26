#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using UnityEditor;
using UnityEngine;

public static class ShooterHolsterInventorySetup
{
    private const string OutputFolder = "Assets/Gameplay/Inventory/GeneratedWeaponItems";
    private const string MenuRoot = "Tools/Apocalypse/Shooter/";

    [MenuItem(MenuRoot + "Create Weapon Items + Populate Selected Holster")]
    private static void CreateItemsAndPopulateSelectedHolster()
    {
        ShooterHolsterHotkey holster = GetSelectedHolster();
        if (holster == null)
        {
            Debug.LogWarning("[ShooterHolsterInventorySetup] Select a GameObject with ShooterHolsterHotkey first.");
            return;
        }

        PopulateHolsterMappings(holster);
    }

    [MenuItem(MenuRoot + "Create Weapon Items + Populate All Open-Scene Holsters")]
    private static void CreateItemsAndPopulateAllHolsters()
    {
        ShooterHolsterHotkey[] holsters = UnityEngine.Object.FindObjectsByType<ShooterHolsterHotkey>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (holsters == null || holsters.Length == 0)
        {
            Debug.LogWarning("[ShooterHolsterInventorySetup] No ShooterHolsterHotkey components found in open scenes.");
            return;
        }

        int count = 0;
        for (int i = 0; i < holsters.Length; i++)
        {
            PopulateHolsterMappings(holsters[i]);
            count += 1;
        }

        Debug.Log($"[ShooterHolsterInventorySetup] Updated {count} holster component(s).");
    }

    private static ShooterHolsterHotkey GetSelectedHolster()
    {
        if (Selection.activeGameObject == null) return null;
        return Selection.activeGameObject.GetComponentInParent<ShooterHolsterHotkey>();
    }

    private static void PopulateHolsterMappings(ShooterHolsterHotkey holster)
    {
        EnsureFolder(OutputFolder);

        List<ShooterWeapon> weapons = FindShooterWeapons();
        if (weapons.Count == 0)
        {
            Debug.LogWarning("[ShooterHolsterInventorySetup] No ShooterWeapon assets found.");
            return;
        }

        Dictionary<ShooterWeapon, Item> map = CreateOrLoadWeaponItems(weapons);
        ApplyMappings(holster, weapons, map);

        EditorUtility.SetDirty(holster);
        PrefabUtility.RecordPrefabInstancePropertyModifications(holster);

        Debug.Log($"[ShooterHolsterInventorySetup] Populated {weapons.Count} mapping(s) on '{holster.name}'.");
    }

    private static List<ShooterWeapon> FindShooterWeapons()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShooterWeapon");
        List<ShooterWeapon> weapons = new List<ShooterWeapon>(guids.Length);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path.IndexOf("Shooter.Examples@", StringComparison.OrdinalIgnoreCase) >= 0) continue;

            ShooterWeapon weapon = AssetDatabase.LoadAssetAtPath<ShooterWeapon>(path);
            if (weapon == null) continue;

            weapons.Add(weapon);
        }

        weapons.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return weapons;
    }

    private static Dictionary<ShooterWeapon, Item> CreateOrLoadWeaponItems(List<ShooterWeapon> weapons)
    {
        Dictionary<ShooterWeapon, Item> map = new Dictionary<ShooterWeapon, Item>(weapons.Count);

        for (int i = 0; i < weapons.Count; i++)
        {
            ShooterWeapon weapon = weapons[i];
            string itemAssetPath = GetItemPathForWeapon(weapon);

            Item item = AssetDatabase.LoadAssetAtPath<Item>(itemAssetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<Item>();
                item.name = BuildItemName(weapon.name);
                AssetDatabase.CreateAsset(item, itemAssetPath);
            }

            map[weapon] = item;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return map;
    }

    private static void ApplyMappings(
        ShooterHolsterHotkey holster,
        List<ShooterWeapon> weapons,
        Dictionary<ShooterWeapon, Item> map
    )
    {
        SerializedObject serialized = new SerializedObject(holster);

        SerializedProperty transferToggle = serialized.FindProperty("transferMappedWeaponsToInventory");
        if (transferToggle != null) transferToggle.boolValue = true;

        SerializedProperty bindings = serialized.FindProperty("weaponItemBindings");
        if (bindings == null)
        {
            Debug.LogError("[ShooterHolsterInventorySetup] weaponItemBindings field was not found on ShooterHolsterHotkey.");
            return;
        }

        bindings.arraySize = weapons.Count;

        for (int i = 0; i < weapons.Count; i++)
        {
            SerializedProperty element = bindings.GetArrayElementAtIndex(i);
            SerializedProperty weaponProp = element.FindPropertyRelative("weapon");
            SerializedProperty itemProp = element.FindPropertyRelative("inventoryItem");

            ShooterWeapon weapon = weapons[i];
            weaponProp.objectReferenceValue = weapon;
            itemProp.objectReferenceValue = map[weapon];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static string BuildItemName(string weaponName)
    {
        string cleaned = weaponName.Replace("_Weapon", string.Empty);
        cleaned = cleaned.Replace(" Weapon", string.Empty);
        return $"{cleaned}_Item";
    }

    private static string GetItemPathForWeapon(ShooterWeapon weapon)
    {
        string itemFileName = BuildItemName(weapon.name) + ".asset";
        return Path.Combine(OutputFolder, itemFileName).Replace('\\', '/');
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif