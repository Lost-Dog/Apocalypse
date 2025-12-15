using UnityEngine;
using UnityEngine.UI;

public class ChallengeMinimapMarker : MonoBehaviour
{
    [Header("Challenge Reference")]
    public ActiveChallenge linkedChallenge;
    
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private RectTransform iconRect;
    
    [Header("Settings")]
    [SerializeField] private float iconSize = 20f;
    [SerializeField] private bool rotateWithPlayer = false;
    [SerializeField] private Sprite defaultIcon;
    
    [Header("Colors by Difficulty")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color extremeColor = Color.red;
    
    private Transform playerTransform;
    private RectTransform minimapIconsContainer;
    private Camera minimapCamera;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        GameObject minimapCameraObj = GameObject.Find("MiniMapCamera");
        if (minimapCameraObj != null)
        {
            minimapCamera = minimapCameraObj.GetComponent<Camera>();
        }
        
        if (iconRect == null)
        {
            iconRect = GetComponent<RectTransform>();
        }
        
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
        
        if (iconImage != null && defaultIcon != null)
        {
            iconImage.sprite = defaultIcon;
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
        
        UpdatePosition();
    }
    
    private void UpdatePosition()
    {
        if (linkedChallenge == null || minimapCamera == null)
            return;
        
        Vector3 worldPos = linkedChallenge.position;
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
        
        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        if (iconRect != null && minimapIconsContainer != null)
        {
            Vector2 localPoint = new Vector2(
                (viewportPos.x - 0.5f) * minimapIconsContainer.rect.width,
                (viewportPos.y - 0.5f) * minimapIconsContainer.rect.height
            );
            
            iconRect.anchoredPosition = localPoint;
            
            if (rotateWithPlayer && playerTransform != null)
            {
                float angle = -playerTransform.eulerAngles.y;
                iconRect.localRotation = Quaternion.Euler(0, 0, angle);
            }
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
        
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
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
    
    public void SetMinimapContainer(RectTransform container)
    {
        minimapIconsContainer = container;
    }
}
