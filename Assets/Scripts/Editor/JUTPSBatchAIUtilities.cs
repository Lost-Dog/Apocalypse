using UnityEngine;
using UnityEditor;
using JUTPS;
using JUTPS.AI;
using JU;
using JU.CharacterSystem.AI;
using System.Collections.Generic;

namespace ApocalypseEditor
{
    public static class JUTPSBatchAIUtilities
    {
        [MenuItem("GameObject/JUTPS Batch/Utilities/Assign Patrol Path to Selected", false, 20)]
        public static void BatchAssignPatrolPath()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            WaypointPath patrolPath = null;
            
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent<WaypointPath>(out var path))
                {
                    patrolPath = path;
                    break;
                }
            }

            if (patrolPath == null)
            {
                var allPaths = Object.FindObjectsByType<WaypointPath>(FindObjectsSortMode.None);
                if (allPaths.Length > 0)
                {
                    string[] options = new string[allPaths.Length];
                    for (int i = 0; i < allPaths.Length; i++)
                    {
                        options[i] = allPaths[i].gameObject.name;
                    }

                    int selectedIndex = EditorUtility.DisplayDialogComplex(
                        "Select Patrol Path",
                        "Choose a patrol path to assign to selected AI characters:",
                        options.Length > 0 ? options[0] : "Cancel",
                        options.Length > 1 ? options[1] : "",
                        options.Length > 2 ? options[2] : ""
                    );

                    if (selectedIndex >= 0 && selectedIndex < allPaths.Length)
                    {
                        patrolPath = allPaths[selectedIndex];
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("No Patrol Paths Found", 
                        "No WaypointPath objects found in the scene. Create one first.", "OK");
                    return;
                }
            }

            if (patrolPath == null)
                return;

            int assignedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent<JU_AI_PatrolCharacter>(out var patrolAI))
                {
                    Undo.RecordObject(patrolAI, "Assign Patrol Path");
                    patrolAI.PatrolPath = patrolPath;
                    patrolAI.PatrolRandomlyIfNotHavePath = false;
                    EditorUtility.SetDirty(patrolAI);
                    assignedCount++;
                }
            }

            EditorUtility.DisplayDialog("Patrol Path Assigned", 
                $"Assigned {patrolPath.gameObject.name} to {assignedCount} AI character(s).", "OK");
        }

        [MenuItem("GameObject/JUTPS Batch/Utilities/Set Detection Range for Selected", false, 21)]
        public static void BatchSetDetectionRange()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            string rangeInput = EditorUtility.DisplayDialogComplex(
                "Set Detection Range",
                "Enter detection range for field of view:",
                "Short (15)", "Medium (30)", "Long (50)"
            ).ToString();

            float range = 30f;
            switch (rangeInput)
            {
                case "0": range = 15f; break;
                case "1": range = 30f; break;
                case "2": range = 50f; break;
            }

            int updatedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                var patrolAI = obj.GetComponent<JU_AI_PatrolCharacter>();
                var zombieAI = obj.GetComponent<JU_AI_Zombie>();

                if (patrolAI != null)
                {
                    Undo.RecordObject(patrolAI, "Set Detection Range");
                    patrolAI.FieldOfView.Distance = range;
                    EditorUtility.SetDirty(patrolAI);
                    updatedCount++;
                }
                else if (zombieAI != null)
                {
                    Undo.RecordObject(zombieAI, "Set Detection Range");
                    zombieAI.FieldOfView.Distance = range;
                    EditorUtility.SetDirty(zombieAI);
                    updatedCount++;
                }
            }

            EditorUtility.DisplayDialog("Detection Range Set", 
                $"Set detection range to {range} for {updatedCount} AI character(s).", "OK");
        }

        [MenuItem("GameObject/JUTPS Batch/Utilities/Configure Field of View for Selected", false, 22)]
        public static void BatchConfigureFieldOfView()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            FieldOfViewConfigWindow.ShowWindow(Selection.gameObjects);
        }

        [MenuItem("GameObject/JUTPS Batch/Utilities/Set AI Movement Speed", false, 23)]
        public static void BatchSetMovementSpeed()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            string speedPreset = EditorUtility.DisplayDialogComplex(
                "Set Movement Speed",
                "Choose movement speed preset:",
                "Slow (2)", "Normal (3)", "Fast (4.5)"
            ).ToString();

            float speed = 3f;
            switch (speedPreset)
            {
                case "0": speed = 2f; break;
                case "1": speed = 3f; break;
                case "2": speed = 4.5f; break;
            }

            int updatedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent<JUCharacterController>(out var controller))
                {
                    Undo.RecordObject(controller, "Set Movement Speed");
                    controller.Speed = speed;
                    EditorUtility.SetDirty(controller);
                    updatedCount++;
                }
            }

            EditorUtility.DisplayDialog("Movement Speed Set", 
                $"Set movement speed to {speed} for {updatedCount} character(s).", "OK");
        }

        [MenuItem("GameObject/JUTPS Batch/Utilities/Enable/Disable Root Motion", false, 24)]
        public static void BatchToggleRootMotion()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            bool enable = EditorUtility.DisplayDialog("Root Motion", 
                "Enable or disable root motion for selected characters?", 
                "Enable", "Disable");

            int updatedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent<JUCharacterController>(out var controller))
                {
                    Undo.RecordObject(controller, "Toggle Root Motion");
                    controller.RootMotion = enable;
                    EditorUtility.SetDirty(controller);
                    updatedCount++;
                }
            }

            EditorUtility.DisplayDialog("Root Motion Updated", 
                $"Root motion {(enable ? "enabled" : "disabled")} for {updatedCount} character(s).", "OK");
        }

        [MenuItem("GameObject/JUTPS Batch/Utilities/Set Tags and Layers", false, 25)]
        public static void BatchSetTagsAndLayers()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters first.", "OK");
                return;
            }

            TagLayerConfigWindow.ShowWindow(Selection.gameObjects);
        }

        [MenuItem("GameObject/JUTPS Batch/Debug/List All AI Components", false, 40)]
        public static void ListAllAIComponents()
        {
            var patrolAIs = Object.FindObjectsByType<JU_AI_PatrolCharacter>(FindObjectsSortMode.None);
            var zombieAIs = Object.FindObjectsByType<JU_AI_Zombie>(FindObjectsSortMode.None);

            string message = $"AI Components in Scene:\n\n" +
                           $"Patrol AI: {patrolAIs.Length}\n" +
                           $"Zombie AI: {zombieAIs.Length}\n" +
                           $"Total: {patrolAIs.Length + zombieAIs.Length}";

            Debug.Log(message);
            EditorUtility.DisplayDialog("AI Components", message, "OK");
        }

        [MenuItem("GameObject/JUTPS Batch/Debug/Validate Selected AI Setup", false, 41)]
        public static void ValidateAISetup()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select AI characters to validate.", "OK");
                return;
            }

            List<string> issues = new List<string>();
            int validCount = 0;

            foreach (var obj in Selection.gameObjects)
            {
                bool hasIssues = false;
                string objName = obj.name;

                if (!obj.GetComponent<Animator>())
                {
                    issues.Add($"{objName}: Missing Animator");
                    hasIssues = true;
                }
                else if (!obj.GetComponent<Animator>().isHuman)
                {
                    issues.Add($"{objName}: Animator is not Humanoid");
                    hasIssues = true;
                }

                if (!obj.GetComponent<JUCharacterController>())
                {
                    issues.Add($"{objName}: Missing JUCharacterController");
                    hasIssues = true;
                }

                if (!obj.GetComponent<JU_AI_PatrolCharacter>() && !obj.GetComponent<JU_AI_Zombie>())
                {
                    issues.Add($"{objName}: Missing AI component");
                    hasIssues = true;
                }

                if (!obj.GetComponent<Rigidbody>())
                {
                    issues.Add($"{objName}: Missing Rigidbody");
                    hasIssues = true;
                }

                if (!obj.GetComponent<CapsuleCollider>())
                {
                    issues.Add($"{objName}: Missing CapsuleCollider");
                    hasIssues = true;
                }

                var patrolAI = obj.GetComponent<JU_AI_PatrolCharacter>();
                if (patrolAI != null && patrolAI.Head == null)
                {
                    issues.Add($"{objName}: AI Head reference not set");
                    hasIssues = true;
                }

                if (!hasIssues)
                    validCount++;
            }

            string resultMessage = $"Validation Results:\n\n" +
                                 $"Valid Characters: {validCount}\n" +
                                 $"Characters with Issues: {issues.Count}\n\n";

            if (issues.Count > 0)
            {
                resultMessage += "Issues Found:\n" + string.Join("\n", issues);
                Debug.LogWarning(resultMessage);
            }
            else
            {
                resultMessage += "All selected characters are properly configured!";
                Debug.Log(resultMessage);
            }

            EditorUtility.DisplayDialog("AI Validation", resultMessage, "OK");
        }
    }

    public class FieldOfViewConfigWindow : EditorWindow
    {
        private GameObject[] targets;
        private float viewDistance = 30f;
        private float viewAngle = 90f;

        public static void ShowWindow(GameObject[] objects)
        {
            var window = GetWindow<FieldOfViewConfigWindow>("Field of View Config");
            window.targets = objects;
            window.minSize = new Vector2(300, 180);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Configure Field of View", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox($"Configuring {targets?.Length ?? 0} AI character(s)", MessageType.Info);
            EditorGUILayout.Space(10);

            viewDistance = EditorGUILayout.FloatField("View Distance", viewDistance);
            viewAngle = EditorGUILayout.Slider("View Angle", viewAngle, 0f, 180f);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Apply to Selected", GUILayout.Height(30)))
            {
                ApplyFieldOfView();
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        private void ApplyFieldOfView()
        {
            if (targets == null)
                return;

            int updatedCount = 0;
            foreach (var obj in targets)
            {
                if (obj == null)
                    continue;

                var patrolAI = obj.GetComponent<JU_AI_PatrolCharacter>();
                var zombieAI = obj.GetComponent<JU_AI_Zombie>();

                if (patrolAI != null)
                {
                    Undo.RecordObject(patrolAI, "Configure Field of View");
                    patrolAI.FieldOfView.Distance = viewDistance;
                    patrolAI.FieldOfView.Angle = viewAngle;
                    EditorUtility.SetDirty(patrolAI);
                    updatedCount++;
                }
                else if (zombieAI != null)
                {
                    Undo.RecordObject(zombieAI, "Configure Field of View");
                    zombieAI.FieldOfView.Distance = viewDistance;
                    zombieAI.FieldOfView.Angle = viewAngle;
                    EditorUtility.SetDirty(zombieAI);
                    updatedCount++;
                }
            }

            EditorUtility.DisplayDialog("Field of View Configured", 
                $"Updated field of view for {updatedCount} character(s).", "OK");
        }
    }

    public class TagLayerConfigWindow : EditorWindow
    {
        private GameObject[] targets;
        private string selectedTag = "Enemy";
        private int selectedLayer = 8;

        public static void ShowWindow(GameObject[] objects)
        {
            var window = GetWindow<TagLayerConfigWindow>("Tag & Layer Config");
            window.targets = objects;
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Configure Tags and Layers", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox($"Configuring {targets?.Length ?? 0} character(s)", MessageType.Info);
            EditorGUILayout.Space(10);

            selectedTag = EditorGUILayout.TagField("Tag", selectedTag);
            selectedLayer = EditorGUILayout.LayerField("Layer", selectedLayer);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Apply to Selected", GUILayout.Height(30)))
            {
                ApplyTagsAndLayers();
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        private void ApplyTagsAndLayers()
        {
            if (targets == null)
                return;

            int updatedCount = 0;
            foreach (var obj in targets)
            {
                if (obj == null)
                    continue;

                Undo.RecordObject(obj, "Set Tag and Layer");
                obj.tag = selectedTag;
                obj.layer = selectedLayer;
                EditorUtility.SetDirty(obj);
                updatedCount++;
            }

            EditorUtility.DisplayDialog("Tags and Layers Set", 
                $"Updated {updatedCount} character(s) with tag '{selectedTag}' and layer {selectedLayer}.", "OK");
        }
    }
}
