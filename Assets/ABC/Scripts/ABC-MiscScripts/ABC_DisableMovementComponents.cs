using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ABCToolkit {
    /// <summary>
    /// will disable standard movement components on the entity (navagent, character controller, rigidbody etc)
    /// </summary>
    public class ABC_DisableMovementComponents : MonoBehaviour {


        // ********************* Settings ********************
        #region Settings

        /// <summary>
        /// If true then component will be removed from entity when disabled
        /// </summary>
        public bool removeComponentOnDisable = true;

        /// <summary>
        /// If true then the velocity of the rigidbody will remain at 0, halting movement without disabling the component (so it can still work out collisions etc)
        /// </summary>
        public bool haltRigidbody = true;

        /// <summary>
        /// If true then any nav agent components on the entity will be disabled
        /// </summary>
        public bool haltNavAgent = true;

        /// <summary>
        /// If true then any character controller components on the entity will be disabled
        /// </summary>
        public bool disableCharacterController = true;


        /// <summary>
        /// If true then ABC AI Navigation will be disabled 
        /// </summary>
        public bool blockABCAINavigation = true;




        #endregion



        //************************ Variables / Private Properties ****************************************

        #region Variables / Private Properties

        /// <summary>
        /// Transform of the entity 
        /// </summary>
        private Transform meTransform;

        /// <summary>
        /// Collider of the entity 
        /// </summary>
        private Collider meCollider;

        /// <summary>
        /// Rigidbody attached to the entity
        /// </summary>
        private Rigidbody meRigidbody;

        /// <summary>
        /// Nav Agent attached to entity
        /// </summary>
        private NavMeshAgent navAgent;

        /// <summary>
        /// CharacterController attached to entity
        /// </summary>
        private CharacterController charController;

        /// <summary>
        /// ABC Entity 
        /// </summary>
        private ABC_IEntity abcEntity;

        /// <summary>
        /// ABC gravity component
        /// </summary>
        private ABC_Gravity abcGravity;

        /// <summary>
        /// ABC Movement Controller
        /// </summary>
        private ABC_MovementController abcMovementController;

#if ABC_GC_2_Integration
    /// <summary>
    /// Stores the GC2 current angular speed
    /// </summary>
    private float gc2CurrentAngularSpeed = 3000;
#endif


        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Determines if the entity is in the air
        /// </summary>
        /// <returns>True if entity is in the air, else false</returns>
        private bool EntityInAir() {

            //If nav agent is off ground or falling then return true
            if (this.abcEntity.isInTheAir == true || this.navAgent != null && this.navAgent.isOnNavMesh == false)
                return true;
            else
                return false; //else return false



        }

        /// <summary>
        /// Will halt the rigidbody keeping velocity at 0
        /// </summary>
        private void HaltRigidbody() {

            if (this.haltRigidbody == false)
                return;

            //If a rigidbody exists then stop velocity except gravity 
            if (this.meRigidbody != null && this.meRigidbody.isKinematic == false) {
                this.meRigidbody.linearVelocity = new Vector3(0f, this.meRigidbody.linearVelocity.y, 0f);
                this.meRigidbody.angularVelocity = new Vector3(0f, this.meRigidbody.linearVelocity.y, 0f);
            }

        }



        /// <summary>
        /// Will halt the NavAgent 
        /// </summary>
        private void HaltNavAgent() {

            if (this.navAgent == null)
                return;

            //If nav agent is off ground or falling then turn nav agent off 
            if (ABC_Utilities.EntityDistanceFromGround(this.transform) > 0.3) {
                navAgent.enabled = false;
            } else {
                navAgent.enabled = true;
            }

            if (this.navAgent.isOnNavMesh == true) {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }

            //Now apply some gravity if rigidbody doesn't exist (as nav agents doesn't have gravity)
            if (this.meRigidbody == null && this.meTransform.gameObject.GetComponent<ABC_Gravity>() == null)
                this.abcGravity = this.meTransform.gameObject.AddComponent<ABC_Gravity>();

        }

        /// <summary>
        /// Will enable or disable the AI Navigation block for the entity, if blocked then the navigation will not run
        /// </summary>
        /// <param name="Enabled">True if to block AI Navigation, else false</param>
        private void BlockABCAINav(bool Enabled) {

            if (this.abcEntity == null || this.blockABCAINavigation == false)
                return;


            this.abcEntity.BlockAINavigation(Enabled);


        }


        /// <summary>
        /// Will enable/disable the Character Controller 
        /// </summary>
        /// <param name="Enabled">True to enable Character Controller, else false to disable</param>
        private void ToggleCharacterController(bool Enabled) {

            //If not disabling character controller then end here
            if (this.disableCharacterController == false)
                return;

#if ABC_GC_2_Integration

        if (this.abcEntity.HasGC2CharacterComponent()) {

            //Set rotation
            ABC_Utilities.mbSurrogate.StartCoroutine(this.abcEntity.AllowGC2Rotation(Time.time, Enabled, 0f));

            //Set GC2 Character is controllerable
            ABC_Utilities.mbSurrogate.StartCoroutine(this.abcEntity.SetGC2CharacterIsControllerable(Time.time, Enabled));

            return; 

        }
#endif

            //If char controller doesn't exist or ABC movement controller is in use then end here (ABC movement controller stops on it's own using events)
            if (this.charController == null || this.abcMovementController != null)
                return;


            this.charController.enabled = Enabled;


        }



        #endregion


        //************************ Game ****************************************

        #region Variables / Private Properties


        // Use this for initialization
        void OnEnable() {

            //Record all compoenents 
            this.meTransform = transform;
            this.meCollider = meTransform.GetComponentInChildren<Collider>(true);
            this.meRigidbody = meTransform.GetComponent<Rigidbody>();
            this.navAgent = meTransform.GetComponent<NavMeshAgent>();
            this.charController = meTransform.GetComponent<CharacterController>();
            this.abcEntity = ABC_Utilities.GetStaticABCEntity(this.meTransform.gameObject);
            this.abcMovementController = meTransform.GetComponentInChildren<ABC_MovementController>();



            //Disable all components
            this.ToggleCharacterController(false);

            //Block ABC AI Nav
            this.BlockABCAINav(true);
        }

        // Update is called once per frame
        void Update() {

            if (this.enabled == false)
                return;

            //Freeze Rigidbody
            this.HaltRigidbody();




        }

        private void FixedUpdate() {
            //Freeze NavAgent
            this.HaltNavAgent();
        }

        private void OnDisable() {


            //Enable all components again        
            this.ToggleCharacterController(true);

            //Unblock ABC AI Nav
            this.BlockABCAINav(false);

            //enable navagent again unless entity is in air
            if (this.navAgent != null && this.EntityInAir() == false) {
                this.navAgent.enabled = true;
            }

            //If setup too then remove component
            if (this.removeComponentOnDisable)
                Destroy(this);
        }

        #endregion
    }
}