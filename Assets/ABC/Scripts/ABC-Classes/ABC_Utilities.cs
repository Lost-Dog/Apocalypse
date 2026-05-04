using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.IO;

namespace ABCToolkit {
#if UNITY_EDITOR
    using UnityEditor;
[InitializeOnLoad]
#endif



    public static class ABC_Utilities {

#if UNITY_EDITOR
        static ABC_Utilities() {
            //Every time a file updates then refresh custom references for save/load
            EditorApplication.projectChanged += RefreshCustomReferences;
            EditorApplication.playModeStateChanged += LogPlayModeState;

            //Refresh custom references
            RefreshCustomReferences();
        }
#endif


        // ************ Nested Classes *****************************

        #region Global Nested Classes

        /// <summary>
        /// Class which records tag replacement, effectively replacing one tag with another for the whole component. Allowing users to change all tags globally. 
        /// </summary>
        [System.Serializable]
        public class TagConverter {

            /// <summary>
            /// Tag to be converted (which will be replaced)
            /// </summary>
            public string tagBefore = "";

            /// <summary>
            /// What we will convert the tag too
            /// </summary>
            public string tagAfter = "";


        }

        #endregion

        // ********************* Variables ********************
        #region Variables


        // The ABC Pool in the hiearchy which contains all the ABC Objects
        private static GameObject _abcPool;

        /// <summary>
        /// The ABC Pool in the hiearchy which contains all the ABC Objects
        /// </summary>
        public static GameObject abcPool {

            get {
                if (_abcPool == null) {
                    if (GameObject.Find("ABC*_Pool") == null) {
                        _abcPool = new GameObject("ABC*_Pool made from Code");
                        Debug.Log("A pool of ability objects (ABC*_Pool) has been created automatically");
                        _abcPool.name = "ABC*_Pool";
                    } else {

                        _abcPool = GameObject.Find("ABC*_Pool");
                    }

                    //Add the MbSurrogate script if it doesn't already exist
                    if (_abcPool.GetComponent<ABC_MbSurrogate>() == null)
                        _abcPool.AddComponent<ABC_MbSurrogate>();
                }

                return _abcPool;
            }
        }

        /// <summary>
        ///  surrogate MonoBehaviour which is used to call courotines
        /// </summary>
        private static MonoBehaviour _mbSurrogate;

        /// <summary>
        /// A property which allows the (non MonoBehaviour) Ability class to use and cache a MonoBehaviour surrogate from the running instance to allow for coroutine calls.
        /// </summary>
        public static MonoBehaviour mbSurrogate {
            get {
                // if no surrogate is current cached or the current one is no longer active then we need to find one (preferably ABC_Controller) 
                if (_mbSurrogate == null || _mbSurrogate.gameObject.activeInHierarchy == false) {

                    //Find our mbsurrogate script first as this will never be destroyed
                    _mbSurrogate = GameObject.FindObjectOfType<ABC_MbSurrogate>();

                    //if mbsurrogate can't be found from controller then find any monobehaviour
                    if (_mbSurrogate == null)
                        _mbSurrogate = GameObject.FindObjectOfType<MonoBehaviour>();

                }

                // if a surrogate is still not gounf then an error has occured, tell user and return null.
                if (_mbSurrogate == null) {
                    Debug.Log("No MonoBehaviour object was found in the scene to start coroutine from ability non monobehaviour class.");
                    return null;

                }

                return _mbSurrogate;

            }


        }




        #endregion


        // ********************* Private Variables ********************
        #region Private Variables

        /// <summary>
        /// Will keep track of all IEntity objects created in the game to be retrieved by any component at any time, helps with performance stopping scripts from having to remake entity objects in real time situations (surrounding checks)
        /// </summary>
        private static Dictionary<GameObject, ABC_IEntity> StaticABCEntities = new Dictionary<GameObject, ABC_IEntity>();

        /// <summary>
        /// The time that the StaticABCEntities list was last cleared
        /// </summary>
        private static float timeOfLastStaticABCEntitiesClear = 0f;

        /// <summary>
        /// How often in seconds that the StaticABCEntities list will be cleared (If 0 then list will never clear)
        /// </summary>
        private static float clearStaticABCEntitiesInterval = 600f;

        /// <summary>
        /// List of bones ignored by utilities functions
        /// </summary>
        private static List<int> ignoredBones = new List<int>{17, 18, 19, 20, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37,
        38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55};


        #endregion

        // ********************* Private Methods ********************
        #region Private Methods

#if UNITY_EDITOR
        /// <summary>
        /// Event handler to call functions when entering/exiting edit mode
        /// </summary>
        /// <param name="state">state of editor</param>
        private static void LogPlayModeState(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.EnteredEditMode:
                    RefreshCustomReferences();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    RefreshCustomReferences();
                    break;
            }
        }

        /// <summary>
        /// Will invoke the method passed in the parameter for any properties on the object which has the method
        /// </summary>
        /// <param name="obj">Object to run property methods</param>
        /// <param name="MethodName">Name of method to run</param>
        private static void InvokeMethodOnAllProperties(object obj, string MethodName) {

            //Get type of the object
            var type = obj.GetType();

            //Search through all fields in the object
            foreach (FieldInfo fi in type.GetFields()) {

                //Grab the property value of the field
                object propertyValue = fi.GetValue(obj);

                //If property value is null then continue to next field
                if (propertyValue == null)
                    continue;

                //Get method from the property value if it exists
                MethodInfo mi = propertyValue.GetType().GetMethod(MethodName);

                //If method exists then invoke it
                if (mi != null) {
                    mi.Invoke(propertyValue, new object[] { });
                }

                //If the property is a list then recursivley call this function to check and run the method (if it exists) on all the properties of the objects
                if (propertyValue.GetType().IsGenericType) {

                    //Get the list collection
                    var collection = (IEnumerable)propertyValue;

                    //Cycle through the list recalling this method recursivley to check the properties of the object to call the method
                    foreach (var item in collection) {
                        //If object exists call method
                        if (item != null)
                            InvokeMethodOnAllProperties(item, MethodName);

                    }
                }
            }
        }
#endif

