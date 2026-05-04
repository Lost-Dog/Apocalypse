using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [CreateAssetMenu(
        fileName = "Motion Link", 
        menuName = "Game Creator/Traversal/Motion Link",
        order    = 50
    )]
    
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoMotionLinkClip.png")]
    
    [Serializable]
    public class MotionLink : MotionBase, IStageGizmos
    {
        public enum Mode
        {
            AnimationClip,
            AnimationState
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Mode m_Mode = Mode.AnimationClip;
        
        [SerializeField] private AnimationClip m_AnimationClip;
        [SerializeField] private AvatarMask m_Mask;
        
        [SerializeField] private State m_AnimationState;
        [SerializeField] private PropertyGetInteger m_Layer = new PropertyGetInteger(1);
        [SerializeField] private float m_TransitionTime = 0.25f;
        [SerializeField] private Easing.Type m_TransitionEase = Easing.Type.QuadInOut;
        [SerializeField] private PropertyGetDecimal m_MovementSpeed = GetDecimalDecimal.Create(3f);
        [SerializeField] private Easing.Type m_MovementEase = Easing.Type.QuadInOut;
        [SerializeField] private float m_Lift;
        [SerializeField] private Easing.Type m_LiftEase = Easing.Type.Linear;
        
        [SerializeField] private RunTraverseSequence m_AnimationSequence = new RunTraverseSequence();
        [SerializeField] private PropertyGetDecimal m_AnimationSpeed = GetDecimalConstantOne.Create;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Mode AnimationMode => this.m_Mode;

        [field: SerializeField] public string EditorModelPath { get; set; }

        public Anchor Anchor => this.m_Anchor;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public async Task<bool> Run(Character character, Args args, ICancellable cancel)
        {
            if (this.m_Mode == Mode.AnimationClip && this.m_AnimationClip == null) return false;
            if (this.m_Mode == Mode.AnimationClip && this.m_AnimationClip == null) return false;

            if (character == null) return false;
            if (!this.CanUse(args)) return false;
            
            _ = this.m_OnStart.Run(args);

            MotionFollowData followData = character.Motion.GetFollowingTarget();
            
            character.Motion.MoveToDirection(Vector3.zero, Space.World, 0);
            character.Motion.MoveToDirection(Vector3.zero, Space.World, MOVE_DIRECTION_KEY);
            
            character.Driver.SetGravityInfluence(GRAVITY_KEY, this.Gravity);
            character.Driver.ResetVerticalVelocity();
            
            float speed = Mathf.Max(0.01f, (float) this.m_AnimationSpeed.Get(args));
            int stateLayer = 0;

            switch (this.m_Mode)
            {
                case Mode.AnimationClip:
                    float animationDuration = Mathf.Max(0f, this.m_AnimationClip.length / speed - this.TransitionOut);
                    ConfigGesture gestureConfig = new ConfigGesture(
                        0f, this.m_AnimationClip.length, speed, true,
                        this.TransitionIn, this.TransitionOut
                    );
                    _ = character.Gestures.CrossFade(
                        this.m_AnimationClip, this.m_Mask, BlendMode.Blend, 
                        gestureConfig, true
                    );
                    await this.m_AnimationSequence.Run(
                        TextUtils.Humanize(this.name),
                        character.Time,
                        speed,
                        animationDuration,
                        this.m_AnimationClip,
                        cancel,
                        args
                    );
                    break;
                
                case Mode.AnimationState:
                    stateLayer = (int) this.m_Layer.Get(args);
                    ConfigState stateConfig = new ConfigState(
                        0f, speed, 1f,
                        this.TransitionIn, this.TransitionOut
                    );
                    _ = character.States.SetState(
                        this.m_AnimationState,
                        stateLayer, BlendMode.Blend, stateConfig
                    );
                    await this.StateSequence(character, cancel, args);
                    break;
                
                default: throw new ArgumentOutOfRangeException();
            }

            if (character != null)
            {
                switch (this.m_Mode)
                {
                    case Mode.AnimationClip: character.Gestures.Stop(this.m_AnimationClip, 0f, this.TransitionOut); break;
                    case Mode.AnimationState: character.States.Stop(stateLayer, 0f, this.TransitionOut); break;
                    default: throw new ArgumentOutOfRangeException();
                }
                
                character.Motion.StopToDirection(MOVE_DIRECTION_KEY);
                character.Driver.RemoveGravityInfluence(GRAVITY_KEY);
                
                if (followData.Transform != null)
                {
                    character.Motion.StartFollowingTarget(
                        followData.Transform,
                        followData.MinRadius,
                        followData.MaxRadius
                    );
                }
                
                this.ApplyMomentum(character);
            }
            
            _ = this.m_OnFinish.Run(args);
            return !cancel.IsCancelled;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async Task StateSequence(Character character, ICancellable cancel, Args args)
        {
            TraverseLink traverseLink = args.Self.Get<TraverseLink>();
            if (traverseLink == null) return;
            
            Vector3 characterStartPosition = this.CharacterPosition(character);
            Quaternion characterStartRotation = character.transform.rotation;
            
            TraverseLinkData traverseData = traverseLink.Type.ToTraverseLinkData(character, traverseLink);
            
            Vector3 positionA = traverseLink.Transform.TransformPoint(traverseData.positionA);
            Vector3 positionB = traverseLink.Transform.TransformPoint(traverseData.positionB);
            Quaternion rotationA = traverseLink.Transform.rotation * traverseData.rotationA;
            Quaternion rotationB = traverseLink.Transform.rotation * traverseData.rotationB;
            
            float moveSpeed = Mathf.Max(0.1f, (float) this.m_MovementSpeed.Get(args));
            float durationTransition = this.m_TransitionTime;
            float durationMovement = Vector3.Distance(positionA, positionB) / moveSpeed;

            float startTime = character.Time.Time;
            while (startTime + durationTransition > character.Time.Time)
            {
                if (ApplicationManager.IsExiting || cancel.IsCancelled) break;
                
                float t = (character.Time.Time - startTime) / durationTransition;
                float ratio = Easing.GetEase(this.m_TransitionEase, 0f, 1f, t);
                
                Vector3 nextPosition = Vector3.Lerp(
                    characterStartPosition,
                    positionA,
                    ratio
                );
                
                Quaternion nextRotation = Quaternion.Lerp(
                    characterStartRotation,
                    rotationA,
                    ratio
                );
                
                Vector3 currentCharacterPosition = this.CharacterPosition(character);
                Vector3 nextDirection = nextPosition - currentCharacterPosition;
                
                character.Motion.MoveToDirection(
                    nextDirection.normalized * moveSpeed,
                    Space.World,
                    MOVE_DIRECTION_KEY
                );
                
                character.Driver.AddPosition(nextDirection);
                character.Driver.SetRotation(nextRotation);
                
                await Task.Yield();
            }
            
            startTime = character.Time.Time;
            while (startTime + durationMovement > character.Time.Time)
            {
                if (ApplicationManager.IsExiting || cancel.IsCancelled) break;
                if (character == null) break;
                
                float t = (character.Time.Time - startTime) / durationMovement;
                float ratio = Easing.GetEase(this.m_MovementEase, 0f, 1f, t);
                
                float ratioLift = Easing.GetEase(this.m_LiftEase, 0f, 1f, t);
                Vector3 normalLift = new Vector3(0f, -4f * ratioLift * ratioLift + 4f * ratioLift, 0f);
                
                Vector3 nextPosition = Vector3.Lerp(
                    positionA,
                    positionB,
                    ratio
                ) + normalLift * this.m_Lift;
                
                Quaternion nextRotation = Quaternion.Lerp(
                    rotationA,
                    rotationB,
                    ratio
                );
                
                Vector3 currentCharacterPosition = this.CharacterPosition(character);
                Vector3 nextDirection = nextPosition - currentCharacterPosition;
                
                character.Motion.MoveToDirection(
                    nextDirection.normalized * moveSpeed,
                    Space.World,
                    MOVE_DIRECTION_KEY
                );
                
                character.Driver.AddPosition(nextDirection);
                character.Driver.SetRotation(nextRotation);
                
                await Task.Yield();
            }
        }
        
        public override AnimationClip GetExitAnimation(Vector3 direction, Quaternion rotation)
        {
            return null;
        }

        // STAGE GIZMOS: --------------------------------------------------------------------------
        
        public void StageGizmos(StagingGizmos stagingGizmos)
        { }
    }
}