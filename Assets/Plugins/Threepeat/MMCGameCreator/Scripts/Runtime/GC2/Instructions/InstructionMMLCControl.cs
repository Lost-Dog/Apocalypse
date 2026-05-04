using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Threepeat
{
    [Version(0,1,0)]

    [Title("Control MMLC")]
    [Description("Allows blending between MMLC/GC2 and initializing MMLC (when in manually initiate mode)")]

    [Category("MMLC/MMLC Control")]
    
    [Parameter("Character", "The character to control")]
    [Parameter("Action to Perform", "Only use ManuallyInitialize if that box is checked in the MMCGameCreator component")]
    [Parameter("Blend Time", "duration of blending period when switching between MMLC and GC2")]

    [Serializable]
    public class InstructionMMLCControl : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private Operation m_actionToPerform;
        [SerializeField] private float m_blendTime = 0.1f;
        public enum Operation
        {
            [InspectorName("Transition to Game Creator")]
            Transition_To_GameCreator,
            [InspectorName("Transition to MMLC")]
            Transition_To_MMLC,
            [InspectorName("Manually Initialize")]
            Manually_Initialize
        }

        public override string Title => $"MMLC Control: {this.m_Character} - {this.m_actionToPerform.ToString().Replace("_", " ")}";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);

            if (character == null)
            {
                Debug.LogError("ActionMMLCControl: selected character does not have a Character component.");
            }

            MMCGameCreator2 mmcgc = character.GetComponent<MMCGameCreator2>();

            if (mmcgc == null)
            {
                Debug.LogError("ActionMMLCControl: selected character does not have an MMCGameCreator component.");
                return DefaultResult;
            }
            switch (m_actionToPerform)
            {
                case Operation.Transition_To_GameCreator:
                    mmcgc.SetMxMAnimatorBlendWeight(0f, m_blendTime, true);
                    break;
                case Operation.Transition_To_MMLC:
                    mmcgc.SetMxMAnimatorBlendWeight(1f, m_blendTime, false);
                    break;
                case Operation.Manually_Initialize:
                    mmcgc.Initialize();
                    break;
            }

            return DefaultResult;
        }
    }
}