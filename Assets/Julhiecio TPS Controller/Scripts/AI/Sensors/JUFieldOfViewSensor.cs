using System.Collections.Generic;
using JUTPS;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace JU.CharacterSystem.AI
{
    /// <summary>
    /// A field of view sensor for AI characters.
    /// </summary>
    [System.Serializable]
    public class FieldOfView
    {
        private JUCharacterAIBase _ai;

        private float _scanTimer;
        private Transform _pivot;
        private Collider[] _detections;

        /// <summary>
        /// If true, the field of view can find colliders.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// The max view distance.
        /// </summary>
        public float Distance;

        /// <summary>
        /// The max view angle.
        /// </summary>
        [Range(1, 180)]
        public float Angle;

        /// <summary>
        /// The view refresh rate.
        /// </summary>
        [Min(0.1f), Space]
        public float RefreshRate;

        /// <summary>
        /// Max raw count of objects that can be detected per update (without any filter like obstacle check or tag check). 
        /// If the AI must found only a object (like the player) set it to 1. If the AI must find any character, set it to 10 or more.
        /// </summary>
        [Min(1)]
        public int MaxDetections;

        /// <summary>
        /// The targets layer.
        /// </summary>
        public LayerMask TargetsLayer;

        /// <summary>
        /// The obstacles, like walls or buildings.
        /// </summary>
        public LayerMask ObstaclesLayer;

        /// <summary>
        /// Used to filter the search using gameObject tags.
        /// </summary>
        public string[] TargetTags;

        /// <summary>
        /// The nearest collider found by the field of view.
        /// </summary>
        public Collider NearestColliderInView { get; private set; }

        /// <summary>
        /// The last object viewed position.
        /// </summary>
        public Vector3 LastColliderViewedPosition { get; private set; }

        /// <summary>
        /// All colliders found by the field of view.
        /// </summary>
        public ReadOnlyArray<Collider> CollidersInView
        {
            get => new ReadOnlyArray<Collider>(_detections);
        }

        /// <summary>
        /// Return true if is viewing a object.
        /// </summary>
        public bool HasCollidersInView
        {
            get => NearestColliderInView != null;
        }

        /// <summary>
        /// Return the field of view position, <see cref="_pivot"/> if is assigned. If not, return the AI bounds center.
        /// </summary>
        public Vector3 Center
        {
            get => _pivot ? _pivot.position : _ai.Center;
        }

        /// <summary>
        /// The field of view forward direction.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                if (_pivot)
                    return Vector3.ProjectOnPlane(_pivot.forward, Vector3.up);

#if UNITY_EDITOR
                Debug.Assert(_ai, $"{nameof(JUCharacterAIBase)} component not added to gameObject.");
                Debug.Assert(_ai.Character, $"{nameof(JUCharacterController)} component not added to gameObject {_ai.name}.");

                if (!Application.isPlaying)
                    return _ai.transform.forward;
#endif

                Vector3 lookAtDirection = _ai.Character.LookAtPosition - _ai.transform.position;
                if (lookAtDirection.magnitude > 0.1f)
                    return Vector3.ProjectOnPlane(lookAtDirection, Vector3.up).normalized;

                return _ai.transform.forward;
            }
        }

        /// <summary>
        /// Create a field of view.
        /// </summary>
        public FieldOfView()
        {
            Enabled = true;
            RefreshRate = 0.5f;
            MaxDetections = 10;

            Distance = 20;
            Angle = 90;

            MaxDetections = 10;
            ObstaclesLayer = 0;
        }

        /// <summary>
        /// Called by editor to reset script properties.
        /// </summary>
        public void Reset()
        {
#if UNITY_EDITOR

            LayerMask[] defaultObstacleLayers = {
                LayerMask.NameToLayer("Default"),
                LayerMask.NameToLayer("Wall"),
                LayerMask.NameToLayer("Walls"),
                LayerMask.NameToLayer("Obstacle"),
                LayerMask.NameToLayer("Obstacles"),
                LayerMask.NameToLayer("Terrain")
            };

            LayerMask[] defaultTargetLayers = {
                LayerMask.NameToLayer("Character"),
                LayerMask.NameToLayer("Characters"),
                LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Players"),
                LayerMask.NameToLayer("Vehicle"),
                LayerMask.NameToLayer("Vehicles")
            };

            string[] defaultTargetTags = {
                "Player",
                "Players",
                "Character",
                "Characters",
                "Vehicle",
                "Vehicles",
                "Distractable",
                "Distractables"
            };

            // Setup default obstacle layers.
            ObstaclesLayer = 0;
            for (int i = 0; i < defaultObstacleLayers.Length; i++)
            {
                if (defaultObstacleLayers[i] != -1)
                    ObstaclesLayer |= 1 << defaultObstacleLayers[i];
            }

            // Setup default target layers.
            TargetsLayer = 0;
            for (int i = 0; i < defaultTargetLayers.Length; i++)
            {
                if (defaultTargetLayers[i] != -1)
                    TargetsLayer |= 1 << defaultTargetLayers[i];
            }

            // Setup default target tags.
            List<string> existentDefaultTags = new List<string>();
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                for (int i = 0; i < defaultTargetTags.Length; i++)
                {
                    if (tag.Equals(defaultTargetTags[i]))
                        existentDefaultTags.Add(tag);
                }
            }

            TargetTags = existentDefaultTags.ToArray();
