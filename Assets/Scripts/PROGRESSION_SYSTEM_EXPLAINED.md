# Progression Manager - Level Up Mechanics Explained

## Overview

The `ProgressionManager` handles all player progression including XP gain, leveling up, and skill point rewards. It uses a cumulative XP system where XP never resets and levels are based on total XP thresholds.

---

## Core Components

### State Variables
- `currentLevel` - Player's current level (starts at 1)
- `currentXP` - Total cumulative XP earned (never decreases)
- `skillPoints` - Available skill points to spend
- `maxLevel` - Maximum achievable level (default: 10)

### XP Requirements Table

```
Level  | Total XP Required | XP to Next Level
-------|-------------------|------------------
  1    |        0          |      100
  2    |      100          |      200
  3    |      300          |      300
  4    |      600          |      400
  5    |    1,000          |      500
  6    |    1,500          |      600
  7    |    2,100          |      700
  8    |    2,800          |      800
  9    |    3,600          |      900
 10    |    4,500          |    (max)
```

---

## How Leveling Works

### Step-by-Step Process

#### 1. **Gaining XP** (Method: `AddExperience(int amount)`)

```
Player Action → Gain XP
    ↓
AddExperience(50) called
    ↓
currentXP += 50
    ↓
Fire onXPGained event
    ↓
CheckLevelUp()
```

**What happens:**
- XP is added to `currentXP` (cumulative total)
- `onXPGained` event fires (UI updates)
- System checks if player should level up

#### 2. **Checking for Level Up** (Method: `CheckLevelUp()`)

```
CheckLevelUp()
    ↓
Is currentLevel < maxLevel? ─── No ──> Exit
    ↓ Yes
Get required XP for current level
    ↓
While currentXP >= requiredXP AND currentLevel < maxLevel:
    ↓
    LevelUp()
    ↓
    Update required XP
    ↓
    Loop (allows multiple level ups at once!)
```

**Key Feature: Multiple Level-Ups**
The `while` loop allows gaining multiple levels in one XP grant!

Example:
```
Current: Level 1, XP: 0
Grant: 350 XP
Result: Level 1 → 2 → 3 (skips level 2 threshold at 100 XP)
```

#### 3. **Leveling Up** (Method: `LevelUp()`)

```
LevelUp()
    ↓
currentLevel++
    ↓
skillPoints += 1
    ↓
Fire onLevelUp event (with new level)
    ↓
Fire onSkillPointGained event (with amount)
    ↓
Update GameManager (if exists)
    ↓
Log "LEVEL UP!" message
```

**Rewards per Level:**
- +1 Level
- +1 Skill Point (constant: `SKILL_POINTS_PER_LEVEL`)

---

## Key Methods Explained

### `AddExperience(int amount)`
**Purpose:** Add XP to the player and trigger level-up checks

**Flow:**
1. Check if already max level → exit if true
2. Add amount to `currentXP`
3. Fire `onXPGained` event
4. Call `CheckLevelUp()`

**Example:**
```csharp
progressionManager.AddExperience(50);  // Gain 50 XP
```

---

### `GetRequiredXPForLevel(int level)`
**Purpose:** Get the total cumulative XP needed to reach a specific level

**Parameters:** `level` - The target level (0-9 array index)

**Returns:** Total XP threshold

**Examples:**
```
GetRequiredXPForLevel(0) = 0      // Level 1 start
GetRequiredXPForLevel(1) = 100    // To reach Level 2
GetRequiredXPForLevel(4) = 1000   // To reach Level 5
```

---

### `GetXPProgress()`
**Purpose:** Calculate progress toward next level as a 0-1 float (for UI sliders)

**Formula:**
```
progress = (currentXP - currentLevelStart) / (nextLevel - currentLevelStart)
```

**Example:**
```
Current Level: 3
Current XP: 400
Level 3 start: 300
Level 4 required: 600

Progress = (400 - 300) / (600 - 300)
         = 100 / 300
         = 0.333 (33.3% to next level)
```

**Returns:** `1.0` if max level reached

---

### `GetXPToNextLevel()`
**Purpose:** Calculate how much XP is still needed for the next level

**Formula:**
```
xpNeeded = GetRequiredXPForLevel(currentLevel) - currentXP
```

**Example:**
```
Current Level: 3
Current XP: 400
Level 4 requires: 600

XP to Next = 600 - 400 = 200
```

**Returns:** `0` if max level reached

---

## Progression Formula Breakdown

