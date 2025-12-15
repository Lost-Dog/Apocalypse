using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JUTPS.VehicleSystem.Inputs
{
    /// <summary>
    /// Used to stores all player inputs to control <see cref="JUWheeledVehicle"/>s.
    /// </summary>
    public class JUVehicleInputAsset : ScriptableObject
    {
        /// <summary>
        /// The throttle action, contains all inputs to accelerate the vehicle.
        /// </summary>
        public InputAction ThrottleAction;

        /// <summary>
        /// The steer action, constains all inputs to turn the vehicle to left or right.
        /// </summary>
        [Space]
        public InputAction SteerAction;

        /// <summary>
        /// The brake action, constains all input to brake the vehicle.
        /// </summary>
        [Space]
        public InputAction BrakeAction;

        /// <summary>
        /// The nitro action, constains all input to active vehicle nitro.
        /// </summary>
        [Space]
        public InputAction NitroAction;

        /// <summary>
        /// The throttle value, -1 to 1 from <see cref="ThrottleAction"/>.
        /// </summary>
        public float ThrottleAxis
        {
            get => ThrottleAction.ReadValue<float>();
        }

        /// <summary>
        /// The steer value, -1 to 1 from <see cref="SteerAction"/>.
        /// </summary>
        public float SteerAxis
        {
            get => SteerAction.ReadValue<float>();
        }

        /// <summary>
        /// The brake value, 0 to 1 from <see cref="BrakeAction"/>.
        /// </summary>
        public float BrakeAxis
        {
            get => Mathf.Clamp01(BrakeAction.ReadValue<float>());
        }

        /// <summary>
        /// Return true if <see cref="NitroAction"/> is pressed.
        /// </summary>
        public bool IsNitroPressed
        {
            get => NitroAction.inProgress;
        }

        /// <summary>
        /// Create an <see cref="ScriptableObject"/> that contains all inputs to control a <see cref="JUWheeledVehicle"/>.
        /// </summary>
        public JUVehicleInputAsset()
        {
            ThrottleAction = new InputAction("Throttle", InputActionType.Value);
            SteerAction = new InputAction("Steer", InputActionType.Value);
            BrakeAction = new InputAction("Brake", InputActionType.Value);
            NitroAction = new InputAction("Nitro", InputActionType.Value);
        }

        /// <summary>
        /// Set input actions as enabled or disabled.
        /// </summary>
        /// <param name="enabled"></param>
        public void SetInputEnabled(bool enabled)
        {
            switch (enabled)
            {
                case true:
                    ThrottleAction.Enable();
                    SteerAction.Enable();
                    BrakeAction.Enable();
                    NitroAction.Enable();
                    break;
                case false:
                    ThrottleAction.Disable();
                    SteerAction.Disable();
                    BrakeAction.Disable();
                    NitroAction.Disable();
                    break;
            }
        }

#if UNITY_EDITOR

        private static void CreateAsset<T>(string assetName, UnityAction<T> onCreated) where T : ScriptableObject
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                var instance = CreateInstance<T>();
                var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

                if (string.IsNullOrEmpty(path))
                    path = "Assets";

                if (path.Contains("."))
                    path = path.Remove(path.LastIndexOf('/'));

                var pathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{assetName}.asset");
                AssetDatabase.CreateAsset(instance, pathAndName);

                onCreated?.Invoke(instance);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = instance;
                EditorUtility.FocusProjectWindow();

                AssetDatabase.StopAssetEditing();
                EditorUtility.SetDirty(instance);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Can't create a new asset on this folder.");
                Debug.LogError(e.Message);
            }
        }

        [MenuItem("Assets/Create/JU TPS/Vehicles/Classic Vehicle Input", false, 1)]
        private static void CreateClassicInputAsset()
        {
            CreateAsset<JUVehicleInputAsset>("Classic Vehicle Input", instance =>
            {
                instance.ThrottleAction.AddCompositeBinding("Axis")
                    .With("Positive", "<Gamepad>/leftStick/up")
                    .With("Negative", "<Gamepad>/leftStick/down")
                    .With("Positive", "<Keyboard>/w")
                    .With("Negative", "<Keyboard>/s");

                instance.SteerAction.AddCompositeBinding("Axis")
                    .With("Positive", "<Gamepad>/leftStick/right")
                    .With("Negative", "<Gamepad>/leftStick/left")
                    .With("Positive", "<Keyboard>/d")
                    .With("Negative", "<Keyboard>/a");

                instance.BrakeAction.AddBinding("<Gamepad>/buttonEast");
                instance.BrakeAction.AddBinding("<Keyboard>/space");

                instance.NitroAction.AddBinding("<Keyboard>/shift");
                instance.NitroAction.AddBinding("<Gamepad>/leftStickPress");
            });
        }

        [MenuItem("Assets/Create/JU TPS/Vehicles/Advanced Vehicle Input", false, 1)]
        private static void CreateAdvancedInputAsset()
        {
            CreateAsset<JUVehicleInputAsset>("Advanced Vehicle Input", instance =>
            {
                instance.ThrottleAction.AddCompositeBinding("Axis")
                    .With("Positive", "<Gamepad>/rightTrigger")
                    .With("Negative", "<Gamepad>/leftTrigger")
                    .With("Positive", "<Keyboard>/w")
                    .With("Negative", "<Keyboard>/s");

                instance.SteerAction.AddCompositeBinding("Axis")
                    .With("Positive", "<Gamepad>/leftStick/right")
                    .With("Negative", "<Gamepad>/leftStick/left")
                    .With("Positive", "<Keyboard>/d")
                    .With("Negative", "<Keyboard>/a");

                instance.BrakeAction.AddBinding("<Gamepad>/buttonEast");
                instance.BrakeAction.AddBinding("<Keyboard>/space");

                instance.NitroAction.AddBinding("<Keyboard>/shift");
                instance.NitroAction.AddBinding("<Gamepad>/leftStickPress");
            });
        }
#endif
    }
}