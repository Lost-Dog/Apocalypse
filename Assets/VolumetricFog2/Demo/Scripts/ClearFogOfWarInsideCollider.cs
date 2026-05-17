using UnityEngine;

namespace VolumetricFogAndMist2.Demos {

    public class ClearFogOfWarInsideCollider : MonoBehaviour {

        public VolumetricFog fogVolume;
        public BoxCollider thisCollider;

        void Start() {
            fogVolume.SetFogOfWarAlpha(thisCollider, 0);
        }
    }

}