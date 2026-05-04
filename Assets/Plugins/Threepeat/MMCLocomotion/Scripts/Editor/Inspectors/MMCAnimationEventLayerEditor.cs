using UnityEngine;
using UnityEditor;
using MxM;
using Threepeat;

namespace ThreepeatEditor
{
    [CustomEditor(typeof(MMCAnimationEventLayer))]
    public class MMCAnimationEventLayerEditor : Editor
    {
        private SerializedProperty sp_animationClips;
        private SerializedProperty sp_positions;
        private SerializedProperty sp_blendSpaceName;
        private SerializedProperty sp_animEventInfos;
        private void OnEnable()
        {
            sp_animationClips = serializedObject.FindProperty("m_clips");
            sp_positions = serializedObject.FindProperty("m_positions");
            sp_blendSpaceName = serializedObject.FindProperty("m_blendSpaceName");
            sp_animEventInfos = serializedObject.FindProperty("animEventInfos");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MMC Animation Event Layer", curHeight);

            if (GUILayout.Button("Delete Asset"))
            {
                if (EditorUtility.DisplayDialog("Delete Asseet",
                    "Are you sure? This cannot be reversed", "Yes", "Cancel"))
                {
                    DestroyImmediate(serializedObject.targetObject, true);
                }
            }

            GUILayout.Space(20);

            EditorGUILayout.PropertyField(sp_blendSpaceName);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("avatarMask"), new GUIContent("Avatar Mask (unset = Full Body)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_additive"), new GUIContent("Additive Animation", "Leave this false if you don't know what it does."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_applyFootIk"), new GUIContent("Apply Foot IK", "Leave this false if you don't know what it does."));
            EditorGUILayout.LabelField("Drop animations here", EditorStyles.boldLabel);
            Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag and drop AnimationClips here");

            if (sp_animationClips.arraySize != sp_animEventInfos.arraySize)
            {
                Debug.Log("resetting animEventInfos due to size mismatch");
                sp_animEventInfos.ClearArray();
                for (int i = 0; i < sp_animationClips.arraySize; i++)
                {
                    SerializedProperty animationClipProperty = sp_animationClips.GetArrayElementAtIndex(i);
                    sp_animEventInfos.InsertArrayElementAtIndex(i);
                    sp_animEventInfos.GetArrayElementAtIndex(i).stringValue = ((AnimationClip)animationClipProperty.objectReferenceValue).name;
                }
            }

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        AnimationClip animationClip = draggedObject as AnimationClip;
                        if (animationClip != null)
                        {
                            sp_animationClips.InsertArrayElementAtIndex(sp_animationClips.arraySize);
                            sp_animationClips.GetArrayElementAtIndex(sp_animationClips.arraySize - 1).objectReferenceValue = animationClip;
                            sp_animEventInfos.InsertArrayElementAtIndex(sp_animationClips.arraySize-1);
                            sp_animEventInfos.GetArrayElementAtIndex(sp_animationClips.arraySize - 1).stringValue = animationClip.name;
                        }
                    }

                    PopulatePositions();

                    currentEvent.Use();
                }
            }

            EditorGUILayout.Space();

            for (int i = 0; i < sp_animationClips.arraySize; i++)
            {
                SerializedProperty animationClipProperty = sp_animationClips.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                SerializedProperty animEventInfoProperty = sp_animEventInfos.GetArrayElementAtIndex(i);
                //Debug.LogFormat("evtInfo( {0}, {2} ), i={1}", evtInfo, i, animEventInfoProperty.objectReferenceValue);
                animEventInfoProperty.stringValue = EditorGUILayout.TextField(animEventInfoProperty.stringValue);
                EditorGUILayout.PropertyField(animationClipProperty, GUIContent.none);
                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void PopulatePositions()
        {
            for (int ii=sp_positions.arraySize; ii < sp_animationClips.arraySize; ii++)
            {
                sp_positions.InsertArrayElementAtIndex(ii);
                sp_positions.GetArrayElementAtIndex(sp_positions.arraySize - 1).vector2Value = MMCAnimationEventLayer.GetPositionByIndex(ii);
            }
        }

    }

}//End of namespace MxMEditor
