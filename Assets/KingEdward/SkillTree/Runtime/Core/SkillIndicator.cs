using UnityEngine;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Renders the skill area indicator (circle, cone, or line) on the ground.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SkillIndicator : MonoBehaviour
    {
        [Tooltip("Assign your indicator/VFX material (circle, ring, etc). Required for custom look.")]
        [SerializeField] private Material m_BaseMaterial;
        [SerializeField] private int m_LineSegments = 64;

        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;
        private Material m_MaterialInstance;
        private SkillIndicatorConfig m_CurrentConfig;
        private bool m_IsVisible;
        private float m_RadiusOverride = -1f;

        public bool IsVisible => m_IsVisible;
        public SkillIndicatorConfig CurrentConfig => m_CurrentConfig;

        private void Awake()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            m_MeshRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (m_MaterialInstance != null)
            {
                Destroy(m_MaterialInstance);
            }
        }


        public void Show(SkillIndicatorConfig config, Vector3 position, Vector3 direction)
        {
            if (config == null || !config.HasIndicator)
            {
                Hide();
                return;
            }

            Material sourceMaterial = config.material != null ? config.material : m_BaseMaterial;
            if (sourceMaterial == null)
            {
                Shader fallback = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Sprites/Default");
                if (fallback != null)
                {
                    sourceMaterial = new Material(fallback) { color = new Color(1f, 0.3f, 0.2f, 0.4f) };
                }
            }

            if (sourceMaterial != null)
            {
                if (m_MaterialInstance == null || m_MaterialInstance.shader != sourceMaterial.shader)
                {
                    if (m_MaterialInstance != null)
                    {
                        Destroy(m_MaterialInstance);
                    }

                    m_MaterialInstance = new Material(sourceMaterial);
                    m_MeshRenderer.material = m_MaterialInstance;
                    m_MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    m_MeshRenderer.receiveShadows = false;
                }
            }

            m_CurrentConfig = config;
            m_IsVisible = true;
            m_RadiusOverride = config.IsExpandingLine ? config.MinRadius : (config.IsExpanding ? config.MinRadius : -1f);

            transform.position = position + Vector3.up * config.GroundOffset;

            if (config.type == SkillIndicatorType.Cone || config.IsLineType)
            {
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction.normalized);
                }
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            RebuildMesh();
            UpdateMaterial();
            m_MeshRenderer.enabled = true;
        }


        public void Hide()
        {
            m_IsVisible = false;
            m_MeshRenderer.enabled = false;
        }

        public void UpdatePosition(Vector3 position, Vector3 direction, float radiusOverride = -1f)
        {
            if (!m_IsVisible || m_CurrentConfig == null) return;

            m_RadiusOverride = radiusOverride;
            transform.position = position + Vector3.up * m_CurrentConfig.GroundOffset;

            if (m_CurrentConfig.type == SkillIndicatorType.Cone || m_CurrentConfig.IsLineType)
            {
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction.normalized);
                }
            }

            RebuildMesh();
        }

        private void RebuildMesh()
        {
            if (m_CurrentConfig == null) return;

            Mesh mesh = new Mesh();
            mesh.name = "SkillIndicator";

            switch (m_CurrentConfig.type)
            {
                case SkillIndicatorType.Circle:
                case SkillIndicatorType.ExpandingCircle:
                    BuildCircleMesh(mesh);
                    break;
                case SkillIndicatorType.Cone:
                    BuildConeMesh(mesh);
                    break;
                case SkillIndicatorType.Line:
                case SkillIndicatorType.ExpandingLine:
                    BuildLineMesh(mesh);
                    break;
                default:
                    mesh.Clear();
                    break;
            }

            m_MeshFilter.mesh = mesh;
        }

        private void BuildCircleMesh(Mesh mesh)
        {
            float r = m_RadiusOverride >= 0f ? m_RadiusOverride : m_CurrentConfig.Radius;
            int segs = m_LineSegments;
            var verts = new Vector3[segs + 1];
            var tris = new int[segs * 3];

            verts[0] = Vector3.zero;
            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            }

            for (int i = 0; i < segs; i++)
            {
                int j = (i + 1) % segs;
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = j + 1;
                tris[i * 3 + 2] = i + 1;
            }

            var uvs = new Vector2[segs + 1];
            uvs[0] = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * 2f * Mathf.PI;
                uvs[i + 1] = new Vector2(0.5f + 0.5f * Mathf.Cos(a), 0.5f + 0.5f * Mathf.Sin(a));
            }
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void BuildConeMesh(Mesh mesh)
        {
            float range = m_CurrentConfig.Range;
            float halfAngle = m_CurrentConfig.ConeAngle * 0.5f * Mathf.Deg2Rad;
            int segs = Mathf.Max(8, m_LineSegments / 4);
            var verts = new Vector3[segs + 2];
            var tris = new int[segs * 3];

            verts[0] = Vector3.zero;
            for (int i = 0; i <= segs; i++)
            {
                float t = (float)i / segs;
                float a = -halfAngle + t * m_CurrentConfig.ConeAngle * Mathf.Deg2Rad;
                float z = range * Mathf.Cos(a);
                float x = range * Mathf.Sin(a);
                verts[i + 1] = new Vector3(x, 0f, z);
            }

            for (int i = 0; i < segs; i++)
            {
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }

            var uvs = new Vector2[segs + 2];
            uvs[0] = new Vector2(0.5f, 0f);
            for (int i = 0; i <= segs; i++)
            {
                float t = (float)i / segs;
                uvs[i + 1] = new Vector2(t, 1f);
            }
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void BuildLineMesh(Mesh mesh)
        {
            float range = m_RadiusOverride >= 0f ? m_RadiusOverride : m_CurrentConfig.Range;
            float r = m_CurrentConfig.Radius * 0.5f;
            var verts = new Vector3[4];
            verts[0] = new Vector3(-r, 0f, -range);
            verts[1] = new Vector3(r, 0f, -range);
            verts[2] = new Vector3(r, 0f, 0f);
            verts[3] = new Vector3(-r, 0f, 0f);

            var tris = new int[] { 0, 2, 1, 0, 3, 2 };
            var uvs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void UpdateMaterial()
        {
            if (m_MaterialInstance == null || m_CurrentConfig == null) return;
            if (m_MaterialInstance.HasProperty("_BaseColor"))
                m_MaterialInstance.SetColor("_BaseColor", m_CurrentConfig.color);
            else if (m_MaterialInstance.HasProperty("_Color"))
                m_MaterialInstance.SetColor("_Color", m_CurrentConfig.color);
            else
                m_MaterialInstance.color = m_CurrentConfig.color;
        }
    }
}
