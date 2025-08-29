using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTNavigation.Geometry
{
	public static class GeometryUtil
	{
		public static float CaculatePathLength(List<Vector3> path)
		{
			if (path.Count <= 1) return 0.0f;
			float sum = 0.0f;
			for (int i = 1; i < path.Count; i++)
			{
				sum += Vector3.Distance(path[i], path[i - 1]);
			}
			return sum;
		}

		public static bool OverlapTriangle(Bounds bounds, Vector3[] triangle)
		{
			Vector3 extends = bounds.extents;
			Vector3 v0 = triangle[0] - bounds.center;
			Vector3 v1 = triangle[1] - bounds.center;
			Vector3 v2 = triangle[2] - bounds.center;

			Vector3 f0 = v1 - v0;
			Vector3 f1 = v2 - v1;
			Vector3 f2 = v0 - v2;

			float[] axes0 = {
				0, -f0.z, f0.y, 0, -f1.z, f1.y, 0, -f2.z, f2.y,
				f0.z, 0, -f0.x, f1.z, 0, -f1.x, f2.z, 0, -f2.x,
				-f0.y, f0.x, 0, -f1.y, f1.x, 0, -f2.y, f2.x, 0
			};

			if (!SatForAxes(axes0, v0, v1, v2, extends))
			{
				return false;
			}

			Vector3 triangleNormal = Vector3.Cross(f0, f1);
			float[] axes2 = { triangleNormal.x, triangleNormal.y, triangleNormal.z };
			return SatForAxes(axes2, v0, v1, v2, extends);
		}

		public static bool ContainTriangle(Bounds bounds, Triangle triangle)
		{
			return bounds.Contains(triangle[0]) && bounds.Contains(triangle[1]) && bounds.Contains(triangle[2]);
		}

		public static bool OverlapTriangle(Bounds bounds, Triangle triangle)
		{
			if (ContainTriangle(bounds, triangle))
			{
				return true;
			}

			Vector3 extends = bounds.extents;
			Vector3 v0 = triangle[0] - bounds.center;
			Vector3 v1 = triangle[1] - bounds.center;
			Vector3 v2 = triangle[2] - bounds.center;

			Vector3 f0 = v1 - v0;
			Vector3 f1 = v2 - v1;
			Vector3 f2 = v0 - v2;

			float[] axes0 = {
				0, -f0.z, f0.y, 0, -f1.z, f1.y, 0, -f2.z, f2.y,
				f0.z, 0, -f0.x, f1.z, 0, -f1.x, f2.z, 0, -f2.x,
				-f0.y, f0.x, 0, -f1.y, f1.x, 0, -f2.y, f2.x, 0
			};

			if (!SatForAxes(axes0, v0, v1, v2, extends))
			{
				return false;
			}

			float[] axes1 = {
				1, 0, 0, 0, 1, 0, 0, 0, 1
			};
			if (!SatForAxes(axes1, v0, v1, v2, extends))
			{
				return false;
			}

			// finally testing the face normal of the triangle
			// use already existing triangle edge vectors here
			Vector3 triangleNormal = Vector3.Cross(f0, f1);
			float[] axes2 = { triangleNormal.x, triangleNormal.y, triangleNormal.z };
			return SatForAxes(axes2, v0, v1, v2, extends);
		}

		private static bool SatForAxes(float[] axes, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 extents)
		{
			for (int i = 0, j = axes.Length - 3; i <= j; i += 3)
			{

				Vector3 testAxis = new Vector3(axes[i], axes[i + 1], axes[i + 2]);
				// project the aabb onto the separating axis
				float r = extents.x * Mathf.Abs(testAxis.x) + extents.y * Mathf.Abs(testAxis.y) + extents.z * Mathf.Abs(testAxis.z);
				// project all 3 vertices of the triangle onto the separating axis
				float p0 = Vector3.Dot(v0, testAxis);
				float p1 = Vector3.Dot(v1, testAxis);
				float p2 = Vector3.Dot(v2, testAxis);
				// actual test, basically see if either of the most extreme of the triangle points intersects r
				if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
				{

					// points of the projected triangle are outside the projected half-length of the aabb
					// the axis is separating and we can exit
					return false;

				}

			}

			return true;
		}

		public static bool BoundsIntersect(Bounds boundsLeft, Bounds boundsRight)
		{
			if (ContainsBounds(boundsLeft, boundsRight))
			{
				return true;
			}

			return OverlapBounds(boundsLeft, boundsRight);
		}

		private static bool OverlapBounds(Bounds boundsLeft, Bounds boundsRight)
		{
			if (!( boundsRight.max.x <= boundsLeft.min.x ||  boundsRight.min.x >= boundsLeft.max.x ||
				   boundsRight.max.y <= boundsLeft.min.y ||  boundsRight.min.y >= boundsLeft.max.y ||
				   boundsRight.max.z <= boundsLeft.min.z ||  boundsRight.min.z >= boundsLeft.max.z))
			{
				return true;
			}
			return false;
		}

		private static bool ContainsBounds(Bounds bounds, Bounds objBounds)
		{
			return bounds.min.x <= objBounds.min.x && objBounds.max.x <= bounds.max.x &&
				   bounds.min.y <= objBounds.min.y && objBounds.max.y <= bounds.max.y &&
				   bounds.min.z <= objBounds.min.z && objBounds.max.z <= bounds.max.z;
		}

		public static bool ContainsPoint(Vector3 min, Vector3 max, Vector3 point)
		{
			return min.x <= point.x && point.x <= max.x &&
				   min.y <= point.y && point.y <= max.y &&
				   min.z <= point.z && point.z <= max.z;
		}

		public static Bounds GetTriangleAABB(Vector3[] triangles)
		{
			Vector3 min = new Vector3(9999999, 999999, 99999);
			Vector3 max = -min;
			for(int i = 0;i< triangles.Length; i++)
			{
				min = Vector3.Min(min, triangles[i]);
				max = Vector3.Max(max, triangles[i]);
			}
			Vector3 center = (min + max) * 0.5f;
			Vector3 size = max - min;
			return new Bounds(center, size);
		}

		public static Bounds GetTriangleAABB(List<Triangle> triangles)
		{
			Vector3 min = new Vector3(9999999, 999999, 99999);
			Vector3 max = -min;
			for (int i = 0; i < triangles.Count; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					min = Vector3.Min(min, triangles[i][j]);
					max = Vector3.Max(max, triangles[i][j]);
				}
			}
			Vector3 center = (min + max) * 0.5f;
			Vector3 size = max - min;
			return new Bounds(center, size);
		}

		public static Bounds GetTerrainAABB(List<Bounds> boundsList)
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = -min;
			foreach(Bounds bounds in boundsList)
			{
				min = Vector3.Min(min, bounds.min);
				max = Vector3.Max(max, bounds.max);
			}
			Vector3 center = (max + min) * 0.5f;
			Vector3 size = max - min;
			return new Bounds(center, size);
		}

		public static Bounds MakeBoundsFromMinMax(Vector3 min, Vector3 max)
		{
			Vector3 center = (min + max) * 0.5f;
			Vector3 size = max - min;
			return new Bounds(center, size);
		}

		private static Mesh BoundsToMesh(Bounds bounds)
		{
			Vector3 min = bounds.min;
			Vector3 size = bounds.size;
			
			Mesh mesh = new Mesh();
			Vector3[] vertices = new Vector3[]
			{
				min,
				min + Vector3.forward*size.z,
				min + Vector3.forward*size.z + Vector3.right*size.x,
				min + Vector3.right*size.x,

				min + Vector3.up*size.y,
				min + Vector3.up*size.y + Vector3.forward*size.z,
				min + Vector3.up*size.y + Vector3.forward*size.z + Vector3.right*size.x,
				min + Vector3.up*size.y + Vector3.right*size.x
			};
			mesh.vertices = vertices;

			int[] triangles = new int[]
			{
				0,1,2,0,2,3,
				0,1,5,0,5,4,
				0,4,7,0,7,3,
				1,5,6,1,6,2,
				2,6,7,2,7,3,
				4,5,6,4,6,7
			};
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			return mesh;
		}

		public static Mesh GenerateStandardCubeMesh()
		{
			Bounds bounds = new Bounds(new Vector3(0,0,0), Vector3.one);
			return BoundsToMesh(bounds);
		}
	}
}
