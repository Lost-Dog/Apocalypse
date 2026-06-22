using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace ABCToolkit {
	public class ABC_GlobalElement : ScriptableObject {


		// ****************** Settings ***************************

		#region Settings 

		/// <summary>
		/// Icon of the exported element, shown in character creator etc
		/// </summary>
		public Texture2D elementIcon;

		/// <summary>
		/// If true then the asset preview will display, else the element icon
		/// </summary>
		public bool showWeaponPreview = true;

		/// <summary>
		/// Description of the exported element
		/// </summary>
		public string elementDescription;

		/// <summary>
		/// Tags for the element, used in filtering
		/// </summary>
		public List<string> elementTags = new List<string>();

		/// <summary>
		/// Date element was made
		/// </summary>
		public string creationDate = System.DateTime.Now.ToString();

		/// <summary>
		/// Who element was created by
		/// </summary>
		public string createdBy = Environment.UserName.ToString();

		/// <summary>
		/// If true then exported element is part of official asset
		/// </summary>
		public bool officialABC = false;

		/// <summary>
		/// If ability only then weapon objects not included
		/// </summary>
		public GlobalElementType elementType = GlobalElementType.Abilities;

		/// <summary>
		/// weapon attached to this element
		/// </summary>
		public ABC_Controller.Weapon ElementWeapon = null;

		/// <summary>
		/// abilities attached to this element
		/// </summary>
		public List<ABC_Ability> ElementAbilities = new List<ABC_Ability>();

		/// <summary>
		/// AI rules attached to this element
		/// </summary>
		public List<ABC_Controller.AIRule> ElementAIRules = new List<ABC_Controller.AIRule>();

		/// <summary>
		/// effects attached to this element
		/// </summary>
		public List<Effect> ElementEffects = new List<Effect>();

		#endregion


		// ****************** Public Methods ***************************

		#region Public Methods

		/// <summary>
		/// Will refresh ID's for Abilities & Weapons in this global element creating a new unique ID and making sure all links stay in place
		/// </summary>
		public void RefreshUniqueIDs() {

			//Abilities
			foreach (ABC_Ability ability in this.ElementAbilities) {

				if (ability.abilityID <= 0)
					continue;

				string currentID = String.Format(@"\b{0}\b", ability.abilityID.ToString());
				string newID = ABC_Utilities.GenerateUniqueID().ToString();

				string newValues = Regex.Replace(JsonUtility.ToJson(this), currentID, newID);

				JsonUtility.FromJsonOverwrite(newValues, this);
			}

			//Weapon

			if (this.elementType == GlobalElementType.Weapon && this.ElementWeapon != null && this.ElementWeapon.weaponID > 0) {


				string currentID = String.Format(@"\b{0}\b", this.ElementWeapon.weaponID.ToString());
				string newID = ABC_Utilities.GenerateUniqueID().ToString();

				string newValues = Regex.Replace(JsonUtility.ToJson(this), currentID, newID);

				JsonUtility.FromJsonOverwrite(newValues, this);
			}

			//Effects
			if (this.elementType == GlobalElementType.Effect && this.ElementEffects != null) {
				foreach (Effect effect in this.ElementEffects) {

					string currentID = String.Format(@"\b{0}\b", effect.effectID.ToString());
					string newID = ABC_Utilities.GenerateUniqueID().ToString();

					string newValues = Regex.Replace(JsonUtility.ToJson(this), currentID, newID);

					JsonUtility.FromJsonOverwrite(newValues, this);
				}
			}



			this.creationDate = System.DateTime.Now.ToString();

			if (this.officialABC == true)
				this.createdBy = "ABC";
			else
				this.createdBy = Environment.UserName.ToString();



		}


		#endregion


		// ********* ENUMS **********************


		#region  ENUMs

		public enum GlobalElementType {
			Weapon,
			Abilities,
			Effect,
			AIRules
		}


		#endregion

	}
}