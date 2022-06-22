// Phong shader to match the ray tracer. Resources used:
// https://janhalozan.com/2017/08/12/phong-shader/
// http://viclw17.github.io/2016/04/19/phong-shading-model-walkthrough/
// Shadow support based on:
// https://docs.unity3d.com/560/Documentation/Manual/SL-VertexFragmentShaderExamples.html
// Note that #pragma multi_compile_fwdadd_fullshadows is required in the add pass for point light shadows.

Shader "Custom/RayTracerShaderTransparentFast"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Ambient("Ambient", Range(0, 1)) = 0.2
    }

        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        // We render both the inside and the outside of objects. This way you can enter them if you want.
        Cull Off

        Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

        Pass
        {
            CGPROGRAM
            #include "GeneralShader.cginc"
            ENDCG
        }

    }

    FallBack "VertexLit" // Any fall back shader that supports casting shadows will do.
}