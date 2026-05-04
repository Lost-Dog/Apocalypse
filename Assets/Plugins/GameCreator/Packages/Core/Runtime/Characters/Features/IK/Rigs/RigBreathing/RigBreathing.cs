using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters.IK
{
    [Title("Breathing")]
    [Category("Breathing")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    
    [Description("Rotates the bones around the chest in a breathing motion fashion")]
    
    [Serializable]
    public class RigBreathing : TRigAnimatorIK
    {
        private const float REST_ANGLE_CHEST = 2f;
        private const float REST_ANGLE_UPPER_CHEST = 4f;
        
        // CONSTANTS: -----------------------------------------------------------------------------

        public const string RIG_NAME = "RigBreathing";
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetDecimal m_Exertion = new PropertyGetDecimal(1f);
        [SerializeField] private PropertyGetDecimal m_Rate = new PropertyGetDecimal(0.3f);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Breathe";
        
        public override string Name => RIG_NAME;
        
        public override bool RequiresHuman => true;
        public override bool DisableOnBusy => false;

        // IMPLEMENT METHODS: ---------------------------------------------------------------------

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
            Transform boneChest = this.Character.Animim.Animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform boneUpperChest = this.Character.Animim.Animator.GetBoneTransform(HumanBodyBones.UpperChest);
            
            Transform boneNeck = this.Character.Animim.Animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform leftShoulder = this.Character.Animim.Animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Transform rightShoulder = this.Character.Animim.Animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            
            float exertion = Mathf.Max(0f, (float) this.m_Exertion.Get(this.Args));
            float rate = Mathf.Max(0f, (float) this.m_Rate.Get(this.Args));
            float cycle = Mathf.Sin(this.Character.Time.Time * Mathf.PI * 2f * rate);
            
            Vector3 axisPitch = leftShoulder.position - rightShoulder.position;
            
            float chestAngle = cycle * exertion * REST_ANGLE_CHEST;
            float upperChestAngle = cycle * exertion * REST_ANGLE_UPPER_CHEST;
            
            boneChest.Rotate(axisPitch, chestAngle, Space.World);
            boneUpperChest.Rotate(axisPitch, upperChestAngle, Space.World);
            
            float counterAngle = cycle * exertion * -(REST_ANGLE_CHEST + REST_ANGLE_UPPER_CHEST);
            
            boneNeck.Rotate(axisPitch, counterAngle, Space.World);
            leftShoulder.Rotate(axisPitch, counterAngle, Space.World);
            rightShoulder.Rotate(axisPitch, counterAngle, Space.World);
        }
    }
}