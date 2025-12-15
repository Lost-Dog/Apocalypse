<|CONTINUATION_MARKER|>

Perfect! Now let me create comprehensive documentation for the world-space health bar system:

```markdown
# World Space Health Bar - Complete Guide

## 📊 Overview

This system provides **world-space health bars** for characters using:
- ✓ **JUTPS JUHealth** component integration
- ✓ **ApocalypseHUD** visual prefabs (professional UI)
- ✓ **Custom WorldSpaceHealthBar** script
- ✓ **Automatic camera facing**
- ✓ **Distance-based visibility**
- ✓ **Show only when damaged** option

---

## 🎯 What's Included

### Scripts Created:

1. **`WorldSpaceHealthBar.cs`** - Main health bar controller
2. **`WorldSpaceHealthBarSetup.cs`** - Editor tool for easy setup

### Features:

```
✓ Auto-connects to JUHealth component
✓ Smooth health bar updates
✓ Color-coded health (green → yellow → red)
✓ Optional character name display
✓ Optional level display
✓ Show only when damaged (auto-hide after delay)
✓ Distance culling for performance
✓ Always faces camera
✓ Smooth positioning and transitions
```

---

## ⚡ Quick Setup (30 Seconds)

### Method 1: Using the Setup Tool (RECOMMENDED)

1. **Open the tool:**
   ```
   Menu: Tools > Character Health Bar Setup
   ```

2. **In the setup window:**
   - Drag your character to **"Character Prefab/GameObject"**
   - (Optional) Set character name and level
   - Click **"Add Health Bar to Character"**

3. **Done!** Health bar is now attached

---

### Method 2: Using ApocalypseHUD Prefabs

1. **Load the prefab:**
   ```
   Click "Create ApocalypseHUD Health Bar Prefab" in setup tool
   ```

2. **Add to character:**
   - Drag character to setup tool
   - Click "Add Health Bar to Character"

3. **Perfect!** Professional health bar added

---

## 📦 Available Health Bar Prefabs

### ApocalypseHUD Prefabs (Professional):

Located in: `/Assets/Synty/InterfaceApocalypseHUD/Prefabs/NPC_HealthBars_EnemyData/`

```
Available Prefabs:
├── HUD_Apocalypse_WorldSpace_EnemyInfo_01.prefab
│   └── Full health bar with name, level, and HP
├── HUD_Apocalypse_WorldSpace_NameEnemy_01.prefab
│   └── Enemy name display
├── HUD_Apocalypse_WorldSpace_NameAlly_01.prefab
│   └── Ally name display
└── HUD_Apocalypse_WorldSpace_ReviveIndicator_01.prefab
    └── Revive/interact indicator
```

**Best for:** Enemies, bosses, NPCs, allies

---

### Simple Health Bar (Minimalist):

Create with: **"Create Simple Health Bar Prefab"** button

```
Features:
├── Clean slider-based design
├── Color-coded (green/yellow/red)
├── No text elements
└── Lightweight and performant
```

**Best for:** Civilians, ambient NPCs, testing

---

## 🔧 Manual Setup

### Step 1: Create Health Bar GameObject

```
1. Right-click character in Hierarchy
2. Create > UI > Canvas
3. Name it "HealthBar_WorldSpace"
4. Set Canvas to "World Space"
5. Set RenderTransform size: 2 x 0.3
6. Set scale: 0.01 x 0.01 x 0.01
```

### Step 2: Add UI Elements

```
Inside Canvas, create:
1. Background Image (dark gray, 80% alpha)
2. Slider (for health bar)
3. Fill Image (green, inside slider)
4. (Optional) TextMeshPro for name
5. (Optional) TextMeshPro for level
```

### Step 3: Add WorldSpaceHealthBar Script

```
1. Add WorldSpaceHealthBar component to canvas
2. Assign references:
   - Target Health: Character's JUHealth component
   - Target Transform: Character's root transform
   - World Space Canvas: The canvas
   - Health Slider: The slider
   - Fill Image: The fill image
   - Name Text: (optional) name text
   - Level Text: (optional) level text
