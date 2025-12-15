# Challenge Examples - Quick Reference

## How to Create Challenges

1. Right-click in `/Assets/Resources/Challenges/` folder
2. Create → Division Game → Challenge Data
3. Copy settings from examples below
4. Add to appropriate ChallengeManager pool

---

## World Event Challenges

### 🚚 Supply Drop - Easy

**File Name:** `WE_SupplyDrop_Easy.asset`

```
Challenge Name: Small Supply Cache
Description: A small enemy squad is guarding a supply cache. Eliminate them and claim the loot.

Type: Supply Drop
Frequency: World Event
Difficulty: Easy

Time Limit: 300 (5 min)
Detection Radius: 40
Recommended Level: 1
Required Player Level: 1

Enemy Count: 5
Spawn Boss: No

XP Reward: 150
Currency Reward: 75
Guaranteed Loot Rarity: Common
Guaranteed Loot Count: 1
```

### 🚚 Supply Drop - Medium

**File Name:** `WE_SupplyDrop_Medium.asset`

```
Challenge Name: Armed Convoy
Description: An enemy convoy is moving supplies through the area. Ambush and secure the cargo.

Type: Supply Drop
Frequency: World Event
Difficulty: Medium

Time Limit: 600 (10 min)
Detection Radius: 50
Recommended Level: 3
Required Player Level: 2

Enemy Count: 10
Spawn Boss: No

XP Reward: 300
Currency Reward: 150
Guaranteed Loot Rarity: Uncommon
Guaranteed Loot Count: 2
```

### 🚚 Supply Drop - Hard

**File Name:** `WE_SupplyDrop_Hard.asset`

```
Challenge Name: Heavily Guarded Shipment
Description: A high-value shipment is being transported by elite forces. High risk, high reward.

Type: Supply Drop
Frequency: World Event
Difficulty: Hard

Time Limit: 900 (15 min)
Detection Radius: 60
Recommended Level: 7
Required Player Level: 5

Enemy Count: 15
Spawn Boss: Yes
Boss Name: Supply Commander

XP Reward: 600
Currency Reward: 300
Guaranteed Loot Rarity: Rare
Guaranteed Loot Count: 3
```

---

## Civilian Rescue Challenges

### 👥 Civilian Rescue - Easy

**File Name:** `WE_CivilianRescue_Easy.asset`

```
Challenge Name: Citizens in Distress
Description: A few civilians are being threatened by rogues. Get there fast!

Type: Civilian Rescue
Frequency: World Event
Difficulty: Easy

Time Limit: 240 (4 min)
Detection Radius: 35
Recommended Level: 1

Civilian Count: 3
Enemy Count: 4

XP Reward: 120
Currency Reward: 60
Guaranteed Loot Rarity: Common
Guaranteed Loot Count: 1
```

### 👥 Civilian Rescue - Medium

**File Name:** `WE_CivilianRescue_Medium.asset`

```
Challenge Name: Civilians Under Siege
Description: Multiple civilians are trapped and under heavy fire. Save them all!

Type: Civilian Rescue
Frequency: World Event
Difficulty: Medium

Time Limit: 420 (7 min)
Detection Radius: 45
Recommended Level: 4

Civilian Count: 6
Enemy Count: 8

XP Reward: 250
Currency Reward: 125
Guaranteed Loot Rarity: Uncommon
Guaranteed Loot Count: 1
```

---

## Control Point Challenges

### 🎯 Control Point - Medium

**File Name:** `WE_ControlPoint_Medium.asset`

```
Challenge Name: Reclaim the Checkpoint
Description: Enemies have taken over a strategic checkpoint. Clear all hostiles and secure the area.

Type: Control Point
Frequency: World Event
Difficulty: Medium

Time Limit: 600 (10 min)
Detection Radius: 50
Recommended Level: 3

Enemy Count: 12
Spawn Boss: No

XP Reward: 350
Currency Reward: 175
Guaranteed Loot Rarity: Uncommon
Guaranteed Loot Count: 2
```

### 🎯 Control Point - Hard

**File Name:** `WE_ControlPoint_Hard.asset`

```
Challenge Name: Fortified Position Assault
Description: A heavily fortified control point must be taken. Expect fierce resistance.

Type: Control Point
Frequency: World Event
Difficulty: Hard

Time Limit: 900 (15 min)
Detection Radius: 65
Recommended Level: 6

Enemy Count: 20
Spawn Boss: Yes
Boss Name: Point Commander

XP Reward: 650
Currency Reward: 325
Guaranteed Loot Rarity: Rare
Guaranteed Loot Count: 2
```

