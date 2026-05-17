using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CompassNavigatorPro {

    public partial class CompassPro : MonoBehaviour {

        #region Mini-map

        [NonSerialized]
        public Camera miniMapCamera;

        bool needUpdateMiniMapIcons;
        Transform miniMapMaskUI, miniMapButtonsPanel;
        RectTransform miniMapUI;
        RectTransform miniMapUIRootRT;
        Transform playerIcon, miniMapCardinalsRT;
        RectTransform playerIconRT;
        Image playerIconImage, miniMapCardinalsImage;
        RenderTexture miniMapTex;
        CanvasGroup miniMapCanvasGroup;
        Material miniMapOverlayMat;
        Vector2 miniMapAnchorMin, miniMapAnchorMax, miniMapPivot, miniMapSizeDelta;
        float miniMapCameraAspect;
        float miniMapLastSnapshotTime;
        Vector3 miniMapLastSnapshotLocation;
        Vector3 miniMapLastSnapshotCenter;
        float miniMapLastSnapshotAltitude;
        float miniMapLastSnapshotFoV;
        float miniMapLastSnapshotRotationY;
        Quaternion miniMapLastSnapshotRotation;
        Vector3 miniMapLastBoundsCheckPos;
        int needMiniMapShot;
        Image miniMapImage, miniMapMaskImage;
        Quaternion miniMapFullScreenFixedCameraRotation;
        Vector3 miniMapFullScreenFixedCameraPosition;
        Quaternion miniMapFullScreenFixedFollowRotation;
        Vector3 miniMapFullScreenFixedFollowPosition;
        bool needsSetupMiniMap;
        bool needsIconSorting;
        float miniMapRegularZoomLevel;
        float lastViewConeCameraAspect, lastViewConeFoV;
        Vector4 viewConeData;
        TextMeshProUGUI ringsDistanceText;
        float lastRadarInfoDistance;
        Vector3 miniMapCenter;

        public bool currentMiniMapAllowsUserDrag => _miniMapFullScreenState ? _miniMapFullScreenAllowUserDrag : _miniMapAllowUserDrag;
        float currentMiniMapClampBorder => _miniMapFullScreenState ? _miniMapFullScreenClampBorder : _miniMapClampBorder;
        bool currentMiniMapIsCircular => _miniMapFullScreenState ? (_miniMapFullScreenClampBorderCircular || _miniMapFullScreenContents == MiniMapContents.Radar) : (_miniMapClampBorderCircular || _miniMapContents == MiniMapContents.Radar);
        bool currentMiniMapUsesEvents => _miniMapShowZoomInOutButtons || _miniMapShowMaximizeButton || _miniMapIconEvents || currentMiniMapAllowsUserDrag;
        MiniMapContents currentMiniMapContents => _miniMapFullScreenState ? _miniMapFullScreenContents : _miniMapContents;
        float currentMiniMapZoomLevel => _miniMapFullScreenState ? _miniMapFullScreenZoomLevel : _miniMapZoomLevel;
        Vector3 currentMiniMapWorldCenter => _miniMapFullScreenState ? _miniMapFullScreenWorldCenter : _miniMapWorldCenter;
        Vector3 currentMiniMapWorldSize => _miniMapFullScreenState ? _miniMapFullScreenWorldSize : _miniMapWorldSize;


        void MiniMapDispose () {
            MiniMapReleaseRenderTexture();
            if (miniMapOverlayMat != null) {
                DestroyImmediate(miniMapOverlayMat);
            }
        }

        void SetupMiniMap (bool force = false) {

            if (_canvas == null) return;

            ResetDragOffset();

            if (_miniMapFullScreenState && !force) {
                MiniMapZoomToggle(true);
                return;
            }

            if (miniMapUIRootRT == null) {
                Transform t = transform.Find("MiniMap Root");
                if (t != null) {
                    miniMapUIRootRT = t.GetComponent<RectTransform>();
                }
            }
            if (miniMapUIRootRT != null) {
                miniMapUIRootRT.gameObject.SetActive(_showMiniMap);
            }

            HideMiniMapIcons();

            if (!_showMiniMap) return;

            if (miniMapUIRootRT == null) {
                Debug.LogError("Mini Map element not found in the hierarchy and could not be intialized.");
                _showMiniMap = false;
                return;
            }

            lastRadarInfoDistance = 0;

            Transform miniMapTransform = miniMapUIRootRT.Find("MiniMap");

            if (miniMapUI == null) {
                miniMapUI = miniMapTransform.GetComponent<RectTransform>();
            }
            if (miniMapUI != null) {
                MiniMapInteraction miniMapInteraction = miniMapUI.GetComponent<MiniMapInteraction>();
                if (miniMapInteraction == null) {
                    miniMapInteraction = miniMapUI.gameObject.AddComponent<MiniMapInteraction>();
                }
                miniMapInteraction.compass = this;
            }

            if (miniMapMaskUI == null) {
                miniMapMaskUI = miniMapUI.Find("MiniMapMask");
            }

            if (miniMapButtonsPanel == null) {
                miniMapButtonsPanel = miniMapUIRootRT.Find("Buttons");
            }

            if (miniMapButtonsPanel != null) {
                miniMapButtonsPanel.transform.localScale = new Vector3(_miniMapButtonsScale, _miniMapButtonsScale, 1f);

                // check buttons
                ToggleButtonEventHandler("ZoomIn", () => {
                    MiniMapZoomIn();
                    EventSystem.current.SetSelectedGameObject(null);
                }, continuous: true, isVisible: _miniMapShowZoomInOutButtons);
                ToggleButtonEventHandler("ZoomOut", () => {
                    MiniMapZoomOut();
                    EventSystem.current.SetSelectedGameObject(null);
                }, continuous: true, isVisible: _miniMapShowZoomInOutButtons);
                ToggleButtonEventHandler("ToggleFull", () => {
                    miniMapFullScreenState = !miniMapFullScreenState;
                    EventSystem.current.SetSelectedGameObject(null);
                }, continuous: false, isVisible: _miniMapShowMaximizeButton);
            }

            if (miniMapCamera == null) {
                miniMapCamera = transform.GetComponentInChildren<Camera>(true);
            }
            if (miniMapCamera != null) {
                miniMapCamera.enabled = false;

                if (CNP2URPCameraSetup.usesURP) {
                    CNP2URPCameraSetup.SetupURPCamera(miniMapCamera, _miniMapEnableShadows);
                } else if (CNP2HDRPCameraSetup.usesHDRP) {
                    CNP2HDRPCameraSetup.SetupHDRPCamera(miniMapCamera, _miniMapEnableShadows, _miniMapBackgroundColor);
                }
            }

            if (_miniMapPositionAndSize == MiniMapPositionAndScaleMode.ControlledByCompassNavigatorPro) {
                // set mini-map position
                float anchorX = 0, anchorY = 0;
                float pivotX = 0, pivotY = 0;
                switch (_miniMapLocation) {
                    case MiniMapPosition.TopLeft:
                        anchorX = 0; anchorY = 1; pivotX = 0; pivotY = 1;
                        break;
                    case MiniMapPosition.TopCenter:
                        anchorX = 0.5f; anchorY = 1; pivotX = 0.5f; pivotY = 1;
                        break;
                    case MiniMapPosition.TopRight:
                        anchorX = 1; anchorY = 1; pivotX = 1; pivotY = 1;
                        break;
                    case MiniMapPosition.MiddleLeft:
                        anchorX = 0; anchorY = 0.5f; pivotX = 0; pivotY = 0.5f;
                        break;
                    case MiniMapPosition.MiddleCenter:
                        anchorX = 0.5f; anchorY = 0.5f; pivotX = 0.5f; pivotY = 0.5f;
                        break;
                    case MiniMapPosition.MiddleRight:
                        anchorX = 1; anchorY = 0.5f; pivotX = 1; pivotY = 0.5f;
                        break;
                    case MiniMapPosition.BottomLeft:
                        anchorX = 0; anchorY = 0; pivotX = 0; pivotY = 0;
                        break;
                    case MiniMapPosition.BottomCenter:
                        anchorX = 0.5f; anchorY = 0; pivotX = 0.5f; pivotY = 0;
                        break;
                    case MiniMapPosition.BottomRight:
                        anchorX = 1; anchorY = 0; pivotX = 1; pivotY = 0;
                        break;
                }

                // Apply viewport rect in Screen Space Overlay mode
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    anchorX = _viewportRect.x + anchorX * _viewportRect.width;
                    anchorY = _viewportRect.y + anchorY * _viewportRect.height;
                }

                miniMapUIRootRT.anchorMin = new Vector2(anchorX, anchorY);
                miniMapUIRootRT.anchorMax = new Vector2(anchorX, anchorY);
                miniMapUIRootRT.pivot = new Vector2(pivotX, pivotY);
                miniMapUIRootRT.anchoredPosition = _miniMapScreenPositionOffset;
            }

            // set mini-map size
            if (_miniMapPositionAndSize == MiniMapPositionAndScaleMode.ControlledByCompassNavigatorPro) {
                float screenSize = effectiveViewportHeight * _miniMapSize;
                miniMapUIRootRT.sizeDelta = new Vector2(screenSize / _canvas.scaleFactor, screenSize / _canvas.scaleFactor);
            }

            if (miniMapOverlayMat == null) {
                miniMapOverlayMat = Instantiate(Resources.Load<Material>("CNPro/Materials/MiniMapOverlayUnlit"));
            }

            // set minimap viewer properties
            miniMapOverlayMat.DisableKeyword(ShaderParams.SKW_COMPASS_ROTATE_BORDER);
            Texture2D borderTexture;
            Sprite maskSprite;
            MiniMapContents contents = currentMiniMapContents;
            if (contents == MiniMapContents.Radar) {
                if (_miniMapRadarGraphicsMethod == MiniMapRadarGraphicsMethod.Texture) {
                    if (_miniMapFullScreenState) {
                        borderTexture = _miniMapBorderTextureFullScreenMode;
                        maskSprite = _miniMapMaskSpriteFullScreenMode;
                    } else {
                        borderTexture = _miniMapBorderTexture;
                        maskSprite = _miniMapMaskSprite;
                    }
                    miniMapOverlayMat.EnableKeyword(ShaderParams.SKW_COMPASS_ROTATE_BORDER);
                } else {
                    borderTexture = null;
                    maskSprite = null;
                }
            } else {
                switch (_miniMapFullScreenState ? _miniMapFullScreenStyle : _miniMapStyle) {
                    case MiniMapStyle.TornPaper:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapMask");
                        break;
                    case MiniMapStyle.SolidBox:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorderSolidBox");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapMaskSolidBox");
                        break;
                    case MiniMapStyle.SolidCircle:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorderSolidCircle");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapMaskSolidCircle");
                        break;
                    case MiniMapStyle.Fantasy1:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy1");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy1_Mask");
                        break;
                    case MiniMapStyle.Fantasy2:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy2");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy2_Mask");
                        break;
                    case MiniMapStyle.Fantasy3:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy3");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy3_Mask");
                        break;
                    case MiniMapStyle.Fantasy4:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy4");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy4_Mask");
                        break;
                    case MiniMapStyle.Fantasy5:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy5");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy5_Mask");
                        break;
                    case MiniMapStyle.Fantasy6:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_Fantasy6");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_Fantasy6_Mask");
                        break;
                    case MiniMapStyle.SciFi1:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_SciFi1");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_SciFi1_Mask");
                        break;
                    case MiniMapStyle.SciFi2:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_SciFi2");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_SciFi2_Mask");
                        break;
                    case MiniMapStyle.SciFi3:
                        borderTexture = Resources.Load<Texture2D>("CNPro/Textures/MiniMapBorder_SciFi3");
                        maskSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapBorder_SciFi3_Mask");
                        break;
                    case MiniMapStyle.None:
                        borderTexture = null;
                        maskSprite = null;
                        break;
                    default:
                        if (_miniMapFullScreenState) {
                            borderTexture = _miniMapBorderTextureFullScreenMode;
                            maskSprite = _miniMapMaskSpriteFullScreenMode;
                        } else {
                            borderTexture = _miniMapBorderTexture;
                            maskSprite = _miniMapMaskSprite;
                        }
                        break;
                }
            }

            if (_miniMapZoomMin < 0.001f) {
                _miniMapZoomMin = 0.001f;
            }
            if (_miniMapZoomMax < _miniMapZoomMin) {
                _miniMapZoomMax = _miniMapZoomMin;
            }
            _miniMapZoomLevel = Mathf.Clamp(_miniMapZoomLevel, _miniMapZoomMin, _miniMapZoomMax);
            _miniMapFullScreenZoomLevel = Mathf.Clamp(_miniMapFullScreenZoomLevel, _miniMapZoomMin, _miniMapZoomMax);

            if (_miniMapCameraMaxAltitude < _miniMapCameraMinAltitude) {
                _miniMapCameraMaxAltitude = _miniMapCameraMinAltitude;
            }

            if (miniMapUI != null) {
                miniMapImage = miniMapUI.GetComponent<Image>();
                // assign minimap border
                if (miniMapImage != null) {
                    miniMapImage.sprite = null;
                    miniMapImage.material = miniMapOverlayMat;
                    Material mat = miniMapImage.materialForRendering;
                    Texture maskTexture = maskSprite != null ? maskSprite.texture : null;
                    miniMapOverlayMat.SetTexture(ShaderParams.MaskTex, maskTexture);
                    miniMapOverlayMat.SetTexture(ShaderParams.BorderTex, borderTexture);
                    mat.SetTexture(ShaderParams.MaskTex, maskTexture);
                    mat.SetTexture(ShaderParams.BorderTex, borderTexture);
                    mat.DisableKeyword(ShaderParams.SKW_COMPASS_FOG_OF_WAR);
                    mat.DisableKeyword(ShaderParams.SKW_COMPASS_RADAR);
                    if (contents == MiniMapContents.Radar && _miniMapRadarGraphicsMethod == MiniMapRadarGraphicsMethod.ProceduralRings) {
                        mat.EnableKeyword(ShaderParams.SKW_COMPASS_RADAR);
                        mat.SetColor(ShaderParams.RingsColor, _miniMapRadarRingsColor);
                    } else {
                        if (_miniMapCameraMode == MiniMapCameraMode.Orthographic && currentMiniMapUsesFogOfWar) {
                            mat.EnableKeyword(ShaderParams.SKW_COMPASS_FOG_OF_WAR);
                        }
                    }
                    if (_miniMapKeepStraight) {
                        mat.DisableKeyword(ShaderParams.SKW_COMPASS_ROTATED);
                    } else {
                        mat.EnableKeyword(ShaderParams.SKW_COMPASS_ROTATED);
                    }
                    if (_miniMapShowViewCone && !_miniMapFullScreenState) {
                        mat.SetColor(ShaderParams.ViewConeColor, _miniMapViewConeColor);
                        if (_miniMapShowViewConeOutline) {
                            mat.DisableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE);
                            mat.EnableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE_OUTLINE);
                            mat.SetColor(ShaderParams.ViewConeOutlineColor, _miniMapViewConeOutlineColor);
                        } else {
                            mat.DisableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE_OUTLINE);
                            mat.EnableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE);
                        }
                        lastViewConeCameraAspect = 0;
                    } else {
                        mat.DisableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE_OUTLINE);
                        mat.DisableKeyword(ShaderParams.SKW_COMPASS_VIEW_CONE);
                    }
                }
                // assign mask
                if (miniMapMaskUI != null) {
                    miniMapMaskImage = miniMapMaskUI.GetComponent<Image>();
                    if (miniMapMaskImage != null) {
                        miniMapMaskImage.sprite = maskSprite;
                    }
                }
            }

            // setup render texture
            if (miniMapCamera != null) {
                miniMapCamera.allowHDR = false;
                miniMapCamera.allowMSAA = false;
                miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
                miniMapCamera.backgroundColor = _miniMapBackgroundColor;
                miniMapCamera.orthographic = _miniMapCameraMode == MiniMapCameraMode.Orthographic || contents == MiniMapContents.Radar || contents == MiniMapContents.WorldMappedTexture;
                miniMapCamera.cullingMask = _miniMapLayerMask & ~(1 << 5);
                miniMapCamera.farClipPlane = _miniMapCameraDepth;
            }

            if (contents != MiniMapContents.TopDownWorldView) {
                MiniMapReleaseRenderTexture();
            }

            if (playerIcon == null) {
                playerIcon = miniMapUIRootRT.Find("CameraCompass");
            }
            if (playerIcon != null) {
                playerIcon.localEulerAngles = Misc.Vector3zero;
                playerIconRT = playerIcon.GetComponent<RectTransform>();
                playerIconImage = playerIcon.GetComponent<Image>();
                playerIcon.gameObject.SetActive(_miniMapShowPlayerIcon);
                if (playerIconImage != null) {
                    if (_miniMapPlayerIconSprite == null) {
                        _miniMapPlayerIconSprite = Resources.Load<Sprite>("CNPro/Sprites/player-icon");
                    }
                    if (_miniMapPlayerIconSprite != null) {
                        playerIconImage.sprite = _miniMapPlayerIconSprite;
                    }
                    playerIconImage.color = _miniMapPlayerIconColor;
                }
            }

            if (miniMapCardinalsRT == null) {
                miniMapCardinalsRT = miniMapUI.Find("Cardinals");
            }
            if (miniMapCardinalsRT != null) {
                miniMapCardinalsImage = miniMapCardinalsRT.GetComponent<Image>();
                miniMapCardinalsRT.gameObject.SetActive(_miniMapShowCardinals && !_miniMapFullScreenState);
                miniMapCardinalsRT.localScale = new Vector3(_miniMapCardinalsSize, _miniMapCardinalsSize, 1f);
                if (miniMapCardinalsImage != null) {
                    if (_miniMapCardinalsSprite == null) {
                        _miniMapCardinalsSprite = Resources.Load<Sprite>("CNPro/Sprites/MiniMapCardinals");
                    }
                    if (_miniMapCardinalsSprite != null) {
                        miniMapCardinalsImage.sprite = _miniMapCardinalsSprite;
                    }
                    miniMapCardinalsImage.color = _miniMapCardinalsColor;
                }
            }
            if (ringsDistanceText == null) {
                Transform t = miniMapUI.Find("RingsDistance");
                ringsDistanceText = t.GetComponent<TextMeshProUGUI>();
            }
            if (ringsDistanceText != null) {
                ringsDistanceText.gameObject.SetActive(_miniMapRadarInfoDisplay != MiniMapRadarInfoType.Nothing && contents == MiniMapContents.Radar);
            }

            if (miniMapCanvasGroup == null) {
                miniMapCanvasGroup = GetMiniMapCanvasGroup(miniMapUIRootRT);
            }

            miniMapCanvasGroup.interactable = miniMapCanvasGroup.blocksRaycasts = currentMiniMapUsesEvents;

            needMiniMapShot = 2;
            needUpdateMiniMapIcons = true;
            miniMapLastBoundsCheckPos = miniMapCamera != null ? miniMapCamera.transform.position : Vector3.zero;
        }

        public Texture2D GetMiniMapMaterialBorderTexture () {
            Material mat = miniMapImage.materialForRendering;
            return mat.GetTexture(ShaderParams.BorderTex) as Texture2D;
        }

        public Texture2D GetMiniMapMaterialMaskTexture () {
            Material mat = miniMapImage.materialForRendering;
            return mat.GetTexture(ShaderParams.MaskTex) as Texture2D;
        }

        void MiniMapReleaseRenderTexture () {
            if (miniMapTex != null) {
                miniMapTex.Release();
            }
        }

        void MiniMapResizeRenderTexture (int width, int height) {
            if (miniMapCamera == null)
                return;

            if (miniMapTex == null || miniMapTex.width != width || miniMapTex.height != height) {
                if (miniMapTex != null) {
                    miniMapTex.Release();
                }
                miniMapTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                miniMapTex.filterMode = FilterMode.Bilinear;
            }

            miniMapCamera.targetTexture = miniMapTex;
        }

        void DisableMiniMap () {
            if (miniMapUIRootRT != null && miniMapUIRootRT.gameObject.activeSelf) {
                miniMapUIRootRT.gameObject.SetActive(false);
            }
            if (miniMapCamera != null) {
                miniMapCamera.enabled = false;
            }
            if (miniMapTex != null) {
                miniMapTex.Release();
                DestroyImmediate(miniMapTex);
            }
        }

        protected virtual void UpdateMiniMap () {

            if (!_showMiniMap || miniMapCamera == null || cameraMain == null)
                return;

            if (needsSetupMiniMap) {
                needsSetupMiniMap = false;
                SetupMiniMap();
            }

			MiniMapContents contents = currentMiniMapContents;
			Transform miniMapCameraTransform = miniMapCamera.transform;
			Transform t = _miniMapOrientation == MiniMapOrientation.Follow && _follow != null ? _follow : _cameraMain.transform;
			bool freezeViewActive = (contents != MiniMapContents.WorldMappedTexture && !contents.usesTexture()) && _miniMapCameraMode == MiniMapCameraMode.Perspective && _miniMapCameraTilt > 0 && _miniMapCameraSnapshotFrequency != MiniMapCameraSnapshotFrequency.Continuous && miniMapLastSnapshotTime > 0f;

			// minimap camera position
			if (!freezeViewActive) {
				if (_miniMapFullScreenState) {
                if (miniMapFullScreenFreezeCamera) {
                    currentCamRot = t.rotation = miniMapFullScreenFixedCameraRotation;
                    currentCamPos = t.position = miniMapFullScreenFixedCameraPosition;
                    if (_follow != null) {
                        _follow.rotation = miniMapFullScreenFixedFollowRotation;
                        _follow.position = miniMapFullScreenFixedFollowPosition;
                    }
                }
                if (miniMapFullScreenWorldCenterFollows) {
                    miniMapCenter = GetMiniMapFollowPos();
                } else {
                    miniMapCenter = GetMiniMapFullScreenWorldCenter();
                }
                if (miniMapFullScreenClampToWorldEdges) {
                    miniMapCenter = ClampMiniMapCenterToWorldSize(miniMapCenter);
                }
                float worldSize = Mathf.Max(miniMapFullScreenWorldSize.x, miniMapFullScreenWorldSize.z) * 0.5f;
                if (_miniMapCameraMode == MiniMapCameraMode.Orthographic) {
                    miniMapCameraTransform.position = new Vector3(miniMapCenter.x, miniMapCenter.y + _miniMapCameraHeightVSFollow + 0.1f, miniMapCenter.z);
                    miniMapCamera.orthographicSize = worldSize;
                } else {
                    float fv = miniMapCamera.fieldOfView;
                    float radAngle = fv * Mathf.Deg2Rad;
                    if (radAngle <= 0) {
                        radAngle = 0.001f;
                    }
                    float altitude = worldSize / Mathf.Tan(radAngle * 0.5f);
                    miniMapCameraTransform.position = new Vector3(miniMapCenter.x, miniMapCenter.y + altitude, miniMapCenter.z);
                }
				} else {
					miniMapCenter = GetMiniMapFollowPos();
					if (contents == MiniMapContents.WorldMappedTexture && miniMapClampToWorldEdges) {
						miniMapCenter = ClampMiniMapCenterToWorldSize(miniMapCenter);
					}
					if (_miniMapCameraMode == MiniMapCameraMode.Orthographic) {
						miniMapCameraTransform.position = new Vector3(miniMapCenter.x, miniMapCenter.y + _miniMapCameraHeightVSFollow + 0.1f, miniMapCenter.z);
						miniMapCamera.orthographicSize = _miniMapCaptureSize * 0.5f;
					} else {
						float altitude = _miniMapCameraMinAltitude + (_miniMapCameraMaxAltitude - _miniMapCameraMinAltitude) * _miniMapZoomLevel;
						miniMapCameraTransform.position = new Vector3(miniMapCenter.x, miniMapCenter.y + altitude, miniMapCenter.z);
					}
				}
			} else {
				// Freeze camera transform to last snapshot pose so viewport/projection matches displayed snapshot
				if (_miniMapFullScreenState) {
					miniMapCenter = miniMapFullScreenWorldCenterFollows ? GetMiniMapFollowPos() : GetMiniMapFullScreenWorldCenter();
					if (miniMapFullScreenClampToWorldEdges) miniMapCenter = ClampMiniMapCenterToWorldSize(miniMapCenter);
				} else {
					miniMapCenter = GetMiniMapFollowPos();
					if (contents == MiniMapContents.WorldMappedTexture && miniMapClampToWorldEdges) miniMapCenter = ClampMiniMapCenterToWorldSize(miniMapCenter);
				}
				miniMapCameraTransform.position = miniMapLastSnapshotLocation;
				if (miniMapLastSnapshotRotation != Quaternion.identity) {
					miniMapCameraTransform.rotation = miniMapLastSnapshotRotation;
				}
			}
            if (miniMapCanvasGroup.alpha != _miniMapAlpha) {
                miniMapCanvasGroup.alpha = _miniMapAlpha;
            }


            // Fits orthographic size to world size
            if (contents == MiniMapContents.WorldMappedTexture) {
                float worldSize = _miniMapFullScreenState ? _miniMapFullScreenWorldSize.x : _miniMapWorldSize.x;
                miniMapCamera.orthographicSize = worldSize * 0.5f;
            }

            // minimap camera rotation
            float rotation;
            if (!freezeViewActive) {
                if (_miniMapKeepStraight || _miniMapFullScreenState) {
                    miniMapCameraTransform.eulerAngles = new Vector3(90, 0, 0);
                    if (playerIcon != null) {
                        Vector3 angles = t.eulerAngles;
                        angles.z = -angles.y;
                        angles.x = angles.y = 0;
                        playerIcon.localEulerAngles = angles;
                    }
                } else {
                    Vector3 forward = _miniMapOrientation == MiniMapOrientation.Follow && _follow != null ? _follow.forward : t.forward;
                    forward.y = 0;
                    miniMapCameraTransform.LookAt(miniMapCenter, forward);
                }
            } else {
                if (!_miniMapKeepStraight && !_miniMapFullScreenState && miniMapLastSnapshotRotation != Quaternion.identity) {
                    miniMapCameraTransform.rotation = miniMapLastSnapshotRotation;
                }
            }

            if (_miniMapCameraTilt > 0 && !_miniMapKeepStraight && _miniMapCameraMode == MiniMapCameraMode.Perspective && _miniMapCameraSnapshotFrequency == MiniMapCameraSnapshotFrequency.Continuous) {
                rotation = 0;
            } else {
                rotation = freezeViewActive ? miniMapLastSnapshotRotationY : t.eulerAngles.y;
            }

            Material mat = miniMapImage.materialForRendering;

            // Position the compass player icon
            bool computePlayerIconPos = _miniMapShowViewCone || (_miniMapShowPlayerIcon && playerIconRT != null);
            if (computePlayerIconPos) {
                float border = currentMiniMapClampBorder;
                Vector2 playerIconPos;
                if (freezeViewActive) {
                    playerIconPos = Misc.Vector2half;
                } else {
                    playerIconPos = GetMiniMapScreenPos(followPos);
                }

                if (currentMiniMapIsCircular) {
                    float dx = playerIconPos.x - 0.5f;
                    float dy = playerIconPos.y - 0.5f;
                    float len = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    if (len < 0.0001f) {
                        playerIconPos = Misc.Vector2half;
                    } else {
                        float clampedLen = Mathf.Min(len, 1f - border);
                        playerIconPos.x = 0.5f + dx * clampedLen / len;
                        playerIconPos.y = 0.5f + dy * clampedLen / len;
                    }
                } else {
                    playerIconPos.x = Mathf.Clamp(playerIconPos.x, border, 1f - border);
                    playerIconPos.y = Mathf.Clamp(playerIconPos.y, border, 1f - border);
                }

                if (playerIconRT != null) {
                    playerIconRT.anchorMin = playerIconRT.anchorMax = playerIconPos;
                    playerIconRT.localScale = new Vector3(_miniMapPlayerIconSize, _miniMapPlayerIconSize, 1f);
                }
                mat.SetVector(ShaderParams.FollowPos, new Vector4(playerIconPos.x, playerIconPos.y, 0, 0));
            }

			// Player/compass icon rotation
			if (playerIcon != null && !_miniMapFullScreenState) {
				if (!_miniMapKeepStraight) {
					if (playerIcon.localEulerAngles != Vector3.zero) {
						playerIcon.localEulerAngles = Vector3.zero;
					}
				} else {
					Vector3 angles = t.eulerAngles;
					angles.z = -angles.y;
					angles.x = angles.y = 0;
					playerIcon.localEulerAngles = angles;
				}
			}

			// Position the North icon
			if (_miniMapShowCardinals && !_miniMapFullScreenState && miniMapCardinalsRT != null) {
				float yaw = t.eulerAngles.y;
				float cardinalsAngle = _miniMapKeepStraight ? 0 : yaw;
				miniMapCardinalsRT.localRotation = Quaternion.Euler(0, 0, cardinalsAngle - _northDegrees);
			}

            if (contents == MiniMapContents.WorldMappedTexture) {

                // Set mini-map shader properties
                mat.SetTexture(ShaderParams.MiniMapTex, _miniMapFullScreenState ? _miniMapFullScreenContentsTexture : _miniMapContentsTexture);

                float zoomLevel = currentMiniMapZoomLevel;

                // mini-map uv
                Vector3 miniMapCameraPos = miniMapCameraTransform.position;

                Vector4 uvOffset;
                float worldSize;
                if (_miniMapFullScreenState) {
                    uvOffset = _miniMapFullScreenWorldCenter - miniMapCameraPos;
                    worldSize = _miniMapFullScreenWorldSize.x;
                } else {
                    uvOffset = _miniMapWorldCenter - miniMapCameraPos;
                    worldSize = _miniMapWorldSize.x;
                }

                uvOffset.x = uvOffset.x / worldSize;
                uvOffset.y = uvOffset.z / worldSize;
                uvOffset.z = zoomLevel;
                uvOffset.w = miniMapUIRootRT.rect.size.y / miniMapUIRootRT.rect.size.x;

                // fog of war shader properties
                Vector4 uvFogOffset = _fogOfWarCenter - miniMapCameraPos;
                uvFogOffset.x = (uvFogOffset.x / worldSize) / zoomLevel;
                uvFogOffset.y = (uvFogOffset.z / worldSize) / zoomLevel;
                uvFogOffset.z = zoomLevel * worldSize / _fogOfWarSize.x;
                uvFogOffset.w = zoomLevel * worldSize / _fogOfWarSize.z;

                // Pass data to shader
                mat.SetVector(ShaderParams.UVOffset, uvOffset);
                mat.SetVector(ShaderParams.UVFogOffset, uvFogOffset);
                mat.SetTexture(ShaderParams.FoWTexture, fogOfWarTexture);
                mat.SetColor(ShaderParams.FoWTintColor, _fogOfWarColor);

                // Pass rotation to shader
                if (_miniMapKeepStraight || _miniMapFullScreenState) {
                    mat.SetFloat(ShaderParams.Rotation, 0);
                    mat.SetFloat(ShaderParams.ConeRotation, -rotation * Mathf.Deg2Rad);
                } else {
                    mat.SetFloat(ShaderParams.Rotation, rotation * Mathf.Deg2Rad);
                    mat.SetFloat(ShaderParams.ConeRotation, 0);
                }
            } else if (contents.usesTexture()) {
                if (miniMapCamera != null) {
                    miniMapCamera.aspect = 1f;
                }
                mat.SetTexture(ShaderParams.MiniMapTex, _miniMapFullScreenState ? _miniMapFullScreenContentsTexture : _miniMapContentsTexture);
                mat.SetVector(ShaderParams.UVOffset, new Vector4(0, 0, 1, 1));
                // Pass rotation to shader
                if (_miniMapFullScreenState) {
                    mat.SetFloat(ShaderParams.Rotation, 0);
                } else {
                    mat.SetFloat(ShaderParams.Rotation, _miniMapContentsTextureAllowRotation ? rotation * Mathf.Deg2Rad : 0);
                }
                if (_miniMapKeepStraight || _miniMapFullScreenState) {
                    mat.SetFloat(ShaderParams.ConeRotation, -rotation * Mathf.Deg2Rad);
                } else {
                    mat.SetFloat(ShaderParams.ConeRotation, 0);
                }
            } else {
                // snapshot control
                switch (_miniMapCameraSnapshotFrequency) {
                    case MiniMapCameraSnapshotFrequency.TimeInterval:
                        if (Time.time - miniMapLastSnapshotTime > _miniMapSnapshotInterval) {
                            needMiniMapShot = 1;
                        }
                        break;
                    case MiniMapCameraSnapshotFrequency.DistanceTravelled: {
                            Vector3 lastCenter = miniMapLastSnapshotCenter == Vector3.zero ? miniMapCenter : miniMapLastSnapshotCenter;
                            Vector3 delta = miniMapCenter - lastCenter;
                            delta.y = 0; // ignore vertical
                            if (delta.sqrMagnitude > _miniMapSnapshotDistance * _miniMapSnapshotDistance) {
                                needMiniMapShot = 1;
                            }
                        }
                        break;
                    case MiniMapCameraSnapshotFrequency.Continuous:
                        needMiniMapShot = 1;
                        break;
                }

                // force snapshot if current displayed area would exceed captured area
                if (_miniMapCameraSnapshotFrequency != MiniMapCameraSnapshotFrequency.Continuous && miniMapTex != null && miniMapLastSnapshotTime > 0f) {
                    Vector3 checkDelta = miniMapCameraTransform.position - miniMapLastBoundsCheckPos;
                    checkDelta.y = 0f;
                    if (checkDelta.sqrMagnitude >= 25f || miniMapLastBoundsCheckPos == Vector3.zero) {
                        Vector3 d = miniMapCameraTransform.position - miniMapLastSnapshotLocation;
                        float viewAspect = GetMiniMapAspectRatio();
                        float zoomLevelForView = currentMiniMapZoomLevel;
                        float currentCamSize;
                        if (miniMapCamera.orthographic) {
                            currentCamSize = miniMapCamera.orthographicSize * 2f;
                        } else {
                            float altitudeNow = Mathf.Abs(miniMapCameraTransform.position.y - miniMapCenter.y);
                            float radAngleNow = miniMapCamera.fieldOfView * Mathf.Deg2Rad;
                            currentCamSize = 2f * altitudeNow * Mathf.Tan(radAngleNow * 0.5f);
                        }
                        float halfH = (currentCamSize * 0.5f) / zoomLevelForView;
                        float halfW = halfH * viewAspect;

                        float snapshotHeight;
                        if (miniMapCamera.orthographic) {
                            snapshotHeight = _miniMapCaptureSize;
                        } else {
                            float radSnap = miniMapLastSnapshotFoV * Mathf.Deg2Rad;
                            snapshotHeight = 2f * miniMapLastSnapshotAltitude * Mathf.Tan(radSnap * 0.5f);
                        }
                        float snapHalf = snapshotHeight * 0.5f;
                        float paddingMeters = 5f;
                        snapHalf = snapHalf > paddingMeters ? (snapHalf - paddingMeters) : 0f;

                        float usedRot = freezeViewActive ? miniMapLastSnapshotRotationY : rotation;
                        float theta = (_miniMapKeepStraight || _miniMapFullScreenState) ? 0f : usedRot * Mathf.Deg2Rad;
                        float c = Mathf.Cos(theta);
                        float s = Mathf.Sin(theta);
                        float extX = Mathf.Abs(halfW * c) + Mathf.Abs(halfH * s);
                        float extZ = Mathf.Abs(halfW * s) + Mathf.Abs(halfH * c);

                        if (Mathf.Abs(d.x) + extX > snapHalf || Mathf.Abs(d.z) + extZ > snapHalf) {
                            needMiniMapShot = 1;
                        }
                        miniMapLastBoundsCheckPos = miniMapCameraTransform.position;
                    }
                }

                // capture map
                if (needMiniMapShot > 0) {
                    needMiniMapShot--;
#if UNITY_EDITOR
                    if (!Application.isPlaying) needMiniMapShot = 0;
#endif
                    if (needMiniMapShot <= 0) {
                        needMiniMapShot = 0;

                        Quaternion oldRot = miniMapCameraTransform.rotation;

                        // Adjust orbit
                        bool isTilted = _miniMapCameraTilt > 0 && _miniMapCameraMode == MiniMapCameraMode.Perspective;
                        if (isTilted) {
                            if (_miniMapCameraSnapshotFrequency != MiniMapCameraSnapshotFrequency.Continuous) {
                                float yaw = (_miniMapKeepStraight || _miniMapFullScreenState) ? 0f : t.eulerAngles.y;
                                miniMapCameraTransform.eulerAngles = new Vector3(90f, yaw, 0f);
                            }
                            miniMapCameraTransform.Rotate(-_miniMapCameraTilt, 0, 0, Space.Self);
                            float dist = Vector3.Distance(miniMapCenter, miniMapCameraTransform.position);
                            miniMapCameraTransform.position = miniMapCenter - miniMapCameraTransform.forward * dist;
                        } else {
                            miniMapCameraTransform.eulerAngles = new Vector3(90, 0, 0);
                        }

                        if (_miniMapFullScreenState) {
                            MiniMapResizeRenderTexture((int)_miniMapFullScreenResolution, (int)_miniMapFullScreenResolution);
                        } else {
                            MiniMapResizeRenderTexture((int)_miniMapResolution, (int)_miniMapResolution);
                        }

                        if (CNP2HDRPCameraSetup.usesHDRP) { // workaround for HDRP bug which renders screen space overlay UI in the minimap render texture..
                            _canvas.enabled = false;
                        }

                        OnMiniMapBeforeCapture.Invoke();

                        Vector3 originalCameraPosition = miniMapCameraTransform.position;
                        Vector3 quantizedCameraPosition = originalCameraPosition;
                        if (_miniMapCameraSnapshotFrequency == MiniMapCameraSnapshotFrequency.Continuous && miniMapCamera.orthographic && miniMapTex != null) {
                            float camSizeQ = miniMapCamera.orthographicSize * 2f;
                            float worldPerPixel = camSizeQ / miniMapTex.width;
                            quantizedCameraPosition.x = Mathf.Round(quantizedCameraPosition.x / worldPerPixel) * worldPerPixel;
                            quantizedCameraPosition.z = Mathf.Round(quantizedCameraPosition.z / worldPerPixel) * worldPerPixel;
                            miniMapCameraTransform.position = quantizedCameraPosition;
                        }

                        // Ensure far clip covers ground and scene when using perspective
                        if (!miniMapCamera.orthographic) {
                            float altitudeToGround = Mathf.Abs(miniMapCameraTransform.position.y - miniMapCenter.y);
                            float desiredFar = altitudeToGround + 1f;
                            if (miniMapCamera.farClipPlane < desiredFar) miniMapCamera.farClipPlane = desiredFar;
                        }

                        bool originalFog = RenderSettings.fog;
                        RenderSettings.fog = false;
                        if (!_miniMapEnableShadows && Application.isPlaying) {
                            ShadowQuality sq = QualitySettings.shadows;
                            QualitySettings.shadows = ShadowQuality.Disable;
                            miniMapCamera.Render();
                            QualitySettings.shadows = sq;
                        } else {
                            miniMapCamera.Render();
                        }
                        RenderSettings.fog = originalFog;

                        OnMiniMapAfterCapture.Invoke();

                        if (_miniMapCameraSnapshotFrequency == MiniMapCameraSnapshotFrequency.Continuous && miniMapCamera.orthographic && miniMapTex != null) {
                            miniMapCameraTransform.position = originalCameraPosition;
                        }

                        if (CNP2HDRPCameraSetup.usesHDRP) {
                            _canvas.enabled = true;
                        }

                        if (!_miniMapKeepStraight && !isTilted) {
                            miniMapCameraTransform.rotation = oldRot;
                        }

                        miniMapLastSnapshotTime = Time.time;
                        miniMapLastSnapshotLocation = (_miniMapCameraSnapshotFrequency == MiniMapCameraSnapshotFrequency.Continuous && miniMapCamera.orthographic && miniMapTex != null) ? quantizedCameraPosition : miniMapCameraTransform.position;
                        miniMapLastSnapshotCenter = miniMapCenter;
                        miniMapLastSnapshotAltitude = Mathf.Abs(miniMapCameraTransform.position.y - miniMapCenter.y);
                        miniMapLastSnapshotFoV = miniMapCamera.fieldOfView;
                        miniMapLastSnapshotRotationY = rotation;
                        miniMapLastSnapshotRotation = miniMapCameraTransform.rotation;

                        needUpdateMiniMapIcons = true;
#if UNITY_EDITOR
                    } else if (!Application.isPlaying) {
                        EditorApplication.delayCall += UpdateMiniMap;
#endif
                    }
                }

                // Set mini-map shader properties
                mat.SetTexture(ShaderParams.MiniMapTex, miniMapTex);

                float zoomLevel = currentMiniMapZoomLevel;

                // mini-map uv
                Vector3 miniMapCameraPos = miniMapCameraTransform.position;

                bool freezeView = _miniMapCameraMode == MiniMapCameraMode.Perspective && _miniMapCameraTilt > 0 && _miniMapCameraSnapshotFrequency != MiniMapCameraSnapshotFrequency.Continuous && miniMapLastSnapshotTime > 0f;

                Vector4 uvOffset;
                float camSize;
                if (freezeView) {
                    uvOffset = Vector4.zero;
                    float radAngleSnap = miniMapLastSnapshotFoV * Mathf.Deg2Rad;
                    camSize = 2f * miniMapLastSnapshotAltitude * Mathf.Tan(radAngleSnap * 0.5f);
                } else {
                    uvOffset = miniMapLastSnapshotLocation - miniMapCameraPos;
                    if (miniMapCamera.orthographic) {
                        camSize = miniMapCamera.orthographicSize * 2f; // world height
                    } else {
                        float altitude = Mathf.Abs(miniMapCameraTransform.position.y - miniMapCenter.y);
                        float radAngle = miniMapCamera.fieldOfView * Mathf.Deg2Rad;
                        camSize = 2f * altitude * Mathf.Tan(radAngle * 0.5f); // world height at ground plane
                    }
                }
                uvOffset.x = uvOffset.x / camSize;
                uvOffset.y = uvOffset.z / camSize;
                uvOffset.z = zoomLevel;
                float aspectRatio = GetMiniMapAspectRatio();
                uvOffset.w = 1f / aspectRatio;

                // fog of war shader properties
                Vector3 refCamPosForFog = freezeView ? miniMapLastSnapshotLocation : miniMapCameraPos;
                Vector4 uvFogOffset = _fogOfWarCenter - refCamPosForFog;
                uvFogOffset.x = (uvFogOffset.x / camSize) / zoomLevel;
                uvFogOffset.y = (uvFogOffset.z / camSize) / zoomLevel;
                uvFogOffset.y *= aspectRatio;
                uvFogOffset.z = zoomLevel * camSize / _fogOfWarSize.x;
                uvFogOffset.w = zoomLevel * camSize / _fogOfWarSize.z;

                // Pass data to shader
                mat.SetVector(ShaderParams.UVOffset, uvOffset);
                mat.SetVector(ShaderParams.UVFogOffset, uvFogOffset);
                mat.SetTexture(ShaderParams.FoWTexture, fogOfWarTexture);
                mat.SetColor(ShaderParams.FoWTintColor, _fogOfWarColor);

                // Pass rotation to shader
                float rotationForShader = freezeView && miniMapLastSnapshotTime > 0f ? miniMapLastSnapshotRotationY : rotation;
                float currentYaw = t.eulerAngles.y;
                if (freezeView) {
                    // Tilted + non-continuous: keep map horizon level; cone rotates only if Keep Straight is enabled
                    mat.SetFloat(ShaderParams.Rotation, 0);
                    float coneRot = _miniMapKeepStraight ? -currentYaw * Mathf.Deg2Rad : 0f;
                    mat.SetFloat(ShaderParams.ConeRotation, coneRot);
                } else if (_miniMapKeepStraight || _miniMapFullScreenState) {
                    mat.SetFloat(ShaderParams.Rotation, 0);
                    mat.SetFloat(ShaderParams.ConeRotation, -rotation * Mathf.Deg2Rad);
                } else {
                    mat.SetFloat(ShaderParams.Rotation, rotationForShader * Mathf.Deg2Rad);
                    mat.SetFloat(ShaderParams.ConeRotation, 0f);
                }
            }

            if (_miniMapShowViewCone) {
                float aspect = _cameraMain.aspect;
                float fov = _cameraMain.fieldOfView;
                if (lastViewConeCameraAspect == 0 || aspect != lastViewConeCameraAspect || fov != lastViewConeFoV) {
                    lastViewConeCameraAspect = aspect;
                    lastViewConeFoV = fov;
                    float radHFOV;
                    if (_miniMapViewConeFoVSource == MiniMapViewConeFovSource.FromCamera) {
                        var radAngle = _cameraMain.fieldOfView * Mathf.Deg2Rad;
                        radHFOV = Mathf.Atan(Mathf.Tan(radAngle / 2f) * aspect);
                    } else {
                        radHFOV = _miniMapViewConeFoV * Mathf.Deg2Rad * 0.5f;
                    }
                    Vector2 pos = GetMiniMapScreenPosNoShift(GetMiniMapFollowPos() + new Vector3(0, 0, _miniMapViewConeDistance));
                    pos.x -= 0.5f;
                    pos.y -= 0.5f;
                    float screenDist = pos.magnitude;
                    viewConeData.x = radHFOV;
                    viewConeData.y = screenDist * screenDist;
                    viewConeData.z = viewConeData.y / (_miniMapViewConeFallOff * _miniMapViewConeFallOff);
                    mat.SetVector(ShaderParams.ViewConeData, viewConeData);
                }
            }

            mat.SetVector(ShaderParams.Effects, new Vector4(_miniMapBrightness, _miniMapContrast, _miniMapLutIntensity, _miniMapVignette && currentMiniMapContents != MiniMapContents.Radar && !_miniMapFullScreenState ? _miniMapVignetteColor.a * 48f : 0));
            mat.SetColor(ShaderParams.BackgroundColor, _miniMapBackgroundColor);
            mat.SetInt(ShaderParams.BackgroundOpaque, _miniMapBackgroundOpaque ? 1 : 0);
            mat.SetColor(ShaderParams.TintColor, _miniMapTintColor);
            mat.SetColor(ShaderParams.VignetteColor, _miniMapVignetteColor);
            if (_miniMapLutTexture != null && _miniMapLutIntensity > 0) {
                mat.SetTexture(ShaderParams.LUTTexture, _miniMapLutTexture);
                mat.EnableKeyword(ShaderParams.SKW_COMPASS_LUT);
            } else {
                mat.DisableKeyword(ShaderParams.SKW_COMPASS_LUT);
            }
            if (contents == MiniMapContents.Radar && _miniMapRadarGraphicsMethod == MiniMapRadarGraphicsMethod.ProceduralRings) {
                float radarInfoDistance = _miniMapRadarInfoDisplay == MiniMapRadarInfoType.RadarRange ? _miniMapCaptureSize * 0.5f : _miniMapRadarRingsDistance;
                if (radarInfoDistance != lastRadarInfoDistance) {
                    float uvDist = 0;
                    for (int k = 0; k < 10; k++) {
                        Vector3 radarRingPos = GetMiniMapScreenPosNoShift(miniMapCenter + new Vector3(0, 0, _miniMapRadarRingsDistance));
                        radarRingPos.x -= 0.5f;
                        radarRingPos.y -= 0.5f;
                        uvDist = Mathf.Sqrt(radarRingPos.x * radarRingPos.x + radarRingPos.y * radarRingPos.y) * 2f;
                        if (uvDist > 0.1f) break;
                    }
                    if (radarInfoDistance != lastRadarInfoDistance && _miniMapRadarInfoDisplay != MiniMapRadarInfoType.Nothing && ringsDistanceText != null) {
                        lastRadarInfoDistance = radarInfoDistance;
                        ringsDistanceText.text = (int)radarInfoDistance + "m";
                    }
                    mat.SetVector(ShaderParams.RingsData, new Vector4(uvDist, 10f / _miniMapRadarRingsWidth, 0, 0));
                }
                float pulseOpacity = _miniMapRadarPulseEnabled ? _miniMapRadarPulseOpacity : 0;
                switch (_miniMapRadarPulseAnimationPreset) {
                    case MiniMapPulsePreset.Default:
                        mat.SetVector(ShaderParams.RingsPulseData, new Vector4(5, 50, 0.1f, pulseOpacity));
                        break;
                    case MiniMapPulsePreset.LongSweep:
                        mat.SetVector(ShaderParams.RingsPulseData, new Vector4(4, 15, 0.25f, pulseOpacity));
                        break;
                    case MiniMapPulsePreset.Scanning:
                        mat.SetVector(ShaderParams.RingsPulseData, new Vector4(30, 4, 3f, pulseOpacity));
                        break;
                    default:
                        mat.SetVector(ShaderParams.RingsPulseData, new Vector4(_miniMapRadarPulseSpeed, _miniMapRadarPulseFallOff, _miniMapRadarPulseFrequency, pulseOpacity));
                        break;
                }
            }
        }


        Vector3 ClampMiniMapCenterToWorldSize (Vector3 miniMapCenter) {
            float zoomLevel = currentMiniMapZoomLevel;
            Vector3 ceroPos;
            float halfWorldSize;
            if (_miniMapFullScreenState) {
                ceroPos = _miniMapFullScreenWorldCenter;
                halfWorldSize = _miniMapFullScreenWorldSize.x * 0.5f;
            } else {
                ceroPos = _miniMapWorldCenter;
                halfWorldSize = _miniMapWorldSize.x * 0.5f;
            }
            float gap = halfWorldSize * zoomLevel;
            float nx = Mathf.Clamp(miniMapCenter.x, ceroPos.x - halfWorldSize + gap, ceroPos.x + halfWorldSize - gap);
            float nz = Mathf.Clamp(miniMapCenter.z, ceroPos.z - halfWorldSize + gap, ceroPos.z + halfWorldSize - gap);
            _miniMapFollowOffset.x += nx - miniMapCenter.x;
            _miniMapFollowOffset.z += nz - miniMapCenter.z;
            miniMapCenter.x = nx;
            miniMapCenter.z = nz;
            return miniMapCenter;
        }

        void ClampDragOffset () {
            float l = Mathf.Sqrt(_miniMapFollowOffset.x * _miniMapFollowOffset.x + _miniMapFollowOffset.z * _miniMapFollowOffset.z);
            if (_miniMapFullScreenState) {
                if (l > _miniMapFullScreenDragMaxDistance) {
                    _miniMapFollowOffset.y = 0;
                    _miniMapFollowOffset = _miniMapFollowOffset.normalized * _miniMapFullScreenDragMaxDistance;
                }
            } else {
                if (l > _miniMapDragMaxDistance) {
                    _miniMapFollowOffset.y = 0;
                    _miniMapFollowOffset = _miniMapFollowOffset.normalized * _miniMapDragMaxDistance;
                }
            }
        }

        /// <summary>
        /// Update minimap icons
        /// </summary>
        protected virtual void UpdateMiniMapIcons () {

            if (!needUpdateMiniMapIcons)
                return;

            if (miniMapCamera == null) {
                return;
            }

            needUpdateMiniMapIcons = false;

            float now = Time.time;
            int frameCount = Time.frameCount;
            float miniMapAspectRatio = GetMiniMapAspectRatio();
            float scaleAnimSpeed = Application.isPlaying ? Time.deltaTime * 10f : 1f;
            Vector3 scaleVector = Vector3.one;
            Quaternion invRot = Quaternion.Inverse(currentCamRot);

            MiniMapContents contents = currentMiniMapContents;

            int iconsCount = activePOIs.Count;
            for (int p = 0; p < iconsCount; p++) {
                CompassProPOI poi = activePOIs[p];
                CompassProPOIState state = poi.GetState(_compassGroup);

                if (poi.miniMapType != POIMiniMapType.Any) {
                    if (contents == MiniMapContents.Radar) {
                        if (poi.miniMapType != POIMiniMapType.RadarOnly) continue;
                    } else {
                        if (poi.miniMapType != POIMiniMapType.MiniMapOnly) continue;
                    }
                }

                bool iconVisibleInMiniMap = true;

                if (state.isVisited && poi.hideWhenVisited) {
                    iconVisibleInMiniMap = false;
                }

                if (!poi.isActiveAndEnabled || !iconVisibleInMiniMap) {
                    ToggleMiniMapIconVisibility(poi, state, false);
                    ToggleMiniMapCircleVisibility(poi, state, false);
                    continue;
                }

                // Update POI viewport position and distance
                if (frameCount != state.viewportPosFrameCount) {
                    state.viewportPosFrameCount = frameCount;
                    ComputePOIViewportPos(poi);
                }

                // Should we make this POI visible in the mini-map?
                Vector3 poiPosition = poi.transform.position;
                float thisPOIVisibleMaxDistance = poi.miniMapVisibleDistanceOverride > 0 ? poi.miniMapVisibleDistanceOverride : _miniMapVisibleMaxDistance;
                bool isInRange = state.distanceToFollow < thisPOIVisibleMaxDistance;

                iconVisibleInMiniMap = poi.miniMapVisibility == POIVisibility.AlwaysVisible || (poi.miniMapVisibility == POIVisibility.WhenInRange && isInRange);

                if (iconVisibleInMiniMap) {

                    Vector3 miniMapScreenPos = GetMiniMapScreenPos(poiPosition, miniMapAspectRatio);

                    // POI is visible, should we create the icon in the minimap? Need to create due to the circle effect even if the icon is out of the mini-map area
                    if (state.miniMapIconRT == null) {
                        GameObject iconGO = CreateMiniMapIcon(poi);
                        if (iconGO == null) continue;
                        state.miniMapIconRT = iconGO.GetComponent<RectTransform>();
                        MiniMapIconElements elements = iconGO.GetComponent<MiniMapIconElements>();
                        if (elements == null) {
                            Debug.LogError("MiniMap icon prefab missing MiniMapIconElements component.");
                            Misc.DestroySafe(iconGO);
                            state.miniMapIconRT = null;
                            continue;
                        }
                        state.miniMapIconImage = elements.iconImage;
                        if (state.miniMapIconImage != null) {
                            state.miniMapIconImageRT = state.miniMapIconImage.GetComponent<RectTransform>();
                            if (_miniMapIconEvents) {
                                state.miniMapIconImage.raycastTarget = true;
                                CompassIconEventHandler eventHandler = iconGO.AddComponent<CompassIconEventHandler>();
                                eventHandler.poi = poi;
                                eventHandler.compass = this;
                            }
                        }

                        state.miniMapCircleRT = elements.circle;
                        if (state.miniMapCircleRT != null) {
                            state.miniMapCircleImage = state.miniMapCircleRT.GetComponent<Image>();
                            if (state.miniMapCircleImage.material == null) {
                                state.miniMapCircleImage.material = Resources.Load<Material>("CNPro/Materials/MiniMapCircle");
                            }
                        }
                        OnMiniMapIconCreated?.Invoke(poi, iconGO);
                    }

                    iconVisibleInMiniMap = miniMapScreenPos.x >= 0 && miniMapScreenPos.x < 1f && miniMapScreenPos.y >= 0 && miniMapScreenPos.y < 1f;

                    float miniMapIconTempScale = 1f;

                    float miniMapIconAlpha = 1f;
                    {
                        float dx = miniMapScreenPos.x - 0.5f;
                        float dy = miniMapScreenPos.y - 0.5f;
                        float len = Mathf.Sqrt(dx * dx + dy * dy) * 2f + 0.000001f;
                        if (currentMiniMapContents == MiniMapContents.Radar && _miniMapRadarFadePOIs && !poi.miniMapClampPosition) {
                            miniMapIconAlpha = 1f - Mathf.Clamp01((len - 0.7f) / 0.3f);
                        }

                        // Always show the clamped icon in the minimap; if out of map, maintain it on the edge with normal scale
                        if (poi.miniMapClampPosition) {
                            iconVisibleInMiniMap = true;
                            float border = currentMiniMapClampBorder;
                            bool isClamped = false;
                            if (currentMiniMapIsCircular) {
                                float clampedLen = Mathf.Min(len, 1f - border);
                                miniMapScreenPos.x = 0.5f + dx * clampedLen / len;
                                miniMapScreenPos.y = 0.5f + dy * clampedLen / len;
                                isClamped = len > 1f - border;
                            } else {
                                if (miniMapScreenPos.x < border) {
                                    miniMapScreenPos.x = border;
                                    isClamped = true;
                                } else if (miniMapScreenPos.x > 1f - border) {
                                    miniMapScreenPos.x = 1f - border;
                                    isClamped = true;
                                }
                                if (miniMapScreenPos.y < border) {
                                    miniMapScreenPos.y = border;
                                    isClamped = true;
                                } else if (miniMapScreenPos.y > 1f - border) {
                                    miniMapScreenPos.y = 1f - border;
                                    isClamped = true;
                                }
                            }
                            if (isClamped) {
                                miniMapIconTempScale = poi.miniMapClampedScaleMultiplier;
                            }
                        }
                    }

                    float miniMapRadius = poi.miniMapCircleRadius > 0 ? poi.miniMapCircleRadius : poi.radius;
                    if (miniMapRadius > 0 && (poi.miniMapShowCircle || (state.circleVisibleTime <= 0 && poi.miniMapCircleAnimationWhenAppears))) {
                        iconVisibleInMiniMap = true;

                        if (state.lastCircleRadius != miniMapRadius || state.lastCircleHeight != miniMapUI.rect.height || state.lastCircleZoomLevel != currentMiniMapZoomLevel) {
                            state.lastCircleRadius = miniMapRadius;
                            state.lastCircleHeight = miniMapUI.rect.height;
                            state.lastCircleZoomLevel = currentMiniMapZoomLevel;
                            ComputeCircleScale(poi, state, miniMapAspectRatio);
                        }

                        Material mat = state.miniMapCircleImage.materialForRendering;
                        state.miniMapCircleImage.color = poi.miniMapCircleColor;
                        mat.SetColor(ShaderParams.CircleInnerColor, poi.miniMapCircleInnerColor);
                        mat.SetFloat(ShaderParams.CircleStartRadius, poi.miniMapCircleStartRadius);
                        ToggleMiniMapCircleVisibility(poi, state, true);
                    } else if (!poi.miniMapCircleAnimationWhenAppears) {
                        ToggleMiniMapCircleVisibility(poi, state, false);
                    }

                    if (state.circleVisibleTime <= 0) {
                        state.circleVisibleTime = now;

                        if (poi.miniMapCircleAnimationWhenAppears && Application.isPlaying) {
                            StartCoroutine(AnimateCircle(poi, state, now));
                        }
                    }

                    if (iconVisibleInMiniMap) {

                        // Position the icon on the mini-map area
                        state.miniMapIconRT.anchorMin = state.miniMapIconRT.anchorMax = miniMapScreenPos;

                        // Assign proper icon
                        if (state.isVisited) {
                            if (state.miniMapIconImage.sprite != poi.iconVisited) {
                                state.miniMapIconImage.sprite = poi.iconVisited;
                            }
                        } else if (state.miniMapIconImage.sprite != poi.iconNonVisited) {
                            state.miniMapIconImage.sprite = poi.iconNonVisited;
                        }

                        // tint color
                        Color miniMapIconTintColor = poi.tintColor;
                        miniMapIconTintColor.a *= miniMapIconAlpha;
                        state.miniMapIconImage.color = miniMapIconTintColor;

                        // Scale icon
                        state.miniMapCurrentIconScale = _miniMapIconSize * poi.miniMapIconScale * miniMapIconTempScale;
                        Vector3 rtCurrentScale = state.miniMapIconImageRT.localScale;
                        if (SignificantlyDifferents(state.miniMapCurrentIconScale, rtCurrentScale.x)) {
                            needUpdateMiniMapIcons = true;
                            float scale = Mathf.Lerp(rtCurrentScale.x, state.miniMapCurrentIconScale, scaleAnimSpeed);
                            scaleVector.x = scaleVector.y = scale;
                            state.miniMapIconImageRT.localScale = scaleVector;
                        }

                        // Rotate?
                        if (poi.miniMapShowRotation) {
                            float angle;
                            if (_miniMapKeepStraight) {
                                angle = poi.transform.localEulerAngles.y;
                            } else {
                                Quaternion poiRot = invRot * poi.transform.localRotation;
                                angle = poiRot.eulerAngles.y;
                            }
                            Vector3 rot = new Vector3(0, 0, poi.miniMapRotationAngleOffset - angle);
                            state.miniMapIconRT.localEulerAngles = rot;
                        }
#if UNITY_EDITOR
else {
                            // in editor, cancel any icon rotation when disabling the "miniMapShowRotation" option
                            state.miniMapIconRT.rotation = Quaternion.identity;
                        }
#endif
                    }
                }

                // Send events
                if (state.miniMapIsVisible && !iconVisibleInMiniMap) {
                    OnPOIHidesInMiniMap.Invoke(poi);
                } else if (iconVisibleInMiniMap && !state.miniMapIsVisible) {
                    OnPOIVisibleInMiniMap.Invoke(poi);
                }

                ToggleMiniMapIconVisibility(poi, state, iconVisibleInMiniMap);
            }
        }


        bool SignificantlyDifferents (float a, float b) {
            float d = a - b;
            return d < -1e-3f || d > 1e-3f;
        }

        void HideMiniMapIcons () {

            int iconsCount = activePOIs.Count;
            for (int p = 0; p < iconsCount; p++) {
                CompassProPOI poi = activePOIs[p];
                if (poi == null) continue;
                CompassProPOIState state = poi.GetState(_compassGroup);

                bool canHide = poi.miniMapType == POIMiniMapType.Any || (poi.miniMapType == POIMiniMapType.RadarOnly && _miniMapContents == MiniMapContents.Radar) || (poi.miniMapType == POIMiniMapType.MiniMapOnly && _miniMapContents != MiniMapContents.Radar);
                if (canHide) {
                    ToggleMiniMapIconVisibility(poi, state, false);
                    ToggleMiniMapCircleVisibility(poi, state, false);
                }
            }
        }


        void ComputeCircleScale (CompassProPOI poi, CompassProPOIState state, float miniMapAspectRatio) {
            float radius = poi.miniMapCircleRadius > 0 ? poi.miniMapCircleRadius : poi.radius;
            Vector3 miniMapScreenPosRadius = GetMiniMapScreenPos(miniMapCenter + new Vector3(0, 0, radius), miniMapAspectRatio);
            float dx = miniMapScreenPosRadius.x - 0.5f;
            float dy = miniMapScreenPosRadius.y - 0.5f;
            float len = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
            state.circleScale = len * miniMapUI.rect.height / state.miniMapCircleRT.rect.height;
            if (state.miniMapCircleMaterial == null) {
                state.miniMapCircleMaterial = Instantiate(state.miniMapCircleImage.material);
            }
            state.miniMapCircleRT.localScale = new Vector3(state.circleScale, state.circleScale, 1f);
        }

        IEnumerator AnimateCircle (CompassProPOI poi, CompassProPOIState state, float startTime) {

            if (poi == null || state.miniMapCircleRT == null) yield break;

            ToggleMiniMapCircleVisibility(poi, state, true);

            int repetitions = _focusedPOI == poi ? int.MaxValue : poi.miniMapCircleAnimationRepetitions;

            for (int k = 0; k < repetitions; k++) {

                // Circle effect
                Vector3 scale = Misc.Vector3one;
                Color color = poi.miniMapCircleColor;
                float t;
                do {
                    if (poi == null || !poi.isActiveAndEnabled || state.miniMapCircleRT == null) yield break;
                    t = (Time.time - startTime) / _miniMapIconCircleAnimationDuration;
                    if (t < 1f) {
                        scale.x = scale.y = t * state.circleScale;
                        state.miniMapCircleRT.localScale = scale;
                        if (!poi.miniMapShowCircle) {
                            color.a = 1f - t;
                        }
                        state.miniMapCircleImage.color = color;
                    }
                    yield return null;
                } while (t < 1f);

                startTime = Time.time;
            }

            if (poi != null && !poi.miniMapShowCircle) {
                ToggleMiniMapCircleVisibility(poi, state, false);
            }

        }

        CanvasGroup GetMiniMapCanvasGroup (Transform transform) {
            if (transform == null) {
                return null;
            }
            CanvasGroup canvasGroup = transform.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = transform.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
            }
            return canvasGroup;

        }

        void ToggleButtonEventHandler (string buttonName, UnityAction handler, bool continuous, bool isVisible) {
            if (miniMapButtonsPanel == null)
                return;
            Transform t = miniMapButtonsPanel.Find(buttonName);
            if (t == null)
                return;
            if (!isVisible) {
                t.gameObject.SetActive(false);
                return;
            }
            t.gameObject.SetActive(true);
            Button button = t.GetComponent<Button>();
            if (button == null)
                return;

            if (continuous) {
                CompassButtonHandler buttonHandler = button.GetComponent<CompassButtonHandler>();
                if (buttonHandler == null) {
                    buttonHandler = button.gameObject.AddComponent<CompassButtonHandler>();
                }
                buttonHandler.actionHandler = handler;
            } else {
                button.onClick.RemoveListener(handler);
                button.onClick.AddListener(handler);
            }
        }

        void MiniMapZoomToggle (bool state) {

            if (miniMapUIRootRT == null) {
                SetupMiniMap(true);
            }

            if (cameraMain == null || miniMapCamera == null) {
                return;
            }

            _miniMapFullScreenState = state;

            RectTransform rt = miniMapUIRootRT;

            if (state) {
                miniMapRegularZoomLevel = _miniMapZoomLevel;
                _miniMapZoomLevel = _miniMapFullScreenZoomLevel;
                Transform t = cameraMain.transform;
                miniMapFullScreenFixedCameraRotation = t.rotation;
                miniMapFullScreenFixedCameraPosition = t.position;
                if (_follow != null) {
                    miniMapFullScreenFixedFollowRotation = _follow.rotation;
                    miniMapFullScreenFixedFollowPosition = _follow.position;
                }
                miniMapAnchorMin = rt.anchorMin;
                miniMapAnchorMax = rt.anchorMax;
                miniMapPivot = rt.pivot;
                miniMapSizeDelta = rt.sizeDelta;
                miniMapCameraAspect = miniMapCamera.aspect;
                SetupMiniMap(true);
                float padding = (1f - _miniMapFullScreenSize) * 0.5f;
                float minX, minY, maxX, maxY;
                if (_miniMapFullScreenPlaceholder != null) {
                    // Disables blocking on the image just in case
                    _miniMapFullScreenPlaceholder.gameObject.SetActive(false);
                    // Adjust viewport
                    Rect vwRect = _miniMapFullScreenPlaceholder.GetViewportRect(_cameraMain);
                    minY = vwRect.yMin + padding;
                    maxY = vwRect.yMax - padding;
                    float aspect = vwRect.width / vwRect.height;
                    minX = vwRect.xMin + padding * aspect;
                    maxX = vwRect.xMax - padding * aspect;
                } else {
                    minY = padding;
                    maxY = 1f - padding;
                    if (_miniMapKeepAspectRatio) {
                        float paddingW = (1f - _miniMapFullScreenSize / cameraMain.aspect) * 0.5f;
                        minX = paddingW;
                        maxX = 1f - paddingW;
                    } else {
                        minX = minY;
                        maxX = maxY;
                    }
                }
                // Apply viewport rect in Screen Space Overlay mode
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    minX = _viewportRect.x + minX * _viewportRect.width;
                    maxX = _viewportRect.x + maxX * _viewportRect.width;
                    minY = _viewportRect.y + minY * _viewportRect.height;
                    maxY = _viewportRect.y + maxY * _viewportRect.height;
                }

                rt.anchorMin = new Vector3(minX, minY);
                rt.anchorMax = new Vector3(maxX, maxY);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(0, 0);
            } else {
                _miniMapZoomLevel = miniMapRegularZoomLevel;
                rt.anchorMin = miniMapAnchorMin;
                rt.anchorMax = miniMapAnchorMax;
                rt.pivot = miniMapPivot;
                rt.sizeDelta = miniMapSizeDelta;
                SetupMiniMap(true);
                miniMapCamera.aspect = miniMapCameraAspect;
            }

            UpdateMiniMap();
            UpdateMiniMapIcons();
        }

        public Vector3 GetMiniMapFollowPos () {
            return followPos + _miniMapFollowOffset;
        }

        public Vector3 GetMiniMapFullScreenWorldCenter () {
            return _miniMapFullScreenWorldCenter + _miniMapFollowOffset;
        }

        Vector2 GetMiniMapScreenPos (Vector3 poiPosition) {
            float aspectRatio = GetMiniMapAspectRatio();
            return GetMiniMapScreenPos(poiPosition, aspectRatio);
        }

        Vector2 GetMiniMapScreenPosNoShift (Vector3 poiPosition) {
            float aspectRatio = GetMiniMapAspectRatio();
            return GetMiniMapScreenPosNoShift(poiPosition, aspectRatio);
        }

        float GetMiniMapAspectRatio () {
            Vector2 rectSize = miniMapUIRootRT.rect.size;
            float aspectRatio = rectSize.y > 0 ? rectSize.x / rectSize.y : 1f;
            return aspectRatio;
        }

        Vector3 GetMiniMapScreenPos (Vector3 poiPosition, float aspectRatio) {
            Vector3 viewportPos = GetMiniMapScreenPosNoShift(poiPosition, aspectRatio);
            viewportPos.x += _miniMapIconPositionShift.x;
            viewportPos.y += _miniMapIconPositionShift.y;
            return viewportPos;
        }

        Vector3 GetMiniMapScreenPosNoShift (Vector3 poiPosition, float aspectRatio) {
            float zoomLevel = currentMiniMapZoomLevel;
            Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(poiPosition);
            viewportPos.x = (viewportPos.x - 0.5f) / zoomLevel + 0.5f;
            viewportPos.y = (viewportPos.y - 0.5f) / zoomLevel + 0.5f;
            viewportPos.y = (viewportPos.y - 0.5f) * aspectRatio + 0.5f;
            return viewportPos;
        }


        public Vector3 GetWorldPositionFromPointerEvent (Vector2 position) {
            Rect screenRect = miniMapUI.GetScreenRect();

            Vector2 uv;
            uv.x = (position.x - screenRect.xMin) / screenRect.width;
            uv.y = (position.y - screenRect.yMin) / screenRect.height;

            Vector3 worldPos = GetMiniMapWorldPositionFromUV(uv);
            return worldPos;
        }


        /// <summary>
        /// Returns the world position corresponding to a uv coordinate in the minimap
        /// </summary>
        public Vector3 GetMiniMapWorldPositionFromUV (Vector2 uv) {

            if (miniMapCamera == null) return Vector3.zero;

            float zoomLevel = currentMiniMapZoomLevel;
            uv.x = (uv.x - 0.5f) * zoomLevel + 0.5f;
            uv.y = (uv.y - 0.5f) * zoomLevel + 0.5f;

            Vector2 rectSize = miniMapUIRootRT.rect.size;
            float aspectRatio = rectSize.y / rectSize.x;
            uv.y = (uv.y - 0.5f) * aspectRatio + 0.5f;

            float dist = Mathf.Abs(followPos.y - miniMapCamera.transform.position.y);
            Vector3 wpos = miniMapCamera.ViewportToWorldPoint(new Vector3(uv.x, uv.y, dist));
            wpos.y = 0;
            return wpos;
        }


        #endregion

    }

    public static class MinimapContentsExtensions {
        public static bool usesTexture (this MiniMapContents contents) {
            return contents == MiniMapContents.UITexture || contents == MiniMapContents.Radar;
        }
    }

    public partial class CompassPro {

        protected virtual GameObject CreateMiniMapIcon(CompassProPOI poi) {
            GameObject prefab = poi.miniMapIconPrefabOverride != null ? poi.miniMapIconPrefabOverride : miniMapIconPrefab;
            GameObject iconGO = Instantiate(prefab);
            iconGO.name = "MiniMap Icon " + poi.name;
            iconGO.transform.SetParent(miniMapMaskUI.transform, false);
            return iconGO;
        }
    }


}



