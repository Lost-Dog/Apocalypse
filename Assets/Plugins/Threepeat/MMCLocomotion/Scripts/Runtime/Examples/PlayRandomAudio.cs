using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class PlayRandomAudio : MonoBehaviour
    {
        public AudioClip[] clips;

        public AudioClip[] scrambledClips;

        public AudioSource audioSource;

        protected int currIndex = 0;

        public static void Shuffle<T>(T[] array)
        {

            int n = array.Length;
            while (n > 1)
            {
                int k = Mathf.RoundToInt(Random.Range(0, n - 0.51f)); //.Next(n--);
                n--;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        void Start()
        {
            scrambledClips = new AudioClip[clips.Length];
            clips.CopyTo(scrambledClips, 0);
            Shuffle<AudioClip>(scrambledClips);
        }

        public void PlayNext()
        {
            //Debug.Log("Called!");
            if (audioSource != null)
            {
                audioSource.PlayOneShot(scrambledClips[currIndex]);
                currIndex++;
                if (currIndex >= scrambledClips.Length)
                {
                    currIndex = 0;
                }
            }
        }
    }
}