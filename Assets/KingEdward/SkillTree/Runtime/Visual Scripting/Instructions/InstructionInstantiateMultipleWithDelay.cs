using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using KingEdward.SkillTree;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Instantiate Multiple With Delay")]
    [Description("Creates multiple instances of a prefab with a delay between each spawn")]

    [Category("KingEdward/Skill Tree/VFX/Instantiate Multiple With Delay")]
    
    [Parameter("Prefab", "The prefab to instantiate")]
    [Parameter("Position", "The position to spawn at")]
    [Parameter("Rotation", "The rotation of spawned objects")]
    [Parameter("Count", "Number of instances to create")]
    [Parameter("Delay", "Delay between each spawn (in seconds)")]
    [Parameter("Parent", "Optional parent transform")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Serializable]
    public class InstructionInstantiateMultipleWithDelay : Instruction
    {
        [SerializeField] private PropertyGetInstantiate m_Prefab = new PropertyGetInstantiate();
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
        [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetInteger m_Count = new PropertyGetInteger(5);
        [SerializeField] private PropertyGetDecimal m_Delay = new PropertyGetDecimal(0.2f);
        [SerializeField] private bool m_WaitForCompletion = true;
        [SerializeField] private LookDirection m_LookDirection = LookDirection.None;
        [SerializeField] private PropertySetGameObject m_Save = new PropertySetGameObject();
        
        public override string Title => $"Instantiate {m_Count}x {m_Prefab} (Delay: {m_Delay}s)";
        
        protected override async Task Run(Args args)
        {
            Vector3 position = m_Position.Get(args);
            Quaternion rotation = m_Rotation.Get(args);
            int count = (int)m_Count.Get(args);
            float delay = (float)m_Delay.Get(args);
            Transform parent = m_Parent.Get<Transform>(args);
            
            if (!m_WaitForCompletion)
            {
                // Fire and forget - spawn in background
                _ = SpawnWithDelay(args, position, rotation, count, delay, parent);
                return;
            }
            
            // Wait for all spawns to complete
            await SpawnWithDelay(args, position, rotation, count, delay, parent);
        }
        
        private async Task SpawnWithDelay(Args args, Vector3 centerPosition, Quaternion rotation, int count, float delay, Transform parent)
        {
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
                
                // Wait for delay before next spawn (except on last iteration)
                if (i < count - 1)
                {
                    await Task.Delay((int)(delay * 1000));
                }
            }
        }

    }
}
