using UnityEngine;

public class ChallengeEnemy : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isBoss;
    private bool isDead;
    private JUTPS.JUHealth juHealth;

    public void Initialize(ActiveChallenge challenge, bool boss = false)
    {
        linkedChallenge = challenge;
        isBoss = boss;
        isDead = false;
        
        // Hook into JUTPS health system
        juHealth = GetComponent<JUTPS.JUHealth>();
        if (juHealth != null)
        {
            juHealth.OnDeath.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning($"ChallengeEnemy on {gameObject.name}: No JUHealth component found!");
        }
    }

    public void OnEnemyDeath()
    {
        if (isDead || linkedChallenge == null)
            return;

        isDead = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnEnemyKilled(linkedChallenge);
            
            Debug.Log($"Challenge enemy killed! {linkedChallenge.enemiesKilled}/{linkedChallenge.challengeData.GetEnemyCount()}");
        }
    }

    private void OnDestroy()
    {
        // Remove listener
        if (juHealth != null)
        {
            juHealth.OnDeath.RemoveListener(OnEnemyDeath);
        }
        
        // Fallback: if health system didn't trigger death but object is being destroyed
        if (!isDead && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            OnEnemyDeath();
        }
    }
}
