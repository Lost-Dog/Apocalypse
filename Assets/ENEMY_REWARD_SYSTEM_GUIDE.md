# Enemy Reward System - Complete Guide

## ✅ You Already Have the Script!

The script you need is **`EnemyKillRewardHandler.cs`** - it's already in your project at `/Assets/Scripts/EnemyKillRewardHandler.cs`

---

## 🎯 How to Add Rewards to Enemies

### **Quick Setup (3 Steps):**

**1. Select Your Enemy**
- In Hierarchy, select the enemy prefab or GameObject (e.g., "Patrol AI")

**2. Add the Component**
- Click "Add Component" in Inspector
- Search for **"Enemy Kill Reward Handler"**
- Click to add it

**3. Configure Rewards**
- Set the reward values in Inspector
- Done! ✅

---

## ⚙️ Component Settings Explained

### **Reward Configuration:**

```
Base XP Reward: 50
├── The base experience points given to player
└── Example: 50 XP for standard enemy

XP Variance: 10
├── Random variation in XP reward
└── Actual XP will be: 50 ± 10 (40-60 XP)

Loot Drop Chance: 0.5
├── Probability of dropping loot (0.0 - 1.0)
├── 0.5 = 50% chance
├── 0.0 = Never drops loot
└── 1.0 = Always drops loot
```

### **Enemy Info:**

```
Enemy Level: 1
├── The level of this enemy
└── Affects loot quality

Is Elite: ☐
├── Mark as elite enemy
└── Gives bonus XP and better loot

Is Boss: ☐
├── Mark as boss enemy
└── Gives huge XP and guaranteed epic loot
```

### **Multipliers:**

```
Elite XP Multiplier: 2
├── XP multiplied by 2 for elite enemies
└── Example: 50 XP × 2 = 100 XP

Boss XP Multiplier: 5
├── XP multiplied by 5 for boss enemies
└── Example: 50 XP × 5 = 250 XP

Elite Loot Chance: 0.75
├── 75% chance to drop loot if elite
└── Drops rare quality loot

Boss Loot Chance: 1.0
├── 100% chance to drop loot if boss
└── Always drops epic quality loot
```

---

## 📊 Enemy Type Examples

### **1. Standard Enemy (Default):**
```
Component Settings:
├── Base XP Reward: 50
├── XP Variance: 10
├── Loot Drop Chance: 0.5 (50%)
├── Enemy Level: 1
├── Is Elite: ☐ No
└── Is Boss: ☐ No

Rewards:
├── XP: 40-60 (average 50)
├── Loot Chance: 50%
└── Loot Rarity: Common/Uncommon
```

### **2. Elite Enemy:**
```
Component Settings:
├── Base XP Reward: 50
├── XP Variance: 10
├── Loot Drop Chance: 0.5 (ignored, uses elite chance)
├── Enemy Level: 3
├── Is Elite: ☑ YES
├── Is Boss: ☐ No
├── Elite XP Multiplier: 2
└── Elite Loot Chance: 0.75

Rewards:
├── XP: 80-120 (average 100)
├── Loot Chance: 75%
└── Loot Rarity: RARE (blue)
```

### **3. Boss Enemy:**
```
Component Settings:
├── Base XP Reward: 100
├── XP Variance: 20
├── Loot Drop Chance: (ignored, boss always drops)
├── Enemy Level: 10
├── Is Elite: ☐ No
├── Is Boss: ☑ YES
├── Boss XP Multiplier: 5
└── Boss Loot Chance: 1.0

Rewards:
├── XP: 400-600 (average 500)
├── Loot Chance: 100% (GUARANTEED)
└── Loot Rarity: EPIC (purple)
```

---

## 🎮 Example Configurations

### **Weak Enemies (Zombies, Basic Soldiers):**
```yaml
Base XP Reward: 25
XP Variance: 5
Loot Drop Chance: 0.3
Enemy Level: 1
Is Elite: No
Is Boss: No

Result: 20-30 XP, 30% loot chance
```

### **Normal Enemies (Patrol Guards, Bandits):**
```yaml
Base XP Reward: 50
XP Variance: 10
Loot Drop Chance: 0.5
Enemy Level: 2
Is Elite: No
Is Boss: No

Result: 40-60 XP, 50% loot chance
```

### **Strong Enemies (Veterans, Heavies):**
```yaml
Base XP Reward: 75
XP Variance: 15
Loot Drop Chance: 0.6
Enemy Level: 4
Is Elite: No
Is Boss: No

Result: 60-90 XP, 60% loot chance
```

### **Elite Enemies (Squad Leaders, Champions):**
```yaml
Base XP Reward: 100
XP Variance: 20
Loot Drop Chance: 0.5
Enemy Level: 5
Is Elite: YES ☑
Elite XP Multiplier: 2
Elite Loot Chance: 0.75

Result: 160-240 XP, 75% loot chance, RARE loot
```

