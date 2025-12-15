using UnityEngine;

namespace JU
{
    /// <summary>
    /// Represents an specific area in the 3D world.
    /// </summary>
    public class JUBoxArea : MonoBehaviour
    {
        /// <summary>
        /// The area bounds.
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                return new Bounds(transform.position, transform.lossyScale);
            }
        }
        private void OnDrawGizmos()
        {
            Bounds area = Bounds;
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawCube(area.center, area.size);

            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawWireCube(area.center, area.size);
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/JUTPS Create/Box Area", false, 0)]
        private static void CreateBox()
        {
            GameObject gameObject = new GameObject("JU Box Area");
            gameObject.AddComponent<JUBoxArea>();
            gameObject.transform.localScale = new Vector3(10, 5, 10);
            gameObject.transform.position = Editor.JUEditor.SceneViewSpawnPosition() + (Vector3.up * 2.5f);
            gameObject.transform.rotation = Editor.JUEditor.SceneViewSpawnRotation();
        }
#endif
    }
}