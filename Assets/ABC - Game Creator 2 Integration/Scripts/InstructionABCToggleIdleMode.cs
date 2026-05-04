
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

    [Title("Toggle ABC Idle Mode")]
    [Category("ABC/Toggle Idle Mode")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Enables/Disables ABC Idle Mode")]

    [Parameter("Target", "The targeted game object to switch Idle Mode")]
    [Parameter("Toggle State", "The state to set the idle mode to: enable idle mode or disable idle mode (enter combat mode)")]

    [Keywords("ABC", "Ability", "Integration", "Combat", "Magic", "Attack", "Idle", "Enable", "Disable")]


    [Serializable]
    public class InstructionABCToggleIdleMode : Instruction {


        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();



        [SerializeField] private ABCToggleState m_ToggleState = ABCToggleState.Enable;



        public override string Title => string.Format(
            "{1} ABC Idle Mode on {0}",
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

                    ABC_Utilities.mbSurrogate.StartCoroutine((entity.ToggleIdleMode(true)));

                    break;
                case ABCToggleState.Disable:

                    ABC_Utilities.mbSurrogate.StartCoroutine((entity.ToggleIdleMode(false)));

                    break;
                case ABCToggleState.Toggle:

                    if (entity.inIdleMode == true)
                        ABC_Utilities.mbSurrogate.StartCoroutine((entity.ToggleIdleMode(false)));
                    else
                        ABC_Utilities.mbSurrogate.StartCoroutine((entity.ToggleIdleMode(true)));


                    break;
            }

            return DefaultResult;
        }


    

    }
}
#endif