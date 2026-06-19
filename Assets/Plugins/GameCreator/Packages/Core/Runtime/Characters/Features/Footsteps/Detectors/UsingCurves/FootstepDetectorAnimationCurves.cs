using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Animation Curves (obsolete)")]
    [Category("Animation Curves (obsolete)")]
    
    [Description("Uses the Phases properties to detect footsteps based on the Animation Clip curves data")]
    [Image(typeof(IconFootprint), ColorTheme.Type.Red)]
    
    [Serializable]
    public class FootstepDetectorAnimationCurves : FootstepDetectorBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        private Dictionary<Transform, Footprint> m_Footprints = new Dictionary<Transform, Footprint>();
        
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
            
            for (int i = 0; i < character.Footsteps.Length && i < Phases.Count; i++)
            {
                Footstep foot = character.Footsteps.Feet[i];
                Transform bone = foot.Bone.GetTransform(animator);
                if (bone == null) continue;
            
                bool phaseGround = character.Phases.IsGround(i);
            
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
        { }
    }
}