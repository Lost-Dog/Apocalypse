# Civilian Spawner - Quick Fix

## 🔴 Spawner Stopped Working

### ⚡ 30-Second Fix

**Select:** `/GameSystems/CivilianSpawner`

**Click these 2 buttons:**
1. **"Auto-Find Civilian Prefabs"**
2. **"Set Recommended Settings"**

**Test:** Enter Play Mode

✅ **Done!**

---

## 🔍 What Was Wrong?

### Issue Found:
```
Initial Pool Size: 1 ❌ (Only 1 character!)
Max Active Characters: 5 ⚠️ (Too low)
```

### What This Means:
- Only **1 character instance** was created
- That same character gets reused
- Spawner appears to "stop" because pool is exhausted

### Fix Applied:
```
Initial Pool Size: 1 → 30 ✓
Max Active Characters: 5 → 20 ✓
```

Now spawner creates **30 character instances** and can have **20 active at once**!

---

## 🧪 Quick Test

### In Play Mode:

1. Select `CivilianSpawner`
2. Check **Runtime Status**:
   ```
   Active Characters: 0 → 5 → 10 → 20
   Available in Pool: 30 → 20 → 10 → 10
   ```
3. ✅ Numbers should increase!

### Manual Test:

1. Click **"Spawn Random"** button (Play Mode)
2. ✅ Character spawns nearby
3. Click **"Force Spawn 5 Characters"**
4. ✅ 5 characters spawn

---

## 📋 What Each Button Does

### "Auto-Find Civilian Prefabs"
- Searches project for civilian character prefabs
- Automatically assigns them to the spawner
- Looks for: `SM_Chr_*` prefabs

### "Set Recommended Settings"
- Max Active Characters: **20**
- Initial Pool Size: **30**
- Spawn Interval: **2 seconds**
- Distances: **30m-100m spawn**, **120m deactivate**
- Auto Spawn: **Enabled**

### "Enable Debug Logging"
- Shows spawn events in Console
- Shows debug gizmos in Scene view
- Helps troubleshoot issues

---

## ✅ Checklist

- [ ] Selected `/GameSystems/CivilianSpawner`
- [ ] Clicked "Auto-Find Civilian Prefabs"
- [ ] Clicked "Set Recommended Settings"
- [ ] Entered Play Mode
- [ ] Waited 10 seconds
- [ ] Saw characters spawning ✓

---

## 🎯 Expected Result

```
Game starts
    ↓
Spawner creates 30 character instances
    ↓
Every 2 seconds, spawns 1 character
    ↓
Characters spawn 30-100m from player
    ↓
Max 20 active characters at once
    ↓
Characters deactivate when >120m away
    ↓
Pool reuses deactivated characters
```

---

## 🐛 Still Not Working?

### If no characters spawn at all:

1. **Check for prefabs:**
   - Look in Inspector under "Civilian Prefabs"
   - Should show 3+ prefabs
   - If empty, click "Auto-Find Civilian Prefabs"

2. **Check for Player:**
   - Player GameObject must have tag "Player"
   - Find player, set Tag in Inspector

3. **Check NavMesh:**
   - Open: Window > AI > Navigation
   - Click "Bake" tab
   - Click "Bake" button

### If only a few characters spawn:

- ✓ You already fixed this!
- Initial Pool Size is now 30
- Should spawn up to 20 characters

---

## 🎉 Success Indicators

✅ Diagnostics section shows no errors  
✅ Active Characters count increases  
✅ Characters visible in Scene view  
✅ Characters spawn continuously  
✅ No console errors  

---

**Your spawner is now fixed!** 🎮
