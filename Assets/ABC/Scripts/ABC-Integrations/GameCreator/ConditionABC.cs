
#if ABC_GC_Integration
namespace GameCreator.Core {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using ABCToolkit;

    using GameCreator.Core;


#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ConditionABC : ICondition {




        public enum ABCCondition {
            IsWeaponEquipped = 0,
            HasAbility = 1
        }

        public enum ABCReferenceType {
            ID, 
            Name
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public ABCCondition abcCondition = ABCCondition.IsWeaponEquipped;
        public ABCReferenceType abcRefType = ABCReferenceType.ID;

        public int ID;
        public string Name;


        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool Check(GameObject target) {
            GameObject targetGO = this.target.GetGameObject(target);
            if (!targetGO) {
                Debug.LogError("Condition Attribute: No target defined");
                return false;
            }

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(this.target.GetGameObject(target));

            if (entity == null) {
                Debug.Log("Entity not found");
                return false; 
            }

            //If StateManager or ABC controller object previously recorded is disabled then refresh to grab the latest references (new object may now have the components changed during play)
            if (entity != null && (entity.HasABCController() == false || entity.HasABCStateManager() == false))
                entity.ReSetupEntity();

            //Switch case the condition
            switch (abcCondition) {
                case ABCCondition.IsWeaponEquipped:

                    if (abcRefType == ABCReferenceType.ID) {

                        if (entity.currentEquippedWeapon != null && entity.currentEquippedWeapon.weaponID == this.ID)
                            return true;
                        else
                            return false; 

                    } else {

                        if (entity.currentEquippedWeapon != null && entity.currentEquippedWeapon.weaponName == this.Name)
                            return true;
                        else
                            return false;
                    }

                case ABCCondition.HasAbility:

                    if (abcRefType == ABCReferenceType.ID) {

                        if (entity.CurrentAbilities.Where(a => a.abilityID  == this.ID).Count() > 0)
                            return true;
                        else
                            return false;

                    } else {

                        if (entity.CurrentAbilities.Where(a => a.name == this.Name).Count() > 0)
                            return true;
                        else
                            return false;
                    }

            }



            return false;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/ABC Condition";
        private const string NODE_TITLE_NAME = "ABC Condition";
        private const string NODE_TITLE_ID = "ABC Condition";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spABCCondition;
        private SerializedProperty spABCRefType;
        private SerializedProperty spABCID;
        private SerializedProperty spABCName;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle() {
            return string.Format(
                             ("ABC Condition: " + this.abcCondition.ToString()),
                            (this.abcRefType == ABCReferenceType.Name ? this.Name : this.ID.ToString())
                        );
        }

        protected override void OnEnableEditorChild() {
            this.spABCCondition = this.serializedObject.FindProperty("abcCondition");
            this.spABCRefType = this.serializedObject.FindProperty("abcRefType");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spABCID = this.serializedObject.FindProperty("ID");
            this.spABCName = this.serializedObject.FindProperty("Name");

        }


        public override void OnInspectorGUI() {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spABCCondition, new GUIContent("Condition"));
            EditorGUILayout.PropertyField(this.spABCRefType, new GUIContent("Ref Type"));

            if (this.abcRefType == ABCReferenceType.ID) {
                EditorGUILayout.PropertyField(this.spABCID, new GUIContent("ID"));
            } else {
                EditorGUILayout.PropertyField(this.spABCName, new GUIContent("Name"));
            }


            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif