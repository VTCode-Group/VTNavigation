using System.Collections.Generic;
using UnityEngine;

namespace VTNavigation.Geometry
{
	public static class GeometryUtil
	{
		public static bool BoxIntersect(Box2D boxLeft, Box2D boxRight)
		{
			if (ContainsBox2D(boxLeft, boxRight))
			{
				return true;
			}

			return OverlapBox2D(boxLeft, boxRight);
		}

		public static bool ContainsBox2D(Box2D boxLeft, Box2D boxRight)
		{
			return boxLeft.min.x <= boxRight.min.x && boxRight.max.x <= boxLeft.max.x &&
				   boxLeft.min.y <= boxRight.min.y && boxRight.max.y <= boxLeft.max.y;
		}

		private static bool OverlapBox2D(Box2D boundsLeft, Box2D boundsRight)
		{
			if (!(boundsRight.max.x <= boundsLeft.min.x || boundsRight.min.x >= boundsLeft.max.x ||
				   boundsRight.max.y <= boundsLeft.min.y || boundsRight.min.y >= boundsLeft.max.y))
			{
				return true;
			}
			return false;
		}

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

		public static bool OverlapTriangle(Box2D box, Triangle2D triangle)
		{
			for (int i = 0; i < 3; i++)
			{
				if (ContainsPoint(box.min, box.max, triangle[i]))
				{
					return true;
				}
			}

			Vector2[] satAxies = new Vector2[5];

			satAxies[0] = Vector2.right;
			satAxies[1] = Vector2.up;

			Vector2 e0 = (triangle[1] - triangle[0]).normalized;
			Vector2 e1 = (triangle[2] - triangle[0]).normalized;
			Vector2 e2 = (triangle[2] - triangle[1]).normalized;

			satAxies[2] = new Vector2(-e0.y, e0.x);
			satAxies[3] = new Vector2(-e1.y, e1.x);
			satAxies[4] = new Vector2(-e2.y, e2.x);

			Vector2[] triPoints = new Vector2[] { triangle[0], triangle[1], triangle[2] };

			Vector2[] boxPoints = new Vector2[] { box.min, box.max, box.min + Vector2.right * box.Size.x, box.min + Vector2.up * box.Size.y };

			for (int i = 0; i < satAxies.Length; i++)
			{
				Vector2 satAxis = satAxies[i];

				ProjectToAxis(satAxis, triPoints, out float triMin, out float triMax);

				ProjectToAxis(satAxis, boxPoints, out float boxMin, out float boxMax);

				if (triMax < boxMin || boxMax < triMin)
				{
					return false;
				}
			}
			return true;
		}

		private static void ProjectToAxis(Vector2 axis, Vector2[] points, out float min, out float max)
		{
			min = 99999999.0f;
			max = -min;

			for (int i = 0; i < points.Length; i++)
			{
				float value = Vector2.Dot(axis, points[i]);
				min = Mathf.Min(min, value);
				max = Mathf.Max(max, value);
			}
		}

		public static bool OverlapCircle(Box2D box, Circle circle)
		{
			Vector2 centerBox = box.Center;
			Vector2 centerCircle = circle.Center;
			Vector2 v = centerCircle - centerBox;
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);

			Vector2 h = box.max - box.Center;

			Vector2 u = v - h;

			u.x = Mathf.Max(u.x, 0.0f);
			u.y = Mathf.Max(u.y, 0.0f);

			return u.sqrMagnitude <= circle.Radius * circle.Radius;

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
			if (!(boundsRight.max.x <= boundsLeft.min.x || boundsRight.min.x >= boundsLeft.max.x ||
				   boundsRight.max.y <= boundsLeft.min.y || boundsRight.min.y >= boundsLeft.max.y ||
				   boundsRight.max.z <= boundsLeft.min.z || boundsRight.min.z >= boundsLeft.max.z))
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

		public static bool ContainsPoint(Vector2 min, Vector2 max, Vector2 point)
		{
			return min.x <= point.x && point.x <= max.x &&
				   min.y <= point.y && point.y <= max.y;
		}

		public static Bounds GetTriangleAABB(Vector3[] triangles)
		{
			Vector3 min = new Vector3(9999999, 999999, 99999);
			Vector3 max = -min;
			for (int i = 0; i < triangles.Length; i++)
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
			foreach (Bounds bounds in boundsList)
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
			Bounds bounds = new Bounds(new Vector3(0, 0, 0), Vector3.one);
			return BoundsToMesh(bounds);
		}

		public static List<Triangle2D> PolygonToTriangle(Vector2[] points)
		{
			List<Vector3> vertices = new List<Vector3>();
			for (int i = 0; i < points.Length; i++)
			{
				vertices.Add(new Vector3(points[i].x, points[i].y, 0));
			}

			int[] indices = EarClippingHelper.EarClipping(vertices.ToArray());
			List<Triangle2D> result = new List<Triangle2D>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				result.Add(new Triangle2D(new Vector2[]
				{
					vertices[indices[i]],
					vertices[indices[i+1]],
					vertices[indices[i+2]]
				}));
			}
			return result;
		}

