# Challenge Modifier & Reward System Guide

## Overview

The Challenge Modifier and Reward System allows you to create dynamic, replayable challenges with customizable difficulty modifiers and comprehensive reward structures including performance bonuses.

---

## 🎯 Challenge Modifiers

Modifiers change how challenges behave and provide additional rewards for increased difficulty.

### Enemy Modifiers

#### Increased Enemy Health
- **Type:** `IncreasedEnemyHealth`
- **Effect:** Multiplies enemy health by specified value
- **Example:** Value = 1.5 → Enemies have 150% health
- **Reward Bonus:** Automatic (scaled difficulty)

#### Increased Enemy Damage
- **Type:** `IncreasedEnemyDamage`
- **Effect:** Multiplies enemy damage output
- **Example:** Value = 1.3 → Enemies deal 130% damage
- **Reward Bonus:** Automatic

#### Increased Enemy Speed
- **Type:** `IncreasedEnemySpeed`
- **Effect:** Multiplies enemy movement speed
- **Example:** Value = 1.2 → Enemies move 20% faster
- **Reward Bonus:** Automatic

#### Increased Enemy Accuracy
- **Type:** `IncreasedEnemyAccuracy`
- **Effect:** Improves enemy aiming
- **Value:** Not yet implemented (placeholder for future)

#### Elite Enemies Only
- **Type:** `EliteEnemiesOnly`
- **Effect:** All spawned enemies are marked as elite variants
- **Reward Bonus:** Elite enemies give better loot/XP through EnemyKillRewardHandler

---

### Environmental Modifiers

#### Time Trial
- **Type:** `TimeTrial`
- **Effect:** Reduces time limit by percentage
- **Example:** Value = 0.75 → Only 75% of normal time
- **Reward Bonus:** +25% XP

#### Night Mode
- **Type:** `NightMode`
- **Effect:** Forces night time or reduced visibility
- **Value:** Not yet implemented (placeholder)

#### Limited Ammo
- **Type:** `LimitedAmmo`
- **Effect:** Player starts with limited ammunition
- **Reward Bonus:** +20% XP
- **Note:** Requires integration with player inventory system

#### No Health Regen
- **Type:** `NoHealthRegen`
- **Effect:** Disables passive health regeneration
- **Reward Bonus:** +30% XP
- **Note:** Requires integration with player health system

---

### Reward Modifiers

#### Double XP
- **Type:** `DoubleXP`
- **Effect:** Doubles all XP rewards
- **Usage:** Special events, challenges

#### Double Currency
- **Type:** `DoubleCurrency`
- **Effect:** Doubles all currency rewards
- **Usage:** Special events, challenges

#### Bonus Loot Drop
- **Type:** `BonusLootDrop`
- **Effect:** Increases number of loot drops
- **Example:** Value = 0.5 → +50% more loot items

#### Guaranteed Rare Loot
- **Type:** `GuaranteedRareLoot`
- **Effect:** Ensures minimum Rare quality loot

---

### Special Challenge Modes

#### Iron Man
- **Type:** `IronMan`
- **Effect:** Single death causes challenge failure
- **Reward Bonus:** +50% XP
- **Note:** Requires challenge failure detection

#### Survival Mode
- **Type:** `SurvivalMode`
- **Effect:** Endless waves until player fails
- **Note:** Placeholder for future implementation

#### Pacifist
- **Type:** `Pacifist`
- **Effect:** Complete without killing enemies
- **Note:** Placeholder for stealth/rescue challenges

#### Speed Runner
- **Type:** `SpeedRunner`
- **Effect:** Bonus for completing quickly
- **Handled by:** Speed completion bonus system

#### Perfect Score
- **Type:** `PerfectScore`
- **Effect:** Bonus for taking no damage
- **Handled by:** Perfect completion bonus system

---

## 💰 Reward System

### Base Rewards

Every challenge has base rewards defined in ChallengeData:
- `baseXPReward` - Base experience points
- `baseCurrencyReward` - Base currency/credits
- `guaranteedLootRarity` - Minimum loot quality
- `guaranteedLootCount` - Number of loot drops
- `bonusRewards` - Special items (optional)

### Difficulty Scaling Rewards

Rewards automatically scale based on:
1. **Difficulty Tier** (Easy/Medium/Hard/Extreme)
   - Easy: 0.75x rewards
   - Medium: 1.0x rewards
   - Hard: 1.5x rewards
   - Extreme: 2.0x rewards

2. **Level Difference**
   - Completing challenges above your level: +10% per level difference
   - Example: Level 5 player completes Level 8 challenge = +30% XP

### Performance Bonuses

#### Perfect Completion Bonus
- **Requirement:** Complete without dying or taking damage
- **Reward:** 1.5x XP multiplier (configurable)
- **Settings:**
  - `perfectCompletionBonus` = true/false
  - `perfectCompletionXPMultiplier` = 1.5

#### Speed Completion Bonus
- **Requirement:** Complete in less than threshold % of time limit
- **Reward:** 1.25x XP multiplier (configurable)
- **Settings:**
  - `speedCompletionBonus` = true/false
  - `speedThresholdPercentage` = 0.5 (50% of time)
  - `speedCompletionXPMultiplier` = 1.25

#### Stealth Completion Bonus
- **Requirement:** Complete without being detected (stealth challenges only)
- **Reward:** 1.3x XP multiplier (configurable)
- **Settings:**
  - `stealthCompletionBonus` = true/false
  - `stealthCompletionXPMultiplier` = 1.3
  - `requireStealth` = true (in challenge objectives)

### Modifier Stacking

