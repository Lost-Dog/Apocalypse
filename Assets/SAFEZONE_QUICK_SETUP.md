# Safe Zone - Quick Setup (2 Minutes!)

## 🚀 Fastest Setup

### **Step 1: Create Safe Zone (30 seconds)**

```
Hierarchy → Right-click → Create Empty
Name: "SafeZone"

Add Components:
1. Box Collider
   └── Is Trigger: ☑ true
   └── Size: (10, 5, 10)

2. SafeZone script
   └── Default settings are good!
```

### **Step 2: Verify Player Tag (10 seconds)**

```
Select Player in Hierarchy
Inspector → Tag: "Player" ✅
```

### **Step 3: Test! (1 minute)**

```
1. Play Mode
2. Walk into SafeZone
3. Watch health restore! ✅
```

**Done! You have a working safe zone!** 🎉

---

## ⚙️ Essential Settings

```yaml
SafeZone Component:
├── Safe Zone Name: "Your Name"
├── Health Restore Rate: 10    # HP/second
├── Stamina Restore Rate: 20   # Stamina/second
└── Restore Delay: 1           # Seconds
```

---

## 🎨 Add Visual (Optional - 1 minute)

```
SafeZone → Right-click → 3D Object → Cylinder
Name: "ZoneVisual"
Scale: (10, 0.1, 10)

Create Material → Green, Transparent
Drag onto cylinder
```

---

## 💬 Add UI Message (Optional - 2 minutes)

**1. Create UI:**
```
Hierarchy → UI → Canvas (if needed)
Canvas → Right-click → UI → Panel
Name: "MessageDisplay"

Settings:
├── Position: Top center
├── Size: (600, 80)
└── Color: Black, semi-transparent

MessageDisplay → UI → Text - TextMeshPro
Name: "MessageText"
└── Alignment: Center
```

**2. Add Script:**
```
MessageDisplay → Add Component → MessageDisplay
└── Auto Setup: ☑ true
```

**3. Rename:**
```
Panel must be named exactly "MessageDisplay"
```

**Done!**

---

## 🎯 Common Presets

### **Fast Healing Station:**
```yaml
Health Restore Rate: 30
Restore Delay: 0
```

### **Rest Area (Must Stand Still):**
```yaml
Health Restore Rate: 5
Require Idle: ☑ true
Restore Delay: 2
```

### **Medical Bay (Health + Cure):**
```yaml
Health Restore Rate: 25
Cure Infection: ☑ true
Infection Cure Rate: 15
```

---

## ✅ Testing Checklist

- [ ] Player has "Player" tag
- [ ] Collider is trigger
- [ ] Walk into zone
- [ ] Health increases
- [ ] Message appears (if UI setup)

---

## 🐛 Quick Fixes

**Not healing?**
- ✅ Check Player tag
- ✅ Check Collider is trigger
- ✅ Check Restore Health enabled

**No message?**
- ✅ Create "MessageDisplay" (exact name!)
- ✅ Add MessageDisplay script

---

**Your safe zone is ready! 🛡️💚**
