using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;
using MxMEditor;
using Threepeat;

namespace ThreepeatEditor
{
#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "AnimDataEditorConfig", menuName = "Threepeat/AnimData Editor Config (for creation of custom animdata)")]
#endif
    public class MMCAnimDataEditorConfig : ScriptableObject
    {
        [System.Serializable]
        public class MMCAnimModuleInfo
        {
            [Tooltip("all lowercase")]
            public string[] supportedRequireTags = null;

            [Tooltip("all lowercase")] 
            public string[] supportedEvents = null;
            
            public AnimationModule[] animModules = null;
        }

        [Tooltip("Leave empty to autopopulate with asset name")]
        public string guiName = "";
        
        public string description = "";

        public MMCAnimDataConfig animDataConfig;

        public GameObject modelPrefab = null;
        //public MMCAnimModuleInfo[] modules = null;
        
        public AnimationModule[] animModules = null;

        public string baseAnimDataConfigName = null;

        // Need to think about these, as truly MMLC requires consistent event and tag ordering/naming
        public EventNamingModule eventModule = null;
        public TagNamingModule tagNamingModule = null;
        public MotionMatchConfigModule configModule = null;
        public MxMPreProcessData preprocessor = null;

    }
}