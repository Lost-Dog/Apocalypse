using UnityEngine;
using CompassNavigatorPro;

namespace CompassNavigatorProDemos {
	public class FreeCameraMove : MonoBehaviour {
		public float cameraSensitivity = 150;
		public float climbSpeed = 20;
		public float normalMoveSpeed = 20;
		public float slowMoveFactor = 0.25f;
		public float fastMoveFactor = 3;
		public Bounds bounds;

		float rotationX = 0.0f;
	
		float rotationY = 0.0f;

		void Update () {

            if (!InputProxy.GetMouseButton(1)) return;

			CompassPro compass = CompassPro.instance;
			if (compass != null) {
				if (!compass.IsMouseOverMiniMap ()) {
                    rotationX += InputProxy.GetAxis ("Mouse X") * cameraSensitivity * Time.deltaTime;
					transform.localRotation = Quaternion.AngleAxis (rotationX, Vector3.up);

                    rotationY += InputProxy.GetAxis ("Mouse Y") * cameraSensitivity * Time.deltaTime;
					rotationY = Mathf.Clamp(rotationY, -45, 45);
					transform.localRotation *= Quaternion.AngleAxis (rotationY, Vector3.left);
				}
			}

			Vector3 oldPos = transform.position;

            if (InputProxy.GetKey (KeyCode.LeftShift) || InputProxy.GetKey (KeyCode.RightShift)) {
                transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * InputProxy.GetAxis ("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * InputProxy.GetAxis ("Horizontal") * Time.deltaTime;
            } else if (InputProxy.GetKey (KeyCode.LeftControl) || InputProxy.GetKey (KeyCode.RightControl)) {
                transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * InputProxy.GetAxis ("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * InputProxy.GetAxis ("Horizontal") * Time.deltaTime;
			} else {
                transform.position += transform.forward * normalMoveSpeed * InputProxy.GetAxis ("Vertical") * Time.deltaTime;
                transform.position += transform.right * normalMoveSpeed * InputProxy.GetAxis ("Horizontal") * Time.deltaTime;
			}

            if (InputProxy.GetKey (KeyCode.Q)) {
				transform.position -= transform.up * climbSpeed * Time.deltaTime;
			}
            if (InputProxy.GetKey (KeyCode.E)) {
				transform.position += transform.up * climbSpeed * Time.deltaTime;
			}

			transform.position = new Vector3 (transform.position.x, 1, transform.position.z);

			if (!bounds.Contains (transform.position)) {
				transform.position = oldPos;
			}
		}

	}
}