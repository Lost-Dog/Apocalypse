# SurvivalManager Refactoring Guide

## Overview

The player stats system has been refactored so that **SurvivalManager** is now the single source of truth for all player survival stats (temperature, stamina, infection). PlayerStatsDisplay has been converted to a pure UI display component.

---

## What Changed

### ✅ **Before (Old System)**

```
PlayerStatsDisplay
├── Manages temperature, stamina, infection (simulation)
├── Updates UI displays
└── Handles safe zone interaction

SurvivalManager
├── Only manages temperature
└── Temperature-based damage

SafeZone
└── Calls PlayerStatsDisplay to restore stats
```

### ✅ **After (New System)**

```
SurvivalManager (Singleton)
├── Manages ALL player stats:
│   ├── Temperature (with normalization)
│   ├── Stamina (regen/drain)
│   └── Infection (decay/damage)
├── Handles safe zone awareness
├── Applies damage from cold/infection
└── References ProgressionManager & JUHealth

PlayerStatsDisplay (UI Only)
├── Reads from SurvivalManager
├── Displays health, XP, temperature, stamina, infection
└── No stat management logic

SafeZone
└── Calls SurvivalManager to restore stats
```

---

## Key Benefits

1. **Single Source of Truth** — All survival stats in one place
2. **Cleaner Architecture** — Separation of concerns (logic vs UI)
3. **Easier to Extend** — Add new stats in SurvivalManager only
4. **Singleton Access** — Access stats from anywhere via `SurvivalManager.Instance`
5. **Better Safe Zone Integration** — Centralized safe zone awareness

---

## Migration Steps

### 1. **Update SurvivalManager Component**

Add **SurvivalManager** to your scene (if not already present):

```
GameObject: GameManagers
└── SurvivalManager (component)
```

**Inspector Configuration:**

```yaml
Player Reference:
  ☑ Auto-find (finds Player tag)
  
Temperature Settings:
  Max Temperature: 100
  Current Temperature: 37
  Normal Temperature: 37
  Temperature Normalize Rate: 0.1
  Critical Cold Threshold: 0.2
  
Stamina Settings:
  Max Stamina: 100
  Current Stamina: 100
  Stamina Regen Rate: 5
  Stamina Drain Rate (Running): 10
  Stamina Drain Rate (Cold): 0.5
  
Infection Settings:
  Max Infection: 100
  Current Infection: 0
  Infection Decay Rate: 1
  Infection Damage Threshold: 50
  Infection Damage Per Second: 1
  
System Toggles:
  ☑ Enable Temperature System
  ☐ Enable Temperature Decrease
  ☑ Enable Cold Damage
  ☑ Enable Stamina System
  ☑ Enable Infection System
  ☐ Show Debug Info
  
Safe Zone Interaction:
  ☐ Is In Safe Zone
  ☑ Pause Temperature Normalization In Safe Zone
```

---

### 2. **Update PlayerStatsDisplay Component**

PlayerStatsDisplay now **only displays** stats, no longer manages them.

**Old Inspector Fields (REMOVED):**
- ❌ Current Temperature
- ❌ Current Stamina
- ❌ Current Infection
- ❌ Normal Temperature
- ❌ Max Stamina
- ❌ Stamina Regen Rate
- ❌ Infection Settings
- ❌ Safe Zone Interaction

**New Inspector Fields:**
- ✅ SurvivalManager (reference)
- ✅ ProgressionManager (reference)
- ✅ UI elements (same as before)
- ✅ Display settings (show prefix toggles)

---

### 3. **Update Code References**

If you have custom scripts calling PlayerStatsDisplay, update them:

#### **Old Code:**
```csharp
PlayerStatsDisplay stats = FindFirstObjectByType<PlayerStatsDisplay>();
stats.ModifyTemperature(-10f);
stats.AddInfection(5f);
stats.DrainStamina(20f);
```

#### **New Code:**
```csharp
SurvivalManager survival = SurvivalManager.Instance;
survival.ModifyTemperature(-10f);
survival.AddInfection(5f);
survival.DrainStamina(20f);
```

---

## API Reference

### SurvivalManager Public API

#### **Temperature**
```csharp
// Get/Set
float currentTemperature
float maxTemperature
float normalTemperature

// Methods
void SetTemperature(float value)
void ModifyTemperature(float delta)
void WarmUp(float amount)
void CoolDown(float amount)
void ResetTemperature()
string GetTemperatureStatus()  // Returns: "Hypothermia", "Cold", "Normal", etc.

// Properties
float TemperaturePercentage { get; }
bool IsCriticalCold { get; }
```

#### **Stamina**
```csharp
// Get/Set
float currentStamina
float maxStamina
float staminaRegenRate
float staminaDrainRateRunning

// Methods
void SetStamina(float value)
void ModifyStamina(float delta)
void DrainStamina(float amount)
void ResetStamina()

// Properties
float StaminaPercentage { get; }
```

#### **Infection**
```csharp
// Get/Set
float currentInfection
float maxInfection
float infectionDecayRate
float infectionDamageThreshold

// Methods
void SetInfection(float value)
void AddInfection(float amount)
void CureInfection(float amount)
void ResetInfection()
string GetInfectionStatus()  // Returns: "None", "Mild", "Moderate", "Severe", "Critical"

// Properties
float InfectionPercentage { get; }
```

#### **Environment Modifiers**
```csharp
void SetIndoors(bool value)
void SetNearFire(bool value)
void SetInColdZone(bool value)
void SetInSafeZone(bool value)
```

#### **Utility**
```csharp
void ResetAllStats()  // Resets health, temperature, stamina, infection
```

#### **Singleton Access**
```csharp
SurvivalManager.Instance  // Access from anywhere
```

