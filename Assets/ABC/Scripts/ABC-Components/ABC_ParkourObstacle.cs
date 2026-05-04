using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    public class ABC_ParkourObstacle : MonoBehaviour {


        // ********************* Settings ********************
        #region Settings


        /// <summary>
        /// If enabled then a parkour movement can be setup just for this obstacle
        /// </summary>
        public bool enableDynamicParkour = false; 

        /// <summary>
        /// Will dynamically activate a specific parkour movement
        /// </summary>
        public ABC_MovementController.ParkourMovement dynamicParkourMovement = new ABC_MovementController.ParkourMovement();

        /// <summary>
        /// The type of parkour to do
        /// </summary>
        public ABC_MovementController.ParkourType parkourType = ABC_MovementController.ParkourType.Vault;

        /// <summary>
        /// If true then the parkour will activate without input being required
        /// </summary>
        public bool activateWithoutInput = false; 


        /// <summary>
        /// If true then a specific parkour will activate from the tag provided
        /// </summary>
        public bool activateSpecificParkour = false; 

        /// <summary>
        /// If provided then a specific parkour will be used if a match is found
        /// </summary>
        [Tooltip("Add a matching tag here to play a specific Parkour Animation for the chosen type")]
        public string specificParkourTag = "";


        #endregion



        // ********************* Variables ********************
        #region Variables



        #endregion


        // ********************* Private Methods ********************
        #region Private Methods


        


        #endregion

        // ********************* Public Methods ********************
        #region Public Methods


        #endregion


        // ********************** Game ******************

        #region Game

      



        #endregion
    }
}