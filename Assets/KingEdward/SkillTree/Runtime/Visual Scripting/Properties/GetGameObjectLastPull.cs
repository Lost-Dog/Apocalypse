using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree.Instructions
{
    [Title("Last Pull")]
    [Category("KingEdward/Skill Tree/Last Pull")]
    
    [Image(typeof(IconPullTo), ColorTheme.Type.Red)]
    [Description("Returns the last Pull Enemies GameObject created")]
    
    [Serializable]
    public class GetGameObjectLastPull : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return PullRegistry.LastPull;
        }

        public override GameObject Get(GameObject gameObject)
        {
            return PullRegistry.LastPull;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectLastPull instance = new GetGameObjectLastPull();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Last Pull";
    }
}
