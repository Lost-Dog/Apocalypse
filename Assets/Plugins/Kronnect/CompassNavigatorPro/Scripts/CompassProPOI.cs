using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CompassNavigatorPro {

    public enum POIMiniMapType {
        Any,
        MiniMapOnly = 1,
        RadarOnly = 2
    }

    public enum POIVisibility {
        WhenInRange = 0,
        AlwaysVisible = 1,
        AlwaysHidden = 2
    }

    public enum TitleVisibility {
        OnlyWhenVisited = 0,
        Always = 1,
        Never = 2
    }

    public delegate void OnHeartbeatEvent();

    /// <summary>
    /// Per-compass state for a POI. Each compass instance has its own state object for each POI.
    /// </summary>
    public class CompassProPOIState {
        // Compass bar
        public RectTransform compassIconRT;
        public Image compassIconImage;
        public TextMeshProUGUI compassIconDistanceText;
        public RectTransform compassIconDistanceTextRT;
        public bool curvedMaterialSet;
        public float compassCurrentIconScale;
        public float visibleTime;
        public bool isVisible;
        public float lastCompassIconDistance;
        public string lastCompassIconDistanceText;

        // Mini-map
        public RectTransform miniMapIconRT;
        public RectTransform miniMapIconImageRT;
        public Image miniMapIconImage;
        public Image miniMapCircleImage;
        public Material miniMapCircleMaterial;
        public RectTransform miniMapCircleRT;
        public bool miniMapIsVisible;
        public float miniMapCurrentIconScale;
        public float circleScale = 2f;
        public float lastCircleRadius, lastCircleHeight, lastCircleZoomLevel;
        public float circleVisibleTime;
        public bool circleIsVisible;
        public int insideCircle;

        // Indicators
        public RectTransform indicatorRT;
        public Image indicatorImage;
        public CanvasGroup indicatorCanvasGroup;
        public RectTransform indicatorArrowRT;
        public TextMeshProUGUI indicatorDistanceText;
        public TextMeshProUGUI indicatorTitleText;
        public bool indicatorIsVisible;
        public int isOnScreen;
        public float prevIndicatorDistance;
        public string lastIndicatorDistanceText;
        public Vector3 lastIndicatorViewportPos;

        // Per-compass computed values
        public float distanceToFollow;
        public Vector3 viewportPos;
        public int viewportPosFrameCount;

        // Per-compass visited state (each player discovers independently)
        public bool isVisited;
        public float visitedTime;
        public bool heartbeatIsActive;
        public Coroutine heartbeatPlayer;

        public void Release() {
            if (compassIconRT != null) {
                Misc.DestroySafe(compassIconRT.gameObject);
                compassIconRT = null;
            }
            if (miniMapIconRT != null) {
                Misc.DestroySafe(miniMapIconRT.gameObject);
                miniMapIconRT = null;
            }
            if (indicatorRT != null) {
                Misc.DestroySafe(indicatorRT.gameObject);
                indicatorRT = null;
            }
            if (miniMapCircleMaterial != null) {
                Misc.DestroySafe(miniMapCircleMaterial);
                miniMapCircleMaterial = null;
            }
        }
    }

    [HelpURL("https://kronnect.com/docs/compass/")]
    [AddComponentMenu("Compass Navigator Pro/Compass POI")]
    [ExecuteAlways]
    [DefaultExecutionOrder(101)]
    public partial class CompassProPOI : MonoBehaviour {

        const string sceneGizmoIconPath = "CNPro/Sprites/compassIcon";
#if UNITY_EDITOR
        const float sceneGizmoIconMinPixelSize = 16f;
        const float sceneGizmoIconMinWorldHalfSize = 0.05f;
        const float sceneGizmoIconWorldScaleFactor = 0.35f;
        static Texture2D sceneGizmoIconTexture;
#endif

        [Tooltip("Unique ID to be used when DontDestroyOnLoad option is set to true.")]
        public int id;

        [Tooltip("Higher priority icons are rendered on top of others. This option only has effect for POIs loaded at the start. POIs loaded in runtime are added to the hierarchy and will render on top of the previous ones.")]
        public int priority;

        [Tooltip("Bitmask specifying which compass groups (1-4) will display this POI. Default: all groups (0xF).")]
        [SerializeField]
        int _compassGroupMask = 0xF;

        public int compassGroupMask {
            get { return _compassGroupMask; }
            set {
                if (value != _compassGroupMask) {
                    _compassGroupMask = value;
                }
            }
        }

        /// <summary>
        /// Returns true if this POI should register with the specified compass based on group mask.
        /// </summary>
        public bool MatchesCompassGroup(int compassGroup) {
            return (_compassGroupMask & (1 << (compassGroup - 1))) != 0;
        }

        [Tooltip("POI visibility in compass bar.")]
        public POIVisibility visibility = POIVisibility.WhenInRange;

        [Tooltip("If enabled, the POI will be always managed ignoring the current area of interest (if area of interest option is enabled in the compass inspector).")]
        public bool ignoreAreaOfInterest;

        [Tooltip("If enabled, the icon will stop at the edges of the bar even if it's behind the player.")]
        public bool clampPosition;

        [Tooltip("A value of 0 uses the global visible distance property from the Compass Bar settings. A value greater than 0 will override the global value for this POI. Useful when you need this POI to use a different visible distance.")]
        public float visibleDistanceOverride;

        [Tooltip("A value of 0 uses the global visible min distance property from the Compass Bar settings. A value greater than 0 will override the global value for this POI.")]
        public float visibleMinDistanceOverride;

        [Tooltip("A value of 0 uses the global min distance text from the Compass Bar settings. Distance text won't be shown for objects within this distance.")]
        public float titleMinPOIDistanceOverride;

        [Tooltip("Title to be shown when this POI is in the center of the compass bar and it's a known location (isVisited = true)")]
        public string title;

        [Tooltip("Rule for title visibility.")]
        public TitleVisibility titleVisibility = TitleVisibility.OnlyWhenVisited;

        [Tooltip("Specifies if this POI can be marked as visited when reached.")]
        public bool canBeVisited = true;

        [Tooltip("A value of 0 uses the global visited distance property from the Compass Bar settings. A value greater than 0 will override the global value for this POI. Useful when you need this POI to use a different visited distance.")]
        public float visitedDistanceOverride;

        [Tooltip("Specifies if POI must be removed from the UI when visited.")]
        public bool hideWhenVisited;

        [Tooltip("Specifies if this POI has been already visited. In multi-compass setups, this represents the initial visited state.")]
        [SerializeField]
        bool _isVisitedInitial;

        /// <summary>
        /// Gets or sets the visited state for this POI (backward compat - accesses compass group 1 state).
        /// For multi-compass setups, use IsVisitedBy(compass) and SetVisitedBy(compass, value).
        /// </summary>
        public bool isVisited {
            get => GetState(1).isVisited;
            set => GetState(1).isVisited = value;
        }

        /// <summary>
        /// Returns true if this POI has been visited by the specified compass instance.
        /// </summary>
        public bool IsVisitedBy(CompassPro compass) {
            if (compass == null) return false;
            return GetState(compass.compassGroup).isVisited;
        }

        /// <summary>
        /// Sets the visited state for this POI for the specified compass instance.
        /// </summary>
        public void SetVisitedBy(CompassPro compass, bool visited) {
            if (compass == null) return;
            GetState(compass.compassGroup).isVisited = visited;
        }

        [Tooltip("Text to show when discovered. Leave this to null if you don't want to show any text.")]
        public string visitedText;

        public bool playAudioClipWhenVisited = true;

        [Tooltip("Sound to play when POI is visited the first time. If nothing set, the default audio-clip assigned in Compass Navigator Pro inspector will be used.")]
        public AudioClip visitedAudioClipOverride;

        [Tooltip("Radius of interest of this POI. Useful for area POIs, like rooms, cities or generic areas of search. The radius is used by the circle feature in the mini-map or to determine if player reaches an area.")]
        public float radius;

        [Tooltip("User defined icon scale multiplier.")]
        public float iconScale = 1f;

        [Tooltip("When true, the icon scale won't be altered based on distance.")]
        public bool iconScaleIsFixed;

        [Tooltip("Show distance to the POI in the compass bar under the icon")]
        public bool iconShowDistance = true;

        [Tooltip("The icon for the POI if has not been discovered/visited.")]
        public Sprite iconNonVisited;

        [Tooltip("The icon for the POI if has been visited.")]
        public Sprite iconVisited;

        [Tooltip("Tinting color")]
        public Color tintColor = Color.white;

        [Tooltip("If the icon will be shown in the scene during playmode. If enabled, the indicator will fade in smoothly as the player approaches it.")]
        public bool showOnScreenIndicator = true;

        [Tooltip("Scale for this POI indicator")]
        public float onScreenIndicatorScale = 1f;

        [Tooltip("Show distance to camera for this POI indicator")]
        public bool onScreenIndicatorShowDistance = true;

        [Tooltip("Show title for this POI indicator")]
        public bool onScreenIndicatorShowTitle = true;

        [Tooltip("Optionally assign a custom prefab for this POI on-screen indicator. If null, the system will use the indicator prefab set in the Compass Navigator Pro component.")]
        public GameObject onScreenIndicatorPrefabOverride;

        [Tooltip("Optional offset added to the POI position to compute the distance or the screen coordinate of the visual indicators.")]
        public Vector3 positionOffset;

        [Tooltip("Show the POI icon as a gizmo in the Scene view.")]
        public bool showSceneGizmo = true;

        [Tooltip("If the icon will be shown around the edges of screen in the scene during playmode when it's not visible in the screen.")]
        public bool showOffScreenIndicator = true;

        [Tooltip("Show distance to camera for this POI when it's off-screen")]
        public bool offScreenIndicatorShowDistance = true;

        [Tooltip("Custom separation between offscreen indicator and screen edges. 0 = use global setting.")]
        public float offScreenIndicatorMarginOverride;

        [Tooltip("Distance at which the on-screen indicator will start to fade when it approaches camera")]
        public float onScreenIndicatorNearFadeDistance;

        [Tooltip("Minimum distance at which the on-screen indicator disappear")]
        public float onScreenIndicatorNearFadeMin;

        [Tooltip("Distance at which the indicator will not be visible")]
        public float onScreenIndicatorFarDistance;

        [Tooltip("Distance at which the on-screen indicator will start to fade out before reaching max visible distance")]
        public float onScreenIndicatorFarFadeDistance;

        [Tooltip("Sound to play when beacon is shown.")]
        public AudioClip beaconAudioClip;

        [Tooltip("Sound to play when scan effect hits this POI.")]
        public AudioClip scanHitAudioClip;

        [Tooltip("Preserves the state of this POI between scene changes. Note that this POI only will be visible in the scene where it was first created.")]
        public bool dontDestroyOnLoad;

        [Tooltip("Enables heartbeat effect. Plays a sound with variable speed when approaching this POI.")]
        public bool heartbeatEnabled;

        [Tooltip("Sound to play when heartbeat effect is enabled.")]
        public AudioClip heartbeatAudioClip;

        [Tooltip("Distance to start playing heartbeat effect is enabled.")]
        public float heartbeatDistance = 20f;

        [Tooltip("Interval of heartbeat rate based on distance.")]
        public AnimationCurve heartbeatInterval = AnimationCurve.Linear(0, 0.25f, 1f, 3f);

        public POIMiniMapType miniMapType = POIMiniMapType.Any;

        [Tooltip("POI visibility on the mini-map.")]
        public POIVisibility miniMapVisibility = POIVisibility.WhenInRange;

        [Tooltip("Optionally assign a custom prefab for this POI icon on the mini-map. If null, the system will use the icon prefab set in the Compass Navigator Pro component.")]
        public GameObject miniMapIconPrefabOverride;

        [Tooltip("A value of 0 uses the global visible distance property from the mini-map settings. A value greater than 0 will override the global value for this POI. Useful when you need this POI to use a different visible distance.")]
        public float miniMapVisibleDistanceOverride;

        [Tooltip("If enabled, the icon will stop at the edges of the mini-map even if it's behind the player.")]
        public bool miniMapClampPosition;

        [Tooltip("Scale multiplier applied to POI when clamped to border")]
        public float miniMapClampedScaleMultiplier = 1.1f;

        [Tooltip("If enabled, the minimap icon will be rotated according to the POI rotation.")]
        public bool miniMapShowRotation;

        [Tooltip("Custom angle rotation adjustment.")]
        public float miniMapRotationAngleOffset;

        [Tooltip("Icon scale on the mini-map.")]
        public float miniMapIconScale = 1f;

        [Tooltip("Add a circle around the POI in the mini-map illustrating the POI radius")]
        public bool miniMapShowCircle;

        [Tooltip("Radius of the circle to be shown on the minimap. Set it to 0 to use the general radius value.")]
        public float miniMapCircleRadius;

        public Color miniMapCircleColor = new Color(0, 1, 0, 0.5f);

        public Color miniMapCircleInnerColor = new Color(0, 0, 1, 0);

        [Range(0, 1)]
        public float miniMapCircleStartRadius = 0.25f;

        [Tooltip("Add a circle animation when the icon appears in the mini-map")]
        public bool miniMapCircleAnimationWhenAppears = true;

        [Tooltip("Number of repetitions for the circle animation")]
        public int miniMapCircleAnimationRepetitions = 5;

        public OnHeartbeatEvent OnHeartbeat;

        #region State variables

        public const int MAX_COMPASS_GROUPS = 4;

        public Scene scene;

        [NonSerialized]
        public Vector3 positionWS;

        /// <summary>
        /// Per-compass state array. Access via GetState(compassGroup).
        /// </summary>
        [NonSerialized]
        public CompassProPOIState[] states;

        /// <summary>
        /// Gets the state object for the specified compass group. Lazily initializes if needed.
        /// </summary>
        public CompassProPOIState GetState(int compassGroup) {
            if (states == null) states = new CompassProPOIState[MAX_COMPASS_GROUPS];
            int idx = compassGroup - 1;
            if (states[idx] == null) states[idx] = new CompassProPOIState();
            return states[idx];
        }

        /// <summary>
        /// Reference to the compass script (backward compat - returns first compass)
        /// </summary>
        [NonSerialized]
        public CompassPro compass;

        // Backward compatibility properties - access state for compass group 1
        public float distanceToFollow { 
            get => GetState(1).distanceToFollow; 
            set => GetState(1).distanceToFollow = value; 
        }

        public Vector3 viewportPos { 
            get => GetState(1).viewportPos; 
            set => GetState(1).viewportPos = value; 
        }

        public Vector3 lastIndicatorViewportPos { 
            get => GetState(1).lastIndicatorViewportPos; 
            set => GetState(1).lastIndicatorViewportPos = value; 
        }

        public int viewportPosFrameCount { 
            get => GetState(1).viewportPosFrameCount; 
            set => GetState(1).viewportPosFrameCount = value; 
        }

        public float visitedTime { 
            get => GetState(1).visitedTime; 
            set => GetState(1).visitedTime = value; 
        }

        // Compass bar backward compat
        public bool isVisible { 
            get => GetState(1).isVisible; 
            set => GetState(1).isVisible = value; 
        }

        public RectTransform compassIconRT { 
            get => GetState(1).compassIconRT; 
            set => GetState(1).compassIconRT = value; 
        }

        public Image compassIconImage { 
            get => GetState(1).compassIconImage; 
            set => GetState(1).compassIconImage = value; 
        }

        public bool curvedMaterialSet { 
            get => GetState(1).curvedMaterialSet; 
            set => GetState(1).curvedMaterialSet = value; 
        }

        public float compassCurrentIconScale { 
            get => GetState(1).compassCurrentIconScale; 
            set => GetState(1).compassCurrentIconScale = value; 
        }

        // Mini-map backward compat
        public bool miniMapIsVisible { 
            get => GetState(1).miniMapIsVisible; 
            set => GetState(1).miniMapIsVisible = value; 
        }

        public RectTransform miniMapIconRT { 
            get => GetState(1).miniMapIconRT; 
            set => GetState(1).miniMapIconRT = value; 
        }

        public RectTransform miniMapIconImageRT { 
            get => GetState(1).miniMapIconImageRT; 
            set => GetState(1).miniMapIconImageRT = value; 
        }

        public Image miniMapIconImage { 
            get => GetState(1).miniMapIconImage; 
            set => GetState(1).miniMapIconImage = value; 
        }

        public Image miniMapCircleImage { 
            get => GetState(1).miniMapCircleImage; 
            set => GetState(1).miniMapCircleImage = value; 
        }

        public Material miniMapCircleMaterial { 
            get => GetState(1).miniMapCircleMaterial; 
            set => GetState(1).miniMapCircleMaterial = value; 
        }

        public RectTransform miniMapCircleRT { 
            get => GetState(1).miniMapCircleRT; 
            set => GetState(1).miniMapCircleRT = value; 
        }

        public bool circleIsVisible { 
            get => GetState(1).circleIsVisible; 
            set => GetState(1).circleIsVisible = value; 
        }

        public float circleScale { 
            get => GetState(1).circleScale; 
            set => GetState(1).circleScale = value; 
        }

        public float lastCircleRadius { 
            get => GetState(1).lastCircleRadius; 
            set => GetState(1).lastCircleRadius = value; 
        }

        public float lastCircleHeight { 
            get => GetState(1).lastCircleHeight; 
            set => GetState(1).lastCircleHeight = value; 
        }

        public float lastCircleZoomLevel { 
            get => GetState(1).lastCircleZoomLevel; 
            set => GetState(1).lastCircleZoomLevel = value; 
        }

        public float circleVisibleTime { 
            get => GetState(1).circleVisibleTime; 
            set => GetState(1).circleVisibleTime = value; 
        }

        public int insideCircle { 
            get => GetState(1).insideCircle; 
            set => GetState(1).insideCircle = value; 
        }

        // Indicator backward compat
        public int isOnScreen { 
            get => GetState(1).isOnScreen; 
            set => GetState(1).isOnScreen = value; 
        }

        public bool indicatorIsVisible { 
            get => GetState(1).indicatorIsVisible; 
            set => GetState(1).indicatorIsVisible = value; 
        }

        public RectTransform indicatorRT { 
            get => GetState(1).indicatorRT; 
            set => GetState(1).indicatorRT = value; 
        }

        public Image indicatorImage { 
            get => GetState(1).indicatorImage; 
            set => GetState(1).indicatorImage = value; 
        }

        public CanvasGroup indicatorCanvasGroup { 
            get => GetState(1).indicatorCanvasGroup; 
            set => GetState(1).indicatorCanvasGroup = value; 
        }

        public RectTransform indicatorArrowRT { 
            get => GetState(1).indicatorArrowRT; 
            set => GetState(1).indicatorArrowRT = value; 
        }

        public TextMeshProUGUI indicatorDistanceText { 
            get => GetState(1).indicatorDistanceText; 
            set => GetState(1).indicatorDistanceText = value; 
        }

        public TextMeshProUGUI indicatorTitleText { 
            get => GetState(1).indicatorTitleText; 
            set => GetState(1).indicatorTitleText = value; 
        }

        public float prevIndicatorDistance { 
            get => GetState(1).prevIndicatorDistance; 
            set => GetState(1).prevIndicatorDistance = value; 
        }

        public float lastCompassIconDistance { 
            get => GetState(1).lastCompassIconDistance; 
            set => GetState(1).lastCompassIconDistance = value; 
        }

        public string lastIndicatorDistanceText { 
            get => GetState(1).lastIndicatorDistanceText; 
            set => GetState(1).lastIndicatorDistanceText = value; 
        }

        public string lastCompassIconDistanceText { 
            get => GetState(1).lastCompassIconDistanceText; 
            set => GetState(1).lastCompassIconDistanceText = value; 
        }

        /// <summary>
        /// Reference to the icon gameobject on the compass bar when it's created (backward compat - returns group 1)
        /// </summary>
        public GameObject compassIconGameObject {
            get {
                var rt = GetState(1).compassIconRT;
                return rt != null ? rt.gameObject : null;
            }
        }

        /// <summary>
        /// Reference to the icon gameobject on the minimap when it's created (backward compat - returns group 1)
        /// </summary>
        public GameObject miniMapIconGameObject {
            get {
                var rt = GetState(1).miniMapIconRT;
                return rt != null ? rt.gameObject : null;
            }
        }

        /// <summary>
        /// Reference to the indicator gameobject when it's created (backward compat - returns group 1)
        /// </summary>
        public GameObject indicatorGameObject {
            get {
                var rt = GetState(1).indicatorRT;
                return rt != null ? rt.gameObject : null;
            }
        }

        public TextMeshProUGUI compassIconDistanceText { 
            get => GetState(1).compassIconDistanceText; 
            set => GetState(1).compassIconDistanceText = value; 
        }

        public RectTransform compassIconDistanceTextRT { 
            get => GetState(1).compassIconDistanceTextRT; 
            set => GetState(1).compassIconDistanceTextRT = value; 
        }

        /// <summary>
        /// Time when the poi appeared on the compass bar (backward compat - returns group 1)
        /// </summary>
        public float visibleTime { 
            get => GetState(1).visibleTime; 
            set => GetState(1).visibleTime = value; 
        }

        public bool heartbeatIsActive { 
            get => GetState(1).heartbeatIsActive; 
            set => GetState(1).heartbeatIsActive = value; 
        }

        Coroutine heartbeatPlayer { 
            get => GetState(1).heartbeatPlayer; 
            set => GetState(1).heartbeatPlayer = value; 
        }

        [HideInInspector]
        public float miniMapCurrentIconScale { 
            get => GetState(1).miniMapCurrentIconScale; 
            set => GetState(1).miniMapCurrentIconScale = value; 
        }

        #endregion

        private void OnValidate() {
            radius = MathF.Max(0, radius);
            iconScale = Mathf.Max(0, iconScale);
            miniMapCircleRadius = Mathf.Max(0, miniMapCircleRadius);
            miniMapCircleAnimationRepetitions = Mathf.Max(1, miniMapCircleAnimationRepetitions);
            onScreenIndicatorNearFadeMin = Mathf.Max(0, onScreenIndicatorNearFadeMin);
            onScreenIndicatorNearFadeDistance = Mathf.Max(0, onScreenIndicatorNearFadeDistance);
            onScreenIndicatorNearFadeMin = Mathf.Min(onScreenIndicatorNearFadeMin, onScreenIndicatorNearFadeDistance);
            onScreenIndicatorFarDistance = Mathf.Max(0, onScreenIndicatorFarDistance);
            onScreenIndicatorFarFadeDistance = Mathf.Max(0, onScreenIndicatorFarFadeDistance);
            onScreenIndicatorFarFadeDistance = Mathf.Min(onScreenIndicatorFarFadeDistance, onScreenIndicatorFarDistance);
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (!showSceneGizmo || !enabled || !gameObject.activeInHierarchy) return;
            Camera sceneCamera = Camera.current;
            if (sceneCamera == null || sceneCamera.cameraType == CameraType.Preview) return;
            if (!UnityEditor.Handles.ShouldRenderGizmos()) return;
            EnsureSceneGizmoIconTexture();
            if (sceneGizmoIconTexture == null) return;

            Vector3 worldPosition = transform.position + positionOffset;
            Vector3 viewportPosition = sceneCamera.WorldToViewportPoint(worldPosition);
            if (viewportPosition.z <= 0) return;

            Vector3 lossyScale = transform.lossyScale;
            float maxScale = Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
            float worldHalfSize = Mathf.Max(sceneGizmoIconMinWorldHalfSize, maxScale * sceneGizmoIconWorldScaleFactor);

            Vector3 screenCenter3 = sceneCamera.WorldToScreenPoint(worldPosition);
            Vector3 screenRight3 = sceneCamera.WorldToScreenPoint(worldPosition + sceneCamera.transform.right * worldHalfSize);
            Vector2 screenCenter = new Vector2(screenCenter3.x, screenCenter3.y);
            Vector2 screenRight = new Vector2(screenRight3.x, screenRight3.y);
            float halfSizePixels = Mathf.Max(sceneGizmoIconMinPixelSize * 0.5f, Vector2.Distance(screenCenter, screenRight));

            Rect rect = new Rect(
                screenCenter.x - halfSizePixels,
                sceneCamera.pixelHeight - screenCenter.y - halfSizePixels,
                halfSizePixels * 2f,
                halfSizePixels * 2f);

            UnityEditor.Handles.BeginGUI();
            GUI.DrawTexture(rect, sceneGizmoIconTexture, ScaleMode.ScaleToFit, true);
            UnityEditor.Handles.EndGUI();
        }
        
        static void EnsureSceneGizmoIconTexture() {
            if (sceneGizmoIconTexture != null) return;
            sceneGizmoIconTexture = Resources.Load<Texture2D>(sceneGizmoIconPath);
        }
#endif

        void OnEnable() {
            if (iconNonVisited == null) {
                iconNonVisited = Resources.Load<Sprite>("CNPro/Sprites/compassIcon");
            }
            #if UNITY_EDITOR
            if (iconNonVisited != null && iconNonVisited.hideFlags != 0) {
                iconNonVisited.hideFlags = 0; // attempt to fix an issue with older versions which could mark this file as don't save preventing builds
            }
            #endif
            #if UNITY_EDITOR
            if (gameObject.scene.rootCount == 0 || UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return; // Skip if this is a prefab in the project or in prefab edit mode
            #endif

            // Initialize per-compass states with the initial visited state from the inspector
            if (_isVisitedInitial) {
                for (int i = 1; i <= MAX_COMPASS_GROUPS; i++) {
                    GetState(i).isVisited = true;
                }
            }

            if (id == 0) {
                GenerateNewId();
            } else if (!dontDestroyOnLoad) { 
                // Check for duplications of this object
                // If there's another POI with same id in edit time, generate a new id
                CompassPro compass = CompassPro.instance;
                if (compass != null) {
                    int poiCount = compass.pois.Count;
                    for (int k = 0; k < poiCount; k++) {
                        CompassProPOI poi = compass.pois[k];
                        if (poi != null && poi != this && poi.id == id) {
                            GenerateNewId();
                            k = -1;
                        }
                    }
                }
            }
            RegisterPOI();
        }

        private void OnDisable() {
            UnRegisterPOI();
        }

        void OnDestroy() {
            Release();
            UnRegisterPOI();
        }

        public void GenerateNewId() {
            id = Guid.NewGuid().GetHashCode();
        }

        public void Release() {
            if (states != null) {
                for (int i = 0; i < MAX_COMPASS_GROUPS; i++) {
                    if (states[i] != null) {
                        states[i].Release();
                    }
                }
            }
        }

        /// <summary>
        /// Releases state for a specific compass group.
        /// </summary>
        public void ReleaseForCompass(int compassGroup) {
            if (states != null) {
                int idx = compassGroup - 1;
                if (idx >= 0 && idx < MAX_COMPASS_GROUPS && states[idx] != null) {
                    states[idx].Release();
                }
            }
        }

        public void RegisterPOI() {
            CompassPro compass = CompassPro.instance;
            if (compass == null)
                return;

            if (dontDestroyOnLoad && Application.isPlaying) {
                DontDestroyOnLoad(gameObject);
            }

            compass.POIRegister(this);
        }

        public void UnRegisterPOI() {
            CompassPro compass = CompassPro.instance;
            if (compass != null) {
                compass.POIUnregister(this);
            }
        }

        public void StartHeartbeat() {
            if (isVisited)
                return;
            heartbeatPlayer = StartCoroutine(HeartBeatPlayer());
            heartbeatIsActive = true;
        }


        public void StopHeartbeat() {
            if (heartbeatPlayer != null) {
                StopCoroutine(heartbeatPlayer);
            }
            heartbeatIsActive = false;
        }


        public void StartCircleAnimation() {
            if (!miniMapCircleAnimationWhenAppears) miniMapCircleAnimationWhenAppears = true;
            circleVisibleTime = 0;
        }

        IEnumerator HeartBeatPlayer() {
            AudioClip heartbeatSound = heartbeatAudioClip != null ? heartbeatAudioClip : CompassPro.instance.heartbeatDefaultAudioClip;
            if (heartbeatSound == null) {
                Debug.LogWarning("Compass POI: heartbeat sound not set.");
                yield break;
            }
            heartbeatDistance = Mathf.Max(1f, heartbeatDistance);
            float minDistance = CompassPro.instance.visitedDistance;
            while (true) {
                float distance = distanceToFollow;
                if (distanceToFollow > heartbeatDistance || isVisited) {
                    heartbeatIsActive = false;
                    yield break;
                }
                if (distance < minDistance) {
                    distance = minDistance;
                }
                Vector3 camPos = CompassPro.instance.cameraMain.transform.position;
                AudioSource.PlayClipAtPoint(heartbeatSound, camPos);
                if (OnHeartbeat != null) {
                    OnHeartbeat();
                }
                float curvePos = (distance - minDistance) / heartbeatDistance;
                float delay = heartbeatInterval.Evaluate(curvePos);
                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// Gets the screen rectangle of the icon in the compass bar
        /// </summary>
        /// <returns>The compass bar icon screen rect.</returns>
        public Rect GetCompassIconScreenRect() {
            if (isVisible && compassIconRT != null && compass != null) {
                Vector3 pos = compassIconRT.transform.position;
                Vector3 size = compassIconRT.sizeDelta;
                return new Rect(pos.x - size.x * 0.5f, Screen.height - pos.y - size.y * 0.5f, size.x, size.y);
            }
            return new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the screen rectangle of the mini-map icon
        /// </summary>
        /// <returns>The mini map icon screen rect.</returns>
        public Rect GetMiniMapIconScreenRect() {
            if (miniMapIsVisible && miniMapIconRT != null && compass != null) {
                return miniMapIconRT.GetScreenRect();
            }
            return new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Used internally. To show/hide the icon in the compass bar use the visibility property.
        /// </summary>
        public bool ToggleCompassBarIconVisibility(bool visible) {
            this.isVisible = visible;
            if (compassIconRT == null) return false;
            GameObject compassIconGO = compassIconRT.gameObject;
            bool imageIsVisible = compassIconGO.activeSelf;
            if (imageIsVisible != visible) {
                compassIconGO.SetActive(visible);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used internally. To show/hide the icon in the mini-map use the miniMapVisibility property.
        /// </summary>
        public bool ToggleMiniMapIconVisibility(bool visible) {
            this.miniMapIsVisible = visible;
            if (miniMapIconImage != null && miniMapIconImage.enabled != visible) {
                miniMapIconImage.enabled = visible;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used internally. To show/hide the indicator use the showOnScreenIndicator property.
        /// </summary>
        public bool ToggleIndicatorVisibility(bool visible) {
            this.indicatorIsVisible = visible;
            if (indicatorImage != null && indicatorImage.isActiveAndEnabled != visible) {
                indicatorImage.gameObject.SetActive(visible);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used internally. To show/hide the circle in the mini-map use the miniMapShowCircle property.
        /// </summary>
        public bool ToggleMiniMapCircleVisibility(bool visible) {
            this.circleIsVisible = visible;
            if (miniMapCircleImage != null && miniMapCircleImage.isActiveAndEnabled != visible) {
                miniMapCircleRT.gameObject.SetActive(visible);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the visited state of this POI and triggers all appropriate actions
        /// </summary>
        /// <param name="visited">True to mark as visited (triggers events/audio/text), false to mark as unvisited</param>
        public void SetVisited(bool visited) {
            if (CompassPro.instance != null) {
                CompassPro.instance.POISetVisited(this, visited);
            }
        }

    }

}