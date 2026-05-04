using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Audio Source Clip")]
    [Category("Game Objects/Audio Source Clip")]

    [Image(typeof(IconAudioSource), ColorTheme.Type.Yellow)]
    [Description("The Audio Clip value attached to the Audio Source")]

    [Serializable]
    public class SetAudioSource : PropertyTypeSetAudio
    {
        [SerializeField]
        private PropertyGetGameObject m_AudioSource = GetGameObjectInstance.Create();

        public override void Set(AudioClip value, Args args)
        {
            AudioSource audioSource = this.m_AudioSource.Get<AudioSource>(args);
            if (audioSource == null) return;

            audioSource.clip = value;
        }

        public override AudioClip Get(Args args)
        {
            AudioSource audioSource = this.m_AudioSource.Get<AudioSource>(args);
            return audioSource != null ? audioSource.clip : null;
        }

        public override string String => $"{this.m_AudioSource} Clip";
    }
}