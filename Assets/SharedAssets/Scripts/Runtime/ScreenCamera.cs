using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ScreenCamera : MonoBehaviour
{
    [SerializeField] private float m_RenderTextureScale = 1;

    private Camera m_Camera;
    private Vector2 lastSize;

    void Awake()
    {
        m_Camera = GetComponent<Camera>();
        UpdateTarget();
    }

    private void Update()
    {
        Vector2 currentSize = new Vector2(Screen.width, Screen.height);

        // If the size has changed, update the render texture size
        if (currentSize != lastSize)
        {
            lastSize = currentSize;
            UpdateTarget();
        }
    }

    public void UpdateTarget()
    {
        if (m_Camera.targetTexture != null)
            m_Camera.targetTexture.Release();

        m_Camera.targetTexture = new RenderTexture((int)(Screen.width * m_RenderTextureScale), (int)(Screen.height * m_RenderTextureScale), GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormat.D24_UNorm_S8_UInt);
    }
}