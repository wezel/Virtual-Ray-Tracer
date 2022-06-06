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
        _Diffuse("Diffuse", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        // We render both the inside and the outside of objects. This way you can enter them if you want.
        Cull Off

        Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

        // With this pass transparent objects are renderer correctly inside and out. 
        Pass
        {
            ZWrite On
            ColorMask 0
        }

        Pass
        {
            CGPROGRAM
            #include "GeneralShader.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One // Additive blending.
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            // Light color. A built-in shader variable from "UnityCG.cginc".
            uniform float4 _LightColor0;

            // The shader inputs are the properties defined above.
            uniform float4 _Color;
            uniform float _Ambient;
            uniform float _Diffuse;

            // Vertex data input to the vertex shader. For acceptable fields see:
            // http://wiki.unity3d.com/index.php?title=Shader_Code.
            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            // Vertex shader output that is the input of the fragment shader. For acceptable fields see:
            // http://wiki.unity3d.com/index.php?title=Shader_Code.
            struct vertexOutput
            {
                float4 pos : SV_POSITION; // If not named "pos" the build can randomly (not always) fail????
                float3 normal : NORMAL;
                float4 worldPosition : TEXCOORD0; // Here TEXCOORD0 is used as an extra field, not texture coordinates.
            };

            float3 unpackData3(float f1, float f2)
            {
                float3 output;
                output.x = floor(f1) / 256;
                output.y = fmod(f1, 1) * 2;
                output.z = floor(f2) / 256;
                //output.w = fmod(f2, 1) * 2;
                return output;
            }

            // The vertex shader.
            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;

                // Calculate the screen position, normal and world position of this vertex.
                output.pos = UnityObjectToClipPos(input.vertex);
                output.normal = normalize(mul(float4(input.normal, 0.0), unity_WorldToObject).xyz);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex);

                return output;
            }

            // The fragment shader.
            float4 frag(vertexOutput input, fixed facing : VFACE) : COLOR
            {
                // For now we do the lighting calculation under the assumption that all lights are point or spot light sources.  
                // We use the _LightColor0 to transfer the lightColor, intensity, spotAngle, ambient, diffuse and specular components.
                // lightColor and intensity are encoded in .r and .g
                // ambDifSpec and spotAngle are encoded in .b and .a
                // See the unpackData function for more info.
                float3 normal = normalize(input.normal);
                if (facing < 0) normal = -normal;
                float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - input.worldPosition.xyz;
                float3 light = normalize(vertexToLightSource);

#ifdef SPOT
                float spotAngle = fmod(_LightColor0.a, 1) * 2; // The angle after which the spotlight does not shine any furter wrt it's direction
                float3 spotLightDirection = normalize(mul(float3(0, 0, 1), (float3x3)unity_WorldToLight)); // Direction the spotlight points towards
                float angle = dot(spotLightDirection, -light); // The angle between the spotlight's direction and the vector to the fragment

                // return black if the point is outside the scope of the spot light.
                if (angle < spotAngle) return float4(0, 0, 0, 0);
#endif

                float diffuseStrength = max(0.0, dot(light, normal));
                // return black if the angle is >= 90 degrees between the normal and the light vector.
                if (diffuseStrength <= 0.0) return float4(0,0,0,0);

                float3 lightColor = unpackData3(_LightColor0.r, _LightColor0.g);
                float diffuse = fmod(_LightColor0.b, 1) * 2;
                float3 diffuseColor = diffuse * lightColor * _Color.rgb * diffuseStrength * _Diffuse;

                return float4(diffuseColor / 8, 0);
            }
            ENDCG
        }

    }

    FallBack "VertexLit" // Any fall back shader that supports casting shadows will do.
}
