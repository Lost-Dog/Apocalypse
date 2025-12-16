# Stats Warnings Panel - Fix Applied

## Changes Made

### 1. Updated `PlayerStatusIndicators.cs`
Changed default behavior to keep the panel always visible:
- `startDisabled` changed from `true` to `false`
- `autoHideWhenNoWarnings` changed from `true` to `false`
- Updated temperature thresholds to use Celsius values:
  - `temperatureWarningThreshold` = 15°C (was 0.4 or 40%)
  - `temperatureCriticalThreshold` = 5°C (was 0.2 or 20%)

### 2. Updated `SurvivalManager.cs`
Rescaled temperature system to use actual Celsius values:
- Maximum temperature: 36.9°C (normal body temperature)
- Warning threshold: 15°C (displays yellow warning)
- Critical threshold: 5°C (displays red warning + causes damage)
- Removed percentage-based `criticalColdThreshold`
- Added absolute temperature thresholds
- Updated temperature status messages to reflect Celsius scale

### 3. Updated `PlayerTemperatureDisplay.cs`
Fixed display to show Celsius properly:
- Changed from percentage display to Celsius display
- Suffix changed from "%" to "°C"
- Updated status thresholds to match new scale
- Shows temperature as "36.9°C (Normal)"

### 4. Created Fix Tool
A new Editor tool has been created: `FixStatsWarningsReferences.cs`

## How to Apply the Fix

### Option 1: Use the Automated Fix Tool (Recommended)
1. Make sure the `Apocalypse` scene is open
2. In Unity's top menu, go to: **Tools → Fix Stats Warnings References**
3. Check the Console for confirmation messages

The tool will automatically:
- Find and link the `PlayerInfectionDisplay` component
- Set `startDisabled` to `false`
- Set `autoHideWhenNoWarnings` to `false`
- Update temperature thresholds to Celsius values (15°C and 5°C)
- Mark the scene as modified so you can save changes

### Option 2: Manual Fix
1. Select `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings` in the Hierarchy
2. In the Inspector, find the `PlayerStatusIndicators` component
3. Locate the `Infection Display` field (under "Auto-Find References")
4. Drag the GameObject `/UI/HUD/ScreenSpace/Bottom/PlayerStats/Player_Infection/Infection` into this field
5. Under "Panel Behavior":
   - Uncheck `Start Disabled`
   - Uncheck `Auto Hide When No Warnings`
6. Under "Temperature Thresholds":
   - Set `Temperature Warning Threshold` = 15
   - Set `Temperature Critical Threshold` = 5
7. Save the scene

## What This Fixes

**Before:**
- Panel would start disabled and only show when warnings were active
- Missing infection reference meant infection warnings wouldn't work
- Panel would auto-hide when no warnings were active
- Temperature used confusing percentage-based thresholds
- Conflict with `HUDManager` trying to keep it visible

**After:**
- Panel stays visible at all times
- Individual warning indicators appear/disappear based on player stats
- Infection warnings will work properly
- Temperature uses realistic Celsius values (36.9°C normal, 15°C warning, 5°C critical)
- No conflicts between scripts

## How It Works Now

The `Stats_Warnings` panel will:
- Always be visible on the HUD
- Show individual warnings when:
  - **Health** ≤ 50% (warning) or ≤ 25% (critical)
  - **Temperature** ≤ 15°C (warning) or ≤ 5°C (critical)
  - **Infection** ≥ 50 (infected) or ≥ 75 (severe)
- Hide individual indicators when stats return to normal
- Display pulsing effects with color changes (yellow/red) based on severity

## New Temperature System

### Temperature Scale
- **36.9°C** - Normal body temperature
- **35-36.9°C** - Normal status
- **30-35°C** - Cool
- **20-30°C** - Cold  
- **15-20°C** - Very Cold (yellow warning appears)
- **5-15°C** - Freezing (yellow warning)
- **Below 5°C** - Critical/Hypothermia (red warning + health damage)
- **0°C** - Death (frozen)

### Warning Display
- **Above 15°C**: No temperature warning
- **15°C or below**: Yellow "COLD" warning
- **5°C or below**: Red "FREEZING" warning + cold damage

## Testing

After applying the fix:
1. Enter Play Mode
2. Check that the `Stats_Warnings` panel is visible (even if empty)
3. Enable `Enable Temperature Decrease` in SurvivalManager to test temperature warnings
4. Damage yourself to see health warnings appear
5. Temperature will drop from 36.9°C and warnings appear at 15°C and 5°C

## Troubleshooting

If warnings still don't appear:
1. Verify the child GameObjects exist:
   - `Warning_Health`
   - `Warning_Temperature`
   - `Warning_Infection`
2. Check that the `WARNINGS` label GameObject is present
3. Ensure references are assigned in the `PlayerStatusIndicators` component:
   - `playerHealth` → Player's `JUHealth` component
   - `survivalManager` → `SurvivalManager` in the scene
   - `infectionDisplay` → `PlayerInfectionDisplay` component
4. Check temperature thresholds are set to:
   - Warning: 15 (not 0.4)
   - Critical: 5 (not 0.2)

## Additional Documentation

See `TEMPERATURE_SYSTEM_UPDATE.md` for complete details on the new Celsius-based temperature system.

