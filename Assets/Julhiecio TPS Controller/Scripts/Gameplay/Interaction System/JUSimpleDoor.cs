using UnityEngine;
using System.Collections;

namespace JUTPS.InteractionSystem
{
    [AddComponentMenu("JU TPS/Interaction System/Utils/JU Simple Door Script")]
    public class JUSimpleDoor : MonoBehaviour
    {
        [Header("Settings")]
        public Vector3 openEulerAngles = new Vector3(0, 90, 0); // target rotation when opened
        public float rotationDuration = 1f; // time to fully open/close

        private Coroutine currentCoroutine = null; // keeps track of the current animation
        private Vector3 closedEulerAngles;
        private void Start()
        {
            closedEulerAngles = transform.localEulerAngles;
        }
        public void OpenDoor()
        {
            // stop current animation if any
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            // start new animation towards open rotation
            currentCoroutine = StartCoroutine(RotateDoor(openEulerAngles));
        }

        public void CloseDoor()
        {
            // stop current animation if any
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            // start new animation towards closed rotation
            currentCoroutine = StartCoroutine(RotateDoor(closedEulerAngles));
        }

        private IEnumerator RotateDoor(Vector3 targetEuler)
        {
            Quaternion startRot = transform.localRotation;           // current rotation
            Quaternion endRot = Quaternion.Euler(targetEuler);       // target rotation

            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);

                // smooth the interpolation factor
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // interpolate smoothly from current to target
                transform.localRotation = Quaternion.Lerp(startRot, endRot, smoothT);

                yield return null; // wait for next frame
            }

            // ensure it ends exactly at the target
            transform.localRotation = endRot;

            // clear coroutine reference
            currentCoroutine = null;
        }
    }
}