# Skill Tree System - Quick Start Guide

## 🚀 Setup in 2 Minutes

### Method 1: Setup Wizard (Recommended)

1. **Open Wizard**
   - Go to `Tools > Skill Tree > Setup Wizard`

2. **Follow Steps**
   - Select your Player GameObject
   - Name your skill tree
   - Choose to create example skills
   - Choose UI (optional)

3. **Done!** ✅

---

### Method 2: Manual Setup

#### Step 1: Add Component to Player
```
1. Select your Player and add SkillTreeComponent
```

#### Step 2: Create Skill Tree Data
```
1. Right-click in Project > Create > KingEdward > Skill Tree > Skill Tree Data
2. Name it (e.g., "My Skill Tree")
3. Drag it to SkillTreeComponent's "Skill Tree" field
```

#### Step 3: Create Skills
```
1. Right-click in Project > Create > KingEdward > Skill Tree > Skill
2. Configure in Inspector:
   - Basic Information (Name, Icon, Cost)
   - Prerequisites (if any)
   - Instructions (On Unlock, On Use, On Level Up)
3. Add skill to Skill Tree Data's "All Skills" list
```

#### Step 4: Add UI to Scene
```
1. Drag "SkillTree.prefab" to your Canvas
2. Drag "SkillHotbar.prefab" to your Canvas
3. Link SkillTreeComponent to SkillTreeUI 
4. Link SkillTreeComponent to SkillHotbarUI
```

---

## 📝 Creating Your First Skill

### Basic Skill (No Prerequisites)
```
1. Create > KingEdward > Skill Tree > Skill
2. Name: "Basic Attack"
3. Set Icon (drag sprite)
4. Set Cost: 1 (skill point)
5. Is Active Skill: ✓
6. Cooldown: 3 seconds
7. Add to Skill Tree Data
```

### Advanced Skill (With Prerequisites)
```
1. Create > KingEdward > Skill Tree > Skill
2. Name: "Power Strike"
3. Prerequisites:
   - Add Element
   - Skill: Basic Attack
   - Required Level: 1
4. Cost: 2
5. Add to Skill Tree Data
```

---

## 🎮 Using Game Creator 2 Instructions

### Give Skill Points
```
Trigger > Instructions > Add Instruction
> KingEdward > Skill Tree > Add Skill Points
Amount: 5
```

### Level Up Skill
```
Trigger > Instructions > Add Instruction
> Skill Tree > Level Up Skill
Skill: [Select your skill]
```

### Check Skill Level
```
Trigger > Conditions > Add Condition
> Skill Tree > Check Skill Level
Skill: [Select your skill]
Level: 2
```

---

## 🎨 Customizing Skills

### Add Instructions to Skill Events

**On Unlock:**
```
1. Select Skill asset
2. Find "On Unlock" section
3. Add Instructions (e.g., Play Sound, Show Message)
```

**On Use:**
```
1. Select Skill asset
2. Find "On Use" section
3. Add Instructions (e.g., Deal Damage, Spawn Effect)
```

**On Level Up:**
```
1. Select Skill asset
2. Find "On Level Up" section
3. Add Instructions (e.g., Increase Stats, Unlock Feature)
```

### Add Conditions

**Can Unlock:**
```
1. Select Skill asset
2. Find "Unlock Conditions" section
3. Add Conditions (e.g., Player Level >= 5)
```

**Can Use:**
```
1. Select Skill asset
2. Find "Use Conditions" section
3. Add Conditions (e.g., Has Mana >= 10)
```

---

## 🔥 Common Patterns

### Pattern 1: Linear Progression
```
Skill 1 (no prereq)
  └─> Skill 2 (requires Skill 1)
      └─> Skill 3 (requires Skill 2)
```

### Pattern 2: Branching Tree
```
        Root Skill
       /          \
   Branch A    Branch B
   /     \      /     \
  A1    A2    B1     B2
```

### Pattern 3: Ultimate Skill
```
   Skill A    Skill B
      \         /
       Ultimate
   (requires both)
```

---

## 💾 Save/Load System

### Automatic Save
```
The system automatically integrates with Game Creator 2's save system.
Just use GC2's Remember component!
```

### Manual Save/Load
```
1. Add Remember component to Player
2. Add Memory > Skill Tree > Skill Tree Memory
3. Configure SkillTreeComponent reference
4. Use GC2's Save/Load instructions
```

### Passive Skills & Save/Load

**For stat modifiers and buffs:**
```
OnUnlock:
> Add Stat Modifier: Health +50
> Add Stat Modifier: Damage +10%

☑ Reapply On Unlock On Load = TRUE
```
Effects will be reapplied when loading a saved game.

**For one-time effects (VFX, spawns):**
```
OnUnlock:
> Play Sound: Unlock
> Spawn VFX: Unlock Effect

☐ Reapply On Unlock On Load = FALSE
```
Visual/audio effects won't replay on load.

**For instantiated objects:**
```
OnUnlock:
> Instantiate: Companion Pet

☐ Reapply On Unlock On Load = TRUE
```
Recreates pet instance.

---

## 🎯 Hotbar Usage

### Assign Skill to Hotbar
```
Method 1: Drag & Drop
- Drag skill from tree to hotbar slot

Method 2: Click + Number
- Click skill in tree
- Press number key (1-8)
```

### Use Skill from Hotbar
```
- Press number key (1-8)
- Or click hotbar slot
```

---

## ⚙️ Advanced Features

### Ground Skill Indicator + Charge (Hold & Release / DualStage)
Use this for ground-targeted skills (AOE circles, cones, lines) and optional charge animation while aiming.

