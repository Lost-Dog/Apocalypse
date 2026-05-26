using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using EmeraldAI;
using Invector.vCharacterController.AI;

public class MakePrefabTool : EditorWindow
{
    private const string DefaultSavePath = "Assets/Prefabs";

    // Tabs
    private int activeTab = 0;
    private readonly string[] tabLabels = { "Make Prefab", "Copy Folder", "Assign Waypoint", "Add Damage Receiver", "Player Bridge", "Hit Effects" };

    // Tab 0 — Make Prefab
    private string savePath = DefaultSavePath;
    private bool connectToPrefab = true;
    private Vector2 makePrefabScroll;
    private List<string> makePrefabLog = new List<string>();

    // Tab 1 — Copy Folder
    private string sourceFolderPath = "";
    private string copyDestinationPath = DefaultSavePath;

    // Tab 2 — Assign Waypoint
    private vWaypointArea waypointAreaToAssign;
    private string civilianPrefabFolder = "Assets/Prefabs/Character_Prefabs/Civilians";
    private List<string> waypointAssignLog = new List<string>();

    // Tab 3 — Add Damage Receiver
    private string damageReceiverPrefabFolder = "Assets/Prefabs/Character_Prefabs/Enemy_Prefabs";
    private List<string> damageReceiverLog = new List<string>();

    // Tab 4 — Player Bridge
    private string playerPrefabPath  = "Assets/Prefabs/Player.prefab";
    private string enemyBridgeFolder = "Assets/Prefabs/Character_Prefabs/Enemy_Prefabs";
    private List<string> playerBridgeLog = new List<string>();

    // Tab 5 — Hit Effects
    private string hitEffectsPrefabFolder = "Assets/Prefabs/Character_Prefabs/Enemy_Prefabs";
    private GameObject hitEffectPrefab;
    private GameObject bloodDecalPrefab;
    private float hitEffectTimeout = 1.5f;
    private bool attachHitEffects = false;
    private Vector2 hitEffectsScroll;
    private List<string> hitEffectsLog = new List<string>();

    [MenuItem("Tools/Make Prefab Tool")]
    public static void ShowWindow()
    {
        MakePrefabTool window = GetWindow<MakePrefabTool>("Make Prefab");
        window.minSize = new Vector2(380, 240);
        window.RefreshSelection();
        window.Show();
    }

    private void OnFocus()
    {
        RefreshSelection();
    }

    private void RefreshSelection() { }

    private void OnGUI()
    {
        activeTab = GUILayout.Toolbar(activeTab, tabLabels);
        EditorGUILayout.Space(8);

        switch (activeTab)
        {
            case 0: DrawMakePrefabTab();     break;
            case 1: DrawCopyFolderTab();     break;
            case 2: DrawAssignWaypointTab(); break;
            case 3: DrawDamageReceiverTab(); break;
            case 4: DrawPlayerBridgeTab();   break;
            case 5: DrawHitEffectsTab();     break;
        }
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 0 — MAKE PREFAB
    // ------------------------------------------------------------------------------------------------

    private void DrawMakePrefabTab()
    {
        EditorGUILayout.LabelField("Create Prefabs from Hierarchy Selection", EditorStyles.boldLabel);

        // Save path picker.
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save To", GUILayout.Width(60));
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Save Folder", savePath, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                savePath = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        connectToPrefab = EditorGUILayout.Toggle("Connect to Prefab", connectToPrefab);

        EditorGUILayout.Space(6);

        // Live selection preview.
        GameObject[] selected = Selection.gameObjects;
        int sceneObjects = 0;
        foreach (GameObject go in selected)
            if (!PrefabUtility.IsPartOfPrefabAsset(go)) sceneObjects++;

        EditorGUILayout.HelpBox(
            sceneObjects == 0
                ? "Select one or more GameObjects in the Hierarchy."
                : $"{sceneObjects} scene object(s) selected — will be saved as prefabs.",
            sceneObjects == 0 ? MessageType.Info : MessageType.None);

        EditorGUILayout.Space(8);

        bool canCreate = sceneObjects > 0 && AssetDatabase.IsValidFolder(savePath);
        GUI.enabled = canCreate;

        if (GUILayout.Button($"SAVE {(sceneObjects > 0 ? sceneObjects.ToString() : string.Empty)} PREFAB(S)".Trim(), GUILayout.Height(38)))
            CreatePrefabs(selected);

        GUI.enabled = true;

        // Log.
        if (makePrefabLog.Count > 0)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            makePrefabScroll = EditorGUILayout.BeginScrollView(makePrefabScroll, GUILayout.MaxHeight(140));
            foreach (string line in makePrefabLog)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }
    }

