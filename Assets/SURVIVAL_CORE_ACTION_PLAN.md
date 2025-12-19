## Survival Core Mechanics - Action Plan

**Goal:** Build a compelling survival experience focused on core survival needs before adding secondary mechanics.

---

## Current Survival Systems ✅

You already have these survival systems implemented:

### **1. Temperature System**
- Body temperature decreases over time
- Cold zones accelerate temperature loss
- Indoor spaces and fires provide warmth
- Critical cold causes damage
- Players can freeze to death

### **2. Infection System**
- Contaminated zones (fog) infect player
- Infection scales with exposure time
- Continues spreading after leaving zone
- High infection causes health damage
- Only cured in safe zones

### **3. Stamina System**
- Running drains stamina
- Cold conditions drain stamina faster
- Regenerates when resting
- Affects movement ability

### **4. Health System**
- Combat damage
- Environmental damage (cold, infection)
- Healing mechanics
- 10% health restore on kill

---

## Missing Critical Survival Systems ❌

### **Priority 1: Hunger & Thirst (ESSENTIAL)**

**Why critical:** Core survival loop - players MUST eat/drink to survive

**Implementation:**
```
HungerThirstSystem
├─ Hunger (0-100%)
│  ├─ Decreases over time (slower than temp)
│  ├─ Accelerates when running/fighting
│  ├─ Below 30%: Stamina regen reduced
│  ├─ Below 10%: Health starts decreasing
│  └─ 0%: Death
│
└─ Thirst (0-100%)
   ├─ Decreases faster than hunger
   ├─ Accelerates when running/hot
   ├─ Below 30%: Stamina regen reduced
   ├─ Below 10%: Health starts decreasing
   └─ 0%: Death
```

**Resource System:**
- Consumable items (food, water, medicine)
- Found via looting/scavenging
- Limited inventory space
- Different items restore different amounts

---

### **Priority 2: Day/Night Cycle (HIGH IMPACT)**

**Why important:** Creates tension, forces strategic planning

**Implementation:**
```
DayNightCycle
├─ 24-minute real-time cycle (1 min = 1 hour)
├─ Dynamic lighting
├─ Time-based events:
│  ├─ Day (6am-6pm): Safer, better visibility
│  ├─ Dusk (6pm-8pm): Enemies start spawning
│  ├─ Night (8pm-6am): More enemies, less visibility
│  └─ Dawn (4am-6am): Enemies retreat
│
└─ Survival impacts:
   ├─ Night: Faster temperature loss
   ├─ Night: Increased stamina drain
   └─ Night: 2x enemy spawns
```

**Strategic Depth:**
- Players must plan around day/night
- Find shelter before dark
- Risk vs reward for night scavenging
- Safe zones more valuable at night

---

### **Priority 3: Resource Scavenging Loop (CORE GAMEPLAY)**

**Why critical:** Drives exploration and risk-taking

**You have:** Loot boxes, pickup system
**Need to add:** 
1. **Resource types:**
   - Food (canned goods, rations, fresh food)
   - Water (bottles, purification tablets)
   - Medicine (healing, antibiotics for infection)
   - Warmth (clothing, fuel, blankets)
   - Ammo (already have)
   
2. **Scarcity and choice:**
   - Limited carry capacity
   - Must choose what to take
   - Different areas have different loot
   
3. **Risk/reward locations:**
   - Safe areas: Low loot, no enemies
   - Contaminated zones: Good loot, infection risk
   - Enemy patrols: Best loot, combat risk
   - Night exploration: Great loot, high danger

---

## Recommended Implementation Order

### **Phase 1: Hunger & Thirst (Week 1)**

**Step 1.1 - Core System (2 days)**
- Create `HungerThirstManager.cs`
- Integrate with `SurvivalManager`
- Add decrease over time
- Add death conditions
- Add UI bars/indicators

**Step 1.2 - Consumable Items (2 days)**
- Extend existing pickup system
- Add food/water item types
- Add consumption mechanics
- Add inventory integration
- Test different item values

**Step 1.3 - Balance & Polish (1 day)**
- Tune depletion rates
- Adjust item spawn rates
- Test survival difficulty
- Add audio/visual feedback

### **Phase 2: Day/Night Cycle (Week 2)**

**Step 2.1 - Time System (2 days)**
- Create `TimeManager.cs`
- Implement 24-hour cycle
- Add time progression
- Add time display UI
- Hook to directional light

