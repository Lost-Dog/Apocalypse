using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single gatekeeper for all live game events — missions, world-event challenges,
/// and weekly challenges. Enforces the rule that only one event may be active at a
/// time and inserts a configurable quiet period between events.
///
/// Attach to the same persistent GameObject as GameManager (or any persistent object).
/// Set ChallengeManager.autoSpawnChallenges = false in the Inspector; this scheduler
/// drives world-event spawning instead.
/// </summary>
public class EventScheduler : MonoBehaviour
{
    // ── Event types tracked by the scheduler ─────────────────────────────────

    private enum ScheduledEventType { WorldEvent, MissionOffer, WeeklyChallenge }

    private class ScheduledEvent
    {
        public ScheduledEventType type;
        public ActiveChallenge weeklyChallenge; // only set for WeeklyChallenge type
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Quiet Period")]
    [Tooltip("Minimum seconds of no active event before the next one may launch.")]
    public float quietPeriodSeconds = 300f; // 5 minutes

    [Header("World Event Scheduling")]
    [Tooltip("How often a world event is checked / enqueued while none is active.")]
    public float worldEventCheckInterval = 60f;

    [Header("Mission Offer Scheduling")]
    [Tooltip("How long after a mission completes (or quiet period ends) before a new mission is offered.")]
    public float missionOfferDelaySeconds = 30f;

    [Header("Weekly Challenge Scheduling")]
    [Tooltip("How long after session start before the first weekly challenge is offered.")]
    public float firstWeeklyChallengeDelaySeconds = 600f; // 10 min into session

    [Header("Debug")]
    public bool logEvents = false;

    // ── State ─────────────────────────────────────────────────────────────────

    private MissionManager missionManager;
    private ChallengeManager challengeManager;
    private MissionOfferManager missionOfferManager;

    private readonly Queue<ScheduledEvent> eventQueue = new Queue<ScheduledEvent>();

    /// <summary>True while any event is considered live (mission active or challenge in progress).</summary>
    private bool isEventLive = false;

    /// <summary>Timestamp when the last live event ended.</summary>
    private float lastEventEndTime = float.NegativeInfinity;

    private float worldEventCheckTimer;
    private bool weeklyScheduled = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        missionManager = GameManager.Instance?.missionManager;
        challengeManager = GameManager.Instance?.challengeManager;
        missionOfferManager = MissionOfferManager.Instance;

        if (challengeManager != null)
        {
            // Hand off spawning control to this scheduler.
            challengeManager.autoSpawnChallenges = false;

            // Subscribe to challenge lifecycle events.
            challengeManager.onChallengeStarted.AddListener(OnChallengeStarted);
            challengeManager.onChallengeCompleted.AddListener(OnChallengeEnded);
            challengeManager.onChallengeExpired.AddListener(OnChallengeEnded);
            challengeManager.onChallengeFailed.AddListener(OnChallengeEnded);
        }

        if (missionManager != null)
        {
            missionManager.onMissionStart.AddListener(OnMissionStarted);
            missionManager.onMissionComplete.AddListener(OnMissionEnded);
            missionManager.onMissionFail.AddListener(OnMissionEnded);
        }

        if (missionOfferManager != null)
        {
            // Disable the offer manager's own timed offer loop — we drive it.
            missionOfferManager.debugRandomOffers = false;
        }

        // Queue the first weekly challenge after the configured delay.
        StartCoroutine(ScheduleFirstWeeklyChallenge());

        worldEventCheckTimer = worldEventCheckInterval;

        if (logEvents)
            Debug.Log("[EventScheduler] Initialized. Quiet period: " +
                      $"{quietPeriodSeconds}s. World event check: {worldEventCheckInterval}s.", this);
    }

    private void OnDestroy()
    {
        if (challengeManager != null)
        {
            challengeManager.onChallengeStarted.RemoveListener(OnChallengeStarted);
            challengeManager.onChallengeCompleted.RemoveListener(OnChallengeEnded);
            challengeManager.onChallengeExpired.RemoveListener(OnChallengeEnded);
            challengeManager.onChallengeFailed.RemoveListener(OnChallengeEnded);
        }

        if (missionManager != null)
        {
            missionManager.onMissionStart.RemoveListener(OnMissionStarted);
            missionManager.onMissionComplete.RemoveListener(OnMissionEnded);
            missionManager.onMissionFail.RemoveListener(OnMissionEnded);
        }
    }

    private void Update()
    {
        // Refresh live state in case a mission or challenge ended outside our callbacks.
        RefreshLiveState();

        // Periodically enqueue a world event if the queue has room.
        if (!isEventLive)
        {
            worldEventCheckTimer -= Time.deltaTime;
            if (worldEventCheckTimer <= 0f)
            {
                worldEventCheckTimer = worldEventCheckInterval;
                TryEnqueueWorldEvent();
            }
        }

        // Attempt to launch the next queued event.
        TryLaunchNext();
    }

    // ── Live State ────────────────────────────────────────────────────────────

    private void RefreshLiveState()
    {
        bool missionLive = missionManager != null && missionManager.activeMission != null;
        bool challengeLive = challengeManager != null &&
                             challengeManager.GetActiveChallengesInProgress().Count > 0;

        isEventLive = missionLive || challengeLive;
    }

    // ── Event Lifecycle Callbacks ─────────────────────────────────────────────

    private void OnChallengeStarted(ActiveChallenge challenge)
    {
        isEventLive = true;
        if (logEvents)
            Debug.Log($"[EventScheduler] Challenge started: {challenge.challengeData.challengeName}", this);
    }

