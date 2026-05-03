using UnityEngine;
using UnityEngine.UI;
using JUTPS;

/// <summary>
/// Renders a Division-style segmented horizontal bar.
/// Place this on any RectTransform. It fills that rect with evenly-spaced
/// image segments, lighting them up based on a normalised health value.
/// Reads from JUHealth (player or explicit reference) or falls back to a Slider.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SegmentedBar : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Automatically find the Player's JUHealth on Start.")]
    [SerializeField] private bool usePlayerHealth = true;
    [SerializeField] private JUHealth healthComponent;
    [Tooltip("Used only when both health options above are unset.")]
    [SerializeField] private Slider sourceSlider;

    [Header("Segments")]
    [SerializeField] private int segmentCount = 20;
    [SerializeField] [Min(0f)] private float gapWidth = 3f;
    [Tooltip("Optional sprite for each segment. Defaults to a plain white quad.")]
    [SerializeField] private Sprite segmentSprite;

    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(1f, 0.55f, 0f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.95f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color inactiveColor = new Color(0.08f, 0.08f, 0.08f, 0.65f);
    [SerializeField] [Range(0f, 0.5f)] private float lowHealthThreshold = 0.25f;

    [Header("Animation")]
    [Tooltip("How fast the internal value approaches the target. Matches UIHealhBar feel.")]
    [SerializeField] private float smoothSpeed = 6f;

    // ── Internal state ───────────────────────────────────────────────────────

    private Image[] segmentImages;
    private float smoothedValue = 1f;

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        if (usePlayerHealth && healthComponent == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                healthComponent = player.GetComponent<JUHealth>();
        }

        if (healthComponent == null && sourceSlider == null)
            Debug.LogWarning("[SegmentedBar] No health source assigned.");

        BuildSegments();
    }

    private void Update()
    {
        float target = SampleTargetValue();
        smoothedValue = Mathf.MoveTowards(smoothedValue, target, smoothSpeed * Time.deltaTime);
        RefreshSegments(smoothedValue);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Manually drive the bar with a normalised value (0–1).
    /// Only effective when neither a JUHealth nor a Slider source is set.
    /// </summary>
    public void SetValue(float value)
    {
        smoothedValue = Mathf.Clamp01(value);
        RefreshSegments(smoothedValue);
    }

    // ── Segment building ─────────────────────────────────────────────────────

    private void BuildSegments()
    {
        // Remove only previously generated segment children (named "Seg_##").
        // Preserve non-segment children such as the Slider, backgrounds, etc.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Seg_"))
                Destroy(child);
        }

        segmentImages = new Image[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = new GameObject($"Seg_{i:00}");
            seg.transform.SetParent(transform, false);

            Image img = seg.AddComponent<Image>();
            img.sprite = segmentSprite;
            img.color = activeColor;
            img.raycastTarget = false;
            img.type = Image.Type.Sliced;

            // HorizontalLayoutGroup will size these; flexible width = even distribution
            LayoutElement le = seg.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            segmentImages[i] = img;
        }

        // Add HorizontalLayoutGroup to this RectTransform to space segments
        HorizontalLayoutGroup hlg = GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
            hlg = gameObject.AddComponent<HorizontalLayoutGroup>();

        hlg.spacing = gapWidth;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(0, 0, 0, 0);

        RefreshSegments(smoothedValue);
    }

    private void RefreshSegments(float value)
    {
        if (segmentImages == null) return;

        // How many whole segments should be lit
        int activeCount = Mathf.RoundToInt(value * segmentCount);
        Color fillColor = value <= lowHealthThreshold ? lowHealthColor : activeColor;

        for (int i = 0; i < segmentImages.Length; i++)
            segmentImages[i].color = i < activeCount ? fillColor : inactiveColor;
    }

    private float SampleTargetValue()
    {
        if (healthComponent != null && healthComponent.MaxHealth > 0f)
            return healthComponent.Health / healthComponent.MaxHealth;

        if (sourceSlider != null)
            return sourceSlider.value;

        return smoothedValue;
    }
}
