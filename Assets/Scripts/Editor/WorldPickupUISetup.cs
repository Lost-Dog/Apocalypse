using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class WorldPickupUISetup : EditorWindow
{
    private GameObject targetPickupObject;
    private GameObject pickupUIPrefab;
    
    [MenuItem("Division Game/World Pickup/Setup Pickup UI")]
    public static void ShowWindow()
    {
        WorldPickupUISetup window = GetWindow<WorldPickupUISetup>("Pickup UI Setup");
        window.minSize = new Vector2(400, 300);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("World Pickup UI Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool helps you add pickup UI (HUD_Apocalypse_ItemPickupInfo_02) to world items.\n\n" +
            "1. Select a pickup object in the scene (or create one)\n" +
            "2. Click 'Add Pickup UI' to attach the UI prefab\n" +
            "3. The UI will automatically follow the object and face the camera",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        targetPickupObject = EditorGUILayout.ObjectField(
            "Target Pickup Object",
            targetPickupObject,
            typeof(GameObject),
            true) as GameObject;
        
        EditorGUILayout.Space(5);
        
        pickupUIPrefab = EditorGUILayout.ObjectField(
            "UI Prefab (Optional)",
            pickupUIPrefab,
            typeof(GameObject),
            false) as GameObject;
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(targetPickupObject == null);
        
        if (GUILayout.Button("Add Pickup UI to Selected Object", GUILayout.Height(40)))
        {
            AddPickupUI();
        }
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Find HUD_Apocalypse_ItemPickupInfo_02 Prefab", GUILayout.Height(30)))
        {
            FindPickupUIPrefab();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Create Sample Bread Consumable", GUILayout.Height(30)))
        {
            CreateSampleBreadConsumable();
        }
    }
    
    private void AddPickupUI()
    {
        if (targetPickupObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target pickup object first!", "OK");
            return;
        }
        
        if (pickupUIPrefab == null)
        {
            FindPickupUIPrefab();
        }
        
        if (pickupUIPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find HUD_Apocalypse_ItemPickupInfo_02 prefab!\n\n" +
                "Please manually assign the UI prefab from:\n" +
                "Assets/Synty/InterfaceApocalypseHUD/Prefabs/Input_Interactions/", 
                "OK");
            return;
        }
        
        // Instantiate the UI prefab as a child
        GameObject uiInstance = PrefabUtility.InstantiatePrefab(pickupUIPrefab) as GameObject;
        uiInstance.transform.SetParent(targetPickupObject.transform);
        uiInstance.transform.localPosition = new Vector3(0, 2f, 0);
        uiInstance.transform.localRotation = Quaternion.identity;
        uiInstance.transform.localScale = Vector3.one * 0.005f; // Smaller scale for world space
        
        // Set up Canvas for world space
        Canvas canvas = uiInstance.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiInstance.AddComponent<Canvas>();
        }
        
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Add CanvasScaler for proper scaling
        var scaler = uiInstance.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null)
        {
            scaler = uiInstance.AddComponent<UnityEngine.UI.CanvasScaler>();
        }
        scaler.dynamicPixelsPerUnit = 10;
        
        // Add GraphicRaycaster if needed
        var raycaster = uiInstance.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = uiInstance.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(600, 200);
        }
        
        // Add WorldPickupUI component
        WorldPickupUI pickupUI = targetPickupObject.GetComponent<WorldPickupUI>();
        if (pickupUI == null)
        {
            pickupUI = targetPickupObject.AddComponent<WorldPickupUI>();
        }
        
        pickupUI.pickupInfoPanel = uiInstance;
        pickupUI.autoFindComponents = true;
        pickupUI.uiOffset = new Vector3(0, 2f, 0);
        
        EditorUtility.SetDirty(targetPickupObject);
        
        Selection.activeGameObject = targetPickupObject;
        
        Debug.Log($"<color=green>✓ Added pickup UI to {targetPickupObject.name}!</color>");
        EditorUtility.DisplayDialog("Success", 
            $"Pickup UI added to {targetPickupObject.name}!\n\n" +
            "The UI will automatically:\n" +
            "• Face the camera\n" +
            "• Show/hide based on player distance\n" +
            "• Display item name and interaction prompt\n\n" +
            "Configured for World Space rendering at proper scale.",
            "OK");
    }
    
    private void FindPickupUIPrefab()
    {
        string[] guids = AssetDatabase.FindAssets("HUD_Apocalypse_ItemPickupInfo_02 t:Prefab");
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            pickupUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.Log($"<color=cyan>Found UI prefab at: {path}</color>");
        }
        else
        {
            Debug.LogWarning("Could not find HUD_Apocalypse_ItemPickupInfo_02 prefab!");
        }
    }
    
    private void CreateSampleBreadConsumable()
    {
        // Create Resources/Consumables folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Consumables"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Consumables");
        }
        
        // Create the consumable asset
        ConsumableItem bread = ScriptableObject.CreateInstance<ConsumableItem>();
        bread.itemID = "consumable_bread_001";
        bread.itemName = "Bread";
        bread.rarity = LootManager.Rarity.Common;
        bread.itemType = LootItemData.ItemType.Consumable;
        bread.description = "A loaf of bread. Restores hunger and provides warmth.";
        
        // Consumable properties
        bread.healthRestore = 10f;
        bread.temperatureChange = 5f;
        bread.staminaRestore = 15f;
        bread.effectDuration = 0f; // Instant
        bread.isStackable = true;
        bread.maxStackSize = 10;
        
        string path = "Assets/Resources/Consumables/Bread.asset";
        AssetDatabase.CreateAsset(bread, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Selection.activeObject = bread;
        EditorGUIUtility.PingObject(bread);
        
        Debug.Log($"<color=green>✓ Created Bread consumable at: {path}</color>");
        EditorUtility.DisplayDialog("Success", 
            $"Created Bread consumable!\n\n" +
            "Properties:\n" +
            "• Health: +10\n" +
            "• Temperature: +5\n" +
            "• Stamina: +15\n" +
            "• Stackable: Yes (max 10)",
            "OK");
    }
}
