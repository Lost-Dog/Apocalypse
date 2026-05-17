using UnityEngine;

namespace VolumetricFogAndMist2.Demos {

    public class ParticleAnimator : MonoBehaviour {
        private Vector3 startPosition;
        private float timeOffset;

        private void Start() {
            startPosition = transform.position;
            timeOffset = Random.Range(0f, 2f * Mathf.PI); // Random offset for variety
        }

        private void Update() {
            float yOffset = Mathf.Sin(Time.time + timeOffset) * 5f;
            transform.position = startPosition + Vector3.up * yOffset;
        }
    }

}