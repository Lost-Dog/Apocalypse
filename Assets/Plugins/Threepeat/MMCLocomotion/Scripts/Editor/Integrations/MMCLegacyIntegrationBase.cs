using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreepeatEditor
{

    public abstract class MMCLegacyIntegrationBase
    {
        public abstract bool IsPlaceholder();

        public abstract void MakeGUI();

        public abstract bool SetupCharacter(GameObject coreObject, GameObject modelObject);

    }

}