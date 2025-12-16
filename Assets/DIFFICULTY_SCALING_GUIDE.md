# Challenge Difficulty Scaling System

## Overview

The challenge system now features **dynamic difficulty scaling** that adapts based on player level, ensuring balanced and rewarding gameplay throughout progression.

---

## How It Works

### 🎯 Automatic Difficulty Adjustment

When a challenge spawns, the system:

1. **Reads player level** from `ProgressionManager` or `GameManager`
2. **Compares** player level vs challenge's `recommendedLevel`
3. **Calculates scaled difficulty** (Easy/Medium/Hard/Extreme)
4. **Adjusts** enemy stats and rewards accordingly

### 📊 Difficulty Scaling Formula

```
Level Difference = Player Level - Recommended Level

If difference <= -3:  EXTREME (much harder)
If difference = -2/-1: HARD (harder)
If difference = 0/1:   Use base difficulty (balanced)
If difference = 2/3:   One tier easier
If difference >= 4:    Two tiers easier
```

**Example:**
- Challenge: Recommended Level 5 (Medium)
- Player Level 3 → Scaled to **HARD**
- Player Level 5 → Stays **MEDIUM**
- Player Level 8 → Scaled to **EASY**

---

## Enemy Scaling

### Health Multipliers

| Difficulty | Base Multiplier | Level Scaling |
|-----------|----------------|---------------|
| Easy      | 0.75x          | +5% per level |
| Medium    | 1.0x           | +5% per level |
| Hard      | 1.5x           | +5% per level |
| Extreme   | 2.0x           | +5% per level |

**Example Calculation (Level 5, Hard):**
```
Base Health: 100
Difficulty Multiplier: 1.5x (Hard)
Level Multiplier: 1.25x (5 × 0.05 + 1.0)
Final Health: 100 × 1.5 × 1.25 = 187.5
```

### Damage Multipliers

| Difficulty | Base Multiplier | Level Scaling |
|-----------|----------------|---------------|
| Easy      | 0.8x           | +3% per level |
| Medium    | 1.0x           | +3% per level |
| Hard      | 1.3x           | +3% per level |
| Extreme   | 1.75x          | +3% per level |

**Note:** Damage scaling is applied to enemy weapons automatically.

---

## Reward Scaling

### XP Rewards

Rewards scale based on **actual difficulty completed**:

| Difficulty | Multiplier |
|-----------|-----------|
| Easy      | 0.75x     |
| Medium    | 1.0x      |
| Hard      | 1.5x      |
| Extreme   | 2.0x      |

**Bonus:** +10% per level above player
- Level 3 player completes Level 6 challenge → +30% bonus

**Example:**
```
Base XP: 200
Difficulty: Hard (1.5x)
Level Difference: +2 (20% bonus)
Final XP: 200 × 1.5 × 1.2 = 360 XP
```

### Currency Rewards

Same multipliers as XP (0.75x to 2.0x based on difficulty)

### Loot Rarity

Harder difficulties upgrade loot quality:

| Difficulty | Rarity Adjustment |
|-----------|------------------|
| Easy      | -1 tier (downgrade) |
| Medium    | No change |
| Hard      | +1 tier (upgrade) |
| Extreme   | +2 tiers (upgrade) |

**Example:**
- Base Loot: Rare
- Completed at EXTREME → Legendary

---

## Configuration

### Per-Challenge Settings (in ChallengeData)

**Inspector Fields:**
```
Difficulty Scaling
├─ Enable Difficulty Scaling ✓
├─ Scale Enemy Health ✓
├─ Scale Enemy Damage ✓
├─ Scale Rewards ✓
├─ Health Scaling Multiplier: 1.0
├─ Damage Scaling Multiplier: 1.0
└─ Reward Scaling Multiplier: 1.0
```

**Fine-Tuning:**
- **Health/Damage/Reward Multipliers** allow per-challenge adjustment
- Set to `2.0` for double scaling, `0.5` for half scaling
- Default `1.0` = standard scaling

### Disable Scaling

To disable scaling for a specific challenge:
1. Select the `ChallengeData` asset
2. Uncheck `Enable Difficulty Scaling`
3. Challenge will always use its base difficulty

---

## In-Game Display

### ActiveChallenge Methods

```csharp
// Get scaled values
int xp = challenge.GetXPReward();
int currency = challenge.GetCurrencyReward();
LootManager.Rarity lootRarity = challenge.GetLootRarity();

// Get difficulty display
string difficultyText = challenge.GetDifficultyDisplayText();
// Returns: "HARD (Scaled from MEDIUM)" if scaled

// Access raw values
int playerLevel = challenge.playerLevelAtSpawn;
ChallengeDifficulty actualDifficulty = challenge.actualDifficulty;
float healthMult = challenge.enemyHealthMultiplier;
float damageMult = challenge.enemyDamageMultiplier;
```

---

## Integration with Existing Systems

### ✅ Automatically Integrated

- **ProgressionManager** - XP rewards granted on completion
- **LootManager** - Scaled loot spawned at challenge position
- **ChallengeSpawner** - Enemy health/damage applied at spawn
- **ChallengeManager** - Rewards calculated on completion

### 🔧 Manual Integration (Optional)

**For custom damage systems:**

Check for `DifficultyDamageMultiplier` component:

