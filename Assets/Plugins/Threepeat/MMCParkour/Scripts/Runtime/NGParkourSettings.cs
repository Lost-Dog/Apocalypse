using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGParkourSettings", menuName = "Threepeat/Parkour Settings")]
    public class NGParkourSettings : ScriptableObject
    {
        public string description = "";

        [Tooltip("This list is ordered by priority.  The first/top entry will take preference over the next, and so on.")]
        public List<ParkourCapability> capabilities = new List<ParkourCapability>();

        [System.Serializable]
        public class ParkourCapability
        {
            [Tooltip("The type of athletic action (e.g. vault, vault on, fence clear, mantle). Note: the list provided has several more options for event type that are not implemented in this version of MMLocoParkour.")]
            public NGMxMEventDef.NGAnimationEventType eventType = NGMxMEventDef.NGAnimationEventType.ByStringName;

            [Tooltip("The unique string name of the behavior (if eventType == ByStringName).  e.g. vault is parkour_vaultlow")]
            public string eventTypeName;

            [Tooltip("The MxM event to be executed when engaging this action when character is standing or moving slowly.")]
            public NGMxMEventDef standingEvent;
            [Tooltip("The MxM event to be executed when engaging this action when character is running.")]
            public NGMxMEventDef runningEvent;

            [Tooltip("Whether this capability is available to the character (enabled) by default")]
            public bool activeByDefault = true;

            public string GetCapabilityName()
            {
                if (eventType == NGMxMEventDef.NGAnimationEventType.ByStringName)
                {
                    return eventTypeName;
                }

                return eventType.ToString();
            }

        }

        [Header("Parkour Detection Settings")]
        [Tooltip("this defines the highest ledge, as a multiple of the character controller's height, that the character can 'reach' to mantle (aka climb onto).")]
        public float mantleCheckCharacterHeightFactor = 1.8f;

        public float mantleReallyHighThreshold = 2.5f;

        public float parkourCheckMaxDistToKneeCollision = 1f;

        [Tooltip("this defines the height of the knee-level obstacle check, as a multiple of the character controller's height.")]
        public float kneeLevelObstacleCheckHeightFactor = 0.25f;

        public float headLevelObstacleCheckHeightFactor = 0.85f;

        public float inclineCheckMaxDist = 4.5f;

        public float inclineCheckOriginHeightFactor = 0.3f;

        public float inclineCheckRadius = 0.15f;

        [Header("Model-specific size factors")]
        public bool applyModelSpecificMultipliers = false;
        public float gaitMultiplier = 1.0f;
        public float heightMultiplier = 1.0f;
        public float apeIndexMultiplier = 1.0f;
    }

}