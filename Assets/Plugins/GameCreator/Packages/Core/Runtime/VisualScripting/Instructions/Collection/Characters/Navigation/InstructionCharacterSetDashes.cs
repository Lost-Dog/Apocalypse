using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Set Dashes")]
    [Description("Sets the number of consecutive dashes of a Character")]

    [Category("Characters/Navigation/Set Dashes")]
    
    [Keywords("Flash", "Roll", "Reach")]
    [Image(typeof(IconCharacterDash), ColorTheme.Type.Yellow)]

    [Serializable]
    public class InstructionCharacterSetDashes : TInstructionCharacterNavigation
    {
        [SerializeField] private PropertyGetInteger m_Dashes = new PropertyGetInteger(2);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Dashes of {this.m_Character} = {this.m_Dashes}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            character.Dash.NumDashes = Mathf.Max((int) this.m_Dashes.Get(args), 0);
            return DefaultResult;
        }
    }
}