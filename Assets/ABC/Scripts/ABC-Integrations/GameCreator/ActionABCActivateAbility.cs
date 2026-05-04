
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
    public class ActionABCActivateAbility : IAction
    {




        public enum AbilityReferenceType
        {
            AbilityName = 0,
            AbilityID = 1
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public AbilityReferenceType abilityRefType = AbilityReferenceType.AbilityName;

        public int abilityID;
        public string abilityName;


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

            if (this.abilityRefType == AbilityReferenceType.AbilityName)
                entity.TriggerAbility(this.abilityName);
            else
                entity.TriggerAbility(this.abilityID);


            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Activate Ability";
        private const string NODE_TITLE_NAME = "ABC Activate {0} Ability";
        private const string NODE_TITLE_ID = "ABC Activate Ability (ID: {0})";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spAbilityID;
        private SerializedProperty spAbilityName;
        private SerializedProperty spAbilityRefType;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(
                             (this.abilityRefType == AbilityReferenceType.AbilityName ? NODE_TITLE_NAME : NODE_TITLE_ID),
                            (this.abilityRefType == AbilityReferenceType.AbilityName ? this.abilityName : this.abilityID.ToString())
                        );
        }

        protected override void OnEnableEditorChild()
        {
            this.spAbilityID = this.serializedObject.FindProperty("abilityID");
            this.spAbilityName = this.serializedObject.FindProperty("abilityName");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spAbilityRefType = this.serializedObject.FindProperty("abilityRefType");


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
            EditorGUILayout.PropertyField(this.spAbilityRefType);

            if (this.abilityRefType == AbilityReferenceType.AbilityName)
            {
                EditorGUILayout.PropertyField(this.spAbilityName);
            }
            else
            {
                EditorGUILayout.PropertyField(this.spAbilityID);
            }


            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif