using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prevents MissingMethodException spam caused by PlayerInput (SendMessages mode) broadcasting
/// action messages (e.g. OnJump) to all components on the GameObject, including GC2's
/// Character.OnJump(float). Unity's InvokeMember finds Character.OnJump by name, then fails
/// to bind the InputValue argument to a float parameter and throws.
///
/// Fix: switch PlayerInput to InvokeCSharpEvents at Awake. This disables all SendMessage
/// broadcasting entirely. ABC reads actions directly from PlayerInput.actions via
/// ABC_InputManager (FindAction / ReadValue) so its input is unaffected by this change.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class GC2InputMessageStub : MonoBehaviour
{
    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.notificationBehavior != PlayerNotifications.InvokeCSharpEvents)
        {
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }
    }
}
