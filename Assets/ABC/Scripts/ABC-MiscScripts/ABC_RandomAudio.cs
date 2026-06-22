using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
    /// <summary>
    /// Will play a random audio clip from a list setup
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ABC_RandomAudio : MonoBehaviour {


        // ********************* Settings ********************

        #region Settings

        /// <summary>
        /// A list of audio clips to play randomly
        /// </summary>
        public List<AudioClip> audioClips;

        #endregion


        // ********************* Variables ********************
        #region Variables

        //Audio source attached to the object
        private AudioSource meAudioSource;

        #endregion



        // ********************* Private Methods ********************
        #region Private Methods

        private void PlayRandomClip() {

            //If we have no clips setup or no audio component then end here
            if (this.audioClips.Count == 0 || this.meAudioSource == null)
                return;

            // select random clip from those setup
            AudioClip randomClip = this.audioClips[Random.Range(0, audioClips.Count)];
            this.meAudioSource.clip = randomClip;
            this.meAudioSource.Play();

        }

        #endregion



        // ********************* Game ********************

        #region Game

        void Awake() {
            this.meAudioSource = transform.GetComponent<AudioSource>();
            this.meAudioSource.playOnAwake = false;
        }


        // Use this for initialization
        void OnEnable() {
            this.PlayRandomClip();
        }


        #endregion


    }
}
