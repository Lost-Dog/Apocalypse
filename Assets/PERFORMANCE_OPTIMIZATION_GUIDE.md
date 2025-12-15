# Performance Optimization Guide - Apocalypse Game

## 🎯 Critical Performance Issues Found

Based on the code analysis, here are the main performance bottlenecks that could cause crashes:

---

## 1. **CompassEnemyTracker - High GC Allocation** ⚠️ CRITICAL

### Issues:
- `GameObject.FindGameObjectWithTag()` creates GC allocations every frame
- `Physics.OverlapSphere()` allocates new arrays every refresh
- `GetComponent<>()` calls in tight loops
- Dictionary allocations when checking enemies

### Impact:
- Garbage collection spikes causing frame drops
- Memory pressure leading to crashes in later waves
- CPU overhead from repeated component lookups

### Solution Applied:
See optimized version below.

---

## 2. **ZombieExplosive - Per-Frame Distance Checks** ⚠️ MEDIUM

### Issues:
- `Vector3.Distance()` calculated every frame for all explosive zombies
- `GameObject.FindGameObjectWithTag("Player")` in Start (acceptable)
- `GetComponent<>()` in explosion loop

### Impact:
- With 20+ explosive zombies, this becomes expensive
- Square root calculations every frame

### Solution:
Use squared distance comparison (cheaper than Distance).

---

## 3. **ZombieHordeBehavior - Component Cache Missing** ⚠️ MEDIUM

### Issues:
- `GetComponent<JU_AI_Zombie>()` called for every zombie in range
- Physics overlap allocates collider array

### Impact:
- Repeated GetComponent calls are expensive
- GC pressure from array allocations

### Solution:
Cache components and use NonAlloc version (already using it ✓).

---

## 4. **Wave Scaling - Health Multiplier Compounding** ⚠️ LOW

### Issues:
- Health multipliers applied on top of variant multipliers
- Later waves (10+) can have extremely tanky enemies

### Impact:
- Longer combat = more active enemies = performance issues
- Memory usage grows with active character count

### Solution:
Monitor and cap health scaling appropriately (already has maxHealthMultiplier ✓).

---

## 5. **Object Pool - Potential Memory Growth** ⚠️ LOW

### Issues:
- Pool can expand up to `maxPoolSizePerPrefab` (20)
- Multiple prefabs × 20 instances = high memory

### Impact:
- Memory usage can grow significantly
- Not returning objects properly could exhaust pool

### Solution:
Already has limits, but monitor active objects.

---

## 📊 Recommended Settings for Stability

### SurvivalMission Settings:
```
Total Waves: 10 (don't go higher without testing)
Absolute Max Enemies: 20-25 (not 30)
Health Increase Per Wave: 0.15 (reduced from 0.2)
Max Health Multiplier: 3.5 (reduced from 5)
```

### CharacterSpawnerNavMesh Settings:
```
Max Active Characters: 15-20 (adjust based on hardware)
Spawn Interval: 3-4 seconds minimum
```

### CompassEnemyTracker Settings:
```
Refresh Rate: 0.25-0.5 (not 0.1)
Max Detection Range: 50-75 (not 100)
```

### ObjectPool Settings:
```
Pool Size Per Prefab: 5
Max Pool Size Per Prefab: 15 (reduce from 20)
Expand Pool: true
```

---

## 🔧 Immediate Optimizations to Apply

I'll create optimized versions of the critical scripts below.

---

## 💡 Additional Recommendations

### 1. **Use Unity Profiler**
- Profile CPU usage (Ctrl+7)
- Check for GC.Alloc spikes
- Monitor active GameObject count
- Track NavMesh agent count

### 2. **Graphics Optimizations**
- Reduce shadow distance
- Lower texture quality for zombies
- Use LOD (Level of Detail) groups
- Disable unnecessary cameras/lights

### 3. **Physics Optimizations**
- Reduce Fixed Timestep (0.02 → 0.03)
- Use simpler colliders
- Disable ragdolls after 5 seconds
- Limit active rigidbodies

### 4. **Audio Optimizations**
- Use 2D sounds where possible
- Limit max audio sources
- Pool audio sources
- Use lower quality audio for distant sounds

### 5. **AI Optimizations**
- Reduce NavMesh agent update frequency
- Use simpler pathfinding for distant zombies
- Disable AI for off-screen enemies
- Stagger AI updates across frames

### 6. **UI Optimizations**
- Use Canvas groups to disable unused UI
- Avoid Layout Groups with many elements
- Cache RectTransform references
- Use object pooling for UI icons (compass)

---

## 🚨 Crash Prevention Checklist

- [ ] Limit max active enemies to 20
- [ ] Enable VSync or frame rate limit (60 FPS cap)
- [ ] Optimize compass refresh rate (0.25s+)
- [ ] Monitor memory usage in Profiler
- [ ] Test wave 5+ for stability
- [ ] Reduce health multiplier cap
- [ ] Add enemy despawn on max distance
- [ ] Clean up dead ragdolls after 10s
- [ ] Reduce shadow casting zombies
- [ ] Disable expensive post-processing

---

## 📈 Performance Monitoring

### Key Metrics to Watch:
1. **Frame Time**: Should stay under 16ms (60 FPS)
2. **GC Allocations**: Should be minimal (< 1KB per frame)
3. **Active GameObjects**: Keep under 200
4. **Draw Calls**: Keep under 500-1000
5. **Memory Usage**: Monitor for steady growth

### Warning Signs:
- ⚠️ Frame time increasing each wave
- ⚠️ GC.Alloc spikes every second
- ⚠️ Memory usage climbing steadily
- ⚠️ Audio crackling/stuttering
- ⚠️ Input lag or delayed responses

---

## 🎮 Testing Protocol

1. **Stress Test**: Run to wave 10 without dying
2. **Memory Test**: Monitor memory growth over 30 minutes
3. **Profile Test**: Run Unity Profiler during wave 5-10
4. **Platform Test**: Test on target hardware (not just editor)
5. **Build Test**: Test in actual build, not editor

---

**Next Steps**: I'll create optimized scripts for the critical issues.
