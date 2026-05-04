using GameCreator.Editor.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(RopeConfig))]
    public class RopeConfigDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Config";

        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            container.Add(new PropertyField(property.FindPropertyRelative("throwHeight")));
            container.Add(new PropertyField(property.FindPropertyRelative("throwHeightEasing")));
            
            container.Add(new SpaceSmaller());
            container.Add(new PropertyField(property.FindPropertyRelative("looseTensionFactor")));
            container.Add(new PropertyField(property.FindPropertyRelative("tightTensionFactor")));
            
            container.Add(new SpaceSmaller());
            container.Add(new PropertyField(property.FindPropertyRelative("reelChaosX")));
            container.Add(new PropertyField(property.FindPropertyRelative("reelChaosY")));
            container.Add(new PropertyField(property.FindPropertyRelative("reelChaosMagnitude")));
        }
    }
}