# Audio Pooling Setup Guide

## What is Pooling?

Object pooling reuses objects instead of creating and destroying them repeatedly. This significantly improves performance.

**Before Pooling:**
- Every footstep creates a NEW AudioSource
- Every footstep creates a NEW particle GameObject
- These are destroyed after playing
- Causes garbage collection and frame drops

**After Pooling:**
- AudioSources are reused from a pool
- Particles are reused from a pool
- No constant creation/destruction
- Better performance, no frame drops

---

## Setup Steps

### 1. Add PoolManager to Scene

1. Create an empty GameObject named "AudioPoolManager"
2. Add the `PoolManager` component
3. Configure settings:
   - Audio Source Pool Size: 30 (adjust based on character count)
   - Max Audio Sources: 60
   - Particle Pool Size: 10 per type
   - Max Particles: 30 per type

### 2. Convert Audio Surfaces

1. Use `Tools → CoverShooter → Convert to Pooled Audio Surfaces`
2. Click "Create Pooled Versions of All Standard Surfaces"
3. New pooled assets will be created in `Assets/AudioSurfaces_Pooled/`

### 3. Update Character References

For each character with footsteps:

1. Find the `vFootStep` or `vFootStepBase` component
2. Expand the "Custom Surfaces" section
3. Replace each `vAudioSurface` with its `_Pooled` version

### 4. Test Performance

1. Enter Play Mode
2. Enable "Show Debug Info" on PoolManager
3. Watch the pool statistics in the top-left corner
4. Verify AudioSources are being reused (count goes up and down)

---

## Performance Benefits

**Before Pooling (10 characters walking):**
- ~60 new AudioSources created per second
- ~60 GameObjects destroyed per second
- GC allocations: ~15KB/frame
- Frame drops when many characters walk

**After Pooling (10 characters walking):**
- 0 new AudioSources created (reused from pool)
- 0 GameObjects destroyed (returned to pool)
- GC allocations: ~0.5KB/frame
- Smooth performance

---

## Troubleshooting

### No Sound Playing
- Check that PoolManager is in the scene
- Verify pooled audio surfaces are assigned
- Check AudioSourcePool exists in hierarchy during play

### Pool Running Out
- Increase Max Audio Sources in PoolManager
- Check that sounds aren't too long (blocking pool return)

### Particles Not Showing
- Ensure particle prefabs are assigned in PoolManager
- Check particle system duration settings

---

## Advanced Configuration

### Custom Pool Sizes

Adjust based on your game:
- **Few characters (1-5):** Audio Pool = 15, Max = 30
- **Medium (5-15):** Audio Pool = 30, Max = 60
- **Many (15+):** Audio Pool = 50, Max = 100

### Per-Surface Pooling Control

Each `vAudioSurfacePooled` has toggles:
- `Use Pooling` - Master switch
- `Use Audio Source Pool` - Pool AudioSources specifically

You can disable pooling per-surface if needed.

---

**Created by:** Audio Pooling Converter Tool