#endif
        }

        /// <summary>
        /// Setup the field of view.
        /// </summary>
        /// <param name="ai">The AI character that will have the field of view.</param>
        public void Setup(JUCharacterAIBase ai)
        {
            _ai = ai;
            _detections = new Collider[MaxDetections + 1];
        }

        /// <summary>
        ///  Update the field of view.
        /// </summary>
        /// <param name="pivot">The field of view base, can be the "head" of a character.</param>
        public void Update(Transform pivot)
        {
            if (!Enabled)
            {
                NearestColliderInView = null;
                return;
            }

            _pivot = pivot;

            _scanTimer += Time.deltaTime;
            if (_scanTimer > RefreshRate)
            {
                _scanTimer = 0;
                Scan();
            }
        }

        private void Scan()
        {
            NearestColliderInView = null;

            Vector3 center = Center;
            Vector3 forward = Forward;
            int foundCount = Physics.OverlapSphereNonAlloc(center, Distance, _detections, TargetsLayer);

            if (foundCount < 1)
                return;

            for (int i = 0; i < foundCount; i++)
            {
                Collider collider = _detections[i];

                // Remove the self collider if was detected.
                if (collider.gameObject == _ai.gameObject)
                {
                    _detections[i] = null;
                    continue;
                }

                Vector3 colliderCenter = collider.bounds.center;

                // Remove colliders that is outside camera view.
                if (Vector3.Angle(forward, colliderCenter - center) > Angle)
                {
                    _detections[i] = null;
                    continue;
                }

                // Remove colliders that not have the correct tag.
                if (TargetTags.Length > 0)
                {
                    bool hasTag = false;
                    for (int x = 0; x < TargetTags.Length; x++)
                    {
                        if (collider.CompareTag(TargetTags[x]))
                        {
                            hasTag = true;
                            break;
                        }
                    }

                    if (!hasTag)
                    {
                        _detections[i] = null;
                        continue;
                    }
                }

                // Remove colliders that have obstacles in front.
                if (Physics.Linecast(center, colliderCenter, out RaycastHit hit, ObstaclesLayer, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider != collider)
                    {
                        _detections[i] = null;
                        continue;
                    }
                }
            }

            // Find the nearest.
            NearestColliderInView = null;

            float minDistance = float.MaxValue;
            for (int i = 0; i < foundCount; i++)
            {
                if (!_detections[i])
                    continue;

                Vector3 colliderCenter = _detections[i].bounds.center;
                float distance = Vector3.Distance(center, colliderCenter);
                if (distance < minDistance)
                {
                    // Ignore death objects.
                    if (_detections[i].TryGetComponent(out JUHealth health))
                    {
                        if (health.IsDead)
                            continue;
                    }

                    minDistance = distance;
                    NearestColliderInView = _detections[i];
                    LastColliderViewedPosition = colliderCenter;
                }
            }
        }

        /// <summary>
        /// Return true if a transform is on field of view.
        /// </summary>
        /// <param name="otherTransform"></param>
        /// <returns></returns>
        public bool IsOnView(Transform otherTransform)
        {
            if (!otherTransform)
                return false;

            if (TargetTags.Length > 0)
            {
                bool hasTag = false;
                for (int x = 0; x < TargetTags.Length; x++)
                {
                    if (otherTransform.CompareTag(TargetTags[x]))
                    {
                        hasTag = true;
                        break;
                    }
                }

                if (!hasTag)
                    return false;
            }

            Vector3 center = Center;
            Vector3 otherTransformPosition = otherTransform.position;

            if (Vector3.Angle(_ai.transform.forward, otherTransformPosition - center) > Angle)
                return false;

            if (Physics.Linecast(center, otherTransformPosition, out RaycastHit hit, ObstaclesLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != otherTransform)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if a collider is on field of view.
        /// </summary>
        /// <param name="otherCollider"></param>
        /// <returns></returns>
        public bool IsOnView(Collider otherCollider)
        {
            if (!otherCollider)
                return false;

            if (TargetTags.Length > 0)
            {
                bool hasTag = false;
                for (int x = 0; x < TargetTags.Length; x++)
                {
                    if (otherCollider.CompareTag(TargetTags[x]))
                    {
                        hasTag = true;
                        break;
                    }
                }

                if (!hasTag)
                    return false;
            }

            Vector3 center = Center;
            Vector3 otherColliderPosition = otherCollider.bounds.center;

            if (Vector3.Angle(_ai.transform.forward, otherColliderPosition - center) > Angle)
                return false;

            if (Physics.Linecast(center, otherColliderPosition, out RaycastHit hit, ObstaclesLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != otherCollider)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if a bounds is on field of view based on bounds center.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public bool IsOnView(Bounds bounds)
        {
            return IsOnView(bounds.center);
        }

        /// <summary>
        /// Return true if a point is on field of view.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsOnView(Vector3 point)
        {
            return IsOnView(Center, _ai.transform.forward, point);
        }

        /// <summary>
        /// Return true if a point is on field of view.
        /// </summary>
        /// <param name="center">The field of view center.</param>
        /// <param name="forward">The field of view direction</param>
        /// <param name="point">The point to check.</param>
        /// <returns></returns>
        public bool IsOnView(Vector3 center, Vector3 forward, Vector3 point)
        {
            if (Vector3.Angle(forward, point - center) > Angle)
                return false;


            if (Physics.Linecast(center, point, ObstaclesLayer, QueryTriggerInteraction.Ignore))
                return false;

            return true;
        }

        /// <summary>
        /// Draw the field of view.
        /// </summary>
        public void DrawGizmos()
        {
#if UNITY_EDITOR
            if (!_ai)
                return;

            Vector3 position = Center;
            Vector3 forward = Forward;
            Vector3 up = Quaternion.LookRotation(forward) * Vector3.up;

            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, up, Distance);

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireArc(position, up, forward, Angle, Distance - 0.1f);
            UnityEditor.Handles.DrawWireArc(position, up, forward, -Angle, Distance - 0.1f);

            UnityEditor.Handles.color = new Color(1, 0, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(position, up, forward, Angle, Distance - 0.2f);
            UnityEditor.Handles.DrawSolidArc(position, up, forward, -Angle, Distance - 0.2f);
#endif
        }
    }
}