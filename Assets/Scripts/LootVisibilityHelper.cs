using UnityEngine;

public class LootVisibilityHelper : MonoBehaviour
{
    [Header("Light Beam")]
    [Tooltip("Show vertical light beam above loot")]
    public bool showLightBeam = true;
    
    [Tooltip("Light beam height")]
    public float beamHeight = 5f;
    
    [Tooltip("Light beam radius")]
    public float beamRadius = 0.3f;
    
    [Tooltip("Beam brightness")]
    public float beamIntensity = 1f;
    
    [Header("Ground Ring")]
    [Tooltip("Show glowing ring on ground")]
    public bool showGroundRing = true;
    
    [Tooltip("Ground ring radius")]
    public float ringRadius = 1f;
    
    [Tooltip("Ring pulse speed")]
    public float ringPulseSpeed = 2f;
    
    [Header("Floating Icon")]
    [Tooltip("Show floating icon above item")]
    public bool showFloatingIcon = true;
    
    [Tooltip("Icon prefab (UI sprite)")]
    public GameObject iconPrefab;
    
    [Tooltip("Icon height above item")]
    public float iconHeight = 2f;
    
    [Tooltip("Icon size")]
    public float iconSize = 0.5f;
    
    [Header("Glow Effect")]
    [Tooltip("Add outer glow to item")]
    public bool showOuterGlow = true;
    
    [Tooltip("Glow intensity")]
    public float glowIntensity = 2f;
    
    [Tooltip("Glow pulse")]
    public bool pulseGlow = true;
    
    [Tooltip("Pulse speed")]
    public float pulseSpeed = 1f;
    
    [Header("Particle Effects")]
    [Tooltip("Spawn particle effect")]
    public bool showParticles = true;
    
    [Tooltip("Particle prefab")]
    public GameObject particlePrefab;
    
    [Header("Distance Visibility")]
    [Tooltip("Increase visibility at distance")]
    public bool scaleWithDistance = true;
    
    [Tooltip("Maximum visibility distance")]
    public float maxVisibilityDistance = 50f;
    
    [Header("Rarity Colors")]
    public bool useRarityColors = true;
    public LootManager.Rarity itemRarity = LootManager.Rarity.Common;
    
    private GameObject lightBeamObject;
    private GameObject groundRingObject;
    private GameObject floatingIconObject;
    private GameObject particleEffectObject;
    private Light beamLight;
    private Transform playerTransform;
    private MeshRenderer[] itemRenderers;
    private MaterialPropertyBlock propertyBlock;
    
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        itemRenderers = GetComponentsInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        if (showLightBeam)
        {
            CreateLightBeam();
        }
        
        if (showGroundRing)
        {
            CreateGroundRing();
        }
        
        if (showFloatingIcon && iconPrefab != null)
        {
            CreateFloatingIcon();
        }
        
        if (showParticles && particlePrefab != null)
        {
            CreateParticleEffect();
        }
        
