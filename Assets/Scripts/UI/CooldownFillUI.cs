using UnityEngine;
using UnityEngine.UI;
using Apocalypse;

/// <summary>
/// Generic vertical-fill cooldown visualiser.
///
/// Attach to any UI Image (Image Type = Filled, Fill Method = Vertical,
/// Fill Origin = Bottom). Set <see cref="source"/> to either
/// <see cref="CooldownSource.TraitReset"/> or <see cref="CooldownSource.Grenade"/>.
///
/// On cooldown  → fill drains from full to empty over the cooldown duration.
/// When ready   → fill stays full and pulses alpha to draw attention.
/// </summary>
[RequireComponent(typeof(Image))]
public class CooldownFillUI : MonoBehaviour
{
    // ── Source enum ───────────────────────────────────────────────────────────

    public enum CooldownSource { TraitReset, Grenade }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Tooltip("Which cooldown this Image represents.")]
    public CooldownSource source = CooldownSource.Grenade;

    [Tooltip("Filled Image to animate. Auto-assigned from this GameObject if left empty.")]
    [SerializeField] private Image fillImage;

    [Tooltip("Cycles per second for the ready-state alpha pulse.")]
    [SerializeField] private float pulseSpeed = 1.2f;

    [Tooltip("Minimum alpha during the ready pulse (0 = fully transparent, 1 = opaque).")]
    [Range(0f, 1f)]
    [SerializeField] private float pulseMinAlpha = 0.35f;

    [Tooltip("Fill colour while on cooldown.")]
    [SerializeField] private Color cooldownColor = new Color(1f, 0.45f, 0.1f, 1f);

    [Tooltip("Fill colour when the ability is ready.")]
    [SerializeField] private Color readyColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Tooltip("When enabled, the fill pulses alpha to draw attention once the cooldown is ready.")]
    [SerializeField] private bool pulseWhenReady = true;

    // ── Private state ─────────────────────────────────────────────────────────

    private ProgressionManager       _progressionManager;
    private vThrowManagerObservable  _throwManager;
    private float                    _pulseTime;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (source == CooldownSource.TraitReset)
        {
            _progressionManager = ProgressionManager.Instance
                                  ?? FindFirstObjectByType<ProgressionManager>();

            if (_progressionManager == null)
            {
                Debug.LogWarning("[CooldownFillUI] ProgressionManager not found — component disabled.");
                enabled = false;
                return;
            }
        }
        else
        {
            _throwManager = FindFirstObjectByType<vThrowManagerObservable>();

            if (_throwManager == null)
            {
                Debug.LogWarning("[CooldownFillUI] vThrowManagerObservable not found — component disabled.");
                enabled = false;
                return;
            }
        }

        ApplyFill(1f, readyColor);
    }

    private void Update()
    {
        if (fillImage == null) return;

        bool  isReady  = GetIsReady();
        float progress = GetProgress(); // 0 = just used, 1 = ready

        if (isReady)
        {
            if (pulseWhenReady)
            {
                _pulseTime += Time.deltaTime * pulseSpeed;
                float alpha = Mathf.Lerp(pulseMinAlpha, 1f, (Mathf.Sin(_pulseTime * Mathf.PI * 2f) + 1f) * 0.5f);
                Color c = readyColor;
                c.a = alpha;
                ApplyFill(1f, c);
            }
            else
            {
                _pulseTime = 0f;
                ApplyFill(1f, readyColor);
            }
        }
        else
        {
            _pulseTime = 0f;
            ApplyFill(progress, cooldownColor);
        }
    }

    // ── Source abstraction ────────────────────────────────────────────────────

    private bool GetIsReady()
    {
        return source == CooldownSource.TraitReset
            ? _progressionManager.IsTraitResetReady
            : !_throwManager.IsCoolingDown;
    }

    private float GetProgress()
    {
        if (source == CooldownSource.TraitReset)
            return _progressionManager.TraitResetCooldownProgress;

        // vThrowManagerObservable.CooldownProgress is 1 = just thrown, 0 = ready.
        // Invert so that 1 = full (ready), 0 = empty (just thrown).
        return 1f - _throwManager.CooldownProgress;
    }

    private void ApplyFill(float fillAmount, Color color)
    {
        fillImage.fillAmount = Mathf.Clamp01(fillAmount);
        fillImage.color      = color;
    }
}
