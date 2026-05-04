using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;

public class LookAtConstraintRT : MonoBehaviour
{
    [SerializeField] private PropertyGetGameObject targetObject = new PropertyGetGameObject();
    public float rotationSpeed;
    public bool freezeX;
    public bool freezeY;
    public bool freezeZ;

    private void Update()
    {
        GameObject targetGameObject = targetObject.Get(gameObject);
        if (targetGameObject != null)
        {
            Vector3 directionToTarget = (targetGameObject.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            if (freezeX)
            {
                targetRotation.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
            }

            if (freezeY)
            {
                targetRotation.eulerAngles = new Vector3(targetRotation.eulerAngles.x, 0, targetRotation.eulerAngles.z);
            }

            if (freezeZ)
            {
                targetRotation.eulerAngles = new Vector3(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0);
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
