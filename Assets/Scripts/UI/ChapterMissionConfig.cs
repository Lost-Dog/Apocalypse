using UnityEngine;

/// <summary>
/// Attach to each Chapter button in the Campaign panel.
/// Assign the <see cref="MissionData"/> ScriptableObject for this chapter in the Inspector,
/// then wire the button's onClick to <see cref="Launch"/>.
/// </summary>
public class ChapterMissionConfig : MonoBehaviour
{
    [Tooltip("The mission that this chapter button will start. Create via Right-click > Division Game > Mission Data.")]
    [SerializeField] private MissionData mission;

    [Tooltip("Auto-found in the scene if left empty.")]
    [SerializeField] private LoadingPanelBridge bridge;

    private void Awake()
    {
        if (bridge == null)
            bridge = FindFirstObjectByType<LoadingPanelBridge>(FindObjectsInactive.Include);

        if (bridge == null)
            Debug.LogError("[ChapterMissionConfig] No LoadingPanelBridge found in scene. Assign it in the Inspector.", this);
    }

    /// <summary>
    /// Wire this to the Chapter button's onClick.
    /// Queues the assigned mission and begins async scene load.
    /// </summary>
    public void Launch()
    {
        if (mission == null)
        {
            Debug.LogWarning("[ChapterMissionConfig] No MissionData assigned — loading scene without a mission.", this);
            bridge?.StartLoading();
            return;
        }

        bridge?.StartLoadingWithMission(mission.missionName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep the GameObject name in sync with the assigned mission for easy scanning in the Hierarchy.
        if (mission != null && !string.IsNullOrWhiteSpace(mission.missionName))
            gameObject.name = mission.missionName;
    }
#endif
}
