using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lovatto.MiniMap
{
    /// <summary>
    /// Attach this script to the game player only
    /// This will automatically assign the player to the minimap
    /// </summary>
    public class bl_MiniMapPlayer : MonoBehaviour
    {
        private bool isAssigned = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            AssignAsTarget();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AssignAsTarget()
        {
            var miniMap = bl_MiniMap.ActiveMiniMap;
            if (miniMap == null)
            {
                Debug.LogWarning("No active minimap found to assign the player as target.");
                return;
            }

            miniMap.SetTarget(gameObject);
            isAssigned = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // In case the minimap is loaded after the player
            if (isAssigned) return;

            AssignAsTarget();
        }

    }
}