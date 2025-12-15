# Character Spawner/Pooler Setup Guide

## Overview

The Character Spawner system provides efficient character pooling with:
- ✅ **Object Pooling** - Reuses characters instead of instantiating/destroying
- ✅ **NavMesh Integration** - Spawns only on valid NavMesh positions
- ✅ **Distance-Based Management** - Auto-deactivates characters far from player
- ✅ **Configurable Parameters** - All settings exposed in Inspector
- ✅ **Auto-Spawning** - Maintains population around player
- ✅ **Editor Tools** - Runtime controls in Inspector

---

## Quick Setup (3 Minutes)

### 1. Create Character Spawner

1. **Create empty GameObject** in your scene: `CharacterSpawner`
2. **Add Component** → `CharacterSpawner` script
3. **Position it** anywhere (it manages spawning globally)

### 2. Assign Civilian Prefabs

**In Inspector:**
1. Find **Character Prefabs** section
2. Set **Size** to how many different civilian types you have
3. **Drag civilian prefabs** into the list:
   - `SM_Chr_Homeless_Male_01`
   - `SM_Chr_Teen_Male_01`
   - `SM_Chr_Wanderer_Male_01`
   - (Add as many as you want)

### 3. Configure Settings

**Spawn Settings:**
- Max Active Characters: `20` (how many civilians at once)
- Initial Pool Size: `30` (pre-created instances)
- Spawn Interval: `2` seconds (time between spawns)

