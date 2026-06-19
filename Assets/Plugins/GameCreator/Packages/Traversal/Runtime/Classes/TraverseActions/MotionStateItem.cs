using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class MotionStateItem : TPolymorphicItem<MotionStateItem>
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private IdString m_Id = new IdString("my-state-id");
        
        [SerializeField] private bool m_AllowMovement = true;
        
        [SerializeField] private RunInstructionsList m_InstructionsOnEnter = new RunInstructionsList();
        [SerializeField] private RunInstructionsList m_InstructionsOnExit = new RunInstructionsList();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public IdString Id => this.m_Id;
        
        public override string Title => $"{this.m_Id.String}: {this.m_InstructionsOnEnter}";

        public void OnEnter(Traverse traverse, Character character)
        {
            if (traverse == null) return;
            if (character == null) return;

            character.Combat.RequestStance<TraversalStance>().AllowMovement = this.m_AllowMovement;
            
            Args args = new Args(traverse, character);
            _ = this.m_InstructionsOnEnter.Run(args);
        }
        
        public void OnExit(Traverse traverse, Character character)
        {
            if (traverse == null) return;
            if (character == null) return;
            
            character.Combat.RequestStance<TraversalStance>().AllowMovement = true;
            
            Args args = new Args(traverse, character);
            _ = this.m_InstructionsOnExit.Run(args);
        }
    }
}