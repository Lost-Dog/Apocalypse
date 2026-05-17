using UnityEngine;

/// <summary>
/// Tracks player actions during challenges for bonus reward calculations.
/// Health damage detection uses IPlayerProvider's OnHealthChanged event.
/// Attach this to the player or have ChallengeManager create it dynamically.
/// </summary>
public class ChallengeBonusTracker : MonoBehaviour
{
    private ChallengeManager challengeManager;
    private IPlayerProvider playerProvider;
    private float lastHealth;

    private void Start()
    {
        challengeManager = ChallengeManager.Instance;

        // Resolve IPlayerProvider from this GameObject first, then search the scene.
        foreach (var mb in GetComponents<MonoBehaviour>())
        {
            if (mb is IPlayerProvider provider)
            {
                playerProvider = provider;
                break;
            }
        }

        if (playerProvider == null)
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is IPlayerProvider provider)
                {
                    playerProvider = provider;
                    break;
                }
            }
        }

        if (playerProvider != null)
        {
            lastHealth = playerProvider.Health;
            playerProvider.OnHealthChanged += OnHealthChanged;
        }
        else
        {
            Debug.LogWarning("[ChallengeBonusTracker] No IPlayerProvider found — damage tracking disabled.");
        }
    }

    private void OnDestroy()
    {
        if (playerProvider != null)
        {
            playerProvider.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // Only act when health decreased (damage taken).
        if (currentHealth >= lastHealth)
        {
            lastHealth = currentHealth;
            return;
        }

        lastHealth = currentHealth;

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
