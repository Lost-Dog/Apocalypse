using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CompassNavigatorPro {

    [ExecuteAlways]
    public partial class ScanEffect : MonoBehaviour {

        public Vector3 center;
        public Color color = new Color(0, 0.5f, 1f, 0.2f);
        public LayerMask scanLayerMask = -1;
        public float width = 100;
        public float maxRange = 800;
        public float duration = 2f;
        public float fadeOutStart = 2f;
        public float fadeOutDuration = 1f;
        public bool grid = true;
        public Color gridColor = new Color(1, 1, 0);
        public float gridSize = 0.1f;
        public float gridDepthWidthControl = 150;
        public float gridDepthStart = 100;
        public bool filled;
        public bool scanLines = true;
        public bool noise = true;
        public bool edgeOutline = true;
        public Color edgeColor = Color.white;
        public float edgeDepthThreshold = 0.01f;
        public float edgeColorThreshold = 0.1f;
        public float edgeStrength = 2f;
        public float edgeWidth = 1f;
        [ColorUsage(showAlpha: true, hdr: false)]
        public Color colorBlend = new Color(0, 0, 0, 0.75f);
        public AudioClip audioClipStart;
        public AudioClip audioClipHit;

        Material mat;

        private void OnEnable () {
            Renderer r = GetComponent<Renderer>();
            mat = Instantiate(r.sharedMaterial);
            r.sharedMaterial = mat;
        }

        private void OnDestroy () {
            RemoveInstanceFromPool(this);
            if (mat != null) {
                DestroyImmediate(mat);
            }
        }

        #region Public API

        public UnityEvent<ScanEffect, CompassProPOI, Transform> OnScanHit;
        public UnityEvent<ScanEffect> OnScanEnd;
        public readonly List<CompassProPOI> pois = new List<CompassProPOI>();

        public void Reset () {
            OnScanHit.RemoveAllListeners();
            OnScanEnd.RemoveAllListeners();
            pois.Clear();
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
        }

        public void ScanStart () {
            StopAllCoroutines();
            StartCoroutine(ScanExecute());
        }

        IEnumerator ScanExecute () {
            float start = Time.time;
            float t;

            if (audioClipStart != null) {
                AudioSource.PlayClipAtPoint(audioClipStart, center);
            }
            transform.position = center;

            mat.SetColor(ShaderParams.Color, color);
            mat.SetColor(ShaderParams.ColorBlend, colorBlend);
            mat.SetVector(ShaderParams.Center, center);
            mat.SetFloat(ShaderParams.Width, width);
            mat.SetFloat(ShaderParams.MaxRange, maxRange - width);
            mat.SetFloat(ShaderParams.Alpha, 1f);
            if (grid) {
                mat.EnableKeyword(ShaderParams.SKW_SCAN_GRID);
                mat.SetColor(ShaderParams.GridColor, gridColor);
                mat.SetVector(ShaderParams.GridData, new Vector4(gridSize, gridDepthWidthControl, gridDepthStart, 0));
            } else {
                mat.DisableKeyword(ShaderParams.SKW_SCAN_GRID);
            }
            mat.SetInt(ShaderParams.Filled, filled ? 1 : 0);
            mat.SetInt(ShaderParams.ScanLines, scanLines ? 1 : 0);
            mat.SetInt(ShaderParams.Noise, noise ? 1 : 0);

            if (edgeOutline) {
                mat.EnableKeyword(ShaderParams.SKW_SCAN_OUTLINE);
                mat.SetColor(ShaderParams.EdgeColor, edgeColor);
                mat.SetVector(ShaderParams.EdgeData, new Vector4(edgeDepthThreshold, edgeColorThreshold, edgeStrength, edgeWidth));
            } else {
                mat.DisableKeyword(ShaderParams.SKW_SCAN_OUTLINE);
            }

            int poiCount = pois.Count;
            int currentPOIIndex = 0;
            bool soundPlayed = false;
            do {
                float elapsed = Time.time - start;
                t = elapsed / duration;
                float radius = maxRange * t;
                transform.localScale = new Vector3(radius, radius, radius);

                while (currentPOIIndex < poiCount) {
                    CompassProPOI poi = pois[currentPOIIndex];
                    if (poi == null || ((1 << poi.gameObject.layer) & scanLayerMask) == 0) {
                        currentPOIIndex++;
                        continue;
                    }
                    float distance = Vector3.Distance(center, poi.transform.position);
                    if (distance <= radius) {
                        if (!soundPlayed) {
                            soundPlayed = true;
                            if (poi.scanHitAudioClip != null) {
                                AudioSource.PlayClipAtPoint(poi.scanHitAudioClip, poi.transform.position);
                            } else if (audioClipHit != null) {
                                AudioSource.PlayClipAtPoint(audioClipHit, poi.transform.position);
                            }
                        }
                        OnScanHit.Invoke(this, poi, transform);
                        currentPOIIndex++;
                    } else {
                        break;
                    }
                }

                if (fadeOutDuration > 0) {
                    float fadeT = (elapsed - fadeOutStart) / fadeOutDuration;
                    fadeT = Mathf.Clamp01(fadeT);
                    mat.SetFloat(ShaderParams.Alpha, 1f - fadeT);
                    if (fadeT >= 1) break;
                }
                yield return null;
            } while (t < 1f);

            if (fadeOutDuration > 0) {
                do {
                    if (gameObject == null) break;
                    float elapsed = Time.time - start;
                    t = (elapsed - fadeOutStart) / fadeOutDuration;
                    t = Mathf.Clamp01(t);
                    mat.SetFloat(ShaderParams.Alpha, 1f - t);
                    yield return null;
                } while (t < 1f);
            }

            ScanEnd();
        }

        void ScanEnd () {
            if (gameObject == null) return;
            OnScanHit.RemoveAllListeners();
            OnScanEnd.Invoke(this);
            OnScanEnd.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        #endregion

    }

}

