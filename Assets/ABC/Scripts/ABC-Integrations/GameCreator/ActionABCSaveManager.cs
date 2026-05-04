
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
    public class ActionABCSaveManager : IAction
    {




        public enum ManagerAction
        {
            Save = 0,
            Load = 1
        }

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);
        public ManagerAction managerActionType = ManagerAction.Save;

        public string saveName = "Save1";


        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {


            if (this.managerActionType == ManagerAction.Save)
                ABC_SaveManager.Manager.NewSaveGameLocally(this.saveName);
            else
                ABC_SaveManager.Manager.NewLoadGameLocally(this.saveName);

            

            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "ABC Integration/Save Manager";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spSaveName;
        private SerializedProperty spManagerActionType;
        private SerializedProperty spTarget;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return "ABC Save Manager";
        }

        protected override void OnEnableEditorChild()
        {
            this.spSaveName = this.serializedObject.FindProperty("saveName");
            this.spManagerActionType = this.serializedObject.FindProperty("managerActionType");
            this.spTarget = this.serializedObject.FindProperty("target");


        }


        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();


            EditorGUILayout.PropertyField(this.spManagerActionType);

  
            EditorGUILayout.PropertyField(this.spSaveName);
            


            EditorGUILayout.Space();


            this.serializedObject.ApplyModifiedProperties();
        }

#endif

    }
}
#endif