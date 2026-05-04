#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

using System;
using System.Linq;
using UnityEngine;

namespace ABCToolkit {

    /// <summary>
    /// ABC Input Manager which will integrate with both new and old Unity input systems.
    /// </summary>
    [System.Serializable]
    public class ABC_InputManager {

        /// <summary>
        /// Instantiates New InputManager
        /// </summary>
        public ABC_InputManager() {
            //Legacy Input
        }

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// Instantiates New InputManager
        /// </summary>
        /// <param name="playerInput">player input for the entity</param>
        public ABC_InputManager(PlayerInput playerInput) {

            //set player input
            _playerInput = playerInput;

        }



        /// <summary>
        /// Stores player input component
        /// </summary>
        private PlayerInput _playerInput; 

        /// <summary>
        /// Get/Set Method to retrieve and return player input component
        /// </summary>
        private PlayerInput playerInput {

            get {

                if (_playerInput == null) {

                    //try and find input system
                    PlayerInput inputSystem = UnityEngine.Object.FindObjectsOfType<PlayerInput>().FirstOrDefault();

                    //If found set variable and return
                    if (inputSystem != null)
                        _playerInput = inputSystem;
                    else
                        Debug.LogWarning("Player Input enabled and not found. Please add one to the scene manually or through the ABC character creation.");


                    //If using gamepad then find virtual mouse
                    if (this._virtualMouse == null && InputSystem.devices.Where(d => d is Gamepad).Count() > 0) {

                        if (UnityEngine.Object.FindObjectsOfType<ABC_VirtualMouse>().Count() > 0) {
                            this._virtualMouse = UnityEngine.Object.FindObjectsOfType<ABC_VirtualMouse>().First();
                        } else {
                            Debug.Log("Gamepad Detected, loading Virtual Mouse.");
                            this._virtualMouse = UnityEngine.Object.Instantiate(Resources.Load("ABC-InputActions/VirtualMouse/VirtualMouse") as GameObject).GetComponentInChildren<ABC_VirtualMouse>();
                        }

                    }

                }
              

                return _playerInput; 

            }

    }

        /// <summary>
        /// Virtual mouse used if a controller exists
        /// </summary>
        private ABC_VirtualMouse _virtualMouse;

        /// <summary>
        /// Virtual mouse used if a controller exists
        /// </summary>
        private ABC_VirtualMouse virtualMouse {

            get {

                //If using gamepad then find virtual mouse
                if (this.virtualMouseChecked == false && this._virtualMouse == null && InputSystem.devices.Where(d => d is Gamepad).Count() > 0) {

                    if (UnityEngine.Object.FindObjectsOfType<ABC_VirtualMouse>().Count() > 0) {
                        this._virtualMouse = UnityEngine.Object.FindObjectsOfType<ABC_VirtualMouse>().First();
                    } else {
                        Debug.Log("Gamepad Detected, loading Virtual Mouse.");
                        this._virtualMouse = UnityEngine.Object.Instantiate(Resources.Load("ABC-InputActions/VirtualMouse/VirtualMouse") as GameObject).GetComponentInChildren<ABC_VirtualMouse>();
                    }

                } 

                //Mark that we already checked for virtual mouse
                virtualMouseChecked = true;


                return _virtualMouse;
            }
        }

        /// <summary>
        /// Will track if a virtual mouse has been checked for
        /// </summary>
        private bool virtualMouseChecked = false;

#endif
        /// <summary>
        /// Will reset mouse position to center screen
        /// </summary>
        public void ResetMousePosition() {

#if ENABLE_INPUT_SYSTEM

            Mouse.current?.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 1.7f));

