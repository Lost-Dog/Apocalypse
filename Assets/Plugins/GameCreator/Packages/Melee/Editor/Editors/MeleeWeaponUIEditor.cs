using GameCreator.Editor.Common;
using GameCreator.Runtime.Melee;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Melee
{
    [CustomEditor(typeof(MeleeWeaponUI))]
    public class MeleeWeaponUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            root.Add(new PropertyField(this.serializedObject.FindProperty("m_Character")));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_WeaponName"), "Name"));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_WeaponDescription"), "Description"));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_WeaponIcon"), "Icon"));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_WeaponColor"), "Color"));
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ActiveHasWeapon")));
            root.Add(new PropertyField(this.serializedObject.FindProperty("m_ActiveHasNoWeapon")));
            
            return root;
        }
    }
}