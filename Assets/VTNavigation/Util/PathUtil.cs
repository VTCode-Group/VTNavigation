using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Geometry;

namespace VTNavigation.Util
{
    public static class PathUtil
    {
        public static List<Vector3> SmoothPathWithBezierCurve(List<Vector3> points, float smoothDistance = 1.0f, float smoothness = 0.5f, int sampleCount = 16)
        {
            List<Vector3> result = new List<Vector3>();
            if (points == null || points.Count <= 2)
            {
                result.AddRange(points);
                return result;
            }

            result.Add(points[0]);
            
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 prev = result[result.Count - 1];
                Vector3 curr = points[i];
                Vector3 next = points[i + 1];
                
                List<Vector3> controlPoints = new List<Vector3>();
                controlPoints.Add(GetControlPoint(curr,prev, smoothDistance, smoothness));
                controlPoints.Add(curr);
                controlPoints.Add(GetControlPoint(curr,next, smoothDistance, smoothness));
                BezierCurve curve = new BezierCurve(controlPoints);

                for (int j = 0; j <= sampleCount; j++)
                {
                    float t = j / (float)sampleCount;
                    result.Add(curve.Evaluate(t));
                }
                
            }
            
            result.Add(points[points.Count - 1]);
            return result;
        }

        private static Vector3 GetControlPoint(Vector3 from, Vector3 to, float smoothDistance = 1.0f,
            float smoothness = 0.5f)
        {
            Vector3 offset = (to - from);
            float distanceSq = offset.sqrMagnitude;
            float smoothDistanceSq = smoothDistance * smoothDistance;

            if (distanceSq < smoothDistanceSq)
            {
                return from + offset.normalized*Mathf.Sqrt(distanceSq)*smoothness;
            }
            return from + offset.normalized*smoothDistance*smoothness;
        }
    }
}