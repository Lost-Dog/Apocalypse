using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class VisualsManagerURP : MonoBehaviour
{
    [Header("Volume & Toggles")]
    public VolumeProfile urpProfile;
    public Toggle motionBlurToggle;
    public Toggle filmGrainToggle;
    public Toggle chromaticAberrationToggle;

    [Header("Sliders")]
    public Slider gammaSlider;
    public TextMeshProUGUI gammaValueText;
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessValueText;

    [Header("Tonemapping Selector")]
    public GameObject tonemappingSelectorPanel;

    public Button resetButton;

    private MotionBlur motionBlur;
    private FilmGrain filmGrain;
    private ChromaticAberration chromaticAberration;
    private LiftGammaGain liftGammaGain;
    private ColorAdjustments colorAdjustments;
    private Tonemapping tonemapping;

    private float defaultGammaAlpha = 1f;
    private float defaultExposureValue = 0f;
    private int defaultTonemappingIndex = 0;

    private Button leftArrowTonemappingButton;
    private Button rightArrowTonemappingButton;
    private TextMeshProUGUI currentTonemappingText;
    private readonly string[] tonemappingOptions = { "None", "Neutral", "ACES" };
    private int currentTonemappingIndex;

    void Start()
    {
        if (urpProfile != null)
        {
            urpProfile.TryGet(out motionBlur);
            urpProfile.TryGet(out filmGrain);
            urpProfile.TryGet(out chromaticAberration);
            urpProfile.TryGet(out liftGammaGain);
            urpProfile.TryGet(out colorAdjustments);
            urpProfile.TryGet(out tonemapping);
        }

        if (liftGammaGain != null)
            liftGammaGain.gamma.overrideState = true;
        if (colorAdjustments != null)
            colorAdjustments.postExposure.overrideState = true;
        if (tonemapping != null)
            tonemapping.mode.overrideState = true;

        if (motionBlur != null)
            motionBlurToggle.isOn = motionBlur.active;
        if (filmGrain != null)
            filmGrainToggle.isOn = filmGrain.active;
        if (chromaticAberration != null)
            chromaticAberrationToggle.isOn = chromaticAberration.active;

        if (liftGammaGain != null && gammaSlider != null)
        {
            gammaSlider.minValue = -1f;
            gammaSlider.maxValue = 1f;
            gammaSlider.wholeNumbers = false;

            defaultGammaAlpha = liftGammaGain.gamma.value.w;
            gammaSlider.value = defaultGammaAlpha;
            gammaSlider.onValueChanged.AddListener(SetGamma);
            UpdateGammaText(defaultGammaAlpha);
        }

        if (colorAdjustments != null && brightnessSlider != null)
        {
            brightnessSlider.minValue = -5f;
            brightnessSlider.maxValue = 5f;
            brightnessSlider.wholeNumbers = false;

            defaultExposureValue = colorAdjustments.postExposure.value;
            brightnessSlider.value = defaultExposureValue;
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
            UpdateBrightnessText(defaultExposureValue);
        }

        InitializeTonemappingSelector();

        motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
        filmGrainToggle.onValueChanged.AddListener(SetFilmGrain);
        chromaticAberrationToggle.onValueChanged.AddListener(SetChromaticAberration);

        resetButton.onClick.AddListener(ResetValues);
    }

    private void InitializeTonemappingSelector()
    {
        if (tonemappingSelectorPanel == null || tonemapping == null)
        {
            Debug.LogError("Tonemapping selector panel or Tonemapping component not assigned.");
            return;
        }

        leftArrowTonemappingButton = tonemappingSelectorPanel.transform.Find("LeftArrowButton").GetComponent<Button>();
        rightArrowTonemappingButton = tonemappingSelectorPanel.transform.Find("RightArrowButton").GetComponent<Button>();
        currentTonemappingText = tonemappingSelectorPanel.transform.Find("CurrentSelectionText").GetComponent<TextMeshProUGUI>();

        // Determine default index
        switch (tonemapping.mode.value)
        {
            case TonemappingMode.None: defaultTonemappingIndex = 0; break;
            case TonemappingMode.Neutral: defaultTonemappingIndex = 1; break;
            case TonemappingMode.ACES: defaultTonemappingIndex = 2; break;
            default: defaultTonemappingIndex = 0; break;
        }
        currentTonemappingIndex = defaultTonemappingIndex;

        UpdateTonemappingText();
        leftArrowTonemappingButton.onClick.AddListener(SelectPreviousTonemapping);
        rightArrowTonemappingButton.onClick.AddListener(SelectNextTonemapping);
    }

    private void SelectPreviousTonemapping()
    {
        if (currentTonemappingIndex > 0)
        {
            currentTonemappingIndex--;
            UpdateTonemappingText();
            SetTonemapping(currentTonemappingIndex);
        }
    }

    private void SelectNextTonemapping()
    {
        if (currentTonemappingIndex < tonemappingOptions.Length - 1)
        {
            currentTonemappingIndex++;
            UpdateTonemappingText();
            SetTonemapping(currentTonemappingIndex);
        }
    }

    private void UpdateTonemappingText()
    {
        if (currentTonemappingText != null)
            currentTonemappingText.text = tonemappingOptions[currentTonemappingIndex];
    }

    private void SetTonemapping(int index)
    {
        if (tonemapping != null)
        {
            tonemapping.mode.overrideState = true;
            switch (index)
            {
                case 0: tonemapping.mode.value = TonemappingMode.None; break;
                case 1: tonemapping.mode.value = TonemappingMode.Neutral; break;
                case 2: tonemapping.mode.value = TonemappingMode.ACES; break;
            }
        }
    }

    void SetMotionBlur(bool isOn) => motionBlur.active = isOn;
    void SetFilmGrain(bool isOn) => filmGrain.active = isOn;
    void SetChromaticAberration(bool isOn) => chromaticAberration.active = isOn;
    void SetGamma(float value)
    {
        liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, value));
        UpdateGammaText(value);
    }
    void SetBrightness(float value)
    {
        colorAdjustments.postExposure.Override(value);
        UpdateBrightnessText(value);
    }

    void UpdateGammaText(float value) { gammaValueText.text = value.ToString("F2"); }
    void UpdateBrightnessText(float value) { brightnessValueText.text = value.ToString("F2"); }

    void ResetValues()
    {
        if (motionBlur != null && motionBlurToggle != null)
        {
            motionBlur.active = true;
            motionBlurToggle.isOn = true;
        }
        if (filmGrain != null && filmGrainToggle != null)
        {
            filmGrain.active = true;
            filmGrainToggle.isOn = true;
        }
        if (chromaticAberration != null && chromaticAberrationToggle != null)
        {
            chromaticAberration.active = true;
            chromaticAberrationToggle.isOn = true;
        }

        if (liftGammaGain != null && gammaSlider != null)
        {
            liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, defaultGammaAlpha));
            gammaSlider.value = defaultGammaAlpha;
            UpdateGammaText(defaultGammaAlpha);
        }

        if (colorAdjustments != null && brightnessSlider != null)
        {
            colorAdjustments.postExposure.Override(defaultExposureValue);
            brightnessSlider.value = defaultExposureValue;
            UpdateBrightnessText(defaultExposureValue);
        }

        if (tonemapping != null)
        {
            currentTonemappingIndex = defaultTonemappingIndex;
            UpdateTonemappingText();
            SetTonemapping(currentTonemappingIndex);
        }
    }
}