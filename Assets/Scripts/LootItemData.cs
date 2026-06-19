using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Item", menuName = "Apocalypse/Loot/Loot Item Data")]
public class LootItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemID;
    public string itemName;
    public LootRarity rarity;

    /// <summary>Inventory item id that corresponds to this loot entry in the item database.</summary>
    [Tooltip("Must match the id field of the linked inventory item definition.")]
    public int inventoryItemID;

    [Header("Item Properties")]
    public ItemType itemType;

    [Tooltip("Base gear score for this item (modified by player level and rarity).")]
    public int baseGearScore = 100;

    [Header("Visual")]
    public Sprite icon;
    public GameObject worldPrefab;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;

    [Header("Inventory Properties")]
    [Tooltip("Can this item be stacked in inventory?")]
    public bool isStackable = false;

    [Tooltip("Maximum stack size (if stackable).")]
    public int maxStackSize = 1;

    [Tooltip("Can this item be dropped?")]
    public bool isDroppable = true;

    [Tooltip("Can this item be sold?")]
    public bool isSellable = true;

    [Tooltip("Sell value (credits).")]
    public int sellValue = 0;

    [Header("Usage")]
    [Tooltip("Can this item be used/consumed?")]
    public bool isUsable = false;

    [Tooltip("Use cooldown in seconds.")]
    public float useCooldown = 0f;

    [Header("Heal On Pickup")]
    [Tooltip("When true, collecting this item instantly restores all survival traits and health.")]
    public bool healOnPickup = false;

    [Tooltip("Percentage of each stat's maximum to restore on pickup (0–100).")]
    [Range(0f, 100f)]
    public float healPercentage = 20f;

    public enum ItemType
    {
        Weapon,
        Armor,
        Gear,
        Consumable,
        Material,
        Collectible,
        Ammo,
        KeyItem
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = System.Guid.NewGuid().ToString();
        }
    }
}
