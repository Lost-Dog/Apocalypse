using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ABCToolkit {
    /// <summary>
    /// Component to control the UI Icons of ABC. Handles events like dragging, dropping, button clicks and swaps etc
    /// </summary>
    public class ABC_IconController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {


        // ********************* Settings ********************
        #region Settings

        /// <summary>
        /// The IconUI Object which holds all the properties driving the controller. This object is setup in ABC.
        /// </summary>
        public ABC_IconUI IconUI = new ABC_IconUI();

        #endregion



        // ********************* Static Proprties ********************
        #region Static Settings
        /// <summary>
        /// Static property which tells the whole system which icon is currently selected
        /// </summary>
        static public ABC_IconController selectedIcon; // dragged item

        /// <summary>
        /// Static property which holds the current drag icon. A drag icon is a cloned icon which follows the mouse
        /// </summary>
        static public GameObject dragIcon; // icon

        #endregion

        // ********************* Variables ********************
        #region Variables

        /// <summary>
        /// Button attached to the icon
        /// </summary>
        private Button iconButton;

        /// <summary>
        /// Input Manager for Icon
        /// </summary>
        private ABC_InputManager inputManager;

        #endregion


        // ********************* Private Methods ********************
        #region Private Methods


        /// <summary>
        /// Method which listens to the button onclick event. Once called it will then call it's own delegate event for other components to know when the icon was clicked
        /// </summary>
        private void OnClickEvent() {
            IconUI.Click();
        }


        /// <summary>
        /// Creates a gameobject which is a clone of the icon this component is attached too. 
        /// </summary>
        /// <returns>A new gameobject which is a clone of the icon this component is attached too</returns>
        private GameObject CreateDragIcon() {

            //Create new gameobject
            GameObject retval = new GameObject("Icon");

            //Add the raw image and update it's texture to match                             
            RawImage image = retval.AddComponent<RawImage>();
            image.texture = GetComponent<RawImage>().texture;
            image.raycastTarget = false;

            //Mimic the size                
            RectTransform iconRect = retval.GetComponent<RectTransform>();
            iconRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;

            //Make sure Icon is always on top of GUI
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) {
                retval.transform.SetParent(canvas.transform, true);
                retval.transform.SetAsLastSibling();
            }

            return retval;
        }


        /// <summary>
        /// Method to destroy the drag icon
        /// </summary>
        private void DestroyDragIcon() {
            if (dragIcon != null)
                Destroy(dragIcon);
        }



        /// <summary>
        /// Will swap this icon with the icon which is currently selected by the system
        /// </summary>
        private void SwapWithSelectedIcon() {

            //If we are not set to swap or there is no current selected icon then end here
            if (selectedIcon == null || this.IconUI.actionType != ActionType.Dynamic)
                return;


            //Store the type settings from this icon into a placeholder
            ABC_IconUI iconUIPlaceHolder = new ABC_IconUI();
            iconUIPlaceHolder.CloneTypeSettings(this.IconUI);

            //Copy icon type settings from the icon we are swapping with 
            this.IconUI.CloneTypeSettings(selectedIcon.IconUI);

            //Now copy over to the icon we are swapping with the icon type settings which this icon use to have (stored in the placeholder just created)
            selectedIcon.IconUI.CloneTypeSettings(iconUIPlaceHolder);


        }

        /// <summary>
        /// Will replace this icon with a clone of the icon which is currently selected by the system
        /// </summary>
        private void SourceFromSelectedIcon() {

            //If there is no current selected icon or the current selected icon isn't a source type or this icon is already a source type (Don't want to overwrite a source with a source) then end here
            if (selectedIcon == null || selectedIcon.IconUI.actionType != ActionType.Source || this.IconUI.actionType == ActionType.Source)
                return;


            //Copy the icon type settings from the sourced icon to the icon that got dropped on (us)
            this.IconUI.CloneTypeSettings(selectedIcon.IconUI);


        }


        #endregion

        // ********************* Public Methods ********************
        #region Public Methods

        public void OnPointerEnter(PointerEventData eventData) {

            this.IconUI.ToggleToolTip();


        }

        public void OnPointerExit(PointerEventData eventData) {

            this.IconUI.ToggleToolTip(false);
        }

        /// <summary>
        /// Called by a BaseInputModule before a drag is started.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData) {

            if (this.IconUI.actionType == ActionType.Static || this.IconUI.iconType == IconType.EmptyIcon)
                return;


            //Fill the static property so whole system knows we are currently the selected Icon
            selectedIcon = this;

            //Create temporary drag icon which will follow the mouse cursor                                   
            dragIcon = this.CreateDragIcon();


        }

        /// <summary>
        /// Called by the EventSystem every time the pointer is moved during dragging.
        /// </summary>
        public void OnDrag(PointerEventData eventData) {

            //Make the drag icon  follow mouse position
            if (dragIcon != null)
                dragIcon.transform.position = this.inputManager.GetMousePosition();
        }

        /// <summary>
        /// Called by a BaseInputModule when a drag is ended.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData) {

            //Destroy the drag icon
            DestroyDragIcon();

            if (this.IconUI.actionType == ActionType.Static || this.IconUI.iconType == IconType.EmptyIcon)
                return;

            //If we dragged onto nothing outside the action bar then convert this icon into a blank
            if (this.IconUI.actionType == ActionType.Dynamic && this.IconUI.removeOnEmptyDrag && eventData.pointerEnter == null)
                this.IconUI.ConvertToEmptyIcon(false);

            //Reset static variables so other Icons can use them
            selectedIcon = null;
            dragIcon = null;

        }

        /// <summary>
        /// Called by a BaseInputModule when a drop occurs.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData) {

            //If this UI is static type then it can't be swapped or sourced to 
            if (this.IconUI.actionType == ActionType.Static)
                return;

            ABC_IconController selectedIcon = ABC_IconController.selectedIcon;

            if (selectedIcon == null)
                return;

            switch (selectedIcon.IconUI.actionType) {
                case ActionType.Dynamic:

                    this.SwapWithSelectedIcon();

                    break;
                case ActionType.Source:

                    this.SourceFromSelectedIcon();

                    break;
            }

        }




        #endregion


        // ********************** Game ******************

        #region Game

        void Update() {

            if (this.IconUI == null)
                return;

            //Enable handler to make sure Icon is correctly enabled or not
            this.IconUI.EnableHandler();
            //Count down handler which will update the UI with the remaining time
            this.IconUI.CountDownHandler();
            /// Will update all key text display values
            this.IconUI.KeyDisplayHandler();
            //WIll watch for icon key and button triggers
            this.IconUI.TriggerWatcher();
        }

        void OnEnable() {

            this.inputManager = new ABC_InputManager();

            //If an icon object hasn't been set then this component has been added outside of ABC so use the gameobject the component is attached to as the icon object
            if (this.IconUI.iconObject.GameObject == null)
                this.IconUI.iconObject.GameObject = this.gameObject;

            //subscribe click event to button 
            this.iconButton = this.gameObject.GetComponent<Button>();
            if (this.iconButton != null)
                iconButton.onClick.AddListener(OnClickEvent);

        }

        void OnDisable() {
            //When disabled remove the button event subscription
            if (this.iconButton != null)
                this.iconButton.onClick.RemoveListener(OnClickEvent);

        }


        void OnDestroy() {
            //If item is destroyed then remove it from the entitys list
            IconUI.RemoveFromCurrentEntity();
        }





        #endregion

    }
}