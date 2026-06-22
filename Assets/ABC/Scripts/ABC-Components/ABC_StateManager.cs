using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace ABCToolkit {

    /// <summary>
    /// Component which handles Health, Active Effects, ABC Tags and other minor settings on the Entity. All objects which are involved in ABC in anyway is required to have this component attached. 
    /// </summary>
    /// <remarks>
    /// Without this component then effects can not be activated and other funcitonality like becoming a surrounding object will not work. This component also holds settings which can block certain functionality. Useful if you don't want a boss
    /// to be used as a surrounding object or is only allowed one type of effect at one time. 
    /// </remarks>
    public class ABC_StateManager : MonoBehaviour {

        // ************ Nested Classes *****************************

        #region Nested Classes

        /// <summary>
        ///  Class for handling health GUI interfaces like sliders and text. Class is only used and managed by StateManager Component.
        /// </summary>
        [System.Serializable]
        public class EntityStat {

            // ************ Settings *****************************

            #region Settings For Stats

            /// <summary>
            /// Used by inspector only - determines if the effect settings are collapsed out or not 
            /// </summary>
            public bool foldOut = false;

            /// <summary>
            /// Stat Name (Intelligence, Agility etc)
            /// </summary>
            public string statName;

            /// <summary>
            /// Stat Value
            /// </summary>
            public float statValue;

            /// <summary>
            /// Text object which will show stat name
            /// </summary>
            public ABC_TextReference textStatName;

            /// <summary>
            /// Text object which will show stat value
            /// </summary>
            public ABC_TextReference textStatValue;

            /// <summary>
            /// Variable determining if text is showing
            /// </summary>
            public bool textShowing = false;


            /// <summary>
            /// If true then the text will only show when the entity is selected. 
            /// </summary>
            public bool onlyShowTextWhenSelected = false;


            #endregion

            // ************************** Variables *************************************

            #region Variables

            /// <summary>
            /// Records how much increase has been applied. This is added onto the base value
            /// </summary>
            private float statIncreaseValue;

            #endregion


            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Create new object 
            /// </summary>
            public EntityStat() {

            }

            /// <summary>
            /// Returns the stat value taking into consideration any boosts
            /// </summary>
            /// <returns>Value of the stat</returns>
            public float GetValue() {
                return this.statValue + this.statIncreaseValue;
            }

            /// <summary>
            /// Sets the stat to the value provided
            /// </summary>
            /// <param name="Value">Value to set the stat too</param>
            public float SetValue(float Value) {
                return this.statIncreaseValue = Value;
            }

            /// <summary>
            /// Will modify the value by the amount provided
            /// </summary>
            /// <param name="Amount">Amount to increase or decrease the stat value by</param>
            public void AdjustValue(float Amount) {
                this.statIncreaseValue += Amount;
            }


            /// <summary>
            /// Will reset the stat to it's base value
            /// </summary>
            public void ResetValue() {
                this.statIncreaseValue = 0f;
            }

            #endregion



        }


        /// <summary>
        ///  Class for handling which how many entites can target us and which entities are currently targeting
        /// </summary>
        [System.Serializable]
        public class TargeterLimitation {

            // ************ Settings *****************************

            #region Settings

            /// <summary>
            /// If enabled then target limitations are in, which stops more then X amount of entites from targeting us at once
            /// </summary>
            public bool enableTargeterLimit = true;

            /// <summary>
            /// Tags of the entities which this limitation affects
            /// </summary>
            public List<string> targeterTags = new List<string>();

            /// <summary>
            /// The max number of entities which can target us at one time
            /// </summary>
            public int maxNumberOfTargeters = 2;

            /// <summary>
            /// If true then the current targetters will have their targets removed every x interval
            /// </summary>
            public bool enableCurrentTargeterResets = false;

            /// <summary>
            /// The interval between resetting the current targetters
            /// </summary>
            public float resetCurrentTargetersInterval = 40f;


            #endregion

            // ************************** Variables *************************************

            #region Variables

            /// <summary>
            /// A list of game objects which currently have us as a target 
            /// </summary>
            private List<GameObject> currentTargeters = new List<GameObject>();

            /// <summary>
            /// A list of game objects which have priority targeting, if an object is in this list then they will take priority and can always target us removing any non priority targetters to keep within the limit
            /// </summary>
            private List<GameObject> priorityTargeters = new List<GameObject>();

            /// <summary>
            /// The game time where the targetters last had their targets reset
            /// </summary>
            private float lastResetTime = 0f;

            #endregion

            // ************************** Private Methods *************************************

            #region Private Methods

            /// <summary>
            /// Will make sure that the current targeters does not exceed the max number due to a bug or for any other reason
            /// </summary>
            private void CurrentTargetersCleaner() {

                //If for any reason, bug or otherwise the current targeters exceeds the max number allowed then remove the first and oldest
                if (this.currentTargeters.Count() > this.maxNumberOfTargeters)
                    this.currentTargeters.RemoveAt(0);

            }

            #endregion


            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Create new object 
            /// </summary>
            public TargeterLimitation() {

            }


            /// <summary>
            /// Determines if the targeter provided can target this entity
            /// </summary>
            /// <param name="Targeter">Object which is attempting to target us</param>
            /// <returns>True if the targeter provided can target this entity, else false</returns>
            public bool CanBeTargetedBy(GameObject Targeter) {

                //If the entity is already at limit and the Targeter provided isn't already in the list return false
                if (this.IsAtTargeterLimit() && this.ContainsTargeter(Targeter) == false && this.priorityTargeters.Contains(Targeter) == false)
                    return false;


                return true;

            }

            /// <summary>
            /// Returns a boolean indicating if we are at the targeter limit, if at the limit then no other entities can target us unless they are set as a priority
            /// </summary>
            /// <returns></returns>
            public bool IsAtTargeterLimit() {

                //Double check the current targeters is cleaned up (the count is not larger then the max)
                this.CurrentTargetersCleaner();

                if (this.currentTargeters.Count() >= this.maxNumberOfTargeters)
                    return true;
                else
                    return false;

            }

            /// <summary>
            /// Determines if the object provided is already targeting us
            /// </summary>
            /// <param name="Targeter">Object to check if it's already targeting us</param>
            /// <returns>True if object provided is targeting us, else false</returns>
            public bool ContainsTargeter(GameObject TargeterObj) {

                if (this.currentTargeters.Contains(TargeterObj))
                    return true;
                else
                    return false;

            }

            /// <summary>
            /// Will add the object to our priority list, if an object is in this list then they will take priority and can always target us removing any non priority targetters to keep within the limit
            /// </summary>
            /// <param name="Originator">The entity the target limitation is attached too</param>
            /// <param name="TargeterObj">Object to add to the priority list</param>
            /// <returns>True if successfully added to the priority list, else false</returns>
            public bool AddPriorityTargeter(ABC_IEntity Originator, GameObject TargeterObj) {

                //If object provided doesn't have the tag we are expecting in the limit then return false
                if (ABC_Utilities.ObjectHasTag(TargeterObj, ABC_Utilities.ConvertTags(Originator, this.targeterTags)) == false)
                    return false;

                //If object is already in the priority list then return true
                if (this.priorityTargeters.Contains(TargeterObj))
                    return true;


                //If priority list is at max then remove the first (oldest) targeter 
                if (this.priorityTargeters.Count() == this.maxNumberOfTargeters) {

                    //Find the first (oldest) prio targeter and remove them from prio list
                    GameObject prioTargeterToRemove = priorityTargeters.Where(t => t.gameObject != TargeterObj).First();
                    if (prioTargeterToRemove != null)
                        this.priorityTargeters.Remove(prioTargeterToRemove);
                }



                //Add object to list 
                this.priorityTargeters.Add(TargeterObj);

                //If this far then object was added successfully 
                return true;

            }


            /// <summary>
            /// Will add the object provided to our current targeter list, once the list has reached the limit no one else can target us
            /// </summary>
            /// <param name="Originator">The entity the target limitation is attached too</param>
            /// <param name="Targeter">Object to add to the current targeter list</param>
            /// <returns>True if the object was successfully added as a targeter, else false</returns>
            public bool AddTargeter(ABC_IEntity Originator, GameObject TargeterObj) {

                //If object provided doesn't have the tag we are expecting in the limit  then return false
                if (ABC_Utilities.ObjectHasTag(TargeterObj, ABC_Utilities.ConvertTags(Originator, this.targeterTags)) == false)
                    return false;


                //If object was in priority and we are at our targeter limit then remove them from the priority list as they about to be added and remove the oldest targeter to make space
                if (this.priorityTargeters.Contains(TargeterObj) && this.IsAtTargeterLimit()) {

                    //Remove object from priority list
                    this.priorityTargeters.Remove(TargeterObj);

                    //Find the first (oldest) targeter and remove them from main tracked list
                    GameObject targeterToRemove = currentTargeters.Where(t => t.gameObject != TargeterObj).First();
                    if (targeterToRemove != null)
                        this.currentTargeters.Remove(targeterToRemove);
                }


                //If object is not already apart of the current list then add them and return true 
                if (this.currentTargeters.Contains(TargeterObj) == false) {
                    this.currentTargeters.Add(TargeterObj);
                    return true;
                }

                //Entity was not added if we got this far so return false
                return false;

            }

            /// <summary>
            /// Will remove the object provided from our list which tracks who is targeting us
            /// </summary>
            /// <param name="TargeterObj">Object to remove from the tracking list</param>
            public void RemoveTargeter(GameObject TargeterObj) {

                //If object was in priority then remove them now
                if (this.priorityTargeters.Contains(TargeterObj))
                    this.priorityTargeters.Remove(TargeterObj);

                //Remove object from the list if they exist
                if (this.currentTargeters.Contains(TargeterObj))
                    this.currentTargeters.Remove(TargeterObj);

            }

            /// <summary>
            /// Will reset all current targeters allowing for new entities to target us
            /// </summary>
            public void ResetCurrentTargeters() {

                if (this.enableCurrentTargeterResets == false)
                    return;

                if (Time.time - this.lastResetTime < this.resetCurrentTargetersInterval)
                    return;

                //clear both lists
                this.currentTargeters.Clear();
                this.priorityTargeters.Clear();


            }



            #endregion



        }


        /// <summary>
        ///  Class for handling health GUI interfaces like sliders and text. Class is only used and managed by StateManager Component.
        /// </summary>
        [System.Serializable]
        public class HealthGUI {

            /// <summary>
            /// Slider Object which can display current health. 
            /// </summary>
            public ABC_SliderReference healthSlider;

            /// <summary>
            /// Slider object which will display current health but will update over time
            /// </summary>
            public ABC_SliderReference healthOverTimeSlider;

            /// <summary>
            /// The delay which will occur before the health over time slider starts updating to match the current health
            /// </summary>
            public float healthOverTimeSliderUpdateDelay = 0.7f;

            /// <summary>
            /// How long it takes for the health over time slider to match the current health
            /// </summary>
            public float healthOverTimeSliderUpdateDuration = 0.5f;

            /// <summary>
            /// Variable determining if slider is showing
            /// </summary>
            public bool sliderShowing = false;

            /// <summary>
            /// If true then the slider will only show when the entity is selected.
            /// </summary>
            public bool onlyShowSliderWhenSelected = false;

            /// <summary>
            /// GUI Text which can display current health.
            /// </summary>
            public ABC_TextReference healthText;

            /// <summary>
            /// Variable determining if text is showing
            /// </summary>
            public bool textShowing = false;

            /// <summary>
            /// If true then the text will only show when the entity is selected. 
            /// </summary>
            public bool onlyShowTextWhenSelected = false;

            /// <summary>
            /// Create new object 
            /// </summary>
            public HealthGUI() {
            }

            // ************************** Variables *************************************

            #region Variables

            /// <summary>
            /// Used to determine if we are past the delay needed to start updating the over time slider
            /// </summary>
            private float overTimeSliderUpdatingElapsedTime;

            /// <summary>
            /// Determines if the over time slider is currently updating
            /// </summary>
            private bool overTimeSliderUpdating = false;


            #endregion

            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Toggles the slider GUI on and off. If only show slider when selected is false then GUI will always be shown
            /// </summary>
            /// <param name="Enabled">If true GUI will display else it will be hidden</param>
            public void ToggleSliderGUI(bool Enabled) {

                if (this.onlyShowSliderWhenSelected == false)
                    Enabled = true;

                if (this.healthSlider.Slider != null)
                    this.healthSlider.Slider.gameObject.SetActive(Enabled);

                if (this.healthOverTimeSlider.Slider != null) {
                    this.healthOverTimeSlider.Slider.gameObject.SetActive(Enabled);
                    //If we are enabling the GUI then set health over time slider to 0 so it will automatically jump to the current health 
                    if (Enabled == true)
                        this.healthOverTimeSlider.Slider.value = 0f;
                }

                this.sliderShowing = Enabled;

            }

            /// <summary>
            /// Toggles the text GUI on and off. If only show text when selected is false then GUI will always be shown
            /// </summary>
            /// <param name="Enabled">If true GUI will display else it will be hidden</param>
            public void ToggleTextGUI(bool Enabled) {

                if (this.onlyShowTextWhenSelected == false)
                    Enabled = true;

                if (this.healthText.text != null)
                    this.healthText.Graphic.gameObject.SetActive(Enabled);

                this.textShowing = Enabled;

            }

            /// <summary>
            /// Update the GUI to represent the current and max health provided
            /// </summary>
            /// <param name="CurrentHealth">Current health to show on GUI</param>
            /// <param name="MaxHealth">Max health to show on GUI</param>
            public void UpdateGUI(float CurrentHealth, float MaxHealth) {

                if (this.healthText.text != null && this.textShowing == true)
                    this.healthText.text = ((int)CurrentHealth).ToString() + "/" + MaxHealth.ToString();

                if (this.healthSlider.Slider != null && this.sliderShowing == true) {
                    // Make sure the max value of the health slider matches our max health property (this might change during play) 
                    this.healthSlider.Slider.maxValue = MaxHealth;
                    // Update current health
                    this.healthSlider.Slider.value = CurrentHealth;
                }


                if (this.healthOverTimeSlider.Slider != null && this.sliderShowing == true) {

                    // Make sure the max value of the health over time slider matches our max health property (this might change during play) 
                    this.healthOverTimeSlider.Slider.maxValue = MaxHealth;

                    //If the health over time slider is greater then current health then we need it to slowly reach the current health value
                    if (this.healthOverTimeSlider.Slider.value > CurrentHealth && this.overTimeSliderUpdating == false) {
                        //Record the time so we can start updating after a delay
                        overTimeSliderUpdatingElapsedTime = Time.time;
                        this.overTimeSliderUpdating = true;

                    } else if (this.healthOverTimeSlider.Slider.value == CurrentHealth) {  //If we have reached current health then we are no longer updating the over time slider
                        this.overTimeSliderUpdating = false;
                    }

                    //If we are past the updating delay then update the overtime slider
                    if (Time.time - overTimeSliderUpdatingElapsedTime > this.healthOverTimeSliderUpdateDelay)
                        this.healthOverTimeSlider.Slider.value = Mathf.Max(CurrentHealth, this.healthOverTimeSlider.Slider.value -= (this.healthOverTimeSlider.Slider.value / this.healthOverTimeSliderUpdateDuration) * Time.deltaTime);

                }
            }


            #endregion


        }

        /// <summary>
        ///  Class for hit animations - hit animations activate when any ability (particle/raycast/melee) collides (hits) or adds effects to the entity with this component
        /// </summary>
        [System.Serializable]
        public class HitAnimation {

            // ************ Settings *****************************

            #region Settings For Hit Animation

            /// <summary>
            /// Used by inspector only - determines if the hit animation settings are collapsed out or not 
            /// </summary>
            public bool foldOut = false;

            /// <summary>
            /// Name of the hit animation
            /// </summary>
            [Tooltip("Name of the hit animation")]
            public string hitAnimationName;

            /// <summary>
            /// Bool which determines if the hit animation is enabled and can be activated
            /// </summary>
            [Tooltip("If hit animation is enabled ")]
            public bool hitAnimationEnabled = true;

            /// <summary>
            /// Bool which determines if the hit animation can only be activated from an effect
            /// </summary>
            [Tooltip("Bool which determines if the hit animation can only be activated from an effect")]
            public bool hitAnimationActivateFromEffectOnly = false;

            /// <summary>
            /// Minimum probability of the animation activating. If the dice roll is higher then the minimum and lower then the maximum then it will activate if other conditions are met. 
            /// </summary>
            [Range(0f, 100f)]
            [Tooltip("Minimum probability of the animation activating")]
            public float hitAnimationProbabilityMinValue = 0f;

            /// <summary>
            /// Maximum probability of the animation activating. If the dice roll is higher then the minimum and lower then the maximum then it will activate if other conditions are met. 
            /// </summary>
            [Range(0f, 100f)]
            [Tooltip("Maximum probability of the animation activating")]
            public float hitAnimationProbabilityMaxValue = 100f;

            /// <summary>
            /// Animation Clips to play in the Animation Runner
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner")]
            public List<ABC_AnimationClipReference> hitAnimationRunnerClips;

            /// <summary>
            /// Animation Clips to play in the Animation Runner when the entity is in the air
            /// </summary>
            [Tooltip("Animation Clip to play in the Animation Runner when the entity is in the air")]
            public List<ABC_AnimationClipReference> hitAnimationAirRunnerClips;

            /// <summary>
            /// The avatar mask applied for the animation clip playing in the Animation Runner
            /// </summary>
            [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
            public ABC_AvatarMaskReference hitAnimationRunnerMask;

            /// <summary>
            /// Speed of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
            public float hitAnimationRunnerClipSpeed = 1f;

            /// <summary>
            /// Delay of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
            public float hitAnimationRunnerClipDelay = 0f;

            /// <summary>
            /// Duration of the Animation Clip to play in the Animation Runner
            /// </summary>
            [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
            public float hitAnimationRunnerClipDuration = 1f;


            /// <summary>
            /// Name of the hitAnimation animation
            /// </summary>
            [Tooltip("Name of the Animation in the controller")]
            public string hitAnimationAnimatorParameter;

            /// <summary>
            /// Type of parameter for the hitAnimation animation
            /// </summary>
            [Tooltip("Parameter type to start the animation")]
            public AnimatorParameterType hitAnimationAnimatorParameterType;

            /// <summary>
            /// Value to turn on the hitAnimation animation
            /// </summary>
            [Tooltip("Value to turn on the animation")]
            public string hitAnimationAnimatorOnValue;

            /// <summary>
            /// Value to turn off the hitAnimation animation
            /// </summary>
            [Tooltip("Value to turn off the animation")]
            public string hitAnimationAnimatorOffValue;

            /// <summary>
            /// Duration of the hitAnimation animation
            /// </summary>
            [Tooltip("How long to play animation for ")]
            public float hitAnimationAnimatorDuration = 3f;



            #endregion

            // ************************** Private Methods *************************************

            #region Private Methods

            /// <summary>
            /// Starts an animation clip using the ABC animation runner stopping it after the hit animation duration
            /// </summary>
            /// <param name="Entity">Entity hit animation is running on</param>
            /// <param name="PrioritiseAirAnimations">If true then an air animation will be used, else if false air animation will be used depnding on if entity is in air or not</param>
            private IEnumerator StartAndStopAnimationRunner(ABC_IEntity Entity, bool PrioritiseAirAnimations = false) {



                ABC_AnimationsRunner AnimationRunner = Entity.animationRunner;

                // if no clips have been setup or animation runner is not given then animation can't start so end here. 
                if (this.hitAnimationRunnerClips.Count == 0 && this.hitAnimationAirRunnerClips.Count == 0 || AnimationRunner == null)
                    yield break;

                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated the same call
                //this function would not continue after duration
                float functionRequestTime = Time.time;

                //Disable IK whilst animation plays       
                ABC_Utilities.mbSurrogate.StartCoroutine(Entity.ToggleIK(functionRequestTime, false));

                //Get random clip from the list
                AnimationClip animationClip = null;

                //Check if entity is in air 
                bool inAir = Entity.isInTheAir;

                //If entity is in air and we have air animation clips
                if ((PrioritiseAirAnimations == true || inAir == true) && this.hitAnimationAirRunnerClips.Count > 0)
                    animationClip = this.hitAnimationAirRunnerClips[Random.Range(0, this.hitAnimationAirRunnerClips.Count() - 1)].AnimationClip;
                else
                    animationClip = this.hitAnimationRunnerClips[Random.Range(0, this.hitAnimationRunnerClips.Count() - 1)].AnimationClip;


                //Start animation
                bool animationStarted = AnimationRunner.StartAnimation(animationClip, this.hitAnimationRunnerClipDelay, this.hitAnimationRunnerClipSpeed, this.hitAnimationRunnerMask.AvatarMask, true);

                //If animation did not start (interruption turned off) then end here
                if (animationStarted == false)
                    yield break;

                //else wait for duration and then end animation
                for (var i = this.hitAnimationRunnerClipDuration; i > 0;) {

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

                AnimationRunner.EndAnimation(animationClip);

                //Enable the IK
                ABC_Utilities.mbSurrogate.StartCoroutine(Entity.ToggleIK(functionRequestTime, true, 0.4f));


            }



            /// <summary>
            /// Starts the hit animation
            /// </summary>
            /// <param name="Entity">Entity hit animation is running on</param>
            private IEnumerator StartAndStopAnimation(ABC_IEntity Entity) {

                //Track what time this method was called
                //Stops overlapping i.e if another part of the system activated the same call
                //this function would not continue after duration
                float functionRequestTime = Time.time;

                Animator Animator = Entity.animator;

                // set variables to be used later 
                AnimatorParameterType animatorParameterType = this.hitAnimationAnimatorParameterType;
                string animatorParameter = this.hitAnimationAnimatorParameter;
                string animatorOnValue = this.hitAnimationAnimatorOnValue;
                string animatorOffValue = this.hitAnimationAnimatorOffValue;
                float duration = this.hitAnimationAnimatorDuration;


                // if animator parameter is null or animator is not given then animation can't start so end here. 
                if (animatorParameter == "" || Animator == null) {
                    yield break;
                }

                //disable IK whilst animation plays
                ABC_Utilities.mbSurrogate.StartCoroutine(Entity.ToggleIK(functionRequestTime, false));

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


                //wait for duration then end animation
                for (var i = duration; i > 0;) {

                    // actual wait time 
                    if (i < 0.2f) {
                        // less then 0.2  so we only need to wait the .xx time
                        yield return new WaitForSeconds(i);
                    } else {
                        // wait a small amount and keep looping till casting time = 0; 
                        yield return new WaitForSeconds(0.2f);
                    }

                    //Take of a second from the animator duration tracker if play speed is higher then 0.2 (if lower then hitstop is happening) 
                    if (Animator.speed > 0.2f)
                        i = i - 0.2f;
                }


                //end animation
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

                //Wait small while then enable IK
                ABC_Utilities.mbSurrogate.StartCoroutine(Entity.ToggleIK(functionRequestTime, true, 0.4f));

            }


            #endregion


            // ************************** Public Methods *************************************

            #region Public Methods

            /// <summary>
            /// Constructor mainly used by inspector
            /// </summary>
            public HitAnimation(int ID) {

                this.hitAnimationRunnerClipSpeed = 1f;
                this.hitAnimationRunnerClipDelay = 0f;
                this.hitAnimationRunnerClipDuration = 1f;
            }

            /// <summary>
            /// Returns a bool indicating if the animation can activate. 
            /// </summary>
            /// <remarks>
            /// Will check the enabled flag and perform a dice roll - if the roll is between the min and max then it will return true
            /// </remarks>
            /// <returns>True if the hit animation can activate, else false</returns>
            public bool CanActivate() {

                //Check if animation is enabled
                if (this.hitAnimationEnabled == false)
                    return false;

                //Dice roll 
                return ABC_Utilities.DiceRoll(this.hitAnimationProbabilityMinValue, hitAnimationProbabilityMaxValue);

            }

            /// <summary>
            /// Activates the HitAnimation. Animation will start and then end after the duration set up.
            /// </summary>
            /// <param name="Entity">Entity hit animation is running on</param>
            /// <param name="PrioritiseAirAnimations">If true then an air animation will be used, else if false air animation will be used depnding on if entity is in air or not</param>
            public void ActivateAnimation(ABC_IEntity Entity, bool PrioritiseAirAnimations = false) {

                if (Entity.HasABCController() && Entity.activatingAbility == true && Entity.AbilityActivationCurrentlyInterrupted() == false)
                    return;

                //Start and stop Animator animations
                ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimation(Entity));

                //Start and stop animation using the runner
                ABC_Utilities.mbSurrogate.StartCoroutine(this.StartAndStopAnimationRunner(Entity, PrioritiseAirAnimations));
            }

            #endregion





        }

        /// <summary>
        /// Class defines information regarding an Active Effect. Including what effect is active and what time it started. 
        /// </summary>
        /// <remarks>
        /// Used by StateManager to store what effects are currently active.
        /// </remarks>
        [System.Serializable]
        public class ActiveEffect {

            /// <summary>
            /// Ability name which applied the effect
            /// </summary>
            public string abilityName = "";

            /// <summary>
            /// Property holding the effect Object with all the settings 
            /// </summary>
            public Effect effect = null;

            /// <summary>
            /// Where the effect hit 
            /// </summary>
            public Vector3 hitPoint = new Vector3(0, 0, 0);

            /// <summary>
            /// What time the effect was activated
            /// </summary>
            public float activationTime = 0f;

            /// <summary>
            /// The coroutine which was called to activate the effect. This can be stopped early to stop the effect.
            /// </summary>
            public IEnumerator ActivateCoroutine;

            public ActiveEffect(Effect effect, Vector3 hitPosition = default(Vector3), string AbilityName = "") {

                this.effect = effect;
                this.hitPoint = hitPosition;
                this.activationTime = Time.time;
                this.abilityName = AbilityName;
            }

        }

        #endregion

        // ************ Delegate *****************************

        #region Delegate

        public delegate void OnEffectActivation(Effect Effect, ABC_IEntity Target, ABC_IEntity Originator);
        public event OnEffectActivation onEffectActivation;

        public delegate void OnEffectDeActivation(Effect Effect, ABC_IEntity Target, ABC_IEntity Originator);
        public event OnEffectActivation onEffectDeActivation;


        public delegate void OnDisableMovement();
        public event OnDisableMovement onDisableMovement;

        public delegate void OnEnableMovement();
        public event OnEnableMovement onEnableMovement;


        public delegate void OnDisableGravity();
        public event OnDisableMovement onDisableGravity;

        public delegate void OnEnableGravity();
        public event OnEnableMovement onEnableGravity;

        #endregion

        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// If true then tag conversions will be applied to the entities ABC Controller and/or StateManager during play
        /// </summary>
        public bool enableTagConversions = false;

        /// <summary>
        /// A list of tag conversions (i.e replace x tag with y in whole component)
        /// </summary>
        public List<ABC_Utilities.TagConverter> tagConversions = new List<ABC_Utilities.TagConverter>();

        /// <summary>
        /// What toolbar tab is selected in the StateManager inspector
        /// </summary>
        public int toolbarStateManagerSelection;

        /// <summary>
        /// What tab is selected in the StateManager general settings inspector
        /// </summary>
        public int toolbarStateManagerGeneralSettingsSelection;

        /// <summary>
        /// What tab is selected in the StateManager effect watcher settings inspector
        /// </summary>
        public int toolbarStateManagerEffectWatcherSettingsSelection;

        /// <summary>
        /// show blue guidance boxes in inspector to aid the user 
        /// </summary>
        public bool showHelpInformation = true;

        /// <summary>
        /// If true then inspector editor will have it's values update whilst unity is running in play mode (uses repaint). Will decrease performance of game running. 
        /// </summary>
        public bool updateEditorAtRunTime = false;

        /// <summary>
        /// Extra tagging which is used throughout ABC. Everytime a normal unity tag is checked it also checks this property. 
        /// </summary>
        public List<string> ABCTag = new List<string>();

        /// <summary>
        /// A list of targeter limitations, this defines how many entites can target us at one time
        /// </summary>
        public List<TargeterLimitation> TargeterLimitations = new List<TargeterLimitation>();

        /// <summary>
        /// List which contains all the hit animations setup for the entity. A hit animation will activate when any ability (particle/raycast/melee) collides (hits) or adds effects to the entity
        /// The animation which plays depends on the list order. The list will be cycled through until a dice roll matches, animations at the top will be checked first
        /// </summary>
        public List<HitAnimation> HitAnimations = new List<HitAnimation>();

        /// <summary>
        /// If true then a hit animation will activate whenever any ability (particle/raycast/melee) collides (hits) the entity
        /// </summary>
        public bool activateHitAnimationsFromAbilityHit = true;

        /// <summary>
        /// If true then a hit animation will activate whenever a  hitanimation effect from an ability is applied to the entity
        /// </summary>
        public bool activateHitAnimationsFromAbilityEffect = true;

        /// <summary>
        /// If true then the hit animations will be randomized before the list is checked. When checked The list will be cycled through until a dice roll matches - resulting in the animation activating.
        /// Animations at the top will be checked first.
        /// </summary>
        public bool randomizeHitAnimations = false;

        /// <summary>
        /// If true then the entity can't be pushed by effects/impacts
        /// </summary>
        [Tooltip("If true then the entity can't be pushed by effects/impacts")]
        public bool blockPushEffects = false;

        /// <summary>
        /// If enabled then hits from ability can stop movement
        /// </summary>
        public bool hitsStopMovement = true;

        /// <summary>
        /// How long hits will stop movement for
        /// </summary>
        public float hitStopMovementDuration = 1f;

        /// <summary>
        /// If true then entity will stop moving due to the position being frozen
        /// </summary>
        [Tooltip("If true then  entity will stop moving due to the position being frozen")]
        public bool hitStopMovementFreezePosition = false;

        /// <summary>
        /// If true then  entity will stop moving due to movement components being disabled
        /// </summary>
        [Tooltip("If true then entity will stop moving due to movement components being disabled")]
        public bool hitStopMovementDisableComponents = true;


        /// <summary>
        /// List which contains all the HealthGUI Objects. 
        /// </summary>
        /// <remarks>
        /// StateManager component will loop through all these Objects updating the health stat. 
        /// </remarks>
        public List<HealthGUI> HealthGUIList = new List<HealthGUI>();

        /// <summary>
        /// If true then an GUI image will appear when health is reduced for a duration set
        /// </summary>
        /// <remarks>Useful for blood camera splats</remarks>
        public bool showGUIImageOnHealthReduction = false;

        /// <summary>
        /// The image to show when health is reduced
        /// </summary>
        public ABC_RawImageReference imageOnHealthReduction = null;


        /// <summary>
        /// How long to show the health reduction image for 
        /// </summary>
        public float imageOnHealthReductionDuration = 1f;

        /// <summary>
        /// Max starting health of the entity
        /// </summary>
        public float maxHealth = 500f;

        /// <summary>
        ///  Current max health of the entity
        /// </summary>
        public float currentMaxHealth {
            get {

                if (this.healthIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCMaxAttributeValue(this.gcHealthID);
                else if (this.healthIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2MaxAttributeValue(this.gcHealthID);
                else if (this.healthIntergrationType == ABCIntegrationType.EmeraldAI)
                    return meEntity.GetEmeraldAIMaxHealth();
                else
                    return this.maxHealth;
            }
            set {

                if (this.healthIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcHealthID, value, GCStatType.Attribute);
                else if (this.healthIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcHealthID, value, GCStatType.Attribute);
                else if (this.healthIntergrationType == ABCIntegrationType.EmeraldAI)
                    meEntity.SetEmeraldMaxHealth((int)value);
                else
                    this.maxHealth = value;
            }
        }


        /// <summary>
        /// Current health 
        /// </summary>
        public float healthABC = 500;


        /// <summary>
        ///  Current health of the entity
        /// </summary>
        public float currentHealth {
            get {

                if (this.healthIntergrationType == ABCIntegrationType.GameCreator)
                    return meEntity.GetGCStatValue(this.gcHealthID, GCStatType.Attribute);
                if (this.healthIntergrationType == ABCIntegrationType.GameCreator2)
                    return meEntity.GetGC2StatValue(this.gcHealthID, GCStatType.Attribute);
                else if (this.healthIntergrationType == ABCIntegrationType.EmeraldAI)
                    return meEntity.GetEmeraldAICurrentHealth();
                else
                    return this.healthABC;
            }
            set {
                if (this.healthIntergrationType == ABCIntegrationType.GameCreator)
                    meEntity.SetGCStatValue(this.gcHealthID, value, GCStatType.Attribute);
                else if (this.healthIntergrationType == ABCIntegrationType.GameCreator2)
                    meEntity.SetGC2StatValue(this.gcHealthID, value, GCStatType.Attribute);
                else if (this.healthIntergrationType == ABCIntegrationType.EmeraldAI)
                    meEntity.SetEmeraldAICurrentHealth((int)value);
                else
                    this.healthABC = value;
            }
        }





        /// <summary>
        /// The integration type for health functionality - if ABC is picked then normal functionality is used
        /// else health from another integration system i.e game creator is used
        /// </summary>
        public ABCIntegrationType healthIntergrationType = ABCIntegrationType.ABC;

        /// <summary>
        /// Game Creator Integration: The ID of the GC stat/attribute which represents health 
        /// To work correctly the health value needs to be added as a GC attribute not a stat
        /// </summary>
        public string gcHealthID = "health";

        /// <summary>
        /// How much health will regenerate each second. 
        /// </summary>
        public float healthRegenPerSecond = 0;

        /// <summary>
        /// Determines if the entity will restore to full health when enabled
        /// </summary>
        [Tooltip("Restore full health on enable.")]
        public bool fullHealthOnEnable = true;

        /// <summary>
        /// If true then a model will be swapped on zero health
        /// </summary>
        public bool swapModelOnZeroHealth = false;

        /// <summary>
        /// The gameobject to disable on zero health
        /// </summary>
        public ABC_GameObjectReference swapModelToDisable = null;

        /// <summary>
        /// The gameobject to enable on zero health 
        /// </summary>
        public ABC_GameObjectReference swapModelToEnable = null;

        /// <summary>
        /// If true then the entity gameobject will be disabled once health reaches zero
        /// </summary>
        public bool disableEntityOnZeroHealth = true;

        /// <summary>
        /// The delay before object is disabled due to being on 0 health
        /// </summary>
        public float disableDelay = 1.5f;

        /// <summary>
        /// If true then the entity will stop moving when they reach 0 health
        /// </summary>
        public bool stopMovementOnZeroHealth = true;

        /// <summary>
        /// If true then entity will stop moving due to the position being frozen
        /// </summary>
        [Tooltip("If true then  entity will stop moving due to the position being frozen")]
        public bool stopMovementOnZeroHealthFreezePosition = true;

        /// <summary>
        /// If true then  entity will stop moving due to movement components being disabled
        /// </summary>
        [Tooltip("If true then entity will stop moving due to movement components being disabled")]
        public bool stopMovementOnZeroHealthDisableComponents = false;

        /// <summary>
        /// Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Animation Clip to play in the Animation Runner")]
        public ABC_AnimationClipReference zeroHealthAnimationRunnerClip;

        /// <summary>
        /// The avatar mask applied for the animation clip playing in the Animation Runner
        /// </summary>
        [Tooltip("The avatar mask applied for the animation clip playing in the Animation Runner")]
        public ABC_AvatarMaskReference zeroHealthAnimationRunnerMask = null;

        /// <summary>
        /// Speed of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Speed of the Animation Clip to play in the Animation Runner")]
        public float zeroHealthAnimationRunnerClipSpeed = 1f;

        /// <summary>
        /// Delay of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Delay of the Animation Clip to play in the Animation Runner")]
        public float zeroHealthAnimationRunnerClipDelay = 0f;

        /// <summary>
        /// Duration of the Animation Clip to play in the Animation Runner
        /// </summary>
        [Tooltip("Duration of the Animation Clip to play in the Animation Runner")]
        public float zeroHealthAnimationRunnerClipDuration = 1f;

        /// <summary>
        /// Name of the disableAnimation animation
        /// </summary>
        [Tooltip("Name of the Animation in the controller")]
        public string zeroHealthAnimationAnimatorParameter;

        /// <summary>
        /// Type of parameter for the disableAnimation animation
        /// </summary>
        [Tooltip("Parameter type to start the animation")]
        public AnimatorParameterType zeroHealthAnimationAnimatorParameterType;

        /// <summary>
        /// Value to turn on the disableAnimation animation
        /// </summary>
        [Tooltip("Value to turn on the animation")]
        public string zeroHealthAnimationAnimatorOnValue;

        /// <summary>
        /// Value to turn off the disableAnimation animation
        /// </summary>
        [Tooltip("Value to turn off the animation")]
        public string zeroHealthAnimationAnimatorOffValue;

        /// <summary>
        /// Duration of the disableAnimation animation
        /// </summary>
        [Tooltip("How long to play animation for ")]
        public float zeroHealthAnimationAnimatorDuration = 3f;

        /// <summary>
        /// Dictionary of custom stats created (Strength, Intelligence) etc
        /// </summary>
        public List<EntityStat> EntityStatList = new List<EntityStat>();

        /// <summary>
        /// If true then effects can only ever be activated once per caster
        /// </summary>
        public bool blockDuplicateEffectActivation = false;

        /// <summary>
        /// If true then the entity can receive damage frome ffect.
        /// </summary>
        public bool canDamage = true;

        /// <summary>
        /// If ticked then ABC will automatically pool graphics and objects on start, else if false they will be made on the go
        /// </summary>
        [Tooltip("If ticked then ABC will automatically pool graphics and gameobjects on game start, else if false they will be made on the go")]
        public bool enablePooling = true;

        /// <summary>
        /// Graphic Text object which displays effect text in the actual game world (Not on the GUI)  - Is an object and not text as it is moved around in world positions. 
        /// </summary>
        public ABC_GameObjectReference effectGraphicTextGUI;

        /// <summary>
        /// If true then the position of the effect graphic will be randomised and the size/scale will depend on the potency value done
        /// </summary>
        [Tooltip(" If true then the position of the effect graphic will be randomised and the size/scale will depend on the potency value done")]
        public bool effectGraphicTextRandomise = true;

        /// <summary>
        /// Offset of the effect Graphic Text
        /// </summary>
        [Tooltip("Offset on the starting position")]
        public Vector3 effectGraphicTextOffset;

        /// <summary>
        /// Forward offset of the effect Graphic Text
        /// </summary>
        [Tooltip("Forward offset from starting position")]
        public float effectGraphicTextForwardOffset = 0f;

        /// <summary>
        /// Right offset of the effect Graphic Text
        /// </summary>
        [Tooltip("Right offset from starting position")]
        public float effectGraphicTextRightOffset = 0f;

        /// <summary>
        /// GUI Text which can display effect logs. 
        /// </summary>
        public ABC_TextReference effectLogGUIText;

        /// <summary>
        /// How many lines are saved in the Effect Text log before it is cleared
        /// </summary>
        public int effectLogMaxLines = 5;

        /// <summary>
        /// If true then the effect log can be configured to only appear for a duration after an entry is added
        /// </summary>
        public bool effectLogUseDuration = false;

        /// <summary>
        /// The duration in which the effect log will be shown for
        /// </summary>
        public float effectLogDuration = 4f;

        /// <summary>
        /// If the entity is converted to a SurroundingObject then this property tracks what Projectile the entity is linked too
        /// </summary>
        public GameObject surroundingObjectLinkedProjectile = null;

        /// <summary>
        /// Records if the entity converted to a surrounding object was a kinematic rigibody, so it can be reverted back to the same status once it's done
        /// </summary>
        public bool surroundingObjectLinkIsKinematic = false;

        /// <summary>
        /// Records if the entity converted to a surrounding object collider was a trigger, so it can be reverted back to the same status once it's done
        /// </summary>
        public bool surroundingObjectLinkIsTrigger = false;


        /// <summary>
        /// Records the rigidbody interpolate state of the converted surrunding object, so it can be reverted back to the same status once it's done
        /// </summary>
        public RigidbodyInterpolation surroundingObjectLinkInterpolateState = RigidbodyInterpolation.None;


        /// <summary>
        /// If true then the entity will never turn into a surrounding Object 
        /// </summary>
        public bool blockSurroundingObjectStatus = false;

        /// <summary>
        /// If true then the entity will ignore all ability collisions
        /// </summary>
        public bool ignoreAbilityCollision = false;

        /// <summary>
        /// If true then the entity is invulnerable to all effects. 
        /// </summary>
        public bool effectProtection = false;

        #endregion


        // ********************* Variables ******************

        #region Variables

        /// <summary>
        /// Used for inspector only to toggle the fold out for integrations
        /// </summary>
        public bool foldOutIntegration = false;

        Transform meTransform;

        // entity interface 
        ABC_IEntity _meEntity;

        // entity interface 
        ABC_IEntity meEntity {
            get {

                //If entity has not been retrieved yet then find it
                if (this._meEntity == null)
                    _meEntity = ABC_Utilities.GetStaticABCEntity(gameObject);

                //Return the entity 
                return _meEntity;

            }
            set {
                this._meEntity = value;
            }
        }


        //Animator of the entity using this component
        Animator Ani;

        /// <summary>
        /// List of active effects. Keeps track of what is currently effecting the entity and the originator who applied the effects through abilities. 
        /// </summary>
        public Dictionary<ABC_IEntity, List<ActiveEffect>> ActiveEffects = new Dictionary<ABC_IEntity, List<ActiveEffect>>();


        /// <summary>
        /// Pool manager which holds a list of EffectText which appear in game. 
        /// </summary>
        private List<GameObject> EffectTextOnScreenPool = new List<GameObject>();

        /// <summary>
        /// Variable to track if the entity hit 0 health
        /// </summary>
        private bool entityZeroHealthReached = false;

        /// <summary>
        /// If true then melee effects will not be applied to the entity
        /// </summary>
        private bool preventMeleeEffects = false;

        /// <summary>
        /// If true then projectile and raycast effects will not be applied to the entity 
        /// </summary>
        private bool preventProjectileAndRayCastEffects = false;

        /// <summary>
        /// amount of melee damage the entity will currently mitigate
        /// </summary>
        private float mitigateMeleeDamagePercentage = 0f;

        /// <summary>
        /// amount of projectile and raycast damage the entity will currently mitigate
        /// </summary>
        private float mitigateProjectileAndRaycastDamagePercentage = 0f;


        /// <summary>
        /// Variable used for editor if true then the active effect history will be recorded
        /// </summary>
        public bool editorRecordActiveEffectHistory = false;

        /// <summary>
        /// List of active effect history logs
        /// </summary>
        public List<string> editorActiveEffectHistory = new List<string>();






        #endregion


        // ********************* Public Methods ********************

        #region Public Methods

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

            //Reinitalise the component but skip health restore. 
            InitialiseComponent(true);
        }

        /// <summary>
        /// Will add an ABC tag to the entity
        /// </summary>
        /// <param name="TagName">Name of the tag to add</param>
        public void AddABCTag(string TagName) {
            this.ABCTag.Add(TagName);
        }

        /// <summary>
        /// Will remove an ABC tag to the entity
        /// </summary>
        /// <param name="TagName">Name of the tag to remove</param>
        public void RemoveABCTag(string TagName) {
            this.ABCTag.Remove(TagName);
        }


        /// <summary>
        /// Will return a list of all ABC tags setup on this entity 
        /// </summary>
        /// <returns></returns>
        public List<string> GetABCTags() {

            return ABC_Utilities.ConvertTags(this.meEntity, this.ABCTag);

        }

        /// <summary>
        /// Determines if the object provided can target this entity, will check through all limit lists
        /// </summary>
        /// <param name="Targeter">Object which is attempting to target us</param>
        /// <returns>True if the targeter provided can target this entity, else false</returns>
        public bool CanBeTargetedByObject(GameObject Targeter) {

            //If we have no active target limitations then return true, as no limit restrictions have been setup 
            if (this.TargeterLimitations.Where(t => t.enableTargeterLimit == true).Count() == 0)
                return true;

            //default true
            bool retval = true;

            //Find all target limitations which match the tag of the targeter provided, If none found then the retval will stay true as no limitation for that tag has been setup
            foreach (TargeterLimitation TL in this.TargeterLimitations.Where(t => t.enableTargeterLimit == true && ABC_Utilities.ObjectHasTag(Targeter, ABC_Utilities.ConvertTags(this.meEntity, t.targeterTags)))) {

                //If the targeter can't target us due to limitations then set the retval as false
                if (TL.CanBeTargetedBy(Targeter) == false) {
                    retval = false;
                } else {
                    retval = true; // If a space is found in any list then turn the retval to true and end here as we have found an open place
                    break;
                }
            }



            return retval;

        }

        /// <summary>
        /// Will add the gameobject provided to a priority list which will allow the Targeter to take a spot in the tracking list if requested
        /// </summary>
        /// <param name="Targeter">Object to track</param>
        public void AddObjectAsPriorityTargeter(GameObject Targeter) {

            //cycle through target limitations until targeter has been successfully added then return once done (so targeter is always only added to one list)
            foreach (TargeterLimitation TL in this.TargeterLimitations.Where(t => t.enableTargeterLimit == true)) {
                if (TL.AddPriorityTargeter(this.meEntity, Targeter))
                    return;
            }

        }


        /// <summary>
        /// Will add the gameobject provided to a list which tracks who is targeting the entity
        /// </summary>
        /// <param name="Targeter">Object to track</param>
        public void AddObjectToTargeterTracker(GameObject Targeter) {

            //cycle through target limitations until targeter has been successfully added then return once done (so targeter is always only added to one list)
            foreach (TargeterLimitation TL in this.TargeterLimitations.Where(t => t.enableTargeterLimit == true)) {
                if (TL.AddTargeter(this.meEntity, Targeter))
                    return;
            }


        }

        /// <summary>
        /// Will remove the gameobject provided from all lists which tracks who is targeting the entity
        /// </summary>
        /// <param name="Targeter">Object to track</param>
        public void RemoveObjectFromTargeterTracker(GameObject Targeter) {

            foreach (TargeterLimitation TL in this.TargeterLimitations.Where(t => t.enableTargeterLimit == true))
                TL.RemoveTargeter(Targeter);

        }


        /// <summary>
        /// Will reset all lists currently tracking who is targeting us, allowing for new entities to target us
        /// </summary>
        public void ResetAllCurrentTargeters() {

            //Find all active target limitations
            foreach (TargeterLimitation TL in this.TargeterLimitations.Where(t => t.enableTargeterLimit == true))
                TL.ResetCurrentTargeters();



        }


        /// <summary>
        /// Will enable or disable effect protection on the entity, effect protection will stop any ability effects from activating on the entity
        /// </summary>
        /// <param name="Enabled">True if to enable effect protection, else false to disable</param>
        public void ToggleEffectProtection(bool Enabled = true) {

            this.effectProtection = Enabled;

        }


        /// <summary>
        /// Will enable or disable ability collision on the entity, if true then no ability will collide or apply effects to the entity
        /// </summary>
        /// <param name="Enabled">True if to enable ignoring ability collision, else false to disable</param>
        public void ToggleIgnoreAbilityCollision(bool Enabled = true) {

            this.ignoreAbilityCollision = Enabled;

        }

        /// <summary>
        /// Function to show or hide the entity Health GUI 
        /// </summary>
        /// <param name="Enabled">If true will show Health, else will hide it</param>
        public void ShowHealthGUI(bool Enabled) {

            // no health guis to update so return here
            if (this.HealthGUIList.Count == 0)
                return;


            foreach (HealthGUI gui in this.HealthGUIList) {
                gui.ToggleSliderGUI(Enabled);
                gui.ToggleTextGUI(Enabled);
            }

        }

        /// <summary>
        /// Function to show or hide the entity Stats GUI 
        /// </summary>
        /// <param name="Enabled">If true will show Stats GUI, else will hide it</param>
        public void ShowStatsGUI(bool Enabled) {

            // no health guis to update so return here
            if (this.EntityStatList.Count == 0)
                return;


            foreach (EntityStat stat in this.EntityStatList) {
                if (stat.textStatName.text != null) {
                    if (stat.onlyShowTextWhenSelected == true) {
                        stat.textStatName.Graphic.gameObject.SetActive(Enabled);
                        stat.textShowing = Enabled;
                    } else {
                        // slider should always be shown so set true regardless of enable or disable
                        stat.textStatName.Graphic.gameObject.SetActive(true);
                        stat.textShowing = true;
                    }
                }

                if (stat.textStatValue.text != null) {
                    if (stat.onlyShowTextWhenSelected == true) {
                        stat.textStatValue.Graphic.enabled = Enabled;
                        stat.textShowing = Enabled;
                    } else {
                        // text should always be shown so set true regardless of enable or disable
                        stat.textStatValue.Graphic.enabled = true;
                        stat.textShowing = true;
                    }
                }
            }

        }

        /// <summary>
        /// Will retrieve the EffectText Graphic object from the entitys pool. This is then normally used to display effect information in the game world. 
        /// </summary>
        /// <returns>Effect Text Graphic Object</returns>
        public GameObject GetEffectTextObject() {

            GameObject effectText = null;


            effectText = this.EffectTextOnScreenPool.Where(obj => obj.activeInHierarchy == false).OrderBy(obj => UnityEngine.Random.value).FirstOrDefault();

            if (effectText == null)
                effectText = CreateEffectTextObjects(true);


            return effectText;

        }


        /// <summary>
        /// Adds the string provided to the effect log if setup correctly. If log is currently at or greater then the max lines provided then the oldest message will be removed.
        /// </summary>
        /// <param name="Text">Message to add to log</param>
        public IEnumerator AddToEffectLog(string Text) {

            if (effectLogGUIText.text == null)
                yield break;

            // Split log so we can see how many lines it currently has
            string[] logSplit = this.effectLogGUIText.text.Split("\n"[0]);

            // if we are over or at the line count limit then remove the oldest message
            if (logSplit.Length >= this.effectLogMaxLines + 1) {

                // clear current log
                this.effectLogGUIText.text = "";

                // re-add to log everything except the oldest line (element 0 in array)
                for (int i = 1; i < logSplit.Length - 1; i++) {
                    this.effectLogGUIText.text += logSplit[i] + "\n";
                }

            }

            //add new text to the end of the log
            this.effectLogGUIText.text += Text + "\n";


            //enable log
            effectLogGUIText.Graphic.enabled = true;

            //Determine if the log should only appear for a small while
            if (this.effectLogUseDuration) {

                //Store what is currently present in effect log
                string savedLog = effectLogGUIText.text;

                //Wait for duration
                yield return new WaitForSeconds(this.effectLogDuration);

                //If the text hasn't changed (in other words another entry has not been made meaning effect log needs to reset duration) then turn off log. 
                if (savedLog == effectLogGUIText.text)
                    effectLogGUIText.Graphic.enabled = false;


            }

        }

        /// <summary>
        /// Will display the text as a graphic in game for the duration provided
        /// </summary>
        /// <param name="Text">Text to display</param>
        /// <param name="Duration">Duration of the graphic</param>
        /// <param name="Color">Color of the graphic</param>
        /// <param name="CastingOriginator">Entity that applied the effect</param>
        public IEnumerator DisplayTextGraphic(string Text, float Duration, Color Color, ABC_IEntity CastingOriginator) {

            GameObject effectText = this.GetEffectTextObject();

            // if effect text was not retrieved then we can finish here
            if (effectText == null)
                yield break;

            // take effect text out of ABCPool and on to the person hit
            effectText.transform.SetParent(null, false);

            // set transform of the gui to our object and look at camera 
            effectText.transform.position = meTransform.position + effectGraphicTextOffset + meTransform.forward * effectGraphicTextForwardOffset + meTransform.right * effectGraphicTextRightOffset;
            effectText.transform.LookAt(CastingOriginator.Camera.transform.position);

            //add some randomness if set to

            if (this.effectGraphicTextRandomise) {
                //get rect transform
                RectTransform effectTextTransform = effectText.GetComponent<RectTransform>();

                //Random position
                effectText.transform.position += meTransform.up * Random.Range(-1.5f, 1.2f) + meTransform.right * Random.Range(-1.5f, 1.5f);

                //reset scale for normal text
                effectTextTransform.localScale = new Vector3(1, 1, 1);


                //If a number then scale it depending on value
                int numberCheck;
                if (effectTextTransform != null && int.TryParse(Text, out numberCheck)) {

                    effectTextTransform.localScale = new Vector3(Mathf.Clamp(numberCheck / 33, 0.6f, 3f), Mathf.Clamp(numberCheck / 33, 0.6f, 3f), Mathf.Clamp(numberCheck / 33, 0.6f, 3f));
                }
            }

            // enable objects and script 
            effectText.SetActive(true);

            // set the text and colour of the text component (should be a child) 
            Text gameText = effectText.GetComponentInChildren<Text>();
            if (gameText != null) {
                gameText.text = Text;
                gameText.color = new Color(Color.r, Color.g, Color.b);
            }

            TMP_Text gameTMP = effectText.GetComponentInChildren<TMP_Text>(); 
            if (gameTMP != null) {
                gameTMP.text = Text; 
                gameTMP.color = new Color(Color.r, Color.g, Color.b);
            }


            yield return new WaitForSeconds(Duration + 0.1f);

            //disable and place back in pool
            ABC_Utilities.PoolObject(effectText);


        }



        /// <summary>
        /// Adds all effects provided to the entity. 
        /// </summary>
        /// <param name="AbilityName">The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param>
        /// <param name="effects">Effects to add</param>
        /// <param name="AfflictingOriginator">Entity that applied the effect</param>
        /// <param name="AbilityType">The type of ability which applied the effect (projectile, raycast, melee)</param>
        /// <param name="IsEffectLink">If true then the effect has been applied via ability effect link</param>
        /// <param name="Projectile">(Optional) Object that we collided with which started the process of adding an effect - Used in some range settings etc</param>
        /// <param name="ObjectHit">The object which was hit which effects will be applied on</param>
        /// <param name="AbilityHitPoint">(Optional) The vector position where the ability collided, if setup correctly this will play the effect graphics at that position</param>
        /// <param name="ActivateAnimationFromHit">If true then hit animation will activate from effect being applied</param>
        /// <param name="ActivateAnimationFromHitDelay">(Optional) delay before animation activates</param>
        /// <param name="ActivateAnimationFromHitClip">(Optional) the animation clip to play on hit</param>
        /// <param name="SpecificAnimationToActivate">The specific hit animation to activate, if left blank then a random hit animation setup will activate (using probability checks and if configured to activate on hit etc)</param>
        /// <param name="OverrideWeaponBlocking">(Optional)If True then the ability will ignore any weapon blocking, stopping the entity hit from blocking</param>
        /// <param name="ReduceWeaponBlockDurability">(Optional)If True then the block durability will be decreased on the entity that blocks this ability</param>
        /// <param name="OverrideWeaponParrying">(Optional)If True then the weapon parry will be ignored</param>  
        /// <param name="PushAmount">(Optional) Amount of push to apply</param>
        /// <param name="LiftAmount">(Optional) Amount of lift to apply</param>
        /// <param name="PushDelay">(Optional) Delay before push/lift is applied</param>
        /// <param name="Ability">(Optional) Ability with all the additional settings</param>
        public IEnumerator AddEffects(string AbilityName, List<Effect> effects, ABC_IEntity AfflictingOriginator, AbilityType AbilityType, bool IsEffectLink, GameObject Projectile = null, GameObject ObjectHit = null, Vector3 AbilityHitPoint = default(Vector3), bool ActivateAnimationFromHit = true, float ActivateAnimationFromHitDelay = 0f, AnimationClip ActivateAnimationFromHitClip = null, string SpecificAnimationToActivate = "", bool OverrideWeaponBlocking = false, bool ReduceWeaponBlockDurability = true, bool OverrideWeaponParrying = false, float PushAmount = 0f, float LiftAmount = 0f, float PushDelay = 0f, ABC_Ability Ability = null) {

            //Let AI know that we just hit them so they should be aware of us
            meEntity.AINavigationAlert(AfflictingOriginator);

            // if effect protection is on then end here
            if (this.effectProtection == true)
                yield break;


            //Call weapon parry handler which will manage if the ability was parried, returning the result  
            bool parrySuccessful = meEntity.ActivateCurrentEquippedWeaponParryHandler(AfflictingOriginator, AbilityType, AbilityHitPoint, OverrideWeaponParrying);

            //If parry was successful then end here 
            if (parrySuccessful == true)
                yield break;


            //Call weapon block handler which will manage if the ability was blocked, returning the result 
            //If effect link then we won't reduce the durability again (as it's already been reduced)
            bool blockSuccessful = meEntity.ActivateCurrentEquippedWeaponBlockHandler(AfflictingOriginator, AbilityType, AbilityHitPoint, OverrideWeaponBlocking, IsEffectLink ? false : ReduceWeaponBlockDurability);

            //If ability type is melee and entity is preventing melee effects then end here
            if (AbilityType == AbilityType.Melee && this.IsPreventingMeleeEffects() == true)
                yield break;
            else if (this.IsPreventingProjectileAndRayCastEffects() == true)    //If ability type is projectile/raycast and entity is preventing melee effects then end here
                yield break;


            //Apply impact gravity 
            if (Ability != null && Ability.defyEntityGravityOnImpact == true) {
                ABC_Utilities.mbSurrogate.StartCoroutine(meEntity.DefyOriginatorGravity(Ability.defyEntityGravityOnImpactDuration, Ability.defyEntityGravityOnImpactDelay, true));
            }

            //Apply impact push 
            if (PushAmount > 0 || LiftAmount > 0) {

                //if melee attack then hit point can be the afflicting originator position
                Vector3 pushPoint = AbilityHitPoint;

                if (AfflictingOriginator != null && AbilityType == AbilityType.Melee)
                    pushPoint = AfflictingOriginator.transform.position;

                meEntity.Push(PushAmount, pushPoint, 0.3f, LiftAmount, PushDelay);
            }


            //Default to not critical hit until dice is checked
            bool WasCriticalHit = false; 

            //Roll a dice and see if it's between 0 and the crit chance (base + stat modification) + global crit chance
            if (Ability.enableCriticalHit == true && ABC_Utilities.DiceRoll(0, Ability.potencyCriticalHitBaseChance 
                + (Ability.potencyCriticalHitEnableChanceStatModification == false ? 0 : ABC_Utilities.PercentageOfValue(Ability.potencyCriticalHitChanceStatModification.percentageValue, AfflictingOriginator.GetStatValue(Ability.potencyCriticalHitChanceStatModification.statName, Ability.potencyCriticalHitChanceStatModification.statIntegrationType)))
                + AfflictingOriginator.GetAbilityGlobalCritChanceAdjustment())) {
                WasCriticalHit = true; 
            }

            //Activate effects
            if (ObjectHit.activeInHierarchy == true)
            StartCoroutine(ActivateEffects(AbilityName, effects, AfflictingOriginator, AbilityType, WasCriticalHit, Projectile, ObjectHit, AbilityHitPoint));

#if ABC_GC_Integration
        if (Ability != null && Ability.gcEffectActionList != null)
            Ability.gcEffectActionList?.Execute(meEntity.gameObject, null);
#endif

#if ABC_GC_2_Integration

        if (Ability != null && Ability.gc2EffectAction != null)
            meEntity.RunGC2Action(Ability.gc2EffectAction, new GameCreator.Runtime.Common.Args(AfflictingOriginator.gameObject, meEntity.gameObject));
            
#endif

            //Wait fraction incase ability is interrupted (so hit animation will play)
            yield return new WaitForSeconds(0.01f);

            //if set to run hit animations on hit 
            if (this.activateHitAnimationsFromAbilityHit == true && ActivateAnimationFromHit == true && blockSuccessful == false) {

                if (ActivateAnimationFromHitDelay > 0f)
                    yield return new WaitForSeconds(ActivateAnimationFromHitDelay);

                //If an animation clip has been added then run that 
                if (ActivateAnimationFromHitClip != null)
                    StartCoroutine(ABC_Utilities.RunAnimationClip(meEntity, ActivateAnimationFromHitClip, true));
                else if (string.IsNullOrEmpty(SpecificAnimationToActivate)) //else If no specific animation has been passed through to activate then activate one randomly (depending on probability checks and if configured to activate on hits)
                    this.ActivateHitAnimation(Ability.activateAnimationFromHitUseAirAnimation);
                else
                    this.ActivateHitAnimation(SpecificAnimationToActivate, true, Ability.activateAnimationFromHitUseAirAnimation); // else activate the animation provided
            }



        }

        /// <summary>
        /// Will raise the entities effect activation delegate event. 
        /// </summary>
        /// <param name="effect">Effect activated</param>
        public void RaiseEffectActivationEvent(Effect Effect, ABC_IEntity Target, ABC_IEntity Originator) {

            if (Effect != null && onEffectActivation != null)
                this.onEffectActivation(Effect, Target, Originator);

        }

        /// <summary>
        /// Will raise the entities effect deactivation delegate event. 
        /// </summary>
        /// <param name="effect">Effect activated</param>
        public void RaiseEffectDeactivationEvent(Effect Effect, ABC_IEntity Target, ABC_IEntity Originator) {

            if (Effect != null && onEffectDeActivation != null)
                this.onEffectDeActivation(Effect, Target, Originator);
        }

        /// <summary>
        /// Activates a random hit animation ability effect
        /// </summary>
        public void ActivateRandomHitAnimationEffect() {

            //if set to run hit animations from the ability effect
            if (this.activateHitAnimationsFromAbilityEffect == true)
                this.ActivateHitAnimation();

        }


        /// <summary>
        /// Activates a  hit animation ability effect
        /// </summary>
        public void ActivateHitAnimationEffect(string HitAnimationName) {

            //if set to run hit animations from the ability effect
            if (this.activateHitAnimationsFromAbilityEffect == true)
                this.ActivateHitAnimation(HitAnimationName);

        }


        /// <summary>
        /// Determines if an effect is currently active on the entity. 
        /// </summary>
        /// <param name="Effect">Effect object to determine if active or not</param>
        /// <param name="CastingOriginator">Entity that applied the effect</param>
        /// <returns>True if effect is active, else false.</returns>
        public bool IsEffectActive(string AbilityName, Effect Effect, ABC_IEntity CastingOriginator) {

            if (ActiveEffects.ContainsKey(CastingOriginator) && ActiveEffects[CastingOriginator].Any(c => c.effect.effectID == Effect.effectID && c.effect.effectAbilityID == Effect.effectAbilityID && c.abilityName == AbilityName)) {
                return true;
            } else {
                return false;
            }

        }


        /// <summary>
        /// Determines if an effect is currently active on the entity. 
        /// </summary>
        /// <param name="EffectList">List of effect names to determine if they are active or not</param>
        /// <returns>True if effect is active, else false.</returns>
        public bool IsEffectActive(List<string> EffectList) {


            foreach (string condition in EffectList) {

                // looks through all effects by every origin and if one is found we return true 
                if (this.ActiveEffects.SelectMany(c => c.Value).Select(v => v).Select(e => e.abilityName).Contains(condition)) {
                    return true;

                }
            }

            // effect is not active so we return false
            return false;

        }


        /// <summary>
        /// Will remove the effects inflicted by the ability ID provided
        /// </summary>
        /// <param name="AbilityID">ID of the ability whos effects will be removed</param>
        /// <param name="BypassDispelRestriction">(Optional) If true then the dispellable restriction will be ignored</param>
        public void RemoveAbilitiesEffects(int AbilityID, bool BypassDispelRestriction = false) {

            foreach (ActiveEffect activeEffect in ActiveEffects.SelectMany(k => k.Value).Where(t => t.effect.effectAbilityID == AbilityID).ToList()) {
                activeEffect.effect.Dispel(BypassDispelRestriction);
            }

        }

        /// <summary>
        /// Will remove the effects inflicted by the ability name provided
        /// </summary>
        /// <param name="AbilityName">Name of the ability whos effects will be removed</param>
        /// <param name="BypassDispelRestriction">(Optional) If true then the dispellable restriction will be ignored</param>
        public void RemoveAbilitiesEffects(string AbilityName, bool BypassDispelRestriction = false) {

            foreach (ActiveEffect activeEffect in ActiveEffects.SelectMany(k => k.Value).Where(t => t.abilityName == AbilityName).ToList()) {
                activeEffect.effect.Dispel(BypassDispelRestriction);
            }

        }


        /// <summary>
        /// Will dispel all Active Effects on the Entity, tagging them for removal. 
        /// </summary>
        public void RemoveAllActiveEffects() {

            foreach (ActiveEffect activeEffect in ActiveEffects.SelectMany(k => k.Value).ToList()) {
                activeEffect.effect.Dispel(true);
            }

        }

        /// <summary>
        /// Will clear the current active effect history log which is displayed in the editor
        /// </summary>
        public void ClearActiveEffectHistory() {

            this.editorActiveEffectHistory.Clear();

        }

        /// <summary>
        /// Will stop movement due to a hit
        /// </summary>
        public void HitStopMovement() {

            if (this.hitsStopMovement == false)
                return;

            StartCoroutine(meEntity.StopMovementForDuration(this.hitStopMovementDuration, true, this.hitStopMovementFreezePosition, this.hitStopMovementDisableComponents));

        }


        /// <summary>
        /// Will invoke the enable movement event
        /// </summary>
        public void RaiseEnableMovementEvent() {

            if (this.onEnableMovement != null)
                this.onEnableMovement();

        }

        /// <summary>
        /// Will invoke the diable movement event
        /// </summary>
        public void RaiseDisableMovementEvent() {

            if (this.onDisableMovement != null)
                this.onDisableMovement();

        }


        /// <summary>
        /// Will invoke the enable movement event
        /// </summary>
        public void RaiseEnableGravityEvent() {

            if (this.onEnableGravity != null)
                this.onEnableGravity();

        }

        /// <summary>
        /// Will invoke the diable movement event
        /// </summary>
        public void RaiseDisableGravityEvent() {

            if (this.onDisableGravity != null)
                this.onDisableGravity();

        }


        /// <summary>
        /// Will restore Health to max capacity 
        /// </summary>
        public void RestoreHealth() {

            this.currentHealth = this.currentMaxHealth;

        }


        /// <summary>
        /// Will adjust max health
        /// </summary>
        /// <param name="Amount">Amount to adjust max health by</param>
        /// <param name="RestoreFullHealth">If true then health will be restored to full</param>
        public void AdjustMaxHealth(float Amount, bool RestoreFullHealth = false) {

            this.currentMaxHealth += Amount;

            if (RestoreFullHealth)
                this.RestoreHealth();

        }


        /// <summary>
        /// Will reduce entities health by the amount provided
        /// </summary>
        /// <param name="Amount">Amount to reduce health by</param>
        public void AdjustHealth(float Amount) {
            this.currentHealth += Amount;


            //if amount was a minus number then display health reduction image
            if (Amount < 0)
                this.ShowHealthReductionImage();
        }


        /// <summary>
        /// Will adjust health regen rate
        /// </summary>
        /// <param name="Amount">Amount to adjust regen by</param>
        public void AdjustHealthRegen(float Amount) {
            this.healthRegenPerSecond += Amount;

            if (this.healthRegenPerSecond < 0)
                this.healthRegenPerSecond = 0;

        }

        /// <summary>
        /// Will display the setup health reduction image for the duration configured 
        /// </summary>
        /// <param name="BypassRestriction">If true then the image will show even if setup not too</param>
        public void ShowHealthReductionImage(bool BypassRestriction = false) {

            //If we are not set to show the health reduction image (and bypass restriction is not true) or an image has not been setup then end here
            if ((this.showGUIImageOnHealthReduction == false && BypassRestriction == false) || this.imageOnHealthReduction.RawImage == null)
                return;


            //Make image appear 
            this.imageOnHealthReduction.RawImage.enabled = true;
            this.imageOnHealthReduction.RawImage.CrossFadeAlpha(1f, 0f, false);

            //Crossfade the alpha to make it dissapear again within the duration
            this.imageOnHealthReduction.RawImage.CrossFadeAlpha(0f, this.imageOnHealthReductionDuration, false);



        }


        /// <summary>
        /// Will return the stat value asked for in the parameter
        /// </summary>
        /// <param name="StatName">Stat to find value for</param>
        /// <param name="IntegrationType">The integration type for stat functionality - if ABC is picked then normal stat functionality is used else stat from another integration system i.e game creator is used</param>>
        /// <param name="GCStatType">The type of GC stat to adjust: Stat or Attribute</param>
        /// <returns>Value of the stat</returns>
        public float GetStatValue(string StatName, ABCIntegrationType IntegrationType = ABCIntegrationType.ABC, GCStatType GCStatType = GCStatType.Stat) {

            //If integrating with GameCreator then retrieve stat from that Asset
            if (IntegrationType == ABCIntegrationType.GameCreator)
                return meEntity.GetGCStatValue(StatName, GCStatType);

            //If integrating with GameCreator 2 then retrieve stat from that Asset
            if (IntegrationType == ABCIntegrationType.GameCreator2)
                return meEntity.GetGC2StatValue(StatName, GCStatType);

            //else get stat from ABC
            EntityStat entityStat = this.EntityStatList.Where(stat => stat.statName == StatName).FirstOrDefault();

            if (entityStat != null)
                return this.EntityStatList.Where(stat => stat.statName == StatName).FirstOrDefault().GetValue();
            else
                return 0;


        }

        /// <summary>
        /// Will set the stat to the value provided
        /// </summary>
        /// <param name="StatName">Stat which will have its value set</param>
        /// <param name="Value">Amount to set value too</param>
        /// <param name="IntegrationType">The integration type for stat functionality - if ABC is picked then normal stat functionality is used else stat from another integration system i.e game creator is used</param>>
        /// <param name="GCStatType">The type of GC stat to adjust: Stat or Attribute</param>
        public void SetStatValue(string StatName, float Value, ABCIntegrationType IntegrationType = ABCIntegrationType.ABC, GCStatType GCStatType = GCStatType.Stat) {

            switch (IntegrationType) {
                case ABCIntegrationType.ABC:

                    EntityStat entityStat = this.EntityStatList.Where(stat => stat.statName == StatName).FirstOrDefault();

                    if (entityStat != null)
                        entityStat.SetValue(Value);

                    break;
                case ABCIntegrationType.GameCreator:

                    this.meEntity.SetGCStatValue(StatName, Value, GCStatType);

                    break;
                case ABCIntegrationType.GameCreator2:

                    this.meEntity.SetGC2StatValue(StatName, Value, GCStatType);

                    break;
            }


        }

        /// <summary>
        /// Will modify a stats value by the amount provided
        /// </summary>
        /// <param name="StatName">Stat which will have its value modified</param>
        /// <param name="Amount">Amount to increase or decrease the stat value by</param>
        /// <param name="IntegrationType">The integration type for stat functionality - if ABC is picked then normal stat functionality is used else stat from another integration system i.e game creator is used</param>>
        /// <param name="GCStatType">The type of GC stat to adjust: Stat or Attribute</param>
        public void AdjustStatValue(string StatName, float Amount, ABCIntegrationType IntegrationType = ABCIntegrationType.ABC, GCStatType GCStatType = GCStatType.Stat) {


            switch (IntegrationType) {
                case ABCIntegrationType.ABC:

                    EntityStat entityStat = this.EntityStatList.Where(stat => stat.statName == StatName).FirstOrDefault();

                    if (entityStat != null)
                        entityStat.AdjustValue(Amount);

                    break;
                case ABCIntegrationType.GameCreator:

                    this.meEntity.AdjustGCStatValue(StatName, Amount, GCStatType);

                    break;
                case ABCIntegrationType.GameCreator2:

                    this.meEntity.AdjustGC2StatValue(StatName, Amount, GCStatType);

                    break;
            }

        }


        /// <summary>
        /// Will return the % of melee damage mitigation the entity currently has
        /// </summary>
        /// <returns>Float value which determines the current melee damage mitigation</returns>
        public float GetMeleeDamageMitigationPercentage() {

            return this.mitigateMeleeDamagePercentage;

        }

        /// <summary>
        /// Will adjust the % of melee damage mitigation the entity currently has
        /// </summary>
        /// <param name="Amount">Float value of the amount to adjust the current melee damage mitigation by</param>
        public void AdjustMeleeDamageMitigationPercentage(float Amount) {

            this.mitigateMeleeDamagePercentage += Amount;



        }

        /// <summary>
        /// Will return the % of projectile and raycast damage mitigation the entity currently has
        /// </summary>
        /// <returns>Float value which determines the current projectile and raycast damage mitigation</returns>
        public float GetProjectileAndRayCastDamageMitigationPercentage() {

            return this.mitigateProjectileAndRaycastDamagePercentage;

        }

        /// <summary>
        /// Will adjust the % of projectile and raycast damage mitigation the entity currently has
        /// </summary>
        /// <param name="Amount">Float value of the amount to adjust the current of projectile and raycast damage mitigation by</param>
        public void AdjustProjectileAndRayCastDamageMitigationPercentage(float Amount) {

            this.mitigateProjectileAndRaycastDamagePercentage += Amount;

        }




        /// <summary>
        /// Will return if the entity is set to currently prevent melee effects from being applied 
        /// </summary>
        /// <returns>True if melee effects are set to be prevented, else false</returns>
        public bool IsPreventingMeleeEffects() {

            return this.preventMeleeEffects;
        }

        /// <summary>
        /// Toggles the prevent melee effect status 
        /// </summary>
        /// <param name="Enabled">If true prevent melee effects will be active, else false will disable it</param>
        public void TogglePreventMeleeEffects(bool Enabled) {
            this.preventMeleeEffects = Enabled;
        }

        /// <summary>
        /// Will return if the entity is set to currently prevent projectile and raycast effects from being applied 
        /// </summary>
        /// <returns>True if projectile and raycast effects are set to be prevented, else false</returns>
        public bool IsPreventingProjectileAndRayCastEffects() {

            return this.preventProjectileAndRayCastEffects;
        }

        /// <summary>
        /// Toggles the prevent projectile and raycast effect status 
        /// </summary>
        /// <param name="Enabled">If true prevent projectile and raycast effects will be active, else false will disable it</param>
        public void TogglePreventProjectileAndRayCastEffects(bool Enabled) {
            this.preventProjectileAndRayCastEffects = Enabled;
        }


        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will setup the component ready to be used, called OnEnable()
        /// </summary>
        /// <param name="SkipFullHealthRestore">Will skip restoring all health if set to true (use: set to true when loading a game from a save point)</param>
        private void InitialiseComponent(bool SkipFullHealthRestore = false) {

            //reset tracker so we will know once again when zero health is reached
            if (this.entityZeroHealthReached == true) {
                this.entityZeroHealthReached = false;

                //turn ability activation and collision back on
                meEntity.ToggleAbilityActivation(true);
                this.ToggleIgnoreAbilityCollision(false);
            }


            // turn off health GUI
            this.ShowHealthGUI(false);

            // turn off stat gui 
            this.ShowStatsGUI(false);

            // start to regen health 
            InvokeRepeating("RegenHealth", 1f, 1f);

            // Refresh who is currently targeting us
            InvokeRepeating("ResetAllCurrentTargeters", 1f, 10f);

            //Restore Health?
            if (this.fullHealthOnEnable == true && SkipFullHealthRestore == false)
                Invoke("RestoreHealth", 0.8f);
        }

        /// <summary>
        /// Will create the EffectText Graphic object and place them into a pool. This is then normally used to display effect information in the game world. 
        /// </summary>
        /// <param name="createOne"></param>
        /// <returns></returns>
        private GameObject CreateEffectTextObjects(bool createOne = false) {


            GameObject effectTextObj = null;

            // if we are showing effects on screen then load the canvas
            if (this.effectGraphicTextGUI.GameObject != null) {

                //how many objects to make
                float objCount = createOne ? 1 : 4;


                // create effect texts and place in ABCPool
                for (int i = 0; i <= objCount; i++) {

                    GameObject effectText = (GameObject)(Instantiate(effectGraphicTextGUI.GameObject));
                    effectText.name = effectGraphicTextGUI.GameObject.name;

                    //disable object and place in pool
                    ABC_Utilities.PoolObject(effectText);


                    EffectTextOnScreenPool.Add(effectText);

                }
            }

            return effectTextObj;

        }

        /// <summary>
        /// Will pool all objects setup by this component. 
        /// </summary>
        private void CreatePools() {

            //If pooling is not enabled then end here, objects will be created on the way
            if (this.enablePooling == false)
                return;

            this.CreateEffectTextObjects();


        }


        /// <summary>
        /// Will manage health calling events depending on the situation, like calling zero health manager when health reaches 0
        /// </summary>
        private void HealthManager() {

            // keep health GUI up to date
            this.UpdateHealthGUI();

            //If health isn't at 0 then end here
            if (currentHealth > 0) {

                //reset tracker so we will know once again when zero health is reached
                if (this.entityZeroHealthReached == true) {

                    //turn ability activation and collision back on
                    meEntity.ToggleAbilityActivation(true);
                    this.ToggleIgnoreAbilityCollision(false);

                    this.entityZeroHealthReached = false;
                }

                return;
            }

            // Zero health has been reached so call ZeroHealthManager
            StartCoroutine(ZeroHealthManager());

        }

        /// <summary>
        /// Function will manage the events that happen if entity hits 0 health; including: disabling, stopping movement, swapping models and playing any animations. 
        /// </summary>
        private IEnumerator ZeroHealthManager() {

            //If set then stop movement for a second due to zero health
            if (this.stopMovementOnZeroHealth)
                StartCoroutine(meEntity.StopMovementForDuration(0.5f, true, this.stopMovementOnZeroHealthFreezePosition, stopMovementOnZeroHealthDisableComponents));

            //Stop AI Nav
            meEntity.ClearAINavigation();


            //If zerohealth was already reached then end here but make sure death animation is still playing
            if (entityZeroHealthReached == true) {
                this.StartAndStopAnimationRunner(StateManagerAnimationState.ZeroHealth, meEntity.animationRunner);
                yield break;
            }

            //let rest of script know that the entity hit 0 so we don't repeat any any animations etc
            entityZeroHealthReached = true;


            //Stop any abilities that are activating and any future abilities from activating
            meEntity.InterruptAbilityActivation();
            meEntity.ToggleAbilityActivation(false);

            //Stop any effect collision
            this.ToggleIgnoreAbilityCollision(true);


            //Swap models if set to (can change to a fractured model)
            if (this.swapModelOnZeroHealth) {

                //Turn off model to disable
                if (this.swapModelToDisable.GameObject != null && this.swapModelToDisable.GameObject.activeInHierarchy)
                    this.swapModelToDisable.GameObject.SetActive(false);

                //Turn on model to enable
                if (this.swapModelToEnable.GameObject != null && this.swapModelToEnable.GameObject.transform.IsChildOf(meTransform))
                    this.swapModelToEnable.GameObject.SetActive(true);

            }


            //If health has just reached zero then disable the entity if set too 
            if (this.disableEntityOnZeroHealth)
                StartCoroutine(meEntity.Disable(this.disableDelay));


            //Play any zero health animations
            this.StartAnimation(StateManagerAnimationState.ZeroHealth, meEntity.animator);
            this.StartAndStopAnimationRunner(StateManagerAnimationState.ZeroHealth, meEntity.animationRunner);


            yield return new WaitForSeconds(this.zeroHealthAnimationAnimatorDuration);


            this.EndAnimation(StateManagerAnimationState.ZeroHealth, meEntity.animator);


        }


        /// <summary>
        /// Will Regen and increase health by the amount determined in the settings.
        /// </summary>
        private void RegenHealth() {

            if (this.currentMaxHealth > 0) {

                // if we have health and it's not at 0 then regen it every second (when this is called)
                if (this.entityZeroHealthReached == false && this.currentHealth != 0 && this.healthRegenPerSecond > 0 && this.currentHealth < this.currentMaxHealth)
                    this.currentHealth += this.healthRegenPerSecond;


                // if we accidently go over the limit then set it back to the max
                if (this.currentHealth > this.currentMaxHealth)
                    this.currentHealth = this.currentMaxHealth;

                // if we go under 0 then set it to 0 
                if (this.currentHealth < 0)
                    this.currentHealth = 0;

            } else {
                // revert health to 0 as it's not used
                this.currentHealth = 0;
            }

        }

        /// <summary>
        /// Updates the Health GUIs to show the current health values
        /// </summary>
        private void UpdateHealthGUI() {

            // If no GUI has been setup then finish here
            if (HealthGUIList.Count == 0)
                return;


            foreach (HealthGUI gui in HealthGUIList) {

                gui.UpdateGUI(this.currentHealth, this.currentMaxHealth);

            }



        }



        /// <summary>
        /// Updates the Entity Stats GUIs to show the current Stats values
        /// </summary>
        private void UpdateEntityStatsGUI() {

            // If no stats have been setup then finish here
            if (EntityStatList.Count == 0)
                return;


            foreach (EntityStat stat in EntityStatList) {

                if (stat.textStatName.text != null && stat.textShowing == true)
                    stat.textStatName.text = stat.statName;

                if (stat.textStatValue.text != null && stat.textShowing == true)
                    stat.textStatValue.text = stat.GetValue().ToString();
            }



        }

        /// <summary>
        /// Will Start and stop the animation clip using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to play - disable etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        private void StartAndStopAnimationRunner(StateManagerAnimationState State, ABC_AnimationsRunner AnimationRunner) {


            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipSpeed = 1f;
            float animationClipDelay = 0f;
            float animationClipDuration = 0f;
            AvatarMask animationClipMask = null;


            switch (State) {
                case StateManagerAnimationState.ZeroHealth:

                    animationClip = this.zeroHealthAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.zeroHealthAnimationRunnerClipSpeed;
                    animationClipDelay = this.zeroHealthAnimationRunnerClipDelay;
                    animationClipMask = this.zeroHealthAnimationRunnerMask.AvatarMask;
                    animationClipDuration = this.zeroHealthAnimationRunnerClipDuration;

                    break;
            }



            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null)
                return;

            //If animation is not running then play
            if (AnimationRunner.IsAnimationRunning(animationClip) == false)
                AnimationRunner.PlayAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipDuration, animationClipMask, true);


        }

        /// <summary>
        /// Starts an animation clip using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to play - disable etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        private void StartAnimationRunner(StateManagerAnimationState State, ABC_AnimationsRunner AnimationRunner) {


            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipSpeed = 1f;
            float animationClipDelay = 0f;
            AvatarMask animationClipMask = null;


            switch (State) {
                case StateManagerAnimationState.ZeroHealth:

                    animationClip = this.zeroHealthAnimationRunnerClip.AnimationClip;
                    animationClipSpeed = this.zeroHealthAnimationRunnerClipSpeed;
                    animationClipDelay = this.zeroHealthAnimationRunnerClipDelay;
                    animationClipMask = this.zeroHealthAnimationRunnerMask.AvatarMask;

                    break;
            }



            // if animator parameter is null or animation runner is not given then animation can't start so end here. 
            if (animationClip == null || AnimationRunner == null)
                return;


            AnimationRunner.StartAnimation(animationClip, animationClipDelay, animationClipSpeed, animationClipMask, true, true);


        }

        /// <summary>
        /// End an animation clip currently playing using the ABC animation runner
        /// </summary>
        /// <param name="State">The animation to play - disable etc</param>
        /// <param name="AnimationRunner">The ABC Animation Runner component to manage the animation clip</param>
        /// <param name="Delay">(Optional) Delay before animation ends</param>
        private void EndAnimationRunner(StateManagerAnimationState State, ABC_AnimationsRunner AnimationRunner, float Delay = 0f) {

            // set variables to be used later 
            AnimationClip animationClip = null;
            float animationClipDuration = 0f;


            switch (State) {
                case StateManagerAnimationState.ZeroHealth:

                    animationClip = this.zeroHealthAnimationRunnerClip.AnimationClip;
                    animationClipDuration = this.zeroHealthAnimationRunnerClipDuration;

                    break;
            }



            // if animator parameter is null or animation runner is not given then end here. 
            if (animationClip == null || AnimationRunner == null)
                return;

            AnimationRunner.EndAnimation(animationClip, animationClipDuration);
        }




        /// <summary>
        /// Starts an animation for the statemanager depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to play - disable etc</param>
        /// <param name="Animator">Animator component</param>
        private void StartAnimation(StateManagerAnimationState State, Animator Animator) {


            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOnValue = "";



            switch (State) {
                case StateManagerAnimationState.ZeroHealth:

                    animatorParameterType = this.zeroHealthAnimationAnimatorParameterType;
                    animatorParameter = this.zeroHealthAnimationAnimatorParameter;
                    animatorOnValue = this.zeroHealthAnimationAnimatorOnValue;

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
        /// Ends the animation for the statemanager depending on what state is passed through
        /// </summary>
        /// <param name="State">The animation to stop - disable etc</param>
        /// <param name="Animator">Animator component</param>
        private void EndAnimation(StateManagerAnimationState State, Animator Animator) {

            // set variables to be used later 
            AnimatorParameterType animatorParameterType = AnimatorParameterType.Trigger;
            string animatorParameter = "";
            string animatorOffValue = "";



            switch (State) {
                case StateManagerAnimationState.ZeroHealth:

                    animatorParameterType = this.zeroHealthAnimationAnimatorParameterType;
                    animatorParameter = this.zeroHealthAnimationAnimatorParameter;
                    animatorOffValue = this.zeroHealthAnimationAnimatorOffValue;

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
        /// Activates a random hit animation
        /// </summary>
        /// <param name="PrioritiseAirAnimations">If true then an air animation will be used, else if false air animation will be used depnding on if entity is in air or not</param>
        /// <remarks>
        /// Function will cycle through all the enabled animations activating the first one that can be activated (depending on dice rolls)
        /// </remarks>
        private void ActivateHitAnimation(bool PrioritiseAirAnimations = false) {

            //if reloading then don't play hit animation (not on other specific hit animation method on purpose incase it's a push/fall etc)
            if (meEntity.IsReloading() == true)
                return;

            // Grab a list of the enabled hit animations
            List<HitAnimation> enabledHitAnimations = HitAnimations.Where(item => item.hitAnimationEnabled == true && item.hitAnimationActivateFromEffectOnly == false).ToList();

            //filter by air if they exist and entity is in the air 
            if ((meEntity.isInTheAir == true || PrioritiseAirAnimations == true) && enabledHitAnimations.Where(ha => ha.hitAnimationAirRunnerClips.Count() > 0).Count() > 0)
                enabledHitAnimations = enabledHitAnimations.Where(ha => ha.hitAnimationAirRunnerClips.Count() > 0).ToList();

            //If list is empty then we can end function here
            if (enabledHitAnimations.Count == 0)
                return;

            //Determines the chosen hit animation which is decided depending on dice rolls 
            HitAnimation selectedHitAnimation = null;
            //Variable used to escape the while loop
            bool animationSelected = false;

            //If there is only one animation in the list then we will just select 
            if (enabledHitAnimations.Count == 1) {
                selectedHitAnimation = enabledHitAnimations.First();
                animationSelected = true;
            }


            // If we are randoming the hit animations (changes probability of animations happening) and the animation hasn't already been selected then switch around the list. 
            if (this.randomizeHitAnimations == true && animationSelected == false) {
                for (int t = 0; t < enabledHitAnimations.Count; t++) {
                    var tmp = enabledHitAnimations[t];
                    int r = Random.Range(t, enabledHitAnimations.Count);
                    enabledHitAnimations[t] = enabledHitAnimations[r];
                    enabledHitAnimations[r] = tmp;
                }
            }


            //Loop through animations until we find one that can activate (dice roll is between it's min and max probability)
            while (animationSelected == false) {

                foreach (HitAnimation animation in enabledHitAnimations) {

                    //If the animation can activate then set the selected hit animation and break out of for each and while loop
                    if (animation.CanActivate()) {
                        selectedHitAnimation = animation;
                        animationSelected = true;
                        break;
                    }
                }

            }

            //activate the selected animation
            if (selectedHitAnimation != null)
                selectedHitAnimation.ActivateAnimation(meEntity, PrioritiseAirAnimations);

        }

        /// <summary>
        /// Activates a hit animation defined by the string name given
        /// </summary>
        /// <param name="HitAnimationName">Name of the hit animation to activate</param>
        /// <param name="PlayRandomAnimationIfNotFound">If true then a random hit animation will play if the specific hit animation set to activate is not found</param>
        /// <param name="PrioritiseAirAnimations">If true then an air animation will be used, else if false air animation will be used depnding on if entity is in air or not</param>
        /// <remarks>
        /// Function will cycle through all the enabled animations activating the first one that can be activated (depending on dice rolls)
        /// </remarks>
        private void ActivateHitAnimation(string HitAnimationName, bool PlayRandomAnimationIfNotFound = false, bool PrioritiseAirAnimations = false) {

            // Grab the animation by name list of the enabled hit animations
            HitAnimation enabledHitAnimations = HitAnimations.Where(item => item.hitAnimationName.Trim() == HitAnimationName.Trim() && item.hitAnimationEnabled == true).FirstOrDefault();

            //If list is empty then we can end function here
            if (enabledHitAnimations == null) {

                //If set to play random animation if specific hit animation is not found then call the normal activate hit animation function
                if (PlayRandomAnimationIfNotFound == true)
                    this.ActivateHitAnimation(PrioritiseAirAnimations);

                //Return here as hit animation not found 
                return;

            }


            //activate the selected animation
            enabledHitAnimations.ActivateAnimation(meEntity);

        }


        /// <summary>
        /// Will count how many of the effect provided is currently activate
        /// </summary>
        /// <param name="Effect">The Effect which will be counted to see how many of this effect is currently active</param>
        /// <param name="Originator">Entity that applied the effect</param>
        /// <returns>Will return an integer which is a count of how many of the effect provided is currently active</returns>
        private int CountDuplicateActiveEffect(Effect Effect, ABC_IEntity Originator) {

            //Start with 0
            int retVal = 0;

            //If the activate effects list contains effects added by the originator passed in the function 
            if (ActiveEffects.ContainsKey(Originator)) {

                //Count how many of the effect provided is currently active
                retVal = ActiveEffects[Originator].Where(c => c.effect.effectID == Effect.effectID && c.effect.effectAbilityID == Effect.effectAbilityID).Count();

            }

            //return result
            return retVal;


        }

        /// <summary>
        /// Returns a bool determining if the effect can be added (checks duplication flags etc)
        /// </summary>
        /// <param name="AbilityName">Name of the ability</param>
        /// <param name="Effect">Effect to add</param>
        /// <param name="Originator">Entity that applied the effect</param>
        /// <param name="ObjectHit">Object Hit by ability</param>
        /// <param name="WasCriticalHit">If ability did a critical hit</param>
        /// <returns>True if effect can be added, else false</returns>
        private bool EffectCanBeAdded(string AbilityName, Effect Effect, ABC_IEntity Originator, GameObject ObjectHit = null, bool WasCriticalHit = false) {

            //Check if effect can activate 
            if (Effect.CanActivate(Originator, meEntity, ObjectHit) == false)
                return false;

            //If crit is required to activate and critical hit didn't occur then return false 
            if (Effect.requireCriticalHitToActivate == true && WasCriticalHit == false)
                return false;

            // Check if we can add the effect due to duplicate effect activation being turned on and the entity the effect is being applied too isn't set to block duplicate effect activation
            if (Effect.allowDuplicateEffectActivation == true && this.blockDuplicateEffectActivation == false) {

                // return true if the effect can be added as: duplicate effect activations are allowed and we have not reached the limit
                if (Effect.limitNoOfDuplicateEffectActivations == false || Effect.limitNoOfDuplicateEffectActivations == true && this.CountDuplicateActiveEffect(Effect, Originator) < Effect.maxNoDuplicateEffectActivations)
                    return true;

            }


            // if effect isn't already active then a new one can be added
            if (IsEffectActive(AbilityName, Effect, Originator) == false) {
                return true;
            } else {
                return false;
            }

        }


        /// <summary>
        /// Function to add and activate new effects. Will wait for delays/range etc. Also checks if the effect can be added before adding/activating. 
        /// </summary>
        /// <param name="AbilityName"> The name of the ability which applied the effect (can be used later on to retrieve effects by ability name to dispel etc)</param> 
        /// <param name="Effects">Effects to add</param>
        /// <param name="AfflictingOriginator">Entity that applied the effect</param>
        /// <param name="AbilityType">The type of ability which applied the effect (projectile, raycast, melee)</param>
        /// <param name="WasCriticalHit">Determines if the potency has been modified by a critical hit</param>
        /// <param name="Projectile">(Optional) Object that we collided with which started the process of adding an effect - Used in some range settings etc</param>
        /// <param name="AbilityHitPoint">(Optional) The vector position where the ability collided, if setup correctly this will play the effect graphics at that position</param>
        private IEnumerator ActivateEffects(string AbilityName, List<Effect> Effects, ABC_IEntity AfflictingOriginator, AbilityType AbilityType, bool WasCriticalHit = false, GameObject Projectile = null, GameObject ObjectHit = null, Vector3 AbilityHitPoint = default(Vector3)) {

            if (enabled == false)
                yield break;

            // go through list of  effects
            foreach (Effect effect in Effects) {

                //Create effect copy so it's not modified by being applied or modified 
                Effect effectToActivate = new Effect();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effectToActivate);

                yield return StartCoroutine(effectToActivate.WaitForActivationDelay());


                // double check the effect is in range of target (us) and hasn't already been activated unless the effect can be applied multiple times and multiple effect activation isn't being blocked by this object
                if (EffectCanBeAdded(AbilityName, effectToActivate, AfflictingOriginator, ObjectHit, WasCriticalHit) == false || effectToActivate.TargetInRange(AbilityName, meEntity, Projectile) == false) {

                    // log that ability could not activate 
                    effectToActivate.LogEffectText(meEntity, AbilityName, EffectAction.NoEffect);

                    continue;

                }




                ActiveEffect activeEffect = new ActiveEffect(effectToActivate, AbilityHitPoint, AbilityName);

                // add the caster and the effect to dictionary 
                if (ActiveEffects.ContainsKey(AfflictingOriginator)) {

                    ActiveEffects[AfflictingOriginator].Add(activeEffect);

                } else {

                    // add new origin and ability and effect  
                    ActiveEffects.Add(AfflictingOriginator, new List<ActiveEffect> { activeEffect });

                }

                //record for history log which is shown in editor
                if (this.editorRecordActiveEffectHistory)
                    this.editorActiveEffectHistory.Add(System.DateTime.Now + ": " + activeEffect.effect.effectName + " from " + activeEffect.abilityName + " was activated on " + gameObject.name + " by " + AfflictingOriginator.gameObject.name);


                // activate effect and store the coroutine so we can interupt the activation later if required
                StartCoroutine(activeEffect.ActivateCoroutine = activeEffect.effect.Activate(activeEffect.abilityName, meEntity, AfflictingOriginator, AbilityType, WasCriticalHit, activeEffect.hitPoint, Projectile));

                AfflictingOriginator.AddToDiagnosticLog(activeEffect.abilityName + " " + activeEffect.effect.effectName + " effect is now active on " + gameObject.name);

            }

        }


        /// <summary>
        /// Will remove an active effect from the entity cancelling anything it is currently doing. 
        /// </summary>
        /// <param name="activeEffect">Effect to remove</param>
        /// <param name="castingOriginator">Entity that applied the effect</param>
        private void RemoveActiveEffect(ActiveEffect activeEffect, ABC_IEntity castingOriginator) {


            // stop the effect from activating
            StopCoroutine(activeEffect.ActivateCoroutine);

            // remove effect
            ActiveEffects[castingOriginator].Remove(activeEffect);

            //run Deactivate code
            activeEffect.effect.Deactivate(activeEffect.abilityName, meEntity, castingOriginator);


            //Inform user effect has been removed
            castingOriginator.AddToDiagnosticLog(activeEffect.effect.effectName + " Has been removed from " + meTransform.name);

        }


        /// <summary>
        /// Function loops through all active effects checking if the effect duration is up. If it is then it will call the RemoveActiveEffect function
        /// </summary>
        private void ActiveEffectsWatcher() {


            foreach (ABC_IEntity castingOriginator in ActiveEffects.Keys.ToList()) {

                //If no effects are currently active from any entities then we can remove them.
                if (ActiveEffects[castingOriginator].Count == 0) {
                    ActiveEffects.Remove(castingOriginator);
                    // continue to next in list
                    continue;
                }


                foreach (ActiveEffect activeEffect in ActiveEffects[castingOriginator].ToList()) {

                    // duration is up on effect so we can remove it now 
                    if (activeEffect.effect.DurationReached(activeEffect.activationTime))
                        RemoveActiveEffect(activeEffect, castingOriginator);
                }
            }
        }


        /// <summary>
        /// Function is called by most OnCollision methods to stop duplicate code.
        /// </summary>
        /// <remarks>
        /// At the moment all this does is force a collision on any projectiles that this entity is a surrounding object for (acting like a child collider for the projectile) 
        /// </remarks>
        /// <param name="CollidedObj">The collided object</param>
        /// <param name="Type">Type of Collision</param> 
        /// <param name="HitPoint">Collision contact point</param> 
        private void OnCollision(GameObject CollidedObject, CollisionType Type, Vector3 HitPoint = default(Vector3)) {


            // if this script is disabled then end here
            if (enabled == false)
                return;


            // If we collided with an object but we are currently a surrounding object then activate collision on our projectile parent instead (We are just a child collider right now)
            if (this.gameObject.name.Contains("*_ABCSurroundingObject") && this.surroundingObjectLinkedProjectile != null)
                this.surroundingObjectLinkedProjectile.GetComponent<ABC_Projectile>().ActivateCollision(CollidedObject, Type, HitPoint);




        }


        #endregion

        // ********************* Game ********************

        #region Game

        void OnCollisionEnter(Collision col) {

            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnEnter, col.contacts.FirstOrDefault().point);


        }


        void OnTriggerEnter(Collider col) {

            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnEnter, meTransform.position);

        }



        void OnCollisionStay(Collision col) {

            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnStay, col.contacts.FirstOrDefault().point);

        }


        void OnTriggerStay(Collider col) {

            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnStay, meTransform.position);

        }


        void OnCollisionExit(Collision col) {

            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnExit, col.contacts.FirstOrDefault().point);

        }


        void OnTriggerExit(Collider col) {


            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.OnExit, meTransform.position);

        }


        //Make sure to turn on Send Message on particle

        void OnParticleCollision(GameObject col) {


            //Run method to add effects if the collision is a projectile
            this.OnCollision(col.gameObject, CollisionType.Particle);
        }


        void Awake() {

            // setup pool for future use 
            CreatePools();

            meTransform = transform;

            // if object doesn't have ABC Controller (which should have priority in creating the ABC entity) then create the entity interface
            // entity interface which holds information regarding the entity using this component
            if (ABC_Utilities.TraverseObjectForComponent(meTransform.gameObject, typeof(ABC_Controller)) as ABC_Controller == null) {
                meEntity = ABC_Utilities.GetStaticABCEntity(gameObject);

                //Used for when domain loading is off
                if (this.meEntity.HasABCStateManager() == false)
                    this.meEntity.ReSetupEntity();

            }



        }


        // Use this for initialization
        void OnEnable() {

            //Initialise the component setting everything up ready to be used
            this.InitialiseComponent();

        }


        void OnDisable() {

            // remove all state effects 
            RemoveAllActiveEffects();

            // remove any target indicator 

            Transform targetIndicator = meTransform.Find("ABC_TargetIndicator");
            if (targetIndicator != null) {
                targetIndicator.parent = null;
                targetIndicator.gameObject.SetActive(false);
            }

            Transform softTargetIndicator = meTransform.Find("ABC_SoftTargetIndicator");
            if (softTargetIndicator != null) {
                softTargetIndicator.parent = null;
                softTargetIndicator.gameObject.SetActive(false);
            }


            // reset health incase we respawn 
            if (this.fullHealthOnEnable == true)
                currentHealth = currentMaxHealth;

            gameObject.SetActive(false);


            // stop any invokes
            CancelInvoke();
            StopAllCoroutines();

        }


        void Update() {

            //If time has been paused then stop update from running
            if (Time.timeScale == 0)
                return;

            //Call health manager which will deal with what happens when entity runs out of health
            this.HealthManager();

            //Keep entity stat GUI up to date
            this.UpdateEntityStatsGUI();

            // do we need to remove any effects?
            this.ActiveEffectsWatcher();

        }


        // ********* ENUMS for StateManager **********************


        #region StateManager ENUMs


        private enum StateManagerAnimationState {

            ZeroHealth = 1
        }




        #endregion

        #endregion
    }
}