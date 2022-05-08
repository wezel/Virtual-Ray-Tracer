//#ifndef GETLIGHT_INCLUDED
//#define GETLIGHT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightDefinition.cs.hlsl"

void MainLight_float(out float3 Direction, out float3 Color)
{
#ifdef SHADERGRAPH_PREVIEW
	Direction = float3(0.5, 0.5, 0);
	Color = 1;
#else
	Light light = GetMainLight();
	Direction = light.direction;
	Color = light.color;
#endif
}
//#endif