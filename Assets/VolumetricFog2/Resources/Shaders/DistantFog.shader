Shader "Hidden/VolumetricFog2/DistantFog"
{
	Properties
	{
		[HideInInspector] _MainTex("Main Texture", 2D) = "white" {}
		[HideInInspector] _Color("Color", Color) = (1,1,1)
		[HideInInspector] _DistantFogData("Distant Fog Data", Vector) = (100,0.1,400,0.5)
		[HideInInspector] _DistantFogData2("Base Altitude", Vector) = (0, 1, 0, 0)
		[HideInInspector] _LightColor("Light Color", Color) = (1,1,1)
		[HideInInspector] _LightDiffusionData("Sun Diffusion Data", Vector) = (32, 0.4, 100)
		[HideInInspector] _SunDir("Sun Direction", Vector) = (1,0,0)
		[HideInInspector] _DistantFogDistanceNoiseData("Distance Noise Data", Vector) = (50, 0.5, 250, 0)
		[HideInInspector] _DistantFogNoiseTexture("Distant Fog Noise Texture", 3D) = "white" {}
		[HideInInspector] _DistantFogNoiseWind("Distant Fog Noise Wind", Vector) = (0, 0, 0, 0)
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent-1" "DisableBatching" = "True" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always
			Cull Off
			ZWrite Off
			ZClip False

			Pass
			{
				Name "Distant Fog"
				Tags { "LightMode" = "UniversalForward" }
				HLSLPROGRAM
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ VF2_DEPTH_PREPASS VF2_DEPTH_PEELING
				#pragma multi_compile_local_fragment _ VF2_DISTANT_FOG_NOISE

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
				#include "CommonsURP.hlsl"
				#include "Input.hlsl"
				#include "Primitives.cginc"
				#include "Raymarch2D.cginc"

				half4 _Color;

				float4 _DistantFogData;
				#define START_DISTANCE _DistantFogData.x
				#define DISTANCE_DENSITY _DistantFogData.y
				#define MAX_HEIGHT _DistantFogData.z
				#define HEIGHT_DENSITY _DistantFogData.w

				float4 _DistantFogData2;
				#define BASE_ALTITUDE _DistantFogData2.x
				#define MIN_ALTITUDE _DistantFogData2.y

				#if defined(VF2_DISTANT_FOG_NOISE)
				float4 _DistantFogDistanceNoiseData;
				#define DISTANCE_NOISE_SCALE _DistantFogDistanceNoiseData.x
				#define DISTANCE_NOISE_STRENGTH _DistantFogDistanceNoiseData.y
				#define DISTANCE_NOISE_MAX_DISTANCE _DistantFogDistanceNoiseData.z

				sampler3D _DistantFogNoiseTexture;
				float4 _DistantFogNoiseWind;
				#endif

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv  : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos    : SV_POSITION;
					float4 scrPos : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = TransformObjectToHClip(v.vertex.xyz);
					o.scrPos = ComputeScreenPos(o.pos);

					#if defined(UNITY_REVERSED_Z)
						o.pos.z = o.pos.w * UNITY_NEAR_CLIP_VALUE * 0.99995; //  0.99999 avoids precision issues on some Android devices causing unexpected clipping of light mesh
					#else
						o.pos.z = o.pos.w - 0.000005;
					#endif

					return o;
				}

				#if defined(VF2_DISTANT_FOG_NOISE)
				float SampleNoise3D_3(float3 pos, float scale) {
					float3 uvw = pos * scale - _DistantFogNoiseWind.xyz;
					float noise1 = tex3Dlod(_DistantFogNoiseTexture, float4(uvw, 0)).r;
					float noise2 = tex3Dlod(_DistantFogNoiseTexture, float4(uvw * 2.0, 0)).r;
					float noise3 = tex3Dlod(_DistantFogNoiseTexture, float4(uvw * 4.0, 0)).r;
					return noise1 * 0.5 + noise2 * 0.3 + noise3 * 0.2;
				}
				#endif

				float IsSkybox(float depth) {
					#if UNITY_REVERSED_Z
						return depth <= 0.00001; // skybox (near 0 in reversed Z)
					#else
						return depth >= 0.99999; // skybox (near 1 in regular Z)
					#endif
				}

				half4 frag(v2f i) : SV_Target {
					UNITY_SETUP_INSTANCE_ID(i);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					float2 uv = i.scrPos.xy / i.scrPos.w;

					float depth;
					float isSkybox;
					#if VF2_DEPTH_PEELING
						// First pass (before transparents): Only render behind transparent objects
						float sceneDepth = SampleSceneDepth(VF2_FLIP_DEPTH_TEXTURE ? float2(uv.x, 1.0 - uv.y) : uv);
						float customDepth = SAMPLE_TEXTURE2D_X(_CustomDepthTexture, sampler_CustomDepthTexture, uv).r;
						#if UNITY_REVERSED_Z
							// In reversed Z, closer objects have higher depth values
							// If custom depth <= scene depth or is skybox, there's no transparent object - skip
							if (customDepth <= sceneDepth || IsSkybox(customDepth)) {
								return 0;
							}
						#else
							// In regular Z, closer objects have lower depth values
							// If custom depth >= scene depth or is skybox, there's no transparent object - skip
							if (customDepth >= sceneDepth || IsSkybox(customDepth)) {
								return 0;
							}
						#endif
						depth = sceneDepth;
						isSkybox = IsSkybox(sceneDepth);
					#elif VF2_DEPTH_PREPASS
						// Second pass (after transparents): Use custom depth to compute distance to transparents
						float sceneDepth = SampleSceneDepth(VF2_FLIP_DEPTH_TEXTURE ? float2(uv.x, 1.0 - uv.y) : uv);
						float customDepth = SAMPLE_TEXTURE2D_X(_CustomDepthTexture, sampler_CustomDepthTexture, uv).r;
						#if UNITY_REVERSED_Z
							depth = max(sceneDepth, customDepth);
						#else
							depth = min(sceneDepth, customDepth);
						#endif
						isSkybox = IsSkybox(sceneDepth);
					#else
						depth = GetRawDepth(uv);
						isSkybox = IsSkybox(depth);
					#endif

					#if !UNITY_REVERSED_Z
						depth = depth * 2.0 - 1.0;
					#endif

					uv.y = VF2_FLIP_DEPTH_TEXTURE ? 1.0 - uv.y : uv.y;
					float3 wpos = ComputeWorldSpacePosition(uv, depth, unity_MatrixInvVP);

					float3 rayStart = GetRayStart(wpos);
					float3 ray = wpos - rayStart;
					
                   	float t1 = length(ray);
					float3 rayDir = ray / t1;

			
					float3 hitPos = t1 * rayDir;

					float maxZ = _ProjectionParams.z - 10;
					float startDistance = min(maxZ, START_DISTANCE);
					float d = (t1 - startDistance) * DISTANCE_DENSITY;
					float hitPosY = max(MIN_ALTITUDE, hitPos.y + rayStart.y - BASE_ALTITUDE);
					
					float h = (hitPosY != 0 ? MAX_HEIGHT / abs(hitPosY) : MAX_HEIGHT) * HEIGHT_DENSITY;

					float f = min(d, h);
					f = max(f, 0);
	
					half sum = exp2(-f);
					sum = 1.0 - saturate(sum);

					#if defined(VF2_DISTANT_FOG_NOISE)
						float noise = SampleNoise3D_3(wpos, DISTANCE_NOISE_SCALE);
						float distanceAttenuation = saturate(DISTANCE_NOISE_MAX_DISTANCE / t1);
						float surfaceNoise = lerp(1, noise, DISTANCE_NOISE_STRENGTH * distanceAttenuation);
						sum *= surfaceNoise;
					#endif

					half4 color = half4(_Color.rgb, sum * _Color.a);

					if (isSkybox) {
						half diffusionIntensity = GetDiffusionIntensity(rayDir);
						color.rgb *= 1 + diffusionIntensity;
					}
					
					color.rgb *= _LightColor.rgb;
					return color;
				}
				ENDHLSL
			}

		}
}