        /// <summary>
        /// Will disable the object and place it back into the ABC Pool after the delay. This is private as it's called from PoolObject public method which handles the courotine
        /// </summary>
        /// <param name="Obj">Object to disable and pool</param>
        /// <param name="Delay">(Optional)Delay until object is disabled and placed in the pool</param> 
        private static IEnumerator PoolObjectAfterDuration(GameObject Obj, float Delay) {

            // if gameobject is not active then we can finish the method here
            if (Obj.activeInHierarchy == false)
                yield break;

            yield return new WaitForSeconds(Delay);

            Obj.transform.SetParent(abcPool.transform);

            Obj.SetActive(false);


        }

        #endregion

        // ********************* Public Methods ********************
        #region Public Methods


        /// <summary>
        /// Will call the refresh method on all custom references, updating their refname to match the value (refname is used for save/loading)
        /// <remarks>Called for example when a asset name changes, this will in turn call this method refreshing all the custom references resetting the refname to the new name of the asset</remarks>
        /// </summary>
        public static void RefreshCustomReferences() {
#if UNITY_EDITOR
            //Get all objects with ABC Components (in scene and files)
            foreach (GameObject obj in GetAllABCObjects(true)) {

                //Get controller component
                ABC_Controller abcController = obj.GetComponent<ABC_Controller>();

                //If controller found then call the refresh method on all custom reference properties
                if (abcController != null) {
                    InvokeMethodOnAllProperties(abcController, "RefreshCustomReference");
                    //Set dirty to save the changes
                    EditorUtility.SetDirty(abcController);
                }

                //Get state manager component
                ABC_StateManager abcStateManager = obj.GetComponent<ABC_StateManager>();

                //If state manager found then call the refresh method on all custom reference properties
                if (abcStateManager != null) {
                    InvokeMethodOnAllProperties(abcStateManager, "RefreshCustomReference");
                    //Set dirty to save the changes
                    EditorUtility.SetDirty(abcStateManager);
                }

            }

            //refresh all parts of the global elements
            foreach (ABC_GlobalElement globalElement in (Resources.FindObjectsOfTypeAll(typeof(ABC_GlobalElement)) as ABC_GlobalElement[]).Select(c => c).ToList()) {
                InvokeMethodOnAllProperties(globalElement.ElementAbilities, "RefreshCustomReference");
                InvokeMethodOnAllProperties(globalElement.ElementAIRules, "RefreshCustomReference");
                InvokeMethodOnAllProperties(globalElement.ElementEffects, "RefreshCustomReference");
                InvokeMethodOnAllProperties(globalElement.ElementWeapon, "RefreshCustomReference");
                InvokeMethodOnAllProperties(globalElement.elementIcon, "RefreshCustomReference");
            }

#endif
        }

        /// <summary>
        /// Generates a unique ID
        /// </summary>
        /// <returns>ID integer</returns>
        public static int GenerateUniqueID() {

            //Return ID made up of (1) 
            //last number of year  (0)
            //Day of the year      (140)
            //last number of minute (5)
            //random number       (465)
            return int.Parse(("1") + System.DateTime.Now.ToString("yy").Substring(1, 1) + "" + System.DateTime.Now.DayOfYear + "" + System.DateTime.Now.ToString("mm").Substring(1, 1) + Random.Range(0, 999));

        }

        /// <summary>
        /// Will add the ABC entity objects to a dictionary, tracked via the object as a key
        /// </summary>
        /// <param name="Obj">GameObject key</param>
        /// <param name="Entity">IEntity object which was made from the object</param>
        public static void AddStaticABCEntity(GameObject Obj, ABC_IEntity Entity) {

            //If IEntity object has not already been added then add it
            if (StaticABCEntities.ContainsKey(Obj) == false)
                StaticABCEntities.Add(Obj, Entity);

        }

        /// <summary>
        /// Will retrieve the IEntity object which links to the gameobject provided. If the IEntity object doesn't exist then one is created and returned
        /// </summary>
        /// <param name="Obj">Gameobject of the IEntity object</param>
        /// <returns>The IEntity Object linked to the gameobject, else creates a new one and returns that</returns>
        public static ABC_IEntity GetStaticABCEntity(GameObject Obj) {

            //If the interval defined is over 0 and the interval has passed since the last clean out then  clear the StaticABCEntities list to stop it growing too big with objects no longer in game. 
            if (clearStaticABCEntitiesInterval > 0 && Time.time - timeOfLastStaticABCEntitiesClear > clearStaticABCEntitiesInterval) {
                //clear list to be made fresh 
                StaticABCEntities.Clear();
                //Record time of last clear
                timeOfLastStaticABCEntitiesClear = Time.time;
            }


            if (StaticABCEntities.ContainsKey(Obj) == true)
                return StaticABCEntities[Obj];
            else if (Obj.transform.parent != null && StaticABCEntities.ContainsKey(Obj.transform.parent.gameObject))
                return StaticABCEntities[Obj.transform.parent.gameObject];
            else if (Obj.transform.root != null && StaticABCEntities.ContainsKey(Obj.transform.root.gameObject))
                return StaticABCEntities[Obj.transform.root.gameObject];
            else
                return new ABC_IEntity(Obj);

        }

        /// <summary>
        /// Returns true if the object provided has already been converted into a Static ABC Entity 
        /// </summary>
        /// <param name="Obj">GameObject to check if conversion to Static ABC entity has happened</param>
        /// <returns>True if static ABC entity, else false</returns>
        public static bool IsStaticABCEntity(GameObject Obj) {
            return StaticABCEntities.ContainsKey(Obj);
        }

