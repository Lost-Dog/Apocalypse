Shader "Rutz Studios/Cel Shader Alpha Blending" 
{
    Properties
    {
        [Header(Shader Settings)]
        [Toggle(USE_GLOSS)] _UseGloss("Use Gloss Shader Variant", Int) = 0
        [Toggle(USE_NORMAL)] _UseNormal("Use Normal Shader Variant", Int) = 0
        [Toggle(USE_GRADIENT)] _UseGradient("Use Gradient Shader Variant", Int) = 0
        [KeywordEnum(Off, Front, Back)] _Cull("Culling", Int) = 2.0

        [Header(Color)]
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

        _BumpMap ("Normalmap", 2D) = "bump" {}

        [Header(Gradient Settings)]
        _GradientColor ("Gradient Color", Color) = (1,1,1)
        _GradientFalloff("Gradient Falloff", Float) = 1
        _GradientOffset("Gradient Offset", Float) = 0

        [Header(Gloss Settings)]
        _GlossStrength("Gloss Strength", Range(0,1)) = 0.15
        _Glossiness("Glossiness", Float) = 1.8
        _GlossSmoothness("Gloss Smoothness", Range(0, 1)) = 1

        [Header(Shadow Settings)]
        _ShadowCol ("Shadow Color", Color) = (0.5,0.5,0.5)
        _ShadowFalloff("Shadow Falloff", Range(0, 1)) = 0.25
        _ShadowSmoothness("Shadow Smoothness", Range(0, 1)) = 0.01

        _RimStrength ("Rim Strength", Range (0, 1)) = .25
        
    }

    SubShader
    {
        Tags 
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "LightMode" = "ForwardBase"
            "PassFlags" = "OnlyDirectional"
        }
        Pass
        {  
            Cull [_Cull]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            //shader features
            #pragma shader_feature USE_GLOSS
            #pragma shader_feature USE_NORMAL
            #pragma shader_feature USE_GRADIENT
            
            #include "CelLighting.cginc"

            ENDCG
        }

        //Uncomment this line to enable shadows for alpha blended materials
        //might cause trouble later
        //UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    } 
    Fallback "Transparent/VertexLit"
}
