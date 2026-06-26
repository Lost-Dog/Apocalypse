using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Shooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ShooterHolsterSetupTool : EditorWindow
{
    private GameObject playerRoot;
    private Character character;
    private ShooterWeapon defaultPistolWeapon;
    private GameObject defaultPistolModel;

    private KeyCode holsterKey = KeyCode.F2;
    private bool enableGamepadFaceButton = true;
    private KeyCode gamepadFaceButton = KeyCode.JoystickButton3;
    private bool toggleBackOnSecondPress = true;
    private bool autoResolveCharacter = true;
    private bool loadKeybindFromPrefs = true;
    private bool saveKeybindToPrefs = true;
    private string keybindPrefsKey = "ShooterHolsterHotkey.HolsterKey";
    private bool enableDebugLogs = true;
    private bool enableDebugOverlay = true;
    private float debugOverlayDuration = 1.5f;

    [MenuItem("Tools/Apocalypse/Shooter/Holster Setup Tool")]
    public static void OpenWindow()
    {
        ShooterHolsterSetupTool window = GetWindow<ShooterHolsterSetupTool>("Holster Setup");
        window.minSize = new Vector2(420f, 300f);
        window.Show();
    }

    private void OnEnable()
    {
        TryPickFromSelection();
        TryPickPlayerByTag();
        TryResolveCharacter();
        SyncFromExistingComponent();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Shooter Holster / Unholster Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(6f);

        DrawTargetSection();
        EditorGUILayout.Space(8f);

        DrawConfigSection();
        EditorGUILayout.Space(12f);

        EditorGUILayout.HelpBox(
            "Assign a default weapon/model if you want guaranteed re-equip when no prior shooter weapon was cached.",
            MessageType.Info
        );

        using (new EditorGUI.DisabledScope(playerRoot == null))
        {
            if (GUILayout.Button("Apply Holster Setup", GUILayout.Height(36f)))
            {
                ApplySetup();
            }
        }
    }

    private void DrawTargetSection()
    {
        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

        playerRoot = (GameObject)EditorGUILayout.ObjectField("Player Root", playerRoot, typeof(GameObject), true);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Use Selection"))
            {
                TryPickFromSelection(force: true);
                TryResolveCharacter();
                SyncFromExistingComponent();
            }

            if (GUILayout.Button("Find Player Tag"))
            {
                TryPickPlayerByTag(force: true);
                TryResolveCharacter();
                SyncFromExistingComponent();
            }
        }

        character = (Character)EditorGUILayout.ObjectField("Character", character, typeof(Character), true);

        if (playerRoot == null)
        {
            EditorGUILayout.HelpBox("Pick your player GameObject first.", MessageType.Warning);
        }
        else if (character == null)
        {
            EditorGUILayout.HelpBox("No Character component found. Assign Character manually or enable auto resolve.", MessageType.Warning);
        }
    }

    private void DrawConfigSection()
    {
        EditorGUILayout.LabelField("Holster Settings", EditorStyles.boldLabel);

        holsterKey = (KeyCode)EditorGUILayout.EnumPopup("Holster Key", holsterKey);
        enableGamepadFaceButton = EditorGUILayout.Toggle("Enable Gamepad Face Button", enableGamepadFaceButton);
        gamepadFaceButton = (KeyCode)EditorGUILayout.EnumPopup("Gamepad Face Button", gamepadFaceButton);
        toggleBackOnSecondPress = EditorGUILayout.Toggle("Toggle Re-equip", toggleBackOnSecondPress);
        autoResolveCharacter = EditorGUILayout.Toggle("Auto Resolve Character", autoResolveCharacter);

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Default Fallback", EditorStyles.boldLabel);

        defaultPistolWeapon = (ShooterWeapon)EditorGUILayout.ObjectField(
            "Default Shooter Weapon",
            defaultPistolWeapon,
            typeof(ShooterWeapon),
            false
        );

        defaultPistolModel = (GameObject)EditorGUILayout.ObjectField(
            "Default Weapon Model",
            defaultPistolModel,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Keybind Persistence", EditorStyles.boldLabel);
        loadKeybindFromPrefs = EditorGUILayout.Toggle("Load Key From Prefs", loadKeybindFromPrefs);
        saveKeybindToPrefs = EditorGUILayout.Toggle("Save Key To Prefs", saveKeybindToPrefs);
        keybindPrefsKey = EditorGUILayout.TextField("Prefs Key", keybindPrefsKey);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Debug Feedback", EditorStyles.boldLabel);
        enableDebugLogs = EditorGUILayout.Toggle("Enable Debug Logs", enableDebugLogs);
        enableDebugOverlay = EditorGUILayout.Toggle("Enable Debug Overlay", enableDebugOverlay);
        debugOverlayDuration = EditorGUILayout.FloatField("Overlay Duration", debugOverlayDuration);
    }

    private void ApplySetup()
    {
        if (playerRoot == null)
        {
            EditorUtility.DisplayDialog("Holster Setup", "Select a Player Root first.", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(playerRoot, "Apply Holster Setup");

        ShooterHolsterHotkey hotkey = playerRoot.GetComponent<ShooterHolsterHotkey>();
        if (hotkey == null)
        {
            hotkey = Undo.AddComponent<ShooterHolsterHotkey>(playerRoot);
        }

        SerializedObject so = new SerializedObject(hotkey);

        so.FindProperty("gcCharacter").objectReferenceValue = character;
        so.FindProperty("holsterKey").enumValueIndex = (int)holsterKey;
        so.FindProperty("enableGamepadFaceButton").boolValue = enableGamepadFaceButton;
        so.FindProperty("gamepadFaceButton").enumValueIndex = (int)gamepadFaceButton;
        so.FindProperty("toggleBackOnSecondPress").boolValue = toggleBackOnSecondPress;
        so.FindProperty("autoResolveCharacter").boolValue = autoResolveCharacter;
        so.FindProperty("defaultPistolWeapon").objectReferenceValue = defaultPistolWeapon;
        so.FindProperty("defaultPistolModel").objectReferenceValue = defaultPistolModel;
        so.FindProperty("loadKeybindFromPrefs").boolValue = loadKeybindFromPrefs;
        so.FindProperty("saveKeybindToPrefs").boolValue = saveKeybindToPrefs;
        so.FindProperty("keybindPrefsKey").stringValue = keybindPrefsKey;
        so.FindProperty("enableDebugLogs").boolValue = enableDebugLogs;
        so.FindProperty("enableDebugOverlay").boolValue = enableDebugOverlay;
        so.FindProperty("debugOverlayDuration").floatValue = Mathf.Max(0.5f, debugOverlayDuration);

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(hotkey);
        EditorUtility.SetDirty(playerRoot);
        EditorSceneManager.MarkSceneDirty(playerRoot.scene);

        Selection.activeGameObject = playerRoot;

        EditorUtility.DisplayDialog(
            "Holster Setup",
            "Holster/unholster setup applied. Enter Play Mode and press your configured key.",
            "OK"
        );
    }

    private void TryPickFromSelection(bool force = false)
    {
        if (!force && playerRoot != null) return;
        if (Selection.activeGameObject == null) return;

        playerRoot = Selection.activeGameObject;
    }

    private void TryPickPlayerByTag(bool force = false)
    {
        if (!force && playerRoot != null) return;

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            playerRoot = taggedPlayer;
        }
    }

    private void TryResolveCharacter()
    {
        if (playerRoot == null)
        {
            character = null;
            return;
        }

        character = playerRoot.GetComponent<Character>();
        if (character == null)
        {
            character = playerRoot.GetComponentInChildren<Character>(true);
        }
    }

    private void SyncFromExistingComponent()
    {
        if (playerRoot == null)
            return;

        ShooterHolsterHotkey existing = playerRoot.GetComponent<ShooterHolsterHotkey>();
        if (existing == null)
            return;

        SerializedObject so = new SerializedObject(existing);

        character = so.FindProperty("gcCharacter").objectReferenceValue as Character;
        holsterKey = (KeyCode)so.FindProperty("holsterKey").enumValueIndex;
        enableGamepadFaceButton = so.FindProperty("enableGamepadFaceButton").boolValue;
        gamepadFaceButton = (KeyCode)so.FindProperty("gamepadFaceButton").enumValueIndex;
        toggleBackOnSecondPress = so.FindProperty("toggleBackOnSecondPress").boolValue;
        autoResolveCharacter = so.FindProperty("autoResolveCharacter").boolValue;
        defaultPistolWeapon = so.FindProperty("defaultPistolWeapon").objectReferenceValue as ShooterWeapon;
        defaultPistolModel = so.FindProperty("defaultPistolModel").objectReferenceValue as GameObject;
        loadKeybindFromPrefs = so.FindProperty("loadKeybindFromPrefs").boolValue;
        saveKeybindToPrefs = so.FindProperty("saveKeybindToPrefs").boolValue;
        keybindPrefsKey = so.FindProperty("keybindPrefsKey").stringValue;
        enableDebugLogs = so.FindProperty("enableDebugLogs").boolValue;
        enableDebugOverlay = so.FindProperty("enableDebugOverlay").boolValue;
        debugOverlayDuration = so.FindProperty("debugOverlayDuration").floatValue;
    }
}
