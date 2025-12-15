using JUTPS.VehicleSystem.Inputs;
using UnityEngine;

namespace JUTPS.VehicleSystem
{
    public class CustomGenericVehicle : JUVehicle
    {
        public JUVehicleInputAsset PlayerInputs;
        public LayerMask GroundLayer;
        public float MaxHeight;
        public float Acceleration;
        public float Damping;
        public float RotationSpeed;

        private Transform _camera;

        public CustomGenericVehicle()
        {
            MaxHeight = 2;
            Acceleration = 500;
            GroundLayer = -1;
            Damping = 1;
            RotationSpeed = 10;
        }

        protected override void Awake()
        {
            base.Awake();
            RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            RigidBody.useGravity = false;
        }

        protected override void Start()
        {
            base.Start();
            _camera = Camera.main.transform;
        }

        protected override void Update()
        {
            base.Update();

            // Controlling the vehicle.
            if (IsOn && ControlsEnabled && UsePlayerInputs && PlayerInputs)
            {
                Horizontal = PlayerInputs.SteerAxis;
                Vertical = PlayerInputs.ThrottleAxis;

                float acceleration = Acceleration * Time.deltaTime;
                Vector3 forward = _camera.forward * Vertical;
                Vector3 right = _camera.right * Horizontal;
                Vector3 force = (right + forward).normalized * acceleration;
                RigidBody.AddForce(force, ForceMode.Acceleration);
            }

            // Update vehicle orientation
            if (ForwardSpeed > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(RigidBody.linearVelocity.normalized, Vector3.up));
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Mathf.Clamp01(Time.deltaTime * RotationSpeed));
            }

            // Floating
            if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, float.PositiveInfinity, GroundLayer))
            {
                Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + MaxHeight, transform.position.z);
                float smooth = Vector3.Distance(transform.position, targetPosition) * Damping;
                RigidBody.position = Vector3.Lerp(RigidBody.position, targetPosition, Mathf.Clamp01(smooth * Time.deltaTime));
            }
        }
    }
}