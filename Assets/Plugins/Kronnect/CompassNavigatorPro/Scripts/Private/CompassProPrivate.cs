using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CompassNavigatorPro {

    [ExecuteAlways]
    [DefaultExecutionOrder(100)]
    public partial class CompassPro : MonoBehaviour {

        enum CompassPoint {
            CardinalEast = 0,
            OrdinalNorthEast = 1,
            CardinalNorth = 2,
            OrdinalNorthWest = 3,
            CardinalWest = 4,
            OrdinalSouthWest = 5,
            CardinalSouth = 6,
            OrdinalSouthEast = 7
        }

        struct CompassPointPOI {
            public Vector3 position;
            public float cos, sin;
            public Text text;
        }

        readonly int[] cardinals = new int[] { 0, 2, 4, 6 };
        readonly int[] ordinals = new int[] { 1, 3, 5, 7 };

        const string SAMPLE_TITLE_TEXT = "SAMPLE TITLE";
        const int TEXT_POOL_SIZE = 256;
        const string TEXT_POOL_OBJECT_NAME = "CompassProTextPool";

        [NonSerialized]
        public readonly List<CompassProPOI> pois = new List<CompassProPOI>();

        readonly List<CompassProPOI> poisOfInterest = new List<CompassProPOI>();

        [NonSerialized]
        public List<CompassProPOI> activePOIs;

        static readonly List<CompassPro> compasses = new List<CompassPro>();
        static CompassPro _instance;
        float fadeStartTime, prevAlpha;
        CanvasGroup canvasGroup;
        RectTransform compassBackRT;
        Image compassBackImage;
        Text text, textShadow;
        Text title, titleShadow;
        TextMeshProUGUI titleTMP;
        float endTimeOfCurrentTextReveal;
        Vector3 lastCamPos;
        Quaternion lastCamRot, lastFollowRot;
        int lastUpdateFrameCount;
        float lastUpdateTime;
        readonly StringBuilder titleText = new StringBuilder();
        RectTransform titleRT, titleShadowRT, titleTMPRT;
        Vector3 titleRTDefaultPosition, titleShadowRTDefaultPosition;
        Vector3 titleTMPRTDefaultPosition;
        AudioSource audioSource;
        int poiVisibleCount;
        bool autoHiding;
        // performing autohide fade
        float thisAlpha;
        bool needUpdateCompassBarIcons;
        CompassPointPOI[] compassPoints;
        float usedNorthDegrees;
        LetterAnimator[] textPool;
        Vector3 textPoolOriginalLocalPosition, textPoolOriginalShadowLocalPosition;
        int poolIndex;
        Transform canvasTextPool;
        Canvas _canvas;

        // Effective viewport dimensions (accounts for viewportRect in Screen Space Overlay mode)
        float effectiveViewportWidth {
            get {
                float baseWidth = _cameraMain != null ? _cameraMain.pixelWidth : Screen.width;
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    return baseWidth * _viewportRect.width;
                }
                return baseWidth;
            }
        }

        float effectiveViewportHeight {
            get {
                float baseHeight = _cameraMain != null ? _cameraMain.pixelHeight : Screen.height;
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    return baseHeight * _viewportRect.height;
                }
                return baseHeight;
            }
        }

        CompassProPOI lastNearestPOI;
        string lastNearestPOIDistanceText;
        float lastNearestPOIDistance;
        float nearestPOIDistance;
        float nearestPOIAlpha;
        Vector3 currentCamPos;
        Quaternion currentCamRot;
        Matrix4x4 currentVP;
        Vector3 followPos;
        Vector3 lastFollowPos;
        bool needsUpdateSettings;
        Vector3 lastVisitedDistanceFollowPos;
        Vector3 forward;

        #region On-Screen Indicators
        Transform indicatorsRoot;

        #endregion

        #region Curved compass

        Material compassBarMat, curvedMat, defaultUICurvedMatForCardinals, defaultUICurvedMatForText;

        #endregion

        #region Gameloop lifecycle

