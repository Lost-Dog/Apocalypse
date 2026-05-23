using Invector;
using UnityEngine;

public class ChallengeEnemy : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isBoss;
    private bool isDead;
    private vHealthController healthController;

    /// <summary>
    /// Links this enemy to an active challenge and subscribes to Invector's death event.
    /// </summary>
    public void Initialize(ActiveChallenge challenge, bool boss = false)
    {
        linkedChallenge = challenge;
        isBoss          = boss;
        isDead          = false;

        healthController = GetComponent<vHealthController>();
        if (healthController != null)
        {
            healthController.onDead.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning($"ChallengeEnemy on {gameObject.name}: No vHealthController component found!");
        }
    }

    public void OnEnemyDeath(GameObject deadObject)
    {
        if (isDead || linkedChallenge == null) return;

        isDead = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnEnemyKilled(linkedChallenge);
            int total = linkedChallenge.totalEnemiesSpawned > 0
                ? linkedChallenge.totalEnemiesSpawned
                : linkedChallenge.challengeData.GetEnemyCount();
            Debug.Log($"Challenge enemy killed! {linkedChallenge.enemiesKilled}/{total}");
        }
    }

    private void OnDestroy()
    {
        if (healthController != null)
            healthController.onDead.RemoveListener(OnEnemyDeath);

        // Fallback: if the object was destroyed before the death event fired
        if (!isDead && linkedChallenge != null && ChallengeManager.Instance != null)
            OnEnemyDeath(gameObject);
    }
}
