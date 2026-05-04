using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;


namespace ABCToolkit {

    /// <summary>
    /// Class for Effect Objects. Effects are applied by Abilities. Once an effect is applied it will do different actions depending on the effect name and settings. Like damage an entity or move an object
    /// </summary>
    /// <remarks>
    /// An effect object can be activated and once this has occured it will run it's own code to apply the effect. 
    /// </remarks>
    [System.Serializable]
    public class Effect {


        // ********************* Settings ********************
        #region Settings

        /// <summary>
        /// List of effect graphics (pool)
        /// </summary>
        public List<GameObject> effectPool = new List<GameObject>();


        /// <summary>
        /// Used by inspector only - determines if the effect settings are collapsed out or not 
        /// </summary>
        public bool foldOut = false;


        /// <summary>
        /// Links to a global effect which will activate instead of the properties in this effect
        /// </summary>
        [Tooltip("Links to a global effect which will activate instead of the properties in this effect")]
        public ABC_GlobalElement globalEffect = null;

        /// <summary>
        /// The ID of the ability which this effect links with. This is declared in the inspector when created and used to remove all effects from ability xyz
        /// </summary>
        public int effectAbilityID = 0;

        /// <summary>
        /// unique ID of the effect
        /// </summary>
        [Tooltip("unique ID of the effect")]
        public int effectID = 0;

        /// <summary>
        /// Name of the effect 
        /// </summary>
        [Tooltip("Name of the effect ")]
        public string effectName = "";



        /// <summary>
        /// Strength of the effect
        /// </summary>
        [Tooltip("Strength of the effect")]
        public float potency = 10f;

        /// <summary>
        /// If true then the potency value will be modified depending on stats setup from either the entity, originator or both 
        /// </summary>
        [Tooltip("If true then the potency value will be modified depending on stats setup from either the entity, originator or both ")]
        public bool modifyPotencyUsingStats = false;


        //class for working out any potency modifications (Add 80% of Originators Strength)
        [System.Serializable]
        public class PotencyStatModifications {

            public PotencyStatModifications() {

            }

            /// <summary>
            /// The integration type for stat functionality - if ABC is picked then normal functionality is used
            /// else stats from another integration system i.e game creator is used
            /// </summary>
            public ABCIntegrationType statIntegrationType;

            /// <summary>
            /// The operator used in changing the potency (Add, Subtract, Divide, Multiply)
            /// </summary>
            public ArithmeticOperators arithmeticOperator;

            /// <summary>
            /// How much of the stat value to use (`0% of strength etc)
            /// </summary>
            public float percentageValue;

            /// <summary>
            /// Where the stat comes from (originator, entity, both)
            /// </summary>
            public ActivateOn statSource;

            /// <summary>
            /// Name of the stat
            /// </summary>
            public string statName;


        }

        /// <summary>
        /// A list of stat modifications to make to the potency value which is used in many effects
        /// </summary>
        [Tooltip("A list of stat modifications to make to the potency value which is used in many effects")]
        public List<PotencyStatModifications> potencyStatModifications = new List<PotencyStatModifications>();

        /// <summary>
        /// If true then potency will be modified by a critical hit, this is done after any stat modifications 
        /// </summary>
        [Tooltip("If true then potency will be modified by a critical hit, this is done after any stat modifications")]
        public bool enhancePotencyFromCriticalHit = false;

        /// <summary>
        /// Percentage Amount to enhance potency by when a critical hit happens
        /// </summary>
        [Tooltip("Percentage Amount to modify potency by when a critical hit happens")]
        public float potencyCriticalHitBasePercentageEnhancement = 100;

        /// <summary>
        /// Will allow for a stat to modify the crit enhancement percentage
        /// </summary>
        [Tooltip("Will allow for a stat to modify the crit enhancement percentage")]
        public bool potencyCriticalHitEnablePercentageEnhancementStatModification = false;

        /// <summary>
        /// Stat modification for potency percentage enhancement
        /// </summary>
        [Tooltip("Stat modification for potency percentage enhancement")]
        public PotencyStatModifications potencyCriticalHitPercentageEnhancementStatModification = new PotencyStatModifications();

        /// <summary>
        /// Audio to play when a critical hit happens
        /// </summary>
        [Tooltip("Audio to play when a critical hit happens")]
        public ABC_AudioClipReference potencyCriticalHitAudio = new ABC_AudioClipReference();

        /// <summary>
        /// Color of text when critical hit happens
        /// </summary>
        [Tooltip("Color of text when critical hit happens")]
        public Color potencyCriticalHitColor = Color.yellow;

        /// <summary>
        /// A field which can be used to transfer further information about the effect. Can be used by developers to pass through objects ID's or say which stat to increase etc.
        /// </summary>
        [Tooltip("A field which can be used to transfer further information about the effect. Can be used by developers to pass through objects ID's or say which stat to increase etc. ")]
        public string miscellaneousProperty = "";

        /// <summary>
        /// An alternative which can be used to transfer further information about the effect. Can be used by developers to pass through objects ID's or say which stat to increase etc.
        /// </summary>
        [Tooltip("An alternative field which can be used to transfer further information about the effect. Can be used by developers to pass through objects ID's or say which stat to increase etc. ")]
        public string miscellaneousAltProperty = "";

        /// <summary>
        /// Determines who the effect will activate on 
        /// </summary>
        /// <remarks>
        /// Entity - Will apply the effect on the entity hit by the ability. 
        /// Originator - Will apply the effect on the originator. 
        /// BothSimultaneously - Will apply the effect simultaneously on both the entity hit and the originator
        /// </remarks>
        public ActivateOn activateOn;

        /// <summary>
        /// Determines the event type of the effect when activating and deactivating. Standard will run the normal code, RaiseEvent will invoke the event delegate and both will do everything. 
        /// </summary>
        public EffectEventType eventType = EffectEventType.Standard;

        /// <summary>
        /// Time before effect starts
        /// </summary>
        [Tooltip("Time before effect starts")]
        public float delay = 0f;

        /// <summary>
        /// How long the effect lasts on the entity. Whilst active on target the same effect from the same ability can not be applied again unless allow duplicate effect activation has been set 
        /// </summary>
        [Tooltip("How long effect lasts on object - Whilst active on target the same effect from the same ability can not be applied again unless allow duplicate effect activation has been set")]
        public float effectDuration = 2f;

        /// <summary>
        /// If true then the effect can be applied multiple times on the enemy. If false then the effect can only be applied once per ABC Entity.
        /// </summary>
        [Tooltip("If true then the effect can be applied multiple times on the enemy. If false then the effect can only be applied once per ABC user")]
        public bool allowDuplicateEffectActivation = false;

        /// <summary>
        /// If true then the effect will only allow duplicate effects up to a certain limit.
        /// </summary>
        [Tooltip("If true then the effect will only allow duplicate effects up to a certain limit.")]
        public bool limitNoOfDuplicateEffectActivations = false;

        /// <summary>
        /// The max amount of duplicate effects allowed to be activated
        /// </summary>
        [Tooltip("The max amount of duplicate effects allowed to be activated")]
        public int maxNoDuplicateEffectActivations = 2;

        /// <summary>
        /// If true then the effect can be repeated
        /// </summary>
        [Tooltip("Does the effect repeat")]
        public bool repeatEffect = false;

        /// <summary>
        /// How long the effect will repeat for
        /// </summary>
        [Tooltip("How long the effect can repeat for")]
        public int repeatDuration = 10;

        /// <summary>
        /// The interval between repeat effect activation
        /// </summary>
        [Tooltip("The interval between when effects happen")]
        public int repeatInterval = 2;

        /// <summary>
        /// How many times the ability has been repeated
        /// </summary>
        public int repeatCount = 0;

        /// <summary>
        /// If true the effect graphics will activate everytime the effect is repeated
        /// </summary>
        [Tooltip("Does it repeat all aesthetics to do with the effect")]
        public bool repeatAesthetics = true;

        /// <summary>
        /// If true then the effect will only activate on the target if they are still in range of the collision which applied the effect. Works well for dodging effects that happen after a certain amount of time. 
        /// </summary>
        [Tooltip("Ability effect will only activate on the target if they are still in range of the main projectile. Works well for dodging effect delays")]
        public bool enableEffectActivationRange = false;

        /// <summary>
        /// How far the target has to be from the collision position for the effect to activate. 
        /// </summary>
        [Tooltip("Range of effect activation")]
        public float effectActivationRange = 5f;

        /// <summary>
        /// List of tags the effects will not activate on
        /// </summary>
        public List<string> effectIgnoreTag = new List<string>();

        /// <summary>
        /// If true then the ignore tags will be looked for on the object hit
        /// </summary>
        public bool effectIgnoreTagCheckObject = false;

        /// <summary>
        /// If true then the ignore tags will be looked for on the entity
        /// </summary>
        public bool effectIgnoreTagCheckEntity = false;

        /// <summary>
        /// List of tags the effects requires to activate on
        /// </summary>
        public List<string> effectRequiredTag = new List<string>();

        /// <summary>
        /// If true then the required tags will be looked for on the object hit
        /// </summary>
        public bool effectRequiredTagCheckObject = false;

        /// <summary>
        /// If true then the required tags will be looked for on the entity
        /// </summary>
        public bool effectRequiredTagCheckEntity = false;

        /// <summary>
        /// Instead of setting the effectActivationRange setting this will instead take the range depending on the projectile graphic size
        /// </summary>
        [Tooltip("The range is the size of the projectile")]
        public bool useProjectileForActivationRange = true;


        /// <summary>
        /// If true then a dice roll will decide if the effect can activate on the entity
        /// </summary>
        [Tooltip("If true then a dice roll will decide if the effect can activate on the entity")]
        public bool effectRandomChance = false;

        /// <summary>
        /// Minimum probability of effect activating on entity. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float effectRandomChanceProbabilityMin = 0f;

        /// <summary>
        /// Maximum probability of effect activating on the entity. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float effectRandomChanceProbabilityMax = 100f;

        /// <summary>
        /// If true then a hit will prevent the target from moving for a time set in their settings
        /// </summary>
        [Tooltip("If true then a hit will prevent the target from moving for a time set in their settings")]
        public bool hitStopsMovement = true;

        /// <summary>
        /// If true then a dice roll will decide if the entity movement is prevented applies some chance to the situation. 
        /// </summary>
        [Tooltip("If true then a dice roll chance will determine if the target casting is prevented ")]
        public bool hitStopsMovementRandomChance = false;

        /// <summary>
        /// Minimum probability of preventing the entity from moving. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitStopsMovementProbabilityMin = 0f;

        /// <summary>
        /// Maximum probability of preventing the entity from moving. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitStopsMovementProbabilityMax = 100f;

        /// <summary>
        /// If true then when this effects activate it will prevent the entity from activating an ability for a time set in their StateManager settings. 
        /// </summary>
        [Tooltip("If true then a hit will prevent the target from casting for a time set in their settings")]
        public bool hitPreventsCasting = false;

        /// <summary>
        /// If true then a dice roll will decide if the entity casting is prevented applies some chance to the situation. 
        /// </summary>
        [Tooltip("If true then a dice roll chance will determine if the target casting is prevented ")]
        public bool hitPreventsCastingRandomChance = false;

        /// <summary>
        /// Minimum probability of preventing the entity from casting. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitPreventsCastingProbabilityMin = 0f;

