using System;
using GameCreator.Editor.Characters;
using GameCreator.Editor.Core;
using GameCreator.Runtime.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KingEdward.SkillTree.Editor
{
    public class SkillPreviewStage : TPreviewSceneStage<SkillPreviewStage>
    {
        private const string HEADER_TITLE = "Skill Preview";
        private const string HEADER_ICON = "Assets/KingEdward/SkillTree/Editor/Icons/SkillIcon.png";

        public static GameObject CharacterReference { get; private set; }
        
        [NonSerialized] private GameObject m_Character;

        protected override string Title => HEADER_TITLE;
        protected override string Icon => HEADER_ICON;

        public Skill Skill => this.Asset as Skill;
        
        public Animator Animator => this.m_Character != null
            ? this.m_Character.GetComponent<Animator>()
            : null;

        protected override GameObject FocusOn => this.m_Character;

        public static Action EventOpenStage;
        public static Action EventCloseStage;

        public static void OpenStage(Skill skill)
        {
            if (skill == null) return;
            
            // If already in stage with same skill, just focus instead of reopening
            if (InStage && Stage != null && Stage.Skill == skill)
            {
                // Just focus the scene view without reopening
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Focus();
                }
                return;
            }
            
            string assetPath = AssetDatabase.GetAssetPath(skill);
            if (string.IsNullOrEmpty(assetPath)) return;
            
            // Close current stage if open (to avoid conflicts)
            if (InStage)
            {
                StageUtility.GoToMainStage();
            }
            
            EnterStage(assetPath);
        }

        public static void ChangeCharacter(GameObject reference)
        {
            if (!InStage) return;

            CharacterReference = reference;
            GameObject character = GetTarget();

            if (Stage.m_Character != null) DestroyImmediate(Stage.m_Character);
            Stage.m_Character = character;
            
            StageUtility.PlaceGameObjectInCurrentStage(Stage.m_Character);
        }

        public override void AfterStageSetup()
        {
            base.AfterStageSetup();
            
            // Clean up old character if exists
            if (Stage.m_Character != null)
            {
                DestroyImmediate(Stage.m_Character);
                Stage.m_Character = null;
            }
            
            Stage.m_Character = GetTarget();
            if (Stage.m_Character == null) return;
            
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
            if (m_Character != null)
            {
                DestroyImmediate(m_Character);
                m_Character = null;
            }
            
            base.OnCloseStage();
            EventCloseStage?.Invoke();
        }

        private static GameObject GetTarget()
        {
            if (Stage == null || Stage.Skill == null) return null;
            
            GameObject source = CharacterReference == null
                ? AssetDatabase.LoadAssetAtPath<GameObject>(CharacterEditor.MODEL_PATH)
                : CharacterReference;

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
