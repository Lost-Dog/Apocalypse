using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;

/// <summary>
/// When the player takes damage, recruits the nearest idle friendly AIs already
/// present in the scene to run to the player and engage the attacker.
/// No spawning — companions are scene objects with vAICompanion components.
/// Attach to any persistent scene GameObject (e.g. GameManager).
/// </summary>
public class CompanionSummoner : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("Support Settings")]
    [Tooltip("Maximum number of companions that may be actively helping at the same time.")]
    public int maxActiveHelpers = 3;
    [Tooltip("Only recruit companions within this radius of the player. 0 = unlimited.")]
    public float recruitRadius = 30f;
    [Tooltip("Minimum seconds between two consecutive recruit calls.")]
    public float recruitCooldown = 5f;
    [Tooltip("Speed at which recruited companions run to the player.")]
    public vAIMovementSpeed approachSpeed = vAIMovementSpeed.Running;

    [Header("Player Reference")]
    [Tooltip("Assign the player Transform. If empty, searched at Start by tag + vThirdPersonController.")]
    public Transform playerOverride;

    [Header("Player Provider")]
    [Tooltip("Assign InvectorPlayerProvider to enable death-recall. Auto-searched if empty.")]
    public InvectorPlayerProvider playerProvider;

    [Header("Debug")]
    public bool logEvents = false;
    public bool showGizmos = true;

    // ── State ─────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private vHealthController playerHealth;
    private vAICompanionControl companionControl;
    private float lastRecruitTime = float.NegativeInfinity;

    /// <summary>All vAICompanion instances currently helping the player.</summary>
    private readonly HashSet<vAICompanion> activeHelpers = new HashSet<vAICompanion>();

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        FindPlayer();
        BindPlayerProvider();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
        UnbindPlayerProvider();
    }

    // ── Player Discovery ──────────────────────────────────────────────────────

    private void FindPlayer()
    {
        if (playerOverride != null)
        {
            playerTransform = playerOverride;
        }
        else
        {
            foreach (GameObject candidate in GameObject.FindGameObjectsWithTag(PlayerTag))
            {
                if (candidate.GetComponent<vThirdPersonController>() != null)
                {
                    playerTransform = candidate.transform;
                    break;
                }
            }
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[CompanionSummoner] Player not found. Support will not trigger.", this);
            return;
        }

        playerHealth = playerTransform.GetComponent<vHealthController>();
        if (playerHealth == null)
        {
            Debug.LogWarning("[CompanionSummoner] vHealthController not found on player.", this);
            return;
        }

        playerHealth.onReceiveDamage.AddListener(OnPlayerDamaged);
        companionControl = playerTransform.GetComponent<vAICompanionControl>();

        if (logEvents)
            Debug.Log("[CompanionSummoner] Subscribed to player damage events.", this);
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerHealth != null)
            playerHealth.onReceiveDamage.RemoveListener(OnPlayerDamaged);
    }

    // ── Player Provider (death recall) ────────────────────────────────────────

    private void BindPlayerProvider()
    {
        if (playerProvider == null)
            playerProvider = FindFirstObjectByType<InvectorPlayerProvider>();

        if (playerProvider == null)
        {
            if (logEvents)
                Debug.LogWarning("[CompanionSummoner] InvectorPlayerProvider not found. Death-recall disabled.", this);
            return;
        }

        playerProvider.OnDeath += OnPlayerDied;
    }

    private void UnbindPlayerProvider()
    {
        if (playerProvider != null)
            playerProvider.OnDeath -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        StartCoroutine(RecallNextFrame());
    }

    private IEnumerator RecallNextFrame()
    {
        yield return null;
        RecallAll();
    }

    // ── Damage Callback ───────────────────────────────────────────────────────

    private void OnPlayerDamaged(vDamage damage)
    {
        if (playerTransform == null || playerHealth.isDead) return;
        if (activeHelpers.Count >= maxActiveHelpers) return;
        if (Time.time - lastRecruitTime < recruitCooldown) return;

        RecruitNearbyCompanions(damage.sender);
    }

    // ── Recruitment ───────────────────────────────────────────────────────────

    private void RecruitNearbyCompanions(Transform attacker)
    {
        vAICompanion[] allCompanions = FindObjectsByType<vAICompanion>(FindObjectsSortMode.None);

        if (allCompanions.Length == 0)
        {
            if (logEvents)
                Debug.Log("[CompanionSummoner] No vAICompanion instances found in the scene.", this);
            return;
        }

        // Sort by distance to the player so the closest ones are recruited first.
        Vector3 playerPos = playerTransform.position;
        System.Array.Sort(allCompanions, (a, b) =>
        {
            float dA = Vector3.SqrMagnitude(a.transform.position - playerPos);
            float dB = Vector3.SqrMagnitude(b.transform.position - playerPos);
            return dA.CompareTo(dB);
        });

        int recruited = 0;
        int slots = maxActiveHelpers - activeHelpers.Count;

        foreach (vAICompanion companion in allCompanions)
        {
            if (recruited >= slots) break;
            if (companion == null || !companion.gameObject.activeSelf) continue;
            if (activeHelpers.Contains(companion)) continue;
            if (companion.friendIsDead) continue;

            // Distance gate.
            if (recruitRadius > 0f)
            {
                float dist = Vector3.Distance(companion.transform.position, playerPos);
                if (dist > recruitRadius) continue;
            }

            ActivateHelper(companion, attacker);
            recruited++;
        }

        if (recruited > 0)
        {
            lastRecruitTime = Time.time;
            if (logEvents)
                Debug.Log($"[CompanionSummoner] Recruited {recruited} companion(s). Active: {activeHelpers.Count}/{maxActiveHelpers}.", this);
        }
    }

    private void ActivateHelper(vAICompanion companion, Transform attacker)
    {
        activeHelpers.Add(companion);

        // Ensure the companion is linked to the player and runs toward them.
        if (companion.friend == null || companion.friend.gameObject != playerTransform.gameObject)
            companion.FindFriend();

        companion.GoToFriend(approachSpeed);

        // Point at the attacker so it engages immediately on arrival.
        if (attacker != null && companion.controlAI != null)
            companion.controlAI.SetCurrentTarget(attacker, true);

        // Register with vAICompanionControl so future damage relays reach it.
        if (companionControl != null && !companionControl.aICompanions.Contains(companion))
            companionControl.aICompanions.Add(companion);

        if (logEvents)
            Debug.Log($"[CompanionSummoner] '{companion.name}' is moving to support the player.", this);
    }

    // ── Recall ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Releases all active helpers and deregisters them from companion control.
    /// Call externally for cutscenes, safe zones, etc.
    /// </summary>
    public void RecallAll()
    {
        foreach (vAICompanion companion in activeHelpers)
        {
            if (companion == null) continue;
            if (companionControl != null)
                companionControl.aICompanions.Remove(companion);
        }

        activeHelpers.Clear();

        if (logEvents)
            Debug.Log("[CompanionSummoner] All companions recalled.", this);
    }

    /// <summary>
    /// Notifies the summoner that a helper has died so it is removed from tracking.
    /// Called by CompanionDeathNotifier on the companion GameObject.
    /// </summary>
    public void NotifyHelperDied(vAICompanion companion)
    {
        if (activeHelpers.Remove(companion) && logEvents)
            Debug.Log($"[CompanionSummoner] '{companion.name}' died and was removed. Active: {activeHelpers.Count}/{maxActiveHelpers}.", this);
    }

    // ── Public Queries ────────────────────────────────────────────────────────

    /// <summary>Number of companions currently helping the player.</summary>
    public int ActiveCount => activeHelpers.Count;

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || recruitRadius <= 0f) return;
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;
        Gizmos.color = Color.cyan;
        DrawGizmoCircle(center, recruitRadius);
    }

    private static void DrawGizmoCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = 360f / segments * Mathf.Deg2Rad;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