        /// <summary>
        /// Maximum probability of preventing the entity from casting. If the dice roll is higher then the minimum and lower then the maximum then it will prevent casting. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitPreventsCastingProbabilityMax = 100f;

        /// <summary>
        /// If true then when this effect activates it will interrupt any ability the entity is currently activating. 
        /// </summary>
        [Tooltip("If true then a hit will stop the target from casting")]
        public bool hitInterruptsCasting = false;


        /// <summary>
        /// If true then the ability interruption will depend on a dice roll chance. 
        /// </summary>
        [Tooltip("If true then a dice roll chance will determine if the target casting is interuppted ")]
        public bool hitInterruptsCastingRandomChance = false;

        /// <summary>
        /// Minimum probability of interrupting the entity ability activation. If the dice roll is higher then the minimum and lower then the maximum then it will interrupt the ability. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitInterruptsCastingProbabilityMin = 0f;

        /// <summary>
        /// Maximum probability of interrupting the entity ability activation. If the dice roll is higher then the minimum and lower then the maximum then it will interrupt the ability. 
        /// </summary>
        [Range(0f, 100f)]
        public float hitInterruptsCastingProbabilityMax = 100f;

        /// <summary>
        /// If true then the effect will activate the graphic setup. 
        /// </summary>
        [Tooltip("Determines if the impact effect plays per effect on an ability")]
        public bool playEffect = true;

        /// <summary>
        /// If true then the effect will add text to the effect text graphic which appears in game
        /// </summary>
        [Tooltip("Adds text to the effect text graphic")]
        public bool addToEffectText = true;

        /// <summary>
        /// The text to display on the effect text graphic which appears in game
        /// </summary>
        [Tooltip("Text to display on graphic")]
        public string effectText = "";

        /// <summary>
        /// How long the text graphic which appears in game should be displayed for
        /// </summary>
        [Tooltip("How long text graphic is displayed")]
        public float effectTextDuration = 3f;

        /// <summary>
        /// Colour of the text graphic which appears in game
        /// </summary>
        public Color effectTextColour = Color.white;

        /// <summary>
        /// If true then the effect will add text to the GUI Log
        /// </summary>
        [Tooltip("Adds text to the effect log Gui")]
        public bool addToEffectLog = true;

        /// <summary>
        /// Text to display on the effect log
        /// </summary>
        [Tooltip("Text to display in log")]
        public string effectLogText = "";

        /// <summary>
        /// If it exists and this option is true then the remove effect will occur (like taking stats back down)
        /// </summary>
        [Tooltip("If it exists and this option is true then the remove effect will occur (like taking stats back down)")]
        public bool enableRemoveEffect = true;

        /// <summary>
        /// If true then effects can be dispelled (removed earlier) through other effects
        /// </summary>
        public bool dispellable = false;

        /// <summary>
        /// If true then a specific amount of prepare time will be required for the effect to activate, either greater or less then
        /// </summary>
        [Tooltip("If true then a specific amount of prepare time will be required for the effect to activate, either greater or less then")]
        public bool specificPrepareTimeRequried = false;

        /// <summary>
        /// If the prepare time has to be greater then or less then to activate
        /// </summary>
        [Tooltip(" If the prepare time has to be greater then or less then to activate")]
        public ArithmeticComparisons specificPrepareTimeArithmeticComparison = ArithmeticComparisons.GreaterThan;

        /// <summary>
        /// The amount of time the ability had to prepare below or greater then for effect to activate
        /// </summary>
        [Tooltip("The amount of time the ability had to prepare below or greater then for effect to activate")]
        public float prepareTimeToActivate = 1f;

        /// <summary>
        /// If enabled then the effect will only be activated if it was applied from the ability 'splashing' 
        /// </summary>
        [Tooltip("If enabled then the effect will only be activated if it was applied from the ability 'splashing'")]
        public bool requireSplashToActivate = false;

        /// <summary>
        /// If enabled then the effect will only be activated if a critical hit occured
        /// </summary>
        [Tooltip("If enabled then the effect will only be activated if a critical hit occured")]
        public bool requireCriticalHitToActivate = false;


        /// <summary>
        /// If true when the remove effect is activated text will be added to the effect text graphic which appears in game
        /// </summary>
        [Tooltip("Adds text to the effect graphic gui when an effect is removed")]
        public bool removeEffectAddToEffectText = true;

        /// <summary>
        /// Text to display on the text graphic when the effect is removed
        /// </summary>
        [Tooltip("Text to display in log gui when effect is removed")]
        public string removeEffectText = "";

        /// <summary>
        /// How long the text graphic which appears in game should be displayed for
        /// </summary>
        [Tooltip("How long text graphic is displayed")]
        public float removeEffectTextDuration = 3f;

        /// <summary>
        /// Colour of the text graphic which appears in game 
        /// </summary>
        public Color removeEffectTextColour = Color.white;

        /// <summary>
        /// If true then the effect will add text to the GUI Log once it is removed
        /// </summary>
        [Tooltip("Adds text to the effect log when the effect is removed")]
        public bool removeEffectAddToEffectLog = true;

        /// Text to display on the effect log once effect is removed
        [Tooltip("Text to display in log when effect is removed")]
        public string removeEffectLogText = "";

        /// <summary>
        /// If true then the effect log will notify the user why the effect didn't activate (already exists, resisted etc)
        /// </summary>
        [Tooltip("If true then the effect log will notify the user why the effect didn't activate (already exists, resisted etc)")]
        public bool showNonActivationReason = true;

        /// <summary>
        /// Graphic which is created when the effect activates
        /// </summary>
        [Tooltip("Particle to show when effect activates")]
        public ABC_GameObjectReference effectGraphic;


        /// <summary>
        /// Sub objects which will be a child of the main graphic which is created when the effect activates
        /// </summary>
        [Tooltip("Sub particle to show on impact as a child of the main particle")]
        public ABC_GameObjectReference effectChildGraphic;

        /// <summary>
        /// If true then graphic scale will be modified during play
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public bool scaleEffectGraphic = false;

        /// <summary>
        /// scale to apply to graphic
        /// </summary>
        [Tooltip("If true then graphic scale will be modified during play")]
        public float effectGraphicScale = 1f;

        /// <summary>
        /// offset of effect graphic (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Offset of selected target (gets the target position but then adds in the offset)")]
        public Vector3 effectGraphicOffset;

        /// <summary>
        /// forward offset of effect graphic (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Forward offset from  position")]
        public float effectGraphicForwardOffset = 0f;

        /// <summary>
        /// right offset of effect graphic (gets the target position but then adds in the offset) 
        /// </summary>
        [Tooltip("Right offset from  position")]
        public float effectGraphicRightOffset = 0f;



        /// <summary>
        /// How long to wait till effect graphic appears
        /// </summary>
        [Tooltip("Wait time before particle appears")]
        public float effectGraphicDelay;

        /// <summary>
        /// How long graphic will appear for
        /// </summary>
        [Tooltip("How long particle lasts for")]
        public float effectGraphicDuration = 2f;

        /// <summary>
        /// If true then the effect graphic will follow the target (becomes a child object)
        /// </summary>
        [Tooltip("If true the impact effect will follow the target")]
        public bool effectFollowTarget;

        /// <summary>
        /// If true then the effect graphic will appear where the hit occured. If false it will be at the targets transform position
        /// </summary>
        [Tooltip("If true then the effect graphic will appear where the hit occured. If false it will be at the targets transform position")]
        public bool effectOnHitPosition = true;




        #endregion

        // ********************* Variables ********************

        #region Variables

        /// <summary>
        /// Tracks what potency was applied to the effect to be used when removing effects etc
        /// </summary>
        private float appliedEffectPotency = 0f;

        /// <summary>
        /// If true then the effect will end early
        /// </summary>
        private bool effectDispelled = false;


        /// <summary>
        /// The ability type which applied the effect
        /// </summary>
        private AbilityType effectAbilityType;



        #endregion


        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will create all the graphics relating to this effect and place it in the ABC Pool. It will also return one of the graphic objects. 
        /// </summary>
        /// <param name="CreateOne">Only one graphic will be created and this will also be returned. Useful if the pool has run out of premade objects</param>
        /// <returns>1 effect graphic object which was created</returns>
        public GameObject CreateEffectObjects(bool CreateOne = false) {

            GameObject effAB = null;

            if (this.playEffect == true && this.effectGraphic.GameObject != null) {
                //how many objects to make
                float objCount = CreateOne ? 1 : this.effectGraphicDuration + 3;

                for (int i = 0; i < objCount; i++) {
                    // create object particle 
                    effAB = (GameObject)(GameObject.Instantiate(this.effectGraphic.GameObject));
                    effAB.name = this.effectGraphic.GameObject.name;

                    // copy child object for additional Aesthetic 
                    if (this.effectChildGraphic.GameObject != null) {
                        GameObject impChildAB = (GameObject)(GameObject.Instantiate(this.effectChildGraphic.GameObject));
                        impChildAB.name = this.effectChildGraphic.GameObject.name;
                        impChildAB.transform.position = effAB.transform.position;
                        impChildAB.transform.rotation = effAB.transform.rotation;
                        impChildAB.transform.parent = effAB.transform;
                    }


                    // disable effect and return to pool
                    ABC_Utilities.PoolObject(effAB);


                    // add to generic list. 
                    this.effectPool.Add(effAB);
                }


            }


            return effAB;

        }


        /// <summary>
        /// Depending on the action (Add/Remove/OutOfRange/NoEffect) will add the related text setup to the targets effect log.
        /// </summary>
        /// <param name="Target">Target effect was applied too</param>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Action">Effect action (Add/Remove/OutOfRange/NoEffect)</param>
        /// <param name="Originator">(Optional) the originator who activated the ability which is now applying effects</param>
        public void LogEffectText(ABC_IEntity Target, string AbilityName, EffectAction Action, ABC_IEntity Originator = null) {

            //If target doesn't exist then can't add to log
            if (Target == null)
                return;

            string text = "";

            switch (Action) {
                case EffectAction.Add:

                    // if setting has not allowed for us to add to effect log then return here
                    if (this.addToEffectLog == false)
                        return;

                    text = this.ReplacePlaceHolders(this.effectLogText, AbilityName, Target, Originator);


                    break;

                case EffectAction.Remove:

                    // if setting has not allowed for us to add to effect log then return here
                    if (this.removeEffectAddToEffectLog == false)
                        return;

                    text = this.ReplacePlaceHolders(this.removeEffectLogText, AbilityName, Target, Originator);


                    break;

                case EffectAction.OutOfRange:

                    // if setting has not allowed for us to add to effect log then return here
                    if (this.showNonActivationReason == false)
                        return;

                    text = Target.gameObject.name + " is not in range of " + AbilityName + ". " + this.effectName + " did not activate";


                    break;
                case EffectAction.NoEffect:

                    // if setting has not allowed for us to add to effect log then return here
                    if (this.showNonActivationReason == false)
                        return;

                    text = AbilityName + " " + this.effectName + " had no effect on " + Target.gameObject.name;


                    break;



            }


            //If we got this far then log the text 
            Target.AddToEffectLog(text);

        }



