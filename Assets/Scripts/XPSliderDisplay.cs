using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a <see cref="Slider"/> to reflect XP progress toward the next level.
/// Subscribes to <see cref="ProgressionManager.onXPGained"/> and
/// <see cref="ProgressionManager.onLevelUp"/> for instant, event-driven updates.
/// Falls back to lazy registration each frame until the manager is ready.
/// </summary>
[RequireComponent(typeof(Slider))]
public class XPSliderDisplay : MonoBehaviour
{
    // ── Private state ──────────────────────────────────────────────────────────

    private Slider             _slider;
    private ProgressionManager _pm;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.minValue        = 0f;
        _slider.maxValue        = 1f;
        _slider.wholeNumbers    = false;
        _slider.interactable    = false;
    }

    private void Update()
    {
        if (_pm == null)
            TrySubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    // ── Subscription ───────────────────────────────────────────────────────────

    private void TrySubscribe()
    {
        ProgressionManager pm = ProgressionManager.Instance;
        if (pm == null) return;

        _pm = pm;
        _pm.onXPGained.AddListener(OnXPGained);
        _pm.onLevelUp.AddListener(OnLevelUp);
        Refresh();
    }

    private void Unsubscribe()
    {
        if (_pm == null) return;
        _pm.onXPGained.RemoveListener(OnXPGained);
        _pm.onLevelUp.RemoveListener(OnLevelUp);
    }

    // ── Handlers ───────────────────────────────────────────────────────────────

    private void OnXPGained(int xpAmount) => Refresh();

    private void OnLevelUp(int newLevel)   => Refresh();

    // ── Refresh ────────────────────────────────────────────────────────────────

    /// <summary>Pushes the current [0,1] XP progress to the slider.</summary>
    private void Refresh()
    {
        if (_pm == null || _slider == null) return;
        _slider.value = _pm.GetXPProgress();
    }
}
