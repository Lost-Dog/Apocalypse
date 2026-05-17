#ifndef VOLUMETRIC_FOG_2_RAYMARCH
#define VOLUMETRIC_FOG_2_RAYMARCH 

#if defined(_LIGHT_LAYERS)
    static uint meshRenderingLayers;
#endif

#if defined(FOG_NATIVE_LIGHT_FALLOFF)
    #define VF2_NATIVE_LIGHT_ATTEN(light) ((light).distanceAttenuation * NATIVE_LIGHT_FALLOFF + ONE_MINUS_NATIVE_LIGHT_FALLOFF)
#else
    #define VF2_NATIVE_LIGHT_ATTEN(light) ((light).distanceAttenuation)
#endif

void SetJitter(float2 uv) {

    float2 screenSize = lerp(_ScreenParams.xy, _VFRTSize.xy, _VFRTSize.z);
    float2 pixelPos = uv * screenSize;

    #if defined(FOG_BLUE_NOISE)
        float2 noiseUV = pixelPos * _BlueNoise_TexelSize.xy;
        jitter =  SAMPLE_TEXTURE2D(_BlueNoise, sampler_BlueNoise_PointRepeat, noiseUV).r;
    #else
        //Jitter = frac(dot(float2(2.4084507, 3.2535211), (scrPos.xy / scrPos.w) * _ScreenParams.xy));
        const float3 magic = float3( 0.06711056, 0.00583715, 52.9829189 );
        jitter = frac( magic.z * frac( dot( pixelPos, magic.xy ) ) );
    #endif
}


inline float3 ProjectOnPlane(float3 v, float3 planeNormal) {
    // assume plane normal has a modulus of 1
    float dt = dot(v, planeNormal);
	return v - planeNormal * dt;
}

inline float3 GetRayStart(float3 wpos) {
    float3 cameraPosition = GetCameraPositionWS();
    #if defined(ORTHO_SUPPORT)
	    float3 cameraForward = UNITY_MATRIX_V[2].xyz;
	    float3 rayStart = ProjectOnPlane(wpos - cameraPosition, cameraForward) + cameraPosition;
        return lerp(cameraPosition, rayStart, unity_OrthoParams.w);
    #else
        return cameraPosition;
    #endif
}

inline half Brightness(half3 color) {
    return max(color.r, max(color.g, color.b));
}


half4 SampleDensity(float3 wpos) {

    wpos.y -= BOUNDS_VERTICAL_OFFSET;
    float3 boundsCenter = _BoundsCenter;
    float3 boundsExtents = _BoundsExtents;

#if VF2_SURFACE
    SurfaceApply(boundsCenter, boundsExtents);
#endif

#if VF2_DETAIL_NOISE
    #if !defined(USE_WORLD_SPACE_NOISE)
        wpos.xyz -= boundsCenter;
    #endif
    half detail = tex3Dlod(_DetailTex, float4(wpos * DETAIL_SCALE - _DetailWindDirection, 0)).a;
    half4 density = _DetailColor;
    if (USE_BASE_NOISE) {
        #if defined(USE_WORLD_SPACE_NOISE)
            wpos.y -= boundsCenter.y;
        #endif
        wpos.y /= boundsExtents.y;
        density = tex2Dlod(_NoiseTex, float4(wpos.xz * _NoiseScale - _WindDirection.xz, 0, 0));
        density.a -= abs(wpos.y);
    }
    density.a += (detail + DETAIL_OFFSET) * DETAIL_STRENGTH;
#else
    #if defined(USE_WORLD_SPACE_NOISE) || VF2_CONSTANT_DENSITY
        wpos.y -= boundsCenter.y;
    #else
        wpos.xyz -= boundsCenter;
    #endif
    wpos.y /= boundsExtents.y;
    #if VF2_CONSTANT_DENSITY
        half4 density = half4(_DetailColor.rgb, 1.0);
    #else
        half4 density = tex2Dlod(_NoiseTex, float4(wpos.xz * _NoiseScale - _WindDirection.xz, 0, 0));
    #endif
    density.a -= abs(wpos.y);
#endif

    return density;
}


#define dot2(x) dot(x,x)

void AddFog(float3 rayStart, float3 wpos, float2 uv, half energyStep, half4 baseColor, inout half4 sum) {

   half4 density = SampleDensity(wpos);

   float3 rotatedWPos = wpos;
   #if defined(FOG_ROTATION)
        rotatedWPos = Rotate(rotatedWPos);
   #endif

   #if VF2_VOIDS
        density.a -= ApplyFogVoids(rotatedWPos);
   #endif

   #if defined(FOG_BORDER)
        #if VF2_SHAPE_SPHERE
            float3 delta = wpos - _BoundsCenter;
            float distSqr = dot2(delta);
            float border = 1.0 - saturate( (distSqr - BORDER_START_SPHERE) / BORDER_SIZE_SPHERE );
        #else
            float2 dist2 = abs(wpos.xz - _BoundsCenter.xz);
            float2 border2 = saturate( (dist2 - BORDER_START_BOX) / BORDER_SIZE_BOX );
            float border = 1.0 - max(border2.x, border2.y);
        #endif
        density.a *= smoothstep(0, 1, border);
   #endif

   #if VF2_DISTANCE
        density.a -= ApplyFogDistance(rayStart, wpos); 
   #endif

   UNITY_BRANCH
   if (density.a > 0) {
        half4 fgCol = baseColor * half4((1.0 - density.a * _DeepObscurance).xxx, density.a);
        #if VF2_RECEIVE_SHADOWS
            if (loop_t < loop_shadowMaxDistance) {
                half shadowAtten = GetLightAttenuation(rotatedWPos);
                fgCol.rgb *= lerp(1.0, shadowAtten, SHADOW_INTENSITY);
                #if defined(FOG_SHADOW_CANCELLATION)
                    fgCol.a *= lerp(1.0, shadowAtten, SHADOW_CANCELLATION);
                #endif
            }
        #endif
        #if VF2_NATIVE_LIGHTS
            #if USE_FORWARD_PLUS && !defined(FOG_FORWARD_PLUS_IGNORE_CLUSTERING)
                // additional directional lights
                #if defined(FOG_FORWARD_PLUS_ADDITIONAL_DIRECTIONAL_LIGHTS)
                    for (uint lightIndex = 0; lightIndex < URP_FP_DIRECTIONAL_LIGHTS_COUNT; lightIndex++) {
                        Light light = GetAdditionalLight(lightIndex, rotatedWPos, 1.0.xxxx);
                        #if defined(_LIGHT_LAYERS)
                            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                        #endif
                        {
                            half distanceAtten = VF2_NATIVE_LIGHT_ATTEN(light);
                            fgCol.rgb += light.color * (distanceAtten * light.shadowAttenuation * NATIVE_LIGHTS_MULTIPLIER);
                        }
                    }
                #endif
                // clustered lights
                {
                    uint lightIndex;
                    ClusterIterator _urp_internal_clusterIterator = ClusterInit(uv, rotatedWPos, 0);
                    [loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) { 
                        lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT;
                        Light light = GetAdditionalLight(lightIndex, rotatedWPos, 1.0.xxxx);
                        #if defined(_LIGHT_LAYERS)
                            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                        #endif
                        {
                            half distanceAtten = VF2_NATIVE_LIGHT_ATTEN(light);
                            fgCol.rgb += light.color * (distanceAtten * light.shadowAttenuation * NATIVE_LIGHTS_MULTIPLIER);
                        }
                    }
                }
            #else
                #if USE_FORWARD_PLUS
                    uint additionalLightCount = min(URP_FP_PROBES_BEGIN, 8); // more than 8 lights is too slow for raymarching
                #else
                    uint additionalLightCount = GetAdditionalLightsCount();
                #endif
                for (uint i = 0; i < additionalLightCount; ++i) {
                    #if UNITY_VERSION >= 202030
                        Light light = GetAdditionalLight(i, rotatedWPos, 1.0.xxxx);
                    #else
                        Light light = GetAdditionalLight(i, rotatedWPos);
                    #endif
                    #if defined(_LIGHT_LAYERS)
                        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                    #endif
                    {
                        half distanceAtten = VF2_NATIVE_LIGHT_ATTEN(light);
                        fgCol.rgb += light.color * (distanceAtten * light.shadowAttenuation * NATIVE_LIGHTS_MULTIPLIER);
                    }
                }
            #endif
        #endif

        #if UNITY_VERSION >= 202310 && defined(VF2_APV)
            fgCol.rgb += GetAPVColor(wpos);
        #endif

        #if VF2_LIGHT_COOKIE
            half3 cookieColor = SampleMainLightCookie(wpos);
            fgCol.rgb *= cookieColor;
            #if defined(V2F_LIGHT_COOKIE_CANCELLATION)
                fgCol.a *= Brightness(cookieColor);
            #endif
        #endif

		#if VF2_DEPTH_GRADIENT
			fgCol *= ApplyDepthGradient(rayStart, wpos);
		#endif

		#if VF2_HEIGHT_GRADIENT
			fgCol *= ApplyHeightGradient(wpos);
		#endif

        fgCol.rgb *= density.rgb * fgCol.aaa;
        #if VF2_FOW
            fgCol *= ApplyFogOfWar(rotatedWPos);
        #endif

        fgCol *= energyStep;
        sum += fgCol * (1.0 - sum.a);
   }
}

