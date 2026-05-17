Shader "CompassNavigatorPro/Scan"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0.5,1,0.2)
        _Width ("Width", Float) = 100
        _MaxRange("Max Range", Float) = 1000
        _GridColor("Grid Color", Color) = (1,1,0)
        _GridData("Grid Data", Vector) = (150, 0.1, 0, 0)
        _Alpha("Alpha", Float) = 1
        _Center("Center", Vector) = (0,0,0)
        _Filled("Filled", Int) = 0
        _ScanLines("ScanLines", Int) = 1
        _Noise("Noise", Int) = 1
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _EdgeData("Edge Data", Vector) = (0.01, 0.1, 2, 1.0)
        _ColorBlend("Color Blend", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="Yes" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ZClip False
        
        Pass
        {

            PackageRequirements {
                "com.unity.render-pipelines.universal"
            }            

            HLSLPROGRAM
            #pragma target 3.0
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local_fragment _ SCAN_OUTLINE
            #pragma multi_compile_local_fragment _ SCAN_GRID

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "ScanLib.hlsl"


            sampler2D _MainTex;
            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
            float3 _Center;
            half4 _Color;
            float _MaxRange;
            float _Width;
            half4 _GridColor;
            float4 _GridData;
            float _Alpha;
            int _Filled;
            int _ScanLines;
            int _Noise;
            half4 _EdgeColor;
            half4 _EdgeData;
            half4 _ColorBlend;
            CBUFFER_END

            #define GRID_SIZE _GridData.x
            #define GRID_DEPTH_WIDTH_CONTROL _GridData.y
            #define GRID_DEPTH_START _GridData.z

            #define EDGE_DEPTH_THRESHOLD _EdgeData.x
            #define EDGE_COLOR_THRESHOLD _EdgeData.y
            #define EDGE_STRENGTH _EdgeData.z
            #define EDGE_WIDTH _EdgeData.w

            float3 GetWorldPosFromDepth(float2 uv) {
                float depth = SampleSceneDepth(uv);
                #if !UNITY_REVERSED_Z
                    depth = depth * 2.0 - 1.0;
                #endif
                float3 wpos = ComputeWorldSpacePosition(uv, depth, unity_MatrixInvVP);
                return wpos;
            }

            float GetEdgeFactor(float2 uv) {
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * EDGE_WIDTH;
                
                float depthCenter = Linear01Depth(SampleSceneDepth(uv), _ZBufferParams);
                float depthLeft = Linear01Depth(SampleSceneDepth(uv - float2(texelSize.x, 0)), _ZBufferParams);
                float depthRight = Linear01Depth(SampleSceneDepth(uv + float2(texelSize.x, 0)), _ZBufferParams);
                float depthUp = Linear01Depth(SampleSceneDepth(uv - float2(0, texelSize.y)), _ZBufferParams);
                float depthDown = Linear01Depth(SampleSceneDepth(uv + float2(0, texelSize.y)), _ZBufferParams);
                
                // Cálculo de gradientes horizontales/verticales
                float gx = depthRight - depthLeft;  // Gradiente horizontal
                float gy = depthDown - depthUp;     // Gradiente vertical
                float depthDiff = sqrt(gx * gx + gy * gy);  // Magnitud del gradiente
                
                // Umbral adaptativo basado en la profundidad local
                float avgDepth = (depthLeft + depthRight + depthUp + depthDown + depthCenter) / 5.0;
                float adaptiveThreshold = EDGE_DEPTH_THRESHOLD * (1.0 + avgDepth * 2.0);
                float depthEdge = (depthDiff > adaptiveThreshold) ? EDGE_STRENGTH : 0;

                // Sample the 2x2 neighborhood for color and convert to luma
                float3 weights = float3(0.299, 0.587, 0.114); // Rec. 601 luma coefficients
                float luma00 = dot(SampleSceneColor(uv).rgb, weights);
                float luma10 = dot(SampleSceneColor(uv + float2(texelSize.x, 0)).rgb, weights);
                float luma01 = dot(SampleSceneColor(uv + float2(0, texelSize.y)).rgb, weights);
                float luma11 = dot(SampleSceneColor(uv + texelSize).rgb, weights);
                
                // Roberts Cross operator for luma
                float lumaGx = luma00 - luma11;
                float lumaGy = luma10 - luma01;
                float lumaDiff = sqrt(lumaGx * lumaGx + lumaGy * lumaGy);
                float lumaEdge = (lumaDiff > EDGE_COLOR_THRESHOLD) ? EDGE_STRENGTH : 0;
                
                // Combine edges
                return max(depthEdge, lumaEdge);
            }            

            ScanVaryings vert (ScanAttributes input) {
                ScanVaryings output = (ScanVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }


            float4 frag (ScanVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Discard if camera is facing down
                float3 viewDir = unity_WorldToCamera._m20_m21_m22;
                if (viewDir.y < -0.9) {
                    discard;
                }

                float2 uv = input.screenPos.xy / input.screenPos.w;
                float3 wpos = GetWorldPosFromDepth(uv);

                float sceneDist = distance(_Center, wpos);
                float range = distance(_Center, input.positionWS);

                float waveFront = step(sceneDist, range);
                sceneDist = min(sceneDist, _MaxRange);
                float wave = max(0, smoothstep(range - _Width, range, sceneDist)) * 5.0;
                wave = lerp(wave * waveFront, waveFront, _Filled);

                // dither noise
                float ditherScale = 500.0;
                float2 ditherPos = floor(uv * ditherScale) / ditherScale;
                float dither = _Noise ? rand(ditherPos) * 0.5 + 0.5 : 1.0;

                // scan lines
                float scanLineScale = 100;
                float scanLine = frac(uv.y * scanLineScale);
                dither *= _ScanLines ? scanLine: 1.0;

                float4 color = _Color;
                color *= waveFront * wave * dither;

                #if SCAN_GRID
                    // grid effect
                    float3 normal = abs(ComputeNormal(wpos));
                    float gridWidth = max(1, GRID_DEPTH_WIDTH_CONTROL / sceneDist);
                    float3 gpos = (floor(wpos * GRID_SIZE) + 0.5) / GRID_SIZE;
                    float3 gridXYZ = max(0, 0.5 - abs(wpos - gpos) * gridWidth);
                    if (normal.x > 0.5) {
                        gridXYZ.x = 0;
                    }
                    if (normal.y > 0.5) {
                        gridXYZ.y = 0;
                    }
                    if (normal.z > 0.5) {
                        gridXYZ.z = 0;
                    }
                    float grid = 1.5 * max(max(gridXYZ.x, gridXYZ.y), gridXYZ.z) * waveFront;
                    grid *= saturate(sceneDist / GRID_DEPTH_START);
                    color = lerp(color, _GridColor, grid);
                #endif

                #if SCAN_OUTLINE
                    // add edge detection
                    float edgeFactor = GetEdgeFactor(uv);
                    color = lerp(color, _EdgeColor, edgeFactor * waveFront);
                #endif

                color.a = saturate(color.a);
                color = lerp(color, half4(_ColorBlend.rgb,1), (1.0 - color.a) * _ColorBlend.a);
                color.a *= _Alpha;
                
                return color;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="Yes" "RenderPipeline" = "HDRenderPipeline" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ZClip False

        Pass
        {
            PackageRequirements {
                "com.unity.render-pipelines.high-definition"
            }            

            Name "Scan"
            Tags { "LightMode" = "Forward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local_fragment _ SCAN_OUTLINE
            #pragma multi_compile_local_fragment _ SCAN_GRID

            #define SHADERPASS SHADERPASS_FORWARD_UNLIT
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "ScanLib.hlsl"


            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float3 _Center;
                half4 _Color;
                float _MaxRange;
                float _Width;
                half4 _GridColor;
                float4 _GridData;
                float _Alpha;
                int _Filled;
                int _ScanLines;
                int _Noise;
                half4 _EdgeColor;
                half4 _EdgeData;
                half4 _ColorBlend;
            CBUFFER_END

            #define GRID_SIZE _GridData.x
            #define GRID_DEPTH_WIDTH_CONTROL _GridData.y
            #define GRID_DEPTH_START _GridData.z

            #define EDGE_DEPTH_THRESHOLD _EdgeData.x
            #define EDGE_COLOR_THRESHOLD _EdgeData.y
            #define EDGE_STRENGTH _EdgeData.z
            #define EDGE_WIDTH _EdgeData.w
            ScanVaryings vert(ScanAttributes input)
            {
                ScanVaryings output = (ScanVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.screenPos = output.positionCS;

                return output;
            }


            float2 ComputeViewportPosition(float4 positionCS) {
                #if UNITY_UV_STARTS_AT_TOP
                    positionCS.y = -positionCS.y;
                #endif
                positionCS *= rcp(positionCS.w);
                positionCS.xy = positionCS.xy * 0.5 + 0.5;
                return positionCS.xy;
            }            


            float GetEdgeFactor(float2 uv) {
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * EDGE_WIDTH;
                
                float depthCenter = Linear01Depth(SampleCameraDepth(uv), _ZBufferParams);
                float depthLeft = Linear01Depth(SampleCameraDepth(uv - float2(texelSize.x, 0)), _ZBufferParams);
                float depthRight = Linear01Depth(SampleCameraDepth(uv + float2(texelSize.x, 0)), _ZBufferParams);
                float depthUp = Linear01Depth(SampleCameraDepth(uv - float2(0, texelSize.y)), _ZBufferParams);
                float depthDown = Linear01Depth(SampleCameraDepth(uv + float2(0, texelSize.y)), _ZBufferParams);
                
                // Cálculo de gradientes horizontales/verticales
                float gx = depthRight - depthLeft;  // Gradiente horizontal
                float gy = depthDown - depthUp;     // Gradiente vertical
                float depthDiff = sqrt(gx * gx + gy * gy);  // Magnitud del gradiente
                
                // Umbral adaptativo basado en la profundidad local
                float avgDepth = (depthLeft + depthRight + depthUp + depthDown + depthCenter) / 5.0;
                float adaptiveThreshold = EDGE_DEPTH_THRESHOLD * (1.0 + avgDepth * 2.0);
                float depthEdge = (depthDiff > adaptiveThreshold) ? EDGE_STRENGTH : 0;
                
                // Sample the 2x2 neighborhood for color and convert to luma
                float3 weights = float3(0.299, 0.587, 0.114); // Rec. 601 luma coefficients
                float luma00 = dot(SampleCameraColor(uv).rgb, weights);
                float luma10 = dot(SampleCameraColor(uv + float2(texelSize.x, 0)).rgb, weights);
                float luma01 = dot(SampleCameraColor(uv + float2(0, texelSize.y)).rgb, weights);
                float luma11 = dot(SampleCameraColor(uv + texelSize).rgb, weights);
                
                // Roberts Cross operator for luma
                float lumaGx = luma00 - luma11;
                float lumaGy = luma10 - luma01;
                float lumaDiff = sqrt(lumaGx * lumaGx + lumaGy * lumaGy);
                float lumaEdge = (lumaDiff > EDGE_COLOR_THRESHOLD) ? EDGE_STRENGTH : 0;
                
                // Combine edges
                return max(depthEdge, lumaEdge);
            } 

            float4 frag(ScanVaryings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Discard if camera is facing down
                float3 viewDir = GetViewForwardDir();
                if (viewDir.y < -0.9) {
                    discard;
                }

                float2 uv = ComputeViewportPosition(input.screenPos);

                float depth = SampleCameraDepth(uv);
                #if !UNITY_REVERSED_Z
                    depth = depth * 2.0 - 1.0;
                #endif
                float3 wpos = GetAbsolutePositionWS(ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP));

                float sceneDist = distance(_Center, wpos);
                float range = distance(_Center, input.positionWS);

                float waveFront = step(sceneDist, range);
                sceneDist = min(sceneDist, _MaxRange);
                float wave = max(0, smoothstep(range - _Width, range, sceneDist)) * 5.0;
                wave = lerp(wave * waveFront, waveFront, _Filled);

                // dither noise
                float ditherScale = 500.0;
                float2 ditherPos = floor(uv * ditherScale) / ditherScale;
                float dither = _Noise ? rand(ditherPos) * 0.5 + 0.5 : 1.0;

                // scan lines
                float scanLineScale = 100;
                float scanLine = frac(uv.y * scanLineScale);
                dither *= _ScanLines ? scanLine: 1.0;

                float4 color = _Color;
                color *= waveFront * wave * dither;

                #if SCAN_GRID
                // grid effect
                float3 normal = abs(ComputeNormal(wpos));
                float gridWidth = max(1, GRID_DEPTH_WIDTH_CONTROL / sceneDist);
                float3 gpos = (floor(wpos * GRID_SIZE) + 0.5) / GRID_SIZE;
                float3 gridXYZ = max(0, 0.5 - abs(wpos - gpos) * gridWidth);
                if (normal.x > 0.5) {
                    gridXYZ.x = 0;
                }
                if (normal.y > 0.5) {
                    gridXYZ.y = 0;
                }
                if (normal.z > 0.5) {
                    gridXYZ.z = 0;
                }
                float grid = 1.5 * max(max(gridXYZ.x, gridXYZ.y), gridXYZ.z) * waveFront;
                grid *= saturate(sceneDist / GRID_DEPTH_START);
                color = lerp(color, _GridColor, grid);
            #endif

                #if SCAN_OUTLINE
                    // add edge detection
                    float edgeFactor = GetEdgeFactor(uv);
                    color = lerp(color, _EdgeColor, edgeFactor * waveFront);
                #endif

                color.a = saturate(color.a);
                color = lerp(color, half4(_ColorBlend.rgb,1), (1.0 - color.a) * _ColorBlend.a);
                color.a *= _Alpha;
                
                return color;
            }
            ENDHLSL
        }
    }    

    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="Yes" }

        GrabPass { }        

        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            ZClip False

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local_fragment _ SCAN_OUTLINE
            #pragma multi_compile_local_fragment _ SCAN_GRID

            #include "UnityCG.cginc"
            #include "ScanLib.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float3 _Center;
            half4 _Color;
            float _MaxRange;
            float _Width;
            half4 _GridColor;
            float4 _GridData;
            float _Alpha;
            int _Filled;
            int _ScanLines;
            int _Noise;
            half4 _EdgeColor;
            half4 _EdgeData;
            half4 _ColorBlend;
            sampler2D _GrabTexture;

            #define GRID_SIZE _GridData.x
            #define GRID_DEPTH_WIDTH_CONTROL _GridData.y
            #define GRID_DEPTH_START _GridData.z
            #define EDGE_DEPTH_THRESHOLD _EdgeData.x
            #define EDGE_COLOR_THRESHOLD _EdgeData.y
            #define EDGE_STRENGTH _EdgeData.z
            #define EDGE_WIDTH _EdgeData.w
            ScanVaryings vert (ScanAttributes v)
            {
                ScanVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(ScanVaryings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.positionCS = UnityObjectToClipPos(v.positionOS);
                o.positionWS = mul(unity_ObjectToWorld, v.positionOS).xyz;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            float GetEdgeFactor(float2 uv) {
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * EDGE_WIDTH;
                
                float depthCenter = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                float depthLeft = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv - float2(texelSize.x, 0)));
                float depthRight = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + float2(texelSize.x, 0)));
                float depthUp = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv - float2(0, texelSize.y)));
                float depthDown = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + float2(0, texelSize.y)));
                
                // Cálculo de gradientes horizontales/verticales
                float gx = depthRight - depthLeft;  // Gradiente horizontal
                float gy = depthDown - depthUp;     // Gradiente vertical
                float depthDiff = sqrt(gx * gx + gy * gy);  // Magnitud del gradiente
                
                // Umbral adaptativo basado en la profundidad local
                float avgDepth = (depthLeft + depthRight + depthUp + depthDown + depthCenter) / 5.0;
                float adaptiveThreshold = EDGE_DEPTH_THRESHOLD * (1.0 + avgDepth * 2.0);
                
                // Detección final de bordes
                float depthEdge = (depthDiff > adaptiveThreshold) ? EDGE_STRENGTH : 0;
                
                // Sample the 2x2 neighborhood for color and convert to luma
                float3 weights = float3(0.299, 0.587, 0.114); // Rec. 601 luma coefficients
                float luma00 = dot(tex2D(_GrabTexture, uv).rgb, weights);
                float luma10 = dot(tex2D(_GrabTexture, uv + float2(texelSize.x, 0)).rgb, weights);
                float luma01 = dot(tex2D(_GrabTexture, uv + float2(0, texelSize.y)).rgb, weights);
                float luma11 = dot(tex2D(_GrabTexture, uv + texelSize).rgb, weights);
                
                // Roberts Cross operator for luma
                float lumaGx = luma00 - luma11;
                float lumaGy = luma10 - luma01;
                float lumaDiff = sqrt(lumaGx * lumaGx + lumaGy * lumaGy);
                float lumaEdge = 0; //(lumaDiff > EDGE_COLOR_THRESHOLD) ? EDGE_STRENGTH : 0;
                
                // Combine edges
                return max(depthEdge, lumaEdge);
            } 

            fixed4 frag (ScanVaryings i) : SV_Target
            {
                // Discard if camera is facing down
                float3 viewDir = UNITY_MATRIX_V[2].xyz;
                if (viewDir.y < -0.9) {
                    discard;
                }

                float2 uv = i.screenPos.xy / i.screenPos.w;

                float3 camRelPos = i.positionWS - _WorldSpaceCameraPos;
                float vz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                float3 viewPlane = camRelPos / dot(camRelPos, viewDir);
                float3 wpos = viewPlane * vz + _WorldSpaceCameraPos;

                float sceneDist = distance(_Center, wpos);
                float range = distance(_Center, i.positionWS);

                float waveFront = step(sceneDist, range);
                sceneDist = min(sceneDist, _MaxRange);
                float wave = max(0, smoothstep(range - _Width, range, sceneDist)) * 5.0;
                wave = lerp(wave * waveFront, waveFront, _Filled);

                // dither noise
                float ditherScale = 500.0;
                float2 ditherPos = floor(uv * ditherScale) / ditherScale;
                float dither = _Noise ? rand(ditherPos) * 0.5 + 0.5 : 1.0;

                // scan lines
                float scanLineScale = 100;
                float scanLine = frac(uv.y * scanLineScale);
                dither *= _ScanLines ? scanLine: 1.0;

                float4 color = _Color;
                color *= waveFront * wave * dither;

                #if SCAN_GRID
                    // grid effect
                    float3 normal = abs(ComputeNormal(wpos));
                    float gridWidth = max(1, GRID_DEPTH_WIDTH_CONTROL / sceneDist);
                    float3 gpos = (floor(wpos * GRID_SIZE) + 0.5) / GRID_SIZE;
                    float3 gridXYZ = max(0, 0.5 - abs(wpos - gpos) * gridWidth);
                    if (normal.x > 0.5) {
                        gridXYZ.x = 0;
                    }
                    if (normal.y > 0.5) {
                        gridXYZ.y = 0;
                    }
                    if (normal.z > 0.5) {
                        gridXYZ.z = 0;
                    }
                    float grid = 1.5 * max(max(gridXYZ.x, gridXYZ.y), gridXYZ.z) * waveFront;
                    grid *= saturate(sceneDist / GRID_DEPTH_START);
                    color = lerp(color, _GridColor, grid);
                #endif

                #if SCAN_OUTLINE
                    // add edge detection
                    float edgeFactor = GetEdgeFactor(uv);
                    color = lerp(color, _EdgeColor, edgeFactor * waveFront);
                #endif

                color.a = saturate(color.a);
                color = lerp(color, half4(_ColorBlend.rgb,1), (1.0 - color.a) * _ColorBlend.a);
                color.a *= _Alpha;

                return color;
            }
            ENDCG
        }
    }
    
    
}
