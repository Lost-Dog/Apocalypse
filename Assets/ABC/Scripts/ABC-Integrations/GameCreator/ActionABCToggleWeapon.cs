
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
    public class ActionABCToggleWeapon : IAction
    {




        public enum WeaponReferenceType
        {
            WeaponName = 0,
            WeaponID = 1
        }

        public enum ToggleState {
            EnableWeapon = 0,
            DisableWeapon = 1,
            ToggleWeapon = 2, 
            ImportGlobalWeapon = 3, 
            EquipWeapon = 4
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public WeaponReferenceType weaponRefType = WeaponReferenceType.WeaponName;
        public ToggleState weaponToggleState = ToggleState.EnableWeapon;

        public int weaponID;
        public string weaponName;


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

            switch (weaponToggleState) {
                case ToggleState.EnableWeapon:

                    if (this.weaponRefType == WeaponReferenceType.WeaponName)
                        entity.EnableWeapon(this.weaponName);
                    else
                        entity.EnableWeapon(this.weaponID);

                    break;
                case ToggleState.DisableWeapon:

                    if (this.weaponRefType == WeaponReferenceType.WeaponName)
                        entity.DisableWeapon(this.weaponName);
                    else
                        entity.DisableWeapon(this.weaponID);

                    break;
                case ToggleState.ToggleWeapon:

                    if (this.weaponRefType == WeaponReferenceType.WeaponName)
                        entity.ToggleWeaponEnableState(this.weaponName);
                    else
                        entity.ToggleWeaponEnableState(this.weaponID);

                    break;
                case ToggleState.EquipWeapon:

                    if (this.weaponRefType == WeaponReferenceType.WeaponName)
                        entity.EquipWeapon(this.weaponName);
                    else
                        entity.EquipWeapon(this.weaponID);

                    break;
                case ToggleState.ImportGlobalWeapon:

                    if (this.weaponRefType == WeaponReferenceType.WeaponName)
                        ABC_Utilities.mbSurrogate.StartCoroutine(entity.AddGlobalElementAtRunTime(this.weaponName));
                    else
                        Debug.LogWarning("ABC & GC - Integration: Unable to add global weapons using weapon ID, please select weapon name in dropdown");

                    break; 
            }



            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Toggle Weapon";
        private const string NODE_TITLE_NAME = "ABC Toggle {0} Weapon";
        private const string NODE_TITLE_ID = "ABC Toggle Weapon (ID: {0})";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spWeaponID;
        private SerializedProperty spWeaponName;
        private SerializedProperty spWeaponRefType;
        private SerializedProperty spWeaponToggleState;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(
                             (this.weaponRefType == WeaponReferenceType.WeaponName ? NODE_TITLE_NAME : NODE_TITLE_ID),
                            (this.weaponRefType == WeaponReferenceType.WeaponName ? this.weaponName : this.weaponID.ToString())
                        );
        }

        protected override void OnEnableEditorChild()
        {
            this.spWeaponID = this.serializedObject.FindProperty("weaponID");
            this.spWeaponName = this.serializedObject.FindProperty("weaponName");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spWeaponToggleState = this.serializedObject.FindProperty("weaponToggleState");
            this.spWeaponRefType = this.serializedObject.FindProperty("weaponRefType");


        }


        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spWeaponRefType);
            EditorGUILayout.PropertyField(this.spWeaponToggleState);

            if (this.weaponRefType == WeaponReferenceType.WeaponName)
            {
                EditorGUILayout.PropertyField(this.spWeaponName);
            }
            else
            {
                EditorGUILayout.PropertyField(this.spWeaponID);
            }


            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif