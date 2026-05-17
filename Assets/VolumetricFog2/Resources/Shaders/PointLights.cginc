#ifndef VOLUMETRIC_FOG_2_POINT_LIGHTS
#define VOLUMETRIC_FOG_2_POINT_LIGHTS

#if VF2_POINT_LIGHTS

//#define FAST_POINT_LIGHTS_OCCLUSION

#define FOG_MAX_POINT_LIGHTS 16

CBUFFER_START(VolumetricFog2PointLightBuffers)
    float4 _VF2_FogPointLightPosition[FOG_MAX_POINT_LIGHTS];
    half4 _VF2_PointLightColor[FOG_MAX_POINT_LIGHTS];
    float4 _VF2_PointLightParams;
    int _VF2_PointLightCount;
CBUFFER_END

#define VF2_POINT_LIGHT_INSIDE_ATTEN _VF2_PointLightParams.x
#define VF2_POINT_LIGHT_BLENDING _VF2_PointLightParams.y

#define dot2(x) dot(x,x)

float minimum_distance_sqr(float fogLengthSqr, float3 w, float3 p) {
    // Return minimum distance between line segment vw and point p
    float t = saturate(dot(p, w) / fogLengthSqr); 
    float3 projection = t * w;
    float distSqr = dot2(p - projection);
    return distSqr;
}

void AddPointLights(float3 rayStart, float3 rayDir, inout half4 sum, float t0, float fogLength) {
    float3 fogCeilingCut = rayStart + rayDir * t0;
    fogCeilingCut += rayDir * VF2_POINT_LIGHT_INSIDE_ATTEN;
    fogLength -= VF2_POINT_LIGHT_INSIDE_ATTEN;
    rayDir *= fogLength;
    float fogLengthSqr = fogLength * fogLength;

    for (int k=_VF2_PointLightCount-1;k>=0;k--) {
        float3 pointLightPosition = _VF2_FogPointLightPosition[k].xyz;
        #if defined(FAST_POINT_LIGHTS_OCCLUSION)
            float4 clipPos = TransformWorldToHClip(pointLightPosition);
            float4 scrPos  = ComputeScreenPos(clipPos);
            float  depth   = LinearEyeDepth(SampleSceneDepth(scrPos.xy / scrPos.w), _ZBufferParams);
            if (depth < clipPos.w) continue;
        #endif

        half pointLightInfluence = minimum_distance_sqr(fogLengthSqr, rayDir, pointLightPosition - fogCeilingCut) * _VF2_PointLightColor[k].w;
        half scattering = sum.a / (1.0 + pointLightInfluence);
        half pointLightIntensity = dot(_VF2_PointLightColor[k].rgb, float3(0.5f, 0.5f, 0.5f));
        sum.rgb = (sum.rgb + _VF2_PointLightColor[k].rgb * scattering) / (1.0 + scattering * VF2_POINT_LIGHT_BLENDING * pointLightIntensity);
    }
}

half3 GetPointLights(float3 wpos) {
    half3 color = half3(0,0,0);
    for (int k=0;k<_VF2_PointLightCount;k++) {
        float3 toLight = _VF2_FogPointLightPosition[k].xyz - wpos;
        float dist = dot2(toLight);
        color += _VF2_PointLightColor[k].rgb / (_VF2_PointLightColor[k].w * dist);
    }
    return color;
}

#endif // VF2_POINT_LIGHTS

#endif // VOLUMETRIC_FOG_2_POINT_LIGHTS