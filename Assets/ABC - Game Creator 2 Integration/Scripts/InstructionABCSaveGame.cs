
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

    [Title("ABC Save Game")]
    [Category("ABC/Save Game")]

    [Image(typeof(IconCircleSolid), ColorTheme.Type.Blue, typeof(IconDice))]
    [Description("Will Save ABC Game Data")]

    [Parameter("Save Name", "The name of the save file which will be created/updated")]


    [Keywords("ABC", "Ability", "Integration", "Save")]

    [Serializable]
    public class InstructionABCSaveGame: Instruction {


        [SerializeField] private string m_saveName = "Save1";



        public override string Title => string.Format(
            "Save ABC Game Data"
        );


        protected override Task Run(Args args) {

            ABC_SaveManager.Manager.NewSaveGameLocally(this.m_saveName);


            return DefaultResult;
        }


    

    }
}
#endif