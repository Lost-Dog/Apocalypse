using UnityEngine;

public class ChallengeCivilian : MonoBehaviour
{
    private ActiveChallenge linkedChallenge;
    private bool isRescued;

    public void Initialize(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        isRescued = false;
    }

    public void OnCivilianRescued()
    {
        if (isRescued || linkedChallenge == null)
            return;

        isRescued = true;

        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianRescued(linkedChallenge);
        }

        gameObject.SetActive(false);
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
        if (!isRescued && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianDied(linkedChallenge);
        }
    }
}