    private void CreatePrefabs(GameObject[] objects)
    {
        makePrefabLog.Clear();
        int saved = 0;

        foreach (GameObject go in objects)
        {
            // Skip prefab assets dropped in from the Project window.
            if (PrefabUtility.IsPartOfPrefabAsset(go))
            {
                makePrefabLog.Add($"SKIP  {go.name}  — already a prefab asset");
                continue;
            }

            string fullPath = Path.Combine(savePath, go.name + ".prefab").Replace("\\", "/");

            if (File.Exists(fullPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Overwrite?",
                    $"{go.name}.prefab already exists.\n\nOverwrite it?", "Overwrite", "Skip");
                if (!overwrite)
                {
                    makePrefabLog.Add($"SKIP  {go.name}  — already exists");
                    continue;
                }
            }

            InteractionMode mode = InteractionMode.UserAction;

            if (connectToPrefab)
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, fullPath, mode);
            else
                PrefabUtility.SaveAsPrefabAsset(go, fullPath);

            makePrefabLog.Add($"OK    {go.name}  → {fullPath}");
            saved++;
        }

        AssetDatabase.Refresh();

        string summary = $"Done — {saved} prefab(s) saved to {savePath}.";
        makePrefabLog.Add(string.Empty);
        makePrefabLog.Add(summary);
        Debug.Log($"[MakePrefabTool] {summary}");
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 1 — COPY FOLDER
    // ------------------------------------------------------------------------------------------------

    private void DrawCopyFolderTab()
    {
        EditorGUILayout.LabelField("Copy Prefab Folder", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Source", GUILayout.Width(80));
        sourceFolderPath = EditorGUILayout.TextField(sourceFolderPath);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Source Folder", sourceFolderPath, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                sourceFolderPath = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Destination", GUILayout.Width(80));
        copyDestinationPath = EditorGUILayout.TextField(copyDestinationPath);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Destination Folder", copyDestinationPath, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                copyDestinationPath = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);

        bool canCopy = AssetDatabase.IsValidFolder(sourceFolderPath) && !string.IsNullOrEmpty(copyDestinationPath);
        GUI.enabled = canCopy;

        if (GUILayout.Button("COPY FOLDER", GUILayout.Height(38)))
            CopyFolder();

        GUI.enabled = true;
    }