        /// <summary>
        /// Returns a bool if the target is still in range of the object (projectile) that applied the effects
        /// </summary>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Target">Target the effect is activating on</param>
        /// <param name="Projectile">object (projectile) that applied the effects</param>
        /// <returns>True if the target is in range of the object applying the effect, else false</returns>
        public bool TargetInRange(string AbilityName, ABC_IEntity Target, GameObject Projectile) {

            // if setting is off then the effect is always added. If proj script doesn't exist then it was probs a ray cast so we will add effect anyway 
            if (this.enableEffectActivationRange == false || Projectile == null)
                return true;


            // do we use the projectile size or a custom range?
            float range = this.useProjectileForActivationRange == true ? Projectile.GetComponent<Renderer>().bounds.extents.magnitude : this.effectActivationRange;

            // if our spawn position and target position distance is less then range then return true else false
            if (Vector3.Distance(Target.transform.position, Projectile.transform.position) <= range) {
                return true;
            } else {

                // log to user that target was not in range
                this.LogEffectText(Target, AbilityName, EffectAction.OutOfRange);

                return false;
            }


        }

        /// <summary>
        /// Will apply the activation delay determined in the effect settings
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForActivationDelay() {

            yield return new WaitForSeconds(this.delay);

        }


        /// <summary>
        /// Returns a bool determining if the effect can be added (checks ignore tags and random activation chance)
        /// </summary>
        /// <param name="Originator">entity that is applying the effect</param>
        /// <param name="Target">Target entity the effect will be added to</param>
        /// <param name="ObjectHit">The object which was actually hit by the ability (can be used for only applying effects to headshots etc)</param>
        /// <returns>True if effect can be added, else false</returns>
        public bool CanActivate(ABC_IEntity Originator, ABC_IEntity Target, GameObject ObjectHit = null) {

            //If effect has a random chance of activating then dice roll, if not inbetween the probability then return false
            if (this.effectRandomChance == true && ABC_Utilities.DiceRoll(this.effectRandomChanceProbabilityMin, this.effectRandomChanceProbabilityMax) == false)
                return false;

            //If entity has a tag the effect is ignoring then effect can't be added so return false
            if (this.effectIgnoreTagCheckEntity == true && ABC_Utilities.ObjectHasTag(Target.gameObject, ABC_Utilities.ConvertTags(Originator, this.effectIgnoreTag)))
                return false;

            //If entity does not have a tag the effect requires then effect can't be added so return false
            if (this.effectRequiredTagCheckEntity == true && ABC_Utilities.ObjectHasTag(Target.gameObject, ABC_Utilities.ConvertTags(Originator, this.effectRequiredTag)) == false)
                return false;

            //Check for tags on the object hit
            if (ObjectHit != null) {

                //If object hit has a tag the effect is ignoring then effect can't be added so return false
                if (this.effectIgnoreTagCheckObject == true && ABC_Utilities.ObjectHasTag(ObjectHit, ABC_Utilities.ConvertTags(Originator, this.effectIgnoreTag)))
                    return false;

                //If object hit does not have a tag the effect requires then effect can't be added so return false
                if (this.effectRequiredTagCheckObject == true && ABC_Utilities.ObjectHasTag(ObjectHit, ABC_Utilities.ConvertTags(Originator, this.effectRequiredTag)) == false)
                    return false;
            }


            //Effect can activate if we got this far so return true 
            return true;

        }

