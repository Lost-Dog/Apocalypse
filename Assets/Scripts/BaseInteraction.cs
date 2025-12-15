using UnityEngine;
using UnityEngine.Events;

public class BaseInteraction : MonoBehaviour
{
    [Header("Base Settings")]
    [Tooltip("Name of this base location")]
    public string baseName = "Safe House";
    
    [Tooltip("Interaction radius")]
    public float interactionRadius = 30f;
    
    [Header("Visual Settings")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;
    
    [Header("Events")]
    public UnityEvent onPlayerEnterBase;
    public UnityEvent onPlayerExitBase;
    
    private bool playerInRange = false;
    
    private void Start()
    {
        if (MissionOfferManager.Instance != null && MissionOfferManager.Instance.baseLocation == null)
        {
            MissionOfferManager.Instance.baseLocation = transform;
            Debug.Log($"BaseInteraction: Set as base location for MissionOfferManager");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInRange)
        {
            playerInRange = true;
            onPlayerEnterBase?.Invoke();
            
            if (MissionOfferManager.Instance != null && MissionOfferManager.Instance.hasPendingOffer)
            {
                ShowMissionAcceptPrompt();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInRange)
        {
            playerInRange = false;
            onPlayerExitBase?.Invoke();
        }
    }
    
    private void ShowMissionAcceptPrompt()
    {
        if (NotificationManager.Instance != null && MissionOfferManager.Instance != null)
        {
            string missionName = MissionOfferManager.Instance.GetOfferedMissionName();
            NotificationManager.Instance.ShowMissionNotification(
                $"<b>Mission Available</b>\n{missionName}\n\nOpen Mission Menu to Accept"
            );
        }
    }
    
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.DrawIcon(transform.position, "sv_label_1", true);
    }
}
