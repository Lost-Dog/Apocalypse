using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Title("Instantiate (Register Projectile Caster)")]
    [Description("Creates a new instance of a game object. If the instance has a ProjectileBehavior, sets its caster to the instruction's Self (e.g. the character who triggered the skill).")]

    [Category("KingEdward/Skill Tree/VFX/Instantiate Projectile")]

    [Parameter("Game Object", "Game Object reference that is instantiated")]
    [Parameter("Position", "The position of the new game object instance")]
    [Parameter("Rotation", "The rotation of the new game object instance")]
    [Parameter("Save", "Optional value where the newly instantiated game object is stored")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]

    [Keywords("Create", "Instantiate", "Projectile", "Caster")]
    [Serializable]
    public class InstructionGameObjectInstantiateWithCaster : Instruction
    {
        [SerializeField] private PropertyGetInstantiate m_GameObject = new PropertyGetInstantiate();
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
        [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [SerializeField] private PropertySetGameObject m_Save = SetGameObjectNone.Create;

        public override string Title => $"Instantiate {this.m_GameObject} (Register Caster)";

        protected override Task Run(Args args)
        {
            Vector3 position = this.m_Position.Get(args);
            Quaternion rotation = this.m_Rotation.Get(args);
            GameObject instance = this.m_GameObject.Get(args, position, rotation);

            if (instance != null)
            {
                ProjectileBehavior.RegisterCasterFromArgs(instance, args);
                Transform parent = this.m_Parent.Get<Transform>(args);
                if (parent != null) instance.transform.SetParent(parent);
                this.m_Save.Set(instance, args);
            }

            return DefaultResult;
        }
    }
}
