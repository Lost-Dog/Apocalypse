using System.Collections.Generic;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Editor.Traversal
{
    public class MotionActionTool : TPolymorphicItemTool
    {
        private static readonly IIcon ICON = new IconCharacterGesture(ColorTheme.Type.Blue);
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override List<string> CustomStyleSheetPaths => new List<string>();

        protected override object Value => null;

        public override string Title => this.m_Property
            .FindPropertyRelative("m_Id")
            .FindPropertyRelative("m_String").stringValue;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public MotionActionTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
        
        // IMPLEMENTATIONS: -----------------------------------------------------------------------

        protected override Texture2D GetIcon()
        {
            return ICON.Texture;
        }
    }
}