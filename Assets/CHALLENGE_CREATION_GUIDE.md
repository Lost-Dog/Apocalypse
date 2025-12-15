# Challenge Creation Guide

## System Overview

The new challenge system automatically:
- ✅ **Spawns markers** when challenge starts (not pre-placed)
- ✅ **Spawns enemies** around the challenge location
- ✅ **Spawns civilians** for rescue missions
- ✅ **Spawns objects** (extraction zones, supply drops, etc.)
- ✅ **Cleans up everything** when challenge completes/fails/expires

---

## Quick Setup (One-Time)

### 1. Add ChallengeSpawner to Scene

1. **Find** `/GameSystems` in Hierarchy
2. **Create empty child**: `ChallengeSpawner`
3. **Add Component** → `ChallengeSpawner` script

That's it! The system will now handle all spawning automatically.

---

## Creating Challenges Step-by-Step

### Challenge Type 1: Clear Enemy Outpost

**Objective:** Kill all enemies in an area

#### Step 1: Create Challenge Data

1. **Right-click** in Project → `Create` → `Division Game` → `Challenge Data`
2. **Name it**: `Challenge_EnemyOutpost_Easy`
3. **Configure:**

```
CHALLENGE INFO:
  Challenge Name: "Enemy Outpost"
  Description: "Clear out the rogue agent outpost and eliminate all hostiles."
  Challenge Type: Control Point
  Frequency: World Event
  Difficulty: Easy

REQUIREMENTS:
  Recommended Level: 5
  Required Player Level: 1
  Time Limit: 300 (5 minutes)
  Detection Radius: 50

OBJECTIVES:
  Enemy Count: 8
  Civilian Count: 0
  Require No Deaths: false
  Require Stealth: false
  Spawn Boss: false

SPAWNING:
  Enemy Prefabs: [Drag your enemy prefabs here]
    - RogueAgent_Rifle
    - RogueAgent_Shotgun
    - RogueAgent_SMG
  Spawn Radius: 25

REWARDS:
  XP Reward: 500
  Currency Reward: 150
  Guaranteed Loot Rarity: Common
  Guaranteed Loot Count: 2
```

#### Step 2: Add to ChallengeManager

1. **Select** `GameSystems/ChallengeManager`
2. **Find** "World Event Challenges" list
3. **Drag** your new challenge into the list

#### Step 3: Test

- Press Play
- Challenge will spawn automatically
- Enemies spawn around the marker
- Kill all enemies to complete

---

### Challenge Type 2: Civilian Rescue

**Objective:** Rescue civilians from hostile area

#### Create Challenge Data

```
CHALLENGE INFO:
  Challenge Name: "Civilian Rescue"
  Description: "Rescue civilians trapped in the quarantine zone."
  Challenge Type: Civilian Rescue
  Frequency: World Event
  Difficulty: Medium

OBJECTIVES:
  Enemy Count: 5
  Civilian Count: 3
  Require No Deaths: true (civilians can't die)
  Require Stealth: false

SPAWNING:
  Enemy Prefabs: [Guards protecting civilians]
  Civilian Prefabs: [Drag civilian prefabs]
    - Civilian_Male_01
    - Civilian_Female_01
  Spawn Radius: 20

REWARDS:
  XP Reward: 750
  Currency Reward: 200
  Guaranteed Loot Rarity: Uncommon
  Guaranteed Loot Count: 1
```

**How it works:**
- Civilians spawn with guards
- Player walks near civilian → auto-rescued
- If civilian dies → challenge fails (if Require No Deaths = true)

---

### Challenge Type 3: Boss Encounter

**Objective:** Defeat elite/boss enemy

#### Create Challenge Data

```
CHALLENGE INFO:
  Challenge Name: "Warlord Showdown"
  Description: "Eliminate the rogue warlord and his elite squad."
  Challenge Type: Boss Encounter
  Frequency: World Event
  Difficulty: Hard

OBJECTIVES:
  Enemy Count: 10
  Spawn Boss: true ✓
  Boss Name: "Warlord Kane"

SPAWNING:
  Enemy Prefabs: [Elite guards]
  Boss Prefab: [Drag boss prefab]
    - Elite_Warlord
  Spawn Radius: 30

REWARDS:
  XP Reward: 2000
  Currency Reward: 500
  Guaranteed Loot Rarity: Rare
  Guaranteed Loot Count: 3
```

**Features:**
- Boss spawns closer to center (50% of spawn radius)
- Boss counts as 1 enemy
- Killing boss + all enemies = complete

---

### Challenge Type 4: Supply Drop Defense

**Objective:** Defend extraction zone while enemies attack

#### Create Challenge Data

```
CHALLENGE INFO:
  Challenge Name: "Supply Drop Defense"
  Description: "Defend the supply drop location from incoming hostiles."
  Challenge Type: Supply Drop
  Frequency: World Event
  Difficulty: Medium

OBJECTIVES:
  Enemy Count: 15
  Time Limit: 180 (3 minutes - waves attack)

SPAWNING:
  Enemy Prefabs: [Multiple types for waves]
  Extraction Zone Prefab: [Supply drop marker/zone]
  Spawn Radius: 35
```

---

### Challenge Type 5: Hostage Rescue

**Objective:** Eliminate captors, rescue hostages

#### Create Challenge Data

