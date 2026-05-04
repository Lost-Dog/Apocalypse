using System.Collections.Generic;
using UnityEngine;

public class Factions : MonoBehaviour
{
    [System.Serializable]
    public class FactionGroup
    {
        public AIClass.Faction faction;
        public List<GameObject> members = new List<GameObject>();
    }

    [Header("Faction Groups")]
    public List<FactionGroup> factions = new List<FactionGroup>();

    private void Start()
    {
        CollectFactions();
    }

    public void CollectFactions()
    {
        factions.Clear();

        foreach (AIClass.Faction faction in System.Enum.GetValues(typeof(AIClass.Faction)))
        {
            factions.Add(new FactionGroup { faction = faction });
        }

        AIClass[] allAIClasses = GameObject.FindObjectsOfType<AIClass>();

        foreach (AIClass aiClass in allAIClasses)
        {
            FactionGroup group = factions.Find(f => f.faction == aiClass.selectedFaction);
            if (group != null)
            {
                group.members.Add(aiClass.gameObject);
            }
        }

        Debug.Log("Faction collection completed.");
    }
}
