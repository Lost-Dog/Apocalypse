using UnityEngine;
using JUTPS;
using JU.CharacterSystem.AI;

public class JUTPSFactionIntegration : MonoBehaviour
{
    [Header("References")]
    public FactionMember factionMember;
    
    [Header("AI Components")]
    public JU_AI_PatrolCharacter patrolAI;
    public JU_AI_Zombie zombieAI;
    
    [Header("Settings")]
    public bool autoConfigureTargetLayers = true;
    public bool debugTargetDetection = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void OnValidate()
    {
        if (factionMember == null)
        {
            factionMember = GetComponent<FactionMember>();
        }
        
        if (patrolAI == null)
        {
            patrolAI = GetComponent<JU_AI_PatrolCharacter>();
        }
        
        if (zombieAI == null)
        {
            zombieAI = GetComponent<JU_AI_Zombie>();
        }
    }
    
    private void Initialize()
    {
        if (factionMember == null)
        {
            Debug.LogWarning($"{gameObject.name}: No FactionMember component found!", this);
            return;
        }
        
        if (autoConfigureTargetLayers)
        {
            ConfigureTargetLayers();
        }
    }
    
    private void ConfigureTargetLayers()
    {
        LayerMask targetLayers = GetTargetLayersForFaction();
        
        if (patrolAI != null)
        {
            patrolAI.FieldOfView.TargetsLayer = targetLayers;
            
            if (debugTargetDetection)
            {
                Debug.Log($"{gameObject.name} (Patrol AI) configured to target layers: {LayerMaskToString(targetLayers)}", this);
            }
        }
        
        if (zombieAI != null)
        {
            zombieAI.FieldOfView.TargetsLayer = targetLayers;
            
            if (debugTargetDetection)
            {
                Debug.Log($"{gameObject.name} (Zombie AI) configured to target layers: {LayerMaskToString(targetLayers)}", this);
            }
        }
    }
    
    private LayerMask GetTargetLayersForFaction()
    {
        switch (factionMember.faction)
        {
            case FactionManager.Faction.Enemy:
                return LayerMask.GetMask("Player", "Character");
                
            case FactionManager.Faction.Friendly:
                return LayerMask.GetMask("Enemy", "Character");
                
            case FactionManager.Faction.Rogue:
                return LayerMask.GetMask("Player", "Agent", "Character");
                
            case FactionManager.Faction.Player:
                return LayerMask.GetMask("Enemy", "Rogue", "Character");
                
            case FactionManager.Faction.Civilian:
                return LayerMask.GetMask("Enemy", "Rogue");
                
            default:
                return LayerMask.GetMask("Player");
        }
    }
    
    private string LayerMaskToString(LayerMask mask)
    {
        string result = "";
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ", ";
                result += LayerMask.LayerToName(i);
            }
        }
        return string.IsNullOrEmpty(result) ? "None" : result;
    }
    
    public bool IsValidTarget(GameObject target)
    {
        if (target == null || factionMember == null)
            return false;
        
        FactionMember targetFaction = target.GetComponent<FactionMember>();
        
        if (targetFaction == null)
            return false;
        
        bool isEnemy = factionMember.IsEnemyOf(targetFaction);
        
        if (debugTargetDetection)
        {
            Debug.Log($"{gameObject.name} checking {target.name}: IsEnemy={isEnemy}, MyFaction={factionMember.faction}, TheirFaction={targetFaction.faction}", this);
        }
        
        return isEnemy;
    }
    
    public void OnTargetDetected(GameObject target)
    {
        if (!IsValidTarget(target))
        {
            return;
        }
        
        if (debugTargetDetection)
        {
            Debug.Log($"{gameObject.name} detected valid enemy target: {target.name}", this);
        }
    }
}
