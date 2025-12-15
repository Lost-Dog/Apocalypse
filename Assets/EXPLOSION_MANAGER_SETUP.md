# Explosion Manager Setup Guide

## Overview
System for randomly triggering explosions on wrecked vehicles with automatic deactivation.

**You have selected:** 87 wrecked vehicle GameObjects

---

## Quick Setup

### **Step 1: Create Explosion Effects**

For each vehicle that should explode, add a child explosion GameObject:

**Method A: Manual Setup**
1. Select a vehicle (e.g., `SM_Prop_Car_Wrecked_SUV_01`)
2. Right-click → Create Empty
3. Rename to: `Explosion`
4. Add explosion particle systems as children
5. Set `Explosion` GameObject to **inactive** (uncheck in Inspector)

**Method B: Prefab Setup (Recommended)**
1. Create a prefab: `VehicleExplosion.prefab`
2. Add particle systems (fire, smoke, debris)
3. Add component: **AutoDeactivateExplosion**
4. For each vehicle:
   - Drag `VehicleExplosion` prefab as child
   - Rename to: `Explosion`
   - Set to inactive

### **Step 2: Create ExplosionManager**

1. Create Empty GameObject in scene: `ExplosionManager`
2. Add Component → **ExplosionManager**
3. Drag all 87 selected vehicles into `Explosion Targets` list

### **Step 3: Configure**

**In ExplosionManager Inspector:**
```
Min Time Between Explosions: 5 seconds
Max Time Between Explosions: 15 seconds
Explosion Duration: 3 seconds
Min Explosions Per Cycle: 1
Max Explosions Per Cycle: 3
Start On Awake: ☑
Continuous Explosions: ☑
Explosion Component Tag: "Explosion"
```

### **Step 4: Test**

- Enter Play Mode ▶️
- Wait 5-15 seconds
- Random vehicles will explode
- 1-3 explosions per cycle
- Explosions auto-deactivate after 3 seconds

---

## Detailed Setup

### **Creating Explosion Effects**

**Option 1: Simple Fire/Smoke**
```
Vehicle
└── Explosion (inactive)
    ├── Fire (ParticleSystem)
    ├── Smoke (ParticleSystem)
    └── Sparks (ParticleSystem)
```

**Option 2: Advanced Explosion**
```
Vehicle
└── Explosion (inactive)
    ├── ExplosionFlash (ParticleSystem)
    ├── Fire (ParticleSystem)
    ├── Smoke (ParticleSystem)
    ├── Debris (ParticleSystem)
    ├── Sparks (ParticleSystem)
    ├── ExplosionLight (Light)
    └── ExplosionSound (AudioSource)
```

**Particle System Settings:**
```
Fire:
├── Duration: 2-3 seconds
├── Start Lifetime: 1-2 seconds
├── Start Speed: 2-5
├── Start Size: 0.5-2
├── Start Color: Orange → Red
└── Emission: 50-100 particles

Smoke:
├── Duration: 3-5 seconds
├── Start Lifetime: 2-4 seconds
├── Start Speed: 1-3
├── Start Size: 1-3
├── Start Color: Dark gray → Light gray
└── Emission: 20-50 particles

Sparks:
├── Duration: 0.5-1 second
├── Start Lifetime: 0.3-0.8 seconds
├── Start Speed: 5-10
├── Start Size: 0.1-0.3
├── Start Color: Yellow
└── Emission: 100-200 particles
```

---

## Configuration

### **ExplosionManager Settings:**

**Timing:**
```
Min Time Between Explosions: 5s
  └── Minimum wait between explosion cycles

Max Time Between Explosions: 15s
  └── Maximum wait between explosion cycles

Explosion Duration: 3s
  └── How long explosions stay active before deactivating
```

**Random Selection:**
```
Min Explosions Per Cycle: 1
  └── Minimum number of simultaneous explosions

Max Explosions Per Cycle: 3
  └── Maximum number of simultaneous explosions
```

**System Control:**
```
Start On Awake: ☑
  └── Begin explosions automatically on scene start

Continuous Explosions: ☑
  └── Keep cycling explosions (uncheck for one-time only)
```

**Component Names:**
```
Explosion Component Tag: "Explosion"
  └── Name of child GameObject containing explosion effects
  └── Change if you named your explosion GameObjects differently
```

**Audio (Optional):**
```
Explosion Sound: (AudioClip)
  └── Sound effect to play on explosion

Sound Volume: 0.7
  └── Volume of explosion sound

Sound Max Distance: 50
  └── Max distance player can hear explosions
```

