
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

    public enum ABCToggleState {
        Enable,
        Disable,
        Toggle
    }

    [Version(0, 1, 1)]

    [Title("Toggle ABC Ability")]
    [Category("ABC/Toggle Ability")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Enables/Disables an ABC Ability")]

    [Parameter("Target", "The targeted game object to toggle the ABC Ability")]
    [Parameter("Toggle State", "The state to set the ability to: enable, disable or toggle from the current value to the other")]
    [Parameter("Ability Reference", "The name or ID of the Ability to activate")]

    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Enable", "Disable")]


    [Serializable]
    public class InstructionABCToggleAbility : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private ABCToggleState m_ToggleState = ABCToggleState.Enable;

        [SerializeField] private string m_AbilityReference;



        public override string Title => string.Format(
            "{2} ABC Ability  - {0} on {1}",
            this.m_AbilityReference != null
                ? this.m_AbilityReference.ToString()
                : "(none)",
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
                case ABCToggleState.Enable:


                    if (int.TryParse(this.m_AbilityReference, out int a))
                        entity.EnableAbility(int.Parse(this.m_AbilityReference));
                    else
                        entity.EnableAbility(this.m_AbilityReference);


                    break;
                case ABCToggleState.Disable:


                    if (int.TryParse(this.m_AbilityReference, out int b))
                        entity.DisableAbility(int.Parse(this.m_AbilityReference));
                    else
                        entity.DisableAbility(this.m_AbilityReference);

                    break;
                case ABCToggleState.Toggle:

                    if (int.TryParse(this.m_AbilityReference, out int c))
                        entity.ToggleAbilityEnableState(int.Parse(this.m_AbilityReference));
                    else
                        entity.ToggleAbilityEnableState(this.m_AbilityReference);


                    break;
            }

            return DefaultResult;
        }


    

    }
}
#endif