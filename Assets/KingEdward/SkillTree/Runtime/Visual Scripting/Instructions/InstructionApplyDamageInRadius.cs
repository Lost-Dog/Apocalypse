using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Execute In Radius")]
    [Description("Executes instructions on all targets in a radius")]

    [Category("KingEdward/Skill Tree/Combat/Execute In Radius")]
    
    [Parameter("Position", "The center position of the area")]
    [Parameter("Radius", "The radius of the area")]
    [Parameter("On Hit", "Instructions to execute on each hit target")]
    
    [Image(typeof(IconDamageRadius), ColorTheme.Type.Red)]
    
    [Serializable]
    public class InstructionApplyDamageInRadius : Instruction
    {
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetDecimal m_Radius = new PropertyGetDecimal(5f);
        [SerializeField] private InstructionList m_OnHit = new InstructionList();
        [SerializeField] private PropertyGetString m_TargetTag = new PropertyGetString("Enemy");
        [SerializeField] private LayerMask m_LayerMask = -1;
        [SerializeField] private bool m_UseTag = true;
        [SerializeField] private PropertyGetGameObject m_Caster = GetGameObjectSelf.Create();
        
        public override string Title => $"Execute in Radius {m_Radius}";
        
        protected override Task Run(Args args)
        {
            Vector3 position = m_Position.Get(args);
            float radius = (float)m_Radius.Get(args);
            string targetTag = m_TargetTag.Get(args);
            GameObject caster = m_Caster.Get(args);
            
            // Create independent executor
            GameObject executorObject = new GameObject("ExecuteInRadiusExecutor");
            executorObject.transform.position = position;
            RadiusExecutor executor = executorObject.AddComponent<RadiusExecutor>();
            
            // Initialize and execute
            executor.Initialize(position, radius, m_OnHit, targetTag, m_LayerMask, m_UseTag, caster);
            
            // Return immediately
            return DefaultResult;
        }
        
        // Independent executor component
        private class RadiusExecutor : MonoBehaviour
        {
            private Vector3 m_Position;
            private float m_Radius;
            private InstructionList m_OnHit;
            private string m_TargetTag;
            private LayerMask m_LayerMask;
            private bool m_UseTag;
            private GameObject m_Caster;
            
            public void Initialize(Vector3 position, float radius, InstructionList onHit, 
                string targetTag, LayerMask layerMask, bool useTag, GameObject caster)
            {
                m_Position = position;
                m_Radius = radius;
                m_OnHit = onHit;
                m_TargetTag = targetTag;
                m_LayerMask = layerMask;
                m_UseTag = useTag;
                m_Caster = caster;
            }
            
            private void Start()
            {
                // Execute on next frame
                ExecuteRadius();
                Destroy(gameObject);
            }
            
            private void ExecuteRadius()
            {
                Collider[] colliders = Physics.OverlapSphere(m_Position, m_Radius, m_LayerMask);
                
                foreach (Collider col in colliders)
                {
                    if (col == null) continue;
                    
                    // Skip caster
                    if (m_Caster != null && col.gameObject == m_Caster) continue;
                    
                    // Check tag filter
                    if (m_UseTag && !string.IsNullOrEmpty(m_TargetTag) && !col.CompareTag(m_TargetTag)) continue;
                    
                    // Execute instructions with caster as Self and target as Target
                    Args targetArgs = new Args(m_Caster, col.gameObject);
                    _ = m_OnHit.Run(targetArgs);
                }
            }
        }
    }
}
