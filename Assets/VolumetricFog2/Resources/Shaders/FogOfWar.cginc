#ifndef VOLUMETRIC_FOG_2_FOW
#define VOLUMETRIC_FOG_2_FOW

sampler2D _FogOfWar;

half4 ApplyFogOfWar(float3 wpos) {
    float2 fogTexCoord = wpos.xz / _FogOfWarSize.xz - _FogOfWarCenterAdjusted.xz;
    half4 fowColor = tex2Dlod(_FogOfWar, float4(fogTexCoord, 0, 0));
    return half4(fowColor.rgb * fowColor.a, fowColor.a);
}

#endif