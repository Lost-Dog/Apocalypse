using UnityEngine;
using UnityEngine.UI;

public class LootCompassMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    [Tooltip("Show marker on screen edge when loot is off-screen")]
    public bool showOffScreenMarkers = true;
    
    [Tooltip("Marker icon for UI")]
    public Sprite markerIcon;
    
    [Tooltip("Marker size")]
    public float markerSize = 30f;
    
    [Tooltip("Distance to show marker")]
    public float maxMarkerDistance = 100f;
    
    [Tooltip("Minimum distance to show marker")]
    public float minMarkerDistance = 5f;
    
    [Header("Display")]
    [Tooltip("Show distance text")]
    public bool showDistance = true;
    
    [Tooltip("Show item name")]
    public bool showItemName = false;
    
    private GameObject markerObject;
    private Image markerImage;
    private Canvas worldCanvas;
    private Transform playerTransform;
    private Camera mainCamera;
    private WorldPickupItem pickupItem;
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
        pickupItem = GetComponent<WorldPickupItem>();
        
        if (showOffScreenMarkers)
        {
            CreateWorldSpaceMarker();
        }
    }
    
    private void Update()
    {
        if (playerTransform == null || mainCamera == null || !showOffScreenMarkers)
            return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance < minMarkerDistance || distance > maxMarkerDistance)
        {
            if (markerObject != null)
                markerObject.SetActive(false);
            return;
        }
        
        UpdateMarkerVisibility();
    }
    
    private void CreateWorldSpaceMarker()
    {
        GameObject canvasObj = new GameObject("LootMarkerCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * 2f;
        
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100);
        canvasRect.localScale = Vector3.one * 0.01f;
        
        markerObject = new GameObject("Marker");
        markerObject.transform.SetParent(canvasObj.transform);
        
        RectTransform markerRect = markerObject.AddComponent<RectTransform>();
        markerRect.sizeDelta = new Vector2(markerSize, markerSize);
        markerRect.anchoredPosition = Vector3.zero;
        
        markerImage = markerObject.AddComponent<Image>();
        
        if (markerIcon != null)
        {
            markerImage.sprite = markerIcon;
        }
        
        if (pickupItem != null)
        {
            Color rarityColor = GetRarityColor(pickupItem.rarity);
            markerImage.color = rarityColor;
        }
        else
        {
            markerImage.color = Color.yellow;
        }
    }
    
    private void UpdateMarkerVisibility()
    {
        if (markerObject == null) return;
        
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        bool isVisible = screenPoint.z > 0 && 
                        screenPoint.x > 0 && screenPoint.x < 1 && 
                        screenPoint.y > 0 && screenPoint.y < 1;
        
        markerObject.SetActive(!isVisible);
        
        if (worldCanvas != null)
        {
            worldCanvas.transform.LookAt(playerTransform);
            worldCanvas.transform.Rotate(0, 180, 0);
        }
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float alpha = Mathf.Clamp01(1f - (distance / maxMarkerDistance));
        
        if (markerImage != null)
        {
            Color currentColor = markerImage.color;
            currentColor.a = alpha;
            markerImage.color = currentColor;
        }
    }
    
    private Color GetRarityColor(LootManager.Rarity rarity)
    {
        switch (rarity)
        {
            case LootManager.Rarity.Common:
                return Color.white;
            case LootManager.Rarity.Uncommon:
                return Color.green;
            case LootManager.Rarity.Rare:
                return Color.blue;
            case LootManager.Rarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f);
            case LootManager.Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.yellow;
        }
    }
}
