using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorldPickupUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The world-space canvas displaying pickup info")]
    public GameObject pickupInfoPanel;
    
    [Tooltip("Text component showing item name")]
    public TextMeshProUGUI itemNameText;
    
    [Tooltip("Text component showing interaction prompt")]
    public TextMeshProUGUI interactionText;
    
    [Tooltip("Icon image for the item")]
    public Image itemIcon;
    
    [Header("Settings")]
    [Tooltip("Offset above the pickup object")]
    public Vector3 uiOffset = new Vector3(0, 2f, 0);
    
    [Tooltip("Should UI always face camera?")]
    public bool billboardToCamera = true;
    
    [Tooltip("Show/hide UI based on player distance")]
    public bool useDistanceVisibility = true;
    
    [Tooltip("Maximum distance to show UI")]
    public float maxVisibilityDistance = 10f;
    
    [Tooltip("Scale of the UI (adjust if too large/small)")]
    public float uiScale = 0.005f;
    
    [Header("Auto-Find")]
    [Tooltip("Automatically find UI components on Start")]
    public bool autoFindComponents = true;
    
    private Camera mainCamera;
    private Transform playerTransform;
    private ConsumableItem consumableData;
    
    private void Start()
    {
        mainCamera = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        if (autoFindComponents)
        {
        // Apply UI scale
        if (pickupInfoPanel != null)
        {
            pickupInfoPanel.transform.localScale = Vector3.one * uiScale;
            
            // Ensure it's set to world space
            Canvas canvas = pickupInfoPanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }
        }
        
            FindComponents();
        }
        
        UpdateUI();
    }
    
    private void FindComponents()
    {
        if (pickupInfoPanel == null)
        {
            pickupInfoPanel = transform.GetChild(0)?.gameObject;
        }
        
        if (pickupInfoPanel != null)
        {
            // Find TextMeshProUGUI components
            TextMeshProUGUI[] texts = pickupInfoPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name.Contains("Name") || text.name.Contains("Item"))
                {
                    itemNameText = text;
                }
                else if (text.name.Contains("Interaction") || text.name.Contains("Prompt") || text.name.Contains("Press"))
                {
                    interactionText = text;
                }
            }
            
            // Find Image component for icon
            Image[] images = pickupInfoPanel.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.Contains("Icon") && img != pickupInfoPanel.GetComponent<Image>())
                {
                    itemIcon = img;
                }
            }
        }
    }
    
    private void Update()
    {
        if (billboardToCamera && mainCamera != null && pickupInfoPanel != null)
        {
            pickupInfoPanel.transform.LookAt(pickupInfoPanel.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
        
        if (useDistanceVisibility && playerTransform != null && pickupInfoPanel != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            pickupInfoPanel.SetActive(distance <= maxVisibilityDistance);
        }
    }
    
    public void SetConsumableData(ConsumableItem data)
    {
        consumableData = data;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (consumableData == null) return;
        
        if (itemNameText != null)
        {
            itemNameText.text = consumableData.itemName;
        }
        
        if (interactionText != null)
        {
            interactionText.text = "[E] Pick Up";
        }
        
        if (itemIcon != null && consumableData.icon != null)
        {
            itemIcon.sprite = consumableData.icon;
            itemIcon.enabled = true;
        }
        else if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxVisibilityDistance);
    }
}
