# Survival System Setup Guide

## Overview
Complete survival and stats display system with individual display components for each stat.

---

## Created Scripts

### **Core Systems:**
1. **SurvivalManager.cs** - Temperature survival system

### **Display Scripts:**
2. **PlayerHealthDisplay.cs** - Shows health
3. **PlayerXPDisplay.cs** - Shows experience
4. **PlayerLevelDisplay.cs** - Shows level
5. **PlayerTemperatureDisplay.cs** - Shows temperature
6. **PlayerStaminaDisplay.cs** - Shows stamina
7. **PlayerInfectionDisplay.cs** - Shows infection

---

## Quick Setup

### **Step 1: Add SurvivalManager to Scene**
1. Select `/GameSystems` in Hierarchy
2. Right-click → Create Empty Child
3. Rename to `SurvivalManager`
4. Add Component → **SurvivalManager**
5. Configure settings (see below)

### **Step 2: Add Display Scripts to UI**
For each stat UI element, add the corresponding script:

**Health:**
- Select: `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Health/Health`
- Add Component: **PlayerHealthDisplay**

**XP:**
- Select your XP text element
- Add Component: **PlayerXPDisplay**

**Level:**
- Select: `/UI/HUD/ScreenSpace/Bottom/PlayerStats/PlayerStat_Level/Level`
- Add Component: **PlayerLevelDisplay**

**Temperature:**
- Select your Temperature text element
- Add Component: **PlayerTemperatureDisplay**

**Stamina:**
- Select your Stamina text element
- Add Component: **PlayerStaminaDisplay**

**Infection:**
- Select: `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection`
- Add Component: **PlayerInfectionDisplay**

### **Step 3: Test**
- Enter Play Mode ▶️
- All stats should auto-populate
- Temperature decreases if enabled
- Stamina drains when running
- Infection naturally decays

---

## SurvivalManager Configuration

### **Temperature Settings:**
```
Max Temperature: 100
Current Temperature: 100
Temperature Decrease Rate: 0.5
Critical Temperature Threshold: 0.2 (20%)
```

### **Health & Stamina Effects:**
```
Health Damage Per Second: 0 (when critical)
Stamina Drain Per Second: 0.5 (when cold)
Damage Tick Interval: 1 second
```

### **Temperature Modifiers:**
```
Indoor Temperature Gain: 5/sec
Fire Temperature Gain: 10/sec
Cold Zone Multiplier: 2x decrease
```

### **System Toggles:**
```
☑ Enable Temperature System
☐ Enable Temperature Decrease (turn on for active gameplay)
☐ Enable Cold Damage (turn on to damage player when cold)
☐ Show Debug Info (turn on for testing)
```

---

## Display Script Configuration

All display scripts have **Auto Find References** enabled by default:
- Automatically find player
- Automatically find managers
- Automatically find TextMeshProUGUI component
- No manual setup required!

### **Common Settings:**

**Show Prefix:** `☐`
- Disabled: `100/100`
- Enabled: `HP: 100/100`

**Show Status:** (Temperature & Infection) `☑`
- Displays status text like (Warm), (Critical), etc.

**Show Fraction:** (Health, XP, Stamina) `☑`
- Shows current/max instead of just current

**Show As Percentage:** (XP, Health, Temperature) `☐`
- Shows as percentage instead of raw values

---

## System Features

### **Temperature System:**

**States:**
- **Warm** (80-100%) - Normal gameplay
- **Normal** (60-80%) - Optimal temperature
- **Cool** (40-60%) - Starting to get cold
- **Cold** (20-40%) - Need warmth
- **Critical** (<20%) - Takes damage if enabled

**Mechanics:**
- Decreases over time (if enabled)
- Increases when indoors
- Increases near fire
- Decreases faster in cold zones
- Can trigger damage when critical

**Events:**
- `OnTemperatureChanged` - Fires on any change
- `OnEnteredCriticalTemperature` - When going critical
- `OnExitedCriticalTemperature` - When recovering
- `OnPlayerFroze` - When temperature hits 0

### **Stamina System:**

**Mechanics:**
- Drains when running/sprinting
- Regenerates when idle/walking
- Real-time integration with JUCharacterController

**Configuration:**
```
Max Stamina: 100
Stamina Regen Rate: 10/sec
Stamina Drain Rate: 20/sec
```

### **Infection System:**

**States:**
- **None** (0%) - Healthy
- **Mild** (1-24%) - Minor symptoms
- **Moderate** (25-49%) - Noticeable effects
- **Severe** (50-74%) - Dangerous
- **Critical** (75-100%) - Life-threatening

**Mechanics:**
- Naturally decays over time
- Can be added by enemy attacks
- Can be cured with medicine

**Configuration:**
```
Max Infection: 100%
Infection Growth Rate: 0.5%/sec (when infected)
Infection Decay Rate: 1%/sec (natural recovery)
```

---

## Using the Systems in Code

### **Temperature:**

```csharp
SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();

// Modify temperature
survival.ModifyTemperature(-10f); // Cool down by 10
survival.WarmUp(20f);             // Warm up by 20
survival.CoolDown(15f);           // Cool down by 15
survival.ResetTemperature();      // Reset to max

// Set environmental states
survival.SetIndoors(true);        // Player entered building
survival.SetNearFire(true);       // Player near campfire
survival.SetInColdZone(true);     // Player in cold area

// Check state
if (survival.IsCritical)
{
    Debug.Log("Player is critically cold!");
}

float percentage = survival.TemperaturePercentage; // 0.0 to 1.0
```

