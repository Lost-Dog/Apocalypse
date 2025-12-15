using UnityEngine;
using JUTPS;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Apocalypse/Items/Consumable Item")]
public class ConsumableItem : LootItemData
{
    [Header("Consumable Properties")]
    [Tooltip("Health restored when consumed")]
    public float healthRestore = 0f;
    
    [Tooltip("Stamina restored when consumed")]
    public float staminaRestore = 0f;
    
    [Tooltip("Temperature change when consumed")]
    public float temperatureChange = 0f;
    
    [Tooltip("Infection change when consumed")]
    public float infectionChange = 0f;
    
    [Tooltip("XP granted when consumed")]
    public int xpGrant = 0;
    
    [Tooltip("Duration of the effect (0 = instant)")]
    public float effectDuration = 0f;
    
    [Tooltip("Can this item be stacked?")]
    public bool isStackable = true;
    
    [Tooltip("Maximum stack size")]
    public int maxStackSize = 99;
    
    [Tooltip("Cooldown time after use")]
    public float cooldownTime = 0f;
    
    [Header("Visual Effects")]
    public GameObject useEffectPrefab;
    public AudioClip useSound;
    
    [Header("Animation")]
    public string useAnimationTrigger = "UseItem";
    
    public virtual void Use(GameObject user)
    {
        JUCharacterController character = user.GetComponent<JUCharacterController>();
        
        if (character != null)
        {
            if (effectDuration > 0f)
            {
                ApplyOverTime(character, effectDuration);
            }
            else
            {
                ApplyInstant(character);
            }
            
            if (useEffectPrefab != null)
            {
                GameObject effect = Instantiate(useEffectPrefab, character.transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }
            
            if (useSound != null && character.GetComponent<AudioSource>() != null)
            {
                character.GetComponent<AudioSource>().PlayOneShot(useSound);
            }
            
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification($"Used {itemName}");
            }
        }
    }
    
    protected virtual void ApplyInstant(JUCharacterController character)
    {
        JUHealth health = character.GetComponent<JUHealth>();
        
        if (health != null && healthRestore > 0f)
        {
            health.Health += healthRestore;
            health.Health = Mathf.Clamp(health.Health, 0f, health.MaxHealth);
        }
        
        if (GameManager.Instance != null && xpGrant > 0)
        {
            GameManager.Instance.progressionManager.AddExperience(xpGrant);
        }
    }
    
    protected virtual void ApplyOverTime(JUCharacterController character, float duration)
    {
        ConsumableEffectHandler handler = character.GetComponent<ConsumableEffectHandler>();
        
        if (handler == null)
        {
            handler = character.gameObject.AddComponent<ConsumableEffectHandler>();
        }
        
        handler.ApplyEffect(this, duration);
    }
}
