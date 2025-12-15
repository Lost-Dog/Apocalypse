using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JU.CharacterSystem.AI.HearSystem
{
    /// <summary>
    /// Listen the environment to find targets.
    /// </summary>
    [System.Serializable]
    public class HearSensor
    {
        // Stores and update all AI hear sensors.
        private class JU_AIHearManager : MonoBehaviour
        {
            // Max amount of sensors that can be updated on each frame.
            // Used to not update large count of sensors on the same frame.
            public const int MAX_SENSORS_PER_GROUP = 10;

            private int _currentGroupToUpdateIndex;
            private List<HearSensor> _currentGroupToUpdate;

            private List<SoundData> _sounds;

            /// <summary>
            /// The sensors groups, each group can have only <see cref="MAX_SENSORS_PER_GROUP"/> amount of sensors.
            /// The key is the group index, and the value is the group with the sensors.
            /// </summary>
            private Dictionary<int, List<HearSensor>> _sensorsGroups;

            private void Update()
            {
                if (_sensorsGroups == null || _sounds == null)
                    return;

                if (_sounds.Count == 0)
                    return;

                // All groups are updated, but only a single group per frame to help with performance.

                // Update all sensors of the current group.
                foreach (var sensor in _currentGroupToUpdate)
                {
                    // Remove invalid sensors.
                    if (!sensor.AI)
                    {
                        _currentGroupToUpdate.Remove(sensor);
                        break;
                    }

                    if (!sensor.Enabled || !sensor.AI.enabled)
                        continue;

                    // Check if have a sound closest to the sensor.
                    foreach (var sound in _sounds)
                    {
                        // The sensor is hearing the sounds of the your our character.
                        // So it's can be ignored
                        if (sound.Owner && sound.Owner == sensor.AI.gameObject)
                            continue;

                        if (Vector3.Distance(sensor.AI.Center, sound.Position) > sound.Distance)
                            continue;

                        // Ignore the sound if have a tag to ignore.
                        if (sound.Tag)
                        {
                            bool ignoreSound = false;
                            for (int i = 0; i < sensor.SoundsToIgnore.Length; i++)
                            {
                                if (sensor.SoundsToIgnore[i] == sound.Tag)
                                {
                                    ignoreSound = true;
                                    break;
                                }
                            }

                            if (ignoreSound)
                                continue;
                        }

                        sensor.Alert(sound);
                        break;
                    }
                }

                // Set the next group to update on the next frame.

                if (_currentGroupToUpdateIndex == _sensorsGroups.Count - 1)
                {
                    _currentGroupToUpdateIndex = 0;
                    _sounds.Clear();
                }
                else
                    _currentGroupToUpdateIndex += 1;

                _currentGroupToUpdate = _sensorsGroups[_currentGroupToUpdateIndex];
            }

            /// <summary>
            /// Add a new sound to hear sensors proccess.
            /// </summary>
            /// <param name="position">The posision of the sound.</param>
            /// <param name="distance">The max sound distance.</param>
            /// <param name="owner">The object owner of the sound.</param>
            /// <param name="soundTag">The sound tag, used by AIs to filter wich sound should be heared.</param>
            public void AddSoundSource(Vector3 position, float distance, GameObject owner, JUTag soundTag)
            {
                if (_sounds == null)
                    _sounds = new List<SoundData>();

                _sounds.Add(new SoundData
                {
                    Position = position,
                    Distance = distance,
                    Owner = owner,
                    Tag = soundTag
                });
            }

            /// <summary>
            /// Add a new hear sensor to listen the environment.
            /// </summary>
            /// <param name="sensor"></param>
            public void AddSensor(HearSensor sensor)
            {
                // Sensors are separated by groups, all sensors are updated but only a single group
                // per frame to help with performance.

                // Add always on the last group. If the group is full, create a new group. 

                if (_sensorsGroups == null)
                    _sensorsGroups = new Dictionary<int, List<HearSensor>>();

                if (_sensorsGroups.Count == 0)
                {
                    _currentGroupToUpdate = new List<HearSensor>(MAX_SENSORS_PER_GROUP);
                    _sensorsGroups.Add(0, _currentGroupToUpdate);
                }

                // Add to last group if avaliable.
                var lastGroup = _sensorsGroups[_sensorsGroups.Count - 1];
                if (lastGroup.Count < MAX_SENSORS_PER_GROUP)
                {
                    lastGroup.Add(sensor);
                    return;
                }

                // Add to a new group if the last if full.
                _sensorsGroups.Add(_sensorsGroups.Count, new List<HearSensor>());
                _sensorsGroups[_sensorsGroups.Count - 1].Add(sensor);
            }
        }

        private struct SoundData
        {
            public Vector3 Position;
            public float Distance;
            public GameObject Owner;
            public JUTag Tag;
        }

        private static JU_AIHearManager _hearManager;

        /// <summary>
        /// If true, the sensor can listen the environment.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// The tag of the sounds that should be ignored.
        /// </summary>
        public JUTag[] SoundsToIgnore;

        /// <summary>
        /// The AI character.
        /// </summary>
        public JUCharacterAIBase AI { get; private set; }

        /// <summary>
        /// Called when the AI listen something.
        /// Returns the position of the sound and the owner.
        /// </summary>
        public UnityEvent<Vector3, GameObject> OnHear;

        /// <summary>
        /// Create a new hear sensor.
        /// </summary>
        public HearSensor()
        {
            Enabled = true;
        }

        /// <summary>
        /// Setup the hear sensor.
        /// </summary>
        /// <param name="ai"></param>
        public void Setup(JUCharacterAIBase ai)
        {
            CreateManagerIfNotHave();

            AI = ai;
            _hearManager.AddSensor(this);
        }

        private void Alert(SoundData sound)
        {
            OnHear.Invoke(sound.Position, sound.Owner);
        }

        /// <summary>
        /// Add a new sound source that can be listened by some <see cref="JUCharacterAIBase"/> with <see cref="HearSensor"/> sensor.
        /// </summary>
        /// <param name="position">The position of the sound.</param>
        /// <param name="distance">The max sound distance.</param>
        /// <param name="owner">The sound owner.</param>
        /// <param name="soundTag">The sound tag, used by AIs to filter wich sound should be heared.</param>
        public static void AddSoundSource(Vector3 position, float distance, GameObject owner, JUTag soundTag)
        {
            if (distance == 0)
                return;

            CreateManagerIfNotHave();

            _hearManager.AddSoundSource(position, distance, owner, soundTag);
        }

        private static void CreateManagerIfNotHave()
        {
            if (_hearManager)
                return;

            _hearManager = new GameObject("JU AI Hear Manager").AddComponent<JU_AIHearManager>();
            _hearManager.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}