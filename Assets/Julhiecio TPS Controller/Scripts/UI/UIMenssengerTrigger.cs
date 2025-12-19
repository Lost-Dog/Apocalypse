using UnityEngine;
using UnityEngine.UI;
namespace JUTPS.Utilities
{

    [AddComponentMenu("JU TPS/Utilities/Trigger Menssage")]
    [RequireComponent(typeof(BoxCollider))]
    public class UIMenssengerTrigger : MonoBehaviour
    {
        private GameObject TextPanel;
        private Text TextTarget;
        [TextArea(0, 10)]
        public string TextToShow;
        [SerializeField] private string PlayerTag = "Player";
        [SerializeField] private string MessageFieldName = "MenssagesPanel";

        BoxCollider boxcollider;
        private bool hasTriedToFind = false;
        
        private void EnsureMessagePanelFound()
        {
            if (TextPanel != null) return;
            if (hasTriedToFind) return;
            
            hasTriedToFind = true;
            TextPanel = GameObject.Find(MessageFieldName);
            
            if (TextPanel != null)
            {
                TextTarget = TextPanel.GetComponentInChildren<Text>();
                
                if (TextTarget == null)
                {
                    Debug.LogWarning($"UIMenssengerTrigger on '{gameObject.name}': Found '{MessageFieldName}' but it has no Text component in children.");
                }
            }
            else
            {
                Debug.LogWarning($"UIMenssengerTrigger on '{gameObject.name}': Could not find GameObject named '{MessageFieldName}'. Message system will not work.");
            }
        }

        public void ShowMenssage()
        {
            EnsureMessagePanelFound();
            if (TextPanel == null || TextTarget == null) return;
            
            TextPanel.SetActive(true);
            TextTarget.text = TextToShow;
        }
        public void HideMenssage()
        {
            EnsureMessagePanelFound();
            if (TextPanel == null || TextTarget == null) return;
            
            TextPanel.SetActive(false);
            TextTarget.text = "";
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == PlayerTag)
                ShowMenssage();
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == PlayerTag)
                HideMenssage();
        }

        private void OnDrawGizmos()
        {
            if (boxcollider == null) boxcollider = GetComponent<BoxCollider>();

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawCube(boxcollider.center, boxcollider.size);
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawWireCube(boxcollider.center, boxcollider.size);
        }
    }
}