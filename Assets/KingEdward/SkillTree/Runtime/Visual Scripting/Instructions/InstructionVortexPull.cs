using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    /// <summary>
    /// Static registry for last created vortex
    /// </summary>
    public static class VortexRegistry
    {
        public static GameObject LastVortex { get; set; }
    }
    
    [Title("Vortex Pull")]
    [Description("Pulls enemies in a spiral/vortex pattern (hurricane effect)")]

    [Category("KingEdward/Skill Tree/Combat/Vortex Pull")]
    
    [Parameter("Position", "The center position of the vortex")]
    [Parameter("Radius", "The radius to search for enemies")]
    [Parameter("Pull Force", "The inward pull force strength")]
    [Parameter("Spin Force", "The rotational/spin force strength")]
    [Parameter("Duration", "How long the vortex effect lasts")]
    [Parameter("VFX Prefab", "Optional VFX that follows the vortex center")]
    
    [Image(typeof(IconVortex), ColorTheme.Type.Purple)]
    
    [Serializable]
    public class InstructionVortexPull : Instruction
    {
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetDecimal m_Radius = new PropertyGetDecimal(10f);
        [SerializeField] private PropertyGetDecimal m_PullForce = new PropertyGetDecimal(5f);
        [SerializeField] private PropertyGetDecimal m_SpinForce = new PropertyGetDecimal(8f);
        [SerializeField] private PropertyGetDecimal m_Duration = new PropertyGetDecimal(3f);
        [SerializeField] private PropertyGetDecimal m_MinDistance = new PropertyGetDecimal(1f);
        [SerializeField] private PropertyGetDecimal m_SeparationForce = new PropertyGetDecimal(3f);
        [SerializeField] private PropertyGetDecimal m_SeparationRadius = new PropertyGetDecimal(2f);
        [SerializeField] private PropertyGetGameObject m_VFXPrefab = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetPosition m_VFXOffset = GetPositionVector3.Create(Vector3.zero);
        [SerializeField] private PropertyGetDirection m_MoveDirection = GetDirectionConstantForward.Create;
        [SerializeField] private PropertyGetDecimal m_MoveSpeed = new PropertyGetDecimal(0f);
        [SerializeField] private PropertyGetDecimal m_SinuousAmplitude = new PropertyGetDecimal(0f);
        [SerializeField] private PropertyGetDecimal m_SinuousFrequency = new PropertyGetDecimal(1f);
        [SerializeField] private PropertyGetString m_Tag = new PropertyGetString("Enemy");
        [SerializeField] private LayerMask m_LayerMask = -1;
        [SerializeField] private bool m_UseTag = true;
        [SerializeField] private bool m_AffectRigidbodies = true;
        [SerializeField] private bool m_AffectCharacters = true;
        [SerializeField] private ForceMode m_ForceMode = ForceMode.Force;
        [SerializeField] private PropertyGetDecimal m_RigidbodyDrag = new PropertyGetDecimal(5f);
        
        public override string Title => $"Vortex Pull at {m_Position} (R:{m_Radius})";
        
        protected override Task Run(Args args)
        {
            Vector3 targetPosition = m_Position.Get(args);
            float radius = (float)m_Radius.Get(args);
            float pullForce = (float)m_PullForce.Get(args);
            float spinForce = (float)m_SpinForce.Get(args);
            float duration = (float)m_Duration.Get(args);
            float minDistance = (float)m_MinDistance.Get(args);
            float separationForce = (float)m_SeparationForce.Get(args);
            float separationRadius = (float)m_SeparationRadius.Get(args);
            string tag = m_Tag.Get(args);
            GameObject vfxPrefab = m_VFXPrefab.Get(args);
            Vector3 vfxOffset = m_VFXOffset.Get(args);
            Vector3 moveDirection = m_MoveDirection.Get(args);
            float moveSpeed = (float)m_MoveSpeed.Get(args);
            float sinuousAmplitude = (float)m_SinuousAmplitude.Get(args);
            float sinuousFrequency = (float)m_SinuousFrequency.Get(args);
            float rigidbodyDrag = (float)m_RigidbodyDrag.Get(args);
            
            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(targetPosition, radius, m_LayerMask);
            
            if (colliders.Length == 0 && vfxPrefab == null)
            {
                return DefaultResult;
            }
            
            // Create independent vortex controller
            GameObject vortexObject = new GameObject("VortexController");
            vortexObject.transform.position = targetPosition;
            VortexController controller = vortexObject.AddComponent<VortexController>();
            
            // Register as last vortex
            VortexRegistry.LastVortex = vortexObject;
            
            // Initialize and let it run independently
            controller.Initialize(
                colliders,
                targetPosition,
                pullForce,
                spinForce,
                duration,
                minDistance,
                separationForce,
                separationRadius,
                tag,
                m_UseTag,
                m_AffectRigidbodies,
                m_AffectCharacters,
                m_ForceMode,
                rigidbodyDrag,
                vfxPrefab,
                vfxOffset,
                moveDirection,
                moveSpeed,
                sinuousAmplitude,
                sinuousFrequency
            );
            
            // Return immediately
            return DefaultResult;
        }
        
        // Independent vortex controller
        private class VortexController : MonoBehaviour
        {
            private Collider[] m_Colliders;
            private Vector3 m_TargetPosition;
            private float m_PullForce;
            private float m_SpinForce;
            private float m_Duration;
            private float m_MinDistance;
            private float m_SeparationForce;
            private float m_SeparationRadius;
            private string m_Tag;
            private bool m_UseTag;
            private bool m_AffectRigidbodies;
            private bool m_AffectCharacters;
            private ForceMode m_ForceMode;
            private float m_RigidbodyDrag;
            private GameObject m_VFXInstance;
            private Vector3 m_VFXOffset;
            private Vector3 m_MoveDirection;
            private float m_MoveSpeed;
            private float m_SinuousAmplitude;
            private float m_SinuousFrequency;
            private Vector3 m_PerpendicularAxis;
            
            private float m_Elapsed = 0f;
            private System.Collections.Generic.Dictionary<Rigidbody, float> m_OriginalDrags = new System.Collections.Generic.Dictionary<Rigidbody, float>();
            
            public void Initialize(
                Collider[] colliders,
                Vector3 targetPosition,
                float pullForce,
                float spinForce,
                float duration,
                float minDistance,
                float separationForce,
                float separationRadius,
                string tag,
                bool useTag,
                bool affectRigidbodies,
                bool affectCharacters,
                ForceMode forceMode,
                float rigidbodyDrag,
                GameObject vfxPrefab,
                Vector3 vfxOffset,
                Vector3 moveDirection,
                float moveSpeed,
                float sinuousAmplitude,
                float sinuousFrequency)
            {
                m_Colliders = colliders;
                m_TargetPosition = targetPosition;
                m_PullForce = pullForce;
                m_SpinForce = spinForce;
                m_Duration = duration;
                m_MinDistance = minDistance;
                m_SeparationForce = separationForce;
                m_SeparationRadius = separationRadius;
                m_Tag = tag;
                m_UseTag = useTag;
                m_AffectRigidbodies = affectRigidbodies;
                m_AffectCharacters = affectCharacters;
                m_ForceMode = forceMode;
                m_RigidbodyDrag = rigidbodyDrag;
                m_VFXOffset = vfxOffset;
                m_MoveDirection = moveDirection.normalized;
                m_MoveSpeed = moveSpeed;
                m_SinuousAmplitude = sinuousAmplitude;
                m_SinuousFrequency = sinuousFrequency;
                
                // Calculate perpendicular axis for sinuous movement
                if (moveDirection != Vector3.zero)
                {
                    m_PerpendicularAxis = Vector3.Cross(moveDirection.normalized, Vector3.up).normalized;
                    if (m_PerpendicularAxis == Vector3.zero)
                    {
                        m_PerpendicularAxis = Vector3.Cross(moveDirection.normalized, Vector3.right).normalized;
                    }
                }
                
                // Instantiate VFX if provided
                if (vfxPrefab != null)
                {
                    m_VFXInstance = Instantiate(vfxPrefab, targetPosition + vfxOffset, Quaternion.identity);
                    m_VFXInstance.transform.SetParent(transform);
                }
            }
            
            private void Update()
            {
                m_Elapsed += UnityEngine.Time.deltaTime;
                
                // Check if vortex should end
                if (m_Elapsed >= m_Duration)
                {
                    // Restore original drag values
                    foreach (var kvp in m_OriginalDrags)
                    {
                        if (kvp.Key != null)
                        {
                            kvp.Key.linearDamping = kvp.Value;
                        }
                    }
                    
                    // Destroy VFX
                    if (m_VFXInstance != null)
                    {
                        Destroy(m_VFXInstance);
                    }
                    
                    Destroy(gameObject);
                    return;
                }
                
                // Move vortex center
                if (m_MoveSpeed > 0f && m_MoveDirection != Vector3.zero)
                {
                    // Forward movement
                    Vector3 forwardMovement = m_MoveDirection * m_MoveSpeed * UnityEngine.Time.deltaTime;
                    
                    // Sinuous (side-to-side) movement
                    Vector3 sinuousMovement = Vector3.zero;
                    if (m_SinuousAmplitude > 0f && m_PerpendicularAxis != Vector3.zero)
                    {
                        float sineWave = Mathf.Sin(m_Elapsed * m_SinuousFrequency * Mathf.PI * 2f);
                        sinuousMovement = m_PerpendicularAxis * sineWave * m_SinuousAmplitude * UnityEngine.Time.deltaTime;
                    }
                    
                    m_TargetPosition += forwardMovement + sinuousMovement;
                    transform.position = m_TargetPosition;
                }
                
                // Update VFX position (with offset)
                if (m_VFXInstance != null)
                {
                    m_VFXInstance.transform.position = m_TargetPosition + m_VFXOffset;
                }
                
                // Apply vortex force to all targets
                foreach (Collider col in m_Colliders)
                {
                    if (col == null) continue;
                    
                    // Check tag filter
                    if (m_UseTag && !string.IsNullOrEmpty(m_Tag) && !col.CompareTag(m_Tag))
                    {
                        continue;
                    }
                    
                    Vector3 toCenter = m_TargetPosition - col.transform.position;
                    float distance = toCenter.magnitude;
                    
                    Vector3 pullDirection = toCenter.normalized;
                    
                    // Calculate tangential (spin) direction
                    // Cross product with up vector to get perpendicular direction
                    Vector3 spinDirection = Vector3.Cross(Vector3.up, pullDirection).normalized;
                    
                    // Reduce pull force when close to center (but keep spinning)
                    float pullStrength = distance < m_MinDistance ? 0f : m_PullForce;
                    
                    Vector3 totalForce = pullDirection * pullStrength + spinDirection * m_SpinForce;
                    
                    // Add separation force from other targets
                    foreach (Collider other in m_Colliders)
                    {
                        if (other == null || other == col) continue;
                        if (m_UseTag && !string.IsNullOrEmpty(m_Tag) && !other.CompareTag(m_Tag)) continue;
                        
                        float distToOther = Vector3.Distance(col.transform.position, other.transform.position);
                        if (distToOther < m_SeparationRadius && distToOther > 0.01f)
                        {
                            Vector3 separationDir = (col.transform.position - other.transform.position).normalized;
                            float separationStrength = (1f - (distToOther / m_SeparationRadius)); // Stronger when closer
                            totalForce += separationDir * m_SeparationForce * separationStrength;
                        }
                    }
                    
                    if (totalForce == Vector3.zero) continue;
                    
                    // Try to affect Rigidbody
                    if (m_AffectRigidbodies)
                    {
                        Rigidbody rb = col.GetComponent<Rigidbody>();
                        if (rb != null && !rb.isKinematic)
                        {
                            if (!m_OriginalDrags.ContainsKey(rb))
                            {
                                m_OriginalDrags[rb] = rb.linearDamping;
                            }
                            
                            if (m_RigidbodyDrag > 0f)
                            {
                                rb.linearDamping = m_RigidbodyDrag;
                            }
                            
                            rb.AddForce(totalForce, m_ForceMode);
                            continue;
                        }
                    }
                    
                    Vector3 movementForce = totalForce * UnityEngine.Time.deltaTime;
                    
                    if (m_AffectCharacters)
                    {
                        Character character = col.GetComponent<Character>();
                        if (character != null)
                        {
                            character.transform.position += movementForce * 0.25f;
                            continue;
                        }
                    }
                    
                    col.transform.position += movementForce;
                }
            }
        }
    }
}
