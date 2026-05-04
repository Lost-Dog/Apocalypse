using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxMEditor;

namespace ThreepeatEditor
{
#if ENABLE_THREEPEAT_DEV_MODE
    [CreateAssetMenu(fileName = "ParkourAnimModuleConfig", menuName = "Threepeat/Parkour Anim Module Config")]
#endif
    public class ParkourAnimModuleConfig : ScriptableObject
    {
        public string guiName = "";
        public string description = "";
        public AnimationModule[] animModules = null;
    }
}