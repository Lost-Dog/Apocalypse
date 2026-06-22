using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Component to switch who the user is playing as, will turn off and on certain components and change camera
/// </summary>
[System.Serializable]
public class PlayerSwitcher : MonoBehaviour {


    #region SwitchEntity Class
    [System.Serializable]
    public class SwitchEntity {


        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// Name of the switch entity
        /// </summary>
        public string name = "New Switch Entity";

        /// <summary>
        /// Unique ID of the switch entity
        /// </summary>
        public int switchID = 0; 

        /// <summary>
        /// Entity which will be switched too
        /// </summary>
        public GameObject entityObject;

        /// <summary>
        /// Camera which will be enabled
        /// </summary>
        public GameObject entityCamera;
 

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods


        #endregion


        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will enable this entity 'switching to it'
        /// </summary>
        public void SwitchToEntity() {

            //If entity not already enabled then enable it
            if (this.entityObject != null && this.entityObject.activeInHierarchy == false )
                this.entityObject.SetActive(true);

            //Always enable camera
            if (this.entityCamera != null)
                this.entityCamera.SetActive(true);

        }


        /// <summary>
        /// Will disable this entity 'switching from it'
        /// </summary>
        public void SwitchFromEntity(bool DisableEntity = false) {

            
            if (DisableEntity == true && this.entityObject != null && this.entityObject.activeInHierarchy == true)
                this.entityObject.SetActive(false);

            if (this.entityCamera != null)
                this.entityCamera.SetActive(false);

        }




        #endregion

    }
    #endregion

    // ************ Settings *****************************

    #region Settings


    /// <summary>
    /// Will switch to a random entity in game (Editor testing)
    /// </summary>
    public bool randomSwitch = false;

    [Space(10)]

    /// <summary>
    /// List of entities that can be switched too
    /// </summary>
    public List<SwitchEntity> switchEntities = new List<SwitchEntity>();


    #endregion



    // ********************* Public Methods ********************

    #region Public Methods

    /// <summary>
    /// Will switch to the entity chosen by the ID provided
    /// </summary>
    /// <param name="SwitchEntityID">ID of the entity to switch too</param>
    /// <param name="DisableEntities">If true then the other entities will be disabled when switching</param>
    public void SwitchToEntity(int SwitchEntityID) {

        //Find entity with the matching ID
        SwitchEntity switchEntity = this.switchEntities.Where(s => s.switchID == SwitchEntityID).FirstOrDefault();

        //If doesn't exist then return here
        if (switchEntity == null)
            return;

        
        //Switch from other entities 
        foreach (SwitchEntity entity in this.switchEntities) {

            if (entity.switchID != switchEntity.switchID)
                entity.SwitchFromEntity(true);

        }


        //Switch to the entity requested
        switchEntity.SwitchToEntity();


    }

    /// <summary>
    /// Will switch to the entity chosen by the name provided
    /// </summary>
    /// <param name="SwitchEntityName">Name of the entity to switch too</param>
    public void SwitchToEntity(string SwitchEntityName) {

        //Find entity with the matching Name
        SwitchEntity switchEntity = this.switchEntities.Where(s => s.name == SwitchEntityName).FirstOrDefault();

        if (switchEntity == null)
            return;

        this.SwitchToEntity(switchEntity.switchID);

    }




    #endregion



    // ********************* Game ********************

    #region Game


    void Update() {

        if (this.randomSwitch == true) {
            this.randomSwitch = false;

            this.SwitchToEntity(this.switchEntities[Random.Range(0, this.switchEntities.Count() - 1)].switchID);
            //this.SwitchToEntity(1);
        }


    }


    #endregion
}
