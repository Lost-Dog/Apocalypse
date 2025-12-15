# Notification System - Setup Guide

## Overview

A flexible notification system that displays temporary messages with optional audio feedback. The panel starts disabled and shows briefly when events are triggered.

---

## Quick Setup (2 Minutes)

### Step 1: Add NotificationPanel Component

1. Select your notification GameObject: `/UI/HUD/ScreenSpace/Bottom/Bottom Left/HUD_Apocalypse_Comms_01`
2. Add Component → `NotificationPanel`
3. The Inspector will show setup buttons

### Step 2: Configure the Component

In the `NotificationPanel` component:

1. Click **"Auto-Find Message Text"** button
   - This will automatically find and assign the TextMeshProUGUI component

2. Click **"Add Audio Source"** button
   - This adds an AudioSource for notification sounds

3. Set the display duration (default: 3 seconds)

4. ✅ **Ensure "Start Disabled" is checked** (it is by default)

### Step 3: Test It!

1. Enter Play Mode
2. In the Inspector, enter a test message
3. Click **"Show Test Notification"**
4. Watch the notification appear and disappear!

---

## Component Settings

### Display Settings

```
Display Duration: 3.0
├── How long the notification stays visible (in seconds)
└── Can be overridden per notification

Start Disabled: ✓ Checked
└── Panel will be hidden at start
```

### References

```
Message Text: Label_Name (auto-found)
└── The TextMeshProUGUI component to display messages
```

### Audio

```
Audio Source: (auto-added)
├── Plays notification sounds
└── Can use different sounds per notification type

Default Notification Sound: (optional)
└── Sound to play if no specific sound is provided
```

### Animation (Optional)

```
Panel Animator: (optional)
├── Animator for fade in/out effects
Show Trigger: "Show"
└── Animation trigger name for showing
Hide Trigger: "Hide"
└── Animation trigger name for hiding
```

---

## Usage Examples

### Basic Usage

```csharp
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    public NotificationPanel notificationPanel;
    
    void Start()
    {
        notificationPanel.ShowNotification("Mission Started!");
    }
}
```

### With Custom Duration

```csharp
notificationPanel.ShowNotification("Quick message!", 1.5f);
```

### With Audio Clip

```csharp
public AudioClip levelUpSound;

void OnLevelUp()
{
    notificationPanel.ShowNotification("Level Up!", levelUpSound);
}
```

### With Custom Duration and Sound

```csharp
notificationPanel.ShowNotification("Achievement Unlocked!", achievementSound, 5f);
```

---

## Integration with Existing Systems

### 1. Level Up Notifications

In your `ProgressionManager.cs`:

```csharp
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [Header("Notifications")]
    public NotificationPanel notificationPanel;
    public AudioClip levelUpSound;
    
    private void LevelUp()
    {
        currentLevel++;
        skillPoints += SKILL_POINTS_PER_LEVEL;
        
        // Show notification
        if (notificationPanel != null)
        {
            string message = $"Level {currentLevel} Reached!";
            notificationPanel.ShowNotification(message, levelUpSound);
        }
        
        onLevelUp?.Invoke(currentLevel);
        // ... rest of your code
    }
}
```

### 2. XP Gain Notifications

```csharp
public void AddExperience(int amount)
{
    if (currentLevel >= maxLevel) return;
    
    currentXP += amount;
    onXPGained?.Invoke(amount);
    
    // Show XP notification
    if (notificationPanel != null)
    {
        notificationPanel.ShowNotification($"+{amount} XP");
    }
    
    CheckLevelUp();
}
```

### 3. Mission Complete Notifications

```csharp
public void CompleteMission(string missionName)
{
    // ... your mission completion logic
    
    if (notificationPanel != null)
    {
        notificationPanel.ShowNotification($"{missionName} Complete!", missionCompleteSound, 4f);
    }
}
```

### 4. Item Pickup Notifications

```csharp
void OnItemPickup(string itemName, int quantity)
{
    if (notificationPanel != null)
    {
        string message = quantity > 1 
            ? $"Picked up {itemName} x{quantity}" 
            : $"Picked up {itemName}";
        notificationPanel.ShowNotification(message, itemPickupSound);
    }
}
```

---

## Using the Notification Manager (Advanced)

For managing multiple notifications and queuing:

### Setup NotificationManager

1. Create empty GameObject: `/GameSystems/NotificationManager`
2. Add Component → `NotificationManager`
3. Assign your NotificationPanel to "Default Notification Panel"

### Usage via Manager

```csharp
// From anywhere in your code
NotificationManager.Instance.ShowNotification("Message here!");

// With specific sound
NotificationManager.Instance.ShowNotification("Level Up!", levelUpSound);

// Pre-defined notification types
NotificationManager.Instance.ShowMissionNotification("Mission Complete!");
NotificationManager.Instance.ShowLevelUpNotification("Level 5 Reached!");
NotificationManager.Instance.ShowItemNotification("Found Medkit!");
NotificationManager.Instance.ShowWarningNotification("Low Health!");
```

### Create Sound Library

1. In NotificationManager component, create a new Sound Library
2. Assign different audio clips for different event types:
   - Mission Sound
   - Level Up Sound
   - Item Sound
   - Warning Sound
   - Achievement Sound
   - Combat Sound

