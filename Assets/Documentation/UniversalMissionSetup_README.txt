╔══════════════════════════════════════════════════════════════════╗
║           UNIVERSAL MISSION SETUP SYSTEM - COMPLETE GUIDE        ║
╚══════════════════════════════════════════════════════════════════╝

QUICK START
===========

1. Open the Tool
   └─ Division Game → Challenge System → Universal Mission Setup

2. Select Mission Type
   └─ Choose from 7 different types

3. Configure Spawns
   └─ Set enemy/civilian/objective counts and prefabs

4. Choose Layout
   └─ Circle, Grid, Line, or Random

5. Create Mission Zone
   └─ Click "Create Mission Zone" button


MISSION TYPES
=============

┌─ SUPPLY DROP ────────────────────────────────────────┐
│  • Enemies guarding loot crates                      │
│  • Kill enemies and collect supplies                 │
│  • Recommended: 5-8 enemies, 1-2 loot boxes         │
└──────────────────────────────────────────────────────┘

┌─ CIVILIAN RESCUE ────────────────────────────────────┐
│  • Save civilians from enemy attack                  │
│  • Protect civilians from harm                       │
│  • Recommended: 6 enemies, 4-5 civilians            │
└──────────────────────────────────────────────────────┘

┌─ CONTROL POINT (Zone Control) ──────────────────────┐
│  • Capture and hold territory                        │
│  • Clear enemies and secure zone                     │
│  • Recommended: 5-7 enemies per zone                │
└──────────────────────────────────────────────────────┘

┌─ HOSTAGE RESCUE ─────────────────────────────────────┐
│  • Rescue hostages without casualties                │
│  • Precision and stealth required                    │
│  • Recommended: 4-6 enemies, 2-3 hostages           │
└──────────────────────────────────────────────────────┘

┌─ EXTRACTION DEFENSE ─────────────────────────────────┐
│  • Defend extraction point against waves             │
│  • Survive enemy assault                             │
│  • Recommended: 10+ enemies in waves                │
└──────────────────────────────────────────────────────┘

┌─ BOSS ENCOUNTER ─────────────────────────────────────┐
│  • Elite enemy boss fight                            │
│  • High difficulty, high rewards                     │
│  • Recommended: 1 boss + 6-10 guards                │
└──────────────────────────────────────────────────────┘

┌─ RIVAL AGENT ────────────────────────────────────────┐
│  • Rogue Division agent duel                         │
│  • Tactical 1v1 combat                               │
│  • Recommended: 1 agent + 0-4 backup                │
└──────────────────────────────────────────────────────┘


SPAWN LAYOUTS
=============

[CIRCLE]
   Enemy      Enemy
      \        /
       \ Zone /
        \    /
   Enemy - ○ - Enemy
        /    \
       /      \
   Enemy      Enemy

[GRID]
   Enemy  Enemy  Enemy
   Enemy  Zone   Enemy
   Enemy  Enemy  Enemy

[LINE]
   Enemy Enemy Enemy Zone Enemy Enemy

[RANDOM]
   Enemy scattered randomly within zone radius


SPAWN CATEGORIES
================

🔴 ENEMY
   - Regular hostile NPCs
   - Auto-links to challenge kill tracking
   - Prefab needs JUHealth component

🔥 BOSS
   - Elite enemy, higher health/damage
   - Spawns at zone center
   - Prefab needs JUHealth component

🟢 CIVILIAN
   - Friendly NPCs to rescue
   - Links to civilian rescue tracking
   - Avoid harm for mission success

📦 LOOTBOX
   - Supply crates and loot containers
   - Interactable objectives
   - Rewards players on collection

🎯 OBJECTIVE
   - Mission-specific objects
   - Capture points, terminals, etc
   - Custom interaction logic

🛡️ COVER
   - Destructible/static cover
   - Environmental objects
   - Tactical positioning

🚗 VEHICLE
   - Cars, trucks, helicopters
   - Can be objectives or props
   - Optional interaction


WORKFLOW EXAMPLES
=================

┌─ EXAMPLE 1: Supply Drop Mission ────────────────────┐
│                                                      │
│  1. Select Type: Supply Drop                        │
│  2. Enemy Prefab: Rogue_AI                          │
│  3. Enemy Count: 6                                  │
│  4. Loot Box Prefab: SupplyCrate                   │
│  5. Loot Box Count: 2                               │
│  6. Layout: Circle                                  │
│  7. Zone Radius: 25m                                │
│  8. Click "Create Mission Zone"                     │
│                                                      │
│  Result:                                             │
│  • 6 enemies in circle around zone                  │
│  • 2 loot boxes in center                           │
│  • Visual marker on ground                          │
│  • Ready to spawn in game                           │
└──────────────────────────────────────────────────────┘

