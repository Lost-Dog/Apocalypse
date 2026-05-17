using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's stamina bar driven exclusively by SurvivalManager.
/// Subscribes to SurvivalManager.onStaminaChanged so the bar is always in sync
/// with the authoritative survival state — no local drain/regen simulation.
/// </summary>
public class PlayerStaminaDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI staminaText;
    public Slider staminaSlider;
    public Image staminaDial;

    [Header("Display Settings")]
    public bool showAsPercentage = false;
    public bool showFraction     = true;
    public bool showPrefix       = false;
    public string prefix         = "Stamina: ";

    [Header("Dial Settings")]
    [Tooltip("Enable dial fill and color transition")]
    public bool enableDial = false;
    [Tooltip("Smooth transition speed for the dial fill")]
    public float dialTransitionSpeed = 4f;

    [Header("Dial Colors")]
    public Color fullStaminaColor     = new Color(0f,   1f,    0.2f,  1f);
    public Color highStaminaColor     = new Color(0.5f, 1f,    0f,    1f);
    public Color moderateStaminaColor = new Color(1f,   0.92f, 0.016f,1f);
    public Color lowStaminaColor      = new Color(1f,   0.5f,  0f,    1f);
    public Color criticalStaminaColor = new Color(1f,   0f,    0f,    1f);

    [Header("Auto-Find")]
    public bool autoFindReferences = true;

    // ── State ─────────────────────────────────────────────────────────────────

    private float _currentStamina;
    private float _maxStamina     = 100f;
    private float _currentDialFill = 1f;
    private float _targetDialFill  = 1f;

    private SurvivalManager _survival;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (autoFindReferences)
            FindReferences();

        BindSurvivalManager();
        InitializeSlider();
        Refresh(_currentStamina);
    }

    private void OnDestroy()
    {
        if (_survival != null)
            _survival.onStaminaChanged.RemoveListener(Refresh);
    }

    private void Update()
    {
        if (enableDial && staminaDial != null)
            UpdateDialDisplay();
    }

    // ── Setup ─────────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        if (staminaText   == null) staminaText   = GetComponent<TextMeshProUGUI>();
        if (staminaSlider == null) staminaSlider = GetComponent<Slider>();

        if (enableDial && staminaDial == null)
        {
            staminaDial = GetComponent<Image>() ?? GetComponentInChildren<Image>();
            if (staminaDial != null && staminaDial.type != Image.Type.Filled)
            {
                staminaDial.type       = Image.Type.Filled;
                staminaDial.fillMethod = Image.FillMethod.Radial360;
            }
        }
    }

    /// <summary>
    /// Locates SurvivalManager and subscribes to its stamina event.
    /// Seeds the initial values from SurvivalManager so the bar is correct on frame 1.
    /// </summary>
    private void BindSurvivalManager()
    {
        _survival = SurvivalManager.Instance ?? FindFirstObjectByType<SurvivalManager>();

        if (_survival == null)
        {
            Debug.LogWarning("[PlayerStaminaDisplay] SurvivalManager not found — stamina bar will not update.");
            return;
        }

        _maxStamina     = _survival.maxStamina;
        _currentStamina = _survival.currentStamina;

        _survival.onStaminaChanged.AddListener(Refresh);
    }

    private void InitializeSlider()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = _maxStamina;
            staminaSlider.value    = _currentStamina;
        }

        if (staminaDial != null && enableDial)
        {
            _currentDialFill       = _maxStamina > 0f ? _currentStamina / _maxStamina : 1f;
            _targetDialFill        = _currentDialFill;
            staminaDial.fillAmount = _currentDialFill;
            UpdateDialColor();
        }
    }

    // ── Display ───────────────────────────────────────────────────────────────

    /// <summary>Called by SurvivalManager.onStaminaChanged with the new stamina value.</summary>
    public void Refresh(float stamina)
    {
        // maxStamina can change at runtime (e.g. level-up bonuses), re-read it each call.
        if (_survival != null)
            _maxStamina = _survival.maxStamina;

        _currentStamina = stamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = _maxStamina;
            staminaSlider.value    = _currentStamina;
        }

        if (staminaText != null)
        {
            string displayText;
            float pct = _maxStamina > 0f ? _currentStamina / _maxStamina : 0f;

            if (showAsPercentage)
                displayText = $"{Mathf.RoundToInt(pct * 100f)}%";
            else if (showFraction)
                displayText = $"{Mathf.RoundToInt(_currentStamina)}/{Mathf.RoundToInt(_maxStamina)}";
            else
                displayText = Mathf.RoundToInt(_currentStamina).ToString();

            staminaText.text = showPrefix ? $"{prefix}{displayText}" : displayText;
        }

        if (staminaDial != null && enableDial)
            _targetDialFill = _maxStamina > 0f ? _currentStamina / _maxStamina : 0f;
    }

    private void UpdateDialDisplay()
    {
        _currentDialFill       = Mathf.MoveTowards(_currentDialFill, _targetDialFill, dialTransitionSpeed * Time.deltaTime);
        staminaDial.fillAmount = _currentDialFill;
        UpdateDialColor();
    }

    private void UpdateDialColor()
    {
        if (staminaDial == null) return;

        float pct = _currentDialFill * 100f;
        Color color;

        if (pct >= 75f)
            color = Color.Lerp(highStaminaColor,     fullStaminaColor,     Mathf.InverseLerp(75f,  100f, pct));
        else if (pct >= 50f)
            color = Color.Lerp(moderateStaminaColor, highStaminaColor,     Mathf.InverseLerp(50f,  75f,  pct));
        else if (pct >= 25f)
            color = Color.Lerp(lowStaminaColor,      moderateStaminaColor, Mathf.InverseLerp(25f,  50f,  pct));
        else if (pct > 0f)
            color = Color.Lerp(criticalStaminaColor, lowStaminaColor,      Mathf.InverseLerp(0f,   25f,  pct));
        else
            color = criticalStaminaColor;

        staminaDial.color = color;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Current stamina as a 0–1 normalised value.</summary>
    public float StaminaNormalized => _maxStamina > 0f ? _currentStamina / _maxStamina : 0f;
}
