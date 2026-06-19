using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusWarning : MonoBehaviour
{
    [Header("Warning UI")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;
    public Image warningIcon;

    [Header("Auto-Find References")]
    public bool autoFindReferences = true;
    [Tooltip("Legacy reference; warning checks now use provider traits.")]
    public SurvivalManager survivalManager;
    [Tooltip("Legacy reference; warning checks now use provider traits.")]
    public PlayerInfectionDisplay infectionDisplay;

    [Header("Player Provider")]
    [Tooltip("Assign GC2PlayerProvider (or any IPlayerProvider). Auto-finds GC2 first if left empty.")]
    [SerializeField] private MonoBehaviour playerProviderObject;
    private IPlayerProvider playerProvider;
    private ISurvivalStatsProvider survivalStatsProvider;

    [Header("Threshold Settings")]
    [Range(0f, 1f)] public float healthLowThreshold = 0.3f;
    [Range(0f, 1f)] public float healthCriticalThreshold = 0.15f;
    [Range(0f, 1f)] public float temperatureLowThreshold = 0.4f;
    [Range(0f, 1f)] public float temperatureCriticalThreshold = 0.2f;
    [Tooltip("Show warning when immunity drops at or below this value (0–100)")]
    public float infectionLowThreshold = 50f;
    [Tooltip("Show critical warning when immunity drops at or below this value (0–100)")]
    public float infectionCriticalThreshold = 25f;

    [Header("Warning Messages")]
    public string healthLowMessage = "LOW HEALTH";
    public string healthCriticalMessage = "CRITICAL HEALTH";
    public string temperatureLowMessage = "GETTING COLD";
    public string temperatureCriticalMessage = "FREEZING";
    public string infectionLowMessage = "LOW IMMUNITY";
    public string infectionCriticalMessage = "CRITICAL IMMUNITY";

    [Header("Display Settings")]
    public float warningDisplayDuration = 3f;
    public float warningCooldown = 5f;
    public Color lowWarningColor = new Color(1f, 0.8f, 0f, 1f);
    public Color criticalWarningColor = new Color(1f, 0f, 0f, 1f);

    [Header("Initialization")]
    [Tooltip("Suppress warnings briefly on startup to avoid false positives before survival stats finish initializing.")]
    public bool suppressWarningsOnStartup = true;
    [Tooltip("Seconds to wait before warning checks become active.")]
    public float startupWarningDelay = 1f;

    [Header("Flashing Effect")]
    public bool enableFlashing = true;
    public float flashSpeed = 2f;

    private float warningTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isShowingWarning = false;
    private string currentWarning = "";
    private Color currentWarningColor = Color.white;

    private bool wasHealthLow = false;
    private bool wasHealthCritical = false;
    private bool wasTemperatureLow = false;
    private bool wasTemperatureCritical = false;
    private bool wasInfectionLow = false;
    private bool wasInfectionCritical = false;
    private bool baselineInitialized = false;
    private float startupTimer = 0f;

    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }

        startupTimer = Mathf.Max(0f, startupWarningDelay);

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    private void FindReferences()
    {
        if (warningPanel == null)
        {
            warningPanel = gameObject;
        }

        if (warningText == null)
        {
            warningText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (warningIcon == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            if (images.Length > 0)
            {
                warningIcon = images[0];
            }
        }

        // Resolve IPlayerProvider — prefer the serialized field, then search the scene.
        if (playerProvider == null)
        {
            playerProvider = playerProviderObject as IPlayerProvider;
        }

        if (playerProvider == null)
        {
            GC2PlayerProvider gc2Provider = FindFirstObjectByType<GC2PlayerProvider>();
            if (gc2Provider != null)
                playerProvider = gc2Provider;

            if (playerProvider == null)
            {
                foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    if (mb is IPlayerProvider provider)
                    {
                        playerProvider = provider;
                        break;
                    }
                }
            }
        }

        if (playerProvider == null)
        {
            Debug.LogWarning("[PlayerStatusWarning] No IPlayerProvider found — health warnings disabled.");
        }

        survivalStatsProvider = playerProvider as ISurvivalStatsProvider;
        if (survivalStatsProvider == null)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is ISurvivalStatsProvider provider)
                {
                    survivalStatsProvider = provider;
                    break;
                }
            }
        }

        if (survivalStatsProvider == null)
            Debug.LogWarning("[PlayerStatusWarning] No ISurvivalStatsProvider found — temperature/infection warnings disabled.");

        if (survivalManager == null)
            survivalManager = FindFirstObjectByType<SurvivalManager>();

        if (infectionDisplay == null)
            infectionDisplay = FindFirstObjectByType<PlayerInfectionDisplay>();
    }

    private void Update()
    {
        if (!baselineInitialized)
        {
            if (suppressWarningsOnStartup && startupTimer > 0f)
            {
                startupTimer -= Time.deltaTime;
                return;
            }

            InitializeWarningBaseline();
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isShowingWarning)
        {
            UpdateWarningDisplay();
        }
        else
        {
            CheckForWarnings();
        }
    }

    private void CheckForWarnings()
    {
        if (cooldownTimer > 0f) return;

        EvaluateWarningState(
            out bool healthLow,
            out bool healthCritical,
            out bool temperatureLow,
            out bool temperatureCritical,
            out bool infectionLow,
            out bool infectionCritical);

        if (healthCritical && !wasHealthCritical)
        {
            ShowWarning(healthCriticalMessage, criticalWarningColor);
        }
        else if (healthLow && !wasHealthLow)
        {
            ShowWarning(healthLowMessage, lowWarningColor);
        }
        else if (temperatureCritical && !wasTemperatureCritical)
        {
            ShowWarning(temperatureCriticalMessage, criticalWarningColor);
        }
        else if (temperatureLow && !wasTemperatureLow)
        {
            ShowWarning(temperatureLowMessage, lowWarningColor);
        }
        else if (infectionCritical && !wasInfectionCritical)
        {
            ShowWarning(infectionCriticalMessage, criticalWarningColor);
        }
        else if (infectionLow && !wasInfectionLow)
        {
            ShowWarning(infectionLowMessage, lowWarningColor);
        }

        wasHealthLow = healthLow;
        wasHealthCritical = healthCritical;
        wasTemperatureLow = temperatureLow;
        wasTemperatureCritical = temperatureCritical;
        wasInfectionLow = infectionLow;
        wasInfectionCritical = infectionCritical;
    }

    private void InitializeWarningBaseline()
    {
        EvaluateWarningState(
            out bool healthLow,
            out bool healthCritical,
            out bool temperatureLow,
            out bool temperatureCritical,
            out bool infectionLow,
            out bool infectionCritical);

        // Seed previous state so we don't show warnings purely due to startup ordering.
        wasHealthLow = healthLow;
        wasHealthCritical = healthCritical;
        wasTemperatureLow = temperatureLow;
        wasTemperatureCritical = temperatureCritical;
        wasInfectionLow = infectionLow;
        wasInfectionCritical = infectionCritical;
        baselineInitialized = true;
    }

    private void EvaluateWarningState(
        out bool healthLow,
        out bool healthCritical,
        out bool temperatureLow,
        out bool temperatureCritical,
        out bool infectionLow,
        out bool infectionCritical)
    {
        healthLow = false;
        healthCritical = false;
        temperatureLow = false;
        temperatureCritical = false;
        infectionLow = false;
        infectionCritical = false;

        if (playerProvider != null && playerProvider.MaxHealth > 0f)
        {
            float healthPercentage = playerProvider.Health / playerProvider.MaxHealth;
            healthCritical = healthPercentage <= healthCriticalThreshold;
            healthLow      = !healthCritical && healthPercentage <= healthLowThreshold;
        }

        if (survivalStatsProvider != null && survivalStatsProvider.MaxTemperature > 0f)
        {
            float tempPercentage = Mathf.Clamp01(survivalStatsProvider.Temperature / survivalStatsProvider.MaxTemperature);
            temperatureCritical = tempPercentage <= temperatureCriticalThreshold;
            temperatureLow = !temperatureCritical && tempPercentage <= temperatureLowThreshold;
        }

        if (survivalStatsProvider != null && survivalStatsProvider.MaxInfection > 0f)
        {
            // Infection value represents immunity (100 = fully immune, 0 = no immunity).
            float immunity = survivalStatsProvider.Infection / survivalStatsProvider.MaxInfection * 100f;
            infectionCritical = immunity <= infectionCriticalThreshold;
            infectionLow = !infectionCritical && immunity <= infectionLowThreshold;
        }
    }

    private void ShowWarning(string message, Color color)
    {
        currentWarning = message;
        currentWarningColor = color;
        isShowingWarning = true;
        warningTimer = warningDisplayDuration;

        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }

        if (warningText != null)
        {
            warningText.text = message;
            warningText.color = color;
        }

        if (warningIcon != null)
        {
            warningIcon.color = color;
        }
    }

    private void UpdateWarningDisplay()
    {
        warningTimer -= Time.deltaTime;

        if (warningTimer <= 0f)
        {
            HideWarning();
            return;
        }

        if (enableFlashing && warningText != null)
        {
            float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * flashSpeed * Mathf.PI);
            Color flashColor = currentWarningColor;
            flashColor.a = alpha;
            warningText.color = flashColor;

            if (warningIcon != null)
            {
                warningIcon.color = flashColor;
            }
        }
    }

    private void HideWarning()
    {
        isShowingWarning = false;
        cooldownTimer = warningCooldown;

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    /// <summary>Imperatively shows a warning panel with a custom message and colour.</summary>
    public void ForceShowWarning(string message, Color color, float duration = 3f)
    {
        warningDisplayDuration = duration;
        ShowWarning(message, color);
    }

    /// <summary>Immediately hides the warning panel.</summary>
    public void ClearWarning()
    {
        HideWarning();
    }
}
