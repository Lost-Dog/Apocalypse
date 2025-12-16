# Challenge Player Agency System - Complete Guide

## Overview

Phase 2 implementation adds player agency features that give players control over when and how they engage with challenges, along with systems for discovery, preview, and retry mechanics.

---

## 🎯 Implemented Features

### 1. Challenge Discovery/Scouting ✅
- Challenges can be discovered before starting
- Manual start requirement (optional per challenge)
- Discovery notifications
- Proximity-based detection system
- Visual indicators for undiscovered vs discovered challenges

### 2. Risk/Reward Preview ✅
- Complete preview data structure
- Show all challenge details before committing
- Display scaled rewards based on player level
- Show active modifiers and bonuses
- Difficulty warnings for under-leveled players

### 3. Retry Mechanism ✅
- Retry failed challenges
- Configurable cooldown system
- Max attempt limits (optional)
- Track attempt count
- Separate retry state management

---

## 📦 New Components

### ChallengePreviewData
**File:** [ChallengeManager.cs](Assets/Scripts/ChallengeManager.cs)

Complete data structure for challenge preview UI:
```csharp
public class ChallengePreviewData
{
    // Basic info
    public string challengeName;
    public string description;
    public ChallengeData.ChallengeDifficulty difficulty;
    public int recommendedLevel;
    public int playerLevel;
    
    // Objectives
    public int enemyCount;
    public int civilianCount;
    public string objectiveDescription;
    public bool requireNoDeaths;
    public bool requireStealth;
    
    // Rewards (base and max with bonuses)
    public int baseXP;
    public int maxXP;
    public int baseCurrency;
    public int maxCurrency;
    public LootManager.Rarity lootRarity;
    public int lootCount;
    
    // Scaling info
    public float enemyHealthMultiplier;
    public float enemyDamageMultiplier;
    
    // State
    public int attemptCount;
    public ChallengeState state;
}
```

### ChallengeState Enum
**Location:** ActiveChallenge class

```csharp
public enum ChallengeState
{
    Discovered,    // Player found it but hasn't started
    Active,        // Currently in progress
    Completed,     // Successfully finished
    Failed,        // Failed (can potentially retry)
    Available      // Ready to start after retry
}
```

---

## 🔧 New ChallengeData Fields

### Player Agency Settings
```csharp
[Header("Player Agency")]

requireManualStart (bool)
  - If true, challenge requires player interaction to start
  - If false, auto-starts when spawned (old behavior)
  - Default: false

allowRetry (bool)
  - Enable/disable retry after failure
  - Default: true

retryCooldownSeconds (float)
  - Time before retry is available
  - 0 = immediate retry
  - Default: 60

maxAttempts (int)
  - Maximum retry attempts allowed
  - 0 = unlimited
  - Default: 0

showDiscoveryNotification (bool)
  - Show notification when discovered
  - Default: true
```

---

## 🎮 ActiveChallenge New Features

### State Management

```csharp
// Start a discovered challenge
challenge.StartChallenge()

// Mark as failed
challenge.FailChallenge()

// Check if can retry
bool canRetry = challenge.CanRetry()

// Retry a failed challenge
challenge.RetryChallenge()

// Get retry cooldown remaining
float timeLeft = challenge.GetRetryCooldownRemaining()
```

### Preview Data

```csharp
// Get complete preview for UI
ChallengePreviewData preview = challenge.GetPreviewData()
```

### Tracking Fields
```csharp
public ChallengeState state;
public bool hasBeenPreviewed;
public float discoveryTime;
public int attemptCount;
public float lastFailTime;
public float retryCooldown;
```

---

## 🎯 ChallengeManager New Methods

### Discovery & Preview
```csharp
// Get preview data for UI
ChallengePreviewData preview = challengeManager.GetChallengePreview(challenge)

// Notify player of discovery
challengeManager.NotifyChallengeDiscovered(challenge)
```

### Starting Challenges
```csharp
// Manually start a discovered challenge
bool success = challengeManager.StartDiscoveredChallenge(challenge)
```

