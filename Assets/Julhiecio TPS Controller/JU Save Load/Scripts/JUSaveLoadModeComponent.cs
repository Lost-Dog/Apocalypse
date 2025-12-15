using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save Modes.
    /// </summary>
    public enum SaveLoadModes
    {
        /// <summary>
        /// Save the data into a specific scene.
        /// </summary>
        Scene,

        /// <summary>
        /// Save the data to be avaliable on any scene.
        /// </summary>
        Global
    }

    /// <summary>
    /// Specify the save mode used by save load components of this gameObjects or child components.
    /// </summary>
    [AddComponentMenu("JU/Save Load/JU Save Load Mode")]
    public class JUSaveLoadModeComponent : MonoBehaviour
    {
        private static JUSaveLoadModeComponent _instance;

        /// <summary>
        /// The save mode for the gameObject.
        /// </summary>
        public SaveLoadModes Mode;

        /// <summary>
        /// Return the instance, if have.
        /// </summary>
        public static JUSaveLoadModeComponent Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<JUSaveLoadModeComponent>();

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }
    }
}