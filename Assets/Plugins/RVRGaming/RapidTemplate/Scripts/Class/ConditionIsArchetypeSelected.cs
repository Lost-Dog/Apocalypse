using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Archetype Selected")]
    [Description("Returns true if the selected Archetype on the specified target matches one of the given Archetypes")]

    [Category("AI/Is Archetype Selected")]

    [Parameter("Target", "The GameObject to check for the AIOptions component")]
    [Parameter("Archetypes", "The Archetypes to compare against")]

    [Keywords("AI", "Archetype", "Check")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Red)]
    [Serializable]
    public class ConditionIsArchetypeSelected : Condition
    {
        [Flags]
        public enum ArchetypeMask
        {
            None = 0,
            Assault = 1 << 0,
            Rusher = 1 << 1,
            Tank = 1 << 2,
            Sniper = 1 << 3,
            Thrower = 1 << 4,
            Controller = 1 << 5,
            Player = 1 << 6,
            Other = 1 << 7
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = new PropertyGetGameObject();
        [SerializeField] private ArchetypeMask m_Archetypes = ArchetypeMask.None;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Is Archetype {m_Archetypes} on {m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return false;

            AIClass aiOptions = target.GetComponent<AIClass>();
            if (aiOptions == null) return false;

            return ((ArchetypeMask)(1 << (int)aiOptions.selectedArchetype) & m_Archetypes) != 0;
        }
    }
}