### Retry System
```csharp
// Retry a failed challenge
bool success = challengeManager.RetryChallenge(challenge)
```

### Query Methods
```csharp
// Get challenges in specific states
List<ActiveChallenge> discovered = challengeManager.GetDiscoveredChallenges()
List<ActiveChallenge> active = challengeManager.GetActiveChallengesInProgress()
List<ActiveChallenge> failed = challengeManager.GetFailedChallenges()
List<ActiveChallenge> retryable = challengeManager.GetRetryableChallenges()
```

---

## 💻 UI Components

### ChallengePreviewUI
**File:** [ChallengePreviewUI.cs](Assets/Scripts/ChallengePreviewUI.cs)

Complete UI implementation for challenge preview system.

**Features:**
- Display all challenge details
- Show base vs bonus rewards
- Display modifiers and warnings
- Start/Retry buttons with proper state handling
- Cooldown timer display
- Difficulty color coding

**Usage:**
```csharp
// Attach to UI Panel GameObject
ChallengePreviewUI previewUI;

// Show preview
previewUI.ShowPreview(activeChallenge);

// Hide preview
previewUI.HidePreview();
```

**Required UI Elements:**
- Preview Panel (GameObject)
- Title Text (TextMeshProUGUI)
- Description Text (TextMeshProUGUI)
- Difficulty Text (TextMeshProUGUI)
- Time Limit Text (TextMeshProUGUI)
- Objectives Text (TextMeshProUGUI)
- Rewards Text (TextMeshProUGUI)
- Modifiers Text (TextMeshProUGUI)
- Warning Text (TextMeshProUGUI)
- Start Button (Button)
- Retry Button (Button)
- Close Button (Button)

### ChallengeDiscoverySystem
**File:** [ChallengeDiscoverySystem.cs](Assets/Scripts/ChallengeDiscoverySystem.cs)

Proximity-based discovery system for player interaction.

**Features:**
- Periodic scanning for nearby challenges
- Discovery notifications
- Interaction prompts
- Tracks discovered challenges
- Keyboard interaction (default: E key)

**Usage:**
```csharp
// Attach to player GameObject
[SerializeField] private ChallengePreviewUI previewUI;
[SerializeField] private GameObject interactPrompt;

// Configure in inspector
discoveryRange = 100f
interactKey = KeyCode.E
```

**How it Works:**
1. Scans for challenges within `discoveryRange` every `discoveryCheckInterval`
2. Shows interact prompt when near undiscovered/retryable challenge
3. Press `interactKey` to open preview UI
4. Notifies ChallengeManager on first discovery

---

## 📋 Setup Guide

### 1. Configure Challenge for Manual Start

```
ChallengeData Asset:
  Player Agency Section:
    ☑ Require Manual Start
    ☑ Allow Retry
    Retry Cooldown Seconds: 60
    Max Attempts: 3 (or 0 for unlimited)
    ☑ Show Discovery Notification
```

### 2. Setup Player Discovery

```csharp
// On player GameObject, add:
- ChallengeDiscoverySystem component
  - Assign ChallengePreviewUI reference
  - Assign Interact Prompt UI (optional)
  - Set Discovery Range (100-150 recommended)
  - Set Interact Key
```

### 3. Create Preview UI

```
Create UI Panel:
1. Add ChallengePreviewUI component
2. Create child text elements:
   - Title, Description, Difficulty, etc.
3. Create buttons:
   - Start Challenge
   - Retry Challenge
   - Close
4. Assign all references in inspector
```

---

## 🎮 Workflow Examples

### Example 1: Simple Manual Start

```csharp
// 1. Challenge spawns in Discovered state (requireManualStart = true)
// 2. Player enters discovery range
// 3. ChallengeDiscoverySystem detects it
// 4. Shows interact prompt "Press E to View Challenge"
// 5. Player presses E
// 6. ChallengePreviewUI.ShowPreview() displays full details
// 7. Player clicks "Start Challenge"
// 8. ChallengeManager.StartDiscoveredChallenge() begins challenge
// 9. Challenge content spawns, timer starts
```

