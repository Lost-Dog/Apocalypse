using UnityEngine;

namespace VolumetricFogAndMist2.Demos {

    public class ClearFogOfWarInsideBounds : MonoBehaviour {

        public VolumetricFog fogVolume;
        public Bounds bounds;

        void Start() {
            fogVolume.SetFogOfWarAlpha(bounds, 0);
        }
    }

}