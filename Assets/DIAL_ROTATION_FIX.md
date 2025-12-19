# Dial Rotation Fix - Quick Reference

## ✅ FIXED: Dials Now Support Needle Rotation!

The dial scripts have been updated to support **both** methods of display:
1. **Radial Fill** - The dial fills up like a circle (color change only)
2. **Needle Rotation** - A needle/pointer rotates like a speedometer gauge

---

## 🎯 How to Enable Needle Rotation

### **Step 1: Find Your Dial Hierarchy**
In your Unity scene, find the dial GameObject (e.g., Stat_Dial_00, Stat_Dial_01, etc.)

Look for a child object named one of these:
- `Needle`
- `Pointer`
- `IMG_Dial_Indicator`
- `Indicator`
- `Arrow`

The script will **automatically find** these common names!

### **Step 2: Configure Inspector Settings**

Select your dial GameObject and check the **Needle Rotation Settings**:

```
Enable Needle Rotation: ✅ (checked)
Min Rotation Angle: -120  (leftmost position)
Max Rotation Angle: 120   (rightmost position)
Rotation Axis: (0, 0, 1)  (Z-axis for 2D UI)
```

### **Step 3: Assign Needle Manually (if auto-find fails)**

If the script doesn't automatically find the needle:
1. Select your dial GameObject
2. Locate the **Needle Transform** field in Inspector
3. Drag the needle/pointer child object into that field

---

## 📐 Adjusting Rotation Angles

Different dial designs use different angle ranges. Common configurations:

**Standard Gauge (240° range):**
```
Min Rotation Angle: -120
Max Rotation Angle: 120
```

**Half Circle (180° range):**
```
Min Rotation Angle: -90
Max Rotation Angle: 90
```

**3/4 Circle (270° range):**
```
Min Rotation Angle: -135
Max Rotation Angle: 135
```

**Full Circle (360° range):**
```
Min Rotation Angle: 0
Max Rotation Angle: 360
```

**Inverted/Reversed:**
```
Min Rotation Angle: 120
Max Rotation Angle: -120
```

---

## 🎨 Using Both Methods Together

You can have **BOTH** radial fill AND needle rotation enabled:
- The dial background changes color (radial fill)
- The needle rotates to point at the value

To enable both:
1. Assign `dialFillImage` (for color/fill)
2. Assign `needleTransform` (for rotation)
3. Check both `enableColorGradient` and `enableNeedleRotation`

---

## 🔧 Testing Your Setup

1. **Enter Play Mode**
2. **Open SurvivalManager** in Inspector
3. **Adjust the sliders:**
   - Infection: `currentInfection` (0-100)
   - Thirst: `currentThirst` (0-100)
   - Hunger: `currentHunger` (0-100)
4. **Watch the needles rotate!**

---

## 📝 Scripts Updated

All three dial scripts now support needle rotation:
- ✅ [InfectionDial.cs](Assets/Scripts/InfectionDial.cs)
- ✅ [ThirstDial.cs](Assets/Scripts/ThirstDial.cs)
- ✅ [HungerDial.cs](Assets/Scripts/HungerDial.cs)

---

## 🎯 Common Needle Names in Synty HUD Assets

The script auto-searches for these child names:
- `IMG_Dial_Indicator` (Synty standard)
- `Needle`
- `Pointer`
- `Indicator`
- `Arrow`

If your dial uses a different name, just assign it manually in the Inspector!

---

## 🐛 Troubleshooting

### Needle Not Rotating?
✅ Check `enableNeedleRotation` is checked
✅ Verify `needleTransform` is assigned
✅ Ensure the needle is a child of the dial GameObject
✅ Check rotation angles are different (min ≠ max)

### Needle Rotating Wrong Direction?
✅ Swap `minRotationAngle` and `maxRotationAngle` values

### Needle Rotating Too Fast/Slow?
✅ Adjust `fillTransitionSpeed` (default: 2.0)
✅ Higher = faster rotation
✅ Lower = slower, smoother rotation

### Needle Starting at Wrong Position?
✅ Check the needle's initial rotation in the Scene view
✅ Adjust `minRotationAngle` to match your dial's design
✅ Make sure the needle is centered at 0° when value is 0

---

## 🚀 You're All Set!

The dials will now **turn/rotate** their needles based on the stat values, just like a real speedometer or gauge!
