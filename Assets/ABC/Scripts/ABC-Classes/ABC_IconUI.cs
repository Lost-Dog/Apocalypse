using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ABCToolkit {
    /// <summary>
    /// Class which manages Ability Icons including setting up the textures on the icon and initialising on clicks for the icon. Class is only used and managed by ABC Controller Component.
    /// </summary>
    [System.Serializable]
    public class ABC_IconUI {

        /// <summary>
        /// Create new object 
        /// </summary>
        public ABC_IconUI() {
        }


        // ****************** Settings ***************************

        #region IconUI Settings 

        /// <summary>
        /// Name of the IconUI which allows for easier modification in inspector
        /// </summary>
        [HideInInspector]
        public string iconName;

        /// <summary>
        /// ID of the IconUI
        /// </summary>
        [HideInInspector]
        public int iconID;

        /// <summary>
        /// The type of icon which is generated - ability activation etc
        /// </summary>
        [HideInInspector]
        [Tooltip("The type of icon which is generated")]
        public IconType iconType = IconType.EmptyIcon;

        /// <summary>
        /// The ID of the ability this IconUI relates too. ID is used so ability names can be changed without breaking the setup. 
        /// </summary>
        [Tooltip("ID of the ability which will activate")]
        [HideInInspector]
        public int iconUIAbilityID = -1;

        /// <summary>
        ///  used for inspector, keeps track of what ability is currently chosen for the IconUI 
        /// </summary>
        [HideInInspector]
        public int IconUIAbilityListChoice = 0;

        /// <summary>
        /// If true then the icon will be disabled when the ability is disabled
        /// </summary>
        [HideInInspector]
        [Tooltip("If true then the icon will be disabled when the ability is disabled")]
        public bool disableWithAbility = true;

        /// <summary>
        /// If true then an Substitute ability will take this icon for as long as the main ability is disabled
        /// </summary>
        public bool substituteAbilityWhenDisabled = false;

        /// The ID of the Substitute ability this IconUI relates too when the main ability is disabled. ID is used so ability names can be changed without breaking the setup. 
        /// </summary>
        [Tooltip("ID of the Substitute ability which will activate when the main ability is disabled")]
        [HideInInspector]
        public List<int> iconUISubstituteAbilityIDs = new List<int>();

        /// <summary>
        ///  used for inspector, keeps track of what ability is currently chosen for the Substitute IconUI 
        /// </summary>
        [HideInInspector]
        public int IconUISubstituteAbilityListChoice = 0;

        /// <summary>
        /// If true then the UI will display countdown information and grapics
        /// </summary>
        [Tooltip("If true then the UI will display countdown information and grapics")]
        [HideInInspector]
        public bool displayCountdown = true;

        /// <summary>
        /// Image object which will be placed over the image and will fill down 
        /// </summary>
        [Tooltip("Image object which will be placed over the image and will fill down ")]
        [HideInInspector]
        public ABC_ImageReference countdownFillOverlay = new ABC_ImageReference();

        /// <summary>
        /// Text object which will be used to display the current countdown
        /// </summary>
        [Tooltip("Text object which will be used to display the current countdown")]
        [HideInInspector]
        public ABC_TextReference countdownText = new ABC_TextReference();

        /// <summary>
        /// Image which displays for scroll ability activation texts
        /// </summary>
        [Tooltip("Image which displays for scroll ability activation texts")]
        [HideInInspector]
        public ABC_Texture2DReference ScrollAbilityActivationTexture = new ABC_Texture2DReference();

        /// <summary>
        /// The ID of the weapon this IconUI relates too. ID is used so weapon names can be changed without breaking the setup. 
        /// </summary>
        [Tooltip("ID of the weapon which will activate")]
        [HideInInspector]
        public int iconUIWeaponID = -1;

        /// <summary>
        ///  used for inspector, keeps track of what weapon is currently chosen for the IconUI 
        /// </summary>
        [HideInInspector]
        public int IconUIWeaponListChoice = 0;

        /// <summary>
        /// If true then the icon will be disabled when the weapon is disabled
        /// </summary>
        [HideInInspector]
        [Tooltip("If true then the icon will be disabled when the weapon is disabled")]
        public bool disableWithWeapon = true;

        /// <summary>
        /// If enable then the weapon will equip instantly on selection
        /// </summary>
        [Tooltip("If enable then the weapon will equip instantly on selection")]
        [HideInInspector]
        public bool weaponQuickEquip = false;


        /// <summary>
        /// Gameobject which will be created into an icon
        /// </summary>
        [HideInInspector]
        [Tooltip("Gameobject which will be created into an icon")]
        public ABC_GameObjectReference iconObject = new ABC_GameObjectReference();

        /// <summary>
        /// A list of tags for the Icon UI which can be used for filtering 
        /// </summary>
        [Tooltip("A list of tags for the Icon UI which can be used for filtering")]
        public List<string> iconTags = new List<string>();

        /// <summary>
        /// Defines what type this icon is. Swap allows the icon to swap positions with other swap icons. Source will place a clone of this icon at the destination.
        /// </summary>
        public ActionType actionType = ActionType.Dynamic;

        /// <summary>
        /// Can be clicked to initiate the buttons action
        /// </summary>
        [Tooltip("Can be clicked to initiate the buttons action")]
        public bool isClickable = true;

        /// <summary>
        /// If true then this icon will be removed if it is not dragged onto anything
        /// </summary>
        [Tooltip("If true then this icon will be removed if it is not dragged onto anything")]
        public bool removeOnEmptyDrag;

        /// <summary>
        /// Text to display what key is assigned to the icon
        /// </summary>
        [Tooltip("Text to display what key is assigned to the icon")]
        [HideInInspector]
        public ABC_TextReference keyText = new ABC_TextReference();

        /// <summary>
        /// If true then a string can be setup to override the button or key text
        /// </summary>
        [Tooltip("If true then a string can be setup to override the button or key text")]
        [HideInInspector]
        public bool overrideKeyText = false;

        /// <summary>
        /// An override string of the key text label
        /// </summary>
        [Tooltip("An override string of the key text label")]
        [HideInInspector]
        public string keyTextOverride;

        /// <summary>
        /// If true then the key assigned to the ability will be modified once it has been placed in a slot
        /// </summary>
        [Tooltip("Assign key on drop")]
        [HideInInspector]
        public bool clickFromTrigger = false;

        /// <summary>
        /// type of input to assign
        /// </summary>
        [Tooltip("type of input to assign")]
        [HideInInspector]
        public InputType clickFromTriggerInputType;

        /// <summary>
        /// If true then Icon can activate on key press
        /// </summary>
        [Tooltip("If true then Icon can activate on key press")]
        [HideInInspector]
        public bool clickFromTriggerOnKeyPress = true;

        /// <summary>
        /// If true then Icon can activate on key down
        /// </summary>
        [Tooltip("If true then Icon can activate on key down")]
        [HideInInspector]
        public bool clickFromTriggerKeyDown = false;

        /// <summary>
        /// Key to assign on drop 
        /// </summary>
        [HideInInspector]
        [Tooltip("Key to assign on drop")]
        public KeyCode clickTriggerKey;

        /// <summary>
        /// Button to assign on drop 
        /// </summary>
        [Tooltip("Button to assign on drop")]
        [HideInInspector]
        public string clickTriggerButton;



        /// <summary>
        /// Object which will contain the ability information 
        /// </summary>
        [Tooltip("Object which will contain the ability information ")]
        [HideInInspector]
        public ABC_GameObjectReference toolTip = new ABC_GameObjectReference();

        /// <summary>
        /// If true then the Tooltip will only show when the icon is hovered over
        /// </summary>
        [Tooltip("If true then the Tooltip will only show when the icon is hovered over")]
        [HideInInspector]
        public bool toolTipShowOnHover = true;

        /// <summary>
        /// Text which shows ability name in the tooltip
        /// </summary>
        [Tooltip("Text which shows ability name in the tooltip")]
        [HideInInspector]
        public ABC_TextReference toolTipNameText = new ABC_TextReference();

        /// <summary>
        /// Text which shows ability description in the tooltip
        /// </summary>
        [Tooltip("Text which shows ability description in the tooltip")]
        [HideInInspector]
        public ABC_TextReference toolTipDescriptionText = new ABC_TextReference();

        /// <summary>
        /// Text which shows ability mana cost in the tooltip
        /// </summary>
        [Tooltip("Text which shows ability mana cost in the tooltip")]
        [HideInInspector]
        public ABC_TextReference toolTipManaText = new ABC_TextReference();

        /// <summary>
        /// Text which shows ability recast time in the tooltip
        /// </summary>
        [Tooltip("Text which shows ability recast time in the tooltip")]
        [HideInInspector]
        public ABC_TextReference toolTipRecastText = new ABC_TextReference();





        #endregion


        // ********************* Variables ********************

        #region Variables

        /// <summary>
        /// Entity linked to this IconUI
        /// </summary>
        private ABC_IEntity Originator = null;

        /// <summary>
        /// The ID of the current ability or weapon this Icon relates too
        /// </summary>
        private int currentUILinkedID = -1;

        /// <summary>
        /// RawImage attached to icon
        /// </summary>
        private RawImage iconTexture;

        /// <summary>
        /// Button attached to icon
        /// </summary>
        private Button iconButton;

        /// <summary>
        /// Bool to determine the status of the icon
        /// </summary>
        private bool isEnabled = true;



        #endregion


        // ********************* Private Methods ********************

        #region Private Methods

        /// <summary>
        /// Will add the texture provided to the icon
        /// </summary>
        /// <param name="Texture">Texture to add</param>
        private void AddTextureToIcon(Texture2D Texture) {


            //Get image component or add a new one
            this.iconTexture = iconObject.GameObject.GetComponent<RawImage>();
            if (this.iconTexture == null) {
                this.iconTexture = iconObject.GameObject.AddComponent<RawImage>();
            }

            // set it to the image GUI
            if (Texture != null && this.iconTexture != null)
                this.iconTexture.texture = Texture;

            //Make sure it's not transparent
            Color c = iconTexture.color;
            c = new Color(1f, 1f, 1f, 1f);
            iconTexture.color = c;

        }

        /// <summary>
        /// Will disable the icon so it's unable to be clicked or used. The texture will have a greyscale color applied to represent this
        /// </summary>
        private void DisableIcon() {

            this.isEnabled = false;

            if (iconTexture != null) {
                //greyscale the texture
                Color c = iconTexture.color;
                c = new Color(0.454f, 0.454f, 0.454f);
                iconTexture.color = c;
            }


        }

        /// <summary>
        /// Will enable the icon so it can be used and clicked on
        /// </summary>
        private void EnableIcon() {

            this.isEnabled = true;

            if (iconTexture != null) {
                //greyscale the texture
                Color c = iconTexture.color;
                c = new Color(255, 255, 255);
                iconTexture.color = c;
            }


        }



        #endregion


        // ********************* Public Methods ********************

        #region Public Methods

        /// <summary>
        /// Will Initialise and setup the icon including adding textures and subscribing to the onclick event of the button
        /// </summary>
        /// <param name="Originator">Entity linked to the abilities and Icons</param>
        /// <param name="NewLinkID">ID of any new abilities or weapons to initialise with, if not provided then the main ability/weapon ID setup will be used</param>
        public void Initialise(ABC_IEntity Originator, int NewLinkID = -1) {

            //Add originator
            this.Originator = Originator;

            if (this.iconObject.GameObject == null)
                return;


            switch (this.iconType) {
                case IconType.AbilityActivation:
                    //Store current ability ID
                    if (NewLinkID == -1)
                        this.currentUILinkedID = this.iconUIAbilityID;
                    else
                        this.currentUILinkedID = NewLinkID; // if new linkID is given then update                
                    break;
                case IconType.WeaponEquip:
                    this.currentUILinkedID = this.iconUIWeaponID;
                    break;
            }


            //Make sure button component exists
            this.iconButton = iconObject.GameObject.GetComponent<Button>();
            if (this.iconButton == null)
                this.iconButton = iconObject.GameObject.AddComponent<Button>();

            this.iconButton.enabled = true;

            //Depending on the icon type add textures etc
            switch (this.iconType) {
                case IconType.EmptyIcon:
                    this.AddTextureToIcon(null);
                    this.ConvertToEmptyIcon(false);
                    break;
                case IconType.AbilityActivation:
                    this.AddTextureToIcon(Originator.GetAbilityIcon(this.currentUILinkedID));
                    //Update any tooltips
                    this.UpdateToggleTipText();
                    break;
                case IconType.ScrollAbilityActivation:
                    this.AddTextureToIcon(this.ScrollAbilityActivationTexture.Texture2D);
                    break;
                case IconType.WeaponEquip:
                    this.AddTextureToIcon(Originator.GetWeaponIcon(this.currentUILinkedID));
                    //Update any tooltips
                    this.UpdateToggleTipText();
                    break;

            }


            //Add Icon Controller Component and pass us to it
            ABC_IconController iconComponent = iconObject.GameObject.GetComponent<ABC_IconController>();
            if (iconComponent == null)
                iconComponent = iconObject.GameObject.AddComponent<ABC_IconController>();

            iconComponent.IconUI = this;

        }


        /// <summary>
        /// Will show or hide the tooltip for the linked ability
        /// </summary>
        /// <param name="Enabled">True to show tooltip, else false</param>
        public void ToggleToolTip(bool Enabled = true) {

            //If this isn't an ability activation or we arn't hiding and showing tooltip then return here
            if (this.iconType != IconType.AbilityActivation && this.iconType != IconType.WeaponEquip || this.toolTipShowOnHover == false)
                return;


            if (Enabled && this.toolTip.GameObject != null) {
                //Show tooltip
                this.toolTip.GameObject.SetActive(true);

                //Update tooltip text
                this.UpdateToggleTipText();

            } else if (Enabled == false && this.toolTip.GameObject != null) { // Disable tooltip
                this.toolTip.GameObject.SetActive(false);
            }

        }


        /// <summary>
        /// Will update the tooltip texts with information from the linked ability
        /// </summary>
        public void UpdateToggleTipText() {

            switch (this.iconType) {
                case IconType.AbilityActivation:
                    //Get information for the ability and populate the texts
                    Dictionary<string, string> abilityDetails = this.Originator.GetAbilityDetails(this.currentUILinkedID);


                    if (abilityDetails == null)
                        return;

                    if (this.toolTipNameText.text != null)
                        this.toolTipNameText.text = abilityDetails["Name"];

                    if (this.toolTipDescriptionText.text != null)
                        this.toolTipDescriptionText.text = abilityDetails["Description"];

                    if (this.toolTipManaText.text != null)
                        this.toolTipManaText.text = abilityDetails["Mana"];

                    if (this.toolTipRecastText.text != null)
                        this.toolTipRecastText.text = abilityDetails["Recast"];
                    break;
                case IconType.WeaponEquip:
                    //Get information for the weapon and populate the texts
                    Dictionary<string, string> weaponDetails = this.Originator.GetWeaponDetails(this.currentUILinkedID);


                    if (weaponDetails == null)
                        return;

                    if (this.toolTipNameText.text != null)
                        this.toolTipNameText.text = weaponDetails["Name"];

                    if (this.toolTipDescriptionText.text != null)
                        this.toolTipDescriptionText.text = weaponDetails["Description"];

                    break;
            }


        }





        /// <summary>
        /// Will handle if the icon should be enabled or not depending on the ability it is linked too
        /// </summary>
        public void EnableHandler() {

            //If empty icon then return here
            if (this.iconType == IconType.EmptyIcon)
                return;

            if (this.Originator == null)
                return;

            switch (this.iconType) {
                case IconType.AbilityActivation:
                    //If the main ability linked to this icon is enabled then make sure it is initialised
                    if (this.Originator.IsAbilityEnabled(this.iconUIAbilityID) == true) {

                        if (this.currentUILinkedID != this.iconUIAbilityID)
                            this.Initialise(this.Originator);

                    }

                    //If the main ability is not enabled then we will initialise a substitute ability setup 
                    if (this.Originator.IsAbilityEnabled(this.iconUIAbilityID) == false && this.substituteAbilityWhenDisabled == true && this.iconUISubstituteAbilityIDs.Count > 0) {

                        //find first active substitue ability 
                        int substituteID = this.iconUISubstituteAbilityIDs.Where(id => this.Originator.IsAbilityEnabled(id) == true).FirstOrDefault();

                        //If a substitue ID was found then initialise and enable
                        if (substituteID != 0 && this.currentUILinkedID != substituteID) {

                            this.Initialise(this.Originator, (int)substituteID);
                        }

                    }

                    //If disable with ability then check mana/stat and if enabled
                    if (this.disableWithAbility == true) {

                        //If not enabled then disable icon
                        if (this.Originator.IsAbilityEnabled(this.currentUILinkedID) == false) {
                            this.DisableIcon();
                            return; 
                        }

                        //Get ability
                        ABC_Ability iconAbility = this.Originator.FindAbility(this.currentUILinkedID);

                        //Check stat and mana
                        if (iconAbility != null && (Originator.manaValue < iconAbility.manaCost || iconAbility.statCost > 0 && Originator.GetStatValue(iconAbility.statCostName, iconAbility.statCostIntegrationType, GCStatType.Attribute) < iconAbility.statCost)) {                            
                            this.DisableIcon();
                            return;                            
                        }
       
                    }
                    
                     // else all ok and icon can be enabled
                        this.EnableIcon();

                    break;
                case IconType.WeaponEquip:
                    //If the main weapon linked to this icon is enabled then make sure it is initialised and the icon is enabled
                    if (this.Originator.IsWeaponEnabled(this.iconUIWeaponID) == true) {

                        if (this.currentUILinkedID != this.iconUIWeaponID)
                            this.Initialise(this.Originator);

                        if (this.isEnabled == false)
                            this.EnableIcon();

                        //Return to finish function early as we enabled the icon
                        return;

                    }


                    //If we got this far and the current weapon is not enabled then disable icon
                    if (this.isEnabled == true && this.Originator.IsWeaponEnabled(this.currentUILinkedID) == false && this.disableWithWeapon == true)
                        this.DisableIcon();

                    break;

            }


        }

        /// <summary>
        /// Will handle ability countdown texts and effects for the UI 
        /// </summary>
        public void CountDownHandler() {


            //Only affects ability activation which isn't a source type where display countdown setting has been turned on and icon isn't empty type
            if (this.Originator == null || this.iconType != IconType.AbilityActivation || this.displayCountdown == false || this.actionType == ActionType.Source || this.iconType == IconType.EmptyIcon) {

                if (this.countdownText.text != null)
                    this.countdownText.text = "";

                if (this.countdownFillOverlay.Image != null)
                    this.countdownFillOverlay.Image.fillAmount = 0;

                return;
            }

            //Update text
            if (this.countdownText.text != null) {

                int remainingCooldown = Mathf.RoundToInt(Originator.GetAbilityRemainingCooldown(this.currentUILinkedID));

                //If remaining cooldown is 0 then blank the text, else show the value
                if (remainingCooldown <= 0) {
                    this.countdownText.text = "";
                } else {
                    this.countdownText.text = remainingCooldown.ToString();
                }
            }

            //Update fill
            if (this.countdownFillOverlay.Image != null)
                this.countdownFillOverlay.Image.fillAmount = (Originator.GetAbilityRemainingCooldown(this.currentUILinkedID, true) / 100) * 1;


        }

        /// <summary>
        /// Will update all text display values
        /// </summary>
        public void KeyDisplayHandler() {

            //No key text has been defined so end here
            if (this.keyText.text == null)
                return;

            //If this is a source icon or it's empty and we don't assign keys on drop then hide the display
            if (this.actionType == ActionType.Source || this.iconType == IconType.EmptyIcon && this.clickFromTrigger == false) {
                this.keyText.Graphic.enabled = false;
                return;
            }

            //else enable the text display
            this.keyText.Graphic.enabled = true;

            //If override text is given then display that
            if (this.overrideKeyText == true) {
                this.keyText.text = this.keyTextOverride;
            } else if (this.clickFromTrigger == false) {
                //If we are not clicking icons from a seperate trigger then get key from ability    
                if (this.iconType == IconType.AbilityActivation) {
                    this.keyText.text = Originator.GetAbilityKey(this.currentUILinkedID);   //If icon type if ability activation then get the key/button setup to trigger the ability
                } else if (this.iconType == IconType.ScrollAbilityActivation) {
                    this.keyText.text = Originator.GetScrollAbilityActivationKey(); // else if the icon type is scroll ability activation then get the key/button setup to trigger the current scroll ability
                } else if (this.iconType == IconType.WeaponEquip) {
                    this.keyText.text = Originator.GetWeaponEquipKey(this.currentUILinkedID); // else if the icon type is weaponequip then get the key/button setup to equip the weapon
                }
            } else {
                //Else show the key which will be assigned to activate the 'click' on the icon  
                if (this.clickFromTriggerInputType == InputType.Key)
                    this.keyText.text = this.clickTriggerKey.ToString();
                else
                    this.keyText.text = this.clickTriggerButton.ToString();
            }
        }


        /// <summary>
        /// Will watch for button/key triggers which will simulate an icon click (which will activate abilities etc)
        /// </summary>
        public void TriggerWatcher() {

            //If this is not set to trigger from a key or button, or its an empty icon or not dynamic then return here
            if (this.clickFromTrigger == false || this.actionType == ActionType.Source || this.iconType == IconType.EmptyIcon)
                return;

            // check if correct button is being pressed/held 
            if (this.clickFromTriggerOnKeyPress) {

                //If input type is key and the configured key is being pressed then click the icon
                if (this.clickFromTriggerInputType == InputType.Key && this.Originator.inputManager.GetKeyDown(this.clickTriggerKey))
                    this.Click(true);

                //If input type is key and the configured key is being pressed then click the icon
                if (this.clickFromTriggerInputType == InputType.Button && this.Originator.inputManager.GetButtonDown(this.clickTriggerButton))
                    this.Click(true);


            } else if (this.clickFromTriggerKeyDown) {

                // If input type is key and the configured key is being held down then click the icon
                if (this.clickFromTriggerInputType == InputType.Key && this.Originator.inputManager.GetKey(this.clickTriggerKey))
                    this.Click(true);


                // if input type is button and the configured button is being held down then click the icon
                if (this.clickFromTriggerInputType == InputType.Button && this.Originator.inputManager.GetButton(this.clickTriggerButton))
                    this.Click(true);

            }

        }

        /// <summary>
        /// Click event method which will do numerous things depending on the type of Icon UI - Activate ability or scroll ability etc
        /// </summary>
        /// <param name="OverrideClickableChecks">If true then the click event will occur even if it's set to not trigger from mouse clicks</param>
        public void Click(bool OverrideClickableChecks = false) {

            if (this.isClickable == false && OverrideClickableChecks == false || this.isEnabled == false)
                return;

            switch (this.iconType) {
                case IconType.AbilityActivation:
                    Originator.TriggerAbility(this.currentUILinkedID);
                    break;
                case IconType.ScrollAbilityActivation:
                    Originator.TriggerCurrentScrollAbility();
                    break;
                case IconType.WeaponEquip:
                    Originator.EquipWeapon(this.currentUILinkedID, this.weaponQuickEquip);
                    break;
            }


        }


        /// <summary>
        /// Transform this icon into an empty icon blocking it from being dragged or clicked. 
        /// </summary>
        /// <param name="RemoveFromEntity">If true then icon will be removed from entities list</param>
        public void ConvertToEmptyIcon(bool RemoveFromEntity = true) {

            if (RemoveFromEntity)
                this.RemoveFromCurrentEntity();

            //Turn off any buttons
            if (this.iconButton != null)
                this.iconButton.enabled = false;

            //Turn to empty type
            this.iconType = IconType.EmptyIcon;
            this.actionType = ActionType.Dynamic;


            ////Hide the image
            RawImage image = this.iconObject.GameObject.GetComponent<RawImage>();
            Color c = image.color;
            c.a = 0f;
            image.color = c;

        }


        /// <summary>
        /// Will add the icon provided to the originator 
        /// </summary>
        /// <param name="Icon">Icon to add to the Originator</param>
        public void AddIconToOriginator(ABC_IconUI Icon) {

            //Add the icon provided to the originator already linked to us
            Icon.Originator = this.Originator;
            Icon.Originator.AddIconUI(Icon);

        }


        /// <summary>
        /// Will copy the provided icons type settings to this icon
        /// </summary>
        /// <param name="Icon">Icon to copy action type settings too</param>
        public void CloneTypeSettings(ABC_IconUI Icon) {

            this.iconType = Icon.iconType;
            this.iconUIAbilityID = Icon.iconUIAbilityID;
            this.IconUIAbilityListChoice = Icon.IconUIAbilityListChoice;
            this.iconUISubstituteAbilityIDs = Icon.iconUISubstituteAbilityIDs;
            this.IconUISubstituteAbilityListChoice = Icon.IconUISubstituteAbilityListChoice;
            this.substituteAbilityWhenDisabled = Icon.substituteAbilityWhenDisabled;
            this.iconTexture = Icon.iconTexture;

            this.iconUIWeaponID = Icon.iconUIWeaponID;
            this.IconUIWeaponListChoice = Icon.IconUIWeaponListChoice;
            this.weaponQuickEquip = Icon.weaponQuickEquip;


            this.Initialise(this.Originator);
        }



        /// <summary>
        /// Will swap key assignment settings with the icon provided
        /// </summary>
        /// <param name="Icon">Icon to swap key assignment settings with</param>
        public void SwapKeyAssignmentSettings(ABC_IconUI Icon) {

            //Temporary store the key assignment settings
            bool tempclickFromTrigger = this.clickFromTrigger;
            InputType tempClickFromTriggerInputType = this.clickFromTriggerInputType;
            KeyCode tempClickTriggerKey = this.clickTriggerKey;
            string tempClickTriggerButton = this.clickTriggerButton;
            bool tempOverrideKeyText = this.overrideKeyText;
            string tempKeyTextOverride = this.keyTextOverride;

            //Swap settings around
            this.clickFromTrigger = Icon.clickFromTrigger;
            this.clickFromTriggerInputType = Icon.clickFromTriggerInputType;
            this.clickTriggerKey = Icon.clickTriggerKey;
            this.clickTriggerButton = Icon.clickTriggerButton;
            this.overrideKeyText = Icon.overrideKeyText;
            this.keyTextOverride = Icon.keyTextOverride;

            Icon.clickFromTrigger = tempclickFromTrigger;
            Icon.clickFromTriggerInputType = tempClickFromTriggerInputType;
            Icon.clickTriggerKey = tempClickTriggerKey;
            Icon.clickTriggerButton = tempClickTriggerButton;
            Icon.overrideKeyText = tempOverrideKeyText;
            Icon.keyTextOverride = tempKeyTextOverride;


        }





        /// <summary>
        /// Will remove this Icon from the entity it is linked with
        /// </summary>
        public void RemoveFromCurrentEntity() {

            if (this.Originator != null)
                this.Originator.RemoveIconUI(this);

        }


        #endregion
    }


    /// <summary>
    /// Enum used to define the icons type
    /// </summary>
    public enum ActionType {
        Dynamic,
        Source,
        Static
    }


    public enum IconType {
        EmptyIcon,
        AbilityActivation,
        ScrollAbilityActivation,
        WeaponEquip
    }
}