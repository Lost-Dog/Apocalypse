using TMPro;
using UnityEngine;

namespace Apocalypse
{
    /// <summary>
    /// Updates a TMP label whenever the grenade count actually changes.
    /// Requires <see cref="vThrowManagerObservable"/> on the player — subscribe
    /// to its <c>onCountChanged</c> event instead of polling every frame.
    /// </summary>
    public class GrenadeCountDisplay : MonoBehaviour
    {
        [Tooltip("TextMeshProUGUI label that shows the grenade count.")]
        public TextMeshProUGUI countLabel;

        [Tooltip("Root of the player GameObject that owns the vThrowManagerObservable.")]
        public GameObject throwManagerRoot;

        [Tooltip("Format string passed to string.Format. {0} = current amount, {1} = max amount.")]
        public string format = "x{0}";

        // ── Private state ────────────────────────────────────────────────────────

        private vThrowManagerObservable _throwManager;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Start()
        {
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // ── Subscription ─────────────────────────────────────────────────────────

        private void Subscribe()
        {
            _throwManager = ResolveThrowManager();
            if (_throwManager == null)
                return;

            _throwManager.onCountChanged += OnCountChanged;

            // Sync label immediately with the current state.
            OnCountChanged(_throwManager.CurrentThrowAmount, _throwManager.MaxThrowObjects);
        }

        private void Unsubscribe()
        {
            if (_throwManager != null)
                _throwManager.onCountChanged -= OnCountChanged;
        }

        // ── Handler ───────────────────────────────────────────────────────────────

        private void OnCountChanged(int current, int max)
        {
            if (countLabel != null)
                countLabel.text = string.Format(format, current, max);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private vThrowManagerObservable ResolveThrowManager()
        {
            vThrowManagerObservable result = null;

            if (throwManagerRoot != null)
                result = throwManagerRoot.GetComponentInChildren<vThrowManagerObservable>(includeInactive: true);

            if (result == null)
                result = FindFirstObjectByType<vThrowManagerObservable>();

            if (result == null)
                Debug.LogWarning("[GrenadeCountDisplay] vThrowManagerObservable not found. " +
                                 "Replace vThrowManager on the player with vThrowManagerObservable.");
            return result;
        }
    }
}
