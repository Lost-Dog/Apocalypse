using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class CustomJumpAnimations : MonoBehaviour
    {
        protected NGCharacter character = null;

        [Tooltip("If true, this action will replace the standard jump (Parkour will remain at the top of the stack).  If false, the E key will fire this jump action.")]
        public bool overrideJumpKey = true;

        public float landingHeight = 0f;
        public float customJumpHeight = -1;
        public bool keepMomentum = true;
        public Vector3 jumpSpeedBoostLocalSpace = Vector3.zero;

        private Vector3 startVelocity;

        public JumpPhase jumpPhase = JumpPhase.None;

        public string jumpAndMidairLayer = "Jump";
        public string landingLayer = "Landing";

        public string jumpAnimationName = "jump";
        public string midairAnimationName = "midair";
        public string landingAnimationName = "landing-soft";
        public float blendOutTime = 0.1f;

        private string ACTIONNAME_CUSTOMJUMP = "CUSTOMJUMP";

        public enum JumpPhase
        {
            None,
            Launch,
            Midair,
            Landing
        }

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<NGCharacter>();

            character.movement.onLand.AddListener(OnLand);

            if (overrideJumpKey)
            {
                character.RegisterContextualAction(ACTIONNAME_CUSTOMJUMP, CustomJump_CanPerform, CustomJump_Perform);
                character.onInputSchemeChanged.AddListener(OnInputSchemeChangedListener);
                OnInputSchemeChangedListener(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!overrideJumpKey && Input.GetKeyDown(KeyCode.E))
            {
                DoJump();
            }

            if ((jumpPhase == JumpPhase.Midair) && (landingHeight > 0))
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position, Vector3.down, out hit, landingHeight, character.controllerWrapper.GroundLayers))
                {
                    OnLand(2);
                    jumpPhase = JumpPhase.None;
                }
            }

            if (keepMomentum && ((jumpPhase == JumpPhase.Launch) || (jumpPhase == JumpPhase.Midair)))
            {
                character.controllerWrapper.Move(Time.deltaTime * startVelocity/2.0f + Time.deltaTime * transform.TransformVector(jumpSpeedBoostLocalSpace));
            }
        }

        private void DoJump()
        {
            startVelocity = character.controllerWrapper.Velocity;
            character.anim.PlayOneshot(jumpAndMidairLayer, jumpAnimationName, OnComplete, 0.25f, 0.1f, false);
            jumpPhase = JumpPhase.Launch;
            character.controllerWrapper.Jump((customJumpHeight > 0) ? customJumpHeight : character.config.configJump.jumpBigHeight);
            character.movement.doFallDetection = false;
        }

        private void OnComplete(string layerName, string animName)
        {
            switch (jumpPhase)
            {
                case JumpPhase.Launch:
                    // move to midair loop
                    jumpPhase = JumpPhase.Midair;
                    character.anim.PlayLoopSameLayer(jumpAndMidairLayer, midairAnimationName, 0.05f);
                    break;
                default:
                    break;
            }
        }

        void OnLand(int landType)
        {
            if (jumpPhase == JumpPhase.None)
            {
                character.movement.doFallDetection = true;
                return;
            }
            character.anim.StopEventLayer(jumpAndMidairLayer, 0.1f);
            character.anim.PlayOneshot(landingLayer, landingAnimationName, OnLandingComplete, 0.25f, blendOutTime);
            jumpPhase = JumpPhase.None;
        }

        private void OnLandingComplete(string layerName, string animName)
        {
            character.movement.doFallDetection = true;
        }

        private IEnumerator SpeedBoostWorker(float speedToAdd)
        {
            while (true)
            {
                character.controllerWrapper.Move(transform.forward * Time.deltaTime * speedToAdd);
                yield return null;
            }
        }

        private void OnInputSchemeChangedListener(bool isInitial)
        {
            /*if (isInitial)
            {
                return;
            }*/

            if (!character.InputScheme.IsInputDriven())
            {
                // NPC, nothing to do here.
                return;
            }
            NGInputSchemeInputDriven scheme = (NGInputSchemeInputDriven)character.InputScheme;

            if (scheme.keyProcessorJumpParkour == null)
            {
                return;
            }

            List<ContextualActionTrigger> triggers = new List<ContextualActionTrigger>(scheme.keyProcessorJumpParkour.Triggers);

            if (!NGCharacter.ActionNamesMatch(triggers[0].actionName, "Parkour"))
            {
                // parkour's not at top of this trigger, so we can just add to top
                triggers.Insert(0, new ContextualActionTrigger(ACTIONNAME_CUSTOMJUMP));
                scheme.keyProcessorJumpParkour.Triggers = triggers.ToArray();
            }
            else
            {
                triggers.Insert(1, new ContextualActionTrigger(ACTIONNAME_CUSTOMJUMP));
                scheme.keyProcessorJumpParkour.Triggers = triggers.ToArray();
            }
        }

        public bool CustomJump_CanPerform(ContextualActionTrigger.TriggeringCondition trigger)
        {
            return character.controllerWrapper.IsGrounded && (jumpPhase == JumpPhase.None);
        }

        public ContextualAction.Propagation CustomJump_Perform(ContextualActionTrigger.TriggeringCondition trigger)
        {
            DoJump();
            return ContextualAction.Propagation.StopPropagationForAllSuccessiveEvents;
        }

    }
}