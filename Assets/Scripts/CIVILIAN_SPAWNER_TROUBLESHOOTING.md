# Civilian Spawner - Troubleshooting Guide

## 🔴 Problem: Spawner Has Stopped Spawning

### Common Causes & Solutions

---

## ⚡ Quick Fix (30 Seconds)

1. Select `/GameSystems/CivilianSpawner`
2. Find `CharacterSpawner` component
3. Look for **Diagnostics** section (should auto-expand)
4. Click **"Auto-Find Civilian Prefabs"** button
5. Click **"Set Recommended Settings"** button
6. ✅ Enter Play Mode and test!

---

## 🔍 Detailed Diagnostics

### Issue 1: No Civilian Prefabs Assigned

**Symptoms:**
- ⚠️ Error: "NO CIVILIAN PREFABS ASSIGNED!"
- Nothing spawns at all
- Console warning: "No civilian prefabs assigned!"

**Fix:**
```
1. Click "Auto-Find Civilian Prefabs" button
   OR
2. Manually drag civilian prefabs to "Civilian Prefabs" list:
   - SM_Chr_Business_Male_01
   - SM_Chr_Homeless_Male_01
   - SM_Chr_Press_Male_01
   - etc.
```

---

### Issue 2: Initial Pool Size Too Low

**Symptoms:**
- ⚠️ Warning: "Initial Pool Size (1) is very low"
- Only 1-2 characters spawn
- Spawning stops quickly

**Current Setting:**
- Initial Pool Size: **1** ❌ (Too low!)

**Fix:**
```
1. Click "Set Recommended Settings"
   OR
2. Manually set:
   Initial Pool Size: 30
   Max Active Characters: 20
```

**Why This Matters:**
- Pool Size = Number of character instances created at start
- If pool is too small, spawner runs out of characters
- Recommended: 30 (creates 30 character instances)

---

### Issue 3: Max Active Characters Too Low

**Symptoms:**
- Only a few characters spawn
- Spawning stops after reaching limit

**Current Setting:**
- Max Active Characters: **5**

**Fix:**
```
Recommended: 20
For crowded areas: 30-50
For performance: 10-15
```

---

### Issue 4: Auto Spawn Disabled

**Symptoms:**
- ⚠️ Warning: "Auto Spawn is DISABLED"
- No characters spawn automatically
- Manual spawn works, but not automatic

**Fix:**
```
Enable Auto Spawn: ✓ Check this box
```

---

### Issue 5: No Player Found

**Symptoms:**
- ⚠️ Error: "No GameObject with 'Player' tag found!"
- Console warning: "Player not found!"
- Characters don't spawn around player

**Fix:**
```
1. Find your player GameObject in scene
2. Select it
3. In Inspector, set Tag: "Player"
```

---

### Issue 6: NavMesh Not Baked

**Symptoms:**
- Console warning: "Failed to find valid spawn position"
- Characters don't spawn even with prefabs assigned
- Spawn attempts fail

**Fix:**
```
1. Open Window > AI > Navigation
2. Select "Bake" tab
3. Click "Bake" button
4. Wait for NavMesh to generate
```

---

## 📊 Current Configuration (Your Scene)

Based on the CivilianSpawner inspection:

```
Current Settings:
├── Max Active Characters: 5 ⚠️ (Low - recommended 20)
├── Initial Pool Size: 1 ❌ (Too low! - recommended 30)
├── Spawn Interval: 2s ✓ (Good)
├── Min Spawn Distance: 30m ✓ (Good)
├── Max Spawn Distance: 50m ✓ (Good)
├── Deactivate Distance: 80m ✓ (Good)
└── Enable Auto Spawn: ✓ (Enabled)
```

**Problems Identified:**
1. ❌ **Initial Pool Size is only 1** - This is the main issue!
2. ⚠️ Max Active Characters is only 5 - Should be higher for better world population

---

## ✅ Recommended Settings

For apocalyptic city environment:

```
Character Prefabs
├── Civilian Prefabs: 3-10 different prefabs ✓

Spawn Settings
├── Max Active Characters: 20
├── Initial Pool Size: 30
└── Spawn Interval: 2.0s

Distance Settings
├── Min Spawn Distance: 30m
├── Max Spawn Distance: 100m
└── Deactivate Distance: 120m

Performance Settings
├── Distance Check Interval: 1.0s
└── Enable Auto Spawn: ✓ Checked
```

---

## 🧪 Testing Steps

### Test 1: Basic Spawning

1. Click **"Set Recommended Settings"**
2. Enter Play Mode
3. Wait 5-10 seconds
4. ✅ You should see characters spawning around you

### Test 2: Manual Spawn (Play Mode)

