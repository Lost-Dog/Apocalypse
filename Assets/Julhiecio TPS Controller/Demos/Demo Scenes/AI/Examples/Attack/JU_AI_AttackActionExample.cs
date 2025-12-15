using System.ComponentModel;
using UnityEngine;

namespace JU.CharacterSystem.AI.Examples
{
    /// <summary>
    /// Example of <see cref="Attack"/> AI action.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Examples/JU AI Attack Action Example")]
    public class JU_AI_AttackActionExample : JUCharacterAIBase
    {
        /// <summary>
        /// The attack action.
        /// </summary>
        public Attack Attack;

        /// <summary>
        /// The target to attack.
        /// </summary>
        public GameObject Target;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            Attack.Setup(this);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            AIControlData control = new AIControlData();

            // Attack the target.
            Attack.Update(Target, ref control);

            Control = control;
        }
    }
}