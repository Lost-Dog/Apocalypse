using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Find by Factions")]
    [Category("Characters/Find by Factions")]

    [Image(typeof(IconCubeOutline), ColorTheme.Type.Blue)]
    [Description("Finds the first character matching any of the specified Factions from the Factions script")]

    [Serializable]
    public class GetFactionFromAIClass : PropertyTypeGetGameObject
    {
        [Flags]
        public enum FactionMask
        {
            None = 0,
            EnemyGroup1 = 1 << 0,
            EnemyGroup2 = 1 << 1,
            EnemyGroup3 = 1 << 2,
            EnemyGroup4 = 1 << 3,
            Player = 1 << 4,
            Friendly = 1 << 5,
            Civilian = 1 << 6,
            Wildlife = 1 << 7,
            Dead = 1 << 8
        }

        [SerializeField]
        private FactionMask m_TargetFactions = FactionMask.None;

        public override GameObject Get(Args args)
        {
            Factions factions = GameObject.FindObjectOfType<Factions>();
            if (factions == null)
            {
                Debug.LogWarning("No Factions component found in the scene");
                return null;
            }

            List<AIClass.Faction> selectedFactions = Enum.GetValues(typeof(AIClass.Faction))
                .Cast<AIClass.Faction>()
                .Where(faction => ((FactionMask)(1 << (int)faction) & m_TargetFactions) != 0)
                .ToList();

            foreach (AIClass.Faction faction in selectedFactions)
            {
                var factionGroup = factions.factions.FirstOrDefault(group => group.faction == faction);
                if (factionGroup != null && factionGroup.members.Count > 0)
                {
                    return factionGroup.members[0];
                }
            }

            Debug.LogWarning($"No GameObjects found in the selected factions: {m_TargetFactions}");
            return null;
        }

        public override string String => $"GameObject with Factions {m_TargetFactions}";
    }
}
