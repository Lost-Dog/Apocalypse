using UnityEngine;

namespace ABCToolkit {
	/// <summary>
	/// Component will freeze rotation to any object it is applied too
	/// </summary>
	public class ABC_FreezeRotation : MonoBehaviour {

		Transform meTransform;
		Quaternion startRotation;


		// Use this for initialization
		void Start() {
			meTransform = transform;
			//Records the initial rotation  
			startRotation = meTransform.rotation;

		}

		// Update is called once per frame
		void FixedUpdate() {

			// always make sure rotation does not change 
			meTransform.rotation = startRotation;

		}
	}
}
