# Challenge System - Setup Checklist

## ✅ One-Time Setup

### 1. Add ChallengeSpawner (2 min)

- [ ] Open your scene
- [ ] Find `GameSystems` in Hierarchy
- [ ] Right-click → Create Empty → Name: `ChallengeSpawner`
- [ ] Add Component → `ChallengeSpawner`
- [ ] Configure:
  - Max NavMesh Attempts: `20`
  - NavMesh Sample Distance: `5`
  - Obstruction Mask: `Default` (or walls/terrain)

### 2. Verify ChallengeManager (1 min)

- [ ] Select `GameSystems/ChallengeManager`
- [ ] Check `World Marker Prefab` is assigned
- [ ] Check `Compass Marker Prefab` is assigned  
- [ ] Check `Compass Marker Container` is assigned
- [ ] Check `Spawn Zones` list has spawn points

### 3. Prepare Enemy Prefabs (5 min)

Pick 3-5 enemy prefabs and test them:

- [ ] Enemy has NavMeshAgent
- [ ] Enemy has health/death system
- [ ] Add this to enemy death code:

```csharp
ChallengeEnemy challengeComp = GetComponent<ChallengeEnemy>();
if (challengeComp != null)
{
    challengeComp.OnEnemyDeath();
}
```

### 4. Prepare Civilian Prefabs (Optional, 3 min)

- [ ] Civilian has trigger collider
- [ ] Civilian layer: "Civilian"
- [ ] Player tag is "Player"
- [ ] Test: Player walks near civilian → should trigger

---

## 🎯 Creating Your First Challenge

### Challenge 1: Simple Enemy Clear (5 min)

- [ ] **Right-click** Project → Create → Division Game → Challenge Data
- [ ] **Name:** `Challenge_Outpost_Easy`
- [ ] **Fill in:**

```
Challenge Name: "Clear Outpost"
Description: "Eliminate all enemies at the checkpoint."
Challenge Type: Control Point
Difficulty: Easy

Enemy Count: 8
Time Limit: 300

Enemy Prefabs: [Add 2-3 enemy variants]
Spawn Radius: 25

XP Reward: 500
Currency Reward: 150
Guaranteed Loot Rarity: Common
Guaranteed Loot Count: 2
```

- [ ] **Drag** into ChallengeManager → World Event Challenges

### Test It!

- [ ] Press Play
- [ ] Wait for challenge to spawn (or trigger manually)
- [ ] Check console: "Challenge spawned: Clear Outpost"
- [ ] Go to marker location
- [ ] Verify 8 enemies spawned
- [ ] Kill all enemies
- [ ] Challenge completes → UI disappears, loot drops

**If it works:** ✅ Continue to next challenge!

**If not:** Check troubleshooting section below

---

## 📝 Create More Challenges

Now create these challenge types:

### Challenge 2: Civilian Rescue (5 min)

- [ ] Create new ChallengeData: `Challenge_Rescue_Medium`
- [ ] Settings:
  - Enemy Count: 5
  - Civilian Count: 3
  - Require No Deaths: true
  - Add civilian prefabs
  - Difficulty: Medium
  - XP: 750

### Challenge 3: Boss Fight (5 min)

- [ ] Create: `Challenge_Boss_Hard`
- [ ] Settings:
  - Enemy Count: 10
  - Spawn Boss: true
  - Boss Prefab: [Elite enemy]
  - Difficulty: Hard
  - XP: 2000

### Challenge 4: Supply Defense (5 min)

- [ ] Create: `Challenge_SupplyDrop_Medium`
- [ ] Settings:
  - Enemy Count: 15
  - Extraction Zone Prefab: [Supply marker]
  - Time Limit: 180
  - Spawn Radius: 35

### Challenge 5: Hostage Rescue (5 min)

- [ ] Create: `Challenge_Hostage_Extreme`
- [ ] Settings:
  - Enemy Count: 12
  - Civilian Count: 2
  - Spawn Boss: true
  - Require No Deaths: true
  - Difficulty: Extreme
  - XP: 3000

---

## 🔄 Configure Challenge Pools

### World Events (Always Active)

- [ ] Add 5-8 challenges to "World Event Challenges" list
- [ ] Mix difficulties (2 easy, 3 medium, 2 hard, 1 extreme)
- [ ] Variety of types

Settings:
- Auto Spawn: ✓
- Spawn Interval: 300s (5 min)
- Max Active: 2

