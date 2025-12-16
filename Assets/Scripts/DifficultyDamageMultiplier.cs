using UnityEngine;

/// <summary>
/// Component added to challenge enemies to apply difficulty-based damage scaling.
/// Your damage system can check for this component and apply the multiplier.
/// </summary>
public class DifficultyDamageMultiplier : MonoBehaviour
{
    [Tooltip("Damage multiplier based on challenge difficulty")]
    public float multiplier = 1.0f;
    
    /// <summary>
    /// Get the scaled damage value
    /// </summary>
    public float GetScaledDamage(float baseDamage)
    {
        return baseDamage * multiplier;
    }
    
    /// <summary>
    /// Check if an object has difficulty scaling and return the multiplier
    /// </summary>
    public static float GetMultiplier(GameObject obj)
    {
        if (obj == null)
            return 1.0f;
        
        DifficultyDamageMultiplier component = obj.GetComponent<DifficultyDamageMultiplier>();
        return component != null ? component.multiplier : 1.0f;
    }
}
