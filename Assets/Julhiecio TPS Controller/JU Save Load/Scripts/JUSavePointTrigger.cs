using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save the current progress on save file on player enter this trigger.
    /// </summary>
    [AddComponentMenu("JU/Save Load/JU Save Point Trigger")]
    public class JUSavePointTrigger : MonoBehaviour
    {
        [SerializeField] private string _playerTag;

        /// <summary>
        /// Create component instance.
        /// </summary>
        public JUSavePointTrigger()
        {
            _playerTag = "Player";
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_playerTag))
                JUSaveLoadManager.SaveOnFile();
        }
    }
}