1. Enter Play Mode
2. Select CivilianSpawner in Hierarchy
3. In Inspector, click **"Spawn Random"**
4. ✅ Character should spawn nearby

### Test 3: Pool Status (Play Mode)

1. Enter Play Mode
2. Select CivilianSpawner
3. Check **Runtime Status**:
   - Active Characters: Should increase over time
   - Available in Pool: Should decrease as characters spawn

### Test 4: Distance Deactivation

1. Enter Play Mode
2. Wait for characters to spawn
3. Move far away (120m+)
4. ✅ Characters should deactivate
5. Move back
6. ✅ New characters should spawn

---

## 🔧 Debug Mode

Enable detailed logging to see what's happening:

```
1. Select CivilianSpawner
2. Click "Enable Debug Logging"
3. Enter Play Mode
4. Open Console (Ctrl+Shift+C)
5. Watch for spawn events:
   - "Initialized pool for..."
   - "Spawned [name] at [position]"
   - "Deactivated [name]..."
```

---

## 🎯 Step-by-Step Fix

Follow these steps in order:

### Step 1: Fix Prefabs
```
☐ Click "Auto-Find Civilian Prefabs"
☐ Verify prefabs appear in list (should show 3+)
```

### Step 2: Fix Settings
```
☐ Click "Set Recommended Settings"
☐ Verify Initial Pool Size: 30
☐ Verify Max Active Characters: 20
☐ Verify Enable Auto Spawn: ✓ Checked
```

### Step 3: Verify Player
```
☐ Find player GameObject
☐ Check Tag is set to "Player"
```

### Step 4: Check NavMesh
```
☐ Open Window > AI > Navigation
☐ Verify NavMesh is baked (blue overlay in scene)
☐ If not, click "Bake"
```

### Step 5: Test
```
☐ Enter Play Mode
☐ Wait 10 seconds
☐ Verify characters are spawning
☐ Check Console for errors
```

---

## 🐛 Common Errors

### Error: "No civilian prefabs assigned!"

**Solution:**
```
Click "Auto-Find Civilian Prefabs" button
```

### Warning: "Failed to find valid spawn position"

**Solution:**
```
1. Bake NavMesh (Window > AI > Navigation > Bake)
2. Increase Max Spawn Attempts: 20
3. Increase NavMesh Sample Distance: 10
```

### Characters spawn but immediately disappear

**Solution:**
```
Increase Deactivate Distance to 150+
Characters deactivate when too far from player
```

### Only 1 character spawns

**Solution:**
```
Increase Initial Pool Size to 30
Increase Max Active Characters to 20
```

---

## 💡 Performance Tips

### For Low-End Systems

```
Max Active Characters: 10-15
Initial Pool Size: 20
Spawn Interval: 3-5s
Distance Check Interval: 2s
```

### For High-End Systems

```
Max Active Characters: 30-50
Initial Pool Size: 50
Spawn Interval: 1-2s
Distance Check Interval: 0.5s
```

---

## 📋 Checklist

Complete this checklist to fix spawning:

- [ ] CivilianSpawner GameObject exists
- [ ] CharacterSpawner component attached
- [ ] Civilian prefabs assigned (3+ prefabs)
- [ ] Initial Pool Size set to 30
- [ ] Max Active Characters set to 20
- [ ] Enable Auto Spawn is checked
- [ ] Player GameObject has "Player" tag
- [ ] NavMesh is baked in scene
- [ ] Tested in Play Mode
- [ ] Characters spawning successfully

---

## 🎉 Success!

After applying fixes, you should see:

```
✓ Characters spawn around player
✓ Active characters increase to 20
✓ Characters deactivate when far away
✓ New characters spawn continuously
✓ No errors in Console
```

---

## 🆘 Still Not Working?

1. **Enable Debug Logging** (button in Inspector)
2. **Enter Play Mode**
3. **Open Console** and look for:
   - Red errors → Fix the error
   - Yellow warnings → Address warnings
   - "Spawned..." logs → Spawner is working!
4. **Click "Force Spawn 5 Characters"** in Play Mode
   - If this works → Auto spawn settings issue
   - If this fails → Prefabs/NavMesh issue

---

## 📞 Need More Help?

Check these in order:

1. ✅ All prefabs assigned
2. ✅ Pool size is 30+
3. ✅ Max active is 20+
4. ✅ Player tag is set
5. ✅ NavMesh is baked
6. ✅ No errors in Console

If all checked and still not working:
- Check if civilian prefabs have required components
- Verify NavMesh is baked in the play area
- Try manually spawning in Play Mode
- Check console for specific error messages

---

**Your main issue:** Initial Pool Size is only **1** - this means the spawner only creates 1 character instance total, which gets reused. Click **"Set Recommended Settings"** to fix this immediately!
