
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

    [Title("Activate ABC Weapon Parry")]
    [Category("ABC/Activate Weapon Parry")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Activates ABC Weapon Parry")]

    [Parameter("Target", "The targeted game object to activate the weapon Parry")]


    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Parry")]

    [Serializable]
    public class InstructionABCActivateWeaponParry : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();



        public override string Title => string.Format(
            "Activate {0} ABC Weapon Parry",
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

                entity.ActivateWeaponParry();


            return DefaultResult;
        }


    

    }
}
#endif