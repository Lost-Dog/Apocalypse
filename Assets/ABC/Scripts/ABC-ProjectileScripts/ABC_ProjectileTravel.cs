using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Component Script for moving Ability projectiles. Depending on the travel type provided the script will move the object in different ways. 
    /// </summary>
    /// <remarks>
    /// TravelTypes:
    /// 
    /// - MouseTarget, ToWorld, Crosshair, Mouse2D: Moves the projectile object towards the target position.
    /// - SelectedTarget, Self: Moves the projectile object towards the target gameobject.
    /// - Forward3D, MouseForward: Moves the projectile forward in a straight line. 
    /// 
    /// Although travel types group together to do the same action different targets/positions will be provided to the script by the Ability class. 
    /// In future further functionality can be added for just 1 travel type but for now most share common functions. 
    /// 
    /// </remarks>
    public class ABC_ProjectileTravel : MonoBehaviour {

        // ********************* Settings ********************

        #region Settings

        /// <summary>
        /// The travel type for the script. Different types move in different ways to different positions or targets.
        /// </summary>
        public TravelType travelType;

        /// <summary>
        /// The entity which has activated the ability also known as the Originator throughout ABC. 
        /// </summary>
        public GameObject originator = null;

        /// <summary>
        /// The object the projectile started on 
        /// </summary>
        public GameObject startingPositionObject = null;

        /// <summary>
        /// Offset of the starting position
        /// </summary>
        public Vector3 startingPositionOffset;

        /// <summary>
        /// Forward offset of the starting position
        /// </summary>
        public float startingPositionForwardOffset = 0f;

        /// <summary>
        /// Right offset of the starting position
        /// </summary>
        public float startingPositionRightOffset = 0f;

        /// <summary>
        /// How fast the projectile moves.
        /// </summary>
        public float travelSpeed = 20f;

        /// <summary>
        /// Determines if the projectile should move by force or velocity. String variable so only "Force" or "Velocity" should be entered
        /// </summary>
        public string travelPhysics = "Force";

        /// <summary>
        /// Time until projectile will start moving. Use if you want to wait before projectile start movings like adding a slight delay if deploying a missle from an aircraft. 
        /// </summary>
        public float travelDelay = 0f;

        /// <summary>
        /// If true then the projectile will only move for a duration.
        /// </summary>
        public bool applyTravelDuration = false;

        /// <summary>
        /// List of tags that the originator activating the ability requires for the travel duration to be applied
        /// </summary>
        public List<string> travelDurationOriginatorTagsRequired = new List<string>();

        /// <summary>
        /// Determines how long the projectile should move for.
        /// </summary>
        public float travelDurationTime = 5f;

        /// <summary>
        /// If true then once the projectile stops moving current velocity will be stopped. Leave false if you want the projectile to slow down naturally.
        /// </summary>
        public bool travelDurationStopSuddenly = false;

        /// <summary>
        /// Boomerang mode will change the projectile travel type so it returns back to the Originator after the delay. Once it reaches the Originator it will be disabled.
        /// If true then projectile will activate boomerang mode after the delay provided. This needs to be set before the Component is enabled as it is only activated from OnEnable(). 
        /// </summary>
        public bool boomerangMode = false;

        /// <summary>
        /// How long the projectile will wait before activating boomerang mode.
        /// </summary>
        public float boomerangDelay = 1f;

        #endregion


        // ********************* Target Settings ********************

        #region Target Type Settings


        /// <summary>
        /// Object the projectile will move towards if in Selected Target or Self mode (Travel Type). 
        /// </summary>
        public GameObject targetObject = null;

        /// <summary>
        /// Vector3 Position for the projectile to move towards if in MouseTarget, ToWorld, Crosshair or Mouse2D Mode (Travel Type).
        /// </summary>
        public Vector3 targetPosition = new Vector3(0, 0, 0);

        /// <summary>
        /// Will add the offset provided to the target destination position. Useful if you want to the entity to stop before the target destination or above it rather then directly on it.
        /// </summary>
        public Vector3 targetOffset = new Vector3(0, 0, 0);

        /// <summary>
        /// Will add an offset to the target destination forward position. Useful if you want to the entity to stop before or after the target destination.
        /// </summary>
        public float targetForwardOffset = 0f;

        /// <summary>
        /// Will add an offset  to the target destination right position. Useful if you want to the entity to stop next to the target destination.
        /// </summary>
        public float targetRightOffset = 0f;

        /// <summary>
        /// If true then the projectile will continuously rotate towards the target current destination even if they move, else if false then the projectile will record and rotate to 
        /// the target destination once and then just keep moving forward towards and past that position. 
        /// Turn this setting to false if you want the projectile to aim for the target but miss if the target moves or you want the projectile to keep moving past the destination.
        /// </summary>
        public bool continuouslyTurnToDestination = true;

        /// <summary>
        /// Adds a delay till the projectile will start heading towards the target destination. If greater then 0 then the ability will move forward in it's starting direction until the delay is up then it will head towards the destination.
        /// Useful if you want the ability projectile to move up from the entity before striking down. 
        /// </summary>
        public float seekTargetDelay = 0f;


        #endregion

        // ********************* Variables ********************

        #region  Variables



        // game object transform and rigidbody 
        Transform meTransform;
        Rigidbody meRigidbody;

        /// <summary>
        /// Used to destroy the projectile in boomerang mode when it reaches back to the Originator.
        /// </summary>
        ABC_Projectile meABCProjectile;

        /// <summary>
        /// If travel has been enabled (works with the delay setting)
        /// </summary>
        private bool travelEnabled = false;

        /// <summary>
        /// Bool to control when we can head towards our target or not.
        /// </summary>
        private bool seekTarget = false;

        /// <summary>
        /// Bool to control if the projectile can turn to face the target. If true the projectile will rotate to the current target destination each loop (moving towards them), if false then it will never rotate to target and will just move forward.
        /// </summary>
        private bool enableTurnToDestination = false;

        /// <summary>
        /// Bool to control if the projectile has already been set as a child to the target or not
        /// </summary>
        private bool parentSet = false;

        /// <summary>
        /// Bool to control if boomerang mode has been enabled
        /// </summary>
        private bool boomerangEnabled = false;


        // position of selected target 
        private Vector3 destination;

        #endregion

        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Function to tell the script to start moving the projectile. 
        /// </summary>
        public void EnableTravel() {
            this.travelEnabled = true;
        }

        /// <summary>
        /// Function which stops the projectile from moving. 
        /// </summary>
        /// <remarks>Used in invoke needs to be parameterless</remarks>
        public void DisableTravel() {

            this.travelEnabled = false;

            //stop travel immediately if set too
            if (this.travelDurationStopSuddenly)
                meRigidbody.linearVelocity = Vector3.zero;
        }



        /// <summary>
        /// Function which stops the projectile from moving. 
        /// </summary>
        /// <param name="OverrideTravelStop">If true then projectile will stop suddenly overwriting the stop suddenly setting</param>
        public void DisableTravel(bool OverrideTravelStop = false) {

            this.travelEnabled = false;

            //stop travel immediately if set too
            if (this.travelDurationStopSuddenly || OverrideTravelStop == true)
                meRigidbody.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Changes the current target used by the component. If a projectile is heading towards object x then you can call this method to change it to head towards object y.
        /// </summary>
        /// <param name="NewTarget">New object the projectile will move towards</param>
        /// <param name="ContinuouslyTurnToDestination">If true then the projectile will continously turn to the new target</param>
        public void GoToTarget(GameObject NewTarget, bool ContinuouslyTurnToDestination = false) {

            // change travel type to selected target so we can actually head towards the target 
            ChangeTravelType(TravelType.SelectedTarget);

            //update target and make sure we can rotate towards them 
            this.targetObject = NewTarget;
            this.enableTurnToDestination = true;
            this.continuouslyTurnToDestination = ContinuouslyTurnToDestination;

            //Deattach us from any objects we previously collided with and became a child of
            meTransform.parent = null;
            this.parentSet = false;
        }

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Switches travel type of the component.
        /// </summary>
        /// <param name="Type">New travel type to change too.</param>
        private void ChangeTravelType(TravelType Type) {

            this.travelType = Type;
        }



        /// <summary>
        /// Function which tells the script to start heading towards the target destination.
        /// </summary>
        private void EnableSeekTarget() {
            this.seekTarget = true;
        }

        /// <summary>
        /// Activates boomerang mode
        /// </summary>
        private void ActivateBoomerangMode() {


            // keep track if we have toggled the boomerang
            this.boomerangEnabled = true;

            // change the travel type since boomerang mode needs to go back to starting position (selected target)
            this.ChangeTravelType(TravelType.SelectedTarget);

            // make the starting position the new destination target 
            this.targetObject = this.startingPositionObject;
            this.targetOffset = this.startingPositionOffset;
            this.targetForwardOffset = this.startingPositionForwardOffset;
            this.targetRightOffset = this.startingPositionRightOffset;

            //Deattach us from any objects we previously collided with and became a child of
            meTransform.parent = null;
            this.parentSet = false;

            // reset looked at target 
            this.enableTurnToDestination = true;

            // make sure we stop at target (originator)
            this.continuouslyTurnToDestination = true;

            //turn travel back off if stopped
            this.EnableTravel();



        }


        /// <summary>
        /// Calculates the destination position depending on the Travel Type that the projectile needs to head towards.
        /// </summary>
        /// <returns>Vector3 position that the projectile will head towards</returns>
        private Vector3 GetDestinationPosition() {

            switch (travelType) {
                case TravelType.ToWorld:
                case TravelType.MouseTarget:
                case TravelType.Crosshair:
                case TravelType.Mouse2D:
                    // offset is in relation to ability for these types
                    return this.targetPosition + this.targetOffset + meTransform.forward * this.targetForwardOffset + meTransform.right * this.targetRightOffset;

                default:
                    // offset is in relation to the target for these types
                    return this.targetObject.transform.position + this.targetOffset + this.targetObject.transform.forward * this.targetForwardOffset + this.targetObject.transform.right * targetRightOffset;

            }
        }

        /// <summary>
        /// Rotates the projectile object towards the destination position
        /// </summary>
        private void TurnToDestination() {

            //If we have already looked at target then we can't do that again
            if (this.enableTurnToDestination == false)
                return;

            meTransform.LookAt(this.destination);

            // If we are travelling to a world or mouse location or not continuously keeping track of the destination then we will only turn to the destination once
            if (this.continuouslyTurnToDestination == false)
                enableTurnToDestination = false;

        }

        /// <summary>
        /// Moves the object by adding force/velocity to the objects forward transform
        /// </summary>
        private void Move() {

            //If in hit stop then slow movement right down
            if (meABCProjectile != null && meABCProjectile.ability.hitStopCurrentlyActive == true) {
                meRigidbody.linearVelocity = transform.forward * 0.3f;
                return;
            }


            // move out from caster:
            if (this.travelPhysics.ToUpper() == "VELOCITY")
                meRigidbody.linearVelocity = transform.forward * this.travelSpeed;

            if (this.travelPhysics.ToUpper() == "FORCE")
                meRigidbody.AddForce(transform.forward * this.travelSpeed);


        }


        /// <summary>
        /// Function called by travel types where the projectile will just move forward.
        /// </summary>
        private void TravelForward() {

            //If travel is not enabled we can end here
            if (this.travelEnabled == false)
                return;

            this.Move();
        }

        /// <summary>
        /// Function called by travel types where the projectile will move towards a target/position. 
        /// </summary>
        private void TravelToDestination() {

            //If travel is not enabled we can end here
            if (this.travelEnabled == false)
                return;

            // record destination position
            this.destination = this.GetDestinationPosition();


            //If we have reached the destination and we are always moving towards the target then set the projectile object as a child of the target so it can just move with the target.
            if (Vector3.Distance(meTransform.position, this.destination) < 0.4 && this.continuouslyTurnToDestination == true) {

                // we have reached target already unless they suddenly move very fast out of distance then we need to stay at target (MMO RPG style)
                meRigidbody.linearVelocity = Vector3.zero;
                meTransform.position = this.destination;

                // If we are in boomerange mode and we have reached back to originator then we can destroy now 
                if (this.boomerangEnabled == true && this.targetObject == this.startingPositionObject && meABCProjectile != null)
                    meABCProjectile.Destroy();


                // make sure we only set the parent once and only if a target is present and it's not the main world 
                if (this.parentSet == false && this.targetObject != null && this.travelType != TravelType.ToWorld) {
                    meTransform.parent = targetObject.transform;
                    this.parentSet = true;
                }


                // end here

                return;
            }


            // If we are currently seeking a target then look at destination (If not seeking target projectile will just move forward in its starting direction)
            if (this.seekTarget == true)
                TurnToDestination();

            // move object 
            this.Move();



        }

        #endregion



        // ********************* Game ********************

        #region Game

        // Use this for initialization
        void OnEnable() {



            //reset variables
            this.boomerangEnabled = false;
            this.parentSet = false;
            this.enableTurnToDestination = true;
            this.seekTarget = false;
            this.travelEnabled = false;


            // reset target position (forgets previous)
            this.destination = Vector3.zero;

            // cache root of transform and rigidbody 
            meTransform = transform.root;
            meRigidbody = transform.GetComponent<Rigidbody>();

            // get projectile script 
            if (gameObject.GetComponent<ABC_Projectile>() != null)
                meABCProjectile = gameObject.GetComponent<ABC_Projectile>();


            //Starts travel after the time given. 	
            Invoke("EnableTravel", this.travelDelay);

            // setting to stop projectile after it has travelled x time. 
            if (applyTravelDuration == true && (this.travelDurationOriginatorTagsRequired.Count == 0 || ABC_Utilities.ObjectHasTag(originator, this.travelDurationOriginatorTagsRequired)))
                Invoke("DisableTravel", this.travelDurationTime + this.travelDelay);


            // if were doing boomerang then enable after delay
            if (boomerangMode == true)
                Invoke("ActivateBoomerangMode", this.boomerangDelay);

            // in a small time we will go towards target
            Invoke("EnableSeekTarget", this.seekTargetDelay + this.travelDelay);

            // stop any previous velocity 
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;


        }




        void FixedUpdate() {

            // Depending on travel type we call different movement methods 
            switch (travelType) {

                case TravelType.SelectedTarget:
                case TravelType.NearestTag:
                case TravelType.MouseTarget:
                case TravelType.ToWorld:
                case TravelType.Self:
                case TravelType.Crosshair:
                case TravelType.Mouse2D:

                    TravelToDestination();
                    break;

                case TravelType.Forward:
                case TravelType.MouseForward:
                default:

                    TravelForward();
                    break;
            }


            //If gravity applied then look towards velocity
            if (this.meRigidbody != null && this.meRigidbody.useGravity == true) {
                this.meTransform.rotation = Quaternion.LookRotation(meRigidbody.linearVelocity);
            }


        }


        private void OnDisable() {

            // stop any previous velocity 
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            // stop any invokes/coroutines
            CancelInvoke();
            StopAllCoroutines();
        }

        #endregion

    }
}