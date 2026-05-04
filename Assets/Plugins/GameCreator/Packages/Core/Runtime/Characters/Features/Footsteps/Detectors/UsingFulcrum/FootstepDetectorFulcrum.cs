using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Fulcrum Plane")]
    [Category("Fulcrum Plane")]
    
    [Description("Uses the bone data to detect when it goes below a plane at the ground level")]
    [Image(typeof(IconFootprint), ColorTheme.Type.Green)]
    
    [Serializable]
    public class FootstepDetectorFulcrum : FootstepDetectorBase
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private float m_Fulcrum = -0.85f;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        private Dictionary<Transform, Footprint> m_Footprints = new Dictionary<Transform, Footprint>();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public float Fulcrum
        {
            get => this.m_Fulcrum;
            set => this.m_Fulcrum = value;
        }

        // INITIALIZERS: --------------------------------------------------------------------------
        
        public override void OnEnable(Character character)
        { }

        public override void OnDisable(Character character)
        { }
        
        // UPDATE METHODS: ------------------------------------------------------------------------

        public override void OnUpdate(Character character)
        {
            Animator animator = character.Animim.Animator;
            if (animator == null) return;
            
            bool isGrounded = character.Driver.IsGrounded;
            float fulcrum = character.transform.TransformPoint(Vector3.up * this.m_Fulcrum).y;
            
            for (int i = 0; i < character.Footsteps.Length && i < Phases.Count; i++)
            {
                Footstep foot = character.Footsteps.Feet[i];
                Transform bone = foot.Bone.GetTransform(animator);
                if (bone == null) continue;
            
                bool phaseGround = bone.position.y <= fulcrum;
                
                if (isGrounded && this.m_Footprints.TryGetValue(bone, out Footprint footprint))
                {
                    if (phaseGround && !footprint.WasGrounded)
                    {
                        character.Footsteps.OnStep(bone);
                    }
            
                    footprint.WasGrounded = phaseGround;
                }
                else
                {
                    this.m_Footprints[bone] = new Footprint
                    {
                        WasGrounded = true,
                    };
                }
            }
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        public override void OnGizmos(Character character)
        {
            Gizmos.color = Application.isPlaying
                ? new Color(0f, 0f, 1f, 0.1f)
                : new Color(0f, 0f, 1f, 0.5f);
            
            Vector3 position = character.transform.TransformPoint(Vector3.up * this.m_Fulcrum);
            GizmosExtension.Circle(position, 0.5f, character.transform.up, true);
        }
    }
}