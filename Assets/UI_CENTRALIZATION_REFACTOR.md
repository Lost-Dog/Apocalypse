# UI Centralization Refactoring Guide

## 🎯 Goal
Centralize ALL UI updates through HUDManager for better organization, maintainability, and performance.

---

## 📋 Current Architecture Problems

### ❌ What's Wrong Now:
1. **Multiple scripts directly updating UI** - No central control
2. **Scripts polling in Update()** - Performance waste
3. **Direct references to UI elements** - Tight coupling
4. **No unified update pattern** - Inconsistent behavior

### Scripts That Need Refactoring:
- `PlayerHealthDisplay.cs` - Updates health UI in Update()
- `PlayerStatsDisplay.cs` - Updates 6 different stats in Update()
- `PlayerInfectionDisplay.cs` - Updates infection UI in Update()
- `PlayerTemperatureDisplay.cs` - Updates temperature UI in Update()
- `PlayerStaminaDisplay.cs` - Updates stamina UI in Update()
- `PlayerLevelDisplay.cs` - Updates level UI in Update()
- `PlayerXPDisplay.cs` - Updates XP UI in Update()
- `PlayerStatusIndicators.cs` - Updates warning indicators in Update()
- `PlayerStatusWarning.cs` - Shows warnings in Update()
- `PlayerSystemBridge.cs` - Bridges JUTPS health to HUD

---

## ✅ New Architecture

### Event-Driven Pattern:
```
Manager (e.g., SurvivalManager)
  └─→ Fires UnityEvent when value changes
       └─→ HUDManager listens to event
            └─→ Updates specific UI element
```

### Benefits:
- ✅ UI updates **only when needed** (not every frame)
- ✅ Single source of truth (HUDManager)
- ✅ Easy to disable/swap UI systems
- ✅ Better performance (no polling)
- ✅ Cleaner separation of concerns

---

## 🔧 Required Changes

### 1. Enhanced HUDManager
Add UI update methods for all player stats:

