using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;

namespace ABCToolkit {

    /// <summary>
    /// The SaveManager is ABC functionality which manages the saving and loading of ABC related data and other extras like transforms, enable states and scenes. 
    /// This class can be called from any script. There is 2 parts to the ABC_SaveManager. One part is the component which you can setup to save and load data locally
    /// or retrieve the save and load data to be placed in a Database. The other part is function calls to create and load the game data which 
    /// should be used if you create your own Save System. Encryption can be used throughout for security. 
    /// </summary>
    [System.Serializable]
    public class ABC_SaveManager : MonoBehaviour {



        /// <summary>
        /// Singleton instance so switchercan be accessed by any part of the game
        /// </summary>
        public static ABC_SaveManager Manager = null;


        // ********************** Nested Classes ****************

        #region Nested Classes
        [Serializable]
        /// <summary>
        /// The Save master serves as the save file which stores all the game data for each object recorded. It also stores the save date 
        /// and other data like transform and scene information. 
        /// Save master files can be loaded by the SaveManager restoring its data and loading the scene etc 
        /// </summary>
        private class SaveMaster {

            /// <summary>
            /// The name of the save
            /// </summary>
            public string saveName;

            /// <summary>
            /// The date when the save was created
            /// </summary>
            public string saveDate;

            /// <summary>
            /// The name of the scene which is being saved
            /// </summary>
            public string sceneName;

            /// <summary>
            /// The game data which has been created to be saved and loaded later
            /// this data consists of a list of GameData
            /// </summary>
            public string savedData;

            /// <summary>
            /// Any tags listed here will also be saved along side all the ABC related objects in the scene
            /// </summary>
            public List<string> includeObjTags;


            /// <summary>
            /// A list of persistant entity names which will persist through game saves
            /// i.e the entities in this list will never be cleared when updating a save as they might be loaded in future scenes
            /// </summary>
            public List<string> persistantEntityNames = new List<string>();

            /// <summary>
            /// If enabled then the SaveManager will load transforms when loading a save file
            /// </summary>
            public bool loadTransform = true;

            /// <summary>
            /// If enabled then the SaveManager will load the active/disable state of each gameobject when loading a save file
            /// </summary>
            public bool loadEnableState = true;

            /// <summary>
            /// If enabled then the save manager will load the scene when loading a save file 
            /// </summary>
            public bool loadScene = true;



        }


        /// <summary>
        /// A wrapper class for the gamedata, the list in this class holds all the gamedata which will be saved and loaded
        /// This class holds all the important values in a save to be restored later
        /// </summary>
        private class SaveData {
            public List<GameData> saveData = new List<GameData>();
        }

        [Serializable]
        /// <summary>
        /// The Save data class holds all the data for each object being saved/loaded. Objects from this class are stored in a list in the SaveData class. 
        /// The data in this class is cycled through by the loader restoring the data to the appropriate object in the scene. 
        /// </summary>
        private class GameData {
            public string objName;
            public int objID;
            public Vector3 transformPosition;
            public Quaternion transformRotation;
            public bool objEnabled;
            public string dataABC_Controller;
            public string dataABC_StateManager;
            public string dataABC_WeaponPickUp;

            public GameData(String ObjName, int ObjID, Vector3 TransformPosition, Quaternion TransformRotation, bool ObjEnabled) {
                this.objName = ObjName;
                this.objID = ObjID;
                this.transformPosition = TransformPosition;
                this.transformRotation = TransformRotation;
                this.objEnabled = ObjEnabled;

            }

        }

        #endregion


        // ********************** Settings ******************

        #region Settings

        /// <summary>
        /// Name of the save file which will store data for all objects in the scene
        /// </summary>
        public string saveFileName = "Save1";

        /// <summary>
        /// If true then the persistant data path will be used when saving and loading locally
        /// </summary>
        public bool usePersistantDataPath = true;

        /// <summary>
        /// Path to save and load the save file which stores data for all objects in the scene 
        /// </summary>
        public string saveFilePath = @"C:\";

        /// <summary>
        /// Any tags listed here will also be saved along side all the ABC related objects in the scene
        /// </summary>
        public List<string> includeObjTags;

