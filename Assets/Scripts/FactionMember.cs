using UnityEngine;

public class FactionMember : MonoBehaviour
{
    [Header("Faction Settings")]
    public FactionManager.Faction faction = FactionManager.Faction.Neutral;
    
    [Header("Debug")]
    public bool showFactionGizmo = false;
    public float gizmoRadius = 1f;
    
    private FactionManager factionManager;
    
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            factionManager = GameManager.Instance.factionManager;
        }
    }
    
    public bool IsEnemyOf(FactionMember other)
    {
        if (other == null) return false;
        if (factionManager == null) return false;
        
        return factionManager.AreEnemies(faction, other.faction);
    }
    
    public bool IsAllyOf(FactionMember other)
    {
        if (other == null) return false;
        if (factionManager == null) return faction == other.faction;
        
        return factionManager.AreAllies(faction, other.faction);
    }
    
    public bool IsNeutralTo(FactionMember other)
    {
        if (other == null) return true;
        if (factionManager == null) return faction != other.faction;
        
        return factionManager.IsNeutral(faction, other.faction);
    }
    
    public void ChangeFaction(FactionManager.Faction newFaction)
    {
        FactionManager.Faction oldFaction = faction;
        faction = newFaction;
        
        Debug.Log($"{gameObject.name} changed faction from {oldFaction} to {newFaction}");
    }
    
    private void OnDrawGizmos()
    {
        if (!showFactionGizmo) return;
        
        Color factionColor = GetFactionColor();
        Gizmos.color = factionColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, gizmoRadius);
    }
    
    private Color GetFactionColor()
    {
        switch (faction)
        {
            case FactionManager.Faction.Player:
                return Color.green;
            case FactionManager.Faction.Rogue:
                return Color.red;
            case FactionManager.Faction.Civilian:
                return Color.blue;
            case FactionManager.Faction.Neutral:
                return Color.yellow;
            case FactionManager.Faction.Enemy:
                return new Color(1f, 0.5f, 0f);
            case FactionManager.Faction.Friendly:
                return Color.cyan;
            default:
                return Color.white;
        }
    }
}