┌─ EXAMPLE 2: Boss Fight ──────────────────────────────┐
│                                                      │
│  1. Select Type: Boss Encounter                     │
│  2. Enemy Prefab: Elite_Guard                       │
│  3. Enemy Count: 8                                  │
│  4. Include Boss: YES                               │
│  5. Boss Prefab: Commander_Boss                     │
│  6. Layout: Circle                                  │
│  7. Zone Radius: 35m                                │
│  8. Click "Create Mission Zone"                     │
│                                                      │
│  Result:                                             │
│  • Boss at center                                   │
│  • 8 guards in circle                               │
│  • Larger combat area                               │
└──────────────────────────────────────────────────────┘

┌─ EXAMPLE 3: Civilian Rescue ─────────────────────────┐
│                                                      │
│  1. Select Type: Civilian Rescue                    │
│  2. Enemy Prefab: Rogue_AI                          │
│  3. Enemy Count: 6                                  │
│  4. Civilian Prefab: Friendly_Civilian              │
│  5. Civilian Count: 5                               │
│  6. Layout: Random                                  │
│  7. Zone Radius: 30m                                │
│  8. Click "Create Mission Zone"                     │
│                                                      │
│  Result:                                             │
│  • Enemies and civilians mixed randomly             │
│  • Tactical challenge (avoid civilian casualties)   │
└──────────────────────────────────────────────────────┘


CUSTOMIZING SPAWN POINTS
=========================

After creating a mission zone, you can manually adjust spawn points:

1. Find zone in Hierarchy
   └─ MissionZones/YourMissionName

2. Expand to see spawn points
   └─ SpawnPoints/Enemy_01, Enemy_02, etc.

3. Move spawn points to custom positions
   └─ Drag in scene view

4. Rotate spawn points for facing direction
   └─ Use rotation gizmo

5. Changes automatically sync to MissionZone component


INTEGRATION WITH CHALLENGE SYSTEM
==================================

Mission zones automatically integrate with challenges:

┌─ AUTOMATIC LINKING ──────────────────────────────────┐
│                                                      │
│  ChallengeManager spawns challenge                  │
│           ↓                                          │
│  ChallengeSpawner finds nearest MissionZone         │
│           ↓                                          │
│  Spawns prefabs at MissionZone spawn points         │
│           ↓                                          │
│  Links enemies/civilians to challenge tracking      │
│           ↓                                          │
│  Progress updates automatically                     │
│                                                      │
└──────────────────────────────────────────────────────┘


ADVANCED FEATURES
=================

┌─ Custom Spawn Point Settings ───────────────────────┐
│                                                      │
│  In MissionZone component:                          │
│                                                      │
│  Spawn Point:                                       │
│  ├─ Point Name: "Sniper_Rooftop"                   │
│  ├─ Category: Enemy                                 │
│  ├─ Prefab Override: Sniper_Enemy                  │
│  ├─ Use Custom Settings: ☑                         │
│  ├─ Require NavMesh: ☑                             │
│  ├─ Random Rotation: ☐                             │
│  ├─ Fixed Rotation: (0, 45, 0)                     │
│  └─ Priority: 10                                    │
│                                                      │
└──────────────────────────────────────────────────────┘


QUICK PRESETS
=============

The tool includes presets for common mission types:

[Supply Drop (Easy)]
• 5 enemies
• 1 loot box
• Circle layout
• 25m radius

[Civilian Rescue (Medium)]
• 6 enemies
• 4 civilians
• Random layout
• 30m radius

[Boss Fight (Hard)]
• 8 enemies
• 1 boss
• Circle layout
• 35m radius


TIPS & BEST PRACTICES
======================

✓ Use Circle layout for defensive missions
✓ Use Random layout for dynamic encounters
✓ Use Grid layout for structured combat
✓ Keep zone radius proportional to enemy count
✓ Place boss at zone center for focus
✓ Mix cover objects for tactical gameplay
✓ Test spawn positions in Play Mode
✓ Adjust spawn points manually for best results
✓ Use visual markers to see zone in scene
✓ Name zones clearly for organization


TROUBLESHOOTING
===============

Problem: Enemies not spawning
Solution: Check prefabs have proper AI components

Problem: Spawn points not showing in scene
Solution: Enable Gizmos in Scene view

Problem: Challenge doesn't link to zone
Solution: Ensure mission types match

Problem: Civilians dying immediately
Solution: Position away from crossfire

Problem: Can't see visual marker
Solution: Enable "Create Visual Marker" option


KEYBOARD SHORTCUTS
==================

None currently - use menu access


RELATED SYSTEMS
===============

• ChallengeManager - Spawns challenges
• ChallengeSpawner - Handles spawning logic
• ChallengeData - Challenge configuration
• ControlZone - Specific control point logic
• MissionZone - Universal mission setup


VERSION HISTORY
===============

v1.0 - Initial release
     - Universal mission setup for all types
     - Custom spawn point placement
     - Auto-layout generation
     - Visual zone markers


═══════════════════════════════════════════════════════
For more info, see Unity Pages in Bezi
═══════════════════════════════════════════════════════
