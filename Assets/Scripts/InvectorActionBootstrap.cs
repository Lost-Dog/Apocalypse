using UnityEngine;
using Invector;
using Invector.vCharacterController;

/// <summary>
/// Wires up all IActionListener components (vGenericAction, vLadderAction, etc.)
/// to vCharacter.onActionEnter/Stay/Exit events. Invector's LoadActionControllers
/// was removed from vCharacter.Init() in newer versions; this component restores
/// that wiring without modifying any Invector core files.
/// </summary>
[DefaultExecutionOrder(-50)]
public class InvectorActionBootstrap : MonoBehaviour
{
    [Tooltip("Log registered action listeners to the console for debugging.")]
    public bool debugMode = false;

    private void Start()
    {
        var character = GetComponentInChildren<vCharacter>();
        if (character == null)
        {
            if (debugMode)
                Debug.LogWarning("[InvectorActionBootstrap] No vCharacter found in children.", this);
            return;
        }

        character.LoadActionControllers(debugMode);

        if (debugMode)
            Debug.Log($"[InvectorActionBootstrap] LoadActionControllers called on '{character.name}'.", this);
    }
}
