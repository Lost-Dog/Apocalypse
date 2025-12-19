using UnityEngine;

public class ConsumablePickup : MonoBehaviour
{
    [Header("Consumable Data")]
    [Tooltip("The consumable item data")]
    public ConsumableItem itemData;
    
    [Header("Auto-Consume Settings")]
    [Tooltip("Auto-consume on pickup instead of adding to inventory")]
    public bool autoConsumeOnPickup = true;
    
    [Tooltip("Show pickup notification")]
    public bool showNotification = true;
    
    [Header("Visual")]
    public GameObject visualPrefab;
    
    private bool hasBeenPickedUp = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPickedUp)
            return;
        
        if (!other.CompareTag("Player"))
            return;
        
        if (itemData == null)
        {
            Debug.LogWarning($"ConsumablePickup on {gameObject.name} has no item data assigned!");
            return;
        }
        
        if (autoConsumeOnPickup)
        {
            ConsumeItem(other.gameObject);
        }
        else
        {
            AddToInventory(other.gameObject);
        }
    }
    
    private void ConsumeItem(GameObject player)
    {
        hasBeenPickedUp = true;
        
        itemData.Use(player);
        
        if (showNotification && NotificationManager.Instance != null)
        {
            string message = GetConsumptionMessage();
            NotificationManager.Instance.ShowNotification(message);
        }
        
        Destroy(gameObject);
    }
    
    private void AddToInventory(GameObject player)
    {
        hasBeenPickedUp = true;
        
        if (showNotification && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"Picked up {itemData.itemName}");
        }
        
        Destroy(gameObject);
    }
    
    private string GetConsumptionMessage()
    {
        string msg = $"Consumed {itemData.itemName}";
        
        if (itemData.hungerRestore > 0f)
            msg += $" (+{itemData.hungerRestore:F0} hunger)";
        
        if (itemData.thirstRestore > 0f)
            msg += $" (+{itemData.thirstRestore:F0} thirst)";
        
        if (itemData.healthRestore > 0f)
            msg += $" (+{itemData.healthRestore:F0} HP)";
        
        return msg;
    }
    
    public static GameObject CreatePickup(ConsumableItem itemData, Vector3 position, bool autoConsume = true)
    {
        GameObject pickup = new GameObject($"Pickup_{itemData.itemName}");
        pickup.transform.position = position;
        
        SphereCollider trigger = pickup.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1f;
        
        ConsumablePickup consumable = pickup.AddComponent<ConsumablePickup>();
        consumable.itemData = itemData;
        consumable.autoConsumeOnPickup = autoConsume;
        
        return pickup;
    }
}
