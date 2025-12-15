# Mission Offer System - Setup Guide

## Overview

This system offers missions to the player sequentially after completing challenges. The player receives a UI notification when a mission is available, then must travel to the base to accept it.

## Features

- **Sequential Mission Offers**: Missions are offered starting from Mission01, Mission02, etc.
- **Level Requirements**: Existing mission level requirements are respected
- **UI Notification**: Player gets a notification when a mission is available
- **Base Acceptance**: Player must return to base to accept the mission
- **Distance Tracking**: Shows distance to base and enables accept button when in range

---

## Setup Instructions

### Step 1: Add MissionOfferManager to GameSystems

1. Select the `GameSystems` GameObject in the Hierarchy
2. Add Component → `MissionOfferManager`
3. Configure the settings:
   - **Base Location**: Drag your base/safe house Transform here
   - **Base Detection Radius**: 50 (default, adjust as needed)
   - **Next Mission Index**: 1 (starts with Mission01)

### Step 2: Create Base Interaction Point

1. Create or locate your base/safe house GameObject in the scene
2. Add Component → `BaseInteraction`
3. Add a **SphereCollider** component:
   - Set **Is Trigger**: True
   - Set **Radius**: 30 (or match your base detection radius)
4. Configure BaseInteraction:
   - **Base Name**: "Safe House" or your preferred name
   - **Interaction Radius**: 30
   - **Show Gizmos**: True (to visualize the zone)

### Step 3: Create Mission Offer UI

#### Option A: Manual UI Setup

1. In the Canvas, create a new Panel:
   - Name it `MissionOfferPanel`
   - Add background and styling

2. Add child UI elements:
   ```
   MissionOfferPanel
   ├── MissionNameText (TextMeshProUGUI)
   ├── DescriptionText (TextMeshProUGUI)
   ├── LevelRequirementText (TextMeshProUGUI)
   ├── RewardsText (TextMeshProUGUI)
   ├── DistanceText (TextMeshProUGUI)
   └── AcceptButton (Button)
       └── ButtonText (TextMeshProUGUI - "Accept Mission")
   ```

3. Add `MissionOfferUI` component to MissionOfferPanel
4. Drag UI elements to the component fields:
   - **Offer Panel**: MissionOfferPanel itself
   - **Mission Name Text**: MissionNameText
   - **Description Text**: DescriptionText
   - **Level Requirement Text**: LevelRequirementText
   - **Rewards Text**: RewardsText
   - **Accept Button**: AcceptButton
   - **Distance Text**: DistanceText

5. Configure settings:
   - **Auto Show On Offer**: True
   - **Show Distance Indicator**: True
   - **Update Interval**: 0.5

#### Option B: Quick UI Setup

The system will work without a dedicated UI panel. Notifications will be shown using the NotificationManager when missions are offered.

### Step 4: Connect to GameManager

1. Select the `GameSystems` GameObject
2. Find the `GameManager` component
3. If not already set, ensure:
   - **Mission Manager**: Drag the MissionManager component here
   - **Challenge Manager**: Drag the ChallengeManager component here

### Step 5: Create Mission Data Assets

Your missions should be named sequentially for the system to work:

1. Create mission assets in `/Assets/Resources/Missions/`
2. Name them:
   - Mission01 (or "Supply Run - Mission01")
   - Mission02 (or "Rescue Operation - Mission02")
   - Mission03, etc.

The system will search for missions containing "Mission01", "Mission02", etc.

**Example Mission Configuration:**

```
Mission Name: Mission01
Description: Investigate the supply depot and recover medical supplies.
Level Requirement: 1
XP Reward: 500
Currency Reward: 200
```

```
Mission Name: Mission02
Description: Rescue civilians trapped in the abandoned mall.
Level Requirement: 2
XP Reward: 750
Currency Reward: 300
```

### Step 6: Verify Event Connections

The MissionOfferManager automatically connects to ChallengeManager events in Start(). Verify:

1. Select `GameSystems/ChallengeManager`
2. Expand **Challenge Events → On Challenge Completed**
3. You should see `MissionOfferManager.OnChallengeCompleted` added at runtime

---

## How It Works

### Flow Diagram

```
Challenge Completed
    ↓
MissionOfferManager.OnChallengeCompleted()
    ↓
Find next mission (Mission01, Mission02, etc.)
    ↓
Check player level requirement
    ↓
If eligible → Offer mission
    ↓
Show UI notification
    ↓
Player travels to base
    ↓
Player enters base radius
    ↓
Accept button becomes enabled
    ↓
Player clicks Accept
    ↓
MissionManager.StartMission()
    ↓
Next mission index increments
```

### Mission Naming Convention

The system looks for missions with names containing:
- "Mission01" for the first mission
- "Mission02" for the second mission
- "Mission03", etc.

