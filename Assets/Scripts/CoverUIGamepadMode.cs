using UnityEngine;

/// <summary>
/// Forces the Cover UI prefab to display gamepad prompts and hide keyboard prompts.
/// Attach to the root of any Cover UI prefab (Cover UI Prefab, CoverUI, etc.).
/// On Awake it recursively scans the hierarchy for GameObjects named "Keyboard"
/// or "Joystick" (case-insensitive) and sets their active state accordingly.
/// </summary>
public class CoverUIGamepadMode : MonoBehaviour
{
    private const string KeyboardNodeName = "keyboard";
    private const string JoystickNodeName = "joystick";

    private void Awake()
    {
        ApplyGamepadMode(transform);
    }

    /// <summary>Recursively walks the hierarchy and toggles input-icon nodes.</summary>
    private void ApplyGamepadMode(Transform root)
    {
        foreach (Transform child in root)
        {
            string lowerName = child.gameObject.name.ToLowerInvariant();

            if (lowerName.Contains(KeyboardNodeName))
            {
                child.gameObject.SetActive(false);
            }
            else if (lowerName.Contains(JoystickNodeName))
            {
                child.gameObject.SetActive(true);
            }

            // Always recurse so nested prompts are also covered.
            ApplyGamepadMode(child);
        }
    }
}
