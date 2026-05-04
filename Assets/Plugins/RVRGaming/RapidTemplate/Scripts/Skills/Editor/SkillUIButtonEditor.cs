using GameCreator.Editor.Common;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(SkillUIButton))]
public class MyScriptEditorSkillUIButton : Editor
{
    Texture2D headerImage;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        var header = new Image() { image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/RVRGaming/RapidTemplate/Textures/Editor/HeaderImageSkillUIButton.png") };
        header.style.maxHeight = 34;
        header.style.maxWidth = 200;
        header.style.marginLeft = 0;
        root.Add(header);

        root.Add(new SpaceSmall());

        root.Add(new PropertyField(serializedObject.FindProperty("item")));
        root.Add(new PropertyField(serializedObject.FindProperty("Active")));
        root.Add(new SpaceSmall());
  
        var labelOnCheck = new Label("Unlock Conditions:");
        root.Add(labelOnCheck);
        var fieldOnCheck = new PropertyField(serializedObject.FindProperty("onCheck"));
        fieldOnCheck.name = "onCheckField";
        root.Add(fieldOnCheck);
        root.Add(new SpaceSmall());
        var labelOnUnlock = new Label("Unlock Actions:");
        root.Add(labelOnUnlock);
        var fieldOnUnlock = new PropertyField(serializedObject.FindProperty("onUnlock"));
        fieldOnUnlock.name = "onUnlockField";
        root.Add(fieldOnUnlock);

        root.Add(new SpaceSmall());
        root.Add(new PropertyField(serializedObject.FindProperty("icon")));
        root.Add(new PropertyField(serializedObject.FindProperty("bgPassive")));
        root.Add(new PropertyField(serializedObject.FindProperty("cdPassive")));
        root.Add(new PropertyField(serializedObject.FindProperty("bgActive")));
        root.Add(new PropertyField(serializedObject.FindProperty("cdActive")));
        root.Add(new SpaceSmall());
        root.Add(new PropertyField(serializedObject.FindProperty("popupPrefab")));
        root.Add(new PropertyField(serializedObject.FindProperty("popupPosition")));
        root.Add(new PropertyField(serializedObject.FindProperty("descriptionImage")));
        root.Add(new PropertyField(serializedObject.FindProperty("unlockCost")));
        root.Add(new SpaceSmall());
        root.Add(new PropertyField(serializedObject.FindProperty("lockColor")));
        root.Add(new PropertyField(serializedObject.FindProperty("unlockColor")));
        root.Add(new SpaceSmall());
        root.Add(new PropertyField(serializedObject.FindProperty("m_BagGetter")));
        root.Add(new PropertyField(serializedObject.FindProperty("m_EquipmentIndex")));
        root.Add(new SpaceSmall());

        return root;
    }
}