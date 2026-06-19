using System;
using System.Collections.Generic;
using System.Text;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Quests;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Bridges custom mission/challenge lifecycle events to Game Creator 2 Journal quests.
/// Assign this on a scene object (typically GameSystems/GameManager), then map each
/// MissionData/ChallengeData to its corresponding GC2 Quest asset in the inspector.
/// </summary>
public class GC2MissionChallengeJournalBridge : MonoBehaviour
{
    [Serializable]
    public class MissionQuestMapping
    {
        public MissionData mission;
        public Quest quest;
        public bool trackOnActivate = true;
        public bool completeOnMissionComplete = true;
        public bool failOnMissionFail = true;
    }

    [Serializable]
    public class ChallengeQuestMapping
    {
        public ChallengeData challenge;
        public Quest quest;
        public bool trackOnActivate = true;
        public bool completeOnChallengeComplete = true;
        public bool failOnChallengeFailOrExpire = true;
    }

    [Header("References")]
    [SerializeField] private Journal journal;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private ChallengeManager challengeManager;

    [Header("Mission -> Quest Mappings")]
    [SerializeField] private List<MissionQuestMapping> missionMappings = new List<MissionQuestMapping>();

    [Header("Challenge -> Quest Mappings")]
    [SerializeField] private List<ChallengeQuestMapping> challengeMappings = new List<ChallengeQuestMapping>();

    [Header("Behavior")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private bool autoFixNavigationUiRoots = true;

    [Header("Auto Mapping")]
    [SerializeField] private bool overwriteExistingQuestAssignments;

    private readonly Dictionary<MissionData, MissionQuestMapping> missionMap = new Dictionary<MissionData, MissionQuestMapping>();
    private readonly Dictionary<ChallengeData, ChallengeQuestMapping> challengeMap = new Dictionary<ChallengeData, ChallengeQuestMapping>();

    private void Awake()
    {
        if (autoFindReferences)
        {
            ResolveReferences();
        }

        BuildMaps();

        if (autoFixNavigationUiRoots)
        {
            FixNavigationUiRoots();
        }
    }

    [ContextMenu("Fix GC2 Navigation UI Roots")]
    public void FixNavigationUiRoots()
    {
        string[] targetNames =
        {
            "Navigation_KronnectMap",
            "Navigation_Compass",
            "Navigation_Indicators"
        };

        int fixedCount = 0;
        int renamedCount = 0;
        Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null) continue;

            if (t.name.StartsWith("Navigation_", StringComparison.Ordinal)
                && t.name.IndexOf("map", StringComparison.OrdinalIgnoreCase) >= 0
                && !string.Equals(t.name, "Navigation_KronnectMap", StringComparison.Ordinal))
            {
                t.name = "Navigation_KronnectMap";
                renamedCount++;
            }

            bool isTarget = false;
            for (int n = 0; n < targetNames.Length; n++)
            {
                if (string.Equals(t.name, targetNames[n], StringComparison.Ordinal))
                {
                    isTarget = true;
                    break;
                }
            }

            if (!isTarget) continue;

            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
            }

            if (t.localScale == Vector3.zero)
            {
                t.localScale = Vector3.one;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Log($"Fixed {fixedCount} GC2 navigation UI root(s) with zero scale.");
        }

        if (renamedCount > 0)
        {
            Log($"Renamed {renamedCount} legacy navigation root(s) to Kronnect naming.");
        }
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    [ContextMenu("Auto Find References")]
    public void ResolveReferences()
    {
        if (journal == null)
        {
            journal = FindObjectOfType<Journal>();
        }

        if (missionManager == null)
        {
            missionManager = GameManager.Instance != null
                ? GameManager.Instance.missionManager
                : FindObjectOfType<MissionManager>();
        }

        if (challengeManager == null)
        {
            challengeManager = ChallengeManager.Instance != null
                ? ChallengeManager.Instance
                : (GameManager.Instance != null
                    ? GameManager.Instance.challengeManager
                    : FindObjectOfType<ChallengeManager>());
        }
    }

    [ContextMenu("Rebuild Mapping Cache")]
    public void BuildMaps()
    {
        missionMap.Clear();
        challengeMap.Clear();

        for (int i = 0; i < missionMappings.Count; i++)
        {
            MissionQuestMapping map = missionMappings[i];
            if (map == null || map.mission == null || map.quest == null) continue;

            if (!missionMap.ContainsKey(map.mission))
            {
                missionMap.Add(map.mission, map);
            }
        }

        for (int i = 0; i < challengeMappings.Count; i++)
        {
            ChallengeQuestMapping map = challengeMappings[i];
            if (map == null || map.challenge == null || map.quest == null) continue;

            if (!challengeMap.ContainsKey(map.challenge))
            {
                challengeMap.Add(map.challenge, map);
            }
        }
    }

