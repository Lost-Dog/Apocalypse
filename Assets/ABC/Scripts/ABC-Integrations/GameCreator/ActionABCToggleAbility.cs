
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
    public class ActionABCToggleAbility : IAction
    {




        public enum AbilityReferenceType
        {
            AbilityName = 0,
            AbilityID = 1
        }

        public enum ToggleState {
            EnableAbility = 0,
            DisableAbility = 1,
            ToggleAbility = 2
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public AbilityReferenceType abilityRefType = AbilityReferenceType.AbilityName;
        public ToggleState abilityToggleState = ToggleState.EnableAbility;

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

            switch (abilityToggleState) {
                case ToggleState.EnableAbility:

                    if (this.abilityRefType == AbilityReferenceType.AbilityName)
                        entity.EnableAbility(this.abilityName);
                    else
                        entity.EnableAbility(this.abilityID);

                    break;
                case ToggleState.DisableAbility:

                    if (this.abilityRefType == AbilityReferenceType.AbilityName)
                        entity.DisableAbility(this.abilityName);
                    else
                        entity.DisableAbility(this.abilityID);

                    break;
                case ToggleState.ToggleAbility:

                    if (this.abilityRefType == AbilityReferenceType.AbilityName)
                        entity.ToggleAbilityEnableState(this.abilityName);
                    else
                        entity.ToggleAbilityEnableState(this.abilityID);

                    break;
            }

          


            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Toggle Ability";
        private const string NODE_TITLE_NAME = "ABC Toggle {0} Ability";
        private const string NODE_TITLE_ID = "ABC Toggle Ability (ID: {0})";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spAbilityID;
        private SerializedProperty spAbilityName;
        private SerializedProperty spAbilityRefType;
        private SerializedProperty spAbilityToggleState;
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
            this.spAbilityToggleState = this.serializedObject.FindProperty("abilityToggleState");            
            this.spAbilityRefType = this.serializedObject.FindProperty("abilityRefType");


        }

 

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spAbilityRefType);
            EditorGUILayout.PropertyField(this.spAbilityToggleState);

            if (this.abilityRefType == AbilityReferenceType.AbilityName) {
                EditorGUILayout.PropertyField(this.spAbilityName);
            } else {
                EditorGUILayout.PropertyField(this.spAbilityID);
            }


            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif