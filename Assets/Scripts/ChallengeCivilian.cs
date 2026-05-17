using Invector;
using UnityEngine;

public class ChallengeCivilian : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isRescued;
    private bool isDead;
    private vHealthController healthController;

    /// <summary>
    /// Links this civilian to an active challenge and hooks into Invector's health death event.
    /// </summary>
    public void Initialize(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        isRescued = false;
        isDead = false;

        healthController = GetComponent<vHealthController>();
        if (healthController != null)
        {
            healthController.onDead.AddListener(OnCivilianDiedFromDamage);
        }
    }

    /// <summary>
    /// Call this when the player reaches the civilian to rescue them.
    /// </summary>
    public void OnCivilianRescued()
    {
        if (isRescued || linkedChallenge == null) return;

        isRescued = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianRescued(linkedChallenge);
            Debug.Log($"Civilian rescued! {linkedChallenge.civiliansRescued}/{linkedChallenge.challengeData.GetCivilianCount()}");
        }

        gameObject.SetActive(false);
    }

    private void OnCivilianDiedFromDamage(GameObject deadObject)
    {
        OnCivilianDied();
    }

    private void OnCivilianDied()
    {
        if (isDead || isRescued || linkedChallenge == null) return;

        isDead = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianDied(linkedChallenge);
            Debug.LogWarning("Civilian died! Challenge may fail if requireNoDeaths is enabled.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCivilianRescued();
        }
    }

    private void OnDestroy()
    {
        if (healthController != null)
        {
            healthController.onDead.RemoveListener(OnCivilianDiedFromDamage);
        }

        // Fallback: notify death if not rescued and not already processed
        if (!isRescued && !isDead && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianDied(linkedChallenge);
        }
    }
}
