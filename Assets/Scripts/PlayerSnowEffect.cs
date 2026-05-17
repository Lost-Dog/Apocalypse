using UnityEngine;

/// <summary>
/// Attaches a Particle System snow effect to this GameObject and moves it with
/// the player so the blizzard cylinder always surrounds the player.
///
/// Setup: Add this component to a child of the player (or any GameObject that
/// already follows the player). Configure the fields in the Inspector and the
/// particle system is created / configured at runtime — no separate prefab needed.
///
/// Alternatively, assign an existing ParticleSystem prefab to
/// <see cref="overrideParticleSystem"/> and the script will position/configure
/// only the emission shape without touching the other modules.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PlayerSnowEffect : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Shape & Coverage")]
    [Tooltip("Horizontal radius of the snow cylinder around the player (metres).")]
    public float radius = 50f;

    [Tooltip("Vertical span of the snow column. Particles spawn at the top and fall.")]
    public float columnHeight = 20f;

    [Header("Emission")]
    [Tooltip("Snowflakes emitted per second.")]
    public float emissionRate = 400f;

    [Header("Particle Appearance")]
    [Tooltip("Minimum size of a snowflake.")]
    public float minSize = 0.05f;

    [Tooltip("Maximum size of a snowflake.")]
    public float maxSize = 0.25f;

    [Tooltip("How long each snowflake lives (seconds). Combined with fall speed this controls how far flakes fall before recycling.")]
    public float particleLifetime = 6f;

    [Tooltip("Material used for each snowflake. Leave null to use a plain white default.")]
    public Material snowMaterial;

    [Header("Physics")]
    [Tooltip("Base downward fall speed (m/s).")]
    public float fallSpeed = 3f;

    [Tooltip("Lateral wind strength (m/s).")]
    public float windStrength = 0.8f;

    [Tooltip("Wind direction on the XZ plane (world-space). Zero = auto-random.")]
    public Vector2 windDirection = Vector2.zero;

    [Header("Colour")]
    [Tooltip("Base snowflake colour. Alpha drives opacity.")]
    public Color snowColor = new Color(1f, 1f, 1f, 0.85f);

    // -------------------------------------------------------------------------
    // Internals
    // -------------------------------------------------------------------------

    private ParticleSystem _ps;
    private Transform _playerTransform;

    // Snow should not fall with the GameObject — we simulate in world space.
    private const ParticleSystemSimulationSpace SimulationSpace =
        ParticleSystemSimulationSpace.World;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        _playerTransform = transform;

        ConfigureParticleSystem();
    }

    private void LateUpdate()
    {
        // Keep the emitter cylinder centred on the player's XZ position.
        // Y offset places the spawn plane above the player's head.
        Vector3 pos = _playerTransform.position;
        pos.y += columnHeight * 0.5f;
        transform.position = pos;
    }

    // -------------------------------------------------------------------------
    // Particle system configuration
    // -------------------------------------------------------------------------

    private void ConfigureParticleSystem()
    {
        // --- Main module ---
        var main = _ps.main;
        main.loop                   = true;
        main.simulationSpace        = SimulationSpace;
        main.startLifetime          = particleLifetime;
        main.startSpeed             = 0f;          // velocity set via Velocity-over-Lifetime
        main.startSize              = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor             = snowColor;
        main.maxParticles           = Mathf.CeilToInt(emissionRate * particleLifetime * 1.1f);
        main.gravityModifier        = 0f;          // we control gravity manually
        main.stopAction             = ParticleSystemStopAction.Disable;

        // --- Emission ---
        var emission = _ps.emission;
        emission.enabled            = true;
        emission.rateOverTime       = emissionRate;

        // --- Shape: flat disc at the top of the column ---
        var shape = _ps.shape;
        shape.enabled               = true;
        shape.shapeType             = ParticleSystemShapeType.Circle;
        shape.radius                = radius;
        shape.radiusThickness       = 1f;          // full disc (not just the edge)
        shape.rotation              = new Vector3(90f, 0f, 0f); // face downward

        // --- Velocity over lifetime (fall + wind) ---
        var vel = _ps.velocityOverLifetime;
        vel.enabled                 = true;
        vel.space                   = ParticleSystemSimulationSpace.World;

        Vector2 wd = windDirection == Vector2.zero
            ? Random.insideUnitCircle.normalized
            : windDirection.normalized;

        vel.x = new ParticleSystem.MinMaxCurve(wd.x * windStrength * 0.5f,
                                                wd.x * windStrength);
        vel.y = new ParticleSystem.MinMaxCurve(-fallSpeed * 1.2f, -fallSpeed * 0.8f);
        vel.z = new ParticleSystem.MinMaxCurve(wd.y * windStrength * 0.5f,
                                                wd.y * windStrength);

        // --- Size over lifetime: slight shrink near end (melting look) ---
        var sizeOL = _ps.sizeOverLifetime;
        sizeOL.enabled              = true;
        AnimationCurve sizeCurve    = AnimationCurve.Linear(0f, 1f, 1f, 0.4f);
        sizeOL.size                 = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // --- Noise: turbulence for organic drift ---
        var noise = _ps.noise;
        noise.enabled               = true;
        noise.strength              = 0.3f;
        noise.frequency             = 0.15f;
        noise.scrollSpeed           = 0.1f;
        noise.quality               = ParticleSystemNoiseQuality.Medium;

        // --- Renderer ---
        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode         = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge       = -10f;        // render above most geometry

        if (snowMaterial != null)
        {
            renderer.material       = snowMaterial;
        }
        else
        {
            // Fallback: create a simple URP particle-unlit material
            renderer.material       = BuildFallbackMaterial();
        }

        // Play immediately
        _ps.Play();
    }

    /// <summary>
    /// Creates a minimal URP Particles/Unlit material tinted white.
    /// Used only when no material is assigned in the Inspector.
    /// </summary>
    private static Material BuildFallbackMaterial()
    {
        // URP ships with "Universal Render Pipeline/Particles/Unlit"
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        var mat = new Material(shader)
        {
            name = "Snow_Fallback"
        };
        mat.SetColor("_BaseColor", Color.white);
        return mat;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualise the snow cylinder in the Scene view.
        Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.35f);
        Vector3 top    = transform.position + Vector3.up * (columnHeight * 0.5f);
        Vector3 bottom = transform.position - Vector3.up * (columnHeight * 0.5f);
        DrawWireDisc(top,    radius);
        DrawWireDisc(bottom, radius);

        // Verticals at four cardinal points
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f,
                                          Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(top + offset, bottom + offset);
        }
    }

    private static void DrawWireDisc(Vector3 centre, float r)
    {
        const int Segments = 32;
        Vector3 prev = centre + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            Vector3 next = centre + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
