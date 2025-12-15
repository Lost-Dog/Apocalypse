# Temperature System - Simple Explanation

## 🌡️ How Temperature Works

### **Simple Rule:**
```
COLD = BAD ❄️
WARM = GOOD ☀️

The HIGHER your temperature, the BETTER!
```

---

## 📊 Temperature Scale

```
┌─────────────────────────────────────────────┐
│          TEMPERATURE ZONES                  │
├─────────────────────────────────────────────┤
│                                             │
│  100% ████████████████████████ PERFECT! ✅  │
│   90% ████████████████████████ Excellent ✅ │
│   80% ████████████████████████ Great ✅     │
│   70% ████████████████████████ Good ✅      │
│   60% ████████████████████████ Normal ✅    │
│   50% ████████████████████████ Normal ✅    │
│   40% ████████████████████████ Cool ✅      │
│   30% ████████████████████████ Cold ⚠️      │
│   20% ════════════════════════ ← DANGER LINE│
│   10% ░░░░░░░░░░░░░░░░░░░░░░░░ FREEZING ❌  │
│    0% ░░░░░░░░░░░░░░░░░░░░░░░░ DEATH 💀     │
│                                             │
└─────────────────────────────────────────────┘

Safe Zone:   21-100% (No damage)
Danger Zone: 0-20%   (2 HP/sec damage)
```

---

## ⚠️ Damage Rules

### **When Does Temperature Damage You?**

```
Temperature ≤ 20% → YOU TAKE DAMAGE ❌
Temperature > 20% → YOU ARE SAFE ✅
```

### **Damage Rate:**
```
At 0-20% Temperature:
├── Damage: 2 HP per second
├── Death Time: 50 seconds (at 100 HP)
└── Warning: Critical Cold!

At 21-100% Temperature:
├── Damage: None
├── Status: Safe
└── Higher = Better!
```

---

## 🔥 Staying Warm

### **Ways to Increase Temperature:**

**1. Stand Near Fire:**
```
Effect: +10 temperature per second
Code: survivalManager.SetNearFire(true)
Visual: 🔥
```

**2. Go Indoors:**
```
Effect: +5 temperature per second
Code: survivalManager.SetIndoors(true)
Visual: 🏠
```

**3. Drink Hot Beverage:**
```
Effect: +20 temperature instant
Code: survivalManager.WarmUp(20)
Visual: ☕
```

**4. Wear Warm Clothes (future):**
```
Effect: Slower temperature decrease
Visual: 🧥
```

---

## ❄️ What Makes You Cold

### **Ways Temperature Decreases:**

**1. Cold Zones:**
```
Effect: 2x faster temperature drop
Code: survivalManager.SetInColdZone(true)
Visual: Snow, ice, blizzard
```

**2. Natural Decay (if enabled):**
```
Effect: -0.5 temperature per second
Toggle: enableTemperatureDecrease
```

**3. Weather Events (future):**
```
Rain: -1 temp/sec
Snow: -2 temp/sec
Blizzard: -5 temp/sec
```

---

## 🎮 Temperature Gameplay Loop

```
NORMAL GAMEPLAY:

Start at 100% (Warm) ✅
    ↓
Enter cold zone (snow area)
    ↓
Temperature drops slowly
    ↓
Reaches 50% (Still safe)
    ↓
Keep exploring
    ↓
Drops to 25% (Getting cold)
    ↓
Warning: Temperature low!
    ↓
Drops to 20% → CRITICAL ⚠️
    ↓
HEALTH STARTS DECREASING
-2 HP per second
    ↓
PLAYER OPTIONS:
├─→ Find building (indoors) → +5/sec
├─→ Light campfire → +10/sec
├─→ Drink hot beverage → +20 instant
└─→ Leave cold zone → Stop rapid cooling
    ↓
Temperature rises above 20%
    ↓
Damage STOPS ✅
    ↓
Back to exploring!
```

---

## 💡 Examples

### **Example 1: Exploring Snowy Area**
```
1. Player enters snow zone
2. Temperature: 100% → 90% → 80% (still safe)
3. Continues exploring
4. Temperature: 60% → 40% (getting cold)
5. Temperature: 25% (warning appears)
6. Temperature: 20% → DAMAGE STARTS!
7. Player finds building
8. Goes inside (SetIndoors = true)
9. Temperature: 20% → 30% → 50% (warming up)
10. Damage STOPS at 21%
11. Fully warms to 100%
12. Continues journey
```

### **Example 2: Campfire Strategy**
```
1. Player in cold area
2. Temperature dropping fast
3. Reaches 18% (taking damage)
4. Player lights campfire
5. SetNearFire(true) → +10/sec
6. Temperature: 18% → 28% → 38%
7. Damage stops at 21%
8. Rests until 100%
9. Continues exploring
```

