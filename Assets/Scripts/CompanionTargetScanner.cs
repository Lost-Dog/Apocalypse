using System.Collections;
using UnityEngine;
using Invector.vCharacterController.AI;

/// <summary>
/// Periodically calls FindTarget on the companion AI so it picks up nearby enemies
/// even while in the GoToFriend state, where the FSM never triggers FindTarget Decision.
/// Attach this to the same GameObject as vAICompanion + vControlAI.
/// </summary>
[RequireComponent(typeof(vAICompanion))]
public class CompanionTargetScanner : MonoBehaviour
{
    [Tooltip("Seconds between each scan for a new target while the companion has none.")]
    public float scanInterval = 0.5f;

    private vControlAI controlAI;
    private Coroutine scanRoutine;

    private void Start()
    {
        controlAI = GetComponent<vControlAI>();
        scanRoutine = StartCoroutine(ScanRoutine());
    }

    private void OnDisable()
    {
        if (scanRoutine != null)
        {
            StopCoroutine(scanRoutine);
            scanRoutine = null;
        }
    }

    private void OnEnable()
    {
        if (controlAI != null)
            scanRoutine = StartCoroutine(ScanRoutine());
    }

    /// <summary>
    /// Continuously looks for a target when the companion has none.
    /// Backs off to a longer interval once a target is acquired to avoid unnecessary overhead.
    /// </summary>
    private IEnumerator ScanRoutine()
    {
        var wait = new WaitForSeconds(scanInterval);
        var waitLong = new WaitForSeconds(scanInterval * 4f);

        while (true)
        {
            if (controlAI == null)
            {
                yield return wait;
                continue;
            }

            if (controlAI.currentTarget.transform == null)
            {
                controlAI.FindTarget();
                yield return wait;
            }
            else
            {
                // Already has a target — check less often
                yield return waitLong;
            }
        }
    }
}
