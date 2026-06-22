using UnityEngine;
using System.Collections;

namespace ABCToolkit {
    public class ABC_CameraShake : MonoBehaviour {


        // ********************* Settings ********************

        #region Settings

        //Total time for shaking in seconds
        /// <summary>
        /// The duration which the camera will shake for
        /// </summary>
        public float shakeDuration = 2.0f;

        /// <summary>
        /// The amount to shake by
        /// </summary>
        public Vector3 shakeAmount = new Vector3(1f, 1f, 0);

        /// <summary>
        /// The speed of the camera shake
        /// </summary>
        public float shakeSpeed = 2.0f;

        /// <summary>
        /// If enabled then the camera will start shaking
        /// </summary>
        public bool shake = false;

        #endregion

        // ********************* Variables ********************
        #region Variables

        /// <summary>
        /// Transform to manipulate
        /// </summary>
        private Transform meTransform = null;

        /// <summary>
        /// Camera component on object
        /// </summary>
        private Camera meCamera = null;


        /// <summary>
        /// Determines if the camera is currently shaking
        /// </summary>
        private bool isShaking = false;

        /// <summary>
        /// Amount over Lifetime [0,1]
        /// </summary>
        private AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Set it to true: The camera position is set in reference to the old position of the camera
        /// Set it to false: The camera position is set in absolute values or is fixed to an object
        /// </summary>
        private bool DeltaMovement = true;


        /// <summary>
        /// Last position of shake
        /// </summary>
        private Vector3 lastPos;

        /// <summary>
        /// Next position of shake
        /// </summary>
        private Vector3 nextPos;

        /// <summary>
        /// Last field of view
        /// </summary>
        private float lastFoV;

        /// <summary>
        /// Next field of view
        /// </summary>
        private float nextFoV;

        #endregion


        // ********************* Private Methods ********************
        #region Private Methods

        /// <summary>
        /// Will shake the object this component is attached too
        /// </summary>
        private void Shake(float Duration, float Amount, float Speed) {

            //If already shaking then end here
            if (this.isShaking)
                return;

            //Reset cam ready
            ResetCam();

            //Update global values ready to use when camera is shaken 
            this.shakeAmount = new Vector3(Amount, Amount, 0);
            this.shakeSpeed = Speed;

            //Start the shake 
            this.shakeDuration = Duration;


            // we are shaking now
            this.isShaking = true;
        }

        /// <summary>
        /// Resets the camera
        /// </summary>
        private void ResetCam() {
            //reset the last delta
            meTransform.Translate(DeltaMovement ? -lastPos : Vector3.zero);
            meCamera.fieldOfView -= lastFoV;

            //clear values
            lastPos = nextPos = Vector3.zero;
            lastFoV = nextFoV = 0f;
        }

        #endregion



        // ********************* Public Methods ********************
        #region Public Methods

        /// <summary>
        /// Will initiate the camera shake
        /// </summary>
        public void ActivateCameraShake() {


            if (this.isActiveAndEnabled == false)
                return;


            //If already shaking then end here
            if (this.isShaking)
                return;



            this.Shake(this.shakeDuration, this.shakeAmount.x, this.shakeSpeed);
        }


        /// <summary>
        /// Will initiate the camera shake with the parameters provided
        /// </summary>
        /// <param name="Duration">Duration to shake camera for</param>
        /// <param name="Amount">Amount to shake camera by</param>
        /// <param name="Speed">The speed of the camera shake</param>
        public void ActivateCameraShake(float Duration, float Amount, float Speed) {

            if (this.isActiveAndEnabled == false)
                return;

            //If already shaking then end here
            if (this.isShaking)
                return;



            Shake(Duration, Amount, Speed);
        }

        #endregion


        // ********************* Game ********************

        #region Game


        void Awake() {
            //Get transform component
            meTransform = this.transform;

            meCamera = this.GetComponent<Camera>();
        }


        void Update() {

            //For Component Editor testing 
            if (this.shake == true) {
                this.ActivateCameraShake();
                this.shake = false;
            }

        }


        private void LateUpdate() {

            //If duration higher then 0 then shake has started
            if (shakeDuration > 0) {

                this.shakeDuration -= Time.deltaTime;

                if (this.shakeDuration > 0) {
                    //next position based on perlin noise
                    nextPos = (Mathf.PerlinNoise(this.shakeDuration * this.shakeSpeed, shakeDuration * this.shakeSpeed * 2) - 0.5f) * this.shakeAmount.x * transform.right * Curve.Evaluate(1f - shakeDuration / this.shakeDuration) +
                              (Mathf.PerlinNoise(this.shakeDuration * this.shakeSpeed * 2, shakeDuration * this.shakeSpeed) - 0.5f) * this.shakeAmount.y * transform.up * Curve.Evaluate(1f - shakeDuration / this.shakeDuration);
                    nextFoV = (Mathf.PerlinNoise(this.shakeDuration * this.shakeSpeed * 2, shakeDuration * this.shakeSpeed * 2) - 0.5f) * this.shakeAmount.z * Curve.Evaluate(1f - shakeDuration / this.shakeDuration);

                    meCamera.fieldOfView += (nextFoV - lastFoV);
                    meCamera.transform.Translate(DeltaMovement ? (nextPos - lastPos) : nextPos);

                    lastPos = nextPos;
                    lastFoV = nextFoV;
                } else {
                    //shake ending
                    ResetCam();
                    this.isShaking = false;
                }
            }
        }




        #endregion
    }
}