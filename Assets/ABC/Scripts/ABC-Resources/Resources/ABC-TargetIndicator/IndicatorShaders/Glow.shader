Shader "ABC/Glow"
{
	Properties
	{
		_ColourTint("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	_BumpMap("Normal Map", 2D) = "bump" {}
	_OutlineColour("OutLine Color", Color) = (1, 0.17255, 1, 1)
		_OutlinePower("OutLine Power", Range(1.0, 6.0)) = 3.0

	}
		SubShader{

		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM
#pragma surface surf Lambert

		struct Input {

		float4 color : Color;
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float3 viewDir;

	};

	float4 _ColourTint;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	float4 _OutlineColour;
	float _OutlinePower;

	void surf(Input IN, inout SurfaceOutput o)
	{


		IN.color = _ColourTint;
		o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color;
		o.Normal = UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap));

		half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
		o.Emission = _OutlineColour.rgb * pow(rim, _OutlinePower);


	}
	ENDCG
	}
		FallBack "Diffuse"
}﻿