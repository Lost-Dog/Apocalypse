using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GameCreator.Editor.Common;

namespace KingEdward.SkillTree.Editor
{
    [CustomEditor(typeof(SkillTreeUI))]
    public class SkillTreeUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SkillTreeUI skillTreeUI = target as SkillTreeUI;
            
            // Core References
            PropertyField skillTreeComponentField = new PropertyField(serializedObject.FindProperty("m_SkillTreeComponent"));
            root.Add(skillTreeComponentField);
            
            // Skill Item UIs
            PropertyField skillItemsField = new PropertyField(serializedObject.FindProperty("skillItems"));
            root.Add(skillItemsField);
            
            // Shared Tooltip
            PropertyField tooltipField = new PropertyField(serializedObject.FindProperty("sharedTooltip"));
            root.Add(tooltipField);
            
            root.Add(new VisualElement { style = { height = 10 } });
            
            // Gamepad / Navigation Section
            Label gamepadLabel = new Label("Gamepad / Navigation");
            gamepadLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(gamepadLabel);
            
            SerializedProperty gamepadModeProp = serializedObject.FindProperty("gamepadControlMode");
            SerializedProperty showTooltipOnSelectionProp = serializedObject.FindProperty("showTooltipOnSelection");
            
            PropertyField gamepadModeField = new PropertyField(gamepadModeProp);
            root.Add(gamepadModeField);
            
            root.Add(new PropertyField(serializedObject.FindProperty("hideCursorOnGamepad")));
            
            SerializedProperty selectionSouthEntersProp = serializedObject.FindProperty("selectionSouthEntersInnerButtons");
            VisualElement tooltipOnSelectionContainer = new VisualElement();
            tooltipOnSelectionContainer.Add(new PropertyField(showTooltipOnSelectionProp));
            tooltipOnSelectionContainer.Add(new PropertyField(selectionSouthEntersProp));
            root.Add(tooltipOnSelectionContainer);
            
            VisualElement cursorOptionsContainer = new VisualElement();
            cursorOptionsContainer.style.marginLeft = 15;
            cursorOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("cursorSpeed")));
            cursorOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("cursorGraphic")));
            root.Add(cursorOptionsContainer);
            
            var selectionHelp = new Label("Selection: South = enter inner buttons / submit. North (Y/Triangle) = click active button (Unlock/Level Up/Refund). East = back to nodes. Each SkillItemUI: 'North Clicks Active Button'.");
            selectionHelp.style.fontSize = 10;
            selectionHelp.style.color = new Color(0.6f, 0.6f, 0.6f);
            selectionHelp.style.marginTop = 2;
            selectionHelp.style.marginBottom = 4;
            selectionHelp.style.whiteSpace = WhiteSpace.Normal;
            var selectionHelpContainer = new VisualElement();
            selectionHelpContainer.style.marginLeft = 15;
            selectionHelpContainer.Add(selectionHelp);
            root.Add(selectionHelpContainer);
            
            void UpdateTooltipOnSelectionVisibility()
            {
                bool isSelection = gamepadModeProp.enumValueIndex == 0;
                bool southEnters = selectionSouthEntersProp.boolValue;
                tooltipOnSelectionContainer.style.display = isSelection ? DisplayStyle.Flex : DisplayStyle.None;
                cursorOptionsContainer.style.display = isSelection ? DisplayStyle.None : DisplayStyle.Flex;
                selectionHelpContainer.style.display = (isSelection && southEnters) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateTooltipOnSelectionVisibility();
            gamepadModeField.RegisterValueChangeCallback(evt => UpdateTooltipOnSelectionVisibility());
            root.TrackPropertyValue(selectionSouthEntersProp, _ => UpdateTooltipOnSelectionVisibility());
            
            root.Add(new VisualElement { style = { height = 10 } });
            
            // Connection Lines Section
            Label linesLabel = new Label("Connection Lines");
            linesLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(linesLabel);
            
            PropertyField drawLinesField = new PropertyField(serializedObject.FindProperty("drawConnectionLines"));
            root.Add(drawLinesField);
            
            // Line options container
            VisualElement lineOptionsContainer = new VisualElement();
            lineOptionsContainer.style.marginLeft = 15;
            
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("lineColor")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("lineWidth")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("lineRecess")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("taperedEnds")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("roundedEnds")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("curveType")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("curveIntensity")));
            lineOptionsContainer.Add(new PropertyField(serializedObject.FindProperty("lineResolution")));
            
            root.Add(lineOptionsContainer);
            
            // Show/hide line options based on drawConnectionLines
            void UpdateLineOptionsVisibility()
            {
                bool show = serializedObject.FindProperty("drawConnectionLines").boolValue;
                lineOptionsContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            UpdateLineOptionsVisibility();
            drawLinesField.RegisterValueChangeCallback(evt => UpdateLineOptionsVisibility());
            
            // Add space
            root.Add(new VisualElement { style = { height = 10 } });
            
            // Quick Actions section
            Label actionsLabel = new Label("Quick Actions");
            actionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionsLabel.style.marginTop = 10;
            root.Add(actionsLabel);
            
            // Create/Refresh button
            Button refreshButton = new Button(() =>
            {
                
                Transform linesContainer = skillTreeUI.transform.Find("ConnectionLines");
                if (linesContainer == null)
                {
                    GameObject containerGO = new GameObject("ConnectionLines");
                    linesContainer = containerGO.transform;
                    linesContainer.SetParent(skillTreeUI.transform, false);
                    
                    RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    
                    linesContainer.SetAsFirstSibling();
                }
                
                skillTreeUI.ForceRefreshConnections();
                EditorUtility.SetDirty(skillTreeUI);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(skillTreeUI.gameObject.scene);
            })
            {
                text = "Create/Refresh Connection Lines",
                style = { height = 30, marginTop = 5 }
            };
            root.Add(refreshButton);
            
            // Find All button
            Button findButton = new Button(() =>
            {
                skillTreeUI.FindAllSkillItems();
                EditorUtility.SetDirty(skillTreeUI);
            })
            {
                text = "Find All Skill Items",
                style = { marginTop = 5 }
            };
            root.Add(findButton);
            
            return root;
        }
    }
}
