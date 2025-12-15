using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeWorldMarker : MonoBehaviour
{
    [Header("References")]
    public ActiveChallenge linkedChallenge;
    
    [Header("Visual Components")]
    [SerializeField] private GameObject markerPivot;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private Light markerLight;
    [SerializeField] private ParticleSystem markerParticles;
    
    [Header("3D Text")]
    [SerializeField] private TextMeshPro distanceText;
    [SerializeField] private TextMeshPro challengeNameText;
    
    [Header("Settings")]
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private float maxVisibleDistance = 500f;
    [SerializeField] private float hideWhenCloseDistance = 5f;
    
    [Header("Colors by Difficulty")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color extremeColor = Color.red;
    
    private Transform playerTransform;
    private Vector3 startPosition;
    private float bobTimer;

    private void Start()
    {
        startPosition = transform.position;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        UpdateMarkerAppearance();
    }

    private void Update()
    {
        if (linkedChallenge == null || linkedChallenge.IsCompleted() || linkedChallenge.IsExpired())
        {
            Destroy(gameObject);
            return;
        }

        UpdateBobbing();
        UpdateRotation();
        UpdateVisibility();
        UpdateDistanceDisplay();
    }

    private void UpdateBobbing()
    {
        if (markerPivot == null) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float newY = Mathf.Sin(bobTimer) * bobHeight;
        markerPivot.transform.localPosition = new Vector3(0, newY, 0);
    }

    private void UpdateRotation()
    {
        if (markerPivot == null) return;

        markerPivot.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void UpdateVisibility()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        bool shouldBeVisible = distance <= maxVisibleDistance && distance >= hideWhenCloseDistance;

        if (iconRenderer != null)
        {
            iconRenderer.enabled = shouldBeVisible;
        }

        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(shouldBeVisible);
        }

        if (challengeNameText != null)
        {
            challengeNameText.gameObject.SetActive(shouldBeVisible && distance <= 100f);
        }
    }

    private void UpdateDistanceDisplay()
    {
        if (playerTransform == null || distanceText == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < 1000f)
        {
            distanceText.text = $"{Mathf.RoundToInt(distance)}m";
        }
        else
        {
            distanceText.text = $"{(distance / 1000f):F1}km";
        }

        if (distanceText.transform.parent != null)
        {
            distanceText.transform.parent.LookAt(playerTransform);
        }

        if (challengeNameText != null && challengeNameText.transform.parent != null)
        {
            challengeNameText.transform.parent.LookAt(playerTransform);
        }
    }

    private void UpdateMarkerAppearance()
    {
        if (linkedChallenge == null || linkedChallenge.challengeData == null)
            return;

        Color difficultyColor = GetDifficultyColor(linkedChallenge.challengeData.difficulty);

        if (iconRenderer != null)
        {
            iconRenderer.color = difficultyColor;
        }

        if (markerLight != null)
        {
            markerLight.color = difficultyColor;
        }

        if (markerParticles != null)
        {
            var main = markerParticles.main;
            main.startColor = difficultyColor;
        }

        if (challengeNameText != null)
        {
            challengeNameText.text = linkedChallenge.challengeData.challengeName;
            challengeNameText.color = difficultyColor;
        }
    }

    private Color GetDifficultyColor(ChallengeData.ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeData.ChallengeDifficulty.Easy:
                return easyColor;
            case ChallengeData.ChallengeDifficulty.Medium:
                return mediumColor;
            case ChallengeData.ChallengeDifficulty.Hard:
                return hardColor;
            case ChallengeData.ChallengeDifficulty.Extreme:
                return extremeColor;
            default:
                return Color.white;
        }
    }

    public void SetChallenge(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        UpdateMarkerAppearance();
    }
}
