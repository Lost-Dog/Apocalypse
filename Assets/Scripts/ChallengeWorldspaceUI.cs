using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeWorldspaceUI : MonoBehaviour
{
    [Header("Challenge Reference")]
    public ActiveChallenge linkedChallenge;
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private GameObject iconContainer;
    [SerializeField] private GameObject distanceContainer;
    
    [Header("Settings")]
    [SerializeField] private float maxVisibleDistance = 500f;
    [SerializeField] private float minVisibleDistance = 10f;
    [SerializeField] private Vector3 targetOffset = Vector3.up * 2f;
    [SerializeField] private float updateInterval = 0.1f;
    
    [Header("Colors by Difficulty")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color extremeColor = Color.red;
    
    private Transform playerTransform;
    private Camera mainCamera;
    private Canvas parentCanvas;
    private float updateTimer;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
        
        UpdateMarkerAppearance();
    }
    
    private void Update()
    {
        if (linkedChallenge == null || linkedChallenge.IsCompleted() || linkedChallenge.IsExpired())
        {
            gameObject.SetActive(false);
            return;
        }
        
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateMarkerPosition();
            UpdateDistanceDisplay();
        }
    }
    
    private void UpdateMarkerPosition()
    {
        if (playerTransform == null || mainCamera == null || markerRoot == null || linkedChallenge == null)
            return;
        
        Vector3 targetWorldPos = linkedChallenge.position + targetOffset;
        float distance = Vector3.Distance(targetWorldPos, playerTransform.position);
        
        if (distance < minVisibleDistance || distance > maxVisibleDistance)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
        
        bool isOnScreen = screenPos.z > 0 && 
                         screenPos.x > 0 && screenPos.x < Screen.width && 
                         screenPos.y > 0 && screenPos.y < Screen.height;
        
        if (!isOnScreen)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            markerRoot.position = screenPos;
        }
        else if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(),
                screenPos,
                parentCanvas.worldCamera,
                out localPoint
            );
            markerRoot.localPosition = localPoint;
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
