using UnityEngine;
using UnityEngine.Rendering;

namespace CompassNavigatorPro {

    [CreateAssetMenu(fileName = "Compass Navigator Pro Scan Profile", menuName = "Compass Navigator Pro Scan Profile", order = 1200)]
    [ExecuteAlways]
    public class ScanProfile : ScriptableObject {

        public Vector3 center;
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color color = new Color(0, 0.5f, 1f, 0.2f);
        public float maxRange = 800;
        public float duration = 2f;
        public float fadeOutStart = 2f;
        public float fadeOutDuration = 1f;
        public float width = 100f;

        public bool grid = true;
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color gridColor = new Color(1, 1, 0);
        public float gridSize = 0.1f;
        public float gridDepthWidthControl = 150;
        public float gridDepthStart = 100;

        public bool edgeOutline = true;
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color edgeColor = Color.white;
        [Range(0.001f, 0.1f)]
        public float edgeDepthThreshold = 0.01f;
        [Range(0.0001f, 0.5f)]
        public float edgeColorThreshold = 0.1f;
        [Range(0f, 1f)]
        public float edgeStrength = 1f;
        public float edgeWidth = 1f;

        public bool filled;
        [ColorUsage(showAlpha: true, hdr: false)]
        public Color colorBlend = new Color(0, 0, 0, 0.75f);

        public bool scanLines = true;
        public bool noise = true;
        public LayerMask hitLayerMask = -1;
        [Tooltip("If true, disabled POIs will also be detected, otherwise only active/enabled POIs will be used in the scan detection")]
        public bool detectDisabledPOIs = true;
        [Tooltip("Audio clip to play when the scan starts")]
        public AudioClip audioClipStart;
        [Tooltip("Audio clip to play when the scan finds a suitable target based on the hit layer mask")]
        public AudioClip audioClipHit;

        void Validate () {
            ValidateSettings();
        }

        public void ValidateSettings () {
            maxRange = Mathf.Max(0, maxRange);
            duration = Mathf.Max(0, duration);
            fadeOutStart = Mathf.Max(0, fadeOutStart);
            fadeOutDuration = Mathf.Max(0, fadeOutDuration);
            width = Mathf.Max(0, width);
            gridSize = Mathf.Max(0.001f, gridSize);
            gridDepthWidthControl = Mathf.Max(1f, gridDepthWidthControl);
            gridDepthStart = Mathf.Max(1f, gridDepthStart);
            edgeWidth = Mathf.Max(0.01f, edgeWidth);
        }
    }
}
