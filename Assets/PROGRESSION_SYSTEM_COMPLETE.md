# Progression System - Complete Implementation Guide

## Overview

The Progression System provides comprehensive player advancement through:
- **XP-based leveling** (Levels 1-10)
- **Cumulative XP requirements** (100 → 4,500 total XP)
- **Skill points** earned per level
- **Gear score integration** with equipment system
- **Power level calculation** combining player level + gear

---

## 🎯 System Components

### 1. ProgressionManager
**Location:** `/GameSystems/ProgressionManager`
**Purpose:** Core progression tracking and leveling

### 2. PlayerInventory
**Location:** Auto-created singleton
**Purpose:** Gear score tracking and equipment management

### 3. Integration Points
- ChallengeManager (XP rewards)
- LootManager (gear score calculation)
- GameManager (power level tracking)
- UI systems (XP bars, level display)

---

## 📊 Level Progression Table

### XP Requirements (Cumulative)

| Level | Total XP Required | XP to Next Level | Skill Points |
|-------|-------------------|------------------|--------------|
| 1     | 0                 | 100              | 0            |
| 2     | 100               | 200              | 1            |
| 3     | 300               | 300              | 2            |
| 4     | 600               | 400              | 3            |
| 5     | 1,000             | 500              | 4            |
| 6     | 1,500             | 600              | 5            |
| 7     | 2,100             | 700              | 6            |
| 8     | 2,800             | 800              | 7            |
| 9     | 3,600             | 900              | 8            |
| 10    | 4,500             | MAX              | 9            |

**Formula:** Each level requires progressively more XP
- Level 2: +100 XP
- Level 3: +200 XP
- Level 4: +300 XP
- etc. (increases by 100 each level)

---

## ⚙️ Skill Points System

### Earning Skill Points
- **Rate:** 1 skill point per level
- **Total Available:** 9 skill points at max level
- **Reset:** No automatic reset (manual refund only)

### Using Skill Points

```csharp
// Spend a skill point
if (ProgressionManager.Instance.SpendSkillPoint())
{
    Debug.Log("Skill point spent!");
    // Apply skill upgrade
}

// Refund a skill point (for respec)
ProgressionManager.Instance.RefundSkillPoint();

// Check available points
int available = ProgressionManager.Instance.skillPoints;
```

### Integration with Skill Trees
```csharp
public class SkillNode : MonoBehaviour
{
    public int skillPointCost = 1;
    
    public void UnlockSkill()
    {
        if (ProgressionManager.Instance.SpendSkillPoint())
        {
            // Grant skill bonus
            ApplySkillEffect();
        }
        else
        {
            Debug.Log("Not enough skill points!");
        }
    }
}
```

---

## 🎮 Gear Score System

### Overview
Gear score represents equipment quality and contributes to overall power level.

### Gear Score Sources
1. **Equipped Items** - Active gear contributing to power
2. **Total Inventory** - Average of all items owned

### Recommended Gear Score by Level

```
Formula: 100 + (Player Level × 40)

Level 1:  140 GS (100 + 40)
Level 2:  180 GS (100 + 80)
Level 3:  220 GS (100 + 120)
Level 4:  260 GS (100 + 160)
Level 5:  300 GS (100 + 200)
Level 6:  340 GS (100 + 240)
Level 7:  380 GS (100 + 280)
Level 8:  420 GS (100 + 320)
Level 9:  460 GS (100 + 360)
Level 10: 500 GS (100 + 400)
```

### Gear Quality Assessment

```csharp
// Check if player is undergeared
if (ProgressionManager.Instance.IsUndergeared())
{
    Debug.Log("Gear score too low! Upgrade equipment.");
}

// Check if player is overgeared
if (ProgressionManager.Instance.IsOvergeared())
{
    Debug.Log("Ready for harder content!");
}

// Get gear quality percentage (0-100%)
float quality = ProgressionManager.Instance.GetGearQuality();
// 0.75 = 75% of recommended (undergeared)
// 1.0 = 100% of recommended (perfect)
// 1.25 = 125% of recommended (overgeared)
```

---

## 💪 Power Level System

### What is Power Level?
Combined metric of player level + gear score contribution.

### Calculation
```
Power Level = Player Level + (Equipped Gear Score / 100)

Examples:
- Level 5, 200 GS equipped → 5 + 2 = 7 power
- Level 8, 400 GS equipped → 8 + 4 = 12 power
- Level 10, 500 GS equipped → 10 + 5 = 15 power (max)
```

### Usage

