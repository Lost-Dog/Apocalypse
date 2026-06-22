using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace ABCToolkit {
    public class ABC_ImportGlobalElement_EditorWindow : EditorWindow {

        // ************ Settings *****************************

        #region Settings

        /// <summary>
        /// type of global element we exporting (abilities, weapons etc)
        /// </summary>
        public GlobalElementType elementType = GlobalElementType.Abilities;

        /// <summary>
        /// The import type - link will add the global element, copy will copy over the global elements
        /// </summary>
        public ImportType importType = ImportType.Link;

        /// <summary>
        /// Used to refocus the current window if provided
        /// </summary>
        public ABC_ControllerAbility_EditorWindow CurrentAbilityWindow;

        /// <summary>
        /// If true then the abilities will be modified at run type to match the game type selected
        /// </summary>
        public bool globalAbilitiesEnableGameTypeModification = false;

        /// <summary>
        /// What game type to modify the global abilities to
        /// </summary>
        public ABC_GlobalPortal.GameType globalAbilitiesGameTypeModification = ABC_GlobalPortal.GameType.Action;


        /// <summary>
        /// Import abilities attached to the weapon
        /// </summary>
        public bool importWeaponAbilities = true;

        /// <summary>
        /// Import AI rules attached to abilities
        /// </summary>
        public bool importAbilitiesAIRules = false;



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

        public void ResetLabelWidth() {

            EditorGUIUtility.labelWidth = 110;

        }


        public void InspectorHelpBoxFullWidth(string text, bool space = true, bool alwaysShow = false) {

            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            myStyle.wordWrap = true;
            myStyle.fixedWidth = 0;
            myStyle.fontSize = 10;
            myStyle.fontStyle = FontStyle.Normal;

            if (EditorGUIUtility.isProSkin) {
                GUI.color = inspectorSectionHelpProColor;
            } else {
                GUI.color = inspectorSectionHelpColor;
            }
            EditorGUILayout.LabelField(text, myStyle);

            if (space == true) {
                EditorGUILayout.Space();
            }



            GUI.color = Color.white;



        }


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
        //Entity we are importing too
        public ABC_Controller importingEntity = null;

        [HideInInspector]
        //Global element we are importing too
        public ABC_GlobalElement importingGlobalElement = null;

        [HideInInspector]
        //Global element we are importing from
        public ABC_GlobalElement globalElement = null;


        #endregion

        // ********************* Private Methods ********************

        #region Private Methods


        public void ImportAbilities() {

            if (importingEntity == null && importingGlobalElement == null) {
                Debug.LogWarning("Import failed: ABC Controller Component not found");
                return;
            }

            //No abilities so end here
            if (this.globalElement.ElementAbilities.Count == 0)
                return;


            List<ABC_Ability> currentAbilities = new List<ABC_Ability>();

            if (this.importingEntity != null) {
                currentAbilities = this.importingEntity.Abilities;
            } else {
                currentAbilities = this.importingGlobalElement.ElementAbilities;
            }



            switch (this.importType) {
                case ImportType.Link:


                    if (currentAbilities.Where(a => a.globalAbilities == this.globalElement).Count() == 0 || currentAbilities.Where(a => a.globalAbilities == this.globalElement).Count() > 0
                        && EditorUtility.DisplayDialog("Global Abilities Already Linked", "Global Abilities are already linked. Do you want to add another link to the Entity? ", "Yes", "No")) {

                        ABC_Ability newGlobalAbility = new ABC_Ability();

                        newGlobalAbility.globalAbilities = this.globalElement;

                        // get unique effect ID
                        newGlobalAbility.abilityID = -1;
                        newGlobalAbility.globalAbilitiesEnableGameTypeModification = this.globalAbilitiesEnableGameTypeModification;
                        newGlobalAbility.globalAbilitiesGameTypeModification = this.globalAbilitiesGameTypeModification;

                        currentAbilities.Add(newGlobalAbility);


                    }


                    break;

                case ImportType.Copy:

                    if (currentAbilities.Where(a => this.globalElement.ElementAbilities.Any(ea => ea.abilityID == a.abilityID)).Count() == 0 ||
                        currentAbilities.Where(a => this.globalElement.ElementAbilities.Any(ea => ea.abilityID == a.abilityID)).Count() > 0 &&
                        EditorUtility.DisplayDialog("Global Abilities Already Exist", "Global Abilities already exist on the Entity. Do you want to continue copying the Abilities to the Entity?", "Yes", "No")) {

                        if (this.importingEntity != null) {
                            currentAbilities = this.importingEntity.Abilities;
                        } else {
                            currentAbilities = this.importingGlobalElement.ElementAbilities;
                        }


                        foreach (ABC_Ability ability in this.globalElement.ElementAbilities) {

                            ABC_Ability newAbility = new ABC_Ability();
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ability), newAbility);

#if ABC_GC_2_Integration
                    if (this.importingEntity != null && importingEntity.transform.GetComponent<GameCreator.Runtime.Characters.Character>() != null)
                         newAbility.AdjustAbilityForGameCreator2();
#endif

                            //Game type modification if enabled
                            if (this.globalAbilitiesEnableGameTypeModification)
                                newAbility.ConvertToGameType(this.globalAbilitiesGameTypeModification);

                            currentAbilities.Add(newAbility);

                        }


                    }


                    break;
            }


            //Attached to weapon if weapon type
            if (this.importingEntity == null && this.importingGlobalElement != null && this.importingGlobalElement.elementType == ABC_GlobalElement.GlobalElementType.Weapon && this.importingGlobalElement.ElementWeapon != null) {

                foreach (ABC_Ability ability in ABC_Utilities.GetAbilitiesFromGlobalElement(this.globalElement)) {

                    if (this.importingGlobalElement.ElementWeapon.enableAbilityIDs.Contains(ability.abilityID) == false)
                        this.importingGlobalElement.ElementWeapon.enableAbilityIDs.Add(ability.abilityID);

                }
            }

            //AI Rules
            List<ABC_Controller.AIRule> currentAIRules = new List<ABC_Controller.AIRule>();

            if (this.importingEntity != null) {
                currentAIRules = this.importingEntity.AIRules;
            } else {
                currentAIRules = this.importingGlobalElement.ElementAIRules;
            }

            List<ABC_Controller.AIRule> newAIRules = new List<ABC_Controller.AIRule>();


            if (importAbilitiesAIRules == true) {
                //Get all rules of ability activations linked to ability
                foreach (ABC_Controller.AIRule rule in globalElement.ElementAIRules) {

                    //If rule already exists then continue 
                    if (currentAIRules.Where(ai => ai.selectedAIAction == rule.selectedAIAction && ai.AIAbilityID == rule.AIAbilityID).Count() > 0)
                        continue;

                    ABC_Controller.AIRule newRule = new ABC_Controller.AIRule();
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(rule), newRule);
                    newAIRules.Add(newRule);
                }


                //Get all rules of any linked abilities
                foreach (ABC_Ability elementAbilities in globalElement.ElementAbilities) {

                    if (elementAbilities.globalAbilities == null)
                        continue;

                    //Get all rules of ability activations linked to ability
                    foreach (ABC_Controller.AIRule rule in elementAbilities.globalAbilities.ElementAIRules) {

                        //If rule already exists then continue 
                        if (currentAIRules.Where(ai => ai.selectedAIAction == rule.selectedAIAction && ai.AIAbilityID == rule.AIAbilityID).Count() > 0)
                            continue;

                        //If rule already exists then continue 
                        if (newAIRules.Where(ai => ai.selectedAIAction == rule.selectedAIAction && ai.AIAbilityID == rule.AIAbilityID).Count() > 0)
                            continue;

                        ABC_Controller.AIRule newRule = new ABC_Controller.AIRule();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(rule), newRule);
                        newAIRules.Add(newRule);
                    }

                }
            }



            currentAIRules.AddRange(newAIRules);

        }



        /// <summary>
        /// Exports weapon for the entity
        /// </summary>
        public void ImportWeapon() {

#if UNITY_EDITOR // only useable in editor, without build errors will occur due to using Unity Editor namespace

            if (importingEntity == null) {
                Debug.LogWarning("Import failed: ABC Controller Component not found");
                return;
            }


            switch (this.importType) {
                case ImportType.Link:

                    if (this.importingEntity.Weapons.Where(w => w.globalWeapon == this.globalElement).Count() == 0 || this.importingEntity.Weapons.Where(w => w.globalWeapon == this.globalElement).Count() > 0
                     && EditorUtility.DisplayDialog("Global Weapon Already Linked", "Global Weapon already exist on the Entity.  Do you want to add another link to the Entity?", "Yes", "No")) {

                        ABC_Controller.Weapon newGlobalWeapon = new ABC_Controller.Weapon();

                        newGlobalWeapon.globalWeapon = this.globalElement;

                        // get unique effect ID
                        newGlobalWeapon.weaponID = -1;

                        this.importingEntity.Weapons.Add(newGlobalWeapon);

                        this.importingEntity.CurrentWeaponIndex = this.importingEntity.Weapons.Count() - 1;

                    }

                    break;

                case ImportType.Copy:

                    if (this.importingEntity.Weapons.Where(w => w.globalWeapon == this.globalElement).Count() == 0 || this.importingEntity.Weapons.Where(w => w.globalWeapon == this.globalElement).Count() > 0
                     && EditorUtility.DisplayDialog("Global Weapon Already Exists", "Global Weapon already linked. Do you want to continue copying the Weapon to the Entity?", "Yes", "No")) {

                        ABC_Controller.Weapon newWeapon = new ABC_Controller.Weapon();
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this.globalElement.ElementWeapon), newWeapon);
                        this.importingEntity.Weapons.Add(newWeapon);

                    }

                    break;
            }

            //add abilities 
            if (this.importWeaponAbilities == true)
                this.ImportAbilities();