### **Boss Enemies (Commanders, Named Bosses):**
```yaml
Base XP Reward: 200
XP Variance: 50
Loot Drop Chance: (ignored)
Enemy Level: 10
Is Boss: YES ☑
Boss XP Multiplier: 5
Boss Loot Chance: 1.0

Result: 750-1250 XP, 100% loot chance, EPIC loot
```

---

## 🔧 How It Works

### **Automatic Detection:**

```
1. Component added to enemy
   ↓
2. Finds JUHealth component on Start()
   ↓
3. Subscribes to OnDeath event
   ↓
4. Enemy dies (health reaches 0)
   ↓
5. OnEnemyDeath() triggered
   ↓
6. RewardPlayer() called
   ↓
7. Finds Player with "Player" tag
   ↓
8. Gets PlayerSystemBridge component
   ↓
9. Gives XP reward
   ↓
10. Rolls for loot drop
   ↓
11. Spawns loot if successful
```

### **XP Calculation:**

```csharp
Base XP: 50
Variance: ±10
Random Roll: +7
Subtotal: 57 XP

If Standard Enemy:
  Final XP = 57

If Elite Enemy:
  Final XP = 57 × 2 = 114

If Boss Enemy:
  Final XP = 57 × 5 = 285
```

### **Loot Calculation:**

```csharp
Standard Enemy:
  Drop Chance = lootDropChance (0.5)
  Rarity = Random (Common/Uncommon)

Elite Enemy:
  Drop Chance = eliteLootChance (0.75)
  Rarity = RARE (forced)

Boss Enemy:
  Drop Chance = bossLootChance (1.0)
  Rarity = EPIC (forced)
```

---

## 📝 Step-by-Step Setup

### **For Enemy Prefabs:**

**1. Open Prefab:**
- Project window → Navigate to enemy prefab
- Double-click to open in Prefab mode

**2. Add Component:**
- Select root GameObject of prefab
- Inspector → "Add Component"
- Type "EnemyKillRewardHandler"
- Click to add

**3. Configure:**
```
Base XP Reward: 50 (adjust based on difficulty)
XP Variance: 10 (10-20% of base XP)
Loot Drop Chance: 0.5 (50%)
Enemy Level: 1 (match enemy strength)
```

**4. Save:**
- File → Save (or Ctrl+S)
- Exit Prefab mode

**5. Apply to All Instances:**
- All instances in scene automatically updated! ✅

### **For Scene Enemies:**

**1. Select Enemy:**
- Hierarchy → Find enemy GameObject
- Click to select

**2. Add Component:**
- Inspector → "Add Component"
- Type "EnemyKillRewardHandler"

**3. Configure:**
- Set values as needed

**4. Test:**
- Enter Play Mode
- Kill enemy
- Check Console for reward messages

---

## ✅ Requirements Checklist

The `EnemyKillRewardHandler` script requires these systems to work:

**Required on Enemy:**
- ✅ `JUHealth` component (for OnDeath event)
- ✅ `EnemyKillRewardHandler` component (this script)

**Required on Player:**
- ✅ Tag: "Player"
- ✅ `PlayerSystemBridge` component

**Required in Scene:**
- ✅ `GameManager` singleton instance
- ✅ `LootManager` assigned in GameManager

**If Missing:**
- Check Console for warning messages
- Rewards won't work until all requirements met

---

## 🧪 Testing Your Setup

### **Quick Test:**

**1. Setup:**
```
- Add EnemyKillRewardHandler to enemy
- Set Base XP Reward: 100
- Set Loot Drop Chance: 1.0 (always drop)
```

**2. Enter Play Mode:**
```
- Kill the enemy
- Check Console logs:
  ✓ "[Enemy Name] killed! Player gained 100 XP"
  ✓ "Loot dropped at [position]"
```

**3. Verify:**
```
- Player XP bar should increase
- Loot item should spawn on ground
- Enemy should give ragdoll/death animation
```

### **Debug Messages:**

**Success Messages:**
```
✅ "Zombie killed! Player gained 50 XP"
✅ "Loot dropped at (10, 1, 5)"
```

**Warning Messages:**
```
⚠️ "Player not found! Cannot reward XP/loot."
   → Make sure player has "Player" tag

⚠️ "PlayerSystemBridge not found on player!"
   → Add PlayerSystemBridge component to player

⚠️ "GameManager or LootManager not found!"
   → Make sure GameManager exists in scene
   → Check LootManager is assigned

⚠️ "JUHealth component not found!"
   → Add JUHealth component to enemy
```

---

## 🎯 Common Enemy Setups

### **Copy-Paste Values:**

**Trash Mob:**
```
Base XP: 25
Variance: 5
Loot Chance: 0.2
Enemy Level: 1
Elite: No
Boss: No
```

