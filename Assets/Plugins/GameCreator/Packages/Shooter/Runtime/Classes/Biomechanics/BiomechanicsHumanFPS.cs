using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace GameCreator.Runtime.Shooter
{
    [Title("Human FPS")]
    [Category("Human FPS")]
    
    [Description("Aligns the Weapon with an optics point through the scope. Useful for FPS games with iron-sights or similar")]
    [Image(typeof(IconFace), ColorTheme.Type.Green)]
    
    [MovedFrom(
        true,
        null,
        null,
        "BiomechanicsHumanIK"
    )]
    
    [Serializable]
    public class BiomechanicsHumanFPS : TBiomechanicsHuman
    {
        [SerializeField] private HumanRotationLean m_BonesLean = new HumanRotationLean();
        
        [SerializeField] private PropertyGetDecimal m_MaxPitch = GetDecimalDecimal.Create(160f);
        [SerializeField] private PropertyGetDecimal m_MaxYaw = GetDecimalDecimal.Create(120f);
        
        [SerializeField] private PropertyGetDecimal m_SwayWeight = GetDecimalDecimal.Create(0.75f);
        [SerializeField] private PropertyGetDecimal m_Sway = GetDecimalDecimal.Create(0.025f);
        
        [SerializeField] private EnablerLayerMask m_PullOnObstruction = new EnablerLayerMask(true);
        [SerializeField] private PropertyGetDecimal m_MinDistance = GetDecimalConstantPointOne.Create;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override bool UseKinematics => true;
        
        public override THumanRotations HumanBonesPitch => null;
        public override THumanRotations HumanBonesYaw => null;
        public override THumanRotations HumanBonesLean => this.m_BonesLean;

        public EnablerLayerMask PullOnObstruction => this.m_PullOnObstruction;
        
        // GETTER METHODS: ------------------------------------------------------------------------
        
        public override float GetMaxPitch(Args args)
        {
            return (float) this.m_MaxPitch.Get(args);
        }

        public override float GetMaxYaw(Args args)
        {
            return (float) this.m_MaxYaw.Get(args);
        }

        public float GetMinDistance(Args args)
        {
            return (float) this.m_MinDistance.Get(args);
        }

        public float GetSwayWeight(Args args)
        {
            return (float) this.m_SwayWeight.Get(args);
        }
        
        public float GetSway(Args args)
        {
            return (float) this.m_Sway.Get(args);
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override void Enter(Character character, ShooterWeapon weapon, float enterDuration)
        {
            character.IK.RequireRig<RigShooterHuman>()?.OnEnter(weapon, this, enterDuration);
        }

        public override void Exit(Character character, ShooterWeapon weapon, float exitDuration)
        {
            character.IK.GetRig<RigShooterHuman>()?.OnExit(weapon, this, exitDuration);
        }
    }
}