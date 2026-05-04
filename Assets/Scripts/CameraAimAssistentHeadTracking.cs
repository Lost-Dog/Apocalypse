using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

namespace RVR.Camera
{
    [AddComponentMenu("RVR/Camera/Aim Assistant (Head Tracking)")]
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
                        return tag.UpOffset;
                }

                return 0;
            }
        }

        [Header("Detection Settings")]
        public float DistanceToDetect = 50;
        public float AssistentForce = 3;
        public LayerMask TargetLayer;
        public TargetTagOffset[] TargetsTagsAndOffsets = new[] { new TargetTagOffset("Enemy", 1) };

        [Header("Head Tracking")]
        [Tooltip("Automatically aim at the target's head transform instead of using UpOffset")]
        public bool trackHeadTransform = true;

        [Tooltip("Fallback UpOffset if head transform is not found")]
        public float fallbackHeadOffset = 1.5f;

        [Header("Debug")]
        public bool showDebugRays = false;

        private UnityEngine.Camera activeCamera;
        private float UpOffset => TargetTagOffset.GetUpOffset(TargetsTagsAndOffsets, ObjectInCameraCenter);
        private string[] AllTags;
        private GameObject ObjectInCameraCenter;

        private void Start()
        {
            // Resolve the active camera via GC2 shortcut, falling back to Camera.main
            activeCamera = ShortcutMainCamera.Get<UnityEngine.Camera>();
            if (activeCamera == null)
                activeCamera = UnityEngine.Camera.main;

            var tagList = new List<string>();
            foreach (TargetTagOffset tag in TargetsTagsAndOffsets)
                tagList.Add(tag.Tag);

            AllTags = tagList.ToArray();
        }

        private void Update()
        {
            if (activeCamera == null) return;

            ObjectInCameraCenter = GetObjectOnCameraCenter();
            if (ObjectInCameraCenter == null) return;

            for (int i = 0; i < AllTags.Length; i++)
            {
                if (!ObjectInCameraCenter.CompareTag(AllTags[i])) continue;

                Vector3 targetPosition = GetAimPosition(ObjectInCameraCenter);

                if (showDebugRays)
                    Debug.DrawLine(activeCamera.transform.position, targetPosition, Color.red);

                // Steer the camera transform toward the aim position.
                // GC2's camera shot system writes position/rotation in LateUpdate, so we nudge
                // the transform here (Update) which the shot system will blend from each frame.
                Vector3 toTarget = (targetPosition - activeCamera.transform.position).normalized;
                Quaternion desiredRotation = Quaternion.LookRotation(toTarget);

                activeCamera.transform.rotation = Quaternion.Slerp(
                    activeCamera.transform.rotation,
                    desiredRotation,
                    AssistentForce * Time.deltaTime
                );
                break;
            }
        }

        /// <summary>
        /// Casts a ray from the centre of the screen and returns the first hit object
        /// within <see cref="DistanceToDetect"/> on <see cref="TargetLayer"/>.
        /// </summary>
        private GameObject GetObjectOnCameraCenter()
        {
            Ray ray = activeCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, DistanceToDetect, TargetLayer))
                return hit.collider.gameObject;

            return null;
        }

        private Vector3 GetAimPosition(GameObject target)
        {
            if (!trackHeadTransform)
                return target.transform.position + Vector3.up * UpOffset;

            Transform headTransform = FindHeadTransform(target);

            return headTransform != null
                ? headTransform.position
                : target.transform.position + Vector3.up * fallbackHeadOffset;
        }

        /// <summary>
        /// Resolves the head bone of a target, preferring GC2 Character's animator,
        /// then falling back to a direct Animator search and common bone name lookups.
        /// </summary>
        private Transform FindHeadTransform(GameObject target)
        {
            // Method 1: GC2 Character — get head bone from the model's Animator
            Character gc2Character = target.GetComponent<Character>();
            if (gc2Character != null)
            {
                Animator characterAnimator = gc2Character.GetComponentInChildren<Animator>();
                if (characterAnimator != null && characterAnimator.isHuman)
                {
                    Transform head = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                    if (head != null) return head;
                }
            }

            // Method 2: Direct Animator on the target (non-GC2 characters or legacy setups)
            Animator animator = target.GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) return head;
            }

            // Method 3: Common head bone name variants
            Transform headByName = FindChildByName(target.transform, "Head")
                                ?? FindChildByName(target.transform, "head")
                                ?? FindChildByName(target.transform, "mixamorig:Head");

            return headByName;
        }

        /// <summary>
        /// Searches all descendants for a Transform with the exact given name.
        /// </summary>
        private Transform FindChildByName(Transform parent, string boneName)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == boneName) return child;
            }
            return null;
        }

        /// <summary>
        /// Returns the GameObject currently centered in the camera's aim.
        /// </summary>
        public GameObject GetCurrentTarget() => ObjectInCameraCenter;

        /// <summary>
        /// Returns the world-space aim position for the current target, including head tracking.
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
