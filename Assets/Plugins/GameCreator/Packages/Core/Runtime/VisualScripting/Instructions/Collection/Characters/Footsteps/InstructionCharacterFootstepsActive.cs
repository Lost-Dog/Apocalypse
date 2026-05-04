using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Footsteps Set Active")]
    [Description("Changes whether a Character plays footstep events or not")]

    [Category("Characters/Footsteps/Footsteps Set Active")]

    [Parameter("Character", "The character targeted")]
    [Parameter("Active", "Whether the footstep events are executed or not")]

    [Keywords("Character", "Foot", "Step", "Stomp", "Foliage", "Audio", "Run", "Walk", "Move")]
    [Image(typeof(IconFootprint), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class InstructionCharacterFootstepsActive : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetBool m_Active = new PropertyGetBool(true);

        public override string Title => $"Footstep of {this.m_Character} = {this.m_Active}";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            character.Footsteps.IsActive = this.m_Active.Get(args);
            return DefaultResult;
        }
    }
}