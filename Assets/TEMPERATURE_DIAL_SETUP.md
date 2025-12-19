# Temperature Dial Setup Guide

## Overview
This guide shows how to implement the Stat_Dial_04 dial to visually display player temperature using a radial fill meter.

---

## Script Created: `TemperatureDial.cs`

**Location:** `Assets/Scripts/TemperatureDial.cs`

### Features:
- ✅ Automatic radial fill based on player temperature
- ✅ Smooth fill animations
- ✅ Color gradient (warm → cold → critical)
- ✅ Optional text display (temperature value)
- ✅ Auto-finds required components
- ✅ Works with SurvivalManager temperature system
- ✅ Fully configurable thresholds and colors

---

## Quick Setup (5 Steps)

### 1. Locate Your Dial UI Element

Find **Stat_Dial_04** in your scene or HUD:
- Typically in Canvas hierarchy
- Look for: `HUD_Apocalypse_Stat_Dial_04` or similar
- Should have Image components with Fill type

### 2. Add TemperatureDial Script

1. Select the **Stat_Dial_04** GameObject (root)
2. Click "Add Component"
3. Search for "TemperatureDial"
4. Add the script

### 3. Let Auto-Find Do Its Magic

The script will automatically find:
- ✅ SurvivalManager (temperature source)
- ✅ Fill Image (the radial dial)
- ✅ Text component (if exists)

**Nothing to configure if your hierarchy is standard!**

### 4. Verify Setup

Check the Inspector - you should see:
```
✅ Survival Manager: [Auto-assigned]
✅ Dial Fill Image: [Auto-assigned]
✅ Temperature Text: [Auto-assigned or empty]
```

### 5. Test in Play Mode

- Temperature dial should fill based on player temperature
- Full dial = Normal temperature (36.9°C)
- Empty dial = Freezing (0°C)
- Colors change: Orange (warm) → Blue (cold) → Dark Blue (critical)

---

## Manual Configuration (If Needed)

### Finding the Correct Image Component

If auto-find doesn't work, manually assign:

1. **Dial Fill Image:**
   - Look for child GameObject named "Fill", "Dial_Fill", or "IMG_Fill"
   - Must have Image component with Type = "Filled"
   - Fill Method should be "Radial 360"

2. **Drag and Drop:**
   - Drag the Image component → Dial Fill Image slot
   - The script will auto-configure it as radial

---

## Configuration Options

### Dial Settings

```
Dial Settings:
├── Max Fill Amount: 1.0         (full circle)
├── Min Fill Amount: 0.0         (empty)
├── Fill Transition Speed: 2.0   (smoothness)
└── Smooth Fill: ✅              (animated transitions)
```

**Effect:**
- Higher transition speed = faster dial movement
- Disable smooth fill for instant updates

---

### Color Settings

```
Color Settings:
├── Warm Color: Orange (FF8000)      (normal temperature)
├── Cold Color: Blue (0080FF)        (low temperature)
├── Critical Color: Dark Blue (4C4CFF) (freezing)
└── Enable Color Gradient: ✅
```

**Temperature → Color Mapping:**
```
36.9°C (Normal)    →  Orange (warm)
20-35°C            →  Blue to Orange gradient
10-20°C (Cold)     →  Blue
0-10°C (Critical)  →  Dark Blue
```

**Customize:**
- Click color boxes to choose your own colors
- Disable gradient for solid color

---

### Text Display (Optional)

```
Text Display:
├── Show Temperature Value: ✅
├── Show As Percentage: ☐        (or show °C)
├── Decimal Places: 1
└── Text Format: "{0}°C"
```

**Example Outputs:**
- `{0}°C` → "36.9°C"
- `{0}%` → "100%"
- `Temp: {0}` → "Temp: 36.9"
- `{0}` → "36.9"

---

### Temperature Thresholds

```
Temperature Thresholds:
├── Critical Threshold: 10°C     (triggers critical color)
└── Cold Threshold: 20°C         (triggers cold color)
```

**Adjust these to change when colors appear:**
- Lower critical = more time in normal/cold colors
- Higher cold = more time in warm colors

---

## Understanding the Dial Behavior

### Temperature Ranges

| Temperature | Dial Fill | Color      | Status      |
|-------------|-----------|------------|-------------|
| 36.9°C      | 100%      | Orange     | Normal      |
| 30°C        | 81%       | Orange     | Warm        |
| 20°C        | 54%       | Blue       | Cold        |
| 10°C        | 27%       | Dark Blue  | Critical    |
| 5°C         | 14%       | Dark Blue  | Freezing    |
| 0°C         | 0%        | Dark Blue  | Hypothermia |

### Fill Calculation

```
Fill Amount = (Current Temperature / Max Temperature) × 100%
Example: 30°C / 36.9°C = 0.81 = 81% fill
```

---

## UI Hierarchy Example

**Recommended Structure:**

```
Stat_Dial_04 (TemperatureDial script here)
├── Background (Image - static circle)
├── Fill (Image - Type: Filled, Radial 360) ← Auto-found
├── Icon (Image - temperature icon)
└── Text_Value (TextMeshPro) ← Auto-found
```

**Script Attachment:**
- Attach TemperatureDial to the **root** GameObject
- It will find Fill and Text in children automatically

---

## Integration with SurvivalManager

The dial automatically reads from:
```csharp
SurvivalManager.currentTemperature  // Current player temp (°C)
SurvivalManager.maxTemperature      // Maximum temp (36.9°C)
```

