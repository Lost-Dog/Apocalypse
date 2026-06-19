using GameCreator.Runtime.Behavior;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GameCreator.Editor.Behavior
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : GraphEditor
    {
        protected override string TypeName => "State Machine";
        
        // OVERRIDE METHODS: ----------------------------------------------------------------------
        
        protected override void OpenGraph()
        {
            StateMachine instance = this.target as StateMachine;
            OpenAsset(instance);
        }
        
        // OPEN ASSET: ----------------------------------------------------------------------------
        
        [OnOpenAsset]
        public static bool OnOpenAsset(EntityId instanceID, int line)
        {
            StateMachine instance = EditorUtility.EntityIdToObject(instanceID) as StateMachine;
            if (instance == null) return false;
            
            OpenAsset(instance);
            return true;
        }

        private static void OpenAsset(StateMachine stateMachine)
        {
            WindowStateMachine.Open(stateMachine);
        }
    }
}