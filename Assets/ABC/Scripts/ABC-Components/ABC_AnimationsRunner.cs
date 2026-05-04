using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ABCToolkit {
    /// <summary>
    /// The Animation runner component will allow for animation clips to play without the use of the animator. 
    /// ABC will attach clips to the component and run them. Requires an Animator component to be attached to the object however no animations 
    /// are required to be setup on the Animator. If an Animator component doesn't exist one is added 
    /// </summary>
    public class ABC_AnimationsRunner : MonoBehaviour {

        // ********************* Settings ********************
        #region Settings

        /// <summary>
        /// The animation clip to run
        /// </summary>
        public AnimationClip animationClip;

        /// <summary>
        /// Avatar mask for the animation clip running
        /// </summary>
        public AvatarMask avatarMask;

        /// <summary>
        /// Tracks how far through the animation the clip is (0-100%)
        /// </summary>
        public float clipProgress = 0f;


        /// <summary>
        /// If enabled then the animation will play, used for debugging
        /// </summary>
        public bool Play = false;


        /// <summary>
        /// If true then debug mode is enabled
        /// </summary>
        public bool debugMode = false;


#if ABC_GC_2_Integration

    /// <summary>
    /// If true then root motion will be applied (GC 2 only)
    /// </summary>
    public bool applyRootMotion = true;    

#endif

        #endregion

        // ********************* Variables ********************

        #region Variables

        /// <summary>
        /// Animator Component - needs to exist but doesn't need to have the animations
        /// </summary>
        private Animator meAni;

        /// <summary>
        /// ABC entity
        /// </summary>
        private ABC_IEntity abcEntity;

        /// <summary>
        /// Will record when the last animation started, to allow for queueing if animations are executed to run at same time
        /// </summary>
        private float lastRunTime = 0;

        /// <summary>
        /// If true then the current animation playing will not be overwritten by any new runner overrides updates
        /// </summary>
        private bool blockRunnerOverrides = false;

        /// <summary>
        /// If true then the next override animation to play will not blend and will activate instantly 
        /// </summary>
        private bool quickStartNextClipOverride = false;

        /// <summary>
        /// List over animator clip overrides (Animation Clips which will run instead of any animations currently playing on the Animator which has a name that contains
        /// the text defined. For example play an Idle animation clip instead of any animation in the Animator that contains the name 'Idle')
        /// </summary>
        private List<ABC_Controller.AnimatorClipRunnerOverride> AnimatorClipRunnerOverrides = new List<ABC_Controller.AnimatorClipRunnerOverride>();

        /// <summary>
        /// If provided then this clip will override the 'current animator animation' 
        /// </summary>
        private AnimationClip currentAnimatorAnimationOverride = null;

        /// <summary>
        /// The playable graph used to run the animation
        /// </summary>
        private PlayableGraph playableGraph;

        /// <summary>
        /// Will keep track of old graphs not yet deleted due to courotines ending early
        /// </summary>
        private List<PlayableGraph> oldPlayableGraphs = new List<PlayableGraph>();

        /// <summary>
        /// The animation script playable currently in use (used for full body IK)
        /// </summary>
        private AnimationScriptPlayable scriptPlayable;

        /// <summary>
        /// The mixerPlayable currently in use
        /// </summary>
        private AnimationMixerPlayable mixerPlayable;

        /// <summary>
        /// The animation layer mixer currently in use
        /// </summary>
        private AnimationLayerMixerPlayable animationLayerMixer;

        /// <summary>
        /// The variable to track the animation currently running
        /// </summary>
        private AnimationClipPlayable clipPlayable;

        /// <summary>
        /// The variable to track the animation currently running on the Animator
        /// </summary>
        private AnimationClipPlayable clipPlayableAnimator;

        /// <summary>
        /// The variable to track the animation currently running on the Animator (with a mask)
        /// </summary>
        private AnimationLayerMixerPlayable clipPlayableAnimatorAvatarMask;

        /// <summary>
        /// Tracks the run animation method currently running the animation clip, used to interupt the method if another clip is played
        /// </summary>
        private List<IEnumerator> aniRunCoroutines = new List<IEnumerator>();

        /// <summary>
        /// Tracks the stop animation method currently stopping the animation clip, used to interupt the method if another clip is ended
        /// </summary>
        private List<IEnumerator> aniStopCoroutines = new List<IEnumerator>();

        /// <summary>
        /// Tracks the run and stop animation method currently starting and ending the animation clip in one call, used to interupt the method if another clip is played
        /// </summary>
        private List<IEnumerator> aniRunStopCoroutines = new List<IEnumerator>();

        /// <summary>
        /// Can pause/play the zero layer avatar mask updating 
        /// </summary>
        private bool pauseZeroLayerCoroutine = true;


        #endregion

        // ********************* Public Methods ********************

        #region Public Methods


        /// <summary>
        /// Will start and stop an animation using the clip provided after the delay and duration set. 
        /// </summary>
        /// <param name="Animation">Animation clip to play</param>
        /// <param name="Delay">Delay before animation clip runs</param>
        /// <param name="Speed">The speed of the animation clip</param>
        /// <param name="Duration">The duration before the animation ends, if 0 is provided then the duration will last for the whole animation clip</param>
        /// <param name="AvatarMask">(Optional) If provided then animation clip will run using the avatar mask</param>
        /// <param name="InterruptCurrentAnimation">(Optional) If true then this animation will interrupt any animations currently playing, else if false then the animation won't play if one is currently running</param>
        public void PlayAnimation(AnimationClip Animation, float Delay = 0f, float Speed = 1f, float Duration = 0f, AvatarMask AvatarMask = null, bool InterruptCurrentAnimation = true, bool SkipBlending = false) {

            //If set to not interrupt any current animations and one is playing then return here
            if (InterruptCurrentAnimation == false && this.playableGraph.IsValid() && this.playableGraph.IsPlaying()) {
                return;
            }


            //If any run and stop animations are currently in progress then stop them ready for the new one
            if (this.aniRunStopCoroutines.Count > 0) {
                this.aniRunStopCoroutines.ForEach(c => StopCoroutine(c));
                this.aniRunStopCoroutines.Clear();
            }

            //Track the animation run and stop method which is about to activate
            IEnumerator newCoroutine = null;

            //Run animation and then stop it after duration 
            StartCoroutine((newCoroutine = RunAndStopAnimation(Animation, Delay, Speed, Duration, AvatarMask, SkipBlending)));

            //Add the method to the tracker to remove early if required (another animation may start early) 
            this.aniRunStopCoroutines.Add(newCoroutine);

        }

        /// <summary>
        /// Will start the animation clip after the delay
        /// </summary>
        /// <param name="Animation">Animation clip to play</param>
        /// <param name="Delay">Delay before animation clip runs</param>
        /// <param name="Speed">(Optional) The speed of the animation clip</param>
        /// <param name="AvatarMask">(Optional) If provided then animation clip will run using the avatar mask</param>
        /// <param name="InterruptCurrentAnimation">(Optional) If true then this animation will interrupt any animations currently playing, else if false then the animation won't play if one is currently running</param>
        /// <param name="BlockRunnerOverrides">If true then runner override updates will be blocked whilst this new animation is running</param>
        /// <param name="SkipBlending">If true then blending will not occur and animation will play instantly</param>
        /// <param name="CurrentAnimationOverride">If provided then this clip will override the 'current animator animation playing' as a base animation/param>
        /// <returns>True if animation started, else false</returns>
        public bool StartAnimation(AnimationClip Animation, float Delay, float Speed = 1f, AvatarMask AvatarMask = null, bool InterruptCurrentAnimation = true, bool BlockRunnerOverrides = false, bool SkipBlending = false, AnimationClip CurrentAnimationOverride = null) {

            //If set to not interrupt any current animations and one is playing then return here
            if (InterruptCurrentAnimation == false && this.playableGraph.IsValid() && this.animationClip != null) {
                return false;
            }

            if (this.aniRunCoroutines.Count > 0) {
                this.aniRunCoroutines.ForEach(c => StopCoroutine(c));
                this.aniRunCoroutines.Clear();
            }

            //If any run and stop animations are currently in progress then stop them ready for the new one
            if (this.aniStopCoroutines.Count > 0) {
                this.aniStopCoroutines.ForEach(c => StopCoroutine(c));
                this.aniStopCoroutines.Clear();
            }


            //Store current animation clip/mask running
            AnimationClip currentRunningAnimationClip = this.animationClip;
            AvatarMask currentRunningAvatarMask = this.avatarMask;

            //Store any current animation clip overrides
            this.currentAnimatorAnimationOverride = CurrentAnimationOverride;

            //Show in editor what animation is playing
            this.animationClip = Animation;

            //set avatar mask
            this.avatarMask = AvatarMask;

            //Reset clip run time
            this.clipProgress = 0f;

            //Determine if we blocking runner overrides for this animation clip 
            this.blockRunnerOverrides = BlockRunnerOverrides;

            //Track the animation run  method which is about to activate
            IEnumerator newCoroutine = null;

            //Run animation 
            StartCoroutine(newCoroutine = RunAnimation(Animation, Delay, Speed, currentRunningAnimationClip, currentRunningAvatarMask, SkipBlending));

            //Add the method to the tracker to remove early if required (another animation may start early) 
            this.aniRunCoroutines.Add(newCoroutine);

            //Animation started so return true
            return true;
        }

        /// <summary>
        /// Will end the animation that is currently playing, unless another one has started
        /// </summary>
        /// <param name="Animation">Animation clip to end</param>
        /// <param name="Delay">Delay before animation clip stops</param>   
        /// <param name="SkipBlending">If true then blending will be skipped</param>
        public void EndAnimation(AnimationClip Animation, float Delay = 0f, bool SkipBlending = false) {

            //If the animation currently playing does not match the animation we are ending
            //then end here as another animation has already started
            if (Animation != this.animationClip)
                return;

            //If any run and stop animations are currently in progress then stop them ready for the new one
            if (this.aniStopCoroutines.Count > 0) {
                this.aniStopCoroutines.ForEach(c => StopCoroutine(c));
                this.aniStopCoroutines.Clear();
            }

            //Track the animation run and stop method which is about to activate
            IEnumerator newCoroutine = null;

            //If the entity is no longer active then finish here as there is no animation to end
            if (this.gameObject.activeInHierarchy == false)
                return;

            //Run animation and then stop it after duration 
            StartCoroutine((newCoroutine = this.StopAnimation(Delay, SkipBlending)));

            //Add the method to the tracker to remove early if required (another animation may start early) 
            this.aniStopCoroutines.Add(newCoroutine);
        }

        /// <summary>
        /// Will determine if the animation clip provided is already running
        /// </summary>
        /// <param name="Animation">Animation to check if currently being played</param>
        /// <returns>True if animation clip is currently playing, else false</returns>
        public bool IsAnimationRunning(AnimationClip Animation) {

            if (this.playableGraph.IsValid() && this.animationClip == Animation)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Will determine if the animation clip override is currently running
        /// </summary>
        /// <returns>True if animation clip ovverride is currently playing, else false</returns>
        public bool IsAnimationClipOverrideRunning() {

            if (this.playableGraph.IsValid() && this.AnimationIsAClipOverride(this.animationClip))
                return true;
            else
                return false;

        }

        /// <summary>
        /// Will return the progress (0-100%) of the current animation clip playing. If no animation clip is playing it will instead return
        /// the current progress of the clip playing in the Animator 
        /// </summary>
        /// <returns>Float indicating the current progress (0-100%) of the animation runner clip if playing else if not the Animator animation clip progress</returns>
        public float GetCurrentAnimationProgress() {

            float retVal = 0;

            //if animation clip is playing then return the clip progress
            if (this.animationClip != null)
                retVal = this.clipProgress;

            return retVal;

        }

        /// <summary>
        /// Will interrupt the current animation that is playing 
        /// </summary>
        /// <param name="QuickStartNextClipOverride">If true then clip override functionality will quick start, causing this end animation to end abruptly and not blend out</param>
        public void InterruptCurrentAnimation(bool QuickStartNextClipOverride = false) {




#if ABC_GC_2_Integration

        if (this.abcEntity.HasGC2CharacterComponent() == true)
            QuickStartNextClipOverride = false; 

#endif

            //If graph is playing then stop it
            if (this.playableGraph.IsValid() && this.playableGraph.IsPlaying()) {

                //Stop not destory 
                this.playableGraph.Stop();

                //Reset values
                this.animationClip = null;
                this.avatarMask = null;

                //If we just want to quick start next clip override (and not blend out of the current animation) then turn the global variable on to do this and end here 
                //Used for coming out of crosshair override animation as this has to be quite quick
                if (QuickStartNextClipOverride == true && this.AnimatorClipRunnerOverrides.Count > 0) {
                    this.blockRunnerOverrides = false;
                    this.currentAnimatorAnimationOverride = null;
                    quickStartNextClipOverride = true;
                    return;
                }

            }


            //Stop all coroutines 
            this.aniRunCoroutines.ForEach(c => StopCoroutine(c));
            this.aniRunCoroutines.Clear();

            this.aniStopCoroutines.ForEach(c => StopCoroutine(c));
            this.aniStopCoroutines.Clear();

            this.aniRunStopCoroutines.ForEach(c => StopCoroutine(c));
            this.aniRunStopCoroutines.Clear();



        }

        /// <summary>
        /// Will add Animator Clip Overrides to the Animation Runner
        /// </summary>
        /// <param name="ClipOverrides">List of Animator Clip Overrides</param>
        public void AddAnimatorClipOverrides(List<ABC_Controller.AnimatorClipRunnerOverride> ClipOverrides) {

            if (ClipOverrides.Count > 0)
                this.AnimatorClipRunnerOverrides = ClipOverrides;
        }

        /// <summary>
        /// Will clear Animator Clip Overrides from the Animation Runner
        /// </summary>
        public void ClearAnimatorClipOverrides() {

            //make sure no overrides are running
            if (this.AnimatorClipRunnerOverrides.Any(c => c.animationRunnerClip.AnimationClip == this.animationClip)) {
                this.animationClip = null;
                this.avatarMask = null;
                this.clipProgress = 0;
                this.blockRunnerOverrides = false;

                if (this.playableGraph.IsValid() && this.playableGraph.IsPlaying())
                    this.playableGraph.Stop();

            }

            //clear list
            this.AnimatorClipRunnerOverrides.Clear();
        }


        /// <summary>
        /// Will return a bool indicatating if the current animation runner has been frozen (or moving very slowly)
        /// </summary>
        /// <returns>True if animation runner is frozen/running very slow, else false for running at a normal speed</returns>
        public bool RunnerSpeedIsFrozen() {

            //If animation not running then can return false as nothing is frozen
            if (this.playableGraph.IsValid() == false || this.playableGraph.IsPlaying() == false)
                return false;


            //If Avatar Mask animation clip is running (AnimationLayerMixer being used)
            if (this.avatarMask != null) {

                //If mixer is valid and speed is less then 0.4 then return true as animation runner is currently frozen
                if (this.animationLayerMixer.IsValid() && this.animationLayerMixer.GetSpeed() <= 0.3f)
                    return true;
                else
                    return false; // else return false as all running at normal speed

            } else { // else check mixerPlayable 

                //If mixer is valid and speed is less then 0.4 then return true as animation runner is currently frozen
                if (this.mixerPlayable.IsValid() && this.mixerPlayable.GetSpeed() <= 0.3f)
                    return true;
                else
                    return false; // else return false as all running at normal speed

            }


        }

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will return the animation clip currently running on the Animator
        /// </summary>
        /// <returns>Current animation running on Animator</returns>
        /// <param name="CurrentRunningAnimation">The current animation clip playing in the animation runner. Will blend from this clip to the new animation clip instead of the animator clip if provided</param>
        /// <param name="CurrentRunningAvatarMask">The current avatar Mask playing in the animation runner. Will blend from this clip to the new animation clip instead of the animator clip if provided</param>
        private IEnumerator GetCurrentAnimatorAnimation(AnimationClip CurrentRunningAnimation = null, AvatarMask CurrentRunningAvatarMask = null) {

            if (this.playableGraph.IsValid() == false)
                yield break;

            bool SkipAniCheck = false;

            //If have animancer then don't check for animator
#if ABC_Animancer_Integration
            SkipAniCheck = true; 
#endif

            //If animator is not used at all then skip this functionality as we have no animation clips to find in Animator
            if (SkipAniCheck == false && (this.meAni == null || this.meAni.runtimeAnimatorController == null || this.meAni.runtimeAnimatorController.animationClips.Length == 0))
                yield break;


            //If Animator is currently in transition then wait for that to end before getting the animation playing on the Animator
            while (this.meAni != null && this.meAni.runtimeAnimatorController != null && this.meAni.runtimeAnimatorController.animationClips.Length > 0 && this.meAni.GetCurrentAnimatorClipInfo(0).Length == 0) {
                yield return null;
            }

            //get current base animation clip if one is running already (the Animator clip to blend from)
            AnimationClip baseClip = null;

            //Do we skip the time set sync
            bool skipTimeset = false;

            //If no clip was running then we will get the current animation running in the animator
            if (this.currentAnimatorAnimationOverride != null) {
                baseClip = this.currentAnimatorAnimationOverride;
            } else {

                //Get current clip
                if (this.meAni != null && this.meAni.runtimeAnimatorController != null && this.meAni.runtimeAnimatorController.animationClips.Length > 0) {
                    baseClip = this.meAni.GetCurrentAnimatorClipInfo(0)[0].clip;
                }


#if ABC_GC_2_Integration

                //If GC2 weapon state is used then grab the idle animation
                if (this.abcEntity.currentEquippedWeapon != null && this.abcEntity.currentEquippedWeapon.gc2EquipState != null){
                    baseClip = this.abcEntity.currentEquippedWeapon.gc2EquipState.StateController.animationClips[10]; //10 is always idle
                    skipTimeset = true;
                }
#endif

#if ABC_Animancer_Integration



                //If Animancer then we want to return back into the clip state it's currently in
                if (this.abcEntity.animancerComponent != null && this.abcEntity.animancerComponent.Playable.States.Current != null) {

                    if (this.abcEntity.animancerComponent.Playable.States.Current.GetType() == typeof(Animancer.ControllerState)) {

                    //Get the clip from the controller its running
                        Animancer.ControllerState aniControllerState = (Animancer.ControllerState)this.abcEntity.animancerComponent.Playable.States.Current;

                        baseClip = aniControllerState.GetCurrentAnimatorClipInfo(0)[0].clip;
                        skipTimeset = true;

                    } else {
                    //get clip from the current state
                        baseClip = this.abcEntity.animancerComponent.Playable.States.Current.Clip;
                        skipTimeset = true;
                    }
                     
                }
#endif



            }


            //If using current running animation then create playable with that
            if (CurrentRunningAnimation != null) {
                this.clipPlayableAnimator = AnimationClipPlayable.Create(playableGraph, CurrentRunningAnimation);
            } else { // else use base clip just found (from animator)
                this.clipPlayableAnimator = AnimationClipPlayable.Create(playableGraph, baseClip);
            }

            //Set the properties of the animation 
            this.clipPlayableAnimator.SetSpeed(this.meAni.GetCurrentAnimatorStateInfo(0).speed);

            if (CurrentRunningAnimation != null && CurrentRunningAvatarMask != null) { // if mask provided with current running animation then create approproiate playable with avatar mask

                // Create layer mixer playable
                this.clipPlayableAnimatorAvatarMask = AnimationLayerMixerPlayable.Create(playableGraph, 2);

                //connect base animator clip
                this.clipPlayableAnimatorAvatarMask.ConnectInput(0, AnimationClipPlayable.Create(playableGraph, baseClip), 0, 1);

                //connect the animation clip to play with the mask (on layer index 2 as layer index 1 is used temporarily for blending when updating zero layer) 
                this.clipPlayableAnimatorAvatarMask.SetLayerMaskFromAvatarMask(1, CurrentRunningAvatarMask);
                this.clipPlayableAnimatorAvatarMask.ConnectInput(1, AnimationClipPlayable.Create(playableGraph, CurrentRunningAnimation), 0, 1);
                this.clipPlayableAnimatorAvatarMask.SetLayerAdditive(1, false);



            } else {

                //pause current avatar clip playable, to mark it as not to be used
                if (this.playableGraph.IsValid() && this.clipPlayableAnimatorAvatarMask.IsValid())
                    this.clipPlayableAnimatorAvatarMask.Pause();

            }



            //Skip timeset?
            if (skipTimeset == false) {

                //Record the clip run time so we can make sure the animation starts at the right state/time/position
                float currentRunTime = this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime;

                //Record how far through the current animator state was (out of 100%)
                //Done by getting remainder of run time as a percentage (1.3 runtime = 30% through repeating animation)
                double runPercentage = (currentRunTime) % 1 * 100;
                runPercentage = System.Math.Round(runPercentage, 1);

                //Start animation where it was on original animator (has to be called twice to fix root motion issue)
                //Worked out by getting what runpercentage is of the duration i.e 30% of a 1 second animation is 0.3 time to set into animator 
                this.clipPlayableAnimator.SetTime(System.Math.Round((baseClip.length / 100) * runPercentage, 3));
                this.clipPlayableAnimator.SetTime(System.Math.Round((baseClip.length / 100) * runPercentage, 3));


                //Start animation where it was on original animator for avatar mask setup also
                if (this.clipPlayableAnimatorAvatarMask.IsValid()) {
                    this.clipPlayableAnimatorAvatarMask.SetTime(System.Math.Round((baseClip.length / 100) * runPercentage, 3));
                    this.clipPlayableAnimatorAvatarMask.SetTime(System.Math.Round((baseClip.length / 100) * runPercentage, 3));
                }

            }



            //Animator is already playing a clip so we don't need to grab any Animator overrides so end here 
            if (CurrentRunningAnimation != null || this.currentAnimatorAnimationOverride != null || this.blockRunnerOverrides == true)
                yield break;


            //See if we can find an override clip which matches the name of the current animation playing in animator
            ABC_Controller.AnimatorClipRunnerOverride clipOverride = this.GetClipOverrideMatchingCurrentAnimatorAnimation();

            if (clipOverride != null && clipOverride.animationRunnerClip.AnimationClip != this.animationClip) {

                this.clipPlayableAnimator = AnimationClipPlayable.Create(playableGraph, clipOverride.animationRunnerClip.AnimationClip);

                if (clipOverride.animationRunnerMask != null && clipOverride.animationRunnerMask.AvatarMask != null) {

                    // Create layer mixer playable
                    this.clipPlayableAnimatorAvatarMask = AnimationLayerMixerPlayable.Create(playableGraph, 2);

                    //connect base animator clip
                    this.clipPlayableAnimatorAvatarMask.ConnectInput(0, AnimationClipPlayable.Create(playableGraph, baseClip), 0, 1);

                    //connect the animation clip to play with the mask (on layer index 2 as layer index 1 is used temporarily for blending when updating zero layer) 
                    this.clipPlayableAnimatorAvatarMask.ConnectInput(1, AnimationClipPlayable.Create(playableGraph, this.clipPlayableAnimator.GetAnimationClip()), 0, 1);
                    this.clipPlayableAnimatorAvatarMask.SetLayerMaskFromAvatarMask(1, clipOverride.animationRunnerMask.AvatarMask);
                    this.clipPlayableAnimatorAvatarMask.SetLayerAdditive(1, false);


                    //Set the properties of the animation 
                    this.clipPlayableAnimatorAvatarMask.SetSpeed(clipOverride.animationRunnerClipSpeed);
                    //Start animation where it was on original animator (has to be called twice to fix root motion issue)
                    this.clipPlayableAnimatorAvatarMask.SetTime((clipOverride.animationRunnerClip.AnimationClip.length * this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime) - Time.deltaTime);
                    this.clipPlayableAnimatorAvatarMask.SetTime((clipOverride.animationRunnerClip.AnimationClip.length * this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime) - Time.deltaTime);


                } else {

                    //pause current avatar clip playable, to mark it as not to be used
                    if (this.playableGraph.IsValid() && this.clipPlayableAnimatorAvatarMask.IsValid())
                        this.clipPlayableAnimatorAvatarMask.Pause();

                }


                //Set the properties of the animation 
                this.clipPlayableAnimator.SetSpeed(clipOverride.animationRunnerClipSpeed);
                //Start animation where it was on original animator (has to be called twice to fix root motion issue)
                this.clipPlayableAnimator.SetTime((clipOverride.animationRunnerClip.AnimationClip.length * this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime) - Time.deltaTime);
                this.clipPlayableAnimator.SetTime((clipOverride.animationRunnerClip.AnimationClip.length * this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime) - Time.deltaTime);

            }



        }


        /// <summary>
        /// Method will ensure when using an avatar mask/layer mixer that the animation on the zero layer (being overriden partially by the avatar mask) 
        /// will always match the animation currently playing in the Animator.
        /// </summary>
        private IEnumerator UpdateZeroLayerMixerWithCurrentAnimatorAnimation() {

            //Infinite loop
            while (true) {

                //wait for end of frame
                yield return new WaitForFixedUpdate();

                //If paused then continue
                if (this.pauseZeroLayerCoroutine == true)
                    continue;

                //If playable graph isn't in use or the animation runner isn't using an avatar mask or the entities Animator is in transition to another animation then continue
                if (this.playableGraph.IsValid() == false || this.playableGraph.IsPlaying() == false || this.avatarMask == null || this.meAni.GetCurrentAnimatorClipInfo(0).Length == 0 || this.animationLayerMixer.IsValid() == false)
                    continue;

                //Get current override clip
                ABC_Controller.AnimatorClipRunnerOverride currentOverrideClip = this.GetClipOverrideMatchingCurrentAnimatorAnimation();


                //If the current animation playing on the Animator (or override animation) doesn't match the animation running on this components zero layer then update the animation to match the Animator
                //unless override has an avatar mask 
                if (this.currentAnimatorAnimationOverride == null && currentOverrideClip == null && this.meAni.GetCurrentAnimatorClipInfo(0)[0].clip != this.clipPlayableAnimator.GetAnimationClip() || this.currentAnimatorAnimationOverride != null && this.currentAnimatorAnimationOverride != this.clipPlayableAnimator.GetAnimationClip()
                    || currentOverrideClip != null && (currentOverrideClip.animationRunnerMask.AvatarMask != null || currentOverrideClip.animationRunnerMask.AvatarMask == null && currentOverrideClip.animationRunnerClip.AnimationClip != this.clipPlayableAnimator.GetAnimationClip())) {


                    ////Create temp clip playable of current zero layer animation so we can blend from this to the new animation
                    AnimationClipPlayable clipPlayableTempBlend = AnimationClipPlayable.Create(playableGraph, this.clipPlayableAnimator.GetAnimationClip());

                    //Populate the global variable tracking the animation currently running in the Animator
                    yield return StartCoroutine(this.GetCurrentAnimatorAnimation());

                    //If we about to blend to the same animation then end here
                    if (this.pauseZeroLayerCoroutine == true || this.clipPlayableAnimator.GetAnimationClip() == clipPlayableTempBlend.GetAnimationClip() || this.animationLayerMixer.IsValid() == false)
                        continue;


                    //Add existing animation ready to blend from
                    this.animationLayerMixer.ConnectInput(1, clipPlayableTempBlend, 0, 1);
                    this.animationLayerMixer.SetInputWeight(0, 0);

                    //Update speed/time for temp animation to where we was
                    clipPlayableTempBlend.SetSpeed(this.animationLayerMixer.GetInput(0).GetSpeed());
                    clipPlayableTempBlend.SetTime(this.animationLayerMixer.GetInput(0).GetTime());
                    clipPlayableTempBlend.SetTime(this.animationLayerMixer.GetInput(0).GetTime());

                    //update the new zero layer animation to blend too               
                    this.animationLayerMixer.DisconnectInput(0);

                    if (this.animationLayerMixer.IsValid())
                        this.animationLayerMixer.ConnectInput(0, this.clipPlayableAnimator, 0, 0);

                    //Define weight ready to blend
                    float weight = 0f;

                    //Blend weight to new zero layer animation
                    while (weight < 0.9f) {

                        //If mixerPlayable not created yet then end here
                        if (this.animationLayerMixer.IsValid() == false)
                            break;

                        weight += 0.15f;
                        this.animationLayerMixer.SetInputWeight(1, 0.9f - weight);
                        this.animationLayerMixer.SetInputWeight(0, 0.1f + weight);



                        yield return new WaitForSeconds(0.008f);
                    }



                    //Disconnect the temporarily blend
                    if (clipPlayableTempBlend.IsValid() && this.playableGraph.IsValid()) {
                        //fianlise weight
                        this.playableGraph.Disconnect(animationLayerMixer, 1);
                    }





                }


            }


        }




        /// <summary>
        /// This method is the code to run the animation - it will create the clip and playable graph and then start the animation
        /// </summary>
        /// <param name="Animation">Animation clip to play</param>
        /// <param name="Delay">Delay before animation clip runs</param>
        /// <param name="Speed">The speed of the animation clip</param>
        /// <param name="CurrentRunningAnimation">The current animation clip playing in the animation runner. Will blend from this clip to the new animation clip instead of the animator clip if provided</param>
        /// <param name="CurrentRunningAvatarMask">The current avatar Mask playing in the animation runner. Will blend from this clip to the new animation clip instead of the animator clip if provided</param>
        /// <param name="SkipBlending">If true then blending will not occur and animation will play instantly</param>
        private IEnumerator RunAnimation(AnimationClip Animation, float Delay, float Speed = 1f, AnimationClip CurrentRunningAnimation = null, AvatarMask CurrentAvatarMask = null, bool SkipBlending = false) {

            //If this run animation method was called very close to another one, then wait a small amount to stop any glitches
            if (Time.time - lastRunTime < 0.1f)
                yield return new WaitForSeconds(0.07f);

            //record when this was last called
            this.lastRunTime = Time.time;

            //If animation clip has not been provided then end here
            if (Animation == null)
                yield return null;

            //If a delay has been provided then wait 
            if (Delay > 0)
                yield return new WaitForSeconds(Delay);

            //Reset time on mixer and animation layer
            if (this.mixerPlayable.IsValid())
                this.mixerPlayable.SetTime(0);

            if (this.animationLayerMixer.IsValid())
                this.animationLayerMixer.SetTime(0);

            //Clean up old graphs that were not cleaned up due to interruptions
            foreach (PlayableGraph oldGraph in this.oldPlayableGraphs.ToList()) {

                if (oldGraph.IsValid()) {
                    oldGraph.Stop();
                    oldGraph.Destroy();
                }

                this.oldPlayableGraphs.Remove(oldGraph);
            }


            //store old playergraph to stop when new one starts
            PlayableGraph oldPlayableGraph = this.playableGraph;
            this.oldPlayableGraphs.Add(oldPlayableGraph);

            //Stop zero layer update
            this.pauseZeroLayerCoroutine = true;

            //Create the playable graph and set time update mode
            this.playableGraph = PlayableGraph.Create();
            this.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);


            // create and wrap the animation clip in a playable
            clipPlayable = AnimationClipPlayable.Create(playableGraph, Animation);

            //Set the properties of the animation (speed, footOK)
            clipPlayable.SetSpeed(Speed);
            clipPlayable.SetApplyFootIK(true);


            //Create the animator animation playable output 
            var playableOutput = AnimationPlayableOutput.Create(this.playableGraph, "ABC_Animation", this.meAni);


            //Populate the global variable tracking the animation currently running in the Animator/Animation Runner
            yield return StartCoroutine(this.GetCurrentAnimatorAnimation(CurrentRunningAnimation, CurrentAvatarMask));


            //Create the high level mixer which will blend from one clip to another
            this.mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);


            //Create upper level animation script playable for IK customisation and connect our mixerPlayable to it 
            this.scriptPlayable = AnimationScriptPlayable.Create(this.playableGraph, this.abcEntity.ikController.GetAnimationRunnerIKJob(), 1);
            this.scriptPlayable.ConnectInput(0, mixerPlayable, 0, 1.0f);

            //Set output
            playableOutput.SetSourcePlayable(this.scriptPlayable);

            //If avatar mask is on animator to blend from then setup the animation layer mixer with both clips attached and avatar mask applied
            if (this.clipPlayableAnimatorAvatarMask.IsValid() && this.clipPlayableAnimatorAvatarMask.GetPlayState() != PlayState.Paused) {

                //Connect layer mixer to our mixer playable
                this.playableGraph.Connect(this.clipPlayableAnimatorAvatarMask, 0, mixerPlayable, 0);
                this.mixerPlayable.SetInputWeight(0, 0);

            } else { // else just connect clip to the original mixer

                //Connect animator to animation clip setting weight for a smooth blend
                this.playableGraph.Connect(this.clipPlayableAnimator, 0, mixerPlayable, 0);
                this.mixerPlayable.SetInputWeight(0, 0);


            }



            if (this.debugMode == true) {
                Debug.Log("From: " + clipPlayableAnimator.GetAnimationClip().name);
                Debug.Log("To: " + clipPlayable.GetAnimationClip().name);
            }


            //If avatar mask then setup the animation layer mixer with both clips attached and avatar mask applied
            if (avatarMask != null) {

                // Create layer mixer playable
                this.animationLayerMixer = AnimationLayerMixerPlayable.Create(playableGraph, 3);

                //Connect layer mixer to our mixer playable
                this.playableGraph.Connect(this.animationLayerMixer, 0, mixerPlayable, 1);

                //connect base animator clip
                this.animationLayerMixer.ConnectInput(0, AnimationClipPlayable.Create(playableGraph, this.clipPlayableAnimator.GetAnimationClip()), 0, 1);

                //connect the animation clip to play with the mask (on layer index 2 as layer index 1 is used temporarily for blending when updating zero layer) 
                this.animationLayerMixer.SetLayerMaskFromAvatarMask(2, avatarMask);
                this.animationLayerMixer.ConnectInput(2, clipPlayable, 0, 1);
                this.animationLayerMixer.SetLayerAdditive(2, false);

                this.mixerPlayable.SetInputWeight(1, 1);

                //restart zero layer update
                this.pauseZeroLayerCoroutine = false;


            } else { // else just connect clip to the original mixer

                this.playableGraph.Connect(clipPlayable, 0, mixerPlayable, 1);
                this.mixerPlayable.SetInputWeight(1, 1);


            }


            // Plays the Graph/Animation.
            this.playableGraph.Play();


            if (SkipBlending == false && this.AnimationIsAClipOverride(clipPlayable.GetAnimationClip())) {
                //Define weight ready to blend
                float weight = 0f;

                //Blend weights back to Animator
                while (weight < 0.9f) {

                    //If mixerPlayable not created yet then end here
                    if (this.mixerPlayable.IsValid() == false)
                        break;

                    weight += 0.05f;
                    this.mixerPlayable.SetInputWeight(0, 1.0f - weight);
                    this.mixerPlayable.SetInputWeight(1, 0 + weight);


                    yield return new WaitForSeconds(0.008f);
                }

                //Ensure blend has fully reached correct weight
                this.mixerPlayable.SetInputWeight(0, 0);
                this.mixerPlayable.SetInputWeight(1, 1);
            }





            if (oldPlayableGraph.IsValid() && oldPlayableGraph.IsPlaying()) {
                oldPlayableGraph.Destroy();
                this.oldPlayableGraphs.Remove(oldPlayableGraph);
            }

        }


        /// <summary>
        /// Will perform the code to stop the animation that is currently playing 
        /// </summary>
        /// <param name="Delay">Delay before animation clip runs</param>
        /// <param name="SkipBlending">Will skip blending and stop animation instantly</param>
        private IEnumerator StopAnimation(float Delay = 0f, bool SkipBlending = false) {

            //Wait for delay - minus how long it takes to blend back to animator
            yield return new WaitForSeconds(Delay - 0.5f);

            //remove runner override blocks so we can move back to any idle overrides
            this.blockRunnerOverrides = false;
            this.pauseZeroLayerCoroutine = true;

            //Populate the global variable tracking the animation currently running in the Animator
            yield return StartCoroutine(this.GetCurrentAnimatorAnimation());


            //Store end clip
            AnimationClip endClip = null;

            if (this.playableGraph.IsValid())
                endClip = this.clipPlayableAnimator.GetAnimationClip();

            //If end clip found then blend to it 
            if (endClip != null) {

                //Add most recent animator animation incase it changed since the runner started
                if (this.playableGraph.IsValid() && this.gameObject.activeInHierarchy && this.mixerPlayable.IsValid()) {


                    //If avatar mask is on animator to blend from then setup the animation layer mixer with both clips attached and avatar mask applied
                    if (this.clipPlayableAnimatorAvatarMask.IsValid() && this.clipPlayableAnimatorAvatarMask.GetPlayState() != PlayState.Paused) {

                        //Connect layer mixer to our mixer playable
                        this.playableGraph.Disconnect(mixerPlayable, 0);

                        if (this.playableGraph.IsValid() && this.mixerPlayable.IsValid())
                            this.playableGraph.Connect(this.clipPlayableAnimatorAvatarMask, 0, mixerPlayable, 0);


                    } else { // else just connect clip to the original mixer

                        this.playableGraph.Disconnect(mixerPlayable, 0);

                        if (this.playableGraph.IsValid() && this.mixerPlayable.IsValid())
                            this.playableGraph.Connect(this.clipPlayableAnimator, 0, mixerPlayable, 0);

                    }

                }

                if (SkipBlending == false) {

                    //Define weight ready to blend
                    float weight = 0f;

                    //Blend weights back to Animator
                    while (weight < 0.9f) {

                        //If mixerPlayable not created yet then end here
                        if (this.mixerPlayable.IsValid() == false)
                            break;

                        weight += 0.05f;
                        this.mixerPlayable.SetInputWeight(0, 0 + weight);
                        this.mixerPlayable.SetInputWeight(1, 1.0f - weight);

                        yield return new WaitForSeconds(0.008f);
                    }

                }

                if (this.mixerPlayable.IsValid() == true) {
                    this.mixerPlayable.SetInputWeight(1, 0);
                    this.mixerPlayable.SetInputWeight(0, 1);
                }
            }

            this.pauseZeroLayerCoroutine = false;


            //If clip overriding then then don't stop the graph as we just merged into the new clip and don't want to go back to animator
            if (this.meAni != null && this.meAni.runtimeAnimatorController != null && this.meAni.runtimeAnimatorController.animationClips.Length > 0 && this.AnimatorClipRunnerOverrides.Any(c => c.animationRunnerClip.AnimationClip == endClip)) {

                //Get clip override details 
                ABC_Controller.AnimatorClipRunnerOverride clipOverride = this.AnimatorClipRunnerOverrides.Where(c => c.animationRunnerClip.AnimationClip == endClip).First();

                this.animationClip = endClip;
                this.avatarMask = clipOverride.animationRunnerMask != null ? clipOverride.animationRunnerMask.AvatarMask : null;
                this.clipProgress = 0f;
                this.blockRunnerOverrides = false;
                this.currentAnimatorAnimationOverride = null;

            } else {

                //Stop the graph and go back to animator
                if (this.playableGraph.IsValid() && this.playableGraph.IsPlaying())
                    this.playableGraph.Stop();

                this.animationClip = null;
                this.avatarMask = null;
                this.clipProgress = 0f;
                this.blockRunnerOverrides = false;
                this.currentAnimatorAnimationOverride = null;
            }

            //Clean up old graphs
            foreach (PlayableGraph oldGraph in this.oldPlayableGraphs.ToList()) {

                if (oldGraph.IsValid()) {
                    oldGraph.Stop();
                    oldGraph.Destroy();
                }

                this.oldPlayableGraphs.Remove(oldGraph);
            }


        }

        /// <summary>
        /// Will start and stop an animation using the clip provided after the delay and duration set. 
        /// </summary>
        /// <param name="Animation">Animation clip to play</param>
        /// <param name="Delay">Delay before animation clip runs</param>
        /// <param name="Speed">The speed of the animation clip</param>
        /// <param name="Duration">The duration before the animation ends, if 0 is provided then the duration will last for the whole animation clip</param>
        /// <param name="AvatarMask">(Optional) If provided then animation clip will run using the avatar mask</param>
        private IEnumerator RunAndStopAnimation(AnimationClip Animation, float Delay, float Speed = 1f, float Duration = 0, AvatarMask AvatarMask = null, bool SkipBlending = false) {

            //Track what time this method was called
            //Stops overlapping i.e if another part of the system activated the same call
            //this function would not continue after duration
            float functionRequestTime = Time.time;

            //If we are starting and stopping the animation then disable IK
            if (this.abcEntity != null)
                StartCoroutine(this.abcEntity.ToggleIK(functionRequestTime, false));


            //Start the animation
            this.StartAnimation(Animation, Delay, Speed, AvatarMask, true, false, SkipBlending);

            //Wait for duration (if 0 duration is set then use the animations full duration)
            if (Duration == 0)
                yield return new WaitForSeconds((float)(Animation.length / Speed) + Delay);
            else
                yield return new WaitForSeconds(Duration + Delay);

            //Stop the animation
            this.EndAnimation(Animation);

            //Enable the IK 
            if (this.abcEntity != null) {
                StartCoroutine(this.abcEntity.ToggleIK(functionRequestTime, true, 0.4f));
            }

        }


        /// <summary>
        /// Determines if the animation clip provided is an override clip
        /// </summary>
        /// <param name="Animation">The clip to determine if it's an override or not</param>
        /// <returns>True if the clip is an override clip, else false</returns>
        private bool AnimationIsAClipOverride(AnimationClip Animation) {

            foreach (ABC_Controller.AnimatorClipRunnerOverride clipOverride in this.AnimatorClipRunnerOverrides) {

                if (clipOverride.animationRunnerClip.AnimationClip == Animation)
                    return true;

            }


            return false;

        }

        /// <summary>
        /// Will return any override clips which have been set to override the current animators animation
        /// </summary>
        /// <returns>AnimatorClipRunnerOverride Object which should override the current animators animation</returns>
        private ABC_Controller.AnimatorClipRunnerOverride GetClipOverrideMatchingCurrentAnimatorAnimation() {

            //Return Value
            ABC_Controller.AnimatorClipRunnerOverride retVal = null;

            //We have an override so don't need to get any clip overrides
            if (this.currentAnimatorAnimationOverride != null || this.blockRunnerOverrides == true || this.meAni == null || this.meAni.runtimeAnimatorController == null || this.meAni.runtimeAnimatorController.animationClips.Length == 0)
                return retVal;

            //Get Current Clip Running
            AnimationClip currentAnimatorClipInfo = this.meAni.GetCurrentAnimatorClipInfo(0)[0].clip;

            foreach (ABC_Controller.AnimatorClipRunnerOverride clipOverride in this.AnimatorClipRunnerOverrides) {

                if (clipOverride.exactNameRequired == true) {

                    //If the exact name of the current animation clip playing matches the list of animation names to override
                    if (clipOverride.animatorClipNamesToOverride.Any(c => c == currentAnimatorClipInfo.name)) {
                        retVal = clipOverride;
                        break;
                    }

                } else {
                    //If any part of the name of the current animation clip playing matches the list of animation names to override
                    if (clipOverride.animatorClipNamesToOverride.Any(c => currentAnimatorClipInfo.name.Contains(c))) {
                        retVal = clipOverride;
                        break;
                    }
                }


            }


            return retVal;

        }


        /// <summary>
        /// Will record the progress of the current animation clip playing (0 - 100%)
        /// </summary>
        private void RecordAnimationClipProgress() {

            //Record current progress of animation clip running
            if (this.animationLayerMixer.IsValid() && this.avatarMask != null) {

                float clipDuration = ABC_Utilities.ModifyTimeByPercentage((float)this.clipPlayable.GetSpeed(), this.clipPlayable.GetAnimationClip().length) / 100;

                this.clipProgress = ((float)animationLayerMixer.GetTime() / clipDuration * 100) % 100;

            } else if (this.mixerPlayable.IsValid() && this.avatarMask == null) {

                float clipDuration = ABC_Utilities.ModifyTimeByPercentage((float)this.clipPlayable.GetSpeed(), this.clipPlayable.GetAnimationClip().length) / 100;

                this.clipProgress = ((float)mixerPlayable.GetTime() / clipDuration * 100) % 100;
            }

        }

        /// <summary>
        /// Will ensure that the animation runner speed always matches the speed of the Animator. 
        /// </summary>
        private void SyncSpeedWithAnimator() {

            //If animation not running then can return false as nothing is frozen
            if (this.playableGraph.IsValid() == false || this.playableGraph.IsPlaying() == false)
                return;


            //Update layer mixer to match Animator speed

            if (this.animationLayerMixer.IsValid())
                this.animationLayerMixer.SetSpeed(this.meAni.speed);

            if (this.mixerPlayable.IsValid())
                this.mixerPlayable.SetSpeed(this.meAni.speed);

        }


        /// <summary>
        /// Will handle Animation Clips which will run instead of any animations currently playing on the Animator which has a name that contains
        /// the text defined. For example play an Idle animation clip instead of any animation in the Animator that contains the name 'Idle'
        /// </summary>
        private void AnimatorClipOverrideHandler() {

            //if Animator in state transition or runner override block has been enabled 
            // or we pressing keys and the animator has just changed (allows swapping from right to left movement without going back to Idle), however if no keys pressing it can go straight back to idle
            if (this.meAni.GetCurrentAnimatorClipInfo(0).Length == 0 || this.blockRunnerOverrides == true || quickStartNextClipOverride == false && this.abcEntity.inputManager.AnyKey() == true && this.meAni.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.08f)
                return;



            //Find an override clip which matches the name of the current animation playing in animator
            ABC_Controller.AnimatorClipRunnerOverride clipOverride = this.GetClipOverrideMatchingCurrentAnimatorAnimation();


            if (clipOverride != null) {

                //If not currently running any animation clips (so don't interrupt normal animation runner clips running)
                // or we want to run a new overriding clip and the current animation playing is another override clip (otherwise its a normal clip playing we don't want to override)
                if (quickStartNextClipOverride == true || this.animationClip == null || (this.animationClip != clipOverride.animationRunnerClip.AnimationClip || this.animationClip == clipOverride.animationRunnerClip.AnimationClip && this.avatarMask != clipOverride.animationRunnerMask.AvatarMask) && this.AnimatorClipRunnerOverrides.Any(c => c.animationRunnerClip.AnimationClip == this.animationClip)) {
                    this.StartAnimation(clipOverride.animationRunnerClip.AnimationClip, clipOverride.animationRunnerClipDelay, clipOverride.animationRunnerClipSpeed, clipOverride.animationRunnerMask.AvatarMask, true, false, this.quickStartNextClipOverride);
                    this.quickStartNextClipOverride = false;
                }

            } else {

                //make sure no overrides are running
                if (this.AnimatorClipRunnerOverrides.Any(c => c.animationRunnerClip.AnimationClip == this.animationClip)) {
                    this.animationClip = null;
                    this.avatarMask = null;
                    this.clipProgress = 0;
                    this.blockRunnerOverrides = false;
                    this.currentAnimatorAnimationOverride = null;

                    if (this.playableGraph.IsValid() && this.playableGraph.IsPlaying())
                        this.playableGraph.Stop();

                }
            }
        }


        #endregion

        // ********************** Game ******************

        #region Game

        private void Awake() {


            this.abcEntity = ABC_Utilities.GetStaticABCEntity(gameObject);

            //If animator component doesn't exist on the object add it and track
            if (this.meAni == null) {
                this.meAni = gameObject.GetComponent<Animator>();

                if (this.meAni == null)
                    Debug.LogError("Animator not found on object. Please add one to the object to play animation clips using ABC's Animation Runner");
            }



            //Stop all coroutines 
            this.aniRunCoroutines.ForEach(c => StopCoroutine(c));
            this.aniRunCoroutines.Clear();

            this.aniStopCoroutines.ForEach(c => StopCoroutine(c));
            this.aniStopCoroutines.Clear();

            this.aniRunStopCoroutines.ForEach(c => StopCoroutine(c));
            this.aniRunStopCoroutines.Clear();


            //Track the zero layer coroutine
            StartCoroutine(this.UpdateZeroLayerMixerWithCurrentAnimatorAnimation());

        }


        void Update() {

            //run Animator Clip override Handler
            this.AnimatorClipOverrideHandler();

            //make sure speed is always in sync with Animator
            this.SyncSpeedWithAnimator();

            //Record how long current clip has been playing
            this.RecordAnimationClipProgress();


            //For debugging will play the animation using the inspector 
            if (this.Play == true) {
                this.PlayAnimation(this.animationClip);
                this.Play = false;
            }


            ////Show graph
            //if (this.debugMode == true) {
            //    GraphVisualizerClient.Show(playableGraph);
            //}

        }

        private void LateUpdate() {

            if (this.scriptPlayable.IsValid() == false)
                return;
        }


#if ABC_GC_2_Integration
    void OnAnimatorMove() {
        
        //Fix for GC 2 root motion issues with animation runner
        if (this.meAni != null && this.animationClip != null) {
                if (this.abcEntity.HasGC2CharacterComponent() && this.clipProgress < 70) {

                    Vector3 animatorVelocity = new Vector3(0, this.meAni.velocity.y, 0);

                    //If animation is moving in same direction as player then record the x velocity
                    if (transform.forward.x < 0 && this.meAni.velocity.x < 0 || transform.forward.x > 0 && this.meAni.velocity.x > 0)
                        animatorVelocity.x = this.meAni.velocity.x;

                    //If animation is moving in same direction as player then record the z velocity
                    if (transform.forward.z < 0 && this.meAni.velocity.z < 0 || transform.forward.z > 0 && this.meAni.velocity.z > 0)
                        animatorVelocity.z = this.meAni.velocity.z;

                    //Simulate animation movement if facing right direction
                    if (Vector3.Dot(transform.forward, animatorVelocity.normalized) > 0.7)
                        this.abcEntity.MoveGC2ToDirection(animatorVelocity, Space.World, 0);


                    //Keep child object in sync
                    this.meAni.transform.localPosition = Vector3.zero;
                    this.meAni.transform.localRotation = Quaternion.identity;


                } else if (this.applyRootMotion == true) {

                    this.meAni.ApplyBuiltinRootMotion();
                }
        }
                   
    }
#endif


        void OnDisable() {

            //Destroy the playable graph 
            if (this.playableGraph.IsValid())
                this.playableGraph.Destroy();
        }

        #endregion

    }

}