using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CompassNavigatorPro {

    [CustomEditor(typeof(CompassProPOI), true)]
    [CanEditMultipleObjects]
    public class CompassProPOIInspector : Editor {

        bool expandGeneralSettings, expandCompassBarSettings, expandMiniMapSettings, expandScreenIndicatorSettings, expandOtherOptions;

        SerializedProperty id;
        SerializedProperty compassGroupMask;
        SerializedProperty priority;
        SerializedProperty dontDestroyOnLoad;
        SerializedProperty ignoreAreaOfInterest;

        SerializedProperty visibility, visibleDistanceOverride, visibleMinDistanceOverride, titleMinPOIDistanceOverride, iconShowDistance;
        SerializedProperty iconScale, iconScaleIsFixed, iconNonVisited, iconVisited, tintColor, clampPosition;
        SerializedProperty title, titleVisibility;
        SerializedProperty positionOffset;
        SerializedProperty showSceneGizmo;

        SerializedProperty canBeVisited, radius, visitedDistanceOverride, hideWhenVisited, isVisited, visitedText, playAudioClipWhenVisited, visitedAudioClipOverride;

        SerializedProperty showOnScreenIndicator, onScreenIndicatorScale, onScreenIndicatorShowDistance, onScreenIndicatorShowTitle, onScreenIndicatorNearFadeDistance, onScreenIndicatorNearFadeMin, onScreenIndicatorFarDistance, onScreenIndicatorFarFadeDistance, onScreenIndicatorPrefabOverride;
        SerializedProperty showOffScreenIndicator, offScreenIndicatorMarginOverride, offScreenIndicatorShowDistance;

        SerializedProperty beaconAudioClip, scanHitAudioClip;
        SerializedProperty heartbeatEnabled, heartbeatAudioClip, heartbeatDistance, heartbeatInterval;

        SerializedProperty miniMapType, miniMapVisibility, miniMapVisibleDistanceOverride, miniMapClampPosition, miniMapClampedScaleMultiplier, miniMapShowRotation, miniMapRotationAngleOffset, miniMapIconScale;
        SerializedProperty miniMapIconPrefabOverride;

        SerializedProperty miniMapShowCircle, miniMapCircleRadius, miniMapCircleColor, miniMapCircleInnerColor, miniMapCircleStartRadius, miniMapCircleAnimationWhenAppears, miniMapCircleAnimationRepetitions;

        static GUIStyle sectionHeaderStyle, titleLabelStyle;
        static Color titleColor;

        CompassProPOI[] pois;

        void OnEnable() {
            titleColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.16f, 0.4f);

            // Get all selected POIs
            pois = new CompassProPOI[targets.Length];
            for (int i = 0; i < targets.Length; i++) {
                pois[i] = (CompassProPOI)targets[i];
            }

            id = serializedObject.FindProperty("id");
            compassGroupMask = serializedObject.FindProperty("_compassGroupMask");
            priority = serializedObject.FindProperty("priority");
            dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            ignoreAreaOfInterest = serializedObject.FindProperty("ignoreAreaOfInterest");
            visibility = serializedObject.FindProperty("visibility");
            visibleDistanceOverride = serializedObject.FindProperty("visibleDistanceOverride");
            visibleMinDistanceOverride = serializedObject.FindProperty("visibleMinDistanceOverride");
            titleMinPOIDistanceOverride = serializedObject.FindProperty("titleMinPOIDistanceOverride");
            iconShowDistance = serializedObject.FindProperty("iconShowDistance");
            iconScale = serializedObject.FindProperty("iconScale");
            iconScaleIsFixed = serializedObject.FindProperty("iconScaleIsFixed");
            clampPosition = serializedObject.FindProperty("clampPosition");
            positionOffset = serializedObject.FindProperty("positionOffset");
            showSceneGizmo = serializedObject.FindProperty("showSceneGizmo");

            canBeVisited = serializedObject.FindProperty("canBeVisited");
            iconNonVisited = serializedObject.FindProperty("iconNonVisited");
            iconVisited = serializedObject.FindProperty("iconVisited");
            tintColor = serializedObject.FindProperty("tintColor");
            title = serializedObject.FindProperty("title");
            titleVisibility = serializedObject.FindProperty("titleVisibility");
            radius = serializedObject.FindProperty("radius");
            visitedDistanceOverride = serializedObject.FindProperty("visitedDistanceOverride");
            hideWhenVisited = serializedObject.FindProperty("hideWhenVisited");
            isVisited = serializedObject.FindProperty("_isVisitedInitial");
            visitedText = serializedObject.FindProperty("visitedText");
            playAudioClipWhenVisited = serializedObject.FindProperty("playAudioClipWhenVisited");
            visitedAudioClipOverride = serializedObject.FindProperty("visitedAudioClipOverride");

            miniMapType = serializedObject.FindProperty("miniMapType");
            miniMapVisibility = serializedObject.FindProperty("miniMapVisibility");
            miniMapClampPosition = serializedObject.FindProperty("miniMapClampPosition");
            miniMapClampedScaleMultiplier = serializedObject.FindProperty("miniMapClampedScaleMultiplier");
            miniMapShowRotation = serializedObject.FindProperty("miniMapShowRotation");
            miniMapRotationAngleOffset = serializedObject.FindProperty("miniMapRotationAngleOffset");
            miniMapIconScale = serializedObject.FindProperty("miniMapIconScale");
            miniMapVisibleDistanceOverride = serializedObject.FindProperty("miniMapVisibleDistanceOverride");
            miniMapIconPrefabOverride = serializedObject.FindProperty("miniMapIconPrefabOverride");

            showOnScreenIndicator = serializedObject.FindProperty("showOnScreenIndicator");
            onScreenIndicatorScale = serializedObject.FindProperty("onScreenIndicatorScale");
            onScreenIndicatorShowDistance = serializedObject.FindProperty("onScreenIndicatorShowDistance");
            onScreenIndicatorShowTitle = serializedObject.FindProperty("onScreenIndicatorShowTitle");
            onScreenIndicatorNearFadeDistance = serializedObject.FindProperty("onScreenIndicatorNearFadeDistance");
            onScreenIndicatorNearFadeMin = serializedObject.FindProperty("onScreenIndicatorNearFadeMin");
            onScreenIndicatorFarDistance = serializedObject.FindProperty("onScreenIndicatorFarDistance");
            onScreenIndicatorFarFadeDistance = serializedObject.FindProperty("onScreenIndicatorFarFadeDistance");
            onScreenIndicatorPrefabOverride = serializedObject.FindProperty("onScreenIndicatorPrefabOverride");

            showOffScreenIndicator = serializedObject.FindProperty("showOffScreenIndicator");
            offScreenIndicatorMarginOverride = serializedObject.FindProperty("offScreenIndicatorMarginOverride");
            offScreenIndicatorShowDistance = serializedObject.FindProperty("offScreenIndicatorShowDistance");

            heartbeatEnabled = serializedObject.FindProperty("heartbeatEnabled");
            heartbeatAudioClip = serializedObject.FindProperty("heartbeatAudioClip");
            heartbeatDistance = serializedObject.FindProperty("heartbeatDistance");
            heartbeatInterval = serializedObject.FindProperty("heartbeatInterval");

            beaconAudioClip = serializedObject.FindProperty("beaconAudioClip");
            scanHitAudioClip = serializedObject.FindProperty("scanHitAudioClip");

            miniMapShowCircle = serializedObject.FindProperty("miniMapShowCircle");
            miniMapCircleRadius = serializedObject.FindProperty("miniMapCircleRadius");
            miniMapCircleColor = serializedObject.FindProperty("miniMapCircleColor");
            miniMapCircleInnerColor = serializedObject.FindProperty("miniMapCircleInnerColor");
            miniMapCircleStartRadius = serializedObject.FindProperty("miniMapCircleStartRadius");
            miniMapCircleAnimationWhenAppears = serializedObject.FindProperty("miniMapCircleAnimationWhenAppears");
            miniMapCircleAnimationRepetitions = serializedObject.FindProperty("miniMapCircleAnimationRepetitions");

            expandGeneralSettings = EditorPrefs.GetBool("CNPOIGeneralSettings", true);
            expandCompassBarSettings = EditorPrefs.GetBool("CNPOICompassBarSettings", true);
            expandMiniMapSettings = EditorPrefs.GetBool("CNPOIMiniMapSettings", true);
            expandScreenIndicatorSettings = EditorPrefs.GetBool("CNPOIScreenIndicatorSettings", true);
            expandOtherOptions = EditorPrefs.GetBool("CNPOIOtherSettings", true);
        }

        private void OnDisable() {
            EditorPrefs.SetBool("CNPOIGeneralSettings", expandGeneralSettings);
            EditorPrefs.SetBool("CNPOICompassBarSettings", expandCompassBarSettings);
            EditorPrefs.SetBool("CNPOIMiniMapSettings", expandMiniMapSettings);
            EditorPrefs.SetBool("CNPOIScreenIndicatorSettings", expandScreenIndicatorSettings);
            EditorPrefs.SetBool("CNPOIOtherSettings", expandOtherOptions);
        }

        public override void OnInspectorGUI() {

            if (sectionHeaderStyle == null) {
                sectionHeaderStyle = new GUIStyle(EditorStyles.foldout);
                sectionHeaderStyle.fontStyle = FontStyle.Bold;
            }

            GUIContent overrideLabel = new GUIContent("Custom Overrides", "These settings can be used to override the global values set in Compass Navigator Pro component.");

            serializedObject.Update();

            expandGeneralSettings = DrawSectionTitle("General POI Settings", expandGeneralSettings);
            if (expandGeneralSettings) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(id);
                if (GUILayout.Button("Clear", GUILayout.Width(80))) {
                    foreach (var poi in pois) {
                        poi.id = 0;
                    }
                }
                if (GUILayout.Button("Randomize", GUILayout.Width(80))) {
                    foreach (var poi in pois) {
                        poi.GenerateNewId();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (pois[0].id == 0) {
                    EditorGUILayout.HelpBox("ID is 0. This POI id will automatically be generated.", MessageType.Info);
                }

                // Compass group mask checkboxes
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Compass Groups", "Select which compass instances will display this POI."), GUILayout.Width(EditorGUIUtility.labelWidth));
                int mask = compassGroupMask.intValue;
                bool g1 = (mask & 1) != 0;
                bool g2 = (mask & 2) != 0;
                bool g3 = (mask & 4) != 0;
                bool g4 = (mask & 8) != 0;
                g1 = EditorGUILayout.ToggleLeft("1", g1, GUILayout.Width(30));
                g2 = EditorGUILayout.ToggleLeft("2", g2, GUILayout.Width(30));
                g3 = EditorGUILayout.ToggleLeft("3", g3, GUILayout.Width(30));
                g4 = EditorGUILayout.ToggleLeft("4", g4, GUILayout.Width(30));
                mask = (g1 ? 1 : 0) | (g2 ? 2 : 0) | (g3 ? 4 : 0) | (g4 ? 8 : 0);
                compassGroupMask.intValue = mask;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(iconNonVisited, new GUIContent("Icon"));
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(positionOffset);
                EditorGUILayout.PropertyField(showSceneGizmo, new GUIContent("Show Scene Gizmo"));
                EditorGUILayout.PropertyField(canBeVisited);
                if (canBeVisited.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(isVisited);
                    EditorGUILayout.PropertyField(iconVisited, new GUIContent("Icon When Visited"));
                    EditorGUILayout.PropertyField(visitedDistanceOverride);
                    EditorGUILayout.PropertyField(playAudioClipWhenVisited);
                    if (playAudioClipWhenVisited.boolValue) {
                        EditorGUILayout.PropertyField(visitedAudioClipOverride, new GUIContent("Custom Audio Clip"));
                    }
                    EditorGUILayout.PropertyField(hideWhenVisited);
                    EditorGUILayout.PropertyField(heartbeatEnabled, new GUIContent("Enable Heartbeat"));
                    if (heartbeatEnabled.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(heartbeatDistance, new GUIContent("Start Distance"));
                        EditorGUILayout.PropertyField(heartbeatAudioClip, new GUIContent("Audio Clip"));
                        EditorGUILayout.PropertyField(heartbeatInterval, new GUIContent("Interval"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(dontDestroyOnLoad);
                EditorGUILayout.PropertyField(ignoreAreaOfInterest);
                EditorGUILayout.Separator();
            }

            expandCompassBarSettings = DrawSectionTitle("Compass Bar Settings", expandCompassBarSettings);
            if (expandCompassBarSettings) {
                EditorGUILayout.PropertyField(visibility, new GUIContent("Visibility In Compass Bar"));
                if (targets.Length == 1) {
                    EditorGUILayout.LabelField("Is Visible Now", pois[0].isVisible.ToString());
                }
                if (visibility.intValue != (int)POIVisibility.AlwaysHidden) {
                    EditorGUILayout.PropertyField(iconScale);
                    EditorGUILayout.PropertyField(iconScaleIsFixed, new GUIContent("Fixed Scale"));
                    EditorGUILayout.PropertyField(clampPosition);
                    EditorGUILayout.PropertyField(tintColor);
                    EditorGUILayout.PropertyField(title);
                    EditorGUILayout.PropertyField(titleVisibility);
                    EditorGUILayout.PropertyField(visitedText);
                    EditorGUILayout.PropertyField(iconShowDistance, new GUIContent("Show Distance Text"));
                    EditorGUILayout.LabelField(overrideLabel, EditorStyles.miniBoldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(visibleDistanceOverride, new GUIContent("Max Distance"));
                    EditorGUILayout.PropertyField(visibleMinDistanceOverride, new GUIContent("Min Distance"));
                    EditorGUILayout.PropertyField(titleMinPOIDistanceOverride, new GUIContent("Min Distance Title"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Separator();
            }

            expandMiniMapSettings = DrawSectionTitle("Mini-Map Settings", expandMiniMapSettings);
            if (expandMiniMapSettings) {
                EditorGUILayout.PropertyField(miniMapType, new GUIContent("Allowed Minimap")); ;
                EditorGUILayout.PropertyField(miniMapVisibility, new GUIContent("Visibility In Minimap")); ;
                if (miniMapVisibility.intValue != (int)POIVisibility.AlwaysHidden) {
                    if (targets.Length == 1) {
                        EditorGUILayout.LabelField("Is Visible Now", pois[0].miniMapIsVisible.ToString());
                    }
                    EditorGUILayout.PropertyField(miniMapIconScale, new GUIContent("Icon Scale"));
                    EditorGUILayout.PropertyField(miniMapClampPosition, new GUIContent("Clamp Position"));
                    if (miniMapClampPosition.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(miniMapClampedScaleMultiplier, new GUIContent("Scale Multiplier"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(miniMapShowRotation, new GUIContent("Show Rotation"));
                    EditorGUILayout.PropertyField(miniMapRotationAngleOffset, new GUIContent("Rotation Angle Offset"));
                    EditorGUILayout.PropertyField(miniMapShowCircle, new GUIContent("Show Circle Radius"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(miniMapCircleRadius, new GUIContent("Radius", "Radius of the circle to be shown on the minimap. Set it to 0 to use the general radius value."));
                    EditorGUILayout.PropertyField(miniMapCircleColor, new GUIContent("Border Color"));
                    EditorGUILayout.PropertyField(miniMapCircleInnerColor, new GUIContent("Inner Color"));
                    EditorGUILayout.PropertyField(miniMapCircleStartRadius, new GUIContent("Start Radius"));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(miniMapCircleAnimationWhenAppears, new GUIContent("Enable Circle Animation"));
                    if (miniMapCircleAnimationWhenAppears.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(miniMapCircleAnimationRepetitions, new GUIContent("Repetitions"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.LabelField(overrideLabel, EditorStyles.miniBoldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(miniMapIconPrefabOverride, new GUIContent("Icon Prefab"));
                    EditorGUILayout.PropertyField(miniMapVisibleDistanceOverride, new GUIContent("Visible Max Distance"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Separator();
            }

            expandScreenIndicatorSettings = DrawSectionTitle("Screen Indicators Settings", expandScreenIndicatorSettings);
            if (expandScreenIndicatorSettings) {
                EditorGUILayout.PropertyField(showOnScreenIndicator, new GUIContent("Show On-Screen Indicator"));
                if (showOnScreenIndicator.boolValue) {
                    CompassPro compass = CompassPro.instance;
                    if (compass != null && !compass.showOnScreenIndicators) {
                        EditorGUILayout.HelpBox("On-Screen Indicators are disabled globally on the Compass Navigator Pro component.", MessageType.Warning);
                    }
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(onScreenIndicatorScale, new GUIContent("Scale"));
                    EditorGUILayout.PropertyField(onScreenIndicatorShowDistance, new GUIContent("Show Distance"));
                    if (onScreenIndicatorShowDistance.boolValue && compass != null && compass.showOnScreenIndicators && !compass.onScreenIndicatorShowDistance) {
                        EditorGUILayout.HelpBox("Show Distance is disabled globally on the Compass Navigator Pro component.", MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(onScreenIndicatorShowTitle, new GUIContent("Show Title"));
                    if (onScreenIndicatorShowTitle.boolValue && compass != null && compass.showOnScreenIndicators && !compass.onScreenIndicatorShowTitle) {
                        EditorGUILayout.HelpBox("Show Title is disabled globally on the Compass Navigator Pro component.", MessageType.Warning);
                    }
                    EditorGUILayout.LabelField(overrideLabel, EditorStyles.miniBoldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(onScreenIndicatorPrefabOverride, new GUIContent("Indicator Prefab"));
                    EditorGUILayout.PropertyField(onScreenIndicatorNearFadeDistance, new GUIContent("Near Fade Distance"));
                    EditorGUILayout.PropertyField(onScreenIndicatorNearFadeMin, new GUIContent("Near Fade Min"));
                    EditorGUILayout.PropertyField(onScreenIndicatorFarDistance, new GUIContent("Max Visible Distance"));
                    EditorGUILayout.PropertyField(onScreenIndicatorFarFadeDistance, new GUIContent("Far Fade Distance"));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(showOffScreenIndicator, new GUIContent("Show Off-Screen Indicator"));
                if (showOffScreenIndicator.boolValue) {
                    CompassPro compass = CompassPro.instance;
                    if (compass != null && !compass.showOffScreenIndicators) {
                        EditorGUILayout.HelpBox("Off-Screen Indicators are disabled globally on the Compass Navigator Pro component.", MessageType.Warning);
                    }
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(offScreenIndicatorShowDistance, new GUIContent("Show Distance"));
                    if (offScreenIndicatorShowDistance.boolValue && compass != null && compass.showOffScreenIndicators && !compass.offScreenIndicatorShowDistance) {
                        EditorGUILayout.HelpBox("Show Distance is disabled globally on the Compass Navigator Pro component.", MessageType.Warning);
                    }
                    EditorGUILayout.LabelField(overrideLabel, EditorStyles.miniBoldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(offScreenIndicatorMarginOverride, new GUIContent("Margin"));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Separator();
            }

            expandOtherOptions = DrawSectionTitle("Other Options", expandOtherOptions);
            if (expandOtherOptions) {
                EditorGUILayout.PropertyField(priority);
                EditorGUILayout.PropertyField(beaconAudioClip);
                EditorGUILayout.PropertyField(scanHitAudioClip);
            }

            if (serializedObject.ApplyModifiedProperties()) {
                CompassPro lastCompass = null;
                foreach (var poi in pois) {
                    if (poi.compass != null && poi.compass != lastCompass) {
                        lastCompass = poi.compass;
                        lastCompass.Refresh();
                    }
                }
            }
        }

        bool DrawSectionTitle(string s, bool expanded) {
            if (titleLabelStyle == null) {
                GUIStyle skurikenModuleTitleStyle = "ShurikenModuleTitle";
                titleLabelStyle = new GUIStyle(skurikenModuleTitleStyle);
                titleLabelStyle.contentOffset = new Vector2(5f, -2f);
                titleLabelStyle.normal.textColor = titleColor;
                titleLabelStyle.fixedHeight = 22;
                titleLabelStyle.fontStyle = FontStyle.Bold;
            }

            if (GUILayout.Button(s, titleLabelStyle)) expanded = !expanded;
            return expanded;
        }

    }
}
