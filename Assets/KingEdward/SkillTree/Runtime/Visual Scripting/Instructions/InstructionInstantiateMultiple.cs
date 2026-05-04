using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Instantiate Multiple")]
    [Description("Creates multiple instances of a prefab at the same position")]

    [Category("KingEdward/Skill Tree/VFX/Instantiate Multiple")]
    
    [Parameter("Prefab", "The prefab to instantiate")]
    [Parameter("Position", "The position to spawn at")]
    [Parameter("Rotation", "The rotation of spawned objects")]
    [Parameter("Count", "Number of instances to create")]
    [Parameter("Parent", "Optional parent transform")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionInstantiateMultiple : Instruction
    {
        [SerializeField] private PropertyGetInstantiate m_Prefab = new PropertyGetInstantiate();
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
        [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetInteger m_Count = new PropertyGetInteger(5);
        [SerializeField] private LookDirection m_LookDirection = LookDirection.None;
        [SerializeField] private PropertySetGameObject m_Save = new PropertySetGameObject();
        
        public override string Title => $"Instantiate {m_Count}x {m_Prefab}";
        
        protected override Task Run(Args args)
        {
            Vector3 centerPosition = m_Position.Get(args);
            Quaternion rotation = m_Rotation.Get(args);
            int count = (int)m_Count.Get(args);
            Transform parent = m_Parent.Get<Transform>(args);
            
            for (int i = 0; i < count; i++)
            {
                GameObject instance = m_Prefab.Get(args, centerPosition, rotation);
                
                if (instance != null)
                {
                    ProjectileBehavior.RegisterCasterFromArgs(instance, args);
                    if (parent != null)
                    {
                        instance.transform.SetParent(parent);
                    }
                    
                    // Apply look direction
                    if (m_LookDirection != LookDirection.None)
                    {
                        Vector3 instancePosition = instance.transform.position;
                        Vector3 direction = Vector3.zero;
                        
                        if (m_LookDirection == LookDirection.LookAtCenter)
                        {
                            // Look towards center
                            direction = centerPosition - instancePosition;
                        }
                        else if (m_LookDirection == LookDirection.LookOutFromCenter)
                        {
                            // Look away from center
                            direction = instancePosition - centerPosition;
                        }
                        
                        if (direction != Vector3.zero)
                        {
                            instance.transform.rotation = Quaternion.LookRotation(direction);
                        }
                    }
                    
                    m_Save.Set(instance, args);
                }
            }
            
            return DefaultResult;
        }
    }
}