half SimpleDiffusionIntensity(half cosTheta, half power) {
    return pow(cosTheta, power);
}

#define FAST_POW_1_5(x) (x * sqrt(x))

half HenyeyGreenstein(half cosTheta, half g) {
    half g2 = g * g;
    half denom = 1.0 + g2 - 2.0 * g * cosTheta;
    return (1.0 - g2) / (4.0 * 3.14159265 * (FAST_POW_1_5(denom)) );
}

half MiePhase(half cosTheta, half g) {
    half g2 = g * g;
    half denom = 1.0 + g2 - 2.0 * g * cosTheta;
    return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + cosTheta * cosTheta) / (FAST_POW_1_5(denom));
}

half GetDiffusionIntensity(float3 viewDir) {
    half cosTheta = max(dot(viewDir, _SunDir.xyz), 0);
    #if VF2_DIFFUSION_SMOOTH
        half diffusion = HenyeyGreenstein(cosTheta, LIGHT_DIFFUSION_POWER);
    #elif VF2_DIFFUSION_STRONG
        half diffusion = MiePhase(cosTheta, LIGHT_DIFFUSION_POWER);
    #else
        half diffusion = SimpleDiffusionIntensity(cosTheta, LIGHT_DIFFUSION_POWER);
    #endif
    return diffusion * LIGHT_DIFFUSION_INTENSITY;
}
 
half3 GetDiffusionColor(float3 viewDir, float t1) {
    half diffusion = GetDiffusionIntensity(viewDir);
    half3 diffusionColor = _LightColor.rgb * (1.0 + diffusion * saturate(dot2(t1 / LIGHT_DIFFUSION_DEPTH_ATTEN)));
    return diffusionColor;
}

