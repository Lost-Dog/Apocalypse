# Challenge Location & Notification Setup Guide

## Overview

Your challenge system now includes:
- ✅ **Persistent Notifications** that stay active during challenges
- ✅ **Live Progress Tracking** showing current progress
- ✅ **Time Remaining** countdown for timed challenges
- ✅ **3D World Markers** to see challenges from afar
- ✅ **Compass Markers** to track challenge directions
- ✅ **Distance indicators** showing how far away challenges are
- ✅ **Multi-challenge support** - cycle between active challenges

---

## 1. Persistent Notification System Setup

### Create Notification UI Panel

1. **In your Canvas** (usually in the scene):
   - Right-click → UI → Panel
   - Name it: `ChallengeNotificationPanel`
   - Set Anchor: **Top-Center** or **Top-Right** of screen
   - Position: `X: -300, Y: -100` (top-right corner)
   - Size: `Width: 400, Height: 250`

2. **Add CanvasGroup** component to the panel for fading

3. **Add child UI elements:**

```
ChallengeNotificationPanel
├── Background (Image)
│   └── Dark semi-transparent background
├── TitleText (TextMeshPro)
│   └── Font Size 20, Bold, Color: White
├── DescriptionText (TextMeshPro)
│   └── Font Size 14, Color: Light Gray
├── DifficultyText (TextMeshPro)
│   └── Font Size 16, Bold (color auto-set by difficulty)
├── RewardText (TextMeshPro)
│   └── Font Size 16, Color: Yellow/Gold
├── ProgressText (TextMeshPro)
│   └── Font Size 18, Bold, ex: "5 / 50"
├── TimeRemainingText (TextMeshPro)
│   └── Font Size 14, Color: Cyan, ex: "Time: 05:30"
├── ProgressSlider (Slider)
│   └── Fill color matches difficulty
├── DifficultyImage (Image)
│   └── Small color bar/icon
├── ChallengeIcon (Image)
│   └── Optional icon for challenge type
└── CycleButtons (Optional)
    ├── NextButton (Button) → ">"
    └── PrevButton (Button) → "<"
```

4. **Add the ChallengeNotificationUI script** to the panel:
   - Drag the script onto `ChallengeNotificationPanel`
   - Assign all the UI elements in the Inspector
   - Set Fade In/Out: `0.5` seconds each
   - Assign notification sounds:
     - Notification Sound (challenge starts)
     - Complete Sound (challenge completed)
     - Fail Sound (challenge failed/expired)

### NEW: Progress Tracking Elements

The notification now shows **LIVE** updates:
- **Progress Text**: "15 / 50" (current / total)
- **Progress Slider**: Visual bar filling up
- **Time Remaining**: "Time: 04:23" or "Active" for world events

### NEW: Multiple Challenge Support

When you have multiple active challenges:
- Panel shows one at a time
- Press cycle buttons to switch between challenges
- Or call `CycleToNextChallenge()` / `CycleToPreviousChallenge()`

---

## 2. 3D World Marker Setup

### Create World Marker Prefab

1. **Create empty GameObject**: `ChallengeWorldMarker`

2. **Add structure:**

```
ChallengeWorldMarker
├── MarkerPivot (empty, this will bob/rotate)
│   ├── Icon (Sprite Renderer)
│   │   └── Use a circular sprite or icon
│   │   └── Sort Order: 100 (render on top)
│   ├── PointLight (Light component)
│   │   └── Type: Point
│   │   └── Range: 10
│   │   └── Intensity: 2
│   └── Particles (Particle System)
│       └── Simple upward beam/glow effect
├── DistanceTextParent (empty, will face camera)
│   └── DistanceText (TextMeshPro - 3D)
│       └── Font Size: 2
│       └── Alignment: Center
└── NameTextParent (empty, will face camera)
    └── NameText (TextMeshPro - 3D)
        └── Font Size: 1.5
        └── Alignment: Center
```

3. **Configure the sprite:**
   - Use a simple diamond/beacon shape
   - Set material to **Sprites/Default**
   - Or use a billboard shader for always facing camera

4. **Add ChallengeWorldMarker script** to root:
   - Assign all references
   - Bob Speed: `1`
   - Bob Height: `0.5`
   - Rotate Speed: `30`
   - Max Visible Distance: `500`
   - Hide When Close: `5`

5. **Save as Prefab** in `/Assets/Prefabs/`

### Alternative: Simple Version

If you want a minimal marker:

1. **Create empty GameObject**: `ChallengeWorldMarker_Simple`
2. **Add Sphere primitive** as child
   - Scale: `(2, 2, 2)`
   - Material: Emissive material
3. **Add ChallengeWorldMarker script**
4. **Save as prefab**

---

## 3. Compass Marker Setup

### Create Compass Marker UI

1. **In your HUD Canvas**, find or create a **Compass Panel**

2. **Inside Compass Panel**, create:
   - Name: `CompassMarkerContainer` (empty RectTransform)
   - Width: Match compass width
   - Height: 30-50