    private void CopyFolder()
    {
        string sourceName = Path.GetFileName(sourceFolderPath);
        string destPath   = Path.Combine(copyDestinationPath, sourceName).Replace("\\", "/");

        if (AssetDatabase.IsValidFolder(destPath))
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                $"Destination already exists:\n{destPath}\n\nContinue and merge?", "Continue", "Cancel"))
                return;
        }

        CopyFolderRecursive(sourceFolderPath, copyDestinationPath);
        AssetDatabase.Refresh();

        Debug.Log($"[MakePrefabTool] Copied '{sourceFolderPath}' → '{destPath}'");
        EditorUtility.DisplayDialog("Copy Complete", $"Copied to:\n{destPath}", "OK");
    }

    private static void CopyFolderRecursive(string source, string destParent)
    {
        string folderName = Path.GetFileName(source);
        string destFolder = Path.Combine(destParent, folderName).Replace("\\", "/");

        if (!AssetDatabase.IsValidFolder(destFolder))
            AssetDatabase.CreateFolder(destParent, folderName);

        foreach (string file in Directory.GetFiles(source))
        {
            if (file.EndsWith(".meta")) continue;
            string assetFile = "Assets" + file.Substring(Application.dataPath.Length);
            string destFile  = Path.Combine(destFolder, Path.GetFileName(file)).Replace("\\", "/");
            AssetDatabase.CopyAsset(assetFile, destFile);
        }

        foreach (string subDir in Directory.GetDirectories(source))
            CopyFolderRecursive(subDir, destFolder);
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 2 — ASSIGN WAYPOINT
    // ------------------------------------------------------------------------------------------------

    private void DrawAssignWaypointTab()
    {
        EditorGUILayout.LabelField("Assign Waypoint Area to Civilian Prefabs", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Waypoint Area", GUILayout.Width(100));
        waypointAreaToAssign = (vWaypointArea)EditorGUILayout.ObjectField(waypointAreaToAssign, typeof(vWaypointArea), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Civilian Folder", GUILayout.Width(100));
        civilianPrefabFolder = EditorGUILayout.TextField(civilianPrefabFolder);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Civilian Prefab Folder", civilianPrefabFolder, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                civilianPrefabFolder = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);

        bool canAssign = waypointAreaToAssign != null && AssetDatabase.IsValidFolder(civilianPrefabFolder);
        GUI.enabled = canAssign;

        if (GUILayout.Button("ASSIGN WAYPOINT AREA TO ALL PREFABS", GUILayout.Height(38)))
            AssignWaypointToPrefabs();

        GUI.enabled = true;

        if (waypointAssignLog.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            foreach (string line in waypointAssignLog)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
        }
    }

    private void AssignWaypointToPrefabs()
    {
        waypointAssignLog.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { civilianPrefabFolder });
        if (guids.Length == 0)
        {
            waypointAssignLog.Add("No prefabs found in the specified folder.");
            return;
        }

        int assigned = 0;
        int skipped  = 0;

        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;
                var ai = root.GetComponent<vControlAI>();

                if (ai == null)
                {
                    waypointAssignLog.Add($"SKIP  {root.name}  — no vControlAI");
                    skipped++;
                    continue;
                }

                ai.waypointArea = waypointAreaToAssign;
                waypointAssignLog.Add($"OK    {root.name}  — waypoint area assigned");
                assigned++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"Done — {assigned} prefab(s) updated, {skipped} skipped.";
        waypointAssignLog.Add(string.Empty);
        waypointAssignLog.Add(summary);
        Debug.Log($"[MakePrefabTool] Assign Waypoint: {summary}");
        EditorUtility.DisplayDialog("Assign Waypoint Area", summary, "OK");
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 3 — ADD DAMAGE RECEIVER
    // ------------------------------------------------------------------------------------------------

    private void DrawDamageReceiverTab()
    {
        EditorGUILayout.LabelField("Setup Emerald AI Enemy Prefabs for Invector Damage", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "For every Emerald AI prefab in the chosen folder:\n" +
            "  1. Adds InvectorAIBridge to the root (receives TakeDamage from Invector, routes to EmeraldHealth).\n" +
            "  2. Adds vDamageReceiver to every non-trigger child collider (so bullets hitting bones find a vIDamageReceiver and forward damage up to the root).",
            MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Prefab Folder", GUILayout.Width(90));
        damageReceiverPrefabFolder = EditorGUILayout.TextField(damageReceiverPrefabFolder);

        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Prefab Folder", damageReceiverPrefabFolder, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                damageReceiverPrefabFolder = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);

        GUI.enabled = AssetDatabase.IsValidFolder(damageReceiverPrefabFolder);

        if (GUILayout.Button("SETUP ALL PREFABS FOR INVECTOR DAMAGE", GUILayout.Height(38)))
            AddDamageReceiverToPrefabs();

        GUI.enabled = true;

        if (damageReceiverLog.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            foreach (string line in damageReceiverLog)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
        }
    }

    /// <summary>
    /// For every Emerald AI prefab in the folder:
    ///   - Ensures InvectorAIBridge is on the root (the vIDamageReceiver implementation that calls EmeraldHealth.Damage).
    ///   - Adds vDamageReceiver to every non-trigger child Collider so bullets that hit
    ///     bone GameObjects can find a vIDamageReceiver and forward damage up to the root bridge.
    /// </summary>
    private void AddDamageReceiverToPrefabs()
    {
        damageReceiverLog.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { damageReceiverPrefabFolder });
        if (guids.Length == 0)
        {
            damageReceiverLog.Add("No prefabs found in the specified folder.");
            return;
        }

        int prefabsModified       = 0;
        int bridgesAdded          = 0;
        int receiversAdded        = 0;

        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null) continue;

            // Only process Emerald AI prefabs.
            if (prefabAsset.GetComponent<EmeraldSystem>() == null)
            {
                damageReceiverLog.Add($"SKIP  {prefabAsset.name}  — no EmeraldSystem");
                continue;
            }

            bool modified = false;

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;

                // ── Step 1: InvectorAIBridge on root ──────────────────────────────
                if (root.GetComponent<InvectorAIBridge>() == null)
                {
                    root.AddComponent<InvectorAIBridge>();
                    damageReceiverLog.Add($"  + InvectorAIBridge → {prefabAsset.name} (root)");
                    bridgesAdded++;
                    modified = true;
                }

                // ── Step 2: vDamageReceiver on every non-trigger child collider ───
                // The root already has InvectorAIBridge (a vHealthController / vIHealthController).
                // vDamageReceiver.TakeDamage walks up via GetComponentInParent<vIHealthController>()
                // and lands on InvectorAIBridge, which calls EmeraldHealth.Damage.
                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                foreach (Collider col in colliders)
                {
                    // Skip trigger colliders (Emerald AI uses these for detection, not physics hits).
                    if (col.isTrigger) continue;

                    // Skip the root GameObject itself — InvectorAIBridge already handles it.
                    if (col.gameObject == root) continue;

                    // Skip if already set up.
                    if (col.gameObject.GetComponent<Invector.vCharacterController.vDamageReceiver>() != null) continue;

                    col.gameObject.AddComponent<Invector.vCharacterController.vDamageReceiver>();
                    damageReceiverLog.Add($"  + vDamageReceiver  → {prefabAsset.name}/{col.gameObject.name}");
                    receiversAdded++;
                    modified = true;
                }
            }

            if (modified) prefabsModified++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"Done — {prefabsModified} prefab(s) modified, {bridgesAdded} InvectorAIBridge(s) added, {receiversAdded} vDamageReceiver(s) added.";
        damageReceiverLog.Add(string.Empty);
        damageReceiverLog.Add(summary);
        Debug.Log($"[MakePrefabTool] Setup Damage Receivers: {summary}");
        EditorUtility.DisplayDialog("Setup Damage Receivers", summary, "OK");
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 4 — PLAYER ↔ EMERALD AI BRIDGE
    // ------------------------------------------------------------------------------------------------

    private void DrawPlayerBridgeTab()
    {
        EditorGUILayout.LabelField("Player ↔ Emerald AI Damage Bridge", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Player → Emerald AI damage is handled by InvectorPlayerBridge (official integration).\n\n" +
            "Add InvectorPlayerBridge to your Player prefab root manually in the Inspector. " +
            "It requires TargetPositionModifier and FactionExtension, which Unity adds automatically " +
            "via [RequireComponent].\n\n" +
            "Step 2 patches DetectionLayerMask on all enemy prefabs so Emerald AI bullets include the Player layer.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Player Prefab", GUILayout.Width(100));
        playerPrefabPath = EditorGUILayout.TextField(playerPrefabPath);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFilePanel("Select Player Prefab", "Assets/Prefabs", "prefab");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                playerPrefabPath = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy Folder", GUILayout.Width(100));
        enemyBridgeFolder = EditorGUILayout.TextField(enemyBridgeFolder);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select Enemy Prefab Folder", enemyBridgeFolder, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                enemyBridgeFolder = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);

        bool folderValid = AssetDatabase.IsValidFolder(enemyBridgeFolder);

        GUI.enabled = folderValid;
        if (GUILayout.Button("STEP 2 — Patch Enemy DetectionLayerMask (add Player layer)", GUILayout.Height(38)))
            PatchEnemyDetectionLayerMask();

        GUI.enabled = true;

        if (playerBridgeLog.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            foreach (string line in playerBridgeLog)
                EditorGUILayout.LabelField(line, EditorStyles.helpBox);
        }
    }

    private void PatchEnemyDetectionLayerMask()
    {
        playerBridgeLog.Clear();

        // Layer 8 = Player layer. DetectionLayerMask must include it so Emerald bullets
        // pass the layer guard in BulletProjectile.DamageTarget.
        const int PlayerLayer    = 8;
        const int PlayerLayerBit = 1 << PlayerLayer;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { enemyBridgeFolder });
        if (guids.Length == 0)
        {
            playerBridgeLog.Add("No prefabs found in the specified folder.");
            return;
        }

        int patched = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string prefabPath  = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null) continue;

            if (prefabAsset.GetComponent<EmeraldSystem>() == null)
            {
                playerBridgeLog.Add($"SKIP  {prefabAsset.name}  — no EmeraldSystem");
                continue;
            }

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;
                EmeraldSystem sys = root.GetComponent<EmeraldSystem>();
                if (sys == null) continue;

                var so   = new SerializedObject(sys);
                var prop = so.FindProperty("DetectionLayerMask");

                if (prop == null)
                {
                    playerBridgeLog.Add($"SKIP  {prefabAsset.name}  — DetectionLayerMask property not found");
                    skipped++;
                    continue;
                }

                int currentMask = prop.intValue;
                if ((currentMask & PlayerLayerBit) != 0)
                {
                    playerBridgeLog.Add($"SKIP  {prefabAsset.name}  — Player layer already included");
                    skipped++;
                    continue;
                }

                prop.intValue = currentMask | PlayerLayerBit;
                so.ApplyModifiedPropertiesWithoutUndo();
                playerBridgeLog.Add($"OK    {prefabAsset.name}  — added Player layer to DetectionLayerMask");
                patched++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"Done — {patched} prefab(s) patched, {skipped} already correct or skipped.";
        playerBridgeLog.Add(string.Empty);
        playerBridgeLog.Add(summary);
        Debug.Log($"[MakePrefabTool] Patch DetectionLayerMask: {summary}");
        EditorUtility.DisplayDialog("Patch Enemy DetectionLayerMask", summary, "OK");
    }

    // ------------------------------------------------------------------------------------------------
    // TAB 5 — HIT EFFECTS
    // ------------------------------------------------------------------------------------------------

    private void DrawHitEffectsTab()
    {
        EditorGUILayout.LabelField("Patch Enemy Hit Effects (EmeraldHealth)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Assigns a hit effect prefab to EmeraldHealth.HitEffectsList on every Emerald AI enemy prefab " +
            "in the selected folder, and optionally wires an EmeraldDecals blood decal prefab.\n\n" +
            "The effect plays at the AI's damage-position every time it takes a hit.",
            MessageType.Info);

        EditorGUILayout.Space(8);

        // Folder picker
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy Folder", GUILayout.Width(110));
        hitEffectsPrefabFolder = EditorGUILayout.TextField(hitEffectsPrefabFolder);
        if (GUILayout.Button("Pick", GUILayout.Width(40)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Enemy Prefab Folder", hitEffectsPrefabFolder, "");
            if (!string.IsNullOrEmpty(chosen) && chosen.StartsWith(Application.dataPath))
                hitEffectsPrefabFolder = "Assets" + chosen.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Hit effect prefab picker
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Hit Effect Prefab",
            "Spawns at the AI's damage position on every hit. Added to EmeraldHealth.HitEffectsList."),
            GUILayout.Width(130));
        hitEffectPrefab = (GameObject)EditorGUILayout.ObjectField(hitEffectPrefab, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();

        // Blood decal prefab picker
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Blood Decal Prefab",
            "Optional. Spawns a ground blood decal via EmeraldDecals. Added to EmeraldDecals.BloodEffects (component is added if missing)."),
            GUILayout.Width(130));
        bloodDecalPrefab = (GameObject)EditorGUILayout.ObjectField(bloodDecalPrefab, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Options
        hitEffectTimeout  = EditorGUILayout.Slider(
            new GUIContent("Effect Timeout (s)", "How long the hit effect stays visible before being pooled back."),
            hitEffectTimeout, 0.1f, 10f);
        attachHitEffects  = EditorGUILayout.Toggle(
            new GUIContent("Attach to AI", "Parents the hit effect to the AI transform so it follows movement."),
            attachHitEffects);

        EditorGUILayout.Space(12);

        bool canRun = AssetDatabase.IsValidFolder(hitEffectsPrefabFolder) && hitEffectPrefab != null;
        GUI.enabled = canRun;
        if (GUILayout.Button("APPLY HIT EFFECTS TO ALL ENEMY PREFABS", GUILayout.Height(38)))
            PatchEnemyHitEffects();
        GUI.enabled = true;

        if (!canRun)
            EditorGUILayout.HelpBox("Select a valid folder and at least a Hit Effect Prefab.", MessageType.Warning);

        if (hitEffectsLog.Count > 0)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            hitEffectsScroll = EditorGUILayout.BeginScrollView(hitEffectsScroll, GUILayout.MaxHeight(180));
            foreach (string line in hitEffectsLog)
                EditorGUILayout.LabelField(line, EditorStyles.helpBox);
            EditorGUILayout.EndScrollView();
        }
    }

    private void PatchEnemyHitEffects()
    {
        hitEffectsLog.Clear();

        string hitEffectGuid  = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(hitEffectPrefab));
        string decalGuid      = bloodDecalPrefab != null
            ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bloodDecalPrefab))
            : null;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { hitEffectsPrefabFolder });
        if (guids.Length == 0)
        {
            hitEffectsLog.Add("No prefabs found in the specified folder.");
            return;
        }

        int modified = 0;
        int skipped  = 0;

        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject asset  = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (asset == null) continue;

            // Only patch Emerald AI prefabs
            if (asset.GetComponent<EmeraldSystem>() == null)
            {
                hitEffectsLog.Add($"SKIP  {asset.name}  — no EmeraldSystem");
                skipped++;
                continue;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = scope.prefabContentsRoot;

                // --- EmeraldHealth hit effect -----------------------------------------
                EmeraldHealth health = root.GetComponent<EmeraldHealth>();
                if (health == null)
                {
                    hitEffectsLog.Add($"SKIP  {asset.name}  — no EmeraldHealth");
                    skipped++;
                    continue;
                }

                SerializedObject soHealth     = new SerializedObject(health);
                SerializedProperty useHitProp = soHealth.FindProperty("UseHitEffect");
                SerializedProperty listProp   = soHealth.FindProperty("HitEffectsList");
                SerializedProperty attachProp = soHealth.FindProperty("AttachHitEffects");
                SerializedProperty timeoutProp= soHealth.FindProperty("HitEffectTimeoutSeconds");

                // Enable hit effects
                useHitProp.intValue  = 1; // YesOrNo.Yes
                attachProp.boolValue = attachHitEffects;
                timeoutProp.floatValue = hitEffectTimeout;

                // Clear existing list and add the chosen prefab
                listProp.ClearArray();
                listProp.InsertArrayElementAtIndex(0);
                listProp.GetArrayElementAtIndex(0).objectReferenceValue = hitEffectPrefab;

                soHealth.ApplyModifiedPropertiesWithoutUndo();

                string decalNote = "";

                // --- EmeraldDecals blood decal (optional) --------------------------------
                if (bloodDecalPrefab != null)
                {
                    EmeraldDecals decals = root.GetComponent<EmeraldDecals>();
                    if (decals == null) decals = root.AddComponent<EmeraldDecals>();

                    SerializedObject soDecals       = new SerializedObject(decals);
                    SerializedProperty bloodListProp = soDecals.FindProperty("BloodEffects");

                    bloodListProp.ClearArray();
                    bloodListProp.InsertArrayElementAtIndex(0);
                    bloodListProp.GetArrayElementAtIndex(0).objectReferenceValue = bloodDecalPrefab;

                    soDecals.ApplyModifiedPropertiesWithoutUndo();
                    decalNote = " + blood decal";
                }

                hitEffectsLog.Add($"OK    {asset.name}  — hit effect{decalNote} applied");
                modified++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"Done — {modified} prefab(s) patched, {skipped} skipped.";
        hitEffectsLog.Add(string.Empty);
        hitEffectsLog.Add(summary);
        Debug.Log($"[MakePrefabTool] Patch Hit Effects: {summary}");
        EditorUtility.DisplayDialog("Patch Enemy Hit Effects", summary, "OK");
    }
}
