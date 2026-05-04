using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Version(0, 1, 1)]

    [Title("Enter Traverse Interactive")]
    [Description("Makes a Character start a Traverse Interactive element")]

    [Category("Traversal/Enter Traverse Interactive")]

    [Keywords("Vault", "Climb", "Pass", "Mantle", "Step", "Jump")]
    [Image(typeof(IconTraverseInteractive), ColorTheme.Type.Green, typeof(OverlayTick))]
    
    [Serializable]
    public class InstructionTraversalEnterTraverseInteractive : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_TraverseInteractive = new PropertyGetGameObject();

        [SerializeField] private bool m_WaitUntilFinish = true;
        
        public override string Title
        {
            get
            {
                string wait = this.m_WaitUntilFinish ? " and wait" : string.Empty;
                return $"Enter {this.m_TraverseInteractive} with {this.m_Character}{wait}";
            }
        }

        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;
            
            GameObject root = this.m_TraverseInteractive.Get(args);
            if (root == null) return;
            
            TraverseInteractive[] traverseList = root.GetComponentsInChildren<TraverseInteractive>();
            Args argsInteractive = new Args(character.gameObject);
            
            foreach (TraverseInteractive interactive in traverseList)
            {
                if (interactive == null) continue;
                if (!interactive.isActiveAndEnabled) continue;

                argsInteractive.ChangeSelf(interactive.gameObject);
                if (!interactive.Motion.CanUse(argsInteractive)) continue;
                
                Task task = interactive.Enter(character, InteractiveTransitionData.None);
                if (this.m_WaitUntilFinish) await task;
                
                return;
            }
        }
    }
}
