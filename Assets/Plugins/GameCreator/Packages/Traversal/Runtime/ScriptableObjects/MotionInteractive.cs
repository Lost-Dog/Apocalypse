using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [CreateAssetMenu(
        fileName = "Motion Interactive", 
        menuName = "Game Creator/Traversal/Motion Interactive",
        order    = 50
    )]
    
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoMotionAreaClip.png")]
    
    [Serializable]
    public class MotionInteractive : MotionBase
    {
        private enum InputMap
        {
            None,
            X,
            Y,
            Z,
            NegativeX,
            NegativeY,
            NegativeZ,
            AlwaysForward,
            AlwaysBackward,
        }
        
        private const float MIN_INPUT = 0.25f;
        
        private const float EXIT_TRANSITION = 0.1f;
        private const float LINK_TRANSITION = 0.1f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private State m_AnimationState;
        [SerializeField] private PropertyGetInteger m_Layer = new PropertyGetInteger(1);
        
        [SerializeField] private PropertyGetDecimal m_AnimationSpeed = GetDecimalConstantOne.Create;

        [SerializeField] private PropertyGetDirection m_InputDirection = GetDirectionCharactersLocalInput.Create;
        
        [SerializeField] private InputMap m_InputX = InputMap.X;
        [SerializeField] private InputMap m_InputY = InputMap.None;
        [SerializeField] private InputMap m_InputZ = InputMap.Z;

        [SerializeField] private Easing.Type m_TransitionEase = Easing.Type.QuadIn;
        
        [SerializeField] public TransitionAnimationsEnter m_EnterAnimations = new TransitionAnimationsEnter();
        [SerializeField] public TransitionAnimationsExit m_ExitAnimations = new TransitionAnimationsExit();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [field: SerializeField] public string EditorModelPath { get; set; }

        public Anchor Anchor => this.m_Anchor;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public async Task Enter(
            Traverse traverse,
            Character character,
            Args args,
            InteractiveTransitionData transition,
            ICancellable cancel)
        {
            if (character == null) return;
            if (!this.CanUse(args)) return;
            
            _ = this.m_OnStart.Run(args);
            
            character.Motion.MoveToDirection(Vector3.zero, Space.World, 0);
            character.Motion.MoveToDirection(Vector3.zero, Space.World, MOVE_DIRECTION_KEY);
            
            character.Driver.SetGravityInfluence(GRAVITY_KEY, this.Gravity);
            character.Driver.ResetVerticalVelocity();
            character.Driver.UpdateKinematics = false;
            
            float speed = Mathf.Max(0.01f, (float) this.m_AnimationSpeed.Get(args));
            int stateLayer = (int) this.m_Layer.Get(args);
            
            if (this.m_AnimationState != null)
            {
                ConfigState stateConfig = new ConfigState(
                    0f, speed, 1f,
                    this.TransitionIn, this.TransitionOut
                );
                
                _ = character.States.SetState(
                    this.m_AnimationState,
                    stateLayer, BlendMode.Blend, stateConfig
                );
            }
            
            Traverse nextTraverse = await this.OnUpdate(character, transition, cancel, args);

            if (character != null)
            {
                character.States.Stop(stateLayer, 0f, this.TransitionOut);
                character.Motion.StopToDirection(MOVE_DIRECTION_KEY);
                character.Driver.RemoveGravityInfluence(GRAVITY_KEY);
                character.Driver.UpdateKinematics = true;
            }
            
            _ = this.m_OnFinish.Run(args);
            
            if (nextTraverse != null && !cancel.IsCancelled)
            {
                _ = Traverse.ChangeTo(traverse, nextTraverse, character, true);
            }
            else
            {
                this.ApplyMomentum(character);
            }
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private async Task<Traverse> OnUpdate(
            Character character,
            InteractiveTransitionData transition,
            ICancellable cancel,
            Args args)
        {
            TraverseInteractive traverseInteractive = args.Self.Get<TraverseInteractive>();
            if (traverseInteractive == null) return null;

            Vector3 characterCurrentPosition = this.CharacterPosition(character);
            Vector3 characterStartPosition = traverseInteractive.CalculateStartPosition(character);
            
            TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
            stance.RelativePosition = traverseInteractive.Transform.InverseTransformPoint(characterStartPosition);

            float distance = Vector3.Distance(characterCurrentPosition, characterStartPosition);
            
            if (distance > Traverse.MIN_DISTANCE_TRANSITION)
            {
                await this.OnTransition(
                    this.m_EnterAnimations.Get(
                        characterStartPosition - characterCurrentPosition,
                        this.GetRotation(traverseInteractive, character.transform.forward, 0f, out Quaternion characterStartRotation)
                            ? characterStartRotation
                            : character.transform.rotation
                    ),
                    character, traverseInteractive,
                    transition,
                    cancel
                );
            }
            
            stance.InInteractiveTransition = false;

            while (!ApplicationManager.IsExiting && character != null && !cancel.IsCancelled)
            {
                float deltaTime = character.Time.DeltaTime;
                Vector3 traverseLocalInput = this.m_InputDirection.Get(args);
                                
                Vector3 swizzleLocalInput = stance.AllowMovement
                    ? new Vector3(
                        GetInput(this.m_InputX, traverseLocalInput),
                        GetInput(this.m_InputY, traverseLocalInput),
                        GetInput(this.m_InputZ, traverseLocalInput)
                    ) : Vector3.zero;
                
                swizzleLocalInput.Normalize();
                
                Vector3 worldMovement = traverseInteractive.Transform.TransformDirection(
                    swizzleLocalInput
                ).normalized * character.Motion.LinearSpeed;
                
                if (traverseInteractive.Width <= float.Epsilon && Mathf.Abs(swizzleLocalInput.x) > 0.5f)
                {
                    swizzleLocalInput = Vector3.zero;
                }
                
                Vector3 currentLocalPosition = stance.RelativePosition;
                Vector3 nextLocalPosition = currentLocalPosition + swizzleLocalInput * (deltaTime * character.Motion.LinearSpeed);
                
                if (traverseInteractive.PushingOutA(currentLocalPosition, swizzleLocalInput))
                {
                    if (traverseInteractive.ExitOnEdgeA)
                    {
                        return null;
                    }
                    
                    if (traverseInteractive.ContinueA != null)
                    {
                        return traverseInteractive.ContinueA;
                    }
                }

                if (traverseInteractive.PushingOutB(currentLocalPosition, swizzleLocalInput))
                {
                    if (traverseInteractive.ExitOnEdgeB)
                    {
                        return null;
                    }
                    
                    if (traverseInteractive.ContinueB != null)
                    {
                        return traverseInteractive.ContinueB;
                    }
                }
                
                nextLocalPosition = traverseInteractive.ClampInBounds(nextLocalPosition);
                
                stance.RelativePosition = nextLocalPosition;
                character.Driver.SetPosition(
                    traverseInteractive.Transform.TransformPoint(nextLocalPosition) + this.m_Anchor switch
                    {
                        Anchor.Crown => Vector3.down * character.Motion.Height,
                        Anchor.Center => Vector3.down * (character.Motion.Height * 0.5f),
                        Anchor.Feet => Vector3.zero,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                );
                
                character.Motion.MoveToDirection(
                    worldMovement,
                    Space.World,
                    MOVE_DIRECTION_KEY
                );

                bool changeRotation = this.GetRotation(
                    traverseInteractive,
                    character.transform.forward,
                    swizzleLocalInput.z,
                    out Quaternion nextRotation
                );

                if (changeRotation)
                {
                    float rotationSpeed = Mathf.Max(0f, character.Motion.AngularSpeed);
                    nextRotation = Quaternion.RotateTowards(
                        character.transform.rotation,
                        nextRotation,
                        rotationSpeed * deltaTime
                    );
                    
                    character.Driver.SetRotation(nextRotation);
                }
                
                await Task.Yield();
            }

            return null;
        }
        
        private async Task OnTransition(
            AnimationClip enterAnimation,
            Character character,
            TraverseInteractive traverseInteractive,
            InteractiveTransitionData transition,
            ICancellable cancel)
        {
            float startTime = character.Time.Time;
            float totalDuration = Mathf.Max(
                transition.ExitAnimationLength +
                Mathf.Max(enterAnimation != null ? enterAnimation.length - this.TransitionOut : 0f, 0f),
                this.TransitionIn
            );
            
            float exitDuration = transition.ExitAnimation != null ? transition.ExitAnimationLength : this.TransitionIn;
            float exitAnimationStartTime = startTime + Mathf.Max(0f, transition.ExitAnimationLength - EXIT_TRANSITION);
            
            bool enterAnimationStarted = false;
            bool exitAnimationStarted = false;
            
            Vector3 characterStartPosition = this.CharacterPosition(character);
            Quaternion characterStartRotation = character.transform.rotation;
            TraversalStance stance = character.Combat.RequestStance<TraversalStance>();
            
            while (character != null && character.Time.Time < startTime + totalDuration && !cancel.IsCancelled)
            {
                float currentTime = character.Time.Time;
                
                if (startTime < currentTime && !enterAnimationStarted)
                {
                    if (transition.ExitAnimation != null)
                    {
                        ConfigGesture config = new ConfigGesture(
                            0f, transition.ExitAnimationLength, 1f, false,
                            EXIT_TRANSITION,
                            enterAnimation != null ? 0f : this.TransitionOut
                        );
                        
                        _ = character.Gestures.CrossFade(transition.ExitAnimation, null, BlendMode.Blend, config, true);
                    }
                    
                    enterAnimationStarted = true;
                }
                
                if (exitAnimationStartTime < currentTime && !exitAnimationStarted)
                {
                    if (enterAnimation != null)
                    {
                        ConfigGesture config = new ConfigGesture(0f, enterAnimation.length, 1f, false, LINK_TRANSITION, this.TransitionOut);
                        _ = character.Gestures.CrossFade(enterAnimation, null, BlendMode.Blend, config, true);
                    }
                    
                    exitAnimationStarted = true;
                }
                
                float t = (currentTime - startTime) / exitDuration;
                stance.InInteractiveTransition = t < 1f;
                
                Vector3 nextPosition = Vector3.Lerp(
                    characterStartPosition,
                    traverseInteractive.Transform.TransformPoint(stance.RelativePosition),
                    Easing.GetEase(this.m_TransitionEase, 0f, 1f, t)
                );
                
                character.Driver.SetPosition(
                    nextPosition + this.m_Anchor switch
                    {
                        Anchor.Crown => Vector3.down * character.Motion.Height,
                        Anchor.Center => Vector3.down * (character.Motion.Height * 0.5f),
                        Anchor.Feet => Vector3.zero,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                );

                bool changeRotation = this.GetRotation(
                    traverseInteractive,
                    character.transform.forward,
                    0f,
                    out Quaternion nextRotation
                );

                if (changeRotation)
                {
                    nextRotation = Quaternion.Lerp(
                        characterStartRotation,
                        nextRotation,
                        Easing.GetEase(this.m_TransitionEase, 0f, 1f, t)
                    );
                    
                    character.Driver.SetRotation(nextRotation);
                }
                
                await Task.Yield();
            }
        }

        private static float GetInput(InputMap input, Vector3 vector)
        {
            return input switch
            {
                InputMap.None => 0f,
                InputMap.X => vector.x,
                InputMap.Y => vector.y,
                InputMap.Z => vector.z,
                InputMap.NegativeX => -vector.x,
                InputMap.NegativeY => -vector.y,
                InputMap.NegativeZ => -vector.z,
                InputMap.AlwaysForward => 1f,
                InputMap.AlwaysBackward => -1f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool GetRotation(TraverseInteractive traverse, Vector3 characterForward, float inputZ, out Quaternion rotation)
        {
            bool changeRotation = false;
            rotation = Quaternion.identity;
            
            switch (traverse.RotationMode)
            {
                case TraverseInteractive.CharacterRotationMode.None:
                    break;
                
                case TraverseInteractive.CharacterRotationMode.Path:
                    if (traverse.Transform.forward != Vector3.up)
                    {
                        changeRotation = true;
                        Vector3 directionForward = Vector3.ProjectOnPlane(traverse.Transform.forward, Vector3.up).normalized;
                        
                        if (Math.Abs(inputZ) > MIN_INPUT)
                        {
                            rotation = inputZ > 0f 
                                ? Quaternion.LookRotation(directionForward)
                                : Quaternion.LookRotation(directionForward * -1f);
                        }
                        else
                        {
                            switch (traverse.RotationIdle)
                            {
                                case TraverseInteractive.CharacterRotationIdle.DoNotChange:
                                    changeRotation = false;
                                    break;
                            
                                case TraverseInteractive.CharacterRotationIdle.Right:
                                    Vector3 directionRight = Vector3.Cross(directionForward, Vector3.up);
                                    rotation = Quaternion.LookRotation(directionRight);
                                    break;
                            
                                case TraverseInteractive.CharacterRotationIdle.Left:
                                    Vector3 directionLeft = Vector3.Cross(directionForward * -1f, Vector3.up);
                                    rotation = Quaternion.LookRotation(directionLeft);
                                    break;
                            
                                case TraverseInteractive.CharacterRotationIdle.Along:
                                    rotation = traverse.transform.InverseTransformDirection(characterForward).z >= 0f
                                        ? Quaternion.LookRotation(directionForward)
                                        : Quaternion.LookRotation(directionForward * -1f);
                                    break;
                                
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }   
                    }
                    break;
            
                case TraverseInteractive.CharacterRotationMode.Fixed:
                    changeRotation = true;
                    rotation = traverse.Transform.rotation * Quaternion.Euler(traverse.RotationValue);
                    break;
            
                default: throw new ArgumentOutOfRangeException();
            }

            return changeRotation;
        }

        public override AnimationClip GetExitAnimation(Vector3 direction, Quaternion rotation)
        {
            return this.m_ExitAnimations.Get(direction, rotation);
        }
    }
}