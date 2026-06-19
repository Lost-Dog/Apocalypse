using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneActorCullingManager : MonoBehaviour
{
    private static readonly HashSet<SceneActorCullingTarget> RegisteredTargets = new HashSet<SceneActorCullingTarget>();
    private static SceneActorCullingManager instance;

    [Header("Player Source")]
    public Transform playerTransform;
    public bool autoFindPlayerByTag = true;
    public string playerTag = "Player";

    [Header("Global Distances")]
    [Tooltip("When a culled target gets closer than this distance, it is restored.")]
    public float globalActiveDistance = 45f;

    [Tooltip("When an active target goes beyond this distance, it is culled.")]
    public float globalCullDistance = 65f;

    [Header("Update Budget")]
    [Tooltip("How often culling checks run.")]
    public float updateInterval = 0.2f;

    [Tooltip("Maximum targets processed per tick. Set to 0 to process all.")]
    public int maxTargetsPerTick = 128;

    [Tooltip("How often target list is rebuilt to catch spawned actors.")]
    public float refreshTargetsInterval = 2f;

    [Header("Debug")]
    [SerializeField] private int trackedTargets;

    private readonly List<SceneActorCullingTarget> targets = new List<SceneActorCullingTarget>();
    private Coroutine cullingRoutine;
    private int scanCursor;
    private float refreshTimer;

    public static void Register(SceneActorCullingTarget target)
    {
        if (target == null) return;
        RegisteredTargets.Add(target);
        if (instance != null)
        {
            instance.AddTargetIfMissing(target);
        }
    }

    public static void Unregister(SceneActorCullingTarget target)
    {
        if (target == null) return;
        RegisteredTargets.Remove(target);
        if (instance != null)
        {
            instance.targets.Remove(target);
            instance.trackedTargets = instance.targets.Count;
        }
    }

    private void Awake()
    {
        instance = this;
        RefreshTargets();
    }

    private void OnEnable()
    {
        if (cullingRoutine == null)
        {
            cullingRoutine = StartCoroutine(CullingLoop());
        }
    }

    private void OnDisable()
    {
        if (cullingRoutine != null)
        {
            StopCoroutine(cullingRoutine);
            cullingRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private IEnumerator CullingLoop()
    {
        while (enabled)
        {
            if (autoFindPlayerByTag && playerTransform == null)
            {
                FindPlayerTransform();
            }

            refreshTimer += Mathf.Max(updateInterval, 0.01f);
            if (refreshTimer >= Mathf.Max(refreshTargetsInterval, 0.2f))
            {
                refreshTimer = 0f;
                RefreshTargets();
            }

            TickCulling();

            if (updateInterval <= 0f)
                yield return null;
            else
                yield return new WaitForSeconds(updateInterval);
        }
    }

    private void TickCulling()
    {
        if (playerTransform == null || targets.Count == 0)
            return;

        int count = targets.Count;
        int budget = maxTargetsPerTick <= 0 ? count : Mathf.Min(maxTargetsPerTick, count);

        for (int i = 0; i < budget; i++)
        {
            if (count == 0)
                break;

            if (scanCursor >= count)
                scanCursor = 0;

            SceneActorCullingTarget target = targets[scanCursor];
            scanCursor++;

            if (target == null)
                continue;

            if (!target.ShouldBeManaged)
                continue;

            float activeDistance = target.overrideDistances ? target.activeDistance : globalActiveDistance;
            float cullDistance = target.overrideDistances ? target.cullDistance : globalCullDistance;

            if (cullDistance < activeDistance)
                cullDistance = activeDistance;

            float adjustedActiveDistance = Mathf.Max(0f, activeDistance - target.distanceBias);
            float adjustedCullDistance = Mathf.Max(0f, cullDistance - target.distanceBias);

            float distanceSqr = (playerTransform.position - target.transform.position).sqrMagnitude;
            float activeDistanceSqr = adjustedActiveDistance * adjustedActiveDistance;
            float cullDistanceSqr = adjustedCullDistance * adjustedCullDistance;

            bool nextCulled = target.IsCulled
                ? distanceSqr > activeDistanceSqr
                : distanceSqr > cullDistanceSqr;

            target.ApplyCulledState(nextCulled, Mathf.Sqrt(distanceSqr));
        }
    }

    private void FindPlayerTransform()
    {
        if (string.IsNullOrEmpty(playerTag))
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    [ContextMenu("Refresh Targets")]
    public void RefreshTargets()
    {
        targets.Clear();

        foreach (SceneActorCullingTarget target in RegisteredTargets)
        {
            AddTargetIfMissing(target);
        }

        SceneActorCullingTarget[] found = FindObjectsByType<SceneActorCullingTarget>(FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            AddTargetIfMissing(found[i]);
        }

        trackedTargets = targets.Count;
        if (scanCursor >= trackedTargets)
            scanCursor = 0;
    }

    private void AddTargetIfMissing(SceneActorCullingTarget target)
    {
        if (target == null)
            return;

        if (targets.Contains(target))
            return;

        targets.Add(target);
        trackedTargets = targets.Count;
    }

    private void OnValidate()
    {
        if (globalActiveDistance < 0f) globalActiveDistance = 0f;
        if (globalCullDistance < globalActiveDistance) globalCullDistance = globalActiveDistance;
        if (updateInterval < 0f) updateInterval = 0f;
        if (refreshTargetsInterval < 0.2f) refreshTargetsInterval = 0.2f;
        if (maxTargetsPerTick < 0) maxTargetsPerTick = 0;
    }
}