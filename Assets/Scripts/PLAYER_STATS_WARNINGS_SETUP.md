# Player Stats Warnings - Setup Guide

## Overview

Your Stats Warnings panel at `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings` has been updated to:
- ✅ Start disabled (hidden)
- ✅ Automatically show when player stats warnings are triggered
- ✅ Support for audio clip playback when warnings appear
- ✅ Auto-hide when all warnings are resolved

---

## Quick Setup (30 seconds)

1. Select your GameObject: `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings`
2. In the Inspector, find the `PlayerStatusIndicators` component
3. Click **"Add Audio Source"** button
4. Click **"Enable Panel Behavior"** button
5. ✅ Done! The panel will now start disabled and show only when warnings trigger

---

## How It Works

### Automatic Panel Behavior

```
Player stats are normal
    ↓
Panel is HIDDEN (disabled)
    ↓
Health drops below threshold
OR Temperature drops too low
OR Infection increases
    ↓
⚠️ Warning triggered!
    ↓
Panel SHOWS (enabled)
Audio plays (if assigned)
Warning displayed with color/text
    ↓
Stats return to normal
    ↓
Panel HIDES (disabled)
```

---

## Settings Explained

### Panel Behavior (New)

```
Start Disabled: ✓ Checked
└── Panel will be hidden at game start

Auto Hide When No Warnings: ✓ Checked
└── Panel hides automatically when all stats are normal
```

### Audio (New)

```
Audio Source: (auto-added)
└── Plays warning sounds

Warning Sound: (your audio clip)
└── Plays when ANY warning becomes active
```

---

## Adding Your Audio Clip

### Step 1: Import Audio

1. Import your warning sound to `/Assets/Audio/Warnings/` (or any folder)
2. Recommended: Short alert sound (0.3-1 second)

### Step 2: Assign to Component

1. Select `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings`
2. Find `PlayerStatusIndicators` component
3. Drag your audio clip to the **"Warning Sound"** field

### Step 3: Test

1. Enter Play Mode
2. Damage the player to trigger low health warning
3. Listen for the audio clip!

---

## Warning Triggers

The panel shows when ANY of these conditions are met:

### Health Warning (Yellow)
```
Condition: Health ≤ 50%
Display: "LOW HEALTH" (yellow text)
```

### Health Critical (Red)
```
Condition: Health ≤ 25%
Display: "CRITICAL" (red text, faster pulse)
```

### Temperature Warning (Yellow)
```
Condition: Temperature ≤ 40%
Display: "COLD" (yellow text)
```

### Temperature Critical (Red)
```
Condition: Temperature ≤ 20%
Display: "FREEZING" (red text, faster pulse)
```

### Infection Warning (Yellow)
```
Condition: Infection ≥ 50
Display: "INFECTED" (yellow text)
```

### Infection Critical (Red)
```
Condition: Infection ≥ 75
Display: "SEVERE" (red text, faster pulse)
```

---

## Customizing Thresholds

You can adjust when warnings appear:

```csharp
[Header("Health Thresholds")]
Health Warning Threshold: 0.5 (50%)
Health Critical Threshold: 0.25 (25%)

[Header("Temperature Thresholds")]
Temperature Warning Threshold: 0.4 (40%)
Temperature Critical Threshold: 0.2 (20%)

[Header("Infection Thresholds")]
Infection Warning Threshold: 50
Infection Critical Threshold: 75
```

To change:
1. Select the `Stats_Warnings` GameObject
2. Find `PlayerStatusIndicators` component
3. Adjust the threshold values
4. Test in Play Mode

---

## Configuration Options

### Current Settings

The `PlayerStatusIndicators` component has these sections:

```
Indicator Objects
├── Health Indicator
├── Temperature Indicator
└── Infection Indicator

Auto-Find References: ✓ Checked
├── Player Health (auto-found)
├── Survival Manager (auto-found)
└── Infection Display (auto-found)

Thresholds
├── Health Warning: 50%
├── Health Critical: 25%
├── Temperature Warning: 40%
├── Temperature Critical: 20%
├── Infection Warning: 50
└── Infection Critical: 75

Visual Effects
├── Enable Pulse Effect: ✓ Checked
├── Normal Pulse Speed: 1
├── Warning Pulse Speed: 2
└── Critical Pulse Speed: 4

Panel Behavior (NEW)
├── Start Disabled: ✓ Checked
└── Auto Hide When No Warnings: ✓ Checked

Audio (NEW)
├── Audio Source: (component reference)
└── Warning Sound: (your audio clip)
```

---

## Testing the System

### Test 1: Low Health Warning

1. Enter Play Mode
2. Take damage to drop health below 50%
3. ✅ Panel should appear with "LOW HEALTH"
4. ✅ Audio should play
5. Heal above 50%
6. ✅ Panel should disappear

### Test 2: Multiple Warnings

1. Enter Play Mode
2. Drop health below 50%
3. ✅ Panel appears
4. Let temperature drop below 40%
5. ✅ Panel stays visible, shows both warnings
6. Heal health above 50%
7. ✅ Panel still visible (temperature warning active)
8. Warm up above 40%
9. ✅ Panel disappears (no warnings)

### Test 3: Critical State

1. Enter Play Mode
2. Drop health below 25%
3. ✅ "CRITICAL" text appears in RED
4. ✅ Pulse effect speeds up
5. ✅ Audio plays

