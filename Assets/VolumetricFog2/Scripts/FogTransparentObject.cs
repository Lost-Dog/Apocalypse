#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using UnityEngine;

namespace VolumetricFogAndMist2 {

    [ExecuteAlways]
    public class FogTransparentObject : MonoBehaviour {

        public VolumetricFog fogVolume;

        Renderer thisRenderer;
        Material[] registeredMats;

        void OnEnable () {
            CheckSettings();
#if UNITY_EDITOR
            // workaround for volumetric effect disappearing when saving the scene
            if (!Application.isPlaying) {
                EditorSceneManager.sceneSaving += OnSceneSaving;
                EditorApplication.update += OnEditorUpdate;
            }
#endif            
        }

        void OnDisable () {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorSceneManager.sceneSaving -= OnSceneSaving;
                EditorApplication.update -= OnEditorUpdate;
            }
#endif
            if (fogVolume != null && registeredMats != null) {
                for (int i = 0; i < registeredMats.Length; i++) {
                    fogVolume.UnregisterFogMat(registeredMats[i]);
                }
            }
            registeredMats = null;
        }

        void OnSceneSaving (UnityEngine.SceneManagement.Scene scene, string path) {
            CheckSettings();
        }


#if UNITY_EDITOR
        void OnEditorUpdate () {
            if (registeredMats == null || registeredMats.Length == 0) return;
            if (!registeredMats[0].HasProperty(ShaderParams.Density)) {
                CheckSettings();
            }
        }
#endif

        void OnValidate () {
            CheckSettings();
        }

        void CheckSettings () {
            if (thisRenderer == null) {
                thisRenderer = GetComponent<Renderer>();
                if (thisRenderer == null) return;
            }

            Material[] materials = thisRenderer.sharedMaterials;
            if (materials.Length == 0) return;

            if (fogVolume == null) {
                if (VolumetricFog.volumetricFogs.Count > 0) {
                    fogVolume = VolumetricFog.volumetricFogs[0];
                }
                if (fogVolume == null) return;
            }

            if (registeredMats != null) {
                for (int i = 0; i < registeredMats.Length; i++) {
                    fogVolume.UnregisterFogMat(registeredMats[i]);
                }
            }

            for (int i = 0; i < materials.Length; i++) {
                fogVolume.RegisterFogMat(materials[i]);
            }
            registeredMats = materials;
            fogVolume.UpdateMaterialProperties();
        }
    }
}
