using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_StrafeTarget : SimpleKeyProcessor
    {
        [Tooltip("If true, whenever you enable/disabled strafe-target, strafe will also be enabled/disabled")]
        public bool toggleStrafeEnabledWithTargetToggle = true;

        public Transform strafeTarget = null;

        protected bool strafeTargetSet = false;
        
        public override void HandleKeyDown()
        {
            if (strafeTargetSet)
            {
                character.movement.ClearStrafeTarget(toggleStrafeEnabledWithTargetToggle);
            }
            else 
            {
                if (strafeTarget != null)
                {
                    character.movement.SetStrafeTarget(strafeTarget, toggleStrafeEnabledWithTargetToggle);
                }
                else
                {
                    Debug.Log("Example_StrafeTarget: no strafe target is set, please set Example_StrafeTarget.strafeTarget.");
                }
            }
            strafeTargetSet = !strafeTargetSet;
        }
    }
}