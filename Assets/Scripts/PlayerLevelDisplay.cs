using TMPro;
using UnityEngine;

/// <summary>
/// Displays the player's current level, driven entirely by <see cref="ProgressionManager.onLevelUp"/>.
/// No polling — the label updates only when a level-up event is fired.
/// </summary>
public class PlayerLevelDisplay : MonoBehaviour
{
    private const string LabelFormat = "{0}";

    [SerializeField] private TextMeshProUGUI levelLabel;

    private void Start()
    {
        // Start() runs after all Awake() calls, so ProgressionManager.Instance is guaranteed to exist.
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.onLevelUp.AddListener(OnLevelUp);
            SetLevel(ProgressionManager.Instance.currentLevel);
        }
        else
        {
            Debug.LogWarning("[PlayerLevelDisplay] ProgressionManager not found — level label will not update.");
        }
    }

    private void OnDestroy()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
    }

    /// <summary>Called by ProgressionManager.onLevelUp whenever the player levels up.</summary>
    private void OnLevelUp(int newLevel)
    {
        SetLevel(newLevel);
    }

    private void SetLevel(int level)
    {
        if (levelLabel != null)
            levelLabel.text = string.Format(LabelFormat, level);
    }
}
