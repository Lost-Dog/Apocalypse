using JUTPS;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Load and save data for <see cref="JUHealth"/>.
    /// </summary>
    [RequireComponent(typeof(JUHealth))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Health")]
    public class JUSaveLoadHealth : JUSaveLoadComponent
    {
        private JUHealth _health;

        private const string VALUE_KEY = "Health";
        private const string MAX_VALUE_KEY = "Max Health";

        /// <inheritdoc/>
        public JUSaveLoadHealth() : base()
        {
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            _health = GetComponent<JUHealth>();

            base.Awake();
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(VALUE_KEY, _health.Health);
            SetValue(MAX_VALUE_KEY, _health.MaxHealth);
        }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            _health.Health = GetValue(VALUE_KEY, _health.MaxHealth);
            _health.MaxHealth = GetValue(MAX_VALUE_KEY, _health.MaxHealth);
            _health.CheckHealthState();
        }

        /// <inheritdoc/>
        protected override void OnExitPlayMode()
        {
            base.OnExitPlayMode();

            _health = null;
        }
    }
}