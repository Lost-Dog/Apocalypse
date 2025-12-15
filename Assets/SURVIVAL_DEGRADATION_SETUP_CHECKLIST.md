# Survival Health Degradation - Setup Checklist

## ✅ Scripts Updated

**Modified Files:**
1. ✅ `/Assets/Scripts/PlayerInfectionDisplay.cs` - Added health damage at max infection
2. ✅ `/Assets/Scripts/SurvivalManager.cs` - Added cold & heat damage support
3. ✅ `/Assets/SURVIVAL_HEALTH_DEGRADATION_GUIDE.md` - Comprehensive documentation

---

## 🔧 Scene Configuration Required

You need to update the scene components to use the new features:

### **1. PlayerInfectionDisplay Component**

**Location:** `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection`

**Current Scene Values:**
```
playerHealth: None (null)
currentInfection: 0
maxInfection: 100
infectionGrowthRate: 0.5
infectionDecayRate: 1
autoFindReferences: true
```

**New Fields to Configure:**
```
Player Health: (auto-finds Player tag)
Enable Health Damage: ☑ true
Health Damage Per Second: 2
Damage Tick Interval: 1
```

**Steps:**
1. Select the GameObject in Hierarchy: `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection`
2. In Inspector, find **PlayerInfectionDisplay** component
3. The script will auto-detect these new fields
4. **Enable Health Damage** should be checked (enabled by default)
5. Leave defaults: **Health Damage Per Second = 2**, **Damage Tick Interval = 1**

---

### **2. SurvivalManager Component**

**Location:** `/GameSystems/SurvivalManager`

**Current Scene Values (OLD):**
```
criticalTemperatureThreshold: 0.2
healthDamagePerSecond: 0
enableColdDamage: true
```

**New Fields to Configure:**
```
Critical Cold Threshold: 0.2 (20%)
Critical Heat Threshold: 0.8 (80%)
Cold Damage Per Second: 2
Heat Damage Per Second: 2
Enable Cold Damage: ☑ true
Enable Heat Damage: ☑ true
Damage Tick Interval: 1
```

**Steps:**
1. Select the GameObject in Hierarchy: `/GameSystems/SurvivalManager`
2. In Inspector, find **SurvivalManager** component
3. The component will show the new fields automatically
4. **Update these values:**
   - **Critical Cold Threshold:** `0.2` (already set)
   - **Critical Heat Threshold:** `0.8` (NEW field)
   - **Cold Damage Per Second:** `2` (change from 0)
   - **Heat Damage Per Second:** `2` (NEW field)
   - **Enable Cold Damage:** ☑ (already enabled)
   - **Enable Heat Damage:** ☑ (NEW toggle - enable it)
   - **Damage Tick Interval:** `1` (already set)

**Note:** The old field `healthDamagePerSecond` has been replaced with:
- `coldDamagePerSecond`
- `heatDamagePerSecond`

---

## 🎮 Testing Instructions

### **Test 1: Infection Health Damage**

1. **Enter Play Mode**
2. **Select** `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection` in Hierarchy
3. **In Inspector**, set `Current Infection` to `100`
4. **Watch Player Health** - should decrease by 2 HP every second
5. **Check Console** - should see: `Infection damage: 2 HP (Infection: 100/100)`
6. **Reduce infection** to 99 or below - damage should stop

**Expected Result:**
```
Infection = 100% → Health decreases 2 HP/sec
Infection < 100% → No damage
```

---

### **Test 2: Critical Cold Damage**

1. **Enter Play Mode**
2. **Select** `/GameSystems/SurvivalManager` in Hierarchy
3. **In Inspector**, set `Current Temperature` to `20` or below
4. **Watch Player Health** - should decrease by 2 HP every second
5. **Enable Show Debug Info** - Console shows: `Cold damage: 2 HP (Temp: 20.0/100)`
6. **Increase temperature** above 20 - damage should stop

**Expected Result:**
```
Temperature ≤ 20% → Health decreases 2 HP/sec
Temperature > 20% → No damage
```

---

### **Test 3: Critical Heat Damage**

1. **Enter Play Mode**
2. **Select** `/GameSystems/SurvivalManager` in Hierarchy
3. **In Inspector**, set `Current Temperature` to `80` or above
4. **Watch Player Health** - should decrease by 2 HP every second
5. **Enable Show Debug Info** - Console shows: `Heat damage: 2 HP (Temp: 80.0/100)`
6. **Decrease temperature** below 80 - damage should stop

**Expected Result:**
```
Temperature ≥ 80% → Health decreases 2 HP/sec
Temperature < 80% → No damage
```

