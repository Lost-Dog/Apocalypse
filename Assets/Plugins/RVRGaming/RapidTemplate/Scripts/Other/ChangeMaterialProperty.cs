using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChangeMaterialProperty : MonoBehaviour
{
    [Header("Property Settings Float")]
    public string propertyName = "_MyFloatProperty";
    public float increaseValue = 1.0f;
    public float decreaseValue = 0.0f;
    public float duration = 1.0f;

    [Header("Property Settings Color")]
    public string colorPropertyName = "_Color";
    [ColorUsage(true, true)]
    public Color targetColor = Color.white;
    public float colorDuration = 1.0f;

    [Header("Easing Settings")]
    public AnimationCurve increaseEasingCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve decreaseEasingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public void Increase()
    {
        ChangeProperty(increaseValue, duration, increaseEasingCurve);
    }

    public void Decrease()
    {
        ChangeProperty(decreaseValue, duration, decreaseEasingCurve);
    }

    public void ChangeHDRColor(Color newHDRColor)
    {
        StartCoroutine(ChangeHDRColorCoroutine(newHDRColor, colorDuration));
    }

    public void ChangeProperty(float value, float duration, AnimationCurve easingCurve)
    {
        StartCoroutine(ChangePropertyCoroutine(value, duration, easingCurve));
    }

    private IEnumerator ChangePropertyCoroutine(float value, float duration, AnimationCurve easingCurve)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsedTime = 0;

        var initialValues = new Dictionary<Material, float>();

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty(propertyName))
                {
                    initialValues[material] = material.GetFloat(propertyName);
                }
            }
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float easedT = easingCurve.Evaluate(t);

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty(propertyName))
                    {
                        float initialValue = initialValues[material];
                        float newValue = Mathf.Lerp(initialValue, value, easedT);
                        material.SetFloat(propertyName, newValue);
                    }
                }
            }

            yield return null;
        }

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty(propertyName))
                {
                    material.SetFloat(propertyName, value);
                }
            }
        }
    }

    private IEnumerator ChangeHDRColorCoroutine(Color newHDRColor, float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsedTime = 0;

        var initialColors = new Dictionary<Material, Color>();

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty(colorPropertyName))
                {
                    initialColors[material] = material.GetColor(colorPropertyName);
                }
            }
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty(colorPropertyName))
                    {
                        Color initialColor = initialColors[material];
                        Color newLerpColor = Color.Lerp(initialColor, newHDRColor, t);
                        material.SetColor(colorPropertyName, newLerpColor);
                    }
                }
            }

            yield return null;
        }

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty(colorPropertyName))
                {
                    material.SetColor(colorPropertyName, newHDRColor);
                }
            }
        }
    }
}
