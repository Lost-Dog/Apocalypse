# Player Status Warning System Setup

## Overview
Two scripts for displaying critical health, temperature, and infection warnings:

1. **PlayerStatusWarning.cs** - Single warning panel that cycles through alerts
2. **PlayerStatusIndicators.cs** - Multiple persistent indicators (recommended)

---

## Quick Setup - Single Warning Panel

### **Step 1: Prepare Warning Panel**
You selected: `/UI/HUD/ScreenSpace/Bottom/HUD_Apocalypse_Event_Loading_01`

This panel will display warnings.

### **Step 2: Add Component**
1. Select `/UI/HUD/ScreenSpace/Bottom/HUD_Apocalypse_Event_Loading_01`
2. Add Component → **PlayerStatusWarning**

### **Step 3: Configure (Auto)**
- ✅ **Auto Find References** is enabled
- Script automatically finds:
  - Warning text (Label_Loading or Label_Autosaving)
  - Warning icon
  - Player health
  - Survival manager
  - Infection display

### **Step 4: Customize Messages (Optional)**

**In Inspector:**
```
Health Low Message: "LOW HEALTH"
Health Critical Message: "CRITICAL HEALTH"
Temperature Low Message: "GETTING COLD"
Temperature Critical Message: "FREEZING"
Infection Low Message: "INFECTED"
Infection Critical Message: "SEVERE INFECTION"
```

### **Step 5: Test**
- Enter Play Mode
- Panel is hidden initially
- When health drops below 30% → Shows "LOW HEALTH" (yellow)
- When health drops below 15% → Shows "CRITICAL HEALTH" (red)
- Same for temperature and infection

---

## Advanced Setup - Multiple Indicators

### **Recommended: Create Separate Warning Indicators**

**Step 1: Duplicate the Warning Panel**
1. Select `/UI/HUD/ScreenSpace/Bottom/HUD_Apocalypse_Event_Loading_01`
2. Duplicate (Ctrl+D) 3 times
3. Rename:
   - `Warning_Health`
   - `Warning_Temperature`
   - `Warning_Infection`

**Step 2: Position Indicators**
Stack them vertically on the screen:
```
Warning_Health      → Top right
Warning_Temperature → Middle right
Warning_Infection   → Bottom right
```

**Step 3: Add Parent Container**
1. Create Empty GameObject: `/UI/HUD/ScreenSpace/StatusWarnings`
2. Move all 3 warning panels under it
3. Add Component → **PlayerStatusIndicators**

**Step 4: Assign References**
In PlayerStatusIndicators Inspector:
```
Health Indicator:
  ├── Indicator Object: Warning_Health
  ├── Icon Image: (auto-found)
  ├── Label Text: (auto-found)
  └── Pulse Image: (optional background)

Temperature Indicator:
  ├── Indicator Object: Warning_Temperature
  ├── Icon Image: (auto-found)
  ├── Label Text: (auto-found)
  └── Pulse Image: (optional)

Infection Indicator:
  ├── Indicator Object: Warning_Infection
  ├── Icon Image: (auto-found)
  ├── Label Text: (auto-found)
  └── Pulse Image: (optional)
```

---

## Configuration

### **PlayerStatusWarning Settings:**

**Thresholds:**
```
Health Low: 30% (0.3)
Health Critical: 15% (0.15)
Temperature Low: 40% (0.4)
Temperature Critical: 20% (0.2)
Infection Low: 50%
Infection Critical: 75%
```

**Display:**
```
Warning Display Duration: 3 seconds
Warning Cooldown: 5 seconds
Enable Flashing: ☑
Flash Speed: 2
```

**Colors:**
```
Low Warning Color: Yellow (1, 0.8, 0, 1)
Critical Warning Color: Red (1, 0, 0, 1)
```

### **PlayerStatusIndicators Settings:**

**Thresholds:**
```
Health Warning: 50% (0.5)
Health Critical: 25% (0.25)
Temperature Warning: 40% (0.4)
Temperature Critical: 20% (0.2)
Infection Warning: 50%
Infection Critical: 75%
```

**Visual Effects:**
```
Enable Pulse Effect: ☑
Normal Pulse Speed: 1
Warning Pulse Speed: 2
Critical Pulse Speed: 4
```

**Colors (per indicator):**
```
Normal Color: White
Warning Color: Yellow
Critical Color: Red
```

---

## How It Works

### **PlayerStatusWarning (Cycling):**

**Behavior:**
- Monitors all stats continuously
- Shows ONE warning at a time
- Critical warnings take priority
- Auto-hides after 3 seconds
- 5-second cooldown between warnings
- Flashing effect for visibility

**Priority Order:**
1. Health Critical
2. Health Low
3. Temperature Critical
4. Temperature Low
5. Infection Critical
6. Infection Low

### **PlayerStatusIndicators (Persistent):**

**Behavior:**
- Monitors all stats continuously
- Shows ALL active warnings simultaneously
- Indicators stay visible while condition persists
- Pulse effect gets faster as severity increases
- Color changes from yellow (warning) to red (critical)

**States:**
- **Hidden:** Stat is normal/safe
- **Warning:** Stat is low but not critical (yellow, slow pulse)
- **Critical:** Stat is dangerously low (red, fast pulse)

---

## Visual Effects

### **Single Warning Panel:**

**Warning State:**
```
Text: "LOW HEALTH"
Color: Yellow
Effect: Fades in/out (flashing)
Duration: 3 seconds
```

**Critical State:**
```
Text: "CRITICAL HEALTH"
Color: Red
Effect: Rapid flashing
Duration: 3 seconds
```

### **Multiple Indicators:**