---

## Audio Best Practices

### Recommended Audio Specs

- **Format**: WAV or OGG
- **Length**: 0.3-1.0 seconds
- **Type**: Alert beep, alarm, warning tone
- **Volume**: Medium (not too loud)

### Good Warning Sounds

- Short beep (0.3s)
- Alert tone (0.5s)
- Alarm pulse (0.8s)
- Warning chime (1.0s)

### Audio Source Settings

The auto-added AudioSource is configured with:
- Play On Awake: ✗ Off
- Loop: ✗ Off
- Spatial Blend: 0 (2D)
- Volume: 1.0

You can adjust volume in the AudioSource component if needed.

---

## Integration with Existing Systems

The panel automatically integrates with:

### ✅ Health System (JUTPS.JUHealth)
- Monitors player health component
- Auto-finds player by "Player" tag
- Updates every frame

### ✅ Temperature System (SurvivalManager)
- Monitors temperature percentage
- Only active if temperature system is enabled
- Auto-finds SurvivalManager

### ✅ Infection System (PlayerInfectionDisplay)
- Monitors infection level
- Auto-finds infection display component
- Updates every frame

---

## Troubleshooting

### Panel doesn't hide at start

**Check:**
1. "Start Disabled" is checked ✓
2. No errors in Console
3. GameObject is not manually set to inactive in hierarchy

**Fix:**
Click "Enable Panel Behavior" button in Inspector

---

### Panel doesn't show when health is low

**Check:**
1. Player has JUHealth component
2. Player is tagged as "Player"
3. Health threshold is correct (default 50%)
4. No errors in Console

**Test:**
- Select Stats_Warnings in Play Mode
- Check "Runtime Info" section
- Verify "Has Active Warnings" status

---

### Audio doesn't play

**Check:**
1. AudioSource component exists
2. Warning Sound is assigned
3. AudioSource volume is not 0
4. Audio Listener exists in scene

**Fix:**
1. Click "Add Audio Source" button
2. Assign your audio clip
3. Test by damaging player

---

### Multiple audio plays stacking

**Expected Behavior:**
- Audio plays once when warning **becomes** active
- Won't play again while warning stays active
- Will play again if warning deactivates then reactivates

This prevents audio spam!

---

### Panel shows but no warnings visible

**Check:**
1. Warning indicator objects are assigned
2. TextMeshProUGUI components exist on child objects:
   - `Warning_Health`
   - `Warning_Temperature`
   - `Warning_Infection`
3. Canvas is rendering correctly

---

## Advanced Customization

### Different Audio for Each Warning

If you want different sounds for health, temperature, and infection:

1. Modify the `PlayerStatusIndicators.cs` script
2. Add fields:
```csharp
public AudioClip healthWarningSound;
public AudioClip temperatureWarningSound;
public AudioClip infectionWarningSound;
```
3. Update `PlayWarningSound()` to accept a clip parameter
4. Pass specific clips from each update method

### Disable Auto-Hide

To keep the panel always visible:

1. Uncheck "Auto Hide When No Warnings"
2. Uncheck "Start Disabled"
3. Panel will stay visible, warnings show/hide as needed

### Custom Warning Messages

Edit the text in each update method:

```csharp
UpdateLabel(healthIndicator.labelText, "YOUR CUSTOM TEXT");
```

Example: Change "LOW HEALTH" to "HEALTH WARNING"

---

## API Reference

### Public Methods

```csharp
bool HasActiveWarnings()
```
Returns `true` if any warning is currently active.

Usage:
```csharp
PlayerStatusIndicators indicators = GetComponent<PlayerStatusIndicators>();
if (indicators.HasActiveWarnings())
{
    Debug.Log("Player has active warnings!");
}
```

---

## Summary

### ✅ What Changed

1. **Panel starts disabled** - Hidden until warnings trigger
2. **Auto-shows on warnings** - Activates when health/temp/infection warnings occur
3. **Audio support added** - Plays sound when warnings become active
4. **Auto-hides** - Disappears when all stats return to normal
5. **Custom editor** - Quick setup buttons in Inspector

### 🎯 Current State

Your Stats_Warnings panel now:
- ✓ Starts hidden
- ✓ Shows when player stats drop to warning levels
- ✓ Plays audio when warnings activate (when assigned)
- ✓ Hides when player stats return to normal
- ✓ Supports health, temperature, and infection warnings
- ✓ Color-coded (yellow = warning, red = critical)
- ✓ Pulse effect for visual feedback

### 📋 Quick Checklist

- [ ] "Add Audio Source" button clicked
- [ ] "Enable Panel Behavior" button clicked
- [ ] Warning audio clip imported
- [ ] Warning audio clip assigned to component
- [ ] Tested in Play Mode (damage player to trigger)
- [ ] Verified panel appears when health is low
- [ ] Verified audio plays when warning triggers
- [ ] Verified panel hides when stats return to normal

---

## Next Steps

1. **Import your warning audio clip**
2. **Assign it to the Warning Sound field**
3. **Test in Play Mode** by damaging the player
4. **Adjust thresholds** if warnings trigger too early/late
5. **(Optional) Customize warning messages**

**You're all set!** Your Stats Warnings panel will now automatically show and hide based on player stats, with audio feedback! 🎉
