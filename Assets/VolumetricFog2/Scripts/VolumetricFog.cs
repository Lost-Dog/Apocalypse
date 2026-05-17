//#define FOG_ROTATION

//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist 2
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricFogAndMist2 {

    public delegate void OnUpdateMaterialPropertiesEvent (VolumetricFog fogVolume);

    public enum VolumetricFogFollowMode {
        FullXYZ = 0,
        RestrictToXZPlane = 1
    }

    public enum VolumetricFogUpdateMode {
        WhenFogVolumeIsVisible = 1,
        WhenCameraIsInsideArea = 2
    }

    [ExecuteInEditMode]
    [DefaultExecutionOrder(100)]
    [HelpURL("https://kronnect.com/docs/volumetric-fog-urp/")]
    public partial class VolumetricFog : MonoBehaviour {

        public VolumetricFogProfile profile;

        public event OnUpdateMaterialPropertiesEvent OnUpdateMaterialProperties;

        [Tooltip("Supports Unity native lights including point and spot lights.")]
        public bool enableNativeLights;
        [Tooltip("Multiplier to native lights intensity")]
        public float nativeLightsMultiplier = 1f;
        [Tooltip("Native lights distance attenuation falloff (0 = no attenuation, 1 = full attenuation).")]
        [Range(0f, 1f)]
        public float nativeLightFallOff = 1f;
        [Tooltip("Enable fast point lights. This option is much faster than native lights. However, if you enable native lights, this option can't be enabled as point lights are already included in the native lights support.")]
        public bool enablePointLights;
        [Tooltip("Supports Adaptative Probe Volumes (Unity 2023.1+)")]
        public bool enableAPV;
        [Tooltip("Multiplier to native lights intensity")]
        public float apvIntensityMultiplier = 1f;
        public bool enableVoids;
        [Tooltip("Makes this fog volume follow another object automatically")]
        public bool enableFollow;
        public Transform followTarget;
        public VolumetricFogFollowMode followMode = VolumetricFogFollowMode.RestrictToXZPlane;
        public bool followIncludeDistantFog;
        public Vector3 followOffset;
        [Tooltip("Fades in/out fog effect when reference controller enters the fog volume.")]
        public bool enableFade;
        [Tooltip("Also fades distant fog along with this volume.")]
        public bool fadeIncludeDistantFog;
        [Tooltip("Fog volume blending starts when reference controller is within this fade distance to any volume border.")]
        public float fadeDistance = 1;
        [Tooltip("If this option is disabled, the fog disappears when the reference controller exits the volume and appears when the controller enters the volume. Enable this option to fade out the fog volume when the controller enters the volume. ")]
        public bool fadeOut;
        [Tooltip("The controller (player or camera) to check if enters the fog volume.")]
        public Transform fadeController;
        [Tooltip("Enable sub-volume blending.")]
        public bool enableSubVolumes;
        [Tooltip("Allowed subVolumes. If no subvolumes are specified, any subvolume entered by this controller will affect this fog volume.")]
        public List<VolumetricFogSubVolume> subVolumes;
        [Tooltip("Customize how this fog volume data is updated and animated")]
        public bool enableUpdateModeOptions;
        public VolumetricFogUpdateMode updateMode = VolumetricFogUpdateMode.WhenFogVolumeIsVisible;
        [Tooltip("Camera used to compute visibility of this fog volume. If not set, the system will use the main camera.")]
        public Camera updateModeCamera;
        public Bounds updateModeBounds = new Bounds(Vector3.zero, Vector3.one * 100);
        [Tooltip("Shows the fog volume boundary in Game View")]
        public bool showBoundary;

        [NonSerialized]
        public MeshRenderer meshRenderer;
        MeshFilter mf;
        Material fogMat, noiseMat, turbulenceMat;
        Shader fogShader;
        RenderTexture rtNoise, rtTurbulence;
        float turbAcum;
        Vector4 windAcum, detailNoiseWindAcum, distantFogNoiseWindAcum;
        Vector3 sunDir;
        float dayLight, moonLight;
        Texture3D detailTex, refDetailTex;
        Mesh debugMesh;
        Material fogDebugMat;
        VolumetricFogProfile activeProfile, lerpProfile;
        Vector3 lastControllerPosition;
        float alphaMultiplier = 1f;
        Material distantFogMat;

        bool profileIsInstanced;
        bool requireUpdateMaterial;
        ColorSpace currentAppliedColorSpace;
        static Texture2D blueNoiseTex;
        Color ambientMultiplied;

        float lastVolumeHeight;
        Bounds cachedBounds;
        const float DISTANT_FOG_FAR_PLANE = 50000;
        const int TRANSPARENT_RENDER_QUEUE = 3000;
        static Vector3 distantFogBounds = new Vector3(DISTANT_FOG_FAR_PLANE, DISTANT_FOG_FAR_PLANE, DISTANT_FOG_FAR_PLANE);

        /// <summary>
        /// This property will return an instanced copy of the profile and use it for this volumetric fog from now on. Works similarly to Unity's material vs sharedMaterial.
        /// </summary>
        public VolumetricFogProfile settings {
            get {
                if (!profileIsInstanced && profile != null) {
                    profile = Instantiate(profile);
                    profileIsInstanced = true;
                }
                requireUpdateMaterial = true;
                return profile;
            }
            set {
                profile = value;
                profileIsInstanced = false;
                requireUpdateMaterial = true;
            }
        }

        [NonSerialized]
        public bool forceTerrainCaptureUpdate;

        [NonSerialized]
        public uint renderingLayerMaskCopy;

        public readonly static List<VolumetricFog> volumetricFogs = new List<VolumetricFog>();

        public Material material => fogMat;



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init () {
            volumetricFogs.Clear();
        }


        void OnEnable () {
            volumetricFogs.Add(this);
            Tools.CheckMainManager();
            FogOfWarInit();
            CheckSurfaceCapture();
            UpdateMaterialPropertiesNow();
        }

        void OnDisable () {
            if (volumetricFogs.Contains(this)) volumetricFogs.Remove(this);
            if (profile != null) {
                profile.onSettingsChanged -= UpdateMaterialProperties;
            }
        }

        void OnDidApplyAnimationProperties () {  // support for animating property based fields
            UpdateMaterialProperties();
        }

        void OnValidate () {
            nativeLightsMultiplier = Mathf.Max(0, nativeLightsMultiplier);
            nativeLightFallOff = Mathf.Clamp01(nativeLightFallOff);
            apvIntensityMultiplier = Mathf.Max(0, apvIntensityMultiplier);
            UpdateMaterialProperties();
        }

        void OnDestroy () {
            if (rtNoise != null) {
                rtNoise.Release();
            }
            if (rtTurbulence != null) {
                rtTurbulence.Release();
            }
            if (fogMat != null) {
                DestroyImmediate(fogMat);
                fogMat = null;
            }
            if (distantFogMat != null) {
                DestroyImmediate(distantFogMat);
                distantFogMat = null;
            }
            FogOfWarDestroy();
            DisposeSurfaceCapture();
        }

        void OnDrawGizmosSelected () {
            if (enableFogOfWar && fogOfWarShowCoverage) {
                Gizmos.color = new Color(1, 0, 0, 0.75F);
                Vector3 position = anchoredFogOfWarCenter;
                position.y = transform.position.y;
                Vector3 size = fogOfWarSize;
                size.y = transform.localScale.y;
                Gizmos.DrawWireCube(position, size);
            }

            if (enableUpdateModeOptions && updateMode == VolumetricFogUpdateMode.WhenCameraIsInsideArea) {
                Gizmos.color = new Color(0, 1, 0, 0.75F);
                Gizmos.DrawWireCube(updateModeBounds.center, updateModeBounds.size);
            }

            Gizmos.color = new Color(1, 1, 0, 0.75F);
            // Gizmos.matrix =  transform.localToWorldMatrix;
            // Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Bounds bounds = GetBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        public Bounds GetBounds () {
            if (meshRenderer != null) {
                // Get the mesh bounds in world space
                return meshRenderer.bounds;
            }
            return new Bounds(transform.position, transform.lossyScale);
        }

        public void SetBounds (Bounds bounds) {
            transform.position = bounds.center;
            transform.localScale = bounds.size;
        }

        void LateUpdate () {
            if (fogMat == null || meshRenderer == null || profile == null) return;

            if (enableUpdateModeOptions && !CanUpdate()) return;

            CleanupFogMats();

            if (requireUpdateMaterial) {
                requireUpdateMaterial = false;
                UpdateMaterialPropertiesNow();
            }

#if FOG_ROTATION
            Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
            foreach (var mat in fogMats) {
                mat.SetMatrix(ShaderParams.RotationMatrix, rot);
                mat.SetMatrix(ShaderParams.RotationInvMatrix, rot.inverse);
            }
#else
            transform.rotation = Quaternion.identity;
#endif

            ComputeActiveProfile();

            if (activeProfile.customHeight) {
                Vector3 scale = transform.localScale;
                if (activeProfile.height == 0) {
                    activeProfile.height = scale.y;
                }
                if (scale.y != activeProfile.height) {
                    scale.y = activeProfile.height;
                    transform.localScale = scale;
                }
            }

            if (activeProfile.scaleNoiseWithHeight > 0 && lastVolumeHeight != transform.localScale.y) {
                ApplyProfileSettings();
            }

            if (enableFollow && followTarget != null) {
                Vector3 position = followTarget.position;
                if (followMode == VolumetricFogFollowMode.RestrictToXZPlane) {
                    position.y = transform.position.y;
                }
                transform.position = position + followOffset;
            }

            Bounds bounds = GetBounds();
            Vector3 center = bounds.center;
            Vector3 extents = transform.lossyScale * 0.5f; // required to account for non-uniform scaling & rotation; don't use bounds.extents

            bool requireApplyProfileSettings = enableFade || enableSubVolumes;
#if UNITY_EDITOR
            if (currentAppliedColorSpace != QualitySettings.activeColorSpace) {
                requireApplyProfileSettings = true;
            }
#endif
            if (requireApplyProfileSettings) {
                ApplyProfileSettings();
            }

            if (activeProfile.shape == VolumetricFogShape.Sphere) {
                Vector3 scale = transform.localScale;
                if (scale.z != scale.x) {
                    scale.z = scale.x;
                    transform.localScale = scale;
                    extents = transform.lossyScale * 0.5f;
                }
                extents.x *= extents.x;
            }

            Vector4 border = new Vector4(extents.x * activeProfile.border + 0.0001f, extents.x * (1f - activeProfile.border), extents.z * activeProfile.border + 0.0001f, extents.z * (1f - activeProfile.border));
            if (activeProfile.terrainFit) {
                extents.y = Mathf.Max(extents.y, activeProfile.terrainFogHeight);
            }
            Vector4 boundsData = new Vector4(activeProfile.verticalOffset, center.y - extents.y, extents.y * 2f, 0);

            Color ambientColor = RenderSettings.ambientLight;
            float ambientIntensity = RenderSettings.ambientIntensity;
            ambientMultiplied = ambientColor * ambientIntensity;

            VolumetricFogManager globalManager = VolumetricFogManager.instance;
            Light sun = globalManager.sun;
            Color lightColor;
            Color sunColor;
            float sunIntensity;

            if (sun != null) {
                if (activeProfile.dayNightCycle) {
                    sunDir = -sun.transform.forward;
                    sunColor = sun.color;
                    if (sun.useColorTemperature) {
                        sunColor *= Mathf.CorrelatedColorTemperatureToRGB(sun.colorTemperature);
                    }
                    sunIntensity = sun.intensity;
                } else {
                    sunDir = activeProfile.sunDirection.normalized;
                    sunColor = activeProfile.sunColor;
                    sunIntensity = activeProfile.sunIntensity;
                }
            } else {
                sunDir = activeProfile.sunDirection.normalized;
                sunColor = Color.white;
                sunIntensity = 1f;
            }

            dayLight = 1f + sunDir.y * 2f;
            if (dayLight < 0) dayLight = 0; else if (dayLight > 1f) dayLight = 1f;
            float brightness = activeProfile.brightness;

            float colorSpaceMultiplier = QualitySettings.activeColorSpace == ColorSpace.Gamma ? 2f : 1.33f;
            lightColor = sunColor * (dayLight * sunIntensity * brightness * colorSpaceMultiplier);

            Light moon = globalManager.moon;
            moonLight = 0;
            if (activeProfile.dayNightCycle && !enableNativeLights && moon != null) {
                Vector3 moonDir = -moon.transform.forward;
                moonLight = 1f + moonDir.y * 2f;
                if (moonLight < 0) moonLight = 0; else if (moonLight > 1f) moonLight = 1f;
                Color moonColor = moon.color;
                if (moon.useColorTemperature) {
                    moonColor *= Mathf.CorrelatedColorTemperatureToRGB(moon.colorTemperature);
                }
                lightColor += moonColor * (moonLight * moon.intensity * brightness * colorSpaceMultiplier);
            }

            lightColor.a = activeProfile.albedo.a;

            if (enableFade && fadeOut && Application.isPlaying) {
                lightColor.a *= 1f - alphaMultiplier;
            } else {
                lightColor.a *= alphaMultiplier;
            }

            lightColor.r += ambientMultiplied.r * activeProfile.ambientLightMultiplier;
            lightColor.g += ambientMultiplied.g * activeProfile.ambientLightMultiplier;
            lightColor.b += ambientMultiplied.b * activeProfile.ambientLightMultiplier;

            meshRenderer.enabled = activeProfile.density > 0 && lightColor.a > 0;

            float deltaTime = Time.deltaTime;
            windAcum.x += activeProfile.windDirection.x * deltaTime;
            windAcum.y += activeProfile.windDirection.y * deltaTime;
            windAcum.z += activeProfile.windDirection.z * deltaTime;
            windAcum.x %= 10000;
            windAcum.y %= 10000;
            windAcum.z %= 10000;

            Vector4 detailWindDirection = windAcum;
            if (activeProfile.useCustomDetailNoiseWindDirection) {
                detailNoiseWindAcum.x += activeProfile.detailNoiseWindDirection.x * deltaTime;
                detailNoiseWindAcum.y += activeProfile.detailNoiseWindDirection.y * deltaTime;
                detailNoiseWindAcum.z += activeProfile.detailNoiseWindDirection.z * deltaTime;
                detailNoiseWindAcum.x %= 10000;
                detailNoiseWindAcum.y %= 10000;
                detailNoiseWindAcum.z %= 10000;
                detailWindDirection = detailNoiseWindAcum;
            } else {
                detailWindDirection = windAcum;
            }

            foreach (var mat in fogMats) {
                mat.SetVector(ShaderParams.BoundsCenter, center);
                mat.SetVector(ShaderParams.BoundsExtents, extents);
                mat.SetVector(ShaderParams.BoundsBorder, border);
                mat.SetVector(ShaderParams.BoundsData, boundsData);
                mat.SetVector(ShaderParams.SunDir, sunDir);
                mat.SetVector(ShaderParams.LightColor, lightColor);
                mat.SetVector(ShaderParams.WindDirection, windAcum);
                mat.SetVector(ShaderParams.DetailWindDirection, detailWindDirection);
            }

            UpdateNoise();

            if (enableFogOfWar) {
                UpdateFogOfWar();
            }

            if (showBoundary) {
                if (fogDebugMat == null) {
                    fogDebugMat = new Material(Shader.Find("Hidden/VolumetricFog2/VolumeDebug"));
                }
                if (debugMesh == null) {
                    if (mf != null) {
                        debugMesh = mf.sharedMesh;
                    }
                }
                Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Graphics.DrawMesh(debugMesh, m, fogDebugMat, 0);
            }

            if (enablePointLights && !enableNativeLights) {
                PointLightManager.usingPointLights = true;
            }

            if (enableVoids) {
                FogVoidManager.usingVoids = true;
            }

            if (activeProfile.terrainFit) {
                SurfaceCaptureUpdate();
            }

            if (activeProfile.distantFog && ShouldRenderDistantFogInEditMode()) {
                if (GetDistantFogFadeAlpha() <= 0f) return;
                if (mf != null && distantFogMat != null) {
                    distantFogMat.SetVector(ShaderParams.SunDir, sunDir);
                    distantFogMat.SetVector(ShaderParams.LightColor, lightColor);
                    float baseAltitude = activeProfile.distantFogBaseAltitude;
                    if (enableFollow && followIncludeDistantFog && followMode == VolumetricFogFollowMode.FullXYZ && followTarget != null) {
                        baseAltitude += followTarget.position.y + followOffset.y;
                    }
                    distantFogMat.SetVector(ShaderParams.DistantFogData2, new Vector4(baseAltitude, activeProfile.distantFogSymmetrical ? -1e6f : 1, 0, 0));
                    if (activeProfile.distantFogNoise) {
                        distantFogMat.EnableKeyword(ShaderParams.SKW_DISTANT_FOG_NOISE);
                        distantFogMat.SetTexture(ShaderParams.DistantFogNoiseTexture, activeProfile.distantFogNoiseTexture);
                        distantFogMat.SetVector(ShaderParams.DistantFogDistanceNoiseData, new Vector4(
                            activeProfile.distantFogDistanceNoiseScale * 0.01f,
                            activeProfile.distantFogDistanceNoiseStrength,
                            activeProfile.distantFogDistanceNoiseMaxDistance,
                            0
                        ));
                        // Accumulate distant fog noise wind direction
                        distantFogNoiseWindAcum.x += activeProfile.distantFogNoiseWindDirection.x * deltaTime;
                        distantFogNoiseWindAcum.y += activeProfile.distantFogNoiseWindDirection.y * deltaTime;
                        distantFogNoiseWindAcum.z += activeProfile.distantFogNoiseWindDirection.z * deltaTime;
                        distantFogNoiseWindAcum.x %= 10000;
                        distantFogNoiseWindAcum.y %= 10000;
                        distantFogNoiseWindAcum.z %= 10000;
                        distantFogMat.SetVector(ShaderParams.DistantFogNoiseWind, distantFogNoiseWindAcum);
                    } else {
                        distantFogMat.DisableKeyword(ShaderParams.SKW_DISTANT_FOG_NOISE);
                    }
                    bool useTransparencySupport = VolumetricFogRenderFeature.isUsingDepthPeeling && activeProfile.distantFogTransparencySupport;
                    if (!useTransparencySupport && !VolumetricFogRenderFeature.isRenderingBeforeTransparents) {
                        int renderQueue = activeProfile.distantFogRenderQueue;
                        if (activeProfile.distantFogTransparencySupport && renderQueue < 3001) {
                            renderQueue = 3001;
                        }
                        distantFogMat.renderQueue = renderQueue;
                        Matrix4x4 m = Matrix4x4.TRS(transform.position, Quaternion.identity, distantFogBounds);
                        Graphics.DrawMesh(mf.sharedMesh, m, distantFogMat, gameObject.layer);
                    }
                }
            }
        }


        public bool DistantFogUsesTransparencySupport {
            get {
                return activeProfile != null && activeProfile.distantFog && activeProfile.distantFogTransparencySupport && ShouldRenderDistantFogInEditMode();
            }
        }

        public bool ShouldRenderDistantFogOverTransparents {
            get {
                return DistantFogUsesTransparencySupport;
            }
        }

        public void RenderDistantFog (CommandBuffer cmd) {
            if (mf == null || distantFogMat == null || activeProfile == null || !activeProfile.distantFog || !ShouldRenderDistantFogInEditMode()) return;
            if (GetDistantFogFadeAlpha() <= 0f) return;
            Matrix4x4 m = Matrix4x4.TRS(transform.position, Quaternion.identity, distantFogBounds);
            UpdateDistantFogPropertiesNow();
            cmd.DrawMesh(mf.sharedMesh, m, distantFogMat);
        }

        bool ShouldRenderDistantFogInEditMode () {
#if UNITY_EDITOR
            if (!Application.isPlaying && activeProfile != null && !activeProfile.distantFogShowInEditMode) return false;
#endif
            return true;
        }

        Bounds cameraFrustumBounds;
        static readonly Vector3[] frustumVertices = new Vector3[8];
        Vector3 cameraFrustumLastPosition;
        Quaternion cameraFrustumLastRotation;


        bool CanUpdate () {

#if UNITY_EDITOR
            if (!Application.isPlaying) return true;
#endif

            Camera cam = updateModeCamera;
            if (cam == null) {
                cam = Camera.main;
                if (cam == null) return true;
            }

            bool isVisible;
            Transform camTransform = cam.transform;
            Vector3 camPos = camTransform.position;
            if (updateMode == VolumetricFogUpdateMode.WhenFogVolumeIsVisible) {
                Quaternion camRot = camTransform.rotation;
                if (camPos != cameraFrustumLastPosition || camRot != cameraFrustumLastRotation) {
                    cameraFrustumLastPosition = camPos;
                    cameraFrustumLastRotation = camRot;
                    CalculateFrustumBounds(cam);
                }
                if (transform.hasChanged || cachedBounds.size.x == 0) {
                    cachedBounds = meshRenderer.bounds;
                    transform.hasChanged = false;
                }
                isVisible = cameraFrustumBounds.Intersects(cachedBounds);
            } else {
                isVisible = updateModeBounds.Contains(camPos);
            }
            return isVisible;
        }

        void CalculateFrustumBounds (Camera camera) {
            CalculateFrustumVertices(camera);
            cameraFrustumBounds = new Bounds(frustumVertices[0], Vector3.zero);
            for (int k = 1; k < 8; k++) {
                cameraFrustumBounds.Encapsulate(frustumVertices[k]);
            }
        }

        void CalculateFrustumVertices (Camera cam) {
            float nearClipPlane = cam.nearClipPlane;
            frustumVertices[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, nearClipPlane));
            frustumVertices[1] = cam.ViewportToWorldPoint(new Vector3(0, 1, nearClipPlane));
            frustumVertices[2] = cam.ViewportToWorldPoint(new Vector3(1, 0, nearClipPlane));
            frustumVertices[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, nearClipPlane));
            float farClipPlane = cam.farClipPlane;
            frustumVertices[4] = cam.ViewportToWorldPoint(new Vector3(0, 0, farClipPlane));
            frustumVertices[5] = cam.ViewportToWorldPoint(new Vector3(0, 1, farClipPlane));
            frustumVertices[6] = cam.ViewportToWorldPoint(new Vector3(1, 0, farClipPlane));
            frustumVertices[7] = cam.ViewportToWorldPoint(new Vector3(1, 1, farClipPlane));
        }

        void UpdateNoise () {

            if (activeProfile == null) return;
            Texture noiseTex = activeProfile.noiseTexture;
            if (noiseTex == null) return;

            float fogIntensity = 1.15f;
            fogIntensity *= dayLight + moonLight;
            Color textureBaseColor = Color.Lerp(ambientMultiplied, activeProfile.albedo * fogIntensity, fogIntensity);

            if (!activeProfile.constantDensity) {
                if (rtTurbulence == null || rtTurbulence.width != noiseTex.width) {
                    RenderTextureDescriptor desc = new RenderTextureDescriptor(noiseTex.width, noiseTex.height, RenderTextureFormat.ARGB32, 0);
                    rtTurbulence = new RenderTexture(desc);
                    rtTurbulence.wrapMode = TextureWrapMode.Repeat;
                }
                turbAcum += Time.deltaTime * activeProfile.turbulence;
                turbAcum %= 10000;
                turbulenceMat.SetFloat(ShaderParams.TurbulenceAmount, turbAcum);
                turbulenceMat.SetFloat(ShaderParams.NoiseStrength, activeProfile.noiseStrength);
                turbulenceMat.SetFloat(ShaderParams.NoiseFinalMultiplier, activeProfile.noiseFinalMultiplier);
                Graphics.Blit(noiseTex, rtTurbulence, turbulenceMat);

                int noiseSize = Mathf.Min(noiseTex.width, (int)activeProfile.noiseTextureOptimizedSize);
                if (rtNoise == null || rtNoise.width != noiseSize) {
                    RenderTextureDescriptor desc = new RenderTextureDescriptor(noiseSize, noiseSize, RenderTextureFormat.ARGB32, 0);
                    rtNoise = new RenderTexture(desc);
                    rtNoise.wrapMode = TextureWrapMode.Repeat;
                }
                noiseMat.SetColor(ShaderParams.SpecularColor, activeProfile.specularColor);
                noiseMat.SetFloat(ShaderParams.SpecularIntensity, activeProfile.specularIntensity);

                float spec = 1.0001f - activeProfile.specularThreshold;
                float nlighty = sunDir.y > 0 ? (1.0f - sunDir.y) : (1.0f + sunDir.y);
                float nyspec = nlighty / spec;

                noiseMat.SetFloat(ShaderParams.SpecularThreshold, nyspec);
                noiseMat.SetVector(ShaderParams.SunDir, sunDir);

                noiseMat.SetColor(ShaderParams.Color, textureBaseColor);
                Graphics.Blit(rtTurbulence, rtNoise, noiseMat);
            }

            Color detailColor = new Color(textureBaseColor.r * 0.5f, textureBaseColor.g * 0.5f, textureBaseColor.b * 0.5f, 0);

            foreach (var mat in fogMats) {
                mat.SetColor(ShaderParams.DetailColor, detailColor);
                mat.SetTexture(ShaderParams.NoiseTex, rtNoise);
            }
        }


        public void UpdateMaterialProperties () {
            UpdateMaterialProperties(false);
        }

        /// <summary>
        /// Schedules an update of the fog properties at end of this frame
        /// </summary>
        /// <param name="forceTerrainCaptureUpdate">In addition to apply any fog property change, perform a terrain heightmap capture (if Terrain Fit option is enabled)</param>
        public void UpdateMaterialProperties (bool forceTerrainCaptureUpdate) {
#if UNITY_EDITOR
            if (!Application.isPlaying && activeProfile != null) {
                UpdateMaterialPropertiesNow(true);
            }
#endif
            if (forceTerrainCaptureUpdate) {
                this.forceTerrainCaptureUpdate = true;
            }
            requireUpdateMaterial = true;
        }

        /// <summary>
        /// Forces an immediate material update
        /// </summary>
        /// <param name="skipTerrainCapture">Applies all fog properties changes but do not perform a terrain heightmap capture (if Terrain Fit option is enabled)</param>
        /// <param name="forceTerrainCaptureUpdate">In addition to apply any fog property change, perform a terrain heightmap capture (if Terrain Fit option is enabled).</param>
        public void UpdateMaterialPropertiesNow (bool skipTerrainCapture = false, bool forceTerrainCaptureUpdate = false) {

            if (gameObject == null || !gameObject.activeInHierarchy) {
                return;
            }

            if (forceTerrainCaptureUpdate) {
                this.forceTerrainCaptureUpdate = true;
            }

            // Note: fog volumes can be on any layer including Default.
            // Terrain fit capture camera hides the renderer during capture instead of relying on layer exclusion.

            fadeDistance = Mathf.Max(0.1f, fadeDistance);

            if (meshRenderer == null) {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            if (mf == null) {
                mf = GetComponent<MeshFilter>();
            }

            if (profile == null) {
                if (fogMat == null && meshRenderer != null) {
                    fogMat = new Material(Shader.Find("Hidden/VolumetricFog2/Empty"));
                    fogMat.hideFlags = HideFlags.DontSave;
                    meshRenderer.sharedMaterial = fogMat;
                }
                return;
            }

            if (mf != null) {
                if (profile.shape == VolumetricFogShape.Custom) {
                    if (profile.customMesh != null) {
                        mf.sharedMesh = profile.customMesh;
                    }
                } else {
                    // Reset to default cube mesh if not using custom shape
                    Mesh defaultMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                    if (mf.sharedMesh != defaultMesh) {
                        mf.sharedMesh = defaultMesh;
                    }
                }
            }

            // Subscribe to profile changes
            profile.onSettingsChanged -= UpdateMaterialProperties;
            profile.onSettingsChanged += UpdateMaterialProperties;

            // Subscribe to sub-volume profile changes
            if (subVolumes != null) {
                foreach (VolumetricFogSubVolume subVol in subVolumes) {
                    if (subVol != null && subVol.profile != null) {
                        subVol.profile.onSettingsChanged -= UpdateMaterialProperties;
                        subVol.profile.onSettingsChanged += UpdateMaterialProperties;
                    }
                }
            }

            if (turbulenceMat == null) {
                turbulenceMat = new Material(Shader.Find("Hidden/VolumetricFog2/Turbulence2D"));
            }
            if (noiseMat == null) {
                noiseMat = new Material(Shader.Find("Hidden/VolumetricFog2/Noise2DGen"));
            }
            if (blueNoiseTex == null) {
                blueNoiseTex = Resources.Load<Texture2D>("Textures/BlueNoiseVF128");
            }

            if (meshRenderer != null) {
                fogMat = meshRenderer.sharedMaterial;
                if (fogShader == null) {
                    fogShader = Shader.Find("VolumetricFog2/VolumetricFog2DURP");
                    if (fogShader == null) return;
                    // make sure this fog material doesn't copy other fog volumes (occurs when duplicating a fog volume in the scene)
                    foreach (VolumetricFog fog in volumetricFogs) {
                        if (fog != null && fog != this && fog.fogMat == fogMat) {
                            fogMat = null;
                            break;
                        }
                    }
                }
                if (fogMat == null || fogMat.shader != fogShader) {
                    fogMat = new Material(fogShader);
                    meshRenderer.sharedMaterial = fogMat;
                }
            }

            if (fogMat == null) return;

            profile.ValidateSettings();

            lastControllerPosition.x = float.MaxValue;
            activeProfile = profile;

            ComputeActiveProfile();
            ApplyProfileSettings();

            if (!skipTerrainCapture) {
                SurfaceCaptureSupportCheck();
            }

            OnUpdateMaterialProperties?.Invoke(this);
        }

        void ComputeActiveProfile () {

            if (Application.isPlaying) {
                if (enableFade || enableSubVolumes) {
                    if (fadeController == null) {
                        Camera cam = Camera.main;
                        if (cam != null) {
                            fadeController = Camera.main.transform;
                        }
                    }
                    if (fadeController != null && lastControllerPosition != fadeController.position) {

                        lastControllerPosition = fadeController.position;
                        activeProfile = profile;
                        alphaMultiplier = 1f;

                        // Self volume
                        if (enableFade) {
                            float t = ComputeVolumeFade(transform, fadeDistance);
                            alphaMultiplier *= t;
                        }

                        // Check sub-volumes
                        if (enableSubVolumes) {
                            int subVolumeCount = VolumetricFogSubVolume.subVolumes.Count;
                            int allowedSubVolumesCount = subVolumes != null ? subVolumes.Count : 0;
                            for (int k = 0; k < subVolumeCount; k++) {
                                VolumetricFogSubVolume subVolume = VolumetricFogSubVolume.subVolumes[k];
                                if (subVolume == null || subVolume.profile == null) continue;
                                if (allowedSubVolumesCount > 0 && !subVolumes.Contains(subVolume)) continue;
                                float t = ComputeVolumeFade(subVolume.transform, subVolume.fadeDistance);
                                if (t > 0) {
                                    if (lerpProfile == null) {
                                        lerpProfile = ScriptableObject.CreateInstance<VolumetricFogProfile>();
                                    }
                                    lerpProfile.Lerp(activeProfile, subVolume.profile, t);
                                    activeProfile = lerpProfile;
                                }
                            }
                        }
                    }
                } else {
                    alphaMultiplier = 1f;
                }
            }

            if (activeProfile == null) {
                activeProfile = profile;
            }
        }

        float ComputeVolumeFade (Transform transform, float fadeDistance) {
            Vector3 diff = transform.position - fadeController.position;
            diff.x = diff.x < 0 ? -diff.x : diff.x;
            diff.y = diff.y < 0 ? -diff.y : diff.y;
            diff.z = diff.z < 0 ? -diff.z : diff.z;
            Vector3 extents = transform.lossyScale * 0.5f;
            Vector3 gap = diff - extents;
            float maxDiff = gap.x > gap.y ? gap.x : gap.y;
            maxDiff = maxDiff > gap.z ? maxDiff : gap.z;
            fadeDistance += 0.0001f;
            float t = 1f - Mathf.Clamp01(maxDiff / fadeDistance);
            return t;
        }


        void ApplyProfileSettings () {

            currentAppliedColorSpace = QualitySettings.activeColorSpace;

            lastVolumeHeight = transform.localScale.y;
            int sortingLayerID = activeProfile.sortingLayerID;
            if (!SortingLayer.IsValid(sortingLayerID)) {
                sortingLayerID = 0;
            }
            meshRenderer.sortingLayerID = sortingLayerID;
            meshRenderer.sortingOrder = activeProfile.sortingOrder;
            fogMat.renderQueue = activeProfile.renderQueue;

            RegisterFogMat(fogMat);
            CleanupFogMats();
            int volumeLayerMask = 1 << gameObject.layer;
            foreach (var mat in fogMats) {
                mat.SetInt(ShaderParams.VolumeLayerMask, volumeLayerMask);
                SetFogMaterialProperties(mat);
            }

            if (activeProfile.distantFog) {
                UpdateDistantFogPropertiesNow();
            }
        }

        void SetFogMaterialProperties (Material mat) {

            if (activeProfile == null) return;

            float noiseScale = activeProfile.noiseScale;
            if (activeProfile.scaleNoiseWithHeight > 0) {
                noiseScale *= Mathf.Lerp(1f, transform.localScale.y * 0.04032f, activeProfile.scaleNoiseWithHeight);
            }
            noiseScale = 0.1f / noiseScale;
            mat.SetVector(ShaderParams.LightDiffusionData, new Vector4(activeProfile.lightDiffusionModel != DiffusionModel.Simple ? activeProfile.lightDiffusionPower / 256.1f : activeProfile.lightDiffusionPower, activeProfile.lightDiffusionIntensity, activeProfile.lightDiffusionNearDepthAtten));
            mat.SetVector(ShaderParams.ShadowData, new Vector4(activeProfile.shadowIntensity, activeProfile.shadowCancellation, activeProfile.shadowMaxDistance, 0));
            mat.SetVector(ShaderParams.RaymarchSettings, new Vector4(1f / activeProfile.raymarchQuality, activeProfile.dithering * 0.01f, activeProfile.jittering, activeProfile.raymarchMinStep));
            mat.SetFloat(ShaderParams.NearStepping, 1f / (1f + activeProfile.raymarchNearStepping));
            mat.SetFloat(ShaderParams.NoiseScale, noiseScale);
            mat.SetFloat(ShaderParams.DeepObscurance, activeProfile.deepObscurance * (currentAppliedColorSpace == ColorSpace.Gamma ? 1f : 1.2f));
            mat.SetFloat(ShaderParams.Density, activeProfile.density);
            mat.SetVector(ShaderParams.NativeLightsData, new Vector4(nativeLightsMultiplier, nativeLightFallOff, 1f - nativeLightFallOff, 0f));
            mat.SetFloat(ShaderParams.APVIntensityMultiplier, apvIntensityMultiplier);

            if (activeProfile.useDetailNoise) {
                float detailScale = (1f / activeProfile.detailScale) * noiseScale;
                mat.SetVector(ShaderParams.DetailData, new Vector4(activeProfile.detailStrength, activeProfile.detailOffset, detailScale, activeProfile.noiseFinalMultiplier));
                mat.SetColor(ShaderParams.DetailColor, activeProfile.albedo);
                mat.SetFloat(ShaderParams.DetailOffset, activeProfile.detailOffset);
                if ((detailTex == null || refDetailTex != activeProfile.detailTexture) && activeProfile.detailTexture != null) {
                    refDetailTex = activeProfile.detailTexture;
                    Texture3D tex = new Texture3D(activeProfile.detailTexture.width, activeProfile.detailTexture.height, activeProfile.detailTexture.depth, TextureFormat.Alpha8, false);
                    tex.filterMode = FilterMode.Trilinear;
                    Color32[] colors = activeProfile.detailTexture.GetPixels32();
                    int colorsLength = colors.Length;
                    for (int k = 0; k < colorsLength; k++) {
                        colors[k].a = colors[k].r;
                    }
                    tex.SetPixels32(colors);
                    tex.Apply();
                    detailTex = tex;
                }
                mat.SetTexture(ShaderParams.DetailTex, detailTex);
            }

            mat.SetTexture(ShaderParams.BlueNoiseTexture, blueNoiseTex);

            mat.DisableKeyword(ShaderParams.SKW_SHAPE_BOX);
            mat.DisableKeyword(ShaderParams.SKW_SHAPE_SPHERE);
            mat.DisableKeyword(ShaderParams.SKW_DISTANCE);
            mat.DisableKeyword(ShaderParams.SKW_DEPTH_GRADIENT);
            mat.DisableKeyword(ShaderParams.SKW_HEIGHT_GRADIENT);
            mat.DisableKeyword(ShaderParams.SKW_NATIVE_LIGHTS);
            mat.DisableKeyword(ShaderParams.SKW_POINT_LIGHTS);
            mat.DisableKeyword(ShaderParams.SKW_APV);
            mat.DisableKeyword(ShaderParams.SKW_VOIDS);
            mat.DisableKeyword(ShaderParams.SKW_RECEIVE_SHADOWS);
            mat.DisableKeyword(ShaderParams.SKW_DIRECTIONAL_COOKIE);
            mat.DisableKeyword(ShaderParams.SKW_FOW);
            mat.DisableKeyword(ShaderParams.SKW_CONSTANT_DENSITY);
            mat.DisableKeyword(ShaderParams.SKW_DETAIL_NOISE);
            mat.DisableKeyword(ShaderParams.SKW_SURFACE);
            mat.DisableKeyword(ShaderParams.SKW_DIFFUSION_SMOOTH);
            mat.DisableKeyword(ShaderParams.SKW_DIFFUSION_STRONG);


            bool mustSetDistanceData = activeProfile.distance > 0 || activeProfile.enableDepthGradient;
            if (mustSetDistanceData) {
                float fallOffFactor = 10f * (1f - activeProfile.distanceFallOff);
                mat.SetVector(ShaderParams.DistanceData, new Vector4(0, -1f + fallOffFactor, 1f / (0.0001f + activeProfile.depthGradientMaxDistance * activeProfile.depthGradientMaxDistance), fallOffFactor / (0.0001f + activeProfile.distance * activeProfile.distance)));

                if (activeProfile.distance > 0) {
                    mat.EnableKeyword(ShaderParams.SKW_DISTANCE);
                }
            }

            float maxDistanceFallOff = activeProfile.maxDistance - activeProfile.maxDistance * (1f - activeProfile.maxDistanceFallOff) + 1f;
            mat.SetVector(ShaderParams.MaxDistanceData, new Vector4(activeProfile.maxDistance, maxDistanceFallOff, activeProfile.maxDistanceFallOff, 0));

            if (activeProfile.enableDepthGradient) {
                mat.EnableKeyword(ShaderParams.SKW_DEPTH_GRADIENT);
                mat.SetTexture(ShaderParams.DepthGradientTexture, activeProfile.depthGradientTex);
            }
            if (activeProfile.enableHeightGradient) {
                mat.EnableKeyword(ShaderParams.SKW_HEIGHT_GRADIENT);
                mat.SetTexture(ShaderParams.HeightGradientTexture, activeProfile.heightGradientTex);
            }
            if (activeProfile.shape == VolumetricFogShape.Sphere) {
                mat.EnableKeyword(ShaderParams.SKW_SHAPE_SPHERE);
            } else {
                mat.EnableKeyword(ShaderParams.SKW_SHAPE_BOX);
            }

            if (enableNativeLights) {
                if (nativeLightsMultiplier > 0) {
                    mat.EnableKeyword(ShaderParams.SKW_NATIVE_LIGHTS);
                }
            } else if (enablePointLights) {
                mat.EnableKeyword(ShaderParams.SKW_POINT_LIGHTS);
            }
            if (enableAPV && apvIntensityMultiplier > 0) {
                mat.EnableKeyword(ShaderParams.SKW_APV);
            }
            if (enableVoids) {
                mat.EnableKeyword(ShaderParams.SKW_VOIDS);
            }
            if (activeProfile.receiveShadows && activeProfile.shadowMaxDistance > 0) {
                mat.EnableKeyword(ShaderParams.SKW_RECEIVE_SHADOWS);
            }
#if UNITY_2021_3_OR_NEWER
            if (activeProfile.cookie) {
                mat.EnableKeyword(ShaderParams.SKW_DIRECTIONAL_COOKIE);
            }
#endif
            if (enableFogOfWar) {
                mat.SetTexture(ShaderParams.FogOfWarTexture, fogOfWarTexture);
                UpdateFogOfWarMaterialBoundsProperties();
                mat.EnableKeyword(ShaderParams.SKW_FOW);
            }
            if (activeProfile.density == 0 || activeProfile.constantDensity) {
                mat.EnableKeyword(ShaderParams.SKW_CONSTANT_DENSITY);
            } else if (activeProfile.useDetailNoise) {
                mat.EnableKeyword(ShaderParams.SKW_DETAIL_NOISE);
            }
            if (activeProfile.terrainFit) {
                mat.EnableKeyword(ShaderParams.SKW_SURFACE);
            }
            if (activeProfile.lightDiffusionModel == DiffusionModel.Smooth) {
                mat.EnableKeyword(ShaderParams.SKW_DIFFUSION_SMOOTH);
            } else if (activeProfile.lightDiffusionModel == DiffusionModel.Strong) {
                mat.EnableKeyword(ShaderParams.SKW_DIFFUSION_STRONG);
            }
        }

        void UpdateDistantFogPropertiesNow () {
            if (distantFogMat == null) {
                distantFogMat = new Material(Shader.Find("Hidden/VolumetricFog2/DistantFog"));
            }
            Color distantFogColor = activeProfile.distantFogColor;
            distantFogColor.a *= GetDistantFogFadeAlpha();
            distantFogMat.SetColor(ShaderParams.Color, distantFogColor);
            distantFogMat.SetVector(ShaderParams.DistantFogData, new Vector4(activeProfile.distantFogStartDistance, activeProfile.distantFogDistanceDensity, activeProfile.distantFogMaxHeight, activeProfile.distantFogHeightDensity));
            distantFogMat.SetVector(ShaderParams.LightDiffusionData, new Vector4(activeProfile.lightDiffusionPower, activeProfile.distantFogDiffusionIntensity * activeProfile.lightDiffusionIntensity, activeProfile.lightDiffusionNearDepthAtten, 0));
            if (activeProfile.distantFogNoise) {
                distantFogMat.EnableKeyword(ShaderParams.SKW_DISTANT_FOG_NOISE);
                distantFogMat.SetTexture(ShaderParams.DistantFogNoiseTexture, activeProfile.distantFogNoiseTexture);
                distantFogMat.SetVector(ShaderParams.DistantFogDistanceNoiseData, new Vector4(
                    activeProfile.distantFogDistanceNoiseScale * 0.01f,
                    activeProfile.distantFogDistanceNoiseStrength,
                    activeProfile.distantFogDistanceNoiseMaxDistance,
                    0
                ));
                distantFogMat.SetVector(ShaderParams.DistantFogNoiseWind, distantFogNoiseWindAcum);
            } else {
                distantFogMat.DisableKeyword(ShaderParams.SKW_DISTANT_FOG_NOISE);
            }
        }

        float GetDistantFogFadeAlpha () {
            if (!enableFade || !fadeIncludeDistantFog) return 1f;
            if (fadeOut && Application.isPlaying) {
                return 1f - alphaMultiplier;
            }
            return alphaMultiplier;
        }

        /// <summary>
        /// Issues a refresh of the depth pre-pass alpha clipping renderers list
        /// </summary>
        public static void FindAlphaClippingObjects () {
            DepthRenderPrePassFeature.DepthRenderPass.FindAlphaClippingRenderers();
        }

        /// <summary>
        /// Adds a specific renderer to the alpha clipping objects managed by the semitransparent depth prepass option
        /// </summary>
        public static void AddAlphaClippingObject (Renderer renderer) {
            DepthRenderPrePassFeature.DepthRenderPass.AddAlphaClippingObject(renderer);
        }

        /// <summary>
        /// Removes a specific renderer to the alpha clipping objects managed by the semitransparent depth prepass option
        /// </summary>
        public static void RemoveAlphaClippingObject (Renderer renderer) {
            DepthRenderPrePassFeature.DepthRenderPass.RemoveAlphaClippingObject(renderer);
        }

        readonly List<Material> fogMats = new List<Material>();

        void CleanupFogMats () {
            for (int i = fogMats.Count - 1; i >= 0; i--) {
                if (fogMats[i] == null) {
                    fogMats.RemoveAt(i);
                }
            }
        }

        public void RegisterFogMat (Material fogMat) {
            if (fogMat == null) return;
            if (!fogMats.Contains(fogMat)) {
                fogMats.Add(fogMat);
            }
        }

        public void UnregisterFogMat (Material fogMat) {
            if (fogMat == null) return;
            fogMats.Remove(fogMat);
        }


    }

}
