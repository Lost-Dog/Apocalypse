using UnityEngine;

namespace VolumetricFogAndMist2.Demos {

    public class ClearFogOfWarUnderGameObject : MonoBehaviour {

        public VolumetricFog fogVolume;

        void Start() {
            fogVolume.SetFogOfWarAlpha(gameObject, 0);
        }
    }

}