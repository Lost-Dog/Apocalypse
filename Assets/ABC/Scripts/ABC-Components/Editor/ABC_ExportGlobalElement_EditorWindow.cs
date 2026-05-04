using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace ABCToolkit {
    public class ABC_ExportGlobalElement_EditorWindow : EditorWindow {

        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// Who created the global element
        /// </summary>
        public string createdBy = " ";

        /// <summary>
        /// Icon for the element
        /// </summary>
        public Texture2D elementIcon;

        /// <summary>
        /// Description for the element
        /// </summary>
        public string elementDescription;

        /// <summary>
        /// Tags for the element
        /// </summary>
        public List<string> elementTags = new List<string>();


        /// <summary>
        /// type of global element we exporting (abilities, weapons etc)
        /// </summary>
        public GlobalElementType elementType = GlobalElementType.Abilities;

        #region Ability Specific Settings

        /// <summary>
        /// export AI rules for abilities
        /// </summary>
        public bool exportAbilitiesAIRules = true;

        #endregion


        #region Weapon Specific Settings

        /// <summary>
        /// Export abilities attached to the weapon
        /// </summary>
        public bool exportWeaponAbilities = true;


        #endregion



        #endregion

        // ********************* Design ******************

        #region Design

        #region Window Colors
        public Color inspectorBackgroundColor = Color.white;
        public Color inspectorBackgroundProColor = new Color32(155, 185, 255, 255);

        public Color inspectorSectionHeaderColor = new Color32(137, 134, 134, 210);
        public Color inspectorSectionHeaderProColor = new Color32(165, 195, 255, 255);

        public Color inspectorSectionHeaderTextColor = new Color(0, 0.45f, 1, 1);
        public Color inspectorSectionHeaderTextProColor = new Color(1, 1, 1, 1f);

        public Color inspectorSectionBoxColor = new Color32(255, 255, 255, 190);
        public Color inspectorSectionBoxProColor = new Color32(0, 0, 0, 255);

        public Color inspectorSectionHelpColor = new Color32(113, 151, 243, 200);
        public Color inspectorSectionHelpProColor = new Color32(215, 235, 255, 255);
        #endregion

        Vector2 editorScrollPos;

        char UpArrowSymbol = '\u2191';
        char DownArrowSymbol = '\u2193';

        // Button Icons
        Texture AddIcon;





        public void InspectorListBox(string title, SerializedProperty list, bool expandWidth = false) {


            if (expandWidth) {
                EditorGUILayout.BeginVertical();
            } else {
                EditorGUILayout.BeginVertical(GUILayout.Width(300));
            }

            Color originalTextColor = GUI.skin.button.normal.textColor;

            GUI.color = new Color32(208, 212, 211, 255);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box(title, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(21) });
            GUI.color = Color.white;
            GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
            if (GUILayout.Button(new GUIContent(AddIcon), GUILayout.Width(30))) {
                list.InsertArrayElementAtIndex(list.arraySize);

                if (list.GetArrayElementAtIndex(list.arraySize - 1).type.ToString() == "string")
                    list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = "";
            }
            GUILayout.EndHorizontal();
            GUI.skin.button.normal.textColor = originalTextColor;

            editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos,
                                                                        false,
                                                                        false);

            if (list.arraySize > 0) {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < list.arraySize; i++) {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element, new GUIContent(""));
                    GUI.skin.button.normal.textColor = new Color(0, 0.45f, 1, 1);
                    if (GUILayout.Button(UpArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button(DownArrowSymbol.ToString())) {
                        list.MoveArrayElement(i, i + 1);
                    }


                    GUI.skin.button.normal.textColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(40))) {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            GUI.skin.button.normal.textColor = originalTextColor;
        }

        #endregion



        // ********************* Variables ******************

        #region Variables

        [HideInInspector]
        public ABC_Controller exportingEntity = null;

        [HideInInspector]
        public List<ABC_Ability> exportingAbilities = new List<ABC_Ability>();

        [HideInInspector]
        public int weaponIDToExport = -1;

        #endregion

        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Exports abilities for the entity
        /// </summary>
        public void ExportAbilities() {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace




            //Abilites
            List<ABC_Ability> newAbilities = new List<ABC_Ability>();

            if (exportingEntity != null) {
                exportingAbilities = exportingEntity.Abilities.Where(a => a.enableExport == true).ToList();
            }

            foreach (ABC_Ability ability in exportingAbilities) {
                ABC_Ability newAbility = new ABC_Ability();
                ability.enableExport = false;
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ability), newAbility);
                newAbilities.Add(newAbility);
                ability.enableExport = false;
            }


            //AI Rules
            List<ABC_Controller.AIRule> newAIRules = new List<ABC_Controller.AIRule>();

            if (exportAbilitiesAIRules == true && exportingEntity != null) {

                //Get all rules of ability activations linked to weapon
                foreach (ABC_Controller.AIRule rule in exportingEntity.AIRules.Where(ai => ai.selectedAIAction == ABC_Controller.AIRule.AIAction.ActivateAbility && newAbilities.Select(a => a.abilityID).ToList().Contains(ai.AIAbilityID))) {

                    ABC_Controller.AIRule newRule = new ABC_Controller.AIRule();
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(rule), newRule);
                    newAIRules.Add(newRule);
                }
            }


            // create an empty exported element object
            ABC_GlobalElement exportedElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();


            if (exportedElement.officialABC == true) {
                exportedElement.createdBy = this.createdBy;
            } else {
                exportedElement.createdBy = this.createdBy;
            }

            exportedElement.creationDate = System.DateTime.Now.ToString();
            exportedElement.elementIcon = this.elementIcon;
            exportedElement.elementDescription = this.elementDescription;
            exportedElement.elementTags = this.elementTags;

            exportedElement.elementType = ABC_GlobalElement.GlobalElementType.Abilities;
            exportedElement.ElementWeapon = null;
            exportedElement.ElementAbilities = newAbilities;
            exportedElement.ElementAIRules = newAIRules;

            //save to path 
            string fullPath = UnityEditor.EditorUtility.SaveFilePanel("Save Abilities", "Assets", "New ABC Global Ability", "asset");
            string basePath = fullPath.Replace(Application.dataPath, "Assets");
            UnityEditor.AssetDatabase.CreateAsset(exportedElement, basePath);