```csharp
// In HUDManager.cs

[Header("Player Stats UI")]
[SerializeField] private TextMeshProUGUI healthText;
[SerializeField] private Slider healthSlider;
[SerializeField] private TextMeshProUGUI xpText;
[SerializeField] private Slider xpSlider;
[SerializeField] private TextMeshProUGUI levelText;
[SerializeField] private TextMeshProUGUI temperatureText;
[SerializedField] private Slider temperatureSlider;
[SerializeField] private TextMeshProUGUI staminaText;
[SerializeField] private Slider staminaSlider;
[SerializeField] private TextMeshProUGUI infectionText;
[SerializeField] private Slider infectionSlider;

[Header("Status Indicators")]
[SerializeField] private PlayerStatusIndicators statusIndicators;
[SerializeField] private GameObject warningPanel;
[SerializeField] private TextMeshProUGUI warningText;

private SurvivalManager survivalManager;
private ProgressionManager progressionManager;
private JUHealth playerHealth;

// Initialize and subscribe to events
private void InitializePlayerStatsUI()
{
    // Find player
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        playerHealth = player.GetComponent<JUHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnDamaged.AddListener(OnPlayerHealthChanged);
        }
    }
    
    // Get managers
    survivalManager = SurvivalManager.Instance;
    if (survivalManager != null)
    {
        survivalManager.onTemperatureChanged.AddListener(OnTemperatureChanged);
        survivalManager.onStaminaChanged.AddListener(OnStaminaChanged);
    }
    
    progressionManager = gameManager.progressionManager;
    if (progressionManager != null)
    {
        progressionManager.onXPChanged.AddListener(OnXPChanged);
        progressionManager.onLevelUp.AddListener(OnLevelUp);
    }
    
    // Initial update
    UpdateAllPlayerStats();
}

// Health
private void OnPlayerHealthChanged(JUHealth.DamageInfo damageInfo)
{
    UpdateHealthDisplay();
}

public void UpdateHealthDisplay()
{
    if (playerHealth == null) return;
    
    if (healthSlider != null)
    {
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.Health;
    }
    
    if (healthText != null)
    {
        healthText.text = $"{Mathf.RoundToInt(playerHealth.Health)}/{Mathf.RoundToInt(playerHealth.MaxHealth)}";
    }
}

// Temperature
private void OnTemperatureChanged(float temperature)
{
    UpdateTemperatureDisplay(temperature);
}

public void UpdateTemperatureDisplay(float temperature)
{
    if (survivalManager == null) return;
    
    if (temperatureSlider != null)
    {
        temperatureSlider.maxValue = survivalManager.maxTemperature;
        temperatureSlider.value = temperature;
    }
    
    if (temperatureText != null)
    {
        float percentage = (temperature / survivalManager.maxTemperature) * 100f;
        temperatureText.text = $"{Mathf.RoundToInt(percentage)}%";
    }
}

// Stamina
private void OnStaminaChanged(float stamina)
{
    UpdateStaminaDisplay(stamina);
}

public void UpdateStaminaDisplay(float stamina)
{
    if (survivalManager == null) return;
    
    if (staminaSlider != null)
    {
        staminaSlider.maxValue = survivalManager.maxStamina;
        staminaSlider.value = stamina;
    }
    
    if (staminaText != null)
    {
        staminaText.text = $"{Mathf.RoundToInt(stamina)}/{Mathf.RoundToInt(survivalManager.maxStamina)}";
    }
}

// XP
private void OnXPChanged(int xp)
{
    UpdateXPDisplay(xp);
}

public void UpdateXPDisplay(int xp)
{
    if (progressionManager == null) return;
    
    if (xpSlider != null)
    {
        xpSlider.maxValue = progressionManager.GetXPForNextLevel();
        xpSlider.value = xp;
    }
    
    if (xpText != null)
    {
        xpText.text = $"{xp}/{progressionManager.GetXPForNextLevel()}";
    }
}

// Level
private void OnLevelUp(int newLevel)
{
    UpdateLevelDisplay(newLevel);
}

public void UpdateLevelDisplay(int level)
{
    if (levelText != null)
    {
        levelText.text = $"LVL {level}";
    }
}

// Infection (called by PlayerInfectionDisplay when infection changes)
public void UpdateInfectionDisplay(float infection, float maxInfection)
{
    if (infectionSlider != null)
    {
        infectionSlider.maxValue = maxInfection;
        infectionSlider.value = infection;
    }
    
    if (infectionText != null)
    {
        float percentage = (infection / maxInfection) * 100f;
        infectionText.text = $"{Mathf.RoundToInt(percentage)}%";
    }
}

// Warnings
public void ShowWarning(string message, Color color)
{
    if (warningPanel != null)
    {
        warningPanel.SetActive(true);
    }
    
    if (warningText != null)
    {
        warningText.text = message;
        warningText.color = color;
    }
}

public void HideWarning()
{
    if (warningPanel != null)
    {
        warningPanel.SetActive(false);
    }
}

// Status indicators (delegate to PlayerStatusIndicators if needed)
public void UpdateStatusIndicators()
{
    if (statusIndicators != null)
    {
        statusIndicators.ManualUpdate();
    }
}

// Refresh everything
public void UpdateAllPlayerStats()
{
    UpdateHealthDisplay();
    
    if (survivalManager != null)
    {
        UpdateTemperatureDisplay(survivalManager.currentTemperature);
        UpdateStaminaDisplay(survivalManager.currentStamina);
    }
    
    if (progressionManager != null)
    {
        UpdateXPDisplay(progressionManager.currentXP);
        UpdateLevelDisplay(progressionManager.currentLevel);
    }
}
```

---

### 2. Refactor PlayerHealthDisplay

**Before (polling):**
```csharp
private void Update()
{
    UpdateDisplay();
}
```

**After (event-driven):**
```csharp
// REMOVE Update() entirely
// Health updates now handled by HUDManager via JUHealth.OnDamaged event
// This script can be deleted or kept as a fallback
```

---

### 3. Refactor PlayerStatsDisplay

**Before (6 Update() calls per frame):**
```csharp
private void Update()
{
    UpdateHealthDisplay();
    UpdateXPDisplay();
    UpdateLevelDisplay();
    UpdateTemperatureDisplay();
    UpdateStaminaDisplay();
    UpdateInfectionDisplay();
}
```

**After (delegate to HUDManager):**
```csharp
// OPTION 1: Delete this script entirely (HUDManager handles it)

// OPTION 2: Keep as wrapper that calls HUDManager
private HUDManager hudManager;

private void Start()
{
    hudManager = FindFirstObjectByType<HUDManager>();
}

// Remove Update() - no polling needed
// Events will trigger HUDManager updates automatically
```

