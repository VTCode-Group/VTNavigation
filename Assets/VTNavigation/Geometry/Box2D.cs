using UnityEngine;

namespace VTNavigation.Geometry
{
	public struct Box2D
	{
		public Vector2 min;
		public Vector2 max;

		public Box2D(Vector2 center, Vector2 size)
		{
			min = center - size * 0.5f;
			max = center + size * 0.5f;
		}

		public Vector2 Min
		{
			get { return min; }
		}

		public Vector2 Center
		{
			get
			{
				return (min + max) * 0.5f;
			}
		}

		public Vector2 Size
		{
			get
			{
				return max - min;
			}
		}

		public bool Contains(Vector2 point)
		{
			return !(point.x < min.x || point.x > max.x || point.y < min.y || point.y > max.y);
		}
	}
}