#if UNITY_EDITOR

        [MenuItem("GameObject/UI/Compass Navigator Pro", false)]
        static void CreateCompassNavigatorPro (MenuCommand menuCommand) {
            // Create a custom game object
            GameObject go = Instantiate(Resources.Load<GameObject>("CNPro/Prefabs/CompassNavigatorPro")) as GameObject;
            go.name = "CompassNavigatorPro";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

#endif
        public void OnEnable () {

            if (_instance == null) _instance = this;

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Sprite iconNonVisited = Resources.Load<Sprite>("CNPro/Sprites/compassIcon");
                if (iconNonVisited != null && iconNonVisited.hideFlags != 0) {
                    iconNonVisited.hideFlags = 0; // attempt to fix an issue with older versions which could mark this file as don't save preventing builds
                    UnityEditor.EditorUtility.SetDirty(iconNonVisited);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
#endif

            activePOIs = pois;
            RequestPOIsOfInterestUpdate();

            if (!compasses.Contains(this)) {
                compasses.Add(this);
            }

            if (_follow == null) {
                if (cameraMain != null) {
                    _follow = _cameraMain.transform;
                }
            }

            if (compassPoints == null || compassPoints.Length == 0) {
                Init();
            }

            // ensure there's an EventSystem gameobject is buttons are visible so they can be used
            // an EventSystem gameobject is automatically created when instantiating a Canvas prefab so here we go
            if (Application.isPlaying && currentMiniMapUsesEvents && Misc.FindObjectOfType<EventSystem>() == null) {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                eventSystem.AddComponent<StandaloneInputModule>();
#endif
            }

            EnableCompass();
            SetupTextPool();
            SetupMiniMap();

            if (dontDestroyOnLoad && Application.isPlaying) {
                if (Misc.FindObjectsOfType(GetType()).Length > 1) {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(this);
                SceneManager.sceneLoaded += UpdateFogOfWarOnLoadScene;
            }

            // locate existing POIs and register them
            CompassProPOI[] scenePOIs = Misc.FindObjectsOfType<CompassProPOI>();
            foreach (CompassProPOI poi in scenePOIs) {
                if (!POIisRegistered(poi)) {
                    poi.RegisterPOI();
                }
            }
        }

        void Start () {
            needsSetupMiniMap = true;
        }

        void OnRectTransformDimensionsChange () {
            needsSetupMiniMap = true;
        }

        void OnDisable () {
            SceneManager.sceneLoaded -= UpdateFogOfWarOnLoadScene;
        }

        private void OnDrawGizmosSelected () {
            if (currentMiniMapContents != MiniMapContents.WorldMappedTexture || !_showMiniMap) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(currentMiniMapWorldCenter, currentMiniMapWorldSize);
        }

        private void OnDestroy () {
            DisableCompass();
            DisableMiniMap();
            DisableIndicators();

            foreach (CompassProPOI poi in pois) {
                if (poi != null) {
                    poi.Release();
                }
            }
            pois.Clear();
            if (curvedMat != null) {
                DestroyImmediate(curvedMat);
            }
            if (compassBarMat != null) {
                DestroyImmediate(compassBarMat);
            }
            if (defaultUICurvedMatForCardinals != null) {
                DestroyImmediate(defaultUICurvedMatForCardinals);
            }
            if (defaultUICurvedMatForText != null) {
                DestroyImmediate(defaultUICurvedMatForText);
            }
            MiniMapDispose();
            if (compasses.Contains(this)) {
                compasses.Remove(this);
            }
        }

        private void OnValidate () {
            _miniMapSize = Mathf.Max(0.001f, _miniMapSize);
            _miniMapIconSize = Mathf.Max(0, _miniMapIconSize);
            _miniMapViewConeDistance = Mathf.Max(_miniMapViewConeDistance, 0);
            _visitedDistance = Mathf.Max(_visitedDistance, 1f);
            _nearDistance = Mathf.Max(10, _nearDistance);
            _miniMapRadarRingsDistance = Mathf.Max(1f, _miniMapRadarRingsDistance);
            _onScreenIndicatorScale = Mathf.Max(0.001f, _onScreenIndicatorScale);
            _visibleMaxDistance = Mathf.Max(_visibleMaxDistance, 0);
            _visibleMinDistance = Mathf.Max(_visibleMinDistance, 0);
            _miniMapCaptureSize = Mathf.Max(_miniMapCaptureSize, 2);
            _miniMapRadarPulseFallOff = Mathf.Max(0, _miniMapRadarPulseFallOff);
            _miniMapRadarPulseFrequency = Mathf.Max(0, _miniMapRadarPulseFrequency);
            _onScreenIndicatorNearFadeDistance = Mathf.Max(0, _onScreenIndicatorNearFadeDistance);
            _onScreenIndicatorNearFadeMin = Mathf.Max(0, _onScreenIndicatorNearFadeMin);
            _onScreenIndicatorNearFadeMin = Mathf.Min(_onScreenIndicatorNearFadeMin, _onScreenIndicatorNearFadeDistance);
            _onScreenIndicatorFarDistance = Mathf.Max(0, _onScreenIndicatorFarDistance);
            _onScreenIndicatorFarFadeDistance = Mathf.Max(0, _onScreenIndicatorFarFadeDistance);
            _onScreenIndicatorFarFadeDistance = Mathf.Min(_onScreenIndicatorFarFadeDistance, _onScreenIndicatorFarDistance);
            _onScreenIndicatorScaleSpeed = Mathf.Max(1f, _onScreenIndicatorScaleSpeed);
            needsUpdateSettings = true;
        }

        void Init () {
#if UNITY_EDITOR
            PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (prefabInstanceStatus != PrefabInstanceStatus.NotAPrefab) {
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
#endif
            Canvas.ForceUpdateCanvases();
            Invoke(nameof(CanvasRefresh), 0);
            InitDelayed();
        }

        Camera FindSuitableCamera () {
            Camera theCam = Camera.main;
            if (theCam == null) {
                Camera[] cams = Misc.FindObjectsOfType<Camera>(true);
                foreach (var cam in cams) {
                    // exclude depth capture cameras
                    if (cam.targetTexture != null && cam.targetTexture.format == RenderTextureFormat.Default) continue;
                    // exclude top down of custom cameras
                    if (cam.GetComponentInParent<CompassPro>(true) != null) continue;
                    theCam = cam;
                    break;
                }
            }
            if (hideIfNoCamera) {
                gameObject.SetActive(theCam != null);
            }
            return theCam;
        }

        void CanvasRefresh () {
            Canvas.ForceUpdateCanvases();
        }

        void InitDelayed () {

            _canvas = GetComponent<Canvas>();
            _canvas.pixelPerfect = false; // improves performance
            pois.Clear();
            audioSource = GetComponent<AudioSource>();
            if (compassIconPrefab == null) {
                compassIconPrefab = Resources.Load<GameObject>("CNPro/Prefabs/CompassBarIcon");
            }
            if (miniMapIconPrefab == null) {
                miniMapIconPrefab = Resources.Load<GameObject>("CNPro/Prefabs/MiniMapIcon");
            }
            if (_titleFont == null) {
                _titleFont = Resources.Load<Font>("CNPro/Fonts/Actor-Regular");
            }

            if (_titleFontTMP == null) {
                _titleFontTMP = Resources.Load<TMP_FontAsset>("CNPro/Fonts/Title Font SDF");
            }

            GameObject compassBack = transform.Find("CompassBack").gameObject;
            compassBackRT = compassBack.GetComponent<RectTransform>();
            compassBackImage = compassBack.GetComponent<Image>();
            canvasGroup = GetMiniMapCanvasGroup(compassBackRT);
            text = compassBackRT.transform.Find("Text").GetComponent<Text>();
            textShadow = compassBackRT.transform.Find("TextShadow").GetComponent<Text>();
            text.text = textShadow.text = "";
            titleRT = compassBackRT.transform.Find("Title").GetComponent<RectTransform>();
            titleRTDefaultPosition = titleRT.position;
            title = titleRT.GetComponent<Text>();
            titleShadowRT = compassBackRT.transform.Find("TitleShadow").GetComponent<RectTransform>();
            titleShadowRTDefaultPosition = titleShadowRT.position;
            titleShadow = titleShadowRT.GetComponent<Text>();
            title.text = titleShadow.text = "";
            titleTMPRT = compassBackRT.transform.Find("TitleTMP").GetComponent<RectTransform>();
            titleTMPRTDefaultPosition = titleTMPRT.position;
            titleTMP = titleTMPRT.GetComponent<TextMeshProUGUI>();
            titleTMP.text = "";
            canvasGroup.alpha = 0;
            prevAlpha = 0f;
            fadeStartTime = Time.time;
            lastNearestPOIDistanceText = "";
            lastNearestPOIDistance = float.MinValue;
            compassPoints = new CompassPointPOI[8];
            compassPoints[(int)CompassPoint.CardinalNorth].text = compassBackRT.Find("CardinalN").GetComponent<Text>();
            compassPoints[(int)CompassPoint.CardinalWest].text = compassBackRT.Find("CardinalW").GetComponent<Text>();
            compassPoints[(int)CompassPoint.CardinalSouth].text = compassBackRT.Find("CardinalS").GetComponent<Text>();
            compassPoints[(int)CompassPoint.CardinalEast].text = compassBackRT.Find("CardinalE").GetComponent<Text>();
            compassPoints[(int)CompassPoint.OrdinalNorthWest].text = compassBackRT.Find("InterCardinalNW").GetComponent<Text>();
            compassPoints[(int)CompassPoint.OrdinalNorthEast].text = compassBackRT.Find("InterCardinalNE").GetComponent<Text>();
            compassPoints[(int)CompassPoint.OrdinalSouthWest].text = compassBackRT.Find("InterCardinalSW").GetComponent<Text>();
            compassPoints[(int)CompassPoint.OrdinalSouthEast].text = compassBackRT.Find("InterCardinalSE").GetComponent<Text>();
            usedNorthDegrees = -1;

            // Destroy rogue icons
            MiniMapIconElements[] miniMapIcons = GetComponentsInChildren<MiniMapIconElements>(true);
            foreach (var icon in miniMapIcons) {
                DestroyImmediate(icon.gameObject);
            }
            CompassPOIElements[] compassIcons = GetComponentsInChildren<CompassPOIElements>(true);
            foreach (var icon in compassIcons) {
                DestroyImmediate(icon.gameObject);
            }
            // Destroy rogue indicators
            var indicatorsRoot = transform.Find(INDICATORS_ROOT_NAME);
            if (indicatorsRoot != null) {
                DestroyImmediate(indicatorsRoot.gameObject);
            }
            // Destroy rogue background renderers
            MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in mrs) {
                DestroyImmediate(mr.gameObject);
            }

            lastUpdateFrameCount = int.MinValue;
            lastUpdateTime = float.MinValue;
            InitIndicators();
            UpdatePOIs();
            ComputeCompassPointsPositions();
            UpdateCompassBarAppearance();
            UpdateHalfWindsAppearance();
            UpdateCompassBarAlpha();
            SetupMiniMap();
            UpdateFogOfWarTexture();
            Refresh();
        }

        void LateUpdate () {

            if (needsUpdateSettings) {
                needsUpdateSettings = false;
                UpdateSettings();
            }

            if (_canvas == null) return;

            UpdatePOIs();

            if (_showCompassBar || _showMiniMap) {
                if (_showCompassBar) {
                    UpdateCompassBarIcons();
                    UpdateCompassBarAlpha();
                }
                if (_showMiniMap) {
                    UpdateFogOfWarPosition();
                    UpdateMiniMap();
                    UpdateMiniMapIcons();
                }
            }

            if ((_showOnScreenIndicators || _showOffScreenIndicators) && Application.isPlaying) {
                UpdateIndicators();
            }

        }


        internal void BubbleEvent<T> (UnityEvent<T> a, T t) {
            if (a != null && t != null)
                a.Invoke(t);
        }

        internal void BubbleEvent<T, Q> (UnityEvent<T, Q> a, T t, Q q) {
            if (t != null && q != null)
                a.Invoke(t, q);
        }


        #endregion

        #region Internal stuff

        public void UpdateSettings () {

#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfAnyPrefab(gameObject)) return;
#endif

            if (_canvas == null) InitDelayed();
            if (!hideIfNoCamera) gameObject.SetActive(true);
            SetupMiniMap();
            UpdateCompassBarAppearance();
            UpdateHalfWindsAppearance();
            UpdateCompassBarAlpha();
            UpdateFogOfWarTexture();
            UpdateTitleAppearance();
            UpdateTextAppearance();
            InitIndicators();
            if (scanProfile != null) {
                scanProfile.ValidateSettings();
            }
            Refresh();
        }


        void EnableCompass () {
            if (compassBackRT != null) {
                compassBackRT.gameObject.SetActive(true);
            }
            needUpdateCompassBarIcons = true;
        }

        void DisableCompass () {
            if (compassBackRT != null) {
                compassBackRT.gameObject.SetActive(false);
            }
        }

        void UpdatePOIs () {

            activePOIs = pois;

            if (_follow != null) {
                followPos = _follow.position;
            }
            if (cameraMain == null) {
                return;
            }

            Transform t = _cameraMain.transform;
            currentCamPos = t.position;
            currentCamRot = t.rotation;

            bool forceUpdate = false;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                if (lastFollowRot != _follow.rotation) {
                    lastFollowRot = _follow.rotation;
                    forceUpdate = true;
                }
            }

            if (lastCamPos != currentCamPos || lastCamRot != currentCamRot || !Application.isPlaying || forceUpdate) {
                lastCamPos = currentCamPos;
                lastCamRot = currentCamRot;
                RequestPOIIconsUpdate();
            } else {
                // If camera has not moved, then don't refresh contents so often
                switch (_updateMode) {
                    case UpdateMode.NumberOfFrames:
                        int frameCount = Time.frameCount;
                        if (frameCount - lastUpdateFrameCount >= _updateIntervalFrameCount) {
                            lastUpdateFrameCount = frameCount;
                            RequestPOIIconsUpdate();
                        }
                        break;
                    case UpdateMode.Time:
                        float now = Time.time;
                        if (now - lastUpdateTime >= _updateIntervalTime) {
                            lastUpdateTime = now;
                            RequestPOIIconsUpdate();
                        }
                        break;
                    case UpdateMode.Continuous:
                        RequestPOIIconsUpdate();
                        break;
                }
            }

            Matrix4x4 P = cameraMain.projectionMatrix;
            Matrix4x4 V;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                V = _follow.worldToLocalMatrix;
            } else {
                V = t.worldToLocalMatrix;
            }
            currentVP = P * V;

            if (enableAreaOfInterest) {
                activePOIs = poisOfInterest;
                float dx = followPos.x - lastFollowPos.x;
                float dz = followPos.z - lastFollowPos.z;
                float distanceSqr = dx * dx + dz * dz;
                if (distanceSqr >= areaOfInterestUpdateDistance * areaOfInterestUpdateDistance) {
                    lastFollowPos = followPos;
                    UpdatePOIsOfInterest();
                }
            }
            CheckPOIListConsistency();

            if (needsIconSorting) {
                needsIconSorting = false;
                activePOIs.Sort(IconPriorityComparer);
            }

            if (!Application.isPlaying) return;

            Vector3 visitedDistDelta = lastVisitedDistanceFollowPos;
            visitedDistDelta.x -= followPos.x;
            visitedDistDelta.z -= followPos.z;
            if (visitedDistDelta.x * visitedDistDelta.x + visitedDistDelta.z * visitedDistDelta.z > 1f) {
                lastVisitedDistanceFollowPos = followPos;
                for (int k = 0; k < activePOIs.Count; k++) {
                    CompassProPOI poi = activePOIs[k];
                    CompassProPOIState state = poi.GetState(_compassGroup);

                    // Check enter/exit circle events
                    if (poi.miniMapShowCircle) {
                        if (state.distanceToFollow < 0.02f) {
                            if (state.insideCircle >= 0) {
                                state.insideCircle = -1;
                                OnPOIEnterCircle.Invoke(poi);
                            }
                        } else {
                            if (state.insideCircle <= 0) {
                                state.insideCircle = 1;
                                OnPOIExitCircle.Invoke(poi);
                            }
                        }
                    }

                    if (poi == null) continue;

                    if (state.isVisited) continue;

                    // Do we visit this POI for the first time?
                    float thisPOIVisitedDistance = poi.visitedDistanceOverride > 0 ? poi.visitedDistanceOverride : _visitedDistance;
                    if (poi.canBeVisited && !state.isVisited && state.distanceToFollow > 0 && state.distanceToFollow < thisPOIVisitedDistance) {
                        state.isVisited = true;
                        state.visitedTime = Time.time;
                        if (poi.hideWhenVisited) {
                            poi.enabled = false;
                        }
                        OnPOIVisited.Invoke(poi);

                        if (poi == null) continue;

                        if (poi.playAudioClipWhenVisited && audioSource != null) {
                            if (poi.visitedAudioClipOverride != null) {
                                audioSource.PlayOneShot(poi.visitedAudioClipOverride);
                            } else if (_visitedDefaultAudioClip != null) {
                                audioSource.PlayOneShot(_visitedDefaultAudioClip);
                            }
                        }
                        ShowPOIDiscoveredText(poi);
                    }

                    // Check heartbeat
                    if (poi.heartbeatEnabled) {
                        bool inHeartbeatRange = state.distanceToFollow < poi.heartbeatDistance;
                        if (!state.heartbeatIsActive && inHeartbeatRange) {
                            StartHeartbeat(poi, state);
                        } else if (state.heartbeatIsActive && !inHeartbeatRange) {
                            StopHeartbeat(poi, state);
                        }

                        if (poi == null) continue;
                    }

                    CheckPOIListConsistency();
                }
            }

        }

        void StartHeartbeat(CompassProPOI poi, CompassProPOIState state) {
            if (state.isVisited) return;
            state.heartbeatPlayer = poi.StartCoroutine(HeartbeatPlayerCoroutine(poi, state));
            state.heartbeatIsActive = true;
        }

        void StopHeartbeat(CompassProPOI poi, CompassProPOIState state) {
            if (state.heartbeatPlayer != null) {
                poi.StopCoroutine(state.heartbeatPlayer);
            }
            state.heartbeatIsActive = false;
        }

        System.Collections.IEnumerator HeartbeatPlayerCoroutine(CompassProPOI poi, CompassProPOIState state) {
            AudioClip heartbeatSound = poi.heartbeatAudioClip != null ? poi.heartbeatAudioClip : heartbeatDefaultAudioClip;
            if (heartbeatSound == null) {
                Debug.LogWarning("Compass POI: heartbeat sound not set.");
                yield break;
            }
            float heartbeatDistance = Mathf.Max(1f, poi.heartbeatDistance);
            float minDistance = _visitedDistance;
            while (true) {
                float distance = state.distanceToFollow;
                if (distance > heartbeatDistance || state.isVisited) {
                    state.heartbeatIsActive = false;
                    yield break;
                }
                if (distance < minDistance) {
                    distance = minDistance;
                }
                Vector3 camPos = _cameraMain.transform.position;
                AudioSource.PlayClipAtPoint(heartbeatSound, camPos);
                if (poi.OnHeartbeat != null) {
                    poi.OnHeartbeat();
                }
                float curvePos = (distance - minDistance) / heartbeatDistance;
                float delay = poi.heartbeatInterval.Evaluate(curvePos);
                yield return new WaitForSeconds(delay);
            }
        }


        void RequestPOIIconsUpdate () {
            needUpdateMiniMapIcons = true;
            needUpdateCompassBarIcons = true;
            needUpdateIndicators = true;
        }

        void RequestPOIsOfInterestUpdate () {
            lastFollowPos.x = float.MaxValue;
        }

        void UpdatePOIsOfInterest () {

            float interestDistanceSqr = areaOfInterestMaxDistance * areaOfInterestMaxDistance;

            poisOfInterest.Clear();

            int poisCount = pois.Count;
            for (int k = 0; k < poisCount; k++) {
                CompassProPOI poi = pois[k];
                if (poi == null) {
                    pois.RemoveAt(k);
                    k--;
                    poisCount--;
                    continue;
                }

                if (poi.ignoreAreaOfInterest) {
                    poisOfInterest.Add(poi);
                    continue;
                }

                // Just compute distance, no need for viewport position yet
                Vector3 poiPosition = poi.transform.position;
                poiPosition.x += poi.positionOffset.x;
                poiPosition.z += poi.positionOffset.z;
                float dx = poiPosition.x - followPos.x;
                float dz = poiPosition.z - followPos.z;
                float distanceSqr = dx * dx + dz * dz;
                distanceSqr -= poi.radius * poi.radius;

                // Check if POI is within area of interest
                if (distanceSqr <= interestDistanceSqr) {
                    poisOfInterest.Add(poi);
                } else {
                    CompassProPOIState state = poi.GetState(_compassGroup);
                    if (state.isVisible) {
                        ToggleCompassBarIconVisibility(poi, state, false);
                    }
                    if (state.miniMapIsVisible) {
                        ToggleMiniMapIconVisibility(poi, state, false);
                    }
                    if (state.indicatorIsVisible) {
                        ToggleIndicatorVisibility(poi, state, false);
                    }
                    if (state.circleIsVisible) {
                        ToggleMiniMapCircleVisibility(poi, state, false);
                    }
                }
            }
        }

        protected virtual bool ToggleMiniMapIconVisibility(CompassProPOI poi, CompassProPOIState state, bool visible) {
            state.miniMapIsVisible = visible;
            if (state.miniMapIconImage != null && state.miniMapIconImage.enabled != visible) {
                state.miniMapIconImage.enabled = visible;
                return true;
            }
            return false;
        }

        protected virtual bool ToggleIndicatorVisibility(CompassProPOI poi, CompassProPOIState state, bool visible) {
            state.indicatorIsVisible = visible;
            if (state.indicatorImage != null && state.indicatorImage.isActiveAndEnabled != visible) {
                state.indicatorImage.gameObject.SetActive(visible);
                return true;
            }
            return false;
        }

        protected virtual bool ToggleMiniMapCircleVisibility(CompassProPOI poi, CompassProPOIState state, bool visible) {
            state.circleIsVisible = visible;
            if (state.miniMapCircleImage != null && state.miniMapCircleImage.isActiveAndEnabled != visible) {
                state.miniMapCircleRT.gameObject.SetActive(visible);
                return true;
            }
            return false;
        }

        void CheckPOIListConsistency () {
            int poisCount = activePOIs.Count;
            for (int k = 0; k < poisCount; k++) {
                CompassProPOI poi = activePOIs[k];
                if (poi == null) {
                    activePOIs.RemoveAt(k);  // POI no longer registered; remove and exit to prevent indexing errors
                    k--;
                    poisCount--;
                    continue;
                }
            }
        }

        protected virtual void ComputePOIViewportPos (CompassProPOI poi) {
            CompassProPOIState state = poi.GetState(_compassGroup);

            // Update POI distance
            Vector3 poiPosition = poi.transform.position;
            poiPosition.x += poi.positionOffset.x;
            poiPosition.y += poi.positionOffset.y;
            poiPosition.z += poi.positionOffset.z;
            float dx = poiPosition.x - followPos.x;
            float dz = poiPosition.z - followPos.z;
            float distance = dx * dx + dz * dz;
            if (_use3Ddistance) {
                float dy = poiPosition.y - followPos.y;
                distance += dy * dy;
            }
            distance = Mathf.Sqrt(distance);
            distance -= poi.radius;
            if (distance <= 0) {
                distance = 0.01f;
            }
            poi.positionWS = poiPosition;
            state.distanceToFollow = distance;

            // Compute viewport position
            Vector3 vpos;
            if (_compassBarOrientation == CompassBarOrientation.Follow) {
                vpos = _cameraMain.WorldToViewportPoint(poi.positionWS);
            } else {
                vpos = GetViewportPos(poiPosition);
            }

            // Remap viewport position to custom viewport rect (for split-screen support in Screen Space Overlay mode)
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                if (_viewportRect.width < 1f || _viewportRect.height < 1f || _viewportRect.x > 0f || _viewportRect.y > 0f) {
                    vpos.x = (vpos.x - _viewportRect.x) / _viewportRect.width;
                    vpos.y = (vpos.y - _viewportRect.y) / _viewportRect.height;
                }
            }
            state.viewportPos = vpos;
        }

        Vector3 GetViewportPos (Vector3 position) {
            if (_cameraMain != null && _cameraMain.orthographic) {
                return _cameraMain.WorldToViewportPoint(position);
            }

            Vector4 r4;
            r4.x = position.x;
            r4.y = position.y;
            r4.z = position.z;
            r4.w = 1f;
            r4 = currentVP * r4;
            if (r4.w > 0) {
                if (r4.w < 0.00001f) r4.w = 0.00001f;
            } else {
                if (r4.w > -0.00001f) r4.w = -0.00001f;
            }

            r4.w = -r4.w;
            r4.x /= r4.w;
            r4.y /= r4.w;

            // clip to view space
            Vector3 viewportPos;
            viewportPos.x = r4.x / 2f + 0.5f;
            viewportPos.y = r4.y / 2f + 0.5f;
            viewportPos.z = r4.w;
            return viewportPos;
        }

        /// <summary>
        /// Update bar icons
        /// </summary>
        protected virtual void UpdateCompassBarIcons () {

            if (!needUpdateCompassBarIcons || _cameraMain == null)
                return;

            needUpdateCompassBarIcons = false;

            float now = Time.time;
            const float visibleDistanceFallOff = 4f;
            float barMax = _width * 0.5f - _endCapsWidth / effectiveViewportWidth;

            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                forward = _follow.forward;
            } else {
                forward = _cameraMain.transform.forward;
            }
            forward.y = 0;

            // Cardinal & Ordinal (ordinal) Points
            ComputeCompassPointsPositions();
            UpdateCardinalPoints(barMax);
            UpdateOrdinalPoints(barMax);
            UpdateHalfWinds(barMax);

            // Update Icons
            poiVisibleCount = 0;

            nearestPOIDistance = float.MaxValue;
            nearestPOI = null;

            Scene currentScene = SceneManager.GetActiveScene();
            int frameCount = Time.frameCount;
            Vector2 canvasRenderingSize = canvas.renderingDisplaySize;
            // Adjust for viewport rect in Screen Space Overlay mode
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                canvasRenderingSize.x *= _viewportRect.width;
                canvasRenderingSize.y *= _viewportRect.height;
            }
            float distanceBendAmount = _bendAmount * -0.5f * canvasRenderingSize.y / canvas.transform.localScale.y;

            int iconsCount = activePOIs.Count;
            for (int p = 0; p < iconsCount; p++) {

                CompassProPOI poi = activePOIs[p];
                CompassProPOIState state = poi.GetState(_compassGroup);

                if (!poi.isActiveAndEnabled) {
                    ToggleCompassBarIconVisibility(poi, state, false);
                    continue;
                }

                // Update POI viewport position and distance
                if (frameCount != state.viewportPosFrameCount) {
                    state.viewportPosFrameCount = frameCount;
                    ComputePOIViewportPos(poi);
                }

                // Change in visibility?
                bool iconVisible = false;
                float distanceFactor = state.distanceToFollow / _nearDistance;
                float alphaFactor = poi.visibility == POIVisibility.AlwaysVisible ? 1f : distanceFactor;
                float computedIconScale = poi.iconScale;
                if (!poi.iconScaleIsFixed) {
                    computedIconScale *= Mathf.Lerp(_maxIconSize, _minIconSize, distanceFactor);
                }
                state.compassCurrentIconScale = computedIconScale;

                // Should we make this POI visible in the compass bar?
                bool prevVisible = state.isVisible;

                float thisPOIVisibleMaxDistance = poi.visibleDistanceOverride > 0 ? poi.visibleDistanceOverride : _visibleMaxDistance;
                float thisPOIVisibleMinDistance = poi.visibleMinDistanceOverride > 0 ? poi.visibleMinDistanceOverride : _visibleMinDistance;
                bool isInRange = state.distanceToFollow >= thisPOIVisibleMinDistance && state.distanceToFollow < thisPOIVisibleMaxDistance;
                state.isVisible = (isInRange && poi.visibility == POIVisibility.WhenInRange) || poi.visibility == POIVisibility.AlwaysVisible;

                // Is it same scene?
                if (state.isVisible && poi.dontDestroyOnLoad && poi.scene != currentScene) {
                    state.isVisible = false;
                }

                // Visited and hide when visited
                if (state.isVisited && poi.hideWhenVisited) {
                    state.isVisible = false;
                }

                if (Application.isPlaying) {
                    // Notify POI visibility change
                    if (prevVisible != state.isVisible) {
                        if (state.isVisible) {
                            OnPOIVisible.Invoke(poi);
                        } else if (!state.isVisible) {
                            OnPOIHide.Invoke(poi);
                        }
                    }
                }

                // If POI is not visible, then hide and skip
                if (!state.isVisible) {
                    ToggleCompassBarIconVisibility(poi, state, false);
                } else {
                    // POI is visible, should we create the icon in the compass bar?
                    if (state.compassIconRT == null) {
                        GameObject iconGO = CreateCompassBarIcon(poi);
                        if (iconGO == null) continue;
                        CompassPOIElements elements = iconGO.GetComponent<CompassPOIElements>();
                        if (elements == null) {
                            Debug.LogError("Compass POI prefab missing Compass POI elements component.");
                            DestroySafe(iconGO);
                            continue;
                        }
                        iconGO.name = "CompassIcon " + poi.gameObject.name;
                        state.compassIconRT = iconGO.GetComponent<RectTransform>();
                        state.compassIconImage = elements.iconImage;
                        state.compassIconImage.material = null;
                        state.compassIconDistanceText = elements.distanceText;
                        if (state.compassIconDistanceText != null) {
                            state.compassIconDistanceTextRT = state.compassIconDistanceText.GetComponent<RectTransform>();
                        }
                        state.curvedMaterialSet = false;
                        state.visibleTime = now;
                        OnCompassBarIconCreated?.Invoke(poi, iconGO);
                    }

                    // Check bending
                    if (state.curvedMaterialSet) {
                        if (_bendAmount == 0 && !_edgeFadeOut) {
                            state.compassIconImage.material = null;
                        }
                    } else if (_bendAmount != 0 || _edgeFadeOut) {
                        state.compassIconImage.material = curvedMat;
                    }

                    // Position the icon on the compass bar
                    Vector3 screenPos = GetScreenPos(poi, state);
                    float posX = screenPos.x;

                    // Always show the focused icon in the compass bar; if out of bar, maintain it on the edge with normal scale
                    if (poi.clampPosition || _focusedPOI == poi) {
                        if (screenPos.z < 0) {
                            posX = barMax * -Mathf.Sign(screenPos.x - 0.5f);
                            if (state.compassCurrentIconScale > 1f) {
                                state.compassCurrentIconScale = 1f;
                            }
                        } else if (posX < -barMax) {
                            posX = -barMax;
                            if (state.compassCurrentIconScale > 1f) {
                                state.compassCurrentIconScale = 1f;
                            }
                        } else if (posX > barMax) {
                            posX = barMax;
                            if (state.compassCurrentIconScale > 1f) {
                                state.compassCurrentIconScale = 1f;
                            }
                        }
                        screenPos.z = 0;
                    }

                    float absPosX = Mathf.Abs(posX);

                    // Set icon position
                    if (absPosX > barMax || screenPos.z < 0) {
                        // Icon outside of bar
                        ToggleCompassBarIconVisibility(poi, state, false);
                    } else {
                        // Unhide icon
                        if (ToggleCompassBarIconVisibility(poi, state, true)) {
                            state.visibleTime = now;
                        }
                        state.compassIconRT.anchorMin = state.compassIconRT.anchorMax = new Vector2(0.5f + posX / _width, 0.5f);
                        iconVisible = true;
                    }

                    // Icon is visible, manage it
                    if (iconVisible) {
                        poiVisibleCount++;

                        // Assign proper icon
                        if (state.isVisited) {
                            if (state.compassIconImage.sprite != poi.iconVisited) {
                                state.compassIconImage.sprite = poi.iconVisited;
                            }
                        } else if (state.compassIconImage.sprite != poi.iconNonVisited) {
                            state.compassIconImage.sprite = poi.iconNonVisited;
                        }

                        // Scale in animation
                        float iconAnimatedAlphaMultiplier = 1f;
                        if (_scaleInDuration > 0) {
                            float t = (now - state.visibleTime) / _scaleInDuration;
                            if (t < 1) {
                                needUpdateCompassBarIcons = true;
                                state.compassCurrentIconScale *= t;
                            } else {
                                t = 1f;
                            }
                            iconAnimatedAlphaMultiplier = t;
                        }

                        // Scale icon
                        Transform iconTransform = state.compassIconImage.transform;
                        if (state.compassCurrentIconScale != iconTransform.localScale.x) {
                            iconTransform.localScale = new Vector3(state.compassCurrentIconScale, state.compassCurrentIconScale, 1f);
                        }

                        // Set icon's color and alpha
                        if (poi.visibility != POIVisibility.AlwaysVisible) {
                            float t = (_visibleMaxDistance - state.distanceToFollow) / visibleDistanceFallOff;
                            t = Mathf.Clamp01(t);
                            iconAnimatedAlphaMultiplier *= t;
                        }

                        Color spriteColor = poi.tintColor;
                        spriteColor.a *= iconAnimatedAlphaMultiplier;
                        state.compassIconImage.color = spriteColor;

                        // Get title if POI is centered
                        if (_focusedPOI == poi || (absPosX < _labelHotZone && state.distanceToFollow < nearestPOIDistance)) {
                            nearestPOI = poi;
                            nearestPOIDistance = state.distanceToFollow;
                            nearestPOIAlpha = iconAnimatedAlphaMultiplier;
                        }

                        if (state.compassIconDistanceText != null) {
                            bool distanceTextVisible = false;
                            if (_showDistance && poi.iconShowDistance && state.distanceToFollow > 0.1f) {
                                distanceTextVisible = true;
                                if (state.lastCompassIconDistance != state.distanceToFollow) {
                                    state.lastCompassIconDistance = state.distanceToFollow;
                                    state.lastCompassIconDistanceText = state.distanceToFollow.ToString(_showDistanceFormat);
                                }
                                state.compassIconDistanceText.text = state.lastCompassIconDistanceText;

                                float absX = Mathf.Abs(screenPos.x);
                                float distToEdge = _width - absX * 2f + 0.001f;
                                float fadeOut = (distToEdge - _edgeFadeOutStart) / (_edgeFadeOutWidth + 0.0001f);
                                if (fadeOut < 0) fadeOut = 0; else if (fadeOut > 1f) fadeOut = 1f;
                                state.compassIconDistanceText.color = new Color(1, 1, 1, iconAnimatedAlphaMultiplier * fadeOut);
                                RectTransform compassIconDistanceTextRT = state.compassIconDistanceTextRT;
                                if (_bendAmount != 0) {
                                    float s = Mathf.Sin(compassIconDistanceTextRT.position.x / canvasRenderingSize.x * Mathf.PI) * distanceBendAmount;
                                    compassIconDistanceTextRT.localPosition = new Vector3(0, s, 0);
                                } else {
                                    compassIconDistanceTextRT.localPosition = Misc.Vector3zero;
                                }
                                compassIconDistanceTextRT.sizeDelta = new Vector2(200, _showDistanceHeight * (1f + Mathf.Max(0f, state.compassCurrentIconScale - 1f) * 0.52f));
                            } else {
                                distanceTextVisible = false;
                            }
                            state.compassIconDistanceText.enabled = distanceTextVisible;
                        }
                    }
                }

                // Update title
#if UNITY_EDITOR
                if (Application.isPlaying) {
#endif
                    CompassProPOIState nearestPOIState = nearestPOI != null ? nearestPOI.GetState(_compassGroup) : null;
                    if (nearestPOIDistance < float.MaxValue && nearestPOI.titleVisibility != TitleVisibility.Never & (nearestPOIState.isVisited || nearestPOI.titleVisibility == TitleVisibility.Always)) {
                        if (nearestPOI != lastNearestPOI) {
                            lastNearestPOIDistance = -1;
                        }
                        if (lastNearestPOIDistance != nearestPOIDistance) {
                            lastNearestPOIDistance = nearestPOIDistance;
                            if (titleText.Length > 0) {
                                titleText.Length = 0;
                            }
                            titleText.Append(nearestPOI.title);

                            float titleMinPOIDistance = nearestPOI.titleMinPOIDistanceOverride > 0 ? nearestPOI.titleMinPOIDistanceOverride : _titleMinPOIDistance;
                            if (lastNearestPOIDistance >= titleMinPOIDistance) {
                                // indicate "above" or "below"
                                bool addedAlt = false;
                                if (nearestPOI.positionWS.y > lastCamPos.y + _sameAltitudeThreshold) {
                                    if (titleText.Length > 0) {
                                        titleText.Append(" ");
                                    }
                                    titleText.Append("(Above");
                                    addedAlt = true;
                                } else if (nearestPOI.positionWS.y < lastCamPos.y - _sameAltitudeThreshold) {
                                    if (titleText.Length > 0)
                                        titleText.Append(" ");
                                    titleText.Append("(Below");
                                    addedAlt = true;
                                }
                                if (_titleShowDistance) {
                                    if (addedAlt) {
                                        titleText.Append(", ");
                                    } else {
                                        if (titleText.Length > 0) {
                                            titleText.Append(" ");
                                        }
                                        titleText.Append("(");
                                    }
                                    titleText.Append(lastNearestPOIDistance.ToString(_titleShowDistanceFormat));
                                    titleText.Append(")");

                                } else if (addedAlt) {
                                    titleText.Append(")");
                                }
                            }

                            int titleTextLength = titleText.Length;
                            int lastNearestPOIDistanceTextLength = lastNearestPOIDistanceText.Length;
                            bool newTitle = titleTextLength != lastNearestPOIDistanceTextLength;
                            if (!newTitle) {
                                for (int c = 0; c < titleTextLength; c++) {
                                    if (titleText[c] != lastNearestPOIDistanceText[c]) {
                                        newTitle = true;
                                        break;
                                    }
                                }
                            }
                            if (newTitle) {
                                lastNearestPOIDistanceText = titleText.ToString();
                                UpdateTitleText(lastNearestPOIDistanceText);
                                UpdateTitleAppearance();
                            }
                        }
                        lastNearestPOI = nearestPOI;
                        UpdateTitleAlpha(nearestPOIAlpha);
                        Vector3 pos = nearestPOIState.compassIconRT.position;
                        if (_bendAmount != 0) {
                            titleRT.position = titleShadowRT.position = new Vector3(pos.x, titleRT.position.y, 0);
                        } else {
                            titleTMPRT.position = new Vector3(pos.x, titleTMPRT.position.y, 0);
                        }
                    } else {
                        if (_bendAmount != 0) {
                            if (title.text.Length > 0) {
                                title.text = titleShadow.text = "";
                                titleRT.position = titleRTDefaultPosition;
                                titleShadowRT.position = titleShadowRTDefaultPosition;
                            }
                        } else {
                            if (titleTMP.text.Length > 0) {
                                titleTMP.text = "";
                                titleTMPRT.position = titleTMPRTDefaultPosition;
                            }
                        }
                        lastNearestPOIDistanceText = "";
                    }
#if UNITY_EDITOR
                }
#endif
            }

        }

        protected virtual GameObject CreateCompassBarIcon(CompassProPOI poi) {
            if (compassIconPrefab == null) {
                Debug.LogError("Compass icon prefab couldn't be loaded. This prefab should be located at CompassNavigatorPro/Resources/CNPro/Prefabs/CompassBarIcon");
                return null;
            }
            return Instantiate(compassIconPrefab, compassBackRT, false);
        }

        protected virtual bool ToggleCompassBarIconVisibility(CompassProPOI poi, CompassProPOIState state, bool visible) {
            state.isVisible = visible;
            if (state.compassIconRT == null) return false;
            GameObject compassIconGO = state.compassIconRT.gameObject;
            bool imageIsVisible = compassIconGO.activeSelf;
            if (imageIsVisible != visible) {
                compassIconGO.SetActive(visible);
                return true;
            }
            return false;
        }

        int IconPriorityComparer (CompassProPOI p1, CompassProPOI p2) {
            if (p1.priority < p2.priority) return -1;
            if (p1.priority > p2.priority) return 1;
            return 0;
        }

        Vector3 GetScreenPos (CompassProPOI poi, CompassProPOIState state) {

            Vector3 screenPos = Misc.Vector3zero;
            Vector3 centerPos = lastCamPos;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                centerPos = followPos;
            }

            switch (_worldMappingMode) {
                case WorldMappingMode.LimitedToBarWidth:
                    screenPos = state.viewportPos;
                    break;
                case WorldMappingMode.Full180Degrees: {
                        Vector3 v2poi = poi.positionWS - centerPos;
                        float angle = (Quaternion.FromToRotation(forward, v2poi).eulerAngles.y + 180f) / 180f;
                        screenPos.x = 0.5f + (angle % 2.0f - 1.0f) * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    }
                    break;
                case WorldMappingMode.Full360Degrees: {
                        Vector3 v2poi = poi.positionWS - centerPos;
                        float angle = (Quaternion.FromToRotation(forward, v2poi).eulerAngles.y + 180f) / 180f;
                        screenPos.x = 0.5f + (angle % 2.0f - 1f) * 0.5f * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    }
                    break;
                default: // WORLD_MAPPING_MODE.CameraFustrum: 
                    screenPos = state.viewportPos;
                    screenPos.x = 0.5f + (screenPos.x - 0.5f) * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    break;
            }
            screenPos.x -= 0.5f;

            return screenPos;
        }


        Vector3 GetScreenPos (Vector3 position) {

            Vector3 screenPos = Misc.Vector3zero;
            Vector3 centerPos = lastCamPos;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                centerPos = followPos;
            }

            switch (_worldMappingMode) {
                case WorldMappingMode.LimitedToBarWidth:
                    screenPos = GetViewportPos(position);
                    break;
                case WorldMappingMode.Full180Degrees: {
                        Vector3 v2poi = position - centerPos;
                        float angle = (Quaternion.FromToRotation(forward, v2poi).eulerAngles.y + 180f) / 180f;
                        screenPos.x = 0.5f + (angle % 2.0f - 1.0f) * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    }
                    break;
                case WorldMappingMode.Full360Degrees: {
                        Vector3 v2poi = position - centerPos;
                        float angle = (Quaternion.FromToRotation(forward, v2poi).eulerAngles.y + 180f) / 180f;
                        screenPos.x = 0.5f + (angle % 2.0f - 1f) * 0.5f * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    }
                    break;
                default: // WORLD_MAPPING_MODE.CameraFustrum: 
                    screenPos = GetViewportPos(position);
                    screenPos.x = 0.5f + (screenPos.x - 0.5f) * (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                    break;
            }
            screenPos.x -= 0.5f;

            return screenPos;
        }

        public Sprite GetCompassBarSprite () {
            if (compassBackImage == null) return null;
            return compassBackImage.sprite;
        }

        protected virtual void ComputeCompassPointsPositions () {

            if (_cameraMain == null || compassPoints == null)
                return;

            int compassPointsLength = compassPoints.Length;
            if (_northDegrees != usedNorthDegrees) {
                usedNorthDegrees = _northDegrees;
                for (int k = 0; k < compassPointsLength; k++) {
                    float angle = (Mathf.PI * 2f * k / compassPointsLength) - _northDegrees * Mathf.Deg2Rad;
                    compassPoints[k].cos = Mathf.Cos(angle);
                    compassPoints[k].sin = Mathf.Sin(angle);
                }
            }
            Vector3 pos;
            Vector3 centerPos = lastCamPos;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                centerPos = followPos;
            }
            for (int k = 0; k < compassPointsLength; k++) {
                pos = centerPos;
                pos.x += compassPoints[k].cos;
                pos.z += compassPoints[k].sin;
                compassPoints[k].position = pos;
            }
        }


        /// <summary>
        /// If showCardinalPoints is enabled, show N, W, S, E across the compass bar
        /// </summary>
        protected virtual void UpdateCardinalPoints (float barMax) {

            float minZ = _worldMappingMode == WorldMappingMode.Full180Degrees || _worldMappingMode == WorldMappingMode.Full360Degrees ? 0 : 0.001f;
            int cardinalsLength = cardinals.Length;
            for (int i = 0; i < cardinalsLength; i++) {
                int k = cardinals[i];
                if (!_showCardinalPoints || _style.HasDegreesOrTicks()) {
                    if (compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = false;
                    }
                    continue;
                }
                Vector3 screenPos = GetScreenPos(compassPoints[k].position);
                float posX = screenPos.x;
                float absPosX = Mathf.Abs(posX);

                // Set icon position
                if (absPosX > barMax || screenPos.z < minZ) {
                    // Icon outside of bar
                    if (compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = false;
                    }
                } else {
                    // Unhide icon
                    if (!compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = true;
                    }
                    RectTransform rt = compassPoints[k].text.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f + posX / _width, 0.5f + _cardinalPointsVerticalOffset / rt.sizeDelta.y);
                    rt.localScale = new Vector3(0.12f * _cardinalScale, 0.12f * _cardinalScale, 1f);
                }
            }
        }


        /// <summary>
        /// If showOrdinalPoints is enabled, show NE, NW, SW, SE across the compass bar
        /// </summary>
        protected virtual void UpdateOrdinalPoints (float barMax) {

            float minZ = _worldMappingMode == WorldMappingMode.Full180Degrees || _worldMappingMode == WorldMappingMode.Full360Degrees ? 0 : 0.001f;
            for (int i = 0; i < ordinals.Length; i++) {
                int k = ordinals[i];
                if (compassPoints[k].text == null) continue;

                if (!_showOrdinalPoints || _style.HasDegreesOrTicks()) {
                    if (compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = false;
                    }
                    continue;
                }

                Vector3 screenPos = GetScreenPos(compassPoints[k].position);
                float posX = screenPos.x;
                float absPosX = Mathf.Abs(posX);

                // Set icon position
                if (absPosX > barMax || screenPos.z < minZ) {
                    // Icon outside of bar
                    if (compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = false;
                    }
                } else {
                    // Unhide icon
                    if (!compassPoints[k].text.enabled) {
                        compassPoints[k].text.enabled = true;
                    }
                    RectTransform rt = compassPoints[k].text.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f + posX / _width, 0.5f + _cardinalPointsVerticalOffset / rt.sizeDelta.y);
                    rt.localScale = new Vector3(0.12f * _ordinalScale, 0.12f * _ordinalScale, 1f);
                }
            }

        }

        /// <summary>
        /// Manages compass bar alpha transitions
        /// </summary>
        protected virtual void UpdateCompassBarAlpha () {

            if (!_showCompassBar) return;

            // Alpha
            if (_alwaysVisibleInEditMode && !Application.isPlaying) {
                thisAlpha = Mathf.Max(0.2f, _alpha);
            } else if (_autoHide) {
                if (!autoHiding) {
                    if (poiVisibleCount == 0) {
                        if (thisAlpha > 0) {
                            autoHiding = true;
                            fadeStartTime = Time.time;
                            prevAlpha = canvasGroup.alpha;
                            thisAlpha = 0;
                        }
                    } else if (poiVisibleCount > 0 && thisAlpha == 0) {
                        thisAlpha = _alpha;
                        autoHiding = true;
                        fadeStartTime = Time.time;
                        prevAlpha = canvasGroup.alpha;
                    }
                }
            } else {
                thisAlpha = _alpha;
            }

            if (_miniMapFullScreenState && _showMiniMap) {
                thisAlpha = 0;
            }

            if (thisAlpha != canvasGroup.alpha) {
                float t = Application.isPlaying ? (Time.time - fadeStartTime) / _fadeDuration : 1.0f;
                canvasGroup.alpha = Mathf.Lerp(prevAlpha, thisAlpha, t);
                if (t >= 1) {
                    prevAlpha = canvasGroup.alpha;
                }
                canvasGroup.gameObject.SetActive(canvasGroup.alpha > 0);
            } else if (autoHiding)
                autoHiding = false;
        }

        protected virtual void UpdateCompassBarAppearance () {

            // Toggle on/off compass bar
            if (compassBackImage.isActiveAndEnabled != _showCompassBar) {
                compassBackRT.gameObject.SetActive(_showCompassBar);
            }

            // Width & Vertical Position
            float anchorMinX = (1 - _width) * 0.5f;
            float anchorMaxX = 1f - anchorMinX;
            float posX = anchorMinX + _horizontalPosition;
            float posY = _verticalPosition;
            float posX2 = anchorMaxX + _horizontalPosition;

            // Apply viewport rect in Screen Space Overlay mode
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                posX = _viewportRect.x + posX * _viewportRect.width;
                posX2 = _viewportRect.x + posX2 * _viewportRect.width;
                posY = _viewportRect.y + posY * _viewportRect.height;
            }

            compassBackRT.anchorMin = new Vector2(posX, posY);
            compassBackRT.anchorMax = new Vector2(posX2, posY);
            compassBackRT.sizeDelta = new Vector2(compassBackRT.sizeDelta.x, 25f * _height);

            // Style
            if (compassBarMat == null) {
                compassBarMat = Instantiate(Resources.Load<Material>("CNPro/Materials/CompassBar"));
            }
            compassBarMat.DisableKeyword(ShaderParams.SKW_SCROLLABLE);
            compassBarMat.DisableKeyword(ShaderParams.SKW_SCROLLABLE_180);
            compassBarMat.DisableKeyword(ShaderParams.SKW_SCROLLABLE_360);
            Sprite barSprite;
            switch (_style) {
                case CompassStyle.Rounded:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar2");
                    break;
                case CompassStyle.Celtic_White:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar3-White");
                    break;
                case CompassStyle.Celtic_Black:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar3-Black");
                    break;
                case CompassStyle.Fantasy1:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar4");
                    break;
                case CompassStyle.Fantasy2:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar5");
                    break;
                case CompassStyle.Fantasy3:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar8");
                    break;
                case CompassStyle.Fantasy4:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar9");
                    break;
                case CompassStyle.SciFi1:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar6");
                    break;
                case CompassStyle.SciFi2:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar7");
                    break;
                case CompassStyle.SciFi3:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar10");
                    break;
                case CompassStyle.SciFi4:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar11");
                    break;
                case CompassStyle.SciFi5:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar12");
                    break;
                case CompassStyle.SciFi6:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar13");
                    break;
                case CompassStyle.Clean:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar14");
                    break;
                case CompassStyle.CleanWithIntegratedDegrees:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar15");
                    worldMappingMode = WorldMappingMode.Full180Degrees;
                    SetCompassScrollableProperties(0.5f);
                    break;
                case CompassStyle.CleanWithIntegratedDegreesAndTicks1:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar16");
                    worldMappingMode = WorldMappingMode.Full180Degrees;
                    SetCompassScrollableProperties(0.5f);
                    break;
                case CompassStyle.CleanWithIntegratedDegreesAndTicks2:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar17");
                    worldMappingMode = WorldMappingMode.Full180Degrees;
                    SetCompassScrollableProperties(0.5f);
                    break;
                case CompassStyle.CleanWithIntegratedDegreesAndTicks3:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar18");
                    worldMappingMode = WorldMappingMode.Full180Degrees;
                    SetCompassScrollableProperties(0.5f);
                    break;
                case CompassStyle.CleanWithIntegratedDegreesAndTicks4:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar19");
                    worldMappingMode = WorldMappingMode.Full180Degrees;
                    SetCompassScrollableProperties(0.5f);
                    break;
                case CompassStyle.Custom:
                    barSprite = _compassBackSprite;
                    if (_compassBackSpriteScrollable) {
                        SetCompassScrollableProperties(_compassBackSpriteScrollOffset);
                    }
                    break;
                default:
                    barSprite = Resources.Load<Sprite>("CNPro/Sprites/Bar1");
                    break;
            }
            if (barSprite != null && compassBackImage.sprite != barSprite) {
                compassBackImage.sprite = barSprite;
            }

            ToggleCurvedCompass();

            if (cameraMain != null) {
                float w = (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
                float hPos = _horizontalPosition + 0.5f;
                float halfWidth = _width * 0.5f - _endCapsWidth / effectiveViewportWidth;

                // Adjust for viewport rect in Screen Space Overlay mode
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    w *= _viewportRect.width;
                    hPos = _viewportRect.x + hPos * _viewportRect.width;
                    halfWidth *= _viewportRect.width;
                }

                Vector4 compassData = new Vector4(w, hPos, halfWidth);
                if (_worldMappingMode == WorldMappingMode.LimitedToBarWidth) {
                    compassData.x = 1f;
                }
                if (compassBarMat != null) {
                    compassBarMat.SetVector(ShaderParams.CompassData, compassData);
                }
                if (defaultUICurvedMatForCardinals != null) {
                    defaultUICurvedMatForCardinals.SetVector(ShaderParams.CompassData, compassData);
                }
                if (defaultUICurvedMatForText != null) {
                    defaultUICurvedMatForText.SetVector(ShaderParams.CompassData, compassData);
                }
                if (curvedMat != null) {
                    curvedMat.SetVector(ShaderParams.CompassData, compassData);
                }
            }
        }


        void SetCompassScrollableProperties (float offset) {
            switch (_worldMappingMode) {
                case WorldMappingMode.Full360Degrees:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_SCROLLABLE_360);
                    break;
                case WorldMappingMode.Full180Degrees:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_SCROLLABLE_180);
                    break;
                default:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_SCROLLABLE);
                    break;
            }
            compassBarMat.SetFloat(ShaderParams.ScrollOffset, offset);
        }


        /// <summary>
        /// If showHalfWinds is enabled, show NNE, ENE, ESE, SSE, SSW, WSW, WNW, NNW marks
        /// </summary>
        void UpdateHalfWinds (float barMax) {
            if (compassBarMat == null) return;
            float angle;
            if (_compassBarOrientation == CompassBarOrientation.Follow && _follow != null) {
                angle = _follow.eulerAngles.y;
            } else {
                angle = currentCamRot.eulerAngles.y;
            }
            compassBarMat.SetFloat(ShaderParams.CompassAngle, angle - _northDegrees);
            if (_worldMappingMode == WorldMappingMode.LimitedToBarWidth || _worldMappingMode == WorldMappingMode.CameraFrustum) {
                compassBarMat.SetMatrix(ShaderParams.CompassIP, _cameraMain.projectionMatrix.inverse);
            }
        }

        void UpdateHalfWindsAppearance () {
            compassBarMat.DisableKeyword(ShaderParams.SKW_TICKS);
            compassBarMat.DisableKeyword(ShaderParams.SKW_TICKS_180);
            compassBarMat.DisableKeyword(ShaderParams.SKW_TICKS_360);
            if (!_showHalfWinds || cameraMain == null || _style.HasDegreesOrTicks()) return;

            float w = (_width - _endCapsWidth / effectiveViewportWidth) * 0.9f;
            float hPos = _horizontalPosition + 0.5f;
            float halfWidth = _width * 0.5f - _endCapsWidth / effectiveViewportWidth;

            // Adjust for viewport rect in Screen Space Overlay mode
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                w *= _viewportRect.width;
                hPos = _viewportRect.x + hPos * _viewportRect.width;
                halfWidth *= _viewportRect.width;
            }

            Vector4 compassData = new Vector4(w, hPos, halfWidth);

            switch (_worldMappingMode) {
                case WorldMappingMode.LimitedToBarWidth:
                    compassData.x = 1f;
                    compassBarMat.EnableKeyword(ShaderParams.SKW_TICKS);
                    break;
                case WorldMappingMode.CameraFrustum:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_TICKS);
                    break;
                case WorldMappingMode.Full180Degrees:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_TICKS_180);
                    break;
                case WorldMappingMode.Full360Degrees:
                    compassBarMat.EnableKeyword(ShaderParams.SKW_TICKS_360);
                    break;
            }
            compassBarMat.SetVector(ShaderParams.CompassData, compassData);
            compassBarMat.SetColor(ShaderParams.TicksColor, _halfWindsTintColor);
            compassBarMat.SetVector(ShaderParams.TicksSize, new Vector4(_halfWindsWidth, _halfWindsHeight, _halfWindsInterval, 0));
        }

        public void UpdateTextAppearanceEditMode () {
            if (!gameObject.activeInHierarchy)
                return;
            text.gameObject.SetActive(_textRevealEnabled);
            textShadow.gameObject.SetActive(_textRevealEnabled);
            text.text = textShadow.text = "SAMPLE TEXT";
            UpdateTextAlpha(1);
            UpdateTextAppearance();
        }

        void UpdateTextAppearance () {
            // Vertical and horizontal position
            text.alignment = TextAnchor.MiddleCenter;
            Vector3 localScale = new Vector3(_textScale, _textScale, 1f);
            RectTransform rt = text.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = new Vector3(0, _textVerticalPosition, 0);
            text.transform.localScale = localScale;
            text.font = _textFont;

            textShadow.enabled = _textShadowEnabled;
            textShadow.alignment = TextAnchor.MiddleCenter;
            rt = textShadow.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = new Vector3(1f, _textVerticalPosition - 1f, 0);
            textShadow.transform.localScale = localScale;
            textShadow.font = _textFont;
        }

        void UpdateTextAlpha (float t) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, t);
            textShadow.color = new Color(0, 0, 0, t);
        }

        public void UpdateTitleAppearanceEditMode () {
            if (!gameObject.activeInHierarchy || Application.isPlaying)
                return;
            UpdateTitleText(SAMPLE_TITLE_TEXT);
            UpdateTitleAlpha(1);
            UpdateTitleAppearance();
        }

        void UpdateTitleText (string text) {
            if (_bendAmount != 0) {
                title.text = titleShadow.text = text;
            } else {
                titleTMP.text = text;
            }
        }

        void UpdateTitleAppearance () {
            // Vertical and horizontal position
            if (_bendAmount != 0) {
                titleRT.anchoredPosition3D = new Vector3(0, _titleVerticalPosition, 0);
                Vector3 localScale = new Vector3(_titleScale, _titleScale, 1f);
                titleRT.localScale = localScale;
                title.font = _titleFont;
                titleShadow.enabled = _titleShadowEnabled;
                titleShadowRT.anchoredPosition3D = new Vector3(1f, _titleVerticalPosition - 1f, 0);
                titleShadow.transform.localScale = localScale;
                titleShadow.font = _titleFont;
                titleRT.gameObject.SetActive(true);
                titleShadowRT.gameObject.SetActive(true);
                titleTMPRT.gameObject.SetActive(false);
            } else {
                titleTMPRT.anchoredPosition3D = new Vector3(0, _titleVerticalPosition, 0);
                Vector3 localScale = new Vector3(_titleScale, _titleScale, 1f);
                titleTMPRT.localScale = localScale;
                titleTMP.font = _titleFontTMP;
                titleRT.gameObject.SetActive(false);
                titleShadowRT.gameObject.SetActive(false);
                titleTMPRT.gameObject.SetActive(true);
            }

        }

        void UpdateTitleAlpha (float t) {
            if (_bendAmount != 0) {
                if (title.color.a != t) {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, t);
                    titleShadow.color = new Color(0, 0, 0, t);
                }
            } else {
                if (titleTMP.color.a != t) {
                    titleTMP.color = new Color(titleTMP.color.r, titleTMP.color.g, titleTMP.color.b, t);
                }
            }
        }

        void SetupTextPool () {
            if (!Application.isPlaying)
                return;

            text.text = textShadow.text = "";
            UpdateTextAppearance();
            if (textPool == null || textPool.Length != TEXT_POOL_SIZE) {
                textPool = new LetterAnimator[TEXT_POOL_SIZE];
            }

            GameObject o = GameObject.Find(TEXT_POOL_OBJECT_NAME);
            if (o == null) {
                o = new GameObject(TEXT_POOL_OBJECT_NAME);
            }
            canvasTextPool = o.transform;

            for (int k = 0; k < TEXT_POOL_SIZE; k++) {
                GameObject letterShadow = Instantiate(textShadow.gameObject, canvasTextPool, false);
                letterShadow.name = "TextShadowPool";
                Text lts = letterShadow.GetComponent<Text>();

                GameObject letter = Instantiate(text.gameObject, canvasTextPool, false);
                letter.name = "TextPool";
                Text lt = letter.GetComponent<Text>();

                LetterAnimator animator = lts.gameObject.AddComponent<LetterAnimator>();
                animator.poolIndex = k;
                animator.text = lt;
                animator.textShadow = lts;
                animator.OnAnimationEnds += PushTextToPool;
                animator.used = false;
                textPool[k] = animator;

                textPoolOriginalLocalPosition = letter.transform.localPosition;
                textPoolOriginalShadowLocalPosition = letterShadow.transform.localPosition;
            }
        }

        void FetchTextFromPool (out Text lt, out Text lts) {
            for (int k = 0; k < TEXT_POOL_SIZE; k++) {
                ++poolIndex;
                if (poolIndex >= TEXT_POOL_SIZE) {
                    poolIndex = 0;
                }
                if (!textPool[poolIndex].used) {
                    break;
                }
            }
            // Setup shadow
            lts = textPool[poolIndex].textShadow;
            Transform t = lts.transform;
            t.SetParent(compassBackRT, false);
            SetCurvedTextMaterial(lts);
            // Setup text
            lt = textPool[poolIndex].text;
            t = lt.transform;
            t.SetParent(compassBackRT, false);
            SetCurvedTextMaterial(lt);
            textPool[poolIndex].used = true;
        }

        void PushTextToPool (int index) {
            Transform t = textPool[index].text.transform;
            t.SetParent(canvasTextPool);
            t.localPosition = textPoolOriginalLocalPosition;
            t = textPool[index].textShadow.transform;
            t.SetParent(canvasTextPool);
            t.localPosition = textPoolOriginalShadowLocalPosition;
            textPool[index].used = false;
        }


        protected virtual void ShowPOIDiscoveredText (CompassProPOI poi) {
            if (!_textRevealEnabled || !_showCompassBar || string.IsNullOrEmpty(poi.visitedText))
                return;
            StartCoroutine(AnimateDiscoverText(poi.visitedText));
        }

        IEnumerator AnimateDiscoverText (string discoverText) {

            int len = discoverText.Length;
            if (_cameraMain == null || textPool == null || textPool.Length != TEXT_POOL_SIZE)
                yield break;

            while (Time.time < endTimeOfCurrentTextReveal) {
                yield return Misc.WaitForOneSecond;
            }

            float now = Time.time;
            endTimeOfCurrentTextReveal = now + _textRevealDuration + _textDuration + _textFadeOutDuration * 0.5f;

            text.text = textShadow.text = "";
            UpdateTextAppearance();

            // initial pos of text
            TextGenerationSettings settings = text.GetGenerationSettings(Misc.Vector2zero);
            settings.scaleFactor = 1f;
            TextGenerator textGenerator = text.cachedTextGenerator;

            float spreadFactor = _textScale * _textLetterSpacing;
            string discoverTextSpread = discoverText.Replace(" ", "A");
            float posX = textGenerator.GetPreferredWidth(discoverTextSpread, settings) * -0.5f * spreadFactor;
            float sizeOfSpace = textGenerator.GetPreferredWidth("A", settings) * spreadFactor;

            float acum = 0;
            for (int k = 0; k < len; k++) {
                Text lts, lt;
                string ch = discoverText.Substring(k, 1);
                FetchTextFromPool(out lt, out lts);
                lts.text = ch;
                lt.text = ch;

                float letw;
                if (" ".Equals(ch)) {
                    letw = sizeOfSpace;
                } else {
                    letw = textGenerator.GetPreferredWidth(ch, settings) * spreadFactor;
                }

                RectTransform letterRT = lt.GetComponent<RectTransform>();
                letterRT.anchoredPosition3D = new Vector3(posX + acum + letw * 0.5f, letterRT.anchoredPosition3D.y, 0);
                RectTransform shadowRT = lts.GetComponent<RectTransform>();
                shadowRT.anchoredPosition3D = new Vector3(posX + acum + letw * 0.5f + 1f, shadowRT.anchoredPosition3D.y, 0);

                acum += letw;

                // Trigger animator
                LetterAnimator anim = textPool[poolIndex];
                anim.startTime = now + k * _textRevealLetterDelay;
                anim.revealDuration = _textRevealDuration;
                anim.startFadeTime = now + _textRevealDuration + _textDuration;
                anim.fadeDuration = _textFadeOutDuration;
                anim.enabled = true;
                anim.Play();
            }
        }

        #endregion

        #region Curved compass

        void ToggleCurvedCompass () {

            if (compassBackRT == null) {
                return;
            }

            CompassBarMeshModifier m = compassBackImage.GetComponent<CompassBarMeshModifier>();
            if (m == null) {
                compassBackImage.gameObject.AddComponent<CompassBarMeshModifier>();
            }

            // In Screen Space Overlay mode with viewport rect, scale width for shader calculations
            float shaderWidth = _width;
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                shaderWidth *= _viewportRect.width;
            }

            Vector4 fxData = new Vector4(_bendAmount, shaderWidth, _edgeFadeOutWidth, _edgeFadeOutStart);
            if (!_edgeFadeOut) {
                fxData.z = fxData.w = 0;
            }

            compassBarMat.SetVector(ShaderParams.FXData, fxData);
            compassBarMat.SetColor(ShaderParams.TintColor, _compassTintColor);
            compassBackImage.material = compassBarMat;

            if (curvedMat == null) {
                curvedMat = Instantiate(Resources.Load<Material>("CNPro/Materials/SpriteCurved"));
                curvedMat.DisableKeyword(ShaderParams.SKW_TICKS);
                curvedMat.DisableKeyword(ShaderParams.SKW_TICKS_180);
                curvedMat.DisableKeyword(ShaderParams.SKW_TICKS_360);
            }
            if (defaultUICurvedMatForCardinals == null) {
                defaultUICurvedMatForCardinals = Instantiate(Resources.Load<Material>("CNPro/Materials/UIDefaultCurved"));
                defaultUICurvedMatForText = Instantiate(defaultUICurvedMatForCardinals);
            }

            curvedMat.SetVector(ShaderParams.FXData, fxData);
            defaultUICurvedMatForCardinals.SetVector(ShaderParams.FXData, fxData);

            // cancel edge fade out for texts
            fxData.z = fxData.w = 0;
            defaultUICurvedMatForText.SetVector(ShaderParams.FXData, fxData);

            // Images
            Image[] ii = compassBackRT.GetComponentsInChildren<Image>(true);
            for (int k = 0; k < ii.Length; k++) {
                if (ii[k] != compassBackImage) {
                    ii[k].material = curvedMat;
                }
            }

            // Adjust texts
            Text[] tt = compassBackRT.GetComponentsInChildren<Text>(true);
            for (int k = 0; k < tt.Length; k++) {
                if (tt[k].name.Contains("Cardinal")) {
                    tt[k].material = defaultUICurvedMatForCardinals;
                } else {
                    tt[k].material = defaultUICurvedMatForText;
                }
            }

            // Adjust ticks
            RawImage[] im = compassBackRT.GetComponentsInChildren<RawImage>(true);
            for (int k = 0; k < im.Length; k++) {
                im[k].material = defaultUICurvedMatForCardinals;
            }

        }

        void SetCurvedTextMaterial (Text text) {
            if (_bendAmount != 0) {
                text.material = defaultUICurvedMatForText;
            } else {
                text.material = null;
            }
        }

        #endregion



    }

}