### Daily Challenges (Reset Daily)

- [ ] Add 5-10 challenges to "Daily Challenge Pool"
- [ ] Medium difficulty
- [ ] Max Daily: 3

### Weekly Challenges (Reset Weekly)

- [ ] Add 3-5 challenges to "Weekly Challenge Pool"  
- [ ] Hard/Extreme difficulty
- [ ] Big rewards
- [ ] Max Weekly: 2

---

## 🎨 Polish (Optional)

### Add Audio

For each challenge:
- [ ] Start Sound (alert/notification)
- [ ] Complete Sound (success fanfare)
- [ ] Fail Sound (defeat tone)

### Add Icons

- [ ] Challenge Icon (shows in UI)
- [ ] Different icons per type

### Customize Colors

- [ ] Difficulty Color (override default)
- [ ] Easy: Green
- [ ] Medium: Yellow
- [ ] Hard: Orange
- [ ] Extreme: Red

---

## 🐛 Troubleshooting

### No Challenges Spawn

**Check:**
- [ ] ChallengeSpawner exists in scene?
- [ ] ChallengeManager has challenges in list?
- [ ] Auto Spawn enabled?
- [ ] Spawn Zones list not empty?

**Fix:**
1. Select ChallengeManager
2. Check "World Event Challenges" has entries
3. Check "Spawn Zones" has Transform references
4. Check "Auto Spawn Challenges" is checked

### Enemies Don't Spawn

**Check:**
- [ ] Challenge has Enemy Prefabs assigned?
- [ ] Enemy Count > 0?
- [ ] NavMesh baked in scene?
- [ ] Spawn location has walkable NavMesh?

**Fix:**
1. Select challenge asset
2. Check "Enemy Prefabs" list is filled
3. Check "Enemy Count" is set
4. Re-bake NavMesh (Window → AI → Navigation)

### Challenge Won't Complete

**Check:**
- [ ] Enemies call OnEnemyDeath()?
- [ ] ChallengeEnemy component on enemies?
- [ ] Killing enemies updates progress?

**Fix:**
Add to enemy death:
```csharp
ChallengeEnemy comp = GetComponent<ChallengeEnemy>();
if (comp != null) comp.OnEnemyDeath();
```

### UI Doesn't Show

**Check:**
- [ ] HUDManager has ChallengeNotificationUI assigned?
- [ ] Challenge panel active in scene?
- [ ] Text fields assigned?

**Fix:**
1. GameSystems/HUDManager
2. Assign "Challenge Notification UI" field
3. Check panel has all text references

### Markers Don't Appear

**Check:**
- [ ] World Marker Prefab assigned on ChallengeManager?
- [ ] Prefab has ChallengeWorldMarker script?

**Fix:**
1. ChallengeManager → World Marker Prefab
2. Drag marker prefab
3. Check prefab has script component

---

## ✅ Verification Checklist

Before going live, test:

- [ ] Challenge spawns at random zone
- [ ] World marker appears
- [ ] Compass marker shows direction
- [ ] UI notification displays
- [ ] Enemies spawn around marker
- [ ] Killing enemy updates progress (X/Y)
- [ ] Time counts down
- [ ] Completing challenge:
  - ✓ Shows completion notification
  - ✓ Grants XP
  - ✓ Drops loot
  - ✓ Despawns all enemies
  - ✓ Removes markers
  - ✓ Clears UI
- [ ] Failing challenge:
  - ✓ Time expires → fail
  - ✓ Civilian dies (if required) → fail
  - ✓ Everything despawns
- [ ] Multiple challenges work simultaneously
- [ ] Next challenge spawns after interval

---

## 🚀 You're Ready!

Your challenge system is now:

✅ **Spawning** enemies and markers dynamically
✅ **Tracking** progress in real-time
✅ **Rewarding** players on completion
✅ **Cleaning up** after completion/failure
✅ **Displaying** in HUD with notifications

Now create 10-15 unique challenges for your game!

---

## 📚 Next Steps

1. Create 8-10 world event challenges
2. Create 5 daily challenges
3. Create 3 weekly challenges
4. Tune difficulty and rewards
5. Add custom audio/icons
6. Playtest and adjust spawn radius/enemy count
7. Add more enemy variety

**Reference:** See `CHALLENGE_CREATION_GUIDE.md` for detailed challenge templates
