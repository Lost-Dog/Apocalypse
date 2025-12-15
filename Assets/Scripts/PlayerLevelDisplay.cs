using UnityEngine;
using TMPro;

public class PlayerLevelDisplay : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;
    public TextMeshProUGUI levelText;
    
    [Header("Display Settings")]
    public bool showPrefix = false;
    public string prefix = "Level: ";
    
    [Header("Auto-Find")]
    public bool autoFindReferences = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private void OnEnable()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        UpdateDisplay();
    }
    
    private void Start()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }
        
        UpdateDisplay();
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>PlayerLevelDisplay initialized:</color>");
            Debug.Log($"  ProgressionManager: {(progressionManager != null ? "Found" : "Missing")}");
            Debug.Log($"  LevelText: {(levelText != null ? "Found" : "Missing")}");
            if (progressionManager != null)
            {
                Debug.Log($"  Current Level: {progressionManager.currentLevel}");
            }
        }
    }
    
    private void FindReferences()
    {
        if (levelText == null)
        {
            levelText = GetComponent<TextMeshProUGUI>();
            if (levelText == null)
            {
                Debug.LogWarning("PlayerLevelDisplay: No TextMeshProUGUI component found on this GameObject!");
                return;
            }
            else if (showDebugInfo)
            {
                Debug.Log("<color=green>PlayerLevelDisplay: Found TextMeshProUGUI component</color>");
            }
        }
        
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<ProgressionManager>();
            if (progressionManager == null)
            {
                Debug.LogWarning("PlayerLevelDisplay: Could not find ProgressionManager in scene!");
            }
            else if (showDebugInfo)
            {
                Debug.Log($"<color=green>PlayerLevelDisplay: Found ProgressionManager (Level: {progressionManager.currentLevel})</color>");
            }
        }
    }
    
    private void Update()
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (levelText == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("PlayerLevelDisplay: levelText is null!");
            }
            return;
        }
        
        if (progressionManager == null)
        {
            if (autoFindReferences)
            {
                FindReferences();
            }
            
            if (progressionManager == null)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("PlayerLevelDisplay: progressionManager is null!");
                }
                return;
            }
        }
        
        string displayText;
        if (showPrefix)
        {
            displayText = $"{prefix}{progressionManager.currentLevel}";
        }
        else
        {
            displayText = progressionManager.currentLevel.ToString();
        }
        
        if (levelText.text != displayText)
        {
            levelText.text = displayText;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=yellow>PlayerLevelDisplay updated: {displayText}</color>");
            }
        }
    }
}
