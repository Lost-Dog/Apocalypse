using TMPro;
using UnityEngine;

/// <summary>
/// Displays the player's current level sourced from <see cref="PlayerTraitsRuntime"/>.
/// Refreshes once per second to avoid per-frame string allocation overhead.
/// </summary>
public class PlayerLevelDisplay : MonoBehaviour
{
    private const float RefreshInterval = 1f;
    private const string LabelFormat    = "LVL {0}";

    [SerializeField] private TextMeshProUGUI levelLabel;

    private int   _lastLevel    = -1;
    private float _refreshTimer;

    private void Update()
    {
        _refreshTimer += Time.deltaTime;
        if (_refreshTimer < RefreshInterval) return;
        _refreshTimer = 0f;
        Refresh();
    }

    private void Refresh()
    {
        if (PlayerTraitsRuntime.Instance == null) return;

        int level = PlayerTraitsRuntime.Instance.CurrentLevel;
        if (level == _lastLevel) return;

        _lastLevel = level;
        levelLabel.text = string.Format(LabelFormat, level);
    }
}
