using UnityEngine;
using UnityEngine.InputSystem;

namespace JUTPS.VehicleSystem
{
    /// <summary>
    /// Applies torque to a <see cref="JUVehicle"/> if not grounded.
    /// </summary>
    public class JUVehicleAirRotation : MonoBehaviour
    {
        /// <summary>
        /// Use input actions to rotate the vehicle.
        /// </summary>
        public bool UseInputs;

        /// <summary>
        /// The vertical action used to turn the vehicle on X axis when not grounded.
        /// </summary>
        public InputAction VerticalAction;

        /// <summary>
        /// The horizontal action used to turn the vehicle on Y axis when not grounded.
        /// </summary>
        public InputAction HorizontalAction;

        /// <summary>
        /// The rotation force.
        /// </summary>
        public float Force;

        /// <summary>
        /// If true, the <see cref="Vehicle"/> can rotate on the X axis when not grounded.
        /// </summary>
        [Header("Direction")]
        public bool X;

        /// <summary>
        /// If true, the <see cref="Vehicle"/> can rotate on the Y axis when not grounded.
        /// </summary>
        public bool Y;

        /// <summary>
        /// If true, the <see cref="Vehicle"/> can rotate on the Z axis when not grounded.
        /// </summary>
        public bool Z;

        /// <summary>
        ///  The vehicle attacked to this <see cref="GameObject"/>.
        /// </summary>
        public JUWheeledVehicle Vehicle { get; private set; }

        /// <summary>
        /// The vertical axis value from <see cref="VerticalAction"/> to rotate the <see cref="Vehicle"/> vertically when not grounded.
        /// </summary>
        public float VerticalAxis
        {
            get => VerticalAction.ReadValue<float>();
        }

        /// <summary>
        /// The vertical axis value from <see cref="VerticalAction"/> to rotate the <see cref="Vehicle"/> horizontally when not grounded.
        /// </summary>
        public float HorizontalAxis
        {
            get => HorizontalAction.ReadValue<float>();
        }

        /// <summary>
        /// Create a <see cref="JUVehicleAirRotation"/> component instance.
        /// </summary>
        public JUVehicleAirRotation()
        {
            UseInputs = true;
            Force = 200;

            X = true;
            Y = true;
            Z = false;

            VerticalAction = new InputAction("Vertical");
            VerticalAction.AddCompositeBinding("Axis")
             .With("Positive", "<Keyboard>/w")
             .With("Negative", "<Keyboard>/s")
              .With("Positive", "<Gamepad>/leftStick/up")
             .With("Negative", "<Gamepad>/leftStick/down");

            HorizontalAction = new InputAction("Horizontal");
            HorizontalAction.AddCompositeBinding("Axis")
             .With("Positive", "<Keyboard>/d")
             .With("Negative", "<Keyboard>/a")
             .With("Positive", "<Gamepad>/leftStick/left")
             .With("Negative", "<Gamepad>/leftStick/right");
        }

        private void Awake()
        {
            Vehicle = GetComponent<JUWheeledVehicle>();
        }

        private void OnEnable()
        {
            VerticalAction.Enable();
            HorizontalAction.Enable();
        }

        private void OnDisable()
        {
            VerticalAction.Disable();
            HorizontalAction.Disable();
        }

        private void Update()
        {
            if (!UseInputs || !Vehicle || Vehicle.IsGrounded || !Vehicle.IsOn)
                return;
            RotateVehicle(new Vector3(VerticalAxis, HorizontalAxis, 0) * Time.deltaTime);
        }

        public void RotateVehicle(Vector3 torque)
        {
            if (!Vehicle)
                return;

            if (!X) torque.x = 0;
            if (!Y) torque.y = 0;
            if (!Z) torque.z = 0;

            Vehicle.RigidBody?.AddRelativeTorque(torque * Force, ForceMode.Acceleration);
        }
    }
}