using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.UI.Shift
{
    public class PressKeyEvent : MonoBehaviour
    {
        [Header("Key")]
        public KeyCode hotkey;
        public bool pressAnyKey;
        public bool invokeAtStart;

        [Header("Action")]
        public UnityEvent pressAction;

        void Start()
        {
            if (invokeAtStart)
                pressAction?.Invoke();
        }

        void Update()
        {
            if (WasPressedThisFrame())
            {
                pressAction?.Invoke();
            }
        }

        private bool WasPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (pressAnyKey)
            {
                return keyboard.anyKey.wasPressedThisFrame;
            }

            return TryGetInputSystemKey(hotkey, out Key key) && keyboard[key].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (pressAnyKey)
            {
                return Input.anyKeyDown;
            }

            return Input.GetKeyDown(hotkey);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static bool TryGetInputSystemKey(KeyCode keyCode, out Key key)
        {
            switch (keyCode)
            {
                case KeyCode.Alpha0: key = Key.Digit0; return true;
                case KeyCode.Alpha1: key = Key.Digit1; return true;
                case KeyCode.Alpha2: key = Key.Digit2; return true;
                case KeyCode.Alpha3: key = Key.Digit3; return true;
                case KeyCode.Alpha4: key = Key.Digit4; return true;
                case KeyCode.Alpha5: key = Key.Digit5; return true;
                case KeyCode.Alpha6: key = Key.Digit6; return true;
                case KeyCode.Alpha7: key = Key.Digit7; return true;
                case KeyCode.Alpha8: key = Key.Digit8; return true;
                case KeyCode.Alpha9: key = Key.Digit9; return true;
                case KeyCode.Keypad0: key = Key.Numpad0; return true;
                case KeyCode.Keypad1: key = Key.Numpad1; return true;
                case KeyCode.Keypad2: key = Key.Numpad2; return true;
                case KeyCode.Keypad3: key = Key.Numpad3; return true;
                case KeyCode.Keypad4: key = Key.Numpad4; return true;
                case KeyCode.Keypad5: key = Key.Numpad5; return true;
                case KeyCode.Keypad6: key = Key.Numpad6; return true;
                case KeyCode.Keypad7: key = Key.Numpad7; return true;
                case KeyCode.Keypad8: key = Key.Numpad8; return true;
                case KeyCode.Keypad9: key = Key.Numpad9; return true;
                case KeyCode.KeypadEnter: key = Key.NumpadEnter; return true;
                case KeyCode.Return: key = Key.Enter; return true;
                case KeyCode.Escape: key = Key.Escape; return true;
                case KeyCode.Space: key = Key.Space; return true;
                case KeyCode.Backspace: key = Key.Backspace; return true;
                case KeyCode.Tab: key = Key.Tab; return true;
                case KeyCode.Delete: key = Key.Delete; return true;
                case KeyCode.UpArrow: key = Key.UpArrow; return true;
                case KeyCode.DownArrow: key = Key.DownArrow; return true;
                case KeyCode.LeftArrow: key = Key.LeftArrow; return true;
                case KeyCode.RightArrow: key = Key.RightArrow; return true;
                case KeyCode.LeftShift: key = Key.LeftShift; return true;
                case KeyCode.RightShift: key = Key.RightShift; return true;
                case KeyCode.LeftControl: key = Key.LeftCtrl; return true;
                case KeyCode.RightControl: key = Key.RightCtrl; return true;
                case KeyCode.LeftAlt: key = Key.LeftAlt; return true;
                case KeyCode.RightAlt: key = Key.RightAlt; return true;
                default:
                    return System.Enum.TryParse(keyCode.ToString(), ignoreCase: true, out key);
            }
        }
#endif
    }
}