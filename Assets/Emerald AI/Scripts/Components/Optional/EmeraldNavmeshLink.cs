using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/navmesh-link-component")]
    public class EmeraldNavmeshLink : MonoBehaviour
    {
        [Range(0f, 6f)] public float NavmeshLinkCooldown = 1f;
        public bool SettingsFoldout;
        public bool HideSettingsFoldout;

        EmeraldSystem EmeraldComponent;
        bool traversing;
        Vector3 CurrentDestination;
        LinkMoveType CurrentLinkMoveType = LinkMoveType.Drop;
        public enum LinkMoveType { Jump, Vault, Drop }

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
        }

        void Update()
        {
            EmeraldComponent.m_NavMeshAgent.autoTraverseOffMeshLink = !EmeraldComponent.AnimationComponent.BusyBetweenStates || EmeraldComponent.MovementComponent.enabled || EmeraldComponent.AnimationComponent.IsIdling;

            if (!traversing && EmeraldComponent.m_NavMeshAgent.isOnOffMeshLink && EmeraldComponent.m_NavMeshAgent.autoTraverseOffMeshLink)
                StartCoroutine(Traverse(EmeraldComponent.m_NavMeshAgent.currentOffMeshLinkData));
        }

        IEnumerator Traverse(OffMeshLinkData data)
        {
            traversing = true;

            EmeraldComponent.m_NavMeshAgent.speed = 1f;
            CurrentDestination = EmeraldComponent.m_NavMeshAgent.destination;

            //Determine the Link Move Type
            float linkDistance = EmeraldComponent.m_NavMeshAgent.currentOffMeshLinkData.endPos.y - EmeraldComponent.m_NavMeshAgent.currentOffMeshLinkData.startPos.y;
            if (linkDistance < -0.8f) CurrentLinkMoveType = LinkMoveType.Drop;
            else if (linkDistance > 0.6f) CurrentLinkMoveType = LinkMoveType.Jump;
            else CurrentLinkMoveType = LinkMoveType.Vault;

            //Trigger the animation based on CurrentLinkMoveType
            switch (CurrentLinkMoveType)
            {
                case LinkMoveType.Vault: EmeraldComponent.AIAnimator.SetTrigger("Vault"); break;
                case LinkMoveType.Drop: EmeraldComponent.AIAnimator.SetTrigger("Drop"); break;
                default: EmeraldComponent.AIAnimator.SetTrigger("Jump"); break;
            }

            //Play the jump sound (this is done for vault, jump, or drop)
            EmeraldComponent.SoundComponent.PlayJumpSound();

            //Pause the agent while handling the NavMesh Link
            EmeraldComponent.m_NavMeshAgent.updatePosition = false;
            EmeraldComponent.m_NavMeshAgent.updateRotation = false;
            EmeraldComponent.m_NavMeshAgent.isStopped = true;

            yield return NavMeshSequence(data);

            traversing = false;
        }

        /// <summary>
        /// Attempt to automatically calculate the arc and height needed to make the jump.
        /// </summary>
        float ComputeArcHeight(Vector3 start, Vector3 end, float baseClearance = 0.35f, float distFactor = 0.18f, float upFactor = 0.6f, float downFactor = 0.15f, float minArc = 0.0f, float maxArc = 6.0f)
        {
            //Get the distance
            float d = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));
            float dy = end.y - start.y;

            //Arc Height
            float clearance = baseClearance + d * distFactor + Mathf.Max(0f, dy) * upFactor + Mathf.Max(0f, -dy) * downFactor;
            clearance = Mathf.Max(0f, clearance);
            float targetPeakY = Mathf.Max(start.y, end.y) + clearance;
            float midY = Mathf.Lerp(start.y, end.y, 0.5f);
            float arcHeight = Mathf.Max(0f, targetPeakY - midY);
            arcHeight = Mathf.Clamp(arcHeight, minArc, maxArc);
            return arcHeight;
        }


        IEnumerator NavMeshSequence(OffMeshLinkData data)
        {
            float arcHeight = 1f;
            float moveSpeed = 5f;

            Vector3 start = transform.position;
            Vector3 end = data.endPos;
            Vector3 direction = (end - start);

            float distance = Vector3.Distance(start, end);
            if (distance <= 0.001f)
                yield break;

            if (CurrentLinkMoveType == LinkMoveType.Drop)
            {
                arcHeight = 1f;
                moveSpeed = 5.25f;
            }
            else if (CurrentLinkMoveType == LinkMoveType.Jump)
            {
                arcHeight = 1f;
                moveSpeed = 5f;
            }
            else if (CurrentLinkMoveType == LinkMoveType.Vault)
            {
                moveSpeed = 6f + (distance * 0.25f);
                arcHeight = ComputeArcHeight(start, end);
            }

            direction.y = 0f;

            Quaternion startRot = transform.rotation;
            Quaternion lookRot = Quaternion.LookRotation(direction.normalized);
            Vector3 targetEuler = startRot.eulerAngles;
            targetEuler.y = lookRot.eulerAngles.y;
            Quaternion targetRot = Quaternion.Euler(targetEuler);

            float traveled = 0f;
            float t = 0f;

            while (t < 1f)
            {
                traveled += moveSpeed * Time.deltaTime;
                t = Mathf.Clamp01(traveled / distance);

                Vector3 pos = Vector3.Lerp(start, end, t);
                float arc = Mathf.Sin(Mathf.PI * t) * arcHeight;
                pos.y += arc;

                transform.rotation = Quaternion.Slerp(startRot, targetRot, t * 3);

                transform.position = pos;

                //Cancel
                if (EmeraldComponent.AnimationComponent.IsStunned || EmeraldComponent.AnimationComponent.IsDead)
                {
                    if (EmeraldComponent.m_NavMeshAgent.enabled)
                    {
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.transform.position;
                        EmeraldComponent.m_NavMeshAgent.isStopped = false;
                        EmeraldComponent.m_NavMeshAgent.CompleteOffMeshLink();
                    }

                    EmeraldComponent.m_NavMeshAgent.updatePosition = true;
                    EmeraldComponent.m_NavMeshAgent.updateRotation = true;
                    yield break;
                }

                yield return null;
            }

            //Trigger land animation
            EmeraldComponent.AIAnimator.SetTrigger("Land");

            //Play the land sound
            EmeraldComponent.SoundComponent.PlayLandSound();

            //Re-enable the agent
            if (EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.Warp(end);
                EmeraldComponent.m_NavMeshAgent.isStopped = false;
                EmeraldComponent.m_NavMeshAgent.CompleteOffMeshLink();
                EmeraldComponent.m_NavMeshAgent.destination = CurrentDestination;
            }

            EmeraldComponent.m_NavMeshAgent.updatePosition = true;
            EmeraldComponent.m_NavMeshAgent.updateRotation = true;
            yield return new WaitUntil(() => EmeraldComponent.m_NavMeshAgent.enabled);
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
            yield return new WaitForSeconds(0.5f); //Cooldown
        }
    }
}