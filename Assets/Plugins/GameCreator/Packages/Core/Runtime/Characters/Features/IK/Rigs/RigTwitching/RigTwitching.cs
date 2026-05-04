using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters.IK
{
    [Title("Twitching")]
    [Category("Twitching")]
    [Image(typeof(IconTwitching), ColorTheme.Type.Green)]
    
    [Description("Subtly rotates the arms, fingers and hand bones to make them appear alive")]
    
    [Serializable]
    public class RigTwitching : TRigAnimatorIK
    {
        private const int RANDOM_MIN = 0;
        private const int RANDOM_MAX = 999;
        
        // CONSTANTS: -----------------------------------------------------------------------------

        public const string RIG_NAME = "RigTwitching";
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetDecimal m_Speed = new PropertyGetDecimal(0.2f);
        [SerializeField] private PropertyGetDecimal m_Intensity = new PropertyGetDecimal(5f);
        
        [SerializeField] private PropertyGetDecimal m_ArmsTwitch = new PropertyGetDecimal(1f);
        [SerializeField] private PropertyGetDecimal m_HandsTwitch = new PropertyGetDecimal(1f);
        [SerializeField] private PropertyGetDecimal m_FingersTwitch = new PropertyGetDecimal(1f);
        
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Dictionary<HumanBodyBones, Vector3> m_Noises = new Dictionary<HumanBodyBones, Vector3>();

        [NonSerialized] private readonly HumanBodyBones[] m_Arms =
        {
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightUpperArm
        };
        
        [NonSerialized] private readonly HumanBodyBones[] m_Hands =
        {
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand
        };

        [NonSerialized] private readonly HumanBodyBones[] m_Fingers =
        {
            HumanBodyBones.LeftIndexProximal,
            HumanBodyBones.LeftMiddleProximal,
            HumanBodyBones.LeftRingProximal,
            HumanBodyBones.LeftLittleProximal,
            HumanBodyBones.LeftThumbProximal,
            HumanBodyBones.RightIndexProximal,
            HumanBodyBones.RightMiddleProximal,
            HumanBodyBones.RightRingProximal,
            HumanBodyBones.RightLittleProximal,
            HumanBodyBones.RightThumbProximal,
        };
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Twitch";
        
        public override string Name => RIG_NAME;
        
        public override bool RequiresHuman => true;
        public override bool DisableOnBusy => false;

        // IMPLEMENT METHODS: ---------------------------------------------------------------------

        protected override void DoStartup(Character character)
        {
            base.DoStartup(character);

            foreach (HumanBodyBones bone in this.m_Arms)
            {
                Vector3 noise = new Vector3(
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX)
                );
                
                this.m_Noises.TryAdd(bone, noise);
            }
            
            foreach (HumanBodyBones bone in this.m_Hands)
            {
                Vector3 noise = new Vector3(
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX)
                );
                
                this.m_Noises.TryAdd(bone, noise);
            }
            
            foreach (HumanBodyBones bone in this.m_Fingers)
            {
                Vector3 noise = new Vector3(
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX),
                    UnityEngine.Random.Range(RANDOM_MIN, RANDOM_MAX)
                );
                
                this.m_Noises.TryAdd(bone, noise);
            }
        }

        protected override void DoEnable(Character character)
        {
            base.DoEnable(character);
            
            character.EventBeforeLateUpdate -= this.OnLateUpdate;
            character.EventBeforeLateUpdate += this.OnLateUpdate;
        }

        protected override void DoDisable(Character character)
        {
            base.DoDisable(character);
            character.EventBeforeLateUpdate -= this.OnLateUpdate;
        }

        private void OnLateUpdate()
        {
            float time = this.Character.Time.Time * (float) this.m_Speed.Get(this.Args);
            float intensity = (float) this.m_Intensity.Get(this.Args);
            
            float armsTwitch = (float) this.m_ArmsTwitch.Get(this.Args);
            float handsTwitch = (float) this.m_HandsTwitch.Get(this.Args);
            float fingersTwitch = (float) this.m_FingersTwitch.Get(this.Args);

            this.ApplyTwitch(this.m_Arms, time, armsTwitch * intensity);
            this.ApplyTwitch(this.m_Hands, time, handsTwitch * intensity);
            this.ApplyTwitch(this.m_Fingers, time, fingersTwitch * intensity);
        }
        
        private void ApplyTwitch(in HumanBodyBones[] bones, float time, float twitch)
        {
            if (twitch <= 0f) return;
            
            foreach (HumanBodyBones bone in bones)
            {
                float noiseX = Mathf.PerlinNoise(time + this.m_Noises[bone].x, time + this.m_Noises[bone].y) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(time + this.m_Noises[bone].y, time + this.m_Noises[bone].z) * 2f - 1f;
                float noiseZ = Mathf.PerlinNoise(time + this.m_Noises[bone].z, time + this.m_Noises[bone].x) * 2f - 1f;

                Quaternion noiseRotation = Quaternion.Euler(
                    noiseX * twitch,
                    noiseY * twitch,
                    noiseZ * twitch
                );
                
                Transform transform = this.Character.Animim.Animator.GetBoneTransform(bone);
                if (transform != null) transform.localRotation *= noiseRotation;
            }
        } 
    }
}