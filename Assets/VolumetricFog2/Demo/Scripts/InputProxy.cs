using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace VolumetricFogAndMist2.Demos {

    static class InputProxy {

        public static bool GetKey(KeyCode keyCode) {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(keyCode);
#elif ENABLE_INPUT_SYSTEM
            return GetKeyInternal(keyCode, false);
#else
            return false;
#endif
        }

        public static bool GetKeyDown(KeyCode keyCode) {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#elif ENABLE_INPUT_SYSTEM
            return GetKeyInternal(keyCode, true);
#else
            return false;
#endif
        }

        public static float GetAxis(string axisName) {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxis(axisName);
#elif ENABLE_INPUT_SYSTEM
            if (axisName == HorizontalAxisName) {
                return GetDigitalAxis(horizontalNegativeKeys, horizontalPositiveKeys);
            }

            if (axisName == VerticalAxisName) {
                return GetDigitalAxis(verticalNegativeKeys, verticalPositiveKeys);
            }

            if (axisName == MouseXAxisName) {
                return GetMouseAxis(true);
            }

            if (axisName == MouseYAxisName) {
                return GetMouseAxis(false);
            }

            return 0f;
#else
            return 0f;
#endif
        }

        public static Vector3 MousePosition {
            get {
#if ENABLE_LEGACY_INPUT_MANAGER
                return Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM
                var mouse = Mouse.current;
                if (mouse == null) return Vector3.zero;
                var position = mouse.position.ReadValue();
                mousePositionCache.x = position.x;
                mousePositionCache.y = position.y;
                mousePositionCache.z = 0f;
                return mousePositionCache;
#else
                return Vector3.zero;
#endif
            }
        }

#if ENABLE_INPUT_SYSTEM
        const string HorizontalAxisName = "Horizontal";
        const string VerticalAxisName = "Vertical";
        const string MouseXAxisName = "Mouse X";
        const string MouseYAxisName = "Mouse Y";

        static readonly KeyCode[] horizontalNegativeKeys = { KeyCode.A, KeyCode.LeftArrow };
        static readonly KeyCode[] horizontalPositiveKeys = { KeyCode.D, KeyCode.RightArrow };
        static readonly KeyCode[] verticalNegativeKeys = { KeyCode.S, KeyCode.DownArrow };
        static readonly KeyCode[] verticalPositiveKeys = { KeyCode.W, KeyCode.UpArrow };

        static Vector3 mousePositionCache;
        static Vector2 mouseDelta;
        static int mouseDeltaFrame = -1;

        static float GetDigitalAxis(KeyCode[] negativeKeys, KeyCode[] positiveKeys) {
            var keyboard = Keyboard.current;
            if (keyboard == null) return 0f;

            float value = 0f;
            if (IsAnyKeyPressed(keyboard, negativeKeys)) value -= 1f;
            if (IsAnyKeyPressed(keyboard, positiveKeys)) value += 1f;
            return Mathf.Clamp(value, -1f, 1f);
        }

        static bool IsAnyKeyPressed(Keyboard keyboard, KeyCode[] keys) {
            for (int i = 0; i < keys.Length; i++) {
                var control = GetKeyControl(keyboard, keys[i]);
                if (control != null && control.isPressed) return true;
            }
            return false;
        }

        static float GetMouseAxis(bool horizontal) {
            var mouse = Mouse.current;
            if (mouse == null) return 0f;
            if (mouseDeltaFrame != Time.frameCount) {
                mouseDelta = mouse.delta.ReadValue();
                mouseDeltaFrame = Time.frameCount;
            }
            return horizontal ? mouseDelta.x : mouseDelta.y;
        }

        static bool GetKeyInternal(KeyCode keyCode, bool down) {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            var control = GetKeyControl(keyboard, keyCode);
            if (control == null) return false;
            return down ? control.wasPressedThisFrame : control.isPressed;
        }

        static KeyControl GetKeyControl(Keyboard keyboard, KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.LeftArrow: return keyboard.leftArrowKey;
                case KeyCode.RightArrow: return keyboard.rightArrowKey;
                case KeyCode.UpArrow: return keyboard.upArrowKey;
                case KeyCode.DownArrow: return keyboard.downArrowKey;
                case KeyCode.A: return keyboard.aKey;
                case KeyCode.D: return keyboard.dKey;
                case KeyCode.W: return keyboard.wKey;
                case KeyCode.S: return keyboard.sKey;
                case KeyCode.LeftShift: return keyboard.leftShiftKey;
                case KeyCode.Space: return keyboard.spaceKey;
                case KeyCode.Alpha1: return keyboard.digit1Key;
                case KeyCode.Alpha2: return keyboard.digit2Key;
                case KeyCode.F: return keyboard.fKey;
                case KeyCode.T: return keyboard.tKey;
                default:
                    return null;
            }
        }
#else
        static Vector3 mousePositionCache;
#endif
    }
}

