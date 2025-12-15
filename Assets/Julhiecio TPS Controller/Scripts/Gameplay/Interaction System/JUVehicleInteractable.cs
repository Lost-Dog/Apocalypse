using JUTPS.ActionScripts;
using JUTPS.VehicleSystem;
using UnityEngine;

namespace JUTPS.InteractionSystem.Interactables
{
    [AddComponentMenu("JU TPS/Interaction System/Interactables/JU Vehicle Interactable")]
    public class JUVehicleInteractable : JUInteractable
    {
        [System.Serializable]
        public struct EnterVehicleSettings
        {
            /// <summary>
            /// Don't allow the character enter on the <see cref="NearestVehicle"/> 
            /// if the <see cref="Vehicle"/> speed is greater than <see cref="MaxVehicleSpeedToEnter"/>.
            /// </summary>
            [Min(1)] public float MaxVehicleSpeedToEnter;

            /// <summary>
            /// Don't allow the character enter to the <see cref="NearestVehicle"/> 
            /// if the character <see cref="Rigidbody.velocity"/> is greater than <see cref="MaxCharacterSpeedToEnter"/>.
            /// </summary>
            [Min(0.1f)] public float MaxCharacterSpeedToEnter;
        }

        /// <summary>
        /// Settings to character enter on the vehicle.
        /// </summary>
        public EnterVehicleSettings EnterExitVehicle;

        /// <summary>
        /// The vehicle.
        /// </summary>
        public JUVehicle Vehicle { get; private set; }

        /// <summary>
        /// Instantiate component.
        /// </summary>
        public JUVehicleInteractable() : base()
        {
            EnterExitVehicle = new EnterVehicleSettings
            {
                MaxCharacterSpeedToEnter = 5f,
                MaxVehicleSpeedToEnter = 5f,
            };

            InteractionEnabled = true;
        }

        /// <inheritdoc/>
        public override bool CanInteract(JUInteractionSystem interactionSystem)
        {
            if (!InteractionEnabled)
                return false;

            DriveVehicles driveVehicleSyetem = interactionSystem.GetComponent<DriveVehicles>();
            Rigidbody characterPhysics = interactionSystem.GetComponent<Rigidbody>();
            JUCharacterController character = interactionSystem.GetComponent<JUCharacterController>();

            if (!driveVehicleSyetem || !character || !characterPhysics || driveVehicleSyetem.IsDriving)
                return false;

            if (character.IsRolling)
                return false;

            if (character.IsRagdolled)
                return false;

            if (!isActiveAndEnabled)
                return false;

            float vehicleSpeed = Vehicle.Velocity.magnitude;
            float characterSpeed = characterPhysics.linearVelocity.magnitude;

            bool isVehicleStoped = vehicleSpeed < EnterExitVehicle.MaxVehicleSpeedToEnter;
            bool isCharacterStoped = characterSpeed < EnterExitVehicle.MaxCharacterSpeedToEnter;

            return isVehicleStoped && isCharacterStoped;
        }

        protected override void Start()
        {
            Vehicle = GetComponentInParent<JUVehicle>();
        }
    }
}