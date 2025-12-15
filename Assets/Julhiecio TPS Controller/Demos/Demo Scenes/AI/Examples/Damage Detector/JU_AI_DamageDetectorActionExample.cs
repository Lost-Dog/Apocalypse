using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="DamageDetector"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Damage Detector Area Action")]
    public class JU_AI_DamageDetectorActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The damage detector.
        /// </summary>
        public DamageDetector DamageDetector;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            DamageDetector.Setup(this);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            DamageDetector.Unsetup();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();
            DamageDetector.Update(ref control);

            Control = control;
        }
    }
}