		public static bool Overlap(Interval slap0, Interval slap1)
		{
			return !(slap0.max <= slap1.min || slap0.min >= slap1.max);
		}

		public static Interval Intersect(Interval slap0, Interval slap1)
		{
			return new Interval(Mathf.Max(slap0.min, slap1.min), Mathf.Min(slap0.max, slap1.max));
		}

		public static bool IntersectWithRay(Box2D box, Ray2D ray, out Interval interval)
		{
			Interval slapX = new Interval(box.min.x, box.max.x);
			Interval slapY = new Interval(box.min.y, box.max.y);

			interval = new Interval(Mathf.Infinity, Mathf.Infinity);

			if (Mathf.Approximately(ray.direction.x, 0.0f))
			{
				if (!slapX.Contains(ray.origin.x))
				{
					return false;
				}

				interval = IntersectWithSplitRay(slapY, ray.origin.y, ray.direction.y);
			}
			else if (Mathf.Approximately(ray.direction.y, 0.0f))
			{
				if (!slapY.Contains(ray.origin.y))
				{
					return false;
				}
				interval = IntersectWithSplitRay(slapX, ray.origin.x, ray.direction.x);
			}
			else
			{
				Interval intersectX = IntersectWithSplitRay(slapX, ray.origin.x, ray.direction.x);
				Interval intersectY = IntersectWithSplitRay(slapY, ray.origin.y, ray.direction.y);
				if (!Overlap(intersectX, intersectY))
				{
					return false;
				}
				interval = Intersect(intersectX, intersectY);
			}

			if (interval.max < 0)
				return false;
			else if (interval.min < 0)
				interval.min = interval.max;
			return true;
		}

		public static Interval IntersectWithSplitRay(Interval slap, float origin, float dir)
		{
			float t0 = (slap.min - origin) / dir;
			float t1 = (slap.max - origin) / dir;
			return new Interval(Mathf.Min(t0, t1), Mathf.Max(t1, t0));
		}

		/// <summary>
		/// 根据盒子的中心和尺寸，计算8个顶点的位置
		/// 顶点布局与BoundsToMesh一致：
		/// 0: min(-x,-y,-z)  1: (-x,-y,+z)  2: (+x,-y,+z)  3: (+x,-y,-z)
		/// 4: (-x,+y,-z)     5: (-x,+y,+z)  6: (+x,+y,+z)  7: (+x,+y,-z)
		/// </summary>
		public static Vector3[] GetBoxCorners(Vector3 center, Vector3 size)
		{
			Vector3 extents = size * 0.5f;
			Vector3[] corners = new Vector3[8];
			corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
			corners[1] = center + new Vector3(-extents.x, -extents.y,  extents.z);
			corners[2] = center + new Vector3( extents.x, -extents.y,  extents.z);
			corners[3] = center + new Vector3( extents.x, -extents.y, -extents.z);
			corners[4] = center + new Vector3(-extents.x,  extents.y, -extents.z);
			corners[5] = center + new Vector3(-extents.x,  extents.y,  extents.z);
			corners[6] = center + new Vector3( extents.x,  extents.y,  extents.z);
			corners[7] = center + new Vector3( extents.x,  extents.y, -extents.z);
			return corners;
		}

		/// <summary>
		/// 将8个顶点表示的包围盒表面转换为12个三角形（6个面，每面2个三角形）
		/// 顶点布局需与GetBoxCorners一致
		/// </summary>
		public static List<Triangle> BoxCornersToTriangles(Vector3[] corners)
		{
			List<Triangle> triangles = new List<Triangle>(12);
			// 底面
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[1], corners[2] }));
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[2], corners[3] }));
			// 前面 (z-)
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[1], corners[5] }));
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[5], corners[4] }));
			// 左面 (x-)
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[4], corners[7] }));
			triangles.Add(new Triangle(new Vector3[] { corners[0], corners[7], corners[3] }));
			// 右面 (x+)
			triangles.Add(new Triangle(new Vector3[] { corners[1], corners[5], corners[6] }));
			triangles.Add(new Triangle(new Vector3[] { corners[1], corners[6], corners[2] }));
			// 后面 (z+)
			triangles.Add(new Triangle(new Vector3[] { corners[2], corners[6], corners[7] }));
			triangles.Add(new Triangle(new Vector3[] { corners[2], corners[7], corners[3] }));
			// 顶面
			triangles.Add(new Triangle(new Vector3[] { corners[4], corners[5], corners[6] }));
			triangles.Add(new Triangle(new Vector3[] { corners[4], corners[6], corners[7] }));
			return triangles;
		}
	}
}