        /// <summary>
        /// List of specific object names which will never be deleted from a save file
        /// </summary>
        /// <remarks>
        /// Useful if you have objects that may not be loaded in current scene but will be in future scenes
        /// the objects data will always remain and not be overwritten but new save data
        /// </remarks>
        public List<String> persistantEntityNames = new List<string>();

        /// <summary>
        /// Crypto key to encrypt/decrypt the save file, if left blank "" no encryption is used
        /// </summary>
        public string key32Char = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z";

        /// <summary>
        /// If enabled then the SaveManager will load transforms when loading a save file
        /// </summary>
        public bool loadTransform = true;

        /// <summary>
        /// If enabled then the SaveManager will load the active/disable state of each gameobject when loading a save file
        /// </summary>
        public bool loadEnableState = true;

        /// <summary>
        /// If enabled then the save manager will load the scene when loading a save file 
        /// </summary>
        public bool loadScene = true;



        #endregion

        // ********************** Variables ******************

        #region Settings

        /// <summary>
        /// Records the current save in place, so this can be overwritten
        /// </summary>
        private SaveMaster currentSaveMaster;

        #endregion




        // *********************  Editor Methods ********************

        #region Editor Methods
#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace

        /// <summary>
        /// Will open the file panel to retrieve the save file location
        /// </summary>
        public string GetSaveFileLocation() {

            return UnityEditor.EditorUtility.SaveFolderPanel("Select a location to store the save file", "", ""); ;

        }

#endif

        #endregion



        // ********************* Public Methods ********************

        #region Public Methods


        /// <summary>
        /// Will add a persistant entity to the SaveManager 
        /// persistant entities which will persist through game saves and never be deleted
        /// </summary>
        /// <param name="EntityName">Name of entity to add to persistant list</param>
        public void AddPersistantObject(String EntityName) {

            if (EntityName != "")
                Manager.persistantEntityNames.Add(EntityName);
        }

        /// <summary>
        /// Will remove a persistant entity from the SaveManager 
        /// persistant entities which will persist through game saves and never be deleted
        /// </summary>
        /// <param name="EntityName">Name of entity to add to persistant list</param>
        public void RemovePersistantObject(String EntityName) {

            if (Manager.persistantEntityNames.Contains(EntityName))
                Manager.persistantEntityNames.Remove(EntityName);

        }



        // ********************* Local Save/Load Methods ********************

        /// <summary>
        /// Will save all ABC related data in the scene, creating an SaveMaster file locally
        /// </summary>
        /// <remarks>
        /// A save file will be created locally which lists all the names, ID's and data. This data will be cycled through on load restoring the values to each object saved.
        /// Will use settings from the SaveManager including save location and file name
        /// </remarks>
        public void SaveGameLocally() {

            //Grab save path if defined
            string path = this.saveFilePath + "/Saves";

            //If using persistant data path then use this instead
            if (this.usePersistantDataPath)
                path = Application.persistentDataPath + "/Saves";

            //Create Saves folder if it doesn't exist
            Directory.CreateDirectory(path);

            //Create the main save file (The Save Master)
            string saveFile = CreateSaveMaster(this.key32Char);


            //Save the file to the location
            File.WriteAllText(path + "/" + this.saveFileName + "-ABCSave.json", saveFile);

            Debug.Log("Game Saved Locally to: " + path);

        }


        /// <summary>
        /// Will load all data from the save defined in the SaveManager settings
        /// </summary>
        /// <param name="LoadPersistantObjectsOnly">(Optional) If enabled then only persistant entities will be loaded, potentially useful when just loading a new scene</param>
        public void LoadGameLocally(bool LoadPersistantObjectsOnly = false) {

            //Grab save path if defined where the save should exist
            string path = this.saveFilePath + "/Saves";

            //If using persistant data path then use this instead
            if (this.usePersistantDataPath)
                path = Application.persistentDataPath + "/Saves";

            //Check if file exists if it doesn't then log to console
            if (File.Exists(path + "/" + this.saveFileName + "-ABCSave.json") == false) {
                Debug.LogError("Loading Failed, File " + this.saveFileName + " does not exist in the following location: " + path);
                return;
            }

            //Load the game using the save file at the path and name configured in settings
            StartCoroutine(LoadSaveMaster(File.ReadAllText(path + "/" + this.saveFileName + "-ABCSave.json"), LoadPersistantObjectsOnly, this.key32Char));

            Debug.Log("Game Loading Completed");
        }


