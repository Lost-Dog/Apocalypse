using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Serializable]
    public class MotionActionsItem : TPolymorphicItem<MotionActionsItem>
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private IdString m_Id = new IdString("my-action-id");

        [SerializeField] private RunInstructionsList m_Instructions = new RunInstructionsList();

        [SerializeField] private bool m_Exits = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public IdString Id => this.m_Id;
        
        public override string Title => $"{this.m_Id.String}: {this.m_Instructions}";

        public void Run(Traverse traverse, Character character)
        {
            if (traverse == null) return;
            if (character == null) return;

            if (this.m_Exits)
            {
                character.Combat.RequestStance<TraversalStance>().ForceCancel();
            }
            
            Args args = new Args(traverse, character);
            _ = this.m_Instructions.Run(args);
        }
    }
}