using UnityEngine;

public class PauseInputHandler : MonoBehaviour
{
    [Header("Keyboard")]
    [SerializeField] private KeyCode keyboardPauseKey = KeyCode.Escape;

    [Header("Gamepad Fallback Keys")]
    [SerializeField] private KeyCode[] gamepadPauseKeys =
    {
        KeyCode.JoystickButton7, // Start/Options on many controllers
        KeyCode.JoystickButton9  // Menu on some mappings
    };

    [Header("Behavior")]
    [SerializeField] private bool ignoreWhileLoading = true;
    [SerializeField] private bool requireFocus = true;

    private SceneFlowManager _sceneFlow;

    private void Start()
    {
        _sceneFlow = SceneFlowManager.Instance;
        if (_sceneFlow == null)
        {
            Debug.LogWarning("[PauseInputHandler] SceneFlowManager not found. Input handler will stay idle.");
        }
    }

    private void Update()
    {
        if (!IsAllowedToProcessInput()) return;
        if (!WasPausePressedThisFrame()) return;
        if (_sceneFlow == null) return;

        _sceneFlow.TogglePause();
    }

    private bool IsAllowedToProcessInput()
    {
        if (requireFocus && !Application.isFocused) return false;

        if (_sceneFlow == null)
        {
            _sceneFlow = SceneFlowManager.Instance;
            if (_sceneFlow == null) return false;
        }

        var state = _sceneFlow.CurrentState;

        if (ignoreWhileLoading &&
            (state == SceneFlowManager.FlowState.Loading ||
             state == SceneFlowManager.FlowState.Transitioning))
        {
            return false;
        }

        return state == SceneFlowManager.FlowState.Playing ||
               state == SceneFlowManager.FlowState.Paused;
    }

    private bool WasPausePressedThisFrame()
    {
        if (Input.GetKeyDown(keyboardPauseKey)) return true;

        if (gamepadPauseKeys == null || gamepadPauseKeys.Length == 0) return false;

        for (int i = 0; i < gamepadPauseKeys.Length; i++)
        {
            if (Input.GetKeyDown(gamepadPauseKeys[i]))
                return true;
        }

        return false;
    }
}
