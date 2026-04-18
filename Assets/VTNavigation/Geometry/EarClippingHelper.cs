using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Common;

namespace VTNavigation.Geometry
{
	public static class EarClippingHelper
	{
		public static Vector3[] MergeInternalGeometry(Vector3[] OuterPolygon, Vector3[] InnerPolygon)
		{
			int RighteastIndex = 0;
			for (int i = 1; i < InnerPolygon.Length; i++)
			{
				if (InnerPolygon[i].x > InnerPolygon[RighteastIndex].x)
				{
					RighteastIndex = i;
				}
			}
			Vector3 Point = InnerPolygon[RighteastIndex];
			Ray InnerRay = new Ray(Point, Vector3.right);
			float NearestDis = 9999999.0f;
			int NearestIntersectIndex0 = 0;
			int NearestIntersectIndex1 = 0;
			for (int i = 0; i < OuterPolygon.Length - 1; i++)
			{
				float t = IntersectSegment(InnerRay, OuterPolygon[i], OuterPolygon[i + 1]);
				if (t > 0 && t < NearestDis)
				{
					NearestDis = t;
					NearestIntersectIndex0 = i;
					NearestIntersectIndex1 = i + 1;
				}
			}

			Vector3 IntersectPoint = OuterPolygon[NearestIntersectIndex0] + (OuterPolygon[NearestIntersectIndex1] - OuterPolygon[NearestIntersectIndex0]) * NearestDis;
			Vector3 VisibleCorner = OuterPolygon[NearestIntersectIndex0];
			Vector3[] Triangle = new Vector3[] { Point, VisibleCorner, IntersectPoint };


			float MaxCos = Vector3.Dot(Vector3.right, (VisibleCorner - Point).normalized);
			int VisiblePointIndex = NearestIntersectIndex0;
			for (int i = 0; i < OuterPolygon.Length; i++)
			{
				if (i != NearestIntersectIndex0 && IsPointInTriangle(OuterPolygon[i], Triangle))
				{
					float Cos = Vector3.Dot(Vector3.right, (OuterPolygon[i] - Point).normalized);
					if (Cos > MaxCos)
					{
						MaxCos = Cos;
						VisiblePointIndex = i;
					}
				}
			}

			List<Vector3> ResultPolygon = new List<Vector3>();
			for (int i = 0; i <= VisiblePointIndex; i++)
			{
				ResultPolygon.Add(OuterPolygon[i]);
			}

			ResultPolygon.Add(InnerPolygon[RighteastIndex]);
			for (int i = (RighteastIndex + 1) % InnerPolygon.Length; i != RighteastIndex; i = (i + 1) % InnerPolygon.Length)
			{
				ResultPolygon.Add(InnerPolygon[i]);
			}
			ResultPolygon.Add(InnerPolygon[RighteastIndex]);
			ResultPolygon.Add(OuterPolygon[VisiblePointIndex]);
			for (int i = VisiblePointIndex + 1; i < OuterPolygon.Length; i++)
			{
				ResultPolygon.Add(OuterPolygon[i]);
			}

			return ResultPolygon.ToArray();
		}

		private static float IntersectSegment(Ray inRay, Vector3 inPoint0, Vector3 inPoint1)
		{
			Ray SegmentRay = new Ray(inPoint0, (inPoint1 - inPoint0).normalized);
			Vector3 IntersectPoint = IntersectRay(inRay, SegmentRay);
			if (IsPointInSegment(IntersectPoint, inPoint0, inPoint1, out float t))
			{
				return t;
			}
			return -1;
		}

		private static bool IsPointInSegment(Vector3 inPoint, Vector3 inSegPoint0, Vector3 inSegPoint1, out float t)
		{
			t = Vector3.Dot(inPoint - inSegPoint0, inSegPoint1 - inSegPoint0) / (Vector3.Magnitude(inSegPoint1 - inSegPoint0));
			return t >= 0 && t <= 1;
		}

		private static bool IsPointInTriangle(Vector3 inPoint, Vector3[] inTri)
		{
			Vector3 v0 = inTri[1] - inTri[0];
			Vector3 v1 = inTri[2] - inTri[0];
			Vector3 v2 = inPoint - inTri[0];
			float v1v1 = Vector3.Dot(v1, v1);
			float v2v0 = Vector3.Dot(v2, v0);
			float v1v0 = Vector3.Dot(v1, v0);
			float v2v1 = Vector3.Dot(v2, v1);
			float v0v0 = Vector3.Dot(v0, v0);
			float u = (v1v1 * v2v0 - v1v0 * v2v1) / (v0v0 * v1v1 - v1v0 * v1v0);
			float v = (v0v0 * v2v1 - v1v0 * v2v0) / (v0v0 * v1v1 - v1v0 * v1v0);
			return !(u < 0 || v < 0 || u > 1 || v > 1);
		}

		private static Vector3 IntersectRay(Ray inRay0, Ray inRay1)
		{
			Vector3 dxd = Vector3.Cross(inRay1.direction, inRay0.direction);
			float t = Vector3.Dot(Vector3.Cross(inRay0.origin - inRay1.origin, inRay0.direction), dxd) / dxd.sqrMagnitude;
			return inRay1.GetPoint(t);
		}

		public static int[] EarClipping(Vector3[] inPolygon)
		{
			DList PolygonVerticesList = new DList();
			for (int i = 0; i < inPolygon.Length; i++)
			{
				PolygonVerticesList.AppendNode(i);
			}

			List<int> Result = new List<int>();
			ListNode CurrentEar = PolygonVerticesList.Head;
			int FirstIndex = CurrentEar.Index;
			int HitCount = 0;
			while (PolygonVerticesList.Count > 3)
			{
				int PrevIndex = CurrentEar.Prev.Index;
				int CurrIndex = CurrentEar.Index;
				int NextIndex = CurrentEar.Next.Index;

				if (CurrIndex == FirstIndex)
				{
					HitCount++;
					if (HitCount > 1)
					{
						Debug.LogError("Dead Loop.");
						break;
					}
				}

				bool Flag = true;
				Vector3[] Tri = new Vector3[3] { inPolygon[PrevIndex], inPolygon[CurrIndex], inPolygon[NextIndex] };

				PolygonVerticesList.Traver((int index) =>
				{
					if (index == PrevIndex || index == CurrIndex || index == NextIndex)
						return true;
					Vector3 Point = inPolygon[index];
					if (Vector3.Distance(Point, Tri[0]) <= 0.01f || Vector3.Distance(Point, Tri[1]) <= 0.01f || Vector3.Distance(Point, Tri[2]) <= 0.01f)
						return true;
					if (IsPointInTriangle(Point, Tri))
					{
						Flag = false;
						return false;
					}
					return true;
				});

				if (Flag && Vector3.Cross(Tri[0] - Tri[1], Tri[2] - Tri[1]).z < 0)
				{
					ListNode PrevNode = CurrentEar.Prev;
					PolygonVerticesList.RemoveNode(CurrentEar);
					Result.Add(PrevIndex);
					Result.Add(CurrIndex);
					Result.Add(NextIndex);
					CurrentEar = PrevNode;
					FirstIndex = CurrentEar.Index;
					HitCount = 0;
				}
				else
				{
					CurrentEar = CurrentEar.Next;
				}
			}
			if (HitCount > 1)
			{
				return null;
			}
			PolygonVerticesList.Traver((int index) =>
			{
				Result.Add(index);
				return true;
			});
			return Result.ToArray();
		}
	}
}