**Standard Enemy:**
```
Base XP: 50
Variance: 10
Loot Chance: 0.5
Enemy Level: 2
Elite: No
Boss: No
```

**Veteran Enemy:**
```
Base XP: 75
Variance: 15
Loot Chance: 0.6
Enemy Level: 3
Elite: No
Boss: No
```

**Elite (Blue):**
```
Base XP: 100
Variance: 20
Loot Chance: (ignored)
Enemy Level: 5
Elite: YES ☑
Elite XP Mult: 2
Elite Loot: 0.75
Boss: No
```

**Mini-Boss:**
```
Base XP: 150
Variance: 30
Loot Chance: (ignored)
Enemy Level: 7
Elite: YES ☑
Elite XP Mult: 3
Elite Loot: 1.0
Boss: No
```

**World Boss:**
```
Base XP: 250
Variance: 50
Loot Chance: (ignored)
Enemy Level: 10
Elite: No
Boss: YES ☑
Boss XP Mult: 5
Boss Loot: 1.0
```

---

## 🔄 Integration with Other Systems

### **Challenge System:**
```
Enemies can count toward challenge progress:
- Kill X enemies
- Kill elite enemies
- Kill bosses

No extra setup needed - challenges track automatically!
```

### **Control Zone System:**
```
Zone enemies already tracked:
- ControlZone counts enemy deaths
- Works with EnemyKillRewardHandler
- Both systems run independently
```

### **Infection System:**
```
You can expand EnemyKillRewardHandler to add infection:

private void RewardPlayer()
{
    // Existing XP/Loot code...
    
    // Add infection on kill
    var infection = player.GetComponent<PlayerInfectionDisplay>();
    if (infection != null)
    {
        infection.AddInfection(5f); // 5% infection per kill
    }
}
```

---

## 📊 Reward Balance Guidelines

### **XP Rewards (Level 1-10 Player):**

```
Enemy Type          Base XP    With Multiplier    Time to Kill
─────────────────────────────────────────────────────────────
Trash Mob           25         25                 2 seconds
Standard            50         50                 5 seconds
Veteran             75         75                 8 seconds
Elite               100        200 (×2)           15 seconds
Mini-Boss           150        300 (×2)           30 seconds
Boss                250        1250 (×5)          120 seconds

Player needs ~1000 XP per level (adjust in PlayerSystemBridge)
```

### **Loot Drop Chances:**

```
Enemy Type          Drop %     Rarity
──────────────────────────────────────
Trash Mob           20%        Common
Standard            50%        Common/Uncommon
Veteran             60%        Uncommon
Elite               75%        RARE (forced)
Mini-Boss           100%       RARE (forced)
Boss                100%       EPIC (forced)
```

---

## 🛠️ Customization Examples

### **Add Currency Rewards:**

Extend the script to give money/currency:

```csharp
private void RewardPlayer()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    // ... existing code ...
    
    GiveXPReward(playerBridge);
    TryDropLoot(playerBridge);
    
    // NEW: Add currency
    GiveCurrencyReward(playerBridge);
}

private void GiveCurrencyReward(PlayerSystemBridge playerBridge)
{
    int currency = baseCurrencyReward;
    
    if (isBoss) currency *= 10;
    else if (isElite) currency *= 3;
    
    // Add to player's currency
    // playerBridge.AddCurrency(currency);
    
    Debug.Log($"Player gained {currency} credits");
}
```

### **Add Stat Tracking:**

```csharp
private void OnEnemyDeath()
{
    if (hasRewardedPlayer) return;
    hasRewardedPlayer = true;
    
    RewardPlayer();
    
    // NEW: Track stats
    TrackKill();
}

private void TrackKill()
{
    // Update kill counters
    if (isBoss)
    {
        // PlayerStats.bossKills++;
    }
    else if (isElite)
    {
        // PlayerStats.eliteKills++;
    }
    else
    {
        // PlayerStats.normalKills++;
    }
}
```

---

## ✅ Quick Setup Summary

**To add rewards to an enemy:**

1. Select enemy (prefab or scene instance)
2. Add Component → `EnemyKillRewardHandler`
3. Set `Base XP Reward` (e.g., 50)
4. Set `Loot Drop Chance` (e.g., 0.5 for 50%)
5. Optional: Check `Is Elite` or `Is Boss`
6. Done! ✅

**Enemy will now:**
- Give XP when killed
- Drop loot based on drop chance
- Use multipliers if elite/boss
- Integrate with existing systems

---

## 📚 Related Documentation

- Player progression: Check `PlayerSystemBridge.cs`
- Loot system: Check `LootManager.cs`
- Challenge system: `/Assets/Challenge System - Setup Guide.md`
- Control zones: `/Assets/Documentation/CONTROL_ZONE_MANAGER_SETUP.txt`

---

**Status:** ✅ Script ready to use! Just add to enemies and configure! 🎮💰