---

## Boss Encounter Challenges

### 💀 Boss - Medium

**File Name:** `WE_BossEncounter_Medium.asset`

```
Challenge Name: Rogue Lieutenant
Description: A rogue officer has been spotted with a small squad. Neutralize the threat.

Type: Boss Encounter
Frequency: World Event
Difficulty: Medium

Time Limit: 600 (10 min)
Detection Radius: 55
Recommended Level: 4

Enemy Count: 8
Spawn Boss: Yes
Boss Name: Rogue Lieutenant

XP Reward: 400
Currency Reward: 200
Guaranteed Loot Rarity: Rare
Guaranteed Loot Count: 1
```

### 💀 Boss - Hard

**File Name:** `WE_BossEncounter_Hard.asset`

```
Challenge Name: Elite Commander
Description: An Elite Commander leads a heavily armed squad. Extreme danger.

Type: Boss Encounter
Frequency: World Event
Difficulty: Hard

Time Limit: 900 (15 min)
Detection Radius: 60
Recommended Level: 7

Enemy Count: 12
Spawn Boss: Yes
Boss Name: Elite Commander

XP Reward: 750
Currency Reward: 375
Guaranteed Loot Rarity: Epic
Guaranteed Loot Count: 1
```

### 💀 Boss - Extreme

**File Name:** `WE_BossEncounter_Extreme.asset`

```
Challenge Name: Named Enemy - Warlord
Description: A legendary warlord has appeared. Only the strongest agents survive.

Type: Boss Encounter
Frequency: World Event
Difficulty: Extreme

Time Limit: 1200 (20 min)
Detection Radius: 70
Recommended Level: 10

Enemy Count: 15
Spawn Boss: Yes
Boss Name: The Warlord

XP Reward: 1500
Currency Reward: 750
Guaranteed Loot Rarity: Legendary
Guaranteed Loot Count: 1
```

---

## Daily Challenges

### 📅 Daily - Kill Enemies

**File Name:** `Daily_KillEnemies_50.asset`

```
Challenge Name: Hunter
Description: Eliminate 50 enemy combatants throughout the city.

Type: Supply Drop (tracks kills)
Frequency: Daily
Difficulty: Easy

Enemy Count: 50

XP Reward: 400
Currency Reward: 200
Guaranteed Loot Rarity: Uncommon
```

### 📅 Daily - Headshots

**File Name:** `Daily_Headshots_25.asset`

```
Challenge Name: Marksman
Description: Score 25 headshot kills. Precision matters.

Type: Supply Drop
Frequency: Daily
Difficulty: Medium

Enemy Count: 25

XP Reward: 500
Currency Reward: 250
Guaranteed Loot Rarity: Rare
```

### 📅 Daily - Rescue Civilians

**File Name:** `Daily_RescueCivilians_10.asset`

```
Challenge Name: Guardian Angel
Description: Rescue 10 civilians from danger.

Type: Civilian Rescue
Frequency: Daily
Difficulty: Easy

Civilian Count: 10

XP Reward: 350
Currency Reward: 175
Guaranteed Loot Rarity: Uncommon
```

---

## Weekly Challenges

### 📆 Weekly - Elite Hunter

**File Name:** `Weekly_KillEnemies_200.asset`

```
Challenge Name: Elite Hunter
Description: Eliminate 200 enemies throughout the week.

Type: Supply Drop
Frequency: Weekly
Difficulty: Medium

Enemy Count: 200

XP Reward: 2000
Currency Reward: 1000
Guaranteed Loot Rarity: Epic
Guaranteed Loot Count: 2
```

### 📆 Weekly - Boss Slayer

**File Name:** `Weekly_DefeatBosses_5.asset`

```
Challenge Name: Boss Slayer
Description: Defeat 5 named bosses or elite enemies.

Type: Boss Encounter
Frequency: Weekly
Difficulty: Hard

Enemy Count: 5
Spawn Boss: Yes

XP Reward: 2500
Currency Reward: 1250
Guaranteed Loot Rarity: Legendary
Guaranteed Loot Count: 1
```

---

## Quick Setup Checklist

- [ ] Create `/Assets/Resources/Challenges/` folders
- [ ] Create 3-5 World Event challenges (mix difficulties)
- [ ] Create 5+ Daily challenges
- [ ] Create 3+ Weekly challenges
- [ ] Add challenges to ChallengeManager pools
- [ ] Set up spawn zones across map
- [ ] Configure spawn intervals
- [ ] Test challenge spawning
- [ ] Test reward distribution
- [ ] Connect to enemy/civilian systems
