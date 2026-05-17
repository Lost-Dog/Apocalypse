using UnityEngine;

/// <summary>
/// Continuously rotates the character's Y axis to match the ABC camera rig's horizontal
/// rotation so that weapon forward direction aligns with where the camera is looking.
///
/// Attach to the same GameObject as ABC_Controller (Tobias TPS root).
/// Assign the ABC_CameraBase's Transform (e.g. Camera_TPS) to <see cref="cameraRig"/>.
/// </summary>
public class ABCCameraBodyAlign : MonoBehaviour
{
    /// <summary>The Transform of the ABC_CameraBase rig (not the child Camera).</summary>
    [Tooltip("Assign the Camera_TPS GameObject (the one with ABC_CameraBase).")]
    public Transform cameraRig;

    /// <summary>How fast the character body rotates to match the camera. Set to a very
    /// high value (e.g. 720) for an instant snap, or a lower value for a smoothed follow.</summary>
    [Tooltip("Degrees per second. Use 720+ for instant, lower for smoothed.")]
    public float rotationSpeed = 720f;

    private void Update()
    {
        if (cameraRig == null)
            return;

        // Read only the horizontal (Y) component of the camera rig's world rotation.
        float targetYaw = cameraRig.eulerAngles.y;

        // Current character yaw.
        float currentYaw = transform.eulerAngles.y;

        // Shortest-path lerp toward the camera yaw.
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }
}
