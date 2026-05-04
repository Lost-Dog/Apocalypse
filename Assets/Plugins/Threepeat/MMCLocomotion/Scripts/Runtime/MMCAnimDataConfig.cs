using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace Threepeat
{
#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "AnimDataConfig", menuName = "Threepeat/AnimData Config (for runtime use and configwizard discovery)")]
#endif
    public class MMCAnimDataConfig : ScriptableObject
    {
        public string guiName;
        public string description;
        public MxMAnimData animData;
        public NGCharacterBaseConfig config;
        public CalibrationModule calibrationModule;

        // these strings map to the superset of NGCharacter.CharacterState and Sprint, Crouch, Strafe, CrouchStrafe
        public string[] supportedCharacterStates = null;

        /*[Tooltip("all lowercase")]
        public string[] supportedRequireTags = null;

        [Tooltip("all lowercase")]
        public string[] supportedEvents = null;*/

        public bool hasParkour = false;
    }
}