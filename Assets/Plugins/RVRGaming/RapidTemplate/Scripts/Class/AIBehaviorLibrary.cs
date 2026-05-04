using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AIBehaviorLibrary",
    menuName = "Rapid Template/AI Behavior Library",
    order = 1
)]
public class AIBehaviorLibrary : ScriptableObject
{
    [System.Serializable]
    public class ArchetypeBehaviorMapping
    {
        public AIClass.Archetype archetype;
        public AIBehavior behavior;
    }

    public List<ArchetypeBehaviorMapping> archetypeBehaviors = new List<ArchetypeBehaviorMapping>();

    public AIBehavior GetBehavior(AIClass.Archetype archetype)
    {
        var entry = archetypeBehaviors.FirstOrDefault(e => e.archetype == archetype);
        return entry?.behavior;
    }
}