        /// <summary>
        /// Will reload all ABC entities ensuring that all components are up to date
        /// </summary>
        public static void ReSetupAllABCEntities() {

            foreach (ABC_IEntity entity in StaticABCEntities.Values)
                entity.ReSetupEntity();

        }

        /// <summary>
        /// Will reload all ABC entities ensuring that all properties are up to date
        /// </summary>
        public static void ReloadAllABCEntities() {

            foreach (GameObject Obj in GetAllABCObjects().Where(obj => obj.activeInHierarchy == true && obj.GetComponent<ABC_Controller>() != null).ToList())
                Obj.GetComponent<ABC_Controller>().Reload();

        }

        /// <summary>
        /// Will return all ABC Entities that are in range of the starting position provided
        /// </summary>
        /// <param name="StartingPosition">Starting position to check distance to</param>
        /// <param name="Range">The range in which entities have to be within the starting position to be returned</param>
        /// <param name="OrderByClosest">Will order the list by closest entity from starting position</param>
        /// <returns>List of ABC entities in the range</returns>
        public static List<ABC_IEntity> GetAllABCEntitiesInRange(Vector3 StartingPosition, float Range, bool OrderByClosest = false) {

            List<ABC_IEntity> retVal = StaticABCEntities.Values.Where(e => e != null && e.gameObject != null && e.transform != null && e.gameObject.activeInHierarchy == true && Vector3.Distance(StartingPosition, e.transform.position) <= Range).ToList();

            //order by distance if too
            if (retVal.Count > 1 && OrderByClosest == true) {
                retVal = retVal.OrderBy(d => (d.transform.position - StartingPosition).sqrMagnitude).ToList();
            }


            return retVal;


        }

        /// <summary>
        /// Returns a bool indicating if the object has any tags that match the tags in the list provided
        /// </summary>
        /// <remarks>
        /// Will check the objects tags and ABC tags created in the statemanager component
        /// </remarks>
        /// <param name="Obj">Object which will be checked to see if it has a tag matching any elements in the list provided</param>
        /// <param name="TagList">List of string tags</param>
        /// <returns>True if the object's tags match any element in the taglist provided, else false.</returns>
        public static bool ObjectHasTag(GameObject Obj, List<string> TagList) {

            //If object not provided then return false
            if (Obj == null)
                return false;

            // get iEntity to check for ABC tags
            ABC_IEntity iEntity = GetStaticABCEntity(Obj);

            // loop through taglist and return true if object has a normal tag
            foreach (string element in TagList) {

                // check if list matches any ABC tags
                if (iEntity.HasABCTag(element))
                    return true;


                if (Obj.tag == element)
                    return true;

            }


            // went through tag list and no match was found so we return false; 
            return false;
        }

        /// <summary>
        /// Returns a bool indicating if the object has any tags that match the tag provided
        /// </summary>
        /// <remarks>
        /// Will check the objects tags and ABC tags created in the statemanager component
        /// </remarks>
        /// <param name="Obj">Object which will be checked to see if it has a tag matching any elements in the list provided</param>
        /// <param name="Tag">string tag to check</param>
        /// <returns>True if the object's tags match any element in the tag provided, else false.</returns>
        public static bool ObjectHasTag(GameObject Obj, string Tag) {

            //If object not provided then return false
            if (Obj == null)
                return false;

            // get iEntity to check for ABC tags
            ABC_IEntity iEntity = GetStaticABCEntity(Obj);

            // we can only deal with objects which have a state manager script
            if (iEntity.HasABCTag(Tag))
                return true;


            if (Obj.tag == Tag)
                return true;



            // went through tag list and no match was found so we return false; 
            return false;
        }


        /// <summary>
        /// Function will loop through the tags provided changing any set to be converted from X to Y. For example 'Friendly' tag may have been setup to be converted to 'Enemy'
        /// tags will remain unchanged if they do not match any conversions setup in either the ABC Controller or StateManager
        /// </summary>
        /// <param name="Entity">IEntity object which has the tag conversions setup</param>
        /// <param name="TagsToConvert">Tags to convert</param>
        /// <returns>List of new tags gone through the conversion process, if a conversion existed then it will have been replaced into the new value, otherwise if not setup to be converted then the tags provided will be unchanged</returns>
        public static List<string> ConvertTags(ABC_IEntity Entity, List<string> TagsToConvert) {

            //get current tags
            List<TagConverter> tagConversions = Entity.TagConversions.ToList();

            //final returned list
            List<string> retVal = new List<string>();

            //Loop through tags to convert
            foreach (string tag in TagsToConvert) {

                //If tag to convert does not match one of the conversions then just readd the original tag
                if (tagConversions.Where(t => t.tagBefore.Trim() == tag.Trim()).Count() == 0)
                    retVal.Add(tag);
                else // else add the converted tag
                    retVal.Add(tagConversions.Where(t => t.tagBefore.Trim() == tag.Trim()).FirstOrDefault().tagAfter);

            }

            //Return the result
            return retVal;
        }


