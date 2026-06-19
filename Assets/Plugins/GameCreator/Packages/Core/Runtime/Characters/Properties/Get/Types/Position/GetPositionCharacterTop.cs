using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Character Position Head")]
    [Category("Characters/Character Position Head")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow, typeof(OverlayArrowUp))]
    [Description("Returns the top (head) position of the Character")]

    [Serializable]
    public class GetPositionCharacterTop : PropertyTypeGetPosition
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        public GetPositionCharacterTop()
        { }

        public GetPositionCharacterTop(Character character)
        {
            this.m_Character = GetGameObjectCharactersInstance.CreateWith(character);
        }

        public override Vector3 Get(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null ? character.Crown : default;
        }

        public static PropertyGetPosition Create => new PropertyGetPosition(
            new GetPositionCharacterTop()
        );
        
        public static PropertyGetPosition CreateWith(Character character)
        {
            return new PropertyGetPosition(
                new GetPositionCharacterTop(character)
            );
        }

        public override Vector3 EditorValue
        {
            get
            {
                GameObject gameObject = this.m_Character.EditorValue;
                if (gameObject == null) return default;

                Character character = gameObject.GetComponent<Character>();
                return character != null ? character.Crown : default;
            }
        }

        public override string String => $"{this.m_Character} Head";
    }
}