        /// <summary>
        /// Create a new save locally 
        /// </summary>
        /// <param name="NewSaveName">Name of new save</param>
        public void NewSaveGameLocally(string NewSaveName) {

            //Set the new save name in the settings so this can be loaded later
            this.saveFileName = NewSaveName;

            //Save game locally now new save name has been set
            this.SaveGameLocally();

        }


        /// <summary>
        /// Will find the local save file in the save path defined in the SaveManager which matches the name provided loading all it's data
        /// </summary>
        /// <param name="SaveFileNameToLoad">Name of save file to load</param>
        /// <param name="LoadPersistantObjectsOnly">(Optional) If enabled then only persistant entities will be loaded, potentially useful when just loading a new scene</param>
        public void NewLoadGameLocally(string SaveFileNameToLoad, bool LoadPersistantObjectsOnly = false) {

            //Set the new save name in the settings so this can be loaded next
            this.saveFileName = SaveFileNameToLoad;

            //Load game 
            this.LoadGameLocally(LoadPersistantObjectsOnly);

        }



        /// <summary>
        /// Will delete a local save file.  
        /// </summary>
        /// <param name="SaveFileName">Name of the ABCSave file</param>
        /// <param name="DeletePath">The path where all the saves files are stored, including the Save Master and individual object saved scripts</param>
        public void DeleteLocalSaveFile(string SaveFileName) {

            //Grab save path if defined where the save should exist
            string path = this.saveFilePath + "/Saves";

            //If using persistant data path then use this instead
            if (this.usePersistantDataPath)
                path = Application.persistentDataPath + "/Saves";

            //If file exists then delete it
            if (File.Exists(path))
                File.Delete(path);
            else
                Debug.LogError("Delete Failed, File " + SaveFileName + " does not exist in the following location: " + path);


        }



        // ********************* Custom Save/Load Methods ********************



        /// <summary>
        /// Will save all ABC related data in the scene, returning an ABCSaveMaster string which has all the main save data stored. 
        /// This can be used to be stored in a DB and loaded later rather then storing a file locally. 
        /// </summary>
        /// <returns>A Json string which holds all the data regarding the save</returns>
        public string CreateSaveGame() {

            return CreateSaveMaster(this.key32Char);


        }


        /// <summary>
        /// Will save all ABC related data in the scene, returning a new ABCSaveMaster string which has all the main save data stored. 
        /// This can be used to be stored in a DB and loaded later rather then storing a file locally. 
        /// </summary>
        /// <param name="NewSaveName">New Save Name</param>
        /// <returns>A Json string which holds all the data regarding the save</returns>
        public string CreateNewSaveGame(string NewSaveName) {
            //update save in settings
            this.saveFileName = NewSaveName;

            //Create and return the save file
            return this.CreateSaveGame();

        }


