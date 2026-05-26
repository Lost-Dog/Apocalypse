using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Drives a single floating damage number popup in The Division style:
/// punch-in scale, upward drift, then fade out.
/// Returned to pool automatically when the animation completes.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class FloatingDamageText : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Motion")]
    [Tooltip("World-space distance the number rises over its lifetime.")]
    public float riseDistance = 1.8f;

    [Tooltip("Random horizontal spread to prevent stacking on the same spot.")]
    public float horizontalSpread = 0.4f;

    [Tooltip("Total visible lifetime in seconds.")]
    public float lifetime = 1.1f;

    [Tooltip("Fraction of lifetime spent fading out (0-1).")]
    [Range(0.1f, 0.9f)]
    public float fadeFraction = 0.45f;

    [Header("Scale Punch")]
    [Tooltip("Scale multiplier at the peak of the punch-in.")]
    public float punchScale = 1.45f;

    [Tooltip("Fraction of lifetime the punch-in lasts (0-1).")]
    [Range(0.05f, 0.4f)]
    public float punchFraction = 0.18f;

    [Header("Colours")]
    public Color colourNormal    = new Color(1f,    1f,    1f,    1f); // white
    public Color colourCritical  = new Color(1f,    0.78f, 0.1f,  1f); // gold
    public Color colourPlayer    = new Color(1f,    0.22f, 0.1f,  1f); // red
    public Color colourHealing   = new Color(0.3f,  1f,    0.45f, 1f); // green

    [Header("Size")]
    public float baseFontSize   = 5f;
    public float critFontSize   = 7f;
    public float playerFontSize = 5.5f;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private TextMeshPro _label;
    private Transform   _cam;
    private Coroutine   _animCoroutine;
    private System.Action<FloatingDamageText> _returnToPool;

    private static readonly System.Random Rng = new System.Random();

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _label = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// Called by FloatingDamageTextSpawner before activating the object.
    /// </summary>
    public void Initialise(System.Action<FloatingDamageText> returnToPool)
    {
        _returnToPool = returnToPool;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public enum DamageKind { Normal, Critical, PlayerReceived, Healing }

    /// <summary>
    /// Plays the floating animation for the given value at this object's current world position.
    /// </summary>
    public void Play(float value, DamageKind kind)
    {
        if (_cam == null)
            _cam = Camera.main?.transform;

        // Pick colour and font size
        Color  targetColour;
        float  fontSize;
        string prefix = string.Empty;

        switch (kind)
        {
            case DamageKind.Critical:
                targetColour = colourCritical;
                fontSize     = critFontSize;
                prefix       = "! ";
                break;
            case DamageKind.PlayerReceived:
                targetColour = colourPlayer;
                fontSize     = playerFontSize;
                break;
            case DamageKind.Healing:
                targetColour = colourHealing;
                fontSize     = baseFontSize;
                prefix       = "+ ";
                break;
            default:
                targetColour = colourNormal;
                fontSize     = baseFontSize;
                break;
        }

        _label.fontSize  = fontSize;
        _label.color     = targetColour;
        _label.text      = $"{prefix}{Mathf.RoundToInt(value)}";
        _label.alpha     = 1f;

        // Apply a small random horizontal offset so numbers don't perfectly stack
        float xOff = ((float)Rng.NextDouble() - 0.5f) * 2f * horizontalSpread;
        float zOff = ((float)Rng.NextDouble() - 0.5f) * 2f * horizontalSpread * 0.5f;
        transform.localPosition += new Vector3(xOff, 0f, zOff);

        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(Animate());
    }

    // -------------------------------------------------------------------------
    // Animation
    // -------------------------------------------------------------------------

    private IEnumerator Animate()
    {
        Vector3 startPos    = transform.position;
        Vector3 endPos      = startPos + Vector3.up * riseDistance;
        float   punchEnd    = lifetime * punchFraction;
        float   fadeStart   = lifetime * (1f - fadeFraction);
        float   elapsed     = 0f;
        Color   baseColour  = _label.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            // Billboard — face camera every frame
            if (_cam != null)
                transform.forward = _cam.forward;

            // Position — ease-out rise
            transform.position = Vector3.LerpUnclamped(startPos, endPos, EaseOut(t));

            // Scale punch-in then settle to 1
            float scaleT = Mathf.Clamp01(elapsed / punchEnd);
            float scale  = elapsed <= punchEnd
                ? Mathf.LerpUnclamped(1f, punchScale, EaseOut(scaleT))
                : Mathf.LerpUnclamped(punchScale, 1f, EaseOut((elapsed - punchEnd) / (lifetime - punchEnd)));
            transform.localScale = Vector3.one * scale;

            // Fade out in the final portion of lifetime
            float alpha = elapsed >= fadeStart
                ? 1f - Mathf.Clamp01((elapsed - fadeStart) / (lifetime - fadeStart))
                : 1f;
            _label.alpha = alpha;

            yield return null;
        }

        _label.alpha = 0f;
        _returnToPool?.Invoke(this);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

    private void OnDisable()
    {
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }
    }
}