```

### Step 4: Configure Settings

```
Positioning:
- World Offset: (0, 2.5, 0) - adjust height

Visibility:
- Show Only When Damaged: ✓ Check
- Hide Delay: 3 seconds
- Max Visible Distance: 30 meters

Colors:
- Full Health: Green (0.2, 0.8, 0.2)
- Mid Health: Yellow (0.9, 0.9, 0.2)
- Low Health: Red (0.9, 0.2, 0.2)
```

---

## ⚙️ WorldSpaceHealthBar Settings

### References Section

| Field | Description | Required |
|-------|-------------|----------|
| Target Health | JUHealth component | ✓ Yes |
| Target Transform | Character root transform | ✓ Yes |
| World Space Canvas | Canvas component | ✓ Yes |
| Health Slider | UI Slider | ✓ Yes |
| Fill Image | Slider fill image | ✓ Yes |
| Name Text | TextMeshPro for name | Optional |
| Level Text | TextMeshPro for level | Optional |

---

### Positioning Section

| Field | Default | Description |
|-------|---------|-------------|
| World Offset | (0, 2.5, 0) | Height above character |
| Smooth Speed | 8 | Position/value smoothing |

---

### Visibility Section

| Field | Default | Description |
|-------|---------|-------------|
| Show Only When Damaged | ✓ True | Auto-hide when full HP |
| Hide Delay | 3 seconds | Time before hiding |
| Max Visible Distance | 30 meters | Culling distance |
| Always Show | False | Never hide health bar |

---

### Health Colors Section

| Field | Default | When |
|-------|---------|------|
| Full Health Color | Green | HP > 60% |
| Mid Health Color | Yellow | HP 30-60% |
| Low Health Color | Red | HP < 30% |
| Mid Threshold | 0.6 | Yellow starts |
| Low Threshold | 0.3 | Red starts |

---

## 🎮 Usage Examples

### Example 1: Enemy with Full Info

```csharp
// Already set up via prefab or tool
// Health bar shows:
// - Enemy name
// - Level number
// - Color-coded health bar
// - Only visible when damaged
// - Auto-hides after 3 seconds
```

### Example 2: Civilian with Simple Bar

```csharp
// Simple health bar setup
WorldSpaceHealthBar healthBar = GetComponent<WorldSpaceHealthBar>();
healthBar.SetName("Survivor");
healthBar.alwaysShow = false; // Hide when not damaged
healthBar.maxVisibleDistance = 20f; // Shorter distance
```

### Example 3: Boss with Always-Visible Bar

```csharp
WorldSpaceHealthBar healthBar = GetComponent<WorldSpaceHealthBar>();
healthBar.SetName("WARLORD");
healthBar.SetLevel(50);
healthBar.alwaysShow = true; // Always visible
healthBar.maxVisibleDistance = 100f; // Far visibility
```

### Example 4: Showing Health Bar Programmatically

```csharp
public class MyCharacter : MonoBehaviour
{
    private WorldSpaceHealthBar healthBar;
    
    void Start()
    {
        healthBar = GetComponentInChildren<WorldSpaceHealthBar>();
        healthBar.SetName("Elite Guard");
        healthBar.SetLevel(25);
    }
    
    void OnTakeDamage()
    {
        // Show health bar for 5 seconds
        healthBar.ShowTemporarily(5f);
    }
}
```

---

## 🔍 Integration with JUTPS

### JUHealth Component Required

The health bar system **requires** characters to have the **JUHealth** component:

```
Character GameObject
├── Transform
├── JUHealth ← Required!
├── ... (other components)
└── HealthBar_WorldSpace (child)
    └── WorldSpaceHealthBar script
