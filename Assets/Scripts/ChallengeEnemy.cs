using GameCreator.Runtime.Characters;
using UnityEngine;

public class ChallengeEnemy : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isBoss;
    private bool isDead;
    private Character character;

    /// <summary>
    /// Links this enemy to an active challenge and subscribes to the GC2 death event.
    /// </summary>
    public void Initialize(ActiveChallenge challenge, bool boss = false)
    {
        linkedChallenge = challenge;
        isBoss          = boss;
        isDead          = false;

        character = GetComponent<Character>();
        if (character != null)
        {
            character.EventDie += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"ChallengeEnemy on {gameObject.name}: No GC2 Character component found!");
        }
    }

    public void OnEnemyDeath()
    {
        if (isDead || linkedChallenge == null) return;

        isDead = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnEnemyKilled(linkedChallenge);
            Debug.Log($"Challenge enemy killed! {linkedChallenge.enemiesKilled}/{linkedChallenge.challengeData.GetEnemyCount()}");
        }
    }

    private void OnDestroy()
    {
        if (character != null)
            character.EventDie -= OnEnemyDeath;

        // Fallback: if the object was destroyed before the death event fired
        if (!isDead && linkedChallenge != null && ChallengeManager.Instance != null)
            OnEnemyDeath();
    }
}
