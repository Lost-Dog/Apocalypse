using System;
using System.Collections;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    /// <summary>
    /// Static registry for last created pull
    /// </summary>
    public static class PullRegistry
    {
        public static GameObject LastPull { get; set; }
    }
    
    [Title("Pull Enemies To Position")]
    [Description("Pulls enemies towards a position (black hole effect)")]

    [Category("KingEdward/Skill Tree/Combat/Pull Enemies To Position")]
    
    [Parameter("Position", "The target position to pull enemies towards")]
    [Parameter("Radius", "The radius to search for enemies")]
    [Parameter("Force", "The pull force strength")]
    [Parameter("Duration", "How long the pull effect lasts")]
    [Parameter("Tag", "Optional tag filter for enemies")]
    [Parameter("Layer", "Layer mask for enemy detection")]
    
    [Image(typeof(IconPullTo), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class InstructionPullEnemiesToPosition : Instruction
    {
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetDecimal m_Radius = new PropertyGetDecimal(10f);
        [SerializeField] private PropertyGetDecimal m_Force = new PropertyGetDecimal(5f);
        [SerializeField] private PropertyGetDecimal m_Duration = new PropertyGetDecimal(2f);
        [SerializeField] private PropertyGetDecimal m_MinDistance = new PropertyGetDecimal(2f);
        [SerializeField] private PropertyGetDecimal m_SeparationForce = new PropertyGetDecimal(2f);
        [SerializeField] private PropertyGetString m_Tag = new PropertyGetString("Enemy");
        [SerializeField] private LayerMask m_LayerMask = -1;
        [SerializeField] private bool m_UseTag = true;
        [SerializeField] private bool m_AffectRigidbodies = true;
        [SerializeField] private bool m_AffectCharacters = true;
        
        public override string Title => $"Pull Enemies to {m_Position} (R:{m_Radius})";
        
        protected override Task Run(Args args)
        {
            Vector3 targetPosition = m_Position.Get(args);
            float radius = (float)m_Radius.Get(args);
            float force = (float)m_Force.Get(args);
            float duration = (float)m_Duration.Get(args);
            float minDistance = (float)m_MinDistance.Get(args);
            float separationForce = (float)m_SeparationForce.Get(args);
            string tag = m_Tag.Get(args);
            
            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(targetPosition, radius, m_LayerMask);
            
            if (colliders.Length == 0)
            {
                return DefaultResult;
            }
            
            // Create independent pull controller
            GameObject pullObject = new GameObject("PullEnemiesController");
            pullObject.transform.position = targetPosition;
            PullController controller = pullObject.AddComponent<PullController>();
            
            // Register as last pull
            PullRegistry.LastPull = pullObject;
            
            // Initialize and let it run independently
            controller.Initialize(
                colliders,
                targetPosition,
                force,
                duration,
                minDistance,
                separationForce,
                tag,
                m_UseTag,
                m_AffectRigidbodies,
                m_AffectCharacters
            );
            
            // Return immediately
            return DefaultResult;
        }
        
        // Independent pull controller
        private class PullController : MonoBehaviour
        {
            private Collider[] m_Colliders;
            private Vector3 m_TargetPosition;
            private float m_Force;
            private float m_Duration;
            private float m_MinDistance;
            private float m_SeparationForce;
            private string m_Tag;
            private bool m_UseTag;
            private bool m_AffectRigidbodies;
            private bool m_AffectCharacters;
            
            private float m_Elapsed = 0f;
            
            public void Initialize(
                Collider[] colliders,
                Vector3 targetPosition,
                float force,
                float duration,
                float minDistance,
                float separationForce,
                string tag,
                bool useTag,
                bool affectRigidbodies,
                bool affectCharacters)
            {
                m_Colliders = colliders;
                m_TargetPosition = targetPosition;
                m_Force = force;
                m_Duration = duration;
                m_MinDistance = minDistance;
                m_SeparationForce = separationForce;
                m_Tag = tag;
                m_UseTag = useTag;
                m_AffectRigidbodies = affectRigidbodies;
                m_AffectCharacters = affectCharacters;
            }
            
            private void Update()
            {
                m_Elapsed += UnityEngine.Time.deltaTime;
                
                // Check if pull should end
                if (m_Elapsed >= m_Duration)
                {
                    Destroy(gameObject);
                    return;
                }
                
                // Apply pull force to all targets
                foreach (Collider col in m_Colliders)
                {
                    if (col == null) continue;
                    
                    // Check tag filter
                    if (m_UseTag && !string.IsNullOrEmpty(m_Tag) && !col.CompareTag(m_Tag))
                    {
                        continue;
                    }
                    
                    Vector3 pullDirection = (m_TargetPosition - col.transform.position).normalized;
                    float distance = Vector3.Distance(col.transform.position, m_TargetPosition);
                    
                    // Calculate pull force
                    Vector3 totalForce = Vector3.zero;
                    
                    // Add pull force if not at minimum distance
                    if (distance > m_MinDistance)
                    {
                        totalForce += pullDirection * m_Force * UnityEngine.Time.deltaTime;
                    }
                    
                    // Add separation force from other enemies
                    foreach (Collider other in m_Colliders)
                    {
                        if (other == null || other == col) continue;
                        if (m_UseTag && !string.IsNullOrEmpty(m_Tag) && !other.CompareTag(m_Tag)) continue;
                        
                        float distToOther = Vector3.Distance(col.transform.position, other.transform.position);
                        if (distToOther < m_MinDistance * 0.8f)
                        {
                            Vector3 separationDir = (col.transform.position - other.transform.position).normalized;
                            totalForce += separationDir * m_SeparationForce * UnityEngine.Time.deltaTime;
                        }
                    }
                    
                    if (totalForce == Vector3.zero) continue;
                    
                    // Try to affect Rigidbody
                    if (m_AffectRigidbodies)
                    {
                        Rigidbody rb = col.GetComponent<Rigidbody>();
                        if (rb != null && !rb.isKinematic)
                        {
                            rb.AddForce(totalForce, ForceMode.VelocityChange);
                            continue;
                        }
                    }
                    
                    // Try to affect Character
                    if (m_AffectCharacters)
                    {
                        Character character = col.GetComponent<Character>();
                        if (character != null)
                        {
                            character.transform.position += totalForce;
                            continue;
                        }
                    }
                    
                    // Fallback: directly move transform
                    col.transform.position += totalForce;
                }
            }
        }
    }
}
