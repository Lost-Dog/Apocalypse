using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
    [Title("Last Hit Direction")]
    [Category("Melee/Last Hit Direction")]
    
    [Image(typeof(IconMeleeSword), ColorTheme.Type.Yellow)]
    [Description("Returns the last Melee Hit direction")]

    [Serializable]
    public class GetDirectionMeleeLastHit : PropertyTypeGetDirection
    {
        public override Vector3 Get(Args args) => Skill.LastHitDirection;
        public override Vector3 Get(GameObject gameObject) => Skill.LastHitDirection;

        public static PropertyGetDirection Create => new PropertyGetDirection(
            new GetDirectionMeleeLastHit()
        );

        public override string String => "Last Hit";
    }
}