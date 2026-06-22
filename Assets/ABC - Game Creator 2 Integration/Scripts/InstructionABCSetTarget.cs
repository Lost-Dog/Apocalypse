
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

    public enum ABCTargetSetType {
        SetTarget = 0,
        RemoveTarget = 1,        
    }


    [Version(0, 1, 1)]

    [Title("Set ABC Target")]
    [Category("ABC/Set Target")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Sets an ABC Target")]

    [Parameter("Target Set Type", "The type of ABC target event: Target or Soft Target")]

    [Keywords("Soft Target", "Target", "ABC", "Ability", "Integration")]

    [Serializable]
    public class InstructionABCSetTarget : Instruction {


        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private ABCTargetSetType m_TargetSetType = ABCTargetSetType.SetTarget;

        public override string Title => string.Format(
            "ABC - {0}",
            this.m_TargetSetType == ABCTargetSetType.SetTarget ? "Set Target" : "Remove Target"
        );


        protected override Task Run(Args args) {
            GameObject m_Character = this.m_Character.Get(args);
            GameObject m_Target = this.m_Target.Get(args);

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(m_Character);

            if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return DefaultResult;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            switch (m_TargetSetType) {
                case ABCTargetSetType.SetTarget:

                    entity.SetNewTarget(m_Target);

                    break;
                case ABCTargetSetType.RemoveTarget:

                    entity.RemoveTarget();

                    break;
            }


            return DefaultResult;
        }


    

    }
}
#endif