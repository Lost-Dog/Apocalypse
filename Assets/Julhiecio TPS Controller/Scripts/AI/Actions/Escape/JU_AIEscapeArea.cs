using System;
using JUTPS;
using UnityEngine;

namespace JU.CharacterSystem.AI.EscapeSystem
{
    /// <summary>
    /// Create an area used by AIs that have <see cref="Escape"/> state action. 
    /// Ais will try avoid the area. Useful to represent explosion areas / grenades.
    /// </summary>
    public class JU_AIEscapeArea : MonoBehaviour
    {
        private Guid _areaId;
        private Bounds _area;
        private Vector3 _lastPosition;
        private Vector3 _latScale;

        /// <summary>
        /// Min movement distance to refresh the area.
        /// </summary>
        public float PositionThreshold;

        /// <summary>
        /// The min scale change to refresh the area.
        /// </summary>
        public float ScaleThreshold;

        /// <summary>
        /// The tag used to AIs identify the area.
        /// </summary>
        public JUTag AreaTag;

        /// <summary>
        /// The area size.
        /// </summary>
        public float Size;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public JU_AIEscapeArea()
        {
            Size = 1;
            PositionThreshold = 2f;
            ScaleThreshold = 1;
        }

        private void OnValidate()
        {
            PositionThreshold = Mathf.Max(PositionThreshold, 0.5f);
            ScaleThreshold = Mathf.Max(ScaleThreshold, 0.01f);
        }

        private void OnEnable()
        {
            _area = default;
            _lastPosition = transform.position;
            _latScale = transform.localScale;
            RefreshArea();
        }

        private void OnDisable()
        {
            Escape.RemoveEscapeArea(_areaId);
            _areaId = default;
        }

        private void Update()
        {
            Vector3 currentPosition = transform.position;
            if (Vector3.Distance(currentPosition, _lastPosition) > PositionThreshold)
            {
                RefreshArea();
                _lastPosition = currentPosition;
            }

            Vector3 currentScale = transform.localScale;
            if (Mathf.Abs(currentScale.magnitude - _latScale.magnitude) > ScaleThreshold)
            {
                RefreshArea();
                _latScale = currentScale;
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 center;
            Vector3 size;

            if (Application.isPlaying)
            {
                center = _area.center;
                size = _area.size;
            }
            else
                CalculateArea(out center, out size);

            Gizmos.color = Color.yellow * 0.5f;
            Gizmos.DrawWireCube(center, size);
        }

        public void RefreshArea()
        {
            CalculateArea(out Vector3 center, out Vector3 size);
            _area = new Bounds(center, size);

            if (_areaId == default)
            {
                Escape.AddEscapeArea(_area, out _areaId, AreaTag);
                return;
            }

            Escape.UpdateEscapeArea(_area, _areaId);
        }

        private void CalculateArea(out Vector3 center, out Vector3 size)
        {
            Vector3 scale = transform.localScale;
            size = Vector3.one * Size * Mathf.Max(scale.x, scale.y, scale.z);
            center = transform.position;
        }
    }
}