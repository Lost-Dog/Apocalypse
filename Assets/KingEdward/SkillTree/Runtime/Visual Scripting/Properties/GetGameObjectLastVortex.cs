using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Last Vortex")]
    [Category("KingEdward/Skill Tree/Last Vortex")]
    
    [Image(typeof(IconPullTo), ColorTheme.Type.Purple)]
    [Description("Returns the last Vortex Pull GameObject created")]
    
    [Serializable]
    public class GetGameObjectLastVortex : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return VortexRegistry.LastVortex;
        }

        public override GameObject Get(GameObject gameObject)
        {
            return VortexRegistry.LastVortex;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectLastVortex instance = new GetGameObjectLastVortex();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Last Vortex";
    }
}