**Debug:**
```
Show Debug Info: ☐
  └── Log explosion events to console (for testing)
```

### **AutoDeactivateExplosion Settings:**

```
Deactivate Delay: 3s
  └── How long before auto-deactivating

Deactivate On Enable: ☑
  └── Start timer when GameObject is activated

Stop Particles Before Deactivate: ☑
  └── Stop particle emission before deactivating

Particle Stop Delay: 1s
  └── Wait time after stopping particles before deactivating
```

---

## How It Works

### **Explosion Cycle:**

1. **Wait:** Random time between min/max
2. **Select:** Pick 1-3 random vehicles
3. **Activate:** Turn on explosion GameObject
4. **Play:** Start particle systems
5. **Wait:** Keep active for explosion duration
6. **Stop:** Stop particle emission
7. **Deactivate:** Turn off explosion GameObject
8. **Repeat:** Go to step 1

### **Example Timeline:**

```
0:00 - Scene starts
0:00 - ExplosionManager initializes 87 targets
0:08 - First cycle: 2 random vehicles explode
0:08 - Particles play on both vehicles
0:11 - Explosions deactivate (after 3 seconds)
0:23 - Second cycle: 1 random vehicle explodes
0:26 - Explosion deactivates
0:35 - Third cycle: 3 random vehicles explode
...continues indefinitely
```

---

## Advanced Usage

### **Batch Setup Helper Script:**

Add this to selected vehicles automatically:

```csharp
// Editor script to add explosion GameObjects to all selected vehicles
using UnityEngine;
using UnityEditor;

public class AddExplosionsToVehicles : MonoBehaviour
{
    [MenuItem("Tools/Add Explosion Children to Selected")]
    static void AddExplosions()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            // Check if Explosion child already exists
            Transform existingExplosion = obj.transform.Find("Explosion");
            if (existingExplosion == null)
            {
                // Create new explosion GameObject
                GameObject explosion = new GameObject("Explosion");
                explosion.transform.SetParent(obj.transform);
                explosion.transform.localPosition = Vector3.zero;
                explosion.SetActive(false);
                
                // Add auto-deactivate component
                explosion.AddComponent<AutoDeactivateExplosion>();
                
                Debug.Log($"Added Explosion to {obj.name}");
            }
        }
    }
}
```

### **Code Integration:**

**Manually Trigger Single Explosion:**
```csharp
ExplosionManager explosionMgr = FindFirstObjectByType<ExplosionManager>();
explosionMgr.TriggerSingleExplosion();
```

**Trigger Specific Vehicle:**
```csharp
ExplosionManager explosionMgr = FindFirstObjectByType<ExplosionManager>();
explosionMgr.TriggerExplosionAt(5); // Explode target at index 5
```

**Start/Stop System:**
```csharp
ExplosionManager explosionMgr = FindFirstObjectByType<ExplosionManager>();

// Start explosions
explosionMgr.StartExplosions();

// Stop explosions
explosionMgr.StopExplosions();
```

**Add/Remove Targets Dynamically:**
```csharp
ExplosionManager explosionMgr = FindFirstObjectByType<ExplosionManager>();

// Add new vehicle
explosionMgr.AddTarget(newVehicle);

// Remove vehicle
explosionMgr.RemoveTarget(oldVehicle);

// Clear all
explosionMgr.ClearAllTargets();
```

### **Trigger Explosions by Event:**

**On Player Proximity:**
```csharp
using UnityEngine;

public class ProximityExplosion : MonoBehaviour
{
    public float triggerDistance = 10f;
    private Transform player;
    private bool hasExploded = false;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    private void Update()
    {
        if (hasExploded) return;
        
        if (Vector3.Distance(transform.position, player.position) < triggerDistance)
        {
            TriggerExplosion();
            hasExploded = true;
        }
    }
    
    private void TriggerExplosion()
    {
        Transform explosion = transform.Find("Explosion");
        if (explosion != null)
        {
            explosion.gameObject.SetActive(true);
        }
    }
}
```

**On Gunfire Hit:**
```csharp
// In your bullet/damage script:
private void OnCollisionEnter(Collision collision)
{
    Transform explosion = collision.transform.Find("Explosion");
    if (explosion != null)
    {
        explosion.gameObject.SetActive(true);
    }
}
```

---

## Customization Examples

### **Frequent Small Explosions:**
```
Min Time: 2s
Max Time: 5s
Min Per Cycle: 1
Max Per Cycle: 1
Duration: 2s
```

### **Rare Large Explosions:**
```
Min Time: 30s
Max Time: 60s
Min Per Cycle: 3
Max Per Cycle: 5
Duration: 5s
```

