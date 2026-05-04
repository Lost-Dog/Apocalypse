using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace ABCToolkit {
    public class ABC_IKController : MonoBehaviour {

        // ********************* Unity Animator IK ********************
        #region Unity Animator IK

        // ********************* Settings ********************
        #region Settings

        [Header("Unity Animator IK")]

        /// <summary>
        /// Will enable the IK Controller
        /// </summary>
        public bool enableIK = true;

        /// <summary>
        /// Target for the right hand to attach to
        /// </summary>
        public Transform rightHandTarget = null;


        /// <summary>
        /// The weight for right hand
        /// </summary>
        public float rightHandWeight = 1f;

        /// <summary>
        /// Target for the left hand to attach to
        /// </summary>
        public Transform leftHandTarget = null;

        /// <summary>
        /// The weight for left hand
        /// </summary>
        public float leftHandWeight = 1f;

        #endregion

        // ********************* Variables ********************

        #region IK Variables

        /// <summary>
        /// ABC Entity 
        /// </summary>
        private ABC_IEntity abcEntity = null;

        /// <summary>
        /// Animator 
        /// </summary>
        private Animator animator;

        /// <summary>
        /// Will track the state of weight
        /// </summary>
        private float rightHandWeightState = 0;

        /// <summary>
        /// Will track the elapsed time when setting the weight
        /// </summary>
        private float rightHandElapsedTime = 0;

        /// <summary>
        /// The speed at which the weight will transition
        /// </summary>
        private float rightHandTransitionSpeed = 0.5f;

        /// <summary>
        /// Will track the state of weight
        /// </summary>
        private float leftHandWeightState = 0;

        /// <summary>
        /// Will track the elapsed time when setting the weight
        /// </summary>
        private float leftHandElapsedTime = 0;

        /// <summary>
        /// The speed at which the weight will transition
        /// </summary>
        private float leftHandTransitionSpeed = 0.5f;


        #endregion

        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will enable or disable the IK
        /// </summary>
        /// <param name="Enabled">True if to enable the IK, else false</param>
        public void ToggleIK(bool Enabled) {

            this.enableIK = Enabled;

            //If enabled then reset the weight state
            if (Enabled == true) {
                this.rightHandWeightState = 0;
                this.leftHandWeightState = 0;
                this.rightHandElapsedTime = 0;
                this.leftHandElapsedTime = 0;
            }

        }

        /// <summary>
        /// Will set the IK Bone to the target transform provided
        /// </summary>
        /// <param name="IKGoal">IKGoal to attach to target</param>
        /// <param name="Target">Target transform to attack IK too</param>
        /// <param name="Weight">The weight of the IK</param>
        /// <param name="TransitionSpeed">Transition Speed for when applying the IK</param>
        public void SetIKTarget(AvatarIKGoal IKGoal, Transform Target, float Weight = 1, float TransitionSpeed = 0.5f) {

            switch (IKGoal) {
                case AvatarIKGoal.LeftHand:
                    this.leftHandTarget = Target;
                    this.leftHandWeight = Weight;
                    this.leftHandTransitionSpeed = TransitionSpeed;
                    break;
                case AvatarIKGoal.RightHand:
                    this.rightHandTarget = Target;
                    this.rightHandWeight = Weight;
                    this.rightHandTransitionSpeed = TransitionSpeed;
                    break;
            }

            if (this.enableIK == false)
                this.enableIK = true;

            //Reset weight as new targets have been assigned
            this.rightHandWeightState = 0;
            this.leftHandWeightState = 0;
            this.rightHandElapsedTime = 0;
            this.leftHandElapsedTime = 0;
        }

        /// <summary>
        /// Will remove the IK goal from it's current target 
        /// </summary>
        /// <param name="IKGoal">IKGoal to remove target for</param>
        public void RemoveIKTarget(AvatarIKGoal IKGoal) {
            switch (IKGoal) {
                case AvatarIKGoal.LeftHand:
                    this.leftHandTarget = null;
                    break;
                case AvatarIKGoal.RightHand:
                    this.rightHandTarget = null;
                    break;
            }
        }

        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will set the IK Bone to the target transform provided
        /// </summary>
        /// <param name="IKGoal">IKGoal to attach to target</param>
        /// <param name="Target">Target transform to attack IK too</param>
        /// <param name="Weight">Sets the Weight of the IK</param>
        private void SetIKToTarget(AvatarIKGoal IKGoal, Transform Target, float Weight = 1) {

            //Set weight
            this.animator.SetIKPositionWeight(IKGoal, Weight);
            this.animator.SetIKRotationWeight(IKGoal, Weight);

            //Set position and rotation
            this.animator.SetIKPosition(IKGoal, Target.position);
            this.animator.SetIKRotation(IKGoal, Target.rotation);
        }

        /// <summary>
        /// Will remove the IK goal from it's current target 
        /// </summary>
        /// <param name="IKGoal">IKGoal to attach to target</param>
        private void RemoveIK(AvatarIKGoal IKGoal) {

            //If IK is already 0 then return
            if (this.animator.GetIKPositionWeight(IKGoal) == 0)
                return;

            this.animator.SetIKPositionWeight(IKGoal, 0);
            this.animator.SetIKRotationWeight(IKGoal, 0);
        }

        #endregion

        #endregion

        // ********************* Animation Runner Stream IK ********************
        #region Animation Runner Stream IK

        // ********************* Variables ********************

        #region Variables

        /// <summary>
        /// the animation runner IK job
        /// </summary>
        ABC_AnimationsRunnerIKJob animationRunnerIKJob;

        #endregion

        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will return the Animations Runner IK Job
        /// </summary>
        /// <returns>The current animations runner IK Job</returns>
        public ABC_AnimationsRunnerIKJob GetAnimationRunnerIKJob() {

            return this.animationRunnerIKJob;
        }

        #endregion

        #endregion

        // ********************** Game ******************


        #region Game

        void OnEnable() {

            this.abcEntity = ABC_Utilities.GetStaticABCEntity(this.gameObject);
            this.animator = abcEntity.animator;

            // **** Unity Animator IK

            //Reset values
            this.rightHandWeightState = 0;
            this.leftHandWeightState = 0;
            this.rightHandElapsedTime = 0;
            this.leftHandElapsedTime = 0;

        }

        //IK callback 
        void OnAnimatorIK() {

            //if ABC entity is null or the animator is null
            if (this.abcEntity == null || this.animator == null)
                return;            

            //If component is not enabled then remove the IK resetting it
            if (enableIK == false) {
                this.RemoveIK(AvatarIKGoal.RightHand);
                this.RemoveIK(AvatarIKGoal.LeftHand);
                this.animator.SetLookAtWeight(0);
                return;
            }

            //Smooth the weight to transition the IK to the right position
            if (this.rightHandWeightState < this.rightHandWeight) {
                this.rightHandElapsedTime += Time.deltaTime;
                this.rightHandWeightState = Mathf.Lerp(0, 1, this.rightHandElapsedTime / this.rightHandTransitionSpeed);
            } else {
                this.rightHandWeightState = this.rightHandWeight;
                this.rightHandElapsedTime = 0;
            }

            // set right hand to target if it exists
            if (this.rightHandTarget != null) 
                this.SetIKToTarget(AvatarIKGoal.RightHand, this.rightHandTarget, this.rightHandWeightState);
            else
                this.RemoveIK(AvatarIKGoal.RightHand); //If target not provided then remove the IK


            //Smooth the weight to transition the IK to the right position
            if (this.leftHandWeightState < this.leftHandWeight) {
                this.leftHandElapsedTime += Time.deltaTime;
                this.leftHandWeightState = Mathf.Lerp(0, 1, this.leftHandElapsedTime / this.leftHandTransitionSpeed);
            } else {
                this.leftHandWeightState = this.leftHandWeight;
                this.leftHandElapsedTime = 0;
            }

            // set left hand to target if it exists
            if (this.leftHandTarget != null)
                this.SetIKToTarget(AvatarIKGoal.LeftHand, this.leftHandTarget, this.leftHandWeightState);
            else
                this.RemoveIK(AvatarIKGoal.LeftHand);  //If target not provided then remove the IK

        }

        #endregion

        // ****************** IAnimationJob Struct ************

        public struct ABC_AnimationsRunnerIKJob : IAnimationJob {
            

            private void Solve(AnimationStream stream) {
                AnimationHumanStream humanStream = stream.AsHuman();

                humanStream.SolveIK();
            }

            public void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream) {
    
                Solve(stream);
            }
        }



    }
}