            //If we have a virtual mouse then check for that position first
            if (this.virtualMouse != null)
                this.virtualMouse.ResetMousePosition();

#endif


        }

        /// <summary>
        /// Will return the current mouse position, using both new and old Unity Input Systems
        /// </summary>
        /// <returns>Vector3 of current mouse position</returns>
        public Vector3 GetMousePosition() {
#if ENABLE_INPUT_SYSTEM

            //If we have a virtual mouse then check for that position first
            if (this.virtualMouse != null)
                return this.virtualMouse.GetVirtualMousePosition();


            return Mouse.current.position.ReadValue();

#else
            return Input.mousePosition;
#endif
        }


        /// <summary>
        /// Will return the X value of the axis provided
        /// </summary>
        /// <param name="Axis">Axis to return the X value of</param>
        /// <returns>value of the Axis provided</returns>
        public float GetXAxis(string Axis) {
#if ENABLE_INPUT_SYSTEM

            if (IsButtonSupported(Axis) == true)
            return playerInput.actions.FindAction(Axis).ReadValue<Vector2>().x;

       switch (Axis) {

            case "X":
            case "Mouse X":

                if (Mouse.current == null)
                    return 0f;

                return Mouse.current.delta.ReadValue().x;
            case "RightStickHorizontal":        
            
                if (Gamepad.current == null)
                    return 0f;

                return Gamepad.current.rightStick.ReadValue().x;
            case "Horizontal":

                if (IsButtonSupported("Move") == false)
                    return 0;

                return playerInput.actions.FindAction("Move").ReadValue<Vector2>().x;

            default:
                return 0f;

        }

#else
            return Input.GetAxis(Axis);
#endif
        }

        /// <summary>
        /// Will return the value of the axis provided
        /// </summary>
        /// <param name="Axis">Axis to return the value of</param>
        /// <returns>value of the Axis provided</returns>
        public float GetYAxis(string Axis) {
#if ENABLE_INPUT_SYSTEM

       if (IsButtonSupported(Axis) == true)
            return playerInput.actions.FindAction(Axis).ReadValue<Vector2>().y;

       switch (Axis) {

            case "Mouse ScrollWheel":
                return Mouse.current.scroll.ReadValue().y;
            case "Y":
            case "Mouse Y":

                if (Mouse.current == null)
                    return 0f;

                return Mouse.current.delta.ReadValue().y;
            case "RightStickVertical":            
                
                if (Gamepad.current == null)
                    return 0f;

                return Gamepad.current.rightStick.ReadValue().y;
            case "Vertical":

                if (IsButtonSupported("Move") == false)
                    return 0;

                return playerInput.actions.FindAction("Move").ReadValue<Vector2>().y;

            default:
                return 0f;

        }

#else
            return Input.GetAxis(Axis);
#endif
        }


        /// <summary>
        /// Returns a bool indicating if a Button is supported
        /// </summary>
        /// <param name="Button">Button to check if supported</param>
        /// <returns>True if Button is supported, else false</returns>
        public bool IsButtonSupported(string Button) {
#if ENABLE_INPUT_SYSTEM

        try {
            InputAction button = playerInput.actions.FindAction(Button);

            if (button == null)
                return false; 

            return true; 
        } catch {

            return false; 
        }

#else
            return true;
#endif
        }


        /// <summary>
        /// Will return if the button provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Button">Button to check if triggered or not</param>
        /// <returns>True if button pressed, else false</returns>
        public bool GetButton(string Button) {
#if ENABLE_INPUT_SYSTEM


        if (IsButtonSupported(Button) == false)
            return false; 

        return playerInput.actions.FindAction(Button).ReadValue<float>() != 0;

#else
            return Input.GetButton(Button);
#endif
        }


        /// <summary>
        /// Will return if the button provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Button">Button to check if triggered or not</param>
        /// <returns>True if button pressed, else false</returns>
        public bool GetButtonDown(string Button) {
#if ENABLE_INPUT_SYSTEM


        if (IsButtonSupported(Button) == false)
            return false; 

        return playerInput.actions.FindAction(Button).triggered;

#else
            return Input.GetButtonDown(Button);
#endif
        }


        /// <summary>
        /// Will return if the button provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Button">Button to check if triggered or not</param>
        /// <returns>True if button pressed, else false</returns>
        public bool GetButtonUp(string Button) {
#if ENABLE_INPUT_SYSTEM


        if (IsButtonSupported(Button) == false)
            return false; 

        return playerInput.actions.FindAction(Button).triggered;

#else
            return Input.GetButtonUp(Button);
#endif
        }



        /// <summary>
        /// Returns a bool indicating if a key is supported
        /// </summary>
        /// <param name="key">Key to check if supported</param>
        /// <returns>True if key is supported, else false</returns>
        private bool IsKeySupported(KeyCode key) {
#if ENABLE_INPUT_SYSTEM
            return key != KeyCode.None
                   && Keyboard.current != null
                   && Keyboard.current.allKeys.Any(k => k.keyCode == this.ToKey(key));
#else
            return true;
#endif
        }

        /// <summary>
        /// Will return if the key(code) provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Key">Key to check if triggered or not</param>
        /// <returns>True if Key pressed, else false</returns>
        public bool GetKey(KeyCode key) {
#if ENABLE_INPUT_SYSTEM

        switch (key) {
            case KeyCode.Mouse0:
                return Mouse.current.leftButton.isPressed;
            case KeyCode.Mouse1:
                return Mouse.current.rightButton.isPressed;
            case KeyCode.Mouse2:
                return Mouse.current.middleButton.isPressed;
        }

         return IsKeySupported(key)
                && Keyboard.current[this.ToKey(key)].isPressed;

#else
            return Input.GetKey(key);
#endif
        }

        /// <summary>
        /// Will return if the key(code) provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Key">Key to check if triggered or not</param>
        /// <returns>True if Key pressed, else false</returns>
        public bool GetKeyDown(KeyCode key) {
#if ENABLE_INPUT_SYSTEM

        switch (key) {
            case KeyCode.Mouse0:
                return Mouse.current.leftButton.wasPressedThisFrame;
            case KeyCode.Mouse1:
                return Mouse.current.rightButton.wasPressedThisFrame;
            case KeyCode.Mouse2:
                return Mouse.current.middleButton.wasPressedThisFrame;
        }

        return IsKeySupported(key)
                   && Keyboard.current[this.ToKey(key)].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        /// <summary>
        /// Will return if the key(code) provided is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <param name="Key">Key to check if triggered or not</param>
        /// <returns>True if Key pressed, else false</returns>
        public bool GetKeyUp(KeyCode key) {
#if ENABLE_INPUT_SYSTEM

        switch (key) {
            case KeyCode.Mouse0:
                return Mouse.current.leftButton.wasReleasedThisFrame;
            case KeyCode.Mouse1:
                return Mouse.current.rightButton.wasReleasedThisFrame;
            case KeyCode.Mouse2:
                return Mouse.current.middleButton.wasReleasedThisFrame;
        }

        return IsKeySupported(key)
                && Keyboard.current[this.ToKey(key)].wasReleasedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        /// <summary>
        /// Determines if the shoulder/trigger buttons on a gamepad is currently being pressed
        /// </summary>
        /// <returns>True if any are pressed, else false</returns>
        public bool ShoulderTriggersPressed() {

#if ENABLE_INPUT_SYSTEM

            //If no player input then return false
            if (this.playerInput == null)
                return false;


            if (Gamepad.current == null)
                return false;
            

            if (Gamepad.current.leftTrigger.isPressed == true || Gamepad.current.rightTrigger.isPressed == true || Gamepad.current.leftShoulder.isPressed == true || Gamepad.current.rightShoulder.isPressed == true)
                return true;
            else
                return false;


#else
            return false;
#endif

        }

        /// <summary>
        /// Will return if any key(code) is being pressed, checking both new and old Unity Input Systems
        /// </summary>
        /// <returns>True if Key pressed, else false</returns>
        public bool AnyKey() {
#if ENABLE_INPUT_SYSTEM

            //Check keyboard first
            if (Keyboard.current.anyKey.wasPressedThisFrame == true) 
                return true;

            //If no player input then return false
            if (this.playerInput == null)
                return false; 

            //If not check for other devices
            foreach (InputAction input in this.playerInput.actions) {

            //If input is not pressed, contains UI or any looking mechanics then this would not be classed as a button press
            if (input.IsPressed() == false || input.ToString().Contains("UI") || input.ToString().Contains("Mouse/delta") || input.ToString().Contains("rightStick"))
                continue;

                //If we got this far then the right button was pressed so return true
                return true; 

            }

            //If we got this far then nothing been pressed so return false
            return false;                   


#else
            return Input.anyKey;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private Key ToKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.None: return Key.None;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.BackQuote: return Key.Backquote;
                case KeyCode.Quote: return Key.Quote;
                case KeyCode.Semicolon: return Key.Semicolon;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Backslash: return Key.Backslash;
                case KeyCode.LeftBracket: return Key.LeftBracket;
                case KeyCode.RightBracket: return Key.RightBracket;
                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Equals: return Key.Equals;
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.AltGr: return Key.AltGr;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftWindows: return Key.LeftWindows;
                case KeyCode.RightWindows: return Key.RightWindows;
                case KeyCode.LeftCommand: return Key.LeftCommand;
                case KeyCode.RightCommand: return Key.RightCommand;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.PageDown: return Key.PageDown;
                case KeyCode.PageUp: return Key.PageUp;
                case KeyCode.Home: return Key.Home;
                case KeyCode.End: return Key.End;
                case KeyCode.Insert: return Key.Insert;
                case KeyCode.Delete: return Key.Delete;
                case KeyCode.CapsLock: return Key.CapsLock;
                case KeyCode.Numlock: return Key.NumLock;
                case KeyCode.Print: return Key.PrintScreen;
                case KeyCode.ScrollLock: return Key.ScrollLock;
                case KeyCode.Pause: return Key.Pause;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.KeypadDivide: return Key.NumpadDivide;
                case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
                case KeyCode.KeypadEquals: return Key.NumpadEquals;
                case KeyCode.Keypad0: return Key.Numpad0;
                case KeyCode.Keypad1: return Key.Numpad1;
                case KeyCode.Keypad2: return Key.Numpad2;
                case KeyCode.Keypad3: return Key.Numpad3;
                case KeyCode.Keypad4: return Key.Numpad4;
                case KeyCode.Keypad5: return Key.Numpad5;
                case KeyCode.Keypad6: return Key.Numpad6;
                case KeyCode.Keypad7: return Key.Numpad7;
                case KeyCode.Keypad8: return Key.Numpad8;
                case KeyCode.Keypad9: return Key.Numpad9;
                case KeyCode.F1: return Key.F1;
                case KeyCode.F2: return Key.F2;
                case KeyCode.F3: return Key.F3;
                case KeyCode.F4: return Key.F4;
                case KeyCode.F5: return Key.F5;
                case KeyCode.F6: return Key.F6;
                case KeyCode.F7: return Key.F7;
                case KeyCode.F8: return Key.F8;
                case KeyCode.F9: return Key.F9;
                case KeyCode.F10: return Key.F10;
                case KeyCode.F11: return Key.F11;
                case KeyCode.F12: return Key.F12;
                default: return Key.None;
            }
        }
#endif
    }
}