### **Example 3: Emergency Hot Drink**
```
1. Temperature: 15% (critical!)
2. Health: 50 HP (losing 2/sec)
3. Player drinks hot coffee
4. WarmUp(20) → Temp instantly 35%
5. Damage STOPS immediately
6. Player is safe!
```

---

## 🎯 Temperature Management Tips

### **For Players:**

**Stay Warm:**
- Monitor temperature bar
- Find shelter before 20%
- Carry hot drinks for emergencies
- Use campfires strategically
- Plan routes near buildings

**Warning Signs:**
- Temperature < 50%: Start looking for warmth
- Temperature < 30%: Find shelter soon
- Temperature = 20%: CRITICAL - act now!
- Temperature < 20%: Taking damage - emergency!

**Recovery:**
- Indoors: Slow but safe (+5/sec)
- Campfire: Fast warmup (+10/sec)
- Hot drinks: Emergency boost (+20 instant)

---

## ⚙️ For Game Designers

### **Temperature Settings:**

**Conservative (Easy):**
```
Critical Threshold: 0.1 (10%)
Cold Damage: 1 HP/sec
Temperature Decrease: 0.25/sec
Result: Very forgiving
```

**Balanced (Default):**
```
Critical Threshold: 0.2 (20%)
Cold Damage: 2 HP/sec
Temperature Decrease: 0.5/sec
Result: Moderate challenge
```

**Hardcore (Survival):**
```
Critical Threshold: 0.3 (30%)
Cold Damage: 5 HP/sec
Temperature Decrease: 1/sec
Result: Constant threat
```

### **Environmental Modifiers:**

```
Indoor Warmth:       +5/sec (safe havens)
Fire Warmth:         +10/sec (player-created safety)
Cold Zone Multiplier: 2x (dangerous areas)
Item Boost:          +20 instant (consumables)
```

---

## 📊 Death Time Calculator

```
Starting at Full Health (100 HP):

Temperature 15% (Cold):
├── Damage: 2 HP/sec
└── Death: 50 seconds

Temperature 15% + Infection 100%:
├── Damage: 4 HP/sec (2+2)
└── Death: 25 seconds

Temperature 50% (Safe):
├── Damage: 0 HP/sec
└── Death: Never (from cold)

Temperature 100% (Perfect):
├── Damage: 0 HP/sec
└── Death: Never (from cold)
```

---

## 🧪 Quick Test

**Test in Play Mode:**

1. **Start Safe:**
   - Temperature: 100%
   - Health: Stable ✅

2. **Go Cold:**
   - Set temperature to 15%
   - Health decreases 2/sec ❌

3. **Warm Up:**
   - Set temperature to 50%
   - Health stable again ✅

4. **Go Hot:**
   - Set temperature to 100%
   - Health stable (perfect!) ✅

**Remember:** Higher temperature = Better health!

---

## 🎨 Visual Feedback Ideas

### **Temperature Indicators:**

**Safe (50-100%):**
```
Bar Color: Green/Yellow
Effect: None
Sound: None
UI: Normal display
```

**Getting Cold (21-49%):**
```
Bar Color: Yellow/Orange
Effect: Slight screen darkening
Sound: Wind ambience
UI: "Cold" text
```

**Critical (0-20%):**
```
Bar Color: Blue/White
Effect: Screen frost/vignette
Sound: Shivering, wind howling
UI: "FREEZING!" warning
Screen: Blue tint, frost edges
Player: Shaking animation
```

---

## 🔄 Temperature Recovery Speed

```
FROM CRITICAL (15%) TO SAFE (21%):

No Help:        Never recovers (natural decay)
Indoors:        1.2 seconds (+5/sec)
Near Fire:      0.6 seconds (+10/sec)
Hot Drink:      Instant! (+20 boost)

FROM CRITICAL (15%) TO FULL (100%):

Indoors:        17 seconds (+5/sec → 85 points)
Near Fire:      8.5 seconds (+10/sec → 85 points)
Both:           5.7 seconds (+15/sec → 85 points)
```

---

## ✅ Summary

**Temperature System Rules:**
1. ✅ Higher temperature = Better
2. ✅ 21-100% = Safe zone
3. ❌ 0-20% = Danger zone (2 HP/sec)
4. 🔥 Fire/Indoors = Warmth
5. ❄️ Cold zones = Faster cooling
6. ☕ Hot drinks = Emergency boost

**Remember:**
- Monitor your temperature bar
- Stay above 20% to avoid damage
- Use fires and buildings to stay warm
- Higher is always better!

**Status:** Ready to survive the cold! ❄️🔥
