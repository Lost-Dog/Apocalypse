using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldNavmeshLink))]
    [CanEditMultipleObjects]
    public class EmeraldNavmeshlinkEditor : Editor
    {
        public static EditorWindow EditorWindowRef;
        GUIStyle FoldoutStyle;
        Texture NavmeshLinkEditorIcon;

        #region SerializedProperties
        SerializedProperty HideSettingsFoldout, SettingsFoldoutProp, NavmeshLinkCooldownProp;
        #endregion

        void OnEnable()
        {
            if (NavmeshLinkEditorIcon == null) NavmeshLinkEditorIcon = Resources.Load("Editor Icons/EmeraldNavmeshLink") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            SettingsFoldoutProp = serializedObject.FindProperty("SettingsFoldout");
            NavmeshLinkCooldownProp = serializedObject.FindProperty("NavmeshLinkCooldown");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldNavmeshLink self = (EmeraldNavmeshLink)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("NavMesh Link", NavmeshLinkEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DisplayNavmeshLinkSettings(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        void DisplayNavmeshLinkSettings(EmeraldNavmeshLink self)
        {
            SettingsFoldoutProp.boolValue = CustomEditorProperties.Foldout(SettingsFoldoutProp.boolValue, "NavMesh Link", true, FoldoutStyle);

            if (SettingsFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("NavMesh Link Settings", "The NavMesh Link component gives AI the ability to jump over obstacles as well as jump up and down slopes. This is done through Unity's NavMesh Links.", true);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(NavmeshLinkCooldownProp);
                CustomEditorProperties.CustomHelpLabelField("The cooldown needed before an AI can use a NavMesh link again. This is to avoid an AI using them too quickly.", false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}