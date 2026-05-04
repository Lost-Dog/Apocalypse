
#if ABC_GC_Integration
namespace GameCreator.Core {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using ABCToolkit;
    using GameCreator.Core;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ActionABCToggleIdleMode : IAction
    {



        public enum ToggleState {
            EnableIdleMode = 0,
            DisableIdleMode = 1,
            ToggleIdleMode = 2
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public ToggleState IdleToggleState = ToggleState.EnableIdleMode;


        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {


            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(this.target.GetGameObject(target));

          if (entity == null) {
                Debug.Log("Entity not found");
            }

            if (entity == null) return true;

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false ||  entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            switch (IdleToggleState) {
                case ToggleState.EnableIdleMode:

                  StartCoroutine(entity.ToggleIdleMode(true));
                  

                    break;
                case ToggleState.DisableIdleMode:

                   StartCoroutine(entity.ToggleIdleMode(false));

                    break;
                case ToggleState.ToggleIdleMode:

                    if (entity.inIdleMode == true)
                        StartCoroutine(entity.ToggleIdleMode(false));
                    else
                        StartCoroutine(entity.ToggleIdleMode(true));

                    break;
            }

          


            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Toggle Idle Mode";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spIdleModeToggleState;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(
                             "ABC Toggle Idle Mode: " + IdleToggleState.ToString()
                        );
        }

        protected override void OnEnableEditorChild()
        {
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spIdleModeToggleState = this.serializedObject.FindProperty("IdleToggleState");            


        }

 

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spIdleModeToggleState);
   

            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif