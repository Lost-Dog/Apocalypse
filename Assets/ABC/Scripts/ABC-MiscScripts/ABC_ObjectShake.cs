using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ABCToolkit {
	/// <summary>
	/// Will shake the entity 
	/// </summary>
	public class ABC_ObjectShake : MonoBehaviour {


		// ********************* Settings ********************

		#region Settings

		/// <summary>
		/// The amount of shake performed 
		/// </summary>
		[Range(0f, 1f)]
		public float shakeAmount = 0.1f;

		/// <summary>
		/// The amount of the shake decreased each cycle. Also determines the duration, once shake decays to 0 the shake will stop
		/// </summary>
		[Range(0f, 1f)]
		public float shakeDecay = 0.007f;


		/// <summary>
		/// If enabled then the object will start shaking
		/// </summary>
		public bool shake = false;

		#endregion


		// ********************* Variables ********************
		#region Variables

		/// <summary>
		/// The amount of current shake, once this reaches 0 the shake will stop
		/// </summary>
		private float currentShakeAmount = 0;

		/// <summary>
		/// Original rotation before shake occured
		/// </summary>
		private Vector3 originalRotation;

		/// <summary>
		/// Determines if a shake has occured
		/// </summary>
		private bool shakeActivated = false;

		#endregion


		// ********************* Private Methods ********************
		#region Private Methods

		/// <summary>
		/// Main code which shakes the object, ran in Update
		/// </summary>
		private void ShakeObject() {

			//While shake amount is higher then 0
			if (this.currentShakeAmount > 0) {

				//Shake randomly in position
				transform.position = this.transform.position + Random.insideUnitSphere * this.currentShakeAmount;
				transform.rotation = new Quaternion(
					transform.rotation.x + Random.Range(-this.currentShakeAmount, this.currentShakeAmount) * .2f,
					transform.rotation.y + Random.Range(-this.currentShakeAmount, this.currentShakeAmount) * .2f,
					transform.rotation.z + Random.Range(-this.currentShakeAmount, this.currentShakeAmount) * .2f,
					transform.rotation.w + Random.Range(-this.currentShakeAmount, this.currentShakeAmount) * .2f);

				//Lower the shake amount gradually till reaches 0
				this.currentShakeAmount -= this.shakeDecay;
			} else if (this.shakeActivated == true) {

				//Script has finished so lets revert rotation and disable 
				this.transform.rotation = Quaternion.Euler(this.originalRotation.x, this.originalRotation.y, this.originalRotation.z);
				Destroy(this);



			}

		}




		#endregion



		// ********************* Public Methods ********************
		#region Public Methods

		/// <summary>
		/// Will activate the object shake
		/// </summary>
		/// <param name="Amount">Amount to shake object by</param>
		/// <param name="Decay">The amount of the shake decreased each cycle. Also determines the duration, once shake decays to 0 the shake will stop</param>
		/// <param name="Delay">Delay till shake starts</param>
		public IEnumerator ActivateObjectShake(float Amount, float Decay, float Delay = 0f) {

			if (this.gameObject.activeInHierarchy == false)
				yield break;

			//Delay till shake starts
			if (Delay > 0f)
				yield return new WaitForSeconds(Delay);

			//Update shake amount and decay
			this.shakeAmount = Amount;
			this.shakeDecay = Decay;


			//Start the shake by updating the current shake amount (script will shake if not 0), unless already shaking 
			if (this.currentShakeAmount <= 0) {
				this.currentShakeAmount = this.shakeAmount;
				this.originalRotation = this.transform.eulerAngles;
				this.shakeActivated = true;
			}



		}

		#endregion


		// ********************* Game ********************

		#region Game

		void Update() {

			//For Component Editor testing 
			if (this.shake == true) {
				StartCoroutine(this.ActivateObjectShake(this.shakeAmount, this.shakeDecay));
				this.shake = false;
			}


			//Shake the object
			this.ShakeObject();


		}

		#endregion






	}
}