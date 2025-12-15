using UnityEngine;
using UnityEditor;

public class UIManagerWiringTool : EditorWindow
{
    private const string GAME_SYSTEMS_PATH = "GameSystems";
    private const string MISSION_UI_PATH = "UI/HUD/ScreenSpace/MissionUIManager";
    private const string PROGRESSION_UI_PATH = "UI/HUD/ScreenSpace/ProgressionUIManager";
    private const string LOOT_UI_PATH = "UI/HUD/ScreenSpace/LootUIManager";
    
    [MenuItem("Division Game/UI Wiring/Auto-Wire UI Managers")]
    public static void AutoWireUIManagers()
    {
        GameObject gameSystems = GameObject.Find(GAME_SYSTEMS_PATH);
        if (gameSystems == null)
        {
            Debug.LogError($"Could not find GameObject: {GAME_SYSTEMS_PATH}");
            return;
        }
        
        HUDManager hudManager = gameSystems.GetComponent<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogError("HUDManager component not found on GameSystems!");
            return;
        }
        
        SerializedObject hudManagerSO = new SerializedObject(hudManager);
        
        bool changesMade = false;
        
        GameObject missionUI = GameObject.Find(MISSION_UI_PATH);
        if (missionUI != null)
        {
            MissionUIManager missionUIManager = missionUI.GetComponent<MissionUIManager>();
            if (missionUIManager != null)
            {
                SerializedProperty missionUIProp = hudManagerSO.FindProperty("missionUIManager");
                missionUIProp.objectReferenceValue = missionUIManager;
                Debug.Log($"✓ Connected MissionUIManager");
                changesMade = true;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find: {MISSION_UI_PATH}");
        }
        
        GameObject progressionUI = GameObject.Find(PROGRESSION_UI_PATH);
        if (progressionUI != null)
        {
            ProgressionUIManager progressionUIManager = progressionUI.GetComponent<ProgressionUIManager>();
            if (progressionUIManager != null)
            {
                SerializedProperty progressionUIProp = hudManagerSO.FindProperty("progressionUIManager");
                progressionUIProp.objectReferenceValue = progressionUIManager;
                Debug.Log($"✓ Connected ProgressionUIManager");
                changesMade = true;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find: {PROGRESSION_UI_PATH}");
        }
        
        GameObject lootUI = GameObject.Find(LOOT_UI_PATH);
        if (lootUI != null)
        {
            LootUIManager lootUIManager = lootUI.GetComponent<LootUIManager>();
            if (lootUIManager != null)
            {
                SerializedProperty lootUIProp = hudManagerSO.FindProperty("lootUIManager");
                lootUIProp.objectReferenceValue = lootUIManager;
                Debug.Log($"✓ Connected LootUIManager");
                changesMade = true;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find: {LOOT_UI_PATH}");
        }
        
        if (changesMade)
        {
            hudManagerSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(hudManager);
            Debug.Log("<color=green><b>✓ UI Managers wired successfully!</b></color>");
        }
        else
        {
            Debug.LogWarning("No changes were made. Check warnings above.");
        }
    }
    
    [MenuItem("Division Game/UI Wiring/Validate UI Connections")]
    public static void ValidateUIConnections()
    {
        Debug.Log("=== UI Connection Validation ===");
        
        GameObject gameSystems = GameObject.Find(GAME_SYSTEMS_PATH);
        if (gameSystems == null)
        {
            Debug.LogError($"✗ GameSystems not found");
            return;
        }
        Debug.Log($"✓ GameSystems found");
        
        HUDManager hudManager = gameSystems.GetComponent<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogError("✗ HUDManager component missing");
            return;
        }
        Debug.Log($"✓ HUDManager component exists");
        
        SerializedObject hudManagerSO = new SerializedObject(hudManager);
        
        SerializedProperty missionUIProp = hudManagerSO.FindProperty("missionUIManager");
        if (missionUIProp.objectReferenceValue != null)
        {
            Debug.Log($"✓ MissionUIManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ MissionUIManager not connected");
        }
        
        SerializedProperty progressionUIProp = hudManagerSO.FindProperty("progressionUIManager");
        if (progressionUIProp.objectReferenceValue != null)
        {
            Debug.Log($"✓ ProgressionUIManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ ProgressionUIManager not connected");
        }
        
        SerializedProperty lootUIProp = hudManagerSO.FindProperty("lootUIManager");
        if (lootUIProp.objectReferenceValue != null)
        {
            Debug.Log($"✓ LootUIManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ LootUIManager not connected");
        }
        
        GameManager gameManager = gameSystems.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("✗ GameManager component missing");
            return;
        }
        Debug.Log($"✓ GameManager component exists");
        
        if (gameManager.missionManager != null)
        {
            Debug.Log($"✓ GameManager.missionManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ GameManager.missionManager not connected");
        }
        
        if (gameManager.progressionManager != null)
        {
            Debug.Log($"✓ GameManager.progressionManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ GameManager.progressionManager not connected");
        }
        
        if (gameManager.lootManager != null)
        {
            Debug.Log($"✓ GameManager.lootManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ GameManager.lootManager not connected");
        }
        
        if (gameManager.hudManager != null)
        {
            Debug.Log($"✓ GameManager.hudManager connected");
        }
        else
        {
            Debug.LogWarning($"✗ GameManager.hudManager not connected");
        }
        
        Debug.Log("=== Validation Complete ===");
    }
    
    [MenuItem("Division Game/UI Wiring/Test UI System")]
    public static void TestUISystem()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("This test requires Play Mode. Entering Play Mode...");
            EditorApplication.isPlaying = true;
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Make sure the game has initialized.");
            return;
        }
        
        Debug.Log("=== Testing UI System ===");
        
        if (GameManager.Instance.progressionManager != null)
        {
            Debug.Log("Testing XP gain...");
            GameManager.Instance.progressionManager.AddExperience(50);
        }
        
        if (GameManager.Instance.lootManager != null)
        {
            Debug.Log("Testing loot drop...");
            GameManager.Instance.lootManager.DropLoot(Vector3.zero, 1);
        }
        
        Debug.Log("Check the UI to see if notifications appeared!");
    }
}
