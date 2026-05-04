
#if ABC_GC_Integration
namespace GameCreator.Core {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using ABCToolkit;

    using GameCreator.Core;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ActionABCActivateWeaponParry : IAction
    {




        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);



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

            entity.ActivateWeaponParry();


            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Activate Weapon Parry";
        private const string NODE_TITLE_NAME = "ABC Activate Weapon Parry";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return NODE_TITLE_NAME;
        }

        protected override void OnEnableEditorChild()
        {
         
            this.spTarget = this.serializedObject.FindProperty("target");


        }

        //protected override void OnDisableEditorChild() {
        //    this.abilityID = 0;
        //    this.abilityName = null;
        //    this.target = null;
        //    this.abilityRefType = AbilityReferenceType.AbilityName;
        //}

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spTarget);

            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif