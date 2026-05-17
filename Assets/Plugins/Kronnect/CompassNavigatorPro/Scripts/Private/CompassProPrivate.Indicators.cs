using TMPro;
using UnityEngine;

namespace CompassNavigatorPro {

    public partial class CompassPro : MonoBehaviour {

        #region Indicators

        const string INDICATORS_ROOT_NAME = "OnScreen Indicators Root";
        const int MAX_STORED_VPOS = 100;
        readonly Vector3[] lastVPos = new Vector3[MAX_STORED_VPOS];
        bool needUpdateIndicators;

        void InitIndicators() {
            if (_onScreenIndicatorPrefab == null) {
                _onScreenIndicatorPrefab = Resources.Load<GameObject>("CNPro/Prefabs/POIGizmo");
            }

            if (indicatorsRoot == null) {
                indicatorsRoot = transform.Find(INDICATORS_ROOT_NAME);
                if (indicatorsRoot == null) {
                    GameObject root = Resources.Load<GameObject>("CNPro/Prefabs/OnScreenIndicatorsRoot");
                    if (root != null) {
                        GameObject rootGO = Instantiate(root, transform, false);
                        rootGO.name = INDICATORS_ROOT_NAME;
                        indicatorsRoot = rootGO.transform;
                    }
                }
            }
            indicatorsRoot.gameObject.SetActive(_showOnScreenIndicators || _showOffScreenIndicators);
        }

        void DisableIndicators() {
            if (indicatorsRoot != null) {
                indicatorsRoot.gameObject.SetActive(false);
            }
        }

