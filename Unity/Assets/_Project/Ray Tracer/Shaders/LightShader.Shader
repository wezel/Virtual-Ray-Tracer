// The light shader for both opaque and transparent objects.

Shader "Custom/LightShader"
{
    SubShader
    {
        // We render both the inside and the outside of objects. This way you can enter them if you want.
        // However because of this shadows are not perfect and we get some light phasing through objects on sharp corners.
        Cull off
        
        Pass
        {
            Name "LightPass"
            
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One // Additive blending.
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fwdadd_fullshadows // The _fullshadows is required!

            #include "UnityCG.cginc"
            #include "AutoLight.cginc" // Needed for shadow functions.

            // Light color. A built-in shader variable from "UnityCG.cginc".
            uniform float4 _LightColor0;

            // The shader inputs are the properties defined above.
            uniform float4 _Color;
            uniform float _Ambient;
            uniform float _Diffuse;
            uniform float _Specular;
            uniform float _Shininess;

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
                SHADOW_COORDS(1) // The 1 indicates we use TEXCOORD1 for this field.
            };

            // This function decodes 2 floats to a vector of 3.
            // The encoded floats can only have range(1,0).
            // The formula for f1 = ( x * 256 + y / 2 ) the devision by 2 is to make sure both values don't interfere with eachother.
            // f2 is just z.
            float3 unpackData(float f1, float f2)
            {
                float3 output;
                output.x = floor(f1) / 256;
                output.y = fmod(f1,1) * 2;
                output.z = f2;
                return output;
            }

            // The vertex shader.
            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;

                float4x4 modelMatrix = unity_ObjectToWorld;
                float4x4 modelMatrixInverse = unity_WorldToObject;

                // Calculate the screen position, normal and world position of this vertex.
                output.pos = UnityObjectToClipPos(input.vertex);
                output.normal = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
                output.worldPosition = mul(modelMatrix, input.vertex);
                                
                // The TRANSFER_SHADOW function expects the input to be called v. I know that this copy is not needed,
                // but I refuse to use single letter variables against my will.
                vertexInput v = input;
                TRANSFER_SHADOW(output);
                return output;
            }

            // The fragment shader.
            float4 frag(vertexOutput input, fixed facing : VFACE) : COLOR
            {
                // For now we do the lighting calculation under the assumption that all lights are point light sources.
                // We also ignore light attenuation. Such things should be added if they are added to the ray tracer.
  
                float3 normal = normalize(input.normal);
                if(facing < 0) normal = -normal;
                float3 view = normalize(_WorldSpaceCameraPos - input.worldPosition.xyz);
                float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - input.worldPosition.xyz;
                float3 light = normalize(vertexToLightSource);
                
                float diffuseStrength = max(0.0, dot(light, normal));

                 // return black if the angle is more than 90 degrees between the normal and the light vector.
                if(diffuseStrength <= 0.0) return float4(0,0,0,0);

                float specularStrength = pow(max(0.0, dot(reflect(-light, normal), view)), _Shininess);
                
                // We use the _lightColor0 to transfer the lightColor and the ambient, diffuse and specular light component.
                // lightColor is encoded in .r and .g
                // ambDifSpec is encoded in .b and .a
                // See the unpackData function for more info.
                float3 lightColor = unpackData(_LightColor0.r,_LightColor0.g);
                float3 ambDifSpec = unpackData(_LightColor0.b,_LightColor0.a);

                //float3 ambientColor =                                 ambDifSpec.x * lightColor * _Color.rgb;
                float3 ambientColor =                     _Ambient  * ambDifSpec.x * _Color.rgb;
                float3 diffuseColor =  diffuseStrength  * _Diffuse  * ambDifSpec.y * lightColor * _Color.rgb;
                float3 specularColor = specularStrength * _Specular * ambDifSpec.z * lightColor * _Color.rgb;

                float3 result = ambientColor + diffuseColor + specularColor;
                float attenuation = SHADOW_ATTENUATION(input); // Will be 1.0 for no shadow and 0.0 for full shadow.

                #ifdef SPOT // Attenuate arealighting (currently made of spotlights)
                float3 lightDirection = normalize(mul(float3(0, 0, -1), (float3x3)unity_WorldToLight));
                float angle = dot(lightDirection, light);
                if (angle < 0.04)
                    result *= angle;
                else
                    result *= pow(angle, 0.6);
                #endif

                return float4(result * attenuation, 0);
            }
            ENDCG
        }
    }
}
