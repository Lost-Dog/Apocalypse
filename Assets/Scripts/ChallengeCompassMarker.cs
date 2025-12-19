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
        // Warning if attached to main compass GameObject
        if (gameObject.name.Contains("HUD_Apocalypse_Compass"))
        {
            Debug.LogWarning($"[ChallengeCompassMarker] This component should be on a CHILD marker object, not the main compass GameObject '{gameObject.name}'! The marker functionality may not work correctly.");
        }

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
        
        // If no marker image found and we're on the compass itself, disable this component
        if (markerImage == null && gameObject.name.Contains("HUD_Apocalypse_Compass"))
        {
            Debug.LogWarning($"[ChallengeCompassMarker] No marker image found on compass '{gameObject.name}'. Disabling component to prevent issues.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (linkedChallenge == null || linkedChallenge.IsCompleted() || linkedChallenge.IsExpired())
        {
            // Safety check: Don't destroy the compass itself!
            // Only destroy if this is a dynamically created marker
            if (gameObject.name != "HUD_Apocalypse_Compass_01" && 
                gameObject.name != "HUD_Apocalypse_Compass_02" && 
                gameObject.name != "HUD_Apocalypse_Compass_03")
            {
                Destroy(gameObject);
            }
            else
            {
                // If attached to main compass, just hide the marker image
                if (markerImage != null)
                {
                    markerImage.enabled = false;
                }
            }
            return;
        }

        // Show marker if we have a valid challenge
        if (markerImage != null && !markerImage.enabled)
        {
            markerImage.enabled = true;
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
