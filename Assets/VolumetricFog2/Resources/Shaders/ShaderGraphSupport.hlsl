#ifndef VOLUMETRIC_FOG_2_SHADERGRAPH_SUPPORT
#define VOLUMETRIC_FOG_2_SHADERGRAPH_SUPPORT

#ifndef SHADERGRAPH_PREVIEW
#include_with_pragmas "API.hlsl"
#endif

void GetFogOpacity_half(float3 wpos, float4 screenUV, out half fogOpacity) {
#ifdef SHADERGRAPH_PREVIEW
    fogOpacity = 1.0;
#else
    half4 fog = ComputeFog(wpos, screenUV.xy);
    fogOpacity = 1.0 - saturate(fog.a);
#endif
}

void GetFogOpacity_float(float3 wpos, float4 screenUV, out float fogOpacity) {
#ifdef SHADERGRAPH_PREVIEW
    fogOpacity = 1.0;
#else
    half opacityHalf;
    GetFogOpacity_half(wpos, screenUV, opacityHalf);
    fogOpacity = (float)opacityHalf;
#endif
}

#endif // VOLUMETRIC_FOG_2_SHADERGRAPH_SUPPORT

