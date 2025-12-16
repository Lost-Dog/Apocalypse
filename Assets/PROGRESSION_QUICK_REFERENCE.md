# Progression System - Quick Reference

## ⚡ Quick Stats

**Levels:** 1-10  
**Total XP to Max:** 4,500  
**Skill Points:** 1 per level (9 total)  
**Power Level:** Player Level + (Gear Score / 100)

---

## 📊 XP Table

| Lvl | Total XP | To Next | SP |
|-----|----------|---------|-----|
| 1   | 0        | 100     | 0   |
| 2   | 100      | 200     | 1   |
| 3   | 300      | 300     | 2   |
| 4   | 600      | 400     | 3   |
| 5   | 1,000    | 500     | 4   |
| 6   | 1,500    | 600     | 5   |
| 7   | 2,100    | 700     | 6   |
| 8   | 2,800    | 800     | 7   |
| 9   | 3,600    | 900     | 8   |
| 10  | 4,500    | MAX     | 9   |

---

## 🎯 Gear Score by Level

| Level | Recommended GS | Min GS (75%) | Max GS (125%) |
|-------|----------------|--------------|---------------|
| 1     | 140            | 105          | 175           |
| 2     | 180            | 135          | 225           |
| 3     | 220            | 165          | 275           |
| 4     | 260            | 195          | 325           |
| 5     | 300            | 225          | 375           |
| 6     | 340            | 255          | 425           |
| 7     | 380            | 285          | 475           |
| 8     | 420            | 315          | 525           |
| 9     | 460            | 345          | 575           |
| 10    | 500            | 375          | 625           |

**Formula:** 100 + (Level × 40)

---

## 🔥 Common Code Snippets

### Add Experience
```csharp
ProgressionManager.Instance.AddExperience(amount);
```

### Check Level
```csharp
int level = ProgressionManager.Instance.currentLevel;
```

### Spend Skill Point
```csharp
if (ProgressionManager.Instance.SpendSkillPoint())
{
    // Success
}
```

### Get Power Level
```csharp
int power = ProgressionManager.Instance.GetPowerLevel();
```

### Check Gear Quality
```csharp
if (ProgressionManager.Instance.IsUndergeared())
{
    Debug.Log("Upgrade equipment!");
}
```

### Equip Item
```csharp
PlayerInventory.Instance.EquipItem(item);
```

### Get XP Progress
```csharp
float progress = ProgressionManager.Instance.GetXPProgress();
xpSlider.value = progress;
```

---

## 🎮 Typical XP Rewards

**Enemies:**
- Common: 10-20 XP
- Elite: 30-50 XP
- Boss: 100-200 XP

**Challenges:**
- Easy: 500-800 XP
- Medium: 1000-1500 XP
- Hard: 1500-2500 XP
- Extreme: 2500-4000 XP

**Activities:**
- Quest: 100-500 XP
- Discovery: 25-100 XP
- Bonus Objective: 50-200 XP

---

## 💪 Power Level Ranges

| Power | Category  | Description |
|-------|-----------|-------------|
| 1-5   | Early     | Starting content |
| 6-10  | Mid       | Standard content |
| 11-15 | Late      | Endgame content |

---

## 📡 Events

```csharp
// Subscribe to events
ProgressionManager.Instance.onLevelUp.AddListener(OnLevelUp);
ProgressionManager.Instance.onXPGained.AddListener(OnXPGained);
ProgressionManager.Instance.onSkillPointGained.AddListener(OnSkillPointGained);
ProgressionManager.Instance.onGearScoreChanged.AddListener(OnGearScoreChanged);
```

---

## 🔍 Quick Checks

```csharp
// Max level?
bool isMax = ProgressionManager.Instance.IsMaxLevel();

// Undergeared?
bool needsGear = ProgressionManager.Instance.IsUndergeared();

// Overgeared?
bool hasGoodGear = ProgressionManager.Instance.IsOvergeared();

// Gear quality (0-1)
float quality = ProgressionManager.Instance.GetGearQuality();

// XP to next level
int xpNeeded = ProgressionManager.Instance.GetXPToNextLevel();

// Recommended GS
int recommendedGS = ProgressionManager.Instance.GetRecommendedGearScore();
```

---

## 🎯 Key Properties

### ProgressionManager
```csharp
int currentLevel           // 1-10
int currentXP              // Cumulative XP
int skillPoints            // Available to spend
int currentGearScore       // Average of all items
int equippedGearScore      // Total of equipped items
int maxLevel               // Default: 10
```

### PlayerInventory
```csharp
List<InventoryItem> items         // All items
int maxInventorySize              // Default: 100
int totalItemsCollected           // Lifetime stat
int highestGearScore              // Best item ever
```

---

## ⚙️ Integration Points

**Challenge System:**
```csharp
ProgressionManager.Instance.AddExperience(challenge.xpReward);
```

**Loot System:**
```csharp
int playerLevel = ProgressionManager.Instance.currentLevel;
LootManager.Instance.DropLoot(position, playerLevel);
```

**Inventory System:**
```csharp
// Automatic - equipping items updates gear score
PlayerInventory.Instance.EquipItem(item);
```

**UI Systems:**
```csharp
// XP bar
xpBar.value = ProgressionManager.Instance.GetXPProgress();

// Level display
levelText.text = ProgressionManager.Instance.currentLevel.ToString();

// Gear score
gsText.text = $"GS: {ProgressionManager.Instance.equippedGearScore}";
```

---

## 🚀 Quick Start

1. **ProgressionManager** exists at `/GameSystems/ProgressionManager`
2. **PlayerInventory** auto-creates as singleton
3. **Add XP** via `AddExperience(amount)`
4. **Equip items** to increase power
5. **Spend skill points** for upgrades
6. **Check power level** for content gating

**That's it!** System is fully integrated and ready to use.
