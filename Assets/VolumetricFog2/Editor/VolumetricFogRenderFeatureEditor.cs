using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VolumetricFogAndMist2 {
    [CustomEditor(typeof(VolumetricFogRenderFeature))]
    public class VolumetricFogRenderFeatureEditor : Editor {

        public override void OnInspectorGUI () {
            serializedObject.Update();

            SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp != null) {
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.PropertyField(scriptProp);
                }
            }

            SerializedProperty renderPassEventProp = serializedObject.FindProperty("renderPassEvent");
            SerializedProperty renderPassEventOrderProp = serializedObject.FindProperty("renderPassEventOrder");

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(renderPassEventProp);
            bool enumChanged = EditorGUI.EndChangeCheck();

            int enumValue = renderPassEventProp.intValue;
            int effectiveOrder = renderPassEventOrderProp.intValue == 0 ? enumValue : renderPassEventOrderProp.intValue;

            EditorGUI.BeginChangeCheck();
            int newOrder = EditorGUILayout.IntField(GUIContent.none, effectiveOrder, GUILayout.MaxWidth(60));
            bool orderChanged = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndHorizontal();

            if (enumChanged) {
                renderPassEventOrderProp.intValue = 0;
            }

            if (orderChanged) {
                renderPassEventOrderProp.intValue = newOrder == enumValue ? 0 : newOrder;
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "renderPassEvent", "renderPassEventOrder");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
