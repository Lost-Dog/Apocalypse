using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Toasts
{
    [Version(0, 0, 1)]

    [Title("Show Toast")]
    [Description("Shows a Toast message")]

    [Category("Toasts/Show Toast")]

    [Keywords("Notification", "Alert", "Message", "Notify", "Open", "Push")]
    [Image(typeof(IconSquareSolid), ColorTheme.Type.Green, typeof(OverlayArrowUp))]
    
    [Serializable]
    public class InstructionToastsPush : Instruction
    {
        [SerializeField] private IdString m_PanelId = IdString.EMPTY;
        
        [SerializeField] private PropertyGetString m_Text = new PropertyGetString();
        [SerializeField] private PropertyGetSprite m_Icon = new PropertyGetSprite();
        [SerializeField] private PropertyGetColor m_Color = new PropertyGetColor();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Show Toast: {this.m_Text}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            ToastsManager.Instance.Push(
                this.m_PanelId,
                this.m_Text.Get(args),
                this.m_Icon.Get(args),
                this.m_Color.Get(args)
            );
            
            return DefaultResult;
        }
    }
}