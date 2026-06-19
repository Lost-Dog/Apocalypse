using EmeraldAI;
using UnityEngine;

/// <summary>
/// Routes Emerald AI damage calls into the project's GC2 player provider so AI bullets
/// and abilities damage the same health/armour values used by HUD and survival systems.
/// </summary>
[DisallowMultipleComponent]
public class EmeraldGC2PlayerBridge : EmeraldPlayerBridge
{
    [SerializeField] private GC2PlayerProvider playerProvider;
    [SerializeField] private bool autoResolveProvider = true;
    [SerializeField] private bool showCombatText = true;

    private Collider cachedCollider;

    public override void Awake()
    {
        base.Awake();
        cachedCollider = GetComponent<Collider>();
        ResolveProvider();
    }

    public override void Start()
    {
        ResolveProvider();

        if (playerProvider == null)
        {
            base.Start();
            return;
        }

        StartHealth = Mathf.Max(1, Mathf.RoundToInt(playerProvider.MaxHealth));
        Health = Mathf.RoundToInt(playerProvider.Health);
    }

    public override void DamageCharacterController(int DamageAmount, Transform Target)
    {
        if (Immortal || DamageAmount <= 0)
            return;

        ResolveProvider();

        if (playerProvider == null)
        {
            base.DamageCharacterController(DamageAmount, Target);
            return;
        }

        playerProvider.ApplyDamage(DamageAmount);

        StartHealth = Mathf.Max(1, Mathf.RoundToInt(playerProvider.MaxHealth));
        Health = Mathf.RoundToInt(playerProvider.Health);

        if (showCombatText)
            DisplayDamageText(DamageAmount);

        OnTakeDamage?.Invoke();

        if (Health <= 0)
        {
            if (cachedCollider != null)
                cachedCollider.enabled = false;

            OnDeath?.Invoke();
        }
    }

    private void ResolveProvider()
    {
        if (!autoResolveProvider && playerProvider != null)
            return;

        if (playerProvider == null)
            playerProvider = GetComponent<GC2PlayerProvider>();

        if (playerProvider == null)
            playerProvider = FindFirstObjectByType<GC2PlayerProvider>();
    }
}
