using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicsManagerURP : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject resolutionSelectorPanel;
    public GameObject refreshRateSelectorPanel;
    public GameObject graphicsQualitySelectorPanel;
    public GameObject antiAliasingSelectorPanel;

    private Button leftArrowResolutionButton;
    private Button rightArrowResolutionButton;
    private Text currentResolutionText;

    private Button leftArrowRefreshRateButton;
    private Button rightArrowRefreshRateButton;
    private Text currentRefreshRateText;

    private Button leftArrowGraphicsQualityButton;
    private Button rightArrowGraphicsQualityButton;
    private Text currentGraphicsQualityText;

    private Button leftArrowAntiAliasingButton;
    private Button rightArrowAntiAliasingButton;
    private Text currentAntiAliasingText;

    public Toggle fullScreenToggle;
    public Toggle vsyncToggle;
    public Button resetButton;

    private Resolution[] availableResolutions;
    private int currentResolutionIndex;
    private int[] refreshRates;
    private int currentRefreshRateIndex;
    private string[] graphicsQualities = { "Performant", "Balanced", "High Fidelity" };
    private int currentGraphicsQualityIndex;
    private string[] antiAliasingOptions = { "None", "FXAA", "SMAA" };
    private int currentAntiAliasingIndex;
    private UniversalAdditionalCameraData urpCameraData;

    void Start()
    {
        InitializeResolutionSelector();
        InitializeRefreshRateSelector();
        InitializeGraphicsQualitySelector();
        InitializeAntiAliasingSelector();

        Camera mainCamera = Camera.main;
        urpCameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();

        resetButton.onClick.AddListener(ResetToDefaults);

        LoadSettings();

        fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    private void InitializeResolutionSelector()
    {
        if (resolutionSelectorPanel == null)
        {
            Debug.LogError("resolutionSelectorPanel is not assigned.");
            return;
        }

        leftArrowResolutionButton = resolutionSelectorPanel.transform.Find("LeftArrowButton").GetComponent<Button>();
        rightArrowResolutionButton = resolutionSelectorPanel.transform.Find("RightArrowButton").GetComponent<Button>();
        currentResolutionText = resolutionSelectorPanel.transform.Find("CurrentSelectionText").GetComponent<Text>();

        availableResolutions = Screen.resolutions
            .OrderByDescending(res => res.width * res.height)
            .ThenByDescending(res => res.refreshRateRatio.value)
            .GroupBy(res => new { res.width, res.height })
            .Select(g => g.First())
            .ToArray();

        currentResolutionIndex = 0;
        UpdateResolutionText();

        leftArrowResolutionButton.onClick.AddListener(SelectPreviousResolution);
        rightArrowResolutionButton.onClick.AddListener(SelectNextResolution);
    }

    private void InitializeRefreshRateSelector()
    {
        if (refreshRateSelectorPanel == null)
        {
            Debug.LogError("refreshRateSelectorPanel is not assigned.");
            return;
        }

        leftArrowRefreshRateButton = refreshRateSelectorPanel.transform.Find("LeftArrowButton").GetComponent<Button>();
        rightArrowRefreshRateButton = refreshRateSelectorPanel.transform.Find("RightArrowButton").GetComponent<Button>();
        currentRefreshRateText = refreshRateSelectorPanel.transform.Find("CurrentSelectionText").GetComponent<Text>();

        refreshRates = availableResolutions.Select(res => (int)res.refreshRateRatio.value).Distinct().ToArray();

        currentRefreshRateIndex = 0;
        UpdateRefreshRateText();

        leftArrowRefreshRateButton.onClick.AddListener(SelectPreviousRefreshRate);
        rightArrowRefreshRateButton.onClick.AddListener(SelectNextRefreshRate);
    }

    private void InitializeGraphicsQualitySelector()
    {
        if (graphicsQualitySelectorPanel == null)
        {
            Debug.LogError("graphicsQualitySelectorPanel is not assigned.");
            return;
        }

        leftArrowGraphicsQualityButton = graphicsQualitySelectorPanel.transform.Find("LeftArrowButton").GetComponent<Button>();
        rightArrowGraphicsQualityButton = graphicsQualitySelectorPanel.transform.Find("RightArrowButton").GetComponent<Button>();
        currentGraphicsQualityText = graphicsQualitySelectorPanel.transform.Find("CurrentSelectionText").GetComponent<Text>();

        currentGraphicsQualityIndex = 0;
        UpdateGraphicsQualityText();

        leftArrowGraphicsQualityButton.onClick.AddListener(SelectPreviousGraphicsQuality);
        rightArrowGraphicsQualityButton.onClick.AddListener(SelectNextGraphicsQuality);
    }

    private void InitializeAntiAliasingSelector()
    {
        if (antiAliasingSelectorPanel == null)
        {
            Debug.LogError("antiAliasingSelectorPanel is not assigned.");
            return;
        }

        leftArrowAntiAliasingButton = antiAliasingSelectorPanel.transform.Find("LeftArrowButton").GetComponent<Button>();
        rightArrowAntiAliasingButton = antiAliasingSelectorPanel.transform.Find("RightArrowButton").GetComponent<Button>();
        currentAntiAliasingText = antiAliasingSelectorPanel.transform.Find("CurrentSelectionText").GetComponent<Text>();

        currentAntiAliasingIndex = 0;
        UpdateAntiAliasingText();

        leftArrowAntiAliasingButton.onClick.AddListener(SelectPreviousAntiAliasing);
        rightArrowAntiAliasingButton.onClick.AddListener(SelectNextAntiAliasing);
    }

    private void SelectPreviousResolution()
    {
        if (currentResolutionIndex > 0)
        {
            currentResolutionIndex--;
            UpdateResolutionText();
            SetResolution(currentResolutionIndex);
        }
    }

    private void SelectNextResolution()
    {
        if (currentResolutionIndex < availableResolutions.Length - 1)
        {
            currentResolutionIndex++;
            UpdateResolutionText();
            SetResolution(currentResolutionIndex);
        }
    }

    private void UpdateResolutionText()
    {
        Resolution res = availableResolutions[currentResolutionIndex];
        currentResolutionText.text = $"{res.width} x {res.height}";
    }

    private void SelectPreviousRefreshRate()
    {
        if (currentRefreshRateIndex > 0)
        {
            currentRefreshRateIndex--;
            UpdateRefreshRateText();
            SetRefreshRate(currentRefreshRateIndex);
        }
    }

    private void SelectNextRefreshRate()
    {
        if (currentRefreshRateIndex < refreshRates.Length - 1)
        {
            currentRefreshRateIndex++;
            UpdateRefreshRateText();
            SetRefreshRate(currentRefreshRateIndex);
        }
    }

    private void UpdateRefreshRateText()
    {
        int rate = refreshRates[currentRefreshRateIndex];
        currentRefreshRateText.text = $"{rate} Hz";
    }

    private void SelectPreviousGraphicsQuality()
    {
        if (currentGraphicsQualityIndex > 0)
        {
            currentGraphicsQualityIndex--;
            UpdateGraphicsQualityText();
            SetGraphicsQuality(currentGraphicsQualityIndex);
        }
    }

    private void SelectNextGraphicsQuality()
    {
        if (currentGraphicsQualityIndex < graphicsQualities.Length - 1)
        {
            currentGraphicsQualityIndex++;
            UpdateGraphicsQualityText();
            SetGraphicsQuality(currentGraphicsQualityIndex);
        }
    }

    private void UpdateGraphicsQualityText()
    {
        string quality = graphicsQualities[currentGraphicsQualityIndex];
        currentGraphicsQualityText.text = quality;
    }

    private void SelectPreviousAntiAliasing()
    {
        if (currentAntiAliasingIndex > 0)
        {
            currentAntiAliasingIndex--;
            UpdateAntiAliasingText();
            SetAntiAliasing(currentAntiAliasingIndex);
        }
    }

    private void SelectNextAntiAliasing()
    {
        if (currentAntiAliasingIndex < antiAliasingOptions.Length - 1)
        {
            currentAntiAliasingIndex++;
            UpdateAntiAliasingText();
            SetAntiAliasing(currentAntiAliasingIndex);
        }
    }

    private void UpdateAntiAliasingText()
    {
        string option = antiAliasingOptions[currentAntiAliasingIndex];
        currentAntiAliasingText.text = option;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= availableResolutions.Length)
        {
            Debug.LogError("Invalid resolution index.");
            return;
        }

        Resolution resolution = availableResolutions[resolutionIndex];
        Debug.Log($"Setting resolution to {resolution.width} x {resolution.height} @ {resolution.refreshRateRatio.value}Hz");
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    public void SetRefreshRate(int refreshRateIndex)
    {
        if (refreshRateIndex < 0 || refreshRateIndex >= refreshRates.Length)
        {
            Debug.LogError("Invalid refresh rate index.");
            return;
        }

        int refreshRate = refreshRates[refreshRateIndex];
        Debug.Log($"Setting refresh rate to {refreshRate}Hz");

        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreenMode, currentResolution.refreshRateRatio);
        PlayerPrefs.SetInt("RefreshRate", refreshRateIndex);
    }


    public void SetGraphicsQuality(int qualityIndex)
    {
        Debug.Log($"Setting graphics quality to {qualityIndex}");
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
    }

    public void SetAntiAliasing(int antiAliasingIndex)
    {
        Debug.Log($"Setting anti-aliasing to {antiAliasingIndex}");
        switch (antiAliasingIndex)
        {
            case 0:
                urpCameraData.antialiasing = AntialiasingMode.None;
                break;
            case 1:
                urpCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                break;
            case 2:
                urpCameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
            default:
                Debug.LogError("Invalid anti-aliasing index.");
                return;
        }
        PlayerPrefs.SetInt("AntiAliasing", antiAliasingIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Debug.Log($"Setting fullscreen to {isFullScreen}");
        Screen.fullScreen = isFullScreen;
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
    }

    public void SetVSync(bool isVSync)
    {
        Debug.Log($"Setting VSync to {isVSync}");
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isVSync ? 1 : 0);
    }

    public void ResetToDefaults()
    {
        Debug.Log("Resetting to defaults...");

        int resolutionIndex = availableResolutions.ToList().FindIndex(res => res.width == 1920 && res.height == 1080);
        if (resolutionIndex == -1)
        {
            Debug.LogWarning("1920x1080 resolution not found, defaulting to first available resolution.");
            resolutionIndex = 0;
        }
        currentResolutionIndex = resolutionIndex;
        UpdateResolutionText();
        SetResolution(resolutionIndex);

        currentRefreshRateIndex = 0;
        UpdateRefreshRateText();
        SetRefreshRate(currentRefreshRateIndex);

        currentGraphicsQualityIndex = 0;
        UpdateGraphicsQualityText();
        SetGraphicsQuality(currentGraphicsQualityIndex);

        currentAntiAliasingIndex = 0;
        UpdateAntiAliasingText();
        SetAntiAliasing(currentAntiAliasingIndex);

        Debug.Log("Resetting FullScreen to default (true)...");
        fullScreenToggle.isOn = true;
        SetFullScreen(true);

        Debug.Log("Resetting VSync to default (true)...");
        vsyncToggle.isOn = true;
        SetVSync(true);
    }

    private void LoadSettings()
    {
        int defaultResolutionIndex = availableResolutions.ToList().FindIndex(res => res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height);
        int resolutionIndex = PlayerPrefs.GetInt("Resolution", defaultResolutionIndex);
        currentResolutionIndex = resolutionIndex;
        UpdateResolutionText();
        SetResolution(resolutionIndex);

        int qualityIndex = PlayerPrefs.GetInt("GraphicsQuality", 0);
        currentGraphicsQualityIndex = qualityIndex;
        UpdateGraphicsQualityText();
        SetGraphicsQuality(qualityIndex);

        int antiAliasingIndex = PlayerPrefs.GetInt("AntiAliasing", 0);
        currentAntiAliasingIndex = antiAliasingIndex;
        UpdateAntiAliasingText();
        SetAntiAliasing(antiAliasingIndex);

        bool isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        fullScreenToggle.isOn = isFullScreen;
        SetFullScreen(isFullScreen);

        bool isVSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        vsyncToggle.isOn = isVSync;
        SetVSync(isVSync);

        if (refreshRates == null || refreshRates.Length == 0)
        {
            Debug.LogError("No available refresh rates found in LoadSettings.");
            return;
        }

        int defaultRefreshRateIndex = refreshRates.ToList().FindIndex(rate => rate == (int)Screen.currentResolution.refreshRateRatio.value);
        if (defaultRefreshRateIndex == -1)
        {
            Debug.LogWarning("Default refresh rate not found, using index 0.");
            defaultRefreshRateIndex = 0;
        }

        int refreshRateIndex = PlayerPrefs.GetInt("RefreshRate", defaultRefreshRateIndex);
        currentRefreshRateIndex = refreshRateIndex;

        UpdateRefreshRateText();
        SetRefreshRate(refreshRateIndex);
    }
}