---

### 4. Refactor PlayerInfectionDisplay

**Before:**
```csharp
private void Update()
{
    if (currentInfection != lastInfection)
    {
        UpdateDisplay();
        lastInfection = currentInfection;
    }
}
```

**After:**
```csharp
private HUDManager hudManager;

private void Start()
{
    hudManager = FindFirstObjectByType<HUDManager>();
}

private void Update()
{
    // Still need to track infection (no event from SurvivalManager yet)
    if (currentInfection != lastInfection)
    {
        lastInfection = currentInfection;
        
        // Delegate to HUDManager
        if (hudManager != null)
        {
            hudManager.UpdateInfectionDisplay(currentInfection, maxInfection);
        }
        
        // Keep health damage logic here (separate concern)
        HandleInfectionDamage();
    }
}
```

---

### 5. Refactor SurvivalManager Events

**Add missing UnityEvents:**
```csharp
[Header("Events")]
public UnityEvent<float> onTemperatureChanged;
public UnityEvent<float> onStaminaChanged;
public UnityEvent<float> onInfectionChanged; // NEW
public UnityEvent onEnteredCriticalTemperature;
public UnityEvent onExitedCriticalTemperature;
public UnityEvent onStaminaDepleted;
public UnityEvent onInfectionCritical; // NEW
public UnityEvent onPlayerFroze;

// In SetTemperature():
if (!Mathf.Approximately(oldTemperature, currentTemperature))
{
    onTemperatureChanged?.Invoke(currentTemperature);
}

// In SetStamina():
if (!Mathf.Approximately(oldStamina, currentStamina))
{
    onStaminaChanged?.Invoke(currentStamina);
}

// NEW: In infection update:
public void SetInfection(float value)
{
    float oldInfection = currentInfection;
    currentInfection = Mathf.Clamp(value, 0f, maxInfection);
    
    if (!Mathf.Approximately(oldInfection, currentInfection))
    {
        onInfectionChanged?.Invoke(currentInfection);
    }
}
```

---

### 6. Refactor ProgressionManager Events

**Ensure events exist:**
```csharp
[Header("Events")]
public UnityEvent<int> onXPChanged;
public UnityEvent<int> onLevelUp;
public UnityEvent<int> onSkillPointsChanged;

// In AddXP():
currentXP += xp;
onXPChanged?.Invoke(currentXP);

// In LevelUp():
currentLevel++;
onLevelUp?.Invoke(currentLevel);
```

---

### 7. Refactor PlayerStatusIndicators

**Before:**
```csharp
private void Update()
{
    UpdateHealthIndicator();
    UpdateTemperatureIndicator();
    UpdateInfectionIndicator();
    UpdatePanelVisibility();
}
```

**After:**
```csharp
// Remove Update() - add public method for manual trigger
public void ManualUpdate()
{
    UpdateHealthIndicator();
    UpdateTemperatureIndicator();
    UpdateInfectionIndicator();
    UpdatePanelVisibility();
}

// HUDManager can call this when needed, or subscribe to events:
private void Start()
{
    if (survivalManager != null)
    {
        survivalManager.onTemperatureChanged.AddListener((temp) => UpdateTemperatureIndicator());
        survivalManager.onInfectionChanged.AddListener((inf) => UpdateInfectionIndicator());
    }
    
    if (playerHealth != null)
    {
        playerHealth.OnDamaged.AddListener((info) => UpdateHealthIndicator());
    }
}
```

---

### 8. Refactor PlayerStatusWarning

**Before:**
```csharp
private void Update()
{
    CheckHealth();
    CheckTemperature();
    CheckInfection();
    UpdateWarningDisplay();
}
```

**After:**
```csharp
private HUDManager hudManager;

private void Start()
{
    hudManager = FindFirstObjectByType<HUDManager>();
    
    // Subscribe to events
    if (playerHealth != null)
    {
        playerHealth.OnDamaged.AddListener((info) => CheckHealth());
    }
    
    if (survivalManager != null)
    {
        survivalManager.onTemperatureChanged.AddListener((temp) => CheckTemperature());
    }
}

private void CheckHealth()
{
    // Logic to determine warning level
    if (needsWarning)
    {
        if (hudManager != null)
        {
            hudManager.ShowWarning(message, color);
        }
    }
}
```

