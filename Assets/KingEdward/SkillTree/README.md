# Advanced Skill System for Game Creator 2

A complete, production-ready skill tree system with visual scripting integration, save/load support, and a powerful sequencer for Game Creator 2.



## 🎯 Features

### Core System
- ✅ **Flexible Skill Trees** - Create unlimited skill trees with prerequisites and level requirements
- ✅ **Multi-Level Skills** - Skills can have multiple levels with different effects per level
- ✅ **Skill Points System** - Configurable currency system for unlocking and upgrading skills
- ✅ **Prerequisites** - Skills can require other skills to be unlocked first
- ✅ **Unlock Conditions** - Use Game Creator 2 conditions for complex unlock requirements
- ✅ **Active & Passive Skills** - Support for both skill types

### Visual Scripting Integration
- ✅ **40+ Visual Scripting Elements** - Complete integration with Game Creator 2
  - 6 Events/Triggers (OnSkillUnlocked, OnSkillLevelUp, OnSkillUsed, etc.)
  - 9 Conditions (IsSkillUnlocked, CanUnlockSkill, CompareSkillLevel, IsSkillInHotbar, etc.)
  - 20 Instructions (UnlockSkill, UseSkill, SpawnInFormation, VortexPull, PersistentZone, etc.)
  - 6 Properties (GetSkillValue, GetSkillTreeValue, CheckSkillLevel, LastVortex, LastPull, ProjectileCaster)

### Animation System
- ✅ **Sequencer Integration** - Timeline-based skill execution with animation sync
- ✅ **Root Motion Support** - Full support for root motion animations
- ✅ **Phase-Based Execution** - Cast, Release, and Recovery phases
- ✅ **Frame-Perfect Instructions** - Execute instructions at specific animation frames

### UI System
- ✅ **Drag & Drop** - Intuitive skill tree UI with drag and drop support
- ✅ **Skill Hotbar** - Quick access bar with keyboard shortcuts (1-8)
- ✅ **Tooltips** - Configurable tooltips with skill information
- ✅ **Visual Feedback** - Cooldown indicators, level display, unlock states, stack counters
- ✅ **Gamepad/Mobile UI Navigation** - Selection/navigation modes for controller support (node focus + inner buttons)
- ✅ **Ground Skill Indicator** - Circle/Cone/Line AOE preview with Hold&Release or DualStage input
- ✅ **Charge State While Aiming** - Optional character charge State while indicator is active
- ✅ **Connection Lines** - Beautiful vector-quality lines between skills
- ✅ **Confirmation System** - Confirmation UI for unlock/level up/refund
- ✅ **Refund System** - Refund skills and recover spent points
- ✅ **Skill Points Display** - Animated skill points counter with color feedback

### Technical Features
- ✅ **Object Pooling** - Optimized memory management for skill instances
- ✅ **Save/Load System** - Full persistence support with Game Creator 2 memory
- ✅ **Cooldown System** - Robust cooldown management with visual feedback
- ✅ **Skill Stacks** - Use skills multiple times in a time window before triggering cooldown
- ✅ **Channelled Skills** - Optional per-frame channel tick while input/conditions remain true
- ✅ **Debug Mode** - Built-in debug logging for troubleshooting
- ✅ **Clean Architecture** - Professional code with encapsulation and error handling

---

## 📋 Requirements

- **Unity** 6000.0.50f1 or higher
- **Game Creator 2** (Core package required)
- **TextMeshPro** (included with Unity)

---

## 🚀 Quick Start

### 1. Installation

1. Import the Skill Tree System package into your project
2. Ensure Game Creator 2 is already installed
3. The system will automatically set up required folders

### 2. Setup Wizard (Recommended)

The easiest way to get started:

1. Go to `Tools > Skill Tree > Setup Wizard`
2. Follow the step-by-step wizard:
   - Select your Player GameObject
   - Create your first skill tree
   - Choose to create example skills (recommended)
   - Set up UI in your scene
3. Click "Setup!" and you're done!

### 3. Manual Setup

If you prefer manual setup:

#### Create a Skill Tree
1. Right-click in Project window
2. `Create > Skill Tree > Skill Tree Data`
3. Name it (e.g., "Combat Skills")

#### Create Skills
1. Right-click in Project window
2. `Create > Skill Tree > Skill`
3. Configure the skill:
   - Name and description
   - Icon
   - Cost (skill points)
   - Max level
   - Cooldown duration
   - Prerequisites (optional)

#### Add to Player
1. Select your Player GameObject
2. Add Component > `Skill Tree Component`
3. Assign your Skill Tree Data
4. Set initial skill points

