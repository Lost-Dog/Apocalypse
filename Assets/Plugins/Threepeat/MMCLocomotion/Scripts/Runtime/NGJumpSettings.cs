using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGJumpSettings", menuName = "Threepeat/Jump Settings")]
    public class NGJumpSettings : ScriptableObject
    {

        [Tooltip("height above ground to achieve on a regular jump event")]
        public float jumpHeight = 1.2f;
        [Tooltip("height above ground to achieve on a big jump event")]
        public float jumpBigHeight = 2.0f;
        [Tooltip("MxM Event to fire for jumps")]
        public MxMEventDefinition jump_EventDef;
        [Tooltip("MxM Event to fire whenever the player lands (impacts the ground after having not been grounded.  this includes both post-jump conditions and any time the player walks, runs or falls off a ledge.)")]
        public MxMEventDefinition landing_EventDef;
        [Tooltip("MxM Event to fire on hard landings (as defined by Landing_HeavySpeedThreshold) with low forward velocity or when ground path ahead of character is obstructed.")]
        public MxMEventDefinition landingHeavy_EventDef;
        [Tooltip("MxM Event to fire on hard landings (as defined by Landing_HeavySpeedThreshold) with high forward velocity or when ground path ahead of character is clear.")]
        public MxMEventDefinition landingHeavyForwardClear_EventDef;
        [Tooltip("(leave this set to 22): Favour tag to use to achieve correct launch animation for big jumps.")]
        public int jumpBig_favourTag = 22;
        [Tooltip("Distance ahead of player upon hard landing to check for obstruction to decide which event to fire (e.g. if there's room to do a landing forward roll, do that, otherwise do a more stationary or space-constrained landing animation.)")]
        public float landing_distanceToCheckForClearPathAhead = 4f;
        [Tooltip("Ground Impact Speed to engage heavy landing logic.  This is a negative number, since it represents a negative speed in the Y direction (fall speed at impact with ground).")]
        public float landing_HeavySpeedThreshold = -6.5f;
        [Tooltip("For normal landings, if speed is below this value, the player will just resume their idle or walking behavior upon landing instead of engaging the landing event.  This makes the landing system more responsive/realistic for small jumps or falls.")]
        public float landing_minForwardSpeedForLandingEvent = 3f;
        [Tooltip("Duration of time gravity re-enabling should be delayed after the landing event is complete.")]
        public float delayGravityTime = 0f;

        [Tooltip("Max horizontal speed character can generate from a standing jump.")]
        public float standingJumpMaxHorizontalJumpSpeed = 3f;
        [Tooltip("Max speed (squared for compute efficiency) to be considered a standing jump (this concept is also applicable to walking speeds)")]
        public float standingJumpMaxCharacterSpeedSquared = 2.25f;


    }

}