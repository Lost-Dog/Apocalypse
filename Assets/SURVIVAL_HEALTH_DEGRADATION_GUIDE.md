# Survival Health Degradation System

## ✅ New Features Implemented

**Health degradation now occurs when:**
1. **Infection reaches maximum** (100%)
2. **Temperature becomes critical** (too cold OR too hot)

---

## Infection Health Degradation

### **How It Works:**

**When Infection = 100% (Maximum):**
```
Continuous Health Damage:
├── Damage Rate: 2 HP per second (default)
├── Tick Interval: Every 1 second
├── Total Damage: 2 HP per tick
└── Continues until infection decreases below max
```

### **Configuration:**

**PlayerInfectionDisplay Component:**
```
Health Damage Settings:
├── Enable Health Damage: ☑ (true)
├── Health Damage Per Second: 2
├── Damage Tick Interval: 1
```

### **Infection Thresholds:**

```
0%       No infection (None)
1-24%    Mild infection (no damage)
25-49%   Moderate infection (no damage)
50-74%   Severe infection (no damage)
75-99%   Critical infection (no damage yet)
100%     MAXIMUM ← Health starts degrading! ❌
```

### **Death Scenario:**

```
Infection reaches 100%
    ↓
Health starts degrading (2 HP/sec)
    ↓
Player has 100 HP
    ↓
Death in: 100 / 2 = 50 seconds
    ↓
Unless infection is cured or decreases
```

---

## Temperature Health Degradation

### **How It Works:**

**Critical Cold (Temperature ≤ 20%):**
```
Cold Damage:
├── Damage Rate: 2 HP per second (default)
├── Tick Interval: Every 1 second
├── Total Damage: 2 HP per tick
└── Continues while temperature stays critical
```

**Critical Heat (Temperature ≥ 80%):**
```
Heat Damage:
├── Damage Rate: 2 HP per second (default)
├── Tick Interval: Every 1 second
├── Total Damage: 2 HP per tick
└── Continues while temperature stays critical
```

### **Configuration:**

**SurvivalManager Component:**
```
Temperature Settings:
├── Max Temperature: 100
├── Current Temperature: 100
├── Critical Cold Threshold: 0.2 (20%)
├── Critical Heat Threshold: 0.8 (80%)

Health & Stamina Effects:
├── Cold Damage Per Second: 2
├── Heat Damage Per Second: 2
├── Damage Tick Interval: 1

System Toggles:
├── Enable Temperature System: ☑
├── Enable Cold Damage: ☑
├── Enable Heat Damage: ☑
```

### **Temperature Zones:**

```
Temperature Scale (0-100):

100%  ═══════════════════════  Max (Normal)
 90%  ═══════════════════════  Warm
 80%  ═══════════════════════  ← Critical Heat Threshold
 70%  ═══════════════════════  Hot
 60%  ═══════════════════════  Normal
 50%  ═══════════════════════  Normal
 40%  ═══════════════════════  Cool
 30%  ═══════════════════════  Cold
 20%  ═══════════════════════  ← Critical Cold Threshold
 10%  ═══════════════════════  Freezing ❄️
  0%  ═══════════════════════  Death (player froze)

Damage Zones:
├── 81-100% = Heat damage (if enabled)
├── 21-79%  = Safe zone (no damage)
└── 0-20%   = Cold damage (if enabled)
```

---

## Combined Health Threats

### **Multiple Hazards:**

```
Scenario: Player has max infection AND critical cold
    ↓
Infection damage: 2 HP/sec
Temperature damage: 2 HP/sec
    ↓
TOTAL: 4 HP/sec damage!
    ↓
Death in: 100 / 4 = 25 seconds
```

### **Priority Order:**

```
1. Infection at 100% → Take damage
2. Temperature critical → Take damage
3. Both active → Take BOTH damages
4. Neither active → No survival damage
```

---

## Damage Rate Examples

### **Infection Damage:**

**Conservative (Default):**
```
Damage: 2 HP/sec
Player Health: 100 HP
Time to Death: 50 seconds
```

**Moderate:**
```
Damage: 5 HP/sec
Player Health: 100 HP
Time to Death: 20 seconds
```

**Aggressive:**
```
Damage: 10 HP/sec
Player Health: 100 HP
Time to Death: 10 seconds
```

### **Temperature Damage:**

**Cold Settings:**
```
Critical Threshold: 20%
Cold Damage: 2 HP/sec
Time to Death: 50 seconds (at 100 HP)
```

**Heat Settings:**
```
Critical Threshold: 80%
Heat Damage: 2 HP/sec
Time to Death: 50 seconds (at 100 HP)
```

**Hardcore Survival:**
```
Cold Threshold: 30%
Heat Threshold: 70%
Cold Damage: 5 HP/sec
Heat Damage: 5 HP/sec
Time to Death: 20 seconds (much more punishing!)
```

---

## Balancing Guidelines

### **Infection Damage Rates:**

**Easy (Forgiving):**
```
Damage: 1-2 HP/sec
Decay Rate: 2-3/sec (faster cure)
Max Infection: 100
Result: ~50-100 seconds to death
```

