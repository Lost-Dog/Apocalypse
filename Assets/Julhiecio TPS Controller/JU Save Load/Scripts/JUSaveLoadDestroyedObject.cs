using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save state if object is destroyed.
    /// </summary>
    [AddComponentMenu("JU/Save Load/JU Save Load Destroyed Object")]
    public class JUSaveLoadDestroyedObject : JUSaveLoadComponent
    {
        private const string DESTROYED_KEY = "Destroyed";

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            if (HasValue(DESTROYED_KEY))
                Destroy(gameObject);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            if (!gameObject.scene.isLoaded)
                return;

            SetValue(DESTROYED_KEY, true);

            base.OnDestroy();
        }
    }
}