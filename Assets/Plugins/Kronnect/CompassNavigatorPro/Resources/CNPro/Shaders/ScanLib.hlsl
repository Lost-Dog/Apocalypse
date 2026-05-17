#ifndef SCANLIB_INCLUDED
#define SCANLIB_INCLUDED


            struct ScanAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ScanVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            float rand(float2 co){
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            #define dot2(x) dot(x,x)

            float3 ComputeNormal(float3 wpos)
            {
                // Compute the partial derivatives of the world position
                float3 dpdx = ddx(wpos);
                float3 dpdy = ddy(wpos);
                
                // Compute the cross product to get the normal
                float3 normal = normalize(cross(dpdx, dpdy));
            
                return normal;
            }            


#endif  // SCANLIB_INCLUDED