        /// <summary>
        /// Will load the save master data passed within the parameters, this file is a string of all the data which potentially was retrieved from a DB
        /// </summary>
        /// <param name="SaveFile">The save master data which was created from saving the game</param>
        /// <param name="LoadPersistantObjectsOnly">(Optional) If enabled then only persistant entities will be loaded, potentially useful when just loading a new scene</param>
        public void LoadGame(string SaveFile, bool LoadPersistantObjectsOnly = false) {
            StartCoroutine(LoadSaveMaster(SaveFile, LoadPersistantObjectsOnly, this.key32Char));

        }




        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will create and return a string of the main save which can then be saved locally or in a database. 
        /// </summary>
        /// <remarks>
        /// A save file will be created which lists all the names, ID's and data. This data will be cycled through on load restoring the values to each object saved.
        /// </remarks>
        /// <param name="ObjectsToSave">List of objects to save</param>
        /// <param name="CryptoKey">(Optional) The key to encrypt and decrypt the save file, if blank then file will not be encrypted/decrypted</param>
        private static string CreateSaveMaster(string CryptoKey = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z") {

            //If a save has not been loaded yet (i.e first time playing) create new save master
            if (Manager.currentSaveMaster == null)
                Manager.currentSaveMaster = new SaveMaster();

            //update save with name, date and persistant objects
            Manager.currentSaveMaster.saveDate = DateTime.Now.ToString();
            Manager.currentSaveMaster.saveName = Manager.saveFileName;
            Manager.currentSaveMaster.sceneName = SceneManager.GetActiveScene().name;
            Manager.currentSaveMaster.persistantEntityNames = Manager.persistantEntityNames;
            Manager.currentSaveMaster.includeObjTags = Manager.includeObjTags;
            Manager.currentSaveMaster.loadTransform = Manager.loadTransform;
            Manager.currentSaveMaster.loadEnableState = Manager.loadEnableState;
            Manager.currentSaveMaster.loadScene = Manager.loadScene;

            //Find initial ABC objects to save
            List<GameObject> objectsToSave = ABC_Utilities.GetAllABCObjects();

            //Loop through our include object tags and retrieve all objects with that tag adding it to the objectsToSave list unless it's already in the list
            foreach (string tag in Manager.currentSaveMaster.includeObjTags) {
                foreach (GameObject go in ABC_Utilities.GetAllObjectsWithTag(tag)) {
                    if (objectsToSave.Contains(go) == false)
                        objectsToSave.Add(go);
                }
            }


            Manager.currentSaveMaster.savedData = UpdateSaveData(Manager.currentSaveMaster.savedData, objectsToSave, CryptoKey, Manager.currentSaveMaster.persistantEntityNames);




            // if not in debug mode then encrypt, else don't
            if (CryptoKey != "")
                return Convert.ToBase64String(Encrypt(JsonUtility.ToJson(Manager.currentSaveMaster), CryptoKey));
            else
                return JsonUtility.ToJson(Manager.currentSaveMaster);


        }

        /// <summary>
        /// Will load all data from the save file provided
        /// </summary>
        /// <param name="SaveData">The SaveMaster data to load</param>
        /// <param name="LoadPersistantObjectsOnly">(Optional) If true then only persistant objects will be loaded and not the scene or other objects
        /// <param name="CryptoKey">(Optional) The key to encrypt and decrypt the save file, if blank then file will not be encrypted/decrypted</param>
        private static IEnumerator LoadSaveMaster(string SaveData, bool LoadPersistantObjectsOnly = false, string CryptoKey = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z") {

            //If a save has not been loaded yet (i.e first time playing) create new save master
            if (Manager.currentSaveMaster == null)
                Manager.currentSaveMaster = new SaveMaster();

            //If not in debug mode then decrypt the encrypted file
            if (CryptoKey != "")
                JsonUtility.FromJsonOverwrite(Decrypt(Convert.FromBase64String(SaveData), CryptoKey), Manager.currentSaveMaster);
            else
                JsonUtility.FromJsonOverwrite(SaveData, Manager.currentSaveMaster);


            //Load the scene async in background if set too and current scene isn't already loaded and we not just loading persistant objects
            if (LoadPersistantObjectsOnly == false && Manager.currentSaveMaster.loadScene == true && SceneManager.GetActiveScene().name != Manager.currentSaveMaster.sceneName) {
                AsyncOperation ao;
                ao = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Manager.currentSaveMaster.sceneName);

                // here's exactly how you wait for it to load:
                while (!ao.isDone) {
                    Debug.Log("Loading " + ao.progress.ToString("n2"));
                    yield return null;
                }
            }



            //Update the manager settings with the settings from the load so these values are used on next save
            Manager.saveFileName = Manager.currentSaveMaster.saveName;

            Manager.persistantEntityNames = Manager.currentSaveMaster.persistantEntityNames;

            //Update include obj tags
            Manager.includeObjTags = Manager.currentSaveMaster.includeObjTags;

            //update transform/status/scene
            Manager.loadTransform = Manager.currentSaveMaster.loadTransform;
            Manager.loadEnableState = Manager.currentSaveMaster.loadEnableState;
            Manager.loadScene = Manager.currentSaveMaster.loadScene;



            //Load the save data
            LoadSaveData(Manager.currentSaveMaster.savedData, Manager.currentSaveMaster.loadTransform, Manager.currentSaveMaster.loadEnableState, CryptoKey, LoadPersistantObjectsOnly ? Manager.currentSaveMaster.persistantEntityNames : null);



        }




