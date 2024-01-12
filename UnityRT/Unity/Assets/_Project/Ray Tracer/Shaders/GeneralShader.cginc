// This pass is for ambient light and is used by both transparent and opaque objects.
// Sadly this doesn't work for transparent objects if it's done via usepass.

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase

#include "UnityCG.cginc"

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

// The vertex shader.
float4 vert(vertexInput input) : SV_POSITION
{
    return UnityObjectToClipPos(input.vertex);
}

// The fragment shader.
float4 frag() : COLOR
{
    return float4(_Ambient * _Color.rgb, _Color.a);
}