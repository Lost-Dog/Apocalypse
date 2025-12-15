using UnityEngine;
using UnityEngine.InputSystem;

namespace JUTPS.VehicleSystem
{
    /// <summary>
    /// This component allow the vehicle jump.
    /// </summary>
    public class VehicleJump : MonoBehaviour
    {
        /// <summary>
        /// Use <see cref="JumpAction"/> to do vehicle jump?
        /// </summary>
        public bool UseDefaultInput;

        /// <summary>
        /// The action that contains the keys/buttons to player jump.
        /// </summary>
        public InputAction JumpAction;

        /// <summary>
        /// The jump force.
        /// </summary>
        public float JumpForce;

        /// <summary>
        /// The vehicle that be controled by this <see cref="VehicleJump"/> component.
        /// </summary>
        public JUWheeledVehicle Vehicle { get; private set; }

        /// <summary>
        /// Return true when press <see cref="JumpAction"/> by the first time.
        /// </summary>
        public bool IsJumpTriggered
        {
            get => JumpAction.triggered;
        }

        /// <summary>
        /// Create a <see cref="VehicleJump"/> component instance.
        /// </summary>
        public VehicleJump()
        {
            JumpForce = 7000;
            UseDefaultInput = true;

            JumpAction = new InputAction();
            JumpAction.AddBinding("<Keyboard>/space");
            JumpAction.AddBinding("<Gamepad>/buttonSouth");
        }

        private void OnEnable()
        {
            JumpAction.Enable();
        }

        private void OnDisable()
        {
            JumpAction.Disable();
        }

        private void Start()
        {
            Vehicle = GetComponent<JUWheeledVehicle>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!UseDefaultInput || !Vehicle || !Vehicle.IsOn)
                return;

            if (IsJumpTriggered)
                Jump(JumpForce);
        }

        /// <summary>
        /// Do vehicle jump.
        /// </summary>
        /// <param name="force">The jump force</param>
        public void Jump(float force)
        {
            if (!Vehicle || !Vehicle.IsGrounded)
                return;

            Vehicle.RigidBody.AddRelativeForce(0, force, 0, ForceMode.Impulse);
        }
    }
}