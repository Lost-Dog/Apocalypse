using System;
using GameCreator.Editor.Characters;
using GameCreator.Editor.Core;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Traversal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameCreator.Editor.Traversal
{
    public class TraverseConfigurationStage : TPreviewSceneStage<TraverseConfigurationStage>
    {
        private const string HEADER_TITLE = "Animation Configuration";
        private const string HEADER_ICON = RuntimePaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoTraverse.png"; // TODO: Add icon to project files

        public static GameObject CharacterReference { get; private set; }
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private GameObject m_Character;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Title => HEADER_TITLE;
        protected override string Icon => HEADER_ICON;

        public MotionLink MotionLink => this.Asset as MotionLink;
        
        public Animator Animator => this.m_Character != null
            ? this.m_Character.GetComponent<Animator>()
            : null;

        protected override GameObject FocusOn => this.m_Character;

        // EVENTS: --------------------------------------------------------------------------------

        public static Action EventOpenStage;
        public static Action EventCloseStage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void ChangeCharacter(GameObject reference)
        {
            if (!InStage) return;

            CharacterReference = reference;
            Stage.MotionLink.EditorModelPath = AssetDatabase.GetAssetPath(reference);
            GameObject character = GetTarget();

            if (Stage.m_Character != null) DestroyImmediate(Stage.m_Character);
            Stage.m_Character = character;
            
            StagingGizmos.Bind(Stage.m_Character, Stage.MotionLink);
            StageUtility.PlaceGameObjectInCurrentStage(Stage.m_Character);
        }

        // INITIALIZE METHODS: --------------------------------------------------------------------

        public override void AfterStageSetup()
        {
            base.AfterStageSetup();
            
            Stage.m_Character = GetTarget();
            if (Stage.m_Character == null) return;
            
            StagingGizmos.Bind(Stage.m_Character, Stage.MotionLink);
            StageUtility.PlaceGameObjectInCurrentStage(Stage.m_Character);
        }

        protected override bool OnOpenStage()
        {
            if (!base.OnOpenStage()) return false;
            
            EventOpenStage?.Invoke();
            return true;
        }

        protected override void OnCloseStage()
        {
            base.OnCloseStage();
            EventCloseStage?.Invoke();
        }

        // PRIVATE STATIC METHODS: ----------------------------------------------------------------

        private static GameObject GetTarget()
        {
            if (Stage == null || Stage.MotionLink == null) return null;
            
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(Stage.MotionLink.EditorModelPath);
            if (source == null)
            {
                source = CharacterReference == null
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(CharacterEditor.MODEL_PATH)
                    : CharacterReference;
            }

            if (source == null) return null;
            GameObject target = Instantiate(source);

            if (target == null) return null;
            if (target.TryGetComponent(out Character character))
            {
                if (character.Animim.Animator != null)
                {
                    GameObject child = Instantiate(character.Animim.Animator.gameObject);
                    
                    DestroyImmediate(target);
                    target = child;
                }
            }

            if (target == null) return null;
            target.name = source.name;

            return target;
        }
    }
}