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

    [Title("MMLC Change Motion-Matching AnimData")]
    [Description("Changes MxM AnimData.  AnimData must already be in the MxMAnimator's animData list for this runtime change to work.")]

    [Category("MMLC/Change Motion-Matching AnimData")]

    [Parameter("Character", "The character to modify")]
    [Parameter("New Animation Data", "New animation data")]


    [Serializable]
    public class InstructionMMLCChangeAnimationData : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private MxMAnimData newAnimationData;

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null)
            {
                Debug.LogError("ActionMMLCControl: selected character does not have a Character component.");
            }

            MMCGameCreator2 mmcgc = character.GetComponent<MMCGameCreator2>();
            //mmcgc.mxmAnimator.AnimData = new MxMAnimData[] { newAnimationData };

            for (int ii = 0; ii < mmcgc.mxmAnimator.AnimData.Length; ii++)
            {
                if (mmcgc.mxmAnimator.AnimData[ii] == newAnimationData)
                {
                    Debug.Log($"Changing animData: {newAnimationData.name}");
                    mmcgc.mxmAnimator.SwapAnimData(ii);
                    break;
                }
            }

            return DefaultResult;
        }
    }
}