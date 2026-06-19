using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Shooter
{
    [Title("Character Forward")]
    [Description("Aims forward from the character perspective")]
    
    [Category("Character Forward")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]
    
    [Serializable]
    public class AimCharacterForward : TAim
    {
        private const float INFINITY = 999f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectSelf.Create();
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override Vector3 GetPoint(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null
                ? character.transform.TransformPoint(Vector3.forward * INFINITY)
                : default;
        }
    }
}