### **Stamina:**

```csharp
PlayerStaminaDisplay stamina = FindFirstObjectByType<PlayerStaminaDisplay>();

// Check stamina
if (stamina.HasStamina(25f))
{
    // Perform special action
    stamina.DrainStamina(25f);
}

// Restore stamina
stamina.RestoreStamina(50f); // Energy drink
```

### **Infection:**

```csharp
PlayerInfectionDisplay infection = FindFirstObjectByType<PlayerInfectionDisplay>();

// Add infection
infection.AddInfection(15f);  // Zombie bite +15%
infection.AddInfection(30f);  // Infected wound +30%

// Remove infection
infection.RemoveInfection(50f); // Medicine -50%
infection.CureInfection();       // Full cure

// Check infection
if (infection.IsInfected())
{
    float severity = infection.GetInfectionPercentage();
    Debug.Log($"Infected! Severity: {severity * 100}%");
}
```

---

## Environmental Triggers

### **Indoor Zone (Temperature Gain):**

```csharp
using UnityEngine;

public class IndoorZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetIndoors(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetIndoors(false);
            }
        }
    }
}
```

### **Fire Zone (Temperature Gain):**

```csharp
using UnityEngine;

public class FireZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetNearFire(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetNearFire(false);
            }
        }
    }
}
```

### **Cold Zone (Faster Temperature Loss):**

```csharp
using UnityEngine;

public class ColdZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetInColdZone(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetInColdZone(false);
            }
        }
    }
}
```

---

## Integration Examples

### **Enemy Attack Adds Infection:**

```csharp
// In your enemy attack script:
PlayerInfectionDisplay infection = target.GetComponent<PlayerInfectionDisplay>();
if (infection != null)
{
    infection.AddInfection(10f); // +10% infection per zombie hit
}
```

### **Medicine Item Cures Infection:**

```csharp
// In your item use script:
PlayerInfectionDisplay infection = FindFirstObjectByType<PlayerInfectionDisplay>();
if (infection != null)
{
    infection.RemoveInfection(50f); // Medicine removes 50% infection
}
```

### **Energy Drink Restores Stamina:**

```csharp
// In your consumable item script:
PlayerStaminaDisplay stamina = FindFirstObjectByType<PlayerStaminaDisplay>();
if (stamina != null)
{
    stamina.RestoreStamina(50f); // +50 stamina
}
```

### **Campfire Warms Player:**

```csharp
// On campfire collider:
public class Campfire : MonoBehaviour
{
    public float warmthRadius = 5f;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
            if (survival != null)
            {
                survival.SetNearFire(true);
            }
        }
    }
}
```

---

## Testing Checklist

### **Temperature System:**
- ☐ SurvivalManager added to `/GameSystems`
- ☐ Temperature display shows current value
- ☐ Enable Temperature Decrease → temperature drops over time
- ☐ Create indoor zone → temperature increases indoors
- ☐ Create fire zone → temperature increases near fire
- ☐ Enable Cold Damage → player takes damage when critical
- ☐ Temperature reaches 0 → OnPlayerFroze event fires

### **Stamina System:**
- ☐ PlayerStaminaDisplay added to UI
- ☐ Stamina display shows 100/100
- ☐ Sprint/run → stamina drains
- ☐ Stop running → stamina regenerates
- ☐ Stamina reaches 0 → player exhausted

### **Infection System:**
- ☐ PlayerInfectionDisplay added to UI
- ☐ Infection display shows 0% (None)
- ☐ Call AddInfection(50) → infection increases
- ☐ Wait → infection naturally decays
- ☐ Call CureInfection() → infection goes to 0%

### **Display Scripts:**
- ☐ Health shows current/max HP
- ☐ XP shows current/required
- ☐ Level shows current level
- ☐ All displays auto-find references
- ☐ All displays update in real-time

---

## Troubleshooting

**Temperature doesn't change:**
- Check "Enable Temperature System" is checked
- Check "Enable Temperature Decrease" is checked
- Verify SurvivalManager is in scene

**Stamina doesn't drain:**
- Verify player has JUCharacterController
- Check that player is actually running
- Adjust Stamina Drain Rate

**Infection doesn't show:**
- PlayerInfectionDisplay added to correct UI element
- Check Auto Find References is enabled
- Manually call AddInfection(25) to test

**Displays show "0" or empty:**
- Check Auto Find References is enabled
- Verify managers are in scene
- Check player has "Player" tag

---

## Performance Notes

- All display scripts update every frame
- Consider reducing update frequency for mobile
- Temperature system uses minimal calculations
- Stamina/Infection are lightweight simulations

---

**Created:**
- SurvivalManager.cs
- PlayerHealthDisplay.cs
- PlayerXPDisplay.cs
- PlayerLevelDisplay.cs
- PlayerTemperatureDisplay.cs
- PlayerStaminaDisplay.cs
- PlayerInfectionDisplay.cs

**Location:** `/Assets/Scripts/`  
**Documentation:** `/Assets/SURVIVAL_SYSTEM_SETUP.md`
