using UnityEngine;

public class SafeZoneVisualEffect : MonoBehaviour
{
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
    
    private Vector3 originalScale;
    private Renderer zoneRenderer;
    private Material glowMaterial;
    private GameObject particleRing;
    private float particleAngle = 0f;
    
    private void Start()
    {
        originalScale = transform.localScale;
        zoneRenderer = GetComponent<Renderer>();
        
        if (enableGlow && zoneRenderer != null)
        {
            SetupGlowMaterial();
        }
        
        if (enableParticleRing && particlePrefab != null)
        {
            CreateParticleRing();
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
    
    private void ApplyPulseEffect()
    {
        float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = originalScale * scale;
    }
    
    private void ApplyRotationEffect()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
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
        glowMaterial = new Material(zoneRenderer.material);
        zoneRenderer.material = glowMaterial;
        
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }
    
    private void CreateParticleRing()
    {
        particleRing = new GameObject("ParticleRing");
        particleRing.transform.SetParent(transform);
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
