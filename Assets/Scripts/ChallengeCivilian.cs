using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using UnityEngine;

public class ChallengeCivilian : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    private ActiveChallenge linkedChallenge;
    private bool isRescued;
    private bool isDead;
    private Traits traits;

    /// <summary>
    /// Links this civilian to an active challenge and hooks into the GC2 Stats health attribute.
    /// </summary>
    public void Initialize(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        isRescued = false;
        isDead = false;

        traits = GetComponent<Traits>();
        if (traits != null)
        {
            traits.RuntimeAttributes.EventChange += OnAttributeChanged;
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

    private void OnAttributeChanged(IdString attributeId)
    {
        if (attributeId.String != HealthAttributeId) return;

        RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
        if (health.Value <= health.MinValue)
        {
            OnCivilianDied();
        }
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
        if (traits != null)
        {
            traits.RuntimeAttributes.EventChange -= OnAttributeChanged;
        }

        // Fallback: notify death if not rescued and not already processed
        if (!isRescued && !isDead && linkedChallenge != null && ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnCivilianDied(linkedChallenge);
        }
    }
}
