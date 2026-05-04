using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 0)]

    [Title("Find Best Cover and Move")]
    [Description("Finds the first appropriate cover point for the character and moves them to it, then enters cover")]

    [Category("Cover/Find Best Cover")]

    [Parameter("Character", "The Character game object that will find and move to cover")]
    [Parameter("Stop Distance", "Distance to the cover point that the character considers it has reached the target")]
    [Parameter("Cancel on Fail", "Stops executing the rest of Instructions if the path has been obstructed")]
    [Parameter("On Fail", "Instructions executed when it can't reach the destination")]
    [Parameter("Search Distance", "Maximum distance to search for cover")]

    [Keywords("Character", "Cover", "Move", "Evaluator")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCharacterFindBestCover : TInstructionCharacterNavigation
    {
        [Serializable]
        public class NavigationOptions
        {
            [SerializeField] private bool m_WaitToArrive = true;
            [SerializeField] private bool m_CancelOnFail = true;
            [SerializeField] private InstructionList m_OnFail = new InstructionList();

            public bool WaitToArrive => this.m_WaitToArrive;
            public bool CancelOnFail => this.m_CancelOnFail;
            public InstructionList OnFail => this.m_OnFail;
        }

        private class NavigationResult
        {
            [NonSerialized] private bool m_Complete;
            [NonSerialized] private bool m_Success = true;

            public void OnFinish(Character character, bool success)
            {
                this.m_Complete = true;
                this.m_Success = success;
            }

            public async Task<bool> Await(Character character, Vector3 targetPosition, float stopDistance, NavigationOptions options)
            {
                if (options.WaitToArrive)
                {
                    while (!this.m_Complete)
                    {
                        float distanceToTarget = Vector3.Distance(character.transform.position, targetPosition);
                        if (distanceToTarget <= stopDistance)
                        {
                            this.m_Complete = true;
                            break;
                        }

                        await Task.Yield();
                    }
                }

                return this.m_Success;
            }
        }

        [SerializeField] private LayerMask m_CoverLayerMask;
        [SerializeField] private float m_SearchDistance = 10f;
        [SerializeField] private float m_MinValidDotProduct = 0.5f;
        [SerializeField] private float m_MinimalDistance = 2f;
        [SerializeField] private PropertyGetDecimal m_StopDistance = GetDecimalConstantZero.Create;
        [SerializeField] private NavigationOptions m_Options = new NavigationOptions();

        [NonSerialized] private NavigationResult m_Result;

        public override string Title => $"Find and Move {this.m_Character} to Best Cover";

        protected override async Task Run(Args args)
        {
            this.m_Result = new NavigationResult();

            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;

            Transform bestCover = FindFirstValidCover(character.transform.position);
            if (bestCover == null)
            {
                await this.m_Options.OnFail.Run(args);
                if (this.m_Options.CancelOnFail) this.NextInstruction = int.MaxValue;
                return;
            }

            Vector3 coverPosition = bestCover.position;
            Quaternion coverRotation = Quaternion.LookRotation(bestCover.forward, Vector3.up);
            Location coverLocation = new Location(coverPosition, coverRotation);

            character.Motion.MoveToLocation(
                coverLocation,
                (float)this.m_StopDistance.Get(args),
                this.m_Result.OnFinish
            );

            bool success = await this.m_Result.Await(character, coverPosition, (float)this.m_StopDistance.Get(args), this.m_Options);

            if (!success)
            {
                await this.m_Options.OnFail.Run(args);
                if (this.m_Options.CancelOnFail) this.NextInstruction = int.MaxValue;
                return;
            }

            character.transform.rotation = coverRotation;

            CoverController coverController = character.GetComponent<CoverController>();
            if (coverController != null)
            {
                await coverController.TryEnterCover();
            }
        }

        private Transform FindFirstValidCover(Vector3 enemyPosition)
        {
            Collider[] potentialCovers = Physics.OverlapSphere(enemyPosition, this.m_SearchDistance, this.m_CoverLayerMask);

            Array.Sort(potentialCovers, (a, b) =>
                Vector3.Distance(enemyPosition, a.transform.position).CompareTo(
                    Vector3.Distance(enemyPosition, b.transform.position))
            );

            foreach (Collider coverCollider in potentialCovers)
            {
                Transform coverTransform = coverCollider.transform;

                if (IsCoverValid(coverTransform, enemyPosition, out _))
                {
                    return coverTransform;
                }
            }

            return null;
        }

        private bool IsCoverValid(Transform cover, Vector3 enemyPosition, out float score)
        {
            score = 0;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return false;

            if (Vector3.Distance(cover.position, player.transform.position) < this.m_MinimalDistance)
            {
                return false;
            }

            Collider[] nearbyColliders = Physics.OverlapSphere(cover.position, 2f, LayerMask.GetMask("Default"));
            if (nearbyColliders.Length == 0) return false;

            Vector3 wallNormal = (cover.position - nearbyColliders[0].ClosestPoint(cover.position)).normalized;
            float wallAlignment = Vector3.Dot(cover.forward, wallNormal);
            if (wallAlignment > -0.5f) return false;

            if (!Physics.Linecast(cover.position, player.transform.position, out _, this.m_CoverLayerMask)) return false;

            return true;
        }
    }
}
