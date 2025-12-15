# LootManager System - Complete Guide

## Overview
The LootManager system provides a complete loot and inventory solution with persistent storage between game sessions. The system includes:

- **LootManager**: Manages loot spawning, rarity rolls, and gear score calculation
- **PlayerInventory**: Tracks collected items with automatic save/load using PlayerPrefs
- **LootItem**: Individual loot pickup behavior in the world
- **LootItemData**: ScriptableObject-based item definitions with gear scores

---

## Quick Start

### 1. Create Loot Items
You can create loot items in two ways:

#### Option A: Using the Creator Tool
1. Go to menu: `Apocalypse > Loot > Create Loot Item`
2. Fill in the item details (name, rarity, type, etc.)
3. Click "Create Loot Item"
4. Items are saved to `/Assets/Game/Loot/Items`

#### Option B: Using the Asset Menu
1. Right-click in Project window
2. Select `Create > Apocalypse > Loot > Loot Item Data`
3. Configure the item properties in the Inspector

### 2. Configure LootManager
1. Select the `GameSystems/LootManager` GameObject in your scene
2. In the Inspector, you'll see:

#### Lootable Items Database
- Click "Auto-Find All Loot Items in Project" to populate the list automatically
- OR manually add your LootItemData assets to the list
- Each item must have:
  - **Item ID**: Unique identifier (auto-generated)
  - **Item Name**: Display name
  - **Rarity**: Common, Uncommon, Rare, Epic, or Legendary
  - **Item Type**: Weapon, Armor, Gear, Consumable, Material, or Collectible
  - **Base Gear Score**: Starting gear score value
  - **Icon**: Sprite for UI display
  - **World Prefab**: Optional 3D prefab for world representation
  - **Description**: Item description text

#### Loot Prefab Pools
- Configure fallback prefabs for each rarity tier
- Used when LootItemData doesn't have a custom worldPrefab
- Set the default loot prefab and drop force

#### Gear Score Settings
- **Min Gear Score**: Minimum possible gear score (default: 100)
- **Max Gear Score**: Maximum possible gear score (default: 500)
- Formula: `Base (100) + (Level × 40) + (Rarity × 50) ± Random(10)`

#### Rarity Drop Chances
- Adjust the probability for each rarity tier
- Values should sum to 1.0 for proper distribution
- Defaults:
  - Common: 50%
  - Uncommon: 25%
  - Rare: 15%
  - Epic: 8%
  - Legendary: 2%

### 3. Spawn Loot in Your Game

#### Method 1: Using LootSpawner Component
Add the `LootSpawner` component to any GameObject (like enemies):

```csharp
// Automatically spawns when GameObject is destroyed
public bool spawnOnDestroy = true;
public int playerLevel = 1;
public int dropCount = 1;
```

#### Method 2: Via Code
```csharp
// Spawn random rarity loot
LootManager.Instance.DropLoot(position, playerLevel);

// Spawn specific rarity loot
LootManager.Instance.DropLootWithRarity(position, playerLevel, LootManager.Rarity.Legendary);
```

### 4. Testing the System

#### Option A: Use Debug Tester Component
1. Add `LootDebugTester` component to any GameObject
2. Press keys during Play Mode:
   - **L**: Spawn random loot
   - **K**: Spawn legendary loot
   - **I**: Print inventory to console
   - **Delete**: Clear inventory

#### Option B: Use Inspector Debug Tools
1. Select LootManager in the hierarchy
2. Expand "Debug Tools" section (only available in Play Mode)
3. Click buttons to spawn different rarity loot

---

## System Architecture

### LootItemData (ScriptableObject)
Defines a lootable item's properties. Create multiple instances for different items.

```
Properties:
- itemID: Unique identifier
- itemName: Display name
- rarity: Rarity tier (affects drop chance and gear score)
- itemType: Category (Weapon, Armor, etc.)
- baseGearScore: Base value before modifiers
- icon: UI sprite
- worldPrefab: 3D representation
- description: Flavor text
```

### LootManager (Singleton)
Central system managing loot spawning and distribution.

```csharp
// Key Methods:
void DropLoot(Vector3 position, int playerLevel)
void DropLootWithRarity(Vector3 position, int playerLevel, Rarity rarity)
void AddItemToInventory(LootItemData itemData, int gearScore, Rarity rarity)
LootItemData GetLootItemByID(string itemID)
Color GetRarityColor(Rarity rarity)
```

### PlayerInventory (Singleton)
Manages the player's collected items with persistent storage.

```csharp
// Key Methods:
bool AddItem(LootItemData itemData, int gearScore, Rarity rarity)
bool RemoveItem(InventoryItem item)
List<InventoryItem> GetItemsByType(ItemType type)
List<InventoryItem> GetItemsByRarity(Rarity rarity)
void SaveInventory()
void LoadInventory()
void ClearInventory()
void ResetSave()

// Properties:
int maxInventorySize
int totalItemsCollected
int highestGearScore
```

### LootItem (MonoBehaviour)
Handles individual loot pickup behavior in the world.

```csharp
// Configuration:
float pickupRadius = 2f
KeyCode pickupKey = KeyCode.E
LayerMask playerLayer

// Visual:
GameObject visualEffect
Light rarityLight
float bobHeight = 0.3f
float bobSpeed = 2f
```

---

## Persistence System

### Automatic Saving
- Inventory automatically saves when items are added/removed (if `autoSaveOnChange = true`)
- Saves on application quit
- Uses PlayerPrefs for storage

### Save Data Includes:
- All inventory items with properties
- Total items collected (lifetime stat)
- Highest gear score achieved
- Item equipped states
- Acquisition timestamps

