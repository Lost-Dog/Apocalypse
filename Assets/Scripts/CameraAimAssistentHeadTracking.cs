using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS;
using JU.CharacterSystem.AI;

namespace JUTPS.CameraSystems
{
    [AddComponentMenu("JU TPS/Cameras/Aim Assistant System/JU Aim Assistent (Head Tracking)")]
    public class CameraAimAssistentHeadTracking : MonoBehaviour
    {
        [System.Serializable]
        public class TargetTagOffset
        {
            public string Tag = "Enemy";
            public float UpOffset;
            public TargetTagOffset(string tag, float upOffset)
            {
                Tag = tag;
                UpOffset = upOffset;
            }
            public static float GetUpOffset(TargetTagOffset[] targetTagList, GameObject objectTag)
            {
                if (objectTag == null || targetTagList == null) return 0;

                foreach (TargetTagOffset tag in targetTagList)
                {
                    if (tag.Tag == objectTag.tag)
                    {
                        return tag.UpOffset;
                    }
                }

                return 0;
            }
        }

        private JUCameraController targetCamera;

        [Header("Detection Settings")]
        public float DistanceToDetect = 50;
        public float AssistentForce = 3;
        public LayerMask TargetLayer;
        public TargetTagOffset[] TargetsTagsAndOffsets = new[] { new TargetTagOffset("Enemy", 1) };

        [Header("Head Tracking")]
        [Tooltip("Automatically aim at enemy head transform instead of using UpOffset")]
        public bool trackHeadTransform = true;
        
        [Tooltip("Fallback UpOffset if head transform not found")]
        public float fallbackHeadOffset = 1.5f;

        [Header("Debug")]
        public bool showDebugRays = false;

        private float UpOffset => TargetTagOffset.GetUpOffset(TargetsTagsAndOffsets, ObjectInCameraCenter);
        private string[] AllTags;
        private GameObject ObjectInCameraCenter;

        void Start()
        {
            targetCamera = GetComponent<JUCameraController>();

            List<string> taglist = new List<string>();
            foreach (TargetTagOffset tag in TargetsTagsAndOffsets)
            {
                taglist.Add(tag.Tag);
            }
            AllTags = taglist.ToArray();
        }

        void Update()
        {
            ObjectInCameraCenter = targetCamera.GetObjectOnCameraCenter(DistanceToDetect, TargetLayer);
            if (ObjectInCameraCenter == null) return;

            for (int i = 0; i < AllTags.Length; i++)
            {
                if (ObjectInCameraCenter.CompareTag(AllTags[i]))
                {
                    Vector3 targetPosition = GetAimPosition(ObjectInCameraCenter);
                    
                    if (showDebugRays)
                    {
                        Debug.DrawLine(targetCamera.mCamera.transform.position, targetPosition, Color.red);
                    }

                    Vector3 TargetRotationEuler = Quaternion.LookRotation((targetPosition - targetCamera.mCamera.transform.position).normalized).eulerAngles;

                    targetCamera.rotytarget = Mathf.LerpAngle(targetCamera.rotytarget, TargetRotationEuler.y, AssistentForce * Time.deltaTime);
                    targetCamera.rotxtarget = Mathf.LerpAngle(targetCamera.rotxtarget, TargetRotationEuler.x, AssistentForce * Time.deltaTime);
                    break;
                }
            }
        }

        private Vector3 GetAimPosition(GameObject target)
        {
            if (!trackHeadTransform)
            {
                // Use original UpOffset method
                return target.transform.position + transform.up * UpOffset;
            }

            // Try to find head transform
            Transform headTransform = FindHeadTransform(target);
            
            if (headTransform != null)
            {
                // Aim at actual head position
                return headTransform.position;
            }
            else
            {
                // Fallback to offset method
                return target.transform.position + transform.up * fallbackHeadOffset;
            }
        }

        private Transform FindHeadTransform(GameObject target)
        {
            // Method 1: Check for JU AI components that have Head reference
            JU_AI_PatrolCharacter patrolAI = target.GetComponent<JU_AI_PatrolCharacter>();
            if (patrolAI != null && patrolAI.Head != null)
            {
                return patrolAI.Head;
            }

            JU_AI_Zombie zombieAI = target.GetComponent<JU_AI_Zombie>();
            if (zombieAI != null && zombieAI.Head != null)
            {
                return zombieAI.Head;
            }

            // Method 2: Try to find via Animator
            Animator animator = target.GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null)
                {
                    return head;
                }
            }

            // Method 3: Search for common head bone names
            Transform headByName = FindChildByName(target.transform, "Head");
            if (headByName != null)
            {
                return headByName;
            }

            headByName = FindChildByName(target.transform, "head");
            if (headByName != null)
            {
                return headByName;
            }

            // Method 4: Try "mixamorig:Head" for Mixamo rigs
            headByName = FindChildByName(target.transform, "mixamorig:Head");
            if (headByName != null)
            {
                return headByName;
            }

            return null;
        }

        private Transform FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the current target being aimed at
        /// </summary>
        public GameObject GetCurrentTarget()
        {
            return ObjectInCameraCenter;
        }

        /// <summary>
        /// Get the current aim position (including head tracking)
        /// </summary>
        public Vector3 GetCurrentAimPosition()
        {
            if (ObjectInCameraCenter == null) return Vector3.zero;
            return GetAimPosition(ObjectInCameraCenter);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugRays || ObjectInCameraCenter == null) return;

            Vector3 aimPos = GetAimPosition(ObjectInCameraCenter);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimPos, 0.2f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, aimPos);
        }
    }
}
