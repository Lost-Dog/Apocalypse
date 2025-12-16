using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeWorldMarker : MonoBehaviour
{
    [Header("References")]
    public ActiveChallenge linkedChallenge;
    
    [Header("UI Components")]
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    [SerializeField] private float maxVisibleDistance = 500f;
    [SerializeField] private float minVisibleDistance = 10f;
    [SerializeField] private Vector3 worldOffset = Vector3.up * 2f;
    [SerializeField] private float distanceUpdateInterval = 0.25f;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float smoothSpeed = 15f;
    [SerializeField] private bool worldSpaceMode = true;
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private bool scaleWithDistance = false;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float scaleDistance = 100f;
    
    [Header("Screen Edge Clamping")]
    [SerializeField] private bool clampToScreenEdges = true;
    [SerializeField] private float edgePadding = 50f;
    
    [Header("Colors by Difficulty")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color extremeColor = Color.red;
    
    private Transform playerTransform;
    private Camera mainCamera;
    private Canvas parentCanvas;
    private float distanceUpdateTimer;
    private bool isVisible;
    private Vector3 targetScreenPosition;
    private Vector3 currentScreenPosition;
    private Vector3 targetWorldPosition;
    private bool isInitialized;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            worldSpaceMode = true;
        }
        
        if (markerRoot == null)
        {
            markerRoot = GetComponent<RectTransform>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        UpdateMarkerAppearance();
        
        if (linkedChallenge != null)
        {
            targetWorldPosition = linkedChallenge.position + worldOffset;
            
            if (worldSpaceMode)
            {
                transform.position = targetWorldPosition;
            }
            else if (mainCamera != null)
            {
                currentScreenPosition = mainCamera.WorldToScreenPoint(targetWorldPosition);
                targetScreenPosition = currentScreenPosition;
            }
            
            isInitialized = true;
        }
    }

    private void Update()
    {
        if (linkedChallenge == null || linkedChallenge.IsCompleted() || linkedChallenge.IsExpired())
        {
            Destroy(gameObject);
            return;
        }

        distanceUpdateTimer += Time.deltaTime;
        if (distanceUpdateTimer >= distanceUpdateInterval)
        {
            distanceUpdateTimer = 0f;
            UpdateDistanceDisplay();
        }
        
        UpdateVisibilityFade();
    }
    
    private void LateUpdate()
    {
        if (linkedChallenge != null && mainCamera != null && playerTransform != null)
        {
            CalculateTargetPosition();
            UpdateMarkerPosition();
        }
    }
    
    private void CalculateTargetPosition()
    {
        targetWorldPosition = linkedChallenge.position + worldOffset;
        float distance = Vector3.Distance(targetWorldPosition, playerTransform.position);

        if (distance < minVisibleDistance || distance > maxVisibleDistance)
        {
            isVisible = false;
            return;
        }

        if (worldSpaceMode)
        {
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(targetWorldPosition);
            bool isOnScreen = viewportPoint.z > 0 && 
                             viewportPoint.x > 0 && viewportPoint.x < 1 && 
                             viewportPoint.y > 0 && viewportPoint.y < 1;

            if (!isOnScreen && !clampToScreenEdges)
            {
                isVisible = false;
                return;
            }
            
            isVisible = true;
        }
        else
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPosition);

            bool isOnScreen = screenPos.z > 0 && 
                             screenPos.x > 0 && screenPos.x < Screen.width && 
                             screenPos.y > 0 && screenPos.y < Screen.height;

            if (!isOnScreen && !clampToScreenEdges)
            {
                isVisible = false;
                return;
            }

            isVisible = true;
            
            // Clamp to screen edges if enabled
            if (clampToScreenEdges)
            {
                screenPos = ClampToScreenEdges(screenPos);
            }
            
            targetScreenPosition = screenPos;
            
            if (!isInitialized)
            {
                currentScreenPosition = targetScreenPosition;
                isInitialized = true;
            }
        }
    }

    private void UpdateMarkerPosition()
    {
        if (markerRoot == null || !isVisible)
            return;

        if (worldSpaceMode)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPosition, Time.deltaTime * smoothSpeed);
            
            // Apply distance-based scaling
            if (scaleWithDistance && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                float scaleFactor = Mathf.Lerp(maxScale, minScale, distance / scaleDistance);
                scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);
                transform.localScale = Vector3.one * baseScale * scaleFactor;
            }
            else
            {
                transform.localScale = Vector3.one * baseScale;
            }
            
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
            }
        }
        else
        {
            currentScreenPosition = Vector3.Lerp(currentScreenPosition, targetScreenPosition, Time.deltaTime * smoothSpeed);

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                markerRoot.position = currentScreenPosition;
            }
            else if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.GetComponent<RectTransform>(),
                    currentScreenPosition,
                    parentCanvas.worldCamera,
                    out localPoint
                );
                markerRoot.localPosition = localPoint;
            }
        }
    }

    private void UpdateDistanceDisplay()
    {
        if (distanceText == null || linkedChallenge == null || playerTransform == null)
            return;

        float distance = Vector3.Distance(linkedChallenge.position, playerTransform.position);

        if (distance < 1000f)
        {
            distanceText.text = $"{Mathf.RoundToInt(distance)}m";
        }
        else
        {
            distanceText.text = $"{(distance / 1000f):F1}km";
        }
    }
    
    private void UpdateVisibilityFade()
    {
        if (canvasGroup == null) return;
        
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
    
    private Vector3 ClampToScreenEdges(Vector3 screenPos)
    {
        // Handle behind camera (z < 0)
        if (screenPos.z < 0)
        {
            // Flip coordinates when behind camera
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }
        
        // Clamp to screen boundaries with padding
        screenPos.x = Mathf.Clamp(screenPos.x, edgePadding, Screen.width - edgePadding);
        screenPos.y = Mathf.Clamp(screenPos.y, edgePadding, Screen.height - edgePadding);
        screenPos.z = Mathf.Max(screenPos.z, 0); // Ensure positive z
        
        return screenPos;
    }

    private void UpdateMarkerAppearance()
    {
        if (linkedChallenge == null || linkedChallenge.challengeData == null)
            return;

        Color difficultyColor = GetDifficultyColor(linkedChallenge.challengeData.difficulty);

        if (iconImage != null)
        {
            iconImage.color = difficultyColor;
        }

        if (distanceText != null)
        {
            distanceText.color = difficultyColor;
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
