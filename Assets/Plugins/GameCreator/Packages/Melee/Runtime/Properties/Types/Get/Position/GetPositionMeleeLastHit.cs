using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
    [Title("Last Hit Position")]
    [Category("Melee/Last Hit Position")]
    
    [Image(typeof(IconMeleeSword), ColorTheme.Type.Yellow)]
    [Description("Returns the last Melee Hit position")]

    [Serializable]
    public class GetPositionMeleeLastHit : PropertyTypeGetPosition
    {
        public override Vector3 Get(Args args) => Skill.LastHitPosition;
        public override Vector3 Get(GameObject gameObject) => Skill.LastHitPosition;

        public static PropertyGetPosition Create => new PropertyGetPosition(
            new GetPositionMeleeLastHit()
        );

        public override string String => "Last Hit";
    }
}