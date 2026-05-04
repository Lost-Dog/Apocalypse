using UnityEngine;
using UnityEngine.UI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;

/// <summary>
/// Renders a Division-style segmented horizontal bar.
/// Place this on any RectTransform. It fills that rect with evenly-spaced
/// image segments, lighting them up based on a normalised health value.
/// Reads from the player's GC2 Traits health attribute or falls back to a Slider.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SegmentedBar : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    [Header("Source")]
    [Tooltip("Automatically find the player's health Attribute via GC2 Traits on Start.")]
    [SerializeField] private bool usePlayerHealth = true;
    [Tooltip("Used only when usePlayerHealth is false.")]
    [SerializeField] private Slider sourceSlider;

    [Header("Segments")]
    [SerializeField] private int segmentCount = 20;
    [SerializeField] [Min(0f)] private float gapWidth = 3f;
    [Tooltip("Optional sprite for each segment. Defaults to a plain white quad.")]
    [SerializeField] private Sprite segmentSprite;

    [Header("Colors")]
    [SerializeField] private Color activeColor      = new Color(1f, 0.55f, 0f, 1f);
    [SerializeField] private Color lowHealthColor   = new Color(0.95f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color inactiveColor    = new Color(0.08f, 0.08f, 0.08f, 0.65f);
    [SerializeField] [Range(0f, 0.5f)] private float lowHealthThreshold = 0.25f;

    [Header("Animation")]
    [Tooltip("How fast the internal value approaches the target.")]
    [SerializeField] private float smoothSpeed = 6f;

    // Internal state
    private Image[] segmentImages;
    private float smoothedValue = 1f;
    private Traits playerTraits;

    private void Start()
    {
        if (usePlayerHealth)
        {
            GameObject player = ShortcutPlayer.Instance;
            if (player != null)
                playerTraits = player.GetComponent<Traits>();

            if (playerTraits == null)
                Debug.LogWarning("[SegmentedBar] No player Traits component found.");
        }

        if (playerTraits == null && sourceSlider == null)
            Debug.LogWarning("[SegmentedBar] No health source assigned.");

        BuildSegments();
    }

    private void Update()
    {
        float target = SampleTargetValue();
        smoothedValue = Mathf.MoveTowards(smoothedValue, target, smoothSpeed * Time.deltaTime);
        RefreshSegments(smoothedValue);
    }

    // PUBLIC API ----------------------------------------------------------------------------------

    /// <summary>
    /// Manually drives the bar with a normalised value (0–1).
    /// Only effective when neither a Traits nor a Slider source is set.
    /// </summary>
    public void SetValue(float value)
    {
        smoothedValue = Mathf.Clamp01(value);
        RefreshSegments(smoothedValue);
    }

    // SEGMENT BUILDING ----------------------------------------------------------------------------

    private void BuildSegments()
    {
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
            img.sprite        = segmentSprite;
            img.color         = activeColor;
            img.raycastTarget = false;
            img.type          = Image.Type.Sliced;

            LayoutElement le = seg.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            segmentImages[i] = img;
        }

        HorizontalLayoutGroup hlg = GetComponent<HorizontalLayoutGroup>() ?? gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = gapWidth;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment       = TextAnchor.MiddleLeft;
        hlg.padding              = new RectOffset(0, 0, 0, 0);

        RefreshSegments(smoothedValue);
    }

    private void RefreshSegments(float value)
    {
        if (segmentImages == null) return;

        int activeCount  = Mathf.RoundToInt(value * segmentCount);
        Color fillColor  = value <= lowHealthThreshold ? lowHealthColor : activeColor;

        for (int i = 0; i < segmentImages.Length; i++)
            segmentImages[i].color = i < activeCount ? fillColor : inactiveColor;
    }

    private float SampleTargetValue()
    {
        if (playerTraits != null)
        {
            try
            {
                RuntimeAttributeData health = playerTraits.RuntimeAttributes.Get(HealthAttributeId);
                if (health.MaxValue > 0)
                    return (float)(health.Value / health.MaxValue);
            }
            catch (System.Exception) { }
        }

        if (sourceSlider != null)
            return sourceSlider.value;

        return smoothedValue;
    }
}
