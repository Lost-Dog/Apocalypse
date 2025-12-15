using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JUTPS.FX
{
    [System.Serializable]
    public class SurfaceAudios
    {
        public string SurfaceTag;
        public List<AudioClip> AudioClips = new List<AudioClip>(4);
        public static void PlayRandomAudio(AudioSource audioSource, List<SurfaceAudios> SurfaceAudioClips, string surfaceTag = "Untagged")
        {
            for (int i = 0; i < SurfaceAudioClips.Count; i++)
            {
                if (SurfaceAudioClips[i].SurfaceTag == surfaceTag)
                {
                    audioSource.PlayOneShot(SurfaceAudioClips[i].AudioClips[Random.Range(0, SurfaceAudioClips.Count)]);
                }
            }
        }
    }

    /// <summary>
    /// Spawns audio and visual effects.
    /// </summary>
    [System.Serializable]
    public class SurfaceAudiosWithFX
    {
        /// <summary>
        /// The tag of the surface, like ground, wall or anything.
        /// </summary>
        public string SurfaceTag;

        /// <summary>
        /// The audios to play when interact with the surface.
        /// </summary>
        public List<AudioClip> AudioClips;

        /// <summary>
        /// The prefab effects to spawn when interact with the surface.
        /// </summary>
        public List<GameObject> Effects;

        /// <summary>
        /// The life time o the <see cref="Effects"/> when spawned.
        /// </summary>
        public float EffectsLifeTime;

        /// <summary>
        /// Create a new surfaceFX.
        /// </summary>
        public SurfaceAudiosWithFX()
        {
            EffectsLifeTime = 5f;
            AudioClips = new List<AudioClip>(4);
            Effects = new List<GameObject>(4);
        }

        /// <summary>
        /// Create a new surfaceFX.
        /// </summary>
        /// <param name="tagName">The surface tag.</param>
        public SurfaceAudiosWithFX(string tagName = "Skin")
        {
            SurfaceTag = tagName;
            EffectsLifeTime = 5f;
        }

        /// <summary>
        /// Play a random <see cref="AudioClip"/> from a <see cref="SurfaceAudiosWithFX"/> based on a surface tag, 
        /// if not have a tag, will play any <see cref="AudioClip"/>.
        /// </summary>
        /// <param name="audioSource">The <see cref="AudioSource"/> to play the audio.</param>
        /// <param name="surfaceAudioClips">The surface clips to play. The system will select one clip based on the surface tag, if have.</param>
        /// <param name="surfaceTag">The tag of the surface.</param>
        public static void PlayRandomAudioFX(AudioSource audioSource, List<SurfaceAudiosWithFX> surfaceAudioClips, string surfaceTag = "Untagged")
        {
            if (surfaceAudioClips.Count < 1)
                return;

            SurfaceAudiosWithFX selectedSurface = surfaceAudioClips[0];
            for (int i = 0; i < surfaceAudioClips.Count; i++)
            {
                if (surfaceAudioClips[i].SurfaceTag.Equals(surfaceTag))
                {
                    selectedSurface = surfaceAudioClips[i];
                    break;
                }
            }

            int randomAudio = Random.Range(0, selectedSurface.AudioClips.Count);
            audioSource.PlayOneShot(selectedSurface.AudioClips[randomAudio]);
        }

        /// <summary>
        /// Instantiate a random FX based on a surface tag, if not have a surface will instantiate any effect.
        /// </summary>
        /// <param name="surfaceAudioClips">The surfaces.</param>
        /// <param name="fxPosition">The position to spawn the effect.</param>
        /// <param name="fxRotation">The rotation to spawn the effect.</param>
        /// <param name="surfaceTag">The surface tag.</param>
        /// <param name="hideInHierarchy">Hide the spawned object on the hierarchy editor?</param>
        /// <returns></returns>
        public static GameObject SpawnRandomFX(List<SurfaceAudiosWithFX> surfaceAudioClips, Vector3 fxPosition, Quaternion fxRotation = default, string surfaceTag = "Untagged", bool hideInHierarchy = true)
        {
            if (surfaceAudioClips.Count < 1)
                return null;

            SurfaceAudiosWithFX selectedSurface = surfaceAudioClips[0];
            for (int i = 0; i < surfaceAudioClips.Count; i++)
            {
                if (surfaceAudioClips[i].SurfaceTag.Equals(surfaceTag))
                {
                    selectedSurface = surfaceAudioClips[i];
                    break;
                }
            }

            if (selectedSurface.Effects.Count < 1)
                return null;

            var randomFx = Random.Range(0, selectedSurface.Effects.Count);
            GameObject obj = GameObject.Instantiate(selectedSurface.Effects[randomFx], fxPosition, fxRotation);

            if (hideInHierarchy)
                obj.hideFlags = HideFlags.HideInHierarchy;

            GameObject.Destroy(obj, selectedSurface.EffectsLifeTime);
            return obj;
        }

        /// <summary>
        /// Play a surface audio and spawn an effect.
        /// </summary>
        /// <param name="audioSource">The <see cref="AudioSource" to play the surface audio./></param>
        /// <param name="surfaceAudioClips">The surfaces.</param>
        /// <param name="fxPosition"><The position to spawn the effect./param>
        /// <param name="fxRotation">The rotation to spawn the effect.</param>
        /// <param name="parent">The spawned effect parent.</param>
        /// <param name="surfaceTag">The surface tag to spawn.</param>
        /// <param name="hideInHierarchy">Hide the spawned object on the hierarchy editor?</param>
        /// <returns></returns>
        public static GameObject Play(AudioSource audioSource, SurfaceAudiosWithFX[] surfaceAudioClips, Vector3 fxPosition, Quaternion fxRotation = default, Transform parent = null, string surfaceTag = "Untagged", bool hideInHierarchy = true)
        {
            if (surfaceAudioClips.Length == 0)
                return null;

            SurfaceAudiosWithFX selectedSurface = surfaceAudioClips[0];
            for (int i = 0; i < surfaceAudioClips.Length; i++)
            {
                if (surfaceAudioClips[i].SurfaceTag == surfaceTag)
                {
                    selectedSurface = surfaceAudioClips[i];
                    break;
                }
            }

            if (audioSource)
                audioSource.PlayOneShot(selectedSurface.AudioClips[Random.Range(0, selectedSurface.AudioClips.Count)]);

            if (selectedSurface.Effects.Count < 1)
                return null;

            int randomFx = Random.Range(0, selectedSurface.Effects.Count);
            GameObject obj = GameObject.Instantiate(selectedSurface.Effects[randomFx], fxPosition, fxRotation);
            obj.transform.SetParent(parent, true);

            if (hideInHierarchy)
                obj.hideFlags = HideFlags.HideInHierarchy;

            GameObject.Destroy(obj, surfaceAudioClips[randomFx].EffectsLifeTime);
            return obj;
        }
    }

    [System.Serializable]
    public class SurfaceFX
    {
        public string SurfaceTag;
        public GameObject ParticleFXPrefab;
        private static bool Instantiated;

        /// <summary>
        /// Instantiates a ParticleFX according to the surface tag
        /// </summary>
        /// <param name="SurfaceFx"> SurfaceFX variable type </param>
        /// <param name="SurfaceTag"> Tag of surface </param>
        /// <param name="Postion"> Position to instantiate ParticleFX </param>
        /// <param name="Rotation"> Rotation to instantiate ParticleFX </param>
        /// <param name="TimeToDestroy"> Time to auto destroy ParticleFX</param>
        public static void InstantiateParticleFX(SurfaceFX[] SurfaceFx, string SurfaceTag = "Untagged", Vector3 Postion = default, Quaternion Rotation = default, Transform parent = null, float TimeToDestroy = 5f)
        {
            Instantiated = false;

            for (int i = 0; i < SurfaceFx.Length; i++)
            {
                if (SurfaceTag == SurfaceFx[i].SurfaceTag && SurfaceFx[i].ParticleFXPrefab != null)
                {
                    GameObject pfx = GameObject.Instantiate(SurfaceFx[i].ParticleFXPrefab, Postion, Rotation);
                    pfx.transform.parent = parent;
                    //pfx.hideFlags = HideFlags.HideInHierarchy;

                    if (TimeToDestroy > 0)
                    {
                        GameObject.Destroy(pfx, TimeToDestroy);
                    }
                    Instantiated = true;
                }
            }

            //If the code has come this far and the particle has not yet been instantiated,
            //it means that a tag that matches the surface was not found, then it will instantiate the first ParticleFX in the list
            if (Instantiated == false && SurfaceFx.Length > 0)
            {
                GameObject pfx = GameObject.Instantiate(SurfaceFx[0].ParticleFXPrefab, Postion, Rotation);
                pfx.transform.parent = parent;
                //pfx.hideFlags = HideFlags.HideInHierarchy;

                if (TimeToDestroy > 0)
                {
                    GameObject.Destroy(pfx, TimeToDestroy);
                }
                Instantiated = true;
            }
        }
    }

}