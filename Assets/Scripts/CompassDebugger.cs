using UnityEngine;

public class CompassDebugger : MonoBehaviour
{
    private Compass compass;
    private float lastAngle = 0f;
    private Vector2 lastPosition = Vector2.zero;

    void Start()
    {
        compass = GetComponent<Compass>();
        if (compass == null)
        {
            // Debug.LogError("[CompassDebugger] No Compass component found!");
            enabled = false;
            return;
        }

        // Debug.Log($"[CompassDebugger] Started - Compass component found and enabled: {compass.enabled}");
    }

    void Update()
    {
        if (compass == null) return;

        // Log compass state every 2 seconds
        /*if (Time.frameCount % 120 == 0)
        {
            string status = $"[Compass Status]\n" +
                           $"  Component Enabled: {compass.enabled}\n" +
                           $"  View Direction: {(compass.viewDirection != null ? compass.viewDirection.name : "NULL")}\n" +
                           $"  Compass Element: {(compass.compassElement != null ? compass.compassElement.name : "NULL")}\n" +
                           $"  Compass Size: {compass.compassSize}";

            if (compass.viewDirection != null)
            {
                Vector3 forward = compass.viewDirection.forward;
                Vector3 forwardFlat = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
                float angle = Vector3.SignedAngle(forwardFlat, Vector3.forward, Vector3.up);
                
                status += $"\n  Camera Angle: {angle:F1}°";
                
                if (Mathf.Abs(angle - lastAngle) > 0.1f)
                {
                    status += " (CHANGED)";
                }
                else
                {
                    status += " (static)";
                }
                
                lastAngle = angle;
            }

            if (compass.compassElement != null)
            {
                Vector2 currentPos = compass.compassElement.anchoredPosition;
                status += $"\n  Element Position: {currentPos}";
                
                if (Vector2.Distance(currentPos, lastPosition) > 0.1f)
                {
                    status += " (MOVING)";
                }
                else
                {
                    status += " (STATIC - NOT MOVING!)";
                }
                
                lastPosition = currentPos;
            }

            Debug.Log(status);
        }*/
    }
}
