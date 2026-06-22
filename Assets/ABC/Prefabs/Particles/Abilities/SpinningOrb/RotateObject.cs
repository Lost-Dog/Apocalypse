using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will rotate the object depending on speeds provided
/// </summary>
public class RotateObject : MonoBehaviour {

    // ********************* Settings ********************

    #region Settings


    /// <summary>
    /// Determines if rotation is enabled or not
    /// </summary>
    public bool rotationEnabled = false;

    /// <summary>
    /// Rotation speed of the X Axis
    /// </summary>
    public float xRotationSpeed = 0f;

    /// <summary>
    /// Rotation speed of Y Axis
    /// </summary>
    public float yRotationSpeed = 0f;

    /// <summary>
    /// Rotation speed of Z Axis
    /// </summary>
    public float zRotationSpeed = 100f;


    /// <summary>
    /// Delay before rotation starts
    /// </summary>
    public float Delay = 0f;
    #endregion


    // ********************* Variables ********************

    #region  Variables

    /// <summary>
    /// Transform of entity
    /// </summary>
    Transform meTransform;

    #endregion

    // ********************* Private Methods ********************

    #region Private Methods

    /// <summary>
    /// Will enable and start the rotation of the entity (method is called with a delayed invoke)
    /// </summary>
    private void EnableRotation() {
        this.rotationEnabled = true;
    }

    /// <summary>
    /// Will rotate the entity using the settings provided
    /// </summary>
    private void Rotate() {

        //If rotation is not enabled then end here
        if (this.rotationEnabled == false)
            return;

        //Rotate entity
        transform.Rotate(xRotationSpeed * Time.deltaTime, yRotationSpeed * Time.deltaTime, zRotationSpeed * Time.deltaTime);
    }



    #endregion




    // ********************* Game ********************
    #region Game


    // Use this for initialization
    void Start () {
        this.meTransform = transform;

        //Starts rotation after the time given. 	
        Invoke("EnableRotation", this.Delay);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        this.Rotate();
   
    }

#endregion
}
