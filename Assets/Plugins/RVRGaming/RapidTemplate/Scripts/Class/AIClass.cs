using System.Linq;
using UnityEngine;

public class AIClass : MonoBehaviour
{
    public enum Archetype
    {
        Assault,
        Rusher,
        Tank,
        Sniper,
        Thrower,
        Controller,
        Main,
        Other
    }

    public enum Faction
    {
        EnemyGroup1,
        EnemyGroup2,
        EnemyGroup3,
        EnemyGroup4,
        Player,
        Friendly,
        Civilian,
        Wildlife,
        Dead
    }

    [Header("Settings")]
    public Archetype selectedArchetype;
    public Faction selectedFaction;

    [Header("Behavior Library (Assign in Inspector)")]
    [SerializeField] public AIBehaviorLibrary behaviorLibrary;

    private AIBehavior currentBehavior;

    void Start()
    {
        AddToFactionList();
        LoadBehavior();
        ExecuteBehavior();
    }

    public void SetArchetype(Archetype archetype)
    {
        selectedArchetype = archetype;
        LoadBehavior();
    }

    public void SetFaction(Faction faction)
    {
        selectedFaction = faction;
        AddToFactionList();
    }

    private void AddToFactionList()
    {
        Factions factions = FindObjectOfType<Factions>();
        if (factions == null)
        {
            Debug.LogWarning("No Factions component found in the scene");
            return;
        }

        var factionGroup = factions.factions.FirstOrDefault(g => g.faction == selectedFaction);
        if (factionGroup == null) return;

        if (!factionGroup.members.Contains(gameObject))
        {
            factionGroup.members.Add(gameObject);
        }
    }

    private void LoadBehavior()
    {
        if (behaviorLibrary == null)
        {
            Debug.LogWarning("No Behavior Library assigned to AIClass!");
            return;
        }

        currentBehavior = behaviorLibrary.GetBehavior(selectedArchetype);
        if (currentBehavior == null)
        {
            Debug.LogWarning($"No behavior mapped for archetype: {selectedArchetype}");
        }
    }

    public void ExecuteBehavior()
    {
        if (currentBehavior != null)
        {
            currentBehavior.RunInstructions(gameObject);
        }
        else
        {
            Debug.LogWarning("No behavior assigned or found. Skipping execution.");
        }
    }
}