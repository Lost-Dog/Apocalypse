using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LootUIManager : MonoBehaviour
{
    [Header("Loot Notification")]
    [SerializeField] private GameObject lootNotificationPanel;
    [SerializeField] private TextMeshProUGUI lootRarityText;
    [SerializeField] private TextMeshProUGUI lootGearScoreText;
    [SerializeField] private Image lootBackgroundImage;
    [SerializeField] private float notificationDisplayDuration = 3f;
    
    [Header("Panel Behavior")]
    [Tooltip("Should the entire UI Manager start inactive?")]
    [SerializeField] private bool startInactive = false;
    
    [Header("Audio")]
    [Tooltip("Audio source for playing loot sounds")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Sound to play when loot is collected")]
    [SerializeField] private AudioClip lootCollectedSound;
    
    [Header("Event Log")]
    [SerializeField] private GameObject eventLogPanel;
    [SerializeField] private TextMeshProUGUI eventLogText;
    
    private LootManager lootManager;
    private float notificationTimer = 0f;
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (startInactive)
        {
            gameObject.SetActive(false);
        }
    }
    
    public void Initialize(LootManager manager)
    {
        lootManager = manager;
        
        if (lootManager != null)
        {
            lootManager.onItemCollected.AddListener(OnItemCollected);
        }
        
        if (lootNotificationPanel != null)
        {
            lootNotificationPanel.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (lootManager != null)
        {
            lootManager.onItemCollected.RemoveListener(OnItemCollected);
        }
    }
    
    private void Update()
    {
        if (notificationTimer > 0)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0)
            {
                HideLootNotification();
            }
        }
    }
    
    private void OnItemCollected(LootItemData itemData, int gearScore, LootManager.Rarity rarity)
    {
        ShowLootNotification(itemData, rarity, gearScore);
        LogLootEvent(itemData, rarity, gearScore);
    }
    
    private void ShowLootNotification(LootItemData itemData, LootManager.Rarity rarity, int gearScore)
    {
        if (lootNotificationPanel != null)
        {
            lootNotificationPanel.SetActive(true);
        }
        
        if (lootRarityText != null)
        {
            string itemName = itemData != null ? itemData.itemName : "Item";
            lootRarityText.text = $"{itemName}";
            lootRarityText.color = GetRarityColor(rarity);
        }
        
        if (lootGearScoreText != null)
        {
            lootGearScoreText.text = $"GS {gearScore}";
            lootGearScoreText.color = GetRarityColor(rarity);
        }
        
        if (lootBackgroundImage != null)
        {
            Color rarityColor = GetRarityColor(rarity);
            rarityColor.a = 0.3f;
            lootBackgroundImage.color = rarityColor;
        }
        
        PlayLootSound();
        
        notificationTimer = notificationDisplayDuration;
    }
    
    private void HideLootNotification()
    {
        if (lootNotificationPanel != null)
        {
            lootNotificationPanel.SetActive(false);
        }
    }
    
    private void LogLootEvent(LootItemData itemData, LootManager.Rarity rarity, int gearScore)
    {
        if (eventLogPanel == null || eventLogText == null) return;
        
        string itemName = itemData != null ? itemData.itemName : "Item";
        string rarityName = GetRarityDisplayName(rarity);
        
        string logMessage = $"Picked up: <color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(rarity))}>{itemName}</color> ({rarityName} GS {gearScore})";
        
        eventLogText.text = logMessage;
        
        StartCoroutine(FadeOutEventLog());
    }
    
    private IEnumerator FadeOutEventLog()
    {
        yield return new WaitForSeconds(3f);
        
        if (eventLogText != null)
        {
            eventLogText.text = "";
        }
    }
    
    private Color GetRarityColor(LootManager.Rarity rarity)
    {
        if (lootManager != null)
        {
            return lootManager.GetRarityColor(rarity);
        }
        
        switch (rarity)
        {
            case LootManager.Rarity.Common:
                return Color.white;
            case LootManager.Rarity.Uncommon:
                return Color.green;
            case LootManager.Rarity.Rare:
                return Color.blue;
            case LootManager.Rarity.Epic:
                return new Color(0.6f, 0f, 1f);
            case LootManager.Rarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
    
    private string GetRarityDisplayName(LootManager.Rarity rarity)
    {
        if (lootManager != null)
        {
            return lootManager.GetRarityName(rarity);
        }
        
        return rarity.ToString();
    }
    
    public void ShowLootPickupFeedback(string itemName, LootManager.Rarity rarity)
    {
        if (eventLogPanel == null || eventLogText == null) return;
        
        string message = $"Picked up: <color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(rarity))}>{itemName}</color>";
        eventLogText.text = message;
        
        StartCoroutine(FadeOutEventLog());
    }
    
    private void PlayLootSound()
    {
        if (audioSource != null && lootCollectedSound != null)
        {
            audioSource.PlayOneShot(lootCollectedSound);
        }
    }
}