        /// <summary>
        /// Will search the hiearchy within the object provided returning the first object to match the tag provided. 
        /// </summary>
        /// <param name="EntityToTraverse">Entity Object which will be looked through for a tag</param>
        /// <param name="Tag">Tag to match</param>
        /// <param name="IncludeParent">(Optional) If true then parent object will be checked to see if it has the correct tag, else function will only focus on children</param>
        /// <param name="PrioritiseEquippedWeapon">(Optional) If true then equipped weapon objects are checked first for the tag</param>
        /// <returns>First Gameobject encountered which has the matching tag </returns>
        public static GameObject TraverseObjectForTag(ABC_IEntity EntityToTraverse, string Tag, bool IncludeParent = true, bool PrioritiseEquippedWeapon = true) {

            //If tag is null or empty then return
            if (string.IsNullOrEmpty(Tag))
                return null;

            //if priotised then check equipped weapon first
            if (PrioritiseEquippedWeapon == true && EntityToTraverse.currentEquippedWeapon != null) {

                //Loop through all the weapon graphics in game
                foreach (GameObject weaponGraphic in EntityToTraverse.currentEquippedWeapon.GetWeaponGraphics().ToList()) {

                    if (weaponGraphic == null)
                        continue;

                    //If any transform in the graphic has the tag
                    foreach (Transform obj in weaponGraphic.GetComponentsInChildren<Transform>().Where(obj => obj.gameObject.activeInHierarchy == true).ToList()) {

                        //Return the object
                        if (obj.CompareTag(Tag))
                            return obj.gameObject;

                    }

                }

            }

            //Get children
            List<Transform> linkedObjects = new List<Transform>();
            linkedObjects = EntityToTraverse.gameObject.GetComponentsInChildren<Transform>().Where(obj => obj.gameObject.activeInHierarchy == true).ToList();

            //If parameter set then include parent
            if (IncludeParent == true && EntityToTraverse.gameObject.transform.parent != null)
                linkedObjects.Add(EntityToTraverse.gameObject.transform.parent.GetComponent<Transform>());


            //Then search through all object returning the first object matching the tag
            foreach (Transform obj in linkedObjects) if (obj.CompareTag(Tag))
                    return obj.gameObject;

            //If reached this far then tag can't be found so return null
            return null;
        }



        /// <summary>
        /// Will search the hiearchy within the object provided searching for the component provided in the parameter, returning the first one found
        /// </summary>
        /// <remarks>Will look in following order: Object, Parent of Object, Root of Object, Children in Parent, Children in Root</remarks>
        /// <param name="ObjectToTraverse">Parent Object which will be looked through for a tag</param>
        /// <returns>First statemanager component encountered</returns>
        public static Component TraverseObjectForComponent(GameObject ObjectToTraverse, System.Type ComponentToFind) {


            // find state script on the object 
            Component retVal = ObjectToTraverse.transform.GetComponent(ComponentToFind);

            //If doesn't exist then try children
            if (retVal == null)
                retVal = ObjectToTraverse.GetComponentInChildren(ComponentToFind);

            //return what we have found
            return retVal;
        }

        /// <summary>
        /// Will disable the object and place it back into the ABC Pool
        /// </summary>
        /// <param name="Obj">Object to disable and pool</param>
        /// <param name="Delay">(Optional)Delay until object is disabled and placed in the pool</param> 
        public static void PoolObject(GameObject Obj, float Delay = 0f) {

            if (Obj == null)
                return;


            //If a duration has been given then pool object after the duration
            if (Delay > 0f) {
                mbSurrogate.StartCoroutine(PoolObjectAfterDuration(Obj, Delay));
                return;
            }

            // else no duration so we can set inactive and pool straight away
            Obj.transform.SetParent(abcPool.transform);
            Obj.SetActive(false);


        }

        /// <summary>
        /// Will retrieve all objects in the game which contain either the ABC_Controller or ABC_StateManager component
        /// </summary>
        /// <param name="GetObjectsNotInScene">If true then objects not in scene (prefabs etc) are also returned</param>
        /// <returns></returns>
        public static List<GameObject> GetAllABCObjects(bool GetObjectsNotInScene = false) {

            //Collect all objects in scene that relate to ABC (Making sure not to repeat ourselves)
            List<GameObject> retval = new List<GameObject>();
            retval.AddRange((Resources.FindObjectsOfTypeAll(typeof(ABC_Controller)) as ABC_Controller[]).Select(a => a.gameObject).ToList());
            retval.AddRange((Resources.FindObjectsOfTypeAll(typeof(ABC_StateManager)) as ABC_StateManager[]).Select(b => b.gameObject).Except(retval).ToList());
            retval.AddRange((Resources.FindObjectsOfTypeAll(typeof(ABC_WeaponPickUp)) as ABC_WeaponPickUp[]).Select(c => c.gameObject).Except(retval).ToList());


            //Remove all persistant objects (prefabs etc)
            if (GetObjectsNotInScene == false)
                retval = retval.Select(item => item.gameObject).Where(obj => obj.scene.name != null).ToList();

            return retval;

        }


        /// <summary>
        /// Will find all objects in the current scene
        /// </summary>
        /// <returns>List of GameObjects in the scene</returns>
        public static List<GameObject> GetAllObjectsInScene() {
            List<GameObject> objectsInScene = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
                if (go.hideFlags != HideFlags.None)
                    continue;

                objectsInScene.Add(go);
            }

            objectsInScene = objectsInScene.Select(item => item.gameObject).Where(obj => obj.scene.name != null).ToList();

