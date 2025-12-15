# Loot UI Manager - Quick Setup

## ⚡ 30-Second Setup

**GameObject:** `/UI/HUD/ScreenSpace/LootUIManager`

### Steps:

1. Select the `LootUIManager` GameObject
2. Find the `LootUIManager` component in Inspector
3. Click **"Add Audio Source"** button
4. Click **"Enable Start Inactive"** button
5. ✅ Done!

---

## 🎵 Add Audio (Recommended)

1. Import loot pickup sound (0.3-1s, blip/chime/coin sound)
2. Drag to **"Loot Collected Sound"** field
3. Test by collecting loot

---

## ✨ What It Does

### Before:
- Panel always visible ❌
- No audio feedback ❌
- Shows loot info ✓

### After:
- ✅ Panel starts **HIDDEN**
- ✅ Shows **briefly** when loot collected (3 seconds)
- ✅ Plays **audio** when loot collected
- ✅ **Auto-hides** after display duration
- ✅ Color-coded by rarity

---

## 🎯 How It Works

```
Game starts → Panel HIDDEN
    ↓
Collect loot → Panel SHOWS + Audio
    ↓
Wait 3 seconds → Panel HIDES
```

---

## 🎨 Rarity Colors

| Rarity | Color |
|--------|-------|
| Common | White |
| Uncommon | Green |
| Rare | Blue |
| Epic | Purple |
| Legendary | Orange |

---

## 🧪 Testing

1. Enter Play Mode
2. Collect any loot item
3. ✅ Panel appears showing item + gear score
4. ✅ Audio plays
5. ✅ Panel disappears after 3 seconds

---

## 🔧 Settings

```
Panel Behavior
└── Start Inactive: ✓ (panel hidden at start)

Notification Display Duration: 3.0s
└── How long panel shows

Audio
├── Audio Source: (auto-added)
└── Loot Collected Sound: (your audio clip)
```

---

## 📋 Checklist

- [ ] "Add Audio Source" clicked
- [ ] "Enable Start Inactive" clicked
- [ ] Audio clip imported
- [ ] Audio clip assigned
- [ ] Tested in Play Mode
- [ ] Panel hides at start ✓
- [ ] Panel shows on loot collection ✓
- [ ] Audio plays ✓
- [ ] Panel auto-hides ✓

---

**Done!** Your loot UI is ready! 🎉