**Balanced (Recommended):**
```
Damage: 2-3 HP/sec
Decay Rate: 1/sec (natural cure)
Max Infection: 100
Result: ~33-50 seconds to death
```

**Hardcore:**
```
Damage: 5-10 HP/sec
Decay Rate: 0.5/sec (slow cure)
Max Infection: 100
Result: ~10-20 seconds to death
```

### **Temperature Damage Rates:**

**Exploration Game:**
```
Cold Threshold: 10% (very forgiving)
Heat Threshold: 90% (very forgiving)
Damage: 1 HP/sec
```

**Survival Game (Recommended):**
```
Cold Threshold: 20%
Heat Threshold: 80%
Damage: 2-3 HP/sec
```

**Hardcore Survival:**
```
Cold Threshold: 30%
Heat Threshold: 70%
Damage: 5-10 HP/sec
```

---

## Integration with Gameplay

### **Infection Sources:**

```csharp
PlayerInfectionDisplay infection = FindFirstObjectByType<PlayerInfectionDisplay>();

// Enemy attack causes infection
infection.AddInfection(25f); // +25% infection

// Environmental hazard
infection.AddInfection(10f); // +10% infection

// Infected area over time
void Update() {
    if (playerInInfectedZone) {
        infection.AddInfection(5f * Time.deltaTime); // +5% per second
    }
}
```

### **Infection Cures:**

```csharp
PlayerInfectionDisplay infection = FindFirstObjectByType<PlayerInfectionDisplay>();

// Medical item removes infection
infection.RemoveInfection(50f); // -50% infection

// Full cure
infection.CureInfection(); // Set to 0%

// Medkit example
public void UseMedkit() {
    playerHealth.Heal(50f);
    infection.RemoveInfection(30f); // Also reduces infection
}
```

### **Temperature Management:**

```csharp
SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();

// Enter building
survival.SetIndoors(true); // Slowly warms up

// Near campfire
survival.SetNearFire(true); // Warms up faster

// Enter snow area
survival.SetInColdZone(true); // Cools down faster

// Instant warm-up (hot drink)
survival.WarmUp(20f); // +20 temperature

// Instant cool-down (cold water)
survival.CoolDown(15f); // -15 temperature
```

---

## Visual Feedback

### **Current Status Display:**

**Infection:**
```
Text: "75% (Severe)"
Color: Yellow/Orange/Red based on severity
Slider: Visual bar showing 75%
```

**Temperature:**
```
Text: "15.0°C (Critical)"
Color: Blue (cold) or Red (heat)
Slider: Visual bar showing 15%
```

### **Recommended Enhancements:**

**Low Health Warning (from infection):**
```csharp
if (infection.currentInfection >= 100f) {
    // Show warning icon
    // Play heartbeat sound
    // Screen vignette effect
}
```

**Critical Temperature Warning:**
```csharp
if (survival.IsCriticalCold) {
    // Blue screen tint
    // Frost on screen edges
    // Shivering animation
}

if (survival.IsCriticalHeat) {
    // Red screen tint
    // Heat wave effect
    // Sweat particles
}
```

---

## Player Survival Strategies

### **Managing Infection:**

**Prevention:**
- Avoid infected enemies
- Stay out of contaminated areas
- Use protective gear/perks

**Treatment:**
- Use medkits regularly
- Find antibiotics/medicine
- Natural decay over time (1%/sec default)

**Emergency:**
- If infection hits 100%, use medkit immediately
- Seek shelter to recover health
- Avoid combat while infected

### **Managing Temperature:**

**Stay Warm (Cold Areas):**
- Enter buildings (+5 temp/sec)
- Stand near fires (+10 temp/sec)
- Drink hot beverages (+instant boost)
- Wear warm clothing (future feature)

**Stay Cool (Hot Areas):**
- Find shade/AC
- Drink cold water (+instant cool)
- Avoid fires
- Wear light clothing (future feature)

**Emergency:**
- If critical cold: Find fire/building immediately
- If critical heat: Find shade/water immediately
- Heal up before venturing into extreme areas

---

## Configuration Examples

### **Standard Survival Game:**

**PlayerInfectionDisplay:**
```
Enable Health Damage: ☑
Health Damage Per Second: 2
Damage Tick Interval: 1
Infection Decay Rate: 1
```

**SurvivalManager:**
```
Enable Cold Damage: ☑
Enable Heat Damage: ☑
Cold Damage Per Second: 2
Heat Damage Per Second: 2
Critical Cold Threshold: 0.2 (20%)
Critical Heat Threshold: 0.8 (80%)
```

### **Hardcore Mode:**

**PlayerInfectionDisplay:**
```
Health Damage Per Second: 5
Infection Decay Rate: 0.5 (slower cure)
```

**SurvivalManager:**
```
Cold Damage Per Second: 5
Heat Damage Per Second: 5
Critical Cold Threshold: 0.3 (30%)
Critical Heat Threshold: 0.7 (70%)
Temperature Decrease Rate: 1 (faster cooling)
```

