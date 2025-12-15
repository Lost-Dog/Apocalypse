using JU;
using UnityEngine;
using UnityEngine.Events;

namespace JUTPS.GameSettings
{
    /// <summary>
    /// The game settings system, apply game configurations.
    /// </summary>
    public class JUGameSettings : MonoBehaviour
    {
        private static AudioListener _audioListener;

        private const string GRAPHICS_RENDER_SCALE_KEY = "SETTINGS_GRAPHICS_RENDER_SCALE";
        private const string GRAPHICS_QUALITY_KEY = "SETTINGS_GRAPHICS_QUALITY";
        private const string CONTROLS_CAMERA_INVERT_VERTICAL_KEY = "SETTINGS_CONTROLS_CAMERA_INVERT_VERTICAL";
        private const string CONTROLS_CAMERA_INVERT_HORIZONTAL_KEY = "SETTINGS_CONTROLS_CAMERA_INVERT_HORIZONTAL";
        private const string CONTROLS_CAMERA_SENSITIVE_KEY = "SETTINGS_CONTROLS_CAMERA_SENSITIVE";
        private const string AUDIO_GENERAL_VOLUME_KEY = "SETTINGS_GENERAL_AUDIO_VOLUME";

#if UNITY_ANDROID || UNITY_IOS
        private static bool IsMobile => true;
#else
        private static bool IsMobile => false;
#endif

        /// <summary>
        /// Called when the settings are changed.
        /// </summary>
        public static event UnityAction OnChangeSettings;

        private static AudioListener AudioListener
        {
            get
            {
                if (!_audioListener || !_audioListener.isActiveAndEnabled)
                    _audioListener = FindObjectOfType<AudioListener>(includeInactive: false);

                return _audioListener;
            }
        }

        /// <summary>
        /// The render scale resolution multiplier, a value between 0.1 and 1 based on the window size.
        /// </summary>
        public static float RenderScale
        {
            get
            {
                return PlayerPrefs.GetFloat(GRAPHICS_RENDER_SCALE_KEY, IsMobile ? 0.75f : 1f);
            }
            set
            {
                value = Mathf.Clamp(value, 0.1f, 1f);
                PlayerPrefs.SetFloat(GRAPHICS_RENDER_SCALE_KEY, value);

                ApplyRenderScale(value);

                OnChangeSettings?.Invoke();
            }
        }

        /// <summary>
        /// The current graphics settings.
        /// </summary>
        public static int GraphicsQuality
        {
            get
            {
                return PlayerPrefs.GetInt(GRAPHICS_QUALITY_KEY, QualitySettings.GetQualityLevel());
            }
            set
            {
                PlayerPrefs.SetInt(GRAPHICS_QUALITY_KEY, value);
                ApplyQuality(value);

                OnChangeSettings?.Invoke();
            }
        }

        /// <summary>
        /// Invert vertical camera orientation.
        /// </summary>
        public static bool CameraInvertVertical
        {
            get
            {
                return PlayerPrefs.GetInt(CONTROLS_CAMERA_INVERT_VERTICAL_KEY, 0) == 1 ? true : false;
            }
            set
            {
                PlayerPrefs.SetInt(CONTROLS_CAMERA_INVERT_VERTICAL_KEY, value ? 1 : 0);
                OnChangeSettings?.Invoke();
            }
        }

        /// <summary>
        /// Invert horizontal camera orientation.
        /// </summary>
        public static bool CameraInvertHorizontal
        {
            get
            {
                return PlayerPrefs.GetInt(CONTROLS_CAMERA_INVERT_HORIZONTAL_KEY, 0) == 1 ? true : false;
            }
            set
            {
                PlayerPrefs.SetInt(CONTROLS_CAMERA_INVERT_HORIZONTAL_KEY, value ? 1 : 0);
                OnChangeSettings?.Invoke();
            }
        }

