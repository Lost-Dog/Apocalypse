using UnityEngine;
using JUTPS;

/// <summary>
/// Tracks player actions during challenges for bonus reward calculations
/// Attach this to the player or have ChallengeManager create it dynamically
/// </summary>
public class ChallengeBonusTracker : MonoBehaviour
{
    private ChallengeManager challengeManager;
    private JUHealth playerHealth;
    
    private void Start()
    {
        challengeManager = ChallengeManager.Instance;
        playerHealth = GetComponent<JUHealth>();
        
        if (playerHealth != null)
        {
            // Subscribe to damage events
            playerHealth.OnDamaged.AddListener(OnPlayerTookDamage);
        }
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged.RemoveListener(OnPlayerTookDamage);
        }
    }
    
    private void OnPlayerTookDamage(JUHealth.DamageInfo damageInfo)
    {
        if (challengeManager == null || challengeManager.activeChallenges == null)
            return;
        
        // Notify all active challenges that player took damage
        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted)
            {
                // Check if player is in range of this challenge
                if (challenge.IsPlayerInRange(transform.position))
                {
                    challenge.OnPlayerDamaged();
                }
            }
        }
    }
    
    /// <summary>
    /// Call this when player is detected by enemies during stealth challenges
    /// </summary>
    public void OnPlayerDetected()
    {
        if (challengeManager == null || challengeManager.activeChallenges == null)
            return;
        
        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted)
            {
                if (challenge.IsPlayerInRange(transform.position))
                {
                    challenge.OnPlayerDetected();
                }
            }
        }
    }
    
    /// <summary>
    /// Call this when player kills an enemy during a challenge
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        if (challengeManager == null || challengeManager.activeChallenges == null)
            return;
        
        foreach (var challenge in challengeManager.activeChallenges)
        {
            if (challenge != null && !challenge.isCompleted)
            {
                if (challenge.IsPlayerInRange(transform.position))
                {
                    challenge.OnEnemyKilled();
                }
            }
        }
    }
}