```csharp
// Get integer power level
int power = ProgressionManager.Instance.GetPowerLevel();

// Get precise power level
float precisePower = ProgressionManager.Instance.GetPowerLevelFloat();
// e.g., 7.45 for level 5 with 245 GS

// Use for content gating
if (ProgressionManager.Instance.GetPowerLevel() >= 10)
{
    UnlockEndgameContent();
}
```

### Power Level Ranges
- **Early Game:** 1-5 power
- **Mid Game:** 6-10 power
- **Late Game:** 11-15 power

---

## 🔄 Experience Gain

### Adding Experience

```csharp
// From kill rewards
ProgressionManager.Instance.AddExperience(50);

// From challenge completion
ProgressionManager.Instance.AddExperience(1500);

// From quest rewards
ProgressionManager.Instance.AddExperience(200);
```

### XP Sources
1. **Enemy Kills:** 10-50 XP per enemy
2. **Challenge Completion:** 500-3000 XP per challenge
3. **Quest Completion:** 100-500 XP per quest
4. **Discovery:** 25-100 XP per location
5. **Bonus Objectives:** 50-200 XP per bonus

### Level-Up Process

```
Player gains XP
     ↓
AddExperience() called
     ↓
currentXP increases (cumulative)
     ↓
onXPGained event fires
     ↓
CheckLevelUp() executes
     ↓
IF currentXP >= required XP:
  ├─ currentLevel++
  ├─ skillPoints++
  ├─ onLevelUp event fires
  ├─ onSkillPointGained event fires
  └─ Update GameManager
```

### Multi-Level Gains
The system supports gaining multiple levels at once:

```csharp
// Gain massive XP (e.g., from rare challenge)
ProgressionManager.Instance.AddExperience(3000);

// Will level up multiple times automatically
// Level 1 → 2 → 3 → 4 in one call
```

---

## 🎯 Equipment Integration

### Equipping Items

```csharp
// Equip an item
InventoryItem helmet = GetHelmetItem();
PlayerInventory.Instance.EquipItem(helmet);
// Automatically updates ProgressionManager gear score

// Unequip an item
PlayerInventory.Instance.UnequipItem(helmet);

// Toggle equipped status
PlayerInventory.Instance.ToggleEquip(helmet);
```

### Automatic Gear Score Updates

The system automatically updates ProgressionManager when:
- Items are added to inventory
- Items are removed from inventory
- Items are equipped
- Items are unequipped

```csharp
// This happens automatically
private void UpdateProgressionGearScore()
{
    int equippedGS = GetTotalGearScore();    // Equipped only
    int averageGS = GetAverageGearScore();   // All items
    
    ProgressionManager.Instance.UpdateEquippedGearScore(equippedGS);
    ProgressionManager.Instance.UpdateGearScore(averageGS);
}
```

### Getting Gear Scores

```csharp
// Total equipped gear score
int equipped = PlayerInventory.Instance.GetTotalGearScore();

// Average of all items
int average = PlayerInventory.Instance.GetAverageGearScore();

// Highest item ever obtained
int highest = PlayerInventory.Instance.highestGearScore;

// From ProgressionManager
int currentGS = ProgressionManager.Instance.currentGearScore;
int equippedGS = ProgressionManager.Instance.equippedGearScore;
```

---

## 📡 Events System

### Available Events

```csharp
public UnityEvent<int> onLevelUp;
public UnityEvent<int> onXPGained;
public UnityEvent<int> onSkillPointGained;
public UnityEvent<int> onGearScoreChanged;
```

### Event Subscriptions

```csharp
void Start()
{
    ProgressionManager pm = ProgressionManager.Instance;
    
    pm.onLevelUp.AddListener(OnLevelUp);
    pm.onXPGained.AddListener(OnXPGained);
    pm.onSkillPointGained.AddListener(OnSkillPointGained);
    pm.onGearScoreChanged.AddListener(OnGearScoreChanged);
}

void OnLevelUp(int newLevel)
{
    Debug.Log($"Level Up! Now level {newLevel}");
    ShowLevelUpAnimation();
    PlayLevelUpSound();
}

void OnXPGained(int amount)
{
    Debug.Log($"+{amount} XP");
    ShowXPPopup(amount);
}

void OnSkillPointGained(int amount)
{
    Debug.Log($"+{amount} Skill Point");
    ShowSkillPointNotification();
}

void OnGearScoreChanged(int newGearScore)
{
    Debug.Log($"Gear Score: {newGearScore}");
    UpdateGearScoreUI();
}
```

---

## 🎨 UI Integration

### XP Bar

