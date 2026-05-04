using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Perception
{
    [Version(0, 0, 1)]

    [Title("Track Nearest Awareness")]
    [Description("Starts tracking the nearest game object with a specified tag within a given radius, making the target become the focus of awareness.")]

    [Category("Perception/Track Nearest Awareness")]

    [Keywords("Perceive", "Alert", "Nearest", "Track")]
    [Image(typeof(IconTrack), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionPerceptionTrackNearest : Instruction
    {
        // Reference to the Perception component on the character
        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectPerception.Create;

        // The tag used as criteria for potential targets (e.g., "Enemy")
        [SerializeField]
        private string m_TargetTag = "Enemy";

        // Optional: A search radius within which to look for targets
        [SerializeField]
        private float m_SearchRadius = 50f;

        public override string Title => $"{this.m_Perception} track nearest '{this.m_TargetTag}'";

        protected override Task Run(Args args)
        {
            // Retrieve the Perception component from the provided property
            Perception perception = this.m_Perception.Get<Perception>(args);
            if (perception == null) return DefaultResult;

            // Find all game objects with the specified tag
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(this.m_TargetTag);
            if (candidates.Length == 0) return DefaultResult;

            // Determine the nearest candidate within the search radius.
            GameObject nearestTarget = null;
            float closestDistanceSqr = this.m_SearchRadius * this.m_SearchRadius; // using squared distances for efficiency
            Vector3 currentPosition = perception.transform.position;

            foreach (GameObject candidate in candidates)
            {
                if (candidate == null) continue;

                // Calculate squared distance to candidate
                float distanceSqr = (candidate.transform.position - currentPosition).sqrMagnitude;
                if (distanceSqr <= closestDistanceSqr)
                {
                    // If this candidate is closer than any previously found, select it
                    if (nearestTarget == null || distanceSqr < (nearestTarget.transform.position - currentPosition).sqrMagnitude)
                    {
                        nearestTarget = candidate;
                        closestDistanceSqr = distanceSqr;
                    }
                }
            }

            // If a target was found, instruct the Perception to track it
            if (nearestTarget != null)
            {
                perception.Track(nearestTarget);
            }

            return DefaultResult;
        }
    }
}