```

### Automatic Health Tracking

The system automatically:
- ✓ Reads `JUHealth.Health`
- ✓ Reads `JUHealth.MaxHealth`
- ✓ Updates when health changes
- ✓ Detects damage events
- ✓ Shows bar when damaged

---

## 🎨 Using ApocalypseHUD Prefabs

### EnemyInfo_01 (Full Display)

```
Features:
├── Enemy name display
├── Level badge
├── Health bar with background
├── Color-coded fill
└── Professional apocalypse theme
```

**Use for:** Main enemies, bosses, named NPCs

### NameEnemy_01 (Name Only)

```
Features:
├── Enemy name display
├── Orange/red enemy color
└── Clean minimal design
```

**Use for:** Quick enemy identification

### NameAlly_01 (Ally Name)

```
Features:
├── Ally name display
├── Green/cyan ally color
└── Friendly indicator
```

**Use for:** Friendly NPCs, companions

---

## 📊 Performance Considerations

### Optimization Tips:

1. **Distance Culling:**
   ```
   Set maxVisibleDistance appropriately:
   - Enemies: 30-50m
   - Civilians: 15-20m
   - Bosses: 100m
   ```

2. **Show Only When Damaged:**
   ```
   Enable for most characters:
   - Reduces active UI elements
   - Better performance
   - Less visual clutter
   ```

3. **Canvas Settings:**
   ```
   Use World Space canvas
   Set appropriate pixel density
   Disable raycast when hidden
   ```

4. **Update Frequency:**
   ```
   System uses LateUpdate()
   Smooth lerping reduces jitter
   Only updates when visible
   ```

---

## 🐛 Troubleshooting

### Health Bar Not Showing

**Check:**
- [ ] JUHealth component exists on character
- [ ] WorldSpaceHealthBar references are assigned
- [ ] Canvas RenderMode is "World Space"
- [ ] Character is within maxVisibleDistance
- [ ] alwaysShow is true OR character has taken damage

**Fix:**
```
1. Select health bar GameObject
2. Check Inspector for missing references (red/None)
3. Reassign missing components
4. Test with alwaysShow = true first
```

---

### Health Bar Not Updating

**Check:**
- [ ] Target Health is assigned
- [ ] JUHealth component is active
- [ ] Health value is changing
- [ ] Slider max value is 1.0

**Fix:**
```csharp
// Manually test health change
JUHealth health = GetComponent<JUHealth>();
health.DoDamage(10f); // Should show health bar
```

---

### Health Bar Facing Wrong Direction

**Check:**
- [ ] Main Camera exists in scene
- [ ] FaceCamera() is running in LateUpdate

**Fix:**
```
The health bar auto-faces the main camera.
Ensure Camera.main is not null.
```

---

### Health Bar Too Small/Large

**Adjust:**
```
Canvas RectTransform:
- Size Delta: 2 x 0.3 (default)
- Scale: 0.01 x 0.01 x 0.01 (default)

Larger:
- Increase Size Delta to 3 x 0.4
- Keep scale at 0.01

Smaller:
- Decrease Size Delta to 1.5 x 0.25
- Keep scale at 0.01
```

---

### Health Bar Position Wrong

**Adjust:**
```
World Offset in WorldSpaceHealthBar:
- Default: (0, 2.5, 0)

For taller characters:
- Set to (0, 3.5, 0)

For shorter characters:
- Set to (0, 2.0, 0)

For specific placement:
- Adjust X/Z for horizontal offset
```

---

## 📋 Setup Checklist

### For Each Character:

- [ ] Character has JUHealth component
- [ ] Health bar prefab added as child
- [ ] WorldSpaceHealthBar script added
- [ ] Target Health assigned to JUHealth
- [ ] Target Transform assigned to character
- [ ] Canvas references assigned
- [ ] World Offset adjusted for character height
- [ ] Visibility settings configured
- [ ] (Optional) Name and level set
- [ ] Tested in Play Mode

---

## 🎯 Common Use Cases

### 1. Enemy Health Bars

```
Settings:
├── Show Only When Damaged: ✓ True
├── Hide Delay: 3 seconds
├── Max Distance: 40 meters
├── Name: Enemy Type
└── Level: Enemy level number
```

### 2. Boss Health Bars

```
Settings:
├── Always Show: ✓ True
├── Max Distance: 100 meters
├── Name: Boss Name (large text)
├── Level: Boss level
└── Full Health Color: Orange/Gold
```

### 3. Civilian Health Bars

```
Settings:
├── Show Only When Damaged: ✓ True
├── Hide Delay: 2 seconds
├── Max Distance: 20 meters
├── No name text
└── No level text (minimal)
```

### 4. Ally Health Bars

```
Settings:
├── Always Show: ✓ True (or when damaged)
├── Max Distance: 50 meters
├── Name: Ally name
├── Full Health Color: Cyan/Blue
└── Mid Health Color: Green
```

---

## 📖 API Reference

### Public Methods

```csharp
// Set character name
void SetName(string characterName)

