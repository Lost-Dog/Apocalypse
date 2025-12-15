# Safe Zone Setup Guide

## 🛡️ Overview

Safe Zones are areas where players can restore their health, stamina, cure infection, and normalize temperature. Perfect for creating safe havens in your apocalypse game!

---

## 📦 What's Included

### **Scripts:**
1. ✅ `SafeZone.cs` - Main safe zone logic
2. ✅ `MessageDisplay.cs` - UI message system
3. ✅ `SafeZoneVisualEffect.cs` - Visual effects (optional)

### **Features:**
- ✅ Health restoration
- ✅ Stamina restoration
- ✅ Infection cure
- ✅ Temperature normalization
- ✅ Visual & audio feedback
- ✅ UI messages
- ✅ Customizable restoration rates
- ✅ Idle requirement option
- ✅ Events system

---

## 🚀 Quick Setup (5 Minutes)

### **Step 1: Create Safe Zone Object**

1. **Create Empty GameObject:**
   ```
   Hierarchy → Right-click → Create Empty
   Name: "SafeZone_Base"
   ```

2. **Add Collider:**
   ```
   Add Component → Box Collider (or Sphere Collider)
   └── Is Trigger: ☑ true
   └── Size: 10, 5, 10 (adjust as needed)
   ```

3. **Add SafeZone Script:**
   ```
   Add Component → SafeZone
   ```

4. **Tag the Player:**
   ```
   Make sure your player has Tag: "Player"
   ```

**That's it! You now have a basic safe zone!** ✅

---

## ⚙️ Configuration

### **SafeZone Component Settings:**

#### **Safe Zone Settings:**
```yaml
Safe Zone Name: "Main Base"
Restore Health: ☑ true
Restore Stamina: ☑ true
Cure Infection: ☑ true
Normalize Temperature: ☑ true
```

#### **Restoration Rates:**
```yaml
Health Restore Rate: 10     # HP per second
Stamina Restore Rate: 20    # Stamina per second
Infection Cure Rate: 5      # Infection % per second
Temperature Normalize Speed: 2
```

#### **Restoration Settings:**
```yaml
Restore Delay: 1            # Seconds before healing starts
Require Idle: ☐ false       # Must player stand still?
Idle Movement Threshold: 0.1
```

#### **UI Feedback:**
```yaml
Show UI Message: ☑ true
Enter Message: "Entered Safe Zone - Restoring Stats"
Message Duration: 3
```

---

## 🎨 Visual Setup (Optional)

### **Option 1: Simple Colored Zone**

1. **Add Visual Mesh:**
   ```
   Hierarchy → Right-click SafeZone → 3D Object → Cylinder
   Name: "ZoneVisual"
   Transform:
   ├── Position: (0, 0, 0)
   ├── Rotation: (0, 0, 0)
   └── Scale: (10, 0.1, 10)
   ```

2. **Create Material:**
   ```
   Project → Create → Material
   Name: "SafeZoneMaterial"
   
   Settings:
   ├── Rendering Mode: Transparent
   ├── Albedo Color: Green (0, 255, 0, 100)
   └── Emission: ☑ enabled, Color: Light Green
   ```

3. **Apply Material:**
   ```
   Drag "SafeZoneMaterial" onto ZoneVisual
   ```

### **Option 2: Advanced Visual Effects**

1. **Add Visual Effect Script:**
   ```
   SafeZone → Add Component → SafeZoneVisualEffect
   ```

2. **Configure Effects:**
   ```yaml
   Enable Pulse: ☑ true
   Pulse Speed: 1
   
   Enable Rotation: ☑ true
   Rotation Speed: 10
   
   Enable Glow: ☑ true
   Glow Color: (0, 255, 128, 128)
   ```

---

## 🔊 Audio Setup (Optional)

### **Add Audio:**

1. **Prepare Audio Clips:**
   ```
   Import your audio files:
   ├── SafeZoneEnter.wav (short ding/chime)
   └── SafeZoneHeal.wav (looping ambient sound)
   ```

2. **Configure SafeZone:**
   ```yaml
   SafeZone Component:
   ├── Enter Sound: SafeZoneEnter
   ├── Healing Sound: SafeZoneHeal
   └── Sound Volume: 0.5
   ```

**Audio Source will be auto-created!**

---

## 💬 UI Message Setup

### **Step 1: Create Message Display UI**

