using UnityEngine;

public class LootableBox : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField] private LootManager.Rarity minRarity = LootManager.Rarity.Common;
    [SerializeField] private LootManager.Rarity maxRarity = LootManager.Rarity.Rare;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 3;
    [SerializeField] private int xpReward = 50;
    
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private string interactionPrompt = "Press E to Open";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject openedModel;
    [SerializeField] private GameObject closedModel;
    [SerializeField] private ParticleSystem lootParticles;
    [SerializeField] private AudioClip openSound;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionUI;
    
    private bool isOpened = false;
    private Transform playerTransform;
    private AudioSource audioSource;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        UpdateVisuals();
    }
    
    private void Update()
    {
        if (isOpened || playerTransform == null)
        {
            if (interactionUI != null && interactionUI.activeSelf)
            {
                interactionUI.SetActive(false);
            }
            return;
        }
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionDistance;
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(inRange);
        }
        
        if (inRange && Input.GetKeyDown(interactionKey))
        {
            OpenBox();
        }
    }
    
    public void OpenBox()
    {
        if (isOpened) return;
        
        isOpened = true;
        
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        if (lootParticles != null)
        {
            lootParticles.Play();
        }
        
        UpdateVisuals();
        GiveLoot();
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void UpdateVisuals()
    {
        if (closedModel != null)
        {
            closedModel.SetActive(!isOpened);
        }
        
        if (openedModel != null)
        {
            openedModel.SetActive(isOpened);
        }
    }
    
    private void GiveLoot()
    {
        LootManager lootManager = FindObjectOfType<LootManager>();
        if (lootManager == null)
        {
            Debug.LogWarning("LootableBox: LootManager not found in scene!");
            return;
        }
        
        int itemCount = Random.Range(minItems, maxItems + 1);
        int playerLevel = 1;
        
        if (GameManager.Instance != null)
        {
            playerLevel = GameManager.Instance.currentPlayerLevel;
        }
        else
        {
            ProgressionManager progressionManager = FindObjectOfType<ProgressionManager>();
            if (progressionManager != null)
            {
                playerLevel = progressionManager.currentLevel;
            }
        }
        
        for (int i = 0; i < itemCount; i++)
        {
            LootManager.Rarity rarity = GetRandomRarity();
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            lootManager.DropLootWithRarity(dropPosition, playerLevel, rarity);
        }
        
        ProgressionManager progManager = FindObjectOfType<ProgressionManager>();
        if (progManager != null && xpReward > 0)
        {
            progManager.AddExperience(xpReward);
        }
        
        Debug.Log($"Opened loot box! Received {itemCount} items and {xpReward} XP");
    }
    
    private LootManager.Rarity GetRandomRarity()
    {
        int minValue = (int)minRarity;
        int maxValue = (int)maxRarity;
        int randomValue = Random.Range(minValue, maxValue + 1);
        return (LootManager.Rarity)randomValue;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