// Set character level
void SetLevel(int level)

// Set health component reference
void SetTargetHealth(JUHealth health)

// Set transform to follow
void SetTargetTransform(Transform target)

// Show temporarily then hide
void ShowTemporarily(float duration = 3f)
```

### Example Usage

```csharp
WorldSpaceHealthBar healthBar = GetComponent<WorldSpaceHealthBar>();

// Setup
healthBar.SetName("Elite Soldier");
healthBar.SetLevel(30);

// Show for 5 seconds
healthBar.ShowTemporarily(5f);

// Change target at runtime
healthBar.SetTargetHealth(newHealthComponent);
```

---

## 🚀 Advanced Setup

### Multiple Health Bars

```
Character
├── HealthBar_Main (always visible)
│   └── WorldSpaceHealthBar (alwaysShow = true)
└── HealthBar_Damage (show on damage)
    └── WorldSpaceHealthBar (showOnlyWhenDamaged = true)
```

### Custom Color Schemes

```csharp
WorldSpaceHealthBar healthBar = GetComponent<WorldSpaceHealthBar>();

// Boss with orange/red scheme
healthBar.fullHealthColor = new Color(1f, 0.6f, 0f); // Orange
healthBar.midHealthColor = new Color(1f, 0.4f, 0f); // Dark orange
healthBar.lowHealthColor = new Color(1f, 0.2f, 0f); // Red-orange

// Ally with blue/cyan scheme
healthBar.fullHealthColor = new Color(0.2f, 0.8f, 1f); // Cyan
healthBar.midHealthColor = new Color(0.2f, 0.6f, 1f); // Blue
healthBar.lowHealthColor = new Color(0.4f, 0.4f, 1f); // Purple
```

### Dynamic Name Updates

```csharp
public class DynamicEnemyName : MonoBehaviour
{
    private WorldSpaceHealthBar healthBar;
    
    void Start()
    {
        healthBar = GetComponentInChildren<WorldSpaceHealthBar>();
        UpdateNameBasedOnState();
    }
    
    void UpdateNameBasedOnState()
    {
        JUHealth health = GetComponent<JUHealth>();
        float healthPercent = health.Health / health.MaxHealth;
        
        if (healthPercent > 0.8f)
            healthBar.SetName("Elite Guard");
        else if (healthPercent > 0.5f)
            healthBar.SetName("Wounded Guard");
        else
            healthBar.SetName("Critically Wounded");
    }
}
```

---

## ✅ Final Checklist

### Before Building:

- [ ] All enemy prefabs have health bars
- [ ] Boss health bars always visible
- [ ] Civilian health bars minimal
- [ ] Distance culling optimized
- [ ] Colors match faction/type
- [ ] Names and levels set
- [ ] Tested performance with many characters
- [ ] No missing references in prefabs

---

## 🎉 You're Ready!

Your characters now have professional world-space health bars with:

✓ JUTPS integration  
✓ ApocalypseHUD visuals  
✓ Auto-hide functionality  
✓ Distance culling  
✓ Smooth animations  
✓ Color-coded health  
✓ Optional name/level display  

**Next Steps:**
1. Open the setup tool: `Tools > Character Health Bar Setup`
2. Add health bars to your characters
3. Test in Play Mode
4. Adjust settings to your preference

Enjoy! 🎮
```
