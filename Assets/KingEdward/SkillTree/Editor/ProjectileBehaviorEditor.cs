using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Editor
{
    [CustomEditor(typeof(ProjectileBehavior))]
    public class ProjectileBehaviorEditor : UnityEditor.Editor
{
    private Dictionary<ProjectileBehavior.ProjectileType, VisualElement> typeContainers = new Dictionary<ProjectileBehavior.ProjectileType, VisualElement>();
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        
        // Basic Settings
        root.Add(CreateLabel("Basic Settings"));
        root.Add(new PropertyField(serializedObject.FindProperty("m_Target")));
        root.Add(new PropertyField(serializedObject.FindProperty("speed")));
        root.Add(new PropertyField(serializedObject.FindProperty("lifetime")));
        root.Add(new PropertyField(serializedObject.FindProperty("startDelay")));
        
        root.Add(CreateSpace());
        
        // Movement Type
        root.Add(CreateLabel("Movement Type"));
        SerializedProperty projectileType = serializedObject.FindProperty("projectileType");
        PropertyField projectileTypeField = new PropertyField(projectileType);
        root.Add(projectileTypeField);
        
        // Rotation settings
        PropertyField rotateToMovementField = new PropertyField(serializedObject.FindProperty("rotateToMovement"));
        PropertyField rotationSpeedField = new PropertyField(serializedObject.FindProperty("rotationSpeed"));
        root.Add(rotateToMovementField);
        root.Add(rotationSpeedField);
        
        root.Add(CreateSpace());
        
        // Type Settings Label
        root.Add(CreateLabel("Type Settings"));
        
        // Create all type-specific containers upfront
        CreateAllTypeContainers(root);
        
        root.Add(CreateSpace());
        
        // Hit Settings
        root.Add(CreateLabel("Hit Settings"));
        root.Add(new PropertyField(serializedObject.FindProperty("destroyOnHit")));
        root.Add(new PropertyField(serializedObject.FindProperty("hitRadius")));
        root.Add(new PropertyField(serializedObject.FindProperty("hitLayers")));
        root.Add(new PropertyField(serializedObject.FindProperty("hitTags")));
        
        root.Add(CreateSpace());
        
        // On Hit
        root.Add(CreateLabel("On Hit"));
        root.Add(new PropertyField(serializedObject.FindProperty("m_OnHitInstructions")));
        
        root.Add(CreateSpace());
        
        // Can Hit
        root.Add(CreateLabel("Can Hit"));
        root.Add(new PropertyField(serializedObject.FindProperty("m_CanHitConditions")));
        
        // Update visibility based on projectile type
        void UpdateVisibility()
        {
            serializedObject.Update();
            ProjectileBehavior.ProjectileType type = (ProjectileBehavior.ProjectileType)projectileType.enumValueIndex;
            
            // Update rotation visibility
            bool showRotation = type != ProjectileBehavior.ProjectileType.Boomerang;
            rotateToMovementField.style.display = showRotation ? DisplayStyle.Flex : DisplayStyle.None;
            
            bool showRotationSpeed = showRotation && serializedObject.FindProperty("rotateToMovement").boolValue;
            rotationSpeedField.style.display = showRotationSpeed ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Show/hide type-specific containers
            foreach (var kvp in typeContainers)
            {
                kvp.Value.style.display = (kvp.Key == type) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        // Initial update
        UpdateVisibility();
        
        projectileTypeField.RegisterValueChangeCallback(evt => 
        {
            serializedObject.ApplyModifiedProperties();
            UpdateVisibility();
        });
        
        rotateToMovementField.RegisterValueChangeCallback(evt => 
        {
            serializedObject.ApplyModifiedProperties();
            UpdateVisibility();
        });
        
        return root;
    }
    
    private void CreateAllTypeContainers(VisualElement root)
    {
        typeContainers.Clear();
        
        // Straight
        VisualElement straightContainer = new VisualElement();
        straightContainer.Add(new HelpBox("Projectile moves in a straight line.", HelpBoxMessageType.Info));
        typeContainers[ProjectileBehavior.ProjectileType.Straight] = straightContainer;
        root.Add(straightContainer);
        
        // Curve
        VisualElement curveContainer = new VisualElement();
        curveContainer.Add(new PropertyField(serializedObject.FindProperty("curveHeight")));
        curveContainer.Add(new PropertyField(serializedObject.FindProperty("curveShape")));
        
        SerializedProperty curveDirection = serializedObject.FindProperty("curveDirection");
        PropertyField curveDirectionField = new PropertyField(curveDirection);
        curveContainer.Add(curveDirectionField);
        
        PropertyField customAngleField = new PropertyField(serializedObject.FindProperty("customAngle"));
        curveContainer.Add(customAngleField);
        
        curveDirectionField.RegisterValueChangeCallback(evt =>
        {
            bool showCustomAngle = (ProjectileBehavior.CurveDirection)curveDirection.enumValueIndex == ProjectileBehavior.CurveDirection.Custom;
            customAngleField.style.display = showCustomAngle ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        typeContainers[ProjectileBehavior.ProjectileType.Curve] = curveContainer;
        root.Add(curveContainer);
        
        // Spiral
        VisualElement spiralContainer = new VisualElement();
        spiralContainer.Add(new PropertyField(serializedObject.FindProperty("spiralRadius")));
        spiralContainer.Add(new PropertyField(serializedObject.FindProperty("spiralSpeed")));
        spiralContainer.Add(new PropertyField(serializedObject.FindProperty("spiralTightness")));
        typeContainers[ProjectileBehavior.ProjectileType.Spiral] = spiralContainer;
        root.Add(spiralContainer);
        
        // Homing
        VisualElement homingContainer = new VisualElement();
        homingContainer.Add(new PropertyField(serializedObject.FindProperty("homingStrength")));
        homingContainer.Add(new PropertyField(serializedObject.FindProperty("homingDelay")));
        homingContainer.Add(new PropertyField(serializedObject.FindProperty("maxTurnRate")));
        typeContainers[ProjectileBehavior.ProjectileType.Homing] = homingContainer;
        root.Add(homingContainer);
        
        // Wave
        VisualElement waveContainer = new VisualElement();
        waveContainer.Add(new PropertyField(serializedObject.FindProperty("waveAmplitude")));
        waveContainer.Add(new PropertyField(serializedObject.FindProperty("waveFrequency")));
        typeContainers[ProjectileBehavior.ProjectileType.Wave] = waveContainer;
        root.Add(waveContainer);
        
        // Boomerang
        VisualElement boomerangContainer = new VisualElement();
        boomerangContainer.Add(new PropertyField(serializedObject.FindProperty("boomerangRange")));
        boomerangContainer.Add(new PropertyField(serializedObject.FindProperty("boomerangReturnSpeed")));
        boomerangContainer.Add(new PropertyField(serializedObject.FindProperty("boomerangCurvature")));
        boomerangContainer.Add(new PropertyField(serializedObject.FindProperty("destroyOnComplete")));
        
        SerializedProperty followTarget = serializedObject.FindProperty("followTargetOnReturn");
        PropertyField followTargetField = new PropertyField(followTarget);
        boomerangContainer.Add(followTargetField);
        
        PropertyField returnTargetField = new PropertyField(serializedObject.FindProperty("m_ReturnTarget"));
        boomerangContainer.Add(returnTargetField);
        
        followTargetField.RegisterValueChangeCallback(evt =>
        {
            returnTargetField.style.display = followTarget.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        typeContainers[ProjectileBehavior.ProjectileType.Boomerang] = boomerangContainer;
        root.Add(boomerangContainer);
        
        // Orbit
        VisualElement orbitContainer = new VisualElement();
        orbitContainer.Add(new PropertyField(serializedObject.FindProperty("orbitRadius")));
        orbitContainer.Add(new PropertyField(serializedObject.FindProperty("orbitSpeed")));
        orbitContainer.Add(new PropertyField(serializedObject.FindProperty("orbitCount")));
        typeContainers[ProjectileBehavior.ProjectileType.Orbit] = orbitContainer;
        root.Add(orbitContainer);
        
        // Zigzag
        VisualElement zigzagContainer = new VisualElement();
        zigzagContainer.Add(new PropertyField(serializedObject.FindProperty("zigzagAmplitude")));
        zigzagContainer.Add(new PropertyField(serializedObject.FindProperty("zigzagFrequency")));
        typeContainers[ProjectileBehavior.ProjectileType.Zigzag] = zigzagContainer;
        root.Add(zigzagContainer);
        
        // Artillery
        VisualElement artilleryContainer = new VisualElement();
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryHeight")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryArcTime")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryArcCurve")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryUseGravity")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryLockTargetAt")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryStartCurveAt")));
        artilleryContainer.Add(new PropertyField(serializedObject.FindProperty("artilleryGroundOffset")));
        typeContainers[ProjectileBehavior.ProjectileType.Artillery] = artilleryContainer;
        root.Add(artilleryContainer);
    }
    
    private void BuildTypeSpecificSettings_OLD(VisualElement container, ProjectileBehavior.ProjectileType type)
    {
        switch (type)
        {
            case ProjectileBehavior.ProjectileType.Straight:
                HelpBox helpBox = new HelpBox("Projectile moves in a straight line.", HelpBoxMessageType.Info);
                container.Add(helpBox);
                break;
                
            case ProjectileBehavior.ProjectileType.Curve:
                container.Add(new PropertyField(serializedObject.FindProperty("curveHeight")));
                container.Add(new PropertyField(serializedObject.FindProperty("curveShape")));
                
                SerializedProperty curveDirection = serializedObject.FindProperty("curveDirection");
                PropertyField curveDirectionField = new PropertyField(curveDirection);
                container.Add(curveDirectionField);
                
                PropertyField customAngleField = new PropertyField(serializedObject.FindProperty("customAngle"));
                container.Add(customAngleField);
                
                void UpdateCurveAngle()
                {
                    bool showCustomAngle = (ProjectileBehavior.CurveDirection)curveDirection.enumValueIndex == ProjectileBehavior.CurveDirection.Custom;
                    customAngleField.style.display = showCustomAngle ? DisplayStyle.Flex : DisplayStyle.None;
                }
                
                UpdateCurveAngle();
                curveDirectionField.RegisterValueChangeCallback(evt => UpdateCurveAngle());
                break;
                
            case ProjectileBehavior.ProjectileType.Spiral:
                container.Add(new PropertyField(serializedObject.FindProperty("spiralRadius")));
                container.Add(new PropertyField(serializedObject.FindProperty("spiralSpeed")));
                container.Add(new PropertyField(serializedObject.FindProperty("spiralTightness")));
                break;
                
            case ProjectileBehavior.ProjectileType.Homing:
                container.Add(new PropertyField(serializedObject.FindProperty("homingStrength")));
                container.Add(new PropertyField(serializedObject.FindProperty("homingDelay")));
                container.Add(new PropertyField(serializedObject.FindProperty("maxTurnRate")));
                break;
                
            case ProjectileBehavior.ProjectileType.Wave:
                container.Add(new PropertyField(serializedObject.FindProperty("waveAmplitude")));
                container.Add(new PropertyField(serializedObject.FindProperty("waveFrequency")));
                break;
                
            case ProjectileBehavior.ProjectileType.Boomerang:
                container.Add(new PropertyField(serializedObject.FindProperty("boomerangRange")));
                container.Add(new PropertyField(serializedObject.FindProperty("boomerangReturnSpeed")));
                container.Add(new PropertyField(serializedObject.FindProperty("boomerangCurvature")));
                container.Add(new PropertyField(serializedObject.FindProperty("destroyOnComplete")));
                
                SerializedProperty followTarget = serializedObject.FindProperty("followTargetOnReturn");
                PropertyField followTargetField = new PropertyField(followTarget);
                container.Add(followTargetField);
                
                PropertyField returnTargetField = new PropertyField(serializedObject.FindProperty("m_ReturnTarget"));
                container.Add(returnTargetField);
                
                void UpdateReturnTarget()
                {
                    returnTargetField.style.display = followTarget.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
                
                UpdateReturnTarget();
                followTargetField.RegisterValueChangeCallback(evt => UpdateReturnTarget());
                break;
                
            case ProjectileBehavior.ProjectileType.Orbit:
                container.Add(new PropertyField(serializedObject.FindProperty("orbitRadius")));
                container.Add(new PropertyField(serializedObject.FindProperty("orbitSpeed")));
                container.Add(new PropertyField(serializedObject.FindProperty("orbitCount")));
                break;
                
            case ProjectileBehavior.ProjectileType.Zigzag:
                container.Add(new PropertyField(serializedObject.FindProperty("zigzagAmplitude")));
                container.Add(new PropertyField(serializedObject.FindProperty("zigzagFrequency")));
                break;
                
            case ProjectileBehavior.ProjectileType.Artillery:
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryHeight")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryArcTime")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryArcCurve")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryUseGravity")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryLockTargetAt")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryStartCurveAt")));
                container.Add(new PropertyField(serializedObject.FindProperty("artilleryGroundOffset")));
                break;
        }
    }
    
    private Label CreateLabel(string text)
    {
        Label label = new Label(text);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.marginTop = 5;
        label.style.marginBottom = 2;
        return label;
    }
    
    private VisualElement CreateSpace()
    {
        VisualElement space = new VisualElement();
        space.style.height = 10;
        return space;
    }
}
}
