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

            // This function decodes 2 floats to a vector of 4.
            // The encoded floats can only have range(0,1).
            // The formula for f1 = ( x * 256 + y / 2 ) the division by 2 is to make sure both values don't interfere with eachother.
            // The formula for f2 = ( z * 256 + w / 2 ) the division by 2 is to make sure both values don't interfere with eachother.
            float4 unpackData4(float f1, float f2)
            {
                float4 output;
                output.x = floor(f1) / 256;
                output.y = fmod(f1,1) * 2;
                output.z = floor(f2) / 256;
                output.w = fmod(f2, 1) * 2;
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
                // For now we do the lighting calculation under the assumption that all unity-lights are point or spot light sources.  
                // We use the _LightColor0 to transfer the lightColor, intensity, spotAngle, ambient, diffuse and specular components.
                // lightColor and intensity are encoded in .r and .g
                // ambDifSpec, power and spotAngle are encoded in .b and .a
                // See the unpackData4 function for more info.
                float3 normal = normalize(input.normal);
                if (facing < 0) normal = -normal;
                float3 view = normalize(_WorldSpaceCameraPos - input.worldPosition.xyz);
                float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - input.worldPosition.xyz;
                float3 light = normalize(vertexToLightSource);
                float4 ambDifSpecAngle = unpackData4(_LightColor0.b, _LightColor0.a);

#ifdef SPOT
                float spotAngle = ambDifSpecAngle.w; // The angle after which the spotlight does not shine any furter wrt it's direction
                float3 spotLightDirection = normalize(mul(float3(0, 0, 1), (float3x3)unity_WorldToLight)); // Direction the spotlight points towards
                float angle = dot(spotLightDirection, -light); // The angle between the spotlight's direction and the vector to the fragment
                
                // return black if the point is outside the scope of the spot light.
                if (angle < spotAngle) return float4(0, 0, 0, 0);
                float power = floor(ambDifSpecAngle.x / 2) * 64 / 256; // Power is compressed to 0-1 but is actually 0-64.
#endif
                // ambient also contains the Spotlight Attenuation Power. This uses the same principle as in the upackData4 function.
                ambDifSpecAngle.x = fmod(ambDifSpecAngle.x, 2);

                float diffuseStrength = max(0.0, dot(light, normal));
                 // return black if the angle is >= 90 degrees between the normal and the light vector.
                if (diffuseStrength <= 0.0) return float4(0,0,0,0);

                float specularStrength = pow(max(0.0, dot(reflect(-light, normal), view)), _Shininess);                
                // lightDistanceAttenuation bool is packed in with specular. If its bigger than 1, it's enabled. Added 2 to not interfere.
                bool lightDistanceAttenuation = false;
                if (ambDifSpecAngle.z > 1)
                {
                    lightDistanceAttenuation = true;
                    ambDifSpecAngle.z -= 2;
                }
                float4 lightInfo  = unpackData4(_LightColor0.r, _LightColor0.g);
                float3 lightColor = lightInfo.xyz;
                float  intensity  = lightInfo.w * 30; // Intensity range is set to 0 - 30 but packed as 0 - 1.

                float3 ambientColor  = ambDifSpecAngle.x * lightColor * _Color.rgb;
                float3 diffuseColor  = ambDifSpecAngle.y * lightColor * _Color.rgb * diffuseStrength  * _Diffuse;
                float3 specularColor = ambDifSpecAngle.z * lightColor * _Color.rgb * specularStrength * _Specular;

                float3 result = diffuseColor + specularColor; // Add ambient at the end, as it's immune to attenuation

#ifdef SPOT     // Spotlight attenuation
                result *= pow(angle, power);
#endif

                // Light distance attenuation
                if (lightDistanceAttenuation)
                {
                    float distance = length(vertexToLightSource);
                    result /= 0.04 + 0.1 * distance + 0.06 * (distance * distance);
                }

                float attenuation = SHADOW_ATTENUATION(input); // Will be 1.0 for no shadow and 0.0 for full shadow.
                return float4(ambientColor + result * attenuation * intensity, 0);
            }
            ENDCG
        }
    }
}
