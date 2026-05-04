
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

    [Title("ABC Load Game")]
    [Category("ABC/Load Game")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Will Load ABC Game Data")]

    [Parameter("Save Name", "The name of the save file which will be loaded")]


    [Keywords("ABC", "Ability", "Integration", "Load")]

    [Serializable]
    public class InstructionABCLoadGame: Instruction {


        [SerializeField] private string m_loadName = "Save1";



        public override string Title => string.Format(
            "Load ABC Game Data"
        );


        protected override Task Run(Args args) {

            ABC_SaveManager.Manager.NewLoadGameLocally(this.m_loadName);


            return DefaultResult;
        }


    

    }
}
#endif