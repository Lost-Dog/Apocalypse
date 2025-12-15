using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace JUTPS.JUInputSystem
{
    /// <summary>
    /// Input Manager.
    /// </summary>
    public class JUInputManager : MonoBehaviour
    {
        /// <summary>
        /// The input types, like keyboard, gamepad, touch...
        /// </summary>
        public enum InputTypes
        {
            /// <summary>
            /// Keyboard or mouse input.
            /// </summary>
            KeyboardOrMouse,

            /// <summary>
            /// Xbox gamepad input.
            /// </summary>
            Xbox,

            /// <summary>
            /// Playstation gamepad input.
            /// </summary>
            Playstation,

            /// <summary>
            /// Nintendo gamepad input.
            /// </summary>
            Nintendo,

            /// <summary>
            /// Touch input.
            /// </summary>
            Touch,
        }

        private static float _lastTouchPressTime;

        private static JUInputManager _instance;
        private static InputTypes _currentInputType;

        /// <summary>
        /// Called if the player switch the input (like switch keyboard to gamepad).
        /// </summary>
        public static UnityAction<InputTypes> OnChangeInputType;

        /// <summary>
        /// Return true if is using gamepad.
        /// </summary>
        public static bool IsUsingGamepad
        {
            get
            {
                CreateInstanceIfNull();
                return IsGamepadInput(_currentInputType);
            }
        }

        /// <summary>
        /// The current input type.
        /// </summary>
        public static InputTypes CurrentInputType
        {
            get
            {
                CreateInstanceIfNull();
                return _currentInputType;
            }
        }

        private JUInputManager()
        {
        }

        private void Awake()
        {
            InputSystem.onActionChange += OnActionChange;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDestroy()
        {
            InputSystem.onActionChange -= OnActionChange;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionStarted && change != InputActionChange.ActionPerformed)
                return;

            if (!(obj is InputAction action))
                return;

            InputTypes inputType = GetInputType(action.activeControl.device);

            if (inputType == InputTypes.Touch)
            {
                _lastTouchPressTime = Time.time;
            }

            // Was pressing on the last second.
            bool isUsingTouch = (Time.time - _lastTouchPressTime) < 1f;

            // UI elements like buttons can override gamepad inputs, so to avoid
            // override the last touch press this will ignore the action. 
            if (inputType != InputTypes.Touch && isUsingTouch)
            {
                return;
            }

            if (inputType != _currentInputType)
            {
                _currentInputType = inputType;
                OnChangeInputType?.Invoke(_currentInputType);
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!gameObject.scene.isLoaded)
                return;

            bool wasAdded = change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected;
            bool wasRemoved = change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected;
            InputTypes inputType = GetInputType(device);

            if (wasAdded && inputType != _currentInputType)
            {
                _currentInputType = inputType;
                OnChangeInputType?.Invoke(_currentInputType);
            }

            if (wasRemoved && IsGamepadInput(inputType))
            {
                _currentInputType = InputTypes.KeyboardOrMouse;
                OnChangeInputType?.Invoke(_currentInputType);
            }
        }

        private static InputTypes GetInputType(InputDevice device)
        {
            if (device is DualShockGamepad)
                return InputTypes.Playstation;

#if UNITY_SWITCH
            if (device is UnityEngine.InputSystem.Switch.SwitchProControllerHID)
                return InputTypes.Nintendo;
#endif

            if (device is Gamepad)
                return InputTypes.Xbox;

            if (device is Pointer)
                return InputTypes.Touch;

            return InputTypes.KeyboardOrMouse;
        }

        private static void CreateInstanceIfNull()
        {
            if (_instance)
            {
                return;
            }

            _instance = new GameObject("[ Input Manager ]").AddComponent<JUInputManager>();
            _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(_instance.gameObject);

            InputDevice currentDevice = Keyboard.current;
            if (Touchscreen.current != null)
            {
                currentDevice = Touchscreen.current;
            }
            else if (Gamepad.current != null)
            {
                currentDevice = Gamepad.current;
            }

            _currentInputType = GetInputType(currentDevice);
        }

        private static bool IsGamepadInput(InputTypes type)
        {
            switch (type)
            {
                case InputTypes.Xbox:
                case InputTypes.Playstation:
                case InputTypes.Nintendo:
                    return true;
                default:
                    return false;
            }
        }
    }
}
