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
        [Tooltip("Log runtime GC2/MxM blend diagnostics at intervals to verify whether MxM is actively contributing.")]
        public bool showRuntimeBlendDiagnostics = false;
        [Min(0.1f)] public float runtimeBlendDiagnosticsInterval = 1.0f;
        [Range(0f, 1f)] public float mxmContributingWeightThreshold = 0.05f;

        public bool doFootElevateChange = false;
        public float footElevateGC = 0f;
        public float footElevateMMLC = 0f;

        public bool fireGameCreatorLandingEventOnLanding = true;
        public bool fireGameCreatorFootstepEventsOnFootsteps = true;
        [Tooltip("When enabled, GC2 Character stays enabled in MxM mode for the player so GC2 interaction systems can still run.")]
        public bool keepGCCharacterEnabledForPlayerInMxMMode = true;
        [Tooltip("When enabled alongside keepGCCharacterEnabledForPlayerInMxMMode, GC2 Player remains controllable in MxM mode. Disable to avoid locomotion conflicts.")]
        public bool keepGCPlayerControllableInMxMMode = false;
        [Tooltip("When enabled, GC2 Character stays enabled in MxM mode for non-controllable actors (NPCs) so AI/pathing movement keeps running.")]
        public bool keepGCCharacterEnabledForNPCInMxMMode = true;
        [Tooltip("When GC2 owns NPC movement in MxM mode, disable MMLC movement updates and use MxM as animation-only to avoid transform jitter.")]
        public bool mxmAnimationOnlyForNpcWhenGCDrivesMotion = true;

        private bool currentlySetToFireGameCreatorLandingEvent = false;
        private bool currentlySetToFireGameCreatorFootstepEvents = false;
        private float lastOwnershipCorrectionLogTime = -10f;

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
        private float nextInitializeRetryTime = 0f;
        private const float InitializeRetryInterval = 0.5f;
        private bool jumpListenerRegistered = false;
        private bool initialGCWasControllable = true;
        private float nextRuntimeDiagnosticsTime = 0f;
        private Coroutine deferredInitializeRoutine = null;
        private const float DeferredInitializeTimeoutSeconds = 10f;

        public bool IsGCCharacterEnabled => gcCharacter != null && gcCharacter.enabled;

        private bool IsLikelyPlayerActor()
        {
            if (gcCharacter != null && gcCharacter.gameObject != null && gcCharacter.gameObject.CompareTag("Player"))
            {
                return true;
            }

            return gameObject.CompareTag("Player");
        }

        private bool ShouldKeepGCEnabledInMxMMode()
        {
            if (gcCharacter == null) return false;

            if (IsLikelyPlayerActor())
            {
                return keepGCCharacterEnabledForPlayerInMxMMode;
            }

            if (!keepGCCharacterEnabledForNPCInMxMMode) return false;

            var playerData = gcCharacter.Player;
            if (playerData == null) return false;

            return !initialGCWasControllable || !playerData.IsControllable;
        }

        private bool ShouldUseNpcAnimationOnlyMxMMode()
        {
            if (IsLikelyPlayerActor()) return false;

            return ShouldKeepGCEnabledInMxMMode() && mxmAnimationOnlyForNpcWhenGCDrivesMotion;
        }

        public bool TryGetDebugMixerWeights(out float gcWeight, out float mxmWeight)
        {
            gcWeight = 0f;
            mxmWeight = 0f;

            if (!mixerUpstreamForMxM.IsValid())
            {
                return false;
            }

            gcWeight = mixerUpstreamForMxM.GetInputWeight(0);
            mxmWeight = mixerUpstreamForMxM.GetInputWeight(1);
            return true;
        }

        public bool IsMxMContributing()
        {
            if (!TryGetDebugMixerWeights(out _, out float mxmWeight))
            {
                return false;
            }

            return mxmWeight > mxmContributingWeightThreshold && mxmAnimator != null && !mxmAnimator.IsPaused;
        }



        [HideInInspector] public int PARKOUR_START = Animator.StringToHash("PARKOUR_START");
        [HideInInspector] public int PARKOUR_COMPLETE = Animator.StringToHash("PARKOUR_COMPLETE");

        // Start is called before the first frame update
        void Start()
        {
            if (!InitializeManually)
            {
                if (deferredInitializeRoutine == null)
                {
                    deferredInitializeRoutine = StartCoroutine(InitializeWhenPlayableGraphReady());
                }
            }
        }

        private IEnumerator InitializeWhenPlayableGraphReady()
        {
            float deadline = Time.realtimeSinceStartup + DeferredInitializeTimeoutSeconds;

            while (Time.realtimeSinceStartup < deadline)
            {
                EnsureCoreReferences();

                if (IsPlayableGraphReady(out _))
                {
                    Initialize();

                    if (mixerUpstreamForMxM.IsValid())
                    {
                        deferredInitializeRoutine = null;
                        yield break;
                    }
                }

                yield return null;
            }

            if (debugMode)
            {
                IsPlayableGraphReady(out string reason);
                Debug.LogWarning($"[{name}] Deferred initialization timed out: {reason}");
            }

            Initialize();
            deferredInitializeRoutine = null;
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
            EnsureMixerReady();

            if (mmlcCharacter == null || mmlcCharacter.movement == null)
            {
                return;
            }

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

            MaintainRuntimeOwnership();

            if (showRuntimeBlendDiagnostics)
            {
                EmitRuntimeBlendDiagnostics();
            }
        }

        private void EmitRuntimeBlendDiagnostics()
        {
            if (Time.time < nextRuntimeDiagnosticsTime)
            {
                return;
            }

            nextRuntimeDiagnosticsTime = Time.time + Mathf.Max(0.1f, runtimeBlendDiagnosticsInterval);

            if (!TryGetDebugMixerWeights(out float gcWeight, out float mxmWeight))
            {
                Debug.Log($"[{name}] GC2/MxM diagnostics: mixer not ready.");
                return;
            }

            bool mxmContributing = mxmWeight > mxmContributingWeightThreshold && mxmAnimator != null && !mxmAnimator.IsPaused;
            Debug.Log(
                $"[{name}] GC2/MxM diagnostics: gcWeight={gcWeight:0.000}, mxmWeight={mxmWeight:0.000}, " +
                $"mxmPaused={(mxmAnimator != null && mxmAnimator.IsPaused)}, mmlcEnabled={(mmlcCharacter != null && mmlcCharacter.enabled)}, " +
                $"gcEnabled={(gcCharacter != null && gcCharacter.enabled)}, mxmContributing={mxmContributing}");
        }

        private void EnsureMixerReady()
        {
            if (mixerUpstreamForMxM.IsValid())
            {
                return;
            }

            if (deferredInitializeRoutine != null)
            {
                return;
            }

            if (Time.time < nextInitializeRetryTime)
            {
                return;
            }

            nextInitializeRetryTime = Time.time + InitializeRetryInterval;

            EnsureCoreReferences();
            if (!IsPlayableGraphReady(out _))
            {
                return;
            }

            Initialize();
        }

        private void EnsureCoreReferences()
        {
            if (mmlcCharacter == null)
                mmlcCharacter = GetComponent<NGCharacter>();

            if (mxmAnimator == null)
                mxmAnimator = GetComponentInChildren<MxMAnimator>();

            if (gcCharacter == null)
                gcCharacter = GetComponent<Character>();
        }

        private bool IsPlayableGraphReady(out string reason)
        {
            reason = string.Empty;

            if (gcCharacter == null)
            {
                reason = "Character reference is missing";
                return false;
            }

            PlayableGraph candidateGraph = gcCharacter.AnimationGraph;
            if (!candidateGraph.IsValid())
            {
                reason = "AnimationGraph is invalid";
                return false;
            }

            if (candidateGraph.GetOutputCount() <= 0)
            {
                reason = "AnimationGraph has no outputs";
                return false;
            }

            PlayableOutput head = candidateGraph.GetOutput(0);
            Playable sourcePlayable = head.GetSourcePlayable();
            if (!sourcePlayable.IsValid())
            {
                reason = "Output source playable is invalid";
                return false;
            }

            if (sourcePlayable.GetInputCount() <= 0)
            {
                reason = "Output source playable has no inputs yet";
                return false;
            }

            Playable firstInput = sourcePlayable.GetInput(0);
            if (!firstInput.IsValid())
            {
                reason = "First playable input is invalid";
                return false;
            }

            return true;
        }

        private void MaintainRuntimeOwnership()
        {
            if (!MMLCCurrentlyEnabled)
            {
                return;
            }

            bool corrected = false;
            bool keepGCEnabled = ShouldKeepGCEnabledInMxMMode();
            bool npcAnimationOnlyMode = ShouldUseNpcAnimationOnlyMxMMode();

            if (!keepGCEnabled && gcCharacter != null && gcCharacter.enabled)
            {
                gcCharacter.enabled = false;
                corrected = true;
            }

            if (!keepGCEnabled && gcCharacter != null && gcCharacter.Player != null && gcCharacter.Player.IsControllable)
            {
                gcCharacter.Player.IsControllable = false;
                corrected = true;
            }

            if (mixerUpstreamForMxM.IsValid())
            {
                float gcWeight = mixerUpstreamForMxM.GetInputWeight(0);
                float mxmWeight = mixerUpstreamForMxM.GetInputWeight(1);

                if (Mathf.Abs(gcWeight) > 0.001f || Mathf.Abs(mxmWeight - 1f) > 0.001f)
                {
                    mixerUpstreamForMxM.SetInputWeight(0, 0f);
                    mixerUpstreamForMxM.SetInputWeight(1, 1f);
                    corrected = true;
                }
            }

            if (mxmAnimator != null && mxmAnimator.IsPaused)
            {
                mxmAnimator.UnPause();
                corrected = true;
            }

            if (npcAnimationOnlyMode && mmlcCharacter != null && mmlcCharacter.enabled)
            {
                mmlcCharacter.enabled = false;
                corrected = true;
            }

            if (npcAnimationOnlyMode && mmlcCharacter != null && mmlcCharacter.controllerWrapper != null && mmlcCharacter.controllerWrapper.enabled)
            {
                mmlcCharacter.controllerWrapper.enabled = false;
                corrected = true;
            }

            if (debugMode && corrected && (Time.time - lastOwnershipCorrectionLogTime > 0.5f))
            {
                lastOwnershipCorrectionLogTime = Time.time;
                Debug.Log("MMLC ownership watchdog corrected GC2/MxM runtime ownership drift.");
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

            if (gcCharacter == null)
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] MMCGameCreator2.Initialize: Character reference is missing.");
                return;
            }

            if (mxmAnimator == null)
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] MMCGameCreator2.Initialize: MxMAnimator reference is missing.");
                return;
            }

            if (mmlcCharacter == null)
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] MMCGameCreator2.Initialize: NGCharacter reference is missing.");
                return;
            }

            initialGCWasControllable = gcCharacter != null && gcCharacter.Player != null && gcCharacter.Player.IsControllable;
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

            if (mmlcCharacter != null && mmlcCharacter.movement != null && !jumpListenerRegistered)
            {
                mmlcCharacter.movement.onJump.AddListener(OnMMLCJump);
                jumpListenerRegistered = true;
            }
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
                if (jumpListenerRegistered && mmlcCharacter.movement != null)
                {
                    mmlcCharacter.movement.onJump.RemoveListener(OnMMLCJump);
                    jumpListenerRegistered = false;
                }

                mmlcCharacter.UnregisterNamedEventHandler(PARKOUR_START, OnParkourStart);
                mmlcCharacter.UnregisterNamedEventHandler(PARKOUR_COMPLETE, OnParkourComplete);
            }
        }

        void OnMMLCJump(bool isBigJump)
        {
            //Debug.Log("OnMMLCJump");
            //TODO: make sure MMLC is in charge.
            if (gcCharacter == null || gcCharacter.Motion == null)
            {
                return;
            }

            float jumpHeight = isBigJump ? mmlcCharacter.config.configJump.jumpBigHeight : mmlcCharacter.config.configJump.jumpHeight;

            float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gcCharacter.Motion.GravityDownwards); // Physics.gravity.y);
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.LogFormat("Jumping with force: {0}", jumpForce);
            }
