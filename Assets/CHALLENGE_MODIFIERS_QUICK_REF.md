# Challenge Modifiers & Rewards - Quick Reference

## 🎯 Modifier Types

### Enemy Modifiers
| Modifier | Effect | Reward Bonus |
|----------|--------|--------------|
| IncreasedEnemyHealth | Health × value | Auto |
| IncreasedEnemyDamage | Damage × value | Auto |
| IncreasedEnemySpeed | Speed × value | Auto |
| EliteEnemiesOnly | All elite variants | Elite rewards |

### Environment Modifiers
| Modifier | Effect | Reward Bonus |
|----------|--------|--------------|
| TimeTrial | Time limit × value | +25% XP |
| LimitedAmmo | Restricted ammo | +20% XP |
| NoHealthRegen | No passive regen | +30% XP |
| IronMan | One death = fail | +50% XP |

### Reward Modifiers
| Modifier | Effect |
|----------|--------|
| DoubleXP | 2× all XP |
| DoubleCurrency | 2× all currency |
| BonusLootDrop | Extra loot items |
| GuaranteedRareLoot | Min. Rare quality |

---

## 💰 Reward Calculations

### Base Scaling
```
Easy:     0.75× rewards
Medium:   1.0× rewards
Hard:     1.5× rewards
Extreme:  2.0× rewards
```

### Performance Bonuses
```
Perfect (no damage):  1.5× XP
Speed (<50% time):    1.25× XP
Stealth (undetected): 1.3× XP
```

### Example Calculation
```
Base XP:        500
Difficulty:     Hard (1.5×)
Perfect:        Yes (1.5×)
Speed:          Yes (1.25×)
IronMan:        Active (1.5×)

Total = 500 × 1.5 × 1.5 × 1.25 × 1.5 = 2,109 XP
```

---

## 🛠️ Quick Setup

### 1. Add Modifier to Challenge
```
ChallengeData → Challenge Modifiers
  + Add Element
  Type: IncreasedEnemyHealth
  Value: 1.5
  Is Active: ☑
```

### 2. Configure Bonuses
```
Perfect Completion Bonus: ☑
  Multiplier: 1.5

Speed Completion Bonus: ☑
  Threshold: 0.5 (50%)
  Multiplier: 1.25
```

### 3. Add Tracker to Player
```csharp
// Add component
player.AddComponent<ChallengeBonusTracker>();
```

---

## 📋 Preset Configurations

### Speed Run
```
Modifiers: TimeTrial (0.5), DoubleXP
Bonuses: Speed ☑
Result: Rewards fast players
```

### Iron Man
```
Modifiers: IronMan, NoHealthRegen, IncreasedEnemyDamage (1.3)
Bonuses: Perfect ☑
Result: High risk, high reward
```

### Elite Gauntlet
```
Modifiers: EliteEnemiesOnly, IncreasedEnemyHealth (1.5)
Loot: Guaranteed Rare+
Result: Better enemy drops
```

### Weekend Event
```
Modifiers: DoubleXP, DoubleCurrency, BonusLootDrop (0.5)
Result: 2× rewards + extra loot
```

---

## 🎮 Code Snippets

### Check Active Modifiers
```csharp
if (challengeData.HasModifier(ChallengeModifier.ModifierType.DoubleXP))
{
    // Do something
}

float value = challengeData.GetModifierValue(ChallengeModifier.ModifierType.IncreasedEnemyHealth);
```

### Get Total Rewards
```csharp
int xp = challenge.GetTotalXPReward();
int currency = challenge.GetTotalCurrencyReward();
LootManager.Rarity rarity = challenge.GetTotalLootRarity();
int lootCount = challenge.GetTotalLootCount();
```

### Track Events
```csharp
// Automatic
ChallengeBonusTracker.OnPlayerDamaged()

// Manual
tracker.OnEnemyKilled(enemy);
tracker.OnPlayerDetected();
```

---

## ✅ Checklist

**Challenge Setup:**
- [ ] Base rewards configured
- [ ] Modifiers added and active
- [ ] Bonus settings configured
- [ ] Time limit appropriate

**Integration:**
- [ ] ChallengeBonusTracker on player
- [ ] Enemy kill events tracked
- [ ] Detection events (if stealth)
- [ ] Damage events hooked up

**Testing:**
- [ ] Modifiers apply correctly
- [ ] Rewards calculate properly
- [ ] Bonuses trigger
- [ ] UI displays modifiers

---

## 🐛 Common Issues

**Issue:** Bonuses not triggering
**Fix:** Check ChallengeBonusTracker attached to player

**Issue:** Modifiers not applying
**Fix:** Verify `isActive = true` in inspector

**Issue:** Rewards too high
**Fix:** Adjust `rewardScalingMultiplier` in ChallengeData

**Issue:** Speed bonus always/never triggers
**Fix:** Check `speedThresholdPercentage` (0.5 = 50% of time)

---

## 📖 Full Documentation
See `CHALLENGE_MODIFIERS_GUIDE.md` for complete details.
