
#if ABC_GC_2_Integration
namespace GameCreator.Core {
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using ABCToolkit;
    using GameCreator.Runtime.Characters;
    using GameCreator.Runtime.Common;
    using GameCreator.Runtime.VisualScripting;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    [Version(0, 1, 1)]

    [Title("Activate ABC Weapon Block")]
    [Category("ABC/Activate Weapon Block")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Activates ABC Weapon Block")]

    [Parameter("Target", "The targeted game object to activate the weapon block")]
    [Parameter("Block Duration", "The duration of the weapon block")]
    [Parameter("Block Cooldown", "The cooldown till another weapon block can be activated")]


    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Block")]

    [Serializable]
    public class InstructionABCActivateWeaponBlock : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private float m_BlockDuration;
        [SerializeField] private float m_BlockCooldown;

        public override string Title => string.Format(
            "Activate {0} ABC Weapon Block",
            this.m_Target
        );


        protected override Task Run(Args args) {
            GameObject target = this.m_Target.Get(args);

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(target);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return DefaultResult;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

                entity.ActivateWeaponBlock(this.m_BlockDuration, this.m_BlockCooldown);


            return DefaultResult;
        }


    

    }
}
#endif