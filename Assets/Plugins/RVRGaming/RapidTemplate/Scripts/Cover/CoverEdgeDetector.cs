using UnityEngine;
using RT;

public class CoverEdgeDetector : MonoBehaviour
{
    public enum EdgeType { Left, Right }

    [Tooltip("Choose whether this edge is the left or right side of the cover.")]
    public EdgeType edgeType;

    private void OnTriggerEnter(Collider other)
    {
        CoverController coverController = other.GetComponent<CoverController>();
        if (coverController != null)
        {
            coverController.SetAtEdge(true, edgeType == EdgeType.Left);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CoverController coverController = other.GetComponent<CoverController>();
        if (coverController != null)
        {
            coverController.SetAtEdge(false, edgeType == EdgeType.Left);
        }
    }
}
