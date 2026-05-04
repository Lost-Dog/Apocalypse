using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Will spawn entities in a random position from the active player
/// </summary>
public class EntitySpawner : MonoBehaviour {

    #region SwitchEntity Class
    [System.Serializable]
    public class SpawnableEntity {


        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// Unique ID of the switch entity
        /// </summary>
        public int spawnID = 0;

        /// <summary>
        /// Entity which will be spawned
        /// </summary>
        public GameObject spawnObject;

        /// <summary>
        /// How far from the active player can the entity spawn randomly
        /// </summary>
        public float spawnRadius;

        /// <summary>
        /// graphic which will show when entity spawns
        /// </summary>
        public GameObject spawnGraphic;


        #endregion



        // ********************* Public Methods ********************

        #region Public Methods


        /// <summary>
        /// Function will spawn the object in a random position from the players position
        /// </summary>
        public IEnumerator Spawn() {


            //Create Object
            GameObject objectSpawned = Instantiate(this.spawnObject);
            objectSpawned.SetActive(false);

            //Find player
            GameObject player = GameObject.FindGameObjectsWithTag("Player").Where(p => p.activeInHierarchy == true).FirstOrDefault();

            //Get random position
            Vector3 randomPosition = player.transform.position + player.transform.forward * Random.Range(-this.spawnRadius, this.spawnRadius) + player.transform.right * Random.Range(-this.spawnRadius, this.spawnRadius);

            //Set position
            objectSpawned.transform.position = randomPosition;

            //Activate spawn graphic

            GameObject graphicObj = Instantiate(this.spawnGraphic);

            graphicObj.transform.parent = null;

            //place object at entity position
            graphicObj.transform.position = objectSpawned.transform.position + new Vector3(0, 1.3f, 0);

            //Set active 
            graphicObj.SetActive(true);


            //Wait for delay between graphic and spawn
            yield return new WaitForSeconds(0.2f);

            objectSpawned.SetActive(true);

            //Wait for duration
            yield return new WaitForSeconds(2f);

            //destroy graphic 
            Destroy(graphicObj);
        }



        #endregion

    }
    #endregion

    // ************ Settings *****************************

    #region Settings

    /// <summary>
    /// Will spawn a random entity in game (Editor testing)
    /// </summary>
    public bool randomSpawn = false;

    [Space(10)]


    /// <summary>
    /// The list of objects to spawn
    /// </summary>
    public List<SpawnableEntity> spawnableEntities = new List<SpawnableEntity>();




    #endregion



    // ********************* Public Methods ********************

    #region Public Methods

    /// <summary>
    /// Will spawn all entities
    /// </summary>
    /// <param name="ID">The ID of the entity to spawn</param>
    public void SpawnEntity(int ID) {


        StartCoroutine(this.spawnableEntities.Where(i => i.spawnID == ID).FirstOrDefault().Spawn());

    }


    #endregion



    // ********************* Game ********************

    #region Game


    void Update() {

        if (this.randomSpawn == true) {
            this.randomSpawn = false;

            this.SpawnEntity(this.spawnableEntities[Random.Range(0, this.spawnableEntities.Count() - 1)].spawnID);
        }


    }

    #endregion


}
