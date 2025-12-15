using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PickupEvent : UnityEvent<LootItemData, int, LootManager.Rarity> { }

public class ExtendedPickupSystem : MonoBehaviour
{
    public static ExtendedPickupSystem Instance { get; private set; }
    
    [Header("Pickup Settings")]
    [Tooltip("Layer for pickupable items")]
    public LayerMask pickupLayer = 1 << 12;
    
    [Tooltip("Pickup detection radius")]
    public float pickupRadius = 2f;
    
    [Tooltip("Offset from player position")]
    public Vector3 detectionOffset = Vector3.zero;
    
    [Tooltip("Hold time required to pick up item")]
    public float holdTimeToPickup = 0.1f;
    
    [Tooltip("Should pickup be automatic on trigger enter?")]
    public bool autoPickup = false;
    
    [Header("UI Settings")]
    [Tooltip("Show pickup prompts")]
    public bool showPickupPrompts = true;
    
    [Tooltip("Pickup prompt message")]
    public string pickupPromptFormat = "[E] Pick up {0}";
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    
    [Header("Events")]
    public PickupEvent onItemPickedUp;
    public UnityEvent<WorldPickupItem> onPickupAvailable;
    public UnityEvent onPickupUnavailable;
    
    private WorldPickupItem currentNearbyItem;
    private float currentHoldTime;
    private bool isHoldingPickup;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    private void Update()
    {
        CheckForNearbyPickups();
        HandlePickupInput();
    }
    
    private void CheckForNearbyPickups()
    {
        Vector3 checkPosition = transform.position + detectionOffset;
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, pickupRadius, pickupLayer);
        
        WorldPickupItem closestItem = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in hitColliders)
        {
            WorldPickupItem pickupItem = col.GetComponent<WorldPickupItem>();
            if (pickupItem != null && pickupItem.enabled)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = pickupItem;
                }
            }
        }
        
        if (closestItem != currentNearbyItem)
        {
            if (currentNearbyItem != null)
            {
                onPickupUnavailable?.Invoke();
            }
            
            currentNearbyItem = closestItem;
            
            if (currentNearbyItem != null)
            {
                onPickupAvailable?.Invoke(currentNearbyItem);
                
                if (showPickupPrompts && NotificationManager.Instance != null)
                {
                    string message = string.Format(pickupPromptFormat, currentNearbyItem.itemData.itemName);
                    NotificationManager.Instance.ShowNotification(message, 0.5f);
                }
                
                if (autoPickup)
                {
                    TryPickupItem(currentNearbyItem);
                }
            }
        }
    }
    
    private void HandlePickupInput()
    {
        if (currentNearbyItem == null) return;
        
        bool pickupInput = Input.GetKey(KeyCode.E);
        
        if (pickupInput)
        {
            if (!isHoldingPickup)
            {
                isHoldingPickup = true;
                currentHoldTime = 0f;
            }
            
            currentHoldTime += Time.deltaTime;
            
            if (currentHoldTime >= holdTimeToPickup)
            {
                TryPickupItem(currentNearbyItem);
                isHoldingPickup = false;
                currentHoldTime = 0f;
            }
        }
        else
        {
            isHoldingPickup = false;
            currentHoldTime = 0f;
        }
    }
    
    public void TryPickupItem(WorldPickupItem pickupItem)
    {
        if (pickupItem == null || pickupItem.itemData == null) return;
        
        bool success = PickupItem(
            pickupItem.itemData,
            pickupItem.gearScore,
            pickupItem.rarity
        );
        
        if (success)
        {
            PlayPickupSound();
            
            onItemPickedUp?.Invoke(pickupItem.itemData, pickupItem.gearScore, pickupItem.rarity);
            
            if (NotificationManager.Instance != null)
            {
                string rarityColor = GetRarityColor(pickupItem.rarity);
                string message = $"Picked up <color={rarityColor}>{pickupItem.itemData.itemName}</color>";
                NotificationManager.Instance.ShowNotification(message);
            }
            
            Destroy(pickupItem.gameObject);
            
            if (currentNearbyItem == pickupItem)
            {
                currentNearbyItem = null;
                onPickupUnavailable?.Invoke();
            }
        }
    }
    
    private bool PickupItem(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        if (itemData.itemType == LootItemData.ItemType.Weapon)
        {
            return PickupWeapon(itemData, gearScore, rarity);
        }
        else
        {
            return PickupGeneralItem(itemData, gearScore, rarity);
        }
    }
    
    private bool PickupWeapon(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        if (PlayerInventory.Instance != null)
        {
            return PlayerInventory.Instance.AddItem(itemData, gearScore, rarity);
        }
        
        Debug.LogWarning("PlayerInventory not found! Cannot pickup weapon.");
        return false;
    }
    
    private bool PickupGeneralItem(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        if (PlayerInventory.Instance != null)
        {
            return PlayerInventory.Instance.AddItem(itemData, gearScore, rarity);
        }
        
        Debug.LogWarning("PlayerInventory not found! Cannot pickup item.");
        return false;
    }
    
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    private string GetRarityColor(LootManager.Rarity rarity)
    {
        switch (rarity)
        {
            case LootManager.Rarity.Common:
                return "#FFFFFF";
            case LootManager.Rarity.Uncommon:
                return "#00FF00";
            case LootManager.Rarity.Rare:
                return "#0070FF";
            case LootManager.Rarity.Epic:
                return "#A335EE";
            case LootManager.Rarity.Legendary:
                return "#FF8000";
            default:
                return "#FFFFFF";
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkPosition = transform.position + detectionOffset;
        Gizmos.DrawWireSphere(checkPosition, pickupRadius);
    }
}
