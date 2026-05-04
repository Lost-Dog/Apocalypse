using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Skill Indicator Target")]
    [Category("KingEdward/Skill Tree/Skill Indicator")]

    [Image(typeof(IconDamageRadius), ColorTheme.Type.Red)]
    [Description("Returns the current skill indicator target position (cursor ground position when aiming)")]

    [Serializable]
    public class GetPositionSkillIndicatorTarget : PropertyTypeGetPosition
    {
            public override Vector3 Get(Args args) => SkillIndicatorController.LastTargetPosition;

        public override Vector3 Get(GameObject gameObject) => SkillIndicatorController.LastTargetPosition;

        public static PropertyGetPosition Create()
        {
            return new PropertyGetPosition(new GetPositionSkillIndicatorTarget());
        }

        public override string String => "Skill Indicator Target";
    }
}