half4 GetFogColor(float3 rayStart, float3 viewDir, float2 uv, float t0, float t1) {

    float len = t1 - t0;
    float distanceStepMultiplier;

    // stepping ratio with atten detail with distance and finer step at short distances
    #if VF2_NATIVE_LIGHTS
        float rs = log(t1depth - t0);
    #else
        float rs = log(len);
    #endif
    rs *= saturate(t1 * _NearStepping);
    rs = rs * FOG_STEPPING + MIN_STEPPING;

    half3 diffusionColor = GetDiffusionColor(viewDir, t1);
    half4 lightColor = half4(diffusionColor, 1.0);

    float3 wpos = rayStart + viewDir * t0;
    float3 endPos = rayStart + viewDir * t1;

    #if VF2_SURFACE
        SurfaceComputeEndPoints(wpos, endPos);
    #endif

    rs = max(rs, 1.0 / MAX_ITERATIONS);

    viewDir *= rs;

    half energyStep = min(1.0, _Density * rs);
    half4 sum = half4(0,0,0,0);

    #if VF2_RECEIVE_SHADOWS
        loop_shadowMaxDistance = (SHADOW_MAX_DISTANCE - t0) / len;
    #endif

    // Set the global variable before the loop
    #if defined(_LIGHT_LAYERS)
        meshRenderingLayers = GetMeshRenderingLayer();
    #endif

    // normalize raystep
    rs /= len;
    
    // Use this Unroll macro to support WebGL. Increase 50 value if needed.
    #if defined(WEBGL_COMPATIBILITY_MODE)
        UNITY_UNROLLX(50)
    #elif VF2_LIGHT_COOKIE
        UNITY_LOOP
    #endif
    for (loop_t = 0; loop_t < 1.0; loop_t += rs) {
        AddFog(rayStart, wpos, uv, energyStep, lightColor, sum);
        if (sum.a > 0.99) {
            break;
        }
        wpos += viewDir;
    }
    if (sum.a > 0.99) {
        sum.a = 1;
    } else {
        energyStep = _Density * len * (rs - (loop_t-1.0));
        energyStep = min(1.0, energyStep);
        AddFog(rayStart, endPos, uv, energyStep, lightColor, sum);
    }

    return sum;
}


half4 ComputeFog(float3 wpos, float2 uv) {

    float3 rayStart = GetRayStart(wpos);
    float3 ray = wpos - rayStart;
    float t1 = length(ray);

    #if defined(FOG_ROTATION)
        float3 rayStartNonRotated = rayStart;
        float3 rayDirNonRotated = ray / t1;
        rayStart = RotateInv(rayStart);
        ray = mul((float3x3)_InvRotMatrix, ray);
        float3 rayDir = ray / t1;
    #else
        float3 rayDir = ray / t1;
        float3 rayStartNonRotated = rayStart;
        float3 rayDirNonRotated = rayDir;
    #endif

    #if VF2_SHAPE_SPHERE
        float t0;
        SphereIntersection(rayStart, rayDir, t0, t1);
    #else
        float t0 = BoxIntersection(rayStart, rayDir);
    #endif

    #if defined(FOG_MAX_DISTANCE_XZ)
        float slope = 1.0001 - abs(rayDir.y);
        FOG_MAX_LENGTH /= slope;
    #endif

    SetJitter(uv);

    t1 = min(t1, FOG_MAX_LENGTH); // max distance

    float jiterring = jitter * JITTERING;
    t0 += jiterring;
    t1 += jiterring;

    t1depth = t1;

    CLAMP_RAY_DEPTH(rayStartNonRotated, uv, t1); // clamp to geometry

    #if VF2_DEPTH_PEELING
        CLAMP_RAY_START(rayStartNonRotated, uv, t0); // clamp to start of transparent objects
    #endif

    if (t0 >= t1) return 0;

    half4 fogColor = GetFogColor(rayStart, rayDir, uv, t0, t1);

    // dither
    #if !VF2_DEPTH_PEELING // dithering shouldn't be used when depth peeling is enabled
        fogColor.rgb = max(0, fogColor.rgb - jitter * DITHERING);
    #endif

    // alpha
    fogColor *= _LightColor.a;

    #if VF2_POINT_LIGHTS
        AddPointLights(rayStartNonRotated, rayDirNonRotated, fogColor, t0, t1 - t0);
    #endif

    #if defined(FOG_MAX_DISTANCE_XZ)
        float fallOffFactor = FOG_MAX_LENGTH * FOG_MAX_LENGTH_FALLOFF + 1.0;
        half maxDistanceFallOff = (FOG_MAX_LENGTH - t0) / fallOffFactor;
    #else
        half maxDistanceFallOff = (FOG_MAX_LENGTH - t0) / FOG_MAX_LENGTH_FALLOFF_PRECOMPUTED;
    #endif
    half mdfo_sq = maxDistanceFallOff * maxDistanceFallOff;
    half mdfo_quad = mdfo_sq * mdfo_sq;
    fogColor *= saturate(mdfo_quad);

    return fogColor;
}

#endif // VOLUMETRIC_FOG_2_RAYMARCH