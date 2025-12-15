using UnityEngine;
using UnityEngine.UI;

public class ChallengeCompassMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform markerRectTransform;
    [SerializeField] private Image markerImage;
    
    [Header("Settings")]
    [SerializeField] private float compassWidth = 1000f;
    [SerializeField] private float edgePadding = 50f;
    
    public ActiveChallenge linkedChallenge;
    private Transform playerTransform;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (markerRectTransform == null)
        {
            markerRectTransform = GetComponent<RectTransform>();
        }

        if (markerImage == null)
        {
            markerImage = GetComponent<Image>();
        }
    }

    private void Update()
    {
        if (linkedChallenge == null || linkedChallenge.IsCompleted() || linkedChallenge.IsExpired())
        {
            Destroy(gameObject);
            return;
        }

        UpdateMarkerPosition();
    }

    private void UpdateMarkerPosition()
    {
        if (playerTransform == null || mainCamera == null)
            return;

        Vector3 directionToChallenge = linkedChallenge.position - playerTransform.position;
        directionToChallenge.y = 0;

        float angle = Vector3.SignedAngle(playerTransform.forward, directionToChallenge, Vector3.up);

        float normalizedAngle = angle / 180f;
        float xPosition = normalizedAngle * (compassWidth * 0.5f);

        xPosition = Mathf.Clamp(xPosition, -compassWidth * 0.5f + edgePadding, compassWidth * 0.5f - edgePadding);

        markerRectTransform.anchoredPosition = new Vector2(xPosition, markerRectTransform.anchoredPosition.y);

        if (markerImage != null && linkedChallenge.challengeData != null)
        {
            markerImage.color = linkedChallenge.challengeData.GetDifficultyColor();
        }
    }

    public void SetChallenge(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
    }
}