            return objectsInScene;
        }

        /// <summary>
        /// Will find all objects with the tag
        /// </summary>
        /// <returns>List of GameObjects in the scene</returns>
        public static List<GameObject> GetAllObjectsWithTag(string Tag) {

            return GameObject.FindGameObjectsWithTag(Tag).ToList();
        }

        /// <summary>
        /// Will return a global element (from the database/files) matching the global element name provided
        /// </summary>
        /// <param name="GlobalElementName">Name of the Global Element to get</param>
        /// <param name="IncludeWeapons">If true then weapons are included in the search, else they are ignored</param>
        /// <param name="IncludeAbilities">If true then abilities are included in the search, else they are ignored</param>
        /// <param name="IncludeEffects">If true then effects are included in the search, else they are ignored</param>
        /// <returns></returns>
        public static ABC_GlobalElement GetGlobalElement(string GlobalElementName, bool IncludeWeapons = true, bool IncludeAbilities = true, bool IncludeEffects = true) {

            ABC_GlobalElement retVal = null;

            ABC_GlobalElement[] globalElements = Resources.LoadAll<ABC_GlobalElement>("");

            for (int i = 0; i < globalElements.Length; i++) {

                if (globalElements[i].elementType == ABC_GlobalElement.GlobalElementType.Weapon && IncludeWeapons == false)
                    continue;

                if (globalElements[i].elementType == ABC_GlobalElement.GlobalElementType.Abilities && IncludeAbilities == false)
                    continue;

                if (globalElements[i].elementType == ABC_GlobalElement.GlobalElementType.Effect && IncludeEffects == false)
                    continue;

                if (globalElements[i].name == GlobalElementName) {
                    retVal = globalElements[i];
                }
            }


            //Return the global element found
            return retVal;

        }

        /// <summary>
        /// Will recursively generate and return a list of all the abilities attached to the global element
        /// </summary>
        /// <param name="GlobalElement">Global Element storing all the abilities</param>
        /// <returns>list of abilities grabbed from the global element</returns>
        public static List<ABC_Ability> GetAbilitiesFromGlobalElement(ABC_GlobalElement GlobalElement) {

            //List to generate and return
            List<ABC_Ability> retVal = new List<ABC_Ability>();

            //Loop through all abilities attached to the global element
            foreach (ABC_Ability ability in GlobalElement.ElementAbilities) {

                //If normal ability then add to list
                if (ability.globalAbilities == null)
                    retVal.Add(ability);
                else // else if global abilities are attached to this ability then recursively get those abilities
                    retVal.AddRange(GetAbilitiesFromGlobalElement(ability.globalAbilities));

            }

            return retVal;
        }

        /// <summary>
        /// Will determine if the parameter provided has been setup on the Animator provided
        /// </summary>
        /// <param name="Animator">The Animator to check if the parameter has been setup</param>
        /// <param name="ParameterName">Name of the parameter to check if it exists</param>
        /// <returns>True if the parameter exists on the Animator, else false</returns>
        public static bool AnimatorParameterExist(Animator Animator, string ParameterName) {

            //Check the Animator for the parameter, returning true if found
            foreach (AnimatorControllerParameter param in Animator.parameters) {
                if (param.name == ParameterName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Starts an animation clip using the ABC animation runner stopping it after the animation clip duration
        /// </summary>
        /// <param name="Entity">Entity hit animation is running on</param>
        /// <param name="AnimationClip">Clip to run</param>
        /// <param name="InterruptCurrentAnimation">If true then current animation playing will be interrupted, else it won't</param>
        public static IEnumerator RunAnimationClip(ABC_IEntity Entity, AnimationClip AnimationClip, bool InterruptCurrentAnimation = true) {

            ABC_AnimationsRunner AnimationRunner = Entity.animationRunner;

            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (AnimationClip == null || AnimationRunner == null)
                yield break;


            //Start animation
            bool animationStarted = AnimationRunner.StartAnimation(AnimationClip, 0, 1, null, InterruptCurrentAnimation);

            //If animation did not start (interruption turned off) then end here
            if (animationStarted == false)
                yield break;

            //else wait for duration and then end animation
            for (var i = AnimationClip.length; i > 0;) {

                // actual wait time for preparation
                if (i < 0.2f) {
                    // less then 0.2  so we only need to wait the .xx time
                    yield return new WaitForSeconds(i);
                } else {
                    // wait a second and keep looping till casting time = 0; 
                    yield return new WaitForSeconds(0.2f);
                }

                //Take of a second from the animation runner duration tracker if animator play speed is higher then 0.2 (if lower then hitstop is happening) 
                //Animation Runner gets is speed live from the animator so don't need to check that, checking Animators speed results in checking for both 
                if (Entity.animator == null || Entity.animator != null && Entity.animator.speed > 0.2f)
                    i = i - 0.2f;

            }

            AnimationRunner.EndAnimation(AnimationClip);


        }

        /// <summary>
        /// From the position provided the function will return the closest mesh surface on the entity
        /// </summary>
        /// <param name="Entity">Entity to compare mesh surface distance</param>
        /// <param name="Position">Position to compare which mesh surface is closest to it</param>
        /// <returns>The transform of the closest mesh surface found, if not found then the same position is returned</returns>
        public static Vector3 GetClosestSurfaceFromPosition(ABC_IEntity Entity, Vector3 Position) {

            MeshFilter entityMeshFilter = Entity.transform.GetComponentInChildren<MeshFilter>();

            if (entityMeshFilter == null)
                return Position;


            // convert position to local space
            Vector3 localPosition = Entity.transform.InverseTransformPoint(Position);

            float currentClosestDistance = 0f;
            Vector3 retVal = Vector3.zero;

            // scan all vertices to find nearest
            foreach (Vector3 vertex in entityMeshFilter.mesh.vertices) {


                if (currentClosestDistance == 0 || Vector3.Distance(localPosition, vertex) < currentClosestDistance) {
                    currentClosestDistance = Vector3.Distance(localPosition, vertex);
                    retVal = vertex;
                }
            }

            // convert nearest vertex back to world space
            retVal = Entity.transform.TransformPoint(retVal);

            //If distance is too big then the model has not got detailed vertices so we will just return the same position
            if (Vector3.Distance(retVal, Position) > 3)
                return Position;
            else
                return retVal;

        }


        /// <summary>
        /// From the position provided the function will return the closest bone on the entity
        /// </summary>
        /// <param name="Entity">Entity to compare bone distance</param>
        /// <param name="Position">Position to compare which bone is closest to it</param>
        /// <returns>The transform of the closest bone found, if not found then null is returned</returns>
        public static Transform GetClosestBoneFromPosition(ABC_IEntity Entity, Vector3 Position) {

            //If entity hasn't got state manager then finish here
            if (Entity.HasABCStateManager() == false)
                return null;

            //Store animator as we will be referencing it a lot
            Animator Ani = Entity.animator;

            //If not humanoid return null;
            if (Ani == null || Ani.isHuman == false)
                return null;

            //If entity doesn't have an animator then return null
            if (Ani == null)
                return null;

            Transform retVal = null;
            float currentClosestDistance = 0f;

            //Loop through every bone
            foreach (HumanBodyBones boneID in System.Enum.GetValues(typeof(HumanBodyBones))) {

                //If we reached last bone then continue (GetBoneTransform function errors if last bone enum inputted)
                if (ignoredBones.Contains((int)boneID))
                    continue;


                //Get bone from transform
                Transform bone = Ani.GetBoneTransform(boneID);

                //If no bone found then continue
                if (bone == null)
                    continue;

                //If first time storing (closest current distance is 0) or the new bone is closer then record it ready to be returned
                if (currentClosestDistance == 0f || Vector3.Distance(bone.position, Position) < currentClosestDistance) {
                    retVal = bone;
                    currentClosestDistance = Vector3.Distance(bone.position, Position);
                }

            }


            return retVal;

        }

        /// <summary>
        /// Returns how far up the entity is from the closest object below (jumping/in air)
        /// </summary>
        /// <param name="Entity">Entity's transform that distance from ground is being checked on</param>
        /// <returns>Float distance from ground</returns>
        public static float EntityDistanceFromGround(Transform Entity) {

            //Origin and direction
            Vector3 origin = Entity.position + new Vector3(0, 0.2f, 0);
            Vector3 direction = Vector3.down;

            // perform accurate raycast
            List<RaycastHit> hit = Physics.RaycastAll(origin, direction, 10f).Where(h => ABC_Utilities.IsStaticABCEntity(h.transform.gameObject) == false).ToList();

            if (hit.Count() > 0) {
                // hit is also in this if needed in future     
                return hit.OrderBy(x => x.distance).First().distance;
            } else {
                // return max distance that we check (though the entity is probably falling to the void so it doesn't matter)
                return 100f;
            }

        }

        /// <summary>
        /// Returns if the entity is grounded or not
        /// </summary>
        /// <param name="Entity">Entity which will be used to check if entity is grounded</param>
        /// <param name="OverlapCollector">Persistant array for overlap so one is not created each time</param>
        /// <returns></returns>
        public static bool EntityIsGrounded(ABC_IEntity Entity) {

            //If capsule collider doesn't exist then get distance from ground
            if (Entity.capsuleCollider == null) {

                if (EntityDistanceFromGround(Entity.transform) > 0.3f)
                    return false;
                else
                    return true;
            }

            //get the radius of the players capsule collider, and make it a tiny bit smaller than that
            float radius = Entity.capsuleCollider.radius * 0.9f;


            Vector3 positionLeeway = new Vector3(0, 0, 0);

#if ABC_GC_2_Integration
        //GC starts 1 vector up so give leeway 
        if (Entity.HasGC2CharacterComponent())
            positionLeeway = new Vector3(0, -1, 0);
       
#endif

            //get the position (assuming its right at the bottom) and move it up by almost the whole radius
            Vector3 pos = Entity.transform.position + positionLeeway + Vector3.up * (radius * 0.6f);

            //Overlap the sphere
            List<Collider> overlap = Physics.OverlapSphere(pos, radius).ToList();

            //IsGrounded if the overlap has a hit
            bool isGrounded = overlap.Where(h => h != null && ABC_Utilities.IsStaticABCEntity(h.transform.gameObject) == false && h.name.Contains("ABC*_") == false && h.gameObject != Entity.gameObject && h.transform.IsChildOf(Entity.transform) == false).Count() > 0;

            //return result
            return isGrounded;

        }

        /// <summary>
        /// Returns the first object hit (raycast) below the entities current position
        /// </summary>
        /// <param name="Entity">Entity which will be used to check for any objects below</param>
        /// <returns>First Gameobject found below the entities position if found, else null</returns>
        public static GameObject GetObjectBelowEntity(ABC_IEntity Entity) {

            //Cycle through all nearby entites 
            foreach (ABC_IEntity nearbyEntity in GetAllABCEntitiesInRange(new Vector3(Entity.transform.position.x, Entity.transform.position.y - 1.5f, Entity.transform.position.z), 2f, true)) {

                //Ignore self
                if (nearbyEntity == Entity)
                    continue;


                //If we are above the entity 
                if (Vector3.Dot(Entity.transform.position - nearbyEntity.transform.position, nearbyEntity.transform.up) > 0.06f && Vector3.Dot(Entity.transform.position - nearbyEntity.transform.position, nearbyEntity.transform.up) < 2.5) {


                    return nearbyEntity.gameObject;
                }

            }

            //else return null
            return null;

        }

        /// <summary>
        /// Will add an outline glow to the entity provided
        /// </summary>
        /// <param name="Entity">Gameobject to add the glow too</param>
        /// <param name="Enabled">True to add the outline glow, else false to remove it</param>
        /// <param name="Color">Color of the outline glow</param>
        public static void ToggleOutlineGlow(GameObject Obj, bool Enabled, Color Color) {

            //Check if component added
            ABC_OutlineGlowRenderer outlineGlow = Obj.gameObject.GetComponent<ABC_OutlineGlowRenderer>();

            if (Enabled == false && outlineGlow == null)
                return;

            //if not add component
            if (outlineGlow == null)
                outlineGlow = Obj.gameObject.AddComponent<ABC_OutlineGlowRenderer>();

            //Enable or disable the component updating the color
            outlineGlow.enabled = Enabled;
            outlineGlow.outlineColor = Color;

        }

        /// <summary>
        /// Will apply a new shader to the object provided, returning the old shader which was replaced.
        /// </summary>
        /// <param name="Obj">Object to apply shader too</param>
        /// <param name="Shader">Shader to apply</param>
        /// <returns>The old shader which was replaced by the new shader</returns>
        /// <remarks>Old shader is returned incase this is to be reapplied at a later date</remarks>
        public static Shader ApplyShader(GameObject Obj, Shader Shader) {

            //If either the object or shader is null then end here
            if (Obj == null || Shader == null)
                return null;

            //Collect the renderer
            Renderer objRenderer = Obj.GetComponentInChildren<Renderer>(true);

            if (objRenderer == null)
                return null;


            //Track the current shader to return
            Shader retVal = objRenderer.material.shader;

            //Apply the shader
            objRenderer.material.shader = Shader;

            return retVal;

        }


        /// <summary>
        /// Rolls a dice to generate a number. If the number is between the min and max then it will return true, else false
        /// </summary>
        /// <param name="Min">Minimum value the roll has to be above to return true</param>
        /// <param name="Max">Maximum value the roll has to be below to return true</param>
        /// <returns>True if the dice roll was between the min and max value, else false</returns>
        public static bool DiceRoll(float Min, float Max) {
            float dice = Random.Range(0f, 100f);

            if (dice >= Min && dice <= Max) {
                return true;
            } else {
                return false;
            }


        }

        /// <summary>
        /// Will determine what X% of a value is
        /// </summary>
        /// <param name="Percentage">Percentage of X i.e 50% of value</param>
        /// <param name="Value">Value to work out the percentage of</param>
        /// <returns>Float value resulting in the % of the value provided</returns>
        public static float PercentageOfValue(float Percentage, float Value) {

            return Value / 100 * Percentage;

        }

        /// <summary>
        /// Will increase or decrease the value depending on the percentage provided 
        /// i.e 100% is normal speed so no changes, 50% is slower so value is increased by 50% and 150% is faster so value is decreased by 50%
        /// </summary>
        /// <param name="Percentage">Percentage of X i.e 50% of value</param>
        /// <param name="Value">Value to work out the percentage of</param>
        /// <returns>Float value resulting in the % of the value provided</returns>
        public static float ModifyTimeByPercentage(float Percentage, float Value) {

            float retVal = Value;

            //If percentage is 100% then no changes required
            if (Percentage == 0 || Percentage == 100 || Value == 0)
                return retVal;

            retVal = (100 / Percentage) * Value;

            return retVal;

        }

        /// <summary>
        /// Will modify the game speed for the duration given
        /// </summary>
        /// <param name="SpeedFactor">Value to change the game speed to (lower the number the slower the speed)</param>
        /// <param name="Duration">How long to modify the game speed for</param>
        /// <param name="Delay">Delay before the game speed is modified</param>
        public static IEnumerator ModifyGameSpeed(float SpeedFactor, float Duration, float Delay = 0f) {

            //If we are already in the middle of a slow motion then end here 
            if (Time.timeScale != 1f)
                yield break;

            //wait for any delay
            if (Delay > 0f)
                yield return new WaitForSeconds(Delay);

            if (SpeedFactor < 0)
                SpeedFactor = 0;

            Time.timeScale = SpeedFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;

            while (Time.timeScale != 1f) {
                yield return new WaitForEndOfFrame();

                //If we slowed down the game time then gradually increase the speed till we get to 1
                if (Time.timeScale < 1f) {
                    Time.timeScale += (1f / Duration) * Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
                } else {
                    //If we have sped the game up then gradually decrease speed till we get to 1
                    Time.timeScale -= (1f / Duration) * Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Max(Time.timeScale, 1f);
                }

                Time.fixedDeltaTime = Time.timeScale * 0.02f;


            }


        }

        #endregion


        // ********************* Asset Integration - Editor Only Code ********************


#if UNITY_EDITOR


        /// <summary>
        /// Returns a bool indicating if the define symbol has been added to the project
        /// </summary>
        /// <param name="Define">Name of the scripting define symbol i.e ABC_GC_Integration</param>
        /// <returns>True if the define symbol exists, else false</returns>
        public static bool IntegrationDefineSymbolExists(string Define) {

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = defines.Split(';').ToList();

            if (allDefines.Contains(Define))
                return true;
            else
                return false;

        }

        /// <summary>
        /// Add scripting define symbols for integration - if correct symbol is added then code for integrations will compile
        /// else it is ignored
        /// </summary>
        /// <param name="Define">Name of the scripting define symbol i.e ABC_GC_Integration</param>
        public static void AddIntegrationDefineSymbols(string Define) {

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = defines.Split(';').ToList();

            if (allDefines.Contains(Define)) {
                Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + Define + "</b> <i>Scripting Define Symbol</i>.");
                return;
            }

            allDefines.Add(Define);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));

            Debug.LogWarning("<b>" + Define + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");

        }

        /// <summary>
        /// remove scripting define symbols for integration - if correct symbol is added then code for integrations will compile
        /// else it is ignored
        /// </summary>
        /// <param name="Define">Name of the scripting define symbol i.e ABC_GC_Integration</param>
        public static void RemoveIntegrationDefineSymbols(string Define) {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = defines.Split(';').ToList();

            if (allDefines.Contains(Define)) {
                allDefines.Remove(Define);
            } else {
                //Wasn't added already so can just end here
                return;
            }


            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));

            Debug.LogWarning("<b>" + Define + "</b> removed from <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
        }

        public static void CreateFolderStructure(string path) {
            string[] pathSplit = path.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            string stackPath = pathSplit[0];

            for (int i = 1; i < pathSplit.Length; ++i) {
                string thisFolder = pathSplit[i];
                string indexPath = Path.Combine(stackPath, thisFolder);
                if (!AssetDatabase.IsValidFolder(indexPath)) {
                    string guid = AssetDatabase.CreateFolder(stackPath, thisFolder);
                    stackPath = AssetDatabase.GUIDToAssetPath(guid);
                } else {
                    stackPath = Path.Combine(stackPath, thisFolder);
                }
            }
        }

        /// <summary>
        /// Will convert global elements to work with GC 2 out of the box
        /// </summary>
        public static void ConvertGlobalElementsForGC2() {

#if ABC_GC_2_Stats_Integration
        if (EditorUtility.DisplayDialog("GC 2 Conversion?", "This will overwrite ABC presets, Are you sure you want to continue?", "Yes", "No")) {

            Dictionary<ABC_GlobalElement, string> ExportedElements = new Dictionary<ABC_GlobalElement, string>();

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ABC_GlobalElement).Name);
            ABC_GlobalElement[] a = new ABC_GlobalElement[guids.Length];
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<ABC_GlobalElement>(path);

                if (a[i].elementType == ABC_GlobalElement.GlobalElementType.Effect) {
                    ExportedElements.Add(a[i], path);
                }

            }

            //Find all effects
            foreach (KeyValuePair<ABC_GlobalElement, string> expElement in ExportedElements.Where(e => e.Key.elementType == ABC_GlobalElement.GlobalElementType.Effect)) {

                //Copy to new object 
                ABC_GlobalElement newGlobalElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(expElement.Key), newGlobalElement);

                //Modify new object
                foreach (Effect element in newGlobalElement.ElementEffects) {

                    //Change adjust stat values
                    if (element.effectName == "AdjustStatValue") {

                        element.effectName = "AdjustGC2StatValue";

                        switch (element.miscellaneousProperty) {
                            case "Strength":
                                element.miscellaneousProperty = "strength";
                                break;
                            case "Defence":
                                element.miscellaneousProperty = "constitution";
                                break;
                            case "Magic":
                                element.miscellaneousProperty = "wisdom";
                                break;
                        }

                        element.miscellaneousAltProperty = "True";

                    }

                    //Change potency stat modifications
                    for (int i = 0; i < element.potencyStatModifications.Count; i++) {
                        Effect.PotencyStatModifications statMod = element.potencyStatModifications[i];

                        statMod.statIntegrationType = ABCIntegrationType.GameCreator2;

                        switch (statMod.statName) {
                            case "Strength":
                                statMod.statName = "strength";
                                break;
                            case "Defence":
                                statMod.statName = "constitution";
                                break;
                            case "Magic":
                                statMod.statName = "will";
                                break;
                        }
                    }
                }

                //Overwrite effect with GC 2 changes from new object
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(newGlobalElement), expElement.Key);
                //Set dirty to save the changes
                EditorUtility.SetDirty(expElement.Key);
            }

        }
