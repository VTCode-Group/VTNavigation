
using UnityEngine;

namespace VTNavigation.Geometry
{
	public struct Circle
	{
		private Vector2 m_Center;
		private float m_Radius;

		public Vector2 Center
		{
			get { return m_Center; }
		}

		public float Radius
		{
			get
			{
				return m_Radius;
			}
		}
		public Circle(Vector2 center, float radius)
		{
			m_Center = center;
			m_Radius = radius;
		}
	}
}
