using System;
using System.Text.RegularExpressions;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Serializable]
    public class Typewriter
    {
        private static readonly Regex RX_STRIP = new Regex("<.*?>", RegexOptions.Compiled);
        
        [SerializeField] private bool m_UseTypewriter = true;
        [SerializeField] private int m_Frequency = 10;
        [SerializeField] private PropertyGetAudio m_Gibberish = GetAudioNone.Create;
        [SerializeField] private Vector2 m_Pitch = new Vector2(0.9f, 1.1f);

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector2 Pitch => this.m_Pitch;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float GetDuration(string text)
        {
            text = StripTags(text);
            return this.m_UseTypewriter
                ? text.Length / (float) this.m_Frequency
                : 0f;
        }

        public int GetCharactersVisible(float startTime, TimeMode timeMode)
        {
            float time = timeMode.Time - startTime;
            return this.m_UseTypewriter 
                ? Mathf.FloorToInt(time * this.m_Frequency) 
                : int.MaxValue;
        }

        public AudioClip GetGibberish(Args args) => this.m_Gibberish.Get(args);
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        public static string StripTags(string input)
        {
            return !string.IsNullOrEmpty(input)
                ? RX_STRIP.Replace(input, string.Empty)
                : string.Empty;
        }
    }
}