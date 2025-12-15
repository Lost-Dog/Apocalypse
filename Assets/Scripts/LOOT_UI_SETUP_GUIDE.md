# Loot UI Manager - Setup Guide

## Overview

Your Loot UI at `/UI/HUD/ScreenSpace/LootUIManager` has been updated to:
- ✅ Start **inactive** (hidden)
- ✅ Display **briefly** when player collects loot (default: 3 seconds)
- ✅ Support for **audio clip** playback when loot is collected
- ✅ **Auto-hide** after display duration

---

## ⚡ Quick Setup (30 Seconds)

1. Select: `/UI/HUD/ScreenSpace/LootUIManager`
2. Find the `LootUIManager` component in Inspector
3. Click **"Add Audio Source"** button
4. Click **"Enable Start Inactive"** button
5. ✅ Done!

---

## 🎵 Add Your Loot Sound (Optional)

1. Import your loot collection sound (0.3-1 second)
   - Recommended: Item pickup sound, coin sound, blip, or chime
2. Drag it to the **"Loot Collected Sound"** field
3. Test by collecting loot in Play Mode

---

## 🎯 How It Works

### Loot Collection Flow

```
Game starts
    ↓
LootUIManager is HIDDEN (inactive)
    ↓
Player collects loot
    ↓
⚡ Event triggered!
    ↓
Panel SHOWS (becomes active)
Audio plays (if assigned)
Displays: Item name + Gear Score
Color coded by rarity
    ↓
Wait 3 seconds (default)
    ↓
Panel HIDES (becomes inactive)
```

---

## 📋 Settings

### Panel Behavior (New)

```
Start Inactive: ✓ Checked
└── Panel will be hidden at game start
```

### Notification Display

```
Notification Display Duration: 3.0 seconds
└── How long the loot notification shows
```

### Audio (New)

```
Audio Source: (auto-added component)
└── Plays loot collection sounds

Loot Collected Sound: (your audio clip)
└── Plays when ANY loot is collected
```

---

## 🎨 Rarity Colors

The panel automatically color-codes loot by rarity:

| Rarity | Color | Example |
|--------|-------|---------|
| Common | White | Standard items |
| Uncommon | Green | Better items |
| Rare | Blue | Good items |
| Epic | Purple | Great items |
| Legendary | Orange | Best items |

The background tint and text color match the rarity!

---

## 🔧 Customization

### Change Display Duration

Want loot notifications to show longer/shorter?

1. Select `/UI/HUD/ScreenSpace/LootUIManager`
2. Find `Notification Display Duration`
3. Change value:
   - **1.5s** - Very quick
   - **3.0s** - Default
   - **5.0s** - Extended display

### Always Keep Panel Visible

Don't want it to hide?

1. Uncheck "Start Inactive"
2. Panel will stay visible, content updates on loot collection

---

## 🧪 Testing

### Test 1: Loot Collection

1. Enter Play Mode
2. Collect any loot item
3. ✅ Panel should appear
4. ✅ Shows item name and gear score
5. ✅ Audio plays (if assigned)
6. ✅ Panel disappears after 3 seconds

### Test 2: Multiple Items

1. Enter Play Mode
2. Collect first item
3. ✅ Panel shows item 1
4. Collect second item quickly
5. ✅ Panel updates to item 2
6. ✅ Timer resets (shows for another 3 seconds)

### Test 3: Rarity Colors

1. Enter Play Mode
2. Collect items of different rarities
3. ✅ Common items = White
4. ✅ Rare items = Blue
5. ✅ Legendary items = Orange
6. Background tint matches text color

---

## 🎵 Audio Setup

### Recommended Audio

**Loot Collection Sound:**
- **Type**: Item pickup, coin, blip, chime
- **Length**: 0.3-1.0 seconds
- **Format**: WAV or OGG
- **Volume**: Medium

### Audio Source Settings

Auto-configured with:
- Play On Awake: ✗ Off
- Loop: ✗ Off
- Spatial Blend: 0 (2D sound)
- Volume: 1.0

Adjust volume in AudioSource component if needed.

---

## 🔌 Integration

### How Loot Collection Works

The LootUIManager automatically integrates with `LootManager`:

```
LootManager
    ↓
onItemCollected event fires
    ↓
LootUIManager.OnItemCollected()
    ↓
Shows notification with:
    • Item name
    • Gear score
    • Rarity color
    • Audio feedback
```

### Already Configured

Your LootUIManager is listening to:
- `LootManager.onItemCollected` event
- Automatically initialized by game systems
- No additional setup needed!

---

## 📦 Components Breakdown

### LootNotificationPanel Child

```
/LootUIManager
├── /LootNotificationPanel (shows/hides)
    ├── /LootRarirtText (displays item name)
    └── /LootGearScoreText (displays gear score)
```

