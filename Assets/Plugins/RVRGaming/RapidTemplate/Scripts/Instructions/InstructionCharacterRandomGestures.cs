using System;
using System.Threading;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 0)]
    [Title("Play Random Gestures")]
    [Description("Plays a sequence of random Animation Clips on a Character one after the other")]
    [Category("Characters/Animation/Play Random Gestures")]

    [Parameter("Character", "The character that plays the animations")]
    [Parameter("Animation Clips", "A list of Animation Clips to choose from randomly")]
    [Parameter("Gesture Count", "The number of random gestures to play in sequence")]
    [Parameter("Avatar Mask", "(Optional) Allows playing the animation on specific body parts of the Character")]
    [Parameter("Blend Mode", "Either additively adds the new animation on top of others or overrides lower layer animations")]
    [Parameter("Delay", "Seconds to wait before the animation starts to play")]
    [Parameter("Speed", "Speed coefficient at which the animation plays. 1 means normal speed")]
    [Parameter("Use Root Motion", "Determines if the gesture applies root motion")]
    [Parameter("Transition In", "Seconds the animation takes to blend in")]
    [Parameter("Transition Out", "Seconds the animation takes to blend out")]
    [Parameter("Wait To Complete", "If true, waits until each animation completes before playing the next")]

    [Keywords("Characters", "Animation", "Animate", "Gesture", "Play", "Random")]
    [Image(typeof(IconCharacterGesture), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionCharacterRandomGestures : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [Space]
        [SerializeField] private PropertyGetAnimation[] m_AnimationClips;
        [SerializeField] private PropertyGetDecimal m_GestureCount = new PropertyGetDecimal(1);

        [SerializeField] private AvatarMask m_AvatarMask = null;
        [SerializeField] private BlendMode m_BlendMode = BlendMode.Blend;
        [SerializeField] private PropertyGetDecimal m_Delay = GetDecimalConstantZero.Create;
        [SerializeField] private PropertyGetDecimal m_Speed = GetDecimalConstantOne.Create;
        [SerializeField] private bool m_UseRootMotion = false;
        [SerializeField] private PropertyGetDecimal m_TransitionIn = new PropertyGetDecimal(0.1f);
        [SerializeField] private PropertyGetDecimal m_TransitionOut = new PropertyGetDecimal(0.1f);

        [Space]
        [SerializeField] private bool m_WaitToComplete = true;

        public override string Title => $"Random Gestures on {this.m_Character}";

        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null)
            {
                Debug.LogError("Character is null");
                return;
            }

            if (character.Gestures == null)
            {
                Debug.LogError("Character does not have a Gestures component!");
                return;
            }

            if (this.m_AnimationClips == null || this.m_AnimationClips.Length == 0)
            {
                Debug.LogError("No Animation Clips assigned");
                return;
            }

            int gestureCount = (int)this.m_GestureCount.Get(args);
            if (gestureCount <= 0) return;

            RandomGestureController.Start(character);
            CancellationToken token = RandomGestureController.GetToken(character);

            for (int i = 0; i < gestureCount; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Debug.Log("Random gesture sequence cancelled before starting gesture #" + i);
                    break;
                }

                int randomIndex = UnityEngine.Random.Range(0, this.m_AnimationClips.Length);

                if (this.m_AnimationClips[randomIndex] == null)
                {
                    Debug.LogWarning($"Animation clip property at index {randomIndex} is not assigned. Skipping this gesture.");
                    continue;
                }

                AnimationClip animationClip = this.m_AnimationClips[randomIndex].Get(args);
                if (animationClip == null)
                {
                    Debug.LogWarning($"Animation clip at index {randomIndex} is null. Skipping this gesture.");
                    continue;
                }

                ConfigGesture configuration = new ConfigGesture(
                    (float)this.m_Delay.Get(args),
                    animationClip.length,
                    (float)this.m_Speed.Get(args),
                    this.m_UseRootMotion,
                    (float)this.m_TransitionIn.Get(args),
                    (float)this.m_TransitionOut.Get(args)
                );

                Task gestureTask = character.Gestures.CrossFade(
                    animationClip, this.m_AvatarMask, this.m_BlendMode,
                    configuration, false
                );

                if (this.m_WaitToComplete)
                {
                    await gestureTask;

                    if (token.IsCancellationRequested)
                    {
                        Debug.Log("Cancellation requested after gesture #" + i + ". Stopping current gesture.");
                        character.Gestures.Stop(0f, (float)this.m_TransitionOut.Get(args));
                        break;
                    }
                }
            }
        }
    }
}
