Shader "Custom/FogPlane"
{
    Properties
    {
        _MainTex ("Fog Pattern (Optional)", 2D) = "white" {}
        _MaskTex ("Fog Mask (From Script)", 2D) = "white" {}
        _Color ("Fog Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Fog {Mode Off}
		ZWrite Off
		ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Get the mask value (1 = revealed, 0 = hidden)
                float mask = tex2D(_MaskTex, i.uv).r;
                
                // Optional: add a texture pattern to the fog
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // The magic: Alpha is 1 minus the mask
                // If mask is 1 (white), alpha is 0 (invisible)
                col.a = (1.0 - mask) * _Color.a;
                
                return col;
            }
            ENDCG
        }
    }
}