#### Add UI
1. Drag `SkillTree.prefab` into your Canvas
2. Drag `SkillHotbar.prefab` into your Canvas
3. Link the Skill Tree Component reference

---

## 📖 Core Concepts

### Skill Tree Data
The main container for your skills. Think of it as a "skill database" that holds all available skills for a character class or system.

### Skill
A ScriptableObject that defines:
- **Basic Info**: Name, description, icon
- **Costs**: Unlock cost, level up cost
- **Requirements**: Prerequisites, unlock conditions
- **Actions**: What happens on unlock, use, and level up
- **Animation**: Optional sequencer for animated skills
 - **Indicator & Charge**: Optional ground indicator (circle, cone, line) and charge state while aiming

### Skill Instance
Runtime representation of a skill for a specific player:
- Current level
- Unlock state
- Cooldown state
 - Stack window state (uses remaining before cooldown)
- Managed automatically by the system

### Cooldown & Stack Logic (Overview)

- **Cooldown Duration**: Base cooldown time after a stack window finishes.
- **Stacks**:
  - When **Has Stacks** is disabled, every successful use immediately starts cooldown.
  - When **Has Stacks** is enabled, the skill can be used multiple times before cooldown starts.
  - You control this with:
    - **Stack Uses Before Cooldown** – how many uses fit in a “window” before the cooldown fires.
    - **Use Stack Time** + **Stack Window Duration** – optional time limit for that window.
- **Behaviour**:
  - Each successful use increments an internal counter.
  - Cooldown starts when:
    - The number of uses reaches **Stack Uses Before Cooldown**, **or**
    - **Use Stack Time** is enabled and the time since the first use in the window exceeds **Stack Window Duration**.
  - When the cooldown starts, the internal stack window resets for the next cycle.

### Skill Tree Component
The main component on your player that:
- Manages skill points
- Tracks unlocked skills
- Handles skill usage
- Triggers events
- Manages cooldowns and stack windows per-skill.
- Exposes helpers for channelled skills and indicator input so your skills can react to “key held” state.

---

## 🎮 Usage Examples

### Unlock a Skill (Visual Scripting)

```
Trigger: On Button Click
Condition: Has Skill Points >= 5
Instruction: Unlock Skill [Fireball]
```

### Use a Skill with Hotkey

The hotbar automatically handles this! Just:
1. Drag a skill from the tree to a hotbar slot
2. Press the number key (1-8)
3. The skill executes with cooldown

### Use a Ground Indicator + Charge (Hold & Release)

To create a skill that shows a ground indicator, plays a charge state while aiming, and fires on key release:

1. **Configure the Skill asset**
   - In the **Skill**:
     - Enable **Use Charge State With Indicator** if you want a charge animation while aiming.
     - Assign a **Charge State**  and **Charge State Layer** on the skill.
   - In **Indicator Config**:
     - Enable **Has Indicator**.
     - Choose a type: **Circle**, **Cone**, **Line**, **Expanding Circle** or **Expanding Line**.
     - Set **Radius / Range**, **Min Radius**, **Max Radius**, **Expand Duration**, etc.

2. **Place `SkillIndicatorController` in the scene**
   - **GameObject/KingEdward/SkillTree/Create Skill Indicator**.
   - Assign:
     - **Character**: the `Character` GameObject (Game Creator 2 Character).
     - **Camera**: the camera used for mouse aiming.
     - **Ground Layers**: layer mask used for ground raycasts.

3. **Configure the Hotbar for Hold & Release**
   - On `SkillHotbarUI`:
     - Set **Indicator Input Mode** to **HoldAndRelease**.
     - For each slot that uses hold-to-aim, set the **slot hotkey** to an input that sends both **press** (Start) and **release** (Cancel). Assign your Input Action Asset (e.g. `InputSystem_Actions`) and action (e.g. `Player/Attack`). Bind keyboard and gamepad in the asset so one config works for all devices. The hotbar uses Start = hold begin, Cancel = release to cast; 
   - At runtime:
     - Hold the mapped key (e.g. `1`) → the indicator appears and follows the cursor.
     - While holding, the **charge state** is played on the character if configured.
     - Release the key → the indicator hides and the skill is cast using the aimed position/direction.

Internally:
- `SkillTreeComponent.BeginAim` + `EndAimAndCast` control the aim mode.
- `SkillIndicatorController`:
  - Calculates ground position under the cursor or fixed-at-character position.
  - Updates **Last Target Position** and **Last Radius** (available as properties in Visual Scripting).
  - Starts and stops the **charge state** while the indicator is visible.

