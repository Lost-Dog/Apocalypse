using UnityEngine;
using Invector;
using Invector.vCharacterController.AI;

/// <summary>
/// Attach to every friendly AI alongside vAICompanion.
/// Listens for vHealthController.onDead and notifies CompanionSummoner
/// so it can remove this AI from its active helpers tracking.
/// </summary>
[RequireComponent(typeof(vAICompanion))]
public class CompanionDeathNotifier : MonoBehaviour
{
    [Tooltip("The CompanionSummoner in the scene. Auto-searched if empty.")]
    public CompanionSummoner summoner;

    private vAICompanion aiCompanion;
    private vHealthController healthController;

    private void Start()
    {
        aiCompanion = GetComponent<vAICompanion>();
        healthController = GetComponent<vHealthController>();

        if (healthController == null)
            Debug.LogWarning("[CompanionDeathNotifier] vHealthController not found on companion.", this);

        if (summoner == null)
            summoner = FindFirstObjectByType<CompanionSummoner>();

        if (summoner == null)
            Debug.LogWarning("[CompanionDeathNotifier] CompanionSummoner not found in scene.", this);
    }

    private void OnEnable()
    {
        if (healthController == null)
            healthController = GetComponent<vHealthController>();

        if (healthController != null)
            healthController.onDead.AddListener(HandleDeath);
    }

    private void OnDisable()
    {
        if (healthController != null)
            healthController.onDead.RemoveListener(HandleDeath);
    }

    /// <summary>
    /// Called by vHealthController.onDead when this companion's health reaches zero.
    /// </summary>
    private void HandleDeath(GameObject deadObject)
    {
        if (summoner != null && aiCompanion != null)
            summoner.NotifyHelperDied(aiCompanion);
    }
}
