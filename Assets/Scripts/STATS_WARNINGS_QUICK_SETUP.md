# Stats Warnings Panel - Quick Setup

## ⚡ 30-Second Setup

**GameObject:** `/UI/HUD/ScreenSpace/Bottom/Stats_Warnings`

### Steps:

1. Select the `Stats_Warnings` GameObject
2. Find the `PlayerStatusIndicators` component in Inspector
3. Click **"Add Audio Source"** button
4. Click **"Enable Panel Behavior"** button
5. ✅ Done!

---

## 🎵 Add Your Audio (Optional but Recommended)

1. Import your warning audio clip (0.3-1 second beep/alert)
2. Drag it to the **"Warning Sound"** field
3. Test by damaging the player in Play Mode

---

## ✨ What It Does

### Before Changes:
- Panel always visible ❌
- No audio feedback ❌
- Warnings show when triggered ✓

### After Changes:
- ✅ Panel starts **HIDDEN**
- ✅ Shows automatically when warnings trigger
- ✅ Plays audio when warning becomes active
- ✅ **Auto-hides** when all stats return to normal

---

## 🎯 When Warnings Appear

| Warning | Trigger | Display |
|---------|---------|---------|
| Low Health | Health ≤ 50% | "LOW HEALTH" (yellow) |
| Critical Health | Health ≤ 25% | "CRITICAL" (red) |
| Cold | Temperature ≤ 40% | "COLD" (yellow) |
| Freezing | Temperature ≤ 20% | "FREEZING" (red) |
| Infected | Infection ≥ 50 | "INFECTED" (yellow) |
| Severe Infection | Infection ≥ 75 | "SEVERE" (red) |

---

## 🧪 Testing

1. Enter Play Mode
2. Damage the player to drop health below 50%
3. ✅ Panel appears with "LOW HEALTH"
4. ✅ Audio plays (if assigned)
5. Heal back above 50%
6. ✅ Panel disappears

---

## 📋 Settings Overview

```
Panel Behavior
├── Start Disabled: ✓ (panel hidden at start)
└── Auto Hide When No Warnings: ✓ (auto-hides)

Audio
├── Audio Source: (auto-added component)
└── Warning Sound: (your audio clip here)
```

---

## 🔧 Customization

### Change Warning Thresholds

Want warnings to appear earlier/later?

```
Health Warning: 0.5 → Change to trigger at different %
Temperature Warning: 0.4 → Adjust as needed
Infection Warning: 50 → Modify threshold
```

### Disable Auto-Hide

Want panel always visible?

1. Uncheck "Auto Hide When No Warnings"
2. Uncheck "Start Disabled"

---

## ✅ Checklist

- [ ] "Add Audio Source" button clicked
- [ ] "Enable Panel Behavior" button clicked
- [ ] Audio clip imported (optional)
- [ ] Audio clip assigned (optional)
- [ ] Tested in Play Mode
- [ ] Verified panel hides at start
- [ ] Verified panel shows when health drops
- [ ] Verified audio plays (if assigned)

---

**That's it!** Your Stats Warnings panel is ready! 🎉
