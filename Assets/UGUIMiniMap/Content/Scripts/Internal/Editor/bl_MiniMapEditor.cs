using Lovatto.MiniMap;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(bl_MiniMap))]
public class bl_MiniMapEditor : Editor
{
    AnimBool GeneralAnim;
    AnimBool ZoomAnim;
    AnimBool RotationAnim;
    AnimBool GripAnim;
    AnimBool PositionAnim;
    AnimBool AnimationsAnim;
    AnimBool DragAnim;
    AnimBool RenderAnim;
    AnimBool ReferencesAnim;
    AnimBool MarksAnim;
    AnimBool FogAnim;
    SerializedProperty generalProp;
    SerializedProperty zoomProp;
    SerializedProperty positionProp;
    SerializedProperty rotationProp;
    SerializedProperty animationProp;
    SerializedProperty gripProp;
    SerializedProperty dragProp;
    SerializedProperty renderProp;
    SerializedProperty refProp;
    SerializedProperty marksProp;
    SerializedProperty fogProp;

    private void OnEnable()
    {
        generalProp = serializedObject.FindProperty("m_Target");
        InitAnim(ref GeneralAnim, generalProp);

        zoomProp = serializedObject.FindProperty("DefaultHeight");
        InitAnim(ref ZoomAnim, zoomProp);

        positionProp = serializedObject.FindProperty("FullMapPosition");
        InitAnim(ref PositionAnim, positionProp);

        rotationProp = serializedObject.FindProperty("mapShape");
        InitAnim(ref RotationAnim, rotationProp);

        gripProp = serializedObject.FindProperty("ShowAreaGrid");
        InitAnim(ref GripAnim, gripProp);

        animationProp = serializedObject.FindProperty("FadeOnFullScreen");
        InitAnim(ref AnimationsAnim, animationProp);

        dragProp = serializedObject.FindProperty("DragOnlyOnFullScreen");
        InitAnim(ref DragAnim, dragProp);

        renderProp = serializedObject.FindProperty("PlayerIconSprite");
        InitAnim(ref RenderAnim, renderProp);

        refProp = serializedObject.FindProperty("minimapRig");
        InitAnim(ref ReferencesAnim, refProp);

        marksProp = serializedObject.FindProperty("AllowMapMarks");
        InitAnim(ref MarksAnim, marksProp);

        fogProp = serializedObject.FindProperty("hasFogOfWar");
        InitAnim(ref FogAnim, fogProp);
    }

    private void InitAnim(ref AnimBool anim, SerializedProperty prop)
    {
        anim = new AnimBool(prop.isExpanded);
        anim.valueChanged.AddListener(Repaint);
    }

    void CheckLayer(bl_MiniMap script)
    {
        string layer = LayerMask.LayerToName(script.MiniMapLayer);
        if (string.IsNullOrEmpty(layer))
        {
            CreateLayer("MiniMap");
            int layerID = LayerMask.NameToLayer("MiniMap");
            script.MiniMapLayer = layerID;
        }
    }

