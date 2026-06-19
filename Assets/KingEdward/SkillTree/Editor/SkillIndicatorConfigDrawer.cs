using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GameCreator.Editor.Common;
using KingEdward.SkillTree;

namespace KingEdward.SkillTree.Editor
{
    [CustomPropertyDrawer(typeof(SkillIndicatorConfig))]
    public class SkillIndicatorConfigDrawer : PropertyDrawer
    {
        private const string TYPE_PROP = "type";
        private const string M_RADIUS = "m_Radius";
        private const string M_MIN_RADIUS = "m_MinRadius";
        private const string M_MAX_RADIUS = "m_MaxRadius";
        private const string M_EXPAND_DURATION = "m_ExpandDuration";
        private const string M_CONE_ANGLE = "m_ConeAngle";
        private const string M_RANGE = "m_Range";
        private const string M_GROUND_OFFSET = "m_GroundOffset";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            SerializedProperty typeProp = property.FindPropertyRelative(TYPE_PROP);
            if (typeProp == null) { root.Add(new PropertyField(property)); return root; }

            var typeField = new PropertyField(typeProp);
            root.Add(typeField);

            VisualElement configContainer = new VisualElement();
            root.Add(configContainer);

            void Refresh()
            {
                configContainer.Clear();
                property.serializedObject.Update();
                int type = typeProp.enumValueIndex; // None=0, Circle=1, ExpandingCircle=2, Cone=3, Line=4

                if (type == (int)SkillIndicatorType.None)
                {
                    configContainer.Bind(property.serializedObject);
                    return;
                }

                Add(configContainer, property, M_GROUND_OFFSET);

                switch (type)
                {
                    case (int)SkillIndicatorType.Circle:
                        Add(configContainer, property, M_RADIUS);
                        break;
                    case (int)SkillIndicatorType.ExpandingCircle:
                        Add(configContainer, property, M_MIN_RADIUS);
                        Add(configContainer, property, M_MAX_RADIUS);
                        Add(configContainer, property, M_EXPAND_DURATION);
                        break;
                    case (int)SkillIndicatorType.Cone:
                        Add(configContainer, property, M_CONE_ANGLE);
                        Add(configContainer, property, M_RANGE);
                        break;
                    case (int)SkillIndicatorType.Line:
                        Add(configContainer, property, M_RANGE);
                        Add(configContainer, property, M_RADIUS); // line width
                        break;
                    case (int)SkillIndicatorType.ExpandingLine:
                        Add(configContainer, property, M_MIN_RADIUS); // min range
                        Add(configContainer, property, M_MAX_RADIUS); // max range
                        Add(configContainer, property, M_EXPAND_DURATION);
                        Add(configContainer, property, M_RADIUS); // line width
                        break;
                }

                Add(configContainer, property, "material");
                Add(configContainer, property, "color");

                if (type == (int)SkillIndicatorType.Circle || type == (int)SkillIndicatorType.ExpandingCircle)
                    Add(configContainer, property, "fixedAtCharacter");

                configContainer.Bind(property.serializedObject);
            }

            typeField.RegisterValueChangeCallback(_ => Refresh());
            Refresh();
            return root;
        }

        private static void Add(VisualElement parent, SerializedProperty parentProp, string relativeName)
        {
            var prop = parentProp.FindPropertyRelative(relativeName);
            if (prop != null)
                parent.Add(new PropertyField(prop));
        }
    }
}
