using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 0)]
    [Title("Stop Random Gestures")]
    [Description("Stops any random gesture sequence currently playing on the Character")]
    [Category("Characters/Animation/Stop Random Gestures")]

    [Parameter("Character", "The character that is playing random gestures")]
    [Parameter("Delay", "Amount of seconds to wait before the animation starts to blend out")]
    [Parameter("Transition", "The amount of seconds the animation takes to blend out")]

    [Keywords("Characters", "Animation", "Animate", "Gesture", "Stop", "Random")]
    [Image(typeof(IconCharacterGesture), ColorTheme.Type.TextLight, typeof(OverlayCross))]
    [Serializable]
    public class InstructionCharacterStopRandomGestures : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [Space]
        [SerializeField] private PropertyGetDecimal m_Delay = GetDecimalConstantZero.Create;
        [SerializeField] private PropertyGetDecimal m_Transition = new PropertyGetDecimal(0.1f);

        public override string Title => $"Stop random gestures on {this.m_Character}";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            RandomGestureController.Cancel(character, (float)this.m_Delay.Get(args), (float)this.m_Transition.Get(args));

            return DefaultResult;
        }
    }
}
