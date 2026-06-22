using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.AI;


namespace ABCToolkit {
    /// <summary>
    /// Component which contains the list of abilities setup for the entity. Will handle selecting targets and will activate abilities. 
    /// </summary>
    [System.Serializable]
    public class ABC_Controller : MonoBehaviour {

        // ********************** Nested Classes ****************

        #region Nested Classes

        [System.Serializable]
        public class AnimatorClipRunnerOverride {

            /// <summary>
            /// List of animator clips to override with the animation runner clip defined
            /// </summary>
            public List<string> animatorClipNamesToOverride = new List<string>();

            /// <summary>
            /// If true then to override a clip the name of the clip has to match exactly
            /// </summary>
            [Tooltip("If true then to override a clip the name of the clip has to match exactly")]
            public bool exactNameRequired = false; 

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference animationRunnerClip;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float animationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float animationRunnerClipDelay = 0f;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference animationRunnerMask = null;


            /// <summary>
            /// If true then the animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate  animation on the main entities animation runner")]
            public bool animationRunnerOnEntity = true;

            /// <summary>
            /// If true then the  animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate  animation on the current scroll ability's graphic animation runner")]
            public bool animationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool animationRunnerOnWeapon = false;

        }

        /// <summary>
        /// Configures weapons to be used during play, a weapon can have multiple graphics and positions (for dual wielding). Will define
        /// the animation to play when enabling and disabling and what abilities or ability groups will be enabled/disabled when weapon is equipped
        /// </summary>
        [System.Serializable]
        public class Weapon {

            // ************ Settings *****************************

            #region Settings Weapons 

            /// <summary>
            /// Links to a global weapon which will activate instead of the properties in this weapon
            /// </summary>
            [Tooltip("Links to a global weapon which will activate instead of the properties in this weapon")]
            public ABC_GlobalElement globalWeapon = null;

            /// <summary>
            /// The global element this weapon was created from during play (if this weapon was made from a global element)
            /// </summary>
            [Tooltip("The global element this weapon was created from during play (if this weapon was made from a global element)")]
            public ABC_GlobalElement globalElementSource = null;

            /// <summary>
            /// Name of the weapon
            /// </summary>
            [Tooltip("Name of the weapon")]
            public string weaponName;

            /// <summary>
            /// ID of the weapon
            /// </summary>
            [Tooltip("ID of the weapon")]
            public int weaponID;

            /// <summary>
            /// Description of the weapon
            /// </summary>
            [Tooltip("Description of the weapon")]
            public string weaponDescription = " ";


            /// <summary>
            /// weapon Icon which can be displayed on GUIs 
            /// </summary>
            [Tooltip("Icon image of the weapon")]
            public ABC_Texture2DReference weaponIconImage;

            /// <summary>
            /// How much speed is adjusted by when weapon is equipped
            /// </summary>
            [Tooltip("How much speed is adjusted by when weapon is equipped")]
            public float weaponMoveSpeedModification = 0;


            //Used for inspector only right now - will equip the weapon
            public bool inspectorEquipWeapon = false;


            /// <summary>
            /// Used for global weapons, can pick to override the enable status locally
            /// </summary>
            [Tooltip("Used for global weapons, can pick to override the enable status locally")]
            public bool globalWeaponOverrideEnableStatus = false;

            /// <summary>
            /// Determines if the weapon is enabled or not
            /// </summary>
            [Tooltip("Determines if the weapon is enabled or not")]
            public bool weaponEnabled = true;


            /// <summary>
            /// If true then weapon can be assigned to  Groups
            /// </summary>
            [Tooltip("Determines if this weapon can be assigned to Groups IDs of the groups this ability belongs to")]
            public bool allowWeaponGroupAssignment = false;

            /// <summary>
            /// The IDs of the groups this weapon is assigned too
            /// </summary>
            [Tooltip("The IDs of the groups this weapon belongs to")]
            public List<int> assignedGroupIDs = new List<int>();

            /// <summary>
            /// The names of the groups this weapon is assigned too
            /// </summary>
            [Tooltip("The names of the groups this weapon belongs to")]
            public List<string> assignedGroupNames = new List<string>();


            /// <summary>
            /// If true then abilities will be enabled when this weapon is equipped
            /// </summary>
            [Tooltip("If true then abilities will be enabled when this weapon is equipped")]
            public bool enableAbilitiesWhenEquipped = false;

            /// <summary>
            /// The IDs of the abilities this weapon will enable
            /// </summary>
            [Tooltip("the IDs of the abilities this weapon will enable")]
            public List<int> enableAbilityIDs = new List<int>();

            /// <summary>
            /// list of ID's or names of abilities which are not attached to weapon but if exist on the entity will be enabled when the weapon is equipped
            /// </summary>
            [Tooltip(" list of ID's or names of abilities which are not attached to weapon but if exist on the entity will be enabled when the weapon is equipped")]
            public List<string> enableAbilityStrings = new List<string>();

            /// <summary>
            /// If true then once equipped then all other abilities not listed to be enabled, will be disabled
            /// </summary>
            [Tooltip("If true then once equipped then all other abilities not listed to be enabled, will be disabled")]
            public bool disableAllOtherAbilitiesNotListed = false;

            /// <summary>
            /// If true then once equipped then all abilities not listed and linked to other weapons will be disabled
            /// </summary>
            [Tooltip(" If true then once equipped then all abilities not listed and linked to other weapons will be disabled")]
            public bool disableAllAbilitiesLinkedToOtherWeapons = true;


            /// <summary>
            /// If true then abilites will be disabled when this weapon is equipped
            /// </summary>
            [Tooltip("If true then abilites will be disabled when this weapon is equipped")]
            public bool disableAbilitiesWhenEquipped = false;

            /// <summary>
            /// The IDs of the abilities this weapon will disable
            /// </summary>
            [Tooltip("The IDs of the abilities this weapon will disable")]
            public List<int> disableAbilityIDs = new List<int>();

            /// <summary>
            /// list of ID's or names of abilities which are not attached to this weapon but if exist on the entity will be disabled when weapon is equipped
            /// </summary>
            [Tooltip("list of ID's or names of abilities which are not attached to this weapon but if exist on the entity will be disabled when weapon is equipped")]
            public List<string> disableAbilityStrings = new List<string>();

            /// <summary>
            /// If true then once equipped then all other abilities not listed to be disabled, will be enabled
            /// </summary>
            [Tooltip("If true then once equipped then all other abilities not listed to be disabled, will be enabled")]
            public bool enableAllOtherAbilitiesNotListed = false;

            /// <summary>
            /// If true then ability groups will be enabled when this weapon is equipped
            /// </summary>
            [Tooltip(" If true then ability groups will be enabled when this weapon is equipped")]
            public bool enableAbilityGroupsWhenEquipped = false;

            /// <summary>
            /// The IDs of the groups this weapon will enable
            /// </summary>
            [Tooltip("The IDs of the groups this weapon will enable")]
            public List<int> enableAbilityGroupIDs = new List<int>();

            /// <summary>
            /// The names of the groups this weapon is assigned too
            /// </summary>
            [Tooltip("The names of the groups this weapon belongs to")]
            public List<string> enableAbilityGroupNames = new List<string>();

            /// <summary>
            /// If true then once equipped then all other ability groups not listed to be enabled, will be disabled
            /// </summary>
            [Tooltip("If true then once equipped then all other ability groups not listed to be enabled, will be disabled")]
            public bool disableAllOtherGroupsNotListed = false;


            /// <summary>
            /// If true then ability groups will be disabled when this weapon is equipped
            /// </summary>
            [Tooltip("If true then ability groups will be disabled when this weapon is equipped")]
            public bool disableAbilityGroupWhenEquipped = false;

            /// <summary>
            /// The IDs of the groups this weapon will disable
            /// </summary>
            [Tooltip("The IDs of the groups this weapon will disable")]
            public List<int> disableAbilityGroupIDs = new List<int>();

            /// <summary>
            /// The names of the groups this weapon will disable
            /// </summary>
            [Tooltip("The names of the groups this weapon will disable")]
            public List<string> disableAbilityGroupNames = new List<string>();

            /// <summary>
            /// If true then once equipped then all other ability groups not listed to be disabled, will be enabled
            /// </summary>
            [Tooltip("If true then once equipped then all other ability groups not listed to be disabled, will be enabled")]
            public bool enableAllOtherGroupsNotListed = false;


            /// <summary>
            /// If true then gameobjects can be defined which will be enabled when weapon is equipped and disabled when weapon is unequipped
            /// </summary>
            [Tooltip("If true then gameobjects can be defined which will be enabled when weapon is equipped and disabled when weapon is unequipped")]
            public bool weaponToggleObjectsOnEquipStateChange = false;

            /// <summary>
            /// A list of object namess which will be enabled when weapon is equipped and disabled when weapon is unequipped
            /// </summary>
            [Tooltip("A list of object names which will be enabled when weapon is equipped and disabled when weapon is unequipped")]
            public List<string> weaponToggleGameObjectNames = new List<string>();




            /// <summary>
            /// If true then movement will temporarily be disabled when equipping/unequipping weapon
            /// </summary>
            [Tooltip("If true then movement will temporarily be disabled when equipping/unequipping weapon")]
            public bool weaponSwitchTemporarilyDisableMovement = true;

            /// <summary>
            /// If true then entity will stop moving due to the position being frozen
            /// </summary>
            [Tooltip("If true then  entity will stop moving due to the position being frozen")]
            public bool weaponSwitchDisableMovementFreezePosition = false;

            /// <summary>
            /// If true then  entity will stop moving due to movement components being disabled
            /// </summary>
            [Tooltip("If true then entity will stop moving due to movement components being disabled")]
            public bool weaponSwitchDisableMovementDisableComponents = true;

            /// <summary>
            /// If true then ability activation will temporarily disabled when equipping/unequipping weapon
            /// </summary>
            [Tooltip("If true then ability activation will temporarily disabled when equipping/unequipping weapon")]
            public bool weaponSwitchTemporarilyDisableAbilityActivation = true;

            /// <summary>
            /// How long to disable ability activation when equipping/unequipping
            /// </summary>
            [Tooltip("How long to disable ability activation when equipping/unequipping")]
            public float disableAbilityActivationDuration = 1f;

            /// <summary>
            /// Determines how long it takes to equip and enable the weapon
            /// </summary>
            [Tooltip("Determines how long it takes to equip and enable the weapont")]
            public float enableDuration = 1f;

            /// <summary>
            /// Determines how long it takes to de-equip and disable the weapon
            /// </summary>
            [Tooltip("Determines how long it takes to de-equip and disable the weapon")]
            public float disableDuration = 1f;



            /// <summary>
            /// If true then the weapon can be equipped through cycling through weapons
            /// </summary>
            public bool enableWeaponOnCycle = true;

            /// <summary>
            /// If true then the weapon will be enabled on key/button press
            /// </summary>
            [Tooltip("If true then the weapon will be enabled on key/button press")]
            public bool enableWeaponOnInput = false;

            /// <summary>
            /// Type of input to enable the weapon 
            /// </summary>
            [Tooltip("type of input to enable the weapon")]
            public InputType weaponEnableInputType;

            /// <summary>
            /// Button name to enable the weapon
            /// </summary>
            [Tooltip("The Button name to enable the weapon")]
            public string weaponEnableButton;

            /// <summary>
            /// Key to enable the weapon
            /// </summary>
            [Tooltip("What button to press to enable the weapon")]
            public KeyCode weaponEnableKey;

            /// <summary>
            /// Defines the weapon objects for this 'weapon'. The weapon obj will hold information like the graphic to show 
            /// and the position it's in when enabled or disabled 
            /// </summary>
            [System.Serializable]
            public class WeaponObj {

                // ************ Settings *****************************

                #region Settings WeaponObj


                /// <summary>
                /// Used by inspector only - determines if the effect settings are collapsed out or not 
                /// </summary>
                [Tooltip("Used by inspector only - determines if the effect settings are collapsed out or not ")]
                public bool foldOut = false;

                /// <summary>
                /// The main weapon graphic object
                /// </summary>
                [Tooltip("The main weapon graphic object")]
                public ABC_GameObjectReference weaponObjMainGraphic;

                /// <summary>
                /// the graphic object which will become a child of the main graphic
                /// </summary>
                [Tooltip("the graphic object which will become a child of the main graphic")]
                public ABC_GameObjectReference weaponObjSubGraphic;

                /// <summary>
                /// The delay before the weapon graphic appears
                /// </summary>
                [Tooltip("The delay before the weapon graphic appears")]
                public float equipDelay;

                /// <summary>
                /// The position type for the weapon obj graphic when enabled
                /// </summary>
                [Tooltip("The position type for the weapon obj graphic when enabled")]
                public WeaponPositionType weaponObjEnabledPositionType;

                /// <summary>
                /// used for weapon starting position if weapon is enabled and OnObject is selected. Determines what object the weapon will be attached too
                /// </summary>
                [Tooltip("used for weapon starting position if weapon is enabled and OnObject is selected. Determines what object the weapon will be attached too")]
                public ABC_GameObjectReference weaponEnabledPositionOnObject;


                /// <summary>
                /// Tag which the weapon will attach to when Enabled. Does not work for ABC tags. 
                /// </summary>
                [Tooltip("Tag which the weapon will attach to when Enabled. Does not work for ABC tags. ")]
                public string weaponEnabledPositionOnTag;


                /// <summary>
                /// Offset of the weapon Enabled position
                /// </summary>
                [Tooltip("Offset of the weapon Enabled position")]
                public Vector3 weaponEnabledPositionOffset;



                /// <summary>
                /// Determines the direction of the weapon when Enabled
                /// </summary>
                [Tooltip("Determines the direction of the weapon when Enabled")]
                public Vector3 weaponEnabledStartingRotation;


                /// <summary>
                /// If true then the euler rotation of the weapon will be set to the starting points euler rotation
                /// </summary>
                [Tooltip("If true then the euler rotation of the ability will be set to the starting points euler rotation")]
                public bool weaponEnabledSetEulerRotation;


                /// <summary>
                /// The delay before the weapon graphic is disabled
                /// </summary>
                [Tooltip("The delay before the weapon graphic is disabled")]
                public float unequipDelay;

                /// <summary>
                /// If enabled then the weapon will always show when disabled, else it will dissapear when disabled
                /// </summary>
                [Tooltip("If enabled then the weapon will always show when disabled, else it will dissapear when disabled")]
                public bool showWeaponWhenDisabled;

                /// <summary>
                /// The position type for the weapon object when disabled and always showing
                /// </summary>
                [Tooltip("The position type for the weapon object when disabled and always showing")]
                public WeaponPositionType weaponObjDisabledPositionType;

                /// <summary>
                /// used for weapon starting position if weapon is disabled and OnObject is selected. Determines what object the weapon will be attached too
                /// </summary>
                [Tooltip("used for weapon starting position if weapon is disabled and OnObject is selected. Determines what object the weapon will be attached too")]
                public ABC_GameObjectReference weaponDisabledPositionOnObject;


                /// <summary>
                /// Tag which the weapon will attach to when Disabled. Does not work for ABC tags. 
                /// </summary>
                [Tooltip("Tag which the weapon will attach to when Disabled. Does not work for ABC tags. ")]
                public string weaponDisabledPositionOnTag;


                /// <summary>
                /// Offset of the weapon Disabled position
                /// </summary>
                [Tooltip("Offset of the weapon Disabled position")]
                public Vector3 weaponDisabledPositionOffset;


                /// <summary>
                /// Determines the direction of the weapon when Disabled
                /// </summary>
                [Tooltip("Determines the direction of the weapon when Disabled")]
                public Vector3 weaponDisabledStartingRotation;


                /// <summary>
                /// If true then the euler rotation of the weapon will be set to the starting points euler rotation
                /// </summary>
                [Tooltip("If true then the euler rotation of the weapon will be set to the starting points euler rotation")]
                public bool weaponDisabledSetEulerRotation;

                /// <summary>
                /// If enabled then a weapon trail can be configured for the weapon
                /// </summary>
                [Tooltip("If enabled then a weapon trail can be configured for the weapon")]
                public bool useWeaponTrail = false;

                /// <summary>
                /// Material for the weapon trail
                /// </summary>
                [Tooltip("Material for the weapon trail")]
                public ABC_MaterialReference weaponTrailMaterial;

                /// <summary>
                /// used for weapon trail, determines the base of the trail
                /// </summary>
                [Tooltip("used for weapon trail, determines the base of the trail")]
                public string weaponTrailBaseTag = "ABC/WeaponBase";

                /// <summary>
                /// used for weapon trail, determines the tip of the trail
                /// </summary>
                [Tooltip("used for weapon trail, determines the tip of the trail")]
                public string weaponTrailTipTag = "ABC/WeaponTip";

                /// <summary>
                /// List of colours that will render on the trail
                /// </summary>
                [Tooltip("List of colours that will render on the trail")]
                public List<Color> weaponTrailColors = new List<Color>();

                #endregion


                // ************ Variables *****************************

                #region Variables For Weapons

                /// <summary>
                ///The weapon pool storing the weapon graphic
                /// </summary>
                private GameObject weaponPool;

                /// <summary>
                /// Keeps track of the current scroll ability Aesthetic animator 
                /// </summary>
                private Animator weaponObjAnimator = null;

                /// <summary>
                /// Keeps track of the current scroll ability Aesthetic ABC Animation Runner 
                /// </summary>
                private ABC_AnimationsRunner weaponObjAnimationRunner = null;

                /// <summary>
                /// Variable to hold weapon trail component
                /// </summary>
                private ABC_WeaponTrail weaponTrail = null;

                #endregion



                // ************ Public Methods *****************************

                #region Public Methods 

                /// <summary>
                /// Constructor mainly used by inspectors
                /// </summary>
                public WeaponObj() { }

                /// <summary>
                /// Will create weapon enable graphics setup for the weapon
                /// </summary>
                /// <param name="CreateOne">If true then only one  graphic will be created and returned but not pool'd</param>
                public GameObject CreateGraphicObject(bool CreateOne = false) {

                    //If the weapon has already been created then return that 
                    if (this.weaponPool != null && CreateOne == false)
                        return this.weaponPool;

                    GameObject weaponObj = null;


                    if (this.weaponObjMainGraphic == null)
                        return null;

                    //how many objects to make
                    float objCount = 1;


                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        weaponObj = (GameObject)(GameObject.Instantiate(this.weaponObjMainGraphic.GameObject));
                        weaponObj.name = this.weaponObjMainGraphic.GameObject.name;

                        if (this.useWeaponTrail == true) {
                            this.weaponTrail = weaponObj.AddComponent<ABC_WeaponTrail>();
                            this.weaponTrail.trailMaterial = this.weaponTrailMaterial.Material;
                            this.weaponTrail.weaponBase = weaponObj.GetComponentsInChildren<Transform>(true).Where(t => t.tag == this.weaponTrailBaseTag)?.FirstOrDefault();
                            this.weaponTrail.weaponTip = weaponObj.GetComponentsInChildren<Transform>(true).Where(t => t.tag == this.weaponTrailTipTag)?.FirstOrDefault();
                            this.weaponTrail.trailColours = this.weaponTrailColors;

                        }

                        // copy child object for additional Aesthetic 
                        if (this.weaponObjSubGraphic.GameObject != null) {
                            GameObject weaponChildObj = (GameObject)(GameObject.Instantiate(this.weaponObjSubGraphic.GameObject));
                            weaponChildObj.name = this.weaponObjSubGraphic.GameObject.name;
                            weaponChildObj.transform.position = weaponObj.transform.position;
                            weaponChildObj.transform.rotation = weaponObj.transform.rotation;
                            weaponChildObj.transform.parent = weaponObj.transform;
                        }


                        //If we are only creating one then don't pool
                        if (CreateOne == false) {
                            //disable and pool the object 
                            ABC_Utilities.PoolObject(weaponObj);

                            // add to tracker. 
                            this.weaponPool = weaponObj;
                        }
                    }



                    return weaponObj;

                }

                /// <summary>
                /// Will destroy the graphic object
                /// </summary>
                public void ClearGraphicObject() {

                    if (this.weaponPool != null) {
                        Destroy(this.weaponPool);
                        this.weaponPool = null;
                    }
                }

                /// <summary>
                /// Will disable the graphic object placing it back in the pool
                /// </summary>
                public void DisableGraphicObject() {
                    //disable and pool the object                

                    if (this.weaponPool != null)
                        ABC_Utilities.PoolObject(this.weaponPool);
                }


                /// <summary>
                /// enable/disable graphics for the weapon, placing it in the right position.
                /// </summary>
                /// <param name="GraphicType">Type of graphic to activate</param>
                /// <param name="QuickToggle">If true then graphic will appear instantly</param>
                /// <returns>Will return the graphic gameobject which has been created</returns>
                public IEnumerator ToggleGraphic(ABC_IEntity Originator, WeaponState WeaponToggleType, bool QuickToggle = false) {

                    //Variables to be used later
                    WeaponPositionType startingPosition = WeaponPositionType.Self;
                    GameObject positionOnObject = null;
                    string positionOnTag = null;

                    Vector3 positionOffset = new Vector3(0, 0, 0);


                    bool setEulerRotation = false;
                    Vector3 rotation = new Vector3(0, 0, 0);

                    float delay = 0f;
                    GameObject graphicObj = null;

                    //Depending on if we are enabling or disabling fill out the variables
                    switch (WeaponToggleType) {

                        case WeaponState.Equip:

                            startingPosition = this.weaponObjEnabledPositionType;
                            positionOnObject = this.weaponEnabledPositionOnObject.GameObject;
                            positionOnTag = this.weaponEnabledPositionOnTag;

                            positionOffset = this.weaponEnabledPositionOffset;

                            setEulerRotation = this.weaponEnabledSetEulerRotation;
                            rotation = this.weaponEnabledStartingRotation;

                            delay = this.equipDelay;

                            graphicObj = this.weaponPool;

                            if (graphicObj == null)
                                graphicObj = this.CreateGraphicObject();


                            break;

                        case WeaponState.UnEquip:

                            startingPosition = this.weaponObjDisabledPositionType;
                            positionOnObject = this.weaponDisabledPositionOnObject.GameObject;
                            positionOnTag = this.weaponDisabledPositionOnTag;

                            positionOffset = this.weaponDisabledPositionOffset;

                            setEulerRotation = this.weaponDisabledSetEulerRotation;
                            rotation = this.weaponDisabledStartingRotation;


                            delay = this.unequipDelay;

                            graphicObj = this.weaponPool;

                            if (graphicObj == null)
                                graphicObj = this.CreateGraphicObject();


                            break;
                    }



                    //if delay is greater then 0 then wait unless quick toggle is true
                    if (delay > 0f && QuickToggle == false)
                        yield return new WaitForSeconds(delay);

                    //initial starting point is the entity incase anything goes wrong
                    Transform meTransform = Originator.transform;
                    Vector3 position = meTransform.position;
                    //record values which might be used
                    GameObject parentObject = Originator.gameObject;

                    // get starting position 
                    switch (startingPosition) {

                        case WeaponPositionType.Self:
                            position = Vector3.zero + positionOffset;
                            parentObject = Originator.gameObject;
                            break;
                        case WeaponPositionType.OnObject:
                            if (positionOnObject != null) {
                                Transform onObjectTransform = positionOnObject.transform;
                                position = Vector3.zero + positionOffset;
                                parentObject = onObjectTransform.gameObject;
                            }
                            break;
                        case WeaponPositionType.OnTag:
                            GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                            if (onTagObj != null) {
                                Transform onTagTransform = onTagObj.transform;
                                position = Vector3.zero + positionOffset;
                                parentObject = onTagTransform.gameObject;
                            }
                            break;
                        case WeaponPositionType.OnSelfTag:
                            GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(Originator, positionOnTag);
                            if (onSelfTagObj != null) {
                                Transform onSelfTagTransform = onSelfTagObj.transform;
                                position = Vector3.zero + positionOffset;
                                parentObject = onSelfTagTransform.gameObject;
                            }
                            break;
                        default:
                            Originator.AddToDiagnosticLog("Error: starting position for weapon " + startingPosition.ToString() + "  graphic was not correctly determined.");
                            break;
                    }



                    // set position and parent
                    graphicObj.transform.parent = parentObject.transform;

                    graphicObj.transform.localPosition = position;
                    graphicObj.transform.localRotation = Quaternion.identity * Quaternion.Euler(rotation);


                    //If set to then copy euler angles also
                    if (setEulerRotation)
                        graphicObj.transform.localEulerAngles = parentObject.transform.localEulerAngles + rotation;

                    // set it true 
                    graphicObj.SetActive(true);

                    // if we are disabling the graphic and not showing weapon when disabled then repool the object 
                    if (WeaponToggleType == WeaponState.UnEquip && this.showWeaponWhenDisabled == false) {
                        ABC_Utilities.PoolObject(graphicObj);
                    }


                }


                /// <summary>
                /// Will return the weapon object in the pool
                /// </summary>
                /// <returns>Gameobject of the weapon pool</returns>
                public GameObject GetWeaponPoolObject() {

                    return this.weaponPool;
                }

                /// <summary>
                /// Will return the ABC Animation runner component for the weapon graphic obj
                /// </summary>
                /// <returns>ABC Animation Runner Component</returns>
                public ABC_AnimationsRunner GetAnimationRunner() {

                    //try and recieve animation runner if null
                    if (this.weaponObjAnimationRunner == null && this.weaponPool != null)
                        this.weaponObjAnimationRunner = this.weaponPool.GetComponentInChildren<ABC_AnimationsRunner>();

                    //If we have an animator but no runner then add it
                    if (this.GetAnimator() != null && this.weaponPool != null && this.weaponObjAnimationRunner == null)
                        this.weaponObjAnimationRunner = this.weaponPool.AddComponent<ABC_AnimationsRunner>();


                    //If we now have an animation runner for this weapon then return that 
                    if (this.weaponObjAnimationRunner != null)
                        return this.weaponObjAnimationRunner;

                    return null;

                }

                /// <summary>
                /// Will return the animator component for the weapon graphic object 
                /// </summary>
                /// <returns>Animator Component</returns>
                public Animator GetAnimator() {

                    //Try and get the animator component
                    if (this.weaponObjAnimator == null && this.weaponPool != null)
                        this.weaponObjAnimator = this.weaponPool.GetComponentInChildren<Animator>();

                    //If an animator for this weapon exists then return that 
                    if (this.weaponObjAnimator != null)
                        return this.weaponObjAnimator;


                    return null;

                }

                /// <summary>
                /// Will return the weapon trial for the weapon graphic
                /// </summary>
                /// <returns></returns>
                public ABC_WeaponTrail GetWeaponTrail() {
                    return this.weaponTrail;
                }

                #endregion


            }






            /// <summary>
            /// List of weapon objects/graphics which will show when this weapon is enabled
            /// </summary>
            [Tooltip("List of weapon objects/graphics which will show when this weapon is enabled")]
            public List<WeaponObj> weaponGraphics = new List<WeaponObj>();


            /// <summary>
            /// If true then animation runner clips can override animator clips 
            /// </summary>
            [Tooltip("If true then animation runner clips can override animator clips ")]
            public bool useWeaponAnimatorOverrides = false;

            /// <summary>
            /// List of clips which will override animations in the Animator
            /// </summary>
            [Tooltip("List of clips which will override animations in the Animator ")]
            public List<AnimatorClipRunnerOverride> weaponAnimatorClipRunnerOverrides = new List<AnimatorClipRunnerOverride>();

            /// <summary>
            /// If true then animation overrides will work by adding and removing GC Character States
            /// </summary>
            [Tooltip("If true then animation overrides will work by adding and removing GC Character States")]
            public bool overrideWithGCCharacterState = false;


#if ABC_GC_Integration

        /// <summary>
        /// The GC state to add when the weapon is equipped
        /// </summary>
        [Tooltip("The GC state to add when the weapon is equipped")]
        public GameCreator.Characters.CharacterState gcEquipState = null;


        /// <summary>
        /// The GC state to add when the weapon is unequipped
        /// </summary>
        [Tooltip("The GC state to add when the weapon is unequipped")]
        public GameCreator.Characters.CharacterState gcUnEquipState = null;

#endif

#if ABC_GC_2_Integration

        /// <summary>
        /// If true then ABC will not run it's equip animations if a GC2 state is to be used
        /// </summary>
        [Tooltip("The GC 2 state to add when the weapon is equipped")]
        public bool GCCharacterStateDisableABCEquipAnimations = true; 

        /// <summary>
        /// The GC 2 state to add when the weapon is equipped
        /// </summary>
        [Tooltip("The GC 2 state to add when the weapon is equipped")]
        public GameCreator.Runtime.Characters.State gc2EquipState = null;

        /// <summary>
        /// The GC 2 state to add when the weapon is unequipped
        /// </summary>
        [Tooltip("The GC 2 state to add when the weapon is unequipped")]
        public GameCreator.Runtime.Characters.State gc2UnEquipState = null;

#endif


            /// <summary>
            /// If true then graphics and animations will activate when the weapon is enabled
            /// </summary>
            [Tooltip("Show effect and play animation when an weapon is enabled/disabled")]
            public bool useWeaponEquipAnimations = true;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponEnableAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponEnableAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponEnableAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponEnableAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponEnableAnimationRunnerClipDuration = 1f;


            /// <summary>
            /// If true then the weapon enable animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate weapon enable animation on the main entities animation runner")]
            public bool weaponEnableAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the  weapon enable animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate weapon enable animation on the current scroll ability's graphic animation runner")]
            public bool weaponEnableAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the  weapon enable animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate weapon enable animation on the current weapons animation runner")]
            public bool weaponEnableAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the weapon Enable animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponEnableAnimatorParameter;

            /// <summary>
            /// Type of parameter for the  weapon Enable animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponEnableAnimatorParameterType;

            /// <summary>
            /// Value to turn on the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponEnableAnimatorOnValue;

            /// <summary>
            /// Value to turn off the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponEnableAnimatorOffValue;

            /// <summary>
            /// Duration of the ability group animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponEnableAnimatorDuration = 3f;

            /// <summary>
            /// If true then the weapon enable animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate weapon enable animation on the main entities animator")]
            public bool weaponEnableAnimateOnEntity = true;

            /// <summary>
            /// If true then the weapon enable animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate weapon enable animation on the current scroll ability's graphic animator")]
            public bool weaponEnableAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the weapon enable animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate weapon enable animation on the current weapons animator")]
            public bool weaponEnableAnimateOnWeapon = false;

            /// <summary>
            /// The position type for the weapon left hand IK
            /// </summary>
            [Tooltip("The position type for the weapon left hand IK")]
            public WeaponIKTargetType weaponLeftHandIKTargetType;

            /// <summary>
            /// The weight of the IK
            /// </summary>
            [Tooltip("The weight of the IK")]
            public float weaponLeftHandIKWeight = 1f;

            /// <summary>
            /// The transition speed when applying the IK
            /// </summary>
            [Tooltip("The transition speed when applying the IK")]
            public float weaponLeftHandIKTransitionSpeed = 0.5f;

            /// <summary>
            /// used for weapon IK starting position if OnObject is selected. Determines what object the IK will be attached too
            /// </summary>
            [Tooltip("used for weapon IK starting position if OnObject is selected. Determines what object the IK will be attached too")]
            public ABC_GameObjectReference weaponLeftHandIKOnObject;

            /// <summary>
            /// Tag which the weapon IK will attach to when Enabled. Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag which the weapon IK will attach to when Enabled. Does not work for ABC tags.")]
            public string weaponLeftHandIKOnTag;

            /// <summary>
            /// The position type for the weapon right hand IK
            /// </summary>
            [Tooltip("The position type for the weapon right hand IK")]
            public WeaponIKTargetType weaponRightHandIKTargetType;

            /// <summary>
            /// The weight of the IK
            /// </summary>
            [Tooltip("The weight of the IK")]
            public float weaponRightHandIKWeight = 1f;

            /// <summary>
            /// The transition speed when applying the IK
            /// </summary>
            [Tooltip("The transition speed when applying the IK")]
            public float weaponRightHandIKTransitionSpeed = 0.5f;

            /// <summary>
            /// used for weapon IK starting position if OnObject is selected. Determines what object the IK will be attached too
            /// </summary>
            [Tooltip("used for weapon IK starting position if OnObject is selected. Determines what object the IK will be attached too")]
            public ABC_GameObjectReference weaponRightHandIKOnObject;

            /// <summary>
            /// Tag which the weapon IK will attach to when Enabled. Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag which the weapon IK will attach to when Enabled. Does not work for ABC tags.")]
            public string weaponRightHandIKOnTag;


            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponDisableAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponDisableAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponDisableAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponDisableAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponDisableAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the weapon Disable animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate weapon disable animation on the main entities animation runner")]
            public bool weaponDisableAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the  weapon Disable animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate weapon disable animation on the current scroll ability's graphic animation runner")]
            public bool weaponDisableAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the  weapon Disable animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate weapon disable animation on the current weapons animation runner")]
            public bool weaponDisableAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the weapon Enable animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponDisableAnimatorParameter;

            /// <summary>
            /// Type of parameter for the  weapon Enable animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponDisableAnimatorParameterType;

            /// <summary>
            /// Value to turn on the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponDisableAnimatorOnValue;

            /// <summary>
            /// Value to turn off the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponDisableAnimatorOffValue;

            /// <summary>
            /// Duration of the ability group animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponDisableAnimatorDuration = 3f;

            /// <summary>
            /// If true then the weapon disable animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate weapon disable animation on the main entities animator")]
            public bool weaponDisableAnimateOnEntity = true;

            /// <summary>
            /// If true then the weapon Disable animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate weapon Disable animation on the current scroll ability's graphic animator")]
            public bool weaponDisableAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the weapon Disable animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate weapon Disable animation on the current weapons animator")]
            public bool weaponDisableAnimateOnWeapon = false;

            /// <summary>
            /// If true then weapon will have ammo avaliable for abilities to consume
            /// </summary>
            [Tooltip("If true then weapon will have ammo avaliable for abilities to consume")]
            public bool UseWeaponAmmo = false;

            /// <summary>
            /// How much ammo the weapon currently has
            /// </summary>
            [Tooltip("How much ammo the weapon currently has")]
            public int weaponAmmoCount = 100;

            /// <summary>
            /// can weapon reload
            /// </summary>
            [Tooltip("Enable weapon reload")]
            public bool useWeaponReload = false;

            /// <summary>
            /// size of clip before reload needed
            /// </summary>
            [Tooltip("size of clip before reload needed")]
            public int weaponClipSize = 50;

            /// <summary>
            /// How long it takes to reload the weapon, filling the clip
            /// </summary>
            [Tooltip("How long it takes to reload the weapon, filling the clip")]
            public float weaponReloadDuration = 1;

            /// <summary>
            /// stops any abilities from being activated for the duration when reloading starts
            /// </summary>
            [Tooltip("stops any abilities from being activated for the duration when reloading starts")]
            public float weaponReloadRestrictAbilityActivationDuration = 1f;

            /// <summary>
            /// Will automatically reload the weapon when required
            /// </summary>
            [Tooltip("Will automatically reload the weapon when required")]
            public bool autoReloadWeaponWhenRequired = true;

            /// <summary>
            /// If true then every reload duration set the clip size will increase by 1 (like adding shotgun shells)
            /// </summary>
            [Tooltip("If true then every reload duration set the clip size will increase by 1 (like adding shotgun shells)")]
            public bool weaponReloadFillClip = false;

            /// <summary>
            /// If true then the reload graphic will repeat every time the clip is added too
            /// </summary>
            [Tooltip(" If true then the reload graphic will repeat every time the clip is added too")]
            public bool weaponReloadFillClipRepeatGraphic = true;

            /// <summary>
            /// If true then graphics will appear when ability is reloaded
            /// </summary>
            [Tooltip("Use reload effects and animations")]
            public bool useReloadWeaponAesthetics;

            /// <summary>
            /// If true then the reload animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate reload animation on the main entities animator")]
            public bool reloadWeaponAnimateOnEntity = true;

            /// <summary>
            /// If true then the reload animation will activate on the scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate reload animation on the scroll ability's graphic animator")]
            public bool reloadWeaponAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate animation on the current weapons animator")]
            public bool reloadWeaponAnimateOnWeapon = false;

            /// <summary>
            /// Name of the reload animation
            /// </summary>
            [Tooltip("Name of the animation in the controller ")]
            public string reloadWeaponAnimatorParameter;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference reloadWeaponAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference reloadWeaponAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float reloadWeaponAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float reloadWeaponAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// If true then the reload weapon animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate reload weapon animation on the main entities animation runner")]
            public bool reloadWeaponAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the reload weapon animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate reload weapon animation on the current scroll ability's graphic animation runner")]
            public bool reloadWeaponAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool reloadWeaponAnimationRunnerOnWeapon = false;

            /// <summary>
            /// Parameter type of the reload animation
            /// </summary>
            [Tooltip("Parameter type to activate animation")]
            public AnimatorParameterType reloadWeaponAnimatorParameterType;

            /// <summary>
            /// Value to start the reload animation
            /// </summary>
            [Tooltip("Value to turn on animation")]
            public string reloadWeaponAnimatorOnValue;

            /// <summary>
            /// Value to end the reload animation
            /// </summary>
            [Tooltip("Value to turn off animation ")]
            public string reloadWeaponAnimatorOffValue;

            /// <summary>
            /// Graphic object that shows when reloading
            /// </summary>
            [Tooltip("Particle or object to show when preparing")]
            public ABC_GameObjectReference reloadWeaponGraphic;

            /// <summary>
            /// Sub reloading graphic. Will be a child of the main graphic
            /// </summary>
            [Tooltip("Sub mainGraphic or object to show when preparing")]
            public ABC_GameObjectReference reloadWeaponSubGraphic;


            /// <summary>
            /// duration of the reload graphic 
            /// </summary>
            [Tooltip("How long to show the graphical effect for")]
            public float reloadWeaponAestheticDuration = 2f;


            /// <summary>
            /// delay before reload graphic is shown
            /// </summary>
            [Tooltip(" delay before reload graphic is shown")]
            public float reloadWeaponAestheticDelay = 0f;

            /// <summary>
            /// Starting position of the reload graphic
            /// </summary>
            [Tooltip("Starting position of the effect")]
            public StartingPosition reloadWeaponStartPosition;

            /// <summary>
            /// The object which is used when starting position is OnObject
            /// </summary>
            public ABC_GameObjectReference reloadWeaponPositionOnObject;

            /// <summary>
            /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag to start from")]
            public string reloadWeaponPositionOnTag;


            /// <summary>
            /// Offset for reload graphics
            /// </summary>
            [Tooltip("Offset of the preparing effects")]
            public Vector3 reloadWeaponAestheticsPositionOffset;

            /// <summary>
            /// Forward Offset for reload graphics
            /// </summary>
            [Tooltip("Forward offset from  position")]
            public float reloadWeaponAestheticsPositionForwardOffset = 0f;

            /// <summary>
            /// Right Offset for reload graphics
            /// </summary>
            [Tooltip("Right offset from  position")]
            public float reloadWeaponAestheticsPositionRightOffset = 0f;


            /// <summary>
            /// If true then the weapon is able to parry 
            /// </summary>
            [Tooltip("If true then the weapon is able to parry ")]
            public bool enableWeaponParry = false;

            /// <summary>
            /// Delay before weapon parry status is in effect
            /// </summary>
            [Tooltip("Delay before weapon parry status is in effect")]
            public float weaponParryDelay = 0.2f;

            /// <summary>
            /// Duration of weapon parry status
            /// </summary>
            [Tooltip(" Duration of weapon parry status ")]
            public float weaponParryDuration = 0.5f;

            /// <summary>
            /// Cooldown duration till entity can parry weapon again
            /// </summary>
            [Tooltip("Cooldown duration till entity can parry weapon again")]
            public float weaponParryCooldown = 1f;


            /// <summary>
            /// If true then the entity needs to be facing the ability for the parry to occur, attacks from behind won't parry
            /// </summary>
            [Tooltip("If true then the entity needs to be facing the ability for the parry to occur, attacks from behind won't parry")]
            public bool weaponParryFaceAbilityRequired = false;


            /// <summary>
            /// If true then the entity will automatically rotate to face the direction of where the ability came from
            /// </summary>
            [Tooltip("If true then the entity will automatically rotate to face the direction of where the ability came from")]
            public bool weaponParryTurnToAbilityHitPoint = true;

            /// <summary>
            /// If true then abilities will be enabled after parrying
            /// </summary>
            [Tooltip("If true then abilities will be enabled after parrying")]
            public bool enableAbilitiesAfterParrying = false;

            /// <summary>
            /// Duration that abilities will be enabled for
            /// </summary>
            [Tooltip("Duration that abilities will be enabled for")]
            public float enableAbilitiesAfterParryingDuration = 1f;

            /// <summary>
            /// Ability ID's to enable
            /// </summary>
            [Tooltip("Ability ID's to enable")]
            public List<int> abilityIDsToEnableAfterParrying = new List<int>();

            /// <summary>
            /// If true then an ability will activate after Parrying
            /// </summary>
            [Tooltip("If true then an ability will activate after Parrying")]
            public bool activateAbilityAfterParrying = false;

            /// <summary>
            /// Ability ID to activate
            /// </summary>
            [Tooltip("Ability ID to activate")]
            public int abilityIDToActivateAfterParrying = 0;

            /// <summary>
            ///  used for inspector, keeps track of what ability is currently chosen for the rule 
            /// </summary>
            public int activateAbilityAfterParryListChoice = 0;




            /// <summary>
            /// If true then graphics will appear when entity parrys
            /// </summary>
            [Tooltip("Use weapon parry effects and animations")]
            public bool useWeaponParryAesthetics;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponParryAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponParryAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponParryAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponParryAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponParryAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate animation on the main entities animation runner")]
            public bool weaponParryAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animation runner")]
            public bool weaponParryAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponParryAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the weapon parry animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponParryAnimatorParameter;

            /// <summary>
            /// Type of parameter for the animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponParryAnimatorParameterType;

            /// <summary>
            /// Value to turn on the animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponParryAnimatorOnValue;

            /// <summary>
            /// Value to turn off the animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponParryAnimatorOffValue;

            /// <summary>
            /// Duration of the  animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponParryAnimatorDuration = 3f;


            /// <summary>
            /// If true then the animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate animation on the main entities animator")]
            public bool weaponParryAnimateOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animator")]
            public bool weaponParryAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate  animation on the current weapons animator")]
            public bool weaponParryAnimateOnWeapon = false;

            //CHANGE BELOW 

            /// <summary>
            /// Graphic object that shows when Parrying
            /// </summary>
            [Tooltip("Particle or object to show when Parrying")]
            public ABC_GameObjectReference weaponParryEffectGraphic;

            /// <summary>
            /// Sub Parrying graphic. Will be a child of the main graphic
            /// </summary>
            [Tooltip("Sub mainGraphic or object to show when Parrying an attack")]
            public ABC_GameObjectReference weaponParryEffectSubGraphic;


            /// <summary>
            /// duration of the Parrying graphic 
            /// </summary>
            [Tooltip("How long to show the graphical effect for")]
            public float weaponParryEffectAestheticDuration = 2f;


            /// <summary>
            /// delay before Parrying graphic is shown
            /// </summary>
            [Tooltip(" delay before reload graphic is shown")]
            public float weaponParryEffectAestheticDelay = 0f;

            /// <summary>
            /// Starting position of the Parrying graphic
            /// </summary>
            [Tooltip("Starting position of the effect")]
            public StartingPosition weaponParryEffectStartPosition;

            /// <summary>
            /// The object which is used when starting position is OnObject
            /// </summary>
            public ABC_GameObjectReference weaponParryEffectPositionOnObject;


            /// <summary>
            /// Offset for Parrying graphics
            /// </summary>
            [Tooltip("Offset of the Parrying effects")]
            public Vector3 weaponParryEffectAestheticsPositionOffset;

            /// <summary>
            /// Forward Offset for Parrying graphics
            /// </summary>
            [Tooltip("Forward offset from position")]
            public float weaponParryEffectAestheticsPositionForwardOffset = 0f;

            /// <summary>
            /// Right Offset for Parrying graphics
            /// </summary>
            [Tooltip("Right offset from  position")]
            public float weaponParryEffectAestheticsPositionRightOffset = 0f;


            /// <summary>
            /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag to start from")]
            public string weaponParryEffectPositionOnTag;




            /// <summary>
            /// If true then weapon block will be enabled 
            /// </summary>
            [Tooltip("If true then weapon block will be enabled ")]
            public bool enableWeaponBlock = false;


            /// <summary>
            /// If true then the entity needs to be facing the ability for the block to occur, attacks from behind won't block
            /// </summary>
            [Tooltip("If true then the entity will automatically rotate to face the direction of where the ability came from")]
            public bool weaponBlockFaceAbilityRequired = true;

            /// <summary>
            /// If true then the entity will automatically rotate to face the direction of where the ability came from
            /// </summary>
            [Tooltip("If true then the entity will automatically rotate to face the direction of where the ability came from")]
            public bool weaponBlockTurnToAbilityHitPoint = false;

            /// <summary>
            /// If true then any incoming melee attacks will be interrupted on block
            /// </summary>
            [Tooltip("If true then any incoming melee attacks will be interrupted on block")]
            public bool interruptBlockedMeleeAttack = false;

            /// <summary>
            /// if true then a stat can be set to increase when blocking
            /// </summary>
            [Tooltip(" if true then a stat can be set to increase when blocking")]
            public bool weaponBlockIncreaseStat = false;

            /// <summary>
            /// Stat to increase when blocking
            /// </summary>
            [Tooltip("Stat to increase when blocking")]
            public string weaponBlockStatToIncrease = "Defence";

            /// <summary>
            /// if true then adjust health damage will be mitigated
            /// </summary>
            [Tooltip("if true then adjust health damage will be mitigated")]
            public float weaponBlockStatPercentageIncrease;

            /// <summary>
            /// The amount to mitigate damage by when blocking
            /// </summary>
            [Tooltip("The amount to mitigate damage by when blocking")]
            public float weaponBlockMitigateMeleeDamagePercentage = 30f;

            /// <summary>
            /// The amount to mitigate damage by when blocking
            /// </summary>
            [Tooltip("The amount to mitigate damage by when blocking")]
            public float weaponBlockMitigateProjectileDamagePercentage = 30f;


            /// <summary>
            /// If true then effect prevention will be applied on melee attacks
            /// </summary>
            [Tooltip("If true then effect prevention will be applied on melee attacks")]
            public bool weaponBlockPreventMeleeEffects = false;

            /// <summary>
            /// If true then effect prevention will be applied to projectile and raycast attacks
            /// </summary>
            [Tooltip(" If true then effect prevention will be applied to projectile and raycast attacks")]
            public bool weaponBlockPreventProjectileEffects = false;

            /// <summary>
            /// Amount to reduce block durability by for each ability blocked, once 0 entity will stop blocking
            /// </summary>
            [Tooltip("Amount to reduce block durability by for each ability blocked, once 0 entity will stop blocking")]
            public float weaponBlockDurabilityReduction = 50f;

            //Structure for working out any potency modifications (Add 80% of Originators Strength)
            [System.Serializable]
            public class BlockStatModifications {

                /// <summary>
                /// The integration type for stat functionality - if ABC is picked then normal functionality is used
                /// else stats from another integration system i.e game creator is used
                /// </summary>
                public ABCIntegrationType statIntegrationType;

                /// <summary>
                /// The operator used in changing the potency (Increase, Decrease)
                /// </summary>
                public ArithmeticIncrDecrOperators arithmeticOperator;

                /// <summary>
                /// Name of the stat to modify
                /// </summary>
                public string statName;

                /// <summary>
                /// Value to increase stat by
                /// </summary>
                public float modificationValue;

                /// <summary>
                /// Increase by percent or base value (true if percent, else false)
                /// </summary>
                public PercentOrBase modifyByPercentOrBaseValue;


                /// <summary>
                /// The final value which the stat was increased or decreased by
                /// </summary>
                public float finalStatValueModification;


            }

            /// <summary>
            /// If true then weapon stats will be modified on blocking
            /// </summary>
            [Tooltip("If true then weapon stats will be modified on blocking")]
            public bool weaponBlockModifyStats = false;

            /// <summary>
            /// A list of block stat increase modifications to make when the entity is blocking
            /// </summary>
            [Tooltip("A list of block stat increase modifications to make when the entity is blocking")]
            public List<BlockStatModifications> weaponBlockStatModifications = new List<BlockStatModifications>();

            /// <summary>
            /// If true then abilities will be enabled after blocking
            /// </summary>
            [Tooltip("If true then abilities will be enabled after blocking")]
            public bool enableAbilitiesAfterBlocking = false;

            /// <summary>
            /// Duration that abilities will be enabled for
            /// </summary>
            [Tooltip("Duration that abilities will be enabled for")]
            public float enableAbilitiesAfterBlockingDuration = 1f;

            /// <summary>
            /// Ability ID's to enable
            /// </summary>
            [Tooltip("Ability ID's to enable")]
            public List<int> abilityIDsToEnableAfterBlocking = new List<int>();

            /// <summary>
            /// If true then an ability will activate after blocking
            /// </summary>
            [Tooltip("If true then an ability will activate after blocking")]
            public bool activateAbilityAfterBlocking = false;

            /// <summary>
            /// Ability ID to activate
            /// </summary>
            [Tooltip("Ability ID to activate")]
            public int abilityIDToActivateAfterBlocking = 0;

            /// <summary>
            ///  used for inspector, keeps track of what ability is currently chosen for the rule 
            /// </summary>
            public int activateAbilityAfterBlockListChoice = 0;


            /// <summary>
            /// If true then graphics will appear when entity blocks
            /// </summary>
            [Tooltip("Use weapon block effects and animations")]
            public bool useWeaponBlockAesthetics;

            /// <summary>
            /// Graphic object that shows when blocking
            /// </summary>
            [Tooltip("Particle or object to show when blocking")]
            public ABC_GameObjectReference weaponBlockEffectGraphic;

            /// <summary>
            /// Sub blocking graphic. Will be a child of the main graphic
            /// </summary>
            [Tooltip("Sub mainGraphic or object to show when blocking an attack")]
            public ABC_GameObjectReference weaponBlockEffectSubGraphic;


            /// <summary>
            /// duration of the blocking graphic 
            /// </summary>
            [Tooltip("How long to show the graphical effect for")]
            public float weaponBlockEffectAestheticDuration = 2f;


            /// <summary>
            /// delay before blocking graphic is shown
            /// </summary>
            [Tooltip(" delay before reload graphic is shown")]
            public float weaponBlockEffectAestheticDelay = 0f;

            /// <summary>
            /// Starting position of the blocking graphic
            /// </summary>
            [Tooltip("Starting position of the effect")]
            public StartingPosition weaponBlockEffectStartPosition;

            /// <summary>
            /// The object which is used when starting position is OnObject
            /// </summary>
            public ABC_GameObjectReference weaponBlockEffectPositionOnObject;


            /// <summary>
            /// Offset for blocking graphics
            /// </summary>
            [Tooltip("Offset of the blocking effects")]
            public Vector3 weaponBlockEffectAestheticsPositionOffset;

            /// <summary>
            /// Forward Offset for blocking graphics
            /// </summary>
            [Tooltip("Forward offset from position")]
            public float weaponBlockEffectAestheticsPositionForwardOffset = 0f;

            /// <summary>
            /// Right Offset for blocking graphics
            /// </summary>
            [Tooltip("Right offset from  position")]
            public float weaponBlockEffectAestheticsPositionRightOffset = 0f;


            /// <summary>
            /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag to start from")]
            public string weaponBlockEffectPositionOnTag;


            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponBlockAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponBlockAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the  animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate animation on the main entities animation runner")]
            public bool weaponBlockAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animation runner")]
            public bool weaponBlockAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponBlockAnimationRunnerOnWeapon = false;

            /// <summary>
            /// If true then animation overrides will be blocked
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponBlockStopAnimationOverrides = false; 


            /// <summary>
            /// Name of the animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponBlockAnimatorParameter;

            /// <summary>
            /// Type of parameter for the animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponBlockAnimatorParameterType;

            /// <summary>
            /// Value to turn on the animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponBlockAnimatorOnValue;

            /// <summary>
            /// Value to turn off the animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponBlockAnimatorOffValue;


            /// <summary>
            /// If true then the animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate animation on the main entities animator")]
            public bool weaponBlockAnimateOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animator")]
            public bool weaponBlockAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate animation on the current weapons animator")]
            public bool weaponBlockAnimateOnWeapon = false;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponBlockReactionAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponBlockReactionAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockReactionAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockReactionAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponBlockReactionAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate animation on the main entities animation runner")]
            public bool weaponBlockReactionAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animation runner")]
            public bool weaponBlockReactionAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponBlockReactionAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponBlockReactionAnimatorParameter;

            /// <summary>
            /// Type of parameter for the animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponBlockReactionAnimatorParameterType;

            /// <summary>
            /// Value to turn on the  animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponBlockReactionAnimatorOnValue;

            /// <summary>
            /// Value to turn off the animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponBlockReactionAnimatorOffValue;

            /// <summary>
            /// Duration of the ability group animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponBlockReactionAnimatorDuration = 3f;


            /// <summary>
            /// If true then the animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate animation on the main entities animator")]
            public bool weaponBlockReactionAnimateOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animator")]
            public bool weaponBlockReactionAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate animation on the current weapons animator")]
            public bool weaponBlockReactionAnimateOnWeapon = false;

            /// <summary>
            /// If true then animations will activate
            /// </summary>
            [Tooltip("If true then animations will activate")]
            public bool useWeaponMeleeAttackReflectedAnimations = false;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponMeleeAttackReflectedAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponMeleeAttackReflectedAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponMeleeAttackReflectedAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponMeleeAttackReflectedAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponMeleeAttackReflectedAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate animation on the main entities animation runner")]
            public bool weaponMeleeAttackReflectedAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animation runner")]
            public bool weaponMeleeAttackReflectedAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponMeleeAttackReflectedAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponMeleeAttackReflectedAnimatorParameter;

            /// <summary>
            /// Type of parameter for the  animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponMeleeAttackReflectedAnimatorParameterType;

            /// <summary>
            /// Value to turn on the animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponMeleeAttackReflectedAnimatorOnValue;

            /// <summary>
            /// Value to turn off the animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponMeleeAttackReflectedAnimatorOffValue;

            /// <summary>
            /// Duration of the animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponMeleeAttackReflectedAnimatorDuration = 3f;


            /// <summary>
            /// If true then the animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate animation on the main entities animator")]
            public bool weaponMeleeAttackReflectedAnimateOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animator")]
            public bool weaponMeleeAttackReflectedAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate animation on the current weapons animator")]
            public bool weaponMeleeAttackReflectedAnimateOnWeapon = false;

            /// <summary>
            /// If true then animations will activate
            /// </summary>
            [Tooltip("If true then animations will activate")]
            public bool useWeaponCrosshairOverrideAnimations = false;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponCrosshairOverrideAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponCrosshairOverrideAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponCrosshairOverrideAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponCrosshairOverrideAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponCrosshairOverrideAnimationRunnerClipDuration = 1f;

            /// <summary>
            /// If true then the animation will activate on the main entities animation runner
            /// </summary>
            [Tooltip("Activate animation on the main entities animation runner")]
            public bool weaponCrosshairOverrideAnimationRunnerOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animation runner
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animation runner")]
            public bool weaponCrosshairOverrideAnimationRunnerOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animation runner
            /// </summary>
            [Tooltip("Activate animation on the current weapons animation runner")]
            public bool weaponCrosshairOverrideAnimationRunnerOnWeapon = false;


            /// <summary>
            /// Name of the animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponCrosshairOverrideAnimatorParameter;

            /// <summary>
            /// Type of parameter for the  animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponCrosshairOverrideAnimatorParameterType;

            /// <summary>
            /// Value to turn on the animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponCrosshairOverrideAnimatorOnValue;

            /// <summary>
            /// Value to turn off the animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponCrosshairOverrideAnimatorOffValue;

            /// <summary>
            /// Duration of the animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponCrosshairOverrideAnimatorDuration = 3f;


            /// <summary>
            /// If true then the animation will activate on the main entities animator
            /// </summary>
            [Tooltip("Activate animation on the main entities animator")]
            public bool weaponCrosshairOverrideAnimateOnEntity = true;

            /// <summary>
            /// If true then the animation will activate on the current scroll ability's graphic animator
            /// </summary>
            [Tooltip("Activate animation on the current scroll ability's graphic animator")]
            public bool weaponCrosshairOverrideAnimateOnScrollGraphic = false;

            /// <summary>
            /// If true then the animation will activate on the current weapons animator
            /// </summary>
            [Tooltip("Activate animation on the current weapons animator")]
            public bool weaponCrosshairOverrideAnimateOnWeapon = false;



            /// <summary>
            /// If true then the weapon can be dropped
            /// </summary>
            [Tooltip("If true then the weapon can be dropped")]
            public bool enableWeaponDrop = false;

            /// <summary>
            /// How long it takes to drop the weapon
            /// </summary>
            [Tooltip("How long it takes to drop the weapon")]
            public float weaponDropDuration = 1f;

            /// <summary>
            /// Determines if the weapon is disabled, deleted or nothing happens on weapon drop
            /// </summary>
            [Tooltip("Determines if the weapon is disabled, deleted or nothing happens on weapon drop")]
            public WeaponDropAction weaponDropAction = WeaponDropAction.DisableWeapon;


            /// <summary>
            /// If true then the abilities set to be enabled will have the weapon drop action (disable/delete) applied
            /// </summary>
            [Tooltip("If true then the abilities set to be enabled will have the weapon drop action (disable/delete) applied")]
            public bool weaponDropActionApplyToWeaponEnableAbilities = true;


            /// <summary>
            /// If true then the groups this weapon is assigned to will have the weapon drop action (disable/delete) applied
            /// </summary>
            [Tooltip("If true then the groups this weapon is assigned to will have the weapon drop action (disable/delete) applied")]
            public bool weaponDropActionApplyToAssignedGroups = true;

            /// <summary>
            /// If true then the UI this weapon is assigned to will have the weapon drop action (disable/delete) applied
            /// </summary>
            [Tooltip("If true then the UI this weapon is assigned to will have the weapon drop action (disable/delete) applied")]
            public bool weaponDropActionApplyToAssignedUI = true;



            /// <summary>
            /// If true then a gameobject can be set to drop when the weapon is dropped
            /// </summary>
            [Tooltip("If true then a gameobject can be set to drop when the weapon is dropped")]
            public bool useWeaponDropObject = false;

            /// <summary>
            /// The object which can be dropped 
            /// </summary>
            [Tooltip("The object which can be dropped (also can be updated by picking up)")]
            public ABC_GameObjectReference weaponDropObject = null;

            /// <summary>
            /// Delay before the weapon drop object is actually dropped
            /// </summary>
            [Tooltip("Delay before the weapon drop object is actually dropped")]
            public float weaponDropObjectDelay = 0.5f;

            /// <summary>
            /// If true then the drop object will be deleted after a delay once dropped
            /// </summary>
            [Tooltip("If true then the drop object will be deleted after a delay once dropped")]
            public float weaponDropObjectDuration = 0f;

            /// <summary>
            /// If true then on drop the weapon drop object will have it's ammo updated to match the current ammo on the weapon
            /// </summary>
            [Tooltip("If true then on drop the weapon drop object will have it's ammo updated to match the current ammo on the weapon")]
            public bool updateWeaponDropAmmo = false;


            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference weaponDropAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference weaponDropAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float weaponDropAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float weaponDropAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float weaponDropAnimationRunnerClipDuration = 1f;


            /// <summary>
            /// Name of the weapon Enable animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string weaponDropAnimatorParameter;

            /// <summary>
            /// Type of parameter for the  weapon Enable animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType weaponDropAnimatorParameterType;

            /// <summary>
            /// Value to turn on the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string weaponDropAnimatorOnValue;

            /// <summary>
            /// Value to turn off the  weapon Enable animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string weaponDropAnimatorOffValue;

            /// <summary>
            /// Duration of the ability group animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float weaponDropAnimatorDuration = 3f;





            /// <summary>
            /// Used by inspector to select dropdowns
            /// </summary>
            public int abilitiesListChoice = 0;

            /// <summary>
            /// Used by inspector to select dropdowns
            /// </summary>
            public int abilityGroupListChoice = 0;


            #endregion

            // ************ Variables *****************************

            #region Variables For Weapons

            /// <summary>
            /// Stores the drop weapon object
            /// </summary>
            private GameObject weaponDropObjectPool;

            /// <summary>
            /// Store weapon reload graphic
            /// </summary>
            private List<GameObject> weaponReloadPool = new List<GameObject>();

            /// <summary>
            /// Store weapon parry effect graphic
            /// </summary>
            private List<GameObject> weaponParryEffectPool = new List<GameObject>();

            /// <summary>
            /// Store weapon block effect graphic
            /// </summary>
            private List<GameObject> weaponBlockEffectPool = new List<GameObject>();


            /// <summary>
            /// If true then the weapon is currently parrying
            /// </summary>
            private bool isWeaponParrying = false;

            /// <summary>
            /// If true then the weapon is currently blocking
            /// </summary>
            private bool isWeaponBlocking = false;

            /// <summary>
            /// Tracks the current active weapon reload graphic being used in the situation where reload is interrupted and graphic needs to disable quick
            /// </summary>
            private GameObject currentActiveWeaponReloadGraphic = null;

            /// <summary>
            /// The amount of ammo we currently have in our clip
            /// </summary>
            /// <remarks>Set to public to allow for saving/loading</remarks>
            public int currentWeaponAmmoClipCount = -1;

            /// <summary>
            /// Determines if a reload is required
            /// </summary>
            private bool weaponReloadRequired = false;

            /// <summary>
            /// Determines if the weapon is currently reloading
            /// </summary>
            private bool weaponIsReloading = false;

            /// <summary>
            /// If true then the weapon will immediatly stop reloading
            /// </summary>
            /// <remarks>Used for when an weapon is switched over before reloading has finished </remarks>
            private bool weaponReloadInterrupted = false;



            #endregion


            // ************ ENUMS *****************************

            #region ENUMS

            public enum WeaponDropAction {
                None = 0,
                DisableWeapon = 1,
                DeleteWeapon = 2
            }

            public enum WeaponPositionType {
                OnObject = 0,
                OnSelfTag = 1,
                OnTag = 2,
                Self = 3
            }

            public enum WeaponIKTargetType {
                None = 0,
                OnObject = 1,
                OnSelfTag = 2,
            }

            public enum WeaponButtonPressState {
                EnableWeapon = 0
            }

            public enum WeaponGraphicType {
                WeaponDropObject = 0,
                WeaponReload = 1,
                WeaponBlockEffect = 2,
                WeaponParryEffect = 3
            }

            public enum WeaponAnimationType {
                StartAndStop = 0,
                Start = 1,
                Stop = 2
            }

            #endregion

            // ************************** Private Methods *************************************

            #region Private Methods

            /// <summary>
            /// Returns a bool indicating if the weapon is current equipped by the entity provided. 
            /// </summary>
            /// <param name="Originator">Entity which will be be used to determine if this weapon is current ly equipped</param>
            /// <returns>True if this weapon is currently equipped by the Originator, else false</returns>
            private bool IsCurrentlyEquippedBy(ABC_IEntity Originator) {

                if (this.IsEnabled() == false)
                    return false;

                if (Originator.currentEquippedWeapon == this)
                    return true;
                else
                    return false;

            }

            /// <summary>
            /// Main function for checking if a button has been pressed for different weapon events. Depending on the state given the method will return true or false if the setup button has been pressed. 
            /// </summary>
            /// <param name="Originator">Entity that activated the ability</param>
            /// <param name="State">Depending on the state the method will return if a button setup for that state has been pressed. States include: EnableWeapon 
            /// <returns>True if the correct button is being pressed, else false</returns>
            private bool ButtonPressed(ABC_IEntity Originator, WeaponButtonPressState State) {

                InputType inputType = InputType.Button;
                KeyCode key = KeyCode.None;
                string button = "";

                // determine the right configuration depending on the type provided
                switch (State) {
                    case WeaponButtonPressState.EnableWeapon:

                        inputType = this.weaponEnableInputType;
                        key = this.weaponEnableKey;
                        button = this.weaponEnableButton;

                        break;

                }

                // If input type is key and the configured key is being pressed return true
                if (inputType == InputType.Key &&  Originator.inputManager.GetKeyDown(key))
                    return true;

                // if input type is button and the configured button is being pressed return true
                if (inputType == InputType.Button && Originator.inputManager.GetButtonDown(button))
                    return true;


                // correct button is not currently being pressed so return false
                return false;


            }

            /// <summary>
            /// Will create and pool reloading graphics setup for the weapon
            /// </summary>
            /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
            /// <returns>One graphic gameobject which has been created</returns>
            private GameObject CreateWeaponReloadingObjects(bool CreateOne = false) {

                //If the weapon has already been created then return that 
                if (this.weaponReloadPool.Count > 0 && CreateOne == false)
                    return this.weaponReloadPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                GameObject reloadingWep = null;

                if (this.UseWeaponAmmo == true && this.useWeaponReload == true && this.useReloadWeaponAesthetics == true && this.reloadWeaponGraphic.GameObject != null) {

                    //how many objects to make
                    int objCount = CreateOne ? 1 : 3;

                    // only 1 scroll ability activation graphic should play at once so only store 3
                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        reloadingWep = (GameObject)(GameObject.Instantiate(this.reloadWeaponGraphic.GameObject));


                        // copy child object for additional Aesthetic 
                        if (this.reloadWeaponSubGraphic.GameObject != null) {
                            GameObject reloadingChildWep = (GameObject)(GameObject.Instantiate(this.reloadWeaponSubGraphic.GameObject));
                            reloadingChildWep.transform.position = reloadingWep.transform.position;
                            reloadingChildWep.transform.rotation = reloadingWep.transform.rotation;
                            reloadingChildWep.transform.parent = reloadingWep.transform;
                        }

                        //disable and pool the object 
                        ABC_Utilities.PoolObject(reloadingWep);

                        // add to generic list.
                        this.weaponReloadPool.Add(reloadingWep);
                    }

                }

                return reloadingWep;

            }

            /// <summary>
            /// Will create and pool weapon parry effect graphics setup for the weapon
            /// </summary>
            /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
            /// <returns>One graphic gameobject which has been created</returns>
            private GameObject CreateWeaponParryEffectObjects(bool CreateOne = false) {

                //If the weapon has already been created then return that 
                if (this.weaponParryEffectPool.Count > 0 && CreateOne == false)
                    return this.weaponParryEffectPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                GameObject parryEffect = null;

                if (this.enableWeaponParry == true && this.useWeaponParryAesthetics == true && this.weaponParryEffectGraphic.GameObject != null) {

                    //how many objects to make
                    int objCount = CreateOne ? 1 : 3;

                    // only 1 scroll ability activation graphic should play at once so only store 3
                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        parryEffect = (GameObject)(GameObject.Instantiate(this.weaponParryEffectGraphic.GameObject));


                        // copy child object for additional Aesthetic 
                        if (this.weaponParryEffectSubGraphic.GameObject != null) {
                            GameObject parryChildEffect = (GameObject)(GameObject.Instantiate(this.weaponParryEffectSubGraphic.GameObject));
                            parryChildEffect.transform.position = parryEffect.transform.position;
                            parryChildEffect.transform.rotation = parryEffect.transform.rotation;
                            parryChildEffect.transform.parent = parryEffect.transform;
                        }

                        //disable and pool the object 
                        ABC_Utilities.PoolObject(parryEffect);

                        // add to generic list.
                        this.weaponParryEffectPool.Add(parryEffect);
                    }

                }

                return parryEffect;

            }

            /// <summary>
            /// Will create and pool weapon block effect graphics setup for the weapon
            /// </summary>
            /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
            /// <returns>One graphic gameobject which has been created</returns>
            private GameObject CreateWeaponBlockEffectObjects(bool CreateOne = false) {

                //If the weapon has already been created then return that 
                if (this.weaponBlockEffectPool.Count > 0 && CreateOne == false)
                    return this.weaponBlockEffectPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                GameObject blockEffect = null;

                if (this.enableWeaponBlock == true && this.useWeaponBlockAesthetics == true && this.weaponBlockEffectGraphic.GameObject != null) {

                    //how many objects to make
                    int objCount = CreateOne ? 1 : 3;

                    // only 1 scroll ability activation graphic should play at once so only store 3
                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        blockEffect = (GameObject)(GameObject.Instantiate(this.weaponBlockEffectGraphic.GameObject));


                        // copy child object for additional Aesthetic 
                        if (this.weaponBlockEffectSubGraphic.GameObject != null) {
                            GameObject blockChildEffect = (GameObject)(GameObject.Instantiate(this.weaponBlockEffectSubGraphic.GameObject));
                            blockChildEffect.transform.position = blockEffect.transform.position;
                            blockChildEffect.transform.rotation = blockEffect.transform.rotation;
                            blockChildEffect.transform.parent = blockEffect.transform;
                        }

                        //disable and pool the object 
                        ABC_Utilities.PoolObject(blockEffect);

                        // add to generic list.
                        this.weaponBlockEffectPool.Add(blockEffect);
                    }

                }

                return blockEffect;

            }

            /// <summary>
            /// Will create the weapon drop object setup
            /// </summary>
            /// <param name="CreateOne">If true then only one object will be created and returned but not pool'd</param>
            private GameObject CreateWeaponDropObject(bool CreateOne = false) {

                //If the weapon has already been created then return that 
                if (this.weaponDropObjectPool != null && CreateOne == false)
                    return this.weaponDropObjectPool;

                GameObject weaponDropObj = null;


                if (this.weaponDropObject.GameObject == null)
                    return null;

                //how many objects to make
                float objCount = 1;


                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    weaponDropObj = (GameObject)(GameObject.Instantiate(this.weaponDropObject.GameObject));


                    //If we are only creating one then don't pool
                    if (CreateOne == false) {
                        //disable and pool the object 
                        ABC_Utilities.PoolObject(weaponDropObj);

                        // add to tracker. 
                        this.weaponDropObjectPool = weaponDropObj;
                    }
                }



                return weaponDropObj;

            }

            /// <summary>
            /// Will destroy the weapon drop object
            /// </summary>
            private void ClearWeaponDropObject() {

                if (this.weaponDropObjectPool != null) {
                    Destroy(this.weaponDropObjectPool);
                    this.weaponDropObjectPool = null;
                }
            }


            /// <summary>
            /// activate the weapon graphics, making it appear in game
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            /// <param name="GraphicType">Type of graphic to activate (blocking, parrying, dropping etc)</param>
            private IEnumerator ActivateGraphic(ABC_IEntity Originator, WeaponGraphicType GraphicType) {


                StartingPosition startingPosition = StartingPosition.Self;
                GameObject positionOnObject = null;
                string positionOnTag = null;
                Vector3 positionOffset = new Vector3(0, 0, 0);
                float positionForwardOffset = 5f;
                float positionRightOffset = 0f;
                float duration = 2f;
                float delay = 0f;
                bool auxiliarySoftTarget = false;
                GameObject graphicObj = null;



                switch (GraphicType) {
                    case WeaponGraphicType.WeaponDropObject:

                        startingPosition = StartingPosition.Self;
                        positionForwardOffset = 5f;
                        positionRightOffset = 0f;
                        positionOnTag = null;
                        positionOnObject = null;

                        duration = this.weaponDropObjectDuration;

                        delay = this.weaponDropObjectDelay;

                        graphicObj = this.weaponDropObjectPool;

                        if (graphicObj == null || graphicObj.activeInHierarchy == true)
                            graphicObj = this.CreateWeaponDropObject(true);

                        //If set then update weapon pickup ammo to match the weapons ammo which is being 'dropped'
                        if (this.updateWeaponDropAmmo) {
                            ABC_WeaponPickUp wepPickup = graphicObj.GetComponentInChildren<ABC_WeaponPickUp>();

                            //Component found update ammo with current count + whats currently in clip (if clip is not empty)
                            if (wepPickup != null)
                                wepPickup.SetAmmo(this.weaponAmmoCount + (this.currentWeaponAmmoClipCount > 0 ? this.currentWeaponAmmoClipCount : 0));

                        }

                        //track object for future use 
                        this.weaponDropObjectPool = graphicObj;

                        break;
                    case WeaponGraphicType.WeaponReload:

                        startingPosition = this.reloadWeaponStartPosition;
                        positionOnObject = this.reloadWeaponPositionOnObject.GameObject;
                        positionOnTag = this.reloadWeaponPositionOnTag;

                        positionOffset = this.reloadWeaponAestheticsPositionOffset;
                        positionForwardOffset = this.reloadWeaponAestheticsPositionForwardOffset;
                        positionRightOffset = this.reloadWeaponAestheticsPositionRightOffset;

                        duration = this.reloadWeaponAestheticDuration;
                        delay = this.reloadWeaponAestheticDelay;

                        graphicObj = this.weaponReloadPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                        if (graphicObj == null)
                            graphicObj = this.CreateWeaponReloadingObjects(true);

                        //track object for future use 
                        this.currentActiveWeaponReloadGraphic = graphicObj;

                        break;
                    case WeaponGraphicType.WeaponBlockEffect:

                        startingPosition = this.weaponBlockEffectStartPosition;
                        positionOnObject = this.weaponBlockEffectPositionOnObject.GameObject;
                        positionOnTag = this.weaponBlockEffectPositionOnTag;

                        positionOffset = this.weaponBlockEffectAestheticsPositionOffset;
                        positionForwardOffset = this.weaponBlockEffectAestheticsPositionForwardOffset;
                        positionRightOffset = this.weaponBlockEffectAestheticsPositionRightOffset;

                        duration = this.weaponBlockEffectAestheticDuration;
                        delay = this.weaponBlockEffectAestheticDelay;

                        graphicObj = this.weaponBlockEffectPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                        if (graphicObj == null)
                            graphicObj = this.CreateWeaponBlockEffectObjects(true);

                        //No graphic object setup so end here
                        if (graphicObj == null)
                            yield break;


                        break;
                    case WeaponGraphicType.WeaponParryEffect:

                        startingPosition = this.weaponParryEffectStartPosition;
                        positionOnObject = this.weaponParryEffectPositionOnObject.GameObject;
                        positionOnTag = this.weaponParryEffectPositionOnTag;

                        positionOffset = this.weaponParryEffectAestheticsPositionOffset;
                        positionForwardOffset = this.weaponParryEffectAestheticsPositionForwardOffset;
                        positionRightOffset = this.weaponParryEffectAestheticsPositionRightOffset;

                        duration = this.weaponParryEffectAestheticDuration;
                        delay = this.weaponParryEffectAestheticDelay;

                        graphicObj = this.weaponParryEffectPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                        if (graphicObj == null)
                            graphicObj = this.CreateWeaponParryEffectObjects(true);

                        //No graphic object setup so end here
                        if (graphicObj == null)
                            yield break;

                        break;

                    default:

                        break;

                }

                //if delay is greater then 0 then wait 
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);

                //initial starting point is the entity incase anything goes wrong
                Transform meTransform = Originator.transform;
                Vector3 position = meTransform.position;
                //record values which might be used
                GameObject parentObject = Originator.gameObject;
                GameObject targetObject = Originator.target;
                GameObject softTargetObject = Originator.softTarget;
                GameObject worldTargetObject = Originator.worldTargetObj;
                Vector3 worldTargetPosition = Originator.worldTargetPos;

                // get starting position 
                switch (startingPosition) {

                    case StartingPosition.Self:
                        position = meTransform.position + positionOffset + meTransform.forward * positionForwardOffset + meTransform.right * positionRightOffset;
                        parentObject = Originator.gameObject;
                        break;
                    case StartingPosition.OnObject:
                        if (positionOnObject != null) {
                            Transform onObjectTransform = positionOnObject.transform;
                            position = onObjectTransform.position + positionOffset + onObjectTransform.forward * positionForwardOffset + onObjectTransform.right * positionRightOffset;
                            parentObject = onObjectTransform.gameObject;
                        }
                        break;
                    case StartingPosition.OnTag:
                        GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                        if (onTagObj != null) {
                            Transform onTagTransform = onTagObj.transform;
                            position = onTagTransform.position + positionOffset + onTagTransform.forward * positionForwardOffset + onTagTransform.right * positionRightOffset;
                            parentObject = onTagTransform.gameObject;
                        }
                        break;
                    case StartingPosition.OnSelfTag:
                        GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(Originator, positionOnTag);
                        if (onSelfTagObj != null) {
                            Transform onSelfTagTransform = onSelfTagObj.transform;
                            position = onSelfTagTransform.position + positionOffset + onSelfTagTransform.forward * positionForwardOffset + onSelfTagTransform.right * positionRightOffset;
                            parentObject = onSelfTagTransform.gameObject;
                        }
                        break;
                    case StartingPosition.Target:
                        if (targetObject != null) { // get target object
                            var targetTransform = targetObject.transform;
                            position = targetTransform.position + positionOffset + targetTransform.forward * positionForwardOffset + targetTransform.right * positionRightOffset;
                            parentObject = targetObject;

                        } else if (auxiliarySoftTarget == true && softTargetObject != null) {
                            // if there is no current target object and auxiliary soft target is enabled then record current soft target instead
                            var softTargetTransform = softTargetObject.transform;
                            position = softTargetTransform.position + positionOffset + softTargetTransform.forward * positionForwardOffset + softTargetTransform.right * positionRightOffset;
                            parentObject = softTargetObject;
                        }
                        break;
                    case StartingPosition.OnWorld:
                        if (worldTargetObject != null) {
                            var worldTargetTransform = worldTargetObject.transform;
                            position = worldTargetPosition + positionOffset + worldTargetTransform.forward * positionForwardOffset + worldTargetTransform.right * positionRightOffset;
                            parentObject = worldTargetObject;
                        }
                        break;
                    case StartingPosition.CameraCenter:
                        var cameraTransform = Originator.Camera.transform;
                        position = Originator.Camera.transform.position + positionOffset + cameraTransform.forward * positionForwardOffset + cameraTransform.right * positionRightOffset;
                        parentObject = Originator.Camera.gameObject;
                        break;
                    default:
                        Originator.AddToDiagnosticLog("Error: starting position for " + GraphicType.ToString() + "  graphic was not correctly determined.");
                        break;
                }


                // set position and parent
                graphicObj.transform.position = position;
                graphicObj.transform.SetParent(null);

                // set it true 
                graphicObj.SetActive(true);

                // disable and pool graphic  after delay if we haven't set this to 0 (which means infinite) 
                if (duration != 0) {
                    ABC_Utilities.PoolObject(graphicObj, duration);
                }



            }

            /// <summary>
            /// Starts an animation clip using the ABC animation runner stopping it after the duration
            /// </summary>
            /// <param name="ToggleType">The animation to play - WeaponEnable etc</param>
            /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
            /// <param name="AnimationType">The state of animation to run (Start&Stop, Start, Stop)</param>
            private void StartAndStopAnimationRunner(WeaponState ToggleType, ABC_AnimationsRunner AnimationRunner, WeaponAnimationType AnimationType = WeaponAnimationType.StartAndStop) {

                // set variables to be used later 
                AnimationClip animationClip = null;
                float animationClipSpeed = 1f;
                float animationClipDelay = 0f;
                float animationClipDuration = 1f;
                AvatarMask animationClipMask = null;


                //If false then animation will not start if it's currently already being played by the animation runner
                bool startAnimationIfAlreadyRunning = true;

                //If true then runner overrides will be blocked
                bool blockRunnerOverrides = false;

                //If true then the animation will interrupt instead of smoothly end 
                bool interruptInsteadOfEnd = false;

                // If true then the animation will skip blending (used for coming out of crosshair override animation)
                bool skipBlending = false;


                switch (ToggleType) {
                    case WeaponState.Equip:

                        animationClip = this.weaponEnableAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponEnableAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponEnableAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponEnableAnimationRunnerClipDuration;
                        animationClipMask = this.weaponEnableAnimationRunnerMask.AvatarMask;

                        break;

                    case WeaponState.UnEquip:

                        animationClip = this.weaponDisableAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponDisableAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponDisableAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponDisableAnimationRunnerClipDuration;
                        animationClipMask = this.weaponDisableAnimationRunnerMask.AvatarMask;

                        break;

                    case WeaponState.Drop:

                        animationClip = this.weaponDropAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponDropAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponDropAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponDropAnimationRunnerClipDuration;
                        animationClipMask = this.weaponDropAnimationRunnerMask.AvatarMask;

                        break;
                    case WeaponState.Reload:

                        animationClip = this.reloadWeaponAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.reloadWeaponAnimationRunnerClipSpeed;
                        animationClipDelay = this.reloadWeaponAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponReloadDuration;
                        animationClipMask = this.reloadWeaponAnimationRunnerMask.AvatarMask;

                        blockRunnerOverrides = true;

                        break;
                    case WeaponState.Block:

                        animationClip = this.weaponBlockAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponBlockAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponBlockAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponBlockAnimationRunnerClipDuration;
                        animationClipMask = this.weaponBlockAnimationRunnerMask.AvatarMask;
                        
                        blockRunnerOverrides = this.weaponBlockStopAnimationOverrides; 


                        break;
                    case WeaponState.UnBlock:

                        animationClip = this.weaponBlockAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponBlockAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponBlockAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponBlockAnimationRunnerClipDuration;
                        animationClipMask = this.weaponBlockAnimationRunnerMask.AvatarMask;


                        break;
                    case WeaponState.BlockReaction:

                        animationClip = this.weaponBlockReactionAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponBlockReactionAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponBlockReactionAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponBlockReactionAnimationRunnerClipDuration;
                        animationClipMask = this.weaponBlockAnimationRunnerMask.AvatarMask;


                        break;
                    case WeaponState.AttackReflected:

                        animationClip = this.weaponMeleeAttackReflectedAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponMeleeAttackReflectedAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponMeleeAttackReflectedAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponMeleeAttackReflectedAnimationRunnerClipDuration;
                        animationClipMask = this.weaponMeleeAttackReflectedAnimationRunnerMask.AvatarMask;


                        break;
                    case WeaponState.Parry:

                        animationClip = this.weaponParryAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponParryAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponParryAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponParryAnimationRunnerClipDuration;
                        animationClipMask = this.weaponParryAnimationRunnerMask.AvatarMask;


                        break;
                    case WeaponState.CrossHairOverride:

                        animationClip = this.weaponCrosshairOverrideAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.weaponCrosshairOverrideAnimationRunnerClipSpeed;
                        animationClipDelay = this.weaponCrosshairOverrideAnimationRunnerClipDelay;
                        animationClipDuration = this.weaponCrosshairOverrideAnimationRunnerClipDuration;
                        animationClipMask = this.weaponCrosshairOverrideAnimationRunnerMask.AvatarMask;


                        startAnimationIfAlreadyRunning = false;
                        blockRunnerOverrides = true;

                        
                        if (this.IsReloading() == false)
                            skipBlending = true;


                        break;

                }


                // if animator parameter is null or animation runner is not given then animation can't start so end here. 
                if (animationClip == null || AnimationRunner == null)
                    return;

                switch (AnimationType) {
                    case WeaponAnimationType.StartAndStop:
                        AnimationRunner.PlayAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipDuration, animationClipMask, true);
                        break;
                    case WeaponAnimationType.Start:

                        //If animation isn't running or it's already running and animation is set to run if already running then start animation
                        if (AnimationRunner.IsAnimationRunning(animationClip) == false || AnimationRunner.IsAnimationRunning(animationClip) == true && startAnimationIfAlreadyRunning == true)
                            AnimationRunner.StartAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipMask, true, blockRunnerOverrides);

                        break;
                    case WeaponAnimationType.Stop:

                        if (interruptInsteadOfEnd == true)
                            AnimationRunner.InterruptCurrentAnimation(true);
                        else if (skipBlending == true)
                            AnimationRunner.EndAnimation(animationClip, 0, true);
                        else
                            AnimationRunner.EndAnimation(animationClip);

                        break;
                }


            }


            /// <summary>
            /// Starts an animation for the entity depending on what state is passed through
            /// </summary>
            /// <param name="ToggleType">The animation to play - weaponEnable etc</param>
            /// <param name="Animator">Animator component</param>
            /// <param name="AnimationType">The state of animation to run (Start&Stop, Start, Stop)</param>
            private IEnumerator StartAndStopAnimation(WeaponState ToggleType, Animator Animator, WeaponAnimationType AnimationType = WeaponAnimationType.StartAndStop) {

                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated the same call
                //this function would not continue after duration
                float functionRequestTime = Time.time;

                ABC_IEntity abcEntity = ABC_Utilities.GetStaticABCEntity(Animator.gameObject);

                // set variables to be used later 
                AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
                string animatorParameter = "";
                string animatorOnValue = "";
                string animatorOffValue = "";
                float animatorDuration = 1f;

                switch (ToggleType) {
                    case WeaponState.Equip:

                        animatorParameterType = this.weaponEnableAnimatorParameterType;
                        animatorParameter = this.weaponEnableAnimatorParameter;
                        animatorOnValue = this.weaponEnableAnimatorOnValue;
                        animatorOffValue = this.weaponEnableAnimatorOffValue;
                        animatorDuration = this.weaponEnableAnimatorDuration;

                        //Check parameter exists before we continue 
                        if (ABC_Utilities.AnimatorParameterExist(Animator, animatorParameter) == false)
                            yield break;


                        break;
                    case WeaponState.UnEquip:

                        animatorParameterType = this.weaponDisableAnimatorParameterType;
                        animatorParameter = this.weaponDisableAnimatorParameter;
                        animatorOnValue = this.weaponDisableAnimatorOnValue;
                        animatorOffValue = this.weaponDisableAnimatorOffValue;
                        animatorDuration = this.weaponDisableAnimatorDuration;

                        //Check parameter exists before we continue 
                        if (ABC_Utilities.AnimatorParameterExist(Animator, animatorParameter) == false)
                            yield break;

                        break;
                    case WeaponState.Drop:

                        animatorParameterType = this.weaponDropAnimatorParameterType;
                        animatorParameter = this.weaponDropAnimatorParameter;
                        animatorOnValue = this.weaponDropAnimatorOnValue;
                        animatorOffValue = this.weaponDropAnimatorOffValue;
                        animatorDuration = this.weaponDropAnimatorDuration;

                        break;
                    case WeaponState.Reload:

                        animatorParameterType = this.reloadWeaponAnimatorParameterType;
                        animatorParameter = this.reloadWeaponAnimatorParameter;
                        animatorOnValue = this.reloadWeaponAnimatorOnValue;
                        animatorOffValue = this.reloadWeaponAnimatorOffValue;
                        animatorDuration = this.reloadWeaponAestheticDuration;

                        break;

                    case WeaponState.Block:

                        animatorParameterType = this.weaponBlockAnimatorParameterType;
                        animatorParameter = this.weaponBlockAnimatorParameter;
                        animatorOnValue = this.weaponBlockAnimatorOnValue;
                        animatorOffValue = this.weaponBlockAnimatorOffValue;

                        break;
                    case WeaponState.UnBlock:

                        animatorParameterType = this.weaponBlockAnimatorParameterType;
                        animatorParameter = this.weaponBlockAnimatorParameter;
                        animatorOnValue = this.weaponBlockAnimatorOnValue;
                        animatorOffValue = this.weaponBlockAnimatorOffValue;

                        break;
                    case WeaponState.BlockReaction:

                        animatorParameterType = this.weaponBlockReactionAnimatorParameterType;
                        animatorParameter = this.weaponBlockReactionAnimatorParameter;
                        animatorOnValue = this.weaponBlockReactionAnimatorOnValue;
                        animatorOffValue = this.weaponBlockReactionAnimatorOffValue;
                        animatorDuration = this.weaponBlockReactionAnimatorDuration;


                        break;
                    case WeaponState.AttackReflected:

                        animatorParameterType = this.weaponMeleeAttackReflectedAnimatorParameterType;
                        animatorParameter = this.weaponMeleeAttackReflectedAnimatorParameter;
                        animatorOnValue = this.weaponMeleeAttackReflectedAnimatorOnValue;
                        animatorOffValue = this.weaponMeleeAttackReflectedAnimatorOffValue;
                        animatorDuration = this.weaponMeleeAttackReflectedAnimatorDuration;


                        break;
                    case WeaponState.Parry:

                        animatorParameterType = this.weaponParryAnimatorParameterType;
                        animatorParameter = this.weaponParryAnimatorParameter;
                        animatorOnValue = this.weaponParryAnimatorOnValue;
                        animatorOffValue = this.weaponParryAnimatorOffValue;
                        animatorDuration = this.weaponParryAnimatorDuration;

                        break;

                    case WeaponState.CrossHairOverride:

                        animatorParameterType = this.weaponCrosshairOverrideAnimatorParameterType;
                        animatorParameter = this.weaponCrosshairOverrideAnimatorParameter;
                        animatorOnValue = this.weaponCrosshairOverrideAnimatorOnValue;
                        animatorOffValue = this.weaponCrosshairOverrideAnimatorOffValue;
                        animatorDuration = this.weaponCrosshairOverrideAnimatorDuration;

                        break;
                }

                //Start animation if set too
                if (AnimationType == WeaponAnimationType.StartAndStop || AnimationType == WeaponAnimationType.Start) {

                    // if animator parameter is null or animator is not given then animation can't start so end here. 
                    if (animatorParameter == "" || Animator == null || Animator.gameObject.activeInHierarchy == false) {
                        yield break;
                    }


                    //if we are starting and stopping the animation then disable the IK
                    if (AnimationType == WeaponAnimationType.StartAndStop && abcEntity != null)
                        ABC_Utilities.mbSurrogate.StartCoroutine(abcEntity.ToggleIK(functionRequestTime, false));



                    switch (animatorParameterType) {
                        case AnimatorParameterType.Float:
                            Animator.SetFloat(animatorParameter, float.Parse(animatorOnValue));
                            break;
                        case AnimatorParameterType.integer:
                            Animator.SetInteger(animatorParameter, int.Parse(animatorOnValue));
                            break;
                        case AnimatorParameterType.Bool:
                            if (animatorOnValue != "True" && animatorOnValue != "False") {
                                Debug.Log("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                            }
                            Animator.SetBool(animatorParameter, bool.Parse(animatorOnValue));
                            break;

                        case AnimatorParameterType.Trigger:
                            Animator.SetTrigger(animatorParameter);
                            break;
                    }
                }


                //wait for delay then end animation (if set to start and stop in one call)
                if (animatorDuration > 0 && AnimationType == WeaponAnimationType.StartAndStop)
                    yield return new WaitForSeconds(animatorDuration);


                //Stop animation if set too
                if (AnimationType == WeaponAnimationType.StartAndStop || AnimationType == WeaponAnimationType.Stop) {

                    // if animator parameter is null or animator is not given then animation can't start so end here. 
                    if (animatorParameter == "" || Animator == null || animatorOffValue == "") {
                        yield break;
                    }



                    switch (animatorParameterType) {
                        case AnimatorParameterType.Float:
                            Animator.SetFloat(animatorParameter, float.Parse(animatorOffValue));
                            break;
                        case AnimatorParameterType.integer:
                            Animator.SetInteger(animatorParameter, int.Parse(animatorOffValue));
                            break;
                        case AnimatorParameterType.Bool:
                            if (animatorOffValue != "True" && animatorOffValue != "False") {
                                Debug.Log("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                            }
                            Animator.SetBool(animatorParameter, bool.Parse(animatorOffValue));
                            break;

                        case AnimatorParameterType.Trigger:
                            // don't need to switch off as trigger does that straight away anyway.
                            break;
                    }

                    //animation has ended so enable the IK
                    if (AnimationType == WeaponAnimationType.StartAndStop && abcEntity != null)
                        ABC_Utilities.mbSurrogate.StartCoroutine(abcEntity.ToggleIK(functionRequestTime, true));

                }

            }

            /// <summary>
            /// Will handle the enable/disable of abilities when this weapon is equipped, depending on setup 
            /// </summary>
            /// <param name="WeaponToggleType">Determines if the weapon is being enabled or disabled</param>
            /// <param name="Originator">Entity the weapon is attached too</param>
            private void AbilitiesToggleHandler(ABC_IEntity Originator, WeaponState WeaponToggleType) {


                //Define lists to enable abilities from
                List<int> enableAbilitiesID = this.enableAbilityIDs.ToList();
                List<string> enableAbilitiesName = new List<string>();

                //Cycle through the soft text list adding through int or strings to enable abilities
                foreach (string abilityRef in this.enableAbilityStrings) {

                    if (int.TryParse(abilityRef, out int tryInt) == true)
                        enableAbilitiesID.Add(int.Parse(abilityRef));
                    else
                        enableAbilitiesName.Add(abilityRef);

                }

                //Define lists to disable abilities from
                List<int> disableAbilitiesID = this.disableAbilityIDs.ToList();
                List<string> disableAbilitiesName = new List<string>();

                //Cycle through the soft text list adding through int or strings to disable abilities
                foreach (string abilityRef in this.disableAbilityStrings) {

                    if (int.TryParse(abilityRef, out int tryInt) == true)
                        disableAbilitiesID.Add(int.Parse(abilityRef));
                    else
                        disableAbilitiesName.Add(abilityRef);

                }


                switch (WeaponToggleType) {
                    case WeaponState.Equip:

                        //If we have set to enable abilities when weapon is equipped
                        if (this.enableAbilitiesWhenEquipped) {


                            //Cycle through each ability ID enabling the ability
                            foreach (int abilityID in enableAbilitiesID)
                                Originator.EnableAbility(abilityID);

                            //Cycle through each ability name enabling the ability
                            foreach (string abilityName in enableAbilitiesName)
                                Originator.EnableAbility(abilityName);


                            //If set to disable any other ability not listed
                            if (this.disableAllOtherAbilitiesNotListed) {

                                //Cycle through all the abilities returning a list of ID's which don't match those set to be enabled with this weapon
                                foreach (int abilityID in Originator.CurrentAbilities.Where(a => enableAbilitiesID.Contains(a.abilityID) == false && enableAbilitiesName.Contains(a.name) == false).Select(a => a.abilityID).ToList()) {
                                    Originator.DisableAbility(abilityID);
                                }

                            } else if (this.disableAllAbilitiesLinkedToOtherWeapons) {

                                //Cycle through all the abilities returning a list of ID's which don't match those set to be enabled with this weapon
                                foreach (int abilityID in Originator.CurrentAbilities.Where(a => enableAbilitiesID.Contains(a.abilityID) == false && enableAbilitiesName.Contains(a.name) == false).Select(a => a.abilityID).ToList()) {

                                    //Disable ability if another weapon is set to enable it 
                                    if (Originator.AllWeapons.Where(w => w.enableAbilityIDs.Contains(abilityID) == true || w.enableAbilityStrings.Contains(abilityID.ToString()) == true || w.enableAbilityStrings.Contains(Originator.CurrentAbilities.Where(a => a.abilityID == abilityID).FirstOrDefault().name) == true).ToList().Count() > 0) {
                                        Originator.DisableAbility(abilityID);
                                    }
                                }

                            }

                        }

                        //If we have set to disable abilities when weapon is equipped
                        if (this.disableAbilitiesWhenEquipped) {


                            //Cycle through each ability ID disabling the ability
                            foreach (int abilityID in disableAbilitiesID)
                                Originator.DisableAbility(abilityID);


                            //Cycle through each ability name enabling the ability
                            foreach (string abilityName in disableAbilitiesName)
                                Originator.DisableAbility(abilityName);


                            //If set to enable any other ability not listed
                            if (this.enableAllOtherAbilitiesNotListed) {

                                //Cycle through all the abilities returning a list of ID's which don't match those set to be disabled with this weapon
                                foreach (int abilityID in Originator.CurrentAbilities.Where(a => disableAbilitiesID.Contains(a.abilityID) == false || disableAbilitiesName.Contains(a.name) == false).Select(a => a.abilityID).ToList()) {
                                    Originator.EnableAbility(abilityID);
                                }

                            }

                        }
                        break;

                    case WeaponState.UnEquip:
                        //For now nothing happens with ability groups when the weapon is disabled
                        return;

                    case WeaponState.Drop:

                        if (this.weaponDropActionApplyToWeaponEnableAbilities == true) {

                            //Cycle through each ability ID applying the weapon action (disable or delete)
                            foreach (int abilityID in enableAbilitiesID) {

                                if (this.weaponDropAction == WeaponDropAction.DisableWeapon) //disable ability
                                    Originator.DisableAbility(abilityID);
                                else if (this.weaponDropAction == WeaponDropAction.DeleteWeapon) // delete ability
                                    Originator.DeleteAbility(abilityID);

                            }

                            //Cycle through each ability ID applying the weapon action (disable or delete)
                            foreach (string abilityName in enableAbilityStrings) {

                                if (this.weaponDropAction == WeaponDropAction.DisableWeapon) //disable ability
                                    Originator.DisableAbility(abilityName);
                                else if (this.weaponDropAction == WeaponDropAction.DeleteWeapon) // delete ability
                                    Originator.DeleteAbility(abilityName);

                            }

                        }

                        break;

                }

            }

            /// <summary>
            /// Will handle the enable/disable of ability groups when this weapon is equipped, depending on setup 
            /// </summary>
            /// <param name="WeaponToggleType">Determines if the weapon is being enabled or disabled</param>
            /// <param name="Originator">Entity the weapon is attached too</param>
            private void AbilityGroupToggleHandler(ABC_IEntity Originator, WeaponState WeaponToggleType) {

                switch (WeaponToggleType) {

                    case WeaponState.Equip:

                        //If we have set to enable ability groups when weapon is equipped
                        if (this.enableAbilityGroupsWhenEquipped) {

                            //Cycle through each group ID enabling the group
                            foreach (int groupID in this.enableAbilityGroupIDs)
                                Originator.ToggleAbilityGroup(groupID, true);

                            //Cycle through each group name enabling the group
                            foreach (string groupName in this.enableAbilityGroupNames)
                                Originator.ToggleAbilityGroup(groupName, true);

                            //If set to disable any other group not listed
                            if (this.disableAllOtherGroupsNotListed) {

                                //Cycle through all the groups returning a list of ID's which don't match those set to be enabled with this weapon
                                foreach (int groupID in Originator.AllAbilityGroups.Where(g => this.enableAbilityGroupIDs.Contains(g.groupID) == false && this.enableAbilityGroupNames.All(w => g.groupName.Trim().ToUpper() != w.Trim().ToUpper())).Select(g => g.groupID).ToList()) {
                                    Originator.ToggleAbilityGroup(groupID, false);
                                }

                            }

                        }

                        //If we have set to disable ability groups when weapon is equipped
                        if (this.disableAbilityGroupWhenEquipped) {

                            //Cycle through each group ID disabling the group
                            foreach (int groupID in this.disableAbilityGroupIDs)
                                Originator.ToggleAbilityGroup(groupID, false);


                            //Cycle through each group name disabling the group
                            foreach (string groupName in this.disableAbilityGroupNames)
                                Originator.ToggleAbilityGroup(groupName, false);


                            //If set to enable any other group not listed
                            if (this.enableAllOtherGroupsNotListed) {

                                //Cycle through all the groups returning a list of ID's which don't match those set to be disable with this weapon
                                foreach (int groupID in Originator.AllAbilityGroups.Where(g => this.enableAbilityGroupIDs.Contains(g.groupID) == false && this.enableAbilityGroupNames.All(w => g.groupName.Trim().ToUpper() != w.Trim().ToUpper())).Select(g => g.groupID).ToList()) {
                                    Originator.ToggleAbilityGroup(groupID, true);
                                }

                            }

                        }

                        break;
                    case WeaponState.UnEquip:
                        //For now nothing happens with ability groups when the weapon is disabled
                        return;
                    case WeaponState.Drop:

                        if (this.weaponDropActionApplyToAssignedGroups == true) {

                            //Cycle through each group ID disabling or deleting the group
                            foreach (int groupID in this.enableAbilityGroupIDs) {
                                if (this.weaponDropAction == WeaponDropAction.DisableWeapon)
                                    Originator.ToggleAbilityGroup(groupID, false);
                                else if (this.weaponDropAction == WeaponDropAction.DeleteWeapon)
                                    Originator.DeleteAbilityGroup(groupID);

                            }

                            //Cycle through each group name disabling or deleting the group
                            foreach (string groupName in this.enableAbilityGroupNames) {
                                if (this.weaponDropAction == WeaponDropAction.DisableWeapon)
                                    Originator.ToggleAbilityGroup(groupName, false);
                                else if (this.weaponDropAction == WeaponDropAction.DeleteWeapon)
                                    Originator.DeleteAbilityGroup(groupName);

                            }



                        }


                        break;

                }



            }

            /// <summary>
            /// Will handle the enable/deletion of Icon UI when this weapon is toggled 
            /// </summary>
            /// <param name="WeaponToggleType">Determines if the weapon is being enabled or disabled</param>
            /// <param name="Originator">Entity the weapon is attached too</param>
            private void UIToggleHandler(ABC_IEntity Originator, WeaponState WeaponToggleType) {

                switch (WeaponToggleType) {

                    case WeaponState.Equip:


                        RawImage ImageGUI = Originator.weaponImageGUI;

                        //If no gui setup return here
                        if (ImageGUI == null)
                            return;

                        ImageGUI.enabled = true;

                        // if our current scroll ability has an icon image
                        if (this.weaponIconImage.Texture2D != null) {
                            // set it to the image GUI
                            ImageGUI.texture = this.weaponIconImage.Texture2D;

                            // set it to be minorally transparent
                            Color c = ImageGUI.color;
                            c.a = 0.7f;
                            ImageGUI.color = c;
                        } else {
                            // no image to display so we will hide the image gui for now 
                            Color c = ImageGUI.color;
                            c.a = 0f;
                            ImageGUI.color = c;
                        }

                        break;
                    case WeaponState.UnEquip:
                        // if we are turning off then just turn off the texture and make transparent 
                        RawImage imageGUI = Originator.weaponImageGUI;

                        //If no gui setup return here
                        if (imageGUI == null)
                            return;

                        imageGUI.texture = null;
                        imageGUI.enabled = false;
                        break;
                    case WeaponState.Drop:

                        if (this.weaponDropActionApplyToAssignedUI == true) {

                            //Cycle through each group ID disabling or deleting the group
                            foreach (int iconID in Originator.FindIconsLinkedToWeapon(this.weaponID)) {

                                if (this.weaponDropAction == WeaponDropAction.DeleteWeapon)
                                    Originator.DeleteIconUI(iconID);

                            }



                        }


                        break;

                }

            }

            /// <summary>
            /// Will work out and set the current weapon clip count if not already set
            /// </summary>
            private void SetWeaponAmmoClipCount() {

                //If clip count is -1 then it's not been set yet. We will find out the starting clip count using Mod if we have starting ammo
                if (this.useWeaponReload && this.currentWeaponAmmoClipCount == -1) {

                    //No division by 0
                    if (this.weaponClipSize == 0)
                        this.weaponClipSize = 1;

                    //If we have no ammo then can just set current ammo clip count to 0
                    if (this.weaponAmmoCount == 0) {
                        this.currentWeaponAmmoClipCount = 0;
                    } else if (this.weaponAmmoCount >= this.weaponClipSize && this.weaponReloadFillClip) {
                        //If reload fill clip is true then set current clip count to clipsize
                        this.currentWeaponAmmoClipCount = this.weaponClipSize;
                    } else {
                        //Find current clip using mod on ammo count and clip size
                        this.currentWeaponAmmoClipCount = this.weaponAmmoCount % this.weaponClipSize;


                        //If clipcount is 0 due to mod calculation then make count equal to clipsize 
                        if (currentWeaponAmmoClipCount == 0)
                            currentWeaponAmmoClipCount = weaponClipSize;
                    }


                    // remove current clip count from ammo (it's not in our ammo stock as it's current in use in our clip) 
                    this.weaponAmmoCount -= this.currentWeaponAmmoClipCount;

                }
            }

            /// <summary>
            /// Will change the GUI Text provided showing how much ammo the weapon currently has.
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <param name="Enabled">If true will display ammo information, else will hide the display</param>
            private void DisplayAmmoOnGUI(ABC_IEntity Originator, bool Enabled = true) {

                ABC_TextReference AmmoGUIText = Originator.weaponAmmoGUI;

                //If no GUI text then end function here 
                if (AmmoGUIText.Graphic == null)
                    return;

                //If weapon is not enabled, has no ammo or is not a scroll ability then turn the GUI text off and end here
                if (AmmoGUIText != null && (this.IsEnabled() == false || this.UseWeaponAmmo == false)) {
                    AmmoGUIText.Graphic.enabled = false;
                    return;
                }


                //Enable GUI 
                AmmoGUIText.Graphic.enabled = true;

                //If we can reload then show clip and max ammo, else just show ammo
                if (this.useWeaponReload == true) {
                    AmmoGUIText.text = this.currentWeaponAmmoClipCount + " | " + this.weaponAmmoCount;
                } else {
                    AmmoGUIText.text = this.weaponAmmoCount.ToString();
                }

            }


            #endregion



            // ************************** Public Methods *************************************

            #region Public Methods


            /// <summary>
            /// Constructor mainly used by inspector
            /// </summary>
            /// <param name="ID">Unique ID of the weapon</param>
            public Weapon() {
            }


            /// <summary>
            /// Returns a dictionary detailing information about the weapon including name, description etc
            /// </summary>
            /// <returns>Dictionary holding information regarding the weapon</returns>
            public Dictionary<string, string> GetWeaponDetails(ABC_IEntity Originator) {

                Dictionary<string, string> retval = new Dictionary<string, string>();

                retval.Add("Name", this.weaponName);
                retval.Add("ID", this.weaponID.ToString());
                retval.Add("Description", this.weaponDescription);

                return retval;

            }

            /// <summary>
            /// Will create graphics setup for the weapon
            /// </summary>
            public void CreateObjectPools() {

                //Look through each weapon graphic pooling the object
                foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                    weaponGraphic.CreateGraphicObject();
                }

                //Create weapon drop object if it exists
                this.CreateWeaponDropObject();

                //create weapon reload object 
                this.CreateWeaponReloadingObjects();

                //create weapon bock effect pool
                this.CreateWeaponParryEffectObjects();

                //create weapon bock effect pool
                this.CreateWeaponBlockEffectObjects();

            }

            public void ClearObjectPools() {

                //Look through each weapon graphic pooling the object
                foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                    weaponGraphic.ClearGraphicObject();
                }

                //clear weapon drop object if it exists
                this.ClearWeaponDropObject();

                //clear weapon reload pool
                this.weaponReloadPool.Clear();

                //clear weapon parry effect pool
                this.weaponParryEffectPool.Clear();

                //clear weapon block effect pool
                this.weaponBlockEffectPool.Clear();

            }

            /// <summary>
            /// Will disable and pool all the weapons graphic objects
            /// </summary>
            public void DisableWeaponGraphics() {
                //Look through each weapon graphic pooling the object
                foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                    weaponGraphic.DisableGraphicObject();
                }
            }

            /// <summary>
            /// Will return all the in game active weapon graphics relating to this weapon (i.e the object that is equipped/unequipped)
            /// </summary>
            /// <returns>List of gameobject weapon graphics</returns>
            public List<GameObject> GetWeaponGraphics() {

                List<GameObject> retval = new List<GameObject>();

                foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                    retval.Add(weaponGraphic.GetWeaponPoolObject());

                }

                return retval;
            }


            /// <summary>
            /// Will return a list of all ABC Animation runner components relating to the weapons object graphics
            /// </summary>
            /// <returns>List of ABC Animation Runner Components</returns>
            public List<ABC_AnimationsRunner> GetWeaponAnimationRunners() {

                List<ABC_AnimationsRunner> retVal = new List<ABC_AnimationsRunner>();

                //Load the weapon graphics
                if (this.weaponGraphics != null && this.weaponGraphics.Count() > 0) {
                    foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                        ABC_AnimationsRunner AniRunner = weaponGraphic.GetAnimationRunner();

                        if (AniRunner != null)
                            retVal.Add(AniRunner);
                    }
                }

                return retVal;

            }

            /// <summary>
            /// Will return a list of all Animator components relating to the weapons object graphics
            /// </summary>
            /// <returns>List of ABC Animation Runner Components</returns>
            public List<Animator> GetWeaponAnimators() {

                List<Animator> retVal = new List<Animator>();

                //Load the weapon graphics
                if (this.weaponGraphics != null && this.weaponGraphics.Count() > 0) {
                    foreach (WeaponObj weaponGraphic in this.weaponGraphics) {

                        Animator animator = weaponGraphic.GetAnimator();

                        if (animator != null)
                            retVal.Add(animator);
                    }
                }

                return retVal;

            }

            /// <summary>
            /// Will convert the clip provided into an override clip if criteria is met (i.e clip name matches one of the overrides).
            /// </summary>
            /// <param name="animationClip">Clip to convert into an override clip</param>
            /// <returns>The converted override clip if the criteria is met else null</returns>
            public AnimatorClipRunnerOverride ConvertClipIntoOverrideClip(AnimationClip AnimationClip) {

                //If overrides is not turned on then end here
                if (this.useWeaponAnimatorOverrides == false)
                    return null;


                foreach (AnimatorClipRunnerOverride clipOverride in this.weaponAnimatorClipRunnerOverrides) {

                    //If any part of the name of the animation clip playing matches the list of animation names to override, then return the override clip

                    if (clipOverride.animatorClipNamesToOverride.Any(c => AnimationClip.name.Contains(c))) {
                        return clipOverride;
                    }
                }


                //No override found so just return the original clip (no conversion) 
                return null;


            }


            /// <summary>
            /// Returns a bool indicating if the weapon is enabled and able to be equipped
            /// </summary>
            /// <returns>True if the weapon is enabled, else false</returns>
            public bool IsEnabled() {

                //If weapon is enabled then return true
                if (this.weaponEnabled == true) {
                    return true;
                } else {
                    return false;
                }

            }

            /// <summary>
            /// Will enable the weapon making it avaliable in game. 
            /// </summary>
            /// <param name="Delay">Delay until weapon is enabled</param>
            /// <param name="Originator">Entity that the weapon is attached too</param>
            /// <param name="RunSetup">If true then setup will be called ensuring that a weapon and scroll ability is equipped</param>
            /// <param name="EquipWeapon">If true then the weapon enabled will be equipped</param>
            public IEnumerator Enable(float delay = 0f, ABC_IEntity Originator = null, bool RunSetup = true, bool EquipWeapon = false) {

                //If delay is given then wait before weapons enabled
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);

                //Enable ability 
                this.weaponEnabled = true;

                //Create the object pool if it doesn't already exist
                this.CreateObjectPools();

                //if running setup
                if (RunSetup)
                    Originator.SetupWeaponsAndScrollAbilities(EquipWeapon ? this.weaponID : -1);


            }

            /// <summary>
            /// Will disable the weapon making it unavaliable in game. 
            /// </summary>
            /// <param name="Originator">Entity that the weapon is attached too</param>
            /// <param name="RunSetup">If true then setup will be called ensuring that a weapon and scroll ability is equipped</param>
            public void Disable(ABC_IEntity Originator = null, bool RunSetup = true) {

                //disable ability 
                this.weaponEnabled = false;

                //Disable weapon graphics
                this.DisableWeaponGraphics();

                //if running setup
                if (RunSetup)
                    Originator.SetupWeaponsAndScrollAbilities();

            }

            /// <summary>
            /// Returns the key/button which will equip the weapon
            /// </summary>
            /// <returns>String of the key or button which will equip the weapon, blank is returned if weapon is not set to be equiped via key input</returns>
            public string GetEquipKey() {

                string retVal = "";

                //If a button/key has been triggered to enable this weapon then return the key/button string, else blank string
                if (this.enableWeaponOnInput == true) {
                    if (this.weaponEnableInputType == InputType.Key)
                        retVal = this.weaponEnableKey.ToString();
                    else
                        retVal = this.weaponEnableButton.ToString();
                } else {
                    retVal = "";
                }

                return retVal;

            }

            /// <summary>
            /// Returns a bool indicating if the weapon has been triggered to be equipped 
            /// </summary>
            /// <param name="Originator">Entity that activated the ability</param>
            /// <returns>True if weapon has been triggered to be equipped (from key press etc)</returns>
            public bool EquipWeaponTriggered(ABC_IEntity Originator) {

                if (this.IsEnabled() == false)
                    return false;

                //If equip weapon has been clicked in the inspector then return true
                if (this.inspectorEquipWeapon == true) {
                    this.inspectorEquipWeapon = false;
                    return true;
                }

                //If a button/key has been triggered to enable this weapon then return true, else false
                if (this.enableWeaponOnInput == true && this.ButtonPressed(Originator, WeaponButtonPressState.EnableWeapon))
                    return true;
                else
                    return false;

            }


            /// <summary>
            /// will toggle the weapon either enabling/equipping or disabling/unequipping the weapon
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <param name="WeaponToggleType">Determines if the weapon should be enabled or disabled</param>
            /// <param name="QuickToggle">True if this is a quick toggle which means weapon will equip/unequip instantly</param> 
            public IEnumerator ToggleWeapon(ABC_IEntity Originator, WeaponState WeaponToggleType, bool QuickToggle = false) {

                //If reloading and disabling weapon then interrupt the reload
                if (WeaponToggleType == WeaponState.UnEquip && this.weaponIsReloading)
                    this.weaponReloadInterrupted = true;


                //Ensure the weapon clip count has been set if required 
                this.SetWeaponAmmoClipCount();

                //How long it will take for the weapon to toggle on/off
                float waitDuration = 1f;

                //Animations
                AnimationClip animationClip = null;
                bool animationRunnerOnEntity = false;
                bool animationRunnerOnScrollGraphic = false;
                bool animationRunnerOnWeapon = false;

                string animatorParameter = null;
                bool animateOnEntity = false;
                bool animateOnScrollGraphic = false;
                bool animateOnWeapon = false;

                //Fill out variables with correct data depending on if we are enabling or disabling the weapon
                switch (WeaponToggleType) {
                    case WeaponState.Equip:

                        waitDuration = this.enableDuration;

                        animationClip = this.weaponEnableAnimationRunnerClip.AnimationClip;
                        animationRunnerOnEntity = this.weaponEnableAnimationRunnerOnEntity;
                        animationRunnerOnScrollGraphic = this.weaponEnableAnimationRunnerOnScrollGraphic;
                        animationRunnerOnWeapon = this.weaponEnableAnimationRunnerOnWeapon;

                        animatorParameter = this.weaponEnableAnimatorParameter;
                        animateOnEntity = this.weaponEnableAnimateOnEntity;
                        animateOnScrollGraphic = this.weaponEnableAnimateOnScrollGraphic;
                        animateOnWeapon = this.weaponEnableAnimateOnWeapon;



                        break;

                    case WeaponState.UnEquip:

                        waitDuration = this.disableDuration;

                        animationClip = this.weaponDisableAnimationRunnerClip.AnimationClip;
                        animationRunnerOnEntity = this.weaponDisableAnimationRunnerOnEntity;
                        animationRunnerOnScrollGraphic = this.weaponDisableAnimationRunnerOnScrollGraphic;
                        animationRunnerOnWeapon = this.weaponDisableAnimationRunnerOnWeapon;

                        animatorParameter = this.weaponDisableAnimatorParameter;
                        animateOnEntity = this.weaponDisableAnimateOnEntity;
                        animateOnScrollGraphic = this.weaponDisableAnimateOnScrollGraphic;
                        animateOnWeapon = this.weaponDisableAnimateOnWeapon;

                        break;
                }

                //If using animator overrides then update (if equipping) or clear (if unequipping) the animation runner component overrides 
                if (this.useWeaponAnimatorOverrides) {

                    List<ABC_Controller.AnimatorClipRunnerOverride> ClipOverrides = new List<AnimatorClipRunnerOverride>();

                    switch (WeaponToggleType) {
                        case WeaponState.Equip:

                            //adjust speed
                            Originator.SetWeaponMovementSpeed(this.weaponMoveSpeedModification);

                            //If using GC states to override
                            if (this.overrideWithGCCharacterState == true) {

#if ABC_GC_Integration
                            //Get Character Animator
                            GameCreator.Characters.CharacterAnimator gcAnimator = Originator.transform.GetComponentInChildren<GameCreator.Characters.CharacterAnimator>();

                            //If animator exists then add the equip state
                            if (gcAnimator != null && this.gcEquipState != null)
                                gcAnimator.ResetControllerTopology(this.gcEquipState.rtcLocomotion);
#endif



#if ABC_GC_2_Integration

                            //equip GC2 State                            
                                Originator.SetGC2State(this.gc2EquipState);

                            if ((this.gc2UnEquipState != null || this.gc2EquipState != null) && this.GCCharacterStateDisableABCEquipAnimations == true)
                                animationClip = null;  

#endif




                            } else {

                                //Add to Entity if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnEntity == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    Originator.animationRunner.AddAnimatorClipOverrides(ClipOverrides);

                                //Add to Scroll Graphic if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnScrollGraphic == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner().AddAnimatorClipOverrides(ClipOverrides);

                                //Add to Weapon Objects if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnWeapon == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    this.GetWeaponAnimationRunners().ForEach(ar => ar.AddAnimatorClipOverrides(ClipOverrides));
                            }

                            break;

                        case WeaponState.UnEquip:

                            //Reset speed
                            Originator.SetWeaponMovementSpeed(0);

                            //If using GC states to override
                            if (this.overrideWithGCCharacterState == true) {

#if ABC_GC_Integration
                            //Get Character Animator
                            GameCreator.Characters.CharacterAnimator gcAnimator = Originator.transform.GetComponentInChildren<GameCreator.Characters.CharacterAnimator>();

                            //If animator exists then add the unequip state
                            if (gcAnimator != null && this.gcUnEquipState != null)
                                gcAnimator.ResetControllerTopology(this.gcUnEquipState.rtcLocomotion);
#endif

#if ABC_GC_2_Integration

                            //unequip GC 2 state                            
                            Originator.SetGC2State(this.gc2UnEquipState);

                            if ((this.gc2UnEquipState != null || this.gc2EquipState != null) && this.GCCharacterStateDisableABCEquipAnimations == true)
                                animationClip = null;
#endif



                            } else {

                                //Remove from Entity if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnEntity == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    Originator.animationRunner.ClearAnimatorClipOverrides();

                                //Remove from Scroll Graphic if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnScrollGraphic == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner().ClearAnimatorClipOverrides();

                                //Remove from Weapon Objects if setup too
                                ClipOverrides = this.weaponAnimatorClipRunnerOverrides.Where(c => c.animationRunnerOnWeapon == true).ToList();

                                if (ClipOverrides.Count > 0)
                                    this.GetWeaponAnimationRunners().ForEach(ar => ar.ClearAnimatorClipOverrides());

                            }

                            break;
                    }



                }


                //Load the weapon graphics
                foreach (WeaponObj weaponGraphic in this.weaponGraphics) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(weaponGraphic.ToggleGraphic(Originator, WeaponToggleType, QuickToggle));
                }

                //If using weapon animations
                if (this.useWeaponEquipAnimations == true) {

                    //Start animation runner
                    if (animationClip != null && QuickToggle == false) {
                        if (animationRunnerOnEntity)
                            this.StartAndStopAnimationRunner(WeaponToggleType, Originator.animationRunner);

                        if (animationRunnerOnScrollGraphic)
                            this.StartAndStopAnimationRunner(WeaponToggleType, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner());

                        if (animationRunnerOnWeapon)
                            this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponToggleType, ar));

                    }

                    // start animator
                    if (animatorParameter != "") {

                        if (animateOnEntity)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponToggleType, Originator.animator));

                        if (animateOnScrollGraphic)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponToggleType, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator()));

                        if (animateOnWeapon)
                            this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponToggleType, a)));
                    }

                }


                //temporarily turn off movement for the duration of the switch
                if (this.weaponSwitchTemporarilyDisableMovement)
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.StopMovementForDuration(waitDuration, true, this.weaponSwitchDisableMovementFreezePosition, this.weaponSwitchDisableMovementDisableComponents));

                //temporarily turn off ability activation 
                if (this.weaponSwitchTemporarilyDisableAbilityActivation)
                    Originator.TemporarilyDisableAbilityActivation(this.disableAbilityActivationDuration);


                //wait for the duration so graphic and animation can finish appearing unless this is a quick toggle
                // quick toggle = weapon will appear instantly so no wait needed
                if (QuickToggle == false)
                    yield return new WaitForSeconds(waitDuration);

                //if equipping then set weapon IK
                if (WeaponToggleType == WeaponState.Equip) {

                    //If left hand type is not none then set object or set by tag
                    if (this.weaponLeftHandIKTargetType != WeaponIKTargetType.None)
                        Originator.SetIKTarget(AvatarIKGoal.LeftHand, weaponLeftHandIKTargetType == WeaponIKTargetType.OnObject ? this.weaponLeftHandIKOnObject.GameObject?.transform : ABC_Utilities.TraverseObjectForTag(Originator, this.weaponLeftHandIKOnTag)?.transform, this.weaponLeftHandIKWeight, this.weaponLeftHandIKTransitionSpeed);

                    //If right hand type is not none then set object or set by tag
                    if (this.weaponRightHandIKTargetType != WeaponIKTargetType.None)
                        Originator.SetIKTarget(AvatarIKGoal.RightHand, weaponRightHandIKTargetType == WeaponIKTargetType.OnObject ? this.weaponRightHandIKOnObject.GameObject?.transform : ABC_Utilities.TraverseObjectForTag(Originator, this.weaponRightHandIKOnTag)?.transform, this.weaponRightHandIKWeight, this.weaponRightHandIKTransitionSpeed);


                } else if (WeaponToggleType == WeaponState.UnEquip) {
                    //If unequipping then remove both IK targets
                    Originator.RemoveIKTarget(AvatarIKGoal.LeftHand);
                    Originator.RemoveIKTarget(AvatarIKGoal.RightHand);

                }



                //Enable/Disable any ability groups set up to be enabled/disabled when this weapon is equipped
                this.AbilityGroupToggleHandler(Originator, WeaponToggleType);

                //Enable/disable any abilities set up to be enabled/disabled when this weapon is equipped
                this.AbilitiesToggleHandler(Originator, WeaponToggleType);

                //Enable/Disable toggle objects 
                if (this.weaponToggleObjectsOnEquipStateChange) {

                    foreach (string objName in this.weaponToggleGameObjectNames) {

                        //Retval 
                        GameObject obj = null;

                        //try and find object on entity first 
                        obj = Originator.transform.Find(objName).gameObject;

                        //if not found then look in scene
                        if (obj == null)
                            obj = GameObject.Find(objName).gameObject;

                        //If not set then continue in loop
                        if (obj == null)
                            continue;

                        if (WeaponToggleType == WeaponState.Equip)
                            obj.SetActive(true);
                        else if (WeaponToggleType == WeaponState.UnEquip)
                            obj.SetActive(false);
                    }

                }



                //If a reload is required (clip = 0 or reload was interrupted) then reload gun
                if (WeaponToggleType == WeaponState.Equip && this.weaponReloadRequired && this.autoReloadWeaponWhenRequired)
                    ABC_Utilities.mbSurrogate.StartCoroutine(ReloadWeaponAmmo(Originator));


                //Show or hide UI 
                this.UIToggleHandler(Originator, WeaponToggleType);
                // display ammo gui if equipping else hide
                this.DisplayAmmoOnGUI(Originator, WeaponToggleType == WeaponState.Equip ? true : false);


            }

            /// <summary>
            /// Will activate weapon parry, activating animations and setting on/off the parry flag
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            public IEnumerator ActivateWeaponParry(ABC_IEntity Originator) {

                //If not parrying or already parrying then end here
                if (this.enableWeaponParry == false || this.isWeaponParrying == true)
                    yield break;

                //turn on weapon parry status after delay 
                if (this.weaponParryDelay > 0f)
                    yield return new WaitForSeconds(this.weaponParryDelay);

                //set parry on
                this.isWeaponParrying = true;

                //If using weapon animations
                if (this.useWeaponParryAesthetics == true) {

                    //Start animation runner
                    if (weaponParryAnimationRunnerClip != null) {
                        if (weaponParryAnimationRunnerOnEntity)
                            this.StartAndStopAnimationRunner(WeaponState.Parry, Originator.animationRunner, WeaponAnimationType.StartAndStop);

                        if (weaponParryAnimationRunnerOnScrollGraphic)
                            this.StartAndStopAnimationRunner(WeaponState.Parry, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.StartAndStop);

                        if (weaponParryAnimationRunnerOnWeapon)
                            this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.Parry, ar, WeaponAnimationType.StartAndStop));

                    }

                    // start animator
                    if (weaponParryAnimatorParameter != "") {

                        if (weaponParryAnimateOnEntity)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Parry, Originator.animator, WeaponAnimationType.StartAndStop));

                        if (weaponParryAnimateOnScrollGraphic)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Parry, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.StartAndStop));

                        if (weaponParryAnimateOnWeapon)
                            this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Parry, a, WeaponAnimationType.StartAndStop)));
                    }

                }


                //turn off after duration 
                yield return new WaitForSeconds(this.weaponParryDuration);

                //set parry off
                this.isWeaponParrying = false;


                //stop parrying for a duration (cooldown) so can't be spammed after parry has finished
                Originator.PreventWeaponParryForDuration(this.weaponParryCooldown);



            }

            /// <summary>
            /// Function determines if the weapon is currently parrying 
            /// </summary>
            /// <returns>True if weapon is currently parrying, else false</returns>
            public bool IsWeaponParrying() {

                return this.isWeaponParrying;

            }

            /// <summary>
            /// Will activate the parry handler which will play animations and perform events which happen on a successful parry (enabling abilities etc)
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <param name="ParriedEntity">Entity that had an ability parried</param>
            /// <param name="AbilityType">The type of ability that was blocked</param>
            /// <param name="AbilityHitPoint">The vector position where the ability collided</param>
            /// <param name="IgnoreWeaponParry">(Optional)If True then the weapon parry will be ignored</param>
            /// <returns>A float indicating how long the block reaction is playing for, returns -1 if the block failed</returns>
            public bool ActivateParryHandler(ABC_IEntity Originator, ABC_IEntity ParriedEntity, AbilityType AbilityType, Vector3 AbilityHitPoint = default(Vector3), bool IgnoreWeaponParry = false) {

                //If ability isn't a melee type or ability is set to ignore weapon parrying or entity is not currently parrying then end here returning false 
                if (IgnoreWeaponParry == true || this.isWeaponParrying == false || AbilityType != AbilityType.Melee)
                    return false;


                //if entity is required to face the incoming ability to parry 
                if (this.weaponParryFaceAbilityRequired == true) {
                    var dir = (AbilityHitPoint - Originator.transform.position).normalized;
                    var dot = Vector3.Dot(dir, Originator.transform.forward);

                    //Note: higher the value the more accuracy needed to face
                    if (dot < 0.2)
                        return false;


                } else if (this.weaponParryTurnToAbilityHitPoint && AbilityHitPoint != default(Vector3)) {
                    //If set to turn to ability hit point and hit point is not default then turn to the direction of the attacker

                    //use originator y vector so its a turns too keeping same height
                    Vector3 lookDirection = ParriedEntity.transform.position;
                    lookDirection.y = Originator.transform.position.y;
                    Originator.LookAt(lookDirection);
                }



                //show parry graphics
                if (this.useWeaponParryAesthetics == true) {

                    //play graphic
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateGraphic(Originator, WeaponGraphicType.WeaponParryEffect));

                }


                //stop entity from attacking as we have successfully parried
                ParriedEntity.InterruptAbilityActivation();
                //Stagger the enemy
                ParriedEntity.HitRestrictAbilityActivation(2f);
                ParriedEntity.HitStopMovement();
                ParriedEntity.ActivateCurrentEquippedWeaponMeleeAttackBlockedReaction();



                //Enable any abilities set to enable after parrying 
                if (this.enableAbilitiesAfterParrying) {
                    //Loop through all abilities to enable 
                    foreach (int abilityID in this.abilityIDsToEnableAfterParrying) {
                        Originator.EnableAbilityForDuration(abilityID, this.enableAbilitiesAfterParryingDuration);
                    }

                }

                //If activating ability after parry 
                if (this.activateAbilityAfterParrying)
                    Originator.TriggerAbility(this.abilityIDToActivateAfterParrying);


                //Return true as melee ability was parried
                return true;

            }


            /// <summary>
            /// Will toggle weapon block state
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <param name="State">The weapon state, should either be Block or Unblock </param>
            /// <param name="Refresh">If refresh is provided then only animations will update </param>
            /// <returns>True if weapon block toggle was successful</returns>
            public void ToggleWeaponBlock(ABC_IEntity Originator, WeaponState State, bool Refresh = false) {

                //If weapon block has not been enabled then end here
                if (this.enableWeaponBlock == false)
                    return;

                switch (State) {
                    case WeaponState.Block:

                        //If refreshing then skip the adjustments so we can just update animation
                        if (Refresh == false) {

                            this.isWeaponBlocking = true;

                            //adjust prevent melee effects/melee mitigation damage
                            if (this.weaponBlockPreventMeleeEffects) {
                                Originator.TogglePreventMeleeEffects(true);
                            } else if (this.weaponBlockMitigateMeleeDamagePercentage > 0) {
                                Originator.AdjustMeleeDamageMitigationPercentage(this.weaponBlockMitigateMeleeDamagePercentage);
                            }


                            //adjust prevent melee effects/projectile and raycast mitigation damage
                            if (this.weaponBlockPreventProjectileEffects) {
                                Originator.TogglePreventProjectileAndRayCastEffects(true);
                            } else if (this.weaponBlockMitigateProjectileDamagePercentage > 0) {
                                Originator.AdjustProjectileMitigationPercentage(this.weaponBlockMitigateProjectileDamagePercentage);
                            }


                            //Modify Stats 
                            foreach (BlockStatModifications statMod in this.weaponBlockStatModifications) {

                                //Retrieve the stat modification value from the originator and/or target entity (uses the percentage configured - 70% of intelligence etc)
                                float statModification = 0;

                                if (statMod.modifyByPercentOrBaseValue == PercentOrBase.Percent)
                                    statModification = statMod.modificationValue / 100 * Originator.GetStatValue(statMod.statName, statMod.statIntegrationType);
                                else
                                    statModification = statMod.modificationValue;


                                //Get rid of Decimal 
                                statModification = (float)(int)statModification;


                                //Apply the modification to the potency depending on the operator setup
                                switch (statMod.arithmeticOperator) {
                                    case ArithmeticIncrDecrOperators.Increase:

                                        Originator.AdjustStatValue(statMod.statName, statModification, statMod.statIntegrationType, GCStatType.Stat);


                                        break;
                                    case ArithmeticIncrDecrOperators.Decrease:

                                        Originator.AdjustStatValue(statMod.statName, -statModification, statMod.statIntegrationType, GCStatType.Stat);

                                        break;
                                }

                                //record statModification to reverse when unblocking
                                statMod.finalStatValueModification = statModification;

                            }

                        }

                        //If using weapon animations
                        if (this.useWeaponBlockAesthetics == true) {


                            //Start animation runner
                            if (weaponBlockAnimationRunnerClip != null) {
                                if (weaponBlockAnimationRunnerOnEntity)
                                    this.StartAndStopAnimationRunner(State, Originator.animationRunner, WeaponAnimationType.Start);

                                if (weaponBlockAnimationRunnerOnScrollGraphic)
                                    this.StartAndStopAnimationRunner(State, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.Start);

                                if (weaponBlockAnimationRunnerOnWeapon)
                                    this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(State, ar, WeaponAnimationType.Start));

                            }

                            // start animator
                            if (weaponBlockAnimatorParameter != "") {

                                if (weaponBlockAnimateOnEntity)
                                    ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, Originator.animator, WeaponAnimationType.Start));

                                if (weaponBlockAnimateOnScrollGraphic)
                                    ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.Start));

                                if (weaponBlockAnimateOnWeapon)
                                    this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, a, WeaponAnimationType.Start)));
                            }

                        }



                        break;
                    case WeaponState.UnBlock:

                        //If refreshing then skip the adjustments so we can just update animation
                        if (Refresh == false) {

                            this.isWeaponBlocking = false;

                            //adjust prevent melee effects/melee mitigation damage
                            if (this.weaponBlockPreventMeleeEffects) {
                                Originator.TogglePreventMeleeEffects(false);
                            } else if (this.weaponBlockMitigateMeleeDamagePercentage > 0) {
                                Originator.AdjustMeleeDamageMitigationPercentage(-this.weaponBlockMitigateMeleeDamagePercentage);
                            }


                            //adjust prevent melee effects/projectile and raycast mitigation damage
                            if (this.weaponBlockPreventProjectileEffects) {
                                Originator.TogglePreventProjectileAndRayCastEffects(false);
                            } else if (this.weaponBlockMitigateProjectileDamagePercentage > 0) {
                                Originator.AdjustProjectileMitigationPercentage(-this.weaponBlockMitigateProjectileDamagePercentage);
                            }


                            //revert stat modification 
                            foreach (BlockStatModifications statMod in this.weaponBlockStatModifications) {


                                //If we previously increased the stat, then decrease and vice versa
                                switch (statMod.arithmeticOperator) {
                                    case ArithmeticIncrDecrOperators.Increase:

                                        Originator.AdjustStatValue(statMod.statName, -statMod.finalStatValueModification, statMod.statIntegrationType, GCStatType.Stat);


                                        break;
                                    case ArithmeticIncrDecrOperators.Decrease:

                                        Originator.AdjustStatValue(statMod.statName, statMod.finalStatValueModification, statMod.statIntegrationType, GCStatType.Stat);

                                        break;
                                }

                            }

                        }

                        //If using weapon animations
                        if (this.useWeaponBlockAesthetics == true) {

                            //Start animation runner
                            if (weaponBlockAnimationRunnerClip != null) {
                                if (weaponBlockAnimationRunnerOnEntity)
                                    this.StartAndStopAnimationRunner(State, Originator.animationRunner, WeaponAnimationType.Stop);

                                if (weaponBlockAnimationRunnerOnScrollGraphic)
                                    this.StartAndStopAnimationRunner(State, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.Stop);

                                if (weaponBlockAnimationRunnerOnWeapon)
                                    this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(State, ar, WeaponAnimationType.Stop));

                            }

                            // start animator
                            if (weaponBlockAnimatorParameter != "") {

                                if (weaponBlockAnimateOnEntity)
                                    ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, Originator.animator, WeaponAnimationType.Stop));

                                if (weaponBlockAnimateOnScrollGraphic)
                                    ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.Stop));

                                if (weaponBlockAnimateOnWeapon)
                                    this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(State, a, WeaponAnimationType.Stop)));
                            }

                        }



                        break;
                }



            }

            /// <summary>
            /// Function determines if the weapon is currently blocking 
            /// </summary>
            /// <returns>True if weapon is currently blocking, else false</returns>
            public bool IsWeaponBlocking() {

                return this.isWeaponBlocking;

            }

            /// <summary>
            /// Will activate the block handler which will play animations and perform events which happen after blocking (enabling abilities etc)
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <param name="BlockedEntity">Entity that had an ability blocked</param>
            /// <param name="AbilityType">The type of ability that was blocked</param>
            /// <param name="AbilityHitPoint">The vector position where the ability collided</param>
            /// <param name="InterruptWeaponBlock">(Optional)If True then the weapon blocking will be cancelled stopping the entity from blocking</param>
            /// <param name="ReduceWeaponBlockDurability">(Optional)If True then the block durability will be decreased due to an ability hit</param>
            /// <returns>A float indicating how long the block reaction is playing for, returns -1 if the block failed</returns>
            public float ActivateBlockHandler(ABC_IEntity Originator, ABC_IEntity BlockedEntity, AbilityType AbilityType, Vector3 AbilityHitPoint = default(Vector3), bool InterruptWeaponBlock = false, bool ReduceWeaponBlockDurability = true) {

                //reaction wait time
                float retVal = 0;

                //If set to interrupt the weapon block then stop blocking and end here
                if (InterruptWeaponBlock == true) {
                    //stop blocking, preventing blocking for a moment so entity doesn't reblock before effects activate
                    Originator.PreventWeaponBlockingForDuration(1f);
                    Originator.StopWeaponBlocking();

                    //  were not blocking anymore so return -1 and end here
                    return -1f;
                }


                if (this.weaponBlockFaceAbilityRequired == true) {
                    var dir = (AbilityHitPoint - Originator.transform.position).normalized;
                    var dot = Vector3.Dot(dir, Originator.transform.forward);

                    //Note: higher the value the more accuracy needed to face
                    if (dot < 0.08) {

                        //stop blocking, preventing blocking for a moment so entity doesn't reblock before effects activate
                        Originator.PreventWeaponBlockingForDuration(1f);
                        Originator.StopWeaponBlocking();

                        //  were not facing the ability hit point so end here 
                        return -1f;
                    }
                } else if (this.weaponBlockTurnToAbilityHitPoint) { //If set to turn to ability hit point 

                    //If ability we blocked was melee then just face the entity we blocked
                    if (AbilityType == AbilityType.Melee) {

                        Originator.TurnTo(BlockedEntity.gameObject);

                    } else if (AbilityHitPoint != default(Vector3)) {

                        //else if hit point is not default then turn to the direction of where the projectile attack came in 

                        //use originator y vector so its a turns too keeping same height
                        AbilityHitPoint.y = Originator.transform.position.y;
                        Originator.LookAt(AbilityHitPoint);
                    }


                }



                //Adjust block durability if the ability blocked was set to reduce weapon block durability
                if (ReduceWeaponBlockDurability == true && this.weaponBlockDurabilityReduction > 0)
                    Originator.AdjustBlockDurability(-this.weaponBlockDurabilityReduction);


                //if block durability is now 0 or below then stop weapon blocking and return here
                if (Originator.blockDurability <= 0f) {
                    //stop blocking, preventing blocking for a short while so entity doesn't reblock
                    Originator.PreventWeaponBlockingForDuration(4f);
                    Originator.StopWeaponBlocking();

                    //end here as block was interrupted due to durability reaching 0
                    return -1f;

                }


                //block graphics and animations
                if (this.useWeaponBlockAesthetics == true) {

                    //play graphic
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateGraphic(Originator, WeaponGraphicType.WeaponBlockEffect));

                    //Start and stop animation runner
                    if (weaponBlockReactionAnimationRunnerClip != null) {

                        //Update return value (which tells the method calling how long the reaction will last)
                        retVal = this.weaponBlockReactionAnimationRunnerClipDelay + this.weaponBlockReactionAnimationRunnerClipDuration;

                        if (weaponBlockReactionAnimationRunnerOnEntity)
                            this.StartAndStopAnimationRunner(WeaponState.BlockReaction, Originator.animationRunner, WeaponAnimationType.StartAndStop);

                        if (weaponBlockReactionAnimationRunnerOnScrollGraphic)
                            this.StartAndStopAnimationRunner(WeaponState.BlockReaction, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.StartAndStop);

                        if (weaponBlockReactionAnimationRunnerOnWeapon)
                            this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.BlockReaction, ar, WeaponAnimationType.StartAndStop));

                    }

                    // start and stop animator
                    if (weaponBlockReactionAnimatorParameter != "") {

                        retVal = Mathf.Max(retVal, this.weaponBlockReactionAnimatorDuration);

                        if (weaponBlockReactionAnimateOnEntity)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.BlockReaction, Originator.animator, WeaponAnimationType.StartAndStop));

                        if (weaponBlockReactionAnimateOnScrollGraphic)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.BlockReaction, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.StartAndStop));

                        if (weaponBlockReactionAnimateOnWeapon)
                            this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.BlockReaction, a, WeaponAnimationType.StartAndStop)));
                    }


                }


                //stop entity from attacking 
                if (this.interruptBlockedMeleeAttack && AbilityType == AbilityType.Melee) {
                    BlockedEntity.InterruptAbilityActivation();
                    //Stagger the enemy
                    BlockedEntity.HitRestrictAbilityActivation();
                    BlockedEntity.ActivateCurrentEquippedWeaponMeleeAttackBlockedReaction();
                }




                //Enable any abilities set to enable after blocking 
                if (this.enableAbilitiesAfterBlocking) {
                    //Loop through all abilities to enable 
                    foreach (int abilityID in this.abilityIDsToEnableAfterBlocking) {
                        Originator.EnableAbilityForDuration(abilityID, this.enableAbilitiesAfterBlockingDuration);
                    }

                }

                //If activating ability after block 
                if (this.activateAbilityAfterBlocking)
                    Originator.TriggerAbility(this.abilityIDToActivateAfterBlocking);


                return retVal;



            }

            /// <summary>
            /// Will activate the melee attack reflected reaction handler which will play animations and perform events when an attack is blocked or parried
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            public void ActivateMeleeAttackReflectedReaction(ABC_IEntity Originator) {

                //If using weapon animations
                if (this.useWeaponMeleeAttackReflectedAnimations == true) {

                    //Start and stop animation runner
                    if (weaponMeleeAttackReflectedAnimationRunnerClip != null) {

                        if (weaponMeleeAttackReflectedAnimationRunnerOnEntity)
                            this.StartAndStopAnimationRunner(WeaponState.AttackReflected, Originator.animationRunner, WeaponAnimationType.StartAndStop);

                        if (weaponMeleeAttackReflectedAnimationRunnerOnScrollGraphic)
                            this.StartAndStopAnimationRunner(WeaponState.AttackReflected, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.StartAndStop);

                        if (weaponMeleeAttackReflectedAnimationRunnerOnWeapon)
                            this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.AttackReflected, ar, WeaponAnimationType.StartAndStop));

                    }

                    // start and stop animator
                    if (weaponMeleeAttackReflectedAnimatorParameter != "") {

                        if (weaponMeleeAttackReflectedAnimateOnEntity)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.AttackReflected, Originator.animator, WeaponAnimationType.StartAndStop));

                        if (weaponMeleeAttackReflectedAnimateOnScrollGraphic)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.AttackReflected, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.StartAndStop));

                        if (weaponMeleeAttackReflectedAnimateOnWeapon)
                            this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.AttackReflected, a, WeaponAnimationType.StartAndStop)));
                    }

                }

            }

            /// <summary>
            /// Will activate the weapon trail for the duration provided 
            /// </summary>
            /// <param name="Duration">How long to render the weapon trail for</param>
            /// <param name="Delay">Delay before weapon trail shows</param>
            /// <param name="TrailColours">List of colours to render on the trail</param>	
            /// <param name="ActivatedAbility">(Optional)The ability that activated the weapon trail</param>
            public void ActivateWeaponTrail(float Duration, float Delay, int GraphicIteration = 0, ABC_Ability ActivatedAbility = null) {

                if (this.weaponGraphics.Count == 0)
                    return;

                WeaponObj weaponObject = this.weaponGraphics[0];

                if (GraphicIteration < weaponGraphics.Count)
                    weaponObject = this.weaponGraphics[GraphicIteration];

                if (weaponObject.GetWeaponTrail() != null)
                    ABC_Utilities.mbSurrogate.StartCoroutine(weaponObject.GetWeaponTrail().Activate(Duration, Delay, null, ActivatedAbility));

            }

            /// <summary>
            /// Will interrupt all weapon trails for the weapon
            /// </summary>
            public void InterruptAllWeaponTrails() {

                foreach (WeaponObj obj in this.weaponGraphics) {

                    obj.GetWeaponTrail()?.InterruptTrail();

                }

            }

            /// <summary>
            /// Determines if the weapon has any ammo (used when checking if ability can activate)
            /// </summary>
            /// <returns>True if weapon has ammo, else false if weapon has 0 ammo</returns>
            public bool HasAmmo() {

                // if weapon uses ammo and has no ammo or is set to reload and has no ammo in the clip then return false 
                if (this.UseWeaponAmmo == false || this.UseWeaponAmmo == true && this.weaponAmmoCount <= 0 && (this.useWeaponReload == false || this.useWeaponReload == true && this.currentWeaponAmmoClipCount <= 0))
                    return false;

                // else return true 
                return true;
            }

            /// <summary>
            /// Determines if a reload is required on the weapon
            /// </summary>
            /// <returns>Returns true if a reload is required, else false</returns>
            public bool ReloadRequired() {

                bool retVal = false;

                //If using weapon reload and a reload is required
                if (this.UseWeaponAmmo == true && this.useWeaponReload == true && this.weaponReloadRequired == true) {

                    // If we are currently already reloading by filling a clip one by one then interrupt the reload if clip count has already increased during the process
                    // Technically reload is not required any more as the clip has been filled fully or partially
                    if (this.weaponReloadFillClip == true && this.currentWeaponAmmoClipCount > 0) {

                        //Interrupt reload
                        this.weaponReloadInterrupted = true;

                        //Reload is no longer required
                        retVal = false;

                    } else {

                        //If not filling clip and reload is required then return true
                        retVal = true;
                    }
                }

                //Return result
                return retVal;
            }

            /// <summary>
            /// Adjusts weapons's ammo by the value provided. Will also update originators ammo GUI if provided.
            /// </summary>
            /// <remarks>
            /// If an originator is provided then the method will retrieve the entitys Ammo Text GUI and update it reflecting the weapons's current ammo. 
            /// </remarks>
            /// <param name="Value">Value to adjust ammo by (positive and negative possible)</param>
            /// <param name="Originator">(Optional) Entity which is adjusting weapons's ammo </param>
            /// <param name="AdjustAmmoOnly">(Optional) If true then only ammo will be modified, else it's up to logic to decide if ammo or current clip count is changed. </param>
            public void AdjustWeaponAmmo(int Value, ABC_IEntity Originator = null, bool AdjustAmmoOnly = false) {


                if (this.useWeaponReload == true && this.UseWeaponAmmo == true) {

                    //If we have just set to AdjustAmmoOnly then update count and ignore clip
                    if (AdjustAmmoOnly)
                        this.weaponAmmoCount += Value;
                    else // else change current clip count
                        this.currentWeaponAmmoClipCount += Value;



                    //If clip has gone over max compacity then add the remainder to the ammo count
                    if (this.currentWeaponAmmoClipCount > this.weaponClipSize) {
                        this.weaponAmmoCount += this.currentWeaponAmmoClipCount % weaponClipSize;
                        this.currentWeaponAmmoClipCount = this.weaponClipSize;
                    }

                    if (this.currentWeaponAmmoClipCount < 0)
                        this.currentWeaponAmmoClipCount = 0;


                    //If we have run out of ammo in our clip then set reload required to true 
                    if (this.currentWeaponAmmoClipCount == 0 && this.weaponReloadRequired == false) {

                        //set reload required to true
                        this.weaponReloadRequired = true;

                        //auto reloading if enabled
                        if (this.autoReloadWeaponWhenRequired && currentWeaponAmmoClipCount == 0 && IsCurrentlyEquippedBy(Originator) == true)
                            ABC_Utilities.mbSurrogate.StartCoroutine(this.ReloadWeaponAmmo(Originator));

                    }


                } else {
                    this.weaponAmmoCount += Value;
                }


                //If ammo count has gone into negative then set to 0
                if (this.weaponAmmoCount < 0)
                    this.weaponAmmoCount = 0;

                // if this weapon is enabled and this weapopn is currently equipped for the originator then we can refresh the number with new calculations
                if (Originator != null && this.IsEnabled() == true && IsCurrentlyEquippedBy(Originator) == true)
                    this.DisplayAmmoOnGUI(Originator);


            }

            /// <summary>
            /// Will set the weapons ammo to the value provided, recalculating weapon clip count
            /// </summary>
            /// <param name="Value">Value to change weapon ammo too</param>
            /// <param name="Originator">(Optional) Entity which is adjusting weapons's ammo </param>
            public void SetWeaponAmmo(int Value, ABC_IEntity Originator) {

                //Change ammo to value provided
                this.weaponAmmoCount = Value;

                //Reset the clip count so it can be recalculated
                this.currentWeaponAmmoClipCount = -1;

                //Recalculate how much ammo is in the clip
                this.SetWeaponAmmoClipCount();

                //update UI if currently equipped
                if (Originator != null && this.IsEnabled() == true && IsCurrentlyEquippedBy(Originator) == true)
                    this.DisplayAmmoOnGUI(Originator);

            }

            /// <summary>
            /// Determines if the weapon is currently reloading
            /// </summary>
            /// <returns>true if weapon is reloading, else false</returns>
            public bool IsReloading() {

                return this.weaponIsReloading;
            }

            /// <summary>
            /// Will reload the weapons Ammo
            /// </summary>
            /// <param name="Originator">Entity reloading the weapon</param>
            public IEnumerator ReloadWeaponAmmo(ABC_IEntity Originator) {

                // If We have no ammo to reload or this isn't setup to reload then stop the function here
                // Also we won't reload if we are on a fresh clip that hasnt been used yet or we already reloading
                if (this.weaponAmmoCount <= 0 || this.IsEnabled() == false || this.currentWeaponAmmoClipCount == this.weaponClipSize || this.useWeaponReload == false || this.weaponIsReloading)
                    yield break;


                //Stop weapon blocking 
                if (this.IsWeaponBlocking() == true)
                    Originator.StopWeaponBlocking();


                //let rest of code know we are reloading and reload is required (incase we are interrupted) 
                this.weaponIsReloading = true;
                this.weaponReloadRequired = true;


                // turn canCast off for the reloading duration so we can't cast ANY spells whilst we are reloading
                Originator.TemporarilyDisableAbilityActivation(this.weaponReloadRestrictAbilityActivationDuration);

                //update logs
                Originator.AddToDiagnosticLog("Reloading " + this.weaponName);

                if (Originator.LogInformationAbout(LoggingType.AmmoInformation))
                    Originator.AddToAbilityLog("Reloading " + this.weaponName);


                //Tracks reload graphic
                GameObject reloadGraphic = null;

                //Determines if reload graphic should activate
                bool activateReloadGraphic = false;

                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated the same call
                //this function would not continue after duration
                float functionRequestTime = Time.time;

                //activate reloading Aesthetic if enabled
                if (this.useReloadWeaponAesthetics) {

                    //Disable the IK for reloading
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, false));

                    activateReloadGraphic = true;

                    //Start animation on ABC runner 
                    if (this.reloadWeaponAnimationRunnerOnEntity)
                        this.StartAndStopAnimationRunner(WeaponState.Reload, Originator.animationRunner, WeaponAnimationType.Start);

                    if (this.reloadWeaponAnimationRunnerOnScrollGraphic)
                        this.StartAndStopAnimationRunner(WeaponState.Reload, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.Start);

                    if (this.reloadWeaponAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.Reload, ar, WeaponAnimationType.Start));

                    if (this.reloadWeaponAnimateOnEntity)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, Originator.animator, WeaponAnimationType.Start));

                    //If enabled then activate the animation on the graphic object
                    if (this.reloadWeaponAnimateOnScrollGraphic)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.Start));

                    if (this.reloadWeaponAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, a, WeaponAnimationType.Start)));

                }


                //While loop tracker
                float reloadTimeTracker = Time.time;

                while (this.weaponIsReloading) {

                    yield return new WaitForSeconds(0.1f);

                    //If the originator activates another ability during reloading then we will wait for that to finish then interrupt the reload (to not mess with the other ability activation animations)
                    while (Originator.activatingAbility == true) {
                        yield return new WaitForEndOfFrame();
                        this.weaponReloadInterrupted = true;
                    }


                    //if interrupted or we have ran out of ammo then break early 
                    if (this.weaponReloadInterrupted || weaponAmmoCount == 0) {

                        //remove the graphic 
                        if (reloadGraphic != null)
                            ABC_Utilities.PoolObject(reloadGraphic);

                        //exit wait loop early 
                        this.weaponIsReloading = false;
                    }

                    //If reload graphic is set to activate then activate graphic
                    if (activateReloadGraphic == true) {
                        yield return ABC_Utilities.mbSurrogate.StartCoroutine(ActivateGraphic(Originator, WeaponGraphicType.WeaponReload));
                        //Activate graphic will store the reload graphic in use so lets grab that for this method
                        reloadGraphic = this.currentActiveWeaponReloadGraphic;
                        activateReloadGraphic = false; // make sure loop doesn't activate reload graphic again unless told to later
                    }

                    //If the reloading animation duration has not passed since we started reloading then continue while loop
                    if (Time.time - reloadTimeTracker < this.weaponReloadDuration || this.weaponIsReloading == false)
                        continue;




                    //Reload animaiton duration has passed - if we are not filling a clip bit by bit then we should have finished reloading a whole clip 
                    if (this.weaponReloadFillClip == false) {

                        //If ammo count is less then clip size then make current clip count equal to ammo and set ammo to 0
                        if (this.weaponAmmoCount < this.weaponClipSize) {
                            this.currentWeaponAmmoClipCount = this.weaponAmmoCount;
                            this.weaponAmmoCount = 0;
                        } else {

                            //Else remove clip size from our ammo stack and make current ammo clip full capacity 
                            this.weaponAmmoCount -= this.weaponClipSize;
                            this.currentWeaponAmmoClipCount = this.weaponClipSize;
                        }

                        //break the loop
                        this.weaponIsReloading = false;

                    } else {
                        //If we are filling the clip then we will adjust the ammo up after every animatior duration (like adding shotgun shell every x duration) instead of wasting a magazine 

                        //if set to repeat reload graphic, then set variable to activate the graphic again
                        if (this.weaponReloadFillClipRepeatGraphic && this.useReloadWeaponAesthetics)
                            activateReloadGraphic = true;

                        //adjust ammo bit by bit 
                        this.weaponAmmoCount--;
                        this.AdjustWeaponAmmo(1, Originator);

                        //If we reach the clip capacity then we can end here 
                        if (this.currentWeaponAmmoClipCount == this.weaponClipSize)
                            this.weaponIsReloading = false;

                        //Record the current time so we know when next duration is up to increase the ammo (put another shell in)
                        reloadTimeTracker = Time.time;


                    }



                }


                //end reloading Aesthetic if enabled
                if (this.useReloadWeaponAesthetics) {

                    //Start animation on ABC runner 
                    if (this.reloadWeaponAnimationRunnerOnEntity)
                        this.StartAndStopAnimationRunner(WeaponState.Reload, Originator.animationRunner, WeaponAnimationType.Stop);

                    if (this.reloadWeaponAnimationRunnerOnScrollGraphic)
                        this.StartAndStopAnimationRunner(WeaponState.Reload, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), WeaponAnimationType.Stop);

                    if (this.reloadWeaponAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.Reload, ar, WeaponAnimationType.Stop));

                    if (this.reloadWeaponAnimateOnEntity)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, Originator.animator, WeaponAnimationType.Stop));

                    //If enabled then activate the animation on the graphic object
                    if (this.reloadWeaponAnimateOnScrollGraphic)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), WeaponAnimationType.Stop));

                    if (this.reloadWeaponAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Reload, a, WeaponAnimationType.Stop)));

                    //Wait a small while then enable the IK                
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, true, 0.2f));

                }


                //If the reload has been interrupted then just exit the whole function - as reload will still be required when scroll ability is next equipped it will restart the reloading
                if (weaponReloadInterrupted) {

                    //If we filled a clip before it got interrupted then we no longer need to reload
                    if (this.weaponReloadFillClip && this.currentWeaponAmmoClipCount > 0)
                        this.weaponReloadRequired = false;

                    //reset variable
                    this.weaponReloadInterrupted = false;

                    //break out of function
                    yield break;

                }

                //We no longer need to reload
                this.weaponReloadRequired = false;

                // if this weapon is enabled and this weapopn is currently equipped for the originator  then we can refresh the number with new calculations
                if (Originator != null && this.IsEnabled() == true && IsCurrentlyEquippedBy(Originator) == true)
                    this.DisplayAmmoOnGUI(Originator);


            }




            /// <summary>
            /// Will start or stop the weapons crosshair override animation (true to start, flase to stop)
            /// </summary>
            /// <param name="Originator">Entity that is showing the crosshair override</param>
            /// <param name="Enabled">True to start the crosshair override animation, else false to stop animation</param>
            public void ToggleCrosshairOverrideAnimation(ABC_IEntity Originator, bool Enabled = true) {

                //If crosshair animation is not enabled return false
                if (this.useWeaponCrosshairOverrideAnimations == false)
                    return;


                //Start animation runner
                if (weaponCrosshairOverrideAnimationRunnerClip != null) {
                    if (weaponCrosshairOverrideAnimationRunnerOnEntity)
                        this.StartAndStopAnimationRunner(WeaponState.CrossHairOverride, Originator.animationRunner, Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop);

                    if (weaponCrosshairOverrideAnimationRunnerOnScrollGraphic)
                        this.StartAndStopAnimationRunner(WeaponState.CrossHairOverride, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner(), Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop);

                    if (weaponCrosshairOverrideAnimationRunnerOnWeapon)
                        this.GetWeaponAnimationRunners().ForEach(ar => this.StartAndStopAnimationRunner(WeaponState.CrossHairOverride, ar, Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop));

                }

                // start animator
                if (weaponCrosshairOverrideAnimatorParameter != "") {

                    if (weaponCrosshairOverrideAnimateOnEntity)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.CrossHairOverride, Originator.animator, Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop));

                    if (weaponCrosshairOverrideAnimateOnScrollGraphic)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.CrossHairOverride, Originator.currentScrollAbility.GetCurrentScrollAbilityAnimator(), Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop));

                    if (weaponCrosshairOverrideAnimateOnWeapon)
                        this.GetWeaponAnimators().ForEach(a => ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.CrossHairOverride, a, Enabled == true ? WeaponAnimationType.Start : WeaponAnimationType.Stop)));
                }



            }



            /// <summary>
            /// Will drop the weapon, running animations, showing the drop graphic and disabling/deleting the graphic
            /// </summary>
            /// <param name="Originator">Entity the weapon is attached too</param>
            /// <returns>A float value indicating how long the weapon drop will take, if 0 then weapon drop was not successful.</returns>
            public float DropWeapon(ABC_IEntity Originator) {

                if (this.enableWeaponDrop == false)
                    return 0;

                //Ensure weapon drop duration is atleast 0.1 or bugs occur
                if (this.weaponDropDuration == 0)
                    this.weaponDropDuration = 0.1f;

                //Stop weapon blocking 
                if (this.IsWeaponBlocking() == true)
                    Originator.StopWeaponBlocking();

                //Start animation runner
                if (this.weaponDropAnimationRunnerClip != null)
                    this.StartAndStopAnimationRunner(WeaponState.Drop, Originator.animationRunner);

                // start animator
                if (this.weaponDropAnimatorParameter != "")
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(WeaponState.Drop, Originator.animator));

                //create graphic
                if (this.useWeaponDropObject == true)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateGraphic(Originator, WeaponGraphicType.WeaponDropObject));

                //Disable or Delete any abilities set up to be enabled when this weapon is equipped
                this.AbilitiesToggleHandler(Originator, WeaponState.Drop);

                //Disable or Delete any ABC groups set up to be enabled when this weapon is equipped
                this.AbilityGroupToggleHandler(Originator, WeaponState.Drop);

                //Delete any UI icons linked to this weapon. 
                this.UIToggleHandler(Originator, WeaponState.Drop);


                //Disable weapon if deleting or disabling (gets rid of graphics)
                if (this.weaponDropAction != WeaponDropAction.None)
                    this.Disable(Originator, false);

                if (this.weaponDropAction == WeaponDropAction.DeleteWeapon)
                    Originator.DeleteWeapon(this.weaponID);


                return this.weaponDropDuration;


            }



            #endregion


        }

        /// <summary>
        /// Groups together ABC abilities. Allows for additional functionality which focuses on all the abilities in the same group. Such functionality as enabling all abilities in a group at once whilst playing graphics or able to easily change settings for a whole group.
        /// Also used to group abilities in the inspector to easily see what you need to change. 
        /// </summary>
        /// <remarks>
        /// Functionality could include changing a setting once that will automatically apply to all abilities in a group, enabling a group during play to change 'character modes' etc
        /// </remarks>
        [System.Serializable]
        public class AbilityGroup {

            // ************ Settings *****************************

            #region Settings For Ability Group 

            /// <summary>
            /// Name of the ability group 
            /// </summary>
            [Tooltip("Name of the ability group")]
            public string groupName;

            /// <summary>
            /// ID of the ability group
            /// </summary>
            [Tooltip("ID of the ability group")]
            public int groupID;

            //Used for inspector only right now - will enable/disable toggle the group 
            public bool toggleGroup = false;

            /// <summary>
            /// Determines if the group is currently enabled (all abilities activated)
            /// </summary>
            public bool groupEnabled = false;

            /// <summary>
            /// The game time in which the group was enabled
            /// </summary>
            public float groupEnableTime = 0f;

            /// <summary>
            /// If true then group will be enabled when the entity is initialised
            /// </summary>
            [Tooltip("If true then group will be enabled when the entity is initialised")]
            public bool enableGroupOnStart = false;

            /// <summary>
            /// If true then group will be disabled when the entity is initialised
            /// </summary>
            [Tooltip("If true then group will be disabled when the entity is initialised")]
            public bool disableGroupOnStart = false;

            /// <summary>
            /// If true then all abilities in group will be enabled after selected scroll abilities are activated (equiped)
            /// </summary>
            [Tooltip("If true then all abilities in group will be enabled after selected scroll abilities are enabled")]
            public bool enableOnScrollAbilitiesEnabled = false;

            /// <summary>
            /// If true then all abilities in group will be disabled after selected scroll abilities are deactivated (dequiped)
            /// </summary>
            [Tooltip("If true then all abilities in group will be disabled after selected scroll abilities are disabled")]
            public bool disableOnScrollAbilitiesDisabled = false;

            /// <summary>
            /// group will be enabled after any scroll abilities (ID) in this list are activated (equiped)
            /// </summary>
            [Tooltip("enable all abilities in this group when the following scroll abilities are activated")]
            public List<int> enableOnScrollAbilityIDsActivated;


            /// <summary>
            /// If true then user can activate an input to enable all abilities in the group which also activates animation and graphic effects
            /// </summary>
            [Tooltip("Enable all abilities on input")]
            public bool enableGroupedAbilitiesOnInput = false;

            /// <summary>
            /// If true then ability group can only be enabled via input if the enable group points have reached the limit 
            /// </summary>
            [Tooltip(" If true then ability group can only be enabled via input if the enable group points have reached the limit")]
            public bool enableOnInputGroupPointsLimitRequired = false;

            /// <summary>
            /// Type of input to enable the collider 
            /// </summary>
            [Tooltip("type of input to enable all abilities in the group")]
            public InputType abilityGroupEnableInputType;

            /// <summary>
            /// Button name to enable the collider
            /// </summary>
            [Tooltip("The Button name to enable all abilities in the group")]
            public string abilityGroupEnableButton;

            /// <summary>
            /// Key to enable the collider
            /// </summary>
            [Tooltip("What button to pressed to enable all abilities in the group")]
            public KeyCode abilityGroupEnableKey;

            /// <summary>
            /// If true then the group can be enabled via a point counter. When the counter reaches the number set it will enable the group. The counter can be incremented via different mechanics. 
            /// </summary>
            [Tooltip("If true then the group can be enabled via a point counter. When the counter reaches the number set it will enable the group.")]
            public bool enableGroupViaPoints = false;

            /// <summary>
            /// The max number of points the group can reach
            /// </summary>
            [Tooltip("The max number of points the group can reach.")]
            public float groupPointLimit = 100f;

            /// <summary>
            /// The current point count value
            /// </summary>
            [Tooltip("The current point count value")]
            public float currentPointCount = 0f;

            /// <summary>
            /// If true then the points will be adjusted over time (either incremented or decremented)
            /// </summary>
            [Tooltip("If true then the points will be adjusted over time (either incremented or decremented)")]
            public bool adjustPointsOverTime = false;

            /// <summary>
            /// The amount to adjust the points each tick (Can be positive or negative)
            /// </summary>
            [Tooltip("The amount to adjust the points each tick (Can be positive or negative)")]
            public float pointAdjustmentValue = -1f;

            /// <summary>
            /// The interval between point adjustment 
            /// </summary>
            [Tooltip("The interval between point adjustment ")]
            public float pointAdjustmentInterval = 1f;

            /// <summary>
            /// Slider Object which can display current Group Points. 
            /// </summary>
            [Tooltip("Slider GUI to show groupPoints")]
            public ABC_SliderReference groupPointSlider;

            /// <summary>
            /// Variable determining if slider is showing
            /// </summary>
            public bool sliderShowing = false;

            /// <summary>
            /// If true then the slider will only show when the entity is selected.
            /// </summary>
            [Tooltip("Will only show slider when targeted")]
            public bool onlyShowSliderWhenSelected = false;

            /// <summary>
            /// GUI Text which can display current Group Points.
            /// </summary>
            [Tooltip("GUIText to show groupPoints information")]
            public ABC_TextReference groupPointText;

            /// <summary>
            /// Variable determining if text is showing
            /// </summary>
            public bool textShowing = false;

            /// <summary>
            /// If true then the text will only show when the entity is selected. 
            /// </summary>
            [Tooltip("Only show GUIText when targeted")]
            public bool onlyShowTextWhenSelected = false;

            /// <summary>
            /// Delay until abilities in the group are enabled
            /// </summary>
            [Tooltip("Delay until abilities in the group are enabled")]
            public float abilityGroupEnableDelay = 0f;

            /// <summary>
            /// How long abilities in the group are enabled for if 0 is set then duration is not applied and abilities will stay enabled
            /// </summary>
            [Tooltip("How long abilities in the group are enabled for if 0 is set then duration is not applied and abilities will stay enabled.")]
            public float abilityGroupEnableDuration = 0f;

            /// <summary>
            /// Slider Object which can display current duration of the enabled group
            /// </summary>
            [Tooltip("Slider GUI to show groupPoints")]
            public ABC_SliderReference groupEnableDurationSlider;

            /// <summary>
            /// Variable determining if slider is showing
            /// </summary>
            public bool groupEnableDurationSliderShowing = false;

            /// <summary>
            /// If true then the slider will only show when the entity is selected.
            /// </summary>
            [Tooltip("Will only show slider when targeted")]
            public bool onlyShowEnableDurationSliderWhenSelected = false;

            /// <summary>
            /// GUI Text which can display current duration of the enabled group.
            /// </summary>
            [Tooltip("GUIText to show groupPoints information")]
            public ABC_TextReference groupEnableDurationText;

            /// <summary>
            /// Variable determining if text is showing
            /// </summary>
            public bool groupEnableDurationTextShowing = false;

            /// <summary>
            /// If true then the text will only show when the entity is selected. 
            /// </summary>
            [Tooltip("Only show GUIText when targeted")]
            public bool onlyShowEnableDurationTextWhenSelected = false;

            /// <summary>
            /// If true then another group wil be enabled when this group is disabled
            /// </summary>
            public bool enableNewGroupOnDisable = false;

            /// <summary>
            /// List of groupsID to enable when this group is disabled
            /// </summary>
            public List<int> enableGroupOnDisableIDs = new List<int>();

            /// <summary>
            /// If true then another group wil be disabled when this group is enabled
            /// </summary>
            public bool disableNewGroupOnEnable = false;

            /// <summary>
            /// List of groupsID to disable when this group is enabled
            /// </summary>
            public List<int> disableGroupOnEnableIDs = new List<int>();

            /// <summary>
            /// If true then graphics and animations will activate when an ability group is enabled
            /// </summary>
            [Tooltip("Show effect and play animation when an ability group is enabled")]
            public bool useAbilityGroupEnableAesthetics;


            /// <summary>
            /// Offset for the group enable graphics
            /// </summary>
            [Tooltip("Offset for the effect graphics")]
            public Vector3 abilityGroupEnableAestheticsPositionOffset;

            /// <summary>
            /// Forward offset for the abilityGroupEnable graphics
            /// </summary>
            [Tooltip("Forward offset from starting position")]
            public float abilityGroupEnableAestheticsPositionForwardOffset = 0f;

            /// <summary>
            /// Right offset for the abilityGroupEnable graphics
            /// </summary>
            [Tooltip("Right offset from starting position")]
            public float abilityGroupEnableAestheticsPositionRightOffset = 0f;

            /// <summary>
            /// Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public ABC_AnimationClipReference abilityGroupEnableAnimationRunnerClip;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference abilityGroupEnableAnimationRunnerMask = null;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float abilityGroupEnableAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float abilityGroupEnableAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float abilityGroupEnableAnimationRunnerClipDuration = 1f;


            /// <summary>
            /// Name of the abilityGroupEnable animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string abilityGroupEnableAnimatorParameter;

            /// <summary>
            /// Type of parameter for the abilityGroupEnable animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType abilityGroupEnableAnimatorParameterType;

            /// <summary>
            /// Value to turn on the abilityGroupEnable animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string abilityGroupEnableAnimatorOnValue;

            /// <summary>
            /// Value to turn off the abilityGroupEnable animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string abilityGroupEnableAnimatorOffValue;

            /// <summary>
            /// Duration of the ability group animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float abilityGroupEnableAnimatorDuration = 3f;

            /// <summary>
            /// Graphic object that appears when abilityGroupEnable
            /// </summary>
            [Tooltip("Particle or object that shows when abilityGroupEnable")]
            public ABC_GameObjectReference abilityGroupEnableGraphic;

            /// <summary>
            /// Sub graphic which appears when abilityGroupEnable, will be a child of the main graphic object
            /// </summary>
            [Tooltip("Sub graphic or object that shows when abilityGroupEnable. Will be child of abilityGroupEnable graphic")]
            public ABC_GameObjectReference abilityGroupEnableSubGraphic;

            /// <summary>
            /// How long the abilityGroupEnable graphic will show for
            /// </summary>
            [Tooltip("How long to show the effect for")]
            public float abilityGroupEnableAestheticDuration;

            /// <summary>
            /// The delay before graphic will appear
            /// </summary>
            [Tooltip("The delay before graphic will appear")]
            public float abilityGroupEnableAestheticDelay;

            /// <summary>
            /// starting position for the intiating graphic
            /// </summary>
            [Tooltip("where the abilityGroupEnable graphic starts")]
            public StartingPosition abilityGroupEnableStartPosition;

            /// <summary>
            /// If no target is currently selected for group enable graphic starting position then the graphic will as a backup start on the soft target
            /// </summary>
            [Tooltip("If no target is currently selected for group enable graphic starting position then the graphic will as a backup start on the soft target")]
            public bool abilityGroupEnablePositionAuxiliarySoftTarget = false;

            /// <summary>
            /// Object for abilityGroupEnable graphic to start on if OnObject is selected for the start position
            /// </summary>
            public ABC_GameObjectReference abilityGroupEnablePositionOnObject;


            /// <summary>
            /// Tag which the graphic can start from if starting position is OnTag. Will retrieve the first gameobject with the tag defined. Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag to start from")]
            public string abilityGroupEnablePositionOnTag;

            /// <summary>
            /// If true then a weapon will be equipped when the group is enabled
            /// </summary>
            [Tooltip("If true then a weapon will be equipped when the group is enabled")]
            public bool equipWeaponOnEnable = false;

            /// <summary>
            /// WeaponID to equip when this group is enable
            /// </summary>
            [Tooltip("WeaponID to equip when this group is enable")]
            public int equipWeaponOnEnableID;

            /// <summary>
            /// If true then quick toggle will be applied equipping the weapon instantly
            /// </summary>
            [Tooltip("If true then quick toggle will be applied equipping the weapon instantly")]
            public bool equipWeaponOnEnableQuickToggle = true;

            /// <summary>
            /// If true then a weapon will be equipped when the group is disabled
            /// </summary>
            [Tooltip("If true then a weapon will be equipped when the group is disabled")]
            public bool equipWeaponOnDisable = false;

            /// <summary>
            /// WeaponID to equip when this group is disabled
            /// </summary>
            [Tooltip(" WeaponID to equip when this group is disabled")]
            public int equipWeaponOnDisableID;

            /// <summary>
            /// If true then quick toggle will be applied equipping the weapon instantly
            /// </summary>
            [Tooltip("If true then quick toggle will be applied equipping the weapon instantly")]
            public bool equipWeaponOnDisableQuickToggle = true;


            /// <summary>
            /// Used by inspector to select dropdowns
            /// </summary>
            public int abilityGroupListChoice = 0;

            /// <summary>
            /// Used by inspector to select dropdown
            /// </summary>
            public int weaponEquipListChoice = -1;

            /// <summary>
            /// List of group enable pool graphics
            /// </summary>
            public List<GameObject> abilityGroupEnablePool = new List<GameObject>();

            #endregion

            // ************ Variables *****************************

            #region Variables For Ability Group 

            /// <summary>
            /// the current active group enable graphic
            /// </summary>
            /// <remarks>Public for save/load functionality</remarks>
            private GameObject activeEnableAbilitiesGraphic = null;

            /// <summary>
            /// Returns a bool indicating if the enable group points have maxed out reaching the limit
            /// </summary>
            private bool enableGroupPointsLimitReached {

                get {
                    if (this.currentPointCount >= this.groupPointLimit)
                        return true;
                    else
                        return false;
                }
            }



            #endregion


            // ************ ENUMS *****************************

            #region ENUMS

            public enum AbilityGroupButtonPressState {
                AbilityGroupEnable
            }


            public enum AbilityGroupGraphicType {
                AbilityGroupEnable
            }

            private enum AbilityGroupAnimationState {
                AbilityGroupEnable
            }

            #endregion

            // ************************** Private Methods *************************************

            #region Private Methods

            /// <summary>
            /// Will create and pool ability group enable graphics setup for the group
            /// </summary>
            /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
            /// <returns>One graphic gameobject which has been created</returns>
            private GameObject CreateAbilityGroupEnableObjects(bool CreateOne = false) {


                // *************** Initiating Pools ***********************

                GameObject groupEnableObj = null;

                if (this.useAbilityGroupEnableAesthetics == true && this.abilityGroupEnableGraphic.GameObject != null) {

                    //how many objects to make
                    float objCount = CreateOne ? 1 : this.abilityGroupEnableAestheticDuration + 3;


                    for (int i = 0; i < objCount; i++) {
                        // create object particle 
                        groupEnableObj = (GameObject)(GameObject.Instantiate(this.abilityGroupEnableGraphic.GameObject));
                        groupEnableObj.name = this.abilityGroupEnableGraphic.GameObject.name;


                        // copy child object for additional Aesthetic 
                        if (this.abilityGroupEnableSubGraphic.GameObject != null) {
                            GameObject enableChildObj = (GameObject)(GameObject.Instantiate(this.abilityGroupEnableSubGraphic.GameObject));
                            enableChildObj.name = this.abilityGroupEnableSubGraphic.GameObject.name;
                            enableChildObj.transform.position = groupEnableObj.transform.position;
                            enableChildObj.transform.rotation = groupEnableObj.transform.rotation;
                            enableChildObj.transform.parent = groupEnableObj.transform;
                        }


                        //disable and pool the object 
                        ABC_Utilities.PoolObject(groupEnableObj);

                        // add to generic list. 
                        this.abilityGroupEnablePool.Add(groupEnableObj);
                    }

                }

                return groupEnableObj;

            }


            /// <summary>
            /// Main function for checking if a button has been pressed for different Ability Group events. Depending on the state given the method will return true or false if the setup button has been pressed. 
            /// </summary>
            /// <param name="Originator">Entity that activated the ability</param>
            /// <param name="State">Depending on the state the method will return if a button setup for that state has been pressed. States include: AbilityGroupEnable 
            /// <returns>True if the correct button is being pressed, else false</returns>
            private bool ButtonPressed(ABC_IEntity Originator, AbilityGroupButtonPressState State) {

                InputType inputType = InputType.Button;
                KeyCode key = KeyCode.None;
                string button = "";

                // determine the right configuration depending on the type provided
                switch (State) {
                    case AbilityGroupButtonPressState.AbilityGroupEnable:

                        //If we can only enable the group via input when the groups points reaches the limit and the groups points have not reached the limit then return false as button can't be pressed 
                        if (this.enableOnInputGroupPointsLimitRequired == true && this.enableGroupPointsLimitReached == false)
                            return false;

                        inputType = this.abilityGroupEnableInputType;
                        key = this.abilityGroupEnableKey;
                        button = this.abilityGroupEnableButton;

                        break;

                }

                // If input type is key and the configured key is being pressed return true
                if (inputType == InputType.Key && Originator.inputManager.GetKeyDown(key))
                    return true;

                // if input type is button and the configured button is being pressed return true
                if (inputType == InputType.Button && Originator.inputManager.GetButtonDown(button))
                    return true;


                // correct button is not currently being pressed so return false
                return false;


            }

            /// <summary>
            /// Starts an animation clip using the ABC animation runner
            /// </summary>
            /// <param name="State">The animation to play - GroupEnable etc</param>
            /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
            private void StartAnimationRunner(AbilityGroupAnimationState State, ABC_AnimationsRunner AnimationRunner) {

                // set variables to be used later 
                AnimationClip animationClip = null;
                float animationClipSpeed = 1f;
                float animationClipDelay = 0f;


                switch (State) {
                    case AbilityGroupAnimationState.AbilityGroupEnable:

                        animationClip = this.abilityGroupEnableAnimationRunnerClip.AnimationClip;
                        animationClipSpeed = this.abilityGroupEnableAnimationRunnerClipSpeed;
                        animationClipDelay = this.abilityGroupEnableAnimationRunnerClipDelay;

                        break;
                }


                // if animator parameter is null or animation runner is not given then animation can't start so end here. 
                if (animationClip == null || AnimationRunner == null)
                    return;


                AnimationRunner.StartAnimation(animationClip, animationClipDelay, animationClipSpeed);


            }

            /// <summary>
            /// End an animation clip currently playing using the ABC animation runner
            /// </summary>
            /// <param name="State">The animation to play - GroupEnable etc</param>
            /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
            /// <param name="Delay">(Optional) Delay before animation ends</param>
            private void EndAnimationRunner(AbilityGroupAnimationState State, ABC_AnimationsRunner AnimationRunner, float Delay = 0f) {

                // set variables to be used later 
                AnimationClip animationClip = null;


                switch (State) {
                    case AbilityGroupAnimationState.AbilityGroupEnable:

                        animationClip = this.abilityGroupEnableAnimationRunnerClip.AnimationClip;

                        break;
                }


                // if animator parameter is null or animation runner is not given then animation can't start so end here. 
                if (animationClip == null || AnimationRunner == null)
                    return;


                AnimationRunner.EndAnimation(animationClip, Delay);
            }

            /// <summary>
            /// Starts an animation for the entity depending on what state is passed through
            /// </summary>
            /// <param name="State">The animation to play - GroupEnable etc</param>
            /// <param name="Animator">Animator component</param>
            private void StartAnimation(AbilityGroupAnimationState State, Animator Animator) {


                // set variables to be used later 
                AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
                string animatorParameter = "";
                string animatorOnValue = "";



                switch (State) {
                    case AbilityGroupAnimationState.AbilityGroupEnable:

                        animatorParameterType = this.abilityGroupEnableAnimatorParameterType;
                        animatorParameter = this.abilityGroupEnableAnimatorParameter;
                        animatorOnValue = this.abilityGroupEnableAnimatorOnValue;

                        break;
                }

                // if animator parameter is null or animator is not given then animation can't start so end here. 
                if (animatorParameter == "" || Animator == null) {
                    return;
                }


                switch (animatorParameterType) {
                    case AnimatorParameterType.Float:
                        Animator.SetFloat(animatorParameter, float.Parse(animatorOnValue));
                        break;
                    case AnimatorParameterType.integer:
                        Animator.SetInteger(animatorParameter, int.Parse(animatorOnValue));
                        break;
                    case AnimatorParameterType.Bool:
                        if (animatorOnValue != "True" && animatorOnValue != "False") {
                            Debug.Log("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                        }
                        Animator.SetBool(animatorParameter, bool.Parse(animatorOnValue));
                        break;

                    case AnimatorParameterType.Trigger:
                        Animator.SetTrigger(animatorParameter);
                        break;
                }



            }


            /// <summary>
            /// Ends the animation for the entity depending on what state is passed through
            /// </summary>
            /// <param name="State">The animation to stop - GroupEnable</param>
            /// <param name="Animator">Animator component</param>
            private void EndAnimation(AbilityGroupAnimationState State, Animator Animator) {

                // set variables to be used later 
                AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
                string animatorParameter = "";
                string animatorOffValue = "";



                switch (State) {
                    case AbilityGroupAnimationState.AbilityGroupEnable:

                        animatorParameterType = this.abilityGroupEnableAnimatorParameterType;
                        animatorParameter = this.abilityGroupEnableAnimatorParameter;
                        animatorOffValue = this.abilityGroupEnableAnimatorOffValue;

                        break;
                }


                // if animator parameter is null or animator is not given then animation can't start so end here. 
                if (animatorParameter == "" || Animator == null) {
                    return;
                }




                switch (animatorParameterType) {
                    case AnimatorParameterType.Float:
                        Animator.SetFloat(animatorParameter, float.Parse(animatorOffValue));
                        break;
                    case AnimatorParameterType.integer:
                        Animator.SetInteger(animatorParameter, int.Parse(animatorOffValue));
                        break;
                    case AnimatorParameterType.Bool:
                        if (animatorOffValue != "True" && animatorOffValue != "False") {
                            Debug.Log("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                        }
                        Animator.SetBool(animatorParameter, bool.Parse(animatorOffValue));
                        break;

                    case AnimatorParameterType.Trigger:
                        // don't need to switch off as trigger does that straight away anyway.
                        break;
                }



            }


            /// <summary>
            /// activate graphics for the ability group
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            /// <param name="GraphicType">Type of graphic to activate</param>
            /// <returns>Will return the graphic gameobject which has been created</returns>
            private IEnumerator ActivateGraphic(ABC_IEntity Originator, AbilityGroupGraphicType GraphicType) {


                StartingPosition startingPosition = StartingPosition.Self;
                GameObject positionOnObject = null;
                string positionOnTag = null;
                Vector3 positionOffset = new Vector3(0, 0, 0);
                float positionForwardOffset = 0f;
                float positionRightOffset = 0f;
                float duration = 2f;
                float delay = 0f;
                bool auxiliarySoftTarget = false;
                GameObject graphicObj = null;



                switch (GraphicType) {
                    case AbilityGroupGraphicType.AbilityGroupEnable:

                        startingPosition = this.abilityGroupEnableStartPosition;
                        positionOnObject = this.abilityGroupEnablePositionOnObject.GameObject;
                        positionOnTag = this.abilityGroupEnablePositionOnTag;
                        auxiliarySoftTarget = this.abilityGroupEnablePositionAuxiliarySoftTarget;

                        positionOffset = this.abilityGroupEnableAestheticsPositionOffset;
                        positionForwardOffset = this.abilityGroupEnableAestheticsPositionForwardOffset;
                        positionRightOffset = this.abilityGroupEnableAestheticsPositionRightOffset;

                        duration = this.abilityGroupEnableAestheticDuration;
                        delay = this.abilityGroupEnableAestheticDelay;

                        graphicObj = this.abilityGroupEnablePool.Where(obj => obj.activeInHierarchy == false).FirstOrDefault();

                        if (graphicObj == null)
                            graphicObj = this.CreateAbilityGroupEnableObjects(true);

                        //track object for future use 
                        this.activeEnableAbilitiesGraphic = graphicObj;

                        break;

                    default:

                        break;

                }

                //if delay is greater then 0 then wait 
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);

                //initial starting point is the entity incase anything goes wrong
                Transform meTransform = Originator.transform;
                Vector3 position = meTransform.position;
                //record values which might be used
                GameObject parentObject = Originator.gameObject;
                GameObject targetObject = Originator.target;
                GameObject softTargetObject = Originator.softTarget;
                GameObject worldTargetObject = Originator.worldTargetObj;
                Vector3 worldTargetPosition = Originator.worldTargetPos;

                // get starting position 
                switch (startingPosition) {

                    case StartingPosition.Self:
                        position = meTransform.position + positionOffset + meTransform.forward * positionForwardOffset + meTransform.right * positionRightOffset;
                        parentObject = Originator.gameObject;
                        break;
                    case StartingPosition.OnObject:
                        if (positionOnObject != null) {
                            Transform onObjectTransform = positionOnObject.transform;
                            position = onObjectTransform.position + positionOffset + onObjectTransform.forward * positionForwardOffset + onObjectTransform.right * positionRightOffset;
                            parentObject = onObjectTransform.gameObject;
                        }
                        break;
                    case StartingPosition.OnTag:
                        GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                        if (onTagObj != null) {
                            Transform onTagTransform = onTagObj.transform;
                            position = onTagTransform.position + positionOffset + onTagTransform.forward * positionForwardOffset + onTagTransform.right * positionRightOffset;
                            parentObject = onTagTransform.gameObject;
                        }
                        break;
                    case StartingPosition.OnSelfTag:
                        GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(Originator, positionOnTag);
                        if (onSelfTagObj != null) {
                            Transform onSelfTagTransform = onSelfTagObj.transform;
                            position = onSelfTagTransform.position + positionOffset + onSelfTagTransform.forward * positionForwardOffset + onSelfTagTransform.right * positionRightOffset;
                            parentObject = onSelfTagTransform.gameObject;
                        }
                        break;
                    case StartingPosition.Target:
                        if (targetObject != null) { // get target object
                            var targetTransform = targetObject.transform;
                            position = targetTransform.position + positionOffset + targetTransform.forward * positionForwardOffset + targetTransform.right * positionRightOffset;
                            parentObject = targetObject;

                        } else if (auxiliarySoftTarget == true && softTargetObject != null) {
                            // if there is no current target object and auxiliary soft target is enabled then record current soft target instead
                            var softTargetTransform = softTargetObject.transform;
                            position = softTargetTransform.position + positionOffset + softTargetTransform.forward * positionForwardOffset + softTargetTransform.right * positionRightOffset;
                            parentObject = softTargetObject;
                        }
                        break;
                    case StartingPosition.OnWorld:
                        if (worldTargetObject != null) {
                            var worldTargetTransform = worldTargetObject.transform;
                            position = worldTargetPosition + positionOffset + worldTargetTransform.forward * positionForwardOffset + worldTargetTransform.right * positionRightOffset;
                            parentObject = worldTargetObject;
                        }
                        break;
                    case StartingPosition.CameraCenter:
                        var cameraTransform = Originator.Camera.transform;
                        position = Originator.Camera.transform.position + positionOffset + cameraTransform.forward * positionForwardOffset + cameraTransform.right * positionRightOffset;
                        parentObject = Originator.Camera.gameObject;
                        break;
                    default:
                        Originator.AddToDiagnosticLog("Error: starting position for " + GraphicType.ToString() + "  graphic was not correctly determined.");
                        break;
                }


                // set position and parent
                graphicObj.transform.position = position;
                graphicObj.transform.rotation = parentObject.transform.rotation;
                graphicObj.transform.parent = parentObject.transform;

                // set it true 
                graphicObj.SetActive(true);

                // disable and pool graphic  after delay if we haven't set this to 0 (which means infinite) 
                if (duration != 0) {
                    ABC_Utilities.PoolObject(graphicObj, duration);
                }





            }

            /// <summary>
            /// Updates the entities Ability Group GUI with the current mana values
            /// </summary>
            private void UpdateAbilityGroupGUI() {

                //**** Group Points ****

                if (this.groupPointText.text != null && this.textShowing == true)
                    this.groupPointText.text = ((int)this.currentPointCount).ToString() + "/" + this.groupPointLimit.ToString();  // convert to int before string to get rid of decimals

                if (this.groupPointSlider.Slider != null && sliderShowing == true) {
                    this.groupPointSlider.Slider.value = (float)this.currentPointCount;
                    // Make sure the max value of the slider  matches the max group points (this might change during play) 
                    this.groupPointSlider.Slider.maxValue = this.groupPointLimit;
                }

                //**** Group Enable Duration ****

                //Work out the current enable duration
                float remainingEnableDuration = this.abilityGroupEnableDuration - (Time.time - this.groupEnableTime);

                //If the group is not enabled or duration is infinite then don't show a duration slider countdown (value stays at 0)
                if (this.groupEnabled == false || this.abilityGroupEnableDuration == 0f)
                    remainingEnableDuration = 0f;

                //If the group is enabled and a duration is applied then update the text to represent else hide it
                if (this.groupEnableDurationText.text != null && this.groupEnableDurationTextShowing == true)
                    this.groupEnableDurationText.text = remainingEnableDuration == 0 ? "" : ((int)remainingEnableDuration).ToString();


                if (this.groupEnableDurationSlider.Slider != null && groupEnableDurationSliderShowing == true) {
                    this.groupEnableDurationSlider.Slider.value = remainingEnableDuration;
                    // Make sure the max value of the slider  matches the max group points (this might change during play) 
                    this.groupEnableDurationSlider.Slider.maxValue = this.abilityGroupEnableDuration;
                }


            }



            /// <summary>
            /// Will return a list of the Originators abilities that have been assigned to this group
            /// </summary>
            /// <param name="Originator">Entity with the abilities setup</param>
            /// <returns>list of abilities assigned to this group</returns>
            private List<ABC_Ability> GetOriginatorsAbilitiesAssignedToMyGroup(ABC_IEntity Originator) {

                if (Originator == null)
                    return null;

                return Originator.CurrentAbilities.Where(item => item.allowAbilityGroupAssignment == true && (item.assignedAbilityGroupIDs.Contains(this.groupID) || item.assignedAbilityGroupNames.Any(g => g.Trim().ToUpper() == this.groupName.Trim().ToUpper()))).ToList();

            }

            /// <summary>
            /// Will return a list of the Originators weapons that have been assigned to this group
            /// </summary>
            /// <param name="Originator">Entity with the weapons setup</param>
            /// <returns>list of weapons assigned to this group</returns>
            private List<Weapon> GetOriginatorsWeaponsAssignedToMyGroup(ABC_IEntity Originator) {

                if (Originator == null)
                    return null;

                return Originator.AllWeapons.Where(item => item.allowWeaponGroupAssignment == true && (item.assignedGroupIDs.Contains(this.groupID) || item.assignedGroupNames.Contains(this.groupName))).ToList();

            }





            #endregion

            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Constructor mainly used by inspector
            /// </summary>
            /// <param name="ID">Unique ID of the group</param>
            public AbilityGroup() {
            }

            /// <summary>
            /// Clears all Object pools relating to the Ability Group.
            /// </summary>
            public void ClearObjectPools() {

                //Destroy old objects
                foreach (GameObject obj in this.abilityGroupEnablePool) {
                    Destroy(obj);
                }

                this.abilityGroupEnablePool.Clear();

            }

            /// <summary>
            /// Will pool all objects and other objects setup by ability groups. 
            /// </summary>
            public void CreateObjectPools() {
                this.CreateAbilityGroupEnableObjects();

            }

            /// <summary>
            /// Will activate/disable the Ability Group GUI for the entity 
            /// </summary>
            /// <param name="Enabled">If true will enable the GUI, else disable it</param>
            public void ShowAbilityGroupGUI(bool Enabled = true) {

                //**** Group Points ****

                if (groupPointSlider.Slider != null) {
                    if (this.onlyShowSliderWhenSelected == true) {
                        this.groupPointSlider.Slider.gameObject.SetActive(Enabled);
                        this.sliderShowing = Enabled;
                    } else {
                        // slider should always be shown so set true regardless of enable or disable
                        this.groupPointSlider.Slider.gameObject.SetActive(true);
                        this.sliderShowing = true;
                    }
                }


                if (groupPointText.text != null) {
                    if (this.onlyShowTextWhenSelected == true) {
                        this.groupPointText.Graphic.enabled = Enabled;
                        this.textShowing = Enabled;
                    } else {
                        // text should always be shown so set true regardless of enable or disable
                        this.groupPointText.Graphic.enabled = true;
                        this.textShowing = true;
                    }
                }


                //**** Group Enable Duration ****

                if (groupEnableDurationSlider.Slider != null) {
                    if (this.onlyShowEnableDurationSliderWhenSelected == true) {
                        this.groupEnableDurationSlider.Slider.gameObject.SetActive(Enabled);
                        this.groupEnableDurationSliderShowing = Enabled;
                    } else {
                        // slider should always be shown so set true regardless of enable or disable
                        this.groupEnableDurationSlider.Slider.gameObject.SetActive(true);
                        this.groupEnableDurationSliderShowing = true;
                    }
                }


                if (groupEnableDurationText.text != null) {
                    if (this.onlyShowEnableDurationTextWhenSelected == true) {
                        this.groupEnableDurationText.Graphic.enabled = Enabled;
                        this.groupEnableDurationTextShowing = Enabled;
                    } else {
                        // text should always be shown so set true regardless of enable or disable
                        this.groupEnableDurationText.Graphic.enabled = true;
                        this.groupEnableDurationTextShowing = true;
                    }
                }

            }


            /// <summary>
            /// Will adjust the enable group points by the value provided
            /// </summary>
            /// <param name="AdjustmentValue">Value to adjust the enable group points by</param>
            public void AdjustEnableGroupPoints(float AdjustmentValue) {

                //Only adjust values if the group is not enabled
                if (this.groupEnabled == false)
                    this.currentPointCount += AdjustmentValue;

                //If we have gone below 0 then reset the counter to 0 
                if (this.currentPointCount < 0)
                    this.currentPointCount = 0;

            }

            /// <summary>
            /// Adjusts the enable group points by a value set every x seconds defined by the interval set
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            public IEnumerator AdjustGroupPointsOverTime(ABC_IEntity Originator) {

                //If we don't need to adjust points overtime then end function here 
                if (this.adjustPointsOverTime == false)
                    yield break;


                while (Originator.gameObject.activeInHierarchy == true) {

                    yield return new WaitForSeconds(this.pointAdjustmentInterval);

                    //Only adjust values if the group is not enabled
                    if (this.groupEnabled == false)
                        this.currentPointCount += this.pointAdjustmentValue;

                    //If we have gone below 0 then reset the counter to 0 
                    if (this.currentPointCount < 0)
                        this.currentPointCount = 0;

                    // If we have gone over the limit then adjust the counter to the limit so we are not over
                    if (this.currentPointCount > this.groupPointLimit)
                        this.currentPointCount = this.groupPointLimit;

                    yield return new WaitForSeconds(0.1f);

                }



            }


            /// <summary>
            /// Will toggle the ability group. If no boolean is provided and it is currently enabled it will be disabled and vice versa
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            public void ToggleOnOff(ABC_IEntity Originator) {

                //If group is not enabled then enable it else disable it
                if (this.groupEnabled == false) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.EnableAllAbilitiesAndWeapons(Originator));

                } else {
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.DisableAllAbilitiesAndWeapons(Originator));
                }

                //reset toggle button
                this.toggleGroup = false;

            }

            /// <summary>
            /// Will toggle the ability group. If no boolean is provided and it is currently enabled it will be disabled and vice versa
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            /// <param name="Enabled">True to enable ability group, else false to disable</param>
            public void ToggleOnOff(ABC_IEntity Originator, bool Enabled) {

                //If group is not enabled and true has been passed then enable
                if (this.groupEnabled == false && Enabled == true) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.EnableAllAbilitiesAndWeapons(Originator));

                } // else if group is enabled and false has been passed then disable
                else if (this.groupEnabled == true && Enabled == false) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.DisableAllAbilitiesAndWeapons(Originator));
                }

                //reset toggle button
                this.toggleGroup = false;

            }

            /// <summary>
            /// Will enable all abilities and weapons linked under the group activating graphics and animations setup
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            /// <param name="IgnorePointReset">If true then Ability Group Point count will not be reset </param>
            public IEnumerator EnableAllAbilitiesAndWeapons(ABC_IEntity Originator, bool IgnorePointReset = false) {


                // turn the group enabled flag to true
                this.groupEnabled = true;
                //record time of enable
                this.groupEnableTime = Time.time;


                // Reset current point count
                if (IgnorePointReset == false)
                    currentPointCount = 0f;

                //Get all abilities in my group and enable
                foreach (ABC_Ability ability in this.GetOriginatorsAbilitiesAssignedToMyGroup(Originator))
                    ABC_Utilities.mbSurrogate.StartCoroutine(ability.Enable(abilityGroupEnableDelay, Originator));

                //Get all weapons in my group and enable
                foreach (Weapon weapon in this.GetOriginatorsWeaponsAssignedToMyGroup(Originator))
                    ABC_Utilities.mbSurrogate.StartCoroutine(weapon.Enable(0, Originator, false));

                //Disable any groups set to deactivate when this group enables
                if (this.disableNewGroupOnEnable == true) {

                    foreach (AbilityGroup group in Originator.AllAbilityGroups.Where(item => this.disableGroupOnEnableIDs.Contains(item.groupID) && item.groupEnabled == true))
                        ABC_Utilities.mbSurrogate.StartCoroutine(group.DisableAllAbilitiesAndWeapons(Originator, abilityGroupEnableDelay));

                }


                //Disable abilities after duration+delay (If duration is not infinite - 0)
                if (abilityGroupEnableDuration > 0f)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.DisableAllAbilitiesAndWeapons(Originator, abilityGroupEnableDuration + abilityGroupEnableDelay));


                //If enabled then  activate graphics 
                if (this.useAbilityGroupEnableAesthetics == true) {

                    //Track what time this method was called
                    //Stops overlapping i.e if another part of the system activated the same call
                    //this function would not continue after duration
                    float functionRequestTime = Time.time;

                    //Turn off IK whilst animating
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, false));

                    //Store active enable abilities graphic incase we need to destroy when disabling
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateGraphic(Originator, AbilityGroupGraphicType.AbilityGroupEnable));

                    //Start the animation
                    this.StartAnimation(AbilityGroupAnimationState.AbilityGroupEnable, Originator.animator);

                    //start animation runner
                    this.StartAnimationRunner(AbilityGroupAnimationState.AbilityGroupEnable, Originator.animationRunner);

                    //If animation runner duration is infinite (0f) then don't end the animation, else end animation after the duration
                    if (this.abilityGroupEnableAnimationRunnerClip.AnimationClip != null && this.abilityGroupEnableAnimationRunnerClipDuration != 0f)
                        this.EndAnimationRunner(AbilityGroupAnimationState.AbilityGroupEnable, Originator.animationRunner, this.abilityGroupEnableAnimationRunnerClipDuration);

                    //If animation duration is infiniate (0f) then return here, else end animation after the duration
                    if (this.abilityGroupEnableAnimatorDuration == 0f)
                        yield break;

                    //Wait for duration then end animation
                    yield return new WaitForSeconds(this.abilityGroupEnableAnimatorDuration);
                    this.EndAnimation(AbilityGroupAnimationState.AbilityGroupEnable, Originator.animator);

                    //Enable IK 
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, true));
                }


                //setup weapons and scroll abilities equipping the weapon enabled if configured too
                if (this.equipWeaponOnEnable == true && this.equipWeaponOnEnableID > -1) {
                    Originator.SetupWeaponsAndScrollAbilities(this.equipWeaponOnEnableID, this.equipWeaponOnEnableQuickToggle);
                } else {
                    Originator.SetupWeaponsAndScrollAbilities();
                }



            }


            /// <summary>
            /// Will disable all abilities and weapons linked under the group. Ending any activating animations and graphics
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            public IEnumerator DisableAllAbilitiesAndWeapons(ABC_IEntity Originator, float delay = 0f) {


                //Wait for delay if given
                if (delay > 0f)
                    yield return new WaitForSeconds(delay);


                // turn the group enabled flag to false
                this.groupEnabled = false;
                this.groupEnableTime = 0f;

                //Get all abilities in my group and disable
                foreach (ABC_Ability ability in this.GetOriginatorsAbilitiesAssignedToMyGroup(Originator))
                    ability.Disable(Originator);


                //Get all weapons in my group and enable
                foreach (Weapon weapon in this.GetOriginatorsWeaponsAssignedToMyGroup(Originator))
                    weapon.Disable(Originator, false);

                //If aesthetics are enabled then make sure we stop animating and destroy any graphics 
                if (useAbilityGroupEnableAesthetics == true) {

                    //If graphic or animation duration is infinite then we will end it now
                    if (this.abilityGroupEnableAestheticDuration == 0f)
                        ABC_Utilities.PoolObject(activeEnableAbilitiesGraphic);

                    if (this.abilityGroupEnableAnimatorDuration == 0f)
                        this.EndAnimation(AbilityGroupAnimationState.AbilityGroupEnable, Originator.animator);

                }

                //Enable any groups set to activate when this group disables
                if (this.enableNewGroupOnDisable == true) {

                    foreach (AbilityGroup group in Originator.AllAbilityGroups.Where(item => this.enableGroupOnDisableIDs.Contains(item.groupID) && item.groupEnabled == false))
                        ABC_Utilities.mbSurrogate.StartCoroutine(group.EnableAllAbilitiesAndWeapons(Originator));

                }


                //setup weapons and scroll abilities equipping the weapon enabled if configured too
                if (this.equipWeaponOnEnable == true && this.equipWeaponOnEnableID > -1) {
                    Originator.SetupWeaponsAndScrollAbilities(this.equipWeaponOnEnableID, this.equipWeaponOnEnableQuickToggle);
                } else {
                    Originator.SetupWeaponsAndScrollAbilities();
                }

            }



            /// <summary>
            /// Will handle all the different group scenarios running different functions when certain events happen  
            /// </summary>
            /// <param name="Originator">Entity the group is attached too</param>
            public void GroupHandler(ABC_IEntity Originator) {

                //Update any GUI
                this.UpdateAbilityGroupGUI();

                //If the toggle group flag is true then we need to switch the group from enable to disable or disable to enable
                if (this.toggleGroup == true)
                    this.ToggleOnOff(Originator);


                //Check if we need to enable any abilities due to key/button press
                if (this.enableGroupedAbilitiesOnInput == true && this.ButtonPressed(Originator, AbilityGroup.AbilityGroupButtonPressState.AbilityGroupEnable))
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.EnableAllAbilitiesAndWeapons(Originator));



                //handle enabling grouping via points mechanic
                if (this.enableGroupViaPoints == true && this.enableGroupPointsLimitReached == true)
                    //If we have reached the points to enable then enable all abilities and reset the count
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.EnableAllAbilitiesAndWeapons(Originator));



            }


            #endregion

        }

        /// <summary>
        ///  Class for handling AI Rules, determining when to automatically activate an ability and which entity to activate it on. Class is only used and managed by ABC Controller Component.
        /// </summary>
        [System.Serializable]
        public class AIRule {

            // ************ Settings *****************************

            #region Settings for AI RUle

            /// <summary>
            /// Name of the AI Rule
            /// </summary>
            [Tooltip("Name of the Rule ")]
            public string RuleName;

            /// <summary>
            /// Will track when the AI Rule was last activated 
            /// </summary>
            [Tooltip("Will track when the AI Rule was last activated")]
            public float AIRuleLastActivationTime = 0f;

            /// <summary>
            /// Determines what action the AI will do if all conditions are met (Activate ability, activate ability group etc)
            /// </summary>
            public AIAction selectedAIAction = AIAction.ActivateAbility;

            /// <summary>
            /// The ID of the ability which will activate if the rule meets all the conditions. ID is used so ability names can be changed without breaking AI rules setup. 
            /// </summary>
            [Tooltip("ID of the ability which will activate")]
            public int AIAbilityID = -1;

            /// <summary>
            ///  used for inspector, keeps track of what ability is currently chosen for the rule 
            /// </summary>
            public int AIAbilityListChoice = 0;

            /// <summary>
            /// list of IDs of ability groups which will activate if the rule meets all the conditions. ID is used so ability group name can be changed without breaking AI rules setup. 
            /// </summary>
            [Tooltip("List of ability group (IDs) that will activate")]
            public List<int> AIActivateAbilityGroupIDs = new List<int>();

            /// <summary>
            /// Used by inspector to select dropdowns
            /// </summary>
            public int AIActivateGroupListChoice = 0;

            /// <summary>
            /// The ID of the weapon to equip if the rule meets all the conditions
            /// </summary>
            [Tooltip("The ID of the weapon to equip if the rule meets all the conditions")]
            public int AIWeaponID = -1;

            /// <summary>
            /// Used by inspector to select dropdowns
            /// </summary>
            public int AIWeaponListChoice = 0;

            /// <summary>
            /// The minimum duration of AI Blocking
            /// </summary>
            [Tooltip("The minimum duration of AI Blocking")]
            [Range(0f, 100f)]
            public float AIBlockDurationMin = 10f;

            /// <summary>
            /// The maximum duration of AI Blocking
            /// </summary>
            [Tooltip("The maximum duration of AI Blocking")]
            [Range(0f, 100f)]
            public float AIBlockDurationMax = 20f;

            /// <summary>
            /// If true then the AI rule will trigger a cooldown after use between a random min and max value. Whilst on cooldown the rule can't be used.
            /// </summary>
            [Tooltip("If true then the AI rule will enforce it's own cooldown after use between a random min and max value")]
            public bool AIEnableRuleCooldown = false;

            /// <summary>
            /// The minimum cooldown which will be triggered after the rule is used
            /// </summary>
            [Tooltip("The minimum cooldown which will be triggered after the rule is used")]
            [Range(0f, 600f)]
            public float AIRuleCooldownMinValue = 30f;

            /// <summary>
            /// The maximum cooldown which will be triggered after the rule is used
            /// </summary>
            [Tooltip("The maximum cooldown which will be triggered after the rule is used")]
            [Range(0f, 600f)]
            public float AIRuleCooldownMaxValue = 90f;

            /// <summary>
            /// Will track the current cooldown of the AI Rule
            /// </summary>
            [Tooltip("The maximum cooldown which will be triggered after the rule is used")]
            public float AIRuleCurrentCooldown = 0f;

            /// <summary>
            /// Minimum probability of the ability activating. If the dice roll is higher then the minimum and lower then the maximum then it will activate if other conditions are met. 
            /// </summary>
            [Range(0f, 100f)]
            public float probabilityMinValue = 0f;

            /// <summary>
            /// Maximum probability of the ability activating. If the dice roll is higher then the minimum and lower then the maximum then it will activate if other conditions are met. 
            /// </summary>
            [Range(0f, 100f)]
            public float probabilityMaxValue = 100f;


            /// <summary>
            /// Target Object to check conditions against
            /// </summary>
            public List<GameObject> AITargets;

            /// <summary>
            /// Tags to check conditions against
            /// </summary>
            public List<string> AITags;

            /// <summary>
            /// If condition is met which target should the ability be activated on: Original, Self, Nearest, Target of Nearest
            /// </summary>
            [Tooltip("If condition is met what target should the ability be used on?")]
            public AIRuleTarget AIAbilityTarget;

            /// <summary>
            /// List of targets used to determine the target of the target 
            /// </summary>
            public List<GameObject> AINearestTargets;

            /// <summary>
            /// List of tags used to determine the target of the tag provided
            /// </summary>
            public List<string> AINearestTags;

            /// <summary>
            /// If true then ability will always activate ignoring all conditions setup
            /// </summary>
            [Tooltip("If true then ability will always activate ignoring all conditions setup")]
            public bool castWithoutConditions = false;


            /// <summary>
            /// If true then ability will activate without checking for nearby potential targets (Dice Roll is still performed)
            /// </summary>
            [Tooltip("If true then ability will activate without checking for nearby potential targets (Dice Roll is still performed)")]
            public bool activateWithoutTarget = false;

            /// <summary>
            /// If true then the ability will activate even if there is no open targeting slots due to the targeter limit already being reached
            /// </summary>
            [Tooltip("If true then the ability will activate even if there is no open targeting slots on the entity due to the targeter limit already being reached")]
            public bool bypassTargetLimitations = false;


            /// <summary>
            /// Min range in which objects are checked against conditions setup for ability to activate i.e if object in x distance has health below 50% then activate
            /// </summary>
            [Tooltip("range to check conditions against objects")]
            public float AIRangeMin = 0f;

            /// <summary>
            /// Max range in which objects are checked against conditions setup for ability to activate i.e if object in x distance has health below 50% then activate
            /// </summary>
            [Tooltip("range to check conditions against objects")]
            public float AIRangeMax = 30f;


            /// <summary>
            /// If condition is met should the target be changed before using the ability 
            /// </summary>
            [Tooltip("If conditions are met should the active target change?")]
            public bool updateTarget = false;

            /// <summary>
            /// If true then conditions are not checked against the entity activating the ability 
            /// </summary>
            [Tooltip("Does the entity Ignore caster in these conditions")]
            public bool ignoreCaster = true;

            /// <summary>
            /// If the entity we check against currently has the effect active then this condition has been met
            /// </summary>
            public List<string> stateEffectConditions;

            /// <summary>
            /// If the entity we check against currently does not have the effect active then this condition has been met
            /// </summary>
            public List<string> stateEffectAbsentConditions;

            /// <summary>
            /// booling to determine to check targets HP as a condition 
            /// </summary>
            public bool AIHP = false;

            /// <summary>
            /// If health is greater then the value provided the condition has been met
            /// </summary>
            [Range(0f, 100f)]
            public float AIHealthGreaterThan = 0;

            /// <summary>
            /// If health is less then the value provided the condition has been met
            /// </summary>
            [Range(0f, 100f)]
            public float AIHealthLessThan = 100;

            /// <summary>
            /// booling to determine to check targets MP as a condition 
            /// </summary>
            public bool AIMP = false;

            /// <summary>
            /// If mana is greater then the value provided the condition has been met
            /// </summary>
            [Range(0f, 100f)]
            public float AIManaGreaterThan = 0;

            /// <summary>
            /// If mana is less then the value provided the condition has been met
            /// </summary>
            [Range(0f, 100f)]
            public float AIManaLessThan = 100;

            /// <summary>
            /// booling which when enabled will only activate depending on how many surrounding tags in the area are also currently activating
            /// </summary>
            public bool AIAreaActivationLimit = false;

            /// <summary>
            /// How far in the surrounding area to count tags activating 
            /// </summary>
            public float AIAreaActivationLimitRange = 20f;

            /// <summary>
            /// The max amount of surrounding objects that can be activating for the condition to continue
            /// </summary>
            public int AIAreaActivationLimitMax = 2;

            /// <summary>
            /// Tags to check conditions against
            /// </summary>
            public List<string> AIAreaActivationLimitTags;

            /// <summary>
            /// If true then the entity has to face the target for the condition to continue and activate the ability
            /// </summary>
            public bool AIFacingTarget = false;


            /// <summary>
            /// Bool to determine if the rule is enabled or not
            /// </summary>
            [Tooltip("Is the rule enabled ")]
            public bool AIActive = true;

            #endregion

            // ************ ENUMS *****************************

            #region ENUMS

            public enum AIAction {
                ActivateAbility,
                ActivateAbilityGroup,
                ActivateBlocking,
                EquipWeapon
            }
            #endregion

            #region Private Methods

            // ************************** Private Methods *************************************

            /// <summary>
            /// Will return an integer indicating how many entities are currently activating abilities within a range of the position provided
            /// </summary>
            /// <param name="SurroundingPosition">Position from which the area is checked</param>
            /// <param name="Range">The range of the area checked</param>
            /// <param name="Originator">entity which is activating the ability</param>
            /// <returns>Integer indicating how many entities are activating</returns>
            private int NumberOfSurroundingTagsActivating(Vector3 SurroundingPosition, float Range, ABC_IEntity Originator) {

                // get all targets in the  range specified
                List<ABC_IEntity> surroundingTags = ABC_Utilities.GetAllABCEntitiesInRange(SurroundingPosition, Range).ToList();

                //Counts how many objects are activating in the area
                int retVal = 0;

                // loop through all objects found which are enabled and have the tags specificed
                foreach (ABC_IEntity Obj in surroundingTags.Where(a => a.gameObject.activeInHierarchy == true && ABC_Utilities.ObjectHasTag(a.gameObject, ABC_Utilities.ConvertTags(Originator, this.AIAreaActivationLimitTags)))) {


                    //If the object is activating an ability then increase the count
                    if (Obj.HasABCController() && Obj.activatingAbility == true)
                        retVal++;

                }

                //Return the count
                return retVal;
            }


            /// <summary>
            /// Returns a bool indication if the entity is facing the target 
            /// </summary>
            /// <param name="Target">object to check if originator is facing</param>
            /// <param name="Originator">entity which is activating the ability</param>
            /// <returns>True if originator is facing target, else false</returns>
            private bool FacingTargetCheck(GameObject Target, ABC_IEntity Originator) {

                if (Target == Originator.gameObject)
                    return true; 

                var dir = (Target.transform.position - Originator.transform.position).normalized;
                var dot = Vector3.Dot(dir, Originator.transform.forward);

                if (dot >= 0.3) {
                    //  were facing the target! Note: higher the value the more accuracy needed to face
                    return true;
                } else {
                    return false;

                }
            }

            /// <summary>
            /// A dice roll is performed and checked against the probability min and max values. If the roll value is inbetween the values true is returned, else false
            /// </summary>
            /// <returns>True if dice roll is inbetween the probability min and max, else false</returns>
            private bool DiceRollValid() {
                // roll the dice for probability settings on each rule
                float dice = Random.Range(0f, 100f);

                // if the dice roll is not inbetween the min and max value then return false 
                if (dice < this.probabilityMinValue || dice > this.probabilityMaxValue)
                    return false;

                //Else dice roll is inbetween min and max so return true
                return true;

            }

            #endregion

            #region Public Methods

            // ************************** Public Methods *************************************

            /// <summary>
            /// Constructor mainly used by inspector
            /// </summary>
            public AIRule() {
            }





            /// <summary>
            /// Returns a bool indicating if the target provided meets the conditions set for the Rule to activate. Example: If target health is > 50% and is effected by poison
            /// </summary>
            /// <param name="Target">object to check if conditions are met</param>
            /// <param name="Originator">entity which is activating the rule</param>
            /// <returns>True if correct AI target conditions are met, else false</returns>
            public bool CorrectTargetConditions(GameObject Target, ABC_IEntity Originator) {

                // Retrieve/Create entity object  for target as we are about to check health and mana 
                ABC_IEntity targetEntity = ABC_Utilities.GetStaticABCEntity(Target);

                //retrieve target distance to check against AI range min and max
                float targetDistance = Vector3.Distance(targetEntity.transform.position, Originator.transform.position);

                // if we collided with ourself and ignoring caster then do nothing |or| if the collided object is not within the range  |or| gameobject is not active anymore then continue to next object
                if (this.ignoreCaster == true && Target.gameObject == Originator.gameObject || targetDistance < this.AIRangeMin || targetDistance > this.AIRangeMax || Target.gameObject.activeInHierarchy == false)
                    return false;

                // if the dice roll is not inbetween the min and max value then return false 
                if (this.DiceRollValid() == false)
                    return false;

                // if we are casting without conditions then we can return true here
                if (this.castWithoutConditions == true)
                    return true;

                // else we check against conditions

                // If no state manager script then return false
                if (targetEntity.HasABCStateManager() == false)
                    return false;



                // If we are checking for a certain tag or target and the object we are checking conditions against isn't the target or hasn't got the right tag then return false
                if ((this.AITags.Count > 0 && ABC_Utilities.ObjectHasTag(Target.gameObject, ABC_Utilities.ConvertTags(Originator, this.AITags)) == false) || (this.AITargets.Count > 0 && this.AITargets.Contains(Target.gameObject) == false))
                    return false;

                //If facing target condition is true and the entity is not facing the object we are checking conditions against then return false
                if (this.AIFacingTarget == true && this.FacingTargetCheck(Target, Originator) == false)
                    return false;


                // if were checking against HP and we don't meet condition then turn return false
                if (this.AIHP == true && ((targetEntity.healthValue / targetEntity.maxHealthValue) * 100 >= this.AIHealthGreaterThan && (targetEntity.healthValue / targetEntity.maxHealthValue) * 100 <= this.AIHealthLessThan) == false)
                    return false;


                //// if were checking against MP and were not meeting the criteria then return false
                try {
                    if (this.AIMP == true && ((targetEntity.manaValue / targetEntity.maxManaValue) * 100 >= this.AIManaGreaterThan && (targetEntity.manaValue / targetEntity.maxManaValue) * 100 <= this.AIManaLessThan) == false)
                        return false;
                } catch {
                    // On the off chance that the originator mana value has not been setup right we have added a try catch 
                    Originator.AddToDiagnosticLog("Could not test condition for AIRule: " + this.RuleName + " from " + Originator.gameObject.name + " Due to " + Target.name + "not having mana setup correctly. Rule condition failed.");
                    return false;
                }


                // checking for active state effects - If not active then we can return false 
                if (this.stateEffectConditions.Count > 0 && targetEntity.IsEffectActive(stateEffectConditions) == false)
                    return false;



                // checking for absent state effects - If the effect is active we can return false
                if (this.stateEffectAbsentConditions.Count > 0 && targetEntity.IsEffectActive(stateEffectAbsentConditions))
                    return false;


                // Check how many any entities surrounding the target are activating abilities. If more then the limit setup then return false
                if (this.AIAreaActivationLimit == true && this.NumberOfSurroundingTagsActivating(targetEntity.transform.position, this.AIAreaActivationLimitRange, Originator) > this.AIAreaActivationLimitMax)
                    return false;


                // If we have reached this far then all conditions have been met and we can return true 
                return true;

            }


            /// <summary>
            /// Returns a bool indicating if the rule requires a potential target to activate, if true then no target conditions need to be checked. 
            /// </summary>
            /// <returns>True if rule can activate ability without potential target/conditions, else false</returns>
            public bool CanActivateWithoutTarget() {

                if (this.castWithoutConditions == false || this.castWithoutConditions == true && this.activateWithoutTarget == false)
                    return false;


                if (this.DiceRollValid() == false)
                    return false;



                // If we have reached this far then all conditions have been met and we can return true 
                return true;

            }



            /// <summary>
            /// Returns a GameObject which should be the final target for the ability. This is finalised incase the AI has been setup to activate on another target if conditions are met on the original target 
            /// Exampel: Fire on a nearby Enemy tag if my health is above 50%
            /// </summary>
            /// <param name="CurrentTarget">Original target for the activating ability </param>
            /// <param name="Originator">entity which is activating the ability</param>
            /// <param name="PotentialTargets">List of potential new targets to activate the ability on</param>
            /// <returns></returns>
            public GameObject FinaliseTarget(GameObject CurrentTarget, ABC_IEntity Originator, List<ABC_IEntity> PotentialTargets) {

                if (this.updateTarget == false)
                    return CurrentTarget;


                switch (this.AIAbilityTarget) {
                    case AIRuleTarget.Original:
                        // return original target
                        return CurrentTarget;

                    case AIRuleTarget.Self:
                        // return the originator
                        return Originator.gameObject;

                    case AIRuleTarget.Nearest:

                        //  using the potential targets list passed above
                        foreach (ABC_IEntity AITarget in PotentialTargets) {

                            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(AITarget.gameObject);

                            // check through list to see if any object has the right tag or target 
                            if (iEntity.HasABCStateManager() == true && this.AINearestTags.Count > 0 & ABC_Utilities.ObjectHasTag(AITarget.gameObject, ABC_Utilities.ConvertTags(Originator, this.AINearestTags)) || this.AINearestTargets.Count > 0 && this.AINearestTargets.Contains(AITarget.gameObject))
                                return AITarget.gameObject;
                        }

                        // correct target not found so return empty object
                        return null;
                    case AIRuleTarget.TargetOfNearest:
                        // continue using the list collected above
                        foreach (ABC_IEntity AITarget in PotentialTargets) {

                            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(AITarget.gameObject);

                            // check through list to see if any object has the right tag or target 
                            if (iEntity.HasABCStateManager() == true && this.AINearestTags.Count > 0 & ABC_Utilities.ObjectHasTag(AITarget.gameObject, ABC_Utilities.ConvertTags(Originator, this.AINearestTags)) || this.AINearestTargets.Count > 0 && this.AINearestTargets.Contains(AITarget.gameObject)) {

                                // get target of the potential target 
                                GameObject retVal = ABC_Utilities.GetStaticABCEntity(AITarget.gameObject).target;

                                if (retVal != null)
                                    return retVal;

                            }


                        }

                        // correct target not found so return empty object
                        return null;

                    default:
                        return null;
                }// end of switch

            }


            /// <summary>
            /// will trigger a rule cooldown between a random min and max value. Whilst on cooldown the rule can't be used.
            /// </summary>
            public void ActivateRuleCoolDown() {

                //If cooldown not enabled then end here and don't trigger the cooldown
                if (this.AIEnableRuleCooldown == false)
                    return;


                //Set cooldown to a random value between min and max 
                //If AI rule not activated before then make the minimum value 0
                this.AIRuleCurrentCooldown = Random.Range(this.AIRuleLastActivationTime == 0 ? 0 : this.AIRuleCooldownMinValue, this.AIRuleCooldownMaxValue);

                //Record the time the rule last activated for the cooldown tracking
                this.AIRuleLastActivationTime = Time.time;

            }

            #endregion

        }


        /// <summary>
        ///  Class for handling Mana GUI interfaces like sliders and text. Class is only used and managed by ABC Controller Component.
        /// </summary>
        [System.Serializable]
        public class ManaGUI {

            #region Settings


            /// <summary>
            /// Slider Object which can display current Mana. 
            /// </summary>
            [Tooltip("Slider GUI to show Mana")]
            public ABC_SliderReference manaSlider;

            /// <summary>
            /// Variable determining if slider is showing
            /// </summary>
            public bool sliderShowing = false;

            /// <summary>
            /// If true then the slider will only show when the entity is selected.
            /// </summary>
            [Tooltip("Will only show slider when targeted")]
            public bool onlyShowSliderWhenSelected = false;

            /// <summary>
            /// GUI Text which can display current Mana.
            /// </summary>
            [Tooltip("GUIText to show mana information")]
            public ABC_TextReference manaText;

            /// <summary>
            /// Variable determining if text is showing
            /// </summary>
            public bool textShowing = false;

            /// <summary>
            /// If true then the text will only show when the entity is selected. 
            /// </summary>
            [Tooltip("Only show GUIText when targeted")]
            public bool onlyShowTextWhenSelected = false;

            #endregion

            /// <summary>
            /// Create new object 
            /// </summary>
            public ManaGUI() {
            }

            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Toggles the slider GUI on and off. If only show slider when selected is false then GUI will always be shown
            /// </summary>
            /// <param name="Enabled">If true GUI will display else it will be hidden</param>
            public void ToggleSliderGUI(bool Enabled) {

                if (this.onlyShowSliderWhenSelected == false)
                    Enabled = true;

                if (this.manaSlider.Slider != null)
                    this.manaSlider.Slider.gameObject.SetActive(Enabled);

                this.sliderShowing = Enabled;

            }

            /// <summary>
            /// Toggles the text GUI on and off. If only show text when selected is false then GUI will always be shown
            /// </summary>
            /// <param name="Enabled">If true GUI will display else it will be hidden</param>
            public void ToggleTextGUI(bool Enabled) {

                if (this.onlyShowTextWhenSelected == false)
                    Enabled = true;

                if (this.manaText.text != null)
                    this.manaText.Graphic.gameObject.SetActive(Enabled);

                this.textShowing = Enabled;

            }

            /// <summary>
            /// Update the GUI to represent the current and max mana provided
            /// </summary>
            /// <param name="CurrentMana">Current mana to show on GUI</param>
            /// <param name="MaxMana">Max mana to show on GUI</param>
            public void UpdateGUI(float CurrentMana, float MaxMana) {

                if (this.manaText.text != null && this.textShowing == true)
                    this.manaText.text = ((int)CurrentMana).ToString() + "/" + MaxMana.ToString();  // convert to int before string to get rid of decimals


                if (this.manaSlider.Slider != null && this.sliderShowing == true) {
                    this.manaSlider.Slider.value = (float)CurrentMana;

                    // Make sure the max value of the mana slider matches our max mana property (this might change during play) 
                    this.manaSlider.Slider.maxValue = MaxMana;
                }

            }

            #endregion

        }



        #endregion


        // ************ Delegate *****************************

        #region Delegate


        public delegate void OnAbilityActivation(string AbilityName, int AbilityID);
        public event OnAbilityActivation onAbilityActivation;

        public delegate void OnAbilityActivationCompleted(string AbilityName, int AbilityID);
        public event OnAbilityActivationCompleted onAbilityActivationComplete;

        public delegate void OnTargetSet(GameObject TargetObj);
        public event OnTargetSet onTargetSet;

        public delegate void OnSoftTargetSet(GameObject SoftTargetObj);
        public event OnSoftTargetSet onSoftTargetSet;

        public delegate void OnScrollAbilitySetAndUnset(int AbilityID, string AbilityName, bool Set);
        public event OnScrollAbilitySetAndUnset onScrollAbilitySetAndUnset;

        public delegate void OnAbilityBeforeTarget(int AbilityID, bool Enabled);
        public event OnAbilityBeforeTarget onAbilityBeforeTarget;


        #endregion

        // ********************** Settings ******************

        #region Settings

        /// <summary>
        /// If true then the ABC controller has been set on a child object (useful for making abilities prefab to be used globally on other entities)
        /// </summary>
        public bool isChildInHierarchy = false;

        /// <summary>
        /// If true then tag conversions will be applied to the entities ABC Controller and/or StateManager during play
        /// </summary>
        public bool enableTagConversions = false;

        /// <summary>
        /// A list of tag conversions (i.e replace x tag with y in whole component)
        /// </summary>
        public List<ABC_Utilities.TagConverter> tagConversions = new List<ABC_Utilities.TagConverter>();

        /// <summary>
        /// List of weapons setup for this entity 
        /// </summary>
        public List<Weapon> Weapons = new List<Weapon>();

        /// <summary>
        /// Current weapons used in game
        /// </summary>
        public List<Weapon> _currentWeapons = new List<Weapon>();

        /// <summary>
        /// Retrieve current weapons used in game
        /// </summary>
        public List<Weapon> CurrentWeapons {

            get {

                if (_currentWeapons.Count == 0 && this.Weapons.Count > 0) {

                    foreach (Weapon weapon in this.Weapons) {

                        if (weapon.globalWeapon != null) {

                            Weapon newGlobalWeapon = new Weapon();
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(weapon.globalWeapon.ElementWeapon), newGlobalWeapon);

                            //override weapon enable status if set too
                            if (weapon.globalWeaponOverrideEnableStatus == true)
                                newGlobalWeapon.weaponEnabled = weapon.weaponEnabled;

                            //Link this weapon to the source
                            newGlobalWeapon.globalElementSource = weapon.globalWeapon;

                            _currentWeapons.Add(newGlobalWeapon);


                        } else {
                            _currentWeapons.Add(weapon);
                        }

                    }

                }


                return _currentWeapons;

            }
            set {
                _currentWeapons = value;
            }

        }


        /// <summary>
        /// List of ability groups setup for this entity
        /// </summary>
        public List<AbilityGroup> AbilityGroups = new List<AbilityGroup>();

        /// <summary>
        /// List of abilities setup for this entity 
        /// </summary>
        public List<ABC_Ability> Abilities = new List<ABC_Ability>();

        /// <summary>
        /// Current abilities used in game
        /// </summary>
        public List<ABC_Ability> _currentAbilities = new List<ABC_Ability>();


        /// <summary>
        /// Retrieve current abilities used in game
        /// </summary>
        public List<ABC_Ability> CurrentAbilities {

            get {

                if (_currentAbilities.Count == 0 && this.Abilities.Count > 0) {

                    CurrentAbilities = this.Abilities;

                }

                return _currentAbilities;

            }
            set {

                foreach (ABC_Ability ability in value) {

                    if (ability.globalAbilities != null) {

                        foreach (ABC_Ability globalAbility in ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalAbilities)) {

                            //If ability already added from another global element then continue
                            if (_currentAbilities.Where(a => a.abilityID == globalAbility.abilityID).Count() > 0)
                                continue;

                            ABC_Ability newGlobalAbility = new ABC_Ability();
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(globalAbility), newGlobalAbility);

                            //override ability enable status if set too
                            if (ability.globalAbilityOverrideEnableStatus == true)
                                newGlobalAbility.abilityEnabled = ability.abilityEnabled;

                            //override ability key if set too
                            if (ability.globalAbilityOverrideKeyTrigger == true) {
                                newGlobalAbility.key = ability.key;
                                newGlobalAbility.keyButton = ability.keyButton;
                            }

                            //Link this ability to the source
                            newGlobalAbility.globalElementSource = ability.globalAbilities;

                            //Copy over the details of the game type modification so on reload this can be reapplied to each instance
                            newGlobalAbility.globalAbilitiesEnableGameTypeModification = ability.globalAbilitiesEnableGameTypeModification;
                            newGlobalAbility.globalAbilitiesGameTypeModification = ability.globalAbilitiesGameTypeModification;


#if ABC_GC_2_Integration
                        if (meEntity.HasGC2CharacterComponent())
                            newGlobalAbility.AdjustAbilityForGameCreator2();
#endif

                            //Do Game type modification if enabled
                            if (newGlobalAbility.globalAbilitiesEnableGameTypeModification == true)
                                newGlobalAbility.ConvertToGameType(newGlobalAbility.globalAbilitiesGameTypeModification);


                            _currentAbilities.Add(newGlobalAbility);
                        }

                    } else {
                        _currentAbilities.Add(ability);
                    }

                }

            }

        }

        /// <summary>
        /// List of rules setup for this entity
        /// </summary>
        public List<AIRule> AIRules = new List<AIRule>();

        /// <summary>
        /// List of Mana GUI setup for this entity 
        /// </summary>
        public List<ManaGUI> ManaGUIList = new List<ManaGUI>();

        /// <summary>
        /// A list of ABC UI Icon elements which when setup can be dragged into taskbars and clicked on to activate abilities etc
        /// </summary>
        [Tooltip("list of ABC UI Icon elements")]
        public List<ABC_IconUI> IconUIs = new List<ABC_IconUI>();

        /// <summary>
        /// What toolbar tab is selected in the controller manager inspector
        /// </summary>
        public int toolbarControllerManagerSelection;

        /// <summary>
        /// What tab is selected in the controller manager general settings inspector
        /// </summary>
        public int toolbarControllerManagerGeneralSettingsSelection;

        /// <summary>
        /// What tab is selected in the controller manager Target settings inspector
        /// </summary>
        public int toolbarControllerManagerTargetSettingsSelection;

        /// <summary>
        /// What tab is selected in the controller manager AI settings inspector
        /// </summary>
        public int toolbarControllerManagerAISettingsSelection;

        /// <summary>
        /// What tab is selected in the controller manager ability group settings inspector
        /// </summary>
        public int toolbarControllerManagerAbilityGroupSettingsSelection;

        /// <summary>
        /// What tab is selected in the controller manager weapon settings inspector
        /// </summary>
        public int toolbarControllerManagerWeaponSettingsSelection;

        /// <summary>
        /// What toolbar tab is selected in the ability manager inspector
        /// </summary>
        public int toolbarAbiltyManagerSelection;

        /// <summary>
        /// What tab is selected in the ability manager general settings inspector
        /// </summary>
        public int toolbarAbilityManagerGeneralSettingsSelection;

        /// <summary>
        /// What tab is selected in the ability manager position & travel settings inspector
        /// </summary>
        public int toolbarAbilityManagerPositionTravelSettingsSelection;

        /// <summary>
        /// What tab is selected in the ability manager collision & impact settings inspector
        /// </summary>
        public int toolbarAbilityManagerCollisionImpactSettingsSelection;

        /// <summary>
        /// What tab is selected in the ability manager animation settings inspector
        /// </summary>
        public int toolbarAbilityManagerAnimationSettingsSelection;

        /// <summary>
        /// What IconUI is currently showing in the inspector 
        /// </summary>
        public int CurrentIconUI = 0;

        /// <summary>
        /// What icon list filter selection is currently selected in the inspector
        /// </summary>
        public int iconUISideListFilterChoice = 0;
        public int iconUISideListPreviousFilterChoice = 0;

        /// <summary>
        /// What Ability Group is currently showing in the inspector 
        /// </summary>
        public int CurrentAbilityGroup = 0;

        /// <summary>
        /// What Weapon is currently showing in the inspector
        /// </summary>
        public int CurrentWeaponIndex = 0;

        /// <summary>
        /// what AI Rule is currently showing in the inspector 
        /// </summary>
        public int CurrentAI = 0;


        /// <summary>
        /// What Ability is currently showing in the inspector
        /// </summary>
        public int CurrentAbility = 0;

        /// <summary>
        /// What ability list filter selection is currently selected in the inspector
        /// </summary>
        public int abilitySideListFilterChoice = 0;
        public int abilitySideListPreviousFilterChoice = 0;

        /// <summary>
        /// If true then the list will become draggable to move abilities around in inspector
        /// </summary>
        public bool DraggableMode = false;


        /// <summary>
        /// For important abilities created by another user. Adding An Exported Abilities object in this property and clicking 'Import' will transfer the abilities to this entity
        /// </summary>
        public ABC_GlobalElement importElementObj;


        /// <summary>
        ///  shows a quick edit list in side inspector
        /// </summary>
        public bool showAbilityQuickEdits = false;

        /// <summary>
        ///  shows a the list of abilities assigned to groups in side inspector
        /// </summary>
        public bool showAbilitiesInGroup = false;


        /// <summary>
        /// show blue guidance boxes in inspector to aid the user 
        /// </summary>
        public bool showHelpInformation = true;

        /// <summary>
        /// If true then inspector editor will have it's values update whilst unity is running in play mode (uses repaint). Will decrease performance of game running. 
        /// </summary>
        public bool updateEditorAtRunTime = false;

        /// <summary>
        /// If true then diagnostic logs will show in the component
        /// </summary>
        public bool enableDiagnosticLogging = false;

        /// <summary>
        /// If true then diagnostic log will also appear in the console window
        /// </summary>
        public bool logDiagnosticToConsole = false;

        /// <summary>
        /// List of diagnostic information
        /// </summary>
        public List<string> diagnosticLog = new List<string>();

        /// <summary>
        /// If true then abilities if set too will be randomly swapped by this component
        /// </summary>
        public bool allowAbilitiesToRandomlySwapPositions = true;

        /// <summary>
        /// The interval delay between ability random position swaps
        /// </summary>
        public float abilityRandomPositionSwapInterval = 3f;

        /// <summary>
        /// If ticked then ABC will automatically pool graphics and objects on start, else if false they will be made on the go
        /// </summary>
        [Tooltip("If ticked then ABC will automatically pool graphics and gameobjects on game start, else if false they will be made on the go")]
        public bool enablePooling = true;

        /// <summary>
        /// The entities camera, used when recording raycasts from camera and working out crosshair positions, if no camera is set the main one is used
        /// </summary>
        public ABC_CameraReference entityCamera;

        /// <summary>
        /// Bool to determine if idle mode can be toggled on and off
        /// </summary>
        [Tooltip("Can Idle mode be toggled.")]
        public bool enableIdleModeToggle = true;


        /// <summary>
        /// If false then the entity is in 'combat' mode and can activate abilities, else if true the entity is not able to activate abilities and is set in IdleMode.
        /// </summary>
        public bool inIdleMode = false;



        /// <summary>
        /// Type of Input to toggle in and out of Idle mode.
        /// </summary>
        [Tooltip("type of input (Key/Button) to toggle in and out of Idle mode.")]
        public InputType idleToggleInputType;

        /// <summary>
        /// The button to toggle in and out of idle mode.
        /// </summary>
        [Tooltip("The Button to toggle in and out of Idle mode.")]
        public string idleToggleButton;

        /// <summary>
        /// The key to toggle in and out of idle mode.
        /// </summary>
        [Tooltip("Key to toggle in and out of Idle mode.")]
        public KeyCode idleToggleKey = KeyCode.End;

        /// <summary>
        /// Delay till idle mode is toggled 
        /// </summary>
        [Tooltip("Delay till idle mode is toggled")]
        public float idleToggleDelay = 0f;

        /// <summary>
        /// if in idle mode the entity can quickly switch out of idle mode (into  combat) by activating an ability, no animation will play currently.
        /// </summary>
        [Tooltip("if in idle mode the entity can quickly switch out of idle mode (into  combat) by activating an ability, no animation will play currently.")]
        public bool deactiveIdleModeOnAbilityInput = true;

        /// <summary>
        /// Enables or disables AI (automatic ability activation)
        /// </summary>
        [Tooltip("Enables or disables AI (automatic ability activation)")]
        public bool enableAI = false;

        /// <summary>
        /// If true then whilst AI is enabled certain triggers can't be activated via key/button triggers. 
        /// Triggers including: weapon switch, parry, block, reload, target triggers etc
        /// </summary>
        [Tooltip("If true then whilst AI is enabled certain triggers can't be activated via key/button triggers. ")]
        public bool aiRestrictSystemTriggers = true;


        /// <summary>
        /// Depending on value will record all objects in the range provided before working out AI - the greater the range here the more powerful the AI  but the more to process
        /// </summary>
        [Tooltip("Depending on value will record all objects in the range provided before working out AI - the greater the range here the more powerful the AI  but the more to process")]
        public float maxAIRange = 100f;

        /// <summary>
        /// The interval between retriving potential targets
        /// </summary>
        [Tooltip("The interval between retriving potential targets")]
        public float aiPotentialTargetRetrievalIntermission = 2f;

        /// <summary>
        /// If true then each time potential targets are retrieved the list is randomized 
        /// </summary>
        [Tooltip("If true then each time potential targets are retrieved the list is randomized ")]
        public bool aiRandomizePotentialTargets = false;

        /// <summary>
        /// the minimum Time interval between starting to decide on which ability to activate automatically
        /// </summary>
        [Tooltip("the minimum Time interval between starting to decide on which ability to activate automatically")]
        public float minimumAICheckIntermission = 5f;

        /// <summary>
        /// the maximum Time interval between starting to decide on which ability to activate automatically
        /// </summary>
        [Tooltip("the maximum Time interval between starting to decide on which ability to activate automatically")]
        public float maximumAICheckIntermission = 8f;


        /// <summary>
        /// Will randomize AI rules each time they are looked at. If the entity has more then 1 100% firing rules this will make sure it doesn't always just activate the ability at the top
        /// </summary>
        [Tooltip("Will randomize AI rules each time they are looked at. If the entity has more then 1 100% firing rules this will make sure it doesn't always just activate the ability at the top")]
        public bool randomizeAIRules = true;


        //if Disabled then entity will not navigate
        [Tooltip("if Disabled then entity will not AI navigate")]
        public bool navAIEnabled = false;

        /// <summary>
        /// If true then the entity will toggle out of idle mode when a destination is set and will toggle back into idle mode when destination is lost
        /// </summary>
        [Tooltip(" If true then the entity will toggle out of idle mode when a destination is set and will toggle back into idle mode when destination is lost")]
        public bool navAIToggleIdleMode = false;

        /// <summary>
        /// Entity will switch into Idle Mode if the current navigation is any of the tags listed below
        /// </summary>
        [Tooltip("Entity will switch into Idle Mode if the current navigation is any of the tags listed below")]
        public List<string> navAITriggerIdleModeForTags = new List<string>();

        /// <summary>
        /// If true then the entity will not move whilst switching in and out of idle mode
        /// </summary>
        [Tooltip("If true then the entity will not move whilst switching in and out of idle mode")]
        public bool navAIPreventMovementWhenSwitchingIdleMode = false;

        /// <summary>
        /// If enabled then entity will wander until the destination has been set
        /// </summary>
        [Tooltip("If enabled then entity will wander until the destination has been set")]
        public bool wanderTillDestinationSet = false;

        /// <summary>
        /// Time between wander destination being updated
        /// </summary>
        [Tooltip("Time between wander destination being updated")]
        public float wanderInterval = 10f;

        /// <summary>
        /// Area range in which the next random position will be selcted
        /// </summary>
        [Tooltip("Area range in which the next random position will be selcted")]
        public float wanderAreaRange = 20f;

        /// <summary>
        /// The speed of the entity when wandering 
        /// </summary>
        [Tooltip("The speed of the entity when wandering ")]
        public float wanderSpeed = 2f;


        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference wanderAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference wanderAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float wanderAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float wanderAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float wanderAnimationRunnerClipDuration = 1f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool wanderAnimationRunnerEnableWeaponAnimationOverrides = false;

        /// <summary>
        /// Name of the wander animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string wanderAnimatorParameter;

        /// <summary>
        /// Type of parameter for the wander animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType wanderAnimatorParameterType;

        /// <summary>
        /// Value to turn on the wander animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string wanderAnimatorOnValue;

        /// <summary>
        /// Value to turn off the wander animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string wanderAnimatorOffValue;

        /// <summary>
        /// How long to play animation for
        /// </summary>
        [Tooltip("How long to play animation for")]
        public float wanderAnimatorDuration = 3f;


        /// <summary>
        /// Destination we are attempting to reach via Tag
        /// </summary>
        [Tooltip("Destination we are trying to reach via Tag")]
        public List<string> destinationTags;

        /// <summary>
        /// Will randomise the tags before attempting to find one
        /// </summary>
        [Tooltip("Will randomise the tags before attempting to find one")]
        public bool randomizeDestinationTags = false;


        /// <summary>
        /// The radius to search for entities with the correct tag
        /// </summary>
        [Tooltip("The radius to search for entities with the correct tag")]
        public float destinationSearchRadius = 25f;

        /// <summary>
        /// The interval before destination changes to a new target, if 0 then this functionality is turned off
        /// </summary>
        [Tooltip("The interval before destination changes to a new target, if 0 then this functionality is turned off")]
        public float aiNavNewDestinationInterval = 15f;


        /// <summary>
        /// Speed of the entity when travelling to destination
        /// </summary>
        [Tooltip("Speed of the entity when travelling to destination")]
        public float speed = 5f;


        /// <summary>
        /// adjustment to the navmesh speed
        /// </summary>
        [Tooltip(" adjustment to the navmesh speed")]
        public float navSpeedAdjustment = 0f;

        /// <summary>
        /// adjustment to the navmesh speed from the weapon equipped
        /// </summary>
        [Tooltip(" adjustment to the navmesh speed from the weapon equipped")]
        public float navWeaponSpeedAdjustment = 0f;


        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference toDestinationAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference toDestinationAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float toDestinationAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float toDestinationAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool toDestinationAnimationRunnerEnableWeaponAnimationOverrides = false;

        /// <summary>
        /// Name of the toDestination animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string toDestinationAnimatorParameter;

        /// <summary>
        /// Type of parameter for the toDestination animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType toDestinationAnimatorParameterType;

        /// <summary>
        /// Value to turn on the toDestination animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string toDestinationAnimatorOnValue;

        /// <summary>
        /// Value to turn off the toDestination animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string toDestinationAnimatorOffValue;


        /// <summary>
        /// The minimum stopping distance from destination
        /// </summary>
        [Tooltip("The minimum stopping distance from destination")]
        [Range(1f, 100f)]
        public float minimumStopDistance = 5f;

        /// <summary>
        /// The maximum stopping distance from destination
        /// </summary>
        [Tooltip("The maximum stopping distance from destination")]
        [Range(1f, 100f)]
        public float maximumStopDistance = 13f;


        /// <summary>
        /// If true then the stopping distance checks will include if the entity agent is currently automatically moving due to nav obstacles so then it won't stop movement animations cause agent velocity is higher then 0 etc
        /// </summary>
        [Tooltip("If true then the stopping distance checks will include if the entity agent is currently automatically moving due to nav obstacles so then it won't stop movement animations cause agent velocity is higher then 0 etc")]
        public bool stopDistanceCheckVelocity = false;

        /// <summary>
        /// If true then the entity will always turn towards the destination
        /// </summary>
        [Tooltip("If true then the entity will always turn towards the destination")]
        public bool alwaysFaceDestination = true;

        /// <summary>
        /// The speed at which the entity will turn to face the destination
        /// </summary>
        [Tooltip("If true then the entity will always turn towards the destination")]
        public float faceDestinationTurnSpeed = 20f;


        /// <summary>
        /// If enabled then the entity will only move towards the destination if it's in line of sight 
        /// </summary>
        [Tooltip("If enabled then the entity will only move towards the destination if it's in line of sight ")]
        public bool lineOfSightRequired = false;

        /// <summary>
        /// Line of sight range
        /// </summary>
        [Tooltip("Line of sight range")]
        public float lineOfSightRange = 25f;


        /// <summary>
        /// If enabled then the entity has the potential to rotate around the destination once reached
        /// </summary>
        [Tooltip("If enabled then the entity has the potential to rotate around the destination once reached")]
        public bool enableRotationBehaviour = false;

        /// <summary>
        /// The minimum time which must pass between rotations
        /// </summary>
        [Tooltip("The minimum time which must pass between rotations")]
        public float rotationInterval = 25f;


        /// <summary>
        /// The speed in which the entity will rotate
        /// </summary>
        [Tooltip("The speed in which the entity will rotate")]
        public float rotationSpeed = 10f;

        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference rotateAroundLeftAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference rotateAroundLeftAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float rotateAroundLeftAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float rotateAroundLeftAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool rotateAroundLeftAnimationRunnerEnableWeaponAnimationOverrides = false;



        /// <summary>
        /// Name of the rotateAroundLeft animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string rotateAroundLeftAnimatorParameter;

        /// <summary>
        /// Type of parameter for the rotateAroundLeft animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType rotateAroundLeftAnimatorParameterType;

        /// <summary>
        /// Value to turn on the rotateAroundLeft animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string rotateAroundLeftAnimatorOnValue;

        /// <summary>
        /// Value to turn off the rotateAroundLeft animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string rotateAroundLeftAnimatorOffValue;


        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference rotateAroundRightAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference rotateAroundRightAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float rotateAroundRightAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float rotateAroundRightAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool rotateAroundRightAnimationRunnerEnableWeaponAnimationOverrides = false;



        /// <summary>
        /// Name of the rotateAroundRight animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string rotateAroundRightAnimatorParameter;

        /// <summary>
        /// Type of parameter for the rotateAroundRight animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType rotateAroundRightAnimatorParameterType;

        /// <summary>
        /// Value to turn on the rotateAroundRight animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string rotateAroundRightAnimatorOnValue;

        /// <summary>
        /// Value to turn off the rotateAroundRight animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string rotateAroundRightAnimatorOffValue;


        /// <summary>
        /// The minimum duration the entity will rotate around for
        /// </summary>
        [Tooltip("The minimum duration the entity will rotate around for")]
        [Range(0f, 100f)]
        public float rotationMinDuration = 4f;


        /// <summary>
        /// The maximum duration the entity will rotate around for
        /// </summary>
        [Tooltip("The maximum duration the entity will rotate around for")]
        [Range(0f, 100f)]
        public float rotationMaxDuration = 9f;


        /// <summary>
        /// If enabled then the entity has the potential to rotate around the destination once reached
        /// </summary>
        [Tooltip("If enabled then the entity has the potential to rotate around the destination once reached")]
        public bool enableDistanceBehaviour = false;

        /// <summary>
        /// The minimum time which must pass between distance change
        /// </summary>
        [Tooltip("The minimum time which must pass between distance change")]
        public float distanceChangeInterval = 40f;


        /// <summary>
        /// The speed in which the entity will move forward or back
        /// </summary>
        [Tooltip("The speed in which the entity will move forward or back")]
        public float moveBackSpeed = 1f;

        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference moveBackAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference moveBackAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float moveBackAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float moveBackAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool moveBackAnimationRunnerEnableWeaponAnimationOverrides = false;

        /// <summary>
        /// Name of the moveBack animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string moveBackAnimatorParameter;

        /// <summary>
        /// Type of parameter for the moveBack animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType moveBackAnimatorParameterType;

        /// <summary>
        /// Value to turn on the moveBack animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string moveBackAnimatorOnValue;

        /// <summary>
        /// Value to turn off the moveBack animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string moveBackAnimatorOffValue;

        /// <summary>
        /// The speed in which the entity will move forward or back
        /// </summary>
        [Tooltip("The speed in which the entity will move forward or back")]
        public float moveForwardSpeed = 1f;

        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference moveForwardAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference moveForwardAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float moveForwardAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float moveForwardAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool moveForwardAnimationRunnerEnableWeaponAnimationOverrides = false;



        /// <summary>
        /// Name of the moveForward animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string moveForwardAnimatorParameter;

        /// <summary>
        /// Type of parameter for the moveForward animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType moveForwardAnimatorParameterType;

        /// <summary>
        /// Value to turn on the moveForward animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string moveForwardAnimatorOnValue;

        /// <summary>
        /// Value to turn off the moveForward animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string moveForwardAnimatorOffValue;

        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference aiFallAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference aiFallAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float aiFallAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float aiFallAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then it is possible for the animation clip setup to be overriden by weapon animation overrides
        /// </summary>
        [Tooltip("If true then it is possible for the animation clip setup to be overriden by weapon animation overrides")]
        public bool aiFallAnimationRunnerEnableWeaponAnimationOverrides = false;



        /// <summary>
        /// Name of the aiFall animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string aiFallAnimatorParameter;

        /// <summary>
        /// Type of parameter for the aiFall animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType aiFallAnimatorParameterType;

        /// <summary>
        /// Value to turn on the aiFall animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string aiFallAnimatorOnValue;

        /// <summary>
        /// Value to turn off the aiFall animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string aiFallAnimatorOffValue;

        /// <summary>
        /// The interval between the last recorded input before the combination of recent inputs is recycled to start fresh
        /// </summary>
        [Tooltip("The interval between the last recorded input before the combination of recent inputs is recycled to start fresh")]
        public float inputComboRecycleInterval = 0.5f;

        /// <summary>
        /// Time till another ability can be used globally. not recast (stop chain casting).
        /// </summary>
        [Tooltip("Time till another ability can be used globally.")]
        public float abilityActivationInterval = 0.4f;


        /// <summary>
        /// Will change the speed of the ability initiation i.e speeding up or slowing down an attack
        /// </summary>
        [Tooltip(" Will change the speed of the ability initiation i.e speeding up or slowing down an attack")]
        public float globalAbilityInitiationSpeedAdjustment = 100f;

        /// <summary>
        /// Will add the following value onto all ability recasts, can be a positive or negative number
        /// </summary>
        [Tooltip("Will add the following value onto all ability recasts, can be a positive or negative number")]
        public float globalAbilityCoolDownAdjustment = 100f;

        /// <summary>
        /// Will add the following value onto all ability preparation times, can be a positive or negative number
        /// </summary>
        [Tooltip("Will add the following value onto all ability preparation times, can be a positive or negative number")]
        public float globalAbilityPrepareTimeAdjustment = 100f;

        /// <summary>
        /// Will add the following value onto all ability crit chance rolls, can be a positive or negative number
        /// </summary>
        [Tooltip("Will add the following value onto all ability crit chance rolls, can be a positive or negative number")]
        public float globalAbilityCritChanceAdjustment = 0f;

        /// <summary>
        /// Will add the following value onto all ability crit potency values, can be a positive or negative number
        /// </summary>
        [Tooltip("Will add the following value onto all ability crit chance rolls, can be a positive or negative number")]
        public float globalAbilityCritPotencyAdjustment = 0f;

        /// <summary>
        /// Determines the chance that an ability can miss the target, will offset on to the position it was intended to go to. 
        /// The higher the chance the more the entities abilities will miss
        /// </summary>
        [Range(0f, 100f)]
        [Tooltip("Determines the chance that an ability can miss the target, will offset on to the position it was intended to go to. The higher the chance the more the entities abilities will miss")]
        public float globalAbilityMissChance = 0f;

        /// <summary>
        /// The minimum offset which will be applied if the ability misses
        /// </summary>
        [Tooltip("The minimum offset which will be applied to the position the ability is travelling towards if the ability is set to miss")]
        public Vector3 abilityMissMinOffset = new Vector3(-7, 0, -6);

        /// <summary>
        /// The maximum offset which will be applied if the ability misses
        /// </summary>
        [Tooltip("The minimum offset which will be applied to the position the ability is travelling towards if the ability is set to miss")]
        public Vector3 abilityMissMaxOffset = new Vector3(8, 7, 6);

        /// <summary>
        /// If true then activating the cancel key will clear targets and stop abilities from activating. Note: Should be false for anyone but the entity controlled by the player
        /// </summary>
        [Tooltip("Does pressing escape key clear targets etc. should be turned off for anyone but controlling player")]
        public bool inputCancelEnabled = true;

        /// <summary>
        /// Input type (Button/Key) to activate the cancel event
        /// </summary>
        [Tooltip("type of input for cancel")]
        public InputType inputCancelInputType;

        /// <summary>
        /// Button to activate the cancel event
        /// </summary>
        [Tooltip("The Button Name to cancel")]
        public string inputCancelButton;

        /// <summary>
        /// Key to activate the cancel event
        /// </summary>
        [Tooltip("The key to click for cancel")]
        public KeyCode inputCancelKey = KeyCode.Escape;

        /// <summary>
        /// If enabled then a hit (from another ability) can prevent the entity from activating abilities
        /// </summary>
        [Tooltip("If enabled then a hit can prevent casting. This is controlled by state effects.")]
        public bool hitsPreventCasting = true;

        /// <summary>
        /// How long a hit will prevent the entity from activating abilities
        /// </summary>
        [Tooltip("How long the hit has prevented casting for")]
        public float hitsPreventCastingDuration = 1.5f;

        /// <summary>
        /// If enabled then a hit (from another ability) can interrupt the entity currently activating an ability 
        /// </summary>
        [Tooltip("If enabled then a hit can interrupt casting. This is controlled by state effects.")]
        public bool hitsInterruptCasting = true;

        /// <summary>
        /// Max mana of the entity
        /// </summary>
        [Tooltip("Max amount of Mana.")]
        public float maxMana = 500f;


        /// <summary>
        ///  Current max mana of the entity
        /// </summary>
        public float currentMaxMana {
            get {

                if (this.manaIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCMaxAttributeValue(this.gcManaID);
                else if (this.manaIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2MaxAttributeValue(this.gcManaID);
                else
                    return this.maxMana;
            }
            set {
                if (this.manaIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcManaID, value, GCStatType.Attribute);
                else if (this.manaIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcManaID, value, GCStatType.Attribute);
                else
                    this.maxMana = value;
            }
        }

        /// <summary>
        /// Current Mana value of the entity
        /// </summary>
        [Tooltip("The mana value of the entity.")]
        public float manaABC = 500;

        /// <summary>
        ///  Current mana of the entity
        /// </summary>
        public float currentMana {
            get {

                if (this.manaIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCStatValue(this.gcManaID, GCStatType.Attribute);
                else if (this.manaIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2StatValue(this.gcManaID, GCStatType.Attribute);
                else
                    return this.manaABC;
            }
            set {
                if (this.manaIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcManaID, value, GCStatType.Attribute);
                else if (this.manaIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcManaID, value, GCStatType.Attribute);
                else
                    this.manaABC = value;

            }
        }


        /// <summary>
        /// The integration type for mana functionality - if ABC is picked then normal functionality is used
        /// else mana from another integration system i.e game creator is used
        /// </summary>
        public ABCIntegrationType manaIntergrationType = ABCIntegrationType.ABC;

        /// <summary>
        /// Game Creator Integration: The ID of the GC stat/attribute which represents mana 
        /// To work correctly the mana value needs to be added as a GC attribute not a stat
        /// </summary>
        public string gcManaID = "mana";

        /// <summary>
        /// How much mana is recovered each tick
        /// </summary>
        [Tooltip("How much mana is restored each second.")]
        public float manaRegenPerSecond = 3;

        /// <summary>
        /// Determines if the entity will restore to full mana when enabled
        /// </summary>
        [Tooltip("Restore full mana on enable.")]
        public bool fullManaOnEnable = true;

        /// <summary>
        /// If enabled then an input can be pressed to equip the next weapon in the list
        /// </summary>
        [Tooltip(" If enabled then an input can be pressed to equip the next weapon in the list")]
        public bool nextWeapon = false;

        /// <summary>
        /// Input type to equip next weapon
        /// </summary>
        [Tooltip("Input type to equip next weapon")]
        public InputType nextWeaponInputType;

        /// <summary>
        /// Button to equip next weapon
        /// </summary>
        [Tooltip("Button to equip next weapon")]
        public string nextWeaponButton;


        /// <summary>
        /// Key to equip next weapon
        /// </summary>
        [Tooltip("Key to equip next weapon")]
        public KeyCode nextWeaponKey;

        /// <summary>
        /// If enabled then an input can be pressed to equip the previous weapon in the list
        /// </summary>
        [Tooltip(" If enabled then an input can be pressed to equip the previous weapon in the list")]
        public bool previousWeapon = false;

        /// <summary>
        /// Input type to equip previous weapon
        /// </summary>
        [Tooltip("Input type to equip previous weapon")]
        public InputType previousWeaponInputType;

        /// <summary>
        /// Button to equip previous weapon
        /// </summary>
        [Tooltip("Button to equip previous weapon")]
        public string previousWeaponButton;


        /// <summary>
        /// Key to equip previous weapon
        /// </summary>
        [Tooltip("Key to equip previous weapon")]
        public KeyCode previousWeaponKey;

        /// <summary>
        /// If true then scrolling the mouse wheel will cycle through weapons
        /// </summary>
        [Tooltip("If true then scrolling the mouse wheel will cycle through weapons")]
        public bool cycleWeaponsUsingMouseScroll = true;

        /// <summary>
        /// Type of input to activate the reload event (Key/Button) on the current equipped weapon
        /// </summary>
        [Tooltip(" Type of input to activate the reload event (Key/Button) on the current equipped weapon")]
        public InputType reloadWeaponInputType;

        /// <summary>
        /// Button to activate the reload event on the current equipped weapon
        /// </summary>
        [Tooltip("Button to activate the reload event on the current equipped weapon")]
        public string reloadWeaponButton;

        /// <summary>
        /// Key to activate the reload event on the current equipped weapon
        /// </summary>
        [Tooltip("Key to activate the reload event on the current equipped weapon")]
        public KeyCode reloadWeaponKey;


        /// <summary>
        /// If true then the entity can not drop a weapon if the entity has less then or equal to x weapons enabled
        /// </summary>
        [Tooltip("If true then the entity can not drop a weapon if the entity has less then x weapons enabled")]
        public bool restrictDropWeaponCount = true;

        /// <summary>
        /// The number which is checked against when restricting drop weapon, the entity must have more then the number defined to drop weapons
        /// </summary>
        [Tooltip("The number which is checked against when restricting drop weapon, the entity must have more then the number defined to drop weapons")]
        public int restrictDropWeaponIfWeaponCountLessOrEqualTo = 1;

        /// <summary>
        /// If true then the entity can parry with weapons
        /// </summary>
        [Tooltip("If true then the entity can parry with weapons")]
        public bool enableWeaponParry = false;

        /// <summary>
        /// Type of input to activate the parry event (Key/Button) on the current equipped weapon
        /// </summary>
        [Tooltip(" Type of input to activate the parry event (Key/Button) on the current equipped weapon")]
        public InputType weaponParryInputType;

        /// <summary>
        /// Button to activate the parry event on the current equipped weapon
        /// </summary>
        [Tooltip("Button to activate the weapon parry event on the current equipped weapon")]
        public string weaponParryButton;

        /// <summary>
        /// Key to activate the parry event on the current equipped weapon
        /// </summary>
        [Tooltip("Key to activate the weapon parry event on the current equipped weapon")]
        public KeyCode weaponParryKey;



        /// <summary>
        /// If true then the entity can block with weapons
        /// </summary>
        [Tooltip("If true then the entity can block with weapons")]
        public bool enableWeaponBlock = false;

        /// <summary>
        /// Type of input to activate the block event (Key/Button) on the current equipped weapon
        /// </summary>
        [Tooltip(" Type of input to activate the reload event (Key/Button) on the current equipped weapon")]
        public InputType weaponBlockInputType;

        /// <summary>
        /// Button to activate the block event on the current equipped weapon
        /// </summary>
        [Tooltip("Button to activate the weapon block event on the current equipped weapon")]
        public string weaponBlockButton;

        /// <summary>
        /// Key to activate the block event on the current equipped weapon
        /// </summary>
        [Tooltip("Key to activate the weapon block event on the current equipped weapon")]
        public KeyCode weaponBlockKey;


        /// <summary>
        /// Max block durability of the entity
        /// </summary>
        [Tooltip("Max amount of block durability.")]
        public float maxBlockDurability = 500f;


        /// <summary>
        ///  Current max block durability of the entity
        /// </summary>
        public float currentMaxBlockDurability {
            get {

                if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCMaxAttributeValue(this.gcBlockDurabilityID);
                else if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2MaxAttributeValue(this.gcBlockDurabilityID);
                else
                    return this.maxBlockDurability;

            }
            set {

                if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcBlockDurabilityID, value, GCStatType.Attribute);
                else if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcBlockDurabilityID, value, GCStatType.Attribute);
                else
                    this.maxBlockDurability = value;

            }
        }

        /// <summary>
        /// Current block durability value of the entity
        /// </summary>
        [Tooltip("The block durability value of the entity.")]
        public float blockDurabilityABC = 500;

        /// <summary>
        ///  Current block durability of the entity
        /// </summary>
        public float currentBlockDurability {
            get {

                if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCStatValue(this.gcBlockDurabilityID, GCStatType.Attribute);
                else if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2StatValue(this.gcBlockDurabilityID, GCStatType.Attribute);
                else
                    return this.blockDurabilityABC;

            }
            set {
                if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcBlockDurabilityID, value, GCStatType.Attribute);
                else if (this.blockDurabilityIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcBlockDurabilityID, value, GCStatType.Attribute);
                else
                    this.blockDurabilityABC = value;
            }
        }


        /// <summary>
        /// The integration type for block durability functionality - if ABC is picked then normal functionality is used
        /// else block durability from another integration system i.e game creator is used
        /// </summary>
        public ABCIntegrationType blockDurabilityIntergrationType = ABCIntegrationType.ABC;

        /// <summary>
        /// Game Creator Integration: The ID of the GC stat/attribute which represents block durability 
        /// To work correctly the block durability value needs to be added as a GC attribute not a stat
        /// </summary>
        public string gcBlockDurabilityID = "blockdurability";

        /// <summary>
        /// How much block durability is recovered each tick
        /// </summary>
        [Tooltip("How much block durability is restored every second.")]
        public float blockDurabilityRegenPerSecond = 10;

        /// <summary>
        /// If true then block durability will only regen when not blocking
        /// </summary>
        [Tooltip("If true then block durability will only regen when not blocking.")]
        public bool blockDurabilityRegenWhenNotBlocking = true;

        /// <summary>
        /// Determines if the entity will restore to full block durability when enabled
        /// </summary>
        [Tooltip("Restore full block durability on enable.")]
        public bool fullBlockDurabilityOnEnable = true;

        /// <summary>
        /// If enabled then an input can be pressed to drop the weapon (if a weapon drop object exists)
        /// </summary>
        [Tooltip("If enabled then an input can be pressed to drop the weapon (if a weapon drop object exists)")]
        public bool allowWeaponDrop = false;

        /// <summary>
        /// Input type to equip next weapon
        /// </summary>
        [Tooltip("Input type to equip next weapon")]
        public InputType weaponDropInputType;

        /// <summary>
        /// Button to equip next weapon
        /// </summary>
        [Tooltip("Button to equip next weapon")]
        public string weaponDropButton;


        /// <summary>
        /// Key to equip next weapon
        /// </summary>
        [Tooltip("Key to equip next weapon")]
        public KeyCode weaponDropKey;


        /// <summary>
        /// If enabled then an input can be pressed to move the current scroll ability to the next in the list
        /// </summary>
        [Tooltip("If enabled then a key can be pressed to select to the next scrollable ability ")]
        public bool nextScroll = true;

        /// <summary>
        /// Input type to select next scroll ability 
        /// </summary>
        [Tooltip("Input type of next scroll")]
        public InputType nextScrollInputType;

        /// <summary>
        /// Button to select next scroll ability  
        /// </summary>
        [Tooltip("The Button Name for next ability")]
        public string nextScrollButton;


        /// <summary>
        /// Key to select next scroll ability 
        /// </summary>
        [Tooltip("Key to select next ability")]
        public KeyCode nextScrollKey;

        /// <summary>
        /// If enabled then an input can be pressed to move the current scroll ability to the previous in the list
        /// </summary>
        [Tooltip("If enabled then a key can be pressed to select to the previous scrollable ability ")]
        public bool previousScroll = true;

        /// <summary>
        /// Input type to select previous scroll ability 
        /// </summary>
        [Tooltip("Input type of previous scroll")]
        public InputType previousScrollInputType;

        /// <summary>
        /// Button to select previous scroll ability  
        /// </summary>
        [Tooltip("The Button Name for previous ability")]
        public string previousScrollButton;

        /// <summary>
        /// Key to select previous scroll ability 
        /// </summary>
        [Tooltip("Key to select previous ability")]
        public KeyCode previousScrollKey;

        /// <summary>
        /// Type of input to activate the reload event (Key/Button) on scroll abilities
        /// </summary>
        [Tooltip("type of input to reload on scroll abilities")]
        public InputType reloadScrollAbilityInputType;

        /// <summary>
        /// Button to activate the reload event on scroll abilities
        /// </summary>
        [Tooltip("The button to reload on scroll abilities")]
        public string reloadScrollAbilityButton;

        /// <summary>
        /// Key to activate the reload event on scroll abilities
        /// </summary>
        [Tooltip("Key to reload on scroll abilities")]
        public KeyCode reloadScrollAbilityKey;

        /// <summary>
        /// Input type to activate the current scroll ability
        /// </summary>
        [Tooltip("Input type of key to activate current selected scroll ability")]
        public InputType activateCurrentScrollAbilityInputType;

        /// <summary>
        /// Button to activate current scroll ability
        /// </summary>
        [Tooltip("The Button Name to activate current selected scroll ability")]
        public string activateCurrentScrollAbilityButton;

        /// <summary>
        /// Key to activate current scroll ability
        /// </summary>
        [Tooltip("Key to activate current selected scroll ability")]
        public KeyCode activateCurrentScrollAbilityKey;

        /// <summary>
        /// If true the entity will always play the animation for selecting the current scroll ability once the object is enabled, else it will never play. 
        /// Example: On respawn if true the entity will power up the staff which fires the current scroll ability
        /// </summary>
        [Tooltip("When the player gets enabled should they play the animation for equipping the default scroll ability?")]
        public bool playScrollAnimationOnEnable;

        /// <summary>
        /// Target select type determines if the a target can be selected via the mouse cursor location or the center crosshair
        /// </summary>
        [Tooltip("How do we select our targets")]
        public TargetSelectType targetSelectType = TargetSelectType.None;

        /// <summary>
        /// If true the target will be soft selected, a target that has been soft selected will need to have a key pressed to change it to a target
        /// </summary>
        [Tooltip("Soft select will not change target unless confirmed by user")]
        public bool selectSoftTarget = false;

        /// <summary>
        /// Max range that the entity can select targets from. If the current target moves out of this range they will also be deselected
        /// </summary>
        [Tooltip("Max range from entity that can be auto targetted")]
        public float targetSelectRange = 40f;

        /// <summary>
        /// The interval between checking if a target has been selected - how often code is run to see if a target has been selected. If 0 is entered then will run with Update
        /// </summary>
        [Tooltip("The interval between checking if a target has been selected - how often code is run to see if a target has been selected. If 0 is entered then will run with Update")]
        public float targetSelectInterval = 0f;

        /// <summary>
        /// The layer mask used for the target select raycast 
        /// </summary>
        [Tooltip("The layer mask used for the target select raycast ")]
        public LayerMask targetSelectLayerMask = -1;

        /// <summary>
        /// If true then an event delegate will be invoked notifiying all subscribed of the target that has been set
        /// </summary>
        [Tooltip("If true then an event delegate will be invoked notifiying all subscribed of the target that has been set")]
        public bool targetSelectRaiseEvent = false;


        /// <summary>
        /// If true then component will send out an accurate cast first - If no targets are found then it will send out a less accurate cast (leeway)
        /// </summary>
        [Tooltip("If true then component will send out an accurate cast first incase targets are close together - If no targets are found then it will send out a less accurate cast (leeway)")]
        public bool targetSelectLeeway = false;

        /// <summary>
        /// The radius of the leeway cast sent out by the target selector. The higher the number the less accurate the crosshair/mouse has to be to select a target. 
        /// </summary>
        /// <remarks>
        /// Will send out an accurate cast first incase targets are close together - If no targets are found then it will send out a less accurate cast
        /// </remarks>
        [Tooltip("The radius of the leeway cast sent out by the target selector. The higher the number the less accurate the crosshair/mouse has to be to select a target.")]
        public float targetSelectLeewayRadius = 0.5f;

        /// <summary>
        /// If true then crosshair functionality can be used
        /// </summary>
        public bool crosshairEnabled = false;

        /// <summary>
        /// Cross hair texture which will appear in the game
        /// </summary>
        [Tooltip("Crosshair texture which will appear in game")]
        public ABC_TextureReference crosshair;

        /// <summary>
        /// X screen position of Crosshair
        /// </summary>
        public float crosshairPositionX = 0.5f;

        /// <summary>
        /// y screen position of Crosshair 
        /// </summary>
        public float crosshairPositionY = 0.5f;

        /// <summary>
        /// If true then a different crosshair will show when a key is pressed (crosshair override). Useful for making a more focused crosshair when targeting.
        /// </summary>
        public bool showCrossHairOnKey = false;

        /// <summary>
        /// Type of input required to show the crosshair override
        /// </summary>
        [Tooltip("the input type to show crosshair")]
        public InputType showCrossHairInputType;

        /// <summary>
        /// Button to press to show the crosshair override
        /// </summary>
        [Tooltip("The Button for showing crosshair")]
        public string showCrossHairButton;

        /// <summary>
        /// Key to press to show the crosshair override
        /// </summary>
        [Tooltip("The key for showing crosshair")]
        public KeyCode showCrossHairKey = KeyCode.Mouse1;

        /// <summary>
        /// Texture which will appear in game when the crosshair override is visible 
        /// </summary>
        [Tooltip("Crosshair texture which will appear in game when the show crosshair keycode is pressed")]
        public ABC_TextureReference crossHairOverride;

        /// <summary>
        /// If true then when the crosshair override is visible then graphics will activate
        /// </summary>
        [Tooltip("Use preparing effects and animations")]
        public bool useCrossHairOverrideAesthetics = false;

        /// <summary>
        /// If true then if the current weapon equipped has crosshair overrides animations setup then those will be used instead of the global ones set. 
        /// </summary>
        [Tooltip("If true then if the current weapon equipped has crosshair overrides animations setup then those will be used instead of the global ones set. ")]
        public bool prioritiseCurrentWeaponCrosshairAnimation = true;

        /// <summary>
        /// If true then the entity will persistently show crosshair aesthetics (animations and graphics)
        /// </summary>
        [Tooltip("If true then the entity will persistently show crosshair aesthetics (animations and graphics)")]
        public bool persistentCrosshairAestheticMode = false;


        /// <summary>
        /// Offset for the crosshair override graphics
        /// </summary>
        [Tooltip("Offset of the preparing effects")]
        public Vector3 crossHairOverrideAestheticsPositionOffset;

        /// <summary>
        /// Forward offset for crosshair override graphics
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float crossHairOverrideAestheticsPositionForwardOffset = 0f;

        /// <summary>
        /// Right offset for crosshair override graphics
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float crossHairOverrideAestheticsPositionRightOffset = 0f;

        /// <summary>
        /// If true then the crosshair override animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate crosshair override animation on the main entities animator")]
        public bool crossHairOverrideAnimateOnEntity = true;

        /// <summary>
        /// If true then the crosshair override animation will activate on the current scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate crosshair override animation on the current scroll ability's graphic animator")]
        public bool crossHairOverrideAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool crossHairOverrideAnimateOnWeapon = false;

        /// <summary>
        /// Name of the animation to play when crosshair override is enabled
        /// </summary>
        [Tooltip("Name of the animation in the controller ")]
        public string crossHairOverrideAnimatorParameter;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference crossHairOverrideAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference crossHairOverrideAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float crossHairOverrideAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float crossHairOverrideAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then the crossHair Override animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate crossHair Override animatio on the main entities animation runner")]
        public bool crossHairOverrideAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the crossHair Override animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate crossHair Override animation on the current scroll ability's graphic animation runner")]
        public bool crossHairOverrideAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool crossHairOverrideAnimationRunnerOnWeapon = false;

        /// <summary>
        /// Type of parameter linked to the crosshair override animation (float, interger, bool, trigger)
        /// </summary>
        [Tooltip("Parameter type to activate animation")]
        public AnimatorParameterType crossHairOverrideAnimatorParameterType;

        /// <summary>
        /// Value to turn on animation
        /// </summary>
        [Tooltip("Value to turn on animation")]
        public string crossHairOverrideAnimatorOnValue;

        /// <summary>
        /// Valye to turn off animation
        /// </summary>
        [Tooltip("Value to turn off animation ")]
        public string crossHairOverrideAnimatorOffValue;

        /// <summary>
        /// Object which is shown when crosshair override is enabled (particle effect)
        /// </summary>
        [Tooltip("Particle or object to show when crossHairOverride")]
        public ABC_GameObjectReference crossHairOverrideParticle;

        /// <summary>
        /// Child object to show when crosshair override is enabled
        /// </summary>
        [Tooltip("Sub graphic or object to show when crossHairOverride")]
        public ABC_GameObjectReference crossHairOverrideObject;

        /// <summary>
        /// Position of the crosshair override graphic (Self, Target, OnObject, Onworld, Camera Center)
        /// </summary>
        [Tooltip("Starting position of the effect")]
        public StartingPosition crossHairOverrideStartPosition;

        /// <summary>
        /// If no target is currently selected for crosshair override graphic starting position then the graphic will as a backup start on the soft target
        /// </summary>
        [Tooltip("If no target is currently selected for crosshair override graphic starting position then the graphic will as a backup start on the soft target")]
        public bool crossHairOverridePositionAuxiliarySoftTarget = false;


        /// <summary>
        /// if starting position is game object then we need to know where
        /// </summary>
        public ABC_GameObjectReference crossHairOverridePositionOnObject;


        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag. Will retrieve the first gameobject with the tag defined. Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string crossHairOverridePositionOnTag;

        /// <summary>
        /// If true then target can be selected by hovering over the object
        /// </summary>
        [Tooltip("Target will be selected by hovering over")]
        public bool hoverForTarget = false;

        /// <summary>
        /// If true then target can be selected via a button press (click)
        /// </summary>
        [Tooltip("Target will be selected by clicking")]
        public bool clickForTarget = true;

        /// <summary>
        /// Input type for clicking for target (Button/Key)
        /// </summary>
        [Tooltip("type of input to click for target")]
        public InputType clickForTargetInputType;

        /// <summary>
        /// Button to click for target
        /// </summary>
        [Tooltip("The Button Name to click for target")]
        public string clickForTargetButton;

        /// <summary>
        /// Key to click for target 
        /// </summary>
        [Tooltip("The key to click for target")]
        public KeyCode clickForTargetKey = KeyCode.Mouse0;

        /// <summary>
        /// If enabled the target can not be deselected
        /// </summary>
        [Tooltip("When enabled user can not deselect a target")]
        public bool disableDeselect = false;

        /// <summary>
        /// If true then the select targeting will only be set when they have the tags defined
        /// </summary>
        [Tooltip("Only tab through to tags defined")]
        public bool selectTargetTagOnly = false;

        /// <summary>
        /// List of tags entity can be selected as a target
        /// </summary>
        public List<string> selectTargetTags = new List<string>();

        /// <summary>
        /// If true then the entity can 'tab' and cycle through targets
        /// </summary>
        [Tooltip("Turns on the option to tab through targets")]
        public bool tabThroughTargets = true;

        /// <summary>
        /// If true then tab target will apply the soft target first
        /// </summary>
        [Tooltip("Soft select will not change target unless confirmed by user")]
        public bool tabSoftTarget = false;

        /// <summary>
        /// Input type to 'tab' to the next target 
        /// </summary>
        [Tooltip("type of input to tab to the next target")]
        public InputType tabTargetNextInputType;

        /// <summary>
        /// Button to tab to next target 
        /// </summary>
        [Tooltip("The Button Name for the next button")]
        public string tabTargetNextButton;

        /// <summary>
        /// Key to tab to next target
        /// </summary>
        [Tooltip("The key to tab to the next target")]
        public KeyCode tabTargetNextKey;

        /// <summary>
        /// Input type to 'tab' to the previous target 
        /// </summary>
        [Tooltip("type of input to tab to the next target")]
        public InputType tabTargetPrevInputType;

        /// <summary>
        /// Button to tab to previous target 
        /// </summary>
        [Tooltip("The Button Name for the next button")]
        public string tabTargetPrevButton;

        /// <summary>
        /// key to tab to previous target 
        /// </summary>
        [Tooltip("The key to tab to the previous target")]
        public KeyCode tabTargetPrevKey;

        /// <summary>
        /// If true then the tab targeting will onlny cycle to targets who have the tags defined
        /// </summary>
        [Tooltip("Only tab through to tags defined")]
        public bool tabTargetTagOnly = false;

        /// <summary>
        /// List of tags entity can tab target through
        /// </summary>
        public List<string> tabTargetTags = new List<string>();

        /// <summary>
        /// If true then targets can only be tabbed to if they are in camera view
        /// </summary>
        [Tooltip("Only tab to targets in camera view")]
        public bool tabTargetInCamera = false;

        /// <summary>
        /// If true then tab targeting can cycle through this entity 
        /// </summary>
        [Tooltip("Can we tab onto self")]
        public bool tabTargetToSelf = false;

        /// <summary>
        /// If true then Auto Target mode will be enabled
        /// </summary>
        [Tooltip("Turns autoTarget on")]
        public bool autoTargetSelect = false;

        /// <summary>
        /// If true then an auto target will be applied with soft target status first 
        /// </summary>
        [Tooltip("Soft select will not change target unless confirmed by user")]
        public bool autoTargetSoftTarget = true;

        /// <summary>
        /// If true then an auto target will be applied with soft target status first, as long as no target currently exists
        /// </summary>
        [Tooltip("If true then an auto target will be applied with soft target status first, as long as no target currently exists")]
        public bool autoTargetSoftTargetOverrideOnlyIfNoTarget = true;

        /// <summary>
        /// The interval that the auto target system will try to find a target 
        /// </summary>
        [Tooltip("How often autoTarget checks")]
        public float autoTargetInterval = 0f;

        /// <summary>
        /// The auto target type. Either automatically find a target or only select a target if a button is pressed on/off or held down. 
        /// </summary>
        [Tooltip("autoTarget types. Hold will only work if this key is held and on release no targets are selected. Press toggles on and off.")]
        public AutoTargetType autoTargetType;

        /// <summary>
        /// Input type to initiate auto target 
        /// </summary>
        [Tooltip("type of input to initiate auto target")]
        public InputType autoTargetInputType;

        /// <summary>
        /// Button to initiate auto target 
        /// </summary>
        [Tooltip("The Input Manager Name for the auto target button")]
        public string autoTargetButton;

        /// <summary>
        /// Key to initiate auto target 
        /// </summary>
        [Tooltip("autoTarget will only work if this key is held on release no targets are selected")]
        public KeyCode autoTargetKey;


        /// <summary>
        /// Auto Target will only select the tags outlined
        /// </summary>
        [Tooltip("Auto Target will only select the tags outlined")]
        public bool autoTargetTagOnly = false;

        /// <summary>
        /// Tags used for auto targeting
        /// </summary>
        [Tooltip("Tags used for auto targeting")]
        public List<string> autoTargetTags = new List<string>();

        /// <summary>
        /// If true then only targets which the camera can see will be auto targetted
        /// </summary>
        [Tooltip("Only targets which the camera can see will be targetted")]
        public bool autoTargetInCamera = true;


        /// <summary>
        /// If true then the auto target system will always swap to the closest target 
        /// </summary>
        [Tooltip("Always swaps to closest target")]
        public bool autoTargetSwapClosest = true;

        /// <summary>
        /// If true then only targets which the entity can see will be targetted
        /// </summary>
        [Tooltip("Only targets which the entity can see will be targetted")]
        public bool autoTargetSelfFacing = false;

        /// <summary>
        /// If true then when auto targetting to closest then it will prioritise objects the entity is facing
        /// </summary>
        [Tooltip("If true then when auto targetting to closest then it will prioritise objects the entity is facing")]
        public bool autoTargetSwapClosestPrioritiseSelfFacing = true;

        /// <summary>
        /// if true then an outline glow will appear when a target is selected
        /// </summary>
        [Tooltip(" if true then an outline glow will appear when a target is selected")]
        public bool targetOutlineGlow = false;


        /// <summary>
        /// The colour of the outline glow when target is selected
        /// </summary>
        [Tooltip("The colour of the outline glow when target is selected")]
        public Color targetOutlineGlowColour = Color.red;


        /// <summary>
        /// Shader which indicates the target has been selected
        /// </summary>
        [Tooltip("Shader which indicates the target has been selected")]
        public ABC_ShaderReference selectedTargetShader;

        /// <summary>
        /// Object which indicates the target has been selected
        /// </summary>
        [Tooltip("Object which shows the target has been selected")]
        public ABC_GameObjectReference selectedTargetIndicator;

        /// <summary>
        /// If true then the object indicator that shows a target has been selected will have it's rotation frozen
        /// </summary>
        [Tooltip("Freezes the rotation of the indicator")]
        public bool targetIndicatorFreezeRotation = true;

        /// <summary>
        /// Offset of the target indicator
        /// </summary>
        [Tooltip("Offset of the indicator")]
        public Vector3 targetIndicatorOffset;

        /// <summary>
        /// Forward offset of the target indicator
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float targetIndicatorForwardOffset = 0f;

        /// <summary>
        /// Right offset from the target indicator
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float targetIndicatorRightOffset = 0f;

        /// <summary>
        /// If true then the target indicator will scale with the target
        /// </summary>
        [Tooltip("Sets if the indicator will scale with the object")]
        public bool targetIndicatorScaleSize = true;

        /// <summary>
        /// The scale factor of the indicator
        /// </summary>
        [Tooltip("How much scale to add on")]
        public float targetIndicatorScaleFactor = 1;

        /// <summary>
        /// if true then an outline glow will appear when a soft target is selected
        /// </summary>
        [Tooltip(" if true then an outline glow will appear when a soft target is selected")]
        public bool softTargetOutlineGlow = false;


        /// <summary>
        /// The colour of the outline glow when target is selected
        /// </summary>
        [Tooltip("The colour of the outline glow when a soft target is selected")]
        public Color softTargetOutlineGlowColour = Color.white;


        /// <summary>
        /// Shader which indicates the soft target has been selected
        /// </summary>
        [Tooltip("Shader which indicates the soft target has been selected")]
        public ABC_ShaderReference softTargetShader;

        /// <summary>
        /// Object which indicates the soft target has been soft selected
        /// </summary>
        [Tooltip("Object which shows the soft target has been selected")]
        public ABC_GameObjectReference softTargetIndicator;

        /// <summary>
        /// If true then the object indicator that shows a target has been soft selected will have it's rotation frozen
        /// </summary>
        [Tooltip("Freezes the rotation of the indicator")]
        public bool softTargetIndicatorFreezeRotation = true;

        /// <summary>
        /// Offset of the soft target indicator
        /// </summary>
        [Tooltip("Offset of the indicator")]
        public Vector3 softTargetIndicatorOffset;

        /// <summary>
        /// Forward offset of the soft target indicator
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float softTargetIndicatorForwardOffset = 0f;

        /// <summary>
        /// Right offset of the soft target indicator
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float softTargetIndicatorRightOffset = 0f;

        /// <summary>
        /// If true then the soft target indicator will scale with the target
        /// </summary> 
        [Tooltip("Sets if the indicator will scale with the object")]
        public bool softTargetIndicatorScaleSize = true;

        /// <summary>
        /// The scale factor of the indicator
        /// </summary>
        [Tooltip("How much scale to add on")]
        public float softTargetIndicatorScaleFactor = 1;

        /// <summary>
        /// The type of input to confirm the soft target. By confirming the soft target will be converted to a target
        /// </summary>
        [Tooltip("type of input for the soft target confirmation")]
        public InputType softTargetConfirmInputType;

        /// <summary>
        /// Button for the soft target confirmation
        /// </summary>
        [Tooltip("The Button Name used to confirm the soft target converting it to the new target")]
        public string softTargetConfirmButton;

        /// <summary>
        /// Key for the soft target confirmation
        /// </summary>
        [Tooltip("The key used to confirm the soft target converting it to the new target")]
        public KeyCode softTargetConfirmKey = KeyCode.KeypadEnter;


        /// <summary>
        /// If true then an event delegate will be invoked notifiying all subscribed of the target that has been set
        /// </summary>
        [Tooltip("If true then an event delegate will be invoked notifiying all subscribed of the target that has been set")]
        public bool softTargetSetRaiseEvent = false;



        /// <summary>
        /// Slider which shows the preparing bar information	
        /// </summary>
        [Tooltip("Slider which shows the casting bar information")]
        public ABC_SliderReference preparingAbilityGUIBar;

        /// <summary>
        /// GuiText which displays what ability the entity is currently preparing
        /// </summary>
        [Tooltip("GuiText which displays what the entity is currently preparing")]
        public ABC_TextReference preparingAbilityGUIText;

        /// <summary>
        /// Gui text which logs whats going on in the game regarding abilities 
        /// </summary>
        [Tooltip("GUIText Object to display the logging")]
        public ABC_TextReference abilityLogGUIText;

        /// <summary>
        /// Gui text which shows the ammo for the current scroll ability 
        /// </summary>
        [Tooltip("GUIText Object to display ammo information")]
        public ABC_TextReference scrollAbilityammoGUIText;

        /// <summary>
        /// RawImage to display what the current ScrollAbilityImage is
        /// </summary>
        [Tooltip("RawImage to display what the current ScrollAbilityImage is")]
        public ABC_RawImageReference scrollAbilityImageGUI;

        /// <summary>
        /// Gui text which shows the ammo for the equipped weapon
        /// </summary>
        [Tooltip("GUIText Object to display ammo for the equipped weapon")]
        public ABC_TextReference weaponAmmoGUIText;

        /// <summary>
        /// RawImage to display what the current equipped weapon is
        /// </summary>
        [Tooltip("RawImage to display what the current equipped weapon is")]
        public ABC_RawImageReference weaponImageGUI;

        /// <summary>
        /// If true then logs when an ability can be cast again
        /// </summary>
        [Tooltip("Logs when an ability can be cast again")]
        public bool logReadyToCastAgain = false;

        /// <summary>
        /// If true then logs when the target is out of range
        /// </summary>
        [Tooltip("Logs when the target is out of range")]
        public bool logRange = true;

        /// <summary>
        /// If true then logs when ability not used due to entity not facing the target
        /// </summary>
        [Tooltip("Logs when ability not used due to entity not facing the target")]
        public bool logFacingTarget = true;

        /// <summary>
        /// If true then logs when a target has been selected by FPS center screen
        /// </summary>
        [Tooltip("Logs when a target has been selected by FPS center screen")]
        public bool logFpsSelection = false;

        /// <summary>
        /// If true then logs when a target has been selected by the mouse
        /// </summary>
        [Tooltip("Logs when a target has been selected by the mouse")]
        public bool logTargetSelection = false;

        /// <summary>
        /// If true then logs when a target has been selected by FPS center screen
        /// </summary>
        [Tooltip("Logs when a target has been selected by FPS center screen")]
        public bool logSoftTargetSelection = false;

        /// <summary>
        /// If true then logs when a world position has been selected
        /// </summary>
        [Tooltip("Logs when a world position has been selected")]
        public bool logWorldSelection = false;

        /// <summary>
        /// If true then logs the reason as to why an ability was unable to activate
        /// </summary>
        [Tooltip("Logs the reason as to why an ability was unable to activate")]
        public bool logAbilityActivationError = false;

        /// <summary>
        /// If true then logs when the entity has not enough mana to use ability
        /// </summary>
        [Tooltip("Logs when the entity has not enough mana to use ability")]
        public bool logNoMana = false;

        /// <summary>
        /// If true then logs when the entity is preparing an ability
        /// </summary>
        [Tooltip("Logs when the entity is preparing an ability")]
        public bool logPreparing = false;



        /// <summary>
        /// If true then logs when preparing an ability has failed 
        /// </summary>
        [Tooltip("Logs when preparing an ability has failed")]
        public bool logAbilityInterruption = false;

        /// <summary>
        /// If true then logs when the entity is iniating an ability
        /// </summary>
        [Tooltip("Logs when the entity is iniating an ability")]
        public bool logInitiating = false;

        /// <summary>
        /// If true then logs when the ability has been fired 
        /// </summary>
        [Tooltip("Logs when the ability has been fired")]
        public bool logAbilityActivation = false;

        /// <summary>
        /// If true then logs when an scroll ability gets equipped
        /// </summary>
        [Tooltip("Logs when an scroll ability gets equipped")]
        public bool logScrollAbilityEquip = false;

        /// <summary>
        /// If true then logs Ammo details
        /// </summary>
        [Tooltip("Logs Ammo details")]
        public bool logAmmoInformation = false;

        /// <summary>
        /// If true then logs when an ability is blocked/unblocked
        /// </summary>
        [Tooltip("If true then logs when an ability is blocked/unblocked")]
        public bool logBlockingInformation = false;

        /// <summary>
        /// If true then logs when an weapon is equipped/unequipped
        /// </summary>
        [Tooltip("If true then logs when an weapon is equipped/unequipped")]
        public bool logWeaponInformation = false;

        /// <summary>
        /// If true then logs when an ability is parried
        /// </summary>
        [Tooltip("If true then logs when an weapon is parried")]
        public bool logParryInformation = false;

        /// <summary>
        /// Max number of lines that will show in the ability log 
        /// </summary>
        [Tooltip("Max number of lines that will show in the ability log  ")]
        public int abilityLogMaxLines = 5;

        /// <summary>
        /// If true then the ability log can be configured to only appear for a duration after an entry is added
        /// </summary>
        [Tooltip("If true then the ability log can be configured to only appear for a duration after an entry is added")]
        public bool abilityLogUseDuration = false;

        /// <summary>
        /// The duration in which the ability log will be shown for
        /// </summary>
        [Tooltip("The duration in which the ability log will be shown for")]
        public float abilityLogDuration = 4f;

        /// <summary>
        /// The current combo key (Which key was last pressed as part of a combo)
        /// </summary>
        public KeyCode currentComboKey = KeyCode.None;

        /// <summary>
        /// The current combo button (Which key was last pressed as part of a combo)
        /// </summary>
        public string currentComboButton;


        #endregion



        // *********************  Variables ********************

        #region Variables

        /// <summary>
        /// Records all inputs made during play - ready to be used to determine if a key input combo should trigger an ability activation
        /// </summary>
        public List<KeyCode> recordedKeyInputHistory = new List<KeyCode>();

        /// <summary>
        /// Time of the last recording of a key input
        /// </summary>
        private float timeOfLastKeyInputRecorded = 0;

        /// <summary>
        /// Stores a list of all keys required to activiate abilities set to trigger from a combinaiton of key inputs
        /// </summary>
        /// <remarks>Allows for ABC to only check for inputs that are required to activate input combo abilities, ignoring other unsued keys (which might be used for movement etc)</remarks>
        private List<KeyCode> abilityInputComboKeys = new List<KeyCode>();

        /// <summary>
        /// Determines the longest number of inputs required to trigger an ability (i.e records the highest possible combination required to activate an abilty)
        /// </summary>
        private int abilityLongestInputCombo = 0;


        /// <summary>
        /// Tracks which weapons is currently enabled and equipped
        /// </summary>
        [System.NonSerialized]
        private Weapon _currentEquippedWeapon = null;


        /// <summary>
        /// Tracks which weapons is currently enabled and equipped
        /// </summary>
        public Weapon CurrentEquippedWeapon {
            get {
                //If current equipped weapon is null and we have a saved ID, or the current equipped weapon doesn't match the ID then retrieve the current equipped weapon from ID 
                //ID variable is always the master i.e Save/Loading
                if (this._currentEquippedWeapon == null && this.currentEquippedWeaponID > -1 || this._currentEquippedWeapon != null && this._currentEquippedWeapon.weaponID != this.currentEquippedWeaponID)
                    this._currentEquippedWeapon = this.FindWeapon(this.currentEquippedWeaponID);

                return this._currentEquippedWeapon;
            }

            set {
                this._currentEquippedWeapon = value;

                //Update ID
                if (value != null)
                    this.currentEquippedWeaponID = value.weaponID;
                else
                    this.currentEquippedWeaponID = -1;
            }
        }


        /// <summary>
        /// Tracks which weaponID is currently enabled and equipped (Used for save and loading)
        /// </summary>
        public int currentEquippedWeaponID = -1;

        /// <summary>
        /// If true then the entity is currently toggling (enabling/disabling) weapons
        /// </summary>
        private bool weaponBeingToggled = false;


        /// <summary>
        /// If true then weapon parrying is disabled
        /// </summary>
        private bool weaponParryingDisabled = false;

        /// <summary>
        /// If true then the entity is currently parrying
        /// </summary>
        private bool isCurrentlyWeaponParrying {
            get {

                if (this.CurrentEquippedWeapon == null)
                    return false;

                return this.CurrentEquippedWeapon.IsWeaponParrying();
            }
        }


        /// <summary>
        /// If true then weapon blocking is disabled
        /// </summary>
        private bool weaponBlockingDisabled = false;

        /// <summary>
        /// If true then the entity is currently blocking
        /// </summary>
        private bool isCurrentlyWeaponBlocking {
            get {

                if (this.CurrentEquippedWeapon == null)
                    return false;

                return this.CurrentEquippedWeapon.IsWeaponBlocking();
            }
        }

        /// <summary>
        /// Whilst true entity will start weapon blocking 
        /// </summary>
        private bool autoWeaponBlock = false;

        /// <summary>
        /// If true then the entity is currently switching in or out of idle mode
        /// </summary>
        private bool idleModeBeingToggled = false;


        /// <summary>
        /// Tracks the current scroll ability. A scroll ability is where one button always fires but the user can cycle through different abilities. Example: Mouse to shoot but Gun, Machine Gun and Rocket Launcher you can scroll through. 
        /// </summary>
        [System.NonSerialized]
        private ABC_Ability _currentScrollAbility = null;

        /// <summary>
        /// Tracks the current scroll ability. A scroll ability is where one button always fires but the user can cycle through different abilities. Example: Mouse to shoot but Gun, Machine Gun and Rocket Launcher you can scroll through. 
        /// </summary>
        public ABC_Ability CurrentScrollAbility {
            get {
                //If current scroll ability is null and we have a saved ID, or the current scroll ability doesn't match the ID then retrieve the current scroll ability from ID 
                //ID variable is always the master i.e Save/Loading
                if (this._currentScrollAbility == null && this.currentScrollAbilityID > -1 || this._currentScrollAbility != null && this._currentScrollAbility.abilityID != this.currentScrollAbilityID)
                    this._currentScrollAbility = this.FindAbility(this.currentScrollAbilityID);

                return this._currentScrollAbility;
            }

            set {
                this._currentScrollAbility = value;

                //Update ID
                if (value != null)
                    this.currentScrollAbilityID = value.abilityID;
                else
                    this.currentScrollAbilityID = -1;
            }
        }

        /// <summary>
        /// Tracks which ScrollAbilityID is currently enabled and equipped (Used for save and loading)
        /// </summary>
        public int currentScrollAbilityID = -1;


        /// <summary>
        /// Tracks melee abilities that are currently active, used when the activation is over but the attack is still going and a reference is needed when interrupting etc (unlike projectiles melee stop instantly)
        /// </summary>
        private Dictionary<ABC_Ability, List<GameObject>> activeMeleeAbilities = new Dictionary<ABC_Ability, List<GameObject>>();

        /// <summary>
        /// Will track which abilities the entity is currently activating. This list is populated by the ability class
        /// </summary>
        private List<ABC_Ability> activatingAbilities = new List<ABC_Ability>();


        /// <summary>
        /// Bool which defines if the entity can activate abilities or not. 
        /// </summary>
        private bool canActivateAbilities = true;


        /// <summary>
        /// Time of the last ability activation
        /// </summary>
        private float timeOfLastAbilityActivation = 0f;

        /// <summary>
        /// The additional adjustment made to the activation interval (can be positive or negative). This is reset after the following interval is over.
        /// </summary>
        private float tempAbilityActivationIntervalAdjustment = 0f;


        /// <summary>
        /// Time interval between starting to decide on which ability to activate automatically
        /// </summary>
        private float AICheckIntermission = 0f;

        /// <summary>
        /// A list of all potential targets which AI will be conditioned against and abilities activated on
        /// </summary>
        private List<ABC_IEntity> potentialTargets = new List<ABC_IEntity>();

        /// <summary>
        /// The time of the last potential target retrieval 
        /// </summary>
        private float timeOfLastAIPotentialTargetRetrieval = 0f;


        /// <summary>
        /// The time of the last AI Action, use to determine if the interval is up to next check AI Rules
        /// </summary>
        private float timeOfLastAIAction = 0f;

        /// <summary>
        /// If true then AI Navigation will be blocked 
        /// </summary>
        private bool blockNavAI = false;

        /// <summary>
        /// Tracks the current AI Navigation destination
        /// </summary>
        private Transform currentNavAIDestination = null;

        /// <summary>
        /// Determines if the AI Nav destination has been set
        /// </summary>
        private bool navAIDestinationSet = false;

        /// <summary>
        /// Determines if the wander destination has been set 
        /// </summary>
        private bool navAIWanderSet = false;

        /// <summary>
        /// Tracks when we last set the wander destination so wandering isn't repeated before the interval time
        /// </summary>
        private float navAIWanderSetTime = 0f;

        /// <summary>
        /// Tracks what time the entity last stopped animation so it's not spammed (coroutines need to run)
        /// </summary>
        private float navAIStopAnimationTime = 0f;

        /// <summary>
        /// If true then entity will rotate around current destination
        /// </summary>
        private bool navAIRotateAroundDestination = false;

        /// <summary>
        /// What direction the entity was rotating around (-1 is off, 0 is right, 1 is left)
        /// </summary>
        private int navAIRotateAroundDirection = -1;

        /// <summary>
        /// Duration of the rotation around destination
        /// </summary>
        private float navAIRotateAroundDestinationDuration = 0f;

        /// <summary>
        /// The time in which the entity started to rotate around the destination
        /// </summary>
        private float navAIRotateAroundDestinationStartTime = 0f;

        /// <summary>
        /// The time in which the entity started to change distance from destination
        /// </summary>
        private float navAIChangeDestinationDistanceStartTime = 0f;

        /// <summary>
        /// If true then entity will move back from current destination
        /// </summary>
        /// <remarks>Used for when the stopping distance is changed, changing this variable to true will move the entity back so it matches the new stopping distance (if it was higher then before) </remarks>
        private bool navAIChangeDestinationMovingAway = false;

        /// <summary>
        /// True if the entity is currently moving towards the destination due to an at destination behaviour
        /// </summary>
        private bool navAIChangeDestinationMovingTowards = false;

        /// <summary>
        /// The distance between the entity and the destination when the entity started moving forward. This is so if the destination changes the behaviour can stop
        /// </summary>
        private float navAIChangeDestinationMoveTowardsStartingDistance = 0;

        /// <summary>
        /// Determines if the AI Nav is currently falling
        /// </summary>
        private bool navAIFalling = false;

        /// <summary>
        /// Bool which defines if the entity has been restricted from activating abilities due to a hit. 
        /// </summary>
        public bool hitRestrictsAbilityActivation = false;

        /// <summary>
        /// Bool which defines if the entity has been restricted from activiating abilities as a toggled ability is active. 
        /// </summary>
        public bool toggledAbilityRestrictsAbilityActivation = false;


        /// <summary>
        /// Current target of the entity
        /// </summary>
        private GameObject targetObject;

        /// <summary>
        /// The shader which the current target had before it was changed to the target shader
        /// </summary>
        private Shader targetsPreviousShader;

        /// <summary>
        /// Current soft target of the entity. it's not the real target but an indication to the player of what the next target will be when the confirm key is pressed
        /// </summary>
        private GameObject softTarget;

        /// <summary>
        /// The shader which the current soft target had before it was changed to the target shader
        /// </summary>
        private Shader softTargetsPreviousShader;

        /// <summary>
        /// If true then soft target will always be applied and override any settings
        /// </summary>
        private bool softTargetOverride = false;


        /// <summary>
        /// Current world target object 
        /// </summary>
        public GameObject worldTargetObject;

        /// <summary>
        /// Current world target position
        /// </summary>
        public Vector3 worldTargetPosition;

        /// <summary>
        /// Bool which determines if entity is waiting on a new target to be selected
        /// </summary>
        private bool waitingOnNewTarget = false;


        /// <summary>
        /// Bool which determines if entity is waiting on a new world target to be selected
        /// </summary>
        private bool waitingOnNewWorldTarget = false;

        /// <summary>
        /// Bool which determines if entity is waiting on a new mouse target to be selected
        /// </summary>
        private bool waitingOnNewMouseTarget = false;


        /// <summary>
        /// keeps track of our tab targets for scrolling through 
        /// </summary>
        private List<ABC_IEntity> CurrentTabTargets = new List<ABC_IEntity>();


        /// <summary>
        /// Stores the target Indicator for this entity (1 per ABC_Controller) 
        /// </summary>
        private GameObject targetIndicator;

        /// <summary>
        /// stores the soft target Indicator for this entity (1 per ABC_Controller)
        /// </summary>
        private GameObject targetIndicatorSoft;

        /// <summary>
        /// Pool that holds the ability range indicator object
        /// </summary>
        public GameObject abilityRangeIndicatorObj = null;

        /// <summary>
        /// Pool that holds the ability world target indicator object
        /// </summary>
        public GameObject abilityWorldTargetIndicatorObj = null;


        /// <summary>
        /// Pool that holds the ability mouse target indicator object
        /// </summary>
        public GameObject abilityMouseTargetIndicatorObj = null;

        /// <summary>
        /// Bool which determines if the crosshair override is currently active
        /// </summary>
        private bool showCrossHairOverride = false;


        /// <summary>
        /// Stores the CrossHairOverride graphic for this entity (1 per ABC_Controller)
        /// </summary>
        private GameObject CrossHairOverrideGraphic = null;


        /// <summary>
        /// If true then the crosshairoverride will be blocked
        /// </summary>
        private bool blockCrosshairOverride = false; 



        /// <summary>
        /// Used for inspector only to toggle the fold out for integrations
        /// </summary>
        public bool foldOutIntegration = false;

        // originator class 
        ABC_IEntity meEntity;


        // player transform and navmeshagent
        Transform meTransform;
        NavMeshAgent meNavAgent;

        // animator object 
        Animator Ani;




        #endregion


        //**************** ENUM  ************************

        #region ENUMS



        public enum AutoTargetType {
            Auto = 0,
            Hold = 1,
            Press = 2
        }



        public enum AIRuleTarget {
            Original = 0,
            Self = 1,
            Nearest = 2,
            TargetOfNearest = 3
        }




        private enum ControllerButtonPressState {
            TargetClick,
            Cancel,
            IdleToggle,
            NextTarget,
            PreviousTarget,
            ConfirmSoftTarget,
            AutoTarget,
            ActivateCurrentScrollAbility,
            NextScrollAbility,
            PreviousScrollAbility,
            ShowCrossHair,
            ScrollAbilityReload,
            NextWeapon,
            PreviousWeapon,
            DropCurrentWeapon,
            WeaponReload,
            WeaponBlock,
            WeaponParry

        }




        private enum ControllerButtonPressType {
            Press,
            Hold
        }



        // only used in controller class
        private enum ControllerGraphicType {
            CrossHairOverride
        }


        private enum ControllerAnimationState {
            CrossHairOverride,
            AINavWander,
            AINavMoveToDestination,
            AINavRotateAroundLeft,
            AINavRotateAroundRight,
            AINavMoveBack,
            AINavMoveForward,
            AINavFalling
        }


        public enum WeaponState {
            Equip = 0,
            UnEquip = 1,
            Drop = 2,
            Reload = 3,
            Block = 4,
            UnBlock = 5,
            BlockReaction = 6,
            AttackReflected = 7,
            Parry = 8,
            CrossHairOverride = 9
        }

        #endregion


        // ****************** Public Methods ***************************

        #region  Public Methods


        /// <summary>
        /// Will add the text provided to the diagnostic log which can be displayed in the editor
        /// </summary>
        /// <param name="TextLog">Text to add to the diagnostic log</param>
        public void AddToDiagnosticLog(string TextLog) {

            if (this.logDiagnosticToConsole)
                Debug.Log(TextLog);

            //If turned on then write to the diagnostic log
            if (this.enableDiagnosticLogging)
                this.diagnosticLog.Add(System.DateTime.Now + ": " + TextLog);


        }

        /// <summary>
        /// Will clear the current diagnostic log which is displayed in the editor
        /// </summary>
        public void ClearDiagnosticLog() {

            this.diagnosticLog.Clear();

        }

        /// <summary>
        /// Will refresh the component recalling Awake and the initialise component method (normally called onenable)
        /// </summary>
        /// <remarks>
        /// Used when loading data from a save
        /// </remarks>
        public void Reload() {
            // stop any invokes/coroutines
            CancelInvoke();
            StopAllCoroutines();

            this.Awake();

            //Reinitalise the component but skip mana restore and ability group initial setup. 
            StartCoroutine(InitialiseComponent(true));
        }

        /// <summary>
        /// Will reload the global elements but making sure any changes made during play are still in affect (enable status, ammo count etc)
        /// </summary>
        /// <remarks>Used when loading game data new version of game might make ability changes but we want to sustain anything changed during game state</remarks>
        public void ReloadGlobalElements() {

            //Abilities
            foreach (ABC_Ability ability in this.Abilities) {

                //check if global 

                if (ability.globalAbilities != null) {

                    foreach (ABC_Ability globalAbility in ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalAbilities)) {

                        ABC_Ability newGlobalAbility = new ABC_Ability();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(globalAbility), newGlobalAbility);


                        //Link this ability to the source
                        newGlobalAbility.globalElementSource = ability.globalAbilities;

                        //Copy over the details of the game type modification so on reload this can be reapplied to each instance
                        newGlobalAbility.globalAbilitiesEnableGameTypeModification = ability.globalAbilitiesEnableGameTypeModification;
                        newGlobalAbility.globalAbilitiesGameTypeModification = ability.globalAbilitiesGameTypeModification;


                        //override ability enable status if set too
                        if (ability.globalAbilityOverrideEnableStatus == true)
                            newGlobalAbility.abilityEnabled = ability.abilityEnabled;

                        //override ability key if set too
                        if (ability.globalAbilityOverrideKeyTrigger == true) {
                            newGlobalAbility.key = ability.key;
                            newGlobalAbility.keyButton = ability.keyButton;
                        }

#if ABC_GC_2_Integration
                        if (meEntity.HasGC2CharacterComponent())
                            newGlobalAbility.AdjustAbilityForGameCreator2();
#endif

                        //Do Game type modification if enabled
                        if (newGlobalAbility.globalAbilitiesEnableGameTypeModification == true)
                            newGlobalAbility.ConvertToGameType(newGlobalAbility.globalAbilitiesGameTypeModification);


                        //find matching ability 
                        ABC_Ability currentGlobalAbility = this.CurrentAbilities.Where(a => a.abilityID == globalAbility.abilityID).FirstOrDefault();

                        //Overwrite with values which may have changed during play
                        if (currentGlobalAbility != null) {

                            //State and Ammo
                            newGlobalAbility.abilityEnabled = currentGlobalAbility.abilityEnabled;
                            newGlobalAbility.ammoCount = currentGlobalAbility.ammoCount;


                            //Reupdate game type modification
                            newGlobalAbility.globalAbilitiesEnableGameTypeModification = currentGlobalAbility.globalAbilitiesEnableGameTypeModification;
                            newGlobalAbility.globalAbilitiesGameTypeModification = currentGlobalAbility.globalAbilitiesGameTypeModification;

                            // reapply game type modification on this reload

#if ABC_GC_2_Integration
                        if (meEntity.HasGC2CharacterComponent())
                            newGlobalAbility.AdjustAbilityForGameCreator2();
#endif

                            if (newGlobalAbility.globalAbilitiesEnableGameTypeModification == true)
                                newGlobalAbility.ConvertToGameType(newGlobalAbility.globalAbilitiesGameTypeModification);




                            //overwrite current ability;
                            this.CurrentAbilities[this.CurrentAbilities.IndexOf(currentGlobalAbility)] = newGlobalAbility;
                        } else {

                            this.CurrentAbilities.Add(newGlobalAbility);


                        }

                    }

                }

            }


            foreach (Weapon weapon in this.Weapons) {

                //check if global 

                if (weapon.globalWeapon != null) {


                    Weapon newGlobalWeapon = new Weapon();
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(weapon.globalWeapon.ElementWeapon), newGlobalWeapon);

                    //Link this weapon to the source
                    newGlobalWeapon.globalElementSource = weapon.globalWeapon;


                    //override  enable status if set too
                    if (weapon.globalWeaponOverrideEnableStatus == true)
                        newGlobalWeapon.weaponEnabled = weapon.weaponEnabled;


                    //find matching weapon 
                    Weapon currentGlobalWeapon = this.CurrentWeapons.Where(w => w.weaponID == weapon.globalWeapon.ElementWeapon.weaponID).FirstOrDefault();

                    if (currentGlobalWeapon != null) {

                        //Clear current graphics 
                        currentGlobalWeapon.ClearObjectPools();

                        //Make sure the ability in the global ability element matches the enable state
                        newGlobalWeapon.weaponEnabled = currentGlobalWeapon.weaponEnabled;
                        newGlobalWeapon.weaponAmmoCount = currentGlobalWeapon.weaponAmmoCount;

                        this.CurrentWeapons[this.CurrentWeapons.IndexOf(currentGlobalWeapon)] = newGlobalWeapon;

                        //If weapon was equipped then make sure we still tracking it
                        if (CurrentEquippedWeapon != null && CurrentEquippedWeapon == currentGlobalWeapon)
                            CurrentEquippedWeapon = newGlobalWeapon;

                    } else {

                        this.CurrentWeapons.Add(newGlobalWeapon);

                    }

                }

            }
        }

        /// <summary>
        /// Will clear all pools
        /// </summary>
        public void ClearAllPools() {


            // create weapon object pools, no need to clear pool as there is only ever 1 graphic in cycle, which gets overwritten on creation
            foreach (Weapon weapon in CurrentWeapons) {
                weapon.ClearObjectPools();
            }

            // clear the ability group pools then create new ones
            foreach (AbilityGroup group in AbilityGroups) {
                group.ClearObjectPools();
            }


            // clear the ability pools then create new ones
            foreach (ABC_Ability a in CurrentAbilities) {
                a.ClearObjectPools();
            }


            // clear target indicator 
            if (this.targetIndicator != null) {
                Destroy(this.targetIndicator);
            }


            // clear current soft target indicator 
            if (this.targetIndicatorSoft != null) {
                Destroy(targetIndicatorSoft);
            }


            // clear current soft target indicator 
            if (this.abilityRangeIndicatorObj != null) {
                Destroy(abilityRangeIndicatorObj);
            }


            // clear current soft target indicator 
            if (this.abilityWorldTargetIndicatorObj != null) {
                Destroy(abilityWorldTargetIndicatorObj);
            }


            // clear current soft target indicator 
            if (this.abilityMouseTargetIndicatorObj != null) {
                Destroy(abilityMouseTargetIndicatorObj);
            }


            // clear current ability indicator
            if (this.abilityWorldTargetIndicatorObj != null) {
                Destroy(this.abilityWorldTargetIndicatorObj);
            }

            // delete crossHair graphic 
            if (this.CrossHairOverrideGraphic != null) {
                Destroy(this.CrossHairOverrideGraphic);
            }


        }

        /// <summary>
        /// Will pool all ability objects and other objects setup by this component. 
        /// </summary>
        public void CreatePools() {

            this.ClearAllPools();

            //If pooling is not enabled then end here, objects will be created on the way
            if (this.enablePooling == false)
                return;


            // create weapon object pools, no need to clear pool as there is only ever 1 graphic in cycle, which gets overwritten on creation
            foreach (Weapon weapon in CurrentWeapons) {
                weapon.CreateObjectPools();
            }

            // clear the ability group pools then create new ones
            foreach (AbilityGroup group in AbilityGroups) {
                group.CreateObjectPools();
            }


            // clear the ability pools then create new ones
            foreach (ABC_Ability a in CurrentAbilities) {
                a.CreateObjectPools();
            }


            // If this controller is using target indicators then create it if it doesn't exist 
            if (this.selectedTargetIndicator.GameObject != null) {
                // instantiate the material and assign it to our targetIndicator variable to be used throughout the game
                this.targetIndicator = (GameObject)(Instantiate(selectedTargetIndicator.GameObject));
                this.targetIndicator.name = meTransform.name.ToString() + "_ABC_TargetIndicator";

                if (this.targetIndicatorFreezeRotation == true) {
                    this.targetIndicator.AddComponent<ABC_FreezeRotation>();
                }

                ABC_Utilities.PoolObject(this.targetIndicator);

            }

            // if were using soft targeting indicators then create it if doesn't exist 
            if (this.softTargetIndicator.GameObject != null) {
                // instantiate the material and assign it to our softTargetIndicator variable to be used throughout the game
                this.targetIndicatorSoft = (GameObject)(Instantiate(softTargetIndicator.GameObject));
                this.targetIndicatorSoft.name = meTransform.name.ToString() + "_ABC_SoftTargetIndicator";


                if (this.softTargetIndicatorFreezeRotation == true) {
                    this.targetIndicatorSoft.AddComponent<ABC_FreezeRotation>();
                }


                ABC_Utilities.PoolObject(this.targetIndicatorSoft);

            }


            //Create Ability World Target Indicator
            GameObject resourceObj = (GameObject)Resources.Load("ABC-TargetIndicator/ABC_AbilityWorldTargetIndicator");

            if (resourceObj != null) {
                this.abilityWorldTargetIndicatorObj = GameObject.Instantiate(resourceObj);
                this.abilityWorldTargetIndicatorObj.name = this.name.ToString() + "_ABC_WorldTargetIndicator";
                ABC_Utilities.PoolObject(this.abilityWorldTargetIndicatorObj);
            }

            //Create Ability Range Indicator
            resourceObj = (GameObject)Resources.Load("ABC-TargetIndicator/ABC_AbilityRangeIndicator");

            if (resourceObj != null) {
                this.abilityRangeIndicatorObj = GameObject.Instantiate(resourceObj);
                this.abilityRangeIndicatorObj.name = this.name.ToString() + "_ABC_AbilityRangeIndicator";
                ABC_Utilities.PoolObject(this.abilityRangeIndicatorObj);
            }


            //Create Ability Mouse Target Indicator
            resourceObj = (GameObject)Resources.Load("ABC-TargetIndicator/ABC_AbilityMouseTargetIndicator");

            if (resourceObj != null) {
                this.abilityMouseTargetIndicatorObj = GameObject.Instantiate(resourceObj);
                this.abilityMouseTargetIndicatorObj.name = this.name.ToString() + "_ABC_AbilityMouseTargetIndicator";
                ABC_Utilities.PoolObject(this.abilityMouseTargetIndicatorObj);
            }



            // instantiate the CrossHairOverride graphics (this only needs to be done once)
            if (this.showCrossHairOnKey == true && this.useCrossHairOverrideAesthetics == true && this.crossHairOverrideParticle.GameObject != null) {

                // store main graphic
                this.CrossHairOverrideGraphic = (GameObject)(Instantiate(crossHairOverrideParticle.GameObject));
                this.CrossHairOverrideGraphic.name = crossHairOverrideParticle.GameObject.name;

                // copy child object for additional Aesthetic 
                if (this.crossHairOverrideObject.GameObject != null) {
                    GameObject crossHairOverrideChildAB = (GameObject)(Instantiate(crossHairOverrideObject.GameObject));
                    crossHairOverrideChildAB.name = crossHairOverrideObject.GameObject.name;
                    crossHairOverrideChildAB.transform.position = CrossHairOverrideGraphic.transform.position;
                    crossHairOverrideChildAB.transform.rotation = CrossHairOverrideGraphic.transform.rotation;
                    crossHairOverrideChildAB.transform.parent = CrossHairOverrideGraphic.transform;
                }


                ABC_Utilities.PoolObject(this.CrossHairOverrideGraphic);

            }
        }

        /// <summary>
        /// Will run through each ability calling the function which ensures that all trigger linked abilities have the same key/button set
        /// </summary>
        public void SetupAbilityTriggerLinks() {

            // clear the ability pools then create new ones
            foreach (ABC_Ability a in CurrentAbilities)
                a.AbilityTriggerLinkSetup(meEntity);

        }



        /// <summary>
        /// Will find and return an IconUI
        /// </summary>
        /// <param name="IconID">ID of the icon to find</param>
        /// <returns>IconUI found from the ID given</returns>
        public ABC_IconUI FindIconUI(int IconID) {

            return this.IconUIs.Where(a => a.iconID == IconID).FirstOrDefault();

        }

        /// <summary>
        /// Will find and return an IconUI
        /// </summary>
        /// <param name="IconName">Name of the icon to find</param>
        /// <returns>IconUI found from the ID given</returns>
        public ABC_IconUI FindIconUI(string IconName) {

            return this.IconUIs.Where(a => a.iconName == IconName).FirstOrDefault();

        }

        /// <summary>
        /// Will setup and initialise all UI Icons setup on abilities
        /// </summary>
        public void SetupUIIcons() {

            for (int i = 0; i < this.IconUIs.Count(); i++)
                this.IconUIs[i].Initialise(meEntity);

        }

        /// <summary>
        /// Will enable or disable the AI for the entity
        /// </summary>
        /// <param name="Enabled">True if to enable AI, else false</param>
        public void ToggleAI(bool Enabled = true) {

            //Toggle the AI
            this.enableAI = Enabled;

        }


        /// <summary>
        /// Will enable or disable the AI Navigation block for the entity, if blocked then the navigation will not run
        /// </summary>
        /// <param name="Enabled">True if to block AI Navigation, else false</param>
        public void BlockAINavigation(bool Enabled = true) {

            //Toggle the AI
            this.blockNavAI = Enabled;

        }

        /// <summary>
        /// Will immediately stop any destination behaviours currently in progress
        /// </summary>
        public void AINavigationStopDestinationBehaviours() {

            //Stop moving away
            if (this.navAIChangeDestinationMovingAway) {
                this.navAIChangeDestinationMovingAway = false;
                this.EndAnimation(ControllerAnimationState.AINavMoveBack, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavMoveBack, meEntity.animationRunner);
            }

            //Stop moving towards
            if (this.navAIChangeDestinationMovingTowards) {
                this.navAIChangeDestinationMovingTowards = false;
                this.navAIChangeDestinationMoveTowardsStartingDistance = 0f;
                this.EndAnimation(ControllerAnimationState.AINavMoveForward, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavMoveForward, meEntity.animationRunner);
            }

            //Stop rotating around
            if (this.navAIRotateAroundDestination) {
                this.navAIRotateAroundDestination = false;
                this.navAIRotateAroundDirection = 0;
                this.EndAnimation(ControllerAnimationState.AINavRotateAroundRight, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavRotateAroundRight, meEntity.animationRunner);
                this.EndAnimation(ControllerAnimationState.AINavRotateAroundLeft, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavRotateAroundLeft, meEntity.animationRunner);
            }

        }


        /// <summary>
        /// Will add a new UI Icon to the entity
        /// </summary>
        /// <param name="Icon">Icon object to add</param>
        public void AddIconUI(ABC_IconUI Icon) {

            this.IconUIs.Add(Icon);
            Icon.Initialise(meEntity);

        }

        /// <summary>
        /// Will remove the UI Icon from the entity
        /// </summary>
        /// <param name="Icon">Icon object to remove</param>
        public void RemoveIconUI(ABC_IconUI Icon) {

            this.IconUIs.Remove(Icon);

        }


        /// <summary>
        /// Will delete an Icon  
        /// </summary>
        /// <param name="IconName">Name of the Icon  to delete</param>
        public void DeleteIconUI(string IconName) {

            //Find weapon with the ID
            ABC_IconUI icon = this.FindIconUI(IconName);

            if (icon == null)
                return;

            this.RemoveIconUI(icon);
        }

        /// <summary>
        /// Will delete an Icon  
        /// </summary>
        /// <param name="IconID">ID of the Icon  to delete</param>
        public void DeleteIconUI(int IconID) {

            //Find weapon with the ID
            ABC_IconUI icon = this.FindIconUI(IconID);

            if (icon == null)
                return;

            this.RemoveIconUI(icon);
        }

        /// <summary>
        /// Will find all icons that are setup to equip the weapon passed in the parameter
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to find links to UI icons</param>
        /// <returns>List of ID's of Icons which equip the weapon passed through in parameters</returns>
        public List<int> FindIconsLinkedToWeapon(int WeaponID) {

            List<int> retVal = new List<int>();

            //Cycle through all icons adding those which link to the weapon ID passed in the parameter
            foreach (ABC_IconUI icon in this.IconUIs) {

                if (icon.iconUIWeaponID == WeaponID)
                    retVal.Add(icon.iconID);

            }

            //Return result
            return retVal;
        }

        /// <summary>
        /// Returns the entities camera, used when recording raycasts from camera and working out crosshair positions, if no camera is set the main one is returned
        /// </summary>
        public Camera GetEntitiesCamera() {

            if (this.entityCamera.Camera == null)
                return Camera.main;

            return this.entityCamera.Camera;

        }

        /// <summary>
        /// Will enable or disable idle mode
        /// </summary>
        /// <param name="Enabled">True to toggle into idle mode, else false to toggle out of idle mode </param>
        /// <param name="Instant">(Optional) If set to <c>true</c> then idle mode is switched instantly else it waits a certain time defined in settings.</param>
        public IEnumerator ToggleIdleMode(bool Enabled, bool Instant = false) {

            //If toggle into idle mode and currently not in idle mode
            if (Enabled == true && this.inIdleMode == false) {
                yield return StartCoroutine(SwitchIdleMode(Instant));
            } else if (Enabled == false && this.inIdleMode == true) {
                yield return StartCoroutine(SwitchIdleMode(Instant)); // else if in idle mode and we are toggling out of idle mode to battle mode
            }

        }

        /// <summary>
        /// Switchs the idle mode.
        /// </summary>
        /// <param name="Instant">(Optional) If set to <c>true</c> then idle mode is switched instantly else it waits a certain time defined in settings.</param>
        public IEnumerator SwitchIdleMode(bool Instant = false) {


            if (enableIdleModeToggle == false || this.idleModeBeingToggled == true)
                yield break;

            //Tell rest of code idle mode is being toggled
            this.idleModeBeingToggled = true;


            // If we are not switching instantly then we wait for the toggle delay (allow time for animations in future). 
            if (Instant == false && idleToggleDelay > 0f)
                yield return new WaitForSeconds(idleToggleDelay);


            if (inIdleMode == true) {


                // no longer in idle mode (battle ready!)
                inIdleMode = false;

                // log the switch
                if (logReadyToCastAgain == true)
                    StartCoroutine(this.AddToAbilityLog("Switching out of Idle stance"));


                // turn scroll ability back on
                // if instant is true then we will won't play animation and load graphic faster
                yield return StartCoroutine(this.EnableScrollAbility(this.CurrentScrollAbility, (Instant) ? false : true));

                //Equip weapon
                if (this.CurrentEquippedWeapon != null)
                    yield return StartCoroutine(this.ToggleWeapon(CurrentEquippedWeapon, WeaponState.Equip, (Instant) ? true : false));


            } else {

                // we are in idle mode
                inIdleMode = true;

                // log if were in combat mode
                if (logReadyToCastAgain == true)
                    StartCoroutine(this.AddToAbilityLog("Switching into Idle stance"));

                // turn of current scroll ability
                // if instant is true then we will won't play animation
                yield return StartCoroutine(this.DisableScrollAbility(this.CurrentScrollAbility, (Instant) ? false : true));

                //unequip weapon
                if (this.CurrentEquippedWeapon != null)
                    yield return StartCoroutine(this.ToggleWeapon(CurrentEquippedWeapon, WeaponState.UnEquip, (Instant) ? true : false));




            }

            //Tell rest of code idle mode is no longer being toggled
            this.idleModeBeingToggled = false;

        }

        /// <summary>
        /// Will make the entity aware that an AI navigation target is around, used when being hit when looking in other direction etc
        /// If the entity doesn't have a current AI navigation target then this function will 
        /// turn the entity to face the AI navigation target that it's now aware of for the AI handler to do the rest of logic
        /// </summary>
        /// <param name="EntityToBeAwareOf">Object that this entity should be aware of</param>
        public void AINavigationAlert(ABC_IEntity EntityToBeAwareOf) {

            //If AI navigation not enabled, entity is activating an ability or doesn't have a AI tag we care about then end here
            if (this.currentNavAIDestination != null || this.navAIEnabled == false || this.blockNavAI == true || this.blockNavAI == true || this.IsActivatingAbility() == true || ABC_Utilities.ObjectHasTag(EntityToBeAwareOf.gameObject, ABC_Utilities.ConvertTags(this.meEntity, this.destinationTags)) == false)
                return;

            //Are we already facing target?
            var dir = (EntityToBeAwareOf.transform.position - meEntity.transform.position).normalized;
            var dot = Vector3.Dot(dir, meEntity.transform.forward);

            if (dot >= 0.3)
                return;    //  were facing the target so already alerted so can end here


            //If this far then we are not facing the alerted entity so will turn to face it, the AI handler will pick up the rest
            meEntity.TurnTo(EntityToBeAwareOf.gameObject);


        }

        /// <summary>
        /// Will clear the current destination stopping the navagent from moving towards anywhere 
        /// </summary>
        public void ClearAINavigationDestination() {

            if (this.navAIEnabled == false)
                return;

            if (this.meNavAgent != null && meNavAgent.isOnNavMesh) {
                this.meNavAgent.isStopped = true;
                this.meNavAgent.ResetPath();
            }

            this.currentNavAIDestination = null;
            this.navAIDestinationSet = false;
            this.navAIWanderSet = false;

            //End all navigation animations
            this.EndAnimation(ControllerAnimationState.AINavMoveToDestination, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavMoveToDestination, meEntity.animationRunner);
            this.EndAnimation(ControllerAnimationState.AINavWander, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavWander, meEntity.animationRunner);
            this.EndAnimation(ControllerAnimationState.AINavRotateAroundLeft, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavRotateAroundLeft, meEntity.animationRunner);
            this.EndAnimation(ControllerAnimationState.AINavRotateAroundRight, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavRotateAroundRight, meEntity.animationRunner);
            this.EndAnimation(ControllerAnimationState.AINavMoveBack, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavMoveBack, meEntity.animationRunner);
            this.EndAnimation(ControllerAnimationState.AINavMoveForward, this.Ani);
            this.EndAnimationRunner(ControllerAnimationState.AINavMoveForward, meEntity.animationRunner);

        }

        /// <summary>
        /// Will adjust nav agent speed
        /// </summary>
        /// <param name="SpeedAdjustment">Amount to adjust the speed by</param>
        public void AdjustNavSpeed(float SpeedAdjustment) {

            //If AI Nav not enabled then end here
            if (this.navAIEnabled == false)
                return;

            //Add adjustment
            this.navSpeedAdjustment = SpeedAdjustment;

        }

        /// <summary>
        /// Will adjust nav agent speed from weapon equipped
        /// </summary>
        /// <param name="SpeedAdjustment">Amount to adjust the speed by</param>
        public void SetWeaponNavSpeed(float SpeedAdjustment) {

            //If AI Nav not enabled then end here
            if (this.navAIEnabled == false)
                return;

            //Add adjustment
            this.navWeaponSpeedAdjustment = SpeedAdjustment;

        }


        /// <summary>
        /// Will activate/disable the Mana GUI for the entity 
        /// </summary>
        /// <param name="Enabled">If true will enable the GUI, else disable it</param>
        public void ShowManaGUI(bool Enabled = true) {

            // no mana guis to update so return here
            if (this.ManaGUIList.Count == 0)
                return;


            foreach (ManaGUI gui in this.ManaGUIList) {
                gui.ToggleSliderGUI(Enabled);
                gui.ToggleTextGUI(Enabled);
            }


        }

        /// <summary>
        /// Determines if information is allowed to be display for the logging type given
        /// </summary>
        /// <param name="LoggingType">Type of logging (Ability Activation/Interruption etc)</param>
        /// <returns>True if information is allowed to be displayed, else false.</returns>
        public bool LogInformationAbout(LoggingType LoggingType) {

            switch (LoggingType) {
                case LoggingType.AbilityActivation:
                    return this.logAbilityActivation;

                case LoggingType.AbilityActivationError:
                    return this.logAbilityActivationError;

                case LoggingType.AbilityInterruption:
                    return this.logAbilityInterruption;

                case LoggingType.AmmoInformation:
                    return this.logAmmoInformation;

                case LoggingType.FacingTarget:
                    return this.logFacingTarget;

                case LoggingType.FpsSelection:
                    return this.logFpsSelection;

                case LoggingType.Initiating:
                    return this.logInitiating;

                case LoggingType.NoMana:
                    return this.logNoMana;

                case LoggingType.Preparing:
                    return this.logPreparing;

                case LoggingType.Range:
                    return this.logRange;

                case LoggingType.ReadyToCastAgain:
                    return this.logReadyToCastAgain;

                case LoggingType.ScrollAbilityEquip:
                    return this.logScrollAbilityEquip;

                case LoggingType.SoftTargetSelection:
                    return this.logSoftTargetSelection;

                case LoggingType.TargetSelection:
                    return this.logTargetSelection;

                case LoggingType.WorldSelection:
                    return this.logWorldSelection;

                case LoggingType.BlockingInformation:
                    return this.logBlockingInformation;

                case LoggingType.WeaponInformation:
                    return this.logWeaponInformation;

                case LoggingType.ParryInformation:
                    return this.logParryInformation;


                default:
                    return false;


            }
        }

        /// <summary>
        /// Will return the current recorded key input history 
        /// </summary>
        /// <returns>List of recent key inputs made by the entity</returns>
        public List<KeyCode> GetRecordedKeyInputHistory() {

            return this.recordedKeyInputHistory;
        }

        /// <summary>
        /// Adds the string provided to the ability log if setup correctly. If log is currently at or greater then the max lines provided then the oldest message will be removed.
        /// </summary>
        /// <param name="Text">Message to add to log</param>
        public IEnumerator AddToAbilityLog(string Text) {

            if (abilityLogGUIText.text == null)
                yield break;

            // Split log so we can see how many lines it currently has
            string[] logSplit = this.abilityLogGUIText.text.Split("\n"[0]);

            // if we are over or at the line count limit then remove the oldest message
            if (logSplit.Length >= this.abilityLogMaxLines + 1) {

                // clear current log
                this.abilityLogGUIText.text = "";

                // re-add to log everything except the oldest line (element 0 in array)
                for (int i = 1; i < logSplit.Length - 1; i++) {
                    this.abilityLogGUIText.text += logSplit[i] + "\n";
                }

            }

            //add new text to the end of the log
            this.abilityLogGUIText.text += Text + "\n";


            //enable log
            abilityLogGUIText.Graphic.enabled = true;

            //Determine if the log should only appear for a small while
            if (this.abilityLogUseDuration) {

                //Store what is currently present in effect log
                string savedLog = abilityLogGUIText.text;

                //Wait for duration
                yield return new WaitForSeconds(this.abilityLogDuration);

                //If the text hasn't changed (in other words another entry has not been made meaning effect log needs to reset duration) then turn off log. 
                if (savedLog == abilityLogGUIText.text)
                    abilityLogGUIText.Graphic.enabled = false;


            }

        }

        /// <summary>
        /// Set the key of the ability 
        /// </summary>
        /// <param name="AbilityID">Ability to modify</param>
        /// <param name="Key">Key to set the ability too</param>
        /// <param name="Originator">Entity that activated the ability</param>
        public void SetAbilityKey(int AbilityID, KeyCode Key, ABC_IEntity Originator = null) {
            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            ability.SetKey(Key, Originator);
        }

        /// <summary>
        /// Set the button of the ability 
        /// </summary>
        /// <param name="AbilityID">Ability to modify</param>
        /// <param name="KeyButton">Button to set the ability too</param>
        /// <param name="Originator">Entity that activated the ability</param>
        public void SetAbilityKey(int AbilityID, string KeyButton, ABC_IEntity Originator = null) {
            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            ability.SetKey(KeyButton, Originator);
        }



        /// <summary>
        /// Will trigger the ability starting the activation
        /// </summary>
        /// <param name="AbilityID">ID of ability to trigger</param>
        public void TriggerAbility(int AbilityID) {
            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            ability.AutoCast(meEntity);

        }

        /// <summary>
        /// Will trigger the ability starting the activation
        /// </summary>
        /// <param name="AbilityName">Name of ability to trigger</param>
        public void TriggerAbility(string AbilityName) {
            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            ability.AutoCast(meEntity);

        }

        /// <summary>
        /// Will force ability activation ignoring all trigger and activation permitted checks
        /// </summary>
        /// <param name="AbilityID">ID of ability to force activation</param>
        public void ForceAbilityActivation(int AbilityID) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            //Activate ability ignoring trigger, activation permitted and combo checks
            this.ActivateAbility(ability, true, true, true);


        }

        /// <summary>
        /// Will force ability activation ignoring all trigger and activation permitted checks
        /// </summary>
        /// <param name="AbilityID">name of the ability to force activation</param>
        public void ForceAbilityActivation(string AbilityName) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            //Activate ability ignoring trigger, activation permitted and combo checks
            this.ActivateAbility(ability, true, true, true);


        }

        /// <summary>
        /// Will trigger the current scroll ability if one is active
        /// </summary>
        public void TriggerCurrentScrollAbility() {

            if (this.CurrentScrollAbility != null)
                this.TriggerAbility(this.CurrentScrollAbility.abilityID);

        }

        /// <summary>
        /// Will raise the ability activation delegate event
        /// </summary>
        /// <param name="AbilityName">Name of ability that is activating</param>
        /// <param name="AbilityID">ID of ability that is activating</param>
        public void RaiseAbilityActivationEvent(string AbilityName, int AbilityID) {

            if (this.onAbilityActivation != null)
                this.onAbilityActivation(AbilityName, AbilityID);

        }

        /// <summary>
        /// Will raise the entities ability activation completed delegate event
        /// </summary>
        /// <param name="AbilityName">Name of ability that completed activating</param>
        /// <param name="AbilityID">ID of ability that completed activating</param>
        public void RaiseAbilityActivationCompleteEvent(string AbilityName, int AbilityID) {

            if (this.onAbilityActivationComplete != null)
                this.onAbilityActivationComplete(AbilityName, AbilityID);

        }


        /// <summary>
        /// Will raise the entities ability before target event stating if entity is in ability before target state or not
        /// </summary>
        /// <param name="AbilityID">ID of ability that completed activating</param>
        /// <param name="Enabled">True if ability before target enabled, else false if disabled</param>
        public void RaiseAbilityBeforeTargetEvent(int AbilityID, bool Enabled) {

            if (this.onAbilityBeforeTarget != null)
                this.onAbilityBeforeTarget(AbilityID, Enabled);

        }

        /// <summary>
        /// Will get the effects from the ability ID provided 
        /// </summary>
        /// <param name="AbilityID">ID of ability whose effects will be applied</param>
        public List<Effect> GetAbilitiesEffects(int AbilityID) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return null;

            return ability.GetAllEffects();

        }

        /// <summary>
        /// Will get the effects from the ability name provided 
        /// </summary>
        /// <param name="AbilityName">Name of ability whose effects will be applied</param>
        public List<Effect> GetAbilitiesEffects(string AbilityName) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return null;

            return ability.GetAllEffects();

        }

        /// <summary>
        /// Will apply the effects from the ability ID provided to the target object provided
        /// </summary>
        /// <param name="AbilityID">ID of ability whose effects will be applied</param>
        /// <param name="EffectTarget">Target Object of the effects</param>
        /// <param name="AddedBySplash">(Optional) If true then effect was added by splash functionality </param>
        public void ApplyAbilitiesEffects(int AbilityID, GameObject EffectTarget, bool AddedBySplash = false) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            ability.ApplyAbilityEffectsToObject(EffectTarget, meEntity, null, default(Vector3), AddedBySplash, true);

        }

        /// <summary>
        /// Will apply the effects from the ability ID provided to the target object provided
        /// </summary>
        /// <param name="AbilityName">Name of ability whose effects will be applied</param>
        /// <param name="EffectTarget">Target Object of the effects</param>
        /// <param name="AddedBySplash">(Optional) If true then effect was added by splash functionality </param>
        public void ApplyAbilitiesEffects(string AbilityName, GameObject EffectTarget, bool AddedBySplash = false) {

            //Find ability 
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            ability.ApplyAbilityEffectsToObject(EffectTarget, meEntity, null, default(Vector3), AddedBySplash);

        }

        /// <summary>
        /// Will return a list of all ability names 
        /// </summary>
        /// <returns>List of all ability names</returns>
        public List<string> GetAllAbilityNames(bool EnabledOnly = false) {

            //If enabledonly is true then only return names of abilities currently enabled in game
            if (EnabledOnly)
                return CurrentAbilities.Where(ability => ability.abilityEnabled == true).Select(ability => ability.name).ToList();
            else
                return CurrentAbilities.Select(ability => ability.name).ToList();

        }


        /// <summary>
        /// Will find and return an ability
        /// </summary>
        /// <param name="AbilityID">ID of the ability to find</param>
        /// <returns>Ability found from the ID given</returns>
        public ABC_Ability FindAbility(int AbilityID) {

            return this.CurrentAbilities.Where(a => a.abilityID == AbilityID).FirstOrDefault();

        }

        /// <summary>
        /// Will find and return an ability
        /// </summary>
        /// <param name="AbilityName">Name of the ability to find</param>
        /// <returns>Ability found from the Name given</returns>
        public ABC_Ability FindAbility(string AbilityName) {

            return this.CurrentAbilities.Where(a => a.name == AbilityName).FirstOrDefault();

        }


        /// <summary>
        /// Returns a dictionary detailing information about the ability requested including name, description etc
        /// </summary>
        /// <param name="AbilityID">ID of the ability</param>
        /// <returns>Dictionary holding information regarding the ability</returns>
        public Dictionary<string, string> GetAbilityDetails(int AbilityID) {
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability != null)
                return ability.GetAbilityDetails(meEntity);

            return null;
        }

        /// <summary>
        /// Returns a dictionary detailing information about the ability requested including name, description etc
        /// </summary>
        /// <param name="AbilityName">Name of ability</param>
        /// <returns>Dictionary holding information regarding the ability</returns>
        public Dictionary<string, string> GetAbilityDetails(string AbilityName) {
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability != null)
                return ability.GetAbilityDetails(meEntity);

            return null;
        }

        /// <summary>
        /// Will return the texture Icon for the ability passed in the method
        /// </summary>
        /// <param name="AbilityID">ID of the ability</param>
        /// <returns>Texture2D Icon of the ability passed</returns>
        public Texture2D GetAbilityIcon(int AbilityID) {

            Texture2D retval = null;
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability != null)
                retval = ability.iconImage.Texture2D;

            return retval;

        }

        /// <summary>
        /// Will return a string which represents what key/button will trigger the current scroll ability. 
        /// </summary>
        /// <returns>String of the key or button which will trigger the the current scroll ability</returns>
        public string GetScrollAbilityActivationKey() {

            if (this.activateCurrentScrollAbilityInputType == InputType.Button)
                return this.activateCurrentScrollAbilityButton;
            else
                return this.activateCurrentScrollAbilityKey.ToString();

        }

        /// <summary>
        /// Returns the key/button which will trigger the ability
        /// </summary>
        /// <param name="AbilityID">ID of the ability</param>
        /// <returns>String of the key or button which will trigger the ability</returns>
        public string GetAbilityKey(int AbilityID) {

            ABC_Ability ability = this.FindAbility(AbilityID);

            //if ability not found return empty string
            if (ability == null)
                return "";


            //return the abilities key trigger
            return ability.GetKey();

        }

        /// <summary>
        /// Returns the key/button which will trigger the ability
        /// </summary>
        /// <param name="AbilityName">Name of ability</param>
        /// <returns>String of the key or button which will trigger the ability</returns>
        public string GetAbilityKey(string AbilityName) {

            ABC_Ability ability = this.FindAbility(AbilityName);

            //if ability not found return empty string
            if (ability == null)
                return "";


            //return the abilities key trigger
            return ability.GetKey();
        }

        /// <summary>
        ///  Will use the ability ID provided to find the next avaliable (not combo blocked) ability ID in it's combo chain
        /// </summary>
        /// <param name="AbilityID">ID of the ability used to find the next combo ability</param>
        /// <param name="AIActivated">(Optional)If true then the next combo ID has been requested AI which means function will add leeway onto the combo next time, allowing for more time to pass before combo resets </param>
        /// <returns>ID of the next ability in the combo chain or -1 if not found</returns>
        public int GetAbilityNextAvaliableComboID(int AbilityID, bool AIActivated = false) {

            //Find the ability we are dealing with
            ABC_Ability ability = this.FindAbility(AbilityID);

            //get a list of all our potential combo abilities which are
            // Enabled, Set as Ability combos, With same input type as ability provided
            List<ABC_Ability> potentialComboAbilities = this.CurrentAbilities.Where(a => a.abilityEnabled == true && a.abilityCombo == true && a.keyInputType == ability.keyInputType && (a.LandOrAir == ABC_Ability.AbilityLandOrAir.LandOrAir || a.LandOrAir == ability.LandOrAir)).ToList();

            //Find index of current ability so we can work our way from the next one
            int currentIndex = potentialComboAbilities.IndexOf(ability) + 1;

            for (int i = currentIndex; i < potentialComboAbilities.Count(); i++) {

                //If a key trigger and keys match and the ability isn't combo blocked then return the ID
                if (potentialComboAbilities[i].keyInputType == InputType.Key && potentialComboAbilities[i].key == ability.key && potentialComboAbilities[i].IsComboBlocked(meEntity, true, AIActivated) == false)
                    return potentialComboAbilities[i].abilityID;
                else if (ability.keyInputType == InputType.Button && potentialComboAbilities[i].keyButton == ability.keyButton && potentialComboAbilities[i].IsComboBlocked(meEntity, true, AIActivated) == false)  //If a button trigger and buttons match return the ID
                    return potentialComboAbilities[i].abilityID;

            }


            //If this far then nothing was found so return -1
            return -1;

        }

        /// <summary>
        /// Will use the ability ID provided to find the next avaliable (not combo blocked) ability ID in it's combo chain
        /// </summary>
        /// <param name="AbilityName">Name of the ability used to find the next combo ability</param>
        /// <param name="AIActivated">(Optional)If true then the next combo ID has been requested AI which means function will add leeway onto the combo next time, allowing for more time to pass before combo resets </param>
        /// <returns>ID of the next ability in the combo chain</returns>
        public int GetAbilityNextAvaliableComboID(string AbilityName, bool AIActivated = false) {

            //Find the ability we are dealing with
            ABC_Ability ability = this.FindAbility(AbilityName);

            return this.GetAbilityNextAvaliableComboID(ability.abilityID);

        }

        /// <summary>
        /// Will determine if the ability provided is the last in the combo chain 
        /// </summary> 
        /// <param name="AbilityID">ID of the ability to check if it's last in a combo chain </param>
        /// <returns>True if Ability is last in the combo, else false</returns>
        public bool IsAbilityLastInComboChain(int AbilityID) {

            //Find the ability we are dealing with
            ABC_Ability ability = this.FindAbility(AbilityID);


            //get a list of all our potential combo abilities which are
            // Enabled, Set as Ability combos, With same input type as ability provided
            List<ABC_Ability> potentialComboAbilities = this.CurrentAbilities.Where(a => a.abilityEnabled == true && a.abilityCombo == true && (a.LandOrAir == ABC_Ability.AbilityLandOrAir.LandOrAir || a.LandOrAir == ability.LandOrAir)
            && (a.keyInputType == InputType.Key && ability.keyInputType == InputType.Key && a.key == ability.key || a.keyInputType == InputType.Button && a.keyButton == ability.keyButton && a.keyButton == ability.keyButton)).ToList();

            //Find index of current ability
            int currentIndex = potentialComboAbilities.IndexOf(ability);

            //If current index is not found (not a combo ability) or last in the list then return true
            if (currentIndex == -1 || currentIndex == potentialComboAbilities.Count - 1)
                return true;
            else
                return false; // else return false


        }

        /// <summary>
        /// Will determine if the ability provided is the last in the combo chain 
        /// </summary> 
        /// <param name="AbilityName">name of the ability to check if it's last in a combo chain </param>
        /// <returns>True if Ability is last in the combo, else false</returns>
        public bool IsAbilityLastInComboChain(string AbilityName) {

            //Find the ability we are dealing with
            ABC_Ability ability = this.FindAbility(AbilityName);

            return this.IsAbilityLastInComboChain(ability.abilityID);


        }


        /// <summary>
        /// Will return if the ability provided is enabled
        /// </summary>
        /// <param name="AbilityID">ID of the ability</param>
        /// <returns>True if enabled, else false</returns>
        public bool IsAbilityEnabled(int AbilityID) {

            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return false;

            return ability.isEnabled();
        }

        /// <summary>
        /// Will return if the ability provided is enabled
        /// </summary>
        /// <param name="AbilityName">Name of ability</param>
        /// <returns>True if enabled, else false</returns>
        public bool IsAbilityEnabled(string AbilityName) {

            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return false;

            return ability.isEnabled();
        }

        /// <summary>
        /// Will return if the ability provided is enabled and not on cooldown
        /// </summary>
        /// <param name="AbilityName">Name of ability</param>
        /// <returns>Yes if ability is enabled and ready, else false</returns>
        public bool IsAbilityPrimed(string AbilityName) {
            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return false;

            return ability.isPrimed();
        }

        /// <summary>
        /// Will return if the ability provided is enabled and not on cooldown
        /// </summary>
        /// <param name="AbilityID">ID of ability</param>
        /// <returns>Yes if ability is enabled and ready, else false</returns>
        public bool IsAbilityPrimed(int AbilityID) {
            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return false;

            return ability.isPrimed();
        }

        /// <summary>
        /// Will return a bool indicating if ability can be activated via key/button triggers
        /// </summary>
        /// <returns>True if ability can be activated by key/button triggers, else false</returns>
        /// <remarks>Currently triggers can only blocked due to AI mode being enabled and aiRestrictAbilityTriggers setting is true </remarks>
        public bool CanActivateAbilitiesFromTriggers() {

            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return false;
            else
                return true;

        }


        /// <summary>
        /// Will adjust the global ability miss chance by by the value provided  (can be positive or negative)
        /// </summary>
        /// <param name="Value">% Value to adjust the global ability miss chance by (can be positive or negative)</param>
        public void AdjustGlobalAbilityMissChance(float Value) {

            //Adjust the global miss chance
            this.globalAbilityMissChance += Value;

            //Clamp the value between 0 and 100
            Mathf.Clamp(this.globalAbilityMissChance, 0, 100);


        }


        /// <summary>
        /// Will return the entities global ability prepare time adjustment (can be positive or negative)
        /// </summary>
        /// <returns>Ability prepare time adjustment %</returns>
        public float GetAbilityGlobalPrepareTimeAdjustment() {

            //If not set then return the normal 100
            if (this.globalAbilityPrepareTimeAdjustment == 0)
                return 100;

            return this.globalAbilityPrepareTimeAdjustment;
        }

        /// <summary>
        /// Will adjust the ability global prepare time by the value provided
        /// </summary>
        /// <param name="Value">% Value to adjust prepare time by (can be positive or negative)</param>
        public void AdjustAbilityGlobalPrepareTime(float Value) {

            this.globalAbilityPrepareTimeAdjustment += Value;

            if (this.globalAbilityPrepareTimeAdjustment < 0)
                this.globalAbilityPrepareTimeAdjustment = 0.1f;

        }



        /// <summary>
        /// Will return the entities global ability initiation speed adjustment (can be positive or negative)
        /// </summary>
        /// <returns>Ability initiation speed adjustment %</returns>
        public float GetAbilityGlobalInitiationSpeedAdjustment() {

            //If not set then return the normal 100
            if (this.globalAbilityInitiationSpeedAdjustment == 0)
                return 100f;

            return this.globalAbilityInitiationSpeedAdjustment;
        }

        /// <summary>
        /// Will adjust the entities ability initiation speed by the value provided
        /// </summary>
        /// <param name="Value">% Value to adjust ability initiation speed by (can be positive or negative)</param>
        public void AdjustAbilityGlobalInitiationSpeedAdjustment(float Value) {

            this.globalAbilityInitiationSpeedAdjustment += Value;

            if (this.globalAbilityInitiationSpeedAdjustment < 0)
                this.globalAbilityInitiationSpeedAdjustment = 0.1f;

        }


        /// <summary>
        /// Will return the entities global ability cooldown adjustment (can be positive or negative)
        /// </summary>
        /// <returns>Ability cooldown adjustment %</returns>
        public float GetAbilityGlobalCooldownAdjustment() {

            //If not set then return the normal 100
            if (this.globalAbilityCoolDownAdjustment == 0)
                return 100;

            return this.globalAbilityCoolDownAdjustment;
        }

        /// <summary>
        /// Will adjust the ability global cooldown by the value provided
        /// </summary>
        /// <param name="Value">% Value to adjust global cooldown by (can be positive or negative)</param>
        public void AdjustAbilityGlobalCooldown(float Value) {

            this.globalAbilityCoolDownAdjustment += Value;

            if (this.globalAbilityCoolDownAdjustment < 0)
                this.globalAbilityCoolDownAdjustment = 0.1f;

        }

        /// <summary>
        /// Will return the entities global ability cooldown adjustment (can be positive or negative)
        /// </summary>
        /// <returns>Ability crit chance adjustment %</returns>
        public float GetAbilityGlobalCritChanceAdjustment() {            

            return this.globalAbilityCritChanceAdjustment;
        }

        /// <summary>
        /// Will adjust the ability global crit chance by the value provided
        /// </summary>
        /// <param name="Value">% Value to adjust global crit chance by (can be positive or negative)</param>
        public void AdjustAbilityGlobalCritChance(float Value) {

            this.globalAbilityCritChanceAdjustment += Value;

        }

        /// <summary>
        /// Will return the entities global ability crit potency adjustment (can be positive or negative)
        /// </summary>
        /// <returns>Ability crit potency adjustment %</returns>
        public float GetAbilityGlobalCritPotencyAdjustment() {

            return this.globalAbilityCritPotencyAdjustment;
        }

        /// <summary>
        /// Will adjust the ability global crit potency by the value provided
        /// </summary>
        /// <param name="Value">% Value to adjust global crit potency by (can be positive or negative)</param>
        public void AdjustAbilityGlobalCritPotency(float Value) {

            this.globalAbilityCritPotencyAdjustment += Value;

        }

        /// <summary>
        /// Will return a value which represents the remaining cooldown for the ability provided
        /// </summary>
        /// <param name="AbilityName">Name of ability</param>
        /// <param name="ReturnPercentage">If true then the remaining cooldown value returned will be a percentage</param>
        /// <returns>Remaining cooldown of the ability</returns>
        public float GetAbilityRemainingCooldown(string AbilityName, bool ReturnPercentage = false) {

            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return 0;

            return ability.GetRemainingCooldown(meEntity, ReturnPercentage);

        }

        /// <summary>
        /// Will return a value which represents the remaining cooldown for the ability provided
        /// </summary>
        /// <param name="AbilityID">ID of ability</param>
        /// <param name="ReturnPercentage">If true then the reamining cooldown value returned will be a percentage</param>
        /// <returns>Remaining cooldown of the ability</returns>
        public float GetAbilityRemainingCooldown(int AbilityID, bool ReturnPercentage = false) {

            //Find ability with the name
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return 0;

            return ability.GetRemainingCooldown(meEntity, ReturnPercentage);

        }

        /// <summary>
        /// Will enable a ability 
        /// </summary>
        /// <param name="AbilityID">ID of the ability to enable</param>
        public void EnableAbility(int AbilityID) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            //Enable ability if found
            StartCoroutine(ability.Enable(0f, meEntity));


        }


        /// <summary>
        /// Will enable a ability 
        /// </summary>
        /// <param name="AbilityName">Name of the ability to enable</param>
        public void EnableAbility(string AbilityName) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            //Enable ability if found
            StartCoroutine(ability.Enable(0f, meEntity));


        }

        /// <summary>
        /// Enables the Entities Ability for the duration provided
        /// </summary>
        /// <param name="AbilityID">ID of the ability to enable</param>
        /// <param name="Duration">The duration to enable the ability for</param>
        public IEnumerator EnableAbilityForDuration(int AbilityID, float Duration) {

            //Enable ability
            this.EnableAbility(AbilityID);

            //Wait for duration
            yield return new WaitForSeconds(Duration);

            //Disable ability
            this.DisableAbility(AbilityID);
        }


        /// <summary>
        /// Will disable a ability 
        /// </summary>
        /// <param name="AbilityName">Name of the ability to disable</param>
        public void DisableAbility(string AbilityName) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            //Enable ability if found
            ability.Disable(meEntity);


        }


        /// <summary>
        /// Will disable a ability 
        /// </summary>
        /// <param name="AbilityID">ID of the ability to disable</param>
        public void DisableAbility(int AbilityID) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            //Enable ability if found
            ability.Disable(meEntity);


        }



        /// <summary>
        /// Will swap the enable state of an ability, disabling if enabled, else enabling if disabled
        /// </summary>
        /// <param name="AbilityName">name of the ability to toggle</param>
        public void ToggleAbilityEnableState(string AbilityName) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            //If enabled then disable
            if (ability.isEnabled())
                ability.Disable(meEntity);
            else
                StartCoroutine(ability.Enable(0f, meEntity));

        }


        /// <summary>
        /// Will swap the enable state of an ability, disabling if enabled, else enabling if disabled
        /// </summary>
        /// <param name="AbilityID">ID of the ability to toggle</param>
        public void ToggleAbilityEnableState(int AbilityID) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            //If enabled then disable
            if (ability.isEnabled())
                ability.Disable(meEntity);
            else
                StartCoroutine(ability.Enable(0f, meEntity));

        }


        /// <summary>
        /// Will delete an ability  
        /// </summary>
        /// <param name="AbilityName">Name of the ability  to delete</param>
        public void DeleteAbility(string AbilityName) {

            //Find weapon with the ID
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            this.CurrentAbilities.Remove(ability);
        }

        /// <summary>
        /// Will delete a weapon 
        /// </summary>
        /// <param name="AbilityID">ID of the weapon to delete</param>
        public void DeleteAbility(int AbilityID) {
            //Find weapon with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            this.CurrentAbilities.Remove(ability);
        }



        /// <summary>
        /// Will find and return an weapon
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to find</param>
        /// <returns>Weapon found from the ID given</returns>
        public Weapon FindWeapon(int WeaponID) {
            Weapon retVal = null;

            retVal = this.CurrentWeapons.Where(w => w.weaponID == WeaponID).FirstOrDefault();

            return retVal;

        }

        /// <summary>
        /// Will find and return a Weapon
        /// </summary>
        /// <param name="WeaponName">Name of the Weapon to find</param>
        /// <returns>Weapon found from the Name given</returns>
        public Weapon FindWeapon(string WeaponName) {

            Weapon retVal = null;

            retVal = this.CurrentWeapons.Where(w => w.weaponName == WeaponName).FirstOrDefault();

            return retVal;

        }


        /// <summary>
        /// Returns a dictionary detailing information about the weapon requested including name, description etc
        /// </summary>
        /// <param name="WeaponID">ID of the weapon</param>
        /// <returns>Dictionary holding information regarding the weapon</returns>
        public Dictionary<string, string> GetWeaponDetails(int WeaponID) {
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon != null)
                return weapon.GetWeaponDetails(meEntity);

            return null;
        }

        /// <summary>
        /// Returns a dictionary detailing information about the weapon requested including name, description etc
        /// </summary>
        /// <param name="WeaponName">Name of weapon</param>
        /// <returns>Dictionary holding information regarding the weapon</returns>
        public Dictionary<string, string> GetWeaponDetails(string WeaponName) {
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon != null)
                return weapon.GetWeaponDetails(meEntity);

            return null;
        }

        /// <summary>
        /// Will return the number of enabled weapons on the entity
        /// </summary>
        /// <returns>Number of enabled weapons on the entity</returns>
        public int NoOfEnabledWeapons() {

            return this.CurrentWeapons.Where(w => w.IsEnabled() == true).ToList().Count();

        }

        /// <summary>
        /// Will return the texture Icon for the weapon passed in the method
        /// </summary>
        /// <param name="WeaponID">ID of the weapon</param>
        /// <returns>Texture2D Icon of the weapon passed</returns>
        public Texture2D GetWeaponIcon(int WeaponID) {

            Texture2D retval = null;
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon != null)
                retval = weapon.weaponIconImage.Texture2D;

            return retval;

        }

        /// <summary>
        /// Returns the key/button which will equip the weapon
        /// </summary>
        /// <param name="WeaponID">ID of the Weapon</param>
        /// <returns>String of the key or button which will equip the weapon</returns>
        public string GetWeaponEquipKey(int WeaponID) {

            Weapon weapon = this.FindWeapon(WeaponID);

            //if ability not found return empty string
            if (weapon == null)
                return "";


            //return the abilities key trigger
            return weapon.GetEquipKey();

        }


        /// <summary>
        /// Returns the key/button which will equip the weapon
        /// </summary>
        /// <param name="WeaponName">Name of the Weapon</param>
        /// <returns>String of the key or button which will equip the weapon</returns>
        public string GetWeaponEquipKey(string WeaponName) {

            Weapon weapon = this.FindWeapon(WeaponName);

            //if ability not found return empty string
            if (weapon == null)
                return "";


            //return the abilities key trigger
            return weapon.GetEquipKey();
        }


        /// <summary>
        /// Will activate the weapon trail for the duration provided
        /// </summary>
        /// <param name="Duration">How long to render the weapon trail for</param>
        /// <param name="Delay">Delay before weapon trail shows</param>
        /// <param name="TrailColours">List of colours to render on the trail</param>	
        /// <param name="ActivatedAbility">(Optional)The ability that activated the weapon trail</param>
        public void ActivateWeaponTrailOnCurrentEquippedWeapon(float Duration, float Delay, int GraphicIteration = 0, ABC_Ability ActivatedAbility = null) {

            //No weapon equipped so end here
            if (this.CurrentEquippedWeapon == null)
                return;

            this.CurrentEquippedWeapon.ActivateWeaponTrail(Duration, Delay, GraphicIteration, ActivatedAbility);

        }


        /// <summary>
        /// Will interrupt all weapon trails for the current equipped weapon
        /// </summary>
        public void InterruptWeaponTrailOnCurrentEquippedWeapon() {

            //No weapon equipped so end here
            if (this.CurrentEquippedWeapon == null)
                return;

            this.CurrentEquippedWeapon.InterruptAllWeaponTrails();


        }

        /// <summary>
        /// Adjusts current equipped weapons's ammo by the value provided. Will also update originators ammo GUI if provided.
        /// </summary>
        /// <remarks>
        /// If an originator is provided then the method will retrieve the entitys Ammo Text GUI and update it reflecting the weapons's current ammo. 
        /// </remarks>
        /// <param name="Value">Value to adjust ammo by (positive and negative possible)</param>
        /// <param name="AdjustAmmoOnly">(Optional) If true then only ammo will be modified, else it's up to logic to decide if ammo or current clip count is changed. </param>
        public void AdjustCurrentEquippedWeaponAmmo(int Value, bool AdjustAmmoOnly = false) {

            //No weapon equipped so no ammo to adjust
            if (this.CurrentEquippedWeapon == null)
                return;

            //Else adjust ammo for current equipped weapon 
            this.CurrentEquippedWeapon.AdjustWeaponAmmo(Value, this.meEntity, AdjustAmmoOnly);
        }


        /// <summary>
        /// Returns a bool indicating if the current equipped weapon has ammo
        /// </summary>
        /// <returns>True if equipped weapon has ammo, else false</returns>
        public bool CurrentEquippedWeaponHasAmmo() {

            //No weapon equipped so no ammo to use
            if (this.CurrentEquippedWeapon == null)
                return false;

            //Else return if current equipped weapon has ammo
            return this.CurrentEquippedWeapon.HasAmmo();

        }

        /// <summary>
        /// Will adjust the ammo of the weapon ID provided
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to adjust ammo</param>
        /// <param name="Value">Value to adjust ammo by</param>
        public void AdjustWeaponAmmo(int WeaponID, int Value) {

            //Find weapon with the ID
            ABC_Controller.Weapon weapon = this.FindWeapon(WeaponID);

            //adjust ammo if weapon is found
            if (weapon != null)
                weapon.AdjustWeaponAmmo(Value, meEntity, true);

        }

        /// <summary>
        /// Will set the weapons ammo to the value provided, recalculating weapon clip count
        /// </summary>
        /// <param name="Value">Value to change weapon ammo too</param>
        public void SetWeaponAmmo(int WeaponID, int Value) {

            //Find weapon with the ID
            ABC_Controller.Weapon weapon = this.FindWeapon(WeaponID);

            //adjust ammo if weapon is found
            if (weapon != null)
                weapon.SetWeaponAmmo(Value, meEntity);

        }

        /// <summary>
        /// Determines if the entity is currently reloading 
        /// </summary>
        /// <returns>true if entity is reloading, else false</returns>
        public bool IsReloading() {

            if (this.CurrentEquippedWeapon != null && this.CurrentEquippedWeapon.IsReloading())
                return true;

            if (this.CurrentScrollAbility != null && this.CurrentScrollAbility.IsReloading())
                return true;


            return false;

        }


        /// <summary>
        /// Determines if a reload is required on the current equipped weapon
        /// </summary>
        /// <returns>Returns true if a reload is required, else false</returns>
        public bool CurrentEquippedWeaponReloadRequired() {

            //No weapon equipped so no reloading can be happening
            if (this.CurrentEquippedWeapon == null)
                return false;

            //Else return if current equipped weapon requires reloading
            return this.CurrentEquippedWeapon.ReloadRequired();
        }


        /// <summary>
        /// Will enable a weapon 
        /// </summaryWeaponID
        /// <param name="WeaponID">ID of the weapon to enable</param>
        /// <param name="EquipWeapon">If true then the weapon enabled will be equipped</param>
        /// <param name="RunSetup">If true then setup will be called ensuring that a weapon and scroll ability is equipped</param>
        /// <returns>True if weapon was successfully enabled, else false</returns>
        public bool EnableWeapon(int WeaponID, bool EquipWeapon = false, bool RunSetup = true) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon == null)
                return false;

            //Enable weapon if found
            StartCoroutine(weapon.Enable(0f, meEntity, RunSetup, EquipWeapon));

            //weapon enabled successfully
            return true;

        }

        /// <summary>
        /// Will enable a weapon 
        /// </summaryWeaponID
        /// <param name="WeaponName">Name of the weapon to disable</param>
        /// <param name="EquipWeapon">If true then the weapon enabled will be equipped</param>
        /// <param name="RunSetup">If true then setup will be called ensuring that a weapon and scroll ability is equipped</param>
        /// <returns>True if weapon was successfully enabled, else false</returns>
        public bool EnableWeapon(string WeaponName, bool EquipWeapon = false, bool RunSetup = true) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon == null)
                return false;

            //Enable ability if found
            StartCoroutine(weapon.Enable(0f, meEntity, RunSetup, EquipWeapon));

            //weapon enabled successfully
            return true;
        }



        /// <summary>
        /// Will disable a weapon 
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to disable</param>
        /// <returns>True if weapon was successfully disabled, else false</returns>
        public bool DisableWeapon(int WeaponID) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon == null)
                return false;

            //Enable ability if found
            weapon.Disable(meEntity);


            //weapon disabled successfully
            return true;

        }

        /// <summary>
        /// Will disable a weapon 
        /// </summary>
        /// <param name="WeaponName">Name of the weapon to disable</param>
        /// <returns>True if weapon was successfully disabled, else false</returns>
        public bool DisableWeapon(string WeaponName) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon == null)
                return false;

            //Enable ability if found
            weapon.Disable(meEntity);


            //weapon disabled successfully
            return true;

        }


        /// <summary>
        /// Will swap the enable state of an weapon, disabling if enabled, else enabling if disabled
        /// </summary>
        /// <param name="WeaponName">name of the weapon to toggle</param>
        public void ToggleWeaponEnableState(string WeaponName) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon == null)
                return;

            //If enabled then disable
            if (weapon.IsEnabled())
                weapon.Disable(meEntity);
            else
                StartCoroutine(weapon.Enable(0f, meEntity));

        }

        /// <summary>
        /// Will swap the enable state of an weapon, disabling if enabled, else enabling if disabled
        /// </summary>
        /// <param name="WeaponID">name of the weapon to toggle</param>
        public void ToggleWeaponEnableState(int WeaponID) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon == null)
                return;

            //If enabled then disable
            if (weapon.IsEnabled())
                weapon.Disable(meEntity);
            else
                StartCoroutine(weapon.Enable(0f, meEntity));

        }




        /// <summary>
        /// Will delete a weapon 
        /// </summary>
        /// <param name="WeaponName">Name of the weapon to delete</param>
        public void DeleteWeapon(string WeaponName) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon == null)
                return;

            this.CurrentWeapons.Remove(weapon);
        }

        /// <summary>
        /// Will delete a weapon 
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to delete</param>
        public void DeleteWeapon(int WeaponID) {

            //Find weapon with the ID
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon == null)
                return;

            this.CurrentWeapons.Remove(weapon);
        }


        /// <summary>
        /// Will drop the weapon the entity currently has equipped, running animations, showing the drop graphic and disabling/deleting the graphic
        /// </summary>
        /// <param name="SkipNextWeaponEquip">If true then once current weapon is dropped the next weapon is not automatically equipped</param>
        public IEnumerator DropCurrentWeapon(bool SkipNextWeaponEquip = false) {


            //If weapon drop has been disabled then end here
            if (this.allowWeaponDrop == false)
                yield break;

            //If entity doesn't currently have an equipped weapon then end here
            if (this.CurrentEquippedWeapon == null)
                yield break;

            //If configured to restrict weapon if weapon count is less or equal to x and this condition is true then end here
            if (this.restrictDropWeaponCount == true && this.NoOfEnabledWeapons() <= this.restrictDropWeaponIfWeaponCountLessOrEqualTo)
                yield break;


            //If we are idle then come out of idle mode instantly to equip the weapon
            if (this.inIdleMode == true)
                StartCoroutine(this.SwitchIdleMode(true));


            //Drop weapon and record how long it will take to drop (to play out animations etc)
            float dropWaitTime = this.CurrentEquippedWeapon.DropWeapon(meEntity);

            //If drop wait time was higher then 0 then drop was successful
            if (dropWaitTime > 0) {


                //stop tracking current equipped weapon (resetup weapons?)
                this.CurrentEquippedWeapon = null;

                //wait for the current weapon to drop/animate
                yield return new WaitForSeconds(dropWaitTime);

                //If not skipping next weapon equip then equip next weapon
                if (SkipNextWeaponEquip == false)
                    StartCoroutine(this.EquipNextWeapon());
            }


        }


        /// <summary>
        /// Will return if the weapon provided is enabled
        /// </summary>
        /// <param name="WeaponID">ID of the weapon</param>
        /// <returns>True if enabled, else false</returns>
        public bool IsWeaponEnabled(int WeaponID) {

            //Find ability with the name
            Weapon weapon = this.FindWeapon(WeaponID);

            if (weapon == null)
                return false;

            return weapon.IsEnabled();
        }

        /// <summary>
        /// Will return if the weapon provided is enabled
        /// </summary>
        /// <param name="WeaponName">Name of weapon</param>
        /// <returns>True if enabled, else false</returns>
        public bool IsWeaponEnabled(string WeaponName) {


            //Find ability with the name
            Weapon weapon = this.FindWeapon(WeaponName);

            if (weapon == null)
                return false;

            return weapon.IsEnabled();
        }

        /// <summary>
        /// Will equip a weapon 
        /// </summary>
        /// <param name="WeaponID">ID of the weapon to equip</param>
        /// <param name="QuickToggle">True if the weapon should equip instantly, else false.</param>
        public IEnumerator EquipWeapon(int WeaponID, bool QuickToggle = false) {

            //Determine which weapon we are enabling and disabling first incase the method is called in quick succession  
            Weapon weaponToDisable = this.CurrentEquippedWeapon;
            Weapon weaponToEnable = this.FindWeapon(WeaponID);

            //If weapon was not found then end here
            if (weaponToEnable == null)
                yield break;

            //If weapon isn't enabled then end here
            if (weaponToEnable.IsEnabled() == false)
                yield break;

            //If weapon to equip is already equipped then end here
            if (this.CurrentEquippedWeapon == weaponToEnable)
                yield break;


            // disable old weapon and wait till its done if not in idle mode (weapon already disabled)
            if (this.inIdleMode == false)
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, QuickToggle));

            // equip new weapon
            yield return StartCoroutine(this.ToggleWeapon(weaponToEnable, WeaponState.Equip, QuickToggle));


        }

        /// <summary>
        /// Will equip a weapon
        /// </summary>
        /// <param name="WeaponName">Name of the weapon to equip</param>
        /// <param name="QuickToggle">True if the weapon should equip instantly, else false.</param>
        public IEnumerator EquipWeapon(string WeaponName, bool QuickToggle = false) {

            //Determine which weapon we are enabling and disabling first incase the method is called in quick succession  
            Weapon weaponToDisable = this.CurrentEquippedWeapon;
            Weapon weaponToEnable = this.FindWeapon(WeaponName);

            //If weapon was not found then end here
            if (weaponToEnable == null)
                yield break;


            //If weapon isn't enabled then end here
            if (weaponToEnable.IsEnabled() == false)
                yield break;


            //If weapon to equip is already equipped then end here
            if (this.CurrentEquippedWeapon == weaponToEnable)
                yield break;


            // disable old weapon and wait till its done if not in idle mode (weapon already disabled)
            if (this.inIdleMode == false)
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, QuickToggle));

            // equip new weapon
            yield return StartCoroutine(this.ToggleWeapon(weaponToEnable, WeaponState.Equip, QuickToggle));


        }

        /// <summary>
        /// Will equip a weapon
        /// </summary>
        /// <param name="WeaponObj">Obj of the weapon to equip</param>
        public IEnumerator EquipWeapon(Weapon Weapon) {

            //Determine which weapon we are enabling and disabling first incase the method is called in quick succession  
            Weapon weaponToDisable = this.CurrentEquippedWeapon;
            Weapon weaponToEnable = Weapon;

            //If weapon was not found then end here
            if (weaponToEnable == null)
                yield break;

            //If weapon isn't enabled then end here
            if (weaponToEnable.IsEnabled() == false)
                yield break;

            //If weapon to equip is already equipped then end here
            if (this.CurrentEquippedWeapon == weaponToEnable)
                yield break;


            // disable old weapon and wait till its done if not in idle mode (weapon already disabled)
            if (this.inIdleMode == false)
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip));

            // equip new weapon
            yield return StartCoroutine(this.ToggleWeapon(weaponToEnable, WeaponState.Equip));


        }

        /// <summary>
        /// Will unequip the weapon which is currently equipped
        /// </summary>
        /// <param name="QuickToggle">True if the weapon should unequip instantly, else false.</param>
        public IEnumerator UnEquipCurrentEquippedWeapon(bool QuickToggle = false) {

            //Determine which weapon we are enabling and disabling first incase the method is called in quick succession  
            Weapon weaponToDisable = this.CurrentEquippedWeapon;
            this.CurrentEquippedWeapon = null;

            //If weapon was not found then end here
            if (weaponToDisable == null)
                yield break;

            //If weapon isn't enabled then end here
            if (weaponToDisable.IsEnabled() == false)
                yield break;


            if (this.inIdleMode) {
                // unequip weapon with quick toggle as already in idle mode
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, true));
            } else {
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, QuickToggle));
            }


        }


        /// <summary>
        /// Will enable and equip the next enabled weapon in the list
        /// </summary>
        /// <param name="QuickToggle">True if this is a quick toggle which means weapon will equip/unequip instantly</param> 
        public IEnumerator EquipNextWeapon(bool QuickToggle = false) {

            // Double check more then one weapon enabled weapon exists in our list else end function here as there is nothing to go next too
            if (this.CurrentWeapons.Count < 2 || this.CurrentWeapons.Count(w => w.IsEnabled()) < 2)
                yield break;

            //Determine which weapon we are enabling and disabling first incase the next button is being hit very fast 
            Weapon weaponToEnable = null;
            Weapon weaponToDisable = this.CurrentEquippedWeapon;

            //default index incase this is the first time we are equipping a weapon (game start)
            var currentIndex = 0;

            // if we have a weapon already equipped then find the index 
            if (this.CurrentEquippedWeapon != null)
                currentIndex = this.CurrentWeapons.FindIndex(w => w == this.CurrentEquippedWeapon) + 1;

            //Record next enabled weapon in list
            for (int i = currentIndex; i <= CurrentWeapons.Count; i++) {

                //If we have reached end of list then go to the start
                if (i >= this.CurrentWeapons.Count())
                    i = 0;

                //If weapon is enabled and set to be equippable from being cycled through
                if (CurrentWeapons[i].IsEnabled() && CurrentWeapons[i].enableWeaponOnCycle == true) {
                    weaponToEnable = CurrentWeapons[i];
                    break;
                }


            }


            // disable old weapon and wait till its done if not in idle mode (weapon already disabled)
            if (this.inIdleMode == false)
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, QuickToggle));

            // equip new weapon
            yield return StartCoroutine(this.ToggleWeapon(weaponToEnable, WeaponState.Equip, QuickToggle));



        }

        /// <summary>
        /// Will enable and equip the next scroll ability in the list
        /// </summary>
        /// <param name="QuickToggle">True if this is a quick toggle which means weapon will equip/unequip instantly</param> 
        public IEnumerator EquipPreviousWeapon(bool QuickToggle = false) {


            // Double check more then one weapon exists in our list else end function here as there is nothing to go back too
            if (this.CurrentWeapons.Count < 2 || this.CurrentWeapons.Count(w => w.IsEnabled()) < 2)
                yield break;

            //Determine which weapon we are enabling and disabling first incase the next button is being hit very fast 
            Weapon weaponToEnable = null;
            Weapon weaponToDisable = this.CurrentEquippedWeapon;

            //default index incase this is the first time we are equipping a weapon (game start)
            var currentIndex = 0;

            // if we have a weapon already equipped then find the index 
            if (this.CurrentEquippedWeapon != null)
                currentIndex = this.CurrentWeapons.FindIndex(w => w == this.CurrentEquippedWeapon) - 1;

            //find the previous weapon in the list 
            for (int i = currentIndex; i >= -1; i--) {

                //If we have reached end of list then go to the start
                if (i < 0)
                    i = this.CurrentWeapons.Count() - 1;

                //If weapon is enabled and set to be equippable from being cycled through
                if (CurrentWeapons[i].IsEnabled() && CurrentWeapons[i].enableWeaponOnCycle == true) {
                    weaponToEnable = CurrentWeapons[i];
                    break;
                }


            }



            // disable old weapon and wait till its done if not in idle mode (weapon already disabled)
            if (this.inIdleMode == false)
                yield return StartCoroutine(this.ToggleWeapon(weaponToDisable, WeaponState.UnEquip, QuickToggle));

            // equip new weapon
            yield return StartCoroutine(this.ToggleWeapon(weaponToEnable, WeaponState.Equip, QuickToggle));



        }


        /// <summary>
        /// Will initiate weapon parry, setting the current weapon to parry
        /// </summary>
        public void ActivateWeaponParry() {

            if (this.CurrentEquippedWeapon == null || this.inIdleMode == true || this.AbilityActivationPermitted() == false || this.isCurrentlyWeaponBlocking == true || this.isCurrentlyWeaponParrying == true || this.weaponParryingDisabled == true || this.hitRestrictsAbilityActivation == true || this.activeMeleeAbilities.Count > 0 || this.weaponBeingToggled == true)
                return;


            //Activate weapon parry 
            StartCoroutine(this.CurrentEquippedWeapon.ActivateWeaponParry(meEntity));


        }


        /// <summary>
        /// Will prevent the entity from parrying for the duration provided
        /// </summary>
        /// <param name="Duration">Amount of time to prevent entity from being able to parry</param>
        public IEnumerator PreventWeaponParryForDuration(float Duration) {

            //Disable weapon blocking
            this.weaponParryingDisabled = true;

            //Wait for duration
            yield return new WaitForSeconds(Duration);

            //Enable weapon blocking
            this.weaponParryingDisabled = false;


        }


        /// <summary>
        /// Will activate the parry handler on the entities current equipped weapon (i.e handles the parrying of an ability)
        /// </summary>
        /// <param name="BlockedEntity">Entity that had an ability blocked</param>
        /// <param name="AbilityType">The type of ability that was blocked</param>
        /// <param name="AbilityHitPoint">The vector position where the ability collided</param>
        /// <param name="IgnoreWeaponParry">(Optional)If True then the weapon parry will be ignored</param>
        /// <returns>True if ability was successfully parried, else false</returns>
        public bool ActivateCurrentEquippedWeaponParryHandler(ABC_IEntity BlockedEntity, AbilityType AbilityType, Vector3 AbilityHitPoint = default(Vector3), bool IgnoreWeaponParry = false) {

            if (this.CurrentEquippedWeapon == null || this.isCurrentlyWeaponParrying == false || this.weaponParryingDisabled == true)
                return false;

            //Call block handler returning duration ready to refresh weapon blocking after the handler is finished
            bool abilityParried = this.CurrentEquippedWeapon.ActivateParryHandler(meEntity, BlockedEntity, AbilityType, AbilityHitPoint, IgnoreWeaponParry);


            //If ability parried then add to logs
            if (abilityParried == true) {

                //Add logs
                this.AddToDiagnosticLog(this.name + " parried " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");

                if (this.LogInformationAbout(LoggingType.ParryInformation))
                    this.AddToAbilityLog(this.name + " parried " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");
            }



            //Return if ability was parried successfully or not
            return abilityParried;

        }




        /// <summary>
        /// Will activate auto weapon block, which will make the entity automatically toggle blocking for the duration provided. 
        /// </summary>
        /// <param name="Duration">The duration for the weapon block, if 0 is provided then the entity will block for an infinite duration</param>
        /// <param name="Cooldown">Once the duration is over the cooldown will stop entity blocking again for the time provided</param>
        public IEnumerator ActivateAutoWeaponBlock(float Duration, float Cooldown) {

            //Turn auto weapon block on 
            this.autoWeaponBlock = true;

            //If duration is greater then 0 then wait for duration before turning off auto weapon block
            if (Duration > 0f) {

                //wait duration 
                yield return new WaitForSeconds(Duration);


                //If cooldown is greater then 0 then prevent weapon blocking for the cooldown
                if (Cooldown > 0f)
                    StartCoroutine(this.PreventWeaponBlockingForDuration(Cooldown));

                //Turn auto weapon block
                this.autoWeaponBlock = false;

            }


        }


        /// <summary>
        /// Will return if entity is currently weapon blocking
        /// </summary>
        /// <returns>True if weapon blocking, else false</returns>
        public bool IsWeaponBlocking() {

            return this.isCurrentlyWeaponBlocking;

        }

        /// <summary>
        /// Will initiate weapon blocking, setting the current weapon to block
        /// </summary>
        public void StartWeaponBlocking() {

            if (this.CurrentEquippedWeapon == null || this.inIdleMode == true || this.AbilityActivationPermitted() == false || this.isCurrentlyWeaponBlocking == true || this.isCurrentlyWeaponParrying == true || this.weaponBlockingDisabled == true || this.hitRestrictsAbilityActivation == true || this.activeMeleeAbilities.Count > 0 || this.weaponBeingToggled == true)
                return;


            //Toggle weapon block on
            this.CurrentEquippedWeapon.ToggleWeaponBlock(meEntity, WeaponState.Block);


        }

        /// <summary>
        /// Will stop weapon blocking, stopping the current weapon from blocking 
        /// </summary>
        public void StopWeaponBlocking() {

            if (this.CurrentEquippedWeapon == null || this.inIdleMode == true || this.isCurrentlyWeaponBlocking == false)
                return;


            //Toggle weapon block off
            this.CurrentEquippedWeapon.ToggleWeaponBlock(meEntity, WeaponState.UnBlock);

        }


        /// <summary>
        /// Will prevent the entity from blocking for the duration provided
        /// </summary>
        /// <param name="Duration">Amount of time to prevent entity from being able to start blocking</param>
        public IEnumerator PreventWeaponBlockingForDuration(float Duration) {

            //Disable weapon blocking
            this.weaponBlockingDisabled = true;

            //Wait for duration
            yield return new WaitForSeconds(Duration);

            //Enable weapon blocking
            this.weaponBlockingDisabled = false;


        }

        /// <summary>
        /// Will refresh the weapon blocking ensuring that the correct animation state is in play (used after block reacting where other animations may play)
        /// </summary>
        /// <param name="Delay">Delay before weapon blocking is refresh</param>
        /// <returns></returns>
        public IEnumerator RefreshWeaponBlocking(float Delay = 0f) {

            //Wait for delay
            if (Delay > 0)
                yield return new WaitForSeconds(Delay);

            //Toggle weapon blocking on/off depending on current stay passing in refresh parameter as true (so only animations are changed)
            if (this.CurrentEquippedWeapon != null)
                this.CurrentEquippedWeapon.ToggleWeaponBlock(meEntity, this.isCurrentlyWeaponBlocking ? WeaponState.Block : WeaponState.UnBlock, true);


        }



        /// <summary>
        /// Will activate the block handler on the entities current equipped weapon (i.e handles the blocking of an ability)
        /// </summary>
        /// <param name="BlockedEntity">Entity that had an ability blocked</param>
        /// <param name="AbilityType">The type of ability that was blocked</param>
        /// <param name="AbilityHitPoint">The vector position where the ability collided</param>
        /// <param name="InterruptWeaponBlock">(Optional)If True then the weapon blocking will be cancelled stopping the entity from blocking</param>
        /// <param name="ReduceWeaponBlockDurability">(Optional)If True then the block durability will be decreased due to an ability hit</param>
        /// <returns>True if ability was successfully blocked, else false</returns>
        public bool ActivateCurrentEquippedWeaponBlockHandler(ABC_IEntity BlockedEntity, AbilityType AbilityType, Vector3 AbilityHitPoint = default(Vector3), bool InterruptWeaponBlock = false, bool ReduceWeaponBlockDurability = true) {

            if (this.CurrentEquippedWeapon == null || this.isCurrentlyWeaponBlocking == false)
                return false;

            //Call block handler returning duration ready to refresh weapon blocking after the handler is finished
            float blockHandlerDuration = this.CurrentEquippedWeapon.ActivateBlockHandler(meEntity, BlockedEntity, AbilityType, AbilityHitPoint, InterruptWeaponBlock, ReduceWeaponBlockDurability);



            //If duration comes back -1 then return false as block was not successful
            if (blockHandlerDuration == -1f) {

                //Add logs
                this.AddToDiagnosticLog(this.name + " blocking breaked due to " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");

                if (this.LogInformationAbout(LoggingType.BlockingInformation))
                    this.AddToAbilityLog(this.name + " blocking breaked due to " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");

                //Return false as block was not successful 
                return false;
            }


            //refresh weapon blocking to make sure entity goes back into block animation position taking into account the block handler duration
            StartCoroutine(RefreshWeaponBlocking(blockHandlerDuration));

            //Add logs
            this.AddToDiagnosticLog(this.name + " blocked " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");

            if (this.LogInformationAbout(LoggingType.BlockingInformation))
                this.AddToAbilityLog(this.name + " blocked " + BlockedEntity.gameObject.name + " " + AbilityType.ToString() + " ability");

            //Return true as block was successful 
            return true;

        }


        /// <summary>
        /// Will activate the entities weapon melee attack blocked reaction (i.e sword knockback animation when weapon is blocked)
        /// </summary>
        public void ActivateCurrentEquippedWeaponMeleeAttackBlockedReaction() {


            if (this.CurrentEquippedWeapon == null)
                return;

            this.CurrentEquippedWeapon.ActivateMeleeAttackReflectedReaction(meEntity);


        }



        /// <summary>
        /// Will return a list of all ABC Animation runner components attached to the weapon that is currently equipped
        /// </summary>
        /// <returns>List of ABC Animation Runner Components</returns>
        public List<ABC_AnimationsRunner> GetCurrentEquippedWeaponAnimationRunners() {

            //If we have a weapon currently equipped then return the animation runners attached to the weapon graphics
            if (this.CurrentEquippedWeapon != null)
                return this.CurrentEquippedWeapon.GetWeaponAnimationRunners();


            return null;

        }

        /// <summary>
        /// Will return a list of all Animator components relating to the weapons object graphics
        /// </summary>
        /// <returns>List of ABC Animation Runner Components</returns>
        public List<Animator> GetCurrentEquippedWeaponAnimators() {

            //If we have a weapon currently equipped then return the animators attached to the weapon graphics
            if (this.CurrentEquippedWeapon != null)
                return this.CurrentEquippedWeapon.GetWeaponAnimators();


            return null;

        }


        /// <summary>
        /// Will return the number of active abilities set up to be a ScrollAbility. 
        /// </summary>
        /// <returns></returns>
        public int ScrollAbilityCount() {

            int retval = 0;

            foreach (ABC_Ability ability in this.CurrentAbilities) {

                if (ability.IsAnEnabledScrollAbility())
                    retval += 1;

            }

            return retval;

        }

        /// <summary>
        /// Will equip a scroll ability 
        /// </summary>
        /// <param name="AbilityID">ID of the scroll ability to equip</param>
        public void EquipScrollAbility(int AbilityID) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            if (ability == null)
                return;

            //equip the ability if its a scroll and enabled
            if (ability.scrollAbility == true && ability.abilityEnabled)
                StartCoroutine(this.JumpToScrollAbility(AbilityID));


        }

        /// <summary>
        /// Will equip a scroll ability 
        /// </summary>
        /// <param name="AbilityName">Name of the scroll ability to equip</param>
        public void EquipScrollAbility(string AbilityName) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityName);

            if (ability == null)
                return;

            //equip the ability if its a scroll and enabled
            if (ability.scrollAbility == true && ability.abilityEnabled)
                StartCoroutine(this.JumpToScrollAbility(ability.abilityID));


        }

        /// <summary>
        /// Will equip the next scroll ability
        /// </summary>
        /// <param name="ActivateAesthetic">True if the disable animation and graphic should activate, else false.</param> 
        public void EquipNextScrollAbility(bool ActivateAesthetic = true) {

            StartCoroutine(this.NextScrollAbility(ActivateAesthetic));

        }

        /// <summary>
        /// Will equip the previous scroll ability
        /// </summary>
        /// <param name="ActivateAesthetic">True if the disable animation and graphic should activate, else false.</param> 
        public void EquipPreviousScrollAbility(bool ActivateAesthetic = true) {

            StartCoroutine(this.PreviousScrollAbility(ActivateAesthetic));

        }


        /// <summary>
        /// Will disable the current scroll ability
        /// </summary>
        /// <param name="MinNumberOfActiveConstraint">If true then the scroll ability will only be disabled if a minimum number of scroll abilities are currently active i.e only disable if 2 scroll abilities are currently enabled</param>
        /// <param name="MinNumberOfActiveToDisable">Number of scroll abilities that need to be enabled to disable the currently scroll ability</param>
        public void DisableCurrentScrollAbility(bool MinNumberOfEnabledConstraint = false, int MinNumberOfEnabledToDisable = 2) {

            //If the constraint is true and we don't currently have a number enabled scroll abilities greater or equal to the minimum number needed to disable then end here
            if (MinNumberOfEnabledConstraint == true && this.ScrollAbilityCount() < MinNumberOfEnabledToDisable)
                return;

            //Disable current scroll
            if (this.CurrentScrollAbility != null)
                this.DisableAbility(this.CurrentScrollAbility.abilityID);



        }


        /// <summary>
        /// Will 'swap' the current scroll ability with another scroll ability defined by ID provided. The current scroll ability will be disabled and the new scroll ability will beenabled
        /// </summary>
        /// <param name="NewScrollAbilityID">ID of the new scroll ability to enable and swap with the current scroll ability which will be disabled</param>
        /// <param name="MinNumberOfActiveConstraint">If true then the current scroll ability will only be disabled if a minimum number of scroll abilities are currently active i.e only disable if 2 scroll abilities are currently enabled</param>
        /// <param name="MinNumberOfActiveToDisable">Number of scroll abilities that need to be enabled to disable the currently scroll ability</param>
        public void SwapCurrentScrollAbility(int NewScrollAbilityID, bool MinNumberOfEnabledConstraint = false, int MinNumberOfEnabledToDisable = 2) {


            //If the new scroll ability is already active then end here as it's already swapped in
            if (this.FindAbility(NewScrollAbilityID).IsAnEnabledScrollAbility() == true)
                return;

            //tracks the ability which can be disabled
            int abilityIDToDisable = -1;

            //If the constraint is true and we currently have a number of enabled scroll abilities greater or equal to the minimum number needed to disable, then record the current scroll ability ID to disable later
            if (MinNumberOfEnabledConstraint == true && this.ScrollAbilityCount() >= MinNumberOfEnabledToDisable && this.CurrentScrollAbility != null)
                abilityIDToDisable = this.CurrentScrollAbility.abilityID;


            //Enable new ability
            this.EnableAbility(NewScrollAbilityID);

            //equip new scroll ability
            this.EquipScrollAbility(NewScrollAbilityID);

            //Disable ability if an ID has been provided (not -1), after the new ability has been equipped
            if (abilityIDToDisable != -1)
                this.DisableAbility(abilityIDToDisable);


        }

        /// <summary>
        /// Will raise the Scroll ability set and unset delegate event informing subscribers that a scroll ability has been initialised ('equipped') or deinitialised ('unequipped')
        /// </summary>
        /// <param name="AbilityID">ID of the scroll ability initialised ('equipped') or deinitialised ('unequipped')</param>
        /// <param name="AbilityName">Name of the scroll ability initialised ('equipped') or deinitialised ('unequipped')</param>
        /// <param name="Set">True if the scroll ability was initialised ('equipped') else, false if the scroll ability was deinitialised ('uneqipped')</param>
        public void RaiseScrollAbilitySetAndUnsetEvent(int ScrollAbilityID, string ScrollAbilityName, bool Set) {

            if (this.onScrollAbilitySetAndUnset != null)
                this.onScrollAbilitySetAndUnset(ScrollAbilityID, ScrollAbilityName, Set);

        }


        /// <summary>
        /// Method will setup the weapons and scroll abilities for the entity
        /// </summary>
        /// <param name="EquipWeaponID">(Optional) if entered then the weapon matching the ID will be equipped after the weapons have been setup, if -1 is entered then the first weapon configured will be equipped</param>
        /// <param name="QuickToggle">True if the weapon should equip instantly, else false.</param>
        public IEnumerator SetupWeaponsAndScrollAbilities(int EquipWeaponID = -1, bool QuickToggle = true) {

            //Setup weapons
            yield return StartCoroutine(this.SetupWeapons(EquipWeaponID, QuickToggle));

            //Setup Scroll abilities
            yield return SetupScrollAbilities();

        }


        /// <summary>
        /// Will adjust the ammo of the ability ID provided
        /// </summary>
        /// <param name="AbilityID">ID of the ability to adjust ammo</param>
        /// <param name="Value">Value to adjust ammo by</param>
        public void AdjustAbilityAmmo(int AbilityID, int Value) {

            //Find ability with the ID
            ABC_Ability ability = this.FindAbility(AbilityID);

            //Enable ability if found
            if (ability != null)
                ability.AdjustAmmo(Value, meEntity, true);

        }

        /// <summary>
        /// Will find and return an ability Grou[
        /// </summary>
        /// <param name="AbilityGroupID">ID of the ability group to find</param>
        /// <returns>Ability Group found from the ID given</returns>
        public AbilityGroup FindAbilityGroup(int AbilityGroupID) {

            return this.AbilityGroups.Where(a => a.groupID == AbilityGroupID).FirstOrDefault();

        }

        /// <summary>
        /// Will find and return an ability Group
        /// </summary>
        /// <param name="AbilityGroupName">name of the ability group to find</param>
        /// <returns>Ability Group found from the name given</returns>
        public AbilityGroup FindAbilityGroup(string AbilityGroupName) {

            return this.AbilityGroups.Where(a => a.groupName == AbilityGroupName).FirstOrDefault();

        }

        /// <summary>
        /// Will activate/disable all the ability groups GUI for the entity 
        /// </summary>
        /// <param name="Enabled">If true will enable the GUI, else disable it</param>
        public void ShowAbilityGroupsGUI(bool Enabled = true) {

            foreach (AbilityGroup group in AbilityGroups) {
                group.ShowAbilityGroupGUI(Enabled);

            }

        }

        /// <summary>
        /// Will adjust the current enable points by the value provided for the ability group defined by the ID passed in the parameters
        /// </summary>
        /// <param name="GroupID">ID of the ability group </param>
        /// <param name="AdjustmentValue">Amount to adjust the enable points by</param>
        public void AdjustAbilityGroupsEnablePoints(int GroupID, float AdjustmentValue) {

            //Get group 
            AbilityGroup group = this.AbilityGroups.Where(item => item.groupID == GroupID).FirstOrDefault();

            //If group exists then enable all abilities
            if (group != null)
                group.AdjustEnableGroupPoints(AdjustmentValue);

        }

        /// <summary>
        /// Will enable/disable the ability group linked to the ID provided
        /// </summary>
        /// <param name="GroupID">ID of the ability group </param>
        /// <param name="Enabled">True to enable the ability group, else false to disable</param>
        public void ToggleAbilityGroup(int GroupID, bool Enabled = true) {

            //Get group 
            AbilityGroup group = this.AbilityGroups.Where(item => item.groupID == GroupID).FirstOrDefault();

            //If group exists then enable/disable depending on parameter provided
            if (group != null)
                group.ToggleOnOff(meEntity, Enabled);

        }

        /// <summary>
        /// Will enable/disable the ability group which name matches the string provided
        /// </summary>
        /// <param name="GroupName">Name of the ability group </param>
        /// <param name="Enabled">True to enable the ability group, else false to disable</param>
        public virtual void ToggleAbilityGroup(string GroupName, bool Enabled = true) {

            //Get group 
            AbilityGroup group = this.AbilityGroups.Where(item => item.groupName == GroupName).FirstOrDefault();

            //If group exists then enable/disable depending on parameter provided
            if (group != null)
                group.ToggleOnOff(meEntity, Enabled);

        }


        /// <summary>
        /// Enables all the abilities in the ability group defined by the ID passed in the parameters
        /// </summary>
        /// <param name="GroupID">ID of the ability group </param>
        public void EnableAbilityGroup(int GroupID) {
            //Get group 
            AbilityGroup group = this.AbilityGroups.Where(item => item.groupID == GroupID).FirstOrDefault();

            //If group exists then enable all abilities
            if (group != null)
                StartCoroutine(group.EnableAllAbilitiesAndWeapons(meEntity));

        }

        /// <summary>
        /// Disables all the abilities in the ability group defined by the ID passed in the parameters
        /// </summary>
        /// <param name="GroupID">ID of the ability group </param>
        public void DisableAbilityGroup(int GroupID) {
            //Get group 
            AbilityGroup group = this.AbilityGroups.Where(item => item.groupID == GroupID).FirstOrDefault();

            //If group exists then enable all abilities
            if (group != null)
                StartCoroutine(group.DisableAllAbilitiesAndWeapons(meEntity));

        }

        /// <summary>
        /// Will delete an ability group  
        /// </summary>
        /// <param name="GroupName">Name of the Group to delete</param>
        public void DeleteAbilityGroup(string GroupName) {

            //Find weapon with the ID
            AbilityGroup group = this.FindAbilityGroup(GroupName);

            if (group == null)
                return;

            this.AbilityGroups.Remove(group);
        }

        /// <summary>
        /// Will delete an ability group  
        /// </summary>
        /// <param name="GroupID">ID of the Group to delete</param>
        public void DeleteAbilityGroup(int GroupID) {

            //Find weapon with the ID
            AbilityGroup group = this.FindAbilityGroup(GroupID);

            if (group == null)
                return;

            this.AbilityGroups.Remove(group);
        }



        /// <summary>
        /// Will go through all the  abilities enabling any which have been set to become enabled due to the ability determined in parameters performing an event determined in parameters. 
        /// </summary>
        /// <remarks>
        /// Will link abilities to each other. When 1 ability does a specific action like activation of collision then any abilities linked will become enabled. 
        /// This can be used to  make combos i.e Enable attack 2 once attack 1 activates
        /// </remarks>
        /// <param name="AbilityID">Ability that was activated</param>
        /// <param name="AbilityEvent">Event this ability is performing (activation/collision)</param>
        public void EnableAbilitiesAfterEvent(int AbilityID, AbilityEvent AbilityEvent) {

            List<ABC_Ability> abilitiesToEnable = null;


            switch (AbilityEvent) {
                case AbilityEvent.Activation:
                    abilitiesToEnable = this.CurrentAbilities.Where(item => item.enableAfterEvent == true && item.enableAfterAbilityIDsActivated.Contains(AbilityID)).ToList();
                    break;
                case AbilityEvent.Collision:
                    abilitiesToEnable = this.CurrentAbilities.Where(item => item.enableAfterEvent == true && item.enableAfterAbilityIDsCollide.Contains(AbilityID)).ToList();
                    break;
            }



            foreach (ABC_Ability ability in abilitiesToEnable) {
                StartCoroutine(ability.Enable(0f, meEntity));


                this.AddToDiagnosticLog(ability.name + " enabled due to after event ability: " + AbilityEvent.ToString());

            }



        }

        /// <summary>
        /// Will add the ability to a list which tracks all active melee abilities, used when the activation is over but the attack is still going and a reference is needed when interrupting etc (unlike projectiles melee stop instantly)
        /// </summary>
        /// <param name="Ability">Active melee Ability to track</param>
        /// <param name="AbilityObject">Ability Object ("Projectile") created in game which shows the graphic and deals with collisions</param>
        public void AddToActiveMeleeAbilities(ABC_Ability Ability, GameObject AbilityObject) {

            //If ability is already being tracked then add any new objects provided
            if (this.activeMeleeAbilities.ContainsKey(Ability)) {
                this.activeMeleeAbilities[Ability].Add(AbilityObject);
            } else {

                //else add new entry to dictionary with ability and object 
                List<GameObject> newAbilityObjList = new List<GameObject>();
                newAbilityObjList.Add(AbilityObject);

                this.activeMeleeAbilities.Add(Ability, newAbilityObjList);
            }

        }

        /// <summary>
        /// Will remove the ability from a list which tracks all active melee abilities
        /// </summary>
        /// <param name="Ability">Active melee Ability to remove from the tracked list</param>
        /// <param name="AbilityObjDisabledCountToRemove">(Optional) will only remove the melee ability if x number of ability objects being tracked are disabled (i.e melee ability will only be removed once all additional starting positions objects have activated)</param>
        public void RemoveFromActiveMeleeAbilities(ABC_Ability Ability, int AbilityObjDisabledCountToRemove = 0) {

            //If ability not tracked then end here
            if (this.activeMeleeAbilities.ContainsKey(Ability) == false)
                return;

            //If we require a certain ability object disable count to remove the melee ability and the current  obj disabled count is less then the number needed to remove then end here
            //Used for melee additional starting positions, don't want to remove melee ability from being tracked if it hasn't finished and instantiating all the 'additional starting positions etc' 
            if (Ability.AbilityActivationInterrupted() == false && AbilityObjDisabledCountToRemove > 0 && this.activeMeleeAbilities[Ability].Where(obj => obj.activeInHierarchy == false).Count() < AbilityObjDisabledCountToRemove)
                return;

            //If this far then remove ability
            this.activeMeleeAbilities.Remove(Ability);

            //stop weapon block for a second to prevent switching back so quickly 
            StartCoroutine(this.PreventWeaponBlockingForDuration(0.7f));


        }

        /// <summary>
        /// Will add the ability to a list which tracks all abilities that the entity is currently activating
        /// </summary>
        /// <param name="Ability">Ability to add to the activating list</param>
        public void AddToActivatingAbilities(ABC_Ability Ability) {

            this.activatingAbilities.Add(Ability);

        }

        /// <summary>
        /// Will remove the ability from a list which tracks all abilities that the entity is currently activating
        /// </summary>
        /// <param name="Ability">Ability to remove from the activating list</param>
        public void RemoveFromActivatingAbilities(ABC_Ability Ability) {

            if (this.activatingAbilities.Contains(Ability))
                this.activatingAbilities.Remove(Ability);

        }


        /// <summary>
        /// Will determine if the entity is in the process of activating the ability provided in the parameter
        /// </summary>
        /// <returns>True if activating the ability, else false</returns>
        public bool IsActivatingAbility(ABC_Ability Ability) {

            //If the list is not empty then an ability is currently being activated
            if (this.activatingAbilities.Where(a => a == Ability).ToList().Count > 0 || this.activeMeleeAbilities.Where(a => a.Key == Ability).ToList().Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Will return a list of current activating abilities
        /// </summary>
        /// <returns>list of abilities entity is currently activating</returns>
        public List<ABC_Ability> GetActivatingAbilities() {

            List<ABC_Ability> retVal = this.activatingAbilities;

            foreach (ABC_Ability ability in this.activeMeleeAbilities.Keys) {

                if (retVal.Contains(ability) == false)
                    retVal.Add(ability);
            }

            return retVal;

        }

        /// <summary>
        /// Returns a float which represents the time of last ability activation
        /// </summary>
        /// <returns>float of last time ability was activated</returns>
        public float TimeOfLastAbilityActivation() {

            return this.timeOfLastAbilityActivation;

        }

        /// <summary>
        /// Will determine if the entity is in the process of activating any abilities
        /// </summary>
        /// <returns>True if any ability is being activated, else false</returns>
        public bool IsActivatingAbility() {

            //clean up any melee abilities being tracked but not correctly removed
            if (this.activeMeleeAbilities.Count > 0) {

                for (int i = 0; i < this.activeMeleeAbilities.Count(); i++) {

                    //If all melee projectiles are disabled then we can remove from tracker
                    if (this.activeMeleeAbilities.ElementAt(i).Value.All(m => m.activeInHierarchy == false))
                        this.RemoveFromActiveMeleeAbilities(this.activeMeleeAbilities.ElementAt(i).Key);

                }
            }



            //If the list is not empty then an ability is currently being activated
            if (this.activatingAbilities.Count > 0 || this.activeMeleeAbilities.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets a value indicating whether the entity can currently activate abilities
        /// </summary>
        /// <value><c>true</c> if ability activation permitted; otherwise, <c>false</c>.</value>
        public bool AbilityActivationPermitted() {

            // if entity is not currently activating an ability AND not blocked by ability activation interval AND entity can activate abilities AND a toggledAbility is not restricting activation 
            // AND entity is inCombat OR is not in combat but will switch to combat on ability input 
            if (this.IsActivatingAbility() == false && AbilitiesBlockedByActivationInterval() == false && canActivateAbilities && toggledAbilityRestrictsAbilityActivation == false && (inIdleMode == false || inIdleMode && deactiveIdleModeOnAbilityInput)) {
                return true;
            } else {
                return false;
            }

        }


        /// <summary>
        /// Will disable ability activation for the entity
        /// </summary>
        public void DisableAbilityActivation() {
            canActivateAbilities = false;
        }

        /// <summary>
        /// Will enable ability activation for the entity
        /// </summary>
        public void EnableAbilityActivation() {

            canActivateAbilities = true;
        }

        /// <summary>
        /// Will temporarily adjust the ability activation interval. This is reset once the interval wait is over.
        /// </summary>
        /// <param name="IntervalAdjustment">Amount to adjust ability activation interval by</param>
        public void TemporarilyAdjustAbilityActivationInterval(float IntervalAdjustment) {

            this.tempAbilityActivationIntervalAdjustment = IntervalAdjustment;

        }

        /// <summary>
        /// Will temporarily disable ability activation for the entity
        /// </summary>
        /// <param name="Duration">Time until ability activation is enabled again</param>
        public void TemporarilyDisableAbilityActivation(float Duration = 1f) {

            if (canActivateAbilities == true) {
                this.DisableAbilityActivation();
                //turn on canactivateabilities 
                Invoke("EnableAbilityActivation", Duration);
            }



        }

        /// <summary>
        /// Returns a bool indicating if any abilities are currently interrupted
        /// </summary>
        /// <returns>true if any ability is currently interrupted, else false</returns>
        public bool AbilityActivationCurrentlyInterrupted() {

            //Check if any melee abilities are currently interrupted
            foreach (ABC_Ability meleeAbility in activeMeleeAbilities.Keys) {

                if (meleeAbility.AbilityActivationInterrupted())
                    return true;

            }


            //Check if any projectile abilities are currently interrupted
            foreach (ABC_Ability ability in activatingAbilities) {

                if (ability.AbilityActivationInterrupted())
                    return true;
            }


            //No abilities are currently interrupted
            return false;


        }


        /// <summary>
        /// Will interrupt the abilities the entity is currently activating. 
        /// </summary>
        /// <param name="HitInterrupted">(Optional) If true then ability activation was interrupted by a hit</param>
        public void InterruptAbilityActivation(bool HitInterrupted = false) {

            //ability not successfully activated so clear list of recorded keys(to stop quick interruption combos)
            this.recordedKeyInputHistory.Clear();

            //Interrupt any active melee attacks in progress (activation is over but the attack is still going, different to projectiles as the melee attack will stop instantly when active)
            foreach (ABC_Ability meleeAbility in activeMeleeAbilities.Keys)
                meleeAbility.InterruptAbilityActivation(meEntity, HitInterrupted);


            //Interrupt all abilities entity is currently activating
            foreach (ABC_Ability ability in activatingAbilities)
                ability.InterruptAbilityActivation(meEntity, HitInterrupted);


        }




        /// <summary>
        /// Will interrupt any abilities the entity is currently activating due to a recent hit (Hit interrupt casting needs to be setup on the entity). 
        /// </summary>
        public void ActivateHitInterruptsAbilityActivation() {
            // if hits can interrupt casting then interrupt any activations
            if (this.hitsInterruptCasting == true)
                this.InterruptAbilityActivation(true);


        }

        /// <summary>
        /// Will restrict ability activation due to a recent hit by a duration defined in settings if allowed (hit prevents casting needs to be setup on the entity). 
        /// </summary>
        public void ActivateHitRestrictsAbilityActivation() {

            // if setting is not turned on to prevent casting on hit return here
            if (this.hitsPreventCasting == false)
                return;

            // turn on flag
            this.hitRestrictsAbilityActivation = true;


            this.AddToDiagnosticLog("Hit has prevented casting for a duration " + gameObject.name + " will not be able to attack (unless special conditions)");

            // cancel any previous disable as we are not done with preventing hitting and might want to chain the affect
            CancelInvoke("DisableHitRestrictsAbilityActivation");

            // restart the countdown
            Invoke("DisableHitRestrictsAbilityActivation", hitsPreventCastingDuration);



        }

        /// <summary>
        /// Will restrict ability activation due to a recent hit by a duration defined in settings if allowed (hit prevents casting needs to be setup on the entity). 
        /// </summary>
        /// <param name="Duration">Duration to apply hit restriction for</param>
        public void ActivateHitRestrictsAbilityActivation(float Duration) {

            // if setting is not turned on to prevent casting on hit return here
            if (this.hitsPreventCasting == false)
                return;

            // turn on flag
            this.hitRestrictsAbilityActivation = true;


            this.AddToDiagnosticLog("Hit has prevented casting for a duration " + gameObject.name + " will not be able to attack (unless special conditions)");

            // cancel any previous disable as we are not done with preventing hitting and might want to chain the affect
            CancelInvoke("DisableHitRestrictsAbilityActivation");

            float disableTime = this.hitsPreventCastingDuration;

            //Only update disable time if it's bigger then the hit prevent casting duration
            if (Duration > disableTime)
                disableTime = Duration;

            // restart the countdown
            Invoke("DisableHitRestrictsAbilityActivation", disableTime);


        }

        /// <summary>
        /// Returns a bool indicating if the user has pressed/held the key to activate the current scroll ability. 
        /// </summary>
        /// <param name="OnPress">If true will look for button press, else if false will look for a button hold</param>
        /// <returns>True if scroll ability activation button has been pressed, else false</returns>
        public bool CurrentScrollAbilityButtonPressed(bool OnPress = true) {


            // check if the correct activate current ability input has been pressed
            if (OnPress == true && this.ButtonPressed(ControllerButtonPressState.ActivateCurrentScrollAbility))
                return true;

            if (OnPress == false && this.ButtonPressed(ControllerButtonPressState.ActivateCurrentScrollAbility, ControllerButtonPressType.Hold))
                return true;


            // correct input was not dectected so we can return false 
            return false;


        }


        /// <summary>
        /// Determines whether ABC cancel has been triggered by the player. Cancel will stop casting/toggle abilities/waiting for target etc.
        /// </summary>
        /// <returns><c>true</c> if cancel has been triggered; otherwise, <c>false</c>.</returns>
        public bool CancelTriggered() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return false;

            // if the input cancel 
            if (inputCancelEnabled == false)
                return false;

            // if the correct input cancel has been pressed then return true
            if (this.ButtonPressed(ControllerButtonPressState.Cancel)) {
                return true;
            } else {
                return false;
            }

        }

        /// <summary>
        /// Will return a position offset which will be added on to the ability target position to make the ability miss. A dice roll is performed first to determine
        /// if the ability will miss or not
        /// </summary>
        /// <returns>Vector3 offset to be applied to ability target position to make it miss or not (not if dice roll determines ability won't miss)</returns>
        public Vector3 GetMissChancePositionOffset() {

            Vector3 retVal = new Vector3(0, 0, 0);

            //If miss chance is 0 or the diceroll did not return true (the number rolled is outside the miss chance) then return 0, 0, 0 retval so ability doesn't miss
            if (this.globalAbilityMissChance == 0 || ABC_Utilities.DiceRoll(1, this.globalAbilityMissChance) == false)
                return retVal;

            //else return the miss position offset ranging from the values set
            retVal = new Vector3(UnityEngine.Random.Range(this.abilityMissMinOffset.x, this.abilityMissMaxOffset.x),
                UnityEngine.Random.Range(this.abilityMissMinOffset.y, abilityMissMaxOffset.y),
                UnityEngine.Random.Range(this.abilityMissMinOffset.z, abilityMissMaxOffset.z));

            return retVal;

        }

        /// <summary>
        /// Will return the entities current target
        /// </summary>
        /// <returns>Target gameobject</returns>
        public GameObject GetCurrentTarget() {

            //If the entity currently has a target then return the object
            if (this.targetObject != null)
                return this.targetObject;

            //Entity doesn't currently have a target
            return null;

        }

        /// <summary>
        /// Will return the entities current Soft Target
        /// </summary>
        /// <returns>Soft Target gameobject</returns>
        public GameObject GetCurrentSoftTarget() {

            //If the entity currently has a Soft Target then return the object
            if (this.softTarget != null)
                return this.softTarget;

            //Entity doesn't currently have a Soft Target
            return null;

        }

        /// <summary>
        /// Will set the gameobject provided as the controllers target
        /// </summary>
        /// <param name="Target">target gameobject</param>
        public void SetNewTarget(GameObject Target) {

            //If active then set target 
            if (Target.activeInHierarchy == true)
                this.SetTarget(Target, false, true);

        }


        /// <summary>
        /// Will force the entity to select a new target by setting the waitingontarget variables to true
        /// </summary>
        /// <param name="Type">What type of target the entity is to select by (Target, Mouse, World) </param>
        /// <param name="SoftTargetOverride">(Optional) If true new target will become a soft target, else a target</param>
        public void SelectNewTarget(TargetType Type = TargetType.Target, bool SoftTargetOverride = false) {

            // when selecting new target do we apply softtarget first for confirmation of target?
            this.softTargetOverride = SoftTargetOverride;

            switch (Type) {
                case TargetType.Target:
                    waitingOnNewTarget = true;
                    break;
                case TargetType.Mouse:
                    waitingOnNewMouseTarget = true;
                    break;
                case TargetType.World:
                    waitingOnNewWorldTarget = true;

                    this.meEntity.inputManager.ResetMousePosition();

                    break;
            }


        }



        /// <summary>
        /// Returns true if a new target has been selected by the entity. Will only work correctly if the waiting on target variables have been set to true via the SelectNewtarget function or another means. 
        /// </summary>
        /// <param name="Type">What type of target the entity is to select by (Target, Mouse, World) </param>
        /// <returns>True if a new target has been selected, else false</returns>
        public bool NewTargetSelected(TargetType Type = TargetType.Target) {

            switch (Type) {
                case TargetType.Target:
                    if (waitingOnNewTarget == false)
                        return true;
                    break;
                case TargetType.Mouse:
                    if (waitingOnNewMouseTarget == false)
                        return true;
                    break;
                case TargetType.World:
                    if (waitingOnNewWorldTarget == false)
                        return true;
                    break;
            }

            // still waiting on targets so new target has been selected yet 

            return false;


        }

        /// <summary>
        /// Will deselect and removes the entities current target. 
        /// </summary>
        /// <param name="BypassRestrictions">(Optional) If true then the target will be removed without restriction, else other factors will be taken into consideration and the target might not be removed</param>
        public void RemoveTarget(bool BypassRestrictions = false) {
            //If we are currently activating an ability or we are unable to disable target selection then stop method here. Unless we are bypassing restrictions
            if ((this.disableDeselect == true || this.IsActivatingAbility() == true) && BypassRestrictions == false)
                return;

            this.ShowTargetIndicator(false);

            //remove us from targets targeter tracking
            if (this.targetObject != null) {
                ABC_IEntity prevTargetEntity = ABC_Utilities.GetStaticABCEntity(this.targetObject);
                prevTargetEntity.RemoveObjectFromTargeterTracker(meEntity.gameObject);
            }

            this.targetObject = null;

            //Inform event no target has been selected 
            if (this.targetSelectRaiseEvent)
                this.meEntity.RaiseTargetSetEvent(null);


        }

        /// <summary>
        /// Will deselect and removes the entities current soft target. 
        /// </summary>
        /// <param name="BypassRestrictions">(Optional) If true then the soft target will be removed without restriction, else other factors will be taken into consideration and the soft target might not be removed</param>
        public void RemoveSoftTarget(bool BypassRestrictions = false) {
            //If we are unable to disable target selection then return and stop method here. Unless override is given.
            if ((disableDeselect == true || this.IsActivatingAbility() == true) && BypassRestrictions == false)
                return;

            this.ShowSoftTargetIndicator(false);
            this.softTarget = null;

            //Inform event no soft target has been selected 
            if (this.softTargetSetRaiseEvent)
                this.meEntity.RaiseSoftTargetSetEvent(null);


        }

        /// <summary>
        /// Will deselect and removes the entities current world target. 
        /// </summary>
        /// <param name="BypassRestrictions">(Optional) If true then the world target will be removed without restriction, else other factors will be taken into consideration and the world target might not be removed</param>
        public void RemoveWorldTarget(bool BypassRestrictions = false) {

            this.worldTargetObject = null;
            this.worldTargetPosition = Vector3.zero;

        }

        /// <summary>
        /// Will raise the target set delegate event. 
        /// </summary>
        /// <param name="Target">target gameobject</param>
        public void RaiseTargetSetEvent(GameObject Target) {

            if (this.onTargetSet != null)
                this.onTargetSet(Target);

        }

        /// <summary>
        /// Will raise the soft target set delegate event. 
        /// </summary>
        /// <param name="SoftTarget">target gameobject</param>
        public void RaiseSoftTargetSetEvent(GameObject SoftTarget) {

            if (this.onSoftTargetSet != null)
                this.onSoftTargetSet(SoftTarget);

        }


        /// <summary>
        ///  Determines if the crosshair override is currently active
        /// </summary>
        /// <param name="ReturnTrueIfNotSetup">If true then this method will return true if crosshair override system is not setup on the entity (AI etc)</param>
        /// <returns>True if crosshairoverride is active, else false</returns>
        public bool IsCrossHairOverrideActive(bool ReturnTrueIfNotSetup = false) {

            //If the crosshairoverride is showing or it's not configured and the parameter states to return true if not configured
            if (this.showCrossHairOverride == true || ReturnTrueIfNotSetup == true && (this.crosshairEnabled == false || this.showCrossHairOnKey == false))
                return true;
            else
                return false;


        }

        /// <summary>
        /// Will toggle the crosshair override block 
        /// </summary>
        /// <param name="Blocked">True to block crosshairoverride, else false</param>
        public void ToggleCrossHairOverrideBlock(bool Blocked) {

            //toggle crosshair block
            this.blockCrosshairOverride = Blocked; 

        }


        /// <summary>
        /// Will restore Mana to max capacity 
        /// </summary>
        public void RestoreMana() {

            this.currentMana = this.currentMaxMana;

        }

        /// <summary>
        /// Will adjust max mana
        /// </summary>
        /// <param name="Amount">Amount to adjust max mana  by</param>
        /// <param name="RestoreFullHealth">If true then mana will be restored to full</param>
        public void AdjustMaxMana(float Amount, bool RestoreFullMana = false) {

            this.currentMaxMana += Amount;

            if (RestoreFullMana)
                this.RestoreMana();

        }

        /// <summary>
        /// Will adjust the current mana by the amount given can be positive or negative
        /// </summary>
        /// <param name="Amount">Amount to adjust mana by</param>
        public void AdjustMana(float Amount) {
            this.currentMana += Amount;

            // if we go under 0 then set it to 0 
            if (this.currentMana < 0)
                this.currentMana = 0;

        }


        /// <summary>
        /// Will adjust mana regen rate
        /// </summary>
        /// <param name="Amount">Amount to adjust regen by</param>
        public void AdjustManaRegen(float Amount) {
            this.manaRegenPerSecond += Amount;

            if (this.manaRegenPerSecond < 0)
                this.manaRegenPerSecond = 0;

        }

        /// <summary>
        /// Will restore Block Durability to max capacity 
        /// </summary>
        public void RestoreBlockDurability() {

            this.currentBlockDurability = this.currentMaxBlockDurability;

        }

        /// <summary>
        /// Will adjust max Block Durability
        /// </summary>
        /// <param name="Amount">Amount to adjust max block durability by</param>
        /// <param name="RestoreFullBlockDurability">If true then block durability will be restored to full</param>
        public void AdjustMaxBlockDurability(float Amount, bool RestoreFullBlockDurability = false) {

            this.currentMaxBlockDurability += Amount;

            if (RestoreFullBlockDurability)
                this.RestoreBlockDurability();

        }

        /// <summary>
        /// Will adjust the current Block Durability by the amount given can be positive or negative
        /// </summary>
        /// <param name="Amount">Amount to adjust Block Durability by</param>
        public void AdjustBlockDurability(float Amount) {
            this.currentBlockDurability += Amount;

            // if we go under 0 then set it to 0 
            if (this.currentBlockDurability < 0)
                this.currentBlockDurability = 0;

        }


        /// <summary>
        /// Will adjust block duration regen rate
        /// </summary>
        /// <param name="Amount">Amount to adjust regen by</param>
        public void AdjustBlockDurabilityRegen(float Amount) {
            this.blockDurabilityRegenPerSecond += Amount;

            if (this.blockDurabilityRegenPerSecond < 0)
                this.blockDurabilityRegenPerSecond = 0;

        }




        #endregion


        // ********************* Animation Events ********************
        #region Animation Events

        /// <summary>
        /// Animation event which will trigger a footstep 
        /// </summary>
        public void ABCFootStep() {
            //Here to record events from enemy animations but no code ran
        }

        #endregion


        // ****************** Private Methods ***************************

        #region  Private Methods

        /// <summary>
        /// Will setup the component ready to be used, called OnEnable()
        /// </summary>
        /// <param name="GameReload">If true then the game is being reloaded from a save and won't do some initial setup (restore mana to full for etc)</param>
        /// <param name="WeaponEquipID">(Optional) if a ID is provided then that weapon will be equipped on initialise</param>
        private IEnumerator InitialiseComponent(bool GameReload = false, int WeaponEquipID = -1) {


            //If not loading game then clear current abilities to regenerate incase any global items have been changed 
            if (GameReload == true) {
                this.ReloadGlobalElements(); // reget linked elements but keep enable state + ammo etc
            } else {
                //reset current equipped weapon 
                this.CurrentEquippedWeapon = null;

                //Clear current abilities to regenerate incase any global items have been changed 
                this.CurrentAbilities.Clear();
                this.CurrentWeapons.Clear();
            }


            //recreate pools
            this.CreatePools(); 

            //Record what potential keys need to be tracked to determine if abilities set to trigger from a combination of keys (F, F, B) can activate
            this.GenerateListOfAbilityInputComboKeys();

            // make sure we are set to activate abilities
            this.EnableAbilityActivation();

            //Restore Mana? (if game isn't being loaded)
            if (this.fullManaOnEnable == true && GameReload == false)
                this.RestoreMana();

            //Restore Block Durability? (if game isn't being loaded)
            if (this.fullBlockDurabilityOnEnable == true && GameReload == false)
                this.RestoreBlockDurability();

            // turn off Mana GUI
            this.ShowManaGUI(false);

            //setup UI Icons
            this.SetupUIIcons();

            // turn off ability group GUI 
            this.ShowAbilityGroupsGUI(false);

            // setup ability groups set to be enabled/disabled at start 
            // unless game is reloading then don't do initial setup and just activate groups already enabled from the load
            yield return StartCoroutine(this.SetupAbilityGroups(GameReload ? false : true));

            //reset any weapon toggle
            this.weaponBeingToggled = false;

            //Setup weapons and scroll abilities
            yield return StartCoroutine(this.SetupWeaponsAndScrollAbilities(WeaponEquipID));

            //Reset blocking 
            this.weaponBlockingDisabled = false;


            //Setup any ability trigger links
            this.SetupAbilityTriggerLinks();

            // start to regen mana 
            InvokeRepeating("RegenMana", 1f, 1f);

            // start to regen blockdurability
            InvokeRepeating("RegenBlockDurability", 1f, 1f);

            // user can select to have the target select handler run less then update for performance
            if ((this.targetSelectType != TargetSelectType.None && this.targetSelectInterval > 0f))
                InvokeRepeating("TargetSelectHandler", 1f, this.targetSelectInterval);


            // user can select to have the handler run less then update for performance
            if (this.autoTargetSelect == true && this.autoTargetInterval > 0f)
                InvokeRepeating("AutoTargetHandler", 2f, this.autoTargetInterval);

            // Randomly swap ability positions
            if (this.allowAbilitiesToRandomlySwapPositions == true)
                InvokeRepeating("RandomlySwapAbilityPositions", 1f, this.abilityRandomPositionSwapInterval);

            //Make sure AI Nav isn't blocked 
            this.BlockAINavigation(false);

            //If minimum intermission value is 0 then change to 0.2 so it will still run
            if (this.minimumAICheckIntermission == 0)
                this.minimumAICheckIntermission = 0.2f;

            //auto cast handler is called under Invoke repeating as the handler is process heavy and doesn't need to be called the amount of times update runs
            InvokeRepeating("AutoCastHandler", 0f, Mathf.Min(0.2f, this.minimumAICheckIntermission));

            //Invoke our AI Navigation settings (starting after random delay so not all entities move at same time always)
            InvokeRepeating("AINavigationWanderHandler", Random.Range(0f, 2f), 1f);
            InvokeRepeating("AINavigationHandler", Random.Range(0f, 3f), 0.3f);
            InvokeRepeating("AINavigationNewTargetInterval", this.aiNavNewDestinationInterval, this.aiNavNewDestinationInterval);

            //reset navspeed adjustments
            this.navSpeedAdjustment = 0;
            this.navWeaponSpeedAdjustment = 0;

            //reset crosshair block
            this.blockCrosshairOverride = false; 

        }


        /// <summary>
        /// Method will setup the weapons for the entity, equipping the first enabled one in the list. 
        /// Called at start of game
        /// </summary>
        /// <param name="EquipWeaponID">(Optional) if entered then the weapon matching the ID will be equipped after the weapons have been setup, if -1 is entered then the first weapon configured will be equipped</param>
        /// <param name="QuickToggle">True if the weapon should equip instantly, else false.</param>
        private IEnumerator SetupWeapons(int EquipWeaponID = -1, bool QuickToggle = true) {


            //If no weapons have been configured then end here
            if (CurrentWeapons.Count() == 0)
                yield break;


            // disable all weapons instantly unless weapon is already equipped 
            foreach (Weapon weapon in CurrentWeapons.ToList().Where(w => w.IsEnabled() && w != this.CurrentEquippedWeapon)) {
                yield return StartCoroutine(this.ToggleWeapon(weapon, WeaponState.UnEquip, true));
            }



            //If current weapon has not been set yet then find a weapon to equip 
            if (this.CurrentEquippedWeapon == null || this.CurrentEquippedWeapon.IsEnabled() == false || EquipWeaponID > -1) {

                //Reset the current ready to get the weapon to equip
                this.CurrentEquippedWeapon = null;

                //If a weapon has been provided in parameters then find and equip that
                if (EquipWeaponID > -1) {
                    this.CurrentEquippedWeapon = this.FindWeapon(EquipWeaponID);

                    //If the weapon we wanted to equip is not enabled then reset current equipped weapon 
                    if (this.CurrentEquippedWeapon.IsEnabled() == false)
                        this.CurrentEquippedWeapon = null;
                }


                //If no weapon has been found or an ID has not been provided then find first weapon in list
                if (this.CurrentEquippedWeapon == null)
                    this.CurrentEquippedWeapon = CurrentWeapons.Where(w => w.IsEnabled()).FirstOrDefault();
            }


            //If we started in idle mode then deactivate new current weapon instantly 
            if (inIdleMode == true) {
                StartCoroutine(this.ToggleWeapon(CurrentEquippedWeapon, WeaponState.UnEquip, QuickToggle));
            } else {
                // else enable the weapon instantly 
                StartCoroutine(this.ToggleWeapon(CurrentEquippedWeapon, WeaponState.Equip, QuickToggle));
            }

        }


        /// <summary>
        /// Useful for when entity is enabled will equip and setup the first scroll ability in the list 
        /// </summary>
        private IEnumerator SetupScrollAbilities() {

            //If no scroll abilities setup then end here
            if (CurrentAbilities.Where(a => a.IsAnEnabledScrollAbility()).Count() == 0)
                yield break;


            // disable all current scroll ability
            foreach (ABC_Ability ability in CurrentAbilities.Where(a => a.IsAScrollAbility())) {
                yield return StartCoroutine(this.DisableScrollAbility(ability, false));
            }


            //If current scroll ability has not been set yet then find first scroll ability in the list
            if (this.CurrentScrollAbility == null || this.CurrentScrollAbility.IsAnEnabledScrollAbility() == false)
                this.CurrentScrollAbility = CurrentAbilities.Where(a => a.IsAnEnabledScrollAbility()).FirstOrDefault();


            //If we started in idle mode then deactivate current scroll ability instantly
            if (inIdleMode == true)
                StartCoroutine(this.DisableScrollAbility(this.CurrentScrollAbility, true));
            else // else enable the scroll ability
                StartCoroutine(this.EnableScrollAbility(this.CurrentScrollAbility, this.playScrollAnimationOnEnable));

        }


        /// <summary>
        /// Determines if ability activiation is blocked by the interval configured
        /// </summary>
        /// <returns>True if ability activation is permitted and not blocked, else false</returns>
        private bool AbilitiesBlockedByActivationInterval() {

            //If time since last activation is greater then the interval + temp interval then return false as activation is no longer blocked 
            if (this.timeOfLastAbilityActivation == 0 || Time.time - this.timeOfLastAbilityActivation >= this.abilityActivationInterval + this.tempAbilityActivationIntervalAdjustment) {
                //reset the temp interval adjustment as this interval wait is over
                this.tempAbilityActivationIntervalAdjustment = 0f;
                return false;
            }


            //Time is less then the interval wait so return true to indicate activation block
            return true;

        }


        /// <summary>
        /// Main function for checking if a button has been pressed for different ABC events. Depending on the state given the method will return true or false if the setup button has been pressed. 
        /// </summary>
        /// <param name="State">Depending on the state the method will return if a button setup for that state has been pressed. States include: TargetClick, Cancel, IdleToggle, NextTarget, PreviousTarget, ConfirmSoftTarget, AutoTarget, ActivateCurrentScrollAbility, 
        /// NextScrollAbility, PreviousScrollAbility, ShowCrossHair, Reload and more</param>
        /// <param name="PressType">The press type to check for (Press/Hold)</param>
        /// <returns>True if the correct button is being pressed/held, else false</returns>
        private bool ButtonPressed(ControllerButtonPressState State, ControllerButtonPressType PressType = ControllerButtonPressType.Press) {

            InputType inputType = InputType.Button;
            ControllerButtonPressType pressType = PressType;
            KeyCode key = KeyCode.None;
            string button = "";

            //If set then a scroll wheel up or down is checked when seeing if correct button pressed
            bool scrollWheelUpIncluded = false;
            bool scrollWheelDownIncluded = false;

            // determine the right configuration depending on the type provided
            switch (State) {
                case ControllerButtonPressState.TargetClick:

                    inputType = this.clickForTargetInputType;
                    key = this.clickForTargetKey;
                    button = this.clickForTargetButton;

                    break;
                case ControllerButtonPressState.Cancel:

                    inputType = this.inputCancelInputType;
                    key = this.inputCancelKey;
                    button = this.inputCancelButton;

                    break;
                case ControllerButtonPressState.IdleToggle:

                    inputType = this.idleToggleInputType;
                    key = this.idleToggleKey;
                    button = this.idleToggleButton;

                    break;
                case ControllerButtonPressState.NextTarget:

                    inputType = this.tabTargetNextInputType;
                    key = this.tabTargetNextKey;
                    button = this.tabTargetNextButton;

                    break;
                case ControllerButtonPressState.PreviousTarget:

                    inputType = this.tabTargetPrevInputType;
                    key = this.tabTargetPrevKey;
                    button = this.tabTargetPrevButton;

                    break;
                case ControllerButtonPressState.ConfirmSoftTarget:

                    inputType = this.softTargetConfirmInputType;
                    key = this.softTargetConfirmKey;
                    button = this.softTargetConfirmButton;

                    break;
                case ControllerButtonPressState.AutoTarget:

                    inputType = this.softTargetConfirmInputType;

                    key = this.autoTargetKey;
                    button = this.autoTargetButton;

                    break;
                case ControllerButtonPressState.ActivateCurrentScrollAbility:

                    inputType = this.activateCurrentScrollAbilityInputType;
                    key = this.activateCurrentScrollAbilityKey;
                    button = this.activateCurrentScrollAbilityButton;

                    break;
                case ControllerButtonPressState.NextScrollAbility:

                    inputType = this.nextScrollInputType;
                    key = this.nextScrollKey;
                    button = this.nextScrollButton;

                    break;
                case ControllerButtonPressState.PreviousScrollAbility:

                    inputType = this.previousScrollInputType;
                    key = this.previousScrollKey;
                    button = this.previousScrollButton;

                    break;
                case ControllerButtonPressState.ShowCrossHair:

                    inputType = this.showCrossHairInputType;
                    key = this.showCrossHairKey;
                    button = this.showCrossHairButton;


                    break;
                case ControllerButtonPressState.ScrollAbilityReload:

                    inputType = this.reloadScrollAbilityInputType;
                    key = this.reloadScrollAbilityKey;
                    button = this.reloadScrollAbilityButton;


                    break;
                case ControllerButtonPressState.WeaponReload:

                    inputType = this.reloadWeaponInputType;
                    key = this.reloadWeaponKey;
                    button = this.reloadWeaponButton;


                    break;
                case ControllerButtonPressState.NextWeapon:

                    inputType = this.nextWeaponInputType;
                    key = this.nextWeaponKey;
                    button = this.nextWeaponButton;

                    if (this.cycleWeaponsUsingMouseScroll == true)
                        scrollWheelUpIncluded = true;


                    break;
                case ControllerButtonPressState.PreviousWeapon:

                    inputType = this.previousWeaponInputType;
                    key = this.previousWeaponKey;
                    button = this.previousWeaponButton;


                    if (this.cycleWeaponsUsingMouseScroll == true)
                        scrollWheelDownIncluded = true;

                    break;
                case ControllerButtonPressState.DropCurrentWeapon:

                    inputType = this.weaponDropInputType;
                    key = this.weaponDropKey;
                    button = this.weaponDropButton;

                    break;
                case ControllerButtonPressState.WeaponBlock:

                    inputType = this.weaponBlockInputType;
                    key = this.weaponBlockKey;
                    button = this.weaponBlockButton;

                    break;
                case ControllerButtonPressState.WeaponParry:

                    inputType = this.weaponParryInputType;
                    key = this.weaponParryKey;
                    button = this.weaponParryButton;

                    break;
            }



            // check if correct button is being pressed/held 
            if (pressType == ControllerButtonPressType.Press) {

                // If input type is key and the configured key is being pressed return true
                if (inputType == InputType.Key && this.meEntity.inputManager.GetKeyDown(key))
                    return true;


                // if input type is button and the configured button is being pressed return true
                if (inputType == InputType.Button && this.meEntity.inputManager.GetButtonDown(button))
                    return true;


                //If scrollwheel up included and being 'pressed' then return true 
                if (scrollWheelUpIncluded == true && this.meEntity.inputManager.GetYAxis("Mouse ScrollWheel") > 0)
                    return true;

                //If scrollwheel down included and being 'pressed' then return true 
                if (scrollWheelDownIncluded == true && this.meEntity.inputManager.GetYAxis("Mouse ScrollWheel") < 0)
                    return true;

            } else if (pressType == ControllerButtonPressType.Hold) {

                // If input type is key and the configured key is being held down return true
                if (inputType == InputType.Key && this.meEntity.inputManager.GetKey(key))
                    return true;


                // if input type is button and the configured button is being held down return true
                if (inputType == InputType.Button && this.meEntity.inputManager.GetButton(button))
                    return true;

                //If scrollwheel up included and being 'pressed' then return true 
                if (scrollWheelUpIncluded == true && this.meEntity.inputManager.GetYAxis("Mouse ScrollWheel") > 0)
                    return true;

                //If scrollwheel down included and being 'pressed' then return true 
                if (scrollWheelDownIncluded == true && this.meEntity.inputManager.GetYAxis("Mouse ScrollWheel") < 0)
                    return true;

            }




            // correct button is not currently being pressed so return false
            return false;


        }


        /// <summary>
        /// Main function to regen mana by the amount defined in the settings
        /// </summary>
        private void RegenMana() {
            if (this.currentMaxMana > 0) {
                // if we have mana then regen it every second (when this is called)
                if (this.manaRegenPerSecond > 0 && this.currentMana < this.currentMaxMana)
                    this.currentMana += this.manaRegenPerSecond;


                // if we accidently go over the limit then set it back to the max
                if (this.currentMana > this.currentMaxMana)
                    this.currentMana = this.currentMaxMana;

                // if we go under 0 then set it to 0 
                if (this.currentMana < 0)
                    this.currentMana = 0;

            } else {
                // revert mana to 0 as it's not used
                this.currentMana = 0;
            }

        }

        /// <summary>
        /// Updates the entities Mana GUI with the current mana values
        /// </summary>
        private void UpdateManaGUI() {

            // If GUI has not been setup then finish here
            if (this.ManaGUIList.Count == 0)
                return;


            foreach (ManaGUI gui in this.ManaGUIList) {

                gui.UpdateGUI(this.currentMana, this.currentMaxMana);

            }



        }

        /// <summary>
        /// Will randomly swap abilities (set to randomly swap) within the same trigger group.
        /// </summary>
        private void RandomlySwapAbilityPositions() {

            List<ABC_Ability> swappableAbilityList = null;

            for (int i = 0; i < CurrentAbilities.Count(); i++) {

                //retrieve ability 
                ABC_Ability ability = CurrentAbilities[i];

                //If ability isn't enabled, is a scroll ability or not set to randomly swap then continue to next
                if (ability.abilityEnabled == false || ability.randomlySwapAbilityPosition == false || ability.scrollAbility == true)
                    continue;

                //Get temp list of other randomly swapped abilities with same trigger group (key/button) that are enabled, not a scroll ability and not the ability we are swapping
                swappableAbilityList = CurrentAbilities.Where(a => a != ability && a.randomlySwapAbilityPosition == true && a.abilityEnabled == true && a.scrollAbility == false && (a.keyInputType == InputType.Key && a.key == ability.key || a.keyInputType == InputType.Button && a.keyButton == ability.keyButton)).ToList();

                //If nothing to swap with then continue to next ability 
                if (swappableAbilityList.Count < 2)
                    continue;

                ABC_Ability abilityToSwap = swappableAbilityList[Random.Range(0, swappableAbilityList.Count)];

                int originalIndex = CurrentAbilities.FindIndex(a => a == ability);
                int swapIndex = CurrentAbilities.FindIndex(a => a == abilityToSwap);

                CurrentAbilities[swapIndex] = ability;
                CurrentAbilities[originalIndex] = abilityToSwap;

            }

        }

        /// <summary>
        /// function will activate all ability groups set to be already enabled when the entity is initalised
        /// </summary>
        /// <param name="InitialSetup">If true then game start code will be run</param>
        private IEnumerator SetupAbilityGroups(bool InitialSetup = true) {

            //If no groups have been configured then end here
            if (this.AbilityGroups.Count() == 0)
                yield break;

            //Enable all groups that are already enabled
            foreach (AbilityGroup group in AbilityGroups) {

                if (InitialSetup) {
                    //enable those set to be enabled on start
                    if (group.enableGroupOnStart == true)
                        StartCoroutine(group.EnableAllAbilitiesAndWeapons(meEntity));

                    //disable those set to be disabled on start
                    if (group.disableGroupOnStart == true)
                        StartCoroutine(group.DisableAllAbilitiesAndWeapons(meEntity));
                } else {
                    //else if we are setting up from loading the game then reset the group and enable from scratch
                    if (group.groupEnabled == true) {
                        yield return StartCoroutine(group.DisableAllAbilitiesAndWeapons(meEntity));
                        yield return StartCoroutine(group.EnableAllAbilitiesAndWeapons(meEntity));
                    }

                }


                //Start the adjust group method 
                StartCoroutine(group.AdjustGroupPointsOverTime(meEntity));

            }


        }


        /// <summary>
        /// Will generate a list of the keys which have the potential to trigger Input Combo Abilities. I.e abilities which require a combination of key inputs to trigger (F, F, B)
        /// </summary>
        /// <remarks>Only the keys generated in this list are tracked, so ABC knows when a correct combination has been pressed to activate a input combo ability. This way 
        /// after keys used for movement etc do not interrupt the recording</remarks>
        private void GenerateListOfAbilityInputComboKeys() {

            //Clear the list to start fresh 
            this.abilityInputComboKeys.Clear();

            //Cycle through all abilities
            foreach (ABC_Ability ability in this.CurrentAbilities) {

                List<KeyCode> abilityKeyInputCombo = ability.GetKeyInputCombo();

                //If this ability key input combo is the longest combination of keys we have encountered so far then update the longest combo variable
                if (abilityKeyInputCombo.Count > this.abilityLongestInputCombo)
                    this.abilityLongestInputCombo = abilityKeyInputCombo.Count();

                //Cycle through the keys which can be combined to trigger the ability (if ability is set to trigger from a combination of keys)
                foreach (KeyCode key in ability.GetKeyInputCombo()) {

                    //If key has not been recorded yet then add it to our list
                    if (this.abilityInputComboKeys.Contains(key) == false)
                        this.abilityInputComboKeys.Add(key);
                }


            }


        }

        /// <summary>
        /// Records all inputs made during play, tracking it in the recorded key input tracker, ready to be used to determine if a key input combo should trigger an ability activation
        /// </summary>
        private void KeyInputRecorder() {

            //If entity is activating an ability then freeze the interval of removing the recorded input by updating the time of last input
            if (this.IsActivatingAbility()) {
                this.timeOfLastKeyInputRecorded = Time.time;
                //End here as no inputs should be recorded whilst activating
                return;
            } else if (Time.time - this.timeOfLastKeyInputRecorded >= this.inputComboRecycleInterval) {
                //If interval has passed since last input then clear list ready for new set of combination keys
                this.recordedKeyInputHistory.Clear();
            }


            //If nothing is being pressed then end here as no need to loop for key pressed
            if (this.meEntity.inputManager.AnyKey() == false)
                return;

            //Loop through keycodes recording the input 
            foreach (KeyCode key in this.abilityInputComboKeys) {
                if (this.meEntity.inputManager.GetKeyDown(key)) {

                    //If the input history count is equal or greater then the longest possible combination required to activate an ability then lets recycle the list 
                    //(adding further keys to the existing combination will never activate an ability)                
                    if (this.recordedKeyInputHistory.Count >= this.abilityLongestInputCombo)
                        this.recordedKeyInputHistory.Clear();

                    //Add key
                    this.recordedKeyInputHistory.Add(key);
                    //Update last time of input
                    this.timeOfLastKeyInputRecorded = Time.time;
                    //Diagnostic info
                    this.AddToDiagnosticLog("Current Tracked Input History: " + string.Join(" > ", this.recordedKeyInputHistory));
                }
            }

        }


        /// <summary>
        /// Main function to handle weapon events. 
        /// Will listen to see if a button has been pressed to cycle to the next or previous weapon or that an 
        /// equip weapon trigger has been raised for a weapon
        /// </summary>
        private void WeaponWatcher() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true || this.inIdleMode == true)
                return;

            //No weapons so nothing to watch
            if (this.CurrentWeapons.Count == 0 || this.weaponBeingToggled == true)
                return;

            //If set to cycle to next weapon and the next weapon key has been pressed then equip next weapon in the list
            if (this.nextWeapon == true && this.ButtonPressed(ControllerButtonPressState.NextWeapon)) {
                StartCoroutine(this.EquipNextWeapon());
            }

            //If set to cycle to previous weapon and the previous weapon key has been pressed then equip next weapon in the list
            if (this.previousWeapon == true && this.ButtonPressed(ControllerButtonPressState.PreviousWeapon)) {
                StartCoroutine(this.EquipPreviousWeapon());
            }

            //Cycle through each weapon to see if the equip trigger has been raised 
            foreach (Weapon weapon in this.CurrentWeapons) {

                if (weapon.EquipWeaponTriggered(this.meEntity))
                    StartCoroutine(this.EquipWeapon(weapon));

            }

            //Check if we are dropping current weapon: 
            if (this.ButtonPressed(ControllerButtonPressState.DropCurrentWeapon)) {
                StartCoroutine(this.DropCurrentWeapon());
            }

        }

        /// <summary>
        /// Main function to handle weapon block events. 
        /// Will listen to see if a button is being held down if so then it will activate for the weapon to start blocking
        /// </summary>
        private void WeaponBlockAndParryWatcher() {

            //No weapons so nothing to watch
            if (this.CurrentWeapons.Count == 0)
                return;

            //If AI is not enabled or enabled and we are not restricting triggers then allow blocking
            if (this.enableAI == false || this.enableAI == true && this.aiRestrictSystemTriggers == false || this.autoWeaponBlock == true) {

                //If set to weapon block and block key has been pressed then start blocking
                if (this.enableWeaponBlock == true && this.isCurrentlyWeaponBlocking == false && (this.autoWeaponBlock == true || this.ButtonPressed(ControllerButtonPressState.WeaponBlock, ControllerButtonPressType.Hold))) {
                    this.StartWeaponBlocking();

                    return;
                }

            }


            //If set to weapon block and block key is not being pressed then stop blocking if entity is currently blocking
            if (this.enableWeaponBlock == true && this.isCurrentlyWeaponBlocking && this.autoWeaponBlock == false && this.ButtonPressed(ControllerButtonPressState.WeaponBlock, ControllerButtonPressType.Hold) == false) {
                this.StopWeaponBlocking();

                return;
            }


            //If AI is not enabled or enabled and we are not restricting triggers then allow parrying
            if (this.enableAI == false || this.enableAI == true && this.aiRestrictSystemTriggers == false) {
                //If set to cycle to next weapon and the next weapon key has been pressed then equip next weapon in the list
                if (this.enableWeaponParry == true && this.ButtonPressed(ControllerButtonPressState.WeaponParry, ControllerButtonPressType.Press)) {
                    this.ActivateWeaponParry();

                    return;
                }
            }

        }

        /// <summary>
        /// Main function to regen block duration by the amount defined in the settings
        /// </summary>
        private void RegenBlockDurability() {

            //If set to only regen when not blocking 
            if (this.blockDurabilityRegenWhenNotBlocking == true && this.isCurrentlyWeaponBlocking == true)
                return;

            if (this.currentMaxBlockDurability > 0) {
                // if we have block durability then regen it every second (when this is called)
                if (this.blockDurabilityRegenPerSecond > 0 && this.currentBlockDurability < this.currentMaxBlockDurability)
                    this.currentBlockDurability += this.blockDurabilityRegenPerSecond;


                // if we accidently go over the limit then set it back to the max
                if (this.currentBlockDurability > this.currentMaxBlockDurability)
                    this.currentBlockDurability = this.currentMaxBlockDurability;

                // if we go under 0 then set it to 0 
                if (this.currentBlockDurability < 0)
                    this.currentBlockDurability = 0;

            } else {
                // revert block durability to 0 as it's not used
                this.currentBlockDurability = 0;
            }

        }

        /// <summary>
        /// Main function to handle ability groups. Cycles through all ability groups calling the handler which deals with all the group scenario and events. 
        /// </summary>
        private void AbilityGroupWatcher() {

            // cycle through all ability groups
            foreach (AbilityGroup group in AbilityGroups)
                group.GroupHandler(this.meEntity);

        }


        /// <summary>
        /// Main function to handle abilities. Cycles through all abilities setup for the entity, activating any that can be activated. 
        /// </summary>
        private void AbilityWatcher() {

            // If we are in idle mode and we are not deactivating idle mode on ability input then we don't need to carry on with this method so return here
            if (inIdleMode == true && deactiveIdleModeOnAbilityInput == false)
                return;

            //If we are in idle mode and the current scroll ability has been pressed then take come out of idle mode to display the current scroll graphic (Even if we can activate or not)
            if (inIdleMode == true && deactiveIdleModeOnAbilityInput == true && this.CurrentScrollAbilityButtonPressed())
                StartCoroutine(this.SwitchIdleMode(true));

            foreach (ABC_Ability ability in CurrentAbilities) {

                //If we activated an ability then restart process to check list again
               if (this.ActivateAbility(ability) == true)
                break; 

            }

            return;
        }

        /// <summary>
        /// Activates the ability
        /// </summary>
        /// <param name="Ability">Ability to activate</param>
        ///<param name="IgnoreTriggerCheck">If true then activation will not check if the ability has been triggered or not. Use if you want to know if ability can activate before it is triggered</param>
        ///<param name="IgnoreActivationPermittedCheck">If true then activation will not check if the originator is allowed to activate abilities.</param>
        ///<param name="IgnoreComboCheck">If true then function will not run check combo checks which determines if the ability is next in line to activate.</param>
        ///<param name="IgnoreHoldPreparation">If true then the function will not use the hold to continue preparation functionality</param>
        /// <returns>True if ability was successfully activated else, false</returns>
        private bool ActivateAbility(ABC_Ability Ability, bool IgnoreTriggerCheck = false, bool IgnoreActivationPermittedCheck = false, bool IgnoreComboCheck = false, bool IgnoreHoldPreparation = false) {

            bool retVal = false;

            // if the correct key is pressed (Key/button/scroll) or autoCast is set to true or the wait to stop chainspell is over and were not casting and the spell isn't recharging and were not waiting for collider and were inCombat mode  + more 
            if (Ability.Activate(meEntity, IgnoreTriggerCheck, IgnoreActivationPermittedCheck, IgnoreComboCheck, IgnoreHoldPreparation)) {

                retVal = true;

                // if in idle mode then we need to turn it off (it will only get to here at the moment is the deactiveIdleModeOnAbilityInput is true)
                if (inIdleMode == true) // switch idle mode instantly so we are ready for combat
                    StartCoroutine(this.SwitchIdleMode(true));

                //record time ability was activated 
                this.timeOfLastAbilityActivation = Time.time;

            }

            return retVal;

        }


        /// <summary>
        /// Main function which handles AI for activating abilities. The method will cycle through all AI rules activating abilities that meet the conditions set. 
        /// </summary>
        private void AutoCastHandler() {

            //If the intermission hasn't been calculated yet then record a random between the minimum and maximum range
            if (this.AICheckIntermission == 0f)
                this.AICheckIntermission = Random.Range(this.minimumAICheckIntermission, this.maximumAICheckIntermission);


            //If the time since the last AI Action is less then the intermission then return here as we need to wait longer        
            if (Time.time - this.timeOfLastAIAction < this.AICheckIntermission)
                return;


            // if enableAI is off or we are currently activating an ability or we can't activate abilities then end method here 
            if (this.enableAI == false || this.IsActivatingAbility() == true || this.canActivateAbilities == false)
                return;


            // Grab a list of the rules
            List<AIRule> enabledAIRules = AIRules.Where(a => a.AIActive == true).ToList();


            // If we are randoming the rules (changes probability of abilities happening) then switch around the list. 
            if (this.randomizeAIRules == true && enabledAIRules.Count > 1) {
                for (int t = 0; t < enabledAIRules.Count; t++) {
                    var tmp = enabledAIRules[t];
                    int r = Random.Range(t, enabledAIRules.Count);
                    enabledAIRules[t] = enabledAIRules[r];
                    enabledAIRules[r] = tmp;
                }
            }



            // get all potential targets in a big range if the intermission time has passed since we last retrieved potential targets or character just initiated (AI Check intermission is 0)
            if (this.potentialTargets.Count == 0 || Time.time - this.timeOfLastAIPotentialTargetRetrieval >= this.aiPotentialTargetRetrievalIntermission + Random.Range(0f, 0.8f)) { // random range used so overlap spheres do not all happen at once causing fps drops

                this.potentialTargets = ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, this.maxAIRange);

                //Record the time we last retrieved potential targets
                this.timeOfLastAIPotentialTargetRetrieval = Time.time;

                // If we are randoming the potential targets then switch around the list. 
                if (this.aiRandomizePotentialTargets == true && this.potentialTargets.Count > 1) {
                    for (int t = 0; t < potentialTargets.Count; t++) {
                        var tmp = potentialTargets[t];
                        int r = Random.Range(t, potentialTargets.Count);
                        potentialTargets[t] = potentialTargets[r];
                        potentialTargets[r] = tmp;
                    }
                }

                //make sure our navigational target is top of list as that is our current interest 
                if (this.currentNavAIDestination != null && this.navAIEnabled == true && this.potentialTargets.Count(e => e.transform == this.currentNavAIDestination) > 0) {
                    ABC_IEntity currentNavAI = this.potentialTargets.Where(e => e.transform == this.currentNavAIDestination).FirstOrDefault();
                    this.potentialTargets.Remove(currentNavAI);
                    this.potentialTargets.Insert(0, currentNavAI);
                }

            }



            // loop through active rules
            foreach (AIRule rule in enabledAIRules.Where(a => a.AIActive == true)) {

                //If rule has a cooldown and has not yet activated (game start) then assign a cooldown (for randomness) 
                if (rule.AIEnableRuleCooldown == true && rule.AIRuleLastActivationTime == 0) {
                    rule.ActivateRuleCoolDown();
                    continue;
                }

                //If rule has a cooldown and the cooldown is not over then continue 
                if (rule.AIEnableRuleCooldown == true && Time.time - rule.AIRuleLastActivationTime <= rule.AIRuleCurrentCooldown)
                    continue;


                // The final target which the ability will activate on/conditions have been successfully checked with
                GameObject finalTarget = null;

                //Determine if we can activate without a target
                bool canActivateWithoutTarget = rule.CanActivateWithoutTarget();

                //If we can't activate without a target then cycle through potential targets
                if (canActivateWithoutTarget == false) {
                    // list through targets and check conditions (HP/Mana/Range/Status etc)
                    foreach (ABC_IEntity obj in potentialTargets) {

                        // check if obj has the right target conditions (HP/Mana/Range/Status etc)
                        if (rule.CorrectTargetConditions(obj.gameObject, meEntity) == false)
                            continue;

                        // if condition has been met then finalise target (target might switch due to getting target of x etc)
                        finalTarget = rule.FinaliseTarget(obj.gameObject, meEntity, potentialTargets);


                        // If the finalise target method returns a target then we are ready to activate the ability so we can break this loop
                        if (finalTarget != null)
                            break;


                    } // end looping through targets


                    // If the final target is null then we could not find the appropriate target or satisfy the right conditions so we can move to the next rule
                    if (finalTarget == null)
                        continue;
                }



                //else do the AI Action
                switch (rule.selectedAIAction) {
                    case AIRule.AIAction.ActivateAbility:

                        //set target ready for ability, if target can't be set then continue to next rule
                        if (canActivateWithoutTarget == false && this.SetTarget(finalTarget, false, true, rule.bypassTargetLimitations) == false)
                            continue;

                        // find the ability using the ID
                        ABC_Ability ability = this.FindAbility(rule.AIAbilityID);

                        //If ability doesn't exist end here
                        if (ability == null)
                            continue;

                        //If ability is combo blocked (with some leeway as this is AI) then find the next avaliable comboID
                        if (ability.IsComboBlocked(meEntity, false, true)) {
                            ability = this.FindAbility(meEntity.GetAbilityNextAvaliableComboID(ability.abilityID, true));

                            //If not found continue to next rule
                            if (ability == null)
                                continue;

                        }

                        // Activate ability but skip combo check as we did it just before - if ability doesn't activate then continue to next rule 
                        if (this.ActivateAbility(ability, true, false, true, true) == false) {
                            this.AddToDiagnosticLog("autoCast failed for " + gameObject.name + " AbilityID: " + ability.name + "  could not activate");
                            continue;

                        }


                        //initiate a cooldown on the rule unless ability is a combo and not at the end of the combo chain
                        if (meEntity.IsAbilityLastInComboChain(ability.abilityID) == true)
                            rule.ActivateRuleCoolDown();


                        break;
                    case AIRule.AIAction.ActivateAbilityGroup:

                        //If there is no groups to enable then we can continue to next rule
                        if (this.AbilityGroups.Where(item => rule.AIActivateAbilityGroupIDs.Contains(item.groupID) && item.groupEnabled == false).Count() == 0)
                            continue;

                        //Go for list of groups defined and enable all the abilities in them 
                        foreach (int groupID in rule.AIActivateAbilityGroupIDs)
                            this.EnableAbilityGroup(groupID);


                        //initiate a cooldown on the rule 
                        rule.ActivateRuleCoolDown();

                        break;
                    case AIRule.AIAction.ActivateBlocking:

                        //If weapon blocking is not enabled or disabled (for a duration) or entity is already blocking then continue to next rule 
                        if (this.enableWeaponBlock == false || this.weaponBlockingDisabled == true || this.isCurrentlyWeaponBlocking == true)
                            continue;


                        //Start auto weapon blocking 
                        StartCoroutine(this.ActivateAutoWeaponBlock(Random.Range(rule.AIBlockDurationMin, rule.AIBlockDurationMax), 5f));


                        //initiate a cooldown on the rule 
                        rule.ActivateRuleCoolDown();


                        break;
                    case AIRule.AIAction.EquipWeapon:

                        //Equip the weapon configured 
                        StartCoroutine(this.EquipWeapon(rule.AIWeaponID));


                        //initiate a cooldown on the rule 
                        rule.ActivateRuleCoolDown();

                        break;

                }


                //Record the time we last did an AI Action on a global scale
                this.timeOfLastAIAction = Time.time;

                //Reroll the intermission to get a new random number between the minimum and maximum 
                this.AICheckIntermission = Random.Range(this.minimumAICheckIntermission, this.maximumAICheckIntermission);



                // we have successfully completed the AI Action by getting this far then we can return here and wait for the method to be called again
                return;


            } // end looping through rules
        }


        /// <summary>
        /// Main function which handles auto selecting a target. 
        /// </summary>
        private void AutoTargetHandler() {


            // if were on target type hold and user is not pressing the key then we need to take off target object
            if (this.autoTargetType == AutoTargetType.Hold && (this.targetObject != null || this.softTarget != null)
                && (this.ButtonPressed(ControllerButtonPressState.AutoTarget, ControllerButtonPressType.Hold) == false)) {

                this.RemoveSoftTarget(true);
                this.RemoveTarget(true);

            }


            // if were on target type press and we have a target 
            if (this.autoTargetType == AutoTargetType.Press && (this.targetObject != null || this.softTarget != null)
                && (this.ButtonPressed(ControllerButtonPressState.AutoTarget, ControllerButtonPressType.Press))) {

                this.RemoveSoftTarget(true);
                this.RemoveTarget(true);

                // return method as we don't want to run the below code as it will just retarget
                return;
            }


            // if we already have a target and we are not always swapping to closest OR targettype is not auto and the auto target button is not being pressed then we can return 
            if ((this.targetObject != null || this.softTarget != null) && this.autoTargetSwapClosest == false || (this.autoTargetType != AutoTargetType.Auto && this.ButtonPressed(ControllerButtonPressState.AutoTarget) == false))
                return;



            foreach (ABC_IEntity targ in ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, targetSelectRange, true).ToList()) {

                //If target is not alive then don't target 
                if (targ.HasABCStateManager() == true && targ.healthValue <= 0)
                    continue;

                // we don't want to: Target us, target the object we already targeting (autoTargetSwapClosest) , target an inactive object, target the terrain, target a surrounding object or target something without a state manager script
                if (targ.gameObject == gameObject || targ.gameObject == targetObject && targetObject != null || targ.gameObject == softTarget && softTarget != null || targ.gameObject.activeInHierarchy == false || Terrain.activeTerrain != null && Terrain.activeTerrain.transform.gameObject == targ.gameObject || targ.gameObject.name.Contains("*_ABCSurroundingObject") || targ.HasABCStateManager() == false)
                    continue;


                // if were always swapping then we need to compare the current target/softtarget (depending on settings) to the new target. If the new target is not closer then the conditions have not been met. 
                if (this.autoTargetSwapClosest == true) {

                    if (this.autoTargetSoftTarget == true && this.softTarget != null && Vector3.Distance(softTarget.transform.position, meTransform.position) < Vector3.Distance(targ.gameObject.transform.position, meTransform.position)) {

                        //If we got this far then soft target is closer then the potential new target, however to keep targetting the soft target then the entity needs to be facing it (if ticked too)
                        if (this.autoTargetSelfFacing == false || this.autoTargetSelfFacing == true && this.autoTargetSwapClosestPrioritiseSelfFacing == false || this.autoTargetSelfFacing == true && this.autoTargetSwapClosestPrioritiseSelfFacing == true && this.FacingTargetCheck(this.softTarget) == true)
                            continue;

                    } else if (this.autoTargetSoftTarget == false && this.targetObject != null && Vector3.Distance(targetObject.transform.position, meTransform.position) < Vector3.Distance(targ.gameObject.transform.position, meTransform.position)) {

                        //If we got this far then  target is closer then the potential new target, however to keep targetting the soft target then the entity needs to be facing it (if ticked too)
                        if (this.autoTargetSelfFacing == false || this.autoTargetSelfFacing == true && this.autoTargetSwapClosestPrioritiseSelfFacing == false || this.autoTargetSelfFacing == true && this.autoTargetSwapClosestPrioritiseSelfFacing == true && this.FacingTargetCheck(this.targetObject) == true)
                            continue;

                        continue;
                    }

                }




                // if we are only targeting specific tags then check the target has the tag 
                if (this.autoTargetTagOnly == true && ABC_Utilities.ObjectHasTag(targ.gameObject, ABC_Utilities.ConvertTags(meEntity, autoTargetTags)) == false)
                    continue;


                // is the entity facing the target?
                if (this.autoTargetSelfFacing == true && this.FacingTargetCheck(targ.gameObject) == false)
                    continue;



                // is the entity in the camera view - Note: This checks if the target is visible in ANY camera (includes scene camera in editor)
                if (this.autoTargetInCamera == true && targ.gameObject.GetComponentInChildren<Renderer>() != null && targ.gameObject.GetComponentInChildren<Renderer>().isVisible == false)
                    continue;


                //If we about to auto target the next soft target, but we have a target, and set to only soft target override if we have no target then continue
                if (this.autoTargetSoftTarget == true && this.targetObject != null && this.autoTargetSoftTargetOverrideOnlyIfNoTarget == true)
                    continue; 


                // if we reached this far then the conditions are still good so set the target and return method
                SetTarget(targ.gameObject, autoTargetSoftTarget);
                // return method as we are done
                return;

            }



        }


        /// <summary>
        /// Function will set or remove a target depending on situations or buttons pressed or not pressed. 
        /// </summary>
        private void TargetSelectHandler() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;


            switch (this.targetSelectType) {
                case TargetSelectType.None:

                    // don't get target unless we are waiting on a new target (ability before target) 
                    if (this.WaitingForNewTarget() == true && this.ButtonPressed(ControllerButtonPressState.TargetClick))
                        this.GetTarget("click", TargetSelectType.Mouse);


                    break;
                default:

                    if ((this.clickForTarget == true || this.waitingOnNewTarget == true) && this.ButtonPressed(ControllerButtonPressState.TargetClick))
                        this.GetTarget("click", this.targetSelectType);

                    // we only need to hover for target
                    if (this.hoverForTarget == true && this.waitingOnNewTarget == false)
                        this.GetTarget("hover", this.targetSelectType);
                    break;
            }



        }

        /// <summary>
        /// Function will deselect any targets that shouldn't be targets due to certain conditions like not being in range or target no longer being active
        /// </summary>
        private void TargetDeselectHandler() {

            if (this.targetObject == null)
                return;

            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(this.targetObject);

            // check if our target is still active and in range and has health. if not turn off the target select. Also remove target if the softtarget confirm button has been repressed
            if (targetObject.activeInHierarchy == false || this.InRange(targetObject, targetSelectRange) == false || entity.HasABCStateManager() == true && entity.healthValue <= 0)
                this.RemoveTarget();

        }

        /// <summary>
        /// Main function which handles confirming a soft target and converting it to a target
        /// </summary>
        private void SoftTargetHandler() {

            //If we already have a target then pressing the confirm soft target button again will instead take off the target (toggle on/off)
            if (this.enableAI == false && this.ButtonPressed(ControllerButtonPressState.ConfirmSoftTarget) && this.targetObject != null && this.waitingOnNewTarget == false) {
                this.RemoveTarget();
                return;
            }

            if (this.softTarget == null)
                return;


            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(this.softTarget);


            // if an object dies or is no longer in range we need to take off the softtarget
            if (softTarget.activeInHierarchy == false || InRange(softTarget, targetSelectRange) == false || entity.HasABCStateManager() == true && entity.healthValue <= 0) {
                this.RemoveSoftTarget();
            }


            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;


            // if the confirmation button has been pressed then we can now turn off softTarget and do the real set target
            if (this.ButtonPressed(ControllerButtonPressState.ConfirmSoftTarget)) {


                softTargetOverride = false; // turn off global override (used for ability before target)
                GameObject newTarget = softTarget;

                // set softtarget as new target 
                this.SetTarget(newTarget);




            }

        }

        /// <summary>
        /// Main function which handles tabbing/cycling through targets. Will check for the next or previous button and then switch the target accordingly
        /// </summary>
        private void TabTargetsHandler() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;


            // integer decides if we going next or previous - If next then variable is set to 1. If previous then variable is set to 2. If nothing pressed it stays at 0.
            int nextOrPrev = 0;

            if (this.ButtonPressed(ControllerButtonPressState.NextTarget)) {
                nextOrPrev = 1;

            } else if (this.ButtonPressed(ControllerButtonPressState.PreviousTarget)) {
                nextOrPrev = 2;
            }


            // store the current target/softTarget (in seperate variable so we can add different types of targets)
            GameObject currentTarget = null;
            if (this.targetObject != null) {

                currentTarget = this.targetObject;

            } else if (this.softTarget != null) {

                currentTarget = this.softTarget;

            }



            // If we don't have a current target or button hasn't been pressed to go next or previous then we can end here
            if (currentTarget == null || nextOrPrev == 0)
                return;


            // get all targets in a big range from the current target or softtarget 
            List<ABC_IEntity> potentialTabTargets = ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, this.targetSelectRange).ToList();

            // if any of our new objects found are not present in our global list we are going to add them 
            foreach (ABC_IEntity entity in potentialTabTargets) {


                // lets add any new objects found in the overlap sphere to our global tab targets list as long as:
                //  we don't: Target us (unless conditioned too), target the object we already targeting (autoTargetSwapClosest) , target an inactive object, target surrounding object, target the terrain or target something without a state manager script

                if (this.CurrentTabTargets.Contains(entity) == false && (entity.gameObject == gameObject && this.tabTargetToSelf == true || entity.gameObject != gameObject && this.tabTargetToSelf == false) && entity.gameObject.activeInHierarchy == true && (Terrain.activeTerrain != null && Terrain.activeTerrain.transform.gameObject != entity.gameObject) && entity.gameObject.name.Contains("*_ABCSurroundingObject") == false && entity.HasABCStateManager() == true) {
                    this.CurrentTabTargets.Add(entity);
                }
            }


            //if our global list has targets that are not present in our new objects we are going to remove them as they are now out of range
            CurrentTabTargets.RemoveAll(x => potentialTabTargets.Contains(x) == false);


            //If we don't have any tag targets to go through then end here 
            if (this.CurrentTabTargets.Count() == 0) {
                meEntity.AddToDiagnosticLog("Tab Targetting: no potential tab targets found using range: " + this.targetSelectRange);
                return;
            }

            // as this is tabbing through a list we want to start at the index of our current target or current soft target depending
            // we search for target first unless tab soft target is set to true 

            int currentTargetIndex = 0;

            if (targetObject != null && tabSoftTarget == false) {

                currentTargetIndex = CurrentTabTargets.FindIndex(t => t.gameObject == targetObject);

            } else if (softTarget != null) {


                currentTargetIndex = CurrentTabTargets.FindIndex(t => t.gameObject == softTarget);
            }



            // loop starting from from our current target 
            int counter = currentTargetIndex;


            // depending on next or previous we need to start the counter on one up or one down from starting position (as the loop stops when we get back to the starting point)
            if (nextOrPrev == 1) {
                counter += 1;
            } else if (nextOrPrev == 2) {
                counter -= 1;

            }


            // do while loop going up or down till we find a target which matches the conditions set 
            while (counter != currentTargetIndex) {


                // if we get to the end of the list we need to go back to the start or end depending on if we going next or prev
                if (counter >= CurrentTabTargets.Count && nextOrPrev == 1) {
                    counter = 0;
                } else if (counter <= -1 && nextOrPrev == 2) {
                    counter = CurrentTabTargets.Count - 1;
                }


                // record the object we are currently dealing with 
                ABC_IEntity target = CurrentTabTargets[counter];

                // were going to be checking a lot of conditions so a boolean will keep track of the progress 
                bool tabConditionsOk = true;

                if (this.tabTargetTagOnly == true) {
                    // if the tags do not match then set conditions off
                    if (ABC_Utilities.ObjectHasTag(target.gameObject, ABC_Utilities.ConvertTags(this.meEntity, tabTargetTags)) == false)
                        tabConditionsOk = false;
                }

                // is the entity in the camera view - Note: This checks if the target is visible in ANY camera (includes scene camera in editor)

                if (this.tabTargetInCamera == true && target.gameObject.GetComponent<Renderer>() != null && target.gameObject.GetComponentInChildren<Renderer>().isVisible == false) {
                    // target is not visible in any camera so turn conditions false 
                    tabConditionsOk = false;
                    // condition not met so continue to next loop
                    break;

                }


                // if the conditions are still ok then set the target and break the loop
                if (tabConditionsOk == true) {
                    this.SetTarget(target.gameObject, tabSoftTarget);
                    // return method as we are done
                    return;
                }


                // increment or decrement counter depending on if we going up or down 

                if (nextOrPrev == 1) {
                    counter += 1;
                } else if (nextOrPrev == 2) {
                    counter -= 1;
                }


            }


        }


        /// <summary>
        /// Checks for the key press to switch in and out of Idle mode. In idle mode the entity can not activate abilities.
        /// </summary>
        private void CheckForIdleToggle() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;

            // if the idle toggle button has not been pressed then we can end here 
            if (this.ButtonPressed(ControllerButtonPressState.IdleToggle))
                StartCoroutine(this.SwitchIdleMode());

        }


        /// <summary>
        /// Will reset/cancel ABC actions if the cancel button is triggered. Examples of actions cancelled include: waiting for new targets, softtarget overrides, ability activation, removing current targets
        /// </summary>
        private void CheckForCancelInput() {

            // cancel not triggered so we can stop here
            if (CancelTriggered() == false)
                return;

            // if were waiting on target then we turn that off 
            if (waitingOnNewTarget == true || waitingOnNewMouseTarget == true || waitingOnNewWorldTarget == true) {

                waitingOnNewTarget = false;
                waitingOnNewWorldTarget = false;
                softTargetOverride = false;
                waitingOnNewMouseTarget = false;
            }


            //if currently activating an ability then interrupt the activation
            if (this.IsActivatingAbility() == true) {

                //interupt ability
                this.InterruptAbilityActivation();

            } else {

                // else lets remove all our targets 
                this.RemoveWorldTarget();
                this.RemoveTarget();
                this.RemoveSoftTarget();

            }

        }

        /// <summary>
        /// Checks if user has pressed the next/previous or quick-key to change the current scroll ability. 
        /// </summary>
        private void CheckScrollAbilityKeyPress() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;


            if (this.nextScroll == true && this.ButtonPressed(ControllerButtonPressState.NextScrollAbility)) {
                StartCoroutine(this.NextScrollAbility());
            }

            if (this.previousScroll == true && this.ButtonPressed(ControllerButtonPressState.PreviousScrollAbility)) {
                StartCoroutine(this.PreviousScrollAbility());
            }


            // check through loop for scrollQuickKey if pressed then call jump to method

            for (int i = 0; i < CurrentAbilities.Count; i++) {
                if (CurrentAbilities[i].IsAnEnabledScrollAbility() && CurrentAbilities[i].ScrollQuickKeyPressed(this.meEntity)) {
                    StartCoroutine(this.JumpToScrollAbility(CurrentAbilities[i].abilityID));
                    break;
                }


            }


        }

        /// <summary>
        /// Function will handle showing and hiding the crosshair override depending on if the correct button is being pressed/held down. Will also activate crosshair override graphics and animations
        /// </summary>
        private void CheckCrossHairOverrideKeyPress() {

            //If AI is enabled and we are restricting triggers then end here unless persistant crosshair mode is on
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true && this.persistentCrosshairAestheticMode == false)
                return;

            if (this.crosshairEnabled == false && this.persistentCrosshairAestheticMode == false)
                return;

            //Track if button pressed
            bool showCrossHairPressed = this.ButtonPressed(ControllerButtonPressState.ShowCrossHair, ControllerButtonPressType.Hold);

            if (this.blockCrosshairOverride == false && (showCrossHairPressed == true || this.persistentCrosshairAestheticMode == true)) {


                // make crosshair appear by setting value to true (OnGui does the rest) 
                if (this.showCrossHairOverride == false && showCrossHairPressed == true)
                    this.showCrossHairOverride = true;


                // if not showing Aesthetic then we done here 
                if (this.useCrossHairOverrideAesthetics == false)
                    return;


                // activate object and particle if they exist
                if (this.CrossHairOverrideGraphic != null && this.CrossHairOverrideGraphic.activeInHierarchy == false)
                    this.ActivateGraphic(ControllerGraphicType.CrossHairOverride);


                if (this.IsReloading() == true || this.isCurrentlyWeaponBlocking || this.isCurrentlyWeaponParrying || this.inIdleMode || this.AINavigationInProgress() || this.hitRestrictsAbilityActivation || this.IsActivatingAbility() || this.weaponBeingToggled || Time.time - this.timeOfLastAbilityActivation <= this.abilityActivationInterval + Mathf.Max(0, this.tempAbilityActivationIntervalAdjustment) + (this.tempAbilityActivationIntervalAdjustment < 0 ? 0 : 0.2f)) {
                    //If doing other things (activating, just activated (with breathing space unless it's -tempinterval as thats probably quick firing), blocking, parrying etc) then don't start any crosshair animations

                } else if (this.prioritiseCurrentWeaponCrosshairAnimation == true && this.CurrentEquippedWeapon != null && this.CurrentEquippedWeapon.useWeaponCrosshairOverrideAnimations == true) {
                    //If using the crosshair override animation on weapon then toggle the animation
                    this.CurrentEquippedWeapon.ToggleCrosshairOverrideAnimation(this.meEntity, true);

                } else { //else toggle the global non specific weapon animation

                    //Start animation runner
                    if (this.crossHairOverrideAnimationRunnerClip != null) {
                        if (this.crossHairOverrideAnimationRunnerOnEntity)
                            this.StartAnimationRunner(ControllerAnimationState.CrossHairOverride, meEntity.animationRunner);

                        if (this.crossHairOverrideAnimationRunnerOnScrollGraphic)
                            this.StartAnimationRunner(ControllerAnimationState.CrossHairOverride, meEntity.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner());

                        if (this.crossHairOverrideAnimationRunnerOnWeapon)
                            meEntity.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAnimationRunner(ControllerAnimationState.CrossHairOverride, ar));

                    }

                    //start animator
                    if (this.crossHairOverrideAnimatorParameter != "") {

                        if (this.crossHairOverrideAnimateOnEntity)
                            this.StartAnimation(ControllerAnimationState.CrossHairOverride, Ani);

                        if (this.crossHairOverrideAnimateOnScrollGraphic)
                            this.StartAnimation(ControllerAnimationState.CrossHairOverride, meEntity.currentScrollAbility.GetCurrentScrollAbilityAnimator());


                        if (this.crossHairOverrideAnimateOnWeapon)
                            meEntity.GetCurrentEquippedWeaponAnimators().ForEach(a => this.StartAnimation(ControllerAnimationState.CrossHairOverride, a));
                    }

                }


            }

            // if show crosshair pressed is false and we showing crosshair then disable
            if (showCrossHairPressed == false && showCrossHairOverride == true || this.inIdleMode) {

                //no longer show crosshair 
                showCrossHairOverride = false;


                // if not showing Aesthetic or we always showing aesthetic then we done here 
                if (useCrossHairOverrideAesthetics == false || this.inIdleMode == false && this.persistentCrosshairAestheticMode == true)
                    return;

                // return graphic back to ABCPool
                if (CrossHairOverrideGraphic != null && CrossHairOverrideGraphic.activeInHierarchy == true)
                    ABC_Utilities.PoolObject(CrossHairOverrideGraphic);

                //If using the crosshair override animation on weapon then toggle off the animation
                if (this.prioritiseCurrentWeaponCrosshairAnimation == true && this.CurrentEquippedWeapon != null) {

                    this.CurrentEquippedWeapon.ToggleCrosshairOverrideAnimation(this.meEntity, false);

                } else { //else toggle off the global non specific weapon animation


                    if (this.crossHairOverrideAnimationRunnerClip != null) {
                        if (this.crossHairOverrideAnimationRunnerOnEntity)
                            this.EndAnimationRunner(ControllerAnimationState.CrossHairOverride, meEntity.animationRunner);

                        if (this.crossHairOverrideAnimationRunnerOnScrollGraphic)
                            this.EndAnimationRunner(ControllerAnimationState.CrossHairOverride, meEntity.currentScrollAbility.GetCurrentScrollAbilityAnimationRunner());

                        if (this.crossHairOverrideAnimationRunnerOnWeapon)
                            meEntity.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(ControllerAnimationState.CrossHairOverride, ar));
                    }

                    // end animation if parameter is provided
                    if (this.crossHairOverrideAnimatorParameter != "") {

                        if (this.crossHairOverrideAnimateOnEntity)
                            this.EndAnimation(ControllerAnimationState.CrossHairOverride, Ani);

                        //If enabled then activate the animation on the current scroll graphic object
                        if (this.crossHairOverrideAnimateOnScrollGraphic && meEntity.currentScrollAbility != null)
                            this.EndAnimation(ControllerAnimationState.CrossHairOverride, meEntity.currentScrollAbility.GetCurrentScrollAbilityAnimator());

                        if (this.crossHairOverrideAnimateOnWeapon && meEntity.currentEquippedWeapon != null)
                            meEntity.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(ControllerAnimationState.CrossHairOverride, a));

                    }

                }


            }

        }

        /// <summary>
        /// Will reload the current scroll ability or weapon if the correct button has been pressed
        /// </summary>
        private void CheckAmmoReload() {

            //If AI is enabled and we are restricting triggers then end here
            if (this.enableAI == true && this.aiRestrictSystemTriggers == true)
                return;


            //Check if scroll ability is set to reload 
            if (this.ButtonPressed(ControllerButtonPressState.ScrollAbilityReload) && CurrentScrollAbility != null) {
                // if reloadkey has been pressed
                StartCoroutine(this.CurrentScrollAbility.ReloadAmmo(meEntity));
            }

            //Check if weapon is set to reload 
            if (this.meEntity.inputManager.ShoulderTriggersPressed() == false && this.ButtonPressed(ControllerButtonPressState.WeaponReload) && CurrentEquippedWeapon != null) {
                // if reloadkey has been pressed
                StartCoroutine(this.CurrentEquippedWeapon.ReloadWeaponAmmo(meEntity));
            }


        }

        /// <summary>
        /// Will make the entity wander to a random position on the NavMesh
        /// </summary>
        private void AINavigationWanderHandler() {

            //If not active then end handler here
            if (this.wanderTillDestinationSet == false || this.navAIEnabled == false || this.blockNavAI == true || this.meNavAgent.enabled == false || this.meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;


            //If the entity is moving towards our goal destination then we can return as we don't want to start wandering
            if (this.navAIDestinationSet == true)
                return;

            //If we are wandering and within stopping distance then we can stop wandering as we have reached the random point 
            if (this.navAIWanderSet == true && this.AINavigationWithinStoppingDistance())
                this.ClearAINavigationDestination();


            //if it's too early to start wandering then return here
            if (Time.time - this.navAIWanderSetTime < this.wanderInterval)
                return;


            //Get random point on NavMesh 
            Vector3 randomDestination = meTransform.position + Random.insideUnitSphere * this.wanderAreaRange;
            NavMeshHit hit;



            //if Position is not found then return
            if (NavMesh.SamplePosition(randomDestination, out hit, this.wanderAreaRange, 1) == false)
                return;


            // else set destination to new random destination 
            this.meNavAgent.SetDestination(hit.position);
            this.meNavAgent.speed = this.wanderSpeed;

            //No longer require a stopping distance as this is a random wander so will set it to a hardcoded value
            this.meNavAgent.stoppingDistance = 2f;

            //If the agent was stopped previously start it up again
            if (this.meNavAgent.isStopped == true)
                this.meNavAgent.isStopped = false;

            //we are wandering
            this.navAIWanderSet = true;
            this.navAIWanderSetTime = Time.time;

            //Start wander animation
            this.StartAnimation(ControllerAnimationState.AINavWander, this.Ani);
            this.StartAnimationRunner(ControllerAnimationState.AINavWander, meEntity.animationRunner);



        }

        /// <summary>
        /// Will set the defined stopping distance for the AI Navigation
        /// </summary>
        /// <param name="ValidateOnly">If true then the stopping distance will only be set if the current stopping distance is not between the minimum and max configured</param>
        private void AINavigationSetStoppingDistance(bool ValidateOnly = false) {

            //If not active then end manager here
            if (this.navAIEnabled == false || this.blockNavAI == true || meNavAgent.enabled == false || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;

            //If we validating only and stopping distance is within min and max then end here 
            if (ValidateOnly == true && this.meNavAgent.stoppingDistance >= this.minimumStopDistance && this.meNavAgent.stoppingDistance <= this.maximumStopDistance)
                return;


            this.meNavAgent.stoppingDistance = Random.Range(this.minimumStopDistance, this.maximumStopDistance);

        }

        /// <summary>
        /// Will handle what happens when an AI entity is falling (requires navigation to be enabled)
        /// </summary>
        private void AINavigationFallingHandler() {

            if (this.navAIEnabled == false)
                return;


            //Record if currently in air
            bool currentlyInAir = meEntity.isInTheAir == true;

            //If not in the air then make sure falling animation is not playing
            if (currentlyInAir == false && this.navAIFalling == true) {
                this.EndAnimation(ControllerAnimationState.AINavFalling, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavFalling, meEntity.animationRunner);
                this.navAIFalling = false;
            }


            //If not active then end handler here
            if (this.blockNavAI == true || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;


            //If the entity is in air then start the falling animation
            if (currentlyInAir == true) {
                this.StartAnimationRunner(ControllerAnimationState.AINavFalling, meEntity.animationRunner);
                this.StartAnimation(ControllerAnimationState.AINavFalling, this.Ani);
                this.navAIFalling = true;
            }

        }

        /// <summary>
        /// Sets the destination of the NavMeshAgent to the goal destination defined in settings
        /// </summary>
        private void AINavigationHandler() {

            //If not active then end manager here
            if (this.navAIEnabled == false || this.blockNavAI == true || meNavAgent.enabled == false || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;

            //try and find destination
            this.FindAINavigationDestination();


            //If destination has not been found then end here
            if (this.currentNavAIDestination == null)
                return;


            // If we are not in line of sight of the destination or they are out of range or are no longer active then clear the destination 
            if (this.AINavigationEntityInSight(this.currentNavAIDestination) == false || Vector3.Distance(this.currentNavAIDestination.transform.position, this.meTransform.position) > this.destinationSearchRadius || this.currentNavAIDestination.gameObject.activeInHierarchy == false) {

                //Remove or find new destination if previously set 
                if (this.navAIDestinationSet == true) {

                    //Clear Destination
                    this.ClearAINavigationDestination();

                    //Try and find new destination 
                    this.FindAINavigationDestination();

                    //If no destination found then go to idle if set and end here
                    if (this.currentNavAIDestination == null) {

                        //Toggle to idle mode as we no longer have a destination
                        if (this.navAIToggleIdleMode)
                            StartCoroutine(this.ToggleIdleMode(true));


                        return;
                    }

                }

            }


            //If this is first time setting the destination then set the random stopping distance
            if (this.navAIDestinationSet == false) {

                this.AINavigationSetStoppingDistance();

                //Toggle out of idle mode when destination set (if enabled) 
                if (this.navAIToggleIdleMode) {

                    if (this.navAITriggerIdleModeForTags.Count > 0 && ABC_Utilities.ObjectHasTag(this.currentNavAIDestination.gameObject, ABC_Utilities.ConvertTags(this.meEntity, this.navAITriggerIdleModeForTags)))
                        StartCoroutine(this.ToggleIdleMode(true));
                    else
                        StartCoroutine(this.ToggleIdleMode(false));

                }

            }


            //If for some reason the nav agent has slipped off the mesh then we will warp them back 
            if (this.meNavAgent.enabled == true && this.meNavAgent.isOnNavMesh == false) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(this.transform.position, out hit, 5.0f, NavMesh.AllAreas)) {
                    this.meNavAgent.Warp(hit.position);
                }
            }

            //If we got this far the entities knows about the destination so set it and stopping distance
            this.meNavAgent.SetDestination(this.currentNavAIDestination.position);

            //Tell rest of component that main destination has been set with this also stop the wandering flag
            this.navAIDestinationSet = true;

            if (this.navAIWanderSet) {
                this.navAIWanderSet = false;
                this.EndAnimation(ControllerAnimationState.AINavWander, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavWander, meEntity.animationRunner);
            }


            //If idle mode being toggled then don't move
            if (this.idleModeBeingToggled == true && this.navAIPreventMovementWhenSwitchingIdleMode) {

                this.meNavAgent.speed = 0f;

            } else if (this.AINavigationWithinStoppingDistance()) {
                //If we are within the stopping distance then call the at destination behaviour manager else animate movement
                this.AINavigationAtDestinationBehaviourManager();

            } else if (this.navAIChangeDestinationMovingTowards && Vector3.Distance(this.meTransform.position, this.currentNavAIDestination.position) <= this.navAIChangeDestinationMoveTowardsStartingDistance + 1) {
                // else if entity is moving towards destination and the distance between entity and destination has not increased since the entity started moving then start the move forward animation and set the speed 
                this.StartAnimation(ControllerAnimationState.AINavMoveForward, this.Ani);
                this.StartAnimationRunner(ControllerAnimationState.AINavMoveForward, meEntity.animationRunner);
                this.meNavAgent.speed = this.moveForwardSpeed += this.navSpeedAdjustment + this.navWeaponSpeedAdjustment;

            } else {
                // else move towards destination as normal stopping any previous stop at destination behaviours
                this.AINavigationStopDestinationBehaviours();

                //set speed and start animation
                this.StartAnimation(ControllerAnimationState.AINavMoveToDestination, this.Ani);
                this.StartAnimationRunner(ControllerAnimationState.AINavMoveToDestination, meEntity.animationRunner);
                this.meNavAgent.speed = this.speed += this.navSpeedAdjustment + this.navWeaponSpeedAdjustment;

            }

            //If the agent was stopped previously start it up again
            if (this.meNavAgent.isStopped == true)
                this.meNavAgent.isStopped = false;


        }

        /// <summary>
        /// Will return true if the entity is currently moving towards AI Nav destination or any At Destination Behaviours are running
        /// </summary>
        /// <returns>True if AI Navigation is in progress, else false</returns>
        private bool AINavigationInProgress() {

            //If navigation not turned on then end manager here
            if (this.navAIEnabled == false || meNavAgent.enabled == false)
                return false;

            //If no target 
            if (this.navAIDestinationSet == false)
                return false;

            if (this.AINavigationWithinStoppingDistance() == true && this.navAIChangeDestinationMovingTowards == false && this.navAIChangeDestinationMovingAway == false && this.navAIRotateAroundDestination == false)
                return false;


            //If we got this far then AI navigation is in progress
            return true;


        }

        /// <summary>
        /// Will turn the entity to face the AI navigation destination
        /// </summary>
        private void AINavigationTurnToDestination() {

            //If not active or activating abilities then end manager here
            if (this.navAIEnabled == false || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;

            if (this.navAIDestinationSet == true && this.alwaysFaceDestination && this.IsActivatingAbility() == false) {
                Vector3 lookDirection = currentNavAIDestination.transform.position - meTransform.position;
                //Reset y so it's always just turning to the destination
                lookDirection.y = 0;

                meTransform.rotation = Quaternion.Lerp(meTransform.rotation, Quaternion.LookRotation(lookDirection), this.faceDestinationTurnSpeed * Time.deltaTime);

            }
        }

        /// <summary>
        /// Manages the behaviour once the entity reaches the destination includes stop walking animations and special behaviours like rotating round or changing stopping distance to move forward or back
        /// </summary>
        private void AINavigationAtDestinationBehaviourManager() {

            //If not active then end manager here
            if (this.navAIEnabled == false || this.blockNavAI == true || meTransform.gameObject.activeInHierarchy == false)
                return;

            //Stop main movement animation (Interval in to it spamming the end animation call)

            if (this.navAIStopAnimationTime == 0 || Time.time - this.navAIStopAnimationTime >= 0.5f) {
                this.EndAnimation(ControllerAnimationState.AINavMoveToDestination, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavMoveToDestination, meEntity.animationRunner);
                this.navAIStopAnimationTime = Time.time;
            }

            //If activating ability then end here
            if (this.IsActivatingAbility() == true)
                return;

            if (this.navAIDestinationSet == false || this.inIdleMode == true)
                return;


            //For game start set random start time for rotate around intervals (stops all enemies rotating in sync)
            if (this.navAIRotateAroundDestinationStartTime == 0)
                this.navAIRotateAroundDestinationStartTime = Time.time + Random.Range(0f, this.rotationInterval + 10f);

            //If we was previously in the behaviour of changing distance and moving towards we can turn this off now 
            if (this.navAIChangeDestinationMovingTowards) {
                this.navAIChangeDestinationMovingTowards = false;
                this.EndAnimation(ControllerAnimationState.AINavMoveForward, this.Ani);
                this.EndAnimationRunner(ControllerAnimationState.AINavMoveForward, meEntity.animationRunner);
            }


            //determine if we can rotate around target by doing a dice roll and making sure the last rotation ended more then interval time ago 
            if (this.enableRotationBehaviour && ABC_Utilities.DiceRoll(0f, 49f) && (Time.time - navAIRotateAroundDestinationStartTime > this.rotationInterval + this.navAIRotateAroundDestinationDuration)) {
                this.navAIRotateAroundDestination = true;
                this.navAIRotateAroundDestinationStartTime = Time.time;
                this.navAIRotateAroundDestinationDuration = Random.Range(this.rotationMinDuration, this.rotationMaxDuration);
            }


            //determine if we can change distance from target by doing a dice roll and making sure the last destination change ended more then interval time ago 
            if (this.enableDistanceBehaviour && ABC_Utilities.DiceRoll(50f, 100f) && (Time.time - this.navAIChangeDestinationDistanceStartTime > this.distanceChangeInterval)) {
                this.navAIChangeDestinationDistanceStartTime = Time.time;

                //Loop through dice rolling until logic has decided to move forward or back 
                bool diceLocked = true;

                while (diceLocked) {

                    float diceRoll = Random.Range(0f, 100f);

                    //If we are moving forwards then set the stopping distance to a number lower then what we are already on (Navagent will move automatically move due to the stopping distance being lower)
                    if (ABC_Utilities.DiceRoll(0f, 49f)) {
                        //if we are already at lowest distance then can't move forward
                        if (this.meNavAgent.stoppingDistance == this.minimumStopDistance)
                            break;
                        //Update stopping distance so it is closer
                        this.meNavAgent.stoppingDistance = Random.Range(this.meNavAgent.stoppingDistance, this.minimumStopDistance);
                        //Tell rest of component we are moving towards destination
                        this.navAIChangeDestinationMovingTowards = true;
                        //Start the moving forward animation to get a head start
                        this.StartAnimation(ControllerAnimationState.AINavMoveForward, this.Ani);
                        this.StartAnimationRunner(ControllerAnimationState.AINavMoveForward, meEntity.animationRunner);
                        //Track the current distance between destination and entity. If this distance ever grows then this behaviour will stop as entity is no longer at destination and needs to reach it via normal method
                        this.navAIChangeDestinationMoveTowardsStartingDistance = Vector3.Distance(meTransform.position, this.currentNavAIDestination.position);
                        //Stop loop
                        diceLocked = false;
                    }

                    //If we are moving backwards then set the stopping distance to a number higher then what we are already on and also set the boolean to move the entity back away from current destination
                    if (ABC_Utilities.DiceRoll(50f, 100f) && diceLocked == true) {
                        //if we are already at highest distance then can't move back
                        if (this.meNavAgent.stoppingDistance == this.maximumStopDistance)
                            break;
                        //update stopping distance so it's further away
                        this.meNavAgent.stoppingDistance = Random.Range(this.meNavAgent.stoppingDistance, this.maximumStopDistance);
                        //Set property to true so entity will start moving backwards (is not automatic like moving towards)
                        this.navAIChangeDestinationMovingAway = true;
                        //stop loop
                        diceLocked = false;
                    }
                }

            }



        }


        /// <summary>
        /// Will rotate the entity around the destination
        /// </summary>
        private void AINavigationRotateAroundDestination() {

            //If not active then end manager here
            if (this.navAIEnabled == false || this.blockNavAI == true || this.navAIRotateAroundDestination == false || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;

            //if we have rotated for the duration set then make sure to turn off rotation
            if (Time.time - navAIRotateAroundDestinationStartTime > this.navAIRotateAroundDestinationDuration) {
                this.navAIRotateAroundDestination = false;
                this.EndAnimation(this.navAIRotateAroundDirection == 1 ? ControllerAnimationState.AINavRotateAroundRight : ControllerAnimationState.AINavRotateAroundLeft, this.Ani);
                this.EndAnimationRunner(this.navAIRotateAroundDirection == 1 ? ControllerAnimationState.AINavRotateAroundRight : ControllerAnimationState.AINavRotateAroundLeft, meEntity.animationRunner);
                //reset direction
                this.navAIRotateAroundDirection = 0;
            }


            if (this.currentNavAIDestination == null || this.navAIRotateAroundDestination == false)
                return;


            //Random direction (1 for right, 2 for left) - if 0 then not set yet
            if (this.navAIRotateAroundDirection == 0)
                this.navAIRotateAroundDirection = Random.Range(1, 3);

            //Rotate around destination by speed set
            transform.RotateAround(this.currentNavAIDestination.position, this.navAIRotateAroundDirection == 1 ? Vector3.down : Vector3.up, this.rotationSpeed * Time.deltaTime);

            //Start Animation
            this.StartAnimation(this.navAIRotateAroundDirection == 1 ? ControllerAnimationState.AINavRotateAroundRight : ControllerAnimationState.AINavRotateAroundLeft, this.Ani);
            this.StartAnimationRunner(this.navAIRotateAroundDirection == 1 ? ControllerAnimationState.AINavRotateAroundRight : ControllerAnimationState.AINavRotateAroundLeft, meEntity.animationRunner);
        }


        /// <summary>
        /// Will move the entity away from the destination, used once the stopping distance is increased to move the entity to that position
        /// </summary>
        private void AINavigationMoveAwayFromDestination(bool ForceMoveAway = false) {

            //If not active then end manager here
            if (this.navAIEnabled == false || this.blockNavAI == true || this.navAIChangeDestinationMovingAway == false && ForceMoveAway == false || this.currentNavAIDestination == null || meTransform.gameObject.activeInHierarchy == false || this.IsActivatingAbility() == true)
                return;


            //If we are focing the move away then push back 
            if (ForceMoveAway == true && this.meNavAgent.stoppingDistance <= this.maximumStopDistance) {
                this.meNavAgent.stoppingDistance += Random.Range(0.5f, 6f);
            }

            //Stopping distance
            float stoppingDistance = this.meNavAgent.stoppingDistance;

            //If we are close to the stopping distance then end the moving away 
            if (Vector3.Distance(this.meTransform.position, this.currentNavAIDestination.position) >= stoppingDistance - 2.5) {
                this.navAIChangeDestinationMovingAway = false;

                if (this.navAIStopAnimationTime == 0 || Time.time - this.navAIStopAnimationTime >= 0.5f) {
                    this.EndAnimation(ControllerAnimationState.AINavMoveBack, this.Ani);
                    this.EndAnimationRunner(ControllerAnimationState.AINavMoveBack, meEntity.animationRunner);
                    this.navAIStopAnimationTime = Time.time;
                }

                return;
            }

            //else move back
            transform.position = Vector3.MoveTowards(this.meTransform.position, this.currentNavAIDestination.position, -1 * this.moveBackSpeed * Time.deltaTime);

            //Start Animation
            this.StartAnimation(ControllerAnimationState.AINavMoveBack, this.Ani);
            this.StartAnimationRunner(ControllerAnimationState.AINavMoveBack, meEntity.animationRunner);
        }

        /// <summary>
        /// Determines if the entity is in sight of the destination. If the entity was already previously aware of the destination then only range is checked else range and line of sight is checked. 
        /// </summary>
        /// <param name="Entity">Object to check if in line of sight</param>
        /// <returns>true if entity is in range and is either facing the destination or is already aware of the target, else false</returns>
        private bool AINavigationEntityInSight(Transform Entity) {

            if (Entity == null)
                return false;

            //If we don't care about line of sight then return true
            if (this.lineOfSightRequired == false)
                return true;


            // if entity is aware of destination presence already then return true; 
            if (this.currentNavAIDestination != null && this.navAIDestinationSet == true)
                return true;

            // if distance between the entity and destination is greater then the line of sight then return false as we too far away 
            if (meNavAgent.isActiveAndEnabled && meNavAgent.remainingDistance > this.lineOfSightRange)
                return false;

            //Check if entity is looking at the destination 
            var dir = (Entity.position - meTransform.position).normalized;
            var dot = Vector3.Dot(dir, meTransform.forward);

            //If facing the entity
            if (dot >= 0.3) {

                //Raycast from position to the direction of the entity
                Ray ray = new Ray(meTransform.position + new Vector3(0, 1, 0), dir);

                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit)) {

                    //If we hit something other then the Entity then there is an obstruction so entity is not in sight
                    if (ABC_Utilities.GetStaticABCEntity(rayHit.transform.gameObject).transform != Entity.transform)
                        return false;


                }

                // if we got this far then entity is in sight to return true 
                return true;

            } else {
                //If the entity is not already aware of the destination or can't see the destination then return false
                return false;
            }

        }

        /// <summary>
        /// Invoked repeatingly to find a new AI target every x amount of seconds
        /// </summary>
        private void AINavigationNewTargetInterval() {

            //Find new destination enabling the force new destination parameter
            this.FindAINavigationDestination(true);

        }

        /// <summary>
        /// Will search in a radius and set the AI navigation destination if the correct tag has been found
        /// </summary>
        /// <param name="ForceNewDestination">If true then a new destination will be found regardless</param>
        private void FindAINavigationDestination(bool ForceNewDestination = false) {

            if (ForceNewDestination == false && this.currentNavAIDestination != null || this.navAIEnabled == false || this.blockNavAI == true || this.IsActivatingAbility() == true)
                return;

            // get all objects in a  range
            List<ABC_IEntity> potentialAINavDestinations = ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, this.destinationSearchRadius, true);

            //remove self from list 
            if (potentialAINavDestinations.Contains(meEntity))
                potentialAINavDestinations.Remove(meEntity);

            //Store list of people we follow 
            List<string> tagList = this.destinationTags;

            //Do we randomise the tags
            if (this.randomizeDestinationTags == true && tagList.Count > 1) {
                for (int t = 0; t < tagList.Count; t++) {
                    var tmp = tagList[t];
                    int r = Random.Range(t, tagList.Count);
                    tagList[t] = tagList[r];
                    tagList[r] = tmp;
                }
            }


            //Loop through our destination tags in order till we find a match 
            foreach (string tag in ABC_Utilities.ConvertTags(this.meEntity, this.destinationTags)) {

                //loop through to find correct one that matches tag
                foreach (ABC_IEntity potDestination in potentialAINavDestinations) {

                    //If tag doesn't match move on
                    if (ABC_Utilities.ObjectHasTag(potDestination.gameObject, tag) == false)
                        continue;

                    //If entity is not in sight then move on
                    if (this.AINavigationEntityInSight(potDestination.transform) == false)
                        continue;

                    //If we got this far then correct object has been found so set destination and end here
                    this.currentNavAIDestination = potDestination.transform;

                    return;

                }
            }
        }

        /// <summary>
        /// Will determine if the entity is within stopping distance of the destination
        /// </summary>
        /// <returns>True if within stopping distance, else false</returns>
        private bool AINavigationWithinStoppingDistance() {

            //Declare stopping distance
            float stoppingDistance = this.meNavAgent.stoppingDistance;

            //If we have a nav target who we can't currently attack then increase stopping distance if we was about to get to close
            if (this.currentNavAIDestination != null && ABC_Utilities.GetStaticABCEntity(this.currentNavAIDestination.gameObject).CanBeTargetedByObject(meEntity.gameObject) == false && stoppingDistance <= 7)
                stoppingDistance += 5;

            // if distance between the entity and nav agents destination is less then the stopping distance then return true  
            if (this.meNavAgent.remainingDistance <= stoppingDistance && (this.meNavAgent.hasPath == false || (this.stopDistanceCheckVelocity == false || this.stopDistanceCheckVelocity == true && this.meNavAgent.velocity.sqrMagnitude == 0f)))
                return true;



            // else we are not within the stopping distance
            return false;


        }


        /// <summary>
        /// Disables the restrictAbilityActivation variable allowing the entity to activate abilities again
        /// </summary>
        private void DisableHitRestrictsAbilityActivation() {
            hitRestrictsAbilityActivation = false;
            this.AddToDiagnosticLog("Hit preventing casting has now been turned off for: " + gameObject.name);
        }


        /// <summary>
        /// Returns a bool indicating if the entity is in range of the provided object
        /// </summary>
        /// <param name="Target">The target to check range against</param>
        /// <param name="RangeMax">The range</param>
        /// <returns></returns>
        private bool InRange(GameObject Target, float RangeMax) {

            if (Vector3.Distance(meTransform.position, Target.transform.position) <= RangeMax) {
                // if our spawn position and target position distance is less then range then return true else false
                return true;
            } else {

                return false;
            }

        }

        /// <summary>
        /// Returns a bool indication if the entity is facing the target 
        /// </summary>
        /// <param name="Target">Target to check if entity is facing</param>
        /// <returns></returns>
        private bool FacingTargetCheck(GameObject Target) {

            var dir = (Target.transform.position - meTransform.position).normalized;
            var dot = Vector3.Dot(dir, meTransform.forward);

            if (dot >= 0.3) {
                //  were facing the target! Note: higher the value the more accuracy needed to face
                return true;
            } else {
                return false;

            }
        }

        /// <summary>
        /// Determines if the entity is waiting for a new target (ability before target). Can be either a new Target, Mouse Target or New World Target
        /// </summary>
        /// <returns>True if entity is waiting for a new target, else false</returns>
        private bool WaitingForNewTarget() {

            if (this.waitingOnNewTarget == true || this.waitingOnNewMouseTarget == true || this.waitingOnNewWorldTarget == true)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Sets entities world target
        /// </summary>
        /// <param name="Target">World target object</param>
        /// <param name="Pos">World Vector3 Position</param>
        private void SetWorldTarget(GameObject Target, Vector3 Pos) {

            this.worldTargetObject = Target;
            this.worldTargetPosition = Pos;


            if (this.logWorldSelection == true)
                StartCoroutine(this.AddToAbilityLog("World Selected at: " + Pos));

            this.AddToDiagnosticLog("World Selected at: " + Pos);

        }


        /// <summary>
        /// Sets entities target
        /// </summary>
        /// <param name="Target">The new target object</param>
        /// <param name="SoftTargetEnabled">If true then object provided will become a soft target, else a target</param>
        /// <param name="IgnoreRange">If true then target select range will be ignored</param>
        /// <param name="BypassTargeterLimits">If true then the ability will activate even if there is no open targeting slots on the entity due to the targeter limit already being reached</param>
        /// <returns>True if target was successfully set, else false</returns>
        private bool SetTarget(GameObject Target, bool SoftTargetEnabled = false, bool IgnoreRange = false, bool BypassTargeterLimits = false) {

            //If target is not alive then end here
            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Target);
            if (entity.HasABCStateManager() == true && entity.healthValue <= 0)
                return false;

            // If we are not in range and not ignoring range then we can finish the method here 
            if (this.InRange(Target, this.targetSelectRange) == false && IgnoreRange == false)
                return false;

            //If surrounding object then don't target 
            if (Target.name.Contains("*_ABCSurroundingObject"))
                return false;


            if (SoftTargetEnabled == true || this.softTargetOverride == true) { // Soft Target


                //Turn off indicator on current old target
                this.ShowSoftTargetIndicator(false);

                this.softTarget = Target;
                //Apply new indicator to new target
                this.ShowSoftTargetIndicator(true, Target);

                //Raise event 
                if (this.softTargetSetRaiseEvent)
                    this.meEntity.RaiseSoftTargetSetEvent(Target);

                //Add to logs
                if (this.logSoftTargetSelection == true)
                    StartCoroutine(this.AddToAbilityLog("Selecting " + softTarget.name + " please press " + (this.softTargetConfirmInputType == InputType.Button ? this.softTargetConfirmButton : this.softTargetConfirmKey.ToString()) + " to confirm Target"));

                this.AddToDiagnosticLog("Selecting " + softTarget.name + " please press " + (this.softTargetConfirmInputType == InputType.Button ? this.softTargetConfirmButton : this.softTargetConfirmKey.ToString()) + " to confirm Target");
            } else { // Full target



                //Checks if the target will allow for the entity (us) to target it (limits can be placed on the amount of objects that can target one entity)
                ABC_IEntity targetEntity = ABC_Utilities.GetStaticABCEntity(Target);

                if (BypassTargeterLimits == false && targetEntity.CanBeTargetedByObject(meEntity.gameObject) == false) {

                    //It's just been flagged that we can't target this entity, so if the entity is currently set as our target then remove them from being our target
                    if (this.targetObject == Target)
                        this.RemoveTarget();

                    //Lets move back if doing AI Navigation for space as we not attacking
                    this.AINavigationMoveAwayFromDestination(true);

                    //Log information
                    StartCoroutine(this.AddToAbilityLog("Unable to target: " + Target.name + " as the entity already has the limited amount of targeters"));

                    this.AddToDiagnosticLog("Unable to target: " + Target.name + " as the entity already has the limited amount of targeters");
                    return false;
                } else {

                    //make sure stopping distance is set right
                    AINavigationSetStoppingDistance(true);
                }


                //If currently have a softtarget then remove that 
                if (this.softTarget != null)
                    this.RemoveSoftTarget();

                // reset indicators
                this.ShowTargetIndicator(false);

                //remove us from previous targets targeter tracking, to open a slot for another entity to target
                if (this.targetObject != null) {
                    ABC_IEntity prevTargetEntity = ABC_Utilities.GetStaticABCEntity(this.targetObject);
                    prevTargetEntity.RemoveObjectFromTargeterTracker(meEntity.gameObject);
                }

                // set new target 
                this.targetObject = Target;

                //Let the target know we are targeting them, unless this was a target limit bypass
                if (BypassTargeterLimits == false)
                    targetEntity.AddObjectToTargeterTracker(meEntity.gameObject);

                // now set next indicators on new target 
                this.ShowTargetIndicator(true, Target);



                if (this.targetSelectRaiseEvent)
                    this.meEntity.RaiseTargetSetEvent(Target);

                if (this.logTargetSelection == true)
                    StartCoroutine(this.AddToAbilityLog("Selected Target: " + targetObject.name));

                this.AddToDiagnosticLog("Selected Target: " + targetObject.name);

                //  might be waiting on target so set this to false
                this.waitingOnNewTarget = false;


            }

            //target set correctly so return true
            return true;

        }


        /// <summary>
        /// Will get the object or world position at the cross hair or mouse position and set that as the entities target or world target. Will also remove targets depending on the click type (hover/click).
        /// </summary>
        /// <param name="ClickType">Click or Hover</param>
        /// <param name="SelectType">Method to get target (through current mouse position or crosshair)</param>
        private void GetTarget(string ClickType, TargetSelectType SelectType = TargetSelectType.Mouse) {

            //If mouse is currently over a UI menu (Event System) and the game is paused then return here as we are in a menu
            if (SelectType == TargetSelectType.Mouse && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && Time.timeScale == 0)
                return;

            Ray ray = new Ray();

            //If select type is mouse then record mouse position
            if (SelectType == TargetSelectType.Mouse)
                ray = meEntity.Camera.ScreenPointToRay(this.meEntity.inputManager.GetMousePosition());

            //If select type is crosshair then record crosshair position
            if (SelectType == TargetSelectType.Crosshair)
                ray = meEntity.Camera.ViewportPointToRay(new Vector3(crosshairPositionX, crosshairPositionY, 0f));

            //If waiting on mouse target then just retrieve that 
            if (this.waitingOnNewMouseTarget == true) {

                // no longer waiting on mouse target so turn mouse false
                this.waitingOnNewMouseTarget = false;

                //end here as we don't want to select target as we was just looking for mouse target 
                return;

            }

            // perform accurate raycast
            RaycastHit[] hit;
            hit = Physics.RaycastAll(ray, Mathf.Infinity, this.targetSelectLayerMask);

            //order by distance
            hit = hit.OrderBy(x => x.distance).ToArray();

            //capture the first actual hit point for the world target
            RaycastHit worldHit = hit.Where(n => n.transform.root.gameObject.name.Contains("ABC*_") == false).FirstOrDefault();

            //Ready to record target hit
            Transform targetHit = null;

            //If we hit something then record that as our target 
            if (worldHit.transform != null)
                targetHit = worldHit.transform;


            //If no target was hit or no ABC entity was hit and select leeway is true then be less strict about the target 
            if ((targetHit == null || ABC_Utilities.GetStaticABCEntity(targetHit.transform.gameObject).HasABCStateManager() == false) && this.targetSelectLeeway) {

                //first check if anything under our initial raycast had a ABC statemanager component (rather then just checking first collision)
                RaycastHit newHit = hit.Where(n => n.transform.gameObject != meTransform.gameObject && n.transform.root.gameObject.name.Contains("ABC*_") == false
                && ABC_Utilities.GetStaticABCEntity(n.transform.gameObject).HasABCStateManager() == true && ABC_Utilities.GetStaticABCEntity(n.transform.gameObject).healthValue > 0).FirstOrDefault();

                //If we found an ABC entity in initial raycast then set targethit
                if (newHit.transform != null) {

                    //Record as new target hit
                    targetHit = newHit.transform;

                } else {

                    //look for nearby ABC entites in a bigger radius (leeway) using the world hit as an origin or a point in the ray if no world hit recorded
                    List<ABC_IEntity> entities = ABC_Utilities.GetAllABCEntitiesInRange(worldHit.transform == null ? ray.GetPoint(50f) : worldHit.point, this.targetSelectLeewayRadius);

                    //capture the first statemanager hit for a new target
                    ABC_IEntity entityFound = entities.Where(n => n.transform.gameObject != meTransform.gameObject && n.transform.root.gameObject.name.Contains("ABC*_") == false
                    && ABC_Utilities.GetStaticABCEntity(n.transform.gameObject).HasABCStateManager() == true && ABC_Utilities.GetStaticABCEntity(n.transform.gameObject).healthValue > 0).FirstOrDefault();

                    //If we found someone record as the target hit
                    if (entityFound != null)
                        targetHit = entityFound.transform;
                }

            }

            // lets store world position if it exists
            if (worldHit.transform != null)
                this.SetWorldTarget(worldHit.transform.gameObject, worldHit.point);
            else if (ClickType.ToUpper() == "CLICK" && this.clickForTarget == true) { // we clicked/hovered off our target so turn that variable off
                this.RemoveWorldTarget();
            } else if (ClickType.ToUpper() == "HOVER" && this.clickForTarget == false) {
                // if were hovering and click for target is false then turn off (so if click for target we can always keep clicked target)
                this.RemoveWorldTarget();
            }

            //If waiting on world target then just retrieve that and end here
            if (this.waitingOnNewWorldTarget == true) {

                // no longer waiting on mouse target so turn mouse false
                this.waitingOnNewWorldTarget = false;

                //end here as we don't want to select target as we was just looking for world target 
                return;


            }


            //If a ABC Statemanager entity has been hit then set this as target
            if (targetHit != null && (this.selectTargetTagOnly == false || this.selectTargetTagOnly == true && ABC_Utilities.ObjectHasTag(targetHit.transform.gameObject, ABC_Utilities.ConvertTags(this.meEntity, this.selectTargetTags)))) {
                SetTarget(targetHit.transform.gameObject, selectSoftTarget);
            } else if (ClickType.ToUpper() == "CLICK" && this.clickForTarget == true) { // we clicked/hovered off our target so turn that variable off
                                                                                        // we clicked/hovered off our target so turn that variable off
                this.RemoveTarget();
            } else if (ClickType.ToUpper() == "HOVER" && this.clickForTarget == false) {
                // if were hovering and click for target is false then turn off (so if click for target we can always keep clicked target)
                this.RemoveTarget();
            }


        }


        /// <summary>
        /// Will activate/disable the target indicator. 
        /// </summary>
        /// <param name="Enabled">If true will activate and show the target indicator, else disable it</param>
        /// <param name="TargetObject">Object to place the indicator on</param>
        private void ShowTargetIndicator(bool Enabled, GameObject TargetObject = null) {

            // if were turning the indicator on and the target indicator has been created correctly and an object has been passed through and the indicator isn't already attached
            if (Enabled == true) {

                //If no object was provided to place the indicator on then end here
                if (TargetObject == null)
                    return;

                //If set to then add target outline glow
                if (this.targetOutlineGlow == true)
                    ABC_Utilities.ToggleOutlineGlow(TargetObject, true, this.targetOutlineGlowColour);

                //Apply the soft target shader to the target, storing the old shader which was replaced so we can revert back to it at a later date
                if (this.selectedTargetShader.Shader != null)
                    this.targetsPreviousShader = ABC_Utilities.ApplyShader(TargetObject, this.selectedTargetShader.Shader);


                //If the indicator object setting is not null and not already attached to the object then attach the soft indicator
                if (this.targetIndicator != null && this.targetIndicator.transform.IsChildOf(TargetObject.transform) == false) {

                    // change target Indicator position to target and make it a child
                    this.targetIndicator.transform.position = TargetObject.transform.position + this.targetIndicatorOffset + TargetObject.transform.forward * this.targetIndicatorForwardOffset + TargetObject.transform.right * this.targetIndicatorRightOffset;

                    //targetIndicator.transform.position 
                    this.targetIndicator.transform.parent = TargetObject.transform;

                    if (this.targetIndicatorScaleSize == true) {
                        // set the scale to the scale of object if setting has been set
                        this.targetIndicator.transform.localScale = new Vector3(TargetObject.transform.localScale.x + this.targetIndicatorScaleFactor, TargetObject.transform.localScale.y + this.targetIndicatorScaleFactor, TargetObject.transform.localScale.z + this.targetIndicatorScaleFactor);
                    }

                    // turn indicator on
                    this.targetIndicator.SetActive(true);

                }


                // turn on the health by calling the statemanager function from the entity interface
                ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(TargetObject.gameObject);


                if (iEntity.HasABCController()) {
                    iEntity.ShowManaGUI(true);
                    iEntity.ShowAbilityGroupsGUI(true);
                }

                if (iEntity.HasABCStateManager()) {
                    iEntity.ShowHealthGUI(true);
                    iEntity.ShowStatsGUI(true);
                }

            } else if (Enabled == false) {


                // turn off the indicator if it is attached to something
                if (this.targetIndicator != null && this.targetIndicator.transform.parent != ABC_Utilities.abcPool.transform)
                    ABC_Utilities.PoolObject(targetIndicator);


                //If target object is not null then revert shaders and turn off GUI
                if (this.targetObject != null) {

                    //remove target outline glow
                    ABC_Utilities.ToggleOutlineGlow(this.targetObject, false, this.targetOutlineGlowColour);

                    //apply the original shader to the current  target (removing the target shader)
                    if (this.selectedTargetShader.Shader != null)
                        ABC_Utilities.ApplyShader(this.targetObject, this.targetsPreviousShader);

                    // turn off the health and stat gui by calling the statemanager function 
                    ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(this.targetObject.gameObject);

                    if (iEntity.HasABCController()) {
                        iEntity.ShowManaGUI(false);
                        iEntity.ShowAbilityGroupsGUI(false);
                    }


                    if (iEntity.HasABCStateManager()) {
                        iEntity.ShowHealthGUI(false);
                        iEntity.ShowStatsGUI(false);
                    }

                }


            }

        }

        /// <summary>
        /// Will activate/disable the soft target indicator. 
        /// </summary>
        /// <param name="Enabled">If true will activate and show the soft target indicator, else disable it</param>
        /// <param name="TargetObject">Object to place the indicator on</param>
        private void ShowSoftTargetIndicator(bool Enabled, GameObject TargetObject = null) {

            if (Enabled == true) {

                //If no object was provided to place the indicator on then end here
                if (softTarget == null)
                    return;

                //If set to then add soft target outline glow
                if (this.softTargetOutlineGlow == true)
                    ABC_Utilities.ToggleOutlineGlow(TargetObject, true, this.softTargetOutlineGlowColour);


                //Apply the soft target shader to the target, storing the old shader which was replaced so we can revert back to it at a later date
                if (this.softTargetShader.Shader != null)
                    this.softTargetsPreviousShader = ABC_Utilities.ApplyShader(TargetObject, this.softTargetShader.Shader);


                //If the indicator object setting is not null and not already attached to the object then attach the soft indicator
                if (this.targetIndicatorSoft != null && targetIndicatorSoft.transform.IsChildOf(TargetObject.transform) == false) {

                    // change target Indicator position to target and make it a child
                    this.targetIndicatorSoft.transform.position = TargetObject.transform.position + softTargetIndicatorOffset + TargetObject.transform.forward * softTargetIndicatorForwardOffset + TargetObject.transform.right * softTargetIndicatorRightOffset;

                    this.targetIndicatorSoft.transform.parent = TargetObject.transform;

                    if (this.softTargetIndicatorScaleSize == true) {
                        // set the scale to the scale of object if setting has been set
                        this.targetIndicatorSoft.transform.localScale = new Vector3(TargetObject.transform.localScale.x + softTargetIndicatorScaleFactor, TargetObject.transform.localScale.y + softTargetIndicatorScaleFactor, TargetObject.transform.localScale.z + softTargetIndicatorScaleFactor);
                    }

                    this.targetIndicatorSoft.SetActive(true);

                }

                //If entity doesn't have target (which would already show gui) then show gui for soft target
                if (this.targetObject == null) {

                    // turn on the health by calling the statemanager function from the entity interface
                    ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(TargetObject.gameObject);

                    if (iEntity.HasABCController()) {
                        iEntity.ShowManaGUI(true);
                        iEntity.ShowAbilityGroupsGUI(true);
                    }

                    if (iEntity.HasABCStateManager()) {
                        iEntity.ShowHealthGUI(true);
                        iEntity.ShowStatsGUI(true);
                    }
                }



            } else if (Enabled == false) {

                // turn off the indicator if it is attached to something
                if (this.targetIndicatorSoft != null && this.targetIndicatorSoft.transform.parent != null)
                    ABC_Utilities.PoolObject(targetIndicatorSoft);

                if (this.softTarget != null) {

                    //If set to then remove soft target outline glow
                    ABC_Utilities.ToggleOutlineGlow(this.softTarget, false, this.softTargetOutlineGlowColour);

                    //apply the original shader to the current soft target (removing the soft target shader)
                    if (this.softTargetShader.Shader != null)
                        ABC_Utilities.ApplyShader(this.softTarget, this.softTargetsPreviousShader);


                    // turn off the health and stat gui if a target object has not been set 
                    if (this.targetObject == null) {
                        ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(this.softTarget.gameObject);

                        if (iEntity.HasABCController()) {
                            iEntity.ShowManaGUI(false);
                            iEntity.ShowAbilityGroupsGUI(false);
                        }


                        if (iEntity.HasABCStateManager()) {
                            iEntity.ShowHealthGUI(false);
                            iEntity.ShowStatsGUI(false);
                        }
                    }

                }


            }

        }


        /// <summary>
        /// will toggle the weapon provided either enabling/equipping or disabling/unequipping the weapon
        /// </summary>
        /// <param name="Weapon">The Weapon to equip or unequip</param>
        /// <param name="ToggleType">Determines if the weapon should be enabled or disabled</param>
        /// <param name="QuickToggle">True if this is a quick toggle which means weapon will equip/unequip instantly</param> 
        private IEnumerator ToggleWeapon(Weapon Weapon, WeaponState ToggleType, bool QuickToggle = false) {

            //If a weapon has not been provided or a weapon is already being toggled then end here
            if (Weapon == null || this.weaponBeingToggled == true || this.isCurrentlyWeaponParrying)
                yield break;

            //Let system know a weapon is currently being toggled
            this.weaponBeingToggled = true;

            //If equipping weapon
            if (ToggleType == WeaponState.Equip) {

                // track new weapon
                this.CurrentEquippedWeapon = Weapon;

                //If we are in idle mode then switch out of idle mode as we want to equip a weapon, we then end function here as switch idle mode function will recall this function
                if (this.inIdleMode) {
                    StartCoroutine(this.SwitchIdleMode());
                    yield break;
                }
            }

            //If blocking then stop weapon blocking so we can 
            if (this.isCurrentlyWeaponBlocking == true)
                this.StopWeaponBlocking();


            //Add logs
            this.AddToDiagnosticLog(this.name + " " + ToggleType.ToString() + "ping " + Weapon.weaponName);

            if (this.LogInformationAbout(LoggingType.WeaponInformation))
                this.AddToAbilityLog(this.name + " " + ToggleType.ToString() + "ping " + Weapon.weaponName);


            //Toggle the weapon
            yield return StartCoroutine(Weapon.ToggleWeapon(meEntity, ToggleType, QuickToggle));

            //Let system know a weapon is no longer being toggled
            this.weaponBeingToggled = false;

        }


        /// <summary>
        /// Enables the scroll ability - updating GUI Icons and activating any graphics. 
        /// </summary>
        /// <param name="ScrollAbility">Scroll ability to enable</param>
        /// <param name="ActivateAesthetic">True if the enable animation and graphics should activate, else false.</param>
        private IEnumerator EnableScrollAbility(ABC_Ability ScrollAbility, bool ActivateAesthetic = true) {

            if (ScrollAbility != null && ScrollAbility.IsAnEnabledScrollAbility()) {

                //Enable any ability groups set to activate when this scroll ability is enabled
                foreach (int ID in this.AbilityGroups.Where(item => item.enableOnScrollAbilitiesEnabled == true && item.enableOnScrollAbilityIDsActivated.Contains(this.CurrentScrollAbility.abilityID)).Select(group => group.groupID).ToList())
                    this.EnableAbilityGroup(ID);

                // activate Aesthetic
                yield return StartCoroutine(ScrollAbility.InitialiseScrollAbility(meEntity, ActivateAesthetic));


            }

        }


        /// <summary>
        /// Disables the scroll ability - removing GUI Icons and deactivating any graphics. 
        /// </summary>
        /// <param name="ScrollAbility">Scroll ability to disable</param>
        /// <param name="ActivateAesthetic">True if the disable animation and graphic should activate, else false.</param>
        private IEnumerator DisableScrollAbility(ABC_Ability ScrollAbility, bool ActivateAesthetic = true) {

            if (ScrollAbility != null && ScrollAbility.IsAScrollAbility()) {

                //disable any ability groups set to activate when this scroll ability is disabled
                foreach (int ID in this.AbilityGroups.Where(item => item.disableOnScrollAbilitiesDisabled == true && item.enableOnScrollAbilityIDsActivated.Contains(this.CurrentScrollAbility.abilityID)).Select(group => group.groupID).ToList())
                    this.DisableAbilityGroup(ID);

                //play deactivate graphic and wait till its done 
                yield return StartCoroutine(ScrollAbility.DeinitializeScrollAbility(meEntity, ActivateAesthetic));


            }


        }


        /// <summary>
        /// Will enable and equip the next scroll ability in the list
        /// </summary>
        /// <param name="ActivateAesthetic">True if the disable animation and graphic should activate, else false.</param>
        private IEnumerator NextScrollAbility(bool ActivateAesthetic = true) {


            // Double check more then one scroll ability exists in our list else end function here as there is nothing to go next too
            if (this.ScrollAbilityCount() == 1)
                yield break;

            //Determine which ability we are enabling and disabling first incase the scroll next button is being hit very fast 
            ABC_Ability abilityToEnable = null;
            ABC_Ability abilityToDisable = this.CurrentScrollAbility;

            //default index incase this is the first time we are activating a scroll ability (gamestart)
            var currentIndex = 0;

            // if we have a current scroll ability active then find the index 
            if (CurrentScrollAbility != null)
                currentIndex = CurrentAbilities.FindIndex(a => a == CurrentScrollAbility) + 1;

            //Record next scroll ability
            for (int i = currentIndex; i <= CurrentAbilities.Count; i++) {

                //If we have reached end of list then go to the start
                if (i >= CurrentAbilities.Count)
                    i = 0;


                if (CurrentAbilities[i].IsAnEnabledScrollAbility()) {
                    this.CurrentScrollAbility = CurrentAbilities[i];
                    abilityToEnable = this.CurrentScrollAbility;
                    break;
                }


            }


            // disable old scroll ability and wait till its done
            yield return StartCoroutine(this.DisableScrollAbility(abilityToDisable, ActivateAesthetic));

            // activate new scroll ability
            yield return StartCoroutine(this.EnableScrollAbility(abilityToEnable, ActivateAesthetic));



        }


        /// <summary>
        /// Will enable and equip the previous scroll ability in the list
        /// </summary>
        /// <param name="ActivateAesthetic">True if the disable animation and graphic should activate, else false.</param>
        private IEnumerator PreviousScrollAbility(bool ActivateAesthetic = true) {

            // Double check more then one scroll ability exists in our list else end function here as there is nothing to go next too
            if (this.ScrollAbilityCount() == 1)
                yield break;


            //Determine which ability we are enabling and disabling first incase the scroll previous button is being hit very fast 
            ABC_Ability abilityToEnable = null;
            ABC_Ability abilityToDisable = this.CurrentScrollAbility;

            var currentIndex = 0;

            // find the index of our current activated ability
            if (CurrentScrollAbility != null)
                currentIndex = CurrentAbilities.FindIndex(a => a == CurrentScrollAbility) - 1;


            for (int i = currentIndex; i >= -1; i--) {
                //If we have reached end of list then go to the start
                if (i < 0)
                    i = CurrentAbilities.Count - 1;

                if (CurrentAbilities[i].IsAnEnabledScrollAbility()) {
                    this.CurrentScrollAbility = CurrentAbilities[i];
                    abilityToEnable = this.CurrentScrollAbility;
                    break;
                }

            }

            // disable old scroll ability and wait till its done
            yield return StartCoroutine(this.DisableScrollAbility(abilityToDisable, ActivateAesthetic));

            // activate new scroll ability
            yield return StartCoroutine(this.EnableScrollAbility(abilityToEnable, ActivateAesthetic));

        }

        /// <summary>
        /// Will enable and equip the scroll ability
        /// </summary>
        /// <param name="AbilityID">The ID of the ability</param>
        private IEnumerator JumpToScrollAbility(int AbilityID) {

            //Determine which ability we are enabling and disabling first incase the method is called in quick succession  
            ABC_Ability abilityToDisable = this.CurrentScrollAbility;
            ABC_Ability abilityToEnable = this.FindAbility(AbilityID);


            //If ability to enable isn't a scroll then return here
            if (abilityToEnable.IsAnEnabledScrollAbility() == false)
                yield break;

            if (this.CurrentScrollAbility == abilityToEnable) {
                // already on this ability so no need to swap we can just return to stop this function
                yield break;
            }


            // set new scroll ability
            this.CurrentScrollAbility = abilityToEnable;


            // disable current scroll ability and wait till its done
            yield return StartCoroutine(this.DisableScrollAbility(abilityToDisable));


            // enable the ability
            yield return StartCoroutine(this.EnableScrollAbility(abilityToEnable));



        }

        /// <summary>
        /// Starts an animation clip using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to play - Initiating, Preparing etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        private void StartAnimationRunner(ControllerAnimationState State, ABC_AnimationsRunner AnimationRunner) {

            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipSpeed = 1f;
            float animationClipDelay = 0f;
            AvatarMask animationClipMask = null;

            AnimationClip currentAnimatorClipOverride = null;

            //If false then animation will not start if it's currently already being played by the animation runner
            bool startAnimationIfAlreadyRunning = true;

            //If false then animation will not interrupt the current clip playing
            bool interruptCurrentAnimation = true;

            //If true then runner overrides will be blocked
            bool blockRunnerOverrides = false;

            //Used to Check to see if an override clip should be played instead on navigational states
            Weapon currentWeapon = null;
            AnimatorClipRunnerOverride clipRunnerOverride = null;


            switch (State) {
                case ControllerAnimationState.CrossHairOverride:

                    animationClip = this.crossHairOverrideAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.crossHairOverrideAnimationRunnerClipSpeed;
                    animationClipDelay = this.crossHairOverrideAnimationRunnerClipDelay;
                    animationClipMask = this.crossHairOverrideAnimationRunnerMask.AvatarMask;

                    blockRunnerOverrides = true;

                    break;
                case ControllerAnimationState.AINavMoveToDestination:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animationClip = this.toDestinationAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.toDestinationAnimationRunnerClipSpeed;
                    animationClipDelay = this.toDestinationAnimationRunnerClipDelay;
                    animationClipMask = this.toDestinationAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;

                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.toDestinationAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.toDestinationAnimationRunnerClip.AnimationClip;

                    }




                    break;

                case ControllerAnimationState.AINavWander:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && (meNavAgent.isOnNavMesh == false || meNavAgent.isStopped == true))
                        return;

                    animationClip = this.wanderAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.wanderAnimationRunnerClipSpeed;
                    animationClipDelay = this.wanderAnimationRunnerClipDelay;
                    animationClipMask = this.wanderAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;


                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.wanderAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.wanderAnimationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavRotateAroundLeft:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && (meNavAgent.isOnNavMesh == false || meNavAgent.isStopped == true))
                        return;

                    animationClip = this.rotateAroundLeftAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.rotateAroundLeftAnimationRunnerClipSpeed;
                    animationClipDelay = this.rotateAroundLeftAnimationRunnerClipDelay;
                    animationClipMask = this.rotateAroundLeftAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;

                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.rotateAroundLeftAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.rotateAroundLeftAnimationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavRotateAroundRight:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && (meNavAgent.isOnNavMesh == false || meNavAgent.isStopped == true))
                        return;

                    animationClip = this.rotateAroundRightAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.rotateAroundRightAnimationRunnerClipSpeed;
                    animationClipDelay = this.rotateAroundRightAnimationRunnerClipDelay;
                    animationClipMask = this.rotateAroundRightAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;

                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.rotateAroundRightAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.rotateAroundRightAnimationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavMoveForward:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && (meNavAgent.isOnNavMesh == false || meNavAgent.isStopped == true))
                        return;

                    animationClip = this.moveForwardAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.moveForwardAnimationRunnerClipSpeed;
                    animationClipDelay = this.moveForwardAnimationRunnerClipDelay;
                    animationClipMask = this.moveForwardAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;

                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.moveForwardAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.moveForwardAnimationRunnerClip.AnimationClip;
                    }

                    break;
                case ControllerAnimationState.AINavMoveBack:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && (meNavAgent.isOnNavMesh == false || meNavAgent.isStopped == true))
                        return;

                    animationClip = this.moveBackAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.moveBackAnimationRunnerClipSpeed;
                    animationClipDelay = this.moveBackAnimationRunnerClipDelay;
                    animationClipMask = this.moveBackAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;

                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.moveBackAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.moveBackAnimationRunnerClip.AnimationClip;
                    }

                    break;
                case ControllerAnimationState.AINavFalling:

                    //Prevent animation from running if not in air 
                    if (meEntity.isInTheAir == false)
                        return;

                    animationClip = this.aiFallAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.aiFallAnimationRunnerClipSpeed;
                    animationClipDelay = this.aiFallAnimationRunnerClipDelay;
                    animationClipMask = this.aiFallAnimationRunnerMask.AvatarMask;

                    startAnimationIfAlreadyRunning = false;
                    interruptCurrentAnimation = false;


                    //Check to see if an override clip should be played instead
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.aiFallAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                        animationClipSpeed = clipRunnerOverride.animationRunnerClipSpeed;
                        animationClipDelay = clipRunnerOverride.animationRunnerClipDelay;
                        animationClipMask = clipRunnerOverride.animationRunnerMask.AvatarMask;

                        if (animationClipMask != null)
                            currentAnimatorClipOverride = this.aiFallAnimationRunnerClip.AnimationClip;
                    } else {

                        //Interrupt animation if clip override running 
                        if (AnimationRunner.IsAnimationClipOverrideRunning() == true)
                            interruptCurrentAnimation = true;
                    }

                    break;
            }


            // if animator parameter is null or animation runner is not provided or the animation clip is already running and it's set to not start animation if currently playing then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null || startAnimationIfAlreadyRunning == false && AnimationRunner.IsAnimationRunning(animationClip) == true && AnimationRunner.avatarMask == animationClipMask)
                return;


            if (clipRunnerOverride != null && blockRunnerOverrides == false)
                blockRunnerOverrides = true;

            //Start animation (blocking runner override if a cliprunner override was found)
            AnimationRunner.StartAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipMask, interruptCurrentAnimation, blockRunnerOverrides, false, currentAnimatorClipOverride);


        }

        /// <summary>
        /// End an animation clip currently playing using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to stop - Initiating, Preparing etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        /// <param name="Delay">(Optional) Delay before animation ends</param>
        private void EndAnimationRunner(ControllerAnimationState State, ABC_AnimationsRunner AnimationRunner, float Delay = 0f) {

            // set variables to be used later 
            AnimationClip animationClip = null;

            //If true then the animation will interrupt instead of smoothly end (used for coming out of crosshair override animation)
            bool interruptInsteadOfEnd = false;

            //If true then the animation will skip blending
            bool skipBlending = false;


            //Used to Check to see if an override clip should be played instead on navigational states
            Weapon currentWeapon = null;
            AnimatorClipRunnerOverride clipRunnerOverride = null;


            switch (State) {
                case ControllerAnimationState.CrossHairOverride:

                    animationClip = this.crossHairOverrideAnimationRunnerClip.AnimationClip;

                    if (this.IsReloading() == false)
                        skipBlending = true;

                    break;

                case ControllerAnimationState.AINavMoveToDestination:

                    animationClip = this.toDestinationAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.toDestinationAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }


                    break;

                case ControllerAnimationState.AINavWander:

                    animationClip = this.wanderAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.wanderAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavRotateAroundLeft:

                    animationClip = this.rotateAroundLeftAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.rotateAroundLeftAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavRotateAroundRight:

                    animationClip = this.rotateAroundRightAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.rotateAroundRightAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;

                case ControllerAnimationState.AINavMoveForward:

                    animationClip = this.moveForwardAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.moveForwardAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;
                case ControllerAnimationState.AINavMoveBack:

                    animationClip = this.moveBackAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.moveBackAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;
                case ControllerAnimationState.AINavFalling:

                    animationClip = this.aiFallAnimationRunnerClip.AnimationClip;

                    //Check to see if an override clip will be playing instead 
                    currentWeapon = this.meEntity.currentEquippedWeapon;
                    clipRunnerOverride = null;

                    //If a weapon is equipped 
                    if (currentWeapon != null && this.inIdleMode == false && this.aiFallAnimationRunnerEnableWeaponAnimationOverrides == true)
                        clipRunnerOverride = currentWeapon.ConvertClipIntoOverrideClip(animationClip);

                    //If we found a clip runner override then update the properties
                    if (clipRunnerOverride != null) {
                        animationClip = clipRunnerOverride.animationRunnerClip.AnimationClip;
                    }

                    break;
            }

            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null)
                return;

            if (interruptInsteadOfEnd == true)
                AnimationRunner.InterruptCurrentAnimation(true);
            else if (skipBlending == true)
                AnimationRunner.EndAnimation(animationClip, Delay, skipBlending);
            else
                AnimationRunner.EndAnimation(animationClip, Delay);

        }


        /// <summary>
        /// Starts an animation for the entity depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to play - crosshair etc</param>
        /// <param name="Animator">Animator component</param>
        private void StartAnimation(ControllerAnimationState State, Animator Animator) {


            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOnValue = "";



            switch (State) {
                case ControllerAnimationState.CrossHairOverride:

                    animatorParameterType = this.crossHairOverrideAnimatorParameterType;
                    animatorParameter = this.crossHairOverrideAnimatorParameter;
                    animatorOnValue = this.crossHairOverrideAnimatorOnValue;

                    break;
                case ControllerAnimationState.AINavMoveToDestination:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.toDestinationAnimatorParameterType;
                    animatorParameter = this.toDestinationAnimatorParameter;
                    animatorOnValue = this.toDestinationAnimatorOnValue;

                    break;

                case ControllerAnimationState.AINavWander:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.wanderAnimatorParameterType;
                    animatorParameter = this.wanderAnimatorParameter;
                    animatorOnValue = this.wanderAnimatorOnValue;

                    break;

                case ControllerAnimationState.AINavRotateAroundLeft:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.rotateAroundLeftAnimatorParameterType;
                    animatorParameter = this.rotateAroundLeftAnimatorParameter;
                    animatorOnValue = this.rotateAroundLeftAnimatorOnValue;

                    break;

                case ControllerAnimationState.AINavRotateAroundRight:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.rotateAroundRightAnimatorParameterType;
                    animatorParameter = this.rotateAroundRightAnimatorParameter;
                    animatorOnValue = this.rotateAroundRightAnimatorOnValue;

                    break;

                case ControllerAnimationState.AINavMoveForward:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.moveForwardAnimatorParameterType;
                    animatorParameter = this.moveForwardAnimatorParameter;
                    animatorOnValue = this.moveForwardAnimatorOnValue;

                    break;
                case ControllerAnimationState.AINavMoveBack:

                    //Prevent animation from running if nav agent has been stopped
                    if (meNavAgent != null && meNavAgent.isStopped == true)
                        return;

                    animatorParameterType = this.moveBackAnimatorParameterType;
                    animatorParameter = this.moveBackAnimatorParameter;
                    animatorOnValue = this.moveBackAnimatorOnValue;

                    break;
                case ControllerAnimationState.AINavFalling:

                    //Prevent animation from running if not in air
                    if (meEntity.isInTheAir == false)
                        return;

                    animatorParameterType = this.aiFallAnimatorParameterType;
                    animatorParameter = this.aiFallAnimatorParameter;
                    animatorOnValue = this.aiFallAnimatorOnValue;

                    break;


            }

            // if animator parameter is null or animator is not given then animation can't start so end here. 
            if (animatorParameter == "" || Animator == null || Animator.gameObject.activeInHierarchy == false) {
                return;
            }


            switch (animatorParameterType) {
                case AnimatorParameterType.Float:
                    Animator.SetFloat(animatorParameter, float.Parse(animatorOnValue));
                    break;
                case AnimatorParameterType.integer:
                    Animator.SetInteger(animatorParameter, int.Parse(animatorOnValue));
                    break;
                case AnimatorParameterType.Bool:
                    if (animatorOnValue != "True" && animatorOnValue != "False") {
                        this.AddToDiagnosticLog("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                    }
                    Animator.SetBool(animatorParameter, bool.Parse(animatorOnValue));
                    break;

                case AnimatorParameterType.Trigger:
                    Animator.SetTrigger(animatorParameter);
                    break;
            }



        }


        /// <summary>
        /// Ends the animation for the entity depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to stop - crosshair etc</param>
        /// <param name="Animator">Animator component</param>
        private void EndAnimation(ControllerAnimationState State, Animator Animator) {

            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOffValue = "";



            switch (State) {
                case ControllerAnimationState.CrossHairOverride:

                    animatorParameterType = this.crossHairOverrideAnimatorParameterType;
                    animatorParameter = this.crossHairOverrideAnimatorParameter;
                    animatorOffValue = this.crossHairOverrideAnimatorOffValue;

                    break;
                case ControllerAnimationState.AINavMoveToDestination:

                    animatorParameterType = this.toDestinationAnimatorParameterType;
                    animatorParameter = this.toDestinationAnimatorParameter;
                    animatorOffValue = this.toDestinationAnimatorOffValue;

                    break;

                case ControllerAnimationState.AINavWander:

                    animatorParameterType = this.wanderAnimatorParameterType;
                    animatorParameter = this.wanderAnimatorParameter;
                    animatorOffValue = this.wanderAnimatorOffValue;

                    break;

                case ControllerAnimationState.AINavRotateAroundLeft:

                    animatorParameterType = this.rotateAroundLeftAnimatorParameterType;
                    animatorParameter = this.rotateAroundLeftAnimatorParameter;
                    animatorOffValue = this.rotateAroundLeftAnimatorOffValue;

                    break;

                case ControllerAnimationState.AINavRotateAroundRight:

                    animatorParameterType = this.rotateAroundRightAnimatorParameterType;
                    animatorParameter = this.rotateAroundRightAnimatorParameter;
                    animatorOffValue = this.rotateAroundRightAnimatorOffValue;

                    break;

                case ControllerAnimationState.AINavMoveForward:

                    animatorParameterType = this.moveForwardAnimatorParameterType;
                    animatorParameter = this.moveForwardAnimatorParameter;
                    animatorOffValue = this.moveForwardAnimatorOffValue;

                    break;
                case ControllerAnimationState.AINavMoveBack:

                    animatorParameterType = this.moveBackAnimatorParameterType;
                    animatorParameter = this.moveBackAnimatorParameter;
                    animatorOffValue = this.moveBackAnimatorOffValue;

                    break;
                case ControllerAnimationState.AINavFalling:

                    animatorParameterType = this.aiFallAnimatorParameterType;
                    animatorParameter = this.aiFallAnimatorParameter;
                    animatorOffValue = this.aiFallAnimatorOffValue;

                    break;
            }


            // if animator parameter is null or animator is not given then animation can't start so end here. 
            if (animatorParameter == "" || Animator == null || Animator.gameObject.activeInHierarchy == false) {
                return;
            }




            switch (animatorParameterType) {
                case AnimatorParameterType.Float:
                    Animator.SetFloat(animatorParameter, float.Parse(animatorOffValue));
                    break;
                case AnimatorParameterType.integer:
                    Animator.SetInteger(animatorParameter, int.Parse(animatorOffValue));
                    break;
                case AnimatorParameterType.Bool:
                    if (animatorOffValue != "True" && animatorOffValue != "False") {
                        this.AddToDiagnosticLog("Animation unable to start for Boolean type - Make sure on and off are True/False values");
                    }
                    Animator.SetBool(animatorParameter, bool.Parse(animatorOffValue));
                    break;

                case AnimatorParameterType.Trigger:
                    // don't need to switch off as trigger does that straight away anyway.
                    break;
            }



        }



        /// <summary>
        /// activate graphics for this component
        /// </summary>
        /// <param name="GraphicType">Type of graphic to activate</param>
        /// <returns></returns>
        private GameObject ActivateGraphic(ControllerGraphicType GraphicType) {


            StartingPosition startingPosition = StartingPosition.Self;
            GameObject positionOnObject = null;
            string positionOnTag = null;
            bool auxiliarySoftTarget = false;
            Vector3 positionOffset = new Vector3(0, 0, 0);
            float positionForwardOffset = 0f;
            float positionRightOffset = 0f;
            //float duration = 2f;
            GameObject graphicObj = null;



            switch (GraphicType) {
                case ControllerGraphicType.CrossHairOverride:

                    startingPosition = this.crossHairOverrideStartPosition;
                    positionOnObject = this.crossHairOverridePositionOnObject.GameObject;
                    positionOnTag = this.crossHairOverridePositionOnTag;
                    auxiliarySoftTarget = this.crossHairOverridePositionAuxiliarySoftTarget;

                    positionOffset = this.crossHairOverrideAestheticsPositionOffset;
                    positionForwardOffset = this.crossHairOverrideAestheticsPositionForwardOffset;
                    positionRightOffset = this.crossHairOverrideAestheticsPositionRightOffset;

                    graphicObj = CrossHairOverrideGraphic;


                    break;

                default:

                    break;

            }



            //initial starting point is the entity incase anything goes wrong
            Vector3 position = meTransform.position;
            GameObject parentObject = meTransform.gameObject;

            // get starting position for the crosshair
            switch (startingPosition) {

                case StartingPosition.Self:
                    position = meTransform.position + positionOffset + meTransform.forward * positionForwardOffset + meTransform.right * positionRightOffset;
                    parentObject = gameObject;
                    break;
                case StartingPosition.OnObject:
                    if (positionOnObject != null) {
                        Transform onObjectTransform = positionOnObject.transform;
                        position = onObjectTransform.position + positionOffset + onObjectTransform.forward * positionForwardOffset + onObjectTransform.right * positionRightOffset;
                        parentObject = onObjectTransform.gameObject;
                    }
                    break;
                case StartingPosition.OnTag:
                    GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                    if (onTagObj != null) {
                        Transform onTagTransform = onTagObj.transform;
                        position = onTagTransform.position + positionOffset + onTagTransform.forward * positionForwardOffset + onTagTransform.right * positionRightOffset;
                        parentObject = onTagTransform.gameObject;
                    }
                    break;
                case StartingPosition.OnSelfTag:
                    GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(this.meEntity, positionOnTag);
                    if (onSelfTagObj != null) {
                        Transform onSelfTagTransform = onSelfTagObj.transform;
                        position = onSelfTagTransform.position + positionOffset + onSelfTagTransform.forward * positionForwardOffset + onSelfTagTransform.right * positionRightOffset;
                        parentObject = onSelfTagTransform.gameObject;
                    }
                    break;
                case StartingPosition.Target:
                    if (targetObject != null) {
                        var targetTransform = targetObject.transform;
                        position = targetTransform.position + positionOffset + targetTransform.forward * positionForwardOffset + targetTransform.right * positionRightOffset;
                        parentObject = targetObject;
                    } else if (auxiliarySoftTarget == true && softTarget != null) {
                        // if there is no current target object and auxiliary soft target is enabled then record current soft target instead
                        var softTargetTransform = softTarget.transform;
                        position = softTargetTransform.position + positionOffset + softTargetTransform.forward * positionForwardOffset + softTargetTransform.right * positionRightOffset;
                        parentObject = softTarget;
                    }
                    break;
                case StartingPosition.OnWorld:
                    if (worldTargetObject != null) {
                        var worldTargetTransform = worldTargetObject.transform;
                        position = worldTargetPosition + positionOffset + worldTargetTransform.forward * positionForwardOffset + worldTargetTransform.right * positionRightOffset;
                        parentObject = worldTargetObject;
                    }
                    break;
                case StartingPosition.CameraCenter:
                    var cameraTransform = meEntity.Camera.transform;
                    position = meEntity.Camera.transform.position + positionOffset + cameraTransform.forward * positionForwardOffset + cameraTransform.right * positionRightOffset;
                    parentObject = meEntity.Camera.gameObject;
                    break;
                default:
                    this.AddToDiagnosticLog("Error: starting position for " + GraphicType.ToString() + "  graphic was not correctly determined.");
                    break;
            }

            //if graphic object is still null then return here as it has not been setup correctly
            if (graphicObj == null)
                return graphicObj;


            // set position and parent
            graphicObj.transform.position = position;
            graphicObj.transform.rotation = parentObject.transform.rotation;
            graphicObj.transform.parent = parentObject.transform;

            // set it true 
            graphicObj.SetActive(true);


            return graphicObj;


        }




        #endregion


        // ********************** Game ******************

        #region Game

        void Awake() {


            // get transform navmeshagent and animator of object calling ability 
            meTransform = this.isChildInHierarchy ? transform.parent : transform;
            meNavAgent = ABC_Utilities.TraverseObjectForComponent(meTransform.gameObject, typeof(NavMeshAgent)) as NavMeshAgent;
            Ani = meTransform.GetComponentInChildren<Animator>();


            // entity interface to pass over to ability 
            this.meEntity = ABC_Utilities.GetStaticABCEntity(meTransform.gameObject);

            //Used for when domain loading is off
            if (this.meEntity.HasABCController() == false)
                this.meEntity.ReSetupEntity();



            // setup pool for future use 
            this.CreatePools();





        }


        void OnEnable() {

            //Initialise the component setting everything up ready to be used
            StartCoroutine(this.InitialiseComponent());

        }


        void OnDisable() {

            //If AI navigation is enabled then clear destination and stop animations
            if (this.navAIEnabled == true)
                this.ClearAINavigationDestination();

            // turn off any indicators 
            this.ShowTargetIndicator(false);
            this.ShowSoftTargetIndicator(false);

            //Remove target 
            this.RemoveSoftTarget();
            this.RemoveTarget();

            // stop any invokes/coroutines
            CancelInvoke();
            StopAllCoroutines();

            //disable any abilities activating 
            this.DisableAbilityActivation();

        }






        // if key pressed
        void LateUpdate() {

            //If time has been paused then stop update from running
            if (Time.timeScale == 0)
                return;

            // keep mana GUI up to date
            this.UpdateManaGUI();

            //Records all inputs made ready to be used to determine if a input combo should trigger an ability activation
            this.KeyInputRecorder();

            // check to see if key has been pressed to go in and out of combat 
            this.CheckForIdleToggle();

            // check to see if a weapon has been triggered to be equipped
            this.WeaponWatcher();

            //Check to see if weapon has been triggered to block or parry 
            this.WeaponBlockAndParryWatcher();

            //Run ability group handler which deals with everything to do with groups
            this.AbilityGroupWatcher();

            // main check press method for abilities
            this.AbilityWatcher();

            //If enabled then rotate around destination
            if (this.navAIRotateAroundDestination)
                this.AINavigationRotateAroundDestination();

            //If enabled then move away from destination
            if (this.navAIChangeDestinationMovingAway)
                this.AINavigationMoveAwayFromDestination();


            if (this.inIdleMode == false) {
                // none of this will run if we are not in combat mode (sheathed)  

                // check if button being pressed for crosshair
                if (this.crosshairEnabled == true && this.showCrossHairOnKey == true || this.persistentCrosshairAestheticMode == true)
                    this.CheckCrossHairOverrideKeyPress();

                //run target deselect handler if enabled or auto target select is true 
                if ((this.targetSelectType != TargetSelectType.None || this.autoTargetSelect == true))
                    this.TargetDeselectHandler();

                //run target select handler if enabled and interval is less then 1 (so it is using update) or waiting on new target (ability before target) 
                //Also run if waiting on new target (ability before target)
                if ((this.targetSelectType != TargetSelectType.None && this.targetSelectInterval == 0f) || this.WaitingForNewTarget() == true)
                    this.TargetSelectHandler();

                // if were in soft target mode and a soft target object is present 
                this.SoftTargetHandler();

                // can we tab through targets
                if (this.tabThroughTargets == true)
                    this.TabTargetsHandler();


                // can we press escape to cancel selected target etc 
                this.CheckForCancelInput();

                this.CheckScrollAbilityKeyPress();

                this.CheckAmmoReload();

                // auto select target if enabled and the interval is 0 (so it is using update)
                if (this.autoTargetSelect == true && this.autoTargetInterval == 0f)
                    this.AutoTargetHandler();


            }

        }

        private void FixedUpdate() {

            //Run the AI Nav falling handler
            this.AINavigationFallingHandler();

            //Turn to AI Navigation Destination
            this.AINavigationTurnToDestination();
        }


        void OnGUI() {

            //If time has been paused then stop any GUI from showing
            if (Time.timeScale == 0)
                return;

            if (this.crosshairEnabled) {
                // do we show normal or override crosshair
                if (this.showCrossHairOnKey == true && this.crossHairOverride.Texture != null && this.showCrossHairOverride == true) {
                    GUI.DrawTexture(new Rect(Screen.width * this.crosshairPositionX - this.crossHairOverride.Texture.height / 2, Screen.height - Screen.height * this.crosshairPositionY - this.crossHairOverride.Texture.width / 2, this.crossHairOverride.Texture.height, this.crossHairOverride.Texture.width), this.crossHairOverride.Texture, ScaleMode.ScaleToFit);
                } else if (this.crosshair.Texture != null && this.showCrossHairOverride == false) {
                    GUI.DrawTexture(new Rect(Screen.width * this.crosshairPositionX - this.crosshair.Texture.height / 2, Screen.height - Screen.height * this.crosshairPositionY - this.crosshair.Texture.width / 2, this.crosshair.Texture.height, this.crosshair.Texture.width), this.crosshair.Texture, ScaleMode.ScaleToFit);
                }
            }
        }

        #endregion


        // ********************** Export/Import Manager ******************

        #region Export/Import Manager

        /// <summary>
        /// Will add a global element (including weapon, abilities, AI) during run time
        /// </summary>
        /// <param name="GlobalElement">The global element to add</param>
        /// <param name="EquipWeapon">If true then the weapon added by the global weapon will be equipped</param>
        /// <param name="EnableGameTypeModification">Will enable or disable game type modification for the global element</param>
        /// <param name="GameTypeModification">Game type to modify element by</param>
        public IEnumerator AddGlobalElementAtRunTime(ABC_GlobalElement GlobalElement, bool EquipWeapon = false, bool EnableGameTypeModification = false, ABC_GlobalPortal.GameType GameTypeModification = ABC_GlobalPortal.GameType.Action) {


            ///////////////// Global Weapon
            if (GlobalElement.ElementWeapon != null) {

                //If weapon already exists then just enable it
                if (this.Weapons.Where(w => w.globalWeapon == GlobalElement).Count() > 0) {
                    this.EnableWeapon(GlobalElement.ElementWeapon.weaponID, EquipWeapon, false);
                } else {

                    Weapon newGlobalWeapon = new Weapon();
                    newGlobalWeapon.globalWeapon = GlobalElement;


                    // get unique effect ID
                    newGlobalWeapon.weaponID = -1;

                    this.Weapons.Add(newGlobalWeapon);
                }
            }


            ///////////////// Global Ability        
            if (GlobalElement.ElementAbilities != null) {
                ABC_Ability newGlobalAbility = new ABC_Ability();
                newGlobalAbility.globalAbilities = GlobalElement;

                // get unique effect ID
                newGlobalAbility.abilityID = -1;
                newGlobalAbility.globalAbilitiesEnableGameTypeModification = EnableGameTypeModification;
                newGlobalAbility.globalAbilitiesGameTypeModification = GameTypeModification;

                this.Abilities.Add(newGlobalAbility);
            }

            ///////////////// Global AI        
            //Get all rules of ability activations linked to ability        
            foreach (ABC_Controller.AIRule rule in GlobalElement.ElementAIRules) {

                //If rule already exists then continue 
                if (this.AIRules.Where(ai => ai.selectedAIAction == rule.selectedAIAction && ai.AIAbilityID == rule.AIAbilityID).Count() > 0)
                    continue;

                ABC_Controller.AIRule newRule = new ABC_Controller.AIRule();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(rule), newRule);
                AIRules.Add(newRule);
            }


            //If game is running then initialise component and equip any weapons or scroll abilities set in parameters
            if (Application.isPlaying) {
                yield return this.InitialiseComponent(true, EquipWeapon ? GlobalElement.ElementWeapon.weaponID : -1);
            }



            yield break;

        }





        #endregion




    }
}