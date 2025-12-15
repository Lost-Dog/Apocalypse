using UnityEngine;

public class Compass : MonoBehaviour
{

    public Transform viewDirection;
    public RectTransform compassElement;
    public float compassSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 forwardVector = Vector3.ProjectOnPlane(viewDirection.forward, Vector3.up).normalized;
        float forwardSignedAngle = Vector3.SignedAngle(forwardVector, Vector3.forward, Vector3.up);
        float compassOffset = forwardSignedAngle / 180f * compassSize;
        compassElement.anchoredPosition = new Vector2(compassOffset, 0);
    }
}
