using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace KingEdward.SkillTree
{
    [Title("Projectile Caster")]
    [Category("KingEdward/Skill Tree/Projectile Caster")]
    
    [Image(typeof(IconProjectileBehavior), ColorTheme.Type.Blue)]
    [Description("Gets the caster (who launched) of a Projectile Behavior")]

    [Serializable] [HideLabelsInEditor]
    public class GetGameObjectProjectileCaster : PropertyTypeGetGameObject
    {
        [SerializeField] protected PropertyGetGameObject m_Projectile = GetGameObjectSelf.Create();

        public override GameObject Get(Args args)
        {
            GameObject projectileObject = m_Projectile.Get(args);
            if (projectileObject == null) return null;
            
            ProjectileBehavior projectile = projectileObject.Get<ProjectileBehavior>();
            if (projectile == null) return null;
            
            return projectile.GetCaster();
        }
        
        public override GameObject Get(GameObject gameObject)
        {
            GameObject projectileObject = m_Projectile.Get(gameObject);
            if (projectileObject == null) return null;
            
            ProjectileBehavior projectile = projectileObject.Get<ProjectileBehavior>();
            if (projectile == null) return null;
            
            return projectile.GetCaster();
        }

        public GetGameObjectProjectileCaster() : base()
        { }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectProjectileCaster instance = new GetGameObjectProjectileCaster();
            return new PropertyGetGameObject(instance);
        }

        public override string String => $"Caster of {m_Projectile}";
    }
}
