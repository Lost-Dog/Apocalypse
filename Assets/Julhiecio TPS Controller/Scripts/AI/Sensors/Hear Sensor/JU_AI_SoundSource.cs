using UnityEngine;

namespace JU.CharacterSystem.AI.HearSystem
{
    /// <summary>
    /// Play a sound that can alert nearest AIs that have <see cref="HearSensor"/>.
    /// </summary>
    [AddComponentMenu("JU TPS/AI/Hear Sensor/Sound Source")]
    public class JU_AI_SoundSource : MonoBehaviour
    {
        private float _timer;
        private float _playedTime;
        private bool _played;

        /// <summary>
        /// The distance that AIs can detect the sound.
        /// </summary>
        [Header("Sound")]
        public float SoundDistance;

        /// <summary>
        /// The sound tag.
        /// </summary>
        public JUTag SoundTag;

        /// <summary>
        /// Automatic play sound on instantied.
        /// </summary>
        [Header("Automatic Play")]
        public bool PlayOnSpawn;

        /// <summary>
        /// Automatic play sound on destroyed.
        /// </summary>
        public bool PlayOnDestroy;

        /// <summary>
        /// Automatic play sound on component enabled.
        /// </summary>
        public bool PlayOnEnable;

        /// <summary>
        /// Automatic play sound on component disabled.
        /// </summary>
        public bool PlayOnDisable;

        /// <summary>
        /// The repeat time to play sound every X seconds.
        /// </summary>
        [Min(0)]
        public float RepeatRate;

        /// <summary>
        /// The min time to repeat the sound. Useful on play sound on collide
        /// to avoid multiple calls.
        /// </summary>
        public float MinRepeatTime;

        /// <summary>
        /// Play sound on trigger enter.
        /// </summary>
        [Header("Play On Collision")]
        public bool PlayOnTriggerEnter;

        /// <summary>
        /// Play sound on trigger exit.
        /// </summary>
        public bool PlayOnTriggerExit;

        /// <summary>
        /// Play sound on collision enter.
        /// </summary>
        public bool PlayOnCollisionEnter;

        /// <summary>
        /// Play sound on collision exit.
        /// </summary>
        public bool PlayOnCollisionExit;

        /// <summary>
        /// Tags to ignore collision with specific objects.
        /// </summary>
        public string[] IgnoreCollisionTags;

        /// <summary>
        /// Sfx to play audio, must be a different gameObject.
        /// </summary>
        [Header("SFX")]
        public AudioSource SfxSource;

        /// <summary>
        /// Life time of the spawned SFX.
        /// </summary>
        public float SfxLifeTime;

        /// <summary>
        /// Create instance.
        /// </summary>
        public JU_AI_SoundSource()
        {
            SoundDistance = 10;
            RepeatRate = 0;
            MinRepeatTime = 1;

            SfxLifeTime = 10;
        }

        private void OnEnable()
        {
            if (PlayOnEnable)
                Play();
        }

        private void OnDisable()
        {
            if (PlayOnDisable)
                Play();
        }

        private void Start()
        {
            if (PlayOnSpawn)
                Play();
        }

        private void OnDestroy()
        {
            if (PlayOnDestroy)
                Play();
        }

        private void Update()
        {
            if (_played)
                _playedTime += Time.deltaTime;

            if (RepeatRate > 0)
            {
                _timer += Time.deltaTime;
                if (_timer > RepeatRate)
                {
                    _timer = 0;
                    Play();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PlayOnTriggerEnter && IsValidCollider(other))
                Play();
        }

        private void OnTriggerExit(Collider other)
        {
            if (PlayOnTriggerExit && IsValidCollider(other))
                Play();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!PlayOnCollisionEnter)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (IsValidCollider(collision.contacts[i].otherCollider))
                {
                    Play();
                    return;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!PlayOnCollisionExit)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (IsValidCollider(collision.contacts[i].otherCollider))
                {
                    Play();
                    return;
                }
            }
        }

        private bool IsValidCollider(Collider collider)
        {
            for (int i = 0; i < IgnoreCollisionTags.Length; i++)
            {
                if (collider.CompareTag(IgnoreCollisionTags[i]))
                    return false;
            }

            return true;
        }

        public void Play()
        {
            if (_played && _playedTime < MinRepeatTime)
                return;

            _played = true;
            _playedTime = 0;
            HearSensor.AddSoundSource(transform.position, SoundDistance, gameObject, SoundTag);

            if (SfxSource)
            {
                var newSource = Instantiate(SfxSource, transform.position, transform.rotation);
                newSource.transform.SetParent(null, true);
                Destroy(newSource, SfxLifeTime);
            }
        }
    }
}