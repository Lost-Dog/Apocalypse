using System;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCreator.Runtime.Traversal
{
    [AddComponentMenu("Game Creator/Traversal/Utils/Rope")]
    [Icon(EditorPaths.PACKAGES + "Traversal/Editor/Gizmos/GizmoRope.png")]
    
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    [RequireComponent(typeof(LineRenderer))]
    
    [Serializable]
    public class Rope : MonoBehaviour
    {
        private enum Phase
        {
            None,
            Anticipating,
            Throwing,
            Tensioning,
            Reeling,
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private int m_Resolution = 32;
        [SerializeField] private int m_SolverIterations = 10;
        
        [SerializeField] private RopeConfig m_Config = RopeConfig.Default;

        [SerializeField] private PropertyGetInstantiate m_Hook = new PropertyGetInstantiate(); 
        [SerializeField] private Vector3 m_HookRotation;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private LineRenderer m_LineRenderer;
        [NonSerialized] private GameObject m_HookInstance;
        
        [NonSerialized] private float m_RandomCycleOffset;
        
        [NonSerialized] private Phase m_Phase = Phase.None;
        [NonSerialized] private float m_StartTime;
        
        [NonSerialized] private Character m_Character;
        [NonSerialized] private Transform m_Source;
        [NonSerialized] private Transform m_Target;
        
        [NonSerialized] private float m_AnticipationDuration;
        [NonSerialized] private float m_ThrowDuration;
        [NonSerialized] private float m_TensionDuration;
        [NonSerialized] private float m_ReelDuration;
        
        [NonSerialized] private List<RopeSegment> m_Segments = new List<RopeSegment>();
        [NonSerialized] private NativeArray<Vector3> m_DrawPoints;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [field: NonSerialized] public Vector3 CurrentPointA { get; private set; }
        [field: NonSerialized] public Vector3 CurrentPointB { get; private set; }
        [field: NonSerialized] public Vector3 CurrentGravity  { get; private set; }
        [field: NonSerialized] public float CurrentTensionFactor { get; private set; }
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_LineRenderer = this.GetComponent<LineRenderer>();
            this.m_RandomCycleOffset = Random.Range(0f, Mathf.PI);
            
            if (this.m_LineRenderer != null) this.m_LineRenderer.enabled = false;
        }

        private void OnEnable()
        {
            if (this.m_HookInstance != null) Destroy(this.m_HookInstance);
            
            this.m_HookInstance = this.m_Hook.Get(this.transform);
            this.m_HookInstance.SetActive(false);
        }

        private void OnDisable()
        {
            if (this.m_HookInstance != null)
            {
                Destroy(this.m_HookInstance);
            }
        }

        private void OnDestroy()
        {
            this.m_DrawPoints.Dispose();
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            if (this.m_LineRenderer == null) return;
            if (this.m_Character == null) return;
            if (this.m_Source == null) return;
            if (this.m_Target == null) return;
            
            float elapsedTime = this.m_Character.Time.Time - this.m_StartTime;
            
            switch (this.m_Phase)
            {
                case Phase.None: this.UpdateNone(elapsedTime); break;
                case Phase.Anticipating: this.UpdateAnticipating(elapsedTime); break;
                case Phase.Throwing: this.UpdateThrowing(elapsedTime); break;
                case Phase.Tensioning: this.UpdateTensioning(elapsedTime); break;
                case Phase.Reeling: this.UpdateReeling(elapsedTime); break;
                default: throw new ArgumentOutOfRangeException();
            }
            
            if (this.m_Segments.Count < 2) return;
            
            if (this.m_DrawPoints.Length != this.m_Resolution)
            {
                this.m_DrawPoints = new NativeArray<Vector3>(
                    this.m_Resolution,
                    Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory
                );
            }
            
            for (int i = 0; i < this.m_Resolution - 1; i++)
            {
                RopeSegment segment = this.m_Segments[i];
                
                float t = Time.fixedDeltaTime > 0f 
                    ? (Time.unscaledTime - Time.fixedUnscaledTime) / Time.fixedUnscaledDeltaTime
                    : 0f;
                
                this.m_DrawPoints[i] = Vector3.Lerp(
                    segment.PreviousPosition,
                    segment.CurrentPosition,
                    t
                );
            }

            this.m_DrawPoints[^1] = this.CurrentPointB;
            NativeSlice<Vector3> drawSlice = new NativeSlice<Vector3>(
                this.m_DrawPoints,
                0,
                this.m_Phase == Phase.Tensioning || this.m_Phase == Phase.Reeling
                    ? this.m_DrawPoints.Length
                    : this.m_DrawPoints.Length - 1
            );
            
            this.m_LineRenderer.positionCount = drawSlice.Length;
            this.m_LineRenderer.SetPositions(drawSlice);

            if (this.m_HookInstance)
            {
                Vector3 direction = drawSlice[^1] - drawSlice[^2];
                Quaternion offset = Quaternion.Euler(this.m_HookRotation);
                
                this.m_HookInstance.transform.position = drawSlice[^1];
                this.m_HookInstance.transform.rotation = Quaternion.LookRotation(direction) * offset;
            }
        }

        private void UpdateNone(float elapsedTime)
        { }

        private void UpdateAnticipating(float elapsedTime)
        {
            Vector3 offset = this.m_Character.transform.TransformDirection(Vector3.forward * 0.01f);
            
            this.CurrentPointA = this.m_Source.position;
            this.CurrentPointB = this.m_Source.transform.position + offset;
            
            this.CurrentGravity = Physics.gravity.normalized;
            this.CurrentTensionFactor = 0f;
            
            if (elapsedTime >= this.m_AnticipationDuration)
            {
                this.m_Phase = Phase.Throwing;
            }
        }

        private void UpdateThrowing(float elapsedTime)
        {
            float t = (elapsedTime - this.m_AnticipationDuration) / this.m_ThrowDuration;
            float parabolicT = Easing.GetEase(this.m_Config.throwHeightEasing, 0f, 1f, t);
            float parabola = this.m_Config.throwHeight * 4f * (parabolicT - parabolicT * parabolicT);
            
            Vector3 source = this.m_Source.position;
            Vector3 target = this.m_Target.position;
            
            this.CurrentPointA = source;
            this.CurrentPointB = Vector3.Lerp(source, target, t) + Vector3.up * parabola;
            
            this.CurrentGravity = Vector3.down;
            this.CurrentTensionFactor = Mathf.Lerp(
                this.m_Config.looseTensionFactor,
                this.m_Config.tightTensionFactor,
                t
            );
            
            if (elapsedTime >= this.m_AnticipationDuration + this.m_ThrowDuration)
            {
                this.m_Phase = Phase.Tensioning;
            }
        }
        
        private void UpdateTensioning(float elapsedTime)
        {
            this.CurrentPointA = this.m_Source.position;
            this.CurrentPointB = this.m_Target.position;
            this.CurrentGravity = Vector3.zero;
            this.CurrentTensionFactor = this.m_Config.tightTensionFactor;
            
            if (elapsedTime >= this.m_AnticipationDuration + this.m_ThrowDuration + this.m_TensionDuration)
            {
                this.m_Phase = Phase.Reeling;
            }
        }

        private void UpdateReeling(float elapsedTime)
        {
            float t = (elapsedTime - this.m_AnticipationDuration - this.m_ThrowDuration - this.m_TensionDuration) / this.m_ReelDuration;
            
            Vector3 source = this.m_Source.position;
            Vector3 target = this.m_Target.position;
            
            GetNormalsToVector(
                (target - source).normalized,
                out Vector3 normalX,
                out Vector3 normalY
            );
            
            Vector3 offset = normalX * Mathf.Sin(Time.time * this.m_Config.reelChaosX + this.m_RandomCycleOffset) +
                             normalY * Mathf.Sin(Time.time * this.m_Config.reelChaosY + this.m_RandomCycleOffset);
            
            offset *= this.m_Config.reelChaosMagnitude;
            
            this.CurrentPointA = source;
            
            this.CurrentPointB = Vector3Utils.Lerp(
                target, 
                Vector3.Lerp(target, source, 0.5f) + offset,
                source,
                Easing.QuadOut(0f, 1f, t)
            );
            
            this.CurrentGravity = Vector3.down;
            this.CurrentTensionFactor = Mathf.Lerp(
                this.m_Config.tightTensionFactor,
                this.m_Config.looseTensionFactor,
                Easing.QuartOut(0f, 1f, t)
            );
            
            float totalDuration = this.m_AnticipationDuration +
                                  this.m_ThrowDuration +
                                  this.m_TensionDuration +
                                  this.m_ReelDuration;
            
            if (elapsedTime >= totalDuration)
            {
                this.Stop();
            }
        }
        
        // FIXED SIMULATION UPDATE: ---------------------------------------------------------------
        
        private void FixedUpdate()
        {
            if (this.m_LineRenderer == null) return;
            if (this.m_Phase == Phase.None) return;

            this.m_Segments[0] = new RopeSegment(
                this.m_Segments[0].CurrentPosition,
                this.CurrentPointA
            );
            
            this.m_Segments[^1] = new RopeSegment(
                this.m_Segments[^1].CurrentPosition,
                this.CurrentPointB
            );
            
            for (int i = 1; i < this.m_Resolution; i++)
            {
                RopeSegment segment = this.m_Segments[i];
                Vector3 velocity = segment.CurrentPosition - segment.PreviousPosition;
                
                segment.PreviousPosition = segment.CurrentPosition;
                segment.CurrentPosition += velocity + this.CurrentGravity * Time.fixedDeltaTime;
                
                this.m_Segments[i] = segment;
            }
            
            for (int i = 0; i < this.m_SolverIterations; i++)
            {
                RopeSegment segmentPointA = this.m_Segments[0];
                segmentPointA.CurrentPosition = this.CurrentPointA;
                this.m_Segments[0] = segmentPointA;
                
                RopeSegment segmentPointB = this.m_Segments[^1];
                segmentPointB.CurrentPosition = this.CurrentPointB;
                this.m_Segments[^1] = segmentPointB;
                
                float ropeLengthPerSegment = 
                    Vector3.Distance(this.CurrentPointA, this.CurrentPointB) * 
                    this.CurrentTensionFactor / this.m_Resolution;
                
                for (int j = 0; j < this.m_Resolution - 1; j++)
                {
                    RopeSegment segment0 = this.m_Segments[j];
                    RopeSegment segment1 = this.m_Segments[j + 1];
                
                    float distance = (segment0.CurrentPosition - segment1.CurrentPosition).magnitude;
                    
                    float error = Mathf.Abs(distance - ropeLengthPerSegment);
                    Vector3 smoothDirection = Vector3.zero;
                
                    if (distance > ropeLengthPerSegment)
                    {
                        smoothDirection = (segment0.CurrentPosition - segment1.CurrentPosition).normalized;
                    }
                    else if (distance < ropeLengthPerSegment)
                    {
                        smoothDirection = (segment1.CurrentPosition - segment0.CurrentPosition).normalized;
                    }
        
                    Vector3 smoothAmount = smoothDirection * error;
                    if (j != 0)
                    {
                        segment0.CurrentPosition -= smoothAmount * 0.5f;
                        this.m_Segments[j] = segment0;
                    
                        segment1.CurrentPosition += smoothAmount * 0.5f;
                    }
                    else
                    {
                        segment1.CurrentPosition += smoothAmount;
                    }
                
                    this.m_Segments[j + 1] = segment1;
                }
            }
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Throw(
            Character character,
            Transform source,
            Transform target,
            float anticipationDuration,
            float throwDuration,
            float tensionDuration,
            float reelDuration)
        {
            this.m_Phase = Phase.None;
            this.m_Segments.Clear();
            
            if (character == null) return;
            if (source == null) return;
            if (target == null) return;
            
            this.m_Character = character;
            this.m_Source = source;
            this.m_Target = target;
            
            this.m_AnticipationDuration = anticipationDuration;
            this.m_ThrowDuration = throwDuration;
            this.m_TensionDuration = tensionDuration;
            this.m_ReelDuration = reelDuration;
            
            this.CurrentPointA = source.position;
            this.CurrentPointB = character.transform.TransformPoint(Vector3.forward * 0.01f);
            this.CurrentGravity = Vector3.zero;
            this.CurrentTensionFactor = 1f;
            
            for (int i = 0; i < this.m_Resolution; i++)
            {
                float t = (float) i / this.m_Resolution;
                Vector3 position = Vector3.Lerp(this.CurrentPointA, this.CurrentPointB, t);
                
                this.m_Segments.Add(new RopeSegment(position));
            }

            this.m_StartTime = character.Time.Time;
            this.m_Phase = Phase.Anticipating;
            
            if (this.m_LineRenderer != null) this.m_LineRenderer.enabled = true;
            if (this.m_HookInstance != null) this.m_HookInstance.SetActive(true);
        }

        public void Stop()
        {
            if (this.m_Phase != Phase.None)
            {
                this.m_Segments.Clear();
                this.m_Phase = Phase.None;
                
                this.m_Character = null;
                this.m_Source = null;
                this.m_Target = null;

                this.m_LineRenderer.enabled = false;
                if (this.m_HookInstance != null) this.m_HookInstance.SetActive(false);
            }
        }

        public void StopWithReel()
        {
            switch (this.m_Phase)
            {
                case Phase.None:
                case Phase.Reeling:
                    return;
                
                case Phase.Anticipating:
                case Phase.Throwing:
                case Phase.Tensioning:
                    this.m_AnticipationDuration = 0f;
                    this.m_ThrowDuration = 0f;
                    this.m_TensionDuration = 0f;
            
                    this.m_Phase = Phase.Reeling;
                    this.m_StartTime = this.m_Character != null
                        ? this.m_Character.Time.Time
                        : Time.time;
                    break;
                
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static void GetNormalsToVector(
            Vector3 direction1,
            out Vector3 normal1,
            out Vector3 normal2)
        {
            direction1.Normalize();
            
            float minComponent = MathUtils.Min(
                Mathf.Abs(direction1.x),
                Mathf.Abs(direction1.y),
                Mathf.Abs(direction1.z)
            );
            
            if (Mathf.Approximately(Mathf.Abs(direction1.x), minComponent))
            {
                normal1 = new Vector3(0, -direction1.z, direction1.y);
            }
            else if (Mathf.Approximately(Mathf.Abs(direction1.y), minComponent))
            {
                normal1 = new Vector3(-direction1.z, 0, direction1.x);
            }
            else
            {
                normal1 = new Vector3(-direction1.y, direction1.x, 0);
            }

            normal1.Normalize();
            
            normal2 = Vector3.Cross(direction1, normal1);
            normal2.Normalize();
        }
    }
}