using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Component script which moves the entity to another object.
    /// </summary>
    /// <remarks>
    /// The entity with this component attached will move towards another object determined in the settings. You can also override the destination and provide a fixed Vector3 position for the entity to move to instead.  
    /// </remarks>
    public class ABC_ObjectToDestination : MonoBehaviour {


        // ********************* Settings ********************
        #region Settings


        /// <summary>
        /// Overrides destination, If this property is not (0, 0, 0) then the entity will always move to the Vector3 position provided.
        /// </summary>
        public Vector3 positionOverride = Vector3.zero;


        /// <summary>
        /// The GameObject that the entity will move to if positionOverride is (0, 0, 0)
        /// </summary>
        public GameObject destinationObj;

        /// <summary>
        /// Will add the offset provided to the Destination position. Useful if you want to the entity to stop before the destination or above it rather then directly on it.
        /// </summary>
        public Vector3 positionOffset = new Vector3(0, 0, 0);


        /// <summary>
        /// Will add an offset to the Destination forward position. Useful if you want to the entity to stop before or after the destination.
        /// </summary>
        public float positionForwardOffset = 0f;

        /// <summary>
        /// Will add an offset  to the Destination right position. Useful if you want to the entity to stop next to the destination.
        /// </summary>
        public float positionRightOffset = 0f;


        /// <summary>
        ///  How many seconds it takes to reach the destination;
        /// </summary>
        public float secondsToTarget = 3f;

        /// <summary>
        /// The delay before the entity will start travelling to the destination
        /// </summary>
        public float travelDelay = 0f;

        /// <summary>
        /// How close the entity needs to be to the destination position to stop moving towards it
        /// </summary>
        public float stopDistance = 0.6f;

        /// <summary>
        /// If true then the destination will be recalculated on each update so if the position of the gameobject moves then we will still move towards it, else the destination will only be determined once. 
        /// </summary>
        public bool continuouslyCalculateDestination = true;


        /// <summary>
        /// If true then the entity will rotate to face towards the destination, else it will just move to the destination without rotating. 
        /// </summary>
        public bool rotateToTarget = true;


        /// <summary>
        /// Will stop determine if movement will be stopped earlier due to a collision will only collide with entities named "Terrain", "Environment" or have an "Environment" Tag
        /// </summary>
        public bool stopMovementToDestinationOnCollision = false;

        /// <summary>
        /// If true then the stop movement collision will be checked continuously rather then just the once at the start
        /// </summary>
        public bool continuouslyCalculateStopMovementCollision = false;


        /// <summary>
        /// If not null then the entity will become a child of the GameObject provided once it reaches the destination.
        /// </summary>
        public GameObject destinationParent = null;

        /// <summary>
        /// If true then the entity will hover at the destination once it has been reached.
        /// </summary>
        public bool hoverOnSpot = false;

        /// <summary>
        /// The distance in which the entity will hover up and down
        /// </summary>
        public float hoverDistance = 0.4f;


        /// <summary>
        /// If true then this component will be removed once the entity reaches the destination, else it is just disabled. 
        /// </summary>
        public bool removeScript = false;


        #endregion

        // ********************* Variables ********************

        #region Variables

        // game object transform and rigidbody
        Transform meTransform;

        // where we started used in calculations to make sure we reach destination in x seconds
        Vector3 meStartingPosition = Vector3.zero;

        /// <summary>
        /// If true then the script will not operate as we are waiting for a travel delay
        /// </summary>
        private bool waitingOnTravelDelay = false;

        // lerp time which is ued in calculations to make sure we reach destination in x seconds
        private float lerpTime;

        // Tracks if the entity has reached the destination
        private bool destinationReached = false;


        // The actual position the entity is moving towards
        private Vector3 destinationPosition;

        #endregion

        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Calling this function will disable the Component and stop the entity from moving towards the destination.
        /// </summary>
        public void DisableScript() {


            //reset destination position
            this.destinationPosition = Vector3.zero;

            // reset variables
            this.destinationReached = false;
            this.waitingOnTravelDelay = false;


            // we can disable us now or remove depending on the variable 
            if (this.removeScript == true) {
                Destroy(this);
            } else {
                this.enabled = false;
            }

        }

        /// <summary>
        /// Calling this function will reset the component stopping the entity from moving towards its current destination
        /// </summary>
        public void ResetScript() {

            //reset destination position
            this.destinationPosition = Vector3.zero;

            // reset variables
            this.destinationReached = false;
            this.waitingOnTravelDelay = false;

            // reset lerp time
            lerpTime = 0.0f;

        }

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Called by an invoke with a delay, will set the waitingOnTravelDelay variable to true allowing the script to operate
        /// </summary>
        private void DisableWaitingOnTravelDelay() {
            this.waitingOnTravelDelay = false;
        }

        /// <summary>
        /// Will manually detect environment collisions in the circumstance where colliders have not been applied to the entity, will only collide with entities named "Terrain", "Environment" or have an "Environment" Tag
        /// </summary>
        private void ManuallyDetectEnvironmentCollision() {

            //If we are not stopping movement on collision then we do not need to manually detect collision
            if (this.stopMovementToDestinationOnCollision == false)
                return;

            //If offset is going down and we are near ground then end here
            if (this.positionOffset.y < 0 && ABC_Utilities.EntityDistanceFromGround(meTransform) < 0.6f)
                this.DisableScript();


            //If set to continuously calculate stop movement collision
            if (this.continuouslyCalculateStopMovementCollision == true) {
                Collider meCollider = GetComponent<Collider>();

                //If no collider exists then end here
                if (meCollider == null)
                    return;

                //Find all colliders in range but remove ourselves and any abilities
                List<Collider> hitCol;
                hitCol = Physics.OverlapBox(meCollider.bounds.center, new Vector3(meCollider.bounds.size.x + 1, 0, meCollider.bounds.size.z + 1)).ToList();
                hitCol.Remove(meCollider);

                //If we hit an environment (name contains terrain, environment or tagged as environment) then disable script
                if (hitCol.Where(c => c.name.Contains("Terrain") || c.name.Contains("Environment") || c.tag == "Environment" || c.GetComponent<MeshRenderer>() != null).Count() > 0) {
                    this.DisableScript();
                }
            }

        }

        /// <summary>
        /// Will work out and set the destination position for the entity to move towards.
        /// </summary>
        private void SetDestination() {

            if (this.positionOverride != Vector3.zero) {

                this.destinationPosition = this.positionOverride;

            } else if (this.destinationObj != null) {

                this.destinationPosition = this.destinationObj.transform.position;

            }



            //Add position Offset
            this.destinationPosition += positionOffset;

            //Add forward and right offsets if the destination transform was provided
            if (destinationObj != null)
                this.destinationPosition += destinationObj.transform.forward * this.positionForwardOffset + destinationObj.transform.right * this.positionRightOffset;


            //If we stopping movement on collision then raycast to the end position and find  the earliest point we can travel too (i.e stop if anything is in the way)
            if (this.stopMovementToDestinationOnCollision == true) {
                //can we reach the position?
                RaycastHit[] hits;

                Vector3 startingPosition = this.meStartingPosition + new Vector3(0, 1.3f, 0);
                Vector3 destinationPosition = this.destinationPosition + new Vector3(0, 1.3f, 0);
                hits = Physics.RaycastAll(startingPosition, destinationPosition - startingPosition, Vector3.Distance(startingPosition, destinationPosition));

                foreach (RaycastHit hit in hits) {

                    //if we hit ourselves or an ABC then continue
                    if (hit.transform == this.meTransform || hit.transform.name.Contains("ABC*_") || hit.transform.IsChildOf(this.meTransform) || hit.transform.name.Contains("*_ABCSurroundingObject"))
                        continue;

                    //If we hit environment
                    if (hit.transform.name.Contains("Terrain") || hit.transform.name.Contains("Environment") || hit.transform.tag == "Environment" || hit.transform.GetComponent<MeshRenderer>() != null) {

                        //Stop earlier if something was hit
                        if (Vector3.Distance(startingPosition, destinationPosition) > Vector3.Distance(startingPosition, hit.point)) {                            

                            //set the destination position to just before the hit.point 
                            this.destinationPosition = hit.point - (hit.point - this.meStartingPosition) * 1;
                        }
                            
                    }

                }
            }

        }


        /// <summary>
        /// Moves the entity towards the Destination
        /// </summary>
        private void MoveObjectToDestination() {

            //If destination has already been reached then we no longer need to move the object
            if (this.destinationReached == true)
                return;



            // If we are continously calculating destination then reset destination variables
            if (this.continuouslyCalculateDestination == true)
                SetDestination();


            //if we dont have the right information then error here
            if (this.destinationPosition == Vector3.zero)
                throw new System.InvalidOperationException("Error sending object to a position type. Destination position was not correctly set");


            // shall we rotate to destination?
            if (this.rotateToTarget == true)
                meTransform.LookAt(destinationPosition);


            // if seconds to target is 0 then the projectile is to just hover on the spot it was spawned. Rather then move towards the target.
            if (this.secondsToTarget > 0) {

                // next section helps get this graphic to the destination within x seconds
                lerpTime += Time.deltaTime / secondsToTarget;

                // lerp to position
                if (meTransform != null)
                    meTransform.position = Vector3.Lerp(meStartingPosition, destinationPosition, lerpTime);

            }




            // if we have reached destination or the seconds to target is 0 (it just stays on spot) then turn destination reached to true.
            if (Vector3.Distance(meTransform.position, this.destinationPosition) < this.stopDistance || this.secondsToTarget == 0) {


                // if move with target then make the object a child of our destination object
                if (this.destinationParent != null && this.secondsToTarget > 0) {
                    meTransform.parent = this.destinationParent.transform;
                    meTransform.localPosition = Vector3.zero;
                }

                // if hover on spot then enable the script already attached to the projectile
                if (this.hoverOnSpot == true) {
                    ABC_Hover hoverScript = meTransform.GetComponent<ABC_Hover>();

                    // If script doesn't exist add new one
                    if (hoverScript == null)
                        hoverScript = meTransform.gameObject.AddComponent<ABC_Hover>();

                    hoverScript.hoverDistance = this.hoverDistance;
                    hoverScript.enabled = true;

                }


                // we can disable us now or remove depending on the variable 
                this.DisableScript();


            }

        }

        #endregion

        // ********************* Game ********************

        #region Game


        void OnCollisionEnter(Collision col) {

            //we don't want to stop movement due to other other projectiles (which would just overwrite this component with new values if required)
            if (col.transform.GetComponent<ABC_Projectile>() != null)
                return;

            //If set to stop movement on collision then we can disable the script as we have finished
            if (this.stopMovementToDestinationOnCollision)
                this.DisableScript();


        }


        void OnTriggerEnter(Collider col) {

            //we don't want to stop movement due to other other projectiles (which would just overwrite this component with new values if required)
            if (col.transform.GetComponent<ABC_Projectile>() != null)
                return;

            //If set to stop movement on collision then we can disable the script as we have finished
            if (this.stopMovementToDestinationOnCollision)
                this.DisableScript();

        }



        // Use this for initialization
        void OnEnable() {

            // cache root of transform and rigidbody and starting position
            meTransform = transform;
            meStartingPosition = meTransform.position;

            //Reset script values
            this.ResetScript();

            //If we have a travel delay then flag the variable to true and then disable it after the delay 
            if (this.travelDelay > 0f) {
                this.waitingOnTravelDelay = true;
                Invoke("DisableWaitingOnTravelDelay", this.travelDelay);
            }

            //uncomment method call you want to check less often, will increase performance slightly
            //this.InvokeRepeating("ManuallyDetectEnvironmentCollision", 0f, 0.1f);
        }



        void LateUpdate() {

            //If script is not enabled or we are waiting on a travel delay then end here
            if (this.enabled == false || this.waitingOnTravelDelay)
                return;

            if (this.destinationPosition == Vector3.zero) {

                // set initial destinations for script
                this.SetDestination();

            } else {

                this.MoveObjectToDestination();

                this.ManuallyDetectEnvironmentCollision();

            }


        }




        private void OnDisable() {
            this.DisableScript();
            CancelInvoke();
        }
        #endregion
    }
}