#endif

        }


        /// <summary>
        /// Exports weapon for the entity
        /// </summary>
        public void ExportWeapon() {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace


            //Get the weapon to export 
            ABC_Controller.Weapon weaponToExport = exportingEntity.Weapons.Where(w => w.weaponID == weaponIDToExport).FirstOrDefault();

            if (weaponToExport == null) {
                Debug.LogWarning("Export failed: Weapon not found");
                return;
            }

            //Store weapon object
            ABC_Controller.Weapon newWeapon = new ABC_Controller.Weapon();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(weaponToExport), newWeapon);


            //Abilites
            List<ABC_Ability> newAbilities = new List<ABC_Ability>();

            //AI Rules
            List<ABC_Controller.AIRule> newAIRules = new List<ABC_Controller.AIRule>();

            if (exportWeaponAbilities == true) {

                List<int> abilitiesToInclude = new List<int>();

                abilitiesToInclude.AddRange(weaponToExport.enableAbilityIDs);
                abilitiesToInclude.AddRange(weaponToExport.abilityIDsToEnableAfterBlocking);
                abilitiesToInclude.Add(weaponToExport.abilityIDToActivateAfterBlocking);
                abilitiesToInclude.AddRange(weaponToExport.abilityIDsToEnableAfterParrying);
                abilitiesToInclude.Add(weaponToExport.abilityIDToActivateAfterParrying);

                foreach (ABC_Ability ability in exportingEntity.Abilities) {


                    if (abilitiesToInclude.Contains(ability.abilityID) || ability.globalAbilities != null && ABC_Utilities.GetAbilitiesFromGlobalElement(ability.globalAbilities).Where(a => abilitiesToInclude.Contains(a.abilityID)).Count() > 0) {
                        ABC_Ability newAbility = new ABC_Ability();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ability), newAbility);

                        if (newAbilities.Where(a => a.abilityID == newAbility.abilityID).Count() == 0)
                            newAbilities.Add(newAbility);

                    }
                }


                if (exportAbilitiesAIRules == true) {

                    //Get all rules of ability activations linked to weapon
                    foreach (ABC_Controller.AIRule rule in exportingEntity.AIRules.Where(ai => ai.selectedAIAction == ABC_Controller.AIRule.AIAction.ActivateAbility && newAbilities.Select(a => a.abilityID).ToList().Contains(ai.AIAbilityID))) {

                        ABC_Controller.AIRule newRule = new ABC_Controller.AIRule();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(rule), newRule);
                        newAIRules.Add(newRule);
                    }
                }
            }


            // create an empty exported element object
            ABC_GlobalElement exportedElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();

            if (exportedElement.officialABC == true) {
                exportedElement.createdBy = "ABC";
            } else {
                exportedElement.createdBy = this.createdBy;
            }

            exportedElement.creationDate = System.DateTime.Now.ToString();
            exportedElement.elementIcon = this.elementIcon;
            exportedElement.elementDescription = this.elementDescription;
            exportedElement.elementTags = this.elementTags;

            exportedElement.elementType = ABC_GlobalElement.GlobalElementType.Weapon;
            exportedElement.ElementWeapon = newWeapon;
            exportedElement.ElementAbilities = newAbilities;
            exportedElement.ElementAIRules = newAIRules;

            //save to path 
            string fullPath = UnityEditor.EditorUtility.SaveFilePanel("Save Weapon", "Assets", "New ABC Weapon Element", "asset");
            string basePath = fullPath.Replace(Application.dataPath, "Assets");
            UnityEditor.AssetDatabase.CreateAsset(exportedElement, basePath);


