using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Freezes the entity by keeping transform position the same or keeping rigidbody velocity at 0
    /// </summary>
    public class ABC_FreezePosition : MonoBehaviour {


        // ********************* Settings ********************
        #region Settings

        /// <summary>
        /// If true then component will be removed from entity when disabled
        /// </summary>
        public bool removeComponentOnDisable = true;


        /// <summary>
        /// If true then the entities position will always remain the same as where it started, freezing the entity in position
        /// </summary>
        public bool enableFreezePosition = true;


        /// <summary>
        /// If true then the position reset will occur every x seconds
        /// </summary>
        public bool slowReset = false;

        #endregion



        //************************ Variables / Private Properties ****************************************

        #region Variables / Private Properties

        /// <summary>
        /// Transform of the entity 
        /// </summary>
        private Transform meTransform;


        /// <summary>
        /// The Vector3 position to freeze the entity too
        /// </summary>
        private Vector3 freezePosition;


        /// <summary>
        /// Records the last time the position was reset 
        /// </summary>
        private float timeOfLastPositionReset = 0f;

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will record the freeze position (used for slow rest/invoked to get after spawn to avoid it recording transform too early)
        /// </summary>
        private void RecordFreezePosition() {
            this.freezePosition = transform.position;
            this.enableFreezePosition = true;
        }

        /// <summary>
        /// Will keep the entity at the position recorded when the component was enabled
        /// </summary>
        private void FreezePosition() {

            if (this.enableFreezePosition == false)
                return;

            //Keep entity position at the frozen transform
            if (this.slowReset == false || this.slowReset == true && Time.time - this.timeOfLastPositionReset > 15) {
                this.meTransform.position = this.freezePosition;
                this.timeOfLastPositionReset = Time.time;
            }

        }



        #endregion


        //************************ Game ****************************************

        #region Variables / Private Properties


        // Use this for initialization
        void OnEnable() {
            this.meTransform = transform;
            this.freezePosition = transform.position;

            if (this.slowReset == true) {
                this.enableFreezePosition = false;
                Invoke("RecordFreezePosition", 3f);
            }

        }

        // Update is called once per frame
        void Update() {

            //Freeze position
            this.FreezePosition();

        }

        private void OnDisable() {

            //If setup too then remove component
            if (this.removeComponentOnDisable)
                Destroy(this);
        }

        #endregion
    }
}