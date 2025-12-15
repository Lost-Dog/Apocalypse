using UnityEngine;

public class ChallengeMinimapPointer : MonoBehaviour
{
    [Header("Challenge Settings")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color extremeColor = Color.red;
    [SerializeField] private float iconSize = 25f;
    [SerializeField] private bool showOffScreen = true;
    [SerializeField] private Vector3 worldOffset = Vector3.up * 2f;
    
    private ActiveChallenge linkedChallenge;
    private bl_MiniMapEntity minimapEntity;
    
    public void SetChallenge(ActiveChallenge challenge)
    {
        linkedChallenge = challenge;
        
        if (challenge.challengeData.iconData == null)
        {
            Debug.LogWarning($"Challenge '{challenge.challengeData.challengeName}' has no iconData assigned. Minimap pointer will not display.");
            Destroy(gameObject);
            return;
        }
        
        minimapEntity = gameObject.AddComponent<bl_MiniMapEntity>();
        minimapEntity.Target = transform;
        minimapEntity.OffSet = worldOffset;
        minimapEntity.iconData = challenge.challengeData.iconData;
        minimapEntity.OffScreen = showOffScreen;
        minimapEntity.isInteractable = false;
        minimapEntity.DestroyWithObject = true;
        minimapEntity.enableDisableWithTarget = true;
        
        UpdateMarkerAppearance();
    }
    
    private void LateUpdate()
    {
        if (linkedChallenge == null || linkedChallenge.isCompleted)
        {
            Destroy(gameObject);
            return;
        }
        
        transform.position = linkedChallenge.position;
    }
    
    private void UpdateMarkerAppearance()
    {
        if (linkedChallenge == null || linkedChallenge.challengeData.iconData == null)
            return;
        
        Color iconColor = GetDifficultyColor(linkedChallenge.challengeData.difficulty);
        
        linkedChallenge.challengeData.iconData.IconColor = iconColor;
        linkedChallenge.challengeData.iconData.Size = iconSize;
    }
    
    private Color GetDifficultyColor(ChallengeData.ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeData.ChallengeDifficulty.Easy:
                return easyColor;
            case ChallengeData.ChallengeDifficulty.Medium:
                return mediumColor;
            case ChallengeData.ChallengeDifficulty.Hard:
                return hardColor;
            case ChallengeData.ChallengeDifficulty.Extreme:
                return extremeColor;
            default:
                return Color.white;
        }
    }
}