**Step 2.2 - Lighting & Atmosphere (2 days)**
- Dynamic sun rotation
- Skybox transitions
- Fog density changes
- Post-processing adjustments
- Sound ambience per time

**Step 2.3 - Gameplay Integration (1 day)**
- Night temperature penalties
- Increased enemy spawns at night
- Loot quality by time of day
- Safe zone benefits at night

### **Phase 3: Enhanced Resource Loop (Week 3)**

**Step 3.1 - Resource Variety (2 days)**
- Add 10+ food types with different values
- Add 5+ water sources
- Add medicine types (healing, cure infection, stamina boost)
- Add warmth items (jackets, fuel)
- Configure loot tables

**Step 3.2 - Strategic Scarcity (2 days)**
- Reduce overall loot spawn rates
- Create loot zones by area type
- Add inventory weight/limits
- Force meaningful choices
- Balance risk vs reward

**Step 3.3 - Survival Loop Testing (1 day)**
- Full gameplay loop test
- Adjust all timers and rates
- Ensure 30-60 min gameplay sessions feel good
- Tune difficulty curve

---

## Core Survival Loop (Target Experience)

```
START GAME
  ↓
Player spawns with minimal supplies (50% hunger, 60% thirst)
  ↓
Explore nearby buildings for food/water (5-10 min)
  ↓
Manage temperature (find fire/shelter when cold)
  ↓
Encounter enemies → Combat → Use resources
  ↓
Hunger/thirst depleting → MUST find more supplies
  ↓
Night approaching → Find safe zone or risk night
  ↓
Decision: Sleep/wait or scavenge in dark?
  ↓
Repeat cycle with escalating challenge
```

**Session Duration:** 30-60 minutes before death or reaching objective

---

## Success Metrics

After Phase 3, player should experience:

✅ **Constant pressure** - Always managing at least 2-3 needs
✅ **Meaningful choices** - "Do I eat now or save for later?"
✅ **Risk vs reward** - "Is that loot worth the infection risk?"
✅ **Time pressure** - "I need to find shelter before dark"
✅ **Resource scarcity** - "I can only carry 3 items, which ones?"
✅ **Survival satisfaction** - "I survived 2 days in the apocalypse!"

---

## What to SKIP for Now

These are good mechanics but NOT core survival:

❌ **Crafting system** - Can add later
❌ **Base building** - Secondary to survival
❌ **Vehicle system** - Already have, don't expand yet
❌ **Quest/mission complexity** - Keep simple
❌ **Character progression** - Already have XP
❌ **Weapon variety** - Existing weapons sufficient
❌ **Complex AI behaviors** - Current AI is fine
❌ **Multiplayer** - Far future

---

## Technical Implementation

### Integration Points

**SurvivalManager.cs** - Central hub for:
- Temperature (existing ✅)
- Stamina (existing ✅)
- Infection (existing ✅)
- **Hunger (add next)**
- **Thirst (add next)**

**Existing Systems to Leverage:**
- `LootableBox.cs` - Add food/water drops
- `ItemPickup.cs` - Add consumption items
- `PlayerStatsDisplay.cs` - Add hunger/thirst UI
- `SafeZone.cs` - Make it cure infection + provide safety

### Code Architecture

```csharp
SurvivalManager (singleton)
├─ TemperatureSystem (exists)
├─ StaminaSystem (exists)
├─ InfectionSystem (exists)
├─ HungerSystem (new)
├─ ThirstSystem (new)
└─ UpdateAllSystems() - called every frame

TimeManager (new singleton)
├─ CurrentTime (0-24 hours)
├─ TimeScale (how fast time passes)
├─ Events: OnDawn, OnDusk, OnNight, OnDay
└─ UpdateSunRotation()

ConsumableItem (new class)
├─ ItemType (Food/Water/Medicine)
├─ RestoreAmount
├─ Use() method
└─ Integrates with existing pickup system
```

---

## Next Steps (Immediate Actions)

1. **Exit Play Mode** (if in Play Mode)

2. **Complete Dynamic Zone conversion:**
   - Menu: `Division Game → Convert to Dynamic Zone System`
   - Set default enemy prefab
   - Run conversion

3. **Start Phase 1 - Hunger/Thirst:**
   - Confirm you're ready
   - I'll create the HungerThirstManager
   - Integrate with SurvivalManager
   - Add UI and consumable items
   - Test survival pressure

**Ready to start Phase 1?** The hunger/thirst system will create the core survival tension!