3. **Create Compass Marker Prefab:**
   - Right-click in Project → Create → UI → Image
   - Name: `ChallengeCompassMarker`
   - Size: `20x20`
   - Use a simple arrow or diamond sprite
   - Add `ChallengeCompassMarker` script
   - Save as prefab

4. **Configure ChallengeManager:**
   - Drag `CompassMarkerContainer` to ChallengeManager
   - Assign compass marker prefab
   - Enable "Spawn Compass Markers"

---

## 4. Connect to ChallengeManager

### On GameSystems → ChallengeManager:

```
Visual Markers:
├── World Marker Prefab: [Drag your world marker prefab]
├── Compass Marker Prefab: [Drag your compass marker prefab]
├── Compass Marker Container: [Drag your compass container]
├── Spawn World Markers: ✓
└── Spawn Compass Markers: ✓
```

---

## 5. Testing

1. **Press Play**
2. **Wait for a challenge to spawn** (or set spawn interval to 10 seconds)
3. **You should see:**
   - Notification popup at top of screen
   - 3D marker appear at challenge location
   - Marker on compass showing direction

---

## Alternative Location Methods

### Method 1: Minimap Markers

If you have a minimap system:

```csharp
public class ChallengeMinimapMarker : MonoBehaviour
{
    public ActiveChallenge linkedChallenge;
    private RectTransform rectTransform;
    
    void Update()
    {
        // Convert world position to minimap position
        Vector2 minimapPos = WorldToMinimapPosition(linkedChallenge.position);
        rectTransform.anchoredPosition = minimapPos;
    }
}
```

### Method 2: GPS/Waypoint System

```csharp
public class ChallengeWaypoint : MonoBehaviour
{
    public ActiveChallenge linkedChallenge;
    
    void OnGUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(linkedChallenge.position);
        
        if (screenPos.z > 0)
        {
            // Draw arrow or distance on screen
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 200, 50), 
                $"Challenge: {Vector3.Distance(playerPos, linkedChallenge.position):F0}m");
        }
    }
}
```

### Method 3: Flare/Smoke Column

For a visual cue in the world:

```csharp
public void SpawnChallengeFlare(Vector3 position)
{
    // Spawn a tall particle column
    GameObject flare = Instantiate(flarePrefab, position, Quaternion.identity);
    ParticleSystem ps = flare.GetComponent<ParticleSystem>();
    
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Cone;
    shape.angle = 5f;
    
    var main = ps.main;
    main.startLifetime = 10f;
    main.startSpeed = 20f;
}
```

### Method 4: Audio Beacon

Add directional audio cue:

```csharp
public class ChallengeAudioBeacon : MonoBehaviour
{
    [SerializeField] private AudioClip beaconSound;
    [SerializeField] private float beepInterval = 3f;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.maxDistance = 200f;
        audioSource.loop = false;
        
        InvokeRepeating(nameof(PlayBeep), 0f, beepInterval);
    }
    
    void PlayBeep()
    {
        audioSource.PlayOneShot(beaconSound);
    }
}
```

### Method 5: HUD Directional Indicator

Edge-of-screen arrow pointing to challenge:

```csharp
public class ChallengeDirectionalIndicator : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private float edgeOffset = 50f;
    
    void Update()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(challengePosition);
        
        // Clamp to screen edges
        bool isOffScreen = screenPoint.z < 0 || 
                          screenPoint.x < 0 || 
                          screenPoint.x > Screen.width ||
                          screenPoint.y < 0 || 
                          screenPoint.y > Screen.height;
        
        if (isOffScreen)
        {
            // Show arrow at edge pointing to challenge
            Vector2 edgePosition = GetEdgePosition(screenPoint);
            arrow.position = edgePosition;
            arrow.gameObject.SetActive(true);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }
}
```

---

## Recommended Setup

For the best Division-like experience:

1. **Use World Markers** - Easy to see from distance
2. **Use Compass Markers** - Always know direction
3. **Add Notification UI** - Alert player to new challenges
4. **Optional: Audio beacon** - For immersion
5. **Optional: Minimap markers** - If you have a map system

---

## Quick Test Checklist

- [ ] Notification appears when challenge spawns
- [ ] Notification shows challenge name and description
- [ ] Notification displays difficulty color
- [ ] 3D marker appears at challenge location
- [ ] 3D marker bobs and rotates
- [ ] Distance text updates as you move
- [ ] Compass marker points toward challenge
- [ ] Markers disappear when challenge completes
- [ ] Markers disappear when challenge expires
- [ ] Can see multiple challenge markers simultaneously

---

## Troubleshooting

**Notification doesn't appear:**
- Check ChallengeNotificationUI is in the scene
- Verify onChallengeSpawned event is firing
- Check Canvas is set to Screen Space - Overlay

**World marker not visible:**
- Check marker is at correct Y height
- Verify camera can see the marker layer
- Increase maxVisibleDistance

**Compass marker not working:**
- Verify compass container is assigned
- Check player has "Player" tag
- Ensure Camera.main is valid

**Markers don't disappear:**
- Check Update() is being called
- Verify linkedChallenge is set correctly
- Check IsCompleted/IsExpired logic
