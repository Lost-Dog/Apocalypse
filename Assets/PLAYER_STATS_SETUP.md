# Player Stats Display Setup Guide

## Overview
Individual display scripts for each player stat:
- **Health** (HP) - `PlayerHealthDisplay.cs`
- **Experience** (XP) - `PlayerXPDisplay.cs`
- **Level** - `PlayerLevelDisplay.cs`
- **Temperature** - `PlayerTemperatureDisplay.cs` + `SurvivalManager.cs`
- **Stamina** - `PlayerStaminaDisplay.cs`
- **Infection** - `PlayerInfectionDisplay.cs`

---

## Quick Setup (3 Steps)

### Step 1: Add Script to UI Panel
1. Select `/UI/HUD/ScreenSpace/Bottom/Panel` in Hierarchy
2. Click **Add Component**
3. Search for **PlayerStatsDisplay**
4. Add the script

### Step 2: Create UI Text Elements
Inside the Panel, create these TextMeshProUGUI elements:
- `Text_Health` - displays health
- `Text_XP` - displays XP
- `Text_Level` - displays level
- `Text_Temperature` - displays temperature
- `Text_Stamina` - displays stamina
- `Text_Infection` - displays infection

**Quick Create:**
- Right-click Panel → UI → Text - TextMeshPro
- Rename and position as needed

### Step 3: Assign References
1. Select the Panel with PlayerStatsDisplay
2. In the Inspector, drag the text elements:
   - `Text_Health` → **Health Text**
   - `Text_XP` → **XP Text**
   - `Text_Level` → **Level Text**
   - `Text_Temperature` → **Temperature Text**
   - `Text_Stamina` → **Stamina Text**
   - `Text_Infection` → **Infection Text**
3. Check **Auto Find References** ✓

---

## Advanced Setup (Optional Sliders)

### Add Visual Bars
For each stat, you can add a Slider:
1. Right-click Panel → UI → Slider
2. Rename (e.g., `Slider_Health`)
3. Drag to the corresponding slider field in PlayerStatsDisplay

**Slider Fields:**
- Health Slider
- XP Slider
- Temperature Slider
- Stamina Slider
- Infection Slider

---

## Configuration

### Auto-Find References
- **Enabled** (default): Automatically finds Player and ProgressionManager
- **Disabled**: Manually assign references

### Survival Stats Settings

**Temperature:**
- Normal: 37°C (body temperature)
- Range: 20°C - 42°C
- Status: Hypothermia → Cold → Normal → Warm → Fever → Critical

**Stamina:**
- Max: 100
- Regen Rate: 5/sec (when not running)
- Drain Rate: 10/sec (when running)

**Infection:**
- Max: 100%
- Growth Rate: 0.5%/sec (when infected)
- Decay Rate: 1%/sec (natural recovery)
- Status: None → Mild → Moderate → Severe → Critical

---

## Example UI Layout

```
Panel
├── Text_Health       → "HP: 85/100"
├── Slider_Health     → Visual bar
├── Text_XP           → "XP: 450/600"
├── Slider_XP         → Visual bar
├── Text_Level        → "Level: 4"
├── Text_Temperature  → "Temp: 37.2°C (Normal)"
├── Slider_Temp       → Visual bar
├── Text_Stamina      → "Stamina: 82/100"
├── Slider_Stamina    → Visual bar
├── Text_Infection    → "Infection: 0% (None)"
└── Slider_Infection  → Visual bar
```

---

## Using the Stats in Code

### Add Infection
```csharp
PlayerStatsDisplay statsDisplay = FindFirstObjectByType<PlayerStatsDisplay>();
statsDisplay.AddInfection(25f); // Add 25% infection
```

### Modify Temperature
```csharp
statsDisplay.ModifyTemperature(-2f); // Decrease by 2°C
statsDisplay.ModifyTemperature(3f);  // Increase by 3°C
```

### Drain Stamina
```csharp
statsDisplay.DrainStamina(20f); // Remove 20 stamina
```

---

## Stats Explained

### Health (HP)
- Source: `JUHealth` component on Player
- Shows current/max health
- Updates automatically when player takes damage

### Experience & Level
- Source: `ProgressionManager` component
- Shows current XP and required XP for next level
- Shows current player level
- XP bar fills as you gain experience

### Temperature
- **Simulated stat** (not connected to game systems yet)
- Body temperature in Celsius
- Slowly returns to normal (37°C)
- Can be modified by weather, environment, etc.

### Stamina
- **Simulated stat** (not connected to game systems yet)
- Energy for sprinting/running
- Drains when running
- Regenerates when not running
- Uses `JUCharacterController.IsRunning` to detect sprinting

### Infection
- **Simulated stat** (not connected to game systems yet)
- Disease/poison level (0-100%)
- Naturally decays over time
- Can be increased by enemy attacks, environment, etc.

---

## Customization

### Change Colors
Use TextMeshPro color settings or add code:
```csharp
if (currentInfection > 50f)
{
    infectionText.color = Color.red;
}
else if (currentInfection > 25f)
{
    infectionText.color = Color.yellow;
}
```

### Change Update Frequency
Modify the `Update()` method to update less frequently:
```csharp
private float updateTimer;
private void Update()
{
    updateTimer += Time.deltaTime;
    if (updateTimer >= 0.5f) // Update every 0.5 seconds
    {
        UpdateAllDisplays();
        updateTimer = 0f;
    }
}
```

### Add New Stats
1. Add a field for the stat value
2. Add a TextMeshProUGUI field
3. Add an optional Slider field
4. Create an Update method
5. Call it in Update()

---

## Troubleshooting

**Stats not showing:**
- Check that text elements are assigned in Inspector
- Verify Auto Find References is checked
- Ensure Player has tag "Player"

**Health shows 0:**
- Player might not have JUHealth component
- Manually assign playerHealth field

**XP doesn't update:**
- ProgressionManager might not exist in scene
- Check `/GameSystems/PlayerProgressionManager` exists

**Stamina doesn't drain:**
- Player might not have JUCharacterController
- Check running detection logic

---

## Next Steps

**Connect Stamina to Gameplay:**
- Prevent sprinting when stamina is 0
- Add stamina cost to special abilities

**Connect Temperature to Weather:**
- Decrease temperature in cold areas
- Increase temperature near fire

**Connect Infection to Combat:**
- Add infection on zombie hits
- Create infection damage over time
- Add medicine items to cure infection

---

**Created:** PlayerStatsDisplay.cs  
**Location:** /Assets/Scripts/PlayerStatsDisplay.cs  
**UI Target:** /UI/HUD/ScreenSpace/Bottom/Panel
