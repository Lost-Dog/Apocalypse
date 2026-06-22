using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Will control an element to display or hide with a trigger/button
    /// </summary>
    public class DisplayController : MonoBehaviour {

        // ************ Settings *****************************

        #region Settings

        [Header("Settings")]
        public GameObject displayObject;


        /// <summary>
        /// When the entity is displayed what speed does game play at
        /// </summary>
        public float enabledTimeMod = 1f;

        [Header("Mouse Settings")]

        /// <summary>
        /// Will unlock and show the mouse when the entity is displayed
        /// </summary>
        public bool unlockMouseOnDisplay = true;

        /// <summary>
        /// Will lock and hide the mouse when the entity is hidden
        /// </summary>
        public bool lockMouseOnHide = true;

        [Header("Trigger Settings")]

        /// <summary>
        /// The keycode which will display/hide entity when trigger type is set to Key
        /// </summary>
        public KeyCode triggerKey = KeyCode.None;

        /// <summary>
        /// The button which will display/hide entity when trigger type is set to Key
        /// </summary>
        public string triggerButton = "Pause";

        [Header("Collider Settings")]
        /// <summary>
        /// If true then the object will display whilst an entity collides  
        /// </summary>
        public bool showOnCollision = false;

        /// <summary>
        /// If true then the object will only display whilst an entity with a specific tag collides
        /// </summary>
        public bool collisionOnTags = false;

        /// <summary>
        /// The tag which is required to display the object on collision
        /// </summary>
        public string collisionTag;

        [Header("Quit Settings")]

        /// <summary>
        /// If enabled then whilst the display is showing the user can press a button to quit the game
        /// </summary>
        public bool QuitGameEnabled = false;

        /// <summary>
        /// The key press to quit the game
        /// </summary>
        public KeyCode QuitKey = KeyCode.Q;

        #endregion

        // ********************* Variables ******************

        #region Variables

        /// <summary>
        /// Input Manager
        /// </summary>
        private ABC_InputManager inputManager;


        #endregion



        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will display/hide the object depending on the parameter given
        /// </summary>
        /// <param name="Display">True to display the object, else false to hide</param>
        public void ToggleDisplay(bool Display) {

            //If display is false then hide object
            if (Display == false) {
                this.displayObject.SetActive(false);

                //Reset game speed to normal
                Time.timeScale = 1f;

                //Hide mouse if setup too
                if (this.lockMouseOnHide) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            } else { // else show object
                this.displayObject.SetActive(true);

                //Reset game speed to normal
                Time.timeScale = this.enabledTimeMod;

                //Show mouse if setup too
                if (this.unlockMouseOnDisplay) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }


        }


        /// <summary>
        /// Will switch the display state, if disabled it will be enabled and vice versa
        /// </summary>
        public void SwitchDisplayState() {

            //If the entity is active then hide it
            if (this.displayObject.activeInHierarchy)
                this.ToggleDisplay(false);
            else  // else show object
                this.ToggleDisplay(true);
        }


        /// <summary>
        /// Will quit the application
        /// </summary>
        public void QuitGame() {

            Application.Quit();

        }

        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// On collision enter
        /// </summary>
        /// <param name="col">Object that collided</param>
        private void OnColEnter(Transform col) {

            //If we are looking for specific tags and the object that collided doesn't have the tag then end here
            if (this.collisionOnTags == true && col.transform.tag != this.collisionTag)
                return;

            this.ToggleDisplay(true);


        }

        /// <summary>
        /// On collision exit
        /// </summary>
        /// <param name="col">Object that collided</param>
        private void OnColExit(Transform col) {

            //If we are looking for specific tags and the object that collided doesn't have the tag then end here
            if (this.collisionOnTags == true && col.transform.tag != this.collisionTag)
                return;

            this.ToggleDisplay(false);

        }

        /// <summary>
        /// Determines if the triggers setup have been activated/pushed
        /// </summary>
        /// <returns>True if a trigger has been pressed, else false</returns>
        private bool Triggered() {

            // If input type is key and the configured key is being pressed return true
            if (this.inputManager.GetKeyDown(this.triggerKey))
                return true;


            // if input type is button and the configured button is being pressed return true
            if (this.inputManager.GetButtonDown(this.triggerButton))
                return true;


            return false;

        }


        /// <summary>
        /// Will manage the quit game key press, if enabled, and the display is showing and correct key is triggered then application/game will close
        /// </summary>
        private void QuitGameManager() {

            if (this.QuitGameEnabled && this.displayObject.activeInHierarchy && this.inputManager.GetKeyDown(this.QuitKey))
                Application.Quit();


        }

        #endregion


        // ********************* Game ********************

        #region Game

        void OnCollisionEnter(Collision col) {

            this.OnColEnter(col.transform);

        }


        void OnTriggerEnter(Collider col) {

            this.OnColEnter(col.transform);

        }


        void OnCollisionExit(Collision col) {

            this.OnColExit(col.transform);

        }


        void OnTriggerExit(Collider col) {

            this.OnColExit(col.transform);

        }


        // Use this for initialization
        void Start() {

            //create input manager
            this.inputManager = new ABC_InputManager();

            //Hide or show mouse at start depending on if object is already hidden or not
            if (this.displayObject.activeInHierarchy == false && this.lockMouseOnHide)
                Cursor.visible = false;
            else if (this.displayObject.activeInHierarchy && this.unlockMouseOnDisplay)
                Cursor.visible = true;

        }

        // Update is called once per frame
        void Update() {

            if (this.Triggered())
                this.SwitchDisplayState();

            QuitGameManager();

        }

        #endregion


        // ********************* ENUM ********************

        #region ENUMS

        public enum TriggerType {
            Key,
            Button
        }

        #endregion

    }
}