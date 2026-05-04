using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class PlayerVisibilityDiagnostic : EditorWindow
{
    [MenuItem("Tools/Player Visibility Diagnostic")]
    public static void ShowWindow()
    {
        GetWindow<PlayerVisibilityDiagnostic>("Player Diagnostic");
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.HelpBox("Player Visibility Diagnostic Tool\n\nThis will check common issues that prevent player from being visible in play mode.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Run Diagnostic", GUILayout.Height(40)))
        {
            RunDiagnostic();
        }
        
        EditorGUILayout.Space(5);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("PLAY MODE ACTIVE - Running runtime checks", MessageType.Warning);
            
            if (GUILayout.Button("Check Player in Play Mode", GUILayout.Height(30)))
            {
                CheckPlayerInPlayMode();
            }
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Fixes:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("1. Find and Enable Player GameObject"))
        {
            FindAndEnablePlayer();
        }
        
        if (GUILayout.Button("2. Fix Camera Target"))
        {
            FixCameraTarget();
        }
        
        if (GUILayout.Button("3. Reset Player Position to Origin"))
        {
            ResetPlayerPosition();
        }
        
        if (GUILayout.Button("4. Check Player Renderers"))
        {
            CheckPlayerRenderers();
        }
        
        if (GUILayout.Button("5. Set Correct Player Layer"))
        {
            SetPlayerLayer();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void RunDiagnostic()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("PLAYER VISIBILITY DIAGNOSTIC");
        Debug.Log("═══════════════════════════════════════\n");
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("❌ CRITICAL: No GameObject with 'Player' tag found!");
            Debug.LogWarning("Searching for player by other methods...\n");
            
            // Try to find by common names
            string[] commonNames = {
                "JU TPS Third Person Character",
                "Player",
                "PlayerCharacter",
                "Character",
                "ThirdPersonCharacter"
            };
            
            foreach (string name in commonNames)
            {
                GameObject found = GameObject.Find(name);
                if (found != null)
                {
                    player = found;
                    Debug.LogWarning($"⚠️ Found GameObject: '{found.name}' but it lacks 'Player' tag!");
                    break;
                }
            }
            
            // Search for GC2 Character component as last resort
            if (player == null)
            {
                var characters = Object.FindObjectsByType<GameCreator.Runtime.Characters.Character>(FindObjectsSortMode.None);
                if (characters.Length > 0)
                {
                    player = characters[0].gameObject;
                    Debug.LogWarning($"⚠️ Found GC2 Character on '{player.name}' but it lacks 'Player' tag!");
                }
            }
            
            if (player == null)
            {
                Debug.LogError("\n❌ NO PLAYER CHARACTER FOUND AT ALL!");
                Debug.LogError("The player either:");
                Debug.LogError("  1. Doesn't exist in the scene");
                Debug.LogError("  2. Was destroyed when entering play mode");
                Debug.LogError("  3. Has an unusual name/setup");
                Debug.LogError("\nAdd a player character to your scene!");
                return;
            }
            
            // Found a player but no tag - fix it!
            Debug.LogWarning($"\n🔧 FIXING: Setting 'Player' tag on '{player.name}'");
            player.tag = "Player";
            EditorUtility.SetDirty(player);
            Debug.Log($"✓ Fixed! '{player.name}' now has 'Player' tag");
        }
        
        Debug.Log($"✓ Player GameObject found: {player.name}");
        Selection.activeGameObject = player;
        
        // Check if active
        if (!player.activeInHierarchy)
        {
            Debug.LogError($"❌ Player GameObject '{player.name}' is DISABLED!");
            Debug.LogWarning("Fix: Click '1. Find and Enable Player GameObject' button below.");
        }
        else
        {
            Debug.Log("✓ Player is active in hierarchy");
        }
        
        // Check position
        Vector3 pos = player.transform.position;
        Debug.Log($"Player Position: {pos}");
        if (pos.y < -1000 || pos.y > 1000)
        {
            Debug.LogWarning($"⚠️ Player Y position ({pos.y}) is extreme! Player might have fallen through world.");
            Debug.LogWarning("Fix: Click '3. Reset Player Position to Origin' button below.");
        }
        
        // Check scale
        Vector3 scale = player.transform.localScale;
        Debug.Log($"Player Scale: {scale}");
        if (scale.x < 0.01f || scale.y < 0.01f || scale.z < 0.01f)
        {
            Debug.LogWarning("⚠️ Player scale is extremely small!");
            Debug.LogWarning($"Fix: Set scale to (1, 1, 1). Current: {scale}");
        }
        
        // Check renderers
        var renderers = player.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"\nRenderer Status ({renderers.Length} total):");
        
        int disabledRenderers = 0;
        int invisibleRenderers = 0;
        
        foreach (var renderer in renderers)
        {
            if (!renderer.enabled)
            {
                disabledRenderers++;
                Debug.LogWarning($"  ❌ DISABLED: {renderer.gameObject.name} ({renderer.GetType().Name})");
            }
            else if (!renderer.gameObject.activeInHierarchy)
            {
                invisibleRenderers++;
                Debug.LogWarning($"  ⚠️ GameObject disabled: {renderer.gameObject.name}");
            }
            else
            {
                Debug.Log($"  ✓ {renderer.gameObject.name} ({renderer.GetType().Name}) - enabled");
            }
        }
        
        if (renderers.Length == 0)
        {
            Debug.LogError("❌ CRITICAL: No Renderer components found on player or children!");
        }
        else if (disabledRenderers > 0 || invisibleRenderers > 0)
        {
            Debug.LogWarning($"Fix: Click '4. Check Player Renderers' to enable them.");
        }
        
        // Check camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("❌ CRITICAL: No Main Camera found!");
        }
        else
        {
            Debug.Log($"\nCamera Status:");
            Debug.Log($"  Camera: {mainCamera.name}");
            Debug.Log($"  Position: {mainCamera.transform.position}");
            Debug.Log($"  Culling Mask: {LayerMask.LayerToName(player.layer)} layer included: {((mainCamera.cullingMask & (1 << player.layer)) != 0 ? "✓ YES" : "❌ NO")}");
            
            float distance = Vector3.Distance(mainCamera.transform.position, player.transform.position);
            Debug.Log($"  Distance to player: {distance:F2}m");
            
            if (distance > 1000f)
            {
                Debug.LogWarning("⚠️ Camera is very far from player!");
            }
        }
        
        // Check layer
        Debug.Log($"\nPlayer Layer: {LayerMask.LayerToName(player.layer)} ({player.layer})");
        
        Debug.Log("\n═══════════════════════════════════════");
        Debug.Log("Diagnostic Complete - Check messages above");
        Debug.Log("═══════════════════════════════════════");
    }

    private void FindAndEnablePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            // Try common player names
            string[] commonNames = {
                "JU TPS Third Person Character",
                "Player",
                "PlayerCharacter",
                "Character",
                "ThirdPersonCharacter"
            };
            
            foreach (string name in commonNames)
            {
                GameObject playerByName = GameObject.Find(name);
                if (playerByName != null)
                {
                    playerByName.tag = "Player";
                    player = playerByName;
                    Debug.Log($"✓ Found GameObject '{name}' and set tag to 'Player'");
                    break;
                }
            }
            
            // If still not found, search for GC2 Character component
            if (player == null)
            {
                var characters = Object.FindObjectsByType<GameCreator.Runtime.Characters.Character>(FindObjectsSortMode.None);
                if (characters.Length > 0)
                {
                    player = characters[0].gameObject;
                    player.tag = "Player";
                    Debug.Log($"✓ Found GC2 Character on '{player.name}' and set tag to 'Player'");
                }
            }
            
            if (player == null)
            {
                Debug.LogError("═══════════════════════════════════════");
                Debug.LogError("NO PLAYER CHARACTER FOUND IN SCENE!");
                Debug.LogError("═══════════════════════════════════════");
                Debug.LogError("");
                Debug.LogError("You need to add a player character to your scene:");
                Debug.LogError("");
                Debug.LogError("1. Find your player prefab in Project window");
                Debug.LogError("   (Search for: JU TPS Third Person Character)");
                Debug.LogError("");
                Debug.LogError("2. Drag it into the scene Hierarchy");
                Debug.LogError("");
                Debug.LogError("3. Set its Tag to 'Player' in Inspector");
                Debug.LogError("");
                Debug.LogError("4. Position it above the ground (Y = 1 or 2)");
                Debug.LogError("");
                Debug.LogError("OR create one using:");
                Debug.LogError("GameObject > JU TPS > Create Third Person Character");
                Debug.LogError("");
                Debug.LogError("═══════════════════════════════════════");
                return;
            }
        }
        
        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
            EditorUtility.SetDirty(player);
            Debug.Log($"✓ Enabled player GameObject: {player.name}");
        }
        else
        {
            Debug.Log($"Player '{player.name}' is already active");
        }
        
        Selection.activeGameObject = player;
    }

    private void FixCameraTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Cannot find player! Fix player first.");
            return;
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }
        
        Debug.Log($"Camera: {mainCamera.name}");
        Debug.Log($"Player: {player.name}");
        Debug.Log($"Distance: {Vector3.Distance(mainCamera.transform.position, player.transform.position):F2}m");
        Debug.Log("Camera should auto-follow player with 'Player' tag via GC2 camera shot system.");
    }

    private void ResetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
            return;
        }
        
        Undo.RecordObject(player.transform, "Reset Player Position");
        player.transform.position = new Vector3(0, 1, 0);
        player.transform.rotation = Quaternion.identity;
        EditorUtility.SetDirty(player);
        
        Debug.Log($"✓ Reset player position to (0, 1, 0)");
        Selection.activeGameObject = player;
        
        // Focus scene view on player
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    private void CheckPlayerRenderers()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
            return;
        }
        
        var renderers = player.GetComponentsInChildren<Renderer>(true);
        
        if (renderers.Length == 0)
        {
            Debug.LogError("❌ No renderers found on player!");
            return;
        }
        
        int fixedCount = 0;
        foreach (var renderer in renderers)
        {
            if (!renderer.enabled)
            {
                Undo.RecordObject(renderer, "Enable Renderer");
                renderer.enabled = true;
                EditorUtility.SetDirty(renderer);
                fixedCount++;
                Debug.Log($"✓ Enabled renderer on: {renderer.gameObject.name}");
            }
            
            if (!renderer.gameObject.activeInHierarchy)
            {
                Undo.RecordObject(renderer.gameObject, "Enable GameObject");
                renderer.gameObject.SetActive(true);
                EditorUtility.SetDirty(renderer.gameObject);
                fixedCount++;
                Debug.Log($"✓ Enabled GameObject: {renderer.gameObject.name}");
            }
        }
        
        if (fixedCount == 0)
        {
            Debug.Log("All renderers are already enabled!");
        }
        else
        {
            Debug.Log($"✓ Fixed {fixedCount} renderer(s)");
        }
        
        Selection.activeGameObject = player;
    }

    private void SetPlayerLayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Cannot find player!");
            return;
        }
        
        // Set to Default layer (0)
        Undo.RecordObject(player, "Set Player Layer");
        player.layer = 0; // Default layer
        EditorUtility.SetDirty(player);
        
        Debug.Log($"✓ Set player to 'Default' layer");
        
        // Check camera culling mask
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            bool canSeeDefault = (mainCamera.cullingMask & (1 << 0)) != 0;
            if (canSeeDefault)
            {
                Debug.Log("✓ Camera culling mask includes Default layer");
            }
            else
            {
                Debug.LogWarning("⚠️ Camera culling mask does NOT include Default layer!");
            }
        }
        
        Selection.activeGameObject = player;
    }

    private void CheckPlayerInPlayMode()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("PLAY MODE PLAYER CHECK");
        Debug.Log("═══════════════════════════════════════\n");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("❌ CRITICAL: Player GameObject NOT FOUND in play mode!");
            Debug.LogError("");
            Debug.LogError("This means the player was:");
            Debug.LogError("  • Destroyed by a script on scene start");
            Debug.LogError("  • Disabled by a script");
            Debug.LogError("  • Never in the scene to begin with");
            Debug.LogError("");
            Debug.LogWarning("Check your scene in EDITOR mode:");
            Debug.LogWarning("1. Exit play mode");
            Debug.LogWarning("2. Look for player in Hierarchy");
            Debug.LogWarning("3. Check if it has scripts that disable/destroy on Start()");
            return;
        }
        
        Debug.Log($"✓ Player found: {player.name}");
        
        // Check active state
        if (!player.activeInHierarchy)
        {
            Debug.LogError($"❌ Player '{player.name}' is DISABLED!");
            Debug.LogWarning("Attempting to enable...");
            player.SetActive(true);
        }
        else
        {
            Debug.Log("✓ Player is active");
        }
        
        // Check position
        Vector3 pos = player.transform.position;
        Debug.Log($"Position: {pos}");
        
        if (pos.y < -100)
        {
            Debug.LogError($"❌ Player has FALLEN THROUGH WORLD! Y = {pos.y}");
            Debug.LogWarning("Resetting position...");
            player.transform.position = new Vector3(pos.x, 2, pos.z);
        }
        else if (pos.y < 0)
        {
            Debug.LogWarning($"⚠️ Player is below ground! Y = {pos.y}");
            Debug.LogWarning("Resetting position...");
            player.transform.position = new Vector3(pos.x, 2, pos.z);
        }
        
        // Check scale
        Vector3 scale = player.transform.localScale;
        Debug.Log($"Scale: {scale}");
        
        if (scale.x < 0.01f || scale.y < 0.01f || scale.z < 0.01f)
        {
            Debug.LogError("❌ Player scale is ZERO or near-zero!");
            Debug.LogWarning("Resetting scale...");
            player.transform.localScale = Vector3.one;
        }
        
        // Check renderers
        var renderers = player.GetComponentsInChildren<Renderer>();
        Debug.Log($"\nRenderers: {renderers.Length} found");
        
        int visible = 0;
        foreach (var r in renderers)
        {
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                visible++;
                Debug.Log($"  ✓ {r.gameObject.name} - visible");
            }
            else
            {
                Debug.LogWarning($"  ❌ {r.gameObject.name} - HIDDEN");
            }
        }
        
        if (visible == 0 && renderers.Length > 0)
        {
            Debug.LogError("❌ ALL RENDERERS ARE DISABLED!");
        }
        
        // Check camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ No Main Camera!");
        }
        else
        {
            float dist = Vector3.Distance(cam.transform.position, player.transform.position);
            Debug.Log($"\nCamera:");
            Debug.Log($"  Position: {cam.transform.position}");
            Debug.Log($"  Distance to player: {dist:F1}m");
            Debug.Log($"  Looking at: {cam.transform.forward}");
            
            if (dist > 1000)
            {
                Debug.LogWarning("⚠️ Camera is VERY FAR from player!");
            }
            
            // Check if player is in view
            Vector3 viewportPoint = cam.WorldToViewportPoint(player.transform.position);
            bool inView = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
            
            Debug.Log($"  Player in camera view: {(inView ? "✓ YES" : "❌ NO")}");
            Debug.Log($"  Viewport point: {viewportPoint}");
            
            if (!inView)
            {
                Debug.LogWarning("Player is outside camera viewport!");
                if (viewportPoint.z < 0)
                    Debug.LogWarning("  → Player is BEHIND camera");
                else if (viewportPoint.x < 0)
                    Debug.LogWarning("  → Player is to the LEFT of camera");
                else if (viewportPoint.x > 1)
                    Debug.LogWarning("  → Player is to the RIGHT of camera");
                else if (viewportPoint.y < 0)
                    Debug.LogWarning("  → Player is BELOW camera view");
                else if (viewportPoint.y > 1)
                    Debug.LogWarning("  → Player is ABOVE camera view");
            }
        }
        
        // Check components
        Debug.Log("\nComponents:");
        var character  = player.GetComponent<GameCreator.Runtime.Characters.Character>();
        var rigidbody  = player.GetComponent<Rigidbody>();
        var collider   = player.GetComponent<Collider>();

        Debug.Log($"  GC2 Character: {(character != null ? "✓" : "❌")}");
        Debug.Log($"  Rigidbody: {(rigidbody != null ? "✓" : "❌")}");
        Debug.Log($"  Collider: {(collider != null ? "✓" : "❌")}");

        if (character != null)
        {
            Debug.Log($"  Character enabled: {character.enabled}");
            Debug.Log($"  IsDead: {character.IsDead}");
        }
        
        Debug.Log("\n═══════════════════════════════════════");
        Debug.Log("Check complete - see messages above");
        Debug.Log("═══════════════════════════════════════");
        
        Selection.activeGameObject = player;
    }
}
