using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ControlPointAutoSetup
{
    private const string SETUP_COMPLETE_KEY = "ControlPointSetup_Complete";
    
    static ControlPointAutoSetup()
    {
        EditorApplication.delayCall += CheckAndSetupControlPoints;
    }
    
    private static void CheckAndSetupControlPoints()
    {
        if (Application.isPlaying)
            return;
            
        if (EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            return;
        
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "Apocalypse")
            return;
        
        GameObject zonesParent = GameObject.Find("GameSystems/Zones/ControlPointZones");
        if (zonesParent == null)
            return;
        
        bool needsSetup = false;
        foreach (Transform child in zonesParent.transform)
        {
            if (child.name.StartsWith("ControlPoint") && child.gameObject.activeSelf)
            {
                needsSetup = true;
                break;
            }
        }
        
        if (!needsSetup)
        {
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
            return;
        }
        
        bool confirm = EditorUtility.DisplayDialog(
            "Control Point Setup Required",
            "Detected active Control Points in the scene that will spawn 25 enemies at scene start.\n\n" +
            "Would you like to automatically configure them for manager-controlled spawning?\n\n" +
            "This will:\n" +
            "• Set all ControlZones to requiresManagerActivation=true\n" +
            "• Disable all ControlPoint parent GameObjects\n" +
            "• Zones will only spawn when ChallengeManager activates them\n\n" +
            "You can manually configure this later via:\n" +
            "Tools > Control Points > ...",
            "Auto-Setup Now",
            "Skip (Don't ask again)"
        );
        
        if (confirm)
        {
            SetupControlPoints();
        }
        
        EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
    }
    
    [MenuItem("Tools/Control Points/Run Auto-Setup")]
    public static void RunAutoSetup()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Cannot Setup In Play Mode", 
                "Please exit Play mode before running auto-setup.", 
                "OK");
            return;
        }
        
        SetupControlPoints();
    }
    
    [MenuItem("Tools/Control Points/Reset Setup Flag")]
    public static void ResetSetupFlag()
    {
        EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
        Debug.Log("<color=cyan>Control Point setup flag reset. Auto-setup will run again on next scene load.</color>");
    }
    
    private static void SetupControlPoints()
    {
        int zonesConfigured = 0;
        int controlPointsDisabled = 0;
        
        ControlZone[] allZones = Object.FindObjectsByType<ControlZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ControlZone zone in allZones)
        {
            SerializedObject so = new SerializedObject(zone);
            so.FindProperty("spawnOnStart").boolValue = false;
            so.FindProperty("requiresManagerActivation").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(zone);
            zonesConfigured++;
        }
        
        GameObject zonesParent = GameObject.Find("GameSystems/Zones/ControlPointZones");
        if (zonesParent != null)
        {
            foreach (Transform child in zonesParent.transform)
            {
                if (child.name.StartsWith("ControlPoint"))
                {
                    Undo.RecordObject(child.gameObject, "Disable Control Point");
                    child.gameObject.SetActive(false);
                    controlPointsDisabled++;
                }
            }
        }
        
        if (zonesConfigured > 0 || controlPointsDisabled > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        
        string message = $"<color=green>✓ Control Point Setup Complete!</color>\n" +
            $"• Configured {zonesConfigured} ControlZones for manager control\n" +
            $"• Disabled {controlPointsDisabled} ControlPoint GameObjects\n" +
            $"• Zones will now only spawn when ChallengeManager activates them";
        
        Debug.Log(message);
        
        EditorUtility.DisplayDialog("Setup Complete", 
            $"Control Point Setup Complete!\n\n" +
            $"✓ Configured {zonesConfigured} ControlZones\n" +
            $"✓ Disabled {controlPointsDisabled} ControlPoint GameObjects\n\n" +
            "Zones will now only spawn when ChallengeManager activates them.\n\n" +
            "Test by entering Play mode - no enemies should spawn at start!",
            "OK");
    }
}
