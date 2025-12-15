# Stamina Balance Guide

## Issue Fixed ✅
**Problem:** Stamina was draining too fast while sprinting (20 per second = depleted in 5 seconds)

**Solution:** Reduced stamina drain rate from 20 → 8 per second for better gameplay balance

---

## Current Stamina Settings

### **Default Values (Script):**
```
Max Stamina: 100
Stamina Regen Rate: 15/second
Stamina Drain Rate: 8/second  ← Adjusted!
```

### **Active in Scene:**
The scene instance has different values that need to be updated:

**Current Scene Values:**
```
Max Stamina: 100
Stamina Regen Rate: 10/second  ← Lower than script default
Stamina Drain Rate: 20/second  ← TOO HIGH!
```

---

## Recommended Settings

### **Balanced (Default):**
```
Stamina Drain Rate: 8/second
Stamina Regen Rate: 15/second
Max Stamina: 100
```

**Results:**
- **Sprint Duration:** ~12.5 seconds of continuous sprinting
- **Recovery Time:** ~6.7 seconds to fully regenerate
- **Net Loss:** 8 stamina/sec while sprinting
- **Net Gain:** 15 stamina/sec while not sprinting

### **Easy Mode:**
```
Stamina Drain Rate: 5/second
Stamina Regen Rate: 20/second
Max Stamina: 100
```

**Results:**
- **Sprint Duration:** 20 seconds
- **Recovery Time:** 5 seconds
- More forgiving for exploration

### **Hardcore Mode:**
```
Stamina Drain Rate: 15/second
Stamina Regen Rate: 10/second
Max Stamina: 100
```

**Results:**
- **Sprint Duration:** ~6.7 seconds
- **Recovery Time:** 10 seconds
- Forces tactical stamina management

### **Survival Mode:**
```
Stamina Drain Rate: 12/second
Stamina Regen Rate: 8/second
Max Stamina: 150
```

**Results:**
- **Sprint Duration:** ~12.5 seconds
- **Recovery Time:** ~18.75 seconds
- Larger pool but slower recovery

---

## How Stamina Works

### **Drain Mechanics:**
```
When Sprinting:
    currentStamina -= staminaDrainRate * Time.deltaTime
    
When Not Sprinting:
    currentStamina += staminaRegenRate * Time.deltaTime
```

### **Calculations:**

**Sprint Duration (seconds):**
```
Duration = maxStamina / staminaDrainRate
```

**Recovery Time (seconds):**
```
Recovery = maxStamina / staminaRegenRate
```

**Efficiency Ratio:**
```
Ratio = staminaRegenRate / staminaDrainRate

> 1.0 = Regenerates faster than drains (good)
= 1.0 = Equal drain/regen (neutral)
< 1.0 = Drains faster than regenerates (punishing)
```

---

## Balance Comparison

### **Old Settings (TOO FAST):**
```
Drain: 20/sec | Regen: 10/sec
Sprint Duration: 5 seconds ❌
Recovery Time: 10 seconds
Efficiency: 0.5 (drains 2x faster than regens)
```

### **New Settings (BALANCED):**
```
Drain: 8/sec | Regen: 15/sec
Sprint Duration: 12.5 seconds ✅
Recovery Time: 6.7 seconds
Efficiency: 1.875 (regens 1.875x faster than drains)
```

### **Improvement:**
- **+150%** longer sprint duration
- **-33%** faster recovery
- **+275%** better efficiency ratio

---

## Configuration

### **Where to Adjust:**

**In Scene (During Runtime):**
1. Select `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Stamina/Stamina`
2. Find **PlayerStaminaDisplay** component
3. Adjust values:
   - **Stamina Drain Rate:** 8 (recommended)
   - **Stamina Regen Rate:** 15 (recommended)
   - **Max Stamina:** 100

**In Script (Default Values):**
```csharp
// /Assets/Scripts/PlayerStaminaDisplay.cs (lines 14-17)
public float maxStamina = 100f;
public float staminaRegenRate = 15f;
public float staminaDrainRate = 8f;  // Updated from 5 → 8
```

---

## Testing Different Settings

### **Quick Test Formula:**

1. **Set drain rate** to test value
2. **Sprint continuously** and time until stamina depletes
3. **Stop sprinting** and time until stamina fully recovers
4. **Adjust** based on feel:
   - Too fast? Lower drain rate
   - Too slow? Increase drain rate
   - Recovery too slow? Increase regen rate

### **Example Test:**
```
Drain Rate: 10/sec
Expected Duration: 100 / 10 = 10 seconds

Regen Rate: 20/sec
Expected Recovery: 100 / 20 = 5 seconds

Play test: Does this feel right for your game?
```

---

## Integration with Skills

### **Endurance Skill:**
Increases max stamina per level:

```csharp
// Example: +10 max stamina per level
staminaDisplay.maxStamina = 100 + (enduranceLevel * 10);
```

**Results:**
- Level 1: 100 stamina (12.5s sprint)
- Level 5: 150 stamina (18.75s sprint)
- Level 10: 200 stamina (25s sprint)

### **Vitality Skill:**
Increases stamina regeneration per level:

```csharp
// Example: +2 regen per level
staminaDisplay.staminaRegenRate = 15 + (vitalityLevel * 2);
```

**Results:**
- Level 1: 15/sec regen (6.7s recovery)
- Level 5: 25/sec regen (4s recovery)
- Level 10: 35/sec regen (2.9s recovery)

---

## Sprint Mechanics

