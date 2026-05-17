using UnityEngine;
using CompassNavigatorPro;

namespace CompassNavigatorProDemos {

    public class MiniMapScrollWheel : MonoBehaviour {

        public float zoomSpeed = 100f;

        CompassPro compass;
        bool canScrollWheel;

        void Start() {
            // Get a reference to the Compass Pro Navigator component
            compass = GetComponent<CompassPro>();
            if (compass == null) {
                compass = CompassPro.instance;
            }

            // Subscribe to Compass minimap events
            compass.OnMiniMapMouseEnter.AddListener((Vector2 p) => canScrollWheel = true);
            compass.OnMiniMapMouseExit.AddListener((Vector2 p) => canScrollWheel = false);
        }

        void Update() {

            if (canScrollWheel) {
                float scroll = InputProxy.GetAxis("Mouse ScrollWheel");
                if (scroll != 0) {
                    compass.MiniMapZoomIn(zoomSpeed * scroll);
                }
            }

        }


    }
}