1. **Create Canvas (if you don't have one):**
   ```
   Hierarchy → Right-click → UI → Canvas
   Canvas Settings:
   └── Render Mode: Screen Space - Overlay
   ```

2. **Create Message Panel:**
   ```
   Canvas → Right-click → UI → Panel
   Name: "MessageDisplay"
   
   RectTransform:
   ├── Anchor: Top Center
   ├── Position: (0, -50, 0)
   └── Size: (600, 80)
   
   Image:
   ├── Color: (0, 0, 0, 180) - Semi-transparent black
   ```

3. **Create Message Text:**
   ```
   MessageDisplay → Right-click → UI → Text - TextMeshPro
   Name: "MessageText"
   
   RectTransform:
   └── Stretch to fill parent
   
   TextMeshPro:
   ├── Text: ""
   ├── Font Size: 24
   ├── Alignment: Center & Middle
   └── Color: White
   ```

4. **Add MessageDisplay Script:**
   ```
   MessageDisplay panel → Add Component → MessageDisplay
   
   Settings:
   ├── Message Text: MessageText
   ├── Auto Setup: ☑ true
   ```

5. **Name the GameObject:**
   ```
   IMPORTANT: Rename panel to exactly "MessageDisplay"
   (SafeZone looks for this name)
   ```

---

## 🎯 Advanced Setups

### **Setup 1: Healing Station (Full Restore)**

```yaml
Safe Zone Settings:
├── Restore Health: ☑ true
├── Restore Stamina: ☑ true
├── Cure Infection: ☑ true
└── Normalize Temperature: ☑ true

Restoration Rates:
├── Health Restore Rate: 20    # Fast heal
├── Stamina Restore Rate: 50   # Very fast
├── Infection Cure Rate: 10    # Quick cure
└── Temperature Normalize Speed: 5

Restoration Settings:
├── Restore Delay: 0.5         # Almost instant
└── Require Idle: ☐ false
```

### **Setup 2: Rest Area (Slow, Idle Required)**

```yaml
Safe Zone Settings:
├── Restore Health: ☑ true
├── Restore Stamina: ☑ true
├── Cure Infection: ☐ false    # No infection cure
└── Normalize Temperature: ☑ true

Restoration Rates:
├── Health Restore Rate: 5     # Slow heal
├── Stamina Restore Rate: 15   # Medium
└── Temperature Normalize Speed: 1

Restoration Settings:
├── Restore Delay: 2           # 2 second delay
└── Require Idle: ☑ true       # Must stand still!
```

### **Setup 3: Medical Bay (Health Only, Fast)**

```yaml
Safe Zone Settings:
├── Restore Health: ☑ true
├── Restore Stamina: ☐ false
├── Cure Infection: ☑ true
└── Normalize Temperature: ☐ false

Restoration Rates:
├── Health Restore Rate: 30    # Very fast
└── Infection Cure Rate: 15

Restoration Settings:
├── Restore Delay: 0
└── Require Idle: ☐ false
```

### **Setup 4: Shelter (Temperature & Stamina)**

```yaml
Safe Zone Settings:
├── Restore Health: ☐ false
├── Restore Stamina: ☑ true
├── Cure Infection: ☐ false
└── Normalize Temperature: ☑ true

Restoration Rates:
├── Stamina Restore Rate: 30
└── Temperature Normalize Speed: 3

Restoration Settings:
├── Restore Delay: 1
└── Require Idle: ☐ false
```

---

## 🎮 Testing

### **Test 1: Basic Functionality**

1. **Start Play Mode**
2. **Damage the player:**
   ```
   Inspector → Player → JUHealth → Health: 50
   ```
3. **Walk into safe zone**
4. **Verify:**
   - ✅ Message appears "Entered Safe Zone"
   - ✅ Health increases over time
   - ✅ Reaches max health
   - ✅ Sound plays (if configured)

### **Test 2: All Stats Restoration**

1. **Set low stats:**
   ```
   PlayerStatsDisplay:
   ├── Current Stamina: 20
   ├── Current Infection: 50
   └── Current Temperature: 35
   ```
2. **Enter safe zone**
3. **Verify all stats restore:**
   - ✅ Stamina → 100
   - ✅ Infection → 0
   - ✅ Temperature → 37

### **Test 3: Idle Requirement**

1. **Enable Require Idle: ☑**
2. **Enter safe zone**
3. **Try moving:**
   - ✅ Healing stops when moving
   - ✅ Healing resumes when standing still

### **Test 4: Exit Zone**

1. **Enter safe zone**
2. **Wait for healing to start**
3. **Leave zone**
4. **Verify:**
   - ✅ Healing stops
   - ✅ "Left Safe Zone" message appears
   - ✅ Effects stop

---

## 🏗️ Creating Different Safe Zone Types

### **Type 1: Invisible Safe Zone (Building Interior)**

```
GameObject: "SafeZone_Building01"
├── SafeZone component
├── Box Collider (trigger)
└── No visual mesh

Use Case: Inside buildings, rooms
```

### **Type 2: Visible Safe Zone (Checkpoint)**

```
GameObject: "SafeZone_Checkpoint"
├── SafeZone component
├── Box Collider (trigger)
├── Visual mesh (cylinder/plane)
└── SafeZoneVisualEffect

Use Case: Outdoor checkpoints, respawn points
```

### **Type 3: Campfire Safe Zone**

```
GameObject: "SafeZone_Campfire"
├── SafeZone component
├── Sphere Collider (radius: 5, trigger)
├── Campfire prefab (child)
├── Particle System (flames)
└── Point Light (orange glow)

Settings:
├── Restore Health: ☑
├── Normalize Temperature: ☑
└── Healing Color: Orange/Red
```

### **Type 4: Medical Tent**

```
GameObject: "SafeZone_MedicalTent"
├── SafeZone component
├── Box Collider (trigger)
├── Tent model (child)
└── Medical props (beds, supplies)

Settings:
├── Restore Health: ☑ (fast)
├── Cure Infection: ☑ (fast)
└── Enter Message: "Medical Tent - Emergency Treatment"
```

---

## 🎯 Multiple Safe Zones in Scene

### **Example Setup:**

```
Scene Hierarchy:
├── SafeZones (empty parent)
│   ├── SafeZone_MainBase
│   │   └── Health: 20/s, All stats
│   ├── SafeZone_Checkpoint01
│   │   └── Health: 10/s, Stamina only
│   ├── SafeZone_Checkpoint02
│   │   └── Health: 10/s, Stamina only
│   ├── SafeZone_MedicalBay
│   │   └── Health: 30/s, Infection cure
│   └── SafeZone_Shelter
│       └── Temperature & Stamina only
```

**Each zone can have different settings!**

---

## 🔧 Events System

### **Using Events:**

SafeZone has built-in Unity Events:

```yaml
Events:
├── On Player Enter
├── On Player Exit
└── On Restore Complete
```

### **Example: Open Shop on Enter**

1. **Create Shop Manager:**
   ```csharp
   public void OpenShop()
   {
       shopUI.SetActive(true);
   }
   ```

2. **Wire Event:**
   ```
   SafeZone → On Player Enter → +
   └── Drag ShopManager
   └── Select: OpenShop()
   ```

### **Example: Save Game on Enter**

```
SafeZone → On Player Enter → +
└── GameManager → SaveGame()
```

### **Example: Play Custom Sound on Restore Complete**

```
SafeZone → On Restore Complete → +
└── AudioSource → Play()
```

---

## 📊 Safe Zone Comparison

| Zone Type | Health | Stamina | Infection | Temp | Idle Req | Use Case |
|-----------|--------|---------|-----------|------|----------|----------|
| **Main Base** | Fast | Fast | Yes | Yes | No | Primary safe area |
| **Checkpoint** | Medium | Fast | No | No | No | Quick stops |
| **Medical Bay** | Very Fast | No | Yes | No | No | Emergency healing |
| **Rest Area** | Slow | Medium | No | Yes | Yes | Camps |
| **Shelter** | No | Fast | No | Yes | No | Weather protection |

---

## 🎨 Visual Effects Guide

### **Glow Effect:**

```yaml
Material:
├── Shader: Universal Render Pipeline/Lit
├── Surface: Transparent
├── Emission: ☑ enabled
└── Emission Color: (0, 255, 128) - Green

SafeZoneVisualEffect:
├── Enable Glow: ☑ true
├── Glow Intensity: 2
└── Glow Pulse Speed: 2
```

### **Particle Effect:**

1. **Create Particle System:**
   ```
   SafeZone → Right-click → Effects → Particle System
   Name: "HealingParticles"
   ```

2. **Configure:**
   ```yaml
   Main Module:
   ├── Start Color: Green
   ├── Start Speed: 2
   ├── Start Size: 0.5
   └── Max Particles: 100
   
   Emission:
   └── Rate over Time: 10
   
   Shape:
   ├── Shape: Sphere
   └── Radius: 5
   ```

3. **Assign to SafeZone:**
   ```
   SafeZone → Healing Effect: HealingParticles
   ```

---

## 🐛 Troubleshooting

### **Issue: Player not healing**

**Check:**
1. ✅ Player has tag "Player"
2. ✅ Player has JUHealth component
3. ✅ Collider is trigger
4. ✅ Restore Health is enabled
5. ✅ Health Restore Rate > 0

### **Issue: No message appears**

**Solutions:**
1. Create "MessageDisplay" GameObject (exact name!)
2. Add MessageDisplay script
3. Assign TextMeshPro reference
4. Enable Show UI Message

### **Issue: Healing doesn't stop**

**Cause:** Player still in zone collider

**Solution:**
- Check collider size
- Ensure player fully exits zone

### **Issue: Stats restore too slowly**

**Solution:**
```yaml
Increase restoration rates:
├── Health Restore Rate: 20+
├── Stamina Restore Rate: 30+
└── Infection Cure Rate: 10+
```

### **Issue: Visual effects not showing**

**Check:**
1. ✅ Renderer component exists
2. ✅ Material assigned
3. ✅ Effects enabled in SafeZoneVisualEffect
4. ✅ Prefabs assigned

---

## 💡 Tips & Best Practices

### **Performance:**
- ✅ Use simple colliders (Box/Sphere, not Mesh)
- ✅ Disable effects when player not in zone
- ✅ Use object pooling for particle effects
- ✅ Limit particle count

### **Game Design:**
- ✅ Place safe zones strategically
- ✅ Balance restoration rates with difficulty
- ✅ Use different zones for different purposes
- ✅ Make safe zones visually distinct
- ✅ Add audio cues for entering/leaving

### **Level Design:**
- ✅ Main base: Full restoration, no idle required
- ✅ Checkpoints: Medium restoration
- ✅ Hidden spots: Slow restoration, idle required
- ✅ Medical bays: Health + infection only
- ✅ Shelters: Temperature + stamina

---

## 📝 Example Prefab Setup

### **Create Reusable Prefab:**

1. **Setup SafeZone in scene** with desired settings
2. **Drag to Project:**
   ```
   Hierarchy: SafeZone → Drag to /Assets/Prefabs
   Name: "SafeZone_Standard"
   ```
3. **Create Variants:**
   ```
   /Assets/Prefabs/SafeZones/
   ├── SafeZone_Standard.prefab
   ├── SafeZone_Fast.prefab
   ├── SafeZone_Medical.prefab
   ├── SafeZone_Rest.prefab
   └── SafeZone_Shelter.prefab
   ```

4. **Drag into scenes** as needed!

---

## ✅ Setup Checklist

**Basic Setup:**
- [ ] Created SafeZone GameObject
- [ ] Added Box/Sphere Collider (trigger)
- [ ] Added SafeZone script
- [ ] Player has "Player" tag
- [ ] Configured restoration rates
- [ ] Tested healing works

**Visual Setup:**
- [ ] Created visual mesh
- [ ] Applied material
- [ ] Added visual effects (optional)
- [ ] Added particle effects (optional)

**Audio Setup:**
- [ ] Imported audio clips
- [ ] Assigned to SafeZone
- [ ] Tested sounds play

**UI Setup:**
- [ ] Created MessageDisplay UI
- [ ] Added MessageDisplay script
- [ ] Named exactly "MessageDisplay"
- [ ] Tested messages appear

**Testing:**
- [ ] Player heals when entering
- [ ] All stats restore correctly
- [ ] Healing stops when exiting
- [ ] Messages display properly
- [ ] Effects work correctly

---

## 🎯 Summary

**You now have:**
- ✅ Safe zones that restore player stats
- ✅ Customizable restoration rates
- ✅ Visual & audio feedback
- ✅ UI messages
- ✅ Events system
- ✅ Multiple zone types

**Your players can now find safe havens to recover!** 🛡️💚

---

## 📚 Script References

**SafeZone.cs:**
- Detects player entry/exit
- Restores health, stamina, infection, temperature
- Configurable rates and delays
- Events system

**MessageDisplay.cs:**
- Shows UI messages
- Fade in/out animations
- Auto-setup

**SafeZoneVisualEffect.cs:**
- Pulse animation
- Rotation effect
- Glow effect
- Particle rings

---

**Need help? Check console for debug messages!** 🎮