**Distance Settings:**
- Min Spawn Distance: `30m` (don't spawn too close to player)
- Max Spawn Distance: `100m` (spawn within this range)
- Deactivate Distance: `120m` (return to pool when beyond this)

**NavMesh Settings:**
- Max Spawn Attempts: `10` (tries to find valid position)
- NavMesh Sample Distance: `5m` (search radius for NavMesh)

**Performance Settings:**
- Distance Check Interval: `1` second (how often to check distances)
- Enable Auto Spawn: `✓` (automatically spawn civilians)

### 4. Optional: Add Civilian Behavior

**On each civilian prefab:**
1. Add Component → `CivilianBehavior` script
2. Assign Animator (if you have one)
3. Configure:
   - Wander Radius: `20m`
   - Min Wait Time: `2s`
   - Max Wait Time: `5s`

---

## Exposed Parameters Reference

### Character Prefabs
```
civilianPrefabs (List<GameObject>)
```
List of civilian prefabs to randomly spawn from.

### Spawn Settings

**Max Active Characters** (`int`)
- Maximum number of civilians active at once
- Default: `20`
- Recommended: 15-30 for good performance

**Initial Pool Size** (`int`)
- How many instances to pre-create
- Default: `30`
- Should be >= Max Active Characters

**Spawn Interval** (`float`)
- Seconds between spawn attempts
- Default: `2.0`
- Lower = more frequent spawning

### Distance Settings

**Min Spawn Distance** (`float`)
- Minimum distance from player to spawn
- Default: `30`
- Prevents spawning in player's view

**Max Spawn Distance** (`float`)
- Maximum distance from player to spawn
- Default: `100`
- Creates "bubble" of civilians around player

**Deactivate Distance** (`float`)
- Distance at which civilians are deactivated
- Default: `120`
- Should be > Max Spawn Distance

### NavMesh Settings

**Max Spawn Attempts** (`int`)
- How many times to try finding valid position
- Default: `10`
- Higher = more likely to find spot, but slower

**NavMesh Sample Distance** (`float`)
- Search radius for NavMesh position
- Default: `5`
- Larger = more forgiving but less precise

### Performance Settings

**Distance Check Interval** (`float`)
- How often to check character distances (seconds)
- Default: `1.0`
- Lower = more responsive, but more expensive

**Enable Auto Spawn** (`bool`)
- Automatically spawn civilians
- Default: `true`
- Disable for manual control

### Debug Settings

**Show Debug Gizmos** (`bool`)
- Draw distance circles in Scene view
- Yellow = Min Spawn Distance
- Green = Max Spawn Distance
- Red = Deactivate Distance

**Log Spawn Events** (`bool`)
- Log spawning/despawning to Console
- Useful for debugging

---

## Runtime API

### Spawning

```csharp
// Spawn random civilian
GameObject civilian = CharacterSpawner.Instance.SpawnRandomCharacter();

// Spawn specific prefab at position
GameObject specific = CharacterSpawner.Instance.SpawnCharacter(prefab, position);

// Despawn a character
CharacterSpawner.Instance.DespawnCharacter(civilian);

// Despawn all characters
CharacterSpawner.Instance.DespawnAllCharacters();
```

### Settings (Runtime Adjustment)

```csharp
// Change max active characters
CharacterSpawner.Instance.SetMaxActiveCharacters(30);

// Change spawn interval
CharacterSpawner.Instance.SetSpawnInterval(1.5f);

// Change distances
CharacterSpawner.Instance.SetMinSpawnDistance(20f);
CharacterSpawner.Instance.SetMaxSpawnDistance(80f);
CharacterSpawner.Instance.SetDeactivateDistance(100f);

// Toggle auto-spawn
CharacterSpawner.Instance.SetAutoSpawnEnabled(false);
```

### Queries

```csharp
// Get counts
int active = CharacterSpawner.Instance.GetActiveCharacterCount();
int pooled = CharacterSpawner.Instance.GetPooledCharacterCount();
```

---

## How It Works

### 1. Initialization
- Creates object pools for each prefab
- Pre-instantiates `initialPoolSize` characters
- All start deactivated

### 2. Spawning
- Every `spawnInterval` seconds, attempts to spawn
- Finds random position between `minSpawnDistance` and `maxSpawnDistance`
- Validates position is on NavMesh
- Activates character from pool (or creates new if pool empty)

### 3. Distance Management
- Every `distanceCheckInterval` seconds, checks all active characters
- If character is beyond `deactivateDistance`, it's deactivated and returned to pool
- Prevents memory issues from characters getting too far away

### 4. Pooling
- Characters are never destroyed, only deactivated
- Much faster than Instantiate/Destroy
- Automatically expands pool if needed

---

## Integration with Challenge System

```csharp
// When civilian is rescued
public void OnCivilianRescued(GameObject civilian)
{
    // Update challenge progress
    ChallengeManager.Instance.UpdateCivilianRescued();
    
    // Despawn civilian
    CharacterSpawner.Instance.DespawnCharacter(civilian);
}

// Spawn civilians for a challenge
public void SpawnCiviliansForChallenge(Vector3 location, int count)
{
    for (int i = 0; i < count; i++)
    {
        // You can modify to spawn near specific location
        CharacterSpawner.Instance.SpawnRandomCharacter();
    }
}
```

---

## Performance Tips

### Optimization Settings

**For Good Performance (60 FPS):**
- Max Active Characters: `15-20`
- Distance Check Interval: `1.0s`
- Deactivate Distance: `100-120m`

**For High Performance (30+ civilians):**
- Max Active Characters: `30-50`
- Distance Check Interval: `1.5s`
- Deactivate Distance: `150m`
- Use LOD on character models

**For Mobile:**
- Max Active Characters: `10-15`
- Distance Check Interval: `2.0s`
- Deactivate Distance: `80m`

### Best Practices

1. **Pool Size** should be 1.5x Max Active Characters
2. **Deactivate Distance** should be > Max Spawn Distance
3. **Min Spawn Distance** should be > player's view distance
4. Lower **Distance Check Interval** for more responsive culling

---

## Civilian Behavior Script

Optional script for wandering AI behavior:

### Features
- Wanders randomly around spawn point
- Pauses occasionally
- Updates animator walk speed
- Auto-resets when spawned/despawned

### Settings

**Movement:**
- Wander Radius: `20m` (how far from spawn point)
- Min/Max Wait Time: `2-5s` (idle time)

**Animation:**
- Animator reference
- Walk Speed Parameter name (default: "WalkSpeed")

### API

```csharp
CivilianBehavior behavior = civilian.GetComponent<CivilianBehavior>();

// Change wander area
behavior.SetWanderRadius(30f);

// Freeze/unfreeze
behavior.FreezeMovement();
behavior.ResumeMovement();
```

---

## Editor Tools

**In Play Mode, the Inspector shows:**
- Active character count
- Pooled character count
- **Spawn Random** button - Manually spawn a civilian
- **Despawn All** button - Clear all active civilians

---

## Troubleshooting

**Characters don't spawn:**
- Check civilian prefabs are assigned
- Check player has "Player" tag
- Check NavMesh is baked in scene
- Enable "Log Spawn Events" to see errors

**Characters spawn in wrong places:**
- Check NavMesh is baked correctly
- Increase NavMesh Sample Distance
- Check Min/Max Spawn Distance values

**Characters don't despawn:**
- Check Deactivate Distance > Max Spawn Distance
- Lower Distance Check Interval for faster response
- Enable debug gizmos to visualize distances

**Performance issues:**
- Reduce Max Active Characters
- Increase Distance Check Interval
- Reduce Spawn Interval
- Use simpler character models/LODs

**Pool runs out:**
- Increase Initial Pool Size
- System auto-creates more, but wastes time
- Pool size should be >= max characters that can be active

---

## Example Configuration

### Urban Environment (Dense)
```
Max Active Characters: 30
Min Spawn Distance: 20m
Max Spawn Distance: 80m
Deactivate Distance: 100m
Spawn Interval: 1.5s
```

### Rural Environment (Sparse)
```
Max Active Characters: 15
Min Spawn Distance: 40m
Max Spawn Distance: 120m
Deactivate Distance: 150m
Spawn Interval: 3s
```

### Combat Zone (Few civilians)
```
Max Active Characters: 10
Min Spawn Distance: 30m
Max Spawn Distance: 60m
Deactivate Distance: 80m
Spawn Interval: 5s
```

---

## Advanced: Custom Spawn Positions

If you need more control over spawn locations:

```csharp
// Spawn near specific point
public GameObject SpawnNearLocation(Vector3 location, float radius)
{
    Vector3 randomOffset = Random.insideUnitSphere * radius;
    randomOffset.y = 0;
    
    NavMeshHit hit;
    if (NavMesh.SamplePosition(location + randomOffset, out hit, 10f, NavMesh.AllAreas))
    {
        GameObject prefab = civilianPrefabs[Random.Range(0, civilianPrefabs.Count)];
        return CharacterSpawner.Instance.SpawnCharacter(prefab, hit.position);
    }
    
    return null;
}
```

---

## Summary

✅ **Efficient Pooling** - No instantiate/destroy overhead
✅ **NavMesh Integration** - Only spawns on walkable surfaces
✅ **Auto-Management** - Maintains population around player
✅ **Highly Configurable** - All parameters exposed
✅ **Performance-Friendly** - Distance-based culling
✅ **Easy Integration** - Works with existing systems

Perfect for populating your Division-style open world!