### XP Requirements Pattern
The XP requirements follow an increasing curve:

```
Level 2: 100  (+100 from level 1)
Level 3: 300  (+200 from level 2)
Level 4: 600  (+300 from level 3)
Level 5: 1000 (+400 from level 4)
...
```

Each level requires **100 more XP than the previous level gap**.

### Mathematical Pattern
```
XP[1] = 0
XP[2] = 100
XP[n] = XP[n-1] + (100 * (n-1))
```

---

## Skill Points System

### Earning Skill Points
- Gain **1 skill point** per level up
- Constant defined: `SKILL_POINTS_PER_LEVEL = 1`

### Managing Skill Points

#### `SpendSkillPoint()`
```csharp
bool success = progressionManager.SpendSkillPoint();
if (success)
{
    // Apply skill upgrade
}
```
Returns `true` if successful, `false` if no points available.

#### `RefundSkillPoint()`
```csharp
progressionManager.RefundSkillPoint();  // Returns 1 skill point
```
Used for skill respec or undo.

---

## Events System

### Available Events

#### `onLevelUp` (UnityEvent<int>)
- Fires when player gains a level
- Passes the new level number
- Subscribe to update UI, play effects, etc.

#### `onXPGained` (UnityEvent<int>)
- Fires when XP is added
- Passes the amount gained
- Subscribe for XP notification popups

#### `onSkillPointGained` (UnityEvent<int>)
- Fires when skill points are earned
- Passes the amount gained
- Subscribe for skill point notifications

### Event Usage Example
```csharp
void Start()
{
    ProgressionManager pm = GetComponent<ProgressionManager>();
    
    pm.onLevelUp.AddListener(OnPlayerLevelUp);
    pm.onXPGained.AddListener(OnXPGained);
    pm.onSkillPointGained.AddListener(OnSkillPointGained);
}

void OnPlayerLevelUp(int newLevel)
{
    Debug.Log($"Reached level {newLevel}!");
    PlayLevelUpEffect();
}

void OnXPGained(int amount)
{
    ShowXPPopup($"+{amount} XP");
}

void OnSkillPointGained(int amount)
{
    ShowNotification($"+{amount} Skill Point!");
}
```

---

## Integration Points

### With GameManager
```csharp
if (GameManager.Instance != null)
{
    GameManager.Instance.UpdatePlayerLevel(currentLevel);
}
```
Notifies the GameManager when level changes.

### With UI
- Use `GetXPProgress()` for XP bar sliders
- Use `currentLevel` for level text display
- Use `currentXP` for XP number display
- Use `skillPoints` for skill points display

---

## Example Scenarios

### Scenario 1: Kill Enemy Worth 50 XP
```
Player: Level 1, XP: 70
Enemy killed: +50 XP

1. AddExperience(50) called
2. currentXP = 70 + 50 = 120
3. CheckLevelUp()
4. 120 >= 100? Yes → LevelUp()
5. currentLevel = 2, skillPoints = 1
6. 120 >= 300? No → Stop
Final: Level 2, XP: 120, SP: 1
```

### Scenario 2: Complete Quest Worth 500 XP
```
Player: Level 1, XP: 0
Quest complete: +500 XP

1. AddExperience(500) called
2. currentXP = 0 + 500 = 500
3. CheckLevelUp()
4. Loop iteration 1: 500 >= 100? Yes → Level 2
5. Loop iteration 2: 500 >= 300? Yes → Level 3
6. Loop iteration 3: 500 >= 600? No → Stop
Final: Level 3, XP: 500, SP: 2
```

### Scenario 3: Max Level Check
```
Player: Level 10, XP: 4500
Enemy killed: +50 XP

1. AddExperience(50) called
2. currentLevel >= maxLevel? Yes → Exit early
3. No XP added, no level up possible
Final: Level 10, XP: 4500 (unchanged)
```

---

## Summary

The ProgressionManager uses a **cumulative XP system** with these key mechanics:

✅ **XP Never Resets** - Total XP always increases
✅ **Threshold-Based Leveling** - Each level requires reaching a total XP amount
✅ **Multiple Level-Ups Supported** - Can skip levels with large XP gains
✅ **Skill Point Rewards** - 1 point per level gained
✅ **Event-Driven** - UI and systems react to progression events
✅ **Max Level Cap** - Prevents leveling beyond level 10

This design makes it easy to balance progression and allows flexibility in XP rewards from different sources (kills, quests, exploration, etc.).
