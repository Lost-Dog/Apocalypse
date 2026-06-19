using System.Collections;
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

    [Title("MMLC Teleport")]
    [Description("Teleports Character.")]

    [Category("MMLC/Teleport (MMLC)")]

    [Parameter("Character", "The character to teleport")]
    [Parameter("Location", "The teleport target location")]
    [Parameter("Rotate", "whether to rotate character")]

    public class InstructionMMLCTeleport : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetLocation m_Location = GetLocationNavigationMarker.Create;
        [SerializeField] private bool m_Rotate = false;

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            CharacterController ctrl = character.GetComponent<CharacterController>();
            bool wasEnabled = ctrl.enabled;
            ctrl.enabled = false;

            Location location = this.m_Location.Get(args);

            Vector3 position = location.GetPosition(character.gameObject);

            ctrl.transform.position = position;

            if (m_Rotate)
            {
                Quaternion rotation = location.GetRotation(character.gameObject);
                ctrl.transform.rotation = rotation;
            }
            ctrl.enabled = wasEnabled;

            return DefaultResult;
        }
    }
}