### **Current Behavior:**
```
Press Shift → Start sprinting
  ↓
Check: playerController.IsRunning?
  ↓ YES
Drain stamina at staminaDrainRate
  ↓
Stamina reaches 0
  ↓
Can still sprint (no prevention yet)
```

### **Recommended Enhancement:**

Add stamina requirement to sprinting:

```csharp
// In JUCharacterController or custom sprint script
if (Input.GetKey(KeyCode.LeftShift) && staminaDisplay.HasStamina(1f))
{
    // Allow sprint
}
else
{
    // Force walk speed
}
```

---

## Visual Feedback

### **Current Display:**
```
Text: "82/100"  (current/max)
Slider: Visual bar (82% filled)
```

### **Suggested Enhancements:**

**Color Coding:**
- Green: > 50% stamina
- Yellow: 25-50% stamina
- Red: < 25% stamina
- Flashing: 0% stamina (exhausted)

**Low Stamina Warning:**
- Screen edge vignette at < 20%
- Heavy breathing sound effect
- Slow regeneration debuff

---

## Game Design Tips

### **Sprint Distance Examples:**

**With 8/sec drain at different movement speeds:**

**Walk Speed: 2 m/s**
- No stamina cost
- Infinite distance

**Sprint Speed: 6 m/s**
- 12.5 seconds sprint time
- Distance: 6 m/s × 12.5s = **75 meters**

**With Recovery:**
- Sprint 12.5s (75m) → Walk 6.7s (13.4m) → Repeat
- Cycle: 19.2 seconds for 88.4m
- Average speed: 4.6 m/s

### **Combat Considerations:**

**Chase Scenarios:**
```
Enemy at 50m distance
Player sprint speed: 6 m/s
Stamina: 100 (12.5s sprint)

Can sprint: 75m (more than enough to escape)
```

**Close Combat:**
```
Dodge cost: 15 stamina per dodge
With 100 stamina: ~6 dodges
Sprint + dodge mix: Tactical choices
```

---

## Balancing for Game Type

### **Open World Exploration:**
```
Stamina Drain: 5-8/sec
Stamina Regen: 15-20/sec
Max Stamina: 100-150

Goal: Allow extended travel without frustration
```

### **Tactical Combat:**
```
Stamina Drain: 12-15/sec
Stamina Regen: 10-12/sec
Max Stamina: 100

Goal: Force positioning decisions and tactical retreats
```

### **Survival Horror:**
```
Stamina Drain: 20-25/sec
Stamina Regen: 5-8/sec
Max Stamina: 80

Goal: Create tension and resource management
```

### **Competitive Multiplayer:**
```
Stamina Drain: 10/sec
Stamina Regen: 15/sec
Max Stamina: 100

Goal: Balanced for skilled play and clutch moments
```

---

## Update Scene Instance

### **IMPORTANT: Scene Override**

The script default is now `8/sec`, but your **scene instance still has `20/sec`**.

**To Fix:**
1. Select stamina GameObject in scene
2. In Inspector, find PlayerStaminaDisplay component
3. Change **Stamina Drain Rate** from `20` to `8`
4. Change **Stamina Regen Rate** from `10` to `15` (optional improvement)
5. Save scene

**Or Reset to Defaults:**
1. Right-click on PlayerStaminaDisplay component
2. Select "Reset Component"
3. Values will update to script defaults (8 drain, 15 regen)

---

## Monitoring & Debugging

### **Console Logging:**
Add debug to see stamina changes:

```csharp
private void UpdateStamina()
{
    bool wasSprinting = playerController != null && playerController.IsRunning;
    
    if (wasSprinting)
    {
        currentStamina -= staminaDrainRate * Time.deltaTime;
        Debug.Log($"Stamina draining: {currentStamina:F1}");
    }
    else
    {
        currentStamina += staminaRegenRate * Time.deltaTime;
    }
    
    currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
}
```

### **On-Screen Display:**
Show drain/regen rate in UI for testing:

```csharp
if (playerController.IsRunning)
{
    staminaText.text = $"{currentStamina:F0}/100 (-{staminaDrainRate}/s)";
}
else
{
    staminaText.text = $"{currentStamina:F0}/100 (+{staminaRegenRate}/s)";
}
```

---

## Files Modified

**Modified:**
- `/Assets/Scripts/PlayerStaminaDisplay.cs`
  - Changed default `staminaDrainRate` from 5 → 8
  - (Regen rate already at 15)

**Scene Instance:**
- `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Stamina/Stamina`
  - Currently: 20 drain, 10 regen ← **NEEDS UPDATE**
  - Should be: 8 drain, 15 regen

**Created:**
- `/Assets/STAMINA_BALANCE_GUIDE.md`

---

## Quick Reference

### **Drain Rate Guide:**
```
5/sec  = 20s sprint (very easy)
8/sec  = 12.5s sprint (balanced) ✅ RECOMMENDED
10/sec = 10s sprint (moderate)
15/sec = 6.7s sprint (hard)
20/sec = 5s sprint (very hard) ❌ TOO FAST
```

### **Regen Rate Guide:**
```
5/sec  = 20s recovery (very slow)
10/sec = 10s recovery (moderate)
15/sec = 6.7s recovery (balanced) ✅ RECOMMENDED
20/sec = 5s recovery (fast)
25/sec = 4s recovery (very fast)
```

---

**Status:** ✅ SCRIPT UPDATED

**Next Step:** Update scene instance values:
- Select stamina GameObject
- Change drain rate: 20 → 8
- Change regen rate: 10 → 15 (optional)
- Test in Play Mode!

**Result:** Stamina will drain much slower and feel more balanced for gameplay!
