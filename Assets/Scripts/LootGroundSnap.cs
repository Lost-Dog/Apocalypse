using UnityEngine;

public class LootGroundSnap : MonoBehaviour
{
    [Header("Ground Detection")]
    [Tooltip("Enable automatic ground snapping")]
    public bool enableGroundSnap = true;
    
    [Tooltip("Time before checking for ground (allows physics to settle)")]
    public float snapDelay = 2f;
    
    [Tooltip("Maximum distance to check for ground")]
    public float maxGroundDistance = 10f;
    
    [Tooltip("Offset above ground surface")]
    public float groundOffset = 0.1f;
    
    [Tooltip("Layer mask for ground (0 = all layers)")]
    public LayerMask groundLayer;
    
    [Header("Sleep Detection")]
    [Tooltip("Stop rigidbody movement after settling")]
    public bool freezeWhenSettled = true;
    
    [Tooltip("Velocity threshold to consider settled")]
    public float settleVelocityThreshold = 0.1f;
    
    [Tooltip("Time object must be below threshold to settle")]
    public float settleTime = 1f;
    
    [Header("Slope Handling")]
    [Tooltip("Align object to ground normal on slopes")]
    public bool alignToGroundNormal = false;
    
    [Tooltip("Maximum slope angle before disabling alignment")]
    [Range(0f, 90f)]
    public float maxSlopeAngle = 45f;
    
    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    public bool showDebugLogs = false;
    
    [Tooltip("Draw debug rays in scene view")]
    public bool showDebugRays = false;
    
    private Rigidbody rb;
    private float spawnTime;
    private float lowVelocityTime;
    private bool hasSnapped = false;
    private bool hasSettled = false;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        lowVelocityTime = 0f;
        
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground", "Terrain", "Default");
        }
    }
    
    private void FixedUpdate()
    {
        if (!enableGroundSnap) return;
        
        if (!hasSnapped && Time.time >= spawnTime + snapDelay)
        {
            SnapToGround();
        }
        
        if (freezeWhenSettled && !hasSettled && rb != null)
        {
            CheckSettlement();
        }
    }
    
    private void SnapToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        LayerMask layerToUse = groundLayer.value != 0 ? groundLayer : ~0;
        
        if (showDebugRays)
        {
            Debug.DrawRay(rayStart, Vector3.down * maxGroundDistance, Color.yellow, 2f);
        }
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, maxGroundDistance, layerToUse))
        {
            Vector3 targetPosition = hit.point + Vector3.up * groundOffset;
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            if (distance > 0.1f)
            {
                transform.position = targetPosition;
                
                if (alignToGroundNormal)
                {
                    AlignToSurface(hit.normal);
                }
                
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                if (showDebugLogs)
                {
                    Debug.Log($"<color=cyan>LootGroundSnap: {gameObject.name} snapped to ground (moved {distance:F2}m)</color>");
                }
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"LootGroundSnap: {gameObject.name} could not find ground within {maxGroundDistance}m");
        }
        
        hasSnapped = true;
    }
    
    private void AlignToSurface(Vector3 groundNormal)
    {
        float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        
        if (slopeAngle <= maxSlopeAngle)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
            transform.rotation = targetRotation * transform.rotation;
            
            if (showDebugLogs)
            {
                Debug.Log($"<color=green>LootGroundSnap: Aligned to slope ({slopeAngle:F1}°)</color>");
            }
        }
    }
    
    private void CheckSettlement()
    {
        float currentVelocity = rb.linearVelocity.magnitude;
        float currentAngularVelocity = rb.angularVelocity.magnitude;
        
        if (currentVelocity < settleVelocityThreshold && currentAngularVelocity < settleVelocityThreshold)
        {
            lowVelocityTime += Time.fixedDeltaTime;
            
            if (lowVelocityTime >= settleTime)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                hasSettled = true;
                
                if (showDebugLogs)
                {
                    Debug.Log($"<color=green>LootGroundSnap: {gameObject.name} settled and frozen</color>");
                }
            }
        }
        else
        {
            lowVelocityTime = 0f;
        }
    }
    
    public void ForceSnapToGround()
    {
        hasSnapped = false;
        SnapToGround();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!enableGroundSnap) return;
        
        Gizmos.color = hasSnapped ? Color.green : Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * maxGroundDistance);
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position + Vector3.down * groundOffset, 0.1f);
    }
}
