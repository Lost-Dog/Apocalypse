using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [CreateAssetMenu(
        fileName = "Motion Actions", 
        menuName = "Game Creator/Traversal/Motion Actions",
        order    = 50
    )]
    
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoMotionActions.png")]
    
    [Serializable]
    public class MotionActions : ScriptableObject
    {
        private enum JumpType
        {
            Jump,
            Dash
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetBool m_CanCancel = GetBoolTrue.Create;
        [SerializeField] private PropertyGetBool m_CanJump = GetBoolTrue.Create;
        
        [SerializeField] private EnablerFloat m_CustomJumpForce = new EnablerFloat(5f);
        
        [SerializeField] private bool m_DashOnJump = true;
        [SerializeField] private Vector3 m_DashLocalDirection = Vector3.up;
        [SerializeField] private float m_DashSpeed = 10f;
        [SerializeField] private float m_DashDuration = 0.5f;
        
        [SerializeField] private MotionActionsList m_ActionsList = new MotionActionsList();
        [SerializeField] private MotionStateList m_StateList = new MotionStateList();
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool CanCancel(Args args)
        {
            return this.m_CanCancel.Get(args);
        }

        public bool CanJump(Args args)
        {
            return this.m_CanJump.Get(args);
        }

        public void AttemptAction(IdString actionId, Traverse traverse, Character character)
        {
            MotionActionsItem action = this.m_ActionsList.Get(actionId);
            action?.Run(traverse, character);
        }
        
        public void AttemptStateEnter(IdString stateId, Traverse traverse, Character character)
        {
            MotionStateItem state = this.m_StateList.Get(stateId);
            state?.OnEnter(traverse, character);
        }
        
        public void AttemptStateExit(IdString stateId, Traverse traverse, Character character)
        {
            MotionStateItem state = this.m_StateList.Get(stateId);
            state?.OnExit(traverse, character);
        }
        
        // INTERNAL METHODS: ----------------------------------------------------------------------
        
        internal async Task AttemptJump(Traverse traverse, Character character)
        {
            character.Combat.RequestStance<TraversalStance>().ForceCancel();
            
            await Task.Yield();
            if (ApplicationManager.IsExiting || character == null) return;
            
            if (this.m_DashOnJump)
            {
                Vector3 dashDirection = traverse.transform.TransformDirection(this.m_DashLocalDirection).normalized;
                _ = character.Dash.Execute(dashDirection, this.m_DashSpeed, 1f, this.m_DashDuration, 0.25f);
            }
            
            float jumpForce = this.m_CustomJumpForce.IsEnabled
                ? this.m_CustomJumpForce.Value
                : character.Motion.JumpForce;
            
            character.Motion.ForceJump(jumpForce);
        }
    }
}