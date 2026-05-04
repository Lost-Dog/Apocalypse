using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Change Music volume +")]
    [Description("Change the Volume of Music over time")]

    [Category("Audio/Change Music volume +")]

    [Parameter("Volume", "A value between 0 and 1 that indicates the target volume percentage")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The change rate of the parameter over time")]
    [Parameter("Wait to Complete", "Whether to wait until the transition is finished")]

    [Keywords("Audio", "Music", "Background", "Volume", "Level")]
    [Image(typeof(IconVolume), ColorTheme.Type.Green)]

    [Serializable]
    public class InstructionCommonAudioVolumeMusicPlus : Instruction
    {
        public PropertyGetDecimal m_Volume = new PropertyGetDecimal(1f);
        public Transition m_Transition = new Transition(TimeMode.UpdateMode.UnscaledTime);

        public override string Title => $"Change Music volume to {this.m_Volume} over time";

        protected override async Task Run(Args args)
        {
            float valueSource = AudioManager.Instance.Volume.Music;
            float valueTarget = (float)this.m_Volume.Get(args);

            ITweenInput tween = new TweenInput<float>(
                valueSource,
                valueTarget,
                this.m_Transition.Duration,
                (a, b, t) => AudioManager.Instance.Volume.Music = Mathf.Lerp(a, b, t),
                Tween.GetHash(typeof(AudioManager), "music-volume"),
                this.m_Transition.EasingType,
                this.m_Transition.Time
            );

            Tween.To(AudioManager.Instance.gameObject, tween);
            if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
        }
    }
}
