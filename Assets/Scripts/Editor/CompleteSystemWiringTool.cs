using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CompleteSystemWiringTool : EditorWindow
{
    [MenuItem("Division Game/Complete System Setup/Wire All Systems")]
    public static void WireAllSystems()
    {
        Debug.Log("=== Starting Complete System Wiring ===");
        
        WireGameManagerReferences();
        WireUIManagerReferences();
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("<color=green><b>✓✓✓ Complete System Wiring Finished! ✓✓✓</b></color>");
        Debug.Log("Run 'Division Game → Complete System Setup → Validate All Connections' to verify.");
    }
    
    private static void WireGameManagerReferences()
    {
        Debug.Log("\n--- Wiring GameManager References ---");
        
        GameObject gameSystems = GameObject.Find("GameSystems");
        if (gameSystems == null)
        {
            Debug.LogError("GameSystems not found!");
            return;
        }
        
        GameManager gameManager = gameSystems.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager component not found!");
            return;
        }
        
        SerializedObject gmSO = new SerializedObject(gameManager);
        
        MissionManager missionManager = gameSystems.GetComponent<MissionManager>();
        ProgressionManager progressionManager = gameSystems.GetComponent<ProgressionManager>();
        LootManager lootManager = gameSystems.GetComponent<LootManager>();
        FactionManager factionManager = gameSystems.GetComponent<FactionManager>();
        ChallengeManager challengeManager = gameSystems.GetComponent<ChallengeManager>();
        SkillManager skillManager = gameSystems.GetComponent<SkillManager>();
        HUDManager hudManager = gameSystems.GetComponent<HUDManager>();
        
        if (missionManager != null)
        {
            gmSO.FindProperty("missionManager").objectReferenceValue = missionManager;
            Debug.Log("✓ GameManager.missionManager");
        }
        
        if (progressionManager != null)
        {
            gmSO.FindProperty("progressionManager").objectReferenceValue = progressionManager;
            Debug.Log("✓ GameManager.progressionManager");
        }
        
        if (lootManager != null)
        {
            gmSO.FindProperty("lootManager").objectReferenceValue = lootManager;
            Debug.Log("✓ GameManager.lootManager");
        }
        
        if (factionManager != null)
        {
            gmSO.FindProperty("factionManager").objectReferenceValue = factionManager;
            Debug.Log("✓ GameManager.factionManager");
        }
        
        if (challengeManager != null)
        {
            gmSO.FindProperty("challengeManager").objectReferenceValue = challengeManager;
            Debug.Log("✓ GameManager.challengeManager");
        }
        
        if (skillManager != null)
        {
            gmSO.FindProperty("skillManager").objectReferenceValue = skillManager;
            Debug.Log("✓ GameManager.skillManager");
        }
        
        if (hudManager != null)
        {
            gmSO.FindProperty("hudManager").objectReferenceValue = hudManager;
            Debug.Log("✓ GameManager.hudManager");
        }
        
        gmSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameManager);
    }
    
    private static void WireUIManagerReferences()
    {
        Debug.Log("\n--- Wiring HUDManager → UI Managers ---");
        
        GameObject gameSystems = GameObject.Find("GameSystems");
        if (gameSystems == null) return;
        
        HUDManager hudManager = gameSystems.GetComponent<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogWarning("HUDManager not found!");
            return;
        }
        
        SerializedObject hudSO = new SerializedObject(hudManager);
        
        GameObject missionUI = GameObject.Find("UI/HUD/ScreenSpace/MissionUIManager");
        if (missionUI != null)
        {
            MissionUIManager missionUIManager = missionUI.GetComponent<MissionUIManager>();
            if (missionUIManager != null)
            {
                hudSO.FindProperty("missionUIManager").objectReferenceValue = missionUIManager;
                Debug.Log("✓ HUDManager.missionUIManager");
            }
        }
        
        GameObject progressionUI = GameObject.Find("UI/HUD/ScreenSpace/ProgressionUIManager");
        if (progressionUI != null)
        {
            ProgressionUIManager progressionUIManager = progressionUI.GetComponent<ProgressionUIManager>();
            if (progressionUIManager != null)
            {
                hudSO.FindProperty("progressionUIManager").objectReferenceValue = progressionUIManager;
                Debug.Log("✓ HUDManager.progressionUIManager");
            }
        }
        
        GameObject lootUI = GameObject.Find("UI/HUD/ScreenSpace/LootUIManager");
        if (lootUI != null)
        {
            LootUIManager lootUIManager = lootUI.GetComponent<LootUIManager>();
            if (lootUIManager != null)
            {
                hudSO.FindProperty("lootUIManager").objectReferenceValue = lootUIManager;
                Debug.Log("✓ HUDManager.lootUIManager");
            }
        }
        
        hudSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(hudManager);
    }
    
    [MenuItem("Division Game/Complete System Setup/Validate All Connections")]
    public static void ValidateAllConnections()
    {
        Debug.Log("=== COMPLETE SYSTEM VALIDATION ===\n");
        
        GameObject gameSystems = GameObject.Find("GameSystems");
        if (gameSystems == null)
        {
            Debug.LogError("✗ GameSystems not found!");
            return;
        }
        
        Debug.Log("<b>CORE MANAGERS:</b>");
        ValidateComponent<GameManager>(gameSystems, "GameManager");
        ValidateComponent<MissionManager>(gameSystems, "MissionManager");
        ValidateComponent<ProgressionManager>(gameSystems, "ProgressionManager");
        ValidateComponent<LootManager>(gameSystems, "LootManager");
        ValidateComponent<FactionManager>(gameSystems, "FactionManager");
        ValidateComponent<ChallengeManager>(gameSystems, "ChallengeManager");
        ValidateComponent<SkillManager>(gameSystems, "SkillManager");
        ValidateComponent<HUDManager>(gameSystems, "HUDManager");
        
        Debug.Log("\n<b>GAMEMANAGER REFERENCES:</b>");
        GameManager gm = gameSystems.GetComponent<GameManager>();
        if (gm != null)
        {
            ValidateReference(gm.missionManager, "GameManager.missionManager");
            ValidateReference(gm.progressionManager, "GameManager.progressionManager");
            ValidateReference(gm.lootManager, "GameManager.lootManager");
            ValidateReference(gm.factionManager, "GameManager.factionManager");
            ValidateReference(gm.challengeManager, "GameManager.challengeManager");
            ValidateReference(gm.skillManager, "GameManager.skillManager");
            ValidateReference(gm.hudManager, "GameManager.hudManager");
        }
        
        Debug.Log("\n<b>UI MANAGERS:</b>");
        GameObject missionUI = GameObject.Find("UI/HUD/ScreenSpace/MissionUIManager");
        GameObject progressionUI = GameObject.Find("UI/HUD/ScreenSpace/ProgressionUIManager");
        GameObject lootUI = GameObject.Find("UI/HUD/ScreenSpace/LootUIManager");
        
        ValidateUIGameObject(missionUI, "MissionUIManager", typeof(MissionUIManager));
        ValidateUIGameObject(progressionUI, "ProgressionUIManager", typeof(ProgressionUIManager));
        ValidateUIGameObject(lootUI, "LootUIManager", typeof(LootUIManager));
        
        Debug.Log("\n<b>HUDMANAGER → UI CONNECTIONS:</b>");
        HUDManager hud = gameSystems.GetComponent<HUDManager>();
        if (hud != null)
        {
            SerializedObject hudSO = new SerializedObject(hud);
            ValidateSerializedReference(hudSO, "missionUIManager", "HUDManager.missionUIManager");
            ValidateSerializedReference(hudSO, "progressionUIManager", "HUDManager.progressionUIManager");
            ValidateSerializedReference(hudSO, "lootUIManager", "HUDManager.lootUIManager");
        }
        
        Debug.Log("\n=== VALIDATION COMPLETE ===");
    }
    
    private static void ValidateComponent<T>(GameObject go, string name) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component != null)
        {
            Debug.Log($"✓ {name}");
        }
        else
        {
            Debug.LogWarning($"✗ {name} MISSING");
        }
    }
    
    private static void ValidateReference(Object reference, string name)
    {
        if (reference != null)
        {
            Debug.Log($"✓ {name}");
        }
        else
        {
            Debug.LogWarning($"✗ {name} NOT SET");
        }
    }
    
    private static void ValidateUIGameObject(GameObject go, string name, System.Type componentType)
    {
        if (go != null)
        {
            Component comp = go.GetComponent(componentType);
            if (comp != null)
            {
                Debug.Log($"✓ {name} GameObject & Component");
            }
            else
            {
                Debug.LogWarning($"✗ {name} GameObject exists but component missing!");
            }
        }
        else
        {
            Debug.LogWarning($"✗ {name} GameObject NOT FOUND");
        }
    }
    
    private static void ValidateSerializedReference(SerializedObject so, string propertyName, string displayName)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null && prop.objectReferenceValue != null)
        {
            Debug.Log($"✓ {displayName}");
        }
        else
        {
            Debug.LogWarning($"✗ {displayName} NOT CONNECTED");
        }
    }
}
