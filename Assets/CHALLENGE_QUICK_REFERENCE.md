# Challenge System - Quick Reference Card

## ⚡ Setup (Do Once)

1. GameSystems → Create Empty → `ChallengeSpawner`
2. Add `ChallengeSpawner` component
3. Done!

---

## 🎯 Create Challenge (2 min)

**Right-click** → Create → Division Game → Challenge Data

### Minimum Required Fields

```
Challenge Name: "Your Name"
Description: "What to do"
Challenge Type: [Pick one]
Difficulty: [Pick one]
Enemy Count: [How many]
Time Limit: [Seconds]
Enemy Prefabs: [Drag enemies]
Spawn Radius: [Meters]
XP Reward: [Amount]
```

### Add to Manager

Drag challenge → `ChallengeManager` → `World Event Challenges` list

---

## 🔧 Enemy Integration

Add ONE line to enemy death:

```csharp
GetComponent<ChallengeEnemy>()?.OnEnemyDeath();
```

---

## 📋 Challenge Types

| Type | Enemy Count | Civilians | Boss | Use Case |
|------|-------------|-----------|------|----------|
| **Control Point** | 8-15 | 0 | No | Clear area |
| **Civilian Rescue** | 5-10 | 3-5 | No | Save people |
| **Boss Encounter** | 10-20 | 0 | Yes | Elite fight |
| **Supply Drop** | 15-25 | 0 | No | Defend zone |
| **Hostage Rescue** | 10-20 | 2-5 | Yes | Stealth rescue |

---

## ⚙️ Difficulty Settings

| Difficulty | Enemies | Time | Spawn Radius | XP | Loot |
|------------|---------|------|--------------|-----|------|
| **Easy** | 5-10 | 300-600s | 15-25m | 200-500 | Common |
| **Medium** | 10-15 | 180-300s | 25-35m | 500-1500 | Uncommon |
| **Hard** | 15-25 | 120-180s | 30-40m | 1500-3000 | Rare |
| **Extreme** | 25-40 | 120-240s | 35-50m | 3000-5000 | Epic |

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| No spawning | Add ChallengeSpawner to scene |
| Enemies won't spawn | Check Enemy Prefabs list filled |
| Won't complete | Add OnEnemyDeath() call |
| No UI | Assign ChallengeNotificationUI in HUDManager |
| No marker | Assign World Marker Prefab in ChallengeManager |

---

## 📝 Copy-Paste Templates

### Easy Challenge
```
Name: "Checkpoint Capture"
Type: Control Point
Difficulty: Easy
Enemy Count: 8
Time: 300s
Radius: 25m
XP: 500
Loot: Common x2
```

### Medium Challenge
```
Name: "Rescue Squad"
Type: Civilian Rescue
Difficulty: Medium
Enemy Count: 10
Civilian Count: 3
Time: 240s
Radius: 30m
XP: 1000
Loot: Uncommon x2
```

### Hard Challenge
```
Name: "Boss Takedown"
Type: Boss Encounter
Difficulty: Hard
Enemy Count: 15
Boss: Yes
Time: 180s
Radius: 35m
XP: 2500
Loot: Rare x3
```

### Extreme Challenge
```
Name: "Operation Blackout"
Type: Hostage Rescue
Difficulty: Extreme
Enemy Count: 20
Civilian Count: 5
Boss: Yes
No Deaths: Yes
Time: 300s
Radius: 40m
XP: 5000
Loot: Epic x3
```

---

## 🎮 Testing Commands

```csharp
// Manually spawn challenge
ChallengeManager.Instance.SpawnRandomWorldEvent();

// Get active challenges
var challenges = ChallengeManager.Instance.activeChallenges;

// Complete challenge
ChallengeManager.Instance.CompleteChallenge(challenge);

// Fail challenge
ChallengeManager.Instance.FailChallenge(challenge);
```

---

## 📁 File Structure

```
/Assets
  /Scripts
    ChallengeManager.cs        (updated)
    ChallengeData.cs           (existing)
    ChallengeSpawner.cs        (NEW)
    ChallengeEnemy.cs          (NEW)
    ChallengeCivilian.cs       (NEW)
    ChallengeNotificationUI.cs (updated)
    ChallengeWorldMarker.cs    (existing)
    ChallengeCompassMarker.cs  (existing)
    
  /Resources/Challenges
    [Your ChallengeData assets]
```

---

## ✅ Quick Check

Before creating challenges, verify:

- [ ] ChallengeSpawner in scene
- [ ] ChallengeManager configured
- [ ] NavMesh baked
- [ ] Enemy prefabs ready
- [ ] Spawn zones placed

---

## 🚀 Fast Workflow

1. Create ChallengeData (2 min)
2. Fill required fields (2 min)
3. Add to manager (10 sec)
4. Test (1 min)
5. Adjust & polish (5 min)

**Total: ~10 min per challenge**

Create 10 challenges = 1.5 hours

---

## 📖 Full Documentation

- **CHALLENGE_SYSTEM_SETUP.md** - Detailed setup
- **CHALLENGE_CREATION_GUIDE.md** - Challenge templates
- **CHALLENGE_SYSTEM_COMPLETE.md** - Full overview
- **CHALLENGE_QUICK_REFERENCE.md** - This card

---

**Need help?** Check documentation or console for error messages!