```
CHALLENGE INFO:
  Challenge Name: "Hostage Situation"
  Description: "Neutralize the captors and rescue the hostages."
  Challenge Type: Hostage Rescue
  Difficulty: Extreme

OBJECTIVES:
  Enemy Count: 12
  Civilian Count: 2 (hostages)
  Require No Deaths: true
  Require Stealth: true (optional)
  Spawn Boss: true (captor leader)

SPAWNING:
  Enemy Prefabs: [Heavily armed captors]
  Civilian Prefabs: [Hostages]
  Boss Prefab: [Captor Leader]
  Spawn Radius: 25

REWARDS:
  XP Reward: 3000
  Currency Reward: 750
  Guaranteed Loot Rarity: Epic
  Guaranteed Loot Count: 2
```

---

## Challenge Difficulty Guidelines

### Easy (Level 1-5)
- Enemy Count: 5-10
- Time Limit: 300-600s (5-10 min)
- Spawn Radius: 15-25m
- XP Reward: 200-500
- Loot: Common

### Medium (Level 5-10)
- Enemy Count: 10-15
- Time Limit: 180-300s (3-5 min)
- Spawn Radius: 25-35m
- XP Reward: 500-1000
- Loot: Common-Uncommon

### Hard (Level 10-15)
- Enemy Count: 15-25
- Boss: Optional
- Time Limit: 120-180s (2-3 min)
- Spawn Radius: 30-40m
- XP Reward: 1000-2000
- Loot: Uncommon-Rare

### Extreme (Level 15+)
- Enemy Count: 25-40
- Boss: Yes
- Time Limit: 120-240s
- Spawn Radius: 35-50m
- XP Reward: 2000-5000
- Loot: Rare-Epic

---

## Integration with Existing Enemies

### Option 1: Use Existing AI

If your enemies already have AI (Emerald AI, custom scripts):

1. **Just drag prefab** into Enemy Prefabs list
2. **ChallengeEnemy** component auto-added
3. When enemy dies, call: `GetComponent<ChallengeEnemy>().OnEnemyDeath()`

### Option 2: Modify Enemy Death

Add this to your enemy death code:

```csharp
void OnDeath()
{
    ChallengeEnemy challengeEnemy = GetComponent<ChallengeEnemy>();
    if (challengeEnemy != null)
    {
        challengeEnemy.OnEnemyDeath();
    }
    
    // Your existing death code...
}
```

---

## Challenge Prefab Requirements

### Enemy Prefabs
- ✅ NavMeshAgent (for spawning on NavMesh)
- ✅ Health system that calls OnDeath()
- ✅ AI behavior
- ✅ Tag: "Enemy" (optional)

### Civilian Prefabs
- ✅ Collider with trigger enabled
- ✅ NavMeshAgent (optional for movement)
- ✅ Tag: "Civilian"
- Auto-rescued when player touches

### Boss Prefabs
- ✅ Same as enemy prefabs
- ✅ Higher health
- ✅ Unique appearance/abilities

### Extraction Zone Prefab
- ✅ Visual indicator (particle effects, model)
- ✅ Trigger collider (optional for defense zones)

---

## Spawn Zone Setup

The challenge spawn zones in your scene determine **where** challenges can appear:

1. **Find** `GameSystems/ChallengeSpawnZones` in scene
2. **Child objects** are spawn points
3. **Position them** around your map
4. **ChallengeManager** picks random zone when spawning

**Tips:**
- Place in open areas with NavMesh
- Spread across map for variety
- Avoid player spawn area
- 8-12 zones recommended

---

## Testing Your Challenge

1. **Create challenge** following templates above
2. **Add to ChallengeManager** World Event list
3. **Press Play**
4. **Wait or trigger**: Challenge spawns at interval
5. **Check:**
   - ✅ Marker appears at spawn zone
   - ✅ Enemies spawn around marker
   - ✅ Civilians spawn (if configured)
   - ✅ Killing enemy updates progress
   - ✅ Completing objective shows notification
   - ✅ Everything despawns when complete

---

## Common Issues

**Enemies don't spawn:**
- Check Enemy Prefabs list is filled
- Check Enemy Count > 0
- Check NavMesh exists at spawn location
- Enable "Log Spawn Events" on ChallengeSpawner for debugging

**Challenge won't complete:**
- Check if enemies are calling `OnEnemyDeath()`
- Check Enemy Count matches actual spawned enemies
- Look for "ChallengeEnemy" component on spawned enemies

**Civilians don't rescue:**
- Check civilian has trigger collider
- Check player has "Player" tag
- Check ChallengeCivilian component is attached

**Nothing spawns:**
- Check ChallengeSpawner exists in scene (GameSystems/ChallengeSpawner)
- Check console for "ChallengeSpawner.Instance is null" warning

---

## Quick Reference: Challenge Template

```
Challenge Name: [Descriptive name]
Description: [What player needs to do]
Type: [SupplyDrop/CivilianRescue/ControlPoint/etc.]
Difficulty: [Easy/Medium/Hard/Extreme]

Enemy Count: [Number to kill]
Civilian Count: [Number to rescue]
Spawn Boss: [Yes/No]
Time Limit: [Seconds]

Enemy Prefabs: [List]
Civilian Prefabs: [List if needed]
Boss Prefab: [If boss enabled]

XP Reward: [Based on difficulty]
Loot Rarity: [Common/Uncommon/Rare/Epic]
Loot Count: [1-3]
```

---

## Next Steps

Now you can create:
1. **5-10 World Event challenges** (variety of types)
2. **3 Daily challenges** (repeatable, medium difficulty)
3. **2 Weekly challenges** (high difficulty, big rewards)

Each challenge will automatically spawn enemies, markers, and clean up after completion!
