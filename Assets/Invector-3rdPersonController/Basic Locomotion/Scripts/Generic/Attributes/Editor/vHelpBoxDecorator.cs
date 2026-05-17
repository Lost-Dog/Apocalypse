using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (vHelpBoxAttribute))]
public class vHelpBoxDecorator : DecoratorDrawer
{
    public Vector2 size;
    GUIStyle style;
    public override void OnGUI(Rect position)
    {
        EnsureStyle();
        
        var helpbox = attribute as vHelpBoxAttribute;      
       
        GUIContent content = new GUIContent(helpbox.text);       
        
        switch (helpbox.messageType)
        {
            case vHelpBoxAttribute.MessageType.Info:
                content = EditorGUIUtility.IconContent("console.infoicon", helpbox.text);
                break;
            case vHelpBoxAttribute.MessageType.Warning:
                content = EditorGUIUtility.IconContent("console.warnicon", helpbox.text);
                break;
        }       
        content.text = helpbox.text;
        style.richText = true;
        GUI.Box(position, content, style);      
    }

    public override float GetHeight()
    {
        var helpBoxAttribute = attribute as vHelpBoxAttribute;
        if (helpBoxAttribute == null) return base.GetHeight();
        EnsureStyle();
        if (style == null) return base.GetHeight();
        style.richText = true;
        return Mathf.Max(EditorGUIUtility.singleLineHeight, style.CalcHeight(new GUIContent(helpBoxAttribute.text), SafeViewWidth()) + 10);
    }

    private void EnsureStyle()
    {
        if (style != null) return;
        try { style = new GUIStyle(EditorStyles.helpBox); }
        catch { style = new GUIStyle(); }
    }

    private static float SafeViewWidth()
    {
        try { return EditorGUIUtility.currentViewWidth; }
        catch { return 350f; }
    }
}