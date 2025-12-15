using UnityEngine;
using UnityEditor;
using JUTPS;
using JUTPS.InventorySystem;

public class PlayerSetupHelper : EditorWindow
{
    private GameObject targetPlayer;
    private GameObject referencePlayer;
    private bool autoFindReference = true;
    
    private Vector2 scrollPosition;
    
    private bool analyzed = false;
    private bool hasSurvivalManager;
    private bool hasPlayerSystemBridge;
    private bool hasExtendedPickupSystem;
    private bool hasJUTPSInventoryBridge;
    private bool hasJUCharacterController;
    private bool hasJUHealth;
    private bool hasJUInventory;
    
    [MenuItem("Tools/Player/Setup Helper")]
    public static void ShowWindow()
    {
        PlayerSetupHelper window = GetWindow<PlayerSetupHelper>("Player Setup Helper");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Player Setup Helper", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Automatically configure your player with all required survival system components.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Target Player", EditorStyles.boldLabel);
        GameObject newTarget = EditorGUILayout.ObjectField("Player GameObject", targetPlayer, typeof(GameObject), true) as GameObject;
        
        if (newTarget != targetPlayer)
        {
            targetPlayer = newTarget;
            analyzed = false;
        }
        
        if (targetPlayer == null && Selection.activeGameObject != null)
        {
            if (Selection.activeGameObject.GetComponent<JUCharacterController>() != null)
            {
                EditorGUILayout.HelpBox($"Detected player in selection: {Selection.activeGameObject.name}", MessageType.None);
                if (GUILayout.Button("Use Selected GameObject"))
                {
                    targetPlayer = Selection.activeGameObject;
                    analyzed = false;
                }
            }
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Reference Player", EditorStyles.boldLabel);
        autoFindReference = EditorGUILayout.Toggle("Auto-Find Reference", autoFindReference);
        
        if (!autoFindReference)
        {
            referencePlayer = EditorGUILayout.ObjectField("Reference Player", referencePlayer, typeof(GameObject), true) as GameObject;
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Reference Player", referencePlayer, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
        }
        
        EditorGUILayout.Space(10);
        
        if (targetPlayer != null)
        {
            EditorGUI.BeginDisabledGroup(analyzed);
            if (GUILayout.Button("Analyze Player Configuration", GUILayout.Height(30)))
            {
                AnalyzePlayer();
            }
            EditorGUI.EndDisabledGroup();
            
            if (analyzed)
            {
                EditorGUILayout.Space(10);
                DisplayAnalysisResults();
                
                EditorGUILayout.Space(10);
                
                bool hasIssues = !hasSurvivalManager || !hasPlayerSystemBridge || !hasExtendedPickupSystem || !hasJUTPSInventoryBridge;
                
                if (hasIssues)
                {
                    if (GUILayout.Button("Fix All Issues Automatically", GUILayout.Height(40)))
                    {
                        FixAllIssues();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ All required components are present!", MessageType.Info);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select a player GameObject with JUCharacterController component.", MessageType.Warning);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void AnalyzePlayer()
    {
        if (targetPlayer == null) return;
        
        hasJUCharacterController = targetPlayer.GetComponent<JUCharacterController>() != null;
        hasJUHealth = targetPlayer.GetComponent<JUHealth>() != null;
        hasJUInventory = targetPlayer.GetComponent<JUInventory>() != null;
        hasSurvivalManager = targetPlayer.GetComponent<SurvivalManager>() != null;
        hasPlayerSystemBridge = targetPlayer.GetComponent<PlayerSystemBridge>() != null;
        hasExtendedPickupSystem = targetPlayer.GetComponent<ExtendedPickupSystem>() != null;
        hasJUTPSInventoryBridge = targetPlayer.GetComponent<JUTPSInventoryBridge>() != null;
        
        if (autoFindReference)
        {
            FindReferencePlayer();
        }
        
        analyzed = true;
    }
    
    private void FindReferencePlayer()
    {
        SurvivalManager[] allSurvivalManagers = FindObjectsByType<SurvivalManager>(FindObjectsSortMode.None);
        
        foreach (var sm in allSurvivalManagers)
        {
            if (sm.gameObject != targetPlayer)
            {
                referencePlayer = sm.gameObject;
                Debug.Log($"Found reference player: {referencePlayer.name}");
                return;
            }
        }
        
        referencePlayer = null;
    }
    
    private void DisplayAnalysisResults()
    {
        EditorGUILayout.LabelField("Analysis Results", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Base Components:", EditorStyles.miniBoldLabel);
        DisplayCheckmark("JUCharacterController", hasJUCharacterController, true);
        DisplayCheckmark("JUHealth", hasJUHealth, true);
        DisplayCheckmark("JUInventory", hasJUInventory, true);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Required Survival Components:", EditorStyles.miniBoldLabel);
        DisplayCheckmark("SurvivalManager", hasSurvivalManager, false);
        DisplayCheckmark("PlayerSystemBridge", hasPlayerSystemBridge, false);
        DisplayCheckmark("ExtendedPickupSystem", hasExtendedPickupSystem, false);
        DisplayCheckmark("JUTPSInventoryBridge", hasJUTPSInventoryBridge, false);
        
        if (referencePlayer != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox($"Reference player found: {referencePlayer.name}", MessageType.None);
        }
    }
    
    private void DisplayCheckmark(string componentName, bool hasComponent, bool isBaseComponent)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        
        if (hasComponent)
        {
            style.normal.textColor = Color.green;
            EditorGUILayout.LabelField($"✓ {componentName}", style);
        }
        else
        {
            if (isBaseComponent)
            {
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField($"✗ {componentName} (Required base component!)", style);
            }
            else
            {
                style.normal.textColor = new Color(1f, 0.5f, 0f);
                EditorGUILayout.LabelField($"⚠ {componentName} (Missing)", style);
            }
        }
    }
    
    private void FixAllIssues()
    {
        if (targetPlayer == null) return;
        
        if (!hasJUCharacterController)
        {
            Debug.LogError("Cannot setup player without JUCharacterController component!");
            return;
        }
        
        Undo.RegisterCompleteObjectUndo(targetPlayer, "Fix Player Setup");
        
        bool changesMade = false;
        
        if (!hasSurvivalManager)
        {
            SurvivalManager sm = targetPlayer.AddComponent<SurvivalManager>();
            ConfigureSurvivalManager(sm);
            changesMade = true;
            Debug.Log("✓ Added SurvivalManager");
        }
        
        if (!hasPlayerSystemBridge)
        {
            PlayerSystemBridge bridge = targetPlayer.AddComponent<PlayerSystemBridge>();
            ConfigurePlayerSystemBridge(bridge);
            changesMade = true;
            Debug.Log("✓ Added PlayerSystemBridge");
        }
        
        if (!hasExtendedPickupSystem)
        {
            ExtendedPickupSystem pickup = targetPlayer.AddComponent<ExtendedPickupSystem>();
            ConfigureExtendedPickupSystem(pickup);
            changesMade = true;
            Debug.Log("✓ Added ExtendedPickupSystem");
        }
        
        if (!hasJUTPSInventoryBridge)
        {
            JUTPSInventoryBridge invBridge = targetPlayer.AddComponent<JUTPSInventoryBridge>();
            ConfigureJUTPSInventoryBridge(invBridge);
            changesMade = true;
            Debug.Log("✓ Added JUTPSInventoryBridge");
        }
        
        if (changesMade)
        {
            EditorUtility.SetDirty(targetPlayer);
            Debug.Log($"<color=green>Player setup completed for {targetPlayer.name}!</color>");
            AnalyzePlayer();
        }
    }
    
    private void ConfigureSurvivalManager(SurvivalManager sm)
    {
        SerializedObject so = new SerializedObject(sm);
        
        JUCharacterController controller = targetPlayer.GetComponent<JUCharacterController>();
        JUHealth health = targetPlayer.GetComponent<JUHealth>();
        
        so.FindProperty("playerController").objectReferenceValue = controller;
        so.FindProperty("playerHealth").objectReferenceValue = health;
        
        ProgressionManager progression = FindFirstObjectByType<ProgressionManager>();
        if (progression != null)
        {
            so.FindProperty("progressionManager").objectReferenceValue = progression;
        }
        
        so.FindProperty("currentTemperature").floatValue = 100f;
        so.FindProperty("maxTemperature").floatValue = 100f;
        so.FindProperty("currentStamina").floatValue = 100f;
        so.FindProperty("maxStamina").floatValue = 100f;
        so.FindProperty("currentInfection").floatValue = 0f;
        
        if (referencePlayer != null)
        {
            SurvivalManager refSM = referencePlayer.GetComponent<SurvivalManager>();
            if (refSM != null)
            {
                CopySerializedProperties(new SerializedObject(refSM), so);
            }
        }
        
        so.ApplyModifiedProperties();
    }
    
    private void ConfigurePlayerSystemBridge(PlayerSystemBridge bridge)
    {
        SerializedObject so = new SerializedObject(bridge);
        
        JUHealth health = targetPlayer.GetComponent<JUHealth>();
        JUCharacterController controller = targetPlayer.GetComponent<JUCharacterController>();
        
        so.FindProperty("jutpsHealth").objectReferenceValue = health;
        so.FindProperty("jutpsController").objectReferenceValue = controller;
        so.FindProperty("playerLevel").intValue = 1;
        so.FindProperty("xpPerKill").intValue = 50;
        so.FindProperty("lootDropChance").floatValue = 0.5f;
        
        if (referencePlayer != null)
        {
            PlayerSystemBridge refBridge = referencePlayer.GetComponent<PlayerSystemBridge>();
            if (refBridge != null)
            {
                CopySerializedProperties(new SerializedObject(refBridge), so);
            }
        }
        
        so.ApplyModifiedProperties();
    }
    
    private void ConfigureExtendedPickupSystem(ExtendedPickupSystem pickup)
    {
        SerializedObject so = new SerializedObject(pickup);
        
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer != -1)
        {
            so.FindProperty("pickupLayer").intValue = itemLayer;
        }
        
        so.FindProperty("pickupRadius").floatValue = 2.0f;
        so.FindProperty("holdTimeToPickup").floatValue = 0.1f;
        so.FindProperty("autoPickup").boolValue = false;
        so.FindProperty("showPickupPrompts").boolValue = true;
        
        if (referencePlayer != null)
        {
            ExtendedPickupSystem refPickup = referencePlayer.GetComponent<ExtendedPickupSystem>();
            if (refPickup != null)
            {
                CopySerializedProperties(new SerializedObject(refPickup), so);
            }
        }
        
        so.ApplyModifiedProperties();
    }
    
    private void ConfigureJUTPSInventoryBridge(JUTPSInventoryBridge bridge)
    {
        SerializedObject so = new SerializedObject(bridge);
        
        JUInventory inventory = targetPlayer.GetComponent<JUInventory>();
        so.FindProperty("jutpsInventory").objectReferenceValue = inventory;
        so.FindProperty("autoEquipWeapons").boolValue = true;
        so.FindProperty("useNameMapping").boolValue = true;
        
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer != -1 && inventory != null)
        {
            SerializedObject inventorySO = new SerializedObject(inventory);
            inventorySO.FindProperty("ItemLayer").intValue = itemLayer;
            inventorySO.ApplyModifiedProperties();
        }
        
        if (referencePlayer != null)
        {
            JUTPSInventoryBridge refBridge = referencePlayer.GetComponent<JUTPSInventoryBridge>();
            if (refBridge != null)
            {
                CopySerializedProperties(new SerializedObject(refBridge), so);
            }
        }
        
        so.ApplyModifiedProperties();
    }
    
    private void CopySerializedProperties(SerializedObject source, SerializedObject target)
    {
        SerializedProperty prop = source.GetIterator();
        bool enterChildren = true;
        
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            if (prop.name == "m_Script") continue;
            
            SerializedProperty targetProp = target.FindProperty(prop.name);
            if (targetProp != null && targetProp.propertyType == prop.propertyType)
            {
                target.CopyFromSerializedProperty(prop);
            }
        }
    }
    
    [MenuItem("Tools/Player/Fix Survival Manager Reference")]
    public static void FixSurvivalManagerReference()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject with 'Player' tag found!");
            return;
        }
        
        SurvivalManager[] allManagers = FindObjectsByType<SurvivalManager>(FindObjectsSortMode.None);
        if (allManagers.Length == 0)
        {
            Debug.LogError("No SurvivalManager found in scene!");
            return;
        }
        
        SurvivalManager activeSM = SurvivalManager.Instance;
        if (activeSM == null)
        {
            Debug.LogError("SurvivalManager singleton instance is null!");
            return;
        }
        
        JUCharacterController controller = player.GetComponent<JUCharacterController>();
        JUHealth health = player.GetComponent<JUHealth>();
        
        if (controller == null || health == null)
        {
            Debug.LogError("Player missing JUCharacterController or JUHealth!");
            return;
        }
        
        SerializedObject so = new SerializedObject(activeSM);
        so.FindProperty("playerController").objectReferenceValue = controller;
        so.FindProperty("playerHealth").objectReferenceValue = health;
        so.ApplyModifiedProperties();
        
        Debug.Log($"<color=green>✓ Updated SurvivalManager to reference player: {player.name}</color>");
    }
}