        protected virtual void UpdateIndicators() {

            // Calculate effective aspect ratio considering viewport rect in Screen Space Overlay mode
            float aspect = _cameraMain.aspect;
            bool useViewportRect = _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay && 
                                   (_viewportRect.width < 1f || _viewportRect.height < 1f || _viewportRect.x > 0f || _viewportRect.y > 0f);
            if (useViewportRect && _viewportRect.width > 0 && _viewportRect.height > 0) {
                aspect *= _viewportRect.width / _viewportRect.height;
            }
            bool camIsOrthographic = _cameraMain.orthographic;

            float overlapDir = 1f;
            float distThreshold = _offScreenIndicatorOverlapDistance * 0.9f;

            // In Screen Space Overlay with viewport rect, increase margin to account for indicator size in smaller viewport
            float effectiveMargin = _offScreenIndicatorMargin;
            if (useViewportRect) {
                // Scale margin inversely with viewport size to maintain visual spacing
                float minViewportDim = Mathf.Min(_viewportRect.width, _viewportRect.height);
                if (minViewportDim > 0 && minViewportDim < 1f) {
                    effectiveMargin = _offScreenIndicatorMargin / minViewportDim;
                    effectiveMargin = Mathf.Min(effectiveMargin, 0.15f); // Cap to avoid too much margin
                }
            }

            float offScreenIndicatorRectWidth = _offScreenIndicatorRect.width;
            float offScreenIndicatorRectHeight = _offScreenIndicatorRect.height;
            float vextentsX = (0.5f - effectiveMargin) * offScreenIndicatorRectWidth;
            float vextentsY = (0.5f - effectiveMargin * aspect) * offScreenIndicatorRectHeight;
            float minX = _offScreenIndicatorRect.xMin + effectiveMargin;
            float maxX = _offScreenIndicatorRect.xMax - effectiveMargin;
            float minY = _offScreenIndicatorRect.yMin + effectiveMargin * aspect;
            float maxY = _offScreenIndicatorRect.yMax - effectiveMargin * aspect;
            float centerX = _offScreenIndicatorRect.center.x;
            float centerY = _offScreenIndicatorRect.center.y;

            Vector3 scaleVector = Misc.Vector3one;
            float scaleAnimSpeed = Time.deltaTime * _onScreenIndicatorScaleSpeed;
            int frameCount = Time.frameCount;
            int vPosCount = 0;
            int poiCount = activePOIs.Count;

            if (needUpdateIndicators) {
                needUpdateIndicators = false;
                for (int k = 0; k < poiCount; k++) {
                    CompassProPOI poi = activePOIs[k];
                    CompassProPOIState state = poi.GetState(_compassGroup);
                    state.lastIndicatorViewportPos.x = -1f;
                }
            }

            for (int k = 0; k < poiCount; k++) {
                CompassProPOI poi = activePOIs[k];
                CompassProPOIState state = poi.GetState(_compassGroup);

                if (!poi.isActiveAndEnabled) {
                    ToggleIndicatorVisibility(poi, state, false);
                    continue;
                }

                bool visible = !(_miniMapFullScreenState && _showMiniMap);
                if (state.isVisited && poi.hideWhenVisited) visible = false;

                if (!visible) {
                    ToggleIndicatorVisibility(poi, state, false);
                    continue;
                }

                // Update POI viewport position and distance
                if (frameCount != state.viewportPosFrameCount) {
                    state.viewportPosFrameCount = frameCount;
                    ComputePOIViewportPos(poi);
                }

                float maxVisibleDistance = poi.onScreenIndicatorFarDistance > 0 ? poi.onScreenIndicatorFarDistance : _onScreenIndicatorFarDistance;
                if (state.distanceToFollow > maxVisibleDistance) {
                    ToggleIndicatorVisibility(poi, state, false);
                    continue;
                }

                Vector3 vpos = state.viewportPos;

                bool isOnScreen = vpos.z > 0 && vpos.x >= minX && vpos.x < maxX && vpos.y >= minY && vpos.y < maxY;
                float ang = 0;
                float scale = 1f;

                if (isOnScreen) {
                    visible = _showOnScreenIndicators && poi.showOnScreenIndicator;
                    if (visible && state.isOnScreen >= 0) {
                        state.isOnScreen = -1;
                        OnPOIOnScreen.Invoke(poi);
                    }
                    scale = _onScreenIndicatorScale * poi.onScreenIndicatorScale;
                } else {
                    visible = _showOffScreenIndicators && poi.showOffScreenIndicator;
                    if (visible) {
                        if (state.isOnScreen <= 0) {
                            state.isOnScreen = 1;
                            OnPOIOffScreen.Invoke(poi);
                        }
                        scale = _offScreenIndicatorScale * poi.onScreenIndicatorScale;
                        vpos.x -= 0.5f;
                        vpos.y -= 0.5f;
                        if (vpos.z < 0) {
                            if (!camIsOrthographic) {
                                vpos *= -1f;
                            } else {
                                vpos.z = -vpos.z;
                            }
                            if (vpos.y > 0) vpos.y = -vpos.y; // when behind, always show indicator on the bottom half of the screen
                        }
                        ang = Mathf.Atan2(vpos.y, vpos.x);
                        float s = Mathf.Tan(ang);

                        float extentsX;
                        float extentsY;
                        if (poi.offScreenIndicatorMarginOverride != 0) {
                            float poiMargin = poi.offScreenIndicatorMarginOverride;
                            // Scale POI margin for viewport rect
                            if (useViewportRect) {
                                float minViewportDim = Mathf.Min(_viewportRect.width, _viewportRect.height);
                                if (minViewportDim > 0 && minViewportDim < 1f) {
                                    poiMargin = poiMargin / minViewportDim;
                                    poiMargin = Mathf.Min(poiMargin, 0.15f);
                                }
                            }
                            extentsX = (0.5f - poiMargin) * offScreenIndicatorRectWidth;
                            extentsY = (0.5f - poiMargin * aspect) * offScreenIndicatorRectHeight;
                        } else {
                            extentsX = vextentsX;
                            extentsY = vextentsY;
                        }

                        if (vpos.x > 0) {
                            vpos.x = extentsX;
                            vpos.y = extentsX * s;
                        } else {
                            vpos.x = -extentsX;
                            vpos.y = -extentsX * s;
                        }
                        if (vpos.y > extentsY) {
                            vpos.x = extentsY / s;
                            vpos.y = extentsY;
                        } else if (vpos.y < -extentsY) {
                            vpos.x = -extentsY / s;
                            vpos.y = -extentsY;
                        }

                        // check collision
                        if (_offScreenIndicatorAvoidOverlap && vPosCount < MAX_STORED_VPOS) {
                            float disp = 0;
                            bool vert = vpos.x * vpos.x > vpos.y * vpos.y;
                            int maxj = Mathf.Min(vPosCount, MAX_STORED_VPOS);
                            if (vert) {
                                for (int j = 0; j < maxj; j++) {
                                    float dx = lastVPos[j].x - vpos.x;
                                    if (dx < 0) dx = -dx;
                                    float dy = lastVPos[j].y - vpos.y;
                                    if (dy < 0) dy = -dy;
                                    if (dx < distThreshold && dy < distThreshold) {
                                        if (disp <= 0) {
                                            vpos = lastVPos[j];
                                            disp = _offScreenIndicatorOverlapDistance * overlapDir;
                                        }
                                        vpos.y += disp;
                                        if (vpos.y < -0.4f || vpos.y > 0.4f) break;
                                        j = -1;
                                    }
                                }
                            } else {
                                for (int j = 0; j < maxj; j++) {
                                    float dx = lastVPos[j].x - vpos.x;
                                    if (dx < 0) dx = -dx;
                                    float dy = lastVPos[j].y - vpos.y;
                                    if (dy < 0) dy = -dy;
                                    if (dx < distThreshold && dy < distThreshold) {
                                        if (disp <= 0) {
                                            vpos = lastVPos[j];
                                            disp = _offScreenIndicatorOverlapDistance * overlapDir;
                                        }
                                        vpos.x += disp;
                                        if (vpos.x < -0.4f || vpos.x > 0.4f) break;
                                        j = -1;
                                    }
                                }
                            }
                            overlapDir = -overlapDir;
                            lastVPos[vPosCount++] = vpos;
                        }

                        vpos.x += centerX;
                        vpos.y += centerY;
                    }
                }

                if (state.indicatorImage != null) {
                    ToggleIndicatorVisibility(poi, state, visible);
                    if (!visible) continue;
                } else {
                    if (!visible) continue;

                    // Add a dummy child gameObject
                    GameObject go = CreateIndicator(poi);
                    if (go == null) continue;
                    state.indicatorRT = go.GetComponent<RectTransform>();
                    state.indicatorCanvasGroup = go.GetComponent<CanvasGroup>();
                    GizmoElements elements = go.GetComponentInChildren<GizmoElements>();
                    if (elements == null) {
                        Debug.LogError("Gizmo prefab missing GizmoElements component.");
                        DestroyImmediate(go);
                        continue;
                    }
                    state.indicatorImage = elements.iconImage;
                    state.indicatorDistanceText = elements.distanceText;
                    state.indicatorTitleText = elements.titleText;
                    state.indicatorArrowRT = elements.arrowPivot;
                    state.indicatorRT.localScale = Misc.Vector3zero;
                    OnIndicatorCreated?.Invoke(poi, go);
                }

                RectTransform t = state.indicatorRT;
                scaleVector.x = scaleVector.y = scale;
                Vector3 newScale = Vector3.Lerp(t.localScale, scaleVector, scaleAnimSpeed);
                t.localScale = newScale;

                if (state.lastIndicatorViewportPos == vpos) continue;
                state.lastIndicatorViewportPos = vpos;

                // Convert local viewport coords back to screen coords for UI positioning (Screen Space Overlay only)
                Vector2 anchorPos;
                if (useViewportRect) {
                    anchorPos.x = _viewportRect.x + vpos.x * _viewportRect.width;
                    anchorPos.y = _viewportRect.y + vpos.y * _viewportRect.height;
                    // Clamp to viewport bounds to prevent any overflow
                    anchorPos.x = Mathf.Clamp(anchorPos.x, _viewportRect.xMin + 0.01f, _viewportRect.xMax - 0.01f);
                    anchorPos.y = Mathf.Clamp(anchorPos.y, _viewportRect.yMin + 0.01f, _viewportRect.yMax - 0.01f);
                } else {
                    anchorPos.x = vpos.x;
                    anchorPos.y = vpos.y;
                }
                state.indicatorRT.anchorMin = state.indicatorRT.anchorMax = anchorPos;
                state.indicatorImage.sprite = state.isVisited && poi.iconVisited != null ? poi.iconVisited : poi.iconNonVisited;
                bool distanceVisible = (isOnScreen && poi.onScreenIndicatorShowDistance && _onScreenIndicatorShowDistance) || 
                                     (!isOnScreen && poi.offScreenIndicatorShowDistance && _offScreenIndicatorShowDistance);
                if (state.indicatorDistanceText.isActiveAndEnabled != distanceVisible) {
                    state.indicatorDistanceText.gameObject.SetActive(distanceVisible);
                }
                bool titleVisible = isOnScreen && poi.onScreenIndicatorShowTitle && _onScreenIndicatorShowTitle;
                if (state.indicatorTitleText.isActiveAndEnabled != titleVisible) {
                    state.indicatorTitleText.gameObject.SetActive(titleVisible);
                }

                float iconAlpha;
                if (isOnScreen) {
                    float nearFadeMin = poi.onScreenIndicatorNearFadeMin > 0 ? poi.onScreenIndicatorNearFadeMin : _onScreenIndicatorNearFadeMin;
                    float nearFadeDistance = poi.onScreenIndicatorNearFadeDistance > 0 ? poi.onScreenIndicatorNearFadeDistance : _onScreenIndicatorNearFadeDistance;
                    float farFadeDistance = poi.onScreenIndicatorFarFadeDistance > 0 ? poi.onScreenIndicatorFarFadeDistance : _onScreenIndicatorFarFadeDistance;

                    // Calculate near fade factor (1 at nearFadeDistance, 0 at nearFadeMin)
                    float nearFadeFactor = nearFadeDistance <= nearFadeMin ? 1f : Mathf.Clamp01((state.distanceToFollow - nearFadeMin) / (nearFadeDistance - nearFadeMin));
                    
                    // Calculate far fade factor (1 at farFadeDistance, 0 at maxVisibleDistance)
                    float farFadeFactor = farFadeDistance >= maxVisibleDistance ? 1f : Mathf.Clamp01((maxVisibleDistance - state.distanceToFollow) / (maxVisibleDistance - farFadeDistance));
                    
                    // Combine both fade factors
                    float gizmoAlphaFactor = nearFadeFactor * farFadeFactor;
                    iconAlpha = _onScreenIndicatorAlpha * gizmoAlphaFactor;

                    if (poi.onScreenIndicatorShowDistance && _onScreenIndicatorShowDistance) {
                        if (state.prevIndicatorDistance != state.distanceToFollow) {
                            state.prevIndicatorDistance = state.distanceToFollow;
                            state.lastIndicatorDistanceText = state.distanceToFollow.ToString(_onScreenIndicatorShowDistanceFormat);
                            state.indicatorDistanceText.text = state.lastIndicatorDistanceText;
                        }
                    }

                    if (poi.onScreenIndicatorShowTitle && _onScreenIndicatorShowTitle) {
                        if (!state.indicatorTitleText.enabled) {
                            state.indicatorTitleText.enabled = true;
                        }
                        state.indicatorTitleText.text = poi.title;
                        if (vpos.x > 0.85f) {
                            state.indicatorTitleText.alignment = TextAlignmentOptions.MidlineRight;
                        } else if (vpos.x < 0.15f) {
                            state.indicatorTitleText.alignment = TextAlignmentOptions.MidlineLeft;
                        } else {
                            state.indicatorTitleText.alignment = TextAlignmentOptions.Midline;
                        }
                    }
                } else {
                    iconAlpha = _offScreenIndicatorAlpha;
                    state.indicatorArrowRT.localRotation = Quaternion.Euler(0, 0, ang * Mathf.Rad2Deg);
                    if (poi.offScreenIndicatorShowDistance && _offScreenIndicatorShowDistance) {
                        if (state.prevIndicatorDistance != state.distanceToFollow) {
                            state.prevIndicatorDistance = state.distanceToFollow;
                            state.lastIndicatorDistanceText = state.distanceToFollow.ToString(_offScreenIndicatorShowDistanceFormat);
                            state.indicatorDistanceText.text = state.lastIndicatorDistanceText;
                        }
                    }
                }

                state.indicatorImage.color = poi.tintColor;
                state.indicatorCanvasGroup.alpha = iconAlpha;
                state.indicatorArrowRT.gameObject.SetActive(!isOnScreen);
            }
        }

        protected virtual GameObject CreateIndicator(CompassProPOI poi) {
            GameObject prefab = poi.onScreenIndicatorPrefabOverride != null ? poi.onScreenIndicatorPrefabOverride : _onScreenIndicatorPrefab;
            GameObject indicatorGO = Instantiate(prefab, indicatorsRoot, false);
            return indicatorGO;
        }
        #endregion

    }

}

