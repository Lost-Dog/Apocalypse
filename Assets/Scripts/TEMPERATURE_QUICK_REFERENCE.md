# Temperature System - Quick Reference

## Temperature Scale (Celsius)

```
36.9°C ━━━━━━━━━━━━━━━━━━━━━━━━━ Normal (Healthy)
  ↓
35.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ Normal status
  ↓
30.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ Cool
  ↓
20.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ Cold
  ↓
15.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ ⚠️ WARNING (Very Cold - Yellow)
  ↓
 5.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ 🔴 CRITICAL (Freezing - Red + Damage)
  ↓
 0.0°C ━━━━━━━━━━━━━━━━━━━━━━━━━ ☠️ DEATH (Hypothermia)
```

## Warning Thresholds

| Temperature | Status | Warning | Effect |
|------------|--------|---------|--------|
| > 15°C | Safe | None | Normal gameplay |
| ≤ 15°C | Cold | Yellow "COLD" | Warning appears |
| ≤ 5°C | Critical | Red "FREEZING" | Warning + 0.5 HP/sec damage |
| 0°C | Death | Red "HYPOTHERMIA" | Player dies |

## Default Rate Settings

- **Decrease Rate**: 0.2°C/sec (~12°C/min)
- **Recovery Rate**: 2°C/sec
- **Time to freeze**: ~3 minutes (from 36.9°C to 0°C)
- **Indoor warming**: +10°C/sec
- **Fire warming**: +15°C/sec
- **Cold zone multiplier**: 2x decrease rate

## Key Values to Remember

- **Normal**: 36.9°C
- **Warning**: 15°C
- **Critical**: 5°C
- **Death**: 0°C

## Code Examples

```csharp
// Check temperature state
if (SurvivalManager.Instance.IsCriticalCold)
{
    // Player is below 5°C - taking damage
}

if (SurvivalManager.Instance.IsWarningCold)
{
    // Player is below 15°C - show warning
}

// Modify temperature
SurvivalManager.Instance.WarmUp(10f);      // +10°C
SurvivalManager.Instance.CoolDown(5f);     // -5°C
SurvivalManager.Instance.SetTemperature(20f); // Set to 20°C
SurvivalManager.Instance.ResetTemperature();  // Reset to 36.9°C

// Get status
string status = SurvivalManager.Instance.GetTemperatureStatus();
// Returns: "Normal", "Cool", "Cold", "Very Cold", "Freezing", or "Hypothermia"
```

## Inspector Values

### SurvivalManager
- Max Temperature: `36.9`
- Normal Temperature: `36.9`
- Warning Temperature: `15`
- Critical Temperature: `5`

### PlayerStatusIndicators
- Temperature Warning Threshold: `15`
- Temperature Critical Threshold: `5`

### PlayerTemperatureDisplay
- Suffix: `°C`
- Show Decimal: `✓`
