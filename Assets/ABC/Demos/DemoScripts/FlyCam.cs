using UnityEngine;
using System.Collections;

namespace ABCToolkit {
    /// <summary>
    /// Will allow the entity to fly freely around. 
    /// WASD: Movement
    /// Space: Climb
    /// E: Drop
    /// Shift: Move Faster
    /// Control: Move Slower
    /// </summary>
    public class FlyCam : MonoBehaviour {

        /// <summary>
        /// Camera Sensitivity
        /// </summary>
        public float cameraSensitivity = 90;

        /// <summary>
        /// Speed of climbing
        /// </summary>
        public float climbSpeed = 4;

        /// <summary>
        /// Normal move speed
        /// </summary>
        public float normalMoveSpeed = 10;

        /// <summary>
        /// How much slowness is applied
        /// </summary>
        public float slowMoveFactor = 0.25f;

        /// <summary>
        /// How much speed is applied during fast move
        /// </summary>
        public float fastMoveFactor = 3;

        /// <summary>
        /// Current Rotation X
        /// </summary>
        private float rotationX = 0.0f;

        /// <summary>
        /// Current Rotation Y
        /// </summary>
        private float rotationY = 0.0f;

        /// <summary>
        /// Input Manager
        /// </summary>
        private ABC_InputManager inputManager;

        /// <summary>
        /// Records entities rotation
        /// </summary>
        private void RecordRotation() {
            rotationX += this.inputManager.GetXAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationY += this.inputManager.GetYAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        }

        /// <summary>
        /// Will move the entity depending on the input
        /// </summary>
        private void Move() {

            if (this.inputManager.GetKey(KeyCode.LeftShift) || this.inputManager.GetKey(KeyCode.RightShift)) {

                transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * this.inputManager.GetYAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * this.inputManager.GetXAxis("Horizontal") * Time.deltaTime;

            } else if (this.inputManager.GetKey(KeyCode.LeftControl) || this.inputManager.GetKey(KeyCode.RightControl)) {

                transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * this.inputManager.GetYAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * this.inputManager.GetXAxis("Horizontal") * Time.deltaTime;

            } else {

                transform.position += transform.forward * normalMoveSpeed * this.inputManager.GetYAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * normalMoveSpeed * this.inputManager.GetXAxis("Horizontal") * Time.deltaTime;
            }


            if (this.inputManager.GetKey(KeyCode.Space)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
            if (this.inputManager.GetKey(KeyCode.E)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }
        }


        void Start() {
            this.inputManager = new ABC_InputManager();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        void Update() {

            this.RecordRotation();
            this.Move();

        }
    }
}