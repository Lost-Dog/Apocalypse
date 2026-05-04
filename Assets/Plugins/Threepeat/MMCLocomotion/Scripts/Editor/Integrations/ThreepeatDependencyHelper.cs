using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    public class ThreepeatDependencyHelper
    {
        public bool dependencyMet = false;  // This must be set externally.

        public bool showHelp = false;

        public ThreepeatDependencyInfo config;

        public ThreepeatDependencyHelper()
        {

        }

        public ThreepeatDependencyHelper(ThreepeatDependencyInfo cfg)
        {
            config = cfg;
        }

#if SOMETHINGTHATWONTBEDEFFED      
        public void DrawDependencyUI(bool dependencyMet)
        {
            GUIStyle s = new GUIStyle(EditorStyles.label);

            if (/*false &&*/ dependencyMet)
            {
                s.normal.textColor = Color.green;
                EditorGUILayout.LabelField(" \u2713 " + integrationName + ": Found \u2192 Integration Ready", s);
            }
            else if (requiredForMMCFunction)
            {
                s.normal.textColor = Color.red;
                s.fontStyle = FontStyle.Bold;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( //.LabelField(
                        " \u2715 " + integrationName + " Not found or integration package not installed.",
                            s);

                if (GUILayout.Button("Toggle Help"))
                {
                    showHelp = !showHelp;
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                if (showHelp)
                {
                    EditorGUILayout.HelpBox(notFoundHelpBoxText, MessageType.Info);
                }
            }
            else
            {
                s.normal.textColor = Color.yellow;
                s.fontStyle = FontStyle.Bold;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( //.LabelField(
                        " \u2715 " + integrationName + " Not found or integration package not installed.",
                            s);

                if (GUILayout.Button("Toggle Help"))
                {
                    showHelp = !showHelp;
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                if (showHelp)
                {
                    EditorGUILayout.HelpBox(notFoundHelpBoxText, MessageType.Info);
                }

            }

        }
#endif
    }

} // namespace Threepeat
