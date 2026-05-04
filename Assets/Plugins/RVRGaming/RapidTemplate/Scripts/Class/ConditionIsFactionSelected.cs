using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Faction Selected")]
    [Description("Returns true if the selected Faction on the specified target matches one of the given Factions")]

    [Category("AI/Is Faction Selected")]

    [Parameter("Target", "The GameObject to check for the AIOptions component")]
    [Parameter("Factions", "The Factions to compare against")]

    [Keywords("AI", "Faction", "Check")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionIsFactionSelected : Condition
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

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = new PropertyGetGameObject();
        [SerializeField] private FactionMask m_Factions = FactionMask.None;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Is Faction {m_Factions} on {m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return false;

            AIClass aiOptions = target.GetComponent<AIClass>();
            if (aiOptions == null) return false;

            return ((FactionMask)(1 << (int)aiOptions.selectedFaction) & m_Factions) != 0;
        }
    }
}
