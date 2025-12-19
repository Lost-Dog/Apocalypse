# Explosion Fire/Burn Damage Setup Guide

## Overview
This guide explains how to add fire/burn damage to the Explosion prefab with player burn VFX effects.

---

## Script Created: `ExplosionDamage.cs`

**Location:** `Assets/Scripts/ExplosionDamage.cs`

### Features:
- ✅ Automatic trigger sphere collider setup
- ✅ Continuous fire damage while player is inside
- ✅ Burn damage that continues after leaving (DoT effect)
- ✅ Burn VFX effects attached to player
- ✅ Audio support (fire enter sound + burn loop)
- ✅ Unity Events for integration
- ✅ Debug mode for testing

---

## Setup Instructions

### 1. Add Script to Explosion Prefab

**Steps:**
1. Open the Explosion prefab in Unity:
   - Location: `Assets/Prefabs/Explosion.prefab`
2. Select the root "Explosion" GameObject
3. Click "Add Component"
4. Search for "ExplosionDamage"
5. Add the script

The script will automatically:
- Create a SphereCollider (if none exists)
- Set it as a trigger
- Configure the damage radius

---

### 2. Configure Damage Settings

**Recommended Values:**

```
Damage Settings:
├── Fire Base Damage: 5          (damage per second while inside)
├── Burn Damage Per Second: 2    (damage after leaving)
├── Burn Duration: 3             (seconds burn continues)
└── Damage Tick Interval: 0.5    (how often damage applies)
```

**Damage Calculation:**
- **Fire Damage:** 5 HP/sec × 0.5 tick = 2.5 HP per tick
- **Burn Damage:** 2 HP/sec × 0.5 tick = 1 HP per tick (for 3 seconds)

---

### 3. Configure Trigger Settings

```
Trigger Settings:
├── Damage Radius: 5             (meters - adjust to match explosion size)
└── Player Tag: "Player"         (tag to detect)
```

**Visual Feedback:**
- Orange gizmo sphere shown in Scene view when prefab is selected
- Adjust radius to match your explosion visual size

---

### 4. Add Burn VFX (Optional but Recommended)

**Creating Burn VFX:**

1. **Use Existing Particle System:**
   - Search for fire/burn particle effects in your assets
   - Recommended: Look for orange/red flame particles

2. **Create Prefab:**
   - Make the particle system a prefab
   - Ensure it has a ParticleSystem component
   - Set to "Looping" if you want continuous effect

3. **Assign to Script:**
   ```
   Visual Effects:
   ├── Burn VFX Prefab: [Your Fire/Burn Particle Prefab]
   ├── VFX Offset: (0, 1, 0)        (places at player's torso)
   └── Attach VFX To Player: ✅     (VFX follows player)
   ```

**VFX Behavior:**
- Spawns when player enters fire
- Attached to player (follows them)
- Continues during burn duration
- Auto-destroys after burn ends

---

### 5. Add Audio (Optional)

```
Audio:
├── Fire Enter Sound: [Explosion/Fire whoosh sound]
├── Burn Loop Sound: [Crackling fire loop]
└── Sound Volume: 0.7
```

**Audio Behavior:**
- Fire Enter Sound: Plays once when entering
- Burn Loop Sound: Loops while in fire, stops on exit
- 3D spatial audio (volume based on distance)

---

### 6. Configure Events (Optional)

Use Unity Events for additional effects:

```
Events:
├── On Player Enter Fire   → Trigger screen shake, UI warning
├── On Player Exit Fire    → Stop screen effects
├── On Burn Start          → Show "Burning" UI indicator
└── On Burn End            → Hide "Burning" UI indicator
```

**Example Uses:**
- Camera shake when entering fire
- UI message: "You're on fire!"
- Screen tint/overlay effect
- Sound effects

---

## Testing & Debug

### Enable Debug Mode:
```
Debug:
└── Show Debug Info: ✅
```

**Console Output:**
```
[ExplosionDamage] Player entered fire zone
[ExplosionDamage] Applied 2.5 fire damage to player
[ExplosionDamage] Player exited fire zone
[ExplosionDamage] Started burn effect (3s duration)
[ExplosionDamage] Applied 1 burn damage to player (2.5s remaining)
[ExplosionDamage] Stopped burn effect
```

