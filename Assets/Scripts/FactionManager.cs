using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public enum Faction
    {
        Player,
        Rogue,
        Civilian,
        Neutral,
        Enemy,
        Friendly
    }
    
    [Header("Faction Settings")]
    public bool debugFactionRelations = false;
    
    private Dictionary<Faction, List<Faction>> enemyRelations;
    private Dictionary<Faction, List<Faction>> friendlyRelations;
    
    public void Initialize()
    {
        SetupFactionRelations();
        
        if (debugFactionRelations)
        {
            LogFactionRelations();
        }
    }
    
    private void SetupFactionRelations()
    {
        enemyRelations = new Dictionary<Faction, List<Faction>>
        {
            { Faction.Player, new List<Faction> { Faction.Rogue, Faction.Enemy } },
            { Faction.Rogue, new List<Faction> { Faction.Player, Faction.Friendly } },
            { Faction.Civilian, new List<Faction>() },
            { Faction.Neutral, new List<Faction>() },
            { Faction.Enemy, new List<Faction> { Faction.Player, Faction.Friendly } },
            { Faction.Friendly, new List<Faction> { Faction.Rogue, Faction.Enemy } }
        };
        
        friendlyRelations = new Dictionary<Faction, List<Faction>>
        {
            { Faction.Player, new List<Faction> { Faction.Player, Faction.Civilian, Faction.Friendly } },
            { Faction.Rogue, new List<Faction> { Faction.Rogue, Faction.Enemy } },
            { Faction.Civilian, new List<Faction> { Faction.Player, Faction.Civilian, Faction.Friendly } },
            { Faction.Neutral, new List<Faction> { Faction.Neutral } },
            { Faction.Enemy, new List<Faction> { Faction.Enemy, Faction.Rogue } },
            { Faction.Friendly, new List<Faction> { Faction.Friendly, Faction.Player, Faction.Civilian } }
        };
    }
    
    public bool AreEnemies(Faction a, Faction b)
    {
        if (!enemyRelations.ContainsKey(a)) return false;
        return enemyRelations[a].Contains(b);
    }
    
    public bool AreAllies(Faction a, Faction b)
    {
        if (a == b) return true;
        if (!friendlyRelations.ContainsKey(a)) return false;
        return friendlyRelations[a].Contains(b);
    }
    
    public bool IsNeutral(Faction a, Faction b)
    {
        return !AreEnemies(a, b) && !AreAllies(a, b);
    }
    
    public void SetFactionRelation(Faction a, Faction b, bool areEnemies)
    {
        if (!enemyRelations.ContainsKey(a))
        {
            enemyRelations[a] = new List<Faction>();
        }
        
        if (areEnemies)
        {
            if (!enemyRelations[a].Contains(b))
            {
                enemyRelations[a].Add(b);
            }
            
            if (friendlyRelations.ContainsKey(a) && friendlyRelations[a].Contains(b))
            {
                friendlyRelations[a].Remove(b);
            }
        }
        else
        {
            if (enemyRelations[a].Contains(b))
            {
                enemyRelations[a].Remove(b);
            }
        }
    }
    
    private void LogFactionRelations()
    {
        Debug.Log("=== Faction Relations ===");
        foreach (var faction in enemyRelations.Keys)
        {
            string enemies = string.Join(", ", enemyRelations[faction]);
            Debug.Log($"{faction} is hostile to: {(string.IsNullOrEmpty(enemies) ? "None" : enemies)}");
        }
    }
}