        if (showOuterGlow)
        {
            ApplyGlowToRenderers();
        }
    }
    
    private void Update()
    {
        if (pulseGlow && showOuterGlow)
        {
            UpdateGlowPulse();
        }
        
        if (scaleWithDistance && playerTransform != null)
        {
            UpdateDistanceVisibility();
        }
        
        if (showGroundRing && groundRingObject != null)
        {
            UpdateGroundRingPulse();
        }
        
        if (showFloatingIcon && floatingIconObject != null)
        {
            UpdateFloatingIcon();
        }
    }
    
    private void CreateLightBeam()
    {
        lightBeamObject = new GameObject("LightBeam");
        lightBeamObject.transform.SetParent(transform);
        lightBeamObject.transform.localPosition = Vector3.up * (beamHeight / 2f);
        
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(lightBeamObject.transform);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localScale = new Vector3(beamRadius, beamHeight / 2f, beamRadius);
        
        Destroy(cylinder.GetComponent<Collider>());
        
        MeshRenderer renderer = cylinder.GetComponent<MeshRenderer>();
        Material beamMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        beamMaterial.SetFloat("_Surface", 1);
        beamMaterial.SetFloat("_Blend", 0);
        beamMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        beamMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        beamMaterial.SetInt("_ZWrite", 0);
        beamMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        beamMaterial.renderQueue = 3000;
        
        Color beamColor = GetRarityColor();
        beamColor.a = 0.3f;
        beamMaterial.SetColor(BaseColor, beamColor * beamIntensity);
        
        renderer.material = beamMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        beamLight = lightBeamObject.AddComponent<Light>();
        beamLight.type = LightType.Spot;
        beamLight.color = beamColor;
        beamLight.intensity = beamIntensity * 2f;
        beamLight.range = beamHeight;
        beamLight.spotAngle = 30f;
        beamLight.transform.localRotation = Quaternion.Euler(90, 0, 0);
        beamLight.shadows = LightShadows.None;
    }
    
    private void CreateGroundRing()
    {
        groundRingObject = new GameObject("GroundRing");
        groundRingObject.transform.SetParent(transform);
        groundRingObject.transform.localPosition = Vector3.up * 0.05f;
        groundRingObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.SetParent(groundRingObject.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(ringRadius * 2f, 0.02f, ringRadius * 2f);
        
        Destroy(ring.GetComponent<Collider>());
        
        MeshRenderer renderer = ring.GetComponent<MeshRenderer>();
        Material ringMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        ringMaterial.SetFloat("_Surface", 1);
        ringMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ringMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        ringMaterial.SetInt("_ZWrite", 0);
        ringMaterial.renderQueue = 3000;
        
        Color ringColor = GetRarityColor();
        ringColor.a = 0.5f;
        ringMaterial.SetColor(BaseColor, ringColor);
        
        renderer.material = ringMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    private void CreateFloatingIcon()
    {
        floatingIconObject = Instantiate(iconPrefab, transform);
        floatingIconObject.transform.localPosition = Vector3.up * iconHeight;
        floatingIconObject.transform.localScale = Vector3.one * iconSize;
    }
    
    private void CreateParticleEffect()
    {
        particleEffectObject = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
        
        ParticleSystem ps = particleEffectObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = GetRarityColor();
        }
    }
    
    private void ApplyGlowToRenderers()
    {
        Color glowColor = GetRarityColor();
        
        foreach (MeshRenderer renderer in itemRenderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                Material mat = renderer.material;
                
                if (mat.HasProperty(EmissionColor))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor(EmissionColor, glowColor * glowIntensity);
                }
            }
        }
    }
    
    private void UpdateGlowPulse()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float currentIntensity = Mathf.Lerp(glowIntensity * 0.5f, glowIntensity, pulse);
        Color glowColor = GetRarityColor();
        
        foreach (MeshRenderer renderer in itemRenderers)
        {
            if (renderer != null && renderer.material.HasProperty(EmissionColor))
            {
                renderer.material.SetColor(EmissionColor, glowColor * currentIntensity);
            }
        }
        
        if (beamLight != null)
        {
            beamLight.intensity = currentIntensity * 2f;
        }
    }
    
    private void UpdateGroundRingPulse()
    {
        float pulse = (Mathf.Sin(Time.time * ringPulseSpeed) + 1f) / 2f;
        float scale = Mathf.Lerp(ringRadius * 1.8f, ringRadius * 2.2f, pulse);
        
        Transform ring = groundRingObject.transform.GetChild(0);
        if (ring != null)
        {
            ring.localScale = new Vector3(scale, 0.02f, scale);
        }
    }
    
    private void UpdateFloatingIcon()
    {
        if (playerTransform != null)
        {
            floatingIconObject.transform.LookAt(playerTransform);
        }
        
        float bobOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
        floatingIconObject.transform.localPosition = Vector3.up * (iconHeight + bobOffset);
    }
    
    private void UpdateDistanceVisibility()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float visibilityFactor = Mathf.Clamp01(distance / maxVisibilityDistance);
        
        if (beamLight != null)
        {
            beamLight.range = Mathf.Lerp(beamHeight, beamHeight * 2f, visibilityFactor);
            beamLight.intensity = Mathf.Lerp(beamIntensity, beamIntensity * 3f, visibilityFactor);
        }
    }
    
    private Color GetRarityColor()
    {
        if (!useRarityColors)
        {
            return Color.yellow;
        }
        
        switch (itemRarity)
        {
            case LootManager.Rarity.Common:
                return new Color(0.8f, 0.8f, 0.8f);
            case LootManager.Rarity.Uncommon:
                return new Color(0.1f, 0.9f, 0.1f);
            case LootManager.Rarity.Rare:
                return new Color(0.2f, 0.5f, 1f);
            case LootManager.Rarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f);
            case LootManager.Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.yellow;
        }
    }
    
    private void OnDestroy()
    {
        if (lightBeamObject != null) Destroy(lightBeamObject);
        if (groundRingObject != null) Destroy(groundRingObject);
        if (floatingIconObject != null) Destroy(floatingIconObject);
        if (particleEffectObject != null) Destroy(particleEffectObject);
    }
}
