using GameCreator.Runtime.Characters;
using MxM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Threepeat
{

    public class MMCGameCreator2 : MonoBehaviour
    {
        public bool InitializeManually = false;
        public bool StartInMxMMode = false;

        public bool MMLCCurrentlyEnabled { get; protected set; }

        public NGCharacter mmlcCharacter;
        public MxMAnimator mxmAnimator;

        public Character gcCharacter;

        public bool debugMode = false;

        public bool doFootElevateChange = false;
        public float footElevateGC = 0f;
        public float footElevateMMLC = 0f;

        public bool fireGameCreatorLandingEventOnLanding = true;
        public bool fireGameCreatorFootstepEventsOnFootsteps = true;

        private bool currentlySetToFireGameCreatorLandingEvent = false;
        private bool currentlySetToFireGameCreatorFootstepEvents = false;

        public UnityEvent OnMMLCEnable = new();
        public UnityEvent OnMMLCDisable = new();

        private AnimationMixerPlayable mixerUpstreamForMxM;

        private Coroutine blendCoroutine = null;

        protected PlayableGraph graph;
        protected Animator gcAnimator;
        protected int gcIKLayer;

        protected float previousGCIKWeight;
        protected bool previousGCHandIKEnabled;
        protected bool previousGCFootIKEnabled;



        [HideInInspector] public int PARKOUR_START = Animator.StringToHash("PARKOUR_START");
        [HideInInspector] public int PARKOUR_COMPLETE = Animator.StringToHash("PARKOUR_COMPLETE");

        // Start is called before the first frame update
        void Start()
        {
            if (!InitializeManually)
            {
                Initialize();
            }
        }

        private void OnMMLCLand(int landingType)
        {
            if (!mmlcCharacter.lastLandingWasFromJump)
            {
                gcCharacter.OnLand(mmlcCharacter.maxFallSpeed);
            }
        }

        private void OnMMLCAnyFootstep(NGFootstepEvent evt)
        {
            /*TODO: Trigger GC2 footstep system
            CharacterLocomotion.STEP step = CharacterLocomotion.STEP.Any;
            if (evt.IsLeft())
            {
                step = CharacterLocomotion.STEP.Left;
            }
            else if (evt.IsRight())
            {
                step = CharacterLocomotion.STEP.Right;
            }
            gcCharacter.onStep.Invoke(step);
            */
        }

        private void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.R))
            {
                float blendTime = 0.2f;
                if (MMLCCurrentlyEnabled)
                {
                    SetMxMAnimatorBlendWeight(0f, blendTime, true);
                }
                else
                {
                    SetMxMAnimatorBlendWeight(1f, blendTime, false);
                }
                
            }*/

            if (fireGameCreatorLandingEventOnLanding != currentlySetToFireGameCreatorLandingEvent)
            {
                currentlySetToFireGameCreatorLandingEvent = fireGameCreatorLandingEventOnLanding;
                if (fireGameCreatorLandingEventOnLanding)
                {
                    //Debug.LogFormat("Add: mmlcChar( {0} )", mmlcCharacter);
                    mmlcCharacter.movement.onLand.AddListener(this.OnMMLCLand);
                }
                else
                {
                    //Debug.LogFormat("Remove: mmlcChar( {0} )", mmlcCharacter);
                    mmlcCharacter.movement.onLand.RemoveListener(this.OnMMLCLand);
                }
            }

            if (fireGameCreatorFootstepEventsOnFootsteps != currentlySetToFireGameCreatorFootstepEvents)
            {
                currentlySetToFireGameCreatorFootstepEvents = fireGameCreatorFootstepEventsOnFootsteps;
                NGDispatchAnimationEvents ngdae = mmlcCharacter.GetComponentInChildren<NGDispatchAnimationEvents>();
                if (fireGameCreatorFootstepEventsOnFootsteps)
                {
                    // add Any step listener
                    if (ngdae != null)
                    {
                        ngdae.onAnyFootstep.AddListener(OnMMLCAnyFootstep);
                    }
                }
                else
                {
                    if (ngdae != null)
                    {
                        ngdae.onAnyFootstep.RemoveListener(OnMMLCAnyFootstep);
                    }

                }
            }
        }

        public void SetupIKForMMLC()
        {
            // This is not needed for GC2!  
            //Debug.LogError("TODO: SetupIKForMMLC");
            /*
            gcCharAnim = gcCharacter.GetCharacterAnimator();
            gcAnimator = gcCharAnim.animator;
            AnimatorController actrl = (AnimatorController)gcAnimator.runtimeAnimatorController;
            gcIKLayer = -1;
            for (int ii = 0; ii < actrl.layers.Length; ii++)
            {
                AnimatorControllerLayer layer = actrl.layers[ii];
                if (layer.iKPass)
                {
                    gcIKLayer = ii;
                    break;
                }
            }*/
        }

        /*
        protected void OnParkourStart()
        {
            previousGCFootIKEnabled = gcCharAnim.useFootIK;
            previousGCHandIKEnabled = gcCharAnim.useHandIK;
            if (gcIKLayer >= 0)
            {
                previousGCIKWeight = gcAnimator.GetLayerWeight(gcIKLayer);
                gcAnimator.SetLayerWeight(gcIKLayer, 1f);

            }

            gcCharAnim.useFootIK = false;
            gcCharAnim.useHandIK = false;
        }

        protected void OnParkourComplete()
        {
            if (gcIKLayer >= 0)
            {
                gcAnimator.SetLayerWeight(gcIKLayer, previousGCIKWeight);
            }

            gcCharAnim.useFootIK = previousGCFootIKEnabled;
            gcCharAnim.useHandIK = previousGCHandIKEnabled;

        }*/

        public void Initialize()
        {
            //Debug.Log("Initialize");
            if (mmlcCharacter == null)
            {
                mmlcCharacter = GetComponent<NGCharacter>();
            }

            if (mxmAnimator == null)
            {
                mxmAnimator = this.GetComponentInChildren<MxMAnimator>();
            }

            if (gcCharacter == null)
            {
                gcCharacter = GetComponent<Character>();
            }
            //ConfigurePlayableGraph();
            //Animator animator = GetComponent<Animator>();
            graph = GetPlayableGraph();
            if (graph.IsValid())
            {
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.LogFormat("Start: Got graph: {0}", graph.GetPlayableCount());
                }
#endif
                //ConfigurePlayableGraph(false);
                SetupMxMCharacterForGameCreator();
            }
            else
            {
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.LogFormat("Start: Graph is not ready");
                }
