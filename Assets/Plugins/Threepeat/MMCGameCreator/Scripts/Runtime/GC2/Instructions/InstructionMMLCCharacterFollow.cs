using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using MxM;
using UnityEngine;

namespace Threepeat
{
    [Version(0, 1, 0)]

    [Title("MMLC Character Follow")]
    [Description("Starts/stops NavMesh-based character follows (both when GC2 is enabled and when MMLC is enabled.")]

    [Category("MMLC/Character Follow (MMLC+GC2)")]

    [Parameter("Character", "The character to modify")]
    [Parameter("Target", "The target to follow")]
    [Parameter("Operation", "what operation to perform")]
    [Parameter("Min Distance", "minimum follow distance")]
    [Parameter("Max Distance", "maximum follow distance")]

    [Serializable]
    public class InstructionMMLCCharacterFollow : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_Target;
        [SerializeField] private Operation m_Operation = Operation.StartFollow;
        [SerializeField] private float m_MinDistance = 2f;
        [SerializeField] private float m_MaxDistance = 5f;


        public enum Operation
        {
            StartFollow,
            StopFollow
        }

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null)
            {
                Debug.LogError("ChangeCharacterConfig: selected character does not have a Character component.");
            }

            NGCharacter ngchar = character.GetComponent<NGCharacter>();

            if (ngchar == null)
            {
                return DefaultResult;
            }

            MMCGameCreator2 mmcgc = ngchar.GetComponent<MMCGameCreator2>();
            GameObject target = this.m_Target.Get(args);


            if ((mmcgc != null) && mmcgc.MMLCCurrentlyEnabled)
            {
                // set/clear character target
                NGInputScheme_NavMesh nmis = (NGInputScheme_NavMesh)ngchar.InputScheme;
                if (nmis != null)
                {
                    if (m_Operation == Operation.StartFollow)
                    {
                        nmis.SetTarget(target.transform);
                    }
                    else
                    {
                        nmis.SetTarget(null);
                    }
                }
            }
            else
            {
                if (target == null) return DefaultResult;

                if (m_Operation == Operation.StartFollow)
                {
                    character.Motion.StartFollowingTarget(target.transform, m_MinDistance, m_MaxDistance);
                }
                else
                {
                    character.Motion.StopFollowingTarget();
                }

            }

            return DefaultResult;
        }
    }
}