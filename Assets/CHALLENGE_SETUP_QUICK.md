# Challenge System - Quick Setup (GameSystems Integration)

## Your ChallengeManager is Already Ready!

The `ChallengeManager` component on your `GameSystems` GameObject has been enhanced with all the new features. Here's what you need to do:

---

## ✅ Step 1: Select GameSystems

1. In Hierarchy, select `GameSystems`
2. In Inspector, find the `ChallengeManager` component
3. You'll see the new configuration options

---

## ✅ Step 2: Create Spawn Zones

Create empty GameObjects across your map for challenge spawning:

```
Scene Hierarchy:
/ChallengeSpawnZones (empty parent)
  /SpawnZone_Downtown_01
  /SpawnZone_Downtown_02
  /SpawnZone_Midtown_01
  /SpawnZone_Midtown_02
  /SpawnZone_DarkZone_01
  /SpawnZone_SafeHouse_Perimeter
```

1. Create empty GameObject at each desired spawn location
2. Position them strategically across your map
3. Drag them into the `Spawn Zones` list on ChallengeManager

**Tip:** Place spawn zones at:
- Major intersections
- Landmarks
- Control point areas
- Outside safe houses
- Supply route paths

---

## ✅ Step 3: Create Challenge Folder Structure

In your Project window:

```
/Assets
  /Resources
    /Challenges
      /WorldEvents
      /Daily
      /Weekly
```

1. Create `Resources` folder in `/Assets/` (if not exists)
2. Create `Challenges` folder inside Resources
3. Create subfolders: `WorldEvents`, `Daily`, `Weekly`

---

## ✅ Step 4: Create Your First Challenge

1. Right-click in `/Assets/Resources/Challenges/WorldEvents/`
2. Create → Division Game → Challenge Data
3. Name it: `WE_SupplyDrop_Easy`
4. Configure it:

```
Challenge Name: Small Supply Cache
Description: A small enemy squad is guarding a supply cache.

Type: Supply Drop
Frequency: World Event
Difficulty: Easy

Time Limit: 300
Detection Radius: 40
Recommended Level: 1

Enemy Count: 5
Spawn Boss: No

XP Reward: 150
Currency Reward: 75
Guaranteed Loot Rarity: Common
Guaranteed Loot Count: 1
```

---

## ✅ Step 5: Populate Challenge Pools

Back on the `GameSystems` → `ChallengeManager`:

### World Event Challenges
Drag your World Event challenge assets here (3-5 challenges)

### Daily Challenge Pool
Drag your Daily challenge assets here (5-10 challenges)

### Weekly Challenge Pool
Drag your Weekly challenge assets here (3-5 challenges)

---

## ✅ Step 6: Configure Spawn Settings

On the ChallengeManager component:

```
Auto Spawn Challenges: ✓ (checked)
World Event Spawn Interval: 300 (5 minutes)
Max Active World Events: 2
Max Daily Challenges: 3
Max Weekly Challenges: 2
```

---

## ✅ Step 7: Test It!

1. Press Play
2. Wait 5 minutes (or reduce spawn interval for testing)
3. Check Console for: "Challenge spawned: [name] at [position]"
4. Check `Active Challenges` list in Inspector

**Quick Test:** Set `World Event Spawn Interval` to `10` seconds for rapid testing.

---

## 🎮 Integration with Your Systems

### Connect to Enemy Deaths (Emerald AI)

Find where enemies die and add:

```csharp
void OnEnemyDeath()
{
    if (ChallengeManager.Instance != null)
    {
        var nearest = ChallengeManager.Instance.GetNearestChallenge(transform.position);
        
        if (nearest != null && nearest.IsPlayerInRange(playerPosition))
        {
            nearest.enemiesKilled++;
            ChallengeManager.Instance.UpdateChallengeProgress(nearest, 1);
        }
    }
}
```

### Connect to Mission System

In your `MissionManager`:

```csharp
public void OnMissionCompleted(Mission mission)
{
    // Existing code...
    
    // Update challenges
    if (ChallengeManager.Instance != null)
    {
        var nearbyChallenge = ChallengeManager.Instance.GetNearestChallenge(mission.location);
        if (nearbyChallenge != null)
        {
            ChallengeManager.Instance.UpdateChallengeProgress(nearbyChallenge, 1);
        }
    }
}
```

### Connect to Player Death

In your player health script:

```csharp
void OnPlayerDeath()
{
    if (ChallengeManager.Instance != null)
    {
        foreach (var challenge in ChallengeManager.Instance.activeChallenges)
        {
            if (challenge.challengeData.requireNoDeaths)
            {
                challenge.playerDied = true;
                ChallengeManager.Instance.FailChallenge(challenge);
            }
        }
    }
}
```

---

## 📊 HUD Integration

Your `HUDManager` on GameSystems can subscribe to events:

```csharp
void Start()
{
    if (ChallengeManager.Instance != null)
    {
        ChallengeManager.Instance.onChallengeSpawned.AddListener(OnChallengeSpawned);
        ChallengeManager.Instance.onChallengeCompleted.AddListener(OnChallengeCompleted);
        ChallengeManager.Instance.onChallengeProgress.AddListener(OnChallengeProgress);
    }
}

void OnChallengeSpawned(ActiveChallenge challenge)
{
    // Show notification: "New Challenge: [name]"
    ShowNotification($"New Challenge: {challenge.challengeData.challengeName}");
}

void OnChallengeCompleted(ActiveChallenge challenge)
{
    // Show reward notification
    ShowReward(challenge.challengeData.xpReward, challenge.challengeData.currencyReward);
}

void OnChallengeProgress(ActiveChallenge challenge)
{
    // Update progress bar
    UpdateChallengeUI(challenge);
}
```

---

## 🔄 Daily/Weekly Reset

Call these manually or set up a timer system:

```csharp
// Reset daily challenges at midnight
ChallengeManager.Instance.ResetDailyChallenges();

// Reset weekly challenges on Monday
ChallengeManager.Instance.ResetWeeklyChallenges();
```

---

## 🎯 Example Challenge Ideas

Use the examples in `/Assets/CHALLENGE_EXAMPLES.md` to create:

**5 World Events:**
- Small Supply Cache (Easy)
- Armed Convoy (Medium)
- Citizens in Distress (Easy)
- Reclaim Checkpoint (Medium)
- Rogue Lieutenant (Hard)

**5 Daily Challenges:**
- Kill 50 enemies
- Get 25 headshots
- Rescue 10 civilians
- Capture 3 control points
- Complete without dying

**3 Weekly Challenges:**
- Kill 200 enemies
- Rescue 50 civilians
- Defeat 5 bosses

---

## ✨ That's It!

Your existing `ChallengeManager` on `GameSystems` is now fully configured with:
- ✓ World events that spawn automatically
- ✓ Daily/Weekly challenge tracking
- ✓ Progress monitoring
- ✓ Reward distribution
- ✓ Event system for UI integration
- ✓ Integration with your ProgressionManager and LootManager

The system will work alongside your existing MissionManager, FactionManager, and other systems!
