using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    [CreateAssetMenu(fileName = "NGCharacterConfigBase", menuName = "Threepeat/Character Config - Base")]
    public class NGCharacterBaseConfig : ScriptableObject
    {
        public string description = "";

        [Serializable]
        public class LocomotionModeSettings
        {
            public float speed = 5f;
            public float moveBias = 18;
            public float dirBias = 15;
            public float angularWarpRate = 60;
            public float angularWarpRateMagnitudeThreshold = 1f;
            
            public bool doLongitudinalErrorWarpWhenNoStrider = false;
            public Vector2 longitudinalErrorWarpRange = new Vector2(0.5f, 1.5f);
            public float playbackSpeedMultiplierIfNoLongitudeWarping = 1.0f;

            public LocomotionModeSettings(
                    float pSpeed, 
                    float pMoveBias, 
                    float pDirBias, 
                    float pAngularWarpRate, 
                    bool doLongErrorWarpWhenNoStrider = false, 
                    float longErrorMin = 0.5f, 
                    float longErrorMax = 1.5f, 
                    float pAngularWarpRateMagnitudeThreshold = 1.0f, 
                    float pPlaybackSpeedMultiplierIfNoLongitudeWarping = 1.0f)
            {
                this.speed = pSpeed;
                this.moveBias = pMoveBias;
                this.dirBias = pDirBias;
                this.angularWarpRate = pAngularWarpRate;
                this.doLongitudinalErrorWarpWhenNoStrider = doLongErrorWarpWhenNoStrider;
                this.longitudinalErrorWarpRange.x = longErrorMin;
                this.longitudinalErrorWarpRange.y = longErrorMax;
                this.angularWarpRateMagnitudeThreshold = pAngularWarpRateMagnitudeThreshold;
                this.playbackSpeedMultiplierIfNoLongitudeWarping = pPlaybackSpeedMultiplierIfNoLongitudeWarping;
            }
        }

        [Header("General Behavior")]
        public bool faceAwayFromCameraOnIdleInNonStrafeModes = false;

        [Header("Speeds and Biases")]
        public LocomotionModeSettings walkSettings = new LocomotionModeSettings(2.5f, 18, 15, 30);
        public LocomotionModeSettings runSettings = new LocomotionModeSettings(5f, 18, 15, 30);
        public LocomotionModeSettings sprintSettings = new LocomotionModeSettings(10f, 18, 15, 90, true);
        public LocomotionModeSettings crouchSettings = new LocomotionModeSettings(3f, 18, 15, 30);
        public LocomotionModeSettings crouchStrafeSettings = new LocomotionModeSettings(4.5f, 18, 15, 30);
        public LocomotionModeSettings strafeSettings = new LocomotionModeSettings(4.5f, 18, 15, 30);

        [Tooltip("(Only applies when Strider is integrated) Minimum speed to engage Strider")]
        public float minSpeedForStrider = 1.8f;
        
        [HideInInspector] public bool doSprintRamp = false;
        [HideInInspector] public float sprintRampSpeedRate = 0.5f;

        [Header("Jump and Fall")]
        public NGJumpSettings configJump;

        public bool jumpImpulseOnKeypress = false;

        [Tooltip("After character becomes ungrounded (starts falling) with the exception of when they jump, how long before checking if they're falling.")]
        public float fallDetectionTimeDelay = 0.1f;
        [Tooltip("This short-circuits fallDetectionTimeDelay and will cause immediate fall detection and event-firing if player's height above ground is above this value:  the minimum height above ground to engage falling action when character becomes ungrounded.")]
        public float minHeightToStartFalling = 0.7f;

    }
}