### Component References

```
Loot Notification Panel: (GameObject to show/hide)
Loot Rarity Text: (Item name display)
Loot Gear Score Text: (GS number display)
Loot Background Image: (Color tinted by rarity)
```

All references should be auto-assigned in the scene!

---

## 🐛 Troubleshooting

### Panel doesn't hide at start

**Check:**
1. "Start Inactive" is checked ✓
2. No errors in Console
3. LootManager has properly initialized the UI

**Fix:**
Click "Enable Start Inactive" button in Inspector

---

### Panel doesn't show when collecting loot

**Check:**
1. LootManager exists in scene
2. LootManager has initialized the LootUIManager
3. No errors in Console
4. Panel references are assigned

**Test:**
- Check if `LootManager.onItemCollected` event fires
- Verify event listener is subscribed
- Look for initialization errors in Console

---

### Audio doesn't play

**Check:**
1. AudioSource component exists
2. Loot Collected Sound is assigned
3. AudioSource volume is not 0
4. Audio Listener exists in scene

**Fix:**
1. Click "Add Audio Source" button
2. Assign your audio clip to "Loot Collected Sound"
3. Test by collecting loot

---

### Wrong rarity colors

**Check:**
1. LootManager exists and is initialized
2. LootManager has rarity color settings configured
3. Falls back to default colors if LootManager is null

**Default Colors:**
- Common: White
- Uncommon: Green
- Rare: Blue
- Epic: Purple (0.6, 0, 1)
- Legendary: Orange (1, 0.5, 0)

---

### Panel shows but no text

**Check:**
1. LootRarityText reference is assigned
2. LootGearScoreText reference is assigned
3. TextMeshProUGUI components exist
4. Item data is not null

**Fix:**
- Select LootUIManager
- Verify all text references in Inspector
- Re-assign if needed

---

## 💡 Best Practices

### Display Duration

- **Fast-paced games**: 1.5-2 seconds
- **Standard games**: 3 seconds (default)
- **Slow-paced/inventory games**: 4-5 seconds

### Audio Feedback

- ✅ DO: Use short, satisfying pickup sounds
- ✅ DO: Keep volume moderate
- ❌ DON'T: Use long audio clips (>1 second)
- ❌ DON'T: Make it too loud (startling)

### Visual Design

- Panel already color-codes by rarity ✓
- Background auto-tints based on rarity ✓
- Text color matches rarity ✓
- Clean, brief display ✓

---

## 🎮 Player Experience

### What Players See

```
[Picks up Legendary Weapon]
    ↓
Panel slides in (orange glow)
"Legendary Rifle"
"GS 485"
*Satisfying pickup sound plays*
    ↓
[3 seconds later]
Panel disappears
```

### Multiple Pickups

If player collects items quickly:
- Panel updates to newest item
- Timer resets each time
- Each pickup plays audio
- Always shows most recent loot

---

## 📊 Summary

### ✅ What Changed

1. **Panel starts inactive** - Hidden until loot is collected
2. **Auto-shows on loot collection** - Activates when player picks up items
3. **Audio support added** - Plays sound when loot is collected
4. **Auto-hides** - Disappears after display duration (default 3s)
5. **Custom editor** - Quick setup buttons in Inspector

### 🎯 Current Features

- ✓ Starts hidden
- ✓ Shows when loot collected
- ✓ Displays item name
- ✓ Displays gear score
- ✓ Color-coded by rarity (white/green/blue/purple/orange)
- ✓ Background tint by rarity
- ✓ Audio feedback (when assigned)
- ✓ Auto-hides after duration
- ✓ Updates if multiple items collected quickly

### 📋 Quick Checklist

- [ ] "Add Audio Source" button clicked
- [ ] "Enable Start Inactive" button clicked
- [ ] Loot collection audio imported
- [ ] Audio assigned to "Loot Collected Sound"
- [ ] Tested in Play Mode (collect loot)
- [ ] Verified panel appears when loot collected
- [ ] Verified audio plays
- [ ] Verified panel hides after 3 seconds
- [ ] Tested with different rarity items

---

## 🚀 Next Steps

1. **Import your loot pickup sound**
2. **Assign it to the Loot Collected Sound field**
3. **Test in Play Mode** by collecting loot
4. **Adjust display duration** if needed (faster/slower)
5. **(Optional) Customize rarity colors** in LootManager

---

## 🎉 You're All Set!

Your Loot UI Manager will now:
- Start hidden
- Show briefly when you collect loot
- Play audio feedback
- Display item name and gear score
- Color-code by rarity
- Auto-hide after 3 seconds

**Collect some loot and enjoy the satisfying feedback!** 🎮
