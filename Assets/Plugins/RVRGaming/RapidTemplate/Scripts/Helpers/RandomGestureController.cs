using System.Collections.Generic;
using System.Threading;
using GameCreator.Runtime.Characters;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    public static class RandomGestureController
    {
        private static readonly Dictionary<Character, CancellationTokenSource> s_TokenSources = new Dictionary<Character, CancellationTokenSource>();

        public static void Start(Character character)
        {
            if (character == null) return;
            Cancel(character); 
            CancellationTokenSource cts = new CancellationTokenSource();
            s_TokenSources[character] = cts;
        }

        public static CancellationToken GetToken(Character character)
        {
            if (character == null) return CancellationToken.None;
            if (s_TokenSources.TryGetValue(character, out CancellationTokenSource cts))
            {
                return cts.Token;
            }
            return CancellationToken.None;
        }

        public static void Cancel(Character character, float delay = 0f, float transition = 0.1f)
        {
            if (character == null) return;
            if (s_TokenSources.TryGetValue(character, out CancellationTokenSource cts))
            {
                cts.Cancel();
                s_TokenSources.Remove(character);
            }

            if (character.Gestures != null)
            {
                character.Gestures.Stop(delay, transition);
            }
        }
    }
}