**Step 1: Configure the Skill**
```
1. Select your Skill asset
2. Indicator Config:
   - Type: Circle / Cone / Line / Expanding Circle / Expanding Line
3. (Optional) Charge while aiming:
   - Use Charge State With Indicator: ✓
   - Charge State + Charge State Layer: set as desired
```

**Step 2: Add the Indicator Controller to the Scene**
```
GameObject > KingEdward > Skill Tree > Create Skill Indicator
Assign references
```

**Step 3: Configure Hotbar Input (Important)**
```
1. Select SkillHotbarUI
2. Indicator Input Mode:
   - HoldAndRelease: hold to aim, release to cast
   - DualStage: first press shows indicator, second press casts
3. For the slot hotkey: use an input from Input System
   - Input System > Input Action 
   - Bind keyboard + gamepad in the Input Action Asset
```

---

### Channelled Skills (Hold to keep running)
Channelled skills keep executing while the slot input is held and channel conditions remain true.

```
1. Select your Skill asset
2. Enable: Is Channel Skill ✓
3. Configure:
   - On Channel Tick (runs every frame while channeling)
   - Can Channel (optional conditions to keep channeling)
4. If you also use Sequencer (animation phases):
   - Set `Channel Start Mode` to `AtReleaseEnd` (or CustomNormalizedTime) to align when ChannelState starts during the animation.
4. On the Hotbar slot hotkey, ensure you use Input System bindings

```

### Specific Level Conditions
```
1. Select Skill
2. Find "Specific Level Conditions"
3. Add Element
4. Target Level: 3
5. Add specific conditions for level 3
6. Add specific instructions for level 3
```

### Skill Points Economy
```
// Give points on level up
On Player Level Up:
  > Add Skill Points: 1

// Give points on quest complete
On Quest Complete:
  > Add Skill Points: 3
```
---

## 🎨 Customizing Tooltips

### Prerequisite Display Styles

Configure how prerequisites are shown in tooltips:

**Checkbox Style (Default):**
```
Prerequisites:
☑ Fireball Level 2
☐ Ice Blast Level 1
```

**Bullet Style:**
```
Prerequisites:
✓ Fireball Level 2
✗ Ice Blast Level 1
```

**Arrow Style:**
```
Prerequisites:
→ Fireball Level 2
→ Ice Blast Level 1
```

**No Icon:**
```
Prerequisites:
Fireball Level 2
Ice Blast Level 1
```

### Configuration

In SkillTooltip Inspector:
```
Prerequisites Display:
├─ Prerequisites Label: "Prerequisites:" (customizable)
├─ Prerequisite Style: Checkbox / Bullet / Arrow / Dash / None
└─ Custom Icons: Change ☑/☐, ✓/✗, etc.
```

---

## 💰 Skill Refund System

### Refund Individual Skills

**Via SkillItemUI:**
On Refund Button Click:
> Refund Skill (or Add to Pending Changes)

**Via Instruction:**
```
On Button Click:
> Refund Skill
  Target: Player
  Skill: Fireball
  Refund All Levels: ☑
```

### Reset All Skills

```
On Button Click:
> Reset All Skills
  Target: Player
  Refund Points: ☑
```

---

## ✅ Confirmation UI (Optional)

Add a confirmation system to prevent accidental skill unlocks (Souls-like).

### Setup

1. **Add Component to Canvas:**
   - Create a Panel GameObject
   - Add `SkillTreeConfirmationUI` component

2. **Configure References:**
   ```
   References:
   ├─ Confirmation Panel (the panel GameObject)
   ├─ Pending Skills Text
   ├─ Total Cost Text
   ├─ Current Points Text
   ├─ Remaining Points Text
   ├─ Confirm Button
   └─ Cancel Button
   ```

3. **Choose Mode:**
   ```
   Settings:
   ├─ Mode: Immediate / Batch
   ├─ Auto Show Panel: ☑
   └─ Prevent Direct Unlock: ☑
   ```

### Modes

**Immediate Mode:**
- Shows confirmation for each skill individually
- Confirm → Unlocks immediately
- Cancel → Closes without unlocking

**Batch Mode:**
- Accumulates multiple skills
- Shows total cost
- Confirm → Unlocks all at once
- Cancel → Clears all pending

### Example Usage

**Immediate:**
```
Click Fireball → Shows:
"Unlock Fireball?
Cost: 2
Current: 10
After: 8"
[Confirm] [Cancel]
```

**Batch:**
```
Click Fireball → Adds to list
Click Ice Blast → Adds to list
Click Lightning → Adds to list

Shows:
"Pending Changes:
• Unlock Fireball (-2)
• Unlock Ice Blast (-3)
• Unlock Lightning (-2)
Total Cost: 7
Current: 10
After: 3"
[Confirm All] [Cancel All]
```


## 🐛 Troubleshooting

### Skills Don't Unlock
```
✓ Check prerequisites are met
✓ Check skill points are sufficient
✓ Check unlock conditions
✓ Check skill is in Skill Tree Data
```

### Hotbar Not Working
```
✓ Check SkillHotbarUI is linked to SkillTreeComponent
✓ Check skill is unlocked
✓ Check skill is Active Skill
✓ Check Input System is configured
```

### Save/Load Not Working
```
✓ Check Remember component is on Player
✓ Check Skill Tree Memory is configured
✓ Check SkillTreeComponent reference is set
✓ Test with GC2's save/load instructions
```

---

## 💡 Tips

- Start with 3-5 simple skills
- Test unlock flow before adding more
- Use clear, descriptive skill names
- Add icons for better UX
- Test save/load early
- Use debug logs flag for troubleshooting

---
**Need Help?**
- Email: kingedwardstudioscontact@gmail.com

**Enjoying the system?**
Please leave a review! ⭐⭐⭐⭐⭐




