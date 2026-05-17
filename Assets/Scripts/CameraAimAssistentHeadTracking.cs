using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

[AddComponentMenu("Apocalypse/Camera/Aim Assistant (Chest Tracking)")]
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

        [Header("Activation")]
        [Tooltip("When false the assist is completely skipped each frame. " +
                 "Toggle via SetAssistActive() from InvectorInputBridge.")]
        public bool assistActive = false;

        [Header("Detection Settings")]
        public float DistanceToDetect = 50;
        public float AssistentForce = 3;
        public LayerMask TargetLayer;
        public TargetTagOffset[] TargetsTagsAndOffsets = new[] { new TargetTagOffset("Enemy", 1) };

        [Tooltip("Half-angle of the detection cone in degrees. Enemies within this angle of the " +
                 "camera forward are candidates; the one closest to the crosshair is chosen. " +
                 "0 = pixel-perfect raycast only.")]
        [Range(0f, 45f)]
        public float assistAngle = 15f;

        [Header("Chest Tracking")]
        [Tooltip("Automatically aim at the target's chest bone instead of using UpOffset.")]
        public bool trackChestTransform = true;

        [Tooltip("Fallback UpOffset used when the chest bone cannot be found.")]
        public float fallbackChestOffset = 1.0f;

        [Tooltip("How far (metres) behind the chest the correction vector aims. " +
                 "0 = chest surface. Positive values pull the camera to aim through the body, " +
                 "making the assist stickier without changing where the bullet actually hits " +
                 "(Invector's raycast still stops at the first collider surface).")]
        [Range(0f, 1f)]
        public float penetrationDepth = 0.3f;

        [Header("Debug")]
        public bool showDebugRays = false;

        private UnityEngine.Camera _activeCamera;
        private float UpOffset => TargetTagOffset.GetUpOffset(TargetsTagsAndOffsets, _objectInCameraCenter);
        private string[] _allTags;
        private GameObject _objectInCameraCenter;

        // Whether a valid target was found this frame.
        private bool _hasTarget;

        // Pixel offset of the target from screen centre (positive X = target is right of centre,
        // positive Y = target is above centre). Consumed by InvectorInputBridge.
        private Vector2 _screenOffset;

        public GameObject ObjectInCameraCenter => _objectInCameraCenter;

        /// <summary>True when a target is visible on screen and <see cref="ScreenOffset"/> is valid.</summary>
        public bool HasTarget => _hasTarget;

        /// <summary>
        /// Pixel distance of the target chest from the screen centre this frame.
        /// X positive = target is right of crosshair, Y positive = target is above crosshair.
        /// InvectorInputBridge converts this to angular corrections for _mouseX/_mouseY.
        /// </summary>
        public Vector2 ScreenOffset => _screenOffset;

        private void Start()
        {
            _activeCamera = UnityEngine.Camera.main;
            if (_activeCamera == null)
                Debug.LogWarning("[CameraAimAssistentChestTracking] No main camera found in the scene.");

            var tagList = new List<string>();
            foreach (TargetTagOffset tag in TargetsTagsAndOffsets)
                tagList.Add(tag.Tag);

            _allTags = tagList.ToArray();
        }

        private void Update()
        {
            _hasTarget    = false;
            _screenOffset = Vector2.zero;

            if (!assistActive || _activeCamera == null) return;

            _objectInCameraCenter = GetObjectOnCameraCenter();
            if (_objectInCameraCenter == null) return;

            for (int i = 0; i < _allTags.Length; i++)
            {
                if (!_objectInCameraCenter.CompareTag(_allTags[i])) continue;

                Vector3 targetPosition = GetAimPosition(_objectInCameraCenter);

                // Project the target into screen space.
                Vector3 screenPos = _activeCamera.WorldToScreenPoint(targetPosition);

                // Discard targets behind the camera.
                if (screenPos.z <= 0f) break;

                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                _screenOffset = new Vector2(screenPos.x - screenCenter.x,
                                            screenPos.y - screenCenter.y);
                _hasTarget = true;

                if (showDebugRays)
                    Debug.DrawLine(_activeCamera.transform.position, targetPosition, Color.red);

                break;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Enables or disables the aim assist. Called by InvectorInputBridge when
        /// the player enters or leaves aim mode.
        /// </summary>
        public void SetAssistActive(bool active)
        {
            assistActive = active;

            if (!active)
            {
                _objectInCameraCenter = null;
                _hasTarget = false;
            }
        }

        /// <summary>Returns the world-space aim position for the current target.</summary>
        public Vector3 GetCurrentAimPosition()
        {
            if (_objectInCameraCenter == null) return Vector3.zero;
            return GetAimPosition(_objectInCameraCenter);
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the best candidate in the detection cone.
        /// When <see cref="assistAngle"/> is 0 a pixel-perfect centre-screen raycast is used.
        /// Otherwise every collider on <see cref="TargetLayer"/> within <see cref="DistanceToDetect"/>
        /// is tested; the one whose root GameObject matches a tracked tag AND sits closest to the
        /// camera's forward axis (smallest angle) wins.
        /// </summary>
        private GameObject GetObjectOnCameraCenter()
        {
            // ── Pixel-perfect fallback (assistAngle == 0) ─────────────────────
            if (assistAngle <= 0f)
            {
                Ray ray = _activeCamera.ScreenPointToRay(
                    new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

                return Physics.Raycast(ray, out RaycastHit hit, DistanceToDetect, TargetLayer)
                    ? hit.collider.gameObject
                    : null;
            }

            // ── Cone search ───────────────────────────────────────────────────
            Vector3 camPos     = _activeCamera.transform.position;
            Vector3 camForward = _activeCamera.transform.forward;

            Collider[] hits = Physics.OverlapSphere(camPos, DistanceToDetect, TargetLayer);

            GameObject bestTarget   = null;
            float      bestAngle    = assistAngle; // only accept candidates inside the cone

            foreach (Collider col in hits)
            {
                GameObject root = col.attachedRigidbody != null
                    ? col.attachedRigidbody.gameObject
                    : col.gameObject;

                // Must match one of the tracked tags.
                bool tagMatch = false;
                foreach (string tag in _allTags)
                {
                    if (root.CompareTag(tag)) { tagMatch = true; break; }
                }
                if (!tagMatch) continue;

                // Angle from camera forward to the collider centre.
                Vector3 toTarget = (col.bounds.center - camPos).normalized;
                float   angle    = Vector3.Angle(camForward, toTarget);

                if (angle < bestAngle)
                {
                    bestAngle  = angle;
                    bestTarget = root;
                }
            }

            return bestTarget;
        }

        private Vector3 GetAimPosition(GameObject target)
        {
            Vector3 basePosition;

            if (!trackChestTransform)
            {
                basePosition = target.transform.position + Vector3.up * UpOffset;
            }
            else
            {
                Transform chestTransform = FindChestTransform(target);
                basePosition = chestTransform != null
                    ? chestTransform.position
                    : target.transform.position + Vector3.up * fallbackChestOffset;
            }

            if (penetrationDepth <= 0f || _activeCamera == null)
                return basePosition;

            // Offset the aim target behind the chest surface along the camera-to-chest vector,
            // so the correction always pulls the crosshair into the body rather than toward its front face.
            Vector3 camToBase = (basePosition - _activeCamera.transform.position).normalized;
            return basePosition + camToBase * penetrationDepth;
        }

        /// <summary>
        /// Resolves the chest / spine bone of a target. Prefers the Invector animator,
        /// then a direct Animator search, then common bone name lookups.
        /// </summary>
        private Transform FindChestTransform(GameObject target)
        {
            // Method 1: Invector vThirdPersonController.
            vThirdPersonController invectorController = target.GetComponent<vThirdPersonController>();
            if (invectorController != null)
            {
                Animator characterAnimator = invectorController.GetComponentInChildren<Animator>();
                if (characterAnimator != null && characterAnimator.isHuman)
                {
                    // Prefer UpperChest, fall back to Chest, then Spine.
                    Transform bone = characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest)
                                  ?? characterAnimator.GetBoneTransform(HumanBodyBones.Chest)
                                  ?? characterAnimator.GetBoneTransform(HumanBodyBones.Spine);
                    if (bone != null) return bone;
                }
            }

            // Method 2: Direct Animator (non-Invector or legacy).
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
                animator = target.GetComponentInChildren<Animator>();

            if (animator != null && animator.isHuman)
            {
                Transform bone = animator.GetBoneTransform(HumanBodyBones.UpperChest)
                              ?? animator.GetBoneTransform(HumanBodyBones.Chest)
                              ?? animator.GetBoneTransform(HumanBodyBones.Spine);
                if (bone != null) return bone;
            }

            // Method 3: Common bone name variants.
            return FindChildByName(target.transform, "Chest")
                ?? FindChildByName(target.transform, "chest")
                ?? FindChildByName(target.transform, "UpperChest")
                ?? FindChildByName(target.transform, "Spine1")
                ?? FindChildByName(target.transform, "mixamorig:Spine1")
                ?? FindChildByName(target.transform, "mixamorig:Spine2");
        }

        /// <summary>Searches all descendants for a Transform with the exact given name.</summary>
        private Transform FindChildByName(Transform parent, string boneName)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == boneName) return child;
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugRays || _objectInCameraCenter == null) return;

            Vector3 aimPos = GetAimPosition(_objectInCameraCenter);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimPos, 0.2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, aimPos);
        }
}
