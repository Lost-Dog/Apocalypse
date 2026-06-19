using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Transitions 
{
    [Version(0, 1, 1)]

    [Title("Transition to Scene")]
    [Description("Loads a new Scene using the specified Transition")]

    [Category("Transitions/Transition to Scene")]

    [Parameter(
        "Transition",
        "The transition to use"
    )]
    
    [Parameter(
        "Scene",
        "The scene to be loaded"
    )]
    
    [Parameter(
        "Mode",
        "Single mode replaces all other scenes. Additive mode loads the scene on top of the others"
    )]
    
    [Parameter(
        "Scene Entries",
        "Define the starting location of the player and other characters after loading the scene"
    )]

    [Keywords("Scene", "Change", "Move", "Load")]
    [Image(typeof(IconGradient), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class InstructionTransitionsLoadScene : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Transition m_Transition;
        [SerializeField] private bool m_WaitActivation;
        
        [SerializeField] private PropertyGetScene m_Scene = new PropertyGetScene();
        [SerializeField] private UnityEngine.SceneManagement.LoadSceneMode m_Mode;
        
        [SerializeField] private SceneEntries m_SceneEntries = new SceneEntries();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "Transition {0} to {1}scene {2}",
            this.m_Transition != null ? this.m_Transition.name : "(none)",
            this.m_Mode == UnityEngine.SceneManagement.LoadSceneMode.Additive 
                ? " additive"
                : string.Empty,
            this.m_Scene
        );

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            if (this.m_Transition == null) return DefaultResult;
            
            int scene = this.m_Scene.Get(args);
            this.m_SceneEntries.Schedule(scene, args);

            _ = this.m_Transition.ToScene(scene, this.m_Mode, !this.m_WaitActivation);
            return DefaultResult;
        }
    }
}