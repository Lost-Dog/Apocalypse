using MxMEditor;
using System.Collections;
using System.Collections.Generic;
using Threepeat;
using UnityEngine;

namespace ThreepeatEditor
{
    public abstract class MMCIntegrationBase : ScriptableObject
    {
        public List<ThreepeatDependencyInfo>  dependencies;

        [HideInInspector] public bool selected;

        // This method returns whether the integration is available, outside of whether the above 
        // dependencies are met.  Whether any of the dependencies (in the dependencies list) are met
        // will be checked by the ConfigWizard independent of this function.
        //
        // This method is only required to be overridden if there is some peculiar/special availability
        // check required beyond what is covered in the ThreepeatDependencyInfo's.
        protected virtual bool IsAvailable() 
        {
            return true;
        }

        public virtual bool RequiresAnimDataReprocess()
        {
            return false;
        }

        public virtual List<AnimationModule> GetAdditionalAnimationModules()
        {
            return null;
        }

        public virtual bool IsMajorIntegration()
        {
            return false;
        }

        // This is called whenever the integration enable checkbox is enabled (checked) by user.
        public virtual void OnIntegrationEnable()
        {
        }

        public abstract string GetIntegrationName();

        public abstract string GetDescription();

        public virtual string GetHelpLink()
        {
            return null;
        }

        public virtual void MakeGUI()
        {
        }

        public abstract bool SetupIntegration(GameObject rootObject, GameObject modelObject, NGCharacter character, GameObject camObject = null);

    }
}