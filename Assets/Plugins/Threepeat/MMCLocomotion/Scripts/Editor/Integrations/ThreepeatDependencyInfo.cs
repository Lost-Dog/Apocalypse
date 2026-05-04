using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    //TODO: remember to disable this CreateAssetMenu before publishing package, since noone should be creating these
    // and extra SO's in the project would mess up the Dependency checking
    [CreateAssetMenu(fileName = "ThreepeatDependencyInfo", menuName = "Threepeat/Dependency Info")]
    public class ThreepeatDependencyInfo : ScriptableObject
    {
        // Whether this is an optional integration (like Final IK Grounder)
        public bool requiredForMMCFunction = true;

        public string[] GUIDsToCheck = { };

        public bool hasIntegrationPackage = false;

        public string integrationPackagePath = "Assets/Plugins/Threepeat/MMCLocomotion/Integrations/Something.unitypackage";
        public string integrationPackageGUIDOfUnextractedPackage = "";

        // This is a guid of an asset whose existence demonstrates that the integration package has been successfully extracted/imported into the project.
        public string integrationPackageGUIDToCheck = "";

        public string integrationName = "Integration";

        [TextArea(3,10)]
        public string notFoundHelpBoxText = "1. Import PackageName from the Package Manager. \n2. Click Recheck Dependencies button, below.";


        public string[] dependencyPackages = { };

        public bool showOpenPackageManagerButton = false;
        public bool showOpenProjectSettingsButton = false;

        public string MAJOR_SECTION_NAME = "";

        public bool onlyShowWhenShowIfGuidIsPresent = false;
        public string showIfGuid = "";

        public virtual bool HasOverriddenDependencyCheckFunction()
        {
            return false;
        }

        public virtual bool OverriddenDependencyCheck_IsDependencyMet()
        {
            return false;
        }

        // returns null if no custom remediation button and button's GUI text otherwise.
        public virtual string HasOverriddenRemediationButton()
        {
            return null;
        }

        public virtual void OverriddenRemediate()
        {
        }

    }
}