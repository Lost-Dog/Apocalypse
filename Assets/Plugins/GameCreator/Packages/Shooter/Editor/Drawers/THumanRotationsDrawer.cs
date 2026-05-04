using GameCreator.Editor.Common;
using GameCreator.Runtime.Shooter;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Shooter
{
    [CustomPropertyDrawer(typeof(THumanRotations), true)]
    public class THumanRotationsDrawer : TSectionDrawer
    {
        protected override string Name(SerializedProperty property) => property.displayName;
    }
}