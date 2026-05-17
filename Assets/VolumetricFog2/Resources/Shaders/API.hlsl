#ifndef VOLUMETRIC_FOG_2_API
#define VOLUMETRIC_FOG_2_API

//#pragma multi_compile _ VF2_DEPTH_PREPASS VF2_DEPTH_PEELING
#pragma multi_compile_local_fragment VF2_SHAPE_BOX VF2_SHAPE_SPHERE
#pragma multi_compile_local_fragment _ VF2_DETAIL_NOISE VF2_CONSTANT_DENSITY
#pragma shader_feature_local_fragment VF2_DISTANCE
#pragma shader_feature_local_fragment VF2_VOIDS
#pragma shader_feature_local_fragment VF2_FOW
#pragma shader_feature_local_fragment VF2_SURFACE
#pragma shader_feature_local_fragment VF2_DEPTH_GRADIENT
#pragma shader_feature_local_fragment VF2_HEIGHT_GRADIENT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

#include "Input.hlsl"
#include "CommonsURP.hlsl"
#include "Primitives.cginc"
#include "FogVoids.cginc"
#include "FogOfWar.cginc"
#include "FogDistance.cginc"
#include "Surface.cginc"
#include "Raymarch2D.cginc"

#endif // VOLUMETRIC_FOG_2_API