#endif

        }

#endif
    }





    // ********************* Shared ENUMS ********************
    #region Shared ENUMS


    //What ability is doing - the event 
    public enum AbilityEvent {
        Activation,
        Collision
    }


    // only used in ability class
    public enum AbilityPositionGraphicType {
        SpawnObject,
        End
    }




    public enum AbilityType {
        Projectile = 0,
        RayCast = 1,
        Melee = 2
    }


    public enum InputType {
        Key = 0,
        Button = 1
    }

    public enum TriggerType {
        Input = 0,
        InputCombo = 1
    }


    public enum AnimationMethod {
        ABCAnimationRunner = 0,
        Animator = 1
    }


    public enum AnimatorParameterType {
        Float = 0,
        integer = 1,
        Bool = 2,
        Trigger = 3
    }



    public enum TravelType {
        SelectedTarget = 1,
        Self = 2,
        ToWorld = 3, // fires to ground in world (maybe rename - think TSW ground target spells) 
        MouseTarget = 9, // uses selected target to travel to mouse position (without forcing character to rotate like mouse forward)
        Crosshair = 6, // fires to crosshair position setup on screen
        NearestTag = 10, // uses selected target to travel to the nearest tag within a range 
        NoTravel = 5,
        Forward = 0,
        MouseForward = 7, // rotates to mouse position and fires forward (like forward)
        Mouse2D = 4,
        CustomScript = 8 // custom script made by user 

    }



    // Starting postion where particle will spawn 
    public enum StartingPosition {
        Self = 0,
        Target = 1,
        OnObject = 2,
        OnWorld = 3,
        CameraCenter = 4,
        OnTag = 5,
        OnSelfTag = 6
    }


    // how do spells act when collidng with other spells
    public enum AbilityCollisionIgnores {
        IgnoreAll = 0,
        IgnoreForeignAbilities = 1,
        IgnoreSelfAbilities = 2,
        IgnoreNone = 3,
        AlwaysCollideAll = 4,
        AlwaysCollideForeignAbilities = 5,
        AlwaysCollideSelfAbilities = 6
    }

    // on Impact do we destroy
    public enum ImpactDestroy {
        DestroyOnAll = 0,
        DestroyOnABCProjectiles = 1,
        DestroyOnABCStateManagers = 2,
        DestroyOnAllABC = 3,
        DestroyOnAllNotABC = 4,
        DestroyOnTargetOnly = 5,
        DestroyOnTerrainOnly = 6,
        DontDestroy = 7,
        DestroyOnAllNotABCProjectile = 8
    }



    public enum TargetSelectType {
        None = 0,
        Mouse = 1,
        Crosshair = 2
    }

    public enum TargetType {
        Target,
        Mouse,
        World
    }

    // type of bounce targeting 
    public enum BounceTarget {
        NearestABCStateManager = 0,
        NearestObject = 1,
        NearestTag = 2
    }


    public enum LoggingType {


        ReadyToCastAgain = 0,
        Range = 1,
        FacingTarget = 2,
        FpsSelection = 3,
        TargetSelection = 4,
        SoftTargetSelection = 5,
        WorldSelection = 6,
        AbilityActivationError = 7,
        NoMana = 8,
        Preparing = 9,
        AbilityInterruption = 10,
        Initiating = 11,
        AbilityActivation = 12,
        ScrollAbilityEquip = 13,
        AmmoInformation = 14,
        BlockingInformation = 15,
        WeaponInformation = 16,
        ParryInformation = 17

    }


    public enum CollisionType {
        OnStay,
        OnExit,
        OnEnter,
        Particle,
        None
    }


    //integrations 

    public enum ABCIntegrationType {
        ABC = 0,
        GameCreator = 1,
        EmeraldAI = 2,
        GameCreator2
    }

    //Game Creator Integration
    public enum GCStatType {
        Stat = 1,
        Attribute = 2
    }

    #endregion
}