        #endregion


        // ********************* API Methods ********************

        #region API Methods


        /// <summary>
        /// Will create ABC save game data for the objects provided, this is not related to the save manager component and can be called for your own systems
        /// This is however used by the save manager component as an example. The string that returns represents all the game data which can be loaded later
        /// </summary>
        /// <param name="ObjectsToSave">List of objects to save</param>
        /// <param name="CryptoKey">(Optional) The key to encrypt and decrypt the save file, if blank then file will not be encrypted/decrypted</param>
        /// <returns>String of Save Data</returns>
        public static string CreateSaveData(List<GameObject> ObjectsToSave, string CryptoKey = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z") {

            //Create return object
            SaveData retval = new SaveData();


            //Look through each object 
            foreach (GameObject objABC in ObjectsToSave) {

                //Create the game data which will be stored
                GameData gameData = new GameData(objABC.name, objABC.GetInstanceID(), objABC.transform.position, objABC.transform.rotation, objABC.gameObject.activeInHierarchy);

                //Retrieve the ABC Controller components for the object
                ABC_Controller abcController = objABC.GetComponent<ABC_Controller>();

                //If ABC Controller is found then store it's data
                if (abcController != null)
                    gameData.dataABC_Controller = JsonUtility.ToJson(abcController);

                //Retrieve the ABC StateManager components for the object
                ABC_StateManager abcStateManager = objABC.GetComponent<ABC_StateManager>();

                //If ABC StateManager is found then store it's data
                if (abcStateManager != null)
                    gameData.dataABC_StateManager = JsonUtility.ToJson(abcStateManager);


                //Retrieve the ABC WeaponPickUp components for the object
                ABC_WeaponPickUp abcWeaponPickUp = objABC.GetComponent<ABC_WeaponPickUp>();

                //If ABC WeaponPickUp is found then store it's data
                if (abcWeaponPickUp != null)
                    gameData.dataABC_WeaponPickUp = JsonUtility.ToJson(abcWeaponPickUp);

                //add save data to save master
                retval.saveData.Add(gameData);


            }


            // if not in debug mode then encrypt, else don't and return the save data (list of game data)
            if (CryptoKey != "")
                return Convert.ToBase64String(Encrypt(JsonUtility.ToJson(retval), CryptoKey));
            else
                return JsonUtility.ToJson(retval);

        }



