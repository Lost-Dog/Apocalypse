using UnityEngine;
using UnityEditor;
using JUTPS;

public class PlayerIntegrationTool : EditorWindow
{
    [MenuItem("Division Game/Player Integration/Setup Player Bridge")]
    public static void SetupPlayerBridge()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("No GameObject with 'Player' tag found!");
            return;
        }
        
        PlayerSystemBridge bridge = player.GetComponent<PlayerSystemBridge>();
        
        if (bridge == null)
        {
            bridge = player.AddComponent<PlayerSystemBridge>();
            Debug.Log($"✓ Added PlayerSystemBridge to {player.name}");
        }
        else
        {
            Debug.Log($"PlayerSystemBridge already exists on {player.name}");
        }
        
        SerializedObject bridgeSO = new SerializedObject(bridge);
        
        JUHealth health = player.GetComponent<JUHealth>();
        if (health != null)
        {
            bridgeSO.FindProperty("jutpsHealth").objectReferenceValue = health;
            Debug.Log("✓ Connected JUHealth");
        }
        
        JUCharacterController controller = player.GetComponent<JUCharacterController>();
        if (controller != null)
        {
            bridgeSO.FindProperty("jutpsController").objectReferenceValue = controller;
            Debug.Log("✓ Connected JUCharacterController");
        }
        
        bridgeSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(bridge);
        
        Debug.Log("<color=green><b>✓ Player bridge setup complete!</b></color>");
    }
    
    [MenuItem("Division Game/Player Integration/Add Enemy Reward Handlers")]
    public static void AddEnemyRewardHandlers()
    {
        JUHealth[] allHealthComponents = FindObjectsByType<JUHealth>(FindObjectsSortMode.None);
        
        int addedCount = 0;
        int skippedCount = 0;
        
        foreach (JUHealth health in allHealthComponents)
        {
            if (health.CompareTag("Player"))
            {
                continue;
            }
            
            if (health.GetComponent<EnemyKillRewardHandler>() != null)
            {
                skippedCount++;
                continue;
            }
            
            health.gameObject.AddComponent<EnemyKillRewardHandler>();
            addedCount++;
        }
        
        Debug.Log($"<color=green>✓ Added EnemyKillRewardHandler to {addedCount} enemies</color>");
        
        if (skippedCount > 0)
        {
            Debug.Log($"Skipped {skippedCount} enemies (already had component)");
        }
        
        if (addedCount == 0 && skippedCount == 0)
        {
            Debug.LogWarning("No enemies found with JUHealth component (excluding player)");
        }
    }
    
    [MenuItem("Division Game/Player Integration/Validate Player Setup")]
    public static void ValidatePlayerSetup()
    {
        Debug.Log("=== PLAYER INTEGRATION VALIDATION ===\n");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("✗ No GameObject with 'Player' tag found!");
            return;
        }
        
        Debug.Log($"✓ Player GameObject: {player.name}");
        
        JUHealth health = player.GetComponent<JUHealth>();
        if (health != null)
        {
            Debug.Log($"✓ JUHealth (HP: {health.Health}/{health.MaxHealth})");
        }
        else
        {
            Debug.LogWarning("✗ JUHealth not found");
        }
        
        JUCharacterController controller = player.GetComponent<JUCharacterController>();
        if (controller != null)
        {
            Debug.Log("✓ JUCharacterController");
        }
        else
        {
            Debug.LogWarning("✗ JUCharacterController not found");
        }
        
        PlayerSystemBridge bridge = player.GetComponent<PlayerSystemBridge>();
        if (bridge != null)
        {
            Debug.Log("✓ PlayerSystemBridge");
            
            SerializedObject bridgeSO = new SerializedObject(bridge);
            
            if (bridgeSO.FindProperty("jutpsHealth").objectReferenceValue != null)
            {
                Debug.Log("  ✓ JUHealth reference connected");
            }
            else
            {
                Debug.LogWarning("  ✗ JUHealth reference not set");
            }
            
            if (bridgeSO.FindProperty("jutpsController").objectReferenceValue != null)
            {
                Debug.Log("  ✓ JUCharacterController reference connected");
            }
            else
            {
                Debug.LogWarning("  ✗ JUCharacterController reference not set");
            }
        }
        else
        {
            Debug.LogWarning("✗ PlayerSystemBridge not found - Run 'Setup Player Bridge'");
        }
        
        if (GameManager.Instance != null)
        {
            Debug.Log("\n✓ GameManager.Instance exists");
            
            if (GameManager.Instance.progressionManager != null)
            {
                Debug.Log("✓ ProgressionManager available");
            }
            else
            {
                Debug.LogWarning("✗ ProgressionManager not set in GameManager");
            }
            
            if (GameManager.Instance.lootManager != null)
            {
                Debug.Log("✓ LootManager available");
            }
            else
            {
                Debug.LogWarning("✗ LootManager not set in GameManager");
            }
            
            if (GameManager.Instance.hudManager != null)
            {
                Debug.Log("✓ HUDManager available");
            }
            else
            {
                Debug.LogWarning("✗ HUDManager not set in GameManager");
            }
        }
        else
        {
            Debug.LogWarning("✗ GameManager.Instance not found - Enter Play Mode or check scene setup");
        }
        
        Debug.Log("\n=== VALIDATION COMPLETE ===");
    }
    
    [MenuItem("Division Game/Player Integration/Test Player Systems")]
    public static void TestPlayerSystems()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("This test requires Play Mode. Entering Play Mode...");
            EditorApplication.isPlaying = true;
            return;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        
        PlayerSystemBridge bridge = player.GetComponent<PlayerSystemBridge>();
        if (bridge == null)
        {
            Debug.LogError("PlayerSystemBridge not found on player!");
            return;
        }
        
        Debug.Log("=== TESTING PLAYER SYSTEMS ===");
        
        Debug.Log("Test 1: Awarding 100 XP...");
        bridge.GainExperience(100);
        
        Debug.Log("Test 2: Healing player to full...");
        bridge.HealToFull();
        
        Debug.Log("Test 3: Getting player stats...");
        Debug.Log($"  Player Level: {bridge.GetPlayerLevel()}");
        Debug.Log($"  Health: {bridge.GetHealthPercentage() * 100}%");
        Debug.Log($"  Is Alive: {bridge.IsAlive()}");
        
        Debug.Log("\n=== TEST COMPLETE ===");
        Debug.Log("Check UI for XP bar updates!");
    }
}
