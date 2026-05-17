using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using InvInputDevice = Invector.vCharacterController.InputDevice;

public class vJoystickMouseInput : BaseInput
{
    public StandaloneInputModule inputModule;
    public RectTransform cursor;

    // Gamepad stick used to drive the virtual cursor in joystick mode.
    // Defaults to the right stick (for menu navigation).
    [Tooltip("Gamepad stick used to move the virtual UI cursor.")]
    public bool useRightStick = true;
    public float mouseSpeed = 4;

    public BaseInput oldOverride;

    protected override void OnEnable()
    {
        if (inputModule)
            inputModule.inputOverride = this;
    }

    protected override void OnDisable()
    {
        if (inputModule)
            inputModule.inputOverride = oldOverride;
    }

    protected override void Awake()
    {
        base.Awake();
        if (!inputModule)
            inputModule = FindFirstObjectByType<StandaloneInputModule>();
        if (inputModule)
        {
            oldOverride = inputModule.inputOverride;
            inputModule.inputOverride = this;
        }
    }

    protected Vector2 CursorPosition = Vector2.zero;

    /// <summary>
    /// Returns the current virtual cursor position.
    /// In joystick mode the position is driven by the gamepad stick.
    /// </summary>
    public override Vector2 mousePosition
    {
        get
        {
            if (vInput.instance.inputDevice == InvInputDevice.Joystick)
            {
                if (cursor && (!cursor.gameObject.activeSelf || Cursor.visible))
                {
                    Cursor.visible = false;
                    CursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                    cursor.gameObject.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                }

                var stickDelta = ReadGamepadStick();
                CursorPosition.x = Mathf.Clamp(CursorPosition.x + stickDelta.x * mouseSpeed, 0f, Screen.width);
                CursorPosition.y = Mathf.Clamp(CursorPosition.y + stickDelta.y * mouseSpeed, 0f, Screen.height);
            }
            else
            {
                if (cursor && cursor.gameObject.activeSelf)
                {
                    Cursor.visible = true;
                    cursor.gameObject.SetActive(false);
                }
                CursorPosition = base.mousePosition;
            }

            if (cursor) cursor.position = CursorPosition;
            return CursorPosition;
        }
    }

    public override bool GetMouseButton(int button)
    {
        if (vInput.instance.inputDevice == InvInputDevice.Joystick && button == 0)
            return IsSubmitHeld();
        return base.GetMouseButton(button);
    }

    public override bool GetMouseButtonUp(int button)
    {
        if (vInput.instance.inputDevice == InvInputDevice.Joystick && button == 0)
            return IsSubmitReleased();
        return base.GetMouseButtonUp(button);
    }

    public override bool GetMouseButtonDown(int button)
    {
        if (vInput.instance.inputDevice == InvInputDevice.Joystick && button == 0)
            return IsSubmitPressed();
        return base.GetMouseButtonDown(button);
    }

    // Returns the stick delta used for virtual cursor movement.
    private Vector2 ReadGamepadStick()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return Vector2.zero;
        return useRightStick ? gamepad.rightStick.ReadValue() : gamepad.leftStick.ReadValue();
    }

    // Maps the UI "submit" action to the gamepad south button (A / Cross).
    private static bool IsSubmitHeld()
    {
        return Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
    }

    private static bool IsSubmitPressed()
    {
        return Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
    }

    private static bool IsSubmitReleased()
    {
        return Gamepad.current != null && Gamepad.current.buttonSouth.wasReleasedThisFrame;
    }
}
