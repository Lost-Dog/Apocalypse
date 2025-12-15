using UnityEngine;
using UnityEditor;
using JUTPS;

namespace ApocalypseEditor
{
    public static class BatchPoolableSetup
    {
        [MenuItem("GameObject/Character/Add Poolable Behavior", false, 50)]
        public static void AddPoolableBehavior()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
                return;
            }

            int added = 0;
            int skipped = 0;
            int configured = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null)
                    continue;

                PoolableCharacter poolable = obj.GetComponent<PoolableCharacter>();
                
                if (poolable == null)
                {
                    poolable = Undo.AddComponent<PoolableCharacter>(obj);
                    added++;
                }
                else
                {
                    Undo.RecordObject(poolable, "Configure Poolable");
                    configured++;
                }

                JUHealth health = obj.GetComponent<JUHealth>();
                if (health != null)
                {
                    poolable.health = health;
                    poolable.returnToPoolOnDeath = true;
                    poolable.deactivateDelay = 3f;
                    poolable.disableRagdollBeforeReturn = true;
                }
                else
                {
                    Debug.LogWarning($"{obj.name} doesn't have JUHealth component. Poolable behavior may not work correctly.", obj);
                }

                EditorUtility.SetDirty(poolable);
                
                if (PrefabUtility.IsPartOfPrefabInstance(obj))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(poolable);
                }
            }

            string message = "";
            if (added > 0)
                message += $"Added PoolableCharacter to {added} object(s).\n";
            if (configured > 0)
                message += $"Configured {configured} existing PoolableCharacter component(s).\n";
            if (skipped > 0)
                message += $"Skipped {skipped} object(s).\n";

            Debug.Log(message.TrimEnd());
            EditorUtility.DisplayDialog("Poolable Behavior Added", message, "OK");
            
            AssetDatabase.SaveAssets();
        }

        [MenuItem("GameObject/Character/Add Poolable Behavior", true)]
        public static bool ValidateAddPoolableBehavior()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        [MenuItem("GameObject/Character/Configure Pool Settings", false, 51)]
        public static void ConfigurePoolSettings()
        {
            PoolSettingsWindow.ShowWindow();
        }

        [MenuItem("GameObject/Character/Configure Pool Settings", true)]
        public static bool ValidateConfigurePoolSettings()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }
    }

    public class PoolSettingsWindow : EditorWindow
    {
        private float deactivateDelay = 3f;
        private bool disableRagdollBeforeReturn = true;
        private bool debugLogging = false;

        public static void ShowWindow()
        {
            var window = GetWindow<PoolSettingsWindow>("Pool Settings");
            window.minSize = new Vector2(350, 200);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Configure Pool Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure poolable behavior for selected characters.", MessageType.Info);
            EditorGUILayout.Space(10);

            deactivateDelay = EditorGUILayout.FloatField("Deactivate Delay (seconds)", deactivateDelay);
            EditorGUILayout.HelpBox("How long to wait after death before returning to pool.", MessageType.None);

            EditorGUILayout.Space(5);
            disableRagdollBeforeReturn = EditorGUILayout.Toggle("Disable Ragdoll Before Return", disableRagdollBeforeReturn);
            EditorGUILayout.HelpBox("Disable ragdoll physics before returning to pool.", MessageType.None);

            EditorGUILayout.Space(5);
            debugLogging = EditorGUILayout.Toggle("Debug Logging", debugLogging);

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Apply to Selected", GUILayout.Height(40)))
            {
                ApplySettings();
                Close();
            }
        }

        private void ApplySettings()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
                return;
            }

            int updated = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null)
                    continue;

                PoolableCharacter poolable = obj.GetComponent<PoolableCharacter>();
                
                if (poolable == null)
                {
                    Debug.LogWarning($"{obj.name} doesn't have PoolableCharacter component. Add it first.", obj);
                    continue;
                }

                Undo.RecordObject(poolable, "Configure Pool Settings");
                
                poolable.deactivateDelay = deactivateDelay;
                poolable.disableRagdollBeforeReturn = disableRagdollBeforeReturn;
                poolable.debugLogging = debugLogging;

                EditorUtility.SetDirty(poolable);
                
                if (PrefabUtility.IsPartOfPrefabInstance(obj))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(poolable);
                }

                updated++;
            }

            string message = $"Updated pool settings for {updated} character(s).\n\n" +
                           $"Deactivate Delay: {deactivateDelay}s\n" +
                           $"Disable Ragdoll: {disableRagdollBeforeReturn}\n" +
                           $"Debug Logging: {debugLogging}";

            Debug.Log(message);
            EditorUtility.DisplayDialog("Pool Settings Applied", message, "OK");
            
            AssetDatabase.SaveAssets();
        }
    }
}
