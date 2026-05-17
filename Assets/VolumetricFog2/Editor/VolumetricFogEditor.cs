//#define FOG_NATIVE_LIGHT_FALLOFF

using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using System.Reflection;
using UnityEditor.IMGUI.Controls;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(VolumetricFog))]
    public partial class VolumetricFogEditor : Editor {

        VolumetricFogProfile cachedProfile;
        Editor cachedProfileEditor;
        SerializedProperty profile;
        SerializedProperty maskEditorEnabled, maskBrushMode, maskBrushColor, maskBrushWidth, maskBrushFuzziness, maskBrushOpacity;
        SerializedProperty enablePointLights, enableNativeLights, nativeLightsMultiplier, nativeLightFallOff, enableAPV, apvIntensityMultiplier;
        SerializedProperty enableVoids;
        SerializedProperty enableFogOfWar, fogOfWarCenter, fogOfWarIsLocal, fogOfWarSize, fogOfWarShowCoverage, fogOfWarTextureWidth, fogOfWarTextureHeight, fogOfWarRestoreDelay, fogOfWarRestoreDuration, fogOfWarSmoothness, fogOfWarBlur;
        SerializedProperty enableFollow, followTarget, followMode, followOffset, followIncludeDistantFog;
        SerializedProperty enableFade, fadeIncludeDistantFog, fadeDistance, fadeOut, fadeController, enableSubVolumes, subVolumes;
        SerializedProperty enableUpdateModeOptions, updateMode, updateModeCamera, updateModeBounds;
        SerializedProperty showBoundary;

        static GUIStyle boxStyle;
        VolumetricFog fog;
        public static VolumetricFog lastEditingFog;

        static readonly System.Collections.Generic.List<VolumetricFogEditor> activeSceneGuiEditors = new System.Collections.Generic.List<VolumetricFogEditor>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() {
            boxStyle = null;
            lastEditingFog = null;
            activeSceneGuiEditors.Clear();
            SceneView.duringSceneGui -= OnSceneStatic;
        }

        static void OnSceneStatic (SceneView sceneView) {
            for (int i = activeSceneGuiEditors.Count - 1; i >= 0; i--) {
                var editor = activeSceneGuiEditors[i];
                if (editor != null) editor.OnScene(sceneView);
            }
        }

        void OnEnable () {

            if (activeSceneGuiEditors.Count == 0) {
                SceneView.duringSceneGui += OnSceneStatic;
            }
            if (!activeSceneGuiEditors.Contains(this)) {
                activeSceneGuiEditors.Add(this);
            }

            try {
                profile = serializedObject.FindProperty("profile");

                enablePointLights = serializedObject.FindProperty("enablePointLights");
                enableNativeLights = serializedObject.FindProperty("enableNativeLights");
                nativeLightsMultiplier = serializedObject.FindProperty("nativeLightsMultiplier");
                nativeLightFallOff = serializedObject.FindProperty("nativeLightFallOff");
                enableAPV = serializedObject.FindProperty("enableAPV");
                apvIntensityMultiplier = serializedObject.FindProperty("apvIntensityMultiplier");
                enableVoids = serializedObject.FindProperty("enableVoids");
                enableFogOfWar = serializedObject.FindProperty("enableFogOfWar");
                fogOfWarCenter = serializedObject.FindProperty("fogOfWarCenter");
                fogOfWarIsLocal = serializedObject.FindProperty("fogOfWarIsLocal");
                fogOfWarSize = serializedObject.FindProperty("fogOfWarSize");
                fogOfWarShowCoverage = serializedObject.FindProperty("fogOfWarShowCoverage");
                fogOfWarTextureWidth = serializedObject.FindProperty("fogOfWarTextureWidth");
                fogOfWarTextureHeight = serializedObject.FindProperty("fogOfWarTextureHeight");
                fogOfWarRestoreDelay = serializedObject.FindProperty("fogOfWarRestoreDelay");
                fogOfWarRestoreDuration = serializedObject.FindProperty("fogOfWarRestoreDuration");
                fogOfWarSmoothness = serializedObject.FindProperty("fogOfWarSmoothness");
                fogOfWarBlur = serializedObject.FindProperty("fogOfWarBlur");

                maskEditorEnabled = serializedObject.FindProperty("maskEditorEnabled");
                maskBrushColor = serializedObject.FindProperty("maskBrushColor");
                maskBrushMode = serializedObject.FindProperty("maskBrushMode");
                maskBrushWidth = serializedObject.FindProperty("maskBrushWidth");
                maskBrushFuzziness = serializedObject.FindProperty("maskBrushFuzziness");
                maskBrushOpacity = serializedObject.FindProperty("maskBrushOpacity");

                enableFollow = serializedObject.FindProperty("enableFollow");
                followTarget = serializedObject.FindProperty("followTarget");
                followMode = serializedObject.FindProperty("followMode");
                followOffset = serializedObject.FindProperty("followOffset");
                followIncludeDistantFog = serializedObject.FindProperty("followIncludeDistantFog");

                enableFade = serializedObject.FindProperty("enableFade");
                fadeIncludeDistantFog = serializedObject.FindProperty("fadeIncludeDistantFog");
                fadeDistance = serializedObject.FindProperty("fadeDistance");
                fadeOut = serializedObject.FindProperty("fadeOut");
                fadeController = serializedObject.FindProperty("fadeController");
                enableSubVolumes = serializedObject.FindProperty("enableSubVolumes");
                subVolumes = serializedObject.FindProperty("subVolumes");
                enableUpdateModeOptions = serializedObject.FindProperty("enableUpdateModeOptions");
                updateMode = serializedObject.FindProperty("updateMode");
                updateModeCamera = serializedObject.FindProperty("updateModeCamera");
                updateModeBounds = serializedObject.FindProperty("updateModeBounds");
                showBoundary = serializedObject.FindProperty("showBoundary");

                fog = (VolumetricFog)target;
                lastEditingFog = fog;
            }
            catch { }
        }

        void OnDisable () {
            activeSceneGuiEditors.Remove(this);
            if (activeSceneGuiEditors.Count == 0) {
                SceneView.duringSceneGui -= OnSceneStatic;
            }
        }


        public override void OnInspectorGUI () {

            var pipe = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("Universal Rendering Pipeline asset is not set in Project Settings / Graphics !", MessageType.Error);
                return;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            }

            // Check depth texture mode
            FieldInfo renderers = pipe.GetType().GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (renderers == null) return;
            foreach (var renderer in (object[])renderers.GetValue(pipe)) {
                if (renderer == null) continue;
                FieldInfo depthTextureModeField = renderer.GetType().GetField("m_CopyDepthMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (depthTextureModeField != null) {
                    int depthTextureMode = (int)depthTextureModeField.GetValue(renderer);
                    if (depthTextureMode == 1) { // transparent copy depth mode
                        EditorGUILayout.HelpBox("Depth Texture Mode in URP asset must be set to 'After Opaques' or 'Force Prepass'.", MessageType.Warning);
                        if (GUILayout.Button("Show Pipeline Asset")) {
                            Selection.activeObject = (Object)renderer;
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.Separator();
                    }
                }
            }

            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(5, 5, 5, 5);
            }

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(profile);

            if (profile.objectReferenceValue != null) {
                if (cachedProfile != profile.objectReferenceValue) {
                    cachedProfile = null;
                }
                if (cachedProfile == null) {
                    cachedProfile = (VolumetricFogProfile)profile.objectReferenceValue;
                    cachedProfileEditor = CreateEditor(profile.objectReferenceValue);
                }

                // Drawing the profile editor
                EditorGUILayout.BeginVertical(boxStyle);
                cachedProfileEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.HelpBox("Create or assign a fog profile.", MessageType.Info);
                if (GUILayout.Button("New Fog Profile")) {
                    CreateFogProfile();
                }
            }
            if (EditorGUI.EndChangeCheck()) {
                if (!Application.isPlaying) {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
            }

            var leftGreyMini = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                alignment = TextAnchor.MiddleLeft
            };
            EditorGUILayout.LabelField(new GUIContent("FOG VOLUME FEATURES", "Features available in this fog volume"), leftGreyMini);

            EditorGUIUtility.labelWidth = 170;
            EditorGUILayout.PropertyField(enableNativeLights);
            if (enableNativeLights.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nativeLightsMultiplier, new GUIContent("Intensity Multiplier"));
                #if FOG_NATIVE_LIGHT_FALLOFF
                    EditorGUILayout.PropertyField(nativeLightFallOff, new GUIContent("Distance Falloff"));
                #else
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Distance Falloff", "(Disabled in Volumetric Fog Manager)");
                    GUI.enabled = true;
                #endif
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(enableAPV, new GUIContent("Enable APV (Probe Volumes)"));
            if (enableAPV.boolValue) {
                EditorGUI.indentLevel++;
#if !UNITY_2023_1_OR_NEWER
                EditorGUILayout.HelpBox("Adaptative Probe Volumes are only available in Unity 2023.", MessageType.Warning);
#endif
                EditorGUILayout.PropertyField(apvIntensityMultiplier, new GUIContent("Intensity Multiplier"));
                EditorGUI.indentLevel--;
            }

            GUI.enabled = !enableNativeLights.boolValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enablePointLights);
            if (GUILayout.Button("Point Light Manager", GUILayout.Width(180))) {
                Selection.activeGameObject = VolumetricFogManager.pointLightManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enableVoids);
            if (GUILayout.Button("Void Manager", GUILayout.Width(180))) {
                Selection.activeGameObject = VolumetricFogManager.fogVoidManager.gameObject;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(enableFollow);
            if (enableFollow.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(followTarget, new GUIContent("Target"));
                EditorGUILayout.PropertyField(followMode, new GUIContent("Mode"));
                if ((VolumetricFogFollowMode)followMode.intValue == VolumetricFogFollowMode.FullXYZ) {
                    EditorGUILayout.PropertyField(followIncludeDistantFog, new GUIContent("Include Distant Fog", "Also adjusts distant fog base altitude to the followed object altitude."));
                }
                EditorGUILayout.PropertyField(followOffset, new GUIContent("Offset"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableFade);
            if (enableFade.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fadeDistance);
                EditorGUILayout.PropertyField(fadeOut);
                EditorGUILayout.PropertyField(fadeController);
                EditorGUILayout.PropertyField(fadeIncludeDistantFog, new GUIContent("Include Distant Fog", "Also fades distant fog along with this volume."));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(enableSubVolumes);
            if (enableSubVolumes.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("If no sub-volumes are specified below, any sub-volume in the scene will be used.", MessageType.Info);
                EditorGUILayout.PropertyField(fadeController, new GUIContent("Character Controller"));
                EditorGUILayout.PropertyField(subVolumes);
                EditorGUI.indentLevel--;
            }

            bool requiresFogOfWarTextureReload = false;
            EditorGUILayout.PropertyField(enableFogOfWar);
            if (enableFogOfWar.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fogOfWarCenter, new GUIContent("World Center"));
                EditorGUILayout.PropertyField(fogOfWarIsLocal, new GUIContent("Is Local", "Enable if fog of war center is local to the fog volume"));
                EditorGUILayout.PropertyField(fogOfWarSize, new GUIContent("World Coverage"));
                EditorGUILayout.PropertyField(fogOfWarShowCoverage, new GUIContent("Show Coverage Bounds"));
                EditorGUILayout.PropertyField(fogOfWarTextureWidth, new GUIContent("Texture Width"));
                EditorGUILayout.PropertyField(fogOfWarTextureHeight, new GUIContent("Texture Height"));
                EditorGUILayout.PropertyField(fogOfWarRestoreDelay, new GUIContent("Restore Delay"));
                EditorGUILayout.PropertyField(fogOfWarRestoreDuration, new GUIContent("Restore Duration"));
                EditorGUILayout.PropertyField(fogOfWarSmoothness, new GUIContent("Border Smoothness"));
                EditorGUILayout.PropertyField(fogOfWarBlur, new GUIContent("Blur"));

                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(maskEditorEnabled, new GUIContent("Fog Of War Editor", "Activates terrain brush to paint/remove fog of war at custom locations."));

                if (maskEditorEnabled.boolValue) {
                    if (GUILayout.Button("Create New Mask Texture")) {
                        if (EditorUtility.DisplayDialog("Create Mask Texture", "A texture asset will be created with the size specified in current profile (" + fog.fogOfWarTextureWidth + "x" + fog.fogOfWarTextureHeight + ").\n\nContinue?", "Ok", "Cancel")) {
                            CreateNewMaskTexture();
                            GUIUtility.ExitGUI();
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    fog.fogOfWarTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Coverage Texture", "Fog of war coverage mask. A value of alpha of zero means no fog."), fog.fogOfWarTexture, typeof(Texture2D), false);
                    Texture2D tex = fog.fogOfWarTexture;
                    if (EditorGUI.EndChangeCheck()) {
                        requiresFogOfWarTextureReload = true;
                        if (tex != null) {
                            string assetPath = AssetDatabase.GetAssetPath(tex);
                            if (!string.IsNullOrEmpty(assetPath)) {
                                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                                if (importer != null) {
                                    bool settingsChanged = false;
                                    if (!importer.isReadable) {
                                        importer.isReadable = true;
                                        settingsChanged = true;
                                    }

                                    if (importer.textureCompression != TextureImporterCompression.Uncompressed) {
                                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                                        settingsChanged = true;
                                    }
                                    if (settingsChanged) {
                                        importer.SaveAndReimport();
                                        GUIUtility.ExitGUI();
                                    }
                                }
                            }
                        }
                    }

                    if (tex != null) {
                        EditorGUILayout.LabelField("   Texture Width", tex.width.ToString());
                        EditorGUILayout.LabelField("   Texture Height", tex.height.ToString());
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (string.IsNullOrEmpty(path)) {
                            path = "(Temporary texture)";
                        }
                        EditorGUILayout.LabelField("   Texture Path", path);
                    }

                    if (tex != null) {
                        EditorGUILayout.Separator();
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(maskBrushMode, new GUIContent("Brush Mode", "Select brush operation mode."));
                        if (GUILayout.Button("Toggle", GUILayout.Width(70))) {
                            maskBrushMode.intValue = maskBrushMode.intValue == 0 ? 1 : 0;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (maskBrushMode.intValue == (int)MaskTextureBrushMode.ColorFog) {
                            EditorGUILayout.PropertyField(maskBrushColor, new GUIContent("   Color", "Brush color."));
                        }
                        EditorGUILayout.PropertyField(maskBrushWidth, new GUIContent("   Width", "Width of the snow editor brush."));
                        EditorGUILayout.PropertyField(maskBrushFuzziness, new GUIContent("   Fuzziness", "Solid vs spray brush."));
                        EditorGUILayout.PropertyField(maskBrushOpacity, new GUIContent("   Opacity", "Stroke opacity."));
                        EditorGUILayout.BeginHorizontal();
                        if (tex == null) GUI.enabled = false;
                        if (GUILayout.Button("Fill Mask")) {
                            fog.ResetFogOfWar(1f);
                            maskBrushMode.intValue = (int)MaskTextureBrushMode.RemoveFog;
                        }
                        if (GUILayout.Button("Clear Mask")) {
                            fog.ResetFogOfWar(0);
                            maskBrushMode.intValue = (int)MaskTextureBrushMode.AddFog;
                        }

                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableUpdateModeOptions);
            if (enableUpdateModeOptions.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(updateMode);
                EditorGUILayout.PropertyField(updateModeCamera, new GUIContent("Camera"));
                if (updateMode.intValue == (int)VolumetricFogUpdateMode.WhenCameraIsInsideArea) {
                    EditorGUILayout.PropertyField(updateModeBounds, new GUIContent("Bounds"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(showBoundary);

            EditorGUILayout.Separator();
            if (GUILayout.Button("Show Volumetric Fog Manager Settings")) {
                Selection.activeObject = VolumetricFogManager.instance;
            }

            serializedObject.ApplyModifiedProperties();

            if (requiresFogOfWarTextureReload) {
                fog.ReloadFogOfWarTexture();
            }

        }


        void CreateFogProfile () {

            // Find directional light and adjusts brightness to avoid excessive bright fog
            float brightness = 1f;
            Light[] lights = Misc.FindObjectsOfType<Light>();
            
            if (lights != null) {
                foreach (Light light in lights) {
                    if (light.type == LightType.Directional) {
                        brightness /= light.intensity;
                        break;
                    }
                }
            }

            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path)) {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            VolumetricFogProfile fp = CreateInstance<VolumetricFogProfile>();
            fp.name = "New Volumetric Fog Profile";
            fp.brightness = brightness;

            string desiredAssetPath = path + "/" + fp.name + ".asset";
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(desiredAssetPath);
            fp.name = Path.GetFileNameWithoutExtension(uniqueAssetPath);

            AssetDatabase.CreateAsset(fp, uniqueAssetPath);
            AssetDatabase.SaveAssets();
            profile.objectReferenceValue = fp;
            EditorGUIUtility.PingObject(fp);
        }


        void OnScene (SceneView sceneView) {
            OnSceneGUI_FogOfWar();
            OnSceneGUI_TransformHandle();
        }

        private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        void OnSceneGUI_TransformHandle () {
            if (fog == null) return;
            Bounds bounds = fog.GetBounds();
            m_BoundsHandle.center = bounds.center;
            m_BoundsHandle.size = bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(fog, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                fog.SetBounds(newBounds);
            }
        }
    }
}
