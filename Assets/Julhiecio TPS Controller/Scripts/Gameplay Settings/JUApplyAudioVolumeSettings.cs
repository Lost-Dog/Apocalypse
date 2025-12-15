using JU;
using UnityEngine;

namespace JUTPS.GameSettings
{
    /// <summary>
    /// Apply the <seealso cref="JUTPS.GameSettings.JUGameSettings.AudioGeneralVolume"/> to an AudioSource.
    /// </summary>
    public class JUApplyAudioVolumeSettings : MonoBehaviour
    {
        /// <summary>
        /// The type of the audio, like Music, UI or Gameplay.
        /// </summary>
        public JUTag AudioTag;

        /// <summary>
        /// The audio source that will receive the volume settings.
        /// </summary>
        public AudioSource AudioSource;

        private void Reset()
        {
#if UNITY_EDITOR
            AudioSource = GetComponent<AudioSource>();
            AudioTag = UnityEditor.AssetDatabase.LoadAssetAtPath<JUTag>("Assets/Julhiecio TPS Controller/Audio/SFX Audio Tag.asset");
#endif
        }

        private void Awake()
        {
            Debug.Assert(AudioTag, $"The {nameof(JUApplyAudioVolumeSettings)} from gameObject {name} does not have an audio tag.");

            JUGameSettings.OnChangeSettings += ApplySettings;
        }

        private void OnDestroy()
        {
            JUGameSettings.OnChangeSettings -= ApplySettings;
        }

        private void OnEnable()
        {
            ApplySettings();
        }

        /// <summary>
        /// Call to sync the audio volume with the <seealso cref="JUTPS.GameSettings.JUGameSettings.AudioGeneralVolume"/>.
        /// </summary>
        public void ApplySettings()
        {
            if (!AudioSource || !AudioTag)
                return;

            AudioSource.volume = JUGameSettings.GetAudioVolume(AudioTag);
        }
    }
}