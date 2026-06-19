using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class SceneActorCullingTarget : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("If enabled, this actor uses its own distances instead of manager defaults.")]
    public bool overrideDistances;

    [Tooltip("When culled, actor becomes active again once within this distance.")]
    public float activeDistance = 45f;

    [Tooltip("When active, actor becomes culled once beyond this distance.")]
    public float cullDistance = 65f;

    [Tooltip("Positive values cull earlier. Negative values cull later.")]
    public float distanceBias = 0f;

    [Header("Managed Components")]
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public List<MonoBehaviour> aiBehaviours = new List<MonoBehaviour>();
    public List<Renderer> renderers = new List<Renderer>();
    public List<Collider> colliders = new List<Collider>();

    [Header("Options")]
    public bool manageAnimator = true;
    public bool manageNavMeshAgent = true;
    public bool manageAIBehaviours = true;
    public bool manageRenderers = false;
    public bool manageColliders = false;
    public bool ignoreIfTaggedPlayer = true;

    [Header("Debug")]
    [SerializeField] private bool isCulled;
    [SerializeField] private float lastDistance;

    private bool originalAnimatorEnabled;
    private bool originalNavMeshAgentEnabled;
    private readonly Dictionary<MonoBehaviour, bool> originalAIEnabled = new Dictionary<MonoBehaviour, bool>();
    private readonly Dictionary<Renderer, bool> originalRendererEnabled = new Dictionary<Renderer, bool>();
    private readonly Dictionary<Collider, bool> originalColliderEnabled = new Dictionary<Collider, bool>();

    public bool IsCulled => isCulled;

    public bool ShouldBeManaged
    {
        get
        {
            if (!isActiveAndEnabled) return false;
            if (ignoreIfTaggedPlayer && CompareTag("Player")) return false;
            return true;
        }
    }

    private void Reset()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        CacheOriginalState();
    }

    private void OnEnable()
    {
        CacheOriginalState();
        SceneActorCullingManager.Register(this);
    }

    private void OnDisable()
    {
        SceneActorCullingManager.Unregister(this);
    }

    private void OnValidate()
    {
        if (activeDistance < 0f) activeDistance = 0f;
        if (cullDistance < activeDistance) cullDistance = activeDistance;
    }

    [ContextMenu("Auto Assign Animator and NavMeshAgent")]
    private void AutoAssignCoreComponents()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    [ContextMenu("Auto Fill AI Behaviours (This Object)")]
    private void AutoFillAIBehavioursOnThisObject()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        aiBehaviours.Clear();

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null) continue;
            if (behaviour == this) continue;
            aiBehaviours.Add(behaviour);
        }

        CacheOriginalState();
    }

    [ContextMenu("Auto Fill Renderers (Children)")]
    public void AutoFillRenderersOnChildren()
    {
        Renderer[] found = GetComponentsInChildren<Renderer>(true);
        renderers.Clear();

        for (int i = 0; i < found.Length; i++)
        {
            Renderer renderer = found[i];
            if (renderer == null) continue;
            renderers.Add(renderer);
        }

        CacheOriginalState();
    }

    [ContextMenu("Auto Fill Colliders (Children)")]
    public void AutoFillCollidersOnChildren()
    {
        Collider[] found = GetComponentsInChildren<Collider>(true);
        colliders.Clear();

        for (int i = 0; i < found.Length; i++)
        {
            Collider collider = found[i];
            if (collider == null) continue;
            colliders.Add(collider);
        }

        CacheOriginalState();
    }

    public void AutoFillStaticChunkComponents()
    {
        AutoFillRenderersOnChildren();
        AutoFillCollidersOnChildren();
    }

    public void ApplyCulledState(bool culled, float distance)
    {
        lastDistance = distance;

        if (isCulled == culled)
            return;

        isCulled = culled;

        if (manageAnimator && animator != null)
        {
            animator.enabled = culled ? false : originalAnimatorEnabled;
        }

        if (manageNavMeshAgent && navMeshAgent != null)
        {
            navMeshAgent.enabled = culled ? false : originalNavMeshAgentEnabled;
        }

        if (manageAIBehaviours)
        {
            for (int i = aiBehaviours.Count - 1; i >= 0; i--)
            {
                MonoBehaviour behaviour = aiBehaviours[i];
                if (behaviour == null)
                {
                    aiBehaviours.RemoveAt(i);
                    continue;
                }

                if (!originalAIEnabled.TryGetValue(behaviour, out bool initialState))
                {
                    initialState = behaviour.enabled;
                    originalAIEnabled[behaviour] = initialState;
                }

                behaviour.enabled = culled ? false : initialState;
            }
        }

        if (manageRenderers)
        {
            for (int i = renderers.Count - 1; i >= 0; i--)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    renderers.RemoveAt(i);
                    continue;
                }

                if (!originalRendererEnabled.TryGetValue(renderer, out bool initialState))
                {
                    initialState = renderer.enabled;
                    originalRendererEnabled[renderer] = initialState;
                }

                renderer.enabled = culled ? false : initialState;
            }
        }

        if (manageColliders)
        {
            for (int i = colliders.Count - 1; i >= 0; i--)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    colliders.RemoveAt(i);
                    continue;
                }

                if (!originalColliderEnabled.TryGetValue(collider, out bool initialState))
                {
                    initialState = collider.enabled;
                    originalColliderEnabled[collider] = initialState;
                }

                collider.enabled = culled ? false : initialState;
            }
        }
    }

    public void CacheOriginalState()
    {
        if (animator != null)
            originalAnimatorEnabled = animator.enabled;

        if (navMeshAgent != null)
            originalNavMeshAgentEnabled = navMeshAgent.enabled;

        originalAIEnabled.Clear();
        for (int i = aiBehaviours.Count - 1; i >= 0; i--)
        {
            MonoBehaviour behaviour = aiBehaviours[i];
            if (behaviour == null)
            {
                aiBehaviours.RemoveAt(i);
                continue;
            }

            originalAIEnabled[behaviour] = behaviour.enabled;
        }

        originalRendererEnabled.Clear();
        for (int i = renderers.Count - 1; i >= 0; i--)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                renderers.RemoveAt(i);
                continue;
            }

            originalRendererEnabled[renderer] = renderer.enabled;
        }

        originalColliderEnabled.Clear();
        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                colliders.RemoveAt(i);
                continue;
            }

            originalColliderEnabled[collider] = collider.enabled;
        }
    }
}