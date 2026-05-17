#ifndef VOLUMETRIC_FOG_2_INPUT
#define VOLUMETRIC_FOG_2_INPUT

#ifndef VOLUMETRIC_FOG_2_SHADERGRAPH_SUPPORT
CBUFFER_START(UnityPerMaterial)
#endif

half4 _LightColor;
half3 _NativeLightsData;
#define NATIVE_LIGHTS_MULTIPLIER _NativeLightsData.x
#define NATIVE_LIGHT_FALLOFF _NativeLightsData.y
#define ONE_MINUS_NATIVE_LIGHT_FALLOFF _NativeLightsData.z

float _NoiseScale;
half4 _DetailColor;

half4 _DetailData; // x = strength, y = offset, z = scale, w = importance
#define DETAIL_STRENGTH _DetailData.x
#define DETAIL_OFFSET _DetailData.y
#define DETAIL_SCALE _DetailData.z
#define USE_BASE_NOISE _DetailData.w

half _DeepObscurance;

half3 _LightDiffusionData;
#define LIGHT_DIFFUSION_POWER _LightDiffusionData.x
#define LIGHT_DIFFUSION_INTENSITY _LightDiffusionData.y
#define LIGHT_DIFFUSION_DEPTH_ATTEN _LightDiffusionData.z

half  _Density;
float3 _BoundsCenter, _BoundsExtents;
float3 _SunDir;
float3 _WindDirection, _DetailWindDirection;

float4 _DistanceData;
float3 _MaxDistanceData;
#define FOG_MAX_LENGTH _MaxDistanceData.x
#define FOG_MAX_LENGTH_FALLOFF_PRECOMPUTED _MaxDistanceData.y
#define FOG_MAX_LENGTH_FALLOFF _MaxDistanceData.z

float3 _BoundsData;
#define BOUNDS_VERTICAL_OFFSET _BoundsData.x
#define BOUNDS_BOTTOM _BoundsData.y
#define BOUNDS_SIZE_Y _BoundsData.z

float4 _RayMarchSettings;
#define FOG_STEPPING _RayMarchSettings.x
#define DITHERING _RayMarchSettings.y
#define JITTERING _RayMarchSettings.z
#define MIN_STEPPING _RayMarchSettings.w

float _NearStepping;

half3 _ShadowData;
#define SHADOW_INTENSITY _ShadowData.x
#define SHADOW_CANCELLATION _ShadowData.y
#define SHADOW_MAX_DISTANCE _ShadowData.z

float4 _BoundsBorder;
#define BORDER_SIZE_SPHERE _BoundsBorder.x
#define BORDER_START_SPHERE _BoundsBorder.y
#define BORDER_SIZE_BOX _BoundsBorder.xz
#define BORDER_START_BOX _BoundsBorder.yw

half _APVIntensityMultiplier;

float3 _FogOfWarCenter;
float3 _FogOfWarSize;
float3 _FogOfWarCenterAdjusted;

float4x4 _InvRotMatrix;
float4x4 _RotMatrix;

int _VFVolumeLayer;

#ifndef VOLUMETRIC_FOG_2_SHADERGRAPH_SUPPORT
CBUFFER_END
#endif

sampler2D _NoiseTex;
sampler3D _DetailTex;

TEXTURE2D(_BlueNoise);
SAMPLER(sampler_BlueNoise_PointRepeat);
float4 _BlueNoise_TexelSize;

float3 _VFRTSize;

#endif // VOLUMETRIC_FOG_2_INPUT
