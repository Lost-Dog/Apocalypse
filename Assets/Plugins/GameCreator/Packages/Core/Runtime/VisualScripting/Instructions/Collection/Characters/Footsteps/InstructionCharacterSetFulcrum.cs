using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Set Fulcrum")]
    [Description("Changes the Fulcrum value of a Character")]

    [Category("Characters/Footsteps/Set Character Fulcrum")]

    [Parameter("Character", "The character targeted")]
    [Parameter("Fulcrum", "The new Fulcrum value")]

    [Keywords("Character", "Foot", "Step", "Stomp", "Foliage", "Audio", "Run", "Walk", "Move")]
    [Image(typeof(IconFootprint), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class InstructionCharacterSetFulcrum : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetDecimal m_Fulcrum = new PropertyGetDecimal(-0.85f);

        public override string Title => $"Fulcrum of {this.m_Character} = {this.m_Fulcrum}";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;
            
            if (character.Footsteps.FootstepDetector is FootstepDetectorFulcrum fulcrumDetector)
            {
                fulcrumDetector.Fulcrum = (float) this.m_Fulcrum.Get(args);
            }
            
            return DefaultResult;
        }
    }
}