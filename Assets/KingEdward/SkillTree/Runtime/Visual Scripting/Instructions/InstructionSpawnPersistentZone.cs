using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using KingEdward;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Spawn Persistent Zone")]
    [Description("Creates a persistent zone that executes instructions on targets at intervals (damage zone, heal zone, buff zone)")]

    [Category("KingEdward/Skill Tree/Combat/Spawn Persistent Zone")]
    
    [Parameter("Position", "The center position of the zone")]
    [Parameter("Radius", "The radius of the zone")]
    [Parameter("Duration", "How long the zone lasts")]
    [Parameter("Tick Rate", "How often the zone effect triggers (in seconds)")]
    [Parameter("On Tick", "Instructions to execute on each tick for targets in zone")]
    [Parameter("VFX Prefab", "Optional visual effect prefab for the zone")]
    [Parameter("Parent", "Optional parent transform to attach the zone to (for moving zones)")]
    
    [Image(typeof(IconSpawnZone), ColorTheme.Type.Green)]
    
    [Serializable]
    public class InstructionSpawnPersistentZone : Instruction
    {
        [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetDecimal m_Radius = new PropertyGetDecimal(5f);
        [SerializeField] private PropertyGetDecimal m_Duration = new PropertyGetDecimal(5f);
        [SerializeField] private PropertyGetDecimal m_TickRate = new PropertyGetDecimal(1f);
        [SerializeField] private InstructionList m_OnTick = new InstructionList();
        [SerializeField] private PropertyGetInstantiate m_VFXPrefab = new PropertyGetInstantiate();
        [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetString m_TargetTag = new PropertyGetString("Enemy");
        [SerializeField] private LayerMask m_LayerMask = -1;
        [SerializeField] private bool m_UseTag = true;
        [SerializeField] private bool m_AffectOnEnter = false;
        
        public override string Title => $"Spawn Zone at {m_Position} (R:{m_Radius}, D:{m_Duration})";
        
        protected override Task Run(Args args)
        {
            Vector3 position = m_Position.Get(args);
            float radius = (float)m_Radius.Get(args);
            float duration = (float)m_Duration.Get(args);
            float tickRate = (float)m_TickRate.Get(args);
            string targetTag = m_TargetTag.Get(args);
            GameObject parent = m_Parent.Get(args);
            
            // Spawn VFX if provided
            GameObject vfxInstance = null;
            if (m_VFXPrefab != null)
            {
                vfxInstance = m_VFXPrefab.Get(args, position, Quaternion.identity);
            }
            
            // Create zone controller as independent object
            GameObject zoneObject = new GameObject("PersistentZone");
            zoneObject.transform.position = position;
            
            // Set parent if provided
            if (parent != null)
            {
                zoneObject.transform.SetParent(parent.transform);
                if (vfxInstance != null)
                {
                    vfxInstance.transform.SetParent(parent.transform);
                }
            }
            
            PersistentZoneController controller = zoneObject.AddComponent<PersistentZoneController>();
            
            // Initialize and let it run independently
            controller.Initialize(
                radius,
                duration,
                tickRate,
                m_OnTick,
                targetTag,
                m_LayerMask,
                m_UseTag,
                m_AffectOnEnter,
                vfxInstance,
                args.Self
            );
            
            // Return immediately - zone runs independently
            return DefaultResult;
        }
        
        // Helper component to manage zone independently
        private class PersistentZoneController : MonoBehaviour
        {
            private float m_Radius;
            private float m_Duration;
            private float m_TickRate;
            private InstructionList m_OnTick;
            private string m_TargetTag;
            private LayerMask m_LayerMask;
            private bool m_UseTag;
            private bool m_AffectOnEnter;
            private GameObject m_VFXInstance;
            private GameObject m_OriginalSelf;
            
            private float m_Elapsed = 0f;
            private float m_NextTick = 0f;
            
            public void Initialize(
                float radius,
                float duration,
                float tickRate,
                InstructionList onTick,
                string targetTag,
                LayerMask layerMask,
                bool useTag,
                bool affectOnEnter,
                GameObject vfxInstance,
                GameObject originalSelf)
            {
                m_Radius = radius;
                m_Duration = duration;
                m_TickRate = tickRate;
                m_OnTick = onTick;
                m_TargetTag = targetTag;
                m_LayerMask = layerMask;
                m_UseTag = useTag;
                m_AffectOnEnter = affectOnEnter;
                m_VFXInstance = vfxInstance;
                m_OriginalSelf = originalSelf;
            }
            
            private void Update()
            {
                m_Elapsed += UnityEngine.Time.deltaTime;
                
                // Check if zone should end
                if (m_Elapsed >= m_Duration)
                {
                    // Cleanup
                    if (m_VFXInstance != null)
                    {
                        Destroy(m_VFXInstance);
                    }
                    Destroy(gameObject);
                    return;
                }
                
                // Check for tick execution
                if (m_Elapsed >= m_NextTick)
                {
                    m_NextTick = m_Elapsed + (1f / m_TickRate);
                    
                    // Find targets in radius using current transform position (follows parent if attached)
                    Collider[] colliders = Physics.OverlapSphere(transform.position, m_Radius, m_LayerMask);
                    
                    foreach (Collider col in colliders)
                    {
                        if (col == null) continue;
                        
                        // Check tag filter
                        if (m_UseTag && !string.IsNullOrEmpty(m_TargetTag) && !col.CompareTag(m_TargetTag))
                        {
                            continue;
                        }
                        
                        // Execute tick instructions on target
                        Args tickArgs = new Args(m_OriginalSelf, col.gameObject);
                        _ = m_OnTick.Run(tickArgs);
                    }
                }
            }
        }
    }
}
