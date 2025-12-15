using UnityEngine;

public class LootItem : MonoBehaviour
{
    [Header("Item Data")]
    public LootItemData itemData;
    public int gearScore;
    public LootManager.Rarity rarity;
    
    [Header("Pickup Settings")]
    [Tooltip("Enable automatic pickup on collision")]
    public bool autoPickupOnCollision = true;
    [Tooltip("Delay before item can be picked up (prevents instant pickup on spawn)")]
    public float pickupDelay = 0.5f;
    
    [Header("Visual")]
    public GameObject visualEffect;
    public Light rarityLight;
    [Tooltip("Height of bobbing animation (0 = no bobbing, DISABLED for ground physics)")]
    public float bobHeight = 0f;
    [Tooltip("Speed of bobbing animation")]
    public float bobSpeed = 2f;
    [Tooltip("Enable gentle rotation animation")]
    public bool enableRotation = false;
    
    private Vector3 startPosition;
    private bool isPickedUp = false;
    private float spawnTime;
    
    private void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;
        
        SetupCollider();
        SetupVisuals();
    }
    
    private void Update()
    {
        if (isPickedUp) return;
        
        BobAnimation();
    }
    
    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            sphereCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        Collider[] allColliders = GetComponents<Collider>();
        bool hasPhysicsCollider = false;
        
        foreach (Collider c in allColliders)
        {
            if (!c.isTrigger)
            {
                hasPhysicsCollider = true;
                break;
            }
        }
        
        if (!hasPhysicsCollider)
        {
            BoxCollider physicsCol = gameObject.AddComponent<BoxCollider>();
            physicsCol.size = new Vector3(0.5f, 0.5f, 0.5f);
            physicsCol.isTrigger = false;
        }
    }
    
    private void BobAnimation()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        bool isKinematic = rb != null && rb.isKinematic;
        
        if (bobHeight > 0f && isKinematic)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        if (enableRotation && isKinematic)
        {
            transform.Rotate(Vector3.up * (30f * Time.deltaTime));
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickupOnCollision || isPickedUp) return;
        
        if (Time.time < spawnTime + pickupDelay) return;
        
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }
    
    private void SetupVisuals()
    {
        if (rarityLight != null)
        {
            Color rarityColor = GetRarityColor();
            rarityLight.color = rarityColor;
        }
    }
    
    private Color GetRarityColor()
    {
        if (GameManager.Instance != null && GameManager.Instance.lootManager != null)
        {
            return GameManager.Instance.lootManager.GetRarityColor(rarity);
        }
        
        return Color.white;
    }
    
    public void Initialize(LootItemData data, int score, LootManager.Rarity itemRarity)
    {
        itemData = data;
        gearScore = score;
        rarity = itemRarity;
        
        if (itemData != null)
        {
            gameObject.name = $"{itemData.itemName} (GS {gearScore})";
        }
        
        SetupVisuals();
    }
    
    public void Pickup()
    {
        if (isPickedUp) return;
        
        isPickedUp = true;
        
        if (GameManager.Instance != null && GameManager.Instance.lootManager != null)
        {
            GameManager.Instance.lootManager.AddItemToInventory(itemData, gearScore, rarity);
        }
        
        if (visualEffect != null)
        {
            Instantiate(visualEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"<color=yellow>Picked up {itemData?.itemName ?? "Unknown Item"} (GS {gearScore}) - {rarity}</color>");
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            
            if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(transform.position, box.size);
            }
        }
    }
}
