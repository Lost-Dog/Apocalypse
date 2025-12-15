using UnityEngine;

namespace JUTPS.CrossPlataform
{
    /// <summary>
    /// Show or hide elements when is or not on mobile.
    /// </summary>
    public class JUActiveOnMobile : MonoBehaviour
    {
        /// <summary>
        /// Must show if is not on mobile?
        /// </summary>
        public bool Invert;

        /// <summary>
        /// The objects to show when or mobile (or hide, if <see cref="Invert"/> is true).
        /// </summary>
        public GameObject[] Objects;

        private void OnEnable()
        {
            Check();
        }

        private void Update()
        {
            Check();
        }

        private void Check()
        {
            bool actived = (JUGameManager.IsMobileControls && !Invert) || (!JUGameManager.IsMobileControls && Invert);

            for (int i = 0; i < Objects.Length; i++)
            {
                if (Objects[i] && Objects[i].activeSelf != actived)
                    Objects[i].SetActive(actived);
            }
        }
    }
}


