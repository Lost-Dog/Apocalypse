Shader "CompassNavigatorPro/Demo/Gradient"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True" "RenderPipeline" = "UniversalPipeline" }
		LOD 100

		Pass
		{
			PackageRequirements {
				"com.unity.render-pipelines.universal"
			}			

            Tags { "LightMode" = "UniversalForwardOnly" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 y3 : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
				float4 _Color;
			CBUFFER_END

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				float3 worldPos = mul(GetObjectToWorldMatrix(), input.positionOS).xyz;
				float3 centerPos = mul(GetObjectToWorldMatrix(), float4(0,0,0,1)).xyz;
				float3 scalePos = mul(GetObjectToWorldMatrix(), float4(1,1,1,1)).xyz;
				output.y3 = float3(worldPos.y, centerPos.y, scalePos.y);
				return output;
			}

			float4 frag(Varyings input) : SV_Target
			{
				float4 col = _Color;
				float height = input.y3.z - input.y3.y;
				float dark = 1.0 - (input.y3.y + height * 0.5 - input.y3.x) / height;
				col.rgb *= 0.5 + dark * 0.5;
				return col;
			}
			ENDHLSL
		}

		// URP DepthOnly pass
		Pass
		{
			PackageRequirements {
				"com.unity.render-pipelines.universal"
			}			

			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings DepthOnlyVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				return output;
			}

			float4 DepthOnlyFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}
	}

	// Legacy/Built-in pipeline subshader
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 y3 : TEXCOORD0;
			};

			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float ypos = mul(unity_ObjectToWorld, v.vertex).y;
				float ycenter = mul(unity_ObjectToWorld, float4(0,0,0,1)).y;
				float ysize = mul(unity_ObjectToWorld, float4(1,1,1,1)).y;
				o.y3 = float3(ypos, ycenter, ysize);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				float height = i.y3.z - i.y3.y;
				float dark = 1.0 - (i.y3.y + height * 0.5 - i.y3.x) / height;
				col.rgb *= 0.5 + dark * 0.5;
				return col;
			}
			ENDCG
		}

		// Shadow caster pass for built-in pipeline
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