        /// <summary>
        /// Will overwrite the ABC save game data provided, this is not related to the save manager component and can be called for your own systems
        /// This is however used by the save manager component as an example. The string that is passed through should be the SaveData created by the 
        /// Create SaveData function, a new save will be created from the objects provided overwriting the data but keeping any objects you don't want to delete
        /// </summary>
        /// <param name="Data">The existing SaveData string which will be overwritten with the new save data</param>
        /// <param name="NewObjectsToSave">List of new objects to save</param>
        /// <param name="CryptoKey">(Optional) The key to encrypt and decrypt the save file, if blank then file will not be encrypted/decrypted</param>
        /// <param name="DontDeleteObjectsName">A list of object names which are to remain in the existing save which is being overwritten</param>
        ///<remarks>
        ///Use this if you have save data which doesn't relate to the current scene but might be used in future scenes i.e you might have save data on a character
        ///which is not in the current scene but is in a future scene, by updating the save you can save the current scene but mark for the character to not be deleted
        ///ready to be loaded later 
        /// </remarks>
        /// <returns>String of updated save data</returns>
        public static string UpdateSaveData(string Data, List<GameObject> NewObjectsToSave, string CryptoKey = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z", List<string> DontDeleteObjectsName = null) {

            //Create return object
            SaveData retval = new SaveData();


            //If not in debug mode then decrypt the encrypted file
            if (Data != null && Data != "") {
                if (CryptoKey != "")
                    JsonUtility.FromJsonOverwrite(Decrypt(Convert.FromBase64String(Data), CryptoKey), retval);
                else
                    JsonUtility.FromJsonOverwrite(Data, retval);
            }


            //Store list of names of objects to not delete from existing save file when updating
            List<string> dontDelete = new List<string>();

            //add names of persistant objects to our dontDelete list
            if (DontDeleteObjectsName != null)
                dontDelete.AddRange(DontDeleteObjectsName);

            //remove all except objects/names stored in the dont delete list
            retval.saveData.RemoveAll(x => dontDelete.Any(y => y == x.objName) == false);


            //Look through each object 
            foreach (GameObject objABC in NewObjectsToSave) {

                //Create the game data which will be stored
                GameData gameData = new GameData(objABC.name, objABC.GetInstanceID(), objABC.transform.position, objABC.transform.rotation, objABC.gameObject.activeInHierarchy);

                //Retrieve the ABC Controller components for the object
                ABC_Controller abcController = objABC.GetComponent<ABC_Controller>();

                //If ABC Controller is found then store it's data
                if (abcController != null)
                    gameData.dataABC_Controller = JsonUtility.ToJson(abcController);

                //Retrieve the ABC StateManager components for the object
                ABC_StateManager abcStateManager = objABC.GetComponent<ABC_StateManager>();

                //If ABC StateManager is found then store it's data
                if (abcStateManager != null)
                    gameData.dataABC_StateManager = JsonUtility.ToJson(abcStateManager);

                //Retrieve the ABC WeaponPickUp components for the object
                ABC_WeaponPickUp abcWeaponPickUp = objABC.GetComponent<ABC_WeaponPickUp>();

                //If ABC WeaponPickUp is found then store it's data
                if (abcWeaponPickUp != null)
                    gameData.dataABC_WeaponPickUp = JsonUtility.ToJson(abcWeaponPickUp);


                //If we already have persistant data on someone, we are about to update so remove current one ready to add new updated data
                if (dontDelete.Count > 0 && dontDelete.Any(y => y == gameData.objName) && retval.saveData.Any(x => x.objName == gameData.objName)) {
                    GameData removeItem = retval.saveData.Where(x => x.objName == gameData.objName).FirstOrDefault();
                    retval.saveData.Remove(removeItem);
                }






                //add save data to save master
                retval.saveData.Add(gameData);


            }


            // if not in debug mode then encrypt, else don't
            if (CryptoKey != "")
                return Convert.ToBase64String(Encrypt(JsonUtility.ToJson(retval), CryptoKey));
            else
                return JsonUtility.ToJson(retval);

        }


