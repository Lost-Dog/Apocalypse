using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ABCToolkit {

    /// <summary>
    /// Class contains settings and functions to setup and activate abilities.
    /// </summary>
    [System.Serializable]
    public class ABC_Ability {

        /// <summary>
        /// Instantiates New Ability Object
        /// </summary>
        public ABC_Ability() {

        }

        // ****************** Nested Classes *************************** 

        #region Nested Classes

        /// <summary>
        /// Class which holds information regarding additional ability starting positions
        /// Any additional starting positions declared will instanstiate another projectile object 
        /// allowing you to have for example 2 fireballs come from different directions
        /// </summary>
        [System.Serializable]
        public class AdditionalStartingPosition {

            /// <summary>
            /// Instantiates New  Object
            /// </summary>
            public AdditionalStartingPosition() { }


            /// <summary>
            /// The type determining when the additional ability appears either after a delay or at a defined initiating animation percentage (0-100%)
            /// </summary>
            [Tooltip("The type determining when the additional ability appears either after a delay or at a defined initiating animation percentage (0-100%)")]
            public AbilityInitiationDelayType startingDelayType = AbilityInitiationDelayType.AfterDelay;

            /// <summary>
            /// Delay before additional ability appears
            /// </summary>
            public float startingDelay;

            /// <summary>
            /// the additional ability will apper in game when the initiating animation reaches the percentage defined in this settings
            /// </summary>
            [Range(0, 100)]
            [Tooltip(" the additional ability will apper in game when the initiating animation reaches the percentage defined in this settings")]
            public float startingDelayInitiatingAnimationPercentage = 50;


            /// <summary>
            /// The starting position for the ability
            /// </summary>
            public StartingPosition startingPosition;


            /// <summary>
            /// used for starting position if OnObject is selected. Determines what object the ability will start on
            /// </summary>
            [Tooltip("Starting position of the Ability")]
            public ABC_GameObjectReference startingPositionOnObject;


            /// <summary>
            /// Tag which the ability can start from if starting position is OnTag.  Does not work for ABC tags. 
            /// </summary>
            [Tooltip("Tag to start from")]
            public string startingPositionOnTag;


            /// <summary>
            /// Offset of the starting position
            /// </summary>
            [Tooltip("Offset on the starting position")]
            public Vector3 startingPositionOffset;


            /// <summary>
            /// Forward offset of the starting position
            /// </summary>
            [Tooltip("Forward offset from starting position")]
            public float startingPositionForwardOffset;

            /// <summary>
            /// Right offset of the starting position
            /// </summary>
            [Tooltip("Right offset from starting position")]
            public float startingPositionRightOffset;


            /// <summary>
            /// Determines the starting direction for the ability 
            /// </summary>
            [Tooltip("When initialised what way is the object facing")]
            public Vector3 startingRotation;


            /// <summary>
            /// If true then the euler rotation of the ability will be set to the starting points euler rotation
            /// </summary>
            [Tooltip("If true then the euler rotation of the ability will be set to the starting points euler rotation")]
            public bool setEulerRotation = false;

            /// <summary>
            /// If true then the initiating graphic will be repeated for the additional starting point
            /// </summary>
            [Tooltip("If true then the initiating graphic will be repeated for the additional starting point")]
            public bool repeatInitiatingGraphic = false;

            /// <summary>
            /// The delay from first initiation that the graphic will appear
            /// </summary>
            [Tooltip("If true then the euler rotation of the ability will be set to the starting points euler rotation")]
            public float repeatInitiatingGraphicDelay = 0.3f;

            /// <summary>
            /// If true then the weapon trail of the current equipped weapon will be used
            /// </summary>
            [Tooltip("If true then the weapon trail of the current equipped weapon will be used")]
            public bool useWeaponTrail = false;

            /// <summary>
            /// Determines which weapon graphic the weapon trail will activate on
            /// </summary>
            [Tooltip("Determines which weapon graphic the weapon trail will activate on")]
            public int weaponTrailGraphicIteration = 0;


        }

        #endregion


        // ****************** Settings ***************************

        #region Ability Settings 

        /// <summary>
        /// Links to a global ability which will activate instead of the properties in this ability
        /// </summary>
        [Tooltip("Links to a global ability which will activate instead of the properties in this ability")]
        public ABC_GlobalElement globalAbilities = null;

        /// <summary>
        /// The global element this ability was created from during play (if this ability was made from a global element)
        /// </summary>
        [Tooltip("The global element this ability was created from during play (if this ability was made from a global element)")]
        public ABC_GlobalElement globalElementSource = null;

        /// <summary>
        /// If true then the abilities will be modified at run type to match the game type selected
        /// </summary>
        [Tooltip(" If true then the abilities will be modified at run type to match the game type selected")]
        public bool globalAbilitiesEnableGameTypeModification = false;

        /// <summary>
        /// What game type to modify the global abilities to
        /// </summary>
        [Tooltip("What game type to modify the global abilities to")]
        public ABC_GlobalPortal.GameType globalAbilitiesGameTypeModification = ABC_GlobalPortal.GameType.Action;


        /// <summary>
        /// Name of the Ability
        /// </summary>
        [Tooltip("Name of the Ability")]
        public string name = "";

        /// <summary>
        /// ID of the ability so name can be changed but all setups linking to this ability will remain
        /// </summary>
        [Tooltip("ID of the Ability")]
        public int abilityID = 0;

        /// <summary>
        /// When set to true ability will be flagged to activate without the press of buttons. Good for scripting auto activation.
        /// </summary>
        [Tooltip("When set to true AutoCast will automatically fire the ability without the press of buttons. Good for scripting auto activation.")]
        public bool autoCast = false;

        /// <summary>
        /// A list of tags for the ability which can be used for filtering 
        /// </summary>
        [Tooltip("A list of tags for the ability which can be used for filtering")]
        public List<string> abilityTags = new List<string>();

        /// <summary>
        /// If true then ability can be assigned to Ability Groups
        /// </summary>
        [Tooltip("Determines if this ability can be assigned to Ability Groups IDs of the groups this ability belongs to")]
        public bool allowAbilityGroupAssignment = false;

        /// <summary>
        /// The IDs of the groups this ability is assigned too
        /// </summary>
        [Tooltip("The IDs of the groups this ability belongs to")]
        public List<int> assignedAbilityGroupIDs = new List<int>();

        /// <summary>
        /// Used by inspector to select dropdowns
        /// </summary>
        public int abilityGroupListChoice = 0;

        /// <summary>
        /// The names of the groups this ability is assigned too
        /// </summary>
        [Tooltip("The names of the groups this ability belongs to")]
        public List<string> assignedAbilityGroupNames = new List<string>();


        /// <summary>
        ///  used by inspector to show current ability parent choice 
        /// </summary>
        public int inspectorParentAbilityListChoice = 0;


        /// <summary>
        ///  Used by inspector to sit abilities under one each other (parent/child) - although may not be related in game it's a way to group neatly
        /// </summary>
        public int parentAbilityID = 0;


        /// <summary>
        /// If true then any children of the ability will be displayed in inspector
        /// </summary>
        public bool showChildrenInInspector = true;

        /// <summary>
        /// Description of the ability 
        /// </summary>
        [Tooltip("Description of the ability")]
        public string description = " ";

        /// <summary>
        /// Ability Icon which can be displayed on GUIs 
        /// </summary>
        [Tooltip("Icon image of the ability")]
        public ABC_Texture2DReference iconImage;

        /// <summary>
        /// If enabled then the ability will log to the ability gui Log when activating etc
        /// </summary>
        [Tooltip("If enabled then the ability will log to the ability gui Log")]
        public bool loggingEnabled = true;

        /// <summary>
        /// Used for global abilities, can pick to override the enable status locally
        /// </summary>
        [Tooltip("Used for global abilities, can pick to override the enable status locally")]
        public bool globalAbilityOverrideEnableStatus = false;

        /// <summary>
        /// If the ability is enabled or not. If false then the ability can't be used in game
        /// </summary>
        [Tooltip("Is the ability active in the game. If not active then can't be cast and pool is not made")]
        public bool abilityEnabled = true;


        /// <summary>
        /// How long until ability is disabled after becoming enabled 
        /// </summary>
        /// <remarks>
        /// Useful if ability is applied on a pickup that last a short time. Only works from function calls and not after being set manually
        /// </remarks>
        [Tooltip("How long until ability is disabled after becoming enabled ")]
        public float enableDuration = 0f;

        /// <summary>
        /// If true then the ability will be enabled after another ability has activated or collided
        /// </summary>
        [Tooltip("If true then ability will be enabled after a certain event occurs")]
        public bool enableAfterEvent = false;

        /// <summary>
        /// Ability will be enabled after any abilities (ID) in this list are activated
        /// </summary>
        [Tooltip("Ability will be enabled after any abilities in this list are activated")]
        public List<int> enableAfterAbilityIDsActivated;

        /// <summary>
        /// Ability will be enabled after any abilities (ID) in this list collide
        /// </summary>
        [Tooltip("Ability will be enabled after any abilities in this list are activated")]
        public List<int> enableAfterAbilityIDsCollide;

        /// <summary>
        /// If true then when this ability is activated the linked abilities will also activate ignoring triggers and activation restrictions
        /// </summary>
        [Tooltip("If true then when this ability is activated the linked abilities will also activate ignoring triggers and activation restrictions")]
        public bool enableAbilityActivationLinks = false;

        /// <summary>
        /// Any abilities (ID) in this list will be activated when this ability is activated
        /// </summary>
        [Tooltip("Any abilities in this list will be activated when this ability is activated")]
        public List<int> activationLinkAbilityIDs;

        /// <summary>
        /// If true then linked abilities will always have the same trigger key/button as this ability
        /// </summary>
        [Tooltip("If true then linked abilities will always have the same trigger key/button as this ability")]
        public bool enableAbilityTriggerLinks = false;

        /// <summary>
        /// Any abilities (ID) in this list will have their trigger key/button match this ability
        /// </summary>
        [Tooltip("Any abilities (ID) in this list will have their trigger key/button match this ability")]
        public List<int> triggerLinkAbilityIDs;

        /// <summary>
        /// If true then effects can have the potency modified by a critical hit, this is done after any stat modifications 
        /// </summary>
        [Tooltip("If true then effects can have the potency modified by a critical hit, this is done after any stat modifications")]
        public bool enableCriticalHit = false;

        /// <summary>
        /// Base percentage chance for the ability effects to critical hit
        /// </summary>
        [Tooltip("Base percentage chance for the ability effects to critical hit")]
        public float potencyCriticalHitBaseChance = 1f;

        /// <summary>
        /// Will allow for a stat to modify the crit chance
        /// </summary>
        [Tooltip("Will allow for a stat to modify the crit chance")]
        public bool potencyCriticalHitEnableChanceStatModification = false;

        /// <summary>
        /// Stat modification for crit chance
        /// </summary>
        [Tooltip("Stat modification for crit chance")]
        public Effect.PotencyStatModifications potencyCriticalHitChanceStatModification = new Effect.PotencyStatModifications();


        /// <summary>
        /// If true then when this ability applies effects to the target the linked abilities effects will also be applied, under the linked abilities name. 
        /// </summary>
        [Tooltip("If true then when this ability applies effects to the target the linked abilities effects will also be applied, under the linked abilities name. ")]
        public bool enableAbilityEffectLinks = false;

        /// <summary>
        /// Any abilities (ID) in this list will have their effects applied when this abilities effects are applied
        /// </summary>
        [Tooltip("Any abilities (ID) in this list will have their effects applied when this abilities effects are applied")]
        public List<int> effectLinkAbilityIDs;


        /// <summary>
        /// List of tags the ability will ignore
        /// </summary>
        public List<string> abilityIgnoreTag = new List<string>();

        /// <summary>
        /// List of tags that the ability will only collide with 
        /// </summary>
        public List<string> abilityRequiredTag = new List<string>();

        /// <summary>
        /// Mana cost to use the ability
        /// </summary>
        [Tooltip("Cost to use the Ability")]
        public float manaCost = 0;

        /// <summary>
        /// The cost to an stat value when the ability is used
        /// </summary>
        [Tooltip("The cost to an stat value when the ability is used")]
        public float statCost = 0;

        /// <summary>
        /// The type of stat to decrease when the ability is used
        /// </summary>
        [Tooltip("The type of stat to decrease when the ability is used")]
        public ABCIntegrationType statCostIntegrationType = ABCIntegrationType.ABC;

        /// <summary>
        /// The name of the stat to decrease when ability is used
        /// </summary>
        [Tooltip("The name of the stat to decrease when ability is used")]
        public string statCostName = "stamina";

        /// <summary>
        /// Whilst active the ability will continue to reduce mana - dissapearing when reaching 0
        /// </summary>
        [Tooltip("Whilst active the ability will continue to reduce mana - dissapearing when reaching 0")]
        public bool reduceManaWhenActive = false;



        /// <summary>
        /// If true then ability is set to be exported. 
        /// </summary>
        [Tooltip("If true then ability will be exported when button is clicked")]
        public bool enableExport = false;

        /// <summary>
        /// Only shown in Exported Abilities Editor - Allows user to pick what abilities to import
        /// </summary>
        public bool enableImport = true;


        /// <summary>
        /// Will adjust the activation interval for the activating entity when this ability initiates
        /// </summary>
        /// <remarks>This will be reset by the entity ABC controller once the interval is over</remarks>
        public float tempAbilityActivationIntervalAdjustment = 0f;

        /// <summary>
        /// If true then the ability will randomly be swapped with other abilities in the same trigger group that are also set to be randomly swapped
        /// </summary>
        [Tooltip("If true then the ability will randomly be swapped with other abilities in the same trigger group that are also set to be randomly swapped")]
        public bool randomlySwapAbilityPosition = false;

        /// <summary>
        /// If true then the ability will be used as a combo within the same button group. 
        /// </summary>
        /// <remarks>
        /// When set as part of a combo the ability will fire sequentially depending on other abilities in the same button group setup as a combo. 
        /// If the ability before has been activated the next combo ability can be activated depending on if it's within the correct time setup else the combo is reset. 
        /// </remarks>
        [Tooltip("The ability can now be used as a combo in the group")]
        public bool abilityCombo = false;

        /// <summary>
        /// How long the user has to press the same key to activate the next combo in the group. 
        /// </summary>
        [Tooltip("How long the user has to press the same key to activate the next combo in the group.")]
        public float comboNextActivateTime = 2f;


        /// <summary>
        /// If true then this ability when activated will never resetany other ability combos currently in progress. If disabled then using this ability will stop and reset all current combos
        /// </summary>
        [Tooltip("If true then this ability when activated will never reset any other ability combos currently in progress. If disabled then using this ability will stop and reset all current combos")]
        public bool neverResetOtherCombos = false;

        /// <summary>
        /// If true then a collision is required for the next combo attack to become avaliable
        /// </summary>
        [Tooltip("If true then a collision is required for the next combo attack to become avaliable")]
        public bool comboHitRequired = false;

        /// <summary>
        /// Determines if the ability will only fire if the caster is on the ground, in the air or either
        /// </summary>
        [Tooltip("The ability will either only fire if the caster is on the ground, in the air or both")]
        public AbilityLandOrAir LandOrAir;

        /// <summary>
        /// float which identifies what point from the floor the entity is considered jumping
        /// </summary>
        [Tooltip("How far caster has to be from the ground to be considered in the air for this ability")]
        public float airAbilityDistanceFromGround = 2f;


        /// <summary>
        /// If true then the ability will never sleep and always look for collisions. Sleeping occurs when a Rigidbody is moving slower than a defined minimum linear or rotational speed, the physics engine assumes it has come to a halt
        /// </summary>
        [Tooltip("If true then the ability will never sleep and always look for collisions. Sleeping occurs when a Rigidbody is moving slower than a defined minimum linear or rotational speed, the physics engine assumes it has come to a halt")]
        public bool neverSleep = false;

        /// <summary>
        /// If true then the ability's rigidbody kinematic will be set to true
        /// </summary>
        [Tooltip("If true then the ability's rigidbody kinematic will be set to true")]
        public bool isKinematic = false;


        /// <summary>
        /// Mass of the projectile object 
        /// </summary>
        [Tooltip("Mass of the Ability projectile")]
        public float mass = 1;

        /// <summary>
        /// Time to wait until Ability projectile will start travelling
        /// </summary>
        [Tooltip("Time to wait until Ability projectile will start travelling.")]
        public float travelDelay = 0;

        /// <summary>
        /// If true Ability will only activate if CrossHair override is active
        /// </summary>
        [Tooltip("Ability will only activate if CrossHair is present.")]
        public bool requireCrossHairOverride = false;

        /// <summary>
        /// The type of ability. Particle will create a moving projectile in the game, raycast will run effects on any objects hit by the cast sent out 
        /// and melee attacks will create a particle which is attached to an object (Weapon)
        /// </summary>
        [Tooltip("Type of ability (Particle / Raycast / Melee)")]
        public AbilityType abilityType = AbilityType.Projectile;

        /// <summary>
        /// If the ability is in raycast mode then how far the cast will travel
        /// </summary>
        [Tooltip("How far RayCast will travel")]
        [Range(0f, 2000f)]
        public float rayCastLength = 100f;

        /// <summary>
        ///If ticked then the RayCast will only hit one entity else it will be a RayCast sphere capable of hitting multiple entities (shotgun)
        /// </summary>
        [Tooltip("If ticked then the RayCast will only hit one entity else it will be a RayCast sphere capable of hitting multiple entities (shotgun)")]
        public bool rayCastSingleHit = true;

        /// <summary>
        /// If the ability is in raycast mode then the width of the raycast. Increase the radius to hit more then one object 
        /// </summary>
        [Tooltip("Radius (size) of the RayCast")]
        public float rayCastRadius = 1f;

        /// <summary>
        /// the amount of objects that can be hit by the ability raycast 
        /// </summary>
        [Tooltip("If true the raycast will only affect the first entity it hits")]
        public int raycastHitAmount = 1;

        /// <summary>
        /// If true then the raycast can be blocked by non statemanagers 
        /// </summary>
        [Tooltip("If true then the raycast can be blocked by non statemanagers ")]
        public bool raycastBlockable = true;

        /// <summary>
        /// If true then the raycast will ignore terrain collisions
        /// </summary>
        [Tooltip("If true then the raycast will ignore terrain collisions")]
        public bool raycastIgnoreTerrain = false;

        /// <summary>
        /// If true then surrounding objects will be included with the ability 
        /// </summary>
        [Tooltip("Use surrounding oject as part of the graphic")]
        public bool includeSurroundingObject = false;

        /// <summary>
        /// If true then current target will be used as a surrounding object 
        /// </summary>
        [Tooltip("Use current")]
        public bool surroundingObjectTarget = false;

        /// <summary>
        /// Ability will only activate if a target surrounding object is included
        /// </summary>
        [Tooltip("Ability will only fire if a surrounding object is being used")]
        public bool surroundingObjectTargetRequired = false;

        /// <summary>
        /// If true then the target will require specific tags to be used as a surrounding object
        /// </summary>
        [Tooltip("If true then only tags entered can be used as a surrounding object")]
        public bool surroundingObjectTargetRestrict = false;

        /// <summary>
        /// Only target Objects with the following tags can be converted to be a surrounding object
        /// </summary>
        [Tooltip("Ability will only fire on the tags listed below")]
        public List<string> surroundingObjectTargetAffectTag = new List<string>();

        /// <summary>
        /// If no selected target then the soft target will be used as a surrounding object instead
        /// </summary>
        [Tooltip("If no selected target then the soft target will be used as a surrounding object instead")]
        public bool surroundingObjectAuxiliarySoftTarget = false;

        /// <summary>
        /// Range in which surrondingObjects have to be within to be apart of the ability
        /// </summary>
        [Tooltip("Range in which surrondingObject has to be within to be apart of the projectile")]
        public float surroundingObjectTargetRange = 50f;

        /// <summary>
        /// If true then objects around the ability starting position with defined tags will be used as a surrounding object
        /// </summary>
        [Tooltip("Ability will pick up surrounding object tags in a range around from ability start position")]
        public bool surroundingObjectTags = false;

        /// <summary>
        /// Ability will only activate if a surrounding object is included which has been created from a near by object with the correct tag  
        /// </summary>
        [Tooltip("Ability will only fire if a surrounding object is being used")]
        public bool surroundingObjectTagRequired = false;

        /// <summary>
        /// What tags an object is required to have to be converted to a surrounding object
        /// </summary>
        [Tooltip("Which Tags can we use as surrounding object")]
        public List<string> surroundingObjectTagAffectTag = new List<string>();

        /// <summary>
        /// The max amount of surrounding tag objects the ability can have
        /// </summary>
        [Tooltip("How many surrounding tag objects we can pick up at once")]
        public int surroundingObjectTagLimit = 2;

        /// <summary>
        /// Range in which objects and their tags will be checked to be turned into a surrounding object 
        /// </summary>
        [Tooltip("Range to check if we have any surrounding object tags to use")]
        public float surroundingObjectTagsRange = 15f;

        /// <summary>
        /// if true surrounding object will be destroyed with the ability 
        /// </summary>
        [Tooltip("if true surrounding object will be destroyed with projectile")]
        public bool destroySurroundingObject = false;

        /// <summary>
        /// On collision the surrounding object will be inflicted with the ability effects (Damage etc)
        /// </summary>
        [Tooltip("On collision the surrounding object will be inflicted with the projectile state effects (Damage etc)")]
        public bool projectileAffectSurroundingObject = false;

        /// <summary>
        /// If locked then the surrounding object can not be stolen by another ability
        /// </summary>
        [Tooltip("If locked then the surrounding object can not be stolen by another ability using surrounding object")]
        public bool lockSurroundingObject = true;

        /// <summary>
        /// Surrounding objects will scatter around the ability so it's not directly on top. The scatter range is determined by the min and max value
        /// </summary>
        [Tooltip("minimum scatter range")]
        public float minimumScatterRange = 0f;

        /// <summary>
        /// Surrounding objects will scatter around the ability so it's not directly on top. The scatter range is determined by the min and max value
        /// </summary>
        [Tooltip("max scatter range ")]
        public float maximumScatterRange = 0f;

        /// <summary>
        /// If true then the surrounding objects will be able to detect collisions as part of the ability
        /// </summary>
        [Tooltip("If true then the surrounding objects will be able to detect collisions as part of the ability")]
        public bool applyColliderToSurroundingObjects = true;

        /// <summary>
        /// If true then the surrounding object will start detecting collision once it reaches the ability and not before
        /// </summary>
        [Tooltip("If true then the surrounding object collider will be applied once the object reaches the collider. Otherwise it will be applied instantly")]
        public bool applyColliderWhenProjectileReached = true;

        /// <summary>
        /// If enabled then the surrounding objects will travel towards the ability instead of just appearing where the ability starts
        /// </summary>
        [Tooltip("If enabled then the surrounding objects will travel towards the projectile instead of just appearing where the projectile starts")]
        public bool sendObjectToProjectile = false;

        /// <summary>
        /// How long it takes for the surrounding object to reach the ability
        /// </summary>
        [Tooltip("How long it takes for the object to reach the projectile")]
        public float objectToProjectileDuration = 2f;

        /// <summary>
        /// Main graphic object which gets created as an ability projectile
        /// </summary>
        [Tooltip("Main graphic")]
        public ABC_GameObjectReference mainGraphic;

        /// <summary>
        /// additional graphic that sits under the main graphic
        /// </summary>
        [Tooltip("Sub Particle. This graphic will be a child object of Main graphic")]
        public ABC_GameObjectReference subGraphic;

        /// <summary>
        /// If true then graphic scale will be modified during play
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public bool scaleAbilityGraphic = false;

        /// <summary>
        /// scale to apply to graphic
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public float abilityGraphicScale = 1f;

        /// <summary>
        /// If true then the ability projectile will be applied to a layer defined
        /// </summary>
        [Tooltip("apply the projectile to a layer")]
        public bool chooseLayer = false;

        /// <summary>
        /// Name of the Layer the projectile will be applied too
        /// </summary>
        [Tooltip("Name of the Layer the projectile will be on")]
        public string abLayer;

        /// <summary>
        /// If true then collider will be in trigger mode and not use collision physics
        /// </summary>
        [Tooltip("If true then collider will be in trigger mode and not use collision physics")]
        public bool useColliderTrigger = true;

        /// <summary>
        /// If true then gravity will be applied to the ability projectile
        /// </summary>
        [Tooltip("Applies gravity to the ability")]
        public bool applyGravity;

        /// <summary>
        /// How long before gravity is added. Useful for throwing abilities. 
        /// </summary>
        [Tooltip("How long before gravity is added. Handy for throwing items.")]
        public float applyGravityDelay;

        /// <summary>
        /// How the ability will travel: Forward3D, SelectedTarget, ToWorld etc
        /// </summary>
        [Tooltip("How the ability will travel")]
        public TravelType travelType;

        /// <summary>
        /// If true then the ability will still activate even if the hit prevention is active
        /// </summary>
        [Tooltip("If true then the ability will still activate even if the hit prevention is active")]
        public bool castableDuringHitPrevention = false;

        /// <summary>
        /// If true then the ability won't have the activation interrupted due to a hit prevention
        /// </summary>
        [Tooltip("If true then the ability won't have the activation interrupted due to a hit prevention")]
        public bool hitPreventionWontInterruptActivation = false;

        /// <summary>
        /// If true then the ability will activate ignoring all activation restrictions
        /// </summary>
        [Tooltip("If true then the ability will activate ignoring all activation restrictions")]
        public bool forceActivation = false;

        /// <summary>
        /// If true then the force activated ability will interrupt any abilities currently activating
        /// </summary>
        [Tooltip("If true then the force activated ability will interrupt any abilities currently activating")]
        public bool forceActivationInterruptCurrentActivation = false;


        /// <summary>
        /// If true then the ability will always collide even if the entity is currently ignoring ability collision
        /// </summary>
        [Tooltip("If true then the ability will always collide even if the entity is currently ignoring ability collision")]
        public bool overrideIgnoreAbilityCollision = false;

        /// <summary>
        /// If enabled then the ability will ignore any weapon parrying (Melee abilities only)
        /// </summary>
        [Tooltip("If enabled then the ability will ignore any weapon parrying (Melee abilities only)")]
        public bool overrideWeaponParrying = false;

        /// <summary>
        /// If enabled then the ability will ignore any weapon blocking, stopping the entity hit from blocking 
        /// </summary>
        [Tooltip("If enabled then the ability will ignore any weapon blocking, stopping the entity hit from blocking ")]
        public bool overrideWeaponBlocking = false;

        /// <summary>
        /// If ticked then the block durability will be decreased on the entity that blocks this ability
        /// </summary>
        [Tooltip("If ticked then the block durability will be decreased on the entity that blocks this ability")]
        public bool reduceWeaponBlockDurability = true;


        /// <summary>
        /// If true then then this will make the entity animate from this ability hitting if entity is setup to animate from hits
        /// </summary>
        [Tooltip("If true then then this will make the entity animate from this ability hitting if entity is setup to animate from hits")]
        public bool activateAnimationFromHit = true;

        /// <summary>
        /// Delay before the hit animation from ability hit is activated
        /// </summary>
        [Tooltip("Delay before the hit animation from ability hit is activated")]
        public float activateAnimationFromHitDelay = 0f;

        /// <summary>
        /// If true then any air animations will be used if it exists 
        /// </summary>
        [Tooltip("If true then any air animations will be used if it exists")]
        public bool activateAnimationFromHitUseAirAnimation = false;

        /// <summary>
        /// If true then a specific animation can be set to activate
        /// </summary>
        public bool activateSpecificHitAnimation = false;

        /// <summary>
        /// Animation to activate on hit 
        /// </summary>
        public string hitAnimationToActivate = "";

        /// <summary>
        /// If true then an animation clip will be played instead of an ABC hit animation
        /// </summary>
        [Tooltip("If true then an animation clip will be played instead of an ABC hit animation")]
        public bool activateSpecificHitAnimationUseClip = false;

        /// <summary>
        /// Animation clip to activate on hit
        /// </summary>
        [Tooltip("Animation Clip to activate on hit")]
        public ABC_AnimationClipReference hitAnimationClipToActivate;

        /// <summary>
        /// Locks the X axis when travel type is mouse forward 
        /// </summary>
        public bool mouseForwardLockX = false;

        /// <summary>
        /// Locks the Y axis when travel type is mouse forward 
        /// </summary>
        public bool mouseForwardLockY = false;

        /// <summary>
        /// Locks the Z axis when travel type is mouse forward 
        /// </summary>
        public bool mouseForwardLockZ = false;

        /// <summary>
        /// Used for Mouse Target and Mouse Forward. If true then the target position will always be infront of the player. If false then player will fire all around in world position.
        /// </summary>
        [Tooltip("Used for Mouse Target and Mouse Forward. If true then the target position will always be infront of the player. If false then player will fire all around in world position.")]
        public bool mouseFrontOnly = false;

        /// <summary>
        /// if no travel is selected then do we move with the caster good for breath attacks
        /// </summary>
        [Tooltip("Used for no travel. If true then the ability will move with the caster as a child object. (good for breath attacks)")]
        public bool travelWithCaster = false;

        /// <summary>
        /// Determines if force or velocity is used for the Ability movement
        /// </summary>
        [Tooltip("Determine if force or velocity is used on Ability movement")]
        public TravelPhysics travelPhysics;

        /// <summary>
        /// If true then the ability will fire forward if no target has been selected
        /// </summary>
        [Tooltip("If no selected target does the ability just fire forward from character")]
        public bool noTargetStillTravel = true;

        /// <summary>
        /// If no target is currently selected then the ability will activate on the soft target 
        /// </summary>
        [Tooltip("If no selected target then the ability will as backup fire to the softTarget")]
        public bool auxiliarySoftTarget = true;

        /// <summary>
        /// If no target has been selected for a starting position then the ability will start on the soft target
        /// </summary>
        [Tooltip("If no starting position target selected then the ability will as backup fire to the softTarget")]
        public bool startingPositionAuxiliarySoftTarget = false;

        /// <summary>
        /// If true then the ability will travel to the target, else it will automatically start at the targets location
        /// </summary>
        [Tooltip("Will the projectile travel or just appear at target")]
        public bool targetTravel = true;

        /// <summary>
        /// If true then the Entity that activated the ability has to be facing the target
        /// </summary>
        [Tooltip("Does the caster have to be facing the target")]
        public bool targetFacing;

        /// <summary>
        /// If true then the ability projectile will always travel to the targets position, 
        /// else if false the projectile will start travelling towards the target but then keep going in that direction no matter if the target moves
        /// </summary>
        [Tooltip("If true then the ability projectile will always travel to the targets position, else if false the projectile will start travelling towards the target but then keep going in that direction no matter if the target moves ")]
        public bool continuouslyTurnToDestination = true;

        /// <summary>
        /// When true the user will have to pick a target after the Ability is activated.
        /// </summary>
        [Tooltip("When true the user will have to pick a target after the Ability is activated.")]
        public bool abilityBeforeTarget;


        /// <summary>
        /// When operating under ability before target, the ability will wait till a target is found to activate. If false the activation will be interrupted if wrong target found
        /// </summary>
        [Tooltip("When operating under ability before target, the ability will wait till a target is found to fire. If false the cast will cancel if wrong target found")]
        public bool loopTillTargetFound = true;

        /// <summary>
        /// The image indicator which shows when ability before target is setup on an ability. Displays the range in which a world/selected target can be selected.
        /// </summary>
        [Tooltip("Image that shows when in range")]
        public ABC_TextureReference abilityRangeIndicatorImage;

        /// <summary>
        /// The image indicator which shows when ability before target is setup on an ability. Displays the  indicator in which a worldtarget can be selected.
        /// </summary>
        [Tooltip("Image that shows when in range")]
        public ABC_TextureReference abilityWorldTargetIndicatorImage;

        /// <summary>
        /// The image indicator which shows when ability before target is setup on an ability. Displays a graphic from the character towards the mouse position
        /// </summary>
        [Tooltip("Image that shows when in range")]
        public ABC_TextureReference abilityMouseTargetIndicatorImage;

        /// <summary>
        /// If true then an indicator will appear showing where the ability will hit and the radius of collision 
        /// </summary>
        [Tooltip("When true the ability before target indicator will indicator the effect radius and who any splash effects will hit.")]
        public bool abilityBeforeTargetWorldIndicatorScaleToEffectRadius = true;

        /// <summary>
        /// The scale of the indicator if not automatically being set to effect radius
        /// </summary>
        [Tooltip("The scale of the indicator if not automatically being set to effect radius")]
        public float abilityBeforeTargetWorldIndicatorScale = 3f;

        /// <summary>
        /// The length of the mouse target indicator
        /// </summary>
        [Tooltip("The length of the mouse target indicator")]
        public float abilityBeforeTargetMouseTargetIndicatorLength = 7f;


        /// <summary>
        /// How long to wait before the Ability will travel towards the target
        /// </summary>
        [Tooltip("How long to wait before the Ability will travel towards the target.")]
        public float seekTargetDelay;

        /// <summary>
        /// If true then ability will only activate on selected tags
        /// </summary>
        [Tooltip("Turns on restricted casting so it will only fire on selected tags.")]
        public bool selectedTargetRestrictTargets;

        /// <summary>
        /// If ability is restricting targets in Selected Target mode then activation will only occur if the target matches a tag in this list
        /// </summary>
        [Tooltip("In Selected Target mode this ability will only fire if the target matches a tag in this list.")]
        public List<string> selectedTargetOnlyCastOnTag;

        /// <summary>
        /// If false the Ability will effect any object on the way to target, else will only effect the target
        /// </summary>
        [Tooltip("If false the Ability will effect any object on the way to target.")]
        public bool affectOnlyTarget;

        /// <summary>
        /// If true then the entity will rotate to the selected target when activating
        /// </summary>
        [Tooltip("If true then caster will rotate to selected target")]
        public bool rotateToSelectedTarget;

        /// <summary>
        /// Melee abilities only, if true then the originator will keep rotating towards the selected target until the melee is over
        /// </summary>
        [Tooltip("Melee abilities only, if true then the originator will keep rotating towards the selected target until the melee ability is over")]
        public bool meleeKeepRotatingToSelectedTarget;

        /// <summary>
        /// The behaviour in which the entity rotates when not rotating to the current target
        /// </summary>
        [Tooltip("The behaviour in which the entity rotates when not rotating to the current target")]
        public AbilityNoTargetRotateBehaviour noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CurrentDirection;

        /// <summary>
        /// Melee only - If true then originator will immediatly stop the melee attack when hit prevention is toggled on - the projectile will be destroyed and the animation will stop
        /// </summary>
        [Tooltip("Melee only - If true then originator will immediatly stop the melee attack when hit prevention is toggled on - the projectile will be destroyed and the animation will stop")]
        public bool hitsStopMeleeAttack = true;


        /// <summary>
        /// holds the custom travel script if travel type is set to custom. This script will be applied instead of the usual ABC Projectile Travel component
        /// </summary>
        [Tooltip("Add custom travel script")]
        public ABC_ObjectReference customTravelScript;

        /// <summary>
        /// A list of tags which the ability will search for to travel too when set to Nearest Tag travel type
        /// </summary>
        [Tooltip("Holds the tag which the ability will find and travel too when set to nearest tag travel type")]
        public List<string> travelNearestTagList = new List<string>();

        /// <summary>
        /// The range in which the ability can search for nearest tag when set to Nearest Tag travel type
        /// </summary>
        [Tooltip("The range in which the ability can search for nearest tag when set to Nearest Tag travel type")]
        public float travelNearestTagRange = 20f;

        /// <summary>
        /// If true then when searching for the nearest tag to travel to the activating entity (originator) will be ignored
        /// </summary>
        [Tooltip("If true then when searching for the nearest tag to travel to the activating entity (originator) will be ignored")]
        public bool travelNearestTagIgnoreOriginator = true;

        /// <summary>
        /// If true then when searching for the nearest tag to travel the list of potential targets will be randomised
        /// </summary>
        [Tooltip("If true then when searching for the nearest tag to travel the list of potential targets will be randomised")]
        public bool travelNearestTagRandomiseSearch = true;

        /// <summary>
        /// The radius of the raycast when travel type is crosshair
        /// </summary>
        [Tooltip("How big is the radius on the raycasting when in FPS mode.")]
        public float crossHairRaycastRadius = 1f;

        /// <summary>
        /// If true then the ray cast will always get the distance point x units away instead of looking for points on nearest objects.
        /// </summary>
        [Tooltip("If true then the ray cast will always get the distance point x units away instead of looking for points on objects hit by the cast")]
        public bool crossHairRaycastReturnDistancePointOnly = false;

        /// <summary>
        /// The distance point along the raycast which is returned if no objects have been hit or ability has been configured to always return a distance point only
        /// </summary>
        [Tooltip("The distance point along the raycast which is returned if no objects have been hit or ability has been configured to always return a distance point onl ")]
        public float crossHairRaycastReturnedDistance = 30f;

        /// <summary>
        /// If true then centerScreenFPS travel type will record position when ability first activates, otherwise it is done just before the ability is created and set off
        /// </summary>
        [Tooltip("Do we record crosshair position when the ability activates or when the ability is initiated after preparing etc")]
        public bool crossHairRecordTargetOnActivation = false;

        /// <summary>
        /// Limits how many of this Ability is active at once in game, destroying the oldest when a new one is created.
        /// </summary>
        [Tooltip("Limits how many of this Ability is active at once in game")]
        public bool limitActiveAtOnce;

        /// <summary>
        /// If true then an event will be raised when the ability is activated 
        /// </summary>
        [Tooltip("If true then an event will be raised when the ability is activated")]
        public bool abilityActivationRaiseEvent = false;

        /// <summary>
        /// If true then an event will be raised when the ability has finished activation, depending on the type of event 
        /// </summary>
        [Tooltip("If true then an event will be raised when the ability has finished activation, depending on the type of event ")]
        public bool abilityActivationCompleteRaiseEvent = false;

        /// <summary>
        /// Determines when the activation complete event will be raised, either after a delay, after initiation or when ability is destroyed
        /// </summary>
        [Tooltip("Determines when the activation complete event will be raised, either after a delay, after initiation or when ability is destroyed")]
        public AbilityActivationCompleteEventType abilityActivationCompleteEventType = AbilityActivationCompleteEventType.AbilityInitiated;


        /// <summary>
        /// The number of abilities allowed to be active at once. 
        /// </summary>
        [Tooltip("How many of this Ability is allowed to be active in game at once.")]
        public int maxActiveAtOnce;

        /// <summary>
        /// If true the Ability will turn into a scroll ability. This means it can't activate from a Key but from scrolling through abilities and pressing the scroll fire button
        /// </summary>
        [Tooltip("If true the Ability will turn into a scroll ability. This means it can't fire from a Key but from scrolling through abilities and pressing the 1 fire button")]
        public bool scrollAbility = false;

        /// <summary>
        /// If true then an event will be raised when the Scroll ability is set and unset informing subscribers that a scroll ability has been initialised ('equipped') or deinitialised ('unequipped')
        /// </summary>
        [Tooltip("If true then an event will be raised when the Scroll ability is set and unset informing subscribers that a scroll ability has been initialised ('equipped') or deinitialised ('unequipped')")]
        public bool scrollSetUnsetRaiseEvent = false;

        /// <summary>
        /// type of input for jumping to scroll ability
        /// </summary>
        [Tooltip("type of input for jumping to scroll ability")]
        public InputType scrollQuickInputType;

        /// <summary>
        /// The Button Name to jump to the ability
        /// </summary>
        [Tooltip("The Button Name to jump to the ability")]
        public string scrollQuickButton;

        /// <summary>
        /// Key to quickly jump to the ability
        /// </summary>
        [Tooltip("Key to quickly jump to the ability")]
        public KeyCode scrollQuickKey;

        /// <summary>
        /// How long it takes for scroll abilities to switch, no abilities can be activated whilst the switch takes place
        /// </summary>
        [Tooltip("How long it takes to swap abilities. No abilities can be casted whilst swapping")]
        public float scrollSwapDuration = 1;

        /// <summary>
        /// The type of trigger input = normal key press, input combo requires a combination of inputs (F, F, B etc)
        /// </summary>
        [Tooltip("How long it takes to swap abilities. No abilities can be casted whilst swapping")]
        public TriggerType triggerType = TriggerType.Input;

        /// <summary>
        /// A list of keycodes which need to be inputted one after another to trigger the ability
        /// </summary>
        [Tooltip("A list of keycodes which need to be inputted one after another to trigger the ability")]
        public List<KeyCode> keyInputCombo = new List<KeyCode>();

        /// <summary>
        /// Type of input to click for target
        /// </summary>
        [Tooltip("type of input to trigger ability")]
        public InputType keyInputType;

        /// <summary>
        /// Button name to click for target 
        /// </summary>
        [Tooltip("The Button Name to trigger ability")]
        public string keyButton;

        /// <summary>
        /// Used for global abilities, can pick to override the key trigger locally
        /// </summary>
        [Tooltip("Used for global abilities, can pick to override the key trigger locally")]
        public bool globalAbilityOverrideKeyTrigger = false;

        /// <summary>
        /// Key to click for ability trigger
        /// </summary>
        [Tooltip("Key to click for ability trigger")]
        public KeyCode key;

        /// <summary>
        /// If true then ability can activate on key press
        /// </summary>
        [Tooltip("If true then ability can activate on key press")]
        public bool onKeyPress = true;

        /// <summary>
        /// If true then ability can activate on key down
        /// </summary>
        [Tooltip("Key to click for ability trigger")]
        public bool onKeyDown = false;


        /// <summary>
        /// If true then movement will be validated to activate the ability
        /// </summary>
        [Tooltip("If true then movement will be validated to activate the ability")]
        public bool enableMovementValidation = false;

        /// <summary>
        /// If true then movement key needs to be held down for ability to activate
        /// </summary>
        [Tooltip("If true then movement key needs to be held down for ability to activate")]
        public AbilityMovementValidation movementValidation = AbilityMovementValidation.TriggerOnlyIfMoving; 


        /// <summary>
        /// If true then the ability can only be triggered by pressing both the original key input and this additional key input  
        /// </summary>
        [Tooltip("If true then the ability can only be triggered by pressing both the original key input and this additional key input  ")]
        public bool requireAdditionalKeyInput = false;

        /// <summary>
        /// additional input type to trigger ability
        /// </summary>
        [Tooltip("type of input to trigger ability")]
        public InputType additionalKeyInputType;

        /// <summary>
        /// Additional Button name to trigger ability
        /// </summary>
        [Tooltip("The Button Name to trigger ability")]
        public string additionalKeyButton;

        /// <summary>
        /// Additional Key name to trigger ability
        /// </summary>
        [Tooltip("Additional Key name to trigger ability")]
        public KeyCode additionalKey;

        /// <summary>
        /// If true then additional trigger can activate ability on key press
        /// </summary>
        [Tooltip("If true then additional trigger can activate ability on key press")]
        public bool additionalOnKeyPress = true;

        /// <summary>
        /// If true then additional trigger can activate ability on key down
        /// </summary>
        [Tooltip("If true then additional trigger can activate ability on key down")]
        public bool additionalOnKeyDown = false;

        /// <summary>
        /// If true then ability will use ammo when activating
        /// </summary>
        [Tooltip("Does this ability rely on Ammo")]
        public bool UseAmmo = false;

        /// <summary>
        /// If true then the ability will use the current equipped weapons ammo
        /// </summary>
        [Tooltip("If true then the ability will use the current equipped weapons ammo")]
        public bool useEquippedWeaponAmmo = false;

        /// <summary>
        /// If true then the ability will reduce ammo each second whilst active
        /// </summary>
        [Tooltip("If true then the ability will reduce ammo each second whilst active")]
        public bool reduceAmmoWhilstActive = false;

        /// <summary>
        /// The current ammo count for the ability 
        /// </summary>
        [Tooltip("How much ammo we have")]
        public int ammoCount = 100;

        /// <summary>
        /// If ability is a scrollability does it require reloading
        /// </summary>
        [Tooltip("Use ammo and reload")]
        public bool useReload = true;

        /// <summary>
        /// size of clip before reload needed
        /// </summary>
        [Tooltip("size of clip before reload needed")]
        public int clipSize = 50;

        /// <summary>
        /// How long it takes to reload the ability, filling the clip
        /// </summary>
        [Tooltip("How long it takes to reload the ability, filling the clip")]
        public float reloadDuration = 2;

        /// <summary>
        /// stops any abilities from being activated for the duration when reloading starts
        /// </summary>
        [Tooltip("stops any abilities from being activated for the duration when reloading starts")]
        public float reloadRestrictAbilityActivationDuration = 1f;

        /// <summary>
        /// Will automatically reload the ability when required
        /// </summary>
        [Tooltip("Will automatically reload the ability when required")]
        public bool autoReloadWhenRequired = true;

        /// <summary>
        /// If true then every reload duration set the clip size will increase by 1 (like adding shotgun shells)
        /// </summary>
        [Tooltip("If true then every reload duration set the clip size will increase by 1 (like adding shotgun shells)")]
        public bool reloadFillClip = false;

        /// <summary>
        /// If true then the reload graphic will repeat every time the clip is added too
        /// </summary>
        [Tooltip("If true then the reload graphic will repeat every time the clip is added too")]
        public bool reloadFillClipRepeatGraphic = true;

        /// <summary>
        /// Recast time for the ability. How long the entity needs to wait before the ability can be activated again. 
        /// </summary>
        [Tooltip("Recast of the Ability")]
        public float abilityRecast;

        /// <summary>
        /// If true then the recast counter won't start until the ability has been disabled
        /// </summary>
        [Tooltip("The recast counter won't start until the ability has finished")]
        public bool startRecastAfterAbilityEnd = false;

        /// <summary>
        /// How fast the ability projectile will move
        /// </summary>
        [Range(0f, 300f)]
        [Tooltip("How fast the ability projectile will move")]
        public float travelSpeed = 20f;

        /// <summary>
        /// Duration of the ability - how long till it is disabled
        /// </summary>
        [Tooltip("How long till Ability is destroyed")]
        public float duration = 10f;


        /// <summary>
        /// Toggle modes. On/Off will make ability activate and deactivate on button clicks. Hold will make ability active whilst key is pressed
        /// </summary>
        [Tooltip("Toggle modes. On/Off will make ability activate and deactivate on button clicks. Hold will make ability active whilst key is pressed")]
        public AbilityToggle abilityToggle = AbilityToggle.Off;

        /// <summary>
        /// If true then other abilities can be activated whilst this ability has been toggled on
        /// </summary>
        [Tooltip("can other abilities be activated whilst this ability is toggled")]
        public bool canCastWhenToggled = true;

        /// <summary>
        /// Initating Animation will repeat whilst the spell is toggled
        /// </summary>
        [Tooltip("Initating Animation will stay on whilst the spell is toggled")]
        public bool repeatInitiatingAnimationWhilstToggled = true;

        /// <summary>
        /// If true then ability will bounce to other targets on collision
        /// </summary>
        [Tooltip("Activates bounce mode on the ability")]
        public bool bounceMode = false;

        /// <summary>
        /// How many times the ability will bounce
        /// </summary>
        [Tooltip("how many times the ability will bounce")]
        public int bounceAmount = 3;

        /// <summary>
        /// The range in which another target has to be in for the ability to bounce too
        /// </summary>
        [Tooltip("The range in which another target has to be in for the ability to bounce too")]
        public float bounceRange = 20;

        /// <summary>
        /// Type of target for ability to bounce too: NearestABCStateManager, NearestObject, NearestTag
        /// </summary>
        [Tooltip("Type of target for Ability to bounce too after first hit")]
        public BounceTarget bounceTarget;

        /// <summary>
        /// If true then the ability can bounce on the entity that activated it
        /// </summary>
        [Tooltip("Can the ability bounce to caster")]
        public bool bounceOnCaster = false;

        /// <summary>
        /// If true then the next bounce targets will be more random
        /// </summary>
        [Tooltip("Will be more random when selecting the next bounce target")]
        public bool enableRandomBounce = true;

        /// <summary>
        /// If enabled then the ability bouncing will only start if the ability collides with specific tags, else it will start bouncing on first collision
        /// </summary>
        [Tooltip("If enabled then the ability bouncing will only start if the ability collides with specific tags, else it will start bouncing on first collision")]
        public bool startBounceTagRequired = false;

        /// <summary>
        /// List of tags inithat the ability can bounce too if the bounce target is set to nearest tag
        /// </summary>
        [Tooltip("List of tags inithat the ability can bounce too if the bounce target is set to nearest tag")]
        public List<string> startBounceRequiredTags;

        /// <summary>
        /// List of tags that the ability can bounce too if the bounce target is set to nearest tag
        /// </summary>
        public List<string> bounceTag;


        /// <summary>
        /// Offset of the bounce position
        /// </summary>
        [Tooltip("Offset on the bounce position")]
        public Vector3 bouncePositionOffset;

        /// <summary>
        /// Forward offset of the starting position
        /// </summary>
        [Tooltip("Forward offset from bounce position")]
        public float bouncePositionForwardOffset = 0f;

        /// <summary>
        /// Right offset of the starting position
        /// </summary>
        [Tooltip("Right offset from bounce position")]
        public float bouncePositionRightOffset = 0f;

        /// <summary>
        /// If true then the ability will activate boomerang mode after activation. Boomerang mode makes the projectile return to the activating entity after a duration
        /// </summary>
        [Tooltip("Activates boomerang mode on the ability")]
        public bool boomerangMode = false;

        /// <summary>
        /// How long before the boomerang ability will start returning to the activating entity
        /// </summary>
        [Tooltip("Time until boomerang starts returning")]
        public float boomerangDelay;

        /// <summary>
        /// List of effects the ability can imply
        /// </summary>
        public List<Effect> effects = new List<Effect>();


#if ABC_GC_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed when effects are applied
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed when effects are applied")]
    public GameCreator.Core.IActionsList gcEffectActionList;

#endif

#if ABC_GC_2_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed when effects are applied
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed when effects are applied")]
    public GameCreator.Runtime.VisualScripting.Actions gc2EffectAction;

#endif

        /// <summary>
        /// If true then the ability will collide OnEnter
        /// </summary>
        public bool onEnter = true;

        /// <summary>
        /// If true then the ability will collide OnStay
        /// </summary>
        public bool onStay = false;

        /// <summary>
        /// The interval between OnStay collisions with the Ability
        /// </summary>
        public float onStayInterval = 0.3f;

        /// <summary>
        /// If true then the ability will collide OnExit
        /// </summary>
        public bool onExit = false;

        /// <summary>
        /// If true then abillity will use particle collision
        /// </summary>
        public bool particleCollision = false;


        /// <summary>
        /// If true then ability collisions will only occur after a key/button is pressed
        /// </summary>
        [Tooltip("If true then collision will only start after a key/button is pressed")]
        public bool enableCollisionAfterKeyPress = false;

        /// <summary>
        ///  input type to enable ability collisions
        /// </summary>
        [Tooltip("input type to enable ability collisions")]
        public InputType enableCollisionAfterKeyInputType;

        /// <summary>
        /// button to press to enable ability collisions
        /// </summary>
        [Tooltip("button to press to enable ability collisions")]
        public string enableCollisionAfterKeyButton;

        /// <summary>
        /// key to press to enable ability collisions
        /// </summary>
        [Tooltip("key to press to enable ability collisions")]
        public KeyCode enableCollisionAfterKey;

        /// <summary>
        /// Determines the starting direction for the ability 
        /// </summary>
        [Tooltip("When initialised what way is the object facing")]
        public Vector3 startingRotation;

        /// <summary>
        /// If true then the euler rotation of the ability will be set to the starting points euler rotation
        /// </summary>
        [Tooltip("If true then the euler rotation of the ability will be set to the starting points euler rotation")]
        public bool setEulerRotation = false;

        /// <summary>
        /// How much drag is applied to the movement of the Ability
        /// </summary>
        [Tooltip("How much drag is applied to the movement of the Ability")]
        public float travelDrag;

        /// <summary>
        /// The starting position for the ability
        /// </summary>
        public StartingPosition startingPosition;


        /// <summary>
        /// Additional starting positions, a duplicate ability will appear for each additional starting position, allowing 
        /// for multiple ability objects to be created at once (on both fists for example)
        /// </summary>
        public List<AdditionalStartingPosition> additionalStartingPositions = new List<AdditionalStartingPosition>();

        /// <summary>
        /// If true then the ability can effect the activating entity
        /// </summary>
        [Tooltip("Does this Ability affect the original caster")]
        public bool affectOriginObject;

        /// <summary>
        /// used for starting position if OnObject is selected. Determines what object the ability will start on
        /// </summary>
        [Tooltip("Starting position of the Ability")]
        public ABC_GameObjectReference startingPositionOnObject;

        /// <summary>
        /// Tag which the ability can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string startingPositionOnTag;

        /// <summary>
        /// Offset of the starting position
        /// </summary>
        [Tooltip("Offset on the starting position")]
        public Vector3 startingPositionOffset;

        /// <summary>
        /// Forward offset of the starting position
        /// </summary>
        [Tooltip("Forward offset from starting position")]
        public float startingPositionForwardOffset = 0f;

        /// <summary>
        /// Right offset of the starting position
        /// </summary>
        [Tooltip("Right offset from starting position")]
        public float startingPositionRightOffset = 0f;

        /// <summary>
        /// Determines how the Ability handle collision with other Abilities
        /// </summary>
        [Tooltip("Determines how this Ability handle collision with other Abilities")]
        public AbilityCollisionIgnores abilityCollisionIgnores;

        /// <summary>
        /// Determines what impact will destroy the ability
        /// </summary>
        [Tooltip("Does Ability destroy on first impact")]
        public ImpactDestroy impactDestroy;

        /// <summary>
        /// Determines what object tags will be ignored when determining if the ability will be destroyed by the impact.
        /// </summary>
        public List<string> destroyIgnoreTag = new List<string>();

        /// <summary>
        /// How long after impact does the Ability get destroyed.
        /// </summary>
        [Tooltip("How long after Impact does Ability destroy")]
        public float destroyDelay;

        /// <summary>
        /// If true then the abilitys will 'Splash', applying effects to entities around where the ability was destroyed.
        /// </summary>
        [Tooltip("If true then the abilitys will 'Splash' applying effects to entities around where the ability was destroyed.")]
        public bool useDestroySplashEffect;

        /// <summary>
        /// Radius of the splash effect
        /// </summary>
        [Tooltip("Radius of the splash damage")]
        public float destroySplashRadius;

        /// <summary>
        /// If true then on destroy/collision the ability will imply an explosion force to near by objects
        /// </summary>
        [Tooltip("Does the splash cause an explosion moving near by objects affected")]
        public bool destroySplashExplosion = false;

        /// <summary>
        /// Radius of the splash explosion
        /// </summary>
        [Tooltip("Radius of splash explosion")]
        public float destroySplashExplosionRadius = 5f;

        /// <summary>
        /// Potency of the splash explosion
        /// </summary>
        [Tooltip("Power of splash explosion")]
        public float destroySplashExplosionPower = 5f;

        /// <summary>
        /// Upwards potency of the splash explosion
        /// </summary>
        [Tooltip("upwards lift of explosion")]
        public float destroySplashExplosionUplift = 3f;

        /// <summary>
        /// If true then only objects with specific tags can be affected by the splash explosion
        /// </summary>
        [Tooltip("limits which tags can be affected")]
        public bool destroySplashExplosionTagLimit = false;

        /// <summary>
        /// Limits by tag which objects can be affected by the splash explosion
        /// </summary>
        [Tooltip("Limit who can get the explosion affect")]
        public List<string> destroySplashExplosionAffectTag = new List<string>();

        /// <summary>
        /// If true then game speed will be modified on initiation
        /// </summary>
        [Tooltip("If true then game speed will be modified on initiation")]
        public bool modifyGameSpeedOnInitiation = false;

        /// <summary>
        /// The speedfactor to slow down or speed up game speed by
        /// </summary>
        [Tooltip("The speedfactor to slow down or speed up game speed by")]
        public float modifyGameSpeedOnInitiationSpeedFactor = 0.05f;

        /// <summary>
        /// Delay before game speed is modified
        /// </summary>
        [Tooltip("Delay before game speed is modified")]
        public float modifyGameSpeedOnInitiationDelay = 0.2f;

        /// <summary>
        /// How long the game speed will be modified for
        /// </summary>
        [Tooltip("How long the game speed will be modified for")]
        public float modifyGameSpeedOnInitiationDuration = 0.5f;


        /// <summary>
        /// If true then game speed will be modified on impact
        /// </summary>
        [Tooltip("If true then game speed will be modified on impact")]
        public bool modifyGameSpeedOnImpact = false;

        /// <summary>
        /// The speedfactor to slow down or speed up game speed by
        /// </summary>
        [Tooltip("The speedfactor to slow down or speed up game speed by")]
        public float modifyGameSpeedOnImpactSpeedFactor = 0.05f;

        /// <summary>
        /// How long the game speed will be modified for
        /// </summary>
        [Tooltip("How long the game speed will be modified for")]
        public float modifyGameSpeedOnImpactDuration = 0.5f;

        /// Delay before game speed is modified
        /// </summary>
        [Tooltip("Delay before game speed is modified")]
        public float modifyGameSpeedOnImpactDelay = 0.2f;


        /// <summary>
        /// If enabled then hit stop will be activated on impact
        /// </summary>
        [Tooltip("If enabled then hit stop will be activated on impact")]
        public bool enableHitStopOnImpact = false;

        /// <summary>
        /// The delay after ability collides till hitstop is activated
        /// </summary>
        [Tooltip("The delay after ability collides till hitstop is activated")]
        public float hitStopOnImpactDelay = 0f;

        /// <summary>
        /// Duration of the hitstop
        /// </summary>
        [Tooltip("Duration of the hitstop")]
        public float hitStopOnImpactDuration = 0.1f;


        /// <summary>
        /// the delay before the entity hit activates hit stio
        /// </summary>
        [Tooltip("Determines if the ability is currently frozen in a hit stop ")]
        public float hitStopOnImpactEntityHitDelay = 0.2f;


        /// <summary>
        /// Determines if the ability is currently frozen in a hit stop 
        /// </summary>
        [Tooltip("Determines if the ability is currently frozen in a hit stop ")]
        public bool hitStopCurrentlyActive = false;

        /// <summary>
        /// Tracks how long the ability was in the hit stop state for
        /// </summary>
        /// <remarks>This duration is added onto various parts of the system to ensure the timings are all in sync
        /// since hitstop freezes an ability for a small amount of time the duration/combo/activation intervals add this property on to compensate for the amount of time
        /// ability was stopped for. This will be reset once used by the ability</remarks>
        [Tooltip("Tracks how long the ability was in the hit stop state for")]
        public float hitStopCurrentTotalExtendDuration = 0f;


        /// <summary>
        /// List of tags that need to be either activating the ability or included in the ability collision to activate global impacts (shake camera etc)
        /// </summary>
        [Tooltip("List of tags that need to be either activating the ability or included in the ability collision to activate global impacts (shake camera etc)")]
        public List<string> globalImpactRequiredTag = new List<string>();

        /// <summary>
        /// If true then camera will shake on initiation
        /// </summary>
        [Tooltip("If true then camera will shake on impact")]
        public bool shakeCameraOnInitiation = false;

        /// <summary>
        /// Delay before shake camera on initiation starts
        /// </summary>
        [Tooltip("Delay before shake camera on impact starts")]
        public float shakeCameraOnInitiationDelay = 0f;

        /// <summary>
        /// Duration to shake camera for on initiation
        /// </summary>
        [Tooltip("Duration to shake camera for on impact")]
        public float shakeCameraOnInitiationDuration = 0.5f;

        /// <summary>
        /// Amount to shake camera for on initiation
        /// </summary>
        [Tooltip("Amount to shake camera for on impact")]
        public float shakeCameraOnInitiationAmount = 0.5f;

        /// <summary>
        /// The shake speed for the camera on initiation
        /// </summary>
        [Tooltip("The shake speed for the camera on impact")]
        public float shakeCameraOnInitiationSpeed = 25f;


        /// <summary>
        /// If true then camera will shake on impact
        /// </summary>
        [Tooltip("If true then camera will shake on impact")]
        public bool shakeCameraOnImpact = false;

        /// <summary>
        /// Delay before shake camera on impact starts
        /// </summary>
        [Tooltip("Delay before shake camera on impact starts")]
        public float shakeCameraOnImpactDelay = 0f;

        /// <summary>
        /// Duration to shake camera for on impact
        /// </summary>
        [Tooltip("Duration to shake camera for on impact")]
        public float shakeCameraOnImpactDuration = 0.5f;

        /// <summary>
        /// Amount to shake camera for on impact
        /// </summary>
        [Tooltip("Amount to shake camera for on impact")]
        public float shakeCameraOnImpactAmount = 0.5f;

        /// <summary>
        /// The shake speed for the camera on impact
        /// </summary>
        [Tooltip("The shake speed for the camera on impact")]
        public float shakeCameraOnImpactSpeed = 25f;

        /// <summary>
        /// If true then entity will be pushed back on impact
        /// </summary>
        [Tooltip("If true then entity will be pushed back on impact")]
        public bool pushEntityOnImpact = false;

        /// <summary>
        /// Delay before push is applied to the entity 
        /// </summary>
        [Tooltip("Delay before push is applied to the entity ")]
        public float pushEntityOnImpactDelay = 0.1f;

        /// <summary>
        /// How much push is applied to entity on impact
        /// </summary>
        [Tooltip("How much push is applied to entity on impact")]
        public float pushEntityOnImpactAmount = 1.5f;

        /// <summary>
        /// How much lift force is applied to entity on impact
        /// </summary>
        [Tooltip("How much lift force is applied to entity on impact")]
        public float pushEntityOnImpactLiftForce = 0f;

        /// <summary>
        /// If true then entity will defy gravity on impact
        /// </summary>
        [Tooltip("If true then entity will defy gravity on impact")]
        public bool defyEntityGravityOnImpact = false;

        /// <summary>
        /// the delay before gravity is defied on impact
        /// </summary>
        [Tooltip("the delay before gravity is defied on impact")]
        public float defyEntityGravityOnImpactDelay = 0.1f;

        /// <summary>
        /// the duration the entity will defy gravity on impact
        /// </summary>
        [Tooltip("the duration the entity will defy gravity on impact")]
        public float defyEntityGravityOnImpactDuration = 1.5f;

        /// <summary>
        /// If true then camera will shake on impact
        /// </summary>
        [Tooltip("If true then camera will shake on impact")]
        public bool shakeEntityOnImpact = false;

        /// <summary>
        /// The amount of shake performed 
        /// </summary>
        [Tooltip("The amount of shake performed")]
        public float shakeEntityOnImpactShakeAmount = 0.1f;

        /// <summary>
        /// The amount of the shake decreased each cycle. Also determines the duration, once shake decays to 0 the shake will stop
        /// </summary>
        [Tooltip("The amount of the shake decreased each cycle. Also determines the duration, once shake decays to 0 the shake will stop")]
        public float shakeEntityOnImpactShakeDecay = 0.007f;

        /// <summary>
        /// The delay before the shake starts
        /// </summary>
        [Tooltip("The delay before the shake starts")]
        public float shakeEntityOnImpactShakeDelay = 0f;


        /// <summary>
        /// If true then the ability will attach to the first object hit on impact
        /// </summary>
        [Tooltip("If true then the ability will attach to the first object hit on impact")]
        public bool attachToObjectOnImpact = false;

        /// <summary>
        /// Minimum probability of the ability attaching to object on impact. If the dice roll is higher then the minimum and lower then the maximum then it will attach. 
        /// </summary>
        [Range(0f, 100f)]
        [Tooltip("Minimum probability of the ability attaching to object on impact. If the dice roll is higher then the minimum and lower then the maximum then it will attach. ")]
        public float attachToObjectProbabilityMinValue = 0f;

        /// <summary>
        /// Maximum probability of the ability attaching to object on impact. If the dice roll is higher then the minimum and lower then the maximum then it will attach.
        /// </summary>
        [Range(0f, 100f)]
        [Tooltip("Maximum probability of the ability attaching to object on impact. If the dice roll is higher then the minimum and lower then the maximum then it will attach.")]
        public float attachToObjectProbabilityMaxValue = 100f;


        /// <summary>
        /// How much the ability will stick out from the object it attaches to
        /// </summary>
        [Tooltip("How much the ability will stick out from the object it attaches to")]
        public float attachToObjectStickOutFactor = 0.5f;

        /// <summary>
        /// If true then the ability will attach to the nearest bone
        /// </summary>
        [Tooltip("If true then the ability will attach to the nearest bone")]
        public bool attachToObjectNearestBone = false;

        /// <summary>
        /// Will activate a color switch for a duration before reverting back to the entites original color
        /// </summary>
        [Tooltip("Will activate the color switch for a duration before reverting back to the objects original color")]
        public bool switchColorOnImpact = false;

        /// <summary>
        /// Color to switch too for the duration
        /// </summary>
        [Tooltip("Color to switch too for the duration")]
        public Color switchColorOnImpactColor = new Color(70, 4, 4);

        /// <summary>
        /// Delay before the switch occurs
        /// </summary>
        [Tooltip("Delay before the switch occurs")]
        public float switchColorOnImpactDelay = 0f;

        /// <summary>
        /// The duration the color will switch to before reverting back
        /// </summary>
        [Tooltip("The duration the color will switch to before reverting back")]
        public float switchColorOnImpactDuration = 0.2f;

        /// <summary>
        /// If true then the emission color is changed (if enabled), else if false color property is changed
        /// </summary>
        [Tooltip("If true then the emission color is changed (if enabled), else if false or emission is not enabled then color property is changed")]
        public bool switchColorOnImpactUseEmission = false;


        /// <summary>
        /// enables for a collider to be added to the ability via code, allows for other functionality to be used (delay etc)
        /// </summary>
        public bool addAbilityCollider = true;

        /// <summary>
        /// If true then collision will be setup to the ability projectile
        /// </summary>
        [Tooltip("Applies collider settings to the Parent")]
        public bool applyColliderSettingsToParent = true;

        /// <summary>
        /// If true then collision will be setup for any child objects of the ability 
        /// </summary>
        [Tooltip("Applies collider settings to the Children")]
        public bool applyColliderSettingsToChildren = false;

        /// <summary>
        /// Applies the ability to a layer
        /// </summary>
        [Tooltip("Which layer does the Ability effect")]
        public LayerMask affectLayer;

        /// <summary>
        /// If true then the radius of the collider uses the graphic renderer of the projectile
        /// </summary>
        [Tooltip("Uses the radius of the graphic renderer rather then setting a specific radius")]
        public bool useGraphicRadius = false;


        /// <summary>
        /// Determines the radius size of the ability collision. Higher the number the larger range the ability will effect
        /// </summary>
        [Tooltip("Collider size of the Ability")]
        public float colliderRadius = 0.5f;

        /// <summary>
        /// Off set of the ability collider
        /// </summary>
        [Tooltip("Offset of the collider")]
        public Vector3 colliderOffset = Vector3.zero;

        /// <summary>
        /// Applies a travel duraction to the Ability. Which will stop it moving after the duration is up"
        /// </summary>
        [Tooltip("Applies a travel duraction to the Ability. Which will stop it moving after the duration is up")]
        public bool applyTravelDuration;


        /// <summary>
        /// List of tags that the originator activating the ability requires for the travel duration to be applied
        /// </summary>
        [Tooltip(" List of tags that the originator activating the ability requires for the travel duration to be applied")]
        public List<string> travelDurationOriginatorTagsRequired = new List<string>();

        /// <summary>
        /// How long ability travels before it stops - not used on selected target. 0 is off
        /// </summary>
        [Tooltip("How long Ability travels before it stops. Not used on selected target. 0 is off")]
        public float travelDurationTime;

        /// <summary>
        /// If true then the ability will stop suddenly, else it will stop gradually
        /// </summary>
        [Tooltip("After the duration does the Ability stop suddenly or gradually")]
        public bool travelDurationStopSuddenly;

        /// <summary>
        /// If true then graphics and animations will activate when ability is initiated
        /// </summary>
        [Tooltip("Show effect and play animation when iniating an Ability")]
        public bool useInitiatingAesthetics;

#if ABC_GC_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed on initiation 
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed on initiation")]
    public GameCreator.Core.IActionsList gcInitiatingActionList;

#endif

#if ABC_GC_2_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed on initiation
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed on initiation")]
    public GameCreator.Runtime.VisualScripting.Actions gc2InitiatingAction;

#endif

        /// <summary>
        /// If true then the initiating animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate initiating animation on the main entities animator")]
        public bool initiatingAnimateOnEntity = true;

        /// <summary>
        /// If true then the initiating animation will activate on the current scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate initiating animation on the current scroll ability's graphic animator")]
        public bool initiatingAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool initiatingAnimateOnWeapon = false;


        /// <summary>
        /// If true then the initiating entity will defy gravity for a short while
        /// </summary>
        [Tooltip("When the ability is casted does the caster defy gravity for a short while.")]
        public bool defyGravityInitiating = false;

        /// <summary>
        /// How long entity will defy gravity for on initiation
        /// </summary>
        [Tooltip("How long the caster defies gravity for")]
        public float defyGravityInitiatingDuration = 2f;

        /// <summary>
        /// Delay before the defy gravity begins
        /// </summary>
        [Tooltip(" Delay before the defy gravity begins")]
        public float defyGravityInitiatingDelay = 0f;


        /// <summary>
        /// If true then the defy gravity event delgates will be invoked when initiating
        /// </summary>
        [Tooltip("If true then the defy gravity event delgates will be invoked when initiating")]
        public bool defyGravityInitiatingRaiseEvent = false;

        /// <summary>
        /// Offset for the initiating graphics
        /// </summary>
        [Tooltip("Offset for the effect graphics")]
        public Vector3 initiatingAestheticsPositionOffset;

        /// <summary>
        /// Forward offset for the initiating graphics
        /// </summary>
        [Tooltip("Forward offset from starting position")]
        public float initiatingAestheticsPositionForwardOffset = 0f;

        /// <summary>
        /// Right offset for the initiating graphics
        /// </summary>
        [Tooltip("Right offset from starting position")]
        public float initiatingAestheticsPositionRightOffset = 0f;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference initiatingAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference initiatingAnimationRunnerMask;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float initiatingAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float initiatingAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float initiatingAnimationRunnerClipDuration = 1f;


        /// <summary>
        /// If true then the initiating animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate initiating animatio on the main entities animation runner")]
        public bool initiatingAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the initiating animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate initiating animation on the current scroll ability's graphic animation runner")]
        public bool initiatingAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool initiatingAnimationRunnerOnWeapon = false;


        /// <summary>
        /// Name of the initiating animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string initiatingAnimatorParameter;

        /// <summary>
        /// Type of parameter for the initiating animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType initiatingAnimatorParameterType;

        /// <summary>
        /// Value to turn on the initiating animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string initiatingAnimatorOnValue;

        /// <summary>
        /// Value to turn off the initiating animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string initiatingAnimatorOffValue;

        /// <summary>
        /// How long the animation will play for
        /// </summary>
        [Tooltip("How long to play animation for ")]
        public float initiatingAnimatorDuration = 0.5f;

        /// <summary>
        /// Moves the activating entity during ability initiation, useful for when no root motion is applied to the animation
        /// </summary>
        [Tooltip("Moves entity when initiating (useful if no root motion applied to animation")]
        public bool moveSelfWhenInitiating = false;

        /// <summary>
        /// Offset to move the activating entity to during initiation
        /// </summary>
        [Tooltip("Offset value to move too")]
        public Vector3 moveSelfInitiatingOffset = Vector3.zero;

        /// <summary>
        /// Forward Offset to move the activating entity to during initiation
        /// </summary>
        [Tooltip("Forward offset to move too")]
        public float moveSelfInitiatingForwardOffset = 0f;

        /// <summary>
        /// Right Offset to move the activating entity to during initiation
        /// </summary>
        [Tooltip("right offset to move too")]
        public float moveSelfInitiatingRightOffset = 0f;

        /// <summary>
        /// Delay before entity will start moving when initiating
        /// </summary>
        [Tooltip("Delay before entity starts moving")]
        public float moveSelfInitiatingDelay = 0f;

        /// <summary>
        /// How long it takes the entity to get to the new position when moving during initiation
        /// </summary>
        [Tooltip("How long it takes entity to get to the new position")]
        public float moveSelfInitiatingDuration = 1f;

        /// <summary>
        ///  Moves the activating entity to the target of the ability during ability initiation, useful for when no root motion is applied to the animation
        ///  If ability doesn't track target (forward travel type etc) then entity will move towards the current target or soft target if current target doesn't exist
        /// </summary>
        [Tooltip("Moves the activating entity to the target of the ability during ability initiation.  " +
            "If ability doesn't track target (forward travel type etc) then entity will move towards the current target or soft target if current target doesn't exist")]
        public bool moveSelfToTargetWhenInitiating = false;

        /// <summary>
        /// The delay before the entity moves towards the target
        /// </summary>
        [Tooltip("The delay before the entity moves towards the target")]
        public float moveSelfToTargetInitiatingDelay = 0f;

        /// <summary>
        /// How long it takes the entity to get to the target when moving during initiation
        /// </summary>
        [Tooltip("How long it takes the entity to get to the target when moving during initiation")]
        public float moveSelfToTargetInitiatingDuration = 0.5f;


        /// <summary>
        /// The distance that the entity will stop infront of target 
        /// </summary>
        [Tooltip("The distance that the entity will stop infront of target")]
        public float moveSelfToTargetInitiatingStopDistance = 5f;

        /// <summary>
        /// The offset applied to the target we are moving too
        /// </summary>
        [Tooltip("The offset applied to the target we are moving too")]
        public Vector3 moveSelfToTargetInitiatingOffset = Vector3.zero;

        /// <summary>
        /// Forward Offset applied to the target we are moving too
        /// </summary>
        [Tooltip("Forward Offset applied to the target we are moving too")]
        public float moveSelfToTargetInitiatingForwardOffset = 0f;

        /// <summary>
        /// Right Offset applied to the target we are moving too
        /// </summary>
        [Tooltip("Right Offset applied to the target we are moving too")]
        public float moveSelfToTargetInitiatingRightOffset = 0f;

        /// <summary>
        /// Graphic object that appears when initiating
        /// </summary>
        [Tooltip("Particle or object that shows when initiating")]
        public ABC_GameObjectReference initiatingGraphic;

        /// <summary>
        /// Sub graphic which appears when initiating, will be a child of the main graphic object
        /// </summary>
        [Tooltip("Sub graphic or object that shows when initiating. Will be child of initiating graphic")]
        public ABC_GameObjectReference initiatingSubGraphic;

        /// <summary>
        /// If true then the initiating aesthetic will activate with the ability
        /// </summary>
        [Tooltip("Sub graphic or object that shows when initiating. Will be child of initiating graphic")]
        public bool initiatingAestheticActivateWithAbility = false;

        /// <summary>
        /// How long the initiating graphic will show for
        /// </summary>
        [Tooltip("How long to show the effect for")]
        public float initiatingAestheticDuration = 2f;

        /// <summary>
        /// the delay before the initiating Aesthetic is shown
        /// </summary>
        [Tooltip("the delay before the initiating Aesthetic is shown")]
        public float initiatingAestheticDelay = 0f;

        /// <summary>
        /// If true then after a delay the initiating graphic will Detach from the parent it's currently attached too
        /// </summary>
        [Tooltip(" If true then after a delay the initiating graphic will Detach from the parent it's currently attached too")]
        public bool initiatingAestheticDetachFromParentAfterDelay = false;

        /// <summary>
        /// the delay before the initiating Aesthetic is Detached
        /// </summary>
        [Tooltip("the delay before the initiating Aesthetic is Detached")]
        public float initiatingAestheticDetachDelay = 0.1f;

        /// <summary>
        /// If true then the weapon trail of the current equipped weapon will be used
        /// </summary>
        [Tooltip("If true then the weapon trail of the current equipped weapon will be used")]
        public bool initiatingUseWeaponTrail = false;

        /// <summary>
        /// Determines which weapon graphic the weapon trail will activate on
        /// </summary>
        [Tooltip("Determines which weapon graphic the weapon trail will activate on")]
        public int initiatingWeaponTrailGraphicIteration = 0;


        /// <summary>
        /// After initiating the ability/raycast will appear in game after a delay or at a defined initiating animation percentage (0-100%) depending on the type selected
        /// </summary>
        [Tooltip("After initiating the ability/raycast will appear in game after a delay or at a defined initiating animation percentage (0-100%) depending on the type selected")]
        public AbilityInitiationDelayType intiatingProjectileDelayType = AbilityInitiationDelayType.AfterDelay;

        /// <summary>
        /// the delay after initiating until the projectile is created. Allows time for animation/effect to play out correctly
        /// </summary>
        [Tooltip("the delay after initiating until the projectile is created. Allows time for animation/effect to play out correctly")]
        public float delayBetweenInitiatingAndProjectile;

        /// <summary>
        /// After initiation the ability/raycast will apper in game when the initiating animation reaches the percentage defined in this setting
        /// </summary>
        [Range(0, 100)]
        [Tooltip("After initiation the ability/raycast will apper in game when the initiating animation reaches the percentage defined in this setting")]
        public float initiatingProjectileDelayAnimationPercentage = 30;


        /// <summary>
        /// starting position for the intiating graphic
        /// </summary>
        [Tooltip("where the initiating graphic starts")]
        public StartingPosition initiatingStartPosition;

        /// <summary>
        /// Object for initiating graphic to start on if OnObject is selected for the start position
        /// </summary>
        public ABC_GameObjectReference initiatingPositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string initiatingPositionOnTag;

        /// <summary>
        /// If true then collision will be enabled after a delay
        /// </summary>
        [Tooltip("Add collider after a delay (bomb effect)")]
        public bool colliderTimeDelay;

        /// <summary>
        /// Delay time until the collision is enabled on the ability 
        /// </summary>
        [Tooltip("Time to wait before collider is enabled")]
        public float colliderDelayTime;

        /// <summary>
        /// If true then collision will be enabled after a key is pressed
        /// </summary>
        [Tooltip("Turn on collider after a key is pressed (remote bomb)")]
        public bool colliderKeyPressDelay;

        /// <summary>
        /// Type of input to enable the collider 
        /// </summary>
        [Tooltip("type of input to enable collider")]
        public InputType colliderDelayInputType;

        /// <summary>
        /// Button name to enable the collider
        /// </summary>
        [Tooltip("The Button name to enable collider")]
        public string colliderDelayButton;

        /// <summary>
        /// Key to enable the collider
        /// </summary>
        [Tooltip("What button to pressed to enable collider")]
        public KeyCode colliderDelayKey;


        /// <summary>
        /// If true then ability will ignore the active terrain
        /// </summary>
        [Tooltip("Does Ability ignore the active Terrain")]
        public bool ignoreActiveTerrain = true;


        /// <summary>
        /// If true then IK will persist whilst the ability is activating
        /// </summary>
        [Tooltip("If true then IK will persist whilst the ability is activating")]
        public bool persistIK = false;

        /// <summary>
        /// If true then the ability will create an object when it collides or gets destroyed
        /// </summary>
        [Tooltip("Spawn an object where the Ability collides or gets destroyed")]
        public bool spawnObject = false;

        /// <summary>
        /// If true then the ability will create an object when destroyed
        /// </summary>
        [Tooltip("Spawn the object when Ability is destroyed")]
        public bool spawnObjectOnDestroy;

        /// <summary>
        /// If true then the ability will create an object on collision
        /// </summary>
        [Tooltip("Spawn the object when the Ability collides")]
        public bool spawnObjectOnCollide;

        /// <summary>
        /// The object to create on collision/destroy
        /// </summary>
        [Tooltip("Objec to spawn")]
        public ABC_GameObjectReference spawningObject;

        /// <summary>
        /// If true then the activating entity needs to be within range of the selected target for the ability to activate
        /// </summary>
        [Tooltip("Only fire selected target Ability when in range")]
        public bool useRange = false;

        /// <summary>
        /// The range that the entity must be greater than to activate the ability
        /// </summary>
        [Tooltip("The range that the entity must be greater than to activate the ability")]
        public float selectedTargetRangeGreaterThan = 0f;

        /// <summary>
        /// The range that the entity must be less than to activate the ability
        /// </summary>
        [Tooltip("The range that the entity must be less than to activate the ability")]
        public float selectedTargetRangeLessThan = 100f;

        /// <summary>
        /// offset of selected target (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Offset of selected target (gets the target position but then adds in the offset)")]
        public Vector3 selectedTargetOffset;

        /// <summary>
        /// forward offset of selected target (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float selectedTargetForwardOffset = 0f;

        /// <summary>
        /// right offset of selected target (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float selectedTargetRightOffset = 0f;

        /// <summary>
        /// If true then the ability has the potential to miss the target depending on the originators miss chance and values
        /// else if false it will never take the miss chance into account and always go to the position it was meant to. 
        /// </summary>
        [Tooltip("If true then the ability has the potential to miss the target depending on the originators miss chance and values else if false it will never take the miss chance into account and always go to the position it was meant to. ")]
        public bool abilityCanMiss = true;

        /// <summary>
        /// How long the ability will wait before initating simulates the entity 'casting/activating/preparing' to activate
        /// </summary>
        [Tooltip("How long to wait before spell fires (casting animations/effects/wait)")]
        [Range(0f, 300f)]
        public float prepareTime;

        /// <summary>
        /// If true then the prepare time can not be adjusted via effects
        /// </summary>
        [Tooltip("If true then the prepare time can not be adjusted via effects")]
        public bool ignoreGlobalPrepareTimeAdjustments = false;

        /// <summary>
        /// If true then releasing the trigger will cancel the preparation stage early, i.e the preparation stage will only happen whilst the trigger is held down
        /// </summary>
        [Tooltip("If true then releasing the trigger will cancel the preparation stage early, i.e the preparation stage will only happen whilst the trigger is held down")]
        public bool prepareTriggerHoldRequied = false;

        /// <summary>
        /// Will change the speed of the ability initiation i.e speeding up or slowing down an attack
        /// </summary>
        [Tooltip(" Will change the speed of the ability initiation i.e speeding up or slowing down an attack")]
        public float abilityInitiatingBaseSpeedAdjustment = 100f;

        /// <summary>
        /// If true then the initiation speed adjustment will have no effect on the ability
        /// </summary>
        [Tooltip("If true then the initiation speed adjustment will have no effect on the ability")]
        public bool ignoreGlobalInitiatingSpeedAdjustments = false;

        /// <summary>
        /// If true then the initiating base speed adjustment will be modified by a stat
        /// </summary>
        [Tooltip("If true then the initiating base speed adjustment will be modified by a stat")]
        public bool modifyAbilityInitiatingBaseSpeedByStat = false;

        /// <summary>
        /// The stat modification to make to the ability initiating base speed
        /// </summary>
        [Tooltip("The stat modification to make to the ability initiating base speed")]
        public Effect.PotencyStatModifications abilityInitiatingBaseSpeedStatModification = new Effect.PotencyStatModifications();

        /// <summary>
        /// If true then the ability will stop after preparing and not move to the initiating stage until a button is pressed
        /// </summary>
        [Tooltip("Wait for a keypress to move to the initiation stage")]
        public bool waitForKeyBeforeInitiating = false;

        /// <summary>
        /// Input type to move onto initiating stage
        /// </summary>
        [Tooltip("type of input to press to move from preparing to initiation")]
        public InputType waitBeforeInitiatingInputType;

        /// <summary>
        /// Button to press to move onto initiating stage
        /// </summary>
        [Tooltip("The Button Name user has to press to move from preparing to initiation")]
        public string waitBeforeInitiatingButton;

        /// <summary>
        /// Key to press to move onto initiating stage
        /// </summary>
        [Tooltip("What Key user has to press to move from preparing to initiation")]
        public KeyCode waitBeforeInitiatingKey = KeyCode.Return;

        /// <summary>
        /// Wait period before the key to initiate is recognised
        /// </summary>
        [Tooltip("Wait period before the key to initiate is recognised")]
        public float waitForKeyBeforeInitiatingDelay = 0f;

        /// <summary>
        /// If true then the target can be changed whilst waiting for a key to move from preparation to initiation stage
        /// </summary>
        [Tooltip("If true then the target can be changed whilst waiting for a key to move from preparation to initiation stage")]
        public bool waitForKeyAllowChangeOfTarget = true;

        /// <summary>
        /// If true then entity movement can interrupt the ability preparation
        /// </summary>
        [Tooltip("Does moving interupt the Ability casting/preparation")]
        public bool moveInteruptPreparation;

        /// <summary>
        /// Distance to travel to interupt whilst preparing
        /// </summary>
        [Tooltip("Distance to travel to interupt whilst preparing")]
        public float distanceInteruptPreparation;

        /// <summary>
        /// If true then preparation progress will be displayed on the GUI
        /// </summary>
        [Tooltip("Show the preparation/casting in game with a GUI bar")]
        public bool showPrepareTimeOnGUI = false;


        /// <summary>
        /// If true then graphics will show when preparing
        /// </summary>
        [Tooltip("Use preparing effects and animations")]
        public bool usePreparingAesthetics;

#if ABC_GC_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed on preparation 
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed on preparation")]
    public GameCreator.Core.IActionsList gcPreparingActionList;

#endif

#if ABC_GC_2_Integration
    /// <summary>
    /// For GC integration adds an action list which is executed when preparing
    /// </summary>
    [Tooltip("For GC integration adds an action list which is executed when preparing")]
    public GameCreator.Runtime.VisualScripting.Actions gc2PreparingAction;

#endif

        /// <summary>
        /// If true then the preparing animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate preparing animation on the main entities animator")]
        public bool preparingAnimateOnEntity = true;

        /// <summary>
        /// If true then the preparing animation will activate on the current scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate preparing animation on the current scroll ability's graphic animator")]
        public bool preparingAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool preparingAnimateOnWeapon = false;

        // does the jump ability make the player suspend in air for a short period (KH/DMC)
        /// <summary>
        /// If true then the entity will defy gravity whilst ability is preparing
        /// </summary>
        [Tooltip("When the ability is casted does the caster defy gravity for a shirt while.")]
        public bool defyGravityPreparing = false;

        /// <summary>
        /// How long entity will defy gravity for whilst ability is preparing
        /// </summary>
        [Tooltip("How long the caster defies gravity for when preparing")]
        public float defyGravityPreparingDuration = 2f;

        /// <summary>
        /// Delay before the defy gravity begins
        /// </summary>
        [Tooltip(" Delay before the defy gravity begins")]
        public float defyGravityPreparingDelay = 0f;


        /// <summary>
        /// If true then the defy gravity event delgates will be invoked when preparing
        /// </summary>
        [Tooltip("If true then the defy gravity event delgates will be invoked when preparing")]
        public bool defyGravityPreparingRaiseEvent = false;

        /// <summary>
        /// Offset of the preparing graphics
        /// </summary>
        [Tooltip("Offset of the preparing effects")]
        public Vector3 preparingAestheticsPositionOffset;

        /// <summary>
        /// Forward Offset of the preparing graphics
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float preparingAestheticsPositionForwardOffset = 0f;

        /// <summary>
        /// Right Offset of the preparing graphics
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float preparingAestheticsPositionRightOffset = 0f;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference preparingAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference preparingAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float preparingAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float preparingAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then the preparing animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate preparing animatio on the main entities animation runner")]
        public bool preparingAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the preparing animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate preparing animation on the current scroll ability's graphic animation runner")]
        public bool preparingAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool preparingAnimationRunnerOnWeapon = false;

        /// <summary>
        /// Name of the preparing animation
        /// </summary>
        [Tooltip("Name of the animation in the controller ")]
        public string preparingAnimatorParameter;

        /// <summary>
        /// Parameter type of the preparing animation
        /// </summary>
        [Tooltip("Parameter type to activate animation")]
        public AnimatorParameterType preparingAnimatorParameterType;

        /// <summary>
        /// Value to start the preparing animation
        /// </summary>
        [Tooltip("Value to turn on animation")]
        public string preparingAnimatorOnValue;

        /// <summary>
        /// Value to end the preparing animation
        /// </summary>
        [Tooltip("Value to turn off animation ")]
        public string preparingAnimatorOffValue;

        /// <summary>
        /// Moves entity when Preparing (useful if no root motion applied to animation
        /// </summary>
        [Tooltip("Moves entity when Preparing (useful if no root motion applied to animation")]
        public bool moveSelfWhenPreparing = false;

        /// <summary>
        /// Offset to move the entity too
        /// </summary>
        [Tooltip("Offset value to move too")]
        public Vector3 moveSelfPreparingOffset = Vector3.zero;

        /// <summary>
        /// Forward Offset to move the entity too
        /// </summary>
        [Tooltip("Forward offset to move too")]
        public float moveSelfPreparingForwardOffset = 0f;

        /// <summary>
        /// Right Offset to move the entity too
        /// </summary>
        [Tooltip("right offset to move too")]
        public float moveSelfPreparingRightOffset = 0f;

        /// <summary>
        /// Delay before entity starts moving
        /// </summary>
        [Tooltip("Delay before entity starts moving")]
        public float moveSelfPreparingDelay = 0f;

        /// <summary>
        /// How long it takes the entity to move to the new position
        /// </summary>
        [Tooltip("How long it takes entity to get to the new position")]
        public float moveSelfPreparingDuration = 1f;

        /// <summary>
        ///  Moves the activating entity to the target of the ability during ability preparation, useful for when no root motion is applied to the animation
        ///  If ability doesn't track target (forward travel type etc) then entity will move towards the current target or soft target if current target doesn't exist
        /// </summary>
        [Tooltip("Moves the activating entity to the target of the ability during ability preparation.  " +
            "If ability doesn't track target (forward travel type etc) then entity will move towards the current target or soft target if current target doesn't exist")]
        public bool moveSelfToTargetWhenPreparing = false;

        /// <summary>
        /// The delay before the entity moves towards the target
        /// </summary>
        [Tooltip("The delay before the entity moves towards the target")]
        public float moveSelfToTargetPreparingDelay = 0f;

        /// <summary>
        /// How long it takes the entity to get to the target when moving during preparation
        /// </summary>
        [Tooltip("How long it takes the entity to get to the target when moving during preparation")]
        public float moveSelfToTargetPreparingDuration = 0.5f;


        /// <summary>
        /// The distance that the entity will stop infront of target 
        /// </summary>
        [Tooltip("The distance that the entity will stop infront of target")]
        public float moveSelfToTargetPreparingStopDistance = 5f;

        /// <summary>
        /// If true then the preparing animation will only activate if the entity moves towards the target (if already within stop distance animation won't play)
        /// </summary>
        public bool moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving = false;

        /// <summary>
        /// The offset applied to the target we are moving too
        /// </summary>
        [Tooltip("The offset applied to the target we are moving too")]
        public Vector3 moveSelfToTargetPreparingOffset = Vector3.zero;

        /// <summary>
        /// Forward Offset applied to the target we are moving too
        /// </summary>
        [Tooltip("Forward Offset applied to the target we are moving too")]
        public float moveSelfToTargetPreparingForwardOffset = 0f;

        /// <summary>
        /// Right Offset applied to the target we are moving too
        /// </summary>
        [Tooltip("Right Offset applied to the target we are moving too")]
        public float moveSelfToTargetPreparingRightOffset = 0f;

        /// <summary>
        /// Graphic object which shows when ability is preparing
        /// </summary>
        [Tooltip("Particle or object to show when preparing")]
        public ABC_GameObjectReference preparingGraphic;

        /// <summary>
        /// Sub graphic object which shows when ability is preparing. Will be a child of the preparing graphic
        /// </summary>
        [Tooltip("Sub graphic or object to show when preparing")]
        public ABC_GameObjectReference preparingSubGraphic;

        /// <summary>
        /// If enabled then the preparing Aesthetic will show until the ability stops preparing
        /// </summary>
        [Tooltip("If enabled then the preparing Aesthetic will show until the ability stops preparing")]
        public bool preparingAestheticDurationUsePrepareTime = true;

        /// <summary>
        /// How long the preparing graphic will show for
        /// </summary>
        [Tooltip("How long to show the graphical effect for")]
        public float preparingAestheticDuration;

        /// <summary>
        /// The starting position of the preparing graphic
        /// </summary>
        [Tooltip("Starting position of the effect")]
        public StartingPosition preparingStartPosition;

        /// <summary>
        /// If starting position is OnObject then the object where the preparing graphic will start
        /// </summary>
        public ABC_GameObjectReference preparingPositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string preparingPositionOnTag;

        /// <summary>
        /// If true then the ability projectile or another graphic will travel to the start position before initiating. Good for telekensis or reciving lightning from the sky before sending it onwards
        /// </summary>
        [Tooltip("If true then options will appear to setup a projectile travelling to the start position before scrollAbility. Good for telekensis or reciving lightning from the sky before sending it onwards")]
        public bool useProjectileToStartPosition = false;

        /// <summary>
        /// At what ability stage the projectile to start will occur, i.e preparing or initiating
        /// </summary>
        [Tooltip("At what ability stage the projectile to start will occur, i.e preparing or initiating")]
        public ProjectileToStartType projectileToStartType = ProjectileToStartType.Preparing;

        /// <summary>
        /// Delay till projectile to start begins 
        /// </summary>
        [Tooltip("Delay till projectile to start begins")]
        public float projToStartDelay = 0f;

        /// <summary>
        /// If false then a new graphic can be used for Projectile to Start Position. If true then the original ability is used
        /// </summary>
        [Tooltip("If false then a new graphic can be used for Projectile to Start Position. If true then the original is used")]
        public bool useOriginalProjectilePTS = false;

        /// <summary>
        /// Graphic which will travel to the ability start position
        /// </summary>
        [Tooltip("Main graphic for the project to start position")]
        public ABC_GameObjectReference projToStartPosGraphic;

        /// <summary>
        /// Sub graphic which will be apart of the projectile to start graphic
        /// </summary>
        [Tooltip("Child graphic")]
        public ABC_GameObjectReference projToStartPosSubGraphic;

        /// <summary>
        /// How long the projectile to start position will last for
        /// </summary>
        [Tooltip("How long graphic lasts for")]
        public float projToStartPosDuration = 5f;

        /// <summary>
        /// The starting position of the projectile to start graphic. From this position it will travel to the actual starting position of the ability
        /// </summary>
        [Tooltip("The Starting Position of the project to travel from to the main starting position")]
        public StartingPosition projToStartStartingPosition;

        /// <summary>
        /// If true then the projectile to start graphic will rotate towards the target
        /// </summary>
        [Tooltip("Does the projectile rotate towards the target or not")]
        public bool projToStartRotateToTarget = false;

        /// <summary>
        /// The starting direction of the projectile to start graphic
        /// </summary>
        [Tooltip("Which way the projectile is facing")]
        public Vector3 projToStartRotation;

        /// <summary>
        /// If true then the euler rotation of the ability will be set to the starting points euler rotation
        /// </summary>
        [Tooltip("If true then the euler rotation of the ability will be set to the starting points euler rotation")]
        public bool projToStartSetEulerRotation = false;

        /// <summary>
        /// Object which the graphic can start from if starting position is OnObject 
        /// </summary>
        [Tooltip("GameObject to start from")]
        public ABC_GameObjectReference projToStartPositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string projToStartPositionOnTag;

        /// <summary>
        /// Offset for the projectile to start position graphic
        /// </summary>
        [Tooltip("Offset of the preparing effects")]
        public Vector3 projToStartPositionOffset;

        /// <summary>
        /// Forward Offset for the projectile to start position graphic
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float projToStartPositionForwardOffset = 0f;

        /// <summary>
        /// Right Offset for the projectile to start position graphic
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float projToStartPositionRightOffset = 0f;

        /// <summary>
        /// Does the graphic to start position hover once it reaches the destination
        /// </summary>
        [Tooltip("Does the projectile hover")]
        public bool projToStartHoverOnSpot = false;

        /// <summary>
        /// Hover distance of the graphic to start position
        /// </summary>
        [Tooltip("Does the projectile hover")]
        public float projToStartHoverDistance = 0.4f;


        /// <summary>
        /// If true then the graphic to start position will travel to the ability start position
        /// </summary>
        [Tooltip("If true then the graphic to start position will travel to the ability start position")]
        public bool projToStartTravelToAbilityStartPosition = true;

        /// <summary>
        /// If true then the graphic to start position will move with the activating entity
        /// </summary>
        [Tooltip("Does the projectile move with the target")]
        public bool projToStartMoveWithTarget = true;

        /// <summary>
        /// How long it takes for the graphic to reach the start position
        /// </summary>
        [Tooltip("How long it takes for the projectile to reach the start position")]
        public float projToStartReachPositionTime = 2f;

        /// <summary>
        /// The delay before the projectile will start travelling to the start position
        /// </summary>
        [Tooltip("The delay before the projectile will start travelling to the start position")]
        public float projToStartTravelDelay = 0f;

        /// <summary>
        /// if true then graphics will show when it's the current scroll ability
        /// </summary>
        [Tooltip("Use scrollability effects and animations")]
        public bool useScrollAbilityAesthetics;

        /// <summary>
        /// If true then the scroll animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate scroll animation on the main entities animator")]
        public bool scrollAbilityAnimateOnEntity = true;

        /// <summary>
        /// If true then the scroll animation will activate on the scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate scroll animation on the scroll ability's graphic animator")]
        public bool scrollAbilityAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool scrollAbilityAnimateOnWeapon = false;

        /// <summary>
        /// Offset for the graphic used when ability is set as the current scroll ability
        /// </summary>
        [Tooltip("Offset of the preparing effects")]
        public Vector3 scrollAbilityAestheticsPositionOffset;

        /// <summary>
        /// Forward Offset for the graphic used when ability is set as the current scroll ability
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float scrollAbilityAestheticsPositionForwardOffset = 0f;

        /// <summary>
        /// Right Offset for the graphic used when ability is set as the current scroll ability
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float scrollAbilityAestheticsPositionRightOffset = 0f;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference scrollAbilityAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference scrollAbilityAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityAnimationRunnerClipDuration = 1f;


        /// <summary>
        /// If true then the scrollAbility animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate scrollAbility animatio on the main entities animation runner")]
        public bool scrollAbilityAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the scrollAbility animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate scrollAbility animation on the current scroll ability's graphic animation runner")]
        public bool scrollAbilityAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool scrollAbilityAnimationRunnerOnWeapon = false;

        /// <summary>
        /// Name of the scroll ability animation
        /// </summary>
        [Tooltip("Name of the animation in the controller ")]
        public string scrollAbilityAnimatorParameter;

        /// <summary>
        /// Type of parameter to activate the animation
        /// </summary>
        [Tooltip("Parameter type to activate animation")]
        public AnimatorParameterType scrollAbilityAnimatorParameterType;

        /// <summary>
        /// Value to start the animation
        /// </summary>
        [Tooltip("Value to turn on animation")]
        public string scrollAbilityAnimatorOnValue;

        /// <summary>
        /// value to end the animation
        /// </summary>
        [Tooltip("Value to turn off animation ")]
        public string scrollAbilityAnimatorOffValue;

        /// <summary>
        /// How long the animation will play for
        /// </summary>
        [Tooltip("How long to play animation for ")]
        public float scrollAbilityAnimatorDuration = 3f;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference scrollAbilityDeactivateAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference scrollAbilityDeactivateAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityDeactivateAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityDeactivateAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float scrollAbilityDeactivateAnimationRunnerClipDuration = 1f;


        /// <summary>
        /// If true then the scroll Ability Deactivate animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate scroll Ability Deactivate animatio on the main entities animation runner")]
        public bool scrollAbilityDeactivateAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the scroll Ability Deactivate animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate scroll Ability Deactivate animation on the current scroll ability's graphic animation runner")]
        public bool scrollAbilityDeactivateAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool scrollAbilityDeactivateAnimationRunnerOnWeapon = false;




        /// <summary>
        /// Name of the scroll ability deactivate animation
        /// </summary>
        [Tooltip("Name of the animation in the controller ")]
        public string scrollAbilityDeactivateAnimatorParameter;

        /// <summary>
        /// If true then the scroll deactivate animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate scroll animation on the main entities animator")]
        public bool scrollAbilityDeactivateAnimateOnEntity = true;

        /// <summary>
        /// If true then the scroll deactivate animation will activate on the scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate scroll animation on the scroll ability's graphic animator")]
        public bool scrollAbilityDeactivateAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool scrollAbilityDeactivateAnimateOnWeapon = false;

        /// <summary>
        /// Type of parameter to activate the animation
        /// </summary>
        [Tooltip("Parameter type to activate animation")]
        public AnimatorParameterType scrollAbilityDeactivateAnimatorParameterType;

        /// <summary>
        /// Value to start the animation
        /// </summary>
        [Tooltip("Value to turn on animation")]
        public string scrollAbilityDeactivateAnimatorOnValue;

        /// <summary>
        /// value to end the animation
        /// </summary>
        [Tooltip("Value to turn off animation ")]
        public string scrollAbilityDeactivateAnimatorOffValue;

        /// <summary>
        /// How long the animation will play for
        /// </summary>
        [Tooltip("How long to play animation for ")]
        public float scrollAbilityDeactivateAnimatorDuration = 3f;


        /// <summary>
        /// graphic object which shows when ability is the current scroll ability
        /// </summary>
        [Tooltip("graphic object which shows when ability is the current scroll ability")]
        public ABC_GameObjectReference scrollAbilityGraphic;

        /// <summary>
        /// Sub graphic for the current scroll ability. Will be a child of the main graphic.
        /// </summary>
        [Tooltip("Sub graphic or object to show ")]
        public ABC_GameObjectReference scrollAbilitySubGraphic;


        /// <summary>
        /// The delay until the graphic is shown when activating 'equpping' the scroll ability
        /// </summary>
        [Tooltip("The delay until the graphic is shown when activating 'equpping' the scroll ability")]
        public float scrollAbilityGraphicActivateDelay = 0f;

        /// <summary>
        /// The delay until the graphic is shown when deactivating 'unequpping' the scroll ability
        /// </summary>
        [Tooltip("The delay until the graphic is shown when deactivating 'unequpping' the scroll ability")]
        public float scrollAbilityGraphicDeactivateDelay = 0f;


        /// <summary>
        /// Starting position for the scroll ability graphic
        /// </summary>
        [Tooltip("Starting position of the effect")]
        public StartingPosition scrollAbilityStartPosition;

        /// <summary>
        /// the object if starting position is OnObject 
        /// </summary>
        [Tooltip("the object if starting position is OnObject ")]
        public ABC_GameObjectReference scrollAbilityPositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string scrollAbilityPositionOnTag;

        /// <summary>
        /// If true then the scroll ability graphic will only show for a duration set
        /// </summary>
        [Tooltip("Does the graphic have a duration")]
        public DurationType scrollAbilityAestheticDurationType;

        /// <summary>
        /// Duration for the scroll ability graphic
        /// </summary>
        [Tooltip("How long to show the graphical effect for")]
        public float scrollAbilityAestheticDuration = 0f;

        /// <summary>
        /// Position for the scroll ability graphic whilst it is persistant but inactive
        /// </summary>
        [Tooltip("inactive position of the effect")]
        public StartingPosition scrollAbilityPersistantAestheticInactivePosition;

        /// <summary>
        /// the object if inactive position is OnObject 
        /// </summary>
        [Tooltip("the object if inactive position is OnObject ")]
        public ABC_GameObjectReference scrollAbilityPersistantAestheticInactivePositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if inactive position is OnTag. Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string scrollAbilityPersistantAestheticInactivePositionOnTag;



        /// <summary>
        /// If true then graphics will appear when ability is reloaded
        /// </summary>
        [Tooltip("Use reload effects and animations")]
        public bool useReloadAbilityAesthetics;

        /// <summary>
        /// If true then the reload animation will activate on the main entities animator
        /// </summary>
        [Tooltip("Activate reload animation on the main entities animator")]
        public bool reloadAbilityAnimateOnEntity = true;

        /// <summary>
        /// If true then the reload animation will activate on the scroll ability's graphic animator
        /// </summary>
        [Tooltip("Activate reload animation on the scroll ability's graphic animator")]
        public bool reloadAbilityAnimateOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animator
        /// </summary>
        [Tooltip("Activate animation on the current weapons animator")]
        public bool reloadAbilityAnimateOnWeapon = false;

        /// <summary>
        /// Offset for reload graphics
        /// </summary>
        [Tooltip("Offset of the preparing effects")]
        public Vector3 reloadAbilityAestheticsPositionOffset;

        /// <summary>
        /// Forward Offset for reload graphics
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float reloadAbilityAestheticsPositionForwardOffset = 0f;

        /// <summary>
        /// Right Offset for reload graphics
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float reloadAbilityAestheticsPositionRightOffset = 0f;


        /// <summary>
        /// Name of the reload animation
        /// </summary>
        [Tooltip("Name of the animation in the controller ")]
        public string reloadAbilityAnimatorParameter;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference reloadAbilityAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference reloadAbilityAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float reloadAbilityAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float reloadAbilityAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// If true then the reload Ability animation will activate on the main entities animation runner
        /// </summary>
        [Tooltip("Activate reload Ability animatio on the main entities animation runner")]
        public bool reloadAbilityAnimationRunnerOnEntity = true;

        /// <summary>
        /// If true then the reload Ability animation will activate on the current scroll ability's graphic animation runner
        /// </summary>
        [Tooltip("Activate reload Ability animation on the current scroll ability's graphic animation runner")]
        public bool reloadAbilityAnimationRunnerOnScrollGraphic = false;

        /// <summary>
        /// If true then the animation will activate on the current weapons animation runner
        /// </summary>
        [Tooltip("Activate animation on the current weapons animation runner")]
        public bool reloadAbilityAnimationRunnerOnWeapon = false;

        /// <summary>
        /// Parameter type of the reload animation
        /// </summary>
        [Tooltip("Parameter type to activate animation")]
        public AnimatorParameterType reloadAbilityAnimatorParameterType;

        /// <summary>
        /// Value to start the reload animation
        /// </summary>
        [Tooltip("Value to turn on animation")]
        public string reloadAbilityAnimatorOnValue;

        /// <summary>
        /// Value to end the reload animation
        /// </summary>
        [Tooltip("Value to turn off animation ")]
        public string reloadAbilityAnimatorOffValue;

        /// <summary>
        /// Graphic object that shows when reloading
        /// </summary>
        [Tooltip("Particle or object to show when preparing")]
        public ABC_GameObjectReference reloadAbilityGraphic;

        /// <summary>
        /// Sub reloading graphic. Will be a child of the main graphic
        /// </summary>
        [Tooltip("Sub mainGraphic or object to show when preparing")]
        public ABC_GameObjectReference reloadAbilitySubGraphic;


        /// <summary>
        /// duration of the reload graphic 
        /// </summary>
        [Tooltip("How long to show the graphical effect for")]
        public float reloadAbilityAestheticDuration = 2f;


        /// <summary>
        /// Starting position of the reload graphic
        /// </summary>
        [Tooltip("Starting position of the effect")]
        public StartingPosition reloadAbilityStartPosition;

        /// <summary>
        /// The object which is used when starting position is OnObject
        /// </summary>
        public ABC_GameObjectReference reloadAbilityPositionOnObject;

        /// <summary>
        /// Tag which the graphic can start from if starting position is OnTag.  Does not work for ABC tags. 
        /// </summary>
        [Tooltip("Tag to start from")]
        public string reloadAbilityPositionOnTag;


        /// <summary>
        /// If true then a graphic will show when the ability is destroyed
        /// </summary>
        [Tooltip("Show effects when the Ability is destroyed")]
        public bool useAbilityEndAesthetics;

        /// <summary>
        /// If true then the end graphic will play a graphic setup from ability effects. Prioritising the first Adjust Health graphic found.
        /// </summary>
        [Tooltip("If true then the end graphic will play a graphic setup from ability effects. Prioritising the first Adjust Health graphic found.")]
        public bool abilityEndUseEffectGraphic = false;

        /// <summary>
        /// If true then end graphic will only activate on non state managers 
        /// </summary>
        [Tooltip("If true then end graphic will only activate on non state managers ")]
        public bool abilityEndActivateOnEnvironmentOnly = false;

        /// <summary>
        /// Graphic that plays when ability ends
        /// </summary>
        [Tooltip("Particle or object to show when the ability is destroyed")]
        public ABC_GameObjectReference abilityEndGraphic;

        /// <summary>
        /// Sub mainGraphic or object to show when the ability is destroyed. Will be a child of the main graphic
        /// </summary>
        [Tooltip("Sub mainGraphic or object to show when the ability is destroyed")]
        public ABC_GameObjectReference abilityEndSubGraphic;

        /// <summary>
        /// If true then graphic scale will be modified during play
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public bool scaleAbilityEndGraphic = false;

        /// <summary>
        /// scale to apply to graphic
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public float abilityEndGraphicScale = 1f;

        /// <summary>
        /// Duration of ability end graphic
        /// </summary>
        [Tooltip("How long to play the effect for")]
        public float abEndAestheticDuration = 2f;

        /// <summary>
        /// If true then activating entity will stop moving when ability is initiating
        /// </summary>
        [Tooltip("Stop caster from moving when initiating Ability")]
        public bool stopMovementOnInitiate;

        /// <summary>
        /// How long to stop movement for on initiation, if 0 then it will stop after the initiation animation (or if ability toggled off/interrupted)
        /// </summary>
        public float stopMovementOnInitiateDuration = 0.5f;

        /// <summary>
        /// If true then activating entity will stop moving due to the position being frozen
        /// </summary>
        [Tooltip("If true then activating entity will stop moving due to the position being frozen")]
        public bool stopMovementOnInitiateFreezePosition = false;

        /// <summary>
        /// If true then activating entity will stop moving due to movement components being disabled
        /// </summary>
        [Tooltip("If true then activating entity will stop moving due to movement components being disabled")]
        public bool stopMovementOnInitiateDisableComponents = true;

        /// <summary>
        /// If true then the start and stop movement event delgates will be invoked
        /// </summary>
        [Tooltip("If true then the start and stop movement event delgates will be invoked")]
        public bool stopMovementOnInitiateRaiseEvent;

        /// <summary>
        /// If true then activating entity will stop moving when ability is preparing
        /// </summary>
        [Tooltip("Stop caster from moving when preparing Ability")]
        public bool stopMovementOnPreparing;

        /// <summary>
        /// How long to stop movement for on preparing, if 0 then it will stop after the preparation animation (or if ability is interrupted)
        /// </summary>
        public float stopMovementOnPreparingDuration = 0.5f;

        /// <summary>
        /// If true then activating entity will stop moving due to the position being frozen
        /// </summary>
        [Tooltip("If true then activating entity will stop moving due to the position being frozen")]
        public bool stopMovementOnPreparingFreezePosition = false;

        /// <summary>
        /// If true then activating entity will stop moving due to movement components being disabled
        /// </summary>
        [Tooltip("If true then activating entity will stop moving due to movement components being disabled")]
        public bool stopMovementOnPreparingDisableComponents = true;

        /// <summary>
        /// If true then the start and stop movement event delgates will be invoked
        /// </summary>
        [Tooltip("If true then the start and stop movement event delgates will be invoked")]
        public bool stopMovementOnPreparingRaiseEvent;

        #endregion


        // ****************** Variables ***************************

        #region Variables

        /// <summary>
        /// Pool that holds all the ability projectile objects
        /// </summary>
        private List<GameObject> abilityPool = new List<GameObject>();



        /// <summary>
        /// Pool to hold graphics for the scrollability equipping
        /// </summary>
        private List<GameObject> scrollActivatePool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for reloading a scroll ability
        /// </summary>
        private List<GameObject> reloadAbilityPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for ability preparation
        /// </summary>
        private List<GameObject> preparingPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for projectile to start
        /// </summary>
        private List<GameObject> projectileToStartPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for ability initiation
        /// </summary>
        private List<GameObject> initiatingPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for additional ability initiation
        /// </summary>
        private List<GameObject> initiatingAdditionalPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds graphics for ability end/destroy
        /// </summary>
        private List<GameObject> abEndPool = new List<GameObject>();

        /// <summary>
        /// Pool that holds all graphics for spawning objects
        /// </summary>
        private List<GameObject> spawnPool = new List<GameObject>();

        /// <summary>
        /// A projectile object which has already been created for a graphical effect but has not officially been activated yet. Example: Summoning the ability projectile to the entity hand before shooting it off. 
        /// </summary>
        private List<GameObject> preActivatedProjectiles = new List<GameObject>();

        /// <summary>
        /// The coroutine which was called to disable the projectile to start position effect. This can be stopped early to stop the disabling.
        /// </summary>
        public List<IEnumerator> preActivatedProjectileDisableCoroutines = new List<IEnumerator>();

        /// <summary>
        /// Will keep track if no target still travel has been activated
        /// </summary>
        private NoTargetStillTravelPreviousType noTargetStillTravelActivated = NoTargetStillTravelPreviousType.None;

        /// <summary>
        /// Tracks if ability should ignore hold preparation (i.e if autocasted or through AI)
        /// </summary>
        private bool IgnoreHoldPreparation = false;

        /// <summary>
        /// Tracks what time the ability was activated (ensures methods on other threads can know if ability has been activated before the threaded method ended)
        /// </summary>
        private float abilityActivationTime = 0f;

        /// <summary>
        /// Tracks what time the ability was last enabled
        /// </summary>
        private float abilityEnableTime = 0f;

        /// <summary>
        /// Tracks how long the ability prepared for
        /// </summary>
        private float abilitySecondsPrepared = 0;

        /// <summary>
        /// Bool that determines if the ability has been interrupted 
        /// </summary>
        private bool abilityActivationInterrupted = false;

        /// <summary>
        /// Bool that determines if the ability is currently on cooldown
        /// </summary>
        private bool abilityOnCooldown = false;

        /// <summary>
        /// Float that determines the time the ability started cooldown
        /// </summary>
        private float abilityCooldownStartTime;

        /// <summary>
        /// Float that determines what cooldown adjustment the ability currently has 
        /// </summary>
        private float abilityCurrentCooldownAdjustment;

        /// <summary>
        /// Float that determines what prepare time adjustment the ability currently has
        /// </summary>
        private float abilityCurrentPrepareTimeAdjustment;

        /// <summary>
        /// Float that determines what initiation speed adjustment the ability currently has
        /// </summary>
        private float abilityCurrentInitiationSpeedAdjustment;

        /// <summary>
        /// Records when the ability activated and was as a result combo locked, this is used to work out if the combo timer is up to reset the combo and start from the beginning. 
        /// </summary>
        private float comboLockedTime = 0f;

        /// <summary>
        /// Bool which identifies if the ability is locked due to being recently activated as part of a combo
        /// </summary>
        private bool comboLocked = false;

        /// <summary>
        /// Bool which identifies if the ability hit an entity during the combo process
        /// </summary>
        /// <remarks>If set too then the combo won't continue until it's either been reset due to taking too long or a hit was made</remarks>
        private bool comboLockHit = false;


        /// <summary>
        /// Bool that determines if the ability is currently active and toggled on
        /// </summary>
        private bool toggled = false;

        /// <summary>
        /// The projectile object that is currently active and toggled
        /// </summary>
        private List<GameObject> toggledAbilityObj = new List<GameObject>();

        /// <summary>
        ///  track initiating graphic incase we need to delete it later on
        /// </summary>
        private List<GameObject> initiatingObjs = new List<GameObject>();

        /// <summary>
        /// The amount of ammo we currently have in our clip
        /// </summary>
        private int currentAmmoClipCount = -1;

        /// <summary>
        /// Determines if a reload is required
        /// </summary>
        private bool reloadRequired = false;

        /// <summary>
        /// Determines if the ability is currently reloading
        /// </summary>
        private bool isReloading = false;

        /// <summary>
        /// If true then the abilit will immediatly stop reloading
        /// </summary>
        /// <remarks>Used for when an ability is switched over before reloading has finished </remarks>
        private bool reloadInterrupted = false;


        /// <summary>
        /// Returns a Vector3 with the local scale of the ability effect/collision range. 
        /// This size can be applied to objects to indicate to the player which objects are in range and will be hit by the ability.
        /// </summary>
        private Vector3 effectRangeLocalScale {
            get {
                // get radius depending on if we using graphic radius or collider radius
                float radius = this.colliderRadius;

                // return the vector of the highest effect range (either splash, explosion splash or normal radius) 
                if (useGraphicRadius && abilityPool.Count > 0 && abilityPool[0].transform.GetComponent<Renderer>() != null)
                    radius = abilityPool[0].transform.GetComponent<Renderer>().bounds.extents.magnitude;

                if (this.destroySplashExplosion == true && this.destroySplashExplosionRadius > radius)
                    radius = this.destroySplashExplosionRadius;

                if (this.useDestroySplashEffect == true && this.destroySplashRadius > radius)
                    radius = this.destroySplashRadius;

                //Return the final radius (collider radius x2)
                return new Vector3(radius * 2f, radius * 2f, radius * 2f);

            }

        }


        /// <summary>
        /// Keep track of active graphic incase we need to suddenly clear them all
        /// </summary>
        private List<GameObject> activeGraphics = new List<GameObject>();

        /// <summary>
        /// Keeps track of the current scroll ability Aesthetic (currently only used when enabling scroll abilities) as this can have no duration and may be stopped when an ability is no longer active. 
        /// </summary>
        private GameObject _scrollAbilityAesthetic = null;

        /// <summary>
        /// Keeps track of the current scroll ability Aesthetic (currently only used for enabling scroll abilities) as this can have no duration and may be stopped when an ability is no longer active. 
        /// Will also retrieve the animator/ABC Animation Runner from the object provided for later uses
        /// </summary>
        private GameObject scrollAbilityAesthetic {

            get {
                return _scrollAbilityAesthetic;
            }
            set {
                _scrollAbilityAesthetic = value;

                if (value != null) {

                    scrollAbilityAestheticAnimator = _scrollAbilityAesthetic.GetComponent<Animator>();
                    //If the entity doesn't have the animation runner component then add it 
                    if (this.scrollAbilityAestheticAnimator != null && this.scrollAbilityAestheticAnimator.gameObject.GetComponent<ABC_AnimationsRunner>() == null) {
                        this.scrollAbilityAestheticAnimationRunner = this.scrollAbilityAestheticAnimator.gameObject.AddComponent<ABC_AnimationsRunner>();
                    }

                    //If ani runner is null for any reason then reassign the component
                    if (this.scrollAbilityAestheticAnimationRunner == null && this.scrollAbilityAestheticAnimator != null)
                        this.scrollAbilityAestheticAnimationRunner = this.scrollAbilityAestheticAnimationRunner.gameObject.GetComponent<ABC_AnimationsRunner>();

                }


            }
        }


        /// <summary>
        /// Keeps track of the current scroll ability Aesthetic animator 
        /// </summary>
        private Animator scrollAbilityAestheticAnimator = null;

        /// <summary>
        /// Keeps track of the current scroll ability Aesthetic ABC Animation Runner 
        /// </summary>
        private ABC_AnimationsRunner scrollAbilityAestheticAnimationRunner = null;



        /// <summary>
        /// A list of objects which will be converted to surrounding objects when the ability is activated 
        /// </summary>
        private List<GameObject> surroundingObjects = new List<GameObject>();


        /// <summary>
        /// The abilities current target
        /// </summary>
        private GameObject target;

        /// <summary>
        /// Potential tag targets for ability to travel too
        /// </summary>
        private List<GameObject> tagTargets = new List<GameObject>();

        /// <summary>
        /// The abilities current world target
        /// </summary>
        private GameObject worldTarget;

        /// <summary>
        /// The abilities current world target position
        /// </summary>
        private Vector3 worldTargetPosition;

        /// <summary>
        /// The abilities current raycast target position
        /// </summary>
        private Vector3 rayCastTargetPosition;




        #endregion

        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will convert the ability to the game type provided by changing settings like how it travels
        /// </summary>
        /// <param name="GameType">Type of game to convert ability settings too (Action, FPS, TPS etc)</param>
        public void ConvertToGameType(ABC_GlobalPortal.GameType GameType) {

            //If ability is self, no travel, custom or nearest tag then don't modify 
            if (this.travelType == TravelType.Forward || this.travelType == TravelType.Self || this.travelType == TravelType.NoTravel && this.abilityType != AbilityType.Melee || this.travelType == TravelType.CustomScript || this.travelType == TravelType.NearestTag)
                return;

            //Modify settings depending on game type
            switch (GameType) {

                case ABC_GlobalPortal.GameType.Action:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }

                    if (this.abilityType == AbilityType.Melee) {


                        if (this.useRange == false)
                            this.noTargetStillTravel = true;


                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CurrentDirection;

                    } else {

                        this.travelType = TravelType.SelectedTarget;
                        this.noTargetStillTravel = true;
                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CurrentDirection;
                        this.abilityBeforeTarget = false;

                        if (this.selectedTargetOffset.y == 0)
                            this.selectedTargetOffset.y = 1.21f;

                    }

                    break;
                case ABC_GlobalPortal.GameType.FPS:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }

                    if (this.abilityType == AbilityType.Melee) {

                        this.noTargetStillTravel = true;
                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = false;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CameraCenter;


                    } else {

                        this.travelType = TravelType.Crosshair;
                        this.rotateToSelectedTarget = false;
                        this.abilityBeforeTarget = false;
                        this.continuouslyTurnToDestination = false;
                        this.requireCrossHairOverride = false;

                        if (this.selectedTargetOffset.y == 1.21f)
                            this.selectedTargetOffset.y = 0f;
                    }

                    break;
                case ABC_GlobalPortal.GameType.TPS:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }

                    if (this.abilityType == AbilityType.Melee) {

                        this.noTargetStillTravel = true;
                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CameraCenter;


                    } else {

                        this.travelType = TravelType.Crosshair;
                        this.abilityBeforeTarget = false;
                        this.continuouslyTurnToDestination = false;
                        this.crossHairRaycastReturnDistancePointOnly = true;


                        if (this.noTargetStillTravel == false)
                            this.crossHairRaycastReturnDistancePointOnly = false;

                        if (this.selectedTargetOffset.y == 1.21f)
                            this.selectedTargetOffset.y = 0f;
                    }


                    break;
                case ABC_GlobalPortal.GameType.RPGMMO:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }


                    if (this.selectedTargetOffset.y == 0)
                        this.selectedTargetOffset.y = 1.21f;

                    if (this.abilityType == AbilityType.Melee) {

                        this.abilityType = AbilityType.Projectile;
                        this.travelType = TravelType.SelectedTarget;

                        this.noTargetStillTravel = false;
                        this.auxiliarySoftTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CurrentDirection;

                        this.rotateToSelectedTarget = true;
                        this.affectOnlyTarget = true;
                        this.continuouslyTurnToDestination = true;

                        this.targetTravel = false;
                        this.useRange = true;
                        this.selectedTargetRangeGreaterThan = 0;
                        this.selectedTargetRangeLessThan = 7;
                        this.selectedTargetRestrictTargets = true;

                        if (this.selectedTargetOnlyCastOnTag.Contains("Enemy") == false)
                            this.selectedTargetOnlyCastOnTag.Add("Enemy");

                    } else {

                        this.travelType = TravelType.SelectedTarget;

                        this.noTargetStillTravel = false;
                        this.auxiliarySoftTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.CurrentDirection;
                        this.abilityBeforeTarget = false;

                        this.rotateToSelectedTarget = true;
                        this.affectOnlyTarget = true;
                        this.continuouslyTurnToDestination = true;


                    }

                    break;
                case ABC_GlobalPortal.GameType.MOBA:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }

                    if (this.abilityType == AbilityType.Melee) {

                        this.noTargetStillTravel = true;
                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.MousePosition;



                    } else {

                        this.travelType = TravelType.MouseTarget;
                        this.abilityBeforeTarget = true;

                        this.rotateToSelectedTarget = true;
                        this.mouseForwardLockY = true;
                        this.continuouslyTurnToDestination = false;

                        if (this.selectedTargetOffset.y == 0)
                            this.selectedTargetOffset.y = 1.21f;

                    }

                    break;
                case ABC_GlobalPortal.GameType.TopDownAction:

                    //if ability is to world then modify ability before target 
                    if (this.travelType == TravelType.ToWorld) {
                        this.abilityBeforeTarget = true;
                        break;
                    }

                    if (this.abilityType == AbilityType.Melee) {
                        
                        if (this.moveSelfToTargetWhenPreparing == true) {
                            this.keyButton = "";
                            this.key = KeyCode.None; 
                        }

                        this.noTargetStillTravel = true;
                        this.auxiliarySoftTarget = true;
                        this.rotateToSelectedTarget = true;
                        this.noTargetRotateBehaviour = AbilityNoTargetRotateBehaviour.MousePosition;





                    } else {

                        this.travelType = TravelType.MouseTarget;
                        this.abilityBeforeTarget = false;

                        this.rotateToSelectedTarget = true;
                        this.mouseForwardLockY = true;
                        this.continuouslyTurnToDestination = false;

                        if (this.selectedTargetOffset.y == 0)
                            this.selectedTargetOffset.y = 1.21f;

                    }

                    break;

            }
        }



        /// <summary>
        /// Will make adjustment to the ability to support Game Creator 2
        /// </summary>
        public void AdjustAbilityForGameCreator2() {

#if ABC_GC_2_Integration

        //Place holder for any adjustments to abilities needed on game start    
        
            this.preparingAestheticsPositionOffset += new Vector3(0, -1, 0);
            this.initiatingAestheticsPositionOffset += new Vector3(0, -1, 0);
            
            if (this.startingPosition == StartingPosition.Self) ;
                this.startingPositionOffset += new Vector3(0, -1, 0);

            foreach (AdditionalStartingPosition addStartPos in this.additionalStartingPositions) {
                if (addStartPos.startingPosition == StartingPosition.Self)
                    addStartPos.startingPositionOffset += new Vector3(0, -1, 0);
            }

#endif

        }



        /// <summary>
        /// Returns a dictionary detailing information about the ability including name, description etc
        /// </summary>
        /// <returns>Dictionary holding information regarding the ability</returns>
        public Dictionary<string, string> GetAbilityDetails(ABC_IEntity Originator) {

            Dictionary<string, string> retval = new Dictionary<string, string>();

            retval.Add("Name", this.name);
            retval.Add("Description", this.description);
            retval.Add("Mana", ((int)(this.manaCost)).ToString());
            retval.Add("Recast", this.GetAbilityRecast(Originator).ToString());
            retval.Add("StatCost", ((int)(this.statCost)).ToString());
            retval.Add("StatIntegrationType", this.statCostIntegrationType.ToString());
            retval.Add("StatCostName", this.statCostName.ToString());

            return retval;

        }


        /// <summary>
        /// Will enable the ability making it avaliable in game. If enable duration setting is greater then 0 then it will only be enabled for the time set. 
        /// </summary>
        /// <param name="delay">Delay before enable starts</param>
        /// <param name="Originator">Entity that activated the ability</param>        
        public IEnumerator Enable(float delay = 0f, ABC_IEntity Originator = null) {

            //If delay is given then wait before abilities enabled
            if (delay > 0f)
                yield return new WaitForSeconds(delay);           

            //Enable ability 
            this.abilityEnabled = true;
            this.abilityEnableTime = Time.time; 

            //If Scroll then make sure it's graphic has been setup (I.e if its persistant then needs to be in deactivated spot)
            if (Originator != null && this.scrollAbility)
                yield return scrollAbilityAesthetic = this.ActivateGraphic(Originator, AbilityGraphicType.ScrollAbilityDeactivation, true);


            //If we are keeping it enabled and not turning off after a duration then end here
            if (this.enableDuration == 0f)
                yield break;

            // Wait for duration then disable ability again
            yield return new WaitForSeconds(this.enableDuration);

            //Ability already been enabled by other means so end here
            if (Time.time - this.abilityEnableTime < this.enableDuration)
                yield break;

            //Disable Ability
            this.Disable(Originator);

        }

        /// <summary>
        /// Will disable the ability making it unavaliable in game. 
        /// </summary>
        public void Disable(ABC_IEntity Originator = null) {

            //If this is a scroll ability then make sure to remove graphics and move originator on to next etc
            if (this.scrollAbility) {
                if (Originator != null && this.IsCurrentScrollAbilityFor(Originator) && Originator.ScrollAbilityCount() > 1) {
                    Originator.EquipNextScrollAbility(false);
                } else if (this.scrollAbilityAesthetic != null) {
                    //just destroy any persistant graphics 
                    ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(this.scrollAbilityAesthetic));
                }
            }


            //disable ability 
            this.abilityEnabled = false;

        }

        /// <summary>
        /// Clears all Object pools relating to the Ability.
        /// </summary>
        public void ClearObjectPools() {

            //Destroy old objects
            foreach (GameObject obj in this.abilityPool) {
                DestroyObject(obj);
            }

            this.abilityPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.initiatingPool) {
                DestroyObject(obj);
            }

            this.initiatingPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.initiatingAdditionalPool) {
                DestroyObject(obj);
            }

            this.initiatingAdditionalPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.scrollActivatePool) {
                DestroyObject(obj);
            }

            this.scrollActivatePool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.reloadAbilityPool) {
                DestroyObject(obj);
            }

            this.reloadAbilityPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.preparingPool) {
                DestroyObject(obj);
            }

            this.preparingPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.projectileToStartPool) {
                DestroyObject(obj);
            }

            this.projectileToStartPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.abEndPool) {
                DestroyObject(obj);
            }

            this.abEndPool.Clear();

            //Destroy old objects
            foreach (GameObject obj in this.spawnPool) {
                DestroyObject(obj);
            }

            this.spawnPool.Clear();

            //Destroy old objects
            foreach (Effect effect in this.effects) {

                foreach (GameObject obj in effect.effectPool) {
                    DestroyObject(obj);
                }
            }


            //Clear effects
            foreach (Effect effect in this.effects) {

                //clear normal effect objects
                if (effect.globalEffect == null) {
                    effect.effectPool.Clear();
                } else {
                    //create effect objects for global effect
                    effect.globalEffect.ElementEffects.ForEach(globEffect => globEffect.effectPool.Clear());
                }

            }


        }

        /// <summary>
        /// Creates all Object Pools relating to the ability. Objects include all 'graphics' including particle effects and items used in abilities.  
        /// </summary>
        /// <param name="PoolParent">An empty game object in which to store all the pooled objects under to keep the hiearchy clear in the game</param>
        public void CreateObjectPools() {


            CreateReloadingObjects();
            CreateScrollActivateObjects();
            CreatePreparingObjects();
            CreateProjectileToStartObjects();
            CreateInitiatingObjects();
            CreateInitiatingAdditionalObjects();
            CreateAbilityObjects();
            CreateAbilityIndicatorObjects();
            CreateAbilityEndObjects();
            CreateEffectObjects();
            CreateSpawnedObjects();


        }


        /// <summary>
        /// Will make sure all abilities linked to this ability will have the same trigger by setting the key/button of the linked abilities
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        public void AbilityTriggerLinkSetup(ABC_IEntity Originator) {

            //If originator doesn't exist or the trigger link has been disabled then end here
            if (this.enableAbilityTriggerLinks == false || Originator == null)
                return;

            //Change the trigger key/button on IDs that don't match this ability (to avoid infinite loops)
            foreach (int ID in this.triggerLinkAbilityIDs.Where(a => a != this.abilityID).ToList()) {
                if (this.keyInputType == InputType.Key)
                    Originator.SetAbilityKey(ID, this.key);
                else
                    Originator.SetAbilityKey(ID, this.keyButton);
            }
        }

        /// <summary>
        /// Set the key of the ability 
        /// </summary>
        /// <param name="Key">Key to set the ability too</param>
        /// <param name="Originator">Entity that activated the ability</param>
        public void SetKey(KeyCode Key, ABC_IEntity Originator = null) {
            this.keyInputType = InputType.Key;
            this.key = Key;

            //If originator has not been provided or trigger links is disabled then end here as we can't set the trigger of linked abilities
            if (Originator == null || this.enableAbilityTriggerLinks == false)
                return;

            //Change the trigger key/button on IDs that don't match this ability (to avoid infinite loops)
            foreach (int ID in this.triggerLinkAbilityIDs.Where(a => a != this.abilityID).ToList())
                Originator.SetAbilityKey(ID, Key);

        }

        /// <summary>
        /// Set the button of the ability 
        /// </summary>
        /// <param name="KeyButton">Button to set the ability too</param>
        /// <param name="Originator">Entity that activated the ability</param>
        public void SetKey(string KeyButton, ABC_IEntity Originator = null) {
            this.keyInputType = InputType.Button;
            this.keyButton = KeyButton;

            //If originator has not been provided or trigger links is disabled then end here as we can't set the trigger of linked abilities
            if (Originator == null || this.enableAbilityTriggerLinks == false)
                return;

            //Change the trigger key/button on IDs that don't match this ability (to avoid infinite loops)
            foreach (int ID in this.triggerLinkAbilityIDs.Where(a => a != this.abilityID).ToList())
                Originator.SetAbilityKey(ID, KeyButton);
        }

        /// <summary>
        /// Returns the key/button which will trigger the ability
        /// </summary>
        /// <returns>String of the key or button which will trigger the ability</returns>
        public string GetKey() {

            //If input combo then return what combination is needed to trigger the ability
            if (this.triggerType == TriggerType.InputCombo) {

                return string.Join(" ", this.keyInputCombo.Select(k => k.ToString()).ToArray());

            } else { // else return the key/button to trigger

                if (this.keyInputType == InputType.Key)
                    return this.key.ToString();
                else
                    return this.keyButton;
            }

        }

        /// <summary>
        /// Will return a list representing the combination of key inputs required to trigger the ability
        /// </summary>
        /// <returns>List of keycodes which need to be pressed in the right combination to trigger the ability</returns>
        public List<KeyCode> GetKeyInputCombo() {

            //If ability not set to rigger by a combination of inputs then return empty list
            if (this.triggerType == TriggerType.Input)
                return new List<KeyCode>();

            //else return the list setup
            return this.keyInputCombo;
        }

        /// <summary>
        /// Returns a bool indicating if the ability is enabled
        /// </summary>
        /// <returns>True if ability is enabled, else false</returns>
        public bool isEnabled() {
            return this.abilityEnabled;
        }

        /// <summary>
        /// Returns a bool indicating if the ability is prime and ready to be used. Takes into account cooldowns, if it's already active in a toggled state and if the ability is enabled.
        /// </summary>
        /// <value><c>true</c> if ability ready; otherwise, <c>false</c>.</value>
        public bool isPrimed() {


            if (this.abilityEnabled == true && this.abilityOnCooldown == false && this.isToggled() == false) {
                return true;
            } else {
                return false;
            }
        }


        /// <summary>
        /// Returns a bool indicating if the ability is configured to be a ScrollAbility
        /// </summary>
        /// <returns>True if the ability is a ScrollAbility, else false</returns>
        public bool IsAScrollAbility() {

            if (this.scrollAbility == true) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns a bool indicating if the ability is enabled and configured to be a ScrollAbility
        /// </summary>
        /// <returns>True if the ability is enabled and a ScrollAbility</returns>
        public bool IsAnEnabledScrollAbility() {

            if (this.abilityEnabled == true && this.scrollAbility == true) {
                return true;
            } else {
                return false;
            }

        }


        /// <summary>
        /// Returns a bool indicating if the abilities ScrollQuickKey has been pressed. 
        /// </summary>
        /// <remarks>
        /// If triggered then the ABC Controller will currently switch to this ability making it the current active scroll ability ready to be initiated. Equipping the scroll ability on the press of the button instead of cycling through
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if the scroll quick key has been pressed, else false</returns>
        public bool ScrollQuickKeyPressed(ABC_IEntity Originator) {

            return this.ButtonPressed(Originator, AbilityButtonPressState.ScrollQuickKey);

        }

        /// <summary>
        /// Returns a bool indicating if the the trigger has been pressed to enable ability collision
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        public bool EnableCollisionTriggerPressed(ABC_IEntity Originator) {

            return this.ButtonPressed(Originator, AbilityButtonPressState.CollisionEnabledAfterTrigger);
        }

        /// <summary>
        /// Will initialise the scroll ability in game, including loading any graphics and performing animations setup for the ability. 
        /// </summary>
        /// <remarks>
        /// Currently only used for when scroll abilities are 'equipped' by the entity
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="ActivateAesthetic">If true then the animation and graphic will activate, else it won't</param>
        public IEnumerator InitialiseScrollAbility(ABC_IEntity Originator, bool ActivateAesthetic = true) {

            if (Originator == null || this.IsAnEnabledScrollAbility() == false)
                yield break;

            Originator.TemporarilyDisableAbilityActivation(this.scrollSwapDuration);



            // add equip to log
            if (Originator.LogInformationAbout(LoggingType.ScrollAbilityEquip))
                Originator.AddToAbilityLog(this.name + " has been equipped");


            //If clip count is -1 (default game start) then it's not been set yet. We will find out the starting clip count using Mod if we have starting ammo
            if (this.useReload && this.currentAmmoClipCount == -1) {

                //No division by 0
                if (this.clipSize == 0)
                    this.clipSize = 1;

                //If we have no ammo then can just set current ammo clip count to 0
                if (this.ammoCount == 0) {
                    this.currentAmmoClipCount = 0;
                } else if (this.ammoCount >= this.clipSize && this.reloadFillClip) {
                    //If reload fill clip is true then set current clip count to clipsize
                    this.currentAmmoClipCount = this.clipSize;
                } else {
                    //Find current clip using mod on ammo count and clip size
                    this.currentAmmoClipCount = this.ammoCount % this.clipSize;


                    //If clipcount is 0 due to mod calculation then make count equal to clipsize 
                    if (currentAmmoClipCount == 0)
                        currentAmmoClipCount = clipSize;
                }


                // remove current clip count from ammo (it's not in our ammo stock as it's current in use in our clip) 
                this.ammoCount -= this.currentAmmoClipCount;



            }

            //Update gui
            this.DisplayIcon(Originator.abilityImageGUI);
            this.DisplayAmmoOnGUI(Originator.abilityAmmoGUI);


            if (this.useScrollAbilityAesthetics) {

                //Activate graphic after duration - If ActivateAesthetic is false then we will pass true to ignore the delay show graphic instantly
                yield return scrollAbilityAesthetic = this.ActivateGraphic(Originator, AbilityGraphicType.ScrollAbilityActivation, (ActivateAesthetic) ? false : true);

                // play animation if parameter has not been set to activate graphic only 
                if (ActivateAesthetic) {

                    //Track what time this method was called
                    //Stops overlapping i.e if another part of the system activated the same call
                    //this function would not continue after duration
                    float functionRequestTime = Time.time;

                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, false));

                    //Start animation on ABC runner 
                    if (this.scrollAbilityAnimationRunnerOnEntity)
                        this.StartAnimationRunner(AbilityAnimationState.ScrollActivate, Originator.animationRunner);

                    if (this.scrollAbilityAnimationRunnerOnScrollGraphic)
                        this.StartAnimationRunner(AbilityAnimationState.ScrollActivate, GetCurrentScrollAbilityAnimationRunner(Originator));

                    if (this.scrollAbilityAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAnimationRunner(AbilityAnimationState.ScrollActivate, ar));

                    if (this.scrollAbilityAnimateOnEntity)
                        this.StartAnimation(AbilityAnimationState.ScrollActivate, Originator.animator);

                    //If enabled then activate the animation on the graphic object
                    if (this.scrollAbilityAnimateOnScrollGraphic)
                        this.StartAnimation(AbilityAnimationState.ScrollActivate, this.GetCurrentScrollAbilityAnimator(Originator));

                    if (this.scrollAbilityAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.StartAnimation(AbilityAnimationState.ScrollActivate, a));


                    //track the animation runner/animator duration
                    float animationRunnerDuration = (this.scrollAbilityAnimationRunnerClip.AnimationClip != null ? this.scrollAbilityAnimationRunnerClipDuration : 0f) + this.scrollAbilityAnimationRunnerClipDelay;
                    float animatorDuration = this.scrollAbilityAnimatorParameter != string.Empty ? this.scrollAbilityAnimatorDuration : 0f;

                    //Determines when the animation has ended (If no animation has been setup then it will default to already being completed
                    bool animationRunnerComplete = this.scrollAbilityAnimationRunnerClip.AnimationClip != null ? false : true;
                    bool animatorComplete = this.scrollAbilityAnimatorParameter != string.Empty ? false : true;

                    //Work out which duration is higher
                    float maxAnimationDuration = Mathf.Max(animationRunnerDuration, animatorDuration);

                    //Loop through to cancel animations early if ability interuptted 
                    for (var i = maxAnimationDuration; i > 0; i--) {

                        //Wait for seconds
                        if (Mathf.Min(animationRunnerDuration, animatorDuration) > 0 && Mathf.Min(animationRunnerDuration, animatorDuration) < 1) {
                            // less then 1 second so we only need to wait the .xx time
                            yield return new WaitForSeconds(Mathf.Min(animationRunnerDuration, animatorDuration));
                        } else if (i < 1) {
                            // less then 1 second so we only need to wait the .xx time
                            yield return new WaitForSeconds(i);
                        } else {
                            // wait a section and keep looping till casting time = 0; 
                            yield return new WaitForSeconds(1);
                        }


                        //Take of a second from the duration trackers
                        animationRunnerDuration -= 1;
                        animatorDuration -= 1;

                        //If animation runner duration has ended (reached 0) then end animations
                        if (animationRunnerDuration <= 0 && animationRunnerComplete == false) {
                            animationRunnerComplete = true;

                            if (this.scrollAbilityAnimationRunnerOnEntity)
                                this.EndAnimationRunner(AbilityAnimationState.ScrollActivate, Originator.animationRunner);

                            if (this.scrollAbilityAnimationRunnerOnScrollGraphic)
                                this.EndAnimationRunner(AbilityAnimationState.ScrollActivate, GetCurrentScrollAbilityAnimationRunner(Originator));

                            if (this.scrollAbilityAnimationRunnerOnWeapon)
                                Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(AbilityAnimationState.ScrollActivate, ar));
                        }

                        //If animator duration has ended (reached 0) then end animations
                        if (animatorDuration <= 0 && animatorComplete == false) {
                            animatorComplete = true;

                            if (this.scrollAbilityAnimateOnEntity)
                                this.EndAnimation(AbilityAnimationState.ScrollActivate, Originator.animator);

                            //If enabled then disable the animation on the graphic object
                            if (this.scrollAbilityAnimateOnScrollGraphic)
                                this.EndAnimation(AbilityAnimationState.ScrollActivate, GetCurrentScrollAbilityAnimator(Originator));

                            if (this.scrollAbilityAnimateOnWeapon)
                                Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(AbilityAnimationState.ScrollActivate, a));
                        }


                    }

                    //Enable the IK
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, true));

                }


            }


            //raise event 
            if (this.scrollSetUnsetRaiseEvent)
                Originator.RaiseScrollAbilitySetAndUnsetEvent(this.abilityID, this.name, true);


            //If a reload is required (clip = 0 or reload was interrupted) then reload gun
            if (this.reloadRequired && this.autoReloadWhenRequired && this.useEquippedWeaponAmmo == false)
                ABC_Utilities.mbSurrogate.StartCoroutine(ReloadAmmo(Originator));




        }

        /// <summary>
        /// Will disable the scroll ability including removing any graphics if it's currently active replaying the activate (equip) animation if set too
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="ActivateAesthetic">If true then the animation and graphic will activate, else it won't</param>
        public IEnumerator DeinitializeScrollAbility(ABC_IEntity Originator, bool ActivateAesthetic = true) {

            if (Originator == null)
                yield break;

            //stop any reloading currently happening 
            if (isReloading == true)
                reloadInterrupted = true;

            //disable GUIs
            this.DisplayIcon(Originator.abilityImageGUI, false);
            this.DisplayAmmoOnGUI(Originator.abilityAmmoGUI, false);


            //Deactivate animation
            if (this.useScrollAbilityAesthetics == true) {
                //Activate graphic after duration - If ActivateAesthetic is false then we will pass true to ignore the delay and show graphic instantly
                yield return scrollAbilityAesthetic = this.ActivateGraphic(Originator, AbilityGraphicType.ScrollAbilityDeactivation, (ActivateAesthetic) ? false : true);

                if (ActivateAesthetic == true) {

                    //Track what time this method was called
                    //Stops overlapping i.e if another part of the system activated the same call
                    //this function would not continue after duration
                    float functionRequestTime = Time.time;

                    //Disable iK whilst animating
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, false));

                    //Start animation on ABC runner 
                    if (this.scrollAbilityDeactivateAnimationRunnerOnEntity)
                        this.StartAnimationRunner(AbilityAnimationState.ScrollDeactivate, Originator.animationRunner);

                    if (this.scrollAbilityDeactivateAnimationRunnerOnScrollGraphic)
                        this.StartAnimationRunner(AbilityAnimationState.ScrollDeactivate, GetCurrentScrollAbilityAnimationRunner(Originator));

                    if (this.scrollAbilityDeactivateAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAnimationRunner(AbilityAnimationState.ScrollDeactivate, ar));


                    if (this.scrollAbilityDeactivateAnimateOnEntity)
                        this.StartAnimation(AbilityAnimationState.ScrollDeactivate, Originator.animator);

                    //If enabled then activate the animation on the graphic object
                    if (this.scrollAbilityDeactivateAnimateOnScrollGraphic)
                        this.StartAnimation(AbilityAnimationState.ScrollDeactivate, GetCurrentScrollAbilityAnimator(Originator));

                    if (this.scrollAbilityDeactivateAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.StartAnimation(AbilityAnimationState.ScrollDeactivate, a));


                    //track the animation runner/animator duration
                    float animationRunnerDuration = (this.scrollAbilityDeactivateAnimationRunnerClip.AnimationClip != null ? this.scrollAbilityDeactivateAnimationRunnerClipDuration : 0f) + this.scrollAbilityDeactivateAnimationRunnerClipDelay;
                    float animatorDuration = (this.scrollAbilityDeactivateAnimatorParameter != string.Empty ? this.scrollAbilityDeactivateAnimatorDuration : 0f);

                    //Determines when the animation has ended (If no animation has been setup then it will default to already being completed
                    bool animationRunnerComplete = this.scrollAbilityDeactivateAnimationRunnerClip.AnimationClip != null ? false : true;
                    bool animatorComplete = this.scrollAbilityDeactivateAnimatorParameter != string.Empty ? false : true;

                    //Work out which duration is higher
                    float maxAnimationDuration = Mathf.Max(animationRunnerDuration, animatorDuration);

                    //Loop through to cancel animations early if ability interuptted 
                    for (var i = maxAnimationDuration; i > 0; i--) {

                        //Wait for seconds
                        if (Mathf.Min(animationRunnerDuration, animatorDuration) > 0 && Mathf.Min(animationRunnerDuration, animatorDuration) < 1) {
                            // less then 1 second so we only need to wait the .xx time
                            yield return new WaitForSeconds(Mathf.Min(animationRunnerDuration, animatorDuration));
                        } else if (i < 1) {
                            // less then 1 second so we only need to wait the .xx time
                            yield return new WaitForSeconds(i);
                        } else {
                            // wait a section and keep looping till casting time = 0; 
                            yield return new WaitForSeconds(1);
                        }


                        //Take of a second from the duration trackers
                        animationRunnerDuration -= 1;
                        animatorDuration -= 1;

                        //If animation runner duration has ended (reached 0) then end animations
                        if (animationRunnerDuration <= 0 && animationRunnerComplete == false) {
                            animationRunnerComplete = true;

                            if (this.scrollAbilityDeactivateAnimationRunnerOnEntity)
                                this.EndAnimationRunner(AbilityAnimationState.ScrollDeactivate, Originator.animationRunner);

                            if (this.scrollAbilityDeactivateAnimationRunnerOnScrollGraphic)
                                this.EndAnimationRunner(AbilityAnimationState.ScrollDeactivate, GetCurrentScrollAbilityAnimationRunner(Originator));

                            if (this.scrollAbilityDeactivateAnimationRunnerOnWeapon)
                                Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(AbilityAnimationState.ScrollDeactivate, ar));
                        }

                        //If animator duration has ended (reached 0) then end animations
                        if (animatorDuration <= 0 && animatorComplete == false) {
                            animatorComplete = true;

                            if (this.scrollAbilityDeactivateAnimateOnEntity)
                                this.EndAnimation(AbilityAnimationState.ScrollDeactivate, Originator.animator);

                            //If enabled then disable the animation on the graphic object
                            if (this.scrollAbilityDeactivateAnimateOnScrollGraphic)
                                this.EndAnimation(AbilityAnimationState.ScrollDeactivate, GetCurrentScrollAbilityAnimator(Originator));

                            if (this.scrollAbilityDeactivateAnimateOnWeapon)
                                Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(AbilityAnimationState.ScrollDeactivate, a));
                        }

                    }

                    //Enable IK 
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, true));

                }

            }



            //Clear away any active Aesthetic graphics that might exist
            for (int i = 0; i < this.activeGraphics.Count(); i++)
                ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(this.activeGraphics[i]));


            //If ability is no longer enabled then destroy any persistant graphics
            if (this.abilityEnabled == false && this.scrollAbilityAesthetic != null)
                ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(this.scrollAbilityAesthetic));

            //raise event
            if (this.scrollSetUnsetRaiseEvent)
                Originator.RaiseScrollAbilitySetAndUnsetEvent(this.abilityID, this.name, false);


        }


        /// <summary>
        /// Will return the ABC Animation runner component for the originators current scroll ability. 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>ABC Animation Runner Component</returns>
        public ABC_AnimationsRunner GetCurrentScrollAbilityAnimationRunner(ABC_IEntity Originator = null) {

            //If an animator for this ability exists then return that 
            if (this.scrollAbilityAestheticAnimationRunner != null)
                return this.scrollAbilityAestheticAnimationRunner;

            //if animator doesn't exist then return the originators current scroll ability animator 
            if (Originator != null)
                return Originator.currentScrollAbility.scrollAbilityAestheticAnimationRunner;


            return null;

        }

        /// <summary>
        /// Will return the animator component for the originators current scroll ability. 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Animator Component</returns>
        public Animator GetCurrentScrollAbilityAnimator(ABC_IEntity Originator = null) {

            //If an animator for this ability exists then return that 
            if (this.scrollAbilityAestheticAnimator != null)
                return this.scrollAbilityAestheticAnimator;

            //if animator doesn't exist then return the originators current scroll ability animator 
            if (Originator != null && Originator.currentScrollAbility != null)
                return Originator.currentScrollAbility.scrollAbilityAestheticAnimator;


            return null;

        }

        /// <summary>
        /// Will return the amount of seconds the ability prepared for
        /// </summary>
        /// <returns>number of seconds ability prepared for</returns>
        public float GetCurrentAbilitySecondsPrepared() {

            return this.abilitySecondsPrepared;

        }

        /// <summary>
        /// Will return the remaining cooldown of the ability
        /// </summary>
        /// <param name="ReturnPercentage">If true then the reamining cooldown value returned will be a percentage</param>
        /// <returns>Remaining cooldown of the ability</returns>
        public float GetRemainingCooldown(ABC_IEntity Originator, bool ReturnPercentage = false) {

            //If ability is not on cooldown then return 0 
            if (this.abilityOnCooldown == false)
                return 0;

            //Calculate remaining time
            float cooldownRemaining = this.GetAbilityRecast(Originator) - (Time.time - this.abilityCooldownStartTime);

            //If passed in parameters then return a percentage value
            if (ReturnPercentage)
                return cooldownRemaining / this.GetAbilityRecast(Originator) * 100f;

            return cooldownRemaining;

        }

        /// <summary>
        /// Will return the current ammo count, taking into account clips and reloading
        /// </summary>
        /// <returns>Interger defining the current ammo count</returns>
        public int GetAmmoCount() {

            if (this.scrollAbility == true && this.useReload == true && this.UseAmmo == true) {
                return this.currentAmmoClipCount;
            } else {
                return this.ammoCount;
            }
        }

        /// <summary>
        /// Adjusts ability's ammo by the value provided. Will also update originators ammo GUI if provided.
        /// </summary>
        /// <remarks>
        /// If an originator is provided then the method will retrieve the entitys Ammo Text GUI and update it reflecting the ability's current ammo. 
        /// </remarks>
        /// <param name="Value">Value to adjust ammo by (positive and negative possible)</param>
        /// <param name="Originator">(Optional) Entity which is adjusting ability's ammo </param>
        /// <param name="AdjustAmmoOnly">(Optional) If true then only ammo will be modified, else it's up to logic to decide if ammo or current clip count is changed. </param>
        public void AdjustAmmo(int Value, ABC_IEntity Originator = null, bool AdjustAmmoOnly = false) {

            //If ability doesn't use it's own ammo end here
            if (this.useEquippedWeaponAmmo == true)
                return;

            if (this.scrollAbility == true && this.useReload == true && this.UseAmmo == true) {

                //If we have just set to AdjustAmmoOnly then update count and ignore clip
                if (AdjustAmmoOnly)
                    this.ammoCount += Value;
                else // else change current clip count
                    this.currentAmmoClipCount += Value;



                //If clip has gone over max compacity then add the remainder to the ammo count
                if (this.currentAmmoClipCount > this.clipSize) {
                    this.ammoCount += this.currentAmmoClipCount % clipSize;
                    this.currentAmmoClipCount = this.clipSize;
                }

                if (this.currentAmmoClipCount < 0)
                    this.currentAmmoClipCount = 0;


                //If we have run out of ammo in our clip then set reload required to true 
                if (this.currentAmmoClipCount == 0 && this.reloadRequired == false) {

                    //set reload required to true
                    this.reloadRequired = true;

                    //auto reloading if enabled
                    if (this.autoReloadWhenRequired && currentAmmoClipCount == 0 && IsCurrentScrollAbilityFor(Originator) == true)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.ReloadAmmo(Originator));

                }


            } else {
                this.ammoCount += Value;
            }

            //If ammo count has gone into negative then set to 0
            if (this.ammoCount < 0)
                this.ammoCount = 0;

            // if this ability is a scroll ability and this ability is the current selected scroll ability for the originator and the originator has an ammo gui then we can refresh the number with new calculations
            if (Originator != null && this.scrollAbility == true && IsCurrentScrollAbilityFor(Originator) == true && Originator.abilityAmmoGUI != null)
                this.DisplayAmmoOnGUI(Originator.abilityAmmoGUI);


        }

        /// <summary>
        /// Determines if the scroll ability is reloading
        /// </summary>
        /// <returns>True if scroll ability is reloading, else false</returns>
        public bool IsReloading() {

            return this.isReloading;

        }

        /// <summary>
        /// Will reload the abilitys Ammo
        /// </summary>
        /// <remarks>
        /// Reload is only possible for scroll abilities. 
        /// </remarks>
        /// <param name="Originator">Entity reloading the ability</param>
        public IEnumerator ReloadAmmo(ABC_IEntity Originator) {

            //Reload is only functionality for scroll abilities. 
            // If We have no ammo to reload or this isn't a scroll ability then stop the function here
            // Also we won't reload if we are on a fresh clip that hasnt been used yet or we already reloading
            if (this.useEquippedWeaponAmmo == true || this.ammoCount <= 0 || this.IsAnEnabledScrollAbility() == false || this.currentAmmoClipCount == clipSize || this.useReload == false || this.isReloading)
                yield break;


            //let rest of code know we are reloading and reload is required (incase we are interrupted) 
            this.isReloading = true;
            this.reloadRequired = true;


            // turn canCast off for the reloading duration so we can't cast ANY spells whilst we are reloading
            Originator.TemporarilyDisableAbilityActivation(this.reloadRestrictAbilityActivationDuration);

            //update logs
            Originator.AddToDiagnosticLog("Reloading " + this.name);

            if (Originator.LogInformationAbout(LoggingType.AmmoInformation))
                Originator.AddToAbilityLog("Reloading " + this.name);


            //Tracks reload graphic
            GameObject reloadGraphic = null;

            //Determines if reload graphic should activate
            bool activateReloadGraphic = false;

            //Track what time this method was called
            //Stops overlapping i.e if another part of the system activated the same call
            //this function would not continue after duration
            float functionRequestTime = Time.time;


            //activate reloading Aesthetic if enabled
            if (this.useReloadAbilityAesthetics) {

                //Disable IK whilst animating
                ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, false));

                activateReloadGraphic = true;

                //Start animation on ABC runner 
                if (this.reloadAbilityAnimationRunnerOnEntity)
                    this.StartAnimationRunner(AbilityAnimationState.Reload, Originator.animationRunner);

                if (this.reloadAbilityAnimationRunnerOnScrollGraphic)
                    this.StartAnimationRunner(AbilityAnimationState.Reload, GetCurrentScrollAbilityAnimationRunner(Originator));

                if (this.reloadAbilityAnimationRunnerOnWeapon)
                    Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAnimationRunner(AbilityAnimationState.Reload, ar));

                if (this.reloadAbilityAnimateOnEntity)
                    this.StartAnimation(AbilityAnimationState.Reload, Originator.animator);

                //If enabled then activate the animation on the graphic object
                if (this.reloadAbilityAnimateOnScrollGraphic)
                    this.StartAnimation(AbilityAnimationState.Reload, GetCurrentScrollAbilityAnimator(Originator));

                if (this.reloadAbilityAnimateOnWeapon)
                    Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.StartAnimation(AbilityAnimationState.Reload, a));

            }


            //While loop tracker
            float reloadTimeTracker = Time.time;

            while (this.isReloading) {

                yield return new WaitForSeconds(0.5f);

                //If the originator activates another ability during reloading then we will wait for that to finish then interrupt the reload (to not mess with the other ability activation animations)
                while (Originator.activatingAbility == true) {
                    yield return new WaitForEndOfFrame();
                    this.reloadInterrupted = true;
                }


                //if interrupted or we have ran out of ammo then break early 
                if (this.reloadInterrupted || ammoCount == 0) {

                    //remove the graphic 
                    if (reloadGraphic != null)
                        ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(reloadGraphic));

                    //exit wait loop early 
                    this.isReloading = false;
                }

                //If reload graphic is set to activate then activate graphic
                if (activateReloadGraphic == true) {
                    reloadGraphic = ActivateGraphic(Originator, AbilityGraphicType.Reloading);
                    activateReloadGraphic = false; // make sure loop doesn't activate reload graphic again unless told to later
                }

                //If the reloading animation duration has not passed since we started reloading then continue while loop
                if (Time.time - reloadTimeTracker < this.reloadDuration || this.isReloading == false)
                    continue;




                //Reload animaiton duration has passed - if we are not filling a clip bit by bit then we should have finished reloading a whole clip 
                if (this.reloadFillClip == false) {

                    //If ammo count is less then clip size then make current clip count equal to ammo and set ammo to 0
                    if (this.ammoCount < clipSize) {
                        this.currentAmmoClipCount = this.ammoCount;
                        this.ammoCount = 0;
                    } else {

                        //Else remove clip size from our ammo stack and make current ammo clip full capacity 
                        this.ammoCount -= clipSize;
                        this.currentAmmoClipCount = clipSize;
                    }

                    //break the loop
                    this.isReloading = false;

                } else {
                    //If we are filling the clip then we will adjust the ammo up after every animatior duration (like adding shotgun shell every x duration) instead of wasting a magazine 

                    //if set to repeat reload graphic, then set variable to activate the graphic again
                    if (this.reloadFillClipRepeatGraphic && this.useReloadAbilityAesthetics)
                        activateReloadGraphic = true;

                    //adjust ammo bit by bit 
                    this.ammoCount--;
                    this.AdjustAmmo(1, Originator);

                    //If we reach the clip capacity then we can end here 
                    if (this.currentAmmoClipCount == clipSize)
                        this.isReloading = false;

                    //Record the current time so we know when next duration is up to increase the ammo (put another shell in)
                    reloadTimeTracker = Time.time;


                }



            }


            //end reloading Aesthetic if enabled
            if (this.useReloadAbilityAesthetics) {

                //Start animation on ABC runner 
                if (this.reloadAbilityAnimationRunnerOnEntity)
                    this.EndAnimationRunner(AbilityAnimationState.Reload, Originator.animationRunner);

                if (this.reloadAbilityAnimationRunnerOnScrollGraphic)
                    this.EndAnimationRunner(AbilityAnimationState.Reload, GetCurrentScrollAbilityAnimationRunner(Originator));

                if (this.reloadAbilityAnimationRunnerOnWeapon)
                    Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(AbilityAnimationState.Reload, ar));

                //If enabled then disable the animation on entity
                if (this.reloadAbilityAnimateOnEntity)
                    this.EndAnimation(AbilityAnimationState.Reload, Originator.animator);

                //If enabled then disable the animation on the graphic object
                if (this.reloadAbilityAnimateOnScrollGraphic)
                    this.EndAnimation(AbilityAnimationState.Reload, GetCurrentScrollAbilityAnimator(Originator));

                if (this.reloadAbilityAnimateOnWeapon)
                    Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(AbilityAnimationState.Reload, a));

                //Enable IK
                ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(functionRequestTime, true));

            }


            //If the reload has been interrupted then just exit the whole function - as reload will still be required when scroll ability is next equipped it will restart the reloading
            if (reloadInterrupted) {

                //If we filled a clip before it got interrupted then we no longer need to reload
                if (this.reloadFillClip && this.currentAmmoClipCount > 0)
                    this.reloadRequired = false;

                //reset variable
                this.reloadInterrupted = false;

                //break out of function
                yield break;

            }

            //We no longer need to reload
            this.reloadRequired = false;


            //Update GUI
            if (Originator != null) {
                // if this ability is a scroll ability and this ability is the current selected scroll ability for the originator and the originator has an ammo gui then we can refresh the number with new calculations
                if (this.scrollAbility == true && IsCurrentScrollAbilityFor(Originator) == true && Originator.abilityAmmoGUI != null)
                    this.DisplayAmmoOnGUI(Originator.abilityAmmoGUI);
            }


        }

        /// <summary>
        /// Will change the GUI Text provided showing how much ammo the ability currently has.
        /// </summary>
        /// <param name="AmmoGUIText">GUI Text object which will be used to show the ammo</param>
        /// <param name="Enabled">If true will display ammo information, else will hide the display</param>
        public void DisplayAmmoOnGUI(ABC_TextReference AmmoGUIText, bool Enabled = true) {

            //If no GUI text then end function here 
            if (AmmoGUIText == null)
                return;

            //If ability is not enabled, has no ammo or is not a scroll ability then turn the GUI text off and end here
            if (AmmoGUIText != null && (Enabled == false || this.UseAmmo == false || this.scrollAbility == false)) {
                AmmoGUIText.Graphic.enabled = false;
                return;
            }


            //Enable GUI 
            AmmoGUIText.Graphic.enabled = true;

            //If we can reload then show clip and max ammo, else just show ammo
            if (this.useReload == true) {
                AmmoGUIText.text = currentAmmoClipCount + " | " + ammoCount;
            } else {
                AmmoGUIText.text = this.ammoCount.ToString();
            }

        }


        /// <summary>
        /// Will change the RawImage provided to display the Ability's Icon 
        /// </summary>
        /// <param name="ImageGUI">RawImage object which will be used to display the Icon</param>
        /// <param name="Enabled">If true then icon will display, else it will be hidden</param>
        public void DisplayIcon(RawImage ImageGUI, bool Enabled = true) {

            // if we are turning on the icon in the GUI and the ImageGUI is not null or turned off (i.e it has been setup)
            if (Enabled == true && ImageGUI != null) {

                ImageGUI.enabled = true;

                // if our current scroll ability has an icon image
                if (this.iconImage.Texture2D != null) {
                    // set it to the image GUI
                    ImageGUI.texture = this.iconImage.Texture2D;

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

            } else if (Enabled == false && ImageGUI != null && ImageGUI.enabled == true) {
                // if we are turning off then just turn off the texture and make transparent 
                ImageGUI.texture = null;

                ImageGUI.enabled = false;

            }



        }


        /// <summary>
        /// Call to activate the ability, will check if the ability can activate, setting off the activation if it can. Returns a value indicating if the activation was successful or not 
        /// </summary>
        /// <param name="Originator">Entity checking if ability can be activated</param>
        ///<param name="IgnoreTriggerCheck">If true then function will not check if the ability has been triggered or not. Use if you want to know if ability can activate before it is triggered</param>
        ///<param name="IgnoreActivationPermittedCheck">If true then function will not check if the originator is allowed to activate abilities.</param>
        ///<param name="IgnoreComboCheck">If true then function will not run check combo checks which determines if the ability is next in line to activate.</param>
        ///<param name="IgnoreHoldPreparation">If true then the function will not use the hold to continue preparation functionality</param>
        /// <returns>True if ability succesfully activates else, false</returns>
        public bool Activate(ABC_IEntity Originator, bool IgnoreTriggerCheck = false, bool IgnoreActivationPermittedCheck = false, bool IgnoreComboCheck = false, bool IgnoreHoldPreparation = false) {

            //Check if ability can activate (has correct triggers and targets)
            if (this.CanActivate(Originator, IgnoreTriggerCheck, IgnoreActivationPermittedCheck, IgnoreComboCheck) == false)
                return false;

            //Stop weapon blocking
            Originator.StopWeaponBlocking();

            //If true then the function will not use the hold to continue preparation functionality
            this.IgnoreHoldPreparation = IgnoreHoldPreparation;

            //Activate the ability
            ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateAbility(Originator));

            //If ability was interrupted and did not activate then return false
            if (this.AbilityActivationInterrupted())
                return false;


            //ability activated ok so return true
            return true;

        }


        /// <summary>
        /// Will flag the auto cast setting which will trigger ability activation
        /// </summary>
        public void AutoCast(ABC_IEntity Originator) {

            //If this item is combo blocked then auto cast the next ability in the combo chain instead
            if (this.IsComboBlocked(Originator))
                Originator.TriggerAbility(Originator.GetAbilityNextAvaliableComboID(this.abilityID));
            else
                this.autoCast = true;


        }

        /// <summary>
        /// Interrupts this ability stopping it from being activated
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="InterruptLinkedAbilities">If true then any linked abilities will be interrupted also</param>
        /// <param name="HitInterrupted">(Optional) If true then ability activation was interrupted by a hit</param>
        public void InterruptAbilityActivation(ABC_IEntity Originator = null, bool HitInterrupted = false) {

            //Already interrupted so end here
            if (this.abilityActivationInterrupted == true)
                return;

            //If this ability was interrupted by a hit but the hit won't interrupt activation setting is true then end here and don't interrupt the ability
            if (HitInterrupted == true && this.hitPreventionWontInterruptActivation == true)
                return;


            //Interrupt this ability
            this.abilityActivationInterrupted = true;

            if (this.abilityType == AbilityType.Melee && (this.initiatingUseWeaponTrail == true || this.additionalStartingPositions.Where(asp => asp.useWeaponTrail == true).Count() > 0))
                Originator.InterruptEquippedWeaponTrails();

            //If ability was interrupted early then raise the activation complete event 
            if (this.abilityActivationCompleteRaiseEvent == true)
                this.RaiseOriginatorsAbilityActivationCompleteEvent(Originator);

            //make sure entity can still move if previously restricted 
            if (Originator != null && (this.stopMovementOnPreparingRaiseEvent == true || this.stopMovementOnInitiateRaiseEvent == true))
                ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, true));


            if (Originator != null)
                Originator.ToggleMovement(Time.time, true, true, true);

        }


        /// <summary>
        /// Bool that determines if the ability has been interrupted
        /// </summary>
        /// <remarks>Method has been created as this is checked in a lot of places so allows for easier modification of the check in future with the possibility of including an originator (IEntity) parameter</remarks>
        public bool AbilityActivationInterrupted() {

            //return ability activation interrupted flag
            return this.abilityActivationInterrupted;

        }

        /// <summary>
        /// Locks the ability as it has just been activated as part of a combo and can't be activated again until combo has been completed or reset
        /// </summary>
        /// <param name="ObjHit">Object which the ability hit/collided with as part of the combo</param>
        public void SetComboLock(GameObject ObjHit = null) {

            //Set combo lock to true so it won't activate again until the combo has timed out
            if (this.abilityCombo == true && this.comboLocked == false) {
                this.comboLockedTime = Time.time;
                this.comboLocked = true;
            }


            //If we have been provided an object then it's a combo hit. If ComboHitRequired setting is true then the combo won't continue unless a hit has been made or it has been reset due to the combo timing out
            //the combolock hit is recorded if the ability has already been combolocked (it has been activated) and the hit has a statemanager. It's tracked even if combo hit isn't required incase for future requirements
            if (ObjHit != null && ABC_Utilities.GetStaticABCEntity(ObjHit).HasABCStateManager() && this.comboLocked == true) {
                this.comboLockHit = true;
            }

        }

        /// <summary>
        /// Handles abilities that are in a combo. Will determine if the ability passed through should activate based against combo conditions like the ability being next in the combo.
        /// All abilities with the same key are put into the same combo group.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="IgnoreComboLockResets">(Optional) If true then no combo locks will be reset, else resets will occur as normal</param>
        /// <param name="AIActivated">(Optional)If true then combo check is done by AI which means function will add leeway onto the combo next time, allowing for more time to pass before combo resets </param>
        /// <returns><c>true</c>Ability passes combo conditions and should be allowed to activate. <c>false</c> Ability is not next in the combo group and shouldn't be allowed to activate.</returns>
        public bool IsComboBlocked(ABC_IEntity Originator, bool IgnoreComboLockResets = false, bool AIActivated = false) {

            //Determine combo next activate leeway (if AI this is increased)
            float ComboNextActivateTimeLeeway = 0f;

            if (AIActivated == true)
                ComboNextActivateTimeLeeway = 4f;

            // ability has not been set up as a combo, is a scroll ability or is trigger by an input combo (different combo system F, F, B etc) so we can return true as it is allowed to activate
            if (this.abilityCombo == false || this.triggerType == TriggerType.InputCombo || this.scrollAbility == true) {
                // ability is not a combo so we have just broken any combos that we previously had so we can reset them all and return true 

                if (IgnoreComboLockResets == false && AIActivated == false)
                    ResetAllComboLocks(Originator);

                return false;
            }


            // if another ability combo group has been used midway then it interuppted any ongoing combos so we need to reset all locks 
            if (IgnoreComboLockResets == false && (this.keyInputType == InputType.Key && Originator.currentComboKey != this.key || this.keyInputType == InputType.Button && Originator.currentComboButton != this.keyButton))
                ResetAllComboLocks(Originator);


            // set new current combo key/button which can also be used to show what combo you are currently on
            if (this.keyInputType == InputType.Key)
                Originator.SetCurrentComboKey(this.key);

            if (this.keyInputType == InputType.Button)
                Originator.SetCurrentComboButton(this.keyButton);


            // get a temp list to collect all abilities that are grouped and chosen to be part of a combo 
            List<ABC_Ability> tempComboAbilities = new List<ABC_Ability>();
            List<ABC_Ability> Abilities = Originator.CurrentAbilities;

            int positionInList = 0;

            for (int i = 0; i < Abilities.Count; i++) {
                // if the input of the ability is the same then add to temp list as long as it's enabled, not a scroll ability and its an ability combo and it has the same air/land type
                if ((Abilities[i].key == this.key && this.keyInputType == InputType.Key || Abilities[i].keyButton == this.keyButton && this.keyInputType == InputType.Button)
                    && Abilities[i].scrollAbility == false && Abilities[i].abilityCombo == true && Abilities[i].isEnabled() == true && (Abilities[i].LandOrAir == AbilityLandOrAir.LandOrAir || Abilities[i].LandOrAir == this.LandOrAir)) {

                    tempComboAbilities.Add(Abilities[i]);
                    // if abilities compared is the same track what index it was (where in the group the ability is order wise)
                    if (Abilities[i] == this) {
                        positionInList = tempComboAbilities.Count - 1;
                    }
                }
            }


            // now we need to find the last ability to be activiated in the combo group (which ability was last combo locked) 
            ABC_Ability lastComboLocked = null;

            foreach (ABC_Ability element in tempComboAbilities) {
                if (element.comboLocked == true && (lastComboLocked == null || element.comboLockedTime > lastComboLocked.comboLockedTime)) {
                    lastComboLocked = element;
                }
            }


            //If this is the first ability in the combo and the combo has already been locked all way through and leeway has been entered then convert this to 1 as we don't want the entity to wait ages to restart the combo
            if (ComboNextActivateTimeLeeway > 0f && positionInList == 0 && lastComboLocked != null && lastComboLocked == tempComboAbilities[tempComboAbilities.Count - 1])
                ComboNextActivateTimeLeeway = 1f;



            // if the ability firing is first in the group and the combo has been activiated before and the user has pressed the combo button after the designated nextactivationtime (how long user has to press button again for combo to work)
            //We don't modify this by the initiation speed as the combo is done and we are happy for the entity to start the combo again
            if (positionInList == 0 && lastComboLocked != null && Time.time - lastComboLocked.comboLockedTime > (lastComboLocked.comboNextActivateTime + lastComboLocked.hitStopCurrentTotalExtendDuration + ComboNextActivateTimeLeeway)) {
                // time is up and combo has been interuppted we can now reset all our comboLocks passing through the bypass as we 100% want to reset and have nothing block it

                if (IgnoreComboLockResets == false)
                    ResetAllComboLocks(Originator, true);

                return false;
            }


            // if group position is 0 (first in line) and lastcombo is nothing then this is the first time the combo has been activated so we return true 
            // else if the tempAbilities count only has 1 in then fire as we don't have a second ability to combo with
            // else if the last combo locked is not null (so we are in the middle of a combo) and the time between key presses is less then the number entered in the previous abilities combo next time
            // and the combo hit requirement from the previous ability is satisfied (either a hit isn't required to continue the combo or a hit was made before the timeout)
            // then we can continue with the combo and fire
            if (positionInList == 0 && lastComboLocked == null || lastComboLocked != null && this.comboLocked == false && Time.time - lastComboLocked.comboLockedTime < Mathf.Max(lastComboLocked.abilityCurrentInitiationSpeedAdjustment, this.ModifyTimeByInitiatingAdjustments(Originator, lastComboLocked.comboNextActivateTime + lastComboLocked.hitStopCurrentTotalExtendDuration) + ComboNextActivateTimeLeeway) && (lastComboLocked.comboHitRequired == false || lastComboLocked.comboHitRequired == true && lastComboLocked.comboLockHit == true) || tempComboAbilities.Count <= 1) {

                return false;

            } else {

                return true;
            }

        }


        /// <summary>
        /// Returns a bool indicating if the ability melee attack was interrupted. If the attack is interrupted it will activate stagger on the originator entity and end the initiating animation and melee 'projectile'. Will return false always if ability type is not Melee.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if melee attach has been interrupted, else false</returns>
        public bool MeleeAttackInterrupted(ABC_IEntity Originator) {

            //Not melee type so can't be interrupted so return false
            if (this.abilityType != AbilityType.Melee)
                return false;


            // If we arn't restricted by a hit or this melee attack will never stagger then return false
            if (this.AbilityActivationInterrupted() == false || this.hitsStopMeleeAttack == false)
                return false;

            //If melee attack has not been interrupted yet then stop the activation now
            if (this.AbilityActivationInterrupted() == false)
                this.InterruptAbilityActivation();


            //Melee attack has been interrupted by a recent hit so return true
            return true;


        }


        /// <summary>
        /// Will activate hit stop, pausing the animation/attack for the originator and entity hit for the duration provied
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="EntityHit">Entity hit which started the hitstop</param>
        public IEnumerator ActivateHitStop(ABC_IEntity Originator, ABC_IEntity EntityHit) {

            //If not enable or initiation speed has been modified
            if (this.enableHitStopOnImpact == false)
                yield break;

            //Track what time this method was called
            //Stops overlapping i.e if another part of the system activated hit stop
            //this function would not continue after duration
            float functionRequestTime = Time.time;

            //wait for duration then end animation
            if (this.hitStopOnImpactDelay > 0) {
                for (var i = hitStopOnImpactDelay; i > 0; i = i - 0.2f) {

                    if (this.AbilityActivationInterrupted())
                        break;

                    // actual wait time 
                    if (i < 0.2f) {
                        // less then 0.2  so we only need to wait the .xx time
                        yield return new WaitForSeconds(i);
                    } else {
                        // wait and keep looping till casting time = 0; 
                        yield return new WaitForSeconds(0.2f);
                    }

                }
            }

            //If hitstop is already active or ability interrupted then end here
            if (this.hitStopCurrentlyActive == true || this.AbilityActivationInterrupted())
                yield break;


            //Let system know this ability has hit stop active 
            this.hitStopCurrentlyActive = true;

            //Local setting for now
            float hitStopSpeed = 0f;
            float originatorOriginalSpeed = Originator.animator.speed;
            float entityHitOriginalSpeed = EntityHit.animator != null ? EntityHit.animator.speed : 1;

            //If melee then 
            if (this.abilityType == AbilityType.Melee) {
                //record how much time the hitstop is going to be running for to increase combo time/activation time/projectile duration etc
                //since we freezing the attack we want it to last a bit longer then configured which this property manages in the system
                this.hitStopCurrentTotalExtendDuration = this.hitStopOnImpactDuration;

                //Stop animator for originator doing the actual melee attack
                Originator.ModifyAnimatorSpeed(functionRequestTime, hitStopSpeed);
            }

            //wait for time configured before we hit stop the entity that was hit (lets them react a bit before hitstopping)
            for (var i = this.hitStopOnImpactEntityHitDelay; i > 0; i = i - 0.2f) {

                if (this.AbilityActivationInterrupted()) {

                    //Revert animator back to normal speed as ability interrupted
                    Originator.ModifyAnimatorSpeed(functionRequestTime, originatorOriginalSpeed);

                    break;

                }


                // actual wait time 
                if (i < 0.2f) {
                    // less then 0.2  so we only need to wait the .xx time
                    yield return new WaitForSeconds(i);
                } else {
                    // wait and keep looping till casting time = 0; 
                    yield return new WaitForSeconds(0.2f);
                }


            }


            //Stop animator on entity hit and prevent any ability activation
            if (EntityHit.HasABCStateManager()) {
                EntityHit.ModifyAnimatorSpeed(functionRequestTime, hitStopSpeed);
                EntityHit.HitRestrictAbilityActivation(this.hitStopOnImpactDuration - this.hitStopOnImpactEntityHitDelay);
            }


            //wait for remaining hit stop duration (already some time in due to waiting on entity hit delay)
            for (var i = this.hitStopOnImpactDuration - this.hitStopOnImpactEntityHitDelay; i > 0; i = i - 0.2f) {

                if (this.AbilityActivationInterrupted()) {
                    //Revert animators back to normal speed as ability interrupted
                    Originator.ModifyAnimatorSpeed(functionRequestTime, originatorOriginalSpeed);

                    if (EntityHit.HasABCStateManager())
                        EntityHit.ModifyAnimatorSpeed(functionRequestTime, entityHitOriginalSpeed);

                    break;
                }

                // actual wait time 
                if (i < 0.2f) {
                    // less then 0.2  so we only need to wait the .xx time
                    yield return new WaitForSeconds(i);
                } else {
                    // wait and keep looping till casting time = 0; 
                    yield return new WaitForSeconds(0.2f);
                }


            }


            //Revert animators back to normal speed
            Originator.ModifyAnimatorSpeed(functionRequestTime, originatorOriginalSpeed);

            if (EntityHit.HasABCStateManager())
                EntityHit.ModifyAnimatorSpeed(functionRequestTime, entityHitOriginalSpeed);

            //Small breathing space
            yield return new WaitForSeconds(0.1f);

            //Tell system we no longer in hit stop state
            this.hitStopCurrentlyActive = false;

        }


        /// <summary>
        /// Will return all effects assigned to this ability
        /// </summary>
        /// <returns>List of effects created for this ability</returns>
        public List<Effect> GetAllEffects() {

            return this.effects;

        }


        /// <summary>
        /// Will apply ability effects to the object provided 
        /// </summary>
        /// <param name="Obj">Object which will have the abilities effects added</param>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Projectile">(Optional) Object that we collided with which started the process of adding an effect - Used in some range settings etc</param>
        /// <param name="HitPoint">(Optional) where ability/raycast hit. Used for effect graphics </param>
        /// <param name="AddedBySplash">(Optional) If true then effect was added by splash functionality </param>
        /// <param name="IsEffectLink">(Optional) If true then the effect has been applied via ability effect link</param>
        /// <param name="SecondsPrepared">(Optional) If provided then seconds will be used for prepare time effect adjustments </param>
        public void ApplyAbilityEffectsToObject(GameObject Obj, ABC_IEntity Originator, GameObject Projectile = null, Vector3 HitPoint = default(Vector3), bool AddedBySplash = false, bool IsEffectLink = false, float SecondsPrepared = 0f) {

            // get ientity to check state manager script (we only effect these types of objects) and later add effects
            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(Obj);

            //If statemanager couldn't be found then end here
            if (iEntity.HasABCStateManager() == false)
                return;

            //If Obj is ignoring ability collision and this ability doesn't override that setting then we can't apply effects so end here
            if (iEntity.ignoringAbilityCollision == true && this.overrideIgnoreAbilityCollision == false)
                return;

            //Grab all potential effects to add
            List<Effect> potentialEffectsToAdd = this.effects.ToList();

            //Add effects from any ability effect links: 
            //Apply effects from abilities which have been linked
            if (this.enableAbilityEffectLinks) {

                //activate ability effects on any IDs that don't match this ability (to avoid infinite effect loops)
                foreach (int ID in this.effectLinkAbilityIDs.Where(a => a != this.abilityID).ToList())
                    potentialEffectsToAdd.AddRange(Originator.GetAbilitiesEffects(ID));
            }

            //The final list of effects we adding
            List<Effect> effectsToAdd = new List<Effect>();

            //add to final effects to add
            foreach (Effect effect in potentialEffectsToAdd) {

                //If not a global effect then just add normal effect
                if (effect.globalEffect == null) {
                    effectsToAdd.Add(effect);
                } else {
                    //Else add the global effects
                    effectsToAdd.AddRange(effect.globalEffect.ElementEffects);
                }

            }


            //If we not splashing then remove any effects set to only add during a splash
            if (AddedBySplash == false) {
                effectsToAdd.RemoveAll(e => e.requireSplashToActivate == true);
            }

            //If preparing then remove any effects that require certain prepare time (greater or less than) to activate
            if (prepareTime > 0) {
                effectsToAdd.RemoveAll(e => e.specificPrepareTimeRequried == true && (e.specificPrepareTimeArithmeticComparison == ArithmeticComparisons.GreaterThan && SecondsPrepared < e.prepareTimeToActivate
                || e.specificPrepareTimeArithmeticComparison == ArithmeticComparisons.LessThan && SecondsPrepared > e.prepareTimeToActivate));
            } else { // else if not preparing then just remove all effects that require a prepare time
                effectsToAdd.RemoveAll(e => e.specificPrepareTimeRequried == true);
            }


            //Apply effects
            //if activate specific hit animation is also true then provide the name of animation to activate 
            // else add blank string to activate random hit animation setup (if they pass probability checks and setup to activate from hit etc)
            iEntity.AddEffectsToEntity(this.name, effectsToAdd, Originator, this.abilityType, IsEffectLink, Projectile, Obj, HitPoint, this.activateAnimationFromHit, this.activateAnimationFromHitDelay, this.activateSpecificHitAnimation && this.activateSpecificHitAnimationUseClip ? this.hitAnimationClipToActivate.AnimationClip : null, this.activateSpecificHitAnimation ? this.hitAnimationToActivate : "", this.overrideWeaponBlocking, this.reduceWeaponBlockDurability, this.overrideWeaponParrying, this.pushEntityOnImpact ? this.pushEntityOnImpactAmount : 0f, this.pushEntityOnImpact ? this.pushEntityOnImpactLiftForce : 0f, this.pushEntityOnImpactDelay, this);



            //Since we hit a new object, make sure they can retaliate and target us back without any limit restrictions (tell the originator who hit that the entity they hit can now have priority to target the originator back)
            Originator.AddObjectAsPriorityTargeter(Obj);


        }


        /// <summary>
        /// Will activate graphics setup for the ability at the position and rotation provided. Graphic which is activated depends on the type passed.
        /// </summary>
        /// <param name="GraphicType">Type of graphic to activate: SpawnObject or End (when ability is destroyed/disabled) </param>
        /// <param name="Position">Position that graphic will appear</param>
        /// <param name="Rotation">Rotation of Graphic</param>
        /// <returns>Will return the graphic gameobject which has been created</returns>
        public GameObject ActivateGraphicAtPosition(AbilityPositionGraphicType GraphicType, Vector3 Position, Quaternion Rotation = default(Quaternion)) {

            GameObject graphicObj = null;
            float duration = 2f;


            switch (GraphicType) {
                case AbilityPositionGraphicType.SpawnObject:

                    // if we not set to spawn object then end here
                    if (this.spawnObject == false)
                        return graphicObj;

                    // spawned objects don't have a duration so always set to -1 
                    duration = -1f;

                    graphicObj = this.spawnPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateSpawnedObjects(true);

                    break;
                case AbilityPositionGraphicType.End:


                    // if we not set to spawn object then end here
                    if (this.useAbilityEndAesthetics == false)
                        return graphicObj;

                    // spawned objects don't have a duration so always set to -1 
                    duration = this.abEndAestheticDuration;

                    graphicObj = this.abEndPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateAbilityEndObjects(true);

                    if (graphicObj != null && this.scaleAbilityEndGraphic == true)
                        graphicObj.transform.localScale = new Vector3(this.abilityEndGraphicScale, this.abilityEndGraphicScale, this.abilityEndGraphicScale);



                    break;

            }

            //if graphic object is still null then return here as it has not been setup correctly
            if (graphicObj == null)
                return graphicObj;


            // take object out of abc pool
            graphicObj.transform.parent = null;


            // set position and rotation
            graphicObj.transform.position = Position;


            // set active
            graphicObj.SetActive(true);

            // if duration is given then delete object after duration 
            if (duration != -1)
                ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(graphicObj, duration));



            return graphicObj;
        }

        /// <summary>
        /// Will deactivate the ability projectile object returning it to the pool and start countdowns, reset velocity, revert surrounding objects and activate ability end graphics etc
        /// </summary>
        /// <param name="AbilityObj">Ability projectile gameobject</param>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="CollidedObject">(Optional) Object involed in the collision that caused the Projectile to be destroyed.</param>
        public void DisableProjectile(GameObject AbilityObj, ABC_IEntity Originator, GameObject CollidedObject = null) {

            //Track what time this method was called
            float functionRequestTime = Time.time;

            ABC_Projectile projectile = AbilityObj.GetComponent<ABC_Projectile>();

            //If this is not a projectile object then return here
            if (projectile == null)
                return;


            // create end effect if collided object was not provided
            // or we are showing end graphic no matter what 
            // or we just showing end graphic on environment contact and collided object has no ABC state manager 
            if (CollidedObject == null || this.abilityEndActivateOnEnvironmentOnly == false || this.abilityEndActivateOnEnvironmentOnly == true && ABC_Utilities.GetStaticABCEntity(CollidedObject).HasABCStateManager() == false)
                this.ActivateGraphicAtPosition(AbilityPositionGraphicType.End, AbilityObj.transform.position, AbilityObj.transform.rotation);


            // If we are starting recast after ability ends (is destroyed) then start the cooldown now
            if (startRecastAfterAbilityEnd == true)
                ABC_Utilities.mbSurrogate.StartCoroutine(this.StartCooldown(Originator));



            if (AbilityObj.GetComponent<Rigidbody>() != null) {
                // stop any moving
                AbilityObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                AbilityObj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                // disable gravity 
                AbilityObj.GetComponent<Rigidbody>().useGravity = false;
            }






            // get all colliders from the projectile using script (additional tracked objects may be in the list - like surrounding objects)
            Collider[] projectileColliders = projectile.meColliders;

            // disable colliders and revert any surrounding objects 
            foreach (Collider meCol in projectileColliders) {

                //If we have personally added the ability collider then disable it for now, else leave it
                if (this.addAbilityCollider == true) {

                    // turn collider off and back on to reset any collision ignores 
                    meCol.enabled = false;
                    meCol.enabled = true;

                    // if the object is still a child of the projectile then turn the collider back off
                    if (meCol.transform.parent == AbilityObj.transform)
                        meCol.enabled = false;

                }

                // if we find any surrounding objects revert it back to normal 
                if (meCol.transform.name.Contains("*_ABCSurroundingObject"))
                    this.RevertFromSurroundingObject(meCol.transform.gameObject, AbilityObj.gameObject, Originator);

            }


            // if ability gets destroyed before its toggled off we need to turn the toggle off 
            if (this.isToggled())
                this.ToggleOff(Originator, true);


            //disable and send back to pool 
            ABC_Utilities.PoolObject(AbilityObj);

            //If melee ability then remove from the list which is tracking active melee abilities 
            //Note: If additional starting positions are used then this method will only remove the ability once all additional objects have also been deactivated, unless ability is interrupted
            if (this.abilityType == AbilityType.Melee) {
                //Make sure animator is back to normal 
                Originator.ModifyAnimatorSpeed(functionRequestTime, 1);
                //Remove from tracked active melee abilities
                Originator.RemoveFromActiveMeleeAbilities(this, this.AbilityActivationInterrupted() ? 0 : this.additionalStartingPositions.Count + 1);
            }

            //If this disabling ability is part of a combo and is currently expecting a hit to continue but a hit was not made then we can reset the whole combo, as a hit will never be made now. Combo will  start from the beginning
            if (this.abilityCombo && this.comboLocked && this.comboHitRequired && this.comboLockHit == false)
                ResetAllComboLocks(Originator);

            //If the activation complete event is set to raise and has been set to ability destroyed, then after the delay call the activation complete event 
            if (this.abilityActivationCompleteRaiseEvent == true && this.abilityActivationCompleteEventType == AbilityActivationCompleteEventType.AbilityDestroyed)
                this.RaiseOriginatorsAbilityActivationCompleteEvent(Originator);

        }



        #endregion


        // ********************* Private Methods ********************

        #region Private Methods



        /// <summary>
        /// Deactivates the object provided and returns it back to the pool after the delay. 
        /// </summary>
        /// <remarks>
        /// If the object is an ability projectile it will instead call the soft destroy method. 
        /// </remarks>
        /// <param name="Obj">Gameobject to destroy</param>
        /// <param name="Delay">delay before gameobject is destroyed</param>
        /// <param name="PauseDelayOnHitStop">If true then the delay timer will pause whilst hit stop is in play</param>
        /// <param name="DetachFromParentAfterDelay">If true then the graphic will detach from it's parent after the delay</param>
        /// <param name="DetachDelay">The delay to wait till the the graphic will detach from it's parent</param>
        private IEnumerator DestroyObject(GameObject Obj, float Delay = 0f, bool PauseDelayOnHitStop = false, bool DetachFromParentAfterDelay = false, float DetachDelay = 0) {

            //If null object passed then end here
            if (Obj == null)
                yield break;



            // Wait for delay declared before we continue to destroy the object 
            if (Delay + DetachDelay > 0f) {

                //wait for duration then end animation
                for (var i = Delay + DetachDelay; i > 0;) {

                    // actual wait time 
                    if (i < 0.2f) {
                        // less then 0.2  so we only need to wait the .xx time
                        yield return new WaitForSeconds(i);
                    } else {
                        // wait a small amount and keep looping till casting time = 0; 
                        yield return new WaitForSeconds(0.2f);
                    }

                    //If Detach delay has passed then Detach from parent after
                    if (DetachFromParentAfterDelay == true && i <= Delay)
                        Obj.transform.parent = null;

                    //reduce time left unless ability is currently in hit stop then things are on hold 
                    if (PauseDelayOnHitStop == false || PauseDelayOnHitStop == true && this.hitStopCurrentlyActive == false)
                        i = i - 0.2f;
                }

            }


            // if projectile script exists then we should call the destroy on that as a default
            ABC_Projectile projScript = Obj.GetComponent<ABC_Projectile>();

            if (projScript != null) {
                // softDestroy is called here as we don't want to do any splash collisions or special functions. 
                // special functions should be activied by a game event (collision) or direct call of the proj script destroy method
                // this function simply serves to just deactivate the graphic (we still call proj script soft destroy to handle disabling any special children like surroundingobjects)
                projScript.SoftDestroy();
            } else {

                if (this.activeGraphics.Contains(Obj))
                    this.activeGraphics.Remove(Obj);


                // disable and pool the object 
                ABC_Utilities.PoolObject(Obj);
            }

        }

        /// <summary>
        /// Returns a bool indicating if the ability can activate taking into the account the ability and originator provided. Should always be called before activating an ability
        /// </summary>
        /// <param name="Originator">Entity checking if ability can be activated</param>
        ///<param name="IgnoreTriggerCheck">If true then function will not check if the ability has been triggered or not. Use if you want to know if ability can activate before it is triggered</param>
        ///<param name="IgnoreActivationPermittedChecks">If true then function will not check if the originator is allowed to activate abilities.</param>
        ///<param name="IgnoreComboCheck">If true then function will not run check combo checks which determines if the ability is next in line to activate.</param>
        /// <returns>True if ability can activate else, false</returns>
        private bool CanActivate(ABC_IEntity Originator, bool IgnoreTriggerCheck = false, bool IgnoreActivationPermittedChecks = false, bool IgnoreComboCheck = false) {

            // if the ability has been toggled off then it can't activate this time round so return false
            if (this.ToggleOff(Originator))
                return false;

            //If weapon blocking and autocast is not on then return false; 
            if (Originator.IsWeaponBlocking() && this.autoCast == false)
                return false;

            // If the ability has not been triggered by inputs or other forms of activation 
            if (IgnoreTriggerCheck == false && this.Triggered(Originator) == false && this.ScrollAbilityTriggered(Originator) == false)
                return false;


            // If ability is not ready to be used (not on cooldown and enabled) or is waiting for a collider
            if (this.isPrimed() == false)
                return false;

            // method works out if the entity can currently activate abilities depending on a number of settings and properties
            if (this.ActivationForced(Originator) == false && IgnoreActivationPermittedChecks == false && Originator.abilityActivationPermitted == false)
                return false;

            // if the ability requires a crosshair and the crosshair is not being shown
            if (this.requireCrossHairOverride && Originator.crosshairOverrideActive == false)
                return false;


            // If the entity is not in the right space (land or air) to activate the ability
            if (CorrectElevation(Originator) == false)
                return false;

            // if a hit has prevented the entity from casting and this ability is not castable during hit prevention
            if (this.RestrictedByHit(Originator))
                return false;

            // If targets have not been selected 
            if (TargetExists(Originator) == false)
                return false;


            //If ability requires a surrounding object 
            if (this.includeSurroundingObject && (this.surroundingObjectTagRequired == true && this.GetSurroundingTags(Originator).Count() == 0 || this.surroundingObjectTargetRequired && this.GetSurroundingTarget(Originator) == null)) {

                Originator.AddToDiagnosticLog("Surrounding Object(s) required and none found to activate " + this.name);

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Surrounding Object(s) required and none found to activate " + this.name);

                return false;

            }


            //does originator have enough mana
            // check if we have enough mana 
            if (Originator.manaValue < this.manaCost) {
                Originator.AddToDiagnosticLog("Not enough mana to use " + this.name);

                if (Originator.LogInformationAbout(LoggingType.NoMana))
                    Originator.AddToAbilityLog("Not enough mana to use " + this.name);

                // we have less then the cost so just cancel here by returning the courtine
                return false;

            }

            // check we have enough stat 
            if (this.statCost > 0 && Originator.GetStatValue(this.statCostName, this.statCostIntegrationType, GCStatType.Attribute) < this.statCost) {
                Originator.AddToDiagnosticLog("Not enough " + this.statCostName + " to use " + this.name);

                if (Originator.LogInformationAbout(LoggingType.NoMana))
                    Originator.AddToAbilityLog("Not enough " + this.statCostName + " to use " + this.name);

                // we have less then the cost so just cancel here by returning the courtine
                return false;

            }


            // if ability uses ammo and isn't set to reload and we have none left 
            if (this.UseAmmo == true) {

                //Indicates if ammo is avaliable to use
                bool hasAmmo = true;

                //If using ammo from equipped weapon then check if ammo avaliable, else check if ability has ammo (or ammo in clip if reload)
                if (this.useEquippedWeaponAmmo)
                    hasAmmo = Originator.EquippedWeaponHasAmmo();
                else if (this.ammoCount == 0 && this.scrollAbility == false || this.scrollAbility == true && this.ammoCount == 0 && (this.useReload == false || this.useReload == true && this.currentAmmoClipCount <= 0))
                    hasAmmo = false;

                //If no ammo then end here adding diagnostic
                if (hasAmmo == false) {

                    Originator.AddToDiagnosticLog("Not enough Ammo to use " + this.name);

                    if (Originator.LogInformationAbout(LoggingType.AmmoInformation) && this.scrollAbility == false)
                        Originator.AddToAbilityLog("You have run out of " + this.name);

                    if (Originator.LogInformationAbout(LoggingType.AmmoInformation) && this.scrollAbility == true)
                        Originator.AddToAbilityLog("No more ammo. Unable to use " + this.name);

                    return false;
                }
            }


            // If reloading required
            if (this.UseAmmo == true) {

                bool reloadRequired = false;

                //If using ammo from equipped weapon then check if reload required, else check if scrollability requires reloading
                if (this.useEquippedWeaponAmmo) {
                    reloadRequired = Originator.EquippedWeaponReloadRequired();
                } else if (this.scrollAbility == true && this.UseAmmo == true && this.useReload == true && this.reloadRequired == true) {    // else if a reload is required on a scrollability 

                    //If we are currently filling a clip then interrupt the reload to activate ability 
                    if (this.reloadFillClip == true && this.currentAmmoClipCount > 0) {
                        this.reloadInterrupted = true;
                        reloadRequired = false;

                    } else {

                        //Reload required and not currently filling clip so set tracker to true
                        reloadRequired = true;

                    }

                }

                //If reload required then log and end here
                if (reloadRequired == true) {

                    //If reload required then add diagnostic and return false as ability can't activate
                    Originator.AddToDiagnosticLog(this.name + " needs reloading");

                    if (Originator.LogInformationAbout(LoggingType.AmmoInformation) && this.scrollAbility == true)
                        Originator.AddToAbilityLog(this.name + " needs reloading.");


                    return false;
                }

            }



            // If the ability is part of a combo and ComboWatcher decides that this ability is not next to fire in the combo order then ability can't activate so return false
            if (IgnoreComboCheck == false) {
                if (IsComboBlocked(Originator))
                    return false;
            }


            //If we reached this far then ability can activate
            return true;
        }

        /// <summary>
        /// Activates the ability goes through the activation stages (initiating, preparing etc), creates the object and sets it off
        /// </summary>
        /// <remarks>
        /// Main function to activate the ability - going through preparation and initiating
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator ActivateAbility(ABC_IEntity Originator) {

            //Track what time we activated this ability
            this.abilityActivationTime = Time.time;

            //reset counter of how long ability prepared for
            this.abilitySecondsPrepared = 0;

            //Let originator know that the ability is being activated for tracking purposes
            Originator.AddToActivatingAbilities(this);

            //if enabled then raise the ability activation event to inform other scripts that have subscribed
            if (this.abilityActivationRaiseEvent == true)
                this.RaiseOriginatorsAbilityActivationEvent(Originator);

            //Reset any activation interruption flags
            this.abilityActivationInterrupted = false;

            //Reset any hitStops 
            this.hitStopCurrentlyActive = false;
            this.hitStopCurrentTotalExtendDuration = 0;

            //Activate any linked abilities set to activate when this ability does 
            this.ActivateLinkedAbilities(Originator);

            // define raycast position if travel type is crosshair, this is rerecorded again just before the ability initiates for accuracy if crossHairRecordTargetOnActivation is disabled
            if (this.travelType == TravelType.Crosshair) {
                rayCastTargetPosition = this.GetCrosshairRayCastPosition(Originator);
            }


            // get all the targets from originator for rest of code 
            // work our range/face checking/ ability before targets and if the correct target is met by calling the define function
            // this sets the ability target variable which is used by the rest of the ability firing process
            yield return ABC_Utilities.mbSurrogate.StartCoroutine(EstablishTargets(Originator));

            // we can now check for surrounding object tags. This is done after the abilitytarget has been correctly selected due to surrounding objects being retrieved from targets and starting positions
            if (this.includeSurroundingObject)
                surroundingObjects = this.GetSurroundingObjects(Originator);


            // Check to make sure ability wasn't interrupted or originator was not staggered by a hit restricting activation (could have happened whilst picking a target etc)
            if (this.AbilityActivationInterrupted() || this.RestrictedByHit(Originator)) {

                //Make sure animator is back to normal 
                Originator.ModifyAnimatorSpeed(Time.time, 1);

                //Let Originator know that the ability is no longer being activated for tracking purposes
                Originator.RemoveFromActivatingAbilities(this);

                yield break;
            }


            //unless set for the IK to persist then stop any IK (it will be turned on after initiation animation finishes)
            if (this.persistIK == false)
                ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(this.abilityActivationTime, false));

            // start preparing
            yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.StartPreparing(Originator));

            // start initiating phase
            yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.StartInitiating(Originator));

            //Initiate ability
            yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.InitiateAbility(Originator));



            //Temporarily adjust the originators activation interval if the adjustment value is greater then 0
            if (this.tempAbilityActivationIntervalAdjustment != 0f)
                Originator.TemporarilyAdjustAbilityActivationInterval(this.ModifyTimeByInitiatingAdjustments(Originator, this.tempAbilityActivationIntervalAdjustment));

            // if no target still travel is true and the activated variable is not none then this use to be a another travel type ability so we are going to swap it back
            if (this.noTargetStillTravel == true && this.noTargetStillTravelActivated != NoTargetStillTravelPreviousType.None) {
                this.travelType = (TravelType)System.Enum.Parse(typeof(TravelType), noTargetStillTravelActivated.ToString());
                this.noTargetStillTravelActivated = NoTargetStillTravelPreviousType.None;
            }


            // remove any linked projectiles as this ability has done activating 
            this.RemovePreActivatedProjectile();

            //Clear Surrounding Objects list generated above
            if (surroundingObjects != null && surroundingObjects.Count > 0)
                surroundingObjects.Clear();

            // run the function to enable abilities which have been setup to enable after this one was activated
            Originator.EnableOriginatorsAbilitiesAfterEvent(this.abilityID, AbilityEvent.Activation);

            //Let Originator know that the ability is no longer being activated for tracking purposes
            Originator.RemoveFromActivatingAbilities(this);


            //If the activation complete event is set to raise and has been set to ability initiated type then ability has finished initiating at this point so raise event (unless ability was interrupted
            if (this.AbilityActivationInterrupted() == false && this.abilityActivationCompleteRaiseEvent == true && this.abilityActivationCompleteEventType == AbilityActivationCompleteEventType.AbilityInitiated)
                this.RaiseOriginatorsAbilityActivationCompleteEvent(Originator);

            //stop weapon block for a second to prevent switching back so quickly 
            Originator.PreventWeaponBlockingForDuration(0.7f);


        }

        /// <summary>
        /// Will destroy the ability projectile provided
        /// </summary>
        /// <param name="AbilityObj">ability projectile gameobject to destroy, requires projectile script to be attached.</param>
        private void DestroyAbility(GameObject AbilityObj) {

            ABC_Projectile projectile = AbilityObj.GetComponent<ABC_Projectile>();

            if (projectile != null)
                projectile.Destroy();

        }

        /// <summary>
        /// Will raise the ability activating event for the originator 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private void RaiseOriginatorsAbilityActivationEvent(ABC_IEntity Originator) {
            Originator.RaiseAbilityActivationEvent(this.name, this.abilityID);
        }

        /// <summary>
        /// Will raise the ability activating completed event for the originator 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private void RaiseOriginatorsAbilityActivationCompleteEvent(ABC_IEntity Originator) {
            Originator.RaiseAbilityActivationCompleteEvent(this.name, this.abilityID);
        }

        /// <summary>
        /// Will raise the entities ability before target event stating if entity is in ability before target state or not
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Enabled">True if ability before target enabled, else false if disabled</param>
        private void RaiseOriginatorsAbilityBeforeTargetEvent(ABC_IEntity Originator, bool Enabled) {

            Originator.RaiseAbilityBeforeTargetEvent(this.abilityID, Enabled);

        }


        /// <summary>
        /// Will create and pool the ability projectile objects setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One gameobject which has been created</returns>
        private GameObject CreateAbilityObjects(bool CreateOne = false) {


            // if ability is not enabled or it doesn't have a particle effect 
            if ((this.abilityType != AbilityType.Projectile && this.abilityType != AbilityType.Melee) || this.abilityEnabled == false && CreateOne == false)
                return null;



            // create object particle 
            GameObject AB = null;


            //how many objects to make
            float objCount = CreateOne ? 1 : this.duration + 4;

            if (this.limitActiveAtOnce == true)
                objCount = maxActiveAtOnce + 1;



            for (int i = 0; i < objCount; i++) {


                // if one has not been entered then we can use the default ABC_Projectile or DefaultMelee objects 

                if (this.mainGraphic.GameObject == null) {
                    if ((this.abilityType == AbilityType.Projectile)) {
                        AB = (GameObject)(GameObject.Instantiate(Resources.Load("ABC-Defaults/ABC_DefaultProjectile")));
                    } else if (this.abilityType == AbilityType.Melee) {
                        AB = (GameObject)(GameObject.Instantiate(Resources.Load("ABC-Defaults/ABC_DefaultMelee")));
                    }
                } else {
                    AB = (GameObject)(GameObject.Instantiate(this.mainGraphic.GameObject));
                }


                if (AB == null) {
                    throw new System.InvalidOperationException("Main Particle has not been entered for the ABC ability: " + this.name + " and the default mainGraphic can not be found from resources. Please add a mainGraphic to prevent game errors.");
                }

                AB.name = "ABC*_" + this.name;

                // add collider before child graphic (child graphic doesn't need a collider)
                if (this.addAbilityCollider == true && this.applyColliderSettingsToParent == true) {
                    SphereCollider col = AB.AddComponent<SphereCollider>();
                    //Turn off collider by default, will be enabled later on if set too
                    col.enabled = false;
                }

                // if setting is enabled then add collider to all children
                if (this.addAbilityCollider == true && this.applyColliderSettingsToChildren == true) {

                    foreach (Transform childObj in AB.transform) {
                        childObj.gameObject.AddComponent<SphereCollider>();
                    }

                }

                // copy child object for additional Aesthetic 
                if (this.subGraphic.GameObject != null) {
                    GameObject childAB = (GameObject)(GameObject.Instantiate(this.subGraphic.GameObject));
                    childAB.transform.position = AB.transform.position;
                    childAB.transform.rotation = AB.transform.rotation;
                    childAB.transform.parent = AB.transform;
                }


                //disable and pool the object 
                ABC_Utilities.PoolObject(AB);

                // add travel scripts we require - NoTravel has no travel script

                switch (this.travelType) {
                    case TravelType.NoTravel:
                        break;
                    case TravelType.CustomScript:
                        //If custom travel script hasn't been defined then continue
                        if (this.customTravelScript.Object == null)
                            break;

                        var customScript = AB.AddComponent(System.Type.GetType(this.customTravelScript.Object.name)) as MonoBehaviour;
                        customScript.enabled = false;
                        break;
                    default:
                        AB.AddComponent<ABC_ProjectileTravel>().enabled = false;
                        break;
                }


                // add rigid body if doesn't already exist
                if (AB.GetComponent<Rigidbody>() == null)
                    AB.AddComponent<Rigidbody>();


                // ad projectile script 
                ABC_Projectile projScript = AB.AddComponent<ABC_Projectile>();
                // pass through ability 
                projScript.ability = this;
                projScript.enabled = false;


                // add to generic list. 
                this.abilityPool.Add(AB);
            }




            return AB;

        }


        /// <summary>
        /// Will create and pool the ability target indicator objects setup for the ability
        /// </summary>
        /// <returns>One gameobject which has been created</returns>
        private void CreateAbilityIndicatorObjects() {

            // if ability is not enabled or it's not setup for ability before target 
            if (this.abilityEnabled == false || this.travelType != TravelType.MouseTarget && this.travelType != TravelType.ToWorld && this.travelType != TravelType.Self && this.travelType != TravelType.SelectedTarget && this.travelType != TravelType.Mouse2D)
                return;





        }


        /// <summary>
        /// Will create and pool projectile to start graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateProjectileToStartObjects(bool CreateOne = false) {


            GameObject ptsAB = null;

            if (this.useProjectileToStartPosition == true && this.useOriginalProjectilePTS == false && this.projToStartPosGraphic.GameObject != null && this.abilityEnabled == true) {


                //how many objects to make
                float objCount = CreateOne ? 1 : this.projToStartPosDuration + 3;


                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    ptsAB = (GameObject)(GameObject.Instantiate(this.projToStartPosGraphic.GameObject));
                    ptsAB.name = this.projToStartPosGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.projToStartPosSubGraphic.GameObject != null) {
                        GameObject ptsChildAB = (GameObject)(GameObject.Instantiate(this.projToStartPosSubGraphic.GameObject));
                        ptsChildAB.name = this.projToStartPosSubGraphic.GameObject.name;
                        ptsChildAB.transform.position = ptsAB.transform.position;
                        ptsChildAB.transform.rotation = ptsAB.transform.rotation;
                        ptsChildAB.transform.parent = ptsAB.transform;
                    }



                    // set name
                    ptsAB.name = "ABC*_PTS_" + this.name;

                    //Add rigidbody but Turn off gravity by default 
                    ptsAB.AddComponent<Rigidbody>().useGravity = false;

                    // add selected target script
                    ptsAB.AddComponent<ABC_ObjectToDestination>();

                    // add hover script but turn it off
                    ptsAB.AddComponent<ABC_Hover>().enabled = false;

                    //disable and pool the object 
                    ABC_Utilities.PoolObject(ptsAB);

                    this.projectileToStartPool.Add(ptsAB);
                }

            }

            return ptsAB;
        }


        /// <summary>
        /// Will create and pool preparing graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreatePreparingObjects(bool CreateOne = false) {

            // *************** Preparing Pools ***********************

            GameObject prepAB = null;

            if (this.usePreparingAesthetics == true && this.preparingGraphic.GameObject != null && this.abilityEnabled == true) {

                //how many objects to make
                float objCount = CreateOne ? 1 : this.preparingAestheticDuration + 3;

                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    prepAB = (GameObject)(GameObject.Instantiate(this.preparingGraphic.GameObject));
                    prepAB.name = this.preparingGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.preparingSubGraphic.GameObject != null) {
                        GameObject prepChildAB = (GameObject)(GameObject.Instantiate(this.preparingSubGraphic.GameObject));
                        prepChildAB.name = this.preparingSubGraphic.GameObject.name;
                        prepChildAB.transform.position = prepAB.transform.position;
                        prepChildAB.transform.rotation = prepAB.transform.rotation;
                        prepChildAB.transform.parent = prepAB.transform;
                    }



                    //disable and pool the object 
                    ABC_Utilities.PoolObject(prepAB);

                    // add to generic list. 
                    this.preparingPool.Add(prepAB);
                }
            }

            return prepAB;
        }


        /// <summary>
        /// Will create and pool initiating graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateInitiatingObjects(bool CreateOne = false) {


            // *************** Initiating Pools ***********************

            GameObject initAB = null;

            if (this.useInitiatingAesthetics == true && this.initiatingGraphic.GameObject != null && this.abilityEnabled == true) {

                //how many objects to make
                float objCount = CreateOne ? 1 : this.initiatingAestheticDuration + 3;


                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    initAB = (GameObject)(GameObject.Instantiate(this.initiatingGraphic.GameObject));
                    initAB.name = this.initiatingGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.initiatingSubGraphic.GameObject != null) {
                        GameObject initChildAB = (GameObject)(GameObject.Instantiate(this.initiatingSubGraphic.GameObject));
                        initChildAB.name = this.initiatingSubGraphic.GameObject.name;
                        initChildAB.transform.position = initAB.transform.position;
                        initChildAB.transform.rotation = initAB.transform.rotation;
                        initChildAB.transform.parent = initAB.transform;
                    }


                    //disable and pool the object 
                    ABC_Utilities.PoolObject(initAB);

                    // add to generic list. 
                    this.initiatingPool.Add(initAB);
                }

            }

            return initAB;

        }

        /// <summary>
        /// Will create and pool additional initiating graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateInitiatingAdditionalObjects(bool CreateOne = false) {


            // *************** Initiating Additional Pools ***********************

            GameObject initAB = null;

            if (this.useInitiatingAesthetics == true && this.initiatingGraphic.GameObject != null && this.abilityEnabled == true && this.additionalStartingPositions.Count > 0) {

                //how many objects to make
                float objCount = CreateOne ? 1 : this.additionalStartingPositions.Count + 3;


                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    initAB = (GameObject)(GameObject.Instantiate(this.initiatingGraphic.GameObject));
                    initAB.name = this.initiatingGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.initiatingSubGraphic.GameObject != null) {
                        GameObject initChildAB = (GameObject)(GameObject.Instantiate(this.initiatingSubGraphic.GameObject));
                        initChildAB.name = this.initiatingSubGraphic.GameObject.name;
                        initChildAB.transform.position = initAB.transform.position;
                        initChildAB.transform.rotation = initAB.transform.rotation;
                        initChildAB.transform.parent = initAB.transform;
                    }


                    //disable and pool the object 
                    ABC_Utilities.PoolObject(initAB);

                    // add to generic list. 
                    this.initiatingAdditionalPool.Add(initAB);
                }

            }

            return initAB;

        }

        /// <summary>
        /// Will create and pool ability end graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateAbilityEndObjects(bool CreateOne = false) {

            // *************** Ability End Pools ***********************

            GameObject abEnd = null;

            //If not turned on or ability is disabled end here
            if (this.useAbilityEndAesthetics == false || this.abilityEnabled == false)
                return abEnd;


            GameObject graphicOverride = null;
            GameObject graphicSubOverride = null;

            //If set to use ability effect graphics then find one
            if (this.abilityEndUseEffectGraphic == true && this.effects.Count > 0) {

                List<Effect> currentEffects = new List<Effect>();

                //get list of all current effects
                foreach (Effect effect in this.effects) {

                    //normal effect objects
                    if (effect.globalEffect == null) {
                        currentEffects.Add(effect);
                    } else {
                        //global effect
                        currentEffects.AddRange(effect.globalEffect.ElementEffects);
                    }

                }


                //Find first effect graphic with 'adjust health' 
                if (currentEffects.Where(e => e.effectName == "AdjustHealth" && e.effectGraphic != null).Count() > 0) {
                    graphicOverride = currentEffects.Where(e => e.effectName == "AdjustHealth" && e.effectGraphic != null).FirstOrDefault().effectGraphic.GameObject;
                    graphicSubOverride = currentEffects.Where(e => e.effectName == "AdjustHealth" && e.effectGraphic != null).FirstOrDefault().effectChildGraphic.GameObject;

                } else if (currentEffects.Where(e => e.effectGraphic != null).Count() > 0) { // else find the first effect with a graphic
                    graphicOverride = currentEffects.Where(e => e.effectGraphic != null).FirstOrDefault().effectGraphic.GameObject;
                    graphicSubOverride = currentEffects.Where(e => e.effectGraphic != null).FirstOrDefault().effectChildGraphic.GameObject;
                }

            }


            //If we have an effect graphic or a normal graphic then create and pool
            if (this.abilityEndUseEffectGraphic == true && graphicOverride != null || this.abilityEndUseEffectGraphic == false && this.abilityEndGraphic.GameObject != null) {

                //how many objects to make
                float objCount = CreateOne ? 1 : this.abEndAestheticDuration + 3;


                for (int i = 0; i < objCount; i++) {


                    // create object particle using the override if populated else normal graphic
                    if (graphicOverride != null) {

                        abEnd = (GameObject)(GameObject.Instantiate(graphicOverride));
                        abEnd.name = graphicOverride.name;

                    } else {

                        abEnd = (GameObject)(GameObject.Instantiate(this.abilityEndGraphic.GameObject));
                        abEnd.name = this.abilityEndGraphic.GameObject.name;
                    }



                    // copy child object for additional Aesthetic 
                    if (graphicSubOverride != null) {
                        GameObject childAbEnd = (GameObject)(GameObject.Instantiate(graphicSubOverride));
                        childAbEnd.name = graphicSubOverride.name;
                        childAbEnd.transform.position = abEnd.transform.position;
                        childAbEnd.transform.rotation = abEnd.transform.rotation;
                        childAbEnd.transform.parent = abEnd.transform;
                    } else if (this.abilityEndSubGraphic.GameObject != null) {
                        GameObject childAbEnd = (GameObject)(GameObject.Instantiate(this.abilityEndSubGraphic.GameObject));
                        childAbEnd.name = this.abilityEndSubGraphic.GameObject.name;
                        childAbEnd.transform.position = abEnd.transform.position;
                        childAbEnd.transform.rotation = abEnd.transform.rotation;
                        childAbEnd.transform.parent = abEnd.transform;
                    }


                    //disable and pool the object 
                    ABC_Utilities.PoolObject(abEnd);

                    // add to generic list. 
                    this.abEndPool.Add(abEnd);
                }

            }

            return abEnd;


        }

        /// <summary>
        /// Will create and pool reloading graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateReloadingObjects(bool CreateOne = false) {
            // *************** Reloading Ability Aesthetic Pools ***********************

            GameObject reloadingAB = null;

            if (this.scrollAbility == true && this.useReload == true && this.useReloadAbilityAesthetics == true && this.reloadAbilityGraphic.GameObject != null && this.abilityEnabled == true) {

                //how many objects to make
                int objCount = CreateOne ? 1 : 3;

                // only 1 scroll ability activation graphic should play at once so only store 3
                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    reloadingAB = (GameObject)(GameObject.Instantiate(this.reloadAbilityGraphic.GameObject));
                    reloadingAB.name = this.reloadAbilityGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.reloadAbilitySubGraphic.GameObject != null) {
                        GameObject reloadingChildAB = (GameObject)(GameObject.Instantiate(this.reloadAbilitySubGraphic.GameObject));
                        reloadingChildAB.name = this.reloadAbilitySubGraphic.GameObject.name;
                        reloadingChildAB.transform.position = reloadingAB.transform.position;
                        reloadingChildAB.transform.rotation = reloadingAB.transform.rotation;
                        reloadingChildAB.transform.parent = reloadingAB.transform;
                    }

                    //disable and pool the object 
                    ABC_Utilities.PoolObject(reloadingAB);

                    // add to generic list.
                    this.reloadAbilityPool.Add(reloadingAB);
                }

            }

            return reloadingAB;

        }

        /// <summary>
        /// Will create and pool scroll activate graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateScrollActivateObjects(bool CreateOne = false) {

            // *************** Scrollable Activate Aesthetic Pools ***********************

            GameObject scrollAB = null;

            if (this.scrollAbility == true && this.useScrollAbilityAesthetics == true && this.scrollAbilityGraphic.GameObject != null && this.abilityEnabled == true) {

                //how many objects to make
                int objCount = CreateOne ? 1 : 1;

                // only 1 scroll ability activation graphic should play at once so only store 3
                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    scrollAB = (GameObject)(GameObject.Instantiate(this.scrollAbilityGraphic.GameObject));
                    scrollAB.name = this.scrollAbilityGraphic.GameObject.name;


                    // copy child object for additional Aesthetic 
                    if (this.scrollAbilitySubGraphic.GameObject != null) {
                        GameObject scrollActChildAB = (GameObject)(GameObject.Instantiate(this.scrollAbilitySubGraphic.GameObject));
                        scrollActChildAB.name = this.scrollAbilitySubGraphic.GameObject.name;
                        scrollActChildAB.transform.position = scrollAB.transform.position;
                        scrollActChildAB.transform.rotation = scrollAB.transform.rotation;
                        scrollActChildAB.transform.parent = scrollAB.transform;
                    }


                    //disable and pool the object 
                    ABC_Utilities.PoolObject(scrollAB);


                    // add to list. 
                    this.scrollActivatePool.Add(scrollAB);

                }

            }

            return scrollAB;

        }


        /// <summary>
        /// Will create and pool spawned object graphics setup for the ability
        /// </summary>
        /// <param name="CreateOne">If true then only one extra graphic will be created and returned</param>
        /// <returns>One graphic gameobject which has been created</returns>
        private GameObject CreateSpawnedObjects(bool CreateOne = false) {

            GameObject spawnObj = null;

            // *************** spawned objects Pools ***********************

            if (this.spawnObject == true && this.spawningObject.GameObject != null && this.abilityEnabled == true) {

                //how many objects to make
                float objCount = CreateOne ? 1 : 7;

                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    spawnObj = (GameObject)(GameObject.Instantiate(this.spawningObject.GameObject));
                    spawnObj.name = this.spawningObject.GameObject.name;

                    //disable and pool the object 
                    ABC_Utilities.PoolObject(spawnObj);


                    // add to generic list. 
                    this.spawnPool.Add(spawnObj);
                }

            }

            return spawnObj;

        }

        /// <summary>
        /// Will create and pool effect graphics setup for the ability. 
        /// </summary>
        private void CreateEffectObjects() {
            // *************** Effect Pools ***********************

            if (this.abilityEnabled == true) {


                foreach (Effect effect in this.effects) {

                    //create normal effect objects
                    if (effect.globalEffect == null) {
                        effect.CreateEffectObjects();
                    } else {
                        //create effect objects for global effect
                        effect.globalEffect.ElementEffects.ForEach(globEffect => globEffect.CreateEffectObjects());
                    }

                }


            }

        }


        /// <summary>
        /// Will activate graphic setup for the ability. Graphic which is activated depends on the type passed.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="GraphicType">Type of graphic to activate: Preparing, Initiating, Reloading, AbilityActivation</param>
        /// <param name="IgnoreDelay">If true then any delays setup will be ignored and graphic will activate instantly</param>
        /// <param name="AdditionalStartPositionIteration">Used for 'InitiatingAdditionalStartingPosition' graphic type, determines which additional starting position to use</param>
        /// <returns>Will return the graphic gameobject which has been created</returns>
        private GameObject ActivateGraphic(ABC_IEntity Originator, AbilityGraphicType GraphicType, bool IgnoreDelay = false, int AdditionalStartPositionIteration = 0) {


            GameObject graphicObj = null;
            bool trackGraphic = true;


            switch (GraphicType) {
                case AbilityGraphicType.Preparing:

                    graphicObj = this.preparingPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreatePreparingObjects(true);

                    break;
                case AbilityGraphicType.Initiating:
                    graphicObj = this.initiatingPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateInitiatingObjects(true);

                    break;
                case AbilityGraphicType.InitiatingAdditionalStartingPosition:


                    graphicObj = this.initiatingAdditionalPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateInitiatingAdditionalObjects(true);

                    break;
                case AbilityGraphicType.Reloading:


                    graphicObj = this.reloadAbilityPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                    if (graphicObj == null)
                        graphicObj = CreateReloadingObjects(true);

                    break;
                case AbilityGraphicType.ScrollAbilityActivation:

                    //We will track this graphic differently as we don't want it being destroyed when this ability is no longer the current scroll ability
                    if (this.scrollAbilityAestheticDurationType == DurationType.Persistant)
                        trackGraphic = false;

                    //Get graphic
                    if (this.scrollAbilityAestheticDurationType == DurationType.Persistant && this.scrollAbilityAesthetic != null) {
                        graphicObj = this.scrollAbilityAesthetic;
                    } else {
                        graphicObj = this.scrollActivatePool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();
                    }


                    //If graphic still null create new one
                    if (graphicObj == null)
                        graphicObj = CreateScrollActivateObjects(true);


                    break;
                case AbilityGraphicType.ScrollAbilityDeactivation:

                    if (this.scrollAbilityAestheticDurationType != DurationType.Persistant)
                        return null;

                    //We will track this graphic differently as we don't want it being destroyed when this ability is no longer the current scroll ability
                    trackGraphic = false;

                    //Get graphic
                    if (this.scrollAbilityAesthetic != null) {
                        graphicObj = this.scrollAbilityAesthetic;
                    } else {
                        graphicObj = this.scrollActivatePool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();
                    }

                    //If graphic still null create new one
                    if (graphicObj == null)
                        graphicObj = CreateScrollActivateObjects(true);


                    break;

                default:

                    break;

            }


            //add to active graphic list
            if (trackGraphic)
                this.activeGraphics.Add(graphicObj);

            // Enable graphic setting position, parent, delay and duration
            ABC_Utilities.mbSurrogate.StartCoroutine(EnableGraphic(graphicObj, Originator, GraphicType, IgnoreDelay, AdditionalStartPositionIteration));


            return graphicObj;


        }


        /// <summary>
        /// Will enable the graphic object provided setting up positon, parent, delay and durations depending on the graphic type passed
        /// </summary>
        /// <param name="GraphicObj">Object to enable</param>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="GraphicType">Type of graphic to activate: Preparing, Initiating, Reloading, AbilityActivation</param>
        /// <param name="IgnoreDelay">If true then any delays setup will be ignored and graphic will activate instantly</param>
        /// <param name="AdditionalStartPositionIteration">Used for 'InitiatingAdditionalStartingPosition' graphic type, determines which additional starting position to use</param>
        private IEnumerator EnableGraphic(GameObject GraphicObj, ABC_IEntity Originator, AbilityGraphicType GraphicType, bool IgnoreDelay = false, int AdditionalStartPositionIteration = 0) {

            //If graphic is still null then finish here
            if (GraphicObj == null)
                yield break;

            StartingPosition startingPosition = StartingPosition.Self;
            GameObject positionOnObject = null;
            string positionOnTag = null;
            Vector3 positionOffset = new Vector3(0, 0, 0);
            float positionForwardOffset = 0f;
            float positionRightOffset = 0f;
            float duration = 2f;
            float delay = 0f;
            bool pauseDelayOnHitStop = false;

            bool deatchFromParentAfterDelay = false;
            float detachDelay = 0f;



            switch (GraphicType) {
                case AbilityGraphicType.Preparing:

                    startingPosition = this.preparingStartPosition;
                    positionOnObject = this.preparingPositionOnObject.GameObject;
                    positionOnTag = this.preparingPositionOnTag;

                    positionOffset = this.preparingAestheticsPositionOffset;
                    positionForwardOffset = this.preparingAestheticsPositionForwardOffset;
                    positionRightOffset = this.preparingAestheticsPositionRightOffset;

                    //If we are using the prepare time for the graphic duration then retrieve the ability prepare time
                    if (this.preparingAestheticDurationUsePrepareTime == true) {
                        duration = this.GetAbilityPrepareTime() + 0.3f;
                    } else {
                        duration = this.preparingAestheticDuration; // else use the defined time
                    }


                    break;
                case AbilityGraphicType.Initiating:

                    startingPosition = this.initiatingStartPosition;
                    positionOnObject = this.initiatingPositionOnObject.GameObject;
                    positionOnTag = this.initiatingPositionOnTag;

                    positionOffset = this.initiatingAestheticsPositionOffset;
                    positionForwardOffset = this.initiatingAestheticsPositionForwardOffset;
                    positionRightOffset = this.initiatingAestheticsPositionRightOffset;

                    duration = this.ModifyTimeByInitiatingAdjustments(Originator, this.initiatingAestheticDuration);
                    delay = this.ModifyTimeByInitiatingAdjustments(Originator, this.initiatingAestheticDelay);
                    pauseDelayOnHitStop = true;

                    if (this.initiatingAestheticDetachFromParentAfterDelay == true) {
                        deatchFromParentAfterDelay = this.initiatingAestheticDetachFromParentAfterDelay;
                        detachDelay = this.ModifyTimeByInitiatingAdjustments(Originator, this.initiatingAestheticDetachDelay);
                    }

                    //If ability is melee and the initiating graphic is activating with the ability then overwrite duration and detach delay to match when the ability will appear in game
                    if (this.abilityType == AbilityType.Melee && this.initiatingAestheticActivateWithAbility == true) {

                        delay = 0; // this is called straight away when ability is made in game so no delay
                        duration = this.ModifyTimeByInitiatingAdjustments(Originator, this.duration) + 0.05f;

                        deatchFromParentAfterDelay = true;
                        detachDelay = this.ModifyTimeByInitiatingAdjustments(Originator, this.duration);
                    }


                    //If ability toggling and duration is 0 then make it infinite
                    if (this.abilityToggle != AbilityToggle.Off && duration == 0)
                        duration = -1;

                    break;
                case AbilityGraphicType.InitiatingAdditionalStartingPosition:

                    if (this.additionalStartingPositions.Count > 0) {

                        startingPosition = this.additionalStartingPositions[AdditionalStartPositionIteration].startingPosition;
                        positionOnObject = this.additionalStartingPositions[AdditionalStartPositionIteration].startingPositionOnObject.GameObject;
                        positionOnTag = this.additionalStartingPositions[AdditionalStartPositionIteration].startingPositionOnTag;

                        positionOffset = this.initiatingAestheticsPositionOffset;
                        positionForwardOffset = this.initiatingAestheticsPositionForwardOffset;
                        positionRightOffset = this.initiatingAestheticsPositionRightOffset;

                        duration = this.ModifyTimeByInitiatingAdjustments(Originator, this.initiatingAestheticDuration);
                        delay = this.ModifyTimeByInitiatingAdjustments(Originator, this.additionalStartingPositions[AdditionalStartPositionIteration].repeatInitiatingGraphicDelay);
                        pauseDelayOnHitStop = true;

                        if (this.initiatingAestheticDetachFromParentAfterDelay == true) {
                            deatchFromParentAfterDelay = this.initiatingAestheticDetachFromParentAfterDelay;
                            detachDelay = this.ModifyTimeByInitiatingAdjustments(Originator, this.initiatingAestheticDetachDelay);
                        }

                        //If ability is melee and the initiating graphic is activating with the ability then overwrite duration and detach delay to match when the ability will appear in game
                        if (this.abilityType == AbilityType.Melee && this.initiatingAestheticActivateWithAbility == true) {

                            delay = 0; // this is called straight away when ability is made in game so no delay
                            duration = this.ModifyTimeByInitiatingAdjustments(Originator, this.duration) + 0.05f;

                            deatchFromParentAfterDelay = true;
                            detachDelay = this.ModifyTimeByInitiatingAdjustments(Originator, this.duration);
                        }


                    }

                    break;

                case AbilityGraphicType.Reloading:

                    startingPosition = this.reloadAbilityStartPosition;
                    positionOnObject = this.reloadAbilityPositionOnObject.GameObject;
                    positionOnTag = this.reloadAbilityPositionOnTag;

                    positionOffset = this.reloadAbilityAestheticsPositionOffset;
                    positionForwardOffset = this.reloadAbilityAestheticsPositionForwardOffset;
                    positionRightOffset = this.reloadAbilityAestheticsPositionRightOffset;


                    duration = this.reloadAbilityAestheticDuration;


                    break;
                case AbilityGraphicType.ScrollAbilityActivation:


                    startingPosition = this.scrollAbilityStartPosition;
                    positionOnObject = this.scrollAbilityPositionOnObject.GameObject;
                    positionOnTag = this.scrollAbilityPositionOnTag;

                    positionOffset = this.scrollAbilityAestheticsPositionOffset;
                    positionForwardOffset = this.scrollAbilityAestheticsPositionForwardOffset;
                    positionRightOffset = this.scrollAbilityAestheticsPositionRightOffset;



                    //Set duration
                    if (this.scrollAbilityAestheticDurationType == DurationType.Duration && this.scrollAbilityAestheticDuration > 0f) {
                        duration = this.scrollAbilityAestheticDuration;
                    } else {
                        duration = -1;
                    }


                    //delay on graphic 
                    delay = this.scrollAbilityGraphicActivateDelay;


                    break;
                case AbilityGraphicType.ScrollAbilityDeactivation:

                    startingPosition = this.scrollAbilityPersistantAestheticInactivePosition;
                    positionOnObject = this.scrollAbilityPersistantAestheticInactivePositionOnObject.GameObject;
                    positionOnTag = this.scrollAbilityPersistantAestheticInactivePositionOnTag;

                    positionOffset = this.scrollAbilityAestheticsPositionOffset;
                    positionForwardOffset = this.scrollAbilityAestheticsPositionForwardOffset;
                    positionRightOffset = this.scrollAbilityAestheticsPositionRightOffset;


                    //Persistant graphic so will have infinite duration
                    duration = -1;

                    //delay on graphic 
                    delay = this.scrollAbilityGraphicDeactivateDelay;


                    break;

                default:

                    break;

            }



            //If we are ignoring delay then set the delay to -1
            if (IgnoreDelay)
                delay = -1;

            // Wait for any delay before we work out positions and place the graphic
            if (delay > 0f)
                yield return new WaitForSeconds(delay);


            //initial starting point is originator incase anything goes wrong
            Vector3 position = Originator.transform.position;
            GameObject parentObject = Originator.gameObject;


            switch (startingPosition) {
                case StartingPosition.Self:
                    position = Originator.transform.position + positionOffset + Originator.transform.forward * positionForwardOffset + Originator.transform.right * positionRightOffset;
                    parentObject = Originator.gameObject;
                    break;
                case StartingPosition.OnObject:
                    var onObjectTransform = positionOnObject.transform;
                    position = onObjectTransform.position + positionOffset + onObjectTransform.forward * positionForwardOffset + onObjectTransform.right * positionRightOffset;
                    parentObject = onObjectTransform.gameObject;
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
                    if (target != null) {
                        var targetTransform = target.transform;
                        position = targetTransform.position + positionOffset + targetTransform.forward * positionForwardOffset + targetTransform.right * positionRightOffset;
                        parentObject = target;
                    }
                    break;
                case StartingPosition.OnWorld:
                    if (this.worldTarget != null) {
                        var worldTargetTransform = this.worldTarget.transform;
                        position = this.worldTargetPosition + positionOffset + worldTargetTransform.forward * positionForwardOffset + worldTargetTransform.right * positionRightOffset;
                        parentObject = this.worldTarget;
                    }
                    break;
                case StartingPosition.CameraCenter:
                    var cameraTransform = Originator.Camera.transform;
                    position = Originator.Camera.transform.position + positionOffset + cameraTransform.forward * positionForwardOffset + cameraTransform.right * positionRightOffset;
                    parentObject = Originator.Camera.gameObject;
                    break;
                default:
                    Originator.AddToDiagnosticLog(this.name + " Error: ability starting position was not correctly determined.");
                    break;
            }



            // set position 
            if (position != null)
                GraphicObj.transform.position = position;

            // set parent
            if (parentObject != null) {
                GraphicObj.transform.SetParent(parentObject.transform);
                GraphicObj.transform.rotation = parentObject.transform.rotation;
            }


            //Set active 
            GraphicObj.SetActive(true);

            // destroy it after duration if not set to -1 (which means infinite) 
            if (duration != -1)
                ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(GraphicObj, duration, pauseDelayOnHitStop, deatchFromParentAfterDelay, detachDelay));



        }


        /// <summary>
        /// Will activate/deactivate the ability range indicator which places the indicator graphic at the entity showing the range at which the ability can be activated
        /// </summary>
        /// <param name="Enabled">True if the indicator should show, else false</param>
        /// <param name="Originator">Entity that activated the ability</param>
        private void ToggleAbilityRangeIndicator(bool Enabled, ABC_IEntity Originator) {

            //If no indicator has been setup then continue
            if (Originator.abilityRangeIndicator == null || useRange == false || this.abilityRangeIndicatorImage.Texture == null || this.selectedTargetRangeGreaterThan > 0)
                return;

            switch (Enabled) {

                case true:


                    // turn on  indicator and position it 
                    Originator.abilityRangeIndicator.GetComponentInChildren<RawImage>(true).texture = this.abilityRangeIndicatorImage.Texture;
                    Originator.abilityRangeIndicator.transform.SetParent(Originator.transform);
                    Originator.abilityRangeIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);
                    Originator.abilityRangeIndicator.SetActive(true);



                    //Scale range
                    Originator.abilityRangeIndicator.transform.localScale = new Vector3(this.selectedTargetRangeLessThan * 2, this.selectedTargetRangeLessThan * 2, this.selectedTargetRangeLessThan * 2);


                    break;

                case false:


                    //disable and send back to pool 
                    ABC_Utilities.PoolObject(Originator.abilityRangeIndicator);


                    break;
            }

        }

        /// <summary>
        /// Will activate/deactivate the ability world target indicator which places the indicator graphic at the point provided (Mouse or Crosshair)
        /// </summary>
        /// <param name="Enabled">True if the indicator should show, else false</param>
        /// <param name="Originator">Entity that activated the ability</param>
        private void ToggleAbilityWorldTargetIndicator(bool Enabled, ABC_IEntity Originator) {

            //If no indicator has been setup then continue
            if (Originator.abilityWorldTargetIndicator == null || this.abilityWorldTargetIndicatorImage.Texture == null)
                return;

            switch (Enabled) {

                case true:


                    Vector3 indicatorPoint = Vector3.zero;

                    Ray ray = new Ray();

                    // change hit position depending on target select type 
                    if (Originator.targetSelectType == TargetSelectType.Mouse || Originator.targetSelectType == TargetSelectType.None) {
                        ray = Originator.Camera.ScreenPointToRay(Originator.inputManager.GetMousePosition());
                    } else if (Originator.targetSelectType == TargetSelectType.Crosshair) {
                        ray = Originator.Camera.ViewportPointToRay(Originator.crossHairPosition);
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit)) {
                        indicatorPoint = hit.point;
                    } else {
                        this.ToggleAbilityWorldTargetIndicator(false, Originator);
                        return;
                    }



                    // Make sure texture is set on indicator
                    RawImage indicatorImage = Originator.abilityWorldTargetIndicator.GetComponentInChildren<RawImage>(true);
                    indicatorImage.texture = this.abilityWorldTargetIndicatorImage.Texture;

                    //Calculate distance
                    float distance = Vector3.Distance(Originator.transform.position, indicatorPoint);

                    // are we in range?
                    if (this.useRange == true && (distance < this.selectedTargetRangeGreaterThan || distance > this.selectedTargetRangeLessThan)) {
                        // Turn down alpha as not in range
                        indicatorImage.CrossFadeAlpha(0.3f, 0, true);
                    } else {
                        //else show image in full
                        indicatorImage.CrossFadeAlpha(1f, 0, true);
                    }

                    //Position world target indicator
                    Originator.abilityWorldTargetIndicator.transform.position = indicatorPoint + new Vector3(0, 0.1f, 0);
                    Originator.abilityWorldTargetIndicator.transform.SetParent(null);
                    Originator.abilityWorldTargetIndicator.SetActive(true);

                    //Scale with ability?
                    if (this.abilityBeforeTargetWorldIndicatorScaleToEffectRadius == true)
                        Originator.abilityWorldTargetIndicator.transform.localScale = this.effectRangeLocalScale;
                    else
                        Originator.abilityWorldTargetIndicator.transform.localScale = new Vector3(this.abilityBeforeTargetWorldIndicatorScale, this.abilityBeforeTargetWorldIndicatorScale, this.abilityBeforeTargetWorldIndicatorScale);


                    break;

                case false:


                    //disable and send back to pool 
                    ABC_Utilities.PoolObject(Originator.abilityWorldTargetIndicator);


                    break;
            }

        }


        /// <summary>
        /// Will activate/deactivate the ability mouse target indicator which places the indicator graphic at the entity and points in the direction of the mouse
        /// </summary>
        /// <param name="Enabled">True if the indicator should show, else false</param>
        /// <param name="Originator">Entity that activated the ability</param>
        private void ToggleAbilityMouseTargetIndicator(bool Enabled, ABC_IEntity Originator) {

            //If no indicator has been setup then continue
            if (Originator.abilityMouseTargetIndicator == null || this.abilityMouseTargetIndicatorImage.Texture == null)
                return;

            switch (Enabled) {

                case true:

                    RaycastHit hit;
                    Ray ray = Originator.Camera.ScreenPointToRay(Originator.inputManager.GetMousePosition());

                    //Record mousepoint
                    Vector3 mousePoint = Vector3.zero;

                    if (Physics.Raycast(ray, out hit)) {

                        if (hit.transform == Originator.transform)
                            mousePoint = ray.GetPoint(30f);
                        else
                            mousePoint = hit.point;
                    }


                    // turn on  indicator position it at originator and make it face mouse 
                    Originator.abilityMouseTargetIndicator.GetComponentInChildren<RawImage>(true).texture = this.abilityMouseTargetIndicatorImage.Texture;
                    Originator.abilityMouseTargetIndicator.transform.SetParent(Originator.transform);
                    Originator.abilityMouseTargetIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);
                    Originator.abilityMouseTargetIndicator.SetActive(true);

                    //Scale for mouse target indicator
                    Originator.abilityMouseTargetIndicator.transform.localScale = new Vector3(this.abilityBeforeTargetMouseTargetIndicatorLength / 3, this.abilityBeforeTargetMouseTargetIndicatorLength / 3, this.abilityBeforeTargetMouseTargetIndicatorLength);

                    //Set Rotation to follow mouse position 
                    Quaternion newRotation = Quaternion.LookRotation(mousePoint - Originator.transform.position);
                    newRotation.x = 0f;
                    newRotation.z = 0f;
                    Originator.abilityMouseTargetIndicator.transform.rotation = Quaternion.Lerp(newRotation, Originator.abilityMouseTargetIndicator.transform.rotation, 0f);


                    break;

                case false:


                    //disable and send back to pool 
                    ABC_Utilities.PoolObject(Originator.abilityMouseTargetIndicator);


                    break;
            }

        }




        /// <summary>
        /// Starts an animation clip using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to play - Initiating, Preparing etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        private void StartAnimationRunner(AbilityAnimationState State, ABC_AnimationsRunner AnimationRunner) {


            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipSpeed = 1f;
            float animationClipDelay = 0f;
            AvatarMask animationClipMask = null;
            bool skipBlending = false;


            switch (State) {
                case AbilityAnimationState.Initiate:

                    animationClip = this.initiatingAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.initiatingAnimationRunnerClipSpeed;
                    animationClipDelay = this.initiatingAnimationRunnerClipDelay;
                    animationClipMask = this.initiatingAnimationRunnerMask.AvatarMask;

                    //If activation forced and ability isn't set to prepare then skip blending
                    if (this.forceActivation && this.prepareTime == 0f)
                        skipBlending = true;

                    //For rapid firing if on key down is true and temp adjustment is a negative and animation is already running and no where near ending then stop here 
                    if (this.onKeyDown == true && this.tempAbilityActivationIntervalAdjustment < 0 && AnimationRunner.IsAnimationRunning(animationClip) && AnimationRunner.GetCurrentAnimationProgress() < 90)
                        return;

                    break;
                case AbilityAnimationState.Preparation:

                    animationClip = this.preparingAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.preparingAnimationRunnerClipSpeed;
                    animationClipDelay = this.preparingAnimationRunnerClipDelay;
                    animationClipMask = this.preparingAnimationRunnerMask.AvatarMask;

                    //If activation forced then skip blending
                    if (this.forceActivation)
                        skipBlending = true;

                    break;
                case AbilityAnimationState.ScrollActivate:

                    animationClip = this.scrollAbilityAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.scrollAbilityAnimationRunnerClipSpeed;
                    animationClipDelay = this.scrollAbilityAnimationRunnerClipDelay;
                    animationClipMask = this.scrollAbilityAnimationRunnerMask.AvatarMask;

                    break;
                case AbilityAnimationState.ScrollDeactivate:

                    animationClip = this.scrollAbilityDeactivateAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.scrollAbilityDeactivateAnimationRunnerClipSpeed;
                    animationClipDelay = this.scrollAbilityDeactivateAnimationRunnerClipDelay;
                    animationClipMask = this.scrollAbilityDeactivateAnimationRunnerMask.AvatarMask;

                    break;
                case AbilityAnimationState.Reload:

                    animationClip = this.reloadAbilityAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.reloadAbilityAnimationRunnerClipSpeed;
                    animationClipDelay = this.reloadAbilityAnimationRunnerClipDelay;
                    animationClipMask = this.reloadAbilityAnimationRunnerMask.AvatarMask;

                    break;
            }


            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null)
                return;


            AnimationRunner.StartAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipMask, true, true, skipBlending);


        }

        /// <summary>
        /// End an animation clip currently playing using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to stop - Initiating, Preparing etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        /// <param name="Delay">(Optional) Delay before animation ends</param>
        private void EndAnimationRunner(AbilityAnimationState State, ABC_AnimationsRunner AnimationRunner, float Delay = 0f) {

            // set variables to be used later 
            AnimationClip animationClip = null;


            switch (State) {
                case AbilityAnimationState.Initiate:

                    animationClip = this.initiatingAnimationRunnerClip.AnimationClip;

                    break;
                case AbilityAnimationState.Preparation:

                    animationClip = this.preparingAnimationRunnerClip.AnimationClip;

                    break;
                case AbilityAnimationState.ScrollActivate:

                    animationClip = this.scrollAbilityAnimationRunnerClip.AnimationClip;

                    break;
                case AbilityAnimationState.ScrollDeactivate:

                    animationClip = this.scrollAbilityDeactivateAnimationRunnerClip.AnimationClip;


                    break;
                case AbilityAnimationState.Reload:

                    animationClip = this.reloadAbilityAnimationRunnerClip.AnimationClip;

                    break;
            }

            // if animator parameter is null or animation runner is not given then end here. 
            if (animationClip == null || AnimationRunner == null)
                return;

            AnimationRunner.EndAnimation(animationClip, Delay);
        }




        /// <summary>
        /// Starts an animation for the ability depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to play - Initiating, Preparing etc</param>
        /// <param name="Animator">Animator component</param>
        private void StartAnimation(AbilityAnimationState State, Animator Animator) {


            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOnValue = "";



            switch (State) {
                case AbilityAnimationState.Initiate:

                    animatorParameterType = this.initiatingAnimatorParameterType;
                    animatorParameter = this.initiatingAnimatorParameter;
                    animatorOnValue = this.initiatingAnimatorOnValue;

                    break;
                case AbilityAnimationState.Preparation:

                    animatorParameterType = this.preparingAnimatorParameterType;
                    animatorParameter = this.preparingAnimatorParameter;
                    animatorOnValue = this.preparingAnimatorOnValue;

                    break;
                case AbilityAnimationState.ScrollActivate:

                    animatorParameterType = this.scrollAbilityAnimatorParameterType;
                    animatorParameter = this.scrollAbilityAnimatorParameter;
                    animatorOnValue = this.scrollAbilityAnimatorOnValue;

                    break;
                case AbilityAnimationState.ScrollDeactivate:

                    animatorParameterType = this.scrollAbilityDeactivateAnimatorParameterType;
                    animatorParameter = this.scrollAbilityDeactivateAnimatorParameter;
                    animatorOnValue = this.scrollAbilityDeactivateAnimatorOnValue;

                    break;
                case AbilityAnimationState.Reload:
                    animatorParameterType = this.reloadAbilityAnimatorParameterType;
                    animatorParameter = this.reloadAbilityAnimatorParameter;
                    animatorOnValue = this.reloadAbilityAnimatorOnValue;

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
        /// Ends the animation for the ability depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to stop - Initiating, Preparing etc</param>
        /// <param name="Animator">Animator component</param>
        private void EndAnimation(AbilityAnimationState State, Animator Animator) {

            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOffValue = "";



            switch (State) {
                case AbilityAnimationState.Initiate:

                    animatorParameterType = this.initiatingAnimatorParameterType;
                    animatorParameter = this.initiatingAnimatorParameter;
                    animatorOffValue = this.initiatingAnimatorOffValue;

                    break;
                case AbilityAnimationState.Preparation:

                    animatorParameterType = this.preparingAnimatorParameterType;
                    animatorParameter = this.preparingAnimatorParameter;
                    animatorOffValue = this.preparingAnimatorOffValue;

                    break;
                case AbilityAnimationState.ScrollActivate:

                    animatorParameterType = this.scrollAbilityAnimatorParameterType;
                    animatorParameter = this.scrollAbilityAnimatorParameter;
                    animatorOffValue = this.scrollAbilityAnimatorOffValue;

                    break;
                case AbilityAnimationState.ScrollDeactivate:

                    animatorParameterType = this.scrollAbilityDeactivateAnimatorParameterType;
                    animatorParameter = this.scrollAbilityDeactivateAnimatorParameter;
                    animatorOffValue = this.scrollAbilityDeactivateAnimatorOffValue;

                    break;
                case AbilityAnimationState.Reload:
                    animatorParameterType = this.reloadAbilityAnimatorParameterType;
                    animatorParameter = this.reloadAbilityAnimatorParameter;
                    animatorOffValue = this.reloadAbilityAnimatorOffValue;

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
        /// Handler for button triggers - returns an bool indicating if the button relating to the state provided has been triggered. 
        /// </summary>
        /// <remarks>
        /// If the state in the parameter is Ability trigger then this function will return true if the ability trigger button has been triggered
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="State">Which state and button type to check for: AbilityTrigger, ColliderDelayTrigger, WaitBeforeInitiating, ScrollQuickKey</param>
        /// <param name="PressType">Determines if the function will check for a Press or Hold</param>
        /// <returns>True if the button relating to the state has been correctly press or held, else false</returns>
        private bool ButtonPressed(ABC_IEntity Originator, AbilityButtonPressState State, AbillityButtonPressType PressType = AbillityButtonPressType.Press) {

            InputType inputType = InputType.Button;
            AbillityButtonPressType pressType = PressType;
            KeyCode key = KeyCode.None;
            string button = "";

            // determine the right configuration depending on the type provided
            switch (State) {
                case AbilityButtonPressState.AbilityTrigger:

                    inputType = this.keyInputType;
                    key = this.key;
                    button = this.keyButton;

                    break;
                case AbilityButtonPressState.AdditionalAbilityTrigger:

                    inputType = this.additionalKeyInputType;
                    key = this.additionalKey;
                    button = this.additionalKeyButton;

                    break;
                case AbilityButtonPressState.ColliderDelayTrigger:

                    inputType = this.colliderDelayInputType;
                    key = this.colliderDelayKey;
                    button = this.colliderDelayButton;

                    break;

                case AbilityButtonPressState.WaitBeforeInitiating:

                    inputType = this.waitBeforeInitiatingInputType;
                    key = this.waitBeforeInitiatingKey;
                    button = this.waitBeforeInitiatingButton;

                    break;
                case AbilityButtonPressState.ScrollQuickKey:

                    inputType = this.scrollQuickInputType;
                    key = this.scrollQuickKey;
                    button = this.scrollQuickButton;

                    break;
                case AbilityButtonPressState.CollisionEnabledAfterTrigger:

                    inputType = this.enableCollisionAfterKeyInputType;
                    key = this.enableCollisionAfterKey;
                    button = this.enableCollisionAfterKeyButton;

                    break;
            }



            // check if correct button is being pressed/held 
            if (pressType == AbillityButtonPressType.Press) {

                // If input type is key and the configured key is being pressed return true
                if (inputType == InputType.Key && Originator.inputManager.GetKeyDown(key))
                    return true;


                // if input type is button and the configured button is being pressed return true
                if (inputType == InputType.Button && Originator.inputManager.GetButtonDown(button))
                    return true;

            } else if (pressType == AbillityButtonPressType.Hold) {

                // If input type is key and the configured key is being held down return true
                if (inputType == InputType.Key && Originator.inputManager.GetKey(key))
                    return true;


                // if input type is button and the configured button is being held down return true
                if (inputType == InputType.Button && Originator.inputManager.GetButton(button))
                    return true;

            }




            // correct button is not currently being pressed so return false
            return false;


        }

        /// <summary>
        /// Returns a boolean indicating if the abilities activation button has been pressed or autocast has been set to true. The function determines if the ability has been triggered to activate.
        /// </summary>
        /// <returns>True if input detected. False if no input detected</returns>
        private bool Triggered(ABC_IEntity Originator) {

            // if auto cast is on then return true
            if (this.autoCast) {
                // turn autocast back off
                this.autoCast = false;
                return true;
            }

            // if its a scroll ability then we return false as input detection is not handled by the ability 
            if (this.scrollAbility == true)
                return false;

            // If the originator can't currently activate abilities from triggers then return false
            if (Originator.CanActivateAbilitiesFromTriggers() == false)
                return false;

            //If movement validation is needed then check if entity should be moving or not for ability to activate

            if (this.enableMovementValidation == true) {

                if (this.movementValidation == AbilityMovementValidation.TriggerOnlyIfMoving && Originator.isMoving == false)
                    return false;

                if (this.movementValidation == AbilityMovementValidation.TriggerOnlyIfNotMoving && Originator.isMoving == true)
                    return false;

            }                
             

            switch (this.triggerType) {
                case TriggerType.Input:

                    // If we require an additional key input and the key is not being pressed or held down then we can end here 
                    if (this.requireAdditionalKeyInput == true && (this.additionalOnKeyPress && this.ButtonPressed(Originator, AbilityButtonPressState.AdditionalAbilityTrigger, AbillityButtonPressType.Press) == false
                           || this.additionalOnKeyDown && this.ButtonPressed(Originator, AbilityButtonPressState.AdditionalAbilityTrigger, AbillityButtonPressType.Hold) == false))
                        return false;

                    // if the right key or button has been pressed by the player then return true
                    if (this.onKeyPress && this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Press)
                           || this.onKeyDown && this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Hold)) {

                        return true;
                    } else {
                        // no input detected so return false
                        return false;
                    }

                case TriggerType.InputCombo:
                    //if Input Combo and the set of key combinations have recently been pressed by the originator

                    //No inputs setup so return false
                    if (this.keyInputCombo.Count == 0)
                        return false;

                    //If the input combo matches the input history then activate 
                    if (this.keyInputCombo.SequenceEqual(Originator.GetRecordedKeyInputHistory().ToList()))
                        return true;
                    else
                        return false;
                default:
                    return false;
            }



        }

        /// <summary>
        /// Will return a boolean determining if the ability has been activated due to being a current scroll ability for the originator provided. 
        /// </summary>
        /// <param name="Originator">Entity that this ability is a currently an active scroll ability for</param>
        /// <returns>True if input detected, else false if no input detected</returns>
        private bool ScrollAbilityTriggered(ABC_IEntity Originator) {

            // if auto cast is on then return true
            if (this.autoCast) {
                // turn autocast back off
                this.autoCast = false;
                return true;
            }


            // if ability passed through is not a scroll ability or not the current selected scroll ability then we can return false 
            if (this.scrollAbility == false || IsCurrentScrollAbilityFor(Originator) == false)
                return false;


            // check if the correct activate current ability input has been pressed
            if (this.onKeyPress && Originator.CurrentScrollAbilityActivationButtonPressed(true)
                || this.onKeyDown && Originator.CurrentScrollAbilityActivationButtonPressed(false)) {
                return true;
            } else {
                // correct input was not dectected so we can return false 
                return false;
            }

        }

        /// <summary>
        /// Will determine if the ability activation is forced ignoring normal blocks and restrictions
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if activation is forced else false</returns>
        private bool ActivationForced(ABC_IEntity Originator) {

            //If activation is not forced 
            if (this.forceActivation == false)
                return false;

            //if the ability is already being activated then end here to stop force activation spam 
            if (Originator.IsActivatingAbility(this) == true)
                return false;

            //If combo blocked then don't force activation
            if (IsComboBlocked(Originator, true))
                return false;

            //If setup then Interrupt any other abilities currently being activated
            if (this.forceActivationInterruptCurrentActivation)
            Originator.InterruptAbilityActivation();

            //Activation is forced so return true
            return true;


        }

        // private as only initiating an ability can turn it to toggle mode
        /// <summary>
        /// Will toggle on the ability. During toggle state the ability will stay active until toggled off. Depending on settings this may stop further abilities from being activated. 
        /// </summary>
        /// <remarks>
        /// Method will keep track of the projectile obj so it can be destroyed when the ability is toggled off
        /// </remarks>
        /// <param name="Originator">Entity that toggled the ability on</param>
        /// <param name="AbilityObj">Ability projectile gameobject which is tracked so it can be destroyed when the ability is toggled off</param>
        private void ToggleOn(ABC_IEntity Originator, GameObject AbilityObj) {
            // if were toggling the ability then turn toggled on
            this.toggled = true;

            // set the variable keeping track of what the toggle object is (will only ever be 1 in game)
            this.toggledAbilityObj.Add(AbilityObj);


            if (this.canCastWhenToggled == false) {
                Originator.ToggledAbilityRestrictsAbilityActivation(true);
            }

            if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                Originator.AddToAbilityLog(this.name + " toggled on");

        }

        /// <summary>
        /// Determines if the ability is currently active in toggle mode and if the ability has been triggered to switch to the opposite toggle state. 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns><c>true</c>, if toggled ability has been triggered, <c>false</c> otherwise.</returns>
        private bool ToggleOffTriggered(ABC_IEntity Originator) {


            if (this.requireCrossHairOverride == true && Originator.crosshairOverrideActive == false) {
                return false;
            }


            if (this.autoCast) {
                // turn autocast back off
                this.autoCast = false;
                return true;
            }


            if (Originator.CancelTriggered())
                return true;


            if (IsCurrentScrollAbilityFor(Originator) && (this.abilityToggle == AbilityToggle.Hold && Originator.CurrentScrollAbilityActivationButtonPressed(false) == false
                || this.abilityToggle == AbilityToggle.OnOff && Originator.CurrentScrollAbilityActivationButtonPressed())) {
                return true;
            }


            if (this.scrollAbility == false && (this.abilityToggle == AbilityToggle.Hold && this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Hold) == false)
                || this.abilityToggle == AbilityToggle.OnOff && this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Press)) {
                return true;
            }


            return false;


        }

        /// <summary>
        /// Attempts to toggle off the ability, returning a bool indicating if it was toggled off successfully
        /// </summary>
        /// <param name="Originator">Entity that toggled the ability on</param>
        /// <param name="ForceToggleOff">If true then ability will be toggled off no matter what</param>
        /// <returns>True if ability was successfully toggled off, else false</returns>
        private bool ToggleOff(ABC_IEntity Originator, bool ForceToggleOff = false) {

            // if this ability is current toggled, a trigger has been initiated to toggle the ability off OR its a scroll ability and no longer current scroll OR forced override has been passed
            if (this.isToggled() && (this.ToggleOffTriggered(Originator) || this.scrollAbility == true && IsCurrentScrollAbilityFor(Originator) == false && this.canCastWhenToggled == false) || ForceToggleOff == true) {


                // we don't want to fire again just turn off the toggled ability so we can end here 
                this.toggled = false;


                // we need to now destroy the initiating obj if its not already been destroyed

                foreach (GameObject Obj in this.initiatingObjs) {

                    if (Obj != null && Obj.activeInHierarchy == true)
                        ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(Obj)); 

                }

                // no longer need to track the innitiating object 
                this.initiatingObjs.Clear();

                // we need to now destroy the projectile if its not already been destroyed

                foreach (GameObject Obj in this.toggledAbilityObj) {

                    if (Obj != null && Obj.activeInHierarchy == true)
                        this.DestroyAbility(Obj);

                }

                // no longer need to track the toggle object 
                this.toggledAbilityObj.Clear();


                if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                    Originator.AddToAbilityLog(this.name + " toggled off");


                // if we are repeating animation then stop this
                if (this.canCastWhenToggled == false && this.repeatInitiatingAnimationWhilstToggled == true) {

                    //end animation runner
                    if (this.initiatingAnimationRunnerOnEntity)
                        this.EndAnimationRunner(AbilityAnimationState.Initiate, Originator.animationRunner);

                    if (this.initiatingAnimationRunnerOnScrollGraphic)
                        this.EndAnimationRunner(AbilityAnimationState.Initiate, GetCurrentScrollAbilityAnimationRunner(Originator));

                    if (this.initiatingAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(AbilityAnimationState.Initiate, ar));

                    //end animator
                    if (this.initiatingAnimateOnEntity)
                        this.EndAnimation(AbilityAnimationState.Initiate, Originator.animator);

                    //If enabled then disable the animation on the graphic object
                    if (this.initiatingAnimateOnScrollGraphic)
                        this.EndAnimation(AbilityAnimationState.Initiate, GetCurrentScrollAbilityAnimator(Originator));

                    if (this.initiatingAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(AbilityAnimationState.Initiate, a));


                    // cancel the stop movement
                    if (this.stopMovementOnInitiate)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleOriginatorsMovement(Originator, true, this.stopMovementOnInitiateFreezePosition, stopMovementOnInitiateDisableComponents));


                    // If set then raise then enable originator movement event
                    if (this.stopMovementOnInitiateRaiseEvent)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, true));

                }



                // we no longer need to prevent casting due to a toggled ability 
                Originator.ToggledAbilityRestrictsAbilityActivation(false);


                return true;
            }




            return false;




        }

        /// <summary>
        /// Returns a bool indicating if the ability is the current active scroll ability for the entity provided. 
        /// </summary>
        /// <param name="Originator">Entity which will be be used to determine if this ability is current an active scroll ability</param>
        /// <returns>True if this ability is currently an active scroll ability for the Originator, else false</returns>
        private bool IsCurrentScrollAbilityFor(ABC_IEntity Originator) {

            if (this.scrollAbility == false)
                return false;

            if (Originator.currentScrollAbility == this)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Returns a bool indicating if the ability is currently active in toggle mode. 
        /// </summary>
        /// <returns><c>true</c>, if ability is toggled on, <c>false</c> otherwise.</returns>
        private bool isToggled() {
            if (this.toggled == true && this.toggledAbilityObj.Count() > 0 && this.toggledAbilityObj.Count(o => o.activeInHierarchy == true) > 0) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns a bool indicating if the ability is currently restricted from activating due to the originator receiving a recent hit
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if ability is restricted, else false</returns>
        private bool RestrictedByHit(ABC_IEntity Originator) {


            if (this.castableDuringHitPrevention == false && Originator.abilityActivationHitRestricted)
                return true;

            // ability is not restricted by Hit
            return false;

        }

        /// <summary>
        /// Returns a bool indicating if the correct target has been selected for the Ability to activate correctly.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns><c>true</c>, if target was correct, <c>false</c> otherwise.</returns>
        private bool TargetExists(ABC_IEntity Originator) {


            // if no target still travel is true and the activated variable is not none then this use to be a another travel type ability so we are going to swap it back before we continue with target validation 
            if (this.noTargetStillTravel == true && this.noTargetStillTravelActivated != NoTargetStillTravelPreviousType.None) {
                this.travelType = (TravelType)System.Enum.Parse(typeof(TravelType), noTargetStillTravelActivated.ToString());
                this.noTargetStillTravelActivated = NoTargetStillTravelPreviousType.None;
            }


            // if we don't have a target and the traveltype is selected target but 'no target still travel' setting is true then we want to swap to traveltype forward
            if (Originator.target == null && (this.auxiliarySoftTarget == false || this.auxiliarySoftTarget == true && Originator.softTarget == null)
                    && this.travelType == TravelType.SelectedTarget && this.noTargetStillTravel == true && this.abilityBeforeTarget == false) {

                // turn the flag to the current travel type to let rest of code know its been activated ready to be changed back next time
                this.noTargetStillTravelActivated = (NoTargetStillTravelPreviousType)System.Enum.Parse(typeof(NoTargetStillTravelPreviousType), this.travelType.ToString());
                this.travelType = TravelType.Forward;


                Originator.AddToDiagnosticLog("Casting " + this.name + " with forward projection as no target has been selected");
            }


            //If ability is a ray cast but not set to: self, forward, selected target, nearest tag crosshair, mouse forward, mouse 2d or mouse target then we will default to forward
            if (this.abilityType == AbilityType.RayCast && (this.travelType != TravelType.Self && this.travelType != TravelType.SelectedTarget && this.travelType != TravelType.NearestTag && this.travelType != TravelType.Crosshair && this.travelType != TravelType.Forward && this.travelType != TravelType.MouseForward && this.travelType != TravelType.MouseTarget && this.travelType != TravelType.Mouse2D))
                this.travelType = TravelType.Forward;


            // if starting position is on an object and it no longer exists then correct target doesn't exist so we return false
            if (this.startingPosition == StartingPosition.OnObject && this.startingPositionOnObject.GameObject == null) {

                Originator.AddToDiagnosticLog("Can't spawn " + this.name + " OnObject as it doesn't exist");

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Can't spawn OnObject as it doesn't exist");

                return false;
            }


            //If travel type is nearest tag and there is nothing in range with the tag (this is re-called when activating so this might pass true now but entities may move out of range when activating a check is done there also)
            //If no target still travel is true then swap to traveltype forward 
            if (this.travelType == TravelType.NearestTag && this.noTargetStillTravel == false) {

                //Determine if we are ignoring the originator or not when searching for tags
                GameObject IgnoreEntity = null;

                if (this.travelNearestTagIgnoreOriginator)
                    IgnoreEntity = Originator.gameObject;

                //Find nearest object with tag (we don't care about randomising it)
                if (this.GetObjectWithNearestTag(this.GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), this.travelNearestTagRange, ABC_Utilities.ConvertTags(Originator, this.travelNearestTagList), IgnoreEntity) == null) {

                    //Condition not met so return false here
                    Originator.AddToDiagnosticLog("No tagged targets are in range to activate " + this.name);


                    if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                        Originator.AddToAbilityLog("No tagged targets are in range to activate " + this.name);

                    return false;


                }

            }


            // if starting position is on a tag/self tag and the list is empty or the object's with the tags don't exists then return false
            if ((this.startingPosition == StartingPosition.OnTag || this.startingPosition == StartingPosition.OnSelfTag) && this.startingPositionOnTag == string.Empty || this.startingPosition == StartingPosition.OnTag && GameObject.FindGameObjectWithTag(this.startingPositionOnTag) == null || this.startingPosition == StartingPosition.OnSelfTag && ABC_Utilities.TraverseObjectForTag(Originator, this.startingPositionOnTag) == null) {

                Originator.AddToDiagnosticLog("Can't spawn " + this.name + " OnTag as it doesn't exist");

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Can't spawn OnTag as it doesn't exist");

                return false;
            }


            // If player has not selected a world location and the starting position is on the world
            if (this.startingPosition == StartingPosition.OnWorld && Originator.worldTargetObj == null && (this.abilityBeforeTarget == false && (this.travelType != TravelType.SelectedTarget || this.travelType != TravelType.Self || this.travelType != TravelType.MouseTarget || this.travelType != TravelType.Mouse2D))) {
                Originator.AddToDiagnosticLog("Need to pick a world location to activate " + this.name);


                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Need to pick a world location to start this ability");

                return false;
            }


            // If player has not selected a world position and travel type is to world. Takes into account if player is picking a target after ability has started activating
            if (this.travelType == TravelType.ToWorld && Originator.worldTargetObj == null && this.abilityBeforeTarget == false) {
                Originator.AddToDiagnosticLog("Need to pick a world location to activate " + this.name);


                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Need to pick a world location to start this ability");

                return false;

            }


            // If a target is required for the starting position and has not been selected. Takes into account if player is picking a target after ability has started activating and if the ability is not of selected target travel type
            if (this.startingPosition == StartingPosition.Target && Originator.target == null && this.abilityBeforeTarget == false && !(this.travelType == TravelType.SelectedTarget && this.auxiliarySoftTarget == true && Originator.softTarget != null) && !(Originator.softTarget != null && this.startingPositionAuxiliarySoftTarget == true && this.startingPosition == StartingPosition.Target)) {
                Originator.AddToDiagnosticLog("Unable to active " + this.name + " a target needs selecting first");


                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Need to select a target first");

                return false;
            }



            // If a target has not been selected and travel type is selected target 
            if (this.travelType == TravelType.SelectedTarget && Originator.target == null && this.abilityBeforeTarget == false && (this.auxiliarySoftTarget == false || (this.auxiliarySoftTarget == true && Originator.softTarget == null))) {
                Originator.AddToDiagnosticLog("Unable to active " + this.name + " a target needs selecting first");

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Need to select a target first");

                return false;
            }


            // If a target has not been selected and ability type is melee
            if (this.abilityType == AbilityType.Melee && this.noTargetStillTravel == false && Originator.target == null && (this.auxiliarySoftTarget == false || (this.auxiliarySoftTarget == true && Originator.softTarget == null))) {
                Originator.AddToDiagnosticLog("Unable to active " + this.name + " a target needs selecting first");

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Need to select a target first");

                return false;
            }


            //Check range and if originator is facing target for those ability types that use it (and we not doing ability before target)
            if (this.abilityBeforeTarget == false && (this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.ToWorld || this.travelType == TravelType.Self || this.abilityType == AbilityType.Melee)) {
                //Set ability targets then check range and if facing target 
                if (this.SetAbilityTargets(Originator) == false || this.OriginatorInRangeOfTarget(Originator) == false || this.OriginatorFacingTarget(Originator) == false)
                    return false;
            }



            // Correct Target conditions as no checks have failed above so we can return true
            return true;

        }

        /// <summary>
        /// Returns a bool indicating if the Entity that activated the ability is in the correct elevation (In air, on ground or either).
        /// </summary>
        /// <returns><c>true</c>, if entity activating ability is in the correct elevation, <c>false</c> otherwise.</returns>
        /// <param name="Originator">Entity that activated the ability</param>
        private bool CorrectElevation(ABC_IEntity Originator) {

            switch (this.LandOrAir) {
                case AbilityLandOrAir.LandOrAir:
                    // it does not matter if entity is on land or air we can still activate 
                    return true;
                case AbilityLandOrAir.Air:
                    // if entity distance from the below object is greater then the distance setting then we are in air. 
                    if (ABC_Utilities.EntityDistanceFromGround(Originator.transform) >= this.airAbilityDistanceFromGround) {
                        return true;
                    } else {
                        return false;
                    }
                case AbilityLandOrAir.Land:
                    // if entity is touching the ground (less then 1 distance from ground) 
                    if (ABC_Utilities.EntityDistanceFromGround(Originator.transform) <= 1.3f) {
                        return true;
                    } else {
                        return false;
                    }
                default:
                    // if the land or air isn't set then something has gone wrong so we will just fire anyway 
                    return true;
            }

        }

        /// <summary>
        /// Resets the combo locks for all the originators abilities
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="BypassResetRestriction">If true then combo locks will always be reset and all reset restrictions will be ignored</param>
        private void ResetAllComboLocks(ABC_IEntity Originator, bool BypassResetRestriction = false) {

            //If ability has been set to never reset combo locks then end here
            if (this.neverResetOtherCombos == true && BypassResetRestriction == false)
                return;

            // if an ability is part of a combo then reset 
            foreach (ABC_Ability item in Originator.CurrentAbilities.Where(item => item.abilityCombo == true)) {
                item.comboLocked = false;
                item.comboLockedTime = 0f;
                item.comboLockHit = false;
            }
        }




        /// <summary>
        /// Will rotate originator entity to look at the abilities current target
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private void RotateOriginatorToTarget(ABC_IEntity Originator) {


            //If rotate to selected target is true or this is mouse forward (always needs to rotate to the mouse target for mouse forward to work correctly) 
            if (this.rotateToSelectedTarget || this.travelType == TravelType.MouseForward) {
                switch (this.travelType) {
                    case TravelType.SelectedTarget:
                    case TravelType.NearestTag:
                        if (target != null) {
                            if (Originator.isInTheAir)
                                Originator.LookAt(target.transform.position);
                            else
                                Originator.TurnTo(target.transform.position);
                        }
                        break;
                    case TravelType.ToWorld:
                        if (Originator.isInTheAir)
                            Originator.LookAt(this.worldTargetPosition);
                        else
                            Originator.TurnTo(this.worldTargetPosition);
                        break;
                    case TravelType.Crosshair:
                        Originator.TurnTo(rayCastTargetPosition);
                        break;
                    case TravelType.MouseForward:
                    case TravelType.MouseTarget:
                    case TravelType.Mouse2D:
                        Originator.LookAt(GetMousePosition(Originator));
                        break;
                }

                // if the ability type is melee then we also need to turn to the target if option is enabled 
                if (this.abilityType == AbilityType.Melee && target != null) {

                    //If this ability is not going to be missing then just turn to target
                    if (this.GetMissChancePositionOffset(Originator) == Vector3.zero)
                        Originator.TurnTo(target);
                    else // else turn opposite direction to target as we are missing the attack
                        Originator.TurnTo(Originator.transform.position - target.transform.position);

                }


            }


            //if ability type is melee and always activate is on or selected target and rotate to selected target is false, or true but no target exists or travel type is forward then rotate depending on selected behaviour (to mouse, same direction, to camera etc)
            if ((this.abilityType == AbilityType.Melee && this.noTargetStillTravel == true || this.travelType == TravelType.SelectedTarget) && (this.rotateToSelectedTarget == false || this.rotateToSelectedTarget == true && target == null) || this.travelType == TravelType.Forward) {

                switch (this.noTargetRotateBehaviour) {
                    case AbilityNoTargetRotateBehaviour.CameraCenter:
                        // else if we are melee or forward and want to rotate to camera
                        Vector3 cameraDirection = Originator.Camera.transform.forward;
                        cameraDirection.y = 0;

                        if (cameraDirection.sqrMagnitude != 0.0f) {
                            cameraDirection.Normalize();
                            Originator.TurnTo(Originator.transform.position + cameraDirection + this.GetMissChancePositionOffset(Originator));
                        }
                        break;
                    case AbilityNoTargetRotateBehaviour.MousePosition:

                        Originator.TurnTo(GetMousePosition(Originator) + this.GetMissChancePositionOffset(Originator));
                        break;
                    case AbilityNoTargetRotateBehaviour.CurrentDirection:
                        //don't turn 
                        break;
                }

            }


        }


        /// <summary>
        /// Enable or disable the originator entitys movement 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Enable">If true then originator entity will be able to move, else if false then the originator entity will not be able to move</param>
        /// <param name="FreezePosition">If true then originator entity movement will be enabled/disabled by freezing the transform position</param>
        /// <param name="DisableComponents">If true then originator entity movement will be enabled/disabled by changing the active state of movement components (Character Controller, Rigidbody, NavAgent etc)
        /// <param name="Duration"> How long before the toggled movement is switched back, i.e if enabled then will be disabled after the duration. If 0 then the movement won't be switched back after a duration.
        private IEnumerator ToggleOriginatorsMovement(ABC_IEntity Originator, bool Enable = true, bool FreezePosition = true, bool DisableComponents = false, float Duration = 0f) {

            //Track what time this method was called
            float functionRequestTime = Time.time;

            Originator.ToggleMovement(functionRequestTime, Enable, FreezePosition, DisableComponents);

            //If a duration has been provided then toggle back the movement after the duration
            if (Duration > 0) {
                yield return new WaitForSeconds(Duration);

                //If we originally disabled movement, enable it again
                if (Enable == false)
                    Originator.ToggleMovement(functionRequestTime, true, FreezePosition, DisableComponents);
                else
                    Originator.ToggleMovement(functionRequestTime, false, FreezePosition, DisableComponents);
            }

        }


        /// <summary>
        /// Will raise the enable or disable movement event for the originator 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Enable">If true then originator enable movement event will be called, else if false then the originator disable momvement event will be called
        /// <param name="Duration"> How long before the toggled movement is switched back, i.e if enabled then will be disabled after the duration. If 0 then the movement won't be switched back after a duration.
        private IEnumerator RaiseOriginatorsToggleMovementEvent(ABC_IEntity Originator, bool Enable = true, float Duration = 0f) {

            //Track what time this method was called
            //Stops overlapping i.e if another part of the system toggled movement 
            //this function would not continue after duration
            float functionRequestTime = Time.time;

            if (Enable == true)
                Originator.ToggleMovementRaiseEvent(functionRequestTime, true);
            else
                Originator.ToggleMovementRaiseEvent(functionRequestTime, false);

            //If a duration has been provided then toggle back the movement after the duration
            if (Duration > 0) {
                yield return new WaitForSeconds(Duration);

                //If we originally disabled movement, enable it again
                if (Enable == false)
                    Originator.ToggleMovementRaiseEvent(functionRequestTime, true);
                else
                    Originator.ToggleMovementRaiseEvent(functionRequestTime, false);
            }


        }



        /// <summary>
        /// Will raise the enable or disable gravity event for the originator 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Enable">If true then originator enable gravity event will be called, else if false then the originator disable gravity event will be called
        private void RaiseOriginatorsDefyGravityEvent(ABC_IEntity Originator, bool Enable = true) {

            if (Enable == true)
                Originator.RaiseEnableGravityEvent();
            else
                Originator.RaiseDisableGravityEvent();

        }



        /// <summary>
        /// Will return the first object found within a radius that has a tag or ABC tag matching the list provided
        /// </summary>
        /// <param name="Position">Position to look from</param>
        /// <param name="Range">Range to look from position</param>
        /// <param name="Tags">Tags to match</param>
        /// <param name="IgnoreObject">If provided, this object will be ignored when matching</param>
        /// <param name="RandomiseSearch">If true the list of potential objects will be shuffled before the tag matching begins</param>
        /// <returns></returns>
        private GameObject GetObjectWithNearestTag(Vector3 Position, float Range, List<string> Tags, GameObject IgnoreObject = null, bool RandomiseSearch = false) {

            //If no tags declared then return null as no objects will ever be found
            if (Tags.Count == 0)
                return null;

            //Setup return value
            GameObject retval = null;

            // get all objects in a big range
            List<ABC_IEntity> potentialObjects = ABC_Utilities.GetAllABCEntitiesInRange(Position, Range).ToList();

            //If set to randomise search then shuffle the potential objects
            if (RandomiseSearch == true && potentialObjects.Count > 1) {
                for (int t = 0; t < potentialObjects.Count; t++) {
                    var tmp = potentialObjects[t];
                    int r = Random.Range(t, potentialObjects.Count);
                    potentialObjects[t] = potentialObjects[r];
                    potentialObjects[r] = tmp;
                }
            }

            //Cycle through tags as we look in list priority order
            foreach (string tag in Tags) {

                //Check potential objects returning the first one found with the correct tag or ABC tag
                foreach (ABC_IEntity Object in potentialObjects) {

                    //If object is the object set to be ignored or is a surrounding object then continue
                    if (Object.gameObject == IgnoreObject || Object.gameObject.name.Contains("*_ABCSurroundingObject"))
                        continue;

                    // check through list to see if any object has the right tag or target 
                    if (Object.HasABCStateManager() && ABC_Utilities.ObjectHasTag(Object.gameObject, tag))
                        return Object.gameObject;
                }

            }

            //If we got this far then no nearest tags can be found
            return retval;


        }

        /// <summary>
        /// Will return a list of object found within a radius that has a tag or ABC tag matching the list provided
        /// </summary>
        /// <param name="Position">Position to look from</param>
        /// <param name="Range">Range to look from position</param>
        /// <param name="Tags">Tags to match</param>
        /// <param name="IgnoreObject">If provided, this object will be ignored when matching</param>
        /// <param name="RandomiseSearch">If true the list of potential objects will be shuffled before the tag matching begins</param>
        /// <returns></returns>
        private List<GameObject> GetAllObjectsWithNearestTag(Vector3 Position, float Range, List<string> Tags, GameObject IgnoreObject = null, bool RandomiseSearch = false) {

            //If no tags declared then return null as no objects will ever be found
            if (Tags.Count == 0)
                return null;

            //Setup return value
            List<GameObject> retval = new List<GameObject>();

            // get all objects in a big range
            List<ABC_IEntity> potentialObjects = ABC_Utilities.GetAllABCEntitiesInRange(Position, Range).ToList();

            //If set to randomise search then shuffle the potential objects
            if (RandomiseSearch == true && potentialObjects.Count > 1) {
                for (int t = 0; t < potentialObjects.Count; t++) {
                    var tmp = potentialObjects[t];
                    int r = Random.Range(t, potentialObjects.Count);
                    potentialObjects[t] = potentialObjects[r];
                    potentialObjects[r] = tmp;
                }
            }

            //Cycle through tags as we look in list priority order
            foreach (string tag in Tags) {

                //Check potential objects returning the first one found with the correct tag or ABC tag
                foreach (ABC_IEntity Object in potentialObjects) {

                    //If object is the object set to be ignored or is a surrounding object then continue
                    if (Object.gameObject == IgnoreObject || Object.gameObject.name.Contains("*_ABCSurroundingObject"))
                        continue;

                    // check through list to see if any object has the right tag or target 
                    if (Object.HasABCStateManager() && ABC_Utilities.ObjectHasTag(Object.gameObject, tag))
                        retval.Add(Object.gameObject);
                }

            }

            //return the result
            return retval;


        }

        /// <summary>
        /// Will randomise the current gameobjects recorded in the tag target tracker
        /// </summary>
        private void RandomiseTagTargets() {

            //If set to randomise search then shuffle the potential objects
            if (this.tagTargets.Count > 1) {
                for (int t = 0; t < this.tagTargets.Count; t++) {
                    var tmp = this.tagTargets[t];
                    int r = Random.Range(t, this.tagTargets.Count);
                    this.tagTargets[t] = this.tagTargets[r];
                    this.tagTargets[r] = tmp;
                }
            }


        }

        /// <summary>
        /// Returns a bool indicating if originator entity is currently in range of the abilities target 
        /// </summary>
        /// <remarks>
        /// Will always return true if ability has been setup to not consider range or travel type is not SelectedTarget, ToWorld or Self
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if entity is in range of target, else false</returns>
        private bool OriginatorInRangeOfTarget(ABC_IEntity Originator) {


            //If using range 
            // and starting position is onworld or target 
            // or travel type is selected target, to world, self 
            // or ability is melee and 
            if (this.useRange == true && (this.startingPosition == StartingPosition.OnWorld || this.startingPosition == StartingPosition.Target || this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.ToWorld || this.travelType == TravelType.Self || this.abilityType == AbilityType.Melee && this.target != null && this.noTargetStillTravel == false)) {

                // used to set position depending on if were travelling to selected target/self or on world
                Vector3 rangeTargetPos = Vector3.zero;
                Vector3 rangeSpawnPos = Vector3.zero;

                // if toworld or self we need different position for distance then the current selected target 
                switch (this.travelType) {
                    case TravelType.Self:
                        rangeTargetPos = Originator.transform.position;
                        break;
                    case TravelType.ToWorld:
                        rangeTargetPos = this.worldTargetPosition;
                        break;
                    case TravelType.MouseTarget:
                        //Do nothing it's only here because of starting position conditions 
                        break;
                    default:
                        if (this.target == false)
                            return false;

                        rangeTargetPos = this.target.transform.position;
                        break;
                }

                // unless starting position is on target then we need to change that (for drain affects etc) 
                if (this.startingPosition == StartingPosition.Target) {

                    rangeTargetPos = this.target.transform.position;

                } else if (this.startingPosition == StartingPosition.OnWorld) {
                    rangeTargetPos = this.worldTargetPosition;
                }

                // range spawn position shall always be on caster as that is the range - from caster to target 
                rangeSpawnPos = Originator.transform.position;

                //Calculate distance
                float distance = Vector3.Distance(rangeSpawnPos, rangeTargetPos);

                if (distance >= this.selectedTargetRangeGreaterThan && distance <= this.selectedTargetRangeLessThan) {
                    // if our spawn position and target position distance is within the range then return true else false
                    return true;
                } else {
                    Originator.AddToDiagnosticLog("Not in range to activate " + this.name + ". Current Range: " + distance);

                    if (Originator.LogInformationAbout(LoggingType.Range))
                        Originator.AddToAbilityLog("Not in range to activate " + this.name);

                    return false;
                }


            } else {
                //Don't need to check range so return true
                return true;
            }

        }

        /// <summary>
        /// Returns a bool indicating if the originator entity is facing the abilities target
        /// </summary>
        /// <remarks>
        /// Will always return true if ability has been setup to not consider facing targets or travel type is not SelectedTarget or Self
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if entity is facing target, else false</returns>
        private bool OriginatorFacingTarget(ABC_IEntity Originator) {


            // if target facing is true and were selecting target or nearest tag or melee (with no target still travel off) or selecting self (In self case make sure were not spawning on player too - as you will never be able to look at yourself)
            if (this.targetFacing == true && (this.startingPosition == StartingPosition.Target || this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.NearestTag || this.abilityType == AbilityType.Melee && this.target != null) && this.target.gameObject != Originator.transform.gameObject
                || this.targetFacing == true && this.travelType == TravelType.Self && this.startingPosition != StartingPosition.OnWorld && this.startingPosition != StartingPosition.Self && (this.startingPosition == StartingPosition.OnObject && this.startingPositionOnObject.GameObject.transform.IsChildOf(Originator.transform)) == false) {

                var dir = (this.target.transform.position - Originator.transform.position).normalized;
                var dot = Vector3.Dot(dir, Originator.transform.forward);

                if (dot >= 0.3) {
                    //  were facing the target! Note: higher the value the more accuracy needed to face
                    return true;
                } else {

                    Originator.AddToDiagnosticLog("Not facing target so unable to activate " + this.name);

                    if (Originator.LogInformationAbout(LoggingType.FacingTarget))
                        Originator.AddToAbilityLog("Not facing target");

                    return false;
                }
            } else {
                // were not using facing target mode so just return true, we don't care if were facing
                return true;

            }
        }




        /// <summary>
        /// Removes the linked ability from the originator entity
        /// </summary>
        /// <param name="DeleteObj">If true then linked ability object is destroyed and returned back to the pool</param>
        /// <param name="ObjectToRemove">If gameobject is provided then only that object is removed/deleted, else whole list is cleared</param>
        private void RemovePreActivatedProjectile(bool DeleteObj = true, GameObject ObjectToRemove = null) {

            //If no object exists then end here
            if (this.preActivatedProjectiles.Count == 0)
                return;

            //If parameter is set to delete then destroy graphic if: 
            // Object is an original projectile or 
            // Object is not an original projectile and the ability has been interrupted or the ability duration is 0 (if 0 it deletes on initiation, else it deletes itself after the duration)
            if (DeleteObj && (this.useOriginalProjectilePTS == true || (this.useOriginalProjectilePTS == false && (this.AbilityActivationInterrupted() == true || this.projToStartPosDuration == 0)))) {

                //Destroy whole list as no specific object was passed to remove
                if (ObjectToRemove == null) {

                    //If the non original graphic was going to disable itself after duration but has had to be removed early then stop the coroutine
                    if (this.useOriginalProjectilePTS == false && this.preActivatedProjectileDisableCoroutines.Count > 0)
                        foreach (IEnumerator coroutine in this.preActivatedProjectileDisableCoroutines) {
                            ABC_Utilities.mbSurrogate.StopCoroutine(coroutine);
                        }

                    //Go through all the pre-activated projectiles and destroy them all 
                    foreach (GameObject obj in this.preActivatedProjectiles)
                        ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(obj));

                } else {

                    //If the non original graphic was going to disable itself after duration but has had to be removed early then stop the coroutine
                    if (this.useOriginalProjectilePTS == false && this.preActivatedProjectileDisableCoroutines.Count > 0)
                        ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(ObjectToRemove));

                    // Destroy just the object passed in the function
                    ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(ObjectToRemove));
                }

            }

            if (ObjectToRemove == null)
                //Clear whole list as no specific object was passed to remove
                this.preActivatedProjectiles.Clear();
            else
                this.preActivatedProjectiles.Remove(ObjectToRemove);



        }



        /// <summary>
        /// Returns a bool indicating if the object provided can be converted to target surrounding object
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Target">Target object which we are checking if it can be converted to a surrounding object</param>
        /// <returns>True if object can be converted to a surrounding object, else false</returns>
        private bool CanBeUsedAsSurroundingTarget(ABC_IEntity Originator, GameObject Target) {

            // If ability does not make the use of surrounding objects then return false 
            if (this.includeSurroundingObject == false)
                return false;

            // if the target doesn't exist or is not active then return false
            if (Target == null || Target.activeInHierarchy == false)
                return false;

            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(Target);

            // if statemanager script is null or we are blocking surrounding object status or we are not in range then we have not got the right target so return false 
            if (iEntity.HasABCStateManager() == false || iEntity.blockSurroundingObjectStatus == true || Vector3.Distance(Originator.transform.position, Target.transform.position) > this.surroundingObjectTargetRange)
                return false;

            // we now need to check if we can only use certain targets as a surrounding object 
            if (this.surroundingObjectTargetRestrict == true) {

                if (ABC_Utilities.ObjectHasTag(Target, ABC_Utilities.ConvertTags(Originator, surroundingObjectTargetAffectTag))) {
                    return true;
                } else {
                    // no match has been found so return false 
                    return false;
                }


            }


            // we have the right target as we made it down this far passing all restrictions so return true
            return true;
        }

        /// <summary>
        /// Will return the originators target/soft target (depending on settings) if it can be used as a target surrounding object
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Gameobject which can be converted to a target surrounding object</returns>
        private GameObject GetSurroundingTarget(ABC_IEntity Originator) {


            GameObject retVal;

            // First we run checks to see if we can use the target as a surrounding object. 
            if (this.surroundingObjectTarget == true && Originator.target != null && this.CanBeUsedAsSurroundingTarget(Originator, Originator.target)) {
                //If we can then it is set to the global variable used later in the controller
                retVal = Originator.target;

            } else if (this.surroundingObjectTarget == true && this.surroundingObjectAuxiliarySoftTarget == true && Originator.softTarget != null && this.CanBeUsedAsSurroundingTarget(Originator, Originator.softTarget)) {
                // if the current target was no good then if correctly setup we will now check the soft target and set this if it meets all the criteria
                retVal = Originator.softTarget;
            } else {
                // reset global variable as the current target/soft target can not be used as a surrounding object 
                retVal = null;
            }

            return retVal;

        }


        /// <summary>
        /// Returns a list of Gameobjects selected to be a surrounding object dependant on the starting position, tags of the object and properties of the ability 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>list of Gameobjects selected to be a surrounding object</returns>
        private List<GameObject> GetSurroundingTags(ABC_IEntity Originator) {
            List<GameObject> retVal = new List<GameObject>();

            // If ability does not make the use of surrounding objects then return the list 
            if (this.includeSurroundingObject == false)
                return retVal;



            // if we are not including surroundingobject tags then stop the method here and return the list 
            if (this.surroundingObjectTags == false)
                return retVal;



            // for each item around us - lets check if they are an ABC ability if they are lets handle the collison
            foreach (ABC_IEntity entity in ABC_Utilities.GetAllABCEntitiesInRange(GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), this.surroundingObjectTagsRange).ToList()) {

                // if no state manager script exists or it does and were  blocking surroundingobject status then continue
                if (entity.HasABCStateManager() == false || entity.HasABCStateManager() && entity.blockSurroundingObjectStatus == true)
                    continue;

                // check if object has correct tag
                if (ABC_Utilities.ObjectHasTag(entity.gameObject, ABC_Utilities.ConvertTags(Originator, surroundingObjectTagAffectTag))) {

                    // match found we can now add this to our list if we haven't reached our limit
                    if (retVal.Count < this.surroundingObjectTagLimit) {
                        retVal.Add(entity.gameObject);
                    }

                }

            }

            return retVal;

        }

        /// <summary>
        /// Returns a list of objects chosen by the ability to be surrounding objects. Objects found depends on settings, the originators targets and objects surrounding the ability starting position. 
        /// Will also interrupt the ability activating if surrounding objects are required and none are found
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private List<GameObject> GetSurroundingObjects(ABC_IEntity Originator) {


            List<GameObject> retVal = new List<GameObject>();

            retVal.AddRange(this.GetSurroundingTags(Originator));

            // if we haven't picked up any surrounding tag objects 
            if (this.surroundingObjectTags == true && this.surroundingObjectTagRequired == true && retVal.Count == 0 && this.AbilityActivationInterrupted() == false) {

                Originator.AddToDiagnosticLog("Tag Surrounding Object(s) required for activating " + this.name + " and none are avaliable");

                // stop the casting so the ability will not fire from here
                this.InterruptAbilityActivation(Originator);


                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("tag Surrounding Object required and none are avaliable");



            }



            GameObject targetSurroundingObject = this.GetSurroundingTarget(Originator);


            // if we required a surrounding object to use the ability and none was found then we will interupt the casting and log the message to the user. 
            if (targetSurroundingObject == null && this.surroundingObjectTargetRequired == true && this.AbilityActivationInterrupted() == false) {
                Originator.AddToDiagnosticLog("Target Surrounding Object required for activating " + this.name + " and none are avaliable");

                if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                    Originator.AddToAbilityLog("Target Surrounding Object required and none are avaliable");


                // stop the casting so the ability will not fire from here
                this.InterruptAbilityActivation(Originator);

            }

            if (targetSurroundingObject != null) {
                retVal.Add(targetSurroundingObject);
            }


            return retVal;

        }





        /// <summary>
        /// Will setup and return the abilities projectile to start graphic, placing it at the starting position provided. 
        /// </summary>
        /// <remarks>
        /// This is used primarily for Projectile to Start position where a projectile graphic will travel to a start position during the ability preparation stage
        /// </remarks>
        /// <param name="StartingPosition"></param>
        /// <returns>Projectile to start graphic gameobject</returns>
        private GameObject GetProjectileToStartObject(Vector3 StartingPosition) {

            GameObject retVal = null;

            retVal = this.projectileToStartPool.Where(obj => obj.activeInHierarchy == false).FirstOrDefault();

            if (retVal == null)
                retVal = CreateProjectileToStartObjects(true);

            retVal.transform.parent = null;

            retVal.transform.position = StartingPosition;


            // set name
            retVal.name = "ABC*_PTS_" + this.name;

            // turn hover script off
            retVal.GetComponent<ABC_Hover>().enabled = false;

            retVal.SetActive(false);


            return retVal;


        }

        /// <summary>
        /// Will setup and return the main ability projectile. Setup includes converting any surrounding objects, placing the projectile at the start position provided and configuring rigidbody settings. 
        /// </summary>
        /// <remarks>
        /// If the originator entity has a linked ability active then this object will be returned instead of the method grabbing a new object. This is currently used in projectile to start position where the ability projectile has already been created early and linked
        /// thus this method will return that object instead of a new one. 
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="StartingPosition">Vector3 Position where object should be placed</param>
        /// <param name="CreateNew">If true then a new object will always be created, else if one has already been made (for Proj to start) then that will be returned</param>
        /// <returns>main ability projectile gameobject</returns>
        private GameObject GetAbilityObject(ABC_IEntity Originator, Vector3 StartingPosition, bool CreateNew = false) {

            GameObject retVal = null;

            // if we have already spawned the original projectile for "projectile to start position" then move this over to our AB variable else get a new projectile
            if (CreateNew == false && this.preActivatedProjectiles.Count > 0 && this.useProjectileToStartPosition == true && this.useOriginalProjectilePTS == true) {

                retVal = this.preActivatedProjectiles.First();

                // make sure its a free man (not a child of any object anymore i.e moving with the caster)
                retVal.transform.parent = null;


                // reset any linked projectiles we no longer need to save this as its being used but don't delete the object as its about to be used
                this.RemovePreActivatedProjectile(false, retVal);


                // note: we don't set starting posistion as this was already done when the linked ability was first created

            } else {


                // create new ability object
                retVal = this.abilityPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

                if (retVal == null)
                    retVal = CreateAbilityObjects(true);


                if (retVal != null && this.scaleAbilityGraphic == true)
                    retVal.transform.localScale = new Vector3(this.abilityGraphicScale, this.abilityGraphicScale, this.abilityGraphicScale);


                retVal.transform.parent = null;

                retVal.transform.position = StartingPosition;

            }


            //get projectile script 
            ABC_Projectile projScript = retVal.GetComponent<ABC_Projectile>();
            projScript.enabled = false;


            if (surroundingObjects != null) {

                foreach (GameObject item in surroundingObjects) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(ConvertToSurroundingObject(item, retVal, Originator));
                }


            }




            retVal.name = "ABC*_" + this.name;


            // make sure hover and projectile to start scripts are turned off
            ABC_Hover hoverScript = retVal.GetComponent<ABC_Hover>();

            if (hoverScript != null)
                hoverScript.enabled = false;


            //  turn off proj to start if it exists 
            ABC_ObjectToDestination objToDestination = retVal.GetComponent<ABC_ObjectToDestination>();

            if (objToDestination != null)
                objToDestination.enabled = false;

            // reset rigidbody 
            Rigidbody abilityRigidBody = retVal.GetComponent<Rigidbody>();
            abilityRigidBody.useGravity = false;
            abilityRigidBody.linearDamping = this.travelDrag;
            abilityRigidBody.mass = this.mass;
            abilityRigidBody.isKinematic = this.isKinematic;



            //Disable all travel scripts currently on object
            DisableTravelScripts(retVal);



            return retVal;

        }


        /// <summary>
        /// Will create and send the ability projectile or a graphic (depending on configuration) to the abilities starting position. 
        /// <remarks>
        /// If the ability projectile is used then colliders will be disabled as this is for Aesthetic only. It will also be linked to the originator entity to be retrieved later on when it is to be initiated.
        /// If a graphic is used then it will stay for the duration defined unless the delay is 0 in which it will dissapear when the ability projectile is created and initiated
        /// Used during ability preparation to simulate the entity retriving lightning from the sky or rocks from the ground before sending it on etc
        /// </remarks>
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="StartPositionObject">The object relating to the starting position</param>
        /// <param name="StartingPositionOffset">Starting Position Offset</param>
        /// <param name="StartingPositionForwardOffset">Starting Position Forward Offset</param>
        /// <param name="StartingPositionRightOffset">Starting Position Right Offset</param>
        /// <param name="Delay">(Optional) Delay till projectile to start begins</param>
        IEnumerator SendProjectileToStartPosition(ABC_IEntity Originator, GameObject StartPositionObject, Vector3 StartingPositionOffset, float StartingPositionForwardOffset, float StartingPositionRightOffset, float Delay = 0f) {

            if (this.useOriginalProjectilePTS == true && this.abilityType == AbilityType.RayCast) {
                // original projectile can't be created if the ability is a raycast so stop here
                Originator.AddToDiagnosticLog(this.name + " is of type Raycast so original Projectile does not exist, Sending Projectile To Start Position has been cancelled.");
                yield break;
            }


            //Wait for Delay if provided 
            if (Delay > 0f)
                yield return new WaitForSeconds(Delay);


            GameObject ptsAB = null;

            // set starting positions for our projectile to start position   
            Vector3 startingPosition = GetStartingPosition(Originator, AbilityStartPositionType.ProjectileToStartPosition);

            if (this.useOriginalProjectilePTS == true) {
                //True is passed in get ability object so we always create a new projectile 
                ptsAB = this.GetAbilityObject(Originator, startingPosition, true);

            } else {
                ptsAB = this.GetProjectileToStartObject(startingPosition);
            }

            GameObject startingPositionObj = this.GetStartingPositionObject(Originator, AbilityStartPositionType.ProjectileToStartPosition);
            Transform startingPositionObjTransform = startingPositionObj.transform;

            //If on world then get position
            if (this.projToStartStartingPosition == StartingPosition.OnWorld) {

                ptsAB.transform.position = this.GetStartingPosition(Originator, AbilityStartPositionType.ProjectileToStartPosition);

            } else {

                //else attach to starting object

                ptsAB.transform.SetParent(startingPositionObj.transform);
                ptsAB.transform.localRotation = Quaternion.identity;
                ptsAB.transform.localPosition = Vector3.zero;

                if (this.projToStartMoveWithTarget == false) {
                    ptsAB.transform.SetParent(null);
                }


            }

            //Adjust ability by rotation/transform
            ptsAB.transform.position = ptsAB.transform.position + this.projToStartPositionOffset + startingPositionObjTransform.forward * this.projToStartPositionForwardOffset + startingPositionObjTransform.right * this.projToStartPositionRightOffset;
            ptsAB.transform.localRotation *= Quaternion.Euler(this.projToStartRotation);


            //If set to then copy euler angles also
            if (this.projToStartSetEulerRotation)
                ptsAB.transform.localEulerAngles = startingPositionObjTransform.localEulerAngles + this.projToStartRotation;


            if (this.projToStartTravelToAbilityStartPosition == true) {

                // add rigidbody and disable gravity
                Rigidbody ptsABRigidBody = ptsAB.GetComponent<Rigidbody>();
                ptsABRigidBody.useGravity = false;

                // disable collider for now
                ABC_Utilities.mbSurrogate.StartCoroutine(ToggleObjectColliders(ptsAB, false));

                // setup where we want to travel to etc 
                GameObject objToDestinationObj = null;
                GameObject objToDestinationParent = null;
                Vector3 objToDestinationPositionOverride = Vector3.zero;


                //If on world then get position and add to override
                if (this.startingPosition == StartingPosition.OnWorld) {
                    objToDestinationPositionOverride = this.GetStartingPosition(Originator, AbilityStartPositionType.StartPosition);
                }


                ABC_ObjectToDestination objToDestination = ptsAB.GetComponent<ABC_ObjectToDestination>();

                if (objToDestination == null) {
                    objToDestination = ptsAB.AddComponent<ABC_ObjectToDestination>();
                }

                objToDestination.enabled = false;
                objToDestinationObj = StartPositionObject;

                objToDestination.destinationObj = objToDestinationObj;
                objToDestination.positionOverride = objToDestinationPositionOverride;
                objToDestination.positionOffset = StartingPositionOffset;
                objToDestination.positionForwardOffset = StartingPositionForwardOffset;
                objToDestination.positionRightOffset = StartingPositionRightOffset;
                objToDestination.rotateToTarget = this.projToStartRotateToTarget;
                objToDestination.secondsToTarget = this.projToStartReachPositionTime;
                objToDestination.stopMovementToDestinationOnCollision = false;
                objToDestination.continuouslyCalculateStopMovementCollision = false;
                objToDestination.hoverOnSpot = this.projToStartHoverOnSpot;
                objToDestination.hoverDistance = this.projToStartHoverDistance;
                objToDestination.travelDelay = this.projToStartTravelDelay;


                //If a parent override hasn't been set then we take the position transform; 
                if (objToDestinationParent == null && this.startingPosition != StartingPosition.OnWorld)
                    objToDestinationParent = objToDestinationObj.gameObject;

                objToDestination.destinationParent = this.projToStartMoveWithTarget == true ? objToDestinationParent : null;
                objToDestination.continuouslyCalculateDestination = true;

                objToDestination.enabled = true;
            }


            // we might need to destroy the projectile later on so link the graphic to a variable
            this.preActivatedProjectiles.Add(ptsAB);


            IEnumerator disableCoroutine = null;

            // destroy after delay if greater then 0 and not the original projectile 
            if (this.projToStartPosDuration > 0f && this.useOriginalProjectilePTS == false) {
                ABC_Utilities.mbSurrogate.StartCoroutine(disableCoroutine = DestroyObject(ptsAB, this.projToStartPosDuration));
                this.preActivatedProjectileDisableCoroutines.Add(disableCoroutine);
            }

            // set active
            ptsAB.SetActive(true);



        }


        /// <summary>
        /// Sends an object to another object target. Object will take the duration provided to reach the target object.
        /// </summary>
        /// <param name="Target">Target Object</param>
        /// <param name="Obj">Object which will be sent to the target</param>
        /// <param name="TargetOffset">Offset applied to the target position</param>
        /// <param name="Duration">How long it takes for the object to reach the target</param>
        /// <param name="CollidersOn">Determines if the object being sent to the target will have colliders enabled or disabled</param>
        private void SendObjectToTarget(GameObject Target, GameObject Obj, Vector3 TargetOffset, float Duration = 2f, bool CollidersOn = false) {

            if (Obj != null) {


                foreach (Collider meCol in Obj.GetComponentsInChildren<Collider>(true)) {
                    meCol.enabled = CollidersOn;
                }


                // setup script with the right parameters 

                // check if script exists
                ABC_ObjectToDestination objToDestination = Obj.GetComponent<ABC_ObjectToDestination>();

                // if it doesn't exist then add it 
                if (objToDestination == null)
                    objToDestination = Obj.AddComponent<ABC_ObjectToDestination>();


                //Disable script whilst we add parameters (stop remove script as disabling will just remove the script)
                objToDestination.removeScript = false;
                objToDestination.enabled = false;


                objToDestination.destinationObj = Target;
                objToDestination.positionOverride = Vector3.zero;
                objToDestination.destinationParent = Target;
                objToDestination.positionOffset = TargetOffset;
                objToDestination.positionForwardOffset = 0f;
                objToDestination.positionRightOffset = 0f;
                objToDestination.secondsToTarget = Duration;
                objToDestination.stopMovementToDestinationOnCollision = false;
                objToDestination.continuouslyCalculateStopMovementCollision = false; 
                objToDestination.rotateToTarget = true;
                objToDestination.hoverOnSpot = false;
                objToDestination.hoverDistance = 0.1f;
                objToDestination.travelDelay = 0f;
                objToDestination.removeScript = true;
                objToDestination.continuouslyCalculateDestination = true;

                objToDestination.enabled = true;




            }


        }


        /// <summary>
        /// Converts the object to a state in which it can be classified as a child graphic (SurroundingObject) of the ability projectile object provided.
        /// </summary>
        /// <remarks>
        /// if blockSurroundingObjectStatus object is true then the surrounding object can not be stolen by other abilities and will stay attached to the original projectile till the end. 
        /// </remarks>
        /// <param name="ObjectToConvert">Object which will be converted to a surrounding object</param>
        /// <param name="ProjectileParent">Ability Projectile which the object once converted will be a child of</param>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns></returns>
        private IEnumerator ConvertToSurroundingObject(GameObject ObjectToConvert, GameObject ProjectileParent, ABC_IEntity Originator) {

            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(ObjectToConvert);

            //If the object to convert is already a surrounding object of the ProjectileParent then end here
            if (iEntity.surroundingObjectLinkedProjectile == ProjectileParent)
                yield break;

            // object can only be converted if the statemanager has not got a block on
            if (iEntity.HasABCStateManager() == false || iEntity.blockSurroundingObjectStatus == true)
                yield break;


            // turn off all colliders on the surrounding object to start with 
            yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleObjectColliders(ObjectToConvert, false));

            // add to end of name so rest of code knows its a surrounding object
            ObjectToConvert.name = ObjectToConvert.name + "*_ABCSurroundingObject";

            //link to projectile parent 
            iEntity.surroundingObjectLinkedProjectile = ProjectileParent;

            //If the object had a rigibody then track if it was kinematic for when we revert, then make it kinematic so we can move it
            Rigidbody objRigidBody = ObjectToConvert.GetComponent<Rigidbody>();
            if (objRigidBody != null) {
                iEntity.surroundingObjectLinkIsKinematic = objRigidBody.isKinematic;
                iEntity.surroundingObjectLinkInterpolateState = objRigidBody.interpolation;

                // Turn is kinematic status to true and interpolate to allow to work as a child without any issues
                objRigidBody.isKinematic = true;
                objRigidBody.interpolation = RigidbodyInterpolation.Extrapolate;
            }

            //If the object had a collider then track if it was trigger for when we revert, then make it kinematic so we can move it
            Collider objCollider = ObjectToConvert.GetComponent<Collider>();
            if (objCollider != null) {
                iEntity.surroundingObjectLinkIsTrigger = objCollider.isTrigger;
                objCollider.isTrigger = true;
            }



            //Retrieve/Create IEntity so we can toggle movement
            ABC_IEntity objectToConvertIEntity = ABC_Utilities.GetStaticABCEntity(ObjectToConvert);
            //restrict movement whilst a surrounding object
            objectToConvertIEntity.ToggleMovement(Time.time, false, false, true);


            // create random offset depending on the minimum and maximum scatter given in settings. This makes sure that any surrounding objects don't sit on top of each other 
            // it is ok for it to be 0 0 0 (so it is on top of each other)
            Vector3 scatterOffset = new Vector3(Random.Range(this.minimumScatterRange, this.maximumScatterRange), Random.Range(this.minimumScatterRange, this.maximumScatterRange), Random.Range(this.minimumScatterRange, this.maximumScatterRange));


            //If not sending object to projectile then appear at projectile
            if (this.sendObjectToProjectile == false) {

                ObjectToConvert.transform.position = ProjectileParent.transform.position + scatterOffset;

            } else {

                // send the object to the projectile 
                SendObjectToTarget(ProjectileParent, ObjectToConvert, scatterOffset, this.objectToProjectileDuration);
                // wait a bit for object to start approaching target 
                yield return new WaitForSeconds(0.1f);
            }



            // now attach the object to the projectile 
            ObjectToConvert.transform.parent = ProjectileParent.transform;

            // if the surroundingObject is a target and starting positions is not on target then it's not a target now!
            // if originator exists 
            if (Originator != null) {

                if (ObjectToConvert == Originator.target)
                    Originator.RemoveTarget();

                if (ObjectToConvert == Originator.softTarget)
                    Originator.RemoveSoftTarget();

            }


            // if lock object is true then the surrounding object can not be stolen by other abilities as we will turn the block status to true
            if (this.lockSurroundingObject == true)
                iEntity.blockSurroundingObjectStatus = true;



            // Turn on collider
            if (this.applyColliderToSurroundingObjects == true) {
                // if we appling collider when object reaches projectile then add the duration to projectile else pass through 0 
                ABC_Utilities.mbSurrogate.StartCoroutine(ToggleObjectColliders(ObjectToConvert, true, this.applyColliderWhenProjectileReached ? this.objectToProjectileDuration : 1f));
            }


        }

        /// <summary>
        /// Reverts the object from a SurroundingObject back to a normal object. Detaching it from the ability projectile
        /// </summary>
        /// <param name="ObjectToRevert">Object to revert back from being a SurroundingObject</param>
        /// <param name="ProjectileParent">Projectile object which the object is currently a surrounding object for</param>
        /// <param name="Originator">Entity that activated the ability</param>
        private void RevertFromSurroundingObject(GameObject ObjectToRevert, GameObject ProjectileParent, ABC_IEntity Originator) {

            // if its not a surrounding object or not still a child of original projectile and has a parent - may have been snatched by another spell or already collided then end here
            if (ObjectToRevert.name.Contains("*_ABCSurroundingObject") == false || ObjectToRevert.transform.IsChildOf(ProjectileParent.transform) == false && ObjectToRevert.transform.parent != null)
                return;



            //Detach parent
            ObjectToRevert.transform.parent = null;

            //  hover script  turn it off
            var hoverScript = ObjectToRevert.GetComponent<ABC_Hover>();

            if (hoverScript != null)
                hoverScript.enabled = false;

            //  turn off proj to start if it exists 
            ABC_ObjectToDestination objToDestination = ObjectToRevert.GetComponent<ABC_ObjectToDestination>();

            if (objToDestination != null)
                objToDestination.enabled = false;


            // get ientity to modify statemanager values
            ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(ObjectToRevert);

            if (iEntity.HasABCStateManager()) {
                iEntity.surroundingObjectLinkedProjectile = null;

                //If the object had a rigibody then revert the kinematic and interpolate setting to how it was before the conversion
                Rigidbody objRigidBody = ObjectToRevert.GetComponent<Rigidbody>();
                if (objRigidBody != null) {
                    objRigidBody.isKinematic = iEntity.surroundingObjectLinkIsKinematic;
                    objRigidBody.interpolation = iEntity.surroundingObjectLinkInterpolateState;
                }

                //If the object had a collider then rever the trigger to how it was before the conversion
                Collider objCollider = ObjectToRevert.GetComponent<Collider>();
                if (objCollider != null)
                    objCollider.isTrigger = iEntity.surroundingObjectLinkIsTrigger;


            }


            Collider ObjCollider = ObjectToRevert.GetComponent<Collider>();
            if (ObjCollider != null) {
                // turn collider off to reset any collision ignores 
                ObjCollider.enabled = false;
                // make sure collider is back on
                ObjCollider.enabled = true;
                ObjCollider.isTrigger = false;

                if (Terrain.activeTerrain != null)
                    Physics.IgnoreCollision(ObjCollider, Terrain.activeTerrain.GetComponent<Collider>(), false);

                if (ProjectileParent != null && ProjectileParent.GetComponent<Collider>() != null)
                    Physics.IgnoreCollision(ObjCollider, ProjectileParent.GetComponent<Collider>(), false);

                if (Originator != null && Originator.gameObject.GetComponent<Collider>() != null)
                    Physics.IgnoreCollision(ObjCollider, Originator.gameObject.GetComponent<Collider>(), false);
            }




            //Retrieve/Create IEntity so we can toggle movement
            ABC_IEntity objectToRevertIEntity = ABC_Utilities.GetStaticABCEntity(ObjectToRevert);
            //renew movement again
            objectToRevertIEntity.ToggleMovement(Time.time, true, false, true);

            // remove the ABC name
            ObjectToRevert.name = ObjectToRevert.name.Remove(ObjectToRevert.name.Length - 22, 22);

            // turn of the block stopping any other abilities stealing the object
            iEntity.blockSurroundingObjectStatus = false;


            // do we now "delete the projectile" if not it will be dropped"
            if (this.destroySurroundingObject == true)
                ObjectToRevert.gameObject.SetActive(false);




        }

        /// <summary>
        /// Will return a position offset which will be added on to the ability target position to make the ability miss. A dice roll is performed first to determine
        /// if the ability will miss or not depending on the originators miss chance. If ability is set to never miss then this will return no offset adjustments
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Vector3 offset to be applied to ability target position to make it miss or not</returns>
        private Vector3 GetMissChancePositionOffset(ABC_IEntity Originator) {

            if (this.abilityCanMiss == false)
                return new Vector3(0, 0, 0);

            return Originator.GetMissChancePositionOffset();
        }


        /// <summary>
        /// Will return the Vector3 Position of the abilities target.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Vector3 Position of the abilities current target</returns>
        private Vector3 GetTargetPosition(ABC_IEntity Originator) {

            //if were not travelling we need to appear at target
            switch (this.travelType) {
                case TravelType.SelectedTarget:
                    var targetTransform = target.transform;
                    return targetTransform.position + this.selectedTargetOffset + targetTransform.forward * this.selectedTargetForwardOffset + targetTransform.right * this.selectedTargetRightOffset;
                case TravelType.NearestTag:

                    var tagTargetTransform = tagTargets.FirstOrDefault().transform;
                    return tagTargetTransform.position + this.selectedTargetOffset + tagTargetTransform.forward * this.selectedTargetForwardOffset + tagTargetTransform.right * this.selectedTargetRightOffset;
                case TravelType.Self:
                    return Originator.transform.position + this.selectedTargetOffset + Originator.transform.forward * this.selectedTargetForwardOffset + Originator.transform.right * this.selectedTargetRightOffset;
                case TravelType.ToWorld:
                    return this.worldTargetPosition + this.selectedTargetOffset + Originator.transform.forward * this.selectedTargetForwardOffset + Originator.transform.right * this.selectedTargetRightOffset;
                case TravelType.MouseTarget:
                case TravelType.Mouse2D:
                    return GetMousePosition(Originator) + this.selectedTargetOffset + Originator.transform.forward * this.selectedTargetForwardOffset + Originator.transform.right * this.selectedTargetRightOffset;
                case TravelType.Crosshair:
                    return rayCastTargetPosition + this.selectedTargetOffset + Originator.transform.forward * this.selectedTargetForwardOffset + Originator.transform.right * this.selectedTargetRightOffset;
                default:
                    Originator.AddToDiagnosticLog(this.name + " Error: ability target position was not correctly determined");
                    return new Vector3(0, 0, 0);
            }

        }

        /// <summary>
        /// Gets a hit position from a Raycast sent to the mouse. Raycast Ignores the originator and all ABC Projectiles. Will not take into account mouse X, Y, Z position if lock setting is enabled by ability. 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Vector3 hit position, if nothing has been hit it returns the point 50 units along the ray</returns>
        private Vector3 GetMousePosition(ABC_IEntity Originator) {

            Vector3 retval = new Vector3(0, 0, 0);

            Ray ray = Originator.Camera.ScreenPointToRay(Originator.inputManager.GetMousePosition());
            RaycastHit hit;
            // in statement below we ignore ourself and all ABC projectiles as this screws with hit positioning (for no delay casting like beams etc) 
            if (Physics.Raycast(ray, out hit) && this.mouseFrontOnly == false && hit.transform.gameObject != Originator.gameObject && !hit.transform.gameObject.name.Contains(("ABC*_"))) {
                retval = hit.point;
            } else {
                retval = ray.GetPoint(50);
            }


            // *** Explanation of locking x y and z
            //	At a basic level the LookAt() function calculates rotational values for the object you want to rotate (transform) based on the positional values of the object you want to look at (target). The key benefit to LookAt() is that it ensures your transform always faces the target, calculating the necessary rotational values accordingly.
            //				
            //	All the above code is really saying is: "calculate the rotational values for the transform based on the target's X and Z positions (horizontal plane), but the target's Y position will always be the same as mine." E.g. the same height as me. The resulting calculation forces the transform to face in the direction of the target but believes the target is at the same height of the transform. Thus preventing vertical rotation.


            float X;
            float Y;
            float Z;

            // what mouse positioning are we ignoring (X, Y, Z)

            if (this.mouseForwardLockX == false) {
                X = retval.x;
            } else {
                X = Originator.transform.position.x;
            }

            if (this.mouseForwardLockY == false) {
                Y = retval.y;
            } else {
                Y = Originator.transform.position.y;
            }

            if (this.mouseForwardLockZ == false) {
                Z = retval.z;
            } else {
                Z = Originator.transform.position.z;
            }

            retval = new Vector3(X, Y, Z);


            return retval;

        }

        /// <summary>
        /// Gets the position of a raycast shoot out from the crosshair position towards objects or a hit position
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Vector3 Position of the point reached by the raycast</returns>
        private Vector3 GetCrosshairRayCastPosition(ABC_IEntity Originator) {

            // if were hitting for fps mode (fires through recticle) then get ray cast position 
            // reset the previous position
            Vector3 retval = Vector3.zero;

            // direct the ray towards the crosshair position determined in settings
            Ray ray = Originator.Camera.ViewportPointToRay(Originator.crossHairPosition);
            RaycastHit[] hit;
            // spherecast to the ray using the determined radius and layer. 
            hit = Physics.SphereCastAll(ray, this.crossHairRaycastRadius, 600f, this.affectLayer);

            //If we are set to only return distance point only then return here with the distance defined. Rest of code deals with points from objects hit
            if (this.crossHairRaycastReturnDistancePointOnly == true)
                return ray.GetPoint(this.crossHairRaycastReturnedDistance);


            // lets get the first hit object that we require (ignores caster and the linked particle)
            if (hit.Length > 1) {
                for (int i = 0; i < hit.Length; i++) {

                    if (hit[i].transform.gameObject != Originator.gameObject && (Terrain.activeTerrain != null && hit[i].transform.gameObject != Terrain.activeTerrain.transform.gameObject)) {
                        retval = hit[i].point;
                        Originator.AddToDiagnosticLog(this.name + " activation - crosshair target aquired: " + hit[i].transform.name);
                        break;
                    }
                }
            }

            // if we didn't hit anything then it will get the point in the distance to fire anyway 
            if (retval == Vector3.zero)
                retval = ray.GetPoint(crossHairRaycastReturnedDistance);


            return retval;

        }


        /// <summary>
        /// Will return the position where the ability has been set to start from the type provided   
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Type">Type (StartPosition/ProjectileToStartPosition/AdditionalStartPosition) </param>
        /// <param name="AdditionalStartPosition">Additional Start Position Object, If Type is "Additional Start Position" then it will retrieve starting position values from this object</param>
        /// <returns>Vector3 position where ability will start</returns>
        private Vector3 GetStartingPosition(ABC_IEntity Originator, AbilityStartPositionType Type, AdditionalStartingPosition AdditionalStartPosition = null) {



            Vector3 positionOffset = new Vector3(0, 0, 0);
            float positionForwardOffset = 0f;
            float positionRightOffset = 0f;
            StartingPosition positionType = StartingPosition.Self;
            GameObject positionOnObject = null;
            string positionOnTag = null;


            switch (Type) {
                case AbilityStartPositionType.StartPosition:

                    positionOffset = this.startingPositionOffset;
                    positionForwardOffset = this.startingPositionForwardOffset;
                    positionRightOffset = this.startingPositionRightOffset;
                    positionType = this.startingPosition;
                    positionOnObject = this.startingPositionOnObject.GameObject;
                    positionOnTag = this.startingPositionOnTag;

                    break;
                case AbilityStartPositionType.ProjectileToStartPosition:


                    positionOffset = this.projToStartPositionOffset;
                    positionForwardOffset = this.projToStartPositionForwardOffset;
                    positionRightOffset = this.projToStartPositionRightOffset;
                    positionType = this.projToStartStartingPosition;
                    positionOnObject = this.projToStartPositionOnObject.GameObject;
                    positionOnTag = this.projToStartPositionOnTag;

                    break;
                case AbilityStartPositionType.AdditionalStartPosition:

                    if (AdditionalStartPosition != null) {

                        positionOffset = AdditionalStartPosition.startingPositionOffset;
                        positionForwardOffset = AdditionalStartPosition.startingPositionForwardOffset;
                        positionRightOffset = AdditionalStartPosition.startingPositionRightOffset;
                        positionType = AdditionalStartPosition.startingPosition;
                        positionOnObject = AdditionalStartPosition.startingPositionOnObject.GameObject;
                        positionOnTag = AdditionalStartPosition.startingPositionOnTag;


                    } else {
                        //If additional start position was requested and object was not given then values will stay as default
                        Originator.AddToDiagnosticLog("Additional Start Position was requested but no additional start position object was provided, defaulting to start position");
                    }



                    break;
            }



            switch (positionType) {
                case StartingPosition.Self:
                    return Originator.transform.position + positionOffset + Originator.transform.forward * positionForwardOffset + Originator.transform.right * positionRightOffset;
                case StartingPosition.OnObject:
                    if (positionOnObject != null) {
                        var onObjectTransform = positionOnObject.transform;
                        return onObjectTransform.position + positionOffset + onObjectTransform.forward * positionForwardOffset + onObjectTransform.right * positionRightOffset;
                    }
                    break;
                case StartingPosition.OnTag:
                    GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                    if (onTagObj != null) {
                        Transform onTagTransform = onTagObj.transform;
                        return onTagTransform.position + positionOffset + onTagTransform.forward * positionForwardOffset + onTagTransform.right * positionRightOffset;
                    }
                    break;
                case StartingPosition.OnSelfTag:
                    GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(Originator, positionOnTag);
                    if (onSelfTagObj != null) {
                        Transform onSelfTagTransform = onSelfTagObj.transform;
                        return onSelfTagTransform.position + positionOffset + onSelfTagTransform.forward * positionForwardOffset + onSelfTagTransform.right * positionRightOffset;
                    }
                    break;
                case StartingPosition.Target:
                    if (target != null) {
                        var targetTransform = target.transform;
                        return targetTransform.position + positionOffset + targetTransform.forward * positionForwardOffset + targetTransform.right * positionRightOffset;
                    }
                    break;
                case StartingPosition.OnWorld:
                    if (this.worldTarget != null) {
                        var worldTargetTransform = this.worldTarget.transform;
                        return this.worldTargetPosition + positionOffset + worldTargetTransform.forward * positionForwardOffset + worldTargetTransform.right * positionRightOffset;
                    }
                    break;
                case StartingPosition.CameraCenter:
                    var cameraTransform = Originator.Camera.transform;
                    return Originator.Camera.transform.position + positionOffset + cameraTransform.forward * positionForwardOffset + cameraTransform.right * positionRightOffset;
                default:
                    Originator.AddToDiagnosticLog(this.name + " Error: ability starting position was not correctly determined.");
                    break;
            }


            // if we reached this far then something has gone wrong or one of the gameobjects required for the starting position doesn't exist so we will just revert to our self starting position
            return Originator.transform.position + positionOffset + Originator.transform.forward * positionForwardOffset + Originator.transform.right * positionRightOffset;
        }

        /// <summary>
        /// Will return the GameObject relating to where the ability has been set to start from
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Type">Type (StartPosition/ProjectileToStartPosition)</param>
        /// <param name="AdditionalStartPosition">Additional Start Position Object, If Type is "Additional Start Position" then it will retrieve starting position values from this object</param>
        /// <returns>GameObject where the ability will start</returns>
        private GameObject GetStartingPositionObject(ABC_IEntity Originator, AbilityStartPositionType Type, AdditionalStartingPosition AdditionalStartPosition = null) {


            StartingPosition positionType = StartingPosition.Self;
            GameObject positionOnObject = null;
            string positionOnTag = null;


            switch (Type) {
                case AbilityStartPositionType.StartPosition:

                    positionType = this.startingPosition;
                    positionOnObject = this.startingPositionOnObject.GameObject;
                    positionOnTag = this.startingPositionOnTag;

                    break;
                case AbilityStartPositionType.ProjectileToStartPosition:

                    positionType = this.projToStartStartingPosition;
                    positionOnObject = this.projToStartPositionOnObject.GameObject;
                    positionOnTag = this.projToStartPositionOnTag;

                    break;
                case AbilityStartPositionType.AdditionalStartPosition:

                    if (AdditionalStartPosition != null) {
                        positionType = AdditionalStartPosition.startingPosition;
                        positionOnObject = AdditionalStartPosition.startingPositionOnObject.GameObject;
                        positionOnTag = AdditionalStartPosition.startingPositionOnTag;

                    } else {
                        //If additional start position was requested and object was not given then values will stay as default
                        Originator.AddToDiagnosticLog("Additional Start Position was requested but no additional start position object was provided, defaulting to start position");
                    }

                    break;

            }



            switch (positionType) {
                case StartingPosition.Self:
                    return Originator.gameObject;
                case StartingPosition.OnObject:
                    return positionOnObject.gameObject;
                case StartingPosition.OnTag:
                    GameObject onTagObj = GameObject.FindGameObjectWithTag(positionOnTag);
                    if (onTagObj != null)
                        return onTagObj;
                    break;
                case StartingPosition.OnSelfTag:
                    GameObject onSelfTagObj = ABC_Utilities.TraverseObjectForTag(Originator, positionOnTag);
                    if (onSelfTagObj != null)
                        return onSelfTagObj;
                    break;
                case StartingPosition.Target:
                    if (target != null) {
                        return target.gameObject;
                    }
                    break;
                case StartingPosition.OnWorld:
                    if (this.worldTarget != null) {
                        return worldTarget.gameObject;
                    }
                    break;
                case StartingPosition.CameraCenter:
                    return Originator.Camera.gameObject;
                default:
                    Originator.AddToDiagnosticLog(this.name + " Error: ability starting position was not correctly determined.");
                    break;
            }


            // if we reached this far then something has gone wrong or one of the gameobjects required for the starting position doesn't exist so we will just revert to our self starting position
            return Originator.gameObject;

        }

        /// <summary>
        /// Will activate all abilities set to activate when this ability activates (Ability Activation Links)
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private void ActivateLinkedAbilities(ABC_IEntity Originator) {

            //If not set to activate linked abilities then end here
            if (this.enableAbilityActivationLinks == false)
                return;

            //Force ability activation on any IDs that don't match this ability (to avoid infinite activation loops)
            foreach (int ID in this.activationLinkAbilityIDs.Where(a => a != this.abilityID).ToList())
                Originator.ForceAbilityActivation(ID);

        }

        /// <summary>
        /// Method will make ability wait until the originator selects a new target (even if one is already selected or not). Recommended that the function is called with a yield return.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator WaitForNewOriginatorTarget(ABC_IEntity Originator) {

            yield return new WaitForSeconds(0.1f);

            //Selected target
            if (this.travelType == TravelType.SelectedTarget || this.startingPosition == StartingPosition.Target) {

                //raise event as ability before target has started
                this.RaiseOriginatorsAbilityBeforeTargetEvent(Originator, true);

                if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                    Originator.AddToAbilityLog("Select a target to cast " + this.name + " on ");

                // if were selecting an ability before target then set waiting on target as true and wait till we pick a new target
                if (this.abilityBeforeTarget == true) {

                    // waiting on target 
                    Originator.SelectNewTarget(TargetType.Target);

                }

                //Turn on range indicator 
                this.ToggleAbilityRangeIndicator(true, Originator);


                while (Originator.NewTargetSelected(TargetType.Target) == false && this.AbilityActivationInterrupted() == false) {
                    // wait for target to be selected
                    yield return null;
                }

                //Turn off the range indicator
                this.ToggleAbilityRangeIndicator(false, Originator);


            }



            //Mouse
            if (this.travelType == TravelType.MouseTarget || this.travelType == TravelType.Mouse2D) {

                //raise event as ability before target has started
                this.RaiseOriginatorsAbilityBeforeTargetEvent(Originator, true);

                if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                    Originator.AddToAbilityLog("Select a mouse position to activate towards " + this.name);


                Originator.SelectNewTarget(TargetType.Mouse);

                while (Originator.NewTargetSelected(TargetType.Mouse) == false && this.AbilityActivationInterrupted() == false) {

                    //turn on mouse target indicator making sure it's always facing direction of the mouse 
                    this.ToggleAbilityMouseTargetIndicator(true, Originator);

                    yield return null;
                }

                //Turn off mouse target indicator
                this.ToggleAbilityMouseTargetIndicator(false, Originator);


            }


            //World 
            if (this.travelType == TravelType.ToWorld || this.startingPosition == StartingPosition.OnWorld) {

                //raise event as ability before target has started
                this.RaiseOriginatorsAbilityBeforeTargetEvent(Originator, true);

                if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                    Originator.AddToAbilityLog("Select a world position to activate on " + this.name);

                Originator.SelectNewTarget(TargetType.World);

                //Turn on range indicator
                this.ToggleAbilityRangeIndicator(true, Originator);

                //While waiting for new target and ability hasn't been interrupted
                while (Originator.NewTargetSelected(TargetType.World) == false && this.AbilityActivationInterrupted() == false) {

                    // turn on world target indicator keeping it at mouse/crosshair position
                    this.ToggleAbilityWorldTargetIndicator(true, Originator);


                    yield return null;
                }


                // make sure ability indicators are turned off now we no longer waiting 
                this.ToggleAbilityWorldTargetIndicator(false, Originator);
                this.ToggleAbilityRangeIndicator(false, Originator);

            }


            //raise event as ability before target has finished
            this.RaiseOriginatorsAbilityBeforeTargetEvent(Originator, false);

        }

        /// <summary>
        /// Sets the ability's target depending on its travel type and returns a bool reflecting if the target was successfully set 
        /// </summary>
        /// <remarks>
        /// Sets the correct target variable which is used later when activating
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>True if target was set successfully, else false</returns>
        private bool SetAbilityTargets(ABC_IEntity Originator) {


            //If set to nearest tag then find the nearest object with the correct tag and set that as the target 
            if (this.travelType == TravelType.NearestTag) {

                //Determine if we are ignoring the originator or not when searching for tags
                GameObject IgnoreEntity = null;
                if (this.travelNearestTagIgnoreOriginator)
                    IgnoreEntity = Originator.gameObject;

                //Find  nearest tag objects
                tagTargets = this.GetAllObjectsWithNearestTag(this.GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), this.travelNearestTagRange, ABC_Utilities.ConvertTags(Originator, this.travelNearestTagList), IgnoreEntity, this.travelNearestTagRandomiseSearch);

                //Store the first in list just for further checks later on
                target = tagTargets.FirstOrDefault();


                //No object found, if set to still travel then change this to travel type forward, This is also done in CanActivate() incase objects were in range before but now arnt
                if (target == null && this.noTargetStillTravel == true) {
                    // turn the flag to the current travel type to let rest of code know its been activated ready to be changed back next time
                    this.noTargetStillTravelActivated = (NoTargetStillTravelPreviousType)System.Enum.Parse(typeof(NoTargetStillTravelPreviousType), this.travelType.ToString());
                    this.travelType = TravelType.Forward;
                } else if (target == null) {
                    //Condition not met so return false here
                    Originator.AddToDiagnosticLog("No tagged targets are in range to activate " + this.name);


                    if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                        Originator.AddToAbilityLog("No tagged targets are in range to activate " + this.name);

                    return false;
                }
            }


            if ((this.travelType == TravelType.ToWorld || this.startingPosition == StartingPosition.OnWorld || this.useProjectileToStartPosition == true && this.projToStartStartingPosition == StartingPosition.OnWorld)) {

                // if we haven't got world object then ability targets havent been set so return false
                if (Originator.worldTargetObj == null && Originator.worldTargetObj == null)
                    return false;


                worldTarget = Originator.worldTargetObj;
                worldTargetPosition = Originator.worldTargetPos;


            }


            if (this.abilityType == AbilityType.Melee || this.travelType == TravelType.SelectedTarget || this.startingPosition == StartingPosition.Target || this.useProjectileToStartPosition == true && this.projToStartStartingPosition == StartingPosition.Target) {

                // clear out any old ability Target Objects 
                target = null;


                // set ability target object with current target
                if (Originator.target != null) {
                    target = Originator.target;
                } else if (Originator.softTarget != null && this.auxiliarySoftTarget == true && (this.travelType == TravelType.SelectedTarget || this.abilityType == AbilityType.Melee)) {
                    // if setting is set (for melee or selected target or for surroundingobject then set aux target)
                    target = Originator.softTarget;
                } else if (Originator.softTarget != null && this.startingPositionAuxiliarySoftTarget == true && this.startingPosition == StartingPosition.Target) {
                    target = Originator.softTarget;
                }



                if (target != null && this.selectedTargetRestrictTargets == true && (this.abilityType != AbilityType.Melee || this.abilityType == AbilityType.Melee && noTargetStillTravel == false) && ABC_Utilities.ObjectHasTag(target, ABC_Utilities.ConvertTags(Originator, this.selectedTargetOnlyCastOnTag)) == false) {



                    // we are restricting targets so check if the target has the right tag 
                    Originator.AddToDiagnosticLog("Target Restriction is on. Unable to cast " + this.name + " on " + target.name);


                    if (Originator.LogInformationAbout(LoggingType.AbilityActivationError))
                        Originator.AddToAbilityLog("Unable to cast " + this.name + " on " + target.name);

                    target = null;

                }


                // if ability target was not set or ability was interrupted then return false unless it's a melee type
                if (target == null && (this.abilityType != AbilityType.Melee || this.abilityType == AbilityType.Melee && noTargetStillTravel == false))
                    return false;

            }


            // everything was set correctly so we can return true 
            return true;

        }

        /// <summary>
        /// Will establish the abilities target making sure the Ability has set the correct target before continuing. 
        /// Depending on parameters provided the function will either interrupt the ability activation if the correct target is not established or wait till the correct target has been established
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="InterruptCastOnIncorrectTarget">If true then ability activation will be interuppted if the target was not correctly established the first time round, else it will keep trying until ability is interrupted or target is established</param>
        /// <param name="ValidateTarget">If true then the function will request for the Originator to set a new target if the current one can not be established, else it will try and establish the current target not allowing for a new target to be selected first </param>
        private IEnumerator EstablishTargets(ABC_IEntity Originator, bool InterruptCastOnIncorrectTarget = true, bool ValidateTarget = false) {


            // loop until we have selected the right target or interrupted the ability 
            while (this.AbilityActivationInterrupted() == false) {

                if (this.abilityBeforeTarget || ValidateTarget == true && this.SetAbilityTargets(Originator) == false) {

                    //if ability before then we need to wait for originator to pick a new target 
                    yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.WaitForNewOriginatorTarget(Originator));

                }


                //If the target has been set and Originator is in range and is facing the right way and ability has not been interrupted then we can break out the while loop  
                if (this.SetAbilityTargets(Originator) && this.OriginatorInRangeOfTarget(Originator) && this.OriginatorFacingTarget(Originator) && this.AbilityActivationInterrupted() == false)
                    break;


                //correct conditions have not been reached - if we are interrupting on incorrect target and we are not looping till target found then we can interrupt the ability and end here
                if (InterruptCastOnIncorrectTarget == true && (this.abilityBeforeTarget == false || (this.abilityBeforeTarget == true && this.loopTillTargetFound == false))) {

                    this.InterruptAbilityActivation(Originator);

                    // exit while loop 
                    break;


                }


                // If we haven't breaked out the while loop at this point then we should be looping till target found

            }




        }


        /// <summary>
        /// Will return the abilities preparation time (how long it takes to activate the ability), taking into account the originators global adjustments
        /// </summary>
        /// <returns>Float value defining how long the ability will take to cooldown</returns>
        private float GetAbilityPrepareTime() {

            //calculated by returning a percentage of the abilities prepare time. The percentage is dependant on the current prepare time adjustment 
            //i.e 100 % of ability prepare time 12 will return 12, 50% ability prepare time would return 6
            return this.prepareTime / 100 * this.abilityCurrentPrepareTimeAdjustment;

        }


        /// <summary>
        /// Starts preparing the ability 
        /// </summary>
        /// <remarks>
        /// Part of the ability activation will set the originator into preparation mode activating animations and graphics portraying that the ability is being 'casted'
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator StartPreparing(ABC_IEntity Originator) {

            if (this.AbilityActivationInterrupted())
                yield break;

            // get vector3 position of where we started casting (so we can tell if we ended up on this spot)
            Vector3 preparingStartPosition = Originator.transform.position;

            //Record the originators current global prepare time adjustment unless its set to ignore prepare time adjustments
            if (this.ignoreGlobalPrepareTimeAdjustments == false)
                this.abilityCurrentPrepareTimeAdjustment = Originator.GetAbilityGlobalPrepareTimeAdjustment();
            else
                this.abilityCurrentPrepareTimeAdjustment = 100f;


            // if a projectile to starting position setting has been set then call the method (unless graphic has not been entered)
            if (this.useProjectileToStartPosition == true && this.projectileToStartType == ProjectileToStartType.Preparing && (this.projToStartPosGraphic.GameObject != null || this.useOriginalProjectilePTS == true)) {

                //Send projectile to original starting position first 
                ABC_Utilities.mbSurrogate.StartCoroutine(this.SendProjectileToStartPosition(Originator, this.GetStartingPositionObject(Originator, AbilityStartPositionType.StartPosition), this.startingPositionOffset, this.startingPositionForwardOffset, this.startingPositionRightOffset, ABC_Utilities.ModifyTimeByPercentage(abilityCurrentPrepareTimeAdjustment, this.projToStartDelay)));

                //Send projectile to additional starting positions
                foreach (AdditionalStartingPosition addStartPosition in this.additionalStartingPositions)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.SendProjectileToStartPosition(Originator, this.GetStartingPositionObject(Originator, AbilityStartPositionType.AdditionalStartPosition, addStartPosition), addStartPosition.startingPositionOffset, addStartPosition.startingPositionForwardOffset, addStartPosition.startingPositionRightOffset, ABC_Utilities.ModifyTimeByPercentage(abilityCurrentPrepareTimeAdjustment, this.projToStartDelay)));
            }

            // rotate originator to target, if they are preparing attack they want to face their target for realism
            RotateOriginatorToTarget(Originator);

            //Record the abilities prepare time
            float abilityPrepareTime = this.GetAbilityPrepareTime();

            // if preparing time is enabled (value higher then 0)
            if (abilityPrepareTime > 0) {

#if ABC_GC_Integration
            //Play GC action 
            if (this.gcPreparingActionList != null)
                this.gcPreparingActionList?.Execute(Originator.gameObject, null);
#endif

#if ABC_GC_2_Integration
            //Play GC action 
            if (this.gc2PreparingAction != null)
                Originator.RunGC2Action(this.gc2PreparingAction, new GameCreator.Runtime.Common.Args(Originator.gameObject, this.target));                
#endif


                // track preparing graphic incase we need to delete it later on
                GameObject preparingObj = null;


                // get object and particle if they exist
                if (this.usePreparingAesthetics == true) {

                    //If set to true along this process then preparing animation will be skipped
                    bool skipPreparingAnimation = false;

                    // stop originator from movement when initiating
                    if (this.stopMovementOnPreparing)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleOriginatorsMovement(Originator, false, this.stopMovementOnPreparingFreezePosition, stopMovementOnPreparingDisableComponents, this.ModifyTimeByInitiatingAdjustments(Originator, this.stopMovementOnPreparingDuration)));

                    // If set then raise the stop originator movement event
                    if (this.stopMovementOnPreparingRaiseEvent)
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, false, this.ModifyTimeByInitiatingAdjustments(Originator, this.stopMovementOnPreparingDuration)));


                    // are we defying gravity 
                    if (this.defyGravityPreparing == true && this.defyGravityPreparingDuration > 0) {
                        // stop any previous defy gravity (stop any seperate threads from turning gravity back on at wrong time)
                        ABC_Utilities.mbSurrogate.StopCoroutine("DefyOriginatorGravity");
                        ABC_Utilities.mbSurrogate.StartCoroutine(Originator.DefyOriginatorGravity(this.ModifyTimeByInitiatingAdjustments(Originator, this.defyGravityPreparingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.defyGravityPreparingDelay), this.defyGravityPreparingRaiseEvent));

                    }


                    // move originator whilst preparing?
                    if (this.moveSelfWhenPreparing == true)
                        ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfByOffset(this.moveSelfPreparingOffset, this.moveSelfPreparingForwardOffset, this.moveSelfPreparingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfPreparingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfPreparingDelay)));

                    //Move to Target  
                    if (this.moveSelfToTargetWhenPreparing == true) {

                        //Store current target of the ability (if selected target, melee etc)
                        GameObject moveSelfTarget = target;

                        //If null then this is a non target tracking ability (travel type forward etc), so just record current target
                        if (moveSelfTarget == null)
                            moveSelfTarget = Originator.target;

                        //If current target doesn't exist settle for soft target 
                        if (moveSelfTarget == null)
                            moveSelfTarget = Originator.softTarget;

                        //If target still doesn't exist and rotate behaviour is on mouse then use that
                        if (moveSelfTarget == null && this.noTargetRotateBehaviour == AbilityNoTargetRotateBehaviour.MousePosition) {
                            ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfToPosition(GetMousePosition(Originator), this.moveSelfToTargetPreparingOffset, this.moveSelfToTargetPreparingForwardOffset, this.moveSelfToTargetPreparingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetPreparingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetPreparingDelay)));
                        } else if (moveSelfTarget != null && (Vector3.Distance(Originator.transform.position, moveSelfTarget.transform.position) > this.moveSelfToTargetPreparingStopDistance + 0.5f)) {
                            //if further away then stop distance and we have a target then move to it 
                            ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfToObject(moveSelfTarget, this.moveSelfToTargetPreparingStopDistance, this.moveSelfToTargetPreparingOffset, this.moveSelfToTargetPreparingForwardOffset, this.moveSelfToTargetPreparingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetPreparingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetPreparingDelay)));
                        } else {
                            //we are already within distance so no need to move to target but but do we want to skip animation if not moving to target?
                            if (this.moveSelfToTargetActivatePreparingAnimationOnlyWhenMoving == true)
                                skipPreparingAnimation = true;
                        }

                    }


                    // activate preparing graphic
                    preparingObj = ActivateGraphic(Originator, AbilityGraphicType.Preparing);


                    //start preparing animation if not been skipped
                    if (skipPreparingAnimation == false) {
                        if (this.preparingAnimationRunnerOnEntity)
                            this.StartAnimationRunner(AbilityAnimationState.Preparation, Originator.animationRunner);

                        if (this.preparingAnimationRunnerOnScrollGraphic)
                            this.StartAnimationRunner(AbilityAnimationState.Preparation, GetCurrentScrollAbilityAnimationRunner(Originator));

                        if (this.preparingAnimationRunnerOnWeapon)
                            Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.StartAnimationRunner(AbilityAnimationState.Preparation, ar));


                        // start animation
                        if (this.preparingAnimateOnEntity)
                            this.StartAnimation(AbilityAnimationState.Preparation, Originator.animator);


                        //If enabled then activate the animation on the graphic object
                        if (this.preparingAnimateOnScrollGraphic)
                            this.StartAnimation(AbilityAnimationState.Preparation, GetCurrentScrollAbilityAnimator(Originator));


                        if (this.preparingAnimateOnWeapon)
                            Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.StartAnimation(AbilityAnimationState.Preparation, a));
                    }
                }

                // start the GUI handler 
                ABC_Utilities.mbSurrogate.StartCoroutine(ActivatePreparingGUI(Originator));

                Originator.AddToDiagnosticLog("Preparing to activate " + this.name);

                if (Originator.LogInformationAbout(LoggingType.Preparing))
                    Originator.AddToAbilityLog("Preparing " + this.name);

                //Determine time ability started preparing
                this.abilitySecondsPrepared = Time.time;

                //Should we hold preparation?
                bool holdPreparation = false;

                if (this.IgnoreHoldPreparation == false)
                    holdPreparation = this.prepareTriggerHoldRequied;

                for (var i = abilityPrepareTime; i > 0 || holdPreparation == true; i = i - 0.2f) {


                    // actual wait time for preparation
                    if (i < 0.2f) {
                        // less then 0.2  so we only need to wait the .xx time
                        yield return new WaitForSeconds(i);
                    } else {
                        // wait a second and keep looping till casting time = 0; 
                        yield return new WaitForSeconds(0.2f);
                    }


                    // if we moved when we shouldn't or ability is interrupted then stop preparing
                    if (this.moveInteruptPreparation == true && Vector3.Distance(preparingStartPosition, Originator.transform.position) > this.distanceInteruptPreparation || this.AbilityActivationInterrupted()) {

                        // stop preparing as we are not meant to move or the ability has been interrupted.
                        this.InterruptAbilityActivation(Originator);


                        // distance is too big casting is interupted
                        Originator.AddToDiagnosticLog("Preparation interrupted " + this.name + " will not activate");

                        if (Originator.LogInformationAbout(LoggingType.AbilityInterruption))
                            Originator.AddToAbilityLog("Preparation interrupted " + this.name + " will not activate");


                        // destroy the preparing graphic early 
                        if (preparingObj != null)
                            ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(preparingObj));


                        // if we are moving self when preparing we need to stop this now
                        if (this.moveSelfWhenPreparing == true) {
                            Originator.StopMoveSelf();
                        }


                        // we have moved break loop; 
                        break;

                    }

                    // If a button hold is required to continue preparing and it's not held then end preparing early 
                    if (holdPreparation == true && Time.time - this.abilitySecondsPrepared > 0.3f) {

                        if (this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Hold) == false) {
                            holdPreparation = false;
                            i = 0;
                        } else if (i <= 0.2f) {
                            i = 0.2f;
                        }
                    }

                }

                //work out seconds ability was preparing for 
                this.abilitySecondsPrepared = Time.time - this.abilitySecondsPrepared;

                Originator.AddToDiagnosticLog(this.name + " was preparing for: " + abilitySecondsPrepared + " seconds");

                if (Originator.LogInformationAbout(LoggingType.Preparing))
                    Originator.AddToAbilityLog("Preparing " + this.name);


                // turn off animation
                if (this.usePreparingAesthetics == true) {

                    //end preparing animation
                    if (this.preparingAnimationRunnerOnEntity)
                        this.EndAnimationRunner(AbilityAnimationState.Preparation, Originator.animationRunner);

                    if (this.preparingAnimationRunnerOnScrollGraphic)
                        this.EndAnimationRunner(AbilityAnimationState.Preparation, GetCurrentScrollAbilityAnimationRunner(Originator));

                    if (this.preparingAnimationRunnerOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimationRunners().ForEach(ar => this.EndAnimationRunner(AbilityAnimationState.Preparation, ar));


                    if (this.preparingAnimateOnEntity)
                        this.EndAnimation(AbilityAnimationState.Preparation, Originator.animator);

                    //If enabled then activate the animation on the graphic object
                    if (this.preparingAnimateOnScrollGraphic)
                        this.EndAnimation(AbilityAnimationState.Preparation, GetCurrentScrollAbilityAnimator(Originator));

                    if (this.preparingAnimateOnWeapon)
                        Originator.GetCurrentEquippedWeaponAnimators().ForEach(a => this.EndAnimation(AbilityAnimationState.Preparation, a));


                    // cancel the stop movement if we did not use a duration (else it would have been stopped earlier with the duration) unless the ability has been interrupted 
                    if (this.stopMovementOnPreparing && (this.stopMovementOnPreparingDuration == 0f || this.AbilityActivationInterrupted()))
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleOriginatorsMovement(Originator, true, this.stopMovementOnPreparingFreezePosition, stopMovementOnPreparingDisableComponents));


                    // If set then raise then enable originator movement event if we did not use a duration (else it would have been stopped earlier with the duration) unless the ability has been interrupted 
                    if (this.stopMovementOnPreparingRaiseEvent && (this.stopMovementOnPreparingDuration == 0f || this.AbilityActivationInterrupted()))
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, true));

                }

            }


        }

        /// <summary>
        /// Activates a GUI displaying the current progress of the ability preparation.
        /// </summary>
        /// <remarks>
        /// Retrieves Preparation Text and Slider objects from the originator entity, filling the Text object with "Activating Ability" and incrementing the Slider to represent how long the ability is preparing for 
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability which is currently preparing</param>
        IEnumerator ActivatePreparingGUI(ABC_IEntity Originator) {

            //Record the abilities prepare time
            float abilityPrepareTime = this.GetAbilityPrepareTime();
            float activeTime = 0;

            if (this.showPrepareTimeOnGUI == true) {

                ABC_TextReference textGUI = Originator.preparingGUIText;
                Slider barGUI = Originator.preparingGUIBar;

                if (textGUI.Graphic != null) {
                    textGUI.text = this.name;
                    textGUI.Graphic.gameObject.SetActive(true);
                    textGUI.Graphic.enabled = true;

                }

                if (barGUI != null) {
                    // activate the gui but lower max value a tiny bit so the bar finishes just before spell fires
                    barGUI.maxValue = 100;
                    barGUI.value = 0;

                    barGUI.gameObject.SetActive(true);
                    barGUI.enabled = true;

                    //fill up bar gradually until the prepare time has been reached or ability has been interrupted
                    while (barGUI.value < 100) {

                        if (barGUI != null) {

                            float currentPercent = 0;

                            activeTime += Time.fixedDeltaTime;
                            currentPercent = activeTime / abilityPrepareTime;

                            barGUI.value = Mathf.Lerp(0, 100, currentPercent);
                        }

                        yield return new WaitForFixedUpdate();

                        //If we are preparing while trigger is held and button is held down then keep bar value just below 100, else if released end the loop
                        if (this.prepareTriggerHoldRequied == true && this.IgnoreHoldPreparation == false) {
                            if (this.ButtonPressed(Originator, AbilityButtonPressState.AbilityTrigger, AbillityButtonPressType.Hold) == true) {
                                if (barGUI.value >= 99)
                                    barGUI.value = 99.99f;
                            } else {
                                break;
                            }
                        }



                        if (this.AbilityActivationInterrupted() == true) {
                            if (textGUI != null) {
                                textGUI.text = "Activation interrupted!!";
                            }
                            break;
                        }
                    }
                }



                yield return new WaitForSeconds(0.5f);

                if (barGUI != null) {
                    barGUI.gameObject.SetActive(false);
                    barGUI.enabled = false;
                }

                if (textGUI != null && textGUI.Graphic != null) {
                    textGUI.Graphic.enabled = false;
                    textGUI.Graphic.gameObject.SetActive(false);
                }
            }



        }

        /// <summary>
        /// Will return the abilities initiating base speed adjustment taking into account any modifications
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>Float value of the ability initiation base speed</returns>
        private float GetAbilityInitiatingBaseSpeedAdjustment(ABC_IEntity Originator) {

            //Get base speed adjustment for the ability
            float retVal = this.abilityInitiatingBaseSpeedAdjustment;

            //If modifying by a stat
            if (this.modifyAbilityInitiatingBaseSpeedByStat == true) {

                //Retrieve the stat modification value from the originator(uses the percentage configured - 70% of intelligence etc)
                float statModification = this.abilityInitiatingBaseSpeedStatModification.percentageValue / 100 * Originator.GetStatValue(this.abilityInitiatingBaseSpeedStatModification.statName, this.abilityInitiatingBaseSpeedStatModification.statIntegrationType);

                //Apply the modification to the potency depending on the operator setup
                switch (this.abilityInitiatingBaseSpeedStatModification.arithmeticOperator) {
                    case ArithmeticOperators.Add:
                        retVal += statModification;
                        break;
                    case ArithmeticOperators.Minus:
                        retVal -= statModification;
                        break;
                    case ArithmeticOperators.Divide:
                        retVal /= statModification;
                        break;
                    case ArithmeticOperators.Multiply:
                        retVal *= statModification;
                        break;
                }

            }

            //Return the result
            return retVal;


        }

        /// <summary>
        /// Will modify the value by the abilities initiating base speed adjustment first and then by the entities global initiating speed adjustment (applied by effects, slow etc)
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="Value">Value to modify by the initating adjustments</param>
        /// <returns>Modified time value after applying the ability base initiating adjustment and then the originators global initiating speed adjustment</returns>
        private float ModifyTimeByInitiatingAdjustments(ABC_IEntity Originator, float Value) {

            float retVal = Value;

            //Modify base value
            retVal = ABC_Utilities.ModifyTimeByPercentage(this.GetAbilityInitiatingBaseSpeedAdjustment(Originator), retVal);

            //Then modify value by global adjustment 
            retVal = ABC_Utilities.ModifyTimeByPercentage(Originator.GetAbilityGlobalInitiationSpeedAdjustment(), retVal);

            //Return final value
            return retVal;


        }

        /// <summary>
        ///  starts initiating the ability
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator StartInitiating(ABC_IEntity Originator) {

            if (this.AbilityActivationInterrupted())
                yield break;

            //if not ignoring initiation speed adjustment then track the initiation speed adjustment the originator had when this was initiated
            if (this.ignoreGlobalInitiatingSpeedAdjustments == false)
                this.abilityCurrentInitiationSpeedAdjustment = Originator.GetAbilityGlobalInitiationSpeedAdjustment();
            else
                this.abilityCurrentInitiationSpeedAdjustment = 100f;


            // if our current ability target is now a surrounding object then we need to find a new target 
            if (this.includeSurroundingObject == true && surroundingObjects.Contains(this.target) && this.surroundingObjectTarget == true && (this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.NearestTag || this.startingPosition == StartingPosition.Target)) {
                //remove ability target
                target = null;
                //remove originators target
                Originator.RemoveTarget();
                //find new target 
                yield return ABC_Utilities.mbSurrogate.StartCoroutine(EstablishTargets(Originator, false, true));

            }

            // if waiting for a key before moving forward or a new target is required (makes use of waiting for key so user can select a target then press the wait for initiating key) 
            if (this.waitForKeyBeforeInitiating == true)
                yield return ABC_Utilities.mbSurrogate.StartCoroutine(WaitForInitiatingKey(Originator));

            //Start the initiating aesthetics (animations, graphics, movement)
            ABC_Utilities.mbSurrogate.StartCoroutine(ActivateInitiatingAesthetics(Originator));

        }

        /// <summary>
        ///  Activates the initiating aesthetics including animations, graphics and originator movement
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator ActivateInitiatingAesthetics(ABC_IEntity Originator) {

            if (this.AbilityActivationInterrupted())
                yield break;


            // if a projectile to starting position setting has been set then call the method (unless graphic has not been entered)
            if (this.useProjectileToStartPosition == true && this.projectileToStartType == ProjectileToStartType.Initiating && (this.projToStartPosGraphic.GameObject != null || this.useOriginalProjectilePTS == true)) {

                //Send projectile to original starting position first 
                ABC_Utilities.mbSurrogate.StartCoroutine(this.SendProjectileToStartPosition(Originator, this.GetStartingPositionObject(Originator, AbilityStartPositionType.StartPosition), this.startingPositionOffset, this.startingPositionForwardOffset, this.startingPositionRightOffset, ABC_Utilities.ModifyTimeByPercentage(abilityCurrentInitiationSpeedAdjustment, this.projToStartDelay)));

                //Send projectile to additional starting positions
                foreach (AdditionalStartingPosition addStartPosition in this.additionalStartingPositions)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.SendProjectileToStartPosition(Originator, this.GetStartingPositionObject(Originator, AbilityStartPositionType.AdditionalStartPosition, addStartPosition), addStartPosition.startingPositionOffset, addStartPosition.startingPositionForwardOffset, addStartPosition.startingPositionRightOffset, ABC_Utilities.ModifyTimeByPercentage(abilityCurrentInitiationSpeedAdjustment, this.projToStartDelay)));
            }

            // track initiating graphic incase we need to delete it later on
            foreach (GameObject Obj in this.initiatingObjs) {
                if (Obj != null && Obj.activeInHierarchy == true)
                    ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(Obj));
            }
            this.initiatingObjs.Clear();


            // are we using initiating aesthetics?
            if (this.useInitiatingAesthetics == true && this.AbilityActivationInterrupted() == false) {
                Originator.AddToDiagnosticLog("Initiating " + this.name);

                // Rotate originator to face our target or where we are firing (Includes auotmatically facing camera centre) 
                RotateOriginatorToTarget(Originator);



                //track the animation runner/animator duration to be used in this method
                float animationRunnerDuration = (this.initiatingAnimationRunnerClip.AnimationClip != null ? this.initiatingAnimationRunnerClipDuration : 0f) + this.initiatingAnimationRunnerClipDelay;
                float animatorDuration = (this.initiatingAnimatorParameter != string.Empty ? this.initiatingAnimatorDuration : 0f);

                //Record the stop movement duration (if 0 then take the animation duration + buffer)
                float stopMovementDuration = this.stopMovementOnInitiateDuration > 0 ? this.stopMovementOnInitiateDuration : Mathf.Max(animationRunnerDuration, animatorDuration) + 0.07f;

                //If this is a toggle hold or on/off and initiation duration is 0 then stop movement until ability ends
                if (this.abilityToggle != AbilityToggle.Off && this.stopMovementOnInitiateDuration == 0)
                    stopMovementDuration = 0;

                // stop originator from movement when initiating
                if (this.stopMovementOnInitiate)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleOriginatorsMovement(Originator, false, this.stopMovementOnInitiateFreezePosition, this.stopMovementOnInitiateDisableComponents, this.ModifyTimeByInitiatingAdjustments(Originator, stopMovementDuration)));

                // If set then raise the stop originator movement event
                if (this.stopMovementOnInitiateRaiseEvent)
                    ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, false, this.ModifyTimeByInitiatingAdjustments(Originator, stopMovementDuration)));


                // are we defying gravity 
                if (this.defyGravityInitiating == true && this.defyGravityInitiatingDuration > 0) {
                    // stop previous defy gravity (preparing gravity) as we are no longer preparing and now initating
                    ABC_Utilities.mbSurrogate.StopCoroutine("DefyOriginatorGravity");
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.DefyOriginatorGravity(this.ModifyTimeByInitiatingAdjustments(Originator, this.defyGravityInitiatingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.defyGravityInitiatingDelay), this.defyGravityInitiatingRaiseEvent));

                }

                // if no root motion on animation this allows us to move the object by code

                //Move by offset
                if (this.moveSelfWhenInitiating == true)
                    ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfByOffset(this.moveSelfInitiatingOffset, this.moveSelfInitiatingForwardOffset, this.moveSelfInitiatingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfInitiatingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfInitiatingDelay)));


                //Move to Target  
                if (this.moveSelfToTargetWhenInitiating == true) {

                    //Store current target of the ability (if selected target, melee etc)
                    GameObject moveSelfTarget = target;

                    //If null then this is a non target tracking ability (travel type forward etc), so just record current target
                    if (moveSelfTarget == null)
                        moveSelfTarget = Originator.target;

                    //If current target doesn't exist settle for soft target 
                    if (moveSelfTarget == null)
                        moveSelfTarget = Originator.softTarget;

                    //If target still doesn't exist and rotate behaviour is on mouse then use that
                    if (moveSelfTarget == null && this.noTargetRotateBehaviour == AbilityNoTargetRotateBehaviour.MousePosition) {

                        Vector3 mousePosition = GetMousePosition(Originator);

                        ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfToPosition(new Vector3(mousePosition.x, Originator.transform.position.y, mousePosition.z), this.moveSelfToTargetInitiatingOffset, this.moveSelfToTargetInitiatingForwardOffset, this.moveSelfToTargetInitiatingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetInitiatingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetInitiatingDelay)));

                    } else if (moveSelfTarget != null && (Vector3.Distance(Originator.transform.position, moveSelfTarget.transform.position) > this.moveSelfToTargetInitiatingStopDistance + 0.5f)) {

                        //else move to target 
                        ABC_Utilities.mbSurrogate.StartCoroutine(Originator.MoveSelfToObject(moveSelfTarget, this.moveSelfToTargetInitiatingStopDistance, this.moveSelfToTargetInitiatingOffset, this.moveSelfToTargetInitiatingForwardOffset, this.moveSelfToTargetInitiatingRightOffset, this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetInitiatingDuration), this.ModifyTimeByInitiatingAdjustments(Originator, this.moveSelfToTargetInitiatingDelay)));
                    }


                }


                //Activate initiating graphic if not melee or is melee but graphic is not activating with ability
                if (this.abilityType != AbilityType.Melee || this.abilityType == AbilityType.Melee && this.initiatingAestheticActivateWithAbility == false) {
                    // get object and particle if they exist
                    this.initiatingObjs.Add(ActivateGraphic(Originator, AbilityGraphicType.Initiating));

                    for (int i = 0; i < this.additionalStartingPositions.Count; i++) {
                        if (this.additionalStartingPositions[i].repeatInitiatingGraphic == true)
                            this.initiatingObjs.Add(ActivateGraphic(Originator, AbilityGraphicType.InitiatingAdditionalStartingPosition, false, i));
                    }
                }


                //List of runners that will be running clips
                List<ABC_AnimationsRunner> animationRunners = new List<ABC_AnimationsRunner>();

                //collect animation runners that will run the clip
                if (this.initiatingAnimationRunnerOnEntity)
                    animationRunners.Add(Originator.animationRunner);

                if (this.initiatingAnimationRunnerOnScrollGraphic)
                    animationRunners.Add(GetCurrentScrollAbilityAnimationRunner(Originator));

                if (this.initiatingAnimationRunnerOnWeapon)
                    animationRunners.AddRange(Originator.GetCurrentEquippedWeaponAnimationRunners());

                //Start animation on each runner being used
                foreach (ABC_AnimationsRunner runner in animationRunners)
                    this.StartAnimationRunner(AbilityAnimationState.Initiate, runner);


                //List of runners that will be running clips
                List<Animator> animators = new List<Animator>();

                //collect animation runners that will run the clip
                if (this.initiatingAnimateOnEntity)
                    animators.Add(Originator.animator);

                //If enabled then activate the animation on the graphic object
                if (this.initiatingAnimateOnScrollGraphic)
                    animators.Add(GetCurrentScrollAbilityAnimator(Originator));

                if (this.initiatingAnimateOnWeapon)
                    animators.AddRange(Originator.GetCurrentEquippedWeaponAnimators());

                //Start animation on each animator being used
                foreach (Animator ani in animators)
                    this.StartAnimation(AbilityAnimationState.Initiate, ani);


                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated animator speed modification or this ability was activated again before finishing it's initial animation
                //this function would not continue after duration
                float functionRequestTime = Time.time;



                // If the ability is not a toggle type, or is a toggle type but can cast other abilities or is not repeating initiating animation then end animation after duration
                if (this.abilityToggle == AbilityToggle.Off || this.abilityToggle != AbilityToggle.Off && (this.canCastWhenToggled == true || this.repeatInitiatingAnimationWhilstToggled == false)) {


                    //Get base speed of initiating animation
                    float baseAnimatorSpeed = ABC_Utilities.PercentageOfValue(this.GetAbilityInitiatingBaseSpeedAdjustment(Originator), 1);

                    //Modify animator speed by global initiation speed
                    Originator.ModifyAnimatorSpeed(functionRequestTime, ABC_Utilities.PercentageOfValue(this.abilityCurrentInitiationSpeedAdjustment, baseAnimatorSpeed));

                    //Determines when the animation has ended (If no animation has been setup then it will default to already being completed
                    bool animationRunnerComplete = this.initiatingAnimationRunnerClip.AnimationClip != null ? false : true;
                    bool animatorComplete = this.initiatingAnimatorParameter != string.Empty ? false : true;

                    animationRunnerDuration = this.ModifyTimeByInitiatingAdjustments(Originator, animationRunnerDuration);
                    animatorDuration = this.ModifyTimeByInitiatingAdjustments(Originator, animatorDuration);


                    //Work out which duration is higher
                    float maxAnimationDuration = Mathf.Max(animationRunnerDuration, animatorDuration);


                    //Loop through to cancel animations early if ability interuptted 
                    for (var i = maxAnimationDuration; i > 0;) {

                        // if ability is interrupted then we are done with this so time out the durations
                        if (this.AbilityActivationInterrupted()) {
                            animationRunnerDuration = 0;
                            animatorDuration = 0;
                            i = 0;
                        } else {
                            //Wait for seconds
                            if (Mathf.Min(animationRunnerDuration, animatorDuration) > 0 && Mathf.Min(animationRunnerDuration, animatorDuration) < 0.2f) {
                                // less then 0.2f second so we only need to wait the .xx time
                                yield return new WaitForSeconds(Mathf.Min(animationRunnerDuration, animatorDuration));
                            } else if (i < 0.2f) {
                                // less then  0.2f so we only need to wait the .xx time
                                yield return new WaitForSeconds(i);
                            } else {
                                // wait 0.2f and keep looping till casting time = 0; 
                                yield return new WaitForSeconds(0.2f);
                            }
                        }



                        //If ability is currently in hitstop then don't reduce from the duration tracker as animations are currently frozen (hitstop happening)
                        if (this.hitStopCurrentlyActive == true) {

                            //Wait for hit stop to end
                            while (this.hitStopCurrentlyActive == true) {
                                yield return new WaitForSeconds(0.2f);
                            }

                            //small breath
                            yield return new WaitForEndOfFrame();


                        } else {

                            //Else reduce duration tracked in loop
                            animationRunnerDuration -= 0.2f;
                            animatorDuration -= 0.2f;
                            i = i - 0.2f;

                        }

                        //If ability has already been called again before this could finish then end here as we don't want to end animations in the middle of the next version of this ability that activated (Rapid fire)
                        if (functionRequestTime < this.abilityActivationTime)
                            yield break;

                        //If animation runner duration has ended (reached 0) then end animations
                        if (animationRunnerDuration <= 0 && animationRunnerComplete == false) {
                            animationRunnerComplete = true;

                            //end animation on each runner being used
                            foreach (ABC_AnimationsRunner runner in animationRunners)
                                this.EndAnimationRunner(AbilityAnimationState.Initiate, runner);

                        }

                        //If animator duration has ended (reached 0) then end animations
                        if (animatorDuration <= 0 && animatorComplete == false) {
                            animatorComplete = true;


                            //end animation on each animator being used
                            foreach (Animator ani in animators)
                                this.EndAnimation(AbilityAnimationState.Initiate, ani);

                        }


                    }


                    // cancel the stop movement if the ability has been interrupted 
                    if (this.AbilityActivationInterrupted())
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.ToggleOriginatorsMovement(Originator, true, this.stopMovementOnInitiateFreezePosition, stopMovementOnInitiateDisableComponents));


                    // cancel the stop movement event if the ability has been interrupted 
                    if (this.AbilityActivationInterrupted())
                        ABC_Utilities.mbSurrogate.StartCoroutine(this.RaiseOriginatorsToggleMovementEvent(Originator, true));


                    //Revert animator speed back to normal after small breathing space
                    yield return new WaitForSeconds(0.1f);

                    Originator.ModifyAnimatorSpeed(functionRequestTime, 1);

                    //restore any weapon IK unless IK is already enabled (IK is set to persist)
                    if (this.persistIK == false)
                        ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ToggleIK(this.abilityActivationTime, true, 0.3f));

                }




            } // end of use initiating Aesthetic if statement


            if (this.AbilityActivationInterrupted()) {

                // destroy the initiating graphic early 
                foreach (GameObject obj in this.initiatingObjs) {
                    ABC_Utilities.mbSurrogate.StartCoroutine(DestroyObject(obj));
                }


                if (Originator.LogInformationAbout(LoggingType.AbilityInterruption))
                    Originator.AddToAbilityLog("initiating interupted " + this.name + " will not fire");

                // if we are moving self when initiating we need to stop this now
                if (this.moveSelfWhenInitiating == true)
                    Originator.StopMoveSelf();

            }



        }

        /// <summary>
        /// Will suspend the ability activation during the initiating stage and won't continue until the key setup has been triggered. Method will also restablish any new targets selected whilst waiting for the trigger.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator WaitForInitiatingKey(ABC_IEntity Originator) {
            // turn on our wait variable. This aids in our while loop so we wait till the wait key is pressed
            bool waitingOnKeyForInitiating = true;



            // we need to wait for 0.1 of a second otherwise it will just fire in the same tick (as getkeydown will still be active)
            yield return new WaitForSeconds(0.1f);

            // we now need to wait for the amount of time entered in settings before the second key press is recognised (lets graphic reach end destination if required)
            yield return new WaitForSeconds(this.waitForKeyBeforeInitiatingDelay);


            while (waitingOnKeyForInitiating == true) {

                // if waitBeforeKey is pressed (in key press mode) or key is pressed off (in key hold mode)  or casting has been interrupted 
                if (this.AbilityActivationInterrupted() == true || this.onKeyPress == true && this.ButtonPressed(Originator, AbilityButtonPressState.WaitBeforeInitiating)
                    || this.onKeyDown == true && this.ButtonPressed(Originator, AbilityButtonPressState.WaitBeforeInitiating, AbillityButtonPressType.Hold) == false) {


                    // do we allow change of target?
                    if (this.waitForKeyAllowChangeOfTarget == true && (this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.ToWorld)) {

                        yield return ABC_Utilities.mbSurrogate.StartCoroutine(EstablishTargets(Originator, false));
                    }

                    // stop waiting for key if casting is interuptted or we don't care about the ability target object 
                    if ((target == null && (this.travelType == TravelType.SelectedTarget || this.startingPosition == StartingPosition.Target)) == false || this.AbilityActivationInterrupted() == true) {
                        // turn off our wait variable as correct key has been pressed
                        waitingOnKeyForInitiating = false;
                    } else {


                        // still waiting on new target 
                        if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                            Originator.AddToAbilityLog("Please select new target");
                    }

                } else {

                    // keep waiting
                    yield return null;

                }
            }


        }


        /// <summary>
        /// Initiates the ability by creating and activating the projectile object or sending out the raycast
        /// </summary>
        /// <param name="Originator"></param>
        private IEnumerator InitiateAbility(ABC_IEntity Originator) {


            // modify game speed if set too 
            // and global impact required tags is 0 or is above 0 and the originator has the tag required to activate global impacts
            if (this.modifyGameSpeedOnInitiation == true && (this.globalImpactRequiredTag.Count == 0 || this.globalImpactRequiredTag.Count > 0 && ABC_Utilities.ObjectHasTag(Originator.gameObject, ABC_Utilities.ConvertTags(Originator, this.globalImpactRequiredTag))))
                ABC_Utilities.mbSurrogate.StartCoroutine(ABC_Utilities.ModifyGameSpeed(this.modifyGameSpeedOnInitiationSpeedFactor, this.modifyGameSpeedOnInitiationDuration, this.modifyGameSpeedOnInitiationDelay));

            // shake camera if set too 
            // and global impact required tags is 0 or is above 0 and the originator has the tag required to activate global impacts
            if (this.shakeCameraOnInitiation == true && (this.globalImpactRequiredTag.Count == 0 || this.globalImpactRequiredTag.Count > 0 && ABC_Utilities.ObjectHasTag(Originator.gameObject, ABC_Utilities.ConvertTags(Originator, this.globalImpactRequiredTag))))
                ABC_Utilities.mbSurrogate.StartCoroutine(Originator.ShakeCamera(this.shakeCameraOnInitiationDuration, this.shakeCameraOnInitiationAmount, this.shakeCameraOnInitiationSpeed, this.shakeCameraOnInitiationDelay));




            //Wait for either a delay or for the initiating animation to hit a certain percentage before moving on (to create the ability)
            switch (this.intiatingProjectileDelayType) {

                case AbilityInitiationDelayType.AtAnimationPercentage:

                    //Record animation runner and animator
                    ABC_AnimationsRunner aniRunner = Originator.animationRunner;
                    Animator animator = Originator.animator;

                    //If no animator then end here (Animation Runner requires this also)
                    if (animator == null)
                        break;

                    //Tracker for while loop
                    float animationProgressPercentage = 0;

                    while (animationProgressPercentage < 100) {

                        // if ability is interrupted then we are done with this loop
                        if (this.AbilityActivationInterrupted())
                            break;


                        //Prioritise animation runner clip progress if configured
                        if (this.initiatingAnimationRunnerClip.AnimationClip != null) {
                            animationProgressPercentage = aniRunner.GetCurrentAnimationProgress();
                        } else {
                            animationProgressPercentage = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 * 100;
                        }


                        //If progress is over the animation percentage then finish loop so ability object can now be created
                        if (animationProgressPercentage >= this.initiatingProjectileDelayAnimationPercentage) {
                            break;
                        }

                        // wait till end of frame and continue loop
                        yield return new WaitForEndOfFrame();

                    }

                    break;

                case AbilityInitiationDelayType.AfterDelay:

                    float delay = this.ModifyTimeByInitiatingAdjustments(Originator, this.delayBetweenInitiatingAndProjectile);

                    // wait before firing ability taking into account initiation speed
                    if (delay < 0.3) {
                        // if the delay is between 0-0.2 we don't need to check in a loop if casting was interuppted as its almost impossible.

                        yield return new WaitForSeconds(delay);

                    } else {
                        for (var i = delay; i > 0; i--) {

                            // if ability is interrupted then we are done with this loop
                            if (this.AbilityActivationInterrupted())
                                break;


                            if (i < 1) {
                                // less then 1 second so we only need to wait the .xx time
                                yield return new WaitForSeconds(i);
                            } else {
                                // wait a section and keep looping till casting time = 0; 
                                yield return new WaitForSeconds(1);
                            }

                        }
                    }

                    break;
            }


            //If ability has been interrupted end here
            if (this.AbilityActivationInterrupted())
                yield break;

            if (Originator.LogInformationAbout(LoggingType.Initiating))
                Originator.AddToAbilityLog("initiating " + this.name);

            // if a mana reference has been passed then reduce it by the manacost 
            Originator.AdjustMana(-this.manaCost);

            //adjust any stat cost if set too 
            if (this.statCost > 0)
                Originator.AdjustStatValue(this.statCostName, -this.statCost, this.statCostIntegrationType, GCStatType.Attribute);

            // ability is firing so lets reduce ammo if that is enabled
            if (this.UseAmmo == true) {

                //If linked to equipped weapon then reduce it's ammo
                if (this.useEquippedWeaponAmmo == true)
                    Originator.AdjustEquippedWeaponAmmo(-1);
                else
                    this.AdjustAmmo(-1, Originator); // else reduce abilities ammo
            }


            // make sure we can't cast this same spell again until x amount of seconds
            this.abilityOnCooldown = true;

            // we can't recast this ability again until recast time so lets invoke that (on the condition that were not starting recast after the ability has finished)
            // raycast has no projectile so we have no way to know when the ability has collided 
            if (this.startRecastAfterAbilityEnd == false || this.abilityType == AbilityType.RayCast) {
                ABC_Utilities.mbSurrogate.StartCoroutine(this.StartCooldown(Originator));
            }


            // Rotate originator to face our target or where we are firing (Includes auotmatically facing camera centre) 
            RotateOriginatorToTarget(Originator);


            // Set Combo Locks for Ability
            this.SetComboLock();

            if (Originator.LogInformationAbout(LoggingType.AbilityActivation))
                Originator.AddToAbilityLog("Activating " + this.name);

            Originator.AddToDiagnosticLog("Activating Ability " + this.name);


#if ABC_GC_Integration
        //Play GC action 
        if (this.gcInitiatingActionList != null)
            this.gcInitiatingActionList?.Execute(Originator.gameObject, null);
#endif

#if ABC_GC_2_Integration
        //Play GC action 
        if (this.gc2InitiatingAction != null)
            Originator.RunGC2Action(this.gc2InitiatingAction, new GameCreator.Runtime.Common.Args(Originator.gameObject, this.target));
#endif

            // if were raycasting and not sending a particle
            if (this.abilityType == AbilityType.RayCast) {
                this.InitiateRayCast(Originator);

            } else {
                yield return ABC_Utilities.mbSurrogate.StartCoroutine(this.InitiateProjectile(Originator));
            }

        }


        /// <summary>
        /// Initiates raycast abilities sending out a raycast and activating the effects on any objects hit that meet the correct conditions.
        /// </summary>
        /// <remarks>
        /// Method will check that the object hit can have effects applied before activating. 
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        private void InitiateRayCast(ABC_IEntity Originator) {

            if (this.travelType == TravelType.Self) {
                Originator.AddToDiagnosticLog("Initiating Raycast to self: " + Originator.gameObject.name);

                // if we are raycasting to self then just run the hit handler and then do normal ray for AOE
                ApplyAbilityEffectsToObject(Originator.gameObject, Originator, null, Originator.transform.position, false, false, this.abilitySecondsPrepared);
            }

            Ray ray;

            // initiate ray depending on spawn position or travel type - ray goes forward from all spawn positions unless travel type is crosshair then it follows crosshair
            if (this.travelType == TravelType.Crosshair) {

                //setup ray from camera to crosshair
                ray = Originator.Camera.ViewportPointToRay(Originator.crossHairPosition + this.selectedTargetOffset + this.GetMissChancePositionOffset(Originator));

                //If starting position is not from camera then cast to the crosshair using the ray declared above, grab the position of the hit and then create a new ray from starting point to the item hit
                if (this.startingPosition != StartingPosition.CameraCenter) {
                    RaycastHit castHit;
                    if (Physics.Raycast(ray, out castHit))
                        ray = new Ray(GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), castHit.point - GetStartingPosition(Originator, AbilityStartPositionType.StartPosition));
                }

            } else if (this.travelType == TravelType.MouseForward || this.travelType == TravelType.MouseTarget || this.travelType == TravelType.Mouse2D) {
                //setup ray from camera to mouse position
                ray = Originator.Camera.ScreenPointToRay(Originator.inputManager.GetMousePosition() + this.selectedTargetOffset + this.GetMissChancePositionOffset(Originator));

                //If starting position is not from camera then cast to the mouse position using the ray declared above, grab the position of the hit and then create a new ray from starting point to the item hit
                if (this.startingPosition != StartingPosition.CameraCenter) {
                    RaycastHit castHit;
                    if (Physics.Raycast(ray, out castHit))
                        ray = new Ray(GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), castHit.point - GetStartingPosition(Originator, AbilityStartPositionType.StartPosition));
                }

            } else if (this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.NearestTag) {
                ray = new Ray(GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), (GetTargetPosition(Originator) + this.GetMissChancePositionOffset(Originator)) - GetStartingPosition(Originator, AbilityStartPositionType.StartPosition));
            } else {
                //default to forward
                ray = new Ray(GetStartingPosition(Originator, AbilityStartPositionType.StartPosition), Originator.transform.forward + this.GetMissChancePositionOffset(Originator));
            }

            // ray cast out 
            List<RaycastHit> raycastHits = new List<RaycastHit>();

            //If raycast is not single here then do a wide spread sphere cast
            if (rayCastSingleHit == false)
                raycastHits.AddRange(Physics.SphereCastAll(ray, this.rayCastRadius, this.rayCastLength, this.affectLayer));

            //Single raycast
            RaycastHit hitSingle;
            if (Physics.Raycast(ray, out hitSingle, this.rayCastLength, this.affectLayer)) {

                if (rayCastSingleHit == true)
                    raycastHits.Add(hitSingle);

                //if terrain is in the hits then put a more accurate hit point in 
                if (Terrain.activeTerrain != null && raycastHits.Where(h => h.transform.gameObject == Terrain.activeTerrain.transform.gameObject).Count() > 0) {

                    //Have to create new object to update struct in a list
                    RaycastHit newHit = raycastHits.Where(h => h.transform.gameObject == Terrain.activeTerrain.transform.gameObject).FirstOrDefault();
                    newHit.point = hitSingle.point;
                    raycastHits[raycastHits.FindIndex(h => h.transform.gameObject == Terrain.activeTerrain.transform.gameObject)] = newHit;
                }

            }

            //If nothing was hit then we can stop method here
            if (raycastHits.Count == 0)
                return;

            //Due to spherecast not all items recorded will be ordered from nearest to furthest, this code will order so the closest hit is looked at first
            raycastHits = raycastHits.OrderBy(x => Vector3.Distance(ray.origin, x.point)).ToList();

            List<int> hits = new List<int>();

            for (int i = 0; i < raycastHits.Count; i++) {


                // if the list of hits has reached the raycast hit amount then finish loop
                if (raycastHitAmount > 0 && hits.Count == this.raycastHitAmount)
                    break;

                // lets  (ignores caster) and terrain and other abilities
                if (raycastHits[i].transform.IsChildOf(Originator.transform) || this.raycastIgnoreTerrain == true && Terrain.activeTerrain != null && raycastHits[i].transform.gameObject == Terrain.activeTerrain.transform.gameObject || raycastHits[i].transform.gameObject.name.Contains("ABC*_"))
                    continue;

                //activate ability end graphic before we check for statemanagers (this is done after we decide if we can ignore caster or terrain on purpose)
                this.ActivateGraphicAtPosition(AbilityPositionGraphicType.End, raycastHits[i].point);

                // check for state manager script (we only effect these types of objects)
                ABC_IEntity iEntity = ABC_Utilities.GetStaticABCEntity(raycastHits[i].transform.gameObject);

                //If statemanager is not found then either continue to next object or end if raycast can be blocked
                if (iEntity.HasABCStateManager() == false) {
                    if (this.raycastBlockable == false)
                        continue;
                    else // Raycast can be blocked by none statemanagers so end loop
                        break;
                }


                //If entity is ignoring ability collision (and this ability can't override that setting) then continue to next 
                if (iEntity.ignoringAbilityCollision == true && this.overrideIgnoreAbilityCollision == false)
                    continue;

                // if the object hit has a tag we are ignoring then continue
                if (abilityIgnoreTag.Count > 0 && ABC_Utilities.ObjectHasTag(raycastHits[i].transform.gameObject, ABC_Utilities.ConvertTags(Originator, abilityIgnoreTag)) == true)
                    continue;

                // if the object hit does not have a tag we require then continue
                if (abilityRequiredTag.Count > 0 && ABC_Utilities.ObjectHasTag(raycastHits[i].transform.gameObject, ABC_Utilities.ConvertTags(Originator, abilityRequiredTag)) == false)
                    continue;

                // if we got this far then we can add the hit as it hasn't been blocked by settings 
                hits.Add(i);


            }

            //if we haven't collided with anything then end here
            if (hits.Count == 0)
                return;

            //We have collided so can run the enable after event function and combolock
            Originator.EnableOriginatorsAbilitiesAfterEvent(this.abilityID, AbilityEvent.Collision);



            foreach (int rayCastIndex in hits) {

                //Set combo lock
                this.SetComboLock(raycastHits[rayCastIndex].collider.gameObject);

                Originator.AddToDiagnosticLog(this.name + " raycast hit: " + raycastHits[rayCastIndex].transform.name);
                // run effects and states 
                ApplyAbilityEffectsToObject(raycastHits[rayCastIndex].collider.gameObject, Originator, null, raycastHits[rayCastIndex].point, false, false, this.abilitySecondsPrepared);
            }

        }

        /// <summary>
        /// Sets up and activates the main ability projectile object. 
        /// </summary>
        /// <remarks>
        /// Adds all the correct projectile components and settings and activates the object. 
        /// </remarks>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator InitiateProjectile(ABC_IEntity Originator) {


            //If we are re-recording crosshair target after activation then retrieve the position now before the projectile is initiated
            if (this.travelType == TravelType.Crosshair && this.crossHairRecordTargetOnActivation == false) {
                rayCastTargetPosition = this.GetCrosshairRayCastPosition(Originator);
            }

            //Create initial projectile 
            ABC_Utilities.mbSurrogate.StartCoroutine(this.CreateProjectile(Originator, this.CalculateProjectileStartPosition(Originator), this.startingPositionOffset, this.startingPositionForwardOffset, this.startingPositionRightOffset, this.GetStartingPositionObject(Originator, AbilityStartPositionType.StartPosition), this.startingRotation, this.initiatingUseWeaponTrail, this.initiatingWeaponTrailGraphicIteration));

            //Activate initiating graphic if melee and graphic is  activating with ability
            if (this.abilityType == AbilityType.Melee && this.initiatingAestheticActivateWithAbility == true) {
                // get object and particle if they exist
                ActivateGraphic(Originator, AbilityGraphicType.Initiating);
            }


            //Create additional projectiles 
            for (int i = 0; i < this.additionalStartingPositions.Count(); i++) {


                //Wait for either a delay or for the initiating animation to hit a certain percentage before moving on (to create the ability)
                switch (this.additionalStartingPositions[i].startingDelayType) {

                    case AbilityInitiationDelayType.AtAnimationPercentage:

                        //Record animation runner and animator
                        ABC_AnimationsRunner aniRunner = Originator.animationRunner;
                        Animator animator = Originator.animator;

                        //If no animator then end here (Animation Runner requires this also)
                        if (animator == null)
                            break;

                        //Tracker for while loop
                        float animationProgressPercentage = 0;

                        while (animationProgressPercentage < 100) {

                            // if ability is interrupted then we are done with this loop
                            if (this.AbilityActivationInterrupted())
                                break;


                            //Prioritise animation runner clip progress if configured
                            if (this.initiatingAnimationRunnerClip.AnimationClip != null) {
                                animationProgressPercentage = aniRunner.GetCurrentAnimationProgress();
                            } else {
                                animationProgressPercentage = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 * 100;
                            }


                            //If progress is over the animation percentage then finish loop so ability object can now be created
                            if (animationProgressPercentage >= this.additionalStartingPositions[i].startingDelayInitiatingAnimationPercentage) {
                                break;
                            }

                            // wait till end of frame and continue loop
                            yield return new WaitForEndOfFrame();

                        }

                        break;

                    case AbilityInitiationDelayType.AfterDelay:

                        float delay = this.ModifyTimeByInitiatingAdjustments(Originator, this.additionalStartingPositions[i].startingDelay);

                        //wait for additional starting position delay
                        for (var x = delay; x > 0;) {

                            // actual wait time 
                            if (x < 0.2f) {
                                // less then 0.2  so we only need to wait the .xx time
                                yield return new WaitForSeconds(x);
                            } else {
                                // wait a small amount and keep looping till casting time = 0; 
                                yield return new WaitForSeconds(0.2f);
                            }

                            //If ability was interrupted then don't bother creating the additional objects
                            if (this.AbilityActivationInterrupted())
                                yield break;

                            //reduce time left unless ability is currently in hit stop then things are on hold 
                            if (this.hitStopCurrentlyActive == false)
                                x = x - 0.2f;
                        }

                        break;
                }


                //Randomise any tag targets
                if (this.travelType == TravelType.NearestTag && this.travelNearestTagRandomiseSearch == true)
                    this.RandomiseTagTargets();

                //If ability was interrupted then don't bother creating the additional objects
                if (this.AbilityActivationInterrupted())
                    yield break;


                //Time is up so create next additional starting position
                ABC_Utilities.mbSurrogate.StartCoroutine(this.CreateProjectile(Originator, this.CalculateProjectileStartPosition(Originator, this.additionalStartingPositions[i]), this.additionalStartingPositions[i].startingPositionOffset, this.additionalStartingPositions[i].startingPositionForwardOffset, this.additionalStartingPositions[i].startingPositionRightOffset, this.GetStartingPositionObject(Originator, AbilityStartPositionType.AdditionalStartPosition, this.additionalStartingPositions[i]), this.additionalStartingPositions[i].startingRotation, this.additionalStartingPositions[i].useWeaponTrail, this.additionalStartingPositions[i].weaponTrailGraphicIteration));

                //Activate initiating graphic if melee and graphic is  activating with ability
                if (this.additionalStartingPositions[i].repeatInitiatingGraphic == true && this.abilityType == AbilityType.Melee && this.initiatingAestheticActivateWithAbility == true) {
                    // get object and particle if they exist
                    ActivateGraphic(Originator, AbilityGraphicType.InitiatingAdditionalStartingPosition, false, i);
                }
            }


        }

        /// <summary>
        /// Will calculate the projectiles vector3 starting position depending on the ability config setup
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="AdditionalStartPosition">Additional Start Position Object, If provided then start position will be retrieved using values from this this object instead of the normal ability values</param>
        /// <returns>Vector3 position where projectile object will start</returns>
        private Vector3 CalculateProjectileStartPosition(ABC_IEntity Originator, AdditionalStartingPosition AdditionalStartPosition = null) {

            Vector3 retVal = new Vector3();

            //Work out starting position if ability is not travelling and starting at target
            if (this.targetTravel == false && (this.travelType == TravelType.SelectedTarget || this.travelType == TravelType.NearestTag || this.travelType == TravelType.Self || this.travelType == TravelType.ToWorld
           || this.travelType == TravelType.MouseTarget || this.travelType == TravelType.Mouse2D || this.travelType == TravelType.Crosshair)) {

                // we are starting at the target position 
                retVal = GetTargetPosition(Originator);


            } else {

                // start at normal starting position
                if (AdditionalStartPosition == null)
                    retVal = GetStartingPosition(Originator, AbilityStartPositionType.StartPosition);
                else // unless additional start position was provided, if so work it out using the additional start configuration
                    retVal = GetStartingPosition(Originator, AbilityStartPositionType.AdditionalStartPosition, AdditionalStartPosition);

            }


            return retVal;
        }

        /// <summary>
        /// Will create the ability projectile object
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="StartingPosition">Position which projectile object will start at</param>
        /// <param name="StartingPositionOffset">Offset of the starting position</param>
        /// <param name="StartingPositionForwardOffset">Forward offset of the starting position</param>
        /// <param name="StartingPositionRightOffset" Right offset of the starting position></param>
        /// <param name="StartingPositionObject">The starting object which the projectile will link too </param>
        /// <param name="StartingRotation">The starting rotation of the projectile object</param>
        /// <param name="ActivateWeaponTrail">If true then the equipped weapon trail will activate</param>
        /// <param name="WeaponTrailGraphicIteration">The weapon graphic iteration to activate the trail on</param>
        private IEnumerator CreateProjectile(ABC_IEntity Originator, Vector3 StartingPosition, Vector3 StartingPositionOffset, float StartingPositionForwardOffset, float StartingPositionRightOffset, GameObject StartingPositionObject, Vector3 StartingRotation, bool ActivateWeaponTrail = false, int WeaponTrailGraphicIteration = 0) {


            // declare new object
            GameObject abilityObj = GetAbilityObject(Originator, StartingPosition);

            //If melee attack then let originator know for tracking purposes (so attack can be immediatly stopped unlike projectiles), this is removed from tracking once projectile is destroyed
            if (this.abilityType == AbilityType.Melee && this.AbilityActivationInterrupted() == false)
                Originator.AddToActiveMeleeAbilities(this, abilityObj);


            // activate colliders for ability 
            ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateAbilityColliders(Originator, abilityObj));

            //set spawn direction 
            Transform startingPositionObjTransform = StartingPositionObject.transform;
            Quaternion startingDirection = startingPositionObjTransform.rotation * Quaternion.Euler(StartingRotation);
            abilityObj.transform.rotation = startingDirection;
            abilityObj.transform.localRotation = startingDirection;

            //If set to then copy euler angles also
            if (this.setEulerRotation)
                abilityObj.transform.localEulerAngles = startingPositionObjTransform.localEulerAngles + StartingRotation;


            // travel with caster
            if ((this.travelWithCaster == true && this.travelType == TravelType.NoTravel) || this.abilityType == AbilityType.Melee)
                abilityObj.transform.SetParent(StartingPositionObject.transform);


            if (this.abilityToggle != AbilityToggle.Off)
                this.ToggleOn(Originator, abilityObj);



            // add target script parameters and apply speed if applicable
            this.ActivateTravelScript(abilityObj, Originator, StartingPositionObject, StartingPositionOffset, StartingPositionForwardOffset, StartingPositionRightOffset);

            //activate projectile script
            this.ActivateProjectileScript(abilityObj, Originator);

            // let it go
            abilityObj.SetActive(true);

            //Activate current equipped weapon trail
            if (this.abilityType == AbilityType.Melee && ActivateWeaponTrail == true)
                Originator.ActivateEquippedWeaponTrail(ABC_Utilities.ModifyTimeByPercentage(this.GetAbilityInitiatingBaseSpeedAdjustment(Originator), this.duration) + ABC_Utilities.ModifyTimeByPercentage(this.GetAbilityInitiatingBaseSpeedAdjustment(Originator), +0.4f), 0f, WeaponTrailGraphicIteration, this);



            // if were limiting our active objects then run this check now 
            if (this.limitActiveAtOnce == true && GetActiveCount(Originator) > this.maxActiveAtOnce)
                DestroyLongestActive(Originator);


            yield break;

        }


        /// <summary>
        /// Will enable or disable the objects colliders after the delay
        /// </summary>
        /// <param name="Obj">Object which will have it's colliders enabled or disabled</param>
        /// <param name="Enabled">True to enable colliders, else false to disable</param>
        /// <param name="Delay">Delay before colliders are enabled/disabled</param>
        /// <param name="SetAsTrigger">If true then the colliders will be set as a trigger when they are enabled or disabled, else they won't be changed and left how they are</param>
        private IEnumerator ToggleObjectColliders(GameObject Obj, bool Enabled, float Delay = 0.0f, bool SetAsTrigger = false) {

            // wait delay seconds then enable or disable colliders (if it not been added already)
            if (Delay > 0f)
                yield return new WaitForSeconds(Delay);

            //If obj was destroyed stop here
            if (Obj == null)
                yield return null;

            // get all colliders incase others have been used
            Collider[] AllColliders = Obj.GetComponentsInChildren<Collider>(true);


            // enable collider
            foreach (Collider meCol in AllColliders) {
                meCol.enabled = Enabled;

                //set as trigger is passed through as true in parameter
                if (SetAsTrigger)
                    meCol.isTrigger = true;
            }



            yield return null;

        }

        /// <summary>
        /// Will handle activating the ability projectiles colliders. Activation depends on if the ability if colliders have been setup to either enable straight away, after a delay or after a key press 
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="AbilityObj">Ability Object which will have it's colliders enabled or disabled</param>
        private IEnumerator ActivateAbilityColliders(ABC_IEntity Originator, GameObject AbilityObj) {

            //If we are not adding a collider then finish here
            if (this.addAbilityCollider == false)
                yield break;


            // disable collider for now
            ABC_Utilities.mbSurrogate.StartCoroutine(ToggleObjectColliders(AbilityObj, false));


            // If we are not delaying collider then enable them all (stops any bugs with instant collision before this has been set) 
            if (this.colliderTimeDelay == false && this.colliderKeyPressDelay == false) {

                ABC_Utilities.mbSurrogate.StartCoroutine(ToggleObjectColliders(AbilityObj, true));

                // return here as collider are activated
                yield break;

            }


            // keep track of what couritine we are using to apply collider after delay. So we can stop it if collider comes on early stops respawn issues. 
            IEnumerator toggleCollidersCoroutine = null;

            // user can delay collider with setting so check that then enable with the delay 
            //We set the collider as a trigger for now, so when the colliders are enabled they won't immediatly physic in game before the projectile script decides if it should be a trigger or not
            if (this.colliderTimeDelay == true)
                ABC_Utilities.mbSurrogate.StartCoroutine(toggleCollidersCoroutine = ToggleObjectColliders(AbilityObj, true, this.colliderDelayTime, true));


            // can user add collider with a key press
            if (this.colliderKeyPressDelay == true) {

                // wait tiny bit then allow for the right key to add collider
                yield return new WaitForSeconds(0.3f);

                Collider abilityCollider = AbilityObj.GetComponent<Collider>();
                while (abilityCollider.enabled == false) {

                    //wait 1 frame so game can continue 
                    yield return null;


                    // if waiting for collider by key then we need to turn this to true after the second key press has happened
                    if (this.ButtonPressed(Originator, AbilityButtonPressState.ColliderDelayTrigger)) {

                        // stop currently toggleobject (incase its running from delay)
                        if (toggleCollidersCoroutine != null)
                            ABC_Utilities.mbSurrogate.StopCoroutine(toggleCollidersCoroutine);

                        // add colliders straight away 
                        ABC_Utilities.mbSurrogate.StartCoroutine(ToggleObjectColliders(AbilityObj, true, 0.1f, true));


                    }

                }//end of while loop

            }//end of keypressdelay

            yield break;

        }//end of method


        /// <summary>
        /// Will setup and enable the travel component attached on the ability object provided
        /// </summary>
        /// <param name="AbilityObj">Ability projectile gameobject</param>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <param name="StartingPositionObject">The starting object which the projectile will link too </param>
        /// <param name="StartingPositionOffset">Offset of the starting position</param>
        /// <param name="StartingPositionForwardOffset">Forward offset of the starting position</param>
        /// <param name="StartingPositionRightOffset" Right offset of the starting position></param>
        private void ActivateTravelScript(GameObject AbilityObj, ABC_IEntity Originator, GameObject StartingPositionObject, Vector3 StartingPositionOffset, float StartingPositionForwardOffset, float StartingPositionRightOffset) {

            ABC_ProjectileTravel projTravel = AbilityObj.GetComponent<ABC_ProjectileTravel>();

            if (projTravel == null)
                return;

            projTravel.travelType = this.travelType;
            projTravel.travelSpeed = this.travelSpeed;
            projTravel.travelPhysics = this.travelPhysics.ToString();
            projTravel.applyTravelDuration = this.applyTravelDuration;
            projTravel.travelDurationOriginatorTagsRequired = ABC_Utilities.ConvertTags(Originator, this.travelDurationOriginatorTagsRequired);
            projTravel.travelDurationTime = this.travelDurationTime;
            projTravel.travelDurationStopSuddenly = this.travelDurationStopSuddenly;
            projTravel.travelDelay = this.travelDelay;
            projTravel.originator = Originator.gameObject;

            projTravel.startingPositionObject = StartingPositionObject;
            projTravel.startingPositionOffset = StartingPositionOffset;
            projTravel.startingPositionForwardOffset = StartingPositionForwardOffset;
            projTravel.startingPositionRightOffset = StartingPositionRightOffset;

            projTravel.boomerangMode = this.boomerangMode;
            projTravel.boomerangDelay = this.boomerangDelay;



            // not used by all travel types 
            projTravel.continuouslyTurnToDestination = this.continuouslyTurnToDestination;
            projTravel.seekTargetDelay = this.seekTargetDelay;
            projTravel.targetOffset = this.selectedTargetOffset + this.GetMissChancePositionOffset(Originator);
            projTravel.targetForwardOffset = this.selectedTargetForwardOffset;
            projTravel.targetRightOffset = this.selectedTargetRightOffset;


            switch (this.travelType) {

                case TravelType.Crosshair:
                    //if were hitting for crosshair mode (fires through recticle) then use ray cast position
                    projTravel.targetPosition = rayCastTargetPosition;
                    break;
                case TravelType.MouseTarget:
                    projTravel.targetObject = null;
                    projTravel.targetPosition = GetMousePosition(Originator);

                    break;
                case TravelType.Mouse2D:
                    projTravel.targetObject = null;

                    //Get mouse position ignoring Z-Index as we are 2D
                    Vector3 mousePosition2D = GetMousePosition(Originator);
                    mousePosition2D.z = AbilityObj.transform.position.z;

                    projTravel.targetPosition = mousePosition2D;

                    break;
                case TravelType.SelectedTarget:
                    projTravel.targetObject = target;
                    projTravel.targetPosition = new Vector3(0, 0, 0);
                    break;
                case TravelType.NearestTag:
                    projTravel.targetObject = tagTargets.FirstOrDefault();
                    projTravel.targetPosition = new Vector3(0, 0, 0);
                    break;
                case TravelType.Self:
                    projTravel.targetObject = Originator.gameObject;
                    projTravel.targetPosition = new Vector3(0, 0, 0);
                    break;
                case TravelType.ToWorld:
                    projTravel.targetObject = this.worldTarget;
                    projTravel.targetPosition = this.worldTargetPosition;

                    break;
                case TravelType.CustomScript:

                    var customScript = AbilityObj.GetComponent(System.Type.GetType(this.customTravelScript.Object.name)) as MonoBehaviour;
                    customScript.enabled = true;

                    break;


            }


            // enable script
            projTravel.enabled = true;

        }

        /// <summary>
        /// Will disable ABC travel components attached on the ability object provided
        /// </summary>
        /// <param name="AbilityObj">Ability projectile gameobject</param>
        private void DisableTravelScripts(GameObject AbilityObj) {

            if (AbilityObj.GetComponent<ABC_ProjectileTravel>() != null)
                AbilityObj.GetComponent<ABC_ProjectileTravel>().enabled = false;

            // disable any custom scripts
            if (this.travelType == TravelType.CustomScript) {
                var customScript = AbilityObj.GetComponent(this.customTravelScript.Object.name) as MonoBehaviour;

                if (customScript != null)
                    customScript.enabled = true;
            }


        }


        /// <summary>
        /// Will setup and enable the projectile component on the object provided up 
        /// </summary>
        /// <param name="AbilityObj">Ability projectile gameobject</param>
        /// <param name="Originator">Entity that activated the ability</param>
        private void ActivateProjectileScript(GameObject AbilityObj, ABC_IEntity Originator) {

            ABC_Projectile projScript = AbilityObj.GetComponent<ABC_Projectile>();

            if (projScript == null)
                projScript = AbilityObj.AddComponent<ABC_Projectile>();

            //pass ability so projectile can make use of settings
            projScript.ability = this;

            // pass through originator
            projScript.originator = Originator;

            // are we travelling to target or us?	
            if (this.travelType == TravelType.Self) {
                projScript.targetObj = Originator.gameObject;
            } else if (tagTargets.Count > 0) {
                projScript.targetObj = tagTargets.FirstOrDefault();
            } else if (target != null) {
                projScript.targetObj = target;
            }




            // record time we activated the projectile (allows devs to work out earliest ability etc)
            projScript.timeActivated = Time.time;



            //update projectile script with surrounding object settings
            projScript.surroundingObjects = surroundingObjects;

            //if melee attack then let projectile script know initiation speed adjustments (so duration can be modified appropriatly)
            if (this.abilityType == AbilityType.Melee) {
                projScript.initiationBaseSpeedAdjustment = this.GetAbilityInitiatingBaseSpeedAdjustment(Originator);
                projScript.initiationGlobalSpeedAdjustment = this.abilityCurrentInitiationSpeedAdjustment;

            } else {
                //Not melee so leave speed adjustment to 100%
                projScript.initiationBaseSpeedAdjustment = 100;
                projScript.initiationGlobalSpeedAdjustment = 100;
            }

            // make sure projectile script is on 
            projScript.enabled = true;

        }



        /// <summary>
        /// Will return the abilities recast time, taking into account the originators global adjustments
        /// </summary>
        /// <returns>Float value defining how long the ability will take to cooldown</returns>
        private float GetAbilityRecast(ABC_IEntity Originator) {

            //Record the originators current global cooldown adjustment
            this.abilityCurrentCooldownAdjustment = Originator.GetAbilityGlobalCooldownAdjustment();

            //calculated by returning a percentage of the abilities recast. The percentage is dependant on the current cooldown adjustment 
            //i.e 100 % of recast time 12 will return 12, 50% cooldown adjustment would return 6
            return this.abilityRecast / 100 * this.abilityCurrentCooldownAdjustment;

        }


        /// <summary>
        /// Starts ability cooldown allowing the ability to be activated again after waiting for the recast duration.
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        private IEnumerator StartCooldown(ABC_IEntity Originator) {

            //Determine what time we started cooldown
            this.abilityCooldownStartTime = Time.time;


            // wait for recast
            yield return new WaitForSeconds(this.GetAbilityRecast(Originator));


            // set ability can cast to true 
            this.abilityOnCooldown = false;

            if (Originator != null) {
                if (Originator.LogInformationAbout(LoggingType.ReadyToCastAgain)) {
                    Originator.AddToAbilityLog(this.name + " is ready to cast again");
                }
            }

            Originator.AddToDiagnosticLog(this.name + " is ready to cast again");

        }




        /// <summary>
        /// Returns how many of this ability's projectiles are currently active
        /// </summary>
        /// <param name="Originator">Entity that activated the ability</param>
        /// <returns>An integer count of the active projectile objects</returns>
        private int GetActiveCount(ABC_IEntity Originator) {
            int count = 0;
            // check if ability pool has objects
            if (this.abilityPool.Count > 0) {
                // loop through all lists in dictionary 
                foreach (GameObject obj in this.abilityPool) {
                    // if object is active and it is not the current preactivated Projectile (too starting point) 
                    //(as technically this hasn't spawned as an official projectile yet as its only active due to travelling to start position) then add one to counter
                    if (obj.activeInHierarchy == true && this.preActivatedProjectiles.Contains(obj) == false) {
                        count += 1;
                    }
                }
            }
            return count;


        }


        /// <summary>
        /// Returns this ability's longest active projectile which was initiated by the Originator provided
        /// </summary>
        /// <param name="Originator">Entity which initiated the ability projectiles</param>
        /// <returns>This ability's longest active projectile gameobject activated by the originator</returns>
        private GameObject GetLongestActive(ABC_IEntity Originator) {
            // check if key exist
            if (this.abilityPool.Count > 0) {
                GameObject result = null;
                float time = 0f;
                // loop through all lists in dictionary and return the object which has the least active time (as they were activated sooner - active time records how long game running when active) 
                foreach (GameObject obj in this.abilityPool) {
                    ABC_Projectile objProj = obj.GetComponent<ABC_Projectile>();
                    // we make sure object is also not the linkedProjectileTSP as technically this hasn't officially been spawned yet as its only active due to travelling to start position
                    if (obj.activeInHierarchy == true && this.preActivatedProjectiles.Contains(obj) == false && objProj.originator == Originator && (objProj.timeActivated < time || result == null)) {
                        result = obj;
                        time = objProj.timeActivated;
                    }
                }
                return result;
            }
            return null;


        }


        /// <summary>
        /// Destroys this ability's longest active projectile which was initiated by the Originator provided
        /// </summary>
        /// <param name="Originator">Entity which initiated the ability projectiles</param>
        private void DestroyLongestActive(ABC_IEntity Originator) {

            this.DestroyAbility(GetLongestActive(Originator));

        }



        #endregion




        // ********* ENUMS for Ability **********************


        #region Ability ENUMs



        // only used in ability class
        private enum AbilityGraphicType {
            Preparing,
            Initiating,
            Reloading,
            ScrollAbilityActivation,
            ScrollAbilityDeactivation,
            InitiatingAdditionalStartingPosition
        }


        // only used in ability class
        private enum AbilityStartPositionType {
            StartPosition,
            ProjectileToStartPosition,
            AdditionalStartPosition
        }



        private enum AbilityButtonPressState {
            AbilityTrigger,
            AdditionalAbilityTrigger,
            ColliderDelayTrigger,
            WaitBeforeInitiating,
            ScrollQuickKey,
            CollisionEnabledAfterTrigger

        }


        private enum AbillityButtonPressType {
            Press,
            Hold
        }

        public enum AbilityMovementValidation {
            TriggerOnlyIfMoving, 
            TriggerOnlyIfNotMoving
        }

        private enum AbilityAnimationState {
            Preparation = 0,
            Initiate = 1,
            ScrollActivate = 2,
            ScrollDeactivate = 3,
            Reload = 4
        }


        private enum NoTargetStillTravelPreviousType {
            None = 0,
            SelectedTarget = 1,
            NearestTag = 2
        }

        public enum ProjectileToStartType {
            Preparing,
            Initiating
        }

        public enum AbilityInitiationDelayType {
            AfterDelay = 0,
            AtAnimationPercentage = 1
        }

        public enum AbilityNoTargetRotateBehaviour {
            CurrentDirection,
            CameraCenter,
            MousePosition
        }


        public enum AbilityLandOrAir {
            LandOrAir = 0,
            Land = 1,
            Air = 2
        }


        public enum AbilityToggle {
            Off = 0,
            OnOff = 1,
            Hold = 2
        }


        // Starting Direction in relation to object 
        public enum Direction {
            Forward = 0,
            Behind = 1,
            Right = 2,
            Left = 3,
            Up = 4,
            Down = 5,
            ForwardUp = 6,
            BehindUp = 7
        }


        // what type of physic movement we using (Velocity or Add Force)
        public enum TravelPhysics {
            Velocity = 0,
            Force = 1
        }

        public enum DurationType {
            Duration,
            Persistant
        }

        public enum AbilityActivationCompleteEventType {
            AbilityInitiated = 0,
            AbilityDestroyed = 1
        }


        #endregion

    }

}