```csharp
public class XPBarUI : MonoBehaviour
{
    public Slider xpSlider;
    public TextMeshProUGUI xpText;
    
    void Update()
    {
        ProgressionManager pm = ProgressionManager.Instance;
        
        // Update slider (0-1 progress)
        xpSlider.value = pm.GetXPProgress();
        
        // Update text
        int xpIntoLevel = pm.currentXP - pm.GetRequiredXPForLevel(pm.currentLevel - 1);
        int xpNeeded = pm.GetRequiredXPForLevel(pm.currentLevel) - pm.GetRequiredXPForLevel(pm.currentLevel - 1);
        
        xpText.text = $"{xpIntoLevel} / {xpNeeded}";
    }
}
```

### Level Display

```csharp
public class LevelDisplay : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    
    void Update()
    {
        levelText.text = ProgressionManager.Instance.currentLevel.ToString();
    }
}
```

### Gear Score Display

```csharp
public class GearScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI gearScoreText;
    public Image qualityBar;
    
    void Update()
    {
        ProgressionManager pm = ProgressionManager.Instance;
        
        gearScoreText.text = $"GS: {pm.equippedGearScore}";
        qualityBar.fillAmount = pm.GetGearQuality();
        
        // Color code based on quality
        if (pm.IsUndergeared())
            qualityBar.color = Color.red;
        else if (pm.IsOvergeared())
            qualityBar.color = Color.green;
        else
            qualityBar.color = Color.yellow;
    }
}
```

### Power Level Display

```csharp
public class PowerLevelDisplay : MonoBehaviour
{
    public TextMeshProUGUI powerText;
    
    void Update()
    {
        int power = ProgressionManager.Instance.GetPowerLevel();
        powerText.text = $"Power: {power}";
    }
}
```

---

## 🔧 API Reference

### ProgressionManager Methods

#### Experience & Leveling
```csharp
void AddExperience(int amount)
// Add XP and trigger level-up checks

int GetRequiredXPForLevel(int level)
// Get total cumulative XP needed for a level

int GetXPToNextLevel()
// Get remaining XP needed for next level

float GetXPProgress()
// Get 0-1 progress toward next level (for UI sliders)

bool IsMaxLevel()
// Check if player reached max level
```

#### Skill Points
```csharp
bool SpendSkillPoint()
// Spend 1 skill point, returns true if successful

void RefundSkillPoint()
// Refund 1 skill point (for respec)
```

#### Gear Score
```csharp
void UpdateGearScore(int newGearScore)
// Update average gear score (all items)

void UpdateEquippedGearScore(int newEquippedGearScore)
// Update equipped gear score

int GetRecommendedGearScore()
// Get recommended GS for current level

bool IsUndergeared()
// Check if < 75% of recommended GS

bool IsOvergeared()
// Check if > 125% of recommended GS

float GetGearQuality()
// Get 0-1 quality rating vs recommended
```

#### Power Level
```csharp
int GetPowerLevel()
// Get combined power (level + gear/100)

float GetPowerLevelFloat()
// Get precise power level
```

### PlayerInventory Methods

#### Item Management
```csharp
bool AddItem(LootItemData itemData, int gearScore, Rarity rarity)
// Add item to inventory

bool RemoveItem(InventoryItem item)
// Remove item from inventory

void EquipItem(InventoryItem item)
// Equip item (updates gear score)

void UnequipItem(InventoryItem item)
// Unequip item (updates gear score)

void ToggleEquip(InventoryItem item)
// Toggle equipped status
```

#### Queries
```csharp
List<InventoryItem> GetItemsByType(ItemType type)
// Get all items of specific type

List<InventoryItem> GetItemsByRarity(Rarity rarity)
// Get all items of specific rarity

List<InventoryItem> GetEquippedItems()
// Get all equipped items

int GetTotalGearScore()
// Get total GS of equipped items

int GetAverageGearScore()
// Get average GS of all items
```

---

## 📈 Balancing Guidelines

### XP Curve Design
The current progression requires:
- **Total XP to Max:** 4,500 XP
- **Average per Level:** 450-500 XP
- **Designed for:** 40-60 hours to max level

### Challenge XP Rewards
- **Easy:** 500-800 XP
- **Medium:** 1000-1500 XP
- **Hard:** 1500-2500 XP
- **Extreme:** 2500-4000 XP

### Enemy XP Values
- **Common:** 10-20 XP
- **Elite:** 30-50 XP
- **Boss:** 100-200 XP

### Gear Progression
Players should:
- **Replace gear every 1-2 levels**
- **Find 2-3 upgrades per level**
- **Reach 500 GS by level 10**

---

## 🎯 Usage Examples

