# Fire Extinguishing System - Setup Guide

## Overview
Allows players to extinguish fires (Fire_Big FX) with a confirmation popup.

---

## Quick Setup (3 Steps)

### Step 1: Add FireExtinguisher to Fire Objects

1. **Find Fire Objects in Scene:**
   - Look for: `FX_Fire_Big_01`, `FX_Fire_Big_02`, `FX_Fire_Big_03`
   - Usually in scene or prefabs

2. **Add Component:**
   - Select the fire GameObject (parent of Fire_Big FX)
   - Add Component → **FireExtinguisher**
   - Component will auto-find the fire effect

3. **Auto-Configuration:**
   ```
   ✓ Auto Find Fire: Checked (finds Fire_Big in children)
   ```

---

### Step 2: Create Confirmation UI (Optional)

If you want a confirmation popup:

1. **Create UI Panel:**
   - Right-click Canvas → UI → Panel
   - Name it "FireExtinguishUI"

2. **Add Text:**
   - Add TextMeshPro text: "Extinguish this fire?"

3. **Add Buttons:**
   - Add 2 Buttons: "Confirm" and "Cancel"

4. **Assign to FireExtinguisher:**
   ```
   Confirmation UI: FireExtinguishUI
   Prompt Text: (drag text element)
   Confirm Button: (drag confirm button)
   Cancel Button: (drag cancel button)
   ```

**Skip UI for Instant Extinguish:**
Leave "Confirmation UI" empty - fire extinguishes immediately on interact!

---

## How It Works

### Player Approach
```
1. Player gets close to fire
2. Interaction prompt appears: "Press E to Extinguish Fire"
3. Player presses E
```

### With Confirmation UI
```
4. Popup appears: "Extinguish this fire?"
5. Player clicks "Confirm" or "Cancel"
6. If confirmed → Fire fades out
```

### Without Confirmation UI
```
4. Fire immediately starts fading out
```

### Fire Extinguishing
```
5. Fire particles fade over 2 seconds
6. Fire lights dim and turn off
7. Fire object deactivated
8. Player earns XP (default: 25 XP)
```

---

## Configuration Options

### Interaction Settings

```yaml
Interaction Prompt: "Press E to Extinguish Fire"
Interaction Range: 5.0 meters
```

### Fade Out Settings

```yaml
Fade Out Duration: 2.0 seconds
Disable After Extinguish: ✓ true
```

**Fade Effect:**
- Particle emission gradually reduces to 0
- Lights dim to 0 intensity
- Particles stop after fade completes
- Object deactivates after 1 second delay

### Rewards

```yaml
Award XP: ✓ true
XP Reward: 25
```

### Effects (Optional)

```yaml
Extinguish Sound: (Audio clip)
Extinguish Effect: (Particle prefab - steam/smoke)
Sound Volume: 0.7
```

---

## Example Scenarios

### Scenario 1: Quick Extinguish (No UI)

**Setup:**
```
Fire Extinguisher Component:
├── Fire Object: (auto-found)
├── Confirmation UI: None (empty)
├── Interaction Range: 5.0
└── Fade Out Duration: 2.0
```

**Behavior:**
- Player presses E → Fire immediately fades out
- No confirmation needed
- Fast gameplay

---

### Scenario 2: With Confirmation

**Setup:**
```
Fire Extinguisher Component:
├── Fire Object: (auto-found)
├── Confirmation UI: FireExtinguishUI
├── Prompt Text: "Extinguish this fire?"
├── Confirm Button: ConfirmBtn
└── Cancel Button: CancelBtn
```

**Behavior:**
- Player presses E → Popup appears
- Game pauses (Time.timeScale = 0)
- Player confirms → Fire extinguishes
- Player cancels → Popup closes, fire stays
- More deliberate gameplay

---

## Testing

### Test Without UI:

```
1. Play Mode
2. Approach fire
3. Press E
4. Fire should fade out over 2 seconds
5. Check Console: "[FireExtinguisher] Fire extinguished on..."
6. Check XP gained (if ProgressionManager exists)
```

### Test With UI:

```
1. Play Mode
2. Approach fire
3. Press E
4. Popup appears
5. Click "Confirm"
6. Fire fades out
7. Click "Cancel" → Fire stays lit
```

---

## Advanced Customization

### Custom Fade Duration

Make fire fade faster or slower:

```csharp
Fade Out Duration: 5.0  // Slower fade
Fade Out Duration: 0.5  // Instant fade
```

### Custom Rewards

```csharp
XP Reward: 50  // More XP
Award XP: false // No XP reward
```

### Custom Effects

Add steam/smoke when extinguishing:

```
1. Create particle effect (white smoke)
2. Assign to "Extinguish Effect" field
3. Effect spawns and plays when fire extinguished
```

### Custom Sounds

```
1. Find extinguish sound (hiss/steam)
2. Assign to "Extinguish Sound" field
3. Adjust "Sound Volume" (0.0 - 1.0)
```

---

## Troubleshooting

### Issue: Interaction Prompt Not Showing

**Cause:** JUInteractionSystem not finding interactable

**Fix:**
```
1. Check fire object has Collider
2. Set Collider.isTrigger = true
3. Verify player has JUInteractionSystem component
4. Check interaction range (increase if needed)
```

---

### Issue: Fire Not Fading

**Cause:** Fire particles not found

**Fix:**
```
1. Enable "Show Debug Info" on FireExtinguisher
2. Check Console for warnings
3. Manually assign "Fire Object" field
4. Ensure Fire_Big FX has ParticleSystem components
```

---

### Issue: No XP Awarded

**Cause:** ProgressionManager not found

**Fix:**
```
1. Ensure ProgressionManager exists in scene
2. Check "Award XP" is checked
3. Verify "XP Reward" > 0
4. ProgressionManager must be active
```

---

## Integration with Existing Systems

### Works With:

✅ **JU Interaction System** - Uses JUInteractable base  
✅ **ProgressionManager** - Awards XP  
✅ **ParticleSystem** - Fades fire particles  
✅ **Light** - Dims fire lights  

### Compatible With:

✅ All Fire_Big FX prefabs  
✅ Custom fire effects (any ParticleSystem)  
✅ Mission systems (can trigger events)  
✅ Challenge systems (count extinguishes)  

---

## Script API

### Public Methods:

```csharp
// Called by interaction system
void Interact()

// Check if can be extinguished
bool CanInteract(JUInteractionSystem system)
```

### Events (Can Add):

Add UnityEvents in Inspector for:
- OnFireExtinguished
- OnConfirmPressed
- OnCancelPressed

---

## Summary

### ✅ Features:

- ✓ Proximity-based fire extinguishing
- ✓ Optional confirmation popup
- ✓ Smooth fade-out effect
- ✓ Particle & light fading
- ✓ XP rewards
- ✓ Sound effects
- ✓ Visual effects
- ✓ Auto-detection
- ✓ Full JUTPS integration

### 📋 Setup Checklist:

- [ ] Add FireExtinguisher to fire objects
- [ ] Configure auto-find settings
- [ ] Create confirmation UI (optional)
- [ ] Test interaction
- [ ] Verify fade-out
- [ ] Check XP rewards

**Setup Time:** 2-5 minutes per fire  
**Result:** Interactive fire extinguishing mechanic with smooth fade effects!