**Warning Example:**
```
Health Indicator:
├── Background: Yellow glow (slow pulse)
├── Icon: Yellow health icon
└── Text: "LOW HEALTH" (yellow)
```

**Critical Example:**
```
Temperature Indicator:
├── Background: Red glow (fast pulse)
├── Icon: Red snowflake icon
└── Text: "FREEZING" (red)
```

---

## Customization Examples

### **Adjust When Warnings Appear:**

**More Forgiving (warnings appear later):**
```
Health Warning: 25%
Health Critical: 10%
Temperature Warning: 30%
Temperature Critical: 10%
Infection Warning: 65%
Infection Critical: 85%
```

**More Aggressive (warnings appear earlier):**
```
Health Warning: 60%
Health Critical: 30%
Temperature Warning: 50%
Temperature Critical: 25%
Infection Warning: 40%
Infection Critical: 60%
```

### **Adjust Display Duration:**

**Quick Warnings:**
```
Warning Display Duration: 1.5 seconds
Warning Cooldown: 3 seconds
```

**Long Warnings:**
```
Warning Display Duration: 5 seconds
Warning Cooldown: 10 seconds
```

### **Custom Messages:**

**Immersive/Realistic:**
```
Health Low: "Wounded"
Health Critical: "Bleeding Out"
Temperature Low: "Hypothermia Risk"
Temperature Critical: "Severe Hypothermia"
Infection Low: "Fever Detected"
Infection Critical: "Sepsis Warning"
```

**Arcade Style:**
```
Health Low: "DANGER!"
Health Critical: "!!!CRITICAL!!!"
Temperature Low: "COLD!"
Temperature Critical: "!!!FREEZING!!!"
Infection Low: "SICK!"
Infection Critical: "!!!DYING!!!"
```

---

## Code Integration

### **Manually Trigger Warning:**

```csharp
PlayerStatusWarning warning = FindFirstObjectByType<PlayerStatusWarning>();

// Show custom warning
warning.ForceShowWarning("ACHIEVEMENT UNLOCKED", Color.green, 5f);

// Clear current warning
warning.ClearWarning();
```

### **Check Indicator Status:**

```csharp
PlayerStatusIndicators indicators = FindFirstObjectByType<PlayerStatusIndicators>();

// Check if health indicator is showing
if (indicators.healthIndicator.isActive)
{
    Debug.Log("Player is in danger!");
}
```

---

## Testing Checklist

### **Health Warnings:**
- ☐ Take damage → Health drops below 50%
- ☐ Warning appears: "LOW HEALTH" (yellow)
- ☐ Take more damage → Health drops below 25%
- ☐ Warning changes: "CRITICAL HEALTH" (red)
- ☐ Heal above threshold → Warning disappears

### **Temperature Warnings:**
- ☐ Enable temperature decrease in SurvivalManager
- ☐ Temperature drops below 40%
- ☐ Warning appears: "GETTING COLD" (yellow)
- ☐ Temperature drops below 20%
- ☐ Warning changes: "FREEZING" (red)
- ☐ Warm up → Warning disappears

### **Infection Warnings:**
- ☐ Call: `FindFirstObjectByType<PlayerInfectionDisplay>().AddInfection(55)`
- ☐ Warning appears: "INFECTED" (yellow)
- ☐ Call: `AddInfection(25)` (total 80%)
- ☐ Warning changes: "SEVERE INFECTION" (red)
- ☐ Infection decays naturally → Warning disappears

### **Visual Effects:**
- ☐ Warning flashes/pulses
- ☐ Critical warnings pulse faster
- ☐ Colors match severity
- ☐ Multiple warnings can show simultaneously (if using indicators)

---

## UI Layout Recommendations

### **Single Warning (Center Top):**
```
┌─────────────────────────┐
│                         │
│    ⚠ CRITICAL HEALTH    │
│                         │
└─────────────────────────┘
```

### **Multiple Indicators (Right Side):**
```
                    ┌─────────────────┐
                    │ ❤️ LOW HEALTH   │
                    └─────────────────┘
                    
                    ┌─────────────────┐
                    │ ❄️ FREEZING     │
                    └─────────────────┘
                    
                    ┌─────────────────┐
                    │ 🦠 INFECTED     │
                    └─────────────────┘
```

### **Compact Corner (Bottom Right):**
```
                            ┌───┐
                            │❤️│ 
                            └───┘
                            ┌───┐
                            │❄️│
                            └───┘
```

---

## Troubleshooting

**Warning doesn't appear:**
- Check "Auto Find References" is enabled
- Verify player has JUHealth component
- Check SurvivalManager exists in scene
- Verify warning panel is in hierarchy

**Warning shows but no text:**
- Check Label_Loading or Label_Autosaving exists
- Manually assign Warning Text field
- Verify TextMeshProUGUI component exists

**Warning never hides:**
- Check Warning Display Duration > 0
- Verify stat is actually recovering
- Check Update() is being called

**Multiple warnings conflict:**
- Use PlayerStatusIndicators instead
- Adjust warning cooldown time
- Decrease warning display duration

**Colors don't change:**
- Check Color fields are set correctly
- Verify Icon Image and Label Text are assigned
- Check material/shader supports color changes

---

## Performance Notes

- Both scripts update every frame
- Minimal performance impact
- Suitable for mobile/VR
- No allocations during runtime
- Efficient state tracking

---

**Created:**
- PlayerStatusWarning.cs (single cycling warning)
- PlayerStatusIndicators.cs (multiple persistent indicators)

**Location:** `/Assets/Scripts/`  
**Documentation:** `/Assets/STATUS_WARNING_SETUP.md`

**Selected Panel:** `/UI/HUD/ScreenSpace/Bottom/HUD_Apocalypse_Event_Loading_01`
