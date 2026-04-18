using UnityEngine;

namespace VTNavigation.Geometry
{
	public struct Interval
	{
		public float min;
		public float max;

		public Interval(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public bool Contains(float value)
		{
			return value >= min && value <= max;
		}

		public bool IsValid
		{
			get
			{
				return min != Mathf.Infinity && max != Mathf.Infinity;
			}
		}
	}
}
