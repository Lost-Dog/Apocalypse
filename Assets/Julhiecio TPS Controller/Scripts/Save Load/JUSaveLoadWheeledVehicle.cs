using JUTPS.VehicleSystem;
using System.Collections;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save and load data for <see cref="JUWheeledVehicle"/>.
    /// </summary>
    [RequireComponent(typeof(JUWheeledVehicle))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Wheeled Vehicle")]
    public class JUSaveLoadWheeledVehicle : JUSaveLoadComponent
    {
        private JUWheeledVehicle _vehicle;
        private Vector3 _vehicleVelocity;

        [SerializeField] private bool _saveVelocity;

        private const string POSITION_KEY = "Position";
        private const string ROTATION_KEY = "Rotation";
        private const string VELOCITY_KEY = "Velocity";

        /// <inheritdoc/>
        protected override void Awake()
        {
            _vehicle = GetComponent<JUWheeledVehicle>();
            base.Awake();
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            if (!_vehicle)
                return;

            _vehicleVelocity = _vehicle.RigidBody.linearVelocity;
        }

        /// <inheritdoc/>
        public override void Load()
        {
            IEnumerator LoadAfterSetup()
            {
                yield return new WaitUntil(() => _vehicle.RigidBody != null);
                _vehicle.RigidBody.position = GetValue(POSITION_KEY, _vehicle.RigidBody.position);
                _vehicle.RigidBody.rotation = GetValue(ROTATION_KEY, _vehicle.RigidBody.rotation);
                _vehicle.RigidBody.linearVelocity = _saveVelocity ? GetValue(VELOCITY_KEY, _vehicle.RigidBody.linearVelocity) : Vector3.zero;
            }

            base.Load();

            StartCoroutine(LoadAfterSetup());
        }

        /// <inheritdoc/>
        public override void Save()
        {
            base.Save();

            SetValue(POSITION_KEY, _vehicle.RigidBody.position);
            SetValue(ROTATION_KEY, _vehicle.RigidBody.rotation);
            SetValue(VELOCITY_KEY, _vehicleVelocity);
        }

        /// <inheritdoc/>
        protected override void OnExitPlayMode()
        {
            base.OnExitPlayMode();

            _vehicle = null;
            _vehicleVelocity = Vector3.zero;
        }
    }
}