        /// <summary>
        /// Will load the save data provided
        /// </summary>
        /// <param name="Data">Data to load</param>
        /// <param name="LoadTransform">(Optional) If true then transform data will be loaded</param>
        /// <param name="LoadEnableState">(Optional) if true then enabled/disable status for each object will be loaded</param>
        /// <param name="CryptoKey">(Optional) The key to decrypt the save data provided, if blank then file will not be decrypted</param>
        /// <param name="OnlyLoadObjectName">(Optional) If provided then only object names in this list will be loaded</param>
        public static void LoadSaveData(string Data, bool LoadTransform = false, bool LoadEnableState = false, string CryptoKey = "^mABj*sj+_{56MMRBJB&&SGRHAJBKM'z", List<String> OnlyLoadObjectName = null) {
            //Create save data object ready to load the data into
            SaveData savedData = new SaveData();


            //If not in debug mode then decrypt the encrypted file
            if (CryptoKey != "")
                JsonUtility.FromJsonOverwrite(Decrypt(Convert.FromBase64String(Data), CryptoKey), savedData);
            else
                JsonUtility.FromJsonOverwrite(Data, savedData);

            //Get all objects in scene to load into
            List<GameObject> allABCObjs = ABC_Utilities.GetAllObjectsInScene();

            //Loop through the master Saved data retrieving all the objects we are going to load data into 
            foreach (GameData data in savedData.saveData) {

                //If we are only loading certain objects/names and the current data being loaded does not relate to any of those entities then skip 
                if (OnlyLoadObjectName != null && OnlyLoadObjectName.Any(y => y == data.objName) == false)
                    continue;


                //declare the object which will be loading data in this cycle
                GameObject objToLoad;

                //If there is more then 1 object with the same name in our current play which has the same name then try and search for an object with the same name and instance ID recorded
                if (allABCObjs.Count(o => o.name == data.objName) > 1) {
                    objToLoad = allABCObjs.Where(o => o.name == data.objName && o.gameObject.GetInstanceID() == data.objID).FirstOrDefault();

                    //If we still can't find the entity then we will pick one duplicate named object at random and remove it so next time the other one is restored
                    if (objToLoad == null) {
                        objToLoad = allABCObjs.Where(o => o.name == data.objName).FirstOrDefault();
                        allABCObjs.Remove(objToLoad);
                    }

                } else { // else no duplicate of this name exists so retrieve an object with the same name
                    objToLoad = allABCObjs.Where(o => o.name == data.objName).FirstOrDefault();
                }


                //If we found an object then load all it's data
                if (objToLoad != null) {

                    //If enabled in parameter then enable/disable object from the save
                    if (LoadEnableState == true) {
                        objToLoad.SetActive(data.objEnabled);
                    }

                    //Retrieve the ABC Controller components for the object
                    ABC_Controller abcController = objToLoad.GetComponent<ABC_Controller>();

                    //If ABC Controller is found then load it's data
                    if (abcController != null) {

                        //Clear current graphics before overwriting the tracking lists
                        abcController.ClearAllPools();

                        JsonUtility.FromJsonOverwrite(data.dataABC_Controller, abcController);

                        //Reload Controller
                        if (objToLoad.activeInHierarchy == true)
                            abcController.Reload();
                    }

                    //Retrieve the ABC StateManager components for the object
                    ABC_StateManager abcStateManager = objToLoad.GetComponent<ABC_StateManager>();

                    //If ABC StateManager is found then load it's data
                    if (abcStateManager != null) {
                        JsonUtility.FromJsonOverwrite(data.dataABC_StateManager, abcStateManager);

                        //Reload StateManager
                        if (objToLoad.activeInHierarchy == true)
                            abcStateManager.Reload();
                    }

                    //Retrieve the ABC WeaponPickUp components for the object
                    ABC_WeaponPickUp abcWeaponPickUp = objToLoad.GetComponent<ABC_WeaponPickUp>();

                    //If ABC WeaponPickUp is found then load it's data
                    if (abcWeaponPickUp != null) {
                        JsonUtility.FromJsonOverwrite(data.dataABC_WeaponPickUp, abcWeaponPickUp);

                        //Reload StateManager
                        abcWeaponPickUp.Setup();
                    }

                    //If enabled in parameter then load the transform and rotation from the save
                    if (LoadTransform == true) {
                        objToLoad.transform.position = data.transformPosition;
                        objToLoad.transform.rotation = data.transformRotation;
                    }





                }

            }

            //Refresh entites to make sure all properties are up to date 
            ABC_Utilities.ReSetupAllABCEntities();

        }



        #endregion


        // ********************* Encryption Methods ********************


        public static byte[] Encrypt(string original, string key) // key must be 32chars
        {
            byte[] encrypted = null;

            try {
                byte[] iv = Encoding.ASCII.GetBytes("1234567890123456");
                byte[] keyBytes = Encoding.ASCII.GetBytes(key);

                using (RijndaelManaged myRijndael = new RijndaelManaged()) {
                    myRijndael.Key = keyBytes;
                    myRijndael.IV = iv;

                    encrypted = EncryptStringToBytes(original, myRijndael.Key, myRijndael.IV);
                }

            } catch (Exception e) {
                Debug.LogFormat("Error: {0}", e.Message);
            }

            return encrypted;
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged()) {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }


        public static string Decrypt(byte[] soup, string key) {
            string outString = "";

            try {
                byte[] iv = Encoding.ASCII.GetBytes("1234567890123456");
                byte[] keyBytes = Encoding.ASCII.GetBytes(key);

                using (RijndaelManaged myRijndael = new RijndaelManaged()) {
                    myRijndael.Key = keyBytes;
                    myRijndael.IV = iv;

                    outString = DecryptStringFromBytes(soup, myRijndael.Key, myRijndael.IV);
                }
            } catch (Exception e) {
                Debug.LogFormat("Error: {0}", e.Message);
            }

            return outString;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged()) {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }


        // ********************* Game ********************

        #region Game

        private void Awake() {

            //Declare singleton
            if (Manager == null)
                Manager = this;
            else if (Manager != this)
                Destroy(gameObject);

            //Make sure this singleton is accessable all over the game
            DontDestroyOnLoad(gameObject);
        }


        #endregion
    }
}