    public override void OnInspectorGUI()
    {
        bl_MiniMap script = (bl_MiniMap)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(target);
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        CheckLayer(script);
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("window");
        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("General Settings", EditorStyles.toolbarPopup)) { generalProp.isExpanded = !generalProp.isExpanded; GeneralAnim.target = generalProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(GeneralAnim.faded))
        {
            script.m_Target = EditorGUILayout.ObjectField(new GUIContent("Target", "The target that the minimap will follow, if the target is not instanced in the scene but in runtime, you can assign it by code."), script.m_Target, typeof(GameObject), allowSceneObjects) as GameObject;
            script.MiniMapLayer = EditorGUILayout.LayerField(new GUIContent("MiniMap Layer", "The special layer for the minimap stuff, this should be automatically set up in the Project Settings with the name of 'Minimap'."), script.MiniMapLayer);
            script.renderType = (MiniMapRenderType)EditorGUILayout.EnumPopup(new GUIContent("Render Mode", "The Minimap render mode, Realtime = render the map in realtime (costly), Picture = Render a screenshot of the a map."), script.renderType);
            if (script.renderType == MiniMapRenderType.RealTime)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("excludeLayers"), new GUIContent("Exclude Layers", "Layers that wont be render in the minimap."), true);
            }
            script.canvasRenderMode = (MiniMapRenderMode)EditorGUILayout.EnumPopup(new GUIContent("Draw Mode", "The draw mode of the minimap UI (not the game itself), 2D Mode = UI without depth, 3D Mode = UI with depth effect. (Not actual world space)."), script.canvasRenderMode);
            if (script.canvasRenderMode == MiniMapRenderMode.Mode2D)
            {
                script.Ortographic2D = EditorGUILayout.ToggleLeft(new GUIContent("Orthographic", "Render the map in Orthographic perspective?, useful for global minimaps that shown the whole map instead of a focus area where the target is."), script.Ortographic2D, EditorStyles.toolbarButton);
                GUILayout.Space(2);
            }
            script.mapMode = (MiniMapMapType)EditorGUILayout.EnumPopup(new GUIContent("Map Mode", "Local = Follow the target and Render a portion of the map where the target is, Global = render the whole map area."), script.mapMode);
            if (script.renderType == MiniMapRenderType.Picture)
            {
                GUILayout.Label("Map Render");
                GUILayout.BeginHorizontal();
                if (script.mapRender != null)
                {
                    GUILayout.Space(10);
                    var rrect = GUILayoutUtility.GetRect(50, 50);
                    script.mapRender.DrawOnGUI(rrect);
                }
                GUILayout.FlexibleSpace();
                script.mapRender = EditorGUILayout.ObjectField(new GUIContent("", "The component responsible for rendering the map texture."), script.mapRender, typeof(bl_MapRender), allowSceneObjects) as bl_MapRender;

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                if (script.mapBounds != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Render Map", "Instance the tool to Bake a render of the map."), GUILayout.Width(150)))
                    {
                        SetupScreenShot();
                    }
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("Set Bounds", "Select and highlight the map bounds transform in the scene."), GUILayout.Width(75)))
                    {
                        Selection.activeTransform = script.mapBounds.BoundTransform;
                        EditorGUIUtility.PingObject(script.mapBounds.BoundTransform);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(2);
            script.isMobile = EditorGUILayout.ToggleLeft(new GUIContent("Is For Mobile", "Is this project for mobile/touch devices?"), script.isMobile, EditorStyles.toolbarButton);
            script.UpdateRate = EditorGUILayout.IntSlider(new GUIContent("Update Rate", "Minimap update rate, 1 = each frame, 2 = each 2 frame, 5 = each 5 frames, etc..."), script.UpdateRate, 1, 10);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Zoom Settings", EditorStyles.toolbarPopup)) { zoomProp.isExpanded = !zoomProp.isExpanded; ZoomAnim.target = zoomProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(ZoomAnim.faded))
        {
            EditorGUILayout.LabelField(new GUIContent("Zoom Range", "Minimum and Maximum zoom in/out allowed."), EditorStyles.label);
            EditorGUILayout.BeginHorizontal();
            script.MinZoom = EditorGUILayout.FloatField(new GUIContent("", "The minimum zoom level allowed."), script.MinZoom, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref script.MinZoom, ref script.MaxZoom, 1, 200);
            script.MaxZoom = EditorGUILayout.FloatField(new GUIContent("", "The maximum zoom level allowed."), script.MaxZoom, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            script.DefaultHeight = EditorGUILayout.Slider(new GUIContent("Default Zoom", "The initial zoom level when the minimap starts."), script.DefaultHeight, script.MinZoom, script.MaxZoom);
            script.saveZoomInRuntime = EditorGUILayout.ToggleLeft(new GUIContent("Save runtime zoom modifications?", "Save the zoom changes made in runtime so next time the game is loaded that will be the default zoom?"), script.saveZoomInRuntime, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            script.iconsSizeRelativeToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Icons Size Relative to Zoom", "Make the icons size relative to the zoom, this will make the icons bigger when the zoom is increased and smaller when the zoom is decreased."), script.iconsSizeRelativeToZoom, EditorStyles.toolbarButton);
            script.scrollSensitivity = EditorGUILayout.IntSlider(new GUIContent("Zoom Steps", "The amount of zoom increase or deacrese when change it with the scroll."), script.scrollSensitivity, 1, 10);
            script.IconMultiplier = EditorGUILayout.Slider(new GUIContent("Icon Size Multiplier", "Multiplier for the size of all minimap icons."), script.IconMultiplier, 0.05f, 2);
            script.LerpHeight = EditorGUILayout.Slider(new GUIContent("Zoom Speed", "The speed at which the zoom transitions occur."), script.LerpHeight, 1, 20);

            GUILayout.Space(2);
            if (PlayerPrefs.HasKey(bl_MiniMap.MMHeightKey))
            {
                if (GUILayout.Button(new GUIContent("Reset In-Game Zoom", "Reset the in-game modified zoom value and use the inspector defined default zoom value.")))
                {
                    PlayerPrefs.DeleteKey(bl_MiniMap.MMHeightKey);
                }
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Position Settings", EditorStyles.toolbarPopup)) { positionProp.isExpanded = !positionProp.isExpanded; PositionAnim.target = positionProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(PositionAnim.faded))
        {
            script.lerpTrackingPosition = EditorGUILayout.ToggleLeft(new GUIContent("Smooth Player Position Tracking?", "Apply a smoothness to the target position follow? Not recommended if the target moves fast."), script.lerpTrackingPosition, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            script.fullScreenMode = (MiniMapFullScreenMode)EditorGUILayout.EnumPopup(new GUIContent("Fullscreen Mode", "How the minimap expands to fullscreen."), script.fullScreenMode);
            if (script.fullScreenMode != MiniMapFullScreenMode.NoFullScreen)
            {
                if (script.fullScreenMode == MiniMapFullScreenMode.ScreenArea)
                {
                    script.FullMapPosition = EditorGUILayout.Vector3Field(new GUIContent("FullScreen Map Position", "The position of the minimap when in fullscreen mode."), script.FullMapPosition);
                    script.FullMapSize = EditorGUILayout.Vector2Field(new GUIContent("FullScreen Map Size", "The size of the minimap when in fullscreen mode."), script.FullMapSize);
                }

                if (script.canvasRenderMode == MiniMapRenderMode.Mode3D)
                {
                    script.FullMapRotation = EditorGUILayout.Vector3Field(new GUIContent("FullScreen Map Rotation", "The rotation of the minimap when in fullscreen mode."), script.FullMapRotation);
                }

                if (script.fullScreenMode != MiniMapFullScreenMode.ScreenArea)
                {
                    script.fullScreenMargin = EditorGUILayout.Slider(new GUIContent("Fullscreen Margin", "The margin from the screen edges when in fullscreen mode."), script.fullScreenMargin, 0, 100);
                }
            }
        }
        if (script.fullScreenMode != MiniMapFullScreenMode.NoFullScreen)
        {
            if (script.fullScreenMode == MiniMapFullScreenMode.ScreenArea && positionProp.isExpanded)
            {
                if (GUILayout.Button(new GUIContent("Catch Position", "Capture the current minimap position and size for fullscreen settings.")))
                {
                    script.GetFullMapSize();
                }

                if (script._isPreviewFullscreen)
                {
                    if (GUILayout.Button(new GUIContent("Stop Fullscreen Preview", "Exit the fullscreen preview mode.")))
                    {
                        var ui = script.MiniMapUI;
                        if (ui != null)
                        {
                            ui.root.anchoredPosition = script.MiniMapPosition;
                            ui.root.sizeDelta = script.MiniMapSize;
                            ui.root.eulerAngles = script.MiniMapRotation;
                            ui.minimapMaskManager?.ChangeMaskType(false);
                        }
                        script._isPreviewFullscreen = false;
                        EditorUtility.SetDirty(script);
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Preview Fullscreen", "Preview how the minimap looks in fullscreen mode.")))
                    {
                        script.GetMiniMapSize();
                        var ui = script.MiniMapUI;
                        if (ui != null)
                        {
                            ui.root.anchoredPosition = script.FullMapPosition;
                            ui.root.sizeDelta = script.FullMapSize;
                            ui.root.eulerAngles = script.FullMapRotation;
                            ui.minimapMaskManager?.ChangeMaskType(true);
                        }
                        script._isPreviewFullscreen = true;
                        EditorUtility.SetDirty(script);
                    }
                }
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Rotation Settings", EditorStyles.toolbarPopup)) { rotationProp.isExpanded = !rotationProp.isExpanded; RotationAnim.target = rotationProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(RotationAnim.faded))
        {
            script.mapShape = (MiniMapMapShape)EditorGUILayout.EnumPopup(new GUIContent("Shape", "The shape of the minimap (Rectangle or Circle)."), script.mapShape);
            if (script.mapShape == MiniMapMapShape.Circle)
            {
                script.CompassSize = EditorGUILayout.Slider(new GUIContent("Circle Size", "The radius of the minimap circle, this is to delimitate the position of the minimap icons."), script.CompassSize, 25, 500);
            }
            script.RotationMode = (MiniMapRotationMode)EditorGUILayout.EnumPopup(new GUIContent("Rotation Mode", "How the minimap rotates relative to the target."), script.RotationMode);
            if (script.RotationMode != MiniMapRotationMode.RotateMap)
            {
                script.mapRotationOffset = EditorGUILayout.Slider(new GUIContent("Map Rotation Offset", "In some type of games, the cardinals points would work differently, this allow adjust the map direction to fit as needed."), script.mapRotationOffset, 0, 360);
            }
            script.iconsAlwaysFacingUp = EditorGUILayout.ToggleLeft(new GUIContent("Icons Always Facing Up?", "Force the minimap icons facing up or make them rotate towards their target forward direction?"), script.iconsAlwaysFacingUp, EditorStyles.toolbarButton);
            script.SmoothRotation = EditorGUILayout.ToggleLeft(new GUIContent("Smooth Rotation", "Enable smooth transitions for map rotation."), script.SmoothRotation, EditorStyles.toolbarButton);
            if (script.SmoothRotation) { script.LerpRotation = EditorGUILayout.Slider(new GUIContent("Rotation Lerp", "The speed of smooth rotation transitions."), script.LerpRotation, 1, 20); }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Grid Settings", EditorStyles.toolbarPopup)) { gripProp.isExpanded = !gripProp.isExpanded; GripAnim.target = gripProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(GripAnim.faded))
        {
            script.ShowAreaGrid = EditorGUILayout.ToggleLeft(new GUIContent("Show Dynamic Grid", "Display a grid overlay on the minimap."), script.ShowAreaGrid, EditorStyles.toolbarButton);
            if (script.ShowAreaGrid)
            {
                script.AreasSize = EditorGUILayout.Slider(new GUIContent("Row Grid Size", "The size of each grid cell."), script.AreasSize, 1, 25);
                script.gridOpacity = EditorGUILayout.Slider(new GUIContent("Grid Opacity", "The transparency level of the grid."), script.gridOpacity, 0, 1);
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        // fog of war settings

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Fog of War Settings", EditorStyles.toolbarPopup)) { fogProp.isExpanded = !fogProp.isExpanded; FogAnim.target = fogProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(FogAnim.faded))
        {
            script.hasFogOfWar = EditorGUILayout.ToggleLeft(new GUIContent("Enable Fog of War", "Hide unexplored areas of the map."), script.hasFogOfWar, EditorStyles.toolbarButton);
            if (script.hasFogOfWar)
            {
                script.fogOfWarUpdateRate = EditorGUILayout.IntSlider(new GUIContent("Fog Update Rate", "How often the fog of war updates the revealed area based on the target position."), script.fogOfWarUpdateRate, 1, 60);
                script.fogOfWarRadius = EditorGUILayout.Slider(new GUIContent("Revealed Radius", "The radius around the target that will be revealed in the minimap."), script.fogOfWarRadius, 0.01f, 0.5f);
                script.fogOfWarSoftness = EditorGUILayout.Slider(new GUIContent("Fog Softness", "The softness of the edge of the revealed area."), script.fogOfWarSoftness, 0.01f, 0.5f);
                script.fogOfWarColor = EditorGUILayout.ColorField(new GUIContent("Fog Color", "The color of the fog covering unexplored areas."), script.fogOfWarColor);
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Map Pointers Settings", EditorStyles.toolbarPopup)) { marksProp.isExpanded = !marksProp.isExpanded; MarksAnim.target = marksProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(MarksAnim.faded))
        {
            script.AllowMapMarks = EditorGUILayout.ToggleLeft(new GUIContent("Allow Map Pointers", "Allow create pointers when click over the minimap?"), script.AllowMapMarks, EditorStyles.toolbarButton);
            if (script.AllowMapMarks)
            {
                script.AllowMultipleMarks = EditorGUILayout.ToggleLeft(new GUIContent("Allow multiple marks", "Permit placing multiple map pointers simultaneously."), script.AllowMultipleMarks, EditorStyles.toolbarButton);
                script.showPathNav = EditorGUILayout.ToggleLeft(new GUIContent("Show Path Navigation", "Show Path Navigation from the player position to the mark position?"), script.showPathNav, EditorStyles.toolbarButton);
                script.MapPointerPrefab = EditorGUILayout.ObjectField(new GUIContent("Pointer Prefab", "The prefab used for map pointers."), script.MapPointerPrefab, typeof(GameObject), allowSceneObjects) as GameObject;
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Drag Settings", EditorStyles.toolbarPopup)) { dragProp.isExpanded = !dragProp.isExpanded; DragAnim.target = dragProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(DragAnim.faded))
        {
            script.CanDragMiniMap = EditorGUILayout.ToggleLeft(new GUIContent("Active Drag MiniMap", "Allow dragging the minimap to pan the view."), script.CanDragMiniMap, EditorStyles.toolbarButton);
            if (script.CanDragMiniMap)
            {
                script.DragOnlyOnFullScreen = EditorGUILayout.ToggleLeft(new GUIContent("Only on full screen", "Restrict dragging to fullscreen mode only."), script.DragOnlyOnFullScreen, EditorStyles.toolbarButton);
                script.ResetOffSetOnChange = EditorGUILayout.ToggleLeft(new GUIContent("Auto reset position", "Automatically reset the drag offset when switching modes."), script.ResetOffSetOnChange, EditorStyles.toolbarButton);
                var lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100;
                EditorGUILayout.BeginHorizontal();
                Vector2 v = script.DragMovementSpeed;
                v.x = EditorGUILayout.Slider(new GUIContent("Horizontal Speed", "Speed of horizontal dragging."), v.x, 0.01f, 30);
                v.y = EditorGUILayout.Slider(new GUIContent("Vertical Speed", "Speed of vertical dragging."), v.y, 0.01f, 30);
                script.DragMovementSpeed = v;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                Vector2 v2 = script.MaxOffSetPosition;
                v2.x = EditorGUILayout.Slider(new GUIContent("MinMax Horizontal", "Maximum horizontal drag offset."), v2.x, 1, 2000);
                v2.y = EditorGUILayout.Slider(new GUIContent("MinMax Vertical", "Maximum vertical drag offset."), v2.y, 1, 2000);
                script.MaxOffSetPosition = v2;
                EditorGUILayout.EndHorizontal();
                script.DragCursorIcon = EditorGUILayout.ObjectField(new GUIContent("Drag cursor image", "The cursor texture shown during dragging."), script.DragCursorIcon, typeof(Texture2D), allowSceneObjects) as Texture2D;
                EditorGUILayout.BeginHorizontal();
                Vector2 v3 = script.HotSpot;
                v3.x = EditorGUILayout.Slider(new GUIContent("Cursor X offset", "Horizontal offset for the drag cursor."), v3.x, 0.01f, 10);
                v3.y = EditorGUILayout.Slider(new GUIContent("Cursor Y offset", "Vertical offset for the drag cursor."), v3.y, 0.01f, 10);
                script.HotSpot = v3;
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = lw;
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Animations Settings", EditorStyles.toolbarPopup)) { rotationProp.isExpanded = !rotationProp.isExpanded; AnimationsAnim.target = rotationProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(AnimationsAnim.faded))
        {
            script.FadeOnFullScreen = EditorGUILayout.ToggleLeft(new GUIContent("Fade on full screen", "Fade the minimap when entering fullscreen."), script.FadeOnFullScreen, EditorStyles.toolbarButton);
            script.sizeTransitionDuration = EditorGUILayout.Slider(new GUIContent("Resize Transition Duration", "Duration of the size transition animation."), script.sizeTransitionDuration, 0.1f, 2);
            script.sizeTransitionCurve = EditorGUILayout.CurveField(new GUIContent("Resize Transition Curve", "Animation curve for size transitions."), script.sizeTransitionCurve);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Render Settings", EditorStyles.toolbarPopup)) { renderProp.isExpanded = !renderProp.isExpanded; RenderAnim.target = renderProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(RenderAnim.faded))
        {
            script.PlayerIconSprite = EditorGUILayout.ObjectField(new GUIContent("Player Icon", "The sprite used for the player icon on the minimap."), script.PlayerIconSprite, typeof(Sprite), false) as Sprite;
            script.playerColor = EditorGUILayout.ColorField(new GUIContent("Player Color", "The color tint applied to the player icon."), script.playerColor);
            script.emptySpaceColor = EditorGUILayout.ColorField(new GUIContent("Empty Space Color", "Color of the empty space in the minimap."), script.emptySpaceColor);
            if (script.showPathNav)
            {
                script.navPathColor = EditorGUILayout.ColorField(new GUIContent("Nav Path Color", "The color of the navigation path line."), script.navPathColor);
                script.navPathWidth = EditorGUILayout.Slider(new GUIContent("Nav Path Thickness", "The Thickness of the navigation path line."), script.navPathWidth, 0.1f, 10);
            }
            float size = script.playerIconSize;
            script.playerIconSize = EditorGUILayout.Slider(new GUIContent("Player Icon Size", "The size of the player icon."), script.playerIconSize, 1f, 40);
            if (size != script.playerIconSize && script.MiniMapUI != null && script.MiniMapUI.playerIcon != null)
            {
                script.MiniMapUI.playerIcon.SetSize(script.playerIconSize);
                EditorUtility.SetDirty(script.MiniMapUI.playerIcon);
            }
            script.overallOpacity = EditorGUILayout.Slider(new GUIContent("MiniMap Opacity", "The opacity of the whole minimap UI."), script.overallOpacity, 0, 1);
            script.backgroundOpacity = EditorGUILayout.Slider(new GUIContent("Background Opacity", "The opacity of the background UI in the minimap."), script.backgroundOpacity, 0, 1);
            if (script.renderType == MiniMapRenderType.Picture)
                script.planeSaturation = EditorGUILayout.Slider(new GUIContent("Map Saturation", "Adjust the color saturation of the map texture."), script.planeSaturation, 0.2f, 2);
            script.cameraUpdateMode = (MiniMapCameraUpdateMode)EditorGUILayout.EnumPopup(new GUIContent("Camera Update Mode", "Every Frame = Default engine camera update mode.\nRate Limited = Update the camera by script based in the minimap update rate limit (better performance)."), script.cameraUpdateMode);

            EditorGUILayout.BeginHorizontal();
            {
                script._rtSize = (MiniMapRTSize)EditorGUILayout.EnumPopup(new GUIContent("Render Texture Size", "Resolution of the render texture for the minimap."), script._rtSize);
                if (GUILayout.Button(new GUIContent("Set", "Apply the selected render texture size."), EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    string sizeString = script._rtSize.ToString().Replace("_", "");
                    string rtAssetName = "minimap_rt_" + sizeString;
                    string folderPath = Path.Combine(GetAssetFolderPath(), "Content/Art/UI/RenderTexture/");
                    string rtPath = folderPath + rtAssetName + ".renderTexture";

                    var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
                    if (rt != null)
                    {
                        script.miniMapCamera.targetTexture = rt;
                        EditorUtility.SetDirty(script.miniMapCamera);

                        var img = script.m_Canvas.GetComponentInChildren<bl_MiniMapTexture>(true);
                        if (img != null)
                        {
                            var rit = img.GetComponent<UnityEngine.UI.RawImage>();
                            rit.texture = rt;
                            EditorUtility.SetDirty(rit);
                        }

                        Debug.Log("RenderTexture changed!");
                    }
                    else
                    {
                        Debug.LogError("RenderTexture not found at: " + rtPath);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("References", EditorStyles.toolbarPopup)) { refProp.isExpanded = !refProp.isExpanded; ReferencesAnim.target = refProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(ReferencesAnim.faded))
        {
            script.minimapRig = EditorGUILayout.ObjectField(new GUIContent("Mini Map Rig", "The transform that holds the minimap camera and plane."), script.minimapRig, typeof(Transform), allowSceneObjects) as Transform;
            script.miniMapCamera = EditorGUILayout.ObjectField(new GUIContent("Mini Map Camera", "The camera used to render the minimap."), script.miniMapCamera, typeof(Camera), allowSceneObjects) as Camera;
            script.ItemPrefabSimple = EditorGUILayout.ObjectField(new GUIContent("Icon Simple Prefab", "The prefab for simple minimap icons."), script.ItemPrefabSimple, typeof(GameObject), allowSceneObjects) as GameObject;

            script.mapBounds = EditorGUILayout.ObjectField(new GUIContent("Map Bounds", "The component defining the boundaries of the map."), script.mapBounds, typeof(bl_MiniMapBounds), allowSceneObjects) as bl_MiniMapBounds;
            script.m_Canvas = EditorGUILayout.ObjectField(new GUIContent("Canvas", "The UI canvas containing the minimap."), script.m_Canvas, typeof(Canvas), allowSceneObjects) as Canvas;
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(script);

            script.OnValidate();
        }

    }

    public void CreateLayer(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layerProps = tagManager.FindProperty("layers");
        var propCount = layerProps.arraySize;

        SerializedProperty firstEmptyProp = null;

        for (var i = 0; i < propCount; i++)
        {
            var layerProp = layerProps.GetArrayElementAtIndex(i);

            var stringValue = layerProp.stringValue;

            if (stringValue == name) return;

            if (i < 8 || stringValue != string.Empty) continue;

            if (firstEmptyProp == null)
                firstEmptyProp = layerProp;
        }

        if (firstEmptyProp == null)
        {
            UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
            return;
        }

        firstEmptyProp.stringValue = name;
        tagManager.ApplyModifiedProperties();
    }

    void SetupScreenShot()
    {
        GameObject g = PrefabUtility.InstantiatePrefab(bl_MiniMapData.Instance.ScreenShotPrefab, EditorSceneManager.GetActiveScene()) as GameObject;
        g.GetComponent<bl_MiniMapRenderTool>().SetMiniMap((bl_MiniMap)target);
        Selection.activeGameObject = g;
        EditorGUIUtility.PingObject(g);
        g.transform.SetAsLastSibling();
    }

    public static string GetAssetFolderPath()
    {
        string refPath = AssetDatabase.GetAssetPath(bl_MiniMapData.Instance);
        // move two folders up of the reference path
        string folderPath = System.IO.Path.GetDirectoryName(refPath);
        folderPath = System.IO.Path.GetDirectoryName(folderPath);
        return folderPath;
    }
}