using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BootstrapSceneSetupGuide : EditorWindow
{
    private Vector2 _scroll;

    [MenuItem("Tools/Scene Flow/Bootstrap Scene Guide")]
    public static void Open()
    {
        var window = GetWindow<BootstrapSceneSetupGuide>("Bootstrap Guide");
        window.minSize = new Vector2(540f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Bootstrap Scene Setup Guide", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Use this on your startup scene. This scene should contain persistent scene-flow systems and UI that must survive loading.",
            MessageType.Info);

        string activeScenePath = EditorSceneManager.GetActiveScene().path;
        bool hasActiveScene = !string.IsNullOrEmpty(activeScenePath);

        if (!hasActiveScene)
        {
            EditorGUILayout.HelpBox("No saved scene is currently open. Save your startup scene first.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Active Scene", activeScenePath, EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.Space(6f);
        DrawActions();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Checklist (Active Scene)", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawChecklistRow("SceneFlowManager", FindFirstObjectByType<SceneFlowManager>() != null,
            "Core state machine for MainMenu/Loading/Playing/Paused transitions.");
        DrawChecklistRow("MultiSceneBootstrapper", FindFirstObjectByType<MultiSceneBootstrapper>() != null,
            "Loads Loading -> Environment -> Effects -> Gameplay scenes.");
        DrawChecklistRow("LoadingScreenController", FindFirstObjectByType<LoadingScreenController>() != null,
            "Global loading progress UI subscribed to SceneFlowManager.");
        DrawChecklistRow("SceneTransitionFader", FindFirstObjectByType<SceneTransitionFader>() != null,
            "Top-layer blackout fade to hide hard scene transitions.");

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Gameplay Scene Items (not bootstrap)", EditorStyles.boldLabel);
        DrawChecklistRow("PauseMenuController", FindFirstObjectByType<PauseMenuController>() != null,
            "Place in gameplay scene so it only appears while in-game.");
        DrawChecklistRow("PauseInputHandler", FindFirstObjectByType<PauseInputHandler>() != null,
            "Place in gameplay scene to toggle pause on Escape/Start.");

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8f);
        DrawBuildSettingsStatus(hasActiveScene, activeScenePath);
    }

    private void DrawActions()
    {
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Scene Flow Setup Wizard"))
        {
            EditorApplication.ExecuteMenuItem("Tools/Scene Flow/Setup Wizard");
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Loading Screen Canvas"))
                EditorApplication.ExecuteMenuItem("Tools/Scene Flow/Create Loading Screen Canvas");

            if (GUILayout.Button("Create Transition Fader"))
                EditorApplication.ExecuteMenuItem("Tools/Scene Flow/Create Scene Transition Fader");
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Main Menu Canvas"))
                EditorApplication.ExecuteMenuItem("Tools/Scene Flow/Create Main Menu Canvas");

            if (GUILayout.Button("Create Pause Menu Canvas"))
                EditorApplication.ExecuteMenuItem("Tools/Scene Flow/Create Pause Menu Canvas");
        }

        EditorGUILayout.HelpBox(
            "Main Menu can be in bootstrap or in a dedicated main-menu scene. Pause Menu should be in gameplay scenes.",
            MessageType.None);
    }

    private void DrawBuildSettingsStatus(bool hasActiveScene, string activeScenePath)
    {
        EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

        if (!hasActiveScene)
        {
            EditorGUILayout.HelpBox("Open and save your bootstrap scene before editing build settings.", MessageType.Warning);
            return;
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int activeIndex = scenes.FindIndex(s => s.path == activeScenePath);

        if (activeIndex == 0)
        {
            EditorGUILayout.HelpBox("Active scene is Build Index 0 (startup).", MessageType.Info);
        }
        else if (activeIndex > 0)
        {
            EditorGUILayout.HelpBox("Active scene is in Build Settings but not at index 0.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Active scene is not in Build Settings.", MessageType.Warning);
        }

        if (GUILayout.Button("Set Active Scene As Build Index 0"))
        {
            SetActiveSceneAsBuildIndexZero(activeScenePath);
        }
    }

    private static void SetActiveSceneAsBuildIndexZero(string activeScenePath)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        int existing = scenes.FindIndex(s => s.path == activeScenePath);
        if (existing >= 0)
        {
            scenes.RemoveAt(existing);
        }

        scenes.Insert(0, new EditorBuildSettingsScene(activeScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log("[BootstrapSceneSetupGuide] Set active scene as Build Index 0: " + activeScenePath);
    }

    private static void DrawChecklistRow(string label, bool ok, string detail)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField((ok ? "OK  " : "MISSING  ") + label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(detail, EditorStyles.wordWrappedMiniLabel);
        }
    }
}
