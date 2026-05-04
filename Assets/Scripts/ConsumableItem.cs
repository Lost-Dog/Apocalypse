using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;

/// <summary>
/// ScriptableObject defining a consumable item and its stat effects.
/// Health restoration writes directly to the player's GC2 Traits health attribute.
/// Survival stats (hunger, thirst, stamina, temperature, infection) go through SurvivalManager.
/// </summary>
[CreateAssetMenu(fileName = "New Consumable", menuName = "Apocalypse/Items/Consumable Item")]
public class ConsumableItem : LootItemData
{
    private const string HealthAttributeId = "health";

    [Header("Consumable Properties")]
    [Tooltip("Health restored when consumed")]
    public float healthRestore = 0f;

    [Tooltip("Stamina restored when consumed")]
    public float staminaRestore = 0f;

    [Tooltip("Hunger restored when consumed (food)")]
    public float hungerRestore = 0f;

    [Tooltip("Thirst restored when consumed (water)")]
    public float thirstRestore = 0f;

    [Tooltip("Temperature change when consumed")]
    public float temperatureChange = 0f;

    [Tooltip("Infection change when consumed (use negative for cure)")]
    public float infectionChange = 0f;

    [Tooltip("Percentage of infection to cure (0-100). Pauses infection growth at cured level.")]
    [Range(0f, 100f)]
    public float infectionCurePercentage = 0f;

    [Tooltip("XP granted when consumed")]
    public int xpGrant = 0;

    [Tooltip("Duration of the effect (0 = instant)")]
    public float effectDuration = 0f;

    [Tooltip("Cooldown time after use")]
    public float cooldownTime = 0f;

    [Header("Visual Effects")]
    public GameObject useEffectPrefab;
    public AudioClip useSound;

    [Header("Animation")]
    public string useAnimationTrigger = "UseItem";

    /// <summary>
    /// Applies this consumable's effect to the user GameObject.
    /// Requires a GC2 Character component (and optionally Traits, SurvivalManager).
    /// </summary>
    public virtual void Use(GameObject user)
    {
        Character character = user.GetComponent<Character>();

        if (character == null)
        {
            Debug.LogWarning($"[ConsumableItem] '{user.name}' has no GC2 Character component.");
            return;
        }

        if (effectDuration > 0f)
        {
            ApplyOverTime(user, effectDuration);
        }
        else
        {
            ApplyInstant(user);
        }

        if (useEffectPrefab != null)
        {
            GameObject effect = Instantiate(useEffectPrefab, character.transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        AudioSource audio = user.GetComponent<AudioSource>();
        if (useSound != null && audio != null)
            audio.PlayOneShot(useSound);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification($"Used {itemName}");
    }

    /// <summary>Applies the effect immediately in one frame.</summary>
    protected virtual void ApplyInstant(GameObject user)
    {
        // Health via GC2 Traits
        if (healthRestore > 0f)
        {
            Traits traits = user.GetComponent<Traits>();
            if (traits != null)
            {
                try
                {
                    RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
                    health.Value = System.Math.Min(health.Value + healthRestore, health.MaxValue);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ConsumableItem] Could not restore health — {e.Message}");
                }
            }
        }

        // Survival stats
        if (SurvivalManager.Instance != null)
        {
            if (hungerRestore > 0f)       SurvivalManager.Instance.AddHunger(hungerRestore);
            if (thirstRestore > 0f)       SurvivalManager.Instance.AddThirst(thirstRestore);
            if (staminaRestore > 0f)      SurvivalManager.Instance.AddStamina(staminaRestore);
            if (temperatureChange != 0f)  SurvivalManager.Instance.AddTemperature(temperatureChange);

            if (infectionChange != 0f)
            {
                if (infectionChange < 0f)
                    SurvivalManager.Instance.CureInfection(-infectionChange);
                else
                    SurvivalManager.Instance.AddInfection(infectionChange);
            }

            if (infectionCurePercentage > 0f)
                SurvivalManager.Instance.CureInfectionPartial(infectionCurePercentage);
        }

        if (GameManager.Instance != null && xpGrant > 0)
            GameManager.Instance.progressionManager.AddExperience(xpGrant);
    }

    /// <summary>Delegates a timed over-time effect to the ConsumableEffectHandler on the user.</summary>
    protected virtual void ApplyOverTime(GameObject user, float duration)
    {
        ConsumableEffectHandler handler = user.GetComponent<ConsumableEffectHandler>()
                                       ?? user.AddComponent<ConsumableEffectHandler>();
        handler.ApplyEffect(this, duration);
    }
}