You can have descriptive names like:
- "Supply Run - Mission01"
- "Mission01 - First Contact"
- "Mission01"

All formats work as long as "Mission01" (with the two-digit format) is in the name.

---

## Testing

### Test 1: Complete a Challenge

1. Start Play mode
2. Complete any challenge
3. Check the console for: `Mission offered: [MissionName] - Return to base to accept`
4. Verify notification appears

### Test 2: Travel to Base

1. After a mission is offered, travel to your base
2. Watch the distance text update as you approach
3. When within radius, "Accept" button should become active
4. Check console: `At Base` message should appear

### Test 3: Accept Mission

1. Click the "Accept Mission" button (or call TryAcceptMission())
2. Verify mission starts
3. Check console: `Mission accepted: [MissionName]. Next mission index: 2`
4. Complete the mission
5. Next challenge completion should offer Mission02

### Test 4: Level Requirements

1. Set Mission02 level requirement to 5
2. Complete a challenge at level 1
3. System should skip Mission02 and log: `Mission Mission02 requires level 5, player is level 1`

---

## Integration Examples

### Show Mission Offer on HUD

```csharp
public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI missionOfferText;
    
    void Start()
    {
        if (MissionOfferManager.Instance != null)
        {
            MissionOfferManager.Instance.onMissionOffered.AddListener(OnMissionOffered);
        }
    }
    
    void OnMissionOffered(MissionData mission)
    {
        missionOfferText.text = $"New Mission: {mission.missionName}";
    }
}
```

### Custom Acceptance Trigger

```csharp
public class MissionBoard : MonoBehaviour
{
    void OnInteract()
    {
        if (MissionOfferManager.Instance != null)
        {
            bool success = MissionOfferManager.Instance.TryAcceptMission();
            
            if (success)
            {
                Debug.Log("Mission accepted via mission board!");
            }
        }
    }
}
```

### Check If Player Has Pending Mission

```csharp
if (MissionOfferManager.Instance != null && 
    MissionOfferManager.Instance.hasPendingOffer)
{
    string missionName = MissionOfferManager.Instance.GetOfferedMissionName();
    Debug.Log($"Pending mission: {missionName}");
}
```

---

## Customization Options

### Change Starting Mission Index

```csharp
// Start from Mission05 instead of Mission01
MissionOfferManager.Instance.SetNextMissionIndex(5);
```

### Offer Missions Without Challenge Completion

```csharp
// Manually trigger a mission offer
MissionOfferManager.Instance.OfferNextMission();
```

### Custom Base Detection

You can modify the base detection logic in `MissionOfferManager.Update()`:

```csharp
// Example: Check if player is inside a specific zone
if (IsPlayerInMissionZone() && hasPendingOffer)
{
    onPlayerAtBase?.Invoke();
}
```

---

## Troubleshooting

**Mission not offered after challenge completion:**
- Check that ChallengeManager.onChallengeCompleted event is firing
- Verify mission assets exist in Resources/Missions
- Check mission naming matches "Mission01" format
- Ensure player level meets requirement

**Accept button not working:**
- Verify Base Location is assigned in MissionOfferManager
- Check player has "Player" tag
- Ensure player is within baseDetectionRadius
- Check SphereCollider on base has Is Trigger = true

**Wrong mission offered:**
- Check nextMissionIndex in MissionOfferManager
- Verify mission naming convention
- Check that previous missions are in completedMissions list

**UI not updating:**
- Verify MissionOfferUI component has all references assigned
- Check Update Interval is > 0
- Ensure offerPanel is active in hierarchy

---

## Events Reference

### MissionOfferManager Events

- **onMissionOffered** - Fired when a mission is offered (parameter: MissionData)
- **onMissionAccepted** - Fired when player accepts a mission (parameter: MissionData)
- **onPlayerAtBase** - Fired each frame when player is at base with pending offer

### BaseInteraction Events

- **onPlayerEnterBase** - Fired when player enters base radius
- **onPlayerExitBase** - Fired when player exits base radius

---

## Best Practices

1. **Sequential Naming**: Always use two-digit format (Mission01, Mission02) for consistent ordering
2. **Level Gating**: Set appropriate level requirements to pace mission progression
3. **Base Placement**: Place bases in safe, easily accessible locations
4. **Visual Feedback**: Use UI and notifications to keep player informed
5. **Testing**: Test the full flow from challenge completion to mission acceptance
6. **Save System**: Consider saving nextMissionIndex and hasPendingOffer for persistence

---

## Future Enhancements

Possible additions to the system:

- **Multiple Bases**: Support different bases for different mission types
- **Mission Categories**: Separate main story missions from side missions
- **Mission Board UI**: Dedicated UI panel at base showing all available missions
- **Time Windows**: Missions available for limited time after being offered
- **Prerequisites**: Chain missions together with completion requirements
- **Notifications Stack**: Show multiple pending missions
