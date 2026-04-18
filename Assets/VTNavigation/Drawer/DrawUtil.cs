using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Geometry;

namespace VTNavigation.Drawer
{
    public static class DrawUtil
    {
        public static void DrawBox2D(Box2D bounds, Color color)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            
            Vector3 b0 = min;
            Vector3 b1 = min + Vector3.right * bounds.Size.x;

            Vector3 u0 = b0 + Vector3.up * bounds.Size.y;
            Vector3 u1 = b1 + Vector3.up * bounds.Size.y;

            DrawLine(b0, b1, color);
            DrawLine(b0, u0, color);

            DrawLine(u0, u1, color);
            DrawLine(b1, u1, color);
        }
        
        public static void DrawBounds(Bounds bounds, Color color)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 b0 = min;
            Vector3 b1 = min + Vector3.forward * bounds.size.z;
            Vector3 b2 = b1 + Vector3.right * bounds.size.x;
            Vector3 b3 = min + Vector3.right * bounds.size.x;

            Vector3 u0 = b0 + Vector3.up * bounds.size.y;
            Vector3 u1 = b1 + Vector3.up * bounds.size.y;
            Vector3 u2 = b2 + Vector3.up * bounds.size.y;
            Vector3 u3 = b3 + Vector3.up * bounds.size.y;

            DrawLine(b0, b1, color);
            DrawLine(b1, b2, color);
            DrawLine(b2, b3, color);
            DrawLine(b0, b3, color);

            DrawLine(u0, u1, color);
            DrawLine(u1, u2, color);
            DrawLine(u2, u3, color);
            DrawLine(u0, u3, color);

            DrawLine(b0, u0, color);
            DrawLine(b1, u1, color);
            DrawLine(b2, u2, color);
            DrawLine(b3, u3, color);

        }

        public static void DrawLine(Vector3 start,  Vector3 end, Color color)
        {
            Debug.DrawLine(start, end, color);
        }

        public static void DrawTriangle(Triangle tri, Color color)
        {
            DrawLine(tri[0], tri[1], color);
            DrawLine(tri[1], tri[2], color);
            DrawLine(tri[2], tri[0], color);
        }

		public static void DrawTriangle(Triangle2D tri, Color color)
		{
			DrawLine(tri[0], tri[1], color);
			DrawLine(tri[1], tri[2], color);
			DrawLine(tri[2], tri[0], color);
		}

		public static void DrawPath(List<Vector2> path)
		{
			
		}
    }
}