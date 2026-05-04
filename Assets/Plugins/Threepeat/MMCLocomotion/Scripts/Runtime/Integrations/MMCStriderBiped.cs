using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Threepeat
{
    public class MMCStriderBiped : MonoBehaviour
    {
        public string PLACEHOLDER = "This is purely a placeholder, Strider Integration isn't installed.";

        protected Coroutine smoothTransitionCoroutine;

        public float Weight
        {
            get
            {
                return 0;
            }
        }
        public void EnableSmooth(float a_fadeRate)
        {
        }

        public void DisableSmooth(float a_fadeRate)
        {
        }

    }
}