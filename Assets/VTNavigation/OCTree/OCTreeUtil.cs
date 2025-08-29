using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTNavigation.Tree
{
	public static class OCTreeUtil
	{
		public static float ToTreeSpace(OCTree tree, float value)
		{
			return value / tree.MinSizeScale;
		}

		public static float ToWorldSpace(OCTree tree, float value)
		{
			return value * tree.MinSizeScale;
		}

		public static Vector3 ToTreeSpace(OCTree tree, Vector3 value)
		{
			return new Vector3(ToTreeSpace(tree, value.x), ToTreeSpace(tree, value.y), ToTreeSpace(tree, value.z));
		}

		public static Vector3 ToWorldSpace(OCTree tree, Vector3 value)
		{
			return new Vector3(ToWorldSpace(tree, value.x), ToWorldSpace(tree, value.y), ToWorldSpace(tree, value.z));
		}

		public static void ToTreeSpace(OCTree tree, List<Vector3> points)
		{
			for(int i = 0; i < points.Count; i++)
			{
				points[i] = ToTreeSpace(tree, points[i]);
			}
		}

		public static Bounds ToTreeSpace(OCTree tree, Bounds bounds)
		{
			Vector3 minInTreeSpace = ToTreeSpace(tree, bounds.min);
			Vector3 sizeInTreeSpace = ToTreeSpace(tree, bounds.size);
			Vector3 center = minInTreeSpace + sizeInTreeSpace*0.5f;
			return new Bounds(center, sizeInTreeSpace);
		}



		public static Bounds ToWorldSpace(OCTree tree, Bounds bounds)
		{
			Vector3 minInWorldSpace = ToWorldSpace(tree, bounds.min);
			Vector3 sizeInWorldSpace = ToWorldSpace(tree, bounds.size);
			Vector3 center = minInWorldSpace + sizeInWorldSpace * 0.5f;
			return new Bounds(center, sizeInWorldSpace);
		}

		public static void ToWorldSpace(OCTree tree, List<Bounds> boundsList)
		{
			for(int i = 0; i < boundsList.Count; ++i)
			{
				boundsList[i] = ToWorldSpace(tree, boundsList[i]);
			}
		}

		public static Bounds MakeBounds(Vector3 min, float size)
		{
			Vector3 sizeVec = Vector3.one * size;
			Vector3 center = min + sizeVec * 0.5f;
			return new Bounds(center, sizeVec);
		}

		public static Bounds MakeBoundsFromExtents(Vector3 min, float extents)
		{
			Vector3 centerOffset = Vector3.one * extents;
			Vector3 center = min + centerOffset;
			Vector3 size = Vector3.one * extents * 2;
			return new Bounds(center, size);
		}

		public static bool IsBlockStatus(byte status, int offset)
		{
			return (status & (1 << offset)) > 0;
		}

		public static void SetBlockStatus(ref byte status, int offset)
		{
			status |= (byte)(1 << offset);
		}
	}
}
