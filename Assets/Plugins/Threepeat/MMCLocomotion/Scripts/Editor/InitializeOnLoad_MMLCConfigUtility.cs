using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    public class InitializeOnLoad_MMLCConfigUtility
    {
        [InitializeOnLoadMethod]
        static void CheckAndAddMMLCDefine()
        {
            string scriptDefines = "ENABLE_MMLC";

            // Check scripting defines
            BuildTargetGroup target = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            string[] split = defines.Split(';');

            if (Array.IndexOf(split, scriptDefines) == -1)
            {
                Debug.Log($"MMLC: Adding Script Defines '{scriptDefines}'");

                defines += $";{scriptDefines}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            }
        }
    }
}