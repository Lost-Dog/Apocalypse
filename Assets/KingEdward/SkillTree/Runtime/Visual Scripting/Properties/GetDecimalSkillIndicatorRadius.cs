using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Skill Indicator Radius")]
    [Category("KingEdward/Skill Tree/Skill Indicator")]

    [Image(typeof(IconDamageRadius), ColorTheme.Type.Red)]
    [Description("Returns the current skill indicator radius. For ExpandingCircle, this grows while holding. Use in Execute In Radius, Spawn Persistent Zone, etc.")]

    [Serializable]
    public class GetDecimalSkillIndicatorRadius : PropertyTypeGetDecimal
    {
            public override double Get(Args args) => SkillIndicatorController.LastRadius;

        public static PropertyGetDecimal Create()
        {
            return new PropertyGetDecimal(new GetDecimalSkillIndicatorRadius());
        }

        public override string String => "Skill Indicator Radius";
    }
}
