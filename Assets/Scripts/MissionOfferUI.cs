using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionOfferUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel that shows when a mission is offered")]
    public GameObject offerPanel;
    
    [Tooltip("Mission name text")]
    public TextMeshProUGUI missionNameText;
    
    [Tooltip("Mission description text")]
    public TextMeshProUGUI descriptionText;
    
    [Tooltip("Level requirement text")]
    public TextMeshProUGUI levelRequirementText;
    
    [Tooltip("Rewards text")]
    public TextMeshProUGUI rewardsText;
    
    [Tooltip("Accept button")]
    public Button acceptButton;
    
    [Tooltip("Distance to base text")]
    public TextMeshProUGUI distanceText;
    
    [Header("Display Settings")]
    [Tooltip("Show panel when mission is offered")]
    public bool autoShowOnOffer = true;
    
    [Tooltip("Show distance indicator when not at base")]
    public bool showDistanceIndicator = true;
    
    [Tooltip("Update interval for distance check")]
    public float updateInterval = 0.5f;
    
    private MissionData currentOffer;
    private float updateTimer;
    
    private void Start()
    {
        if (MissionOfferManager.Instance != null)
        {
            MissionOfferManager.Instance.onMissionOffered.AddListener(OnMissionOffered);
            MissionOfferManager.Instance.onMissionAccepted.AddListener(OnMissionAccepted);
        }
        
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
        }
        
        if (offerPanel != null)
        {
            offerPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (currentOffer != null && offerPanel != null && offerPanel.activeSelf)
        {
            updateTimer -= Time.deltaTime;
            
            if (updateTimer <= 0f)
            {
                UpdateUI();
                updateTimer = updateInterval;
            }
        }
    }
    
    private void OnMissionOffered(MissionData mission)
    {
        currentOffer = mission;
        
        if (autoShowOnOffer)
        {
            ShowOffer();
        }
    }
    
    private void OnMissionAccepted(MissionData mission)
    {
        HideOffer();
    }
    
    public void ShowOffer()
    {
        if (currentOffer == null && MissionOfferManager.Instance != null)
        {
            currentOffer = MissionOfferManager.Instance.offeredMission;
        }
        
        if (currentOffer == null)
        {
            Debug.LogWarning("MissionOfferUI: No mission to show");
            return;
        }
        
        if (offerPanel != null)
        {
            offerPanel.SetActive(true);
        }
        
        UpdateUI();
        updateTimer = updateInterval;
    }
    
    public void HideOffer()
    {
        if (offerPanel != null)
        {
            offerPanel.SetActive(false);
        }
        
        currentOffer = null;
    }
    
    private void UpdateUI()
    {
        if (currentOffer == null) return;
        
        if (missionNameText != null)
        {
            missionNameText.text = currentOffer.missionName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = currentOffer.description;
        }
        
        if (levelRequirementText != null)
        {
            levelRequirementText.text = $"Required Level: {currentOffer.levelRequirement}";
            
            if (GameManager.Instance != null)
            {
                bool meetsRequirement = GameManager.Instance.currentPlayerLevel >= currentOffer.levelRequirement;
                levelRequirementText.color = meetsRequirement ? Color.green : Color.red;
            }
        }
        
        if (rewardsText != null)
        {
            rewardsText.text = $"Rewards:\n{currentOffer.xpReward} XP\n{currentOffer.currencyReward} Credits";
        }
        
        UpdateDistanceAndButton();
    }
    
    private void UpdateDistanceAndButton()
    {
        if (MissionOfferManager.Instance == null) return;
        
        bool isAtBase = MissionOfferManager.Instance.IsPlayerAtBase();
        float distance = MissionOfferManager.Instance.GetDistanceToBase();
        
        if (acceptButton != null)
        {
            acceptButton.interactable = isAtBase;
        }
        
        if (distanceText != null && showDistanceIndicator)
        {
            if (isAtBase)
            {
                distanceText.text = "<color=green>At Base - Press to Accept</color>";
            }
            else
            {
                distanceText.text = $"<color=yellow>Distance to Base: {distance:F0}m</color>";
            }
            
            distanceText.gameObject.SetActive(true);
        }
    }
    
    private void OnAcceptButtonClicked()
    {
        if (MissionOfferManager.Instance != null)
        {
            MissionOfferManager.Instance.TryAcceptMission();
        }
    }
    
    public void TogglePanel()
    {
        if (offerPanel != null)
        {
            if (offerPanel.activeSelf)
            {
                HideOffer();
            }
            else
            {
                ShowOffer();
            }
        }
    }
}
