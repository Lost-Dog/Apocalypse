using System.Collections.Generic;
using Threepeat;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace ThreepeatEditor
{
    [CustomEditor(typeof(NGInputScheme_InputSystem))]
    public class NGInputScheme_InputSystemEditor : Editor
    {
        public bool showRuntimeHelpers = false;

        public override void OnInspectorGUI()
        {
            /*
            if (Application.isPlaying)
            {
                NGCharacter character = (NGCharacter)target;
                showRuntimeHelpers = EditorGUILayout.BeginFoldoutHeaderGroup(showRuntimeHelpers, "Runtime Helpers");
                if (showRuntimeHelpers)
                {
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }*/
            //serializedObject.Update();

            //Called whenever the inspector is drawn for this object.
            DrawDefaultInspector();
        }
    }
}
