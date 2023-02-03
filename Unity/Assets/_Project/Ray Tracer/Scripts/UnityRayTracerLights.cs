using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using _Project.UI.Scripts;
using _Project.UI.Scripts.Render_Image_Window;
using System.Collections;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Spot_Light;

namespace _Project.Ray_Tracer.Scripts
{
    public partial class UnityRayTracer
    {
        private int areaRayLimit = 4;
		
		private float CalculateAttenuation(float lightDistance, RTLight light, in HitInfo hitInfo)
        {

            if (light.Type == RTLight.RTLightType.Point) return 1;
            
            // We have to check if the object is outside the range of a area/spotlight
            // Use the lightVector to the light's origin for arealights, such that all rays to the area in or out of range
            float angle = Vector3.Dot(light.transform.forward, (hitInfo.Point - light.Position).normalized);

            if (angle < Mathf.Cos(light.SpotAngle * Mathf.PI / 360f)) return 0; // Subtract 0.01f to not go through the light.
            
            float attenuation = 1;
            
            // Spotlight attenuation
            if (light.Type != RTLight.RTLightType.Point)
                // Angle is always positive; position has been checked before.
                attenuation = Mathf.Pow(angle, light.SpotAttenuationPower);
            
            // Light distance attenuation
            if (light.LightDistanceAttenuation)
                 attenuation /= 0.04f + 0.1f * lightDistance + 0.06f * (lightDistance * lightDistance);
            
            return attenuation;

        }
        private void TracePointSpotLight(ref TreeNode<RTRay> rayTree, RTLight light, in HitInfo hitInfo)
        {
            Vector3 lightVector = (light.transform.position - hitInfo.Point).normalized;
            if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                rayTree.AddChild(TraceLight(ref lightVector, light.transform.position, light, in hitInfo));
        }

        private void TraceAreaLight(ref TreeNode<RTRay> rayTree, RTAreaLight arealight, in HitInfo hitInfo)
        {
            Vector3 lightVector = (arealight.Position - hitInfo.Point).normalized;
            if (Vector3.Dot(hitInfo.Normal, lightVector) < 0.0f) return; // Do not trace if it's on the wrong side.

            float lightDistance = Vector3.Dot(lightVector, arealight.Position - hitInfo.Point);
            float angle = Vector3.Dot(arealight.transform.forward, -lightVector);

            // The hitpoint is behind the arealight
            if (angle < Mathf.Cos(arealight.SpotAngle * Mathf.PI / 360f) && arealight.LightSamples > areaRayLimit)
            {
                rayTree.AddChild(new RTRay(hitInfo.Point, lightVector, lightDistance, Color.black,
                    RTRay.RayType.AreaShadow, arealight.GetWorldCorners()));

                return;
            }

            // The hitpoint is in front of the arealight
            TreeNode<RTRay> subRayTree = new TreeNode<RTRay>(new RTRay());
            int samples = arealight.LightSamples * arealight.LightSamples;
            foreach (Vector3 point in arealight.SampleLight())
            {
                lightVector = (point - hitInfo.Point).normalized;
                subRayTree.AddChild(TraceLight(ref lightVector, point, arealight, in hitInfo) / samples);
            }

            // If there are more rays generated than the limit and all rays are the same type, try to turn all of them into a single arearay 
            if (arealight.LightSamples > areaRayLimit)
            {

                // check if they are light rays
                if (subRayTree.Children.TrueForAll(child => child.Data.Type == RTRay.RayType.Light))
                {
                    // All rays are light rays. Sum colors and make it a single arealightray
                    Color color = Color.black;
                    subRayTree.Children.ForEach(child => color += child.Data.Color);
                    rayTree.AddChild(new RTRay(hitInfo.Point, lightVector, lightDistance, ClampColor(color),
                        RTRay.RayType.AreaLight, arealight.GetWorldCorners()));

                    return;
                }

                // check if they are shadow rays
                if (subRayTree.Children.TrueForAll(child => child.Data.Type == RTRay.RayType.Shadow))
                {
                    // All rays are shadow rays. Take the minimal distance and make it a single areashadowray
                    float distance = Mathf.Infinity;
                    subRayTree.Children.ForEach(child => distance = Mathf.Min(distance, child.Data.Length));
                    rayTree.AddChild(new RTRay(hitInfo.Point, lightVector, distance, Color.black,
                        RTRay.RayType.AreaShadow, arealight.GetWorldCorners()));

                    return;
                }
            }

            // There are different types of rays, add every one to the rayTree
            foreach (TreeNode<RTRay> child in subRayTree.Children)
                rayTree.AddChild(child);

        }

        private Color TracePointSpotLightImage(RTLight light, in HitInfo hitInfo)
        {
            Vector3 lightVector = (light.transform.position - hitInfo.Point).normalized;
            if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                return TraceLightImage(ref lightVector, light.Position, light, in hitInfo);
            return Color.black;
        }

        private Color TraceAreaLightImage(RTAreaLight areaLight, in HitInfo hitInfo)
        {
            Color color = Color.black;
            Vector3 lightVector = (areaLight.Position - hitInfo.Point).normalized;
            if (Vector3.Dot(hitInfo.Normal, lightVector) < 0.0f) return color; // Do not trace if it's on the wrong side.

            int samples = areaLight.LightSamples * areaLight.LightSamples;
            foreach (Vector3 point in areaLight.SampleLight())
            {
                lightVector = (point - hitInfo.Point).normalized;
                color += TraceLightImage(ref lightVector, point, areaLight, in hitInfo) / samples;
            }

            return color;
        }
    }
}