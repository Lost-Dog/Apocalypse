# Health & Stamina on Kill - Setup Guide

## ✅ Feature Added!

The `EnemyKillRewardHandler` script now restores player health and stamina when you kill enemies!

---

## 🎯 What You Get on Kill

```
Kill Enemy
    ↓
Player Rewards:
├── ✅ Experience Points (XP)
├── ✅ Loot Drop (chance-based)
├── ✅ Health Restoration (20 HP) ← NEW!
└── ✅ Stamina Restoration (30 stamina) ← NEW!
```

---

## ⚙️ New Configuration Options

When you add `EnemyKillRewardHandler` to an enemy, you'll see:

### **Health & Stamina on Kill Section:**

```yaml
Restore Health On Kill: ☑ true
├── Health Restore Amount: 20
└── Health Restore Percentage: 0

Restore Stamina On Kill: ☑ true
├── Stamina Restore Amount: 30
└── Stamina Restore Percentage: 0
```

---

## 📊 Configuration Examples

### **Example 1: Fixed Amount (Default)**
```yaml
Restore Health On Kill: ☑ true
Health Restore Amount: 20
Health Restore Percentage: 0

Restore Stamina On Kill: ☑ true
Stamina Restore Amount: 30
Stamina Restore Percentage: 0

Result:
├── Kill enemy → +20 HP (fixed)
└── Kill enemy → +30 Stamina (fixed)
```

### **Example 2: Percentage-Based**
```yaml
Restore Health On Kill: ☑ true
Health Restore Amount: 0
Health Restore Percentage: 0.1  (10%)

Restore Stamina On Kill: ☑ true
Stamina Restore Amount: 0
Stamina Restore Percentage: 0.2  (20%)

Result (if player has 100 max HP, 100 max stamina):
├── Kill enemy → +10 HP (10% of 100)
└── Kill enemy → +20 Stamina (20% of 100)
```

### **Example 3: Combined Fixed + Percentage**
```yaml
Restore Health On Kill: ☑ true
Health Restore Amount: 10
Health Restore Percentage: 0.05  (5%)

Restore Stamina On Kill: ☑ true
Stamina Restore Amount: 20
Stamina Restore Percentage: 0.1  (10%)

Result (if player has 100 max HP, 100 max stamina):
├── Kill enemy → +15 HP (10 + 5% of 100)
└── Kill enemy → +30 Stamina (20 + 10% of 100)
```

### **Example 4: Disabled**
```yaml
Restore Health On Kill: ☐ false
Restore Stamina On Kill: ☐ false

Result:
├── No health restored
└── No stamina restored
```

---

## 🎮 Gameplay Examples

### **Balanced (Default):**
```yaml
Health Restore Amount: 20
Stamina Restore Amount: 30

Player Stats: 100 HP, 100 Stamina
Enemy Fight: -30 HP, -40 Stamina
After Kill: +20 HP, +30 Stamina

Net Result:
├── Health: -10 HP per fight
└── Stamina: -10 Stamina per fight
```

### **Aggressive (High Restore):**
```yaml
Health Restore Amount: 40
Stamina Restore Amount: 50

Player Stats: 100 HP, 100 Stamina
Enemy Fight: -30 HP, -40 Stamina
After Kill: +40 HP, +50 Stamina

Net Result:
├── Health: +10 HP per fight (gain!)
└── Stamina: +10 Stamina per fight (gain!)
```

### **Survival (Low Restore):**
```yaml
Health Restore Amount: 10
Stamina Restore Amount: 15

Player Stats: 100 HP, 100 Stamina
Enemy Fight: -30 HP, -40 Stamina
After Kill: +10 HP, +15 Stamina

Net Result:
├── Health: -20 HP per fight
└── Stamina: -25 Stamina per fight
```

### **Percentage Scaling (Scales with Player):**
```yaml
Health Restore Amount: 0
Health Restore Percentage: 0.15  (15%)
Stamina Restore Amount: 0
Stamina Restore Percentage: 0.25  (25%)

Level 1 Player: 100 HP, 100 Stamina
├── Kill → +15 HP, +25 Stamina

Level 10 Player: 200 HP, 150 Stamina
├── Kill → +30 HP, +37.5 Stamina

Scales with player level!
```

---

## 🧪 Quick Test

**1. Select Enemy:**
```
Hierarchy → Patrol AI
```

**2. Check Component:**
```
Inspector → EnemyKillRewardHandler
Scroll to "Health & Stamina on Kill" section
```

