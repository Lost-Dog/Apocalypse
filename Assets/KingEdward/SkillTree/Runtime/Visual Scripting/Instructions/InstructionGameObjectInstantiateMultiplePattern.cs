using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Instantiate Multiple with Pattern")]
    [Description("Creates multiple instances of a referenced game object using various patterns (Circle, Line, Grid, Random, Wave, Spiral, Heart, Star)")]

    [Category("KingEdward/Skill Tree/VFX/Instantiate Multiple with Pattern")]
    
    [Parameter("Game Object", "Game Object reference that is instantiated")]
    [Parameter("Position", "The center position for the instantiation")]
    [Parameter("Rotation", "The base rotation of the new game object instances")]
    [Parameter("Count", "Number of instances to create")]
    [Parameter("Pattern", "The positioning pattern to use")]
    [Parameter("Size", "Scale factor for the pattern")]
    [Parameter("Align Reference", "The GameObject whose local axes will be used for alignment")]
    [Parameter("Save", "Variable where each instantiated game object is stored")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionGameObjectInstantiateMultiplePattern : Instruction
    {
        // ENUMS: ---------------------------------------------------------------------------------
        
        public enum PatternType
        {
            Circle,
            Line,
            Grid,
            Random,
            Wave,
            Spiral,
            Heart,
            Star
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private PropertyGetInstantiate m_GameObject = new PropertyGetInstantiate();

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();

        [SerializeField]
        private PropertyGetGameObject m_AlignReference = GetGameObjectNone.Create();

        [SerializeField]
        private PropertyGetInteger m_Count = new PropertyGetInteger(8);

        [SerializeField]
        private PropertySetGameObject m_Save = new PropertySetGameObject();

        [SerializeField]
        private PatternType m_Pattern = PatternType.Circle;

        [SerializeField]
        private float m_Size = 3f;

        [SerializeField]
        private float m_StartAngle = 0f;

        [SerializeField]
        private LookDirection m_LookDirection = LookDirection.None;

        [SerializeField]
        private int m_GridWidth = 3;

        [SerializeField]
        private float m_WaveFrequency = 1f;

        [SerializeField]
        private float m_WaveAmplitude = 1f;

        [SerializeField]
        private float m_SpiralHeight = 1f;  // Height multiplier for spiral

        [SerializeField]
        private int m_StarPoints = 5;  // Number of points in the star

        [SerializeField]
        private float m_StarInnerRadius = 0.5f;  // Inner radius multiplier for star
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Instantiate Multiple {this.m_GameObject} in {this.m_Pattern}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Vector3 centerPosition = this.m_Position.Get(args);
            Quaternion baseRotation = this.m_Rotation.Get(args);

            Transform alignTransform = this.m_AlignReference.Get<Transform>(args);
            if (alignTransform == null)
            {
                // Use world space if no align reference
                alignTransform = new GameObject("TempAlign").transform;
                alignTransform.position = centerPosition;
                alignTransform.rotation = Quaternion.identity;
            }
            
            int instanceCount = (int)this.m_Count.Get(args);
            if (instanceCount <= 0) return DefaultResult;

            List<Vector3> positions = GeneratePatternPositions(instanceCount, centerPosition, alignTransform);

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];

                // Calculate rotation
                Quaternion rotation = baseRotation;
                if (m_LookDirection != LookDirection.None)
                {
                    Vector3 lookDir = Vector3.zero;
                    
                    if (m_LookDirection == LookDirection.LookAtCenter)
                    {
                        lookDir = centerPosition - position;
                    }
                    else if (m_LookDirection == LookDirection.LookOutFromCenter)
                    {
                        lookDir = position - centerPosition;
                    }
                    
                    if (lookDir != Vector3.zero)
                    {
                        rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                    }
                }

                InstantiateAtPosition(position, rotation, args);
            }

            // Clean up temporary transform if created
            if (alignTransform.name == "TempAlign")
            {
                UnityEngine.Object.DestroyImmediate(alignTransform.gameObject);
            }

            return DefaultResult;
        }

        // PATTERN GENERATION: --------------------------------------------------------------------

        private List<Vector3> GeneratePatternPositions(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();

            switch (m_Pattern)
            {
                case PatternType.Circle:
                    positions = GenerateCirclePattern(count, center, alignTransform);
                    break;
                case PatternType.Line:
                    positions = GenerateLinePattern(count, center, alignTransform);
                    break;
                case PatternType.Grid:
                    positions = GenerateGridPattern(count, center, alignTransform);
                    break;
                case PatternType.Random:
                    positions = GenerateRandomPattern(count, center, alignTransform);
                    break;
                case PatternType.Wave:
                    positions = GenerateWavePattern(count, center, alignTransform);
                    break;
                case PatternType.Spiral:
                    positions = GenerateSpiralPattern(count, center, alignTransform);
                    break;
                case PatternType.Heart:
                    positions = GenerateHeartPattern(count, center, alignTransform);
                    break;
                case PatternType.Star:
                    positions = GenerateStarPattern(count, center, alignTransform);
                    break;
            }

            return positions;
        }

        private List<Vector3> GenerateCirclePattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = m_StartAngle + (i * angleStep);
                float radians = angle * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
                direction = alignTransform.TransformDirection(direction);
                Vector3 position = center + (direction * m_Size);
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateLinePattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();
            float spacing = m_Size / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) / 2f) * spacing;
                Vector3 direction = alignTransform.forward * offset;
                Vector3 position = center + direction;
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateGridPattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();
            int width = m_GridWidth;
            int height = Mathf.CeilToInt((float)count / width);
            float spacing = m_Size / Mathf.Max(width, height);

            for (int i = 0; i < count; i++)
            {
                int x = i % width;
                int z = i / width;
                
                Vector3 offset = new Vector3(
                    (x - (width - 1) / 2f) * spacing,
                    0f,
                    (z - (height - 1) / 2f) * spacing
                );
                
                offset = alignTransform.TransformDirection(offset);
                Vector3 position = center + offset;
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateRandomPattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Vector3 randomDirection = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0f,
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * UnityEngine.Random.Range(0f, m_Size);

                randomDirection = alignTransform.TransformDirection(randomDirection);
                Vector3 position = center + randomDirection;
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateWavePattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();
            float spacing = m_Size / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float x = (t - 0.5f) * m_Size;
                float z = Mathf.Sin(t * m_WaveFrequency * Mathf.PI * 2) * m_WaveAmplitude;

                Vector3 offset = new Vector3(x, 0f, z);
                offset = alignTransform.TransformDirection(offset);
                Vector3 position = center + offset;
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateSpiralPattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float angle = t * Mathf.PI * 4; // 2 full rotations
                float radius = t * m_Size;
                
                // Create the XZ plane direction first (without height)
                Vector3 direction = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f, // Keep Y at 0 for now
                    Mathf.Sin(angle) * radius
                );

                // Transform only the XZ direction
                direction = alignTransform.TransformDirection(direction);
                
                // Calculate height separately and add it after transformation
                float height = t * m_SpiralHeight;
                Vector3 position = center + direction + (Vector3.up * height);
                
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateHeartPattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count * 2 * Mathf.PI;
                
                // Heart shape parametric equations
                float x = 16 * Mathf.Pow(Mathf.Sin(t), 3);
                float z = 13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t);
                
                Vector3 offset = new Vector3(x, 0f, z) * (m_Size / 20f);
                offset = alignTransform.TransformDirection(offset);
                Vector3 position = center + offset;
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateStarPattern(int count, Vector3 center, Transform alignTransform)
        {
            List<Vector3> positions = new List<Vector3>();
            
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float angle = t * 2 * Mathf.PI;
                
                // Alternate between outer and inner radius based on star points
                float anglePerPoint = (2 * Mathf.PI) / (m_StarPoints * 2);
                int pointIndex = Mathf.FloorToInt(angle / anglePerPoint);
                bool isInnerPoint = (pointIndex % 2 == 1);
                
                float radius = isInnerPoint ? m_Size * m_StarInnerRadius : m_Size;

                Vector3 direction = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                direction = alignTransform.TransformDirection(direction);
                Vector3 position = center + direction;
                positions.Add(position);
            }

            return positions;
        }

        // HELPER METHODS: ------------------------------------------------------------------------

        private void InstantiateAtPosition(Vector3 position, Quaternion rotation, Args args)
        {
            GameObject instance = this.m_GameObject.Get(args, position, rotation);

            if (instance != null)
            {
                ProjectileBehavior.RegisterCasterFromArgs(instance, args);
                Transform parent = this.m_Parent.Get<Transform>(args);
                if (parent != null) instance.transform.SetParent(parent);
                
                this.m_Save.Set(instance, args);
            }
        }

    }
} 