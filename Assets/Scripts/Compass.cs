using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform viewDirection;
    public RectTransform compassElement;
    public float compassSize;

    private bool hasWarned = false;

    void Start()
    {
        // Auto-find camera if not assigned
        if (viewDirection == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                viewDirection = mainCamera.transform;
                // Debug.Log($"[Compass] Auto-assigned Main Camera to viewDirection");
            }
            else
            {
                // Debug.LogError($"[Compass] No camera assigned to viewDirection and couldn't find Main Camera!");
            }
        }

        // Validate compassElement
        if (compassElement == null)
        {
            // Debug.LogError($"[Compass] compassElement is not assigned! Compass will not work.");
        }

        // Log initial state
        // Debug.Log($"[Compass] Initialized - Camera: {(viewDirection != null ? viewDirection.name : "NULL")}, " +
        //           $"Element: {(compassElement != null ? compassElement.name : "NULL")}, " +
        //           $"Size: {compassSize}");
    }

    void LateUpdate()
    {
        // Safety checks
        if (viewDirection == null || compassElement == null)
        {
            if (!hasWarned)
            {
                // Debug.LogWarning($"[Compass] Missing references - viewDirection: {viewDirection != null}, compassElement: {compassElement != null}");
                hasWarned = true;
            }
            return;
        }

        if (compassSize == 0)
        {
            if (!hasWarned)
            {
                // Debug.LogWarning($"[Compass] compassSize is 0! Setting to default 2300");
                compassSize = 2300f;
                hasWarned = true;
            }
        }

        Vector3 forwardVector = Vector3.ProjectOnPlane(viewDirection.forward, Vector3.up).normalized;
        float forwardSignedAngle = Vector3.SignedAngle(forwardVector, Vector3.forward, Vector3.up);
        float compassOffset = forwardSignedAngle / 180f * compassSize;
        compassElement.anchoredPosition = new Vector2(compassOffset, 0);
    }
}