All bonuses stack multiplicatively:
```
Total XP = Base XP × Difficulty × Level Bonus × Modifiers × Performance Bonuses

Example:
Base: 500 XP
Difficulty: Hard (1.5x)
Level Bonus: +2 levels (1.2x)
Modifiers: IronMan (+50%), NoHealthRegen (+30%)
Performance: Perfect Completion (1.5x)

Total = 500 × 1.5 × 1.2 × 1.5 × 1.3 × 1.5 = 3,510 XP
```

---

## 🛠️ Setup Guide

### Creating a Challenge with Modifiers

1. **Create Challenge Data Asset**
   - Right-click → Create → Division Game → Challenge Data

2. **Configure Base Settings**
   ```
   Challenge Name: "Elite Squad Elimination"
   Difficulty: Hard
   Base XP Reward: 500
   Base Currency Reward: 250
   ```

3. **Add Modifiers**
   - Expand "Challenge Modifiers" section
   - Click "+" to add new modifier
   - Set Type and Value:
     - Type: IncreasedEnemyHealth
     - Value: 1.5
     - Is Active: ☑

4. **Configure Bonuses**
   ```
   Perfect Completion Bonus: ☑
   Perfect Completion XP Multiplier: 1.5
   
   Speed Completion Bonus: ☑
   Speed Threshold Percentage: 0.5
   Speed Completion XP Multiplier: 1.25
   ```

### Example Configurations

#### High Risk - High Reward Challenge
```
Modifiers:
  - IncreasedEnemyHealth: 2.0
  - IncreasedEnemyDamage: 1.5
  - TimeTrial: 0.6 (40% less time)
  - IronMan: 1.0 (active)

Base XP: 1000
Potential Max XP: ~4,500 (with perfect + speed bonuses)
```

#### Speed Run Challenge
```
Modifiers:
  - TimeTrial: 0.5 (50% time limit)
  - DoubleXP: 2.0

Bonuses:
  - Speed Completion: ☑ (threshold 80%)
  - Perfect Completion: ☑

Rewards fast, skilled players
```

#### Elite Enemy Gauntlet
```
Modifiers:
  - EliteEnemiesOnly: 1.0
  - IncreasedEnemySpeed: 1.3
  - NoHealthRegen: 1.0

Guaranteed Loot Rarity: Rare
Loot Count: 3
```

---

## 📊 Integration

### Player Damage Tracking

Add `ChallengeBonusTracker` component to player:
```csharp
// Automatically tracks:
- Player taking damage
- Player death
- Enemy kills (requires manual call)
```

### Enemy Kill Tracking

In your enemy death handler:
```csharp
void OnEnemyDeath()
{
    ChallengeBonusTracker tracker = player.GetComponent<ChallengeBonusTracker>();
    if (tracker != null)
    {
        tracker.OnEnemyKilled(this.gameObject);
    }
}
```

### Detection Tracking (Stealth)

In enemy AI detection:
```csharp
void OnPlayerDetected()
{
    ChallengeBonusTracker tracker = player.GetComponent<ChallengeBonusTracker>();
    if (tracker != null)
    {
        tracker.OnPlayerDetected();
    }
}
```

---

## 🎮 API Reference

### ChallengeData Methods

```csharp
// Check if modifier is active
bool HasModifier(ModifierType type)

// Get modifier value
float GetModifierValue(ModifierType type)

// Get total rewards with all bonuses
int GetTotalXPReward(playerLevel, difficulty, perfect, speed, stealth)
int GetTotalCurrencyReward(playerLevel, difficulty)
LootManager.Rarity GetTotalLootRarity(difficulty)
int GetTotalLootCount()

// Get modified values
float GetTotalHealthMultiplier(playerLevel, difficulty)
float GetTotalDamageMultiplier(playerLevel, difficulty)
float GetModifiedTimeLimit()

// Get descriptions
string GetModifiersDescription()
```

### ActiveChallenge Methods

```csharp
// Reward getters
int GetXPReward()              // Base scaled XP
int GetTotalXPReward()         // With all bonuses
int GetCurrencyReward()        // Base scaled currency
int GetTotalCurrencyReward()   // With modifiers

// Tracking
void OnPlayerDamaged()
void OnPlayerDetected()
void OnEnemyKilled()
void MarkCompleted()

// Bonus checks
bool CheckSpeedCompletion()
string GetBonusSummary()       // "FLAWLESS VICTORY!\nSPEED RUN BONUS!"
```

---

## 💡 Best Practices

1. **Balance Modifiers**
   - High risk modifiers should have good XP bonuses
   - Stack 2-3 modifiers max for clarity
   - Test reward scaling at different player levels

2. **Communicate Modifiers**
   - Display active modifiers in challenge UI
   - Show potential bonus rewards
   - Explain what triggers bonuses

3. **Progression**
   - Early challenges: Few/no modifiers
   - Mid-game: 1-2 modifiers
   - End-game: Multiple stacked modifiers

4. **Variety**
   - Mix modifier types for different playstyles
   - Some challenges favor speed, others caution
   - Rotate modifiers for dailies/weeklies

---

## 🐛 Troubleshooting

**Modifiers not applying:**
- Check `isActive = true`
- Verify ChallengeSpawner calls ApplyDifficultyScalingToEnemy()

**Bonuses not calculating:**
- Ensure ChallengeBonusTracker is on player
- Check bonus flags: `perfectCompletionBonus`, etc.
- Verify MarkCompleted() is called

**Rewards too high/low:**
- Adjust `rewardScalingMultiplier` in ChallengeData
- Check modifier value settings
- Review stacking calculations

---

## 📝 Future Enhancements

- [ ] Implement NightMode visibility reduction
- [ ] Implement LimitedAmmo inventory integration
- [ ] Add Survival Mode endless waves
- [ ] Add Pacifist challenge tracking
- [ ] Create UI for modifier display
- [ ] Add challenge leaderboards
- [ ] Weekly rotating modifiers