```csharp
// In your damage calculation code
float baseDamage = 10f;
float scaledDamage = DifficultyDamageMultiplier.GetMultiplier(attacker) * baseDamage;
```

---

## Testing Examples

### Scenario 1: Balanced Challenge
```
Player Level: 5
Challenge: Level 5 (Medium)
Result:
  - Difficulty: MEDIUM (no change)
  - Enemy Health: 100 × 1.0 × 1.25 = 125
  - Enemy Damage: 10 × 1.0 × 1.15 = 11.5
  - XP Reward: 200 × 1.0 = 200
```

### Scenario 2: Overleveled
```
Player Level: 8
Challenge: Level 5 (Medium)
Result:
  - Difficulty: EASY (scaled down)
  - Enemy Health: 100 × 0.75 × 1.40 = 105
  - Enemy Damage: 10 × 0.8 × 1.24 = 9.9
  - XP Reward: 200 × 0.75 = 150
```

### Scenario 3: Underleveled
```
Player Level: 3
Challenge: Level 5 (Medium)
Result:
  - Difficulty: HARD (scaled up)
  - Enemy Health: 100 × 1.5 × 1.15 = 172.5
  - Enemy Damage: 10 × 1.3 × 1.09 = 14.2
  - XP Reward: 200 × 1.5 × 1.2 = 360 (bonus!)
```

### Scenario 4: Extreme Challenge
```
Player Level: 2
Challenge: Level 5 (Hard)
Result:
  - Difficulty: EXTREME (scaled up)
  - Enemy Health: 100 × 2.0 × 1.10 = 220
  - Enemy Damage: 10 × 1.75 × 1.06 = 18.6
  - XP Reward: 200 × 2.0 × 1.3 = 520 (huge bonus!)
  - Loot: Rare → Legendary (+2 tiers)
```

---

## Debug Logging

The system logs detailed info on:

**Challenge Spawn:**
```
Challenge spawned at Level 5 with scaling:
  Base Difficulty: Medium
  Actual Difficulty: Hard
  Health Multiplier: 1.725x
  Damage Multiplier: 1.417x
```

**Enemy Spawn:**
```
Enemy scaled: Health 100 → 172.5 (x1.73)
```

**Challenge Complete:**
```
Challenge completed: Supply Drop (Difficulty: Hard, Level: 5)
Rewards: 360 XP, 150 Credits
Granted 360 XP for completing challenge
Spawned 3x Rare loot
```

---

## Best Practices

### For Challenge Designers

1. **Set Recommended Level** appropriately
   - Level 1-3: Early game
   - Level 4-6: Mid game
   - Level 7-10: End game

2. **Adjust base difficulty** for challenge type
   - Supply Drop: Easy-Medium
   - Boss Fight: Hard-Extreme
   - Rescue: Medium

3. **Balance base rewards** assuming Medium difficulty
   - System will auto-scale up/down

4. **Test at different levels**
   - Create a test challenge
   - Use ProgressionManager to set level
   - Verify scaling feels fair

### For Balance

**If challenges feel too easy:**
- Increase `healthScalingMultiplier` (1.5x)
- Increase `damageScalingMultiplier` (1.3x)
- Add more enemies to spawn list

**If challenges feel too hard:**
- Decrease multipliers (0.8x)
- Reduce enemy count
- Increase base XP rewards

---

## Future Enhancements

### Potential Additions

1. **Player Skill Modifier**
   - Track completion success rate
   - Auto-adjust for struggling/pro players

2. **Time-Based Scaling**
   - Faster completions = better rewards
   - Gold/Silver/Bronze tiers

3. **Combo Scaling**
   - Complete multiple challenges → increased difficulty + rewards

4. **Prestige Difficulties**
   - Unlock "Nightmare" tier at max level
   - 3x-5x scaling for extreme rewards

---

## Troubleshooting

### Enemies Not Scaling

**Check:**
1. `ChallengeData` has `scaleEnemyHealth` enabled
2. Enemy has `JUHealth` component
3. Console shows "Enemy scaled" message

**Fix:**
- Ensure enemies are spawned via `ChallengeSpawner`
- Check that `ApplyDifficultyScalingToEnemy()` is called

### Rewards Not Scaling

**Check:**
1. `ChallengeData` has `scaleRewards` enabled
2. `ProgressionManager.Instance` exists in scene
3. Console shows "Granted X XP" message

**Fix:**
- Add ProgressionManager to GameManager
- Verify singleton is set up in Awake()

### Difficulty Always Medium

**Check:**
1. `ChallengeData.enableDifficultyScaling` is true
2. ProgressionManager level is updating
3. recommendedLevel is set on challenge

**Fix:**
- Enable scaling in inspector
- Manually test: `ProgressionManager.Instance.currentLevel = 8;`

---

## Summary

✅ **Implemented Features:**
- Dynamic difficulty calculation based on player level
- Enemy health scaling (0.75x - 2.0x + level bonus)
- Enemy damage scaling (0.8x - 1.75x + level bonus)
- XP reward scaling with level difference bonus
- Currency reward scaling
- Loot rarity upgrades for harder difficulties
- Automatic integration with existing systems
- Per-challenge configuration
- Debug logging for testing

🎮 **Gameplay Impact:**
- Challenges remain relevant at all levels
- Risk/reward balance encourages tackling harder content
- Natural progression curve
- Replay value for high-level players
- Satisfying reward feedback
