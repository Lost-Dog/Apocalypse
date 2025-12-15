using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ApocalypseEditor
{
    public class BatchFactionAssignment : EditorWindow
    {
        private FactionManager.Faction selectedFaction = FactionManager.Faction.Friendly;
        private List<GameObject> selectedObjects = new List<GameObject>();
        private Vector2 scrollPosition;
        private bool addIfMissing = true;

        [MenuItem("Tools/Faction/Batch Faction Assignment")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchFactionAssignment>("Batch Faction Assignment");
            window.minSize = new Vector2(400, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshSelection();
        }

        private void OnSelectionChange()
        {
            RefreshSelection();
            Repaint();
        }

        private void RefreshSelection()
        {
            selectedObjects.Clear();

            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
                return;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj != null)
                {
                    selectedObjects.Add(obj);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Batch Faction Assignment", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Assign factions to multiple GameObjects or prefabs at once.", MessageType.Info);
            EditorGUILayout.Space(10);

            DrawSelectionSection();
            EditorGUILayout.Space(10);

            DrawFactionSettings();
            EditorGUILayout.Space(10);

            DrawActionButtons();
        }

        private void DrawSelectionSection()
        {
            EditorGUILayout.LabelField("Selected Objects", EditorStyles.boldLabel);

            if (selectedObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("No objects selected. Select GameObjects or prefabs in the Project or Hierarchy.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Objects: {selectedObjects.Count}", EditorStyles.miniLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));
            foreach (var obj in selectedObjects)
            {
                if (obj == null)
                    continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);

                var factionMember = obj.GetComponent<FactionMember>();
                if (factionMember != null)
                {
                    EditorGUILayout.LabelField($"Current: {factionMember.faction}", GUILayout.Width(150));
                }
                else
                {
                    EditorGUILayout.LabelField("No FactionMember", GUILayout.Width(150));
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawFactionSettings()
        {
            EditorGUILayout.LabelField("Faction Assignment", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            selectedFaction = (FactionManager.Faction)EditorGUILayout.EnumPopup("Assign Faction", selectedFaction);

            EditorGUILayout.Space(5);
            addIfMissing = EditorGUILayout.Toggle("Add FactionMember if missing", addIfMissing);

            EditorGUILayout.Space(5);
            DrawFactionInfo();

            EditorGUILayout.EndVertical();
        }

        private void DrawFactionInfo()
        {
            EditorGUILayout.LabelField("Faction Relations:", EditorStyles.miniBoldLabel);

            switch (selectedFaction)
            {
                case FactionManager.Faction.Player:
                    EditorGUILayout.HelpBox("Allies: Player, Civilian, Friendly\nEnemies: Rogue, Enemy", MessageType.Info);
                    break;
                case FactionManager.Faction.Rogue:
                    EditorGUILayout.HelpBox("Allies: Rogue, Enemy\nEnemies: Player, Friendly", MessageType.Info);
                    break;
                case FactionManager.Faction.Civilian:
                    EditorGUILayout.HelpBox("Allies: Player, Civilian, Friendly\nEnemies: None", MessageType.Info);
                    break;
                case FactionManager.Faction.Enemy:
                    EditorGUILayout.HelpBox("Allies: Enemy, Rogue\nEnemies: Player, Friendly", MessageType.Info);
                    break;
                case FactionManager.Faction.Friendly:
                    EditorGUILayout.HelpBox("Allies: Friendly, Player, Civilian\nEnemies: Rogue, Enemy", MessageType.Info);
                    break;
                case FactionManager.Faction.Neutral:
                    EditorGUILayout.HelpBox("Allies: Neutral\nEnemies: None", MessageType.Info);
                    break;
            }
        }

        private void DrawActionButtons()
        {
            if (selectedObjects.Count == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Assign Faction to Selected", GUILayout.Height(40)))
            {
                AssignFactions();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Refresh Selection", GUILayout.Height(25)))
            {
                RefreshSelection();
            }
        }

        private void AssignFactions()
        {
            int successCount = 0;
            int addedCount = 0;
            int skippedCount = 0;

            foreach (var obj in selectedObjects)
            {
                if (obj == null)
                    continue;

                try
                {
                    FactionMember factionMember = obj.GetComponent<FactionMember>();

                    if (factionMember == null)
                    {
                        if (addIfMissing)
                        {
                            factionMember = Undo.AddComponent<FactionMember>(obj);
                            addedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"{obj.name} doesn't have FactionMember component. Skipping.", obj);
                            skippedCount++;
                            continue;
                        }
                    }

                    Undo.RecordObject(factionMember, "Assign Faction");
                    factionMember.faction = selectedFaction;
                    EditorUtility.SetDirty(factionMember);
                    
                    if (PrefabUtility.IsPartOfPrefabInstance(obj))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(factionMember);
                    }

                    successCount++;
                    Debug.Log($"Assigned faction {selectedFaction} to {obj.name}", obj);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to assign faction to {obj.name}: {e.Message}", obj);
                    skippedCount++;
                }
            }

            string message = $"Faction Assignment Complete!\n\n" +
                           $"Assigned faction to: {successCount} objects\n" +
                           $"Added FactionMember to: {addedCount} objects\n" +
                           $"Skipped: {skippedCount} objects";

            EditorUtility.DisplayDialog("Batch Faction Assignment", message, "OK");
            
            AssetDatabase.SaveAssets();
        }
    }

    public static class BatchFactionQuickMenu
    {
        [MenuItem("GameObject/Faction/Assign Friendly Faction", false, 30)]
        public static void AssignFriendlyFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Friendly);
        }

        [MenuItem("GameObject/Faction/Assign Enemy Faction", false, 31)]
        public static void AssignEnemyFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Enemy);
        }

        [MenuItem("GameObject/Faction/Assign Player Faction", false, 32)]
        public static void AssignPlayerFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Player);
        }

        [MenuItem("GameObject/Faction/Assign Civilian Faction", false, 33)]
        public static void AssignCivilianFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Civilian);
        }

        [MenuItem("GameObject/Faction/Assign Rogue Faction", false, 34)]
        public static void AssignRogueFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Rogue);
        }

        [MenuItem("GameObject/Faction/Assign Neutral Faction", false, 35)]
        public static void AssignNeutralFaction()
        {
            AssignFactionToSelection(FactionManager.Faction.Neutral);
        }

        [MenuItem("GameObject/Faction/Assign Friendly Faction", true)]
        [MenuItem("GameObject/Faction/Assign Enemy Faction", true)]
        [MenuItem("GameObject/Faction/Assign Player Faction", true)]
        [MenuItem("GameObject/Faction/Assign Civilian Faction", true)]
        [MenuItem("GameObject/Faction/Assign Rogue Faction", true)]
        [MenuItem("GameObject/Faction/Assign Neutral Faction", true)]
        public static bool ValidateFactionAssignment()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        private static void AssignFactionToSelection(FactionManager.Faction faction)
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
                return;
            }

            int count = 0;
            int added = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null)
                    continue;

                FactionMember factionMember = obj.GetComponent<FactionMember>();
                
                if (factionMember == null)
                {
                    factionMember = Undo.AddComponent<FactionMember>(obj);
                    added++;
                }

                Undo.RecordObject(factionMember, "Assign Faction");
                factionMember.faction = faction;
                EditorUtility.SetDirty(factionMember);
                
                if (PrefabUtility.IsPartOfPrefabInstance(obj))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(factionMember);
                }

                count++;
            }

            string message = $"Assigned {faction} faction to {count} object(s).";
            if (added > 0)
            {
                message += $"\nAdded FactionMember component to {added} object(s).";
            }

            Debug.Log(message);
            EditorUtility.DisplayDialog("Faction Assigned", message, "OK");
            
            AssetDatabase.SaveAssets();
        }

        [MenuItem("GameObject/Faction/Add Faction Integration to Selected", false, 40)]
        public static void AddFactionIntegration()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
                return;
            }

            int added = 0;
            int skipped = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null)
                    continue;

                if (obj.GetComponent<JUTPSFactionIntegration>() != null)
                {
                    Debug.LogWarning($"{obj.name} already has JUTPSFactionIntegration. Skipping.", obj);
                    skipped++;
                    continue;
                }

                var integration = Undo.AddComponent<JUTPSFactionIntegration>(obj);
                EditorUtility.SetDirty(integration);
                
                if (PrefabUtility.IsPartOfPrefabInstance(obj))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(integration);
                }

                added++;
            }

            string message = $"Added JUTPSFactionIntegration to {added} object(s).";
            if (skipped > 0)
            {
                message += $"\nSkipped {skipped} object(s) that already had the component.";
            }

            Debug.Log(message);
            EditorUtility.DisplayDialog("Faction Integration Added", message, "OK");
            
            AssetDatabase.SaveAssets();
        }

        [MenuItem("GameObject/Faction/Add Faction Integration to Selected", true)]
        public static bool ValidateFactionIntegrationAdd()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }
    }
}
