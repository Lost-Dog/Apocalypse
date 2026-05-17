using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CompassNavigatorPro {

    public partial class CompassPro : MonoBehaviour {

        public ScanProfile scanProfile;

        /// <summary>
        /// Scan the world for POIs.
        /// </summary>
        /// <param name="includeDisabledPOIs">If true, disabled POIs will also be included in the scan.</param>
        /// <returns>The scan effect.</returns>
        public ScanEffect Scan (bool? includeDisabledPOIs = null) {
            if (scanProfile == null) {
                return Scan(color: default, onScanHit: OnScanHit, onScanEnd: OnScanEnd, detectDisabledPOIs: includeDisabledPOIs ?? true);
            }
            return Scan(scanProfile.color, scanProfile.center, scanProfile.maxRange, scanProfile.duration, scanProfile.fadeOutStart, scanProfile.fadeOutDuration, scanProfile.hitLayerMask,
            scanProfile.width, scanProfile.grid, scanProfile.gridColor, scanProfile.gridSize, scanProfile.gridDepthWidthControl, scanProfile.gridDepthStart, scanProfile.filled, scanProfile.colorBlend,
            scanProfile.scanLines, scanProfile.noise, scanProfile.edgeOutline, scanProfile.edgeColor, scanProfile.edgeDepthThreshold, scanProfile.edgeColorThreshold, scanProfile.edgeStrength, scanProfile.edgeWidth, scanProfile.audioClipStart, scanProfile.audioClipHit,
            OnScanHit, OnScanEnd, null, includeDisabledPOIs ?? scanProfile.detectDisabledPOIs);
        }

        /// <summary>
        /// Scan the world for POIs.
        /// </summary>
        /// <param name="pois">The list of POIs to scan.</param>
        /// <returns>The scan effect.</returns>
        public ScanEffect Scan (List<CompassProPOI> pois) {
            ScanEffect scan = Scan(scanProfile, OnScanHit, OnScanEnd, pois);
            return scan;
        }

        public static ScanEffect Scan (ScanProfile profile, UnityEvent<ScanEffect, CompassProPOI, Transform> onScanHit = null, UnityEvent<ScanEffect> onScanEnd = null, List<CompassProPOI> pois = null) {
            if (profile == null) {
                return Scan(color: default, onScanHit: onScanHit, onScanEnd: onScanEnd, pois: pois);
            }

            return Scan(profile.color, profile.center, profile.maxRange, profile.duration, profile.fadeOutStart, profile.fadeOutDuration, profile.hitLayerMask,
            profile.width, profile.grid, profile.gridColor, profile.gridSize, profile.gridDepthWidthControl, profile.gridDepthStart, profile.filled, profile.colorBlend,
            profile.scanLines, profile.noise, profile.edgeOutline, profile.edgeColor, profile.edgeDepthThreshold, profile.edgeColorThreshold, profile.edgeStrength, profile.edgeWidth, profile.audioClipStart, profile.audioClipHit,
            onScanHit, onScanEnd, pois, profile.detectDisabledPOIs);
        }

        public static ScanEffect Scan (Color color, Vector3 center = default, float maxRange = 800, float duration = 2f, float fadeOutStart = 2f, float fadeOutDuration = 1f, int scanLayerMask = -1,
        float width = 100f, bool grid = false, Color gridColor = default, float gridSize = 0.1f, float gridDepthWidthControl = 150f, float gridDepthStart = 100f, bool filled = false, Color colorBlend = default,
        bool scanLines = true, bool noise = true, bool edgeOutline = true, Color edgeColor = default, float edgeDepthThreshold = 0.01f, float edgeColorThreshold = 0.1f, float edgeStrength = 2f, float edgeWidth = 1f, AudioClip audioClipStart = null, AudioClip audioClipHit = null,
        UnityEvent<ScanEffect, CompassProPOI, Transform> onScanHit = null, UnityEvent<ScanEffect> onScanEnd = null, List<CompassProPOI> pois = null, bool detectDisabledPOIs = true) {
            ScanEffect scan = ScanEffect.GetInstanceFromPool();
            if (scan == null) return null;
            Camera cam = Camera.main;
            if (cam != null) {
                cam.depthTextureMode = DepthTextureMode.Depth;
                if (center == default) {
                    center = cam.transform.position;
                }
            }
            if (color == default) {
                color = new Color(0, 0.5f, 1f, 0.2f);
            }
            if (gridColor == default) {
                gridColor = new Color(1, 1, 0);
            }
            if (edgeColor == default) {
                edgeColor = Color.white;
            }
            scan.color = color;
            scan.center = center;
            scan.maxRange = maxRange;
            scan.duration = duration;
            scan.fadeOutStart = fadeOutStart;
            scan.fadeOutDuration = fadeOutDuration;
            scan.scanLayerMask = scanLayerMask;
            scan.width = width;
            scan.grid = grid;
            scan.gridColor = gridColor;
            scan.gridSize = gridSize;
            scan.gridDepthWidthControl = gridDepthWidthControl;
            scan.gridDepthStart = gridDepthStart;
            scan.filled = filled;
            scan.scanLines = scanLines;
            scan.noise = noise;
            scan.edgeOutline = edgeOutline;
            scan.edgeColor = edgeColor;
            scan.edgeDepthThreshold = edgeDepthThreshold;
            scan.edgeColorThreshold = edgeColorThreshold;
            scan.edgeStrength = edgeStrength;
            scan.edgeWidth = edgeWidth;
            scan.colorBlend = colorBlend;
            scan.audioClipStart = audioClipStart;
            scan.audioClipHit = audioClipHit;

            if (pois != null) {
                scan.pois.AddRange(pois);
            } else if (detectDisabledPOIs) {
                scan.pois.AddRange(Misc.FindObjectsOfType<CompassProPOI>(true));
                int poiCount = scan.pois.Count;
                for (int i = 0; i < poiCount; i++) {
                    CompassProPOI poi = scan.pois[i];
                    if (((1 << poi.gameObject.layer) & scanLayerMask) == 0) {
                        scan.pois.RemoveAt(i);
                        poiCount--;
                        i--;
                    }
                }
            } else {
                CompassPro compass = instance;
                if (compass == null) {
                    scan.pois.AddRange(Misc.FindObjectsOfType<CompassProPOI>(false));
                } else {
                    compass.POIGetAll(scan.pois);
                    int poiCount = scan.pois.Count;
                    for (int i = 0; i < poiCount; i++) {
                        CompassProPOI poi = scan.pois[i];
                        if (((1 << poi.gameObject.layer) & scanLayerMask) == 0) {
                            scan.pois.RemoveAt(i);
                            poiCount--;
                            i--;
                        }
                    }
                }
            }

            // Sort by distance to center
            scan.pois.Sort((a, b) => {
                float dA = (a.transform.position - center).sqrMagnitude;
                float dB = (b.transform.position - center).sqrMagnitude;
                return dA.CompareTo(dB);
            });

            if (onScanHit != null) {
                scan.OnScanHit.AddListener(onScanHit.Invoke);
            }
            if (onScanEnd != null) {
                scan.OnScanEnd.AddListener(onScanEnd.Invoke);
            }

            scan.ScanStart();

            return scan;
        }

    }

}



