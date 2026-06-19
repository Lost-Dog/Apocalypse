using MxMGameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public abstract class NGCharacterHelper
    {
        [HideInInspector] public NGCharacter character;


        public NGCharacterBaseConfig config
        {
            get => character.config;
            set => character.SetConfig(value);
        }

        public NGCharacterControllerWrapper controllerWrapper
        {
            get => character.controllerWrapper;
            set => character.controllerWrapper = value;
        }

        public NGCharacterHelper(NGCharacter ch)
        {
            character = ch;
        }

        public abstract void Initialize();
    }
}