### Example 2: Retry Flow

```csharp
// 1. Player fails challenge (time expired)
// 2. Challenge state → Failed
// 3. Cooldown timer starts (60 seconds)
// 4. Player opens preview
// 5. Retry button shows "Retry (47s)" (disabled)
// 6. After cooldown expires
// 7. Retry button shows "Retry Challenge" (enabled)
// 8. Player clicks Retry
// 9. Challenge state → Available
// 10. Player clicks Start to try again
// 11. Attempt counter increments
```

### Example 3: Preview Before Commit

```csharp
// Player discovers challenge
ChallengePreviewData preview = challenge.GetPreviewData();

// Check if worth attempting
if (preview.playerLevel < preview.recommendedLevel - 2)
{
    Debug.Log("Too hard! Come back later");
    return;
}

// Check rewards
Debug.Log($"Potential XP: {preview.baseXP} to {preview.maxXP}");

// Check modifiers
if (preview.hasModifiers)
{
    Debug.Log(preview.modifiersDescription);
}

// Player decides to start
challengeManager.StartDiscoveredChallenge(challenge);
```

---

## 🎨 UI Display Examples

### Preview Panel Layout
```
┌─────────────────────────────────────┐
│ [HARD] Eliminate the Warlord       │ <-- Title + Difficulty
├─────────────────────────────────────┤
│ A legendary warlord has appeared.  │ <-- Description
│ Only the strongest agents survive. │
├─────────────────────────────────────┤
│ Time Limit: 20:00                  │
│ Recommended Level: 10              │
│ Your Level: 8                      │
├─────────────────────────────────────┤
│ Objectives:                        │
│ • Eliminate 15 enemies             │
│ • Defeat the Warlord (Boss)       │
│ • No Deaths Allowed                │ <-- Red text
├─────────────────────────────────────┤
│ Rewards:                           │
│ XP: 1500 (up to 4500 with bonuses)│ <-- Green for bonuses
│ Currency: 750 (up to 1500)        │
│ Loot: 1× Legendary                │
│ • Perfect Completion Bonus        │ <-- Cyan
│ • Speed Completion Bonus          │
├─────────────────────────────────────┤
│ Active Modifiers:                 │
│ • Increased Enemy Health (1.5×)   │
│ • Iron Man: One Death = Failure   │
│ • Double XP Rewards               │
├─────────────────────────────────────┤
│ ⚠ WARNING:                        │ <-- Orange/Red
│ You are 2 levels below recommended│
│ Enemies have 225% health          │
│ Enemies deal 170% damage          │
├─────────────────────────────────────┤
│  [Start Challenge]    [Close]     │
└─────────────────────────────────────┘
```

### Retry Panel (After Failure)
```
┌─────────────────────────────────────┐
│ [HARD] Eliminate the Warlord       │
├─────────────────────────────────────┤
│ Attempt #2                         │ <-- Yellow text
│ [... same preview info ...]        │
├─────────────────────────────────────┤
│  [Retry (34s)]    [Close]         │ <-- Disabled with timer
└─────────────────────────────────────┘

(After cooldown expires)
┌─────────────────────────────────────┐
│  [Retry Challenge]    [Close]     │ <-- Enabled
└─────────────────────────────────────┘
```

---

## 🔍 Code Integration Examples

### Custom Discovery Trigger
```csharp
public class ChallengeInteractTrigger : MonoBehaviour
{
    public ActiveChallenge challenge;
    public ChallengePreviewUI previewUI;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Auto-show preview on enter
            if (previewUI != null && challenge != null)
            {
                previewUI.ShowPreview(challenge);
            }
        }
    }
}
```

### Quest Integration
```csharp
public class QuestManager : MonoBehaviour
{
    public void OfferChallengeQuest(ActiveChallenge challenge)
    {
        // Show quest dialog with preview
        ChallengePreviewData preview = challenge.GetPreviewData();
        
        string questText = $"Accept the challenge '{preview.challengeName}'?\n";
        questText += $"Difficulty: {preview.difficulty}\n";
        questText += $"Reward: {preview.maxXP} XP\n";
        
        // Show quest accept dialog
        ShowQuestDialog(questText, () => {
            ChallengeManager.Instance.StartDiscoveredChallenge(challenge);
        });
    }
}
```