**3. Default Settings:**
```
✅ Restore Health On Kill: true
   Health Restore Amount: 20
   
✅ Restore Stamina On Kill: true
   Stamina Restore Amount: 30
```

**4. Test in Play Mode:**
```
1. Note player health (e.g., 80/100 HP)
2. Kill enemy
3. Check Console:
   "Restored 20 health on kill! (Health: 100/100)"
   "Restored 30 stamina on kill! (Stamina: 100/100)"
4. Player health increased by 20!
```

---

## 📋 Enemy Type Presets

### **Normal Enemy (Balanced):**
```yaml
Base XP: 50
Loot Chance: 0.5

Health Restore: 20
Stamina Restore: 30

Good for: Standard combat loop
```

### **Elite Enemy (High Reward):**
```yaml
Base XP: 100
Elite: ☑ YES
Loot Chance: 0.75

Health Restore: 40
Stamina Restore: 50

Good for: Rewarding difficult fights
```

### **Boss Enemy (Full Restore):**
```yaml
Base XP: 250
Boss: ☑ YES
Loot Chance: 1.0

Health Restore: 100
Stamina Restore: 100

Good for: Major victories
```

### **Weak Enemy (Low Reward):**
```yaml
Base XP: 25
Loot Chance: 0.3

Health Restore: 10
Stamina Restore: 15

Good for: Trash mobs
```

### **No Restore (Challenge Mode):**
```yaml
Base XP: 50
Loot Chance: 0.5

Restore Health On Kill: ☐ false
Restore Stamina On Kill: ☐ false

Good for: Hardcore difficulty
```

---

## 🔧 How It Works

### **Health Restoration:**

```csharp
On Enemy Death:
1. Check if restoreHealthOnKill = true
2. Find player's JUHealth component
3. Calculate restore amount:
   - Fixed amount: healthRestoreAmount
   - Percentage: maxHealth × healthRestorePercentage
   - Total: fixed + percentage
4. Add to player health (capped at maxHealth)
5. Log result to Console
```

### **Stamina Restoration:**

```csharp
On Enemy Death:
1. Check if restoreStaminaOnKill = true
2. Find PlayerStaminaDisplay component
3. Calculate restore amount:
   - Fixed amount: staminaRestoreAmount
   - Percentage: maxStamina × staminaRestorePercentage
   - Total: fixed + percentage
4. Add to player stamina (capped at maxStamina)
5. Log result to Console
```

---

## 💡 Design Tips

### **For Balanced Gameplay:**

**1. Restore < Damage Taken:**
```
Enemy deals ~30 damage
Restore 20 HP on kill
Net: -10 HP per fight

Encourages strategic play
```

**2. Restore = Damage Taken:**
```
Enemy deals ~20 damage
Restore 20 HP on kill
Net: 0 HP per fight

Infinite combat loop (good for horde modes)
```

**3. Restore > Damage Taken:**
```
Enemy deals ~10 damage
Restore 20 HP on kill
Net: +10 HP per fight

Encourages aggressive play
```

### **For Different Enemy Types:**

**Weak Enemies:**
```
Low health restore (5-10 HP)
Low stamina restore (10-15)
Fights should be easy, low reward
```

**Normal Enemies:**
```
Medium restore (15-25 HP)
Medium stamina (25-35)
Balanced risk/reward
```

**Elite Enemies:**
```
High restore (30-50 HP)
High stamina (40-60)
Hard fight, high reward
```

**Bosses:**
```
Full or near-full restore (75-100 HP)
Full stamina (80-100)
Epic victory feeling!
```

---

## ⚖️ Balance Recommendations

### **By Difficulty:**

**Easy Mode:**
```yaml
Health Restore: 40
Stamina Restore: 50
Health %: 0.2  (20%)
Stamina %: 0.25  (25%)

Result: Combat is very forgiving
```

**Normal Mode (Default):**
```yaml
Health Restore: 20
Stamina Restore: 30
Health %: 0
Stamina %: 0

Result: Balanced risk/reward
```

**Hard Mode:**
```yaml
Health Restore: 10
Stamina Restore: 15
Health %: 0
Stamina %: 0

Result: Every fight matters
```

**Survival Mode:**
```yaml
Restore Health: ☐ false
Restore Stamina: ☐ false

Result: No regeneration, hardcore!
```

---

## 🎯 Common Use Cases

