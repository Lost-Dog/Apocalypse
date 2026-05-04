# Skill Tree System - Roadmap

## 🎯 Future Updates

This document outlines planned features for future updates. These features are not yet implemented but are planned based on community feedback and common use cases.

---

## ✅ Version 1.1 (Current)

### ⚡ Skill Stacks
Skills can have multiple uses before cooldown starts (stack window behavior).

**Use Cases:**
- Abilities with multiple uses before cooldown
- Dash abilities with multiple uses
- Consumable skill uses

**Example:**
```
Dash Skill:
- Stack Uses Before Cooldown: 3
- (Optional) Stack Window Duration: 2s
```

**Benefits:**
- More dynamic gameplay
- Common in MOBAs and action games
- Adds strategic depth

---

### 🎯 Skill Indicator + Charge State 
Show a ground indicator while aiming, optionally playing a charge State during aim.

**Use Cases:**
- AOE targeting (circle/cone/line indicators)
- Charged attacks while holding aim
- Better telegraphing for player skills

**Example:**
```
Meteor:
- Indicator: Circle (expand while holding)
- Charge State: Play while aiming
- Cast: On release
```

**Benefits:**
- Better aiming and feedback
- Supports charged skills cleanly
- Works with keyboard/gamepad via Input System bindings

---

### 🔁 Skill Channeling
Channelled skills keep running while input/conditions remain true, executing On Channel Tick each frame.

**Use Cases:**
- Beam/laser skills
- Continuous AOE zones
- Drain/heal-over-time while holding input

**Example:**
```
Flamethrower:
- Is Channel Skill: true
- On Channel Tick: spawn/attack every frame
- Stop: when input released or conditions fail
```

**Benefits:**
- Enables sustained skills such as Comet Azur
- Integrates with the same hotbar input flow (Start/Cancel)

---

## 📅 Version 1.2 (Planned)

### 🏷️ Skill Tags/Categories
Organize skills with tags for filtering, UI, and tag-based bonuses/conditions.

**Use Cases:**
- "Fire", "Ice", "Lightning" elemental tags
- "AOE", "Single Target", "Buff" type tags
- Tag-based bonuses and conditions

**Example:**
```
Fireball:
- Tags: Fire, Projectile, Damage, AOE

Ice Shield:
- Tags: Ice, Defensive, Buff

Passive: "Fire Mastery"
- If skill has "Fire" tag → +20% damage
```

**Benefits:**
- Better organization
- Tag-based buffs/conditions
- Build themes (all Fire skills, all AOE, etc)

---

### 🦋 Skill Evolution/Morph
Skills transform into more powerful versions at specific levels.

**Use Cases:**
- Pokemon-style evolution
- MOBA ultimate transformations (Kayle, Kha'Zix)
- Prestige/Mastery systems

**Example:**
```
Fireball (Level 1-4)
  ↓ At Level 5
Inferno Blast (Level 5+)
  - New icon
  - New effects
  - More damage
```

**Benefits:**
- Visual progression
- Exciting milestones
- Unique feature

---

## 📅 Version 1.3 (Planned)

### 🔗 Skill Synergies
Skills become more powerful when other specific skills are unlocked.

**Use Cases:**
- Fire skills deal more damage if Ice skills are unlocked
- Combo bonuses between related skills
- Build diversity and specialization

**Example:**
```
Fireball Synergy:
- If "Fire Mastery" unlocked → +20% damage
- If "Meteor" unlocked → Chance to spawn meteor on hit
```

**Benefits:**
- Encourages build diversity
- Rewards specialization
- Increases replayability

---

## 📅 Version 1.3.1 (Planned)

### 🎨 New UI Sprite Sets
Additional visual styles for skill tree UI.

**Includes:**
- 2 new complete sprite sets
- Alternative visual themes
- Easy prefab swap

**Benefits:**
- More visual variety
- Match different game aesthetics
- No code changes needed

---

## 💬 Community Requests

Have a feature request? Let us know!

**Contact:**
- Email: kingedwardstudioscontact@gmail.com
- Discord: [Community Server]
- Asset Store Reviews

---

## 📝 Notes

- Features are subject to change based on feedback
- Release dates are estimates
- Some features may be combined or split across updates
- Free updates for all existing customers

---

**Current Version: 1.1**
**Last Updated: March 2026**
