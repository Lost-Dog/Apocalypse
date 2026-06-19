using GameCreator.Runtime.Toasts;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Toasts
{
    [CustomEditor(typeof(ToastsManager))]
    public class ToastsManagerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new VisualElement();
        }
    }
}