---

## 📝 Implementation Checklist

### Phase 1: HUDManager Enhancement
- [ ] Add all player stat UI references to HUDManager
- [ ] Add `InitializePlayerStatsUI()` method
- [ ] Add individual update methods (UpdateHealthDisplay, etc.)
- [ ] Add `UpdateAllPlayerStats()` refresh method
- [ ] Call `InitializePlayerStatsUI()` in `Initialize()`

### Phase 2: Manager Events
- [ ] Add `onInfectionChanged` to SurvivalManager
- [ ] Add `SetInfection()` method to SurvivalManager
- [ ] Verify `onXPChanged` exists in ProgressionManager
- [ ] Verify `onLevelUp` exists in ProgressionManager
- [ ] Test all events fire correctly

### Phase 3: Script Refactoring
- [ ] Refactor PlayerHealthDisplay (remove Update or delete)
- [ ] Refactor PlayerStatsDisplay (remove Update or delete)
- [ ] Refactor PlayerInfectionDisplay (delegate to HUDManager)
- [ ] Refactor PlayerStatusIndicators (event-driven)
- [ ] Refactor PlayerStatusWarning (event-driven)
- [ ] Refactor PlayerSystemBridge (already uses HUDManager, verify)

### Phase 4: Cleanup
- [ ] Remove unused display scripts (if HUDManager handles everything)
- [ ] Test all UI updates work correctly
- [ ] Verify performance improvement (no polling)
- [ ] Update documentation

---

## 🎯 Migration Strategy

### Option A: Full Refactor (Recommended)
1. Enhance HUDManager with all UI methods
2. Add events to all managers
3. Delete individual display scripts
4. Assign UI references directly to HUDManager in Inspector

### Option B: Gradual Migration
1. Keep existing scripts
2. Make them delegate to HUDManager
3. Remove Update() polling, use events
4. Eventually delete wrapper scripts

### Option C: Hybrid
1. Core stats (health, XP, level) → HUDManager
2. Complex systems (infection, warnings) → Keep separate but event-driven
3. Best of both worlds

---

## ✅ Benefits After Refactoring

1. **Performance**: No more 6-10 Update() calls per frame
2. **Maintainability**: One place to update UI logic
3. **Flexibility**: Easy to swap UI systems
4. **Debugging**: Single point to debug UI issues
5. **Consistency**: Unified update pattern
6. **Extensibility**: Easy to add new stats

---

## 🔍 Testing Plan

1. **Health UI**: Take damage → health updates
2. **XP UI**: Gain XP → XP bar updates
3. **Level UI**: Level up → level text updates
4. **Temperature UI**: Enter cold zone → temperature drops
5. **Stamina UI**: Run → stamina drains
6. **Infection UI**: Get infected → infection rises
7. **Warnings**: Low health → warning shows
8. **Performance**: Profile before/after (expect ~60% reduction in UI Update calls)

---

## 📂 Files to Modify

### Must Modify:
1. `HUDManager.cs` - Add all update methods
2. `SurvivalManager.cs` - Add infection event
3. `ProgressionManager.cs` - Verify events

### Should Refactor:
4. `PlayerHealthDisplay.cs` - Remove or delegate
5. `PlayerStatsDisplay.cs` - Remove or delegate
6. `PlayerInfectionDisplay.cs` - Delegate to HUD
7. `PlayerStatusIndicators.cs` - Event-driven
8. `PlayerStatusWarning.cs` - Event-driven

### Can Keep:
9. `PlayerSystemBridge.cs` - Already uses HUDManager
10. `ChallengeNotificationUI.cs` - Already managed by HUD
11. `MissionUIManager.cs` - Already managed by HUD
12. `ProgressionUIManager.cs` - Already managed by HUD
13. `LootUIManager.cs` - Already managed by HUD

---

## 🚀 Next Steps

**I recommend:**
1. Start with Phase 1 (enhance HUDManager)
2. Add events to managers (Phase 2)
3. Test with one script first (PlayerHealthDisplay)
4. If successful, migrate remaining scripts
5. Clean up and optimize

**Would you like me to:**
- A) Implement the enhanced HUDManager code
- B) Refactor a specific display script first
- C) Add the missing events to managers
- D) All of the above

Let me know which phase to start with!
