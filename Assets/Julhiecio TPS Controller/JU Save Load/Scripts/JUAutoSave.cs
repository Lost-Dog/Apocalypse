using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Auto write the save progress after every X secounds.
    /// </summary>
    [AddComponentMenu("JU/Save Load/JU Auto Save")]
    public class JUAutoSave : MonoBehaviour
    {
        private float _timer;

        /// <summary>
        /// The time on seconds to write the save again.
        /// </summary>
        public float TimeToSave;

        /// <summary>
        /// Create component instance.
        /// </summary>
        public JUAutoSave()
        {
            TimeToSave = 20;
        }

        private void OnDisable()
        {
            _timer = 0;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > TimeToSave)
            {
                _timer = 0;
                JUSaveLoadManager.SaveOnFile();
            }
        }
    }
}