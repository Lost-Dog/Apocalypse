# World Space Health Bar - Quick Start

## ⚡ 30-Second Setup

### Step 1: Open Tool

```
Menu: Tools > Character Health Bar Setup
```

### Step 2: Setup

```
1. Drag your character to "Character Prefab/GameObject"
2. (Optional) Enter name and level
3. Click "Add Health Bar to Character"
```

### Step 3: Done!

```
✓ Health bar added!
✓ Auto-configured!
✓ Ready to use!
```

---

## 🎯 What You Get

```
✓ Integrates with JUHealth component
✓ Uses ApocalypseHUD visuals (optional)
✓ Shows only when damaged
✓ Auto-hides after 3 seconds
✓ Faces camera automatically
✓ Color-coded health (green/yellow/red)
✓ Distance culling (30m default)
```

---

## 📦 Available Prefabs

### ApocalypseHUD (Professional)

Located: `/Assets/Synty/InterfaceApocalypseHUD/Prefabs/NPC_HealthBars_EnemyData/`

```
HUD_Apocalypse_WorldSpace_EnemyInfo_01.prefab
├── Health bar
├── Name display
├── Level badge
└── Professional styling
```

**Load in tool:** Click "Create ApocalypseHUD Health Bar Prefab"

### Simple (Minimalist)

**Create in tool:** Click "Create Simple Health Bar Prefab"

```
Simple health bar
├── Clean slider design
├── Color-coded
└── Lightweight
```

---

## ⚙️ Key Settings

### Show Only When Damaged
```
✓ Check: Hides when full HP
  Uncheck: Always visible
```

### Hide Delay
```
Default: 3 seconds
How long to show after damage
```

### Max Visible Distance
```
Default: 30 meters
Culling distance for performance
```

### World Offset
```
Default: (0, 2.5, 0)
Height above character head
```

---

## 🔧 Quick Adjustments

### Make Always Visible (Boss)

```
1. Select health bar GameObject
2. Find WorldSpaceHealthBar component
3. Check "Always Show"
```

### Change Height

```
1. Select health bar GameObject
2. Find "World Offset"
3. Change Y value (2.5 = default)
```

### Change Colors

```
Full Health Color: Green (HP > 60%)
Mid Health Color: Yellow (HP 30-60%)
Low Health Color: Red (HP < 30%)
```

---

## 📋 Requirements

Your character must have:

- [ ] JUHealth component ← Required!
- [ ] Transform (obviously!)

That's it!

---

## 🐛 Quick Fixes

### Not Showing?

```
1. Check "Always Show" is enabled (test)
2. Verify JUHealth component exists
3. Check character is within 30m of camera
4. Damage character to trigger visibility
```

### Wrong Height?

```
Adjust "World Offset" Y value:
- Taller character: 3.0-3.5
- Normal character: 2.5 (default)
- Shorter character: 2.0
```

### Wrong Size?

```
Select Canvas child:
- Increase "Size Delta" for larger
- Decrease for smaller
- Keep scale at 0.01
```

---

## 📊 Common Configurations

### Enemy

```
Show Only When Damaged: ✓
Hide Delay: 3 sec
Max Distance: 40m
Name: "Enemy Type"
Level: Enemy level
```

### Boss

```
Always Show: ✓
Hide Delay: N/A
Max Distance: 100m
Name: "BOSS NAME"
Level: Boss level
```

### Civilian

```
Show Only When Damaged: ✓
Hide Delay: 2 sec
Max Distance: 20m
No name/level (minimal)
```

---

## ✅ Done!

Your characters now have professional health bars!

**Test:**
1. Enter Play Mode
2. Damage a character
3. Watch health bar appear
4. After 3 seconds, it disappears

**Customize:** Select health bar GameObject and adjust settings in Inspector.

---

**Full Guide:** See `WORLD_SPACE_HEALTHBAR_GUIDE.md` for complete documentation.