        /// <summary>
        /// Main function to activate the effect. Will activate graphics, apply effect logic to the target entity (reduce damage etc), handle repeating the effect and apply any interruptions to the target entity. 
        /// </summary>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Target">Target entity which will recieve the effect</param>
        /// <param name="Originator">Originator who activated the ability which is now activating effects</param>
        /// <param name="AbilityType">The type of ability which applied the effect (projectile, raycast, melee)</param>
        /// <param name="WasCriticalHit">Determines if the potency has been modified by a critical hit</param>
        /// <param name="HitPoint">(Optional) Vector3 position where ability hit - effect graphics will appear at this location (if none is provided then it will appear at target location ) and the position is also used in some effects </param>
        /// <param name="Projectile">(Optional) Ability Projectile Gameobject which collided to activate the effects on the target</param>
        public IEnumerator Activate(string AbilityName, ABC_IEntity Target, ABC_IEntity Originator, AbilityType AbilityType, bool WasCriticalHit = false, Vector3 HitPoint = default(Vector3), GameObject Projectile = null) {

            //Reset the effect dispel tracking variable (if true will remove effect immediatly)
            this.effectDispelled = false;

            //Track what the ability type was that applied the effect 
            this.effectAbilityType = AbilityType;

            //determine who activate the effect on 
            List<ABC_IEntity> entitiesToActivateOn = new List<ABC_IEntity>();

            switch (this.activateOn) {
                case ActivateOn.Entity:
                    entitiesToActivateOn.Add(Target);
                    break;
                case ActivateOn.Originator:
                    entitiesToActivateOn.Add(Originator);
                    break;
                case ActivateOn.BothSimultaneously:
                    //Add both target and originator if they are not the same object
                    entitiesToActivateOn.Add(Target);
                    if (Target.gameObject != Originator.gameObject)
                        entitiesToActivateOn.Add(Originator);
                    break;

            }


            //Calculate how many times effect will repeat
            int repeatCount = 0;
            int maxRepeats = 1;

            if (this.repeatEffect == true && this.repeatDuration > 0 && this.repeatInterval > 0)
                maxRepeats = this.repeatDuration / this.repeatInterval;

            while (repeatCount <= maxRepeats) {


                foreach (ABC_IEntity entity in entitiesToActivateOn) {

                    //interrupt my casting as I have just been hit (as long as chance is false or dice roll returns true)
                    if (this.hitInterruptsCasting == true && (this.hitInterruptsCastingRandomChance == false || this.hitInterruptsCastingRandomChance == true && ABC_Utilities.DiceRoll(this.hitInterruptsCastingProbabilityMin, this.hitInterruptsCastingProbabilityMax) == true))
                        entity.HitInterruptAbilityActivation();


                    // stop originator from casting as he has just been hit (if chance is false or the dice roll meets the chance)
                    if (this.hitPreventsCasting == true && (this.hitPreventsCastingRandomChance == false || this.hitPreventsCastingRandomChance == true && ABC_Utilities.DiceRoll(this.hitPreventsCastingProbabilityMin, this.hitPreventsCastingProbabilityMax) == true))
                        entity.HitRestrictAbilityActivation();


                    //Calculate potency which includes any stat modifications (Stats can change during the repeats, which is why it's called everytime)
                    this.appliedEffectPotency = this.CalculatePotencyValue(entity, Originator, WasCriticalHit);
                    float potencyValue = this.appliedEffectPotency;

                    // If this is first time activation or it's repeated and was to repeat aesthetics
                    if (repeatCount == 0 || this.repeatAesthetics == true) {

                        //If critical then play audio sound
                        if (WasCriticalHit == true && this.potencyCriticalHitAudio.AudioClip != null)
                            AudioSource.PlayClipAtPoint(this.potencyCriticalHitAudio.AudioClip, Originator.Camera.transform.position, 1);

                        // activate graphic
                        ABC_Utilities.mbSurrogate.StartCoroutine(ActivateGraphic(entity, HitPoint));

                        // activate text graphic
                        ABC_Utilities.mbSurrogate.StartCoroutine(ActivateTextGraphic(entity, AbilityName, EffectAction.Add, Originator, WasCriticalHit));

                        // add to effect log
                        this.LogEffectText(entity, AbilityName, EffectAction.Add, Originator);
                    }


                    // stop originator from moving as he has just been hit by an effect (if chance is false or the dice roll meets the chance)
                    if (this.hitStopsMovement == true && (this.hitStopsMovementRandomChance == false || this.hitStopsMovementRandomChance == true && ABC_Utilities.DiceRoll(this.hitStopsMovementProbabilityMin, this.hitStopsMovementProbabilityMax) == true))
                        entity.HitStopMovement();

                    //Determine the event type of the activation - if raise event then call the delegate event
                    if (this.eventType == EffectEventType.RaiseEvent || this.eventType == EffectEventType.BothSimultaneously)
                        entity.RaiseEffectActivationEvent(this, Target, Originator);

                    //If event type is standard then run the code configured in the effect
                    if (this.eventType == EffectEventType.Standard || this.eventType == EffectEventType.BothSimultaneously) {
                        switch (this.effectName.Trim()) {
                            case "AdjustHealth":
                                // Effect: Will adjust current health (negative and positive values work)
                                // PotencyValue: Amount to adjust health by (can be positive or negative)
                                entity.AdjustHealth(potencyValue);
                                break;
                            case "AdjustMana":
                                // Effect: Will adjust current mana 
                                // PotencyValue: Amount to adjust mana by (can be positive or negative)
                                entity.AdjustMana(potencyValue);
                                break;
                            case "RandomHitAnimation":
                                // Effect: Will activate a random hit animation which has been setup
                                entity.RandomHitAnimationEffect();
                                break;
                            case "HitAnimation":
                                // Effect: Will activate a specific hit animation which has been setup
                                // MiscProperty: Name of animation to play
                                entity.HitAnimationEffect(this.miscellaneousProperty);
                                break;
                            case "SetStatValue":
                                // Effect: Will set the stat to the value provided
                                // PotencyValue: Amount to set stat too
                                // MiscProperty: Name of stat to change
                                entity.SetStatValue(this.miscellaneousProperty, potencyValue);
                                break;
                            case "AdjustStatValue":
                                // Effect: Will modify a stat value if it exists
                                // PotencyValue: Amount to change stat by (can be positive or negative)
                                // MiscProperty: Name of stat to change
                                entity.AdjustStatValue(this.miscellaneousProperty, potencyValue);
                                break;
                            case "ShakeCamera":
                                // Effect: Will shake the camera
                                // PotencyValue: Amount of shake to apply
                                // MiscProperty: Duration of the camera shake
                                // AltMiscProperty: Speed of the camera shake
                                entity.ShakeCamera(float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), potencyValue, float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "ShakeObject":
                                // Effect: Will shake the entity
                                // PotencyValue: The amount of shake performed 
                                // MiscProperty: The amount of the shake decreased each cycle. Also determines the duration, once shake decays to 0 the shake will stop
                                // AltMiscProperty: Delay of the shake object
                                entity.ShakeEntity(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "SwitchColor":
                                // Effect: Will activate a color switch for a duration before reverting back to the entites original color
                                // PotencyValue: Duration for the color switch before it reverts back
                                // MiscProperty: Hex code of the color to change to (i.e #880000)
                                // AltMiscProperty: (Boolean) if true then the emission color will be changed if enable, else the normal color will change

                                Color color;

                                if (ColorUtility.TryParseHtmlString(this.miscellaneousProperty, out color)) {
                                    entity.SwitchEntitiesColor(color, potencyValue, 0, bool.Parse(this.miscellaneousAltProperty));
                                }

                                break;
                            case "BulletTime":
                                // Effect: Will modify game speed 
                                // PotencyValue: Amount to slow down or speed up game
                                // MiscProperty: Duration of the game speed change  
                                // AltMiscProperty: Delay before game speed change
                                entity.ModifyGameSpeed(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "AdjustMaxHealth":
                                // Effect: Will adjust the entities max health 
                                // PotencyValue: Amount to increase max health by
                                // MiscProperty: (Boolean) if true then current health will be restored to full, else it will remain the same and only the max health will be adjusted
                                entity.AdjustMaxHealth(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "AdjustMaxMana":
                                // Effect: Will adjust the entities max mana 
                                // PotencyValue: Amount to increase max mana by
                                // MiscProperty: (Boolean) if true then current mana will be restored to full, else it will remain the same and only the max mana will be adjusted
                                entity.AdjustMaxMana(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "AdjustHealthRegen":
                                // Effect: Will adjust the entities health regen
                                // PotencyValue: Amount to increase health regen by
                                entity.AdjustHealthRegen(potencyValue);
                                break;
                            case "AdjustManaRegen":
                                // Effect: Will adjust the entities mana regen
                                // PotencyValue: Amount to increase mana regen by
                                entity.AdjustManaRegen(potencyValue);
                                break;
                            case "AdjustAbilityAmmo":
                                // Effect: Will adjust an abilities ammo count
                                // PotencyValue: Amount to adjust ammo by (can be positive or negative)
                                // MiscProperty: ID of ability whos ammo is being adjusted
                                entity.AdjustAbilityAmmo(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), (int)potencyValue);
                                break;
                            case "AdjustAbilityGlobalPrepareTime":
                                // Effect: Will adjust the prepare time of all abilities
                                // PotencyValue: % value to adjust global prepare time by (can be positive or negative)
                                entity.AdjustAbilityGlobalPrepareTime(potencyValue);
                                break;
                            case "AdjustAbilityGlobalCooldown":
                                // Effect: Will adjust the cooldown of all abilities
                                // PotencyValue: % value to adjust global cooldown by (can be positive or negative)
                                entity.AdjustAbilityGlobalCooldown(potencyValue);
                                break;
                            case "AdjustAbilityGlobalInitiationSpeed":
                                // Effect: Will adjust the initiation speed of all abilities
                                // PotencyValue: % value to adjust global cooldown by (can be positive or negative)
                                entity.AdjustAbilityGlobalInitiationSpeedAdjustment(potencyValue);
                                break;
                            case "AdjustAbilityGlobalCritChance":
                                // Effect: Will adjust the global ability crit chance
                                // PotencyValue: % value to adjust global crit chance by (can be positive or negative)
                                entity.AdjustAbilityGlobalCritChance(potencyValue);
                                break;
                            case "AdjustAbilityGlobalCritPotency":
                                // Effect: Will adjust the global ability crit Potency
                                // PotencyValue: % value to adjust global crit Potency by (can be positive or negative)
                                entity.AdjustAbilityGlobalCritPotency(potencyValue);
                                break;
                            case "TempAbilityActivationIntervalAdjustment":
                                // Effect: Will temporarily adjust the entities ability activation interval. This is reset once the interval wait is over.
                                // PotencyValue: value to adjust ability activation interval by (can be positive or negative)
                                entity.TemporarilyAdjustAbilityActivationInterval(potencyValue);
                                break;
                            case "AdjustMovementSpeed":
                                // Effect: Will temporarily adjust the entities movement speed (including AI)
                                // PotencyValue: value to adjust movement speed by (can be positive or negative)
                                entity.AdjustMovementSpeed(potencyValue);
                                break;
                            case "AddABCTag":
                                // Effect: Will add an ABC Tag
                                // MiscProperty: Name of the tag to add
                                entity.AddABCTag(this.miscellaneousProperty);
                                break;
                            case "RemoveABCTag":
                                // Effect: Will remove an ABC Tag
                                // MiscProperty: Name of the tag to remove
                                entity.RemoveABCTag(this.miscellaneousProperty);
                                break;
                            case "ToggleEffectProtection":
                                // Effect: Will enable/disable effect protection
                                // MiscProperty: (Boolean) if true then effect protection will be enabled, else disabled
                                entity.ToggleEffectProtection(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "ToggleAbilityActivation":
                                // Effect: will enable/disable ability activation
                                // MiscProperty: (Boolean) if true then ability activation will be enabled, else disabled
                                entity.ToggleAbilityActivation(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "DestroyEntity":
                                // Effect: will disable the gameobject                            
                                entity.DestroyEntity();
                                break;
                            case "ToggleIgnoreAbilityCollision":
                                // Effect: will enable or disable ignore ability collision on entity
                                // MiscProperty: (Boolean) if true then ability activation will be enabled, else disabled
                                entity.ToggleIgnoreAbilityCollision(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "DisplayHealthReductionImage":
                                // Effect: will show the health reduction image that has been setup                           
                                entity.ShowHealthReductionImage(true);
                                break;
                            case "DispelAbilityIDEffects":
                                // Effect: will dispel effects applied by the ability ID provided
                                // PotencyValue: ID of the ability whos effects will be removed
                                // MiscProperty: (Boolean) if true then dispellable restrictions will be ignored
                                entity.RemoveAbilitiesEffects((int)potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "DispelAbilityNameEffects":
                                // Effect: will dispel effects applied by the ability name provided
                                // MiscProperty: name of the ability whos effects will be removed
                                // AltMiscProperty: (Boolean) if true then dispellable restrictions will be ignored
                                entity.RemoveAbilitiesEffects(this.miscellaneousProperty, bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "RestrictMovement":
                                // Effect: will restrict movement by freezing the entity in it's position
                                entity.RestrictMovement();
                                break;
                            case "RestrictMovementComponents":
                                // Effect: will restrict movement by turning off common movement components                            
                                entity.RestrictMovementComponents();
                                break;
                            case "RestrictMovementRaiseEvent":
                                // Effect: Will raise the restrict movement event which other components can subscribe too
                                entity.ToggleMovementRaiseEvent(Time.time, false);
                                break;
                            case "EnableMovement":
                                // Effect: will enable movement by unfreezing the entity
                                entity.EnableMovement();
                                break;
                            case "EnableMovementComponents":
                                // Effect: will enable movement by turning back on any common movement componenets attached to the object                            
                                entity.EnableMovementComponents();
                                break;
                            case "EnableMovementRaiseEvent":
                                // Effect: Will raise the enable movement event which other componenets can subscribe too
                                entity.ToggleMovementRaiseEvent(Time.time, true);
                                break;
                            case "ToggleComponent":
                                // Effect: Will enable a component defined by a string
                                // MiscProperty: (String) Component name to enable or disable
                                // AltMiscProperty: (Boolean) enable (true) or disable (false) component
                                entity.ToggleComponent(this.miscellaneousProperty, bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "ToggleAI":
                                // Effect: will enable/disable ability AI
                                // MiscProperty: (Boolean) if true then ability AI will be enabled, else disabled
                                entity.ToggleAI(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TriggerAbility":
                                // Effect: will trigger the ability
                                // Potency: AbilityID to activate
                                entity.TriggerAbility(int.Parse(this.miscellaneousProperty));
                                break;
                            case "TriggerAbilityByName":
                                // Effect: will trigger the ability
                                // MiscProperty: Ability name to activate
                                entity.TriggerAbility(this.miscellaneousProperty);
                                break;
                            case "EnableAbility":
                                // Effect: will enable an ability
                                // MiscProperty: ID of the ability to enable
                                entity.EnableAbility(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                                break;
                            case "DisableAbility":
                                // Effect: will disable an ability
                                // MiscProperty: ID of the ability to disable
                                entity.DisableAbility(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                                break;
                            case "EquipScrollAbilityByID":
                                // Effect: will 'equip' a scroll ability
                                // MiscProperty: ID of the scroll ability to 'equip'
                                entity.EquipScrollAbilityByID(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                                break;
                            case "EquipScrollAbilityByName":
                                // Effect: will 'equip' a scroll ability
                                // Misc Property: name of the scroll ability to 'equip'
                                entity.EquipScrollAbilityByName(this.miscellaneousProperty);
                                break;
                            case "EquipNextScrollAbility":
                                // Effect: will 'equip' the next scroll ability in the list
                                // MiscProperty: (Boolean) if true then the scroll equip aesthetics will play, else they won't and the scroll ability will be equiped instantly
                                entity.EquipNextScrollAbility(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "EquipPreviousScrollAbility":
                                // Effect: will 'equip' the previous scroll ability in the list
                                // MiscProperty: (Boolean) if true then the scroll equip aesthetics will play, else they won't and the scroll ability will be equiped instantly
                                entity.EquipPreviousScrollAbility(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "DisableCurrentScrollAbility":
                                // Effect: will 'disable' the current scroll ability used by the entity
                                // MiscProperty: (Boolean) if true then the current scroll ability will only be disabled if x or greater scroll abilities are enabled
                                // AltMiscProperty: How many scroll abilities need to be currently enabled for the current one to be disabled (requires misc property to be true)
                                entity.DisableCurrentScrollAbility(bool.Parse(this.miscellaneousProperty), int.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "SwapCurrentScrollAbility":
                                // Effect: will 'swap' the current scroll ability with another scroll ability defined by ID. The current scroll ability will be disabled and the new scroll ability enabled
                                // PotencyValue: ID of the new scroll ability to enable and swap with the current scroll ability which will be disabled
                                // MiscProperty: (Boolean) if true then the current scroll ability will only be disabled if x or greater scroll abilities are enabled
                                // AltMiscProperty: How many scroll abilities need to be currently enabled for the current one to be disabled (requires misc property to be true)
                                entity.SwapCurrentScrollAbility((int)(potencyValue), bool.Parse(this.miscellaneousProperty), int.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "AdjustGroupEnablePoint":
                                // Effect: Will adjust group points so it can be setup to enable x attacks hits etc
                                // PotencyValue: Amount to adjust the group points by
                                // MiscProperty: ID of the group to adjust points for
                                entity.AdjustAbilityGroupEnablePoint(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), potencyValue);
                                break;
                            case "ToggleAbilityGroup":
                                // Effect: Will enable/disable the ability group linked to the ID provided
                                // MiscProperty: ID of the ability group to enable/disable
                                // AltMiscProperty: True to enable ability group, else false to disable
                                entity.ToggleAbilityGroup(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "Push":
                                // Effect: Will push the entity back
                                // PotencyValue: Distance to push back
                                // MiscProperty: How long it takes for entity to reach the push distance
                                // AltMiscProperty: The amount of lift produced by the push

                                //Work out the pushImpactPoint
                                Vector3 pushImpactPoint = HitPoint;

                                //If hitpoint not provided or ability is melee then use originator position
                                if (pushImpactPoint == Vector3.zero || HitPoint == Originator.transform.position || AbilityType == AbilityType.Melee)
                                    pushImpactPoint = Originator.transform.position;

                                entity.Push(potencyValue, pushImpactPoint, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "PushSide":
                                // Effect: Will push the entity to the right or left (side)
                                // PotencyValue: Distance to push to the side (positive is right, negative is left)
                                // MiscProperty: How long it takes for entity to reach the side push distance
                                // AltMiscProperty: The amount of lift produced by the push
                                entity.PushSide(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "PushRandomSide":
                                // Effect: Will push the entity randomly to either the right or left (side)
                                // PotencyValue: Distance to push to the random side
                                // MiscProperty: How long it takes for entity to reach the side push distance
                                // AltMiscProperty: The amount of lift produced by the push
                                entity.PushRandomSide(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "PushOnAxis":
                                // Effect: Pushes the entity on the Axis provided (X/Y/Z)
                                // PotencyValue: Distance to push on Axis
                                // MiscProperty: How long it takes for entity to reach the push distance
                                // AltMiscProperty: Which Axis to push on (X, Y, Z)
                                entity.PushOnAxis(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), this.miscellaneousAltProperty);
                                break;
                            case "Lift":
                                // Effect: Will lift the entity up
                                // PotencyValue: Distance to lift up
                                // MiscProperty: How long it takes for entity to reach the lift distance
                                entity.Lift(potencyValue, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                                break;
                            case "Pull":
                                // Effect: Will pull the entity forward
                                // PotencyValue: Distance to pull forward
                                // MiscProperty: How long it takes for entity to reach the pull distance
                                // AltMiscProperty: The amount of lift produced by the pull

                                //Work out the pullImpactPoint
                                Vector3 pullImpactPoint = HitPoint;

                                //If hitpoint not provided use originator position
                                if (pullImpactPoint == Vector3.zero || HitPoint == Originator.transform.position)
                                    pullImpactPoint = Originator.transform.position + Originator.transform.forward * 2;

                                entity.Pull(potencyValue, pullImpactPoint, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "GravitateToOriginator":
                                // Effect: Will gravitate the entity to the activating originator
                                // PotencyValue: How long it takes for the entity to reach the originator
                                // MiscProperty: How far infront of originator the entity will stop
                                // AltMiscProperty: How far to the side of the originator the entity will stop (positive for right, negative for left)

                                //Default Originator size
                                float originatorSize = 3f;

                                //If a collider exists on originator then use that to get size
                                Collider originatorCollider = Originator.gameObject.GetComponentInChildren<Collider>();
                                if (originatorCollider != null)
                                    originatorSize = originatorCollider.bounds.extents.magnitude + 0.5f;

                                entity.GravitateToOriginator(Originator, potencyValue, originatorSize, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;

                            case "GravitateToProjectile":
                                // Effect: Will gravitate the entity to the ability projectile (if no projectile exists like for raycasts then the entity will gravitate to the originator)
                                // PotencyValue: How long it takes for the entity to reach the projectile
                                // MiscProperty: How far randomly infront or behind of projectile the entity will stop
                                // AltMiscProperty: Projectile size override (If 0 then code will try and work out the size from the renderer)

                                //Destination is the projectile which collided to activate abilities
                                GameObject destination = Projectile;

                                //If projectile was not provided then this is a raycast so gravitate to the originator instead (as that's where the raycast came from)
                                if (destination == null)
                                    destination = Originator.gameObject;

                                //Default Projectile Size
                                float projectileSize = float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture);

                                // if a renderer exist use that to determine projectile size
                                Renderer projectileRenderer = Projectile.GetComponentInChildren<Renderer>();
                                if (projectileSize == 0 && projectileRenderer != null)
                                    projectileSize = projectileRenderer.bounds.extents.magnitude;

                                entity.GravitateToProjectile(destination, potencyValue, projectileSize, Random.Range(-float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture)), Random.Range(-projectileSize, projectileSize));
                                break;
                            case "LookAtOriginator":
                                // Effect: Will make the entity look at the activating originator
                                entity.LookAtOriginator(Originator);
                                break;
                            case "ExplosiveForce":
                                // Effect: Will apply an explosion for to the entities rigidbody
                                // PotencyValue: Explosion Force
                                // MiscProperty: Explosion Radius
                                // AltMiscProperty: Lift Force

                                //Work out the explosionImpactPoint
                                Vector3 explosionImpactPoint = HitPoint;

                                if (HitPoint == Originator.transform.position)
                                    explosionImpactPoint = Originator.transform.position + Originator.transform.forward * 2;

                                //If hitpoint not provided use originator position
                                if (explosionImpactPoint == Vector3.zero)
                                    explosionImpactPoint = Target.transform.position + Target.transform.forward * 2;

                                entity.ExplosionForce(potencyValue, explosionImpactPoint, float.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), float.Parse(this.miscellaneousAltProperty, CultureInfo.InvariantCulture));
                                break;
                            case "ToggleHover":
                                // Effect: will enable/disable hovering
                                // PotencyValue: Hover Distance
                                // MiscProperty: True if to enable hovering, else false to disable
                                entity.ToggleHover(bool.Parse(this.miscellaneousProperty), potencyValue);
                                break;
                            case "ToggleVisibility":
                                // Effect: will enable/disable renderer making entity invisible
                                // MiscProperty: False to enable invisibility, else false to make entity visible
                                entity.ToggleVisibility(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TeleportForward":
                                // Effect: Will teleport the entity forward
                                // PotencyValue: distance to teleport forward
                                entity.TeleportForward(potencyValue);
                                break;
                            case "TeleportSide":
                                // Effect: Will teleport the entity to the side
                                // PotencyValue: distance to teleport to the side (positive value for right, negative for left)
                                entity.TeleportSide(potencyValue);
                                break;
                            case "TeleportBehindCurrentTarget":
                                // Effect: Will teleport the entity behind it's current target
                                // PotencyValue: How far behind the current target the entity will teleport too
                                // Misc Property: If true then softtarget will be used as the current target, if a current target doesn't exist
                                entity.TeleportBehindCurrentTarget(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TeleportInfrontCurrentTarget":
                                // Effect: Will teleport the entity infront of it's current target
                                // PotencyValue: How far infront of the current target the entity will teleport too
                                // Misc Property: If true then softtarget will be used as the current target, if a current target doesn't exist
                                entity.TeleportInfrontCurrentTarget(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TeleportSideCurrentTarget":
                                // Effect: Will teleport the entity to the side of it's current target
                                // PotencyValue: How far to the side of the current target the entity will teleport too (positive for right, negative for left)
                                // Misc Property: If true then softtarget will be used as the current target, if a current target doesn't exist
                                entity.TeleportSideCurrentTarget(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TeleportRandomSideCurrentTarget":
                                // Effect: Will teleport the entity to a random side of it's current target
                                // PotencyValue: How far to the side of the current target the entity will teleport too
                                // Misc Property: If true then softtarget will be used as the current target, if a current target doesn't exist
                                entity.TeleportRandomSideCurrentTarget(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "AdjustGCStatValue":
                                // Effect: (GC Integration) Will modify a Game Creator stats value by the amount provided
                                // PotencyValue: Amount to change stat by (can be positive or negative)
                                // MiscProperty: Name of stat to change
                                entity.AdjustGCStatValue(this.miscellaneousProperty, potencyValue, GCStatType.Stat);
                                break;
                            case "AdjustGCAttributeValue":
                                // Effect: (GC Integration) Will modify a Game Creator stats attribute by the amount provided
                                // PotencyValue: Amount to change attribute by (can be positive or negative)
                                // MiscProperty: Name of attribute to change
                                entity.AdjustGCStatValue(this.miscellaneousProperty, potencyValue, GCStatType.Attribute);
                                break;
                            case "AddGCStatusEffect":
                                // Effect: (GC Integration) Will add a Game Creator status effect matching the status effect name/ID provided
                                // MiscProperty: Name of status effect to remove
                                entity.AddGCStatusEffect(this.miscellaneousProperty);
                                break;
                            case "RemoveGCStatusEffect":
                                // Effect: (GC Integration) Will remove a Game Creator status effect matching the status effect name/ID provided
                                // MiscProperty: Name of status effect to remove
                                entity.RemoveGCStatusEffect(this.miscellaneousProperty);
                                break;
                            case "GCAbilityCollisionTrigger":
                                // Effect: (GC Integration) Will set off the ABCAbilityCollision trigger on GC
                                // MiscProperty: (Optional) ABCAbilityCollision GC triggers can define to only be triggered by this effect if this effect misc property matches the misc property value on the GC trigger 
                                break;
                            case "AdjustGC2StatValue":
                                // Effect: (GC Integration) Will modify a Game Creator stats value by the amount provided
                                // PotencyValue: Amount to change stat by (can be positive or negative)
                                // MiscProperty: Name of stat to change
                                // AltMiscProperty: (Boolean) If true then stat value will have a modifier applied, else if false base value will change
                                entity.AdjustGC2StatValue(this.miscellaneousProperty, potencyValue, GCStatType.Stat, bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "AdjustGC2AttributeValue":
                                // Effect: (GC Integration) Will modify a Game Creator stats attribute by the amount provided
                                // PotencyValue: Amount to change attribute by (can be positive or negative)
                                // MiscProperty: Name of attribute to change
                                entity.AdjustGC2StatValue(this.miscellaneousProperty, potencyValue, GCStatType.Attribute);
                                break;
                            case "AdjustGlobalAbilityMissChance":
                                // Effect: Will adjust the global ability miss chance by by the value provided 
                                // PotencyValue: % value to adjust global ability miss chance by (can be positive or negative)
                                entity.AdjustGlobalAbilityMissChance(potencyValue);
                                break;
                            case "EquipWeaponByName":
                                // Effect: will equip a weapon
                                // Misc Property: name of the weapon to equip
                                // AltMiscProperty: (Boolean) If true then quick toggle will be applied, equipping the weapon instantly
                                entity.EquipWeapon(this.miscellaneousProperty, bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "EquipWeaponByID":
                                // Effect:will equip a weapon
                                // Misc Property: ID of the weapon to equip
                                // Alt Misc Property:(Boolean) If true then quick toggle will be applied, equipping the weapon instantly
                                entity.EquipWeapon(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "EquipNextWeapon":
                                // Effect: will equip the next weapon in the list
                                // MiscProperty: (Boolean) If true then quick toggle will be applied, equipping the weapon instantly
                                entity.EquipNextWeapon(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "EquipPreviousWeapon":
                                // Effect: will equip the previous weapon in the list
                                // MiscProperty: (Boolean) If true then quick toggle will be applied, equipping the weapon instantly
                                entity.EquipPreviousWeapon(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "EnableWeapon":
                                // Effect: will enable a Weapon
                                // PotencyValue: ID of the weapon to enable
                                // MiscProperty: (Boolean) if true then the weapon will also be equipped
                                entity.EnableWeapon(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "EnableWeaponByName":
                                // Effect: will enable a Weapon
                                // MiscProperty: String of the weapon to enable
                                // AltMiscProperty: (Boolean) If true then the weapon will also be equipped 
                                entity.EnableWeapon(this.miscellaneousProperty, bool.Parse(this.miscellaneousAltProperty));
                                break;
                            case "DisableWeapon":
                                // Effect: will disable a Weapon
                                // Misc Property: ID of the weapon to disable
                                entity.DisableWeapon(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                                break;
                            case "DisableWeaponByName":
                                // Effect: will enable a Weapon
                                // MiscProperty: String of the weapon to enable
                                entity.DisableWeapon(this.miscellaneousProperty);
                                break;
                            case "ToggleGravity":
                                // Effect: will enable/disable gravity
                                // MiscProperty: True to enable gravity, else false to disable gravity
                                entity.UseGravity(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "ToggleGravityRaiseEvent":
                                // Effect: will raise the enable/disable gravity event
                                // MiscProperty: True to enable gravity, else false to disable gravity
                                if (bool.Parse(this.miscellaneousProperty) == true)
                                    entity.RaiseEnableGravityEvent();
                                else
                                    entity.RaiseDisableGravityEvent();
                                break;
                            case "AdjustWeaponAmmo":
                                // Effect: Will adjust a weapons ammo count
                                // PotencyValue: Amount to adjust ammo by (can be positive or negative)
                                // MiscProperty: ID of weapon whos ammo is being adjusted
                                entity.AdjustWeaponAmmo(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture), (int)potencyValue);
                                break;
                            case "AdjustMaxBlockDurability":
                                // Effect: Will adjust the entities max block durability 
                                // PotencyValue: Amount to increase max block durability by
                                // MiscProperty: (Boolean) if true then current block durability will be restored to full, else it will remain the same and only the max block durability will be adjusted
                                entity.AdjustMaxBlockDurability(potencyValue, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "AdjustBlockDurability":
                                // Effect: Will adjust the entities current block durability
                                // PotencyValue: Amount to adjust block durability by (can be positive or negative)
                                entity.AdjustBlockDurability(potencyValue);
                                break;
                            case "AdjustBlockDurabilityRegen":
                                // Effect: Will adjust the entities block durability regen rate
                                // PotencyValue: Amount to adjust the block durability regen by (can be positive or negative)
                                entity.AdjustBlockDurabilityRegen(potencyValue);
                                break;
                            case "AdjustMitigateMeleeDamage":
                                // Effect: Will adjust the entities current melee damage mitigation percentage
                                // PotencyValue: Amount to adjust melee damage mitigation percentage by (can be positive or negative)
                                entity.AdjustMeleeDamageMitigationPercentage(potencyValue);
                                break;
                            case "AdjustMitigateProjectileDamage":
                                // Effect: Will adjust the entities current projectile damage mitigation percentage
                                // PotencyValue: Amount to adjust projectile damage mitigation percentage by (can be positive or negative)
                                entity.AdjustProjectileMitigationPercentage(potencyValue);
                                break;
                            case "TogglePreventMeleeEffects":
                                // Effect: will enable/disable prevent melee effects status
                                // MiscProperty: (Boolean) True to enable the prevention of melee effects, else false to allow melee effects
                                entity.TogglePreventMeleeEffects(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "TogglePreventProjectileEffects":
                                // Effect: will enable/disable prevent projectile effects status
                                // MiscProperty: (Boolean) True to enable the prevention of projectile effects, else false to allow projectile effects
                                entity.TogglePreventProjectileAndRayCastEffects(bool.Parse(this.miscellaneousProperty));
                                break;
                            case "EmeraldAIDamage":
                                // Effect: will damage the entity using Emerald AI (Call damage method in Emerald AI)
                                // PotencyValue: Amount of damage to apply
                                // MiscProperty: (Boolean) True if target type is player, else false for AI

                                //If misc field has not been field out then default to true
                                if (this.miscellaneousProperty == string.Empty)
                                    this.miscellaneousProperty = "True";

                                entity.ActivateEmeraldAIDamage((int)potencyValue, Originator, bool.Parse(this.miscellaneousProperty));
                                break;
                            case "AddNextEffectHere":
                                //Note to user, example of how to add new effects
                                break;
                            default:
                                //Debug.Log("Effect: " + effectName.ToString() + " was applied but there is no code to handle the effect");
                                break;
                        }

                    }
                }




                // if we are not repeating this effect then we can end the function here
                if (maxRepeats == 1)
                    break;

                // we are repeating so add 1 to the counter and wait for the repeat interval
                repeatCount += 1;

                yield return new WaitForSeconds(repeatInterval);
            }


            // add a tiny frame wait after effect stops 
            yield return new WaitForSeconds(0.1f);

        }

        /// <summary>
        /// Will dispel an effect, letting components know it should be removed instantly (through duration reached method)
        /// </summary>
        /// <param name="BypassDispelRestriction">(Optional) If true then the effect will be dispelled even if it's not set to be dispellable</param>
        public void Dispel(bool BypassDispelRestriction = false) {

            if (this.dispellable || BypassDispelRestriction)
                this.effectDispelled = true;

        }

        /// <summary>
        /// Returns a bool if the effect duration is up which implies it should be removed
        /// </summary>
        /// <param name="ActivationTime">The time in which the effect was first activated</param>
        /// <returns>True if duration is up, else false</returns>
        public bool DurationReached(float ActivationTime) {

            if (this.effectDispelled)
                return true;

            //If duration is -1 then it is infinite
            if (this.effectDuration == -1)
                return false;

            // effect has not been activated yet so the duration hasn't been reached 
            if (ActivationTime == 0f)
                return false;

            if (Time.time - ActivationTime > this.effectDuration + this.delay) {
                return true;
            } else {
                return false;
            }

        }


        /// <summary>
        /// Main function to deactivate the effect. Will activate graphics and apply effect remove logic to the target entity (reduce stats back etc), 
        /// </summary>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Target">Target entity which will recieve the effect</param>
        /// <param name="Originator">Originator who activated the ability which is now activating effects</param>
        public void Deactivate(string AbilityName, ABC_IEntity Target, ABC_IEntity Originator) {

            // if remove effect is not enabled then we can end the function here
            if (this.enableRemoveEffect == false)
                return;

            //determine who activate the effect on 
            List<ABC_IEntity> entitiesToActivateOn = new List<ABC_IEntity>();

            switch (this.activateOn) {
                case ActivateOn.Entity:
                    entitiesToActivateOn.Add(Target);
                    break;
                case ActivateOn.Originator:
                    entitiesToActivateOn.Add(Originator);
                    break;
                case ActivateOn.BothSimultaneously:
                    //Add both target and originator if they are not the same object
                    entitiesToActivateOn.Add(Target);
                    if (Target.gameObject != Originator.gameObject)
                        entitiesToActivateOn.Add(Originator);
                    break;

            }


            foreach (ABC_IEntity entity in entitiesToActivateOn) {

                //record the potency which was applied
                float potencyValue = this.appliedEffectPotency;

                // shows effect text - you can call this under any of the switch cases below instead if desired
                ABC_Utilities.mbSurrogate.StartCoroutine(this.ActivateTextGraphic(entity, AbilityName, EffectAction.Remove, Originator));


                // add to effect log 
                this.LogEffectText(entity, AbilityName, EffectAction.Remove, Originator);

                //reset the repeat count
                this.repeatCount = 0;

                //Determine the event type of the activation - if raise event then call the delegate event
                if (this.eventType == EffectEventType.RaiseEvent || this.eventType == EffectEventType.BothSimultaneously)
                    entity.RaiseEffectDeactivationEvent(this, entity, Originator);

                //If event type is standard then run the code configured in the effect
                if (this.eventType == EffectEventType.Standard || this.eventType == EffectEventType.BothSimultaneously) {
                    // logic to do effects
                    switch (this.effectName.Trim()) {
                        case "AdjustStatValue":
                            // Effect: Will modify a stat value if it exists
                            // PotencyValue: Amount to change stat by (can be positive or negative)
                            // MiscProperty: Name of stat to change
                            entity.AdjustStatValue(this.miscellaneousProperty, this.ReverseValue(potencyValue));
                            break;
                        case "AdjustMaxHealth":
                            // Effect: Will adjust the entities max health 
                            // PotencyValue: Amount to increase max health by
                            // MiscProperty: (Boolean) if true then current health will be restored to full, else it will remain the same and only the max health will be adjusted
                            entity.AdjustMaxHealth(this.ReverseValue(potencyValue), bool.Parse(this.miscellaneousProperty));
                            break;
                        case "AdjustMaxMana":
                            // Effect: Will adjust the entities max mana 
                            // PotencyValue: Amount to increase max mana by
                            // MiscProperty: (Boolean) if true then current mana will be restored to full, else it will remain the same and only the max mana will be adjusted
                            entity.AdjustMaxMana(this.ReverseValue(potencyValue), bool.Parse(this.miscellaneousProperty));
                            break;
                        case "AdjustHealthRegen":
                            // Effect: Will adjust the entities health regen
                            // PotencyValue: Amount to increase health regen by
                            entity.AdjustHealthRegen(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustManaRegen":
                            // Effect: Will adjust the entities mana regen
                            // PotencyValue: Amount to increase mana regen by
                            entity.AdjustManaRegen(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustAbilityGlobalPrepareTime":
                            // Effect: Will adjust the prepare time of all abilities
                            // PotencyValue: % value to adjust global prepare time by (can be positive or negative)
                            entity.AdjustAbilityGlobalPrepareTime(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustAbilityGlobalCooldown":
                            // Effect: Will adjust the cooldown of all abilities
                            // PotencyValue: % value to adjust global cooldown by (can be positive or negative)
                            entity.AdjustAbilityGlobalCooldown(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustAbilityGlobalInitiationSpeed":
                            // Effect: Will adjust the initiation speed of all abilities
                            // PotencyValue: % value to adjust global cooldown by (can be positive or negative)
                            entity.AdjustAbilityGlobalInitiationSpeedAdjustment(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustMovementSpeed":
                            // Effect: Will temporarily adjust the entities movement speed (including AI)
                            // PotencyValue: value to adjust movement speed by (can be positive or negative)
                            entity.AdjustMovementSpeed(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustAbilityGlobalCritChance":
                            // Effect: Will adjust the global ability crit chance
                            // PotencyValue: % value to adjust global crit chance by (can be positive or negative)
                            entity.AdjustAbilityGlobalCritChance(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustAbilityGlobalCritPotency":
                            // Effect: Will adjust the global ability crit Potency
                            // PotencyValue: % value to adjust global crit Potency by (can be positive or negative)
                            entity.AdjustAbilityGlobalCritPotency(this.ReverseValue(potencyValue));
                            break;
                        case "AddABCTag":
                            // Effect: Will add an ABC Tag
                            // MiscProperty: Name of the tag to add
                            entity.RemoveABCTag(this.miscellaneousProperty);
                            break;
                        case "RemoveABCTag":
                            // Effect: Will remove an ABC Tag
                            // MiscProperty: Name of the tag to remove
                            entity.AddABCTag(this.miscellaneousProperty);
                            break;
                        case "ToggleEffectProtection":
                            // Effect: Will enable/disable effect protection
                            // MiscProperty: (Boolean) if true then effect protection will be enabled, else disabled
                            entity.ToggleEffectProtection(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "ToggleAbilityActivation":
                            // Effect: will enable/disable ability activation
                            // MiscProperty: (Boolean) if true then ability activation will be enabled, else disabled
                            entity.ToggleAbilityActivation(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "ToggleIgnoreAbilityCollision":
                            // Effect: will enable or disable ignore ability collision on entity
                            // MiscProperty: (Boolean) if true then ability activation will be enabled, else disabled
                            entity.ToggleIgnoreAbilityCollision(ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "RestrictMovement":
                            // Effect: will unrestrict movement again
                            entity.EnableMovement();
                            break;
                        case "RestrictMovementComponents":
                            // Effect: will unrestrict movement by turning on common movement components                            
                            entity.EnableMovementComponents();
                            break;
                        case "RestrictMovementRaiseEvent":
                            // Effect: Will raise the restrict movement event which other componenets can subscribe too
                            entity.ToggleMovementRaiseEvent(Time.time, true);
                            break;
                        case "EnableMovement":
                            // Effect: will disable movement by unfreezing the entity
                            // MiscProperty: (Boolean) if true then ability activation will be enabled, else disabled
                            entity.RestrictMovement();
                            break;
                        case "EnableMovementComponents":
                            // Effect: will disable movement by turning back on any common movement componenets attached to the object                            
                            entity.RestrictMovementComponents();
                            break;
                        case "EnableMovementRaiseEvent":
                            // Effect: Will raise the disable movement event which other componenets can subscribe too
                            entity.ToggleMovementRaiseEvent(Time.time, false);
                            break;
                        case "ToggleComponent":
                            // Effect: Will enable a component defined by a string
                            // MiscProperty: (String) Component name to enable or disable
                            // AltMiscProperty: (Boolean) enable (true) or disable (false) component
                            entity.ToggleComponent(this.miscellaneousProperty, ReverseBoolean(bool.Parse(this.miscellaneousAltProperty)));
                            break;
                        case "ToggleAI":
                            // Effect: will enable/disable ability AI
                            // MiscProperty: (Boolean) if true then ability AI will be enabled, else disabled
                            entity.ToggleAI(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "EnableAbility":
                            // Effect: will enable an ability
                            // MiscProperty: ID of the ability to enable
                            entity.DisableAbility(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                            break;
                        case "DisableAbility":
                            // Effect: will disable an ability
                            // MiscProperty: ID of the ability to disable
                            entity.EnableAbility(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                            break;
                        case "ToggleAbilityGroup":
                            // Effect: Will enable/disable the ability group linked to the ID provided
                            // PotencyValue: ID of the ability group to enable/disable
                            // MiscProperty: True to enable ability group, else false to disable
                            entity.ToggleAbilityGroup((int)(potencyValue), this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "ToggleHover":
                            // Effect: will enable/disable hovering
                            // PotencyValue: Hover Distance
                            // MiscProperty: True if to enable hovering, else false to disable
                            entity.ToggleHover(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "ToggleVisibility":
                            // Effect: will enable/disable renderer making entity invisible
                            // MiscProperty: False to enable invisibility, else false to make entity visible
                            entity.ToggleVisibility(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "AdjustGCStatValue":
                            // Effect: (GC Integration) Will modify a Game Creator stats value by the amount provided
                            // PotencyValue: Amount to change stat by (can be positive or negative)
                            // MiscProperty: Name of stat to change
                            entity.AdjustGCStatValue(this.miscellaneousProperty, this.ReverseValue(potencyValue), GCStatType.Stat);
                            break;
                        case "AdjustGCAttributeValue":
                            // Effect: (GC Integration) Will modify a Game Creator attribute value by the amount provided
                            // PotencyValue: Amount to change attribute by (can be positive or negative)
                            // MiscProperty: Name of attribute to change
                            entity.AdjustGCStatValue(this.miscellaneousProperty, this.ReverseValue(potencyValue), GCStatType.Attribute);
                            break;
                        case "AddGCStatusEffect":
                            // Effect: (GC Integration) Will add a Game Creator status effect matching the status effect name/ID provided
                            // MiscProperty: Name of status effect to remove
                            entity.RemoveGCStatusEffect(this.miscellaneousProperty);
                            break;
                        case "RemoveGCStatusEffect":
                            // Effect: (GC Integration) Will remove a Game Creator status effect matching the status effect name/ID provided
                            // MiscProperty: Name of status effect to remove
                            entity.AddGCStatusEffect(this.miscellaneousProperty);
                            break;
                        case "AdjustGC2StatValue":
                            // Effect: (GC 2 Integration) Will modify a Game Creator stats value by the amount provided
                            // PotencyValue: Amount to change stat by (can be positive or negative)
                            // MiscProperty: Name of stat to change
                            entity.AdjustGC2StatValue(this.miscellaneousProperty, this.ReverseValue(potencyValue), GCStatType.Stat, true);
                            break;
                        case "AdjustGC2AttributeValue":
                            // Effect: (GC 2 Integration) Will modify a Game Creator stats attribute by the amount provided
                            // PotencyValue: Amount to change attribute by (can be positive or negative)
                            // MiscProperty: Name of attribute to change
                            entity.AdjustGC2StatValue(this.miscellaneousProperty, this.ReverseValue(potencyValue), GCStatType.Attribute);
                            break;
                        case "AdjustGlobalAbilityMissChance":
                            // Effect: Will adjust the global ability miss chance by by the value provided 
                            // PotencyValue: % value to adjust global ability miss chance by (can be positive or negative)
                            entity.AdjustGlobalAbilityMissChance(this.ReverseValue(potencyValue));
                            break;
                        case "EnableWeapon":
                            // Effect: will enable a Weapon
                            // MiscProperty: ID of the weapon to enable
                            entity.DisableWeapon(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                            break;
                        case "EnableWeaponByName":
                            // Effect: will enable a Weapon
                            // MiscValue: Name of the weapon to enable
                            entity.DisableWeapon(this.miscellaneousProperty);
                            break;
                        case "DisableWeapon":
                            // Effect: will disable a Weapon
                            // MiscProperty: ID of the weapon to disable
                            entity.EnableWeapon(int.Parse(this.miscellaneousProperty, CultureInfo.InvariantCulture));
                            break;
                        case "DisableWeaponByName":
                            // Effect: will disable a Weapon
                            // MiscValue: Name of the weapon to enable
                            entity.EnableWeapon(this.miscellaneousProperty);
                            break;
                        case "ToggleGravity":
                            // Effect: will enable/disable gravity
                            // MiscProperty: True to enable gravity, else false to disable gravity
                            entity.UseGravity(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "ToggleGravityRaiseEvent":
                            // Effect: will enable/disable gravity
                            // MiscProperty: True to enable gravity, else false to disable gravity
                            if (bool.Parse(this.miscellaneousProperty) == true)
                                entity.RaiseDisableGravityEvent();
                            else
                                entity.RaiseEnableGravityEvent();
                            break;
                        case "AdjustMaxBlockDurability":
                            // Effect: Will adjust the entities max block durability 
                            // PotencyValue: Amount to increase max block durability by
                            // MiscProperty: (Boolean) if true then current block durability will be restored to full, else it will remain the same and only the max block durability will be adjusted
                            entity.AdjustMaxBlockDurability(this.ReverseValue(potencyValue), bool.Parse(this.miscellaneousProperty));
                            break;
                        case "AdjustBlockDurability":
                            // Effect: Will adjust the entities current block durability
                            // PotencyValue: Amount to adjust block durability by (can be positive or negative)
                            entity.AdjustBlockDurability(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustBlockDurabilityRegen":
                            // Effect: Will adjust the entities block durability regen rate
                            // PotencyValue: Amount to adjust the block durability regen by (can be positive or negative)
                            entity.AdjustBlockDurabilityRegen(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustMitigateMeleeDamage":
                            // Effect: Will adjust the entities current melee damage mitigation percentage
                            // PotencyValue: Amount to adjust melee damage mitigation percentage by (can be positive or negative)
                            entity.AdjustMeleeDamageMitigationPercentage(this.ReverseValue(potencyValue));
                            break;
                        case "AdjustMitigateProjectileDamage":
                            // Effect: Will adjust the entities current projectile damage mitigation percentage
                            // PotencyValue: Amount to adjust projectile damage mitigation percentage by (can be positive or negative)
                            entity.AdjustProjectileMitigationPercentage(this.ReverseValue(potencyValue));
                            break;
                        case "TogglePreventMeleeEffects":
                            // Effect: will enable/disable prevent melee effects status
                            // MiscProperty: True to enable the prevention of melee effects, else false to allow melee effects
                            entity.TogglePreventMeleeEffects(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "TogglePreventProjectileEffects":
                            // Effect: will enable/disable prevent projectile effects status
                            // MiscProperty: True to enable the prevention of projectile effects, else false to allow projectile effects
                            entity.TogglePreventProjectileAndRayCastEffects(this.ReverseBoolean(bool.Parse(this.miscellaneousProperty)));
                            break;
                        case "AddNextDeactivateEffectHere":
                            //Note to user, example of how to add new effects
                            break;
                        default:
                            //Debug.Log("Deactivate Effect: " + effectName.ToString() + " was applied but there is no code to handle the effect");
                            break;
                    }
                }
            }




        }



        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Replaces any placeholders setup by the user when any text regarding the effect is displayed
        /// </summary>
        /// <param name="Text">Text which contains the placeholder we want to replace</param>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Target">Target object which the effect is being applied on</param>
        /// <param name="Originator">(Optional) the originator who activated the ability which is now applying effects</param>
        /// <returns>String of the text with all the placeholders replaced with the correct values</returns>
        private string ReplacePlaceHolders(string Text, string AbilityName, ABC_IEntity Target, ABC_IEntity Originator = null) {


            // If originator is not provided then we will just replace target with 'the originator'
            if (Originator == null) {
                Text = Text.Replace("#Origin#", "The originator");
            } else {
                Text = Text.Replace("#Origin#", Originator.gameObject.name);
            }

            Text = Text.Replace("#Target#", Target.transform.name);
            Text = Text.Replace("#Potency#", Mathf.Abs(this.appliedEffectPotency).ToString()); // always return the positive number (ABC takes 70 damage)
            Text = Text.Replace("#Ability#", AbilityName);
            Text = Text.Replace("#Effect#", this.effectName);

            return Text;

        }


        /// <summary>
        /// Will retrieve an effect graphic from the ABC pool and activate it in game at the target/position provided. 
        /// </summary>
        /// <param name="Target">Target we are activating the graphic on (if set correctly the graphic may become a child of this target)</param>
        /// <param name="HitPoint">(Optional) Vector3 position where effect will appear, if none is provided then it will appear at target location</param>
        private IEnumerator ActivateGraphic(ABC_IEntity Target, Vector3 HitPoint = default(Vector3)) {


            // If this effect is not set to activate or the originator is terrain then return null and play no graphic
            if (this.playEffect == false || Terrain.activeTerrain != null && Target.transform == Terrain.activeTerrain.gameObject)
                yield break;

            // wait for delay before playing graphic
            yield return new WaitForSeconds(this.effectGraphicDelay);

            // get graphic object 
            GameObject graphicObj = this.effectPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

            //If no graphics are avaliable create a new one 
            if (graphicObj == null)
                graphicObj = this.CreateEffectObjects(true);

            if (graphicObj != null && this.scaleEffectGraphic == true)
                graphicObj.transform.localScale = new Vector3(this.effectGraphicScale, this.effectGraphicScale, this.effectGraphicScale);

            bool GC2Mode = false;

#if ABC_GC_2_Integration
        if (Target!= null && Target.HasGC2CharacterComponent())
            GC2Mode = true;
#endif



            // if a hit point is giving and the right setting is true then play the effect at the hit point
            if (HitPoint != Vector3.zero && this.effectOnHitPosition == true) {

                Transform nearestBone = ABC_Utilities.GetClosestBoneFromPosition(Target, HitPoint);

                //If nearest bone was found then place graphic on it's position
                if (nearestBone != null)
                    graphicObj.transform.position = nearestBone.position;
                else // else default to targets position
                    graphicObj.transform.position = Target.transform.position;

            } else {
                // else just play at the targets position 
                graphicObj.transform.position = Target.transform.position;
            }

#if ABC_GC_2_Integration
        if (GC2Mode == true)
            graphicObj.transform.position += new Vector3(0, -1, 0);
#endif

            //Add offset
            graphicObj.transform.position = graphicObj.transform.position + effectGraphicOffset + graphicObj.transform.forward * effectGraphicForwardOffset + graphicObj.transform.right * effectGraphicRightOffset;




            // change rotation to originator
            graphicObj.transform.rotation = Target.transform.rotation;

            // if effect is to follow target then make it a child in the targets hiearchy
            if (this.effectFollowTarget) {
                graphicObj.transform.parent = Target.transform;
            } else {
                // else take it out of pool
                graphicObj.transform.parent = null;
            }


            // play effect
            graphicObj.SetActive(true);

            // disable graphic and place back into the pool after duration

            for (var i = this.effectGraphicDuration; i > 0; i--) {

                //If effect has been dispelled then end early
                if (this.effectDispelled)
                    break;

                // actual wait time foreffect graphic
                if (i < 1) {
                    // less then 1 second so we only need to wait the .xx time
                    yield return new WaitForSeconds(i);
                } else {
                    // wait a second and keep looping till casting time = 0; 
                    yield return new WaitForSeconds(1);
                }
            }



            ABC_Utilities.PoolObject(graphicObj);

        }


        /// <summary>
        /// Depending on the effect action (Add/Remove) will retrieve the Text Graphic from the target's setup and will activate it in game with the effect add/remove text 
        /// </summary>
        /// <param name="Target">Target we are activating graphic text from.</param>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Action">Effect action (Add/Remove)</param>        
        /// <param name="Originator">(Optional) the originator who activated the ability which is now applying effects</param>
        /// <param name="WasCriticalHit">(Optional) If effect was a critical hit</param>
        private IEnumerator ActivateTextGraphic(ABC_IEntity Target, string AbilityName, EffectAction Action, ABC_IEntity Originator = null, bool WasCriticalHit = false) {

            //variables used for effect text graphics
            string text = "";
            Color col = new Color();
            float duration = 3f;



            switch (Action) {
                case EffectAction.Add:

                    // if setting has not allowed effect text to show then return here 
                    if (this.addToEffectText == false)
                        yield break;

                    text = this.ReplacePlaceHolders(this.effectText, AbilityName, Target, Originator);
                    col = new Color(this.effectTextColour.r, this.effectTextColour.g, this.effectTextColour.b);

                    //If critical hit then change color
                    if (WasCriticalHit)
                        col = this.potencyCriticalHitColor;

                    duration = this.effectTextDuration;

                    break;

                case EffectAction.Remove:

                    // if setting has not allowed effect text to show then return here 
                    if (this.removeEffectAddToEffectText == false)
                        yield break;

                    text = this.ReplacePlaceHolders(this.removeEffectText, AbilityName, Target, Originator);
                    col = new Color(this.removeEffectTextColour.r, this.removeEffectTextColour.g, this.removeEffectTextColour.b);
                    duration = this.removeEffectTextDuration;

                    break;


            }


            Target.DisplayTextGraphic(text, duration, col, Originator);




        }

        /// <summary>
        /// Will calculate the potency value taking into consideration any modifications from stats (strength, intelligence etc)
        /// </summary>
        /// <param name="Target">Target entity which will recieve the effect</param>
        /// <param name="Originator">Originator who activated the ability which is now activating effects</param>
        /// <param name="WasCriticalHit">Determines if the potency has been modified by a critical hit</param>
        /// <returns>Final Potency Value</returns>
        private float CalculatePotencyValue(ABC_IEntity Target, ABC_IEntity Originator, bool WasCriticalHit) {


            //Default the return value to the configured potency
            float retval = this.potency;

            //If potency is not set to be modified from stats then return the value here
            if (this.modifyPotencyUsingStats == true) {

                //else loop through stat modifications caluclating the new potency 
                foreach (PotencyStatModifications statMod in this.potencyStatModifications) {

                    //Retrieve the stat modification value from the originator and/or target entity (uses the percentage configured - 70% of intelligence etc)
                    float statModification = 0;

                    switch (statMod.statSource) {
                        case ActivateOn.Entity:
                            statModification += statMod.percentageValue / 100 * Target.GetStatValue(statMod.statName, statMod.statIntegrationType);
                            break;
                        case ActivateOn.Originator:
                            statModification += statMod.percentageValue / 100 * Originator.GetStatValue(statMod.statName, statMod.statIntegrationType);
                            break;
                        case ActivateOn.BothSimultaneously:
                            //Add both target and originator if they are not the same object
                            statModification += statMod.percentageValue / 100 * Originator.GetStatValue(statMod.statName, statMod.statIntegrationType);

                            if (Target.gameObject != Originator.gameObject)
                                statModification += statMod.percentageValue / 100 * Target.GetStatValue(statMod.statName, statMod.statIntegrationType);

                            break;
                    }

                    //If modification value hasn't changed then continue to next stat modification in loop
                    if (statModification == 0)
                        continue;

                    //Apply the modification to the potency depending on the operator setup
                    switch (statMod.arithmeticOperator) {
                        case ArithmeticOperators.Add:
                            retval += statModification;
                            break;
                        case ArithmeticOperators.Minus:
                            retval -= statModification;
                            break;
                        case ArithmeticOperators.Divide:
                            retval /= statModification;
                            break;
                        case ArithmeticOperators.Multiply:
                            retval *= statModification;
                            break;
                    }

                }
            }

            //If enhance potency from critical hit 
            if (this.enhancePotencyFromCriticalHit == true && WasCriticalHit == true) {

                //Work out how much we adjusting the potency by (base + stat modification)
                float critPercentageIncrease = this.potencyCriticalHitBasePercentageEnhancement 
                    + (this.potencyCriticalHitEnablePercentageEnhancementStatModification == false ? 0 : ABC_Utilities.PercentageOfValue(this.potencyCriticalHitPercentageEnhancementStatModification.percentageValue, Originator.GetStatValue(potencyCriticalHitPercentageEnhancementStatModification.statName, potencyCriticalHitPercentageEnhancementStatModification.statIntegrationType)))
                    + Originator.GetAbilityGlobalCritPotencyAdjustment();

                //Adjust potency by the amount 
                retval += ABC_Utilities.PercentageOfValue(critPercentageIncrease, retval);

            }


            //if effect is a adjust health effect with a negative potency then damage is being applied so modify potency depending on mitigated value (if mitigated value is higher then 0)
            if (this.potency < 0 && this.effectName.Trim() == "AdjustHealth") {

                //If melee and melee mitigation is higher then 0 then reduce the potency 
                if (this.effectAbilityType == AbilityType.Melee && Target.meleeDamageMitigationPercentage > 0) {

                    retval -= Target.meleeDamageMitigationPercentage / 100 * retval;

                } else if (Target.projectileDamageMitigationPercentage > 0) {
                    //else reduce projectile/raycast potency if mitigation is higher then 0
                    retval -= Target.projectileDamageMitigationPercentage / 100 * retval;
                }


            }


            //if adjusting value then keep the potency in the right minus/positive i.e if -70 adjust health + 80 defence would heal for 10  
            if (this.effectName.Trim().Contains("Adjust")) {

                if (this.potency < 0 && retval > 0) {
                    retval = 0;
                } else if (this.potency > 0 && retval < 0) {
                    retval = 0;
                }

                //Get rid of Decimal 
                retval = (float)(int)retval;

            }



            //Return the potency
            return retval;
        }

        /// <summary>
        /// Will reverse the boolean value provided
        /// </summary>
        /// <param name="BoolValue">Value to reverse</param>
        /// <returns>Returns true if value provided was false and false if the value provided was true</returns>
        private bool ReverseBoolean(bool BoolValue) {

            if (BoolValue == true)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Will reverse the value provided so if it's positive it will return negative and vice versa
        /// </summary>
        /// <param name="Value">Value to convert</param>
        /// <returns>Negative if positive number provided, positive if negative number provided</returns>
        private float ReverseValue(float Value) {

            return Value * -1;

        }

        #endregion



    }

    // ********************* ENUMS ********************

    #region ENUMS

    /// <summary>
    /// Effect actions: Remove, Add, OutOfRange, No Effect. 
    /// </summary>
    public enum EffectAction {
        Remove,
        Add,
        OutOfRange,
        NoEffect
    }

    public enum ArithmeticComparisons {
        GreaterThan,
        LessThan
    }

    public enum ArithmeticOperators {
        Add,
        Minus,
        Divide,
        Multiply
    }

    public enum ArithmeticIncrDecrOperators {
        Increase,
        Decrease
    }

    public enum PercentOrBase {
        Percent,
        Base
    }


    /// <summary>
    /// Who the effect should apply and activate on
    /// </summary>
    /// <remarks>
    /// Entity - Will apply the effect on the entity if hit by the ability. 
    /// Originator - Will apply the effect on the originator when the ability hits an entity. 
    /// BothSimultaneously - Will apply the effect simultaneously on both the entity and originator when the ability hits an entity. 
    /// </remarks>
    public enum ActivateOn {
        Entity,
        Originator,
        BothSimultaneously,
    }


    /// <summary>
    /// Predefined Effects. More can be added but effects can also be added by just typing in the textbox during setup
    /// </summary>
    public enum enumEffects {
        New,
        AdjustHealth,
        AdjustMana,
        RandomHitAnimation,
        HitAnimation,
        AdjustStatValue,
        ShakeCamera,
        BulletTime,
        AdjustMaxHealth,
        AdjustMaxMana,
        AdjustHealthRegen,
        AdjustManaRegen,
        AdjustAbilityAmmo,
        AdjustAbilityGlobalPrepareTime,
        AdjustAbilityGlobalCooldown,
        TempAbilityActivationIntervalAdjustment,
        AddABCTag,
        RemoveABCTag,
        ToggleEffectProtection,
        ToggleAbilityActivation,
        DestroyEntity,
        ToggleIgnoreAbilityCollision,
        DisplayHealthReductionImage,
        DispelAbilityIDEffects,
        DispelAbilityNameEffects,
        RestrictMovement,
        RestrictMovementComponents,
        RestrictMovementRaiseEvent,
        EnableMovement,
        EnableMovementComponents,
        EnableMovementRaiseEvent,
        ToggleComponent,
        ToggleAI,
        TriggerAbility,
        TriggerAbilityByName,
        EnableAbility,
        DisableAbility,
        EquipScrollAbilityByID,
        EquipScrollAbilityByName,
        EquipNextScrollAbility,
        EquipPreviousScrollAbility,
        DisableCurrentScrollAbility,
        SwapCurrentScrollAbility,
        AdjustGroupEnablePoint,
        ToggleAbilityGroup,
        Push,
        PushSide,
        PushRandomSide,
        Lift,
        Pull,
        GravitateToOriginator,
        GravitateToProjectile,
        LookAtOriginator,
        ExplosiveForce,
        ToggleVisibility,
        ToggleHover,
        TeleportForward,
        TeleportSide,
        TeleportBehindCurrentTarget,
        TeleportInfrontCurrentTarget,
        TeleportSideCurrentTarget,
        TeleportRandomSideCurrentTarget,
        AdjustGCStatValue,
        AdjustGCAttributeValue,
        AddGCStatusEffect,
        RemoveGCStatusEffect,
        GCAbilityCollisionTrigger,
        AdjustGlobalAbilityMissChance,
        EquipWeaponByID,
        EquipWeaponByName,
        EquipNextWeapon,
        EquipPreviousWeapon,
        EnableWeapon,
        EnableWeaponByName,
        DisableWeapon,
        DisableWeaponByName,
        ToggleGravity,
        ToggleGravityRaiseEvent,
        AdjustWeaponAmmo,
        AdjustMaxBlockDurability,
        AdjustBlockDurability,
        AdjustBlockDurabilityRegen,
        AdjustMitigateMeleeDamage,
        AdjustMitigateProjectileDamage,
        TogglePreventMeleeEffects,
        TogglePreventProjectileEffects,
        EmeraldAIDamage,
        ShakeObject,
        SwitchColor,
        AdjustAbilityGlobalInitiationSpeed,
        PushOnAxis,
        SetStatValue,
        AdjustGC2StatValue,
        AdjustGC2AttributeValue,
        AdjustMovementSpeed,
        AdjustAbilityGlobalCritChance, 
        AdjustAbilityGlobalCritPotency
    }


    public enum EffectEventType {
        Standard,
        RaiseEvent,
        BothSimultaneously
    }

    #endregion
}