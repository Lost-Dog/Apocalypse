using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Traversal
{
    [CustomPropertyDrawer(typeof(Connection))]
    public class ConnectionDrawer : PropertyDrawer
    {
        private const string NAME_TRAVERSE = "Traverse To";
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            
            root.Add(new PropertyField(property.FindPropertyRelative("m_Traverse"), NAME_TRAVERSE)
            {
                style =
                {
                    marginTop = new Length(3, LengthUnit.Pixel),
                    marginRight = new Length(5, LengthUnit.Pixel)
                }
            });
            root.Add(new PropertyField(property.FindPropertyRelative("m_MaxDistance"))
            {
                style =
                {
                    marginBottom = new Length(3, LengthUnit.Pixel),
                    marginRight = new Length(5, LengthUnit.Pixel)
                }
            });
            
            return root;
        }
    }
}