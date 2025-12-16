using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI Managers")]
    [SerializeField] private MissionUIManager missionUIManager;
    [SerializeField] private ProgressionUIManager progressionUIManager;
    [SerializeField] private LootUIManager lootUIManager;
    [SerializeField] private ChallengeNotificationUI challengeNotificationUI;
    
    [Header("Status Warnings")]
    [SerializeField] private GameObject statsWarningsPanel;
    
    [Header("Health Display")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Compass")]
    [SerializeField] private GameObject compassPanel;
    [SerializeField] private RectTransform compassContent;
    [SerializeField] private bool showCompassOnStart = true;
    
    [Header("Event Log")]
    [SerializeField] private GameObject eventLogPanel;
    [SerializeField] private TextMeshProUGUI eventLogText;
    
    [Header("General Settings")]
    [SerializeField] private bool showDebugInfo = false;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.LogWarning("HUDManager already initialized!");
            return;
        }
        
        gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("HUDManager: GameManager instance not found!");
            return;
        }
        
        InitializeUIManagers();
        
        if (showCompassOnStart && compassPanel != null)
        {
            compassPanel.SetActive(true);
        }
        
        // Always show Stats_Warnings panel
        if (statsWarningsPanel != null)
        {
            statsWarningsPanel.SetActive(true);
        }
        
        isInitialized = true;
        Debug.Log("HUDManager initialized successfully");
    }
    
    private void InitializeUIManagers()
    {
        if (missionUIManager != null && gameManager.missionManager != null)
        {
            missionUIManager.Initialize(gameManager.missionManager);
        }
        else if (missionUIManager == null)
        {
            Debug.LogWarning("MissionUIManager not assigned in HUDManager!");
        }
        
        if (progressionUIManager != null && gameManager.progressionManager != null)
        {
            progressionUIManager.Initialize(gameManager.progressionManager);
        }
        else if (progressionUIManager == null)
        {
            Debug.LogWarning("ProgressionUIManager not assigned in HUDManager!");
        }
        
        if (lootUIManager != null && gameManager.lootManager != null)
        {
            lootUIManager.Initialize(gameManager.lootManager);
        }
        else if (lootUIManager == null)
        {
            Debug.LogWarning("LootUIManager not assigned in HUDManager!");
        }
        
        if (challengeNotificationUI != null && ChallengeManager.Instance != null)
        {
            challengeNotificationUI.Initialize(ChallengeManager.Instance);
        }
        else if (challengeNotificationUI == null)
        {
            Debug.LogWarning("ChallengeNotificationUI not assigned in HUDManager!");
        }
    }
    
    public void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}";
        }
    }
    
    public void ShowEventMessage(string message, Color color)
    {
        if (eventLogText != null)
        {
            eventLogText.text = message;
            eventLogText.color = color;
        }
    }
    
    public void ShowEventMessage(string message)
    {
        ShowEventMessage(message, Color.white);
    }
    
    public void ClearEventLog()
    {
        if (eventLogText != null)
        {
            eventLogText.text = "";
        }
    }
    
    public void ToggleCompass(bool show)
    {
        if (compassPanel != null)
        {
            compassPanel.SetActive(show);
        }
    }
    
    public void UpdateCompass(float rotation)
    {
        if (compassContent != null)
        {
            Vector3 currentRotation = compassContent.localEulerAngles;
            currentRotation.z = rotation;
            compassContent.localEulerAngles = currentRotation;
        }
    }
    
    public void ShowHUD()
    {
        gameObject.SetActive(true);
    }
    
    public void HideHUD()
    {
        gameObject.SetActive(false);
    }
    
    public void ToggleHUD()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    public void RefreshAllUI()
    {
        if (progressionUIManager != null)
        {
            progressionUIManager.UpdateUI();
        }
        
        if (missionUIManager != null)
        {
            missionUIManager.ManualUpdateMissionUI();
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("HUD Debug Info");
        
        if (gameManager != null)
        {
            GUILayout.Label($"GameManager: OK");
            GUILayout.Label($"Player Level: {gameManager.currentPlayerLevel}");
            
            if (gameManager.missionManager != null)
            {
                GUILayout.Label($"Active Mission: {(gameManager.missionManager.activeMission != null ? gameManager.missionManager.activeMission.missionName : "None")}");
            }
            
            if (gameManager.progressionManager != null)
            {
                GUILayout.Label($"XP: {gameManager.progressionManager.currentXP}");
                GUILayout.Label($"Skill Points: {gameManager.progressionManager.skillPoints}");
            }
        }
        else
        {
            GUILayout.Label("GameManager: NULL");
        }
        
        GUILayout.EndArea();
    }
    
    public MissionUIManager GetMissionUIManager()
    {
        return missionUIManager;
    }
    
    public ProgressionUIManager GetProgressionUIManager()
    {
        return progressionUIManager;
    }
    
    public LootUIManager GetLootUIManager()
    {
        return lootUIManager;
    }
    
    public ChallengeNotificationUI GetChallengeNotificationUI()
    {
        return challengeNotificationUI;
    }
}