        /// <summary>
        /// The camera rotation sensibility with user inputs.
        /// </summary>
        public static float CameraSensibility
        {
            get
            {
                return PlayerPrefs.GetFloat(CONTROLS_CAMERA_SENSITIVE_KEY, 1f);
            }
            set
            {
                if (value == CameraSensibility)
                    return;

                value = Mathf.Min(value, 10);
                PlayerPrefs.SetFloat(CONTROLS_CAMERA_SENSITIVE_KEY, value);
                OnChangeSettings?.Invoke();
            }
        }

        /// <summary>
        /// The game audio volume.
        /// </summary>
        public static float AudioGeneralVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(AUDIO_GENERAL_VOLUME_KEY, 1f);
            }
            set
            {
                if (value == AudioGeneralVolume)
                    return;

                value = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(AUDIO_GENERAL_VOLUME_KEY, value);

                ApplyGeneralVolume(value);

                OnChangeSettings?.Invoke();
            }
        }

        private void Awake()
        {
            ApplySettings();
        }

        /// <summary>
        /// Apply the game settings.
        /// </summary>
        public static void ApplySettings()
        {
            ApplyRenderScale(RenderScale);
            ApplyQuality(GraphicsQuality);
            ApplyGeneralVolume(AudioGeneralVolume);

            OnChangeSettings?.Invoke();
        }

        /// <summary>
        /// Set the volume for a specific audio type, like music, sfx, ui...
        /// </summary>
        /// <param name="audioTag"></param>
        /// <returns></returns>
        public static void SetAudioVolume(JUTag audioTag, float volume)
        {
            Debug.Assert(audioTag, "Audio Tag missing");
            PlayerPrefs.SetFloat(GetAudioVolumeKey(audioTag), volume);
            OnChangeSettings?.Invoke();
        }

        /// <summary>
        /// Gets the volume for a specific audio type, like music, sfx, ui...
        /// </summary>
        /// <param name="audioTag"></param>
        /// <returns></returns>
        public static float GetAudioVolume(JUTag audioTag)
        {
            Debug.Assert(audioTag, "Audio Tag missing");
            return PlayerPrefs.GetFloat(GetAudioVolumeKey(audioTag), 1f);
        }

        private static string GetAudioVolumeKey(JUTag tag)
        {
            return $"SETTINGS_AUDIO_VOLUME_{tag.name}";
        }

        private static void ApplyRenderScale(float scale)
        {
            Resolution biggestResolution = Screen.resolutions[Screen.resolutions.Length - 1];
            Resolution currentResolution = Screen.currentResolution;
            Resolution targetResolution = new Resolution()
            {
                height = (int)(biggestResolution.height * scale),
                width = (int)(biggestResolution.width * scale),

#if UNITY_2022_3_OR_NEWER
                    refreshRateRatio = currentResolution.refreshRateRatio
#else
                refreshRate = currentResolution.refreshRate
#endif

            };

            //On mobile devices the width and height are inverted
            if (!IsMobile)
            {
#if UNITY_2022_3_OR_NEWER
                    Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreenMode, targetResolution.refreshRateRatio);
#else
                Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreen, targetResolution.refreshRate);
#endif
            }
            else
            {
#if UNITY_2022_3_OR_NEWER
                    Screen.SetResolution(targetResolution.height, targetResolution.width, Screen.fullScreenMode, targetResolution.refreshRateRatio);
#else
                Screen.SetResolution(targetResolution.height, targetResolution.width, Screen.fullScreen, targetResolution.refreshRate);
#endif
            }
        }

        private static void ApplyQuality(int value)
        {
            QualitySettings.SetQualityLevel(value);
        }

        private static void ApplyGeneralVolume(float volume)
        {
            if (AudioListener)
                AudioListener.volume = volume;
        }

        /// <summary>
        /// Reset Game Settings.
        /// Delete all playerprefs.
        /// </summary>
        [ContextMenu("Reset Game Settings", false, 100)]
        public void ResetSettings()
        {
            PlayerPrefs.DeleteAll();

            if (Application.isPlaying)
                ApplySettings();
        }
    }
}