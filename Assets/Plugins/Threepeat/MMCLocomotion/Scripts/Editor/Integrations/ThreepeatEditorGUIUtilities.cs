using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    public class ThreepeatEditorGUIUtilities
    {
        [MenuItem("Assets/Dev Tools/Convert Rig to Humanoid", true)]
        private static bool ValidateConvertRig()
        {
            foreach (Object obj in Selection.objects)
            {
                if (!(PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Model))
                {
                    return false;
                }
                
            }
            return true;
        }

        [MenuItem("Assets/Dev Tools/Convert Rig to Humanoid", false)]
        private static void ConvertRig()
        {
            foreach (System.Object obj in Selection.objects) {
                Debug.LogFormat("here: {0}", obj.GetType().Name);
            }
        }

        /*
        [MenuItem("Assets/Test Reflection")]
        private static void TestReflection()
        {
            List<string> classes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => typeof(Threepeat.MMCEventBehavior).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => x.Name).ToList();

            Debug.LogFormat("implements MMCEventBehavior: {0}", string.Join(",", classes));

            classes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => typeof(Threepeat.NGInputSchemeBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => x.Name).ToList();

            Debug.LogFormat("implements base input scheme: {0}", string.Join(",", classes));


            classes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => typeof(Threepeat.NGInputSchemeInputDriven).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => x.Name).ToList();

            Debug.LogFormat("implements input-driven: {0}", string.Join(",", classes));

        }*/


        [MenuItem("Assets/Log GUID To Console")]
        private static void LogGuidToConsole()
        {
            string[] a = Selection.assetGUIDs.OfType<string>().Select(o => o.ToString()).ToArray();
            // Do something with you variable
            Debug.LogFormat("Active object GUID: {0}", string.Join(",", a));
        }

        [MenuItem("Assets/Log GUID To Console", true)]
        private static bool LogGuidToConsoleValidator()
        {
            // Do something with you variable
            return Selection.activeObject != null;
        }


        public class LabelWithHelp
        {
            public bool showHelp = false;
            public string labelText = "";
            public string helpBoxText = "";

            public delegate void DrawContentFunction();

            LabelWithHelp(string cfgLabelText, string cfgHelpBoxText)
            {
                labelText = cfgLabelText;
                helpBoxText = cfgHelpBoxText;
            }

            public void MakeGUI()
            {
                showHelp = LabelWithHelpField(labelText, helpBoxText, showHelp);
            }

            public static bool LabelWithHelpField(GUIContent cfgContent, string cfgHelpBoxText, bool cfgShowHelp, DrawContentFunction customContentFunction = null)
            {
                bool retShowHelp = cfgShowHelp;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(cfgContent);

                if (GUILayout.Button("Toggle Help"))
                {
                    retShowHelp = !cfgShowHelp;
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                if (retShowHelp)
                {
                    if (customContentFunction == null)
                    {
                        EditorGUILayout.HelpBox(cfgHelpBoxText, MessageType.Info);
                    }
                    else
                    {
                        customContentFunction();
                    }
                }
                return retShowHelp;
            }

            public static bool LabelWithHelpField(string cfgLabelText, string cfgHelpBoxText, bool cfgShowHelp, DrawContentFunction customContentFunction = null, GUIStyle s = null, bool showOpenPackageManagerButton = false, bool showOpenProjectSettingsButton = false, string integrationPackageGuidToPing = "")
            {
                bool retShowHelp = cfgShowHelp;
                EditorGUILayout.BeginHorizontal();
                if (s != null)
                {
                    GUILayout.Label(cfgLabelText, s);
                }
                else
                {
                    GUILayout.Label(cfgLabelText);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(" \u21B3");
                if (GUILayout.Button("Toggle Help"))
                {
                    retShowHelp = !cfgShowHelp;
                }
                if (showOpenPackageManagerButton && GUILayout.Button("Open Package Manager"))
                {
                    UnityEditor.PackageManager.UI.Window.Open("");
                }

                if (showOpenProjectSettingsButton && GUILayout.Button("Open Project Settings"))
                {
                    //UnityEditor.ProjectSettings //.PackageManager.UI.Window.Open("");
                    EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
                }

                if ((integrationPackageGuidToPing.Length > 0) && GUILayout.Button("Ping Integration Package"))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(integrationPackageGuidToPing));
                    EditorGUIUtility.PingObject(obj);
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();


                if (retShowHelp)
                {
                    if (customContentFunction == null)
                    {
                        EditorGUILayout.HelpBox(cfgHelpBoxText, MessageType.Info);
                    }
                    else
                    {
                        customContentFunction();
                    }
                }
                return retShowHelp;
            }
        }
    }
}
