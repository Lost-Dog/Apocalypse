using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 0, 1)]
    
    [Title("Change Audio Clip")]
    [Description("Sets the Audio Clip value")]

    [Category("Audio/Change Audio Clip")]

    [Parameter("To", "Where to store the new Audio Clip value")]
    [Parameter("Audio Clip", "The Audio Clip value to be stored")]

    [Keywords("Audio", "Clip", "Sound", "Music")]
    [Image(typeof(IconAudioClip), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class InstructionSetAudioClip : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] protected PropertySetAudio m_To = new PropertySetAudio();
        [SerializeField] private PropertyGetAudio m_AudioClip = new PropertyGetAudio();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Set {this.m_To} = {this.m_AudioClip}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            AudioClip audioClip = this.m_AudioClip.Get(args);
            this.m_To.Set(audioClip, args);
            
            return DefaultResult;
        }
    }
}