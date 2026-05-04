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
    [Title("Spawn In Formation")]
    [Description("Spawns objects in military/tactical formations (V, Line, Square, Hexagon, Triangle, Diamond)")]

    [Category("KingEdward/Skill Tree/VFX/Spawn In Formation")]
    
    [Parameter("Prefab", "The prefab to spawn")]
    [Parameter("Position", "The center position for the formation")]
    [Parameter("Rotation", "The base rotation of spawned objects")]
    [Parameter("Count", "Number of objects to spawn")]
    [Parameter("Formation", "The formation type")]
    [Parameter("Spacing", "Distance between objects")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionSpawnInFormation : Instruction
    {
        public enum FormationType
        {
            VFormation,      // V shape (flying geese)
            Line,            // Straight line
            Square,          // Square perimeter
            Hexagon,         // Hexagon shape
            Triangle,        // Triangle shape
            Diamond,         // Diamond/rhombus shape
            Circle,          // Circle formation
            TwoLines,        // Two parallel lines
            Wedge            // Wedge/arrow shape
        }

        [SerializeField] private PropertyGetInstantiate m_Prefab = new PropertyGetInstantiate();
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
        [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetInteger m_Count = new PropertyGetInteger(5);
        [SerializeField] private FormationType m_Formation = FormationType.VFormation;
        [SerializeField] private float m_Spacing = 2f;
        [SerializeField] private bool m_FaceForward = true;
        [SerializeField] private PropertySetGameObject m_Save = new PropertySetGameObject();
        
        public override string Title => $"Spawn {m_Count} in {m_Formation}";
        
        protected override Task Run(Args args)
        {
            Vector3 centerPosition = m_Position.Get(args);
            Quaternion baseRotation = m_Rotation.Get(args);
            int count = (int)m_Count.Get(args);
            
            if (count <= 0) return DefaultResult;
            
            List<Vector3> positions = GenerateFormationPositions(count, centerPosition, baseRotation);
            
            foreach (Vector3 position in positions)
            {
                Quaternion rotation = m_FaceForward ? baseRotation : Quaternion.identity;
                GameObject instance = m_Prefab.Get(args, position, rotation);
                
                if (instance != null)
                {
                    ProjectileBehavior.RegisterCasterFromArgs(instance, args);
                    Transform parent = m_Parent.Get<Transform>(args);
                    if (parent != null) instance.transform.SetParent(parent);
                    m_Save.Set(instance, args);
                }
            }
            
            return DefaultResult;
        }
        
        private List<Vector3> GenerateFormationPositions(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            
            switch (m_Formation)
            {
                case FormationType.VFormation:
                    positions = GenerateVFormation(count, center, rotation);
                    break;
                case FormationType.Line:
                    positions = GenerateLine(count, center, rotation);
                    break;
                case FormationType.Square:
                    positions = GenerateSquare(count, center, rotation);
                    break;
                case FormationType.Hexagon:
                    positions = GenerateHexagon(count, center, rotation);
                    break;
                case FormationType.Triangle:
                    positions = GenerateTriangle(count, center, rotation);
                    break;
                case FormationType.Diamond:
                    positions = GenerateDiamond(count, center, rotation);
                    break;
                case FormationType.Circle:
                    positions = GenerateCircle(count, center, rotation);
                    break;
                case FormationType.TwoLines:
                    positions = GenerateTwoLines(count, center, rotation);
                    break;
                case FormationType.Wedge:
                    positions = GenerateWedge(count, center, rotation);
                    break;
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateVFormation(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int halfCount = count / 2;
            
            // Center point
            if (count % 2 == 1)
            {
                positions.Add(center);
            }
            
            // Left and right wings
            for (int i = 1; i <= halfCount; i++)
            {
                Vector3 leftOffset = new Vector3(-i * m_Spacing, 0, -i * m_Spacing);
                Vector3 rightOffset = new Vector3(i * m_Spacing, 0, -i * m_Spacing);
                
                positions.Add(center + rotation * leftOffset);
                positions.Add(center + rotation * rightOffset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateLine(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            float startOffset = -(count - 1) * m_Spacing / 2f;
            
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(startOffset + i * m_Spacing, 0, 0);
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateSquare(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int perSide = Mathf.CeilToInt(count / 4f);
            float size = (perSide - 1) * m_Spacing / 2f;
            
            int spawned = 0;
            
            // Top side
            for (int i = 0; i < perSide && spawned < count; i++, spawned++)
            {
                Vector3 offset = new Vector3(-size + i * m_Spacing, 0, size);
                positions.Add(center + rotation * offset);
            }
            
            // Right side
            for (int i = 1; i < perSide && spawned < count; i++, spawned++)
            {
                Vector3 offset = new Vector3(size, 0, size - i * m_Spacing);
                positions.Add(center + rotation * offset);
            }
            
            // Bottom side
            for (int i = 1; i < perSide && spawned < count; i++, spawned++)
            {
                Vector3 offset = new Vector3(size - i * m_Spacing, 0, -size);
                positions.Add(center + rotation * offset);
            }
            
            // Left side
            for (int i = 1; i < perSide - 1 && spawned < count; i++, spawned++)
            {
                Vector3 offset = new Vector3(-size, 0, -size + i * m_Spacing);
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateHexagon(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * 360f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * m_Spacing * 2f,
                    0,
                    Mathf.Sin(angle) * m_Spacing * 2f
                );
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateTriangle(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int rows = Mathf.CeilToInt((-1 + Mathf.Sqrt(1 + 8 * count)) / 2f);
            int spawned = 0;
            
            for (int row = 0; row < rows && spawned < count; row++)
            {
                int objectsInRow = row + 1;
                float rowOffset = -row * m_Spacing;
                float startX = -(objectsInRow - 1) * m_Spacing / 2f;
                
                for (int col = 0; col < objectsInRow && spawned < count; col++, spawned++)
                {
                    Vector3 offset = new Vector3(startX + col * m_Spacing, 0, rowOffset);
                    positions.Add(center + rotation * offset);
                }
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateDiamond(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int halfCount = count / 2;
            
            // Top half
            for (int i = 0; i <= halfCount; i++)
            {
                Vector3 offset = new Vector3(0, 0, i * m_Spacing);
                positions.Add(center + rotation * offset);
                
                if (i > 0 && positions.Count < count)
                {
                    positions.Add(center + rotation * new Vector3(-i * m_Spacing, 0, 0));
                }
                if (i > 0 && positions.Count < count)
                {
                    positions.Add(center + rotation * new Vector3(i * m_Spacing, 0, 0));
                }
            }
            
            // Bottom half
            for (int i = 1; i <= halfCount && positions.Count < count; i++)
            {
                Vector3 offset = new Vector3(0, 0, -i * m_Spacing);
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateCircle(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            float radius = m_Spacing * count / (2f * Mathf.PI);
            
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * 360f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateTwoLines(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int perLine = Mathf.CeilToInt(count / 2f);
            float startOffset = -(perLine - 1) * m_Spacing / 2f;
            float lineSpacing = m_Spacing * 1.5f;
            
            // First line
            for (int i = 0; i < perLine && positions.Count < count; i++)
            {
                Vector3 offset = new Vector3(startOffset + i * m_Spacing, 0, lineSpacing / 2f);
                positions.Add(center + rotation * offset);
            }
            
            // Second line
            for (int i = 0; i < perLine && positions.Count < count; i++)
            {
                Vector3 offset = new Vector3(startOffset + i * m_Spacing, 0, -lineSpacing / 2f);
                positions.Add(center + rotation * offset);
            }
            
            return positions;
        }
        
        private List<Vector3> GenerateWedge(int count, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            int rows = Mathf.CeilToInt(Mathf.Sqrt(count * 2));
            int spawned = 0;
            
            for (int row = 0; row < rows && spawned < count; row++)
            {
                int objectsInRow = Mathf.Min(row * 2 + 1, count - spawned);
                float rowOffset = row * m_Spacing;
                float startX = -(objectsInRow - 1) * m_Spacing / 2f;
                
                for (int col = 0; col < objectsInRow && spawned < count; col++, spawned++)
                {
                    Vector3 offset = new Vector3(startX + col * m_Spacing, 0, rowOffset);
                    positions.Add(center + rotation * offset);
                }
            }
            
            return positions;
        }
    }
}
