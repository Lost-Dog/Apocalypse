using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders a Division-style segmented horizontal bar.
/// Place this on any RectTransform. It fills that rect with evenly-spaced
/// image segments, lighting them up based on a normalised value.
///
/// Sources (in priority order):
///   1. IPlayerProvider health (when usePlayerHealth is true)
///   2. sourceSlider normalised value
///   3. Manual SetValue() calls
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SegmentedBar : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Read health from the scene's IPlayerProvider. Auto-found on Start.")]
    [SerializeField] private bool usePlayerHealth = true;
    [Tooltip("Used as fallback when usePlayerHealth is false or no provider is found.")]
    [SerializeField] private Slider sourceSlider;

    [Header("Segments")]
    [SerializeField] private int segmentCount = 20;
    [SerializeField] [Min(0f)] private float gapWidth = 3f;
    [Tooltip("Optional sprite for each segment. Defaults to a plain white quad.")]
    [SerializeField] private Sprite segmentSprite;

    [Header("Level-Based Segmentation")]
    [Tooltip("When enabled, segmentCount is driven by player level and the settings below.")]
    [SerializeField] private bool useLevelSegmentation = false;
    [Tooltip("Number of segments at level 1.")]
    [SerializeField] [Min(1)] private int baseSegmentCount = 3;
    [Tooltip("Add one segment every N levels (e.g. 5 → level 1=3, level 6=4, level 11=5…).")]
    [SerializeField] [Min(1)] private int levelsPerSegment = 5;

    [Header("Colors")]
    [SerializeField] private Color activeColor    = new Color(1f,    0.55f, 0f,   1f);
    [SerializeField] private Color lowHealthColor = new Color(0.95f, 0.1f,  0.1f, 1f);
    [SerializeField] private Color inactiveColor  = new Color(0.08f, 0.08f, 0.08f, 0.65f);
    [SerializeField] [Range(0f, 0.5f)] private float lowHealthThreshold = 0.25f;

    [Header("Animation")]
    [Tooltip("How fast the displayed value approaches the target.")]
    [SerializeField] private float smoothSpeed = 6f;

    // ── Internal state ────────────────────────────────────────────────────────

    private Image[]         segmentImages;
    private float           smoothedValue = 1f;
    private IPlayerProvider playerProvider;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (usePlayerHealth)
        {
            playerProvider = FindAnyPlayerProvider();

            if (playerProvider == null)
                Debug.LogWarning("[SegmentedBar] usePlayerHealth is true but no IPlayerProvider found — falling back to sourceSlider.");
            else
                playerProvider.OnHealthChanged += OnHealthChanged;
        }

        if (playerProvider == null && sourceSlider == null)
            Debug.LogWarning("[SegmentedBar] No health source assigned.");

        if (useLevelSegmentation)
            segmentCount = baseSegmentCount; // safe default until LateStart resolves the manager

        BuildSegments();

        // Defer one frame so all Awake/Start methods (including ProgressionManager) complete first.
        StartCoroutine(LateStart());
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
            playerProvider.OnHealthChanged -= OnHealthChanged;

        if (useLevelSegmentation && ProgressionManager.Instance != null)
            ProgressionManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
    }

    private void Update()
    {
        // When reading from the slider (non-provider path), poll each frame.
        if (playerProvider == null && sourceSlider != null)
        {
            float target = Mathf.Clamp01(sourceSlider.value);
            smoothedValue = Mathf.MoveTowards(smoothedValue, target, smoothSpeed * Time.deltaTime);
            RefreshSegments(smoothedValue);
        }
        else if (playerProvider != null)
        {
            // Smooth the value toward the last received health.
            float target = playerProvider.MaxHealth > 0f
                ? Mathf.Clamp01(playerProvider.Health / playerProvider.MaxHealth)
                : 0f;
            smoothedValue = Mathf.MoveTowards(smoothedValue, target, smoothSpeed * Time.deltaTime);
            RefreshSegments(smoothedValue);
        }
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnHealthChanged(float current, float max)
    {
        // Smooth movement toward the new target happens in Update;
        // nothing extra needed here unless you want instant snapping.
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Manually drives the bar with a normalised value (0–1).
    /// Only effective when no provider or slider source is active.
    /// </summary>
    public void SetValue(float value)
    {
        smoothedValue = Mathf.Clamp01(value);
        RefreshSegments(smoothedValue);
    }

    // ── Level-up handler ──────────────────────────────────────────────────────

    /// <summary>Recalculates segment count when the player levels up.</summary>
    private void OnLevelUp(int newLevel)
    {
        int newCount = CalculateSegmentCount(newLevel);
        if (newCount == segmentCount) return;

        segmentCount = newCount;
        BuildSegments();
    }

    /// <summary>
    /// Returns the segment count for a given level.
    /// Formula: baseSegmentCount + floor((level - 1) / levelsPerSegment).
    /// </summary>
    private int CalculateSegmentCount(int level)
    {
        return baseSegmentCount + Mathf.FloorToInt((level - 1) / (float)levelsPerSegment);
    }

    // ── Segment building ──────────────────────────────────────────────────────

    private void BuildSegments()
    {
        // Remove any previously created segment objects only.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Seg_"))
                Destroy(child);
        }

        // Leave existing non-segment children (Background, Slider, etc.) intact.

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

        HorizontalLayoutGroup hlg = GetComponent<HorizontalLayoutGroup>()
                                 ?? gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing               = gapWidth;
        hlg.childControlWidth     = true;
        hlg.childControlHeight    = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment        = TextAnchor.MiddleLeft;
        hlg.padding               = new RectOffset(0, 0, 0, 0);

        RefreshSegments(smoothedValue);
    }

    private IEnumerator LateStart()
    {
        yield return null; // wait one frame for all Awake/Start methods to complete

        // ── Level segmentation ────────────────────────────────────────────────
        if (useLevelSegmentation)
        {
            if (ProgressionManager.Instance != null)
            {
                int resolvedCount = CalculateSegmentCount(ProgressionManager.Instance.currentLevel);
                ProgressionManager.Instance.onLevelUp.AddListener(OnLevelUp);

                if (resolvedCount != segmentCount)
                {
                    segmentCount = resolvedCount;
                    BuildSegments();
                }
            }
            else
            {
                Debug.LogWarning("[SegmentedBar] useLevelSegmentation is true but ProgressionManager.Instance is null.");
            }
        }

        // ── Seed health value ─────────────────────────────────────────────────
        if (playerProvider != null && playerProvider.MaxHealth > 0f)
        {
            smoothedValue = playerProvider.Health / playerProvider.MaxHealth;
            RefreshSegments(smoothedValue);
        }
        else if (sourceSlider != null)
        {
            smoothedValue = Mathf.Clamp01(sourceSlider.value);
            RefreshSegments(smoothedValue);
        }
    }

    private void RefreshSegments(float value)
    {
        if (segmentImages == null) return;

        int   activeCount = Mathf.RoundToInt(value * segmentCount);
        Color fillColor   = value <= lowHealthThreshold ? lowHealthColor : activeColor;

        for (int i = 0; i < segmentImages.Length; i++)
            segmentImages[i].color = i < activeCount ? fillColor : inactiveColor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IPlayerProvider FindAnyPlayerProvider()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IPlayerProvider provider)
                return provider;
        }
        return null;
    }
}
