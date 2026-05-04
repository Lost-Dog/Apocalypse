using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Stats;

/// <summary>
/// Applies consumable effects over time on a GC2 Traits-bearing character.
/// Health ticks are written directly to the Traits health attribute value.
/// </summary>
public class ConsumableEffectHandler : MonoBehaviour
{
    private const string HealthAttributeId = "health";

    private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    private class ActiveEffect
    {
        public ConsumableItem item;
        public float remainingTime;
        public readonly float tickInterval = 1f;
        public float nextTickTime;

        public ActiveEffect(ConsumableItem item, float duration)
        {
            this.item          = item;
            this.remainingTime = duration;
            this.nextTickTime  = tickInterval;
        }
    }

    /// <summary>Begins applying the consumable effect for the given duration.</summary>
    public void ApplyEffect(ConsumableItem item, float duration)
    {
        activeEffects.Add(new ActiveEffect(item, duration));

        if (activeEffects.Count == 1)
            StartCoroutine(ProcessEffects());
    }

    private IEnumerator ProcessEffects()
    {
        Traits traits = GetComponent<Traits>();

        while (activeEffects.Count > 0)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveEffect effect = activeEffects[i];
                effect.remainingTime -= Time.deltaTime;
                effect.nextTickTime  -= Time.deltaTime;

                if (effect.nextTickTime <= 0f)
                {
                    float tickRatio = effect.tickInterval / effect.item.effectDuration;
                    float tickValue = effect.item.healthRestore * tickRatio;

                    if (traits != null && tickValue > 0f)
                    {
                        try
                        {
                            RuntimeAttributeData health = traits.RuntimeAttributes.Get(HealthAttributeId);
                            health.Value = System.Math.Min(health.Value + tickValue, health.MaxValue);
                        }
                        catch (System.Exception) { }
                    }

                    effect.nextTickTime = effect.tickInterval;
                }

                if (effect.remainingTime <= 0f)
                    activeEffects.RemoveAt(i);
            }

            yield return null;
        }
    }

    /// <summary>Returns true if an effect from the given item is currently active.</summary>
    public bool HasEffect(ConsumableItem item) => activeEffects.Exists(e => e.item == item);

    /// <summary>Returns the number of currently active effects.</summary>
    public int GetActiveEffectCount() => activeEffects.Count;
}