---

### **Test 4: Combined Damage (Hardcore)**

1. **Enter Play Mode**
2. **Set Infection** to `100`
3. **Set Temperature** to `15` (critical cold)
4. **Watch Player Health** - should decrease by **4 HP every second** (2 from infection + 2 from cold)
5. **Check Console** for both damage logs

**Expected Result:**
```
Infection damage: 2 HP (Infection: 100/100)
Cold damage: 2 HP (Temp: 15.0/100)
Total: 4 HP/sec
```

---

## ⚙️ Quick Settings Presets

### **Preset 1: Balanced (Recommended)**
```
PlayerInfectionDisplay:
├── Enable Health Damage: ☑
├── Health Damage Per Second: 2
└── Damage Tick Interval: 1

SurvivalManager:
├── Critical Cold Threshold: 0.2 (20%)
├── Critical Heat Threshold: 0.8 (80%)
├── Cold Damage Per Second: 2
├── Heat Damage Per Second: 2
├── Enable Cold Damage: ☑
└── Enable Heat Damage: ☑
```

### **Preset 2: Easy (Casual)**
```
PlayerInfectionDisplay:
├── Health Damage Per Second: 1
└── Infection Decay Rate: 2

SurvivalManager:
├── Critical Cold Threshold: 0.1 (10%)
├── Critical Heat Threshold: 0.9 (90%)
├── Cold Damage Per Second: 1
└── Heat Damage Per Second: 1
```

### **Preset 3: Hardcore**
```
PlayerInfectionDisplay:
├── Health Damage Per Second: 5
└── Infection Decay Rate: 0.5

SurvivalManager:
├── Critical Cold Threshold: 0.3 (30%)
├── Critical Heat Threshold: 0.7 (70%)
├── Cold Damage Per Second: 5
└── Heat Damage Per Second: 5
```

---

## 📋 Verification Checklist

Before testing, verify:

**PlayerInfectionDisplay Component:**
- ☐ Component exists on `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection`
- ☐ `Enable Health Damage` is checked
- ☐ `Health Damage Per Second` is set to 2 (or your preferred value)
- ☐ `Auto Find References` is checked

**SurvivalManager Component:**
- ☐ Component exists on `/GameSystems/SurvivalManager`
- ☐ `Critical Cold Threshold` is 0.2
- ☐ `Critical Heat Threshold` is 0.8 (NEW)
- ☐ `Cold Damage Per Second` is 2 (not 0)
- ☐ `Heat Damage Per Second` is 2 (NEW)
- ☐ `Enable Cold Damage` is checked
- ☐ `Enable Heat Damage` is checked (NEW)

**Testing:**
- ☐ Infection at 100% damages health
- ☐ Temperature ≤ 20% damages health (cold)
- ☐ Temperature ≥ 80% damages health (heat)
- ☐ Both can damage at same time
- ☐ Damage stops when conditions clear

---

## 🔍 Troubleshooting

### **Infection damage not working:**
- Check `Enable Health Damage` is true
- Verify `currentInfection` is exactly 100
- Check Console for "Could not find player health" warning
- Verify Player GameObject has `JUHealth` component

### **Temperature damage not working:**
- Check `Enable Cold Damage` or `Enable Heat Damage` is true
- Verify temperature is in critical range (≤20% or ≥80%)
- Check `Enable Temperature System` is true
- Enable `Show Debug Info` to see damage logs

### **No damage at all:**
- Check Player has `JUHealth` component with tag "Player"
- Verify player is alive (not already dead)
- Check `Damage Tick Interval` is not 0
- Look for errors in Console

---

## 🎯 Summary

**What Changed:**

1. **Infection System:**
   - New: Health degrades when infection reaches 100%
   - Rate: 2 HP per second (configurable)
   - Toggle: Can be enabled/disabled per component

2. **Temperature System:**
   - New: Separate cold and heat damage
   - Cold: Triggers at ≤20% temperature
   - Heat: Triggers at ≥80% temperature
   - Rate: 2 HP per second each (configurable)
   - Toggle: Each can be enabled/disabled independently

3. **Combined Effects:**
   - Both systems can damage simultaneously
   - Max damage: 4 HP/sec (infection + cold/heat)
   - All systems are independently toggleable

**Next Steps:**
1. Update the scene component values as listed above
2. Test each system individually
3. Adjust damage rates to your game's balance
4. Enable/disable features as needed
5. Add visual/audio feedback for critical states

---

**Status:** ✅ Scripts complete - Scene configuration required!