#endif
            }
            if (MxMSearchManager.Instance != null)
            {
                RegisterMxMAnimator_if_MxM2_1_16_OR_BETTER();
            }
        }

        private void RegisterMxMAnimator_if_MxM2_1_16_OR_BETTER()
        {
            Type myClassType = typeof(MxMSearchManager);

            MethodInfo method= myClassType.GetMethod("RegisterMxMAnimator", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method != null) 
            {
                method.Invoke(MxMSearchManager.Instance, new object[] { mxmAnimator });
            }
        }

        private void SetupMxMCharacterForGameCreator()
        {
            ConfigurePlayableGraph();

            mmlcCharacter.movement.onJump.AddListener(OnMMLCJump);
            //Debug.LogError("TODO: OnParkourStart/Complete handling");
            //mmlcCharacter.RegisterNamedEventHandler(PARKOUR_START, OnParkourStart);
            //mmlcCharacter.RegisterNamedEventHandler(PARKOUR_COMPLETE, OnParkourComplete);
            SetupIKForMMLC();
        }

        protected void OnParkourStart()
        {

        }
        protected void OnParkourComplete()
        {

        }

        private void OnDestroy()
        {
            if (mmlcCharacter != null)
            {
                mmlcCharacter.UnregisterNamedEventHandler(PARKOUR_START, OnParkourStart);
                mmlcCharacter.UnregisterNamedEventHandler(PARKOUR_COMPLETE, OnParkourComplete);
            }
        }

        void OnMMLCJump(bool isBigJump)
        {
            //Debug.Log("OnMMLCJump");
            //TODO: make sure MMLC is in charge.
            float jumpHeight = isBigJump ? mmlcCharacter.config.configJump.jumpBigHeight : mmlcCharacter.config.configJump.jumpHeight;

            float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gcCharacter.Motion.GravityDownwards); // Physics.gravity.y);
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.LogFormat("Jumping with force: {0}", jumpForce);
            }
