using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class is heavily inspired by MxM's LocomotionSpeedRamp built in AnimationUprising's Youtube channel


namespace Threepeat
{
    public class NGLocomotionTagController
    {
        private MxMAnimator mxmAnimator;
        private MxMTrajectoryGenerator mxmTrajectoryGenerator;
        private NGCharacter character;
        private ETags walkTag;
        private ETags runTag;
        private ETags sprintTag;
        private bool tagsSet = false;
        private float favourMultiplier = 0.6f;
        private float speedDowngradeTime = 0.3f;
        private float speedTimer = -1f;

        private bool haveSetInitialFavourMultiplier = false;

        private LocoState currentState = LocoState.Walk;
        public enum LocoState
        {
            Walk,
            Run,
            Sprint
        }


        public NGLocomotionTagController(MxMAnimator mxmanim, MxMTrajectoryGenerator trajgen, NGCharacter chr)
        {
            Initialize(mxmanim, trajgen, chr);
        }

        public void Initialize(MxMAnimator mxmanim, MxMTrajectoryGenerator trajgen, NGCharacter chr)
        {
            mxmAnimator = mxmanim;
            mxmTrajectoryGenerator = trajgen;
            character = chr;
            tagsSet = false;
            if (mxmAnimator.CurrentAnimData != null)
            {
                runTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Run");
                sprintTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Sprint");
                walkTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Walk");
                tagsSet = true;
            }

        }

        private void CheckTags()
        {
            if (!tagsSet && (mxmAnimator.CurrentAnimData != null))
            {
                runTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Run");
                sprintTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Sprint");
                walkTag = mxmAnimator.CurrentAnimData.FavourTagFromName("Walk");
                tagsSet = true;
            }
        }

        public void BeginSprint()
        {
            CheckTags();
            if ((currentState == LocoState.Run) || (currentState == LocoState.Walk))
            {
                mxmAnimator.RemoveFavourTags(runTag);
                mxmAnimator.RemoveFavourTags(walkTag);
            }

            currentState = LocoState.Sprint;

            mxmAnimator.AddFavourTags(sprintTag);
        }

        public void StopSprint(bool canRun = true)
        {
            CheckTags();
            if (currentState == LocoState.Sprint)
            {
                mxmAnimator.RemoveFavourTags(sprintTag);
            }

            speedTimer = 0f;

            if (canRun)
            {
                currentState = LocoState.Run;
                float inputMag = mxmTrajectoryGenerator.InputVector.sqrMagnitude;
                if (inputMag > 0.7f * 0.7f)
                {
                    mxmAnimator.AddFavourTags(runTag);
                }
                else
                {
                    mxmAnimator.AddFavourTags(walkTag);
                }
            }
            else
            {
                mxmAnimator.AddFavourTags(walkTag);
                currentState = LocoState.Walk;
            }



        }

        public void UpdateSpeedRamp()
        {
            CheckTags();
            float inputMag = mxmTrajectoryGenerator.InputVector.sqrMagnitude;

            if (!haveSetInitialFavourMultiplier)
            {
                mxmAnimator.SetFavourMultiplier(favourMultiplier);
            }

            switch (currentState)
            {
                case LocoState.Walk:
                    if (character.movement.canRun && (inputMag > 0.7f * 0.7f))
                    {
                        //Debug.Log("Running now");
                        currentState = LocoState.Run;
                        mxmAnimator.AddFavourTags(runTag);
                        mxmAnimator.RemoveFavourTags(walkTag);
                        mxmAnimator.SetFavourMultiplier(favourMultiplier);
                    }
                    else
                    {
                        currentState = LocoState.Walk;
                        //Debug.Log("Walking now");
                        mxmAnimator.RemoveFavourTags(runTag);
                        mxmAnimator.AddFavourTags(walkTag);
                    }

                    break;
                case LocoState.Run:
                    //Debug.Log("Running now");
                    if (inputMag < 0.7f * 0.7f)
                    {
                        if (speedTimer < -Mathf.Epsilon)
                        {
                            speedTimer = 0f;
                        }

                        speedTimer += Time.deltaTime;

                        if (speedTimer > speedDowngradeTime)
                        {
                            mxmAnimator.RemoveFavourTags(runTag);
                            mxmAnimator.AddFavourTags(walkTag);
                            mxmAnimator.SetFavourMultiplier(favourMultiplier);
                            speedTimer = -1f;
                        }
                    }
                    else
                    {
                        mxmAnimator.AddFavourTags(runTag);
                        mxmAnimator.RemoveFavourTags(walkTag);
                        mxmAnimator.SetFavourMultiplier(favourMultiplier);
                    }

                    if (!character.movement.canRun)
                    {
                        currentState = LocoState.Walk;
                        //Debug.Log("Walking now");
                        mxmAnimator.RemoveFavourTags(runTag);
                        mxmAnimator.AddFavourTags(walkTag);
                    }
                    break;
                case LocoState.Sprint:
                    break;
            }

        }
    }
}