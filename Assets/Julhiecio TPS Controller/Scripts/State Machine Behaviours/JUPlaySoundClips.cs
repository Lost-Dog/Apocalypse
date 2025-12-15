using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.Events;
namespace JUTPS.AnimatorStateMachineBehaviours
{
    public class JUPlaySoundClips : StateMachineBehaviour
    {
        public AudioClip[] Clips = new AudioClip[1];
        [Range(0, 1)]
        public float PlayAt = 0.15f;
        [Range(0, 1)]
        public float VolumeScale = 1f;
        public bool UseCharacterAudioSource = true;
        public string AlternativeAudioSourceName = "Child Audio Source";
        private bool Played = false;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Played = false;
        }
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime >= PlayAt && Played == false)
            {
                if (UseCharacterAudioSource)
                {
                    if (animator.gameObject.TryGetComponent(out AudioSource aud))
                    {
                        aud.PlayOneShot(Clips[Random.Range(0, Clips.Length-1)], VolumeScale);
                    }
                    else
                    {
                        Debug.LogWarning("Unable to get " + animator.gameObject.name + " Audio Source, try add Audio Source component or use 'AlternativeAudioSourceName'");
                    }
                }
                else
                {
                    Transform AlternativeAudioSourceChild = animator.transform.Find(AlternativeAudioSourceName);
                    if (AlternativeAudioSourceChild.TryGetComponent(out AudioSource aud))
                    {
                        aud.PlayOneShot(Clips[Random.Range(0, Clips.Length - 1)], VolumeScale);
                    }
                    else
                    {
                        Debug.LogWarning("Unable to get a child gameobject with Audio Source Component with name '" + AlternativeAudioSourceName + "'");
                    }
                }
                Played = true;
            }
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Played = false;
        }

    }
}