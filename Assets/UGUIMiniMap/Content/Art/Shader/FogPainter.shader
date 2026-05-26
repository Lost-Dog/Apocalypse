Shader "Hidden/FogPainter"
{
    Properties {
        _MainTex ("Main", 2D) = "white" {}
        _PlayerPos ("Player Pos", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 0.1
        _Softness ("Softness", Float) = 0.05
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _PlayerPos;
            float _Radius;
            float _Softness;

            fixed4 frag (v2f_img i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Calculate distance between current pixel UV and player's map UV
                float dist = distance(i.uv, _PlayerPos.xy);
                float mask = smoothstep(_Radius + _Softness, _Radius, dist);
                
                // Use 'max' to ensure we only ever INCREASE the transparency (memory)
                return max(col, mask); 
            }
            ENDCG
        }
    }
}