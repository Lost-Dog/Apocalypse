using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Audio Clip")]
    [Category("Audio Clip")]
    
    [Image(typeof(IconQuaver), ColorTheme.Type.Yellow)]
    [Description("An Audio Clip asset")]

    [Serializable] [HideLabelsInEditor]
    public class GetAudioClipAudioSource : PropertyTypeGetAudio
    {
        [SerializeField]
        protected PropertyGetGameObject m_AudioSource = GetGameObjectInstance.Create();

        public override AudioClip Get(Args args)
        {
            AudioSource audioSource = this.m_AudioSource.Get<AudioSource>(args);
            return audioSource != null ? audioSource.clip : null;
        }

        public static PropertyGetAudio Create => new PropertyGetAudio(
            new GetAudioClipAudioSource()
        );

        public override string String => $"{this.m_AudioSource} Clip";
    }
}