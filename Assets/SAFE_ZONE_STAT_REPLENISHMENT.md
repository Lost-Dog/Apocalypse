# Safe Zone Stat Replenishment System

## Overview
The SafeZone script now replenishes all player survival stats over a configurable duration (default: 5 seconds) when the player enters a safe zone.

## Features

### Stat Restoration
All player stats are smoothly restored from their current values to maximum over the configured duration:

1. **Health** - Restored to MaxHealth
2. **Stamina** - Restored to maxStamina
3. **Temperature** - Normalized to normalTemperature
4. **Infection** - Reduced to 0 (cured)
5. **Hunger** - Restored to maxHunger
6. **Thirst** - Restored to maxThirst

### Configuration Options

#### Individual Stat Toggles
Each stat can be enabled/disabled independently:
- `restoreHealth` - Enable health restoration (default: true)
- `restoreStamina` - Enable stamina restoration (default: true)
- `normalizeTemperature` - Enable temperature normalization (default: true)
- `cureInfection` - Enable infection cure (default: true)
- `restoreHunger` - Enable hunger restoration (default: true)
- `restoreThirst` - Enable thirst restoration (default: true)

#### Restoration Settings
- `replenishDuration` - Time in seconds to fully replenish all stats (default: 5 seconds)
- `useSmoothTransition` - Uses SmoothStep for smooth easing (default: true)
- `restoreDelay` - Delay before restoration starts (default: 0 seconds)
- `requireIdle` - Only restore when player is not moving (default: false)

## How It Works

### Entry Behavior
1. When player enters safe zone, all current stat values are captured
2. Restoration progress begins immediately (or after restoreDelay)
3. Stats smoothly lerp from starting values to target values over replenishDuration
4. Visual and audio healing effects play during restoration

### Exit Behavior
1. If player leaves safe zone, restoration stops immediately
2. Stats remain at their current partially-restored values
3. Healing effects stop

### Smooth Transition
When `useSmoothTransition` is enabled:
- Uses `Mathf.SmoothStep` for smooth easing in/out
- Provides more natural-feeling restoration curve
- Stats restore faster at the beginning and end, slower in the middle

When disabled:
- Uses linear interpolation
- Constant restoration rate throughout duration

## Example Scenarios

### Scenario 1: Full Restoration (5 seconds)
```
Player enters with:
- Health: 30/100
- Stamina: 20/100
- Hunger: 10/100
- Thirst: 5/100
- Temperature: 15°C (normal: 37°C)
- Infection: 80

After 5 seconds in safe zone:
- Health: 100/100 ✓
- Stamina: 100/100 ✓
- Hunger: 100/100 ✓
- Thirst: 100/100 ✓
- Temperature: 37°C ✓
- Infection: 0 ✓
```

### Scenario 2: Early Exit
```
Player enters with Health: 30/100
After 2.5 seconds (50% progress), player exits
Health: ~65/100 (restored halfway)
```

### Scenario 3: Selective Restoration
```
Configuration:
- restoreHealth: true
- restoreStamina: true
- restoreHunger: false
- restoreThirst: false

Result: Only health and stamina are restored
```

## Integration with Existing Systems

### Ammo Replenishment
- Ammo is still replenished instantly on entry (controlled by `replenishAmmo`)
- Grenades are added instantly (controlled by `grenadestoAdd`)
- These are separate from the stat restoration system

### Visual Feedback
- Healing effects (`healingEffect`) play during stat restoration
- Effects stop when all stats are fully restored
- Enter effects (`enterEffect`) play once on entry

### UI Integration
- Works with existing MessageDisplay system
- Shows "Entered Safe Zone - Restoring Stats" message
- Duration controlled by `messageDuration`

## Inspector Setup

1. **Select SafeZone GameObject** in hierarchy
2. **Configure Stat Toggles** - Enable/disable individual stats
3. **Set Replenish Duration** - Adjust time for full restoration (default: 5 seconds)
4. **Choose Smooth Transition** - Enable for smooth easing, disable for linear
5. **Optional: Set Restore Delay** - Add delay before restoration starts
6. **Optional: Require Idle** - Force player to stand still for restoration

## Technical Details

### Smooth Restoration Algorithm
```csharp
restorationProgress += Time.deltaTime / replenishDuration;
float t = useSmoothTransition ? Mathf.SmoothStep(0f, 1f, restorationProgress) : restorationProgress;
currentStat = Mathf.Lerp(startStat, targetStat, t);
```

### Performance
- Minimal overhead - only lerps active stats
- Stops checking when restoration is complete
- No coroutines or repeated allocations

### Compatibility
- Works with existing SurvivalManager
- Compatible with JUHealth system
- Integrates with existing safe zone features

## Troubleshooting

**Stats not restoring:**
- Check that individual stat toggles are enabled
- Verify SurvivalManager is present in scene
- Ensure player has JUHealth component

**Restoration too fast/slow:**
- Adjust `replenishDuration` value
- Default is 5 seconds, can be set to any value

**Restoration stops when moving:**
- Check `requireIdle` setting
- If enabled, player must stand still
- Disable for continuous restoration while moving

**Ammo not replenishing:**
- This is controlled by separate `replenishAmmo` toggle
- Check that JUInventory exists on player
- Ammo replenishment is instant, not gradual
