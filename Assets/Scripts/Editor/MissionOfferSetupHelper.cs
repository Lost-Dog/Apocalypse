using UnityEngine;
using UnityEditor;

public class MissionOfferSetupHelper : EditorWindow
{
    private const string BASE_NAME = "PlayerBase";
    private const float BASE_RADIUS = 50f;
    
    [MenuItem("Division Game/Setup/Configure Mission Offer System")]
    public static void ShowWindow()
    {
        GetWindow<MissionOfferSetupHelper>("Mission Offer Setup");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Mission Offer System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool will set up the Mission Offer System:\n" +
            "1. Ensure MissionOfferManager component is added\n" +
            "2. Create a PlayerBase location (if needed)\n" +
            "3. Wire up all references", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Mission Offer System", GUILayout.Height(40)))
        {
            SetupMissionOfferSystem();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Base Location at Player Position", GUILayout.Height(30)))
        {
            CreateBaseAtPlayerPosition();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Configuration", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Select MissionOfferManager"))
        {
            GameObject missionOfferGO = GameObject.Find("MissionOffermanager");
            if (missionOfferGO != null)
            {
                Selection.activeGameObject = missionOfferGO;
                EditorGUIUtility.PingObject(missionOfferGO);
            }
        }
        
        if (GUILayout.Button("Select Player Base"))
        {
            GameObject baseGO = GameObject.Find(BASE_NAME);
            if (baseGO != null)
            {
                Selection.activeGameObject = baseGO;
                EditorGUIUtility.PingObject(baseGO);
            }
        }
    }
    
    private void SetupMissionOfferSystem()
    {
        GameObject missionOfferGO = GameObject.Find("MissionOffermanager");
        
        if (missionOfferGO == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'MissionOffermanager' GameObject in scene!", "OK");
            return;
        }
        
        MissionOfferManager offerManager = missionOfferGO.GetComponent<MissionOfferManager>();
        
        if (offerManager == null)
        {
            offerManager = missionOfferGO.AddComponent<MissionOfferManager>();
            Debug.Log("Added MissionOfferManager component");
        }
        
        GameObject baseGO = GameObject.Find(BASE_NAME);
        
        if (baseGO == null)
        {
            baseGO = CreateBaseAtPlayerPosition();
        }
        
        if (baseGO != null)
        {
            offerManager.baseLocation = baseGO.transform;
            offerManager.baseDetectionRadius = BASE_RADIUS;
            offerManager.nextMissionIndex = 1;
            
            EditorUtility.SetDirty(offerManager);
            
            Debug.Log("Mission Offer System setup complete!");
            EditorUtility.DisplayDialog(
                "Setup Complete", 
                $"Mission Offer System configured:\n\n" +
                $"- MissionOfferManager: Ready\n" +
                $"- Base Location: {baseGO.name}\n" +
                $"- Detection Radius: {BASE_RADIUS}m\n" +
                $"- Starting Mission: Mission01\n\n" +
                $"The system will now offer missions after challenge completion!", 
                "OK"
            );
            
            Selection.activeGameObject = missionOfferGO;
        }
    }
    
    private GameObject CreateBaseAtPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find Player in scene!", "OK");
            return null;
        }
        
        GameObject existingBase = GameObject.Find(BASE_NAME);
        if (existingBase != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Base Exists", 
                $"A base named '{BASE_NAME}' already exists. Replace it?", 
                "Yes", 
                "No"
            );
            
            if (replace)
            {
                DestroyImmediate(existingBase);
            }
            else
            {
                return existingBase;
            }
        }
        
        GameObject baseGO = new GameObject(BASE_NAME);
        baseGO.transform.position = player.transform.position;
        baseGO.tag = "Untagged";
        baseGO.layer = LayerMask.NameToLayer("Default");
        
        SphereCollider collider = baseGO.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = BASE_RADIUS;
        
        BaseInteraction baseInteraction = baseGO.AddComponent<BaseInteraction>();
        baseInteraction.baseName = "Safe House";
        baseInteraction.interactionRadius = BASE_RADIUS;
        
        EditorUtility.SetDirty(baseGO);
        
        Debug.Log($"Created PlayerBase at position: {baseGO.transform.position}");
        
        return baseGO;
    }
}
