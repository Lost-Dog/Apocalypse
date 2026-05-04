#ifndef CELLIGHTING_INCLUDED
#define CELLIGHTING_INCLUDED

#include "UnityCG.cginc"
#include "AutoLight.cginc"

float4 _Color;

#if USE_GRADIENT
float4 _GradientColor;
float _GradientOffset;
float _GradientFalloff;
#endif

sampler2D _MainTex;
float4 _MainTex_ST;

#if USE_NORMAL
uniform sampler2D _BumpMap;
uniform float4 _BumpMap_ST;
#endif

#if USE_GLOSS
float _GlossStrength;
float _Glossiness;
float _GlossSmoothness;
#endif

float3 _ShadowCol;
float _ShadowSmoothness;
float _ShadowFalloff;

float _RimStrength;

uniform float4 _LightColor0;

struct appdata
{
    float2 uv : TEXCOORD0;
    float4 position : POSITION;
    float3 normal : NORMAL;

    #if USE_NORMAL
    float4 tangent : TANGENT;
    #endif
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 pos : SV_POSITION;

    #if USE_NORMAL
    half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
    half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
    half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z

    #else
    float3 normal : NORMAL;
    #endif

    #if USE_GRADIENT
    float4 vertex : TEXCOORD6;
    #endif

    float3 viewDir : TEXCOORD4;
    SHADOW_COORDS(5)
};

v2f vert (appdata v)
{
    v2f o;

    o.pos = UnityObjectToClipPos(v.position);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    #if USE_NORMAL
    half3 wNormal = UnityObjectToWorldNormal(v.normal);
    half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);

    half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
    half3 wBitangent = cross(wNormal, wTangent) * tangentSign;

    //output tangent space matrix
    o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
    o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
    o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

    #else
    o.normal = UnityObjectToWorldNormal(v.normal);
    #endif

    #if USE_GRADIENT
    o.vertex = v.position;
    #endif

    o.viewDir = WorldSpaceViewDir(v.position);

    TRANSFER_SHADOW(o);

    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float3 tex = tex2D(_MainTex, i.uv);
    
    #if USE_NORMAL
    half3 texNormal = UnpackNormal(tex2D(_BumpMap, i.uv));
    half3 normal;
    
    normal.x = dot(i.tspace0, texNormal);
    normal.y = dot(i.tspace1, texNormal);
    normal.z = dot(i.tspace2, texNormal);
    
    #else
    float3 normal = normalize(i.normal);
    #endif
    float3 viewDir = normalize(i.viewDir);

    #if USE_GLOSS
    float3 halfVect = normalize(_WorldSpaceLightPos0 + viewDir);
    float NdotH = dot(normal, halfVect);
    #endif
    
    float NdotL = dot(_WorldSpaceLightPos0, normal);

    float NdotV = dot(viewDir, normal);
    
    float rimDot = (NdotV * _RimStrength) + 1-_RimStrength;

    float shadowIntensity = NdotL * SHADOW_ATTENUATION(i);
    float shadow = smoothstep(_ShadowFalloff, _ShadowFalloff + _ShadowSmoothness, shadowIntensity);
    float3 shadowColor = lerp(_ShadowCol, _LightColor0, shadow);

    #if USE_GLOSS
    float specularIntensity = pow(NdotH * shadow, _Glossiness * _Glossiness);
    float specular = smoothstep(0.005, 0.005 + _GlossSmoothness, specularIntensity) * _GlossStrength * _LightColor0;
    #endif
    
    #if USE_GRADIENT
    float4 col = lerp(_GradientColor, _Color, saturate(_GradientOffset + (i.vertex.g * _GradientFalloff)));
    #else
    float4 col = _Color;
    #endif

    col.rgb *= tex * shadowColor * rimDot;

    #if USE_GLOSS
    col.rgb += specular;
    #endif

    return col;
}

#endif 