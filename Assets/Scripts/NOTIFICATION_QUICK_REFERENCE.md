# Notification System - Quick Reference Card

## 🚀 5-Second Setup

1. Select: `/UI/HUD/ScreenSpace/Bottom/Bottom Left/HUD_Apocalypse_Comms_01`
2. Add Component: `NotificationPanel`
3. Click: **"Auto-Find Message Text"**
4. Click: **"Add Audio Source"**
5. ✅ Done!

---

## 📝 Basic Usage

```csharp
// Simple message
notificationPanel.ShowNotification("Hello!");

// With duration (seconds)
notificationPanel.ShowNotification("Quick message", 2f);

// With sound
notificationPanel.ShowNotification("Level Up!", levelUpSound);

// With sound and duration
notificationPanel.ShowNotification("Achievement!", sound, 5f);
```

---

## 🎯 Common Scenarios

### Level Up
```csharp
notificationPanel.ShowNotification($"LEVEL {newLevel}!", levelUpSound, 4f);
```

### XP Gain
```csharp
notificationPanel.ShowNotification($"+{xp} XP", xpSound, 2f);
```

### Mission Complete
```csharp
notificationPanel.ShowNotification("MISSION COMPLETE", missionSound, 4f);
```

### Item Pickup
```csharp
notificationPanel.ShowNotification($"Picked up {itemName}", itemSound, 2.5f);
```

### Warning
```csharp
notificationPanel.ShowNotification("LOW HEALTH!", warningSound, 3f);
```

---

## 🔧 Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Display Duration | 3.0s | How long notification shows |
| Start Disabled | ✓ | Starts hidden |
| Message Text | Auto | TextMeshProUGUI reference |
| Audio Source | Auto | For sound playback |

---

## 🎵 Audio Setup

1. Import audio clips to `/Assets/Audio/Notifications/`
2. Assign to `Default Notification Sound` field
3. Or pass specific sounds per notification

**Recommended Clip Lengths:**
- Level Up: 0.5-1s
- XP/Item: 0.2-0.5s
- Mission: 1-2s
- Warning: 0.3-0.8s

---

## 🔌 Integration with ProgressionManager

```csharp
public class ProgressionManager : MonoBehaviour
{
    public NotificationPanel notificationPanel;
    public AudioClip levelUpSound;
    
    private void LevelUp()
    {
        currentLevel++;
        
        // Add this:
        if (notificationPanel != null)
        {
            notificationPanel.ShowNotification(
                $"LEVEL {currentLevel}!", 
                levelUpSound, 
                4f
            );
        }
    }
}
```

---

## ✨ Advanced: Notification Manager

For queuing and multiple panels:

```csharp
// Singleton access from anywhere
NotificationManager.Instance.ShowNotification("Message");

// Typed notifications (uses sound library)
NotificationManager.Instance.ShowMissionNotification("Complete!");
NotificationManager.Instance.ShowLevelUpNotification("Level 5!");
NotificationManager.Instance.ShowItemNotification("Found Medkit!");
NotificationManager.Instance.ShowWarningNotification("Danger!");
```

---

## 🧪 Testing

**In Editor (Play Mode):**
1. Select notification panel
2. Enter test message in Inspector
3. Click "Show Test Notification"

**Via Code:**
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.N))
    {
        notificationPanel.ShowNotification("Test!");
    }
}
```

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| Doesn't show | Check "Start Disabled" is checked |
| No text | Click "Auto-Find Message Text" |
| No sound | Add AudioSource component |
| Stays visible | Check Display Duration > 0 |

---

## 📋 Checklist

- [ ] NotificationPanel added
- [ ] Message Text assigned
- [ ] AudioSource added
- [ ] Start Disabled checked
- [ ] Tested in Play Mode
- [ ] Integrated with game systems
- [ ] Audio clips assigned

---

## 🎬 Next Steps

1. **Test** the notification in Play Mode
2. **Add audio clips** for different events
3. **Integrate** with ProgressionManager
4. **Customize** duration and messages
5. **Add animations** (optional)

**You're all set!** 🎉
