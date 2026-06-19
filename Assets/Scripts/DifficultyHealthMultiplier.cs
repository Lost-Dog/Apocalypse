using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Component added to challenge enemies to apply difficulty-based health scaling.
/// It also attempts to write multiplier values into common health field/property names.
/// </summary>
public class DifficultyHealthMultiplier : MonoBehaviour
{
    [Tooltip("Health multiplier based on challenge difficulty")]
    public float multiplier = 1.0f;

    public float GetScaledHealth(float baseHealth)
    {
        return baseHealth * multiplier;
    }

    public static float GetMultiplier(GameObject obj)
    {
        if (obj == null)
            return 1.0f;

        DifficultyHealthMultiplier component = obj.GetComponent<DifficultyHealthMultiplier>();
        return component != null ? component.multiplier : 1.0f;
    }

    public void TryApplyToCommonHealthFields(GameObject root)
    {
        if (root == null) return;
        if (Mathf.Approximately(multiplier, 1f)) return;

        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null) continue;

            Type type = behaviour.GetType();

            bool maxScaled =
                TryScaleMember(behaviour, type, "maxHealth") |
                TryScaleMember(behaviour, type, "MaxHealth") |
                TryScaleMember(behaviour, type, "healthMax") |
                TryScaleMember(behaviour, type, "startingHealth") |
                TryScaleMember(behaviour, type, "baseHealth");

            if (maxScaled)
            {
                TryScaleMember(behaviour, type, "currentHealth");
                TryScaleMember(behaviour, type, "CurrentHealth");
                TryScaleMember(behaviour, type, "health");
                TryScaleMember(behaviour, type, "Health");
            }
        }
    }

    private bool TryScaleMember(object target, Type targetType, string memberName)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo field = targetType.GetField(memberName, flags);
        if (field != null)
        {
            if (TryScaleValue(field.FieldType, field.GetValue(target), out object scaled))
            {
                field.SetValue(target, scaled);
                return true;
            }
        }

        PropertyInfo property = targetType.GetProperty(memberName, flags);
        if (property != null && property.CanRead && property.CanWrite)
        {
            if (TryScaleValue(property.PropertyType, property.GetValue(target), out object scaled))
            {
                property.SetValue(target, scaled);
                return true;
            }
        }

        return false;
    }

    private bool TryScaleValue(Type numericType, object originalValue, out object scaledValue)
    {
        scaledValue = null;
        if (originalValue == null) return false;

        if (numericType == typeof(float))
        {
            float current = (float)originalValue;
            scaledValue = current * multiplier;
            return true;
        }

        if (numericType == typeof(double))
        {
            double current = (double)originalValue;
            scaledValue = current * multiplier;
            return true;
        }

        if (numericType == typeof(int))
        {
            int current = (int)originalValue;
            scaledValue = Mathf.RoundToInt(current * multiplier);
            return true;
        }

        return false;
    }
}
