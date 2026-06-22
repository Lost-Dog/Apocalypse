
#if ABC_GC_2_Integration
namespace GameCreator.Core {
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using ABCToolkit;

    using GameCreator.Runtime.Common;
    using GameCreator.Runtime.VisualScripting;
    using GameCreator.Runtime.Characters;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    [Version(0, 1, 1)]

    [Title("Activate ABC Ability")]
    [Category("ABC/Activate Ability")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Activates an ABC Ability")]

    [Parameter("Target", "The targeted game object to activate the ABC Ability")]
    [Parameter("Ability Reference", "The name or ID of the Ability to activate")]

    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack")]

    [Serializable]
    public class InstructionABCActivateAbility : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private string m_AbilityReference;

        public override string Title => string.Format(
            "Activate {1} ABC Ability - {0}",
            this.m_AbilityReference != null
                ? this.m_AbilityReference.ToString()
                : "(none)",
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

            if (int.TryParse(this.m_AbilityReference, out int n))
                entity.TriggerAbility(int.Parse(this.m_AbilityReference));
            else
                entity.TriggerAbility(this.m_AbilityReference);


            return DefaultResult;
        }


    

    }
}
#endif