# Challenge Notification UI - Quick Setup

## ⚡ Quick Start (5 Minutes)

### Step 1: Create the UI Panel (2 min)

1. **Open your game scene**
2. **Find your HUD Canvas** (look for GameSystems/HUDManager in hierarchy)
3. **Right-click Canvas** → UI → Panel
4. **Name it**: `ChallengeNotificationPanel`
5. **Position it**:
   - Anchor Preset: **Top-Right**
   - Pos X: `-220`, Pos Y: `-140`
   - Width: `400`, Height: `250`

### Step 2: Add Required Components (2 min)

**On the Panel:**
1. Add Component → **Canvas Group** (for fading)
2. Add Component → **ChallengeNotificationUI** script

### Step 3: Create Text Elements (1 min)

**Quick method - Duplicate these:**

Right-click `ChallengeNotificationPanel` → UI → Text - TextMeshPro

Create these texts (you can duplicate and rename):

1. **TitleText**
   - Text: "Challenge Title"
   - Font Size: `20`
   - Color: White
   - Alignment: Center
   - Pos Y: `80`

2. **DescriptionText**
   - Text: "Challenge description goes here"
   - Font Size: `14`
   - Color: Light Gray
   - Alignment: Center
   - Pos Y: `50`

3. **DifficultyText**
   - Text: "HARD"
   - Font Size: `16`
   - Color: Orange (auto-set by script)
   - Alignment: Center
   - Pos Y: `20`

4. **RewardText**
   - Text: "+500 XP"
   - Font Size: `16`
   - Color: Yellow
   - Alignment: Center
   - Pos Y: `-10`

5. **ProgressText**
   - Text: "0 / 50"
   - Font Size: `18`
   - Bold
   - Color: White
   - Alignment: Center
   - Pos Y: `-40`

6. **TimeRemainingText**
   - Text: "Time: 05:00"
   - Font Size: `14`
   - Color: Cyan
   - Alignment: Center
   - Pos Y: `-70`

### Step 4: Add Progress Slider (Optional)

1. Right-click Panel → UI → **Slider**
2. Name: `ProgressSlider`
3. Position: Bottom of panel
4. Width: `350`
5. Set Fill Area color to match your theme

### Step 5: Wire Up to HUDManager

**IMPORTANT: This UI is managed by HUDManager (like other UI managers)**

1. **Select GameSystems/HUDManager** in hierarchy
2. **In Inspector**, find the **UI Managers** section
3. **Drag `ChallengeNotificationPanel`** into the **Challenge Notification UI** field

That's it! HUDManager will automatically initialize it when the game starts.

---

## ✅ Test It!

1. **Press Play**
2. **Wait for a challenge to spawn** (or force spawn one)
3. **You should see:**
   - Panel fades in
   - Shows challenge info
   - Progress updates live: "5 / 50" → "6 / 50"
   - Timer counts down: "04:59" → "04:58"
   - Panel stays visible until challenge ends

---

## 🎨 Styling Tips

### Division-Style Look:

**Panel Background:**
- Color: Dark Orange/Brown with alpha 0.8
- Add a border image (optional)

**Text Colors:**
- Title: White
- Description: Light Gray
- Difficulty: Auto (Easy=Green, Medium=Yellow, Hard=Orange, Extreme=Red)
- Reward: Gold/Yellow
- Progress: White
- Timer: Cyan

**Font Suggestion:**
- Use a bold, military-style font
- Or Unity's default "LiberationSans SDF" works great

### Example Layout:

```
╔════════════════════════════════════╗
║  🎯 Supply Drop Incoming           ║  ← Title
║  Secure the supply drop            ║  ← Description
║  [HARD]                            ║  ← Difficulty
║  +500 XP                           ║  ← Reward
║  ──────────────────────────────    ║  ← Progress Bar
║  15 / 50                           ║  ← Progress Text
║  Time: 04:32                       ║  ← Timer
╚════════════════════════════════════╝
```

---

## 🔧 Advanced: Multiple Challenges

If you have multiple active challenges, add cycle buttons:

1. Create two UI Buttons: `<` and `>`
2. Position them on left/right of panel
3. Wire button onClick:
   - Left button → `ChallengeNotificationUI.CycleToPreviousChallenge()`
   - Right button → `ChallengeNotificationUI.CycleToNextChallenge()`

This lets players switch between viewing different active challenges!

---

## 📊 What Gets Updated Live?

The notification panel automatically updates:

✅ **Progress** - Every kill/objective completion
✅ **Timer** - Every second
✅ **Completion** - Auto-hides when challenge completes
✅ **Next Challenge** - Auto-shows next challenge if multiple active

No manual updates needed!

---

## 🐛 Troubleshooting

**Panel doesn't show:**
- Check HUDManager has ChallengeNotificationUI assigned
- Check HUDManager.Initialize() is being called
- Check ChallengeManager exists in GameSystems
- Check Notification Panel field is assigned on the ChallengeNotificationUI component

**"Not initialized" warning:**
- Make sure HUDManager is initialized before challenges spawn
- Check GameManager initializes HUDManager properly

**Progress doesn't update:**
- Make sure ProgressText is assigned on ChallengeNotificationUI component
- Check ChallengeManager is calling `onChallengeProgress.Invoke()`

**Timer shows "00:00":**
- World events show "Active" instead
- Check challenge has timeLimit > 0
- Daily/Weekly challenges have automatic timers

**Multiple challenges don't cycle:**
- Add cycle buttons
- Or they auto-advance when one completes

---

## 🎯 Done!

You now have a Division-style persistent challenge UI that shows:
- Challenge details
- Live progress tracking
- Time remaining
- Auto-updates during gameplay

Next: Set up 3D world markers! (See `CHALLENGE_LOCATION_SETUP.md`)