### Temperature Changes Update Dial:
- ✅ Indoor/outdoor transitions
- ✅ Near fire zones
- ✅ Cold zones
- ✅ Safe zones
- ✅ Any temperature modifier

**No additional code needed** - it just works!

---

## Advanced Customization

### 1. Reverse Fill Direction

Make dial empty when warm, full when cold:

**In Inspector:**
```
Dial Settings:
├── Max Fill Amount: 0.0  (swap these)
└── Min Fill Amount: 1.0
```

### 2. Different Fill Method

Change radial fill style:

1. Select Dial Fill Image
2. In Image component:
   - Fill Method: Radial 360, Radial 180, Radial 90, etc.
   - Fill Origin: Top, Bottom, Left, Right
   - Fill Clockwise: ✅ or ☐

### 3. Custom Temperature Display

Change text format in Inspector:
```
Text Format Examples:
- "{0}°C"           → "36.9°C"
- "TEMP: {0}"       → "TEMP: 36.9"
- "{0}% HEAT"       → "100% HEAT"
- "Body Temp: {0}"  → "Body Temp: 36.9"
```

### 4. Pulse Effect on Critical

Add this component alongside TemperatureDial:
```csharp
using UnityEngine;
using UnityEngine.UI;

public class CriticalPulse : MonoBehaviour
{
    public TemperatureDial dial;
    public float pulseSpeed = 2f;
    
    void Update()
    {
        if (dial.IsCritical())
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            dial.SetDialColor(Color.Lerp(Color.blue, Color.white, pulse * 0.3f));
        }
    }
}
```

---

## Troubleshooting

### Issue: Dial not filling
**Solutions:**
1. Check SurvivalManager is in scene
2. Verify current temperature is > 0
3. Enable Debug mode → check console logs
4. Ensure Image Type is "Filled"

### Issue: No color changes
**Solutions:**
1. Check "Enable Color Gradient" is enabled
2. Verify threshold values are correct
3. Check Image component has proper material

### Issue: Fill image not found
**Manual Fix:**
1. Find the Image component with Fill type
2. Drag it to "Dial Fill Image" slot
3. Script will auto-configure it

### Issue: Text not updating
**Solutions:**
1. Ensure "Show Temperature Value" is enabled
2. Verify Text component is assigned
3. Check Text Format string is valid

### Issue: Dial fills backwards
**Solution:**
- In Dial Fill Image component
- Uncheck "Fill Clockwise"
- Or swap Min/Max Fill Amount values

---

## Public API Methods

Use these from other scripts:

```csharp
TemperatureDial dial = GetComponent<TemperatureDial>();

// Manual control
dial.SetFillAmount(0.75f);          // Set to 75% fill
dial.SetDialColor(Color.red);       // Custom color
dial.ForceUpdate();                 // Instant update

// Get info
float percentage = dial.GetFillPercentage();  // 0-100
bool critical = dial.IsCritical();            // Is freezing?
```

---

## Performance Notes

- ✅ Efficient: Only updates when needed
- ✅ Smooth: Uses Time.deltaTime for smooth transitions
- ✅ No allocations: Reuses color/fill calculations
- ✅ GC-friendly: No string allocations per frame

**Impact:** Negligible - safe to use multiple dials

---

## Multiple Dials

You can use the same script for different stats:

**Example: 4 Dials**
```
Stat_Dial_01 → TemperatureDial (temperature)
Stat_Dial_02 → StaminaDial (stamina - create similar script)
Stat_Dial_03 → InfectionDial (infection)
Stat_Dial_04 → HealthDial (health)
```

**Copy Script Pattern:**
Just replace references:
- `survivalManager.currentTemperature` → `survivalManager.currentStamina`
- Adjust `maxTemperature` → `maxStamina`

---

## Testing Checklist

✅ Dial shows correct fill on scene start  
✅ Fill updates when temperature changes  
✅ Colors transition smoothly  
✅ Text displays correct value  
✅ Critical threshold changes to blue  
✅ Smooth animation works  
✅ No console errors  

**Test Temperature Changes:**
1. Enter cold zone → dial should decrease
2. Near fire → dial should increase
3. Enter safe zone → dial should restore

---

## Visual Examples

### Normal Temperature (36.9°C)
```
    ▓▓▓▓▓▓▓
  ▓▓       ▓▓
 ▓▓   100%  ▓▓   ← Orange fill
 ▓▓         ▓▓
  ▓▓       ▓▓
    ▓▓▓▓▓▓▓
```

### Cold (15°C)
```
    ▓▓▓---
  ▓▓       --
 ▓▓   40%   --   ← Blue fill
 --         --
  --       --
    -------
```

### Critical (5°C)
```
    ▓--
  --       --
 --   14%   --   ← Dark blue fill
 --         --
  --       --
    -------
```

---

## Summary

✅ Add TemperatureDial script to Stat_Dial_04  
✅ Auto-finds all required components  
✅ Displays temperature as radial fill  
✅ Color gradient: warm → cold → critical  
✅ Smooth animations  
✅ Optional text display  
✅ Zero configuration needed  
✅ Works with SurvivalManager  
✅ Fully customizable  

**Setup Time:** < 1 minute  
**Result:** Professional temperature gauge with visual feedback

---

**Script Location:** `Assets/Scripts/TemperatureDial.cs`  
**Usage:** Attach to any dial UI element with radial fill image
