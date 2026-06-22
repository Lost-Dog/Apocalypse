using UnityEngine;
using System;

namespace ABCToolkit {
    /// <summary>
    /// Component script which makes the entity hover up and down by the distance set. 
    /// </summary>
    public class ABC_Hover : MonoBehaviour {

        // ************************ Settings ***********************

        /// <summary>
        /// Determines how far the object will go up and down. 
        /// </summary>
        public float hoverDistance = 0.3f;


        // *************************** Variables ******************


        private float startingY;
        private Transform meTransform;

        // ************************** Game **********************


        void OnEnable() {
            // record starting position and transform
            meTransform = transform;
            startingY = meTransform.position.y;
        }


        void Update() {
            // move the object slightly depending on distance
            transform.position = new Vector3(meTransform.position.x, startingY + ((float)Math.Sin(Time.time) * hoverDistance), meTransform.position.z);
        }


    }
}