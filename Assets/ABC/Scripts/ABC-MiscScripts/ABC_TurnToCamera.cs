using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Will make the object this component is attached to always face the camera 
    /// </summary>
    public class ABC_TurnToCamera : MonoBehaviour {

        // ********************* Variables ******************

        #region Variables

        Transform meTransform;

        #endregion

        // ********************* Game ******************

        #region Game

        // Start is called before the first frame update
        void Start() {
            this.meTransform = this.transform;

        }

        // Update is called once per frame
        void Update() {

            //turn to face camera
            meTransform.LookAt(this.meTransform.transform.position + Camera.main.transform.rotation * Vector3.back, Camera.main.transform.rotation * Vector3.up);
        }

        #endregion
    }
}