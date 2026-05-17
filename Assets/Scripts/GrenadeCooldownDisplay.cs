using UnityEngine;
using UnityEngine.UI;

namespace Apocalypse
{
    /// <summary>
    /// Drives the grenade slot's cooldown visuals without polling.
    ///
    /// On cooldown start  → creates a scrolling gradient overlay on the skill slot
    ///                       and pulses the Selector image colour left-to-right.
    /// On cooldown end    → restores all visuals and disables per-frame work.
    ///
    /// Requires <see cref="vThrowManagerObservable"/> anywhere in the scene.
    /// </summary>
    public class GrenadeCooldownDisplay : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("References")]
        [Tooltip("The Image used as the 'selected' indicator (Selector > Image). " +
                 "Its colour will pulse during cooldown.")]
        public Image selectorImage;

        [Tooltip("Player root that owns vThrowManagerObservable.")]
        public GameObject throwManagerRoot;

        [Header("Sweep Overlay")]
        [Tooltip("Tint colour of the left-to-right sweep gradient. Alpha is modulated by the pulse.")]
        public Color sweepColor = new Color(1f, 0.45f, 0f, 0.65f);

        [Tooltip("How fast the streak travels left-to-right (UV units per second).")]
        public float sweepSpeed = 0.6f;

        [Header("Pulse")]
        [Tooltip("Oscillations per second for the colour pulse.")]
        public float pulseFrequency = 1.4f;

        [Tooltip("Minimum alpha multiplier at the trough of the pulse (0 = fully invisible).")]
        [Range(0f, 1f)]
        public float pulseMinAlpha = 0.15f;

        [Tooltip("Colour the Selector image lerps toward at the peak of each pulse.")]
        public Color cooldownPeakColor = new Color(1f, 0.3f, 0f, 1f);

        // ── Private state ────────────────────────────────────────────────────────

        private vThrowManagerObservable _throwManager;
        private RawImage  _sweepOverlay;
        private Texture2D _sweepTexture;
        private Color     _selectorBaseColor;
        private bool      _isCoolingDown;
        private float     _scrollOffset;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Start()
        {
            _sweepTexture = BuildSweepTexture();
            _sweepOverlay = BuildOverlay();

            if (selectorImage != null)
                _selectorBaseColor = selectorImage.color;

            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (_sweepTexture != null)
                Destroy(_sweepTexture);
        }

        private void Update()
        {
            if (!_isCoolingDown || _sweepOverlay == null)
                return;

            // Advance the scroll position so the bright streak moves left → right.
            _scrollOffset += sweepSpeed * Time.deltaTime;
            _sweepOverlay.uvRect = new Rect(_scrollOffset, 0f, 1f, 1f);

            // Compute a [0..1] pulse value using a sine wave.
            float pulse = Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(pulseMinAlpha, 1f, pulse);

            // Apply pulsing alpha to the overlay.
            Color oc = sweepColor;
            _sweepOverlay.color = new Color(oc.r, oc.g, oc.b, oc.a * alpha);

            // Lerp the selector image between its normal and cooldown peak colours.
            if (selectorImage != null)
                selectorImage.color = Color.Lerp(_selectorBaseColor, cooldownPeakColor, pulse);
        }

        // ── Event callbacks ──────────────────────────────────────────────────────

        private void OnCooldownStarted()
        {
            _isCoolingDown = true;
            _scrollOffset  = 0f;

            if (_sweepOverlay != null)
                _sweepOverlay.gameObject.SetActive(true);
        }

        private void OnCooldownEnded()
        {
            _isCoolingDown = false;

            if (_sweepOverlay != null)
                _sweepOverlay.gameObject.SetActive(false);

            if (selectorImage != null)
                selectorImage.color = _selectorBaseColor;
        }

        // ── Setup helpers ────────────────────────────────────────────────────────

        private void Subscribe()
        {
            _throwManager = ResolveThrowManager();
            if (_throwManager == null) return;

            _throwManager.onCooldownStarted += OnCooldownStarted;
            _throwManager.onCooldownEnded   += OnCooldownEnded;

            // If we are already mid-cooldown when this component starts (e.g. scene reload),
            // sync immediately.
            if (_throwManager.IsCoolingDown)
                OnCooldownStarted();
        }

        private void Unsubscribe()
        {
            if (_throwManager == null) return;

            _throwManager.onCooldownStarted -= OnCooldownStarted;
            _throwManager.onCooldownEnded   -= OnCooldownEnded;
        }

        private vThrowManagerObservable ResolveThrowManager()
        {
            vThrowManagerObservable result = null;

            if (throwManagerRoot != null)
                result = throwManagerRoot.GetComponentInChildren<vThrowManagerObservable>(includeInactive: true);

            if (result == null)
                result = FindFirstObjectByType<vThrowManagerObservable>();

            if (result == null)
                Debug.LogWarning("[GrenadeCooldownDisplay] vThrowManagerObservable not found.");

            return result;
        }

        /// <summary>
        /// Builds the RawImage overlay that covers the entire vThrowUI slot area.
        /// Inactive by default — only shown during cooldown.
        /// </summary>
        private RawImage BuildOverlay()
        {
            var go = new GameObject("CooldownSweep", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(transform, false);

            // Stretch to fill parent.
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.sizeDelta      = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            var raw = go.GetComponent<RawImage>();
            raw.texture       = _sweepTexture;
            raw.color         = sweepColor;
            raw.raycastTarget = false;

            go.SetActive(false);
            return raw;
        }

        /// <summary>
        /// Procedural 128×1 texture: a repeating bright streak with transparent flanks.
        /// Wraps seamlessly so scrolling the uvRect.x produces a continuous leftward sweep.
        /// </summary>
        private static Texture2D BuildSweepTexture()
        {
            const int width = 128;

            var tex = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode   = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (int x = 0; x < width; x++)
            {
                // Normalised position [0..1] within the texture.
                float t = (float)x / width;

                // Place one bell-curve streak occupying roughly the first 40% of the tile;
                // the rest stays transparent so streaks are visually separated when scrolling.
                float streak = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI / 0.4f));
                if (t > 0.4f) streak = 0f;

                tex.SetPixel(x, 0, new Color(1f, 1f, 1f, streak));
            }

            tex.Apply();
            return tex;
        }
    }
}
