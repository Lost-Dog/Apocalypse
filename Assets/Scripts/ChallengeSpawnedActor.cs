using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// Runtime link between a spawned challenge actor and its parent challenge.
/// Handles objective callbacks when the actor is eliminated or rescued.
/// </summary>
public class ChallengeSpawnedActor : MonoBehaviour
{
    [SerializeField] private ChallengeData.SpawnableCategory category = ChallengeData.SpawnableCategory.Other;
    [SerializeField] private bool countDisableAsElimination = false;
    [SerializeField] private bool countDestroyAsElimination = false;
    [SerializeField] private bool enableReflectionDeathProbe = false;

    private ActiveChallenge _challenge;
    private ChallengeManager _manager;
    private bool _callbacksSuppressed;
    private bool _finalized;
    private float _nextDeathProbeTime;
    private MonoBehaviour[] _cachedBehaviours;

    private static readonly string[] DeadFlagNames =
    {
        "isDead", "IsDead", "dead", "Dead", "isDefeated", "IsDefeated"
    };

    private static readonly string[] HealthNames =
    {
        "health", "Health", "currentHealth", "CurrentHealth"
    };

    public void Initialize(ActiveChallenge challenge, ChallengeData.SpawnableCategory spawnCategory)
    {
        _challenge = challenge;
        category = spawnCategory;
        _manager = ChallengeManager.Instance;
        _callbacksSuppressed = false;
        _finalized = false;
        _nextDeathProbeTime = 0f;
        _cachedBehaviours = GetComponentsInChildren<MonoBehaviour>(true);
    }

    public void SuppressCallbacks()
    {
        _callbacksSuppressed = true;
    }

    /// <summary>
    /// Call this from interaction logic when a civilian is safely rescued.
    /// </summary>
    public void MarkCivilianRescued()
    {
        if (_finalized || _callbacksSuppressed) return;
        if (_challenge == null) return;

        _finalized = true;
        _manager = _manager != null ? _manager : ChallengeManager.Instance;
        _manager?.OnCivilianRescued(_challenge);
    }

    /// <summary>
    /// Call this from damage/death logic if you can explicitly detect elimination.
    /// </summary>
    public void MarkEliminated()
    {
        if (_finalized || _callbacksSuppressed) return;
        DispatchEliminationCallback();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (_callbacksSuppressed || _finalized) return;
        if (!countDisableAsElimination) return;

        // In pooled setups, deaths often disable objects instead of destroying them.
        DispatchEliminationCallback();
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (_callbacksSuppressed || _finalized) return;
        if (!countDestroyAsElimination) return;

        DispatchEliminationCallback();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_callbacksSuppressed || _finalized) return;
        if (category != ChallengeData.SpawnableCategory.Civilian) return;

        if (other != null && other.CompareTag("SafeZone"))
        {
            MarkCivilianRescued();
        }
    }

    private void Update()
    {
        if (_callbacksSuppressed || _finalized) return;
        if (!enableReflectionDeathProbe) return;
        if (category != ChallengeData.SpawnableCategory.Enemy && category != ChallengeData.SpawnableCategory.Boss) return;
        if (Time.time < _nextDeathProbeTime) return;

        _nextDeathProbeTime = Time.time + 0.25f;

        if (LooksDeadByReflection())
        {
            DispatchEliminationCallback();
        }
    }

    private bool LooksDeadByReflection()
    {
        if (_cachedBehaviours == null || _cachedBehaviours.Length == 0)
        {
            _cachedBehaviours = GetComponentsInChildren<MonoBehaviour>(true);
        }

        for (int i = 0; i < _cachedBehaviours.Length; i++)
        {
            MonoBehaviour behaviour = _cachedBehaviours[i];
            if (behaviour == null) continue;

            Type type = behaviour.GetType();

            if (HasDeadFlag(type, behaviour))
            {
                return true;
            }

            if (HasZeroHealth(type, behaviour))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDeadFlag(Type type, object target)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (int i = 0; i < DeadFlagNames.Length; i++)
        {
            string name = DeadFlagNames[i];

            FieldInfo field = type.GetField(name, flags);
            if (field != null && field.FieldType == typeof(bool))
            {
                if ((bool)field.GetValue(target)) return true;
            }

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null && property.CanRead && property.PropertyType == typeof(bool))
            {
                object value = property.GetValue(target);
                if (value is bool dead && dead) return true;
            }
        }

        return false;
    }

    private static bool HasZeroHealth(Type type, object target)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (int i = 0; i < HealthNames.Length; i++)
        {
            string name = HealthNames[i];

            FieldInfo field = type.GetField(name, flags);
            if (field != null && TryReadNumeric(field.FieldType, field.GetValue(target), out float healthValue))
            {
                if (healthValue <= 0f) return true;
            }

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null && property.CanRead && TryReadNumeric(property.PropertyType, property.GetValue(target), out healthValue))
            {
                if (healthValue <= 0f) return true;
            }
        }

        return false;
    }

    private static bool TryReadNumeric(Type valueType, object value, out float numeric)
    {
        numeric = 0f;
        if (value == null) return false;

        if (valueType == typeof(float))
        {
            numeric = (float)value;
            return true;
        }

        if (valueType == typeof(double))
        {
            numeric = (float)(double)value;
            return true;
        }

        if (valueType == typeof(int))
        {
            numeric = (int)value;
            return true;
        }

        return false;
    }

    private void DispatchEliminationCallback()
    {
        if (_finalized) return;
        if (_challenge == null) return;

        _finalized = true;
        _manager = _manager != null ? _manager : ChallengeManager.Instance;

        if (_manager == null) return;

        if (category == ChallengeData.SpawnableCategory.Enemy || category == ChallengeData.SpawnableCategory.Boss)
        {
            _manager.OnEnemyKilled(_challenge);
            return;
        }

        if (category == ChallengeData.SpawnableCategory.Civilian)
        {
            _manager.OnCivilianDied(_challenge);
        }
    }
}
