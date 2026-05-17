using System;
using System.Collections;
using Invector.Throw;
using UnityEngine;

namespace Apocalypse
{
    /// <summary>
    /// Drop-in replacement for <see cref="vThrowManager"/> that:
    /// <list type="bullet">
    ///   <item>Fires <see cref="onCountChanged"/> whenever throw count changes.</item>
    ///   <item>Enforces a configurable cooldown after each throw, during which
    ///         <see cref="EnterThrowMode"/> is blocked.</item>
    ///   <item>Fires <see cref="onCooldownStarted"/> and <see cref="onCooldownEnded"/>
    ///         so UI systems can react without polling.</item>
    /// </list>
    /// </summary>
    public class vThrowManagerObservable : vThrowManager
    {
        [Header("Cooldown")]
        [Tooltip("Seconds the player must wait after a throw before using another grenade.")]
        [SerializeField] private float cooldownDuration = 30f;

        // ── Public state ─────────────────────────────────────────────────────────

        public bool  IsCoolingDown    { get; private set; }

        /// <summary>Normalised [0..1] — 1 = just thrown, 0 = ready. Safe to read every frame.</summary>
        public float CooldownProgress { get; private set; }

        public float CooldownDuration => cooldownDuration;

        // ── Events ───────────────────────────────────────────────────────────────

        /// <summary>Fired whenever the throw count or active throwable changes.</summary>
        public event Action<int, int> onCountChanged;

        /// <summary>Fired once when the cooldown begins, immediately after a throw.</summary>
        public event Action onCooldownStarted;

        /// <summary>Fired once when the cooldown expires and the player can throw again.</summary>
        public event Action onCooldownEnded;

        // ── Overrides ────────────────────────────────────────────────────────────

        protected override IEnumerator Start()
        {
            yield return base.Start();
            onThrowObject.AddListener(TriggerCooldown);
        }

        /// <summary>Blocks entry into throw-aim mode while the cooldown is active.</summary>
        protected override void EnterThrowMode()
        {
            if (IsCoolingDown) return;
            base.EnterThrowMode();
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            onCountChanged?.Invoke(CurrentThrowAmount, MaxThrowObjects);
        }

        // ── Cooldown coroutine ───────────────────────────────────────────────────

        private void TriggerCooldown()
        {
            if (!IsCoolingDown)
                StartCoroutine(RunCooldown());
        }

        private IEnumerator RunCooldown()
        {
            IsCoolingDown     = true;
            CooldownProgress  = 1f;
            onCooldownStarted?.Invoke();

            float elapsed = 0f;
            while (elapsed < cooldownDuration)
            {
                elapsed          += Time.deltaTime;
                CooldownProgress  = 1f - Mathf.Clamp01(elapsed / cooldownDuration);
                yield return null;
            }

            CooldownProgress = 0f;
            IsCoolingDown    = false;
            onCooldownEnded?.Invoke();
        }
    }
}
