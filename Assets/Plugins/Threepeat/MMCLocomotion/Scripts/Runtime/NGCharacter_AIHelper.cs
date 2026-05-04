using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    [System.Serializable]
    public class NGCharacter_AIHelper : NGCharacterHelper
    {
        public NGCharacter_AIHelper(NGCharacter ch) : base(ch)
        {
        }

        public override void Initialize()
        {

        }

        public float moveToDestination_DestinationCheckInterval = 0.5f;
        public float moveToDestination_DestinationCheckTolerance = 1f;

        public bool moveTo_active = false;
        protected Transform moveTo_destination = null;
        protected Transform moveTo_backupCamTransform = null;
        protected NGInputSchemeBase moveTo_backupInputScheme = null;
        protected NGCharacter.VoidCompletionCallback moveTo_completionCallback = null;
        protected float moveTo_lastDestinationCheckTime = -1f;
        protected bool moveTo_wasStrafing = false;

        public void MoveToDestination(Transform destination, NGCharacter.VoidCompletionCallback onComplete = null, NGInputScheme_NavMesh nmis = null)
        {
            moveTo_wasStrafing = character.Strafing;
            moveTo_backupCamTransform = character.mxmTrajectoryGenerator.RelativeCameraTransform;
            moveTo_backupInputScheme = character.InputScheme; //Object.Instantiate(character.InputScheme);
            if (nmis != null)
            {
                character.SetInputScheme(nmis, null, false);
                nmis.wantedTargetTransform = destination;
            }
            else
            {
                NGInputScheme_NavMesh nmis2 = NGInputScheme_NavMesh.CreateInstance<NGInputScheme_NavMesh>();
                // set InputScheme to NavMesh
                nmis2.wantedTargetTransform = destination;
                nmis2.character = character;
                nmis2.doEnvironmentAwareTrajectories = false;
                nmis2.mxmTrajectoryGenerator = character.mxmTrajectoryGenerator;
                character.SetInputScheme(nmis2, null, false);
            }
            moveTo_destination = destination;
            moveTo_active = true;
            moveTo_completionCallback = onComplete;

            character.RegisterCustomUpdateMethod(MoveToDestination_CustomUpdate);

        }

        protected void MoveToDestination_CustomUpdate()
        {
            // check if at destination
            if ((Time.time - moveTo_lastDestinationCheckTime) > moveToDestination_DestinationCheckInterval)
            {
                moveTo_lastDestinationCheckTime = Time.time;
                float grounddist = (character.transform.position - new Vector3(moveTo_destination.position.x, character.transform.position.y, moveTo_destination.position.z)).magnitude;
                if (grounddist <= moveToDestination_DestinationCheckTolerance)
                {
                    character.StartCoroutine(EndMoveTo(true));
                }
                /*else
                {
                    Debug.Log($"Checking dist: {Vector3.Project(character.transform.position - moveTo_destination.position, Vector3.up).magnitude} {(character.transform.position - new Vector3(moveTo_destination.position.x, character.transform.position.y, moveTo_destination.position.z)).magnitude} vs thresh( {moveToDestination_DestinationCheckTolerance} )");
                }*/
            }
        }

        protected IEnumerator EndMoveTo(bool complete)
        {
            yield return 0;
            CancelMoveToDestination(complete);
            yield return 0;
        }

        public void CancelMoveToDestination(bool complete = false)
        {
            if (moveTo_backupInputScheme != null)
            {
                //Debug.Log($"setting input scheme back:  camt( {moveTo_backupCamTransform} )");
                character.mxmTrajectoryGenerator.RelativeCameraTransform = moveTo_backupCamTransform;
                //character.InputScheme = moveTo_backupInputScheme;
                character.SetInputScheme(moveTo_backupInputScheme, moveTo_backupCamTransform);
            }
            else
            {
                Debug.Log("Input scheme already null somehow");
            }
            if (moveTo_wasStrafing)
            {
                character.Strafing = true;
            }
            if (complete && (moveTo_completionCallback != null))
            {
                moveTo_completionCallback();
            }
            DisableMoveToDestinationVars();
        }

        protected void DisableMoveToDestinationVars()
        {
            moveTo_active = false;
            moveTo_completionCallback = null;
            moveTo_backupCamTransform = null;
            moveTo_destination = null;
            moveTo_backupInputScheme = null;
            character.UnregisterCustomUpdateMethod(MoveToDestination_CustomUpdate);
        }

    }
}