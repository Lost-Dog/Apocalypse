using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreepeatEditor
{
    [CreateAssetMenu(fileName = "ThreepeatDependencyInfoFBXModifier", menuName = "Threepeat/FBX Modifier Dependency Info")]
    public class ThreepeatFBXModificationDependencyInfo : ThreepeatDependencyInfo
    {
        public FBXAnimationEventInfo fbxModifier;

        public override bool HasOverriddenDependencyCheckFunction()
        {
            return true;
        }

        public override bool OverriddenDependencyCheck_IsDependencyMet()
        {
            return (fbxModifier != null) && fbxModifier.HasModificationAlreadyBeenApplied();
        }

        // returns null if no custom remediation button and button's GUI text otherwise.
        public override string HasOverriddenRemediationButton()
        {
            return "Modify FBX Prefabs";
        }

        public override void OverriddenRemediate()
        {
            if (fbxModifier != null)
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "This will inject Animation Events, apply root-motion settings and remove the root motion node reference from the FBX Prefabs.  Backing up your project first is highly recommended.", "Modify FBX Prefabs for MMLC", "Cancel"))
                {
                    fbxModifier.ApplyEvents();
                }
            }
        }

    }
}