#endif
            //gcCharacter.characterLocomotion.Jump(jumpForce);
            //Debug.LogError("TODO: Jump.");
            //gcCharacter.Motion. //.verticalSpeed = jumpForce;
        }

        PlayableGraph GetPlayableGraph()
        {
            PlayableGraph graph = gcCharacter.AnimationGraph;
            //Debug.LogFormat("graphName: {0}", graph.GetEditorName());
            return graph;

        }

        public void SetMxMAnimatorBlendWeight(float weight, float duration = 0, bool enableGCLoco = false, bool isControllable = true)
        {
            weight = Mathf.Clamp01(weight);

            /*if (duration <= 0)
            {
                this.mixerUpstreamForMxM.SetInputWeight(0, 1.0f - weight);
                this.mixerUpstreamForMxM.SetInputWeight(1, weight);
            }
            else
            {*/
            BlendToMxMWeight(weight, duration, enableGCLoco);
            //}

        }

        private void BlendToMxMWeight(float weight, float duration, bool enableGCLoco, bool isControllable = true)
        {
            if (blendCoroutine != null)
            {
                StopCoroutine(blendCoroutine);
                blendCoroutine = null;
            }

            if (duration <= 0f)
            {
                DoPreWork(enableGCLoco, isControllable);
                DoPostWork(enableGCLoco, isControllable, weight);
            }
            else
            {
                blendCoroutine = StartCoroutine(BlendToWeightWorker(weight, duration, enableGCLoco, isControllable));
            }
        }

        // Unique hash key used to register MMLC's gravity suppression with GC2's Driver.
        private static readonly int MMLC_GRAVITY_KEY = "MMCGameCreator2_GravitySuppress".GetHashCode();

        protected void DoPreWork(bool enableGCLoco, bool isControllable)
        {
            if (enableGCLoco)
            {
                // Restore GC2 player control — MMLC yields locomotion back to GC2.
                gcCharacter.Player.IsControllable = isControllable;

                // Restore GC2 gravity — remove MMLC's suppression influence.
                gcCharacter.Driver.RemoveGravityInfluence(MMLC_GRAVITY_KEY);
            }
            else
            {
                // MMLC takes locomotion control — silence GC2 player input and gravity.
                // Never disable the Character component itself (it owns the CharacterController).
                gcCharacter.Player.IsControllable = false;

                // Set GC2 gravity influence to 0 so it no longer fights MMLC's physics.
                gcCharacter.Driver.SetGravityInfluence(MMLC_GRAVITY_KEY, 0f);

                mmlcCharacter.enabled = true;
                if (mmlcCharacter.controllerWrapper != null)
                {
                    mmlcCharacter.controllerWrapper.enabled = true;
                }
                mxmAnimator.UnPause();
                MMLCCurrentlyEnabled = true;
                OnMMLCEnable.Invoke();
                mxmAnimator.DesiredPlaybackSpeed = 1;
                mxmAnimator.UserPlaybackSpeedMultiplier = 1;
            }
        }

        protected void DoPostWork(bool enableGCLoco, bool isControllable, float weight)
        {
            if (!enableGCLoco)
            {
                // MMLC is now fully in charge — GC2 player input and gravity already silenced in DoPreWork.
                mxmAnimator.DesiredPlaybackSpeed = 1;
                mxmAnimator.UserPlaybackSpeedMultiplier = 1;
            }
            else
            {
                // GC2 resumes — disable MMLC components and pause motion matching.
                mmlcCharacter.enabled = false;
                mmlcCharacter.controllerWrapper.enabled = false;
                mxmAnimator.Pause();
                MMLCCurrentlyEnabled = false;
                OnMMLCDisable.Invoke();
            }

            this.mixerUpstreamForMxM.SetInputWeight(0, 1.0f - weight);
            this.mixerUpstreamForMxM.SetInputWeight(1, weight);
        }

        private IEnumerator BlendToWeightWorker(float weight, float duration, bool enableGCLoco, bool isControllable = true)
        {
            float startTime = Time.time;

            float endTime = startTime + duration;

            float interimGCWeight, interimMxMWeight, currTimeOffset;

            float startMxMWeight = this.mixerUpstreamForMxM.GetInputWeight(1);
            float startGCWeight = this.mixerUpstreamForMxM.GetInputWeight(0);

            //TODO-footelevate float startFootElevate = gcCharacter.GetCharacterAnimator().footElevate;

            DoPreWork(enableGCLoco, isControllable);

            while (Time.time < endTime)
            {
                currTimeOffset = (Time.time - startTime) / duration;
                interimGCWeight = Mathf.Lerp(startGCWeight, 1.0f - weight, currTimeOffset);
                interimMxMWeight = Mathf.Lerp(startMxMWeight, weight, currTimeOffset);
                //TODO-footelevate if (doFootElevateChange)
                /*{
                    gcCharacter.GetCharacterAnimator().footElevate = Mathf.Lerp(startFootElevate, enableGCLoco ? footElevateGC : footElevateMMLC, currTimeOffset);
                }*/

                this.mixerUpstreamForMxM.SetInputWeight(0, interimGCWeight);
                this.mixerUpstreamForMxM.SetInputWeight(1, interimMxMWeight);

                yield return null;
            }

            DoPostWork(enableGCLoco, isControllable, weight);

            yield return 0;
        }

        public void SetMxMAnimatorAndGCSameWeight(float weight)
        {
            weight = Mathf.Clamp01(weight);

            this.mixerUpstreamForMxM.SetInputWeight(0, weight);
            this.mixerUpstreamForMxM.SetInputWeight(1, weight);
        }



        protected void ConfigurePlayableGraph()
        {
            //Debug.LogError("TODO: actually configure playable graph");
            if (mmlcCharacter.IsInitialized)
            {
                //Debug.LogWarning("Character already initialized, bailing.");
                return;
            }

            //Debug.LogFormat("GRAPH: {0}, outputSourcePlayable: {1}", graph.GetEditorName(), graph.GetOutput(0).GetSourcePlayable().GetPlayableType());
            PlayableOutput head = graph.GetOutput(0);
            Playable headPlayable = head.GetSourcePlayable();
            Playable first = headPlayable.GetInput(0);

            //Debug.LogFormat("type: gestures: {0}, headPlayable: {1}, headInputCount: {2}", first.GetPlayableType(), headPlayable.GetPlayableType(), headPlayable.GetInputCount());
            Playable nextSource = first;
            Playable prevPlayable = first;
            int idx = 0;
            while (nextSource.GetInputCount() > 0)
            {
                if (nextSource.GetPlayableType() == typeof(AnimatorControllerPlayable))
                {
                    //Debug.LogFormat("     [{0}] Hit an animator controller", idx);
                    break;
                }
                else
                {
                    //Debug.LogFormat("     [{0}] mixer, type {1}, numInputs = {2}", idx, nextSource.GetPlayableType(), nextSource.GetInputCount());
                    prevPlayable = nextSource;
                    nextSource = nextSource.GetInput(0);
                }
                idx++;
            }

            prevPlayable.DisconnectInput(0);

            this.mixerUpstreamForMxM = AnimationMixerPlayable.Create(graph, 2);
            this.mixerUpstreamForMxM.ConnectInput(0, nextSource, 0, 1f);
            this.mixerUpstreamForMxM.SetInputWeight(0, 1f);
            this.mixerUpstreamForMxM.ConnectInput(1, mxmAnimator.CreateMotionMatchingPlayable(ref graph), 0, 1f);

            prevPlayable.ConnectInput(0, mixerUpstreamForMxM, 0);
            prevPlayable.SetInputWeight(0, 1f);

            //Debug.Log("Initialized MMLC Behaviors");
            mmlcCharacter.InitializeBehaviors();

            mmlcCharacter.IKLayer = 0;

            if (StartInMxMMode)
            {
                StartCoroutine(StartInMxMCoroutine());
            }
            else
            {
                SetMxMAnimatorBlendWeight(0f, 0, true);
            }

        }

        private IEnumerator StartInMxMCoroutine()
        {
            yield return null; // Wait until the end of the current frame
            SetMxMAnimatorBlendWeight(1.0f, 0.25f, false);
        }

    } // class MMCGameCreator2
} // namespace Threepeat