
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

    public enum ABCEnableDisableState {
        Enable,
        Disable
    }

    [Version(0, 1, 1)]

    [Title("Toggle ABC Ability Activation")]
    [Category("ABC/Toggle Ability Activation")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Enables/Disables ABC Ability Activation")]

    [Parameter("Target", "The targeted game object to toggle the ABC Ability")]
    [Parameter("Toggle State", "The state to set the ability to: enable, disable or toggle from the current value to the other")]
    [Parameter("Ability Reference", "The name or ID of the Ability to activate")]

    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Enable", "Disable")]


    [Serializable]
    public class InstructionABCToggleAbilityActivation : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private ABCEnableDisableState m_ToggleState = ABCEnableDisableState.Enable;



        public override string Title => string.Format(
            "{1} ABC Ability Activation  - on {0}",
            this.m_Target, 
            m_ToggleState.ToString()
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

            switch (m_ToggleState) {
                case ABCEnableDisableState.Enable:

                    entity.ToggleAbilityActivation(true);


                    break;
                case ABCEnableDisableState.Disable:

                    entity.ToggleAbilityActivation(false);

                    break;
            }

            return DefaultResult;
        }


    

    }
}
#endif