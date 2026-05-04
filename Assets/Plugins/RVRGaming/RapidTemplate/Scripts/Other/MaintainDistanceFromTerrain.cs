using UnityEngine;

public class MaintainDistanceFromTerrain : MonoBehaviour
{
    public float fixedDistance = 5f;
    public LayerMask terrainLayer;

    void Update()
    {
        MaintainHeight();
    }

    private void MaintainHeight()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            float terrainY = hit.point.y;

            float targetY = terrainY + fixedDistance;

            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }
}