---

## Events System

SurvivalManager provides UnityEvents for stat changes:

```csharp
// Temperature
onTemperatureChanged(float newValue)
onEnteredCriticalTemperature()
onExitedCriticalTemperature()
onPlayerFroze()

// Stamina
onStaminaChanged(float newValue)
onStaminaDepleted()

// Infection
onInfectionChanged(float newValue)
onInfectionCritical()
```

**Usage Example:**
```csharp
void Start()
{
    SurvivalManager.Instance.onTemperatureChanged.AddListener(OnTemperatureChanged);
}

void OnTemperatureChanged(float newTemp)
{
    Debug.Log($"Temperature changed: {newTemp}°C");
}
```

---

## Safe Zone Integration

SafeZone now uses SurvivalManager:

```csharp
// SafeZone automatically:
private void OnPlayerEnterZone()
{
    survivalManager.SetInSafeZone(true);  // Pauses auto-normalization
    // ... restore stats
}

private void OnPlayerExitZone()
{
    survivalManager.SetInSafeZone(false);  // Resumes auto-normalization
}
```

---

## Testing

### Test in Play Mode

```csharp
// In Console:

// 1. Test Temperature
SurvivalManager.Instance.ModifyTemperature(-15)  // Make player cold
SurvivalManager.Instance.GetTemperatureStatus()  // Check status

// 2. Test Stamina
SurvivalManager.Instance.DrainStamina(50)  // Drain stamina
SurvivalManager.Instance.currentStamina  // Check value

// 3. Test Infection
SurvivalManager.Instance.AddInfection(60)  // Add infection
SurvivalManager.Instance.GetInfectionStatus()  // Check status

// 4. Test Safe Zone
SurvivalManager.Instance.SetInSafeZone(true)  // Enter safe zone
// Temperature auto-normalization pauses

// 5. Reset All
SurvivalManager.Instance.ResetAllStats()  // Reset everything
```

---

## Common Scenarios

### Scenario 1: Enemy Damages Player & Adds Infection
```csharp
public class Enemy : MonoBehaviour
{
    public float attackDamage = 10f;
    public float infectionAmount = 15f;
    
    void AttackPlayer()
    {
        // Damage health
        JUHealth health = player.GetComponent<JUHealth>();
        health.DoDamage(attackDamage);
        
        // Add infection
        SurvivalManager.Instance.AddInfection(infectionAmount);
    }
}
```

### Scenario 2: Consumable Item Restores Stats
```csharp
public class HealthPack : MonoBehaviour
{
    public float healthRestore = 50f;
    public float temperatureRestore = 5f;
    public float infectionCure = 20f;
    
    void Use()
    {
        // Restore health
        JUHealth health = player.GetComponent<JUHealth>();
        health.Health += healthRestore;
        
        // Warm up & cure infection
        SurvivalManager.Instance.WarmUp(temperatureRestore);
        SurvivalManager.Instance.CureInfection(infectionCure);
    }
}
```

### Scenario 3: Cold Environment Zone
```csharp
public class ColdZone : MonoBehaviour
{
    public float temperatureDrain = 2f;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager.Instance.SetInColdZone(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager.Instance.SetInColdZone(false);
        }
    }
}
```

### Scenario 4: Campfire Warms Player
```csharp
public class Campfire : MonoBehaviour
{
    public float warmthRadius = 5f;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager.Instance.SetNearFire(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager.Instance.SetNearFire(false);
        }
    }
}
```

---

## Troubleshooting

### Issue: "SurvivalManager.Instance is null"

**Solution:**
- Ensure SurvivalManager component exists in the scene
- SurvivalManager creates singleton in `Awake()`
- Access it after scene loads (not in Awake of other scripts)

### Issue: "Stats not updating in UI"

**Solution:**
- Check PlayerStatsDisplay has reference to SurvivalManager
- Enable "Auto Find References" in PlayerStatsDisplay
- Verify UI elements are assigned (Text/Slider fields)

### Issue: "Temperature not recovering in safe zones"

**Solution:**
- Ensure SafeZone calls `survivalManager.SetInSafeZone(true)` on enter
- Check "Pause Temperature Normalization In Safe Zone" is enabled
- SafeZone must have reference to SurvivalManager

### Issue: "Player not taking cold damage"

**Solution:**
- Enable "Enable Cold Damage" in SurvivalManager
- Enable "Enable Temperature Decrease" to make temperature drop
- Set "Critical Cold Threshold" (default: 0.2 = 20%)

---

## Files Modified

### ✅ Updated Files
1. `/Assets/Scripts/SurvivalManager.cs` — Expanded to handle all stats
2. `/Assets/Scripts/PlayerStatsDisplay.cs` — Converted to UI-only display
3. `/Assets/Scripts/SafeZone.cs` — Updated to use SurvivalManager

### 📝 Documentation Files
- `/Assets/Scripts/SURVIVAL_MANAGER_REFACTORING_GUIDE.md` — This file

---

## Summary

**SurvivalManager** is now your central hub for all player survival mechanics:

```
SurvivalManager.Instance
├── .currentTemperature  → Player's temperature
├── .currentStamina      → Player's stamina
├── .currentInfection    → Player's infection
├── .playerHealth        → Reference to JUHealth
└── .progressionManager  → Reference to ProgressionManager
```

All game systems should now interact with **SurvivalManager** instead of PlayerStatsDisplay for stat management.

PlayerStatsDisplay is now purely responsible for **displaying** the stats from SurvivalManager in the UI.

---

**Refactoring Complete! ✅**

Your survival stats are now centrally managed by SurvivalManager with clean separation between logic and UI display.
