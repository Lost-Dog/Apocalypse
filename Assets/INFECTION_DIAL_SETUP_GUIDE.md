# Infection Dial Setup Guide (Stat_Dial_00)

## ✅ Script Status: READY TO USE
**InfectionDial.cs** is fully implemented and functional.

---

## 📋 Quick Setup Instructions

### **Step 1: Locate Stat_Dial_00 in Your Scene**
1. Open **Apocalypse.unity** scene
2. In Hierarchy, search for `Stat_Dial_00` (likely under Canvas/HUD/Player UI)
3. Select the GameObject

### **Step 2: Add InfectionDial Component**
1. With Stat_Dial_00 selected, click **Add Component**
2. Type `InfectionDial` and select it
3. The script will **auto-find** most components

### **Step 3: Verify Auto-Configuration**
The script should automatically:
- ✅ Find SurvivalManager.Instance
- ✅ Find the Image component (dial fill)
- ✅ Find TMP_Text for displaying infection value
- ✅ Configure Image as Radial Fill type

### **Step 4: Manual Configuration (if needed)**

If auto-find doesn't work, manually assign:

**References:**
- `survivalManager`: Drag SurvivalManager from Hierarchy
- `dialFillImage`: Drag the Image component that should fill (the dial itself)
- `needleTransform`: (NEW) Drag the child object that acts as the needle/pointer
- `infectionText`: (Optional) Drag the text component to show infection value

**Dial Settings:**
- `maxFillAmount`: 1.0 (full circle)
- `minFillAmount`: 0.0 (empty)
- `fillTransitionSpeed`: 2.0 (smooth animation speed)
- `smoothFill`: ✅ Checked (smooth transitions)

**Needle Rotation Settings (NEW):**
- `enableNeedleRotation`: ✅ Checked (enables needle turning)
- `minRotationAngle`: -120 (leftmost position in degrees)
- `maxRotationAngle`: 120 (rightmost position in degrees)
- `rotationAxis`: (0, 0, 1) - Z-axis for 2D UI rotation

**Color Settings:**
- `healthyColor`: Green (0, 255, 0) - No infection
- `warningColor`: Yellow (255, 235, 4) - Low infection
- `dangerColor`: Orange (255, 128, 0) - Moderate infection
- `criticalColor`: Red (255, 0, 0) - High infection
- `enableColorGradient`: ✅ Checked

**Infection Thresholds:**
- `warningThreshold`: 25 (infection level for yellow)
- `dangerThreshold`: 50 (infection level for orange)
- `criticalThreshold`: 75 (infection level for red)

**Text Display:**
- `showInfectionValue`: ✅ Checked
- `showAsPercentage`: ✅ Checked
- `decimalPlaces`: 0
- `textFormat`: "{0}%" (displays as "45%")

---

## 🔍 Verifying Setup

### **Image Component Requirements:**
The dial Image must be configured as:
- **Image Type**: Filled
- **Fill Method**: Radial 360
- **Fill Origin**: Top
- **Fill Clockwise**: ✅ Checked

**The script auto-configures this, but verify it worked.**

### **Testing in Play Mode:**
1. Enter Play Mode
2. Open SurvivalManager in Inspector
3. Adjust `currentInfection` slider (0-100)
4. Watch the dial fill and change colors:
   - **0-25**: Green → Yellow
   - **25-50**: Yellow → Orange
   - **50-75**: Orange → Red
   - **75-100**: Full Red

---

## 🎨 Customization Options

### **Change Fill Direction:**
In `InfectionDial.cs` → `SetupDialImage()`:
```csharp
dialFillImage.fillOrigin = (int)Image.Origin360.Bottom; // Start from bottom
dialFillImage.fillClockwise = false; // Counter-clockwise
```

### **Reverse Fill (Empty on High Infection):**
Swap `minFillAmount` and `maxFillAmount`:
- `maxFillAmount`: 0.0
- `minFillAmount`: 1.0

### **Custom Color Scheme:**
Adjust color gradient in Inspector:
- `healthyColor`: Your color for 0% infection
- `criticalColor`: Your color for 100% infection

---

## 🛠️ Troubleshooting

### **Dial Not Turning/Rotating:**
1. Check if you want **needle rotation** (like a speedometer) or **radial fill** (filling circle)
2. For needle rotation: Ensure `enableNeedleRotation = true` and assign `needleTransform`
3. For radial fill: Ensure `dialFillImage` is assigned and is type "Filled (Radial 360)"
4. The needle should be a child object named "Needle", "Pointer", "IMG_Dial_Indicator", or "Indicator"
5. Verify rotation angles are correct (e.g., -120 to 120 degrees)

### **Dial Not Filling:**
1. Check SurvivalManager exists and `enableInfectionSystem = true`
2. Verify `dialFillImage` is assigned
3. Check Image Type is "Filled (Radial 360)"
4. Enable `showDebugInfo` to see console logs

### **No Color Changes:**
1. Ensure `enableColorGradient = true`
2. Check threshold values are correct (25, 50, 75)
3. Verify colors are not all the same

### **Text Not Updating:**
1. Assign `infectionText` component
2. Enable `showInfectionValue = true`
3. Check text format is valid: "{0}%"

---

## 📊 Public API (Advanced Usage)

### **Manual Control:**
```csharp
InfectionDial dial = GetComponent<InfectionDial>();

// Set custom fill (0-1)
dial.SetFillAmount(0.5f); // 50% full

// Set custom color
dial.SetDialColor(Color.magenta);

// Force immediate update (no smooth transition)
dial.ForceUpdate();

// Get current fill percentage
float fillPercent = dial.GetFillPercentage(); // Returns 0-100

// Get infection status text
string status = dial.GetInfectionStatus(); // "None", "Mild", "Moderate", "Severe", "Critical"

// Check infection level
bool isCritical = dial.IsCritical(); // infection >= 75
bool isDanger = dial.IsDanger();     // infection >= 50
bool isWarning = dial.IsWarning();   // infection >= 25
```

---

## ✅ Component Status Check

**Script Features:**
- ✅ Auto-finds SurvivalManager
- ✅ Auto-finds Image component
- ✅ Auto-configures Radial Fill settings
- ✅ Smooth fill transitions
- ✅ Dynamic color gradients
- ✅ Text value display
- ✅ Percentage or raw value display
- ✅ Public API for manual control
- ✅ Debug logging option
- ✅ Full error validation

**Integration:**
- ✅ Reads `SurvivalManager.currentInfection`
- ✅ Reads `SurvivalManager.maxInfection`
- ✅ Automatically updates every frame
- ✅ Respects infection system enable/disable

---

## 🚀 You're Ready!

The **InfectionDial** script is **production-ready**. Just add it to Stat_Dial_00 and it will automatically track and display player infection from the SurvivalManager system.

**No additional coding required!**