### **Easy/Casual Mode:**

**PlayerInfectionDisplay:**
```
Health Damage Per Second: 1
Infection Decay Rate: 2 (faster cure)
```

**SurvivalManager:**
```
Cold Damage Per Second: 1
Heat Damage Per Second: 1
Critical Cold Threshold: 0.1 (10%)
Critical Heat Threshold: 0.9 (90%)
```

---

## Testing Checklist

### **Infection System:**
- ☐ Add infection to player (using inspector or item)
- ☐ Watch infection naturally decay over time
- ☐ Increase infection to 100%
- ☐ Verify health starts decreasing (2 HP/sec)
- ☐ Use cure item to reduce infection
- ☐ Verify health damage stops when infection < 100%

### **Temperature System:**
- ☐ Decrease temperature to 20% or below
- ☐ Verify "Critical Cold" status appears
- ☐ Verify health starts decreasing (2 HP/sec)
- ☐ Warm up (fire/indoors) above 20%
- ☐ Verify health damage stops
- ☐ Increase temperature to 80% or above
- ☐ Verify "Critical Heat" status appears
- ☐ Verify health starts decreasing (2 HP/sec)

### **Combined Systems:**
- ☐ Set infection to 100% AND temperature to 15%
- ☐ Verify taking damage from BOTH sources
- ☐ Check damage rate is cumulative (4 HP/sec total)

---

## Debug Console Logging

### **Infection Logging:**

When infection = 100% and damage occurs:
```
Infection damage: 2 HP (Infection: 100/100)
```

### **Temperature Logging:**

Enable `Show Debug Info` in SurvivalManager:
```
Temperature: 18.0/100 (18%)
Cold damage: 2 HP (Temp: 18.0/100)
```

Or:
```
Temperature: 85.0/100 (85%)
Heat damage: 2 HP (Temp: 85.0/100)
```

---

## Script References

### **PlayerInfectionDisplay.cs:**

**Key Methods:**
```csharp
AddInfection(float amount)          // Increase infection
RemoveInfection(float amount)       // Decrease infection
CureInfection()                     // Set to 0%
IsInfected()                        // Check if infected
GetInfectionPercentage()            // Get 0-1 value
```

**Key Properties:**
```csharp
currentInfection                    // Current infection level (0-100)
maxInfection                        // Maximum infection (default 100)
enableHealthDamage                  // Toggle damage on/off
healthDamagePerSecond              // Damage rate at max infection
```

### **SurvivalManager.cs:**

**Key Methods:**
```csharp
SetTemperature(float value)         // Set exact temperature
ModifyTemperature(float delta)      // Adjust by amount
WarmUp(float amount)                // Increase temperature
CoolDown(float amount)              // Decrease temperature
SetIndoors(bool value)              // Toggle indoor heating
SetNearFire(bool value)             // Toggle fire heating
```

**Key Properties:**
```csharp
currentTemperature                  // Current temp (0-100)
IsCriticalCold                      // Is critically cold?
IsCriticalHeat                      // Is critically hot?
coldDamagePerSecond                 // Cold damage rate
heatDamagePerSecond                 // Heat damage rate
criticalColdThreshold               // Cold threshold (0-1)
criticalHeatThreshold               // Heat threshold (0-1)
```

---

## Files Modified

**Modified:**
1. `/Assets/Scripts/PlayerInfectionDisplay.cs`
   - Added health damage when infection = 100%
   - Added JUHealth reference
   - Added damage tick system
   - Added debug logging

2. `/Assets/Scripts/SurvivalManager.cs`
   - Separated cold and heat damage
   - Added critical heat threshold
   - Updated damage application
   - Added debug logging for damage type

**Created:**
3. `/Assets/SURVIVAL_HEALTH_DEGRADATION_GUIDE.md`

---

## Quick Reference

### **Death Times (100 HP):**

**Infection Only:**
```
2 HP/sec = 50 seconds
3 HP/sec = 33 seconds
5 HP/sec = 20 seconds
10 HP/sec = 10 seconds
```

**Temperature Only:**
```
2 HP/sec = 50 seconds
3 HP/sec = 33 seconds
5 HP/sec = 20 seconds
```

**Combined (Infection + Cold/Heat):**
```
4 HP/sec = 25 seconds
6 HP/sec = 16.7 seconds
10 HP/sec = 10 seconds
```

---

**Status:** ✅ COMPLETE

**Features:**
- ✅ Infection at 100% degrades health
- ✅ Critical cold (≤20%) degrades health
- ✅ Critical heat (≥80%) degrades health
- ✅ Configurable damage rates
- ✅ Debug logging for testing
- ✅ Fully toggleable systems

**Default Settings:**
- Infection damage: 2 HP/sec at 100% infection
- Cold damage: 2 HP/sec below 20% temperature
- Heat damage: 2 HP/sec above 80% temperature
- Damage tick: Every 1 second

**Result:** Survival systems now pose real threats to player health! 💀🌡️🦠