    [ContextMenu("Auto Build Mappings By Name")]
    public void AutoBuildMappingsByName()
    {
        Quest[] allQuests = GetAllRepositoryQuests();
        if (allQuests == null || allQuests.Length == 0)
        {
            Debug.LogWarning("[GC2JournalBridge] No quests found in Quests Repository. Create/import GC2 Quest assets first.", this);
            return;
        }

        Dictionary<string, Quest> questsByKey = BuildQuestLookup(allQuests);

        int missionMatched = AutoMapMissionsByName(questsByKey);
        int challengeMatched = AutoMapChallengesByName(questsByKey);

        BuildMaps();

        Debug.Log(
            $"[GC2JournalBridge] Auto-map complete. Missions matched: {missionMatched}, Challenges matched: {challengeMatched}. " +
            $"Overwrite Existing: {overwriteExistingQuestAssignments}",
            this
        );

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    private int AutoMapMissionsByName(Dictionary<string, Quest> questsByKey)
    {
        List<MissionData> missions = GetMissionAssets();
        if (missions.Count == 0) return 0;

        Dictionary<MissionData, MissionQuestMapping> existing = new Dictionary<MissionData, MissionQuestMapping>();
        for (int i = 0; i < missionMappings.Count; i++)
        {
            MissionQuestMapping map = missionMappings[i];
            if (map == null || map.mission == null) continue;
            if (!existing.ContainsKey(map.mission)) existing.Add(map.mission, map);
        }

        int matches = 0;

        for (int i = 0; i < missions.Count; i++)
        {
            MissionData mission = missions[i];
            if (mission == null) continue;

            MissionQuestMapping map;
            if (!existing.TryGetValue(mission, out map))
            {
                map = new MissionQuestMapping { mission = mission };
                missionMappings.Add(map);
                existing[mission] = map;
            }

            if (!overwriteExistingQuestAssignments && map.quest != null) continue;

            Quest quest = FindQuestMatch(questsByKey, mission.missionName, mission.name);
            if (quest == null) continue;

            map.quest = quest;
            matches++;
        }

        return matches;
    }

    private int AutoMapChallengesByName(Dictionary<string, Quest> questsByKey)
    {
        List<ChallengeData> challenges = GetChallengeAssets();
        if (challenges.Count == 0) return 0;

        Dictionary<ChallengeData, ChallengeQuestMapping> existing = new Dictionary<ChallengeData, ChallengeQuestMapping>();
        for (int i = 0; i < challengeMappings.Count; i++)
        {
            ChallengeQuestMapping map = challengeMappings[i];
            if (map == null || map.challenge == null) continue;
            if (!existing.ContainsKey(map.challenge)) existing.Add(map.challenge, map);
        }

        int matches = 0;

        for (int i = 0; i < challenges.Count; i++)
        {
            ChallengeData challenge = challenges[i];
            if (challenge == null) continue;

            ChallengeQuestMapping map;
            if (!existing.TryGetValue(challenge, out map))
            {
                map = new ChallengeQuestMapping { challenge = challenge };
                challengeMappings.Add(map);
                existing[challenge] = map;
            }

            if (!overwriteExistingQuestAssignments && map.quest != null) continue;

            Quest quest = FindQuestMatch(questsByKey, challenge.challengeName, challenge.name);
            if (quest == null) continue;

            map.quest = quest;
            matches++;
        }

        return matches;
    }

    private static Dictionary<string, Quest> BuildQuestLookup(Quest[] quests)
    {
        Dictionary<string, Quest> lookup = new Dictionary<string, Quest>();

        for (int i = 0; i < quests.Length; i++)
        {
            Quest quest = quests[i];
            if (quest == null) continue;

            AddQuestKey(lookup, quest.name, quest);
        }

        return lookup;
    }

    private static void AddQuestKey(Dictionary<string, Quest> lookup, string rawName, Quest quest)
    {
        string key = NormalizeName(rawName);
        if (string.IsNullOrEmpty(key)) return;

        if (!lookup.ContainsKey(key))
        {
            lookup.Add(key, quest);
        }
    }

    private static Quest FindQuestMatch(Dictionary<string, Quest> lookup, string primaryName, string fallbackName)
    {
        if (TryGetQuest(lookup, primaryName, out Quest quest)) return quest;
        if (TryGetQuest(lookup, fallbackName, out quest)) return quest;

        return null;
    }

    private static bool TryGetQuest(Dictionary<string, Quest> lookup, string rawName, out Quest quest)
    {
        quest = null;
        if (string.IsNullOrWhiteSpace(rawName)) return false;

        string key = NormalizeName(rawName);
        if (string.IsNullOrEmpty(key)) return false;

        return lookup.TryGetValue(key, out quest);
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        StringBuilder sb = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    private Quest[] GetAllRepositoryQuests()
    {
        try
        {
            QuestsList questsList = Settings.From<QuestsRepository>().Quests;
            return questsList != null && questsList.Quests != null ? questsList.Quests : Array.Empty<Quest>();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[GC2JournalBridge] Could not read Quests Repository: {exception.Message}", this);
            return Array.Empty<Quest>();
        }
    }

    private static List<MissionData> GetMissionAssets()
    {
        MissionData[] resources = Resources.LoadAll<MissionData>("Missions");
        return resources != null ? new List<MissionData>(resources) : new List<MissionData>();
    }

    private static List<ChallengeData> GetChallengeAssets()
    {
        ChallengeData[] resources = Resources.LoadAll<ChallengeData>("Challenges");
        return resources != null ? new List<ChallengeData>(resources) : new List<ChallengeData>();
    }

    private void SubscribeEvents()
    {
        if (missionManager != null)
        {
            missionManager.onMissionStart.AddListener(OnMissionStarted);
            missionManager.onMissionComplete.AddListener(OnMissionCompleted);
            missionManager.onMissionFail.AddListener(OnMissionFailed);
        }

        if (challengeManager != null)
        {
            challengeManager.onChallengeStarted.AddListener(OnChallengeStarted);
            challengeManager.onChallengeCompleted.AddListener(OnChallengeCompleted);
            challengeManager.onChallengeFailed.AddListener(OnChallengeFailed);
            challengeManager.onChallengeExpired.AddListener(OnChallengeExpired);
        }
    }

    private void UnsubscribeEvents()
    {
        if (missionManager != null)
        {
            missionManager.onMissionStart.RemoveListener(OnMissionStarted);
            missionManager.onMissionComplete.RemoveListener(OnMissionCompleted);
            missionManager.onMissionFail.RemoveListener(OnMissionFailed);
        }

        if (challengeManager != null)
        {
            challengeManager.onChallengeStarted.RemoveListener(OnChallengeStarted);
            challengeManager.onChallengeCompleted.RemoveListener(OnChallengeCompleted);
            challengeManager.onChallengeFailed.RemoveListener(OnChallengeFailed);
            challengeManager.onChallengeExpired.RemoveListener(OnChallengeExpired);
        }
    }

    private void OnMissionStarted(MissionData mission)
    {
        _ = HandleMissionStartedAsync(mission);
    }

    private void OnMissionCompleted(MissionData mission)
    {
        _ = HandleMissionCompletedAsync(mission);
    }

    private void OnMissionFailed(MissionData mission)
    {
        _ = HandleMissionFailedAsync(mission);
    }

    private void OnChallengeStarted(ActiveChallenge challenge)
    {
        _ = HandleChallengeStartedAsync(challenge);
    }

    private void OnChallengeCompleted(ActiveChallenge challenge)
    {
        _ = HandleChallengeCompletedAsync(challenge);
    }

    private void OnChallengeFailed(ActiveChallenge challenge)
    {
        _ = HandleChallengeFailedAsync(challenge);
    }

    private void OnChallengeExpired(ActiveChallenge challenge)
    {
        _ = HandleChallengeFailedAsync(challenge);
    }

    private async System.Threading.Tasks.Task HandleMissionStartedAsync(MissionData mission)
    {
        if (!TryGetMissionMap(mission, out MissionQuestMapping map)) return;

        await ActivateAndTrackAsync(map.quest, map.trackOnActivate);
        Log($"Mission started -> Journal activate: {mission.missionName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task HandleMissionCompletedAsync(MissionData mission)
    {
        if (!TryGetMissionMap(mission, out MissionQuestMapping map)) return;
        if (!map.completeOnMissionComplete) return;

        await CompleteQuestFromRootsAsync(map.quest);
        Log($"Mission completed -> Journal complete: {mission.missionName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task HandleMissionFailedAsync(MissionData mission)
    {
        if (!TryGetMissionMap(mission, out MissionQuestMapping map)) return;
        if (!map.failOnMissionFail) return;

        await FailQuestFromRootsAsync(map.quest);
        Log($"Mission failed -> Journal fail: {mission.missionName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task HandleChallengeStartedAsync(ActiveChallenge activeChallenge)
    {
        if (activeChallenge == null || activeChallenge.challengeData == null) return;
        if (!TryGetChallengeMap(activeChallenge.challengeData, out ChallengeQuestMapping map)) return;

        await ActivateAndTrackAsync(map.quest, map.trackOnActivate);
        Log($"Challenge started -> Journal activate: {activeChallenge.challengeData.challengeName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task HandleChallengeCompletedAsync(ActiveChallenge activeChallenge)
    {
        if (activeChallenge == null || activeChallenge.challengeData == null) return;
        if (!TryGetChallengeMap(activeChallenge.challengeData, out ChallengeQuestMapping map)) return;
        if (!map.completeOnChallengeComplete) return;

        await CompleteQuestFromRootsAsync(map.quest);
        Log($"Challenge completed -> Journal complete: {activeChallenge.challengeData.challengeName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task HandleChallengeFailedAsync(ActiveChallenge activeChallenge)
    {
        if (activeChallenge == null || activeChallenge.challengeData == null) return;
        if (!TryGetChallengeMap(activeChallenge.challengeData, out ChallengeQuestMapping map)) return;
        if (!map.failOnChallengeFailOrExpire) return;

        await FailQuestFromRootsAsync(map.quest);
        Log($"Challenge failed/expired -> Journal fail: {activeChallenge.challengeData.challengeName} -> {map.quest.name}");
    }

    private async System.Threading.Tasks.Task ActivateAndTrackAsync(Quest quest, bool track)
    {
        if (!EnsureJournalAndQuest(quest)) return;

        if (journal.IsQuestInactive(quest))
        {
            await journal.ActivateQuest(quest);
        }

        if (track && journal.IsQuestActive(quest) && !journal.IsQuestTracking(quest))
        {
            journal.TrackQuest(quest);
        }
    }

    private async System.Threading.Tasks.Task CompleteQuestFromRootsAsync(Quest quest)
    {
        if (!EnsureJournalAndQuest(quest)) return;

        if (journal.IsQuestCompleted(quest)) return;

        if (journal.IsQuestInactive(quest))
        {
            await journal.ActivateQuest(quest);
        }

        int[] rootIds = quest.Tasks.RootIds;
        if (rootIds != null)
        {
            for (int i = 0; i < rootIds.Length; i++)
            {
                int taskId = rootIds[i];
                if (journal.IsTaskCompleted(quest, taskId)) continue;

                await journal.CompleteTask(quest, taskId);
            }
        }
    }

    private async System.Threading.Tasks.Task FailQuestFromRootsAsync(Quest quest)
    {
        if (!EnsureJournalAndQuest(quest)) return;

        if (journal.IsQuestFailed(quest)) return;

        if (journal.IsQuestInactive(quest))
        {
            await journal.ActivateQuest(quest);
        }

        int[] rootIds = quest.Tasks.RootIds;
        if (rootIds != null)
        {
            for (int i = 0; i < rootIds.Length; i++)
            {
                int taskId = rootIds[i];
                if (journal.IsTaskFailed(quest, taskId)) continue;

                await journal.FailTask(quest, taskId);
            }
        }
    }

    private bool TryGetMissionMap(MissionData mission, out MissionQuestMapping map)
    {
        map = null;
        if (mission == null) return false;

        if (missionMap.Count == 0)
        {
            BuildMaps();
        }

        return missionMap.TryGetValue(mission, out map);
    }

    private bool TryGetChallengeMap(ChallengeData challenge, out ChallengeQuestMapping map)
    {
        map = null;
        if (challenge == null) return false;

        if (challengeMap.Count == 0)
        {
            BuildMaps();
        }

        return challengeMap.TryGetValue(challenge, out map);
    }

    private bool EnsureJournalAndQuest(Quest quest)
    {
        if (quest == null) return false;

        if (journal == null)
        {
            ResolveReferences();
        }

        if (journal == null)
        {
            Debug.LogWarning("[GC2JournalBridge] Journal reference missing. Assign a Journal component in the inspector.", this);
            return false;
        }

        return true;
    }

    private void Log(string message)
    {
        if (!verboseLogging) return;
        Debug.Log($"[GC2JournalBridge] {message}", this);
    }
}
