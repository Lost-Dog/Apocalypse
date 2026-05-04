using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Allows the player to extinguish fires with an optional confirmation popup.
/// Call Interact() from a GC2 instruction or trigger to activate.
/// </summary>
public class FireExtinguisher : MonoBehaviour
{
    [Header("Fire References")]
    [Tooltip("The fire particle system GameObject (Fire_Big FX)")]
    public GameObject fireObject;

    [Tooltip("Auto-find fire in children if not assigned")]
    public bool autoFindFire = true;

    [Header("UI Settings")]
    [Tooltip("Canvas with confirmation popup")]
    public GameObject confirmationUI;

    [Tooltip("Text for the prompt message")]
    public TextMeshProUGUI promptText;

    [Tooltip("Confirm button")]
    public Button confirmButton;

    [Tooltip("Cancel button")]
    public Button cancelButton;

    [Tooltip("Custom prompt message")]
    public string promptMessage = "Extinguish this fire?";

    [Header("Extinguish Settings")]
    [Tooltip("Time it takes for fire to fade out")]
    public float fadeOutDuration = 2f;

    [Tooltip("Disable object after extinguishing")]
    public bool disableAfterExtinguish = true;

    [Header("Effects")]
    [Tooltip("Sound to play when extinguishing")]
    public AudioClip extinguishSound;

    [Tooltip("Particle effect when extinguished")]
    public GameObject extinguishEffect;

    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Rewards (Optional)")]
    [Tooltip("XP reward for extinguishing")]
    public int xpReward = 25;

    [Tooltip("Award XP on extinguish")]
    public bool awardXP = true;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private bool isExtinguished = false;
    private ParticleSystem[] fireParticles;
    private Light[] fireLights;
    private AudioSource audioSource;

    private void Start()
    {
        if (autoFindFire && fireObject == null)
            FindFireObject();

        if (fireObject != null)
        {
            fireParticles = fireObject.GetComponentsInChildren<ParticleSystem>();
            fireLights    = fireObject.GetComponentsInChildren<Light>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && extinguishSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 1f;
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmExtinguish);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelExtinguish);

        if (confirmationUI != null)
            confirmationUI.SetActive(false);
    }

    private void FindFireObject()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Fire_Big") || child.name.Contains("FX_Fire"))
            {
                fireObject = child.gameObject;
                return;
            }
        }

        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            fireObject = ps.gameObject;
    }

    /// <summary>
    /// Entry point called by GC2 interactions (e.g. via an Execute Method instruction)
    /// or any other trigger. Shows a confirmation UI if assigned, otherwise extinguishes immediately.
    /// </summary>
    public void Interact()
    {
        if (isExtinguished) return;
        if (fireObject == null || !fireObject.activeSelf) return;

        if (confirmationUI != null)
            ShowConfirmationUI();
        else
            ExtinguishFire();

        if (showDebugInfo)
            Debug.Log($"[FireExtinguisher] Interact called on {gameObject.name}");
    }

    private void ShowConfirmationUI()
    {
        confirmationUI.SetActive(true);

        if (promptText != null)
            promptText.text = promptMessage;

        Time.timeScale = 0f;
    }

    private void OnConfirmExtinguish()
    {
        HideConfirmationUI();
        ExtinguishFire();
    }

    private void OnCancelExtinguish()
    {
        HideConfirmationUI();

        if (showDebugInfo)
            Debug.Log("[FireExtinguisher] Player cancelled.");
    }

    private void HideConfirmationUI()
    {
        if (confirmationUI != null)
            confirmationUI.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ExtinguishFire()
    {
        if (isExtinguished) return;

        isExtinguished = true;

        StartCoroutine(FadeOutFire());

        if (extinguishSound != null && audioSource != null)
            audioSource.PlayOneShot(extinguishSound, soundVolume);

        if (extinguishEffect != null)
        {
            GameObject effect = Instantiate(extinguishEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        if (awardXP && xpReward > 0)
        {
            ProgressionManager progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager != null)
                progressionManager.AddExperience(xpReward);
        }

        if (showDebugInfo)
            Debug.Log($"[FireExtinguisher] Fire extinguished on {gameObject.name}");
    }

    private IEnumerator FadeOutFire()
    {
        float elapsed = 0f;
        float[] initialEmissionRates    = new float[fireParticles.Length];
        float[] initialLightIntensities = new float[fireLights.Length];

        for (int i = 0; i < fireParticles.Length; i++)
        {
            var emission = fireParticles[i].emission;
            initialEmissionRates[i] = emission.rateOverTime.constant;
        }

        for (int i = 0; i < fireLights.Length; i++)
            initialLightIntensities[i] = fireLights[i].intensity;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float fadeValue = 1f - (elapsed / fadeOutDuration);

            for (int i = 0; i < fireParticles.Length; i++)
            {
                if (fireParticles[i] == null) continue;
                var emission = fireParticles[i].emission;
                var rate = emission.rateOverTime;
                rate.constant = initialEmissionRates[i] * fadeValue;
                emission.rateOverTime = rate;
            }

            for (int i = 0; i < fireLights.Length; i++)
            {
                if (fireLights[i] != null)
                    fireLights[i].intensity = initialLightIntensities[i] * fadeValue;
            }

            yield return null;
        }

        foreach (ParticleSystem ps in fireParticles)
            if (ps != null) ps.Stop();

        foreach (Light light in fireLights)
            if (light != null) light.enabled = false;

        yield return new WaitForSeconds(1f);

        if (fireObject != null && disableAfterExtinguish)
            fireObject.SetActive(false);

        if (disableAfterExtinguish)
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
