using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldPickupItem : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("The item data this pickup represents")]
    public LootItemData itemData;
    
    [Tooltip("Gear score of this specific item instance")]
    public int gearScore = 100;
    
    [Tooltip("Rarity of this specific item instance")]
    public LootManager.Rarity rarity = LootManager.Rarity.Common;
    
    [Header("Visual Settings")]
    [Tooltip("Should the item rotate in place?")]
    public bool rotateItem = true;
    
    [Tooltip("Rotation speed")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Should the item bob up and down?")]
    public bool bobItem = true;
    
    [Tooltip("Bob height")]
    public float bobHeight = 0.2f;
    
    [Tooltip("Bob speed")]
    public float bobSpeed = 2f;
    
    [Header("Highlight")]
    [Tooltip("Renderer to highlight when player is near")]
    public Renderer itemRenderer;
    
    [Tooltip("Highlight color based on rarity")]
    public bool useRarityColor = true;
    
    [Tooltip("Custom highlight color (if not using rarity)")]
    public Color customHighlightColor = Color.yellow;
    
    [Tooltip("Emission intensity")]
    public float emissionIntensity = 2f;
    
    [Header("Visibility Aids")]
    [Tooltip("Add advanced visibility helper")]
    public bool useAdvancedVisibility = false;
    
    [Tooltip("Simple outline glow")]
    public bool useSimpleOutline = true;
    
    [Tooltip("Outline thickness")]
    public float outlineThickness = 0.05f;
    
    private Vector3 startPosition;
    private MaterialPropertyBlock propertyBlock;
    private bool isHighlighted;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    private void Start()
    {
        startPosition = transform.position;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        if (itemRenderer == null)
        {
            itemRenderer = GetComponentInChildren<Renderer>();
        }
        
        propertyBlock = new MaterialPropertyBlock();
        
        if (itemData != null && itemData.worldPrefab != null && transform.childCount == 0)
        {
            Instantiate(itemData.worldPrefab, transform);
        }
        
        if (useAdvancedVisibility)
        {
            SetupAdvancedVisibility();
        }
        
        if (useSimpleOutline)
        {
            SetupSimpleOutline();
        }
    }
    
    private void Update()
    {
        if (rotateItem)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        if (bobItem)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    public void SetHighlight(bool highlight)
    {
        if (itemRenderer == null || isHighlighted == highlight) return;
        
        isHighlighted = highlight;
        
        if (highlight)
        {
            Color highlightColor = useRarityColor ? GetRarityColor() : customHighlightColor;
            itemRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColor, highlightColor * emissionIntensity);
            itemRenderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            itemRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColor, Color.black);
            itemRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    private Color GetRarityColor()
    {
        switch (rarity)
        {
            case LootManager.Rarity.Common:
                return Color.white;
            case LootManager.Rarity.Uncommon:
                return Color.green;
            case LootManager.Rarity.Rare:
                return Color.blue;
            case LootManager.Rarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f);
            case LootManager.Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
    
    private void SetupAdvancedVisibility()
    {
        LootVisibilityHelper helper = gameObject.AddComponent<LootVisibilityHelper>();
        helper.itemRarity = rarity;
        helper.useRarityColors = useRarityColor;
        helper.showLightBeam = true;
        helper.showGroundRing = true;
        helper.showOuterGlow = true;
        helper.pulseGlow = true;
    }
    
    private void SetupSimpleOutline()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        Color outlineColor = GetRarityColor();
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Material mat = renderer.material;
                
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", outlineColor * emissionIntensity);
                }
            }
        }
    }
    
    public static WorldPickupItem Create(LootItemData itemData, int gearScore, LootManager.Rarity rarity, Vector3 position)
    {
        GameObject pickupObject = new GameObject($"Pickup_{itemData.itemName}");
        pickupObject.transform.position = position;
        pickupObject.layer = LayerMask.NameToLayer("Item");
        
        SphereCollider collider = pickupObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        WorldPickupItem pickup = pickupObject.AddComponent<WorldPickupItem>();
        pickup.itemData = itemData;
        pickup.gearScore = gearScore;
        pickup.rarity = rarity;
        
        if (itemData.worldPrefab != null)
        {
            Instantiate(itemData.worldPrefab, pickupObject.transform);
        }
        else
        {
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(pickupObject.transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localScale = Vector3.one * 0.5f;
            Destroy(visualCube.GetComponent<Collider>());
        }
        
        return pickup;
    }
}
