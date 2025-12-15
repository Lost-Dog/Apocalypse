# Kill Rewards - Quick Reference Card

## ✅ Feature Complete!

**Script:** `EnemyKillRewardHandler` (already updated)  
**Status:** ✅ Health on Kill + Stamina on Kill working!

---

## 🎯 What You Get Per Kill

```
KILL ENEMY → INSTANT REWARDS:

✅ XP          → 40-60 (base 50 ± variance)
✅ Loot        → 50% chance to drop
✅ Health      → +20 HP restored
✅ Stamina     → +30 Stamina restored
```

---

## ⚡ Quick Setup (3 Steps)

### **Step 1: Add to Enemy**
```
Select enemy → Add Component → EnemyKillRewardHandler
```

### **Step 2: Configure (Inspector)**
```yaml
Base XP Reward: 50
Loot Drop Chance: 0.5

Restore Health On Kill: ☑ true
Health Restore Amount: 20

Restore Stamina On Kill: ☑ true
Stamina Restore Amount: 30
```

### **Step 3: Test**
```
Play Mode → Kill enemy → Check Console:
"Restored 20 health on kill!"
"Restored 30 stamina on kill!"
```

---

## 📊 Copy-Paste Presets

### **Normal Enemy:**
```yaml
XP: 50
Loot: 0.5
Health: 20
Stamina: 30
```

### **Elite Enemy:**
```yaml
XP: 100
Elite: ☑
Loot: 0.75
Health: 40
Stamina: 50
```

### **Boss:**
```yaml
XP: 250
Boss: ☑
Loot: 1.0
Health: 100
Stamina: 100
```

### **Weak Enemy:**
```yaml
XP: 25
Loot: 0.3
Health: 10
Stamina: 15
```

---

## 🎮 Balance Guide

```
RESTORATION PHILOSOPHY:

Restore < Damage:
├── Player loses net HP/Stamina per fight
├── Must use items or rest
└── Survival/Challenge mode

Restore = Damage:
├── Player breaks even
├── Infinite combat capable
└── Horde mode

Restore > Damage:
├── Player gains net HP/Stamina
├── Aggressive play rewarded
└── Action game
```

---

## ⚙️ Configuration Fields

```
Health & Stamina on Kill:

Restore Health On Kill: ☑
├── Toggle health restore on/off

Health Restore Amount: 20
├── Fixed HP restored (0-100+)

Health Restore Percentage: 0
├── % of max health (0.0 - 1.0)
├── 0.1 = 10% of max health
└── Scales with player level

Restore Stamina On Kill: ☑
├── Toggle stamina restore on/off

Stamina Restore Amount: 30
├── Fixed stamina restored (0-100+)

Stamina Restore Percentage: 0
├── % of max stamina (0.0 - 1.0)
├── 0.2 = 20% of max stamina
└── Scales with player level
```

---

## 💡 Examples

### **Fixed Amount (Simple):**
```yaml
Health Amount: 20
Health %: 0

Result: Always +20 HP
```

### **Percentage (Scaling):**
```yaml
Health Amount: 0
Health %: 0.15  (15%)

Result: 
├── Level 1 (100 max HP) → +15 HP
├── Level 5 (150 max HP) → +22.5 HP
└── Level 10 (200 max HP) → +30 HP
```

### **Combined (Best of Both):**
```yaml
Health Amount: 10
Health %: 0.1  (10%)

Result:
├── Level 1 (100 max HP) → +20 HP (10 + 10)
├── Level 5 (150 max HP) → +25 HP (10 + 15)
└── Level 10 (200 max HP) → +30 HP (10 + 20)
```

---

## 🔍 Debug Console Messages

### **Success:**
```
"Patrol AI killed! Player gained 50 XP"
"Restored 20.0 health on kill! (Health: 80.0/100)"
"Restored 30.0 stamina on kill! (Stamina: 100.0/100)"
"Loot dropped at (10.5, 1.0, 8.2)"
```

