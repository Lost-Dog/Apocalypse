using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace CompassNavigatorPro {


    public partial class CompassPro : MonoBehaviour {

        #region Generic events

        /// <summary>
        /// Event fired when a POI is added to the compass system
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIRegister;

        /// <summary>
        /// Event fired when a POI is removed from the compass system
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIUnregister;

        /// <summary>
        /// Event fired when this POI is visited.
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIVisited;

        /// <summary>
        /// Event fired when player enters the circle of a POI
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIEnterCircle;

        /// <summary>
        /// Event fired when a player exits the circle of a POI
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIExitCircle;

        #endregion


        #region Compass events

        /// <summary>
        /// Event fired when the POI appears in the compass bar (gets near than the visible distance)
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIVisible;

        /// <summary>
        /// Event fired when POI disappears from the compass bar (gets farther than the visible distance)
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIHide;

        /// <summary>
        /// Event fired when a compass bar icon is created for a POI. Use this to customize the icon GameObject after creation.
        /// </summary>
        public UnityEvent<CompassProPOI, GameObject> OnCompassBarIconCreated;

        #endregion


        #region Mini-map events

        /// <summary>
        /// Event fired when mouse enters an icon on the miniMap
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIMiniMapIconMouseEnter;

        /// <summary>
        /// Event fired when mouse exits an icon on the miniMap
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIMiniMapIconMouseExit;

        /// <summary>
        /// Event fired when button is pressed on an icon on the miniMap
        /// </summary>
        public UnityEvent<CompassProPOI, int> OnPOIMiniMapIconMouseDown;

        /// <summary>
        /// Event fired when button is released on an icon on the miniMap
        /// </summary>
        public UnityEvent<CompassProPOI, int> OnPOIMiniMapIconMouseUp;

        /// <summary>
        /// Event fired when an icon is clicked on the minimap
        /// </summary>
        public UnityEvent<CompassProPOI, int> OnPOIMiniMapIconMouseClick;

        /// <summary>
        /// Event fired when a minimap icon is created for a POI. Use this to customize the icon GameObject after creation.
        /// </summary>
        public UnityEvent<CompassProPOI, GameObject> OnMiniMapIconCreated;

        /// <summary>
        /// Event fired when this POI appears in the Mini-Map
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIVisibleInMiniMap;

        /// <summary>
        /// Event fired when the POI disappears from the Mini-Map
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIHidesInMiniMap;

        /// <summary>
        /// Event fired when full screen mode changes
        /// </summary>
        public UnityEvent<bool> OnMiniMapChangeFullScreenState;

        /// <summary>
        /// Event fired when user clicks on minimap - sends the world space position of click
        /// </summary>
        public UnityEvent<Vector3, int> OnMiniMapMouseClick;

        /// <summary>
        /// Event fired when mouse enters minimap area in the screen
        /// </summary>
        public UnityEvent<Vector2> OnMiniMapMouseEnter;

        /// <summary>
        /// Event fired when mouse exits minimap area in the screen
        /// </summary>
        public UnityEvent<Vector2> OnMiniMapMouseExit;

        /// <summary>
        /// Event fired just before the scene is captured for the minimap
        /// </summary>
        public UnityEvent OnMiniMapBeforeCapture;

        /// <summary>
        /// Event fired just after the scene is captured for the minimap
        /// </summary>
        public UnityEvent OnMiniMapAfterCapture;

        #endregion


        #region Indicators events

        /// <summary>
        /// Event fired when an on-screen indicator is created for a POI. Use this to customize the indicator GameObject after creation.
        /// </summary>
        public UnityEvent<CompassProPOI, GameObject> OnIndicatorCreated;

        /// <summary>
        /// Event fired when the indicator of this POI appears on screen
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIOnScreen;

        /// <summary>
        /// Event fired when the indicator of this POI goes off-screen
        /// </summary>
        public UnityEvent<CompassProPOI> OnPOIOffScreen;


        #endregion

        #region Scan events

        /// <summary>
        /// Event fired when the scan hits a collider
        /// </summary>
        public UnityEvent<ScanEffect, CompassProPOI, Transform> OnScanHit;

        /// <summary>
        /// Event fired when the scan ends
        /// </summary>
        public UnityEvent<ScanEffect> OnScanEnd;

        #endregion


    }

}



