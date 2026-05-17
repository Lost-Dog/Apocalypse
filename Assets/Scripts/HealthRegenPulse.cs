using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pulses a UI Graphic (Image, RawImage, etc.) or CanvasGroup alpha while
/// a <see cref="HealthOnKillSkill"/> regen window is active.
///
/// Assign <see cref="targetGraphic"/> or <see cref="targetGroup"/> in the
/// Inspector. If neither is assigned, the component searches on itself.
/// Subscribe to <see cref="HealthOnKillSkill.OnRegenStarted"/> /
/// <see cref="HealthOnKillSkill.OnRegenStopped"/> to drive the animation.
/// </summary>
[RequireComponent(typeof(Graphic))]
public class HealthRegenPulse : MonoBehaviour
{
    private const string LogPrefix = "[HealthRegenPulse]";

    [Header("Skill Reference")]
    [Tooltip("The HealthOnKillSkill to listen to. Auto-found if left empty.")]
    public HealthOnKillSkill skill;

    [Header("Pulse Target")]
    [Tooltip("Graphic to pulse. Defaults to the Graphic on this GameObject.")]
    public Graphic targetGraphic;

    [Tooltip("CanvasGroup to pulse instead of a Graphic (optional, takes priority).")]
    public CanvasGroup targetGroup;

    [Header("Pulse Settings")]
    [Tooltip("Lowest alpha during a pulse cycle.")]
    [Range(0f, 1f)]
    public float minAlpha = 0.2f;

    [Tooltip("Highest alpha during a pulse cycle.")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    [Tooltip("Full cycles (min → max → min) per second.")]
    public float pulseFrequency = 2f;

    [Tooltip("Seconds to fade out the pulse effect after regen stops.")]
    public float fadeOutDuration = 0.4f;

    [Header("Debug")]
    public bool debugMode = false;

    // ── Internals ─────────────────────────────────────────────────────────────

    private Coroutine _pulseCoroutine;
    private float     _baseAlpha;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetGroup == null)
            targetGroup = GetComponent<CanvasGroup>();

        _baseAlpha = GetCurrentAlpha();
    }

    private void Start()
    {
        ResolveSkill();
    }

    private void OnDestroy()
    {
        UnbindSkill();
    }

    // ── Skill binding ─────────────────────────────────────────────────────────

    private void ResolveSkill()
    {
        if (skill == null)
            skill = FindFirstObjectByType<HealthOnKillSkill>();

        if (skill == null)
        {
            Debug.LogWarning($"{LogPrefix} No HealthOnKillSkill found in scene.");
            return;
        }

        BindSkill();
    }

    private void BindSkill()
    {
        skill.OnRegenStarted += HandleRegenStarted;
        skill.OnRegenStopped += HandleRegenStopped;

        if (debugMode)
            Debug.Log($"<color=cyan>{LogPrefix} Bound to '{skill.name}'.</color>");

        // Sync immediately if regen was already running when we bound.
        if (skill.IsRegenerating)
            HandleRegenStarted();
    }

    private void UnbindSkill()
    {
        if (skill == null) return;

        skill.OnRegenStarted -= HandleRegenStarted;
        skill.OnRegenStopped -= HandleRegenStopped;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleRegenStarted()
    {
        if (_pulseCoroutine != null)
            StopCoroutine(_pulseCoroutine);

        _pulseCoroutine = StartCoroutine(PulseCoroutine());

        if (debugMode)
            Debug.Log($"<color=green>{LogPrefix} Pulse started.</color>");
    }

    private void HandleRegenStopped()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }

        StartCoroutine(FadeOutCoroutine());

        if (debugMode)
            Debug.Log($"<color=yellow>{LogPrefix} Pulse stopped — fading out.</color>");
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator PulseCoroutine()
    {
        while (true)
        {
            // One full sine cycle over (1 / pulseFrequency) seconds.
            float t = Time.time * pulseFrequency * Mathf.PI * 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t) + 1f) * 0.5f);
            SetAlpha(alpha);
            yield return null;
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        float start   = GetCurrentAlpha();
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, _baseAlpha, elapsed / fadeOutDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(_baseAlpha);
    }

    // ── Alpha helpers ─────────────────────────────────────────────────────────

    private void SetAlpha(float alpha)
    {
        if (targetGroup != null)
        {
            targetGroup.alpha = alpha;
            return;
        }

        if (targetGraphic != null)
        {
            Color c = targetGraphic.color;
            c.a = alpha;
            targetGraphic.color = c;
        }
    }

    private float GetCurrentAlpha()
    {
        if (targetGroup   != null) return targetGroup.alpha;
        if (targetGraphic != null) return targetGraphic.color.a;
        return 1f;
    }

    // ── Runtime control ───────────────────────────────────────────────────────

    /// <summary>Rebinds the pulse to a different skill at runtime.</summary>
    public void BindToSkill(HealthOnKillSkill newSkill)
    {
        UnbindSkill();
        skill = newSkill;
        BindSkill();
    }
}
