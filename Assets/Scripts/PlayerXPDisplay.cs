using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's current XP, XP-to-next-level threshold, and a fill bar.
/// Polls <see cref="ProgressionManager"/> once per second to avoid per-frame string allocations.
/// </summary>
public class PlayerXPDisplay : MonoBehaviour
{
    private const string XpFormat        = "XP  {0:N0}  /  {1:N0}";
    private const string LevelFormat     = "{0}";

    [SerializeField] private TextMeshProUGUI xpLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private Image           xpBarFill;

    private int   _lastXP    = -1;
    private int   _lastLevel = -1;
    private ProgressionManager _pm;

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        // Lazy bind in case manager is created after this UI object.
        if (_pm == null)
            TrySubscribe();
    }

    private void OnDestroy()
    {
        if (_pm != null)
        {
            _pm.onXPGained.RemoveListener(OnXPGained);
            _pm.onLevelUp.RemoveListener(OnLevelUp);
        }
    }

    private void TrySubscribe()
    {
        if (_pm != null) return;

        ProgressionManager pm = ProgressionManager.Instance;
        if (pm == null) return;

        _pm = pm;
        _pm.onXPGained.AddListener(OnXPGained);
        _pm.onLevelUp.AddListener(OnLevelUp);
        Refresh();
    }

    private void OnXPGained(int amount)
    {
        Refresh();
    }

    private void OnLevelUp(int level)
    {
        Refresh();
    }

    private void Refresh()
    {
        ProgressionManager pm = _pm ?? ProgressionManager.Instance;
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
