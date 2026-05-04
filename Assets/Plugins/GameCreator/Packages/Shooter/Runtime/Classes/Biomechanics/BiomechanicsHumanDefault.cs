using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace GameCreator.Runtime.Shooter
{
    [Title("Human Default")]
    [Category("Human Default")]
    
    [Description("Rotates a human bone chain in order to reach the desired direction and aims at the specific point")]
    [Image(typeof(IconPistol), ColorTheme.Type.Green)]
    
    [MovedFrom(
        true,
        null,
        null,
        "BiomechanicsHumanFK"
    )]
    
    [Serializable]
    public class BiomechanicsHumanDefault : TBiomechanicsHuman
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private HumanRotationPitch m_BonesPitch = new HumanRotationPitch();
        [SerializeField] private HumanRotationYaw m_BonesYaw = new HumanRotationYaw();
        [SerializeField] private HumanRotationLean m_BonesLean = new HumanRotationLean();
        
        [SerializeField] private PropertyGetDecimal m_MaxPitch = GetDecimalDecimal.Create(160f);
        [SerializeField] private PropertyGetDecimal m_MaxYaw = GetDecimalDecimal.Create(120f);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override bool UseKinematics => true;
        
        public override THumanRotations HumanBonesPitch => this.m_BonesPitch;
        public override THumanRotations HumanBonesYaw => this.m_BonesYaw;
        public override THumanRotations HumanBonesLean => this.m_BonesLean;

        // GETTER METHODS: ------------------------------------------------------------------------
        
        public override float GetMaxPitch(Args args)
        {
            return (float) this.m_MaxPitch.Get(args);
        }

        public override float GetMaxYaw(Args args)
        {
            return (float) this.m_MaxYaw.Get(args);
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