#endif

        }

        #endregion



        // ********************* GUI Methods ********************

        #region Methods


        SerializedObject GetTarget;

        public void OnEnable() {

            GUIContent titleContent = new GUIContent("Import Global Element");
            this.titleContent = titleContent;
            this.maxSize = new Vector2(350, 280);
            this.minSize = this.maxSize;

            AddIcon = (Texture2D)Resources.Load("ABC-EditorIcons/Add");


            GetTarget = new SerializedObject(this);

        }

        void OnGUI() {

            EditorGUIUtility.labelWidth = 90;

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(GetTarget.FindProperty("importType"), GUILayout.MaxWidth(200));
            EditorGUILayout.Space();
            if (this.importType == ImportType.Link) {
                InspectorHelpBoxFullWidth("Element will be linked. Changes made to the global element will be applied when the game starts");
            } else {
                InspectorHelpBoxFullWidth("Will create copies of the element to the entity. Changes made to the global element will not change the elements copied.");
            }


            EditorGUIUtility.labelWidth = 200;

            if (this.elementType == GlobalElementType.Weapon) {
                EditorGUILayout.PropertyField(GetTarget.FindProperty("importWeaponAbilities"));
            }

            if (this.elementType == GlobalElementType.Abilities || this.elementType == GlobalElementType.Weapon && this.importWeaponAbilities == true) {

                if (importingGlobalElement == null) {

                    EditorGUIUtility.labelWidth = 200;
                    EditorGUILayout.PropertyField(GetTarget.FindProperty("globalAbilitiesEnableGameTypeModification"), new GUIContent("Enable Game Type Modification"));
                    EditorGUIUtility.labelWidth = 90;

                    EditorGUILayout.Space();

                    if (this.globalAbilitiesEnableGameTypeModification == true) {
                        EditorGUILayout.PropertyField(GetTarget.FindProperty("globalAbilitiesGameTypeModification"), new GUIContent("Game Type"), GUILayout.MaxWidth(220));
                    }

                    EditorGUILayout.Space();

                    if (this.globalAbilitiesEnableGameTypeModification == true) {

                        switch (this.globalAbilitiesGameTypeModification) {

                            case ABC_GlobalPortal.GameType.Action:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'Action' Game Type. Ability will activate on the the nearest enemy targetted and will always activate even if no target exists");
                                break;
                            case ABC_GlobalPortal.GameType.FPS:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'FPS' Game Type. Ability will activate towards the Crosshair");
                                break;
                            case ABC_GlobalPortal.GameType.TPS:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'TPS' Game Type. Ability will activate towards the Crosshair, Melee Ability will attack the nearest enemy targetted.");
                                break;
                            case ABC_GlobalPortal.GameType.RPGMMO:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'RPG/MMO' Game Type. Ability will require a target before activating. Melee attacks will always hit.");
                                break;
                            case ABC_GlobalPortal.GameType.MOBA:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'MOBA' Game Type. Ability will need to be chosen before a second click determines the direction the Ability will travel.");
                                break;
                            case ABC_GlobalPortal.GameType.TopDownAction:
                                InspectorHelpBoxFullWidth("Abilities will be setup based on the 'Top Down Action' Game Type. Abilities and Attacks will activate towards mouse direction. If mouse direction is near a target the Ability will activate towards the target instead.");
                                break;
                        }
                    }
                }
            }

            if (this.elementType == GlobalElementType.Abilities || this.elementType == GlobalElementType.Weapon && this.importWeaponAbilities == true) {
                EditorGUIUtility.labelWidth = 160;
                EditorGUILayout.PropertyField(GetTarget.FindProperty("importAbilitiesAIRules"));
                EditorGUILayout.Space();
            }


            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import")) {

                //If this is a copy then lets duplicate the global element we are importing but generate new ID's (only link's keep ID's) 
                if (this.importType == ImportType.Copy) {

                    ABC_GlobalElement newGlobalElement = ScriptableObject.CreateInstance<ABC_GlobalElement>();

                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this.globalElement), newGlobalElement);
                    newGlobalElement.RefreshUniqueIDs();

                    this.globalElement = newGlobalElement;

                }

                if (this.elementType == GlobalElementType.Abilities) {
                    this.ImportAbilities();

                    //Set focus back to ability window 
                    if (CurrentAbilityWindow != null)
                        CurrentAbilityWindow.Setup();

                } else if (this.elementType == GlobalElementType.Weapon) {
                    this.ImportWeapon();
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

        public enum ImportType {
            Link,
            Copy
        }



        #endregion
    }
}