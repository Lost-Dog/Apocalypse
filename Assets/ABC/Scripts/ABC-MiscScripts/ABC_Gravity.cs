using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

namespace ABCToolkit {
    /// <summary>
    /// Component script which will apply gravity to the entity
    /// </summary>
    public class ABC_Gravity : MonoBehaviour {

        // ************************ Settings ***********************

        /// <summary>
        /// Determines if gravity will be applied or not 
        /// </summary>
        public bool gravityEnabled = true;

        /// <summary>
        /// How long the entity should be grounded for before this script is removed
        /// </summary>
        public float groundedRemoveTime = 1.5f;

        // *************************** Variables ******************

        /// <summary>
        /// Transform of the entity this component is attached too
        /// </summary>
        private Transform meTransform;

        /// <summary>
        /// Nav Agent attached to entity
        /// </summary>
        private NavMeshAgent navAgent;

        /// <summary>
        /// ABC entity 
        /// </summary>
        private ABC_IEntity meEntity;

        /// <summary>
        /// collider of the entity this component is attached too
        /// </summary>
        private Collider meCollider;

        /// <summary>
        /// Tracks the gravity velocity
        /// </summary>
        private Vector3 gravityVelocity;

        /// <summary>
        /// Will track the last time gravity was applied; 
        /// </summary>
        private float lastTimeGravityApplied = 0f;

        /// <summary>
        /// Records the ABC SM Component if used for events
        /// </summary>
        private ABC_StateManager ABCEventsSM;


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will intergate with ABC by retriving the component from the current follow target and then subscribing to it's movement and gravity events
        /// </summary>
        private void IntegrateWithABC() {

            this.ABCEventsSM = meTransform.GetComponentInChildren<ABC_StateManager>();

            //subscribe to the events
            if (this.ABCEventsSM != null) {
                this.ABCEventsSM.onEnableGravity += this.EnableGravity;
                this.ABCEventsSM.onDisableGravity += this.DisableGravity;
            }

        }



        /// <summary>
        /// Will enable Gravity
        /// </summary>
        public void EnableGravity() {

            this.gravityEnabled = true;
        }


        /// <summary>
        /// Will disable gravity
        /// </summary>
        public void DisableGravity() {

            this.gravityEnabled = false;
        }

        /// <summary>
        /// Will apply gravity to the entity attached until they have been grounded for a short while
        /// </summary>
        private void ApplyGravity() {


            //If we have not collided with anything then apply gravity 
            if (this.meEntity.isInTheAir) {

                //move character down if gravity is enabled 
                if (this.gravityEnabled == true) {
                    this.gravityVelocity += Physics.gravity * Time.deltaTime;
                    this.meTransform.position += this.gravityVelocity * Time.deltaTime;
                }

                //track when we last applied gravity 
                this.lastTimeGravityApplied = Time.time;

            }


            //If gravity has not been applied for last x seconds then remove script as we are not falling and have been grounded
            if (Time.time - this.lastTimeGravityApplied > this.groundedRemoveTime)
                OnDisable();


        }


        #endregion

        // ************************** Game **********************


        void OnEnable() {
            // record starting position transform and collider
            this.meTransform = transform;
            this.navAgent = meTransform.GetComponent<NavMeshAgent>();
            this.gravityVelocity = new Vector3();
            this.meCollider = this.meTransform.GetComponent<Collider>();
            this.meEntity = ABC_Utilities.GetStaticABCEntity(this.meTransform.gameObject);

            //Start the tracking of when gravity was last applied (If not applied again in x seconds script will be removed
            this.lastTimeGravityApplied = Time.time;

            this.IntegrateWithABC();
        }



        void Update() {

            this.ApplyGravity();
        }

        private void OnDisable() {

            //Turn on nav agent if disabled
            if (this.navAgent != null && this.navAgent.enabled == false)
                this.navAgent.enabled = true;

            //disable any subscriptions
            if (this.ABCEventsSM != null) {
                this.ABCEventsSM.onEnableGravity -= this.EnableGravity;
                this.ABCEventsSM.onDisableGravity -= this.DisableGravity;
            }

            Destroy(this);
        }

    }
}