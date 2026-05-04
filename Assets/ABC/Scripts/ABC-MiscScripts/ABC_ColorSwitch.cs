using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
	public class ABC_ColorSwitch : MonoBehaviour {


		// ********************* Settings ********************

		#region Settings

		/// <summary>
		/// Color to change object too
		/// </summary>
		public Color Color;

		/// <summary>
		/// Delay before colour is changed
		/// </summary>
		public float switchDelay = 0;

		/// <summary>
		/// Duration till colour is changed back
		/// </summary>
		public float switchDuration = 0.1f;

		/// <summary>
		/// If true then emission colour is used instead
		/// </summary>
		public bool useEmission = false;

		/// <summary>
		/// If enabled then the object will switch colour
		/// </summary>
		public bool activateColourSwitch = false;

		#endregion


		// ********************* Variables ********************
		#region Variables

		/// <summary>
		/// Determines if color switch is already running
		/// </summary>
		private bool colorSwitchInProgress = false;

		/// <summary>
		/// List of renderers on the object
		/// </summary>
		private List<Renderer> entityRenderers = new List<Renderer>();


		/// <summary>
		/// Material property block we will be modifying and applying to change colour
		/// </summary>
		private MaterialPropertyBlock matPropertyBlock;



		#endregion


		// ********************* Private Methods ********************
		#region Private Methods


		/// <summary>
		/// Will activate the object shake
		/// </summary>
		private IEnumerator ActivateColorSwitch() {

			//If no renderers exist or procedure already running then end here
			if (this.entityRenderers.Count == 0 || this.colorSwitchInProgress == true)
				yield break;


			//Tell code switch is in progress
			this.colorSwitchInProgress = true;

			//Wait for delay
			if (this.switchDelay > 0f)
				yield return new WaitForSeconds(this.switchDelay);


			//For each renderer found change the color or emission color
			foreach (Renderer renderer in this.entityRenderers) {

				// Get the current value of the material properties in the renderer.
				renderer.GetPropertyBlock(this.matPropertyBlock);

				//Set color type
				string colorType = "_Color";

				//Use emission if set to and enabled on the material
				if (this.useEmission == true && renderer.material.globalIlluminationFlags != MaterialGlobalIlluminationFlags.EmissiveIsBlack)
					colorType = "_EmissionColor";

				// Assign our new value.
				this.matPropertyBlock.SetColor(colorType, Color);

				// Apply the edited values to the renderer.
				renderer.SetPropertyBlock(this.matPropertyBlock);

			}


			//wait for duration
			yield return new WaitForSeconds(this.switchDuration);


			//Clear the property block to revert the colour
			foreach (Renderer renderer in this.entityRenderers) {

				// Get the current value of the material properties in the renderer.
				renderer.GetPropertyBlock(this.matPropertyBlock);

				this.matPropertyBlock.Clear();

				// Apply the edited values to the renderer.
				renderer.SetPropertyBlock(this.matPropertyBlock);

			}


			//Tell code switch has finished
			this.colorSwitchInProgress = false;


		}





		#endregion



		// ********************* Public Methods ********************
		#region Public Methods

		/// <summary>
		/// Will activate the color switch for a duration before reverting back to the objects original color
		/// </summary>
		/// <param name="Color">Color to switch too for the duration</param>
		/// <param name="Duration">The duration the color will switch to before reverting back</param>
		/// <param name="Delay">Delay before the switch occurs</param>
		/// <param name="UseEmission">If true then the emission color is changed (if enabled), else if false color property is changed</param>
		public void ActivateColorSwitch(Color Color, float Duration, float Delay = 0, bool UseEmission = false) {

			//Assign values
			this.Color = Color;
			this.switchDuration = Duration;
			this.switchDelay = Delay;
			this.useEmission = UseEmission;

			//Activate
			StartCoroutine(this.ActivateColorSwitch());

		}


		#endregion


		// ********************* Game ********************

		#region Game

		void Awake() {

			//Create new material block to play with 
			this.matPropertyBlock = new MaterialPropertyBlock();

			//Collect all renderers on the level below the parent (normal structure for characters) this will then ignore things like weapons
			foreach (Renderer renderer in this.transform.GetComponentsInChildren<Renderer>()) {

				if (renderer.transform.parent == this.transform || renderer.transform.parent == null)
					this.entityRenderers.Add(renderer);

			}

		}

		void Update() {

			//For Component Editor testing 
			if (this.activateColourSwitch == true) {
				StartCoroutine(this.ActivateColorSwitch());
				this.activateColourSwitch = false;
			}


		}

		#endregion










		//public Color Color1, Color2;
		//   public float Speed = 1, Offset;

		//   private Renderer _renderer;
		//   private MaterialPropertyBlock _propBlock;

		//   void Awake() {
		//       this._propBlock = new MaterialPropertyBlock();
		//       _renderer = GetComponent<Renderer>();
		//   }

		//   void Update() {
		//	// Get the current value of the material properties in the renderer.
		//	_renderer.GetPropertyBlock(_propBlock);
		//	// Assign our new value.
		//	_propBlock.SetColor("_Color", Color.Lerp(Color1, Color2, (Mathf.Sin(Time.time * Speed + Offset) + 1) / 2f));

		//	// Apply the edited values to the renderer.
		//	_renderer.SetPropertyBlock(_propBlock);
		//}
	}
}