### **Warnings:**
```
⚠️ "JUHealth component not found on player!"
   → Player needs JUHealth component

⚠️ "PlayerStaminaDisplay not found!"
   → Add PlayerStaminaDisplay to scene
```

---

## ✅ Requirements

**On Enemy:**
- [x] `EnemyKillRewardHandler` component
- [x] `JUHealth` component

**On Player:**
- [x] Tag: "Player"
- [x] `JUHealth` component
- [x] `PlayerSystemBridge` component

**In Scene:**
- [x] `PlayerStaminaDisplay` component
- [x] `GameManager` instance
- [x] `LootManager` assigned

---

## 🎯 Difficulty Presets

### **Easy:**
```yaml
Health: 40
Stamina: 50
Health %: 0.2
Stamina %: 0.25
```

### **Normal (Default):**
```yaml
Health: 20
Stamina: 30
Health %: 0
Stamina %: 0
```

### **Hard:**
```yaml
Health: 10
Stamina: 15
Health %: 0
Stamina %: 0
```

### **Survival:**
```yaml
Restore Health: ☐ false
Restore Stamina: ☐ false
```

---

## 📋 Per-Enemy-Type Settings

```
WEAK (Zombie):
├── Health: 10
├── Stamina: 15
└── Quick kills, small reward

NORMAL (Soldier):
├── Health: 20
├── Stamina: 30
└── Balanced combat

STRONG (Heavy):
├── Health: 30
├── Stamina: 40
└── Tough fight, good reward

ELITE:
├── Health: 50
├── Stamina: 60
└── Challenge, high reward

BOSS:
├── Health: 100
├── Stamina: 100
└── Epic victory!
```

---

## 🧪 Quick Test Sequence

```
1. Select "Patrol AI" enemy
2. Verify EnemyKillRewardHandler exists
3. Check settings:
   ✓ Restore Health On Kill: true
   ✓ Health Restore Amount: 20
   ✓ Restore Stamina On Kill: true
   ✓ Stamina Restore Amount: 30
4. Enter Play Mode
5. Damage player to 50 HP
6. Kill enemy
7. Verify:
   ✓ Health increased to 70 HP
   ✓ Stamina increased
   ✓ Console shows restore messages
```

---

## 🚀 Advanced: Percentage Scaling

```
WHEN TO USE PERCENTAGES:

Use Fixed Amounts When:
✓ Simple, predictable gameplay
✓ Early game (levels 1-5)
✓ Fixed difficulty curve

Use Percentages When:
✓ Long progression system
✓ Endgame balancing
✓ Player power scales widely

Use Combined When:
✓ Best of both worlds
✓ Base amount + scaling bonus
✓ Example: 10 HP + 10% = grows with player
```

---

## 💡 Design Patterns

### **Pattern 1: Life Steal**
```yaml
High health restore (30-50)
Low stamina restore (10-20)
Encourages: Aggressive melee
```

### **Pattern 2: Endurance Fighter**
```yaml
Low health restore (10-15)
High stamina restore (40-60)
Encourages: Mobile, dodging combat
```

### **Pattern 3: Balanced**
```yaml
Medium health (20)
Medium stamina (30)
Encourages: Varied playstyle
```

### **Pattern 4: Boss Killer**
```yaml
Weak enemies: 5 HP, 10 Stamina
Elite enemies: 30 HP, 40 Stamina
Bosses: 100 HP, 100 Stamina
Encourages: Target priority
```

---

## ✅ Final Checklist

**Per Enemy:**
- [ ] Component added
- [ ] XP configured
- [ ] Loot chance set
- [ ] **Health restore configured**
- [ ] **Stamina restore configured**
- [ ] Tested in Play Mode
- [ ] Console shows restore messages

**Scene Setup:**
- [ ] Player ready
- [ ] Stamina system in scene
- [ ] GameManager ready
- [ ] All rewards working

---

**Your kill reward system is complete! 🎮💚⚡**

**Current Selected Enemies:**
- Patrol AI
- Patrol AI_Elite  
- Patrol AI_Boss
- Zombie AI

**Next:** Configure each one with appropriate rewards! ✅
