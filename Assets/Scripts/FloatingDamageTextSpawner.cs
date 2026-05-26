using System.Collections.Generic;
using UnityEngine;
using Invector;

/// <summary>
/// Hooks into Invector's vHealthController.onReceiveDamage and spawns floating
/// damage popups in The Division style. Maintains its own internal pool.
///
/// Usage:
///   Add this component to ANY GameObject that has a vHealthController (enemy, civilian, player).
///   Assign the DamageTextPrefab, then wire vHealthController.onReceiveDamage → OnDamageReceived.
///   For the player, toggle IsPlayerCharacter so damage numbers render in red.
///
/// Alternatively, use FloatingDamageTextManager.SpawnAt() from anywhere in code to
/// spawn a number without needing a component reference.
/// </summary>
public class FloatingDamageTextSpawner : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Prefab")]
    [Tooltip("Prefab with a FloatingDamageText component and a TextMeshPro.")]
    public FloatingDamageText damageTextPrefab;

    [Header("Spawn Settings")]
    [Tooltip("World-space vertical offset above this transform's origin where numbers appear.")]
    public float spawnHeightOffset = 1.8f;

    [Tooltip("Enable to use the player (red) colour scheme instead of enemy (white/gold).")]
    public bool isPlayerCharacter = false;

    [Header("Critical Hit Detection")]
    [Tooltip("Damage amount above this fraction of max health is treated as a critical hit (0 = disabled).")]
    [Range(0f, 1f)]
    public float criticalHitThreshold = 0f;

    [Tooltip("Or use this absolute damage value as the crit threshold (0 = use fraction above).")]
    public float criticalHitAbsoluteThreshold = 30f;

    [Header("Pool")]
    public int poolInitialSize = 8;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private readonly Queue<FloatingDamageText> _pool = new Queue<FloatingDamageText>();
    private Transform _poolParent;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        GameObject poolRoot = new GameObject($"[DamageTextPool] {gameObject.name}");
        poolRoot.transform.SetParent(transform);
        _poolParent = poolRoot.transform;

        for (int i = 0; i < poolInitialSize; i++)
            CreateInstance();
    }

    // -------------------------------------------------------------------------
    // Public API — wire to vHealthController.onReceiveDamage in Inspector
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by vHealthController.onReceiveDamage. Spawns a popup at the hit position.
    /// Wire this in the Inspector: vHealthController → onReceiveDamage → OnDamageReceived.
    /// </summary>
    public void OnDamageReceived(vDamage damage)
    {
        if (damage == null || damage.damageValue <= 0f)
            return;

        Vector3 worldPos = damage.hitPosition != Vector3.zero
            ? damage.hitPosition
            : transform.position + Vector3.up * spawnHeightOffset;

        worldPos.y = Mathf.Max(worldPos.y, transform.position.y + spawnHeightOffset * 0.5f);

        FloatingDamageText.DamageKind kind = DetermineKind(damage);
        Spawn(worldPos, damage.damageValue, kind);
    }

    /// <summary>
    /// Programmatic spawn — use from code without needing a vDamage reference.
    /// </summary>
    public void SpawnAt(Vector3 worldPosition, float value, FloatingDamageText.DamageKind kind)
    {
        Spawn(worldPosition, value, kind);
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

    private FloatingDamageText.DamageKind DetermineKind(vDamage damage)
    {
        if (isPlayerCharacter)
            return FloatingDamageText.DamageKind.PlayerReceived;

        if (criticalHitAbsoluteThreshold > 0f && damage.damageValue >= criticalHitAbsoluteThreshold)
            return FloatingDamageText.DamageKind.Critical;

        return FloatingDamageText.DamageKind.Normal;
    }

    private void Spawn(Vector3 position, float value, FloatingDamageText.DamageKind kind)
    {
        FloatingDamageText instance = GetFromPool();
        instance.transform.position   = position;
        instance.transform.localScale = Vector3.one;
        instance.gameObject.SetActive(true);
        instance.Play(value, kind);
    }

    private FloatingDamageText GetFromPool()
    {
        if (_pool.Count > 0)
        {
            FloatingDamageText obj = _pool.Dequeue();
            if (obj != null)
                return obj;
        }

        return CreateInstance();
    }

    private FloatingDamageText CreateInstance()
    {
        if (damageTextPrefab == null)
        {
            Debug.LogError($"[FloatingDamageTextSpawner] damageTextPrefab is not assigned on '{gameObject.name}'.");
            return null;
        }

        FloatingDamageText instance = Instantiate(damageTextPrefab, _poolParent);
        instance.Initialise(ReturnToPool);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void ReturnToPool(FloatingDamageText instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_poolParent);
        _pool.Enqueue(instance);
    }
}
