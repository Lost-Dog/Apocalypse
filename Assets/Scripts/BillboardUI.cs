using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Tooltip("The camera to face. If null, uses Camera.main")]
    public Camera targetCamera;
    
    [Tooltip("Lock rotation on specific axes")]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    
    [Tooltip("Update mode")]
    public UpdateMode updateMode = UpdateMode.LateUpdate;
    
    public enum UpdateMode
    {
        Update,
        LateUpdate
    }
    
    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }
    
    private void Update()
    {
        if (updateMode == UpdateMode.Update)
        {
            FaceCamera();
        }
    }
    
    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate)
        {
            FaceCamera();
        }
    }
    
    private void FaceCamera()
    {
        if (targetCamera == null) return;
        
        // Make the UI face the camera
        transform.rotation = targetCamera.transform.rotation;
        
        // Apply axis locks if needed
        if (lockX || lockY || lockZ)
        {
            Vector3 euler = transform.eulerAngles;
            if (lockX) euler.x = 0;
            if (lockY) euler.y = 0;
            if (lockZ) euler.z = 0;
            transform.eulerAngles = euler;
        }
    }
}
