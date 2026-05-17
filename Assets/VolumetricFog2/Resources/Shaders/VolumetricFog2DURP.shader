Shader "VolumetricFog2/VolumetricFog2DURP"
{
	Properties
	{
		[HideInInspector] _MainTex("Main Texture", 2D) = "white" {}
		[HideInInspector] _Color("Color", Color) = (1,1,1)
		[HideInInspector] _NoiseTex("Noise Texture", 2D) = "white" {}
		[HideInInspector] _DetailTex("Detail Texture", 3D) = "white" {}
		[HideInInspector] _NoiseScale("Noise Scale", Float) = 0.025
		[HideInInspector] _NoiseFinalMultiplier("Noise Scale", Float) = 1.0
		[HideInInspector] _NoiseStrength("Noise Strength", Float) = 1.0
		[HideInInspector] _Density("Density", Float) = 1.0
		[HideInInspector] _DeepObscurance("Deep Obscurance", Range(0, 2)) = 0.7
		[HideInInspector] _LightColor("Light Color", Color) = (1,1,1)
		[HideInInspector] _LightDiffusionData("Sun Diffusion Data", Vector) = (32, 0.4, 100)
		[HideInInspector] _SunDir("Sun Direction", Vector) = (1,0,0)
		[HideInInspector] _ShadowData("Shadow Data", Vector) = (0.5, 0, 62500)
		[HideInInspector] _WindDirection("Wind Direction", Vector) = (1, 0, 0)
		[HideInInspector] _DetailWindDirection("Detail Wind Direction", Vector) = (1, 0, 0)
		[HideInInspector] _RayMarchSettings("Raymarch Settings", Vector) = (2, 0.01, 1.0, 0.1)
		[HideInInspector] _BoundsCenter("Bounds Center", Vector) = (0,0,0)
		[HideInInspector] _BoundsExtents("Bounds Size", Vector) = (0,0,0)
		[HideInInspector] _BoundsBorder("Bounds Border", Vector) = (0,1,0)
		[HideInInspector] _BoundsData("Bounds Data", Vector) = (0,0,1)
		[HideInInspector] _DetailData("Detail Data", Vector) = (0.5, 4, -0.5, 0)
		[HideInInspector] _DetailColor("Detail Color", Color) = (0.5,0.5,0.5,0)
		[HideInInspector] _DetailOffset("Detail Offset", Float) = -0.5
		[HideInInspector] _DistanceData("Distance Data", Vector) = (0, 5, 1, 1)
		[HideInInspector] _DepthGradientTex("Depth Gradient Texture", 2D) = "white" {}
		[HideInInspector] _HeightGradientTex("Height Gradient Texture", 2D) = "white" {}
		[HideInInspector] _SpecularThreshold("Specular Threshold", Float) = 0.5
		[HideInInspector] _SpecularIntensity("Specular Intensity", Float) = 0
		[HideInInspector] _SpecularColor("Specular Color", Color) = (0.5,0.5,0.5,0)
		[HideInInspector] _FogOfWarCenterAdjusted("FoW Center Adjusted", Vector) = (0,0,0)
		[HideInInspector] _FogOfWarSize("FoW Size", Vector) = (0,0,0)
		[HideInInspector] _FogOfWarCenter("FoW Center", Vector) = (0,0,0)
		[HideInInspector] _FogOfWar("FoW Texture", 2D) = "white" {}
		[HideInInspector] _BlueNoise("_Blue Noise Texture", 2D) = "white" {}
		[HideInInspector] _MaxDistanceData("Max Lengh Data", Vector) = (100000, 0.00001, 0)
		[HideInInspector] _NativeLightsData("Native Lights Data", Vector) = (1, 1, 0, 0)
		[HideInInspector] _APVIntensityMultiplier("APV Intensity Multiplier", Float) = 1
		[HideInInspector] _NearStepping("Near Stepping", Float) = 8
		[HideInInspector] _VFVolumeLayer("Volume Layer", Int) = -1
	}
		SubShader
		{
			Name "Volumetric Fog"
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" "DisableBatching" = "True" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
			Blend One OneMinusSrcAlpha
			ZTest Always
			Cull Front
			ZWrite Off
			ZClip False

			Pass
			{
				Tags { "LightMode" = "UniversalForward" }
				HLSLPROGRAM
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag

				#if UNITY_VERSION < 202100
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
				#else
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
				#endif

                #pragma multi_compile _ _ADDITIONAL_LIGHTS
				#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile _ VF2_DEPTH_PREPASS VF2_DEPTH_PEELING
				#pragma multi_compile_local_fragment _ VF2_POINT_LIGHTS VF2_NATIVE_LIGHTS
				#pragma multi_compile_local_fragment _ VF2_RECEIVE_SHADOWS
				#pragma multi_compile_local_fragment _ VF2_SHAPE_SPHERE
				#pragma multi_compile_local_fragment _ VF2_DETAIL_NOISE VF2_CONSTANT_DENSITY
				#pragma shader_feature_local_fragment VF2_DISTANCE
				#pragma shader_feature_local_fragment VF2_VOIDS
				#pragma shader_feature_local_fragment VF2_FOW
				#pragma shader_feature_local_fragment VF2_SURFACE
				#pragma shader_feature_local_fragment VF2_DEPTH_GRADIENT
				#pragma shader_feature_local_fragment VF2_HEIGHT_GRADIENT
				#pragma shader_feature_local_fragment VF2_LIGHT_COOKIE
				#pragma shader_feature_local_fragment _ VF2_DIFFUSION_SMOOTH VF2_DIFFUSION_STRONG
				#define UNITY_FOVEATED_RENDERING_INCLUDED
				#define _SURFACE_TYPE_TRANSPARENT

				#if UNITY_VERSION >= 60010000
					#pragma multi_compile_fragment _ _CLUSTER_LIGHT_LOOP
					#define USE_FORWARD_PLUS USE_CLUSTER_LIGHT_LOOP
				#elif UNITY_VERSION >= 202200
					#pragma multi_compile_fragment _ _FORWARD_PLUS
				#endif

				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
				#undef SAMPLE_TEXTURE2D
				#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2) SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, 0)
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

				#include "Input.hlsl"
				#include "CommonsURP.hlsl"

				#if UNITY_VERSION >= 202200 && defined(FOG_LIGHT_LAYERS)
					#pragma multi_compile_fragment _ _LIGHT_LAYERS
					#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
				#endif

				#if UNITY_VERSION >= 202310
		            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
					#pragma shader_feature_local_fragment VF2_APV
				#endif

				#include "Primitives.cginc"
				#include "ShadowsURP.cginc"
				#include "APV.cginc"
				#include "PointLights.cginc"
				#include "FogVoids.cginc"
				#include "FogOfWar.cginc"
				#include "FogDistance.cginc"
				#include "Surface.cginc"
				#include "Raymarch2D.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos     : SV_POSITION;
                    float3 wpos    : TEXCOORD0;
					float4 scrPos  : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

					int _ForcedInvisible;
				int _VFLayerMask;

				v2f vert(appdata v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = TransformObjectToHClip(v.vertex.xyz);
				    o.wpos = TransformObjectToWorld(v.vertex.xyz);
					o.scrPos = ComputeScreenPos(o.pos);

					#if defined(UNITY_REVERSED_Z)
						o.pos.z = o.pos.w * UNITY_NEAR_CLIP_VALUE * 0.99995; //  0.99995 avoids precision issues on some Android devices causing unexpected clipping of light mesh
					#else
						o.pos.z = o.pos.w - 0.000005;
					#endif

					if (_ForcedInvisible == 1 && (_VFVolumeLayer & _VFLayerMask) != 0) {
						o.pos.xy = -10000;
                    }

					return o;
				}

				half4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					float3 wpos = i.wpos;
					float2 screenUV = i.scrPos.xy / i.scrPos.w;

					return ComputeFog(wpos, screenUV);
				}
				ENDHLSL
			}

		}
}