### Check Skill Level (Visual Scripting)

```
Condition: Check Skill Level
  - Skill: [Fireball]
  - Comparison: Greater Than or Equal
  - Required Level: 3
```

### Award Skill Points

```
Trigger: On Enemy Killed
Instruction: Add Skill Points
  - Amount: 1
```

### Refund a Skill

```
Trigger: On Button Click
Instruction: Refund Skill
  - Skill: [Fireball]
  - Refund All Levels: true
```

### Create AOE Damage Zone

```
Trigger: On Skill Used
Instruction: Spawn Persistent Zone
  - Position: Player Position
  - Radius: 5
  - Duration: 10 seconds
  - Tick Rate: 1 second
  - On Tick: Apply Damage (10)
  - Parent: [Optional - attach to moving object]
  - VFX Prefab: [Zone Effect]
```

### Pull Enemies (Black Hole Effect)

```
Trigger: On Skill Used
Instruction: Pull Enemies To Position
  - Position: Player Position
  - Radius: 10
  - Force: 5
  - Duration: 3 seconds
```

### Vortex Pull (Hurricane Effect)

```
Trigger: On Skill Used
Instruction: Vortex Pull
  - Position: Player Position
  - Radius: 10
  - Pull Force: 5
  - Spin Force: 8
  - Duration: 3 seconds
  - Separation Force: 3 (prevents enemies stacking)
  - VFX Prefab: [Tornado Effect]
```

### Spawn Projectiles in Formation

```
Trigger: On Skill Used
Instruction: Spawn In Formation
  - Prefab: [Fireball Projectile]
  - Count: 8
  - Formation: Circle
  - Spacing: 2
```

### Create Skill with Animation

1. Create a Skill asset
2. Enable "Use Sequencer"
3. Assign an Animation Clip
4. Add phases in the sequencer:
   - **Cast Phase**: Wind-up animation
   - **Release Phase**: Execute damage/effects
   - **Recovery Phase**: Cool-down animation
5. Add instructions to each phase

### Create a Skill with Stacks (Multiple Uses Before Cooldown)

To create a skill that can be used multiple times before cooldown:

1. Open the **Skill** asset.
2. In **Cooldown & Stacks**:
   - Set **Cooldown Duration** to the base cooldown after a stack window finishes.
   - Enable **Has Stacks**.
   - Set **Stack Uses Before Cooldown**:
     - Example: `3` → the player can use the skill 3 times before cooldown triggers.
   - (Optional) Enable **Use Stack Time** and set **Stack Window Duration**:
     - Example: `5` seconds → if 3 uses are not consumed within 5 seconds, cooldown still triggers at the end of this time.
3. Assign the skill to a **hotbar slot**.
4. At runtime:
   - Each successful use increments the internal counter.
   - The hotbar shows **Remaining Stack Uses** in the slot’s **Stack Text**:
     - When not on cooldown: the number of uses left before the cooldown will start.
     - While on cooldown: stack text shows is deactivated.
   - When the window finishes (by count or time), cooldown starts, the overlay fills and empties, and stacks reset when cooldown ends.

---

## 🎨 UI Customization

### Skill Tree UI
- Modify `SkillTree.prefab` to change layout
- Adjust colors in `SkillItemUI` component
- Configure connection line style in `SkillTreeUI`

### Skill Hotbar
- Change hotbar position (Top, Bottom, Left, Right)
- Customize slot appearance
- Configure hotkeys in `SkillHotbarUI`

### Tooltips
- Adjust tooltip offset per skill
- Modify `SkillTooltip.prefab` for custom styling
- Add/remove information fields

---

## 🎮 Gamepad / Controller

The Skill Tree UI supports two ways to use a gamepad. Configure them on the **Skill Tree UI** component (Gamepad / Navigation section).

### Control modes

| Mode | Description |
|------|-------------|
| **Cursor** | Move a virtual cursor with the left stick or d-pad. Tooltip follows the cursor; South (A) / East (B) click under the cursor. Good for “mouse-like” control. |
| **Selection** | Navigate between skill nodes with the stick or d-pad. One node is focused at a time; South submits, North can trigger the active button (see below). |

### Selection mode options

- **Show Tooltip On Selection** – Show the tooltip for the currently focused skill node.
- **Selection South Enters Inner Buttons** (optional):
  - **On** – **South (A / Enter)** on a node moves focus *into* that node (Unlock / Level Up / Refund buttons). Use d-pad to move between those buttons, then South to press one. **East (B / Escape)** returns focus to the node (back to navigating between nodes).
  - **Off** – South on a node only submits (e.g. select for hotbar); you do not “enter” the inner buttons.