### Map Marker System
```csharp
public class ChallengeMapMarker : MonoBehaviour
{
    public ActiveChallenge challenge;
    public GameObject discoveredIcon;
    public GameObject activeIcon;
    public GameObject failedIcon;
    
    private void Update()
    {
        if (challenge == null) return;
        
        // Update icon based on state
        discoveredIcon.SetActive(challenge.state == ActiveChallenge.ChallengeState.Discovered);
        activeIcon.SetActive(challenge.state == ActiveChallenge.ChallengeState.Active);
        failedIcon.SetActive(challenge.state == ActiveChallenge.ChallengeState.Failed);
    }
    
    public void OnMarkerClicked()
    {
        // Show preview when player clicks map marker
        FindObjectOfType<ChallengePreviewUI>().ShowPreview(challenge);
    }
}
```

---

## 📊 State Transition Diagram

```
[Spawn Challenge]
       ↓
  [Discovered] ←──────────────┐
       ↓                      │
  (Player Starts)             │
       ↓                      │
   [Active]                   │
       ↓                      │
   ┌───┴───┐                 │
   ↓       ↓                 │
[Complete] [Expired]          │
           ↓                 │
    (allowRetry?)            │
      ↓        ↓             │
    Yes       No             │
     ↓         ↓             │
  [Failed]  [Removed]        │
     ↓                       │
  (Cooldown)                 │
     ↓                       │
  [Available] ───────────────┘
     ↓
  (Player Retries)
     ↓
  [Active]
```

---

## ⚙️ Configuration Tips

### Easy Challenges
```
Require Manual Start: false (auto-start)
Allow Retry: true
Retry Cooldown: 30 seconds
Max Attempts: 0 (unlimited)
```

### Hard Challenges
```
Require Manual Start: true (let player prepare)
Allow Retry: true
Retry Cooldown: 120 seconds (2 minutes)
Max Attempts: 3
```

### Story/Boss Challenges
```
Require Manual Start: true (dramatic moment)
Allow Retry: true
Retry Cooldown: 300 seconds (5 minutes)
Max Attempts: 0 (unlimited, but long cooldown)
```

### Daily/Weekly Challenges
```
Require Manual Start: false (encourage attempts)
Allow Retry: false (one shot only)
```

---

## 🐛 Troubleshooting

**Preview not showing:**
- Check ChallengePreviewUI reference is assigned
- Verify challenge is in Discovered or Failed state
- Check preview panel GameObject is enabled

**Can't start challenge:**
- Verify challenge state is Discovered or Available
- Check player meets requiredPlayerLevel
- Ensure ChallengeManager.Instance is not null

**Retry button disabled:**
- Check retry cooldown hasn't expired yet
- Verify maxAttempts not exceeded
- Confirm allowRetry is true in ChallengeData

**Discovery not triggering:**
- Check discoveryRange is large enough
- Verify challenge has requireManualStart = true
- Ensure ChallengeDiscoverySystem is on player

---

## ✨ Future Enhancements

- [ ] Challenge bookmarking/favorites
- [ ] Preview history/log
- [ ] Social features (share challenges)
- [ ] Challenge voting/rating
- [ ] Dynamic difficulty adjustment based on performance
- [ ] Challenge playlists
- [ ] Leaderboards per challenge
- [ ] Achievement integration

---

## 📝 Summary

Phase 2 - Player Agency is **fully implemented** with:

✅ **Discovery System**
- Manual start option
- Proximity detection
- Discovery notifications
- State tracking

✅ **Preview System**
- Complete data structure
- Risk/reward display
- Modifier info
- Warning system
- Example UI implementation

✅ **Retry Mechanism**
- Configurable cooldowns
- Max attempt limits
- State management
- Retry eligibility checks

All systems are integrated and ready for use!
