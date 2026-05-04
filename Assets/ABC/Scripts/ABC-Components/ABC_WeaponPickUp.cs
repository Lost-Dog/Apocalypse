using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ABCToolkit {
    [System.Serializable]
    public class ABC_WeaponPickUp : MonoBehaviour {

        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// Determines if we are enabling an ability or importing 
        /// </summary>
        [Tooltip(" Determines if we are enabling an ability or importing ")]
        public EnableOrImport pickUpMode = EnableOrImport.Enable;

        /// <summary>
        /// If true then the weapon will be enabled on the entity picking up via the weapon name, else if false the weapon ID is used
        /// </summary>
        [Tooltip("If true then the weapon will be enabled on the entity picking up via the weapon name, else if false the weapon ID is used")]
        public bool enableWeaponUsingName = false;

        /// <summary>
        /// The delay before the weapon is enabled
        /// </summary>
        [Tooltip("The delay before the weapon is enabled")]
        public float enableOrImportWeaponDelay = 0.5f;

        /// <summary>
        /// Links this component with the weapon name we are enabling on pickup
        /// </summary>
        [Tooltip("Links this component with the weapon name we are enabling on pickup")]
        public string weaponLinkName;


        /// <summary>
        /// Links this component with the weapon ID we are enabling on pickup
        /// </summary>
        [Tooltip("Links this component with the weapon ID we are enabling on pickup")]
        public int weaponLinkID;


        /// <summary>
        /// The weapon object to import if not using direct link 
        /// </summary>
        [Tooltip("The weapon object to import if not using direct link ")]
        public ABC_GlobalElement GlobalElementToImport;


        /// <summary>
        /// If true then the abilities will be modified at run type to match the game type selected
        /// </summary>
        [Tooltip(" If true then the abilities will be modified at run type to match the game type selected")]
        public bool importGlobalElementEnableGameTypeModification = false;

        /// <summary>
        /// What game type to modify the global abilities to
        /// </summary>
        [Tooltip("What game type to modify the global abilities to")]
        public ABC_GlobalPortal.GameType importGlobalElementGameTypeModification = ABC_GlobalPortal.GameType.Action;


        /// <summary>
        /// If true then the weapon picked up will be equipped straight away 
        /// </summary>
        [Tooltip("If true then the weapon picked up will be equipped straight away ")]
        public bool equipWeaponOnPickup = true;


        /// <summary>
        /// Useful for some inventory integrations, will disable the weapon on pickup essentially adding the weapon to the character ready to be enabled later
        /// </summary>
        [Tooltip("Useful for some inventory integrations, will disable the weapon on pickup essentially adding the weapon to the character ready to be enabled later")]
        public bool disableWeaponOnPickup = false;

        /// <summary>
        /// If true then the entity will drop it's current weapon when this weapon is picked up
        /// </summary>
        [Tooltip("If true then the entity will drop it's current weapon when this weapon is picked up")]
        public bool dropCurrentWeaponOnPickUp = false;


        /// <summary>
        /// If true then the entity will unequip current weapon before picking up the new weapon
        /// </summary>
        [Tooltip(" If true then the entity will unequip current weapon before picking up the new weapon")]
        public bool unequipCurrentWeaponBeforePickUp = false;


        /// <summary>
        /// If true then the weapon pick up object will be disabled and pool'd on pickup
        /// </summary>
        [Tooltip("If true then the weapon pick up object will be disabled and pool'd on pickup")]
        public bool disableOnPickUp = true;

        /// <summary>
        /// If true then ammo will be updated and set to the amount specified in the pickup
        /// </summary>
        [Tooltip("If true then ammo will be updated and set to the amount specified in the pickup")]
        public bool updateAmmoOnPickup = false;

        /// <summary>
        /// Amount of ammo to set on the weapon on pickup
        /// </summary>
        [Tooltip("Amount of ammo to set on the weapon on pickup")]
        public int ammoAmount = 200;

        /// <summary>
        /// If enabled then the correct tag is needed to pick up the weapon
        /// </summary>
        [Tooltip("If enabled then the correct tag is needed to pick up the weapon")]
        public bool pickUpTagRequired = false;

        /// <summary>
        /// List of tags that can pickup the weapon
        /// </summary>
        [Tooltip("List of tags that can pickup the weapon")]
        public List<string> pickUpTags = new List<string>();

        /// <summary>
        /// List of tags that can't pickup the weapon
        /// </summary>
        [Tooltip("List of tags that can't pickup the weapon")]
        public List<string> pickUpRestrictedTags = new List<string>();

        /// <summary>
        /// If enabled then the player needs to press a button to pick up the weapon
        /// </summary>
        [Tooltip("If enabled then the player needs to press a button to pick up the weapon")]
        public bool triggerRequiredToPickUp = true;

        /// <summary>
        /// Type of input to pick up the weapon
        /// </summary>
        [Tooltip("type of input to pick up weapon")]
        public InputType pickUpInputType = InputType.Button;

        /// <summary>
        /// Button name to pick up weapon
        /// </summary>
        [Tooltip("Button name to pick up weapon")]
        public string keyButton = "Interact";

        /// <summary>
        /// Key to pick up weapon
        /// </summary>
        [Tooltip("Key to pick up weapon")]
        public KeyCode key;

        /// <summary>
        /// If true then a weapon graphic will be created to be picked up or dropped.
        /// </summary>
        [Tooltip("If true then a weapon graphic will be created to be 'picked up' ")]
        public bool usePickUpGraphic = true;

        /// <summary>
        /// New graphic to show to represent the weapon graphic
        /// </summary>
        [Tooltip("New graphic to show to represent the weapon graphic")]
        public ABC_GameObjectReference pickUpGraphic = null;


        /// <summary>
        /// enables for a collider to be added to the ability via code, allows for other functionality to be used (delay etc)
        /// </summary>
        [Tooltip("enables for a collider to be added to the ability via code, allows for other functionality to be used (delay etc)")]
        public bool addPickupCollider = true;

        /// <summary>
        /// If true then the collider will be set as a trigger
        /// </summary>
        [Tooltip("If true then the collider will be set as a trigger")]
        public bool isTrigger = true;

        /// <summary>
        /// Determines the radius size of the ability collision. Higher the number the larger range the ability will effect
        /// </summary>
        [Tooltip("Collider size of the Ability")]
        public float colliderRadius = 1.5f;

        /// <summary>
        /// Off set of the ability collider
        /// </summary>
        [Tooltip("Offset of the collider")]
        public Vector3 colliderOffset = Vector3.zero;


#if ABC_GC_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed on weapon pickup 
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed on weapon pickup")]
    public GameCreator.Core.IActionsList gcPickUpActionList;

#endif

#if ABC_GC_2_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed on initiation
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed on initiation")]
    public GameCreator.Runtime.VisualScripting.Actions gc2PickUpAction;

#endif

        /// <summary>
        /// If true then graphics and animations will activate when an ability group is enabled
        /// </summary>
        [Tooltip("Play animation when weapon is picked up or dropped")]
        public bool usePickUpAnimations = false;


        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference weaponPickUpAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference weaponPickUpAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float weaponPickUpAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float weaponPickUpAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float weaponPickUpAnimationRunnerClipDuration = 1f;


        /// <summary>
        /// If true then the weapon enable animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate weapon enable animation on the main entities animation runner")]
        public bool weaponPickUpAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the  weapon enable animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate weapon enable animation on the current scroll ability's graphic animation runner")]
        public bool weaponPickUpAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the  weapon enable animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate weapon enable animation on the current weapons animation runner")]
        public bool weaponPickUpAnimationRunnerOnWeapon = false;


        /// <summary>
        /// Name of the weapon Enable animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string weaponPickUpAnimatorParameter;

        /// <summary>
        /// Type of parameter for the  weapon Enable animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType weaponPickUpAnimatorParameterType;

        /// <summary>
        /// Value to turn on the  weapon Enable animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string weaponPickUpAnimatorOnValue;

        /// <summary>
        /// Value to turn off the  weapon Enable animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string weaponPickUpAnimatorOffValue;

        /// <summary>
        /// Duration of the ability group animation
        /// </summary>
        [Tooltip("How long to play animation for ")]
        public float weaponPickUpAnimatorDuration = 3f;

        /// <summary>
        /// If true then text can be setup to display to aid the player in picking up the weapon
        /// </summary>
        [Tooltip("If true then text can be setup to display to aid the player in picking up the weapon")]
        public bool displayPickUpText = false;

        /// <summary>
        /// The text object to modify 
        /// </summary>
        [Tooltip("The text object to modify ")]
        public ABC_TextReference displayTextObject;

        /// <summary>
        /// The text to display 
        /// </summary>
        [Tooltip("The text to display ")]
        public string textToDisplay = "Press #Key# to pick up weapon";

        /// <summary>
        /// If true then the display will only show when the pick up weapon collides
        /// </summary>
        [Tooltip("If true then the display will only show when the pick up weapon collides")]
        public bool onlyDisplayWhenColliding = true;

        /// <summary>
        /// If true then the display will only show when the pick up tags set up collide (limits when the display will show) 
        /// </summary>
        [Tooltip("If true then the display will only show when the pick up tags set up collide (limits when the display will show) ")]
        public bool onlyDisplayForPickUpTags = true;

        /// <summary>
        /// If true then the text will always turn to face camera should only be used if the text is an object in game and not used on a canvas
        /// </summary>
        [Tooltip("If true then the text will always turn to face camera should only be used if the text is an object in game and not used on a canvas ")]
        public bool alwaysFaceCamera = false;

        /// <summary>
        /// show blue guidance boxes in inspector to aid the user 
        /// </summary>
        public bool showHelpInformation = true;

        /// <summary>
        /// If true then inspector editor will have it's values update whilst unity is running in play mode (uses repaint). Will decrease performance of game running. 
        /// </summary>
        public bool updateEditorAtRunTime = false;

        /// <summary>
        ///  used for inspector, keeps track of what ability is currently chosen for the IconUI 
        /// </summary>
        [HideInInspector]
        public int abcLinkWeaponListChoice = 0;


        #endregion

        // ********************* Variables ******************

        #region Variables

        /// <summary>
        /// Determines if the weapon pick up functionality is currently in progress (so doesn't run again)
        /// </summary>
        private bool pickUpWeaponInProgress = false;


        //Track if weapon was successfully picked up
        bool weaponPickedUp = false;


        /// <summary>
        /// Transform of the weapon pickup object
        /// </summary>
        private Transform meTransform;

        /// <summary>
        /// Rigidbody of the weapon pickup object
        /// </summary>
        private Rigidbody meRigidbody;


        /// <summary>
        /// Determines who is currently colliding with the weapon pickup (less process heavy then on stay)
        /// </summary>
        private List<ABC_IEntity> collidingEntities = new List<ABC_IEntity>();

        /// <summary>
        /// Stores the weapon pickup graphic object
        /// </summary>
        private GameObject graphicPool = null;


        /// <summary>
        /// Starting name of the display text object (name gets modified during play)
        /// </summary>
        private string displayTextObjectStartName;


        #endregion



        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will setup the weapon pickup 
        /// </summary>
        public void Setup() {

            //Setup Collider
            this.SetupCollison();

            //Create weapon graphic
            this.CreateGraphic();

        }

        /// <summary>
        /// Will set the ammo to the value provided in parameter
        /// </summary>
        /// <param name="Value">Amount to set ammo too</param>
        public void SetAmmo(int Value) {

            this.ammoAmount = Value;

        }


        /// <summary>
        /// Import weapon and ability data
        /// </summary>
        /// <param name="Weapon">Weapon to Import</param>
        /// <param name="Abilities">Abilities to Import</param>
        public void ImportGlobalWeapon(ABC_GlobalElement GlobalElement) {

            this.GlobalElementToImport = GlobalElement;
            this.weaponLinkID = GlobalElement.ElementWeapon.weaponID;
            this.weaponLinkName = GlobalElement.ElementWeapon.weaponName;
            this.pickUpMode = ABC_WeaponPickUp.EnableOrImport.Import;

            //If we have weapon graphics then add that
            if (GlobalElement.ElementWeapon.weaponGraphics.Count > 0) {
                this.pickUpGraphic = GlobalElement.ElementWeapon.weaponGraphics.FirstOrDefault().weaponObjMainGraphic;
            }

        }


        #endregion



        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will setup the collider/rigidbody for the weapon pick up 
        /// </summary>
        private void SetupCollison() {

            if (this.addPickupCollider == false)
                return;

            //Setup collider, if a sphere collider doesn't exist add it
            if (this.gameObject.GetComponentsInChildren<SphereCollider>().Count() == 0) {
                SphereCollider meCol = this.gameObject.AddComponent<SphereCollider>();
                meCol.isTrigger = this.isTrigger;
                meCol.radius = this.colliderRadius;
                meCol.center = this.colliderOffset;
            } else { // else modify the one that is there
                foreach (SphereCollider meCol in gameObject.GetComponentsInChildren<SphereCollider>(true)) {
                    meCol.isTrigger = this.isTrigger;
                    meCol.radius = this.colliderRadius;
                    meCol.center = this.colliderOffset;
                }
            }

            //add rigidbody for collision detection
            if (this.gameObject.GetComponentsInChildren<Rigidbody>().Count() == 0) {
                this.meRigidbody = this.gameObject.AddComponent<Rigidbody>();

                //If trigger then don't use gravity etc
                if (this.isTrigger == true) {
                    this.meRigidbody.isKinematic = true;
                    this.meRigidbody.useGravity = false;
                } else { // else not trigger so use gravity etc
                    this.meRigidbody.isKinematic = false;
                    this.meRigidbody.useGravity = true;
                }
            }
        }

        /// <summary>
        /// Will create the graphic for the weapon pickup
        /// </summary>
        private void CreateGraphic() {

            //If not setup to use graphics end here
            if (this.usePickUpGraphic == false)
                return;

            //If graphic pool already has an object and it's different to the graphic set then disable it ready to create the new one 
            if (this.graphicPool != null && this.pickUpGraphic.GameObject != this.graphicPool) {
                this.graphicPool.SetActive(false);
                this.graphicPool = null;
            }

            if (this.pickUpGraphic.GameObject != null && this.graphicPool == null) {
                this.graphicPool = Instantiate(this.pickUpGraphic.GameObject);
            }

            this.graphicPool.transform.SetParent(this.meTransform);
            this.graphicPool.transform.position = this.meTransform.position;

        }



        /// <summary>
        /// Will add the object to the list of entities currently colliding with the weapon pick up - less process heavy then on stay
        /// </summary>
        /// <param name="Obj">Object to add to colliding entity tracker</param>
        private void AddCollidingEntity(GameObject Obj) {

            //Get the entity object
            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

            //If Obj is no longer active or does not have an ABC controller end here
            if (Obj.activeInHierarchy == false || entity.HasABCController() == false)
                return;

            //else add to list of colliding entities
            if (this.collidingEntities.Contains(entity) == false)
                this.collidingEntities.Add(entity);
        }

        /// <summary>
        /// Will remove the object from the list of entities currently colliding with the weapon pick up - less process heavy then on stay
        /// </summary>
        /// <param name="Obj">Object to add to colliding entity tracker</param>
        private void RemoveCollidingEntity(GameObject Obj) {

            //Get the entity object
            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

            //If Obj is no longer active or does not have an ABC controller end here
            if (Obj.activeInHierarchy == false || entity.HasABCController() == false)
                return;

            //else remove from list of colliding entities
            if (this.collidingEntities.Contains(entity))
                this.collidingEntities.Remove(entity);

        }

        /// <summary>
        /// Will Enable the weapon linked to this component
        /// </summary>
        /// <param name="Entity">Entity to enable weapon on</param>
        /// <param name="EquipWeapon">If true then weapon will be equipped</param>
        /// <param name="UseWeaponName">If true then weapon name will be used instead of ID</param>
        /// <returns>True if weapon was successfully picked up, else false</returns>
        private void EnableWeapon(ABC_IEntity Entity, bool EquipWeapon = true, bool UseWeaponName = false) {


            if (UseWeaponName == true && string.IsNullOrEmpty(this.weaponLinkName) || UseWeaponName == false && this.weaponLinkID <= 0)
                return;


            bool weaponSuccessfullyEnabled = false;


            //Enable weapon 
            if (UseWeaponName == false)
                weaponSuccessfullyEnabled = Entity.EnableWeapon(this.weaponLinkID, EquipWeapon);
            else
                weaponSuccessfullyEnabled = Entity.EnableWeapon(this.weaponLinkName, EquipWeapon);


            //If weapon was enabled successfully then let component know weapon was successfully picked up (to allow for it to be disabled on successful pickup etc)
            //Doesn't set if false as another entity in the list also picking up may have already been successful, pickup method starts weapon pickup as false. 
            if (weaponSuccessfullyEnabled == true)
                this.weaponPickedUp = true;

        }

        /// <summary>
        /// Will Enable the weapon linked to this component
        /// </summary>
        /// <param name="Entity">Entity to import the weapon to</param>
        /// <param name="EquipWeapon">If true then weapon will be equipped</param>
        /// <returns>True if weapon was successfully imported, else false</returns>
        private IEnumerator ImportWeapon(ABC_IEntity Entity, bool EquipWeapon = true) {

            //If entity doesn't have the ABC controller end here
            if (Entity.HasABCController() == false)
                yield break;

            //If weapon no longer exists then end here
            if (this.GlobalElementToImport == null)
                yield break;


            //Do the bulk import (Equipping weapon if set too)
            yield return StartCoroutine(Entity.AddGlobalElementAtRunTime(this.GlobalElementToImport, EquipWeapon, this.importGlobalElementEnableGameTypeModification, importGlobalElementGameTypeModification));


            //If this far then import was successfull
            //If weapon was imported successfully then let component know weapon was successfully picked up (to allow for it to be disabled on successful pickup etc)
            //Doesn't set if false as another entity in the list also picking up may have already been successful, pickup method starts weapon pickup as false. 
            this.weaponPickedUp = true;

        }


        /// <summary>
        /// Will pick up the weapon, playing animations, adding/enabling the weapon etc
        /// </summary>
        private IEnumerator PickUpWeapon() {

            //If this function is already in progress end here
            if (this.pickUpWeaponInProgress == true)
                yield break;

            //Tell the component that pick up weapon is currently running
            this.pickUpWeaponInProgress = true;

            //Track if weapon was successfully picked up  
            this.weaponPickedUp = false;


            //Add weapon to those in range
            foreach (ABC_IEntity Entity in this.collidingEntities.ToList()) {

                //If set to only pick up weapon on trigger and trigger has not been pressed then end here
                if (this.triggerRequiredToPickUp == true && this.ButtonPressed(Entity, WeaponPickUpState.PickUpWeapon) == false)
                    continue;

                //If entity doesn't have the ABC controller then end here
                if (Entity.HasABCController() == false)
                    continue;

                //Check its right person (tags etc)
                if (this.pickUpTagRequired == true && ABC_Utilities.ObjectHasTag(Entity.gameObject, this.pickUpTags) == false)
                    continue;

                //Check the person hasn't got a restricted tag 
                if (this.pickUpTagRequired == true && ABC_Utilities.ObjectHasTag(Entity.gameObject, this.pickUpRestrictedTags) == true)
                    continue;

                //If dropping weapon
                if (this.dropCurrentWeaponOnPickUp == true) {
                    yield return StartCoroutine(Entity.DropCurrentWeapon(true));

                } else {

                    //if set to unequip current weapon then return to idle mode (if not dropping weapon)
                    if (this.unequipCurrentWeaponBeforePickUp == true)
                        yield return StartCoroutine(Entity.UnEquipCurrentEquippedWeapon());

                    if (this.equipWeaponOnPickup == true && this.unequipCurrentWeaponBeforePickUp == false)
                        yield return StartCoroutine(Entity.UnEquipCurrentEquippedWeapon(true));

                }

                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated the same call
                //this function would not continue after duration
                float functionRequestTime = Time.time;


                //run animation 
                if (this.usePickUpAnimations) {

                    //Turn of IK whilst animation plays
                    StartCoroutine(Entity.ToggleIK(functionRequestTime, false));

                    //Start animation runner
                    if (this.weaponPickUpAnimationRunnerClip != null)
                        this.StartAndStopAnimationRunner(WeaponPickUpState.PickUpWeapon, Entity.animationRunner);

                    // start animator
                    if (this.weaponPickUpAnimatorParameter != "")
                        StartCoroutine(this.StartAndStopAnimation(WeaponPickUpState.PickUpWeapon, Entity.animator));
                }


                //Ensure weapon drop duration is atleast 0.1 or bugs occur
                if (this.enableOrImportWeaponDelay == 0)
                    this.enableOrImportWeaponDelay = 0.1f;

                //wait for delay (lets animation play out)
                yield return new WaitForSeconds(this.enableOrImportWeaponDelay);


                //Enable IK once more now weapon has been equipped
                StartCoroutine(Entity.ToggleIK(functionRequestTime, true));

                //Enable the weapon and determine if it was picked up successfully
                if (this.pickUpMode == EnableOrImport.Enable)
                    this.EnableWeapon(Entity, this.equipWeaponOnPickup, this.enableWeaponUsingName);

                //else we want to import the weapon from the global portal
                if (this.pickUpMode == EnableOrImport.Import)
                    yield return StartCoroutine(this.ImportWeapon(Entity, this.equipWeaponOnPickup));


                //modify the weapons ammo if set to update
                if (this.updateAmmoOnPickup)
                    Entity.SetWeaponAmmo(this.weaponLinkID, this.ammoAmount);


#if ABC_GC_Integration
            //Play GC action 
            if (this.gcPickUpActionList != null)
                this.gcPickUpActionList?.Execute(Entity.gameObject, null);
#endif

#if ABC_GC_2_Integration

            if (this.gc2PickUpAction != null)
                Entity.RunGC2Action(this.gc2PickUpAction, new GameCreator.Runtime.Common.Args(Entity.gameObject, Entity.gameObject));

#endif

                //remove the entity for current collision 
                this.collidingEntities.Remove(Entity);


            }

            //Disable weapon if successfully picked up 
            if (this.disableOnPickUp && this.weaponPickedUp == true)
                ABC_Utilities.PoolObject(this.gameObject);


            //Tell the component that pick up weapon has finished running
            this.pickUpWeaponInProgress = false;

        }


        /// <summary>
        /// Will handle the pickup event ensuring the right conditions are met to pick up the weapon (button press etc)
        /// </summary>
        private void PickUpHandler() {

            //If no one is currently colliding with the weapon pickup then return here
            if (this.collidingEntities.Count == 0)
                return;

            //If we made it this far then we can pickup weapon
            StartCoroutine(this.PickUpWeapon());

        }

        /// <summary>
        /// Will handle the UI showing the pickup text depending on conditions (only display when colliding etc) 
        /// </summary>
        private void UIHandler() {

            //No text object setup so we can end here
            if (this.displayTextObject == null)
                return;

            //Determine if we should display UI or not
            bool displayUI = true;

            //If not set to display text then set to disable UI 
            if (this.displayPickUpText == false)
                displayUI = false;


            // If we are only displaying when colliding and there is either: 
            // No colliding entities 
            // or 
            // there is colliding entities but none match the tags required to pickup and display 
            // then set to disable UI 
            if (this.onlyDisplayWhenColliding == true && (this.collidingEntities.Count == 0) || this.collidingEntities.Count > 0 && this.onlyDisplayForPickUpTags == true && this.pickUpTagRequired == true && (this.collidingEntities.All(ce => ABC_Utilities.ObjectHasTag(ce.gameObject, this.pickUpTags) == false) || this.collidingEntities.All(ce => ABC_Utilities.ObjectHasTag(ce.gameObject, this.pickUpRestrictedTags) == true)))
                displayUI = false;


            //If display UI stays true then assign the text and set active
            if (displayUI == true) {

                //If we haven't updated the name (Marked the text object as being updated by this gameobject component) then add instance ID to the name
                if (this.displayTextObject.Graphic.gameObject.name.Contains("*_ABC" + this.gameObject.GetInstanceID().ToString()) == false) {
                    this.displayTextObject.Graphic.gameObject.name = this.displayTextObjectStartName;
                    this.displayTextObject.Graphic.gameObject.name = "*_ABC" + this.gameObject.GetInstanceID().ToString() + this.displayTextObjectStartName;
                }

                //update text (replacing key with the button/key to press)
                this.displayTextObject.text = this.textToDisplay.Replace("#Key#", this.pickUpInputType == InputType.Button ? this.keyButton : this.key.ToString());

                if (this.displayTextObject.Graphic.gameObject.activeInHierarchy == false)
                    this.displayTextObject.Graphic.gameObject.SetActive(true);

                //turn to face camera if set too (should only be used if in game and not on canvas)
                if (this.alwaysFaceCamera)
                    this.displayTextObject.Graphic.transform.LookAt(this.displayTextObject.Graphic.transform.position + Camera.main.transform.rotation * Vector3.back, Camera.main.transform.rotation * Vector3.up);


            } else { // else disable the object if not being updated by something else (I.e the name of the text currently has our instance ID)

                if (this.displayTextObject.Graphic.gameObject.name.Contains("*_ABC" + this.gameObject.GetInstanceID().ToString())) {



                    if (this.displayTextObject.Graphic.gameObject.activeInHierarchy == true)
                        this.displayTextObject.Graphic.gameObject.SetActive(false);
                }
            }





        }

        /// <summary>
        /// Handler for button triggers - returns an bool indicating if the button relating to the state provided has been triggered. 
        /// </summary>
        /// <remarks>
        /// If the state in the parameter is pick up weapon then this function will return true if the weapon pickup button has been triggered
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="State">Which state and button type to check for: pickup etcy</param>
        /// <returns>True if the button relating to the state has been correctly press or held, else false</returns>
        private bool ButtonPressed(ABC_IEntity Originator, WeaponPickUpState State) {

            InputType inputType = InputType.Button;
            KeyCode key = KeyCode.None;
            string button = "";

            // determine the right configuration depending on the type provided
            switch (State) {
                case WeaponPickUpState.PickUpWeapon:

                    inputType = this.pickUpInputType;
                    key = this.key;
                    button = this.keyButton;
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
        /// Starts an animation clip using the ABC animation runner stopping it after the duration
        /// </summary>
        /// <param name="State">The animation to play - Weapon PickUP etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        private void StartAndStopAnimationRunner(WeaponPickUpState State, ABC_AnimationsRunner AnimationRunner) {

            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipSpeed = 1f;
            float animationClipDelay = 0f;
            float animationDuration = 1f;
            AvatarMask animationClipMask = null;


            switch (State) {
                case WeaponPickUpState.PickUpWeapon:

                    animationClip = this.weaponPickUpAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.weaponPickUpAnimationRunnerClipSpeed;
                    animationClipDelay = this.weaponPickUpAnimationRunnerClipDelay;
                    animationDuration = this.weaponPickUpAnimationRunnerClipDuration;
                    animationClipMask = this.weaponPickUpAnimationRunnerMask.AvatarMask;


                    break;

            }


            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null)
                return;


            AnimationRunner.PlayAnimation(animationClip, animationClipDelay, animationClipSpeed, animationDuration, animationClipMask, true);


        }


        /// <summary>
        /// Starts an animation for the entity depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to play - weapon PickUp etc</param>
        /// <param name="Animator">Animator component</param>
        private IEnumerator StartAndStopAnimation(WeaponPickUpState State, Animator Animator) {


            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOnValue = "";
            string animatorOffValue = "";
            float animatorDuration = 1f;



            switch (State) {
                case WeaponPickUpState.PickUpWeapon:

                    animatorParameterType = this.weaponPickUpAnimatorParameterType;
                    animatorParameter = this.weaponPickUpAnimatorParameter;
                    animatorOnValue = this.weaponPickUpAnimatorOnValue;
                    animatorDuration = this.weaponPickUpAnimatorDuration;

                    break;
            }

            // if animator parameter is null or animator is not given then animation can't start so end here. 
            if (animatorParameter == "" || Animator == null) {
                yield break;
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


            //wait for delay then end animation

            if (animatorDuration > 0)
                yield return new WaitForSeconds(animatorDuration);


            switch (State) {
                case WeaponPickUpState.PickUpWeapon:

                    animatorParameterType = this.weaponPickUpAnimatorParameterType;
                    animatorParameter = this.weaponPickUpAnimatorParameter;
                    animatorOffValue = this.weaponPickUpAnimatorOffValue;

                    break;
            }


            // if animator parameter is null or animator is not given then animation can't start so end here. 
            if (animatorParameter == "" || Animator == null) {
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

        }

        #endregion



        // ********************* Game ********************

        #region Game


        void OnCollisionEnter(Collision col) {

            //Add to colliding entity tracker to keep track of who is in range
            this.AddCollidingEntity(col.gameObject);
        }


        void OnTriggerEnter(Collider col) {

            //Add to colliding entity tracker to keep track of who is in range
            this.AddCollidingEntity(col.gameObject);

        }

        void OnCollisionExit(Collision col) {

            //Remove from colliding entity tracker as object is no longer in range
            this.RemoveCollidingEntity(col.gameObject);

        }


        void OnTriggerExit(Collider col) {

            //Remove from colliding entity tracker as object is no longer in range
            this.RemoveCollidingEntity(col.gameObject);

        }


        void Awake() {

            this.meTransform = this.GetComponent<Transform>();
            //this.meRigidbody = this.GetComponent<Rigidbody>();


            if (this.displayTextObject.text != null)
                displayTextObjectStartName = this.displayTextObject.Graphic.name;

            this.Setup();

        }

        // Update is called once per frame
        void Update() {

            if (this.gameObject.activeInHierarchy == false)
                return;

            this.PickUpHandler();
            this.UIHandler();
        }

        void FixedUpdate() {

            //If pickup is sleeping then wake it up (for collision)
            if (this.meRigidbody != null && this.meRigidbody.IsSleeping())
                this.meRigidbody.WakeUp();

        }

        void OnDisable() {

            //rerun UI handler to make sure the text object is not still claimed
            UIHandler();
        }


        #endregion

        // ********* ENUMS for WeaponPickup **********************


        #region WeaponPickup ENUMS

        private enum WeaponPickUpState {
            PickUpWeapon

        }

        public enum EnableOrImport {
            Enable = 0,
            Import = 1
        }



        #endregion
    }
}