### **War Zone (Intense):**
```
Min Time: 1s
Max Time: 3s
Min Per Cycle: 2
Max Per Cycle: 4
Duration: 4s
```

### **Ambient Background:**
```
Min Time: 20s
Max Time: 40s
Min Per Cycle: 1
Max Per Cycle: 2
Duration: 3s
```

---

## Particle System Presets

### **Basic Fire:**
```
Main:
├── Duration: 2
├── Looping: No
├── Start Lifetime: 1-2
├── Start Speed: 2
├── Start Size: 0.8-1.5
└── Start Color: (255, 150, 0) → (255, 50, 0)

Emission:
└── Rate over Time: 50

Shape:
├── Shape: Sphere
└── Radius: 0.5

Color over Lifetime:
└── Alpha: 1 → 0

Size over Lifetime:
└── Size: 1 → 0.5
```

### **Dense Smoke:**
```
Main:
├── Duration: 4
├── Start Lifetime: 3-5
├── Start Speed: 1-3
├── Start Size: 1-2
└── Start Color: (50, 50, 50) → (100, 100, 100)

Emission:
└── Rate over Time: 30

Shape:
├── Shape: Cone
├── Angle: 30
└── Radius: 0.3

Color over Lifetime:
└── Alpha: 0.8 → 0

Size over Lifetime:
└── Size: 0.5 → 2
```

### **Impact Sparks:**
```
Main:
├── Duration: 0.5
├── Start Lifetime: 0.3-0.6
├── Start Speed: 8-12
├── Start Size: 0.1-0.2
└── Start Color: (255, 255, 0)

Emission:
└── Burst: 100 particles at time 0

Shape:
├── Shape: Sphere
└── Radius: 0.1

Gravity Modifier: 1

Color over Lifetime:
└── Alpha: 1 → 0
```

---

## Testing Checklist

### **Setup:**
- ☐ Created ExplosionManager GameObject
- ☐ Added ExplosionManager component
- ☐ Assigned all 87 vehicles to Explosion Targets list
- ☐ Created explosion effects on vehicles
- ☐ Named explosion GameObjects "Explosion"
- ☐ Set explosion GameObjects to inactive

### **Configuration:**
- ☐ Set timing values (min/max between explosions)
- ☐ Set explosion count per cycle
- ☐ Set explosion duration
- ☐ Enabled "Start On Awake"
- ☐ Enabled "Continuous Explosions"

### **Testing:**
- ☐ Enter Play Mode
- ☐ Wait for first explosion
- ☐ Verify particles play
- ☐ Verify explosions deactivate after duration
- ☐ Verify multiple cycles occur
- ☐ Verify random selection (different vehicles)

### **Polish:**
- ☐ Add explosion sound effects
- ☐ Adjust particle colors/sizes
- ☐ Fine-tune timing
- ☐ Test from different distances
- ☐ Verify performance with many explosions

---

## Troubleshooting

**Explosions don't trigger:**
- Check "Start On Awake" is enabled
- Verify vehicles are in Explosion Targets list
- Check explosion GameObjects are named correctly
- Enable "Show Debug Info" to see logs

**Explosions don't deactivate:**
- Check AutoDeactivateExplosion component is added
- Verify "Deactivate On Enable" is checked
- Check deactivate delay is set correctly
- Ensure particle systems have finite duration

**Particles don't play:**
- Verify explosion GameObject has ParticleSystem
- Check particle systems aren't set to "Prewarm"
- Ensure "Play On Awake" is unchecked
- Verify explosion GameObject is activating

**Too many/few explosions:**
- Adjust Min/Max Explosions Per Cycle
- Modify Min/Max Time Between Explosions
- Check number of targets in list

**Explosions all at same location:**
- Check each vehicle has its own Explosion child
- Verify Explosion GameObjects are children, not siblings
- Ensure local position is set to (0,0,0)

---

## Performance Notes

- Particle systems are lightweight when inactive
- Only active explosions consume resources
- Recommend max 5 simultaneous explosions for mobile
- Use simple particle counts for better performance
- Consider disabling distant explosions (LOD)

---

**Created:**
- ExplosionManager.cs (main controller)
- AutoDeactivateExplosion.cs (auto-deactivate helper)

**Location:** `/Assets/Scripts/`  
**Documentation:** `/Assets/EXPLOSION_MANAGER_SETUP.md`

**Selected Vehicles:** 87 wrecked vehicles in `/Environment/Apocalypse_City/Vehicles_Wrecked/`
