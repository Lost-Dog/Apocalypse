using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    [Title("Current Character Traverse")]
    [Category("Traverse/Current Character Traverse")]
    
    [Description("The Traverse component the Character is currently grabbed onto")]
    [Image(typeof(IconBug), ColorTheme.Type.Green)]

    [Serializable]
    public class GetGameObjectTraverseCurrent : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        public override GameObject Get(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return null;

            Traverse currentTraverse = character.Combat.RequestStance<TraversalStance>().Traverse;
            return currentTraverse != null
                ? currentTraverse.gameObject
                : null;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectTraverseCurrent instance = new GetGameObjectTraverseCurrent();
            return new PropertyGetGameObject(instance);
        }

        public override string String => $"{this.m_Character} Current Traverse";
    }
}