using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using UnityEngine;

/// <summary>
/// Tracks player actions during challenges for bonus reward calculations.
/// Attach this to the player or have ChallengeManager create it dynamically.
/// </summary>
public class ChallengeBonusTracker : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    private ChallengeManager challengeManager;
    private Traits playerTraits;

    private void Start()
    {
        challengeManager = ChallengeManager.Instance;
        playerTraits = GetComponent<Traits>();

        if (playerTraits != null)
        {
            playerTraits.RuntimeAttributes.EventChange += OnAttributeChanged;
        }
    }

    private void OnDestroy()
    {
        if (playerTraits != null)
        {
            playerTraits.RuntimeAttributes.EventChange -= OnAttributeChanged;
        }
    }

    private void OnAttributeChanged(IdString attributeId)
    {
        if (attributeId.String != HealthAttributeId) return;

        // A negative LastChange means the attribute decreased — i.e., damage was taken
        if (playerTraits.RuntimeAttributes.LastChange >= 0) return;

        if (challengeManager == null || challengeManager.activeChallenges == null) return;

        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted && challenge.IsPlayerInRange(transform.position))
            {
                challenge.OnPlayerDamaged();
            }
        }
    }

    /// <summary>
    /// Call this when player is detected by enemies during stealth challenges.
    /// </summary>
    public void OnPlayerDetected()
    {
        if (challengeManager == null || challengeManager.activeChallenges == null) return;

        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted && challenge.IsPlayerInRange(transform.position))
            {
                challenge.OnPlayerDetected();
            }
        }
    }

    /// <summary>
    /// Call this when player kills an enemy during a challenge.
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (challengeManager == null || challengeManager.activeChallenges == null) return;

        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted && challenge.IsPlayerInRange(transform.position))
            {
                challenge.OnEnemyKilled();
            }
        }
    }
}