### Example 1: Kill Reward
```csharp
public class Enemy : MonoBehaviour
{
    public int xpReward = 25;
    
    void OnDeath()
    {
        // Award XP
        ProgressionManager.Instance.AddExperience(xpReward);
        
        // Drop loot based on player level
        int playerLevel = ProgressionManager.Instance.currentLevel;
        LootManager.Instance.DropLoot(transform.position, playerLevel);
    }
}
```

### Example 2: Challenge Completion
```csharp
public class ChallengeRewards
{
    void GiveRewards(ActiveChallenge challenge)
    {
        // Base XP
        int baseXP = 1000;
        
        // Difficulty multiplier
        float difficultyMult = challenge.challengeData.GetDifficultyXPMultiplier();
        
        // Bonus for perfect completion
        float bonusMult = challenge.IsPerfectCompletion() ? 2.0f : 1.0f;
        
        int totalXP = (int)(baseXP * difficultyMult * bonusMult);
        
        ProgressionManager.Instance.AddExperience(totalXP);
    }
}
```

### Example 3: Skill Tree
```csharp
public class SkillTree : MonoBehaviour
{
    public void UnlockHealthBoost()
    {
        if (ProgressionManager.Instance.SpendSkillPoint())
        {
            // Increase max health
            GetComponent<JUHealth>().MaxHealth += 20;
            Debug.Log("Health increased!");
        }
    }
    
    public void RespecSkills()
    {
        // Refund all spent points
        int level = ProgressionManager.Instance.currentLevel;
        int totalPoints = level - 1; // 1 per level
        
        ProgressionManager.Instance.skillPoints = totalPoints;
        
        // Reset all unlocked skills
        ResetAllSkillBonuses();
    }
}
```

### Example 4: Content Gating
```csharp
public class RaidEntrance : MonoBehaviour
{
    public int requiredPowerLevel = 10;
    public int recommendedGearScore = 400;
    
    public bool CanEnter()
    {
        ProgressionManager pm = ProgressionManager.Instance;
        
        // Check power level
        if (pm.GetPowerLevel() < requiredPowerLevel)
        {
            Debug.Log($"Need power level {requiredPowerLevel}!");
            return false;
        }
        
        // Warn if undergeared
        if (pm.equippedGearScore < recommendedGearScore)
        {
            Debug.LogWarning("You're undergeared for this content!");
            // Allow entry but warn player
        }
        
        return true;
    }
}
```

---

## 🐛 Troubleshooting

### Player Not Gaining XP
**Check:**
- ProgressionManager exists in scene
- Not already max level
- AddExperience() being called
- Check console for "Gained X XP" messages

### Level Not Increasing
**Check:**
- Current XP >= required XP for next level
- Not at max level (10)
- CheckLevelUp() being called
- No errors in console

### Gear Score Not Updating
**Check:**
- PlayerInventory.Instance not null
- Items being equipped (not just added)
- ProgressionManager.Instance accessible
- UpdateProgressionGearScore() being called

### UI Not Updating
**Check:**
- References to ProgressionManager set
- Update() method running
- Text components assigned
- Values being read correctly

---

## ✨ Best Practices

### 1. Save Progression
```csharp
// Save on level up
ProgressionManager.Instance.onLevelUp.AddListener((level) => {
    SaveSystem.SavePlayerProgress();
});
```

### 2. Prevent Exploits
```csharp
// Cap XP gain per frame
private const int MAX_XP_PER_FRAME = 5000;

public void AddExperience(int amount)
{
    amount = Mathf.Min(amount, MAX_XP_PER_FRAME);
    // ... rest of method
}
```

### 3. Balance Feedback
```csharp
// Track time to each level
private float levelStartTime;

void OnLevelUp(int newLevel)
{
    float timeSpent = Time.time - levelStartTime;
    Debug.Log($"Level {newLevel} reached in {timeSpent}s");
    levelStartTime = Time.time;
}
```

### 4. Gear Score Warnings
```csharp
// Warn when entering hard content
void OnEnterDangerZone()
{
    if (ProgressionManager.Instance.IsUndergeared())
    {
        ShowWarning("Your gear score is low for this area!");
    }
}
```

---

## 📝 Summary

The Progression System provides:

✅ **XP-Based Leveling** - 10 levels, cumulative XP (100 → 4,500)
✅ **Skill Points** - 1 per level, 9 total
✅ **Gear Score Tracking** - Automatic updates from equipment
✅ **Power Level** - Combined player level + gear metric
✅ **Event System** - React to progression changes
✅ **Full Integration** - Works with challenges, loot, inventory

**Complete and ready for use!**
