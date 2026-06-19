using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "NGMxMEventDef", menuName = "Threepeat/NGMxMEventDef", order = 0)]
    public class NGMxMEventDef : MxMEventDefinition
    {
        /*
        public MxMAnimData animDataRef;

        [ValueDropdown("EventNamesFunction")]
        public int eventID;         // redundant with eventDefinition below.
        public MxMEventDefinition eventDefinition;*/

        public NGAnimationEventType NGEventType = NGAnimationEventType.parkour_mantle;
        public string eventTypeName;

        public NGInboundMovementType NGMovementType = NGInboundMovementType.Running;

        [Range(0f, 10f)]
        [Tooltip("The range of acceptable forward distances to the contact (where contact is e.g. leading edge of a platform you're jumping to or vaulting over)")]
        public Vector2 zRangeToContact = new Vector2(1.0f, 2.0f);

        [Range(0f, 10f)]
        [Tooltip("The range of acceptable relative heights to the contact (where contact is e.g. a platform you're jumping to or vaulting over)")]
        public Vector2 yRangeToContact = new Vector2(0.3f, 0.6f);

        public float maxPlatformDepth = -1f;  // -1 for vault-ons

        //TODO: maybe make this range support negative as well so you can make events you can back into...
        // like backing into a wall, or backing into a 1m platform and doing a reverse roll on to it.
        [Range(-10f, 50f)]
        [Tooltip("The range of acceptable inbound speeds (this allows you to differentiate between standing, walking, running events, etc")]
        public Vector2 speedRange = new Vector2(0f, 50f);

        [SerializeField]
        public ContactInfo[] contactInfos = new ContactInfo[] {
        new ContactInfo(true, Vector3.up)
    };

        [System.Serializable]
        public class ContactInfo
        {
            public ContactInfo()
            {
                warpable = true;
                relativePosition = Vector3.up;
            }

            public ContactInfo(bool wrp, Vector3 relpos)
            {
                warpable = wrp;
                relativePosition = relpos;
            }
            public bool warpable = true;
            public Vector3 relativePosition = Vector3.up;
            // X should be 0 unless you know what you're doing
            // Y 0 = current level of player, 1 = platform (will be scaled to platform/sill height)
            // Z is forward distance from previous contact or current player position if this is 
            // the first contact (in case of a precision jump or something, but this should likely
            // be zero for first contact as any non-zero value is the offset from leading edge of the
            // platform or trailing edge in case of a drop).

        }

        public enum NGInboundMovementType
        {
            WalkingOrIdle,
            Running,
            Climbing
        }

        public enum NGAnimationEventType
        {
            environment_wallstop,
            parkour_vaultlow,
            parkour_mantlelow,            // Maps to MMLC MxM Event Naming::Mantle
            parkour_vaultwindow,        // different event defs based on idle/walk or running+
            parkour_vaultfenceclear,     // AKA HighVault
            parkour_vaultfenceclear_reallyhigh,
            parkour_mantle,        // AKA HighVaultOn - maps to MMLC MxM Event Naming::MantleHigh
            parkour_mantle_reallyhigh, 
            parkour_drop,
            climb,
            ByStringName,
            parkour_vaultfenceclear_leftwall,
            parkour_vaultfenceclear_rightwall
        }

        [Range(0f, 10f)]
        public Vector2 vaultObstacleLengthRange = new Vector2(0.1f, 3f);

        [SerializeField]
        public MMCAnimationClipInfo[] animationClipInfos;

        /*
        private IList<ValueDropdownItem<int>> EventNamesFunction()
        {
            ValueDropdownList<int> theList = new ValueDropdownList<int>();
            if (animDataRef != null)
            {
                for (int ii=0; ii < animDataRef.EventNames.Length; ii++)
                {
                    theList.Add(animDataRef.EventNames[ii], ii);
                }
            }
            return theList;
        }*/
    }
}