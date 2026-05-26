using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's current XP, XP-to-next-level threshold, and a fill bar.
/// Polls <see cref="ProgressionManager"/> once per second to avoid per-frame string allocations.
/// </summary>
public class PlayerXPDisplay : MonoBehaviour
{
    private const float RefreshInterval  = 1f;
    private const string XpFormat        = "XP  {0:N0}  /  {1:N0}";
    private const string LevelFormat     = "{0}";

    [SerializeField] private TextMeshProUGUI xpLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private Image           xpBarFill;

    private int   _lastXP    = -1;
    private int   _lastLevel = -1;
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
        ProgressionManager pm = ProgressionManager.Instance;
        if (pm == null) return;

        int currentXP = pm.currentXP;
        int level     = pm.currentLevel;

        if (currentXP == _lastXP && level == _lastLevel) return;

        _lastXP    = currentXP;
        _lastLevel = level;

        int xpToNext = pm.GetRequiredXPForLevel(level);

        if (xpLabel != null)
            xpLabel.text = string.Format(XpFormat, currentXP, xpToNext);

        if (levelLabel != null)
            levelLabel.text = string.Format(LevelFormat, level);

        if (xpBarFill != null)
            xpBarFill.fillAmount = pm.GetXPProgress();
    }
}