---

## Events System

The NotificationPanel fires events you can subscribe to:

```csharp
using UnityEngine;
using UnityEngine.Events;

public class NotificationListener : MonoBehaviour
{
    public NotificationPanel notificationPanel;
    
    void Start()
    {
        notificationPanel.onNotificationShown.AddListener(OnNotificationShown);
        notificationPanel.onNotificationHidden.AddListener(OnNotificationHidden);
    }
    
    void OnNotificationShown(string message)
    {
        Debug.Log($"Notification shown: {message}");
        // Play additional effects, pause game, etc.
    }
    
    void OnNotificationHidden()
    {
        Debug.Log("Notification hidden");
        // Resume game, etc.
    }
}
```

---

## Adding Animations (Optional)

### Step 1: Create Animator

1. Select your notification panel
2. Add Component → `Animator`
3. Create a new Animator Controller

### Step 2: Create Animation Clips

Create two animation clips:

**Show Animation:**
- Fade in alpha (CanvasGroup)
- Slide in from left/right
- Scale up

**Hide Animation:**
- Fade out alpha
- Slide out
- Scale down

### Step 3: Setup Triggers

In the Animator Controller:
1. Create triggers: "Show" and "Hide"
2. Create transitions from "Idle" to "Show" when Show trigger fires
3. Create transition from "Show" to "Hide" when Hide trigger fires

### Step 4: Assign to NotificationPanel

In the NotificationPanel component:
- Panel Animator: (your Animator)
- Show Trigger: "Show"
- Hide Trigger: "Hide"

---

## Audio Setup

### Adding Audio Clips

1. Import your audio files to `/Assets/Audio/Notifications/`
2. Assign them to the NotificationPanel or NotificationManager

### Recommended Audio Clips

- **Level Up**: Triumphant sound (0.5-1s)
- **Mission Complete**: Success chime (1-2s)
- **Item Pickup**: Quick pop/blip (0.2-0.5s)
- **Warning**: Alert beep (0.3-0.8s)
- **Achievement**: Fanfare (1-3s)

### Audio Source Settings

On the AudioSource component:
- Play On Awake: ✗ Unchecked
- Loop: ✗ Unchecked
- Volume: 0.7 - 1.0
- Spatial Blend: 0 (2D)

---

## Troubleshooting

### Notification doesn't appear

1. Check that the GameObject is not disabled in hierarchy
2. Ensure "Start Disabled" is checked in NotificationPanel
3. Verify Message Text reference is assigned
4. Check Console for errors

### Notification stays visible

1. Check Display Duration is not set to 0
2. Ensure no errors in Console
3. Try calling `HideNotification()` manually

### Audio doesn't play

1. Check Audio Source is assigned
2. Verify Audio Clip is assigned
3. Check Audio Source volume is not 0
4. Ensure Audio Listener exists in scene

### Text doesn't update

1. Ensure Message Text reference is assigned
2. Check the TextMeshProUGUI component exists
3. Use "Auto-Find Message Text" button

---

## API Reference

### NotificationPanel Methods

```csharp
// Show notification with message only
void ShowNotification(string message)

// Show with custom sound
void ShowNotification(string message, AudioClip sound)

// Show with custom duration
void ShowNotification(string message, float duration)

// Show with sound and duration
void ShowNotification(string message, AudioClip sound, float duration)

// Hide immediately
void HideNotification()

// Check if currently showing
bool IsShowing()
```

### NotificationManager Methods

```csharp
// Basic notifications
void ShowNotification(string message)
void ShowNotification(string message, AudioClip sound)
void ShowNotification(string message, float duration)
void ShowNotification(string message, AudioClip sound, float duration)

// Typed notifications (uses sound library)
void ShowMissionNotification(string message)
void ShowLevelUpNotification(string message)
void ShowItemNotification(string message)
void ShowWarningNotification(string message)

// Queue management
void ClearQueue()
```

---

## Best Practices

1. **Keep messages short** - 1-2 lines max for readability
2. **Use consistent durations** - 2-3 seconds for most notifications
3. **Audio feedback** - Helps players notice notifications
4. **Don't spam** - Use queuing for multiple notifications
5. **Test visibility** - Ensure panel is visible against all backgrounds
6. **Accessibility** - Consider color blind friendly colors

---

## Example Integration Checklist

- [ ] NotificationPanel component added to UI element
- [ ] Message text reference assigned
- [ ] AudioSource component added
- [ ] Start Disabled is checked
- [ ] Display duration set (2-4 seconds)
- [ ] Audio clips imported and assigned
- [ ] Tested in Play Mode
- [ ] Integrated with ProgressionManager (level up)
- [ ] Integrated with XP gain
- [ ] Integrated with mission system
- [ ] Integrated with loot system
- [ ] Sound library configured (if using NotificationManager)

---

## Next Steps

1. **Add more notification panels** for different UI positions
2. **Create notification categories** (info, warning, success, error)
3. **Add visual effects** (particles, glow, pulse)
4. **Implement priority system** for important notifications
5. **Add notification history log** for players to review

---

**Your notification system is now ready to use!** 

Simply call `notificationPanel.ShowNotification("Your message")` from any script to display notifications.