### Testing Checklist:
- ✅ Player takes damage when entering explosion trigger
- ✅ Damage continues while inside
- ✅ Burn effect starts when leaving
- ✅ Burn VFX appears on player
- ✅ Burn damage applies over time
- ✅ VFX removes after burn ends

---

## Integration with Existing Systems

### Works With:
- ✅ JUHealth (JUTPS health system)
- ✅ PlayerSystemBridge (damage callbacks)
- ✅ SurvivalManager (temperature/infection)
- ✅ ExplosionManager (random explosions)

### Example Integration:

**Increase Infection on Burn:**
```csharp
// Add to OnBurnStart event (Inspector)
public void OnPlayerBurnStart()
{
    SurvivalManager survivalManager = SurvivalManager.Instance;
    if (survivalManager != null)
    {
        survivalManager.AddInfection(10f); // +10% infection
    }
}
```

**Screen Effect on Fire:**
```csharp
// Add to OnPlayerEnterFire event
public void ShowFireScreenEffect()
{
    // Add orange/red screen overlay
    // Increase vignette effect
}
```

---

## Advanced Configuration

### Damage Scenarios:

**Deadly Fire (High Damage):**
```
Fire Base Damage: 10
Burn Damage Per Second: 5
Burn Duration: 5
Damage Tick Interval: 0.25
```
Result: ~40 HP/sec in fire, ~20 HP total burn damage

**Environmental Hazard (Low Damage):**
```
Fire Base Damage: 2
Burn Damage Per Second: 1
Burn Duration: 2
Damage Tick Interval: 1
```
Result: ~2 HP/sec in fire, ~2 HP total burn damage

**Instant Death Zone:**
```
Fire Base Damage: 100
Burn Damage Per Second: 0
Burn Duration: 0
Damage Tick Interval: 0.1
```
Result: ~1000 HP/sec (kills instantly)

---

## Public API Methods

Use these from other scripts:

```csharp
ExplosionDamage explosionDamage = GetComponent<ExplosionDamage>();

// Change damage radius at runtime
explosionDamage.SetDamageRadius(10f);

// Force stop burn effect (e.g., when player uses healing item)
explosionDamage.ClearBurnEffect();

// Check if player is burning
bool isBurning = explosionDamage.IsPlayerBurning();
```

---

## Common Issues & Solutions

### Issue: No damage applied
**Solution:**
- Ensure player has "Player" tag
- Check player has JUHealth component
- Enable debug mode to see console logs

### Issue: VFX not appearing
**Solution:**
- Ensure Burn VFX Prefab is assigned
- Check VFX prefab has ParticleSystem component
- Try different VFX Offset values

### Issue: Collider not triggering
**Solution:**
- Player must have a Rigidbody (can be kinematic)
- SphereCollider must be set to "Is Trigger"
- Check layer collision matrix

### Issue: Damage too high/low
**Solution:**
- Adjust Fire Base Damage and Burn Damage values
- Change Damage Tick Interval (lower = more frequent)
- Remember: Actual damage = DamagePerSec × TickInterval

---

## Example VFX Setup

### Recommended Particle Settings:

```
Particle System:
├── Duration: 3.0
├── Looping: ✅
├── Start Lifetime: 0.5-1.0
├── Start Speed: 0.5-2.0
├── Start Size: 0.2-0.5
├── Start Color: Orange → Red gradient
├── Emission Rate: 20-50
└── Shape: Sphere (radius 0.5)
```

**Material:**
- Additive shader
- Fire/flame texture
- Orange/red color

---

## Performance Considerations

- Damage calculations only run when player is in/near fire
- VFX automatically destroyed when not needed
- Uses efficient trigger detection
- Timer-based damage (not every frame)

---

## Summary

✅ Script automatically sets up trigger collider  
✅ Applies fire damage while player inside  
✅ Continues burn damage after leaving  
✅ Supports burn VFX effects on player  
✅ Audio feedback for immersion  
✅ Unity Events for custom integration  
✅ Debug mode for testing  
✅ Works with existing JUTPS health system  

**Next Steps:**
1. Add ExplosionDamage script to Explosion prefab
2. Configure damage values
3. Assign burn VFX prefab (optional)
4. Test in Play mode
5. Adjust values to your liking

---

**Script Location:** `Assets/Scripts/ExplosionDamage.cs`  
**Prefab Location:** `Assets/Prefabs/Explosion.prefab`
