using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace ThreepeatEditor
{
    public class MMCStriderIntegration : MMCLegacyIntegrationBase
    {
        public bool useStrider = false;

        public override bool IsPlaceholder()
        {
            return true;
        }

        public override void MakeGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Optional Strider integration unavailable (See Dependency Checks tab for more information)");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public override bool SetupCharacter(GameObject coreObject, GameObject modelObject)
        {
            return true;
        }
    }
}