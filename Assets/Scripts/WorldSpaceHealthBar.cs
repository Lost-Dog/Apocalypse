using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JUTPS;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private JUHealth targetHealth;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Canvas worldSpaceCanvas;
    
    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Positioning")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float smoothSpeed = 8f;
    
    [Header("Visibility")]
    [SerializeField] private bool showOnlyWhenDamaged = true;
    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private float maxVisibleDistance = 30f;
    [SerializeField] private bool alwaysShow = false;
    
    [Header("Health Colors")]
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color midHealthColor = new Color(0.9f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private float midHealthThreshold = 0.6f;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    private float lastHealthValue;
    private float hideTimer;
    private bool isVisible;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        if (worldSpaceCanvas == null)
        {
            worldSpaceCanvas = GetComponent<Canvas>();
            if (worldSpaceCanvas == null)
                worldSpaceCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && worldSpaceCanvas != null)
            canvasGroup = worldSpaceCanvas.gameObject.AddComponent<CanvasGroup>();
        
        if (targetTransform == null)
            targetTransform = transform.parent;
        
        if (targetHealth == null && targetTransform != null)
            targetHealth = targetTransform.GetComponent<JUHealth>();
        
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);
        
        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
    }
    
    private void Start()
    {
        FindCamera();
        
        if (worldSpaceCanvas != null && mainCamera != null)
        {
            worldSpaceCanvas.worldCamera = mainCamera;
        }
        
        if (targetHealth != null)
        {
            lastHealthValue = targetHealth.Health;
            UpdateHealthBar();
        }
        
        if (showOnlyWhenDamaged && !alwaysShow)
        {
            SetVisibility(false);
        }
        
        ValidateSetup();
    }
    
    private void FindCamera()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    mainCamera = cam;
                    break;
                }
            }
            
            if (mainCamera == null && cameras.Length > 0)
            {
                mainCamera = cameras[0];
            }
        }
    }
    
    private void ValidateSetup()
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogWarning($"[WorldSpaceHealthBar] {name}: No Canvas found! Health bar will not display.", this);
        }
        
        if (healthSlider == null)
        {
            Debug.LogWarning($"[WorldSpaceHealthBar] {name}: No Slider found! Health bar will not update.", this);
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning($"[WorldSpaceHealthBar] {name}: No camera found! Health bar will not face camera.", this);
        }
        
        if (targetHealth == null)
        {
            Debug.LogWarning($"[WorldSpaceHealthBar] {name}: No JUHealth component found! Health bar will not function.", this);
        }
    }
    
    private void LateUpdate()
    {
        if (targetHealth == null || targetTransform == null) return;
        
        if (mainCamera == null)
        {
            FindCamera();
            if (mainCamera != null && worldSpaceCanvas != null)
            {
                worldSpaceCanvas.worldCamera = mainCamera;
            }
        }
        
        UpdatePosition();
        UpdateHealthBar();
        UpdateVisibility();
        FaceCamera();
    }
    
    private void UpdatePosition()
    {
        if (targetTransform == null) return;
        
        // Directly set position to avoid jitter from lerping
        Vector3 targetPosition = targetTransform.position + worldOffset;
        transform.position = targetPosition;
    }
    
    private void UpdateHealthBar()
    {
        if (targetHealth == null) return;
        
        float currentHealth = targetHealth.Health;
        float maxHealth = targetHealth.MaxHealth;
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        
        if (healthSlider != null)
        {
            // Only lerp if there's a significant difference to avoid jitter
            float difference = Mathf.Abs(healthSlider.value - healthPercent);
            if (difference > 0.001f)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, healthPercent, smoothSpeed * Time.deltaTime);
            }
            else
            {
                healthSlider.value = healthPercent;
            }
        }
        
        if (fillImage != null)
        {
            fillImage.color = GetHealthColor(healthPercent);
        }
        
        if (currentHealth < lastHealthValue && showOnlyWhenDamaged)
        {
            SetVisibility(true);
            hideTimer = hideDelay;
        }
        
        lastHealthValue = currentHealth;
    }
    
    private void UpdateVisibility()
    {
        if (alwaysShow)
        {
            SetVisibility(true);
            return;
        }
        
        if (!showOnlyWhenDamaged)
        {
            bool shouldShow = CheckDistanceVisibility();
            SetVisibility(shouldShow);
            return;
        }
        
        if (isVisible && hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            
            if (hideTimer <= 0f)
            {
                SetVisibility(false);
            }
        }
        
        if (!CheckDistanceVisibility())
        {
            SetVisibility(false);
        }
    }
    
    private bool CheckDistanceVisibility()
    {
        if (mainCamera == null || targetTransform == null) return false;
        
        float distance = Vector3.Distance(mainCamera.transform.position, targetTransform.position);
        return distance <= maxVisibleDistance;
    }
    
    private void FaceCamera()
    {
        if (mainCamera == null) return;
        
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
    
    private void SetVisibility(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
        else if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(visible);
        }
    }
    
    private Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > midHealthThreshold)
        {
            float t = (healthPercent - midHealthThreshold) / (1f - midHealthThreshold);
            return Color.Lerp(midHealthColor, fullHealthColor, t);
        }
        else if (healthPercent > lowHealthThreshold)
        {
            float t = (healthPercent - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else
        {
            return lowHealthColor;
        }
    }
    
    public void SetName(string characterName)
    {
        if (nameText != null)
            nameText.text = characterName;
    }
    
    public void SetLevel(int level)
    {
        if (levelText != null)
            levelText.text = level.ToString();
    }
    
    public void SetTargetHealth(JUHealth health)
    {
        targetHealth = health;
        if (targetHealth != null)
        {
            lastHealthValue = targetHealth.Health;
            UpdateHealthBar();
        }
    }
    
    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }
    
    public void ShowTemporarily(float duration = 3f)
    {
        SetVisibility(true);
        hideTimer = duration;
    }
}