- **North (Y / Triangle)** – When a skill node is focused (or one of its inner buttons), North runs the **active** inner button for that node (Unlock if lockable, otherwise Level Up, otherwise Refund). Each **Skill Item UI** has a checkbox **North Clicks Active Button** to enable or disable this.

So in Selection you can either:
- Use **South** to enter the node and navigate its buttons (Unlock / Level Up / Refund), then **East** to exit; or  
- Use **North** to press the current “active” button directly (no entering), and **South** to select the skill for the hotbar.

### Other options (Skill Tree UI)

- **Hide Cursor On Gamepad** – Hides the system mouse cursor while gamepad is used; shows it again when the mouse is used. 
- **Cursor Speed** (Cursor mode) – Speed of the virtual cursor in pixels per second.
- **Cursor Graphic** (Cursor mode) – Optional image used as the virtual cursor; if unset, a simple dot is created at runtime.

### Per-node (Skill Item UI)

- **North Clicks Active Button** – If on, North (Y/Triangle) on this node clicks whichever inner button is currently active (Unlock → Level Up → Refund). South always selects the skill for the hotbar.

---

## 💾 Save & Load

The system integrates with Game Creator 2's save system:

### Automatic Saving
```
Instruction: Save Game
  - Profile: [Your Profile]
```

This automatically saves:
- Unlocked skills
- Skill levels
- Current skill points
- Hotbar configuration

### Loading
```
Instruction: Load Game
  - Profile: [Your Profile]
```

Everything is restored automatically!

---

## 🔧 Advanced Features

### Object Pooling
Skill instances are automatically pooled for optimal performance. No configuration needed!

### Custom Unlock Conditions
Use any Game Creator 2 condition:
- Player level requirements
- Quest completion
- Item possession
- Custom variables
- And more!

### Specific Level Conditions
Define different requirements for each skill level:
```
Level 1: No requirements
Level 2: Requires 100 mana
Level 3: Requires quest "Dragon Slayer" complete
```

### Event System
Subscribe to events in your own scripts:
```csharp
skillTreeComponent.OnSkillUnlocked += (skill) => {
    Debug.Log($"Unlocked: {skill.SkillName}");
};
```

---

## 📚 Visual Scripting Reference

### Events (Triggers)
- **On Skill Unlocked** - When a skill is unlocked
- **On Skill Level Up** - When a skill levels up
- **On Skill Used** - When a skill is used/cast
- **On Skill Points Changed** - When skill points change
- **On Skill Cooldown Start** - When cooldown begins
- **On Skill Cooldown End** - When cooldown finishes

### Conditions
- **Is Skill Unlocked** - Check if skill is unlocked
- **Can Unlock Skill** - Check if skill can be unlocked now
- **Is Skill On Cooldown** - Check cooldown state
- **Is Skill Max Level** - Check if skill is maxed
- **Has Skill Points** - Check if player has enough points
- **Compare Skill Level** - Compare skill level (==, !=, >, >=, <, <=)
- **Compare Skill Points** - Compare skill points (==, !=, >, >=, <, <=)
- **Is Skill In Hotbar** - Check if skill is assigned to hotbar
- **Is Hotbar Slot Empty** - Check if hotbar slot is empty

### Instructions

#### Skill Management
- **Unlock Skill** - Unlock a specific skill
- **Level Up Skill** - Increase skill level
- **Use Skill** - Use/cast a skill
- **Set Skill Level** - Set skill to specific level
- **Refund Skill** - Refund a skill and return points
- **Reset Skill Cooldown** - Clear cooldown
- **Reset All Skills** - Clear all progress (with optional refund)

#### Skill Points
- **Add Skill Points** - Give skill points
- **Set Skill Points** - Set points to specific amount
- **Remove Skill Points** - Take away points

#### VFX & Spawning
- **Instantiate Multiple** - Spawn multiple objects at once
- **Instantiate Multiple With Delay** - Spawn with delay between each
- **Spawn In Formation** - Spawn in formations (V, Line, Square, Hexagon, Triangle, Diamond, Circle, Wedge)
- **Spawn Persistent Zone** - Create zones with tick effects (damage zone, heal zone, buff zone) with optional parent attachment

#### Combat
- **Execute In Radius** - Execute instructions on all targets in radius
- **Pull Enemies To Position** - Black hole effect that pulls enemies
- **Vortex Pull** - Hurricane/tornado effect with spiral pull, spin force, and separation

#### Skill Indicator (Ground AOE Preview)
- **Show Skill Indicator** - Shows the ground indicator for a skill (follows cursor)
- **Hide Skill Indicator** - Hides the skill indicator

