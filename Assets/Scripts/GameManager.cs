using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Core Managers")]
    public MissionManager missionManager;
    public ChallengeManager challengeManager;
    public FactionManager factionManager;
    public ProgressionManager progressionManager;
    public LootManager lootManager;
    public SkillManager skillManager;
    public HUDManager hudManager;
    
    [Header("Player State")]
    public int currentPlayerLevel = 1;
    public int currentGearScore = 100;
    
    [Header("Game Events")]
    public UnityEvent onGameStart;
    public UnityEvent onGamePause;
    public UnityEvent onGameResume;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        if (factionManager != null) factionManager.Initialize();
        if (missionManager != null) missionManager.Initialize();
        if (challengeManager != null) challengeManager.Initialize();
        if (skillManager != null) skillManager.Initialize();
        if (hudManager != null) hudManager.Initialize();
        
        onGameStart?.Invoke();
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
        onGamePause?.Invoke();
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        onGameResume?.Invoke();
    }
    
    public void UpdatePlayerLevel(int newLevel)
    {
        currentPlayerLevel = newLevel;
    }
    
    public void UpdateGearScore(int newGearScore)
    {
        currentGearScore = Mathf.Max(currentGearScore, newGearScore);
    }
}