### **Use Case 1: Horde Mode**
```yaml
Goal: Keep fighting waves forever

Settings:
├── Health Restore: 25
├── Stamina Restore: 40
├── Health %: 0.1
└── Stamina %: 0.15

Result: Player can sustain long fights
```

### **Use Case 2: Stealth Game**
```yaml
Goal: Punish combat, reward stealth

Settings:
├── Restore Health: ☐ false
├── Restore Stamina: ☐ false

Result: Player must avoid fights or use items
```

### **Use Case 3: Action RPG**
```yaml
Goal: Encourage aggressive combat

Settings:
├── Health Restore: 30
├── Stamina Restore: 50
├── Health %: 0.15
└── Stamina %: 0.2

Result: Rewarding "life steal" feeling
```

### **Use Case 4: Battle Royale**
```yaml
Goal: Balance survival vs combat

Settings:
├── Health Restore: 15
├── Stamina Restore: 20
└── Only elite/boss enemies restore

Result: Strategic target selection
```

---

## ✅ Setup Checklist

**For Each Enemy Type:**

- [ ] `EnemyKillRewardHandler` component added
- [ ] `JUHealth` component on enemy
- [ ] Configure XP reward
- [ ] Configure loot chance
- [ ] **NEW: Set health restore amount**
- [ ] **NEW: Set stamina restore amount**
- [ ] **NEW: Enable/disable restore toggles**
- [ ] Test in Play Mode
- [ ] Verify Console messages
- [ ] Verify player health/stamina increases

**Scene Requirements:**

- [ ] Player has "Player" tag
- [ ] Player has `JUHealth` component
- [ ] Player has `PlayerSystemBridge` component
- [ ] `PlayerStaminaDisplay` in scene
- [ ] `GameManager` exists
- [ ] `LootManager` assigned

---

## 🐛 Troubleshooting

### **Health Not Restoring:**

**Problem:** No health gained on kill

**Solutions:**
```
1. Check "Restore Health On Kill" is enabled (☑)
2. Check health amount > 0
3. Verify player has JUHealth component
4. Check player isn't at max health already
5. Look for Console warnings
```

### **Stamina Not Restoring:**

**Problem:** No stamina gained on kill

**Solutions:**
```
1. Check "Restore Stamina On Kill" is enabled (☑)
2. Check stamina amount > 0
3. Verify PlayerStaminaDisplay exists in scene
4. Check player isn't at max stamina
5. Look for Console warnings
```

### **Console Warning: "JUHealth not found":**

```
Problem: Player doesn't have health component
Fix: Player should have JUHealth (it's already there)
```

### **Console Warning: "PlayerStaminaDisplay not found":**

```
Problem: Stamina system missing from scene
Fix: Add PlayerStaminaDisplay to scene or player
```

---

## 📊 Quick Reference

### **Settings:**

```
Amount Field:
├── Fixed number restored (e.g., 20 HP)
└── Simple, predictable

Percentage Field:
├── % of max value (0.1 = 10%)
├── Scales with player progression
└── Good for endgame balance

Combined:
├── Fixed + Percentage
├── Best of both worlds
└── Example: 10 + 5% = flexible scaling
```

### **Formula:**

```csharp
Total Health Restored = 
    healthRestoreAmount + 
    (playerMaxHealth × healthRestorePercentage)

Total Stamina Restored = 
    staminaRestoreAmount + 
    (playerMaxStamina × staminaRestorePercentage)
```

---

## 🚀 Next Steps

**1. Update Your Enemies:**
```
Select each enemy type
Adjust health/stamina restore values
Test balance in Play Mode
```

**2. Adjust for Difficulty:**
```
Easy: High restore (40/50)
Normal: Medium restore (20/30)
Hard: Low restore (10/15)
Survival: No restore (disabled)
```

**3. Test Different Scenarios:**
```
Horde mode: High restore
Stealth game: No restore
Action RPG: Percentage-based
Boss fights: Full restore
```

---

## ✅ Summary

**Default Settings (Already Configured):**
```yaml
✅ Restore Health On Kill: true
   Health Restore Amount: 20
   Health Restore Percentage: 0

✅ Restore Stamina On Kill: true
   Stamina Restore Amount: 30
   Stamina Restore Percentage: 0
```

**What Happens:**
- Kill enemy → Player gains 20 HP
- Kill enemy → Player gains 30 Stamina
- Works automatically with existing system
- Toggleable per enemy or globally

**Your enemies now reward aggressive play! 🎮⚔️💚**
