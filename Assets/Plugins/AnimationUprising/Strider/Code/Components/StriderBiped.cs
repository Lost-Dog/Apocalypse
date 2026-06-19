// ============================================================================================
// File: StriderBiped.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-12-06: Created this file.
// 
//     Contains the StriderBiped class for AnimationUprising StrideWarping in Unity
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Unity.Collections;
using UnityEngine.Playables;
using UnityEngine.Experimental.Animations;
using UnityEngine.Animations;
using Unity.Mathematics;
using Unity.Jobs;
using AnimationUprising.IK;

namespace AnimationUprising.Strider
{
    //============================================================================================
    /**
    *  @brief Strider component used for warping the stride of Bipeds to achieve varying speeds with
    *  the same animation.
    *         
    *********************************************************************************************/
    public class StriderBiped : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0f, 3f)]
        [SerializeField] protected float p_speedScale = 1f;                             //Total Speed scaling (playback + stride)
        [Range(0f, 1f)]
        [SerializeField] protected float p_weight = 1f;                                 //Weight of the stride warping (used to blend in and out)
        [SerializeField] protected float p_sizeCompensation = 1f;                       //Scale or size difference of the character
        [SerializeField] protected float p_smoothRate = 5f;                             //How fast speed changes smooth
        [SerializeField] protected float p_warpDirSmoothRate = 0.5f;                    //How fast warp direction smooth
        [SerializeField] protected ERootMotionMode p_rootMotion = ERootMotionMode.Off;   //Apply root motion or not?
        [SerializeField] protected EWarpDirectionMode p_warpDirectionMode = EWarpDirectionMode.RootVelocity; //Method for calculating the warp direction
        [SerializeField] protected EIKMethod p_IKMethod = EIKMethod.BuiltIn;              //Method for applying IK
        [SerializeField] protected bool p_manualRootMotionScaleFix;

        [Header("Stride Style")]                                                
        [SerializeField] protected float p_maxSpeed = 3f;                       //Maximum speed of the character
        [SerializeField] protected float p_baseOffset = 0f;                     //Distance in the warp direction to offset the stride scale point 
        [SerializeField] protected float p_dynamicOffset = 0f;                  //Dynamic distance to offset the stride scale point (gets multiplied by the curve)
        [SerializeField] protected AnimationCurve p_dynamicOffsetCurve = null;  //Dynamic offset curve. At runtime the total offset = base + dynamic * curve(speed)

        [Range(0f, 1f)]
        [SerializeField] protected float p_hipAdjustCutoff = 0.25f;             //Maximum allowable hip adjustment.
        [Range(0f, 1f)]
        [SerializeField] protected float p_hipDamping = 0.75f;                  //Hip damping multiplier 
        [SerializeField] protected AnimationCurve p_curHipDampingCurve = null;  //Hip damping curve. Total hip damping = p_hipDamping * p_curHipDampingCurve(speed)

        [Header("Animator Playback Speed")]
        [Range(0f, 3f)]
        [SerializeField] protected float p_independentPlaybackSpeed = 1f;
        [SerializeField] protected Vector2 p_minMaxPlaybackSpeed = new Vector2(0.9f, 1.1f);     //Minimum allowable animator playback speed
        [Range(1f, 3f)]
        [SerializeField] protected float p_maxPlaybackSpeed = 1.1f;     //Maximum allowable animator playback speed
        [Range(0f, 1f)]
        [SerializeField] protected float p_playbackSpeedWeight = 0.5f;  //Blend weight between playback and stride speed warping. Methods are blended up to the limits

        [Header("References")]
        [SerializeField] protected Animator p_animator = null;          //Reference to the animator component              
        [SerializeField] protected Transform p_hips = null;             //Reference to the hips transform (automatically set with humanoid)
        [SerializeField] protected Transform p_leftThigh = null;        //Reference to the left thigh transform (automatically set with humanoid)
        [SerializeField] protected Transform p_rightThigh = null;       //Reference to the right thigh transform (automatically set with humanoid)
        [SerializeField] protected Transform p_leftLowerLeg = null;     //Reference to the left lower leg transform (automatically set with humanoid)
        [SerializeField] protected Transform p_rightLowerLeg = null;    //Reference to the right lower leg transform (automatically set with humanoid)
        [SerializeField] protected Transform p_leftFoot = null;         //Reference to the left foot transform (automatically set with humanoid)
        [SerializeField] protected Transform p_rightFoot = null;        //Reference to the right foot transform (automatically set with humanoid)

        [Header("Unity Animation Rigging")]
        [SerializeField] protected Transform p_leftFootIK = null;
        [SerializeField] protected Transform p_rightFootIK = null;

        [Header("Events")]
        [SerializeField] public UnityEvent OnSetupComplete = null; //UnityEvent called once setup is complete.

        //Analysis
        protected float p_desiredSpeedScale;    //The desired speed scale. Actual speed scale is progressively move towards this value by the smooth rate
        protected float p_curHipDamping;        //The current value for hip damping
        protected float p_hipHeightAdjust;      //The cached amount of hip adjustment to be applied in Late Update
        protected float p_leftLegLength;        //The length of the left leg
        protected float p_rightLegLength;       //The length of the right leg
        protected Vector3 p_lastPosition;       //The last position of the character
        protected Vector3 p_lastWarpDir;      //The character position delta

        //Playables
        protected PlayableGraph p_playableGraph;                  //Playable graph for animation jobs
        protected AnimationPlayableOutput p_playableOutput;       //Playable output for stride warping animation jobs
        protected AnimationScriptPlayable p_animScriptPlayable;   //Animation script playable for animation jobs

        //Custom
        private ICustomStrideWarpApplicator m_customStrideWarp; //Interface to a custom stride warp applicator if desired

        //Jobs
        protected NativeArray<float3> p_resultsArray; //Native array for storing results of the stride warp jobs
        protected NativeArray<quaternion> p_ikResults; //Native array for storing results of the IK jobs

        //Coroutines
        private Coroutine m_smoothTransitionCoroutine = null;   //Coroutine handle for smoothly blending strider in and out

        //PROPERTIES
        public Vector3 ManualWarpDirection { get; set; } //Set this when using manual warp direction mode, to control warp direction
        public float MaxSpeed { get { return p_maxSpeed; } set { p_maxSpeed = value; } }
        public EWarpDirectionMode WarpDirectionMode { get { return p_warpDirectionMode; } set { p_warpDirectionMode = value; } }
        public ERootMotionMode RootMotion { get { return p_rootMotion; } set { p_rootMotion = value; } }
        public float BaseOffset { get { return p_baseOffset; } set { p_baseOffset = value; } }
        public float DynamicOffset { get { return p_dynamicOffset; } set { p_dynamicOffset = value; } }
        public float StrideScale { get; protected set; }
        public float SizeCompensation { get { return  p_sizeCompensation; } set { p_sizeCompensation = value; } }
        public float SpeedSmooth { get { return p_smoothRate; } set { p_smoothRate = value; } }
        public float StrideSmooth { get { return p_warpDirSmoothRate; } set { p_warpDirSmoothRate = value; } }
        public float IndependentPlaybackSpeed { get { return p_independentPlaybackSpeed; } set { p_independentPlaybackSpeed = value; } }

        //Use this property to set the speed scale. It will ensure smooth speed scale changes
        public float SpeedScale
        {
            get { return p_speedScale; }
            set { p_desiredSpeedScale = Mathf.Max(0f, value); }
        }

        public float MinPlaybackSpeed
        {
            get { return p_minMaxPlaybackSpeed.x; }
            set { p_minMaxPlaybackSpeed.y = Mathf.Clamp01(value); }
        }

        public float MaxPlaybackSpeed
        {
            get { return p_minMaxPlaybackSpeed.y; }
            set { p_minMaxPlaybackSpeed.y = Mathf.Max(1f, value); }
        }
        
        public float HipAdjustCutoff
        {
            get { return p_hipAdjustCutoff; }
            set { p_hipAdjustCutoff = Mathf.Max(0f, value); }
        }

        public float HipDamping
        {
            get { return p_hipDamping; }
            set { p_hipDamping = Mathf.Clamp01(value); }
        }

        public float PlaybackSpeedWeight
        {
            get { return p_playbackSpeedWeight; }
            set { p_playbackSpeedWeight = Mathf.Max(0f, value); }
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour awake function which sets up the system.
        *         
        *********************************************************************************************/
        protected virtual void Start()
        {
            Initialize();
        }

        //============================================================================================
        /**
        *  @brief Initializes all systems for Strider to work properly
        *         
        *********************************************************************************************/
        protected virtual void Initialize()
        {
            //Get animator if not slotted
            if(p_animator == null)
            {
                p_animator = GetComponentInChildren<Animator>();

                Assert.IsNotNull(p_animator, "Error: Trying to use StrideWarper script but it cannot find a valid animator to operator on. Component Disabled");

                if (p_animator == null)
                {
                    enabled = false;
                    return;
                }
            }

            //Default curves if not already setup
            if (p_curHipDampingCurve.keys.Length == 0)
            {
                p_curHipDampingCurve.AddKey(new Keyframe(0f, 1f));
                p_curHipDampingCurve.AddKey(new Keyframe(1f, 1f));
            }

            if (p_dynamicOffsetCurve.keys.Length == 0)
            {
                p_dynamicOffsetCurve.AddKey(new Keyframe(0f, 1f));
                p_dynamicOffsetCurve.AddKey(new Keyframe(1f, 1f));
            }

            //If the animator is in humanoid mode, set it up automatically
            if (p_animator.isHuman)
            {
                p_hips = p_animator.GetBoneTransform(HumanBodyBones.Hips);
                p_leftThigh = p_animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                p_rightThigh = p_animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                p_leftLowerLeg = p_animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                p_rightLowerLeg = p_animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                p_leftFoot = p_animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                p_rightFoot = p_animator.GetBoneTransform(HumanBodyBones.RightFoot); 
            }

            Assert.IsNotNull(p_hips, "Error: (StrideWarper) Cannot find humanoid character hips. Component Disabled");

            if (p_hips == null)
            {
                enabled = false;
                return;
            }

            if (p_leftFoot == null)
                Debug.LogWarning("Warning: (StrideWarper) No left foot transform found. Left leg stride will not be warped.");

            if (p_rightFoot == null)
                Debug.LogWarning("Warning: (StrideWarper) No right foot transform found. Right leg stride will not be warped.");

            if (p_leftThigh == null)
                Debug.LogWarning("Warning: (StrideWarper) No left thigh transform found. Left leg stride will not be warped.");

            if (p_rightThigh== null)
                Debug.LogWarning("Warning: (StrideWarper) No right thigh transform found. Right leg stride will not be warped.");

            p_leftLegLength = StrideWarp.CalculateLimbLength(p_leftFoot, p_leftThigh);
            p_rightLegLength = StrideWarp.CalculateLimbLength(p_rightFoot, p_rightThigh);

            if (p_leftLegLength < 0f)
                p_leftFoot = null;

            if (p_rightLegLength < 0f)
                p_rightFoot = null;

            p_desiredSpeedScale = p_speedScale;
            p_lastWarpDir = transform.forward;

            InitializePlayableGraph();
            SetupIKMethod(p_IKMethod);

            if(p_playableGraph.IsValid())
                p_playableGraph.Play();

            OnSetupComplete.Invoke();
        }

        //============================================================================================
        /**
        *  @brief Initializes the playable graph will all parts common to all IK methods
        *  
        *********************************************************************************************/
        protected virtual void InitializePlayableGraph()
        {
            p_playableGraph = p_animator.playableGraph;

            if (!p_playableGraph.IsValid())
                p_playableGraph = PlayableGraph.Create(gameObject.name + "StriderGraph_");

            p_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            p_playableOutput = AnimationPlayableOutput.Create(p_playableGraph, "StrideWarpOutput", p_animator);

            if (p_resultsArray.IsCreated)
                p_resultsArray.Dispose();

            if (p_ikResults.IsCreated)
                p_resultsArray.Dispose();
        }

        //============================================================================================
        /**
        *  @brief Sets up the playable graph for strider but only for modifying root motion. 
        *  
        *  This setup is used for all IKMethods which are not run within the animation update
        *  
        *********************************************************************************************/
        private void SetupPlayableGraph_RootMotionOnly()
        {
            p_resultsArray = new NativeArray<float3>(4, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            p_resultsArray[3] = new float3(0f);
            p_ikResults = new NativeArray<quaternion>(2, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            var playableBehaviour = ScriptPlayable<StrideWarpPlayable_RootMotion>.Create(p_playableGraph, 0);
            playableBehaviour.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playableBehaviour.GetBehaviour().StrideWarper = this;

            if (p_manualRootMotionScaleFix)
            {
                p_playableOutput.SetSourcePlayable(playableBehaviour);
            }
            else
            {
                var strideWarpRootMotionJob = new StrideWarpRootMotionJob() { SpeedWarp = 1f };
                p_animScriptPlayable = AnimationScriptPlayable.Create(p_playableGraph, strideWarpRootMotionJob, 1);

                p_animScriptPlayable.ConnectInput(0, playableBehaviour, 0);

                p_playableOutput.SetSourcePlayable(p_animScriptPlayable);
                p_animScriptPlayable.SetInputWeight(0, 1f);
            }

            p_playableOutput.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            p_playableOutput.SetWeight(1f);
        }

        //============================================================================================
        /**
        *  @brief Sets up the playable graph for strider but only when using BuiltIn_AnimJob IKMethod.
        *  
        *********************************************************************************************/
        private void SetupPlayableGraph_FullAnimJob()
        {
            p_resultsArray = new NativeArray<float3>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            p_resultsArray[0] = new float3(0f);

            var strideWarpIKAnimJob = new StrideWarpIKJob()
            {
                SpeedWarp = 1f,
                CharPositionY = transform.position.y,
                Offset = p_baseOffset,
                StrideSmoothing = p_warpDirSmoothRate,
                LastOffset = p_resultsArray,
                Hips = p_animator.BindStreamTransform(p_hips),
                LeftThighJoint = p_animator.BindStreamTransform(p_leftThigh),
                RightThighJoint = p_animator.BindStreamTransform(p_rightThigh),
                LeftLowerLegJoint = p_animator.BindStreamTransform(p_leftLowerLeg),
                RightLowerLegJoint = p_animator.BindStreamTransform(p_rightLowerLeg),
                LeftFoot = p_animator.BindStreamTransform(p_leftFoot),
                RightFoot = p_animator.BindStreamTransform(p_rightFoot),
            };

            p_animScriptPlayable = AnimationScriptPlayable.Create(p_playableGraph, strideWarpIKAnimJob, 1);

            var playableBehaviour = ScriptPlayable<StrideWarpPlayable_AnimJobs>.Create(p_playableGraph, 0);
            playableBehaviour.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playableBehaviour.GetBehaviour().StrideWarper = this;

            p_animScriptPlayable.ConnectInput(0, playableBehaviour, 0);

            p_playableOutput.SetSourcePlayable(p_animScriptPlayable);
            p_animScriptPlayable.SetInputWeight(0, 1f);
            p_playableOutput.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            p_playableOutput.SetWeight(1f);
        }

        //============================================================================================
        /**
        *  @brief Sets up the playable graph for strider but only when using BuiltIn_AnimJob IKMethod.
        *  
        *********************************************************************************************/
        private void SetupPlayableGraph_UnityRiggingAnimJob()
        {
            p_resultsArray = new NativeArray<float3>(4, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            p_resultsArray[0] = new float3(0f);

            var strideWarpAnimJob = new StrideWarpRiggingJob()
            {
                SpeedWarp = 1f,
                CharPositionY = transform.position.y,
                Offset = p_baseOffset,
                StrideSmoothing = p_warpDirSmoothRate,
                LastOffset = p_resultsArray,
                Hips = p_animator.BindStreamTransform(p_hips),
                LeftThighJoint = p_animator.BindStreamTransform(p_leftThigh),
                RightThighJoint = p_animator.BindStreamTransform(p_rightThigh),
                LeftFoot = p_animator.BindStreamTransform(p_leftFoot),
                RightFoot = p_animator.BindStreamTransform(p_rightFoot),
                LeftFootIK = p_animator.BindStreamTransform(p_leftFootIK),
                RightFootIK = p_animator.BindStreamTransform(p_rightFootIK)
            };

            p_animScriptPlayable = AnimationScriptPlayable.Create(p_playableGraph, strideWarpAnimJob, 1);
            p_animScriptPlayable.SetProcessInputs(true);

            var playableBehaviour = ScriptPlayable<StrideWarpPlayable_UnityRigAnimJobs>.Create(p_playableGraph, 0);
            playableBehaviour.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playableBehaviour.GetBehaviour().StrideWarper = this;

            p_animScriptPlayable.ConnectInput(0, playableBehaviour, 0);

            p_playableOutput.SetSourcePlayable(p_animScriptPlayable);
            p_animScriptPlayable.SetInputWeight(0, 1f);
            p_playableOutput.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            p_playableOutput.SetWeight(1f);
        }

        //============================================================================================
        /**
        *  @brief Sets up the currently chosen IK method in the playable graph. If the IK method is
        *  already setup, then this function will also destroy the playables from the previous setup
        *  
        *  @param [EIKMethod] a_ikMethod - the IKMethod to setup.
        *  
        *********************************************************************************************/
        public void SetupIKMethod(EIKMethod a_ikMethod)
        {
            if (p_animScriptPlayable.IsValid())
            {
                var customPlayable = p_animScriptPlayable.GetInput(0);
                if (customPlayable.IsValid())
                    customPlayable.Destroy();

                p_animScriptPlayable.Destroy();
            }

            switch (a_ikMethod)
            {
                case EIKMethod.UnityIK:
                    {
                        SetupPlayableGraph_RootMotionOnly();
                    }
                    break;
                case EIKMethod.BuiltIn:
                    {
                        SetupPlayableGraph_RootMotionOnly();
                    }
                    break;
                case EIKMethod.BuiltIn_AnimJobs:
                    {
                        SetupPlayableGraph_FullAnimJob();
                    }
                    break;
                case EIKMethod.AnimationRigging:
                    {
                        if (p_leftFootIK == null || p_rightFootIK == null)
                        {
                            Debug.LogError("Error: (StriderBiped) IKMethod set to 'AnimationRigging' but it doesn't seem like" +
                                "a Unity Animation Rig has been set up. Check that the IK targets are set in the inspector.");
                        }

                        SetupPlayableGraph_UnityRiggingAnimJob();
                    }
                    break;
                case EIKMethod.Custom:
                    {
                        m_customStrideWarp = GetComponent<ICustomStrideWarpApplicator>();
                        Assert.IsNotNull(m_customStrideWarp, "Error: (StrideWarper) Trying to use custom IK " +
                            "for stride warping but no ICustomStrideWarpApplicator component attached");

                        if (m_customStrideWarp == null)
                        {
                            enabled = false;
                            Debug.LogError("Failed to setup custom stride warping IK solution. Could not find ICustomStrideWarpApplicator interfaced component.");
                            return;
                        }

                        SetupPlayableGraph_RootMotionOnly();

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnEnable function. Sets the playable weight to 1 when enable is triggered
        *  
        *********************************************************************************************/
        private void OnEnable()
        {
            if(p_playableOutput.IsOutputValid())
                p_playableOutput.SetWeight(1f);
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnDisable function. Sets the playable weight to 0 when disabled
        *  
        *********************************************************************************************/
        private void OnDisable()
        {
            if (p_playableOutput.IsOutputValid())
                p_playableOutput.SetWeight(0f);
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnDestroy function. Disposes native data to avoid memory leaks
        *  
        *********************************************************************************************/
        private void OnDestroy()
        {
            if(p_resultsArray.IsCreated)
                p_resultsArray.Dispose();

            if(p_ikResults.IsCreated)
                p_ikResults.Dispose();
        }

        //============================================================================================
        /**
        *  @brief Update function for the 'BuiltIn' IK method.
        *  
        *  This function is called in the LateUpdate phase if 'BuiltIn' IK is being used. It updates 
        *  initial parameters and then runs the stride warp job to calculate desired foot ik locations.
        *  
        *********************************************************************************************/
        public void UpdateRootMotionJob()
        {
            if (p_hips == null)
                return;

            if (p_IKMethod == EIKMethod.BuiltIn_AnimJobs)
                return;

            UpdateParameters1();

            if (!p_manualRootMotionScaleFix)
            {
                var jobData = p_animScriptPlayable.GetJobData<StrideWarpRootMotionJob>();
                jobData.SpeedWarp = StrideScale;

                p_animScriptPlayable.SetJobData(jobData);
            }
        }

        //============================================================================================
        /**
        *  @brief Update function for the 'BuiltIn' IK method.
        *  
        *  This function is called in the LateUpdate phase if 'BuiltIn' IK is being used. It updates 
        *  initial parameters and then runs the stride warp job to calculate desired foot ik locations.
        *  
        *********************************************************************************************/
        public void UpdateBuiltIn_Deferred()
        {
            if (p_hips == null)
                return;

            float offset;
            float3 warpDir;
            UpdateParameters2(out offset, out warpDir);

            var strideWarpJob = new StrideWarpJob()
            {
                SpeedWarp = StrideScale,
                CharPositionY = transform.position.y,
                Offset = offset * p_sizeCompensation,
                StrideSmoothing = p_warpDirSmoothRate * Time.deltaTime,
                HipPosition = p_hips.position,
                LeftThighPosition = p_leftThigh.position,
                RightThighPosition = p_rightThigh.position,
                LeftFootPosition = p_leftFoot.position,
                RightFootPosition = p_rightFoot.position,
                WarpDir = warpDir,
                Results = p_resultsArray,
            };

            strideWarpJob.Run();

            p_hipHeightAdjust = p_resultsArray[0].y;
        }

        //============================================================================================
        /**
        *  @brief Update function for the 'BuiltIn_AnimJobs' IK method.
        *  
        *  This function is called from within the animation update just before the Animation job runs.
        *  It updates stride warp parameters for this frame and sets that data in the animation job
        *  struct.
        *  
        *********************************************************************************************/
        public void UpdateBuiltIn_AnimJobs()
        {
            if (p_hips == null)
                return;

            float offset;
            float3 warpDir;
            UpdateParameters1();
            UpdateParameters2(out offset, out warpDir);

            var jobData = p_animScriptPlayable.GetJobData<StrideWarpIKJob>();
            jobData.SpeedWarp = StrideScale;
            jobData.CharPositionY = transform.position.y;
            jobData.Offset = offset * p_sizeCompensation;
            jobData.StrideSmoothing = p_warpDirSmoothRate * Time.deltaTime;
            jobData.WarpDir = warpDir;
            jobData.HipDamping = p_curHipDamping;
            jobData.HipAdjustCutoff = p_hipAdjustCutoff * p_sizeCompensation;

            p_animScriptPlayable.SetJobData(jobData);
        }

        //============================================================================================
        /**
        *  @brief 
        *  
        *********************************************************************************************/
        public void UpdateUnityRigging_AnimJobs()
        {
            if (p_hips == null)
                return;

            float offset;
            float3 warpDir;
            UpdateParameters1();
            UpdateParameters2(out offset, out warpDir);

            var jobData = p_animScriptPlayable.GetJobData<StrideWarpRiggingJob>();
            jobData.SpeedWarp = StrideScale;
            jobData.CharPositionY = transform.position.y;
            jobData.Offset = offset * p_sizeCompensation;
            jobData.StrideSmoothing = p_warpDirSmoothRate * Time.deltaTime;
            jobData.WarpDir = warpDir;
            jobData.HipDamping = p_curHipDamping;
            jobData.HipAdjustCutoff = p_hipAdjustCutoff * p_sizeCompensation;

            p_animScriptPlayable.SetJobData(jobData);
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnAnimatorIK callback function used for mecanim. 
        *  
        *  This function is used by strider to run the early update for 'UnityIK' IK method. It updates
        *  runtime parameters and runs the stride warper job. The calculated ik target points are then
        *  applied to Unity's internal IK system. UnityIK method only works with mecanim.
        *  
        *  @param [int] layerIndex - the layer this IK is running on (Stride Warping only occurs on the 
        *  lowest layer.
        *         
        *********************************************************************************************/
        private void OnAnimatorIK(int layerIndex)
        {
            if (p_IKMethod != EIKMethod.UnityIK)
                return;

            if (layerIndex != 0)
                return;

            if (p_hips == null)
                return;

            float offset;
            float3 warpDir;
            UpdateParameters2(out offset, out warpDir);

            var strideWarpJob = new StrideWarpJob()
            {
                SpeedWarp = StrideScale,
                CharPositionY = transform.position.y,
                Offset = offset * p_sizeCompensation,

                HipPosition = p_hips.position,
                LeftThighPosition = p_leftThigh.position,
                RightThighPosition = p_rightThigh.position,
                LeftFootPosition = p_leftFoot.position,
                RightFootPosition = p_rightFoot.position,
                WarpDir = warpDir,
                Results = p_resultsArray,
            };

            strideWarpJob.Run();

            p_hipHeightAdjust = p_resultsArray[0].y;

            p_animator.SetIKPosition(AvatarIKGoal.LeftFoot, p_resultsArray[1]);
            p_animator.SetIKPosition(AvatarIKGoal.RightFoot, p_resultsArray[2]);

            p_animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            p_animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        }

        //============================================================================================
        /**
        *  @brief First pass update of parameters. Calculates the desired StrideScale and modifies
        *  the animator speed.
        *  
        *********************************************************************************************/
        protected virtual void UpdateParameters1()
        {
            UpdateSpeedScale(Time.deltaTime);
            float playbackSpeed = math.clamp((p_speedScale - 1f) * p_playbackSpeedWeight + 1f, p_minMaxPlaybackSpeed.x, p_minMaxPlaybackSpeed.y);
            float totalStrideScale = p_speedScale / (playbackSpeed * p_sizeCompensation);

            StrideScale = 1f + ((totalStrideScale - 1f) * p_weight);

            p_animator.speed = (1f + ((playbackSpeed - 1f) * p_weight)) * p_independentPlaybackSpeed;
        }

        //============================================================================================
        /**
        *  @brief Second pass of updates parameters at the beginning of an update.
        *  
        *  This involves updating the smooth speed changes, calculating the playback warp and stride warp
        *  split, calculating offsets, damping and warpDir.
        *  
        *  @param [out float] a_offset - the offset calculated is written out of this function
        *  @param [out float3] a_warpDir - the root delta calculated is written out of this function
        *  
        *********************************************************************************************/
        protected virtual void UpdateParameters2(out float a_offset, out float3 a_warpDir)
        {
            float speedRatio = p_animator.velocity.magnitude / p_maxSpeed;

            a_offset = p_baseOffset + p_dynamicOffset * Mathf.Clamp01(p_dynamicOffsetCurve.Evaluate(speedRatio));
            p_curHipDamping = Mathf.Clamp01(p_hipDamping * p_curHipDampingCurve.Evaluate(speedRatio));

            a_warpDir = GetWarpDirection();
            if (math.lengthsq(a_warpDir) < Mathf.Epsilon)
                a_warpDir = p_lastWarpDir;    
            
            p_lastWarpDir = a_warpDir;
        }

        //============================================================================================
        /**
        *  @brief Calculates the warp direction or 'root delta' based on the currently chosen method.
        *  
        *********************************************************************************************/
        protected Vector3 GetWarpDirection()
        {
            switch(p_warpDirectionMode)
            {
                case EWarpDirectionMode.RootVelocity: { return p_animator.deltaPosition; };
                case EWarpDirectionMode.ActualVelocity:
                    {
                        Vector3 position = transform.position;
                        Vector3 warpDir = position - p_lastPosition;

                        p_lastPosition = position;

                        return warpDir;
                    }
                case EWarpDirectionMode.CharacterFacing: { return transform.forward; }
                case EWarpDirectionMode.Manual: { return ManualWarpDirection; }
                default: { return transform.forward; }
            }
        }

        //============================================================================================
        /**
        *  @brief Updates the actual speed scale by smoothing it towards the desired speed scale by 
        *  the smoothing rate specified in the inspector
        *  
        *  @param [float] a_deltaTime - the time delta this frame
        *  
        *********************************************************************************************/
        protected void UpdateSpeedScale(float a_deltaTime)
        {
            p_speedScale = Mathf.MoveTowards(p_speedScale, p_desiredSpeedScale, p_smoothRate * a_deltaTime);
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour LateUpdate for running stride warper logic after the animation update. 
        *  
        *  The purpose of this function varies depending on the type of IK method used by strider. For 
        *  some methods the entire logic is run here while, for others, like UnityIK, only hips are 
        *  adjusted.
        *  
        *********************************************************************************************/
        public void LateUpdate()
        {
            switch (p_IKMethod)
            {
                case EIKMethod.UnityIK:
                    {
                        AdjustHips();
                    }
                    break;
                case EIKMethod.BuiltIn:
                    {
                        UpdateBuiltIn_Deferred();

                        float3 target = p_resultsArray[1];

                        if (math.distancesq(p_leftFoot.position, target) > 0.00001f)
                        {
                            var leftIKJob = new TwoBoneIKJob()
                            {
                                a = p_leftThigh.position,
                                b = p_leftLowerLeg.position,
                                c = p_leftFoot.position,
                                t = target,
                                a_gr = p_leftThigh.rotation,
                                b_gr = p_leftLowerLeg.rotation,
                                a_lr = p_leftThigh.localRotation,
                                b_lr = p_leftLowerLeg.localRotation,
                                eps = 0.01f,
                                ab_lr = p_ikResults
                            };
                            leftIKJob.Run();

                            
                            p_leftThigh.localRotation = p_ikResults[0];
                            p_leftLowerLeg.localRotation = p_ikResults[1];
                        }

                        target = p_resultsArray[2];

                        if (math.distancesq(p_leftFoot.position, target) > 0.00001f)
                        {
                            var rightIKJob = new TwoBoneIKJob()
                            {
                                a = p_rightThigh.position,
                                b = p_rightLowerLeg.position,
                                c = p_rightFoot.position,
                                t = target,
                                a_gr = p_rightThigh.rotation,
                                b_gr = p_rightLowerLeg.rotation,
                                a_lr = p_rightThigh.localRotation,
                                b_lr = p_rightLowerLeg.localRotation,
                                eps = 0.01f,
                                ab_lr = p_ikResults
                            };
                            rightIKJob.Run();

                            p_rightThigh.localRotation = p_ikResults[0];
                            p_rightLowerLeg.localRotation = p_ikResults[1];
                        }

                        AdjustHips();
                    }
                    break;
                case EIKMethod.Custom:
                    {
                        UpdateBuiltIn_Deferred();

                        if (p_hipHeightAdjust < 0f)
                            p_hipHeightAdjust = Mathf.Clamp(p_hipHeightAdjust * p_curHipDamping, -p_hipAdjustCutoff * p_sizeCompensation, 0f);

                        m_customStrideWarp.StrideWarp(p_hipHeightAdjust, p_resultsArray[1], p_resultsArray[2]);
                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief Adjusts the hips by the cached p_hipHeightAdjust
        *  
        *  This value has to be cached and applied at the end of stride warping logic to ensure that
        *  it does not adversely affect stride calculations and IK calculations. It is usually called
        *  from LateUpdate()
        *  
        *********************************************************************************************/
        private void AdjustHips()
        {
            if (p_hipHeightAdjust < 0f)
                p_hipHeightAdjust = Mathf.Clamp(p_hipHeightAdjust * p_curHipDamping, -p_hipAdjustCutoff * p_sizeCompensation, 0f);

            p_hips.Translate(new Vector3(0f, p_hipHeightAdjust, 0f), Space.World);
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnAnimatorMove function for handling root motion.
        *  
        *  If root motion is set to 'On' in the inspector, root motion will be applied to the transform
        *  and scaled according to the stride scale (root motion scale based on playback speed is already
        *  accounted for by Unity in the Animator Update)
        *  
        *********************************************************************************************/
        private void OnAnimatorMove()
        {
            if (p_rootMotion == ERootMotionMode.On)
            {
                if(p_manualRootMotionScaleFix)
                {
                    Vector3 rootPosition = transform.position + p_animator.deltaPosition * StrideScale;
                    transform.SetPositionAndRotation(rootPosition, p_animator.rootRotation);
                }
                else
                {
                    transform.SetPositionAndRotation(p_animator.rootPosition, p_animator.rootRotation);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief This function can be called to 'smoothly' disable the strider biped component.
        *  
        *  Instead of directly disabling this component, it is better to call DisableSmooth This will cause
        *  the stride warping to be smoothly blended out and the component will be disabled automatically.
        *  
        *  @param [float] - a_fadeRate - the rate at which to fade out the stride effect
        *  
        *********************************************************************************************/
        public void DisableSmooth(float a_fadeRate)
        {
            if (a_fadeRate <= Mathf.Epsilon)
                return;

            if (m_smoothTransitionCoroutine != null)
                StopCoroutine(m_smoothTransitionCoroutine);

            StartCoroutine(DisableSmoothCoroutine(a_fadeRate));
        }

        //============================================================================================
        /**
        *  @brief This function can be called to 'smoothly' enable the strider biped component.
        *  
        *  Instead of directly enabling this component, it is better to call EnableSmooth. This will cause
        *  the stride warping to be smoothly blended in and the component will be enabled automatically.
        *  
        *  @param [float] - a_fadeRate - the rate at which to fade in the stride effect
        *  
        *********************************************************************************************/
        public void EnableSmooth(float a_fadeRate)
        {
            if (a_fadeRate <= Mathf.Epsilon)
                return;

            if (m_smoothTransitionCoroutine != null)
                StopCoroutine(m_smoothTransitionCoroutine);

            StartCoroutine(EnableSmoothCoroutine(a_fadeRate));
        }

        //============================================================================================
        /**
        *  @brief Coroutine used for DisableSmooth
        *  
        *  @param [float] a_fadeRate - the smooth rate
        *  
        *********************************************************************************************/
        private IEnumerator DisableSmoothCoroutine(float a_fadeRate)
        {
            while(true)
            {
                p_weight -= (a_fadeRate * Time.deltaTime);

                if(p_weight <= 0f)
                {
                    p_weight = 0f;
                    break;
                }

                yield return null;
            }

            if(p_playableOutput.IsOutputValid())
                p_playableOutput.SetWeight(0f);

            enabled = false;
        }

        //============================================================================================
        /**
        *  @brief Coroutine used for EnableSmooth
        *  
        *  @param [float] a_fadeRate - the smooth rate
        *  
        *********************************************************************************************/
        private IEnumerator EnableSmoothCoroutine(float a_fadeRate)
        {
            enabled = true;
            p_weight = 0f;

            if (p_playableOutput.IsOutputValid())
                p_playableOutput.SetWeight(1f);

            while (true)
            {
                p_weight += (a_fadeRate * Time.deltaTime);

                if (p_weight >= 1f)
                {
                    p_weight = 1f;
                    break;
                }

                yield return null;
            }
        }

        //============================================================================================
        /**
        *  @brief Instantly sets the speed scale without any smoothing
        *  
        *  @param [float] a_speedScale - the speed scale to set
        *  
        *********************************************************************************************/
        public void SetSpeedScaleInstant(float a_speedScale)
        {
            p_speedScale = p_desiredSpeedScale = math.max(a_speedScale, 0f);
        }

    }//End of class: StrideWarper
}//End of namespace: AnimationUprising.Strider