### Skill Indicator System

For ground-targeted skills (AOE, cones, lines), use the Skill Indicator:
1. Add **Skill Indicator Config** to your Skill (Circle, Cone, Line, Expanding Circle or Expanding Line with radius/range).
2. Create a Skill Indicator: `GameObject > KingEdward > Skill Tree > Create Skill Indicator` 
3. In your skill's Sequencer (optional): you can still use **Show Skill Indicator** / **Hide Skill Indicator** if you prefer fully scripted control.
4. In **Execute In Radius** (or similar): set Position to **Skill Indicator Target** to use cursor ground position (provided by `SkillIndicatorController`).

#### UI
- **Show Unlock Confirmation** - Show confirmation panel for unlock
- **Show Refund Confirmation** - Show confirmation panel for refund

### Properties
- **Get Skill Value** - Get skill data (level, cooldown, etc.)
- **Skill Indicator Target** - Get current indicator/cursor ground position (for ground-targeted skills, comes from `SkillIndicatorController.LastTargetPosition`)
- **Skill Indicator Radius** - Get the current radius of the indicator (for expanding circles/lines, comes from `SkillIndicatorController.LastRadius`)
- **Get Skill Tree Value** - Get tree stats (unlocked count, etc.)
- **Check Skill Level** - Compare skill level
- **Last Vortex** - Get the last created Vortex Pull GameObject
- **Last Pull** - Get the last created Pull Enemies GameObject
- **Projectile Caster** - Get the GameObject that cast a projectile (WORKS ONLY WITH INSTANTIATE INSTRUCTIONS THAT COMES WITH SKILL SYSTEM)

---

## 🐛 Troubleshooting

### Skills not appearing in UI
- Check that skills are added to the Skill Tree Data
- Verify Skill Tree Component has the correct Skill Tree Data assigned
- Ensure UI prefabs are properly linked

### Hotbar not working
- Verify Player GameObject has "Player" tag
- Check that Skill Tree Component reference is set in SkillHotbarUI
- Ensure skills are unlocked before assigning to hotbar

### Save/Load not working
- Confirm you're using Game Creator 2's save system
- Check that SkillTreeMemory component is in the scene
- Verify save profile is correctly configured

### Cooldowns not resetting
- Check that the skill has a cooldown duration set
- Verify the Skill Tree Component is active
- Try using "Reset Skill Cooldown" instruction

### Animation not playing
- Ensure "Use Sequencer" is enabled on the skill
- Verify Animation Clip is assigned
- Check that the character has a Character component
- Confirm Gesture system is working

---

## 📞 Support

- **Documentation**: See `QUICK_START.md` for detailed tutorials
- **E-mail**: kingedwardstudioscontact@gmail.com

---

## 🔄 Changelog

### Version 1.0.0
- Initial release
- Complete skill tree system
- Visual scripting integration (24 elements)
- Projectile system with 9 behaviors (Straight, Curve, Wave, Zig Zag, Spiral, Boomerang, Homing, Orbit, Artillery)
- Area of Effect, Effect Over Time, Gravity Pull, Hurricane
- Animation sequencer
- Save/load support
- UI system with hotbar
- Object pooling
- Debug system

### Version 1.0.1
- Fixed input system inconsistency  
- Fixed problems while using projectiles with Pooling
- Fixed registration of Projectile Caster (must use the skill system's instantiates)
- Added Gamepad/Mobile Support with different options for navigation and clicking buttons
- Added an integration for Crystal Save Professional Save System
---

### Version 1.1.0
- Added Skill Stacks
- Added Indicator System
- Added Charge State while aiming
- Added Chanelling mode for continuous skills
- Improved Skill Hotbar UI:
  - Two indicator input modes:
    - **HoldAndRelease** – hold key to aim, release to cast.
    - **DualStage** – first press shows indicator, second press casts.
  - Added stack counter text per slot to visualize remaining uses before cooldown.
- Improved Skill Editor for better QoL
- Fixed Avatar Mask issue on Skill.cs
---

## 📄 License

This asset is licensed for use with Game Creator 2 and Unity. 

---

## 🙏 Credits

Created by KingEdward
Built for Game Creator 2 by Catsoft Studios

---

## 🚀 What's Next?

Now that you have the basics, explore:
1. Create your first complete skill tree (10-15 skills)
2. Set up prerequisites and unlock conditions
3. Add animations using the sequencer
4. Customize the UI to match your game
5. Test save/load functionality
6. Build your game!

**Happy skill tree building!** 🎮✨
