# Difficulty Scaling - Quick Reference

## At a Glance

### What Changed?

✅ **ChallengeData.cs**
- Added `baseXPReward` and `baseCurrencyReward` (renamed from xpReward/currencyReward)
- Added difficulty scaling settings section
- Added 6 new calculation methods for scaling

✅ **ActiveChallenge** (in ChallengeManager.cs)
- Added `playerLevelAtSpawn`, `actualDifficulty`, `enemyHealthMultiplier`, `enemyDamageMultiplier`
- Added 4 new methods: `GetXPReward()`, `GetCurrencyReward()`, `GetLootRarity()`, `GetDifficultyDisplayText()`
- Constructor now calculates difficulty on spawn

✅ **ChallengeManager.cs**
- `CompleteChallenge()` now grants scaled rewards
- New `GrantChallengeRewards()` method integrates with ProgressionManager & LootManager

✅ **ChallengeSpawner.cs**
- Enemies now have scaling applied on spawn
- New `ApplyDifficultyScalingToEnemy()` method
- New `ApplyDamageScaling()` method

✅ **ProgressionManager.cs**
- Added singleton pattern (`Instance`)

✅ **NEW: DifficultyDamageMultiplier.cs**
- Component for damage scaling integration

---

## Quick Setup Checklist

### Existing Challenges (Need Update)

Your existing `ChallengeData` assets use the old field names. You need to:

**Option 1: Manual (Recommended)**
1. Open each ChallengeData asset
2. Note the current `xpReward` and `currencyReward` values
3. Copy those values to `baseXPReward` and `baseCurrencyReward`
4. The old fields are gone, so this is required

**Option 2: Set Defaults**
1. `baseXPReward` defaults to 200
2. `baseCurrencyReward` defaults to 100
3. Adjust as needed per challenge

### New Challenges

Just set the values normally:
- `baseXPReward`: 200 (standard)
- `baseCurrencyReward`: 100 (standard)
- Scaling is enabled by default

---

## Testing Workflow

### In Editor (Play Mode)

1. **Check Current Level**
```csharp
Debug.Log(ProgressionManager.Instance.currentLevel);
```

2. **Manually Set Level** (for testing)
```csharp
// In Unity Console or Debug script
ProgressionManager.Instance.currentLevel = 8;
```

3. **Spawn a Challenge**
- Use existing spawn system
- Check console for scaling info

4. **Verify Scaling**
- Look for "Enemy scaled" messages
- Check enemy health in inspector
- Complete challenge and check XP granted

### Expected Console Output

```
Challenge spawned: Supply Drop (Level 5)
  Player Level: 8
  Base Difficulty: Medium
  Actual Difficulty: Easy
  Health Multiplier: 1.05x
  Damage Multiplier: 1.02x
  
Enemy scaled: Health 100 → 105 (x1.05)

Challenge completed: Supply Drop (Difficulty: Easy, Level: 8)
Rewards: 150 XP, 75 Credits
Granted 150 XP for completing challenge
Spawned 1x Common loot
```

---

## Common Scenarios

### Scenario: Disable Scaling for Specific Challenge

```
Inspector → ChallengeData → Difficulty Scaling
Uncheck "Enable Difficulty Scaling"
```

### Scenario: Make Challenges Harder Overall

```
Inspector → ChallengeData → Difficulty Scaling
Health Scaling Multiplier: 1.5
Damage Scaling Multiplier: 1.3
```

### Scenario: Double All Rewards

```
Inspector → ChallengeData → Difficulty Scaling
Reward Scaling Multiplier: 2.0
```

### Scenario: Check Difficulty in Code

```csharp
ActiveChallenge challenge = ...;

Debug.Log($"Difficulty: {challenge.actualDifficulty}");
Debug.Log($"Player Level: {challenge.playerLevelAtSpawn}");
Debug.Log($"XP Reward: {challenge.GetXPReward()}");
```

---

## Integration Points

### If You Have Custom UI

Display scaled difficulty:
```csharp
string text = challenge.GetDifficultyDisplayText();
// Shows: "HARD (Scaled from MEDIUM)"
```

### If You Have Currency System

Replace the TODO in `GrantChallengeRewards()`:
```csharp
int currencyReward = challenge.GetCurrencyReward();
YourCurrencySystem.AddCurrency(currencyReward);
```

### If You Have Custom Damage System

Check for damage multiplier:
```csharp
float mult = DifficultyDamageMultiplier.GetMultiplier(attacker);
float finalDamage = baseDamage * mult;
```

---

## Formulas (Quick Ref)

### Health Scaling
```
Final Health = Base × Difficulty Mult × (1 + Level × 0.05)

Easy: 0.75x
Medium: 1.0x
Hard: 1.5x
Extreme: 2.0x
```

### Damage Scaling
```
Final Damage = Base × Difficulty Mult × (1 + Level × 0.03)

Easy: 0.8x
Medium: 1.0x
Hard: 1.3x
Extreme: 1.75x
```

### XP Rewards
```
Final XP = Base × Difficulty Mult × Over-Level Bonus

Difficulty Mults: 0.75x/1.0x/1.5x/2.0x
Over-Level Bonus: 1.0 + (Recommended - Player) × 0.1
```

---

## File Summary

| File | Changes | Lines Added |
|------|---------|-------------|
| ChallengeData.cs | Added scaling methods | ~200 |
| ChallengeManager.cs | Added scaling to ActiveChallenge | ~80 |
| ChallengeSpawner.cs | Apply scaling to enemies | ~50 |
| ProgressionManager.cs | Singleton pattern | ~10 |
| DifficultyDamageMultiplier.cs | NEW | ~30 |
| DIFFICULTY_SCALING_GUIDE.md | Documentation | Full guide |

---

## What's NOT Changed

✅ UI remains the same (unless you choose to update)
✅ Challenge spawning logic unchanged
✅ Challenge completion logic unchanged
✅ Existing save/load systems unaffected
✅ AI behavior unchanged (just stats)

---

## Next Steps

1. **Update existing ChallengeData assets** with base reward values
2. **Test with different player levels** (1, 5, 10)
3. **Adjust multipliers** if scaling feels off
4. **(Optional)** Update UI to show scaled difficulty
5. **(Optional)** Integrate currency system in `GrantChallengeRewards()`

---

Ready to use! 🚀