    private void OnChallengeEnded(ActiveChallenge challenge)
    {
        isEventLive = false;
        lastEventEndTime = Time.time;

        if (logEvents)
            Debug.Log($"[EventScheduler] Challenge ended: {challenge.challengeData.challengeName}. " +
                      $"Quiet period: {quietPeriodSeconds}s.", this);
    }

    private void OnMissionStarted(MissionData mission)
    {
        isEventLive = true;
        if (logEvents)
            Debug.Log($"[EventScheduler] Mission started: {mission.missionName}", this);
    }

    private void OnMissionEnded(MissionData mission)
    {
        isEventLive = false;
        lastEventEndTime = Time.time;

        if (logEvents)
            Debug.Log($"[EventScheduler] Mission ended: {mission.missionName}. " +
                      $"Quiet period: {quietPeriodSeconds}s.", this);

        // Queue a mission offer after a short delay following mission completion.
        StartCoroutine(EnqueueMissionOfferAfterDelay(missionOfferDelaySeconds));
    }

    // ── Queue Management ──────────────────────────────────────────────────────

    private void TryEnqueueWorldEvent()
    {
        if (challengeManager == null || challengeManager.worldEventChallenges.Count == 0) return;

        // Don't stack up world events in the queue — one pending is enough.
        bool alreadyQueued = false;
        foreach (ScheduledEvent e in eventQueue)
        {
            if (e.type == ScheduledEventType.WorldEvent)
            {
                alreadyQueued = true;
                break;
            }
        }

        if (!alreadyQueued)
        {
            eventQueue.Enqueue(new ScheduledEvent { type = ScheduledEventType.WorldEvent });
            if (logEvents)
                Debug.Log("[EventScheduler] World event enqueued.", this);
        }
    }

    private IEnumerator EnqueueMissionOfferAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (missionOfferManager == null || missionOfferManager.hasPendingOffer) yield break;

        eventQueue.Enqueue(new ScheduledEvent { type = ScheduledEventType.MissionOffer });

        if (logEvents)
            Debug.Log("[EventScheduler] Mission offer enqueued.", this);
    }

    private IEnumerator ScheduleFirstWeeklyChallenge()
    {
        yield return new WaitForSeconds(firstWeeklyChallengeDelaySeconds);
        EnqueueNextWeeklyChallenge();
    }

    private void EnqueueNextWeeklyChallenge()
    {
        if (weeklyScheduled || challengeManager == null) return;
        if (challengeManager.weeklyChallenges.Count == 0) return;

        // Pick the first weekly challenge that hasn't been completed yet.
        ActiveChallenge next = null;
        foreach (ActiveChallenge wc in challengeManager.weeklyChallenges)
        {
            if (wc.state == ActiveChallenge.ChallengeState.Discovered ||
                wc.state == ActiveChallenge.ChallengeState.Available)
            {
                next = wc;
                break;
            }
        }

        if (next == null) return;

        eventQueue.Enqueue(new ScheduledEvent
        {
            type = ScheduledEventType.WeeklyChallenge,
            weeklyChallenge = next
        });

        weeklyScheduled = true;

        if (logEvents)
            Debug.Log($"[EventScheduler] Weekly challenge enqueued: {next.challengeData.challengeName}", this);
    }

    // ── Launch ────────────────────────────────────────────────────────────────

    private void TryLaunchNext()
    {
        if (isEventLive) return;
        if (eventQueue.Count == 0) return;
        if (Time.time - lastEventEndTime < quietPeriodSeconds) return;

        ScheduledEvent next = eventQueue.Dequeue();
        LaunchEvent(next);
    }

    private void LaunchEvent(ScheduledEvent scheduledEvent)
    {
        switch (scheduledEvent.type)
        {
            case ScheduledEventType.WorldEvent:
                LaunchWorldEvent();
                break;

            case ScheduledEventType.MissionOffer:
                LaunchMissionOffer();
                break;

            case ScheduledEventType.WeeklyChallenge:
                LaunchWeeklyChallenge(scheduledEvent.weeklyChallenge);
                break;
        }
    }

    private void LaunchWorldEvent()
    {
        if (challengeManager == null) return;

        challengeManager.SpawnRandomWorldEvent();

        if (logEvents)
            Debug.Log("[EventScheduler] World event launched.", this);
    }

    private void LaunchMissionOffer()
    {
        if (missionOfferManager == null) return;
        if (missionOfferManager.hasPendingOffer) return;

        missionOfferManager.OfferNextMission();

        if (logEvents)
            Debug.Log("[EventScheduler] Mission offered.", this);
    }

    private void LaunchWeeklyChallenge(ActiveChallenge challenge)
    {
        if (challengeManager == null || challenge == null) return;

        bool started = challengeManager.StartDiscoveredChallenge(challenge);

        if (started)
        {
            isEventLive = true;
            weeklyScheduled = false; // Allow the next weekly to be queued later.

            if (logEvents)
                Debug.Log($"[EventScheduler] Weekly challenge launched: {challenge.challengeData.challengeName}", this);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if any event is currently live or the quiet period has not elapsed yet.
    /// </summary>
    public bool IsQuietPeriod()
    {
        if (isEventLive) return false;
        return Time.time - lastEventEndTime < quietPeriodSeconds;
    }

    /// <summary>
    /// Seconds remaining in the current quiet period. 0 if no quiet period is active.
    /// </summary>
    public float QuietPeriodRemaining()
    {
        if (isEventLive) return 0f;
        return Mathf.Max(0f, quietPeriodSeconds - (Time.time - lastEventEndTime));
    }

    /// <summary>
    /// How many events are waiting in the queue.
    /// </summary>
    public int QueuedEventCount => eventQueue.Count;
}
