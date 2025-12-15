using UnityEngine;

public class ChallengeEnemy : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isBoss;
    private bool isDead;

    public void Initialize(ActiveChallenge challenge, bool boss = false)
    {
        linkedChallenge = challenge;
        isBoss = boss;
        isDead = false;
    }

    public void OnEnemyDeath()
    {
        if (isDead || linkedChallenge == null)
            return;

        isDead = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnEnemyKilled(linkedChallenge);
        }

        Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        if (!isDead && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            OnEnemyDeath();
        }
    }
}
