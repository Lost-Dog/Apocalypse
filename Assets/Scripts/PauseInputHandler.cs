using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputHandler : MonoBehaviour
{
    private const string FallbackPauseInputActionsJson = "{\n    \"name\": \"PauseInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Gameplay\",\n            \"id\": \"c0000000-0000-0000-0001-000000000000\",\n            \"actions\": [\n                { \"name\": \"Pause\", \"type\": \"Button\", \"id\": \"c0000000-0000-0000-0002-000000000000\", \"expectedControlType\": \"Button\", \"processors\": \"\", \"interactions\": \"\", \"initialStateCheck\": false }\n            ],\n            \"bindings\": [\n                { \"name\": \"\", \"id\": \"d0000000-0000-0000-0001-000000000000\", \"path\": \"<Keyboard>/escape\", \"interactions\": \"\", \"processors\": \"\", \"groups\": \"\", \"action\": \"Pause\", \"isComposite\": false, \"isPartOfComposite\": false },\n                { \"name\": \"\", \"id\": \"d0000000-0000-0000-0002-000000000000\", \"path\": \"<Gamepad>/start\", \"interactions\": \"\", \"processors\": \"\", \"groups\": \"\", \"action\": \"Pause\", \"isComposite\": false, \"isPartOfComposite\": false },\n                { \"name\": \"\", \"id\": \"d0000000-0000-0000-0003-000000000000\", \"path\": \"<Gamepad>/select\", \"interactions\": \"\", \"processors\": \"\", \"groups\": \"\", \"action\": \"Pause\", \"isComposite\": false, \"isPartOfComposite\": false }\n            ]\n        }\n    ],\n    \"controlSchemes\": []\n}";

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private string pauseActionName = "Pause";

    [Header("Behavior")]
    [SerializeField] private bool ignoreWhileLoading = true;
    [SerializeField] private bool requireFocus = true;

    private InputActionAsset _runtimeActions;
    private InputAction _pauseAction;
    private SceneFlowManager _sceneFlow;

    private void Awake()
    {
        _runtimeActions = inputActionAsset != null
            ? Instantiate(inputActionAsset)
            : InputActionAsset.FromJson(FallbackPauseInputActionsJson);

        if (_runtimeActions == null)
        {
            Debug.LogError("[PauseInputHandler] Could not create pause input actions.", this);
            return;
        }

        _pauseAction = _runtimeActions.FindAction(pauseActionName, throwIfNotFound: false);
        if (_pauseAction == null)
        {
            if (inputActionAsset != null)
            {
                Debug.LogWarning($"[PauseInputHandler] Could not find action '{pauseActionName}' in the assigned InputActionAsset. Falling back to the built-in pause actions.", this);
                Destroy(_runtimeActions);
                _runtimeActions = InputActionAsset.FromJson(FallbackPauseInputActionsJson);
                _pauseAction = _runtimeActions != null ? _runtimeActions.FindAction(pauseActionName, throwIfNotFound: false) : null;
            }

            if (_pauseAction == null)
            {
                Debug.LogError($"[PauseInputHandler] Could not find action '{pauseActionName}' in the pause input actions.", this);
            }
        }
    }

    private void Start()
    {
        _sceneFlow = SceneFlowManager.Instance;
        if (_sceneFlow == null)
        {
            Debug.LogWarning("[PauseInputHandler] SceneFlowManager not found. Input handler will stay idle.");
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed += OnPauseActionPerformed;
            _pauseAction.Enable();
        }
    }

    private void OnDestroy()
    {
        if (_pauseAction != null)
        {
            _pauseAction.performed -= OnPauseActionPerformed;
            _pauseAction.Disable();
        }

        if (_runtimeActions != null)
        {
            Destroy(_runtimeActions);
            _runtimeActions = null;
            _pauseAction = null;
        }
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

    private void OnPauseActionPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (!IsAllowedToProcessInput())
        {
            return;
        }

        if (_sceneFlow == null)
        {
            _sceneFlow = SceneFlowManager.Instance;
        }

        if (_sceneFlow != null)
        {
            _sceneFlow.TogglePause();
        }
    }
}