### Manual Save/Load
```csharp
// Manual save
PlayerInventory.Instance.SaveInventory();

// Manual load
PlayerInventory.Instance.LoadInventory();

// Reset all save data
PlayerInventory.Instance.ResetSave();
```

---

## Integration Examples

### Enemy Drops Loot on Death
```csharp
public class Enemy : MonoBehaviour
{
    public int enemyLevel = 5;
    
    void Die()
    {
        // Spawn loot at enemy position
        LootManager.Instance.DropLoot(transform.position, enemyLevel);
        
        Destroy(gameObject);
    }
}
```

### Boss Drops Guaranteed Legendary
```csharp
public class Boss : MonoBehaviour
{
    void Die()
    {
        // Always drop legendary loot
        LootManager.Instance.DropLootWithRarity(
            transform.position, 
            30, 
            LootManager.Rarity.Legendary
        );
    }
}
```

### Check Inventory for Specific Item
```csharp
public class QuestChecker : MonoBehaviour
{
    void CheckQuestItem()
    {
        var weapons = PlayerInventory.Instance.GetItemsByType(LootItemData.ItemType.Weapon);
        
        foreach (var item in weapons)
        {
            if (item.gearScore >= 300)
            {
                Debug.Log("Quest complete! High-level weapon found!");
            }
        }
    }
}
```

### Display Inventory UI
```csharp
public class InventoryUI : MonoBehaviour
{
    void UpdateDisplay()
    {
        var inventory = PlayerInventory.Instance;
        
        // Sort by gear score
        inventory.SortByGearScore(descending: true);
        
        // Display items
        foreach (var item in inventory.items)
        {
            Debug.Log($"{item.itemName} - GS: {item.gearScore}");
        }
        
        // Show stats
        Debug.Log($"Total Items: {inventory.items.Count}");
        Debug.Log($"Average GS: {inventory.GetAverageGearScore()}");
    }
}
```

---

## Rarity System

### Rarity Tiers (in order):
1. **Common** (White)
2. **Uncommon** (Green)
3. **Rare** (Blue)
4. **Epic** (Purple)
5. **Legendary** (Orange)

### Rarity Colors
Access via `LootManager.Instance.GetRarityColor(rarity)`:
- Common: White
- Uncommon: Green
- Rare: Blue
- Epic: Purple
- Legendary: Orange

---

## Gear Score Calculation

The gear score is calculated using this formula:

```
Final GS = Base (100) + (Player Level × 40) + (Rarity Bonus) ± Variance

Rarity Bonuses:
- Common: 0
- Uncommon: 50
- Rare: 100
- Epic: 150
- Legendary: 200

Variance: Random(-10, 10)
```

Example:
- Level 10 player
- Epic rarity
- Result: 100 + (10 × 40) + 150 ± 10 = 640-660 GS

---

## Events

### Available Events:
1. **onLootDropped** - Fired when loot spawns in world
   - Parameters: `Rarity rarity, int gearScore`

2. **onItemCollected** - Fired when player picks up loot
   - Parameters: `LootItemData itemData, int gearScore, Rarity rarity`

### Usage Example:
```csharp
void Start()
{
    LootManager.Instance.onItemCollected.AddListener(OnItemCollected);
}

void OnItemCollected(LootItemData item, int gearScore, LootManager.Rarity rarity)
{
    Debug.Log($"Collected: {item.itemName} with GS {gearScore}!");
    // Show UI notification, play sound, etc.
}
```

---

## Best Practices

1. **Create Item Variety**: Make items for each rarity tier to ensure drops are diverse
2. **Balance Gear Scores**: Adjust min/max values based on your game's progression
3. **Test Drop Rates**: Use the debug tools to verify rarity distribution feels right
4. **Use World Prefabs**: Create unique 3D models for legendary/epic items
5. **Save Regularly**: Don't disable `autoSaveOnChange` unless you have a good reason
6. **Unique Item IDs**: Always use unique IDs - the system auto-generates GUIDs

---

## Troubleshooting

### "No lootable items found!"
- Run "Auto-Find All Loot Items" in the LootManager Inspector
- Verify you've created LootItemData assets
- Check that items are in the project (not just the scene)

### Items not spawning
- Verify LootManager exists in scene at `/GameSystems/LootManager`
- Check that lootable items list is populated
- Ensure you have world prefabs or fallback prefabs configured

### Inventory not persisting
- Check that `autoSaveOnChange` is enabled
- Verify PlayerInventory exists in scene
- Use `SaveInventory()` manually if needed
- Check Unity Console for save/load messages

### Loot not pickable
- Ensure player has the "Player" tag
- Check pickup radius on LootItem
- Verify player layer mask is set correctly
- Make sure Input System is installed

---

## File Structure

```
/Assets/Scripts/
  ├── LootManager.cs           # Main loot management system
  ├── LootItem.cs              # LootItemData + LootItem pickup
  ├── PlayerInventory.cs       # Inventory with persistence
  ├── LootSpawner.cs           # Helper component for spawning
  └── LootDebugTester.cs       # Testing utilities

/Assets/Scripts/Editor/
  ├── LootManagerEditor.cs     # Custom inspector for LootManager
  ├── LootItemCreator.cs       # Tool to create loot items
  └── PlayerInventoryEditor.cs # Custom inspector for inventory

/Assets/Game/Loot/
  ├── /Items/                  # LootItemData assets go here
  └── /Tables/                 # Future: Loot tables
```

---

## Next Steps

1. Create 10-20 loot items covering all rarity tiers
2. Set up world prefabs for visual variety
3. Add the LootSpawner component to enemies
4. Configure loot drop chances to match your game balance
5. Test with LootDebugTester to verify persistence
6. Build UI to display inventory to players
7. Integrate with your progression/leveling system

---

**Need Help?** Check the Unity Console for debug messages - the system logs all important operations!
