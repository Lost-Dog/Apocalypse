//#define FOG_BORDER
//#define FOG_SHADOW_CANCELLATION

using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    [CustomEditor(typeof(VolumetricFogProfile))]
    public class VolumetricFogProfileEditor : Editor {

        static GUIStyle foldoutStyle;
        static GUIStyle headerRightLabelStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() {
            foldoutStyle = null;
            headerRightLabelStyle = null;
        }

        SerializedProperty raymarchQuality, raymarchMinStep, raymarchNearStepping, jittering, dithering;
        SerializedProperty renderQueue, sortingLayerID, sortingOrder;
        SerializedProperty constantDensity, noiseTexture, noiseStrength, noiseScale, noiseFinalMultiplier, noiseTextureOptimizedSize;
        SerializedProperty useDetailNoise, detailTexture, detailScale, detailStrength, detailOffset;
        SerializedProperty density;
        SerializedProperty shape, customMesh, scaleNoiseWithHeight, border, customHeight, height, verticalOffset, distance, distanceFallOff, maxDistance, maxDistanceFallOff;
        SerializedProperty terrainFit, terrainFitResolution, terrainLayerMask, terrainFogHeight, terrainFogMinAltitude, terrainFogMaxAltitude;

        SerializedProperty albedo, enableDepthGradient, depthGradient, depthGradientMaxDistance, enableHeightGradient, heightGradient;
        SerializedProperty brightness, deepObscurance, specularColor, specularThreshold, specularIntensity, ambientLightMultiplier;

        SerializedProperty turbulence, windDirection, useCustomDetailNoiseWindDirection, detailNoiseWindDirection;

        SerializedProperty dayNightCycle, sunDirection, sunColor, sunIntensity, lightDiffusionModel, lightDiffusionPower, lightDiffusionIntensity, lightDiffusionNearDepthAtten;
        SerializedProperty receiveShadows, shadowIntensity, shadowCancellation, shadowMaxDistance;
        SerializedProperty cookie;

        SerializedProperty distantFog, distantFogShowInEditMode, distantFogColor, distantFogStartDistance, distantFogDistanceDensity, distantFogMaxHeight, distantFogBaseAltitude, distantFogHeightDensity, distantFogDiffusionIntensity, distantFogRenderQueue, distantFogSymmetrical, distantFogTransparencySupport;
        SerializedProperty distantFogNoise, distantFogNoiseTexture, distantFogNoiseWindDirection;
        SerializedProperty distantFogDistanceNoiseScale, distantFogDistanceNoiseStrength, distantFogDistanceNoiseMaxDistance;

        const string PrefsKeyPrefix = "VolumetricFog2.ProfileEditor.";
        const string PrefsColors = PrefsKeyPrefix + "Colors";
        const string PrefsDensity = PrefsKeyPrefix + "Density";
        const string PrefsShape = PrefsKeyPrefix + "Shape";
        const string PrefsDistance = PrefsKeyPrefix + "Distance";
        const string PrefsTerrain = PrefsKeyPrefix + "Terrain";
        const string PrefsAnimation = PrefsKeyPrefix + "Animation";
        const string PrefsLighting = PrefsKeyPrefix + "Lighting & Shadows";
        const string PrefsDistantFog = PrefsKeyPrefix + "DistantFog";
        const string PrefsRendering = PrefsKeyPrefix + "Rendering Settings";

        bool showColors;
        bool showDensity;
        bool showShape;
        bool showMinDistance;
        bool showTerrain;
        bool showAnimation;
        bool showLighting;
        bool showDistantFog;
        bool showRendering;

        private void OnEnable () {
            try {
                raymarchQuality = serializedObject.FindProperty("raymarchQuality");
                raymarchMinStep = serializedObject.FindProperty("raymarchMinStep");
                raymarchNearStepping = serializedObject.FindProperty("raymarchNearStepping");
                jittering = serializedObject.FindProperty("jittering");
                dithering = serializedObject.FindProperty("dithering");

                renderQueue = serializedObject.FindProperty("renderQueue");
                sortingLayerID = serializedObject.FindProperty("sortingLayerID");
                sortingOrder = serializedObject.FindProperty("sortingOrder");

                constantDensity = serializedObject.FindProperty("constantDensity");

                noiseTexture = serializedObject.FindProperty("noiseTexture");
                noiseStrength = serializedObject.FindProperty("noiseStrength");
                noiseScale = serializedObject.FindProperty("noiseScale");
                noiseFinalMultiplier = serializedObject.FindProperty("noiseFinalMultiplier");
                noiseTextureOptimizedSize = serializedObject.FindProperty("noiseTextureOptimizedSize");

                useDetailNoise = serializedObject.FindProperty("useDetailNoise");
                detailTexture = serializedObject.FindProperty("detailTexture");
                detailScale = serializedObject.FindProperty("detailScale");
                detailStrength = serializedObject.FindProperty("detailStrength");
                detailOffset = serializedObject.FindProperty("detailOffset");

                density = serializedObject.FindProperty("density");
                shape = serializedObject.FindProperty("shape");
                customMesh = serializedObject.FindProperty("customMesh");
                scaleNoiseWithHeight = serializedObject.FindProperty("scaleNoiseWithHeight");
                border = serializedObject.FindProperty("border");

                customHeight = serializedObject.FindProperty("customHeight");
                height = serializedObject.FindProperty("height");
                verticalOffset = serializedObject.FindProperty("verticalOffset");

                distance = serializedObject.FindProperty("distance");
                distanceFallOff = serializedObject.FindProperty("distanceFallOff");
                maxDistance = serializedObject.FindProperty("maxDistance");
                maxDistanceFallOff = serializedObject.FindProperty("maxDistanceFallOff");

                terrainFit = serializedObject.FindProperty("terrainFit");
                terrainFitResolution = serializedObject.FindProperty("terrainFitResolution");
                terrainLayerMask = serializedObject.FindProperty("terrainLayerMask");
                terrainFogHeight = serializedObject.FindProperty("terrainFogHeight");
                terrainFogMinAltitude = serializedObject.FindProperty("terrainFogMinAltitude");
                terrainFogMaxAltitude = serializedObject.FindProperty("terrainFogMaxAltitude");

                albedo = serializedObject.FindProperty("albedo");
                enableDepthGradient = serializedObject.FindProperty("enableDepthGradient");
                depthGradient = serializedObject.FindProperty("depthGradient");
                depthGradientMaxDistance = serializedObject.FindProperty("depthGradientMaxDistance");
                enableHeightGradient = serializedObject.FindProperty("enableHeightGradient");
                heightGradient = serializedObject.FindProperty("heightGradient");

                brightness = serializedObject.FindProperty("brightness");
                deepObscurance = serializedObject.FindProperty("deepObscurance");
                specularColor = serializedObject.FindProperty("specularColor");
                specularThreshold = serializedObject.FindProperty("specularThreshold");
                specularIntensity = serializedObject.FindProperty("specularIntensity");
                ambientLightMultiplier = serializedObject.FindProperty("ambientLightMultiplier");

                turbulence = serializedObject.FindProperty("turbulence");
                windDirection = serializedObject.FindProperty("windDirection");
                useCustomDetailNoiseWindDirection = serializedObject.FindProperty("useCustomDetailNoiseWindDirection");
                detailNoiseWindDirection = serializedObject.FindProperty("detailNoiseWindDirection");

                dayNightCycle = serializedObject.FindProperty("dayNightCycle");
                sunDirection = serializedObject.FindProperty("sunDirection");
                sunColor = serializedObject.FindProperty("sunColor");
                sunIntensity = serializedObject.FindProperty("sunIntensity");

                lightDiffusionModel = serializedObject.FindProperty("lightDiffusionModel");
                lightDiffusionPower = serializedObject.FindProperty("lightDiffusionPower");
                lightDiffusionIntensity = serializedObject.FindProperty("lightDiffusionIntensity");
                lightDiffusionNearDepthAtten = serializedObject.FindProperty("lightDiffusionNearDepthAtten");

                receiveShadows = serializedObject.FindProperty("receiveShadows");
                shadowIntensity = serializedObject.FindProperty("shadowIntensity");
                shadowCancellation = serializedObject.FindProperty("shadowCancellation");
                shadowMaxDistance = serializedObject.FindProperty("shadowMaxDistance");

                cookie = serializedObject.FindProperty("cookie");

                distantFog = serializedObject.FindProperty("distantFog");
                distantFogShowInEditMode = serializedObject.FindProperty("distantFogShowInEditMode");
                distantFogColor = serializedObject.FindProperty("distantFogColor");
                distantFogStartDistance = serializedObject.FindProperty("distantFogStartDistance");
                distantFogDistanceDensity = serializedObject.FindProperty("distantFogDistanceDensity");
                distantFogMaxHeight = serializedObject.FindProperty("distantFogMaxHeight");
                distantFogBaseAltitude = serializedObject.FindProperty("distantFogBaseAltitude");
                distantFogSymmetrical = serializedObject.FindProperty("distantFogSymmetrical");
                distantFogHeightDensity = serializedObject.FindProperty("distantFogHeightDensity");
                distantFogDiffusionIntensity = serializedObject.FindProperty("distantFogDiffusionIntensity");
                distantFogRenderQueue = serializedObject.FindProperty("distantFogRenderQueue");
                distantFogTransparencySupport = serializedObject.FindProperty("distantFogTransparencySupport");

                distantFogNoise = serializedObject.FindProperty("distantFogNoise");
                distantFogNoiseTexture = serializedObject.FindProperty("distantFogNoiseTexture");
                distantFogNoiseWindDirection = serializedObject.FindProperty("distantFogNoiseWindDirection");
                distantFogDistanceNoiseScale = serializedObject.FindProperty("distantFogDistanceNoiseScale");
                distantFogDistanceNoiseStrength = serializedObject.FindProperty("distantFogDistanceNoiseStrength");
                distantFogDistanceNoiseMaxDistance = serializedObject.FindProperty("distantFogDistanceNoiseMaxDistance");

                showColors = EditorPrefs.GetBool(PrefsColors, true);
                showDensity = EditorPrefs.GetBool(PrefsDensity, true);
                showShape = EditorPrefs.GetBool(PrefsShape, true);
                showMinDistance = EditorPrefs.GetBool(PrefsDistance, true);
                showTerrain = EditorPrefs.GetBool(PrefsTerrain, true);
                showAnimation = EditorPrefs.GetBool(PrefsAnimation, false);
                showLighting = EditorPrefs.GetBool(PrefsLighting, true);
                showDistantFog = EditorPrefs.GetBool(PrefsDistantFog, true);
                showRendering = EditorPrefs.GetBool(PrefsRendering, true);
            }
            catch { }
        }

        static bool DrawHeader (string title, string prefKey, bool state) {
            if (foldoutStyle == null) {
                foldoutStyle = new GUIStyle("ShurikenModuleTitle");
                foldoutStyle.fontStyle = FontStyle.Bold;
                foldoutStyle.fixedHeight = 22f;
                foldoutStyle.contentOffset = new Vector2(20f, -2f);
            }
            if (headerRightLabelStyle == null) {
                headerRightLabelStyle = new GUIStyle(EditorStyles.label);
                headerRightLabelStyle.alignment = TextAnchor.MiddleRight;
                // headerRightLabelStyle.fontStyle = FontStyle.Bold;
            }
            Rect rect = GUILayoutUtility.GetRect(16f, 22f, foldoutStyle);
            GUI.Box(rect, title, foldoutStyle);
            Rect toggleRect = new Rect(rect.x + 6f, rect.y + 2f, 13f, 13f);
            if (Event.current.type == EventType.Repaint) {
                EditorStyles.foldout.Draw(toggleRect, false, false, state, false);
            }
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
                state = !state;
                EditorPrefs.SetBool(prefKey, state);
                Event.current.Use();
            }
            return state;
        }

        static bool DrawHeader (string title, string prefKey, bool state, string rightLabel) {
            state = DrawHeader(title, prefKey, state);
            if (!state && !string.IsNullOrEmpty(rightLabel)) {
                Rect rect = GUILayoutUtility.GetLastRect();
                Rect labelRect = new Rect(rect.xMax - 92f, rect.y - 2f, 88f, rect.height);
                GUI.Label(labelRect, rightLabel, headerRightLabelStyle);
            }
            return state;
        }

        public override void OnInspectorGUI () {

            serializedObject.Update();

            showRendering = DrawHeader("Rendering Settings", PrefsRendering, showRendering);
            if (showRendering) {
                EditorGUILayout.PropertyField(raymarchQuality);
                EditorGUILayout.PropertyField(raymarchNearStepping);
                EditorGUILayout.PropertyField(raymarchMinStep);
                EditorGUILayout.PropertyField(jittering);
                EditorGUILayout.PropertyField(dithering);
                EditorGUILayout.PropertyField(renderQueue);
                EditorGUILayout.PropertyField(sortingLayerID);
                EditorGUILayout.PropertyField(sortingOrder);
            }

            showDensity = DrawHeader("Density & Noise", PrefsDensity, showDensity);
            if (showDensity) {
                EditorGUILayout.PropertyField(density);
                EditorGUILayout.PropertyField(constantDensity);
                if (!constantDensity.boolValue) {
                    EditorGUILayout.PropertyField(noiseTexture);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(noiseStrength, new GUIContent("Strength"));
                    EditorGUILayout.PropertyField(noiseScale, new GUIContent("Scale"));
                    EditorGUILayout.PropertyField(scaleNoiseWithHeight);
                    EditorGUILayout.PropertyField(noiseFinalMultiplier, new GUIContent("Multiplier"));
                    EditorGUILayout.PropertyField(noiseTextureOptimizedSize, new GUIContent("Final Texture Size"));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(useDetailNoise, new GUIContent("Detail Noise"));
                    if (useDetailNoise.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(detailTexture);
                        EditorGUILayout.PropertyField(detailStrength, new GUIContent("Strength"));
                        EditorGUILayout.PropertyField(detailScale, new GUIContent("Scale"));
                        EditorGUILayout.PropertyField(detailOffset, new GUIContent("Offset"));
                        EditorGUI.indentLevel--;
                    }
                }
            }

            showColors = DrawHeader("Lighting & Colors", PrefsColors, showColors);
            if (showColors) {
                EditorGUILayout.PropertyField(albedo);
                Color albedoColor = albedo.colorValue;
                albedoColor.a = EditorGUILayout.Slider(new GUIContent("Alpha"), albedoColor.a, 0, 1f);
                albedo.colorValue = albedoColor;
                EditorGUILayout.PropertyField(brightness);
                EditorGUILayout.PropertyField(deepObscurance);
                EditorGUILayout.PropertyField(specularColor);
                EditorGUILayout.PropertyField(specularThreshold);
                EditorGUILayout.PropertyField(specularIntensity);
                EditorGUILayout.PropertyField(enableDepthGradient);
                if (enableDepthGradient.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(depthGradient);
                    EditorGUILayout.PropertyField(depthGradientMaxDistance, new GUIContent("Max Distance"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(enableHeightGradient);
                if (enableHeightGradient.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(heightGradient);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(dayNightCycle);
                if (dayNightCycle.boolValue) {
                    VolumetricFogManager manager = VolumetricFogManager.GetManagerIfExists();
                    if (manager != null && manager.sun == null) {
                        EditorGUILayout.HelpBox("You must assign a directional light to the Sun property of the Volumetric Fog Manager.", MessageType.Warning);
                        if (GUILayout.Button("Go to Volumetric Fog Manager")) {
                            Selection.activeGameObject = manager.gameObject;
                            EditorGUIUtility.ExitGUI();
                            return;
                        }
                    }
                } else {
                    EditorGUILayout.PropertyField(sunDirection);
                    EditorGUILayout.PropertyField(sunColor);
                    EditorGUILayout.PropertyField(sunIntensity);
                }
                EditorGUILayout.PropertyField(ambientLightMultiplier, new GUIContent("Ambient Light", "Amount of ambient light that influences fog colors"));
                EditorGUILayout.PropertyField(lightDiffusionModel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(lightDiffusionPower, new GUIContent("Spread"));
                EditorGUILayout.PropertyField(lightDiffusionIntensity, new GUIContent("Intensity"));
                EditorGUILayout.PropertyField(lightDiffusionNearDepthAtten, new GUIContent("Near Depth Attenuation", "Reduces the intensity of the sun light diffusion effect at distances below this threshold"));
                EditorGUI.indentLevel--;
#if UNITY_2021_3_OR_NEWER
                EditorGUILayout.PropertyField(cookie);
#endif
            }

            showShape = DrawHeader("Shape", PrefsShape, showShape);
            if (showShape) {
                EditorGUILayout.PropertyField(shape);
                if (shape.enumValueIndex == (int)VolumetricFogShape.Custom) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(customMesh);
                    EditorGUI.indentLevel--;
                }
#if FOG_BORDER
                EditorGUILayout.PropertyField(border);
#else
                GUI.enabled = false;
                EditorGUILayout.LabelField("Border", "(Disabled in Volumetric Fog Manager)");
                GUI.enabled = true;
#endif
                EditorGUILayout.PropertyField(customHeight, new GUIContent("Custom Volume Height"));
                if (customHeight.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(height);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(verticalOffset);
            }

            showMinDistance = DrawHeader("Distance Culling", PrefsDistance, showMinDistance);
            if (showMinDistance) {
                EditorGUILayout.PropertyField(distance, new GUIContent("Distance"));
                if (distance.floatValue > 0) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(distanceFallOff);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(maxDistance);
                EditorGUILayout.PropertyField(maxDistanceFallOff);
            }

            showLighting = DrawHeader("Directional Shadows", PrefsLighting, showLighting, receiveShadows.boolValue ? "ON" : "OFF");
            if (showLighting) {
                EditorGUILayout.PropertyField(receiveShadows);
                if (receiveShadows.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(shadowIntensity);
#if FOG_SHADOW_CANCELLATION
                    EditorGUILayout.PropertyField(shadowCancellation);
#endif
                    EditorGUILayout.PropertyField(shadowMaxDistance);
                    EditorGUI.indentLevel--;
                }
            }

            bool animationEnabled = turbulence.floatValue != 0f || windDirection.vector3Value != Vector3.zero || (useCustomDetailNoiseWindDirection.boolValue && detailNoiseWindDirection.vector3Value != Vector3.zero);
            showAnimation = DrawHeader("Animation", PrefsAnimation, showAnimation, animationEnabled ? "ON" : "OFF");
            if (showAnimation) {
                EditorGUILayout.PropertyField(turbulence);
                EditorGUILayout.PropertyField(windDirection);
                EditorGUILayout.PropertyField(useCustomDetailNoiseWindDirection, new GUIContent("Custom Detail Noise Wind"));
                if (useCustomDetailNoiseWindDirection.boolValue) {
                    EditorGUILayout.PropertyField(detailNoiseWindDirection);
                }
            }

            showTerrain = DrawHeader("Terrain Fit", PrefsTerrain, showTerrain, terrainFit.boolValue ? "ON" : "OFF");
            if (showTerrain) {
                EditorGUILayout.PropertyField(terrainFit);
                if (terrainFit.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(terrainFitResolution, new GUIContent("Resolution"));
                    EditorGUILayout.PropertyField(terrainLayerMask, new GUIContent("Layer Mask"));
                    if (Terrain.activeTerrain != null) {
                        int terrainLayer = Terrain.activeTerrain.gameObject.layer;
                        if ((terrainLayerMask.intValue & (1 << terrainLayer)) == 0) {
                            EditorGUILayout.HelpBox("Current terrain layer is not included in this layer mask. Terrain fit may not work properly.", MessageType.Warning);
                        }
                    }
                    EditorGUILayout.PropertyField(terrainFogHeight, new GUIContent("Fog Height"));
                    EditorGUILayout.PropertyField(terrainFogMinAltitude, new GUIContent("Min Altitude"));
                    EditorGUILayout.PropertyField(terrainFogMaxAltitude, new GUIContent("Max Altitude"));
                    EditorGUI.indentLevel--;
                }
            }

            showDistantFog = DrawHeader("Distant Fog", PrefsDistantFog, showDistantFog, distantFog.boolValue ? "ON" : "OFF");
            if (showDistantFog) {
                EditorGUILayout.PropertyField(distantFog, new GUIContent("Enable Distant Fog"));
                if (distantFog.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(distantFogColor, new GUIContent("Color"));
                    EditorGUILayout.PropertyField(distantFogStartDistance, new GUIContent("Start Distance"));
                    EditorGUILayout.PropertyField(distantFogDistanceDensity, new GUIContent("Distance Density"));
                    EditorGUILayout.PropertyField(distantFogBaseAltitude, new GUIContent("Base Altitude"));
                    EditorGUILayout.PropertyField(distantFogMaxHeight, new GUIContent("Height"));
                    EditorGUILayout.PropertyField(distantFogSymmetrical, new GUIContent("Symmetrical"));
                    EditorGUILayout.PropertyField(distantFogHeightDensity, new GUIContent("Height Density"));
                    EditorGUILayout.PropertyField(distantFogNoise, new GUIContent("Distance Noise"));
                    if (distantFogNoise.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(distantFogNoiseTexture, new GUIContent("Noise Texture"));
                        EditorGUILayout.PropertyField(distantFogDistanceNoiseStrength, new GUIContent("Strength"));
                        EditorGUILayout.PropertyField(distantFogDistanceNoiseScale, new GUIContent("Scale"));
                        EditorGUILayout.PropertyField(distantFogDistanceNoiseMaxDistance, new GUIContent("Max Distance"));
                        EditorGUILayout.PropertyField(distantFogNoiseWindDirection, new GUIContent("Wind Direction"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(distantFogDiffusionIntensity, new GUIContent("Diffusion Intensity Multiplier"));
                    EditorGUILayout.PropertyField(distantFogTransparencySupport, new GUIContent("Transparency Support", "Enables transparency support for distant fog. When enabled, distant fog will render correctly behind and over transparent objects."));
                    if (distantFogTransparencySupport.boolValue) {
                        EditorGUILayout.HelpBox("Transparency Support requires the 'Include Transparent' option to be enabled in the Volumetric Fog Manager.", MessageType.Info);
                    }
                    EditorGUILayout.PropertyField(distantFogRenderQueue, new GUIContent("Render Queue"));
                    if (distantFogTransparencySupport.boolValue && distantFogRenderQueue.intValue < 3001) {
                        EditorGUILayout.HelpBox("Render Queue will be enforced to 3001 when Transparency Support is enabled.", MessageType.Info);
                    }
                    if (VolumetricFogRenderFeature.isRenderingBeforeTransparents) {
                        EditorGUILayout.HelpBox("When using the Volumetric Fog Render Feature, the Distant Fog will be rendered always first before any fog volume.", MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(distantFogShowInEditMode, new GUIContent("Show In Edit Mode", "When disabled, distant fog will only be visible in play mode"));
                    EditorGUI.indentLevel--;

                }
            }



            serializedObject.ApplyModifiedProperties();

        }
    }

}
