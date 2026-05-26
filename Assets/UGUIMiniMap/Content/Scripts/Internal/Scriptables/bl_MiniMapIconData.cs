using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lovatto.MiniMap
{
    [CreateAssetMenu(menuName = "MiniMap/Icon Data", fileName = "Icon Data")]
    public class bl_MiniMapIconData : ScriptableObject
    {
        public Sprite Icon = null;
        public Sprite DeathIcon = null;
        public Color IconColor = new Color(1, 1, 1, 0.9f);

        public MiniMapIconSizeType sizeType = MiniMapIconSizeType.SizeDelta;
        [Range(0.1f, 100)] public float Size = 20;
        [Range(0.1f, 100)] public float OffScreenSize = 15;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_MiniMapIconData))]
    public class bl_MiniMapIconDataEditor : Editor
    {
        bl_MiniMapIconData script;

        private void OnEnable()
        {
            script = (bl_MiniMapIconData)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            script.Icon = EditorGUILayout.ObjectField("Icon", script.Icon, typeof(Sprite), false) as Sprite;
            script.DeathIcon = EditorGUILayout.ObjectField("Death Icon", script.DeathIcon, typeof(Sprite), false) as Sprite;
            script.IconColor = EditorGUILayout.ColorField("Color", script.IconColor);
            script.sizeType = (MiniMapIconSizeType)EditorGUILayout.EnumPopup("Size Type", script.sizeType);
            if (script.sizeType == MiniMapIconSizeType.SizeDelta) script.Size = EditorGUILayout.Slider("Size", script.Size, 0.1f, 100);
            else script.Size = EditorGUILayout.Slider("Size", script.Size, 0.1f, 10);
            script.OffScreenSize = EditorGUILayout.Slider("OffScreen Size", script.OffScreenSize, 0.1f, 100);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}