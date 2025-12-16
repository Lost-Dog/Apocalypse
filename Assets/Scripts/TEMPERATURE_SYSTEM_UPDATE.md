# Temperature System - Celsius Rescaling Update

## Overview

The temperature system has been updated to use actual Celsius values instead of percentages, making it more realistic and easier to understand.

## New Temperature Scale

### Temperature Values
- **36.9°C** - Normal body temperature (maximum/healthy)
- **35°C+** - Normal status
- **30-35°C** - Cool
- **20-30°C** - Cold
- **15-20°C** - Very Cold
- **5-15°C** - Freezing (warning displayed)
- **Below 5°C** - Critical/Hypothermia (damage occurs)
- **0°C** - Player freezes (death)

### Warning Thresholds
- **Warning (Yellow)**: Temperature ≤ 15°C
- **Critical (Red)**: Temperature ≤ 5°C

## Changes Made

### 1. SurvivalManager.cs
Updated temperature system to use Celsius values:

**Old System:**
- Used percentage-based thresholds (0-1 range)
- `criticalColdThreshold = 0.2f` (20% of max)
- Temperature displayed as percentage

**New System:**
- Uses actual Celsius values
- `warningTemperature = 15f` (15°C)
- `criticalTemperature = 5f` (5°C)
- Added `IsWarningCold` property
- Updated `GetTemperatureStatus()` to use Celsius thresholds

**Updated Settings:**
- `temperatureDecreaseRate = 0.2f` (°C per second)
- `temperatureNormalizeRate = 2f` (°C per second)
- Temperature now decreases ~12°C per minute in cold environments
- Recovery rate is 2°C per second

### 2. PlayerStatusIndicators.cs
Updated warning display thresholds:

**Old System:**
- `temperatureWarningThreshold = 0.4f` (40% of max)
- `temperatureCriticalThreshold = 0.2f` (20% of max)

**New System:**
- `temperatureWarningThreshold = 15f` (15°C)
- `temperatureCriticalThreshold = 5f` (5°C)
- Updated `UpdateTemperatureIndicator()` to compare against Celsius values

### 3. PlayerTemperatureDisplay.cs
Updated UI display:

**Old System:**
- `showAsPercentage` option
- Displayed as "37%" or similar

**New System:**
- `showDecimal` option
- Displays as "36.9°C (Normal)"
- Updated suffix from "%" to "°C"
- Updated status thresholds to match Celsius scale

### 4. FixStatsWarningsReferences.cs (Editor Tool)
Enhanced to automatically update old percentage-based values:
- Converts old 0.4 threshold → 15°C
- Converts old 0.2 threshold → 5°C
- Validates and updates all temperature settings

## How Temperature Works Now

### Normal Gameplay
1. Player starts at 36.9°C (normal body temperature)
2. Temperature naturally normalizes toward 36.9°C at 2°C/sec
3. Cold environments decrease temperature at 0.2°C/sec (configurable)

### Environmental Effects
- **Cold Zones**: Temperature decreases 2x faster (0.4°C/sec with default multiplier)
- **Indoors**: Temperature increases by 10°C/sec
- **Near Fire**: Temperature increases by 15°C/sec

### Warning States
- **Below 15°C**: Yellow "COLD" warning appears
- **Below 5°C**: Red "FREEZING" warning appears + health damage begins
- **At 0°C**: Player death (frozen)

### Health Impact
When temperature drops below 5°C:
- Player takes cold damage
- Default: 0.5 HP per second
- No blood screen flash effect
- Stamina drains faster

## Migration from Old System

If you have existing scene data with old percentage-based values:

### Automatic Fix (Recommended)
1. Open the Apocalypse scene
2. Go to **Tools → Fix Stats Warnings References**
3. Check Console for confirmation

### Manual Fix
1. Select `SurvivalManager` in the scene
2. Update these values:
   - Remove old `criticalColdThreshold` (no longer used)
   - Verify `warningTemperature = 15`
   - Verify `criticalTemperature = 5`
   
3. Select `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings`
4. In `PlayerStatusIndicators` component:
   - Set `Temperature Warning Threshold = 15`
   - Set `Temperature Critical Threshold = 5`

## Display Examples

**Temperature Display:**
- `36.9°C (Normal)`
- `20.5°C (Cold)`
- `15.0°C (Very Cold)` ← Warning appears
- `4.2°C (Freezing)` ← Critical warning + damage
- `0.0°C (Hypothermia)` ← Death

**Warning Panel:**
- Above 15°C: No warning shown
- 5-15°C: "COLD" (yellow)
- Below 5°C: "FREEZING" (red)

## Configuration

### SurvivalManager Settings

```
Temperature Settings:
├── Max Temperature: 36.9°C (normal body temp)
├── Normal Temperature: 36.9°C (recovery target)
├── Warning Temperature: 15°C (show warning)
├── Critical Temperature: 5°C (take damage)
├── Decrease Rate: 0.2°C/sec
├── Recovery Rate: 2°C/sec
├── Indoor Gain: 10°C/sec
├── Fire Gain: 15°C/sec
└── Cold Zone Multiplier: 2x
```

### Balancing Tips

**Make it harder (faster cooling):**
- Increase `temperatureDecreaseRate` (e.g., 0.3-0.5°C/sec)
- Increase `coldZoneMultiplier` (e.g., 3x or 4x)
- Decrease `indoorTemperatureGain` (e.g., 5°C/sec)

**Make it easier (slower cooling):**
- Decrease `temperatureDecreaseRate` (e.g., 0.1°C/sec)
- Increase `temperatureNormalizeRate` (e.g., 5°C/sec)
- Lower critical threshold to 3°C

**Realistic timing:**
- Default settings: ~3 minutes from 36.9°C to 0°C in cold environment
- With 2x multiplier in cold zones: ~1.5 minutes to freeze

## Testing

1. **Enter Play Mode**
2. **Enable temperature decrease**: In `SurvivalManager`, check `Enable Temperature Decrease`
3. **Monitor temperature**: Watch it drop from 36.9°C
4. **At 15°C**: Yellow "COLD" warning should appear
5. **At 5°C**: Red "FREEZING" warning should appear + health damage starts
6. **Test recovery**: Stand near fire or go indoors to warm up

## Debugging

Enable `Show Debug Info` in `SurvivalManager` to see console output:
```
[Survival] Temp: 12.3°C (Very Cold) | Stamina: 85/100 | Infection: 0/100 [COLD!]
```

## API Reference

### Public Properties
```csharp
SurvivalManager.currentTemperature  // Current temp in Celsius
SurvivalManager.IsCriticalCold      // True when ≤ 5°C
SurvivalManager.IsWarningCold       // True when ≤ 15°C
SurvivalManager.GetTemperatureStatus() // Returns status string
```

### Public Methods
```csharp
survivalManager.SetTemperature(20f);     // Set to 20°C
survivalManager.ModifyTemperature(-5f);  // Decrease by 5°C
survivalManager.WarmUp(10f);             // Increase by 10°C
survivalManager.CoolDown(10f);           // Decrease by 10°C
survivalManager.ResetTemperature();      // Reset to 36.9°C
```

## Notes

- Temperature values are always clamped between 0°C and 36.9°C
- Natural body temperature recovery happens automatically
- Safe zones can pause temperature normalization
- Cold damage only occurs when `enableColdDamage` is true
