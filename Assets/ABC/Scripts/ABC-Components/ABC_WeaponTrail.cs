using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABCToolkit {
	/// <summary>
	/// Will render a weapon trail between the start and end point
	/// </summary>
	[System.Serializable]
	public class ABC_WeaponTrail : MonoBehaviour {

		// ********************* Settings ********************
		#region Settings


		/// <summary>
		/// The base of the weapon for the trail position
		/// </summary>
		public Transform weaponBase = null;

		/// <summary>
		/// The tip of the weapon for the trail position
		/// </summary>
		public Transform weaponTip = null;

		/// <summary>
		/// Trail material 
		/// </summary>
		public Material trailMaterial = null;

		/// <summary>
		/// How long the trail points are active for
		/// </summary>
		public float trailLifeTime = 1.5f;


		/// <summary>
		/// The trail colours
		/// </summary>
		public List<Color> trailColours;

		#endregion



		// ********************* Variables ********************
		#region Variables

		/// <summary>
		/// Determines if the trail will render or not
		/// </summary>
		private bool Enabled = false;

		/// <summary>
		/// The trail object
		/// </summary>
		private GameObject weaponTrailObject;

		/// <summary>
		/// Mesh of the weapon trail 
		/// </summary>
		private Mesh weaponTrailMesh;

		/// <summary>
		/// The last known position of the trail object
		/// </summary>
		private Vector3 previousPosition;

		[System.Serializable]
		public class TrailPoint {
			public float timeCreated = 0.0f;
			public Vector3 basePosition;
			public Vector3 tipPosition;
		}

		/// <summary>
		/// The current active trail points
		/// </summary>
		private List<TrailPoint> trailPoints = new List<TrailPoint>();

		/// <summary>
		/// Records when script was last activated
		/// </summary>
		private float lastActivateTime = 0f;


		#endregion


		// ********************* Private Methods ********************
		#region Private Methods

		/// <summary>
		/// Will remove all the old trail points that has reached it's life duration
		/// </summary>
		void RemoveOldTrailPoints() {
			List<TrailPoint> remove = new List<TrailPoint>();
			foreach (TrailPoint trailPoint in this.trailPoints) {
				// cull old points first
				if (Time.time - trailPoint.timeCreated > this.trailLifeTime) {
					remove.Add(trailPoint);
				}
			}
			foreach (TrailPoint trailPoint in remove) {
				this.trailPoints.Remove(trailPoint);
			}
		}

		/// <summary>
		/// The main code to process the trail rendering
		/// </summary>
		private void WeaponTrailManager() {
			//If no base or tip has been added end here
			if (this.weaponBase == null || this.weaponTip == null) {
				Debug.LogWarning("Weapon Base or Tip not configured for weapon's trail");
				return;
			}

			// if we have moved then start the manager
			float distanceMoved = (this.previousPosition - transform.position).sqrMagnitude;
			if (distanceMoved > 0f) {
				bool createNewPoint = false;
				if (this.trailPoints.Count < 3) {
					createNewPoint = true;
				} else {
					Vector3 l1 = this.trailPoints[this.trailPoints.Count - 2].tipPosition - this.trailPoints[this.trailPoints.Count - 3].tipPosition;
					Vector3 l2 = this.trailPoints[this.trailPoints.Count - 1].tipPosition - this.trailPoints[this.trailPoints.Count - 2].tipPosition;

					if (Vector3.Angle(l1, l2) > 0f || distanceMoved > 0f)
						createNewPoint = true;
				}

				if (createNewPoint) {
					TrailPoint newPoint = new TrailPoint();
					newPoint.basePosition = this.weaponBase.position;
					newPoint.tipPosition = this.weaponTip.position;
					newPoint.timeCreated = Time.time;
					this.trailPoints.Add(newPoint);
					this.previousPosition = transform.position;


				} else {
					this.trailPoints[this.trailPoints.Count - 1].basePosition = this.weaponBase.position;
					this.trailPoints[this.trailPoints.Count - 1].tipPosition = this.weaponTip.position;


				}
			} else {
				if (this.trailPoints.Count > 0) {
					this.trailPoints[this.trailPoints.Count - 1].basePosition = this.weaponBase.position;
					this.trailPoints[this.trailPoints.Count - 1].tipPosition = this.weaponTip.position;
				}


			}

			//Clean up any old points
			this.RemoveOldTrailPoints();


			//Record the current points in action ready to render
			List<TrailPoint> pointsInAction = this.trailPoints;

			if (pointsInAction.Count > 1) {
				Vector3[] newVertices = new Vector3[pointsInAction.Count * 2];
				Vector2[] newUV = new Vector2[pointsInAction.Count * 2];
				int[] newTriangles = new int[(pointsInAction.Count - 1) * 6];
				Color[] newColors = new Color[pointsInAction.Count * 2];

				for (int n = 0; n < pointsInAction.Count; ++n) {
					TrailPoint trailPoint = pointsInAction[n];
					float time = (Time.time - trailPoint.timeCreated) / this.trailLifeTime;

					Color color = Color.Lerp(Color.white, Color.clear, time);
					if (this.trailColours != null && this.trailColours.Count > 0) {
						float colorTime = time * (this.trailColours.Count - 1);
						float min = Mathf.Floor(colorTime);
						float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, this.trailColours.Count - 1);
						float lerp = Mathf.InverseLerp(min, max, colorTime);
						if (min >= this.trailColours.Count) min = this.trailColours.Count - 1; if (min < 0) min = 0;
						if (max >= this.trailColours.Count) max = this.trailColours.Count - 1; if (max < 0) max = 0;
						color = Color.Lerp(this.trailColours[(int)min], this.trailColours[(int)max], lerp);
					}


					float size = 0.4f;

					Vector3 lineDirection = trailPoint.tipPosition - trailPoint.basePosition;

					newVertices[n * 2] = trailPoint.basePosition - (lineDirection * (size * 0.5f));
					newVertices[(n * 2) + 1] = trailPoint.tipPosition + (lineDirection * (size * 0.5f));

					newColors[n * 2] = newColors[(n * 2) + 1] = color;

					float uvRatio = (float)n / pointsInAction.Count;
					newUV[n * 2] = new Vector2(uvRatio, 0);
					newUV[(n * 2) + 1] = new Vector2(uvRatio, 1);

					if (n > 0) {
						newTriangles[(n - 1) * 6] = (n * 2) - 2;
						newTriangles[((n - 1) * 6) + 1] = (n * 2) - 1;
						newTriangles[((n - 1) * 6) + 2] = n * 2;

						newTriangles[((n - 1) * 6) + 3] = (n * 2) + 1;
						newTriangles[((n - 1) * 6) + 4] = n * 2;
						newTriangles[((n - 1) * 6) + 5] = (n * 2) - 1;
					}
				}

				this.weaponTrailMesh.Clear();
				this.weaponTrailMesh.vertices = newVertices;
				this.weaponTrailMesh.colors = newColors;
				this.weaponTrailMesh.uv = newUV;
				this.weaponTrailMesh.triangles = newTriangles;
			}



		}



		#endregion

		// ********************* Public Methods ********************
		#region Public Methods

		/// <summary>
		/// Will setup the weapon trail object ready 
		/// </summary>
		public void Setup() {

			if (this.weaponTrailObject == null) {

				//Setup game object 
				this.weaponTrailObject = new GameObject(this.transform.name + "_ABC_WeaponTrail");
				this.weaponTrailObject.SetActive(false);
				this.weaponTrailObject.transform.parent = null;
				this.weaponTrailObject.transform.position = Vector3.zero;
				this.weaponTrailObject.transform.rotation = Quaternion.identity;
				this.weaponTrailObject.transform.localScale = Vector3.one;
				this.weaponTrailObject.AddComponent(typeof(MeshFilter));
				this.weaponTrailObject.AddComponent(typeof(MeshRenderer));
				this.weaponTrailObject.GetComponent<Renderer>().material = this.trailMaterial;

				//Setup mesh
				this.weaponTrailMesh = new Mesh();
				this.weaponTrailMesh.name = name + "TrailMesh";
				this.weaponTrailObject.GetComponent<MeshFilter>().mesh = this.weaponTrailMesh;
			}

			//Pool object ready to be used at right time
			ABC_Utilities.PoolObject(this.weaponTrailObject);

		}


		/// <summary>
		/// Will activate the weapon trail for the duration provided
		/// </summary>
		/// <param name="Duration">How long to render the weapon trail for</param>
		/// <param name="Delay">Delay before weapon trail shows</param>
		/// <param name="TrailColours">List of colours to render on the trail</param>	
		/// <param name="ActivatedAbility">(Optional)The ability that activated the weapon trail</param>
		public IEnumerator Activate(float Duration, float Delay, List<Color> TrailColours, ABC_Ability ActivatedAbility = null) {

			//Record the new trails provided
			if (TrailColours != null)
				this.trailColours = TrailColours;

			//Record the time the function was requested
			float functionRequestTime = Time.time;
			this.lastActivateTime = functionRequestTime;


			// Wait for delay declared before we continue to destroy the projectile 
			if (Delay > 0f) {

				//wait for duration then end animation
				for (var i = Delay; i > 0;) {

					// actual wait time 
					if (i < 0.2f) {
						// less then 0.2  so we only need to wait the .xx time
						yield return new WaitForSeconds(i);
					} else {
						// wait a small amount and keep looping till casting time = 0; 
						yield return new WaitForSeconds(0.2f);
					}

					//reduce time left unless ability is currently in hit stop then things are on hold 
					if (ActivatedAbility == null || ActivatedAbility.hitStopCurrentlyActive == false)
						i = i - 0.2f;
				}

			}

			//Clean up any old points
			this.trailPoints.Clear();

			if (this.weaponTrailMesh != null)
				this.weaponTrailMesh.Clear();


			//Record last position
			this.previousPosition = this.transform.position;

			//Get the weapon trail object
			if (this.weaponTrailObject == null) {
				Setup();
			}

			//If function has been activated since then end here
			if (lastActivateTime > functionRequestTime)
				yield break;

			this.weaponTrailObject.transform.SetParent(null);
			this.weaponTrailObject.SetActive(true);

			//Activate component
			this.Enabled = true;

			//wait for duration	
			if (Duration > 0f) {

				//wait for duration then end animation
				for (var i = Duration; i > 0;) {

					// actual wait time 
					if (i < 0.2f) {
						// less then 0.2  so we only need to wait the .xx time
						yield return new WaitForSeconds(i);
					} else {
						// wait a small amount and keep looping till casting time = 0; 
						yield return new WaitForSeconds(0.2f);
					}

					//reduce time left unless ability is currently in hit stop then things are on hold 
					if (ActivatedAbility == null || ActivatedAbility.hitStopCurrentlyActive == false)
						i = i - 0.2f;
				}

			}

			//If function has been activated since then end here
			if (lastActivateTime > functionRequestTime)
				yield break;


			//Disable component
			this.Enabled = false;

			//Return trail object to the pool
			ABC_Utilities.PoolObject(this.weaponTrailObject);

		}

		/// <summary>
		/// Will interrupt the weapon trail 
		/// </summary>
		public void InterruptTrail() {

			this.lastActivateTime = Time.time;

			//Disable component
			this.Enabled = false;

			//Return trail object to the pool
			ABC_Utilities.PoolObject(this.weaponTrailObject);



		}

		#endregion


		// ********************** Game ******************

		#region Game


		void LateUpdate() {

			//If enabled run the weapon trail manager
			if (this.Enabled == true) {
				this.WeaponTrailManager();
			}
		}


		private void OnDisable() {
			//On disable pool object
			ABC_Utilities.PoolObject(this.weaponTrailObject);
			this.Enabled = false;

		}

		private void OnDestroy() {
			//On destroy clean up weapon trail object 
			GameObject.Destroy(this.weaponTrailObject);
		}




		#endregion
	}
}