#endif
            // Mirror MMLC jump into GC2 locomotion so animation and physical lift stay in sync.
            gcCharacter.Jump.Do(jumpForce);
        }

        PlayableGraph GetPlayableGraph()
        {
            if (gcCharacter == null)
            {
                return default;
            }

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

        protected void DoPreWork(bool enableGCLoco, bool isControllable)
        {
            if (enableGCLoco)
            {
                //gcCharacter.enabled = true;
                gcCharacter.Player.IsControllable = isControllable;
                gcCharacter.enabled = true;
            }
            else
            {
                bool npcAnimationOnlyMode = ShouldUseNpcAnimationOnlyMxMMode();
                mmlcCharacter.enabled = !npcAnimationOnlyMode;
                if (mmlcCharacter.controllerWrapper != null)
                {
                    mmlcCharacter.controllerWrapper.enabled = !npcAnimationOnlyMode;
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
                bool keepGCEnabled = ShouldKeepGCEnabledInMxMMode();
                if (gcCharacter != null && gcCharacter.Player != null)
                {
                    bool keepControllable = keepGCEnabled && IsLikelyPlayerActor() && keepGCPlayerControllableInMxMMode;
                    gcCharacter.Player.IsControllable = keepControllable;
                }

                mxmAnimator.DesiredPlaybackSpeed = 1;
                mxmAnimator.UserPlaybackSpeedMultiplier = 1;

                if (gcCharacter != null)
                    gcCharacter.enabled = keepGCEnabled ? true : false;
            }
            else
            {
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
            if (mixerUpstreamForMxM.IsValid())
            {
                return;
            }

            if (gcCharacter == null)
                gcCharacter = GetComponent<Character>();

            if (mmlcCharacter == null)
                mmlcCharacter = GetComponent<NGCharacter>();

            if (mxmAnimator == null)
                mxmAnimator = GetComponentInChildren<MxMAnimator>();

            if (gcCharacter == null || mmlcCharacter == null || mxmAnimator == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: missing refs (Character:{gcCharacter != null}, NGCharacter:{mmlcCharacter != null}, MxMAnimator:{mxmAnimator != null}).");
                }
                return;
            }

            if (!graph.IsValid())
            {
                graph = GetPlayableGraph();
            }

            if (!graph.IsValid() || graph.GetOutputCount() <= 0)
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: animation graph not ready.");
                return;
            }

            //Debug.LogFormat("GRAPH: {0}, outputSourcePlayable: {1}", graph.GetEditorName(), graph.GetOutput(0).GetSourcePlayable().GetPlayableType());
            PlayableOutput head = graph.GetOutput(0);
            Playable headPlayable = head.GetSourcePlayable();

            if (!headPlayable.IsValid() || headPlayable.GetInputCount() <= 0)
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: graph head has no valid source input.");
                return;
            }

            Playable first = headPlayable.GetInput(0);

            if (!first.IsValid())
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: first playable input is invalid.");
                return;
            }

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

            if (prevPlayable.IsValid() && prevPlayable.GetInputCount() > 0)
            {
                prevPlayable.DisconnectInput(0);
            }
            else
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: previous playable is invalid for reconnect.");
                return;
            }

            this.mixerUpstreamForMxM = AnimationMixerPlayable.Create(graph, 2);
            this.mixerUpstreamForMxM.ConnectInput(0, nextSource, 0, 1f);
            this.mixerUpstreamForMxM.SetInputWeight(0, 1f);
            Playable mxmPlayable = mxmAnimator.CreateMotionMatchingPlayable(ref graph);
            if (!mxmPlayable.IsValid())
            {
                if (debugMode)
                    Debug.LogWarning($"[{name}] ConfigurePlayableGraph aborted: MxM playable is invalid.");
                return;
            }

            this.mixerUpstreamForMxM.ConnectInput(1, mxmPlayable, 0, 1f);

            prevPlayable.ConnectInput(0, mixerUpstreamForMxM, 0);
            prevPlayable.SetInputWeight(0, 1f);

            //Debug.Log("Initialized MMLC Behaviors");
            if (!mmlcCharacter.IsInitialized)
            {
                mmlcCharacter.InitializeBehaviors();
            }

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