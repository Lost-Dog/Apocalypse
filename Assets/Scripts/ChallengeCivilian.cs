using UnityEngine;

public class ChallengeCivilian : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isRescued;
    private bool isDead;
    private JUTPS.JUHealth juHealth;

    public void Initialize(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        isRescued = false;
        isDead = false;
        
        // Hook into JUTPS health system for death detection
        juHealth = GetComponent<JUTPS.JUHealth>();
        if (juHealth != null)
        {
            juHealth.OnDeath.AddListener(OnCivilianDied);
        }
    }

    public void OnCivilianRescued()
    {
        if (isRescued || linkedChallenge == null)
            return;

        isRescued = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianRescued(linkedChallenge);
            
            Debug.Log($"Civilian rescued! {linkedChallenge.civiliansRescued}/{linkedChallenge.challengeData.GetCivilianCount()}");
        }

        gameObject.SetActive(false);
    }
    
    private void OnCivilianDied()
    {
        if (isDead || isRescued || linkedChallenge == null)
            return;
        
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
        // Remove listener
        if (juHealth != null)
        {
            juHealth.OnDeath.RemoveListener(OnCivilianDied);
        }
        
        // Fallback: notify death if not rescued and not already processed
        if (!isRescued && !isDead && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianDied(linkedChallenge);
        }
    }
}
