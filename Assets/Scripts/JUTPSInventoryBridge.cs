using UnityEngine;
using JUTPS;
using JUTPS.InventorySystem;
using JUTPS.ItemSystem;

public class JUTPSInventoryBridge : MonoBehaviour
{
    public static JUTPSInventoryBridge Instance { get; private set; }
    
    [Header("References")]
    [Tooltip("JUTPS Inventory component (for weapons)")]
    public JUInventory jutpsInventory;
    
    [Tooltip("Player Inventory component (for all items)")]
    public PlayerInventory playerInventory;
    
    [Header("Weapon Integration")]
    [Tooltip("Automatically add picked weapons to JUTPS inventory")]
    public bool autoEquipWeapons = true;
    
    [Tooltip("Map LootItemData weapons to JUTPS weapons by name")]
    public bool useNameMapping = true;
    
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
        
        if (jutpsInventory == null)
        {
            jutpsInventory = GetComponent<JUInventory>();
        }
        
        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
    }
    
    public void OnItemPickedUp(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        if (itemData.itemType == LootItemData.ItemType.Weapon && autoEquipWeapons)
        {
            TryEquipWeapon(itemData);
        }
    }
    
    private void TryEquipWeapon(LootItemData weaponData)
    {
        if (jutpsInventory == null)
        {
            Debug.LogWarning("JUInventory not assigned!");
            return;
        }
        
        foreach (var weapon in jutpsInventory.AllHoldableItems)
        {
            if (weapon != null && weapon.ItemName == weaponData.itemName)
            {
                weapon.Unlocked = true;
                weapon.ItemQuantity = Mathf.Max(1, weapon.ItemQuantity);
                
                Debug.Log($"Unlocked weapon in JUTPS inventory: {weaponData.itemName}");
                return;
            }
        }
        
        Debug.LogWarning($"Weapon '{weaponData.itemName}' not found in JUTPS inventory. " +
                        "Make sure you have a matching weapon in the character's inventory.");
    }
    
    public void OnItemUsed(LootItemData itemData)
    {
        if (itemData is ConsumableItem consumable)
        {
            consumable.Use(gameObject);
        }
        else if (itemData.itemType == LootItemData.ItemType.Ammo)
        {
            UseAmmo(itemData);
        }
    }
    
    private void UseAmmo(LootItemData ammoData)
    {
        if (jutpsInventory == null) return;
        
        JUCharacterController character = GetComponent<JUCharacterController>();
        if (character != null && character.IsItemEquiped)
        {
            if (character.RightHandWeapon != null)
            {
                character.RightHandWeapon.TotalBullets += 30;
                Debug.Log($"Added ammo to {character.RightHandWeapon.ItemName}");
            }
            
            if (character.LeftHandWeapon != null)
            {
                character.LeftHandWeapon.TotalBullets += 30;
                Debug.Log($"Added ammo to {character.LeftHandWeapon.ItemName}");
            }
        }
    }
    
    public JUItem FindJUTPSItem(string itemName)
    {
        if (jutpsInventory == null) return null;
        
        foreach (var item in jutpsInventory.AllItems)
        {
            if (item != null && item.ItemName == itemName)
            {
                return item;
            }
        }
        
        return null;
    }
}