#endif

        }

        #endregion




        // ********************* GUI Methods ********************

        #region Methods


        SerializedObject GetTarget;

        public void OnEnable() {

            GUIContent titleContent = new GUIContent("Create Global Element");
            this.titleContent = titleContent;
            this.maxSize = new Vector2(400, 400);
            this.minSize = this.maxSize;

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");


            GetTarget = new SerializedObject(this);

        }

        void OnGUI() {

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();



            EditorGUILayout.PropertyField(GetTarget.FindProperty("createdBy"));
            EditorGUILayout.PropertyField(GetTarget.FindProperty("elementIcon"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Description");
            GetTarget.FindProperty("elementDescription").stringValue = EditorGUILayout.TextArea(GetTarget.FindProperty("elementDescription").stringValue, GUILayout.MinHeight(80f));

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (this.elementType == GlobalElementType.Weapon) {
                EditorGUILayout.PropertyField(GetTarget.FindProperty("exportWeaponAbilities"));
            }

            if (this.exportingEntity != null && (this.elementType == GlobalElementType.Abilities || this.elementType == GlobalElementType.Weapon && this.exportWeaponAbilities == true)) {
                EditorGUILayout.PropertyField(GetTarget.FindProperty("exportAbilitiesAIRules"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            this.InspectorListBox("Element Tags", GetTarget.FindProperty("elementTags"), true);

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create")) {

                if (this.elementType == GlobalElementType.Abilities) {
                    this.ExportAbilities();
                } else if (this.elementType == GlobalElementType.Weapon) {
                    this.ExportWeapon();
                }

                this.Close();
                return;

            }


            if (GUILayout.Button("Cancel")) {
                this.Close();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();



            //Apply the changes to our list
            GetTarget.ApplyModifiedProperties();

        }


        //Target update and applymodifiedproperties are in the inspector update call to reduce lag. 
        public void OnInspectorUpdate() {

            //take current state of the SerializedObject, and updates the real object.
            GetTarget.ApplyModifiedProperties();

            //Double check any list edits will get saved
            if (GUI.changed)
                EditorUtility.SetDirty(this);


            //Update our list (takes the current state of the real object, and updates the SerializedObject)
            GetTarget.Update();


        }


        #endregion

        #region  ENUMs

        public enum GlobalElementType {
            Weapon,
            Abilities,
            Effect
        }


        #endregion



    }
}