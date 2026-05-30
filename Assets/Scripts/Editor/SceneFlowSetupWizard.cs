using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneFlowSetupWizard : EditorWindow
{
    // ── Scene slots ──────────────────────────────────────────────────────────
    private SceneAsset _mainMenuScene;
    private SceneAsset _loadingScene;
    private SceneAsset _environmentScene;
    private SceneAsset _environmentEffectsScene;
    private SceneAsset _gameplayObjectsScene;

    // ── Detected objects ─────────────────────────────────────────────────────
    private MultiSceneBootstrapper _bootstrapper;
    private SceneFlowManager       _flowManager;

    // ── UI ───────────────────────────────────────────────────────────────────
    private Vector2 _scroll;
    private string  _status      = "";
    private bool    _statusError;

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Scene Flow/Setup Wizard")]
    public static void Open()
    {
        var w = GetWindow<SceneFlowSetupWizard>(title: "Scene Flow Setup Wizard");
        w.minSize = new Vector2(430, 560);
        w.Scan();
        w.TryAutoFillSceneSlots();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawHeader();
        GUILayout.Space(6);

        SectionLabel("Step 1 — Assign Scenes");
        DrawSceneSlots();
        GUILayout.Space(6);

        SectionLabel("Step 2 — Create & Wire");
        DrawWireSection();
        GUILayout.Space(6);

        SectionLabel("Step 3 — Build Settings");
        DrawBuildSettingsSection();
        GUILayout.Space(8);

        DrawStatus();
        EditorGUILayout.EndScrollView();
    }

    // ── Drawing ───────────────────────────────────────────────────────────────
    private static void DrawHeader()
    {
        GUILayout.Space(4);
        var title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("Scene Flow Setup Wizard", title, GUILayout.Height(24));
        EditorGUILayout.HelpBox(
            "Drag your scene assets into the slots, then click Create & Wire. " +
            "GameObjects, components, and all cross-references are created automatically.",
            MessageType.Info);
    }

    private static void SectionLabel(string text)
    {
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        Rect r = GUILayoutUtility.GetRect(0, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f, 0.6f));
        GUILayout.Space(2);
    }

    private void DrawSceneSlots()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Auto-Fill Scene Slots"))
            {
                TryAutoFillSceneSlots();
            }

            if (GUILayout.Button("Clear"))
            {
                _mainMenuScene = null;
                _loadingScene = null;
                _environmentScene = null;
                _environmentEffectsScene = null;
                _gameplayObjectsScene = null;
                SetStatus("Cleared scene slot assignments.", error: false);
            }
        }

        _mainMenuScene           = SceneField("Main Menu Scene",        _mainMenuScene,
            "Your main menu .unity scene.");
        _loadingScene            = SceneField("Loading Scene",          _loadingScene,
            "Minimal scene shown while other scenes are loading.");
        _environmentScene        = SceneField("Environment Scene",      _environmentScene,
            "Terrain, buildings, static world geometry.");
        _environmentEffectsScene = SceneField("Env Effects Scene",      _environmentEffectsScene,
            "Weather, fog, volumetric lighting, ambient FX.");
        _gameplayObjectsScene    = SceneField("Gameplay Objects Scene", _gameplayObjectsScene,
            "Player, enemies, spawners, game managers.");
    }

    private void DrawWireSection()
    {
        bool allSet = _mainMenuScene && _loadingScene && _environmentScene
                      && _environmentEffectsScene && _gameplayObjectsScene;

        EditorGUI.BeginDisabledGroup(!allSet);
        if (GUILayout.Button("Create & Wire Everything", GUILayout.Height(36)))
        {
            CreateAndWire();
        }
        EditorGUI.EndDisabledGroup();

        if (!allSet)
        {
            EditorGUILayout.HelpBox("Assign all five scene slots to enable this button.", MessageType.Warning);
        }

        GUILayout.Space(4);

        if (GUILayout.Button("Scan Active Scene for Existing Components"))
        {
            Scan();
        }

        if (_flowManager != null)
            EditorGUILayout.HelpBox($"Found SceneFlowManager on: \"{_flowManager.gameObject.name}\"", MessageType.Info);

        if (_bootstrapper != null)
            EditorGUILayout.HelpBox($"Found MultiSceneBootstrapper on: \"{_bootstrapper.gameObject.name}\"", MessageType.Info);
    }

    private void DrawBuildSettingsSection()
    {
        EditorGUILayout.HelpBox(
            "All five scenes must be added to File → Build Settings → Scenes In Build.",
            MessageType.None);

        if (GUILayout.Button("Add All Assigned Scenes to Build Settings"))
        {
            AddToBuildSettings();
        }

        GUILayout.Space(4);

        SceneAsset[] assets = { _mainMenuScene, _loadingScene, _environmentScene, _environmentEffectsScene, _gameplayObjectsScene };
        string[]     names  = { "Main Menu", "Loading", "Environment", "Env Effects", "Gameplay Objects" };

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] == null) continue;
            bool   inBuild = IsInBuildSettings(assets[i]);
            string label   = $"{(inBuild ? "✓" : "✗")}  {names[i]}: {assets[i].name}";
            var    style   = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = inBuild ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.45f, 0.45f) }
            };
            EditorGUILayout.LabelField(label, style);
        }
    }

    private void DrawStatus()
    {
        if (!string.IsNullOrEmpty(_status))
            EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);
    }

    // ── Core logic ────────────────────────────────────────────────────────────
    private void CreateAndWire()
    {
        Scan();

        if (_bootstrapper == null)
        {
            var go = new GameObject("MultiSceneBootstrapper");
            Undo.RegisterCreatedObjectUndo(go, "Create MultiSceneBootstrapper");
            _bootstrapper = go.AddComponent<MultiSceneBootstrapper>();
        }

        WireBootstrapper(_bootstrapper);

        if (_flowManager == null)
        {
            var go = new GameObject("SceneFlowManager");
            Undo.RegisterCreatedObjectUndo(go, "Create SceneFlowManager");
            _flowManager = go.AddComponent<SceneFlowManager>();
        }

        WireFlowManager(_flowManager, _bootstrapper);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        SetStatus("✓ Created and wired. Both objects selected in Hierarchy.", error: false);
        Selection.objects = new Object[] { _bootstrapper.gameObject, _flowManager.gameObject };
    }

    private void WireBootstrapper(MultiSceneBootstrapper target)
    {
        var so = new SerializedObject(target);
        WriteSceneRef(so, "loadingScene",            _loadingScene);
        WriteSceneRef(so, "environmentScene",         _environmentScene);
        WriteSceneRef(so, "environmentEffectsScene",  _environmentEffectsScene);
        WriteSceneRef(so, "gameplayObjectsScene",     _gameplayObjectsScene);

        SerializedProperty lightingOverride = so.FindProperty("setEnvironmentEffectsSceneActiveForLighting");
        if (lightingOverride != null)
        {
            lightingOverride.boolValue = true;
        }

        so.ApplyModifiedProperties();
        target.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
    }

    private void WireFlowManager(SceneFlowManager target, MultiSceneBootstrapper bootstrapper)
    {
        var so = new SerializedObject(target);
        so.FindProperty("gameplayBootstrapper").objectReferenceValue = bootstrapper;
        WriteSceneRef(so, "mainMenuScene", _mainMenuScene);
        so.ApplyModifiedProperties();
        target.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
    }

    private static void WriteSceneRef(SerializedObject so, string fieldName, SceneAsset asset)
    {
        SerializedProperty container = so.FindProperty(fieldName);
        if (container == null)
        {
            Debug.LogWarning($"[SceneFlowSetupWizard] Field '{fieldName}' not found on {so.targetObject.GetType().Name}.");
            return;
        }
        SerializedProperty assetProp = container.FindPropertyRelative("sceneAsset");
        SerializedProperty nameProp  = container.FindPropertyRelative("sceneName");
        if (assetProp != null) assetProp.objectReferenceValue = asset;
        if (nameProp  != null && asset != null) nameProp.stringValue = asset.name;
    }

    private void AddToBuildSettings()
    {
        SceneAsset[] assets = { _mainMenuScene, _loadingScene, _environmentScene, _environmentEffectsScene, _gameplayObjectsScene };
        List<EditorBuildSettingsScene> current = EditorBuildSettings.scenes.ToList();

        int added = 0;
        foreach (SceneAsset asset in assets)
        {
            if (asset == null) continue;
            string path = AssetDatabase.GetAssetPath(asset);
            if (current.Any(s => s.path == path)) continue;
            current.Add(new EditorBuildSettingsScene(path, enabled: true));
            added++;
        }

        EditorBuildSettings.scenes = current.ToArray();
        SetStatus(added > 0 ? $"✓ Added {added} scene(s) to Build Settings." : "All scenes were already in Build Settings.", error: false);
        Repaint();
    }

    private void Scan()
    {
        _bootstrapper = FindFirstObjectByType<MultiSceneBootstrapper>();
        _flowManager  = FindFirstObjectByType<SceneFlowManager>();
        Repaint();
    }

    private static bool IsInBuildSettings(SceneAsset asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        return EditorBuildSettings.scenes.Any(s => s.path == path);
    }

    private static SceneAsset SceneField(string label, SceneAsset current, string tooltip)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        var result = (SceneAsset)EditorGUILayout.ObjectField(
            new GUIContent(label, tooltip), current, typeof(SceneAsset), allowSceneObjects: false);
        EditorGUILayout.EndVertical();
        return result;
    }

    private void SetStatus(string msg, bool error)
    {
        _status      = msg;
        _statusError = error;
        Repaint();
    }

    private void TryAutoFillSceneSlots()
    {
        if (_mainMenuScene == null)
            _mainMenuScene = FindSceneAssetByNames("Main Menu Scene", "MainMenu", "Main Menu");

        if (_loadingScene == null)
            _loadingScene = FindSceneAssetByNames("Loading Scene", "Loading");

        if (_environmentScene == null)
            _environmentScene = FindSceneAssetByNames("Apocalypse_GC2", "Environment");

        if (_environmentEffectsScene == null)
            _environmentEffectsScene = FindSceneAssetByNames("OasisScene", "Oasis");

        if (_gameplayObjectsScene == null)
            _gameplayObjectsScene = FindSceneAssetByNames("Gameplay Scene", "GameplayObjects", "Gameplay");

        SetStatus("Attempted auto-fill for scene slots using common names.", error: false);
    }

    private static SceneAsset FindSceneAssetByNames(params string[] preferredNames)
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < preferredNames.Length; i++)
        {
            string wanted = preferredNames[i];
            for (int g = 0; g < guids.Length; g++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, wanted, System.StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                }
            }
        }

        for (int i = 0; i < preferredNames.Length; i++)
        {
            string wanted = preferredNames[i];
            for (int g = 0; g < guids.Length; g++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.IndexOf(wanted, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                }
            }
        }

        return null;
    }
}

