using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ABCToolkit {
    /// <summary>
    /// Component Script for the Ability Projectile object. This script will setup the colliders and handles collisions. 
    /// It also holds Ability information used by other scripts and performs other Ability related functions like reducing mana whilst active. 
    /// </summary>
    /// <remarks>
    /// Script will setup colliders and handle what to do with all collisions. It also does other miscellaneous functions relating to the Ability including splash collisions, reducing mana and applying gravity after a duration. 
    /// The Component holds all the Ability information in a property which can be accessed by other objects involved in collisions. An example of this is when the ABC StateManager Component on collision will access 
    /// the Ability property from this script to retrieve all the Ability effects. 
    /// 
    /// This script should only ever be added and configured by the Ability class. This is why a lot of the settings are not shown and are just taken from the ability property directly. 
    /// </remarks>
    public class ABC_Projectile : MonoBehaviour {


        // ********************* Settings ********************

        #region Settings

        /// <summary>
        /// Property to hold the Ability object which has all the information regarding the ability including configured settings.
        /// </summary>
        public ABC_Ability ability = null;

        /// <summary>
        /// The entity which has activated the ability also known as the Originator throughout ABC. 
        /// </summary>
        public ABC_IEntity originator;

        // Never take this from the Ability property. Keep this seperate and local to this script only.
        /// <summary>
        /// If one exists then the target of the Ability Projectile. Not always used it depends on the current travel type. 
        /// </summary>
        public GameObject targetObj = null;

        /// <summary>
        /// Records the time the projectile was activated. 
        /// </summary>
        public float timeActivated;

        /// <summary>
        /// Keeps track of all surrounding objects linked to this projectile. SurroundingObjects are GameObjects which have temporarily became apart of the Ability Projectile. Commonly used for telekinetic like effects. 
        /// </summary>
        public List<GameObject> surroundingObjects;

        /// <summary>
        /// Float that determines what base initiation speed adjustment the ability currently has
        /// </summary>
        public float initiationBaseSpeedAdjustment;


        /// <summary>
        /// Float that determines what initiation speed adjustment the ability currently has
        /// </summary>
        public float initiationGlobalSpeedAdjustment;

        /// <summary>
        /// Holds all Colliders of the projectile (Including SurroundingObjects). 
        /// </summary>
        public Collider[] meColliders;




        #endregion


        // ********************* Variables ******************

        #region Variables

        /// <summary>
        /// Determines who is currently colliding with the projectile (less process heavy then on stay)
        /// </summary>
        private List<ABC_IEntity> collidingEntities = new List<ABC_IEntity>();

        Transform meTransform;
        Rigidbody meRigidbody;

        /// <summary>
        /// Holds the Collider of the Projectile
        /// </summary>
        private Collider meCollider;

        /// <summary>
        /// Counts how many times the Projectile has bounced. Once the bouncer counter reaches a limit the projectile will stop bouncing. 
        /// </summary>
        private int bounceCounter = 0;

        /// <summary>
        /// Will track when colliders are setup and ready to start collision
        /// </summary>
        private bool colliderAdded = false;

        /// <summary>
        /// Will track if collision is enabled
        /// </summary>
        private bool collisionEnabled = true;

        /// <summary>
        /// Will track what time On Stay Collision last occured
        /// </summary>
        private float lastRecordedOnStayCollision = 0f;

        /// <summary>
        /// Will track when the projectile is currently in splash mode
        /// </summary>
        private bool splashColliding = false;

        /// <summary>
        /// How long ability prepared for (can modify effects)
        /// </summary>
        private float abilitySecondsPrepared = 0f;


        #endregion

        // *********************  Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Destroys the Ability Projectile.
        /// </summary>
        public void Destroy() {

            StartCoroutine(DestroyProjectile());
        }

        /// <summary>
        /// Will disable the Ability Projectile playing only the end effect. No other destroy functionality will run (splash collision etc).
        /// </summary>
        /// <remarks>
        /// This function is useful if you want to destroy the projectile but do nothing else. This is currently used when an Ability projectile is created but cancelled before it's fully activated.
        /// </remarks>
        public void SoftDestroy() {

            if (gameObject.activeInHierarchy == false)
                return;

            // get all colliders - if the global variable is empty then we will set it now (may be empty due to proj script not activating yet - happens in proj to start position)
            this.GetAllColliders();


            this.DisableProjectile();

        }


        /// <summary>
        /// Function will check if projectile is allowed to collide with the object provided, If it is then it will initiate the main collidehandler function. This is called by every OnCollision method (OnEnter, OnStay etc).
        /// If the projectile can collide with the object then it will run the CollideHandler function which handles what happens once the projectile collides.
        /// </summary>
        /// <param name="CollidedObj">The collided object</param>
        /// <param name="Type">Type of Collision</param> 
        /// <param name="HitPoint">Collision contact point</param> 
        public void ActivateCollision(GameObject CollidedObj, CollisionType Type, Vector3 HitPoint = default(Vector3)) {

            // if the projectile script is not active then the collider manager should not be activated
            if (this.enabled == false)
                return;

            originator.AddToDiagnosticLog(this.ability.name + " Collided with: " + CollidedObj.name);

            if (this.CorrectCollision(CollidedObj, Type) == false)
                return;

            originator.AddToDiagnosticLog(CollidedObj.name + " collision validated. Object meets criteria for collision with " + this.ability.name);

            //Determines if ability should be destroyed
            bool destroyAbility = false;


            // manages what happens with destory on impact 
            switch (ability.impactDestroy) {
                case ImpactDestroy.DestroyOnAll:

                    this.CollideHandler(CollidedObj, HitPoint);

                    break;

                case ImpactDestroy.DestroyOnABCProjectiles:
                    // if we just collided with another ABC Projectile 

                    // no bounce management as it doesn't start bouncing on projectiles
                    if (this.FindProjectileObject(CollidedObj) != null)
                        destroyAbility = true;


                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility, false);



                    break;
                case ImpactDestroy.DestroyOnAllNotABCProjectile:
                    // if we just collided with anything that isn't a projectile

                    // no bounce management as it doesn't start bouncing on projectiles
                    if (this.FindProjectileObject(CollidedObj) == null)
                        destroyAbility = true;

                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility, false);

                    break;
                case ImpactDestroy.DestroyOnABCStateManagers:
                    // if we just collided with an object that is using the StateManager script 
                    if (ABC_Utilities.GetStaticABCEntity(CollidedObj.gameObject).HasABCStateManager())
                        destroyAbility = true;


                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility);
                    break;
                case ImpactDestroy.DestroyOnAllABC:
                    //  if collided with anything to do with ABC
                    if (this.FindProjectileObject(CollidedObj) != null || ABC_Utilities.GetStaticABCEntity(CollidedObj.gameObject).HasABCStateManager())
                        destroyAbility = true;

                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility);

                    break;
                case ImpactDestroy.DestroyOnAllNotABC:
                    //  if collided with something which has nothing to do with ABC
                    if (this.FindProjectileObject(CollidedObj) == null && ABC_Utilities.GetStaticABCEntity(CollidedObj.gameObject).HasABCStateManager() == false)
                        destroyAbility = true;

                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility);

                    break;
                case ImpactDestroy.DestroyOnTargetOnly:
                    // If we collided with the target
                    if (CollidedObj == targetObj)
                        destroyAbility = true;


                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility);

                    break;

                case ImpactDestroy.DestroyOnTerrainOnly:

                    //If we collided with Terrain
                    if (Terrain.activeTerrain != null && CollidedObj.gameObject == Terrain.activeTerrain.gameObject)
                        destroyAbility = true;

                    this.CollideHandler(CollidedObj, HitPoint, destroyAbility);


                    break;
                case ImpactDestroy.DontDestroy:

                    // Don't do anything if triggering with another projectile (if you want to do this then choose destroy on projectile)
                    // Run everything as normal (including bounce) except for destroying
                    if (this.FindProjectileObject(CollidedObj) == null)
                        this.CollideHandler(CollidedObj, HitPoint, false);


                    // ****** if you want to run splash effects uncomment below (This only happens when projectile is disabled which will only happen when duration is up in this scenerio (as we don't ever destroy on collision) ******
                    //******  Splash effects are normally only run when the ability is disabled to simulate an explosion effect etc ****** 
                    // StartCoroutine(SplashMe(CollidedObj));

                    break;
            }



        }



        #endregion


        // *********************  Private Methods ********************

        #region Private Methods


        /// <summary>
        /// Will add the object to the list of entities currently colliding with the projectile - used for 'OnStay' in a less process heavy way
        /// </summary>
        /// <param name="Obj">Object to add to colliding entity tracker</param>
        private void AddCollidingEntity(GameObject Obj) {

            //Get the entity object
            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

            //If Obj is no longer active or does not have an ABC controller end here
            if (Obj.activeInHierarchy == false)
                return;

            //else add to list of colliding entities
            if (this.collidingEntities.Contains(entity) == false)
                this.collidingEntities.Add(entity);
        }

        /// <summary>
        /// Will remove the object from the list of entities currently colliding with the projectile - used for 'OnStay' in a less process heavy way    
        /// </summary>
        /// <param name="Obj">Object to add to colliding entity tracker</param>
        private void RemoveCollidingEntity(GameObject Obj) {

            //Get the entity object
            ABC_IEntity entity = ABC_Utilities.GetStaticABCEntity(Obj);

            //If Obj is no longer active or does not have an ABC controller end here
            if (Obj.activeInHierarchy == false)
                return;

            //else remove from list of colliding entities
            if (this.collidingEntities.Contains(entity))
                this.collidingEntities.Remove(entity);

        }

        /// <summary>
        /// Will scale up the provided Objects hiearchy returning the first Projectile Object found.
        /// </summary>
        /// <param name="Obj">Projectile will be searched for in the Object provided in this parameter</param>
        /// <returns>First Projectile Object found</returns>
        private GameObject FindProjectileObject(GameObject Obj) {

            if (Obj.name.Contains("ABC*_") && Obj.GetComponent<ABC_Projectile>() != null)
                return Obj;



            if (Obj.transform.parent != null && Obj.transform.parent.name.Contains("ABC*_") && Obj.transform.parent.GetComponent<ABC_Projectile>() != null)
                return Obj.transform.parent.gameObject;


            if (Obj.transform.root != null && Obj.transform.root.name.Contains("ABC*_") && Obj.transform.GetComponent<ABC_Projectile>() != null)
                return Obj.transform.root.gameObject;


            return null;


        }

        /// <summary>
        /// Function will retrieve all the colliders to do with the Projectile, including children and surroundingObjects and sets them into the MeColliders and MeCollider variables to be used by the rest of the script.
        /// </summary>
        private void GetAllColliders() {


            // Get all current colliders
            List<Collider> colliders = gameObject.GetComponentsInChildren<Collider>(true).ToList();


            // double check we have collected all colliders for our surrounding objects 
            if (this.surroundingObjects != null) {
                foreach (Collider item in this.surroundingObjects.SelectMany(c => c.gameObject.GetComponentsInChildren<Collider>(true))) {
                    if (colliders.Contains(item) == false)
                        colliders.Add(item);
                }
            }

            // assign colliders to global variable for use in rest of the projectile 
            meColliders = colliders.ToArray();

            // assign main collider to global variable for use in rest of the projectile
            meCollider = GetComponent<Collider>();

            // if no collider was found on parent where this script is then we will fill meCollider with one random child collider (for splash collision)
            if (meCollider == null & meColliders.Length > 0)
                meCollider = meColliders[0];


        }

        /// <summary>
        /// Function to setup Colliders. Applies settings, enables and disables colliders and handles collision ignores. 
        /// </summary>
        private void ColliderSetup() {

            //If we adding ability collider programatically then apply all the settings
            if (ability.addAbilityCollider == true) {
                // setup radius for colliders
                foreach (SphereCollider meCol in gameObject.GetComponentsInChildren<SphereCollider>(true)) {

                    // We don't want to change the radius of any surrounding Objects so continue to next if we find one
                    if (meCol.transform.name.Contains("*_ABCSurroundingObject"))
                        continue;


                    // continue to next iteration if the object is a parent and we are not applying settings to parents or the object is a child and we are not applying settings to the children
                    if (meCol.gameObject == gameObject && ability.applyColliderSettingsToParent == false || meCol.gameObject != gameObject && ability.applyColliderSettingsToChildren == false)
                        continue;


                    // set collider settings 
                    meCol.center = ability.colliderOffset;


                    // if a renderer exist and we are using that for radius then get the magnitude
                    if (ability.useGraphicRadius == true && meTransform.GetComponent<Renderer>() != null) {
                        meCol.radius = meTransform.GetComponent<Renderer>().bounds.extents.magnitude;
                    } else {
                        meCol.radius = ability.colliderRadius;
                    }


                }


                // determine if the collider is enabled or not before we apply more settings (So settings don't get reset half way through loop on disable/enable)
                foreach (Collider meCol in meColliders) {

                    // We don't want to change any settings for the surrounding Objects so continue to next if we find one
                    if (meCol.transform.name.Contains("*_ABCSurroundingObject"))
                        continue;


                    // If the collider is from the projectile object and apply collider to parent is true or  the collider is a child of the projectile and apply collider to children is true
                    if (meCol.gameObject == gameObject && ability.applyColliderSettingsToParent == true || meCol.gameObject != gameObject && ability.applyColliderSettingsToChildren == true) {

                        meCol.enabled = true;
                        meCol.isTrigger = ability.useColliderTrigger;
                    } else {
                        meCol.enabled = false;
                    }

                }
            } // add collider end


            //Get all colliders from the originator
            List<Collider> originatorColliders = new List<Collider>();

            // get all colliders for ABC StateManager first
            if (originator.HasABCStateManager())
                originatorColliders = originator.GetABCStateManagerObject().GetComponentsInChildren<Collider>(true).ToList();
            else if (originator.HasABCController()) // if no statemanager then get colliders from ABC controller
                originatorColliders = originator.GetABCControllerObject().GetComponentsInChildren<Collider>(true).ToList();

            // determine what colliders we are ignoring
            foreach (Collider meCol in meColliders.Where(c => c.enabled == true)) {

                // if any children has a collider then we want to ignore child objects colliding with each other or the parent (So projectile doesn't boom from the inside)
                meColliders.Where(c => meCol != c && c.enabled == true).ToList().ForEach(c => Physics.IgnoreCollision(meCol, c));


                //  if were not travelling to originator (projectile always going out) then we don't want to collide with the originator (unless affect origin object setting is true)
                if (ability.travelType != TravelType.Self && ability.affectOriginObject != true)
                    originatorColliders.Where(c => c.enabled == true).ToList().ForEach(c => Physics.IgnoreCollision(meCol, c));


                // shall we ignore active terrain?
                if (Terrain.activeTerrain != null && ability.ignoreActiveTerrain == true && ability.impactDestroy != ImpactDestroy.DestroyOnTerrainOnly)
                    Physics.IgnoreCollision(meCol, Terrain.activeTerrain.GetComponent<Collider>());

            }


            // determine what layer we are on 
            if (ability.chooseLayer == true) {

                gameObject.layer = LayerMask.NameToLayer(ability.abLayer);

                // loop through all and apply layer
                foreach (Transform obj in meTransform) {
                    obj.gameObject.layer = LayerMask.NameToLayer(ability.abLayer);
                }
            }


            // let the rest of the projectile know the collider has been added
            this.colliderAdded = true;

        }

        /// <summary>
        /// Will check if the trigger has been pressed to enable ability collisions. If the correct trigger has been pressed the collision enabled flag will be set to true allowing all collisions to occur
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private void EnableCollisionTriggerHandler(ABC_IEntity Originator) {

            //If collision has already been enabled then end here
            if (this.collisionEnabled)
                return;



            //Check with ability if enable collision trigger has been pressed, if so then enable this object/projectiles collision
            if (ability.EnableCollisionTriggerPressed(Originator)) {
                this.collisionEnabled = true;
                this.meRigidbody.WakeUp(); //wake up to projectile if object is already asleep
            }
        }

        /// <summary>
        /// Applies gravity to projectile
        /// </summary>
        private void ApplyGravity() {
            if (GetComponent<Rigidbody>() != null)
                GetComponent<Rigidbody>().useGravity = true;
        }

        /// <summary>
        /// Will disable gravity on the projectile
        /// </summary>
        private void DisableGravity() {

            //Stop the invoke 
            CancelInvoke("ApplyGravity");

            //Disable gravity if already set to true
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb != null && rb.useGravity == true)
                rb.useGravity = false;


        }


        /// <summary>
        /// Reduces Mana whilst the Projectile is Active.
        /// </summary>
        private void ReduceManaWhilstActive() {

            // if enabled reduce mana from caster whilst active otherwise end here
            if (ability.reduceManaWhenActive == false || originator == null)
                return;

            // reduce originator mana
            originator.AdjustMana(-ability.manaCost);

            //Reduce any stat cost
            if (ability.statCost > 0)
                originator.AdjustStatValue(ability.statCostName, -ability.statCost, ability.statCostIntegrationType, GCStatType.Attribute);


            // if mana is zero then destroy this ability
            if (originator.manaValue <= 0 || ability.statCost > 0 && originator.GetStatValue(ability.statCostName, ability.statCostIntegrationType, GCStatType.Attribute) <= 0)
                StartCoroutine(DestroyProjectile(ability.destroyDelay));


        }


        /// <summary>
        /// Reduces Ammo whilst the projectile is Active. 
        /// </summary>
        private void ReduceAmmoWhilstActive() {

            // if enabled reduce mana from caster whilst active otherwise end here
            if (ability.reduceAmmoWhilstActive == false || ability.UseAmmo == false)
                return;

            //reduce ammo
            if (ability.useEquippedWeaponAmmo)
                originator.AdjustEquippedWeaponAmmo(-1);
            else
                ability.AdjustAmmo(-1, originator);


            // if ammo is zero then destroy this ability
            if (ability.useEquippedWeaponAmmo == false && ability.GetAmmoCount() <= 0 || ability.useEquippedWeaponAmmo == true && originator.EquippedWeaponHasAmmo() == false)
                StartCoroutine(DestroyProjectile(ability.destroyDelay));

        }


        /// <summary>
        /// Determines if this Ability Projectile is allowed to start bouncing, depending on if bounce is enabled and bouncing is allowed to start from the object that the projectile collided with. 
        /// </summary>
        /// <param name="CollidedObj">The object the projectile has just collided with</param>
        /// <returns>True if ability can start bouncing, else false</returns>
        private bool CanBounce(GameObject CollidedObj) {
            if (ability.bounceMode == true && this.bounceCounter < ability.bounceAmount && ability.abilityType != AbilityType.Melee && (ability.startBounceTagRequired == false || ability.startBounceTagRequired == true && ABC_Utilities.ObjectHasTag(CollidedObj, ABC_Utilities.ConvertTags(this.originator, ability.startBounceRequiredTags))))
                return true;
            else
                return false;

        }


        /// <summary>
        /// Function which handles the main logic for Bounce mode. Will determine the next target to bounce on and travel towards next. 
        /// </summary>
        /// <param name="CollidedObj">The object the projectile has just collided/bounced on.</param>
        private IEnumerator BounceHandler(GameObject CollidedObj, Vector3 HitPoint = default(Vector3)) {

            // If bouncemode is off or we have reached our bounce limit then stop method here 
            if (ability.bounceMode == false || this.bounceCounter > ability.bounceAmount)
                yield break;

            // get everything around us within bounce range
            Collider[] col = Physics.OverlapSphere(meTransform.position, ability.bounceRange);


            // Shuffle the array if the option has been picke
            if (ability.enableRandomBounce == true) {
                for (int t = 0; t < col.Length; t++) {
                    var tmp = col[t];
                    int r = Random.Range(t, col.Length);
                    col[t] = col[r];
                    col[r] = tmp;
                }
            }


            // reset the new bounce target so we know if one is found
            GameObject newBounceTarget = null;



            foreach (Collider item in col) {

                //If another projectile then continue 
                if (item.name.Contains("ABC*_"))
                    continue;

                // if were not bouncing on caster and the item is caster then just skip to next
                if (ability.bounceOnCaster == false && originator.OriginatorLinkedToGameObject(item.gameObject) == true)
                    continue;

                // Make sure we are not trying to bounce on ourselves (as we just got bounced on)
                if (item.gameObject == CollidedObj)
                    continue;

                // what we targeting with bounce. 
                switch (ability.bounceTarget) {
                    case BounceTarget.NearestABCStateManager:

                        // if it has a Statemanager 
                        if (item.gameObject.GetComponent<ABC_StateManager>() != null)
                            newBounceTarget = item.gameObject;


                        break;
                    case BounceTarget.NearestObject:
                        // just hit everything around unless its an ABC projectile or terrain

                        if (item.gameObject != Terrain.activeTerrain.gameObject && this.FindProjectileObject(item.gameObject) == null)
                            newBounceTarget = item.transform.gameObject;

                        break;

                    case BounceTarget.NearestTag:
                        // only target the nearest tags 

                        if (ABC_Utilities.ObjectHasTag(item.gameObject, ABC_Utilities.ConvertTags(this.originator, ability.bounceTag)))
                            newBounceTarget = item.transform.gameObject;


                        break;

                }

                // if we found the next target then we no longer need the loop so break it
                if (newBounceTarget != null)
                    break;

            }

            // if we found a new target then
            if (newBounceTarget != null) {

                // update the target scripts with new target
                UpdateMeTarget(newBounceTarget, true, true, ability.bouncePositionOffset, ability.bouncePositionForwardOffset, ability.bouncePositionRightOffset);

                // increment the bounce counter
                bounceCounter += 1;

                // wait a tiny second for to update our own Proj target Obj (As statemanager uses it to know if to run effects or  not)
                yield return new WaitForSeconds(0.2f);

                // update our target object for statemanager script 
                targetObj = newBounceTarget;

            } else {
                // no new bounce target found so lets stop bouncing by making the counter reach the max
                this.bounceCounter = ability.bounceAmount;
                // since this was the collision of the last object we need to run the manager again for this object so we can do the right destroy 
                this.ActivateCollision(CollidedObj, CollisionType.None, HitPoint);
            }

        }



        /// <summary>
        /// Updates the Projectile's target. 
        /// </summary>
        /// <remarks>
        /// Used by bounce functionality when travelling towards different targets. Function will get the Projectile Travel script used by all projectiles and call the API function to change the target. 
        /// No matter what travel type the projectile is on this method will switch it to Selected Target so the projectile will move towards the new target.
        /// </remarks>
        /// <param name="NewTarget">New target object that the projectile will start travelling too</param>
        /// <param name="ContinuouslyTurnToDestination">If true then the projectile will continously turn to the new target</param>
        /// <param name="ModifyTargetPositionOffset">If true then the target position offsets provided will be applied</param>
        /// <param name="TargetPositionOffset">offset of the new target</param>
        /// <param name="TargetPositionForwardOffset">forward offset of the new target</param>
        /// <param name="TargetPositionRightOffset">right offset of the new target</param>
        private void UpdateMeTarget(GameObject NewTarget, bool ContinuouslyTurnToDestination = false, bool ModifyTargetPositionOffset = false, Vector3 TargetPositionOffset = default(Vector3), float TargetPositionForwardOffset = 0, float TargetPositionRightOffset = 0) {

            // update travel script with new target 

            switch (ability.travelType) {
                case TravelType.CustomScript:
                    //Can't update target with custom script
                    break;

                default:
                    ABC_ProjectileTravel projTravel = gameObject.GetComponent<ABC_ProjectileTravel>();

                    if (projTravel == null)
                        return;

                    //If set to modify target position offset then update script values
                    if (ModifyTargetPositionOffset) {
                        projTravel.targetOffset = TargetPositionOffset;
                        projTravel.targetForwardOffset = TargetPositionForwardOffset;
                        projTravel.targetRightOffset = TargetPositionRightOffset;
                    }

                    projTravel.GoToTarget(NewTarget, ContinuouslyTurnToDestination);
                    break;

            }

        }

        /// <summary>
        /// Will enable or disable projectile travelling
        /// </summary>
        /// <param name="Enabled">True to enable projectile travelling, else false</param>
        private void ToggleProjectileTravel(bool Enabled) {

            // enable or disable depending on travel type
            switch (ability.travelType) {
                case TravelType.CustomScript:
                    //Can't update target with custom script
                    break;

                default:
                    ABC_ProjectileTravel projTravel = gameObject.GetComponent<ABC_ProjectileTravel>();

                    if (projTravel == null)
                        return;

                    if (Enabled == true)
                        projTravel.EnableTravel();
                    else
                        projTravel.DisableTravel(true);


                    break;

            }

        }


        /// <summary>
        /// Determines if the object provided is another Projectile we should be ignoring
        /// </summary>
        /// <param name="collidedObj">The object which the function will determine if it's another projectile which should be ignored or not.</param>
        /// <returns>True if the collided projectile should be ignored, else false.</returns>
        private bool IgnoreProjectileCollision(GameObject collidedObj) {


            GameObject projectileObj = FindProjectileObject(collidedObj.gameObject);


            if (projectileObj == null)
                return false;


            //get projectile script
            ABC_Projectile collidedProjectile = projectileObj.GetComponent<ABC_Projectile>();

            //If the collided projectile is always colliding then return false as we are forced not ignoring the incoming projectile
            if (collidedProjectile.ability.abilityCollisionIgnores == AbilityCollisionIgnores.AlwaysCollideAll)
                return false;

            //If the collided projectile is always colliding with abilities from same originator then return false if we share the same caster
            if (collidedProjectile.ability.abilityCollisionIgnores == AbilityCollisionIgnores.AlwaysCollideSelfAbilities && collidedProjectile.originator == this.originator)
                return false;

            //If the collided projectile is always colliding with abilities from other originators then return false if we don't share the same originator
            if (collidedProjectile.ability.abilityCollisionIgnores == AbilityCollisionIgnores.AlwaysCollideForeignAbilities && collidedProjectile.originator != this.originator)
                return false;

            // depending on what we have set to ignore work out if we need to ignore the collided object or not
            switch (ability.abilityCollisionIgnores) {
                case AbilityCollisionIgnores.AlwaysCollideAll:

                    // target projectile is always colliding (like a shield) so just return false and don't ignore the collided object  
                    return false;

                case AbilityCollisionIgnores.IgnoreAll:

                    // we are ignoring all projectile objects
                    return true;

                case AbilityCollisionIgnores.IgnoreSelfAbilities:
                case AbilityCollisionIgnores.AlwaysCollideForeignAbilities:

                    // if the originators are not the same then return false as we not ignoring
                    if (collidedProjectile.originator != this.originator)
                        return false;
                    else
                        return true;

                case AbilityCollisionIgnores.IgnoreForeignAbilities:
                case AbilityCollisionIgnores.AlwaysCollideSelfAbilities:
                    // if originators are same then don't ignore
                    if (collidedProjectile.originator == this.originator)
                        return false;
                    else
                        return true;
                default:
                    // ignore projectile as default
                    return true;

            }



        }

        /// <summary>
        /// Checks if the object provided can collide with the Projectile.
        /// </summary>
        /// <remarks>
        /// Used to double check if the object this projectile just collided with is correct.
        /// </remarks>
        /// <param name="Obj">Object to check if it's allowed to collide with this Projectile.</param>
        /// <param name="Type">The type of collision (OnEnter, OnStay) etc</param>
        /// <returns>True if the object is allowed to collide, else false.</returns>
        private bool CorrectCollision(GameObject Obj, CollisionType Type) {


            //If no collision are not enabled or no collisions types are enabled then end here
            if (this.collisionEnabled == false || ability.onEnter == false && ability.onStay == false && ability.onExit == false && ability.particleCollision == false)
                return false;


            // is the correct collision type used (If none is given then we can ignore this check)
            if (Type != CollisionType.None && (ability.onEnter == false && Type == CollisionType.OnEnter || ability.onStay == false && Type == CollisionType.OnStay || ability.onExit == false && Type == CollisionType.OnExit || ability.particleCollision == false && Type == CollisionType.Particle))
                return false;


            //If we collided with another projectile and we are ignoring the collision of that projectile then return false here
            if (this.IgnoreProjectileCollision(Obj) == true)
                return false;



            // double check if we are ignoring caster or not (not always checked before hand e.g Particle Collision Types)
            if (ability.affectOriginObject == false && ability.travelType != TravelType.Self && originator.OriginatorLinkedToGameObject(Obj))
                return false;



            // if the object has a tag matching our ignore tags then this is not the correct collision so return false 
            if (ability.abilityIgnoreTag.Count > 0 && ABC_Utilities.ObjectHasTag(Obj, ABC_Utilities.ConvertTags(originator, ability.abilityIgnoreTag)) == true)
                return false;


            // if the object does not have a tag matching our required tags then this is not the correct collision so return false 
            if (ability.abilityRequiredTag.Count > 0 && ABC_Utilities.ObjectHasTag(Obj, ABC_Utilities.ConvertTags(originator, ability.abilityRequiredTag)) == false)
                return false;



            // if were travelling to a target and affect only target is on  we need to make sure it doesn't do anything to any other object unless in explosion
            if ((ability.travelType == TravelType.Self || ability.travelType == TravelType.SelectedTarget || ability.travelType == TravelType.NearestTag) && ability.affectOnlyTarget == true && this.splashColliding == false && this.targetObj.gameObject != Obj.gameObject)
                return false;


            //If the object has a state manager script and is ignoring ability collision (and the ability doesn't override that) then return false, if no SM exists then we skip over this check
            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(Obj);
            if (iEntity.HasABCStateManager() && iEntity.ignoringAbilityCollision == true && ability.overrideIgnoreAbilityCollision == false)
                return false;


            // If we got this far then the correct collision has been determined as true
            return true;

        }


        /// <summary>
        /// Function which handles the main logic when collision has occured with another object. This includes adding any ability effects onto collided objects with the StateManager script.
        /// </summary>
        /// <param name="CollidedObj">The object the projectiled collided with</param>
        /// <param name="HitPoint">(Optional) hit position where projectile collided</param>
        /// <param name="Destroy">(Optional) If the projectile can be destroyed</param>
        /// <param name="Bounce">(Optional) If the projectile is allowed to activate BounceMode</param>
        private void CollideHandler(GameObject CollidedObj, Vector3 HitPoint = default(Vector3), bool Destroy = true, bool Bounce = true) {

            //Apply effects to the collided object
            ability.ApplyAbilityEffectsToObject(CollidedObj, this.originator, this.gameObject, HitPoint, false, false, this.abilitySecondsPrepared);


            //loop through all surrounding objects to  apply effects
            if (meColliders != null) {
                foreach (Collider meCol in meColliders.Where(c => c.transform.name.Contains("*_ABCSurroundingObject"))) {
                    // if set to apply 
                    if (ability.projectileAffectSurroundingObject == true)
                        ability.ApplyAbilityEffectsToObject(meCol.gameObject, this.originator, this.gameObject, default(Vector3), false, false, this.abilitySecondsPrepared);

                }
            }

            // Start bounce handler if we can bounce
            if (Bounce && this.CanBounce(CollidedObj)) {
                StartCoroutine(BounceHandler(CollidedObj));

            } else {


                //Attach object on impact if probability is met
                if (ability.attachToObjectOnImpact == true && this.gameObject.transform.parent == null && ABC_Utilities.DiceRoll(ability.attachToObjectProbabilityMinValue, ability.attachToObjectProbabilityMaxValue) == true) {

                    //stop gravity
                    if (ability.applyGravity == true)
                        this.DisableGravity();

                    //Stop travelling
                    this.ToggleProjectileTravel(false);

                    //Find the nearest bone
                    Transform closestBone = ABC_Utilities.GetClosestBoneFromPosition(ABC_Utilities.GetStaticABCEntity(CollidedObj), HitPoint);

                    //Parent to closest bone if found
                    if (closestBone != null && ability.attachToObjectNearestBone == true) {
                        //Start from closest bone
                        this.gameObject.transform.position = closestBone.transform.position;
                        this.gameObject.transform.parent = closestBone.transform;
                        //Move from origin towards hit location
                        Vector3 movetowards = Vector3.MoveTowards(this.gameObject.transform.position, HitPoint, 0.1f * CollidedObj.transform.localScale.x);
                        this.gameObject.transform.position = new Vector3(movetowards.x, HitPoint.y, movetowards.z);
                        this.gameObject.transform.Translate(0, 0, -ability.attachToObjectStickOutFactor, Space.Self);
                    } else {
                        // else just attach to the object
                        this.gameObject.transform.parent = CollidedObj.transform;
                        //move out by stick out factor
                        this.gameObject.transform.Translate(0, 0, -ability.attachToObjectStickOutFactor, Space.Self);

                    }

                } else {

                    //else destroy projectile if allowed too
                    if (Destroy == true && ABC_Utilities.ObjectHasTag(CollidedObj, ABC_Utilities.ConvertTags(originator, ability.destroyIgnoreTag)) == false) {

                        float destroyDelay = ability.destroyDelay;

                        //If we collided with another ability then don't use destroy delay 
                        if (CollidedObj.name.Contains("ABC*_"))
                            destroyDelay = 0f;

                        StartCoroutine(DestroyProjectile(destroyDelay, CollidedObj));
                    }
                }
            }

            //determine if global impacts are allowed 
            bool globalImpacts = true;

            //If global impacts require a tag to activate and neither the ability originator or the collided object has the required tags then turn global impacts off
            if (ability.globalImpactRequiredTag.Count > 0 && ABC_Utilities.ObjectHasTag(this.originator.gameObject, ABC_Utilities.ConvertTags(originator, ability.globalImpactRequiredTag)) == false && ABC_Utilities.ObjectHasTag(CollidedObj, ABC_Utilities.ConvertTags(originator, ability.globalImpactRequiredTag)) == false)
                globalImpacts = false;

            // Spawn objects from collision
            if (ability.spawnObjectOnCollide == true)
                this.SpawnObject(CollidedObj.transform.position, CollidedObj.transform.rotation);

            //Modify game speed on impact
            if (globalImpacts == true && ability.modifyGameSpeedOnImpact == true)
                originator.ModifyGameSpeed(ability.modifyGameSpeedOnImpactSpeedFactor, ability.modifyGameSpeedOnImpactDuration, ability.modifyGameSpeedOnImpactDelay);

            //Activate hitstop
            if (ability.enableHitStopOnImpact == true) {
                ABC_Utilities.mbSurrogate.StartCoroutine(ability.ActivateHitStop(this.originator, ABC_Utilities.GetStaticABCEntity(CollidedObj)));
            }

            //Shake camera 
            if (globalImpacts == true && ability.shakeCameraOnImpact == true)
                ABC_Utilities.mbSurrogate.StartCoroutine(this.originator.ShakeCamera(ability.shakeCameraOnImpactDuration, ability.shakeCameraOnImpactAmount, ability.shakeCameraOnImpactSpeed, ability.shakeCameraOnImpactDelay));

            //Shake entity
            if (ability.shakeEntityOnImpact == true) {

                ABC_IEntity collidedEntity = ABC_Utilities.GetStaticABCEntity(CollidedObj);

                if (collidedEntity.HasABCStateManager())
                    collidedEntity.ShakeEntity(ability.shakeEntityOnImpactShakeAmount, ability.shakeEntityOnImpactShakeDecay, ability.shakeEntityOnImpactShakeDelay);
            }

            //Switch color for duration
            if (ability.switchColorOnImpact == true) {

                ABC_IEntity collidedEntity = ABC_Utilities.GetStaticABCEntity(CollidedObj);

                if (collidedEntity.HasABCStateManager())
                    collidedEntity.SwitchEntitiesColor(ability.switchColorOnImpactColor, ability.switchColorOnImpactDuration, ability.switchColorOnImpactDelay, ability.switchColorOnImpactUseEmission);
            }





            this.originator.EnableOriginatorsAbilitiesAfterEvent(ability.abilityID, AbilityEvent.Collision);

            // Set Combo Locks for Ability if hit is required
            if (ability.comboHitRequired == true)
                ability.SetComboLock(CollidedObj);


        }

        /// <summary>
        /// Function which should always be called to Disable the Projectile. Will disable the GameObject after activating important functionality. 
        /// </summary>
        /// <remarks>
        /// Examples of functionality called: activating graphics, reverting surrounding objects, starting cooldowns and placing the GameObject back into the Pool.
        /// </remarks>
        /// <param name="CollidedObject">(Optional) Object involed in the collision that caused the Projectile to be destroyed.</param>
        private void DisableProjectile(GameObject CollidedObject = null) {



            // call method to run disable projectile code - This is in an ability method as this code is important to run when a projectile gets disabled - where destroying projectile can run many functions or not up to user
            ability.DisableProjectile(meTransform.gameObject, originator, CollidedObject);



            // cancel all invokes
            CancelInvoke();

        }


        /// <summary>
        /// Function will destroy the Projectile and activate any special functionality which occurs when the Projectile is destroyed (splash collisions etc).
        /// </summary>
        /// <param name="Delay">Time to wait before function will start</param>
        /// <param name="CollidedObject">(Optional) Object involed in the collision that caused the Projectile to be destroyed.</param>
        private IEnumerator DestroyProjectile(float Delay = 0f, GameObject CollidedObject = null) {

            // if gameobject is not active then we can finish the method here
            if (gameObject.activeInHierarchy == false)
                yield break;



            // Wait for delay declared before we continue to destroy the projectile 
            if (Delay > 0f) {

                //wait for duration then end animation
                for (var i = Delay; i > 0;) {

                    // actual wait time 
                    if (i < 0.2f) {
                        // less then 0.2  so we only need to wait the .xx time
                        yield return new WaitForSeconds(i);
                    } else {
                        // wait a small amount and keep looping till casting time = 0; 
                        yield return new WaitForSeconds(0.2f);
                    }

                    //reduce time left unless ability is currently in hit stop then things are on hold 
                    if (ability.hitStopCurrentlyActive == false)
                        i = i - 0.2f;
                }

            }


            //If projectile is still in hitstop then wait for this to end (projectile frozen in frame) before continuing to destroy
            while (ability.hitStopCurrentlyActive == true) {
                yield return new WaitForEndOfFrame();
            }

            //projectile is being destroyed run splash (boom) method
            StartCoroutine(SplashMe(CollidedObject));

            //Spawn any objects which are set to appear on destruction
            if (ability.spawnObjectOnDestroy == true)
                this.SpawnObject(meTransform.position, meTransform.rotation);


            //Disable Projectile
            this.DisableProjectile(CollidedObject);


        }


        /// <summary>
        /// Function will "Splash" the Projectile like an explosion. Depending on the settings of the Ability it will run effects or add explosion force to Objects a set distance around the Projectile's current position. 
        /// </summary>
        /// <param name="IgnoredObject">Splash effects and explosion force will not effect the object provided in this parameter</param>
        private IEnumerator SplashMe(GameObject IgnoredObject = null) {

            // let rest of code know we are splash colliding
            this.splashColliding = true;

            // check splash collision for whole object 
            if (ability.useDestroySplashEffect == true) {

                // for each item around us - lets check if they are an ABC ability if they are lets handle the collison
                foreach (ABC_IEntity entity in ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, ability.destroySplashRadius).ToList()) {

                    // If a statescript doesn't exist then we don't care about the object so continue
                    if (entity.HasABCStateManager() == false)
                        continue;

                    // Ignore collision with the ignored object (normally the object already has an effect on them due to the splash happening cause it hit them)
                    if (IgnoredObject != null && entity.gameObject == IgnoredObject)
                        continue;

                    // If we splashed onto the originator and we have not set the ability to affect the originator then continue to next item
                    if (ability.affectOriginObject == false && originator.OriginatorLinkedToGameObject(entity.gameObject))
                        continue;

                    //check its correct collision
                    if (this.CorrectCollision(entity.gameObject, CollisionType.None) == false)
                        continue;



                    // If we got this far then everything is good so we can activate effects
                    ability.ApplyAbilityEffectsToObject(entity.gameObject, this.originator, this.gameObject, default(Vector3), true, false, this.abilitySecondsPrepared);


                }

            }


            // now run through explosion splashing
            if (ability.destroySplashExplosion == true) {

                // for each item around us - lets check if they are an ABC ability if they are lets handle the collison
                foreach (ABC_IEntity entity in ABC_Utilities.GetAllABCEntitiesInRange(meTransform.position, ability.destroySplashExplosionRadius).ToList()) {

                    // If a statescript doesn't exist then we don't care about the object so continue
                    if (entity.HasABCStateManager() == false)
                        continue;

                    // If the object doesn't have a tag matching our explosion list then continue to next item
                    if (ability.destroySplashExplosionTagLimit == true && ABC_Utilities.ObjectHasTag(entity.gameObject, ABC_Utilities.ConvertTags(originator, ability.destroySplashExplosionAffectTag)) == false)
                        continue;


                    // If we exploded onto the originator and we have not set the ability to affect the originator then continue to next item
                    if (ability.affectOriginObject == false && originator.OriginatorLinkedToGameObject(entity.gameObject))
                        continue;


                    // add explosion force onto the object
                    if (entity.rigidBody != null)
                        entity.rigidBody.AddExplosionForce(ability.destroySplashExplosionPower, meTransform.position, ability.destroySplashExplosionRadius, ability.destroySplashExplosionUplift);

                }


            }


            //wait a frame
            yield return 0;

            // no longer splash colliding
            splashColliding = false;


        }




        /// <summary>
        /// Function call to spawn any "SpawnObjects" setup with the ability. 
        /// </summary>
        /// <param name="Position">Position where the SpawnObject will appear</param>
        /// <param name="Rotation">Rotation of the spawnObject</param>
        private void SpawnObject(Vector3 Position, Quaternion Rotation) {

            ability.ActivateGraphicAtPosition(AbilityPositionGraphicType.SpawnObject, Position, Rotation);

        }



        #endregion



        // ********************* Game ********************

        #region Game



        void OnCollisionEnter(Collision col) {

            //Add to colliding entity tracker 
            this.AddCollidingEntity(col.gameObject);

            //Run method to add effects if the collision is a projectile
            this.ActivateCollision(col.gameObject, CollisionType.OnEnter, col.contacts.FirstOrDefault().point);


        }


        void OnTriggerEnter(Collider col) {

            //Add to colliding entity tracker 
            this.AddCollidingEntity(col.gameObject);

            //Run method to add effects if the collision is a projectile
            this.ActivateCollision(col.gameObject, CollisionType.OnEnter, meTransform.position);

        }


        /// <summary>
        /// Replacement for OnStay handling for better performance
        /// </summary>
        private void OnStayHandler() {

            //If nothing is currently colliding then end here
            //or if we are only doing on stay collisions every x interval (above 0) and the interval has not passed since the last recorded times
            if (this.collidingEntities.Count == 0 || ability.onStayInterval > 0 && this.lastRecordedOnStayCollision > 0 && Time.time - this.lastRecordedOnStayCollision < ability.onStayInterval)
                return;

            //Track the time we did an On Stay Collision
            this.lastRecordedOnStayCollision = Time.time;

            //Loop through each entity that's colliding running the activate collision for 'OnStay' collision type
            foreach (ABC_IEntity entity in this.collidingEntities) {
                this.ActivateCollision(entity.gameObject, CollisionType.OnStay, meTransform.position);
            }

        }




        void OnCollisionExit(Collision col) {

            //Remove from colliding entity tracker
            this.RemoveCollidingEntity(col.gameObject);

            //Run method to add effects if the collision is a projectile
            this.ActivateCollision(col.gameObject, CollisionType.OnExit, col.contacts.FirstOrDefault().point);

        }


        void OnTriggerExit(Collider col) {

            //Remove from colliding entity tracker
            this.RemoveCollidingEntity(col.gameObject);


            //Run method to add effects if the collision is a projectile
            this.ActivateCollision(col.gameObject, CollisionType.OnExit, meTransform.position);

        }


        //Make sure to turn on Send Message on particle

        void OnParticleCollision(GameObject col) {


            //Run method to add effects if the collision is a projectile
            this.ActivateCollision(col.gameObject, CollisionType.Particle);
        }



        void Awake() {

            meTransform = transform;
            meRigidbody = GetComponent<Rigidbody>();

        }




        // Use this for initialization
        void OnEnable() {

            //reset the entities colliding tracker
            this.collidingEntities.Clear();


            // reset any previous movements and collisions
            meRigidbody.linearVelocity = Vector3.zero;
            meRigidbody.angularVelocity = Vector3.zero;

            // reset gravity
            GetComponent<Rigidbody>().useGravity = false;

            //if we are waiting for key press before collision is enabled, then turn collision enabled flag to false
            if (ability.enableCollisionAfterKeyPress == true)
                this.collisionEnabled = false;

            // reset bounce counter
            bounceCounter = 0;

            //Reset the time in which on stay collision last occured
            lastRecordedOnStayCollision = 0f;

            // reset splash collider
            splashColliding = false;

            // reset variable so we know when collider has been added and setup
            colliderAdded = false;

            // get all colliders
            GetAllColliders();


            // if we can collide or trigger and collider is already enabled then run setup
            if (meCollider != null && meCollider.enabled == true)
                ColliderSetup();


            // add gravity after time 
            if (ability.applyGravity == true)
                Invoke("ApplyGravity", ability.applyGravityDelay);

            //record ability prepare time on this component
            this.abilitySecondsPrepared = ability.GetCurrentAbilitySecondsPrepared();

            // Destroy object after chosen seconds - if 0 then the duration is unlimited, takes into account initiation speed adjustment (sync destroy with swing speed etc)
            if (ability.duration > 0) {
                //Work out base duration
                float baseDuration = ABC_Utilities.ModifyTimeByPercentage(this.initiationBaseSpeedAdjustment, ability.duration);
                //then apply global changes
                StartCoroutine(DestroyProjectile(ABC_Utilities.ModifyTimeByPercentage(this.initiationGlobalSpeedAdjustment, baseDuration), null));

            }


            // Checks if we need to reduce mana whilst ability is active 
            InvokeRepeating("ReduceManaWhilstActive", 1f, 1f);


            //Checks if we need to reduce ammo whilst ability is active
            InvokeRepeating("ReduceAmmoWhilstActive", 1f, 1f);


        }




        // Update is called once per frame
        void Update() {

            //If the ability is a melee attack and it was interruped then destroy projectile
            if (ability.MeleeAttackInterrupted(originator))
                StartCoroutine(DestroyProjectile(ability.destroyDelay));

            //if the ability is a melee attack and keep rotating is true then keep the originator rotating to target as long as this projectile is running
            if (ability.abilityType == AbilityType.Melee && ability.meleeKeepRotatingToSelectedTarget && targetObj != null && originator != null)
                originator.TurnTo(targetObj);


            //If collisions have not been enabled then call the handler to wait for the correct key press to reenable collisions
            if (this.collisionEnabled == false)
                this.EnableCollisionTriggerHandler(originator);

            //Call the OnStayHandler
            if (this.isActiveAndEnabled)
                OnStayHandler();

        }


        void FixedUpdate() {

            //If projectile is set to never sleep and rigidbody is sleeping then wake it up
            if (this.meRigidbody != null && ability.neverSleep == true && this.meRigidbody.IsSleeping())
                this.meRigidbody.WakeUp();

            // if we can actually collide or trigger. 
            if (ability.addAbilityCollider == true) {

                // collider might come on at a delay (due to settings) so watch for collider whilst collider is not added
                if (this.colliderAdded == false && meCollider != null && meCollider.enabled == true)
                    ColliderSetup();

            }

        }

        private void OnDisable() {

            // stop any invokes/coroutines
            CancelInvoke();
            StopAllCoroutines();
        }


        #endregion


    }
}

