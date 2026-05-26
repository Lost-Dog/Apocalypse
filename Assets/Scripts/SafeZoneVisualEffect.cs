using UnityEngine;

public class SafeZoneVisualEffect : MonoBehaviour
{
    [Header("Visual Target")]
    [Tooltip("The child GameObject that carries the visual mesh/renderer. " +
             "Pulse and rotation are applied here so the collider on this root is never disturbed. " +
             "Leave empty to create a dedicated child automatically.")]
    public Transform visualTarget;

    [Header("Pulse Effect")]
    public bool enablePulse = true;
    public float pulseSpeed = 1f;
    public float pulseMinScale = 0.95f;
    public float pulseMaxScale = 1.05f;
    
    [Header("Rotation Effect")]
    public bool enableRotation = true;
    public float rotationSpeed = 10f;
    public Vector3 rotationAxis = Vector3.up;
    
    [Header("Glow Effect")]
    public bool enableGlow = true;
    public Color glowColor = new Color(0f, 1f, 0.5f, 0.5f);
    public float glowIntensity = 2f;
    public float glowPulseSpeed = 2f;
    
    [Header("Particle Ring")]
    public bool enableParticleRing = true;
    public GameObject particlePrefab;
    public int particleCount = 20;
    public float ringRadius = 5f;
    public float particleHeight = 0.5f;
    public float rotateRingSpeed = 5f;
    
    private Vector3 originalVisualScale;
    private Renderer visualRenderer;
    private Material glowMaterial;
    private GameObject particleRing;
    private float particleAngle = 0f;

    // The root transform that carries the collider — must never be scaled or rotated by this script.
    private Transform ColliderRoot => transform;
    
    private void Start()
    {
        EnsureVisualTarget();

        originalVisualScale = visualTarget.localScale;
        visualRenderer = visualTarget.GetComponent<Renderer>();
        
        if (enableGlow && visualRenderer != null)
        {
            SetupGlowMaterial();
        }
        
        if (enableParticleRing && particlePrefab != null)
        {
            CreateParticleRing();
        }
    }

    /// <summary>
    /// Guarantees <see cref="visualTarget"/> is a child of this GameObject so collider transforms are
    /// never touched by animation effects.
    /// </summary>
    private void EnsureVisualTarget()
    {
        if (visualTarget != null && visualTarget != transform) return;

        // Look for an existing child named "Visual"
        Transform found = transform.Find("Visual");
        if (found != null)
        {
            visualTarget = found;
            return;
        }

        // Create a dedicated visual child and reparent any Renderer/MeshFilter found on the root.
        GameObject visualGO = new GameObject("Visual");
        visualGO.transform.SetParent(transform, false);
        visualTarget = visualGO.transform;

        // Move renderer components to the child so the pulse doesn't affect the collider root.
        Renderer rootRenderer = GetComponent<Renderer>();
        if (rootRenderer != null)
        {
            // Can't move components — just disable the root renderer; visual child will own the material.
            Debug.LogWarning($"[SafeZoneVisualEffect] '{name}': Renderer found on the collider root. " +
                             "Move it to a child named 'Visual' so the pulse effect doesn't disturb the trigger.", this);
        }
    }
    
    private void Update()
    {
        if (enablePulse)
        {
            ApplyPulseEffect();
        }
        
        if (enableRotation)
        {
            ApplyRotationEffect();
        }
        
        if (enableGlow && glowMaterial != null)
        {
            ApplyGlowEffect();
        }
        
        if (enableParticleRing && particleRing != null)
        {
            RotateParticleRing();
        }
    }

    /// <summary>Scales only the visual child — the collider root stays at its original scale.</summary>
    private void ApplyPulseEffect()
    {
        float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        visualTarget.localScale = originalVisualScale * scale;
    }

    /// <summary>Rotates only the visual child — the collider root keeps its original rotation.</summary>
    private void ApplyRotationEffect()
    {
        visualTarget.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
    
    private void ApplyGlowEffect()
    {
        float intensity = Mathf.Lerp(glowIntensity * 0.5f, glowIntensity, 
            (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) * 0.5f);
        
        if (glowMaterial.HasProperty("_EmissionColor"))
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * intensity);
        }
    }
    
    private void RotateParticleRing()
    {
        particleAngle += rotateRingSpeed * Time.deltaTime;
        particleRing.transform.rotation = Quaternion.Euler(0f, particleAngle, 0f);
    }
    
    private void SetupGlowMaterial()
    {
        glowMaterial = new Material(visualRenderer.material);
        visualRenderer.material = glowMaterial;
        
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }
    
    private void CreateParticleRing()
    {
        // Parent the ring to the collider root so it doesn't inherit visual-child rotation.
        particleRing = new GameObject("ParticleRing");
        particleRing.transform.SetParent(ColliderRoot, false);
        particleRing.transform.localPosition = Vector3.zero;
        
        float angleStep = 360f / particleCount;
        
        for (int i = 0; i < particleCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * ringRadius,
                particleHeight,
                Mathf.Sin(angle) * ringRadius
            );
            
            GameObject particle = Instantiate(particlePrefab, particleRing.transform);
            particle.transform.localPosition = position;
        }
    }
    
    private void OnDestroy()
    {
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}
