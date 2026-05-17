using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace CompassNavigatorProDemos {

    /// <summary>
    /// Simple input bridge so the demo works with both legacy and new input systems.
    /// </summary>
    public static class InputProxy {

        interface IInputImpl {
            void Init();
            void SetupEventSystem();
            bool GetMouseButton(int buttonIndex);
            bool GetKey(KeyCode keyCode);
            bool GetKeyDown(KeyCode keyCode);
            float GetAxis(string axisName);
        }

        static IInputImpl impl;

        static void EnsureInitialized() {
            if (impl != null) return;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            impl = new NewInputImpl();
#else
            impl = new LegacyInputImpl();
#endif
            impl.Init();
        }

        public static void SetupEventSystem() {
            EnsureInitialized();
            impl.SetupEventSystem();
        }

        public static bool GetMouseButton(int buttonIndex) {
            EnsureInitialized();
            return impl.GetMouseButton(buttonIndex);
        }

        public static bool GetKey(KeyCode keyCode) {
            EnsureInitialized();
            return impl.GetKey(keyCode);
        }

        public static bool GetKeyDown(KeyCode keyCode) {
            EnsureInitialized();
            return impl.GetKeyDown(keyCode);
        }

        public static float GetAxis(string axisName) {
            EnsureInitialized();
            return impl.GetAxis(axisName);
        }

        class LegacyInputImpl : IInputImpl {
            public void Init() { }
            public void SetupEventSystem() { }
            public bool GetMouseButton(int buttonIndex) => Input.GetMouseButton(buttonIndex);
            public bool GetKey(KeyCode keyCode) => Input.GetKey(keyCode);
            public bool GetKeyDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
            public float GetAxis(string axisName) => Input.GetAxis(axisName);
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        class NewInputImpl : IInputImpl {
            Mouse mouse;
            Keyboard keyboard;

            public void Init() {
                mouse = Mouse.current;
                keyboard = Keyboard.current;
                SetupEventSystem();
            }

            public void SetupEventSystem() {
                var eventSystem = EventSystem.current;
                if (eventSystem == null) return;

                var standalone = eventSystem.GetComponent<StandaloneInputModule>();
                if (standalone != null) {
                    Object.Destroy(standalone);
                }
                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }

            public bool GetMouseButton(int buttonIndex) {
                if (mouse == null) mouse = Mouse.current;
                if (mouse == null) return false;
                switch (buttonIndex) {
                    case 0: return mouse.leftButton.isPressed;
                    case 1: return mouse.rightButton.isPressed;
                    case 2: return mouse.middleButton.isPressed;
                    default: return false;
                }
            }

            public bool GetKey(KeyCode keyCode) {
                var key = GetKeyControl(keyCode);
                return key != null && key.isPressed;
            }

            public bool GetKeyDown(KeyCode keyCode) {
                var key = GetKeyControl(keyCode);
                return key != null && key.wasPressedThisFrame;
            }

            public float GetAxis(string axisName) {
                if (mouse == null) mouse = Mouse.current;
                if (keyboard == null) keyboard = Keyboard.current;
                switch (axisName) {
                    case "Mouse ScrollWheel":
                        return mouse != null ? mouse.scroll.ReadValue().y / 120f : 0f;
                    case "Mouse X":
                        return mouse != null ? mouse.delta.ReadValue().x : 0f;
                    case "Mouse Y":
                        return mouse != null ? mouse.delta.ReadValue().y : 0f;
                    case "Horizontal":
                        float h = 0f;
                        if (keyboard != null) {
                            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
                            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
                        }
                        return Mathf.Clamp(h, -1f, 1f);
                    case "Vertical":
                        float v = 0f;
                        if (keyboard != null) {
                            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
                            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
                        }
                        return Mathf.Clamp(v, -1f, 1f);
                    default:
                        return 0f;
                }
            }

            static UnityEngine.InputSystem.Controls.KeyControl GetKeyControl(KeyCode keyCode) {
                var kb = Keyboard.current;
                if (kb == null) return null;
                switch (keyCode) {
                    case KeyCode.W: return kb.wKey;
                    case KeyCode.A: return kb.aKey;
                    case KeyCode.S: return kb.sKey;
                    case KeyCode.D: return kb.dKey;
                    case KeyCode.Q: return kb.qKey;
                    case KeyCode.E: return kb.eKey;
                    case KeyCode.Z: return kb.zKey;
                    case KeyCode.X: return kb.xKey;
                    case KeyCode.C: return kb.cKey;
                    case KeyCode.M: return kb.mKey;
                    case KeyCode.V: return kb.vKey;
                    case KeyCode.T: return kb.tKey;
                    case KeyCode.H: return kb.hKey;
                    case KeyCode.F: return kb.fKey;
                    case KeyCode.B: return kb.bKey;
                    case KeyCode.LeftShift: return kb.leftShiftKey;
                    case KeyCode.RightShift: return kb.rightShiftKey;
                    case KeyCode.LeftControl: return kb.leftCtrlKey;
                    case KeyCode.RightControl: return kb.rightCtrlKey;
                    default: return null;
                }
            }
        }
#endif
    }
}
