using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace ThreepeatEditor
{
    [System.Serializable]
    public class MMCFinalIKGrounderIntegration : MMCLegacyIntegrationBase
    {

        public override bool IsPlaceholder()
        {
            return true;
        }

        public override void MakeGUI()
        {
        }

        public override bool SetupCharacter(GameObject coreObject, GameObject modelObject)
        {
            return true;
        }
    }

}
