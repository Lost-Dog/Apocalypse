using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JUTPS;

public class ConsumableEffectHandler : MonoBehaviour
{
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    
    private class ActiveEffect
    {
        public ConsumableItem item;
        public float remainingTime;
        public float tickInterval = 1f;
        public float nextTickTime;
        
        public ActiveEffect(ConsumableItem item, float duration)
        {
            this.item = item;
            this.remainingTime = duration;
            this.nextTickTime = tickInterval;
        }
    }
    
    public void ApplyEffect(ConsumableItem item, float duration)
    {
        activeEffects.Add(new ActiveEffect(item, duration));
        
        if (activeEffects.Count == 1)
        {
            StartCoroutine(ProcessEffects());
        }
    }
    
    private IEnumerator ProcessEffects()
    {
        JUCharacterController character = GetComponent<JUCharacterController>();
        JUHealth health = GetComponent<JUHealth>();
        
        while (activeEffects.Count > 0)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveEffect effect = activeEffects[i];
                
                effect.remainingTime -= Time.deltaTime;
                effect.nextTickTime -= Time.deltaTime;
                
                if (effect.nextTickTime <= 0f)
                {
                    float tickValue = effect.item.healthRestore * (effect.tickInterval / effect.item.effectDuration);
                    
                    if (health != null && tickValue > 0f)
                    {
                        health.Health += tickValue;
                        health.Health = Mathf.Clamp(health.Health, 0f, health.MaxHealth);
                    }
                    
                    effect.nextTickTime = effect.tickInterval;
                }
                
                if (effect.remainingTime <= 0f)
                {
                    activeEffects.RemoveAt(i);
                }
            }
            
            yield return null;
        }
    }
    
    public bool HasEffect(ConsumableItem item)
    {
        return activeEffects.Exists(e => e.item == item);
    }
    
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }
}
